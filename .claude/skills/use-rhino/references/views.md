# [VIEWS]

Captures, display modes, and saved views run through `document.py` entry points and RhinoCommon inside `run_python`.

## [01]-[CAPTURES]

`capture` draws `.artifacts/rhino/<name>.png` and returns `File(path, bytes, Capture(view, mode, changed))`, then `Read` of `path` shows it:
1. `capture(doc, "<name>")` frames visible model-space objects but clipping planes and lights in the active view and mode, one frame in every mode
2. `capture(doc, "<name>", zoom=None, view=describe(doc).current_view)` draws what the user sees
3. Table arguments narrow the picture

| [INDEX] | [ARGUMENT]                           | [DRAWS]                                                                                    |
| :-----: | :----------------------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `zoom=<ids>` or `zoom=<BoundingBox>` | Objects or box, projected extent centered inside the 1.1 zoom-extents border               |
|  [02]   | `zoom=None`                          | Camera the view holds                                                                      |
|  [03]   | `view="<view or page>"`              | Model view or layout page, shown or hidden, `()` framing paper at its aspect               |
|  [04]   | `named="<named view>"`               | Named view with its camera, display mode, and construction plane                           |
|  [05]   | `mode="<English name>"`              | Registered mode in place of the view's own                                                 |
|  [06]   | `mode=<DisplayModeDescription>`      | Edited copy of a mode, its attributes drawn with no setting written                        |
|  [07]   | `size=(<w>, <h>)`                    | Exact size, default 750 thousand pixels at the frustum's aspect                            |
|  [08]   | `since="<before>"`                   | Camera and size `<before>.png` stores, view and mode unless given, `Path` fault without it |

Sections capture in order:
1. `doc.Objects.AddClippingPlane(<plane>, <width>, <height>, view.MainViewport.Id)` clips the view, keeping the side its normal points to
2. `show(doc, view="<view>", zoom=<box>)` frames a box over the kept side, and objects the plane cuts frame whole under `zoom=()`
3. `capture(doc, "<name>", view="<view>", zoom=None)` draws the view's held camera, and a window capture crops the same view
4. `magick compare -metric AE -fuzz 10% <name>.png <window>.png <overlay>.png` marks edge lines, axes, and view title alone, clipped faces matching

Comparisons run in order:
1. `capture(doc, "<before>", ...)`
2. Edits under comparison, a trial move recorded first with `position(doc, "<name>", ids)`
3. `capture(doc, "<after>", since="<before>")`, `changed` counting pixels that differ
4. `Read` of `<after>-diff.png`, changed pixels red over `<before>` at half brightness
5. `position(doc, "<name>")` moves trial objects back, a `since` capture then counting `changed` pixels one level off at most

Pixel reads (levels, line widths, band edges, dither) draw at 1:1 device pixels:
- `size` takes `view.MainViewport.Size` of a view on screen (`view.Size` nonzero), `viewport.WorldToClient(point)` maps a point to its pixel
- `Read` downscales an image over 2000 px and re-encodes one over 500 KB as lossy JPEG
- `magick <png> -crop <w>x<h>+<x>+<y> +repage <crop>.png` cuts a detail `Read` shows at full resolution
- `Bitmap.GetPixel` reads frame buffer bytes and `magick` reads PNG bytes through their profile, one comparison takes every value from one reader

## [02]-[PIPELINE_MODES]

Raytraced and technical-family modes (Technical, Artistic, Pen, Monochrome, Concept, and copies of them) draw through a view on screen that shows the mode from an earlier call:
1. `show(doc, view="<view>", mode="Raytraced")` sets the mode, a `ViewCapture` fault from `capture` listing views on screen
2. `capture(doc, "<name>", view="<view>")` in the next call renders the mode's full pass count at the capture's camera
3. `show(doc, view="<view>", mode="<mode>")` sets back the mode `describe(doc).views` listed, captures in other modes following in a later call

Raytraced views render live once `show` sets the mode, `view.RealtimeDisplayMode.LastRenderedPass()` against `HudMaximumPasses()` reading progress.

Rendered and Raytraced draw a template copy's objects without a render material white under its sun, and scenes for them take one mid-gray `material(doc, "<name>", "#<RRGGBB>")` on each layer through `layer(doc, "<A::B>", Properties(material="<name>"))`.

## [03]-[DISPLAY_MODES]

Display modes are application settings every document and session shares, and an edited copy draws a variant with no registration:

```python
# Draw Shaded on white with no edges, no mode written
from Rhino.Display import DisplayModeDescription
from System.Drawing import Color
import document

variant = DisplayModeDescription.GetDisplayMode(DisplayModeDescription.ShadedId)
variant.DisplayAttributes.ShowSurfaceEdges = False
variant.DisplayAttributes.SetFill(Color.White)
print(document.capture(__rhino_doc__, "<name>", mode=variant))
```

