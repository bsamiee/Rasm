# ty: ignore[unresolved-import, unresolved-attribute, invalid-argument-type, invalid-assignment, not-subscriptable, unsupported-operator, no-matching-overload, too-many-positional-arguments]
# mypy: disable-error-code="import-not-found, import-untyped, no-any-unimported, attr-defined, call-overload, call-arg, type-abstract"
# ruff: file-ignore[blind-except, exec-builtin]
# /// script
# dependencies = ["msgspec", "pillow"]
#
# [tool.ty.environment]
# extra-paths = ["."]
# ///
"""Rhino document operations and plugin loads returning a record or faults, and the `run_python` script runner."""

from collections import Counter
from collections.abc import Callable, Iterable, Iterator, Mapping, Sequence
from contextlib import contextmanager, nullcontext, redirect_stderr
from functools import cache, reduce
from io import BytesIO
import linecache
from pathlib import Path
import sys
from tempfile import TemporaryDirectory
import traceback
from types import FunctionType

from AppKit import NSDocument, NSDocumentController, NSSaveOperationType
import clr
from Foundation import NSUrl
import msgspec
from PIL import Image, ImageChops
from PIL.PngImagePlugin import PngImageFile, PngInfo
from records import collect_faults, Fault, Faults, File, LayerRecord, MaterialRecord, Properties, Record, Resolved
from Rhino import FileIO, RhinoApp, RhinoDoc, UnitSystem
from Rhino.Commands import Command, CommandEventArgs, Result
from Rhino.Display import BackgroundStyle, Color4f, DefinedViewportProjection, DisplayModeDescription, RhinoPageView, RhinoView, RhinoViewport, ViewCapture, ViewCaptureSettings
from Rhino.DocObjects import (
    ActiveSpace,
    InstanceObject,
    Layer,
    Linetype,
    ModelComponent,
    ObjectAttributes,
    ObjectColorSource,
    ObjectEnumeratorSettings,
    ObjectLinetypeSource,
    ObjectMaterialSource,
    ObjectMode,
    ObjectPlotColorSource,
    ObjectPlotWeightSource,
    ObjectSectionFillRule,
    ObjectType,
    RhinoObject,
    ViewInfo,
    ViewportInfo,
)
from Rhino.DocObjects.Tables import NamedPositionTable
from Rhino.Geometry import (
    BoundingBox,
    Curve,
    GeometryBase,
    Hatch,
    HiddenLineDrawing,
    HiddenLineDrawingParameters,
    HiddenLineDrawingSegment,
    Plane,
    Point2d,
    Point3d,
    SilhouetteType,
    SubD,
    SubDToBrepOptions,
    Transform,
    Vector3d,
)
from Rhino.PlugIns import LoadPlugInResult, PlugIn, PlugInType, WriteFileResult
from Rhino.Render import ChildSlotNames, ContentUuids, ParameterNames, RenderChannels, RenderContent, RenderContentType, RenderEnvironment, RenderMaterial, RenderSettings, RenderWindow, Utilities
from Rhino.Render.PostEffects import PostEffectType
from Rhino.Runtime import CommonObject, HostUtils
from Rhino.UI import RhinoEtoApp
import scriptcontext
from System import AppDomain, Array, DateTimeKind, Enum, Guid, Object, Type
from System.Collections.Generic import List
from System.Drawing import Bitmap, Color, ColorConverter, ColorTranslator, Size
from System.Drawing.Imaging import ImageFormat
from System.IO import FileAccess, FileMode, FileStream, MemoryStream
from System.Reflection import Assembly, AssemblyName, MethodInfo
from System.Reflection.Metadata import PEReaderExtensions
from System.Reflection.PortableExecutable import PEReader

# --- [TYPES] ----------------------------------------------------------------------------

type Point = tuple[float, float, float]
type Zoom = BoundingBox | Sequence[str]
type Placement = BoundingBox | ViewportInfo | int | None

# --- [MODELS] ---------------------------------------------------------------------------


class ObjectRecord(Record, frozen=True):
    """Object with the properties it overrides on its layer, `page` naming the layout of a page-space object."""

    id: str
    layer: str
    type: ObjectType
    min: Point
    max: Point
    name: str | None = None
    properties: Properties | None = None
    page: str | None = None


class Objects(Record, frozen=True):
    """Objects an operation matched or wrote, with its deletions and command results."""

    rows: tuple[ObjectRecord, ...]
    deleted: tuple[str, ...] = ()
    results: tuple[tuple[str, Result], ...] = ()
    output: str | None = None


class ViewRecord(Record, frozen=True):
    """View camera and display mode, a layout's `scale` its first detail's model length per page length."""

    name: str
    mode: str | None
    location: Point
    target: Point
    layout: bool = False
    scale: float | None = None


class OpenDocument(Record, frozen=True):
    """Open document with its slot's listener port, `None` without a listener."""

    serial: int
    path: str | None
    modified: bool
    active: bool
    port: int | None


class DocumentRecord(Record, frozen=True):
    """Document facts every change depends on, object counts over model space."""

    path: str | None
    modified: bool
    units: UnitSystem
    page_units: UnitSystem
    tolerance: float
    angle_tolerance: float
    active: bool
    prompt: str | None
    current_view: str | None
    current_layer: str
    style: str
    selected: tuple[str, ...]
    types: dict[ObjectType, int]
    hidden: int
    locked: int
    layers: tuple[LayerRecord, ...]
    materials: tuple[MaterialRecord, ...]
    views: tuple[ViewRecord, ...]
    named_views: tuple[str, ...]
    named_cplanes: tuple[str, ...]
    named_positions: tuple[str, ...]
    layer_states: tuple[str, ...]


class Capture(Record, frozen=True):
    """Record a capture's PNG stores under the class name, with the view and display mode it drew and its pixels changed against an earlier capture."""

    view: str
    mode: str | None
    changed: int | None = None


class AssemblyRecord(Record, frozen=True):
    """Assembly Rhino holds under a file's name with Rhino's plugin load answer, the plugin's id and English command names, and each assembly of the file's folder Rhino holds from another build."""

    name: str
    path: str
    result: LoadPlugInResult
    plugin: str | None = None
    commands: tuple[str, ...] = ()
    stale: tuple[str, ...] = ()


class SunRecord(Record, frozen=True):
    """Document sun at its site and local time with the standard offset, daylight minutes, north angle, and position, `manual` placing it by angle."""

    latitude: float
    longitude: float
    time: str
    zone: float
    daylight: int
    north: float
    intensity: float
    azimuth: float
    altitude: float
    manual: bool = False


class GroundRecord(Record, frozen=True):
    """Ground plane height with its material, `None` catching shadows alone."""

    altitude: float
    automatic: bool = False
    material: str | None = None


class EnvironmentRecord(Record, frozen=True):
    """Render environment with its background color and its texture's type and image file."""

    name: str
    color: str
    texture: str | None = None
    image: str | None = None


class RenderRecord(Record, frozen=True):
    """Rendering panel state: renderer, source view, size (`None` the viewport's), quality, background, the environment each usage renders, light, channels, and the writable flags reading `True`."""

    renderer: str
    source: str
    size: tuple[int, int] | None
    quality: str
    samples: int | None
    background: str
    lighting: dict[str, str | None]
    environments: tuple[EnvironmentRecord, ...]
    sun: SunRecord | None
    skylight: bool
    ground: GroundRecord | None
    channels: tuple[str, ...] | None
    flags: tuple[str, ...]
    dithering: str | None
    gamma: float | None
    tone: str | None
    wallpapers: dict[str, str]


class Rendering(Record, frozen=True):
    """Record a scheduled render writes after its saves, with each command's result, the files saved, and the command history."""

    results: tuple[tuple[str, str], ...]
    files: tuple[str, ...]
    output: str


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [RESOLUTION]


def _active(doc: RhinoDoc) -> Faults | None:
    """Return no fault for the active document while no command waits, else a fault naming the waiting prompt or active document serial."""
    if Command.InCommand():
        return Faults.of(Fault(Command, RhinoApp.CommandPrompt))
    active = RhinoDoc.ActiveDoc
    return None if active == doc else Faults.of(Fault(RhinoDoc, doc.RuntimeSerialNumber, () if active is None else (active.RuntimeSerialNumber,)))


def layer_index(doc: RhinoDoc, path: str) -> Resolved[int]:
    """Return layer `path`'s index, created with its parents when absent, refused when locked."""
    index: int = doc.Layers.AddPath(path)
    if index < 0:
        return Faults.of(Fault(Layer, path))
    if doc.Layers[index].IsLocked:
        return Faults.of(Fault(Layer, path, tuple(entry.FullPath for entry in doc.Layers if not entry.IsDeleted and not entry.IsLocked)))
    return index


