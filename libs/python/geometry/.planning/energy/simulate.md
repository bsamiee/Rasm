# [PY_GEOMETRY_ENERGY_SIMULATE]

`Simulation` owns the simulation egress — where an admitted building model becomes engine input, a recipe run, and typed result frames. Three concerns, one owner, strict seams: `translate` is the OpenStudio translation pair — one in-process/subprocess concept, never parallel translators; `simulate` is the recipe BINDING — runtime owns execution, geometry owns which recipe runs with which typed inputs; `results` decodes `eplusout.sql` and the recipe's own product roster into SELF-DESCRIBING columnar frames crossing the data seam. `honeybee-openstudio`, the runtime recipe owner, and `ladybug`/`honeybee-energy` own every OSM/IDF object mapping, gbXML writer, luigi scheduler, and SQL schema parse. The AGPL-3.0 band those names carry rides the standing companion-lane charter, and its import site is a LICENSE fact rather than a cost one: a static audit reads the LEXICAL import graph, so any module-scope binding — `lazy` included, the soft keyword being module-scope by design — would mark every importer of this module AGPL-coupled. Each band name therefore stays inside its boundary seam under a terse `AGPL isolation` marker, while a permissively-licensed native takes the deferral dialect instead.

Frame discipline is load-bearing: `FRAME_SCHEMA` is the one column-and-dtype correspondence, `FRAME_COLUMNS` derives from it, and a frame is a DECLARED roster beside sealed arrays — so the physical crossing is the data admitting pair — `tabular/columnar` `arrow_columns(columns, table)` then the `tabular/interop` `arrow_bytes` fold — with the transpose living at the producer where the roster is authored and no hand-rolled admission around the entry the data owner declares by name. Run identity chains: the model's HBJSON `ContentKey` seeds the simulation spec, the runtime recipe key covers the handled inputs, and the frame key covers the crossing bytes — the `Rasm.Persistence` reuse ledger dedupes at every tier, and the artifacts chart/table suites consume the same frames downstream. AGPL posture unchanged: every band binding stays a function-local boundary import, because a license audit reads the LEXICAL import graph — a module-scope binding marks every importer of this module AGPL-coupled, and a `lazy` statement is module-scope by design, so the deferred dialect cannot serve this ban — while the function-local form confines that coupling to the seam function itself; document bytes across the wire; evidence graduates under `GeometrySubject.BUILDING_ENERGY`. Permissively licensed natives carry no such ban and bind module-scope `lazy` as everywhere else, `trimesh` the one on this page.

## [01]-[INDEX]

- [02]-[SIMULATE]: one simulation owner — the parent-woven format-row translate offload, the recipe-parameterized runtime binding, the columnar result decode with the declared Arrow crossing, and the captured-scene descriptor decode — under one `SimulationReceipt`.

## [02]-[SIMULATE]

