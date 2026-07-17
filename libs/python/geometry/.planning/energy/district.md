# [PY_GEOMETRY_ENERGY_DISTRICT]

`District` owns the urban-district 2.5-D massing layer above the building model, where a city block or campus stays compact until it explodes into detailed energy models. `District.of` admits the dragonfly graph from a dfjson document, an anchored GeoJSON footprint import, or a computed massing specification; `zone` pairs the plates, `explode` crosses to detailed models, `translate` builds the district-energy egress inputs, and `assign` mirrors the model page's energy assignment over `Room2D` hosts. `dragonfly-core` owns every footprint extrusion, adjacency solver, equirectangular projection, and multiplier-arithmetic kernel; this page composes them into typed, railed, receipted evidence graduating under `GeometrySubject.BUILDING_ENERGY`.

Dragonfly's AGPL-3.0 band rides the standing companion-lane charter — function-local boundary imports, evidence across the wire, no link into a distributed binary. `explode` is the seam onto `energy/model`: emitted honeybee dicts cross straight into `BuildingModel.of` — district constructs, model page admits, ONE gate per tier. Standards assignment reuses the `energy/model` `RESOLVERS` rows downward, one standards access path folder-wide. URBANopt CLI, Modelica, RNM, and the REopt API behind the translate inputs are external process-boundary services this page never drives — it builds the TYPED INPUTS only; run orchestration enters through a future admission motion carrying its own engine provisioning. Every admitted district keys once over its canonical dfjson bytes.

## [01]-[INDEX]

- [01]-[DISTRICT]: one polymorphic district owner — admission under one `check_all` gate, ordered zoning, the explosion seam, the translation union — under one `DistrictReceipt`.

## [02]-[DISTRICT]

- Owner: `District` holds the validated dragonfly `Model`, its `ContentKey`, and the `LanePolicy` the explosion seam threads into each emitted model's admission. `Anchor` carries the geo registration constructed once and projected into the ladybug `Location` at each kernel, never four loose floats per call; `ExplodePolicy` and `DistrictTarget` carry behavior as data over dragonfly's own knobs — one row per district-energy egress, never a per-target method family.
- Entry: every admission arm converges on the ONE `check_all(raise_exception=False, detailed=True)` gate whose defect rows fold to a typed fault census. `zone` runs the only legal order — `intersect_adjacency` splits coincident walls THEN `solve_adjacency` pairs them, never a solve over un-intersected plates — and returns the re-keyed successor. `explode` is `async`, awaiting `BuildingModel.of(..., self.lane)` per emitted honeybee dict over the held lane; failed building admissions accumulate through `traversed(ACCUMULATE)` so every bad building names itself in the combined fault, never a first-fault abort hiding siblings.
- Auto: `use_multiplier=True` keeps the compact graph and `False` instances every floor — a policy value on the explode row; equirectangular meters-to-lon-lat correspondence stays `dragonfly.projection`'s, never re-derived; `des_param`/`opendss`/`reopt` targets consume CALLER-authored dragonfly-energy value objects — this page routes them through the translation seam and re-mints none of their vocabulary.
- Receipt: the graduation residual DERIVES from the receipt's own segment census — `unzoned_segments`/`total_segments`, a fully-zoned district graduating at zero — never a caller-supplied fraction; `modeled_floors` sums story multipliers, the real modeled-floor census.
- Packages: `dragonfly-core` (the module is `dragonfly`, never `dragonfly_core`) and `dragonfly-energy` per the fence imports; `FourthGenThermalLoop` is the 4th-gen hot/chilled loop, `GHEThermalLoop` the 5th-gen ground-heat-exchanger ambient loop with borefield sizing.
- Growth: a new translation egress is one `DistrictTarget` case plus one dispatch arm; window/skylight/shading parameter families attach as `BuildingSpec` rows when a consumer names them; the URBANopt/DES/RNM/REopt run drivers enter only through a future admission motion provisioning their engines; GeoJSON parcel-layer ingest at scale composes the data folder's geospatial owners at the data seam, never a geometry-side `geopandas` import.
- Boundary: rooms/faces/apertures are `energy/model`'s — this page stops at `Room2D` plates and the explosion seam; simulation and result decode are `energy/simulate`'s, weather is `energy/climate`'s; accurate CRS work beyond dragonfly's own equirectangular helpers is the data folder's `pyproj` plane.

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

