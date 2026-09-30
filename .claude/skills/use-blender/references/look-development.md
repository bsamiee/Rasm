# [LOOK_DEVELOPMENT]

Materials, site sky and sun, added lights, downloaded and generated assets, and the asset library, each look judged in one render under site light.

## [01]-[MATERIALS]

New materials build in code and reach faces through slots, colors converted to scene linear and textures sized over UVs in meters:

```python
# [EXECUTE_BLENDER_CODE] ambientCG concrete on Slab and a painted color on Walls, both over UVs in meters
import bmesh
import bpy
from mathutils import Color

scene = bpy.context.scene
scene.ambientcg_material_name, scene.ambientcg_resolution, scene.ambientcg_format, scene.ambientcg_projection = "Concrete034", "1K", "JPG", "FLAT"
bpy.ops.material.fetch_and_create()
concrete = bpy.data.materials["Concrete034"]
next(n for n in concrete.node_tree.nodes if n.type == "MAPPING").inputs["Scale"].default_value = (1 / 1.10, 1 / 0.55, 1.0)
paint = bpy.data.materials.new("Paint")
linear = Color(tuple(int("D8D2C4"[i : i + 2], 16) / 255 for i in (0, 2, 4))).from_srgb_to_scene_linear()
next(n for n in paint.node_tree.nodes if n.type == "BSDF_PRINCIPLED").inputs["Base Color"].default_value = (*linear, 1.0)
paint.diffuse_color = (*linear, 1.0)
for name, material in (("Slab", concrete), ("Walls", paint)):
    obj = bpy.data.objects[name]
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    uv = bm.loops.layers.uv.verify()
    for face in bm.faces:
        axis = max(range(3), key=lambda i: abs(face.normal[i]))
        for loop in face.loops:
            loop[uv].uv = [loop.vert.co[i] for i in range(3) if i != axis]
    bm.to_mesh(obj.data)
    bm.free()
    obj.data.materials.clear()
    obj.data.materials.append(material)
result = {"images": {n.image.name: n.image.colorspace_settings.name for n in concrete.node_tree.nodes if n.type == "TEX_IMAGE"}, "paint": list(linear)}
```

- `material.fetch_and_create` builds the set the scene's `ambientcg_*` fields name, from the shared materials folder when it holds the set, else from ambientCG
- Sets arrive with Color in `sRGB`, Roughness, NormalGL, and Displacement in `Non-Color`, and a Mapping on UV at Scale 1
- UVs in meters take Mapping Scale 1 / set size in meters, `api/v2/full_json?id=<id>&include=dimensionsData` states ambientCG sizes in centimeters
- Box UVs in meters survive every exporter, where Object, Generated, and Box projections compute at render time alone
- New materials hold a Principled BSDF and a Material Output, each node found by `type` since names translate
- Byte colors convert through `from_srgb_to_scene_linear()`, and `diffuse_color` takes that value for Solid mode
- Principled inputs read `Subsurface Weight`, `Specular IOR Level`, `Transmission Weight`, `Coat Weight`, and `Emission Color`, white at Strength 0
- Color textures exported from Rhino read with Rhino's curve under `Gamma 2.2 Encoded Rec.709`, and normal maps read OpenGL, a set's `NormalGL` file
- Use interchange.md for Rhino materials through glTF and `.3dm`

Existing materials change in place, and a new slot takes faces through `material_index`:

```python
# [EXECUTE_BLENDER_CODE] Paint on Walls recolored, and its top faces given a second material in a new slot
import bmesh
import bpy
from mathutils import Color

obj = bpy.data.objects["Walls"]
paint = obj.material_slots[0].material
linear = Color(tuple(int("8A9A5B"[i : i + 2], 16) / 255 for i in (0, 2, 4))).from_srgb_to_scene_linear()
next(n for n in paint.node_tree.nodes if n.type == "BSDF_PRINCIPLED").inputs["Base Color"].default_value = (*linear, 1.0)
paint.diffuse_color = (*linear, 1.0)
cap = bpy.data.materials.new("Cap")
obj.data.materials.append(cap)
index = obj.material_slots.find(cap.name)
bm = bmesh.new()
bm.from_mesh(obj.data)
for face in bm.faces:
    face.material_index = index if face.normal.z > 0.99 else face.material_index
bm.to_mesh(obj.data)
bm.free()
evaluated = obj.evaluated_get(bpy.context.evaluated_depsgraph_get())
result = {"slots": [s.material.name for s in obj.material_slots], "faces": [sum(p.material_index == i for p in evaluated.data.polygons) for i in range(len(obj.material_slots))]}
```

- Slots link to the mesh under `edit.material_link` `OBDATA`, linked duplicates sharing them, and `material_slots[<i>].link = "OBJECT"` gives one object its own
- Material edits reach every object using that material, and `material.copy()` splits one off

