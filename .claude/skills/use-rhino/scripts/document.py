# ty: ignore[unresolved-import, unresolved-attribute, invalid-argument-type, invalid-assignment, not-subscriptable, unsupported-operator, no-matching-overload]
# mypy: disable-error-code="import-not-found, import-untyped, no-any-unimported, attr-defined, call-overload"
# /// script
# dependencies = ["msgspec", "pillow"]
#
# [tool.ty.environment]
# extra-paths = ["."]
# ///
"""Rhino document operations and plugin loads, each returning a record or its faults."""

from collections import Counter
from collections.abc import Callable, Iterable, Iterator, Sequence
from contextlib import contextmanager, nullcontext
from functools import reduce
from io import BytesIO
from pathlib import Path
import sys
from tempfile import TemporaryDirectory
from types import FunctionType

from AppKit import NSDocument, NSDocumentController, NSSaveOperationType
import clr
from Foundation import NSUrl
import msgspec
from PIL import Image, ImageChops
from PIL.PngImagePlugin import PngImageFile, PngInfo
from records import collect_faults, Fault, File, LayerRecord, MaterialRecord, Properties, Record
from Rhino import FileIO, RhinoApp, RhinoDoc, UnitSystem
from Rhino.Commands import Command, CommandEventArgs, Result
from Rhino.Display import Color4f, DefinedViewportProjection, DisplayModeDescription, RhinoPageView, RhinoView, RhinoViewport, ViewCaptureSettings, ViewTypeFilter
from Rhino.DocObjects import (
    ActiveSpace,
    InstanceObject,
    Layer,
    Linetype,
    ObjectAttributes,
    ObjectColorSource,
    ObjectEnumeratorSettings,
    ObjectLinetypeSource,
    ObjectMaterialSource,
    ObjectMode,
    ObjectPlotColorSource,
    ObjectPlotWeightSource,
    ObjectType,
    RhinoObject,
    ViewInfo,
    ViewportInfo,
)
from Rhino.DocObjects.Tables import NamedPositionTable
from Rhino.Geometry import BoundingBox, GeometryBase, HiddenLineDrawing, HiddenLineDrawingParameters, HiddenLineDrawingSegment, Plane, Point2d, Point3d, SubD, SubDToBrepOptions, Transform, Vector3d
from Rhino.PlugIns import PlugIn, PlugInType, WriteFileResult
from Rhino.Render import ContentUuids, ParameterNames, RenderContent, RenderContentType, RenderMaterial
from Rhino.Runtime import CommonObject, HostUtils
from Rhino.UI import RhinoEtoApp
from System import Activator, AppDomain, Array, Guid, Object, Type
from System.Collections.Generic import List
from System.Drawing import ColorTranslator, Size
from System.Drawing.Imaging import ImageFormat
from System.IO import MemoryStream
from System.Reflection import Assembly, AssemblyName, MethodInfo

# --- [TYPES] ----------------------------------------------------------------------------

type Point = tuple[float, float, float]
type Zoom = BoundingBox | Sequence[str]
type Placement = BoundingBox | ViewportInfo | int | None

# --- [MODELS] ---------------------------------------------------------------------------


class ObjectRecord(Record, frozen=True):
    """Object with the properties it overrides on its layer."""

    id: str
    layer: str
    type: ObjectType
    min: Point
    max: Point
    name: str | None = None
    properties: Properties | None = None


class Objects(Record, frozen=True):
    """Objects an operation matched or wrote, with its deletions and command results."""

    rows: tuple[ObjectRecord, ...]
    deleted: tuple[str, ...] = ()
    results: tuple[tuple[str, Result], ...] = ()
    output: str | None = None


class ViewRecord(Record, frozen=True):
    """View camera and display mode, a layout's `scale` in model units per page unit."""

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
    """Capture settings its PNG stores, with pixels changed against an earlier capture."""

    view: str
    mode: str | None
    size: tuple[int, int]
    changed: int | None = None


class AssemblyRecord(Record, frozen=True):
    """Assembly Rhino holds, with the plugin id its assembly declares and English command names when it loaded as a plugin."""

    name: str
    path: str
    plugin: str | None = None
    commands: tuple[str, ...] = ()


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [RESOLUTION]


def _active(doc: RhinoDoc) -> tuple[Fault, ...]:
    """Return no fault for the active document while no command waits, else the waiting prompt or the active document's serial."""
    if Command.InCommand():
        return (Fault(Command, RhinoApp.CommandPrompt),)
    active = RhinoDoc.ActiveDoc
    return () if active == doc else (Fault(RhinoDoc, doc.RuntimeSerialNumber, () if active is None else (active.RuntimeSerialNumber,)),)


def layer_index(doc: RhinoDoc, path: str) -> int | tuple[Fault, ...]:
    """Return layer `path`'s index, created with its parents when absent, refused when locked."""
    index: int = doc.Layers.AddPath(path)
    if index < 0:
        return (Fault(Layer, path),)
    if doc.Layers[index].IsLocked:
        return (Fault(Layer, path, tuple(entry.FullPath for entry in doc.Layers if not entry.IsDeleted and not entry.IsLocked)),)
    return index


