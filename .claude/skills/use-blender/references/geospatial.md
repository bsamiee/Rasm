# [GEOSPATIAL]

Site models sit near the world origin with the map position stored as a scene georeference, object locations and vertices are float32 and lose centimeters at UTM magnitude.

## [01]-[GEOREFERENCE]

BlenderGIS keeps the scene georeference in scene properties (`SRID`, `latitude`, `longitude`, `crs x`, `crs y`) through its `GeoScene` class:

```python
# [EXECUTE_BLENDER_CODE] Scene origin at a longitude and latitude in a UTM or Web Mercator CRS
import importlib

import bpy

package = getattr(bpy.types, bpy.ops.importgis.asc_file.idname()).__module__.split(".")[0]
GeoScene = importlib.import_module(f"{package}.geoscene").GeoScene
geo = GeoScene(bpy.context.scene)
geo.crs = "EPSG:<code>"
geo.setOriginGeo(<longitude>, <latitude>)
result = {"crs": geo.crs, "origin_crs": [geo.crsx, geo.crsy], "georeferenced": geo.isGeoref}
```

- Sun Position and Bonsai solar take the same site and moment
- New sites start with `geo.delOrigin()` and the georeference snippet, then the Sun Position and Bonsai solar fields
- `crs` set after `lon` and `lat` reprojects the origin into `crs x` and `crs y`, `lon` and `lat` range-check and raise outside their bounds
- Reprojection runs offline between WGS84 and Web Mercator (`EPSG:3857`) or UTM, `pyproj` and GDAL are absent
- Other CRS route through the MapTiler web service, a failed reprojection deletes `crs x` and `crs y` and logs a warning with no raise
- Imports refuse a half-set georeference with `Scene georef is broken, please fix it beforehand`, `geo.delOrigin()` clears it
- IFC models hold their georeference in the IFC file, `scene.BIMGeoreferenceProperties` shows it with a Blender offset (`blender_offset_x`)
- BlenderGIS turns off HTTPS certificate checks for the whole Python process unless `PYTHONHTTPSVERIFY` is set
- BlenderGIS replaces `sys.excepthook` and `threading.Thread.__init__` at import
- First registration downloads FreeImage from GitHub into `~/Library/Application Support/imageio` for the basemap, `IMAGEIO_NO_INTERNET=1` stops it

## [02]-[GIS_DATA]

- BlenderGIS importers sit under `bpy.ops.importgis`, `discover("importgis")` lists them with their parameters
- `importgis.asc_file(filepath=)` builds a DEM mesh from an ASCII grid on the scene georeference
- Raster, SHP, and ASC imports rewrite every 3D view's grid and clip distances while `adjust3Dview` is on
- `importgis.osm_query`, `osm_file`, and `dem_query` set every 3D view's `clip_start` to 1, 10, or 100 m whatever `adjust3Dview` holds
- `importgis.dem_query` downloads from the `demServer` preference template, an OpenTopography server needs `opentopography_api_key`
- `dem_query` and `osm_query` take their extent from one selected mesh, headless under a stored window, `active_object`, and `selected_objects`
- DEM heights are orthometric meters, so `location.z = -<site elevation>` puts the ground at the model's zero
- `view3d.map_start` refuses every scene CRS but Web Mercator without GDAL
- `importgis.georaster(filepath=, importMode="MESH", objectsLst=<DEM index>)` drapes a GeoTIFF onto a DEM mesh
- GIS imports link their objects to the scene collection, `osm_query` with `separate` builds one OSM collection with a child per tag
- Use `references/interchange.md` for city models

## [03]-[SUN_AND_CLIMATE]

Sun direction comes from `scene.sun_pos_properties`:
- Sun Position fields `latitude`, `longitude`, `UTC_zone`, `year`, `month`, `day`, and `time` set the moment
- Every field write recomputes and moves the lamp and sky, `sun_elevation` and `sun_azimuth` read the result in radians
- `sun_pos_properties.sun_object` names a Sun light, `sun_distance` places it at that distance along the sun vector, 0 at the origin
- `sky_texture` naming a Sky Texture node beside `sun_object` moves lamp and sky together, both set before the time and place fields
- Sky node writes reset `texture_mapping.rotation.z` to 0, and a `sky_texture` naming no `TEX_SKY` node skips the sky
- `UTC_zone` is the standard offset, `use_daylight_savings` adds the daylight hour, and `north_offset` is radians
- `sun_position.sun_calc.get_sun_coordinates(local_time, latitude, longitude, zone, month, day, year)` answers azimuth and elevation outside the scene
- `calculate_sun` hours are standard time like EPW files, 12:00 daylight time is 11.0
- Bonsai solar fields written through RNA overwrite the Sun Position fields and raise with no IFC project loaded, ID item writes land in float32
- Bonsai `true_north` carries the IFC sign, Sun Position receives it as `north_offset = -true_north`
- EPW files per station download from climate.onebuilding.org
