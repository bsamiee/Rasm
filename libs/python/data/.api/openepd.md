# [PY_DATA_API_OPENEPD]

`openepd` is the typed Pydantic v2 object model and EC3 REST client for the OpenEPD interchange format — Building Transparency's EC3 schema for Environmental Product Declarations. It roots every declaration at `BaseOpenEpdSchema`, carries the EN 15804 + TRACI LCIA payload as a typed matrix, and moves declarations through the sync EC3 client and the offline `bundle` package. It models and fetches OpenEPD/EC3 payloads; it neither computes an LCA — the Brightway cluster owns that — nor parses ILCD+EPD, which is `epdx`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `openepd`
- package: `openepd` (Apache-2.0)
- module: pure Python, zero compiled extensions; `openepd.model` imports without `requests`, which the `api-client` extra adds for the REST client
- namespaces: `openepd.model` (Pydantic v2 declarations/LCIA/specs), `openepd.api` (sync REST client), `openepd.bundle` (zip package IO), `openepd.category` (EC3 category tree), `openepd.m49` (UN M49 ↔ ISO region codes)
- rail: epd-lca (OpenEPD/EC3 interchange)
- depends: `pydantic`, `email-validator`, `idna`, `open-xpd-uuid`, `openlocationcode`; `requests` under the `api-client` extra

## [02]-[DECLARATION_MODELS]

[DECLARATION_SCOPE]: the three OpenEPD doctypes (Pydantic v2 `RootDocument` tree, `openepd.model.*`)
- variants: each doctype ships a `WithDeps` form (referenced PCR/orgs/plants inlined, self-contained) and a `Preview` form (LCIA-free search-listing projection) — `EpdWithDeps` `EpdPreview` `IndustryEpdWithDeps` `IndustryEpdPreview` `GenericEstimateWithDeps` `GenericEstimatePreview`
- ingest: `Epd.model_validate(dict)` / `Epd.model_dump()` are the typed boundary; `Epd` carries identity, declared unit, PCR ref, org/plant refs, validity, `specs`, and the `impacts` LCIA matrix
- specs: concrete material specs under `openepd.model.specs.singular.*` — `ConcreteV1` `PrecastConcreteV1` `SteelV1` `WoodV1` `AsphaltV1` `AggregatesV1` `AluminiumV1` `CMUV1` `AccessoriesV1` — with an `openepd.model.specs.range.*` per-material mirror for range queries

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]                 | [CAPABILITY]                                                   |
| :-----: | :------------------- | :---------------------------- | :------------------------------------------------------------- |
|  [01]   | `Epd` (= `EpdV0`)    | `RootDocument`                | full product EPD — LCIA matrix, `specs`, org/plant/PCR refs    |
|  [02]   | `IndustryEpd`        | `RootDocument`                | sector/industry-average doctype, same LCIA shape as `Epd`      |
|  [03]   | `GenericEstimate`    | `RootDocument`                | non-product generic estimate (typical/conservative dataset)    |
|  [04]   | `BaseDeclaration`    | `RootDocument`, `abc.ABC`     | abstract doctype base — `product_image` validation, fields     |
|  [05]   | `Org` / `OrgRef`     | `BaseOpenEpdSchema`           | manufacturer org entity + lightweight `OrgRef`                 |
|  [06]   | `Plant` / `PlantRef` | `BaseOpenEpdSchema`           | production plant entity + lightweight `PlantRef`               |
|  [07]   | `Pcr` / `PcrRef`     | `BaseOpenEpdSchema`           | Product Category Rule entity + `PcrRef`; `PcrStatus` lifecycle |
|  [08]   | `Specs`              | `BaseOpenEpdHierarchicalSpec` | per-material performance-spec aggregate on `Epd.specs`         |

