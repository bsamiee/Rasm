# [FILES]

`.3dm` files read on disk without Rhino, open as their own documents behind the user, and write, convert, and import through `document.py` entry points.

## [01]-[READ]

`uv run --script <skill>/scripts/file3dm.py <file or folder>...` prints one JSON line per `.3dm`, a folder standing for every `.3dm` under it, and exits 1 when any file faults:
- First runs build `rhino3dm` from source into the uv cache, a Bash call with `run_in_background` holds one
- Records hold the layer and material shapes `describe(doc)` prints, `version` the archive version, `edited_by`, and `edited` in UTC
- `page_units` and annotation `styles` sit beside model units, `layouts` lists pages holding page-space objects and `views` every other view
- Counts, `min`, and `max` cover model-space objects outside block definitions
- `materials` lists materials the file saved, a material no layer or object uses stays out of a saved file
- `open_by` holds user, computer, and open time from the `<file>.rhl` lock Rhino writes beside a file it holds, a lock outlives a quit or crash
- `Fault(File3dm, <path>)` lines name files Rhino answers with a modal alert that holds every close in the process, and those stay unopened
- Layer linetypes read `None`, `rhino3dm` crashes the interpreter on releasing a model whose linetype table it read
- View display modes, render settings, and section styles read through RhinoCommon `File3dm.Read(path)` inside Rhino

```bash
# Paths in meters, then files holding layouts, then the layers of one file
uv run --script <skill>/scripts/file3dm.py <folder> | jq -r 'select(.units == "Meters") | .path'
uv run --script <skill>/scripts/file3dm.py <folder> | jq -r 'select(.layouts | length > 0) | .path'
uv run --script <skill>/scripts/file3dm.py <file> | jq -c '.layers[] | {path, objects}'
```

## [02]-[OPEN]

Tasks reach a document before any change:
1. `documents()` in any slot lists every open document by `serial`, `path` (`None` untitled), `modified`, `active`, and listener `port`
2. Files the task names that no document holds read first through `file3dm.py`
3. `open -g -b com.mcneel.rhinoceros.9 <file>...` opens each readable file as its own active document with a listener, Rhino stays behind
4. New work takes a copy of its unit system's `Template Files/` template under the scratchpad, opened the same way and deleted at task end
5. `documents()` rows with the file's `path` give its `serial` and `port`, the `list_slots` row on that port names the call's `slot`
6. `describe(doc)` fields decide the next step:

| [INDEX] | [FIELD]                   | [DECIDES]                                                                                              |
| :-----: | :------------------------ | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `units`, `tolerance`      | Every number in a script or macro is in model units                                                    |
|  [02]   | `active`                  | `command` refuses an inactive document, `close(doc)` then `open -g` on its file reopen it active       |
|  [03]   | `prompt`                  | Command waiting in Rhino, setup release frees a task's own, another session's holds every command      |
|  [04]   | `layers`, `current_layer` | Hierarchy new objects join, adds without attributes take the active document's current layer index     |
|  [05]   | `selected`                | Objects the user means by "this" or "these"                                                            |
|  [06]   | `current_view`, `views`   | View the user looks at, names and cameras `capture`, `show`, and `save_view` take, each layout's scale |
|  [07]   | `page_units`, `style`     | Paper size units, and the annotation style new dimensions take                                         |
|  [08]   | `named_cplanes`           | Construction planes a script restores by name                                                          |
|  [09]   | `materials`               | Physically based materials a `Properties(material=)` names for a layer or object                       |
|  [10]   | `path`, `modified`        | File `save(doc)` writes, `None` takes a path, `True` marks unsaved edits                               |

## [03]-[SAVE_AND_CLOSE]

- Titled documents autosave into their own file when Rhino leaves the front and every 5 idle minutes
- `save(doc)` writes the document's file through its window's `NSDocument`, `save(doc, path)` makes `path` the document's file
- `save` runs in a call after the edits' call, a call's undo record closes after its `save` and marks the document modified
- `close(doc)` saves a titled document's unsaved edits, discards an untitled document's, and runs from another slot's call by `serial`
- Tasks close each document they opened, a titled one through `close(doc)`, a spawned untitled one through `close_slot`

## [04]-[EXPORT]

`export(doc, path, ids)` writes the document's model space or objects `ids` through the suffix's writer and returns `File` with `detail` the ids the format drops:
- Relative paths land under `.artifacts/rhino`, an overwritten target leaves no backup file
- Unknown suffixes fault with every suffix a writer accepts, a target equal to the document's own file faults
- `.3dm` of a whole document writes it with layouts, other writes go through a headless copy holding the chosen objects with layers, materials, and blocks
- Typed writers run with every dialog and prompt suppressed and ignore the options each export plugin's settings file remembers
- STEP, IGES, SAT, and Parasolid drop object types their format lacks, blocks holding a dropped type write as their pieces
- DWG and DXF write solids as ACIS solids, flatten layer paths to one level joined by `$`, and write black layers white
- `.glb` writes materials double-sided with their IOR, STEP writes AP203 in the document's units
- `FileWriteOptions` with `IncludeRenderMeshes` and `IncludeBitmapTable` True embeds render meshes and the textures of render content
- Exports and headless copies leave document user text (`doc.Strings`) out while `Options/Advanced/ExportDocumentUserText` holds False

## [05]-[CONVERT_AND_IMPORT]

`convert(doc, sources, "<.suffix>", folder)` writes each source as `<stem><suffix>` in `folder`, `.artifacts/rhino` unless named, through a headless document per source:
- `.3dm` sources convert with their own units, other formats read into `doc`'s units, STEP scaled by the units it stores
- STEP reads onto layer Default as one instance of a nested block `Document` holding every shape
- Missing, unreadable, and same-path sources, and sources sharing a target stem, fault alone, and every other source converts
- Batches split across calls, each within the router limit

`load(doc, path, "<A>")` imports a file into `doc` under layer `<A>` with the file's layers as its sublayers:
- `-_Options _Files _LayerImport` set to match short names merges the file's layers into same-named layers instead
- Block definition layers join `<A>`, a block `doc` names takes `doc`'s definition, and new definitions nothing references drop

Use `use-blender` for Blender imports and IFC files.

## [06]-[HEADLESS]

`RhinoDoc.CreateHeadless(None)` makes an empty Millimeters document with no window and `RhinoDoc.OpenHeadless(path)` opens a `.3dm`, each disposed in its creating call:
- Headless documents take RhinoCommon edits, `export`, and typed writers, `command` refuses one
- Headless documents write no lock file, stay out of `list_slots` and `documents()`, and leave the active document as it was
- Headless documents list no render environment of their file, `File3dm.Read(path).RenderEnvironments` lists the file's