def _setter(doc: RhinoDoc, properties: Properties) -> Callable[[Layer | ObjectAttributes], None] | tuple[Fault, ...]:
    """Resolve `properties` into a function that sets them on a layer or on object attributes, a default linetype the document lacks added alone."""
    match properties.linetype:
        case None:
            linetype = None
        case str() as name if (found := doc.Linetypes.Find(name)) >= 0:
            linetype = found
        case str() as name:
            headless = RhinoDoc.CreateHeadless(None)
            try:
                headless.Linetypes.LoadDefaultLinetypes()
                row, names = headless.Linetypes.Find(name), tuple(dict.fromkeys(entry.Name for table in (doc.Linetypes, headless.Linetypes) for entry in table))
                linetype = doc.Linetypes.Add(headless.Linetypes[row]) if row >= 0 else Fault(Linetype, name, names)
            finally:
                headless.Dispose()
    render = None if properties.material is None else next((entry for entry in doc.RenderMaterials if entry.Name == properties.material), None)
    color, print_color = (None if text is None else ColorTranslator.FromHtml(text) for text in (properties.color, properties.print_color))
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


def _objects(doc: RhinoDoc, ids: Iterable[str | Guid]) -> list[RhinoObject] | tuple[Fault, ...]:
    """Return the objects `ids` name, refusing each id the document lacks."""
    found = [(key, doc.Objects.FindId(Guid(key))) for key in map(str, ids)]
    return tuple(Fault(RhinoObject, key) for key, rhino_object in found if rhino_object is None) or [rhino_object for _, rhino_object in found]


def _object_list(doc: RhinoDoc, *, hidden: bool = True, object_type: ObjectType = ObjectType.AnyObject, name: str | None = None, space: ActiveSpace | None = None) -> list[RhinoObject]:
    """Return active objects and lights of `object_type` matching the wildcard `name`, in `space` alone when given."""
    settings = ObjectEnumeratorSettings()
    settings.HiddenObjects, settings.IncludeLights, settings.ObjectTypeFilter, settings.NameFilter, settings.SpaceFilter = hidden, True, object_type, name, ActiveSpace(0) if space is None else space
    return list(doc.Objects.GetObjectList(settings))


# --- [READS]


def _point(point: Point3d) -> Point:
    """Return a point's coordinates."""
    return (point.X, point.Y, point.Z)


def _record(doc: RhinoDoc, rhino_object: RhinoObject) -> ObjectRecord:
    """Read one object with the properties it overrides on its layer."""
    attributes, box = rhino_object.Attributes, rhino_object.Geometry.GetBoundingBox(accurate=True)
    overrides = Properties(
        ColorTranslator.ToHtml(attributes.ObjectColor) if attributes.ColorSource == ObjectColorSource.ColorFromObject else None,
        doc.Linetypes[attributes.LinetypeIndex].Name if attributes.LinetypeSource == ObjectLinetypeSource.LinetypeFromObject else None,
        ColorTranslator.ToHtml(attributes.PlotColor) if attributes.PlotColorSource == ObjectPlotColorSource.PlotColorFromObject else None,
        attributes.PlotWeight if attributes.PlotWeightSource == ObjectPlotWeightSource.PlotWeightFromObject else None,
        rhino_object.RenderMaterial.Name if attributes.MaterialSource == ObjectMaterialSource.MaterialFromObject and rhino_object.RenderMaterial is not None else None,
        False if rhino_object.IsHidden else None,
        rhino_object.IsLocked or None,
        {key: attributes.GetUserString(key) for key in attributes.GetUserStrings().AllKeys} or None,
    )
    return ObjectRecord(
        str(rhino_object.Id), doc.Layers[attributes.LayerIndex].FullPath, rhino_object.ObjectType, _point(box.Min), _point(box.Max), rhino_object.Name, None if overrides == Properties() else overrides
    )


def read_objects(doc: RhinoDoc, ids: Iterable[str | Guid], deleted: tuple[str, ...] = ()) -> Objects | tuple[Fault, ...]:
    """Read the objects `ids` name beside the ids an operation deleted."""
    found = _objects(doc, ids)
    return found if isinstance(found, tuple) else Objects(tuple(_record(doc, rhino_object) for rhino_object in found), deleted)


def _layer_record(doc: RhinoDoc, entry: Layer, objects: Counter[int]) -> LayerRecord:
    """Read one layer with its object count from `objects`."""
    properties = Properties(
        ColorTranslator.ToHtml(entry.Color),
        doc.Linetypes[entry.LinetypeIndex].Name,
        ColorTranslator.ToHtml(entry.PlotColor),
        entry.PlotWeight,
        None if entry.RenderMaterial is None else entry.RenderMaterial.Name,
        entry.IsVisible,
        entry.IsLocked,
        {key: entry.GetUserString(key) for key in entry.GetUserStrings().AllKeys} or None,
    )
    return LayerRecord(entry.FullPath, properties, objects[entry.Index])


