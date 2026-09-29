# ty: ignore[unresolved-attribute, unresolved-import, invalid-argument-type, invalid-return-type, invalid-exception-caught]
# mypy: disable-error-code="import-untyped, import-not-found, no-any-unimported, attr-defined, misc, no-any-return"
"""Rows of Rhino's imperial default template, the metric one beside it, and the default template setting naming the imperial one."""

from collections.abc import Iterable, Mapping
from contextlib import ExitStack
import ctypes
from datetime import datetime, timedelta
from functools import partial
import math
from pathlib import Path
from typing import Final, TypedDict
from uuid import UUID

import clr
import Rhino
from Rhino.ApplicationSettings import FileSettings
from Rhino.Display import BackgroundStyle, DisplayModeDescription, RhinoPageView, ViewTypeFilter
from Rhino.DocObjects import (
    ActiveSpace,
    DimensionStyle,
    Font,
    HatchPattern,
    Layer,
    Linetype,
    ObjectAttributes,
    ObjectEnumeratorSettings,
    ObjectSectionFillRule,
    SectionBackgroundFillMode,
    SectionStyle,
    ViewInfo,
)
from Rhino.FileIO import File3dm, File3dmWriteOptions, FileWriteOptions
from Rhino.Geometry import BoundingBox, MeshingParameterStyle, Plane, Point3d, Rectangle3d, Vector3d
from Rhino.Render import ContentUuids, RenderContent, RenderContentType, RenderSettings
import System
from System import Array, DateTime, DateTimeKind, Guid
from System.Drawing import Size
from System.IO import FileNotFoundException

from interface.render import (
    CAUSTICS,
    DAYLIGHT,
    DIFFUSE_BOUNCES,
    DPI,
    ELEVATION,
    EXPOSURE,
    FILTER_GLOSSY,
    FRAME_SIZE,
    GLOSSY_BOUNCES,
    INDIRECT_CLAMP,
    LATITUDE,
    LENS,
    LONGITUDE,
    MAX_BOUNCES,
    MOMENT,
    NOISE_THRESHOLD,
    NORTH,
    OFFSET,
    SAMPLES,
    SKY_RADIANCE,
    SUN_IRRADIANCE,
    TRANSMISSION_BOUNCES,
    TRANSPARENT_BOUNCES,
    VOLUME_BOUNCES,
)
from interface.report import Row
from interface.rhino.script.accessors import color, found, member, plain
from interface.roles import Annotation, Ink, Surface, Typography
from interface.units import ANGLE_PRECISION, GRID_THICK_EVERY, Length, Pen, Units

# --- [CONSTANTS] ------------------------------------------------------------------------

SKY_SLOT: Final = "texture"
SKY_SUN: Final = "use-document-sun"
SKY_MULTIPLIER: Final = "rdk-texture-adjust-multiplier"

# --- [HOST] -----------------------------------------------------------------------------

ANNOTATION_ID: Final = Guid.Parse(str(Annotation.ID))
SKY_USAGES: Final = tuple(System.Enum.GetValues(clr.GetClrType(RenderSettings.EnvironmentUsage)))
PROCESS: Final = ctypes.CDLL(None)

# --- [MODELS] ---------------------------------------------------------------------------


class UuidStruct(ctypes.Structure):
    """openNURBS `ON_UUID_struct` a native getter returns by value."""

    _fields_ = (("data", ctypes.c_ubyte * 16),)


class Render(TypedDict):
    """Document render settings by RhinoCommon member, the dithering and ground plane as their `Enabled` flags."""

    ImageUnitSystem: Rhino.UnitSystem
    UseViewportSize: bool
    ImageSize: tuple[int, int]
    ImageDpi: int
    Dithering: bool
    GroundPlane: bool
    UserDictionary: Mapping[str, object]


class Environment(TypedDict):
    """Background style, the background environment's sky texture, and the environment counts."""

    BackgroundStyle: Rhino.Display.BackgroundStyle
    Texture: Mapping[str, object]
    UsageEnvironments: int
    FileEnvironments: int


class Dashes(TypedDict):
    """Linetype segment lengths in millimeters, each gap negative, and the line width with its unit."""

    Segments: tuple[float, ...]
    Width: float
    WidthUnits: Rhino.UnitSystem


class Page(TypedDict):
    """Layout page size in page units, and the layer and corners of each object on it."""

    PageWidth: float
    PageHeight: float
    Objects: tuple[tuple[str, tuple[float, float], tuple[float, float]], ...]


