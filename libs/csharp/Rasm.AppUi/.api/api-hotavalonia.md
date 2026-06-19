# [RASM_APPUI_API_HOTAVALONIA]

`HotAvalonia` is a build-asset package with no runtime `lib/` surface: MSBuild props/targets gate XAML hot reload to Debug, an MSBuild task assembly performs injections, and `HotAvalonia.Extensions` injects the `AvaloniaHotReloadExtensions` source into startup projects.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HotAvalonia`
- package: `HotAvalonia`
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: build props/targets
- asset: MSBuild task assembly
- asset: HARFS remote tool
- rail: hot-reload

## [02]-[PACKAGE_ASSETS]

[BUILD_ASSETS]: package payload
- rail: hot-reload

| [INDEX] | [ASSET]                                | [RAIL]                    |
| :-----: | :------------------------------------- | :------------------------ |
|  [01]   | `build/HotAvalonia.props`              | capability + version      |
|  [02]   | `build/HotAvalonia.targets`            | knob defaults + injection |
|  [03]   | `tasks/netstandard2.0/HotAvalonia.dll` | MSBuild task              |
|  [04]   | `tools/HotAvalonia.Remote.dll`         | HARFS file server         |

[DEPENDENCY_ASSETS]: dependency fan-out (all `include="All"`)
- rail: hot-reload

| [INDEX] | [ASSET]                       | [RAIL]                               |
| :-----: | :---------------------------- | :----------------------------------- |
|  [01]   | `HotAvalonia.Core`            | runtime lib (`HotAvalonia.Core.dll`) |
|  [02]   | `HotAvalonia.Extensions`      | injected source (contentFiles)       |
|  [03]   | `HotAvalonia.Fody`            | IL weaver                            |
|  [04]   | `Avalonia.Markup.Xaml.Loader` | runtime XAML compile                 |

## [03]-[ENTRYPOINTS]

[KNOB_ENTRYPOINTS]: MSBuild gate and injection knobs
- rail: hot-reload
- surface-root: `HotAvalonia.targets`

| [INDEX] | [SURFACE]                                                       | [RAIL]                         |
| :-----: | :-------------------------------------------------------------- | :----------------------------- |
|  [01]   | `HotAvalonia`                                                   | master gate (Debug default)    |
|  [02]   | `HotAvaloniaRemote`                                             | remote reload (non-desktop)    |
|  [03]   | `HotAvaloniaLite`                                               | lite mode                      |
|  [04]   | `HotAvaloniaIncludeExtensions`                                  | source injection (exe default) |
|  [05]   | `HotAvaloniaInjections` / `HotAvaloniaInjectionType`            | injection mode                 |
|  [06]   | `HotAvaloniaInitialPatching`                                    | initial patch gate             |
|  [07]   | `HotAvaloniaTimeout` / `HotAvaloniaHotkey`                      | runtime behavior               |
|  [08]   | `HotAvaloniaAutoEnable`                                         | auto-enable (exe only)         |
|  [09]   | `HotAvaloniaRecompileResources`                                 | resource recompile             |
|  [10]   | `HotAvaloniaProcessReferences` / `HotAvaloniaExcludeReferences` | release strip                  |

[HARFS_ENTRYPOINTS]: remote file-server knobs
- rail: hot-reload
- surface-root: `HotAvalonia.targets`

| [INDEX] | [SURFACE]                                | [RAIL]          |
| :-----: | :--------------------------------------- | :-------------- |
|  [01]   | `HarfsAddress` / `HarfsFallbackAddress`  | endpoint        |
|  [02]   | `HarfsPort`                              | TCP port        |
|  [03]   | `HarfsSecret` / `HarfsCertificateFile`   | transport trust |
|  [04]   | `HarfsMaxSearchDepth`                    | read bound      |
|  [05]   | `HarfsTimeout` / `HarfsExitOnDisconnect` | lifetime        |

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

[BUILD_ASSET_LAW]:
- Package: `HotAvalonia`
- Owns: Debug-gated hot-reload wiring through MSBuild knobs and reference stripping
- Accept: the gate is `$(HotAvalonia)`; release builds strip `HotAvalonia.Core`/`Extensions`/`Fody` references
- Reject: hand-written hot-reload bootstrap beside the injected extensions

[API_BOUNDARY_LAW]:
- Package: `HotAvalonia`
- Owns: build orchestration only
- Accept: callable surface lives in injected `AvaloniaHotReloadExtensions` source and `HotAvalonia.Core`
- Reject: documenting the meta-package as a public managed type surface