def _setter(doc: RhinoDoc, properties: Properties) -> Resolved[Callable[[Layer | ObjectAttributes], None]]:
    """Resolve `properties` into a setter for a layer or object attributes, a default linetype the document lacks copied in alone."""
    match properties.linetype:
        case None:
            linetype = None
        case str() as name if (found := doc.Linetypes.Find(name)) >= 0 or name == doc.Linetypes[-1].Name:
            linetype = found
        case str() as name:
            headless = RhinoDoc.CreateHeadless(None)
            try:
                headless.Linetypes.LoadDefaultLinetypes()
                row = headless.Linetypes.Find(name)
                names = tuple(dict.fromkeys((doc.Linetypes[-1].Name, *(entry.Name for table in (doc.Linetypes, headless.Linetypes) for entry in table))))
                linetype = doc.Linetypes.Add(headless.Linetypes[row]) if row >= 0 else Fault(Linetype, name, names)
            finally:
                headless.Dispose()
    render = None if properties.material is None else next((entry for entry in doc.RenderMaterials if entry.Name == properties.material), None)
    color, print_color = (None if text is None else ColorTranslator.FromHtml(text) if ColorConverter().IsValid(text) else Fault(Color, text) for text in (properties.color, properties.print_color))
    match properties:
        case Properties(visible=False):
            mode = ObjectMode.Hidden
        case Properties(locked=True):
            mode = ObjectMode.Locked
        case Properties(visible=None, locked=None):
            mode = None
        case _:
            mode = ObjectMode.Normal
    if faults := collect_faults(
        linetype,
        color,
        print_color,
        None if properties.material is None or render is not None else Fault(RenderMaterial, properties.material, tuple(entry.Name for entry in doc.RenderMaterials)),
        None if properties.material is None or RhinoDoc.ActiveDoc is not None else Fault(RhinoDoc, None),
    ):
        return faults
    members: dict[type[Layer | ObjectAttributes], tuple[tuple[Callable[[object, object], None], object], ...]] = {
        Layer: (
            (Layer.Color.__set__, color),
            (Layer.LinetypeIndex.__set__, linetype),
            (Layer.PlotColor.__set__, print_color),
            (Layer.PlotWeight.__set__, properties.print_width),
            (Layer.IsVisible.__set__, properties.visible),
            (Layer.SetPersistentVisibility, properties.visible),
            (Layer.IsLocked.__set__, properties.locked),
            (Layer.SetPersistentLocking, properties.locked),
        ),
        ObjectAttributes: (
            (ObjectAttributes.ObjectColor.__set__, color),
            (ObjectAttributes.ColorSource.__set__, None if color is None else ObjectColorSource.ColorFromObject),
            (ObjectAttributes.LinetypeIndex.__set__, linetype),
            (ObjectAttributes.LinetypeSource.__set__, None if linetype is None else ObjectLinetypeSource.LinetypeFromObject),
            (ObjectAttributes.PlotColor.__set__, print_color),
            (ObjectAttributes.PlotColorSource.__set__, None if print_color is None else ObjectPlotColorSource.PlotColorFromObject),
            (ObjectAttributes.PlotWeight.__set__, properties.print_width),
            (ObjectAttributes.PlotWeightSource.__set__, None if properties.print_width is None else ObjectPlotWeightSource.PlotWeightFromObject),
            (ObjectAttributes.Mode.__set__, mode),
        ),
    }

    def apply(target: Layer | ObjectAttributes) -> None:
        for setter, value in ((setter, value) for setter, value in members[type(target)] if value is not None):
            setter(target, value)
        if render is not None:
            target.RenderMaterial = render
        for key, text in (properties.strings or {}).items():
            target.SetUserString(key, text)

    return apply


def _objects(doc: RhinoDoc, ids: Iterable[str | Guid]) -> Resolved[tuple[RhinoObject, ...]]:
    """Return objects `ids` name, or a fault per id naming no object."""
    found = [(key, doc.Objects.FindId(Guid.TryParse(key)[1])) for key in map(str, ids)]
    return collect_faults(*(Fault(RhinoObject, key) for key, rhino_object in found if rhino_object is None)) or tuple(rhino_object for _, rhino_object in found)


def _object_list(doc: RhinoDoc, *, hidden: bool = True, object_type: ObjectType = ObjectType.AnyObject, name: str | None = None, space: ActiveSpace = ActiveSpace.NONE) -> tuple[RhinoObject, ...]:
    """Return active objects and lights of `object_type` matching wildcard `name`, in `space` alone unless `NONE`."""
    settings = ObjectEnumeratorSettings()
    settings.HiddenObjects, settings.IncludeLights, settings.ObjectTypeFilter, settings.NameFilter, settings.SpaceFilter = hidden, True, object_type, name, space
    return tuple(doc.Objects.GetObjectList(settings))


# --- [READS]


def _point(point: Point3d) -> Point:
    """Return a point's coordinates."""
    return (point.X, point.Y, point.Z)


def _record(doc: RhinoDoc, rhino_object: RhinoObject) -> ObjectRecord:
    """Read one object with the properties it overrides on its layer."""
    attributes, box = rhino_object.Attributes, rhino_object.Geometry.GetBoundingBox(accurate=True)
    overrides = Properties(
        color=ColorTranslator.ToHtml(attributes.ObjectColor) if attributes.ColorSource == ObjectColorSource.ColorFromObject else None,
        linetype=doc.Linetypes[attributes.LinetypeIndex].Name if attributes.LinetypeSource == ObjectLinetypeSource.LinetypeFromObject else None,
        print_color=ColorTranslator.ToHtml(attributes.PlotColor) if attributes.PlotColorSource == ObjectPlotColorSource.PlotColorFromObject else None,
        print_width=attributes.PlotWeight if attributes.PlotWeightSource == ObjectPlotWeightSource.PlotWeightFromObject else None,
        material=rhino_object.RenderMaterial.Name if attributes.MaterialSource == ObjectMaterialSource.MaterialFromObject and rhino_object.RenderMaterial is not None else None,
        visible=False if rhino_object.IsHidden else None,
        locked=rhino_object.IsLocked or None,
        strings={key: attributes.GetUserString(key) for key in attributes.GetUserStrings().AllKeys} or None,
    )
    return ObjectRecord(
        str(rhino_object.Id),
        doc.Layers[attributes.LayerIndex].FullPath,
        rhino_object.ObjectType,
        _point(box.Min),
        _point(box.Max),
        rhino_object.Name,
        None if overrides == Properties() else overrides,
        doc.Views.Find(attributes.ViewportId).PageName if attributes.Space == ActiveSpace.PageSpace else None,
    )


def read_objects(doc: RhinoDoc, ids: Iterable[str | Guid], deleted: tuple[str, ...] = ()) -> Resolved[Objects]:
    """Read objects `ids` name, with ids an operation deleted."""
    found = _objects(doc, ids)
    return found if isinstance(found, Faults) else Objects(tuple(_record(doc, rhino_object) for rhino_object in found), deleted)


def _layer_record(doc: RhinoDoc, entry: Layer, objects: Counter[int]) -> LayerRecord:
    """Read one layer with its object count from `objects`."""
    properties = Properties(
        color=ColorTranslator.ToHtml(entry.Color),
        linetype=doc.Linetypes[entry.LinetypeIndex].Name,
        print_color=ColorTranslator.ToHtml(entry.PlotColor),
        print_width=entry.PlotWeight,
        material=None if entry.RenderMaterial is None else entry.RenderMaterial.Name,
        visible=entry.IsVisible,
        locked=entry.IsLocked,
        strings={key: entry.GetUserString(key) for key in entry.GetUserStrings().AllKeys} or None,
    )
    return LayerRecord(entry.FullPath, properties, objects[entry.Index])


@cache
def _slots() -> tuple[str, ...]:
    """Return the physically based material's child slot names."""
    return tuple(member.GetValue(None) for member in clr.GetClrType(ChildSlotNames.PhysicallyBased).GetProperties())


def _texture(content: RenderContent, slot: str) -> tuple[str, str | None] | None:
    """Return the type name and image file of the texture in `slot`, `None` for an empty slot."""
    child = content.FindChild(slot)
    return None if child is None else (child.TypeName, None if (file := child.GetParameter("filename")) is None else file.ToString(None))


def _material_record(render: RenderMaterial) -> MaterialRecord:
    """Read one physically based render material from its parameters, with the image or texture type of each slot turned on."""
    names = ParameterNames.PhysicallyBased
    textures = {slot: file or kind for slot in _slots() if render.ChildSlotOn(slot) and (held := _texture(render, slot)) is not None for kind, file in (held,)}
    return MaterialRecord(
        render.Name,
        ColorTranslator.ToHtml(render.GetParameter(names.BaseColor).ToSystemColor()),
        *(render.GetParameter(parameter).ToDouble() for parameter in (names.Roughness, names.Metallic, names.Opacity, names.OpacityIor)),
        textures or None,
    )


def _environment_record(environment: RenderEnvironment) -> EnvironmentRecord:
    """Read one render environment with its background color and the texture in its `texture` slot."""
    kind, image = _texture(environment, "texture") or (None, None)
    return EnvironmentRecord(environment.Name, ColorTranslator.ToHtml(environment.GetParameter("background-color").ToSystemColor()), kind, image)