- Owner: `Simulation` holds the admitted `BuildingModel`, the `LanePolicy` its CPU legs offload under, the `RecipeExecution` handle, and the `composition` custody key every weave and bench stamps — constructed once at the composition edge, never per call. `TranslateTarget` keys the `WRITERS` row, whose values are the `energy/climate` `LateBound` values the standards loaders read, so a format is a row over one late-binding grammar rather than a parallel translator; `SimPar` folds onto one `SimulationParameter` document through the owned `add_*` request rows, never a hand-stitched JSON.
- Law: fidelity GRADES, never guesses — the shared generated `TessellationPolicy` declares the linear deflection, angular tolerance, and triangle budget the capture used; `_graded` refuses `shading_fidelity` when the declared linear deflection crosses the consuming model's own tolerance or the delivered triangle count crosses the declared budget. `UNITS_TOLERANCES` is honeybee's own per-unit floor and this page reads it rather than pinning a literal.
- Law: `authored` sun and `sited` sun answer two different questions — a manual-control sun carries angles and no site, so a point-in-time sky admits it and any recipe demanding a `Site:Location` refuses `authored_sun` by name instead of back-solving coordinates from an altitude and an azimuth.
- Law: irradiance never rides the descriptor — the host document holds no `W/m2`, so `sky` mints the angle-only CIE arm by default and the climate-based arm only where the caller hands the EPW's own direct-normal and diffuse-horizontal pair; a sky synthesized from `intensity_scale` fabricates radiation.
- Law: incomplete and wrong split — a photometric web no consuming engine reads and a light whose power carries `relative-scale` both COUNT onto the receipt and neither refuses, since a daylight study needs neither, while a coarse mesh and a siteless annual run both make results wrong and refuse.
- Entry: `translate` probes the native SDK once (`find_spec("openstudio")`) — present, the in-process writer row; absent, the OSW + OpenStudio CLI fall-through, which serves OSM/IDF alone, so an EPJSON/GBXML request without the SDK is a typed fault naming the constraint, never a silently wrong artifact. The weave runs on the PARENT floor around `lane.offload`, so the crossing carries a span, a cost band, and an evidence row; the kernel itself is bare and raises into the lane's own fence. `simulate` hands execution to the runtime `RecipeExecution` — engine gates, handler coercion, the `queenbee local run` subprocess, the luigi verdict — and geometry receives the typed `RecipeProduct`, never re-parsing a log; `RunSpec.recipe` selects which catalog row runs, so annual energy, daylight, and the three comfort-map workflows ride the one shape. `job()` is queenbee schema only, zero execution — the submission document for a consumer submitting to the Pollination API rather than running locally. `results` is one polymorphic decode over `ResultQuery`, each case carrying its OWN source: the four EnergyPlus arms address a `.sql`, the `matrix` arm addresses a recipe product. `scene` resolves `SceneDescriptor.shading.artifact` through the injected `ArtifactTransfer`, holds its verified `OwnedArtifact` only while the path-based GLB decode runs, and returns one `SceneContext` of engine-ready values. Sun angles project STRAIGHT onto the sky, never back through `Sunpath`: the producer solved them on the kernel almanac and a second ephemeris here answers a different number for the same instant. Shading becomes `ShadeMesh` rows over `Mesh3D`, which is the population `Model.shade_meshes` holds and `Model.shades` never counts — the two are disjoint, so a receipt reading `len(model.shades)` reports zero context on a fully contextualized model.
- Auto: `simulate`/`job` delegate to the runtime owner's own span and receipt — never a doubled page-level weave over the delegated leg; the translate crossing declares `idempotent=False`, dropping the `HOSTILE` trait's `WORKER` retry default — deterministic translation owns no transiency AND the kernel writes artifacts, so a worker death rails typed instead of re-running the write, while the runtime recipe owner retries its own engine gate; `DetailedHVAC` models route through the OpenStudio measure path by construction, and the pure-EnergyPlus IDF row rejects one with a typed fault; the `outputs` census is the router — a requested name absent from the census is the band's `EnergyFault.unknown_output` case carrying the missing names beside the census size, and a recipe output the product never resolved is `unresolved_output` at the `matrix` arm, one closed refusal family either way and never a guessed address; the CLI translate fall-through refuses its unserved target through the same family, so a consumer matches one tag rather than parsing three coordinate strings.
- Receipt: `results` returns `(frame, receipt)` built inside the fold off the frame it just sealed, so no caller hand-asserts a row count or a frame key. `spec` is the evidence subject — the model key beside the recipe and the query — and `graduates` derives its own `ContentKey` from it. Total EUI is the measured graduation fact against the caller's compliance ceiling, and only the `eui` arm measures it: every other query OMITS the key so the spine reports `unmeasured:eui` and refuses honestly rather than crossing clean on a zero that reads as a zero-energy building. That same arm records the total onto the `rasm.geometry.energy.eui` charter distribution at the producing fold.
- Packages: `honeybee-openstudio` wraps the BSD `openstudio` SDK behind the `find_spec` gate; `honeybee-energy` carries the CLI pair, the `SimulationParameter` family, and the result parsers; ladybug `SQLiteResult` is the ONLY EnergyPlus SQL decode path; `queenbee` is MIT schema only — outside the AGPL band, so it binds `lazy` at module scope like the MIT `trimesh` GLB reader the shading decode calls; runtime `transport/artifact` owns verified ArtifactService fetch lifecycle and temporary-path custody; `numpy` seals the frame columns and the data `arrow_columns` (columnar) / `arrow_bytes` (interop) pair is the one serialization, so no `pyarrow` symbol appears on this page at all.
- Growth: a new translation format is one `WRITERS` row; a new output family one `SimPar` policy row over its `add_*` method; a new result decode is one `ResultQuery` case — `loadbalance`/`emissions`/`generation`/`component_sizes` the named next rows over their `honeybee_energy.result` parsers; a new workflow is one `RunSpec.recipe` value over the runtime catalog, zero page edits; a cloud submission consumes `job()` when a consumer names it.
- Boundary: execution is the runtime `execution/recipe` owner's; model semantics are `energy/model`'s, weather algebra `energy/climate`'s; a result frame whose table diverges from `FRAME_SCHEMA` is the deleted form — the C# decoder can neither attribute nor dedupe it.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Sequence
from enum import StrEnum
from functools import partial
from importlib.util import find_spec
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, assert_never