class Template(TypedDict):
    """Template facts by group, each generic group's members named as RhinoCommon names them."""

    unit_systems: Mapping[str, Rhino.UnitSystem]
    units: Mapping[str, object]
    grid_defaults: Mapping[str, float]
    construction_plane: Mapping[str, float]
    dimension_style: Mapping[str, object]
    linetype_table: Mapping[str, float]
    view_mode: Guid
    camera: Mapping[str, object]
    perspective_lens: float
    dimension_styles: int
    render: Render
    render_mesh: MeshingParameterStyle
    sun: Mapping[str, object]
    sun_moment: datetime
    earth_anchor: Mapping[str, float]
    model_axes: Mapping[str, tuple[float, float]]
    environment: Environment
    layers: tuple[Mapping[str, object], Mapping[str, object]]
    dimension_layer: tuple[bool, UUID]
    layout: tuple[Page, ...]
    section_styles: tuple[Mapping[str, object], ...]
    saved_states: tuple[str, ...]
    hatch_patterns: tuple[str, ...]
    linetypes: Mapping[str, Dashes]


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [UNITS]
def unit_system(unit: Length) -> Rhino.UnitSystem:
    """Unit system of the length unit."""
    return System.Enum.Parse(clr.GetClrType(Rhino.UnitSystem), unit.name, ignoreCase=True)


def distance_display(unit: Length, *, page: bool) -> Rhino.UI.DistanceDisplayMode:
    """Distance display of a model or page unit."""
    match unit, page:
        case Length.FEET, False:
            return Rhino.UI.DistanceDisplayMode.FeetInches
        case Length.INCHES, True:
            return Rhino.UI.DistanceDisplayMode.Fractional
        case _:
            return Rhino.UI.DistanceDisplayMode.Decimal


def length_display(unit: Length) -> DimensionStyle.LengthDisplay:
    """Dimension length display of a unit."""
    match unit:
        case Length.FEET:
            return DimensionStyle.LengthDisplay.FeetAndInches
        case Length.MILLIMETERS:
            return DimensionStyle.LengthDisplay.Millmeters
        case _:
            return DimensionStyle.LengthDisplay.ModelUnits


def precision(units: Units, unit: Length, mode: Rhino.UI.DistanceDisplayMode) -> int:
    """Display precision stating the resolution, decimal digits of the unit under decimal display and the power of two of its fraction otherwise, an inch fraction under feet-inches display."""
    return units.places(Length.INCHES if mode == Rhino.UI.DistanceDisplayMode.FeetInches else unit, 10 if mode == Rhino.UI.DistanceDisplayMode.Decimal else 2)


# --- [NATIVE]
def template_folder() -> Path:
    """User `Template Files` folder the new-document chooser lists, created by Rhino's app controller when missing."""
    send, selector = PROCESS.objc_msgSend, PROCESS.sel_registerName
    send.restype, send.argtypes, selector.restype, selector.argtypes = ctypes.c_void_p, [ctypes.c_void_p, ctypes.c_void_p], ctypes.c_void_p, [ctypes.c_char_p]
    path = send(ctypes.c_void_p.in_dll(PROCESS, "AppCtlr"), selector(b"customTemplateDirectoryPath"))
    return Path(ctypes.string_at(send(path, selector(b"UTF8String"))).decode())


def document_properties(doc: Rhino.RhinoDoc) -> int:
    """Address of the document's native `CRhinoDocProperties`."""
    find, owner = PROCESS["_ZN9CRhinoDoc23FromRuntimeSerialNumberEj"], PROCESS["_ZN9CRhinoDoc10PropertiesEv"]
    find.restype, owner.restype, owner.argtypes = ctypes.c_void_p, ctypes.c_void_p, [ctypes.c_void_p]
    return owner(find(ctypes.c_uint(doc.RuntimeSerialNumber)))


def dimension_layer(doc: Rhino.RhinoDoc) -> tuple[bool, UUID]:
    """Whether new dimensions and leaders go to the document's dimension layer, and that layer's id."""
    native = document_properties(doc)
    use, layer = PROCESS["_ZNK19CRhinoDocProperties17UseDimensionLayerEv"], PROCESS["_ZNK19CRhinoDocProperties16DimensionLayerIdEv"]
    use.restype, use.argtypes, layer.restype, layer.argtypes = ctypes.c_bool, [ctypes.c_void_p], UuidStruct, [ctypes.c_void_p]
    return use(native), UUID(bytes_le=bytes(layer(native).data))


