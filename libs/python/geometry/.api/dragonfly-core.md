# [PY_GEOMETRY_API_DRAGONFLY_CORE]

`dragonfly-core` is the district/urban object model of the Ladybug Tools stack — the layer above `honeybee-core` that represents a city block or campus as a 2.5-D massing graph (`Model` → `Building` → `Story` → `Room2D`) plus `ContextShade`, then translates that massing down into an array of detailed Honeybee building-energy models. Stories carry a `multiplier` (one modeled story standing for N identical floors) and discriminated window/skylight/shading/roof parameter families, so an urban model stays compact until `to_honeybee` explodes it into per-room 3-D geometry. It is the URBAN-MASSING leg of the geometry energy-companion owner; the import name is `dragonfly` (NOT `dragonfly_core`), and importing any `dragonfly_*` plugin (e.g. `dragonfly-energy`) auto-registers its `.properties.<ext>` extension onto every object.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `dragonfly-core`
- package: `dragonfly-core`
- import: `import dragonfly` / `from dragonfly.model import Model` (the distribution is `dragonfly-core`; the module is `dragonfly`)
- version: `1.76.27`
- license: AGPL-3.0 — STRONG network-copyleft; see `[LICENSE_BOUNDARY]`
- module: pure Python — `dragonfly.{model,building,story,room2d,context}` (the object graph), `dragonfly.{windowparameter,skylightparameter,shadingparameter,clearstoryparameter,roof,projection}` (parameter families + GeoJSON projection), `dragonfly.properties` (extension hosts), `dragonfly.dictutil` (polymorphic loader)
- owner: `geometry`
- rail: energy-companion (urban massing)
- asset: pure Python over `ladybug-geometry`; no compiled extensions
- depends: `honeybee-core==1.64.55` (exact pin — the building model `to_honeybee` targets; transitively `ladybug-geometry`, `ladybug-core`), `dragonfly-schema==2.2.1` (the Pydantic dfjson schema validating `to_dict`/`from_dict`)
- marker: COMPANION-GATED. Pinned `dragonfly-core; python_version<'3.15'`. The stack is pure-Python and would run on cp315, but the whole Ladybug Tools chain is exact-version-locked (`honeybee-core==1.64.55`, `dragonfly-schema==2.2.1`) and batch-gated `<3.15` as an ecosystem-alignment pin. `assay api resolve dragonfly-core` cannot reflect on the active cp315 interpreter; this surface is source-verified against the `1.76.27` master tree.
- capability: build/edit an urban massing graph, solve inter-room adjacencies, import/export urban GeoJSON, and translate the district into an array of Honeybee energy models with multiplier/plenum/adjacency handling
- scope-law: dragonfly-core owns the URBAN MASSING abstraction and its translation to Honeybee. It is not the detailed building-energy model (`honeybee-core`), not the geometry primitive library (`ladybug-geometry`), and not the energy-simulation extension (`dragonfly-energy`)

[LICENSE_BOUNDARY]: AGPL-3.0 is network-copyleft. dragonfly-core (and the whole honeybee/ladybug/dragonfly stack) must be consumed as a PROCESS-BOUNDARY companion — invoked out-of-process and exchanging data files (dfjson, HBJSON, GeoJSON) across the wire — never statically composed into a distributed proprietary artifact. This is exactly the "energy-companion owner" framing: the companion runs at the edge, its typed outputs cross into the system.

## [02]-[OBJECT_GRAPH]

[GRAPH_SCOPE]: the urban massing hierarchy (`dragonfly.{model,building,story,room2d,context}`)
- rail: energy-companion

| [INDEX] | [SYMBOL] | [ROLE] |
| :-----: | :------- | :----- |
|  [01]   | `Model` | the district root holding `buildings`, `context_shades`, and `units`/`tolerance`/`angle_tolerance`; aggregates `stories`, `room_2ds`, `room_3ds`, `footprint_area`, `floor_area`, `exterior_wall_area`, `volume`, `min`/`max`; `.properties` is the extension host |
|  [02]   | `Building` | one building of stacked `unique_stories` (each with a `multiplier`); `story_count`, `height`, `unique_stories_above_ground`, `zone_dict`, plus optional `room_3ds` for already-detailed floors; `from_footprint(identifier, footprint, floor_to_floor_heights, perimeter_offset=0, tolerance=0)` extrudes a footprint into stories |
|  [03]   | `Story` | one modeled floor of `room_2ds` with `floor_to_floor_height`, `floor_height`, `multiplier`, a `roof: RoofSpecification`, and a `type` (floor/plenum); `is_plenum`, `max_room_2d_ceiling_height`, `is_above_ground` |
|  [04]   | `Room2D` | a single extruded floor-plate room — `floor_geometry` (a `ladybug-geometry` `Face3D`), `floor_to_ceiling_height`, `boundary_conditions`, `window_parameters`, `skylight_parameters`, `shading_parameters`, `air_boundaries`, `is_ground_contact`, `is_top_exposed`, `ceiling_plenum_depth`, `floor_plenum_depth`, `zone`; `from_polygon(identifier, polygon, floor_height, floor_to_ceiling_height, ...)` and `from_vertices(...)` build it from 2-D geometry |
|  [05]   | `ContextShade` | non-conditioned shading context (`geometry`, `is_detached`, `area`) with `move`/`rotate_xy`/`reflect`/`scale` transforms |

