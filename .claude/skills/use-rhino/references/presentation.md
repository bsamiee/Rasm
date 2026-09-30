# [PRESENTATION]

Materials, light, display modes, and saved states come from `document.py` entry points and RhinoCommon.

## [01]-[MATERIALS]

`material` builds Physically Based content as the Materials panel does, edits a same-named material in place, and replaces one of another type. Every layer and object holding it renders the change:
- `describe(doc).materials` and `file3dm.py` list each material with base color, roughness, metallic, opacity (1 opaque), and `ior`, 1.52 by default
- Layers take a material through `layer(..., Properties(material=))`, every object with the layer as material source renders with it
- Objects that differ from their layer take one through `add` or `change` with `Properties(material=)`
- `capture(..., mode="Rendered")` shows render materials, `Shaded` shows display colors, `Arctic` shows form under ambient occlusion with soft shadows

Other parameters and texture maps go on the same content:
- `RenderContentType.NewContentFromTypeId(ContentUuids.PhysicallyBasedMaterialType, doc)` makes one, `doc.RenderMaterials.Add` stores it
- `Material.ToPhysicallyBased()` holds IOR 1.0, typed content holds 1.52
- `Replace(content)` on a stored material puts `content` in its place everywhere it is assigned
- Edits run between `BeginChange(RenderContent.ChangeContexts.Program)` and `EndChange()`
- `SetParameter(ParameterNames.PhysicallyBased.<Name>, value)` sets a parameter
- Layers assigned in a script take `doc.Layers[index].RenderMaterial = content`, layer edits apply at once and `CommitChanges()` does nothing
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
settings.SetRenderEnvironment(RenderSettings.EnvironmentUsage.Background, environment)
for usage in (RenderSettings.EnvironmentUsage.Reflection, RenderSettings.EnvironmentUsage.Skylighting):
    settings.SetRenderEnvironmentOverride(usage, False)
settings.BackgroundStyle = BackgroundStyle.Environment
print(sun.Azimuth, sun.Altitude, settings.RenderEnvironment(RenderSettings.EnvironmentUsage.Background, RenderSettings.EnvironmentPurpose.Standard).Name)
```

- `sun.TimeZone` is the standard offset, `sun.DaylightSavingOn` with `DaylightSavingMinutes` 60 adds daylight time to local time
- `sun.North` is degrees counter-clockwise from +X, true north turned `n` degrees counter-clockwise from +Y takes `90 + n`
- `sun.Here()` reads the Mac's location, a site's sun takes its latitude and longitude
- `sun.Accuracy` reaches no file, a file read compares the sun's inputs and never its azimuth or altitude
- Earth anchors set `EarthBasepointLatitude`, `EarthBasepointLongitude`, and `EarthBasepointElevation` (m) on a copy assigned back
- `ModelNorth` `(-sin n, cos n, 0)` and `ModelEast` `(cos n, sin n, 0)` on a `doc.EarthAnchorPoint` copy turn its north with the sun's
- `EarthBasepointElevationCoordinateSystem` reads `Unset` after a file write, the elevation's reference stays out of `.3dm` files
- `sun.ManualControlOn = True` with `sun.Azimuth` and `sun.Altitude` places the sun by angle
- `settings.GroundPlane.AutoAltitude = False` with `Altitude` places the ground plane
- Physical skies are `ContentUuids.PhysicalSkyTextureType` content set as the `"texture"` child of a `BasicEnvironmentType` environment
- Reflection and skylighting overrides off take the background environment
- Overrides on naming the background's physical sky add lighting quadratic in its `rdk-texture-adjust-multiplier`
- `RenderSettings.RenderEnvironmentId(usage, EnvironmentPurpose.Standard)` reads a usage's environment, the one-argument form fails to bind
- `sun` color follows its altitude with no setter, a matched Blender SUN lamp stays white
- Template physical sky parameters fit Blender's `MULTIPLE_SCATTERING` sky at the document sun's site and moment
- Site or moment changes take sun irradiance, sky parameters, and plane luminance per multiplier measured again
- Template daylight folds exposure -5.3 EV into `sun.Intensity`, the sky's `rdk-texture-adjust-multiplier`, and the sample clamps
- Render window EXRs hold the linear frame before tone mapping, a sunlit white horizontal plane reads 1.0 there and in Raytraced
- Added lights and emissive materials take no exposure, against daylight they render `2 ** 5.3` (39) times brighter than in Blender

## [03]-[RENDERS]

Render windows and Raytraced views tone one linear Cycles frame through separate post-effect chains:
- Render windows run the document's selected tone mapper, templates select Darkroom's AgX (Blender's AgX Base sRGB, byte-exact) at 0 EV
- AgX raises its output to `PostProcessGamma` under `LinearWorkflow.PostProcessGammaOn`, Rhino's Gamma effect restores it
- Darkroom's dither runs after Gamma
- Raytraced tone mappers come from a per-view store only Rhino's internal types write, an empty store runs Clamp whatever the document selects
- Raytraced output then takes the document gamma while `LinearWorkflow.PostProcessGammaOn` is on
- No display mode, RhinoCycles key, or view member selects a Raytraced tone mapper or exposure
- Sunlit near-whites read higher in Raytraced than in the render window, Clamp holds no tone curve
- Look questions take one render each, a read render window closes and a read Raytraced view takes a non-realtime `show(mode=)` or `close_slot`

## [04]-[DISPLAY_MODES]

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
- `CopyDisplayMode` registers in memory and records its source as `DerivedFrom`, `UpdateDisplayMode(mode)` saves the mode whole at once
- Documents find a style by id, `ImportFromFile(path)` keeps the file's id and `CopyDisplayMode` gives a new one on each machine
- `ImportFromFile` of an id Rhino holds resets the held mode to its `DerivedFrom` mode, takes file keys, and turns SubD edges off
- `DeleteDisplayMode(id)` before `ImportFromFile` makes an import take the file whole, omitted keys take a Wireframe-like baseline
- INI files need `Name`, state floats as Rhino exports them (`2.200000047683716`), drop alpha, and take `y` and `n` for booleans
- INI keys another key switches off (shadow keys under `CastShadows=n`, `SolidColor` under `FillMode=1`) draw nothing
- Fresh exports compare against the held mode, a `DisplayModeDescription` fetched before an import keeps its fetched values
- `DeleteDisplayMode(id)` alone lets a mode return from its saved settings, a lasting removal deletes the mode's settings child and saves:

```python
# Remove a display mode and its saved settings permanently
from Rhino import PersistentSettings
from Rhino.Display import DisplayModeDescription

