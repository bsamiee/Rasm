# [RASM_APPUI_API_AVALONIA_XAML_LOADER]

`AvaloniaRuntimeXamlLoader` inflates XAML strings, streams, and loader documents at runtime through the in-process `AvaloniaXamlIlRuntimeCompiler`, the one managed entry that skips build-time `.axaml` precompilation. It feeds the hot-reload rail, where `HotAvalonia.Core` re-parses changed views into their live instances.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Markup.Xaml.Loader`
- package: `Avalonia.Markup.Xaml.Loader` (MIT)
- assembly: `Avalonia.Markup.Xaml.Loader`
- namespace: `Avalonia.Markup.Xaml`
- asset: dev-loop library (`PrivateAssets="all"`)
- rail: dev-loop

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime loader and its document/config inputs

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :------------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `AvaloniaRuntimeXamlLoader`      | static class  | runtime XAML parse and inflate entry                   |
|  [02]   | `RuntimeXamlLoaderDocument`      | class         | input unit `(Uri, object?, Stream)`                    |
|  [03]   | `RuntimeXamlLoaderConfiguration` | class         | `DesignMode`, `LocalAssembly`, compiled-binding toggle |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: static parse, load, and inflate members of `AvaloniaRuntimeXamlLoader`; `localAssembly` resolves `clr-namespace:` and `avares://` URIs, `rootInstance` inflates into the live `x:Class` object, `designMode` gates design-time evaluation.

| [INDEX] | [SURFACE]                                                                                    | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------------------------- | :------------------------------- |
|  [01]   | `Load(string, Assembly?, object?, Uri?, bool)`                                               | inflate a XAML string            |
|  [02]   | `Load(Stream, Assembly?, object?, Uri?, bool)`                                               | inflate a resource stream        |
|  [03]   | `Load(RuntimeXamlLoaderDocument, RuntimeXamlLoaderConfiguration?)`                           | inflate with explicit config     |
|  [04]   | `LoadGroup(IReadOnlyCollection<RuntimeXamlLoaderDocument>, RuntimeXamlLoaderConfiguration?)` | compile documents in one context |
|  [05]   | `Parse(string, Assembly?)`                                                                   | parse-only object                |
|  [06]   | `Parse<T>(string, Assembly?)`                                                                | typed projection of `Parse`      |

- `LoadGroup` returns `IReadOnlyList<object?>`, `null` at each removed document's index.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One public surface `AvaloniaRuntimeXamlLoader` in `Avalonia.Markup.Xaml`; the remaining namespaces hold the `XamlX`/`XamlIl` runtime compiler emitting IL through `System.Reflection.Emit`, internal infrastructure no consumer calls.
- `string` overloads decode UTF-8 into a `MemoryStream` and delegate to the stream path, which builds a `RuntimeXamlLoaderDocument` and `RuntimeXamlLoaderConfiguration` and calls `AvaloniaXamlIlRuntimeCompiler.Load`; `LoadGroup` shares one compile context across documents.
- `rootInstance` is the existing-object inflate channel with no separate `Load(object, ...)` overload, and `Parse<T>` is the typed projection of `Parse`.

[STACKING]:
- `api-hotavalonia`(`.api/api-hotavalonia.md`): `HotAvalonia.Core` drives `Load(Stream, localAssembly, rootInstance)` across the hot-reload loop to re-inflate changed `.axaml` into live instances, and `HotAvaloniaIncludeXamlLoader` keeps this package in the dev-loop weave.
- within-lib: the bare string and stream overloads default `DesignMode` and `UseCompiledBindingsByDefault` off; `Load(document, configuration)` is the composition entry when the host threads explicit config.

[LOCAL_ADMISSION]:
- Pass the calling assembly as `localAssembly` so `clr-namespace:` and `avares://` resource URIs resolve against the authoring project.
- Gate every call site to Debug and hot-reload; every member carries `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`, so this loader binds no trimmed or NativeAOT publish.

[RAIL_LAW]:
- Package: `Avalonia.Markup.Xaml.Loader`
- Owns: runtime XAML parse, batch parse, and inflate for the dev-loop and hot-reload rail
- Accept: string, stream, or `RuntimeXamlLoaderDocument` load through `AvaloniaRuntimeXamlLoader` with `localAssembly` resolution
- Reject: this package on a trimmed or NativeAOT production path; a hand-written runtime XAML parser
