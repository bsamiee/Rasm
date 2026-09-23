# ty: ignore[unresolved-import]
# mypy: disable-error-code="import-not-found, import-untyped, no-any-unimported, attr-defined, abstract"
"""Read, assign, bake, extend, group, and picture the Grasshopper 2 canvas, imported inside Rhino's Python after `g2_start`."""

from collections.abc import Iterable, Sequence
from itertools import groupby
from pathlib import Path

import clr
from document import artifacts, layer_index, Objects, read_objects
from Eto.Drawing import ImageFormat, PointF, Rectangle
from Grasshopper2.Bake import BakeContext, BakeDataState, BakeUpdateMode, IBakeAware, MetaPattern, UserPattern
from Grasshopper2.Components import Component, Side
from Grasshopper2.Doc import DocumentIO, IDocumentObject
from Grasshopper2.Framework import ObjectProxies, PluginRequirement, PluginRequirements, PluginServer
from Grasshopper2.Parameters import IParameter
from Grasshopper2.Parameters.Special import NumberSliderObject, TextInputObject, ToggleObject, ValueObject
from Grasshopper2.SpecialObjects import GroupObject
from Grasshopper2.UI import Editor
from Grasshopper2.UI.Canvas import OpenDocumentOptions
from GrasshopperIO.DataBase import Archive
from records import collect_faults, Fault, File, Record
from Rhino import RhinoDoc
from Rhino.DocObjects import ObjectAttributes
from Rhino.Geometry import Box, Brep, Circle, Curve, GeometryBase, Line, Mesh, Plane, Point3d, Vector3d
from Rhino.Runtime import HostUtils
from RhinoCodePlatform.GH.Context import IScriptObject
from ScriptComponents.Components import BaseScriptComponent, Python3Component
from System import AppDomain, Convert, Enum, Guid
from System.Reflection import Assembly

# --- [TYPES] ----------------------------------------------------------------------------

type Value = str | float | bool | tuple[float, ...] | dict[str, object]

# --- [MODELS] ---------------------------------------------------------------------------


class Data(Record, frozen=True):
    """One parameter's name, path and item counts, and its first values described."""

    name: str
    paths: int
    items: int
    values: tuple[Value, ...]


class Node(Record, frozen=True):
    """One canvas object's id, names, solve messages, inputs, output data, and the objects a group holds."""

    id: str
    name: str
    user_name: str
    messages: tuple[str, ...] = ()
    inputs: tuple[str, ...] = ()
    outputs: tuple[Data, ...] = ()
    members: tuple[str, ...] = ()


class Plugin(Record, frozen=True):
    """One third-party Grasshopper 2 plugin with the id its package names and its components by `chapter/section`, or the reason its library failed to load."""

    id: str | None
    name: str
    location: str
    components: dict[str, tuple[str, ...]]
    failure: str | None = None


# --- [OPERATIONS] -----------------------------------------------------------------------


def _editor() -> Editor | tuple[Fault, ...]:
    """Return the Grasshopper 2 editor, refused until `g2_start` creates it."""
    editor = Editor.Instance
    return (Fault(Editor, None),) if editor is None else editor


def _find(canvas_id: str) -> IDocumentObject | tuple[Fault, ...]:
    """Return the canvas object `canvas_id` names."""
    match _editor():
        case Editor() as editor:
            found = editor.Canvas.Document.Objects.Find(Guid(canvas_id))
            return (Fault(IDocumentObject, canvas_id),) if found is None else found
        case faults:
            return faults


