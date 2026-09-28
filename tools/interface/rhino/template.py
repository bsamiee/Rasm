# ty: ignore[unresolved-attribute, unresolved-import, invalid-argument-type, unsupported-operator]
# mypy: disable-error-code="import-untyped, import-not-found, no-any-unimported, attr-defined, operator"
# ruff: file-ignore[banned-api]
"""Rows of Rhino's imperial default template and the metric one beside it."""

from collections.abc import Iterable, Mapping
import ctypes
from dataclasses import dataclass
from datetime import timedelta
from functools import partial
import math
from pathlib import Path
from types import MappingProxyType
from typing import Final

import clr
import Rhino
import Rhino.ApplicationSettings as Settings
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
from System import Array, DateTime, DateTimeKind, Guid, UInt32
from System.Drawing import Size
from System.Reflection import BindingFlags

from interface.render import (
    CAUSTICS,
    DAYLIGHT,
    DIFFUSE_BOUNCES,
    DPI,
    ELEVATION,
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
    TRANSMISSION_BOUNCES,
    TRANSPARENT_BOUNCES,
    VOLUME_BOUNCES,
)
from interface.report import Row
from interface.rhino.rows import color, found
from interface.roles import Annotation, Ink, Surface, Typography
from interface.units import ANGLE_PRECISION, GRID_THICK_EVERY, INCH, MILLIMETER, Pen, Units

# --- [CONSTANTS] ------------------------------------------------------------------------

CUT_STYLE: Final = "Cut"
SKY_SLOT: Final = "texture"
SKY_SUN: Final = "use-document-sun"
SKY_MULTIPLIER: Final = "rdk-texture-adjust-multiplier"
PLOT_WEIGHT: Final = Pen.THIN / MILLIMETER
LINETYPE_SCALE: Final = 1.0
RENDER_KEYS: Final = MappingProxyType({
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
})
ANNOTATION_ID: Final = Guid.Parse(str(Annotation.ID))

# --- [TABLES] ---------------------------------------------------------------------------

SKY_USAGES: Final = tuple(System.Enum.GetValues(clr.GetClrType(RenderSettings.EnvironmentUsage)))
SKY: Final = MappingProxyType({"TypeId": str(ContentUuids.PhysicalSkyTextureType), SKY_SUN: True, SKY_MULTIPLIER: 8.21})
SUN: Final = MappingProxyType({
    "Enabled": True,
    "Latitude": LATITUDE,
    "Longitude": LONGITUDE,
    "TimeZone": OFFSET / timedelta(hours=1),
    "DaylightSavingOn": timedelta() < DAYLIGHT,
    "DaylightSavingMinutes": DAYLIGHT // timedelta(minutes=1),
    "North": 90 + NORTH,
    "Intensity": 1.115,
})
ANCHOR: Final = MappingProxyType({"EarthBasepointLatitude": LATITUDE, "EarthBasepointLongitude": LONGITUDE, "EarthBasepointElevation": ELEVATION})

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class DocumentUnits:
    """Template facts one units declaration decides, lengths in model or page units."""

    model: Rhino.UnitSystem
    page: Rhino.UnitSystem
    document: Mapping[str, object]
    grid: float
    snap: float
    grid_lines: int
    style: str
    dimension: Mapping[str, object]
    linetypes: Mapping[str, tuple[tuple[float, ...], float]]
    paper: tuple[float, float]
    margin: float


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [UNITS]
def unit_system(unit: str) -> Rhino.UnitSystem:
    """Unit system a units token names."""
    return System.Enum.Parse(clr.GetClrType(Rhino.UnitSystem), unit, ignoreCase=True)


def rounded(value: float) -> float:
    """Length rounded past the noise a unit conversion leaves."""
    return round(value, 9)


def distance_display(system: Rhino.UnitSystem, *, page: bool) -> Rhino.UI.DistanceDisplayMode:
    """Distance display of a model or page unit system."""
    match system, page:
        case Rhino.UnitSystem.Feet, False:
            return Rhino.UI.DistanceDisplayMode.FeetInches
        case Rhino.UnitSystem.Inches, True:
            return Rhino.UI.DistanceDisplayMode.Fractional
        case _:
            return Rhino.UI.DistanceDisplayMode.Decimal


