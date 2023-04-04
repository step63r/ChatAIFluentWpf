using ChatAIFluentWpf.Common;
using System.Collections.Generic;

namespace ChatAIFluentWpf.Services.Interfaces
{
    /// <summary>
    /// VoiceVoxサービス インターフェイス
    /// </summary>
    public interface IVoiceVoxService
    {
        #region プロパティ
        /// <summary>
        /// メタ情報
        /// </summary>
        List<VoiceVoxMetaData> Metas { get; }

        /// <summary>
        /// バージョン情報
        /// </summary>
        string Version { get; }

        /// <summary>
        /// GPUモード
        /// </summary>
        bool IsGpuMode { get; }
        #endregion

        #region メソッド
        /// <summary>
        /// coreの初期化
        /// </summary>
        /// <returns></returns>
        VoiceVoxResultCode Initialize();

        /// <summary>
        /// モデルを読み込む
        /// </summary>
        /// <param name="speakerId">話者ID</param>
        /// <returns></returns>
        VoiceVoxResultCode LoadModel(int speakerId);

        /// <summary>
        /// 音声の生成
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        VoiceVoxResultCode GenerateVoice(string words);
        #endregion
    }
}
