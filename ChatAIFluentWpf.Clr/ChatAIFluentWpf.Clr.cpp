#include "ChatAIFluentWpf.Clr.h"

using namespace ChatAIFluentWpf::Clr;

#pragma region VoiceVoxWrapper
/// <summary>
/// �R���X�g���N�^
/// </summary>
/// <param name="dict"></param>
VoiceVoxWrapper::VoiceVoxWrapper(String^ dict)
{
    auto wsDict = marshal_as<std::wstring>(dict);
    m_lpVoiceVox = new MyVoiceVox(wsDict);
}

/// <summary>
/// �f�X�g���N�^
/// </summary>
VoiceVoxWrapper::~VoiceVoxWrapper()
{
    if (m_lpVoiceVox != nullptr) {
        delete m_lpVoiceVox;
        m_lpVoiceVox = nullptr;
    }
}

/// <summary>
/// ���^���擾
/// </summary>
/// <returns>���^���</returns>
String^ VoiceVoxWrapper::GetMetasJson()
{
    return gcnew String(m_lpVoiceVox->GetMetasJson().c_str());
}

/// <summary>
/// �o�[�W�����擾
/// </summary>
/// <returns>�o�[�W����</returns>
String^ VoiceVoxWrapper::GetVersion()
{
    return gcnew String(m_lpVoiceVox->GetVersion().c_str());
}

/// <summary>
/// �b��ID�擾
/// </summary>
/// <returns>�b��ID</returns>
int VoiceVoxWrapper::GetSpeakerId()
{
    return m_lpVoiceVox->GetSpeakerId();
}

/// <summary>
/// �b��ID�ݒ�
/// </summary>
/// <param name="speakerId">�b��ID</param>
void VoiceVoxWrapper::SetSpeakerId(int speakerId)
{
    m_lpVoiceVox->SetSpeakerId(speakerId);
}

/// <summary>
/// GPU���[�h�擾
/// </summary>
/// <returns>GPU���[�h</returns>
bool VoiceVoxWrapper::IsGpuMode()
{
    return m_lpVoiceVox->IsGpuMode();
}

/// <summary>
/// �w�肵���b��ID�̃��f�����ǂݍ��܂�Ă��邩
/// </summary>
/// <param name="speakerId">�b��ID</param>
/// <returns></returns>
bool VoiceVoxWrapper::IsModelLoaded(int speakerId)
{
    return m_lpVoiceVox->IsModelLoaded(speakerId);
}

/// <summary>
/// core�̏�����
/// </summary>
/// <returns></returns>
int VoiceVoxWrapper::Initialize()
{
    return m_lpVoiceVox->Initialize();
}

/// <summary>
/// ���f����ǂݍ���
/// </summary>
/// <param name="speaker_id">�b��ID</param>
/// <returns></returns>
int VoiceVoxWrapper::LoadModel(int speaker_id)
{
    return m_lpVoiceVox->LoadModel(speaker_id);
}

/// <summary>
/// �����̐���
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
/// �R���X�g���N�^
/// </summary>
/// <param name="sDict">OpenJTalk�����t�@�C���p�X</param>
MyVoiceVox::MyVoiceVox(std::wstring wsDict) {
    std::wcout.imbue(std::locale(""));
    std::wcin.imbue(std::locale(""));

    m_wsDict = wsDict;
    m_output_wav = nullptr;
}

/// <summary>
/// �f�X�g���N�^
/// </summary>
MyVoiceVox::~MyVoiceVox() {
    voicevox_finalize();
}

/// <summary>
/// ���^���擾
/// </summary>
/// <returns>���^��񕶎���</returns>
std::wstring MyVoiceVox::GetMetasJson() {
    return utf8_to_wide_cppapi(voicevox_get_metas_json());
}

/// <summary>
/// �o�[�W�����擾
/// </summary>
/// <returns>�o�[�W����</returns>
std::string MyVoiceVox::GetVersion() {
    return voicevox_get_version();
}