def length_display(system: Rhino.UnitSystem) -> DimensionStyle.LengthDisplay:
    """Dimension length display of a unit system."""
    match system:
        case Rhino.UnitSystem.Feet:
            return DimensionStyle.LengthDisplay.FeetAndInches
        case Rhino.UnitSystem.Millimeters:
            return DimensionStyle.LengthDisplay.Millmeters
        case _:
            return DimensionStyle.LengthDisplay.ModelUnits


def precision(units: Units, system: Rhino.UnitSystem, mode: Rhino.UI.DistanceDisplayMode) -> int:
    """Display precision reaching the declared resolution."""
    decimal = mode == Rhino.UI.DistanceDisplayMode.Decimal
    unit = 1 / Rhino.RhinoMath.UnitScale(Rhino.UnitSystem.Meters, system) if decimal else INCH
    return round(math.log(unit / units.resolution, 10 if decimal else 2))


def document_units(units: Units) -> DocumentUnits:
    """Template facts of a units declaration, style lengths scaled from sheet to model units."""
    style, other = {Units.IMPERIAL: ("Template Foot-Inch Architectural", Units.METRIC), Units.METRIC: ("Template Millimeter Architectural", Units.IMPERIAL)}[units]
    model, page, alternate = unit_system(units.length), unit_system(units.page), unit_system(other.length)
    scale, paper = (Rhino.RhinoMath.UnitScale(Rhino.UnitSystem.Meters, system) for system in (model, page))
    model_display, page_display = distance_display(model, page=False), distance_display(page, page=True)
    model_precision, ratio = precision(units, model, model_display), units.resolution / MILLIMETER
    linetypes = {"Hidden": ((4, -2), Pen.FINE), "Center": ((16, -2, 2, -2), Pen.THIN), "Phantom": ((16, -2, 2, -2, 2, -2), Pen.FINE), "Construction": ((2, -2), Pen.FINE)}
    sizes = {
        "TextHeight": units.text,
        "BaselineSpacing": 4 * units.text,
        "ArrowLength": units.text,
        "LeaderArrowLength": units.text,
        "TextGap": units.resolution,
        "ExtensionLineExtension": units.extension,
        "ExtensionLineOffset": units.offset,
    }
    width, height = (rounded(side * paper) for side in units.paper)
    return DocumentUnits(
        model=model,
        page=page,
        document={
            "ModelAbsoluteTolerance": rounded(units.tolerance * scale),
            "ModelRelativeTolerance": 0.01,
            "ModelAngleToleranceDegrees": 1,
            "ModelDistanceDisplayMode": model_display,
            "ModelDistanceDisplayPrecision": model_precision,
            "PageAbsoluteTolerance": rounded(units.tolerance * paper),
            "PageDistanceDisplayMode": page_display,
            "PageDistanceDisplayPrecision": precision(units, page, page_display),
            "ModelSpaceAnnotationScalingEnabled": True,
            "LayoutSpaceAnnotationScalingEnabled": True,
            "ModelSpaceTextScale": 1.0,
            "ModelSpaceHatchScalingEnabled": True,
            "ModelSpaceHatchScale": 1.0,
        },
        grid=rounded(units.grid * scale),
        snap=rounded(units.snap * scale),
        grid_lines=units.grid_lines,
        style=style,
        dimension={
            "DimensionLengthDisplay": length_display(model),
            "LengthResolution": model_precision,
            "AlternateDimensionLengthDisplay": length_display(alternate),
            "AlternateLengthResolution": precision(other, alternate, distance_display(alternate, page=False)),
            "AlternateUnitsDisplay": False,
            "DrawTextMask": False,
            "ArrowType1": DimensionStyle.ArrowType.Tick,
            "ArrowType2": DimensionStyle.ArrowType.Tick,
            "LeaderArrowType": DimensionStyle.ArrowType.SolidTriangle,
            "AngleResolution": ANGLE_PRECISION,
            "Font": Font.FromQuartetProperties(Typography.INTERFACE.family, bold=False, italic=False),
            "DimensionScale": rounded(units.sheet_scale * units.page_unit * scale),
            **{name: rounded(size * paper) for name, size in sizes.items()},
        },
        linetypes={name: (tuple(rounded(steps * ratio) for steps in lengths), pen / MILLIMETER) for name, (lengths, pen) in linetypes.items()},
        paper=(width, height),
        margin=rounded(units.margin * paper),
    )


