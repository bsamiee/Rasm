# [PY_GEOMETRY_ENERGY_DISTRICT]

The urban-district massing owner — the 2.5-D layer above the building model where a city block or campus stays compact until it explodes into detailed energy models. `District.of` is the one polymorphic admission over the dragonfly object graph (`Model` -> `Building` -> `Story` -> `Room2D` -> `ContextShade`): a dfjson document ARRIVES over the wire and decodes by payload shape (`Model.from_dict`/`from_dfjson`/`from_file`), an urban footprint GeoJSON imports through `Model.from_geojson` anchored on a geo `Anchor`, and a massing specification COMPUTES the graph — `Building.from_footprint(identifier, footprint, floor_to_floor_heights)` extrudes coordinate-tuple footprints into multiplier-carrying stories — all fronted by the same `check_all` validation gate, never a second entry, and every admitted district keys once through the `rasm.runtime.identity` owner over its canonical dfjson bytes. `zone` runs the auto-zoning pair in its only legal order — `Room2D.intersect_adjacency` splits coincident walls THEN `Room2D.solve_adjacency` pairs them — never a solve over un-intersected plates. `explode` is the seam onto `energy/model`: `Model.to_honeybee(object_per_model, use_multiplier, add_plenum, ...)` emits honeybee models whose dicts cross straight into `BuildingModel.of` — the district page constructs, the model page admits, ONE gate per tier, and the multiplier stays honest (a compact story standing for N floors explodes only when the policy demands full instancing).

`translate` is the district-energy egress family over `dragonfly-energy` — importing it registers `.properties.energy` on every dragonfly object, and the translation targets are ROWS of one closed union: the URBANopt feature GeoJSON + per-building bundle (`model_to_urbanopt` layering optional DES loop / electrical network / road network / ground-PV), the district-energy-system parameter dict (`FourthGenThermalLoop`/`GHEThermalLoop.to_des_param_dict` — the 4th-gen hot/chilled loop and the 5th-gen ground-heat-exchanger ambient loop with borefield sizing), the OpenDSS electrical payloads (`ElectricalNetwork.to_geojson_dict`/`to_electrical_database_dict`), the REopt techno-economic body (`REoptParameter.to_assumptions_dict`), and the plain geo export (`to_geojson`). The engines behind those inputs (URBANopt CLI, Modelica, RNM, the REopt API) are external process-boundary services this page never drives — it builds the TYPED INPUTS; run orchestration enters only through a future admission motion carrying its own engine provisioning. Standards assignment reuses the `energy/model` `RESOLVERS` rows downward — one standards access path folder-wide. The dragonfly band is AGPL-3.0 riding the standing companion-lane charter: function-local boundary imports, dfjson/GeoJSON evidence across the wire, nothing linked into a distributed binary. This owner re-implements no footprint extrusion, no adjacency solver, no equirectangular projection, and no multiplier arithmetic — `dragonfly-core` owns the massing kernels; it composes them into typed, railed, receipted district evidence graduating under the `"building-energy"` subject.

## [01]-[INDEX]

- [01]-[DISTRICT]: the one polymorphic `District` owner — the three-modality `DistrictSource` admission under one `check_all` gate, the ordered auto-zoning pair, the `to_honeybee` explosion seam onto `energy/model`, the `DistrictTarget` translation union over dragonfly-energy, the `DistrictReceipt` evidence, and the `building-energy` graduation.

## [02]-[DISTRICT]

