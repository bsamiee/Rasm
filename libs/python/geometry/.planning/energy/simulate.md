# [PY_GEOMETRY_ENERGY_SIMULATE]

`Simulation` owns the simulation egress — where an admitted building model becomes engine input, a recipe run, and typed result frames. Three concerns, one owner, strict seams: `translate` is the OpenStudio translation pair — one in-process/subprocess concept, never parallel translators; `simulate` is the recipe BINDING — runtime owns execution, geometry owns which recipe runs with which typed inputs; `results` decodes `eplusout.sql` and the recipe's own product roster into SELF-DESCRIBING columnar frames crossing the data seam. `honeybee-openstudio`, the runtime recipe owner, and `ladybug`/`honeybee-energy` own every OSM/IDF object mapping, gbXML writer, luigi scheduler, and SQL schema parse.

Frame discipline is load-bearing: `FRAME_SCHEMA` is the one column-and-dtype correspondence, `FRAME_COLUMNS` derives from it, and a frame is a DECLARED roster beside sealed arrays — so the physical crossing is the data admitting pair — `tabular/columnar` `arrow_columns(columns, table)` then the `tabular/interop` `arrow_bytes` fold — with the transpose living at the producer where the roster is authored and no hand-rolled admission around the entry the data owner declares by name. Run identity chains: the model's HBJSON `ContentKey` seeds the simulation spec, the runtime recipe key covers the handled inputs, and the frame key covers the crossing bytes — the `Rasm.Persistence` reuse ledger dedupes at every tier, and the artifacts chart/table suites consume the same frames downstream. AGPL posture unchanged: function-local boundary imports, document bytes across the wire; evidence graduates under `GeometrySubject.BUILDING_ENERGY`.

## [01]-[INDEX]

- [02]-[SIMULATE]: one simulation owner — the parent-woven format-row translate offload, the recipe-parameterized runtime binding, the columnar result decode with the declared Arrow crossing — under one `SimulationReceipt`.

## [02]-[SIMULATE]