def _describe(item: object) -> Value:
    """Describe one value by its geometry, a point by its coordinates and a solid by its volume."""
    box = item.BoundingBox if isinstance(item, Box) else item.GetBoundingBox(accurate=True) if isinstance(item, GeometryBase) else None
    shape: dict[str, object] = {} if box is None else {"type": type(item).__name__, "min": _describe(box.Min), "max": _describe(box.Max)}
    match item:
        case bool() | int() | float() | str():
            return item
        case Point3d() | Vector3d():
            return (round(item.X, 6), round(item.Y, 6), round(item.Z, 6))
        case Plane():
            return {"origin": _describe(item.Origin), "normal": _describe(item.ZAxis)}
        case Circle():
            return {"center": _describe(item.Center), "normal": _describe(item.Normal), "radius": item.Radius}
        case Line():
            return {"from": _describe(item.From), "to": _describe(item.To), "length": item.Length}
        case Box():
            return {**shape, "volume": item.Volume}
        case Brep():
            return {**shape, "solid": item.IsSolid, "faces": item.Faces.Count, "volume": item.GetVolume() if item.IsSolid else None}
        case Mesh():
            return {**shape, "closed": item.IsClosed, "faces": item.Faces.Count}
        case Curve():
            return {**shape, "closed": item.IsClosed, "length": item.GetLength()}
        case GeometryBase():
            return shape
        case _:
            return f"{type(item).__name__}: {item}"


def _outputs(canvas_object: object) -> tuple[IParameter, ...]:
    """Return a component's output parameters, or a parameter itself."""
    match canvas_object:
        case Component():
            return tuple(canvas_object.Parameters.Outputs)
        case IParameter():
            return (canvas_object,)
        case _:
            return ()


def _ports(parameters: Iterable[IParameter]) -> dict[str, IParameter]:
    """Map each parameter's name and user name to it, script components keeping their signature's names in the user name alone."""
    return {key: parameter for parameter in parameters for key in (parameter.Nomen.Name, parameter.UserName) if key}


