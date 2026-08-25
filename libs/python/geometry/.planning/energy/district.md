# [PY_GEOMETRY_ENERGY_DISTRICT]

`District` owns the urban-district 2.5-D massing layer above the building model, where a city block or campus stays compact until it explodes into detailed energy models. `District.of` admits the dragonfly graph from a dfjson document, an anchored GeoJSON footprint import, or a computed massing specification; `zone` pairs the plates, `explode` crosses to detailed models, `translate` builds the district-energy egress inputs as typed `DistrictProduct` arms, and `assign` composes the `energy/model` assignment fold over `Room2D` hosts. `dragonfly-core` owns every footprint extrusion, adjacency solver, equirectangular projection, and multiplier-arithmetic kernel; this page composes them into typed, railed, receipted evidence graduating under `GeometrySubject.BUILDING_ENERGY`.

Dragonfly's AGPL-3.0 band rides the standing companion-lane charter — function-local boundary imports, evidence across the wire, no link into a distributed binary. The import site is a LICENSE fact, not a cost one: an audit reads the lexical import graph, so a module-scope binding marks every importer of this module AGPL-coupled, and a `lazy` statement is module-scope by design and cannot serve the ban — the function-local form confines the coupling to the seam function that calls into the band. `explode` is the seam onto `energy/model`: emitted honeybee dicts cross straight into `BuildingModel.of`, so district constructs and the model page admits, ONE gate per tier. Energy assignment and standards resolution are the model page's `assigned`/`resolved` folds imported downward, so a new `EnergySpec` slot lands once for both tiers. URBANopt CLI, Modelica, RNM, and the REopt API behind the translate inputs are external process-boundary services this page never drives — it builds the TYPED INPUTS only; run orchestration enters through a future admission motion carrying its own engine provisioning. Every admitted district keys once over its canonical dfjson bytes.

## [01]-[INDEX]

- [02]-[DISTRICT]: one polymorphic district owner — admission under one `check_all` gate, ordered zoning, the explosion seam, and the `DistrictTarget`/`DistrictProduct` translation pair — under one `DistrictReceipt`.

## [02]-[DISTRICT]

- Owner: `District` holds the validated dragonfly `Model`, its `ContentKey`, the `LanePolicy` the explosion seam threads into each emitted model's admission, and the `composition` custody key every weave stamps. `Anchor` carries the geo registration constructed once and projected into the ladybug `Location` at each kernel, never four loose floats per call; `ExplodePolicy` is the ONE explosion policy both paths read — `explode` threads every knob into `to_honeybee` and the `urbanopt` translation arm threads the three `model_to_urbanopt` accepts — so one district can no longer produce two building sets under divergent geometry policy; `UrbanoptSpec` and its `DistrictTarget` siblings carry behavior as data, never a positional tuple beside a page of `Struct` fields.
- Entry: every admission arm converges on the ONE `check_all(raise_exception=False, detailed=True)` gate whose defect rows fold to the band's `EnergyFault.district_defects` case — the row count beside the ordered per-code roster, kwargs the converting fence lifts whole. `zone` runs the only legal order — `intersect_adjacency` splits coincident walls THEN `solve_adjacency` pairs them, never a solve over un-intersected plates — and reads the graph's own tolerance, so no caller re-supplies a value the admitted model already carries. `explode` is `async`, awaiting `BuildingModel.of(..., composition=self.composition)` per emitted honeybee dict over the held lane; failed building admissions accumulate through `traversed(ACCUMULATE)` so every bad building names itself in the combined fault, never a first-fault abort hiding siblings. `translate` returns a `DistrictProduct` arm per target, so the two-value OpenDSS emit is a declared case rather than a tuple hiding inside an erased return.
- Auto: every fold returns `(value, receipt)` built where the census lives, so the weave harvests without a caller hand-asserting a segment count; per-building `ModelReceipt`s stay with the model page's own weave rather than being re-carried here; `use_multiplier=True` keeps the compact graph and `False` instances every floor — a policy value on the shared explode row; equirectangular meters-to-lon-lat correspondence stays `dragonfly.projection`'s, never re-derived; `des_param`/`opendss`/`reopt` targets consume CALLER-authored dragonfly-energy value objects — this page routes them through the translation seam and re-mints none of their vocabulary.
- Receipt: the graduation residual DERIVES from the receipt's own segment census — `unzoned_segments`/`total_segments`, a fully-zoned district graduating at zero — never a caller-supplied fraction; `modeled_floors` sums story multipliers, the real modeled-floor census. `spec` is the evidence subject — the graph key beside the target that read it — so `graduates` derives its own `ContentKey` and takes none.
- Packages: `dragonfly-core` (the module is `dragonfly`, never `dragonfly_core`) and `dragonfly-energy` per the fence imports, every one a function-local seam under the band's license-isolation ban and the bare `dragonfly_energy` line under a second ban of its own — it is imported FOR the `_extend_dragonfly` registration and dereferences nothing, so no deferred form could ever fire it; `FourthGenThermalLoop` is the 4th-gen hot/chilled loop, `GHEThermalLoop` the 5th-gen ground-heat-exchanger ambient loop with borefield sizing, `RoadNetwork` and `GroundMountPV` the road-graph and ground-PV layers `model_to_urbanopt` layers onto the feature GeoJSON.
- Growth: a new translation egress is one `DistrictTarget` case, one `DistrictProduct` arm, and one dispatch arm; window/skylight/shading parameter families attach as `BuildingSpec` rows when a consumer names them; the URBANopt/DES/RNM/REopt run drivers enter only through a future admission motion provisioning their engines; GeoJSON parcel-layer ingest at scale composes the data folder's geospatial owners at the data seam, never a geometry-side `geopandas` import.
- Boundary: rooms/faces/apertures are `energy/model`'s — this page stops at `Room2D` plates and the explosion seam, and the urbanopt arm returns its emitted artifact addresses rather than the live honeybee models the writer also hands back, since a model reaches a consumer only through the model page's own gate; simulation and result decode are `energy/simulate`'s, weather is `energy/climate`'s; accurate CRS work beyond dragonfly's own equirectangular helpers is the data folder's `pyproj` plane.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Mapping
from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never