[DECLARATION_FIELD_SCOPE]: the `Epd` attribute rail an identity-and-validity ingest reads (`openepd.model.epd.Epd` over `EpdPreviewV0` → `EpdRef` → `BaseDeclaration` → `RootDocument`)
- optionality: EVERY `Epd` field is optional — `model_fields[name].is_required()` is `False` for all of them; scalars default `None`, `compliance`/`plants`/`includes` default `[]`, `product_classes` defaults `{}`. `Measurement.mean` (`float`) is the one required field the declaration tree carries
- org refs: `Org` extends `OrgRef`, whose whole identity triple — `name`, `web_domain`, `ref` — is `str | None`, so an org's display name is itself absence-bearing and an issuer read unwraps twice
- ABSENT: no `subtype`, `sub_type`, or representativeness field exists anywhere in the distribution — `doctype` is the only representativeness axis, and it is a plain `str` (default `"openEPD"`, no validator) carrying an `OpenEpdDoctypes` value
- ABSENT: no `date_published` — the issue date is spelled `date_of_issue`, and both date fields are `datetime.datetime`, never `date` and never a string
- ABSENT: no closed standard vocabulary — `compliance` is `list[Standard]` and `Standard.short_name`/`Standard.name` are free `str | None` ("Must be unique. Case-insensitive"), so an EN 15804 revision resolves by reading those strings and never by an enum member
- nested values: `Amount` carries `qty: float | None` + `unit: str | None`; `Pcr` carries `issuer`, `issuer_doc_id`, `date_of_issue`, `valid_until`, `status`; `Standard` carries `short_name`, `name`, `link`, `issuer`

| [INDEX] | [MEMBER]                     | [DECLARED_TYPE]                    | [CAPABILITY]                                                      |
| :-----: | :--------------------------- | :--------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `id`                         | `OpenXpdUUID \| None`              | open xPD UUID — the EC3-native document identity                  |
|  [02]   | `doctype`                    | `str` (default `"openEPD"`)        | `OpenEpdDoctypes` value spelling; the doctype axis, not a subtype |
|  [03]   | `date_of_issue`              | `datetime.datetime \| None`        | first day the document is valid — the issue date                  |
|  [04]   | `valid_until`                | `datetime.datetime \| None`        | last day the document is valid, extensions included               |
|  [05]   | `version`                    | `NonNegativeInt \| None`           | issuer-incremented revision counter; an `int`, never a string     |
|  [06]   | `declared_unit`              | `Amount \| None`                   | functional unit as an `Amount`                                    |
|  [07]   | `kg_per_declared_unit`       | `AmountMass \| None`               | product mass in kg per declared unit                              |
|  [08]   | `compliance`                 | `list[Standard]` (default `[]`)    | standards claimed                                                 |
|  [09]   | `pcr`                        | `Pcr \| None`                      | most-specific PCR governing the declaration                       |
|  [10]   | `program_operator`           | `Org \| None`                      | the programme operator ISSUING the declaration                    |
|  [11]   | `program_operator_doc_id`    | `str \| None` (max 200)            | the registration identity at the programme operator               |
|  [12]   | `program_operator_version`   | `str \| None`                      | the programme operator's own version string                       |
|  [13]   | `third_party_verifier`       | `Org \| None`                      | org that critically reviewed the data                             |
|  [14]   | `epd_developer`              | `Org \| None`                      | org responsible for the underlying LCA                            |
|  [15]   | `manufacturer`               | `Org \| None`                      | org that makes the product — never the declaration's issuer       |
|  [16]   | `product_name`               | `str \| None`                      | the name of the product the declaration describes                 |
|  [17]   | `declaration_url`            | `str \| None`                      | link to the object on the original registrar's site               |
|  [18]   | `language`                   | `str \| None` (len 2)              | ISO 639-1 code the document is captured in                        |
|  [19]   | `private`                    | `bool \| None` (default `False`)   | author withholds contents; private entries relax required fields  |
|  [20]   | `product_service_life_years` | `float \| None` (0.0009 < v < 101) | reference service life bounding a replacement interval            |

[DECLARATION_MIXIN_SCOPE]: composable declaration facets (`openepd.model.declaration`, `openepd.model.common`, `openepd.model.lcia`)

| [INDEX] | [SYMBOL]                                  | [CAPABILITY]                                                               |
| :-----: | :---------------------------------------- | :------------------------------------------------------------------------- |
|  [01]   | `WithLciaMixin` (`openepd.model.lcia`)    | adds `impacts`, `resource_uses`, `output_flows` — the LCIA payload carrier |
|  [02]   | `WithOpenXpdUUIDMixin`, `WithAltIdsMixin` | canonical `open_xpd_uuid` (`OpenXpdUUID`) + `alt_ids` keys (`set_alt_id`)  |
|  [03]   | `WithProgramOperatorMixin`                | program-operator org ref                                                   |
|  [04]   | `WithVerifierMixin`                       | third-party verifier org ref                                               |
|  [05]   | `WithEpdDeveloperMixin`                   | EPD-developer org ref                                                      |
|  [06]   | `WithAttachmentsMixin`                    | typed `attachments: dict[str, AnyUrl]` via `set_attachment(name, url)`     |
|  [07]   | `AverageDatasetMixin`                     | marks an industry/average dataset and its representativeness               |

