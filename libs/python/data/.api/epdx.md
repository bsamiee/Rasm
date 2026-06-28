# [PY_DATA_API_EPDX]

`epdx` parses ILCD+EPD documents into one common, machine-readable Environmental Product Declaration exchange shape — a flat `EPD` carrying every EN 15804 impact indicator (`gwp`, `odp`, `ap`, `ep`, `pocp`, the primary-energy and resource/waste indicators) where each indicator is an `ImpactCategory` broken out by life-cycle-stage module (`a1a3`, `a4`, `a5`, `b1`–`b7`, `c1`–`c4`, `d`), plus declared unit, standard version, declaration subtype, validity window, and unit conversions. The conversion core is a Rust extension; the Python layer adds a typed wrapper that returns the result as a JSON string, a `dict`, or a Pydantic `EPD` model. It is the ILCD+EPD INGEST/normalization leg of the data EPD/LCA owner — it normalizes the EPD wire format, it never computes an LCA or stores the system of record.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `epdx`
- package: `epdx`
- version: `1.2.2`
- license: Apache-2.0
- module: `epdx` (compiled Rust core `epdx.abi3.so` + Python wrapper) + `epdx.pydantic` (Pydantic v2 models)
- owner: `data`
- rail: epd-lca (EPD interchange)
- asset: Rust/PyO3 abi3 extension (`epdx.abi3.so`, `datamodel-codegen`-generated Pydantic models from `epdx.schema.json`)
- depends: `pydantic>=2.0.0`
- evidence: surface derived from package source (`epdx 1.2.2`, `packages/python/src/epdx/{__init__.py,epdx.pyi,pydantic.py}`); not cp315-reflectable — admitted under the `python_version < '3.15'` marker (`requires-python >= 3.10`, no cp315-tagged wheel), so members are source-verified, not `assay api`-reflected. UPSTREAM STATUS: legacy/deprecated — the project is archived with `1.2.2` (2024-04) as the final release; treat the surface as frozen
- capability: ILCD+EPD JSON → EPDx common format conversion with `str`/`dict`/Pydantic return selection, and the full EN 15804 `EPD` Pydantic model (indicator × life-cycle-stage matrix) for typed downstream use
- scope-law: the Python binding types ONLY ILCD+EPD ingest (`convert_ilcd`). LCAbyg conversion exists in the Rust crate and JS package but is NOT exposed as a typed Python wrapper in `1.2.2` — do not assume a Python `convert_lcabyg`; ILCD+EPD is the Python entry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: EPD model (Pydantic v2, `epdx.pydantic` / re-exported on `epdx`)
- rail: epd-lca

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [ROLE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `EPD` | `pydantic.BaseModel` | the common EPD record — identity (`id`, `name`, `location`, `version`, `format_version`), classification (`standard: Standard`, `subtype: SubType`, `declared_unit: Unit`), validity (`published_date`, `valid_until` as `datetime`), `reference_service_life` (`conint(ge=0)`), `conversions: list[Conversion]`, `source: Source`, `comment`, `meta_data: dict[str, Any]`, plus every EN 15804 indicator as `Optional[ImpactCategory]` |
|  [02]   | `ImpactCategory` | `pydantic.BaseModel` | one indicator broken out by life-cycle-stage module: `a1a3`, `a4`, `a5`, `b1`–`b7`, `c1`–`c4`, `d` (all `Optional[float]`) — the per-stage value matrix the material-impact owner sums |
|  [03]   | `Conversion` | `pydantic.BaseModel` | a declared-unit conversion: `meta_data: str`, `to: Unit`, `value: float` |
|  [04]   | `Source` | `pydantic.BaseModel` | the declaration source: `name: str`, `url: Optional[str]` |

[PUBLIC_TYPE_SCOPE]: EPD enums
- rail: epd-lca

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [MEMBERS] |
| :-----: | :------- | :------------ | :-------- |
|  [01]   | `Standard` | `Enum` | `EN15804A1`, `EN15804A2`, `UNKNOWN` — the EN 15804 standard generation (drives which indicator set is populated) |
|  [02]   | `SubType` | `Enum` | `Generic`, `Specific`, `Industry`, `Representative` — declaration representativeness |
|  [03]   | `Unit` | `Enum` | `M`, `M2`, `M3`, `KG`, `TONES`, `PCS`, `L`, `M2R1`, `KM`, `TONES_KM`, `UNKNOWN` — declared/conversion functional unit |

EN 15804 indicator fields on `EPD` (each `Optional[ImpactCategory]`): `gwp`, `odp`, `ap`, `ep`, `pocp`, `adpe`, `adpf` (impact); `penre`, `penrm`, `penrt`, `pere`, `perm`, `pert` (primary energy); `sm`, `rsf`, `nrsf`, `fw` (resource); `hwd`, `nhwd`, `rwd`, `cru`, `mfr`, `mer`, `eee`, `eet` (waste/output flows).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: conversion
- rail: epd-lca

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `epdx.convert_ilcd(data: str | dict, *, as_type: type[T] = dict) -> str | dict | EPD` | typed wrapper | parse ILCD+EPD JSON (string or `dict`) into EPDx; `as_type` selects the return form — `dict` (default), `str` (JSON), or `EPD` (Pydantic); raises `epdx.ParsingException` on parse failure |
|  [02]   | `epdx._convert_ilcd(data: str) -> str` | raw Rust core | the PyO3 entry the wrapper calls — string-in, EPDx-JSON-string-out; prefer `convert_ilcd` for typed returns and error wrapping |
|  [03]   | `EPD(**dict)` / `EPD.model_validate(...)` / `EPD.model_dump(...)` | Pydantic | construct/validate/serialize the typed model from a converted `dict` (standard Pydantic v2 surface) |

[ENTRYPOINT_SCOPE]: errors
- rail: epd-lca

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [ROLE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `epdx.ParsingException` | `Exception` | raised by `convert_ilcd` wrapping any Rust-core parse error for the malformed/unsupported ILCD+EPD input |

## [04]-[IMPLEMENTATION_LAW]

[CONVERSION_TOPOLOGY]:
- two-layer: the Rust core (`epdx.abi3.so`) does the ILCD+EPD parse and emits an EPDx JSON string (`_convert_ilcd`); the Python `convert_ilcd` wrapper JSON-dumps a `dict` input, calls the core, then returns `str` (raw), `dict` (`json.loads`), or `EPD` (`EPD(**json.loads(...))`) by `as_type`
- the `EPD` shape is an INDICATOR × STAGE matrix: each EN 15804 indicator is an `ImpactCategory` whose `a1a3`/`a4`/`a5`/`b*`/`c*`/`d` fields are the per-module values; the production total is `a1a3`, and downstream summing into A1–A3 / use / end-of-life is the consumer's, not epdx's
- all indicator fields are `Optional` because an EN15804A1 vs A2 declaration populates different indicator subsets — read `standard` to know which set is meaningful

[INTEGRATION]:
- material-impact owner (the design page these catalogs feed): `convert_ilcd(..., as_type=EPD)` is the typed ingest of an ILCD+EPD document; the `EPD` model is the normalized material-impact carrier the owner reasons over
- openepd seam: `epdx` parses ILCD+EPD while `openepd` models OpenEPD payloads — the EPD/LCA owner normalizes BOTH external EPD formats into one internal material-impact shape; epdx covers the ECO Platform / Ökobau / soda4LCA ILCD+EPD side
- bw2io/bw2calc/olca-ipc seam: ILCD is openLCA's native exchange format, so an EPDx-parsed indicator set maps onto a Brightway database (via a `bw2io` strategy) or an openLCA process/flow (`olca-ipc`); epdx is the EPD-document front door of the two compute engines, and the parsed `gwp`/EN 15804 indicator values reconcile against a `bw2calc`-characterized `score` (the engine recomputes what the EPD asserts)
- contract seam: the `EPD`/`ImpactCategory` Pydantic models drop into the data contract rail (`pydantic` is a substrate) for boundary validation; coerce the converted `dict` straight to `EPD` for a typed receipt
- tabular/profile seam: flatten the indicator × stage matrix (25 indicators × 15 modules) into a `pandas`/`polars` frame for the profile/quality rail and cross-EPD comparison; `msgspec` re-encodes the `dict` form at the wire when a non-Pydantic carrier is wanted
- ContentIdentity seam: key a parsed `EPD` by the runtime `ContentIdentity` over `id` + `published_date` + `version` so re-ingestion of the same declaration is deduped in the persistence reuse ledger

[EXCEPTIONS]:
- `epdx.ParsingException` — malformed or unsupported ILCD+EPD input; the only typed failure the Python wrapper raises
- `convert_ilcd` raises `NotImplementedError` for an `as_type` other than `str`/`dict`/`EPD`
- Pydantic `ValidationError` surfaces when constructing `EPD` from a `dict` that violates the model (e.g. missing required `declared_unit`/`published_date`)

[RAIL_LAW]:
- Package: `epdx`
- Owns: ILCD+EPD → EPDx common-format conversion and the typed EN 15804 `EPD`/`ImpactCategory` model (indicator × life-cycle-stage matrix)
- Accept: `convert_ilcd(data, as_type=EPD)` as the typed ingest; the `EPD` Pydantic model as the normalized EPD carrier; `ParsingException` as the parse-failure rail; the `dict`/`str` forms for wire/`msgspec` handoff
- Reject: hand-rolled ILCD+EPD XML/JSON parsing when `convert_ilcd` owns it; assuming a Python `convert_lcabyg` (Rust/JS-only in `1.2.2`); treating `epdx` as an LCA calculator or the EPD system of record (it normalizes the document); building on it as actively maintained — pin behavior to the frozen `1.2.2` surface
