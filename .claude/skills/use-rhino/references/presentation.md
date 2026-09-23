# [PRESENTATION]

Materials, light, display styles, and saved states come from `document.py` entry points and RhinoCommon, proof comes from `capture`.

## [01]-[MATERIALS]

- `material` adds a name once and updates it after, every layer and object holding it renders the update, `describe(doc).materials` lists them
- Layers take a material through `layer(..., Properties(material=))`, every object with the layer as material source renders with it
- Objects that differ from their layer take one through `add` or `change` with `Properties(material=)`
- `capture(..., mode="Rendered")` shows render materials, `Shaded` shows display colors, `Arctic` shows form in white with soft shadows

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

- `sun.ManualControlOn = True` with `sun.Azimuth` and `sun.Altitude` places the sun by angle
- `settings.GroundPlane.AutoAltitude = False` with `Altitude` places the ground plane

## [03]-[DISPLAY_STYLES]

Display modes are application settings every document and session shares, a new style copies a mode under its own name:

```python
# Copy a mode, change it, apply it to one viewport, keep it as a file
from Rhino.Display import DisplayModeDescription
mode_id = DisplayModeDescription.CopyDisplayMode(DisplayModeDescription.ShadedId, "<Style>")
mode = DisplayModeDescription.GetDisplayMode(mode_id)
mode.DisplayAttributes.CurveThickness = 3
DisplayModeDescription.UpdateDisplayMode(mode)
__rhino_doc__.Views.Find("Perspective", True).ActiveViewport.DisplayMode = DisplayModeDescription.GetDisplayMode(mode_id)
print(DisplayModeDescription.ExportToFile(mode, "</abs/style.ini>"))
```

- `DisplayModeDescription.ImportFromFile(path)` brings a saved style back, `DeleteDisplayMode(id)` removes one
- `capture(..., mode="<Style>")` renders a style by name without changing any view's own mode

## [04]-[SAVED_STATES]

- `save_view(doc, "<name>", zoom=, view=, mode=)` stores camera and display mode without moving any view, an existing name is replaced
- `show(doc, named="<name>")` moves the user's view to it with its mode, `show(doc, view=, zoom=, mode=)` zooms to a box or ids
- `capture(doc, "<file>", named="<name>")` renders a named view and leaves the user's camera as it was
- `position(doc, "<name>", ids)` records placements, `position(doc, "<name>")` moves the objects back after a trial transform
- `doc.NamedLayerStates.Save("<name>")` records every layer's settings, `Restore("<name>", RestoreLayerProperties.All)` brings them back