[CONSTRUCTOR_LAW]: every object has the symmetric IO quartet — `from_dict(data, tolerance=...)` / `to_dict()` (dfjson), `from_honeybee(...)` / `Model.to_honeybee(...)`, plus `Model.from_file`/`from_dfjson`/`from_dfpkl`/`from_pomf` and `from_geojson`. `dragonfly.dictutil.dict_to_object(dragonfly_dict, raise_exception=True)` is the ONE polymorphic loader that discriminates any dfjson dict to its concrete object — the agentic entry, no per-type `from_dict` selection.

[ADJACENCY_SCOPE]: the `Room2D` static massing solvers (urban auto-zoning)
- `Room2D.solve_adjacency(room_2ds, tolerance=0.01, resolve_window_conflicts=True)` — set matching wall boundary conditions across coincident segments
- `Room2D.intersect_adjacency(room_2ds, tolerance=0.01, preserve_wall_props=True)` — split walls so neighbors share coincident segments before solving
- `Room2D.find_adjacency(room_2ds, tolerance)` / `find_adjacency_by_guide_lines(room_2ds, lines, tolerance)` / `find_adjacency_gaps(room_2ds, gap_distance, tolerance)` — adjacency discovery
- `Room2D.group_by_floor_height(rooms, min_difference=0.01)` / `group_by_adjacency(rooms)` — story/zone grouping
- `Room2D.patch_missing_adjacencies(room_2ds)` — repair an incomplete adjacency set

## [03]-[PARAMETER_FAMILIES]

[FENESTRATION_SCOPE]: discriminated aperture/shade parameter unions (`dragonfly.{windowparameter,skylightparameter,shadingparameter,clearstoryparameter}`) — one concept per family, the design page dispatches on type
- rail: energy-companion

| [INDEX] | [FAMILY] | [MEMBERS] |
| :-----: | :------- | :-------- |
|  [01]   | window (`_WindowParameterBase`) | `SingleWindow`, `SimpleWindowArea`, `SimpleWindowRatio`, `RepeatingWindowRatio`, `RepeatingWindowWidthHeight`, `RectangularWindows`, `DetailedWindows` (the latter two extend `_AsymmetricBase` for per-wall explicit geometry) |
|  [02]   | skylight (`_SkylightParameterBase`) | `GriddedSkylightArea`, `GriddedSkylightRatio`, `DetailedSkylights` |
|  [03]   | shading (`_ShadingParameterBase`) | `ExtrudedBorder`, `Overhang`, `LouversByDistance`, `LouversByCount` (louvers extend `_LouversBase`) |
|  [04]   | clerestory (`_ClearstoryParameterBase`) | `DetailedClearstory` |
|  [05]   | roof | `RoofSpecification` (`dragonfly.roof`) — the sloped-roof geometry attached to a `Story` |

[PROJECTION_SCOPE]: GeoJSON ↔ model projection (`dragonfly.projection`)
- `meters_to_long_lat_factors(origin_lon_lat=(0, 0))`, `polygon_to_lon_lat(polygon, origin_lon_lat, conversion_factors=None)`, `lon_lat_to_polygon(coords, origin_lon_lat, ...)`, `origin_long_lat_from_location(location, point)` — the equirectangular helpers `Model.to_geojson`/`from_geojson` use to place a metric massing model on the globe.

## [04]-[TRANSLATION]

[TO_HONEYBEE]: `Model.to_honeybee(object_per_model='Building', shade_distance=None, use_multiplier=True, exclude_plenums=False, cap=False, solve_ceiling_adjacencies=False, merge_method=None, tolerance=None, enforce_adj=True, enforce_solid=True) -> list[honeybee.model.Model]`
- the core explode: `object_per_model` ∈ `District` (one Honeybee Model) / `Building` / `Story`; `use_multiplier` keeps story multipliers (fast) vs. fully instancing every floor; `exclude_plenums` drops plenum stories. This is where the compact urban graph becomes detailed 3-D Honeybee geometry carrying every registered extension's properties.