- Owner: `Simulation` holds the admitted `BuildingModel`, the `LanePolicy` its CPU legs offload under, the `RecipeExecution` handle, and the `composition` custody key every weave and bench stamps — constructed once at the composition edge, never per call. `TranslateTarget` keys the `WRITERS` row, whose values are the `energy/climate` `LateBound` values the standards loaders read, so a format is a row over one late-binding grammar rather than a parallel translator; `SimPar` folds onto one `SimulationParameter` document through the owned `add_*` request rows, never a hand-stitched JSON.
- Entry: `translate` probes the native SDK once (`find_spec("openstudio")`) — present, the in-process writer row; absent, the OSW + OpenStudio CLI fall-through, which serves OSM/IDF alone, so an EPJSON/GBXML request without the SDK is a typed fault naming the constraint, never a silently wrong artifact. The weave runs on the PARENT floor around `lane.offload`, so the crossing carries a span, a cost band, and an evidence row; the kernel itself is bare and raises into the lane's own fence. `simulate` hands execution to the runtime `RecipeExecution` — engine gates, handler coercion, the `queenbee local run` subprocess, the luigi verdict — and geometry receives the typed `RecipeProduct`, never re-parsing a log; `RunSpec.recipe` selects which catalog row runs, so annual energy, daylight, and the three comfort-map workflows ride the one shape. `job()` is queenbee schema only, zero execution — the submission document for a consumer submitting to the Pollination API rather than running locally. `results` is one polymorphic decode over `ResultQuery`, each case carrying its OWN source: the four EnergyPlus arms address a `.sql`, the `matrix` arm addresses a recipe product.
- Auto: `simulate`/`job` delegate to the runtime owner's own span and receipt — never a doubled page-level weave over the delegated leg; the translate crossing declares `idempotent=False`, dropping the `HOSTILE` trait's `WORKER` retry default — deterministic translation owns no transiency AND the kernel writes artifacts, so a worker death rails typed instead of re-running the write, while the runtime recipe owner retries its own engine gate; `DetailedHVAC` models route through the OpenStudio measure path by construction, and the pure-EnergyPlus IDF row rejects one with a typed fault; the `outputs` census is the router — a requested name absent from the census is the band's `EnergyFault.unknown_output` case carrying the missing names beside the census size, and a recipe output the product never resolved is `unresolved_output` at the `matrix` arm, one closed refusal family either way and never a guessed address; the CLI translate fall-through refuses its unserved target through the same family, so a consumer matches one tag rather than parsing three coordinate strings.
- Receipt: `results` returns `(frame, receipt)` built inside the fold off the frame it just sealed, so no caller hand-asserts a row count or a frame key. `spec` is the evidence subject — the model key beside the recipe and the query — and `graduates` derives its own `ContentKey` from it. Total EUI is the measured graduation fact against the caller's compliance ceiling, and only the `eui` arm measures it: every other query OMITS the key so the spine reports `unmeasured:eui` and refuses honestly rather than crossing clean on a zero that reads as a zero-energy building. That same arm records the total onto the `rasm.geometry.energy.eui` charter distribution at the producing fold.
- Packages: `honeybee-openstudio` wraps the BSD `openstudio` SDK behind the `find_spec` gate; `honeybee-energy` carries the CLI pair, the `SimulationParameter` family, and the result parsers; ladybug `SQLiteResult` is the ONLY EnergyPlus SQL decode path; `queenbee` is schema only; `numpy` seals the frame columns and the data `arrow_columns` (columnar) / `arrow_bytes` (interop) pair is the one serialization, so no `pyarrow` symbol appears on this page at all.
- Growth: a new translation format is one `WRITERS` row; a new output family one `SimPar` policy row over its `add_*` method; a new result decode is one `ResultQuery` case — `loadbalance`/`emissions`/`generation`/`component_sizes` the named next rows over their `honeybee_energy.result` parsers; a new workflow is one `RunSpec.recipe` value over the runtime catalog, zero page edits; a cloud submission consumes `job()` when a consumer names it.
- Boundary: execution is the runtime `execution/recipe` owner's; model semantics are `energy/model`'s, weather algebra `energy/climate`'s; a result frame whose table diverges from `FRAME_SCHEMA` is the deleted form — the C# decoder can neither attribute nor dedupe it.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable
from enum import StrEnum
from functools import partial
from importlib.util import find_spec
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, assert_never

import numpy as np
from builtins import frozendict
from expression import Error, Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from msgspec import json as msgjson

from rasm.data.tabular.columnar import arrow_columns
from rasm.data.tabular.interop import arrow_bytes
from rasm.geometry.energy.climate import EnergyFault, LateBound
from rasm.geometry.energy.model import BuildingModel
from rasm.geometry.graduation import (
    EvidenceScope,
    GeometryHandoff,
    GeometrySubject,
    charter_record,
    evidence_key,
    evidence_run,
)
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey
from rasm.runtime.recipe import RECIPES, RecipeExecution, RecipeName, RecipeProduct, RecipeSpec
from rasm.runtime.workers import Kernel, KernelTrait

if TYPE_CHECKING:  # AGPL band: annotations resolve here; every runtime use is a function-local or LateBound seam
    from queenbee.job.job import Job
    from queenbee.recipe.recipe import RecipeInterface

# --- [TYPES] ----------------------------------------------------------------------------


type Row = tuple[object, ...]  # one positional row in FRAME_COLUMNS order, minus the trailing content-key stamp


class TranslateTarget(StrEnum):
    OSM = "osm"
    IDF = "idf"
    EPJSON = "epjson"
    GBXML = "gbxml"


