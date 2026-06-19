# [RASM_APPUI_API_AVALONIA_XAML_LOADER]

`Avalonia.Markup.Xaml.Loader` supplies runtime XAML parsing through `AvaloniaRuntimeXamlLoader`, enabling hot-reload and dev-loop loading of XAML from strings, streams, and assembly resources without ahead-of-time compilation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Markup.Xaml.Loader`
- package: `Avalonia.Markup.Xaml.Loader`
- assembly: `Avalonia.Markup.Xaml.Loader`
- namespace: `Avalonia.Markup.Xaml`
- asset: dev-loop library (`PrivateAssets="all"`)
- rail: dev-loop

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime XAML loader
- rail: dev-loop

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                   |
| :-----: | :-------------------------- | :------------ | :----------------------- |
|  [01]   | `AvaloniaRuntimeXamlLoader` | static loader | runtime XAML parse entry |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: XAML parse and load operations
- rail: dev-loop

| [INDEX] | [SURFACE]                             | [SURFACE_ROOT]              | [RAIL]                  |
| :-----: | :------------------------------------ | :-------------------------- | :---------------------- |
|  [01]   | `Load(string, Assembly?, Uri?, bool)` | `AvaloniaRuntimeXamlLoader` | string XAML parse       |
|  [02]   | `Load(Stream, Assembly?, Uri?, bool)` | `AvaloniaRuntimeXamlLoader` | stream XAML parse       |
|  [03]   | `Load(object, Assembly?, Uri?, bool)` | `AvaloniaRuntimeXamlLoader` | existing-object inflate |

## [04]-[IMPLEMENTATION_LAW]

[XAML_LOADER_TOPOLOGY]:
- namespace: `Avalonia.Markup.Xaml`; 417 types across 21 namespaces in the assembly, one public entry in the declared namespace
- `AvaloniaRuntimeXamlLoader` is a static class; all entry points are static methods
- The package is admitted with `PrivateAssets="all"` — it is a dev-loop and hot-reload dependency, not a production runtime dependency

[LOCAL_ADMISSION]:
- `AvaloniaRuntimeXamlLoader.Load` is the sole programmatic entry for runtime XAML inflation; `HotAvalonia` drives this surface during the hot-reload loop.
- Production release builds do not load XAML at runtime; only debug and hot-reload paths invoke this loader.
- Pass the calling assembly for relative resource URI resolution when loading from embedded resources.

[RAIL_LAW]:
- Package: `Avalonia.Markup.Xaml.Loader`
- Owns: runtime XAML parse and inflate for dev-loop and hot-reload scenarios
- Accept: XAML load from string or stream via `AvaloniaRuntimeXamlLoader`
- Reject: referencing this package in production runtime code; keep its usage gated to dev-loop paths
