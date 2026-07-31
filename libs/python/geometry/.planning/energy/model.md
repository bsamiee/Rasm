# [PY_GEOMETRY_ENERGY_MODEL]

`BuildingModel` owns HBJSON building-energy-model admission — the one place a building model becomes simulation-ready. `BuildingModel.of` discriminates two modalities under one `check_all` census: wire-arrival HBJSON decode, and the COMPUTED BIM-to-BEM derivation lifting `IfcSpace` solids into an adjacency-solved, fenestrated honeybee model. `assigned` folds one `EnergySpec` over any host set through the `.properties.energy` extension spine, so the model and district tiers share one assignment body. `honeybee-core`/`honeybee-energy`/`ladybug-geometry` own every adjacency solver, aperture generator, HBJSON parser, and validation rule; this page composes them into one admitted, validated, energy-assigned, content-keyed model graduating under `GeometrySubject.BUILDING_ENERGY`.

Honeybee's AGPL-3.0 band rides the standing companion-lane charter — the `energy/climate` `LateBound` owner for every row-resolved standards loader, function-local imports at the two boundary kernels, evidence as document bytes. Wire form IS the cross-language contract: the C# `Rasm.Bim` Energy exchange and `Rasm.Compute` simulation are peers meeting at content-keyed HBJSON document bytes over ONE derivation, never two keys over one document and never a mirrored shape. BIM-to-BEM is the modality only this folder can own — `ifcopenshell` binds as a direct package consume, never an `ifc/`-plane page import, and only space solids cross.

## [01]-[INDEX]

- [02]-[MODEL]: one polymorphic model owner — two-modality admission under one `check_all` census, the host-agnostic assignment fold over the standards backends, the one-derivation wire egress, the pulsed BIM crossing and its bench seam — under one `ModelReceipt`.

## [02]-[MODEL]

