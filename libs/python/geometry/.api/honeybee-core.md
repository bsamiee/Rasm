# [PY_GEOMETRY_API_HONEYBEE_CORE]

`honeybee-core` owns the HBJSON building-energy object model: the parented `Model` -> `Room` -> `Face` -> `Aperture`/`Door` graph and its `Shade`/`ShadeMesh`, each wrapping a `ladybug-geometry` primitive, with the `.properties` extension spine, adjacency and aperture/shading generation, the closed face-type/boundary-condition/auto-number vocabularies, `dict_to_object`, and the `check_all` validation family. It is the `dragonfly-core` urban-explode target and the host every energy/radiance extension attaches to, while geometry primitives, weather, and simulation stay in siblings.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `honeybee-core`
- package: `honeybee-core` (AGPL-3.0, Ladybug Tools)
- module: `honeybee` (distribution `honeybee-core`; import `honeybee`, not `honeybee_core`)
- namespaces: `honeybee.{model,room,face,aperture,door,shade,shademesh,properties,facetype,boundarycondition,altnumber,dictutil,extensionutil,units,orientation,search,checkdup}`
- rail: energy-modeling
- consumer: `.planning/energy/model.md` (the `check_all`-gated admission, aperture mint, adjacency, HBJSON wire)
- asset: pure-Python `py3-none-any` wheel; no compiled payload
- depends: `ladybug-core` (units/location/EPW), `ladybug-geometry-polyskel` (the `ladybug-geometry` primitive tier honeybee objects wrap), `honeybee-schema` (HBJSON pydantic-v2 wire validation)
- entry: `honeybee` console script (`validate`/`edit`/`create`/`lib` over HBJSON)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry object model (`honeybee.{model,room,face,aperture,door,shade,shademesh}`)

Every object wraps a `ladybug_geometry` primitive and carries an `identifier`, a mutable `display_name`, free-form `user_data`, and a `.properties` handle; the graph is parented (`Face.parent`/`has_parent`, `Room.faces`, `Model.rooms`) so geometric edits propagate through the owner.

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                                                                      |
| :-----: | :---------- | :------------ | :------------------------------------------------------------------------------------------------ |
|  [01]   | `Model`     | model root    | top-level container of rooms, orphaned faces/apertures/doors, and shades; units/tolerance carrier |
|  [02]   | `Room`      | volume        | closed `Polyface3D` solid of bounding `Face`s; the zone/HVAC/program assignment unit              |
|  [03]   | `Face`      | boundary      | a `Face3D` wall/roof/floor with a `boundary_condition`, a `facetype`, and sub-faces               |
|  [04]   | `Aperture`  | sub-face      | operable/fixed window opening on a parent `Face`                                                  |
|  [05]   | `Door`      | sub-face      | opaque or glass door opening on a parent `Face`                                                   |
|  [06]   | `Shade`     | shading       | indoor/outdoor/detached single-face shading surface                                               |
|  [07]   | `ShadeMesh` | shading       | a `Mesh3D` shade for many-faced context shading (cheaper than per-face `Shade`)                   |

[PUBLIC_TYPE_SCOPE]: extension spine (`honeybee.properties`)

One `*Properties` host per object kind, each a thin attribute bag whose extension slots (`.energy`/`.radiance`/`.doe2`) an extension package populates via `_extend_honeybee`; the owner folds every extension's `to_dict(include=...)`/`apply_properties_from_dict` through one extension-key table, never a branch per extension. Hosts `FaceProperties`, `ApertureProperties`, `DoorProperties`, `ShadeProperties`, and `ShadeMeshProperties` are identical thin bags.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :---------------- | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `ModelProperties` | extension bag | model-level host; `to_dict(include=None)`, `apply_properties_from_dict(data)` |
|  [02]   | `RoomProperties`  | extension bag | room-level host; `add_prefix`, `reset_to_default`, transform passthrough      |

[PUBLIC_TYPE_SCOPE]: bounded vocabularies (`honeybee.{facetype,boundarycondition,altnumber}`)

