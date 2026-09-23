# [LOOK_DEVELOPMENT]

Materials, worlds, lights, and library assets, each change read back through the material's users and slots and proved through a render.

## [01]-[MATERIALS]

Faces render the slot their `material_index` names:
- `mesh.materials.append(<material>)` adds a slot no face uses, and the render keeps the look of slot 0
- `obj.material_slots[<i>].material = <material>` changes the look of every face on slot `<i>`, `material.users` read after it proves the assignment

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

- Poly Haven packs every downloaded image into the `.blend`, a 4k texture set grows the file by its map sizes
- Normalized imports hold the factor in root object scale, `transform_apply(scale=True)` under a selection override bakes it into the mesh
- Download results name imported objects and bounds, the next call reads them by those names

## [03]-[WORLD_AND_LIGHT]

Metals reflect the world, under the flat gray default world a metal proof renders near black, its proofs run under an HDRI or sky:
- `ShaderNodeTexSky` defaults to `MULTIPLE_SCATTERING`, `sun_elevation` and `sun_rotation` place its sun
- Studio rigs come from `bpy.ops.light_it_up.<rig>(replace_existing=True, add_camera=False, fit_to_subject=True)` on the active object
- `discover("light_it_up")` lists the rigs and their control operators
- Use `references/geospatial.md` for a sun from a site and time

## [04]-[ASSET_LIBRARIES]

Asset libraries are folders of `.blend` files holding marked datablocks, with catalogs in `blender_assets.cats.txt` at the folder root:

```python
# [HEADLESS_CALL] Mark <Object> into catalog <path>, render its preview, and write it into the library at <library>
import uuid
from pathlib import Path

import bpy

library = Path("<library>")
library.mkdir(parents=True, exist_ok=True)
catalog = str(uuid.uuid5(uuid.NAMESPACE_URL, "<path>"))
(library / "blender_assets.cats.txt").write_text(f"VERSION 1\n{catalog}:<path>:<name>\n", encoding="utf-8")
obj = bpy.data.objects["<Object>"]
obj.asset_mark()
obj.asset_data.catalog_id, obj.asset_data.author, obj.asset_data.license = catalog, "<author>", "<license>"
obj.asset_data.tags.new("<tag>")
obj.asset_generate_preview()
bpy.ops.wm.save_as_mainfile(filepath=str(library / "<file>.blend"), copy=True)
result = {"preview": list(obj.preview.image_size), "catalog": catalog}
```

- `asset_generate_preview()` renders the preview inside the call in background runs, `asset_mark()` alone stores none
- Catalog lines take the form `<uuid>:<path>:<simple name>` after a `VERSION 1` line, one catalog per path
