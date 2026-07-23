# [PY_GEOMETRY_API_DRAGONFLY_CORE]

`dragonfly-core` owns the urban 2.5-D massing graph — the `Model` → `Building` → `Story` → `Room2D` hierarchy with `ContextShade` — and its translation into an array of detailed Honeybee energy models. A `Story` multiplier stands for N identical floors and the discriminated window/skylight/shading/roof families keep the graph compact, so a district stays lean until `to_honeybee` explodes it into per-room 3-D geometry. Import name is `dragonfly`, not `dragonfly_core`; importing any `dragonfly_*` plugin auto-registers its `.properties.<ext>` accessor onto every object.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `dragonfly-core`
- package: `dragonfly-core` (AGPL-3.0 network-copyleft)
- module: `dragonfly` — `dragonfly.{model,building,story,room2d,context}` graph, `dragonfly.{windowparameter,skylightparameter,shadingparameter,roof,projection}` parameter families + GeoJSON projection, `dragonfly.{properties,dictutil}` extension host + polymorphic loader
- owner: `geometry`
- rail: energy-companion (urban massing)
- consumer: `.planning/energy/district.md` — dfjson/GeoJSON/massing admission, ordered auto-zoning, the `to_honeybee` explosion seam
- depends: `honeybee-core` (the building model `to_honeybee` targets; transitively `ladybug-geometry`, `ladybug-core`), `dragonfly-schema` (the Pydantic dfjson schema validating `to_dict`/`from_dict`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the urban massing hierarchy (`dragonfly.{model,building,story,room2d,context}`)

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `Model`        | class         | district root; buildings + context, geometry aggregates, extension host |
|  [02]   | `Building`     | class         | stacked stories with multipliers; footprint extrusion                   |
|  [03]   | `Story`        | class         | one modeled floor; adjacency zoning + plenum carving                    |
|  [04]   | `Room2D`       | class         | extruded floor-plate room; fenestration + boolean edits                 |
|  [05]   | `ContextShade` | class         | non-conditioned shading context with transforms                         |

[PUBLIC_TYPE_SCOPE]: discriminated aperture/shade/roof parameter families — the design page dispatches on the concrete type
- [WINDOW]: `_WindowParameterBase` — `SingleWindow` `SimpleWindowArea` `SimpleWindowRatio` `RepeatingWindowRatio` `RepeatingWindowWidthHeight` `RectangularWindows` `DetailedWindows`
- [SKYLIGHT]: `_SkylightParameterBase` — `GriddedSkylightArea` `GriddedSkylightRatio` `DetailedSkylights`
- [SHADING]: `_ShadingParameterBase` — `ExtrudedBorder` `Overhang` `LouversByDistance` `LouversByCount`
- [ROOF]: `RoofSpecification` (`dragonfly.roof`) — sloped-roof geometry attached to a `Story`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the canonical district build/zone/translate calls. Every object carries the symmetric `from_dict(data, *, tolerance)`/`to_dict()` dfjson quartet; `dict_to_object` is the one polymorphic loader, no per-type `from_dict` selection. Run `intersect_adjacency` before `solve_adjacency` on touching rooms. `to_honeybee` `object_per_model` ∈ `District`/`Building`/`Story`, `use_multiplier` keeps story multipliers rather than instancing every floor, and `enforce_adj`/`enforce_solid` gate translation on the validation checks.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------------ | :------- | :------------------------------------------------------ |
|  [01]   | `dictutil.dict_to_object(dragonfly_dict, raise_exception)`    | static   | discriminate any dfjson dict to its concrete object     |
|  [02]   | `Model.from_file(df_file)`                                    | factory  | load a model from dfjson/dfpkl/pomf by extension        |
|  [03]   | `Building.from_footprint(identifier, footprint, ...)`         | factory  | stacked-story building by footprint extrusion           |
|  [04]   | `Room2D.from_polygon(identifier, polygon, floor_height, ...)` | factory  | extruded floor-plate room from a `Polygon2D`            |
|  [05]   | `Room2D.intersect_adjacency(room_2ds, tolerance, ...)`        | static   | split walls so neighbors share coincident segments      |
|  [06]   | `Room2D.solve_adjacency(room_2ds, tolerance, ...)`            | static   | match wall BCs across coincident segments               |
|  [07]   | `Model.to_honeybee(object_per_model, use_multiplier, ...)`    | instance | explode the massing graph into detailed Honeybee models |
|  [08]   | `Model.from_geojson(geojson_file_path, location, point)`      | factory  | import an urban footprint GeoJSON to a massing model    |
|  [09]   | `Model.to_geojson(location, point, folder, tolerance)`        | instance | export massing as URBANopt/GIS feature GeoJSON          |
|  [10]   | `Model.check_all(raise_exception, detailed)`                  | instance | run every check the `enforce_*` flags gate on           |

[ENTRYPOINT_SCOPE]: the supporting rosters the canonical calls compose
- construct: `Model.from_dfjson` `from_dfpkl` `from_pomf` `Room2D.from_vertices`
- discover: `Room2D.find_adjacency` `find_adjacency_by_guide_lines` `find_adjacency_gaps` `group_by_adjacency` `group_by_air_boundary_adjacency`
- edit: `Model.add_model` `add_building` `add_context_shade` `add_prefix` `reset_ids` `resolve_id_collisions` `convert_to_units` `to_rectangular_windows` and the `*_by_identifier` lookups
- building/story: `Building.convert_multipliers_to_stories` `separate_top_bottom_floors` `separate_mid_floors`; `Story.solve_room_2d_adjacency` `intersect_room_2d_adjacency` `make_underground` `set_top_exposed_by_story_above` `split_with_story_above`
- room boolean: `Room2D.to_core_perimeter` `split_with_line` `subtract_room_2ds` `join_by_boundary`; `ContextShade.move` `rotate_xy` `reflect` `scale`
- validate: `check_missing_adjacencies` `check_no_room2d_overlaps` `check_no_roof_overlaps` `check_roofs_above_rooms` `check_self_intersecting_room_2ds` `check_degenerate_room_2ds` `check_room2d_floor_heights_valid` `check_window_parameters_valid` `check_duplicate_room_2d_identifiers`
- project: `projection.meters_to_long_lat_factors` `polygon_to_lon_lat` `lon_lat_to_polygon` `origin_long_lat_from_location` — the equirectangular helpers `to_geojson`/`from_geojson` use

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every object folds through the symmetric dfjson IO quartet and the `.properties.<ext>` extension host; a compact multiplier graph carries the whole district until `to_honeybee` instances it into 3-D Honeybee geometry carrying every registered extension's properties.
- Geometry primitives are `ladybug-geometry` values — `Room2D.floor_geometry` is a `Face3D`, `from_polygon`/`from_footprint` take `Polygon2D`/`Face3D`; dragonfly-core owns the massing abstraction and its Honeybee translation, the detailed room-energy model and geometry primitives are sibling owners.

[STACKING]:
- `ladybug-geometry`(`geometry/.api/ladybug-geometry.md`): `Face3D`/`Polygon2D`/`Point2D`/`Point3D`/`Plane` feed `Room2D.from_polygon`/`from_vertices` and `Building.from_footprint`; every transform (`move`/`rotate_xy`/`reflect`/`scale`) delegates to it, and its rhino3dm converters round-trip a massing model to a Grasshopper/Rhino `Model`.
- `honeybee-core`(`geometry/.api/honeybee-core.md`): `Model.to_honeybee` emits `honeybee.model.Model` objects and `from_honeybee` re-abstracts them into massing — the detailed building-energy model lives there.
- `ladybug-core`(`geometry/.api/ladybug-core.md`): `Model.to_geojson(location=...)` takes a `ladybug` `Location`; unit systems align through it.
- `dragonfly-energy`(`geometry/.api/dragonfly-energy.md`): registers `.properties.energy` onto every object; dragonfly-core defines the `_Properties` hosts (`ModelProperties`, `BuildingProperties`, `StoryProperties`, `Room2DProperties`, `ContextShadeProperties`) it hooks.
- `pydantic`/`msgspec`(`.api/pydantic.md`, `.api/msgspec.md`): `to_dict`/`from_dict` produce/consume dfjson validated by the `dragonfly-schema` Pydantic model at the boundary, then carry the typed `Model` inward; `msgspec` re-encodes the dfjson dict for the artifact wire.
- `universal-pathlib`(`.api/universal-pathlib.md`): point `Model.from_file`/`to_dfjson`/`to_geojson` folder args at a `UPath` so massing models round-trip through the artifact store.
- `anyio`(`.api/anyio.md`): `Model` is pure-Python synchronous — run `to_honeybee` per building through `anyio.to_thread.run_sync` to keep an async owner responsive across many districts.
- `geopandas`/`pyproj`(`data/.api/geopandas.md`, `data/.api/pyproj.md`): `Model.from_geojson`/`to_geojson` is the urban footprint exchange; read a parcel layer with `geopandas` and project with `pyproj` beyond dragonfly's equirectangular `projection` helpers.

[LOCAL_ADMISSION]:
- Consume the AGPL honeybee/ladybug/dragonfly stack out-of-process as a process-boundary companion: invoke it at the edge, exchange dfjson/HBJSON/GeoJSON files across the wire, and let its typed outputs cross into the system while its code stays out of any distributed proprietary artifact.

[RAIL_LAW]:
- Package: `dragonfly-core`
- Owns: the urban massing object graph, the discriminated window/skylight/shading/roof parameter families, the `Room2D` adjacency solvers, urban GeoJSON IO, and translation to Honeybee
- Accept: `dict_to_object` as the polymorphic dfjson loader; `Model.to_honeybee(object_per_model=..., use_multiplier=...)` as the canonical district-to-building explode; `intersect_adjacency` then `solve_adjacency` as the auto-zoning pair; `Model.from_geojson`/`to_geojson` as the GIS exchange; `.properties.energy` (via dragonfly-energy) for energy attributes
- Reject: importing as `dragonfly_core` (the module is `dragonfly`); hand-rolling footprint extrusion that `Building.from_footprint`/`Room2D.from_polygon` own; building geometry primitives locally instead of `ladybug-geometry`; statically linking the AGPL stack into a distributed artifact; `solve_adjacency` before `intersect_adjacency` on touching rooms