[LCIA_CARRIER_OPTIONALITY]: all three `WithLciaMixin` slots are OPTIONAL and default `None` — `impacts: Impacts | None`, `resource_uses: ResourceUseSet | None`, `output_flows: OutputFlowSet | None` (verified against the installed distribution). A declaration carrying resource and output flows alone is a LAWFUL document, so `decl.impacts.available_methods()` on the bare attribute answers `AttributeError` rather than a domain refusal: the container admits on the rail before any method is elected.

## [03]-[LCIA_PAYLOAD]

[LCIA_SCOPE]: the impact matrix (`openepd.model.lcia`) — the leg the material-impact owner sums
- impacts: `Impacts` is a `pydantic.RootModel` keyed `dict[LCIAMethod, ImpactSet]` — `set_impact_set(method, impact_set)`, `get_impact_set(method)`, `available_methods() -> set[LCIAMethod]`, `as_dict()`, `replace_lcia_method(old, new)`
- methods: `LCIAMethod` (`StrEnum`) — `TRACI_2_2`/`TRACI_2_1`/`TRACI_2_0`, `IPCC_AR5`/`IPCC_AR6`, `EF_3_1`/`EF_3_0`/`EF_2_0`, `CML_2016`…`CML_1992`, `RECIPE_2016`/`RECIPE_2008`, `USETOX_2_12`, `EN_15978_2011`, `GWP_GHG`, `LIME2`, `UNKNOWN`; `LCIAMethod.get_by_name(name)`/`is_method_supported(name)` resolve a wire name
- indicators: `ScopeSet` unit-fixing subclasses — `ScopeSetGwp` `ScopeSetCarbon` `ScopeSetOdp` `ScopeSetAp` `ScopeSetEpNe` `ScopeSetPocp` `ScopeSetEpFresh` `ScopeSetEpTerr` `ScopeSetIrp` `ScopeSetCTUh` `ScopeSetCTUe` `ScopeSetM3Aware` `ScopeSetKgSbe` `ScopeSetDiseaseIncidence` `ScopeSetMass` `ScopeSetVolume` `ScopeSetMassOrVolume` `ScopeSetEnergy` `ScopeSetPoint`
- units: each subclass pins `allowed_units: ClassVar[str | tuple[str, ...] | None]` — `Gwp` `kgCO2e`, `Carbon` (`kgCO2e`, `kgCO2`), `Odp` `kgCFC11e`, `Ap` (`kgSO2e`, `molHe`), `Pocp` (`kgO3e`, `kgNMVOCe`), `EpFresh` `kgPO4e`, `EpNe` `kgNe`, `EpTerr` `molNe`, `Irp` `kBqU235e`, `CTUh` `CTUh`, `CTUe` `CTUe`, `M3Aware` `m3AWARE`, `KgSbe` `kgSbe`, `DiseaseIncidence` `AnnualPerCapita`, `Mass` `kg`, `Volume` `m3`, `MassOrVolume` (`kg`, `m3`), `Energy` `MJ`, `Point` `Point`. The TWO-value rows are the EN 15804 edition axis in the package's own hand — `kgSO2e`/`kgO3e` are the +A1 spellings and `molHe`/`kgNMVOCe` the +A2 ones — so a consumer that fixes one unit per indicator across both editions publishes wrong-unit amounts
- flows: `ResourceUseSet` (`pere`/`penre`/`fw`/…) and `OutputFlowSet` (`hwd`/`nhwd`/`rwd`/`cru`/…) codes, name-addressed like `ImpactSet`

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]        | [CAPABILITY]                                                         |
| :-----: | :------------------------------------ | :------------------- | :------------------------------------------------------------------- |
|  [01]   | `Impacts`                             | `pydantic RootModel` | top LCIA container keyed by impact method                            |
|  [02]   | `LCIAMethod`                          | `StrEnum`            | the impact-method vocabulary EPDs declare against                    |
|  [03]   | `ImpactSet`                           | `ScopesetByNameBase` | one method's indicators; `get_scopeset_by_name`/`get_scopeset_names` |
|  [04]   | `ScopeSet` + per-indicator subclasses | `BaseOpenEpdSchema`  | one indicator's life-cycle-stage values, unit fixed by subclass      |
|  [05]   | `ResourceUseSet`, `OutputFlowSet`     | `ScopesetByNameBase` | EN 15804 resource-use + output-flow scope-set groups, name-addressed |
|  [06]   | `EolScenario`                         | `BaseOpenEpdSchema`  | end-of-life scenario weighting on C-stage impacts                    |
|  [07]   | `Measurement`, `Amount`               | `BaseOpenEpdSchema`  | value+unit / quantity+unit scalar carriers (`OpenEPDUnit`)           |

