# ty: ignore[no-matching-overload, too-many-positional-arguments, unresolved-import, unsupported-operator]
# mypy: disable-error-code="call-arg, call-overload, import-not-found, import-untyped, no-any-return, no-any-unimported, operator, type-abstract"
# /// script
# dependencies = ["msgspec"]
#
# [tool.ty.environment]
# extra-paths = ["."]
#
# [tool.ty.rules]
# all = "error"
# dynamic-function-decorator-return = "ignore"
# unsound-assignment = "ignore"
# unsound-return-statement = "ignore"
# ///
"""Grasshopper 2 task documents held outside the editor, with builds, layout, values, solves, bakes, clusters, plugins, and pictures, imported after `g2_start`."""

from collections import Counter
from collections.abc import Iterable, Mapping, Sequence
from functools import cache, reduce
from itertools import accumulate, chain, groupby, islice
import math
from pathlib import Path

import clr
from document import artifacts, layer_index, Objects, read_objects
from Eto.Drawing import Bitmap, ImageFormat, PixelFormat, PointF, RectangleF
from Grasshopper2 import Folders
from Grasshopper2.Bake import BakeContext, BakeDataState, BakeUpdateMode, IBakeAware, MetaPattern, UserPattern
from Grasshopper2.Components import Component, Side
from Grasshopper2.Data import Modifiers
from Grasshopper2.Doc import Document, DocumentIO, DocumentState, FileContents, GraphTopology, IAttributes, IDocumentObject, IParameterAttributes, ObjectActivity, SolutionPhase
from Grasshopper2.Framework import FailureKind, ObjectProxies, ObjectProxy, PluginRequirement, PluginRequirements, PluginServer
from Grasshopper2.Parameters import Connections, IParameter, IPin, Pins
from Grasshopper2.Parameters.Special import NumberSliderObject, TextInputObject, ToggleObject, ValueListObject, ValueObject
from Grasshopper2.Parameters.Standard import AbsoluteTolerancePin, UnitSystem, UnitSystemPin
from Grasshopper2.SpecialObjects import GroupObject
from Grasshopper2.Types.Conversion import ConversionServer
from Grasshopper2.UI import Editor, UiNumber
from Grasshopper2.UI.Canvas import WireShape
from Grasshopper2.UI.Flex import ControlGraphics
from Grasshopper2.UI.Skinning import Fades
from Grasshopper2.Undo import ActionList
import msgspec
from records import collect_faults, Fault, Faults, File, Record, Resolved
from Rhino import RhinoDoc
from Rhino.DocObjects import ObjectAttributes
from Rhino.Geometry import Box, Brep, Circle, Curve, GeometryBase, Interval, Line, Mesh, Plane, Point3d, Rectangle3d, Vector3d
from Rhino.Runtime.Code.Languages import LanguageSpec
from RhinoCodePlatform.GH import IScriptParameter
from RhinoCodePlatform.GH.Context import IScriptObject
from ScriptComponents.Components import BaseScriptComponent
from ScriptComponents.Parameters import ConsoleOutParameter
import scriptcontext
from System import Activator, Array, Convert, Enum, Guid, Object, TimeSpan
from System.Reflection import BindingFlags

# --- [TYPES] ----------------------------------------------------------------------------

type Value = str | float | bool | tuple[float, ...] | dict[str, object]
type Item = float | bool | str | GeometryBase
type Modifier = str | tuple[str, int]
type Port = str | int
type Member = Part | Slider

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
    """Document object as the last solve left it, each input's sources as `(input, owner id, port)`, `user_name` None with no user name, `seconds` None with no measured solve."""

    id: str
    name: str
    user_name: str | None
    bounds: tuple[float, float, float, float]
    messages: tuple[str, ...] = ()
    inputs: tuple[str, ...] = ()
    sources: tuple[tuple[str, str, str], ...] = ()
    outputs: tuple[Data, ...] = ()
    members: tuple[str, ...] = ()
    disabled: bool = False
    seconds: float | None = None


