# [RASM_PERSISTENCE_API_H3_PG]

`h3-pg` supplies Uber-H3 v4 hexagonal hierarchical geospatial indexing inside PostgreSQL — the
`h3index` 64-bit cell type and the `h3_*` function surface (indexing, inspection, hierarchy, traversal,
region), plus the `h3_postgis` bridge extension projecting cells to and from PostGIS `geometry`/
`geography`. It carries no managed assembly: every surface is server-side SQL the
`Store/provisioning#SERVER_EXTENSIONS` `ServerExtension("h3")` / `ServerExtension("h3_postgis", Cascade: true)`
rows install and the `Query/lane#GEO_LANES` spatial consumer drives through raw `Npgsql`/`FromSql`/
`SqlQuery`. The extension pair is preload-free — no `shared_preload_libraries` row and no custom index
access method (it ships operator classes for the built-in btree/hash/brin/spgist AMs) — so it is
correctly absent from the `Store/provisioning#SERVER_EXTENSIONS` preload value. The cell id is the SAME
64-bit value the managed `pocketken.H3` pin (`api-h3.md`) computes at ingest, so a row's `h3_cell`
generated column and the managed `H3Index.FromPoint(point, res)` agree bit-for-bit — the dual admission
makes in-process and in-database indexing one cell vocabulary, never a second cell id.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `h3-pg` / extensions `h3` + `h3_postgis`
- package: server-side PostgreSQL extension (C, not a NuGet package); repo `zachasme/h3-pg` (canonical home `postgis/h3-pg`), version `4.2.3` (binds H3 core v4.2.0)
- namespace: SQL `public` (the `h3index` type, the `h3_*` functions, the `h3_postgis` geometry/geography bridge, the operator/cast set)
- registration: preload-free — `CREATE EXTENSION h3` (core, zero extension deps) and `CREATE EXTENSION h3_postgis CASCADE` (`requires = h3, postgis, postgis_raster`); both `RELOCATABLE`, no `shared_preload_libraries` row, no custom access method; the `ServerExtension("h3", PreloadGated: false)` + `ServerExtension("h3_postgis", Cascade: true, PreloadGated: false)` rows carry the install
- license: Apache-2.0 — the in-DB deployment is the license boundary, no managed linkage
- consumed by: `Query/lane#GEO_LANES` `H3Cell` axis (the `h3_cell` generated column + `h3_cell = ANY(@cells)` membership prefilter), driven through raw `Npgsql`; the in-process counterpart is `pocketken.H3` (`api-h3.md`)
- rail: geospatial-index, geo-provisioning

## [02]-[H3INDEX_TYPE]

`h3index` is an unsigned 64-bit integer naming any H3 object (cell, pentagon, directed edge, vertex). Its
text representation is a 15- or 16-character lowercase hex string (`'8928308280fffff'`); it is declared
`LIKE int8` so storage and pass-by-value are identical to `bigint`. A literal lands through the input
function (`'8928308280fffff'::h3index`); the numeric round-trip rides the bigint casts.

| [INDEX] | [SURFACE]                  | [SIGNATURE]                                  | [SEMANTICS]                                  |
| :-----: | :------------------------- | :------------------------------------------- | :------------------------------------------- |
|  [01]   | `h3index`                  | `CREATE TYPE h3index (LIKE = int8, …)`       | 64-bit cell id; hex-string text I/O, binary `RECEIVE`/`SEND` |
|  [02]   | `h3index <-> h3index`      | → `bigint` (`h3index_distance`)              | grid distance (KNN-style; works across resolutions via center child) |
|  [03]   | `h3index = / <>`           | → `boolean` (`h3index_eq` `HASHES, MERGES` / `h3index_ne`) | equality / inequality                     |
|  [04]   | `h3index && / @> / <@`     | → `boolean` (`h3index_overlaps`/`contains`/`contained_by`) | intersects / contains / contained-by (hierarchical) |
|  [05]   | `h3index < <= > >=`        | → `boolean` (raw 64-bit ordering)            | btree-support comparison                      |
|  [06]   | casts                      | `h3index::bigint` (decimal), `bigint::h3index`, `h3index::point` (centroid) | scalar interconversion; `h3_postgis` adds `h3index::geometry`/`::geography` |
|  [07]   | `geometry @ integer` / `geography @ integer` | → `h3index` (`h3_postgis`)     | shorthand for `h3_latlng_to_cell`            |