from rasm.geometry.energy.model import RESOLVERS, BuildingModel, EnergySpec, ModelSource, StandardsKind
from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometrySubject, evidence_run
from rasm.runtime.faults import Disposition, RuntimeRail, traversed
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:  # AGPL band: annotations resolve here; every runtime use is a function-local boundary import
    from dragonfly.model import Model as DistrictGraph
    from dragonfly_energy.des.loop import FourthGenThermalLoop, GHEThermalLoop
    from dragonfly_energy.opendss.network import ElectricalNetwork
    from dragonfly_energy.reopt import REoptParameter

# --- [TYPES] ----------------------------------------------------------------------------


class PerModel(StrEnum):
    DISTRICT = "District"
    BUILDING = "Building"
    STORY = "Story"


type Ring = tuple[tuple[float, float, float], ...]  # one footprint boundary as xyz tuples

# --- [CONSTANTS] ------------------------------------------------------------------------

_ENCODER: Final = msgjson.Encoder(order="deterministic")  # canonical dfjson bytes — one module-level codec, never per-call

# --- [MODELS] ---------------------------------------------------------------------------


class Anchor(Struct, frozen=True, gc=False):
    latitude: float
    longitude: float
    elevation: float = 0.0
    origin: tuple[float, float] = (0.0, 0.0)


class BuildingSpec(Struct, frozen=True):
    identifier: str
    footprint: tuple[Ring, ...]
    floor_to_floor: tuple[float, ...]
    perimeter_offset: float = 0.0


class MassingPolicy(Struct, frozen=True, gc=False):
    identifier: str
    units: str = "Meters"
    tolerance: float = 0.01
    angle_tolerance: float = 1.0


@tagged_union(frozen=True)
class DistrictSource:
    tag: Literal["dfjson", "geojson", "massing"] = tag()
    dfjson: "bytes | str | Path | Mapping[str, object]" = case()
    geojson: tuple[Path, Anchor] = case()
    massing: tuple[Block[BuildingSpec], MassingPolicy] = case()


class ExplodePolicy(Struct, frozen=True, gc=False):
    per_model: PerModel = PerModel.BUILDING
    use_multiplier: bool = True
    add_plenum: bool = False
    solve_ceiling_adjacencies: bool = False
    enforce_adjacency: bool = True
    enforce_solid: bool = True


@tagged_union(frozen=True)
class DistrictTarget:
    tag: Literal["urbanopt", "des_param", "opendss", "reopt", "geojson"] = tag()
    urbanopt: tuple[Path, Anchor, "Option[FourthGenThermalLoop | GHEThermalLoop]", "Option[ElectricalNetwork]"] = case()
    des_param: "FourthGenThermalLoop | GHEThermalLoop" = case()
    opendss: tuple["ElectricalNetwork", Anchor] = case()
    reopt: tuple["REoptParameter", str, str] = case()  # (parameter, base_file, urdb_label)
    geojson: tuple[Path, Anchor] = case()


class DistrictReceipt(Struct, frozen=True):
    buildings: int
    stories: int
    room2ds: int
    modeled_floors: int
    footprint_area: float
    floor_area: float
    exploded: int
    target: Option[str]
    content_key: ContentKey
    unzoned_segments: int = 0  # wall segments whose boundary condition is not Surface after zoning
    total_segments: int = 0

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of(
            "rasm.geometry.energy.district",
            (
                "emitted",
                self.target.default_value("admit"),
                {
                    "buildings": self.buildings,
                    "stories": self.stories,
                    "room2ds": self.room2ds,
                    "modeled_floors": self.modeled_floors,
                    "exploded": self.exploded,
                    "unzoned_segments": self.unzoned_segments,
                    "total_segments": self.total_segments,
                    "content_key": self.content_key.hex,
                },
            ),
        )

    def graduates(self, key: ContentKey, ceiling: float) -> GeometryHandoff:
        # an empty segment census carries no zoning evidence, so it reads fully unzoned (residual 1.0) and refuses the
        # ceiling — a `max(total, 1)` fallback would graduate a segmentless district as fully zoned.
        residual = self.unzoned_segments / self.total_segments if self.total_segments else 1.0
        return GeometryHandoff.of(
            GeometrySubject.BUILDING_ENERGY,
            key,
            {"unzoned": residual, "buildings": float(self.buildings), "floor_area": self.floor_area},
            {"unzoned": ceiling},
        )


# --- [SERVICES] -------------------------------------------------------------------------