/// <summary>
/// �b��ID�擾
/// </summary>
/// <returns>�b��ID</returns>
int MyVoiceVox::GetSpeakerId() {
    return m_iSpeakerId;
}

/// <summary>
/// �b��ID�ݒ�
/// </summary>
/// <param name="iSpeakerId">�b��ID</param>
void MyVoiceVox::SetSpeakerId(int iSpeakerId) {
    m_iSpeakerId = iSpeakerId;
}

/// <summary>
/// GPU���[�h�擾
/// </summary>
/// <returns>GPU���[�h</returns>
bool MyVoiceVox::IsGpuMode() {
    return voicevox_is_gpu_mode();
}

/// <summary>
/// �w�肵���b��ID�̃��f�����ǂݍ��܂�Ă��邩
/// </summary>
/// <param name="iSpeakerId">�b��ID</param>
/// <returns></returns>
bool MyVoiceVox::IsModelLoaded(int iSpeakerId) {
    return voicevox_is_model_loaded(iSpeakerId);
}

/// <summary>
/// core�̏�����
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
/// ���f����ǂݍ���
/// </summary>
/// <param name="iSpeakerId">�b��ID</param>
/// <returns></returns>
HRESULT MyVoiceVox::LoadModel(int iSpeakerId) {
    m_iSpeakerId = iSpeakerId;
    return voicevox_load_model(iSpeakerId);
}

/// <summary>
/// �����̐���
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
        // �����t�@�C���̕ۑ�
        std::ofstream out_stream(GetWaveFileName().c_str(), std::ios::binary);
        out_stream.write(reinterpret_cast<const char*>(output_wav), output_binary_size);
    }
    return result;
}

/// <summary>
/// OpenJTalk�����̃p�X���擾
/// </summary>
/// <returns></returns>
std::string MyVoiceVox::GetOpenJTalkDict() {
    wchar_t buff[MAX_PATH] = { 0 };
    ::PathCchCombine(buff, MAX_PATH, GetExeDirectory().c_str(), m_wsDict.c_str());
    std::string retVal = wide_to_utf8_cppapi(buff);
    return retVal;
}

/// <summary>
/// �����t�@�C�������擾
/// </summary>
/// <returns></returns>
std::wstring MyVoiceVox::GetWaveFileName() {
    wchar_t buff[MAX_PATH] = { 0 };
    ::PathCchCombine(buff, MAX_PATH, GetExeDirectory().c_str(), L"speech.wav");
    return std::wstring(buff);
}

/// <summary>
/// �������g�̂���p�X���擾����
/// </summary>
/// <returns></returns>
std::wstring MyVoiceVox::GetExePath() {
    wchar_t buff[MAX_PATH] = { 0 };
    ::GetModuleFileName(nullptr, buff, MAX_PATH);
    return std::wstring(buff);
}

/// <summary>
/// �������g�̂���f�B���N�g�����擾����
/// </summary>
/// <returns></returns>
std::wstring MyVoiceVox::GetExeDirectory() {
    wchar_t buff[MAX_PATH] = { 0 };
    wcscpy_s(buff, MAX_PATH, GetExePath().c_str());
    //�t���p�X����t�@�C�����̍폜
    ::PathRemoveFileSpec(buff);
    return std::wstring(buff);
}

/// <summary>
/// ���C�h�������UTF8�ɕϊ�����
/// </summary>
/// <param name="src"></param>
/// <returns></returns>
std::string MyVoiceVox::wide_to_utf8_cppapi(std::wstring const& src) {
    std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
    return converter.to_bytes(src);
}

/// <summary>
/// UTF8�����C�h������ɕϊ�����
/// </summary>
/// <param name="src"></param>
/// <returns></returns>
std::wstring MyVoiceVox::utf8_to_wide_cppapi(std::string const& src) {
    std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
    return converter.from_bytes(src);
}
#pragma endregion