class Graph(Record, frozen=True):
    """Every document object with the phase of the document's latest solve."""

    phase: SolutionPhase
    nodes: tuple[Node, ...]


class Plugin(Record, frozen=True):
    """Third-party Grasshopper 2 plugin with its components by `chapter/section`, or a library that failed to load."""

    id: str | None
    name: str
    location: str
    components: dict[str, tuple[str, ...]]
    failure: FailureKind | None = None
    reason: str | None = None


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
    """Component or parameter part by proxy guid, a value list with `items`, or a script component with `script`, `name` the user name of a lone output outside a script."""

    key: str
    selector: str
    name: str | None = None
    values: dict[str, tuple[Item, ...]] = msgspec.field(default_factory=dict)
    modifiers: dict[str, tuple[Modifier, ...]] = msgspec.field(default_factory=dict)
    items: tuple[tuple[str, str], ...] = ()
    selected: int = 0
    script: str | None = None
    outputs: tuple[str, ...] = ()


class Group(Record, frozen=True):
    """Named group of parts in an Open Color family."""

    name: str
    color: str
    parts: tuple[Member, ...]


class Wire(Record, frozen=True):
    """Wire from a source output to a target input, ends named by part key or canvas id."""

    source: str
    output: Port
    target: str
    input: Port


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [RESOLUTION]


def _partition[K, V](results: Mapping[K, Resolved[V]]) -> tuple[dict[K, V], Faults | None]:
    """Split independent results into their values and every fault among them."""
    return {key: result for key, result in results.items() if not isinstance(result, Faults)}, collect_faults(*results.values())


def _find(document: Document, canvas_id: str) -> Resolved[IDocumentObject]:
    """Return the document object `canvas_id` names."""
    parsed, guid = Guid.TryParse(canvas_id)
    found = document.Objects.Find(guid) if parsed else None
    return Faults.of(Fault(IDocumentObject, canvas_id)) if found is None else found


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
    """Return the name a build wires a port by, a script parameter's variable name or another port's name."""
    match parameter:
        case IScriptParameter():
            return IScriptParameter(parameter).VariableName
        case _:
            return parameter.Nomen.Name


def _port(parameters: Sequence[IParameter], port: Port) -> Resolved[IParameter]:
    """Return the parameter `port` names by name, user name, or index, `""` naming a lone parameter."""
    named = {key: parameter for parameter in parameters for key in (_name(parameter), parameter.UserName) if key} | ({"": parameters[0]} if len(parameters) == 1 else {})
    match port:
        case int() if 0 <= port < len(parameters):
            return parameters[port]
        case str() if port in named:
            return named[port]
        case _:
            return Faults.of(Fault(IParameter, port, tuple(named)))


def _end(objects: Mapping[str, Resolved[IDocumentObject]], key: str, port: Port, side: Side) -> Resolved[IParameter | None]:
    """Return a wire end on the object `key` names, a fault for an unknown key, and `None` for an object that faulted on its own."""
    match objects.get(key):
        case None:
            return Faults.of(Fault(Wire, key, tuple(objects)))
        case Faults():
            return None
        case found:
            return _port(_parameters(found, side), port)


def _ends(objects: Mapping[str, Resolved[IDocumentObject]], wires: Sequence[Wire]) -> tuple[tuple[tuple[Resolved[IParameter | None], Resolved[IParameter | None]], ...], Faults | None]:
    """Return each wire's source output and target input beside every fault among the ends."""
    ends = tuple((_end(objects, connection.source, connection.output, Side.Output), _end(objects, connection.target, connection.input, Side.Input)) for connection in wires)
    return ends, collect_faults(*(end for pair in ends for end in pair))


def _converted(parameter: IParameter, items: Sequence[Item]) -> Resolved[object]:
    """Return values as an array of a typed port's type through Grasshopper 2 conversions, or as a list for an untyped port."""
    match parameter.TypeAssistantWeak:
        case None:
            return list(items)
        case assistant:
            converted = [(item, *ConversionServer.Convert(item, assistant.Type)) for item in items]
            if faults := collect_faults(*(Fault(ConversionServer, item, (assistant.Type.Name,)) for item, held, _ in converted if not held)):
                return faults
            array = Array.CreateInstance(assistant.Type, len(converted))
            for index, (_, _, value) in enumerate(converted):
                array.SetValue(value, index)
            return array


