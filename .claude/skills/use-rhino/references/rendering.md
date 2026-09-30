# [RENDERING]

Render settings, materials, light, and renders of one document write through `document.py` entry points and read back as records.

## [01]-[SETTINGS]

Rendering panel values, and every file a panel control picks, write through `doc.RenderSettings`, the view API, or a dashed command, and `render_settings(doc)` reads them back:

```python
# Stage render settings on one copy, commit them in one assignment, and read them back
from System.Drawing import Size
import document

doc = __rhino_doc__
settings = doc.RenderSettings.Duplicate()
settings.UseViewportSize, settings.ImageSize = False, Size(<width>, <height>)
settings.UserDictionary.Set("Samples", val=<samples>)
doc.RenderSettings = settings
print(document.render_settings(doc))
```

- `RenderRecord` opens with renderer, `source` view, `size` (`None` for viewport size), `quality`, document `samples`, and `background`
- `lighting` names each usage's rendering environment, `environments` lists each with its color, texture type, and image
- `sun`, `skylight`, `ground` (`material` `None` catching shadows alone), custom `channels`, and writable `flags` reading `True` follow
- `dithering`, `gamma`, render window `tone` mapper, and each view's `wallpapers` close the record
- Members set on `doc.RenderSettings` commit one by one, and `RenderSource`, `NamedView`, `Snapshot`, and `SpecificViewport` commit through a copy
- Cycles keys take `UserDictionary.Set("<key>", val=<value>)` in the type `render_dictionary` of `tools/interface/rhino/script/template.py` gives
- `Samples` renders while `UseDocumentSamples` holds `True`
- `view.MainViewport.SetWallpaper("<image>", <grayscale>)` sets the image `BackgroundStyle.WallpaperImage` renders
- `-_SetCurrentRenderPlugIn` writes a renderer every document shares, and a task leaves `renderer` as it found it
- Panel pictures read the docked Rendering panel, the one instance Rhino hosts

Copies of `Template Files/Default.3dm` hold every render setting and render content `tools/interface/render.py` declares, and another document takes both in one call:

```python
# Take the interface template's render content and settings, keeping the document's own layers and styles
from Rhino.FileIO import File3dm

doc, template = __rhino_doc__, "<Template Files>/Default.3dm"
layers, styles = doc.Layers.Count, doc.DimStyles.Count
doc.Import(template)
doc.Layers.Delete(list(range(layers, doc.Layers.Count)), quiet=True)
for index in reversed(range(styles, doc.DimStyles.Count)):
    doc.DimStyles.Delete(index, quiet=True)
doc.RenderSettings = File3dm.Read(template).Settings.RenderSettings
```

## [02]-[MATERIALS]

`material` builds Physically Based content as the Materials panel does, edits a same-named material in place, and replaces one of another type, every layer and object holding it rendering each change:

```python
# Textured concrete on Site::Massing, box-mapped at the texture set's physical size
from System import Guid
from Rhino import RhinoMath, UnitSystem
from Rhino.Geometry import Interval, Plane
from Rhino.Render import TextureMapping
import document
from records import Properties

doc, folder = __rhino_doc__, "<design-tools>/materials/<Set>_1K"
print(document.material(doc, "Concrete", roughness=1.0, textures={
    "pbr-base-color": f"{folder}/<Set>_1K-JPG_Color.jpg",
    "pbr-roughness": f"{folder}/<Set>_1K-JPG_Roughness.jpg",
    "pbr-bump": f"{folder}/<Set>_1K-JPG_NormalGL.jpg",
}))
print(document.layer(doc, "Site::Massing", Properties(material="Concrete")))
scale = RhinoMath.UnitScale(UnitSystem.Meters, doc.ModelUnitSystem)
x, y = <width m> * scale, <height m> * scale
mapping = TextureMapping.CreateBoxMapping(Plane.WorldXY, Interval(0, x), Interval(0, y), Interval(0, x), capped=True)
print(doc.Objects.FindId(Guid.Parse("<id>")).SetTextureMapping(1, mapping))
```

