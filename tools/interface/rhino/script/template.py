# ty: ignore[invalid-argument-type, invalid-exception-caught, invalid-return-type, too-many-positional-arguments, unresolved-attribute, unresolved-import]
# mypy: disable-error-code="call-arg, call-overload, import-not-found, import-untyped, misc, no-any-return, no-any-unimported, type-abstract"
"""Rows of Rhino's imperial default template, the metric one beside it, and the default template setting naming the imperial one, and the texts each template's documents show in the Layers and Layouts grids."""

from collections.abc import Iterable, Mapping
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
from Rhino.Collections import ArchivableDictionary
from Rhino.Display import BackgroundStyle, Color4f, DisplayModeDescription, RhinoPageView, ViewTypeFilter
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
from Rhino.DocObjects.Tables import NamedConstructionPlaneTable, NamedLayerStateTable, NamedPositionTable, NamedViewTable
from Rhino.FileIO import File3dm, File3dmRenderContent, File3dmWriteOptions, FileWriteOptions
from Rhino.Geometry import BoundingBox, MeshingParameterStyle, Plane, Point3d, Rectangle3d, Vector2d, Vector3d
from Rhino.Render import ContentUuids, ParameterNames, RenderChannels, RenderContent, RenderContentType, RenderSettings, RenderWindow, Sun
import System
from System import Array, DateTime, DateTimeKind, Guid
from System.Drawing import Color, Size
from System.Globalization import CultureInfo
from System.IO import FileNotFoundException

from interface.render import (
    ADAPTIVE_MIN_SAMPLES,
    CAUSTICS,
    DAYLIGHT,
    DIFFUSE_BOUNCES,
    DIRECT_CLAMP,
    DPI,
    ELEVATION,
    EXPOSURE,
    FILTER_GLOSSY,
    FRAME_SIZE,
    GLOSSY_BOUNCES,
    GROUND_ALBEDO,
    INDIRECT_CLAMP,
    LATITUDE,
    LENS,
    LONGITUDE,
    MAX_BOUNCES,
    MOMENT,
    NOISE_THRESHOLD,
    NORTH,
    OFFSET,
    Pass,
    SAMPLES,
    SUN_IRRADIANCE,
    TRANSMISSION_BOUNCES,
    TRANSPARENT_BOUNCES,
    VOLUME_BOUNCES,
)
from interface.report import Refused, Row, single
from interface.rhino.script.accessors import color, disposed, found, member, plain, preference
from interface.roles import Annotation, Ink, Surface, Typography
from interface.units import ANGLE_PRECISION, GRID_THICK_EVERY, Length, Pen, Units

# --- [CONSTANTS] ------------------------------------------------------------------------

SKY_SLOT: Final = "texture"
SUN_LIGHT_FACTOR: Final = 3.2

# --- [HOST] -----------------------------------------------------------------------------

ANNOTATION_ID: Final = Guid.Parse(str(Annotation.ID))
SKY_USAGES: Final = tuple(System.Enum.GetValues(clr.GetClrType(RenderSettings.EnvironmentUsage)))
PROCESS: Final = ctypes.CDLL(None)

# --- [MODELS] ---------------------------------------------------------------------------


class UuidStruct(ctypes.Structure):
    """openNURBS `ON_UUID_struct` a native getter returns by value."""

    _fields_ = (("data", ctypes.c_ubyte * 16),)


class Content(TypedDict):
    """Render content by its type and the parameters set on it in one program change."""

    TypeId: Guid
    Parameters: Mapping[str, object]


class Material(Content):
    """Render material content under its name."""

    Name: str


class Environment(TypedDict):
    """Reflection and skylighting overrides, the background environment's sky texture, and the environment counts."""

    Overrides: Mapping[RenderSettings.EnvironmentUsage, bool]
    Texture: Content
    UsageEnvironments: int
    FileEnvironments: int


class LayerFacts(TypedDict):
    """Colors, print width in millimeters, and section style name of a layer."""

    Color: Color
    PlotColor: Color
    PlotWeight: float
    SectionStyle: str | None