def _view_record(view: RhinoView) -> ViewRecord:
    """Read one view, a layout page with its first detail's scale."""
    viewport, layout = view.MainViewport, isinstance(view, RhinoPageView)
    mode = viewport.DisplayMode
    return ViewRecord(
        viewport.Name,
        None if mode is None else mode.EnglishName,
        _point(viewport.CameraLocation),
        _point(viewport.CameraTarget),
        layout,
        next((1.0 / detail.DetailGeometry.PageToModelRatio for detail in (view.GetDetailViews() if layout else ())), None),
    )


# --- [FILES]


def artifacts() -> Path:
    """Return the repository's `.artifacts/rhino` folder, created when absent."""
    folder = next(parent for parent in Path(__file__).resolve().parents if (parent / ".git").exists()) / ".artifacts" / "rhino"
    folder.mkdir(parents=True, exist_ok=True)
    return folder


@cache
def _formats(member: str, kind: PlugInType) -> dict[str, MethodInfo]:
    """Map each suffix a file plugin of `kind` accepts to its typed `Rhino.FileIO` method `member`."""
    namespace, signature = clr.GetClrType(FileIO.FileStl), [clr.GetClrType(str), clr.GetClrType(RhinoDoc)]
    typed = {
        f".{method.DeclaringType.Name.removeprefix('File').lower()}": method
        for clr_type in namespace.Assembly.GetExportedTypes()
        if clr_type.Namespace == namespace.Namespace
        for method in clr_type.GetMethods()
        if method.IsStatic and method.Name == member and [parameter.ParameterType for parameter in method.GetParameters()][:2] == signature
    }
    plugins = (PlugIn.GetPlugInInfo(plugin_id) for plugin_id in PlugIn.GetInstalledPlugIns().Keys)
    groups = [{Path(pattern).suffix.lower() for group in info.FileTypeExtensions for pattern in group.split(";")} for info in plugins if info.PlugInType == kind]
    derived = {suffix: method for suffixes in groups for methods in ({typed[suffix] for suffix in suffixes if suffix in typed},) if len(methods) == 1 for method in methods for suffix in suffixes}
    return derived | typed


def _reader(path: str) -> Resolved[bool | MethodInfo]:
    """Return `True` for a `.3dm` file openNURBS reads, a typed reader for another suffix, or faults refusing the file."""
    file, readers = Path(path), _formats(FileIO.FileStl.Read.__name__, PlugInType.FileImport)
    reader = HostUtils.IsRhinoFileExtension(path) or readers.get(file.suffix.lower()) or Faults.of(Fault(PlugIn, file.suffix, (".3dm", *readers)))
    if not file.is_file():
        return Faults.of(Fault(Path, path), reader)
    if reader is not True:
        return reader
    if (model := FileIO.File3dm.Read(path)) is None:
        return Faults.of(Fault(FileIO.File3dm, path))
    model.Dispose()
    return True


def _invoke(method: MethodInfo, path: str, doc: RhinoDoc) -> bool:
    """Call a typed reader or writer with default options and no dialog or prompt (OBJ at its macOS path, glTF double-sided, DWG and DXF solids as solids) and return success."""
    *_, kind = (parameter.ParameterType for parameter in method.GetParameters())
    constructor = min(kind.GetConstructors(), key=lambda candidate: candidate.GetParameters().Length)
    write, read = FileIO.FileWriteOptions(), FileIO.FileReadOptions()
    write.SuppressAllInput, write.SuppressDialogBoxes, read.ImportMode, read.BatchMode = True, True, True, True
    quiet = {clr.GetClrType(FileIO.FileWriteOptions): write, clr.GetClrType(FileIO.FileReadOptions): read}
    options = constructor.Invoke([quiet[parameter.ParameterType] for parameter in constructor.GetParameters()])
    if isinstance(options, FileIO.FileObjWriteOptions):
        options.ActualFilePathOnMac = path
    if isinstance(options, FileIO.FileGltfWriteOptions):
        options.CullBackfaces = False
    if isinstance(options, FileIO.FileDwgWriteOptions):
        options.ExportSurfacesAs = FileIO.FileDwgWriteOptions.ExportSurfaceMode.Solids
    return method.Invoke(None, [path, doc, options]) in {True, WriteFileResult.Success}


# --- [VIEWS]


def _view(doc: RhinoDoc, name: str | None) -> Resolved[RhinoView]:
    """Return view `name`, or the active view without a name."""
    view = doc.Views.ActiveView if name is None else doc.Views.Find(name, compareCase=True)
    return Faults.of(Fault(RhinoView, name, tuple(known.MainViewport.Name for known in doc.Views))) if view is None else view


def _mode(mode: str | DisplayModeDescription | None) -> Resolved[DisplayModeDescription | None]:
    """Return the registered mode an English name finds, a description as given, or `None` to keep a view's own."""
    match mode:
        case str() if (found := DisplayModeDescription.FindByName(mode)) is not None:
            return found
        case str():
            return Faults.of(Fault(DisplayModeDescription, mode, tuple(known.EnglishName for known in DisplayModeDescription.GetDisplayModes())))
        case _:
            return mode


def _placement(doc: RhinoDoc, zoom: Zoom | None, named: str | None, view: Resolved[RhinoView] | None) -> Resolved[BoundingBox | int | None]:
    """Resolve a named view index, a zoom box, or `None` for the view as it is, `()` framing a layout `view`'s paper or every visible model-space object."""
    if named is not None:
        index = doc.NamedViews.FindByName(named)
        return Faults.of(Fault(ViewInfo, named, tuple(known.Name for known in doc.NamedViews))) if index < 0 else index
    match zoom:
        case None | BoundingBox():
            return zoom
        case () if isinstance(view, RhinoPageView):
            return BoundingBox(Point3d.Origin, Point3d(view.PageWidth, view.PageHeight, 0.0))
        case ():
            return doc.Objects.BoundingBoxVisible
        case _:
            objects = _objects(doc, zoom)
            return objects if isinstance(objects, Faults) else reduce(BoundingBox.Union, (item.Geometry.GetBoundingBox(accurate=True) for item in objects), BoundingBox.Empty)


def _set_view(viewport: RhinoViewport, placement: Placement, mode: DisplayModeDescription | None) -> RhinoViewport:
    """Zoom to a box or apply a camera or named view, then apply `mode`."""
    match placement:
        case BoundingBox():
            viewport.ZoomBoundingBox(placement)
        case ViewportInfo():
            viewport.SetViewProjection(placement, updateTargetLocation=False)
            viewport.SetCameraTarget(placement.TargetPoint, updateCameraLocation=False)
        case int():
            views, name = viewport.ParentView.Document.NamedViews, viewport.Name
            mode = mode or DisplayModeDescription.GetDisplayMode(views[placement].DisplayModeId)
            views.Restore(placement, viewport)
            viewport.Name = name
        case _:
            pass
    if mode is not None:
        viewport.DisplayMode = mode
    return viewport


def _bitmap(view: RhinoView, drawn: DisplayModeDescription | None, resolution: tuple[int, int]) -> Resolved[Bitmap]:
    """Draw `view` at `resolution` as it shows, or through `drawn`'s attributes, a pipeline-locked mode drawing on a view that shows it alone."""
    match drawn:
        case None:
            capturer = ViewCapture()
            capturer.Width, capturer.Height, capturer.ScaleScreenItems = *resolution, False
            source, bitmap = ViewCapture, capturer.CaptureToBitmap(view)
        case _ if drawn.PipelineLocked:
            return Faults.of(Fault(ViewCapture, drawn.EnglishName))
        case _:
            source, bitmap = RhinoView, view.CaptureToBitmap(Size(*resolution), drawn.DisplayAttributes)
    return Faults.of(Fault(source, view.MainViewport.Name)) if bitmap is None else bitmap


@contextmanager
def _temporary_view(doc: RhinoDoc, view: RhinoView, placement: Placement, mode: DisplayModeDescription | None) -> Iterator[RhinoViewport]:
    """Place `view` for the block with selection cleared and grid and axes off in its main and active viewports, then restore each setting."""
    viewport = view.MainViewport
    camera, plane, own = ViewportInfo(viewport), viewport.GetConstructionPlane(), viewport.DisplayMode
    overlays = [(shown, shown.ConstructionGridVisible, shown.ConstructionAxesVisible, shown.WorldAxesVisible) for shown in (viewport, view.ActiveViewport)]
    selected = [rhino_object.Id for rhino_object in doc.Objects.GetSelectedObjects(includeLights=True, includeGrips=False)]
    doc.Objects.UnselectAll()
    for shown, *_ in overlays:
        shown.ConstructionGridVisible = shown.ConstructionAxesVisible = shown.WorldAxesVisible = False
    try:
        yield _set_view(viewport, placement, mode)
    finally:
        _set_view(viewport, camera, own)
        viewport.SetConstructionPlane(plane)
        for shown, grid, axes, world in overlays:
            shown.ConstructionGridVisible, shown.ConstructionAxesVisible, shown.WorldAxesVisible = grid, axes, world
        doc.Objects.Select(List[Guid](selected))


