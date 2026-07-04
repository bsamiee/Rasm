# [PY_GEOMETRY_ENERGY_SIMULATE]

The simulation egress owner — where an admitted building model becomes engine input, a recipe run, and typed result frames. Three concerns, one owner, strict seams. `translate` is the in-process OpenStudio leg: the CPU-bound `honeybee-openstudio` writers (`model_to_osm`/`model_to_idf`/`model_to_epjson`/`model_to_gbxml` over the one `model_to_openstudio` core) run OFFLOADED on the worker lane — never on the event loop — with the native `openstudio` SDK gated by a `find_spec` probe and the `honeybee_energy.run` OSW/CLI pair (`to_openstudio_osw` + `run_osw`) as the fall-through when only the CLI is installed: the two are the in-process/subprocess pair of ONE translation concept, a format row selecting the writer, never parallel translators. `simulate` is the recipe BINDING: geometry constructs the submission — the model's durable `.hbjson`, the weather artifact, and the `SimulationParameter` document become the runtime `RecipeSpec` input rows whose `pollination_handlers` chains coerce at the runtime seam, and `RecipeInterface`/`Job` schema shapes project through the runtime owner's `interface` — then hands EXECUTION to `rasm.runtime.recipe` `RecipeExecution.execute` per the folder charter: runtime owns execution, geometry owns which recipe runs with which typed inputs. `results` is the decode leg: the EnergyPlus `eplusout.sql` reads back through `ladybug` `SQLiteResult` — the ONLY EnergyPlus-results decode path — and the `honeybee_energy.result` parsers (`eui_from_sql` the EUI census), folding into SELF-DESCRIBING result frames crossing the data seam.

The crossing is fence-pinned per the data assessment-wire discipline: every result frame carries `output`, `unit`, `period`, `zone`, `step`, `value`, and `content_key` COLUMNS — a frame a consumer can attribute and dedupe with no side channel — and the physical crossing is content-keyed canonical Arrow bytes composed at the consumer edge through the data `tabular/columnar` public Arrow-bytes fold, never comment-flow and never a new intra-data egress import. The run identity chains: the model's HBJSON `ContentKey` seeds the simulation spec, the runtime recipe key covers the handled inputs, and the frame key covers the crossing bytes — the `Rasm.Persistence` reuse ledger dedupes at every tier, and the artifacts plane's chart/table suites consume the same self-describing frames downstream. Every entry is a producer on the folder evidence spine (`evidence_run`, `SimulationReceipt`, graduation under `"building-energy"` with the EUI evidence measured against caller ceilings). AGPL posture unchanged: function-local boundary imports, document bytes across the wire. This owner re-implements no OSM/IDF object mapping, no gbXML writer, no luigi scheduler, and no SQL schema parse — `honeybee-openstudio`, the runtime recipe owner, and `ladybug`/`honeybee-energy` own those kernels.

## [01]-[INDEX]

- [01]-[SIMULATE]: the one `Simulation` owner — the format-row `translate` offload with the in-process/CLI pair, the `sim_par` output-request builder, the runtime recipe binding, the `SQLiteResult`/`eui` decode into self-describing frames with the content-keyed Arrow crossing, the `SimulationReceipt`, and the `building-energy` graduation.

## [02]-[SIMULATE]

