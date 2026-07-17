# [PY_GEOMETRY_ENERGY_SIMULATE]

`Simulation` owns the simulation egress — where an admitted building model becomes engine input, a recipe run, and typed result frames. Three concerns, one owner, strict seams: `translate` is the OpenStudio translation pair — one in-process/subprocess concept, never parallel translators; `simulate` is the recipe BINDING — runtime owns execution, geometry owns which recipe runs with which typed inputs; `results` decodes `eplusout.sql` into SELF-DESCRIBING frames crossing the data seam. `honeybee-openstudio`, the runtime recipe owner, and `ladybug`/`honeybee-energy` own every OSM/IDF object mapping, gbXML writer, luigi scheduler, and SQL schema parse.

Frame column discipline is load-bearing: `output`/`unit`/`period`/`zone`/`step`/`value`/`content_key` are THE columns, so a wire consumer attributes and dedupes with no side channel. Physical crossing is content-keyed canonical Arrow bytes composed at the consumer edge through the data `tabular/columnar` public Arrow-bytes fold, never comment-flow and never a new intra-data egress import. Run identity chains: the model's HBJSON `ContentKey` seeds the simulation spec, the runtime recipe key covers the handled inputs, and the frame key covers the crossing bytes — the `Rasm.Persistence` reuse ledger dedupes at every tier, and the artifacts chart/table suites consume the same frames downstream. AGPL posture unchanged: function-local boundary imports, document bytes across the wire; evidence graduates under `GeometrySubject.BUILDING_ENERGY`.

## [01]-[INDEX]

- [01]-[SIMULATE]: one simulation owner — the format-row translate offload, the runtime recipe binding, the self-describing result decode with the Arrow crossing — under one `SimulationReceipt`.

## [02]-[SIMULATE]

- Owner: `Simulation` holds the admitted `BuildingModel`, the `LanePolicy` its CPU legs offload under, and the `RecipeExecution` handle — constructed once at the composition edge, never per call. `TranslateTarget` keys the `WRITERS` row, so a format is a row, never a parallel translator; `SimPar` folds onto one `SimulationParameter` document through the owned `add_*` request rows, never a hand-stitched JSON.
- Entry: `translate` probes the native SDK once (`find_spec("openstudio")`) — present, the in-process writer row; absent, the OSW + OpenStudio CLI fall-through, which serves OSM/IDF alone, so an EPJSON/GBXML request without the SDK is a typed fault naming the constraint, never a silently wrong artifact; the SDK leg loads the native-hostile `openstudio` band in-process, so the whole kernel crosses as `Kernel.of(_translated, KernelTrait.HOSTILE, idempotent=False)` onto the warm process pool, never the event loop and never a thread arm whose trait the SDK load falsifies. `simulate` hands execution to the runtime `RecipeExecution` — engine gates, handler coercion, the `queenbee local run` subprocess, the luigi verdict — and geometry receives the typed `RecipeProduct`, never re-parsing a log. `job()` is queenbee schema only, zero execution — the submission document for a consumer submitting to the Pollination API rather than running locally.
- Auto: `simulate`/`job` delegate to the runtime owner's own span and receipt — never a doubled page-level weave over the delegated leg; the translate crossing declares `idempotent=False`, dropping the `HOSTILE` trait's `WORKER` retry default — deterministic translation owns no transiency AND the kernel writes artifacts, so a worker death rails typed instead of re-running the write, while the runtime recipe owner retries its own engine gate; `DetailedHVAC` models route through the OpenStudio measure path by construction, and the pure-EnergyPlus IDF row rejects one with a typed fault; the `outputs` census is the router — a requested name absent from the census is a typed fault naming the name and the census size, never a guessed output.
- Receipt: measured graduation fact is the total EUI (kWh/m2) against the caller's compliance ceiling — real building-physics evidence crossing to compute, never a row count.
- Packages: `honeybee-openstudio` wraps the BSD `openstudio` SDK behind the `find_spec` gate; `honeybee-energy` carries the CLI pair, the `SimulationParameter` family, and the result parsers; ladybug `SQLiteResult` is the ONLY EnergyPlus SQL decode path; `queenbee` is schema only; `pyarrow` is module-level-banned and defers inside the crossing kernel; the data `tabular/columnar` Arrow-bytes fold is the one canonical serialization, composed at the consumer edge.
- Growth: a new translation format is one `WRITERS` row; a new output family one `SimPar` policy row over its `add_*` method; a new result decode is one `ResultQuery` case — `loadbalance`/`emissions`/`generation`/`component_sizes` the named next rows over their `honeybee_energy.result` parsers; daylight/comfort-map recipes ride the SAME `simulate` shape — one more `RecipeName` row, the `ladybug_comfort.map` kernels composing over those matrices through the climate owner's vocabulary; a cloud submission consumes `job()` when a consumer names it.
- Boundary: execution is the runtime `execution/recipe` owner's; model semantics are `energy/model`'s, weather algebra `energy/climate`'s; a result frame missing its `unit`/`period`/`zone`/`content_key` columns is the deleted form — the C# decoder can neither attribute nor dedupe it.

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
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt
from rasm.runtime.recipe import RecipeExecution, RecipeName, RecipeProduct, RecipeSpec
from rasm.runtime.workers import Kernel, KernelTrait