# --- [COMPOSITION] ----------------------------------------------------------------------

# --- [READS]


def documents() -> tuple[OpenDocument, ...]:
    """Read every open document in this Rhino process, its port from the listener table of the internal `RhinoAIHost`."""
    listener = Type.GetType("Rhino.AI.RhinoAIHost, RhinoAI", throwOnError=True).GetMethod("TryGetPortFor")
    return tuple(
        OpenDocument(doc.RuntimeSerialNumber, doc.Path or None, doc.Modified, RhinoDoc.ActiveDoc == doc, arguments.GetValue(1) if listener.Invoke(None, arguments) else None)
        for doc in RhinoDoc.OpenDocuments(includeHeadless=False)
        for arguments in (Array[Object]([doc, None]),)
    )


def describe(doc: RhinoDoc) -> DocumentRecord:
    """Read the document's state and contents."""
    objects, view = _object_list(doc, space=ActiveSpace.ModelSpace), doc.Views.ActiveView
    per_layer = Counter(rhino_object.Attributes.LayerIndex for rhino_object in objects)
    return DocumentRecord(
        path=doc.Path or None,
        modified=doc.Modified,
        units=doc.ModelUnitSystem,
        page_units=doc.PageUnitSystem,
        tolerance=doc.ModelAbsoluteTolerance,
        angle_tolerance=doc.ModelAngleToleranceDegrees,
        active=RhinoDoc.ActiveDoc == doc,
        prompt=RhinoApp.CommandPrompt if Command.InCommand() else None,
        current_view=None if view is None else view.MainViewport.Name,
        current_layer=doc.Layers.CurrentLayer.FullPath,
        style=doc.DimStyles.Current.Name,
        selected=tuple(str(rhino_object.Id) for rhino_object in doc.Objects.GetSelectedObjects(includeLights=True, includeGrips=False)),
        types=dict(Counter(rhino_object.ObjectType for rhino_object in objects)),
        hidden=sum(rhino_object.IsHidden for rhino_object in objects),
        locked=sum(rhino_object.IsLocked for rhino_object in objects),
        layers=tuple(_layer_record(doc, entry, per_layer) for entry in doc.Layers if not entry.IsDeleted),
        materials=tuple(_material_record(render) for render in doc.RenderMaterials if render.TypeId == ContentUuids.PhysicallyBasedMaterialType),
        views=tuple(_view_record(known) for known in doc.Views),
        named_views=tuple(named.Name for named in doc.NamedViews),
        named_cplanes=tuple(plane.Name for plane in doc.NamedConstructionPlanes),
        named_positions=tuple(doc.NamedPositions.Names),
        layer_states=tuple(doc.NamedLayerStates.Names),
    )


def find(
    doc: RhinoDoc, *, layer_path: str | None = None, object_type: ObjectType = ObjectType.AnyObject, name: str | None = None, test: Callable[[RhinoObject], bool] | None = None, hidden: bool = False
) -> Resolved[Objects]:
    """Match objects by layer tree, type, name, and test without selecting them."""
    if (root := None if layer_path is None else doc.Layers.FindByFullPath(layer_path, notFoundReturnValue=-1)) is not None and root < 0:
        return Faults.of(Fault(Layer, layer_path, tuple(entry.FullPath for entry in doc.Layers if not entry.IsDeleted)))
    return Objects(
        tuple(
            _record(doc, rhino_object)
            for rhino_object in _object_list(doc, hidden=hidden, object_type=object_type, name=name)
            if (root is None or rhino_object.Attributes.LayerIndex == root or doc.Layers[rhino_object.Attributes.LayerIndex].IsChildOf(root)) and (test is None or test(rhino_object))
        )
    )


# --- [TABLES]


def layer(doc: RhinoDoc, path: str, properties: Properties | None = None) -> Resolved[LayerRecord]:
    """Create layer `path` with its parents and apply `properties`, a visible layer turning its parents visible."""
    properties = properties or Properties()
    match _setter(doc, properties):
        case Faults() as failed:
            return failed
        case apply if (index := doc.Layers.AddPath(path)) >= 0:
            entry = doc.Layers[index]
            apply(entry)
            if properties.visible:
                doc.Layers.ForceLayerVisible(entry.Id)
            doc.Views.Redraw()
            return _layer_record(doc, entry, Counter(rhino_object.Attributes.LayerIndex for rhino_object in _object_list(doc, space=ActiveSpace.ModelSpace)))
        case _:
            return Faults.of(Fault(Layer, path))


# --- [RENDERING]


def material(
    doc: RhinoDoc, name: str, color: str | None = None, *, roughness: float | None = None, metallic: float | None = None, opacity: float | None = None, textures: Mapping[str, str] | None = None
) -> Resolved[MaterialRecord]:
    """Add or edit physically based render material `name` with an image per child slot in `textures`, color slots display-encoded and data slots linear, a same-named material of another type replaced and each `None` setting kept."""
    textures = textures or {}
    if faults := collect_faults(
        None if RhinoDoc.ActiveDoc is not None else Fault(RhinoDoc, None),
        *(Fault(ChildSlotNames.PhysicallyBased, slot, _slots()) for slot in textures if slot not in _slots()),
        *(Fault(Path, file) for file in textures.values() if not Path(file).is_file()),
    ):
        return faults
    existing = next((render for render in doc.RenderMaterials if render.Name == name), None)
    render = existing if existing is not None and existing.TypeId == ContentUuids.PhysicallyBasedMaterialType else RenderContentType.NewContentFromTypeId(ContentUuids.PhysicallyBasedMaterialType, doc)
    names, slots, program = ParameterNames.PhysicallyBased, ChildSlotNames.PhysicallyBased, RenderContent.ChangeContexts.Program
    settings = ((names.BaseColor, None if color is None else Color4f(ColorTranslator.FromHtml(color))), (names.Roughness, roughness), (names.Metallic, metallic), (names.Opacity, opacity))
    render.BeginChange(program)
    try:
        render.Name = name
        for parameter, value in ((parameter, value) for parameter, value in settings if value is not None):
            render.SetParameter(parameter, value)
        for slot, file in textures.items():
            texture = RenderContentType.NewContentFromTypeId(ContentUuids.BitmapTextureType, doc)
            texture.BeginChange(program)
            texture.SetParameter("filename", file)
            texture.SetParameter("treat-as-linear", slot not in {slots.BaseColor, slots.Emission})
            texture.EndChange()
            render.SetChild(texture, slot)
            render.SetChildSlotOn(slot, bOn=True, cc=program)
    finally:
        render.EndChange()
    written = render is existing or (doc.RenderMaterials.Add(render) if existing is None else existing.Replace(render))
    return _material_record(render) if written else Faults.of(Fault(RenderMaterial, name))


def render_settings(doc: RhinoDoc) -> RenderRecord:
    """Read the render settings the Rendering panel shows, with the environment each usage renders and each view's wallpaper."""
    settings, sources, purpose = doc.RenderSettings, RenderSettings.RenderingSources, RenderSettings.EnvironmentPurpose.ForRendering
    sun, ground, stored, workflow = settings.Sun, settings.GroundPlane, settings.UserDictionary, settings.LinearWorkflow
    named = {sources.SpecificViewport: settings.SpecificViewport, sources.NamedView: settings.NamedView, sources.SnapShot: settings.Snapshot}
    channels = {RenderWindow.ChannelId(channel): str(channel) for channel in Enum.GetValues(clr.GetClrType(RenderWindow.StandardChannels))}
    (_, own_samples), (_, samples) = stored.TryGetBool("UseDocumentSamples"), stored.TryGetInteger("Samples")
    toned, tone = settings.PostEffects.GetSelectedPostEffect(PostEffectType(int(PostEffectType.ToneMapping) + 1))
    return RenderRecord(
        renderer=PlugIn.GetPlugInInfo(Utilities.DefaultRenderPlugInId).Name,
        source=named.get(settings.RenderSource) or doc.Views.ActiveView.MainViewport.Name,
        size=None if settings.UseViewportSize else (settings.ImageSize.Width, settings.ImageSize.Height),
        quality=str(settings.AntialiasLevel),
        samples=samples if own_samples else None,
        background=str(settings.BackgroundStyle),
        lighting={str(usage): None if (held := settings.RenderEnvironment(usage, purpose)) is None else held.Name for usage in Enum.GetValues(clr.GetClrType(RenderSettings.EnvironmentUsage))},
        environments=tuple(map(_environment_record, doc.RenderEnvironments)),
        sun=SunRecord(
            sun.Latitude,
            sun.Longitude,
            sun.GetDateTime(DateTimeKind.Local).ToString("s"),
            sun.TimeZone,
            sun.DaylightSavingMinutes if sun.DaylightSavingOn else 0,
            sun.North,
            sun.Intensity,
            sun.Azimuth,
            sun.Altitude,
            sun.ManualControlOn,
        )
        if sun.Enabled
        else None,
        skylight=settings.Skylight.Enabled,
        ground=GroundRecord(
            ground.Altitude,
            ground.AutoAltitude,
            None if ground.ShadowOnly else next((held.Name for held in doc.RenderMaterials if held.Id == ground.MaterialInstanceId), str(ground.MaterialInstanceId)),
        )
        if ground.Enabled
        else None,
        channels=None if settings.RenderChannels.Mode == RenderChannels.Modes.Automatic else tuple(channels.get(each, str(each)) for each in settings.RenderChannels.CustomList),
        flags=tuple(member.Name for member in clr.GetClrType(RenderSettings).GetProperties() if member.PropertyType == clr.GetClrType(bool) and member.CanWrite and member.GetValue(settings)),
        dithering=str(settings.Dithering.Method) if settings.Dithering.Enabled else None,
        gamma=workflow.PostProcessGamma if workflow.PostProcessGammaOn else None,
        tone=doc.PostEffects.PostEffectFromId(tone).LocalName if toned else None,
        wallpapers={view.MainViewport.Name: view.MainViewport.WallpaperFilename for view in doc.Views if view.MainViewport.WallpaperFilename},
    )