- Owner: `District` — the admitted capsule holding the validated dragonfly `Model` and its `ContentKey`; `DistrictSource` the closed three-case admission union — `dfjson` (bytes | path | mapping), `geojson` (path + `Anchor`), `massing` (`Block[BuildingSpec]` + `MassingPolicy`); `Anchor` the geo-registration value (latitude, longitude, elevation, the scene-origin point) the GeoJSON exchange and every geo-referenced translation share — constructed once, projected into the ladybug `Location` at each kernel, never four loose floats per call; `BuildingSpec` one massing row — identifier, the footprint as coordinate-tuple rings, per-story floor-to-floor heights, `Option` perimeter-offset for core/perimeter zoning; `MassingPolicy` the construction policy (units, tolerance, angle tolerance); `ExplodePolicy` the explosion policy — `object_per_model` (`District`/`Building`/`Story` as a closed vocabulary), `use_multiplier`, `add_plenum`, `solve_ceiling_adjacencies`, and the enforce flags — behavior as data over `to_honeybee`'s own knobs; `DistrictTarget` the closed translation union — `urbanopt` (folder + optional DES/electrical/road/ground-PV layers), `des_param` (a `FourthGenThermalLoop | GHEThermalLoop` the caller authored from the dragonfly-energy value objects), `opendss` (an `ElectricalNetwork`), `reopt` (a `REoptParameter` + base file + URDB label), `geojson` (folder) — one row per district-energy egress, never a per-target method family; `DistrictReceipt` the `ReceiptContributor` evidence.
- Cases: `DistrictSource.dfjson` discriminates by payload shape exactly as the model page's hbjson arm — bytes decode `msgspec.json.decode` then `Model.from_dict`, a mapping reconstructs directly, a path routes `Model.from_file` (dfjson/dfpkl by extension); `DistrictSource.geojson` carries `(Path, Anchor)` — `Model.from_geojson(path, location, point)` imports footprints as extruded buildings anchored at the equirectangular origin; `DistrictSource.massing` folds each `BuildingSpec` through `Building.from_footprint(identifier, footprints, floor_to_floor_heights)` over `Face3D`-lifted rings, assembling `Model(identifier, buildings, units, tolerance, angle_tolerance)`.
- Entry: `District.of` runs the admission fold inside the `evidence_run` weave and converges every arm on the ONE gate — dragonfly `check_all(raise_exception=False, detailed=True)` rows fold to a typed fault census on any defect (missing adjacencies, room-plate overlaps, roof/room conflicts, degenerate plates, invalid window parameters), an empty set admits with `ContentIdentity.key("district", dfjson_bytes)`. `zone(tolerance)` runs the ordered pair over every story's `room_2ds` — `intersect_adjacency` then `solve_adjacency` — and returns the re-keyed successor (zoning changed the document). `explode(policy)` runs `to_honeybee(object_per_model=..., use_multiplier=..., add_plenum=..., solve_ceiling_adjacencies=..., enforce_adj=..., enforce_solid=...)` and hands each emitted honeybee model's dict to `BuildingModel.of(ModelSource(hbjson=...))` — the seam onto the model owner: the district constructs, the model page's gate admits, and the result is a `Block[BuildingModel]` of content-keyed building capsules ready for `energy/simulate`; a failed building admission accumulates through the rail's traverse so one bad building names itself rather than aborting the district silently. `assign(spec)` mirrors the model page's energy assignment over `Room2D` hosts — `dragonfly_energy`'s import side effect registers `.properties.energy`, identifiers resolve through the SAME `RESOLVERS` rows imported downward from `energy/model`, and the fold covers `all_room_2ds` (or the spec's named subset). `translate(target)` dispatches the closed union: `urbanopt` calls `model_to_urbanopt(model, location, point, des_loop, electrical_network, road_network, ground_pv, folder)` and returns the feature-GeoJSON path; `des_param` calls `loop.to_des_param_dict(model.buildings, tolerance)`; `opendss` calls `network.to_geojson_dict(model.buildings, location, point, tolerance)` paired with `to_electrical_database_dict()`; `reopt` calls `parameter.to_assumptions_dict(base_file, urdb_label)`; `geojson` calls `model.to_geojson(location, point, folder)` — each arm one row-resolved provider call returning typed payloads, never a run driver.
- Auto: every entry rides `evidence_run(EvidenceScope.ENERGY_DISTRICT, <operation>, dispatch)`; the multiplier discipline is dragonfly's — `use_multiplier=True` keeps the compact graph (fast simulation) and `False` instances every floor, a policy value the explode row carries; the geo anchor projects into `Location(latitude=..., longitude=..., elevation=...)` inside each kernel — the equirectangular meters-to-lon-lat correspondence stays `dragonfly.projection`'s, never re-derived; a `des_param`/`opendss`/`reopt` target consumes CALLER-authored dragonfly-energy value objects (`GHEThermalLoop` with its `SoilParameter`/`FluidParameter`/`PipeParameter`/`BoreholeParameter`/`GHEDesignParameter` rows, `ElectricalNetwork` with its transformer/wire catalogs, `REoptParameter` with its financial/PV/wind/storage rows) — this page routes them through the translation seam and re-mints none of their vocabulary; `ModelEnergyProperties.check_all` rides the one gate after `assign` exactly as on the model page (dragonfly folds its registered extension checks automatically).
- Receipt: `DistrictReceipt` carries buildings/stories/room2d counts, the story-multiplier sum (the real modeled-floor census), footprint and floor areas, the exploded-model count, the translation target discriminant, and the `ContentKey`; `contribute` yields one emitted-phase `Receipt.of("rasm.geometry.energy.district", self)` row; `graduates(key, ceiling)` folds under `GeometrySubject.BUILDING_ENERGY` with the unzoned-boundary fraction as the residual (adjacency-unsolved wall segments over total wall segments — a fully-zoned district graduates at zero), never a count ledger.
- Packages: `dragonfly-core` (`model.Model` — `from_dict`/`from_dfjson`/`from_file`/`from_geojson`/`to_geojson`/`to_dict`/`check_all`/`to_honeybee`; `building.Building.from_footprint`/`unique_stories`; `story.Story.multiplier`/`solve_room_2d_adjacency`; `room2d.Room2D.intersect_adjacency`/`solve_adjacency`/`from_polygon`; `context.ContextShade`; the discriminated window/skylight/shading parameter families as upstream vocabulary — all function-local, AGPL-3.0 companion posture; the module is `dragonfly`, never `dragonfly_core`), `dragonfly-energy` (the `_extend_dragonfly` import side effect, `writer.model_to_urbanopt`, `des.loop.FourthGenThermalLoop`/`GHEThermalLoop.to_des_param_dict`, `opendss.network.ElectricalNetwork.to_geojson_dict`/`to_electrical_database_dict`, `reopt.REoptParameter.to_assumptions_dict` — same posture; the URBANopt/Modelica/RNM/REopt ENGINES are external process-boundary services this page never shells), `ladybug-geometry` (`Face3D`/`Polygon2D` footprint lifting), `ladybug-core` (`location.Location` the anchor projection), `msgspec` (`json` codec for canonical dfjson bytes), `expression` (`Block`/`Map`/`Option`/`tagged_union`), geometry (`evidence_run`/`EvidenceScope`/`GeometryHandoff`/`GeometrySubject`, `energy/model` `BuildingModel`/`ModelSource`/`RESOLVERS`/`StandardsKind` imported downward), runtime (`ContentIdentity`/`ContentKey`, `RuntimeRail`/`BoundaryFault`, `Receipt`).
- Growth: a new translation egress is one `DistrictTarget` case + one dispatch arm; a new massing construction row is one `BuildingSpec` field (window/skylight/shading parameter families attach as spec rows when a consumer names them); the URBANopt/DES/RNM/REopt RUN drivers enter only through a future admission motion provisioning their engines (recorded design pressure, never a pre-admitted subprocess arm); urban-microclimate EPW morphing stays admission-gated growth on the climate owner; the GeoJSON parcel-layer ingest at scale composes the data folder's geospatial owners at the data seam, never a geometry-side `geopandas` import.
- Boundary: no detailed building-energy model (rooms/faces/apertures are `energy/model`'s — this page stops at `Room2D` plates and the explosion seam), no simulation or result decode (`energy/simulate`), no weather (`energy/climate`), no engine subprocess (URBANopt/Modelica/RNM/REopt are external services), and no GIS re-projection beyond dragonfly's own equirectangular helpers (accurate CRS work is the data folder's `pyproj` plane at the data seam). The deleted forms: a solve over un-intersected plates (the ordered `intersect_adjacency` -> `solve_adjacency` pair is the only zoning path); an explode that bypasses the model page's gate (every emitted honeybee model crosses `BuildingModel.of`); a per-target `to_urbanopt`/`to_des`/`to_opendss` method family where one `translate` dispatches the union; four loose lat/lon/elevation/point parameters where `Anchor` carries the geo registration; a re-minted thermal-loop/electrical/REopt vocabulary where the caller authors dragonfly-energy value objects; importing the module as `dragonfly_core`; and a run driver shelling an unprovisioned engine.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Mapping
from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Literal, Self

from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec import json as msgjson

from rasm.geometry.energy.model import RESOLVERS, BuildingModel, EnergySpec, ModelSource, StandardsKind
from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometrySubject, evidence_run
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
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

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of("rasm.geometry.energy.district", self)

    def graduates(self, key: ContentKey, unsolved_fraction: float, ceiling: float) -> GeometryHandoff:
        return GeometryHandoff.of(
            GeometrySubject.BUILDING_ENERGY,
            key,
            {"unzoned": unsolved_fraction, "buildings": float(self.buildings), "floor_area": self.floor_area},
            {"unzoned": ceiling},
        )


# --- [SERVICES] -------------------------------------------------------------------------


class District(Struct, frozen=True):
    graph: "DistrictGraph"
    content_key: ContentKey

    @classmethod
    def of(cls, source: DistrictSource) -> "RuntimeRail[Self]":
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
            return cls._gated(graph)

        return evidence_run(EvidenceScope.ENERGY_DISTRICT, f"admit.{source.tag}", admit)

    def zone(self, tolerance: float = 0.01) -> "RuntimeRail[Self]":
        def fold() -> Self:
            from dragonfly.room2d import Room2D  # noqa: PLC0415 — AGPL boundary import

            for story in self.graph.stories:  # Exemption: dragonfly zones stories in place; the ordered pair is its owned surface.
                Room2D.intersect_adjacency(story.room_2ds, tolerance)
                Room2D.solve_adjacency(story.room_2ds, tolerance)
            return type(self)._gated(self.graph)

        return evidence_run(EvidenceScope.ENERGY_DISTRICT, "zone", fold)

    def explode(self, policy: ExplodePolicy) -> "RuntimeRail[Block[BuildingModel]]":
        # the seam onto energy/model: dragonfly constructs, the model owner's ONE gate admits.
        def fold() -> Block["RuntimeRail[BuildingModel]"]:
            emitted = self.graph.to_honeybee(
                object_per_model=policy.per_model.value,
                use_multiplier=policy.use_multiplier,
                add_plenum=policy.add_plenum,
                solve_ceiling_adjacencies=policy.solve_ceiling_adjacencies,
                enforce_adj=policy.enforce_adjacency,
                enforce_solid=policy.enforce_solid,
            )
            return Block.of_seq([BuildingModel.of(ModelSource(hbjson=model.to_dict())) for model in emitted])

        return evidence_run(EvidenceScope.ENERGY_DISTRICT, "explode", fold).bind(_sequenced)

    def assign(self, spec: EnergySpec) -> "RuntimeRail[Self]":
        # the compact-graph energy assignment: one fold over Room2D hosts BEFORE the explode so a
        # multiplier story assigns once; identifiers resolve through the model page's RESOLVERS rows.
        def fold() -> Self:
            from importlib import import_module  # noqa: PLC0415 — row-resolved AGPL boundary import

            import dragonfly_energy  # noqa: F401, PLC0415 — the _extend_dragonfly side effect registers .properties.energy

            def resolved(kind: StandardsKind, identifier: str) -> object:
                module, loader = RESOLVERS[kind]
                return getattr(import_module(module), loader)(identifier)

            program = spec.program.map(lambda ident: resolved(StandardsKind.PROGRAM, ident)).to_optional()
            constructions = spec.construction_set.map(lambda ident: resolved(StandardsKind.CONSTRUCTION_SET, ident)).to_optional()
            wanted = spec.rooms.map(frozenset).default_value(frozenset())
            for room in self.graph.room_2ds:  # Exemption: dragonfly mutates hosts in place; assignment is its owned imperative surface.
                if wanted and room.identifier not in wanted:
                    continue
                energy = room.properties.energy
                energy.program_type = program if program is not None else energy.program_type
                energy.construction_set = constructions if constructions is not None else energy.construction_set
            return type(self)._gated(self.graph)

        return evidence_run(EvidenceScope.ENERGY_DISTRICT, "assign", fold)

    def receipt(self, exploded: int = 0, target: Option[str] = Nothing) -> DistrictReceipt:
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

        return evidence_run(EvidenceScope.ENERGY_DISTRICT, f"translate.{target.tag}", fold)

    @classmethod
    def _gated(cls, graph: "DistrictGraph") -> Self:
        rows = graph.check_all(raise_exception=False, detailed=True)
        if rows:
            census = Block.of_seq(rows).fold(lambda acc, row: acc.change(str(row.get("code", "?")), lambda n: Some(n.default_value(0) + 1)), Map.empty())
            raise ValueError(f"check_all:district:{len(rows)}:{dict(census.to_seq())}")  # converted once by the evidence_run fence
        return cls(graph=graph, content_key=ContentIdentity.key("district", msgjson.Encoder(order="deterministic").encode(graph.to_dict())))


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


def _sequenced(rails: Block["RuntimeRail[BuildingModel]"]) -> "RuntimeRail[Block[BuildingModel]]":
    # partition the explode admissions so the first bad building names itself with the casualty
    # count on the fault detail, never a silent partial district and never an unattributed abort.
    faults = rails.choose(lambda rail: rail.swap().to_option())
    return Ok(rails.choose(lambda rail: rail.to_option())) if faults.is_empty() else Error(faults.head())
```