import numpy as np
from builtins import frozendict
from connectrpc.errors import ConnectError
from expression import Error, Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from msgspec import json as msgjson

lazy import trimesh
lazy from queenbee.io.artifact_source import ProjectFolder
lazy from queenbee.io.inputs.job import JobArgument, JobPathArgument
lazy from queenbee.job.job import Job
lazy from queenbee.recipe.recipe import RecipeInterface

from rasm.data.tabular.columnar import arrow_columns
from rasm.data.tabular.interop import arrow_bytes
from rasm.contracts import AdmissionError
from rasm.runtime.transport.artifact import ArtifactError, ArtifactTransfer
from rasm.geometry.energy.climate import ENERGY_REGIMES, EnergyFault, EnergyRegime, LateBound, RegimeKey
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
from rasm.runtime.shapes import remote_fault
from protobuf import Oneof
from rasm.contracts.rasm.contracts.geometry.tessellation_pb import TessellationPolicy
from rasm.contracts.rasm.contracts.scene.scene_pb import Photometry, SceneDescriptor, SceneSun, SolarAngles
from rasm.runtime.workers import Kernel, KernelTrait

if TYPE_CHECKING:
    from honeybee.shademesh import ShadeMesh
    from honeybee_radiance.lightsource.sky.cie import CIE
    from honeybee_radiance.lightsource.sky.climatebased import ClimateBased

# --- [TYPES] ----------------------------------------------------------------------------


type Row = tuple[object, ...]


class TranslateTarget(StrEnum):
    OSM = "osm"
    IDF = "idf"
    EPJSON = "epjson"
    GBXML = "gbxml"


@tagged_union(frozen=True)
class ResultQuery:
    tag: Literal["collections", "eui", "tabular", "outputs", "matrix"] = tag()
    collections: tuple[Path, tuple[str, ...]] = case()
    eui: Path = case()
    tabular: tuple[Path, str] = case()
    outputs: Path = case()
    matrix: tuple[RecipeName, RecipeProduct] = case()


@tagged_union(frozen=True)
class SkySource:
    tag: Literal["cie", "climate_based"] = tag()
    cie: int = case()
    climate_based: tuple[float, float] = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

WRITERS: Final[Map[TranslateTarget, LateBound]] = Map.of_seq([
    (TranslateTarget.OSM, LateBound("honeybee_openstudio.writer", "model_to_osm")),
    (TranslateTarget.IDF, LateBound("honeybee_openstudio.writer", "model_to_idf")),
    (TranslateTarget.EPJSON, LateBound("honeybee_openstudio.writer", "model_to_epjson")),
    (TranslateTarget.GBXML, LateBound("honeybee_openstudio.writer", "model_to_gbxml")),
])

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

