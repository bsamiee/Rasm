# [GIS_SITE]

Covers site and geospatial work.

## [01]-[ROUTES]

Site models sit at world origin in local meters with site ground at z 0, and scene georeference names the origin's map position. With Blender locations in float32, stepping 0.25 m at a UTM northing, every import sits at an offset from the georeference origin:

| [INDEX] | [TASK]                          | [ROUTE]                                                                 |
| :-----: | :------------------------------ | :---------------------------------------------------------------------- |
|  [01]   | Read the site                   | `GeoScene`, Blosm keys, Sun Position, grid north in one call            |
|  [02]   | New site                        | `GeoScene` CRS then origin, Blosm keys, Sun Position site and north     |
|  [03]   | Terrain, United States          | `curl` of 3DEP in scene CRS, `importgis.georaster` DEM, ground to z 0   |
|  [04]   | Orthoimage                      | `curl` of NAIP or Esri export in scene CRS, `importgis.georaster` drape |
|  [05]   | OSM buildings and roads         | `curl` of an Overpass map file, `importgis.osm_file` on the terrain     |
|  [06]   | OSM buildings with roofs, parts | Same file through `blosm.import_data`, frame matrix onto scene CRS      |
|  [07]   | Survey TIN                      | `import_scene.landxml_tin`, object matrix from the stored origin        |
|  [08]   | LiDAR or scan                   | `import_scene.point_cloud_las` or `_e57`, placed from offset or poses   |
|  [09]   | Shapefile                       | `importgis.shapefile` in its CRS, on the terrain                        |
|  [10]   | Sun at a moment, study, diagram | Sun Position fields, keyed `time`, diagram operators                    |

Background runs and GUI sessions run every routed operator, BlenderGIS query operators reading the window a background run holds. BlenderGIS and Blosm download with no `bpy.app.online_access` read.

## [02]-[SITE_READ]

Every task starts from one read of georeference stores and grid north, the clockwise angle of true north from +Y in scene CRS at origin:

```python
import math

import bpy

from BlenderGIS.core.proj import reprojPt
from BlenderGIS.geoscene import GeoScene

scene = bpy.context.scene
geo = GeoScene(scene)
sun = scene.sun_pos_properties
x, y = reprojPt(4326, geo.crs, geo.lon, geo.lat)
north_x, north_y = reprojPt(4326, geo.crs, geo.lon, geo.lat + 0.001)
result = {
    "georeference": [geo.crs, geo.lon, geo.lat, geo.crsx, geo.crsy, geo.isGeoref, geo.isBroken],
    "projected_drift": [x - geo.crsx, y - geo.crsy],
    "blosm": [scene.get("lat"), scene.get("lon")],
    "sun_site": [sun.latitude, sun.longitude, math.degrees(sun.north_offset)],
    "grid_north": math.degrees(math.atan2(north_x - x, north_y - y)),
    "sun_bound": [sun.sun_object and sun.sun_object.name, sun.sky_texture],
}
```

| [INDEX] | [READING]                                                     | [NEXT_STEP]                   |
| :-----: | :------------------------------------------------------------ | :---------------------------- |
|  [01]   | Drift zero, Blosm and Sun Position at the origin, north equal | Work on the site as it stands |
|  [02]   | `isGeoref` false, `isBroken` true, or non-zero drift          | New site sequence             |
|  [03]   | Blosm keys or Sun Position site off the origin                | Both writes from new site     |
|  [04]   | `north_offset` off `grid_north`                               | `sun.north_offset` write      |
|  [05]   | `sun_bound` holding `None` or an empty sky name               | Sun binding                   |

With `isGeoref` holding while a stale projected origin sits beside a moved latitude and longitude, drift is the read that finds it.

## [03]-[NEW_SITE]

One call writes the georeference, Blosm's center, and Sun Position's site, moment, and north:

