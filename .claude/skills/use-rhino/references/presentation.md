# [PRESENTATION]

Materials, light, display modes, and saved states come from `document.py` entry points and RhinoCommon, proof comes from `capture`.

## [01]-[MATERIALS]

`material` builds the Physically Based content the Materials panel creates, edits a material of that name in place, and replaces one of another type. Every layer and object holding it renders the change:
- `describe(doc).materials` and `file3dm.py` list each material with base color, roughness, metallic, opacity (1 opaque), and `ior`, 1.52 by default
- Layers take a material through `layer(..., Properties(material=))`, every object with the layer as material source renders with it
- Objects that differ from their layer take one through `add` or `change` with `Properties(material=)`
- `capture(..., mode="Rendered")` shows render materials, `Shaded` shows display colors, `Arctic` shows form in white with soft shadows

Other parameters and texture maps go on the same content:
- `RenderContentType.NewContentFromTypeId(ContentUuids.PhysicallyBasedMaterialType, doc)` makes one, `doc.RenderMaterials.Add` stores it
- `Material.ToPhysicallyBased()` carries IOR 1.0 and removes every dielectric reflection in Blender, typed content carries 1.52
- `Replace(content)` on a stored material puts `content` in its place everywhere it is assigned
- Edits run between `BeginChange(RenderContent.ChangeContexts.Program)` and `EndChange()`
- `SetParameter(ParameterNames.PhysicallyBased.<Name>, value)` sets a parameter
- Layers assigned in a script take `held = doc.Layers[index]`, `held.RenderMaterial = content`, then `held.CommitChanges()`
- Texture maps are `ContentUuids.BitmapTextureType` content with `filename`, joined by `SetChild(texture, ChildSlotNames.PhysicallyBased.<Slot>)`
- `SetChildSlotOn(<slot>, True, RenderContent.ChangeContexts.Program)` turns a slot on
- `FindChild(<slot>)` and `ChildSlotOn(<slot>)` read a slot, the content lists no `Children`
- Roughness, metallic, opacity, occlusion, height, and normal maps set `treat-as-linear` True, normal maps follow the OpenGL convention
- Real world size mapping computes at render time and reaches no export
- `TextureMapping.CreateBoxMapping` set through `SetTextureMapping(1, mapping)` on the object reaches exported UVs

## [02]-[LIGHT]

Lights are geometry, `add(doc, light, "<A::B>")` places one on its layer. `doc.RenderSettings` is live, a property set on it holds without reassignment:

```python
# Sun by place and time, skylight, a shadow-only ground plane, and one environment for background, reflection, and skylight
from Rhino.Display import BackgroundStyle
from Rhino.Render import RenderEnvironment, RenderSettings, SimulatedEnvironment
from System import DateTime, DateTimeKind
from System.Drawing import Color
doc = __rhino_doc__
settings = doc.RenderSettings
sun = settings.Sun
sun.Enabled, sun.ManualControlOn = True, False
sun.Latitude, sun.Longitude, sun.TimeZone = <latitude>, <longitude>, <utc offset hours>
sun.SetDateTime(DateTime(<year>, <month>, <day>, <hour>, 0, 0), DateTimeKind.Local)
settings.Skylight.Enabled = True
settings.GroundPlane.Enabled, settings.GroundPlane.ShadowOnly, settings.GroundPlane.AutoAltitude = True, True, True
simulated = SimulatedEnvironment()
simulated.BackgroundColor = Color.FromArgb(<r>, <g>, <b>)
environment = RenderEnvironment.NewBasicEnvironment(simulated, doc)
environment.Name = "<name>"
doc.RenderEnvironments.Add(environment)
for usage in (RenderSettings.EnvironmentUsage.Background, RenderSettings.EnvironmentUsage.Reflection, RenderSettings.EnvironmentUsage.Skylighting):
    settings.SetRenderEnvironmentOverride(usage, True)
    settings.SetRenderEnvironment(usage, environment)
settings.BackgroundStyle = BackgroundStyle.Environment
print(sun.Azimuth, sun.Altitude, settings.RenderEnvironment(RenderSettings.EnvironmentUsage.Background, RenderSettings.EnvironmentPurpose.Standard).Name)
```

- `sun.TimeZone` is the standard offset, `sun.DaylightSavingOn` with `DaylightSavingMinutes` 60 adds daylight time to the local clock
- `sun.North` is degrees counter-clockwise from +X, true north turned `n` degrees counter-clockwise from +Y takes `90 + n`
- `sun.Here()` reads the Mac's location, a site's sun takes the site's latitude and longitude
- `sun.Accuracy` reaches no file, a file read compares the sun's inputs and never its azimuth or altitude
- Earth anchors set `EarthBasepointLatitude`, `EarthBasepointLongitude`, and `EarthBasepointElevation` (m) on a copy assigned back
- `ModelNorth` `(-sin n, cos n, 0)` and `ModelEast` `(cos n, sin n, 0)` on the `doc.EarthAnchorPoint` copy turn its north with the sun's
- `EarthBasepointElevationCoordinateSystem` reads `Unset` after a file write, the elevation's reference stays out of the `.3dm`
- `sun.ManualControlOn = True` with `sun.Azimuth` and `sun.Altitude` places the sun by angle
- `settings.GroundPlane.AutoAltitude = False` with `Altitude` places the ground plane
- Physical skies are `ContentUuids.PhysicalSkyTextureType` content set as the `"texture"` child of a `BasicEnvironmentType` environment
- `RenderSettings.RenderEnvironmentId(usage, EnvironmentPurpose.Standard)` reads a usage's environment, the one-argument form fails to bind

