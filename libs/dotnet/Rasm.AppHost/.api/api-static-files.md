# [RASM_APPHOST_API_STATIC_FILES]

`Microsoft.AspNetCore.App` serves a static asset tree two ways, and provenance is the whole discriminant: `MapStaticAssets` resolves a manifest the .NET BUILD emitted, `UseStaticFiles` serves whatever an `IFileProvider` resolves at RUNTIME. AppHost's co-hosted bundle is a TypeScript build product the host selects per profile row, so provenance decides the owner rather than taste.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: middleware options family

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]  | [CAPABILITY]                         |
| :-----: | :--------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `StaticFileOptions`                | options value  | runtime-provider serving policy      |
|  [02]   | `SharedOptionsBase`                | options base   | `RequestPath` + `FileProvider` pair  |
|  [03]   | `FileServerOptions`                | options value  | composite files/default/browse       |
|  [04]   | `DefaultFilesOptions`              | options value  | directory index rewrite              |
|  [05]   | `DirectoryBrowserOptions`          | options value  | directory listing                    |
|  [06]   | `StaticFileResponseContext`        | callback value | per-response header seat             |
|  [07]   | `IContentTypeProvider`             | contract       | extension-to-content-type resolution |
|  [08]   | `FileExtensionContentTypeProvider` | provider value | mutable extension map                |

[PUBLIC_TYPE_SCOPE]: build-manifest and file-provider family

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY]      | [CAPABILITY]                    |
| :-----: | :------------------------------------------- | :----------------- | :------------------------------ |
|  [01]   | `StaticAssetsEndpointRouteBuilderExtensions` | class              | build-manifest endpoint mapping |
|  [02]   | `StaticAssetsEndpointConventionBuilder`      | convention builder | conventions over mapped assets  |
|  [03]   | `StaticFilesEndpointRouteBuilderExtensions`  | class              | SPA deep-link fallback mapping  |
|  [04]   | `IFileProvider`                              | contract           | the runtime asset roster        |
|  [05]   | `PhysicalFileProvider`                       | provider value     | a directory root on disk        |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration — the two owners, by asset provenance

| [INDEX] | [SURFACE]                                                                      | [SHAPE] | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `UseStaticFiles(IApplicationBuilder, StaticFileOptions)`                       | static  | serve a RUNTIME-resolved provider |
|  [02]   | `UseStaticFiles(IApplicationBuilder, string requestPath)`                      | static  | serve the web root under a prefix |
|  [03]   | `UseStaticFiles(IApplicationBuilder)`                                          | static  | serve the web root                |
|  [04]   | `MapStaticAssets(IEndpointRouteBuilder, string? staticAssetsManifestPath)`     | static  | serve a BUILD-emitted manifest    |
|  [05]   | `MapFallbackToFile(IEndpointRouteBuilder, string pattern, string filePath)`    | static  | route deep links to the SPA index |
|  [06]   | `MapFallbackToFile(IEndpointRouteBuilder, string filePath, StaticFileOptions)` | static  | fallback under explicit options   |
|  [07]   | `UseFileServer(IApplicationBuilder, FileServerOptions)`                        | static  | static, default files, browsing   |
|  [08]   | `UseDefaultFiles(IApplicationBuilder, DefaultFilesOptions)`                    | static  | rewrite a directory to its index  |
|  [09]   | `UseDirectoryBrowser(IApplicationBuilder, DirectoryBrowserOptions)`            | static  | list a directory                  |

- Every `Use*` row returns `IApplicationBuilder` for chaining; `MapStaticAssets` returns `StaticAssetsEndpointConventionBuilder` and each `MapFallbackToFile` an `IEndpointConventionBuilder`.
- `MapFallbackToFile` also carries a bare `(filePath)` and a `(pattern, filePath, StaticFileOptions)` form; the implicit pattern is `{*path:nonfile}` and the endpoint order is `int.MaxValue`, so it never shadows a real route or a static asset.

