# [GEOSPATIAL]

Site models near the world origin with the map position stored as a scene georeference.

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

- Object locations and vertices are float32 and lose centimeters at UTM magnitude
- New sites clear the origin with `geo.delOrigin()` before setting it, then set the Sun Position and Bonsai solar fields
- `crs` set after `lon` and `lat` reprojects the origin into `crs x` and `crs y`, `lon` and `lat` range-check and raise outside their bounds
- Reprojection runs offline for every EPSG code through the `pyproj` in BlenderGIS's `libraries` folder, GDAL is absent
- Failed reprojections in `setOriginGeo` delete `crs x` and `crs y` and log a warning with no raise
- Imports refuse a half-set georeference with `Scene georef is broken, please fix it beforehand`, `geo.delOrigin()` clears it
- IFC models hold their georeference in the IFC file, `scene.BIMGeoreferenceProperties` shows it with a Blender offset (`blender_offset_x`)
- BlenderGIS replaces `sys.excepthook` and `threading.Thread.__init__` at import
- First registration downloads FreeImage from GitHub into `~/Library/Application Support/imageio` for the basemap, `IMAGEIO_NO_INTERNET=1` stops it

## [02]-[GIS_DATA]

- BlenderGIS importers sit under `bpy.ops.importgis`
- Raster, SHP, and ASC imports set clip distances on the current screen's 3D views and frame the result while `adjust3Dview` is on
- `importgis.osm_query`, `osm_file`, and `dem_query` set `clip_start` to 1, 10, or 100 m on the current screen's 3D views while `adjust3Dview` is on
- `importgis.dem_query` downloads from the `demServer` preference, an OpenTopography server needs `opentopography_api_key`
- `dem_query` pads the extent 0.002° and writes `srtm.tif` beside the `.blend` or in `bpy.app.tempdir`
- `dem_query` and `osm_query` take their extent from one selected mesh, headless under a stored window, `active_object`, and `selected_objects`
- DEM heights are orthometric meters, `location.z = -<site elevation>` puts the ground at the model's zero
- `view3d.map_start` refuses every scene CRS but Web Mercator without GDAL
- `importgis.georaster(filepath=, importMode="MESH", objectsLst=<DEM index>)` drapes a GeoTIFF onto a DEM mesh
- `.tfw` files beside a TIFF georeference it
- GIS imports link their objects to the scene collection, `separate` on the OSM importers builds one `OSM` collection with one object per feature
- OSM features hold their tags as ID properties
- OSM `filterTags` holds no items in a code call, any value raises `ValueError`

## [03]-[SUN_AND_CLIMATE]

Sun direction comes from `scene.sun_pos_properties`:
- Sun Position fields `latitude`, `longitude`, `UTC_zone`, `year`, `month`, `day`, and `time` set the moment
- Every field write recomputes and moves the lamp and sky
- `sun_elevation` and `sun_azimuth` read the result in radians after a `bpy.rna WARNING ... matches no enum` line
- `sun_pos_properties.sun_object` names a Sun light, `sun_distance` places it at that distance along the sun vector, 0 at the origin
- `sky_texture` naming a Sky Texture node beside `sun_object` moves lamp and sky together, both set before the time and place fields
- Sky node writes reset `texture_mapping.rotation.z` to 0, and a `sky_texture` naming no `TEX_SKY` node skips the sky
- `UTC_zone` is the standard offset, `use_daylight_savings` adds the daylight hour, and `north_offset` is radians
- `bl_ext.blender_org.sun_position.sun_calc` computes sun positions outside the scene
- `sun_calc.get_sun_coordinates(local_time, latitude, longitude, zone, month, day, year)` answers azimuth and elevation in radians
- `zone` is `-UTC_zone`, `-UTC_zone + 1` under daylight saving
- `vi_suite.vi_func.solarPosition(<day of year>, <hour>, <latitude>, <longitude>)` takes EPW local standard time, 12:00 daylight time is 11.0
- Bonsai solar RNA writes overwrite the Sun Position fields under a loaded IFC project, and print a traceback with no change without one
- `BIMSolarProperties.shadow_mode = "SHADING"` switches `render.engine` to `BLENDER_WORKBENCH` under a loaded IFC project
- Bonsai solar ID item writes (`props["latitude"]`) skip every update and store float32
- Bonsai `true_north` holds the IFC sign, Sun Position receives it as `north_offset = -true_north`
- EPW files per station download from climate.onebuilding.org
