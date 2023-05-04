#include "ChatAIFluentWpf.Clr.h"

using namespace ChatAIFluentWpf::Clr;

#pragma region VoiceVoxWrapper
/// <summary>
/// コンストラクタ
/// </summary>
/// <param name="dict"></param>
VoiceVoxWrapper::VoiceVoxWrapper(String^ dict)
{
    auto wsDict = marshal_as<std::wstring>(dict);
    m_lpVoiceVox = new MyVoiceVox(wsDict);
}

/// <summary>
/// デストラクタ
/// </summary>
VoiceVoxWrapper::~VoiceVoxWrapper()
{
    if (m_lpVoiceVox != nullptr) {
        delete m_lpVoiceVox;
        m_lpVoiceVox = nullptr;
    }
}

/// <summary>
/// メタ情報取得
/// </summary>
/// <returns>メタ情報</returns>
String^ VoiceVoxWrapper::GetMetasJson()
{
    return gcnew String(m_lpVoiceVox->GetMetasJson().c_str());
}

/// <summary>
/// バージョン取得
/// </summary>
/// <returns>バージョン</returns>
String^ VoiceVoxWrapper::GetVersion()
{
    return gcnew String(m_lpVoiceVox->GetVersion().c_str());
}

/// <summary>
/// 話者ID取得
/// </summary>
/// <returns>話者ID</returns>
int VoiceVoxWrapper::GetSpeakerId()
{
    return m_lpVoiceVox->GetSpeakerId();
}

/// <summary>
/// 話者ID設定
/// </summary>
/// <param name="speakerId">話者ID</param>
void VoiceVoxWrapper::SetSpeakerId(int speakerId)
{
    m_lpVoiceVox->SetSpeakerId(speakerId);
}

/// <summary>
/// GPUモード取得
/// </summary>
/// <returns>GPUモード</returns>
bool VoiceVoxWrapper::IsGpuMode()
{
    return m_lpVoiceVox->IsGpuMode();
}

/// <summary>
/// 指定した話者IDのモデルが読み込まれているか
/// </summary>
/// <param name="speakerId">話者ID</param>
/// <returns></returns>
bool VoiceVoxWrapper::IsModelLoaded(int speakerId)
{
    return m_lpVoiceVox->IsModelLoaded(speakerId);
}

/// <summary>
/// coreの初期化
/// </summary>
/// <returns></returns>
int VoiceVoxWrapper::Initialize()
{
    return m_lpVoiceVox->Initialize();
}

/// <summary>
/// モデルを読み込む
/// </summary>
/// <param name="speaker_id">話者ID</param>
/// <returns></returns>
int VoiceVoxWrapper::LoadModel(int speaker_id)
{
    return m_lpVoiceVox->LoadModel(speaker_id);
}

/// <summary>
/// 音声の生成
/// </summary>
/// <param name="words"></param>
/// <returns></returns>
int VoiceVoxWrapper::GenerateVoice(String^ words)
{
    auto wsWords = marshal_as<std::wstring>(words);
    return m_lpVoiceVox->GenerateVoice(wsWords);
}
#pragma endregion

#pragma region MyVoiceVox
/// <summary>
/// コンストラクタ
/// </summary>
/// <param name="sDict">OpenJTalk辞書ファイルパス</param>
MyVoiceVox::MyVoiceVox(std::wstring wsDict) {
    std::wcout.imbue(std::locale(""));
    std::wcin.imbue(std::locale(""));

    m_wsDict = wsDict;
    m_output_wav = nullptr;
}

/// <summary>
/// デストラクタ
/// </summary>
MyVoiceVox::~MyVoiceVox() {
    voicevox_finalize();
}

/// <summary>
/// メタ情報取得
/// </summary>
/// <returns>メタ情報文字列</returns>
std::wstring MyVoiceVox::GetMetasJson() {
    return utf8_to_wide_cppapi(voicevox_get_metas_json());
}

/// <summary>
/// バージョン取得
/// </summary>
/// <returns>バージョン</returns>
std::string MyVoiceVox::GetVersion() {
    return voicevox_get_version();
}

/// <summary>
/// 話者ID取得
/// </summary>
/// <returns>話者ID</returns>
int MyVoiceVox::GetSpeakerId() {
    return m_iSpeakerId;
}

/// <summary>
/// 話者ID設定
/// </summary>
/// <param name="iSpeakerId">話者ID</param>
void MyVoiceVox::SetSpeakerId(int iSpeakerId) {
    m_iSpeakerId = iSpeakerId;
}

