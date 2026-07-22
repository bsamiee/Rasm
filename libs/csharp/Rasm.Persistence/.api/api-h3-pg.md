# [RASM_PERSISTENCE_API_H3_PG]

`h3-pg` mints Uber-H3 hexagonal hierarchical geospatial indexing inside PostgreSQL: the `h3index` 64-bit cell type with its operator algebra and the `h3_*` function surface, paired with the `h3_postgis` bridge projecting cells to and from PostGIS `geometry`, `geography`, EWKB, and raster. Both extensions ship as server-side C with no managed assembly, so the identity tier drives every cell function as raw SQL and stores the result as a `bigint` cell column.

One cell vocabulary spans in-process and in-database indexing: the id this surface computes is bit-for-bit the id the managed `pocketken.H3` pin computes at ingest.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `h3-pg` — extensions `h3` + `h3_postgis`
- package: `h3-pg` (Apache-2.0, `zachasme/h3-pg`) — server-side PostgreSQL C extension pair, no NuGet asset
- namespace: SQL `public` — the `h3index` type, the `h3_*` functions, the operator, cast, and operator-class set
- registration: both extensions are `RELOCATABLE` and preload-free; `h3` stands alone and `h3_postgis` requires `h3`, `postgis`, `postgis_raster`, which one `CASCADE` install pulls
- consumed by: `Element/identity` cell columns and the `Query/lane` `SetPredicate.Cell` lowering, executed over raw `Npgsql`
- rail: geospatial-index, geo-provisioning

## [02]-[CELL_TYPE]

[CELL_TYPE_SCOPE]: `h3index` — an unsigned 64-bit id naming any H3 object, declared `LIKE int8` so storage and pass-by-value match `bigint`; text I/O is the lowercase hex form (`'8928308280fffff'`) and `h3index_recv`/`h3index_send` carry the binary wire.

[CELL_ENTRY_SCOPE]: the operator algebra and casts over the cell type

| [INDEX] | [SURFACE]                                  | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :----------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `h3index <-> h3index -> bigint`            | operator | grid distance at the lower of the two resolutions     |
|  [02]   | `h3index = h3index -> boolean`             | operator | equality; `h3index_eq` carries `HASHES, MERGES`       |
|  [03]   | `h3index <> h3index -> boolean`            | operator | inequality (`h3index_ne`)                             |
|  [04]   | `h3index < <= > >= h3index -> boolean`     | operator | raw 64-bit ordering backing btree and brin            |
|  [05]   | `h3index && h3index -> boolean`            | operator | hierarchical intersection (`h3index_overlaps`)        |
|  [06]   | `h3index @> h3index -> boolean`            | operator | ancestor containment (`h3index_contains`)             |
|  [07]   | `h3index <@ h3index -> boolean`            | operator | descendant containment (`h3index_contained_by`)       |
|  [08]   | `geometry @ integer -> h3index`            | operator | index a PostGIS point, shorthand for the bridge index |
|  [09]   | `h3index::bigint` / `bigint::h3index`      | cast     | durable `bigint` column round-trip                    |
|  [10]   | `h3index::point`                           | cast     | centroid as a native PG point                         |
|  [11]   | `h3index::geometry` / `h3index::geography` | cast     | centroid as a PostGIS value                           |

- `geometry @ integer`: a `geography` left operand pairs it; both ride `h3_postgis`.

## [03]-[INDEXING]

[INDEXING_ENTRY_SCOPE]: location-to-cell conversion and cell inspection

