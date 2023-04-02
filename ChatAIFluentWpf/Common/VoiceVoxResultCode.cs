namespace ChatAIFluentWpf.Common
{
    /// <summary>
    /// 処理結果を示す結果コード
    /// </summary>
    public enum VoiceVoxResultCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        VOICEVOX_RESULT_OK,
        /// <summary>
        /// open_jtalk辞書ファイルが読み込まれていない
        /// </summary>
        VOICEVOX_RESULT_NOT_LOADED_OPENJTALK_DICT_ERROR,
        /// <summary>
        /// modelの読み込みに失敗した
        /// </summary>
        VOICEVOX_RESULT_LOAD_MODEL_ERROR,
        /// <summary>
        /// サポートされているデバイス情報取得に失敗した
        /// </summary>
        VOICEVOX_RESULT_GET_SUPPORTED_DEVICES_ERROR,
        /// <summary>
        /// GPUモードがサポートされていない
        /// </summary>
        VOICEVOX_RESULT_GPU_SUPPORT_ERROR,
        /// <summary>
        /// メタ情報読み込みに失敗した
        /// </summary>
        VOICEVOX_RESULT_LOAD_METAS_ERROR,
        /// <summary>
        /// ステータスが初期化されていない
        /// </summary>
        VOICEVOX_RESULT_UNINITIALIZED_STATUS_ERROR,
        /// <summary>
        /// 無効なspeaker_idが指定された
        /// </summary>
        VOICEVOX_RESULT_INVALID_SPEAKER_ID_ERROR,
        /// <summary>
        /// 無効なmodel_indexが指定された
        /// </summary>
        VOICEVOX_RESULT_INVALID_MODEL_INDEX_ERROR,
        /// <summary>
        /// 推論に失敗した
        /// </summary>
        VOICEVOX_RESULT_INFERENCE_ERROR,
        /// <summary>
        /// コンテキストラベル出力に失敗した
        /// </summary>
        VOICEVOX_RESULT_EXTRACT_FULL_CONTEXT_LABEL_ERROR,
        /// <summary>
        /// 無効なutf8文字列が入力された
        /// </summary>
        VOICEVOX_RESULT_INVALID_UTF8_INPUT_ERROR,
        /// <summary>
        /// aquestalk形式のテキストの解析に失敗した
        /// </summary>
        VOICEVOX_RESULT_PARSE_KANA_ERROR,
        /// <summary>
        /// 無効なAudioQuery
        /// </summary>
        VOICEVOX_RESULT_INVALID_AUDIO_QUERY_ERROR
    }
}