[GEOJSON_IO]: `Model.from_geojson(geojson_file_path, location=None, point=Point2D(0,0), ...)` imports an urban footprint GeoJSON into a massing model; `Model.to_geojson(location, point=Point2D(0,0), folder=None, tolerance=None)` / `to_geojson_dict(...)` exports it — the URBANopt/GIS feature exchange the energy extension builds on.

[VALIDATION]: `Model.check_all(...)`, `check_for_extension(...)`, and the focused checks (`check_all_room_collisions`, `check_missing_adjacencies`, `check_no_room2d_overlaps`, `check_roofs_above_rooms`, `check_self_intersecting_room_2ds`, `check_degenerate_room_2ds`, …) return validation reports; `enforce_*` flags on `to_honeybee` gate translation on them.

[EDIT_OPS]: `Model.add_model`/`add_building`/`add_context_shade`, `add_prefix`, `reset_ids`/`resolve_id_collisions`, `convert_to_units('Meters')`, `separate_top_bottom_floors`, `to_rectangular_windows`, and the `*_by_identifier` lookups — the in-place massing edits.

## [05]-[INTEGRATION]

[SUBSTRATE_STACK]: stacking onto the universal Python rails (`libs/python/.api/`)
- `pydantic` / `msgspec` — `to_dict`/`from_dict` produce/consume dfjson validated by `dragonfly-schema` (a Pydantic schema); validate at the boundary with the schema model, then carry the typed `Model` inward. `msgspec` re-encodes the dfjson dict for the artifact wire.
- `universal-pathlib` — point `Model.from_file`/`to_dfjson`/`to_geojson` folder args at a `UPath` so massing models round-trip through the artifact store.
- the model is pure-Python and synchronous; when translating many districts, run `to_honeybee` through `anyio.to_thread.run_sync` per building to keep an async owner responsive.

[SIBLING_STACK]: stacking with the geometry folder + cross-folder data (`libs/python/geometry/.api/`, `libs/python/data/.api/`)
- `ladybug-geometry` — the geometry primitive owner: `Face3D`, `Polygon2D`, `Point2D`/`Point3D`, `Plane` are the inputs to `Room2D.from_polygon`/`from_vertices` and `Building.from_footprint`; all transforms (`move`/`rotate_xy`/`reflect`/`scale`) delegate to it.
- `honeybee-core` — the translation target: `Model.to_honeybee` emits `honeybee.model.Model` objects, `from_honeybee` re-abstracts them into massing; the detailed building-energy model lives there.
- `ladybug-core` — weather/location/units: `Model.to_geojson(location=...)` takes a `ladybug` `Location`; unit systems align through it.
- `dragonfly-energy` — the energy extension that registers `.properties.energy` onto every object here (see `dragonfly-energy.md`); dragonfly-core defines the `_Properties` hosts (`ModelProperties`, `BuildingProperties`, `StoryProperties`, `Room2DProperties`, `ContextShadeProperties`) it hooks.
- `rhino3dm` — round-trip massing geometry to Rhino via `ladybug-geometry`'s rhino3dm converters, so a Grasshopper/Rhino urban model becomes a dragonfly `Model`.
- `geopandas` / `shapely` / `pyproj` (data folder, cross-folder) — `Model.from_geojson`/`to_geojson` is the urban footprint exchange; pair with `geopandas` to read a parcel layer and `pyproj` for an accurate projection beyond dragonfly's equirectangular `projection` helpers.

[RAIL_LAW]:
- Package: `dragonfly-core`
- Owns: the urban massing object graph (`Model`/`Building`/`Story`/`Room2D`/`ContextShade`), the discriminated window/skylight/shading/clerestory/roof parameter families, the Room2D adjacency solvers, urban GeoJSON IO, and translation to Honeybee
- Accept: `dict_to_object` as the polymorphic dfjson loader; `Model.to_honeybee(object_per_model=..., use_multiplier=...)` as the canonical district→building explode; `Room2D.intersect_adjacency` then `solve_adjacency` as the auto-zoning pair; `Model.from_geojson`/`to_geojson` as the GIS exchange; `.properties.energy` (via dragonfly-energy) for energy attributes
- Reject: importing it as `dragonfly_core` (the module is `dragonfly`); hand-rolling footprint extrusion when `Building.from_footprint`/`Room2D.from_polygon` own it; statically linking the AGPL stack into a distributed artifact (consume it as a process-boundary companion); skipping `intersect_adjacency` before `solve_adjacency` on touching rooms; building geometry primitives locally instead of using `ladybug-geometry`