_UNMEASURED: Final[float] = float("nan")

_ENCODER: Final = msgjson.Encoder(order="deterministic")

_RADIANT_FLUX: Final[str] = "radiant_flux_w"

_WEATHER_DRIVEN: Final[frozenset[RecipeName]] = frozenset({RecipeName.ANNUAL_ENERGY_USE})

# --- [MODELS] ---------------------------------------------------------------------------


class SimPar(Struct, frozen=True, gc=False):
    reporting_frequency: str = "Hourly"
    zone_energy: bool = True
    hvac_energy: bool = True
    comfort_metrics: bool = False
    unmet_hours: bool = True
    north_angle: float = 0.0
    terrain: str = "City"
    run_period: Option[tuple[tuple[int, int], tuple[int, int]]] = Nothing


class RunSpec(Struct, frozen=True):
    epw: Path
    recipe: RecipeName = RecipeName.ANNUAL_ENERGY_USE
    sim_par: SimPar = SimPar()
    extra: Map[str, object] = Map.empty()


class ResultFrame(Struct, frozen=True):
    columns: tuple[str, ...]
    table: frozendict[str, np.ndarray]
    content_key: ContentKey

    @property
    def rows(self) -> int:
        return int(next(iter(self.table.values())).shape[0]) if self.table else 0


class SceneLighting(Struct, frozen=True):
    identifier: str
    kind: str
    watts: float
    spectrum: tuple[float, float, float]


class SceneContext(Struct, frozen=True):
    descriptor_key: str
    shade_meshes: tuple["ShadeMesh", ...]
    sky: "CIE | ClimateBased"
    north_angle: float
    site: Option[tuple[float, float, float, float]] = Nothing
    lighting: tuple[SceneLighting, ...] = ()


class SceneReceipt(Struct, frozen=True):
    descriptor_key: str
    derivation: str
    shade_meshes: int
    triangles: int
    lights: int
    unranked_lights: int
    unrouted_webs: int

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of(
            "rasm.geometry.energy.simulate",
            (
                "emitted",
                f"scene:{self.derivation}",
                {
                    "descriptor_key": self.descriptor_key,
                    "shade_meshes": self.shade_meshes,
                    "triangles": self.triangles,
                    "lights": self.lights,
                    "unranked_lights": self.unranked_lights,
                    "unrouted_webs": self.unrouted_webs,
                },
            ),
        )


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
                    **self.eui_total.map(lambda total: {"eui": total}).default_value({}),
                },
            ),
        )

    def spec(self) -> bytes:
        return b"|".join((self.model_key.memory, self.recipe.encode(), self.operation.encode(), self.discriminant.encode()))

    def graduates(self, regime: EnergyRegime = ENERGY_REGIMES[RegimeKey.BUILDING_EUI]) -> GeometryHandoff:
        measured = self.eui_total.map(lambda total: {"eui": total}).default_value({}) | {"rows": float(self.rows)}
        subject = GeometrySubject.BUILDING_ENERGY
        return GeometryHandoff.of(subject, evidence_key(subject, self.spec()), measured, {"eui": regime.bar()})


# --- [SERVICES] -------------------------------------------------------------------------


