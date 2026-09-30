# [GEOSPATIAL]

Site models sit near the world origin, the scene georeference holds their map position, and one sun vector feeds every sun store.

## [01]-[SITE]

`tools/interface/render.py` owns the declared site and moment (`LATITUDE`, `LONGITUDE`, `ELEVATION`, `NORTH`, `MOMENT`, `OFFSET`, `DAYLIGHT`), and files from the user's startup file hold them in BlenderGIS, Sun Position, Bonsai solar, and the `Analysis` scene's VI-Suite fields. Every file starts with one read:

```python
# [EXECUTE_BLENDER_CODE] Georeference, Sun Position moment, and the sun vector from Sun Position, sun_calc, VI-Suite, and the lamp
from datetime import date
import importlib
import math

import addon_utils
import bpy
from bl_ext.blender_org.sun_position import sun_calc
from vi_suite.vi_func import solarPosition

scene = bpy.context.scene
package = {m.bl_info["name"]: m.__name__ for m in addon_utils.modules()}["BlenderGIS"]
geo = importlib.import_module(f"{package}.geoscene").GeoScene(scene)
sun = scene.sun_pos_properties
azimuth, elevation = sun_calc.get_sun_coordinates(sun.time, sun.latitude, sun.longitude, -(sun.UTC_zone + sun.use_daylight_savings), sun.month, sun.day, sun.year)
probe = bpy.data.scenes.new("probe")
probe.vi_params["viparams"] = {}
with bpy.context.temp_override(scene=probe):
    altitude, south_azimuth, *_ = solarPosition(date(sun.year, sun.month, sun.day).timetuple().tm_yday, sun.time - sun.use_daylight_savings, sun.latitude, sun.longitude)
bpy.data.scenes.remove(probe)
result = {
    "georeference": [geo.crs, geo.lon, geo.lat, geo.crsx, geo.crsy, geo.isBroken],
    "moment": [sun.year, sun.month, sun.day, sun.time, sun.UTC_zone, sun.use_daylight_savings, math.degrees(sun.north_offset)],
    "bound": [sun.sun_object and sun.sun_object.name, sun.sky_texture],
    "sun_position": [math.degrees(sun.sun_azimuth), math.degrees(sun.sun_elevation)],
    "sun_calc": [math.degrees(azimuth), math.degrees(elevation)],
    "vi_suite": [south_azimuth - 180, altitude],
    "lamp": sun.sun_object and list(sun.sun_object.matrix_world.col[2].to_3d()),
}
```

- Existing targets hold the declared site and moment in every field, with Sun Position, `sun_calc`, and the lamp's +Z axis agreeing
- Azimuths read clockwise from north, the lamp's +Z axis is the sun vector with +Y north
- `sun_calc` takes the zone as `-(UTC_zone + daylight)` and adds the context scene's `north_offset` to its azimuth
- `solarPosition` takes EPW standard time and a day of year, answers azimuth clockwise from south, and departs up to 0.8° from Sun Position
- `solarPosition` raises `KeyError 'viparams'` in a context scene without a VI-Suite export, the probe scene holds that key
- `use_refraction` False reads the geometric elevation Rhino computes, True the apparent one

Files with no georeference, a broken one, or another site's take the new-site sequence:

```python
# [EXECUTE_BLENDER_CODE] Georeference in the site's UTM zone, sky and sun lamp bound, and the moment on the context scene
from datetime import datetime, timedelta
import importlib
import math
from zoneinfo import ZoneInfo

import addon_utils
import bpy

scene = bpy.context.scene
latitude, longitude, north = <latitude>, <longitude>, <north>
moment = datetime(<year>, <month>, <day>, <hour>, <minute>, tzinfo=ZoneInfo("<zone>"))
daylight = moment.dst() / timedelta(hours=1)
package = {m.bl_info["name"]: m.__name__ for m in addon_utils.modules()}["BlenderGIS"]
geo = importlib.import_module(f"{package}.geoscene").GeoScene(scene)
geo.delOrigin()
geo.crs = f"EPSG:{(32600 if latitude >= 0 else 32700) + int((longitude + 180) // 6) + 1}"
geo.setOriginGeo(longitude, latitude)
scene.world = scene.world or bpy.data.worlds.new("World")
tree = scene.world.node_tree
sky = next((node for node in tree.nodes if node.type == "TEX_SKY"), None) or tree.nodes.new("ShaderNodeTexSky")
tree.links.new(sky.outputs["Color"], next(node for node in tree.nodes if node.type == "BACKGROUND").inputs["Color"])
lamp = next((o for o in scene.objects if o.type == "LIGHT" and o.data.type == "SUN"), None)
if lamp is None:
    lamp = bpy.data.objects.new("Sun", bpy.data.lights.new("Sun", "SUN"))
    scene.collection.objects.link(lamp)
sun = scene.sun_pos_properties
sun.sun_object, sun.sky_texture = lamp, sky.name
sun.latitude, sun.longitude = latitude, longitude
sun.UTC_zone, sun.use_daylight_savings = moment.utcoffset() / timedelta(hours=1) - daylight, bool(daylight)
sun.north_offset = -math.radians(north)
sun.year, sun.month, sun.day, sun.time = moment.year, moment.month, moment.day, moment.hour + moment.minute / 60
result = {"georeference": [geo.crs, geo.crsx, geo.crsy], "sun": [math.degrees(sun.sun_azimuth), math.degrees(sun.sun_elevation)], "vector": list(sky.sun_direction)}
```