def environment(doc: RhinoDoc, name: str, image: str | None = None) -> Resolved[RenderRecord]:
    """Render with environment `name` as background, reflection, and skylight, built or rebuilt around HDR image `image` when given, the background style set to it and the skylight on."""
    if image is not None:
        if not Path(image).is_file():
            return Faults.of(Fault(Path, image))
        existing = next((held for held in doc.RenderEnvironments if held.Name == name), None)
        built = existing if existing is not None and existing.TypeId == ContentUuids.BasicEnvironmentType else RenderContentType.NewContentFromTypeId(ContentUuids.BasicEnvironmentType, doc)
        texture, program = RenderContentType.NewContentFromTypeId(ContentUuids.HDRTextureType, doc), RenderContent.ChangeContexts.Program
        texture.BeginChange(program)
        texture.SetParameter("filename", image)
        texture.EndChange()
        built.BeginChange(program)
        try:
            built.Name = name
            built.SetChild(texture, "texture")
        finally:
            built.EndChange()
        if not (built is existing or (doc.RenderEnvironments.Add(built) if existing is None else existing.Replace(built))):
            return Faults.of(Fault(RenderEnvironment, name))
    match next((held for held in doc.RenderEnvironments if held.Name == name), None):
        case None:
            return Faults.of(Fault(RenderEnvironment, name, tuple(held.Name for held in doc.RenderEnvironments)))
        case chosen:
            settings, usage = doc.RenderSettings.Duplicate(), RenderSettings.EnvironmentUsage
            settings.BackgroundStyle, settings.Skylight.Enabled = BackgroundStyle.Environment, True
            settings.SetRenderEnvironmentId(usage.Background, chosen.Id)
            for custom in (usage.Reflection, usage.Skylighting):
                settings.SetRenderEnvironmentOverride(custom, on=False)
            doc.RenderSettings = settings
            return render_settings(doc)


def render(doc: RhinoDoc, name: str, *, view: str | None = None, size: tuple[int, int] | None = None, samples: int | None = None) -> Resolved[str]:
    """Render `view` at the next idle into `.artifacts/rhino/<name>.exr` and `<name>.png`, then restore the render settings, close the render window, and write `<name>.json` holding a `Rendering` record, returning its path."""
    folder = artifacts()
    files, record = tuple(str(folder / f"{name}{suffix}") for suffix in (".exr", ".png")), folder / f"{name}.json"
    match _view(doc, view), _active(doc):
        case RhinoView() as rhino_view, None:
            held, staged = doc.RenderSettings.Duplicate(), doc.RenderSettings.Duplicate()
            staged.RenderSource, staged.SpecificViewport = RenderSettings.RenderingSources.SpecificViewport, rhino_view.MainViewport.Name
            if size is not None:
                staged.UseViewportSize, staged.ImageSize = False, Size(*size)
            if samples is not None:
                staged.UserDictionary.Set("UseDocumentSamples", val=True)
                staged.UserDictionary.Set("Samples", val=samples)
            for stale in (*map(Path, files), record):
                stale.unlink(missing_ok=True)
            results: list[tuple[str, str]] = []

            def ended(_: object, event: CommandEventArgs) -> None:
                results.append((event.CommandEnglishName, str(event.CommandResult)))

            def idle(_: object, __: object) -> None:
                RhinoApp.Idle -= idle
                history = len(RhinoApp.CommandHistoryWindowText)
                Command.EndCommand += ended
                try:
                    doc.RenderSettings = staged
                    for macro in ("_-Render", *(f'-_SaveRenderWindowAs "{file}"' for file in files), "_CloseRenderWindow"):
                        RhinoApp.RunScript(doc.RuntimeSerialNumber, macro, echo=False)
                finally:
                    Command.EndCommand -= ended
                    doc.RenderSettings = held
                    output = RhinoApp.CommandHistoryWindowText[history:]
                    record.write_bytes(msgspec.json.encode(Rendering(tuple(results), tuple(file for file in files if Path(file).is_file()), output)))

            RhinoApp.Idle += idle
            return str(record)
        case failed:
            return Faults.of(*failed)


# --- [OBJECTS]


def add(
    doc: RhinoDoc, geometry: GeometryBase | Sequence[GeometryBase], layer_path: str, properties: Properties | None = None, *, name: str | None = None, page: str | None = None
) -> Resolved[Objects]:
    """Add geometry on `layer_path` with property overrides and a name, in model space or on layout `page` in page units."""
    items = (geometry,) if isinstance(geometry, GeometryBase) else tuple(geometry)
    pages = {view.PageName: view for view in doc.Views.GetPageViews()}
    match layer_index(doc, layer_path), _setter(doc, properties or Properties()), None if page is None else pages.get(page, Faults.of(Fault(RhinoPageView, page, tuple(pages)))):
        case int() as index, FunctionType() as apply, RhinoPageView() | None as layout:
            attributes = ObjectAttributes()
            attributes.LayerIndex, attributes.Name = index, name
            if layout is not None:
                attributes.Space, attributes.ViewportId = ActiveSpace.PageSpace, layout.MainViewport.Id
            apply(attributes)
            ids = [doc.Objects.Add(item, attributes) for item in items]
            doc.Views.Redraw()
            return read_objects(doc, ids)
        case failed:
            return Faults.of(*failed)


def change(doc: RhinoDoc, ids: Sequence[str], properties: Properties | None = None, *, layer_path: str | None = None, name: str | None = None) -> Resolved[Objects]:
    """Set property overrides, layer, and name on existing objects."""
    match _objects(doc, ids), _setter(doc, properties or Properties()), None if layer_path is None else layer_index(doc, layer_path):
        case tuple() as objects, FunctionType() as apply, int() | None as index:
            for rhino_object in objects:
                attributes = rhino_object.Attributes.Duplicate()
                apply(attributes)
                if index is not None:
                    attributes.LayerIndex = index
                if name is not None:
                    attributes.Name = name
                doc.Objects.ModifyAttributes(rhino_object, attributes, quiet=True)
            doc.Views.Redraw()
            return read_objects(doc, [rhino_object.Id for rhino_object in objects])
        case failed:
            return Faults.of(*failed)


def command(doc: RhinoDoc, macro: str, layer_path: str, ids: Sequence[str] = ()) -> Resolved[Objects]:
    """Run `macro` then cancel any prompt it leaves, in the active document with `ids` preselected and `layer_path` current, refused for another document and while a command waits."""
    results: list[tuple[str, Result]] = []

    def ended(_: object, event: CommandEventArgs) -> None:
        results.append((event.CommandEnglishName, event.CommandResult))

    match _objects(doc, ids), layer_index(doc, layer_path), _active(doc):
        case tuple() as targets, int() as index, None:
            before = {rhino_object.Id for rhino_object in _object_list(doc)}
            selected = [rhino_object.Id for rhino_object in doc.Objects.GetSelectedObjects(includeLights=True, includeGrips=False)]
            mark, history, current, streams = RhinoObject.NextRuntimeSerialNumber, len(RhinoApp.CommandHistoryWindowText), doc.Layers.CurrentLayerIndex, (sys.stdout, sys.stderr)
            doc.Objects.UnselectAll()
            doc.Objects.Select(List[Guid]([rhino_object.Id for rhino_object in targets]))
            doc.Layers.SetCurrentLayerIndex(index, quiet=True)
            Command.EndCommand += ended
            try:
                RhinoApp.RunScript(doc.RuntimeSerialNumber, f"{macro} !", echo=True)
                output = RhinoApp.CommandHistoryWindowText[history:]
            finally:
                sys.stdout, sys.stderr = streams
                Command.EndCommand -= ended
                doc.Layers.SetCurrentLayerIndex(current, quiet=True)
                doc.Objects.UnselectAll()
                doc.Objects.Select(List[Guid](selected))
                doc.Views.Redraw()
            after = _object_list(doc)
            return Objects(
                tuple(_record(doc, rhino_object) for rhino_object in after if rhino_object.RuntimeSerialNumber >= mark),
                tuple(str(object_id) for object_id in before - {rhino_object.Id for rhino_object in after}),
                tuple(results),
                output,
            )
        case failed:
            return Faults.of(*failed)


