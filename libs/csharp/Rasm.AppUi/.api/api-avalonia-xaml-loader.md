# [RASM_APPUI_API_AVALONIA_XAML_LOADER]

`Avalonia.Markup.Xaml.Loader` supplies runtime XAML parsing through `AvaloniaRuntimeXamlLoader`, compiling XAML strings, streams, and loader documents at runtime (via the in-process `AvaloniaXamlIlRuntimeCompiler`) without the build-time `.axaml` -> IL precompilation. It is the dev-loop / hot-reload inflation engine anchored to `Diagnostics/devloop.md` (zero direct page composition — HotAvalonia's Debug-only runtime-inflation substrate, PrivateAssets dev-loop scoped), gated out of trimmed production builds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Markup.Xaml.Loader`
- package: `Avalonia.Markup.Xaml.Loader`
- license: `MIT`
- assembly: `Avalonia.Markup.Xaml.Loader`
- namespace: `Avalonia.Markup.Xaml` (one public entry; 417 types across 21 namespaces are internal IL-compiler infrastructure)
- asset: dev-loop library (`PrivateAssets="all"`)
- trim/AOT: every `AvaloniaRuntimeXamlLoader` entry carries `[RequiresUnreferencedCode]` for dynamic `x:Class` and XAML dependency references — the loader is trim/NativeAOT-incompatible by contract, which is precisely why it is private-asset and Debug-gated.
- rail: dev-loop

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime XAML loader and its document/config inputs — rail: dev-loop

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]   | [ROLE]                                                          |
| :-----: | :------------------------------- | :-------------- | :-------------------------------------------------------------- |
|  [01]   | `AvaloniaRuntimeXamlLoader`      | static loader   | runtime XAML parse/inflate entry; all members static            |
|  [02]   | `RuntimeXamlLoaderDocument`      | loader document | `(Uri? baseUri, object? rootInstance, Stream xaml)` input unit  |
|  [03]   | `RuntimeXamlLoaderConfiguration` | loader config   | `DesignMode` + `LocalAssembly` + `UseCompiledBindingsByDefault` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: XAML parse, load, and inflate — `localAssembly` resolves `clr-namespace:` and relative resource URIs; `rootInstance` inflates INTO an existing object (the `x:Class` root); `designMode` toggles design-time evaluation — rail: dev-loop

All entrypoints are static members of `AvaloniaRuntimeXamlLoader`.

| [INDEX] | [SURFACE]   | [CAPABILITY]             |
| :-----: | :---------- | :----------------------- |
|  [01]   | `Load`      | string load              |
|  [02]   | `Load`      | stream load              |
|  [03]   | `Load`      | configured document load |
|  [04]   | `LoadGroup` | multi-document compile   |
|  [05]   | `Parse`     | parse-only object        |
|  [06]   | `Parse<T>`  | typed parse              |

[LOAD_SIGNATURES]:
- String: `Load(string xaml, Assembly? localAssembly = null, object? rootInstance = null, Uri? uri = null, bool designMode = false) : object` carries `[StringSyntax("Xml")]` and delegates UTF-8 data to the stream path.
- Stream: `Load(Stream stream, Assembly? localAssembly = null, object? rootInstance = null, Uri? uri = null, bool designMode = false) : object` is the canonical resource-stream entry.
- Document: `Load(RuntimeXamlLoaderDocument document, RuntimeXamlLoaderConfiguration? configuration = null) : object` controls design mode and compiled bindings explicitly.
- Group: `LoadGroup(IReadOnlyCollection<RuntimeXamlLoaderDocument> documents, RuntimeXamlLoaderConfiguration? configuration = null) : IReadOnlyList<object?>` returns `null` for each removed document.
- Parse: `Parse(string xaml, Assembly? localAssembly = null) : object` omits root-instance and design-mode inputs; `Parse<T>` has the same parameters and constrains `T` through `[DynamicallyAccessedMembers(All)]`.

## [04]-[IMPLEMENTATION_LAW]

[XAML_LOADER_TOPOLOGY]:
- One public surface (`AvaloniaRuntimeXamlLoader`) in `Avalonia.Markup.Xaml`; the remaining 21 internal namespaces are the `XamlIl` runtime compiler and Cecil-backed emit infrastructure, not consumer API.
- `string` `Load`/`Parse` overloads decode UTF-8 into a `MemoryStream` and delegate to the stream path; the stream path builds a `RuntimeXamlLoaderDocument` and a `RuntimeXamlLoaderConfiguration`, then calls `AvaloniaXamlIlRuntimeCompiler.Load`. `LoadGroup` shares one compile context across documents.
- `rootInstance` is the existing-object inflate channel (the catalog has no separate `Load(object,...)` overload — pass the live `x:Class` instance as `rootInstance`). `Parse<T>` is the typed projection of `Parse`.

[LOCAL_ADMISSION]:
- `AvaloniaRuntimeXamlLoader.Load` is the sole programmatic entry for runtime XAML inflation; `HotAvalonia` drives this surface (via `Load(Stream, localAssembly, rootInstance)`) during the hot-reload loop to re-inflate changed views into their live instances.
- Pass the calling/owning assembly as `localAssembly` so `clr-namespace:` and relative `avares://` resource URIs resolve against the project that authored the XAML.
- `Load(document, configuration)` is the integration entry when the hot-reload host needs explicit `DesignMode`/`UseCompiledBindingsByDefault` control; the bare string/stream overloads default both off.
- Production release builds do not load XAML at runtime; the `[RequiresUnreferencedCode]` contract means a trimmed or NativeAOT publish cannot rely on this loader. Keep every call site behind a Debug / hot-reload gate.

[RAIL_LAW]:
- Package: `Avalonia.Markup.Xaml.Loader`
- Owns: runtime XAML parse, batch parse, and inflate for dev-loop and hot-reload scenarios
- Accept: XAML load from string, stream, or `RuntimeXamlLoaderDocument` via `AvaloniaRuntimeXamlLoader`, with `localAssembly` for resource/namespace resolution
- Reject: referencing this package in trimmed/AOT production runtime paths; keep usage gated to Debug and hot-reload