def _material_record(render: RenderMaterial) -> MaterialRecord:
    """Read one physically based render material from its parameters."""
    names = ParameterNames.PhysicallyBased
    return MaterialRecord(
        render.Name,
        ColorTranslator.ToHtml(render.GetParameter(names.BaseColor).ToColor4f().AsSystemColor()),
        *(render.GetParameter(parameter).ToDouble() for parameter in (names.Roughness, names.Metallic, names.Opacity, names.OpacityIor)),
    )


def _view_record(view: RhinoView) -> ViewRecord:
    """Read one view, a layout page with its first detail's scale and no display mode of its own."""
    viewport, layout = view.ActiveViewport, isinstance(view, RhinoPageView)
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


def _readable(path: str) -> tuple[Fault, ...]:
    """Refuse a `.3dm` file openNURBS cannot read."""
    if (model := FileIO.File3dm.Read(path)) is None:
        return (Fault(FileIO.File3dm, path),)
    model.Dispose()
    return ()


def _invoke(method: MethodInfo, path: str, doc: RhinoDoc) -> bool:
    """Call a typed reader or writer with default options, OBJ at its macOS path, glTF double-sided, and DWG and DXF solids as solids, reporting success."""
    *_, kind = (parameter.ParameterType for parameter in method.GetParameters())
    constructor = min(kind.GetConstructors(), key=lambda candidate: candidate.GetParameters().Length)
    options = constructor.Invoke([Activator.CreateInstance(parameter.ParameterType) for parameter in constructor.GetParameters()])
    if isinstance(options, FileIO.FileObjWriteOptions):
        options.ActualFilePathOnMac = path
    if isinstance(options, FileIO.FileGltfWriteOptions):
        options.CullBackfaces = False
    if isinstance(options, FileIO.FileDwgWriteOptions):
        options.ExportSurfacesAs = FileIO.FileDwgWriteOptions.ExportSurfaceMode.Solids
    return method.Invoke(None, [path, doc, options]) in {True, WriteFileResult.Success}


# --- [VIEWS]


def _view(doc: RhinoDoc, name: str | None) -> RhinoView | tuple[Fault, ...]:
    """Return the view `name`, or the active view."""
    view = doc.Views.ActiveView if name is None else doc.Views.Find(name, compareCase=True)
    return (Fault(RhinoView, name, tuple(known.ActiveViewport.Name for known in doc.Views)),) if view is None else view


def _mode(name: str | None) -> DisplayModeDescription | tuple[Fault, ...] | None:
    """Return the display mode `name`, or `None` to keep a view's own."""
    mode = None if name is None else DisplayModeDescription.FindByName(name)
    return (Fault(DisplayModeDescription, name, tuple(known.EnglishName for known in DisplayModeDescription.GetDisplayModes())),) if name is not None and mode is None else mode


def _placement(doc: RhinoDoc, zoom: Zoom | None, named: str | None) -> BoundingBox | int | tuple[Fault, ...] | None:
    """Resolve a named view index, a zoom box, or `None` for the view as it is."""
    if named is not None:
        index = doc.NamedViews.FindByName(named)
        return (Fault(ViewInfo, named, tuple(known.Name for known in doc.NamedViews)),) if index < 0 else index
    match zoom:
        case None | BoundingBox():
            return zoom
        case []:
            return doc.Objects.BoundingBoxVisible
        case _:
            objects = _objects(doc, zoom)
            return objects if isinstance(objects, tuple) else reduce(BoundingBox.Union, (item.Geometry.GetBoundingBox(accurate=True) for item in objects), BoundingBox.Empty)


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


@contextmanager
def _temporary_view(doc: RhinoDoc, view: RhinoView, placement: Placement, mode: DisplayModeDescription | None) -> Iterator[RhinoViewport]:
    """Place the view with the selection cleared for the block, then restore the view and selection as the user had them."""
    viewport = view.ActiveViewport
    camera, plane, own = ViewportInfo(viewport), viewport.GetConstructionPlane(), viewport.DisplayMode
    overlays = (viewport.ConstructionGridVisible, viewport.ConstructionAxesVisible, viewport.WorldAxesVisible)
    selected = [rhino_object.Id for rhino_object in doc.Objects.GetSelectedObjects(includeLights=True, includeGrips=False)]
    doc.Objects.UnselectAll()
    try:
        yield _set_view(viewport, placement, mode)
    finally:
        _set_view(viewport, camera, own)
        viewport.SetConstructionPlane(plane)
        viewport.ConstructionGridVisible, viewport.ConstructionAxesVisible, viewport.WorldAxesVisible = overlays
        doc.Objects.Select(List[Guid](selected))


# --- [COMPOSITION] ----------------------------------------------------------------------

# --- [READS]