- Owner: `Simulation` — the frozen owner holding the admitted `BuildingModel`, the `LanePolicy` its CPU legs offload under, and the `RecipeExecution` handle execution routes through (constructed once at the composition edge from the caller's admission budget — never per call); `TranslateTarget` the closed format vocabulary (`OSM`/`IDF`/`EPJSON`/`GBXML`) keying the `WRITERS` row (the `honeybee_openstudio.writer` function name — a format is a row, never a parallel translator); `SimPar` the simulation-parameter request — reporting frequency, the requested-output families as boolean policy rows (`zone_energy`/`hvac_energy`/`comfort_metrics`/`unmet_hours`), run-period window, north angle, terrain — folded onto one `SimulationParameter` document, never a hand-stitched JSON; `RunSpec` the simulation request — the weather artifact path, the `SimPar`, and the `Option` extra recipe inputs — projected into the runtime `RecipeSpec` for the `annual-energy-use` catalog row; `ResultQuery` the closed decode union — `collections` (output names against the sql), `eui` (the end-use census), `tabular` (a named summary table), `outputs` (the available-output census the router reads first); `ResultFrame` the self-describing crossing carrier — the column-disciplined rows plus the frame `ContentKey`; `SimulationReceipt` the `ReceiptContributor` evidence.
- Cases: `WRITERS` rows `OSM -> model_to_osm` · `IDF -> model_to_idf` · `EPJSON -> model_to_epjson` · `GBXML -> model_to_gbxml`; `ResultQuery.collections` carries the output-name tuple (`available_outputs` is the discriminator a caller reads first — never a guessed name), `eui` carries no payload, `tabular` carries the table name, `outputs` carries no payload.
- Entry: `translate(target, folder)` probes the native SDK once (`find_spec("openstudio")`) inside the kernel — present, the `WRITERS` row's in-process writer serializes the honeybee `Model` to the format STRING (the writers take no folder — verified against the live distribution) and the kernel owns the durable artifact write into `folder`; absent, the fall-through builds the OSW (`to_openstudio_osw(folder, model_path)`) and runs the OpenStudio CLI (`run_osw(osw, measures_only=True)` — translation only, returning the `(osm, idf)` path pair the target selects from; the CLI pair serves OSM/IDF alone, so an EPJSON/GBXML request without the SDK is a typed fault naming the constraint, never a silently wrong artifact) — one translation concept, the SDK probe the selector, the whole kernel offloaded through `self.lane.offload(..., modality=Modality.THREAD)` because both legs are blocking (CPU-bound SDK translation, child-process CLI wait). `sim_par(spec)` builds the `SimulationParameter` — `SimulationOutput(reporting_frequency=...)` with the requested families added through their owned methods (`add_zone_energy_use`/`add_hvac_energy_use`/`add_comfort_metrics`/`add_unmet_hours` — request rows, never raw output strings), run period/north/terrain applied — and returns the document dict for the recipe's sim-par input. `simulate(run)` writes the model's durable `.hbjson` through `BuildingModel.hbjson`, projects the runtime `RecipeSpec(recipe=RecipeName.ANNUAL_ENERGY_USE, inputs={model, epw, sim-par, ...})`, and awaits `self.recipes.execute(spec)` — the runtime owner runs the engine gates, the handler coercion, the `queenbee local run` subprocess, and the luigi-evidence verdict; geometry receives the typed `RecipeProduct` back and never re-parses a log; `job()` projects the queenbee submission schema (`RecipeInterface` through the runtime `interface`, `Job`/`JobArgument` rows over it) for a consumer that submits rather than runs locally. `results(sql, query)` decodes: `collections` folds `data_collections_by_output_name(name)` per requested output into long-form rows — one row per (output, zone, step) with the `Header`'s unit/period and the collection metadata's zone identifier riding every row; `eui` folds `eui_from_sql` into (end-use, kWh/m2) rows; `tabular` folds `tabular_data_by_name`; `outputs` returns the census — every arm landing in a `ResultFrame` keyed by `ContentIdentity.key("energy-result", ...)` over its canonical bytes. `crossing(frame)` composes the CONSUMER-EDGE physical crossing: the deferred `pyarrow` table lifts the column-disciplined rows, the data `tabular/columnar` public Arrow-bytes fold serializes canonically, and the frame's `ContentKey` travels WITH the bytes — the artifacts chart/table suites and the C# decode read a frame that attributes and dedupes itself.
- Auto: every entry rides `evidence_run(EvidenceScope.ENERGY_SIMULATE, <operation>, dispatch)`; the translate offload carries no retry class (a deterministic translation owns no transiency — the runtime recipe owner already retries its own engine gate); `DetailedHVAC` models route through the OpenStudio measure path by construction (the in-process writer and the OSW pair both carry it; the pure-EnergyPlus IDF row rejects a `DetailedHVAC` model with a typed fault naming the constraint); the sql decode never guesses an output name — the `outputs` census is the router, and a requested name absent from the census is a typed fault naming the name and the census size; frame column discipline is load-bearing: `output`/`unit`/`period`/`zone`/`step`/`value`/`content_key` are THE columns, a wire consumer decodes by symbol, and a frame without its key column is the deleted form.
- Receipt: `SimulationReceipt` carries the operation, the translate target or recipe discriminant, the model `ContentKey`, the frame row/column counts, the `Option[float]` total-EUI evidence, and the frame `ContentKey`; `contribute` yields one emitted-phase `Receipt.of("rasm.geometry.energy.simulate", ("emitted", subject, facts))` triple — the receipts owner's `Evidence` shape, never the receipt struct itself; `graduates(key, ceiling)` folds under `GeometrySubject.BUILDING_ENERGY` — the measured fact is the total EUI (kWh/m2) against the caller's compliance ceiling, real building-physics evidence crossing to compute, never a row count.
- Packages: `honeybee-openstudio` (`writer.model_to_openstudio`/`model_to_osm`/`model_to_idf`/`model_to_epjson`/`model_to_gbxml` — the in-process leg over the native `openstudio` SDK (BSD), gated on the `find_spec` probe; every SDK class reaches code only through `honeybee_openstudio.openstudio` — AGPL posture on the wrapper, function-local imports), `honeybee-energy` (`run.to_openstudio_osw`/`run_osw` the CLI fall-through pair, `simulation.parameter.SimulationParameter`/`simulation.output.SimulationOutput` + the `add_*` request rows, `result.eui.eui_from_sql` — same posture), `ladybug-core` (`sql.SQLiteResult` — `available_outputs`/`data_collections_by_output_name`/`tabular_data_by_name`/`run_periods` — the ONLY EnergyPlus SQL decode path), `queenbee` (`Job` + `queenbee.io.inputs.job.JobArgument`/`JobPathArgument` constructed over the runtime `interface` projection — schema only), `pyarrow` (`Table.from_pydict` the frame lift — module-level-banned, deferred inside the crossing kernel), `expression` (`Block`/`Map`/`Option`/`tagged_union`), `msgspec` (`Struct` carriers, canonical json bytes), data (`tabular/columnar` public Arrow-bytes fold — the one canonical serialization the crossing composes at the consumer edge), geometry (`energy/model` `BuildingModel` downward, `evidence_run`/`EvidenceScope`/`GeometryHandoff`/`GeometrySubject`), runtime (`RecipeExecution`/`RecipeSpec`/`RecipeName`/`RecipeProduct` the execution seam, `LanePolicy`/`Modality` the offload, `ContentIdentity`/`ContentKey`, `RuntimeRail`/`BoundaryFault`, `Receipt`).
- Growth: a new translation format is one `WRITERS` row; a new requested-output family is one `SimPar` policy row over its `SimulationOutput.add_*` method; a new result decode is one `ResultQuery` case (`loadbalance`/`emissions`/`generation`/`component_sizes` are the named next rows over their `honeybee_energy.result` parsers); the daylight/comfort-map recipes ride the SAME `simulate` shape — one more `RecipeName` row in the runtime spec, their folder outputs already typed by the runtime handler readers, the `ladybug_comfort.map` kernels composing over those matrices through the climate owner's vocabulary; a cloud submission consumes `job()` against the Pollination API when a consumer names it.
- Boundary: no execution (runtime `execution/recipe` owns the subprocess, the engine gates, and the luigi verdict), no model semantics (`energy/model`), no weather algebra (`energy/climate`), no SQL schema parse (`SQLiteResult` owns it), no OSM/IDF object mapping (`honeybee-openstudio` owns it), and no intra-data egress import beyond the one public Arrow-bytes fold. The deleted forms: a blocking `model_to_openstudio` on the event loop where the lane offload owns the CPU leg; parallel `to_osm`/`to_idf` methods where one `translate` reads the `WRITERS` row; a second runner or a re-parsed luigi log where the runtime `RecipeProduct` is the verdict; a raw project folder returned where the handled outputs and typed frames are the product; a result frame missing its `unit`/`period`/`zone`/`content_key` columns (the C# decoder can neither attribute nor dedupe it — the fence-pinned floor); a guessed output name where the `outputs` census routes; a hand-stitched sim-par JSON where the `SimulationParameter` family owns assembly; and a fresh Arrow serialization where the data fold is the one canonical derivation.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable
from enum import StrEnum
from importlib.util import find_spec
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import Error, Nothing, Option, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from msgspec import json as msgjson

from rasm.data.tabular.columnar import arrow_bytes
from rasm.geometry.energy.model import BuildingModel
from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometrySubject, evidence_run
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.receipts import Receipt
from rasm.runtime.recipe import RecipeExecution, RecipeName, RecipeProduct, RecipeSpec

if TYPE_CHECKING:  # AGPL band: annotations resolve here; every runtime use is a function-local boundary import
    from queenbee.recipe.recipe import RecipeInterface

# --- [TYPES] ----------------------------------------------------------------------------


class TranslateTarget(StrEnum):
    OSM = "osm"
    IDF = "idf"
    EPJSON = "epjson"
    GBXML = "gbxml"


@tagged_union(frozen=True)
class ResultQuery:
    tag: Literal["collections", "eui", "tabular", "outputs"] = tag()
    collections: tuple[str, ...] = case()
    eui: None = case()
    tabular: str = case()
    outputs: None = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

# format -> honeybee_openstudio.writer function; one row per artifact format over the one core.
WRITERS: Final[Map[TranslateTarget, str]] = Map.of_seq([
    (TranslateTarget.OSM, "model_to_osm"),
    (TranslateTarget.IDF, "model_to_idf"),
    (TranslateTarget.EPJSON, "model_to_epjson"),
    (TranslateTarget.GBXML, "model_to_gbxml"),
])

FRAME_COLUMNS: Final[tuple[str, ...]] = ("output", "unit", "period", "zone", "step", "value", "content_key")

_ENCODER: Final = msgjson.Encoder(order="deterministic")  # one module-level canonical-bytes codec; never per-call construction

# --- [MODELS] ---------------------------------------------------------------------------


class SimPar(Struct, frozen=True, gc=False):
    reporting_frequency: str = "Hourly"
    zone_energy: bool = True
    hvac_energy: bool = True
    comfort_metrics: bool = False
    unmet_hours: bool = True
    north_angle: float = 0.0
    terrain: str = "City"
    run_period: Option[tuple[tuple[int, int], tuple[int, int]]] = Nothing  # ((st_month, st_day), (end_month, end_day)); Nothing = annual


class RunSpec(Struct, frozen=True):
    epw: Path
    sim_par: SimPar = SimPar()
    extra: Map[str, object] = Map.empty()  # additional annual-energy-use recipe inputs by name


class ResultFrame(Struct, frozen=True):
    columns: tuple[str, ...]
    rows: tuple[tuple[object, ...], ...]
    content_key: ContentKey


class SimulationReceipt(Struct, frozen=True):
    operation: str
    discriminant: str
    model_key: ContentKey
    rows: int
    eui_total: Option[float]
    frame_key: Option[ContentKey]

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of(
            "rasm.geometry.energy.simulate",
            (
                "emitted",
                f"{self.operation}:{self.discriminant}",
                {
                    "rows": self.rows,
                    "eui": self.eui_total.default_value(0.0),
                    "model_key": self.model_key.hex,
                    "frame_key": self.frame_key.map(lambda key: key.hex).default_value(""),
                },
            ),
        )

    def graduates(self, key: ContentKey, ceiling: float) -> GeometryHandoff:
        measured = self.eui_total.default_value(0.0)
        return GeometryHandoff.of(GeometrySubject.BUILDING_ENERGY, key, {"eui": measured, "rows": float(self.rows)}, {"eui": ceiling})


# --- [SERVICES] -------------------------------------------------------------------------


class Simulation(Struct, frozen=True):
    building: BuildingModel
    lane: LanePolicy
    recipes: RecipeExecution

    async def translate(self, target: TranslateTarget, folder: Path) -> "RuntimeRail[Path]":
        # in-process SDK leg when `openstudio` resolves, OSW+CLI fall-through otherwise — one
        # translation concept behind one probe; both legs block, so the kernel rides the lane.
        return (await self.lane.offload(_translated, self.building, target, folder, modality=Modality.THREAD)).bind(lambda rail: rail)

    def sim_par(self, spec: SimPar) -> "RuntimeRail[dict[str, object]]":
        def fold() -> dict[str, object]:
            from honeybee_energy.simulation.output import SimulationOutput  # noqa: PLC0415 — AGPL boundary import
            from honeybee_energy.simulation.parameter import SimulationParameter  # noqa: PLC0415
            from honeybee_energy.simulation.runperiod import RunPeriod  # noqa: PLC0415
            from ladybug.dt import Date  # noqa: PLC0415

            output = SimulationOutput(reporting_frequency=spec.reporting_frequency)
            requests = (
                (spec.zone_energy, output.add_zone_energy_use),
                (spec.hvac_energy, output.add_hvac_energy_use),
                (spec.comfort_metrics, output.add_comfort_metrics),
                (spec.unmet_hours, output.add_unmet_hours),
            )
            for wanted, add in requests:  # Exemption: SimulationOutput accumulates requests in place; the rows select its owned adders.
                if wanted:
                    add()
            window = spec.run_period.map(lambda period: RunPeriod(Date(*period[0]), Date(*period[1]))).to_optional()
            parameter = SimulationParameter(output=output, run_period=window, north_angle=spec.north_angle, terrain_type=spec.terrain)
            return parameter.to_dict()

        return evidence_run(EvidenceScope.ENERGY_SIMULATE, "sim_par", fold)

    async def simulate(self, run: RunSpec, folder: Path) -> "RuntimeRail[RecipeProduct]":
        # the recipe BINDING: geometry names the recipe and typed inputs; runtime owns execution,
        # handler coercion, engine gates, and the luigi verdict. The model key seeds the chain.
        written = self.building.hbjson(folder)
        staged = written.map2(
            self.sim_par(run.sim_par),
            lambda model_path, parameter: RecipeSpec(
                recipe=RecipeName.ANNUAL_ENERGY_USE,
                inputs=run.extra.add("model", str(model_path)).add("epw", str(run.epw)).add("sim-par", parameter),
            ),
        )
        return await staged.map(self.recipes.execute).default_with(_refused)

    async def job(self) -> "RuntimeRail[RecipeInterface]":
        return await self.recipes.interface(RecipeSpec(recipe=RecipeName.ANNUAL_ENERGY_USE))

    def results(self, sql: Path, query: ResultQuery) -> "RuntimeRail[ResultFrame]":
        def fold() -> ResultFrame:
            from honeybee_energy.result.eui import eui_from_sql  # noqa: PLC0415 — AGPL boundary import
            from ladybug.sql import SQLiteResult  # noqa: PLC0415

            reader = SQLiteResult(str(sql))
            census = tuple(reader.available_outputs)
            match query:
                case ResultQuery(tag="collections", collections=names):
                    missing = tuple(name for name in names if name not in census)
                    if missing:
                        raise ValueError(f"unknown-output:{missing}:{len(census)}")  # converted once by the evidence_run fence
                    rows = tuple(
                        (name, coll.header.unit, str(coll.header.analysis_period), coll.header.metadata.get("Zone", coll.header.metadata.get("System", "")), step, value)
                        for name in names
                        for coll in reader.data_collections_by_output_name(name)
                        for step, value in enumerate(coll.values)
                    )
                case ResultQuery(tag="eui", eui=None):
                    breakdown = eui_from_sql(str(sql))
                    rows = tuple((use, "kWh/m2", "annual", "", 0, value) for use, value in breakdown["end_uses"].items())
                case ResultQuery(tag="tabular", tabular=name):
                    rows = tuple((name, "", "", str(key), 0, value) for key, values in reader.tabular_data_by_name(name).items() for value in values)
                case ResultQuery(tag="outputs", outputs=None):
                    rows = tuple((name, "", "", "", 0, 0.0) for name in census)
                case _ as unreachable:
                    assert_never(unreachable)
            key = ContentIdentity.key("energy-result", _ENCODER.encode(rows))
            return ResultFrame(columns=FRAME_COLUMNS, rows=tuple((*row, key.hex) for row in rows), content_key=key)

        return evidence_run(EvidenceScope.ENERGY_SIMULATE, f"results.{query.tag}", fold)

    def crossing(self, frame: ResultFrame) -> "RuntimeRail[tuple[bytes, ContentKey]]":
        # the consumer-edge physical crossing: pyarrow lifts the column-disciplined rows and the
        # data columnar fold serializes the ONE canonical Arrow bytes the frame key travels with.
        def fold() -> tuple[bytes, ContentKey]:
            import pyarrow as pa  # noqa: PLC0415 — module-level import banned; deferred at the crossing edge

            table = pa.Table.from_pydict({name: [row[i] for row in frame.rows] for i, name in enumerate(frame.columns)})
            return bytes(arrow_bytes(table)), frame.content_key

        return evidence_run(EvidenceScope.ENERGY_SIMULATE, "crossing", fold)


# --- [OPERATIONS] -----------------------------------------------------------------------


async def _refused[T](fault: object) -> "RuntimeRail[T]":
    return Error(fault)


def _translated(building: BuildingModel, target: TranslateTarget, folder: Path) -> "RuntimeRail[Path]":
    # the offloaded kernel: SDK probe -> in-process writer row; no SDK -> OSW + OpenStudio CLI.
    def in_process() -> Path:
        # the in-process writers return the SERIALIZED DOCUMENT STRING (verified against the live
        # 0.5.5 distribution — no folder parameter); this kernel owns the durable artifact write.
        from importlib import import_module  # noqa: PLC0415 — row-resolved AGPL boundary import

        writer = getattr(import_module("honeybee_openstudio.writer"), WRITERS[target])
        artifact = folder / f"{building.model.identifier}.{target.value}"
        artifact.write_text(writer(building.model), encoding="utf-8")
        return artifact

    def cli() -> Path:
        from honeybee_energy.run import run_osw, to_openstudio_osw  # noqa: PLC0415 — AGPL boundary import

        if target not in (TranslateTarget.OSM, TranslateTarget.IDF):
            raise ValueError(f"cli-translate-unsupported:{target}:requires-openstudio-sdk")  # converted once by the evidence_run fence

        model_path = building.model.to_hbjson(name=building.model.identifier, folder=str(folder))
        osw = to_openstudio_osw(str(folder), model_path)
        osm, idf = run_osw(osw, measures_only=True)  # translation only — simulation is the recipe rail's, never this leg
        return Path(idf if target is TranslateTarget.IDF else osm)

    return evidence_run(EvidenceScope.ENERGY_SIMULATE, f"translate.{target}", in_process if find_spec("openstudio") else cli)
```
