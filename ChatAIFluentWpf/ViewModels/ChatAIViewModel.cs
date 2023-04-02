using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using ChatAIFluentWpf.Clr;
using ChatAIFluentWpf.Common;
using ChatAIWpf.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.CoreAudioApi;
using NLog;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using Wpf.Ui.Common.Interfaces;

namespace ChatAIFluentWpf.ViewModels
{
    public partial class ChatAIViewModel : ObservableObject, INavigationAware
    {
        #region プロパティ
        /// <summary>
        /// コンボボックスに表示する入力デバイス一覧
        /// </summary>
        [ObservableProperty]
        private MMDeviceCollection? _audioDevices;

        /// <summary>
        /// 選択された入力デバイス
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CaptureAudioCommand))]
        private MMDevice? _selectedAudioDevice;

        /// <summary>
        /// 録音中フラグ
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CaptureButtonContent))]
        [NotifyCanExecuteChangedFor(nameof(CaptureAudioCommand))]
        private bool _isRecording = false;

        /// <summary>
        /// 初期化済フラグ
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLoading))]
        [NotifyCanExecuteChangedFor(nameof(CaptureAudioCommand))]
        private bool _isLoaded = false;

        /// <summary>
        /// 初期化中フラグ
        /// </summary>
        public bool IsLoading => !IsLoaded;

        /// <summary>
        /// ステータスバーに表示するメッセージ
        /// </summary>
        [ObservableProperty]
        private string _statusBarMessage = string.Empty;

        /// <summary>
        /// 会話した内容
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> _conversation = new();
        #endregion

        #region メンバ変数
        /// <summary>
        /// ボタンに表示する文言
        /// </summary>
        [ObservableProperty]
        private string _captureButtonContent = "話す";
        /// <summary>
        /// VoiceVoxをCLRで使うためのラッパークラス
        /// </summary>
        private VoiceVoxWrapper _wrapper;
        /// <summary>
        /// ロガー
        /// </summary>
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// オーディオサービス
        /// </summary>
        private readonly IAudioService _audioService;
        /// <summary>
        /// OpenAIサービスインスタンス
        /// </summary>
        private OpenAIService? _openAIService;
        /// <summary>
        /// OpenAIに渡す会話履歴
        /// </summary>
        private List<ChatMessage> _messages = new();
        /// <summary>
        /// Azure Key Vault シークレットインスタンス
        /// </summary>
        private SecretClient _secretClient;
        /// <summary>
        /// Azure Cognitive Services スピーチインスタンス
        /// </summary>
        private SpeechConfig _speechConfig;
        /// <summary>
        /// Azure Key VaultのキーコンテナーURI
        /// </summary>
        private readonly string _azureKeyVaultUri = Properties.Settings.Default.AzureKeyVaultUri;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ChatAIViewModel(IAudioService audioService)
        {
            _logger.Info("start");
            _audioService = audioService;

            AudioDevices = _audioService.GetActiveCapture();
            SelectedAudioDevice = AudioDevices.FirstOrDefault();
            _logger.Info("end");
        }
        #endregion

        #region INavigationAware
        /// <summary>
        /// 
        /// </summary>
        public void OnNavigatedFrom()
        {
            // ...
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnNavigatedTo()
        {
            // ...
        }
        #endregion

        #region コマンド
        /// <summary>
        /// Window読み込み完了時のコマンド
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task Loaded()
        {
            _logger.Info("start");
            await Task.Run(() =>
            {
                StatusBarMessage = "OpenAIの初期化中...";

                // Get secrets.
                _secretClient = new SecretClient(new Uri(_azureKeyVaultUri), new DefaultAzureCredential());
                var azureSpeechApiKey = _secretClient.GetSecretAsync("AzureSpeechAPIKey").Result;
                var openAIApiKey = _secretClient.GetSecretAsync("OpenAIApiKey").Result;

                // Create instances.
                _speechConfig = SpeechConfig.FromSubscription(azureSpeechApiKey.Value.Value, "eastus");
                _speechConfig.SpeechRecognitionLanguage = "ja-JP";

                _openAIService = new OpenAIService(new OpenAiOptions()
                {
                    ApiKey = openAIApiKey.Value.Value,
                });

                _messages.Add(ChatMessage.FromSystem("あなたは日本語で会話ができるチャットボットです。"));

                StatusBarMessage = "VoiceVoxの初期化中...";

                // VoiceVoxの初期化
                _wrapper = new VoiceVoxWrapper("open_jtalk_dic_utf_8-1.11");
                var initRet = ConvertFromInt(_wrapper.Initialize());
                if (initRet != VoiceVoxResultCode.VOICEVOX_RESULT_OK)
                {
                    throw new Exception(initRet.ToString());
                }

                StatusBarMessage = "準備完了";
            });
            IsLoaded = true;
            _logger.Info("end");
        }

        /// <summary>
        /// 録音開始/停止
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanExecuteCaptureAudio))]
        private async Task CaptureAudio()
        {
            _logger.Info("start");
            var audioConfig = AudioConfig.FromMicrophoneInput(SelectedAudioDevice?.ID);
            using (var recognizer = new SpeechRecognizer(_speechConfig, audioConfig))
            {
                CaptureButtonContent = "話してください…";
                IsRecording = true;
                var result = await recognizer.RecognizeOnceAsync();
                IsRecording = false;

                // Checks result.
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    Conversation.Add($"あなた: {result.Text}");
                    _messages.Add(ChatMessage.FromUser(result.Text));

                    CaptureButtonContent = "解析中...";
                    var completionResult = await _openAIService!.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
                    {
                        Messages = _messages,
                        Model = "gpt-3.5-turbo",
                        MaxTokens = 150,
                    });

                    if (completionResult.Successful)
                    {
                        Conversation.Add($"ChatGPT: {completionResult.Choices.First().Message.Content}");
                        _messages.Add(ChatMessage.FromAssistant(completionResult.Choices.First().Message.Content));

                        // VoiceVoxで再生
                        CaptureButtonContent = "音声生成中...";
                        var ret = ConvertFromInt(_wrapper.GenerateVoice(completionResult.Choices.First().Message.Content));
                        if (ret != VoiceVoxResultCode.VOICEVOX_RESULT_OK)
                        {
                            Conversation.Add($"[SYSTEM] ERROR: VoiceVox handled error.");
                            Conversation.Add($"[SYSTEM] ResultCode={ret}");
                        }

                        var player = new SoundPlayer(@"./speech.wav");
                        player.Play();
                    }
                    else if (completionResult.Error != null)
                    {
                        Conversation.Add($"[SYSTEM] ERROR: Chat GPT handled error.");
                        Conversation.Add($"[SYSTEM] Code={completionResult.Error.Code}");
                        Conversation.Add($"[SYSTEM] Message={completionResult.Error.Message}");
                    }
                    else
                    {
                        Conversation.Add($"[SYSTEM] ERROR: Unknown error occurred at Chat GPT.");
                    }
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Conversation.Add($"[SYSTEM] NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Conversation.Add($"[SYSTEM] CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Conversation.Add($"[SYSTEM] CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Conversation.Add($"[SYSTEM] CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Conversation.Add($"[SYSTEM] CANCELED: Did you update the subscription info?");
                    }
                }
            }
            CaptureButtonContent = "話す";
            _logger.Info("end");
        }

        #region メンバメソッド
        /// <summary>
        /// int -> VoiceVoxResultCode
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private VoiceVoxResultCode ConvertFromInt(int number)
        {
            return (VoiceVoxResultCode)Enum.ToObject(typeof(VoiceVoxResultCode), number);
        }
        #endregion

        /// <summary>
        /// 録音開始/停止ボタンが押下可能か判定
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteCaptureAudio()
        {
            return SelectedAudioDevice != null && !IsRecording && IsLoaded;
        }
        #endregion
    }
}