def documents() -> tuple[OpenDocument, ...]:
    """Read every open document in this Rhino process."""
    listener = Type.GetType("Rhino.AI.RhinoAIHost, RhinoAI").GetMethod("TryGetPortFor")
    return tuple(
        OpenDocument(doc.RuntimeSerialNumber, doc.Path or None, doc.Modified, RhinoDoc.ActiveDoc == doc, port if found else None)
        for doc in RhinoDoc.OpenDocuments(includeHeadless=False)
        for arguments in (Array[Object]([doc, None]),)
        for found, (_, port) in ((listener.Invoke(None, arguments), arguments),)
    )


def describe(doc: RhinoDoc) -> DocumentRecord:
    """Read the document's state and contents, counting model-space objects."""
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
        current_view=None if view is None else view.ActiveViewport.Name,
        current_layer=doc.Layers.CurrentLayer.FullPath,
        style=doc.DimStyles.Current.Name,
        selected=tuple(str(rhino_object.Id) for rhino_object in doc.Objects.GetSelectedObjects(includeLights=True, includeGrips=False)),
        types=dict(Counter(rhino_object.ObjectType for rhino_object in objects)),
        hidden=sum(rhino_object.IsHidden for rhino_object in objects),
        locked=sum(rhino_object.IsLocked for rhino_object in objects),
        layers=tuple(_layer_record(doc, entry, per_layer) for entry in doc.Layers if not entry.IsDeleted),
        materials=tuple(_material_record(render) for render in doc.RenderMaterials if render.TypeId == ContentUuids.PhysicallyBasedMaterialType),
        views=tuple(_view_record(known) for known in doc.Views.GetViewList(ViewTypeFilter.All)),
        named_views=tuple(named.Name for named in doc.NamedViews),
        named_cplanes=tuple(plane.Name for plane in doc.NamedConstructionPlanes),
        named_positions=tuple(doc.NamedPositions.Names),
        layer_states=tuple(doc.NamedLayerStates.Names),
    )


def find(
    doc: RhinoDoc, *, layer_path: str | None = None, object_type: ObjectType = ObjectType.AnyObject, name: str | None = None, test: Callable[[RhinoObject], bool] | None = None, hidden: bool = False
) -> Objects | tuple[Fault, ...]:
    """Match objects by layer tree, type, name, and test without selecting them."""
    if (root := None if layer_path is None else doc.Layers.FindByFullPath(layer_path, notFoundReturnValue=-1)) is not None and root < 0:
        return (Fault(Layer, layer_path, tuple(entry.FullPath for entry in doc.Layers if not entry.IsDeleted)),)
    return Objects(
        tuple(
            _record(doc, rhino_object)
            for rhino_object in _object_list(doc, hidden=hidden, object_type=object_type, name=name)
            if (root is None or rhino_object.Attributes.LayerIndex == root or doc.Layers[rhino_object.Attributes.LayerIndex].IsChildOf(root)) and (test is None or test(rhino_object))
        )
    )


# --- [TABLES]


def material(doc: RhinoDoc, name: str, color: str | None = None, *, roughness: float | None = None, metallic: float | None = None, opacity: float | None = None) -> MaterialRecord | tuple[Fault, ...]:
    """Add or edit physically based render material `name`, replacing a material of another type, `None` keeping each setting."""
    if RhinoDoc.ActiveDoc is None:
        return (Fault(RhinoDoc, None),)
    existing = next((render for render in doc.RenderMaterials if render.Name == name), None)
    render = existing if existing is not None and existing.TypeId == ContentUuids.PhysicallyBasedMaterialType else RenderContentType.NewContentFromTypeId(ContentUuids.PhysicallyBasedMaterialType, doc)
    names = ParameterNames.PhysicallyBased
    settings = ((names.BaseColor, None if color is None else Color4f(ColorTranslator.FromHtml(color))), (names.Roughness, roughness), (names.Metallic, metallic), (names.Opacity, opacity))
    render.BeginChange(RenderContent.ChangeContexts.Program)
    try:
        render.Name = name
        for parameter, value in ((parameter, value) for parameter, value in settings if value is not None):
            render.SetParameter(parameter, value)
    finally:
        render.EndChange()
    written = render is existing or (doc.RenderMaterials.Add(render) if existing is None else existing.Replace(render))
    return _material_record(next(entry for entry in doc.RenderMaterials if entry.Name == name)) if written else (Fault(RenderMaterial, name),)


def layer(doc: RhinoDoc, path: str, properties: Properties | None = None) -> LayerRecord | tuple[Fault, ...]:
    """Create layer `path` with its parents and apply `properties`, a visible layer turning its parents visible."""
    properties = properties or Properties()
    match _setter(doc, properties):
        case tuple() as failed:
            return failed
        case apply if (index := doc.Layers.AddPath(path)) >= 0:
            entry = parent = doc.Layers[index]
            apply(entry)
            while properties.visible and (parent := doc.Layers.FindId(parent.ParentLayerId)) is not None:
                parent.IsVisible = True
            doc.Views.Redraw()
            return _layer_record(doc, entry, Counter(rhino_object.Attributes.LayerIndex for rhino_object in _object_list(doc, space=ActiveSpace.ModelSpace)))
        case _:
            return (Fault(Layer, path),)