# --- [DOCUMENT]
def new_hatches() -> tuple[HatchPattern, ...]:
    """Rhino's current hatch pattern set without the system patterns `HatchPattern.Defaults` names, Solid kept."""
    system = frozenset(str(prop.GetValue(None).Id) for prop in clr.GetClrType(HatchPattern.Defaults).GetProperties()) - {str(HatchPattern.Defaults.Solid.Id)}
    return tuple(pattern for pattern in HatchPattern.GetDefaultHatchPatterns() if str(pattern.Id) not in system)


def segments(linetype: Linetype) -> tuple[float, ...]:
    """Segment lengths of a linetype in millimeters, each gap negative."""
    return tuple(length if solid else -length for length, solid in map(linetype.GetSegment, range(linetype.SegmentCount)))


def cut_style() -> SectionStyle:
    """Cut section style filling the section surface of solid objects, its boundary at the cut pen weight."""
    style = SectionStyle()
    style.Name, style.BoundaryWidthScale, style.BoundaryPlotWeightMillimeters = CUT_STYLE, 1.0, Pen.MEDIUM / MILLIMETER
    style.BackgroundFillMode, style.SectionFillRule = SectionBackgroundFillMode.SolidColor, ObjectSectionFillRule.SolidObjects
    style.BackgroundFillColor = style.BackgroundFillPrintColor = color(Surface.SECTION)
    return style


def section_facts(style: SectionStyle) -> dict[str, object]:
    """Template facts of a section style."""
    return {
        "Name": style.Name,
        "BoundaryWidthScale": style.BoundaryWidthScale,
        "BoundaryPlotWeightMillimeters": style.BoundaryPlotWeightMillimeters,
        "BackgroundFillMode": str(style.BackgroundFillMode),
        "BackgroundFillColor": style.BackgroundFillColor.ToArgb(),
        "BackgroundFillPrintColor": style.BackgroundFillPrintColor.ToArgb(),
        "SectionFillRule": str(style.SectionFillRule),
    }


def section_styles(doc: Rhino.RhinoDoc) -> tuple[SectionStyle, ...]:
    """Document's section styles not deleted, by table index."""
    return tuple(style for style in map(doc.SectionStyles.FindIndex, range(doc.SectionStyles.Count)) if not style.IsDeleted)


def dimension_layer(doc: Rhino.RhinoDoc) -> tuple[bool, str | None]:
    """Whether new dimensions and leaders go to the document's dimension layer, and that layer's id, None when no layer resolves."""
    native, serial = clr.GetClrType(Rhino.RhinoDoc).Assembly.GetType("UnsafeNativeMethods"), Array[System.Object]([UInt32(doc.RuntimeSerialNumber)])
    use, index = (
        native.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Invoke(None, serial) for name in ("RHC_RhUseDimensionLayer", "RHC_RhGetDimensionLayerIndex")
    )
    return use, str(doc.Layers[index].Id) if index >= 0 else None


def route_annotation(doc: Rhino.RhinoDoc) -> None:
    """Route new dimensions and leaders to the annotation layer."""
    core = ctypes.CDLL(None)
    find, properties, route = (core[name] for name in ("_ZN9CRhinoDoc23FromRuntimeSerialNumberEj", "_ZN9CRhinoDoc10PropertiesEv", "_ZN19CRhinoDocProperties20SetUseDimensionLayerEbRK14ON_UUID_struct"))
    find.restype, properties.restype, properties.argtypes, route.argtypes = ctypes.c_void_p, ctypes.c_void_p, [ctypes.c_void_p], [ctypes.c_void_p, ctypes.c_bool, ctypes.c_char_p]
    route(properties(find(ctypes.c_uint(doc.RuntimeSerialNumber))), ctypes.c_bool(True), Annotation.ID.bytes_le)


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


