# [PY_GEOMETRY_API_HONEYBEE_CORE]

`honeybee-core` is the HBJSON base building-model owner for the geometry energy-modeling rail: the geometric object graph (`Model` -> `Room` -> `Face` -> `Aperture`/`Door`, plus `Shade`/`ShadeMesh`) built directly on `ladybug_geometry` primitives (`Face3D`/`Polyface3D`/`Mesh3D`/`Point3D`/`Vector3D`/`Plane`), the `.properties` extension spine that every energy/radiance/openstudio extension attaches to, the bounded vocabularies (`facetype`, `boundarycondition`, `altnumber`), the `dict_to_object` polymorphic deserializer, and the `check_all` validation family. The geometry owner builds a `Model` from `ladybug_geometry` solids (`ladybug-geometry` sibling), round-trips it through the HBJSON `to_dict`/`from_dict` contract validated by the `honeybee-schema` pydantic-v2 models (the universal `pydantic` rail) and decoded at the boundary through `msgspec`, folds the `check_all(detailed=True)` error rows into the `expression` `Result` rail, and never re-implements the room/face topology, the adjacency solver, the aperture generators, or the extension-dict splitting that honeybee-core already owns. It is the spine of the Honeybee band — `honeybee-energy`/`honeybee-openstudio` ride it through `.properties.energy` and the `_extend_honeybee` registration, and the urban-massing aggregator `dragonfly-core` sits above it, exploding a compact district `Model`/`Building`/`Story` graph into an array of these honeybee `Model`s via `dragonfly` `Model.to_honeybee` (each dragonfly `Room2D` becomes a honeybee `Room`), never a parallel object model.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `honeybee-core`
- package: `honeybee-core`
- import: `import honeybee` then `from honeybee.model import Model`, `from honeybee.room import Room`, `from honeybee.face import Face`, `from honeybee.aperture import Aperture`, `from honeybee.door import Door`, `from honeybee.shade import Shade`, `from honeybee.shademesh import ShadeMesh`
- owner: `geometry`
- rail: energy-modeling
- consumer: `.planning/energy/model.md` (the one `check_all`-gated admission, aperture mint, adjacency, HBJSON wire)
- version: `1.60.0`
- license: AGPL-3.0 (the Ladybug Tools copyleft; AGPL obligations attach to any networked redistribution)
- abi: pure-Python `py3-none-any` wheel; no compiled payload
- depends-on: `ladybug-core` (units/location/EPW) and `ladybug-geometry-polyskel` (which carries the `ladybug-geometry` primitive tier honeybee objects wrap); `honeybee-schema` (pydantic v2) is the direct HBJSON wire-contract validation dependency
- entry points: `honeybee` console script (`validate`/`edit`/`create`/`lib` sub-commands over HBJSON)
- capability: HBJSON building-model assembly and serialization; room/face/sub-face/shade topology; adjacency solving and intersection; aperture and shading generation; unit conversion; the `.properties` extension attachment graph; bounded boundary-condition/face-type/auto-number vocabularies; the `dict_to_object` polymorphic deserializer; the `check_all` geometric-validity validation family

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry object model (`honeybee.model`, `.room`, `.face`, `.aperture`, `.door`, `.shade`, `.shademesh`)
- rail: energy-modeling
- Every object wraps a `ladybug_geometry` primitive (`Face`/`Aperture`/`Door`/`Shade` hold a `Face3D`, `Room` a `Polyface3D`, `ShadeMesh` a `Mesh3D`) and carries an `identifier`, a mutable `display_name`, free-form `user_data`, and a `.properties` extension handle. The graph is parented (`Face.parent`/`has_parent`, `Room.faces`, `Model.rooms`), so geometric edits propagate through the owner.

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :------------ | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `Model`       | model root    | top-level container of rooms, orphaned faces/apertures/doors, and shades; units/tolerance carrier |
|  [02]   | `Room`        | volume        | closed `Polyface3D` solid of bounding `Face`s; the zone/HVAC/program assignment unit |
|  [03]   | `Face`        | boundary      | a `Face3D` wall/roof/floor with a `boundary_condition`, a `facetype`, and sub-faces |
|  [04]   | `Aperture`    | sub-face      | operable/fixed window opening on a parent `Face`                              |
|  [05]   | `Door`        | sub-face      | opaque or glass door opening on a parent `Face`                              |
|  [06]   | `Shade`       | shading       | indoor/outdoor/detached single-face shading surface                          |
|  [07]   | `ShadeMesh`   | shading       | a `Mesh3D` shade for many-faced context shading (cheaper than per-face `Shade`) |