# --- [OBJECTS]


def add(
    doc: RhinoDoc, geometry: GeometryBase | Sequence[GeometryBase], layer_path: str, properties: Properties | None = None, *, name: str | None = None, page: str | None = None
) -> Objects | tuple[Fault, ...]:
    """Add geometry on `layer_path` with property overrides and a name, in model space or on layout `page` in page units."""
    items = (geometry,) if isinstance(geometry, GeometryBase) else tuple(geometry)
    pages = {view.PageName: view for view in doc.Views.GetPageViews()}
    match layer_index(doc, layer_path), _setter(doc, properties or Properties()), None if page is None else pages.get(page, (Fault(RhinoPageView, page, tuple(pages)),)):
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
            return collect_faults(*failed)


def change(doc: RhinoDoc, ids: Sequence[str], properties: Properties | None = None, *, layer_path: str | None = None, name: str | None = None) -> Objects | tuple[Fault, ...]:
    """Set property overrides, layer, and name on existing objects."""
    match _objects(doc, ids), _setter(doc, properties or Properties()), None if layer_path is None else layer_index(doc, layer_path):
        case list() as objects, FunctionType() as apply, int() | None as index:
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
            return collect_faults(*failed)


def command(doc: RhinoDoc, macro: str, layer_path: str, ids: Sequence[str] = ()) -> Objects | tuple[Fault, ...]:
    """Run `macro` in the active document on preselected `ids` with `layer_path` current, canceling any prompt it leaves, refused for another document and while any command waits."""
    results: list[tuple[str, Result]] = []

    def ended(_: object, event: CommandEventArgs) -> None:
        results.append((event.CommandEnglishName, event.CommandResult))

    match _objects(doc, ids), layer_index(doc, layer_path), _active(doc):
        case list() as targets, int() as index, ():
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
            return collect_faults(*failed)


def position(doc: RhinoDoc, name: str, ids: Sequence[str] = ()) -> Objects | tuple[Fault, ...]:
    """Save named position `name` from `ids`, or with no ids move its objects back."""
    match _objects(doc, ids):
        case []:
            done = doc.NamedPositions.Restore(name)
        case list() as objects:
            doc.NamedPositions.Delete(name)
            done = doc.NamedPositions.Save(name, objects) != Guid.Empty
        case faults:
            return faults
    doc.Views.Redraw()
    held = doc.NamedPositions.ObjectIds(name) if done else None
    return (Fault(NamedPositionTable, name, tuple(doc.NamedPositions.Names)),) if held is None else read_objects(doc, held)


# --- [FILES]


def export(doc: RhinoDoc, path: str, ids: Sequence[str] = ()) -> File[tuple[str, ...]] | tuple[Fault, ...]:
    """Write the document or objects `ids` through the writer the suffix names, listing objects the format drops."""
    file, writers = Path(path), _formats(FileIO.FileStl.Write.__name__, PlugInType.FileExport)
    match (
        _objects(doc, ids) if ids else _object_list(doc, space=ActiveSpace.ModelSpace),
        HostUtils.IsRhinoFileExtension(path) or writers.get(file.suffix.lower()) or (Fault(PlugIn, file.suffix, tuple(writers)),),
        (Fault(RhinoDoc, path),) if doc.Path and file.resolve() == Path(doc.Path).resolve() else (),
    ):
        case list() as chosen, True | MethodInfo() as write, ():
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
                written = target is not None and (target.WriteFile(path, options) if write is True else _invoke(write, path, target))
            return File(path, file.stat().st_size, excluded) if written else (Fault(PlugIn, path),)
        case failed:
            return collect_faults(*failed)


def convert(doc: RhinoDoc, sources: Sequence[str], suffix: str, folder: str | None = None) -> tuple[File[tuple[str, ...]] | tuple[Fault, ...], ...]:
    """Write each source as `<stem><suffix>` beside it or in `folder` through a headless document, unitless formats read in `doc`'s units, sources sharing a target refused."""
    readers = _formats(FileIO.FileStl.Read.__name__, PlugInType.FileImport)
    targets = {source: Path(folder or file.parent) / f"{file.stem}{suffix}" for source in sources for file in (Path(source),)}

    def converted(source: str) -> File[tuple[str, ...]] | tuple[Fault, ...]:
        file, target, native = Path(source), targets[source], HostUtils.IsRhinoFileExtension(source)
        shared = tuple(other for other, path in targets.items() if path == target and other != source)
        match (
            native or readers.get(file.suffix.lower()) or (Fault(PlugIn, file.suffix, tuple(readers)),),
            () if file.exists() else (Fault(Path, source),),
            (Fault(Path, str(target), shared),) if shared else (),
            _readable(source) if native and file.exists() else (),
        ):
            case True | MethodInfo() as reader, (), (), ():
                if (headless := RhinoDoc.OpenHeadless(source) if reader is True else RhinoDoc.CreateHeadless(None)) is None:
                    return (Fault(RhinoDoc, source),)
                try:
                    if reader is not True:
                        headless.AdjustModelUnitSystem(doc.ModelUnitSystem, scale=False)
                    return export(headless, str(target)) if reader is True or _invoke(reader, source, headless) else (Fault(Path, source),)
                finally:
                    headless.Dispose()
            case failed:
                return collect_faults(*failed)

    return tuple(converted(source) for source in sources)


