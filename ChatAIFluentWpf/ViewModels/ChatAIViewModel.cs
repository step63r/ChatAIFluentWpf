using Azure.Security.KeyVault.Secrets;
using ChatAIFluentWpf.Common;
using ChatAIFluentWpf.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;
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
        private bool _isInitialized = false;

        /// <summary>
        /// 読込済フラグ
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLoading))]
        [NotifyCanExecuteChangedFor(nameof(CaptureAudioCommand))]
        private bool _isLoaded = false;

        /// <summary>
        /// 読込中フラグ
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

        /// <summary>
        /// VoiceVoxのバージョン
        /// </summary>
        [ObservableProperty]
        private string? _voiceVoxVersion = string.Empty;

        /// <summary>
        /// VoiceVoxのGPUモード
        /// </summary>
        [ObservableProperty]
        private string? _voiceVoxGpuMode = string.Empty;

        /// <summary>
        /// 読み込んだモデルのメタ情報
        /// </summary>
        [ObservableProperty]
        private string? _metaInfo = string.Empty;
        #endregion

        #region メンバ変数
        /// <summary>
        /// ボタンに表示する文言
        /// </summary>
        [ObservableProperty]
        private string _captureButtonContent = "話す";
        /// <summary>
        /// ロガー
        /// </summary>
        private readonly ILogger<ChatAIViewModel> _logger;
        /// <summary>
        /// オーディオサービス
        /// </summary>
        private readonly IAudioService _audioService;
        /// <summary>
        /// VoiceVoxサービス
        /// </summary>
        private readonly IVoiceVoxService _voiceVoxService;
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
        /// VoiceVoxの話者ID
        /// </summary>
        private int _voiceVoxSpeakerId = Properties.Settings.Default.VoiceVoxSpeakerId;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="secretClient"></param>
        /// <param name="audioService"></param>
        /// <param name="voiceVoxService"></param>
        public ChatAIViewModel(ILogger<ChatAIViewModel> logger, SecretClient secretClient, IAudioService audioService, IVoiceVoxService voiceVoxService)
        {
            _logger = logger;
            _secretClient = secretClient;
            _audioService = audioService;
            _voiceVoxService = voiceVoxService;

            AudioDevices = _audioService.GetActiveCapture();
            SelectedAudioDevice = AudioDevices.FirstOrDefault();
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
            _logger.LogInformation("start");

            IsLoaded = false;
            await Task.Run(() =>
            {
                #region Azure Cognitive Servicesの初期化
                if (!_isInitialized)
                {
                    StatusBarMessage = "Azure Cognitive Services の初期化中...";
                    var azureSpeechApiKey = _secretClient.GetSecretAsync("AzureSpeechAPIKey").Result;

                    // Create instances.
                    _speechConfig = SpeechConfig.FromSubscription(azureSpeechApiKey.Value.Value, "eastus");
                    _speechConfig.SpeechRecognitionLanguage = "ja-JP";
                }
                #endregion

                #region OpenAIの初期化
                if (!_isInitialized)
                {
                    StatusBarMessage = "OpenAI の初期化中...";
                    var openAIApiKey = _secretClient.GetSecretAsync("OpenAIApiKey").Result;

                    _openAIService = new OpenAIService(new OpenAiOptions()
                    {
                        ApiKey = openAIApiKey.Value.Value,
                    });

                    _messages.Add(ChatMessage.FromSystem("あなたは日本語で会話ができるチャットボットです。"));
                }
                #endregion

                #region VOICEVOXの初期化
                StatusBarMessage = "VoiceVoxの初期化中...";
                if (!_isInitialized)
                {
                    // VoiceVoxの初期化
                    var initRet = _voiceVoxService.Initialize();
                    if (initRet != VoiceVoxResultCode.VOICEVOX_RESULT_OK)
                    {
                        throw new Exception(initRet.ToString());
                    }

                    // バージョンとGPUモード取得
                    VoiceVoxVersion = _voiceVoxService.Version;
                    VoiceVoxGpuMode = _voiceVoxService.IsGpuMode ? "ON" : "OFF";
                }

                int voiceVoxSpeakerId = Properties.Settings.Default.VoiceVoxSpeakerId;
                if (!_isInitialized || _voiceVoxSpeakerId != voiceVoxSpeakerId)
                {
                    LoadModelAsync(voiceVoxSpeakerId);
                    _voiceVoxService.SpeakerId = voiceVoxSpeakerId;
                }
                #endregion

                StatusBarMessage = "準備完了";
            });

            IsLoaded = true;

            // 初期化済フラグを立てる
            if (!_isInitialized)
            {
                _isInitialized = true;
            }

            _logger.LogInformation("end");
        }

        /// <summary>
        /// 録音開始/停止
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanExecuteCaptureAudio))]
        private async Task CaptureAudio()
        {
            _logger.LogInformation("start");
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
                        var ret = _voiceVoxService.GenerateVoice(completionResult.Choices.First().Message.Content);
                        if (ret != VoiceVoxResultCode.VOICEVOX_RESULT_OK)
                        {
                            _logger.LogError($"[SYSTEM] ERROR: VoiceVox handled error.");
                            _logger.LogError($"[SYSTEM] ResultCode={ret}");

                            Conversation.Add($"[SYSTEM] ERROR: VoiceVox handled error.");
                            Conversation.Add($"[SYSTEM] ResultCode={ret}");
                        }
                        else
                        {
                            var player = new SoundPlayer(@"./speech.wav");
                            player.Play();
                        }
                    }
                    else if (completionResult.Error != null)
                    {
                        _logger.LogError($"[SYSTEM] ERROR: Chat GPT handled error.");
                        _logger.LogError($"[SYSTEM] Code={completionResult.Error.Code}");
                        _logger.LogError($"[SYSTEM] Message={completionResult.Error.Message}");

                        Conversation.Add($"[SYSTEM] ERROR: Chat GPT handled error.");
                        Conversation.Add($"[SYSTEM] Code={completionResult.Error.Code}");
                        Conversation.Add($"[SYSTEM] Message={completionResult.Error.Message}");
                    }
                    else
                    {
                        _logger.LogError($"[SYSTEM] ERROR: Unknown error occurred at Chat GPT.");
                        Conversation.Add($"[SYSTEM] ERROR: Unknown error occurred at Chat GPT.");
                    }
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    _logger.LogWarning($"[SYSTEM] NOMATCH: Speech could not be recognized.");
                    Conversation.Add($"[SYSTEM] NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    _logger.LogWarning($"[SYSTEM] CANCELED: Reason={cancellation.Reason}");
                    Conversation.Add($"[SYSTEM] CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        _logger.LogError($"[SYSTEM] CANCELED: ErrorCode={cancellation.ErrorCode}");
                        _logger.LogError($"[SYSTEM] CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        _logger.LogError($"[SYSTEM] CANCELED: Did you update the subscription info?");

                        Conversation.Add($"[SYSTEM] CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Conversation.Add($"[SYSTEM] CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Conversation.Add($"[SYSTEM] CANCELED: Did you update the subscription info?");
                    }
                }
            }
            CaptureButtonContent = "話す";
            _logger.LogInformation("end");
        }

        /// <summary>
        /// 録音開始/停止ボタンが押下可能か判定
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteCaptureAudio()
        {
            return SelectedAudioDevice != null && !IsRecording && IsLoaded;
        }
        #endregion

        #region メソッド
        /// <summary>
        /// VoiceVoxのモデルを読み込む
        /// </summary>
        /// <returns></returns>
        private void LoadModelAsync(int voiceVoxSpeakerId)
        {
            _logger.LogInformation("start");
            _voiceVoxSpeakerId = voiceVoxSpeakerId;
            var (meta, style) = _voiceVoxService.GetMetadataFromSpeakerId(_voiceVoxSpeakerId);
            MetaInfo = $"{meta.Name} ({style.Name})";

            if (!_voiceVoxService.IsModelLoaded(_voiceVoxSpeakerId))
            {
                StatusBarMessage = "VoiceVoxのモデル読込中...";

                // モデルの読み込み
                var loadModelRet = _voiceVoxService.LoadModel(_voiceVoxSpeakerId);
                if (loadModelRet != VoiceVoxResultCode.VOICEVOX_RESULT_OK)
                {
                    throw new Exception(loadModelRet.ToString());
                }
            }
            _logger.LogInformation("end");
        }
        #endregion
    }
}
