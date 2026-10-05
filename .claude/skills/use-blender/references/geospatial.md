# [GEOSPATIAL]

Site models sit near the world origin, the scene georeference holds their map position, and one sun vector feeds every sun store.

## [01]-[SITE]

`tools/interface/render.py` declares the site and moment (`LATITUDE`, `LONGITUDE`, `ELEVATION`, `NORTH`, `MOMENT`, `OFFSET`, `DAYLIGHT`). Files made from the user's startup file hold them in BlenderGIS, Sun Position, Bonsai solar, and the `Analysis` scene's VI-Suite fields.

Every file starts with one read:

```python
# [EXECUTE_BLENDER_CODE] Georeference, Sun Position moment, and sun vectors from Sun Position, sun_calc, VI-Suite, and the light
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
scratch = bpy.data.scenes.new("scratch")
scratch.vi_params["viparams"] = {}
with bpy.context.temp_override(scene=scratch):
    altitude, south_azimuth, *_ = solarPosition(date(sun.year, sun.month, sun.day).timetuple().tm_yday, sun.time - sun.use_daylight_savings, sun.latitude, sun.longitude)
bpy.data.scenes.remove(scratch)
result = {
    "georeference": [geo.crs, geo.lon, geo.lat, geo.crsx, geo.crsy, geo.isBroken],
    "moment": [sun.year, sun.month, sun.day, sun.time, sun.UTC_zone, sun.use_daylight_savings, math.degrees(sun.north_offset)],
    "bound": [sun.sun_object and sun.sun_object.name, sun.sky_texture],
    "sun_position": [math.degrees(sun.sun_azimuth), math.degrees(sun.sun_elevation)],
    "sun_calc": [math.degrees(azimuth), math.degrees(elevation)],
    "vi_suite": [south_azimuth - 180, altitude],
    "light": sun.sun_object and list(sun.sun_object.matrix_world.col[2].to_3d()),
}
```

Its reading decides the next step:

| [INDEX] | [READING]                                                                  | [NEXT_STEP]                   |
| :-----: | :------------------------------------------------------------------------- | :---------------------------- |
|  [01]   | Declared site and moment, Sun Position, `sun_calc`, and the light agreeing | Work on the site as it stands |
|  [02]   | No georeference, `isBroken` true, or another site's values                 | New-site snippet              |
|  [03]   | `bound` holding `None` or an empty sky name                                | New-site snippet              |

- Azimuths read clockwise from north, the light's +Z axis is the sun vector, +Y north
- `sun_calc` takes the zone as `-(UTC_zone + daylight)` and adds the context scene's `north_offset` to its azimuth
- `solarPosition` takes EPW standard time and a day of year, answers azimuth clockwise from south, and reads `viparams` from the context scene
- `solarPosition` departs 0.82° in elevation and 0.5° in azimuth from Sun Position's geometric sun at the site moment
- `use_refraction` False reads the geometric elevation Rhino computes, True the apparent one

```python
# [EXECUTE_BLENDER_CODE] Georeference in the site UTM zone, sky and sun light bound, and the moment on the context scene
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
light = next((o for o in scene.objects if o.type == "LIGHT" and o.data.type == "SUN"), None)
if light is None:
    light = bpy.data.objects.new("Sun", bpy.data.lights.new("Sun", "SUN"))
    scene.collection.objects.link(light)
sun = scene.sun_pos_properties
sun.sun_object, sun.sky_texture = light, sky.name
sun.latitude, sun.longitude = latitude, longitude
sun.UTC_zone, sun.use_daylight_savings = moment.utcoffset() / timedelta(hours=1) - daylight, bool(daylight)
sun.north_offset = -math.radians(north)
sun.year, sun.month, sun.day, sun.time = moment.year, moment.month, moment.day, moment.hour + moment.minute / 60
result = {"georeference": [geo.crs, geo.crsx, geo.crsy], "sun": [math.degrees(sun.sun_azimuth), math.degrees(sun.sun_elevation)], "vector": list(sky.sun_direction)}
```