def save(doc: RhinoDoc, path: str | None = None) -> File[None] | tuple[Fault, ...]:
    """Save through the window's `NSDocument` to the document's file or to `path`, which becomes its file."""
    match path or doc.Path or None, RhinoEtoApp.MainWindowForDocument(doc):
        case None, _:
            return (Fault(Path, None),)
        case _, None:
            return (Fault(RhinoDoc, doc.Path),)
        case target, window:
            file, document = Path(target).resolve(), NSDocumentController.SharedDocumentController.DocumentForWindow(window.ControlObject)
            file.parent.mkdir(parents=True, exist_ok=True)
            operation = NSSaveOperationType.Save if doc.Path and file == Path(doc.Path).resolve() else NSSaveOperationType.SaveAs
            saved, _ = document.SaveToUrl(NSUrl.FromFilename(str(file)), document.FileType, operation)
            return File(str(file), file.stat().st_size, None) if saved else (Fault(NSDocument, target),)


def close(doc: RhinoDoc) -> tuple[Fault, ...]:
    """Close the document's window, saving a titled document's edits and discarding an untitled one's."""
    match save(doc) if doc.Path and doc.Modified else None, RhinoEtoApp.MainWindowForDocument(doc):
        case tuple() as faults, _:
            return faults
        case _, None:
            return (Fault(RhinoDoc, doc.Path),)
        case _, window:
            doc.Modified = False
            window.Close()
            return ()


def load(doc: RhinoDoc, path: str, layer_path: str) -> Objects | tuple[Fault, ...]:
    """Import a file under `layer_path` with its layers and its block definitions' layers as sublayers, scaled into document units, unreferenced new blocks dropped."""
    native, readers, suffix = HostUtils.IsRhinoFileExtension(path), _formats(FileIO.FileStl.Read.__name__, PlugInType.FileImport), Path(path).suffix
    match layer_index(doc, layer_path), native or readers.get(suffix.lower()) or (Fault(PlugIn, suffix, tuple(readers)),), _readable(path) if native else ():
        case int() as root, True | MethodInfo() as reader, ():
            mark, count, definitions = RhinoObject.NextRuntimeSerialNumber, doc.Layers.Count, doc.InstanceDefinitions.Count
            imported = doc.Import(path) if reader is True else _invoke(reader, path, doc)
            created = [rhino_object for rhino_object in _object_list(doc) if rhino_object.RuntimeSerialNumber >= mark]
            blocks = [doc.InstanceDefinitions[index] for index in range(definitions, doc.InstanceDefinitions.Count)]
            for block in (block for block in blocks if not block.IsDeleted and not block.InUse(1)):
                doc.InstanceDefinitions.Delete(block.Index, deleteReferences=False, quiet=True)
            pieces = {block.Index: list(block.GetObjects()) for block in blocks if not block.IsDeleted}
            held = {rhino_object.Attributes.LayerIndex for rhino_object in (*created, *(piece for group in pieces.values() for piece in group))}
            sublayers = {index: doc.Layers.AddPath(f"{layer_path}{Layer.PathSeparator}{doc.Layers[index].FullPath}") for index in held}

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
            return read_objects(doc, [rhino_object.Id for rhino_object in created]) if imported else (Fault(Path, path),)
        case failed:
            return collect_faults(*failed)


# --- [DRAWINGS]


