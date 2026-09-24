# ty: ignore[unresolved-import]
# mypy: disable-error-code="import-not-found, import-untyped, no-any-unimported, attr-defined"
"""Grasshopper 2 canvas operations, imported after `g2_start`."""

from collections.abc import Iterable, Sequence
from itertools import groupby
from pathlib import Path

import clr
from document import artifacts, layer_index, Objects, read_objects
from Eto.Drawing import ImageFormat, PointF, Rectangle
from Grasshopper2.Bake import BakeContext, BakeDataState, BakeUpdateMode, IBakeAware, MetaPattern, UserPattern
from Grasshopper2.Components import Component, Side
from Grasshopper2.Doc import DocumentIO, IDocumentObject, ObjectSolutionState
from Grasshopper2.Framework import ObjectProxies, PluginRequirement, PluginRequirements, PluginServer
from Grasshopper2.Parameters import IParameter
from Grasshopper2.Parameters.Special import NumberSliderObject, TextInputObject, ToggleObject, ValueListObject, ValueObject
from Grasshopper2.SpecialObjects import GroupObject
from Grasshopper2.UI import Editor
from Grasshopper2.UI.Canvas import OpenDocumentOptions
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
    """Parameter data with its first values described."""

    name: str
    paths: int
    items: int
    values: tuple[Value, ...]


class Node(Record, frozen=True):
    """Canvas object as the last solve left it."""

    id: str
    name: str
    user_name: str
    messages: tuple[str, ...] = ()
    inputs: tuple[str, ...] = ()
    outputs: tuple[Data, ...] = ()
    members: tuple[str, ...] = ()


class Plugin(Record, frozen=True):
    """Third-party Grasshopper 2 plugin with its components by `chapter/section`, or the reason its library failed to load."""

    id: str | None
    name: str
    location: str
    components: dict[str, tuple[str, ...]]
    failure: str | None = None


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [RESOLUTION]


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
    """Map each parameter's name and user name to it, the user name holding a script component's signature name."""
    return {key: parameter for parameter in parameters for key in (parameter.Nomen.Name, parameter.UserName) if key}


# --- [READS]


def _measures(item: Box | GeometryBase) -> dict[str, object]:
    """Measure a shape by what its type holds."""
    match item:
        case Box():
            return {"volume": item.Volume}
        case Brep():
            return {"solid": item.IsSolid, "faces": item.Faces.Count, "volume": item.GetVolume() if item.IsSolid else None}
        case Mesh():
            return {"closed": item.IsClosed, "faces": item.Faces.Count}
        case Curve():
            return {"closed": item.IsClosed, "length": item.GetLength()}
        case _:
            return {}


def _describe(item: object) -> Value:
    """Describe a value by its geometry."""
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
        case Box() | GeometryBase():
            box = item.BoundingBox if isinstance(item, Box) else item.GetBoundingBox(accurate=True)
            return {"type": type(item).__name__, "min": _describe(box.Min), "max": _describe(box.Max), **_measures(item)}
        case _:
            return f"{type(item).__name__}: {item}"


