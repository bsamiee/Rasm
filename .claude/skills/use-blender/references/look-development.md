# [LOOK_DEVELOPMENT]

Site sky and sun, added lights, materials, downloaded and generated assets, and the asset library, each look judged in one render under site light.

## [01]-[SITE_LIGHT]

Startup files hold the site sky, the `Sun` light Sun Position binds, AgX, and exposure -5.3, other files taking the geospatial new-site snippet first:

```python
# [EXECUTE_BLENDER_CODE] Bound sky as the physical sky with no disc at site elevation, the light at the disc's irradiance, and the site exposure
from math import degrees

import bpy

scene = bpy.context.scene
place = scene.sun_pos_properties
sky = scene.world.node_tree.nodes[place.sky_texture]
sky.sky_type, sky.sun_disc, sky.altitude = "MULTIPLE_SCATTERING", False, <elevation>
light = place.sun_object.data
light.energy, light.angle = 139.3, light.bl_rna.properties["angle"].default
scene.view_settings.view_transform, scene.view_settings.exposure = "AgX", -5.3
scene.cycles.sample_clamp_indirect = 10 / 2**scene.view_settings.exposure
result = {"sun": [round(degrees(sky.sun_rotation), 3), round(degrees(sky.sun_elevation), 3)], "light": [light.energy, round(degrees(light.angle), 3)], "clamp": round(scene.cycles.sample_clamp_indirect, 1)}
```

- `<elevation>` is the site's height above sea level in meters
- Sky light reaches both engines through the world Background, sunlight from the SUN light alone at 139.3 W/m², the disc's irradiance
- Sun to sky on a horizontal plane reads 7.9:1 under these values, and -5.3 maps sunlit white ground to display white
- `sample_clamp_indirect` at 394 keeps the factory clamp at 10 times display white, bounce light off sunlit ground reaching shadowed faces
- `angle` at its RNA default is the sun's 0.526°, the stock point light converted to `SUN` keeping 11.4°
- Light handles read after `type = "SUN"` from `bpy.data.lights` reach `angle`, a handle taken before it staying a `PointLight`
- EEVEE turns world light above `world.sun_threshold` (10) into a sun of its own, a bright HDRI included
- Material Preview lights with the look-development studio HDRI at exposure 0, and a render judges the site look

Use rendering.md for look renders and their readings.

## [02]-[ADDED_LIGHTS]

Added lights take the power giving a surface `<surface>` m up the fraction `shown` of display white under the scene exposure:

```python
# [EXECUTE_BLENDER_CODE] Area light <name> in <Collection> lighting the surface below it at half display white under the scene exposure
from math import pi

import bpy

scene = bpy.context.scene
panel = bpy.data.lights.new("<name>", "AREA")
panel.shape, panel.size, panel.size_y = "RECTANGLE", <width>, <depth>
fixture = bpy.data.objects.new("<name>", panel)
fixture.location = (<x>, <y>, <z>)
bpy.data.collections["<Collection>"].objects.link(fixture)
distance, shown = fixture.location.z - <surface>, 0.5
panel.energy = pi * distance**2 * pi * shown * 2**-scene.view_settings.exposure
result = {"energy": round(panel.energy, 1)}
```

- White surfaces read display white at irradiance `pi * 2 ** -exposure` W/m², 124 W/m² at -5.3
- AREA lights facing a surface `d` away give irradiance `energy / (pi * d**2)`, POINT and SPOT lights `energy / (4 * pi * d**2)`
- Fixtures at catalog power read near black beside the sun under -5.3, holding their physical ratio

## [03]-[MATERIALS]

Materials reach faces through slots on meshes holding UVs in meters:
1. Write box UVs once per mesh along each face's dominant axis
2. Build new materials in code, colors converted to scene linear and textures scaled to their set size
3. Change existing materials in place, a new slot taking faces through `material_index`

