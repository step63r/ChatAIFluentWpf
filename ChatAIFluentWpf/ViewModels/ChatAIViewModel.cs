using Azure.Security.KeyVault.Secrets;
using ChatAIFluentWpf.Common;
using ChatAIFluentWpf.Models;
using ChatAIFluentWpf.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
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
using Wpf.Ui.Mvvm.Contracts;

namespace ChatAIFluentWpf.ViewModels
{
    /// <summary>
    /// ChatAIPage.xamlのViewModelクラス
    /// </summary>
    public partial class ChatAIViewModel : ObservableObject, INavigationAware
    {
        #region プロパティ
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
        private ObservableCollection<IConversation> _conversation = new();

        /// <summary>
        /// VoiceVoxのバージョン
        /// </summary>
        [ObservableProperty]
        private string? _voiceVoxVersion = string.Empty;

        /// <summary>
        /// VoiceVoxのGPUモード
        /// </summary>
        [ObservableProperty]
        private bool _voiceVoxGpuMode = false;

        /// <summary>
        /// 読み込んだモデルのメタ情報
        /// </summary>
        [ObservableProperty]
        private string? _metaInfo = "メタ情報";
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
        /// Snackbarサービス
        /// </summary>
        private readonly ISnackbarService _snackbarService;
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
        /// <summary>
        /// 使用するオーディオインターフェイスID
        /// </summary>
        private string _audioDeviceId = Properties.Settings.Default.AudioDeviceId ?? string.Empty;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="secretClient"></param>
        /// <param name="audioService"></param>
        /// <param name="voiceVoxService"></param>
        /// <param name="snackbarService"></param>
        public ChatAIViewModel(ILogger<ChatAIViewModel> logger, SecretClient secretClient, IAudioService audioService, IVoiceVoxService voiceVoxService, ISnackbarService snackbarService)
        {
            _logger = logger;
            _secretClient = secretClient;
            _audioService = audioService;
            _voiceVoxService = voiceVoxService;
            _snackbarService = snackbarService;
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
            if (!_isInitialized)
                InitializeViewModel();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        private void InitializeViewModel()
        {
            // 初回ロード時、デバイスが未選択でなおかつ有効なデバイスがあればそれを選択する
            var devices = _audioService.GetActiveCapture();
            if (string.IsNullOrEmpty(_audioDeviceId) || devices.Where(item => _audioDeviceId.Equals(item.ID)).FirstOrDefault() == null)
            {
                if (devices?.Count > 0)
                {
                    var defaultDevice = devices.First();
                    _audioDeviceId = defaultDevice.ID;
                    _logger.LogWarning($"Selected first-detected device (ID: {_audioDeviceId})");
                    _ = _snackbarService.ShowAsync(
                        $"入力デバイスを自動選択しました ({defaultDevice.FriendlyName})",
                        "必要に応じて設定ページから入力デバイスを変更してください。",
                        Wpf.Ui.Common.SymbolRegular.Warning20,
                        Wpf.Ui.Common.ControlAppearance.Caution);
                }
                else
                {
                    _audioDeviceId = string.Empty;
                    _logger.LogWarning("No audio devices found!");
                    _ = _snackbarService.ShowAsync(
                        "入力デバイスが見つかりませんでした",
                        "有効な入力デバイスを接続してアプリを再起動してください。",
                        Wpf.Ui.Common.SymbolRegular.Warning20,
                        Wpf.Ui.Common.ControlAppearance.Caution);
                }
            }
        }

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
                StatusBarMessage = "Azure Cognitive Services の初期化中...";
                var azureSpeechApiKey = _secretClient.GetSecretAsync("AzureSpeechAPIKey").Result;

                // Create instances.
                _speechConfig = SpeechConfig.FromSubscription(azureSpeechApiKey.Value.Value, "eastus");
                // セグメント化の無音タイムアウトの設定
                _speechConfig.SetProperty(
                    PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs,
                    Properties.Settings.Default.SpeechServiceConnection_InitialSilenceTimeoutMs.ToString());
                // 初期無音タイムアウトの設定
                _speechConfig.SetProperty(
                    PropertyId.Speech_SegmentationSilenceTimeoutMs,
                    Properties.Settings.Default.Speech_SegmentationSilenceTimeoutMs.ToString());
                _speechConfig.SpeechRecognitionLanguage = "ja-JP";
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

                    // 初期システムプロンプトの作成
                    var initPrompts = Properties.Settings.Default.InitialSystemPrompt.Split("\r\n");
                    foreach (string prompt in initPrompts)
                    {
                        if (!string.IsNullOrEmpty(prompt))
                        {
                            _messages.Add(ChatMessage.FromSystem(prompt));
                        }
                    }
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
                    VoiceVoxGpuMode = _voiceVoxService.IsGpuMode;
                }

