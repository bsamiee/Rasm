# [FILES]

Files on disk read through openNURBS outside Rhino, open as documents through macOS, and convert through headless documents no window shows.

## [01]-[READ]

`uv run --script <skill>/scripts/file3dm.py <file or folder>...` prints one JSON line per `.3dm` with no Rhino running, a folder standing for every `.3dm` under it, and exits 1 when any file fails to read:
- Records hold the layer and material shapes `describe(doc)` prints, with `version` the archive version, `edited_by`, and `edited`
- Counts, `min`, `max`, and `invalid` cover model-space objects outside block definitions, `invalid` maps an id to its `IsValidWithLog` text
- `open_by` holds user, computer, and time from the `<file>.rhl` lock Rhino writes beside a file a document holds open
- Locks outlive a crashed Rhino, `list_slots` and `describe(doc)` confirm which document holds the file
- Layer linetypes read `None`, rhino3dm crashes the interpreter on releasing a model whose linetype table it read
- `rhino3dm` exposes no view display mode, render settings walk, or section style, those read through `File3dm` inside Rhino
- `Fault` lines name files openNURBS cannot read, and Rhino opens none of them

```bash
# Paths of every .3dm under a folder in meters, then the layers of one file
uv run --script <skill>/scripts/file3dm.py <folder> | jq -r 'select(.units == "Meters") | .path'
uv run --script <skill>/scripts/file3dm.py <file> | jq -c '.layers[] | {path, objects}'
```

## [02]-[OPEN]

- `open -g -b com.mcneel.rhinoceros.9 <file>...` opens each file as a document with its own slot, Rhino stays behind the user's application
- Opened files become Rhino's active document, `documents()` lists each with its port and `describe(doc).path` names the file
- `RhinoDoc.Open` in a script returns `None`, opens the file after the call, and leaves an extra untitled document with a slot of its own

## [03]-[WRITE]

`export(doc, path, ids)` writes the document or the objects `ids` name, `convert(doc, sources, "<.suffix>", folder)` writes each source file as `<stem><suffix>` beside it or in `folder`:
- Suffixes pick the typed writer, an unknown one faults with every suffix a writer accepts
- STEP, IGES, SAT, and Parasolid targets leave out the object types their exporter drops and list those ids in `File.detail`
- Subsets, typed formats, and dropped types write from a headless copy holding the document's layers, materials, and blocks
- Blocks holding a dropped type write as their pieces, layout details stay out of every copy
- Saves keep an overwritten `.3dm` as `<name>.3dmbak` under `FileSettings.CreateBackupFiles`
- Exports keep an overwritten file as `<name>.<suffix>bak` under `FileSettings.CreateOtherBackupFiles`
- `FileWriteOptions` with `IncludeRenderMeshes` and `IncludeBitmapTable` True embeds render meshes and the textures of render content
- Materials reach Blender through `.glb`, OBJ, FBX, and USD lose materials or units
- `.3dm` gives Blender the layers, layer materials, and the texture files it embeds
- `.glb` exports write materials double-sided with their IOR, and Blender reads specular at half and emission in display values
- `.3dm` sources convert with their own units, other formats read into `doc`'s units, STEP scaled by the units it stores
- Missing, unreadable, and same-path sources fault alone, the rest of the batch converts
- Conversions run on Rhino's UI thread under the router's 5 minute limit, a large batch splits across calls
- Exports and headless copies leave document user text (`doc.Strings`) out while `Options/Advanced/ExportDocumentUserText` holds False
- Per-format export defaults sit in each export plugin's settings file and remember the last dialog choice

`load(doc, path, "<A>")` imports a file into the working document under layer `<A>`, the file's layers as its sublayers, unless `-_Options _Files _LayerImport` matches short names and merges them into same-named layers.

## [04]-[HEADLESS]

- `RhinoDoc.CreateHeadless(None)` makes an empty document in Millimeters with no window, `RhinoDoc.OpenHeadless(path)` opens a `.3dm` alone
- Headless documents write no lock file, stay out of `list_slots` and `documents()`, and leave the active document as it was
- `command` refuses a headless document, RhinoCommon and typed writers work in it
- `Dispose()` in a `finally` closes it
- Headless documents list no render environment of their file and take no named view, `File3dm.Read(path).RenderEnvironments` lists the file's
- `RhinoDoc.Create(None)` makes a document with views and no window that no close removes, work without a window takes `CreateHeadless`
- Headless opens of an unreadable `.3dm` crash the Rhino process, `convert` and `load` refuse one through `File3dm.Read` first