```python
# [EXECUTE_BLENDER_CODE] Box UVs in meters on <Object> meshes
import bmesh
import bpy

meshes = {name: bpy.data.objects[name].data for name in ("<Object>",)}
for mesh in meshes.values():
    bm = bmesh.new()
    bm.from_mesh(mesh)
    uv = bm.loops.layers.uv.verify()
    for face in bm.faces:
        axis = max(range(3), key=lambda i: abs(face.normal[i]))
        for loop in face.loops:
            loop[uv].uv = [loop.vert.co[i] for i in range(3) if i != axis]
    bm.to_mesh(mesh)
    bm.free()
result = {"uv_layers": {name: mesh.uv_layers.active.name for name, mesh in meshes.items()}}
```

- UVs read local mesh coordinates, meters in the world for an object at scale 1
- Box UVs reach every exporter, Object, Generated, and Box projections computing at render time alone

```python
# [EXECUTE_BLENDER_CODE] ambientCG set <Set> of <width> by <height> m on <Object> and color <RRGGBB> as <Material> on <Other>
import bpy
from mathutils import Color

scene = bpy.context.scene
scene.ambientcg_material_name, scene.ambientcg_resolution, scene.ambientcg_format, scene.ambientcg_projection = "<Set>", "1K", "JPG", "FLAT"
bpy.ops.material.fetch_and_create()
textured = bpy.data.materials["<Set>"]
next(n for n in textured.node_tree.nodes if n.type == "MAPPING").inputs["Scale"].default_value = (1 / <width>, 1 / <height>, 1.0)
painted = bpy.data.materials.new("<Material>")
linear = Color(tuple(int("<RRGGBB>"[i : i + 2], 16) / 255 for i in (0, 2, 4))).from_srgb_to_scene_linear()
next(n for n in painted.node_tree.nodes if n.type == "BSDF_PRINCIPLED").inputs["Base Color"].default_value = (*linear, 1.0)
painted.diffuse_color = (*linear, 1.0)
for name, material in (("<Object>", textured), ("<Other>", painted)):
    materials = bpy.data.objects[name].data.materials
    materials.clear()
    materials.append(material)
result = {"images": {n.image.name: n.image.colorspace_settings.name for n in textured.node_tree.nodes if n.type == "TEX_IMAGE"}, "paint": list(linear)}
```

- `material.fetch_and_create` builds the set `ambientcg_*` scene fields name from the `design-tools/materials` `cache_dir`, else downloads it
- Sets arrive with Color in `sRGB`, Roughness, NormalGL, and Displacement in `Non-Color`, a POINT Mapping on UV at Scale 1, and `BUMP` at Scale 0.01
- UVs in meters take Mapping Scale 1 / set size in meters, `api/v2/full_json?id=<Set>&include=dimensionsData` stating ambientCG sizes in centimeters
- New materials hold a Principled BSDF and a Material Output, each node found by `type` since names translate
- Byte colors convert through `from_srgb_to_scene_linear()`, and `diffuse_color` takes that value for Solid mode
- Principled inputs read `Subsurface Weight`, `Specular IOR Level`, `Transmission Weight`, `Coat Weight`, and `Emission Color`, white at Strength 0

```python
# [EXECUTE_BLENDER_CODE] <Object>'s first material recolored to <RRGGBB>, its top faces given <Material> in a new slot
import bmesh
import bpy
from mathutils import Color

obj = bpy.data.objects["<Object>"]
held = obj.material_slots[0].material
linear = Color(tuple(int("<RRGGBB>"[i : i + 2], 16) / 255 for i in (0, 2, 4))).from_srgb_to_scene_linear()
next(n for n in held.node_tree.nodes if n.type == "BSDF_PRINCIPLED").inputs["Base Color"].default_value = (*linear, 1.0)
held.diffuse_color = (*linear, 1.0)
added = bpy.data.materials.new("<Material>")
obj.data.materials.append(added)
index = obj.material_slots.find(added.name)
bm = bmesh.new()
bm.from_mesh(obj.data)
for face in bm.faces:
    face.material_index = index if face.normal.z > 0.99 else face.material_index
bm.to_mesh(obj.data)
bm.free()
evaluated = obj.evaluated_get(bpy.context.evaluated_depsgraph_get())
result = {"slots": [s.material.name for s in obj.material_slots], "faces": [sum(p.material_index == i for p in evaluated.data.polygons) for i in range(len(obj.material_slots))]}
```