Closed singleton-backed vocabularies read through the module singletons (`face_types`, `boundary_conditions`), never by constructing the leaf classes, so a face type or boundary condition is a value-object lookup rather than a string branch.

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY]       | [CAPABILITY]                                                |
| :-----: | :--------------------------------------------------- | :------------------ | :---------------------------------------------------------- |
|  [01]   | `Wall` / `RoofCeiling` / `Floor` / `AirBoundary`     | face type           | the four face types                                         |
|  [02]   | `Outdoors(sun_exposure, wind_exposure, view_factor)` | boundary condition  | exterior face exposed to weather                            |
|  [03]   | `Ground`                                             | boundary condition  | face in contact with ground                                 |
|  [04]   | `Surface(boundary_condition_objects, sub_face)`      | boundary condition  | interior face adjacent to another (adjacency pair)          |
|  [05]   | `Adiabatic` / `OtherSideTemperature`                 | boundary condition  | no-heat-flow / fixed other-side-temperature                 |
|  [06]   | `Autocalculate` / `NoLimit` / `Autosize`             | alt-number sentinel | singleton sentinels (`autocalculate`/`no_limit`/`autosize`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: HBJSON serialization round-trip

HBJSON is the canonical wire shape; `dict_to_object` deserializes polymorphically on the dict `type` key.

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `Model.to_dict(included_prop, triangulate_sub_faces, include_plane)`    | instance | HBJSON dict; `included_prop` selects extensions    |
|  [02]   | `Model.from_dict(data)`                                                 | factory  | reconstruct a live `Model` + extension props       |
|  [03]   | `Model.to_hbjson(name, folder)`                                         | instance | write an `.hbjson` file (graduation handle)        |
|  [04]   | `Model.from_hbjson(hbjson_file)`                                        | factory  | read an `.hbjson` file                             |
|  [05]   | `Model.to_hbpkl` / `Model.from_hbpkl` / `Model.from_file`               | factory  | pickle round-trip; `from_file` auto-detects format |
|  [06]   | `dictutil.dict_to_object(honeybee_dict, raise_exception)`               | static   | polymorphic deserialize on the dict `type` key     |
|  [07]   | `ModelProperties.to_dict(include)` / `apply_properties_from_dict(data)` | instance | extension-prop serialize / re-attach               |

- `Model.to_dict`: `included_prop` is an extension-KEY list (`['energy']` serializes geometry with energy and drops radiance), never a boolean; `None` serializes every registered extension, and `from_dict` re-attaches only the props the outbound `included_prop` selected.
- `dict_to_object`: dispatches on the dict `type` (`Model`/`Room`/`Face`/`Aperture`/`Door`/`Shade`, and any `boundarycondition` class via `hasattr`); a `ShadeMesh` reconstructs only inside its parent, a missing `type` raises `ValueError`, an unrecognized `type` returns `None` under `raise_exception=False`.

[ENTRYPOINT_SCOPE]: model assembly, generators, and adjacency (`honeybee.{model,room}`)

Construction routes through `from_*` classmethods; the `Room` static surface owns grouping, adjacency, and procedural plan generation, so the owner calls these instead of re-deriving topology.

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :-------------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Model(identifier, rooms, ..., units='Meters', tolerance, angle_tolerance)` | ctor     | assemble from rooms/orphaned objects/shades   |
|  [02]   | `Model.from_objects(identifier, objects, units, tolerance)`                 | factory  | build a model from a flat object list         |
|  [03]   | `Model.from_shoe_box` / `from_rectangle_plan` / `from_l_shaped_plan`        | factory  | procedural test/parametric generators         |
|  [04]   | `Room.from_box` / `from_polyface3d` / `from_dict`                           | factory  | build a room from a box or a closed solid     |
|  [05]   | `Room.solve_adjacency(rooms, tolerance)`                                    | static   | faces to `Surface` pairs (also on `Model`)    |
|  [06]   | `Room.intersect_adjacency(rooms)`                                           | static   | split coincident faces for shared segments    |
|  [07]   | `Room.group_by_adjacency` / `group_by_air_boundary_adjacency`               | static   | group by shared-wall / air-boundary adjacency |
|  [08]   | `Room.group_by_floor_height` / `group_by_orientation`                       | static   | group by floor height / orientation           |
|  [09]   | `Room.stories_by_floor_height` / `Room.story`                               | static   | story assignment from floor height            |
|  [10]   | `Room.grouped_horizontal_boundary`                                          | static   | merged horizontal boundary                    |
|  [11]   | `Model.convert_to_units` / `conversion_factor_to_meters`                    | instance | scale geometry into a unit system             |

[ENTRYPOINT_SCOPE]: aperture and shading generation (`honeybee.{face,model}`)

Window-to-wall ratio, gridded glazing, louvers, and overhangs are owned operations the owner composes rather than hand-cutting `Face3D` openings.

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Face.apertures_by_ratio(ratio, tolerance, rect_split)`                 | instance | windows to a window-to-wall ratio  |
|  [02]   | `Face.apertures_by_ratio_gridded` / `apertures_by_ratio_rectangle`      | instance | gridded / rectangular WWR variants |
|  [03]   | `Face.aperture_by_width_height` / `apertures_by_width_height_rectangle` | instance | place a fixed-size window          |
|  [04]   | `Model.wall_apertures_by_ratio` / `Model.skylight_apertures_by_ratio`   | instance | model-wide aperture generation     |
|  [05]   | `Face.louvers_by_count` / `louvers_by_distance_between` / `overhang`    | instance | louver/overhang shades on a face   |
|  [06]   | `Room.generate_grid` / `Model.generate_exterior_aperture_grid`          | instance | sensor grids for daylight handoff  |

[ENTRYPOINT_SCOPE]: validation, vocabularies, and search (`honeybee.{facetype,boundarycondition,units,orientation,search,checkdup}`)

`check_all` and `validate` gate model admission; vocabulary and search helpers resolve types/conditions from geometry (module prefix elided in the table).

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `Model.check_all(raise_exception, detailed)`                            | instance | every geometry check + extensions' `check_all` |
|  [02]   | `Model.validate(check_function, check_args, json_output)`               | instance | static validation; `json_output=True` report   |
|  [03]   | `Model.check_missing_adjacencies` / `check_rooms_solid`                 | instance | individual checks `check_all` folds            |
|  [04]   | `check_planar` / `check_self_intersecting` / `check_duplicate_*`        | instance | more individual checks `check_all` folds       |
|  [05]   | `facetype.get_type_from_normal(normal_vector, roof_angle, floor_angle)` | static   | classify a face type from its normal           |
|  [06]   | `boundarycondition.get_bc_from_position(positions, ground_depth)`       | static   | classify ground/outdoors from elevation        |
|  [07]   | `units.conversion_factor_to_meters` / `parse_distance_string`           | static   | unit conversion + distance-string parsing      |
|  [08]   | `orientation.angles_from_num_orient` / `orient_index`                   | static   | orientation binning for parametrics            |
|  [09]   | `orientation.face_orient_index`                                         | static   | per-face orientation index                     |
|  [10]   | `search.filter_array_by_keywords` / `get_attr_nested`                   | static   | keyword + nested-attribute filtering           |
|  [11]   | `checkdup.check_duplicate_identifiers` / `is_equivalent`                | static   | duplicate-id detection; geometric equivalence  |

- `Model.check_all`: `detailed=False` returns one concatenated string and raises `ValueError` on the first defect; `detailed=True` returns a `list[dict]` of error rows (`type`/`code`/`error_type`/`element_id`/`message`/nested `parents`) and does NOT raise — the only shape that folds into a `Result` receipt. It already invokes every registered extension's `check_all`, so full-stack validation is one call with no extra flag.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every honeybee object wraps a `ladybug_geometry` primitive (`Face`/`Aperture`/`Door`/`Shade` a `Face3D`, `Room` a `Polyface3D`, `ShadeMesh` a `Mesh3D`); the owner composes vertex/normal/area/planarity/solid algebra from `ladybug-geometry` and folds transforms (`move`/`rotate`/`rotate_xy`/`reflect`/`scale`) and aperture/louver/overhang generation into owned face operations, never a per-transform branch.
- `.properties` is the single extension dispatch surface: extensions register `*EnergyProperties`/`*RadianceProperties` onto the host classes via `_extend_honeybee`, so the owner reads `room.properties.energy` through one extension-key table and serializes via `to_dict(include=[...])`, never a parallel per-extension object model; `apply_properties_from_dict(data)` is the re-attach leg after `from_dict`.
- `to_dict` emits exactly the shape `honeybee-schema` pydantic-v2 models validate (`*Abridged` variants are the compact wire forms); `dict_to_object` is the one polymorphic decode dispatching on the dict `type` key.
- `check_all(detailed=True)` and `validate(json_output=True)` return structured error rows folded into the `expression` `Result` rail; individual `check_*` methods are the per-rule legs `check_all` aggregates.
- `facetype`, `boundarycondition`, and `altnumber` are closed value-object vocabularies resolved through the `face_types`/`boundary_conditions` singletons and `get_type_from_normal`/`get_bc_from_position`.

[STACKING]:
- `ladybug-geometry`(`.api/ladybug-geometry.md`): every object holds a `Face3D`/`Polyface3D`/`Mesh3D`; `Room.from_polyface3d`, `Face` construction, and all transforms delegate to its primitive algebra.
- `dragonfly-core`(`.api/dragonfly-core.md`): honeybee-core is the translation target — `dragonfly.Model.to_honeybee(object_per_model='Building'|'Story'|'District', use_multiplier=...)` explodes an urban massing graph into an array of these `Model`s, `from_honeybee` reverses it.
- `honeybee-energy`(`.api/honeybee-energy.md`), `honeybee-openstudio`(`.api/honeybee-openstudio.md`): compose through `.properties.energy` and `_extend_honeybee`; `honeybee-openstudio` is the OpenStudio/EnergyPlus translation, `honeybee-standards`(`.api/honeybee-standards.md`) the standards library.
- `pydantic`, `msgspec`(`libs/python/.api/`): validate an inbound HBJSON dict through `honeybee-schema` (the `pydantic` rail) at the boundary, decode the body through `msgspec`, then materialize via `Model.from_dict`/`dict_to_object`.
- `expression`(`libs/python/.api/expression.md`): fold `check_all(detailed=True)` error rows into the `Result` rail, turning a geometry-defect model into a typed failure carrier rather than a caught exception.

[LOCAL_ADMISSION]:
- Building-energy model assembly feeds the energy-modeling owner and admits this `Model` as the `dragonfly.Model.to_honeybee` translation target.
- Consume the AGPL-3.0 stack as a process-boundary companion exchanging HBJSON across the wire, never statically linked into a distributed proprietary artifact.

[RAIL_LAW]:
- Package: `honeybee-core`
- Owns: the HBJSON building object model (`Model`/`Room`/`Face`/`Aperture`/`Door`/`Shade`/`ShadeMesh`), the `.properties` extension spine, topology and adjacency solving, aperture and shading generation, unit conversion, the bounded face-type/boundary-condition/auto-number vocabularies, `dict_to_object`, and the `check_all` validation family
- Accept: building-energy model assembly feeding the energy-modeling owner; this `Model` as the `dragonfly.Model.to_honeybee` target; the `honeybee-energy`/`honeybee-openstudio` extensions composing through `.properties` and `_extend_honeybee`; HBJSON validated through `honeybee-schema` and decoded through `msgspec`; `check_all(detailed=True)` rows folded into the `expression` `Result` rail
- Reject: a hand-rolled building object model or HBJSON parser; a second DTO family for the HBJSON concept; a per-extension object model parallel to `.properties`; raw-string face-type/boundary-condition branching over the singletons; re-implemented adjacency/solidity/aperture generation; catching the validation exception where `check_all(detailed=True)` returns foldable rows; re-deriving vertex/area/solid algebra that `ladybug-geometry` owns