# --- [SUN_AND_SKY]
def compass() -> dict[str, tuple[float, float]]:
    """Model north and east axes of the site's north as x and y components."""
    north = math.radians(NORTH)
    sine, cosine = math.sin(north), math.cos(north)
    return {"ModelNorth": (-sine, cosine), "ModelEast": (cosine, sine)}


def file_sky(file: File3dm) -> dict[str, object]:
    """Template facts of a file's background environment, its sky texture found by the background environment's id in the file's render content, which a headless document leaves unread, and the environment counts."""
    settings = file.Settings.RenderSettings
    ids = {usage: settings.RenderEnvironmentId(usage, RenderSettings.EnvironmentPurpose.Standard) for usage in SKY_USAGES}
    background = ids[RenderSettings.EnvironmentUsage.Background]
    held = next((child for environment in file.RenderEnvironments if environment.Id == background for child in environment.Children if child.ChildSlotName == SKY_SLOT), None)
    return {
        "BackgroundStyle": str(settings.BackgroundStyle),
        "Texture": None
        if held is None
        else {"TypeId": str(held.TypeId), SKY_SUN: System.Convert.ToBoolean(held.GetParameter(SKY_SUN)), SKY_MULTIPLIER: System.Convert.ToDouble(held.GetParameter(SKY_MULTIPLIER))},
        "UsageEnvironments": len(set(ids.values())),
        "FileEnvironments": len(tuple(file.RenderEnvironments)),
    }


# --- [FACTS]
def fact(value: object) -> object:
    """Document member as a template fact."""
    match value:
        case System.Enum():
            return str(value)
        case float():
            return rounded(value)
        case Font():
            return value.QuartetName
        case _:
            return value


def named(owner: object, names: Iterable[str]) -> dict[str, object]:
    """Owner's named members as template facts."""
    return {name: fact(getattr(owner, name)) for name in names}


def shared(values: Iterable[object]) -> object:
    """Value every item holds, or each distinct value in first-read order when they differ."""
    held = tuple(values)
    match tuple(each for index, each in enumerate(held) if each not in held[:index]):
        case (value,):
            return value
        case several:
            return several


def layer_facts(doc: Rhino.RhinoDoc, layer: Layer) -> dict[str, object]:
    """Template facts of a layer, with its id and name for the annotation layer."""
    held = {
        "Color": layer.Color.ToArgb(),
        "PlotColor": layer.PlotColor.ToArgb(),
        "PlotWeight": fact(layer.PlotWeight),
        "SectionStyle": doc.SectionStyles[layer.SectionStyleIndex].Name if layer.SectionStyleIndex >= 0 else None,
    }
    return {"Id": str(layer.Id), "Name": layer.Name, **held} if layer.Id == ANNOTATION_ID else held


def page_facts(doc: Rhino.RhinoDoc, page: RhinoPageView) -> dict[str, object]:
    """Size of a layout page, and the layer and corners of each object drawn on it."""
    settings = ObjectEnumeratorSettings()
    settings.HiddenObjects = True
    boxes = (
        (doc.Layers[each.Attributes.LayerIndex].Name, each.Geometry.GetBoundingBox(accurate=True)) for each in doc.Objects.GetObjectList(settings) if each.Attributes.ViewportId == page.MainViewport.Id
    )
    return {
        "PageWidth": fact(page.PageWidth),
        "PageHeight": fact(page.PageHeight),
        "Objects": tuple((name, (rounded(box.Min.X), rounded(box.Min.Y)), (rounded(box.Max.X), rounded(box.Max.Y))) for name, box in boxes),
    }