class Simulation(Struct, frozen=True):
    building: BuildingModel
    lane: LanePolicy
    recipes: RecipeExecution
    artifacts: ArtifactTransfer
    composition: ScopeKey = DEFAULT_SCOPE

    async def translate(self, target: TranslateTarget, folder: Path) -> "RuntimeRail[Path]":
        return await evidence_run(
            EvidenceScope.ENERGY_SIMULATE,
            f"translate.{target}",
            partial(self.lane.offload, Kernel.of(_translated, KernelTrait.HOSTILE, idempotent=False), self.building, target, folder),
            composition=self.composition,
        )

    def sim_par(self, spec: SimPar) -> "RuntimeRail[dict[str, object]]":
        def fold() -> dict[str, object]:
            from honeybee_energy.simulation.output import SimulationOutput
            from honeybee_energy.simulation.parameter import SimulationParameter
            from honeybee_energy.simulation.runperiod import RunPeriod
            from ladybug.dt import Date

            output = SimulationOutput(reporting_frequency=spec.reporting_frequency)
            requests = (
                (spec.zone_energy, output.add_zone_energy_use),
                (spec.hvac_energy, output.add_hvac_energy_use),
                (spec.comfort_metrics, output.add_comfort_metrics),
                (spec.unmet_hours, output.add_unmet_hours),
            )
            for wanted, add in requests:
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
        projected = await self.recipes.interface(RecipeSpec(recipe=run.recipe))
        return projected.map(lambda interface: _job(interface, run, model, source))

    def results(self, query: ResultQuery) -> "RuntimeRail[tuple[ResultFrame, SimulationReceipt]]":
        def fold() -> tuple[ResultFrame, SimulationReceipt]:
            rows, discriminant, recipe, eui = _decoded(query)
            eui.map(lambda total: charter_record(GeometrySubject.BUILDING_ENERGY, {"eui_total": total}, composition=self.composition))
            frame = _tabled(rows)
            return frame, self._receipt(f"results.{query.tag}", discriminant, recipe, frame, eui)

        return evidence_run(EvidenceScope.ENERGY_SIMULATE, f"results.{query.tag}", fold, composition=self.composition)

    async def scene(
        self,
        descriptor: SceneDescriptor,
        run: RunSpec,
        sky: SkySource = SkySource(cie=0),
        *,
        units: str = "Meters",
    ) -> "RuntimeRail[tuple[SceneContext, SceneReceipt]]":
        try:
            async with self.artifacts.fetch(descriptor.shading.artifact) as owned:
                def fold() -> tuple[SceneContext, SceneReceipt]:
                    derivation, angles, site, north = _derived_sun(sun=descriptor.sun, recipe=run.recipe)
                    shades, triangles = _shades(glb=owned.path, identifier=descriptor.key.hex())
                    _graded(
                        fidelity=descriptor.shading.fidelity,
                        element_count=descriptor.shading.element_count,
                        decoded_elements=len(shades),
                        triangle_count=descriptor.shading.triangle_count,
                        decoded_triangles=triangles,
                        units=units,
                    )
                    lighting, unranked, unrouted = _lighting(rows=tuple(descriptor.lights))
                    context = SceneContext(
                        descriptor_key=descriptor.key.hex(),
                        shade_meshes=shades,
                        sky=_sky(angles=angles, source=sky),
                        north_angle=north,
                        site=site,
                        lighting=lighting,
                    )
                    return context, SceneReceipt(
                        descriptor_key=descriptor.key.hex(),
                        derivation=derivation,
                        shade_meshes=len(shades),
                        triangles=triangles,
                        lights=len(lighting),
                        unranked_lights=unranked,
                        unrouted_webs=unrouted,
                    )

                return evidence_run(
                    EvidenceScope.ENERGY_SIMULATE,
                    f"scene.{run.recipe.value}",
                    fold,
                    composition=self.composition,
                )
        except ArtifactError as refused:
            return Error(EnergyFault(artifact_integrity=refused.proof.value))
        except AdmissionError as refused:
            return Error(EnergyFault(artifact_admission=refused.phase.value))
        except ConnectError as refused:
            return Error(remote_fault(refused))

    def crossing(self, frame: ResultFrame) -> "RuntimeRail[tuple[bytes, ContentKey]]":
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


