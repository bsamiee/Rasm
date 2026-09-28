# ty: ignore[unresolved-import, too-many-positional-arguments]
# mypy: disable-error-code="import-not-found, import-untyped, no-any-unimported, no-any-return, attr-defined, call-arg, type-abstract"
# /// script
# dependencies = ["msgspec"]
#
# [tool.ty.environment]
# extra-paths = ["."]
# ///
"""Grasshopper 2 task documents, builds, layout, values, bakes, clusters, plugins, and pictures, imported after `g2_start`."""

from collections import Counter
from collections.abc import Iterable, Mapping, Sequence
from functools import cache, reduce
from itertools import accumulate, chain, groupby, islice
import math
from pathlib import Path

import clr
from document import artifacts, layer_index, Objects, read_objects
from Eto.Drawing import Bitmap, ImageFormat, PixelFormat, PointF, RectangleF
from Grasshopper2.Bake import BakeContext, BakeDataState, BakeUpdateMode, IBakeAware, MetaPattern, UserPattern
from Grasshopper2.Components import Component, Side
from Grasshopper2.Data import Modifiers
from Grasshopper2.Doc import Document, DocumentIO, DocumentState, FileContents, IAttributes, IDocumentObject, IParameterAttributes, ObjectActivity
from Grasshopper2.Framework import ObjectProxies, ObjectProxy, PluginRequirement, PluginRequirements, PluginServer
from Grasshopper2.Parameters import Connections, IParameter, IPin, Pins
from Grasshopper2.Parameters.Special import NumberSliderObject, TextInputObject, ToggleObject, ValueListObject, ValueObject
from Grasshopper2.Parameters.Standard import AbsoluteTolerancePin, UnitSystem, UnitSystemPin
from Grasshopper2.SpecialObjects import GroupObject
from Grasshopper2.Types.Conversion import ConversionServer
from Grasshopper2.UI import Editor, UiNumber
from Grasshopper2.UI.Canvas import WireShape
from Grasshopper2.UI.Flex import ControlGraphics
from Grasshopper2.UI.Skinning import Shape
from Grasshopper2.Undo import ActionList
import msgspec
from records import collect_faults, Fault, File, Record
from Rhino import RhinoDoc
from Rhino.DocObjects import ObjectAttributes
from Rhino.Geometry import Box, Brep, Circle, Curve, GeometryBase, Interval, Line, Mesh, Plane, Point3d, Rectangle3d, Vector3d
from Rhino.Runtime import HostUtils
from RhinoCodePlatform.GH import IScriptParameter
from RhinoCodePlatform.GH.Context import IScriptObject
from ScriptComponents.Components import BaseScriptComponent, CSharpComponent, Python3Component
from System import Activator, AppDomain, Array, Convert, Enum, Guid, Object, TimeSpan, Type
from System.Collections.Generic import IEnumerable
from System.Reflection import Assembly, RuntimeReflectionExtensions

# --- [TYPES] ----------------------------------------------------------------------------

type Value = str | float | bool | tuple[float, ...] | dict[str, object]
type Item = float | bool | str | GeometryBase
type Modifier = str | tuple[str, int]
type Port = str | int

# --- [MODELS] ---------------------------------------------------------------------------


class Data(Record, frozen=True):
    """Parameter data with its first paths and values, nulls, and non-finite numbers counted."""

    name: str
    paths: int
    items: int
    values: tuple[Value, ...]
    labels: tuple[str, ...] = ()
    nulls: int = 0
    nonfinite: int = 0


class Node(Record, frozen=True):
    """Document object as the last solve left it, each input's sources as `(input, owner id, port)`, `seconds` None with no measured solve."""

    id: str
    name: str
    user_name: str
    bounds: tuple[float, float, float, float]
    messages: tuple[str, ...] = ()
    inputs: tuple[str, ...] = ()
    sources: tuple[tuple[str, str, str], ...] = ()
    outputs: tuple[Data, ...] = ()
    members: tuple[str, ...] = ()
    disabled: bool = False
    seconds: float | None = None


class Plugin(Record, frozen=True):
    """Third-party Grasshopper 2 plugin with its components by `chapter/section`, or the reason its library failed to load."""

    id: str | None
    name: str
    location: str
    components: dict[str, tuple[str, ...]]
    failure: str | None = None