- `<north>` is true north in degrees anti-clockwise from +Y, `<zone>` the site's IANA zone, Rhino takes `Sun.North` 90 + `<north>`
- Rhino's `Sun.TimeZone` and Sun Position's `UTC_zone` take the standard offset beside a daylight flag, zoneinfo giving both
- Light and sky bind before the place and moment fields, each Sun Position field re-placing both and the last write placing them
- Sun Position callbacks move the context scene's sun, a write to another scene running under `temp_override(scene=<scene>)`
- UTM holds scale within 0.04% across a zone, and BlenderGIS projects through its bundled `pyproj` with no network or GDAL
- Models stay at the origin in local meters, object locations and mesh positions being float32 that step 0.25 m at a UTM northing
- `geo.updOriginGeo(<longitude>, <latitude>)` moves a set origin to the point and every root object by minus the projected delta

Use look-development.md for the site sky and sun.

## [02]-[BONSAI_SOLAR]

Bonsai solar holds Sun Position's values once an IFC project is loaded into the open file, its callbacks rewriting Sun Position from each field:
1. `bim.load_project(filepath=<file>.ifc, should_start_fresh_session=False)` loads the project with the light kept
2. One call copies Sun Position's site, moment, and north in an order leaving both stores holding Sun Position's values

```python
# [HEADLESS_CALL] Sun Position's site, moment, and north copied into Bonsai solar under a loaded IFC project
import bpy

scene = bpy.context.scene
sun, solar = scene.sun_pos_properties, scene.BIMSolarProperties
hour, minute = divmod(round(sun.time * 60), 60)
copied = {
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
for name, value in copied.items():
    setattr(solar, name, value)
result = {"timezone": solar.timezone, "UTC_zone": solar.UTC_zone, "sun": [sun.UTC_zone, sun.use_daylight_savings, sun.time, sun.north_offset]}
```

- Bonsai derives `timezone` from the coordinates and stores `UTC_zone` as the negated offset in effect, 5.0 at 12:00 CDT
- `sun_path_size` sets Sun Position's `sun_distance`, `true_north` its `north_offset` negated
- RNA writes run the callbacks under a loaded IFC project, and item writes (`solar["latitude"] = <value>`) store a value with no callback
- `bim.import_lat_long` copies the IfcSite `RefLatitude` and `RefLongitude` into the solar fields through their callbacks
- `shadow_mode = "SHADING"` switches the scene to Workbench

Use bim.md for IFC loads.

## [03]-[GIS_IMPORTS]

BlenderGIS imports under `bpy.ops.importgis` sit in the scene CRS around the georeference origin, in order:
1. `curl` of a NAIP orthoimage over the site box in the scene CRS, USGS serving keyless imagery for United States sites
2. `dem_query` on an extent mesh, then `georaster` draping the image over it
3. `osm_query` of buildings standing on the DEM, a call answering `OverpassGatewayTimeout` repeated after a 30 s pause
4. `shapefile` of any vector source standing on the DEM
5. `location.z -= <elevation>` on the DEM and each object standing on it, the site ground at zero

BlenderGIS preferences hold keyless `demServer` USGS 3DEP and `overpassServer` overpass-api.de, with no OpenTopography or MapTiler key.

```bash
# NAIP orthoimage in the scene CRS over projected box <west> <south> <east> <north> (geo.crsx and geo.crsy ± <half>), <pixels> at most 4000
curl -fsS -o <dir>/<image>.tif 'https://imagery.nationalmap.gov/arcgis/rest/services/USGSNAIPPlus/ImageServer/exportImage?bbox=<west>,<south>,<east>,<north>&bboxSR=<epsg>&imageSR=<epsg>&size=<pixels>,<pixels>&bandIds=0,1,2&format=tiff&f=image'
```

```python
# [EXECUTE_BLENDER_CODE] Site extent <Object> of <half> m about the origin with the 3DEP DEM displaced onto it and an orthoimage draped over it
import bpy

scene = bpy.context.scene
half = <half>
mesh = bpy.data.meshes.new("<Object>")
mesh.from_pydata([(x * half, y * half, 0.0) for x, y in ((-1, -1), (1, -1), (1, 1), (-1, 1))], (), [(0, 1, 2, 3)])
site = bpy.data.objects.new("<Object>", mesh)
scene.collection.objects.link(site)
with bpy.context.temp_override(active_object=site, selected_objects=[site]):
    bpy.ops.importgis.dem_query()
bpy.ops.importgis.georaster(filepath="<dir>/<image>.tif", importMode="MESH", objectsLst=str(list(scene.objects).index(site)))
result = {"modifiers": [m.type for m in site.modifiers], "material": site.active_material.name}
```