def make2d(doc: RhinoDoc, view: str, layer_path: str, ids: Sequence[str] = (), offset: tuple[float, float] = (0.0, 0.0)) -> Objects | tuple[Fault, ...]:
    """Project `ids` or every visible object through `view` onto World XY at `offset`, SubD as its Brep, visible curves and hidden curves in the Hidden linetype on their own sublayers."""
    kinds = ObjectType.Brep | ObjectType.Extrusion | ObjectType.Mesh | ObjectType.Curve | ObjectType.SubD
    visibilities = (HiddenLineDrawingSegment.Visibility.Visible, HiddenLineDrawingSegment.Visibility.Hidden)
    chosen = _objects(doc, ids) if ids else list(doc.Objects)
    refused = () if isinstance(chosen, tuple) or not ids else tuple(Fault(RhinoObject, str(item.Id)) for item in chosen if not item.ObjectType & (kinds | ObjectType.InstanceReference))
    match (
        _view(doc, view),
        chosen,
        refused,
        layer(doc, f"{layer_path}{Layer.PathSeparator}{HiddenLineDrawingSegment.Visibility.Hidden}", Properties(linetype="Hidden")),
        tuple(layer_index(doc, f"{layer_path}{Layer.PathSeparator}{state}") for state in visibilities),
    ):
        case RhinoView() as rhino_view, list() as objects, (), LayerRecord(), (int(), int()) as indices:
            parameters = HiddenLineDrawingParameters()
            parameters.AbsoluteTolerance, parameters.IncludeHiddenCurves, parameters.IncludeTangentEdges = doc.ModelAbsoluteTolerance, True, False
            parameters.SetViewport(rhino_view.ActiveViewport)
            dropped = tuple(
                Fault(HiddenLineDrawing, str(piece.Id))
                for item in objects
                for piece, _, placement in (zip(*item.Explode(explodeNestedInstances=True), strict=True) if isinstance(item, InstanceObject) else ((item, None, Transform.Identity),))
                if piece.ObjectType & kinds
                and not parameters.AddGeometry(piece.Geometry.ToBrep(SubDToBrepOptions.Default) if isinstance(piece.Geometry, SubD) else piece.Geometry, placement, piece.Id)
            )
            if dropped or (drawing := HiddenLineDrawing.Compute(parameters, multipleThreads=True)) is None:
                return dropped or (Fault(HiddenLineDrawing, view),)
            box, layers, added, (x, y) = drawing.BoundingBox(includeHidden=True), dict(zip(visibilities, indices, strict=True)), [], offset
            flatten = Transform.Translation(Vector3d(x - box.Min.X, y - box.Min.Y, 0.0)) * Transform.PlanarProjection(Plane.WorldXY)
            for segment in (segment for segment in drawing.Segments if segment.ParentCurve is not None and segment.SegmentVisibility in layers):
                curve, attributes = segment.CurveGeometry.DuplicateCurve(), ObjectAttributes()
                curve.Transform(flatten)
                attributes.LayerIndex = layers[segment.SegmentVisibility]
                added.append(doc.Objects.AddCurve(curve, attributes))
            doc.Views.Redraw()
            return read_objects(doc, added)
        case failed:
            return collect_faults(*failed)


def sheet(
    doc: RhinoDoc, name: str, size: tuple[float, float], scale: tuple[float, float], projection: DefinedViewportProjection = DefinedViewportProjection.Top, zoom: Zoom = ()
) -> ViewRecord | tuple[Fault, ...]:
    """Add layout `name` with one locked detail at `scale` page units per model unit, centered on `zoom` ids or box, `()` every visible object."""
    pages = tuple(page.PageName for page in doc.Views.GetPageViews())
    match _placement(doc, zoom, None), (Fault(RhinoPageView, name, pages),) if name in pages else ():
        case BoundingBox() as box, ():
            (width, height), (page_length, model_length) = size, scale
            page = doc.Views.AddPageView(name, width, height)
            detail = None if page is None else page.AddDetailView(str(projection), Point2d(0.0, 0.0), Point2d(width, height), projection)
            if page is None or detail is None:
                return (Fault(RhinoPageView, name),)
            detail.Viewport.SetCameraTarget(box.Center, updateCameraLocation=True)
            detail.CommitViewportChanges()
            detail.DetailGeometry.IsProjectionLocked = True
            detail.DetailGeometry.SetScale(model_length, doc.ModelUnitSystem, page_length, doc.PageUnitSystem)
            detail.CommitChanges()
            return _view_record(page)
        case failed:
            return collect_faults(*failed)


