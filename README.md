# ChatAI Fluent WPF

![](./ChatAIFluentWpf.png)

## Description

ChatGPT�Ɖ�b����V���v���ȃ`���b�g�A�v���ł��B

�������͂𕶎��񉻂���ChatGPT�ɑ���A�Ԃ��Ă������e�����������Œ��点�܂��B

�������͂ɂ� [Azure Cognitive Services (Speech to Text)](https://azure.microsoft.com/ja-jp/products/cognitive-services/speech-to-text/) ���A���������ɂ� [VOICEVOX Core](https://github.com/VOICEVOX/voicevox_core) �𗘗p���Ă��܂��B

## Requirement

- Windows 11 Pro 22H2+
- .NET 6
- Visual Studio 2022
- VOICEVOX Core 0.14.2

## Usage

�͂��߂� VOICEVOX �R�A���C�u�������Z�b�g�A�b�v���܂��B

�ڍׂ� [VOICEVOX/voicevox_core](https://github.com/VOICEVOX/voicevox_core) ���Q�Ƃ��������B

�Ⴆ��CPU�ɂ�鉹�������ɂ̓r���h�o�͐�t�H���_ (bin/Debug[Release]/net6.0-windows) �z���Ɉȉ��̃��W���[������z�u���Ă����K�v������܂��B

- model/
- open_jtalk_dic_utf_8-1.11/
- onnxruntime.dll
- onnxruntime_providers_shared.dll
- voicevox_core.dll
- zlibwapi.dll

GPU�ɂ�鉹�������ɂ͒ǉ��ňȉ��̃��W���[�����K�v�ł��B

- cublas64_11.dll
- cublasLt64_11.dll
- cudart64_110.dll
- cudnn_adv_infer64_8.dll
- cudnn_cnn_infer64_8.dll
- cudnn_ops_infer64_8.dll
- cudnn64_8.dll
- cufft64_10.dll
- curand64_10.dll
- onnxruntime_providers_cuda.dll
- onnxruntime_providers_tensorrt.dll

zlibwapi.dll ��[NVIDIA Developer](https://docs.nvidia.com/deeplearning/cudnn/install-guide/index.html#install-zlib-windows) �œ���\�ł��B

## Install

���̃��|�W�g�����t�H�[�N���ăN���[�����܂��B

```
$ git clone git@github.com:yourname/ChatAIFluentWpf.git
```

## Contribution

1. ���̃��|�W�g�����t�H�[�N����
2. �ύX��������
3. �ύX���R�~�b�g����
4. �u�����`�Ƀv�b�V������
5. �v�����N�G�X�g���쐬����

## License

MIT License

## Author

[minato](https://blog.minatoproject.com/)