def route_annotation(doc: Rhino.RhinoDoc, route: tuple[bool, UUID]) -> None:
    """Set whether new dimensions and leaders go to the layer with the id."""
    setter = PROCESS["_ZN19CRhinoDocProperties20SetUseDimensionLayerEbRK14ON_UUID_struct"]
    setter.argtypes = [ctypes.c_void_p, ctypes.c_bool, ctypes.POINTER(UuidStruct)]
    use, layer = route
    setter(document_properties(doc), use, ctypes.byref(UuidStruct.from_buffer_copy(layer.bytes_le)))


# --- [DOCUMENT]
def new_hatches() -> tuple[HatchPattern, ...]:
    """Rhino's current hatch pattern set without the system patterns `HatchPattern.Defaults` names, Solid kept."""
    system = frozenset(str(prop.GetValue(None).Id) for prop in clr.GetClrType(HatchPattern.Defaults).GetProperties()) - {str(HatchPattern.Defaults.Solid.Id)}
    return tuple(pattern for pattern in HatchPattern.GetDefaultHatchPatterns() if str(pattern.Id) not in system)


def segments(linetype: Linetype) -> tuple[float, ...]:
    """Segment lengths of a linetype in millimeters, each gap negative."""
    return tuple(length if solid else -length for length, solid in map(linetype.GetSegment, range(linetype.SegmentCount)))


def section_styles(doc: Rhino.RhinoDoc) -> tuple[SectionStyle, ...]:
    """Document's section styles not deleted, by table index."""
    return tuple(style for style in map(doc.SectionStyles.FindIndex, range(doc.SectionStyles.Count)) if not style.IsDeleted)


def model_views(file: File3dm, path: str) -> tuple[ViewInfo, ...]:
    """File's model views without its layout pages."""
    pages = {str(page.Viewport.Id) for page in File3dm.ReadPageViews(path)}
    return tuple(view for view in file.Views if str(view.Viewport.Id) not in pages)


def saved_states(doc: Rhino.RhinoDoc) -> tuple[tuple[object, tuple[str, ...]], ...]:
    """Each saved-state table of the document with the names it holds."""
    return (
        (doc.NamedViews, tuple(view.Name for view in doc.NamedViews)),
        (doc.NamedConstructionPlanes, tuple(plane.Name for plane in doc.NamedConstructionPlanes)),
        (doc.NamedPositions, tuple(doc.NamedPositions.Names)),
        (doc.NamedLayerStates, tuple(doc.NamedLayerStates.Names)),
    )


def model_axes() -> dict[str, tuple[float, float]]:
    """Model north and east axes of the site's north as x and y components."""
    north = math.radians(NORTH)
    sine, cosine = math.sin(north), math.cos(north)
    return {"ModelNorth": (-sine, cosine), "ModelEast": (cosine, sine)}