- `<north>` is true north in degrees anti-clockwise from +Y, `<zone>` the site's IANA zone, Rhino takes `Sun.North` 90 + `<north>`
- UTM holds scale within 0.04% across a zone, and BlenderGIS projects through its bundled `pyproj` with no network or GDAL
- Object locations and mesh positions are float32, stepping 0.25 m at a UTM northing, so the model stays at the origin in local meters
- `geo.updOriginGeo(<longitude>, <latitude>)` moves a set origin and every root object by the projected delta, the model keeping its map place
- Sun Position fields each re-place lamp and sky, so lamp and sky bind before the place and moment fields and the last write places both
- Sun Position callbacks move the context scene's sun, a write to another scene runs under `temp_override(scene=<scene>)`
- Rhino's `Sun.TimeZone` and Sun Position's `UTC_zone` take the standard offset beside a daylight flag, zoneinfo gives both
- Use look-development.md for sky model, sun disc, and lamp irradiance

## [02]-[BONSAI_SOLAR]

IFC projects enter the open file through `bim.load_project(filepath=, should_start_fresh_session=False)`, which keeps the lamp. `bim.new_project` and `bim.create_project` delete every object, mesh, and material of a scene holding one mesh, one light, and one camera, the startup scene included. Under a loaded IFC project, RNA writes to `scene.BIMSolarProperties` rewrite Sun Position, so the mirror writes Sun Position's values back in an order that ends with both holding them:

```python
# [HEADLESS_CALL] Sun Position's site, moment, and north mirrored into Bonsai solar under a loaded IFC project
import bpy

scene = bpy.context.scene
sun, solar = scene.sun_pos_properties, scene.BIMSolarProperties
hour, minute = divmod(round(sun.time * 60), 60)
mirrored = {
    "sun_path_size": sun.sun_distance,
    "latitude": sun.latitude,
    "longitude": sun.longitude,
    "year": sun.year,
    "month": sun.month,
    "day": sun.day,
    "hour": hour,
    "minute": minute,
    "true_north": -sun.north_offset,
}
for name, value in mirrored.items():
    setattr(solar, name, value)
result = {"timezone": solar.timezone, "UTC_zone": solar.UTC_zone, "sun": [sun.UTC_zone, sun.use_daylight_savings, sun.time, sun.north_offset]}
```

- Bonsai derives `timezone` from the coordinates and stores `UTC_zone` as the negated offset in effect, 5.0 at 12:00 CDT
- `sun_path_size` sets Sun Position's `sun_distance`, `true_north` its `north_offset` negated
- Without an IFC project each RNA write prints a traceback and changes nothing, item writes (`solar["latitude"] = <value>`) skip every callback
- `bim.import_lat_long` copies the IfcSite `RefLatitude` and `RefLongitude` into the solar fields through their callbacks
- `shadow_mode = "SHADING"` switches the scene to Workbench

## [03]-[GIS_IMPORTS]

BlenderGIS imports under `bpy.ops.importgis` land in the scene CRS around the georeference origin. BlenderGIS preferences hold `demServer` USGS 3DEP and `overpassServer` overpass-api.de, both keyless, with no OpenTopography or MapTiler key. USGS NAIP serves keyless imagery in any CRS for sites in the United States:

```bash
# NAIP orthoimage in the scene CRS over the projected box <west> <south> <east> <north> (geo.crsx and geo.crsy plus or minus <half>) at 0.3 m pixels
curl -fsS -o <dir>/<image>.tif 'https://imagery.nationalmap.gov/arcgis/rest/services/USGSNAIPPlus/ImageServer/exportImage?bbox=<west>,<south>,<east>,<north>&bboxSR=<epsg>&imageSR=<epsg>&size=<pixels>,<pixels>&bandIds=0,1,2&format=tiff&f=image'
```

