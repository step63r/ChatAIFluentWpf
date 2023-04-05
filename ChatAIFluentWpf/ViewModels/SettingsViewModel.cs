using ChatAIFluentWpf.Common;
using ChatAIFluentWpf.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Wpf.Ui.Common.Interfaces;
using static ChatAIFluentWpf.Common.VoiceVoxMetaData;

namespace ChatAIFluentWpf.ViewModels
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _appVersion = String.Empty;

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
        /// 選択されたキャラクター
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Styles))]
        private VoiceVoxMetaData _selectedMetadata;

        /// <summary>
        /// 選択された音声タイプ
        /// </summary>
        [ObservableProperty]
        private VoiceVoxMetaDataStyles _selectedStyle;

        #region メンバ変数
        /// <summary>
        /// ロガー
        /// </summary>
        private readonly ILogger<SettingsViewModel> _logger;
        /// <summary>
        /// VoiceVoxサービス
        /// </summary>
        private readonly IVoiceVoxService _voiceVoxService;
        /// <summary>
        /// VoiceVoxの話者ID
        /// </summary>
        private int _voiceVoxSpeakerId = Properties.Settings.Default.VoiceVoxSpeakerId;
        #endregion

        public SettingsViewModel(ILogger<SettingsViewModel> logger, IVoiceVoxService voiceVoxService)
        {
            _logger = logger;
            _voiceVoxService = voiceVoxService;
        }

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        public void OnNavigatedFrom()
        {
        }

        private void InitializeViewModel()
        {
            CurrentTheme = Wpf.Ui.Appearance.Theme.GetAppTheme();
            AppVersion = $"ChatAIFluentWpf - {GetAssemblyVersion()}";

            Metadatas = new(_voiceVoxService.Metas);
            var (meta, style) = _voiceVoxService.GetMetadataFromSpeakerId(_voiceVoxSpeakerId);
            var meta2 = Metadatas.Where(item => item.SpeakerUuid == meta.SpeakerUuid).FirstOrDefault();
            if (meta2 != null)
            {
                SelectedMetadata = meta2;
                var style2 = meta2.Styles.Where(item => item.Id == style.Id).FirstOrDefault();
                if (style2 != null)
                {
                    SelectedStyle = style2;
                }
            }

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

        /// <summary>
        /// 
        /// </summary>
        [RelayCommand]
        private void Save()
        {
            _voiceVoxSpeakerId = SelectedStyle.Id;
            Properties.Settings.Default.VoiceVoxSpeakerId = _voiceVoxSpeakerId;
            Properties.Settings.Default.Save();
        }
    }
}
