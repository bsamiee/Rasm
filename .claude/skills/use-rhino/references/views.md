# [VIEWS]

Captures, display modes, and saved views run through `document.py` entry points and RhinoCommon inside `run_python`.

## [01]-[CAPTURES]

`capture(doc, "<name>")` draws `.artifacts/rhino/<name>.png` and returns `File(path, bytes, Capture(view, mode, changed))`, then `Read` of `path` shows it:
1. `capture(doc, "<name>")` frames every visible model-space object in the active view and its own mode
2. `capture(doc, "<name>", zoom=None, view=describe(doc).current_view)` draws what the user sees
3. Arguments from the table narrow the picture, each call restoring every view, overlay, and selection before it returns

| [INDEX] | [ARGUMENT]                           | [DRAWS]                                                                        |
| :-----: | :----------------------------------- | :----------------------------------------------------------------------------- |
|  [01]   | `zoom=<ids>` or `zoom=<BoundingBox>` | Objects or box framed at Rhino's zoom-extents border                           |
|  [02]   | `zoom=None`                          | Camera the view holds                                                          |
|  [03]   | `view="<view or page>"`              | Any model view or layout page without showing it, `()` framing a page's paper  |
|  [04]   | `named="<named view>"`               | Named view with its camera, display mode, and construction plane               |
|  [05]   | `mode="<English name>"`              | Registered mode in place of the view's own                                     |
|  [06]   | `mode=<DisplayModeDescription>`      | Edited copy of a mode, its attributes drawn with no setting written            |
|  [07]   | `size=(<w>, <h>)`                    | Exact pixel size, the view's aspect at a 2000 px long side by default          |
|  [08]   | `since="<before>"`                   | Camera and size `<before>.png` stores, its view and mode unless given          |

Captures draw without grid, axes, highlight, Gumball, or Grasshopper 2 preview, and baked objects show in them. Comparisons run:
1. `capture(doc, "<before>", ...)`
2. Edits under comparison
3. `capture(doc, "<after>", since="<before>")`, `changed` counting pixels that differ, 0 for an unchanged picture
4. `Read` of `<after>-diff.png`, changed pixels red over `<before>` at half brightness

Pixel reads (levels, line widths, band edges, dither) draw at 1:1 device pixels:
- `size` takes `view.MainViewport.Size` of a view on screen, `viewport.WorldToClient(point)` maps a point to its pixel
- `magick <png> -crop <w>x<h>+<x>+<y> +repage <crop>.png` cuts a detail `Read` shows at full resolution
- `Bitmap.GetPixel` reads frame buffer bytes and `magick` reads PNG bytes through their profile, one comparison takes every value from one reader

## [02]-[PIPELINE_MODES]

Raytraced and technical-family modes (Technical, Artistic, Pen, Monochrome, Concept, and copies of them) draw only through a view that shows the mode from an earlier call, `capture` returns a `ViewCapture` fault naming such a mode otherwise:
1. `show(doc, view="<view>", mode="Raytraced")` sets the mode
2. `capture(doc, "<name>", view="<view>", size=(1000, 650))` in the next call renders to the mode's pass count
3. `show(doc, view="<view>", mode="<mode>")` restores the mode `describe(doc).views` listed

## [03]-[DISPLAY_MODES]

Display modes are application settings every document and session shares, and an edited copy draws a variant with no registration:

```python
# Draw Shaded with thick curves and no edges, no mode written
from Rhino.Display import DisplayModeDescription
import document

variant = DisplayModeDescription.GetDisplayMode(DisplayModeDescription.ShadedId)
variant.DisplayAttributes.CurveThicknessScale = 3
variant.DisplayAttributes.ShowSurfaceEdges = False
print(document.capture(__rhino_doc__, "<name>", mode=variant))
```

Modes a view keeps are registered copies under a task prefix, removed with their saved settings at task end:

