using ChatAIFluentWpf.Clr;
using ChatAIFluentWpf.Common;
using ChatAIFluentWpf.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace ChatAIFluentWpf.Services
{
    public class VoiceVoxService : IVoiceVoxService
    {
        #region プロパティ
        /// <summary>
        /// メタ情報
        /// </summary>
        public List<VoiceVoxMetaData> Metas
            => JsonSerializer.Deserialize<List<VoiceVoxMetaData>>(_wrapper.GetMetasJson());
        /// <summary>
        /// バージョン情報
        /// </summary>
        public string Version => _wrapper.GetVersion();
        /// <summary>
        /// GPUモード
        /// </summary>
        public bool IsGpuMode => _wrapper.IsGpuMode();
        /// <summary>
        /// 話者ID
        /// </summary>
        public int SpeakerId
        {
            get => _wrapper.GetSpeakerId();
            set
            {
                _wrapper.SetSpeakerId(value);
            }
        }
        #endregion

        #region メンバ変数
        /// <summary>
        /// ロガー
        /// </summary>
        private readonly ILogger<VoiceVoxService> _logger;
        /// <summary>
        /// VoiceVoxをCLRで使うためのラッパークラス
        /// </summary>
        private readonly VoiceVoxWrapper _wrapper;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger">ロガー</param>
        public VoiceVoxService(ILogger<VoiceVoxService> logger)
        {
            _logger = logger;
            _wrapper = new VoiceVoxWrapper("open_jtalk_dic_utf_8-1.11");
        }
        #endregion

        #region メソッド
        /// <summary>
        /// coreの初期化
        /// </summary>
        /// <returns></returns>
        public VoiceVoxResultCode Initialize()
        {
            _logger.LogInformation("start");
            var ret = ConvertFromInt(_wrapper.Initialize());
            _logger.LogInformation($"end [{ret}]");
            return ret;
        }

        /// <summary>
        /// モデルを読み込む
        /// </summary>
        /// <param name="speakerId"></param>
        /// <returns></returns>
        public VoiceVoxResultCode LoadModel(int speakerId)
        {
            _logger.LogInformation("start");
            var ret = ConvertFromInt(_wrapper.LoadModel(speakerId));
            _logger.LogInformation($"end [{ret}]");
            return ret;
        }

        /// <summary>
        /// 音声の生成
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public VoiceVoxResultCode GenerateVoice(string words)
        {
            _logger.LogInformation("start");
            var ret = ConvertFromInt(_wrapper.GenerateVoice(words));
            _logger.LogInformation($"end [{ret}]");
            return ret;
        }

        /// <summary>
        /// 指定した話者IDのモデルが読み込まれているか
        /// </summary>
        /// <param name="speakerId">話者ID</param>
        /// <returns></returns>
        public bool IsModelLoaded(int speakerId)
        {
            _logger.LogInformation("start");
            bool ret = _wrapper.IsModelLoaded(speakerId);
            _logger.LogInformation($"end [{ret}]");
            return ret;
        }

        /// <summary>
        /// 話者IDからメタ情報を取得する
        /// </summary>
        /// <param name="speakerId">話者ID</param>
        /// <returns></returns>
        public (VoiceVoxMetaData Meta, VoiceVoxMetaData.VoiceVoxMetaDataStyles Style) GetMetadataFromSpeakerId(int speakerId)
        {
            foreach (var meta in Metas)
            {
                var style = meta.Styles?.Where(item => item.Id == speakerId).FirstOrDefault();
                if (style != null)
                {
                    return (meta, style);
                }
            }
            throw new Exception($"Speaker ID: {speakerId} not found.");
        }

        /// <summary>
        /// int -> VoiceVoxResultCode
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private static VoiceVoxResultCode ConvertFromInt(int number)
        {
            return (VoiceVoxResultCode)Enum.ToObject(typeof(VoiceVoxResultCode), number);
        }
        #endregion
    }
}