[PUBLIC_TYPE_SCOPE]: extension spine (`honeybee.properties`)
- rail: energy-modeling
- One `*Properties` host per object kind. Each is a thin attribute bag whose extension slots (`.energy`, `.radiance`, `.doe2`, ...) are populated by the extension package's `_extend_honeybee` registration; the geometry owner folds the per-extension `to_dict(include=...)`/`apply_properties_from_dict` into ONE properties-extension table keyed by extension key, never a branch per extension.

| [INDEX] | [SYMBOL]              | [CAPABILITY]                                                              |
| :-----: | :-------------------- | :----------------------------------------------------------------------- |
|  [01]   | `ModelProperties`     | model-level extension host; `to_dict(include=None)`, `apply_properties_from_dict(data)` |
|  [02]   | `RoomProperties`      | room-level extension host; `add_prefix`, `reset_to_default`, transform passthrough |
|  [03]   | `FaceProperties`      | face-level extension host                                                |
|  [04]   | `ApertureProperties`  | aperture-level extension host                                            |
|  [05]   | `DoorProperties`      | door-level extension host                                                |
|  [06]   | `ShadeProperties`     | shade-level extension host                                               |
|  [07]   | `ShadeMeshProperties` | shade-mesh-level extension host                                          |

[PUBLIC_TYPE_SCOPE]: bounded vocabularies (`honeybee.facetype`, `.boundarycondition`, `.altnumber`)
- rail: energy-modeling
- Closed singleton-backed vocabularies. The geometry owner reads them through the module singletons (`face_types`, `boundary_conditions`), never by constructing the leaf classes, so a face type or boundary condition is a value-object lookup, not a string branch.

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY]       | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------- | :------------------ | :--------------------------------------------------- |
|  [01]   | `Wall` / `RoofCeiling` / `Floor` / `AirBoundary`         | face type           | the four face types; `face_types` singleton + `by_name` |
|  [02]   | `Outdoors(sun_exposure, wind_exposure, view_factor)`     | boundary condition  | exterior face exposed to weather                     |
|  [03]   | `Ground`                                                 | boundary condition  | face in contact with ground                          |
|  [04]   | `Surface(boundary_condition_objects, sub_face)`          | boundary condition  | interior face adjacent to another face (adjacency pair) |
|  [05]   | `Adiabatic` / `OtherSideTemperature`                     | boundary condition  | no-heat-flow / fixed other-side-temperature; `boundary_conditions` singleton + `by_name` |
|  [06]   | `Autocalculate` / `NoLimit` / `Autosize`                 | alt-number sentinel | singleton sentinel numerics (`autocalculate`/`no_limit`/`autosize`) replacing magic `None`/autosize fields |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: HBJSON serialization round-trip
- rail: energy-modeling
- The dict is the canonical wire shape; `included_prop`/`include` select which extension props serialize, so a geometry-only export drops energy/radiance. `dict_to_object` is the polymorphic deserializer dispatching on the dict `type` key (the owner's single decode entry, never a per-type `if`).

| [INDEX] | [SURFACE]                                                              | [CALL_SHAPE]              | [CAPABILITY]                                              |
| :-----: | :-------------------------------------------------------------------- | :----------------------- | :------------------------------------------------------- |
|  [01]   | `Model.to_dict(included_prop=None, triangulate_sub_faces=False, include_plane=True)` | extension-key list       | HBJSON dict; `included_prop=['energy']` serializes only the energy extension |
|  [02]   | `Model.from_dict(data)`                                              | HBJSON dict              | reconstruct a live `Model` (geometry + re-attached extension props) |
|  [03]   | `Model.to_hbjson(name=None, folder=None, indent=None, included_prop=None, ...)` | name/folder              | write an `.hbjson` file (the durable graduation handle)  |
|  [04]   | `Model.from_hbjson(hbjson_file)`                                     | file path                | read an `.hbjson` file                                   |
|  [05]   | `Model.to_hbpkl` / `Model.from_hbpkl` / `Model.from_file`            | file path                | pickle round-trip; `from_file` auto-detects hbjson/hbpkl |
|  [06]   | `honeybee.dictutil.dict_to_object(honeybee_dict, raise_exception=True)` | any honeybee dict        | polymorphic deserialize dispatching on the dict `type` key |
|  [07]   | `ModelProperties.to_dict(include=None)` / `apply_properties_from_dict(data)` | extension keys / dict    | extension-prop serialize / re-attach after a geometry load |

[ENTRYPOINT_SCOPE]: model assembly, generators, and adjacency (`honeybee.model`, `.room`)
- rail: energy-modeling
- Construction routes through `from_*` classmethods; the `Room` static surface owns grouping, adjacency, and procedural plan generation — the geometry owner calls these instead of re-deriving topology.

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]             | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------- | :----------------------- | :------------------------------------------------- |
|  [01]   | `Model(identifier, rooms=None, ..., units='Meters', tolerance=None, angle_tolerance=1.0)` | objects + units          | assemble a model from rooms/orphaned objects/shades |
|  [02]   | `Model.from_objects(identifier, objects, units, tolerance, ...)` | mixed honeybee objects   | build a model from a flat object list             |
|  [03]   | `Model.from_shoe_box` / `from_rectangle_plan` / `from_l_shaped_plan` | dimensions               | procedural test/parametric model generators       |
|  [04]   | `Room.from_box(identifier, width, depth, height, ...)` / `from_polyface3d` / `from_dict` | box dims / `Polyface3D`  | build a room from a box or a closed solid          |
|  [05]   | `Room.solve_adjacency(rooms, tolerance)` (also `Model.solve_adjacency`) | room list                | match coincident faces into `Surface` adjacency pairs |
|  [06]   | `Room.intersect_adjacency` / `group_by_adjacency` / `group_by_air_boundary_adjacency` / `group_by_floor_height` / `group_by_orientation` | room list                | split coincident faces; group rooms by topology/attribute |
|  [07]   | `Room.stories_by_floor_height` / `Room.story` / `Room.grouped_horizontal_boundary` | rooms                    | story assignment from floor height; merged horizontal boundary |
|  [08]   | `Model.convert_to_units(units='Meters')` / `Model.conversion_factor_to_meters` | target units             | scale geometry into a unit system                  |