```python
import math
from datetime import datetime, timedelta
from zoneinfo import ZoneInfo

import bpy

from BlenderGIS.core.proj import reprojPt
from BlenderGIS.geoscene import GeoScene

scene = bpy.context.scene
latitude, longitude = <latitude>, <longitude>
moment = datetime(<year>, <month>, <day>, <hour>, <minute>, tzinfo=ZoneInfo("<zone>"))
geo = GeoScene(scene)
geo.delOrigin()
geo.crs = f"EPSG:{(32600 if latitude >= 0 else 32700) + int((longitude + 180) // 6) + 1}"
geo.setOriginGeo(longitude, latitude)
scene["lat"], scene["lon"] = latitude, longitude
x, y = reprojPt(4326, geo.crs, longitude, latitude)
north_x, north_y = reprojPt(4326, geo.crs, longitude, latitude + 0.001)
daylight = moment.dst() / timedelta(hours=1)
sun = scene.sun_pos_properties
sun.latitude, sun.longitude = latitude, longitude
sun.UTC_zone, sun.use_daylight_savings = moment.utcoffset() / timedelta(hours=1) - daylight, bool(daylight)
sun.north_offset = math.atan2(north_x - x, north_y - y)
sun.year, sun.month, sun.day, sun.time = moment.year, moment.month, moment.day, moment.hour + moment.minute / 60
```

- `geo.lon` and `geo.lat` writes leave projected origin and CRS on the old site with no error
- Site writes run `delOrigin`, then `geo.crs`, then `setOriginGeo`
- `geo.updOriginGeo(lon, lat)` moves an existing site and shifts every root object by projected delta, keeping placed data on the map
- `sun.north_offset` takes grid north in radians, a UTM scene's sun sitting off by meridian convergence until the write
- `sun.UTC_zone` takes standard offset, `use_daylight_savings` holding the daylight hour
- `sun.use_refraction` false reads the geometric elevation other solar tools report
- Scene CRS stays metric (UTM, a metric State Plane), BlenderGIS placing CRS units as Blender meters

Confirmation read: site read reports zero drift, Blosm and Sun Position at the origin, and `north_offset` equal to `grid_north`.

## [04]-[FRAMES]

Data in a frame other than scene CRS meets scene through one object matrix, the Jacobian from source-local to scene-local meters at source origin with origin's scene position as translation, accurate to a centimeter within 1 km:

```python
from collections.abc import Callable

from mathutils import Matrix, Vector


def framed(to_scene: Callable[[float, float], Vector], height: float) -> Matrix:
    origin, east, north = to_scene(0.0, 0.0), to_scene(100.0, 0.0), to_scene(0.0, 100.0)
    return Matrix((
        ((east.x - origin.x) / 100.0, (north.x - origin.x) / 100.0, 0.0, origin.x),
        ((east.y - origin.y) / 100.0, (north.y - origin.y) / 100.0, 0.0, origin.y),
        (0.0, 0.0, 1.0, height),
        (0.0, 0.0, 0.0, 1.0),
    ))
```

| [INDEX] | [SOURCE]                | [TO_SCENE]                                                                                    |
| :-----: | :---------------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | Survey in another CRS   | `reprojPt("<source EPSG>", geo.crs, east + x / unit, north + y / unit)` minus scene origin    |
|  [02]   | Blosm import            | `TransverseMercator(lat=, lon=).toGeographic(x, y)`, then `reprojPt(4326, geo.crs, lon, lat)` |
|  [03]   | Survey in the scene CRS | `Vector((east * unit - geo.crsx + x, north * unit - geo.crsy + y))`, a translation            |

`reprojPt` runs offline through the `pyproj` BlenderGIS bundles. Height is source origin's elevation minus site elevation.

## [05]-[TERRAIN]

United States terrain comes from keyless USGS 3DEP image service in scene CRS at 1 m pixels over the site box:

```bash
curl -fsS -o <dir>/dem.tif 'https://elevation.nationalmap.gov/arcgis/rest/services/3DEPElevation/ImageServer/exportImage?bbox=<west>,<south>,<east>,<north>&bboxSR=<epsg>&imageSR=<epsg>&size=<pixels>,<pixels>&format=tiff&pixelType=F32&interpolation=RSP_BilinearInterpolation&f=image'
```

```python
import bpy

before = set(bpy.data.objects)
status = bpy.ops.importgis.georaster(filepath="<dir>/dem.tif", importMode="DEM", subdivision="mesh")
dem = next(obj for obj in bpy.data.objects if obj not in before)
hit, location, normal, index = dem.ray_cast((0.0, 0.0, 10000.0), (0.0, 0.0, -1.0), depsgraph=bpy.context.evaluated_depsgraph_get())
dem.location.z = -location.z
result = {"status": sorted(status), "terrain": dem.name, "site_elevation": location.z}
```