def _node(canvas_object: IDocumentObject, sample: int) -> Node:
    """Describe one canvas object with its messages, inputs, the data each output held after the last solve, and group members."""
    state = canvas_object.State
    fault = None if state is None else state.FaultException
    messages = None if state is None or state.Data is None else state.Data.Messages

    def data(parameter: IParameter) -> Data:
        tree = None if parameter.State is None or parameter.State.Data is None else parameter.State.Data.Tree()
        items = () if tree is None else tuple(tree.NonNullItems)
        return Data(parameter.UserName or parameter.Nomen.Name, 0 if tree is None else tree.PathCount, len(items), tuple(_describe(item) for item in items[:sample]))

    return Node(
        str(canvas_object.InstanceId),
        canvas_object.Nomen.Name,
        canvas_object.UserName or "",
        (f"Fault: {fault.Message}",) if fault is not None else () if messages is None else tuple(f"{messages[index].Level}: {messages[index].Text}" for index in range(messages.Count)),
        tuple(parameter.UserName or parameter.Nomen.Name for parameter in canvas_object.Parameters.Inputs) if isinstance(canvas_object, Component) else (),
        tuple(data(parameter) for parameter in _outputs(canvas_object)),
        tuple(str(member) for member in canvas_object.ContentIds) if isinstance(canvas_object, GroupObject) else (),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def graph(sample: int = 3) -> tuple[Node, ...] | tuple[Fault, ...]:
    """Describe every canvas object's messages, inputs, group members, and first output values by geometry after the last solve, a wrong-type wire showing its converted value."""
    editor = _editor()
    return editor if isinstance(editor, tuple) else tuple(_node(canvas_object, sample) for canvas_object in editor.Canvas.Document.Objects.Forwards)


def open_document(path: str) -> tuple[Node, ...] | tuple[Fault, ...]:
    """Open a `.ghz` as the current canvas and describe its objects, refused with each plugin id and saved version the file needs that no loaded plugin supplies."""
    stored = Archive.Read(path, lambda entry: entry == PluginRequirements.ArchiveEntry)
    missing = tuple(Fault(PluginRequirement, str(requirement.Id), (str(requirement.Version),)) for requirement in PluginRequirements.Read(stored).Missing)
    match _editor(), missing:
        case Editor() as editor, ():
            return graph() if editor.Documents.TryOpenDocument(path, OpenDocumentOptions.Activate) else (Fault(DocumentIO, path),)
        case failed:
            return collect_faults(*failed)


def assign(canvas_id: str, values: Sequence[float | bool | str | GeometryBase], input_name: str | None = None) -> Data | tuple[Fault, ...]:
    """Give a slider, toggle, value list, text input, typed parameter, or a component's named input new persistent values, expire it for the next solve, and read the values back."""
    found = _find(canvas_id)
    ports = _ports(found.Parameters.Inputs) if isinstance(found, Component) else {}
    target = found if input_name is None or isinstance(found, tuple) else ports.get(input_name, (Fault(IParameter, input_name, tuple(ports)),))
    match target:
        case NumberSliderObject():
            target.InternalSlider.Values.Assign(Convert.ToDecimal(values[0]))
            held: tuple[object, ...] = (Convert.ToDouble(target.InternalNumber.Value),)
        case ToggleObject():
            target.ToggleState = bool(values[0])
            held = (target.ToggleState,)
        case ValueObject():
            target.AssignTextAndValue(str(values[0]))
            held = (target.Text,)
        case TextInputObject():
            target.Contents = "\n".join(str(value) for value in values)
            held = tuple(target.Values)
        case IParameter():
            target.Set(list(values))
            held = tuple(target.PersistentDataWeak.NonNullItems)
        case tuple():
            return target
        case _:
            return (Fault(IParameter, target.Nomen.Name, tuple(ports)),)
    target.Expire()
    return Data(target.UserName or target.Nomen.Name, 1, len(held), tuple(_describe(value) for value in held))


def bake(doc: RhinoDoc, canvas_id: str, layer_path: str, output: str | None = None) -> Objects | tuple[Fault, ...]:
    """Bake one canvas object, or one of its outputs, through Grasshopper 2's own bake onto `layer_path`, deleting its earlier bake in the same undo step."""
    found = _find(canvas_id)
    outputs = _outputs(found)
    ports = _ports(outputs)
    source = found if output is None or isinstance(found, tuple) else ports.get(output, (Fault(IParameter, output, tuple(ports)),))
    bakeable = source if isinstance(source, tuple) or (isinstance(source, IBakeAware) and source.BakeCapable) else (Fault(IBakeAware, source.Nomen.Name),)
    match bakeable, layer_index(doc, layer_path):
        case IBakeAware(), int() as index:
            attributes = ObjectAttributes()
            attributes.LayerIndex = index
            context = BakeContext(None, bakeable.InstanceId, doc, attributes, UserPattern(), MetaPattern(enableAll=True, embed=False))
            owners = {parameter.InstanceId for parameter in (outputs if output is None else (bakeable,))}
            earlier = [pair.Item2.Id for pair in BakeContext.FindBakedObjects(doc, BakeDataState.Valid | BakeDataState.Expired | BakeDataState.Invalid) if pair.Item1.ProcessGuid in owners]
            undo = doc.BeginUndoRecord(f"Bake {bakeable.Nomen.Name}")
            try:
                doc.Objects.Delete(earlier, quiet=True)
                written = bakeable.BakeShapes(context, BakeUpdateMode.Add)
            finally:
                doc.EndUndoRecord(undo)
                doc.Views.Redraw()
            return read_objects(doc, written, tuple(str(object_id) for object_id in earlier))
        case failed:
            return collect_faults(*failed)


def plugins() -> tuple[Plugin, ...]:
    """Load Grasshopper 2 libraries from Yak packages installed since Grasshopper 2 started, then list each third-party plugin's id and components by chapter and section and each failed library with its reason."""
    PluginServer.ScopeYakPlugins()
    pending = {location for location in PluginServer.State.ScopedLocations if not PluginServer.State.IsLocationLoaded(location)}
    held = {Path(item.Location).stem for item in AppDomain.CurrentDomain.GetAssemblies() if not item.IsDynamic}
    for dependency in {sibling for location in pending for sibling in Path(location).parent.glob("*.dll") if sibling.stem not in held and HostUtils.IsManagedDll(str(sibling))}:
        Assembly.LoadFrom(str(dependency))
    PluginServer.LoadAllScopedPlugins(lambda location: location in pending)
    core = {Path(location).resolve() for location in (*PluginServer.CorePlugins, clr.GetClrType(ObjectProxies).Assembly.Location)}
    proxies = sorted((proxy for proxy in ObjectProxies.Proxies if not proxy.Obsolete), key=lambda proxy: (str(proxy.Plugin.Id), proxy.Nomen.Chapter, proxy.Nomen.Section, proxy.Nomen.Name))
    sections = {key: tuple(proxy.Nomen.Name for proxy in group) for key, group in groupby(proxies, key=lambda proxy: (str(proxy.Plugin.Id), proxy.Nomen.Chapter, proxy.Nomen.Section))}
    return (
        *(
            Plugin(str(plugin.Id), plugin.Name, location, {f"{chapter}/{section}": names for (owner, chapter, section), names in sections.items() if owner == str(plugin.Id)})
            for location, plugin in ((pair.Item1, pair.Item2) for pair in PluginServer.State.Loaded)
            if Path(location).resolve() not in core
        ),
        *(
            Plugin(None, Path(location).stem, location, {}, failure.Reason if failure.Exception is None else f"{failure.Reason} {(failure.Exception.InnerException or failure.Exception).Message}")
            for location, failure in ((pair.Item1, pair.Item2) for pair in PluginServer.State.Failures)
        ),
    )


def script(component_type: type[BaseScriptComponent], title: str, source: str, at: tuple[float, float], outputs: Sequence[str] = ()) -> Node | tuple[Fault, ...]:
    """Place a Python 3 or C# script component titled `title` at `at` with inputs, type hints, and access from its `RunScript` signature and `outputs` naming Python's returned values in order, solved by a later call."""
    editor = _editor()
    if isinstance(editor, tuple):
        return editor
    document = editor.Canvas.Document
    component = component_type.Create(title, source)
    component.UserName = title
    component.Context.EnforceParamsOnCreate = False
    component.Context.InitLanguages(document, component.Context.GetLanguageSpec())
    component.MarshalInputs = component.MarshalOutputs = component.MarshalGuids = component_type is Python3Component
    document.Objects.Add(component, PointF(*at))
    IScriptObject(component).ParamsCollect()
    for parameter in [parameter for parameter in component.Parameters.Outputs if outputs and parameter.UserName]:
        component.Parameters.RemoveOutput(parameter, None)
    for name in outputs:
        component.DoCreateParameter(Side.Output, component.Parameters.OutputCount, None)
        component.Parameters.Output(component.Parameters.OutputCount - 1).VariableName = name
    return _node(component, 0)


def group(name: str, colour: str, ids: Sequence[str]) -> Node | tuple[Fault, ...]:
    """Wrap canvas objects in one group titled `name` in the Open Color family `colour` and read its members back."""
    editor, found = _editor(), [_find(canvas_id) for canvas_id in ids]
    members, faults = [member for member in found if not isinstance(member, tuple)], collect_faults(editor) or collect_faults(*found)
    if faults or isinstance(editor, tuple):
        return faults
    created = GroupObject()
    created.GroupColour, created.UserName = Enum.Parse(created.GroupColour.GetType(), colour), name
    for member in members:
        created.AddContent(member.InstanceId)
    editor.Canvas.Document.Objects.Add(created, PointF(0.0, 0.0))
    return _node(created, 0)


def image(name: str, margin: int = 24) -> File[tuple[str, ...]] | tuple[Fault, ...]:
    """Draw groups, wires, objects, and messages at 1:1 into `.artifacts/rhino/<name>.png` with `margin` pixels around the objects, listing the objects left of or above the canvas origin that fall outside it."""
    editor = _editor()
    if isinstance(editor, tuple):
        return editor
    objects = editor.Canvas.Document.Objects
    bounds = objects.AttributeBounds
    left, top = max(int(bounds.Left) - margin, 0), max(int(bounds.Top) - margin, 0)
    drawn = editor.Canvas.DrawToBitmap(int(bounds.Right) + margin, int(bounds.Bottom) + margin, drawBackground=True, drawWires=True, drawMessages=True)
    cropped = drawn.Clone(Rectangle(left, top, drawn.Width - left, drawn.Height - top))
    path = artifacts() / f"{name}.png"
    try:
        cropped.Save(str(path), ImageFormat.Png)
    finally:
        cropped.Dispose()
        drawn.Dispose()
    outside = tuple(str(item.InstanceId) for item in objects.Forwards if item.Attributes.Bounds.Left < 0 or item.Attributes.Bounds.Top < 0)
    return File(str(path), path.stat().st_size, outside)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Data", "Node", "Plugin", "assign", "bake", "graph", "group", "image", "open_document", "plugins", "script"]