## [03]-[DISPLAY_MODES]

Display modes are application settings every document and session shares, `DisplayModeDescription` owns them, and a new style copies a mode under its own name:

```python
# Copy a mode, change it, apply it to one viewport, keep it as a file
from Rhino.Display import DisplayModeDescription
mode_id = DisplayModeDescription.CopyDisplayMode(DisplayModeDescription.ShadedId, "<Style>")
mode = DisplayModeDescription.GetDisplayMode(mode_id)
mode.DisplayAttributes.CurveThicknessScale = 3
DisplayModeDescription.UpdateDisplayMode(mode)
__rhino_doc__.Views.Find("Perspective", True).ActiveViewport.DisplayMode = DisplayModeDescription.GetDisplayMode(mode_id)
print(DisplayModeDescription.ExportToFile(mode, "</abs/style.ini>"))
```

- Built-in modes draw curves at their linetype width times `CurveThicknessScale`, `CurveThickness` draws only under `CurveThicknessUsage` Pixels
- `CopyDisplayMode` registers in memory and records the source as `DerivedFrom`, `UpdateDisplayMode(mode)` saves the mode whole at once
- Documents find a style by id, `ImportFromFile(path)` keeps the id the file names and `CopyDisplayMode` gives a new one on each machine
- `ImportFromFile` of an id Rhino holds resets the mode to its `DerivedFrom` mode, takes the file's keys, and turns SubD edges off
- `DeleteDisplayMode(id)` before `ImportFromFile` makes the import take the file whole, omitted keys take a Wireframe-like baseline
- INI files need `Name`, state floats as Rhino exports them (`2.200000047683716`), drop alpha, and take `y` and `n` for booleans
- INI keys another key switches off (shadow keys under `CastShadows=n`, `SolidColor` under `FillMode=1`) draw nothing
- Fresh exports compare against the held mode, a `DisplayModeDescription` fetched before an import keeps the values it was fetched with
- `DeleteDisplayMode(id)` alone lets the mode return from its saved settings, a lasting removal deletes that settings child and saves:

```python
# Remove a display mode and its saved settings for good
from Rhino import PersistentSettings
from Rhino.Display import DisplayModeDescription
DisplayModeDescription.DeleteDisplayMode(<id>)
PersistentSettings.RhinoAppSettings.AddChild("Options").AddChild("DisplayAttributesManager").DeleteChild("<id>")
DisplayModeDescription.SaveDisplayModes()
```

- Scratch modes take a fresh `Guid.NewGuid()` and a name prefix, and `DeleteDisplayMode` in the `finally` of the call that imported them
- `view.CaptureToBitmap(Size(w, h), mode)` draws a mode the view does not show, and `capture(..., mode="<Style>")` renders a style by name
- `'_SetDisplayMode _Viewport=_Active _Mode=<English name>` resolves the mode by name, `_Viewport=_All` sets every viewport, a rename breaks macros
- Technical-family modes (`DerivedFrom` Technical) refuse per-object assignment, `PipelineLocked` alone marks no family
- Display panel rows write the active viewport's whole mode and save it, its Reset copies the built-in parent over a custom mode
- Retina wires draw 1.5 device pixels per thickness unit under antialiasing, a thickness-1 curve spans 2 device pixels in a capture

## [04]-[SAVED_STATES]

- `save_view(doc, "<name>", zoom=, view=, mode=)` stores camera and display mode without moving any view, an existing name is replaced
- `show(doc, named="<name>")` moves the user's view to it with its mode and construction plane, `show(doc, view=, zoom=, mode=)` zooms to a box or ids
- `capture(doc, "<file>", named="<name>")` renders a named view and leaves the user's camera and construction plane as they were
- `doc.NamedViews.Restore(index, viewport)` renames the viewport to the named view, the name writes back afterwards
- `doc.NamedViews.Add(name, viewport.Id)` returns -1 in a headless document, named views need a windowed document
- `position(doc, "<name>", ids)` records placements, `position(doc, "<name>")` moves the objects back after a trial transform
- `doc.NamedLayerStates.Save("<name>")` records every layer's settings, `Restore("<name>", RestoreLayerProperties.All)` brings them back
- Layer state restores leave layers added after the save as they are, `_-Layer _Off * _Enter` before a restore hides them

## [05]-[WINDOW_CAPTURES]

Window captures show the view as drawn, clipping planes and section fills included, without activating Rhino:

```bash
# Rhino document windows on this Space: window id, title, on screen
osascript -l JavaScript -e 'ObjC.import("CoreGraphics"); JSON.stringify(ObjC.deepUnwrap(ObjC.castRefToObject($.CGWindowListCopyWindowInfo($.kCGWindowListOptionAll, 0))).filter(w => w.kCGWindowOwnerName === "RhinoBETA" && w.kCGWindowLayer === 0 && w.kCGWindowIsOnscreen).map(w => [w.kCGWindowNumber, w.kCGWindowName]))'
screencapture -x -o -l <window id> <file>.png
```

- Document windows and the Grasshopper 2 editor are macOS window tabs of one frame, a background tab captures the front tab's pixels
- `view.Redraw()` ends the call before the window capture, window ids change at every launch
- Menus and pop-ups draw only after a click, their contents read from the Eto tree or System Events AX attributes
- Docked panels capture through their own `NSView` with `BitmapImageRepForCachingDisplayInRect`, or hosted in a scratch Eto form captured by id