# --- [TARGET]
def target(units: Units) -> Template:
    """Every template fact the unit system decides, lengths in model or page units."""
    other, cut, weight, ink = next(each for each in Units if each is not units), "Cut", Pen.THIN / Length.MILLIMETERS, color(Ink.DOCUMENT)
    page_system, model_display, page_display = unit_system(units.page), distance_display(units.length, page=False), distance_display(units.page, page=True)
    model_precision, (width, height), margin = precision(units, units.length, model_display), (side / units.page for side in units.paper), units.margin / units.page
    grid = {"GridSpacing": units.grid / units.length, "SnapSpacing": units.snap / units.length, "GridLineCount": units.grid_lines}
    sun_light_factor, sun_color_luminance, sky_radiance_per_multiplier = 3.2, 0.97436, 0.013955
    sizes = {
        "TextHeight": units.text,
        "BaselineSpacing": 4 * units.text,
        "ArrowLength": units.text,
        "LeaderArrowLength": units.text,
        "TextGap": units.resolution,
        "ExtensionLineExtension": units.extension,
        "ExtensionLineOffset": units.offset,
    }
    linetypes = {"Hidden": ((4, -2), Pen.FINE), "Center": ((16, -2, 2, -2), Pen.THIN), "Phantom": ((16, -2, 2, -2, 2, -2), Pen.FINE), "Construction": ((2, -2), Pen.FINE)}
    return {
        "unit_systems": {"ModelUnitSystem": unit_system(units.length), "PageUnitSystem": page_system},
        "units": {
            "ModelAbsoluteTolerance": units.tolerance / units.length,
            "ModelRelativeTolerance": 0.01,
            "ModelAngleToleranceDegrees": 1,
            "ModelDistanceDisplayMode": model_display,
            "ModelDistanceDisplayPrecision": model_precision,
            "PageAbsoluteTolerance": units.tolerance / units.page,
            "PageDistanceDisplayMode": page_display,
            "PageDistanceDisplayPrecision": precision(units, units.page, page_display),
            "ModelSpaceAnnotationScalingEnabled": True,
            "LayoutSpaceAnnotationScalingEnabled": True,
            "ModelSpaceTextScale": 1.0,
            "ModelSpaceHatchScalingEnabled": True,
            "ModelSpaceHatchScale": 1.0,
        },
        "grid_defaults": {**grid, "GridThickFrequency": GRID_THICK_EVERY},
        "construction_plane": {**grid, "ThickLineFrequency": GRID_THICK_EVERY},
        "dimension_style": {
            "Name": {Units.IMPERIAL: "Template Foot-Inch Architectural", Units.METRIC: "Template Millimeter Architectural"}[units],
            "DimensionLengthDisplay": length_display(units.length),
            "LengthResolution": model_precision,
            "AlternateDimensionLengthDisplay": length_display(other.length),
            "AlternateLengthResolution": precision(other, other.length, distance_display(other.length, page=False)),
            "AlternateUnitsDisplay": False,
            "DrawTextMask": False,
            "ArrowType1": DimensionStyle.ArrowType.Tick,
            "ArrowType2": DimensionStyle.ArrowType.Tick,
            "LeaderArrowType": DimensionStyle.ArrowType.SolidTriangle,
            "AngleResolution": ANGLE_PRECISION,
            "Font": Font.FromQuartetProperties(Typography.INTERFACE.family, bold=False, italic=False),
            "DimensionScale": units.sheet_scale * units.page / units.length,
            **{name: size / units.page for name, size in sizes.items()},
        },
        "linetype_table": {"LinetypeScale": 1.0},
        "view_mode": DisplayModeDescription.ShadedId,
        "camera": {"MaximizedMatchesPerspective": True, "TargetPoint": (0.0, 0.0, 0.0)},
        "perspective_lens": LENS,
        "dimension_styles": 1,
        "render": {
            "ImageUnitSystem": page_system,
            "UseViewportSize": False,
            "ImageSize": FRAME_SIZE,
            "ImageDpi": DPI,
            "Dithering": True,
            "GroundPlane": False,
            "UserDictionary": {
                "UseDocumentSamples": True,
                "Samples": SAMPLES,
                "AdaptiveThreshold": NOISE_THRESHOLD,
                "MaxBounce": MAX_BOUNCES,
                "MaxDiffuseBounce": DIFFUSE_BOUNCES,
                "MaxGlossyBounce": GLOSSY_BOUNCES,
                "MaxTransmissionBounce": TRANSMISSION_BOUNCES,
                "MaxVolumeBounce": VOLUME_BOUNCES,
                "TransparentMaxBounce": TRANSPARENT_BOUNCES,
                "SampleClampDirect": 0.0,
                "SampleClampIndirect": INDIRECT_CLAMP,
                "FilterGlossy": FILTER_GLOSSY,
                "CausticsReflective": CAUSTICS,
                "CausticsRefractive": CAUSTICS,
            },
        },
        "render_mesh": MeshingParameterStyle.Quality,
        "sun": {
            "Enabled": True,
            "Latitude": LATITUDE,
            "Longitude": LONGITUDE,
            "TimeZone": OFFSET / timedelta(hours=1),
            "DaylightSavingOn": timedelta() < DAYLIGHT,
            "DaylightSavingMinutes": DAYLIGHT // timedelta(minutes=1),
            "North": 90 + NORTH,
            "Intensity": SUN_IRRADIANCE * 2**EXPOSURE / (sun_light_factor * sun_color_luminance),
        },
        "sun_moment": MOMENT,
        "earth_anchor": {"EarthBasepointLatitude": LATITUDE, "EarthBasepointLongitude": LONGITUDE, "EarthBasepointElevation": ELEVATION},
        "model_axes": model_axes(),
        "environment": {
            "BackgroundStyle": BackgroundStyle.Environment,
            "Texture": {"TypeId": ContentUuids.PhysicalSkyTextureType, SKY_SUN: True, SKY_MULTIPLIER: SKY_RADIANCE * 2**EXPOSURE / sky_radiance_per_multiplier},
            "UsageEnvironments": 1,
            "FileEnvironments": 1,
        },
        "layers": (
            {"Id": ANNOTATION_ID, "Name": Annotation.NAME, "Color": color(Annotation.TAG.value), "PlotColor": ink, "PlotWeight": weight, "SectionStyle": None},
            {"Color": ink, "PlotColor": ink, "PlotWeight": weight, "SectionStyle": cut},
        ),
        "dimension_layer": (True, Annotation.ID),
        "layout": ({"PageWidth": width, "PageHeight": height, "Objects": ((Annotation.NAME, (margin, margin), (width - margin, height - margin)),)},),
        "section_styles": (
            {
                "Name": cut,
                "BoundaryWidthScale": 1.0,
                "BoundaryPlotWeightMillimeters": Pen.MEDIUM / Length.MILLIMETERS,
                "BackgroundFillMode": SectionBackgroundFillMode.SolidColor,
                "BackgroundFillColor": color(Surface.SECTION),
                "BackgroundFillPrintColor": color(Surface.SECTION),
                "SectionFillRule": ObjectSectionFillRule.SolidObjects,
            },
        ),
        "saved_states": (),
        "hatch_patterns": tuple(sorted(pattern.Name for pattern in new_hatches())),
        "linetypes": {
            name: {"Segments": tuple(step * units.resolution / Length.MILLIMETERS for step in steps), "Width": pen / Length.MILLIMETERS, "WidthUnits": Rhino.UnitSystem.Millimeters}
            for name, (steps, pen) in linetypes.items()
        },
    }


