# [RASM_APPUI_API_HOTAVALONIA]

`HotAvalonia` is a build-asset meta-package with no `lib/` runtime surface: `build/HotAvalonia.props` advertises the `HotAvalonia` project capability and minimum Avalonia floor, `build/HotAvalonia.targets` defaults the MSBuild knobs and runs three MSBuild tasks that weave a `HotAvalonia.Fody` config and (optionally) launch the HARFS remote file server, and the `HotAvalonia.Extensions` dependency injects the `AvaloniaHotReloadExtensions` source into the startup project. The callable wiring lives in that injected source plus `HotAvalonia.Core`; the meta-package itself exposes no managed type. It gates entirely to Debug and self-strips on Release, so the unconditional `PrivateAssets="all"` reference keeps `packages.lock.json` configuration-independent.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HotAvalonia`
- package: `HotAvalonia`
- version: `3.1.2`
- license: `MIT` (file `LICENSE.md`)
- assembly: no managed runtime assembly (no `lib/`)
- min-avalonia: `11.0.0` (`HotAvaloniaMinimumSupportedAvaloniaVersion`); the workspace runs Avalonia `12.0.5` and overrides the transitive `Avalonia.Markup.Xaml.Loader` `11.0.0` pin to `12.0.5`
- capability: `<ProjectCapability Include="HotAvalonia" />` (props)
- asset: `build/HotAvalonia.props` (capability + version anchors)
- asset: `build/HotAvalonia.targets` (knob defaults + tasks)
- asset: `tasks/netstandard2.0/HotAvalonia.dll` (MSBuild task assembly)
- asset: `tools/HotAvalonia.Remote.dll` + `tools/HotAvalonia.Remote.runtimeconfig.json` (HARFS file server)
- rail: hot-reload

## [02]-[PACKAGE_ASSETS]

[BUILD_ASSETS]: package payload
- rail: hot-reload

| [INDEX] | [ASSET]                                | [RAIL]                          |
| :-----: | :------------------------------------- | :------------------------------ |
|  [01]   | `build/HotAvalonia.props`              | capability + version anchors    |
|  [02]   | `build/HotAvalonia.targets`            | knob defaults + Fody/HARFS tasks |
|  [03]   | `tasks/netstandard2.0/HotAvalonia.dll` | MSBuild task assembly (3 tasks) |
|  [04]   | `tools/HotAvalonia.Remote.dll`         | HARFS file server (out-of-proc) |

[DEPENDENCY_ASSETS]: dependency fan-out (all `include="All"`)
- rail: hot-reload

| [INDEX] | [ASSET]                              | [RAIL]                               |
| :-----: | :----------------------------------- | :----------------------------------- |
|  [01]   | `HotAvalonia.Core` (v`3.1.2`)        | runtime lib (`HotAvalonia.Core.dll`) — stripped on Release |
|  [02]   | `HotAvalonia.Extensions` (v`3.1.2`)  | injected source (contentFiles) — `AvaloniaHotReloadExtensions` |
|  [03]   | `HotAvalonia.Fody` (v`3.1.2`)        | IL weaver — stripped on Release      |
|  [04]   | `Avalonia.Markup.Xaml.Loader` (v`11.0.0`) | runtime XAML compile (`api-avalonia-xaml-loader`) — kept unless excluded |

[MSBUILD_TASKS]: tasks in `tasks/netstandard2.0/HotAvalonia.dll`
- rail: hot-reload

| [INDEX] | [TASK]                                              | [RAIL]                          |
| :-----: | :-------------------------------------------------- | :------------------------------ |
|  [01]   | `HotAvalonia.GenerateFileSystemServerConfigTask`    | emit HARFS config + Fody weaver config |
|  [02]   | `HotAvalonia.GetFileSystemClientConfigTask`         | resolve client address/secret into runtimeconfig |
|  [03]   | `HotAvalonia.StartFileSystemServerTask`             | launch `HotAvalonia.Remote.dll` after build |

## [03]-[ENTRYPOINTS]

[GATE_KNOBS]: master gate and mode knobs
- rail: hot-reload
- surface-root: `HotAvalonia.targets`

| [INDEX] | [SURFACE]                                                  | [RAIL]                              |
| :-----: | :--------------------------------------------------------- | :---------------------------------- |
|  [01]   | `HotAvalonia` (`enable`/`true`/`''`)                       | master gate (Debug default)         |
|  [02]   | `HotAvaloniaRemote`                                        | remote reload (non-desktop default) |
|  [03]   | `HotAvaloniaLite`                                          | lite mode (default = remote)        |
|  [04]   | `HotAvaloniaMode`                                          | runtime mode runtimeconfig          |
|  [05]   | `HotAvaloniaTimeout` / `HotAvaloniaHotkey`                 | runtime behavior runtimeconfig      |
|  [06]   | `HotAvaloniaMinLogLevel`                                   | runtime log level runtimeconfig     |

[INJECTION_KNOBS]: source-injection and patching knobs
- rail: hot-reload
- surface-root: `HotAvalonia.targets`

| [INDEX] | [SURFACE]                                                  | [RAIL]                              |
| :-----: | :--------------------------------------------------------- | :---------------------------------- |
|  [01]   | `HotAvaloniaIncludeExtensions`                             | source injection (exe default)      |
|  [02]   | `HotAvaloniaInjections` / `HotAvaloniaInjectionType`       | injection mode / runtimeconfig type |
|  [03]   | `HotAvaloniaInitialPatching` / `HotAvaloniaSkipInitialPatching` | initial patch gate            |
|  [04]   | `HotAvaloniaAutoEnable`                                    | auto-enable `UseHotReload` (exe only) |
|  [05]   | `HotAvaloniaGeneratePathResolver`                         | path resolver for auto-enable       |
|  [06]   | `HotAvaloniaRecompileResources`                           | resource recompile (`PopulateOverride`) |
|  [07]   | `HotAvaloniaProcessReferences` / `HotAvaloniaExcludeReferences` | referenced-project weave scope |
|  [08]   | `HotAvaloniaIncludeXamlLoader`                             | keep/strip `Avalonia.Markup.Xaml.Loader` |

[HARFS_KNOBS]: remote file-server knobs
- rail: hot-reload
- surface-root: `HotAvalonia.targets`

| [INDEX] | [SURFACE]                                       | [RAIL]                       |
| :-----: | :---------------------------------------------- | :--------------------------- |
|  [01]   | `HarfsAddress` / `HarfsFallbackAddress` / `HarfsLocalAddress` | client/listen endpoints |
|  [02]   | `HarfsPort` (`0` = random)                      | TCP port                     |
|  [03]   | `HarfsSecret` / `HarfsSecretBase64` / `HarfsCertificateFile` | transport trust  |
|  [04]   | `HarfsMaxSearchDepth` (`256`)                   | recursive-read file bound    |
|  [05]   | `HarfsTimeout` (`300000`) / `HarfsExitOnDisconnect` (`true`) | lifetime         |
|  [06]   | `HarfsConfigOutputPath` (`Avalonia\HotAvalonia.Remote.xml`) | intermediate config sink |

[COMPILER_CONSTANTS]: `DefineConstants` set by the gate
- rail: hot-reload
- surface-root: `HotAvalonia.targets`

| [INDEX] | [CONSTANT]                              | [RAIL]                         |
| :-----: | :-------------------------------------- | :----------------------------- |
|  [01]   | `HOTAVALONIA_ENABLE`                    | gate (`$(HotAvalonia)`)        |
|  [02]   | `HOTAVALONIA_USE_REMOTE_FILE_SYSTEM`    | remote reload                  |
|  [03]   | `HOTAVALONIA_ENABLE_LITE`               | lite mode                      |
|  [04]   | `HOTAVALONIA_EXCLUDE_EXTENSIONS`        | extensions not injected        |

[RUNTIME_OPTIONS]: `runtimeconfig.json` host options read by the injected runtime
- rail: hot-reload

| [INDEX] | [OPTION]                                | [RAIL]                         |
| :-----: | :-------------------------------------- | :----------------------------- |
|  [01]   | `HotAvalonia.InjectionType`             | injection strategy             |
|  [02]   | `HotAvalonia.SkipInitialPatching`       | initial-patch skip             |
|  [03]   | `HotAvalonia.Mode` / `HotAvalonia.MinLogLevel` | runtime mode + log level |
|  [04]   | `HotAvalonia.Timeout` / `HotAvalonia.Hotkey`   | timeout + trigger hotkey  |
|  [05]   | `HotAvalonia.RemoteFileSystemAddress` / `HotAvalonia.RemoteFileSystemSecret` | HARFS client |

[INJECTED_ENTRYPOINTS]: `AvaloniaHotReloadExtensions` injected source surface
- rail: hot-reload

| [INDEX] | [SURFACE]          | [SURFACE_ROOT]          | [RAIL]         |
| :-----: | :----------------- | :---------------------- | :------------- |
|  [01]   | `UseHotReload`     | `AppBuilder` extension  | builder wiring |
|  [02]   | `UseHotReload`     | `Application` extension | app wiring     |
|  [03]   | `EnableHotReload`  | `Application` extension | reload start   |
|  [04]   | `DisableHotReload` | `Application` extension | reload stop    |
|  [05]   | `TriggerHotReload` | `Application` extension | manual trigger |

## [04]-[IMPLEMENTATION_LAW]

[BUILD_FLOW_LAW]:
- The gate is `$(HotAvalonia)`: `true` when `Configuration == Debug` or explicitly `enable`. With the gate on, `DefineConstants` gains `HOTAVALONIA_ENABLE` (and `_USE_REMOTE_FILE_SYSTEM`/`_ENABLE_LITE`/`_EXCLUDE_EXTENSIONS` per knob), `CompileAvaloniaXaml` runs before the weave, and `HotAvaloniaGenerateFodyConfig` injects a `<HotAvalonia>` element into Fody's `WeaverConfiguration` so `HotAvalonia.Fody` patches the XAML-load methods.
- `HotAvaloniaGenerateRuntimeConfig` (when `GenerateRuntimeConfigurationFiles == true`) emits `RuntimeHostConfigurationOption` rows (`HotAvalonia.InjectionType`, `.SkipInitialPatching`, `.Timeout`, `.Mode`, `.Hotkey`, `.MinLogLevel`, `.RemoteFileSystemAddress`, `.RemoteFileSystemSecret`) into the app `runtimeconfig.json`; the injected runtime reads them at startup.
- Remote (HARFS) path: when `HotAvaloniaRemote` is on, `GenerateFileSystemServerConfigTask` writes the server config to `$(IntermediateOutputPath)\Avalonia\HotAvalonia.Remote.xml`, `GetFileSystemClientConfigTask` resolves the client address/secret, and `StartFileSystemServerTask` launches `tools/HotAvalonia.Remote.dll` after build to serve source files to a non-desktop (Android/iOS/Browser) target. iOS additionally force-enables the Mono interpreter and disables AOT.
- Reference stripping: with the gate on, `HotAvaloniaProcessReferences` defaults false (preserve), but `HotAvaloniaExcludeReferences` always lists `HotAvalonia;HotAvalonia.Core;HotAvalonia.Fody`, and adds `Avalonia.Markup.Xaml.Loader` only when `HotAvaloniaIncludeXamlLoader == false`. On Release the gate is off and the weave/strip removes the dev-loop assemblies, so no hot-reload code ships.

[STACKING_LAW]:
- Avalonia startup (`api-avalonia`, `api-avalonia-desktop`): the injected `UseHotReload` chains onto the `AppBuilder` returned by `AppBuilder.Configure<App>()` in the app entry point — it composes with the desktop lifetime, the Fluent theme, and the GPU/Skia render seam without any hand-written bootstrap. `AutoEnable` weaves the `UseHotReload` call in automatically when `HotAvaloniaAutoEnable` is set on an exe.
- XAML loader (`api-avalonia-xaml-loader`): `Avalonia.Markup.Xaml.Loader` is the runtime XAML-compile dependency `HotAvalonia.Core` drives to re-parse changed `.axaml`; the workspace overrides its transitive pin to `12.0.5` to match the Avalonia `12.0.x` core.
- The reference is `PrivateAssets="all"` and unconditional so the lockfile is configuration-independent; the package's own Debug gate — not an MSBuild `Condition` on the reference — decides whether any hot-reload asset is active.

[BUILD_ASSET_LAW]:
- Package: `HotAvalonia`
- Owns: Debug-gated XAML hot-reload wiring through MSBuild knobs, the three MSBuild tasks (Fody weave + HARFS launch), the runtimeconfig option set, and Release reference stripping
- Accept: the gate is `$(HotAvalonia)`; injection lands in the `AvaloniaHotReloadExtensions` source; the reference is unconditional `PrivateAssets="all"`; Release strips `HotAvalonia.Core`/`Extensions`/`Fody`
- Reject: a hand-written hot-reload bootstrap beside the injected extensions; a `Condition` on the package reference that breaks lockfile determinism; documenting the meta-package as a public managed type surface

[API_BOUNDARY_LAW]:
- Package: `HotAvalonia`
- Owns: build orchestration and weave only — no `lib/` managed surface
- Accept: callable surface lives in the injected `AvaloniaHotReloadExtensions` source and `HotAvalonia.Core`
- Reject: treating the meta-package as a referenced managed type; assuming a runtime assembly where only build/task/tool assets exist