- Owner: `BuildingModel` holds the validated honeybee `Model`, its `ContentKey`, and the `composition` custody key its weave, pulse, and bench legs stamp — the honeybee `Model` IS the canonical owner, never mirrored into a local dataclass graph, and the content-keyed bytes are its only projection. `BemPolicy` carries derivation behavior as data: the `wwr` default and orientation-binned ratio rows, the classification bounds `Room.from_polyface3d` reads, and ONE tolerance every kernel reads, never three literals. `HvacSpec.template` keys `HVAC_TYPES_DICT`, which resolves the dynamically-built class, never a per-template import; `StandardsKind` keys `RESOLVERS`, whose rows are `LateBound` values, so standards resolution is the same one late-binding fold the climate and simulate owners read.
- Entry: `of` is `async` over the source, the lane, and the composition, and both admission arms converge on the ONE `check_all(raise_exception=False, detailed=True)` census. `hbjson` decodes on the caller floor — a short pure decode earns no crossing — while the `bim` arm's repeated `ifcopenshell.geom.create_shape` sweep is the genuinely long native phase, so `_derived` crosses as `Kernel.of(_derived, KernelTrait.HOSTILE)` with picklable args: SPF bytes and the lane's pickled pulse tap in, the pure-Python honeybee `Model` graph pickled home, the derivation content-keyed and run-scoped so the trait's `WORKER` death retry stays sound. `assign` re-runs the same census — honeybee invokes every registered extension's checks automatically, so the energy-validity rows ride it — and the returned successor re-keys because assignment changed the document bytes. Fenestration mint walks exterior walls only, a zero ratio skipping the face.
- Auto: every fold returns `(model, receipt)` built where the facts live, so no caller hand-asserts a census; `_derived` beats one `GeometryPulse.BEM` `PulseBeat` per space through the lane conduit, so an unbounded per-space sweep reports monotonic progress under the caller's composition; `bench` wraps the whole `of` crossing through `bench_seam`, keyed by modality and space census so a latency row compares like-for-like across model sizes; structural emptiness refuses as the band's own `EnergyFault.empty_model` case carrying the modality and the census, never a coordinate string the fence flattens; a missing standards identifier resolves a typed fault naming the identifier and the registry kind, never a bare `KeyError` — the extension-dependent `building_program_type_by_identifier` fault names the absent `honeybee-energy-standards` backend; orientation binning derives from `angles_from_num_orient`, so four/eight/sixteen-bin policies are one integer, never a compass ladder.
- Receipt: `check_all` rows are MEASURED admission evidence, not a fatal raise — the census and its per-code roster ride `ModelReceipt`, the graduation residual is the validation-error fraction over the element census, and the CALLER's ceiling owns the verdict, which is the only shape under which that residual can be anything but a zero no producer measured. Structural emptiness stays the raise: a decode or derivation minting no rooms has nothing to simulate, so it refuses at the census rather than graduating on a `1.0` residual nobody set a ceiling for. `spec` is the evidence subject — the wire key beside the modality — so `graduates` derives its own `ContentKey` and takes none.
- Packages: `honeybee-core`, `honeybee-energy`, `ladybug-geometry`, and `ifcopenshell` per the fence imports. Two standards backends merge behind the `lib` loaders — `honeybee-standards` defaults floor always resolvable, `honeybee-energy-standards` ASHRAE/DOE vintage sets when installed — never a direct standards-JSON read; `honeybee-schema` validates the HBJSON dict upstream of C#, and no parallel `msgspec` model family mirrors it.
- Growth: a new fenestration strategy is one `BemPolicy` row family — dimension-driven apertures ride `apertures_by_width_height_rectangle` as one more policy case; shading mints (`louvers_by_count`/`overhang`) enter as `EnergySpec` rows the shared `assigned` fold picks up at both tiers in one edit; a new HVAC template or vintage is zero code — the registry and its `equipment_type` vocabulary are upstream data; a new standards kind is one `RESOLVERS` row; the daylight modality (`honeybee-radiance` and sensor grids) enters only through a future package-admission motion; dragonfly-exploded models arrive as ordinary `hbjson` payloads from `energy/district`, no third modality.
- Boundary: IFC semantic analysis is the `ifc/` plane's — the BIM arm consumes ONLY space solids; simulation is `energy/simulate`'s, urban massing `energy/district`'s, weather `energy/climate`'s; the mesh daemon owns the GLB render wire — this arm's `create_shape` feeds `Face3D` lifting, never a cached render artifact.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
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

from rasm.geometry.energy.climate import EnergyFault, LateBound
from rasm.geometry.graduation import (
    EvidenceScope,
    GeometryHandoff,
    GeometryPulse,
    GeometrySubject,
    PulseBeat,
    bench_seam,
    bench_subject,
    evidence_key,
    evidence_run,
)
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, PulseFact, pulsed
from rasm.runtime.profiles import BenchmarkReceipt
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

if TYPE_CHECKING:  # AGPL band: annotations resolve here; every runtime use is a function-local or LateBound seam
    from honeybee.model import Model

# --- [TYPES] ----------------------------------------------------------------------------


class StandardsKind(StrEnum):
    PROGRAM = "program"
    BUILDING_PROGRAM = "building_program"
    CONSTRUCTION_SET = "construction_set"
    SCHEDULE = "schedule"
    OPAQUE_CONSTRUCTION = "opaque_construction"
    WINDOW_CONSTRUCTION = "window_construction"


# --- [CONSTANTS] ------------------------------------------------------------------------