class AnnotationFacts(LayerFacts):
    """Layer facts of the annotation layer with its id and name."""

    Id: Guid
    Name: str


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
    """Template facts by group, each generic group's members named as RhinoCommon names them, a nested mapping naming a member's own members."""

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
    render: Mapping[str, object]
    render_channels: Mapping[str, object]
    render_dictionary: Mapping[str, float]
    render_mesh: MeshingParameterStyle
    sun_moment: datetime
    earth_anchor: Mapping[str, object]
    environment: Environment
    ground_material: Material
    layers: tuple[AnnotationFacts, LayerFacts]
    dimension_layer: tuple[bool, UUID]
    layout: tuple[Page, ...]
    section_styles: Mapping[str, Mapping[str, object]]
    saved_states: tuple[str, ...]
    hatch_patterns: tuple[str, ...]
    linetypes: Mapping[str, Dashes]


class Labels(TypedDict):
    """Texts a template's documents show in the Layers grid's and the Layouts grid's columns, by column name."""

    layers: Mapping[str, tuple[str, ...]]
    layouts: Mapping[str, tuple[str, ...]]


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
def bundled(units: Units) -> str:
    """Path of the bundled template the unit system's template is built on."""
    return str(Path(FileSettings.TemplateFolder) / {Units.IMPERIAL: "Large Objects - Feet, Feet & Inches.3dm", Units.METRIC: "Large Objects - Millimeters.3dm"}[units])


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


def saved_states(doc: Rhino.RhinoDoc) -> tuple[tuple[NamedViewTable | NamedConstructionPlaneTable | NamedPositionTable | NamedLayerStateTable, tuple[str, ...]], ...]:
    """Each saved-state table of the document with the names it holds."""
    return (
        (doc.NamedViews, tuple(view.Name for view in doc.NamedViews)),
        (doc.NamedConstructionPlanes, tuple(plane.Name for plane in doc.NamedConstructionPlanes)),
        (doc.NamedPositions, tuple(doc.NamedPositions.Names)),
        (doc.NamedLayerStates, tuple(doc.NamedLayerStates.Names)),
    )


