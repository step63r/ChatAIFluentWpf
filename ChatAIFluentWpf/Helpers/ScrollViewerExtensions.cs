using System;
using System.Windows;
using System.Windows.Controls;

namespace ChatAIFluentWpf.Helpers
{
    /// <summary>
    /// ScrollViewerにAlwaysScrollToEndプロパティを添付するビヘイビアクラス
    /// </summary>
    /// <see cref="https://stackoverflow.com/questions/2984803/how-to-automatically-scroll-scrollviewer-only-if-the-user-did-not-change-scrol"/>
    public class ScrollViewerExtensions
    {
        #region 依存関係プロパティ
        /// <summary>
        /// AlwaysScrollToEndの依存関係プロパティ
        /// </summary>
        public static readonly DependencyProperty AlwaysScrollToEndProperty = DependencyProperty.RegisterAttached("AlwaysScrollToEnd", typeof(bool), typeof(ScrollViewerExtensions), new PropertyMetadata(false, AlwaysScrollToEndChanged));
        #endregion

        #region メンバ変数
        /// <summary>
        /// 自動スクロールするか否か
        /// </summary>
        private static bool _autoScroll;
        #endregion

        #region メソッド
        /// <summary>
        /// プロパティが変更された時のイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void AlwaysScrollToEndChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ScrollViewer scroll)
            {
                bool alwaysScrollToEnd = (e.NewValue != null) && (bool)e.NewValue;
                if (alwaysScrollToEnd)
                {
                    scroll.ScrollToEnd();
                    scroll.ScrollChanged += ScrollChanged;
                }
                else { scroll.ScrollChanged -= ScrollChanged; }
            }
            else
            {
                throw new InvalidOperationException("The attached AlwaysScrollToEnd property can only be applied to ScrollViewer instances.");
            }
        }

        /// <summary>
        /// プロパティの設定値を取得する
        /// </summary>
        /// <param name="scroll"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool GetAlwaysScrollToEnd(ScrollViewer scroll)
        {
            if (scroll == null)
            {
                throw new ArgumentNullException(nameof(scroll));
            }
            return (bool)scroll.GetValue(AlwaysScrollToEndProperty);
        }

        /// <summary>
        /// プロパティを設定する
        /// </summary>
        /// <param name="scroll"></param>
        /// <param name="alwaysScrollToEnd"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SetAlwaysScrollToEnd(ScrollViewer scroll, bool alwaysScrollToEnd)
        {
            if (scroll == null)
            {
                throw new ArgumentNullException(nameof(scroll));
            }
            scroll.SetValue(AlwaysScrollToEndProperty, alwaysScrollToEnd);
        }

        /// <summary>
        /// ScrollViewerがスクロールされた時のイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is not ScrollViewer scroll)
            {
                throw new InvalidOperationException("The attached AlwaysScrollToEnd property can only be applied to ScrollViewer instances.");
            }

            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0)
            {
                _autoScroll = scroll.VerticalOffset == scroll.ScrollableHeight;
            }

            // Content scroll event : autoscroll eventually
            if (_autoScroll && e.ExtentHeightChange != 0)
            {
                scroll.ScrollToVerticalOffset(scroll.ExtentHeight);
            }
        }
        #endregion
    }
}