## [03]-[INDEXING]

The core indexing and inspection functions (h3 extension). H3 v4.2.3 renamed the `lat_lng` token to
`latlng`, so `h3_latlng_to_cell`/`h3_cell_to_latlng` are the canonical spellings and `h3_lat_lng_to_cell`/
`h3_cell_to_lat_lng` survive only as deprecated aliases. The native overload takes a PG `point`
(`POINT(lat, lng)` per the H3 convention); the `h3_postgis` `geometry`/`geography` overloads use standard
SRID-4326 `(lng, lat)` and cast internally — distinct conventions, the SAME resulting cell.

| [INDEX] | [FUNCTION]                  | [SIGNATURE]                                                  | [SEMANTICS]                          |
| :-----: | :-------------------------- | :---------------------------------------------------------- | :----------------------------------- |
|  [01]   | `h3_latlng_to_cell`         | `h3_latlng_to_cell(latlng point, resolution integer)` → `h3index` | index a location at resolution 0..15 |
|  [02]   | `h3_cell_to_latlng`         | `h3_cell_to_latlng(cell h3index)` → `point`                 | cell centroid                        |
|  [03]   | `h3_cell_to_boundary`       | `h3_cell_to_boundary(cell h3index)` → `polygon`             | cell boundary (antimeridian via `SET h3.extend_antimeridian`) |
|  [04]   | `h3_get_resolution`         | `h3_get_resolution(h3index)` → `integer`                    | resolution 0..15                     |
|  [05]   | `h3_get_base_cell_number`   | `h3_get_base_cell_number(h3index)` → `integer`              | base cell 0..121                     |
|  [06]   | `h3_is_valid_cell`          | `h3_is_valid_cell(h3index)` → `boolean`                     | validity test                        |
|  [07]   | `h3_is_pentagon` / `h3_is_res_class_iii` | `h3_is_pentagon(h3index)` → `boolean`          | pentagon / Class-III orientation     |
|  [08]   | `h3_get_icosahedron_faces`  | `h3_get_icosahedron_faces(h3index)` → `integer[]`           | icosahedron faces the cell intersects |

## [04]-[HIERARCHY]

Hierarchy and grid-traversal (h3 extension). Parent/children/center-child/uncompact each ship a
no-resolution one-step form alongside the explicit-resolution form.

| [INDEX] | [FUNCTION]                | [SIGNATURE]                                                           | [SEMANTICS]                          |
| :-----: | :------------------------ | :------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `h3_grid_disk`            | `h3_grid_disk(origin h3index, k integer => 1)` → `SETOF h3index`     | filled disk within distance `k`      |
|  [02]   | `h3_grid_disk_distances`  | `h3_grid_disk_distances(origin h3index, k integer => 1, OUT index h3index, OUT distance int)` → `SETOF record` | disk with per-cell distance |
|  [03]   | `h3_grid_ring_unsafe`     | `h3_grid_ring_unsafe(origin h3index, k integer => 1)` → `SETOF h3index` | hollow ring at distance `k`         |
|  [04]   | `h3_grid_path_cells`      | `h3_grid_path_cells(origin h3index, destination h3index)` → `SETOF h3index` | straight-line cell path             |
|  [05]   | `h3_grid_distance`        | `h3_grid_distance(origin h3index, destination h3index)` → `bigint`   | grid distance between two cells      |
|  [06]   | `h3_cell_to_parent`       | `h3_cell_to_parent(cell h3index, resolution integer)` → `h3index` (1-arg = one step coarser) | coarser ancestor cell |
|  [07]   | `h3_cell_to_children`     | `h3_cell_to_children(cell h3index, resolution integer)` → `SETOF h3index` (1-arg = one step finer) | finer child cells |
|  [08]   | `h3_cell_to_center_child` | `h3_cell_to_center_child(cell h3index, resolution integer)` → `h3index` | center child at finer resolution    |
|  [09]   | `h3_compact_cells` / `h3_uncompact_cells` | `h3_compact_cells(cells h3index[])` → `SETOF h3index` / `h3_uncompact_cells(cells h3index[], resolution integer)` → `SETOF h3index` | minimal mixed-resolution cover / expand to uniform resolution |