```python
# Copy Arctic under a task prefix, change it, show it
from Rhino.Display import DisplayModeDescription
import document

mode = DisplayModeDescription.GetDisplayMode(DisplayModeDescription.CopyDisplayMode(DisplayModeDescription.AmbientOcclusionId, "<prefix> <name>"))
mode.DisplayAttributes.CurveThicknessScale = 2
DisplayModeDescription.UpdateDisplayMode(mode)
print(document.show(__rhino_doc__, view="<view>", mode="<prefix> <name>"))
```

```python
# Remove a copied mode and its saved settings
from Rhino import PersistentSettings
from Rhino.Display import DisplayModeDescription

DisplayModeDescription.DeleteDisplayMode(<id>)
PersistentSettings.RhinoAppSettings.AddChild("Options").AddChild("DisplayAttributesManager").DeleteChild("<id>")
DisplayModeDescription.SaveDisplayModes()
```

- `UpdateDisplayMode(mode)` writes the whole mode and saves the settings file, a copy never updated vanishes at the next settings save
- `DisplayModeDescription.ExportToFile(mode, "<file>.ini")` writes a mode other machines import with `ImportFromFile`, which keeps the file's id
- INI files state `Name`, floats as Rhino exports them (`2.200000047683716`), colors as `r,g,b`, and booleans as `y` and `n`
- `DeleteDisplayMode(<id>)` before `ImportFromFile` of a held id imports the file whole, a re-import resets the mode to its `DerivedFrom` parent
- Keys another key switches off (shadow keys under `CastShadows=n`, `SolidColor` under `FillMode=1`) draw nothing
- Built-in modes draw curves at linetype width times `CurveThicknessScale`, `CurveThickness` draws under `CurveThicknessUsage` Pixels alone
- Display panel rows and its Reset write the active viewport's whole mode and save it

## [04]-[SAVED_STATES]

Named views, positions, and layer states record state a task restores:
- `save_view(doc, "<name>", zoom=, view=, mode=)` stores a camera and mode without moving a view, an existing name replaced
- `show(doc, named="<name>")` moves the user's view to a named view with its mode and construction plane
- `show(doc, view=, zoom=, mode=)` moves the user's view to ids or a box and sets its mode
- `position(doc, "<name>", ids)` records placements, `position(doc, "<name>")` moves the objects back after a trial transform
- `doc.NamedLayerStates.Save("<name>")` records every layer, `Restore("<name>", RestoreLayerProperties.All)` brings each back
- Layer state restores leave layers added after the save, `_-Layer _Off * _Enter` before a restore hides them
- Per-detail layer states pass the detail's `Viewport.Id` to both `Save` and `Restore`

## [05]-[WINDOW_CAPTURES]

Window captures show a document window as drawn, chrome and panels included, without activating Rhino:
1. `show(doc, ...)` sets the view, and a later listener call lets the window draw its new frame
2. Window ids of the document's frame come from the window list, its front tab alone holding pixels:

```bash
# Rhino windows on screen: window id and title
osascript -l JavaScript -e 'ObjC.import("CoreGraphics"); JSON.stringify(ObjC.deepUnwrap(ObjC.castRefToObject($.CGWindowListCopyWindowInfo($.kCGWindowListOptionOnScreenOnly, 0))).filter(w => w.kCGWindowOwnerName === "RhinoBETA" && w.kCGWindowLayer === 0).map(w => [w.kCGWindowNumber, w.kCGWindowName]))'
screencapture -x -o -l <window id> <root>/.artifacts/rhino/<name>.png
```

3. `magick <png> -crop` to the view's `ScreenRectangle` times 2 cuts the view out of the window
- Panels capture through a temporary Eto form hosting a new instance of the panel's control, captured by its window id and closed by title
- Rendering panel captures as its docked instance, a frame capture by window id cropped to the panel with `magick`
- Grasshopper 2's editor is a child window of its opening frame, and `Editor.Instance.Visible = False` leaves the preview in a frame capture
