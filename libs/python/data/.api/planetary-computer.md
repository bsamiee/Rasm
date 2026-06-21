# [PY_DATA_API_PLANETARY_COMPUTER]

`planetary_computer` supplies the Microsoft Planetary Computer Shared Access Signature (SAS) signing surface for the data catalog rail: one `sign` `singledispatch` entry point that rewrites Azure Blob Storage HREFs on a `str` URL, GDAL VRT string, `pystac` `Asset`/`Item`/`ItemCollection`/`Collection`, `pystac_client` `ItemSearch`, or STAC `Mapping`, plus typed `sign_url`/`sign_item`/`sign_asset`/`sign_item_collection` rows and `sign_inplace` for mutation. The package owner composes `sign`/`sign_inplace` into the `pystac_client.Client.open(modifier=...)` patch path for `CATALOG_SIGNING_DISCOVERY`; it requests short-lived tokens from the SAS endpoint, caches them per account/container until expiry, and never re-implements the SAS token-fetch or HREF-rewrite that this SDK already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `planetary_computer`
- package: `planetary-computer`
- import: `planetary_computer`
- owner: `data`
- rail: catalog-signing
- installed: `1.0.0` reflected via `python -c "import planetary_computer"` on cp315
- entry points: console script `planetarycomputer` (`planetary_computer.scripts.cli:app`); library use is import-only
- capability: SAS signing of Azure Blob Storage HREFs across STAC objects (`Asset`/`Item`/`ItemCollection`/`Collection`), URL/VRT strings, `ItemSearch`, and STAC/Kerchunk mappings; per-account/container token fetch from the SAS endpoint with TTL-bounded caching and HTTP retry; subscription-key injection; optional `adlfs`/`azure-storage-blob` filesystem and container-client construction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: SAS token and signed-link models
- rail: catalog-signing

`sign` and `sign_inplace` are the `modifier=` callables for `pystac_client.Client.open`/`search`; they dispatch on input type and return the same shape with Azure Blob HREFs replaced by SAS-signed URLs. `SASToken`/`SignedLink` are the `pydantic` response models the token fetch deserializes; `Settings` carries `subscription_key` and `sas_url` resolved from `PC_SDK_*` environment variables or `~/.planetarycomputer/settings.env`.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]     | [RAIL]                                                                   |
| :-----: | :----------- | :---------------- | :----------------------------------------------------------------------- |
|  [01]   | `SASToken`   | token model       | `pydantic` SAS token response (`token`, `msft:expiry`) with `sign`/`ttl` |
|  [02]   | `SignedLink` | signed-link model | `pydantic` signed-HREF response (`href`, `msft:expiry`)                  |
|  [03]   | `SASBase`    | model base        | `pydantic` base carrying the `msft:expiry` expiry field                  |
|  [04]   | `Settings`   | configuration     | dataclass holding `subscription_key`/`sas_url` from `PC_SDK_*` env/file  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: signing dispatch
- rail: catalog-signing

`sign` is a `singledispatch` surface: it dispatches on the runtime type of `obj` and rewrites only Azure Blob Storage HREFs (`*.blob.core.windows.net`), returning unsigned URLs and already-signed URLs (`st`/`se`/`sp` present) unchanged. `copy` clones STAC objects before mutation; it has no effect on immutable strings or on the `ItemSearch` row. `sign_inplace` is `sign(obj, copy=False)`. The typed `sign_*` rows are the registered overloads, callable directly when the input type is fixed.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                                 | [CAPABILITY]                                                     |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `sign`                 | `sign(obj: Any, copy: bool = True) -> Any`                                                   | dispatch-sign any supported object; raises `TypeError` on others |
|  [02]   | `sign_inplace`         | `sign_inplace(obj: Any) -> Any`                                                              | `sign(obj, copy=False)` mutate-in-place modifier                 |
|  [03]   | `sign_url`             | `sign_url(url: str, copy: bool = True) -> str`                                               | sign a single Azure Blob URL; passthrough otherwise              |
|  [04]   | `sign_item`            | `sign_item(item: Item, copy: bool = True) -> Item`                                           | sign all assets of a `pystac` `Item`, add `msft:expiry`          |
|  [05]   | `sign_asset`           | `sign_asset(asset: Asset, copy: bool = True) -> Asset`                                       | sign one `pystac` `Asset` HREF                                   |
|  [06]   | `sign_item_collection` | `sign_item_collection(item_collection: ItemCollection, copy: bool = True) -> ItemCollection` | sign every item's assets in an `ItemCollection`                  |
|  [07]   | `sign_assets`          | `sign_assets(item: Item) -> Item`                                                            | deprecated alias of `sign_item` (emits `FutureWarning`)          |