class Slider(Record, frozen=True):
    """Number slider part in model units, `grip` the grip text format."""

    key: str
    lower: float
    value: float
    upper: float
    decimals: int = 0
    name: str | None = None
    grip: str | None = None


class Part(Record, frozen=True):
    """Component or parameter part by proxy guid, a value list with `items`, or a script component with `script`."""

    key: str
    selector: str
    name: str | None = None
    values: dict[str, tuple[Item, ...]] = msgspec.field(default_factory=dict)
    modifiers: dict[str, tuple[Modifier, ...]] = msgspec.field(default_factory=dict)
    items: tuple[tuple[str, str], ...] = ()
    selected: int = 0
    script: str | None = None
    outputs: tuple[str, ...] = ()


class Stage(Record, frozen=True):
    """Parts one named group holds in an Open Color family."""

    name: str
    color: str
    parts: tuple[Part | Slider, ...]


class Wire(Record, frozen=True):
    """Wire from a source output to a target input, ends named by part key or canvas id."""

    source: str
    output: Port
    target: str
    input: Port


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [RESOLUTION]


def _partition[K, V](results: Mapping[K, V | tuple[Fault, ...]]) -> tuple[dict[K, V], tuple[Fault, ...]]:
    """Split independent results into their values and every fault among them."""
    return {key: result for key, result in results.items() if not isinstance(result, tuple)}, collect_faults(*results.values())


def _find(document: Document, canvas_id: str) -> IDocumentObject | tuple[Fault, ...]:
    """Return the document object `canvas_id` names."""
    found = document.Objects.Find(Guid.Parse(canvas_id))
    return (Fault(IDocumentObject, canvas_id),) if found is None else found


def _parameters(document_object: object, side: Side) -> tuple[IParameter, ...]:
    """Return a component's inputs or outputs, or a parameter itself."""
    match document_object:
        case Component():
            return tuple(document_object.Parameters.Inputs if side == Side.Input else document_object.Parameters.Outputs)
        case IParameter():
            return (document_object,)
        case _:
            return ()


def _name(parameter: IParameter) -> str:
    """Return the name a build wires a port by, a script parameter's variable name and every other port's own name."""
    match parameter:
        case IScriptParameter():
            return IScriptParameter(parameter).VariableName
        case _:
            return parameter.Nomen.Name


def _port(parameters: Sequence[IParameter], port: Port) -> IParameter | tuple[Fault, ...]:
    """Return the parameter `port` names by name, user name, or index, `""` naming a lone parameter."""
    named = {key: parameter for parameter in parameters for key in (_name(parameter), parameter.UserName) if key} | ({"": parameters[0]} if len(parameters) == 1 else {})
    match port:
        case int() if 0 <= port < len(parameters):
            return parameters[port]
        case str() if port in named:
            return named[port]
        case _:
            return (Fault(IParameter, port, tuple(named)),)


def _joint(objects: Mapping[str, IDocumentObject | tuple[Fault, ...]], key: str, port: Port, side: Side) -> IParameter | tuple[Fault, ...]:
    """Return a wire end on the object `key` names, an object that failed to resolve adding no fault of its own."""
    match objects.get(key):
        case None:
            return (Fault(Wire, key, tuple(objects)),)
        case tuple():
            return ()
        case found:
            return _port(_parameters(found, side), port)


def _converted(parameter: IParameter, items: Sequence[Item]) -> object | tuple[Fault, ...]:
    """Return values in a typed port's own type through Grasshopper 2's conversions as its array, a port of any type taking them as a list."""
    match parameter.TypeAssistantWeak:
        case None:
            return list(items)
        case assistant:
            converted = [(item, *ConversionServer.Convert(item, assistant.Type)) for item in items]
            if faults := tuple(Fault(ConversionServer, item, (assistant.Type.Name,)) for item, held, _ in converted if not held):
                return faults
            array = Array.CreateInstance(assistant.Type, len(converted))
            for index, (_, _, value) in enumerate(converted):
                array.SetValue(value, index)
            return array


def _source(document: Document, source_id: object) -> tuple[str, str]:
    """Return the owner id and port name of a wire source, a source the document lacks as its id alone."""
    found = document.Objects.FindParameter(source_id)
    return (str(source_id), "") if found is None else (str((found.ParentObject or found).InstanceId), _name(found))


