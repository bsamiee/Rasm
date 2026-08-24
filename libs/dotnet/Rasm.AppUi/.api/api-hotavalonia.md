# [RASM_APPUI_API_HOTAVALONIA]

`HotAvalonia` is a build-asset meta-package with no `lib/` or managed surface: `build/HotAvalonia.props` advertises the `HotAvalonia` project capability and the Avalonia floor, `build/HotAvalonia.targets` defaults the MSBuild knobs and runs the three tasks that weave `HotAvalonia.Fody` and launch the HARFS server, and `HotAvalonia.Extensions` injects `AvaloniaHotReloadExtensions`, the callable wiring beside `HotAvalonia.Core`. A Debug gate binds every asset and self-strips on Release, so an unconditional `PrivateAssets="all"` reference keeps `packages.lock.json` configuration-independent.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HotAvalonia`
- package: `HotAvalonia` (MIT)
- assembly: none (no `lib/`; build, task, and tool assets only)
- avalonia-floor: `HotAvaloniaMinimumSupportedAvaloniaVersion` (props)
- rail: hot-reload

## [02]-[PACKAGE_ASSETS]

[BUILD_ASSETS]: package payload

| [INDEX] | [ASSET]                                | [CAPABILITY]                     |
| :-----: | :------------------------------------- | :------------------------------- |
|  [01]   | `build/HotAvalonia.props`              | capability + Avalonia floor      |
|  [02]   | `build/HotAvalonia.targets`            | knob defaults + Fody/HARFS tasks |
|  [03]   | `tasks/netstandard2.0/HotAvalonia.dll` | MSBuild task assembly (3 tasks)  |
|  [04]   | `tools/HotAvalonia.Remote.dll`         | HARFS file server (out-of-proc)  |

[DEPENDENCY_ASSETS]: dependency fan-out (all `include="All"`)

| [INDEX] | [ASSET]                       | [CAPABILITY]                                           |
| :-----: | :---------------------------- | :----------------------------------------------------- |
|  [01]   | `HotAvalonia.Core`            | runtime lib (`HotAvalonia.Core.dll`), stripped Release |
|  [02]   | `HotAvalonia.Extensions`      | injected `AvaloniaHotReloadExtensions` source          |
|  [03]   | `HotAvalonia.Fody`            | IL weaver, stripped Release                            |
|  [04]   | `Avalonia.Markup.Xaml.Loader` | runtime XAML compile, kept unless excluded             |

[MSBUILD_TASKS]: tasks in `tasks/netstandard2.0/HotAvalonia.dll`

| [INDEX] | [TASK]                                           | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------- | :----------------------------------------------- |
|  [01]   | `HotAvalonia.GenerateFileSystemServerConfigTask` | emit HARFS server config                         |
|  [02]   | `HotAvalonia.GetFileSystemClientConfigTask`      | resolve client address/secret into runtimeconfig |
|  [03]   | `HotAvalonia.StartFileSystemServerTask`          | launch `HotAvalonia.Remote.dll` after build      |

## [03]-[ENTRYPOINTS]

MSBuild knobs default in `HotAvalonia.targets`; runtime options land in `runtimeconfig.json`; the injected `AvaloniaHotReloadExtensions` source carries the callable wiring.

[GATE_KNOBS]: master gate and mode knobs

| [INDEX] | [SURFACE]                                  | [CAPABILITY]                        |
| :-----: | :----------------------------------------- | :---------------------------------- |
|  [01]   | `HotAvalonia` (`enable`/`true`/`''`)       | master gate (Debug default)         |
|  [02]   | `HotAvaloniaRemote`                        | remote reload (non-desktop default) |
|  [03]   | `HotAvaloniaLite`                          | lite mode (default = remote)        |
|  [04]   | `HotAvaloniaMode`                          | runtime mode runtimeconfig          |
|  [05]   | `HotAvaloniaTimeout` / `HotAvaloniaHotkey` | runtime behavior runtimeconfig      |
|  [06]   | `HotAvaloniaMinLogLevel`                   | runtime log level runtimeconfig     |

[INJECTION_KNOBS]: source-injection and patching knobs

| [INDEX] | [SURFACE]                                                       | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `HotAvaloniaIncludeExtensions`                                  | source injection (exe default)           |
|  [02]   | `HotAvaloniaInjections` / `HotAvaloniaInjectionType`            | injection mode / runtimeconfig type      |
|  [03]   | `HotAvaloniaInitialPatching` / `HotAvaloniaSkipInitialPatching` | initial patch gate                       |
|  [04]   | `HotAvaloniaAutoEnable`                                         | auto-enable `UseHotReload` (exe only)    |
|  [05]   | `HotAvaloniaGeneratePathResolver`                               | path resolver for auto-enable            |
|  [06]   | `HotAvaloniaRecompileResources`                                 | resource recompile (`PopulateOverride`)  |
|  [07]   | `HotAvaloniaProcessReferences` / `HotAvaloniaExcludeReferences` | referenced-project weave scope           |
|  [08]   | `HotAvaloniaIncludeXamlLoader`                                  | keep/strip `Avalonia.Markup.Xaml.Loader` |

[HARFS_KNOBS]: remote file-server knobs

| [INDEX] | [SURFACE]                                                     | [CAPABILITY]              |
| :-----: | :------------------------------------------------------------ | :------------------------ |
|  [01]   | `HarfsAddress` / `HarfsFallbackAddress` / `HarfsLocalAddress` | client/listen endpoints   |
|  [02]   | `HarfsPort` (`0` = random)                                    | TCP port                  |
|  [03]   | `HarfsSecret` / `HarfsSecretBase64` / `HarfsCertificateFile`  | transport trust           |
|  [04]   | `HarfsMaxSearchDepth` (`256`)                                 | recursive-read file bound |
|  [05]   | `HarfsTimeout` (`300000`) / `HarfsExitOnDisconnect` (`true`)  | lifetime                  |
|  [06]   | `HarfsConfigOutputPath` (`Avalonia\HotAvalonia.Remote.xml`)   | intermediate config sink  |

[COMPILER_CONSTANTS]: `DefineConstants` set by the gate

| [INDEX] | [CONSTANT]                           | [CAPABILITY]            |
| :-----: | :----------------------------------- | :---------------------- |
|  [01]   | `HOTAVALONIA_ENABLE`                 | gate (`$(HotAvalonia)`) |
|  [02]   | `HOTAVALONIA_USE_REMOTE_FILE_SYSTEM` | remote reload           |
|  [03]   | `HOTAVALONIA_ENABLE_LITE`            | lite mode               |
|  [04]   | `HOTAVALONIA_EXCLUDE_EXTENSIONS`     | extensions not injected |

[RUNTIME_OPTIONS]: `runtimeconfig.json` host options read by the injected runtime

| [INDEX] | [OPTION]                                                                     | [CAPABILITY]             |
| :-----: | :--------------------------------------------------------------------------- | :----------------------- |
|  [01]   | `HotAvalonia.InjectionType`                                                  | injection strategy       |
|  [02]   | `HotAvalonia.SkipInitialPatching`                                            | initial-patch skip       |
|  [03]   | `HotAvalonia.Mode` / `HotAvalonia.MinLogLevel`                               | runtime mode + log level |
|  [04]   | `HotAvalonia.Timeout` / `HotAvalonia.Hotkey`                                 | timeout + trigger hotkey |
|  [05]   | `HotAvalonia.RemoteFileSystemAddress` / `HotAvalonia.RemoteFileSystemSecret` | HARFS client             |

[INJECTED_ENTRYPOINTS]: `AvaloniaHotReloadExtensions` injected source surface

| [INDEX] | [SURFACE]          | [SHAPE]                 | [CAPABILITY]   |
| :-----: | :----------------- | :---------------------- | :------------- |
|  [01]   | `UseHotReload`     | `AppBuilder` extension  | builder wiring |
|  [02]   | `UseHotReload`     | `Application` extension | app wiring     |
|  [03]   | `EnableHotReload`  | `Application` extension | reload start   |
|  [04]   | `DisableHotReload` | `Application` extension | reload stop    |
|  [05]   | `TriggerHotReload` | `Application` extension | manual trigger |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Gate `$(HotAvalonia)` is `true` under `Configuration == Debug` or explicit `enable`; with it on, `DefineConstants` gains `HOTAVALONIA_ENABLE` (with `_USE_REMOTE_FILE_SYSTEM`/`_ENABLE_LITE`/`_EXCLUDE_EXTENSIONS` per knob), `CompileAvaloniaXaml` runs before the weave, and `HotAvaloniaGenerateFodyConfig` injects a `<HotAvalonia>` element into Fody's `WeaverConfiguration` so `HotAvalonia.Fody` patches the XAML-load methods.
- `HotAvaloniaGenerateRuntimeConfig` (under `GenerateRuntimeConfigurationFiles == true`) emits the `RuntimeHostConfigurationOption` rows the injected runtime reads at startup.
- Remote HARFS path (`HotAvaloniaRemote` on): `GenerateFileSystemServerConfigTask` writes the server config to `$(IntermediateOutputPath)\Avalonia\HotAvalonia.Remote.xml`, `GetFileSystemClientConfigTask` resolves the client address and secret, and `StartFileSystemServerTask` launches `tools/HotAvalonia.Remote.dll` after build to serve source to a non-desktop Android, iOS, or Browser target; iOS also forces the Mono interpreter and disables AOT.
- Reference stripping: `HotAvaloniaProcessReferences` defaults false (preserve), `HotAvaloniaExcludeReferences` always lists `HotAvalonia;HotAvalonia.Core;HotAvalonia.Fody` and adds `Avalonia.Markup.Xaml.Loader` only under `HotAvaloniaIncludeXamlLoader == false`; Release turns the gate off and strips every dev-loop assembly, so no hot-reload code ships.

[STACKING]:
- `api-avalonia`(`.api/api-avalonia.md`), `api-avalonia-desktop`(`.api/api-avalonia-desktop.md`): the injected `UseHotReload` chains onto the `AppBuilder` from `AppBuilder.Configure<App>()`, composing with the desktop lifetime, Fluent theme, and GPU/Skia render seam with no hand-written bootstrap; `HotAvaloniaAutoEnable` weaves the call in on an exe.
- `api-avalonia-xaml-loader`(`.api/api-avalonia-xaml-loader.md`): `Avalonia.Markup.Xaml.Loader` is the runtime XAML-compile dependency `HotAvalonia.Core` drives to re-parse changed `.axaml`.

[LOCAL_ADMISSION]:
- Reference `HotAvalonia` unconditionally with `PrivateAssets="all"`; the package's Debug gate, not an MSBuild `Condition` on the reference, decides whether any hot-reload asset activates, so `packages.lock.json` stays configuration-independent.
- Hot-reload wiring lands through the injected `AvaloniaHotReloadExtensions` source and `HotAvalonia.Core`, never a hand-written bootstrap.

[RAIL_LAW]:
- Package: `HotAvalonia`
- Owns: Debug-gated XAML hot-reload — the MSBuild knob set, the three tasks (Fody weave and HARFS launch), the runtimeconfig option set, and Release reference stripping; no `lib/` managed surface.
- Accept: the `$(HotAvalonia)` gate; injection into the `AvaloniaHotReloadExtensions` source and `HotAvalonia.Core`; an unconditional `PrivateAssets="all"` reference.
- Reject: a hand-written hot-reload bootstrap; a `Condition` on the reference; treating the meta-package as a referenced managed type.
