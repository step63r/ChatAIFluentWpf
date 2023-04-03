using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ChatAIFluentWpf.Common
{
    /// <summary>
    /// VoiceVoxメタ情報クラス
    /// </summary>
    public class VoiceVoxMetaData
    {
        /// <summary>
        /// 名前
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        /// <summary>
        /// スタイル
        /// </summary>
        [JsonPropertyName("styles")]
        public List<VoiceVoxMetaDataStyles>? Styles { get; set; }
        /// <summary>
        /// UUID
        /// </summary>
        [JsonPropertyName("speaker_uuid")]
        public string? SpeakerUuid { get; set; }
        /// <summary>
        /// バージョン
        /// </summary>
        [JsonPropertyName("version")]
        public string? Version { get; set; }

        /// <summary>
        /// VoiceVoxメタ情報スタイル
        /// </summary>
        public class VoiceVoxMetaDataStyles
        {
            /// <summary>
            /// 名前
            /// </summary>
            [JsonPropertyName("name")]
            public string? Name { get; set; }
            /// <summary>
            /// ID
            /// </summary>
            [JsonPropertyName("id")]
            public int Id { get; set; }
        }
    }
}