def _family(color: str) -> object | tuple[Fault, ...]:
    """Return the Open Color family `color` names."""
    family = Type.GetType("Eto.Drawing.OpenColor+Family, Grasshopper2", throwOnError=True)
    names = tuple(Enum.GetNames(family))
    return Enum.Parse(family, color) if color in names else (Fault(GroupObject, color, names),)


def _modifiers(modifiers: Sequence[Modifier]) -> Modifiers | tuple[Fault, ...]:
    """Chain `With<Name>` modifiers from an empty set, `(name, depth)` passing its depth."""
    accepted = tuple(sorted({method.Name.removeprefix("With") for method in clr.GetClrType(Modifiers).GetMethods() if method.Name.startswith("With") and not method.Name.startswith("Without")}))
    steps = [(modifier,) if isinstance(modifier, str) else modifier for modifier in modifiers]
    unknown = tuple(Fault(Modifiers, name, accepted) for name, *_ in steps if name not in accepted)

    def applied(modifier_set: Modifiers, step: tuple[str] | tuple[str, int]) -> Modifiers:
        name, *arguments = step
        return getattr(modifier_set, f"With{name}")(*arguments)

    return unknown or reduce(applied, steps, Modifiers.Empty)


# --- [PARTS]


def _group(document: Document, doc: RhinoDoc, name: str, family: object, members: Sequence[IDocumentObject]) -> GroupObject | tuple[Fault, ...]:
    """Add a named group holding `members`, pinned to `doc`'s unit system and absolute tolerance, refusing a repeated member."""
    created = GroupObject()
    created.GroupColour, created.UserName = family, name
    if refused := tuple(Fault(GroupObject, str(member.InstanceId)) for member in members if not created.AddContent(member.InstanceId)):
        return refused
    document.Objects.Add(created, PointF(0.0, 0.0))
    units, tolerance = UnitSystemPin(), AbsoluteTolerancePin()
    units.Set([UnitSystem(doc)])
    tolerance.Set([doc.ModelAbsoluteTolerance])
    for index, pin in enumerate((units, tolerance)):
        document.Objects.Add(pin, PointF(0.0, 0.0))
        Pins.Pin(pin, created, index, None)
    return created


# --- [READS]


def _describe(item: object) -> Value:
    """Describe a value by its geometry."""

    def measures(shape: Box | GeometryBase) -> dict[str, object]:
        match shape:
            case Box():
                return {"volume": shape.Volume}
            case Brep():
                return {"solid": shape.IsSolid, "faces": shape.Faces.Count, "volume": shape.GetVolume() if shape.IsSolid else None}
            case Mesh():
                return {"closed": shape.IsClosed, "faces": shape.Faces.Count}
            case Curve():
                return {"closed": shape.IsClosed, "length": shape.GetLength()}
            case _:
                return {}

    match item:
        case bool() | int() | float() | str():
            return item
        case Point3d() | Vector3d():
            return (round(item.X, 6), round(item.Y, 6), round(item.Z, 6))
        case Interval():
            return (item.T0, item.T1)
        case Plane():
            return {"origin": _describe(item.Origin), "normal": _describe(item.ZAxis)}
        case Rectangle3d():
            return {"origin": _describe(item.Plane.Origin), "width": item.Width, "height": item.Height}
        case Circle():
            return {"center": _describe(item.Center), "normal": _describe(item.Normal), "radius": item.Radius}
        case Line():
            return {"from": _describe(item.From), "to": _describe(item.To), "length": item.Length}
        case Box() | GeometryBase():
            box = item.BoundingBox if isinstance(item, Box) else item.GetBoundingBox(accurate=True)
            return {"type": type(item).__name__, "min": _describe(box.Min), "max": _describe(box.Max), **measures(item)}
        case _:
            return f"{type(item).__name__}: {item}"