@tagged_union(frozen=True)
class ResultQuery:
    # each case carries its own source: the four EnergyPlus arms address one `.sql`, the matrix arm addresses the
    # recipe product whose declared outputs the comfort-map kernels read.
    tag: Literal["collections", "eui", "tabular", "outputs", "matrix"] = tag()
    collections: tuple[Path, tuple[str, ...]] = case()
    eui: Path = case()
    tabular: tuple[Path, str] = case()
    outputs: Path = case()
    matrix: tuple[RecipeName, RecipeProduct] = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

# format -> honeybee_openstudio.writer member as a LateBound row: one late-binding grammar with the climate comfort
# rows and the model standards loaders, so a format is a row and the page holds no getattr fold of its own.
WRITERS: Final[Map[TranslateTarget, LateBound]] = Map.of_seq([
    (TranslateTarget.OSM, LateBound("honeybee_openstudio.writer", "model_to_osm")),
    (TranslateTarget.IDF, LateBound("honeybee_openstudio.writer", "model_to_idf")),
    (TranslateTarget.EPJSON, LateBound("honeybee_openstudio.writer", "model_to_epjson")),
    (TranslateTarget.GBXML, LateBound("honeybee_openstudio.writer", "model_to_gbxml")),
])

# the ONE column-and-dtype correspondence every frame seals against; the roster derives from it, so a new column is one
# row here and the crossing, the receipt, and the C# decoder all read the same declaration.
FRAME_SCHEMA: Final[frozendict[str, str]] = frozendict({
    "output": "U",
    "unit": "U",
    "period": "U",
    "zone": "U",
    "step": "i8",
    "value": "f8",
    "content_key": "U",
})
FRAME_COLUMNS: Final[tuple[str, ...]] = tuple(FRAME_SCHEMA)

# a roster row addresses an artifact and measures nothing, so its float cell spells absence in the column's own
# vocabulary rather than a zero a chart reads as a real reading.
_UNMEASURED: Final[float] = float("nan")

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
    recipe: RecipeName = RecipeName.ANNUAL_ENERGY_USE  # the runtime catalog row this run binds; comfort-map and daylight rows ride the same shape
    sim_par: SimPar = SimPar()
    extra: Map[str, object] = Map.empty()  # additional recipe inputs by name


class ResultFrame(Struct, frozen=True):
    # columnar by construction: the declared roster beside one sealed array per column, which is exactly the shape the
    # data tier's `arrow_columns` entry admits — a row-major carrier would force a transpose at the consumer edge.
    columns: tuple[str, ...]
    table: frozendict[str, np.ndarray]
    content_key: ContentKey

    @property
    def rows(self) -> int:
        return int(next(iter(self.table.values())).shape[0]) if self.table else 0


class SimulationReceipt(Struct, frozen=True):
    operation: str
    discriminant: str
    model_key: ContentKey
    recipe: str
    rows: int
    frame_key: ContentKey
    eui_total: Option[float] = Nothing

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of(
            "rasm.geometry.energy.simulate",
            (
                "emitted",
                f"{self.operation}:{self.discriminant}",
                {
                    "rows": self.rows,
                    "recipe": self.recipe,
                    "model_key": self.model_key.hex,
                    "frame_key": self.frame_key.hex,
                    # a decode that ran no EUI parse OMITS the key: a zero here reads as a zero-energy building.
                    **self.eui_total.map(lambda total: {"eui": total}).default_value({}),
                },
            ),
        )

    def spec(self) -> bytes:
        # the evidence subject IS the admitted model beside the recipe and the query that read its outputs, so two
        # decodes over one run key apart and an identical re-decode dedupes in the persistence ledger.
        return b"|".join((self.model_key.memory, self.recipe.encode(), self.operation.encode(), self.discriminant.encode()))

    def graduates(self, ceiling: float) -> GeometryHandoff:
        # EUI is the eui arm's own measurement; every other decode OMITS it, so the spine reports `unmeasured:eui`
        # and the crossing refuses rather than clearing a compliance ceiling on a fabricated zero.
        measured = self.eui_total.map(lambda total: {"eui": total}).default_value({}) | {"rows": float(self.rows)}
        subject = GeometrySubject.BUILDING_ENERGY
        return GeometryHandoff.of(subject, evidence_key(subject, self.spec()), measured, {"eui": ceiling})