## [05]-[REGION_BRIDGE]

Region fill and the `h3_postgis` geometry bridge. The core region API uses native PG `polygon`/
`polygon[]` (exterior ring + hole array); the `h3_postgis` variants take/return PostGIS `geometry`/
`geography` (SRID 4326 required) and EWKB `bytea` (PG has no native multipolygon).

| [INDEX] | [FUNCTION]                          | [SIGNATURE]                                                                 | [SEMANTICS]                          |
| :-----: | :---------------------------------- | :------------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `h3_polygon_to_cells`               | `h3_polygon_to_cells(exterior polygon, holes polygon[], resolution integer => 1)` → `SETOF h3index` | native polygon → covering cells      |
|  [02]   | `h3_polygon_to_cells_experimental`  | `h3_polygon_to_cells_experimental(exterior polygon, holes polygon[], resolution integer => 1, containment_mode text => 'center')` → `SETOF h3index` | `center`/`full`/`overlapping`/`overlapping_bbox` containment |
|  [03]   | `h3_cells_to_multi_polygon`         | `h3_cells_to_multi_polygon(h3index[], OUT exterior polygon, OUT holes polygon[])` → `SETOF record` | cell set → multipolygon loops        |
|  [04]   | `h3_latlng_to_cell` (`h3_postgis`)  | `h3_latlng_to_cell(geometry, resolution integer)` / `(geography, resolution integer)` → `h3index` | SRID-4326 point → cell (casts to `point`) |
|  [05]   | `h3_cell_to_geometry` / `h3_cell_to_geography` | `h3_cell_to_geometry(h3index)` → `geometry` / `geography`        | cell centroid as PostGIS geometry/geography |
|  [06]   | `h3_cell_to_boundary_geometry` / `…_geography` | `h3_cell_to_boundary_geometry(h3index)` → `geometry` / `geography` | cell boundary as PostGIS geometry/geography (splits at 180°) |
|  [07]   | `h3_polygon_to_cells` (`h3_postgis`) | `h3_polygon_to_cells(multi geometry, resolution integer)` / `(multi geography, resolution integer)` → `SETOF h3index` | PostGIS polygon → covering cells     |
|  [08]   | `h3_cells_to_multi_polygon_geometry` / `…_geography` / `h3_cell_to_boundary_wkb` / `h3_cells_to_multi_polygon_wkb` | → `geometry` / `geography` / `bytea` / `bytea` | cell set → multipolygon geometry / EWKB |

`h3_cells_to_multi_polygon_geometry`/`_geography` exist BOTH as array functions (`h3index[]`) AND as
`CREATE AGGREGATE` finalisers over a single `h3index` column, so a `GROUP BY` region rolls a cell column
straight into one multipolygon (`SELECT region, h3_cells_to_multi_polygon_geometry(h3_cell) … GROUP BY
region`) without a prior `array_agg`. The `h3_postgis` bridge additionally ships a raster leg
(`h3_raster_*`) and `h3_get_resolution_from_tile_zoom(integer)` for XYZ-tile resolution selection.

## [06]-[INDEX_SUPPORT]

`h3-pg` ships operator classes for the four built-in index access methods over the `h3index` type — no
custom access method. A plain `CREATE INDEX ON tbl (h3col)` picks the DEFAULT btree opclass; the ordering
is index-numeric (raw 64-bit) not spatially contiguous, so spatial locality is supplied by the H3
hierarchy, not the index sort.

| [INDEX] | [OPCLASS]                    | [AM]   | [DEFAULT] | [SEMANTICS]                                                  |
| :-----: | :--------------------------- | :----- | :-------- | :--------------------------------------------------------- |
|  [01]   | `h3index_ops`                | btree  | yes       | `=` exact cell, range, `ORDER BY` (`h3index_sortsupport`), equi-join |
|  [02]   | `h3index_ops`                | hash   | yes       | `=` / `IN` membership, hash partitioning                   |
|  [03]   | `h3index_minmax_ops`         | brin   | yes       | append-mostly, cell-clustered tables                       |
|  [04]   | `h3index_ops_experimental`   | spgist | no        | hierarchical containment `=` / `@>` / `<@` (must be named explicitly) |

