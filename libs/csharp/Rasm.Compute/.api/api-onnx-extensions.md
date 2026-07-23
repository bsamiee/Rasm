# [RASM_COMPUTE_API_ONNX_EXTENSIONS]

`Microsoft.ML.OnnxRuntime.Extensions` ships the native extension-operator runtime (`ortextensions`) and its per-TFM MSBuild copy targets, carrying no managed public assembly. Its sole managed entry `SessionOptions.RegisterOrtExtensions()` lives in `Microsoft.ML.OnnxRuntime` and P/Invokes the shipped native asset, registering the entirely-native tokenizer, preprocessing, and postprocessing custom ops in one call. A tokenizer/detokenizer op model crosses the managed boundary through the string-tensor round-trip on `OrtValue`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime.Extensions`
- package: `Microsoft.ML.OnnxRuntime.Extensions` (MIT)
- assembly: none — no managed public assembly, no `lib/` directory
- namespace: none — the managed entry `SessionOptions.RegisterOrtExtensions` is `Microsoft.ML.OnnxRuntime` surface
- asset: native custom-operator runtime assets (`libortextensions`/`ortextensions`) and per-TFM MSBuild build targets
- build TFM: a `net10.0` non-mobile consumer binds the `netstandard2.0` `build`/`buildTransitive` targets; no `net8.0`/`net9.0` build folder ships, the remaining targets are mobile and legacy TFMs
- rail: model

## [02]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: native runtime assets

| [INDEX] | [SYMBOL]                                                     | [PACKAGE_ROLE] | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `runtimes/osx.10.14-arm64/native/libortextensions.dylib`     | native asset   | loads extension ops (macOS arm64) |
|  [02]   | `runtimes/osx.10.14-x64/native/libortextensions.dylib`       | native asset   | loads extension ops (macOS x64)   |
|  [03]   | `runtimes/linux-arm64/native/libortextensions.so`            | native asset   | loads extension ops (linux arm64) |
|  [04]   | `runtimes/linux-x64/native/libortextensions.so`              | native asset   | loads extension ops (linux x64)   |
|  [05]   | `runtimes/win-arm64/native/ortextensions.dll`                | native asset   | loads extension ops (win arm64)   |
|  [06]   | `runtimes/win-x64/native/ortextensions.dll`                  | native asset   | loads extension ops (win x64)     |
|  [07]   | `runtimes/win-x86/native/ortextensions.dll`                  | native asset   | loads extension ops (win x86)     |
|  [08]   | `runtimes/android/native/onnxruntime-extensions.aar`         | native asset   | loads Android ops                 |
|  [09]   | `runtimes/ios/native/onnxruntime_extensions.xcframework.zip` | native asset   | loads Apple ops                   |

[PACKAGE_ASSET_SCOPE]: build assets
- note: targets live per-TFM under `build/<tfm>/` and `buildTransitive/<tfm>/`, never a plain `build/native/`

| [INDEX] | [SYMBOL]                                    | [PACKAGE_ROLE] | [CAPABILITY]                                                |
| :-----: | :------------------------------------------ | :------------- | :---------------------------------------------------------- |
|  [01]   | `build/netstandard2.0/*.props` / `.targets` | MSBuild import | declares and copies native assets; `net10.0` binds this TFM |
|  [02]   | `buildTransitive/netstandard2.0/*.targets`  | MSBuild import | flows native assets transitively through a referencer       |
|  [03]   | `build/<mobile-tfm>/*.targets`              | MSBuild import | mobile and legacy TFM variants                              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: session registration

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `SessionOptions.RegisterOrtExtensions()`       | instance | registers the native extension ops              |
|  [02]   | `OrtExtensionsNativeMethods.RegisterCustomOps` | static   | internal P/Invoke `RegisterOrtExtensions` folds |

- `SessionOptions.RegisterOrtExtensions`: defined in `Microsoft.ML.OnnxRuntime`; catches the native `DllNotFoundException` and re-throws `OnnxRuntimeException(ErrorCode.NoSuchFile)` when the `ortextensions` asset is absent.
- `OrtExtensionsNativeMethods.RegisterCustomOps`: `[DllImport("ortextensions")]`, `(nint, ref OrtApiBase) -> nint`, internal — not a caller surface.

[ENTRYPOINT_SCOPE]: string-tensor boundary
- type: `CreateFromStringTensor` takes the ONNX-owned `Microsoft.ML.OnnxRuntime.Tensors.Tensor<string>`, distinct from the `System.Numerics.Tensors.Tensor<T>` the numeric `Carrier<T>` bridge rides; the two `Tensor<...>` spellings never unify.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------ | :------- | :---------------------------------------------------- |
|  [01]   | `OrtValue.CreateFromStringTensor(Tensor<string>)`             | factory  | binds the token input; the `RunInput.Strings` case    |
|  [02]   | `OrtValue.CreateTensorWithEmptyStrings(OrtAllocator, long[])` | factory  | allocates empty string-output slots a tokenizer fills |
|  [03]   | `OrtValue.GetStringElement(int)` / `GetStringTensorAsArray()` | instance | reads decoded string elements, element-wise or bulk   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every extension op is native; the surface folds through the single `SessionOptions.RegisterOrtExtensions()` registration call, guarded on asset presence.

[STACKING]:
- `api-onnxruntime`(`.api/api-onnxruntime.md`): the managed entry `SessionOptions.RegisterOrtExtensions` and the string-boundary `OrtValue` factories and readers are `Microsoft.ML.OnnxRuntime` surface; this package supplies only the native `ortextensions` asset those members P/Invoke.
- `Model/extension#EXTENSION_OPS`: `RegisterOrtExtensions()` is one arm of the `CustomOps.Register` fold gated on `SessionPolicy.OrtExtensions`, sequenced beside the leak-free per-path `RegisterCustomOpLibrary(path)` arm; the `File.Exists` guard faults `ExtensionAssetMissing` at the managed layer, ahead of the native `DllNotFoundException`.
- `Model/inference#INFERENCE_MODES`: folds the string boundary as the `RunInput.Strings` ingress (`CreateFromStringTensor` over the ONNX `Tensor<string>`), the `StringSlots` bound-output allocator (`CreateTensorWithEmptyStrings`), and the polymorphic `Egress` reader over the same `OrtValue` carrier; `Egress` discriminates on `OnnxType`, reading a `String`-tensor output through `GetStringTensorAsArray` + `GetTensorTypeAndShape().Shape` and a `ZipMap` sequence/map through `GetValueCount`/`GetValue`, so the `String` dtype stays a model-boundary row and never enters the interior tensor vocabulary.

[LOCAL_ADMISSION]:
- Extension operators enter through `SessionOptions.RegisterOrtExtensions()`; tokenizer, preprocessing, and postprocessing operators stay model-rail assets, and native asset presence is model evidence before execution admits.

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntime.Extensions`
- Owns: the native ONNX extension-operator assets
- Accept: declared custom-operator sessions
- Reject: an opaque model-preprocessor service family; the leaky `out`-handle `RegisterCustomOpLibraryV2(path, out _)` spelling whose discarded handle leaks the library; a duplicate string-input factory, a `String`-only egress reader, or a tokenizer service beside the polymorphic `Egress`
