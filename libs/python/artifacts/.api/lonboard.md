# [PY_ARTIFACTS_API_LONBOARD]

`lonboard` supplies the WebGL-accelerated geospatial map rendering surface for the artifacts geo-viz rail: a `Map` widget backed by deck.gl with a layer family (`ScatterplotLayer`, `PathLayer`, `PolygonLayer`, `SolidPolygonLayer`, `ArcLayer`, `HeatmapLayer`, `ColumnLayer`, `TripsLayer`, `H3HexagonLayer`, `GeohashLayer`, `S2Layer`, `BitmapTileLayer`), view state constructors, layer extension modifiers, colormap utilities, basemap enums, and a `viz` convenience function for automatic layer type inference. Every layer consumes `ArrowStreamExportable` Arrow data and produces a notebook-renderable widget. The package owner drives all deck.gl map construction through these surfaces and never re-implements layer geometry or WebGL primitives.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lonboard`
- package: `lonboard`
- import: `lonboard`
- owner: `artifacts`
- rail: geo-viz
- asset: runtime library (ipywidgets / anywidget deck.gl bridge)
- installed: `0.16.0` reflected via `/tmp/wfpy-artifacts313/bin/python`

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: map widget and layer base
- rail: geo-viz

| [INDEX] | [SYMBOL]    | [PACKAGE_ROLE] | [CAPABILITY]                                                         |
| :-----: | :---------- | :------------- | :------------------------------------------------------------------- |
|   [1]   | `Map`       | map widget     | `(layers, *, basemap_style, **kwargs)` — deck.gl WebGL map container |
|   [2]   | `BaseLayer` | layer base     | abstract base carrying Arrow data and common style kwargs            |

[PUBLIC_TYPE_SCOPE]: geospatial layer family
- rail: geo-viz

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE]     | [CAPABILITY]                                                                      |
| :-----: | :------------------ | :----------------- | :-------------------------------------------------------------------------------- |
|   [1]   | `ScatterplotLayer`  | point layer        | `(table, **kwargs)` — filled circles at point geometry column                     |
|   [2]   | `PathLayer`         | line layer         | `(table, **kwargs)` — polyline strokes at linestring geometry column              |
|   [3]   | `PolygonLayer`      | polygon layer      | `(table, **kwargs)` — filled and/or stroked polygons at polygon geometry column   |
|   [4]   | `SolidPolygonLayer` | solid polygon      | `(table, **kwargs)` — filled-only polygons without stroke overhead                |
|   [5]   | `ArcLayer`          | arc layer          | `(table, get_source_position, get_target_position, **kwargs)` — great-circle arcs |
|   [6]   | `HeatmapLayer`      | heatmap layer      | `(table, **kwargs)` — GPU-accelerated point density heatmap                       |
|   [7]   | `ColumnLayer`       | column layer       | `(table, **kwargs)` — extruded columns at point positions                         |
|   [8]   | `TripsLayer`        | trips layer        | `(table, get_timestamps, **kwargs)` — animated trajectories with timestamp column |
|   [9]   | `BitmapLayer`       | raster image layer | `(**kwargs)` — georeferenced bitmap at four corner coordinates                    |
|  [10]   | `BitmapTileLayer`   | tile raster layer  | `(**kwargs)` — slippy-map tile raster layer                                       |
|  [11]   | `PointCloudLayer`   | 3-D point cloud    | `(table, **kwargs)` — 3-D point cloud with x/y/z positions                        |
|  [12]   | `H3HexagonLayer`    | H3 index layer     | `(table, get_hexagon, **kwargs)` — hexagonal cells by H3 cell index               |
|  [13]   | `GeohashLayer`      | geohash layer      | `(table, get_geohash, **kwargs)` — cells by geohash string token                  |
|  [14]   | `S2Layer`           | S2 index layer     | `(table, get_s2_token, **kwargs)` — cells by S2 cell token string                 |
|  [15]   | `A5Layer`           | A5 index layer     | `(table, get_pentagon, **kwargs)` — cells by A5 pentagon index                    |

[PUBLIC_TYPE_SCOPE]: view state and extension types
- rail: geo-viz

| [INDEX] | [SYMBOL]                                   | [PACKAGE_ROLE]   | [CAPABILITY]                                                         |
| :-----: | :----------------------------------------- | :--------------- | :------------------------------------------------------------------- |
|   [1]   | `view_state.MapViewState`                  | 2-D view state   | `(longitude, latitude, zoom, pitch, bearing, …)` — standard map view |
|   [2]   | `view_state.GlobeViewState`                | globe view state | `(longitude, latitude, zoom, …)` — full-globe projection             |
|   [3]   | `view_state.FirstPersonViewState`          | first-person     | `(longitude, latitude, position, bearing, pitch, …)`                 |
|   [4]   | `layer_extension.BrushingExtension`        | brushing         | hover-radius highlight modifier                                      |
|   [5]   | `layer_extension.CollisionFilterExtension` | collision filter | density-based label/point de-overlap modifier                        |
|   [6]   | `layer_extension.DataFilterExtension`      | data filter      | GPU-side attribute range filter modifier                             |
|   [7]   | `layer_extension.PathStyleExtension`       | path style       | dashed/offset path style modifier for `PathLayer`                    |

[PUBLIC_TYPE_SCOPE]: colormap and basemap utilities
- rail: geo-viz

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]       | [CAPABILITY]                                                    |
| :-----: | :-------------------------------- | :------------------- | :-------------------------------------------------------------- |
|   [1]   | `colormap.apply_continuous_cmap`  | continuous colormap  | map float array through a matplotlib-compatible palette to RGBA |
|   [2]   | `colormap.apply_categorical_cmap` | categorical colormap | map categorical values through a `DiscreteColormap` to RGBA     |
|   [3]   | `basemap.CartoBasemap`            | basemap enum         | `Positron`, `DarkMatter`, `Voyager` (with and without labels)   |
|   [4]   | `basemap.CartoStyle`              | basemap style type   | accepted by `Map(basemap_style=…)` alongside URL strings        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: map construction and export
- rail: geo-viz

| [INDEX] | [SURFACE]                                                                   | [ENTRY_FAMILY]    | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------------------------------- | :---------------- | :---------------------------------------------------- |
|   [1]   | `Map(layers, *, basemap_style, **kwargs)`                                   | map init          | create a deck.gl map widget from one or more layers   |
|   [2]   | `Map.to_html(filename, title)`                                              | HTML export       | write or return self-contained HTML string            |
|   [3]   | `viz(data, *, scatterplot_kwargs, path_kwargs, polygon_kwargs, map_kwargs)` | convenience entry | infer geometry type and produce a `Map` automatically |

[ENTRYPOINT_SCOPE]: vector layer construction
- rail: geo-viz

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|   [1]   | `ScatterplotLayer(table, **kwargs)`                                   | point layer    | Arrow table with point geometry column                |
|   [2]   | `PathLayer(table, **kwargs)`                                          | line layer     | Arrow table with linestring geometry column           |
|   [3]   | `PolygonLayer(table, **kwargs)`                                       | polygon layer  | Arrow table with polygon geometry column              |
|   [4]   | `SolidPolygonLayer(table, **kwargs)`                                  | solid polygon  | Arrow table with polygon geometry, fill-only          |
|   [5]   | `ArcLayer(table, get_source_position, get_target_position, **kwargs)` | arc layer      | origin-destination arc layer from two point accessors |
|   [6]   | `HeatmapLayer(table, **kwargs)`                                       | heatmap        | weighted point density from Arrow table               |
|   [7]   | `ColumnLayer(table, **kwargs)`                                        | extruded bars  | 3-D extruded columns at point positions               |
|   [8]   | `TripsLayer(table, get_timestamps, **kwargs)`                         | animated trips | trajectory animation with separate timestamp column   |
|   [9]   | `H3HexagonLayer(table, get_hexagon, **kwargs)`                        | H3 cells       | H3 resolution-agnostic hexagonal fill                 |
|  [10]   | `GeohashLayer(table, get_geohash, **kwargs)`                          | geohash cells  | geohash-precision polygon fill                        |

[ENTRYPOINT_SCOPE]: colormap operations
- rail: geo-viz

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY]    | [CAPABILITY]                                                     |
| :-----: | :----------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|   [1]   | `apply_continuous_cmap(values, cmap, *, alpha)`  | continuous color  | `NDArray[float]` → RGBA `NDArray[uint8]` via palette             |
|   [2]   | `apply_categorical_cmap(values, cmap, *, alpha)` | categorical color | categorical array → RGBA `NDArray[uint8]` via `DiscreteColormap` |

## [4]-[IMPLEMENTATION_LAW]

[GEO_VIZ_TOPOLOGY]:
- namespace: `lonboard`; all layer types accept `ArrowStreamExportable` (GeoArrow-compatible Arrow table with geometry extension type column)
- geometry column: lonboard layers expect a GeoArrow-typed column; `geopandas` GeoDataFrames and `pyarrow` tables with WKB/GeoArrow extension columns are the primary inputs
- `viz(data)` dispatches automatically by geometry type (Point→`ScatterplotLayer`, LineString→`PathLayer`, Polygon→`PolygonLayer`); per-layer kwargs override the inferred defaults
- colormap: `apply_continuous_cmap(values, cmap)` produces a `uint8` RGBA array suitable for `get_fill_color` or `get_color` kwargs; alpha accepts a float scalar or per-row float array
- basemap: `CartoBasemap.Positron`, `.DarkMatter`, `.Voyager` are `str`-subclass enum values; any tile URL string is also accepted by `Map(basemap_style=…)`
- extensions: layer extension objects are passed as a list to a layer's `extensions` kwarg; they modify GPU render behaviour without altering the data pipeline
- export: `Map.to_html(filename=None)` returns a string when `filename` is `None`; when a path is provided it writes and returns `None`

[LOCAL_ADMISSION]:
- layer data enters as an Arrow table via `ArrowStreamExportable`; geopandas GeoDataFrames require explicit conversion to Arrow before non-trivial batch use.
- `viz(data)` is the fast path for single-geometry exploration; production map owners use explicit layer constructors to control accessor kwargs.
- colormap arrays produced by `apply_continuous_cmap` / `apply_categorical_cmap` are passed directly to `get_fill_color`, `get_color`, or equivalent layer kwargs.
- `Map.to_html` is the headless export path; notebook display uses the ipywidget repr automatically.

[RAIL_LAW]:
- Package: `lonboard`
- Owns: WebGL deck.gl geospatial map widget, vector and index layer family, GPU colormap mapping, and self-contained HTML export
- Accept: Arrow-backed GeoArrow tables as layer data; `CartoBasemap` or tile-URL strings for basemap; `apply_continuous_cmap` / `apply_categorical_cmap` for colour encoding
- Reject: hand-rolled deck.gl JSON or manual tile-layer wiring where the layer constructors apply; re-implementing colormap logic where `apply_continuous_cmap` operates
