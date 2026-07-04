# [PY_GEOMETRY_ENERGY_MODEL]

The HBJSON building-energy-model owner — the plane's canonical model admission and the one place a building model becomes simulation-ready. `BuildingModel.of` is ONE polymorphic admission discriminating by input shape, both modalities fronted by the same `check_all` validation gate, never a second entry: HBJSON document bytes ARRIVE over the wire and decode (`Model.from_dict`/`Model.from_hbjson` by payload shape), and the BIM-to-BEM derivation is COMPUTED here — the modality only this folder can own — `ifcopenshell` bound directly as a package consume (never an ifc-plane page import): `by_type("IfcSpace")` selects the space set, `geom.create_shape` tessellates each space solid, the triangle soup lifts through `Face3D`/`Polyface3D.from_faces` into `Room.from_polyface3d`, `Room.solve_adjacency(rooms, tolerance)` closes the zone graph with `Surface` boundary pairs, and the honeybee aperture generators (`Face.apertures_by_ratio*` composing the `Face3D` `sub_faces_by_ratio*` substrate) mint fenestration where the source model carries none — window-to-wall ratio as orientation-binned policy rows on `BemPolicy`, never a hand-cut opening. The admitted model carries its energy layer through the `.properties.energy` extension spine: `assign` folds one `EnergySpec` over the room set — program type, construction set, HVAC template (the `HVAC_TYPES_DICT` registry row plus its `equipment_type` vintage vocabulary), and service hot water minted as its `SHWSystem(identifier)` template row (the `lib` loaders carry no SHW registry — the system is a template mint, not a backend lookup) — program, construction-set, schedule, and construction identifiers resolve through the two standards backends (`honeybee-standards` defaults floor always resolvable, `honeybee-energy-standards` ASHRAE 90.1/DOE vintage sets when installed) via the `honeybee_energy.lib` loaders — seed rows, never hand-copied data, never a direct read of the standards JSON.

The model's wire form IS the cross-language contract: `wire()` serializes `to_dict(included_prop=("energy",))`, encodes canonical bytes through `msgspec`, and keys them through the `rasm.runtime.identity` owner — the C# `Rasm.Bim` Energy exchange and `Rasm.Compute` simulation are peers meeting at these content-keyed HBJSON document bytes, one `XxHash128` derivation, never a shared client and never a mirrored shape. Every entry is a producer on the folder evidence spine — the `evidence_run` weave, a typed `ModelReceipt`, and graduation under the geometry-minted `GeometrySubject` `"building-energy"` member with the validation-residual ledger measured against caller ceilings. The honeybee band is AGPL-3.0 riding the standing companion-lane charter: all imports are function-local at the boundary kernels, evidence crosses as document bytes. This owner re-implements no adjacency solver, no aperture generator, no HBJSON parser, no construction/schedule/U-value machinery, and no validation rule — `honeybee-core`/`honeybee-energy`/`ladybug-geometry` own those kernels; it composes them into one admitted, validated, energy-assigned, content-keyed model.

## [01]-[INDEX]

- [01]-[MODEL]: the one polymorphic `BuildingModel` owner — the two-modality `ModelSource` admission under one `check_all` gate, the `BemPolicy` derivation policy, the `EnergySpec` assignment fold over the standards backends, the content-keyed HBJSON wire egress, the `ModelReceipt` evidence, and the `building-energy` graduation.

## [02]-[MODEL]