# each kind keys its honeybee_energy.lib loader through the climate owner's LateBound value — one late-binding grammar
# across this folder's whole AGPL band, so a new standards kind is one row and never a second getattr fold.
RESOLVERS: Final[Map[StandardsKind, LateBound]] = Map.of_seq([
    (StandardsKind.PROGRAM, LateBound("honeybee_energy.lib.programtypes", "program_type_by_identifier")),
    (StandardsKind.BUILDING_PROGRAM, LateBound("honeybee_energy.lib.programtypes", "building_program_type_by_identifier")),
    (StandardsKind.CONSTRUCTION_SET, LateBound("honeybee_energy.lib.constructionsets", "construction_set_by_identifier")),
    (StandardsKind.SCHEDULE, LateBound("honeybee_energy.lib.schedules", "schedule_by_identifier")),
    (StandardsKind.OPAQUE_CONSTRUCTION, LateBound("honeybee_energy.lib.constructions", "opaque_construction_by_identifier")),
    (StandardsKind.WINDOW_CONSTRUCTION, LateBound("honeybee_energy.lib.constructions", "window_construction_by_identifier")),
])

_ENCODER: Final = msgjson.Encoder(order="deterministic")  # canonical HBJSON bytes — the one wire derivation the content key folds

# --- [MODELS] ---------------------------------------------------------------------------


class BemPolicy(Struct, frozen=True):
    identifier: str
    tolerance: float = 0.01
    angle_tolerance: float = 1.0
    wwr: float = 0.4
    by_orientation: Map[int, float] = Map.empty()  # orientation-bin index -> ratio; the default fills unbinned walls
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
    template: str  # a HVAC_TYPES_DICT registry key (IdealAirSystem/VAV/PSZ/.../DetailedHVAC)
    equipment_type: Option[str] = Nothing


class EnergySpec(Struct, frozen=True):
    program: Option[str] = Nothing
    construction_set: Option[str] = Nothing
    hvac: Option[HvacSpec] = Nothing
    shw: Option[str] = Nothing
    rooms: Option[tuple[str, ...]] = Nothing  # host identifiers; Nothing assigns model-wide


class ModelReceipt(Struct, frozen=True):
    source: str
    rooms: int
    faces: int
    apertures: int
    shades: int
    check_rows: int
    check_census: Map[str, int]  # per-code roster, so a consumer reads WHICH validation classes failed, not a bare count
    touched: int  # hosts the assignment fold actually reached; zero on an admission that assigned nothing
    program: Option[str]
    construction_set: Option[str]
    hvac: Option[str]
    content_key: ContentKey

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of(
            "rasm.geometry.energy.model",
            (
                "emitted",
                self.source,
                {
                    "rooms": self.rooms,
                    "faces": self.faces,
                    "apertures": self.apertures,
                    "shades": self.shades,
                    "check_rows": self.check_rows,
                    "touched": self.touched,
                    "content_key": self.content_key.hex,
                },
            ),
        )

    def spec(self) -> bytes:
        # the evidence subject IS the wire key beside the modality that produced it: one document, one key, one
        # subject, so re-admitting identical bytes graduates onto the same evidence row.
        return b"|".join((self.content_key.memory, self.source.encode()))

    def graduates(self, ceiling: float) -> GeometryHandoff:
        # measured admission evidence: the validation-error fraction over the element census, which is a real reading
        # only because the census gate records rows instead of raising on them.
        census = max(self.rooms + self.faces + self.apertures, 1)
        return GeometryHandoff.of(
            GeometrySubject.BUILDING_ENERGY,
            evidence_key(GeometrySubject.BUILDING_ENERGY, self.spec()),
            {"invalid": self.check_rows / census, "rooms": float(self.rooms), "apertures": float(self.apertures)},
            {"invalid": ceiling},
        )


# --- [SERVICES] -------------------------------------------------------------------------