# --- [TARGET]
def target(units: Units) -> Template:
    """Every template fact the unit system decides, lengths in model or page units."""
    other, cut, weight, ink = next(each for each in Units if each is not units), "Cut", Pen.THIN / Length.MILLIMETERS, color(Ink.DOCUMENT)
    page_system, model_display, page_display = unit_system(units.page), distance_display(units.length, page=False), distance_display(units.page, page=True)
    model_precision, (width, height), margin = precision(units, units.length, model_display), (side / units.page for side in units.paper), units.margin / units.page
    grid = {"GridSpacing": units.grid / units.length, "SnapSpacing": units.snap / units.length, "GridLineCount": units.grid_lines}
    gamma, zone, saving, north = 2.2, OFFSET / timedelta(hours=1), DAYLIGHT // timedelta(minutes=1), math.radians(NORTH)
    sine, cosine, exposed = math.sin(north), math.cos(north), 2**EXPOSURE
    sky_radiance, sky_radiance_per_multiplier, base = 4.5126, 0.26625, GROUND_ALBEDO ** (1 / gamma)
    hue = Sun.ColorFromAltitude(Sun.AltitudeFromValues(LATITUDE, LONGITUDE, zone, saving, DateTime(MOMENT.year, MOMENT.month, MOMENT.day), MOMENT.hour + MOMENT.minute / 60, fast=False))
    red, green, blue = ((level / 255) ** gamma for level in (hue.R, hue.G, hue.B))

    def channel(kind: Pass) -> RenderWindow.StandardChannels:
        """Render channel carrying the render pass."""
        match kind:
            case Pass.DEPTH:
                return RenderWindow.StandardChannels.DistanceFromCamera
            case Pass.NORMAL:
                return RenderWindow.StandardChannels.NormalXYZ
            case Pass.ALBEDO:
                return RenderWindow.StandardChannels.AlbedoRGB
            case Pass.MATERIAL_INDEX:
                return RenderWindow.StandardChannels.MaterialIds
            case Pass.OBJECT_INDEX:
                return RenderWindow.StandardChannels.ObjectIds

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
        "camera": {"MaximizedMatchesPerspective": True, "TargetPoint": Point3d.Origin},
        "perspective_lens": LENS,
        "dimension_styles": 1,
        "render": {
            "ImageUnitSystem": page_system,
            "UseViewportSize": False,
            "ImageSize": Size(*FRAME_SIZE),
            "ImageDpi": DPI,
            "AmbientLight": color(Surface.SHADOW),
            "UseHiddenLights": False,
            "RenderCurves": False,
            "RenderIsoparams": False,
            "RenderPoints": False,
            "RenderAnnotations": False,
            "RenderMeshEdges": False,
            "BackgroundStyle": BackgroundStyle.Environment,
            "TransparentBackground": False,
            "Skylight": {"Enabled": True},
            "LinearWorkflow": {"PreProcessColors": True, "PostProcessGamma": single(gamma), "PostProcessGammaOn": True},
            "GroundPlane": {
                "Enabled": True,
                "ShadowOnly": False,
                "AutoAltitude": False,
                "Altitude": 0.0,
                "ShowUnderside": False,
                "TextureOffsetLocked": False,
                "TextureOffset": Vector2d.Zero,
                "TextureSizeLocked": True,
                "TextureSize": Vector2d(1 / units.length, 1 / units.length),
                "TextureRotation": 0.0,
            },
            "Sun": {
                "Enabled": True,
                "ManualControlOn": False,
                "Latitude": LATITUDE,
                "Longitude": LONGITUDE,
                "TimeZone": zone,
                "DaylightSavingOn": saving > 0,
                "DaylightSavingMinutes": saving,
                "North": 90 + NORTH,
                "Intensity": SUN_IRRADIANCE * exposed / (SUN_LIGHT_FACTOR * (0.2126 * red + 0.7152 * green + 0.0722 * blue)),
            },
        },
        "render_channels": {
            "CustomList": Array[Guid](sorted((RenderWindow.ChannelId(each) for each in (RenderWindow.StandardChannels.RGBA, *map(channel, Pass))), key=str)),
            "Mode": RenderChannels.Modes.Custom,
        },
        "render_dictionary": {
            "RenderPreset": 0,
            "UseDocumentSamples": True,
            "Samples": SAMPLES,
            "UseAdaptiveSampling": True,
            "AdaptiveThreshold": NOISE_THRESHOLD,
            "AdaptiveMinSamples": ADAPTIVE_MIN_SAMPLES,
            "Seed": 128,
            "MaxBounce": MAX_BOUNCES,
            "MaxDiffuseBounce": DIFFUSE_BOUNCES,
            "MaxGlossyBounce": GLOSSY_BOUNCES,
            "MaxTransmissionBounce": TRANSMISSION_BOUNCES,
            "MaxVolumeBounce": VOLUME_BOUNCES,
            "TransparentMaxBounce": TRANSPARENT_BOUNCES,
            "SampleClampDirect": DIRECT_CLAMP,
            "SampleClampIndirect": INDIRECT_CLAMP,
            "FilterGlossy": FILTER_GLOSSY,
            "CausticsReflective": CAUSTICS,
            "CausticsRefractive": CAUSTICS,
            "UseDirectLight": True,
            "UseIndirectLight": True,
            "AoBounces": 0,
            "TextureBakeQuality": 1,
        },
        "render_mesh": MeshingParameterStyle.Quality,
        "sun_moment": MOMENT,
        "earth_anchor": {
            "EarthBasepointLatitude": LATITUDE,
            "EarthBasepointLongitude": LONGITUDE,
            "EarthBasepointElevation": ELEVATION,
            "ModelNorth": Vector3d(-sine, cosine, 0.0),
            "ModelEast": Vector3d(cosine, sine, 0.0),
        },
        "environment": {
            "Overrides": dict.fromkeys((usage for usage in SKY_USAGES if usage != RenderSettings.EnvironmentUsage.Background), False),
            "Texture": {
                "TypeId": ContentUuids.PhysicalSkyTextureType,
                "Parameters": {
                    "use-document-sun": True,
                    "light-scattering": single(2.3),
                    "particle-scattering": single(40.0),
                    "atmospheric-density": single(556.0),
                    "sun-brightness": single(110.0),
                    "sun-size": single(4.5),
                    "light-wavelengths": Vector3d(0.661, 0.582, 0.496),
                    "rdk-texture-adjust-multiplier": sky_radiance * exposed / sky_radiance_per_multiplier,
                },
            },
            "UsageEnvironments": 1,
            "FileEnvironments": 1,
        },
        "ground_material": {
            "Name": "Ground",
            "TypeId": ContentUuids.PhysicallyBasedMaterialType,
            "Parameters": {
                ParameterNames.PhysicallyBased.BaseColor: Color4f(base, base, base, 1.0),
                ParameterNames.PhysicallyBased.Roughness: 0.5,
                ParameterNames.PhysicallyBased.Metallic: 0.0,
                ParameterNames.PhysicallyBased.Specular: 0.0,
            },
        },
        "layers": (
            {"Id": ANNOTATION_ID, "Name": Annotation.NAME, "Color": color(Annotation.TAG.value), "PlotColor": ink, "PlotWeight": weight, "SectionStyle": None},
            {"Color": ink, "PlotColor": ink, "PlotWeight": weight, "SectionStyle": cut},
        ),
        "dimension_layer": (True, Annotation.ID),
        "layout": ({"PageWidth": width, "PageHeight": height, "Objects": ((Annotation.NAME, (margin, margin), (width - margin, height - margin)),)},),
        "section_styles": {
            cut: {
                "BoundaryWidthScale": 1.0,
                "BoundaryPlotWeightMillimeters": Pen.MEDIUM / Length.MILLIMETERS,
                "BackgroundFillMode": SectionBackgroundFillMode.SolidColor,
                "BackgroundFillColor": color(Surface.SECTION),
                "BackgroundFillPrintColor": color(Surface.SECTION),
                "SectionFillRule": ObjectSectionFillRule.SolidObjects,
            }
        },
        "saved_states": (),
        "hatch_patterns": tuple(sorted(pattern.Name for pattern in new_hatches())),
        "linetypes": {
            name: {"Segments": tuple(step * units.resolution / Length.MILLIMETERS for step in steps), "Width": pen / Length.MILLIMETERS, "WidthUnits": Rhino.UnitSystem.Millimeters}
            for name, (steps, pen) in linetypes.items()
        },
    }