[SCOPESET_LAW]: a `ScopeSet` holds the EN 15804 module values (A1–A3, A4, A5, B1–B7, C1–C4, D) for one indicator; the per-indicator subclass pins the physical unit so a consumer never mixes `kgCO2e` and `mol H+e`. `Impacts.get_impact_set(method).get_scopeset_by_name("gwp")` is the typed path from method+indicator to the stage matrix.

[SCOPESET_STAGE_SCOPE]: `ScopeSet` stage attributes verbatim, every one `Measurement | None` and NONE carrying an alias, so plain attribute access is the read
- modules: `A1A2A3`, `A1`, `A2`, `A3`, `A4`, `A5`, `B1`…`B7`, `C1`…`C4`, `D`
- non-modules: `B1_years`…`B7_years` (`float | None`) are service-life metadata and `C_scenarios` (`list[EolScenario] | None`) the end-of-life weighting; neither is a stage value
- `A1A2A3` is the ONLY production aggregate the model carries — `A1`/`A2`/`A3` are their own fields and the model never sums them into it
- `Measurement.mean` (`float`, required) is the cell value; `unit` (`str | None`), `rsd` (`float > 0 | None`), and `dist` (`Distribution | None`) ride beside it

[IMPACTSET_MEMBER_SCOPE]: `ImpactSet` carries the EN 15804+A2 core-indicator roster whole — attribute, then wire alias where the two differ, each `ScopeSet<subclass> | None`
- identity-spelled: `odp`, `ap`, `pocp`, `gwp_fossil`/`gwp-fossil`, `gwp_biogenic`/`gwp-biogenic`, `gwp_luluc`/`gwp-luluc`, `ep_marine`/`ep-marine`
- divergent: `gwp` (GWP100 total, no alias), `ep_fresh`/`ep-fresh`, `ep_terr`/`ep-terr`, `ADP_mineral`/`ADP-mineral`, `ADP_fossil`/`ADP-fossil`, `WDP` (no alias)
- `ep` is a legacy synonym of `ep_marine` carrying the same meaning, so a fold reading both double-counts marine eutrophication
- `get_scopeset_by_name(name)` matches the ALIAS before the field name and falls through to `getattr` for extension impacts, so the alias spelling is the durable key
- beyond the core roster: `gwp_nonCO2`, `GWP_GHG`, `PM`, `IRP`, `ETP_fw`, `HTP_c`, `HTP_nc`, `SQP`, the `bc*` biogenic-carbon codes, and the `co2_*` carbon-balance codes

[METHOD_LAW]: `Impacts` keys by `LCIAMethod`, and the method decides what an attribute name MEANS — `gwp` under `TRACI 2.2` and under `EF 3.1` are different characterizations at different units, so a fold that ignores the key publishes wrong-unit numbers under a right-looking name. `get_impact_set` accepts a `LCIAMethod`, the method LABEL as `str` (resolved through `LCIAMethod.get_by_name`), or `None` (which reads the `UNKNOWN` bucket), and returns `None` when the declaration carries no set for it — so a label-keyed preference probe never raises. EN 15804+A2 characterization is `EF 3.1`/`EF 3.0`; `CML 2016` is the +A1 method, whose eutrophication is the single `ep`.

## [04]-[CLIENT_AND_BUNDLE]