                int voiceVoxSpeakerId = Properties.Settings.Default.VoiceVoxSpeakerId;
                if (!_isInitialized || _voiceVoxSpeakerId != voiceVoxSpeakerId)
                {
                    try
                    {
                        LoadModelAsync(voiceVoxSpeakerId);
                        _voiceVoxService.SpeakerId = voiceVoxSpeakerId;
                    }
                    catch (Exception ex)
                    {
                        // TODO: ここに書いてはいけないかもしれない
                        _ = _snackbarService.ShowAsync(
                            "合成音声辞書ファイル読込時にエラーが発生しました",
                            ex.ToString(),
                            Wpf.Ui.Common.SymbolRegular.ErrorCircle20,
                            Wpf.Ui.Common.ControlAppearance.Danger);
                    }
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
            var audioConfig = AudioConfig.FromMicrophoneInput(_audioDeviceId);
            using (var recognizer = new SpeechRecognizer(_speechConfig, audioConfig))
            {
                CaptureButtonContent = "話してください…";
                IsRecording = true;
                var result = await recognizer.RecognizeOnceAsync();
                IsRecording = false;

                // Checks result.
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    Conversation.Add(new UserConversation("あなた", result.Text, DateTime.Now));
                    _messages.Add(ChatMessage.FromUser(result.Text));

                    CaptureButtonContent = "解析中...";
                    var completionResult = await _openAIService!.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
                    {
                        Messages = _messages,
                        Model = "gpt-3.5-turbo",
                        MaxTokens = Properties.Settings.Default.MaxTokens,
                        Temperature = (float)Properties.Settings.Default.Temperature,
                    });

                    if (completionResult.Successful)
                    {
                        Conversation.Add(new BotConversation("ChatGPT", completionResult.Choices.First().Message.Content, DateTime.Now));
                        _messages.Add(ChatMessage.FromAssistant(completionResult.Choices.First().Message.Content));

                        // VoiceVoxで再生
                        CaptureButtonContent = "音声生成中...";
                        var ret = _voiceVoxService.GenerateVoice(completionResult.Choices.First().Message.Content);
                        if (ret != VoiceVoxResultCode.VOICEVOX_RESULT_OK)
                        {
                            _logger.LogError($"[SYSTEM] ERROR: VoiceVox handled error.");
                            _logger.LogError($"[SYSTEM] ResultCode={ret}");

                            _ = _snackbarService.ShowAsync(
                                "VOICEVOXエンジンでエラーが発生しました",
                                $"ResultCode: {ret}",
                                Wpf.Ui.Common.SymbolRegular.ErrorCircle20,
                                Wpf.Ui.Common.ControlAppearance.Danger);
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

                        _ = _snackbarService.ShowAsync(
                            "ChatGPTでエラーが発生しました",
                            $"{completionResult.Error.Message} (Code: {completionResult.Error.Code})",
                            Wpf.Ui.Common.SymbolRegular.ErrorCircle20,
                            Wpf.Ui.Common.ControlAppearance.Danger);
                    }
                    else
                    {
                        _logger.LogError($"[SYSTEM] ERROR: Unknown error occurred at Chat GPT.");

                        _ = _snackbarService.ShowAsync(
                            "ChatGPTでエラーが発生しました",
                            "不明なエラー",
                            Wpf.Ui.Common.SymbolRegular.ErrorCircle20,
                            Wpf.Ui.Common.ControlAppearance.Danger);
                    }
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    _logger.LogWarning($"[SYSTEM] NOMATCH: Speech could not be recognized.");

                    _ = _snackbarService.ShowAsync(
                        "Azure Cognitive Servicesからの通知",
                        "音声入力が認識されませんでした",
                        Wpf.Ui.Common.SymbolRegular.Warning20,
                        Wpf.Ui.Common.ControlAppearance.Caution);
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    _logger.LogWarning($"[SYSTEM] CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        _logger.LogError($"[SYSTEM] CANCELED: ErrorCode={cancellation.ErrorCode}");
                        _logger.LogError($"[SYSTEM] CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        _logger.LogError($"[SYSTEM] CANCELED: Did you update the subscription info?");

                        _ = _snackbarService.ShowAsync(
                            "Azure Cognitive Servicesでエラーが発生しました",
                            $"{cancellation.ErrorDetails} (ErrorCode: {cancellation.ErrorCode})",
                            Wpf.Ui.Common.SymbolRegular.ErrorCircle20,
                            Wpf.Ui.Common.ControlAppearance.Danger);
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
            return !string.IsNullOrEmpty(_audioDeviceId) && !IsRecording && IsLoaded;
        }
        #endregion

        #region メソッド
        /// <summary>
        /// VoiceVoxのモデルを読み込む
        /// </summary>
        /// <param name="voiceVoxSpeakerId"></param>
        /// <exception cref="Exception"></exception>
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