def plot_width(weight: float) -> str:
    """Text the Layers grid shows for a print width in millimeters, the default for zero and the invariant general form rounded to three places otherwise."""
    return "Default" if weight == 0 else System.Convert.ToString(System.Math.Round(weight, 3), CultureInfo.InvariantCulture)


def labels(units: Units, template: Template) -> Labels:
    """Texts the unit system's template documents show in the Layers and Layouts grids, the current layer's name, the continuous linetype's, and each page's paper name read from a headless document on its bundled source with the template's page units."""
    with disposed(Rhino.RhinoDoc.CreateHeadless(bundled(units))) as doc:
        doc.AdjustPageUnitSystem(template["unit_systems"]["PageUnitSystem"], scale=False)
        current, continuous = doc.Layers.CurrentLayer.Name, doc.Linetypes[-1].Name
        papers = tuple(doc.Views.AddPageView(None, page["PageWidth"], page["PageHeight"]).PaperName for page in template["layout"])
    (annotation, _), widths = template["layers"], tuple(plot_width(layer["PlotWeight"]) for layer in template["layers"])
    return {
        "layers": {
            "Name": (annotation["Name"], current),
            "Material": (),
            "Linetype": (*template["linetypes"], continuous),
            "PrintWidth": widths,
            "ViewportPrintWidth": widths,
            "Section": (*template["section_styles"], "None"),
        },
        "layouts": {"PageNumber": tuple(str(number) for number in range(1, len(papers) + 1)), "PageSize": papers},
    }


# --- [READS]
def properties(owner: object, targets: Mapping[str, object]) -> dict[str, object]:
    """Owner's members the targets name by name, a member whose target is a mapping read as its own members."""
    return {name: properties(getattr(owner, name), value) if isinstance(value, Mapping) else getattr(owner, name) for name, value in targets.items()}


def uniform(values: Iterable[object]) -> object:
    """Plain value every item holds, or each distinct plain value in first-read order when they differ."""
    held = tuple(map(plain, values))
    match tuple(each for index, each in enumerate(held) if each not in held[:index]):
        case (value,):
            return value
        case several:
            return several