from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec import json as msgjson

from rasm.geometry.energy.climate import ENERGY_REGIMES, EnergyFault, EnergyRegime, RegimeKey
from rasm.geometry.energy.model import BuildingModel, EnergySpec, ModelSource, assigned
from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometrySubject, evidence_key, evidence_run
from rasm.runtime.faults import Disposition, RuntimeRail, traversed
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey

if TYPE_CHECKING:
    from dragonfly.model import Model as DistrictGraph
    from dragonfly_energy.des.loop import FourthGenThermalLoop, GHEThermalLoop
    from dragonfly_energy.opendss.network import ElectricalNetwork, RoadNetwork
    from dragonfly_energy.reopt import GroundMountPV, REoptParameter

# --- [TYPES] ----------------------------------------------------------------------------


class PerModel(StrEnum):
    DISTRICT = "District"
    BUILDING = "Building"
    STORY = "Story"


type Ring = tuple[tuple[float, float, float], ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

_ENCODER: Final = msgjson.Encoder(order="deterministic")

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


class ExplodePolicy(Struct, frozen=True, gc=False):
    per_model: PerModel = PerModel.BUILDING
    use_multiplier: bool = True
    add_plenum: bool = False
    solve_ceiling_adjacencies: bool = False
    enforce_adjacency: bool = True
    enforce_solid: bool = True
    cap: bool = False
    shade_distance: Option[float] = Nothing


class UrbanoptSpec(Struct, frozen=True):
    folder: Path
    anchor: Anchor
    explode: ExplodePolicy = ExplodePolicy()
    loop: "Option[FourthGenThermalLoop | GHEThermalLoop]" = Nothing
    network: "Option[ElectricalNetwork]" = Nothing
    road: "Option[RoadNetwork]" = Nothing
    ground_pv: "Option[GroundMountPV]" = Nothing


class UrbanoptProduct(Struct, frozen=True):
    feature: Path
    buildings: tuple[Path, ...]


@tagged_union(frozen=True)
class DistrictSource:
    tag: Literal["dfjson", "geojson", "massing"] = tag()
    dfjson: "bytes | str | Path | Mapping[str, object]" = case()
    geojson: tuple[Path, Anchor] = case()
    massing: tuple[Block[BuildingSpec], MassingPolicy] = case()


@tagged_union(frozen=True)
class DistrictTarget:
    tag: Literal["urbanopt", "des_param", "opendss", "reopt", "geojson"] = tag()
    urbanopt: UrbanoptSpec = case()
    des_param: "FourthGenThermalLoop | GHEThermalLoop" = case()
    opendss: tuple["ElectricalNetwork", Anchor] = case()
    reopt: tuple["REoptParameter", str, str] = case()
    geojson: tuple[Path, Anchor] = case()


@tagged_union(frozen=True)
class DistrictProduct:
    tag: Literal["urbanopt", "des_param", "opendss", "reopt", "geojson"] = tag()
    urbanopt: UrbanoptProduct = case()
    des_param: Mapping[str, object] = case()
    opendss: tuple[Mapping[str, object], Mapping[str, object]] = case()
    reopt: Mapping[str, object] = case()
    geojson: Path = case()


class DistrictReceipt(Struct, frozen=True):
    buildings: int
    stories: int
    room2ds: int
    modeled_floors: int
    footprint_area: float
    floor_area: float
    exploded: int
    touched: int
    target: Option[str]
    content_key: ContentKey
    unzoned_segments: int = 0
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
                    "touched": self.touched,
                    "unzoned_segments": self.unzoned_segments,
                    "total_segments": self.total_segments,
                    "content_key": self.content_key.hex,
                },
            ),
        )

    def spec(self) -> bytes:
        return b"|".join((self.content_key.memory, self.target.default_value("admit").encode()))

    def graduates(self, regime: EnergyRegime = ENERGY_REGIMES[RegimeKey.DISTRICT_DEFECTS]) -> GeometryHandoff:
        residual = self.unzoned_segments / self.total_segments if self.total_segments else 1.0
        subject = GeometrySubject.BUILDING_ENERGY
        return GeometryHandoff.of(
            subject,
            evidence_key(subject, self.spec()),
            {"unzoned": residual, "buildings": float(self.buildings), "floor_area": self.floor_area},
            {"unzoned": regime.bar()},
        )


