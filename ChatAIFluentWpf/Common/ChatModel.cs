using System.Collections.Generic;

namespace ChatAIFluentWpf.Common
{
    /// <summary>
    /// ChatGPTのモデル
    /// </summary>
    public enum ChatModel
    {
        /// <summary>
        /// gpt-3.5-turbo
        /// </summary>
        GPT_3_5_Turbo,
    }

    /// <summary>
    /// Enumにバインドするための文字列化クラス
    /// </summary>
    public class ChatModelViewData
    {
        /// <summary>
        /// 辞書オブジェクト
        /// </summary>
        public Dictionary<ChatModel, string> Dictionary { get; } = new Dictionary<ChatModel, string>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ChatModelViewData()
        {
            Dictionary.Add(ChatModel.GPT_3_5_Turbo, "gpt-3.5-turbo");
        }
    }
}
