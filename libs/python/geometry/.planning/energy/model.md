# [PY_GEOMETRY_ENERGY_MODEL]

`BuildingModel` owns HBJSON building-energy-model admission — the one place a building model becomes simulation-ready. `BuildingModel.of` discriminates two modalities under one `check_all` gate: wire-arrival HBJSON decode, and the COMPUTED BIM-to-BEM derivation lifting `IfcSpace` solids into an adjacency-solved, fenestrated honeybee model. `assign` folds one `EnergySpec` over the room set through the `.properties.energy` extension spine. `honeybee-core`/`honeybee-energy`/`ladybug-geometry` own every adjacency solver, aperture generator, HBJSON parser, and validation rule; this page composes them into one admitted, validated, energy-assigned, content-keyed model graduating under `GeometrySubject.BUILDING_ENERGY`.

Honeybee's AGPL-3.0 band rides the standing companion-lane charter — function-local boundary imports, evidence as document bytes. Wire form IS the cross-language contract: the C# `Rasm.Bim` Energy exchange and `Rasm.Compute` simulation are peers meeting at content-keyed HBJSON document bytes, one `XxHash128` derivation, never a shared client and never a mirrored shape. BIM-to-BEM is the modality only this folder can own — `ifcopenshell` binds as a direct package consume, never an `ifc/`-plane page import, and only space solids cross.

## [01]-[INDEX]

- [01]-[MODEL]: one polymorphic model owner — two-modality admission under one `check_all` gate, the assignment fold over the standards backends, the content-keyed wire egress — under one `ModelReceipt`.

## [02]-[MODEL]

- Owner: `BuildingModel` holds the validated honeybee `Model` and its `ContentKey` — the honeybee `Model` IS the canonical owner, never mirrored into a local dataclass graph, and the content-keyed bytes are its only projection. `BemPolicy` carries derivation behavior as data: the `wwr` default plus orientation-binned ratio rows, the classification bounds `Room.from_polyface3d` reads, and ONE tolerance every kernel reads, never three literals. `HvacSpec.template` keys `HVAC_TYPES_DICT`, which resolves the dynamically-built class, never a per-template import; `StandardsKind` keys `RESOLVERS`, so standards resolution is one row-keyed lookup.
- Entry: `of` is `async` over the source and the lane, and both admission arms converge on the ONE `check_all(raise_exception=False, detailed=True)` gate. `hbjson` decodes on the caller floor — a short pure decode earns no crossing — while the `bim` arm's repeated `ifcopenshell.geom.create_shape` sweep is the genuinely long native phase, so `_derived` crosses as `Kernel.of(_derived, KernelTrait.HOSTILE)` with picklable args: SPF bytes in, the pure-Python honeybee `Model` graph pickled home, the derivation content-keyed and run-scoped so the trait's `WORKER` death retry stays sound. `assign` re-runs the same gate — honeybee invokes every registered extension's checks automatically, so the energy-validity rows ride it — and the returned successor re-keys because assignment changed the document bytes. Fenestration mint walks exterior walls only, a zero ratio skipping the face.
- Auto: a missing standards identifier resolves a typed fault naming the identifier and the registry kind, never a bare `KeyError` — the extension-dependent `building_program_type_by_identifier` fault names the absent `honeybee-energy-standards` backend; orientation binning derives from `angles_from_num_orient`, so four/eight/sixteen-bin policies are one integer, never a compass ladder.
- Receipt: graduation residual is the validation-error fraction over the element census — a degenerate derivation minting no rooms keys `1.0` — so the compute crossing carries measured admission evidence, never a count clearing any ceiling.
- Packages: `honeybee-core`, `honeybee-energy`, `ladybug-geometry`, and `ifcopenshell` per the fence imports. Two standards backends merge behind the `lib` loaders — `honeybee-standards` defaults floor always resolvable, `honeybee-energy-standards` ASHRAE/DOE vintage sets when installed — never a direct standards-JSON read; `honeybee-schema` validates the HBJSON dict upstream of C#, and no parallel `msgspec` model family mirrors it.
- Growth: a new fenestration strategy is one `BemPolicy` row family — dimension-driven apertures ride `apertures_by_width_height_rectangle` as one more policy case; shading mints (`louvers_by_count`/`overhang`) enter as `EnergySpec` rows when a consumer names them; a new HVAC template or vintage is zero code — the registry and its `equipment_type` vocabulary are upstream data; a new standards kind is one `RESOLVERS` row; the daylight modality (`honeybee-radiance` plus sensor grids) enters only through a future package-admission motion; dragonfly-exploded models arrive as ordinary `hbjson` payloads from `energy/district`, no third modality.
- Boundary: IFC semantic analysis is the `ifc/` plane's — the BIM arm consumes ONLY space solids; simulation is `energy/simulate`'s, urban massing `energy/district`'s, weather `energy/climate`'s; the mesh daemon owns the GLB render wire — this arm's `create_shape` feeds `Face3D` lifting, never a cached render artifact.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Mapping
from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never

