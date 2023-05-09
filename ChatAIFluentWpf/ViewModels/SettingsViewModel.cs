using Azure.Security.KeyVault.Secrets;
using ChatAIFluentWpf.Common;
using ChatAIFluentWpf.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using static ChatAIFluentWpf.Common.VoiceVoxMetaData;

namespace ChatAIFluentWpf.ViewModels
{
    /// <summary>
    /// SettingsPage.xamlのViewModelクラス
    /// </summary>
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        /// <summary>
        /// 
        /// </summary>
        private bool _isInitialized = false;

        #region プロパティ
        /// <summary>
        /// 
        /// </summary>
        [ObservableProperty]
        private string _appVersion = String.Empty;

        /// <summary>
        /// 
        /// </summary>
        [ObservableProperty]
        private Wpf.Ui.Appearance.ThemeType _currentTheme = Wpf.Ui.Appearance.ThemeType.Unknown;

        /// <summary>
        /// キャラクター一覧
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<VoiceVoxMetaData> _metadatas = new();

        /// <summary>
        /// 音声タイプ一覧
        /// </summary>
        public ObservableCollection<VoiceVoxMetaDataStyles> Styles => SelectedMetadata != null ? new(SelectedMetadata.Styles) : new();

        /// <summary>
        /// コンボボックスに表示する入力デバイス一覧
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<MMDevice> _audioDevices;

        /// <summary>
        /// 選択された入力デバイス
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private MMDevice _selectedAudioDevice;

        /// <summary>
        /// 初期無音タイムアウト
        /// </summary>
        [ObservableProperty]
        private int _initialSilenceTimeoutMs = Properties.Settings.Default.SpeechServiceConnection_InitialSilenceTimeoutMs;

        /// <summary>
        /// セグメント化の無音タイムアウト
        /// </summary>
        [ObservableProperty]
        private int _segmentationSilenceTimeoutMs = Properties.Settings.Default.Speech_SegmentationSilenceTimeoutMs;

        /// <summary>
        /// WebSocketプロキシ
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private string _wssProxy = Properties.Settings.Default.WssProxy;

        /// <summary>
        /// 初期化プロンプト
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string _initialSystemPrompt = Properties.Settings.Default.InitialSystemPrompt;

        /// <summary>
        /// 最大トークン
        /// </summary>
        [ObservableProperty]
        private int _maxTokens = Properties.Settings.Default.MaxTokens;

        /// <summary>
        /// 応答多様性
        /// </summary>
        [ObservableProperty]
        private double _temperature = Properties.Settings.Default.Temperature;