def content_facts(held: File3dmRenderContent, target: Content) -> dict[str, object]:
    """Type of the render content and each parameter the target names, converted to the type of the target's value."""
    return {"TypeId": held.TypeId, "Parameters": {name: System.Convert.ChangeType(held.GetParameter(name), clr.GetClrType(type(value))) for name, value in target["Parameters"].items()}}


def file_sky(file: File3dm, target: Environment) -> dict[str, object]:
    """Overrides the target names, the background environment's sky texture from the file's render content, and the environment counts."""
    settings = file.Settings.RenderSettings
    ids = {usage: settings.RenderEnvironmentId(usage, RenderSettings.EnvironmentPurpose.Standard) for usage in SKY_USAGES}
    background = ids[RenderSettings.EnvironmentUsage.Background]
    held = next((child for environment in file.RenderEnvironments if environment.Id == background for child in environment.Children if child.ChildSlotName == SKY_SLOT), None)
    return {
        "Overrides": {usage: settings.RenderEnvironmentOverride(usage) for usage in target["Overrides"]},
        "Texture": None if held is None else content_facts(held, target["Texture"]),
        "UsageEnvironments": len(set(ids.values())),
        "FileEnvironments": len(tuple(file.RenderEnvironments)),
    }


def file_ground(file: File3dm, target: Material) -> dict[str, object] | None:
    """Name, type, and the parameters the target names of the render material the file's ground plane names, None when the file holds no such material."""
    named = file.Settings.RenderSettings.GroundPlane.MaterialInstanceId
    held = next((material for material in file.RenderMaterials if material.Id == named), None)
    return None if held is None else {"Name": held.Name, **content_facts(held, target)}


def typed(dictionary: ArchivableDictionary, targets: Mapping[str, float]) -> dict[str, float | None]:
    """Each target's entry read through the typed getter of the target's type the engine reads it with, None where the entry is absent or holds another type."""

    def entry(name: str, value: float) -> float | None:
        match value:
            case bool():
                return found(dictionary.TryGetBool(name))
            case int():
                return found(dictionary.TryGetInteger(name))
            case float():
                return found(dictionary.TryGetDouble(name))

    return {name: entry(name, value) for name, value in targets.items()}


def layer_facts(doc: Rhino.RhinoDoc, layer: Layer) -> dict[str, object]:
    """Colors, plot weight, and section style name of a layer, with its id and name for the annotation layer."""
    held = {**properties(layer, dict.fromkeys(("Color", "PlotColor", "PlotWeight"))), "SectionStyle": doc.SectionStyles[layer.SectionStyleIndex].Name if layer.SectionStyleIndex >= 0 else None}
    return {**properties(layer, dict.fromkeys(("Id", "Name"))), **held} if layer.Id == ANNOTATION_ID else held


def page_facts(doc: Rhino.RhinoDoc, page: RhinoPageView) -> dict[str, object]:
    """Size of a layout page, and the layer and corners of each object drawn on it."""
    settings = ObjectEnumeratorSettings()
    settings.HiddenObjects = True
    settings.ViewportFilter = page.MainViewport
    boxes = ((doc.Layers[each.Attributes.LayerIndex].Name, each.Geometry.GetBoundingBox(accurate=True)) for each in doc.Objects.GetObjectList(settings))
    return {**properties(page, dict.fromkeys(("PageWidth", "PageHeight"))), "Objects": tuple((name, (box.Min.X, box.Min.Y), (box.Max.X, box.Max.Y)) for name, box in boxes)}