def _source(document: Document, source_id: object) -> tuple[str, str]:
    """Return the owner id and port name of a wire source, a source the document lacks as its id alone."""
    found = document.Objects.FindParameter(source_id)
    return (str(source_id), "") if found is None else (str((found.ParentObject or found).InstanceId), _name(found))


def _family(color: str) -> Resolved[object]:
    """Return the Open Color family `color` names in the type `GroupObject.GroupColour` holds."""
    family = clr.GetClrType(GroupObject).GetProperty("GroupColour").PropertyType
    names = tuple(Enum.GetNames(family))
    return Enum.Parse(family, color) if color in names else Faults.of(Fault(GroupObject, color, names))


def _modifiers(modifiers: Sequence[Modifier]) -> Resolved[Modifiers]:
    """Chain `With<Name>` modifiers from an empty set, `(name, depth)` as `With<Name>(depth)`."""
    accepted = tuple(sorted({method.Name.removeprefix("With") for method in clr.GetClrType(Modifiers).GetMethods() if method.Name.startswith("With") and not method.Name.startswith("Without")}))
    steps = [(modifier,) if isinstance(modifier, str) else modifier for modifier in modifiers]
    unknown = collect_faults(*(Fault(Modifiers, name, accepted) for name, *_ in steps if name not in accepted))

    def applied(modifier_set: Modifiers, step: tuple[str] | tuple[str, int]) -> Modifiers:
        name, *arguments = step
        return getattr(modifier_set, f"With{name}")(*arguments)

    return unknown or reduce(applied, steps, Modifiers.Empty)


# --- [PARTS]


def _group(document: Document, doc: RhinoDoc, name: str, family: object, members: Sequence[IDocumentObject]) -> Resolved[GroupObject]:
    """Add a named group of `members`, one holding a component pinned to `doc`'s unit system and absolute tolerance, or a fault per repeated member."""
    created = GroupObject()
    created.GroupColour, created.UserName = family, name
    if refused := collect_faults(*(Fault(GroupObject, str(member.InstanceId)) for member in members if not created.AddContent(member.InstanceId))):
        return refused
    document.Objects.Add(created, PointF(0.0, 0.0))
    units, tolerance = UnitSystemPin(), AbsoluteTolerancePin()
    units.Set([UnitSystem(doc)])
    tolerance.Set([doc.ModelAbsoluteTolerance])
    for index, pin in enumerate((units, tolerance) if any(isinstance(member, Component) for member in members) else ()):
        document.Objects.Add(pin, PointF(0.0, 0.0))
        Pins.Pin(pin, created, index, None)
    return created


def _relaid(owner: GroupObject) -> GroupObject:
    """Recompute a group's bounds around its current members in the editor skin's shape."""
    owner.Attributes.InvalidateLayout()
    owner.Attributes.Layout(Editor.Instance.Canvas.Skin.Shape)
    return owner


# --- [READS]


def _describe(item: object) -> Value:
    """Return an item as a JSON value, geometry as its coordinates and measures."""

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
        document_object.UserName,
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


