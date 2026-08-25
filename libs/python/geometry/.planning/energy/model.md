# [PY_GEOMETRY_ENERGY_MODEL]

`BuildingModel` owns HBJSON and BIM-to-BEM admission, folds energy assignments through the Honeybee extension spine, and returns the validated, energy-assigned, content-keyed Honeybee model with `ModelCensus`.

Honeybee's AGPL-3.0 band rides the standing companion-lane charter — the `energy/climate` `LateBound` owner for every row-resolved standards loader, function-local imports at the two boundary kernels, evidence as document bytes. Those imports stay inside their seam functions because a static license audit reads the LEXICAL import graph: a module-scope binding — `lazy` included, the soft keyword being module-scope by design — couples every importer of this page to AGPL, so confinement is what the form buys and deferral is beside the point. Wire form IS the cross-language contract: the C# `Rasm.Bim` Energy exchange and `Rasm.Compute` simulation are peers meeting at content-keyed HBJSON document bytes over ONE derivation, never two keys over one document and never a mirrored shape. BIM-to-BEM is the modality only this folder can own — `ifcopenshell` carries no copyleft coupling to confine, so the compiled IFC band binds as ONE module-scope `lazy import` both kernels dereference at first use, a direct package consume and never an `ifc/`-plane page import, and only space solids cross.

## [01]-[INDEX]

- [02]-[MODEL]: one polymorphic model owner — two-modality admission under one `check_all` census, the host-agnostic assignment fold over the standards backends, the one-derivation wire egress, the pulsed BIM crossing and its bench seam — beside one `ModelCensus`.

## [02]-[MODEL]

- Owner: `BuildingModel` holds the validated honeybee `Model`, its `ContentKey`, and the `composition` custody key its weave, pulse, and bench legs stamp — the honeybee `Model` IS the canonical owner, never mirrored into a local dataclass graph, and the content-keyed bytes are its only projection. `BemPolicy` carries derivation behavior as data: the `wwr` default and orientation-binned ratio rows, the classification bounds `Room.from_polyface3d` reads, and ONE tolerance every kernel reads, never three literals. `HvacSpec.template` keys `HVAC_TYPES_DICT`, which resolves the dynamically-built class, never a per-template import; `StandardsKind` keys `RESOLVERS`, whose rows are `LateBound` values, so standards resolution is the same one late-binding fold the climate and simulate owners read.
- Entry: `of` is `async` over the source, the lane, and the composition, and both admission arms converge on the ONE `check_all(raise_exception=False, detailed=True)` census. `hbjson` decodes on the caller floor — a short pure decode earns no crossing — while the `bim` arm's repeated `ifcopenshell.geom.create_shape` sweep is the genuinely long native phase, so `_derived` crosses as `Kernel.of(_derived, KernelTrait.HOSTILE)` with picklable args: SPF bytes and the lane's pickled pulse tap in, the pure-Python honeybee `Model` graph pickled home, the derivation content-keyed and run-scoped so the trait's `WORKER` death retry stays sound. `assign` re-runs the same census — honeybee invokes every registered extension's checks automatically, so the energy-validity rows ride it — and the returned successor re-keys because assignment changed the document bytes. Fenestration mint walks exterior walls only, a zero ratio skipping the face.
- Auto: every fold returns `(model, census)` built where the facts live, so no caller hand-asserts a census; `_derived` beats one `GeometryPulse.BEM` runtime `StageMark` per space through the lane conduit under this page's own closed `ModelStage` roster, so an unbounded per-space sweep reports monotonic progress under the caller's composition; `bench` wraps the whole `of` crossing through `bench_seam`, keyed by modality and space census so a latency row compares like-for-like across model sizes; structural emptiness refuses as the band's own `EnergyFault.empty_model` case carrying the modality and the census, never a coordinate string the fence flattens; a missing standards identifier resolves a typed fault naming the identifier and the registry kind, never a bare `KeyError` — the extension-dependent `building_program_type_by_identifier` fault names the absent `honeybee-energy-standards` backend; orientation binning derives from `angles_from_num_orient`, so four/eight/sixteen-bin policies are one integer, never a compass ladder.
- Output: `check_all` rows stay on `ModelCensus` beside the admitted model. Structural emptiness remains a typed refusal; validation counts are returned as measured facts rather than re-shaped into a second outcome.
- Packages: `honeybee-core`, `honeybee-energy`, `ladybug-geometry`, and `ifcopenshell` per the fence imports. Two standards backends merge behind the `lib` loaders — `honeybee-standards` defaults floor always resolvable, `honeybee-energy-standards` ASHRAE/DOE vintage sets when installed — never a direct standards-JSON read; `honeybee-schema` validates the HBJSON dict upstream of C#, and no parallel `msgspec` model family mirrors it.
- Growth: a new fenestration strategy is one `BemPolicy` row family — dimension-driven apertures ride `apertures_by_width_height_rectangle` as one more policy case; shading mints (`louvers_by_count`/`overhang`) enter as `EnergySpec` rows the shared `assigned` fold picks up at both tiers in one edit; a new HVAC template or vintage is zero code — the registry and its `equipment_type` vocabulary are upstream data; a new standards kind is one `RESOLVERS` row; the daylight modality (`honeybee-radiance` and sensor grids) enters only through a future package-admission motion; dragonfly-exploded models arrive as ordinary `hbjson` payloads from `energy/district`, no third modality.
- Boundary: IFC semantic analysis is the `ifc/` plane's — the BIM arm consumes ONLY space solids; simulation is `energy/simulate`'s, urban massing `energy/district`'s, weather `energy/climate`'s; the mesh daemon owns the GLB render wire — this arm's `create_shape` feeds `Face3D` lifting, never a cached render artifact.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Iterable, Mapping
from enum import StrEnum
from functools import partial
from pathlib import Path
from queue import Queue
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never

