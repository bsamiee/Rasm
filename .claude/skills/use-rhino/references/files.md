# [FILES]

`.3dm` files read on disk without Rhino, and documents save, close, export, convert, and import through `document.py` entry points.

## [01]-[READ]

`uv run --script <skill>/scripts/file3dm.py <file or folder>...` prints one JSON line per `.3dm`, a folder standing for every `.3dm` under it, and exits 1 when any file faults:
- First runs build `rhino3dm` from source into the uv cache, a Bash call with `run_in_background` holds one
- Records hold the layer and material shapes `describe(doc)` prints, `version` the archive version, `edited_by`, and `edited` in UTC
- `page_units` and annotation `styles` sit beside model units, `layouts` lists pages holding page-space objects and `views` every other view
- Counts, `min`, and `max` cover model-space objects outside block definitions
- `materials` lists physically based materials the file saved, a material no layer or object uses stays out of a saved file
- `open_by` holds user, computer, and open time from the `<file>.rhl` lock Rhino writes beside a file it holds, a lock outlives a quit or crash
- Lines holding `items` with `source` `File3dm` name files Rhino opens behind a modal alert holding every close in the process
- Layer linetypes, view display modes, render settings, and section styles read through RhinoCommon `File3dm.Read(path)` inside Rhino

```bash
# Paths in meters, then files holding layouts, then the layers of one file
uv run --script <skill>/scripts/file3dm.py <folder> | jq -r 'select(.units == "Meters") | .path'
uv run --script <skill>/scripts/file3dm.py <folder> | jq -r 'select(.layouts | length > 0) | .path'
uv run --script <skill>/scripts/file3dm.py <file> | jq -c '.layers[] | {path, objects}'
```

## [02]-[SAVE_AND_CLOSE]

Titled documents autosave into their own file when Rhino leaves the front and every 5 idle minutes, and `save` and `close` write on demand:
- `save(doc)` writes the document's file through its window's `NSDocument`, `save(doc, path)` making `path` the document's file
- `doc.Save`, `SaveAs`, and `WriteFile` onto a windowed document's own file make macOS reopen it and drop later edits
- `save` runs in its own call after the edits, a `save` inside the editing call leaving the document modified
- `close(doc)` saves a titled document's unsaved edits and discards an untitled document's
- Relative paths of `save`, `export`, and `convert` land under `.artifacts/rhino`

## [03]-[EXPORT]

`export(doc, path, ids)` writes the document's model space or objects `ids` through the suffix's writer and returns `File` with `detail` the ids the format drops:
- Unknown suffixes fault with every suffix a writer accepts, a target equal to the document's own file faults
- `.3dm` of a whole document keeps layouts, every other write holds the chosen objects with their layers, materials, and blocks
- STEP and IGES drop meshes and annotations, SAT meshes, Parasolid meshes, curves, and points, a block holding one writing as its other pieces
- Writers take default options with every dialog and prompt suppressed, ignoring options an export plugin's settings remember
- DWG and DXF write solids as ACIS solids, flatten layer paths to one level joined by `$`, and write black layers white
- STEP writes AP203 in the document's units, `.glb` writes materials double-sided with their IOR
- STL writes model-unit coordinates that every STL read takes as millimeters
- `FileWriteOptions` with `IncludeRenderMeshes` and `IncludeBitmapTable` True embeds render meshes and the textures of render content
- Exports and headless copies leave document user text (`doc.Strings`) out while `Options/Advanced/ExportDocumentUserText` holds False

## [04]-[CONVERT_AND_IMPORT]

`convert(doc, sources, "<.suffix>", folder)` writes each source as `<stem><suffix>` in `folder`, `.artifacts/rhino` unless named, through a headless document per source:
- Sources read through their typed reader, else through `RhinoDoc.Import` for every suffix an import plugin lists (`.glb`)
- `.3dm` sources keep their own units, files storing units (STEP, DWG, glTF) scale into `doc`'s units, OBJ reads in `doc`'s units
- STEP reads onto layer Default as one instance of a nested block `Document` holding every shape
- Missing, unreadable, and same-path sources, and sources sharing a target stem, fault alone, and every other source converts
- One format per call and one folder per source format keep stems apart and each call within the router limit

`load(doc, path, "<A>")` imports a file into `doc` with every layer it fills as a sublayer of `<A>` by full path:
- Formats without layers (glTF, STL, OBJ) land under `<A>` on the path of the layer their importer assigns
- Block definition layers join `<A>`, and new definitions nothing references drop
- DWG blocks and `.3dm` definitions `doc` holds take `doc`'s definition, another `.3dm` definition of a held name imports as `<name> 01`

Use `use-blender` for Blender imports and IFC files.

## [05]-[HEADLESS]

`RhinoDoc.CreateHeadless(None)` makes an empty Millimeters document with no window and `RhinoDoc.OpenHeadless(path)` opens a `.3dm`, each disposed in its creating call:
- Headless documents take RhinoCommon edits, `export`, and typed writers, `command` refuses one
- Headless documents write no lock file, stay out of `list_slots` and `documents()`, and leave the active document as it was
- Headless documents list no render environment of their file, `File3dm.Read(path).RenderEnvironments` lists the file's