- Owner: `BuildingModel` — the admitted capsule holding the validated honeybee `Model` handle and its `ContentKey`; `ModelSource` the closed two-case admission union — `hbjson` (document bytes, a path, or a decoded mapping — the wire-arrival modality) and `bim` (IFC SPF source bytes plus the `BemPolicy` derivation policy — the computed modality); `BemPolicy` the derivation policy value — model identifier, tolerance/angle-tolerance, the `wwr` default ratio plus `by_orientation` ratio rows binned by `orientation_count`, the ground-depth/roof-angle classification bounds `Room.from_polyface3d` reads, and the skylight ratio — behavior as data, never constructor knobs re-derived per call; `EnergySpec` the one assignment request — `Option` program-type identifier, `Option` construction-set identifier, `Option[HvacSpec]` template row, `Option` SHW identifier, and the room-filter predicate — so energy assignment is one fold over `.properties.energy`, never per-room imperative pokes; `HvacSpec` the template selection (the `HVAC_TYPES_DICT` registry name plus `Option` `equipment_type` vintage/efficiency member — the registry resolves the dynamically-built class, never a per-template import); `StandardsKind` the closed resolver vocabulary keying `RESOLVERS` to the `honeybee_energy.lib` loader name (`program_type_by_identifier`/`building_program_type_by_identifier`/`construction_set_by_identifier`/`schedule_by_identifier`/`opaque_construction_by_identifier`/`window_construction_by_identifier`) so standards resolution is one row-keyed lookup; `ModelReceipt` the `ReceiptContributor` evidence — room/face/aperture/shade tallies, the validation row count, the source modality, and the model `ContentKey`.
- Cases: `ModelSource.hbjson` admits `bytes | str | Path | Mapping[str, object]` — bytes decode through `msgspec.json.decode` then `Model.from_dict`, a path opens `Model.from_hbjson`, a mapping reconstructs `Model.from_dict` — the payload shape is the discriminant; `ModelSource.bim` carries `(bytes, BemPolicy)` — the SPF source bytes the ingress ledger names, never a live ifc handle from another page.
- Entry: `BuildingModel.of` runs the one admission fold inside the `evidence_run` weave: the `hbjson` arm decodes; the `bim` arm derives — `ifcopenshell.file.from_string(source.decode())` opens the model in memory, `geom.settings()` with world coordinates configures the tessellator, each `by_type("IfcSpace")` entity crosses `geom.create_shape(settings, space)` into its triangulated solid, the vertex/face buffers regroup into per-triangle `Face3D` value objects lifted through `Polyface3D.from_faces(faces, tolerance)`, `Room.from_polyface3d(identifier, polyface, roof_angle, floor_angle, ground_depth)` classifies walls/roofs/floors from face normals, `Room.solve_adjacency(rooms, tolerance)` pairs coincident faces into `Surface` boundary conditions, and the fenestration mint walks each exterior wall: `orient_index(face.horizontal_orientation(), angles)` bins the orientation against `angles_from_num_orient(policy.orientation_count)` and `face.apertures_by_ratio(ratio, tolerance)` mints the bin's ratio row (the policy's `by_orientation` row or the `wwr` default; a zero ratio skips the face) with `Model.skylight_apertures_by_ratio` covering roofs when the policy carries a skylight ratio — then BOTH arms converge on the ONE gate: `Model(identifier, rooms, units, tolerance, angle_tolerance)` assembles, `check_all(raise_exception=False, detailed=True)` returns the structured error rows, an empty row set admits `BuildingModel` with its `ContentIdentity.key("energy-model", ...)` key, and a non-empty set is a typed fault carrying the error-code census — never a raise, never an unvalidated model escaping inward. `assign(spec)` imports `honeybee_energy` once at the boundary (the `_extend_honeybee` side effect registers `.properties.energy`), resolves each carried identifier through its `RESOLVERS` row, constructs the HVAC template through `HVAC_TYPES_DICT[spec.hvac.template]` with the `equipment_type` member applied, folds the assignment over the filtered room set through `room.properties.energy`, and re-runs the SAME `check_all` gate (honeybee-core invokes every registered extension's checks automatically, so the energy-validity rows — one-HVAC-per-zone, duplicate resource ids — ride the one gate); the returned successor re-keys, because assignment changed the document bytes. `wire()` projects `to_dict(included_prop=("energy",))`, encodes with the module-level `msgspec.json.Encoder`, and returns `(bytes, ContentKey)` — the seam payload the C# Energy exchange decodes, its key derived seed-zero (`Some(0)`) over the format-key-then-document chunk stream reproducing the C# `CanonicalWriter` `String(format.Key).Raw(bytes)` fold, the reproduction-corpus HBJSON golden fixture the cross-runtime parity proof; `hbjson()` writes the durable `.hbjson` artifact for the recipe rail's model input.
- Auto: every arm rides `evidence_run(EvidenceScope.ENERGY_MODEL, <operation>, dispatch)` — one span, one boundary fence, one receipt harvest; the derivation modality never re-validates what admission proved (the interior holds the validated `Model`, and `assign` is the one transition that re-proves); tolerance discipline is the policy's — `solve_adjacency`, `from_faces`, and `apertures_by_ratio` all read `BemPolicy.tolerance`, one value, never three literals; the SPF byte intake is the in-memory `file.from_string` arm, never a temp-file round trip; orientation binning derives from `angles_from_num_orient` so four/eight/sixteen-bin policies are one integer, never a compass ladder; identifiers entering the standards resolvers are seed DATA on `EnergySpec` — a missing identifier resolves a typed fault naming the identifier and the registry kind, never a bare `KeyError` (the extension-dependent `building_program_type_by_identifier` fault names the absent `honeybee-energy-standards` backend).
- Receipt: `ModelReceipt` carries the source modality, room/face/aperture/shade counts, the assigned program/construction-set/HVAC discriminants, the validation row count, and the `ContentKey`; `contribute` yields one emitted-phase `Receipt.of("rasm.geometry.energy.model", ("emitted", subject, facts))` triple — the receipts owner's `Evidence` shape; `graduates(key, ceiling)` folds into `GeometryHandoff` under `GeometrySubject.BUILDING_ENERGY` — the residual is the validation-error fraction over the element census (a clean model graduates at zero; a degenerate derivation that minted no rooms keys `1.0`), so the compute crossing carries measured admission evidence, never a count that clears any ceiling.
- Packages: `honeybee-core` (`model.Model` — assembly/`from_dict`/`from_hbjson`/`to_dict(included_prop=...)`/`to_hbjson`/`check_all(raise_exception=False, detailed=True)`/`wall_apertures_by_ratio`/`skylight_apertures_by_ratio`, `room.Room.from_polyface3d`/`solve_adjacency`/`intersect_adjacency`, `face.Face.apertures_by_ratio`/`apertures_by_ratio_rectangle`/`louvers_by_count`/`overhang`, `orientation.angles_from_num_orient`/`orient_index`, `boundarycondition.boundary_conditions`/`facetype.face_types` singletons, `dictutil.dict_to_object` the polymorphic decode — all function-local, AGPL-3.0 companion posture), `honeybee-energy` (the `_extend_honeybee` import side effect, `properties.energy` assignment surface, `hvac.HVAC_TYPES_DICT` + per-template `equipment_type`, `shw.SHWSystem` the service-hot-water template mint, `lib.programtypes`/`lib.constructionsets`/`lib.schedules`/`lib.constructions` by-identifier loaders — the SINGLE standards access path), `honeybee-standards`/`honeybee-energy-standards` (data-only backends reached exclusively through the `lib` loaders — the defaults floor always resolvable, the ASHRAE/DOE catalog extension-gated), `ladybug-geometry` (`Face3D`, `Polyface3D.from_faces`, the `sub_faces_by_ratio*`/`contour_fins_*` substrate the honeybee generators compose — never a second geometry kernel), `ifcopenshell` (`file.from_string`/`by_type`/`geom.settings`/`geom.create_shape` — the direct package consume for the BIM-to-BEM arm; the eighth consuming page, never an `ifc/` page import), `msgspec` (`json.Encoder`/`json.decode` the canonical HBJSON byte codec), `expression` (`Block`/`Map`/`Option`/`tagged_union`), geometry (`evidence_run`/`EvidenceScope`/`GeometryHandoff`/`GeometrySubject`), runtime (`ContentIdentity`/`ContentKey`, `RuntimeRail`/`BoundaryFault`, `Receipt`).
- Growth: a new fenestration strategy is one `BemPolicy` row family (dimension-driven apertures ride `apertures_by_width_height_rectangle` as one more policy case); shading mints (`louvers_by_count`/`overhang`) enter as `EnergySpec` rows when a consumer names them; a new HVAC template or vintage is zero code (the registry and its `equipment_type` vocabulary are upstream data); a new standards kind is one `RESOLVERS` row; the daylight modality (`honeybee-radiance` model properties plus sensor grids) extends this owner only through a future package-admission motion carrying its own catalogs and consuming fences — admission-gated growth, never a pre-admitted arm; dragonfly-exploded models arrive as ordinary `hbjson` payloads from `energy/district`, no third modality.
- Boundary: no IFC semantic analysis (the `ifc/` plane owns Psets/IDS/clash — this arm consumes ONLY space solids), no simulation (translation and execution are `energy/simulate`'s), no urban massing (`energy/district`), no weather (`energy/climate`), no GLB tessellation (the mesh daemon owns the render wire; this arm's `create_shape` feeds `Face3D` lifting, never a cached render artifact), and no schema re-mint (the HBJSON dict is the wire; `honeybee-schema` validates it upstream of C#, and no parallel `msgspec` model family mirrors it). The deleted forms: a second admission entry (`of_hbjson`/`of_ifc` siblings) where one `of` discriminates the `ModelSource` case; an unvalidated modality (a derivation path skipping `check_all` — both arms front the ONE gate); a hand-rolled adjacency or aperture cut where `solve_adjacency`/`apertures_by_ratio` own the topology; a direct standards-JSON read or a hand-copied construction table where the `lib` loaders resolve by identifier; a `provider=` knob where the payload shape discriminates; per-face tolerance literals where `BemPolicy` carries the one value; a temp-file SPF round trip where `file.from_string` opens bytes; and a model mirrored into a local dataclass graph where the honeybee `Model` IS the canonical owner and the content-keyed bytes are its only projection.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Mapping
from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never

from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec import json as msgjson

from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometrySubject, evidence_run
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.receipts import Receipt

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

# each kind keys its honeybee_energy.lib loader (module, function) — the SINGLE standards access
# path; the two data backends (defaults floor, ASHRAE/DOE extension) merge behind these loaders.
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
    def of(cls, source: ModelSource) -> "RuntimeRail[Self]":
        def admit() -> Self:
            match source:
                case ModelSource(tag="hbjson", hbjson=payload):
                    return cls._gated(_decoded(payload), "hbjson")
                case ModelSource(tag="bim", bim=(spf, policy)):
                    return cls._gated(_derived(spf, policy), "bim")
                case _ as unreachable:
                    assert_never(unreachable)

        return evidence_run(EvidenceScope.ENERGY_MODEL, f"admit.{source.tag}", admit)

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
        # the cross-language seam: content-keyed canonical HBJSON document bytes. The C# Energy
        # exchange keys SEED-ZERO over the CanonicalWriter String(format.Key).Raw(bytes) fold, so
        # the wire key streams the format-key chunk before the document at Some(0) — the
        # reproduction-corpus HBJSON golden fixture proves this transcription cross-runtime.
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
        # THE one admission gate both modalities front: detailed check rows fold to a typed fault
        # census (code -> count), never a raise and never an unvalidated model escaping inward.
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
    # ifcopenshell is a direct package consume (the eighth consuming page); only space solids cross.
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