[CLIENT_SCOPE]: the EC3/OpenEPD sync REST client (`openepd.api`, requires the `api-client` extra)
- resources: one client, one typed accessor per resource — `.epds` `.pcrs` `.orgs` `.plants` `.standards` `.categories` `.industry_epds` `.generic_estimates` (`EpdApi`, `PcrApi`, …)

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                             |
| :-----: | :----------------------------------------------------- | :------- | :------------------------------------------------------- |
|  [01]   | `OpenEpdApiClientSync(base_url, auth_token, **kwargs)` | ctor     | `auth_token` Bearer auth, `**kwargs` to `SyncHttpClient` |
|  [02]   | resource accessors (`.epds`/`.pcrs`/…)                 | property | one polymorphic client, a typed accessor per resource    |
|  [03]   | `EpdApi.get_by_openxpd_uuid(uuid)`                     | instance | fetch one typed `Epd` by `open_xpd_uuid`                 |
|  [04]   | `EpdApi.find(omf, page_size=None)`                     | instance | lazy paginated stream of typed `Epd`                     |
|  [05]   | `EpdApi.find_raw(omf, page_num, page_size)`            | instance | the raw page response behind `find`                      |
|  [06]   | `EpdApi.get_statistics/create/edit/post_with_refs`     | instance | filter stats, create/edit/upsert of `Epd`                |
|  [07]   | `MaterialFilterDefinition`, `MaterialFilterMeta`       | class    | the typed `omf` (`api.dto.mf`) the search verbs consume  |
|  [08]   | `client.set_error_handler(status_code, handler)`       | instance | per-status `ErrorHandler` over the raise default         |
|  [09]   | `SyncHttpClient`/`TokenAuth`/`HttpStreamReader`        | class    | throttle/retry, Bearer auth, stream reader               |

[CLIENT_ERRORS]: `ApiError` base over the typed failure tree the error-handler maps HTTP status onto — `ObjectNotFound` `ServerError` `ValidationError` `AuthError` `NotAuthorizedError` `AccessDeniedError` (`openepd.api.errors`).

[BUNDLE_SCOPE]: offline declaration package IO (`openepd.bundle`) — a zip carrying objects, blobs, and a manifest
- read: `DefaultBundleReader` — `get_manifest() -> BundleManifest`, `assets_iter()`, `root_assets_iter()`, `get_asset_by_ref(ref)`, `read_blob_asset(ref) -> IO[bytes]`, `read_object_asset(obj_class, ref)`
- write: `DefaultBundleWriter` — `write_object_asset(obj, ...)`, `write_blob_asset(...)`, `commit()`, `close()`
- manifest: `BundleManifest`, `AssetInfo`, `AssetType`, `RelType`, `BundleManifestAssetsStats` model the asset inventory, kinds, and inter-asset relationships

| [INDEX] | [SURFACE]             | [SHAPE] | [CAPABILITY]                                         |
| :-----: | :-------------------- | :------ | :--------------------------------------------------- |
|  [01]   | `DefaultBundleReader` | class   | typed object + blob read out of a bundle             |
|  [02]   | `DefaultBundleWriter` | class   | typed object + binary-attachment write into a bundle |

## [05]-[BASE_MACHINERY]

[BASE_SCOPE]: the schema/extension/factory kernel (`openepd.model.base`, `openepd.model.factory`, `openepd.model.common`)
- `BaseOpenEpdSchema` (extends `pydantic.BaseModel`) — every model's root; `to_serializable()`, `has_values()`, and the typed extension-field API: `set_ext(ext)`, `get_ext(ext_type)`, `get_ext_or_empty(ext_type)`, `set_ext_field(key, value)`, `get_typed_ext_field(key, type, default)`. `OpenEpdExtension` carries vendor data without forking the schema.
- `RootDocument` / `BaseDocumentFactory[TRootDocument]` / `RootDocumentFactory` / `DocumentFactory` — the doctype-discriminated factory: `RootDocumentFactory.from_dict(data)` reads `OpenEpdDoctypes` off the payload and routes to `EpdFactory` / `IndustryEpdFactory` / `GenericEstimateFactory`, so one entrypoint parses any declaration kind.
- `OpenXpdUUID` (str subtype), `OpenEpdDoctypes` (StrEnum), `Location`/`LatLng`, `Ingredient`/`Constituent`, the `RangeFloat`/`RangeInt`/`RangeRatioFloat`/`RangeAmount` range carriers, and `OpenEPDUnit` (StrEnum) — the shared value vocabulary.

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every declaration crosses the boundary through the Pydantic tree: `Epd.model_validate`/`model_dump` (or `RootDocumentFactory.from_dict` for a doctype-agnostic parse) carries a typed model inward, never a loose `dict`.
- `Impacts` is the method × indicator × EN 15804 stage matrix the material-impact owner sums; the sync client and the offline `bundle` are the two IO legs — live EC3 fetch/search and offline packages.