def definition(path: str | None = None) -> Resolved[Document]:
    """Return the editor's document holding `path`, else the task document `scriptcontext.sticky` holds, else `path` opened or created and held there, or the current canvas for `None`."""
    editor = Editor.Instance
    match editor, path:
        case None, _:
            return Faults.of(Fault(Editor, None))
        case _, None:
            return editor.Canvas.Document
        case _, str() if (index := editor.Documents.Index(path)) >= 0:
            return editor.Documents[index].Root
        case _, str() if isinstance(held := scriptcontext.sticky.get(path), Document):
            return held
        case _, str() if Path(path).exists():
            if missing := collect_faults(*(Fault(PluginRequirement, str(requirement.Id), (requirement.Name, str(requirement.Version))) for requirement in PluginRequirements.FromFile(path).Missing)):
                return missing
            reader = DocumentIO(trackFiles=False, reportErrors=False, resolvePlugins=False)
            reader.Open(path)
            opened = reader.Document
            opened.State = DocumentState.Inactive
        case _, str():
            Path(path).parent.mkdir(parents=True, exist_ok=True)
            opened = Document.NewInactiveDocument()
            DocumentIO(opened, trackFiles=False, reportErrors=False).Save(path, FileContents.Small)
    scriptcontext.sticky[path] = opened
    return opened


def close(document: Document) -> Resolved[str]:
    """Close a task document `definition` holds with its autosave deleted and unsaved edits discarded, and return its file."""
    if not (keys := [key for key, held in scriptcontext.sticky.items() if isinstance(held, Document) and held.Equals(document)]):
        return Faults.of(Fault(Document, document.File.Path))
    for key in keys:
        del scriptcontext.sticky[key]
    document.File.DeleteAutoSaveFile()
    path = document.File.Path
    document.Close()
    return path


# --- [READS]


def graph(document: Document, sample: int = 3) -> Graph:
    """Describe every object of a document after its latest solve."""
    return Graph(document.Solution.State.Phase, tuple(_node(document_object, sample) for document_object in document.Objects.Forwards))


def plugins() -> tuple[Plugin, ...]:
    """Load Grasshopper 2 libraries installed since the editor started and list every third-party plugin and failed library."""
    PluginServer.ScopeYakPlugins()
    PluginServer.LoadAllScopedPlugins()
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
            if not Path(location).is_relative_to(Folders.PluginFolder)
        ),
        *(
            Plugin(None, Path(location).stem, location, {}, failure.Kind, failure.Reason if failure.Exception is None else f"{failure.Reason} {failure.Exception.GetBaseException().Message}")
            for location, failure in ((pair.Item1, pair.Item2) for pair in PluginServer.State.Failures)
        ),
    )


# --- [EDITS]