def read(path: str, template: Template) -> dict[str, object] | None:
    """Every fact the template names, read from the file and a headless document opened on it, None when the file is missing."""
    try:
        file = File3dm.Read(path)
    except FileNotFoundException:
        return None
    with disposed(file), disposed(Rhino.RhinoDoc.CreateHeadless(path)) as doc:
        settings, viewports = doc.RenderSettings, tuple(view.ActiveViewport for view in doc.Views.GetViewList(ViewTypeFilter.Model))
        cameras, moment = tuple((view.Maximized, view.Viewport.IsPerspectiveProjection, view.Viewport.TargetPoint) for view in model_views(file, path)), settings.Sun.GetDateTime(DateTimeKind.Local)
        return {
            "unit_systems": properties(doc, template["unit_systems"]),
            "units": properties(doc, template["units"]),
            "grid_defaults": properties(doc.GetGridDefaults(), template["grid_defaults"]),
            "construction_plane": uniform(properties(viewport.GetConstructionPlane(), template["construction_plane"]) for viewport in viewports),
            "dimension_style": properties(doc.DimStyles.Current, template["dimension_style"]),
            "linetype_table": properties(doc.Linetypes, template["linetype_table"]),
            "view_mode": uniform(viewport.DisplayMode.Id for viewport in viewports),
            "camera": uniform({"MaximizedMatchesPerspective": maximized == perspective, "TargetPoint": point} for maximized, perspective, point in cameras),
            "perspective_lens": uniform(viewport.Camera35mmLensLength for viewport in viewports if viewport.IsPerspectiveProjection),
            "dimension_styles": doc.DimStyles.Count,
            "render": properties(settings, template["render"]),
            "render_channels": {"CustomList": tuple(sorted(settings.RenderChannels.CustomList, key=str)), "Mode": settings.RenderChannels.Mode},
            "render_dictionary": typed(settings.UserDictionary, template["render_dictionary"]),
            "render_mesh": doc.MeshingParameterStyle,
            "sun_moment": datetime(moment.Year, moment.Month, moment.Day, moment.Hour, moment.Minute, tzinfo=template["sun_moment"].tzinfo),
            "earth_anchor": properties(doc.EarthAnchorPoint, template["earth_anchor"]),
            "environment": file_sky(file, template["environment"]),
            "ground_material": file_ground(file, template["ground_material"]),
            "layers": tuple(layer_facts(doc, layer) for layer in sorted((layer for layer in doc.Layers if not layer.IsDeleted), key=lambda layer: layer.SortIndex)),
            "dimension_layer": dimension_layer(doc),
            "layout": tuple(page_facts(doc, page) for page in doc.Views.GetPageViews()),
            "section_styles": {style.Name: properties(style, dict.fromkeys(name for facts in template["section_styles"].values() for name in facts)) for style in section_styles(doc)},
            "saved_states": tuple(name for _, names in saved_states(doc) for name in names),
            "hatch_patterns": tuple(sorted(pattern.Name for pattern in doc.HatchPatterns if not pattern.IsDeleted)),
            "linetypes": {linetype.Name: {"Segments": segments(linetype), **properties(linetype, dict.fromkeys(("Width", "WidthUnits")))} for linetype in doc.Linetypes if not linetype.IsDeleted},
        }


# --- [WRITES]
def assign(owner: object, targets: Mapping[str, object]) -> None:
    """Set each member the targets name to its value, a member whose target is a mapping set through its own members."""
    for name, value in targets.items():
        match value:
            case Mapping():
                assign(getattr(owner, name), value)
            case _:
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
    """Write render settings, ground material, sun moment, sky, and earth anchor."""
    environment_facts, ground, settings = template["environment"], template["ground_material"], doc.RenderSettings

    def content(facts: Content) -> RenderContent:
        """New render content of the type with its parameters set in one program change."""
        made = RenderContentType.NewContentFromTypeId(facts["TypeId"], doc)
        made.BeginChange(RenderContent.ChangeContexts.Program)
        for name, value in facts["Parameters"].items():
            made.SetParameter(name, value=value)
        made.EndChange()
        return made

    doc.MeshingParameterStyle = template["render_mesh"]
    assign(settings, template["render"])
    assign(settings.RenderChannels, template["render_channels"])
    for name, value in template["render_dictionary"].items():
        settings.UserDictionary.Set(name, val=value)
    material = content(ground)
    material.Name = ground["Name"]
    doc.RenderMaterials.Add(material)
    settings.GroundPlane.MaterialInstanceId = material.Id
    settings.Sun.SetDateTime(DateTime(*template["sun_moment"].timetuple()[:6]), DateTimeKind.Local)
    for held in tuple(doc.RenderEnvironments):
        doc.RenderEnvironments.Remove(held)
    environment = RenderContentType.NewContentFromTypeId(ContentUuids.BasicEnvironmentType, doc)
    environment.Name = "Sky"
    environment.SetChild(content(environment_facts["Texture"]), SKY_SLOT)
    doc.RenderEnvironments.Add(environment)
    for usage in SKY_USAGES:
        settings.SetRenderEnvironmentId(usage, environment.Id)
    for usage, on in environment_facts["Overrides"].items():
        settings.SetRenderEnvironmentOverride(usage, on)
    doc.RenderSettings = settings
    anchor = doc.EarthAnchorPoint
    assign(anchor, template["earth_anchor"])
    doc.EarthAnchorPoint = anchor