from expression import Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec import json as msgjson

from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometrySubject, evidence_run
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt
from rasm.runtime.workers import Kernel, KernelTrait

if TYPE_CHECKING:  # AGPL band: annotations resolve here; every runtime use is a function-local boundary import
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

# each kind keys its honeybee_energy.lib loader (module, function) — the single standards access path.
RESOLVERS: Final[Map[StandardsKind, tuple[str, str]]] = Map.of_seq([
    (StandardsKind.PROGRAM, ("honeybee_energy.lib.programtypes", "program_type_by_identifier")),
    (StandardsKind.BUILDING_PROGRAM, ("honeybee_energy.lib.programtypes", "building_program_type_by_identifier")),
    (StandardsKind.CONSTRUCTION_SET, ("honeybee_energy.lib.constructionsets", "construction_set_by_identifier")),
    (StandardsKind.SCHEDULE, ("honeybee_energy.lib.schedules", "schedule_by_identifier")),
    (StandardsKind.OPAQUE_CONSTRUCTION, ("honeybee_energy.lib.constructions", "opaque_construction_by_identifier")),
    (StandardsKind.WINDOW_CONSTRUCTION, ("honeybee_energy.lib.constructions", "window_construction_by_identifier")),
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
    rooms: Option[tuple[str, ...]] = Nothing  # room identifiers; Nothing assigns model-wide


class ModelReceipt(Struct, frozen=True):
    source: str
    rooms: int
    faces: int
    apertures: int
    shades: int
    check_rows: int
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
                    "content_key": self.content_key.hex,
                },
            ),
        )

    def graduates(self, key: ContentKey, ceiling: float) -> GeometryHandoff:
        census = max(self.rooms + self.faces + self.apertures, 1)
        residual = 1.0 if self.rooms == 0 else self.check_rows / census
        return GeometryHandoff.of(
            GeometrySubject.BUILDING_ENERGY, key, {"invalid": residual, "rooms": float(self.rooms), "apertures": float(self.apertures)}, {"invalid": ceiling}
        )


# --- [SERVICES] -------------------------------------------------------------------------


class BuildingModel(Struct, frozen=True):
    model: "Model"
    content_key: ContentKey

    @classmethod
    async def of(cls, source: ModelSource, lane: LanePolicy) -> "RuntimeRail[Self]":
        async def admit() -> "RuntimeRail[Self]":
            match source:
                case ModelSource(tag="hbjson", hbjson=payload):
                    # short pure decode: caller-floor by charter, no crossing earned.
                    return Ok(cls._gated(_decoded(payload), "hbjson"))
                case ModelSource(tag="bim", bim=(spf, policy)):
                    # create_shape sweep crosses HOSTILE with picklable args — SPF bytes in, the pure-Python
                    # honeybee Model graph pickled home — and the caller-side gate re-proves it before admission.
                    derived = await lane.offload(Kernel.of(_derived, KernelTrait.HOSTILE), spf, policy)
                    return derived.map(lambda model: cls._gated(model, "bim"))
                case _ as unreachable:
                    assert_never(unreachable)

        return await evidence_run(EvidenceScope.ENERGY_MODEL, f"admit.{source.tag}", admit)

    def assign(self, spec: EnergySpec) -> "RuntimeRail[Self]":
        def fold() -> Self:
            import honeybee_energy  # noqa: PLC0415 — the _extend_honeybee side effect registers .properties.energy
            from honeybee_energy.hvac import HVAC_TYPES_DICT  # noqa: PLC0415
            from honeybee_energy.shw import SHWSystem  # noqa: PLC0415 — the SHW template mint; no lib registry exists

            program = spec.program.map(lambda ident: _resolved(StandardsKind.PROGRAM, ident)).to_optional()
            constructions = spec.construction_set.map(lambda ident: _resolved(StandardsKind.CONSTRUCTION_SET, ident)).to_optional()
            hvac = spec.hvac.map(
                lambda h: HVAC_TYPES_DICT[h.template](f"{h.template}_system")
                if h.equipment_type.is_none()
                else HVAC_TYPES_DICT[h.template](f"{h.template}_system", equipment_type=h.equipment_type.value)
            ).to_optional()
            shw = spec.shw.map(SHWSystem).to_optional()
            wanted = spec.rooms.map(frozenset).default_value(frozenset())
            for room in self.model.rooms:  # Exemption: the honeybee Model mutates in place; assignment is its owned imperative surface.
                if wanted and room.identifier not in wanted:
                    continue
                energy = room.properties.energy
                energy.program_type = program if program is not None else energy.program_type
                energy.construction_set = constructions if constructions is not None else energy.construction_set
                energy.hvac = hvac if hvac is not None else energy.hvac
                energy.shw = shw if shw is not None else energy.shw
            return type(self)._gated(self.model, "assign")

        return evidence_run(EvidenceScope.ENERGY_MODEL, "assign", fold)

    def wire(self) -> "RuntimeRail[tuple[bytes, ContentKey]]":
        # wire key is SEED-ZERO over the format-key-then-document chunk stream, reproducing the C# CanonicalWriter
        # String(format.Key).Raw(bytes) fold; the reproduction-corpus HBJSON golden fixture proves the parity cross-runtime.
        def fold() -> tuple[bytes, ContentKey]:
            document = _ENCODER.encode(self.model.to_dict(included_prop=("energy",)))
            return document, ContentIdentity.key("hbjson", (b"hbjson", document), seed=Some(0))

        return evidence_run(EvidenceScope.ENERGY_MODEL, "wire", fold)

    def hbjson(self, folder: Path) -> "RuntimeRail[Path]":
        return evidence_run(
            EvidenceScope.ENERGY_MODEL, "hbjson", lambda: Path(self.model.to_hbjson(name=self.model.identifier, folder=str(folder)))
        )

    def receipt(self, source: str, check_rows: int, spec: Option[EnergySpec] = Nothing) -> ModelReceipt:
        return ModelReceipt(
            source=source,
            rooms=len(self.model.rooms),
            faces=len(self.model.faces),
            apertures=len(self.model.apertures),
            shades=len(self.model.shades),
            check_rows=check_rows,
            program=spec.bind(lambda s: s.program),
            construction_set=spec.bind(lambda s: s.construction_set),
            hvac=spec.bind(lambda s: s.hvac.map(lambda h: h.template)),
            content_key=self.content_key,
        )

    @classmethod
    def _gated(cls, model: "Model", modality: str) -> Self:
        # detailed check rows fold to a typed fault census (code -> count).
        rows = model.check_all(raise_exception=False, detailed=True)
        if rows:
            census = Block.of_seq(rows).fold(lambda acc, row: acc.change(str(row.get("code", "?")), lambda n: Some(n.default_value(0) + 1)), Map.empty())
            raise ValueError(f"check_all:{modality}:{len(rows)}:{dict(census.to_seq())}")  # converted once by the evidence_run fence
        document = _ENCODER.encode(model.to_dict(included_prop=("energy",)))
        return cls(model=model, content_key=ContentIdentity.key("energy-model", document))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _decoded(payload: "bytes | str | Path | Mapping[str, object]") -> "Model":
    from honeybee.model import Model  # noqa: PLC0415 — AGPL boundary import

    match payload:
        case bytes() as raw:
            return Model.from_dict(msgjson.decode(raw))
        case Mapping() as data:
            return Model.from_dict(dict(data))
        case at:
            return Model.from_hbjson(str(at))