def member_target(units: DocumentUnits) -> dict[str, dict[str, object]]:
    """Template facts read from the RhinoCommon members of the same names, by group."""
    grid = {"GridSpacing": units.grid, "SnapSpacing": units.snap, "GridLineCount": units.grid_lines}
    return {
        "units": {"ModelUnitSystem": str(units.model), "PageUnitSystem": str(units.page), **{name: fact(value) for name, value in units.document.items()}},
        "grid defaults": {**grid, "GridThickFrequency": GRID_THICK_EVERY},
        "construction plane": {**grid, "ThickLineFrequency": GRID_THICK_EVERY},
        "dimension style": {"Name": units.style, **{name: fact(value) for name, value in units.dimension.items()}},
        "linetype table": {"LinetypeScale": LINETYPE_SCALE},
    }


def template_target(units: DocumentUnits, members: Mapping[str, dict[str, object]]) -> dict[str, object]:
    """Every template fact the declarations decide."""
    ink, (width, height), margin = color(Ink.DOCUMENT).ToArgb(), units.paper, units.margin
    return {
        **members,
        "view mode": str(DisplayModeDescription.ShadedId),
        "camera": {"MaximizedMatchesPerspective": True, "TargetPoint": (0.0, 0.0, 0.0)},
        "perspective lens": LENS,
        "dimension styles": 1,
        "render": {"ImageUnitSystem": str(units.page), "UseViewportSize": False, "ImageSize": FRAME_SIZE, "ImageDpi": DPI, "Dithering": True, "GroundPlane": False, "UserDictionary": RENDER_KEYS},
        "render mesh": str(MeshingParameterStyle.Quality),
        "sun": {**SUN, "DateTime": MOMENT.replace(tzinfo=None).isoformat(timespec="minutes")},
        "earth anchor": {**ANCHOR, **{name: (rounded(x), rounded(y)) for name, (x, y) in compass().items()}},
        "environment": {"BackgroundStyle": str(BackgroundStyle.Environment), "Texture": SKY, "UsageEnvironments": 1, "FileEnvironments": 1},
        "layers": (
            {"Id": str(Annotation.ID), "Name": Annotation.NAME, "Color": color(Annotation.TAG.value).ToArgb(), "PlotColor": ink, "PlotWeight": PLOT_WEIGHT, "SectionStyle": None},
            {"Color": ink, "PlotColor": ink, "PlotWeight": PLOT_WEIGHT, "SectionStyle": CUT_STYLE},
        ),
        "dimension layer": (True, str(Annotation.ID)),
        "layout": ({"PageWidth": width, "PageHeight": height, "Objects": ((Annotation.NAME, (margin, margin), (rounded(width - margin), rounded(height - margin))),)},),
        "section styles": (section_facts(cut_style()),),
        "saved states": (),
        "hatch patterns": tuple(sorted(pattern.Name for pattern in new_hatches())),
        "linetypes": {name: {"Segments": lengths, "Width": pen, "WidthUnits": str(Rhino.UnitSystem.Millimeters)} for name, (lengths, pen) in units.linetypes.items()},
    }