# --- [SERVICES] -------------------------------------------------------------------------


class District(Struct, frozen=True):
    graph: "DistrictGraph"
    content_key: ContentKey
    lane: LanePolicy
    composition: ScopeKey = DEFAULT_SCOPE

    @classmethod
    def of(
        cls, source: DistrictSource, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[tuple[Self, DistrictReceipt]]":
        def admit() -> tuple[Self, DistrictReceipt]:
            from dragonfly.model import Model

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
            return cls._gated(graph, lane, composition)

        return evidence_run(EvidenceScope.ENERGY_DISTRICT, f"admit.{source.tag}", admit, composition=composition)

    def zone(self) -> "RuntimeRail[tuple[Self, DistrictReceipt]]":
        def fold() -> tuple[Self, DistrictReceipt]:
            from dragonfly.room2d import Room2D

            for story in self.graph.stories:
                Room2D.intersect_adjacency(story.room_2ds, self.graph.tolerance)
                Room2D.solve_adjacency(story.room_2ds, self.graph.tolerance)
            return type(self)._gated(self.graph, self.lane, self.composition)

        return evidence_run(EvidenceScope.ENERGY_DISTRICT, "zone", fold, composition=self.composition)

    async def explode(self, policy: ExplodePolicy) -> "RuntimeRail[tuple[Block[BuildingModel], DistrictReceipt]]":
        async def fold() -> "RuntimeRail[tuple[Block[BuildingModel], DistrictReceipt]]":
            emitted = self.graph.to_honeybee(**_honeybee_knobs(policy, self.graph.tolerance))
            rails = Block.of_seq(
                [await BuildingModel.of(ModelSource(hbjson=model.to_dict()), self.lane, composition=self.composition) for model in emitted]
            )
            return traversed(rails, by=Disposition.ACCUMULATE).map(
                lambda pairs: (pairs.map(lambda pair: pair[0]), self._receipt(exploded=len(pairs)))
            )

        return await evidence_run(EvidenceScope.ENERGY_DISTRICT, "explode", fold, composition=self.composition)

    def assign(self, spec: EnergySpec) -> "RuntimeRail[tuple[Self, DistrictReceipt]]":
        def fold() -> tuple[Self, DistrictReceipt]:
            import dragonfly_energy

            return type(self)._gated(self.graph, self.lane, self.composition, touched=assigned(self.graph.room_2ds, spec))

        return evidence_run(EvidenceScope.ENERGY_DISTRICT, "assign", fold, composition=self.composition)

    def translate(self, target: DistrictTarget) -> "RuntimeRail[tuple[DistrictProduct, DistrictReceipt]]":
        def fold() -> tuple[DistrictProduct, DistrictReceipt]:
            return _translated(self, target), self._receipt(target=Some(target.tag))

        return evidence_run(EvidenceScope.ENERGY_DISTRICT, f"translate.{target.tag}", fold, composition=self.composition)

    def _receipt(self, exploded: int = 0, touched: int = 0, target: Option[str] = Nothing) -> DistrictReceipt:
        conditions = [type(bc).__name__ for room in self.graph.room_2ds for bc in room.boundary_conditions]
        return DistrictReceipt(
            buildings=len(self.graph.buildings),
            stories=len(self.graph.stories),
            room2ds=len(self.graph.room_2ds),
            modeled_floors=sum(story.multiplier for story in self.graph.stories),
            footprint_area=self.graph.footprint_area,
            floor_area=self.graph.floor_area,
            exploded=exploded,
            touched=touched,
            target=target,
            content_key=self.content_key,
            unzoned_segments=sum(1 for name in conditions if name != "Surface"),
            total_segments=len(conditions),
        )

    @classmethod
    def _gated(
        cls, graph: "DistrictGraph", lane: LanePolicy, composition: ScopeKey, touched: int = 0
    ) -> tuple[Self, DistrictReceipt]:
        rows = graph.check_all(raise_exception=False, detailed=True)
        if rows:
            census = Block.of_seq(rows).fold(lambda acc, row: acc.change(str(row.get("code", "?")), lambda n: Some(n.default_value(0) + 1)), Map.empty())
            raise EnergyFault(district_defects=(len(rows), tuple(census.to_seq())))
        admitted = cls(graph=graph, content_key=ContentIdentity.key("district", _ENCODER.encode(graph.to_dict())), lane=lane, composition=composition)
        return admitted, admitted._receipt(touched=touched)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _location(anchor: Anchor) -> object:
    from ladybug.location import Location

    return Location(latitude=anchor.latitude, longitude=anchor.longitude, elevation=anchor.elevation)


def _point(anchor: Anchor) -> object:
    from ladybug_geometry.geometry2d.pointvector import Point2D

    return Point2D(*anchor.origin)


def _honeybee_knobs(policy: ExplodePolicy, tolerance: float) -> dict[str, object]:
    return {
        "object_per_model": policy.per_model.value,
        "shade_distance": policy.shade_distance.to_optional(),
        "use_multiplier": policy.use_multiplier,
        "add_plenum": policy.add_plenum,
        "cap": policy.cap,
        "solve_ceiling_adjacencies": policy.solve_ceiling_adjacencies,
        "tolerance": tolerance,
        "enforce_adj": policy.enforce_adjacency,
        "enforce_solid": policy.enforce_solid,
    }


def _translated(district: District, target: DistrictTarget) -> DistrictProduct:
    match target:
        case DistrictTarget(tag="urbanopt", urbanopt=spec):
            from dragonfly_energy.writer import model_to_urbanopt

            feature, buildings, _graph = model_to_urbanopt(
                district.graph,
                _location(spec.anchor),
                point=_point(spec.anchor),
                shade_distance=spec.explode.shade_distance.to_optional(),
                use_multiplier=spec.explode.use_multiplier,
                add_plenum=spec.explode.add_plenum,
                solve_ceiling_adjacencies=spec.explode.solve_ceiling_adjacencies,
                des_loop=spec.loop.to_optional(),
                electrical_network=spec.network.to_optional(),
                road_network=spec.road.to_optional(),
                ground_pv=spec.ground_pv.to_optional(),
                folder=str(spec.folder),
                tolerance=district.graph.tolerance,
            )
            return DistrictProduct(urbanopt=UrbanoptProduct(feature=Path(feature), buildings=tuple(Path(one) for one in buildings)))
        case DistrictTarget(tag="des_param", des_param=loop):
            return DistrictProduct(des_param=loop.to_des_param_dict(district.graph.buildings, tolerance=district.graph.tolerance))
        case DistrictTarget(tag="opendss", opendss=(network, anchor)):
            return DistrictProduct(
                opendss=(
                    network.to_geojson_dict(district.graph.buildings, _location(anchor), point=_point(anchor), tolerance=district.graph.tolerance),
                    network.to_electrical_database_dict(),
                )
            )
        case DistrictTarget(tag="reopt", reopt=(parameter, base_file, urdb_label)):
            return DistrictProduct(reopt=parameter.to_assumptions_dict(base_file, urdb_label))
        case DistrictTarget(tag="geojson", geojson=(folder, anchor)):
            return DistrictProduct(
                geojson=Path(
                    district.graph.to_geojson(_location(anchor), point=_point(anchor), folder=str(folder), tolerance=district.graph.tolerance)
                )
            )
        case _ as unreachable:
            assert_never(unreachable)


def _massed(specs: Block[BuildingSpec], policy: MassingPolicy) -> "DistrictGraph":
    from dragonfly.building import Building
    from dragonfly.model import Model
    from ladybug_geometry.geometry3d.face import Face3D
    from ladybug_geometry.geometry3d.pointvector import Point3D

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