[ENTRYPOINT_SCOPE]: serving policy

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `SharedOptionsBase.FileProvider -> IFileProvider?`                   | property | the resolved asset root              |
|  [02]   | `SharedOptionsBase.RequestPath -> PathString`                        | property | the URL prefix the root mounts under |
|  [03]   | `SharedOptionsBase.RedirectToAppendTrailingSlash`                    | property | directory-URL normalization          |
|  [04]   | `StaticFileOptions.ContentTypeProvider`                              | property | extension-to-content-type map        |
|  [05]   | `StaticFileOptions.ServeUnknownFileTypes -> bool`                    | property | admit unmapped extensions            |
|  [06]   | `StaticFileOptions.DefaultContentType -> string?`                    | property | fallback content type                |
|  [07]   | `StaticFileOptions.OnPrepareResponse`                                | property | per-response header seat             |
|  [08]   | `StaticFileOptions.OnPrepareResponseAsync`                           | property | async per-response header seat       |
|  [09]   | `StaticFileOptions.HttpsCompression`                                 | property | compression mode over TLS            |
|  [10]   | `PhysicalFileProvider(string root)`                                  | ctor     | bind a directory as the asset roster |
|  [11]   | `FileExtensionContentTypeProvider.Mappings`                          | property | mutable extension map                |
|  [12]   | `StaticAssetsEndpointConventionBuilder.Add(Action<EndpointBuilder>)` | instance | convention over mapped assets        |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `UseStaticFiles` is MIDDLEWARE and serves from `IWebHostEnvironment.WebRootPath`/`WebRootFileProvider` — the `wwwroot` subfolder by default — or from whatever `FileProvider` the options carry. Its roster is resolved on every request from the provider, so a tree produced by any toolchain and selected at boot serves without the .NET build knowing it exists.
- `MapStaticAssets` is ENDPOINT ROUTING and resolves a build-emitted manifest, located by `IHostEnvironment.ApplicationName` or by an explicit path resolved against `AppContext.BaseDirectory`. Its manifest carries per-asset content hashes, precompressed gzip/brotli variants, and immutable cache headers — none of which a runtime provider can supply.
- `MapFallbackToFile` is not a third serving owner: it routes a NON-file deep link to one already-served index document, so a client-side-routed bundle needs it beside `UseStaticFiles` or every refreshed deep link answers 404 while the assets themselves serve fine. Its `{*path:nonfile}` constraint and `int.MaxValue` order are what keep it from shadowing the asset routes.
- Manifest membership is the foreclosure. Assets appear only as static web assets of the .NET build; directories dropped beside the publish output stay absent and answer 404, which puts every runtime-SELECTED bundle root structurally outside any build manifest.
- `UseFileServer` composes static files, default files, and directory browsing behind one options value; the last two are separate opt-ins and neither belongs on a control-plane asset root.

[STACKING]:
- `api-hosting.md`(`Microsoft.Extensions.Hosting`): the web app root builds through the ASP.NET host, so both registrations reach the same `IApplicationBuilder`/`IEndpointRouteBuilder` the gRPC and control endpoint mappings already use; `UseStaticFiles` orders in the middleware pipeline ahead of endpoint routing, while `MapStaticAssets` orders among the endpoint mappings.
- within-lib: `Runtime/profiles.md` `[02]-[PROFILE_AXIS]` gates on `HostCapability.CoHostedAssets` in `HostDescriptor.Held`, and `UseStaticFiles(StaticFileOptions)` owns the serving — the capability selects a TypeScript bundle at runtime, which `MapStaticAssets` cannot serve — with `FileProvider` a `PhysicalFileProvider` over the selected bundle root and `RequestPath` its mount prefix; `Wire/companion.md` seats the middleware ahead of endpoint routing.
- `MapStaticAssets` stands admitted for any asset the .NET build itself emits and is the better owner there.

[LOCAL_ADMISSION]:
- Provenance decides the registration: a build-emitted asset takes `MapStaticAssets`, a runtime-selected tree takes `UseStaticFiles(StaticFileOptions)`. Choosing on convenience is the defect this row forecloses.
- Any `CoHostedAssets` row whose selected root resolves no readable directory refuses at boot; per-request 404s are the deleted diagnosis.
- `ServeUnknownFileTypes` stays off and `DefaultContentType` unset on a control-plane root: an unmapped extension is a bundle defect, not a byte stream to guess at.
- Directory browsing and default-document rewriting stay unregistered — a control plane serves a declared roster, never a listing.