- Slots link to the mesh under `edit.material_link` `OBDATA`, shared by linked duplicates, and `material_slots[<i>].link = "OBJECT"` splits one off
- Material edits reach every object using that material, and `material.copy()` splits one off

Use interchange.md for Rhino materials through glTF and `.3dm`.

## [04]-[DOWNLOADED_ASSETS]

`mcp-for-blender` fetches library and generated assets into the live scene, each source enabled on the scene first:
1. `get_<source>_status` reads each source
2. Set `bpy.context.scene.blendermcp_use_<source>` True in a `blender` call for each source the work takes, and False after it
3. Search, download, then place and assign in code

- Asset tools answer `Unknown command type` while the active scene's toggle reads False, and saved files keep the toggles
- Precision work builds from dimensions in code and CAD Sketcher and Bonsai operators, library assets serving props and context

Service keys come from add-on preferences, then the Scene, then a `BLENDERMCP_*` variable in the GUI login environment:
- Poly Haven answers with no account, Sketchfab and Poly Pizza with `BLENDERMCP_SKETCHFAB_API_KEY` and `BLENDERMCP_POLYPIZZA_API_KEY`
- Hyper3D takes the trial key `scene.blendermcp_hyper3d_api_key = "vibecoding"` until generation answers `API_INSUFFICIENT_FUNDS`
- Hunyuan3D holds no account, and Tripo takes Premium
- Sidebar key fields write add-on preferences, the Hyper3D trial key alone going into a Scene property every saved `.blend` holds
- Files leaving the machine clear `blendermcp_*_api_key`, `blendermcp_hunyuan3d_secret_id`, and `blendermcp_hunyuan3d_secret_key` first

Poly Haven textures assign in code onto a mesh holding UVs in meters:
1. `search_polyhaven_assets(query=, asset_type="textures", min_size_m=2)` for floors and walls, each row stating its real-world size
2. `get_polyhaven_asset_preview(asset_id=)` shows a row's thumbnail before its download
3. `download_polyhaven_asset(asset_id=, asset_type="textures", resolution="1k")` builds material `<id>` at 0 users, images packed
4. Assign it in the next call, before a save drops it:

```python
# [EXECUTE_BLENDER_CODE] Downloaded Poly Haven material <id> on <Object> at its real-world size
import bpy

materials, material = bpy.data.objects["<Object>"].data.materials, bpy.data.materials["<id>"]
mapping = next(n for n in material.node_tree.nodes if n.type == "MAPPING")
mapping.inputs["Scale"].default_value = (*(1000 / mm for mm in material["polyhaven_scale_mm"]), 1.0)
material.displacement_method = "BUMP"
materials.clear()
materials.append(material)
result = {"scale": list(mapping.inputs["Scale"].default_value), "users": material.users}
```

- `polyhaven_scale_mm` holds the span one repeat covers, and its POINT Mapping multiplying UVs takes `1000 / mm` over UVs in meters
- Downloads set `displacement_method` `BOTH` at Displacement Scale 0.1, and `BUMP` keeps relief in shading on a mesh with no subdivision

Poly Haven HDRIs replace the scene world:
1. Read the site world's name from `scene.world.name` before the download
2. `search_polyhaven_assets(query=, asset_type="hdris")`, then `download_polyhaven_asset(asset_id=, asset_type="hdris", resolution="1k")`
3. Downloads assign world `PolyHaven <id>` at Background Strength 1, the replaced world keeping its other users
4. HDRI worlds kept for a render take Background Strength `2 ** -scene.view_settings.exposure`, their exposure-0 brightness
5. `scene.world = bpy.data.worlds["<site world>"]` returns the site world after a trial