# --- [SERVICES] -------------------------------------------------------------------------


class Simulation(Struct, frozen=True):
    building: BuildingModel
    lane: LanePolicy
    recipes: RecipeExecution
    composition: ScopeKey = DEFAULT_SCOPE

    async def translate(self, target: TranslateTarget, folder: Path) -> "RuntimeRail[Path]":
        # the weave wraps the OFFLOAD on the parent floor, where the cost bracket, the charter cost rows, and the
        # evidence row are live — a weave inside the kernel meters a worker whose recorder is a no-op. HOSTILE because
        # the SDK writer leg loads native openstudio in-process; idempotent=False keeps a worker-death retry from
        # re-running the artifact write, so a death rails typed instead.
        return await evidence_run(
            EvidenceScope.ENERGY_SIMULATE,
            f"translate.{target}",
            partial(self.lane.offload, Kernel.of(_translated, KernelTrait.HOSTILE, idempotent=False), self.building, target, folder),
            composition=self.composition,
        )

    def sim_par(self, spec: SimPar) -> "RuntimeRail[dict[str, object]]":
        def fold() -> dict[str, object]:
            from honeybee_energy.simulation.output import SimulationOutput  # ruff:ignore[import-outside-top-level] — AGPL boundary import
            from honeybee_energy.simulation.parameter import SimulationParameter  # ruff:ignore[import-outside-top-level]
            from honeybee_energy.simulation.runperiod import RunPeriod  # ruff:ignore[import-outside-top-level]
            from ladybug.dt import Date  # ruff:ignore[import-outside-top-level]

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

        return evidence_run(EvidenceScope.ENERGY_SIMULATE, "sim_par", fold, composition=self.composition)

    async def simulate(self, run: RunSpec, folder: Path) -> "RuntimeRail[RecipeProduct]":
        written = self.building.hbjson(folder)
        staged = written.map2(
            self.sim_par(run.sim_par),
            lambda model_path, parameter: RecipeSpec(
                recipe=run.recipe,
                inputs=run.extra.add("model", str(model_path)).add("epw", str(run.epw)).add("sim-par", parameter),
            ),
        )
        return await staged.map(self.recipes.execute).default_with(_refused)

    async def job(self, run: RunSpec, model: Path, source: str) -> "RuntimeRail[Job]":
        # `source` is the caller-supplied recipe source (the Pollination registry reference).
        projected = await self.recipes.interface(RecipeSpec(recipe=run.recipe))
        return projected.map(lambda interface: _job(interface, run, model, source))

    def results(self, query: ResultQuery) -> "RuntimeRail[tuple[ResultFrame, SimulationReceipt]]":
        def fold() -> tuple[ResultFrame, SimulationReceipt]:
            rows, discriminant, recipe, eui = _decoded(query)
            # charter row at the producing fold under the owner's own composition: total EUI is the end-use sum by
            # definition, so no second parser member is claimed and a decode that measured none records nothing.
            eui.map(lambda total: charter_record(GeometrySubject.BUILDING_ENERGY, {"eui_total": total}, composition=self.composition))
            frame = _tabled(rows)
            return frame, self._receipt(f"results.{query.tag}", discriminant, recipe, frame, eui)

        return evidence_run(EvidenceScope.ENERGY_SIMULATE, f"results.{query.tag}", fold, composition=self.composition)

    def crossing(self, frame: ResultFrame) -> "RuntimeRail[tuple[bytes, ContentKey]]":
        # the ONE admitting entry the data owner declares for this carrier by name: a declared roster beside sealed
        # arrays in, canonical IPC stream bytes out, no transposition and no `pyarrow` symbol on this page.
        return evidence_run(
            EvidenceScope.ENERGY_SIMULATE,
            "crossing",
            lambda: (bytes(arrow_bytes(arrow_columns(frame.columns, dict(frame.table)))), frame.content_key),
            composition=self.composition,
        )

    def _receipt(self, operation: str, discriminant: str, recipe: str, frame: ResultFrame, eui: Option[float]) -> SimulationReceipt:
        return SimulationReceipt(
            operation=operation,
            discriminant=discriminant,
            model_key=self.building.content_key,
            recipe=recipe,
            rows=frame.rows,
            frame_key=frame.content_key,
            eui_total=eui,
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


async def _refused[T](fault: object) -> "RuntimeRail[T]":
    return Error(fault)


def _addresses(value: object) -> tuple[str, ...]:
    # a recipe output resolves to one artifact address or a list of them (one per sensor grid); the roster reads both
    # regimes without a per-recipe branch.
    return tuple(str(item) for item in value) if isinstance(value, (list, tuple)) else (str(value),)


def _decoded(query: ResultQuery) -> tuple[tuple[Row, ...], str, str, Option[float]]:
    # one total decode over the query union, each arm opening only the reader it needs and returning its rows beside
    # the receipt discriminant, the recipe it read, and the EUI the eui arm alone measures.
    from honeybee_energy.result.eui import eui_from_sql  # ruff:ignore[import-outside-top-level] — AGPL boundary import
    from ladybug.sql import SQLiteResult  # ruff:ignore[import-outside-top-level]

    match query:
        case ResultQuery(tag="collections", collections=(sql, names)):
            reader = SQLiteResult(str(sql))
            census = tuple(reader.available_outputs)
            missing = tuple(name for name in names if name not in census)
            if missing:
                # the absent names and the census size ride as kwargs, so a consumer reads WHICH outputs the SQL lacks.
                raise EnergyFault(unknown_output=(missing, len(census)))
            rows = tuple(
                (
                    name,
                    coll.header.unit,
                    str(coll.header.analysis_period),
                    coll.header.metadata.get("Zone", coll.header.metadata.get("System", "")),
                    step,
                    value,
                )
                for name in names
                for coll in reader.data_collections_by_output_name(name)
                for step, value in enumerate(coll.values)
            )
            return rows, ";".join(names), RecipeName.ANNUAL_ENERGY_USE.value, Nothing
        case ResultQuery(tag="eui", eui=sql):
            breakdown = eui_from_sql(str(sql))
            rows = tuple((use, "kWh/m2", "annual", "", 0, value) for use, value in breakdown["end_uses"].items())
            # total EUI is the end-use sum by definition, so no second parser member is claimed; the caller-floor fold
            # records it onto the charter and the receipt, and a decode that took no EUI pass carries `Nothing`.
            return rows, "end-uses", RecipeName.ANNUAL_ENERGY_USE.value, Some(sum(float(row[5]) for row in rows))
        case ResultQuery(tag="tabular", tabular=(sql, name)):
            reader = SQLiteResult(str(sql))
            rows = tuple((name, "", "", str(key), 0, value) for key, values in reader.tabular_data_by_name(name).items() for value in values)
            return rows, name, RecipeName.ANNUAL_ENERGY_USE.value, Nothing
        case ResultQuery(tag="outputs", outputs=sql):
            census = tuple(SQLiteResult(str(sql)).available_outputs)
            return tuple((name, "", "", "", 0, _UNMEASURED) for name in census), f"census:{len(census)}", RecipeName.ANNUAL_ENERGY_USE.value, Nothing
        case ResultQuery(tag="matrix", matrix=(recipe, product)):
            # the recipe row's OWN declared outputs are the roster: an output the product never resolved is the same
            # refusal an unknown SQL output is, so a comfort-map consumer never receives a guessed address.
            declared = RECIPES[recipe].outputs
            missing = tuple(name for name in declared if product.outputs.try_find(name).is_none())
            if missing:
                # same refusal class as an unknown SQL output, so a comfort-map consumer matches one tag either way.
                raise EnergyFault(unresolved_output=(recipe.value, missing))
            rows = tuple(
                (name, "", "", address, ordinal, _UNMEASURED)
                for name in declared
                for ordinal, address in enumerate(_addresses(product.outputs[name]))
            )
            return rows, recipe.value, recipe.value, Nothing
        case _ as unreachable:
            assert_never(unreachable)


def _tabled(rows: tuple[Row, ...]) -> ResultFrame:
    # the transpose lands at the PRODUCER, where the roster is authored: each arm emits positional rows in
    # FRAME_COLUMNS order, the key covers the canonical row bytes, and this fold seals one dtype-declared array per
    # column — so the data tier receives the declared roster it schemas from and never the mapping's insertion order.
    key = ContentIdentity.key("energy-result", _ENCODER.encode(rows))
    stamped = tuple((*row, key.hex) for row in rows)
    columns = tuple(zip(*stamped, strict=True)) if stamped else ((),) * len(FRAME_COLUMNS)
    return ResultFrame(
        columns=FRAME_COLUMNS,
        table=frozendict({name: np.asarray(cells, dtype=FRAME_SCHEMA[name]) for name, cells in zip(FRAME_COLUMNS, columns, strict=True)}),
        content_key=key,
    )


def _job(interface: "RecipeInterface", run: RunSpec, model: Path, source: str) -> "Job":
    # v1beta1 Jobs carry ONE inner argument list (one parametric run), artifact paths as ProjectFolder sources.
    from queenbee.io.artifact_source import ProjectFolder  # ruff:ignore[import-outside-top-level] — AGPL band boundary import
    from queenbee.io.inputs.job import JobArgument, JobPathArgument  # ruff:ignore[import-outside-top-level]
    from queenbee.job.job import Job  # ruff:ignore[import-outside-top-level]

    run_arguments = [
        JobPathArgument(name="model", source=ProjectFolder(path=str(model))),
        JobPathArgument(name="epw", source=ProjectFolder(path=str(run.epw))),
        *(JobArgument(name=name, value=str(value)) for name, value in run.extra.items()),
    ]
    return Job(source=source, arguments=[run_arguments], name=interface.metadata.name)


def _translated(building: BuildingModel, target: TranslateTarget, folder: Path) -> Path:
    # bare kernel: the parent weave owns the span and the band, and both legs raise into the lane's async_boundary.
    def in_process() -> Path:
        # in-process writers return the serialized document string and take no folder parameter; this kernel owns the artifact write.
        artifact = folder / f"{building.model.identifier}.{target.value}"
        artifact.write_text(WRITERS[target].resolve()(building.model), encoding="utf-8")
        return artifact

    def cli() -> Path:
        from honeybee_energy.run import run_osw, to_openstudio_osw  # ruff:ignore[import-outside-top-level] — AGPL boundary import

        if target not in (TranslateTarget.OSM, TranslateTarget.IDF):
            # the CLI leg serves OSM/IDF alone; the target and its constraint ride the case, converted by the lane's own fence.
            raise EnergyFault(unsupported_target=(target.value, "requires-openstudio-sdk"))

        model_path = building.model.to_hbjson(name=building.model.identifier, folder=str(folder))
        osw = to_openstudio_osw(str(folder), model_path)
        osm, idf = run_osw(osw, measures_only=True)  # translation only — simulation is the recipe rail's, never this leg
        return Path(idf if target is TranslateTarget.IDF else osm)

    return in_process() if find_spec("openstudio") else cli()
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- MATRIX_ADDRESS-[OPEN]: does `RecipeProduct.outputs` carry a comfort-map recipe's `tcp`/`condition` products as one folder address or as a per-sensor-grid list, and does the runtime `output_value_by_name` read return a path string or a loaded value; read the `lbt-recipes` comfort-map recipe `package.json` output contracts against the runtime `execution/recipe` readback fold, then pin `_addresses` against the proven shape.