def _derived_sun(sun: SceneSun, recipe: RecipeName) -> tuple[str, SolarAngles, Option[tuple[float, float, float, float]], float]:
    match sun.derivation:
        case Oneof(field="sited", value=sited):
            frame = sited.frame
            return (
                "sited",
                sited.angles,
                Some((frame.latitude_deg, frame.longitude_deg, frame.time_zone_hours, frame.elevation_m)),
                frame.north_axis_deg,
            )
        case Oneof(field="authored", value=angles) if recipe not in _WEATHER_DRIVEN:
            return "authored", angles, Nothing, 0.0
        case Oneof(field="authored"):
            raise EnergyFault(authored_sun=(recipe.value, "sited-frame"))
        case _:
            raise EnergyFault(authored_sun=(recipe.value, "sun-derivation"))


def _sky(angles: SolarAngles, source: SkySource) -> "CIE | ClimateBased":
    from honeybee_radiance.lightsource.sky.cie import CIE
    from honeybee_radiance.lightsource.sky.climatebased import ClimateBased

    match source:
        case SkySource(tag="cie", cie=kind):
            return CIE(angles.altitude_deg, angles.azimuth_deg, kind)
        case SkySource(tag="climate_based", climate_based=(direct, diffuse)):
            return ClimateBased(angles.altitude_deg, angles.azimuth_deg, direct, diffuse)
        case _ as unreachable:
            assert_never(unreachable)


def _graded(
    fidelity: TessellationPolicy,
    element_count: int,
    decoded_elements: int,
    triangle_count: int,
    decoded_triangles: int,
    units: str,
) -> None:
    from honeybee.units import UNITS_TOLERANCES, conversion_factor_to_meters

    floor = UNITS_TOLERANCES[units] * conversion_factor_to_meters(units)
    if fidelity.deflection_m > floor:
        raise EnergyFault(shading_fidelity=("deflection_m", fidelity.deflection_m, floor))
    if decoded_triangles > fidelity.triangle_budget:
        raise EnergyFault(shading_fidelity=("triangle_count", float(decoded_triangles), float(fidelity.triangle_budget)))
    if decoded_elements != element_count:
        raise EnergyFault(shading_census=("element_count", element_count, decoded_elements))
    if decoded_triangles != triangle_count:
        raise EnergyFault(shading_census=("triangle_count", triangle_count, decoded_triangles))


def _shades(glb: Path, identifier: str) -> tuple[tuple["ShadeMesh", ...], int]:
    from honeybee.shademesh import ShadeMesh
    from ladybug_geometry.geometry3d.mesh import Mesh3D
    from ladybug_geometry.geometry3d.pointvector import Point3D

    scene = trimesh.load_scene(glb, file_type="glb")
    posed = tuple(
        scene.geometry[scene.graph[node][1]].copy().apply_transform(scene.graph[node][0])
        for node in scene.graph.nodes_geometry
    )
    rows = tuple(
        ShadeMesh(
            f"{identifier}_ctx_{ordinal}",
            Mesh3D(
                tuple(Point3D(*vertex) for vertex in geometry.vertices),
                tuple(tuple(int(index) for index in face) for face in geometry.faces),
            ),
        )
        for ordinal, geometry in enumerate(posed)
    )
    return rows, sum(len(geometry.faces) for geometry in posed)


def _lighting(rows: tuple[Photometry, ...]) -> tuple[tuple[SceneLighting, ...], int, int]:
    live = tuple(row for row in rows if row.enabled)
    ranked = tuple(
        SceneLighting(
            identifier=row.id.hex(),
            kind=row.kind.name.lower(),
            watts=row.power.authority.value,
            spectrum=(row.diffuse.r, row.diffuse.g, row.diffuse.b),
        )
        for row in live
        if row.power.authority is not None and row.power.authority.field == _RADIANT_FLUX
    )
    return ranked, len(live) - len(ranked), sum(1 for row in rows if row.web is not None)