def build(document: Document, doc: RhinoDoc, groups: Sequence[Group], wires: Sequence[Wire] = ()) -> Resolved[dict[str, str]]:
    """Add groups of parts laid out by flow right of the existing objects, with values, modifiers, and wires to parts or existing canvas ids, and map each key to its canvas id, or return every fault."""
    all_parts = [part for entry in groups for part in entry.parts]
    parts = {part.key: part for part in all_parts}
    repeated = collect_faults(*(Fault(Part, key) for key, count in Counter(part.key for part in all_parts).items() if count > 1))
    fields = {(key, name): part for key, part in parts.items() if isinstance(part, Part) for name in dict.fromkeys((*part.values, *part.modifiers))}
    existing = {end: _find(document, end) for connection in wires for end in (connection.source, connection.target) if end not in parts}

    def scripted(emitted: IDocumentObject, part: Part, source: str) -> Resolved[IDocumentObject]:
        if not isinstance(emitted, BaseScriptComponent):
            return Faults.of(Fault(BaseScriptComponent, part.selector))
        component = type(emitted.__implementation__).Create(part.key, source)
        component.Context.EnforceParamsOnCreate = False
        component.Context.InitLanguages(document, component.Context.GetLanguageSpec())
        component.MarshalInputs = component.MarshalOutputs = component.MarshalGuids = LanguageSpec.Python.Matches(component.Context.GetLanguageSpec())
        document.Objects.Add(component, PointF(0.0, 0.0))
        IScriptObject(component).ParamsCollect()
        for parameter in [parameter for parameter in component.Parameters.Outputs if not isinstance(parameter, ConsoleOutParameter)] if part.outputs else ():
            component.Parameters.RemoveOutput(parameter, None)
        for name in part.outputs:
            component.DoCreateParameter(Side.Output, component.Parameters.OutputCount, None)
            component.Parameters.Output(component.Parameters.OutputCount - 1).VariableName = name
        return component

    def listed(emitted: IDocumentObject, part: Part) -> Resolved[IDocumentObject]:
        if not isinstance(emitted, ValueListObject):
            return Faults.of(Fault(ValueListObject, part.selector))
        document.Objects.Add(emitted, PointF(0.0, 0.0))
        setter = clr.GetClrType(ValueListObject).GetMethod("Set", BindingFlags.Instance | BindingFlags.NonPublic)
        kind = setter.GetParameters()[0].ParameterType.GetElementType()
        items = Array.CreateInstance(kind, len(part.items))
        for index, (name, text) in enumerate(part.items):
            items.SetValue(Activator.CreateInstance(kind, Array[Object]([name, text, index == part.selected, None])), index)
        setter.Invoke(emitted, Array[Object]([items, False]))
        return emitted

    def created(part: Member) -> Resolved[IDocumentObject]:
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
                return Faults.of(Fault(ObjectProxy, selector))

    made = {key: created(part) for key, part in parts.items()}
    objects, object_faults = _partition(made)
    _, existing_faults = _partition(existing)
    families, family_faults = _partition(dict(enumerate(_family(entry.color) for entry in groups)))
    ports, port_faults = _partition({field: end for field in fields if (end := _end(made, *field, Side.Input)) is not None})
    chains, chain_faults = _partition({(key, name): _modifiers(part.modifiers[name]) for (key, name), part in fields.items() if name in part.modifiers})
    persistent, value_faults = _partition({(key, name): _converted(ports[key, name], part.values[name]) for (key, name), part in fields.items() if name in part.values and (key, name) in ports})
    ends, end_faults = _ends(made | existing, wires)
    labels, label_faults = _partition({
        key: Faults.of(Fault(IScriptParameter, part.name, part.outputs))
        if part.script is not None
        else _port([output for output in _parameters(objects[key], Side.Output) if not isinstance(output, ConsoleOutParameter)], "")
        for key, part in parts.items()
        if isinstance(part, Part) and part.name is not None and key in objects
    })
    if faults := collect_faults(repeated, object_faults, existing_faults, family_faults, port_faults, value_faults, chain_faults, end_faults, label_faults):
        document.Methods.DeleteObjects(Array[IDocumentObject]([item for item in objects.values() if item.Document is not None]), None, ActionList.Empty)
        return faults
    for item in (item for item in objects.values() if item.Document is None):
        document.Objects.Add(item, PointF(0.0, 0.0))
    for key, output in labels.items():
        output.UserName = parts[key].name
    for field, items in persistent.items():
        ports[field].Set(items)
    for field, modifier_set in chains.items():
        ports[field].Modifiers = modifier_set
    for source, target in ends:
        Connections.Connect(source, target, None)
    grouped = [_group(document, doc, entry.name, families[index], [objects[part.key] for part in entry.parts]) for index, entry in enumerate(groups)]
    ids = {key: str(item.InstanceId) for key, item in objects.items()}
    arrange(document, tuple(ids.values()))
    return collect_faults(*grouped) or ids


def wire(document: Document, wires: Sequence[Wire], *, replace: bool = True) -> Resolved[tuple[Node, ...]]:
    """Join existing objects by canvas id, `replace` dropping each target input's earlier sources, and describe each target."""
    found = {canvas_id: _find(document, canvas_id) for connection in wires for canvas_id in (connection.source, connection.target)}
    objects, object_faults = _partition(found)
    ends, end_faults = _ends(found, wires)
    if faults := collect_faults(object_faults, end_faults):
        return faults
    for _, target in ends if replace else ():
        Connections.DisconnectAllInputs(target, None)
    for source, target in ends:
        Connections.Connect(source, target, None)
    return tuple(_node(objects[canvas_id], 0) for canvas_id in dict.fromkeys(connection.target for connection in wires))


