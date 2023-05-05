using System.Windows;
using System.Windows.Controls;

namespace ChatAIFluentWpf.Views.UserControls
{
    /// <summary>
    /// MessageSentUserControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MessageSentUserControl : UserControl
    {
        #region 依存関係プロパティ
        /// <summary>
        /// メッセージの依存関係プロパティ
        /// </summary>
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(MessageSentUserControl),
            new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnMessagePropertyChanged)));
        #endregion

        #region プロパティ
        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MessageSentUserControl()
        {
            InitializeComponent();
        }
        #endregion

        #region コールバック
        /// <summary>
        /// メッセージが変更された時のコールバック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnMessagePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is MessageSentUserControl ctrl)
            {
                ctrl.tbMessage.Text = e.NewValue.ToString();
            }
        }
        #endregion
    }
}
