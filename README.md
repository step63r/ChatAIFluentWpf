# ChatAI Fluent WPF

## Description

Simple chat app with ChatGPT.

## Requirement

- Windows 11 Pro 22H2+
- .NET 6
- Visual Studio 2022
- VoiceVox Core 0.14.2

## Usage

At first, you have to setup VoiceVox Core library.

See [VOICEVOX/voicevox_core](https://github.com/VOICEVOX/voicevox_core) for details.

For example, you have to put these files to build output directory (bin/Debug/net6.0-windows) for GPU-accelerated execution.

- model/
- open_jtalk_dic_utf_8-1.11/
- cublas64_11.dll
- cublasLt64_11.dll
- cudart64_110.dll
- cudnn_adv_infer64_8.dll
- cudnn_cnn_infer64_8.dll
- cudnn_ops_infer64_8.dll
- cudnn64_8.dll
- cufft64_10.dll
- curand64_10.dll
- onnxruntime.dll
- onnxruntime_providers_cuda.dll
- onnxruntime_providers_shared.dll
- onnxruntime_providers_tensorrt.dll
- voicevox_core.dll
- zlibwapi.dll

Zlib is available at [NVIDIA Developer](https://docs.nvidia.com/deeplearning/cudnn/install-guide/index.html#install-zlib-windows).

## Install

Fork and clone this repository.

```
$ git clone git@github.com:yourname/ChatAIFluentWpf.git
```

## Contribution

1. Fork this repositor
2. Create your feature branc
3. Commit your change
4. Push to the branch
5. Create new Pull Request

## License

MIT License

## Author

[minato](https://blog.minatoproject.com/)