def _derived(spf: bytes, policy: BemPolicy) -> "Model":
    # BIM-to-BEM: IfcSpace solids -> Face3D triangles -> Polyface3D -> Room -> adjacency -> apertures.
    # module-level HOSTILE kernel: ships REFERENCE onto the warm process pool; the live ifcopenshell.file stays worker-local.
    import ifcopenshell  # noqa: PLC0415 — companion-lane worker import
    import ifcopenshell.geom  # noqa: PLC0415
    from honeybee.model import Model  # noqa: PLC0415 — AGPL boundary import
    from honeybee.orientation import angles_from_num_orient, orient_index  # noqa: PLC0415
    from honeybee.room import Room  # noqa: PLC0415
    from ladybug_geometry.geometry3d.face import Face3D  # noqa: PLC0415
    from ladybug_geometry.geometry3d.pointvector import Point3D  # noqa: PLC0415
    from ladybug_geometry.geometry3d.polyface import Polyface3D  # noqa: PLC0415

    ifc = ifcopenshell.file.from_string(spf.decode())
    settings = ifcopenshell.geom.settings()
    settings.set("use-world-coords", True)

    def room_of(space: object, ordinal: int) -> Room:
        shape = ifcopenshell.geom.create_shape(settings, space)
        verts, faces = shape.geometry.verts, shape.geometry.faces
        points = tuple(Point3D(verts[i], verts[i + 1], verts[i + 2]) for i in range(0, len(verts), 3))
        triangles = tuple(Face3D((points[faces[i]], points[faces[i + 1]], points[faces[i + 2]])) for i in range(0, len(faces), 3))
        polyface = Polyface3D.from_faces(triangles, policy.tolerance)
        name = getattr(space, "GlobalId", f"{policy.identifier}_space_{ordinal}")
        return Room.from_polyface3d(f"{policy.identifier}_{name}", polyface, policy.roof_angle, policy.floor_angle, policy.ground_depth)

    rooms = [room_of(space, ordinal) for ordinal, space in enumerate(ifc.by_type("IfcSpace"))]
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


def _resolved(kind: StandardsKind, identifier: str) -> object:
    from importlib import import_module  # noqa: PLC0415 — row-resolved AGPL boundary import

    module, loader = RESOLVERS[kind]
    return getattr(import_module(module), loader)(identifier)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