```python
# [EXECUTE_BLENDER_CODE] OSM buildings over the extent <Object> standing on its DEM, one child collection per tag under OSM
import importlib

import addon_utils
import bpy

scene = bpy.context.scene
site = bpy.data.objects["<Object>"]
package = {m.bl_info["name"]: m.__name__ for m in addon_utils.modules()}["BlenderGIS"]
osm = importlib.import_module(f"{package}.operators.io_import_osm")
osm.OSMTAGS = osm.getTags()
with bpy.context.temp_override(active_object=site, selected_objects=[site]):
    status = bpy.ops.importgis.osm_query(filterTags={"building"}, separate=True, useElevObj=True, objElevLst=str(list(scene.objects).index(site)))
result = {"status": sorted(status), "buildings": sorted(o.name for o in bpy.data.collections["OSM"].all_objects)}
```

```python
# [EXECUTE_BLENDER_CODE] Shapefile in <epsg> as one object per feature standing on the DEM <Object>, extruded by a field and named by another
import bpy

status = bpy.ops.importgis.shapefile(
    filepath="<file>.shp", shpCRS="EPSG:<epsg>", elevSource="OBJ", objElevName="<Object>", fieldExtrudeName="<height field>", separateObjects=True, fieldObjName="<name field>"
)
result = {"status": sorted(status), "objects": sorted(o.name for o in bpy.data.collections["<file stem>"].objects)}
```

- `dem_query` and `osm_query` take their extent from the one selected mesh and read `context.window`, which live and session calls hold
- `dem_query` pads the extent 0.002° and writes `srtm.tif` beside the saved `.blend`, else in `bpy.app.tempdir`
- `dem_query` displaces the extent through SUBSURF and DISPLACE modifiers and leaves it active and selected
- DEM heights are orthometric meters above sea level
- `georaster` imports a GeoTIFF in the scene CRS as it stands, one in another CRS taking `reprojection=True, rastCRS="EPSG:<epsg>"`
- `georaster` with `importMode="DEM"` builds terrain from an elevation GeoTIFF, `"PLANE"` a textured plane
- `osm.OSMTAGS = osm.getTags()` fills the tag list the dialog's invoke fills, and `filterTags` takes tags of the `osmTagsJson` preference
- OSM building height reads `height`, else `building:levels` times `levelHeight`, else `defaultHeight` (20 m)
- `shapefile` takes `shpCRS` as the file's CRS and reprojects to the scene CRS, `elevSource` `GEOM` reading Z and `FIELD` a field
- `shapefile` links one object per feature into a collection named after the file, each origin at its feature
- Blosm's `blosm.import_data` builds roofed buildings over the `scene.blosm` extent on a sphere around `scene["lat"]` and `scene["lon"]`
- Blosm imports sit 1.17° from BlenderGIS imports at the Houston site, the UTM meridian convergence, and 0.42% longer north-south

## [04]-[CLIMATE]

Climate studies read an EPW of the station nearest the site from climate.onebuilding.org, whose `sources/` page lists one station table per region:
1. `duckdb` names the newest TMYx archive of the nearest station in the region table
2. `curl` downloads the archive
3. `unzip` extracts its EPW
4. `head -1 <dir>/*.epw` reads the `LOCATION` line naming the station

```bash
# Newest TMYx archive URL of the station nearest <latitude> <longitude> in the <region> table
duckdb -noheader -list -c "load excel; select URL from read_xlsx('https://climate.onebuilding.org/sources/<region>_TMYx_EPW_Processing_locations.xlsx', all_varchar = true) order by pow(\"Latitude (N+/S-)\"::double - <latitude>, 2) + pow((\"Longitude (E+/W-)\"::double - <longitude>) * cos(radians(<latitude>)), 2), regexp_extract(URL, '(\d{4})\.zip', 1) desc limit 1;"

# Archive downloaded and its EPW extracted under <dir>
curl -fsS -o <dir>/epw.zip '<url>'
unzip -o -d <dir> <dir>/epw.zip '*.epw'
```

- `<region>` names a table on the sources page (`Region4_USA`, `Region4_Canada`, `Region6_Europe`)
- EPW `LOCATION` lines hold station, WMO number, latitude, longitude, standard offset, and elevation, and hourly rows run in standard time
- EnVi and CBDM take the sun from the EPW, VI-Suite sun paths and LiVi skies from `solarPosition`
- VI-Suite's Location node lists the `*.epw` files of the `epweath` preference folder, the bundled UK files while it is empty
- Picking an EPW in the Location node, a GUI step reading `context.space_data`, writes the station's latitude and longitude into `vi_params`