        /// <summary>
        /// 選択されたキャラクター
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Styles))]
        private VoiceVoxMetaData _selectedMetadata;

        /// <summary>
        /// 選択された音声タイプ
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private VoiceVoxMetaDataStyles _selectedStyle;

        /// <summary>
        /// 入力内容がエラーを含むか
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private bool _hasError = false;

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        #endregion

        #region メンバ変数
        /// <summary>
        /// ロガー
        /// </summary>
        private readonly ILogger<SettingsViewModel> _logger;
        /// <summary>
        /// オーディオサービス
        /// </summary>
        private readonly IAudioService _audioService;
        /// <summary>
        /// VoiceVoxサービス
        /// </summary>
        private readonly IVoiceVoxService _voiceVoxService;
        /// <summary>
        /// VoiceVoxの話者ID
        /// </summary>
        private int _voiceVoxSpeakerId = Properties.Settings.Default.VoiceVoxSpeakerId;
        /// <summary>
        /// Snackbarサービス
        /// </summary>
        private readonly ISnackbarService _snackbarService;
        /// <summary>
        /// Dialogコントロール
        /// </summary>
        private readonly IDialogControl _dialogControl;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="audioService"></param>
        /// <param name="voiceVoxService"></param>
        /// <param name="snackbarService"></param>
        public SettingsViewModel(ILogger<SettingsViewModel> logger, IAudioService audioService, IVoiceVoxService voiceVoxService, ISnackbarService snackbarService, IDialogService dialogService)
        {
            _logger = logger;
            _audioService = audioService;
            _voiceVoxService = voiceVoxService;
            _snackbarService = snackbarService;
            _dialogControl = dialogService.GetDialogControl();
        }
        #endregion

        #region INavigationAware
        /// <summary>
        /// 
        /// </summary>
        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnNavigatedFrom()
        {
        }
        #endregion

        private void InitializeViewModel()
        {
            CurrentTheme = Wpf.Ui.Appearance.Theme.GetAppTheme();
            AppVersion = $"ChatAIFluentWpf - {GetAssemblyVersion()}";

            LoadDefaultSettings();

            _isInitialized = true;
        }

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? String.Empty;
        }

        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == Wpf.Ui.Appearance.ThemeType.Light)
                        break;

                    Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Light);
                    CurrentTheme = Wpf.Ui.Appearance.ThemeType.Light;

                    break;

                default:
                    if (CurrentTheme == Wpf.Ui.Appearance.ThemeType.Dark)
                        break;

                    Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Dark);
                    CurrentTheme = Wpf.Ui.Appearance.ThemeType.Dark;

                    break;
            }
        }

        #region コマンドの実装
        /// <summary>
        /// 
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanExecuteSave))]
        private void Save()
        {
            // デバイス設定
            Properties.Settings.Default.AudioDeviceId = SelectedAudioDevice?.ID;

            // 音声入力設定
            Properties.Settings.Default.SpeechServiceConnection_InitialSilenceTimeoutMs = InitialSilenceTimeoutMs;
            Properties.Settings.Default.Speech_SegmentationSilenceTimeoutMs = SegmentationSilenceTimeoutMs;
            Properties.Settings.Default.WssProxy = WssProxy;

            // チャット設定
            Properties.Settings.Default.InitialSystemPrompt = InitialSystemPrompt;
            Properties.Settings.Default.MaxTokens = MaxTokens;
            Properties.Settings.Default.Temperature = Temperature;

            // 合成音声設定
            _voiceVoxSpeakerId = SelectedStyle.Id;
            Properties.Settings.Default.VoiceVoxSpeakerId = _voiceVoxSpeakerId;

            Properties.Settings.Default.Save();
            _ = _snackbarService.ShowAsync(
                "保存完了",
                null,
                Wpf.Ui.Common.SymbolRegular.Info20,
                Wpf.Ui.Common.ControlAppearance.Info);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteSave()
        {
            return !HasError &&
                SelectedStyle != null &&
                SelectedAudioDevice != null &&
                !string.IsNullOrEmpty(InitialSystemPrompt);
        }

        /// <summary>
        /// 
        /// </summary>
        [RelayCommand]
        private async Task Cancel()
        {
            var result = await _dialogControl.ShowAndWaitAsync(
                null,
                "変更前の設定に戻します。" + Environment.NewLine +
                "※変更した情報は破棄されます");

            if (result == IDialogControl.ButtonPressed.Left)
            {
                LoadDefaultSettings();
            }

            _dialogControl.Hide();
        }
        #endregion

        #region メンバメソッド
        /// <summary>
        /// 変更前の設定を読み込む
        /// </summary>
        private void LoadDefaultSettings()
        {
            // デバイス設定
            AudioDevices = new ObservableCollection<MMDevice>(_audioService.GetActiveCapture());
            var device = AudioDevices.Where(item => item.ID.Equals(Properties.Settings.Default.AudioDeviceId)).FirstOrDefault();
            if (device != null)
            {
                SelectedAudioDevice = device;
            }
            else
            {
                SelectedAudioDevice = AudioDevices.FirstOrDefault();
            }

            // 音声入力設定
            InitialSilenceTimeoutMs = Properties.Settings.Default.SpeechServiceConnection_InitialSilenceTimeoutMs;
            SegmentationSilenceTimeoutMs = Properties.Settings.Default.Speech_SegmentationSilenceTimeoutMs;
            WssProxy = Properties.Settings.Default.WssProxy;

            // チャット設定
            InitialSystemPrompt = Properties.Settings.Default.InitialSystemPrompt;
            MaxTokens = Properties.Settings.Default.MaxTokens;
            Temperature = Properties.Settings.Default.Temperature;

            // 合成音声設定
            Metadatas = new(_voiceVoxService.Metas);
            var (meta, style) = _voiceVoxService.GetMetadataFromSpeakerId(_voiceVoxSpeakerId);
            var meta2 = Metadatas.Where(item => item.SpeakerUuid == meta.SpeakerUuid).FirstOrDefault();
            if (meta2 != null)
            {
                SelectedMetadata = meta2;
                var style2 = meta2?.Styles?.Where(item => item.Id == style.Id).FirstOrDefault();
                if (style2 != null)
                {
                    SelectedStyle = style2;
                }
            }
        }

        /// <summary>
        /// WssProxyの値が変更された時のイベントハンドラ
        /// </summary>
        /// <param name="value">変更後の値</param>
        partial void OnWssProxyChanged(string value)
        {
            if (string.IsNullOrEmpty(value) || IsValidUri(value))
            {
                HasError = false;
                ErrorMessage = string.Empty;
            }
            else
            {
                HasError = true;
                ErrorMessage = "WebSocketプロキシが正しくありません。";
            }
        }

        /// <summary>
        /// 指定した文字列がURIとして妥当か検証する
        /// </summary>
        /// <param name="uri">URI文字列</param>
        /// <returns>妥当な場合はtrue</returns>
        private static bool IsValidUri(string uri) => Uri.IsWellFormedUriString(uri, UriKind.Absolute);
        #endregion
    }
}