- `textures` keys are `ChildSlotNames.PhysicallyBased` values, color slots load display-encoded and data slots linear
- Normal maps go in `pbr-bump` from a set's `NormalGL` image, and a set's physical size comes from its ambientCG `dimensionX` and `dimensionY`
- `material` and `describe(doc).materials` print each material with its image per slot
- Layers take a material through `layer(..., Properties(material=))`, objects that differ from theirs through `add` or `change`
- Box mappings on channel 1 reach renders and exports, and a Real world size projection reaches renders alone
- Ground planes take one on a staged copy, `GroundPlane.ShadowOnly = False` and `GroundPlane.MaterialInstanceId = <material>.Id`
- `RenderContent.GetParameter(name)` returns a `Variant`, read through `.ToDouble()`, `.ToBool()`, `.ToSystemColor()`, or `.ToString(None)`
- `SetParameter(ParameterNames.PhysicallyBased.<Name>, value)` between `BeginChange(RenderContent.ChangeContexts.Program)` and `EndChange()` writes one
- `capture(doc, "<name>", mode="Rendered")` draws render materials at once

## [03]-[LIGHT]

Template copies light with the interface's sun, physical sky `Sky`, and ground plane, and `environment` switches one environment for background, reflection, and skylight:
- `environment(doc, "<name>", "<image>.exr")` builds an HDR environment and renders with it, returning `render_settings(doc)`
- `environment(doc, "<name>")` renders with an environment the document holds, `Sky` returning to the template's physical sky
- HDR images sit under `hdri/` of the `MATERIALS` folder `tools/interface/render.py` names
- Lights are geometry, `add(doc, light, "<A::B>")` places one on its layer
- Sun writes go through a windowed document's `doc.RenderSettings`, site and moment from `tools/interface/render.py`:

```python
# env: <repository>/tools
# Sun at the interface's site and moment, committed with the rest of a staged copy
from datetime import timedelta
from System import DateTime, DateTimeKind
from interface.render import DAYLIGHT, LATITUDE, LONGITUDE, MOMENT, NORTH, OFFSET
import document

doc = __rhino_doc__
settings = doc.RenderSettings.Duplicate()
sun = settings.Sun
sun.Enabled, sun.ManualControlOn, sun.Latitude, sun.Longitude, sun.North = True, False, LATITUDE, LONGITUDE, 90 + NORTH
sun.TimeZone, sun.DaylightSavingOn, sun.DaylightSavingMinutes = OFFSET / timedelta(hours=1), DAYLIGHT > timedelta(0), DAYLIGHT // timedelta(minutes=1)
sun.SetDateTime(DateTime(MOMENT.year, MOMENT.month, MOMENT.day, MOMENT.hour, MOMENT.minute, 0), DateTimeKind.Local)
doc.RenderSettings = settings
print(document.render_settings(doc).sun)
```

- `sun.ManualControlOn = True` with `sun.Azimuth` and `sun.Altitude` places the sun by angle
- `doc.EarthAnchorPoint` copies take the site with `ELEVATION` and turn north with `NORTH`, assigned back whole

## [04]-[RENDERS]

`render(doc, "<name>", view=, size=, samples=)` renders at Rhino's next idle and restores every setting it staged:
1. `render` returns `.artifacts/rhino/<name>.json`, written after the render window saves `<name>.exr` and `<name>.png` and closes
2. `wait4path <name>.json` in a background Bash call waits while no call reaches Rhino
3. `jq` on the `Rendering` record reads each command's `results`, saved `files`, and history `output`
4. `Read` of `<name>.png` shows the tone-mapped frame, and `<name>.exr` holds the linear frame before tone mapping

Raytraced views render live in their viewport:
1. `show(doc, view="<view>", mode="Raytraced")` starts the view's render
2. `view.RealtimeDisplayMode.LastRenderedPass()` against `HudMaximumPasses()` reads progress in a later call
3. `capture(doc, "<name>", view="<view>", mode="Raytraced", zoom=None, size=<render size>)` draws the frame
4. `show(doc, view="<view>", mode="<mode it held>")` ends the render

Look comparisons read one render and one Raytraced capture of one camera and size:
- Render windows run the selected `tone` mapper, and Raytraced views draw Clamp with document gamma
- `magick <name>.exr -evaluate pow 0.4545 <name>-clamp.png` gives the Clamp frame Raytraced draws
- `magick compare -metric RMSE <name>-clamp.png <raytraced>.png null:` measures light and geometry apart from tone mapping
- `magick <name>.png <raytraced>.png +append <pair>.png` shows both tone chains side by side