A native `point` argument reads `(lat, lng)`; the `h3_postgis` `geometry`/`geography` overloads read SRID-4326 `(lng, lat)` and cast internally to the same cell. Resolution spans `0..15`.

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :----------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `h3_latlng_to_cell(point, integer) -> h3index`   | function | index a location at a resolution      |
|  [02]   | `h3_cell_to_latlng(h3index) -> point`            | function | cell centroid                         |
|  [03]   | `h3_cell_to_boundary(h3index) -> polygon`        | function | cell boundary ring                    |
|  [04]   | `h3_get_resolution(h3index) -> integer`          | function | resolution of the cell                |
|  [05]   | `h3_get_base_cell_number(h3index) -> integer`    | function | base cell `0..121`                    |
|  [06]   | `h3_is_valid_cell(h3index) -> boolean`           | function | cell validity                         |
|  [07]   | `h3_is_pentagon(h3index) -> boolean`             | function | pentagonal cell test                  |
|  [08]   | `h3_is_res_class_iii(h3index) -> boolean`        | function | Class-III orientation test            |
|  [09]   | `h3_get_icosahedron_faces(h3index) -> integer[]` | function | icosahedron faces the cell intersects |

- `h3_cell_to_boundary`: `SET h3.extend_antimeridian TO true` extends coordinates across the 180th meridian.

## [04]-[TRAVERSAL]

[TRAVERSAL_ENTRY_SCOPE]: grid traversal and the resolution hierarchy

`k integer` defaults to `1`; `h3_cell_to_parent`, `h3_cell_to_children`, `h3_cell_to_center_child`, and `h3_uncompact_cells` each ship a one-step form with the resolution argument dropped.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `h3_grid_disk(h3index, integer) -> SETOF h3index`          | set      | filled disk within `k`                  |
|  [02]   | `h3_grid_disk_distances(h3index, integer) -> SETOF record` | set      | disk paired with per-cell distance      |
|  [03]   | `h3_grid_ring_unsafe(h3index, integer) -> SETOF h3index`   | set      | hollow ring at `k`                      |
|  [04]   | `h3_grid_path_cells(h3index, h3index) -> SETOF h3index`    | set      | inclusive cell line between two cells   |
|  [05]   | `h3_grid_distance(h3index, h3index) -> bigint`             | function | grid distance between two cells         |
|  [06]   | `h3_cell_to_local_ij(h3index, h3index) -> point`           | function | origin-anchored local IJ coordinates    |
|  [07]   | `h3_local_ij_to_cell(h3index, point) -> h3index`           | function | local IJ coordinates back to a cell     |
|  [08]   | `h3_cell_to_parent(h3index, integer) -> h3index`           | function | coarser ancestor cell                   |
|  [09]   | `h3_cell_to_children(h3index, integer) -> SETOF h3index`   | set      | finer child cells                       |
|  [10]   | `h3_cell_to_center_child(h3index, integer) -> h3index`     | function | center child at a finer resolution      |
|  [11]   | `h3_cell_to_child_pos(h3index, integer) -> int8`           | function | child ordinal under a parent resolution |
|  [12]   | `h3_child_pos_to_cell(int8, h3index, int) -> h3index`      | function | ordinal back to its child cell          |
|  [13]   | `h3_compact_cells(h3index[]) -> SETOF h3index`             | set      | minimal mixed-resolution cover          |
|  [14]   | `h3_uncompact_cells(h3index[], integer) -> SETOF h3index`  | set      | expand a cover to a uniform resolution  |

- `h3_cell_to_children_slow`: same children at lower peak allocation.
- `h3_grid_path_cells_recursive`: `h3_postgis` recursion over the same two cells.

## [05]-[EDGE_VERTEX_METRIC]

[EDGE_VERTEX_METRIC_ENTRY_SCOPE]: directed-edge topology, cell vertices, and grid metrics — bare rosters, every member keyed on `h3index` unless its name says otherwise

[EDGE]: `h3_are_neighbor_cells` `h3_cells_to_directed_edge` `h3_is_valid_directed_edge` `h3_get_directed_edge_origin` `h3_get_directed_edge_destination` `h3_directed_edge_to_cells` `h3_origin_to_directed_edges` `h3_directed_edge_to_boundary`
[VERTEX]: `h3_cell_to_vertex` `h3_cell_to_vertexes` `h3_vertex_to_latlng` `h3_is_valid_vertex`
[METRIC]: `h3_cell_area` `h3_edge_length` `h3_great_circle_distance` `h3_get_hexagon_area_avg` `h3_get_hexagon_edge_length_avg` `h3_get_num_cells` `h3_get_res_0_cells` `h3_get_pentagons`

