using ChatAIFluentWpf.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace ChatAIFluentWpf.Views.Pages
{
    /// <summary>
    /// ChatAIPage.xaml の相互作用ロジック
    /// </summary>
    public partial class ChatAIPage : INavigableView<ChatAIViewModel>
    {
        /// <summary>
        /// ViewModelインスタンス
        /// </summary>
        public ChatAIViewModel ViewModel { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="viewModel">ViewModelインスタンス</param>
        public ChatAIPage(ChatAIViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }
    }
}