Modes a view keeps are registered copies under a task prefix, removed with their saved settings at task end:

```python
# Copy Arctic under a task prefix, change it, show it
from Rhino.Display import DisplayModeDescription
import document

mode = DisplayModeDescription.GetDisplayMode(DisplayModeDescription.CopyDisplayMode(DisplayModeDescription.AmbientOcclusionId, "<prefix> <name>"))
mode.DisplayAttributes.ShowSurfaceEdges = True
DisplayModeDescription.UpdateDisplayMode(mode)
print(document.show(__rhino_doc__, view="<view>", mode="<prefix> <name>"))
```

```python
# Remove a copied mode and its saved settings
from Rhino import PersistentSettings
from Rhino.Display import DisplayModeDescription
from System import Guid

DisplayModeDescription.DeleteDisplayMode(Guid("<id>"))
PersistentSettings.RhinoAppSettings.AddChild("Options").AddChild("DisplayAttributesManager").DeleteChild("<id>")
DisplayModeDescription.SaveDisplayModes()
```

- `UpdateDisplayMode(mode)` writes the whole mode and saves the settings file, a copy never updated vanishes at the next settings save
- `DisplayModeDescription.ExportToFile(mode, "<file>.ini")` writes a mode other machines import with `ImportFromFile`, which keeps the file's id
- INI files state `Name`, floats as Rhino exports them (`2.200000047683716`), colors as `r,g,b`, and booleans as `y` and `n`
- `DeleteDisplayMode(<id>)` before `ImportFromFile` of a held id imports the file whole, a re-import resets the mode to its `DerivedFrom` parent
- Keys another key switches off (shadow keys under `CastShadows=n`, `SolidColor` under `FillMode=1`) draw nothing
- Built-in modes draw curves at linetype width times `CurveThicknessScale`, `CurveThickness` draws under `CurveThicknessUsage` Pixels alone

## [04]-[SAVED_STATES]

Named views, positions, and layer states record state a task restores:
- `save_view(doc, "<name>", zoom=, view=, mode=)` stores a camera and mode without moving a view, an existing name replaced
- `show(doc, named="<name>")` moves the user's view to a named view with its mode, construction plane, and stored clipping planes
- `show(doc, view=, zoom=, mode=)` moves the user's view to ids or a box and sets its mode
- `save_view(doc, "<task> return", zoom=None)` before a `show` and `show(doc, named="<task> return")` after it put the user's view back
- `doc.NamedViews.Delete(doc.NamedViews.FindByName("<task> return"))` removes the return view
- `position(doc, "<name>", ids)` records placements, `position(doc, "<name>")` moves the objects back after a trial transform
- `doc.Objects.Replace` takes an object out of each position holding it, and `position(doc, "<name>", ids)` records it again
- `doc.NamedLayerStates.Save("<name>")` records every layer, `Restore("<name>", RestoreLayerProperties.All)` brings each back
- Layer state restores leave layers added after the save, `_-Layer _Off * _Enter` before a restore hides them
- Per-detail layer states pass the detail's `Viewport.Id` to `Save` and to `Restore` with `RestoreLayerProperties.ViewportVisible`

## [05]-[WINDOW_CAPTURES]

Window captures show a document window as drawn, chrome and panels included, without activating Rhino:
1. `show(doc, ...)` sets the view, `view.ScreenRectangle` and `NSScreen.MainScreen.BackingScaleFactor` read in the same call
2. One more listener call lets the window draw its new frame
3. Window ids come from the window list, the document's front tab alone holding pixels:

```bash
# Rhino windows: window id, title, on screen, and origin in points
osascript -l JavaScript -e 'ObjC.import("CoreGraphics"); JSON.stringify(ObjC.deepUnwrap(ObjC.castRefToObject($.CGWindowListCopyWindowInfo($.kCGWindowListOptionAll, 0))).filter(w => w.kCGWindowOwnerName === "RhinoBETA" && w.kCGWindowLayer === 0 && w.kCGWindowName).map(w => [w.kCGWindowNumber, w.kCGWindowName, w.kCGWindowIsOnscreen, w.kCGWindowBounds.X, w.kCGWindowBounds.Y]))'
screencapture -x -o -l <window id> <dir>/<name>.png
```

4. `magick <dir>/<name>.png -crop <w>x<h>+<x>+<y> +repage -resize '1000000@>' <root>/.artifacts/rhino/<name>.png` cuts the view
- Crop sizes and origins come from the rectangle in device pixels, origins less the window origin in points times the scale
- Panels capture through an Eto form hosting a new instance of the panel's control, captured by its window id and closed by title
- Rendering panel captures as its docked instance, a frame capture by window id cropped to the panel with `magick`
- Grasshopper 2's editor is a child window of its opening frame, and `Editor.Instance.Visible = False` leaves the preview in a frame capture
