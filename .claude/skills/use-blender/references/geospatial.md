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

- `crs` set after `lon` and `lat` reprojects the origin into `crs x` and `crs y`, `lon` and `lat` range-check and raise outside their bounds
- Reprojection runs offline between WGS84 and Web Mercator (`EPSG:3857`) or UTM, `pyproj` and GDAL are absent
- Other CRS route through the MapTiler web service, a failed reprojection deletes `crs x` and `crs y` and logs a warning with no raise
- Imports refuse a half-set georeference with `Scene georef is broken, please fix it beforehand`, `geo.delOrigin()` clears it
- IFC models hold their georeference in the IFC file, `scene.BIMGeoreferenceProperties` shows it with a Blender offset (`blender_offset_x`)

## [02]-[GIS_DATA]

- BlenderGIS importers sit under `bpy.ops.importgis`, `discover("importgis")` lists them with their parameters
- `importgis.asc_file(filepath=)` builds a DEM mesh from an ASCII grid on the scene georeference
- Each BlenderGIS import rewrites the 3D view's grid and clip distances while its `adjust3Dview` preference is on
- SRTM downloads need `opentopography_api_key` in the add-on preferences
- Use `references/interchange.md` for city models

## [03]-[SUN_AND_CLIMATE]

Sun direction comes from `scene.sun_pos_properties` or `ladybug`, both agree to 0.01 degrees:
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
- `Sunpath.from_location(Location(latitude=, longitude=, time_zone=))` from `ladybug.sunpath` and `ladybug.location` builds a sun path
- `calculate_sun(month, day, hour)` returns `altitude` and `azimuth` in degrees and touches no scene state

Analysis operators read an EPW weather file and `scene.ladybug`, headless included:
1. `bpy.ops.ladybug.load_epw(filepath=<epw>)` sets `scene.ladybug` city, latitude, longitude, standard time zone, and elevation over earlier values
2. Set the analysis period (`ap_st_month`, `ap_st_day`, `ap_end_month`, `ap_end_day`) on `scene.ladybug`
3. Select target meshes, `ladybug.sensor_grid()` subdivides them into analysis cells
4. `ladybug.direct_sun_hours()` or `ladybug.incident_radiation()` adds a `<target> · <study>` result mesh with a legend beside it

- Result meshes hold one value per analysis cell in a float attribute (`LB Sun Hours`) beside `LB Color`, a script reads the float
- Without `sensor_grid` each target face is one cell
- `scene.ladybug.st_context` default `SELECTED` shades the study with other selected meshes alone, `VISIBLE` with every visible mesh
- `scene.ladybug.north` is degrees counter-clockwise from +Y to project north, `time_zone` is standard time with no daylight field
- EPW weather files download per station from climate.onebuilding.org