# --- [READS]
def properties(owner: object, names: Iterable[str]) -> dict[str, object]:
    """Owner's members of the names by name."""
    return {name: getattr(owner, name) for name in names}


def uniform(values: Iterable[object]) -> object:
    """Plain value every item holds, or each distinct plain value in first-read order when they differ."""
    held = tuple(map(plain, values))
    match tuple(each for index, each in enumerate(held) if each not in held[:index]):
        case (value,):
            return value
        case several:
            return several


def file_sky(file: File3dm) -> dict[str, object]:
    """Background style, the background environment's sky texture from the file's render content, and the environment counts."""
    settings = file.Settings.RenderSettings
    ids = {usage: settings.RenderEnvironmentId(usage, RenderSettings.EnvironmentPurpose.Standard) for usage in SKY_USAGES}
    background = ids[RenderSettings.EnvironmentUsage.Background]
    held = next((child for environment in file.RenderEnvironments if environment.Id == background for child in environment.Children if child.ChildSlotName == SKY_SLOT), None)
    return {
        "BackgroundStyle": settings.BackgroundStyle,
        "Texture": None
        if held is None
        else {"TypeId": held.TypeId, SKY_SUN: System.Convert.ToBoolean(held.GetParameter(SKY_SUN)), SKY_MULTIPLIER: System.Convert.ToDouble(held.GetParameter(SKY_MULTIPLIER))},
        "UsageEnvironments": len(set(ids.values())),
        "FileEnvironments": len(tuple(file.RenderEnvironments)),
    }


def layer_facts(doc: Rhino.RhinoDoc, layer: Layer) -> dict[str, object]:
    """Colors, plot weight, and section style name of a layer, with its id and name for the annotation layer."""
    held = {**properties(layer, ("Color", "PlotColor", "PlotWeight")), "SectionStyle": doc.SectionStyles[layer.SectionStyleIndex].Name if layer.SectionStyleIndex >= 0 else None}
    return {**properties(layer, ("Id", "Name")), **held} if layer.Id == ANNOTATION_ID else held


def page_facts(doc: Rhino.RhinoDoc, page: RhinoPageView) -> dict[str, object]:
    """Size of a layout page, and the layer and corners of each object drawn on it."""
    settings = ObjectEnumeratorSettings()
    settings.HiddenObjects = True
    boxes = (
        (doc.Layers[each.Attributes.LayerIndex].Name, each.Geometry.GetBoundingBox(accurate=True)) for each in doc.Objects.GetObjectList(settings) if each.Attributes.ViewportId == page.MainViewport.Id
    )
    return {**properties(page, ("PageWidth", "PageHeight")), "Objects": tuple((name, (box.Min.X, box.Min.Y), (box.Max.X, box.Max.Y)) for name, box in boxes)}


