# [FILES]

Files on disk read through openNURBS outside Rhino, open as documents through macOS, and convert through headless documents no window shows.

## [01]-[READ]

`uv run --script <skill>/scripts/file3dm.py <file or folder>...` prints one JSON line per `.3dm` with no Rhino running, a folder standing for every `.3dm` under it, and exits 1 when any file fails to read:
- Records hold layer and material shapes `describe(doc)` prints, `version` the archive version, `edited_by`, and `edited` in UTC
- `page_units` and annotation `styles` sit beside model units, and `layouts` lists pages apart from model `views`
- Counts, `min`, and `max` cover model-space objects outside block definitions
- `open_by` holds user, computer, and time from the `<file>.rhl` lock Rhino writes beside a file a document holds open
- Locks outlive a crashed Rhino, `list_slots` and `describe(doc)` name the document holding the file
- Layer linetypes read `None`, rhino3dm crashes the interpreter on releasing a model whose linetype table it read
- `rhino3dm` exposes no view display mode, render settings walk, or section style, and `File3dm` inside Rhino reads each

```bash
# Paths in meters, then drawing files under a folder, then the layers of one file
uv run --script <skill>/scripts/file3dm.py <folder> | jq -r 'select(.units == "Meters") | .path'
uv run --script <skill>/scripts/file3dm.py <folder> | jq -r 'select(.layouts | length > 0) | .path'
uv run --script <skill>/scripts/file3dm.py <file> | jq -c '.layers[] | {path, objects}'
```

## [02]-[OPEN]

- `open -g -b com.mcneel.rhinoceros.9 <file>...` opens each file as a document with its own slot, Rhino stays behind the user's application
- Opened files become Rhino's active document, `documents()` lists each with its port and `describe(doc).path` names the file
- `RhinoDoc.Open` in a script returns `None`, opens its file after the call, and leaves an extra untitled document with a slot of its own

## [03]-[WRITE]

`export(doc, path, ids)` writes the document or objects `ids` names, `convert(doc, sources, "<.suffix>", folder)` writes each source file as `<stem><suffix>` beside it or in `folder`:
- Suffixes pick the typed writer, an unknown one faults with every suffix a writer accepts
- STEP, IGES, SAT, and Parasolid targets leave out object types their exporter drops and list the dropped ids in `File.detail`
- Subsets, typed formats, and dropped types write from a headless copy holding the document's layers, materials, and blocks
- Blocks holding a dropped type write as their pieces, layout details stay out of every copy
- DWG and DXF write solids as ACIS solids, layer paths flatten to one level joined by `$`, and black layers write white
- Saves keep an overwritten `.3dm` as `<name>.3dmbak` under `FileSettings.CreateBackupFiles`
- Exports keep an overwritten file as `<name>.<suffix>bak` under `FileSettings.CreateOtherBackupFiles`
- `FileWriteOptions` with `IncludeRenderMeshes` and `IncludeBitmapTable` True embeds render meshes and the textures of render content
- `.glb` exports write materials double-sided with their IOR
- `.3dm` sources convert with their own units, other formats read into `doc`'s units, STEP scaled by the units it stores
- STEP reads onto layer Default, its shapes as they are, a file holding a block as one instance of a nested block `Document` that holds every shape
- Missing, unreadable, and same-path sources, and sources sharing a target stem, fault alone, and every other source converts
- Large `convert` batches split across calls, each within the router limit
- Exports and headless copies leave document user text (`doc.Strings`) out while `Options/Advanced/ExportDocumentUserText` holds False
- Per-format export defaults sit in each export plugin's settings file and remember the last dialog choice

`load(doc, path, "<A>")` imports a file into `doc` under layer `<A>`, the file's layers as its sublayers, unless `-_Options _Files _LayerImport` matches short names and merges them into same-named layers:
- Block definition layers join `<A>`, and a block `doc` names takes `doc`'s definition
- New block definitions nothing references drop

Use `use-blender` for Blender imports and IFC files.

## [04]-[HEADLESS]

- `RhinoDoc.CreateHeadless(None)` makes an empty document in Millimeters with no window, `RhinoDoc.OpenHeadless(path)` opens a `.3dm` alone
- Headless documents take RhinoCommon and typed writers
- Headless documents write no lock file, stay out of `list_slots` and `documents()`, and leave the active document as it was
- Headless documents list no render environment of their file and take no named view, `File3dm.Read(path).RenderEnvironments` lists the file's
- `RhinoDoc.Create(None)` makes a document with views and no window that no close removes, work without a window takes `CreateHeadless`
- Headless opens of an unreadable `.3dm` crash the Rhino process, `convert` and `load` refuse one through `File3dm.Read` first