def assign(document: Document, canvas_id: str, values: Sequence[Item] | None = None, input_name: str | None = None, modifiers: Sequence[Modifier] | None = None) -> Resolved[Data]:
    """Set a value source or an unwired input and its modifiers, `()` clearing and `None` keeping, expire it for the next solve, and return the values it holds."""
    found = _find(document, canvas_id)
    target = found if input_name is None or isinstance(found, Faults) else _port(_parameters(found, Side.Input), input_name)
    modifier_set = None if modifiers is None else _modifiers(modifiers)
    if isinstance(target, Faults) or isinstance(modifier_set, Faults):
        return Faults.of(target, modifier_set)
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
                return Faults.of(Fault(ValueObject, target.Text, (target.ErrorMessage,)))
            held = (target.Text,)
        case TextInputObject(), [*_]:
            target.Contents = "\n".join(map(str, values))
            held = tuple(target.Values)
        case IParameter(), [*_] if target.Inputs.Count:
            return Faults.of(Fault(IParameter, _name(target)))
        case IParameter(), ():
            target.PersistentDataWeak = None
            held = ()
        case IParameter(), [*_]:
            match _converted(target, values):
                case Faults() as refused:
                    return refused
                case items:
                    target.Set(items)
            held = tuple(target.PersistentDataWeak.NonNullItems)
        case IParameter(), None:
            held = () if target.PersistentDataWeak is None else tuple(target.PersistentDataWeak.NonNullItems)
        case _:
            return Faults.of(Fault(IParameter, target.Nomen.Name))
    if modifier_set is not None:
        target.Modifiers = modifier_set
    target.Expire()
    return Data(_name(target), int(bool(held)), len(held), tuple(_describe(value) for value in held))


def delete(document: Document, ids: Sequence[str]) -> Resolved[int]:
    """Delete objects with their wires and return the count."""
    found = [_find(document, canvas_id) for canvas_id in ids]
    return collect_faults(*found) or document.Methods.DeleteObjects(Array[IDocumentObject](found), None, None)


def cluster(document: Document, ids: Sequence[str]) -> Resolved[Node]:
    """Collapse objects into one cluster that keeps their boundary wires and takes their place in each group, refused with the topology of an empty or concave set."""
    found = [_find(document, canvas_id) for canvas_id in ids]
    if faults := collect_faults(*found):
        return faults
    members, connectivity = Array[IDocumentObject](found), document.Objects.Connectivity
    founding = {member.FoundingObject.InstanceId for member in members}
    if (
        topology := GraphTopology.Concave
        if ({node.Id for member in founding for node in connectivity.FindAllOutputs(member)} & {node.Id for member in founding for node in connectivity.FindAllInputs(member)}) - founding
        else connectivity.SubsetTopology(Array[Guid]([*founding]))
    ) in {GraphTopology.Empty, GraphTopology.Concave}:
        return Faults.of(Fault(Document, tuple(ids), (topology,)))
    created, placed = document.Methods.ClusterObjects(members, None), {member.InstanceId for member in members}
    shared = {owner: common for owner in document.Objects.Groups if (common := placed.intersection(owner.ContentIds))}
    for owner, gone in shared.items():
        for member in gone:
            owner.RemoveContent(member)
        owner.AddContent(created.InstanceId)
        _relaid(owner)
    return _node(created, 0)