def read(path: str, template: Template) -> dict[str, object] | None:
    """Every fact the template names, read from the file and a headless document opened on it, None when the file is missing."""
    try:
        file = File3dm.Read(path)
    except FileNotFoundException:
        return None
    with ExitStack() as scope:
        scope.callback(file.Dispose)
        doc = Rhino.RhinoDoc.CreateHeadless(path)
        scope.callback(doc.Dispose)
        settings, anchor, viewports = doc.RenderSettings, doc.EarthAnchorPoint, tuple(view.ActiveViewport for view in doc.Views.GetViewList(ViewTypeFilter.Model))
        cameras, moment = tuple((view.Maximized, view.Viewport.IsPerspectiveProjection, view.Viewport.TargetPoint) for view in model_views(file, path)), settings.Sun.GetDateTime(DateTimeKind.Local)
        return {
            "unit_systems": properties(doc, template["unit_systems"]),
            "units": properties(doc, template["units"]),
            "grid_defaults": properties(doc.GetGridDefaults(), template["grid_defaults"]),
            "construction_plane": uniform(properties(viewport.GetConstructionPlane(), template["construction_plane"]) for viewport in viewports),
            "dimension_style": properties(doc.DimStyles.Current, template["dimension_style"]),
            "linetype_table": properties(doc.Linetypes, template["linetype_table"]),
            "view_mode": uniform(viewport.DisplayMode.Id for viewport in viewports),
            "camera": uniform({"MaximizedMatchesPerspective": maximized == perspective, "TargetPoint": (point.X, point.Y, point.Z)} for maximized, perspective, point in cameras),
            "perspective_lens": uniform(viewport.Camera35mmLensLength for viewport in viewports if viewport.IsPerspectiveProjection),
            "dimension_styles": doc.DimStyles.Count,
            "render": {
                **properties(settings, ("ImageUnitSystem", "UseViewportSize", "ImageDpi")),
                "ImageSize": (settings.ImageSize.Width, settings.ImageSize.Height),
                "Dithering": settings.Dithering.Enabled,
                "GroundPlane": settings.GroundPlane.Enabled,
                "UserDictionary": {name: found(settings.UserDictionary.TryGetValue(name)) for name in template["render"]["UserDictionary"]},
            },
            "render_mesh": doc.MeshingParameterStyle,
            "sun": properties(settings.Sun, template["sun"]),
            "sun_moment": datetime(moment.Year, moment.Month, moment.Day, moment.Hour, moment.Minute, tzinfo=template["sun_moment"].tzinfo),
            "earth_anchor": properties(anchor, template["earth_anchor"]),
            "model_axes": {name: (axis.X, axis.Y) for name, axis in properties(anchor, template["model_axes"]).items()},
            "environment": file_sky(file),
            "layers": tuple(layer_facts(doc, layer) for layer in sorted((layer for layer in doc.Layers if not layer.IsDeleted), key=lambda layer: layer.SortIndex)),
            "dimension_layer": dimension_layer(doc),
            "layout": tuple(page_facts(doc, page) for page in doc.Views.GetPageViews()),
            "section_styles": tuple(properties(style, dict.fromkeys(name for facts in template["section_styles"] for name in facts)) for style in section_styles(doc)),
            "saved_states": tuple(name for _, names in saved_states(doc) for name in names),
            "hatch_patterns": tuple(sorted(pattern.Name for pattern in doc.HatchPatterns if not pattern.IsDeleted)),
            "linetypes": {linetype.Name: {"Segments": segments(linetype), **properties(linetype, ("Width", "WidthUnits"))} for linetype in doc.Linetypes if not linetype.IsDeleted},
        }


# --- [WRITES]
def assign(owner: object, members: Mapping[str, object]) -> None:
    """Set each named member of the owner to its value."""
    for name, value in members.items():
        setattr(owner, name, value)