def pdf(doc: RhinoDoc, path: str, pages: Sequence[str] = (), dpi: float = 300.0) -> File[tuple[str, ...]] | tuple[Fault, ...]:
    """Print layout `pages` of a windowed document, or every page in order, into one vector PDF at paper size, a relative `path` under `.artifacts/rhino`."""
    known = {page.PageName: page for page in sorted(doc.Views.GetPageViews(), key=lambda page: page.PageNumber)}
    chosen = [known[name] for name in pages if name in known] if pages else list(known.values())
    if faults := collect_faults(
        tuple(Fault(RhinoPageView, name, tuple(known)) for name in pages if name not in known),
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
    doc: RhinoDoc, name: str, *, zoom: Zoom | None = (), view: str | None = None, mode: str | None = None, named: str | None = None, size: tuple[int, int] | None = None, since: str | None = None
) -> File[Capture] | tuple[Fault, ...]:
    """Draw `.artifacts/rhino/<name>.png` through a view's pipeline without grid, axes, or selection, leaving every view as it was."""
    folder = artifacts()
    source = None if since is None else folder / f"{since}.png"
    earlier = PngImageFile(source) if source is not None and source.exists() else None
    stored = None if earlier is None else msgspec.json.decode(earlier.text[Capture.__name__], type=Capture)
    match (
        _view(doc, view if stored is None else stored.view),
        _mode(mode if stored is None else stored.mode),
        _placement(doc, zoom, named) if earlier is None else CommonObject.FromJSON(earlier.text[ViewportInfo.__name__]),
        (Fault(RhinoDoc, None),) if RhinoDoc.ActiveDoc is None else (),
        (Fault(Path, str(source)),) if source is not None and earlier is None else (),
    ):
        case RhinoView() as rhino_view, DisplayModeDescription() | None as description, BoundingBox() | ViewportInfo() | int() | None as placement, (), ():
            own = rhino_view.ActiveViewport.Size
            pixels = stored.size if stored else size or (own.Width, own.Height)
            frame = Size(*pixels)

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

            with _temporary_view(doc, rhino_view, placement, description) as viewport, preview_off():
                viewport.ConstructionGridVisible = viewport.ConstructionAxesVisible = viewport.WorldAxesVisible = False
                drawn = viewport.DisplayMode
                bitmap = rhino_view.CaptureToBitmap(frame) if drawn is None else rhino_view.CaptureToBitmap(frame, drawn)
                camera = ViewportInfo(viewport).ToJSON(FileIO.SerializationOptions())
            stream = MemoryStream()
            try:
                bitmap.Save(stream, ImageFormat.Png)
            finally:
                bitmap.Dispose()
            image = Image.open(BytesIO(bytes(stream.ToArray()))).convert("RGB")
            base = None if earlier is None else earlier.convert(image.mode)
            changes = None if base is None else reduce(ImageChops.lighter, ImageChops.difference(base, image).split()).point(lambda step: 255 if step else 0)
            record = Capture(rhino_view.ActiveViewport.Name, None if drawn is None else drawn.EnglishName, pixels, None if changes is None else changes.histogram()[255])
            chunks = PngInfo()
            chunks.add_text(Capture.__name__, msgspec.json.encode(record).decode())
            chunks.add_text(ViewportInfo.__name__, camera)
            path = folder / f"{name}.png"
            image.save(path, pnginfo=chunks)
            if base is not None and changes is not None:
                Image.composite(Image.new(image.mode, image.size, "red"), Image.blend(base, Image.new(image.mode, image.size, "white"), 0.7), changes).save(folder / f"{name}-diff.png")
            return File(str(path), path.stat().st_size, record)
        case failed:
            return collect_faults(*failed)


def save_view(doc: RhinoDoc, name: str, *, zoom: Zoom = (), view: str | None = None, mode: str | None = None) -> ViewRecord | tuple[Fault, ...]:
    """Save or replace named view `name` from `view` at `zoom` in `mode`, leaving every view as it was."""
    match _view(doc, view), _mode(mode), _placement(doc, zoom, None):
        case RhinoView() as rhino_view, DisplayModeDescription() | None as description, BoundingBox() | None as placement:
            with _temporary_view(doc, rhino_view, placement, description) as viewport:
                index, shown = doc.NamedViews.Add(name, viewport.Id), viewport.DisplayMode
            if index < 0:
                return (Fault(ViewInfo, name),)
            saved = doc.NamedViews[index]
            return ViewRecord(saved.Name, None if shown is None else shown.EnglishName, _point(saved.Viewport.CameraLocation), _point(saved.Viewport.TargetPoint))
        case failed:
            return collect_faults(*failed)


def show(doc: RhinoDoc, *, view: str | None = None, named: str | None = None, zoom: Zoom | None = None, mode: str | None = None) -> ViewRecord | tuple[Fault, ...]:
    """Move the user's view to named view `named` or to `zoom`, then apply `mode`."""
    match _view(doc, view), _mode(mode), _placement(doc, zoom, named):
        case RhinoView() as rhino_view, DisplayModeDescription() | None as description, BoundingBox() | int() | None as placement:
            _set_view(rhino_view.ActiveViewport, placement, description)
            rhino_view.Redraw()
            return _view_record(rhino_view)
        case failed:
            return collect_faults(*failed)


# --- [PLUGINS]


def assembly(path: str) -> AssemblyRecord:
    """Load a file as a Rhino plugin, else as a library, and return the assembly Rhino holds under its name."""
    location = str(Path(path).resolve())
    PlugIn.LoadPlugIn(location)
    name = AssemblyName.GetAssemblyName(location).Name
    held = next((loaded for loaded in AppDomain.CurrentDomain.GetAssemblies() if loaded.GetName().Name == name), None) or Assembly.LoadFrom(location)
    match PlugIn.Find(held):
        case PlugIn() as plugin:
            return AssemblyRecord(held.FullName, held.Location, None if plugin.Id == Guid.Empty else str(plugin.Id), tuple(entry.EnglishName for entry in plugin.GetCommands()))
        case _:
            return AssemblyRecord(held.FullName, held.Location)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "AssemblyRecord",
    "Capture",
    "DocumentRecord",
    "ObjectRecord",
    "Objects",
    "OpenDocument",
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
    "save",
    "save_view",
    "sheet",
    "show",
]
