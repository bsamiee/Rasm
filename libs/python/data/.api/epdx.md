# [PY_DATA_API_EPDX]

`epdx` normalizes an ILCD+EPD document into one common EPDx JSON shape through a single Rust/PyO3 entry, `convert_ilcd`, and carries the typed EN 15804 `EPD` model — every impact indicator broken out by life-cycle-stage module — in `epdx.pydantic` for the consumer to construct from that JSON. It owns the ILCD+EPD ingest leg of the data EPD/LCA owner: it normalizes the EPD wire format, never computing an LCA or holding the system of record.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `epdx`
- package: `epdx` (Apache-2.0)
- module: `epdx` — compiled Rust/PyO3 abi3 core (`epdx/epdx.abi3.so`), re-exported verbatim by `epdx/__init__.py`; `epdx.pydantic` carries the `datamodel-codegen`-generated EN 15804 model
- rail: epd-lca (EPD interchange)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: EN 15804 EPD model — Pydantic v2, `epdx.pydantic` only, never re-exported on the top-level `epdx` namespace

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :--------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `EPD`            | class         | identity, classification, validity, and every EN 15804 indicator |
|  [02]   | `ImpactCategory` | class         | one indicator's per-stage value matrix the impact owner sums     |
|  [03]   | `Conversion`     | class         | a declared-unit conversion                                       |
|  [04]   | `Source`         | class         | the declaration source                                           |
|  [05]   | `Standard`       | enum          | EN 15804 generation                                              |
|  [06]   | `SubType`        | enum          | declaration representativeness                                   |
|  [07]   | `Unit`           | enum          | functional unit                                                  |

- `EPD` identity and meta fields: `id` `name` `location` `version` `format_version` `standard` `subtype` `declared_unit` `published_date` `valid_until` `reference_service_life` `conversions` `source` `comment` `meta_data`
- `EPD` EN 15804 indicator fields, each `Optional[ImpactCategory]`: `gwp` `odp` `ap` `ep` `pocp` `adpe` `adpf` `penre` `penrm` `penrt` `pere` `perm` `pert` `sm` `rsf` `nrsf` `fw` `hwd` `nhwd` `rwd` `cru` `mrf` `mer` `eee` `eet`
- `ImpactCategory` per-stage fields, each `Optional[float]`: `a1a3` `a4` `a5` `b1` `b2` `b3` `b4` `b5` `b6` `b7` `c1` `c2` `c3` `c4` `d`
- leaf models: `Conversion` (`to` `value`), `Source` (`name` `url`)
- `Standard`: `EN15804A1` `EN15804A2` `UNKNOWN`
- `SubType`: `Generic` `Specific` `Industry` `Representative`
- `Unit`: `M` `M2` `M3` `KG` `TONES` `PCS` `L` `M2R1` `UNKNOWN`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: conversion — one Rust/PyO3 entry, then the typed-model construction it feeds

| [INDEX] | [SURFACE]                       | [SHAPE] | [CAPABILITY]                                                |
| :-----: | :------------------------------ | :------ | :---------------------------------------------------------- |
|  [01]   | `convert_ilcd(json) -> str`     | static  | ILCD+EPD JSON string in, normalized EPDx JSON string out    |
|  [02]   | `json.loads(convert_ilcd(...))` | static  | decode the EPDx JSON to a `dict` for `msgspec`/wire handoff |
|  [03]   | `EPD(**json.loads(...))`        | ctor    | the typed model; `EPD.model_validate`/`model_dump` follow   |

- `convert_ilcd`: unwraps its parse `Result`, so malformed ILCD+EPD JSON surfaces as a propagated `pyo3_runtime.PanicException` — no typed `epdx.ParsingException` exists; guard at the call site.
- `EPD(**...)`: raises Pydantic `ValidationError` on a `dict` missing a required field (`declared_unit`, `published_date`, `standard`).

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `convert_ilcd` parses the ILCD+EPD JSON and emits the EPDx JSON string; `epdx` re-exports it with no Python-side `as_type` layer, so the consumer runs `json.loads` then `epdx.pydantic.EPD(**...)` to land the typed model.
- `EPD` is an indicator × stage matrix: each indicator is an `ImpactCategory` whose per-module fields carry the values; the production total is `a1a3`, and summing into A1–A3 / use / end-of-life stages is the consumer's, never epdx's.
- Every indicator field is `Optional` because an EN15804A1 versus A2 declaration populates a different indicator subset; read `standard` to know which set is meaningful.

[STACKING]:
- `openepd`(`.api/openepd.md`): epdx parses ILCD+EPD while openepd models OpenEPD/EC3 — the impact owner normalizes both external formats onto one EN 15804 carrier, epdx covering the ECO Platform / Ökobau / soda4LCA ILCD side.
- `bw2io`(`.api/bw2io.md`) / `olca-ipc`(`.api/olca-ipc.md`): ILCD is openLCA's native exchange, so a parsed indicator set maps onto a Brightway database through a `bw2io` strategy or an openLCA process through `olca-ipc`.
- `bw2calc`(`.api/bw2calc.md`): a parsed `gwp` reconciles against a `bw2calc`-characterized `score` — the engine recomputes what the EPD asserts.
- `pydantic`(`../../.api/pydantic.md`): the `EPD`/`ImpactCategory` models drop onto the data-contract rail for boundary validation; coerce the decoded `dict` straight to `EPD` for a typed receipt.
- `msgspec`(`../../.api/msgspec.md`): re-encode the decoded `dict` at the wire when a non-Pydantic carrier is wanted.
- impact owner (within-lib): `EPD(**json.loads(convert_ilcd(doc)))` is the typed ingest the material-impact owner reasons over; flatten the indicator × stage matrix into a `pandas`/`polars` frame for the profile rail and cross-EPD comparison, and key a parsed `EPD` by the runtime `ContentIdentity` over `id` + `published_date` + `version` for reuse-ledger dedup.

[LOCAL_ADMISSION]:
- `epdx` is the sole ILCD+EPD document normalizer on the impact rail; a folder composing it registers `epdx` in the branch manifest and this catalog.

[RAIL_LAW]:
- Package: `epdx`
- Owns: ILCD+EPD → EPDx JSON conversion through `convert_ilcd`, and the typed EN 15804 `EPD`/`ImpactCategory` indicator × stage model in `epdx.pydantic`.
- Accept: `convert_ilcd(json)` as the conversion entry; `epdx.pydantic.EPD(**json.loads(...))` as the typed normalized carrier; the returned `str` or decoded `dict` for wire/`msgspec` handoff; a call-site guard around the `pyo3_runtime.PanicException`.
- Reject: hand-rolled ILCD+EPD parsing that `convert_ilcd` owns; a `from epdx import EPD` top-level import when the model is `epdx.pydantic.EPD`; treating `epdx` as an LCA calculator or the EPD system of record.