def position(doc: RhinoDoc, name: str, ids: Sequence[str] = ()) -> Resolved[Objects]:
    """Save named position `name` from `ids`, or with no ids move its objects back."""
    match _objects(doc, ids):
        case Faults() as faults:
            return faults
        case ():
            done = doc.NamedPositions.Restore(name)
        case objects:
            done = doc.NamedPositions.Save(name, objects) != Guid.Empty
    doc.Views.Redraw()
    held = doc.NamedPositions.ObjectIds(name) if done else None
    return Faults.of(Fault(NamedPositionTable, name, tuple(doc.NamedPositions.Names))) if held is None else read_objects(doc, held)


# --- [FILES]


def export(doc: RhinoDoc, path: str, ids: Sequence[str] = ()) -> Resolved[File[tuple[str, ...]]]:
    """Write the document or objects `ids` through the suffix's writer to `path`, a relative one under `.artifacts/rhino`, with ids of objects the format drops."""
    file, writers = artifacts() / path, _formats(FileIO.FileStl.Write.__name__, PlugInType.FileExport)
    match (
        _objects(doc, ids) if ids else _object_list(doc, space=ActiveSpace.ModelSpace),
        HostUtils.IsRhinoFileExtension(path) or writers.get(file.suffix.lower()) or Faults.of(Fault(PlugIn, file.suffix, (".3dm", *writers))),
        Fault(RhinoDoc, path) if doc.Path and file.resolve() == Path(doc.Path).resolve() else None,
    ):
        case tuple() as chosen, True | MethodInfo() as write, None:
            dropped = {
                clr.GetClrType(FileIO.FileStp): ObjectType.Mesh | ObjectType.Annotation,
                clr.GetClrType(FileIO.FileIgs): ObjectType.Mesh | ObjectType.Annotation,
                clr.GetClrType(FileIO.FileSat): ObjectType.Mesh,
                clr.GetClrType(FileIO.FileX_T): ObjectType.Mesh | ObjectType.Curve | ObjectType.Point,
            }.get(None if write is True else write.DeclaringType, ObjectType(0))
            excluded = tuple(
                str(item.Id)
                for item in chosen
                if dropped and any(piece.ObjectType & dropped for piece in (item.Explode(explodeNestedInstances=True)[0] if isinstance(item, InstanceObject) else (item,)))
            )
            file.parent.mkdir(parents=True, exist_ok=True)
            file.unlink(missing_ok=True)
            options = FileIO.FileWriteOptions()
            options.SuppressAllInput, options.SuppressDialogBoxes, options.IncludePreviewImage = True, True, False
            whole, kept = not ids and not excluded and (write is True or doc.IsHeadless), {item.Id for item in chosen}

            @contextmanager
            def pruned() -> Iterator[RhinoDoc | None]:
                with TemporaryDirectory() as folder:
                    copy = str(Path(folder) / "copy.3dm")
                    if (headless := RhinoDoc.OpenHeadless(copy) if doc.WriteFile(copy, options) else None) is None:
                        yield None
                        return
                    try:
                        for page in headless.Views.GetPageViews():
                            page.Close()
                        for item in [item for item in _object_list(headless) if item.Id not in kept]:
                            headless.Objects.Delete(item, quiet=True, ignoreModes=True)
                        for item in [item for item in _object_list(headless) if isinstance(item, InstanceObject) and str(item.Id) in excluded]:
                            headless.Objects.AddExplodedInstancePieces(item, explodeNestedInstances=True, deleteInstance=True)
                        for item in [item for item in _object_list(headless) if item.ObjectType & dropped]:
                            headless.Objects.Delete(item, quiet=True, ignoreModes=True)
                        yield headless
                    finally:
                        headless.Dispose()

            with nullcontext(doc) if whole else pruned() as target:
                written = target is not None and (target.WriteFile(str(file), options) if write is True else _invoke(write, str(file), target))
            return File(str(file), file.stat().st_size, excluded) if written else Faults.of(Fault(PlugIn, str(file)))
        case failed:
            return Faults.of(*failed)


def convert(doc: RhinoDoc, sources: Sequence[str], suffix: str, folder: str = "") -> tuple[Resolved[File[tuple[str, ...]]], ...]:
    """Write each source as `<stem><suffix>` in `folder`, a relative one under `.artifacts/rhino`, through a headless document, unitless formats read in `doc`'s units, sources sharing a target refused."""
    targets = {source: artifacts() / folder / f"{Path(source).stem}{suffix}" for source in sources}

    def converted(source: str) -> Resolved[File[tuple[str, ...]]]:
        target = targets[source]
        shared = tuple(other for other, path in targets.items() if path == target and other != source)
        match _reader(source), Fault(Path, str(target), shared) if shared else None:
            case True | MethodInfo() as reader, None:
                if (headless := RhinoDoc.OpenHeadless(source) if reader is True else RhinoDoc.CreateHeadless(None)) is None:
                    return Faults.of(Fault(RhinoDoc, source))
                try:
                    if reader is not True:
                        headless.AdjustModelUnitSystem(doc.ModelUnitSystem, scale=False)
                    return export(headless, str(target)) if reader is True or _invoke(reader, source, headless) else Faults.of(Fault(Path, source))
                finally:
                    headless.Dispose()
            case failed:
                return Faults.of(*failed)

    return tuple(converted(source) for source in sources)


def save(doc: RhinoDoc, path: str | None = None) -> Resolved[File[None]]:
    """Save through the window's `NSDocument` to `path`, a relative one under `.artifacts/rhino`, as the document's new file, else to its current file."""
    match path or doc.Path or None, RhinoEtoApp.MainWindowForDocument(doc):
        case None, _:
            return Faults.of(Fault(Path, None))
        case _, None:
            return Faults.of(Fault(RhinoDoc, doc.Path))
        case target, window:
            file, document = (artifacts() / target).resolve(), NSDocumentController.SharedDocumentController.DocumentForWindow(window.ControlObject)
            file.parent.mkdir(parents=True, exist_ok=True)
            operation = NSSaveOperationType.Save if doc.Path and file == Path(doc.Path).resolve() else NSSaveOperationType.SaveAs
            saved, _ = document.SaveToUrl(NSUrl.FromFilename(str(file)), document.FileType, operation)
            return File(str(file), file.stat().st_size, None) if saved else Faults.of(Fault(NSDocument, target))


def close(doc: RhinoDoc) -> Faults | None:
    """Close the document's window, a titled document's edits saved and an untitled one's discarded."""
    match save(doc) if doc.Path and doc.Modified else None, RhinoEtoApp.MainWindowForDocument(doc):
        case Faults() as faults, _:
            return faults
        case _, None:
            return Faults.of(Fault(RhinoDoc, doc.Path))
        case _, window:
            doc.Modified = False
            window.Close()
            return None


def load(doc: RhinoDoc, path: str, layer_path: str) -> Resolved[Objects]:
    """Import a file under `layer_path` with its layers and its block definitions' layers as sublayers, scaled into document units, unreferenced new blocks dropped."""
    match layer_index(doc, layer_path), _reader(path):
        case int() as root, True | MethodInfo() as reader:
            mark, count, definitions = RhinoObject.NextRuntimeSerialNumber, doc.Layers.Count, doc.InstanceDefinitions.Count
            imported = doc.Import(path) if reader is True else _invoke(reader, path, doc)
            created = [rhino_object for rhino_object in _object_list(doc) if rhino_object.RuntimeSerialNumber >= mark]
            blocks = [doc.InstanceDefinitions[index] for index in range(definitions, doc.InstanceDefinitions.Count)]
            for block in (block for block in blocks if not block.IsDeleted and not block.InUse(1)):
                doc.InstanceDefinitions.Delete(block.Index, deleteReferences=False, quiet=True)
            pieces = {block.Index: list(block.GetObjects()) for block in blocks if not block.IsDeleted}
            held = {rhino_object.Attributes.LayerIndex for rhino_object in (*created, *(piece for group in pieces.values() for piece in group))}
            sublayers = {index: doc.Layers.AddPath(f"{layer_path}{ModelComponent.NamePathSeparator}{doc.Layers[index].FullPath}") for index in held}

            def remapped(rhino_object: RhinoObject) -> ObjectAttributes:
                attributes = rhino_object.Attributes.Duplicate()
                attributes.LayerIndex = sublayers[attributes.LayerIndex]
                return attributes

            for rhino_object in created:
                doc.Objects.ModifyAttributes(rhino_object, remapped(rhino_object), quiet=True)
            for index, group in pieces.items():
                doc.InstanceDefinitions.ModifyGeometry(index, [piece.Geometry for piece in group], [remapped(piece) for piece in group])
            doc.Layers.Delete([entry.Index for entry in doc.Layers if entry.Index >= count and not entry.IsChildOf(root)], quiet=True)
            doc.Views.Redraw()
            return read_objects(doc, [rhino_object.Id for rhino_object in created]) if imported else Faults.of(Fault(Path, path))
        case failed:
            return Faults.of(*failed)