def arrange(document: Document, ids: Sequence[str] = (), gap: float = 60.0) -> Graph:
    """Lay objects `ids` names in their order, or every object, out in columns by wire depth, a column fed past the previous one below it, groups sharing a member as one block, blocks left to right by the longest chain feeding them, a subset right of the rest, and describe the document."""
    placed = {str(item.InstanceId): item for item in document.Objects.Forwards if not isinstance(item, (GroupObject, IPin))}
    objects = {key: placed[key] for key in dict.fromkeys(ids or placed) if key in placed}
    for item in objects.values():
        item.Attributes.Layout(Editor.Instance.Canvas.Skin.Shape)
    groups = [(owner, members) for owner in document.Objects.Groups if (members := frozenset(map(str, owner.ContentIds)) & objects.keys())]
    moved = objects.keys() | {str(item.InstanceId) for owner, _ in groups for item in (owner, *owner.Pins.AboveAndBelow)}
    others = [item.Attributes.Bounds for item in document.Objects.Forwards if str(item.InstanceId) not in moved]
    origin = reduce(RectangleF.Union, others) if others else RectangleF(-gap, 0.0, 0.0, 0.0)
    merged = reduce(
        lambda held, members: [*(block for block in held if not block & members), members.union(*(block for block in held if block & members))],
        (members for _, members in groups),
        list[frozenset[str]](),
    )
    block_of = {key: key for key in objects} | {key: min(block) for block in merged for key in block}
    blocks = {block: tuple(key for key in objects if block_of[key] == block) for block in dict.fromkeys(block_of.values())}

    order = {key: index for index, key in enumerate(objects)}
    feeds = tuple(
        (feed, key, port) for key in objects for port, parameter in enumerate(_parameters(objects[key], Side.Input)) for source in parameter.Inputs.Forwards for feed, _ in (_source(document, source),)
    )
    links = tuple((feed, target, port) for feed, target, port in feeds if feed in objects)

    @cache
    def depth(key: str) -> int:
        return max((depth(feed) + 1 for feed, target, _ in links if target == key), default=0)

    edges = {(block_of[feed], block_of[target]) for feed, target, _ in links if block_of[feed] != block_of[target]}
    start = reduce(lambda held, _: {block: max((held[source] + 1 for source, target in edges if target == block), default=0) for block in blocks}, blocks, dict.fromkeys(blocks, 0))

    def rank(key: str) -> tuple[int, tuple[int, ...], int]:
        consumers = ((start[block_of[target]], depth(target), order[target], port) for feed, target, port in links if feed == key)
        return depth(key), min(consumers, default=(len(blocks),)), order[key]

    def move(keys: Iterable[str], left: float, top: float, frame: RectangleF) -> None:
        for key in keys:
            objects[key].Attributes.Move(left - frame.Left, top - frame.Top)

    def pack(keys: tuple[str, ...]) -> RectangleF:
        columns = [tuple(column) for _, column in groupby(sorted(keys, key=rank), key=depth)]
        widths = [max(objects[key].Attributes.Bounds.Width for key in column) for column in columns]
        heights = [sum(objects[key].Attributes.Bounds.Height + gap for key in column) for column in columns]
        crossed = [index > 0 and any(feed not in columns[index - 1] for feed, target, _ in feeds if target in column) for index, column in enumerate(columns)]
        tops = accumulate(range(1, len(columns)), lambda top, index: top + heights[index - 1] if crossed[index] else 0.0, initial=0.0)
        for left, first, column in zip(accumulate((width + gap for width in widths[:-1]), initial=0.0), tops, columns, strict=True):
            for top, key in zip(accumulate((objects[key].Attributes.Bounds.Height + gap for key in column[:-1]), initial=first), column, strict=True):
                move((key,), left, top, objects[key].Attributes.Bounds)
        owned = [_relaid(owner) for owner, members in groups if members.intersection(keys)]
        return reduce(RectangleF.Union, (item.Attributes.Bounds for item in (*(objects[key] for key in keys), *owned, *(pin for owner in owned for pin in owner.Pins.AboveAndBelow))))

    packed = {block: pack(keys) for block, keys in blocks.items()}
    levels = [tuple(level) for _, level in groupby(sorted(blocks, key=start.__getitem__), key=start.__getitem__)]
    for left, level in zip(accumulate((max(packed[block].Width for block in column) + gap for column in levels[:-1]), initial=origin.Right + gap), levels, strict=True):
        for top, block in zip(accumulate((packed[block].Height + gap for block in level[:-1]), initial=origin.Top), level, strict=True):
            move(blocks[block], left, top, packed[block])
    for owner, _ in groups:
        _relaid(owner)
    return graph(document, 0)