def _product_rows(name: str, value: object) -> tuple[Row, ...]:
    if isinstance(value, (str, Path)):
        return ((name, "", "", str(value), 0, _UNMEASURED),)
    grids = value if isinstance(value, Sequence) else (value,)
    return tuple(
        (name, "", "", f"grid:{at}", step, float(sensor))
        for at, grid in enumerate(grids)
        for step, sensor in enumerate(grid if isinstance(grid, Sequence) else (grid,))
    )


def _decoded(query: ResultQuery) -> tuple[tuple[Row, ...], str, str, Option[float]]:
    from honeybee_energy.result.eui import eui_from_sql
    from ladybug.sql import SQLiteResult

    match query:
        case ResultQuery(tag="collections", collections=(sql, names)):
            reader = SQLiteResult(str(sql))
            census = tuple(reader.available_outputs)
            missing = tuple(name for name in names if name not in census)
            if missing:
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
            return rows, "end-uses", RecipeName.ANNUAL_ENERGY_USE.value, Some(sum(float(row[5]) for row in rows))
        case ResultQuery(tag="tabular", tabular=(sql, name)):
            reader = SQLiteResult(str(sql))
            rows = tuple((name, "", "", str(key), 0, value) for key, values in reader.tabular_data_by_name(name).items() for value in values)
            return rows, name, RecipeName.ANNUAL_ENERGY_USE.value, Nothing
        case ResultQuery(tag="outputs", outputs=sql):
            census = tuple(SQLiteResult(str(sql)).available_outputs)
            return tuple((name, "", "", "", 0, _UNMEASURED) for name in census), f"census:{len(census)}", RecipeName.ANNUAL_ENERGY_USE.value, Nothing
        case ResultQuery(tag="matrix", matrix=(recipe, product)):
            declared = RECIPES[recipe].outputs
            missing = tuple(name for name in declared if product.outputs.try_find(name).is_none())
            if missing:
                raise EnergyFault(unresolved_output=(recipe.value, missing))
            rows = tuple(row for name in declared for row in _product_rows(name, product.outputs[name]))
            return rows, recipe.value, recipe.value, Nothing
        case _ as unreachable:
            assert_never(unreachable)


def _tabled(rows: tuple[Row, ...]) -> ResultFrame:
    key = ContentIdentity.key("energy-result", _ENCODER.encode(rows))
    stamped = tuple((*row, key.hex) for row in rows)
    columns = tuple(zip(*stamped, strict=True)) if stamped else ((),) * len(FRAME_COLUMNS)
    return ResultFrame(
        columns=FRAME_COLUMNS,
        table=frozendict({name: np.asarray(cells, dtype=FRAME_SCHEMA[name]) for name, cells in zip(FRAME_COLUMNS, columns, strict=True)}),
        content_key=key,
    )


def _job(interface: "RecipeInterface", run: RunSpec, model: Path, source: str) -> "Job":
    run_arguments = [
        JobPathArgument(name="model", source=ProjectFolder(path=str(model))),
        JobPathArgument(name="epw", source=ProjectFolder(path=str(run.epw))),
        *(JobArgument(name=name, value=str(value)) for name, value in run.extra.items()),
    ]
    return Job(source=source, arguments=[run_arguments], name=interface.metadata.name)


def _translated(building: BuildingModel, target: TranslateTarget, folder: Path) -> Path:
    def in_process() -> Path:
        artifact = folder / f"{building.model.identifier}.{target.value}"
        artifact.write_text(WRITERS[target].resolve()(building.model), encoding="utf-8")
        return artifact

    def cli() -> Path:
        from honeybee_energy.run import run_osw, to_openstudio_osw

        if target not in (TranslateTarget.OSM, TranslateTarget.IDF):
            raise EnergyFault(unsupported_target=(target.value, "requires-openstudio-sdk"))

        model_path = building.model.to_hbjson(name=building.model.identifier, folder=str(folder))
        osw = to_openstudio_osw(str(folder), model_path)
        osm, idf = run_osw(osw, measures_only=True)
        return Path(idf if target is TranslateTarget.IDF else osm)

    return in_process() if find_spec("openstudio") else cli()
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