def _node(document_object: IDocumentObject, sample: int) -> Node:
    """Describe one document object after the last solve, each output by its first `sample` paths and values."""
    state, inputs, bounds = document_object.State, _parameters(document_object, Side.Input), document_object.Attributes.Bounds

    def data(parameter: IParameter) -> Data:
        name = _name(parameter)
        match parameter.State.Data.Tree():
            case None:
                return Data(name, 0, 0, ())
            case tree:
                return Data(
                    name,
                    tree.PathCount,
                    tree.LeafCount,
                    tuple(_describe(item) for item in islice(tree.NonNullItems, sample)),
                    tuple(str(path) for path in islice(tree.Paths, sample)),
                    tree.NullCount,
                    sum(isinstance(item, float) and not math.isfinite(item) for item in tree.NonNullItems),
                )

    return Node(
        str(document_object.InstanceId),
        document_object.Nomen.Name,
        document_object.UserName or "",
        (bounds.Left, bounds.Top, bounds.Right, bounds.Bottom),
        (f"Fault: {state.FaultException.Message}",)
        if state.FaultException is not None
        else tuple(f"{message.Level}: {message.Text}" for message in chain(state.Data.Messages.Errors, state.Data.Messages.Warnings, state.Data.Messages.Remarks)),
        tuple(map(_name, inputs)),
        tuple((_name(parameter), *_source(document_object.Document, source)) for parameter in inputs for source in parameter.Inputs.Forwards),
        tuple(map(data, _parameters(document_object, Side.Output))),
        tuple(str(member) for member in document_object.ContentIds) if isinstance(document_object, GroupObject) else (),
        document_object.Activity == ObjectActivity.Disabled,
        None if state.Data.Duration == TimeSpan.MinValue else state.Data.Duration.TotalSeconds,
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

# --- [DOCUMENTS]


def definition(path: str | None = None) -> Document | tuple[Fault, ...]:
    """Return the document holding `path`, opened or created behind the current canvas with its Rhino preview off, or the current canvas for `None`."""
    editor = Editor.Instance
    match editor, path:
        case None, _:
            return (Fault(Editor, None),)
        case _, None:
            return editor.Canvas.Document
        case _, str() if (index := editor.Documents.Index(path)) >= 0:
            return editor.Documents[index].Root
        case _, str() if Path(path).exists():
            if missing := tuple(Fault(PluginRequirement, str(requirement.Id), (str(requirement.Version),)) for requirement in PluginRequirements.FromFile(path).Missing):
                return missing
            reader = DocumentIO(trackFiles=False, reportErrors=False, resolvePlugins=False)
            reader.Open(path)
            opened = reader.Document
            opened.State = DocumentState.Inactive
        case _, str():
            Path(path).parent.mkdir(parents=True, exist_ok=True)
            opened = Document.NewInactiveDocument()
            DocumentIO(opened, trackFiles=False, reportErrors=False).Save(path, FileContents.Small)
    opened.Display.Enabled = False
    editor.Documents.Queue(opened)
    return opened


def show(document: Document) -> str:
    """Make a task document the current canvas with its Rhino preview on and return its file."""
    document.Display.Enabled = True
    Editor.Instance.Documents.Push(document)
    return document.File.Path


# --- [READS]


def graph(document: Document, sample: int = 3) -> tuple[Node, ...]:
    """Describe every object of a document after its last solve."""
    return tuple(_node(document_object, sample) for document_object in document.Objects.Forwards)


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


def build(document: Document, doc: RhinoDoc, stages: Sequence[Stage], wires: Sequence[Wire] = ()) -> dict[str, str] | tuple[Fault, ...]:
    """Add stages of parts as named groups laid out by flow, with values, modifiers, and wires, and map each key to its canvas id, or return every fault."""
    all_parts = [part for stage in stages for part in stage.parts]
    parts = {part.key: part for part in all_parts}
    repeated = tuple(Fault(Part, key) for key, count in Counter(part.key for part in all_parts).items() if count > 1)
    fields = {(key, name): part for key, part in parts.items() if isinstance(part, Part) for name in dict.fromkeys((*part.values, *part.modifiers))}

    def scripted(emitted: IDocumentObject, part: Part, source: str) -> IDocumentObject | tuple[Fault, ...]:
        if not isinstance(emitted, BaseScriptComponent):
            return (Fault(BaseScriptComponent, part.selector),)
        component = {clr.GetClrType(kind): kind for kind in (Python3Component, CSharpComponent)}[emitted.GetType()].Create(part.name or part.key, source)
        component.Context.EnforceParamsOnCreate = False
        component.Context.InitLanguages(document, component.Context.GetLanguageSpec())
        component.MarshalInputs = component.MarshalOutputs = component.MarshalGuids = isinstance(component, Python3Component)
        document.Objects.Add(component, PointF(0.0, 0.0))
        IScriptObject(component).ParamsCollect()
        for parameter in [parameter for parameter in component.Parameters.Outputs if part.outputs and parameter.UserName]:
            component.Parameters.RemoveOutput(parameter, None)
        for name in part.outputs:
            component.DoCreateParameter(Side.Output, component.Parameters.OutputCount, None)
            component.Parameters.Output(component.Parameters.OutputCount - 1).VariableName = name
        return component

    def listed(emitted: IDocumentObject, part: Part) -> IDocumentObject | tuple[Fault, ...]:
        if not isinstance(emitted, ValueListObject):
            return (Fault(ValueListObject, part.selector),)
        document.Objects.Add(emitted, PointF(0.0, 0.0))
        kind = clr.GetClrType(ValueListObject).Assembly.GetType("Grasshopper2.Parameters.Special.ValueListItem", throwOnError=True)
        items = Array.CreateInstance(kind, len(part.items))
        for index, (name, text) in enumerate(part.items):
            items.SetValue(Activator.CreateInstance(kind, Array[Object]([name, text, index == part.selected, None])), index)
        setter = next(method for method in RuntimeReflectionExtensions.GetRuntimeMethods(clr.GetClrType(ValueListObject)) if method.Name == "Set" and method.IsAssembly)
        setter.Invoke(emitted, Array[Object]([items, False]))
        return emitted

    def created(part: Part | Slider) -> IDocumentObject | tuple[Fault, ...]:
        match part, None if isinstance(part, Slider) else ObjectProxies.FindById(Guid.Parse(part.selector)):
            case Slider(), _:
                slider = NumberSliderObject(part.name or part.key, UiNumber(part.decimals, *map(Convert.ToDecimal, (part.value, part.lower, part.upper))))
                if part.grip is not None:
                    slider.GripFormat = part.grip
                return slider
            case Part(script=str() as source), ObjectProxy() as proxy:
                return scripted(proxy.Emit(None), part, source)
            case Part(items=()), ObjectProxy() as proxy:
                return proxy.Emit(None)
            case Part(), ObjectProxy() as proxy:
                return listed(proxy.Emit(None), part)
            case Part(selector=selector), _:
                return (Fault(ObjectProxy, selector),)

    made = {key: created(part) for key, part in parts.items()}
    objects, object_faults = _partition(made)
    families, family_faults = _partition(dict(enumerate(_family(stage.color) for stage in stages)))
    ports, port_faults = _partition({(key, name): _joint(made, key, name, Side.Input) for key, name in fields})
    chains, chain_faults = _partition({(key, name): _modifiers(part.modifiers[name]) for (key, name), part in fields.items() if name in part.modifiers})
    persistent, value_faults = _partition({(key, name): _converted(ports[key, name], part.values[name]) for (key, name), part in fields.items() if name in part.values and (key, name) in ports})
    sources, source_faults = _partition({index: _joint(made, connection.source, connection.output, Side.Output) for index, connection in enumerate(wires)})
    targets, target_faults = _partition({index: _joint(made, connection.target, connection.input, Side.Input) for index, connection in enumerate(wires)})
    if faults := (*repeated, *object_faults, *family_faults, *port_faults, *value_faults, *chain_faults, *source_faults, *target_faults):
        document.Methods.DeleteObjects(Array[IDocumentObject]([item for item in objects.values() if item.Document is not None]), None, ActionList.Empty)
        return faults
    for item in (item for item in objects.values() if item.Document is None):
        document.Objects.Add(item, PointF(0.0, 0.0))
    for key, name in ((key, part.name) for key, part in parts.items() if isinstance(part, Part) and part.name is not None):
        objects[key].UserName = name
    for field, items in persistent.items():
        ports[field].Set(items)
    for field, modifier_set in chains.items():
        ports[field].Modifiers = modifier_set
    for index, source in sources.items():
        Connections.Connect(source, targets[index], None)
    grouped = [_group(document, doc, stage.name, families[index], [objects[part.key] for part in stage.parts]) for index, stage in enumerate(stages)]
    arrange(document)
    return collect_faults(*grouped) or {key: str(item.InstanceId) for key, item in objects.items()}


def wire(document: Document, wires: Sequence[Wire], *, replace: bool = True) -> tuple[Node, ...] | tuple[Fault, ...]:
    """Join existing objects by canvas id, `replace` dropping each target input's earlier sources, and describe each target."""
    found = {canvas_id: _find(document, canvas_id) for connection in wires for canvas_id in (connection.source, connection.target)}
    objects, object_faults = _partition(found)
    sources, source_faults = _partition({index: _joint(found, connection.source, connection.output, Side.Output) for index, connection in enumerate(wires)})
    targets, target_faults = _partition({index: _joint(found, connection.target, connection.input, Side.Input) for index, connection in enumerate(wires)})
    if faults := (*object_faults, *source_faults, *target_faults):
        return faults
    for target in targets.values() if replace else ():
        Connections.DisconnectAllInputs(target, None)
    for index, source in sources.items():
        Connections.Connect(source, targets[index], None)
    return tuple(_node(objects[canvas_id], 0) for canvas_id in dict.fromkeys(connection.target for connection in wires))


def assign(document: Document, canvas_id: str, values: Sequence[Item] | None = None, input_name: str | None = None, modifiers: Sequence[Modifier] | None = None) -> Data | tuple[Fault, ...]:
    """Set a value source or an unwired input and its modifiers, `()` clearing and `None` keeping, expire it for the next solve, and return the values it holds."""
    found = _find(document, canvas_id)
    target = found if input_name is None or isinstance(found, tuple) else _port(_parameters(found, Side.Input), input_name)
    modifier_set = None if modifiers is None else _modifiers(modifiers)
    if isinstance(target, tuple) or isinstance(modifier_set, tuple):
        return collect_faults(target, modifier_set)
    match target, values:
        case NumberSliderObject(), [value, *_]:
            target.InternalSlider.Values.Assign(Convert.ToDecimal(value))
            held: tuple[object, ...] = (Convert.ToDouble(target.InternalNumber.Value),)
        case ToggleObject(), [value, *_]:
            target.ToggleState = bool(value)
            held = (target.ToggleState,)
        case ValueListObject(), [value, *_]:
            target.SelectItem(Convert.ToInt32(value))
            held = tuple(index for index in range(target.ItemCount) if target.ItemSelected(index))
        case ValueObject(), [value, *_]:
            target.AssignTextAndValue(str(value))
            if target.ErrorMessage:
                return (Fault(ValueObject, target.Text, (target.ErrorMessage,)),)
            held = (target.Text,)
        case TextInputObject(), [*_]:
            target.Contents = "\n".join(map(str, values))
            held = tuple(target.Values)
        case IParameter(), [*_] if target.Inputs.Count:
            return (Fault(IParameter, _name(target)),)
        case IParameter(), ():
            target.PersistentDataWeak = None
            held = ()
        case IParameter(), [*_]:
            match _converted(target, values):
                case tuple() as refused:
                    return refused
                case items:
                    target.Set(items)
            held = tuple(target.PersistentDataWeak.NonNullItems)
        case IParameter(), None:
            held = () if target.PersistentDataWeak is None else tuple(target.PersistentDataWeak.NonNullItems)
        case _:
            return (Fault(IParameter, target.Nomen.Name),)
    if modifier_set is not None:
        target.Modifiers = modifier_set
    target.Expire()
    return Data(_name(target), int(bool(held)), len(held), tuple(_describe(value) for value in held))


def delete(document: Document, ids: Sequence[str]) -> int | tuple[Fault, ...]:
    """Delete objects with their wires and return the count."""
    found = [_find(document, canvas_id) for canvas_id in ids]
    return collect_faults(*found) or document.Methods.DeleteObjects(Array[IDocumentObject](found), None, None)


def group(document: Document, doc: RhinoDoc, name: str, color: str, ids: Sequence[str]) -> Node | tuple[Fault, ...]:
    """Group objects under `name` in Open Color family `color`, pinned to `doc`'s units and tolerance, refused for an unknown family and a repeated object."""
    found, family = [_find(document, canvas_id) for canvas_id in ids], _family(color)
    match collect_faults(*found, family) or _group(document, doc, name, family, found):
        case GroupObject() as created:
            return _node(created, 0)
        case faults:
            return faults


def cluster(document: Document, ids: Sequence[str], name: str) -> Node | tuple[Fault, ...]:
    """Collapse objects into one cluster named `name` that keeps their boundary wires and takes their place in each group, refused with the reason for a set no cluster holds."""
    found = [_find(document, canvas_id) for canvas_id in ids]
    if faults := collect_faults(*found):
        return faults
    members, connectivity = Array[IDocumentObject](found), document.Objects.Connectivity
    founding = {member.FoundingObject.InstanceId for member in members}
    between = {node.Id for member in founding for node in connectivity.FindAllOutputs(member)} & {node.Id for member in founding for node in connectivity.FindAllInputs(member)}
    allowed, reason = document.Methods.CanCreateCluster(members)
    if not allowed:
        return (Fault(Document, name, (reason,)),)
    if between - founding:
        return (Fault(Document, name, ("A cluster may not contain a concave set of objects.",)),)
    created, placed = document.Methods.ClusterObjects(members, None), {member.InstanceId for member in members}
    created.UserName = name
    shared = {owner: common for owner in document.Objects.Groups if (common := placed.intersection(owner.ContentIds))}
    for owner, gone in shared.items():
        for member in gone:
            owner.RemoveContent(member)
        owner.AddContent(created.InstanceId)
    return _node(created, 0)


def arrange(document: Document, gap: float = 60.0) -> tuple[Node, ...]:
    """Lay objects out in columns by wire depth, groups sharing a member as one block, blocks left to right by the longest chain of blocks feeding them from the origin, and describe the document."""
    objects = {str(item.InstanceId): item for item in document.Objects.Forwards if not isinstance(item, (GroupObject, IPin))}
    groups = [(owner, members) for owner in document.Objects.Groups if (members := frozenset(map(str, owner.ContentIds)) & objects.keys())]
    merged = reduce(
        lambda held, members: [*(block for block in held if not block & members), members.union(*(block for block in held if block & members))],
        (members for _, members in groups),
        list[frozenset[str]](),
    )
    block_of = {key: key for key in objects} | {key: min(block) for block in merged for key in block}
    blocks = {block: tuple(key for key in objects if block_of[key] == block) for block in dict.fromkeys(block_of.values())}

    @cache
    def feeds(key: str) -> frozenset[str]:
        return frozenset(feed for parameter in _parameters(objects[key], Side.Input) for source in parameter.Inputs.Forwards for feed, _ in (_source(document, source),) if feed in objects)

    @cache
    def depth(key: str) -> int:
        return max((depth(feed) + 1 for feed in feeds(key)), default=0)

    def move(keys: Iterable[str], left: float, top: float, origin: RectangleF) -> None:
        for key in keys:
            objects[key].Attributes.Move(left - origin.Left, top - origin.Top)

    def relaid(owner: GroupObject) -> GroupObject:
        owner.Attributes.InvalidateLayout()
        owner.Attributes.Layout(Shape.Default)
        return owner

    def pack(keys: tuple[str, ...]) -> RectangleF:
        columns = [tuple(column) for _, column in groupby(sorted(keys, key=depth), key=depth)]
        widths = [max(objects[key].Attributes.Bounds.Width for key in column) for column in columns]
        for left, column in zip(accumulate((width + gap for width in widths[:-1]), initial=0.0), columns, strict=True):
            for top, key in zip(accumulate((objects[key].Attributes.Bounds.Height + gap for key in column[:-1]), initial=0.0), column, strict=True):
                move((key,), left, top, objects[key].Attributes.Bounds)
        owned = [relaid(owner) for owner, members in groups if members.intersection(keys)]
        chrome = (*owned, *(pin for owner in owned for pin in owner.Pins.AboveAndBelow))
        return reduce(RectangleF.Union, (item.Attributes.Bounds for item in (*(objects[key] for key in keys), *chrome)))

    packed = {block: pack(keys) for block, keys in blocks.items()}
    edges = {(block_of[feed], block_of[key]) for key in objects for feed in feeds(key) if block_of[feed] != block_of[key]}
    start = reduce(lambda held, _: {block: max((held[source] + 1 for source, target in edges if target == block), default=0) for block in blocks}, blocks, dict.fromkeys(blocks, 0))
    levels = [tuple(level) for _, level in groupby(sorted(blocks, key=start.__getitem__), key=start.__getitem__)]
    for left, level in zip(accumulate((max(packed[block].Width for block in column) + gap for column in levels[:-1]), initial=0.0), levels, strict=True):
        for top, block in zip(accumulate((packed[block].Height + gap for block in level[:-1]), initial=0.0), level, strict=True):
            move(blocks[block], left, top, packed[block])
    for owner in document.Objects.Groups:
        relaid(owner)
    return graph(document, 0)


def bake(doc: RhinoDoc, document: Document, canvas_id: str, layer_path: str | None = None, output: str | None = None) -> Objects | tuple[Fault, ...]:
    """Bake a solved object or one output in one undo step that deletes its earlier bake, onto `layer_path` or by each item's `Rhino.*` meta."""
    found = _find(document, canvas_id)
    outputs = _parameters(found, Side.Output)
    source = found if output is None or isinstance(found, tuple) else _port(outputs, output)
    bakeable = source if isinstance(source, tuple) or (isinstance(source, IBakeAware) and source.BakeCapable) else (Fault(IBakeAware, source.Nomen.Name),)

    def baked(target: IBakeAware, attributes: ObjectAttributes | None, meta: MetaPattern) -> Objects | tuple[Fault, ...]:
        context = BakeContext(None, target.InstanceId, doc, attributes, UserPattern(), meta)
        owned = {parameter.InstanceId for parameter in (outputs if output is None else (target,))}
        earlier = [pair.Item2.Id for pair in BakeContext.FindBakedObjects(doc, BakeDataState.Valid | BakeDataState.Expired | BakeDataState.Invalid) if pair.Item1.ProcessGuid in owned]
        undo = doc.BeginUndoRecord(f"Bake {target.Nomen.Name}")
        try:
            doc.Objects.Delete(earlier, quiet=True)
            written = target.BakeShapes(context, BakeUpdateMode.Add)
        finally:
            doc.EndUndoRecord(undo)
            doc.Views.Redraw()
        return read_objects(doc, written, tuple(str(object_id) for object_id in earlier))

    match bakeable, None if layer_path is None else layer_index(doc, layer_path):
        case IBakeAware() as target, int() as index:
            attributes = ObjectAttributes()
            attributes.LayerIndex = index
            return baked(target, attributes, MetaPattern(enableAll=False, embed=False))
        case IBakeAware() as target, None:
            return baked(target, None, MetaPattern(enableAll=True, embed=False))
        case failed:
            return collect_faults(*failed)


# --- [PICTURES]


def image(document: Document, name: str, margin: int = 24) -> File[int]:
    """Draw every group, wire, and object of a document at 1:1 into `<name>.png` beside `capture`'s pictures, `detail` the objects drawn."""
    skin, placed = Editor.Instance.Canvas.Skin.WithFades(None), [item for item in document.Objects.Forwards if not isinstance(item, GroupObject)]
    for item in (*placed, *document.Objects.Groups):
        item.Attributes.Layout(skin.Shape)
    shapes = (
        WireShape.Create(IParameterAttributes(document.Objects.FindParameter(source).Attributes), IParameterAttributes(parameter.Attributes)).Bounds
        for item in placed
        for parameter in _parameters(item, Side.Input)
        for source in parameter.Inputs.Forwards
    )
    bounds = reduce(RectangleF.Union, shapes, document.Objects.AttributeBounds)
    frame = RectangleF(bounds.Left - margin, bounds.Top - margin, bounds.Width + 2 * margin, bounds.Height + 2 * margin)
    bitmap = Bitmap(math.ceil(frame.Width), math.ceil(frame.Height), PixelFormat.Format32bppRgba)
    graphics = ControlGraphics(bitmap)
    try:
        graphics.Control.Clear(skin.Canvas.Background)
        graphics.Content.TranslateTransform(-frame.Left, -frame.Top)
        context = graphics.ContentContext
        for owner in document.Objects.Groups:
            owner.Attributes.Draw(context, skin)
        repository = clr.GetClrType(Editor).Assembly.GetType("Grasshopper2.UI.Canvas.WireRepository", throwOnError=True)
        signature = Array[Type]([context.GetType(), skin.GetType(), frame.GetType(), frame.GetType(), clr.GetClrType(IEnumerable[IAttributes])])
        arguments = Array[Object]([context, skin, frame, frame, Array[IAttributes]([item.Attributes for item in placed])])
        repository.GetMethod("DrawWires", signature).Invoke(Activator.CreateInstance(repository, Array[Object]([document])), arguments)
        for item in placed:
            item.Attributes.Draw(context, skin)
    finally:
        graphics.Dispose()
    path = artifacts() / f"{name}.png"
    try:
        bitmap.Save(str(path), ImageFormat.Png)
    finally:
        bitmap.Dispose()
    return File(str(path), path.stat().st_size, document.Objects.Count)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Data", "Node", "Part", "Plugin", "Slider", "Stage", "Wire", "arrange", "assign", "bake", "build", "cluster", "definition", "delete", "graph", "group", "image", "plugins", "show", "wire"]