Sketchfab, Poly Pizza, and Hyper3D models import under a root that places them:
1. `search_sketchfab_models(query=)` or `search_polypizza_models(query=, licence="CC0")`, each row naming author and license
2. `get_sketchfab_model_preview(uid=)` shows a Sketchfab row's thumbnail before its download
3. `download_sketchfab_model(uid=)` or `download_polypizza_model(model_id=, normalize_size=True)` take `target_size=` in meters for the longest side
4. Poly Pizza roots hold `polypizza_attribution`, `polypizza_id`, and `polypizza_licence`, Sketchfab roots no license
5. Hyper3D runs `generate_hyper3d_model_via_text`, `poll_rodin_job_status`, and `import_generated_asset`, one object per job at a normalized size
6. Imports go into the active collection with their objects selected, a Sketchfab tree under an empty `Sketchfab_model` in `QUATERNION` rotation
7. Size, place, and file each import by its root:

```python
# [EXECUTE_BLENDER_CODE] Imported model under <Root> scaled to <height> m tall, set on the floor at <x>, <y>, <z>, and moved into <Collection>
import bpy
import numpy as np
from mathutils import Vector
from scene import bounds

root = bpy.data.objects["<Root>"]
tree = [root, *root.children_recursive]
names = {o.name for o in tree}


def extent() -> tuple[np.ndarray, np.ndarray]:
    boxes = np.array([box for name, box in bounds(bpy.context.evaluated_depsgraph_get(), drawn=True).items() if name in names])
    return boxes[:, 0].min(axis=0), boxes[:, 1].max(axis=0)


low, high = extent()
root.scale *= <height> / (high[2] - low[2])
low, high = extent()
root.location += Vector((<x>, <y>, <z>)) - Vector(((low[0] + high[0]) / 2, (low[1] + high[1]) / 2, low[2]))
owner = bpy.data.collections["<Collection>"]
for obj in tree:
    for held in obj.users_collection:
        held.objects.unlink(obj)
    owner.objects.link(obj)
low, high = extent()
result = {"low": low.tolist(), "high": high.tolist()}
```

## [05]-[ASSET_LIBRARY]

Assets join the shared `Assets` library as one `.blend` per set beside the library's catalog file, written from a `start` session on the saved file:

```python
# [HEADLESS_CALL] <Object> marked into catalog <A>/<B> with a preview, written to <set>.blend in the Assets library
from itertools import accumulate
from pathlib import Path
import uuid

import bpy

library = Path(bpy.context.preferences.filepaths.asset_libraries["Assets"].path)
catalogs = {level: str(uuid.uuid5(uuid.NAMESPACE_URL, level)) for level in accumulate("<A>/<B>".split("/"), lambda head, name: f"{head}/{name}")}
definitions = library / "blender_assets.cats.txt"
held = definitions.read_text(encoding="utf-8").splitlines() if definitions.exists() else ["VERSION 1"]
entries = (f"{identity}:{path}:{path.replace('/', '-')}" for path, identity in catalogs.items())
definitions.write_text("".join(f"{entry}\n" for entry in dict.fromkeys((*held, *entries))), encoding="utf-8")
assets = {bpy.data.objects[name] for name in ("<Object>",)}
for asset in assets:
    asset.asset_mark()
    asset.asset_data.catalog_id, asset.asset_data.author, asset.asset_data.license = catalogs["<A>/<B>"], "<author>", "<license>"
    asset.asset_data.tags.new("<tag>")
    asset.asset_generate_preview()
bpy.data.libraries.write(str(library / "<set>.blend"), assets, fake_user=True)
result = {"file": str(library / "<set>.blend"), "previews": {a.name: list(a.preview.image_size) for a in assets}}
```

- Sessions read the user's library rows, `filepaths.asset_libraries["Assets"].path` naming the shared `design-tools/assets` folder
- Catalog lines read `<uuid>:<path>:<simple name>` after a `VERSION 1` line, one per path level, a uuid5 of each path keeping it stable
- Background processes render each 128 px preview inside the call, to the same pixels under any scene exposure
- `libraries.write` writes the given IDs with their dependencies, asset data, and previews, replacing its whole target file
- `tools/interface/blender/script/library.py` writes site planting into `assets.blend` and keeps every catalog line the file holds
- Remote rows (`CGMatter`, `ambientCG`) fetch each asset on first use from the Asset Browser
