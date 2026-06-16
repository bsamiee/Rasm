# [PY_DATA_SCHEMA_GEO_AEC]

Schema, geospatial, and AEC ownership turns files and frames into explicit claims. Claims travel with exchange bundles and fail closed when schema, CRS, units, or host-boundary assumptions do not match.

## [1]-[SCHEMA_OWNER]

[SCHEMA_CLAIM]:
- Owns: field names, logical types, required fields, nullable policy, and source schema evidence.
- API routes: `.api/api-pyarrow.md`, `.api/api-polars.md`, `.api/api-pandas.md`.
- Output: schema claim record and failed-claim receipt.
- Boundary: app schema, migrations, retention, and store constraints remain Persistence-owned.

[FRAME_ADMISSION]:
- Owns: frame admission into schema claims without committing to one dataframe provider.
- Cases: Arrow table, Polars frame, Pandas frame, Xarray dataset.
- Boundary: frame admission never creates a durable product model.

## [2]-[GEO_OWNER]

[GEO_CLAIM]:
- Owns: CRS, units, axis order, geometry family, precision policy, and transform provenance.
- API routes: `.api/api-geopandas.md`, `.api/api-shapely.md`, `.api/api-pyogrio.md`, `.api/api-pyproj.md`.
- Output: geospatial claim record and transform receipt.
- Boundary: no live Rhino document, GH canvas, viewport, or C# geometry kernel mutation.

[SPATIAL_EGRESS]:
- Owns: GeoJSON, GeoParquet, flat vector files, and CRS-normalized bundles.
- Packages: `geopandas`, `pyogrio`, `pyarrow`, `pyproj`.
- Output: exchange bundle with schema and geo claims.
- Boundary: no product geospatial store writes.

## [3]-[AEC_OWNER]

[THREEDM_EXCHANGE]:
- Owns: `.3dm` file inspection, object-table extraction, layer/material metadata, and file-level geometry references.
- API route: `.api/api-rhino3dm.md`.
- Output: AEC exchange bundle and digest route.
- Boundary: `rhino3dm` is file exchange only; live Rhino, bridge launch, package staging, and host verification remain bridge/C# owned.

## [4]-[RED_TEAM]

- Reject CRS transforms without a `GeoClaim` receipt.
- Reject required-field checks that live outside `SchemaClaim`.
- Reject AEC exchange that mutates host documents or claims live Rhino state.
- Reject a shadow geometry kernel built from Shapely, Rhino3dm, Trimesh, or Meshio.
