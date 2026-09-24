# [LOOK_DEVELOPMENT]

Materials, worlds, lights, and library assets, each change read back through the material's users and slots and proved through a render.

## [01]-[MATERIALS]

Faces render the slot their `material_index` names:
- `mesh.materials.append(<material>)` adds a slot no face uses, and the render keeps the look of slot 0
- `obj.material_slots[<i>].material = <material>` changes the look of every face on slot `<i>`, `material.users` read after it proves the assignment
- New materials are Principled BSDF with `use_nodes` on, unassigned faces render 0.8 gray, `edit.material_link` OBDATA links materials to the mesh
- Principled inputs, `Material.diffuse_color`, `Object.color`, and world and node colors hold scene linear values
- `mathutils.Color((r / 255, g / 255, b / 255)).from_srgb_to_scene_linear()` converts a hex color for them
- Byte / 255 written unconverted draws lighter, a 180 byte draws near 218
- `Material.diffuse_color` colors Solid mode alone and follows no Base Color change, a built material sets both
- Fresh Principled BSDFs hold `Emission Color` white at `Emission Strength` 0, a strength above 0 glows white until the color is set
- Data images (roughness, metallic, normal, height, opacity, occlusion) take `colorspace_settings.name = "Non-Color"`, color images `sRGB`
- `Gamma 2.2 Encoded Rec.709` among the image color spaces reads a color texture with Rhino's own curve
- Normal Map nodes read the OpenGL convention, a texture set's `NormalGL` file is the one to load
- Object, Generated, and Box projections compute at render time, UVs scaled so one unit is one declared length survive every exporter

## [02]-[LIBRARIES]

Library tools of `mcp-for-blender` download, build, and pack in one call, each with a trap its result states and the action that closes it:

| [INDEX] | [SOURCE]         | [TRAP]                                                     | [ACTION]                                             |
| :-----: | :--------------- | :--------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | Poly Haven maps  | Material named after the asset id with zero users          | `material_slots[<i>].material` in code before a save |
|  [02]   | Poly Haven HDRI  | New world per call, the previous world left with no user   | Delete the orphan by name                            |
|  [03]   | Poly Haven scale | Texture spans `scale_mm` in the world on a `POINT` mapping | `Scale` = surface meters / texture width on 0-1 UVs  |
|  [04]   | Sketchfab        | Required `target_size` becomes the largest dimension       | Real size of the subject in meters                   |
|  [05]   | Poly Pizza       | Arbitrary scale and origin, CC-BY on most models           | `normalize_size=True`, relay `polypizza_attribution` |
|  [06]   | Hyper3D          | Free trial key with a daily quota, normalized size         | One object per job, placed from `world_bounding_box` |

- Hunyuan3D holds no account, and its tools fail
- Poly Haven packs every downloaded image into the `.blend`, a 4k texture set grows the file by its map sizes
- Importer of ambientCG downloads a set (Color, Roughness, NormalGL, NormalDX, Displacement) into its `cache_dir`, the shared materials folder
- Normalized imports hold the factor in root object scale, `transform_apply(scale=True)` under a selection override bakes it into the mesh
- Download results name imported objects and bounds, the next call reads them by those names

## [03]-[WORLD_AND_LIGHT]

Metals reflect the world, under the flat gray default world a metal proof renders near black, its proofs run under an HDRI or sky:
- `ShaderNodeTexSky` defaults to `MULTIPLE_SCATTERING`, `sun_elevation` and `sun_rotation` place its sun, `altitude` is meters above sea level
- `sun_disc` lights Cycles alone, site light pairs the sky with `sun_disc` False and a SUN lamp at the disc's irradiance
- Sun to sky on a horizontal plane is 7.4:1 under the physical sky, a 4 W/m² sun reads as overcast
- EEVEE turns world light above `world.sun_threshold` (10 by default) into a sun of its own, 0 turns the extraction off
- Lights changed to `SUN` read again from `bpy.data.lights`, a handle taken before the change stays a point light
- SUN lamps converted from the stock point light keep an 11.4° angle, `light.angle = light.bl_rna.properties["angle"].default` gives 0.526°
- Cycles has `Object.is_shadow_catcher` and EEVEE no catcher, a ground catcher is a per-scene object
- `bpy.ops.preferences.studiolight_install` copies a file into the user `studiolights/<type>` folder, `studio_lights.load` lasts one session
- `shading.studio_light` names a `WORLD` light for Material Preview, `use_scene_world_render` and `use_scene_lights_render` on show the site sky
- Use `references/geospatial.md` for a sun from a site and time

## [04]-[ASSET_LIBRARIES]

Asset libraries are folders of `.blend` files holding marked datablocks, with catalogs in `blender_assets.cats.txt` at the folder root:

```python
# [HEADLESS_CALL] Mark <Object> into catalog <path>, render its preview, and write it alone into <file>.blend of the library at <library>
import uuid
from pathlib import Path

import bpy

library = Path("<library>")
library.mkdir(parents=True, exist_ok=True)
catalog = str(uuid.uuid5(uuid.NAMESPACE_URL, "<path>"))
cats, line = library / "blender_assets.cats.txt", f"{catalog}:<path>:<name>"
held = cats.read_text(encoding="utf-8").splitlines() if cats.exists() else ["VERSION 1"]
cats.write_text("\n".join(held if line in held else [*held, line]) + "\n", encoding="utf-8")
obj = bpy.data.objects["<Object>"]
obj.asset_mark()
obj.asset_data.catalog_id, obj.asset_data.author, obj.asset_data.license = catalog, "<author>", "<license>"
obj.asset_data.tags.new("<tag>")
obj.asset_generate_preview()
bpy.data.libraries.write(str(library / "<file>.blend"), {obj}, fake_user=True)
result = {"preview": list(obj.preview.image_size), "catalog": catalog}
```

- `asset_generate_preview()` renders the preview inside the call in background runs, `asset_mark()` alone stores none
- `bpy.data.libraries.write` writes the given IDs and their dependencies with asset data and previews, replacing the whole target file
- Catalog lines take the form `<uuid>:<path>:<simple name>` after a `VERSION 1` line, one catalog per path
- Library rows in `preferences.filepaths.asset_libraries` need a folder with a catalog file, `asset_libraries.remove(<row>)` drops one