def bake(doc: RhinoDoc, document: Document, canvas_id: str, layer_path: str | None = None, output: str | None = None) -> Resolved[Objects]:
    """Bake a solved object or one output in one undo step that deletes its earlier bake, onto `layer_path` or by each item's `Rhino.*` meta."""
    found = _find(document, canvas_id)
    outputs = _parameters(found, Side.Output)
    source = found if output is None or isinstance(found, Faults) else _port(outputs, output)
    bakeable = source if isinstance(source, Faults) or (isinstance(source, IBakeAware) and source.BakeCapable) else Faults.of(Fault(IBakeAware, source.Nomen.Name))

    def baked(target: IBakeAware, attributes: ObjectAttributes | None, meta: MetaPattern) -> Resolved[Objects]:
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
            return Faults.of(*failed)


# --- [PICTURES]


def image(document: Document, name: str, ids: Sequence[str] = (), margin: float = 24.0) -> Resolved[File[float]]:
    """Draw objects `ids` names with their groups, or every object, and the wires between them into `.artifacts/rhino/<name>.png` at up to 2 pixels per canvas unit inside 2000 pixels, `detail` the scale."""
    found = [_find(document, canvas_id) for canvas_id in ids]
    if faults := collect_faults(*found):
        return faults
    chosen = set(ids) or {str(item.InstanceId) for item in document.Objects.Forwards}
    groups = [owner for owner in document.Objects.Groups if str(owner.InstanceId) in chosen or chosen.intersection(map(str, owner.ContentIds))]
    members = chosen.union(*(map(str, owner.ContentIds) for owner in groups), *((str(pin.InstanceId) for pin in owner.Pins.AboveAndBelow) for owner in groups))
    if not (placed := [item for item in document.Objects.Forwards if str(item.InstanceId) in members and not isinstance(item, GroupObject)]):
        return Faults.of(Fault(Document, name))
    skin = Editor.Instance.Canvas.Skin.WithFades(Fades.Normal)
    for item in (*placed, *groups):
        item.Attributes.Layout(skin.Shape)
    wires = (
        WireShape.Create(IParameterAttributes(output.Attributes), IParameterAttributes(parameter.Attributes)).Bounds
        for item in placed
        for parameter in _parameters(item, Side.Input)
        for source in parameter.Inputs.Forwards
        if (output := document.Objects.FindParameter(source)) is not None and str((output.ParentObject or output).InstanceId) in members
    )
    bounds = reduce(RectangleF.Union, chain((item.Attributes.Bounds for item in (*placed, *groups)), wires))
    frame = RectangleF(bounds.Left - margin, bounds.Top - margin, bounds.Width + 2 * margin, bounds.Height + 2 * margin)
    scale = min(2.0, 2000.0 / max(frame.Width, frame.Height))
    bitmap = Bitmap(math.floor(frame.Width * scale), math.floor(frame.Height * scale), PixelFormat.Format32bppRgba)
    graphics = ControlGraphics(bitmap)
    try:
        graphics.Control.Clear(skin.Canvas.Foreground)
        content = graphics.Content
        content.ScaleTransform(scale, scale)
        content.TranslateTransform(-frame.Left, -frame.Top)
        context = graphics.ContentContext
        for owner in groups:
            owner.Attributes.Draw(context, skin)
        repository = Activator.CreateInstance(clr.GetClrType(WireShape).Assembly.GetType("Grasshopper2.UI.Canvas.WireRepository", throwOnError=True), Array[Object]([document]))
        repository.DrawWires(context, skin, frame, frame, Array[IAttributes]([item.Attributes for item in placed]))
        for item in placed:
            item.Attributes.Draw(context, skin)
    finally:
        graphics.Dispose()
    path = artifacts() / f"{name}.png"
    try:
        bitmap.Save(str(path), ImageFormat.Png)
    finally:
        bitmap.Dispose()
    return File(str(path), path.stat().st_size, scale)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Data", "Graph", "Group", "Node", "Part", "Plugin", "Slider", "Wire", "arrange", "assign", "bake", "build", "close", "cluster", "definition", "delete", "graph", "image", "plugins", "wire"]