# --- [DRAWINGS]


def make2d(doc: RhinoDoc, view: str, layer_path: str, ids: Sequence[str] = (), offset: tuple[float, float] = (0.0, 0.0)) -> Resolved[Objects]:
    """Project `ids` or every visible model-space object through `view` onto World XY at `offset`, cut by the clipping planes that clip the view, SubD as its Brep, visible, hidden, and cut curves on their own sublayers, a solid's cut hatched as its section style names."""
    kinds = ObjectType.Brep | ObjectType.Extrusion | ObjectType.Mesh | ObjectType.Curve | ObjectType.SubD
    visible, hidden, cut, separator = HiddenLineDrawingSegment.Visibility.Visible, HiddenLineDrawingSegment.Visibility.Hidden, SilhouetteType.SectionCut, ModelComponent.NamePathSeparator
    chosen = _objects(doc, ids) if ids else _object_list(doc, hidden=False, space=ActiveSpace.ModelSpace)
    refused = None if isinstance(chosen, Faults) or not ids else collect_faults(*(Fault(RhinoObject, str(item.Id)) for item in chosen if not item.ObjectType & (kinds | ObjectType.InstanceReference)))
    match (
        _view(doc, view),
        chosen,
        refused,
        layer(doc, f"{layer_path}{separator}{hidden}", Properties(linetype="Hidden")),
        *(layer_index(doc, f"{layer_path}{separator}{kind}") for kind in (visible, hidden, cut)),
    ):
        case RhinoView() as rhino_view, tuple() as objects, None, LayerRecord(), int() as visible_layer, int() as hidden_layer, int() as cut_layer:
            viewport, tolerance = rhino_view.MainViewport, doc.ModelAbsoluteTolerance
            clips = [
                (plane, clip)
                for clip in _object_list(doc, object_type=ObjectType.ClipPlane)
                if viewport.Id in clip.ClippingPlaneGeometry.ViewportIds()
                for surface in (clip.ClippingPlaneGeometry,)
                for plane in (
                    Plane(surface.Plane.Origin, surface.Plane.YAxis, surface.Plane.XAxis),
                    *((Plane(surface.Plane.Origin + surface.PlaneDepth * surface.Plane.Normal, surface.Plane.XAxis, surface.Plane.YAxis),) if surface.PlaneDepthEnabled else ()),
                )
            ]
            pieces = [
                (piece, attributes, placement)
                for item in objects
                for piece, attributes, placement in (
                    zip(*item.Explode(explodeNestedInstances=True), strict=True) if isinstance(item, InstanceObject) else ((item, item.Attributes, Transform.Identity),)
                )
                if piece.ObjectType & kinds
            ]
            parameters = HiddenLineDrawingParameters()
            parameters.AbsoluteTolerance, parameters.IncludeHiddenCurves, parameters.IncludeTangentEdges, parameters.OccludingSectionOption = tolerance, True, False, True
            parameters.SetViewport(viewport)
            for plane, _ in clips:
                parameters.AddClippingPlane(plane)
            dropped = collect_faults(
                *(
                    Fault(HiddenLineDrawing, str(piece.Id))
                    for index, (piece, _, placement) in enumerate(pieces)
                    if not parameters.AddGeometry(piece.Geometry.ToBrep(SubDToBrepOptions.Default) if isinstance(piece.Geometry, SubD) else piece.Geometry, placement, index, occluding_sections=True)
                )
            )
            if dropped or (drawing := HiddenLineDrawing.Compute(parameters, multipleThreads=True)) is None:
                return dropped or Faults.of(Fault(HiddenLineDrawing, view))
            drawing.RejoinCompatibleVisible()
            box, (x, y) = drawing.BoundingBox(includeHidden=True), offset
            targets = {(False, visible): visible_layer, (True, visible): cut_layer, (False, hidden): hidden_layer, (True, hidden): hidden_layer}
            flatten = Transform.Translation(Vector3d(x - box.Min.X, y - box.Min.Y, 0.0)) * Transform.PlanarProjection(Plane.WorldXY)
            skipped = {SilhouetteType.NONE, SilhouetteType.NonSilhouetteSeam, SilhouetteType.NonSilhouetteTangent}
            cuts: dict[tuple[int, int], list[Curve]] = {}
            added = []
            for segment in (segment for segment in drawing.Segments if segment.ParentCurve is not None and segment.ParentCurve.SilhouetteType not in skipped):
                if (target := targets.get((segment.ParentCurve.SilhouetteType == cut, segment.SegmentVisibility))) is None:
                    continue
                curve, attributes = segment.CurveGeometry.DuplicateCurve(), ObjectAttributes()
                curve.Transform(flatten)
                attributes.LayerIndex = target
                added.append(doc.Objects.AddCurve(curve, attributes))
                if target == cut_layer:
                    cuts.setdefault((segment.ParentCurve.ClippingPlaneIndex, int(segment.ParentCurve.SourceObject.Tag)), []).append(curve)
            for (plane_index, piece_index), curves in cuts.items():
                piece, source, _ = pieces[piece_index]
                style = source.ComputedSectionStyle(doc, clips[plane_index][1].Attributes, computeColors=True, viewport_id=Guid.Empty)
                if style is None or style.HatchIndex < 0 or not (piece.IsSolid or style.SectionFillRule == ObjectSectionFillRule.ClosedCurves):
                    continue
                attributes = ObjectAttributes()
                attributes.LayerIndex, attributes.HatchBoundaryVisible = cut_layer, False
                attributes.HatchBackgroundFillColor, attributes.HatchBackgroundFillPrintColor = style.BackgroundFillColor, style.BackgroundFillPrintColor
                loops = [loop for loop in Curve.JoinCurves(curves, tolerance) if loop.IsClosed]
                added.extend(doc.Objects.AddHatch(hatch, attributes) for hatch in Hatch.Create(loops, style.HatchIndex, style.HatchRotationRadians, style.HatchScale, tolerance))
            doc.Views.Redraw()
            return read_objects(doc, added)
        case failed:
            return Faults.of(*failed)


def sheet(
    doc: RhinoDoc,
    name: str,
    scale: tuple[float, float],
    size: tuple[float, float] | None = None,
    *,
    frame: tuple[float, float, float, float] | None = None,
    projection: DefinedViewportProjection = DefinedViewportProjection.Top,
    zoom: Zoom = (),
    clips: Sequence[str] = (),
) -> Resolved[ViewRecord]:
    """Add a locked detail at `scale` page length per model length on layout `name`, the layout added at `size` when absent, over `frame` (left, bottom, right, top) or the whole page, centered on `zoom` and cut by clipping planes `clips`."""
    pages = {page.PageName: page for page in doc.Views.GetPageViews()}
    match _placement(doc, zoom, None, None), pages.get(name), size, _objects(doc, clips):
        case BoundingBox() as box, RhinoPageView() | None as found, tuple() | None, tuple() as planes if (found is None) != (size is None):
            current, page = doc.Views.ActiveView, found if size is None else doc.Views.AddPageView(name, *size)
            left, bottom, right, top = frame or (0.0, 0.0, page.PageWidth, page.PageHeight)
            detail = page.AddDetailView(str(projection), Point2d(left, bottom), Point2d(right, top), projection)
            detail.Viewport.SetCameraTarget(box.Center, updateCameraLocation=True)
            detail.CommitViewportChanges()
            detail.DetailGeometry.IsProjectionLocked = True
            detail.DetailGeometry.SetScale(scale[1], doc.ModelUnitSystem, scale[0], doc.PageUnitSystem)
            detail.CommitChanges()
            for plane in planes:
                plane.ClippingPlaneGeometry.AddClipViewportId(detail.Viewport.Id)
                plane.CommitChanges()
            doc.Views.ActiveView = current
            return _view_record(page)
        case BoundingBox(), _, _, tuple():
            return Faults.of(Fault(RhinoPageView, name, tuple(pages)))
        case failed:
            return Faults.of(*failed)


def pdf(doc: RhinoDoc, path: str, pages: Sequence[str] = (), dpi: float = 300.0) -> Resolved[File[tuple[str, ...]]]:
    """Print layout `pages` of a windowed document, or every page in order, into one vector PDF at paper size, a relative `path` under `.artifacts/rhino`."""
    known = {page.PageName: page for page in sorted(doc.Views.GetPageViews(), key=lambda page: page.PageNumber)}
    chosen = [known[name] for name in pages if name in known] if pages else list(known.values())
    if faults := collect_faults(
        *(Fault(RhinoPageView, name, tuple(known)) for name in pages if name not in known),
        Fault(RhinoPageView, None) if not (pages or known) else None,
        Fault(RhinoDoc, None) if RhinoDoc.ActiveDoc is None else None,
        Fault(RhinoDoc, doc.Path) if doc.IsHeadless else None,
    ):
        return faults
    document = FileIO.FilePdf.Create()
    for page in chosen:
        document.AddPage(ViewCaptureSettings(page, dpi))
    target = artifacts() / path
    target.parent.mkdir(parents=True, exist_ok=True)
    document.Write(str(target))
    return File(str(target), target.stat().st_size, tuple(page.PageName for page in chosen))