/// <summary>
/// GPUモード取得
/// </summary>
/// <returns>GPUモード</returns>
bool MyVoiceVox::IsGpuMode() {
    return voicevox_is_gpu_mode();
}

/// <summary>
/// 指定した話者IDのモデルが読み込まれているか
/// </summary>
/// <param name="iSpeakerId">話者ID</param>
/// <returns></returns>
bool MyVoiceVox::IsModelLoaded(int iSpeakerId) {
    return voicevox_is_model_loaded(iSpeakerId);
}

/// <summary>
/// coreの初期化
/// </summary>
/// <returns></returns>
HRESULT MyVoiceVox::Initialize() {
    auto initializeOptions = voicevox_make_default_initialize_options();
    std::string dict = GetOpenJTalkDict();
    auto sDict = wide_to_utf8_cppapi(m_wsDict);
    initializeOptions.open_jtalk_dict_dir = sDict.c_str();
    initializeOptions.load_all_models = false;

    return voicevox_initialize(initializeOptions);
}

/// <summary>
/// モデルを読み込む
/// </summary>
/// <param name="iSpeakerId">話者ID</param>
/// <returns></returns>
HRESULT MyVoiceVox::LoadModel(int iSpeakerId) {
    m_iSpeakerId = iSpeakerId;
    return voicevox_load_model(iSpeakerId);
}

/// <summary>
/// 音声の生成
/// </summary>
/// <returns></returns>
HRESULT MyVoiceVox::GenerateVoice(std::wstring wsWords) {
    int32_t speaker_id = m_iSpeakerId;
    uintptr_t output_binary_size = 0;
    uint8_t* output_wav = nullptr;

    auto ttsOptions = voicevox_make_default_tts_options();
    auto result = voicevox_tts(wide_to_utf8_cppapi(wsWords).c_str(), speaker_id, ttsOptions, &output_binary_size, &output_wav);
    if (result != VoicevoxResultCode::VOICEVOX_RESULT_OK) {
        return result;
    }

    {
        m_output_wav = output_wav;
        // 音声ファイルの保存
        std::ofstream out_stream(GetWaveFileName().c_str(), std::ios::binary);
        out_stream.write(reinterpret_cast<const char*>(output_wav), output_binary_size);
    }
    return result;
}

/// <summary>
/// OpenJTalk辞書のパスを取得
/// </summary>
/// <returns></returns>
std::string MyVoiceVox::GetOpenJTalkDict() {
    wchar_t buff[MAX_PATH] = { 0 };
    ::PathCchCombine(buff, MAX_PATH, GetExeDirectory().c_str(), m_wsDict.c_str());
    std::string retVal = wide_to_utf8_cppapi(buff);
    return retVal;
}

/// <summary>
/// 音声ファイル名を取得
/// </summary>
/// <returns></returns>
std::wstring MyVoiceVox::GetWaveFileName() {
    wchar_t buff[MAX_PATH] = { 0 };
    ::PathCchCombine(buff, MAX_PATH, GetExeDirectory().c_str(), L"speech.wav");
    return std::wstring(buff);
}

/// <summary>
/// 自分自身のあるパスを取得する
/// </summary>
/// <returns></returns>
std::wstring MyVoiceVox::GetExePath() {
    wchar_t buff[MAX_PATH] = { 0 };
    ::GetModuleFileName(nullptr, buff, MAX_PATH);
    return std::wstring(buff);
}

/// <summary>
/// 自分自身のあるディレクトリを取得する
/// </summary>
/// <returns></returns>
std::wstring MyVoiceVox::GetExeDirectory() {
    wchar_t buff[MAX_PATH] = { 0 };
    wcscpy_s(buff, MAX_PATH, GetExePath().c_str());
    //フルパスからファイル名の削除
    ::PathRemoveFileSpec(buff);
    return std::wstring(buff);
}

/// <summary>
/// ワイド文字列をUTF8に変換する
/// </summary>
/// <param name="src"></param>
/// <returns></returns>
std::string MyVoiceVox::wide_to_utf8_cppapi(std::wstring const& src) {
    std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
    return converter.to_bytes(src);
}

/// <summary>
/// UTF8をワイド文字列に変換する
/// </summary>
/// <param name="src"></param>
/// <returns></returns>
std::wstring MyVoiceVox::utf8_to_wide_cppapi(std::string const& src) {
    std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
    return converter.from_bytes(src);
}
#pragma endregion