class BuildingModel(Struct, frozen=True):
    model: "Model"
    content_key: ContentKey
    composition: ScopeKey = DEFAULT_SCOPE

    @classmethod
    async def of(
        cls, source: ModelSource, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[tuple[Self, ModelReceipt]]":
        async def admit() -> "RuntimeRail[tuple[Self, ModelReceipt]]":
            match source:
                case ModelSource(tag="hbjson", hbjson=payload):
                    # short pure decode: caller-floor by charter, no crossing earned.
                    return Ok(cls._gated(_decoded(payload), "hbjson", composition))
                case ModelSource(tag="bim", bim=(spf, policy)):
                    # create_shape sweep crosses HOSTILE with picklable args — SPF bytes and the conduit's pickled tap
                    # in, the pure-Python honeybee Model graph pickled home — and the caller-side census re-proves it
                    # before admission.
                    derived = await lane.offload(Kernel.of(_derived, KernelTrait.HOSTILE), spf, policy, lane.pulses.tap)
                    return derived.map(lambda model: cls._gated(model, "bim", composition))
                case _ as unreachable:
                    assert_never(unreachable)

        return await evidence_run(EvidenceScope.ENERGY_MODEL, f"admit.{source.tag}", admit, composition=composition)

    @classmethod
    def bench(
        cls, source: ModelSource, lane: LanePolicy, *, rounds: int = 32, warmup: int = 4, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[BenchmarkReceipt]":
        # space-census-parameterized macro-bench over the WHOLE admission crossing — decode or offload, native sweep,
        # adjacency solve, aperture mint, census — so a latency row compares like-for-like across model sizes; the
        # census parse is the harness's own, paid once outside every measured round.
        return bench_seam(
            bench_subject(EvidenceScope.ENERGY_MODEL, source.tag, f"s{_census(source)}"),
            partial(cls.of, source, lane, composition=composition),
            rounds=rounds,
            warmup=warmup,
            composition=composition,
        )

    def assign(self, spec: EnergySpec) -> "RuntimeRail[tuple[Self, ModelReceipt]]":
        def fold() -> tuple[Self, ModelReceipt]:
            # the shared fold reports the hosts it reached, so the successor's census carries the assignment extent
            # rather than a caller's claim about it.
            return type(self)._gated(self.model, "assign", self.composition, spec=Some(spec), touched=assigned(self.model.rooms, spec))

        return evidence_run(EvidenceScope.ENERGY_MODEL, "assign", fold, composition=self.composition)

    def wire(self) -> "RuntimeRail[tuple[bytes, ContentKey]]":
        # ONE derivation, minted at the census gate and re-projected here: the wire key is SEED-ZERO over the
        # format-key-then-document chunk stream, reproducing the C# CanonicalWriter String(format.Key).Raw(bytes) fold
        # under the CANONICAL_BYTE_IDENTITY framing law. The encoder is deterministic and `assign` re-keys its
        # successor, so re-encoding here yields exactly the bytes the held key was minted over.
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
    ) -> tuple[Self, ModelReceipt]:
        # detailed check rows fold to a per-code census the receipt carries and the ceiling grades; only structural
        # emptiness refuses here, since a model with no rooms has nothing to simulate and no residual to measure.
        rows = model.check_all(raise_exception=False, detailed=True)
        if not model.rooms:
            # structural emptiness carries its modality and its census as kwargs the fence lifts whole; converted once by evidence_run.
            raise EnergyFault(empty_model=(modality, len(rows)))
        census = Block.of_seq(rows).fold(lambda acc, row: acc.change(str(row.get("code", "?")), lambda n: Some(n.default_value(0) + 1)), Map.empty())
        document = _document(model)
        admitted = cls(model=model, content_key=_keyed(document), composition=composition)
        return admitted, ModelReceipt(
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
    # the ONE standards access path both energy tiers read: `energy/district` imports this fold rather than
    # re-writing it, so a new registry kind lands once and neither page can lag the other.
    return RESOLVERS[kind].resolve()(identifier)


def assigned(hosts: Iterable[object], spec: EnergySpec) -> int:
    # host-agnostic assignment fold: any object carrying `.identifier` and `.properties.energy` — a honeybee `Room` or
    # a dragonfly `Room2D` — so the model and district tiers share one body and a new `EnergySpec` slot is one edit.
    import honeybee_energy  # ruff:ignore[unused-import, import-outside-top-level] — the _extend_honeybee side effect registers .properties.energy
    from honeybee_energy.hvac import HVAC_TYPES_DICT  # ruff:ignore[import-outside-top-level]
    from honeybee_energy.shw import SHWSystem  # ruff:ignore[import-outside-top-level] — the SHW template mint; no lib registry exists

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
    for host in hosts:  # Exemption: honeybee and dragonfly hosts mutate in place; assignment is their owned imperative surface.
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
    from honeybee.model import Model  # ruff:ignore[import-outside-top-level] — AGPL boundary import

    match payload:
        case bytes() as raw:
            return Model.from_dict(msgjson.decode(raw))
        case Mapping() as data:
            return Model.from_dict(dict(data))
        case at:
            return Model.from_hbjson(str(at))


def _census(source: ModelSource) -> int:
    # bench-subject extent: the space count admission will build, read off the source once so the subject keys the
    # real model size rather than an opaque round number.
    match source:
        case ModelSource(tag="hbjson", hbjson=payload):
            return len(_decoded(payload).rooms)
        case ModelSource(tag="bim", bim=(spf, _policy)):
            import ifcopenshell  # ruff:ignore[import-outside-top-level] — companion-lane native import

            return len(ifcopenshell.file.from_string(spf.decode()).by_type("IfcSpace"))
        case _ as unreachable:
            assert_never(unreachable)


def _derived(spf: bytes, policy: BemPolicy, tap: "Queue[PulseFact | None]") -> "Model":
    # BIM-to-BEM: IfcSpace solids -> Face3D triangles -> Polyface3D -> Room -> adjacency -> apertures.
    # module-level HOSTILE kernel: ships REFERENCE onto the warm process pool; the live ifcopenshell.file stays
    # worker-local, and the trailing tap is the lane conduit's pickled proxy every space beat writes through.
    import ifcopenshell  # ruff:ignore[import-outside-top-level] — companion-lane worker import
    import ifcopenshell.geom  # ruff:ignore[import-outside-top-level]
    from honeybee.model import Model  # ruff:ignore[import-outside-top-level] — AGPL boundary import
    from honeybee.orientation import angles_from_num_orient, orient_index  # ruff:ignore[import-outside-top-level]
    from honeybee.room import Room  # ruff:ignore[import-outside-top-level]
    from ladybug_geometry.geometry3d.face import Face3D  # ruff:ignore[import-outside-top-level]
    from ladybug_geometry.geometry3d.pointvector import Point3D  # ruff:ignore[import-outside-top-level]
    from ladybug_geometry.geometry3d.polyface import Polyface3D  # ruff:ignore[import-outside-top-level]

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
        # the sweep's only progress signal: one beat per shape so an unbounded per-space native phase reports
        # monotonic extent through the parent-side conduit drain.
        pulsed(tap, GeometryPulse.BEM, PulseBeat(stage="space", done=ordinal + 1, total=len(spaces)))
        return Room.from_polyface3d(f"{policy.identifier}_{name}", polyface, policy.roof_angle, policy.floor_angle, policy.ground_depth)

    rooms = [room_of(space, ordinal) for ordinal, space in enumerate(spaces)]
    Room.solve_adjacency(rooms, policy.tolerance)
    angles = angles_from_num_orient(policy.orientation_count)
    for room in rooms:  # Exemption: honeybee mutates faces in place; the aperture mint is its owned surface.
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

- WIRE_PARITY-[OPEN]: does the C# `Rasm.Bim` HBJSON content key fold the SAME octets this page keys — `honeybee.Model.to_dict(included_prop=("energy",))` under `msgspec` deterministic encoding — given the C# end derives from `Model.ToJson()` and honeybee-schema's own serializer, or do the two serializers differ in key order, float spelling, or omitted-default handling; compare the installed `honeybee.model.Model.to_dict` output against a `honeybee-schema` `Model.ToJson()` render of the same document, then pin ONE byte source on both pages or seat a canonical re-encode at the crossing.