[ENTRYPOINT_SCOPE]: aperture and shading generation (`honeybee.face`, `.model`)
- rail: energy-modeling
- Window-to-wall ratio, gridded glazing, louvers, and overhangs are owned operations; the geometry owner composes them rather than hand-cutting `Face3D` openings.

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]             | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------- | :----------------------- | :------------------------------------------------- |
|  [01]   | `Face.apertures_by_ratio(ratio, tolerance)` / `apertures_by_ratio_gridded` / `apertures_by_ratio_rectangle` | WWR ratio                | generate windows to a window-to-wall ratio        |
|  [02]   | `Face.aperture_by_width_height` / `apertures_by_width_height_rectangle` | width/height             | place a fixed-size window                          |
|  [03]   | `Model.wall_apertures_by_ratio` / `Model.skylight_apertures_by_ratio` | WWR/skylight ratio       | model-wide aperture generation                    |
|  [04]   | `Face.louvers_by_count` / `louvers_by_distance_between` / `overhang` | count/spacing            | generate louver/overhang shades on a face          |
|  [05]   | `Room.generate_grid` / `Model.generate_exterior_aperture_grid`  | grid size                | sensor grids for daylight/radiance handoff         |

[ENTRYPOINT_SCOPE]: validation, vocabularies, and search
- rail: energy-modeling
- `check_all(detailed=True)` returns a list of error dicts (the `Result`-foldable shape); `validate` is the static JSON-report entry. Vocabulary singletons and helpers resolve types/conditions from geometry.

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]             | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------- | :----------------------- | :------------------------------------------------- |
|  [01]   | `Model.check_all(raise_exception=True, detailed=False)` | flags                    | run every geometric-validity check (solid, planar, self-intersecting, adjacency, sub-face validity, duplicate ids) plus each registered extension's `check_all` automatically |
|  [02]   | `Model.validate(check_function='check_for_extension', check_args=None, json_output=False)` | check selector           | static validation entry; `json_output=True` returns a structured error report |
|  [03]   | `Model.check_missing_adjacencies` / `check_rooms_solid` / `check_planar` / `check_self_intersecting` / `check_duplicate_*` | per-check                | the individual checks `check_all` folds            |
|  [04]   | `honeybee.facetype.get_type_from_normal(normal_vector, roof_angle=60, floor_angle=130)` | a normal vector          | classify a face type from its normal               |
|  [05]   | `honeybee.boundarycondition.get_bc_from_position(positions, ground_depth=0)` | face positions           | classify ground/outdoors from elevation            |
|  [06]   | `honeybee.units.conversion_factor_to_meters` / `parse_distance_string` | units / `'0.15m'` string | unit conversion + distance-string parsing          |
|  [07]   | `honeybee.orientation.angles_from_num_orient` / `orient_index` / `face_orient_index` | subdivision count        | orientation binning for per-orientation parametrics |
|  [08]   | `honeybee.search.filter_array_by_keywords` / `get_attr_nested`  | keywords / dotted attr   | keyword filtering and nested-attribute extraction (drives `Room.group_by_attribute`) |
|  [09]   | `honeybee.checkdup.check_duplicate_identifiers` / `is_equivalent` | object list              | duplicate-id detection; deep geometric equivalence |

