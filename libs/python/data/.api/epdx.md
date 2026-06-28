# [PY_DATA_API_EPDX]

`epdx` parses ILCD+EPD documents into one common, machine-readable Environmental Product Declaration exchange shape — a flat `EPD` carrying every EN 15804 impact indicator (`gwp`, `odp`, `ap`, `ep`, `pocp`, the primary-energy and resource/waste indicators) where each indicator is an `ImpactCategory` broken out by life-cycle-stage module (`a1a3`, `a4`, `a5`, `b1`–`b7`, `c1`–`c4`, `d`), plus declared unit, standard version, declaration subtype, validity window, and unit conversions. The conversion core is a Rust/PyO3 extension exposing one function, `convert_ilcd`, that takes an ILCD+EPD JSON string and returns the normalized EPDx JSON string; the typed EN 15804 `EPD`/`ImpactCategory` model lives in `epdx.pydantic` for the consumer to construct from that JSON. It is the ILCD+EPD INGEST/normalization leg of the data EPD/LCA owner — it normalizes the EPD wire format, it never computes an LCA or stores the system of record.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `epdx`
- package: `epdx`
- version: `0.3.0`
- license: Apache-2.0
- module: `epdx` (compiled Rust/PyO3 core `epdx/epdx.abi3.so`, re-exported verbatim by `epdx/__init__.py` as `from .epdx import *`) + `epdx.pydantic` (datamodel-codegen-generated Pydantic models)
- owner: `data`
- rail: epd-lca (EPD interchange)
- asset: Rust/PyO3 abi3 extension (`epdx/epdx.abi3.so`); `epdx/pydantic.py` is `datamodel-codegen`-generated from `epdx.schema.json`
- depends: none at runtime — the Rust core is self-contained; `epdx.pydantic` imports `pydantic` (v2 in this env, satisfied by the substrate) for the typed model
- evidence: assay-reflected — `epdx 0.3.0` (`api resolve epdx`), the `<1.0` line the manifest pins; the compiled core's `__all__` is `{convert_ilcd}` and the models live in `epdx.pydantic`
- capability: one Rust entry, `convert_ilcd(json_str) -> str`, normalizes an ILCD+EPD JSON document into the EPDx JSON string; `epdx.pydantic` carries the full EN 15804 `EPD` model (indicator × life-cycle-stage matrix) the consumer constructs from that JSON for typed downstream use
- scope-law: the Rust core exports ONLY `convert_ilcd` (no `convert_lcabyg` in this line, no private `_convert_ilcd`); the typed model is `epdx.pydantic.EPD`, NOT re-exported on the top-level `epdx` namespace — import it from `epdx.pydantic`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: EPD model (Pydantic v2, `epdx.pydantic` only — NOT re-exported on top-level `epdx`)
- rail: epd-lca

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [ROLE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `EPD` | `pydantic.BaseModel` | the common EPD record — required identity (`id`, `name`, `location`, `version`, `format_version`), classification (`standard: Standard`, `subtype: SubType`, `declared_unit: Unit`), validity (`published_date`, `valid_until` as `datetime`); optional `reference_service_life: Optional[conint(ge=0)]`, `conversions: Optional[List[Conversion]]`, `source: Optional[Source]`, `comment: Optional[str]`, `meta_data: Optional[Dict[str, Any]]`, plus every EN 15804 indicator as `Optional[ImpactCategory]` |
|  [02]   | `ImpactCategory` | `pydantic.BaseModel` | one indicator broken out by life-cycle-stage module: `a1a3`, `a4`, `a5`, `b1`–`b7`, `c1`–`c4`, `d` (all `Optional[float]`) — the per-stage value matrix the material-impact owner sums |
|  [03]   | `Conversion` | `pydantic.BaseModel` | a declared-unit conversion: `to: Unit`, `value: float` |
|  [04]   | `Source` | `pydantic.BaseModel` | the declaration source: `name: str`, `url: Optional[str]` |

[PUBLIC_TYPE_SCOPE]: EPD enums
- rail: epd-lca

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [MEMBERS] |
| :-----: | :------- | :------------ | :-------- |
|  [01]   | `Standard` | `Enum` | `EN15804A1`, `EN15804A2`, `UNKNOWN` — the EN 15804 standard generation (drives which indicator set is populated) |
|  [02]   | `SubType` | `Enum` | `Generic`, `Specific`, `Industry`, `Representative` — declaration representativeness |
|  [03]   | `Unit` | `Enum` | `M`, `M2`, `M3`, `KG`, `TONES`, `PCS`, `L`, `M2R1`, `UNKNOWN` — declared/conversion functional unit |

EN 15804 indicator fields on `EPD` (each `Optional[ImpactCategory]`): `gwp`, `odp`, `ap`, `ep`, `pocp`, `adpe`, `adpf` (impact); `penre`, `penrm`, `penrt`, `pere`, `perm`, `pert` (primary energy); `sm`, `rsf`, `nrsf`, `fw` (resource); `hwd`, `nhwd`, `rwd`, `cru`, `mrf`, `mer`, `eee`, `eet` (waste/output flows).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: conversion
- rail: epd-lca

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `epdx.convert_ilcd(json: str) -> str` | Rust core | the single PyO3 entry (text-signature `(json)`); ILCD+EPD JSON string in, EPDx JSON string out — no `as_type`/`dict`/`EPD` selection in this line, the consumer parses the returned JSON |
|  [02]   | `json.loads(epdx.convert_ilcd(doc))` | decode | the converted EPDx JSON decoded to a `dict` for `msgspec`/wire handoff or model construction |
|  [03]   | `epdx.pydantic.EPD(**json.loads(...))` / `EPD.model_validate(...)` / `EPD.model_dump(...)` | Pydantic | construct/validate/serialize the typed model from the converted JSON (standard Pydantic v2 surface) |

[ENTRYPOINT_SCOPE]: errors
- rail: epd-lca

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [ROLE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `pyo3_runtime.PanicException` | Rust panic | `convert_ilcd` propagates a PyO3 panic (the Rust core `unwrap`s its parse `Result`) on malformed/unsupported ILCD+EPD input — there is no typed `epdx.ParsingException` in this line; guard the call and treat the panic as the parse-failure boundary |

## [04]-[IMPLEMENTATION_LAW]

[CONVERSION_TOPOLOGY]:
- single Rust entry: `convert_ilcd(json_str)` parses the ILCD+EPD JSON and emits the EPDx JSON string; `epdx/__init__.py` re-exports it verbatim (`from .epdx import *`) with no Python-side `as_type` layer — the consumer runs `json.loads` then `epdx.pydantic.EPD(**...)` to land the typed model
- the `EPD` shape is an INDICATOR × STAGE matrix: each EN 15804 indicator is an `ImpactCategory` whose `a1a3`/`a4`/`a5`/`b*`/`c*`/`d` fields are the per-module values; the production total is `a1a3`, and downstream summing into A1–A3 / use / end-of-life is the consumer's, not epdx's
- all indicator fields are `Optional` because an EN15804A1 vs A2 declaration populates different indicator subsets — read `standard` to know which set is meaningful

[INTEGRATION]:
- material-impact owner (the design page these catalogs feed): `epdx.pydantic.EPD(**json.loads(epdx.convert_ilcd(doc)))` is the typed ingest of an ILCD+EPD document; the `EPD` model is the normalized material-impact carrier the owner reasons over
- openepd seam: `epdx` parses ILCD+EPD while `openepd` models OpenEPD payloads — the EPD/LCA owner normalizes BOTH external EPD formats into one internal material-impact shape; epdx covers the ECO Platform / Ökobau / soda4LCA ILCD+EPD side
- bw2io/bw2calc/olca-ipc seam: ILCD is openLCA's native exchange format, so an EPDx-parsed indicator set maps onto a Brightway database (via a `bw2io` strategy) or an openLCA process/flow (`olca-ipc`); epdx is the EPD-document front door of the two compute engines, and the parsed `gwp`/EN 15804 indicator values reconcile against a `bw2calc`-characterized `score` (the engine recomputes what the EPD asserts)
- contract seam: the `EPD`/`ImpactCategory` Pydantic models drop into the data contract rail (`pydantic` is a substrate) for boundary validation; coerce the `json.loads(convert_ilcd(...))` dict straight to `EPD` for a typed receipt
- tabular/profile seam: flatten the indicator × stage matrix (25 indicators × 15 modules) into a `pandas`/`polars` frame for the profile/quality rail and cross-EPD comparison; `msgspec` re-encodes the decoded `dict` at the wire when a non-Pydantic carrier is wanted
- ContentIdentity seam: key a parsed `EPD` by the runtime `ContentIdentity` over `id` + `published_date` + `version` so re-ingestion of the same declaration is deduped in the persistence reuse ledger

[EXCEPTIONS]:
- `pyo3_runtime.PanicException` — the Rust core `unwrap`s its parse `Result`, so malformed/unsupported ILCD+EPD JSON surfaces as a propagated PyO3 panic, not a typed exception; this line has no `epdx.ParsingException` to catch, so guard `convert_ilcd` at the call site
- Pydantic `ValidationError` surfaces when constructing `epdx.pydantic.EPD` from a `dict` that violates the model (e.g. missing required `declared_unit`/`published_date`/`standard`)

[RAIL_LAW]:
- Package: `epdx`
- Owns: ILCD+EPD → EPDx JSON conversion (`convert_ilcd`) and the typed EN 15804 `EPD`/`ImpactCategory` model (indicator × life-cycle-stage matrix) in `epdx.pydantic`
- Accept: `convert_ilcd(json_str)` as the conversion entry; `epdx.pydantic.EPD(**json.loads(...))` as the typed normalized carrier; the returned JSON `str`/decoded `dict` for wire/`msgspec` handoff; a guard around the PyO3 panic as the parse-failure boundary
- Reject: hand-rolled ILCD+EPD XML/JSON parsing when `convert_ilcd` owns it; a Python `convert_lcabyg` or `_convert_ilcd` (not exported in this line); a top-level `from epdx import EPD` (the model is `epdx.pydantic.EPD`); treating `epdx` as an LCA calculator or the EPD system of record (it normalizes the document)