from expression import Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec import json as msgjson

lazy import ifcopenshell
lazy import ifcopenshell.geom

from rasm.geometry.energy.climate import EnergyFault, LateBound
from rasm.geometry.graduation import (
    EvidenceScope,
    GeometryPulse,
    bench_seam,
    bench_subject,
    evidence_run,
)
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.hooks import StageMark
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, PulseFact, pulsed
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.profiles import Benchmark
from rasm.runtime.workers import Kernel, KernelTrait

if TYPE_CHECKING:
    from honeybee.model import Model

# --- [TYPES] ----------------------------------------------------------------------------


class ModelStage(StrEnum):
    SPACE = "space"


class StandardsKind(StrEnum):
    PROGRAM = "program"
    BUILDING_PROGRAM = "building_program"
    CONSTRUCTION_SET = "construction_set"
    SCHEDULE = "schedule"
    OPAQUE_CONSTRUCTION = "opaque_construction"
    WINDOW_CONSTRUCTION = "window_construction"


# --- [CONSTANTS] ------------------------------------------------------------------------

RESOLVERS: Final[Map[StandardsKind, LateBound]] = Map.of_seq([
    (StandardsKind.PROGRAM, LateBound("honeybee_energy.lib.programtypes", "program_type_by_identifier")),
    (StandardsKind.BUILDING_PROGRAM, LateBound("honeybee_energy.lib.programtypes", "building_program_type_by_identifier")),
    (StandardsKind.CONSTRUCTION_SET, LateBound("honeybee_energy.lib.constructionsets", "construction_set_by_identifier")),
    (StandardsKind.SCHEDULE, LateBound("honeybee_energy.lib.schedules", "schedule_by_identifier")),
    (StandardsKind.OPAQUE_CONSTRUCTION, LateBound("honeybee_energy.lib.constructions", "opaque_construction_by_identifier")),
    (StandardsKind.WINDOW_CONSTRUCTION, LateBound("honeybee_energy.lib.constructions", "window_construction_by_identifier")),
])

_ENCODER: Final = msgjson.Encoder(order="deterministic")

# --- [MODELS] ---------------------------------------------------------------------------


class BemPolicy(Struct, frozen=True):
    identifier: str
    tolerance: float = 0.01
    angle_tolerance: float = 1.0
    wwr: float = 0.4
    by_orientation: Map[int, float] = Map.empty()
    orientation_count: int = 4
    skylight_ratio: float = 0.0
    ground_depth: float = 0.0
    roof_angle: float = 60.0
    floor_angle: float = 130.0

    def ratio(self, bin_index: int) -> float:
        return self.by_orientation.try_find(bin_index).default_value(self.wwr)


@tagged_union(frozen=True)
class ModelSource:
    tag: Literal["hbjson", "bim"] = tag()
    hbjson: "bytes | str | Path | Mapping[str, object]" = case()
    bim: tuple[bytes, BemPolicy] = case()


class HvacSpec(Struct, frozen=True):
    template: str
    equipment_type: Option[str] = Nothing