[STACKING]:
- `pydantic`(`../../.api/pydantic.md`): `Epd`/`Impacts`/`Specs` drop straight onto the boundary contract rail; `model_validate`/`model_dump` are the parse-not-validate seam.
- `msgspec`(`../../.api/msgspec.md`): re-encode `Epd.model_dump(mode="json")` when a non-Pydantic wire carrier is wanted; the `dict` is the handoff.
- `structlog`(`../../.api/structlog.md`) / `opentelemetry-api`(`../../.api/opentelemetry-api.md`): wrap a synchronous `OpenEpdApiClientSync` fetch in an OTel span and log the EC3 endpoint + filter; `SyncHttpClient` throttles/retries internally, so `stamina` adds only the rail-level idempotent retry, never a double-retry.
- `anyio`(`../../.api/anyio.md`): the client blocks, so `anyio.to_thread.run_sync` keeps a bulk fetch off the structured-concurrency loop.
- `universal-pathlib`(`../../.api/universal-pathlib.md`): point the `bundle` reader/writer at a `UPath` so declaration packages round-trip through the shared artifact store.
- `epdx`(`.api/epdx.md`): the complementary EPD front door — `epdx` parses ILCD+EPD, `openepd` models OpenEPD/EC3; the impact owner folds `ScopeSet` (per-indicator stage matrix) and `epdx.ImpactCategory` (per-stage indicator) onto one internal shape.
- `bw2io`(`.api/bw2io.md`) / `bw2data`(`.api/bw2data.md`) / `bw2calc`(`.api/bw2calc.md`): map an `ImpactSet` onto a Brightway result for cross-check, or write a declared indicator set as a `bw2data` activity through a `bw2io` strategy that `bw2calc` then scores.
- `olca-ipc`(`.api/olca-ipc.md`): push an openepd-modeled product into openLCA as a process with the LCIA result attached.
- `premise`(`.api/premise.md`): combine a current `Epd` foreground with a premise-shifted prospective background for a forward-looking footprint.
- `pandas`(`.api/pandas.md`) / `polars`(`.api/polars.md`) / `pyarrow`(`.api/pyarrow.md`): flatten `Impacts` (method × indicator × stage) into a frame for the profile/quality rail and cross-EPD comparison.
- impact owner (within-lib): key a fetched or parsed `Epd` by `open_xpd_uuid` + version so the persistence reuse ledger dedupes re-ingestion; the bundle's `AssetInfo` ref is the in-package identity.
- declaration owner (within-lib): `data/impact/declaration#DECLARATION` reads the identity-and-validity rail — `program_operator` the issuer, `program_operator_doc_id` the registration, `id` the provenance identity, `date_of_issue`/`valid_until` the dates, `declared_unit` the `Amount`, `compliance` the standard claim, `doctype` the representativeness — and folds an `EF 3.x` `ImpactSet` onto the frozen contract cell map.

[LOCAL_ADMISSION]:
- `openepd` is the sole OpenEPD/EC3 modeler and fetcher on the impact rail; a folder composing it registers `openepd` in the branch manifest and this catalog.

[RAIL_LAW]:
- Package: `openepd`
- Owns: the typed OpenEPD object model (`Epd`/`IndustryEpd`/`GenericEstimate` + org/plant/PCR + `Specs`), the EN 15804/TRACI LCIA payload (`Impacts`/`ImpactSet`/`ScopeSet`/`ResourceUseSet`/`OutputFlowSet`), the EC3 sync REST client (`OpenEpdApiClientSync`), and the offline declaration bundle IO
- Accept: `Epd.model_validate`/`model_dump` as the typed boundary; `RootDocumentFactory.from_dict` as the doctype-agnostic parse; `Impacts.get_impact_set(method).get_scopeset_by_name(ind)` as the typed LCIA read; `OpenEpdApiClientSync(...).epds.find(omf)` as the streaming EC3 search; `DefaultBundleReader`/`DefaultBundleWriter` for offline packages; `OpenEpdExtension` for vendor data
- Reject: hand-rolled OpenEPD JSON parsing the Pydantic tree owns; treating openepd as an LCA calculator (compute routes to `bw2calc`/`olca-ipc`) or the ILCD+EPD parser (`epdx`); double-retrying the client; forking the schema for vendor fields instead of the extension API
- Reject: naming `subtype` or `date_published` on any declaration — neither exists, representativeness reads `doctype` and the issue date `date_of_issue`; reading `Epd.manufacturer` as the issuer; admitting `declared_unit.unit` without its `qty`, which silently rescales every cell; folding an `ImpactSet` without pinning its `LCIAMethod`; summing `A1`/`A2`/`A3` into `A1A2A3`; reading `ep` beside `ep_marine`