def write_units(doc: Rhino.RhinoDoc, template: Template) -> None:
    """Write the unit systems and settings, the model views, the grid defaults, and one dimension style."""
    systems, plane_facts, style_facts = template["unit_systems"], template["construction_plane"], template["dimension_style"]
    doc.AdjustModelUnitSystem(systems["ModelUnitSystem"], scale=False)
    doc.AdjustPageUnitSystem(systems["PageUnitSystem"], scale=False)
    assign(doc, template["units"])
    mode, extent = DisplayModeDescription.GetDisplayMode(template["view_mode"]), plane_facts["GridSpacing"] * plane_facts["ThickLineFrequency"]
    for view in doc.Views.GetViewList(ViewTypeFilter.Model):
        if (viewport := view.ActiveViewport).IsPerspectiveProjection:
            viewport.Camera35mmLensLength = template["perspective_lens"]
        viewport.ZoomBoundingBox(BoundingBox(-extent, -extent, 0, extent, extent, 0))
        plane = viewport.GetConstructionPlane()
        assign(plane, plane_facts)
        viewport.SetConstructionPlane(plane)
        viewport.DisplayMode = mode
    defaults = doc.GetGridDefaults()
    assign(defaults, template["grid_defaults"])
    doc.SetGridDefaults(defaults)
    style = doc.DimStyles.Current
    for extra in [each for each in doc.DimStyles if each.Id != style.Id]:
        doc.DimStyles.Delete(extra.Index, quiet=True)
    style.CopyFrom(next(each for each in doc.DimStyles.BuiltInStyles if each.Name == style_facts["Name"]))
    assign(style, style_facts)
    doc.DimStyles.Modify(style, style.Id, quiet=True)


def write_render(doc: Rhino.RhinoDoc, template: Template) -> None:
    """Write the render, sun, sky, and earth anchor settings."""
    render, environment_facts, settings = template["render"], template["environment"], doc.RenderSettings
    doc.MeshingParameterStyle = template["render_mesh"]
    settings.ImageUnitSystem, settings.UseViewportSize, settings.ImageSize, settings.ImageDpi = render["ImageUnitSystem"], render["UseViewportSize"], Size(*render["ImageSize"]), render["ImageDpi"]
    settings.Dithering.Enabled, settings.GroundPlane.Enabled = render["Dithering"], render["GroundPlane"]
    for name, value in render["UserDictionary"].items():
        settings.UserDictionary.Set(name, val=value)
    assign(settings.Sun, template["sun"])
    settings.Sun.SetDateTime(DateTime(*template["sun_moment"].timetuple()[:6]), DateTimeKind.Local)
    for held in tuple(doc.RenderEnvironments):
        doc.RenderEnvironments.Remove(held)
    environment = RenderContentType.NewContentFromTypeId(ContentUuids.BasicEnvironmentType, doc)
    environment.Name = "Sky"
    sky = environment_facts["Texture"]
    texture = RenderContentType.NewContentFromTypeId(sky["TypeId"], doc)
    texture.BeginChange(RenderContent.ChangeContexts.Program)
    for name in (SKY_SUN, SKY_MULTIPLIER):
        texture.SetParameter(name, value=sky[name])
    texture.EndChange()
    environment.SetChild(texture, SKY_SLOT)
    doc.RenderEnvironments.Add(environment)
    settings.BackgroundStyle = environment_facts["BackgroundStyle"]
    for usage in SKY_USAGES:
        settings.SetRenderEnvironmentId(usage, environment.Id)
    doc.RenderSettings = settings
    anchor = doc.EarthAnchorPoint
    assign(anchor, template["earth_anchor"])
    for name, (x, y) in template["model_axes"].items():
        setattr(anchor, name, Vector3d(x, y, 0))
    doc.EarthAnchorPoint = anchor


