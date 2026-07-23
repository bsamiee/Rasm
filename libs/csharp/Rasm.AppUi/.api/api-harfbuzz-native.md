# [RASM_APPUI_API_HARFBUZZ_NATIVE]

`HarfBuzzSharp.NativeAssets.macOS` and `HarfBuzzSharp.NativeAssets.Linux` place the per-platform `libHarfBuzzSharp` payload that `HarfBuzzSharp` and `SkiaSharp.HarfBuzz` P/Invoke, and the `buildTransitive` targets that copy the RID-matched library into build output. Neither ships a managed assembly, so these packages own only the native shaping load identity the typography evidence stream records across the live-macOS (`osx-arm64`) and headless-Linux profiles. Managed shaping API facts live in `api-skia-harfbuzz.md`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HarfBuzzSharp.NativeAssets.macOS`
- package: `HarfBuzzSharp.NativeAssets.macOS` (MIT, © Microsoft Corporation)
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: `runtimes/osx/native/libHarfBuzzSharp.dylib` (fat arm64+x64) + buildTransitive `.targets`
- rail: typography

[PACKAGE_SURFACE]: `HarfBuzzSharp.NativeAssets.Linux`
- package: `HarfBuzzSharp.NativeAssets.Linux` (MIT, © Microsoft Corporation)
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: `runtimes/<rid>/native/libHarfBuzzSharp.so` across the glibc/musl/bionic Linux RIDs + buildTransitive `.targets`
- rail: typography

## [02]-[PACKAGE_ASSETS]

[MACOS_NATIVE]: fat `libHarfBuzzSharp.dylib`; buildTransitive `.targets` carry copy logic on `net462`/`net48` only, the macOS-TFM groups empty.

| [INDEX] | [ASSET]                                                                                                         | [ROLE]             |
| :-----: | :-------------------------------------------------------------------------------------------------------------- | :----------------- |
|  [01]   | `runtimes/osx/native/libHarfBuzzSharp.dylib`                                                                    | fat arm64+x64      |
|  [02]   | `buildTransitive/{net462,net48}/*.targets`                                                                      | full-fw copy logic |
|  [03]   | `buildTransitive/{net9.0-macos15.0,net10.0-macos26.0}/*.targets`                                                | empty no-op        |
|  [04]   | `lib/{net10.0,net10.0-macos26.0,net9.0,net9.0-macos15.0,net6.0,net462,net48,netstandard2.0,netstandard2.1}/_._` | compile markers    |

[LINUX_NATIVE]: `libHarfBuzzSharp.so` across the glibc/musl/bionic RIDs; buildTransitive `.targets` carry copy logic on `net462`/`net48` only, `net10.0` resolves the RID payload through the SDK graph.

| [INDEX] | [ASSET]                                                                              | [ROLE]          |
| :-----: | :----------------------------------------------------------------------------------- | :-------------- |
|  [01]   | `runtimes/linux-{x64,x86,arm,arm64,riscv64,loongarch64}/native/libHarfBuzzSharp.so`  | glibc RID       |
|  [02]   | `runtimes/linux-musl-{x64,arm,arm64,riscv64,loongarch64}/native/libHarfBuzzSharp.so` | musl RID        |
|  [03]   | `runtimes/linux-bionic-{x64,arm64}/native/libHarfBuzzSharp.so`                       | bionic RID      |
|  [04]   | `buildTransitive/{net462,net48}/*.targets`                                           | full-fw copy    |
|  [05]   | `lib/{net10.0,net9.0,net6.0,net462,net48,netstandard2.0,netstandard2.1}/_._`         | compile markers |

## [03]-[ASSET_ENTRYPOINTS]

[ASSET_ENTRYPOINTS]: RID selection and build-time output copy — no managed call surface.

| [INDEX] | [SURFACE]                          | [ROOT]                           | [ROLE]                       |
| :-----: | :--------------------------------- | :------------------------------- | :--------------------------- |
|  [01]   | `ShouldIncludeNativeHarfBuzzSharp` | `buildTransitive/{net462,net48}` | copy opt-out, default `True` |
|  [02]   | RID asset graph                    | `runtimes/<rid>/native`          | SDK copy on `net10.0`        |
|  [03]   | `libHarfBuzzSharp.{dylib,so}`      | `runtimes/<rid>/native`          | dlopen target, RID selection |
|  [04]   | load-identity probe                | loaded library handle            | version/path/RID evidence    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `libHarfBuzzSharp` reaches build output at the RID-correct path for every admitted profile, or the first `HarfBuzzSharp`/`SKShaper` shape faults at load time with `DllNotFoundException`, never a compile error.
- Full-framework consumers (`net462`/`net48`) copy the native through the `_NativeHarfBuzzSharpFile` glob gated on `ShouldIncludeNativeHarfBuzzSharp`; `net10.0` resolves the same payload through the SDK RID-asset graph, so the matching `buildTransitive/net10.0-*`/`net9.0-*` targets stay empty.

[STACKING]:
- `api-skia-harfbuzz.md`: `SKShaper`/`HarfBuzzSharp.Font.Shape` P/Invoke the `libHarfBuzzSharp` these packages place; one resolved native serves both the retained Avalonia text stack and the shaped-draw rail, and the first shaped draw folds its version, path, and RID into the typography evidence stream beside the `libSkiaSharp` identity.
- `api-skia-native.md`: `libSkiaSharp` rides the same RID graph from the parallel Skia native-asset pair, so one RID-correct output tree carries both text natives for a profile.

[LOCAL_ADMISSION]:
- `HarfBuzzSharp.NativeAssets.macOS` and `.Linux` are the admitted per-platform text natives for the live-macOS (`osx-arm64`) and headless-Linux profiles; `HarfBuzzSharp.NativeAssets.Win32` and `.WebAssembly` reach the restore graph only transitively through the `SkiaSharp.HarfBuzz`/Avalonia closure and never copy-local into AppUi output.

[RAIL_LAW]:
- Package: `HarfBuzzSharp.NativeAssets.macOS`, `HarfBuzzSharp.NativeAssets.Linux`
- Owns: the per-platform HarfBuzz native load identity, the `buildTransitive` copy targets, and per-RID output-asset presence
- Accept: the native arrives through the package `runtimes/<rid>/native` payload and the RID-asset/targets copy, recorded into typography evidence at the first shaped draw; `ShouldIncludeNativeHarfBuzzSharp=False` cedes placement to a higher app-host distribution layer
- Reject: a system HarfBuzz dependency, hand-copying the dylib/so beside the RID-asset graph, or documenting any native asset as a public managed type — managed shaping API facts stay in `api-skia-harfbuzz.md`
