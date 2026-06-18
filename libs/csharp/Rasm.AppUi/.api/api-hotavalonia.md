# [RASM_APPUI_API_HOTAVALONIA]

`HotAvalonia` is a build-asset package with no runtime `lib/` surface: MSBuild props/targets gate XAML hot reload to Debug, an MSBuild task assembly performs injections, and `HotAvalonia.Extensions` injects the `AvaloniaHotReloadExtensions` source into startup projects.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HotAvalonia`
- package: `HotAvalonia`
- assembly: no managed runtime assembly
- namespace: no managed namespace
- asset: build props/targets
- asset: MSBuild task assembly
- asset: HARFS remote tool
- rail: hot-reload

## [2]-[PACKAGE_ASSETS]

[BUILD_ASSETS]: package payload
- rail: hot-reload

| [INDEX] | [ASSET]                                | [RAIL]                    |
| :-----: | :------------------------------------- | :------------------------ |
|   [1]   | `build/HotAvalonia.props`              | capability + version      |
|   [2]   | `build/HotAvalonia.targets`            | knob defaults + injection |
|   [3]   | `tasks/netstandard2.0/HotAvalonia.dll` | MSBuild task              |
|   [4]   | `tools/HotAvalonia.Remote.dll`         | HARFS file server         |

[DEPENDENCY_ASSETS]: dependency fan-out (all `include="All"`)
- rail: hot-reload

| [INDEX] | [ASSET]                       | [RAIL]                               |
| :-----: | :---------------------------- | :----------------------------------- |
|   [1]   | `HotAvalonia.Core`            | runtime lib (`HotAvalonia.Core.dll`) |
|   [2]   | `HotAvalonia.Extensions`      | injected source (contentFiles)       |
|   [3]   | `HotAvalonia.Fody`            | IL weaver                            |
|   [4]   | `Avalonia.Markup.Xaml.Loader` | runtime XAML compile                 |

## [3]-[ENTRYPOINTS]

[KNOB_ENTRYPOINTS]: MSBuild gate and injection knobs
- rail: hot-reload
- surface-root: `HotAvalonia.targets`

| [INDEX] | [SURFACE]                                                       | [RAIL]                         |
| :-----: | :-------------------------------------------------------------- | :----------------------------- |
|   [1]   | `HotAvalonia`                                                   | master gate (Debug default)    |
|   [2]   | `HotAvaloniaRemote`                                             | remote reload (non-desktop)    |
|   [3]   | `HotAvaloniaLite`                                               | lite mode                      |
|   [4]   | `HotAvaloniaIncludeExtensions`                                  | source injection (exe default) |
|   [5]   | `HotAvaloniaInjections` / `HotAvaloniaInjectionType`            | injection mode                 |
|   [6]   | `HotAvaloniaInitialPatching`                                    | initial patch gate             |
|   [7]   | `HotAvaloniaTimeout` / `HotAvaloniaHotkey`                      | runtime behavior               |
|   [8]   | `HotAvaloniaAutoEnable`                                         | auto-enable (exe only)         |
|   [9]   | `HotAvaloniaRecompileResources`                                 | resource recompile             |
|  [10]   | `HotAvaloniaProcessReferences` / `HotAvaloniaExcludeReferences` | release strip                  |

[HARFS_ENTRYPOINTS]: remote file-server knobs
- rail: hot-reload
- surface-root: `HotAvalonia.targets`

| [INDEX] | [SURFACE]                                | [RAIL]          |
| :-----: | :--------------------------------------- | :-------------- |
|   [1]   | `HarfsAddress` / `HarfsFallbackAddress`  | endpoint        |
|   [2]   | `HarfsPort`                              | TCP port        |
|   [3]   | `HarfsSecret` / `HarfsCertificateFile`   | transport trust |
|   [4]   | `HarfsMaxSearchDepth`                    | read bound      |
|   [5]   | `HarfsTimeout` / `HarfsExitOnDisconnect` | lifetime        |

[INJECTED_ENTRYPOINTS]: `AvaloniaHotReloadExtensions` injected source surface
- rail: hot-reload

| [INDEX] | [SURFACE]          | [SURFACE_ROOT]          | [RAIL]         |
| :-----: | :----------------- | :---------------------- | :------------- |
|   [1]   | `UseHotReload`     | `AppBuilder` extension  | builder wiring |
|   [2]   | `UseHotReload`     | `Application` extension | app wiring     |
|   [3]   | `EnableHotReload`  | `Application` extension | reload start   |
|   [4]   | `DisableHotReload` | `Application` extension | reload stop    |
|   [5]   | `TriggerHotReload` | `Application` extension | manual trigger |

## [4]-[IMPLEMENTATION_LAW]

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