if TYPE_CHECKING:  # AGPL band: annotations resolve here; every runtime use is a function-local boundary import
    from queenbee.job.job import Job
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
        # HOSTILE: the SDK writer leg loads native openstudio in-process; idempotent=False keeps a worker-death
        # retry from re-running the artifact write, so a death rails typed instead.
        kernel = Kernel.of(_translated, KernelTrait.HOSTILE, idempotent=False)
        return (await self.lane.offload(kernel, self.building, target, folder)).bind(lambda rail: rail)

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
        written = self.building.hbjson(folder)
        staged = written.map2(
            self.sim_par(run.sim_par),
            lambda model_path, parameter: RecipeSpec(
                recipe=RecipeName.ANNUAL_ENERGY_USE,
                inputs=run.extra.add("model", str(model_path)).add("epw", str(run.epw)).add("sim-par", parameter),
            ),
        )
        return await staged.map(self.recipes.execute).default_with(_refused)

    async def job(self, run: RunSpec, model: Path, source: str) -> "RuntimeRail[Job]":
        # `source` is the caller-supplied recipe source (the Pollination registry reference).
        projected = await self.recipes.interface(RecipeSpec(recipe=RecipeName.ANNUAL_ENERGY_USE))
        return projected.map(lambda interface: _job(interface, run, model, source))

    def receipt(
        self,
        operation: str,
        discriminant: str,
        rows: int,
        eui_total: Option[float] = Nothing,
        frame_key: Option[ContentKey] = Nothing,
    ) -> SimulationReceipt:
        return SimulationReceipt(
            operation=operation,
            discriminant=discriminant,
            model_key=self.building.content_key,
            rows=rows,
            eui_total=eui_total,
            frame_key=frame_key,
        )

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
        def fold() -> tuple[bytes, ContentKey]:
            import pyarrow as pa  # noqa: PLC0415 — module-level import banned; deferred at the crossing edge

            table = pa.Table.from_pydict({name: [row[i] for row in frame.rows] for i, name in enumerate(frame.columns)})
            return bytes(arrow_bytes(table)), frame.content_key

        return evidence_run(EvidenceScope.ENERGY_SIMULATE, "crossing", fold)


# --- [OPERATIONS] -----------------------------------------------------------------------


async def _refused[T](fault: object) -> "RuntimeRail[T]":
    return Error(fault)


def _job(interface: "RecipeInterface", run: RunSpec, model: Path, source: str) -> "Job":
    # v1beta1 Jobs carry ONE inner argument list (one parametric run), artifact paths as ProjectFolder sources.
    from queenbee.io.artifact_source import ProjectFolder  # noqa: PLC0415 — AGPL band boundary import
    from queenbee.io.inputs.job import JobArgument, JobPathArgument  # noqa: PLC0415
    from queenbee.job.job import Job  # noqa: PLC0415

    run_arguments = [
        JobPathArgument(name="model", source=ProjectFolder(path=str(model))),
        JobPathArgument(name="epw", source=ProjectFolder(path=str(run.epw))),
        *(JobArgument(name=name, value=str(value)) for name, value in run.extra.items()),
    ]
    return Job(source=source, arguments=[run_arguments], name=interface.metadata.name)


def _translated(building: BuildingModel, target: TranslateTarget, folder: Path) -> "RuntimeRail[Path]":
    def in_process() -> Path:
        # in-process writers return the serialized document string and take no folder parameter; this kernel owns the artifact write.
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

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