```python
# [EXECUTE_BLENDER_CODE] Site extent of <half> m about the origin with the 3DEP DEM displaced onto it and an orthoimage draped over it
import bpy

scene = bpy.context.scene
half = <half>
mesh = bpy.data.meshes.new("<extent>")
mesh.from_pydata([(x * half, y * half, 0.0) for x, y in ((-1, -1), (1, -1), (1, 1), (-1, 1))], (), [(0, 1, 2, 3)])
site = bpy.data.objects.new("<extent>", mesh)
scene.collection.objects.link(site)
with bpy.context.temp_override(active_object=site, selected_objects=[site]):
    bpy.ops.importgis.dem_query()
bpy.ops.importgis.georaster(filepath="<dir>/<image>.tif", importMode="MESH", objectsLst=str(list(scene.objects).index(site)))
result = {"modifiers": [m.type for m in site.modifiers], "material": site.active_material.name}
```

```python
# [EXECUTE_BLENDER_CODE] OSM buildings over the extent standing on its DEM, one child collection per tag under OSM
import importlib

import addon_utils
import bpy

scene = bpy.context.scene
site = bpy.data.objects["<extent>"]
package = {m.bl_info["name"]: m.__name__ for m in addon_utils.modules()}["BlenderGIS"]
osm = importlib.import_module(f"{package}.operators.io_import_osm")
osm.OSMTAGS = osm.getTags()
with bpy.context.temp_override(active_object=site, selected_objects=[site]):
    status = bpy.ops.importgis.osm_query(filterTags={"building"}, separate=True, useElevObj=True, objElevLst=str(list(scene.objects).index(site)))
result = {"status": sorted(status), "buildings": sorted(o.name for o in bpy.data.collections["OSM"].all_objects)}
```

```python
# [EXECUTE_BLENDER_CODE] Shapefile in <epsg> as one object per feature standing on the DEM, extruded by a field and named by another
import bpy

status = bpy.ops.importgis.shapefile(
    filepath="<file>.shp", shpCRS="EPSG:<epsg>", elevSource="OBJ", objElevName="<extent>", fieldExtrudeName="<height field>", separateObjects=True, fieldObjName="<name field>"
)
result = {"status": sorted(status), "objects": sorted(o.name for o in bpy.data.collections["<file stem>"].objects)}
```

- `dem_query` and `osm_query` take their extent from the one selected mesh and read `context.window`, which live and session calls hold
- `dem_query` pads the extent 0.002° and writes `srtm.tif` beside the saved `.blend`, else in `bpy.app.tempdir`
- `dem_query` displaces the extent through SUBSURF and DISPLACE modifiers and leaves it active and selected
- DEM heights are orthometric meters, `location.z -= <elevation>` on the DEM and each object standing on it puts the site ground at zero
- `georaster` imports a GeoTIFF in the scene CRS as it stands, one in another CRS takes `reprojection=True, rastCRS="EPSG:<epsg>"`
- `georaster` with `importMode="DEM"` builds terrain from an elevation GeoTIFF, `"PLANE"` a textured plane
- `osm.OSMTAGS = osm.getTags()` fills the tag list the dialog's invoke fills, so `filterTags` takes tags of the `osmTagsJson` preference
- OSM building height reads `height`, else `building:levels` times `levelHeight`, else `defaultHeight` (20 m)
- Overpass under load raises `OverpassGatewayTimeout: Server load too high`, and the OSM call repeated a minute later answers
- `shapefile` takes `shpCRS` as the file's CRS and reprojects to the scene CRS, `elevSource` `GEOM` reads Z and `FIELD` a field
- Blosm's `blosm.import_data` builds roofed buildings over the `scene.blosm` extent on a sphere around `scene["lat"]` and `scene["lon"]`
- Blosm imports sit 1.17° from BlenderGIS imports at the Houston site, the UTM meridian convergence

## [04]-[CLIMATE]

Climate studies read an EPW of the station nearest the site from climate.onebuilding.org, whose `sources/` page lists one station table per region:

```bash
# Newest TMYx archive of the station nearest <latitude> <longitude> in the <region> table, extracted under <dir>
url=$(duckdb -noheader -list -c "load excel; select URL from read_xlsx('https://climate.onebuilding.org/sources/<region>_TMYx_EPW_Processing_locations.xlsx', all_varchar = true) order by pow(\"Latitude (N+/S-)\"::double - <latitude>, 2) + pow((\"Longitude (E+/W-)\"::double - <longitude>) * cos(radians(<latitude>)), 2), regexp_extract(URL, '(\d{4})\.zip', 1) desc limit 1;")
curl -fsS -o <dir>/epw.zip "$url"
unzip -o -d <dir> <dir>/epw.zip '*.epw'
```

- `<region>` names a table on the sources page (`Region4_USA`, `Region4_Canada`, `Region6_Europe`)
- EPW `LOCATION` lines hold station, WMO number, latitude, longitude, standard offset, and elevation, and hourly rows run in standard time
- EnVi and CBDM take the sun from the EPW, VI-Suite sun paths and LiVi skies from `solarPosition`
- VI-Suite's Location node lists the `*.epw` files of the `epweath` preference folder, the bundled UK files while it is empty
- Picking an EPW in the Location node writes the station's latitude and longitude into `vi_params`, and the node raises in a background call