DisplayModeDescription.DeleteDisplayMode(<id>)
PersistentSettings.RhinoAppSettings.AddChild("Options").AddChild("DisplayAttributesManager").DeleteChild("<id>")
DisplayModeDescription.SaveDisplayModes()
```

- Temporary display modes take a fresh `Guid.NewGuid()` and a name prefix, and `DeleteDisplayMode` in the `finally` of their importing call
- `view.CaptureToBitmap(Size(w, h), mode)` draws `mode` at once, technical-family modes as plain wireframe and realtime modes without their render
- `ViewCapture` and `view.CaptureToBitmap(Size(w, h))` draw a mode set in a call from the next call on
- `view.CaptureToBitmap(Size(w, h))` of a view showing a realtime mode returns an empty frame, `ViewCapture` draws the render
- Technical-family modes show their lines in a window capture of a view set to the mode
- Black-to-white switching follows the application background and skips clipping edges, black objects draw white on a white mode fill
- Modes on a white fill draw black lines through fixed mode colors alone, black points and point clouds stay white there
- `MeshSpecificAttributes.AllMeshWiresColor` colors mesh edges and selected face wires through `MeshWireColor` and its single-color flag
- `'_SetDisplayMode _Viewport=_Active _Mode=<English name>` resolves the mode by name, `_Viewport=_All` sets every viewport, a rename breaks macros
- Technical-family modes (`DerivedFrom` Technical) refuse per-object assignment, `PipelineLocked` alone marks no family
- Display panel rows write the active viewport's whole mode and save it, its Reset copies a built-in parent over a custom mode
- Retina wires draw 1.5 device pixels per thickness unit under antialiasing, a thickness-1 curve spans 2 device pixels in a capture

## [05]-[SAVED_STATES]

- `save_view(doc, "<name>", zoom=, view=, mode=)` stores camera and display mode without moving any view, an existing name is replaced
- `show(doc, named="<name>")` moves the user's view to it with its mode and construction plane, `show(doc, view=, zoom=, mode=)` zooms to a box or ids
- `capture(doc, "<file>", named="<name>")` draws a named view with its mode and construction plane
- `doc.NamedViews.Restore(index, viewport)` renames `viewport` to the named view, its own name writes back afterwards
- `doc.NamedViews.Add(name, viewport.Id)` stores a named view in any document, `Add` of a `ViewInfo` built from scratch returns -1
- `position(doc, "<name>", ids)` records placements, `position(doc, "<name>")` moves the objects back after a trial transform
- `doc.NamedLayerStates.Save("<name>")` records every layer's settings, `Restore("<name>", RestoreLayerProperties.All)` brings them back
- Layer state restores leave layers added after the save as they are, `_-Layer _Off * _Enter` before a restore hides them

## [06]-[WINDOW_CAPTURES]

Window captures show the view as drawn, clipping planes and section fills included, without activating Rhino:

```bash
# Rhino document windows on the current Space: window id, title, on screen
osascript -l JavaScript -e 'ObjC.import("CoreGraphics"); JSON.stringify(ObjC.deepUnwrap(ObjC.castRefToObject($.CGWindowListCopyWindowInfo($.kCGWindowListOptionAll, 0))).filter(w => w.kCGWindowOwnerName === "RhinoBETA" && w.kCGWindowLayer === 0 && w.kCGWindowIsOnscreen).map(w => [w.kCGWindowNumber, w.kCGWindowName]))'
screencapture -x -o -l <window id> <file>.png
```

- Document windows are macOS window tabs of one frame, a background tab captures the front tab's pixels
- Grasshopper 2's editor is a child window of its opening frame, and its capture includes the frame
- Frame captures show Grasshopper 2's preview without its editor after `Editor.Instance.Visible = False`
- `view.Redraw()` ends the call before a window capture, window ids change at every launch
- Window captures right after a redrawing call can hold the window's previous frame, one listener call between them draws the new frame
- Menus and pop-ups draw only after a click, their contents read from the Eto tree or System Events AX attributes
- Docked panels capture through their own `NSView` with `BitmapImageRepForCachingDisplayInRect`, or hosted in a temporary Eto form captured by id
