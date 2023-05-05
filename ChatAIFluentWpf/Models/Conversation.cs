using System;

namespace ChatAIFluentWpf.Models
{
    /// <summary>
    /// 会話を構成するインターフェイス
    /// </summary>
    public interface IConversation
    {
        /// <summary>
        /// 名前
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 生成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// ユーザーの会話
    /// </summary>
    public class UserConversation : IConversation
    {
        /// <summary>
        /// 名前
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// 生成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">名前</param>
        /// <param name="message">メッセージ</param>
        /// <param name="createdAt">生成日時</param>
        public UserConversation(string name, string message, DateTime createdAt)
        {
            Name = name;
            Message = message;
            CreatedAt = createdAt;
        }
    }

    /// <summary>
    /// チャットボットの会話
    /// </summary>
    public class BotConversation: IConversation
    {
        /// <summary>
        /// 名前
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// 生成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">名前</param>
        /// <param name="message">メッセージ</param>
        /// <param name="createdAt">生成日時</param>
        public BotConversation(string name, string message, DateTime createdAt)
        {
            Name = name;
            Message = message;
            CreatedAt = createdAt;
        }
    }
}
