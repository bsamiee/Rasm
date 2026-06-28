# [PY_DATA_API_OPENEPD]

`openepd` is the typed object model and API client for the OpenEPD format — the open Building Transparency / EC3 schema for Environmental Product Declarations. It is a Pydantic v2 model tree rooted at `BaseOpenEpdSchema` carrying the three declaration doctypes (`Epd`, `IndustryEpd`, `GenericEstimate`), their org/plant/PCR references, the EN 15804 + TRACI LCIA payload (`Impacts` → `ImpactSet` → per-indicator `ScopeSet`), the per-material performance `Specs` hierarchy, plus a `requests`-backed sync client (`OpenEpdApiClientSync`) over the EC3 REST API and a zip-`bundle` reader/writer for offline declaration packages. It is the OpenEPD/EC3 INGEST and modeling leg of the data EPD/LCA owner — it models and fetches OpenEPD payloads; it neither computes an LCA (that is the Brightway cluster) nor parses ILCD+EPD (that is `epdx`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `openepd`
- package: `openepd`
- import: `import openepd` (root patches Pydantic at import — see `[PYDANTIC_PATCH]`); models `from openepd.model.epd import Epd`; client `from openepd.api.sync_client import OpenEpdApiClientSync`
- version: `7.30.0`
- license: Apache-2.0 (Copyright C Change Labs Inc.)
- module: pure Python — `openepd.model.*` (Pydantic v2 declarations/LCIA/specs), `openepd.api.*` (sync REST client over `requests`), `openepd.bundle.*` (zip package IO), `openepd.category.*` (EC3 category tree), `openepd.m49.*` (UN M49 ↔ ISO region codes)
- owner: `data`
- rail: epd-lca (OpenEPD/EC3 interchange)
- asset: pure Python, zero compiled extensions; `pydantic>=2.11.7,<3` is the runtime model engine
- depends: `pydantic>=2.11.7,<3`, `email-validator>=1.3.1`, `idna>=3.7`, `open-xpd-uuid>=0.2.1,<2`, `openlocationcode>=1.0.1`; `requests>=2.0` only under the `api-client` extra (models import without `requests`)
- marker: COMPANION-GATED. Pinned `openepd; python_version<'3.15'`. openepd is pure-Python and its OWN `requires-python` is `>=3.11,<4.0` (admits cp315), so the `<3.15` gate is a conservative EPD/LCA-cluster alignment pin, liftable independently of the heavier `numpy`/`scipy` siblings once validated on cp315 — it is NOT blocked by a Rust/native wheel of its own.
- evidence: surface source-verified against the `7.30.0` release tag (`src/openepd/model/`, `src/openepd/api/`, `src/openepd/bundle/`); not `assay api`-reflected because the active cp315 interpreter does not install it under the marker gate
- capability: round-trip typed OpenEPD declarations (parse/validate/serialize via Pydantic), read/sum the LCIA `Impacts` matrix across methods, fetch and search live EC3 declarations through the sync client, and read/write offline declaration bundles
- scope-law: openepd MODELS and FETCHES OpenEPD/EC3 payloads. It is not an LCA calculator, not the ILCD+EPD parser (`epdx`), and not the material-impact system of record — the data owner normalizes its `Impacts`/`ScopeSet` into the internal impact carrier

## [02]-[DECLARATION_MODELS]

[DECLARATION_SCOPE]: the three OpenEPD doctypes (Pydantic v2 `RootDocument` tree, `openepd.model.*`)
- rail: epd-lca

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [ROLE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `Epd` (= `EpdV0`) | `RootDocument` (`WithLciaMixin`, `EpdPreviewV0`) | the full product EPD — identity, declared unit, PCR ref, org/plant refs, validity, `specs: Specs`, and the `impacts: Impacts` LCIA matrix; `Epd.model_validate(dict)` / `Epd.model_dump()` are the typed ingest/egress |
|  [02]   | `EpdWithDeps` (= `EpdWithDepsV0`) | `EpdV0` | EPD carrying its dependency objects (referenced PCR/orgs/plants inlined) for a self-contained document |
|  [03]   | `EpdPreview` (= `EpdPreviewV0`) | `BaseDeclaration` | the lightweight listing/preview projection (no LCIA payload) used by search responses |
|  [04]   | `IndustryEpd` / `IndustryEpdWithDeps` / `IndustryEpdPreview` | `RootDocument` (`WithLciaMixin`) | sector/industry-average EPD doctype, same LCIA shape as `Epd` |
|  [05]   | `GenericEstimate` / `GenericEstimateWithDeps` / `GenericEstimatePreview` | `RootDocument` (`WithLciaMixin`) | non-product generic impact estimate (typical/conservative dataset), same LCIA shape |
|  [06]   | `BaseDeclaration` | `RootDocument`, `abc.ABC` | abstract base of every doctype — `product_image` validation, common declaration fields |
|  [07]   | `Org` / `OrgRef`, `Plant` / `PlantRef`, `Pcr` / `PcrRef` | `BaseOpenEpdSchema` | the manufacturer org, production plant, and Product Category Rule entities + their lightweight `*Ref` forms; `PcrStatus` (StrEnum) is the PCR lifecycle |
|  [08]   | `Specs` | `BaseOpenEpdHierarchicalSpec` | the per-material performance-spec aggregate on `Epd.specs`; concrete material specs `ConcreteV1`, `PrecastConcreteV1`, `SteelV1`, `WoodV1`, `AsphaltV1`, `AggregatesV1`, `AluminiumV1`, `CMUV1`, `AccessoriesV1`, … live under `openepd.model.specs.singular.*` with a `range/` `RangeSpec` mirror for material-range queries |

[DECLARATION_MIXIN_SCOPE]: composable declaration facets (`openepd.model.declaration`, `openepd.model.common`)
- rail: epd-lca

| [INDEX] | [SYMBOL] | [ROLE] |
| :-----: | :------- | :----- |
|  [01]   | `WithLciaMixin` | adds `impacts: Impacts`, `resource_uses`, `output_flows` — the LCIA payload carrier mixed into every full doctype |
|  [02]   | `WithOpenXpdUUIDMixin`, `WithAltIdsMixin` | the canonical `open_xpd_uuid` identity (`OpenXpdUUID`) and external `alt_ids` cross-system keys (`set_alt_id(domain, value)`) |
|  [03]   | `WithProgramOperatorMixin`, `WithVerifierMixin`, `WithEpdDeveloperMixin` | the program-operator, third-party verifier, and EPD-developer org refs |
|  [04]   | `WithAttachmentsMixin` | typed `attachments: dict[str, AnyUrl]` with `set_attachment(name, url)` |
|  [05]   | `AverageDatasetMixin` | marks an industry/average dataset and its representativeness |

## [03]-[LCIA_PAYLOAD]

[LCIA_SCOPE]: the impact matrix (`openepd.model.lcia`) — the leg the material-impact owner sums
- rail: epd-lca

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [ROLE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `Impacts` | `pydantic.RootModel[dict[LCIAMethod, ImpactSet]]` | the top LCIA container keyed by method; `set_impact_set(method, impact_set)`, `get_impact_set(method)`, `available_methods() -> set[LCIAMethod]`, `as_dict() -> dict[LCIAMethod, ImpactSet]`, `replace_lcia_method(old, new)` |
|  [02]   | `LCIAMethod` | `StrEnum` | the supported impact methods — `TRACI_2_2`/`TRACI_2_1`/`TRACI_2_0`, `IPCC_AR5`/`IPCC_AR6`, `EF_3_1`/`EF_3_0`/`EF_2_0`, `CML_2016`…`CML_1992`, `RECIPE_2016`/`RECIPE_2008`, `USETOX_2_12`, `EN_15978_2011`, `GWP_GHG`, `LIME2`, `UNKNOWN`; `LCIAMethod.get_by_name(name)`, `LCIAMethod.is_method_supported(name)` resolve a wire name to the enum |
|  [03]   | `ImpactSet` | `ScopesetByNameBase` | one method's indicators — `get_scopeset_by_name(name)`, `get_scopeset_names()`; each indicator is a typed `ScopeSet` subclass |
|  [04]   | `ScopeSet` + per-indicator subclasses | `BaseOpenEpdSchema` | one indicator's life-cycle-stage values; subclasses fix the unit: `ScopeSetGwp`, `ScopeSetOdp`, `ScopeSetAp`, `ScopeSetEpNe`, `ScopeSetPocp`, `ScopeSetEpFresh`, `ScopeSetEpTerr`, `ScopeSetIrp`, `ScopeSetCTUh`, `ScopeSetCTUe`, `ScopeSetM3Aware`, `ScopeSetKgSbe`, `ScopeSetMJ`, `ScopeSetDiseaseIncidence`, `ScopeSetMass`, `ScopeSetVolume`, `ScopeSetMassOrVolume`, `ScopeSetEnergy`, `ScopeSetPoint` |
|  [05]   | `ResourceUseSet`, `OutputFlowSet` | `ScopesetByNameBase` | the EN 15804 resource-use (`pere`, `penre`, `fw`, …) and output-flow (`hwd`, `nhwd`, `rwd`, `cru`, …) scope-set groups, name-addressed like `ImpactSet` |
|  [06]   | `EolScenario` | `BaseOpenEpdSchema` | an end-of-life scenario weighting attached to C-stage impacts |
|  [07]   | `Measurement`, `Amount` | `BaseOpenEpdSchema` | a value+unit measurement and a quantity+unit amount (`OpenEPDUnit` StrEnum); the scalar carriers inside a `ScopeSet` |

[SCOPESET_LAW]: a `ScopeSet` holds the EN 15804 module values (A1–A3, A4, A5, B1–B7, C1–C4, D) for one indicator; the per-indicator subclass pins the physical unit so a consumer never mixes `kgCO2e` and `mol H+e`. `Impacts.get_impact_set(method).get_scopeset_by_name("gwp")` is the typed path from method+indicator to the stage matrix.

## [04]-[CLIENT_AND_BUNDLE]

[CLIENT_SCOPE]: the EC3/OpenEPD sync REST client (`openepd.api`, requires the `api-client` extra)
- rail: epd-lca

| [INDEX] | [SURFACE] | [ROLE] |
| :-----: | :-------- | :----- |
|  [01]   | `OpenEpdApiClientSync(base_url: str, auth_token: str \| None, **kwargs)` | the client root; `auth_token` becomes a Bearer `TokenAuth`; `**kwargs` flow to the `SyncHttpClient` (timeout, throttle, retry budget) |
|  [02]   | `.epds`, `.pcrs`, `.orgs`, `.plants`, `.standards`, `.categories`, `.industry_epds`, `.generic_estimates` | the resource API groups (`EpdApi`, `PcrApi`, …) — one polymorphic client, one accessor per resource, no per-call client proliferation |
|  [03]   | `EpdApi.get_by_openxpd_uuid(uuid)` | fetch one typed `Epd` by its `open_xpd_uuid` |
|  [04]   | `EpdApi.find(omf, page_size=None) -> StreamingListResponse[Epd]` / `.find_raw(omf, page_num, page_size)` | search by an Open Material Filter (`omf`); `find` returns a lazy paginated stream of typed `Epd`, `find_raw` the raw page response |
|  [05]   | `EpdApi.get_statistics(omf)` / `.create(epd, with_response=)` / `.edit(epd, with_response=)` / `.post_with_refs(epd, with_response=)` | impact statistics over a filter, and the create/edit/upsert writes (overloaded to return `Epd` or `(Epd, Response)`) |
|  [06]   | `MaterialFilterDefinition`, `MaterialFilterMeta` (`api.dto.mf`) | the typed Open Material Filter (`omf`) model the search verbs consume |
|  [07]   | `client.set_error_handler(http_status_code, handler)` | install a per-status `ErrorHandler` overriding the default raise-on-error mapping |
|  [08]   | `SyncHttpClient`, `TokenAuth`, `HttpStreamReader` (`api.base_sync_client`) | the `requests`-backed transport with built-in throttling/retry, Bearer auth, and a streaming-response reader |

[CLIENT_ERRORS]: `ApiError` base; `ObjectNotFound`, `ServerError`, `ValidationError`, `AuthError`, `NotAuthorizedError`, `AccessDeniedError` (`openepd.api.errors`) — the typed failure tree the error-handler maps HTTP status onto.

[BUNDLE_SCOPE]: offline declaration package IO (`openepd.bundle`) — a zip carrying objects + blobs + a manifest
- rail: epd-lca

| [INDEX] | [SURFACE] | [ROLE] |
| :-----: | :-------- | :----- |
|  [01]   | `DefaultBundleReader` | `get_manifest() -> BundleManifest`, `assets_iter()`, `root_assets_iter()`, `get_asset_by_ref(ref)`, `read_blob_asset(ref) -> IO[bytes]`, `read_object_asset(obj_class, ref) -> TOpenEpdObject` — typed object read out of the bundle |
|  [02]   | `DefaultBundleWriter` | `write_object_asset(obj, ...)`, `write_blob_asset(...)`, `commit()`, `close()` — write typed objects + binary attachments into a bundle |
|  [03]   | `BundleManifest`, `AssetInfo`, `AssetType` (StrEnum), `RelType` (StrEnum), `BundleManifestAssetsStats` | the manifest model — asset inventory, asset kinds, and inter-asset relationships |

## [05]-[BASE_MACHINERY]

[BASE_SCOPE]: the schema/extension/factory kernel (`openepd.model.base`, `openepd.model.factory`)
- `BaseOpenEpdSchema` (extends `pydantic.BaseModel`) — every model's root; `to_serializable()`, `has_values()`, and the typed extension-field API: `set_ext(ext)`, `get_ext(ext_type)`, `get_ext_or_empty(ext_type)`, `set_ext_field(key, value)`, `get_typed_ext_field(key, type, default)`. Extensions (`OpenEpdExtension`) are the sanctioned way to carry vendor data without forking the schema.
- `RootDocument` / `BaseDocumentFactory[TRootDocument]` / `RootDocumentFactory` / `DocumentFactory` — the doctype-discriminated factory: `RootDocumentFactory.from_dict(data)` reads `OpenEpdDoctypes` off the payload and routes to `EpdFactory` / `IndustryEpdFactory` / `GenericEstimateFactory`, so one entrypoint parses any declaration kind.
- `OpenXpdUUID` (str subtype), `OpenEpdDoctypes` (StrEnum), `Location`/`LatLng`, `Ingredient`/`Constituent`, the `RangeFloat`/`RangeInt`/`RangeRatioFloat`/`RangeAmount` range carriers, and `OpenEPDUnit` (StrEnum) — the shared value vocabulary.

[PYDANTIC_PATCH]: `import openepd` runs `openepd.patch_pydantic.patch_pydantic()` at module load (skippable via `OPENEPD_DISABLE_PYDANTIC_PATCH=true`). It adjusts the substrate `pydantic` for openepd's field semantics — a side-effecting import the contract rail must account for when openepd shares a process with other `pydantic` models. The `openepd.compat.pydantic` shim also bridges v1/v2 functional-validator imports.

## [06]-[INTEGRATION]

[SUBSTRATE_STACK]: stacking onto the universal Python rails (`libs/python/.api/`)
- `pydantic` — openepd IS a Pydantic v2 tree; `Epd`/`Impacts`/`Specs` drop straight into the boundary contract rail. `model_validate`/`model_dump` are the parse-not-validate boundary; carry the typed `Epd` inward, never a loose `dict`. NOTE the `[PYDANTIC_PATCH]` import side-effect.
- `msgspec` — re-encode `Epd.model_dump(mode="json")` through `msgspec` when a non-Pydantic wire carrier (e.g. an artifact payload) is wanted; the `dict` form is the handoff.
- `stamina` + `structlog` + `opentelemetry` — the `OpenEpdApiClientSync` is `requests`-based and synchronous: wrap a fetch in a `stamina` retry context and an OTel span, log the EC3 endpoint + filter through `structlog`. `SyncHttpClient` has its own throttle/retry; layer `stamina` only for the rail-level idempotent retry policy, not to double-retry.
- `anyio` — the client blocks; call it through `anyio.to_thread.run_sync` to keep it off the structured-concurrency event loop when an async owner fetches many declarations.
- `universal-pathlib` — point the `bundle` reader/writer at a `UPath` so declaration bundles round-trip through the same artifact store as the rest of the data owner.

[SIBLING_STACK]: stacking with the data EPD/LCA folder cluster (`libs/python/data/.api/`)
- `epdx` — the complementary EPD front door: `epdx` parses ILCD+EPD (ECO Platform / Ökobau / soda4LCA), `openepd` models OpenEPD/EC3. The data owner normalizes BOTH into one internal impact shape — `openepd.model.lcia.ScopeSet` (per-indicator stage matrix) and `epdx.ImpactCategory` (per-stage indicator) are the two source rows the owner folds.
- `bw2io` / `bw2data` / `bw2calc` — map an `Impacts`/`ImpactSet` onto a Brightway LCIA result for cross-checking, or write a measured EPD indicator set as a `bw2data` activity (via a `bw2io` strategy) so the declared product joins the computed graph; `bw2calc` then scores it.
- `olca-ipc` — push an openepd-modeled product into openLCA as a process with the LCIA result attached.
- `premise` — premise supplies the prospective (future-year) background LCI; openepd supplies the as-declared product foreground. The material-impact owner combines a current `Epd` with a premise-shifted background for a forward-looking footprint.
- `pandas` / `polars` / `pyarrow` — flatten `Impacts` (method × indicator × EN 15804 stage) into a frame for the profile/quality rail and cross-EPD comparison.

[CONTENT_IDENTITY_SEAM]: key a fetched/parsed `Epd` by its `open_xpd_uuid` (plus version) so the persistence reuse ledger dedupes re-ingestion of the same declaration; the bundle's `AssetInfo` ref is the in-package identity.

[RAIL_LAW]:
- Package: `openepd`
- Owns: the typed OpenEPD object model (`Epd`/`IndustryEpd`/`GenericEstimate` + org/plant/PCR + `Specs`), the EN 15804/TRACI LCIA payload (`Impacts`/`ImpactSet`/`ScopeSet`/`ResourceUseSet`/`OutputFlowSet`), the EC3 sync REST client (`OpenEpdApiClientSync`), and the offline declaration bundle IO
- Accept: `Epd.model_validate`/`model_dump` as the typed boundary; `RootDocumentFactory.from_dict` as the doctype-agnostic parse; `Impacts.get_impact_set(method).get_scopeset_by_name(ind)` as the typed LCIA read; `OpenEpdApiClientSync(...).epds.find(omf)` as the streaming EC3 search; `DefaultBundleReader/Writer` for offline packages; `OpenEpdExtension` for vendor data
- Reject: hand-rolling OpenEPD JSON parsing when the Pydantic tree owns it; treating openepd as an LCA calculator (route compute to `bw2calc`/`olca-ipc`) or as the ILCD+EPD parser (that is `epdx`); double-retrying the client (it throttles/retries internally — add only the rail idempotency policy); forking the schema for vendor fields instead of using the extension API; ignoring the `[PYDANTIC_PATCH]` import side-effect in a shared-`pydantic` process