[ENTRYPOINT_SCOPE]: token, configuration, and Azure filesystem
- rail: catalog-signing

`set_subscription_key` injects the API key into the process-level `Settings` so token requests carry `Ocp-Apim-Subscription-Key`. `get_container_client`/`get_adlfs_filesystem` build Azure SDK handles credentialed with a fresh SAS token; each raises `ImportError` when its optional dependency (`azure-storage-blob`, `adlfs`) is absent. `SASToken.sign` appends the token to an HREF; `SASToken.ttl` reports remaining validity seconds.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                                         | [CAPABILITY]                                             |
| :-----: | :--------------------- | :--------------------------------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `set_subscription_key` | `set_subscription_key(key: str) -> None`                                                             | set the process-level PC API subscription key            |
|  [02]   | `get_container_client` | `get_container_client(account_name: str, container_name: str) -> azure.storage.blob.ContainerClient` | SAS-credentialed Azure `ContainerClient` (needs `azure`) |
|  [03]   | `get_adlfs_filesystem` | `get_adlfs_filesystem(account_name: str, container_name: str) -> adlfs.AzureBlobFileSystem`          | SAS-credentialed `adlfs` filesystem (needs `adlfs`)      |
|  [04]   | `SASToken.sign`        | `sign(href: str) -> SignedLink`                                                                      | append this token to an HREF, return a `SignedLink`      |
|  [05]   | `SASToken.ttl`         | `ttl() -> float`                                                                                     | seconds the token remains valid                          |

## [04]-[IMPLEMENTATION_LAW]

[CATALOG_SIGNING_SAS]:
- import: `import planetary_computer` at boundary scope only; module-level import is banned by the manifest import policy.
- dispatch axis: one `sign` `singledispatch` owns signing across `str`/VRT, `Asset`, `Item`, `ItemCollection`, `Collection`, `ItemSearch`, and STAC/Kerchunk `Mapping`; per-type overloads are dispatch registrations, never parallel signer types — call the typed `sign_*` row only when the input type is statically fixed.
- modifier axis: `sign`/`sign_inplace` is the `modifier=` callable handed to `pystac_client.Client.open`/`search`; signing rides the catalog request as a patch step, never a re-fetch or a hand-stitched URL concatenation.
- selectivity axis: only `*.blob.core.windows.net` HREFs are signed; non-Azure URLs, the `ai4edatasetspublicassets` thumbnail host, and URLs already carrying `st`/`se`/`sp` query params pass through unmodified — signing is idempotent.
- token axis: `get_token(account_name, container_name, retry_total=10, retry_backoff_factor=0.8)` is the single token source; tokens are cached per `{sas_url}/{account}/{container}` and refreshed when TTL drops below 60s; retries cover `429/500/502/503/504`.
- configuration axis: `Settings.get()` resolves `subscription_key` and `sas_url` from `PC_SDK_SUBSCRIPTION_KEY`/`PC_SDK_SAS_URL` env vars then `~/.planetarycomputer/settings.env`; `set_subscription_key` overrides in-process without writing the settings file.
- mutation axis: `copy=True` clones STAC objects before rewriting HREFs; `sign_inplace`/`copy=False` mutates the passed object; `copy` is inert for strings and the `ItemSearch` row, which always returns a fresh signed `ItemCollection`.
- evidence: each signing run captures the resolved account/container, the `msft:expiry` written onto signed items, token TTL, and the count of HREFs rewritten versus passed through as a catalog-signing receipt.
- boundary: `planetary_computer` owns SAS token fetch and Azure Blob HREF rewriting; STAC modeling routes to `pystac`/`pystac_client`; Azure SDK handle construction (`get_container_client`/`get_adlfs_filesystem`) is opt-in behind `azure-storage-blob`/`adlfs`; raster/IO reads route to `rasterio`/`obstore` over the signed HREFs.

[RAIL_LAW]:
- Package: `planetary-computer`
- Owns: SAS signing of Azure Blob Storage HREFs across STAC objects, URL/VRT strings, `ItemSearch`, and STAC/Kerchunk mappings, with TTL-cached token fetch and subscription-key injection
- Accept: `sign`/`sign_inplace` as the `pystac_client` `modifier=` patch feeding the catalog query and asset-read owners
- Reject: wrapper-renames of `sign`/`sign_url`; a hand-rolled SAS token fetch or HREF query-string concatenation; a parallel signer type per STAC shape where `singledispatch` already discriminates; re-signing of already-signed or non-Azure URLs; eager Azure-SDK import where the signing path needs no `azure`/`adlfs` dependency