def write_tables(doc: Rhino.RhinoDoc, template: Template) -> None:
    """Write the hatch, section style, layer, linetype, and saved-state tables, dimensions routed to the annotation layer."""
    hatches, styles, (annotation_facts, current_facts) = template["hatch_patterns"], template["section_styles"], template["layers"]
    for pattern in [pattern for pattern in doc.HatchPatterns if pattern.Name not in hatches]:
        doc.HatchPatterns.Delete(pattern.Index, quiet=True)
    held = {pattern.Name for pattern in doc.HatchPatterns if not pattern.IsDeleted}
    for pattern in [pattern for pattern in HatchPattern.GetDefaultHatchPatterns() if pattern.Name in hatches and pattern.Name not in held]:
        doc.HatchPatterns.Add(pattern)
    for facts in styles:
        style = SectionStyle()
        assign(style, facts)
        if (index := doc.SectionStyles.Find(style.Name)) < 0:
            doc.SectionStyles.Add(style)
        else:
            doc.SectionStyles.Modify(style, index, quiet=True)
    current = doc.Layers.CurrentLayerIndex
    for layer in [layer for layer in doc.Layers if not layer.IsDeleted and layer.Index != current]:
        doc.Layers.Delete(layer.Index, quiet=True)
    annotation, layer = Layer(), doc.Layers.CurrentLayer
    annotation.LinetypeIndex, annotation.IsVisible, annotation.IsLocked = -1, True, False
    for held, facts in ((annotation, annotation_facts), (layer, current_facts)):
        assign(held, {name: value for name, value in facts.items() if name != "SectionStyle"})
        held.SectionStyleIndex = -1 if facts["SectionStyle"] is None else doc.SectionStyles.Find(facts["SectionStyle"])
    doc.Layers.Modify(layer, current, quiet=True)
    doc.Layers.Sort([doc.Layers.Add(annotation), current])
    route_annotation(doc, template["dimension_layer"])
    for other in [other for other in section_styles(doc) if other.Name not in {facts["Name"] for facts in styles}]:
        doc.SectionStyles.Delete(other.Index, quiet=True)
    linetypes = template["linetypes"]
    doc.Linetypes.LoadDefaultLinetypes()
    assign(doc.Linetypes, template["linetype_table"])
    for name in [name for name in linetypes if doc.Linetypes.Find(name) < 0]:
        doc.Linetypes.Add(name, Array[float](linetypes[name]["Segments"]))
    for name, facts in linetypes.items():
        linetype = doc.Linetypes.FindName(name)
        linetype.SetSegments(Array[float](facts["Segments"]))
        linetype.Width, linetype.WidthUnits = facts["Width"], facts["WidthUnits"]
        doc.Linetypes.Modify(linetype, linetype.Index, quiet=True)
    for linetype in [linetype for linetype in doc.Linetypes if not linetype.IsDeleted and linetype.Name not in linetypes]:
        doc.Linetypes.Delete(linetype.Index, quiet=True)
    for table, names in saved_states(doc):
        for name in [name for name in names if name not in template["saved_states"]]:
            table.Delete(name)


def write_layout(doc: Rhino.RhinoDoc, pages: tuple[Page, ...]) -> None:
    """Replace the layout pages with the target's, each object a rectangle on its layer."""
    for page in doc.Views.GetPageViews():
        page.Close()
    for facts in pages:
        page = doc.Views.AddPageView(None, facts["PageWidth"], facts["PageHeight"])
        for name, (left, bottom), (right, top) in facts["Objects"]:
            attributes = ObjectAttributes()
            attributes.LayerIndex, attributes.Space, attributes.ViewportId = doc.Layers.FindName(name).Index, ActiveSpace.PageSpace, page.MainViewport.Id
            doc.Objects.AddRectangle(Rectangle3d(Plane.WorldXY, Point3d(left, bottom, 0), Point3d(right, top, 0)), attributes)


def write(path: str, source: str, template: Template) -> None:
    """Write the template through a headless document on the bundled source template, Perspective alone maximized in the file."""
    doc = Rhino.RhinoDoc.CreateHeadless(source)
    try:
        write_units(doc, template)
        write_render(doc, template)
        write_tables(doc, template)
        write_layout(doc, template["layout"])
        written = doc.WriteFile(path, FileWriteOptions())
    finally:
        doc.Dispose()
    if not written:
        raise OSError(f"the headless document was not written to {path}")
    file = File3dm.Read(path)
    try:
        for view in model_views(file, path):
            view.Maximized = view.Viewport.IsPerspectiveProjection
        rewritten = file.Write(path, File3dmWriteOptions())
    finally:
        file.Dispose()
    if not rewritten:
        raise OSError(f"the template file was not rewritten to {path}")


# --- [COMPOSITION] ----------------------------------------------------------------------


def rows() -> tuple[Row, ...]:
    """Rows of the imperial default template and the metric one in the user template folder, each built on a bundled template of its units, then the default template setting naming the imperial one."""
    folder, bundle = template_folder(), Path(FileSettings.TemplateFolder)
    default = str(folder / "Default.3dm")
    templates = ((default, "Large Objects - Feet, Feet & Inches.3dm", target(Units.IMPERIAL)), (str(folder / "Metric.3dm"), "Large Objects - Millimeters.3dm", target(Units.METRIC)))
    return (
        *(Row(label=f'templates["{Path(path).name}"]', read=partial(read, path, facts), write=partial(write, path, str(bundle / source)), target=facts) for path, source, facts in templates),
        member(FileSettings, "TemplateFile", target=default),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows"]