## [04]-[IMPLEMENTATION_LAW]

[ENERGY_MODELING_CORE]:
- import: `import honeybee` at boundary scope only; the manifest import policy bans module-level heavy imports.
- geometry axis: every honeybee object wraps a `ladybug_geometry` primitive — `Face`/`Aperture`/`Door`/`Shade` hold a `Face3D`, `Room` a `Polyface3D`, `ShadeMesh` a `Mesh3D`, with `Plane`/`Point3D`/`Vector3D`/`LineSegment3D` for the supporting algebra. The owner never re-implements vertex/normal/area/centroid algebra, planarity, or solid topology; those are `ladybug-geometry` (sibling) operations that honeybee composes. Geometric edits (`move`/`rotate`/`rotate_xy`/`reflect`/`scale`) and aperture/louver/overhang generation are owned face operations folded into one `GeometryOp`-keyed table, never a branch per transform.
- extension-spine axis: `.properties` is the single extension dispatch surface. `honeybee-energy`/`honeybee-openstudio`/radiance register their `*EnergyProperties`/`*RadianceProperties` onto the host `*Properties` classes via `_extend_honeybee`, so the geometry owner reads `room.properties.energy` etc. through one extension-key table and serializes via `to_dict(include=[...])` selecting which extensions cross the wire — never a parallel per-extension object model. `apply_properties_from_dict(data)` is the re-attach leg run after `Model.from_dict` reconstructs geometry; the owner folds the per-extension load through the registered extension key, not a hand-written merge.
- serialization axis: the HBJSON dict produced by `to_dict`/`to_hbjson` is exactly the shape the `honeybee-schema` pydantic-v2 models validate (`honeybee_schema.model.Model.model_validate(dict)`; the `*Abridged` schema variants are the compact wire forms). The owner validates an inbound dict through honeybee-schema (the universal `pydantic` rail) at the boundary, decodes the dict body through `msgspec` for speed, then materializes the live model via `Model.from_dict` / `dict_to_object` (the one polymorphic decode dispatching on the dict `type` key) — never a hand-rolled HBJSON parser and never a second DTO family for the same concept.
- validation axis: `check_all(detailed=True)` and `validate(json_output=True)` return structured error rows (id, message, error-code, element ids) rather than only raising; the owner folds those rows into the `expression` `Result`/validation receipt so a model with geometry defects becomes a typed failure carrier, not a caught exception. The individual `check_*` methods are the per-rule legs `check_all` aggregates; the owner never re-derives solidity/planarity/adjacency checks the package owns.
- vocabulary axis: `facetype` (`Wall`/`RoofCeiling`/`Floor`/`AirBoundary` via the `face_types` singleton), `boundarycondition` (`Outdoors`/`Ground`/`Surface`/`Adiabatic`/`OtherSideTemperature` via `boundary_conditions`), and `altnumber` (`Autocalculate`/`NoLimit`/`Autosize`) are closed value-object vocabularies resolved through the singletons and `get_type_from_normal`/`get_bc_from_position`, never reconstructed from raw strings.
- subpackages: `model`, `room`, `face`, `aperture`, `door`, `shade`, `shademesh`, `_base`/`_basewithshade`/`_lockable` (the lockable mutation guard), `properties`, `facetype`, `boundarycondition`, `altnumber`, `units`, `orientation`, `search`, `dictutil`, `extensionutil` (the extension-dict splitters `model_extension_dicts`/`room_extension_dicts`/...), `checkdup`, `colorobj`, `config` (`Folders`), `writer` (per-format writer registry), `cli`.
- boundary: honeybee-core owns the building object model, topology, adjacency, aperture/shading generation, and HBJSON. Geometry primitives are `ladybug-geometry`; weather/location/units are `ladybug-core`; the energy extension is `honeybee-energy`; the urban-massing aggregator ABOVE it is `dragonfly-core`, whose `Model.to_honeybee(object_per_model='Building'|'Story'|'District', use_multiplier=...)` explodes a compact district graph into an array of these honeybee `Model`s (honeybee-core is the translation TARGET, `from_honeybee` the reverse) — the reciprocal seam in `dragonfly-core.md`; the OpenStudio/EnergyPlus translation is `honeybee-openstudio`; the standards library is `honeybee-standards`; HBJSON validation is `honeybee-schema` (pydantic v2). Live UI and mesh display stay outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `honeybee-core`
- Owns: the HBJSON building object model (`Model`/`Room`/`Face`/`Aperture`/`Door`/`Shade`/`ShadeMesh`), the `.properties` extension spine, topology and adjacency solving, aperture and shading generation, unit conversion, the bounded face-type/boundary-condition/auto-number vocabularies, the `dict_to_object` polymorphic deserializer, and the `check_all` validation family
- Accept: building-energy model assembly feeding the energy-modeling owner; this `Model` as the `dragonfly.Model.to_honeybee` translation target (the urban massing explode emits honeybee `Model`s); the `honeybee-energy`/`honeybee-openstudio` extensions composing through `.properties` and `_extend_honeybee`; HBJSON validated through `honeybee-schema` (the `pydantic` rail) and decoded through `msgspec`; `check_all(detailed=True)` reports folded into the `expression` `Result` rail
- Reject: a hand-rolled building object model or HBJSON parser where honeybee-core is admitted; a second DTO family for the HBJSON concept; a per-extension object model parallel to `.properties`; raw-string face-type/boundary-condition branching over the `face_types`/`boundary_conditions` singletons; re-implemented adjacency/solidity/aperture generation; catching the validation exception where `check_all(detailed=True)` returns foldable rows; re-deriving vertex/area/solid algebra that `ladybug-geometry` owns