class EnergySpec(Struct, frozen=True):
    program: Option[str] = Nothing
    construction_set: Option[str] = Nothing
    hvac: Option[HvacSpec] = Nothing
    shw: Option[str] = Nothing
    rooms: Option[tuple[str, ...]] = Nothing


class ModelCensus(Struct, frozen=True):
    source: str
    rooms: int
    faces: int
    apertures: int
    shades: int
    check_rows: int
    check_census: Map[str, int]
    touched: int
    program: Option[str]
    construction_set: Option[str]
    hvac: Option[str]
    content_key: ContentKey

# --- [SERVICES] -------------------------------------------------------------------------


class BuildingModel(Struct, frozen=True):
    model: "Model"
    content_key: ContentKey
    composition: ScopeKey = DEFAULT_SCOPE

    @classmethod
    async def of(
        cls, source: ModelSource, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[tuple[Self, ModelCensus]]":
        async def admit() -> "RuntimeRail[tuple[Self, ModelCensus]]":
            match source:
                case ModelSource(tag="hbjson", hbjson=payload):
                    return Ok(cls._gated(_decoded(payload), "hbjson", composition))
                case ModelSource(tag="bim", bim=(spf, policy)):
                    derived = await lane.offload(Kernel.of(_derived, KernelTrait.HOSTILE), spf, policy, lane.pulses.tap)
                    return derived.map(lambda model: cls._gated(model, "bim", composition))
                case _ as unreachable:
                    assert_never(unreachable)

        return await evidence_run(EvidenceScope.ENERGY_MODEL, f"admit.{source.tag}", admit, composition=composition)

    @classmethod
    def bench(
        cls, source: ModelSource, lane: LanePolicy, *, rounds: int = 32, warmup: int = 4, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[Benchmark]":
        return bench_seam(
            bench_subject(EvidenceScope.ENERGY_MODEL, source.tag, f"s{_census(source)}"),
            partial(cls.of, source, lane, composition=composition),
            rounds=rounds,
            warmup=warmup,
        )

    def assign(self, spec: EnergySpec) -> "RuntimeRail[tuple[Self, ModelCensus]]":
        def fold() -> tuple[Self, ModelCensus]:
            return type(self)._gated(self.model, "assign", self.composition, spec=Some(spec), touched=assigned(self.model.rooms, spec))

        return evidence_run(EvidenceScope.ENERGY_MODEL, "assign", fold, composition=self.composition)

    def wire(self) -> "RuntimeRail[tuple[bytes, ContentKey]]":
        return evidence_run(
            EvidenceScope.ENERGY_MODEL, "wire", lambda: (_document(self.model), self.content_key), composition=self.composition
        )

    def hbjson(self, folder: Path) -> "RuntimeRail[Path]":
        return evidence_run(
            EvidenceScope.ENERGY_MODEL,
            "hbjson",
            lambda: Path(self.model.to_hbjson(name=self.model.identifier, folder=str(folder))),
            composition=self.composition,
        )

    @classmethod
    def _gated(
        cls, model: "Model", modality: str, composition: ScopeKey, spec: Option[EnergySpec] = Nothing, touched: int = 0
    ) -> tuple[Self, ModelCensus]:
        rows = model.check_all(raise_exception=False, detailed=True)
        if not model.rooms:
            raise EnergyFault(empty_model=(modality, len(rows)))
        census = Block.of_seq(rows).fold(lambda acc, row: acc.change(str(row.get("code", "?")), lambda n: Some(n.default_value(0) + 1)), Map.empty())
        document = _document(model)
        admitted = cls(model=model, content_key=_keyed(document), composition=composition)
        return admitted, ModelCensus(

            source=modality,
            rooms=len(model.rooms),
            faces=len(model.faces),
            apertures=len(model.apertures),
            shades=len(model.shades),
            check_rows=len(rows),
            check_census=census,
            touched=touched,
            program=spec.bind(lambda s: s.program),
            construction_set=spec.bind(lambda s: s.construction_set),
            hvac=spec.bind(lambda s: s.hvac.map(lambda h: h.template)),
            content_key=admitted.content_key,
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def resolved(kind: StandardsKind, identifier: str) -> object:
    return RESOLVERS[kind].resolve()(identifier)


def assigned(hosts: Iterable[object], spec: EnergySpec) -> int:
    import honeybee_energy
    from honeybee_energy.hvac import HVAC_TYPES_DICT
    from honeybee_energy.shw import SHWSystem

    program = spec.program.map(lambda ident: resolved(StandardsKind.PROGRAM, ident)).to_optional()
    constructions = spec.construction_set.map(lambda ident: resolved(StandardsKind.CONSTRUCTION_SET, ident)).to_optional()
    hvac = spec.hvac.map(
        lambda h: HVAC_TYPES_DICT[h.template](f"{h.template}_system")
        if h.equipment_type.is_none()
        else HVAC_TYPES_DICT[h.template](f"{h.template}_system", equipment_type=h.equipment_type.value)
    ).to_optional()
    shw = spec.shw.map(SHWSystem).to_optional()
    wanted = spec.rooms.map(frozenset).default_value(frozenset())
    touched = 0
    for host in hosts:
        if wanted and host.identifier not in wanted:
            continue
        energy = host.properties.energy
        energy.program_type = program if program is not None else energy.program_type
        energy.construction_set = constructions if constructions is not None else energy.construction_set
        energy.hvac = hvac if hvac is not None else energy.hvac
        energy.shw = shw if shw is not None else energy.shw
        touched += 1
    return touched


def _document(model: "Model") -> bytes:
    return _ENCODER.encode(model.to_dict(included_prop=("energy",)))


def _keyed(document: bytes) -> ContentKey:
    return ContentIdentity.key("hbjson", (b"hbjson", document), seed=Some(0))


def _decoded(payload: "bytes | str | Path | Mapping[str, object]") -> "Model":
    from honeybee.model import Model

    match payload:
        case bytes() as raw:
            return Model.from_dict(msgjson.decode(raw))
        case Mapping() as data:
            return Model.from_dict(dict(data))
        case at:
            return Model.from_hbjson(str(at))


def _census(source: ModelSource) -> int:
    match source:
        case ModelSource(tag="hbjson", hbjson=payload):
            return len(_decoded(payload).rooms)
        case ModelSource(tag="bim", bim=(spf, _policy)):
            return len(ifcopenshell.file.from_string(spf.decode()).by_type("IfcSpace"))
        case _ as unreachable:
            assert_never(unreachable)


def _derived(spf: bytes, policy: BemPolicy, tap: "Queue[PulseFact | None]") -> "Model":
    from honeybee.model import Model
    from honeybee.orientation import angles_from_num_orient, orient_index
    from honeybee.room import Room
    from ladybug_geometry.geometry3d.face import Face3D
    from ladybug_geometry.geometry3d.pointvector import Point3D
    from ladybug_geometry.geometry3d.polyface import Polyface3D

    ifc = ifcopenshell.file.from_string(spf.decode())
    settings = ifcopenshell.geom.settings()
    settings.set("use-world-coords", True)
    spaces = ifc.by_type("IfcSpace")

    def room_of(space: object, ordinal: int) -> Room:
        shape = ifcopenshell.geom.create_shape(settings, space)
        verts, faces = shape.geometry.verts, shape.geometry.faces
        points = tuple(Point3D(verts[i], verts[i + 1], verts[i + 2]) for i in range(0, len(verts), 3))
        triangles = tuple(Face3D((points[faces[i]], points[faces[i + 1]], points[faces[i + 2]])) for i in range(0, len(faces), 3))
        polyface = Polyface3D.from_faces(triangles, policy.tolerance)
        name = getattr(space, "GlobalId", f"{policy.identifier}_space_{ordinal}")
        pulsed(tap, GeometryPulse.BEM, StageMark(stage=ModelStage.SPACE.value, done=ordinal + 1, total=Some(len(spaces))))
        return Room.from_polyface3d(f"{policy.identifier}_{name}", polyface, policy.roof_angle, policy.floor_angle, policy.ground_depth)

    rooms = [room_of(space, ordinal) for ordinal, space in enumerate(spaces)]
    Room.solve_adjacency(rooms, policy.tolerance)
    angles = angles_from_num_orient(policy.orientation_count)
    for room in rooms:
        for face in room.faces:
            if face.boundary_condition.name != "Outdoors" or face.type.name != "Wall":
                continue
            ratio = policy.ratio(orient_index(face.horizontal_orientation(), angles))
            if ratio > 0.0:
                face.apertures_by_ratio(ratio, policy.tolerance)
    model = Model(policy.identifier, rooms=rooms, units="Meters", tolerance=policy.tolerance, angle_tolerance=policy.angle_tolerance)
    if policy.skylight_ratio > 0.0:
        model.skylight_apertures_by_ratio(policy.skylight_ratio)
    return model
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