## [07]-[IMPLEMENTATION_LAW]

[H3PG_TOPOLOGY]:
- Two preload-free extensions: `h3` (core, zero extension deps) installs via `ServerExtension("h3", PreloadGated: false)`, and `h3_postgis` installs via `ServerExtension("h3_postgis", Cascade: true)` emitting `CREATE EXTENSION IF NOT EXISTS h3_postgis CASCADE` (`requires = h3, postgis, postgis_raster`), pulling all three prerequisites in one DDL step. Neither registers a background worker, planner hook, or custom access method (only built-in-AM operator classes), so both are correctly absent from the `Store/provisioning#SERVER_EXTENSIONS` `shared_preload_libraries` row.
- v4 naming: `h3_latlng_to_cell`/`h3_cell_to_latlng` are the canonical 4.2.3 spellings; `h3_lat_lng_to_cell`/`h3_cell_to_lat_lng` are the deprecated pre-4.2.3 aliases some cross-referencing design pages still name. Pin the `latlng` spelling at new call sites. The native `point` overload is `(lat, lng)`; the `h3_postgis` `geometry`/`geography` overloads are SRID-4326 `(lng, lat)` and cast internally — both yield the same cell.
- No managed assembly, no EF translator: every cell function rides raw `Npgsql`/`FromSql`/`SqlQuery`; the `h3index` column stores as the bigint-backed cell id, and a `SETOF record` function (`h3_grid_disk_distances`, `h3_cells_to_multi_polygon`) requires the column-definition list. PostGIS inputs must be SRID 4326 — a non-4326 geometry is the rejected form.

[GEO_LANE_STACK]:
- Ingest parity with `pocketken.H3`: the managed `H3Index.FromPoint(point, res)` (`api-h3.md`) computes the identical 64-bit cell `h3_latlng_to_cell` computes server-side over the same NTS `Point` the `Npgsql.NetTopologySuite` plugin binds, so the `h3_cell` generated column (DDL on `Store/provisioning#SERVER_EXTENSIONS`) and the managed cell agree bit-for-bit — the dual admission exists to make in-process and in-database indexing one vocabulary, never a second cell id.
- Spatial prefilter: a radius/region query lowers through the managed `H3CellOps.Cover`/`Disk` (`Query/lane#GEO_LANES`) — or in-database `h3_grid_disk`/`h3_polygon_to_cells` — to a cell set the `h3_cell = ANY(@cells)` btree/brin membership test answers, with `h3_compact_cells` collapsing the cover to the densest mixed-resolution key, ahead of the managed `Geometry` refine rather than a per-row `ST_DWithin` scan.

[RAIL_LAW]:
- Package: `h3-pg` / extensions `h3` + `h3_postgis` (server-side, in the deploy-image PG18)
- Owns: in-PG Uber-H3 v4 cell indexing — the `h3index` type, the indexing/inspection/hierarchy/traversal/region functions, the `h3_postgis` geometry/geography bridge, and the btree/hash/brin/spgist operator classes
- Accept: `CREATE EXTENSION h3` + `CREATE EXTENSION h3_postgis CASCADE`, the `h3_latlng_to_cell` canonical spelling, an `h3index` btree/hash/brin opclass index on a cell column, SRID-4326 PostGIS inputs, the `h3_cell = ANY(@cells)` membership prefilter, ingest parity with the managed `pocketken.H3` pin
- Reject: linking the extension into managed code, placing `h3`/`h3_postgis` on the `shared_preload_libraries` row (preload-free), the deprecated `h3_lat_lng_to_cell` spelling at a new call site, a non-4326 SRID PostGIS input, a second managed coordinate model beside NTS/H3, a per-row `ST_DWithin` scan where a cell-set membership test serves, a stored live mutable cell instance (the immutable `h3index`/`ulong` is the durable key)