[CAPTURE_GAP]:
- extension serialization is selective, not all-or-nothing (verified against `honeybee-core 1.60.0`): `Model.to_dict(included_prop=None, ...)` and `ModelProperties.to_dict(include=None)` take an extension-KEY list, not a boolean — `included_prop=['energy']` serializes geometry plus the energy extension and drops radiance, while `included_prop=None` serializes EVERY registered extension. The owner picks the extension key set per export rather than post-filtering a full dict. `Model.from_dict(data)` reconstructs geometry and then runs `properties.apply_properties_from_dict(data)` internally, so a round-trip is symmetric ONLY when the inbound dict carries the extension props the outbound `included_prop` selected; a geometry-only export re-imports with default (empty) extension props.
- `check_all` has two distinct return contracts: with `detailed=False` (default) it returns a single concatenated error string and `raise_exception=True` raises `ValueError` on the first defect; with `detailed=True` it returns a `list[dict]` of error objects (each carrying `type`, `code`, `error_type`, `extension_type`, `element_type`, `element_id`, `element_name`, `message`, plus a nested `parents` chain) and does NOT raise — this is the only shape that folds into a `Result`/validation receipt without an exception trampoline. `Model.validate(json_output=True)` wraps the same machinery into a JSON `ValidationReport` string for CLI/handoff. `check_all` already invokes every registered extension's `check_all` (energy/radiance) when those extension `Model` properties define one, so a full-stack validation is one call with no extra flag — there is no `all_ext_checks` parameter.
- `dict_to_object(honeybee_dict, raise_exception=True)` dispatches on the dict `type` value (`'Model'`/`'Room'`/`'Face'`/`'Aperture'`/`'Door'`/`'Shade'`, plus any `honeybee.boundarycondition` class — `Outdoors`/`Ground`/`Surface`/`Adiabatic`/... — resolved by `hasattr(boundarycondition, type)`); a `ShadeMesh` is reconstructed only inside its parent `Model`/`Room.from_dict`, never as a top-level `dict_to_object` type. A dict missing the `type` key raises `ValueError`; an unrecognized `type` returns `None` when `raise_exception=False` (raising otherwise), so the boundary decoder checks for `None` before promoting. This is the geometry owner's single honeybee-object decode entry — there is no per-type `from_dict` dispatch to write.