def template_facts(path: str, members: Mapping[str, dict[str, object]]) -> dict[str, object]:
    """Every template fact read from the file and a headless document opened on it, the file holding each model view's maximized state, projection, and target and the render content, none when the file is absent."""
    if not Path(path).exists():
        return {}
    file, doc = File3dm.Read(path), Rhino.RhinoDoc.CreateHeadless(path)
    try:
        settings, anchor, viewports = doc.RenderSettings, doc.EarthAnchorPoint, tuple(view.ActiveViewport for view in doc.Views.GetViewList(ViewTypeFilter.Model))
        sun, dictionary, cameras = settings.Sun, settings.UserDictionary, tuple((view.Maximized, view.Viewport.IsPerspectiveProjection, view.Viewport.TargetPoint) for view in model_views(file, path))
        owners = {"units": doc, "grid defaults": doc.GetGridDefaults(), "dimension style": doc.DimStyles.Current, "linetype table": doc.Linetypes}
        return {
            **{group: named(owner, members[group]) for group, owner in owners.items()},
            "construction plane": shared(named(viewport.GetConstructionPlane(), members["construction plane"]) for viewport in viewports),
            "view mode": shared(str(viewport.DisplayMode.Id) for viewport in viewports),
            "camera": shared(
                {"MaximizedMatchesPerspective": maximized == perspective, "TargetPoint": (rounded(point.X), rounded(point.Y), rounded(point.Z))} for maximized, perspective, point in cameras
            ),
            "perspective lens": shared(fact(viewport.Camera35mmLensLength) for viewport in viewports if viewport.IsPerspectiveProjection),
            "dimension styles": doc.DimStyles.Count,
            "render": {
                **named(settings, ("ImageUnitSystem", "UseViewportSize", "ImageDpi")),
                "ImageSize": (settings.ImageSize.Width, settings.ImageSize.Height),
                "Dithering": settings.Dithering.Enabled,
                "GroundPlane": settings.GroundPlane.Enabled,
                "UserDictionary": {name: found(dictionary.TryGetValue(name)) for name in RENDER_KEYS},
            },
            "render mesh": str(doc.MeshingParameterStyle),
            "sun": {**named(sun, SUN), "DateTime": sun.GetDateTime(DateTimeKind.Local).ToString("yyyy-MM-ddTHH:mm")},
            "earth anchor": {**named(anchor, ANCHOR), **{name: (rounded(axis.X), rounded(axis.Y)) for name in compass() for axis in (getattr(anchor, name),)}},
            "environment": file_sky(file),
            "layers": tuple(layer_facts(doc, layer) for layer in sorted((layer for layer in doc.Layers if not layer.IsDeleted), key=lambda layer: layer.SortIndex)),
            "dimension layer": dimension_layer(doc),
            "layout": tuple(page_facts(doc, page) for page in doc.Views.GetPageViews()),
            "section styles": tuple(map(section_facts, section_styles(doc))),
            "saved states": tuple(name for _, names in saved_states(doc) for name in names),
            "hatch patterns": tuple(sorted(pattern.Name for pattern in doc.HatchPatterns if not pattern.IsDeleted)),
            "linetypes": {linetype.Name: {"Segments": tuple(map(rounded, segments(linetype))), **named(linetype, ("Width", "WidthUnits"))} for linetype in doc.Linetypes if not linetype.IsDeleted},
        }
    finally:
        file.Dispose()
        doc.Dispose()


# --- [WRITES]
def write_units(doc: Rhino.RhinoDoc, units: DocumentUnits) -> None:
    """Write the units declaration into the document's settings, model views, grid defaults, and one dimension style."""
    doc.AdjustModelUnitSystem(units.model, scale=False)
    doc.AdjustPageUnitSystem(units.page, scale=False)
    for name, value in units.document.items():
        setattr(doc, name, value)
    mode, extent = DisplayModeDescription.GetDisplayMode(DisplayModeDescription.ShadedId), GRID_THICK_EVERY * units.grid
    for view in doc.Views.GetViewList(ViewTypeFilter.Model):
        if (viewport := view.ActiveViewport).IsPerspectiveProjection:
            viewport.Camera35mmLensLength = LENS
        viewport.ZoomBoundingBox(BoundingBox(-extent, -extent, 0, extent, extent, 0))
        plane = viewport.GetConstructionPlane()
        plane.GridSpacing, plane.SnapSpacing, plane.GridLineCount, plane.ThickLineFrequency = units.grid, units.snap, units.grid_lines, GRID_THICK_EVERY
        viewport.SetConstructionPlane(plane)
        viewport.DisplayMode = mode
    defaults = doc.GetGridDefaults()
    defaults.GridSpacing, defaults.SnapSpacing, defaults.GridLineCount, defaults.GridThickFrequency = units.grid, units.snap, units.grid_lines, GRID_THICK_EVERY
    doc.SetGridDefaults(defaults)
    style = doc.DimStyles.Current
    for extra in [each for each in doc.DimStyles if each.Id != style.Id]:
        doc.DimStyles.Delete(extra.Index, quiet=True)
    style.CopyFrom(next(each for each in doc.DimStyles.BuiltInStyles if each.Name == units.style))
    style.Name = units.style
    for name, value in units.dimension.items():
        setattr(style, name, value)
    doc.DimStyles.Modify(style, style.Id, quiet=True)