class District(Struct, frozen=True):
    graph: "DistrictGraph"
    content_key: ContentKey
    lane: LanePolicy  # the lane the explosion seam hands each emitted model's BIM-capable admission

    @classmethod
    def of(cls, source: DistrictSource, lane: LanePolicy) -> "RuntimeRail[Self]":
        def admit() -> Self:
            from dragonfly.model import Model  # noqa: PLC0415 — AGPL boundary import; the module is `dragonfly`

            match source:
                case DistrictSource(tag="dfjson", dfjson=bytes() as raw):
                    graph = Model.from_dict(msgjson.decode(raw))
                case DistrictSource(tag="dfjson", dfjson=Mapping() as data):
                    graph = Model.from_dict(dict(data))
                case DistrictSource(tag="dfjson", dfjson=at):
                    graph = Model.from_file(str(at))
                case DistrictSource(tag="geojson", geojson=(path, anchor)):
                    graph = Model.from_geojson(str(path), location=_location(anchor), point=_point(anchor))
                case DistrictSource(tag="massing", massing=(specs, policy)):
                    graph = _massed(specs, policy)
                case _ as unreachable:
                    assert_never(unreachable)
            return cls._gated(graph, lane)

        return evidence_run(EvidenceScope.ENERGY_DISTRICT, f"admit.{source.tag}", admit)

    def zone(self, tolerance: float = 0.01) -> "RuntimeRail[Self]":
        def fold() -> Self:
            from dragonfly.room2d import Room2D  # noqa: PLC0415 — AGPL boundary import

            for story in self.graph.stories:  # Exemption: dragonfly zones stories in place; the ordered pair is its owned surface.
                Room2D.intersect_adjacency(story.room_2ds, tolerance)
                Room2D.solve_adjacency(story.room_2ds, tolerance)
            return type(self)._gated(self.graph, self.lane)

        return evidence_run(EvidenceScope.ENERGY_DISTRICT, "zone", fold)

    async def explode(self, policy: ExplodePolicy) -> "RuntimeRail[Block[BuildingModel]]":
        async def fold() -> "RuntimeRail[Block[BuildingModel]]":
            emitted = self.graph.to_honeybee(
                object_per_model=policy.per_model.value,
                use_multiplier=policy.use_multiplier,
                add_plenum=policy.add_plenum,
                solve_ceiling_adjacencies=policy.solve_ceiling_adjacencies,
                enforce_adj=policy.enforce_adjacency,
                enforce_solid=policy.enforce_solid,
            )
            # hbjson admissions run caller-floor inside the model page's own weave; the sequential await is the
            # statement-bearing async fold, and every failed building accumulates rather than aborting its siblings.
            rails = Block.of_seq([await BuildingModel.of(ModelSource(hbjson=model.to_dict()), self.lane) for model in emitted])
            return traversed(rails, by=Disposition.ACCUMULATE)

        return await evidence_run(EvidenceScope.ENERGY_DISTRICT, "explode", fold)

    def assign(self, spec: EnergySpec) -> "RuntimeRail[Self]":
        # one fold over Room2D hosts BEFORE the explode, so a multiplier story assigns once.
        def fold() -> Self:
            from importlib import import_module  # noqa: PLC0415 — row-resolved AGPL boundary import

            import dragonfly_energy  # noqa: F401, PLC0415 — the _extend_dragonfly side effect registers .properties.energy
            from honeybee_energy.hvac import HVAC_TYPES_DICT  # noqa: PLC0415
            from honeybee_energy.shw import SHWSystem  # noqa: PLC0415 — the SHW template mint; no lib registry exists

            def resolved(kind: StandardsKind, identifier: str) -> object:
                module, loader = RESOLVERS[kind]
                return getattr(import_module(module), loader)(identifier)

            program = spec.program.map(lambda ident: resolved(StandardsKind.PROGRAM, ident)).to_optional()
            constructions = spec.construction_set.map(lambda ident: resolved(StandardsKind.CONSTRUCTION_SET, ident)).to_optional()
            hvac = spec.hvac.map(
                lambda h: HVAC_TYPES_DICT[h.template](f"{h.template}_system")
                if h.equipment_type.is_none()
                else HVAC_TYPES_DICT[h.template](f"{h.template}_system", equipment_type=h.equipment_type.value)
            ).to_optional()
            shw = spec.shw.map(SHWSystem).to_optional()
            wanted = spec.rooms.map(frozenset).default_value(frozenset())
            for room in self.graph.room_2ds:  # Exemption: dragonfly mutates hosts in place; assignment is its owned imperative surface.
                if wanted and room.identifier not in wanted:
                    continue
                energy = room.properties.energy
                energy.program_type = program if program is not None else energy.program_type
                energy.construction_set = constructions if constructions is not None else energy.construction_set
                energy.hvac = hvac if hvac is not None else energy.hvac
                energy.shw = shw if shw is not None else energy.shw
            return type(self)._gated(self.graph, self.lane)

        return evidence_run(EvidenceScope.ENERGY_DISTRICT, "assign", fold)

    def receipt(self, exploded: int = 0, target: Option[str] = Nothing) -> DistrictReceipt:
        # a non-Surface boundary condition after zoning is an adjacency-unsolved segment.
        conditions = [type(bc).__name__ for room in self.graph.room_2ds for bc in room.boundary_conditions]
        return DistrictReceipt(
            buildings=len(self.graph.buildings),
            stories=len(self.graph.stories),
            room2ds=len(self.graph.room_2ds),
            modeled_floors=sum(story.multiplier for story in self.graph.stories),
            footprint_area=self.graph.footprint_area,
            floor_area=self.graph.floor_area,
            exploded=exploded,
            target=target,
            content_key=self.content_key,
            unzoned_segments=sum(1 for name in conditions if name != "Surface"),
            total_segments=len(conditions),
        )

    def translate(self, target: DistrictTarget) -> "RuntimeRail[object]":
        def fold() -> object:
            match target:
                case DistrictTarget(tag="urbanopt", urbanopt=(folder, anchor, loop, network)):
                    from dragonfly_energy.writer import model_to_urbanopt  # noqa: PLC0415 — AGPL boundary import

                    return model_to_urbanopt(
                        self.graph,
                        _location(anchor),
                        point=_point(anchor),
                        des_loop=loop.to_optional(),
                        electrical_network=network.to_optional(),
                        folder=str(folder),
                    )
                case DistrictTarget(tag="des_param", des_param=loop):
                    return loop.to_des_param_dict(self.graph.buildings)
                case DistrictTarget(tag="opendss", opendss=(network, anchor)):
                    return network.to_geojson_dict(self.graph.buildings, _location(anchor), point=_point(anchor)), network.to_electrical_database_dict()
                case DistrictTarget(tag="reopt", reopt=(parameter, base_file, urdb_label)):
                    return parameter.to_assumptions_dict(base_file, urdb_label)
                case DistrictTarget(tag="geojson", geojson=(folder, anchor)):
                    return self.graph.to_geojson(_location(anchor), point=_point(anchor), folder=str(folder))
                case _ as unreachable:
                    assert_never(unreachable)

        return evidence_run(EvidenceScope.ENERGY_DISTRICT, f"translate.{target.tag}", fold)

    @classmethod
    def _gated(cls, graph: "DistrictGraph", lane: LanePolicy) -> Self:
        rows = graph.check_all(raise_exception=False, detailed=True)
        if rows:
            census = Block.of_seq(rows).fold(lambda acc, row: acc.change(str(row.get("code", "?")), lambda n: Some(n.default_value(0) + 1)), Map.empty())
            raise ValueError(f"check_all:district:{len(rows)}:{dict(census.to_seq())}")  # converted once by the evidence_run fence
        return cls(graph=graph, content_key=ContentIdentity.key("district", _ENCODER.encode(graph.to_dict())), lane=lane)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _location(anchor: Anchor) -> object:
    from ladybug.location import Location  # noqa: PLC0415 — AGPL boundary import

    return Location(latitude=anchor.latitude, longitude=anchor.longitude, elevation=anchor.elevation)


def _point(anchor: Anchor) -> object:
    from ladybug_geometry.geometry2d.pointvector import Point2D  # noqa: PLC0415 — AGPL boundary import

    return Point2D(*anchor.origin)


def _massed(specs: Block[BuildingSpec], policy: MassingPolicy) -> "DistrictGraph":
    from dragonfly.building import Building  # noqa: PLC0415 — AGPL boundary import
    from dragonfly.model import Model  # noqa: PLC0415
    from ladybug_geometry.geometry3d.face import Face3D  # noqa: PLC0415
    from ladybug_geometry.geometry3d.pointvector import Point3D  # noqa: PLC0415

    def built(spec: BuildingSpec) -> Building:
        footprints = [Face3D([Point3D(*xyz) for xyz in ring]) for ring in spec.footprint]
        return Building.from_footprint(
            spec.identifier, footprints, list(spec.floor_to_floor), perimeter_offset=spec.perimeter_offset, tolerance=policy.tolerance
        )

    return Model(
        policy.identifier,
        buildings=[built(spec) for spec in specs],
        units=policy.units,
        tolerance=policy.tolerance,
        angle_tolerance=policy.angle_tolerance,
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