def _node(canvas_object: IDocumentObject, sample: int) -> Node:
    """Describe one canvas object after the last solve."""
    match canvas_object.State:
        case ObjectSolutionState(FaultException=None, Data=solved):
            messages = tuple(f"{solved.Messages[index].Level}: {solved.Messages[index].Text}" for index in range(solved.Messages.Count))
        case faulted:
            messages = (f"Fault: {faulted.FaultException.Message}",)

    def data(parameter: IParameter) -> Data:
        tree = parameter.State.Data.Tree()
        items = () if tree is None else tuple(tree.NonNullItems)
        return Data(parameter.UserName or parameter.Nomen.Name, 0 if tree is None else tree.PathCount, len(items), tuple(_describe(item) for item in items[:sample]))

    return Node(
        str(canvas_object.InstanceId),
        canvas_object.Nomen.Name,
        canvas_object.UserName or "",
        messages,
        tuple(parameter.UserName or parameter.Nomen.Name for parameter in canvas_object.Parameters.Inputs) if isinstance(canvas_object, Component) else (),
        tuple(data(parameter) for parameter in _outputs(canvas_object)),
        tuple(str(member) for member in canvas_object.ContentIds) if isinstance(canvas_object, GroupObject) else (),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

# --- [READS]


def graph(sample: int = 3) -> tuple[Node, ...] | tuple[Fault, ...]:
    """Describe every canvas object after the last solve, a wrong-type wire showing its converted value."""
    editor = _editor()
    return editor if isinstance(editor, tuple) else tuple(_node(canvas_object, sample) for canvas_object in editor.Canvas.Document.Objects.Forwards)


def plugins() -> tuple[Plugin, ...]:
    """Load Grasshopper 2 libraries installed since the editor started and list every third-party plugin and failed library."""
    PluginServer.ScopeYakPlugins()
    pending = {location for location in PluginServer.State.ScopedLocations if not PluginServer.State.IsLocationLoaded(location)}
    held = {Path(item.Location).stem for item in AppDomain.CurrentDomain.GetAssemblies() if not item.IsDynamic}
    for dependency in {sibling for location in pending for sibling in Path(location).parent.glob("*.dll") if sibling.stem not in held and HostUtils.IsManagedDll(str(sibling))}:
        Assembly.LoadFrom(str(dependency))
    PluginServer.LoadAllScopedPlugins(lambda location: location in pending)
    core = {Path(location).resolve() for location in (*PluginServer.CorePlugins, clr.GetClrType(ObjectProxies).Assembly.Location)}
    proxies = sorted((proxy for proxy in ObjectProxies.Proxies if not proxy.Obsolete), key=lambda proxy: (str(proxy.Plugin.Id), proxy.Nomen.Chapter, proxy.Nomen.Section, proxy.Nomen.Name))
    components = {
        owner: {
            f"{chapter}/{section}": tuple(proxy.Nomen.Name for proxy in section_members)
            for (chapter, section), section_members in groupby(members, key=lambda proxy: (proxy.Nomen.Chapter, proxy.Nomen.Section))
        }
        for owner, members in groupby(proxies, key=lambda proxy: str(proxy.Plugin.Id))
    }
    return (
        *(
            Plugin((key := str(plugin.Id)), plugin.Name, location, components.get(key, {}))
            for location, plugin in ((pair.Item1, pair.Item2) for pair in PluginServer.State.Loaded)
            if Path(location).resolve() not in core
        ),
        *(
            Plugin(None, Path(location).stem, location, {}, failure.Reason if failure.Exception is None else f"{failure.Reason} {(failure.Exception.InnerException or failure.Exception).Message}")
            for location, failure in ((pair.Item1, pair.Item2) for pair in PluginServer.State.Failures)
        ),
    )


# --- [EDITS]


def assign(canvas_id: str, values: Sequence[float | bool | str | GeometryBase], input_name: str | None = None) -> Data | tuple[Fault, ...]:
    """Set persistent values on a value source or an unwired input, expire it for the next solve, and read the values back."""
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
        case ValueListObject():
            target.SelectItem(Convert.ToInt32(values[0]))
            held = tuple(index for index in range(target.ItemCount) if target.ItemSelected(index))
        case ValueObject():
            target.AssignTextAndValue(str(values[0]))
            if target.ErrorMessage:
                return (Fault(ValueObject, target.Text, (target.ErrorMessage,)),)
            held = (target.Text,)
        case TextInputObject():
            target.Contents = "\n".join(str(value) for value in values)
            held = tuple(target.Values)
        case IParameter() if target.Inputs.Count:
            return (Fault(IParameter, target.UserName or target.Nomen.Name),)
        case IParameter():
            target.Set(list(values))
            held = tuple(target.PersistentDataWeak.NonNullItems)
        case tuple():
            return target
        case _:
            return (Fault(IParameter, target.Nomen.Name, tuple(ports)),)
    target.Expire()
    return Data(target.UserName or target.Nomen.Name, 1, len(held), tuple(_describe(value) for value in held))


def script(component_type: type[BaseScriptComponent], title: str, source: str, at: tuple[float, float], outputs: Sequence[str] = ()) -> Node | tuple[Fault, ...]:
    """Place a Python 3 or C# script component with inputs from its `RunScript` signature and `outputs` in return order, solved by a later call."""
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


def group(name: str, color: str, ids: Sequence[str]) -> Node | tuple[Fault, ...]:
    """Group canvas objects under `name` in Open Color family `color`, refused for an unknown family and each object a group cannot hold."""
    editor, found, created = _editor(), [_find(canvas_id) for canvas_id in ids], GroupObject()
    family = created.GroupColour.GetType()
    families = tuple(Enum.GetNames(family))
    members = [member for member in found if not isinstance(member, tuple)]
    if (faults := collect_faults(editor) or collect_faults(*found, () if color in families else (Fault(GroupObject, color, families),))) or isinstance(editor, tuple):
        return faults
    created.GroupColour, created.UserName = Enum.Parse(family, color), name
    if refused := tuple(Fault(GroupObject, str(member.InstanceId)) for member in members if not created.AddContent(member.InstanceId)):
        return refused
    editor.Canvas.Document.Objects.Add(created, PointF(0.0, 0.0))
    return _node(created, 0)


def bake(doc: RhinoDoc, canvas_id: str, layer_path: str, output: str | None = None) -> Objects | tuple[Fault, ...]:
    """Bake a solved canvas object or one of its outputs onto `layer_path` in one undo step that deletes its earlier bake."""
    found = _find(canvas_id)
    outputs = _outputs(found)
    ports = _ports(outputs)
    source = found if output is None or isinstance(found, tuple) else ports.get(output, (Fault(IParameter, output, tuple(ports)),))
    bakeable = source if isinstance(source, tuple) or (isinstance(source, IBakeAware) and source.BakeCapable) else (Fault(IBakeAware, source.Nomen.Name),)
    match bakeable, layer_index(doc, layer_path):
        case IBakeAware(), int() as index:
            attributes = ObjectAttributes()
            attributes.LayerIndex = index
            context = BakeContext(None, bakeable.InstanceId, doc, attributes, UserPattern(), MetaPattern(enableAll=False, embed=False))
            owners = {parameter.InstanceId for parameter in (outputs if output is None else (bakeable,))}
            earlier = [pair.Item2.Id for pair in BakeContext.FindBakedObjects(doc, BakeDataState.Valid | BakeDataState.Expired | BakeDataState.Invalid) if pair.Item1.ProcessGuid in owners]
            undo = doc.BeginUndoRecord(f"Bake {bakeable.Nomen.Name}")
            try:
                doc.Objects.Delete(earlier, quiet=True)
                written = bakeable.BakeShapes(context, BakeUpdateMode.Add)
            finally:
                doc.EndUndoRecord(undo)
                doc.Views.Redraw()
            return read_objects(doc, written or (), tuple(str(object_id) for object_id in earlier))
        case failed:
            return collect_faults(*failed)


# --- [FILES]


def open_document(path: str) -> tuple[Node, ...] | tuple[Fault, ...]:
    """Open a `.ghz` as the current canvas and describe it, refused for each plugin no loaded build supplies and when the canvas holds another file afterward."""
    missing = tuple(Fault(PluginRequirement, str(requirement.Id), (str(requirement.Version),)) for requirement in PluginRequirements.FromFile(path).Missing)
    match _editor(), missing:
        case Editor() as editor, ():
            editor.Documents.TryOpenDocument(path, OpenDocumentOptions.Activate)
            return graph() if editor.Canvas.Document.File.Path == path else (Fault(DocumentIO, path),)
        case failed:
            return collect_faults(*failed)


def image(name: str, margin: int = 24) -> File[tuple[str, ...]] | tuple[Fault, ...]:
    """Draw the canvas at 1:1 into `.artifacts/rhino/<name>.png`, listing objects left of or above the origin that fall outside it."""
    editor = _editor()
    if isinstance(editor, tuple):
        return editor
    objects = editor.Canvas.Document.Objects
    bounds = objects.AttributeBounds
    left, top = max(int(bounds.Left) - margin, 0), max(int(bounds.Top) - margin, 0)
    if (drawn := editor.Canvas.DrawToBitmap(int(bounds.Right) + margin, int(bounds.Bottom) + margin, drawBackground=True, drawWires=True, drawMessages=True)) is None:
        return (Fault(type(editor.Canvas), name),)
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