Each metric carries a trailing `unit text` argument defaulting to `km` or `km^2`; `h3_get_extension_version()` reports the installed extension build.

## [06]-[POSTGIS_BRIDGE]

[BRIDGE_ENTRY_SCOPE]: region fill and the `h3_postgis` geometry, geography, EWKB, and raster legs

Core region functions take a native PG exterior `polygon` with a `polygon[]` hole array; bridge overloads take PostGIS values at SRID 4326, and every listed surface returns `SETOF h3index` unless the row shows otherwise. A `geometry` argument or `_geometry` suffix pairs with a `geography` twin under the same name. `containment_mode` admits `center`, `full`, `overlapping`, `overlapping_bbox`. PostgreSQL carries no native multipolygon type, so the WKB leg returns EWKB `bytea`.

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `h3_polygon_to_cells(polygon, polygon[], integer)`                    | set      | native polygon to a covering cell set     |
|  [02]   | `h3_polygon_to_cells_experimental(polygon, polygon[], integer, text)` | set      | covering under a containment mode         |
|  [03]   | `h3_cells_to_multi_polygon(h3index[]) -> SETOF record`                | set      | cell set to exterior and hole loops       |
|  [04]   | `h3_latlng_to_cell(geometry, integer) -> h3index`                     | function | SRID-4326 point to a cell                 |
|  [05]   | `h3_polygon_to_cells(geometry, integer)`                              | set      | PostGIS polygon to a covering cell set    |
|  [06]   | `h3_polygon_to_cells_experimental(geometry, integer, text)`           | set      | PostGIS covering under a containment mode |
|  [07]   | `h3_cell_to_geometry(h3index) -> geometry`                            | function | cell centroid as PostGIS                  |
|  [08]   | `h3_cell_to_boundary_geometry(h3index) -> geometry`                   | function | cell boundary, split at the antimeridian  |
|  [09]   | `h3_cells_to_multi_polygon_geometry(h3index[]) -> geometry`           | function | cell set to one multipolygon              |
|  [10]   | `h3_cell_to_boundary_wkb(h3index) -> bytea`                           | function | boundary as EWKB                          |
|  [11]   | `h3_cells_to_multi_polygon_wkb(h3index[]) -> bytea`                   | function | cell set to a multipolygon as EWKB        |
|  [12]   | `h3_get_resolution_from_tile_zoom(integer) -> integer`                | function | resolution for an XYZ tile zoom level     |

- `h3_cells_to_multi_polygon_geometry`: a second form over `setof h3index` is an aggregate finaliser, so a `GROUP BY` rolls a cell column into one multipolygon with no prior `array_agg`.

[RASTER_ENTRY_SCOPE]: per-cell raster summarization over `postgis_raster`, each summary returning `TABLE (h3 h3index, …)` for one band; `_clip`, `_centroids`, and `_subpixel` name the explicit strategies and the bare form selects one by pixels-per-cell

[CONTINUOUS]: `h3_raster_summary` `h3_raster_summary_clip` `h3_raster_summary_centroids` `h3_raster_summary_subpixel` `h3_raster_summary_stats_agg`
[DISCRETE]: `h3_raster_class_summary` `h3_raster_class_summary_clip` `h3_raster_class_summary_centroids` `h3_raster_class_summary_subpixel` `h3_raster_class_summary_item_agg` `h3_raster_class_summary_item_to_jsonb`

## [07]-[INDEX_SUPPORT]

[INDEX_SUPPORT_TYPE_SCOPE]: operator classes over the four built-in access methods

`h3-pg` registers no custom access method. A bare `CREATE INDEX ON t (cell)` picks the default btree class, whose ordering is raw 64-bit, so spatial locality comes from the H3 hierarchy rather than the index sort.