def write_render(doc: Rhino.RhinoDoc, page: Rhino.UnitSystem) -> None:
    """Write the render, sun, sky, and earth anchor declarations into the document."""
    doc.MeshingParameterStyle = MeshingParameterStyle.Quality
    settings = doc.RenderSettings
    settings.ImageUnitSystem = page
    settings.UseViewportSize, settings.ImageSize, settings.ImageDpi = False, Size(*FRAME_SIZE), DPI
    settings.Dithering.Enabled, settings.GroundPlane.Enabled = True, False
    for name, value in RENDER_KEYS.items():
        settings.UserDictionary.Set(name, val=value)
    sun = settings.Sun
    for name, value in SUN.items():
        setattr(sun, name, value)
    sun.SetDateTime(DateTime(MOMENT.year, MOMENT.month, MOMENT.day, MOMENT.hour, MOMENT.minute, 0), DateTimeKind.Local)
    for held in tuple(doc.RenderEnvironments):
        doc.RenderEnvironments.Remove(held)
    environment = RenderContentType.NewContentFromTypeId(ContentUuids.BasicEnvironmentType, doc)
    environment.Name = "Sky"
    texture = RenderContentType.NewContentFromTypeId(ContentUuids.PhysicalSkyTextureType, doc)
    texture.BeginChange(RenderContent.ChangeContexts.Program)
    texture.SetParameter(SKY_SUN, value=SKY[SKY_SUN])
    texture.SetParameter(SKY_MULTIPLIER, value=SKY[SKY_MULTIPLIER])
    texture.EndChange()
    environment.SetChild(texture, SKY_SLOT)
    doc.RenderEnvironments.Add(environment)
    settings.BackgroundStyle = BackgroundStyle.Environment
    for usage in SKY_USAGES:
        settings.SetRenderEnvironmentId(usage, environment.Id)
    doc.RenderSettings = settings
    anchor = doc.EarthAnchorPoint
    for name, value in ANCHOR.items():
        setattr(anchor, name, value)
    for name, (x, y) in compass().items():
        setattr(anchor, name, Vector3d(x, y, 0))
    doc.EarthAnchorPoint = anchor