## [02]-[LIBRARY_ASSETS]

`mcp-for-blender` fetches library and generated assets into the live scene, each source enabled on the scene first:
1. `get_addon_status` reads the protocol match, then `get_<source>_status` reads each source
2. Set `bpy.context.scene.blendermcp_use_<source>` True for each source the work takes, and False after it, saved files keeping the toggles
3. Search, download, then place and assign in code

Poly Haven answers with no account, Sketchfab and Poly Pizza with `BLENDERMCP_SKETCHFAB_API_KEY` and `BLENDERMCP_POLYPIZZA_API_KEY` in the GUI's login environment. Hyper3D answers status with the free-trial key `scene.blendermcp_hyper3d_api_key = "vibecoding"`, and generation answers `API_INSUFFICIENT_FUNDS` once the shared trial balance is spent. Hunyuan3D holds no account, and Tripo takes Premium.

Poly Haven textures:
1. `search_polyhaven_assets(query=, asset_type="textures", min_size_m=2)` for floors and walls, each row stating its real-world size
2. `download_polyhaven_asset(asset_id=, asset_type="textures", resolution="1k")` builds material `<id>` at 0 users with its images packed into the file
3. Assign it in code before a save drops it:

```python
# [EXECUTE_BLENDER_CODE] Downloaded Poly Haven material <id> on <Object> at its real-world size over UVs in meters
import bmesh
import bpy

obj, material = bpy.data.objects["<Object>"], bpy.data.materials["<id>"]
mapping = next(n for n in material.node_tree.nodes if n.type == "MAPPING")
mapping.inputs["Scale"].default_value = (*(1000 / mm for mm in material["polyhaven_scale_mm"]), 1.0)
material.displacement_method = "BUMP"
bm = bmesh.new()
bm.from_mesh(obj.data)
uv = bm.loops.layers.uv.verify()
for face in bm.faces:
    axis = max(range(3), key=lambda i: abs(face.normal[i]))
    for loop in face.loops:
        loop[uv].uv = [loop.vert.co[i] for i in range(3) if i != axis]
bm.to_mesh(obj.data)
bm.free()
obj.data.materials.clear()
obj.data.materials.append(material)
result = {"scale": list(mapping.inputs["Scale"].default_value), "users": material.users}
```

- `polyhaven_scale_mm` holds the span one repeat covers, and its POINT Mapping multiplies UVs, so UVs in meters take `1000 / mm`
- Downloads set `displacement_method` `BOTH` at Displacement Scale 0.1, and `BUMP` keeps relief in shading on a mesh with no subdivision

Poly Haven HDRIs:
1. `search_polyhaven_assets(query=, asset_type="hdris")`, then `download_polyhaven_asset(asset_id=, asset_type="hdris", resolution="1k")`
2. Downloads assign world `PolyHaven <id>` at Background Strength 1, and a replaced world keeps only its other users
3. Hold the site world first (`site = scene.world`) and reassign it after a trial
4. HDRI worlds kept for a render take Background Strength `2 ** -scene.view_settings.exposure`, their exposure-0 brightness under site exposure

Sketchfab, Poly Pizza, and Hyper3D models:
1. `search_sketchfab_models(query=)` or `search_polypizza_models(query=, licence="CC0")`, each row naming author and license
2. `download_sketchfab_model(uid=, target_size=)` scales the largest dimension to `target_size`, the subject's real largest dimension in meters
3. `download_polypizza_model(model_id=)` keeps the source's arbitrary scale, its root holding `polypizza_attribution`, `polypizza_id`, and `polypizza_licence`
4. Hyper3D runs `generate_hyper3d_model_via_text`, `poll_rodin_job_status`, and `import_generated_asset`, one object per job at a normalized size
5. Imports land in the active collection with their objects selected, a Sketchfab tree under an empty `Sketchfab_model` in `QUATERNION` rotation
6. Size, place, and file each import by its root:

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

- `bounds(depsgraph, drawn=True)` from `scene.py` gives each object's evaluated world corners, and a root's tree spans their union
- CC-BY models credit the author their search row names, and Sketchfab writes no license property

## [03]-[SITE_LIGHT]

Startup files hold the site sky, a `Sun` lamp Sun Position binds, AgX, and exposure -5.3, and files without them take geospatial.md's new-site sequence first. Its bound sky and lamp then take physical values:

```python
# [EXECUTE_BLENDER_CODE] Bound sky as the physical sky with no disc at the site elevation, the lamp at the disc's irradiance, and the site exposure
from math import degrees

import bpy

scene = bpy.context.scene
place = scene.sun_pos_properties
sky = scene.world.node_tree.nodes[place.sky_texture]
sky.sky_type, sky.sun_disc, sky.altitude = "MULTIPLE_SCATTERING", False, <elevation>
lamp = place.sun_object.data
lamp.energy, lamp.angle = 139.3, lamp.bl_rna.properties["angle"].default
scene.view_settings.view_transform, scene.view_settings.exposure = "AgX", -5.3
scene.cycles.sample_clamp_indirect = 10 / 2**scene.view_settings.exposure
result = {"sun": [round(degrees(sky.sun_rotation), 3), round(degrees(sky.sun_elevation), 3)], "lamp": [lamp.energy, round(degrees(lamp.angle), 3)], "clamp": round(scene.cycles.sample_clamp_indirect, 1)}
```

- `<elevation>` is the site's height above sea level in meters
- Sky light reaches both engines through the world Background, and the SUN lamp alone carries the sun at 139.3 W/m², the disc's irradiance
- Sun to sky on a horizontal plane reads 7.9:1 under these values
- `angle` at its RNA default is the sun's 0.526°, and a stock point light converted to `SUN` keeps 11.4°
- Light handles taken before `type = "SUN"` stay a `PointLight`, and a read from `bpy.data.lights` after it reaches `angle`
- EEVEE turns world light above `world.sun_threshold` (10) into a sun of its own, a bright HDRI included
- Metals reflect the world, and the site sky gives them their look

Added lights take power for the brightness they give under scene exposure:

```python
# [EXECUTE_BLENDER_CODE] 2' x 4' ceiling panel lighting the floor 10' below it at half display white under the scene exposure
from math import pi

import bpy

FOOT = 0.3048
scene = bpy.context.scene
panel = bpy.data.lights.new("Panel", "AREA")
panel.shape, panel.size, panel.size_y = "RECTANGLE", 2 * FOOT, 4 * FOOT
fixture = bpy.data.objects.new("Panel", panel)
fixture.location = (12 * FOOT, 8 * FOOT, 10 * FOOT)
bpy.data.collections["Room"].objects.link(fixture)
distance, shown = fixture.location.z - 8 * 0.0254, 0.5
panel.energy = pi * distance**2 * pi * shown * 2**-scene.view_settings.exposure
result = {"energy": round(panel.energy, 1)}
```

- White surfaces read display white at irradiance `pi * 2 ** -exposure` W/m², 124 W/m² at -5.3
- AREA lights facing a surface `d` away give irradiance `energy / (pi * d**2)`, POINT and SPOT lights `energy / (4 * pi * d**2)`
- Fixtures at catalog power read near black beside the sun under -5.3, holding their physical ratio
- Material Preview lights with the look-development studio HDRI at exposure 0, and a render judges the site look
- Use rendering.md for look renders and their readings

## [04]-[ASSET_LIBRARY]

Assets join the shared `Assets` library as one `.blend` per set beside the library's catalog file, written from a `start` session on the saved file:

```python
# [HEADLESS_CALL] Columns marked into catalog Structure/Columns with a preview, written to <set>.blend in the Assets library
from itertools import accumulate
from pathlib import Path
import uuid

import bpy

library = Path(bpy.context.preferences.filepaths.asset_libraries["Assets"].path)
catalogs = {level: str(uuid.uuid5(uuid.NAMESPACE_URL, level)) for level in accumulate("Structure/Columns".split("/"), lambda head, name: f"{head}/{name}")}
definitions = library / "blender_assets.cats.txt"
try:
    held = definitions.read_text(encoding="utf-8").splitlines()
except FileNotFoundError:
    held = ["VERSION 1"]
entries = (f"{identity}:{path}:{path.replace('/', '-')}" for path, identity in catalogs.items())
definitions.write_text("".join(f"{entry}\n" for entry in dict.fromkeys((*held, *entries))), encoding="utf-8")
assets = {bpy.data.objects[name] for name in ("Columns",)}
for asset in assets:
    asset.asset_mark()
    asset.asset_data.catalog_id, asset.asset_data.author, asset.asset_data.license = catalogs["Structure/Columns"], "<author>", "<license>"
    asset.asset_data.tags.new("column")
    asset.asset_generate_preview()
bpy.data.libraries.write(str(library / "<set>.blend"), assets, fake_user=True)
result = {"file": str(library / "<set>.blend"), "previews": {a.name: list(a.preview.image_size) for a in assets}}
```

- Sessions read the user's library rows, `filepaths.asset_libraries["Assets"].path` naming the shared folder
- Catalog lines read `<uuid>:<path>:<simple name>` after a `VERSION 1` line, one per path level, a uuid5 of each path keeping it stable
- Background processes render each preview inside the call, to the same pixels under any scene exposure
- `libraries.write` writes the given IDs with their dependencies, asset data, and previews, replacing its whole target file
- Interface planting rebuilds write their catalogs beside every line the file holds and keep `assets.blend` to themselves
- Remote rows (`CGMatter`, `ambientCG`) fetch each asset on first use from the Asset Browser