| [INDEX] | [OPCLASS]                  | [AM]   | [DEFAULT] | [CAPABILITY]                                                     |
| :-----: | :------------------------- | :----- | :-------: | :--------------------------------------------------------------- |
|  [01]   | `h3index_ops`              | btree  |    yes    | exact cell, range, `ORDER BY` (`h3index_sortsupport`), equi-join |
|  [02]   | `h3index_ops`              | hash   |    yes    | `=` and `IN` membership, hash partitioning                       |
|  [03]   | `h3index_minmax_ops`       | brin   |    yes    | append-mostly, cell-clustered tables                             |
|  [04]   | `h3index_ops_experimental` | spgist |    no     | hierarchical `=`, `@>`, `<@`; named explicitly at index DDL      |

## [08]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ServerExtension.H3Pg` carries `ExtensionAdmission.Standalone` and `ServerExtension.H3Postgis` carries `ExtensionAdmission.BaseType("h3")`, so `CreateSql` emits `CREATE EXTENSION IF NOT EXISTS … CASCADE` and one statement pulls `postgis` and `postgis_raster`; neither row yields a `PreloadLibrary`, keeping the pair off the `ClusterSetting` `shared_preload_libraries` value.
- Every cell function executes as raw SQL through `Npgsql` with no EF translator, and a `SETOF record` function carries its column-definition list at the call site.
- `h3_latlng_to_cell` is the canonical indexing spelling; a non-4326 input errors inside `h3_polygon_to_cells` and returns an invalid cell from `h3_latlng_to_cell`.

[STACKING]:
- `api-h3.md` (`pocketken.H3`): `H3Index.FromPoint(Point, res)` inside `IdentityStore.Cell` mints the identical id `h3_latlng_to_cell` computes server-side, so the `ElementIdentity.Cell` and `NodeCell.Cell` `bigint` columns key the in-database index directly; `IdentityStore.Nearby` folds `GridDiskDistancesSafe` into the same disk `h3_grid_disk` yields, so either side of the seam answers one membership predicate.
- `api-postgis.md` + `api-npgsql-nts.md`: the bridge consumes and returns the SRID-4326 `geometry`/`geography` values PostGIS owns and the `UseNetTopologySuite` codec round-trips, so a cell boundary reaches managed code as an NTS `Geometry` with no WKT hop, and `h3_cell_to_boundary_wkb` serves the EWKB leg the missing multipolygon type forces.
- Within-lib composition: `h3_polygon_to_cells` into `h3_compact_cells` yields the minimal mixed-resolution cover an `= ANY` test answers under `h3index_ops` btree or `h3index_minmax_ops` brin; `h3_cells_to_multi_polygon_geometry` closes the round trip as an aggregate finaliser over the same cell column; `<->` with `h3index_sortsupport` drives a cell-ordered scan; and the raster leg summarizes a band per cell in one `GROUP BY`.

[LOCAL_ADMISSION]:
- `Query/lane` lowers the `SetPredicate.Cell` leaf to `h3_grid_disk(anchor, k)` membership over the identity tier's cell column under the admitted `WalkDepth` ring; the `Spatial` leaf keeps the PostGIS GiST plane, and the two planes stay disjoint.
- Durable keying rides the immutable `bigint` cell id on `ElementIdentity` and `NodeCell`, minted through the managed entry and railed as `IdentityFault.CellUnresolvable` when a centroid decodes invalid.

[RAIL_LAW]:
- Package: `h3-pg` — extensions `h3` and `h3_postgis`, server-side in the deploy-image PostgreSQL
- Owns: in-database Uber-H3 cell indexing — the `h3index` type and its operator algebra, the indexing, inspection, traversal, hierarchy, edge, vertex, and metric function surface, the `h3_postgis` geometry, geography, EWKB, and raster bridge, and the btree, hash, brin, and spgist operator classes
- Accept: the `CreateSql` CASCADE install, `h3_latlng_to_cell` at every call site, SRID-4326 PostGIS inputs, an `h3index` opclass index on a cell column, `h3_grid_disk` and `h3_polygon_to_cells` cell-set membership ahead of a geometry refine, ingest parity with the managed `pocketken.H3` pin
- Reject: linking the extension into managed code, a `shared_preload_libraries` placement, a per-row `ST_DWithin` scan where a cell-set membership test serves, a second managed coordinate model beside NTS and H3, a stored mutable cell instance beside the durable `bigint`