def write_tables(doc: Rhino.RhinoDoc, linetypes: Mapping[str, tuple[tuple[float, ...], float]]) -> None:
    """Write the hatch, section style, layer, linetype, and saved-state tables, dimensions routed to the annotation layer."""
    hatches = {pattern.Name: pattern for pattern in new_hatches()}
    for pattern in [pattern for pattern in doc.HatchPatterns if pattern.Name not in hatches]:
        doc.HatchPatterns.Delete(pattern.Index, quiet=True)
    held = {pattern.Name for pattern in doc.HatchPatterns if not pattern.IsDeleted}
    for name in [name for name in hatches if name not in held]:
        doc.HatchPatterns.Add(hatches[name])
    style = cut_style()
    held_index = doc.SectionStyles.Find(CUT_STYLE)
    index = doc.SectionStyles.Add(style) if held_index < 0 else held_index
    doc.SectionStyles.Modify(style, index, quiet=True)
    current = doc.Layers.CurrentLayerIndex
    for layer in [layer for layer in doc.Layers if not layer.IsDeleted and layer.Index != current and layer.Id != ANNOTATION_ID]:
        doc.Layers.Delete(layer.Index, quiet=True)
    if doc.Layers.FindId(ANNOTATION_ID) is None:
        added = Layer()
        added.Id, added.Name = ANNOTATION_ID, Annotation.NAME
        doc.Layers.Add(added)
    annotation, layer = doc.Layers.FindId(ANNOTATION_ID), doc.Layers.CurrentLayer
    annotation.Name, annotation.Color, annotation.PlotColor, annotation.PlotWeight = Annotation.NAME, color(Annotation.TAG.value), color(Ink.DOCUMENT), PLOT_WEIGHT
    annotation.LinetypeIndex, annotation.SectionStyleIndex, annotation.IsVisible, annotation.IsLocked = -1, -1, True, False
    doc.Layers.Modify(annotation, annotation.Index, quiet=True)
    layer.Color = layer.PlotColor = color(Ink.DOCUMENT)
    layer.PlotWeight, layer.SectionStyleIndex = PLOT_WEIGHT, index
    doc.Layers.Modify(layer, current, quiet=True)
    doc.Layers.Sort([annotation.Index, current])
    route_annotation(doc)
    for other in [other for other in section_styles(doc) if other.Name != CUT_STYLE]:
        doc.SectionStyles.Delete(other.Index, quiet=True)
    doc.Linetypes.LoadDefaultLinetypes()
    doc.Linetypes.LinetypeScale = LINETYPE_SCALE
    for name in [name for name in linetypes if doc.Linetypes.Find(name) < 0]:
        doc.Linetypes.Add(name, Array[float](linetypes[name][0]))
    for name, (lengths, width) in linetypes.items():
        linetype = doc.Linetypes.FindName(name)
        linetype.SetSegments(Array[float](lengths))
        linetype.Width, linetype.WidthUnits = width, Rhino.UnitSystem.Millimeters
        doc.Linetypes.Modify(linetype, linetype.Index, quiet=True)
    for linetype in [linetype for linetype in doc.Linetypes if not linetype.IsDeleted and linetype.Name not in linetypes]:
        doc.Linetypes.Delete(linetype.Index, quiet=True)
    for table, names in saved_states(doc):
        for name in names:
            table.Delete(name)


def write_layout(doc: Rhino.RhinoDoc, units: DocumentUnits) -> None:
    """Replace the layout pages with one at the sheet size, its border at the margin on the annotation layer."""
    for page in doc.Views.GetPageViews():
        page.Close()
    (width, height), margin = units.paper, units.margin
    page = doc.Views.AddPageView(None, width, height)
    attributes = ObjectAttributes()
    attributes.LayerIndex, attributes.Space, attributes.ViewportId = doc.Layers.FindId(ANNOTATION_ID).Index, ActiveSpace.PageSpace, page.MainViewport.Id
    doc.Objects.AddRectangle(Rectangle3d(Plane.WorldXY, Point3d(margin, margin, 0), Point3d(width - margin, height - margin, 0)), attributes)


def write_template(path: str, units: DocumentUnits) -> None:
    """Rewrite the template through a headless document opened on it or on the default template, Perspective alone maximized in the file."""
    doc = Rhino.RhinoDoc.CreateHeadless(path if Path(path).exists() else Settings.FileSettings.TemplateFile)
    try:
        write_units(doc, units)
        write_render(doc, units.page)
        write_tables(doc, units.linetypes)
        write_layout(doc, units)
        doc.WriteFile(path, FileWriteOptions())
    finally:
        doc.Dispose()
    file = File3dm.Read(path)
    try:
        for view in model_views(file, path):
            view.Maximized = view.Viewport.IsPerspectiveProjection
        file.Write(path, File3dmWriteOptions())
    finally:
        file.Dispose()


# --- [COMPOSITION] ----------------------------------------------------------------------


def rows() -> tuple[Row, ...]:
    """Rows of the imperial default template and the metric one beside it."""
    default = Settings.FileSettings.TemplateFile

    def row(path: str, units: DocumentUnits) -> Row:
        """Row of the template at the path written from the units declaration."""
        members = member_target(units)
        return Row(label=f"template {Path(path).stem}", read=partial(template_facts, path, members), write=lambda _: write_template(path, units), target=template_target(units, members))

    paths = ((default, Units.IMPERIAL), (str(Path(default).with_name(f"{Units.METRIC.name.title()}.3dm")), Units.METRIC))
    return tuple(row(path, document_units(units)) for path, units in paths)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows"]
