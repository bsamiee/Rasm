# [PY_DATA_API_PLANETARY_COMPUTER]

`planetary_computer` mints the Microsoft Planetary Computer SAS signing surface for the data catalog rail: one `sign` `singledispatch` entry rewrites Azure Blob Storage HREFs across a `str`/VRT URL, `pystac` `Asset`/`Item`/`ItemCollection`/`Collection`, `pystac_client` `ItemSearch`, or STAC `Mapping`, and feeds `pystac_client.Client.open(modifier=...)` as the catalog-signing patch step. It fetches short-lived SAS tokens per account/container, caches them to expiry, and owns the token fetch and HREF rewrite no consumer re-implements.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `planetary-computer`
- package: `planetary-computer` (MIT)
- module: `planetary_computer`
- rail: catalog-signing
- capability: SAS signing of Azure Blob Storage HREFs across STAC `Asset`/`Item`/`ItemCollection`/`Collection`, URL/VRT strings, `ItemSearch`, and STAC/Kerchunk mappings; per-account/container token fetch with TTL cache and HTTP retry; subscription-key injection; opt-in `adlfs`/`azure-storage-blob` filesystem and container-client construction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: SAS token and signed-link response models — internal to the signing rail, consumed indirectly and never imported at the boundary.

| [INDEX] | [SYMBOL]     | [MODULE]                      | [TYPE_FAMILY]     | [CAPABILITY]                                                     |
| :-----: | :----------- | :---------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `SASToken`   | `planetary_computer.sas`      | token model       | SAS token response (`token`, `msft:expiry`) with `sign`/`ttl`    |
|  [02]   | `SignedLink` | `planetary_computer.sas`      | signed-link model | signed-HREF response (`href`, `msft:expiry`)                     |
|  [03]   | `SASBase`    | `planetary_computer.sas`      | model base        | base carrying the `msft:expiry` expiry field                     |
|  [04]   | `Settings`   | `planetary_computer.settings` | configuration     | `subscription_key`/`sas_url` from `PC_SDK_*` env or settings.env |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: signing dispatch — module-level functions returning the same shape signed.
- carry: each `sign_*(obj, *, copy=True)` clones STAC objects before mutation; `sign(obj)` dispatches on runtime type, `sign_inplace(obj)` is `copy=False`.

| [INDEX] | [SURFACE]              | [CAPABILITY]                                                     |
| :-----: | :--------------------- | :--------------------------------------------------------------- |
|  [01]   | `sign`                 | dispatch-sign any supported object; raises `TypeError` on others |
|  [02]   | `sign_inplace`         | `sign(obj, copy=False)` mutate-in-place modifier                 |
|  [03]   | `sign_url`             | sign a single Azure Blob URL; passthrough otherwise              |
|  [04]   | `sign_item`            | sign all assets of a `pystac` `Item`, add `msft:expiry`          |
|  [05]   | `sign_asset`           | sign one `pystac` `Asset` HREF                                   |
|  [06]   | `sign_item_collection` | sign every item's assets in an `ItemCollection`                  |
|  [07]   | `sign_assets`          | deprecated alias of `sign_item` (emits `FutureWarning`)          |

[ENTRYPOINT_SCOPE]: token, configuration, and Azure filesystem handles.
- carry: `set_subscription_key(key)`; `get_container_client(account_name, container_name) -> ContainerClient` and `get_adlfs_filesystem(account_name, container_name) -> AzureBlobFileSystem` share the arg pair; `SASToken.sign(href) -> SignedLink`, `SASToken.ttl() -> float`.

| [INDEX] | [SURFACE]              | [CAPABILITY]                                             |
| :-----: | :--------------------- | :------------------------------------------------------- |
|  [01]   | `set_subscription_key` | set the process-level PC API subscription key            |
|  [02]   | `get_container_client` | SAS-credentialed Azure `ContainerClient` (needs `azure`) |
|  [03]   | `get_adlfs_filesystem` | SAS-credentialed `adlfs` filesystem (needs `adlfs`)      |
|  [04]   | `SASToken.sign`        | append this token to an HREF, return a `SignedLink`      |
|  [05]   | `SASToken.ttl`         | seconds the token remains valid                          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `sign` `singledispatch` owns signing across `str`/VRT, `Asset`, `Item`, `ItemCollection`, `Collection`, `ItemSearch`, and STAC/Kerchunk `Mapping`; the typed `sign_*` rows are dispatch registrations, selected only when the input type is statically fixed, never parallel signer types.
- Signing is idempotent and selective: only `*.blob.core.windows.net` HREFs sign; non-Azure URLs, the `ai4edatasetspublicassets` thumbnail host, and URLs already carrying `st`/`se`/`sp` query params pass through unmodified.
- `get_token(account_name, container_name, retry_total=10, retry_backoff_factor=0.8)` is the single token source; tokens cache per `{sas_url}/{account}/{container}`, refresh when TTL drops below 60s, and retry `429/500/502/503/504`.
- `Settings.get()` resolves `subscription_key`/`sas_url` from `PC_SDK_SUBSCRIPTION_KEY`/`PC_SDK_SAS_URL` then `~/.planetarycomputer/settings.env`; `set_subscription_key` overrides in-process without writing the settings file.
- `copy=True` clones STAC objects before rewriting HREFs; `sign_inplace`/`copy=False` mutates the passed object; `copy` is inert for strings and the `ItemSearch` row, which returns a fresh signed `ItemCollection`.
- Each signing run captures the resolved account/container, the `msft:expiry` written onto signed items, token TTL, and the rewritten-versus-passed HREF counts as a catalog-signing receipt.

[STACKING]:
- `pystac-client`(`.api/pystac-client.md`): `sign`/`sign_inplace` is the `modifier=` callable on `Client.open`/`search`, threaded through `StacApiIO` onto every hydrated `Modifiable`; signing rides the catalog request as a patch step, never a re-fetch or a hand-stitched URL concatenation.
- `pystac`(`.api/pystac.md`): `sign(item)` returns the same `Item` with signed asset HREFs, which flow to `rasterio`/`rioxarray` (`/vsicurl/` COG reads) and `odc-stac`(`.api/odc-stac.md`) as its `patch_url=...sign` per-asset rewriter feeding the lazy cube.
- within `data`: the STAC discovery owner composes `sign_inplace` into the `pystac_client.Client.open(modifier=...)` patch path; Azure SDK handle construction (`get_container_client`/`get_adlfs_filesystem`) is opt-in behind `azure-storage-blob`/`adlfs`.

[LOCAL_ADMISSION]:
- `import planetary_computer` at boundary scope; the signing path needs no eager `azure`/`adlfs` import, and both handle builders raise `ImportError` when their optional dependency is absent.

[RAIL_LAW]:
- Package: `planetary-computer`
- Owns: SAS token fetch and Azure Blob Storage HREF rewriting across STAC objects, URL/VRT strings, `ItemSearch`, and STAC/Kerchunk mappings, with TTL-cached tokens and subscription-key injection; STAC modeling routes to `pystac`/`pystac_client` and raster/IO reads to `rasterio`/`obstore` over the signed HREFs
- Accept: `sign`/`sign_inplace` as the `pystac_client` `modifier=` patch feeding the catalog query and asset-read owners
- Reject: wrapper-renames of `sign`/`sign_url`; a hand-rolled SAS token fetch or HREF query-string concatenation; a parallel signer type per STAC shape where `singledispatch` discriminates; re-signing of already-signed or non-Azure URLs; eager Azure-SDK import where the signing path needs no `azure`/`adlfs` dependency