# --- [VIEWS]


def capture(
    doc: RhinoDoc,
    name: str,
    *,
    zoom: Zoom | None = (),
    view: str | None = None,
    mode: str | DisplayModeDescription | None = None,
    named: str | None = None,
    size: tuple[int, int] | None = None,
    since: str | None = None,
) -> Resolved[File[Capture]]:
    """Draw `.artifacts/rhino/<name>.png` at `size` or a 2000 px long side without overlays, selection, or Grasshopper 2 preview, a capture `since` giving camera, size, view, and mode unless given, with pixels changed against it."""
    folder = artifacts()
    source = None if since is None else folder / f"{since}.png"
    try:
        earlier: Resolved[PngImageFile | None] = None if source is None else PngImageFile(source)
    except FileNotFoundError:
        earlier = Faults.of(Fault(Path, str(source)))
    if isinstance(earlier, PngImageFile):
        stored = msgspec.json.decode(earlier.text[Capture.__name__], type=Capture)
        view, mode, size = view or stored.view, mode or stored.mode, earlier.size
    match (
        rhino_view := _view(doc, view),
        _mode(mode),
        CommonObject.FromJSON(earlier.text[ViewportInfo.__name__]) if isinstance(earlier, PngImageFile) else _placement(doc, zoom, named, rhino_view),
        Fault(RhinoDoc, None) if RhinoDoc.ActiveDoc is None else None,
        earlier,
    ):
        case RhinoView() as rhino_view, DisplayModeDescription() | None as description, BoundingBox() | ViewportInfo() | int() | None as placement, None, PngImageFile() | None as previous:
            native, shown, edited = rhino_view.MainViewport.Size, rhino_view.MainViewport.DisplayMode, isinstance(mode, DisplayModeDescription)
            scale = 2000 / max(native.Width, native.Height)
            resolution = size or (round(native.Width * scale), round(native.Height * scale))

            @contextmanager
            def preview_off() -> Iterator[None]:
                if (editor := None if (owner := Type.GetType("Grasshopper2.UI.Editor, Grasshopper2")) is None else owner.GetProperty("Instance").GetValue(None)) is None:
                    yield
                    return
                display = editor.Canvas.Document.Display
                held, display.Enabled = display.Enabled, False
                try:
                    yield
                finally:
                    display.Enabled = held

            with _temporary_view(doc, rhino_view, placement, None if edited else description) as viewport, preview_off():
                drawn = description if edited else viewport.DisplayMode
                bitmap = _bitmap(rhino_view, None if drawn is None or (not edited and shown is not None and drawn.Id == shown.Id) else drawn, resolution)
                camera = ViewportInfo(viewport).ToJSON(FileIO.SerializationOptions())
            if isinstance(bitmap, Faults):
                return bitmap
            stream = MemoryStream()
            try:
                bitmap.Save(stream, ImageFormat.Png)
            finally:
                bitmap.Dispose()
            image = Image.open(BytesIO(bytes(stream.ToArray()))).convert("RGB")
            match previous:
                case None:
                    changed = None
                case PngImageFile():
                    base = previous.convert(image.mode)
                    changes = reduce(ImageChops.lighter, ImageChops.difference(base, image).split()).point(lambda step: 255 if step else 0)
                    Image.composite(Image.new(image.mode, image.size, "red"), base.point(lambda value: value // 2), changes).save(folder / f"{name}-diff.png")
                    changed = changes.histogram()[255]
            record = Capture(rhino_view.MainViewport.Name, None if drawn is None else drawn.EnglishName, changed)
            chunks = PngInfo()
            chunks.add_text(Capture.__name__, msgspec.json.encode(record).decode())
            chunks.add_text(ViewportInfo.__name__, camera)
            path = folder / f"{name}.png"
            image.save(path, pnginfo=chunks)
            return File(str(path), path.stat().st_size, record)
        case failed:
            return Faults.of(*failed)


def save_view(doc: RhinoDoc, name: str, *, zoom: Zoom = (), view: str | None = None, mode: str | None = None) -> Resolved[ViewRecord]:
    """Save or replace named view `name` from `view` at `zoom` in `mode`, every view left as it was."""
    match (rhino_view := _view(doc, view)), _mode(mode), _placement(doc, zoom, None, rhino_view):
        case RhinoView() as rhino_view, DisplayModeDescription() | None as description, BoundingBox() | None as placement:
            with _temporary_view(doc, rhino_view, placement, description) as viewport:
                index, shown = doc.NamedViews.Add(name, viewport.Id), viewport.DisplayMode
            if index < 0:
                return Faults.of(Fault(ViewInfo, name))
            saved = doc.NamedViews[index]
            return ViewRecord(saved.Name, None if shown is None else shown.EnglishName, _point(saved.Viewport.CameraLocation), _point(saved.Viewport.TargetPoint))
        case failed:
            return Faults.of(*failed)


def show(doc: RhinoDoc, *, view: str | None = None, named: str | None = None, zoom: Zoom | None = None, mode: str | None = None) -> Resolved[ViewRecord]:
    """Move the user's view to named view `named` or to `zoom`, then apply `mode`."""
    match (rhino_view := _view(doc, view)), _mode(mode), _placement(doc, zoom, named, rhino_view):
        case RhinoView() as rhino_view, DisplayModeDescription() | None as description, BoundingBox() | int() | None as placement:
            _set_view(rhino_view.MainViewport, placement, description)
            rhino_view.Redraw()
            return _view_record(rhino_view)
        case failed:
            return Faults.of(*failed)


# --- [PLUGINS]


def assembly(path: str) -> AssemblyRecord:
    """Load a file as a Rhino plugin, else as a library, and return the assembly Rhino holds under its name, a folder assembly held with another module version id stale."""
    file = Path(path).resolve()
    result, plugin_id = PlugIn.LoadPlugIn(str(file))
    name = AssemblyName.GetAssemblyName(str(file)).Name
    if not any(loaded.GetName().Name == name for loaded in AppDomain.CurrentDomain.GetAssemblies()):
        Assembly.LoadFrom(str(file))
    held = {loaded.GetName().Name: loaded for loaded in AppDomain.CurrentDomain.GetAssemblies() if not loaded.IsDynamic}

    def stale(candidates: Iterable[Path]) -> Iterator[str]:
        for candidate in candidates:
            with PEReader(FileStream(str(candidate), FileMode.Open, FileAccess.Read)) as reader:
                metadata = PEReaderExtensions.GetMetadataReader(reader)
                if not held[candidate.stem].ManifestModule.ModuleVersionId.Equals(metadata.GetGuid(metadata.GetModuleDefinition().Mvid)):
                    yield candidate.stem

    plugin = PlugIn.Find(plugin_id)
    return AssemblyRecord(
        held[name].FullName,
        held[name].Location,
        result,
        None if plugin is None else str(plugin_id),
        () if plugin is None else tuple(entry.EnglishName for entry in plugin.GetCommands()),
        tuple(sorted(stale(candidate for candidate in file.parent.iterdir() if candidate.suffix in {".dll", ".rhp"} and candidate.stem in held))),
    )


# --- [SCRIPTS]


def run(source: str, label: str, namespace: dict[str, object]) -> None:
    """Run a `run_python` script on its slot's document in one undo step named `label`, stderr and an exception's traceback on stdout."""
    doc, active = namespace["__rhino_doc__"], scriptcontext.doc
    linecache.cache["<run_python>"] = (len(source), None, source.splitlines(keepends=True), "<run_python>")
    scriptcontext.doc, record = doc, doc.BeginUndoRecord(label)
    try:
        with redirect_stderr(sys.stdout):
            exec(compile(source, "<run_python>", "exec"), namespace)
    except BaseException as error:
        traceback.print_exception(error, file=sys.stdout)
    finally:
        scriptcontext.doc = active
        if record:
            doc.EndUndoRecord(record)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "AssemblyRecord",
    "Capture",
    "DocumentRecord",
    "EnvironmentRecord",
    "GroundRecord",
    "ObjectRecord",
    "Objects",
    "OpenDocument",
    "RenderRecord",
    "Rendering",
    "SunRecord",
    "ViewRecord",
    "add",
    "artifacts",
    "assembly",
    "capture",
    "change",
    "close",
    "command",
    "convert",
    "describe",
    "documents",
    "environment",
    "export",
    "find",
    "layer",
    "layer_index",
    "load",
    "make2d",
    "material",
    "pdf",
    "position",
    "read_objects",
    "render",
    "render_settings",
    "run",
    "save",
    "save_view",
    "sheet",
    "show",
]