- Box corners are `geo.crsx` and `geo.crsy` plus and minus half extent, and `<pixels>` the extent in meters up to 8000
- Site elevation is DEM height at the origin, later imports at absolute height taking `location.z -= <site elevation>`
- `georaster` reads rasters in the scene CRS, its `rastCRS` taking only CRS strings its preference lists
- With DEMs reading GeoTIFFs by absolute path, GeoTIFFs stay beside the `.blend`
- `importgis.dem_query` writes a fixed `srtm.tif` each call, a second query overwriting the first terrain's image
- Sites outside the United States take OpenTopography `demServer` entries with `opentopography_api_key` set

Confirmation read: evaluated DEM spans site box, and a ray at the origin hits z 0.

## [06]-[IMAGERY]

Orthoimages come in scene CRS over the DEM box and drape through its UV map. United States sites take USGS NAIP Plus, and other sites Esri World Imagery as PNG with a world file:

```bash
curl -fsS -o <dir>/naip.tif 'https://imagery.nationalmap.gov/arcgis/rest/services/USGSNAIPPlus/ImageServer/exportImage?bbox=<west>,<south>,<east>,<north>&bboxSR=<epsg>&imageSR=<epsg>&size=<pixels>,<pixels>&bandIds=0,1,2&format=tiff&f=image'
curl -fsS -o <dir>/ortho.png 'https://services.arcgisonline.com/arcgis/rest/services/World_Imagery/MapServer/export?bbox=<west>,<south>,<east>,<north>&bboxSR=<epsg>&imageSR=<epsg>&size=<pixels>,<pixels>&format=png&f=image'
printf '%s\n0\n0\n-%s\n%s\n%s\n' <pixel size> <pixel size> <west + pixel size / 2> <north - pixel size / 2> > <dir>/ortho.pgw
```

```python
import bpy

dem = bpy.data.objects["<terrain>"]
dem.data.materials.clear()
status = bpy.ops.importgis.georaster(filepath="<dir>/<image>", importMode="MESH", objectsLst=str(list(bpy.context.scene.objects).index(dem)))
```

- `objectsLst` takes the mesh's index in `scene.objects` as a string
- Drapes clear the mesh's materials first, `georaster` appending a slot that faces on slot 0 never show
- `importMode="PLANE"` imports the image as a textured plane at z 0
- Images stay external by absolute path, and `bpy.ops.file.pack_all()` before a save holds them in the `.blend`

Confirmation read: one material slot `rastMat` holding the image, and active UV map `rastUVmap`.

## [07]-[OSM]

OSM data comes once as a file, the Overpass `map` call returning every feature in a box to an identifying user agent, and both importers read it:

```bash
curl -fsS -A '<application name>' -o <dir>/site.osm 'https://overpass-api.de/api/map?bbox=<min lon>,<min lat>,<max lon>,<max lat>'
```

```python
import bpy

import BlenderGIS.operators.io_import_osm as osm

dem = bpy.data.objects["<terrain>"]
osm.OSMTAGS = osm.getTags()
status = bpy.ops.importgis.osm_file(filepath="<dir>/site.osm", filterTags={"building", "highway"}, separate=True, useElevObj=True, objElevLst=str(list(bpy.context.scene.objects).index(dem)))
result = {"status": sorted(status), "collections": {c.name: len(c.objects) for c in bpy.data.collections["OSM"].children}}
```

- Box corners come from the terrain extent through `reprojPt(geo.crs, 4326, geo.crsx + <x>, geo.crsy + <y>)`
- `osm.OSMTAGS = osm.getTags()` precedes each script call, `filterTags` validating against a list the dialog fills
- `separate` links one object per feature into a child collection per tag under `OSM`, tags as custom properties
- Building height reads `height`, else `building:levels` times `levelHeight`, else `defaultHeight`, with flat roofs
- OSM data stays inside terrain extent, features off the terrain sitting at its object z
- Overpass answers 406 to curl's default and browser user agents, and the file route keeps one download for every import

Confirmation read: per-tag counts, and building base vertices on the terrain reading z near 0.

## [08]-[BLOSM]

Blosm builds roof shapes and `building:part` stacks in a spherical transverse Mercator about `scene["lat"]` and `scene["lon"]` that turns and stretches against UTM. Terrain enters Blosm's frame through inverse frame matrix, and frame matrix places the result:

```python
import bpy
from mathutils import Vector

from BlenderGIS.core.proj import reprojPt
from BlenderGIS.geoscene import GeoScene
from util.transverse_mercator import TransverseMercator

scene = bpy.context.scene
geo = GeoScene(scene)
scene["lat"], scene["lon"] = geo.lat, geo.lon
projection = TransverseMercator(lat=geo.lat, lon=geo.lon)


def to_scene(x: float, y: float) -> Vector:
    lat, lon = projection.toGeographic(x, y)
    east, north = reprojPt(4326, geo.crs, lon, lat)
    return Vector((east - geo.crsx, north - geo.crsy))


frame = framed(to_scene, 0.0)
dem = bpy.data.objects["<terrain>"]
mesh = bpy.data.meshes.new_from_object(dem.evaluated_get(bpy.context.evaluated_depsgraph_get()))
mesh.transform(frame.inverted() @ dem.matrix_world)
terrain = bpy.data.objects.new("<terrain>_blosm", mesh)
scene.collection.objects.link(terrain)
settings = scene.blosm
settings.dataType, settings.mode, settings.osmSource, settings.osmFilepath = "osm", "3Dsimple", "file", "<dir>/site.osm"
settings.buildings, settings.highways, settings.water, settings.forests, settings.vegetation, settings.railways = True, False, False, False, False, False
settings.terrainObject, settings.singleObject = terrain.name, False
before = set(bpy.data.objects)
status = bpy.ops.blosm.import_data()
added = [obj for obj in bpy.data.objects if obj not in before]
envelopes = [obj for obj in added if obj.name.endswith("_envelope")]
for obj in (obj for obj in added if obj not in envelopes and obj.parent is None):
    obj.matrix_world = frame @ obj.matrix_world
for obj in (terrain, *envelopes):
    data = obj.data
    bpy.data.objects.remove(obj)
    bpy.data.meshes.remove(data)
```

- `util.transverse_mercator` resolves once Blosm loads
- With Blosm reading terrain vertices with no modifiers, its terrain is the baked `<terrain>_blosm` mesh
- `settings.terrainObject` drops features outside the terrain, and an empty one builds every feature at z 0
- Roads and water come from BlenderGIS, Blosm projecting them onto its envelope
- `osmSource="server"` downloads the same Overpass `map` file into `<dataDir>/osm`

Confirmation read: footprints match BlenderGIS's import of the site file in centroid and extent, each building's lowest base corner on terrain.

## [09]-[SURVEY_TIN]

LandXML TINs import with minimum easting and northing subtracted and stored per surface in double precision, and the object matrix places them:

```python
import bpy
from mathutils import Vector

from BlenderGIS.core.proj import reprojPt
from BlenderGIS.geoscene import GeoScene

geo = GeoScene(bpy.context.scene)
status = bpy.ops.import_scene.landxml_tin(filepath="<file>.xml", source_units="<units>")
for tin in [obj for obj in bpy.context.selected_objects if "landxml_source" in obj]:
    unit, east, north = tin["landxml_unit_to_meters"], tin["landxml_origin_easting"], tin["landxml_origin_northing"]

    def to_scene(x: float, y: float) -> Vector:
        px, py = reprojPt("<source EPSG>", geo.crs, east + x / unit, north + y / unit)
        return Vector((px - geo.crsx, py - geo.crsy))

    tin.matrix_world = framed(to_scene, tin["landxml_origin_elevation"] * unit - <site elevation>)
```

- `source_units` names file's `<Units>` element on every call (`METERS`, `US_SURVEY_FEET`, `INTERNATIONAL_FEET`), default US survey feet
- Placement comes from stored origin, float32 `pivot_*` properties rounding projected coordinates
- `<source EPSG>` comes from file's `CoordinateSystem` element or the survey's statement
- `point_order` stays `NEZ`, and `ENZ` answers a TIN mirrored about the diagonal
- Locked transforms block interactive moves alone, and `matrix_world` writes apply
- GUI imports draw an `Import completed` popup over the window with no `modal_operators` entry

Confirmation read: surface vertices sampled across the site read terrain heights to survey accuracy.

## [10]-[POINT_CLOUDS]

Point Cloud I/O imports each file as a `POINTCLOUD` object with `color`, `intensity`, `classification`, and `radius` attributes. LAS and LAZ center their points and store the subtracted minimum as `las_origin_offset`:

```python
import bpy

from BlenderGIS.geoscene import GeoScene

geo = GeoScene(bpy.context.scene)
before = set(bpy.data.objects)
status = bpy.ops.import_scene.point_cloud_las(filepath="<file>.las", auto_radius=False, point_radius=<half point spacing>)
cloud = next(obj for obj in bpy.data.objects if obj not in before)
east, north, height = cloud["las_origin_offset"]
cloud.location = (east - geo.crsx, north - geo.crsy, height - <site elevation>)
```

E57 scans import in each scanner's frame, and each scan's header pose places it:

```python
import pye57
from mathutils import Matrix

before = set(bpy.data.objects)
status = bpy.ops.import_scene.point_cloud_e57(filepath="<file>.e57", merge_scans=False)
reader = pye57.E57("<file>.e57")
for index, scan in enumerate(sorted((obj for obj in bpy.data.objects if obj not in before), key=lambda obj: obj.name)):
    header = reader.get_header(index)
    scan.matrix_world = Matrix.Translation(header.translation) @ Matrix(header.rotation_matrix).to_4x4()
```

- XYZ, PTS, PLY, and PCD imports cast coordinates to float32 uncentered
- Projected text clouds shift to site-local meters before import (`awk '{ $1 -= <crs x>; $2 -= <crs y>; $3 -= <site elevation>; print }'`)
- With `merge_scans` joining scans before any pose applies, posed E57 files import per scan
- Airborne LiDAR takes `auto_radius=False` with half the point spacing
- With `scale_factor` leaving `las_origin_offset` in file units, feet clouds scale offset by the unit
- LAS clouds in another CRS take `framed` with the offset as source origin

Confirmation read: sampled points read the terrain heights, and overlapping scans coincide.

## [11]-[SHAPEFILE]

`importgis.shapefile` reads a shapefile in the CRS `shpCRS` names and reprojects it to scene CRS:

```python
bpy.ops.importgis.shapefile(filepath="<file>.shp", shpCRS="EPSG:<epsg>", elevSource="OBJ", objElevName="<terrain>", fieldExtrudeName="<height field>", separateObjects=True, fieldObjName="<name field>")
```

| [INDEX] | [ELEV_SOURCE] | [HEIGHTS]                       | [AFTER_IMPORT]                   |
| :-----: | :------------ | :------------------------------ | :------------------------------- |
|  [01]   | `OBJ`         | Draped on `objElevName` by name | None                             |
|  [02]   | `FIELD`       | Absolute, from `fieldElevName`  | `location.z -= <site elevation>` |
|  [03]   | `GEOM`        | Absolute, from the shape's Z    | `location.z -= <site elevation>` |
|  [04]   | `NONE`        | Zero                            | None                             |

`separateObjects` links one object per feature into a collection named after the file.

Confirmation read: object count equals layer feature count, and sampled vertices read terrain heights.

## [12]-[SUN]

Sun Position places bound lamp and sky node of the context scene from every field on each field write:

```python
import bpy

scene = bpy.context.scene
world = scene.world or bpy.data.worlds.new("<World>")
scene.world = world
sky = next((node for node in world.node_tree.nodes if node.type == "TEX_SKY"), None) or world.node_tree.nodes.new("ShaderNodeTexSky")
world.node_tree.links.new(sky.outputs["Color"], next(node for node in world.node_tree.nodes if node.type == "BACKGROUND").inputs["Color"])
light = next((obj for obj in scene.objects if obj.type == "LIGHT" and obj.data.type == "SUN"), None)
if light is None:
    light = bpy.data.objects.new("<Sun>", bpy.data.lights.new("<Sun>", "SUN"))
    scene.collection.objects.link(light)
sun = scene.sun_pos_properties
sun.sun_object, sun.sky_texture = light, sky.name
```

- Day studies key `time` per frame (`sun.time = <hour>`, `sun.keyframe_insert("time", frame=<frame>)`)
- Sun Position's `frame_change_post` handler places the lamp per frame in background runs and GUI sessions
- `world.sunpos_create_analemmas_object` and `world.sunpos_create_surface_object` build sun path diagram at the origin, oriented by `north_offset`
- Writes to another scene's `sun_pos_properties` place its sun inside `bpy.context.temp_override(scene=<scene>)` alone
- `sun_object` takes the scene's `SUN` light, a script assignment accepting any object type

Confirmation read: after `view_layer.update()`, lamp +Z bearing `atan2(x, y)` equals sun azimuth plus grid north, its elevation equals `sun_elevation`, and sky node `sun_direction` matches the lamp.