def write_tables(doc: Rhino.RhinoDoc, template: Template) -> None:
    """Write the hatch, section style, layer, linetype, and saved-state tables, dimensions routed to the annotation layer."""
    hatches, styles, (annotation_facts, current_facts) = template["hatch_patterns"], template["section_styles"], template["layers"]
    for pattern in [pattern for pattern in doc.HatchPatterns if pattern.Name not in hatches]:
        doc.HatchPatterns.Delete(pattern.Index, quiet=True)
    held = {pattern.Name for pattern in doc.HatchPatterns if not pattern.IsDeleted}
    for pattern in [pattern for pattern in HatchPattern.GetDefaultHatchPatterns() if pattern.Name in hatches and pattern.Name not in held]:
        doc.HatchPatterns.Add(pattern)
    for name, facts in styles.items():
        style = SectionStyle()
        style.Name = name
        assign(style, facts)
        if (index := doc.SectionStyles.Find(name)) < 0:
            doc.SectionStyles.Add(style)
        else:
            doc.SectionStyles.Modify(style, index, quiet=True)
    current = doc.Layers.CurrentLayerIndex
    for layer in [layer for layer in doc.Layers if not layer.IsDeleted and layer.Index != current]:
        doc.Layers.Delete(layer.Index, quiet=True)
    annotation, layer = Layer(), doc.Layers.CurrentLayer
    annotation.LinetypeIndex, annotation.IsVisible, annotation.IsLocked = -1, True, False
    for owner, facts in ((annotation, annotation_facts), (layer, current_facts)):
        assign(owner, {name: value for name, value in facts.items() if name != "SectionStyle"})
        owner.SectionStyleIndex = -1 if facts["SectionStyle"] is None else doc.SectionStyles.Find(facts["SectionStyle"])
    doc.Layers.Modify(layer, current, quiet=True)
    doc.Layers.Sort([doc.Layers.Add(annotation), current])
    route_annotation(doc, template["dimension_layer"])
    for other in [other for other in section_styles(doc) if other.Name not in styles]:
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


def write(path: str, source: str, template: Template) -> Refused | None:
    """Write the template through a headless document on the bundled source template, Perspective alone maximized in the file, else the write Rhino refused."""
    with disposed(Rhino.RhinoDoc.CreateHeadless(source)) as doc:
        write_units(doc, template)
        write_render(doc, template)
        write_tables(doc, template)
        write_layout(doc, template["layout"])
        written = doc.WriteFile(path, FileWriteOptions())
    if not written:
        return Refused(f"headless document was not written to {path}")
    with disposed(File3dm.Read(path)) as file:
        for view in model_views(file, path):
            view.Maximized = view.Viewport.IsPerspectiveProjection
        return None if file.Write(path, File3dmWriteOptions()) else Refused(f"template file was not rewritten to {path}")


# --- [COMPOSITION] ----------------------------------------------------------------------


def rows(targets: Mapping[Units, Template]) -> tuple[Row, ...]:
    """Rows of the imperial default template and the metric one in the user template folder, each built on a bundled template of its units, then the default template setting naming the imperial one."""
    folder = template_folder()
    paths = {Units.IMPERIAL: folder / "Default.3dm", Units.METRIC: folder / "Metric.3dm"}
    return (
        *(
            preference(label=f'templates["{paths[units].name}"]', read=partial(read, (path := str(paths[units])), facts), write=partial(write, path, bundled(units)), target=facts)
            for units, facts in targets.items()
        ),
        member(FileSettings, "TemplateFile", target=str(paths[Units.IMPERIAL])),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["SUN_LIGHT_FACTOR", "Labels", "Template", "labels", "rows", "target"]
