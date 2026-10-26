"""Record schema walks and paths, entry inputs over record fields, and graph values and holds stored as JSON beside owned data."""

from collections.abc import Callable, Iterator, Mapping
import functools
from functools import reduce
from operator import or_
import typing
from typing import Final

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import msgspec.inspect as native

from blender.bpy.data.rna.descriptors import Descriptor, struct_at, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.changes import owned
from blender.bpy.nodes.interface import addresses, item_key, MenuItems, slot
from blender.bpy.settings.properties import RecordGroup, Unit
from blender.bpy.settings.shapes import unwrapped, vector
from blender.imaging.chain import Domain, Effect
from blender.imaging.faults import Access, accumulated, ImagingFileFault, paired
from blender.imaging.parameters import Case, Choice, ColorMode, Gated, GraphForm, HuePath, Interpolation, Knot, markers, Presentation, Ramp, Rgb, Stop

# --- [TYPES] ----------------------------------------------------------------------------

type KeyPath = tuple[Key | int | type[Case], ...]

# --- [MODELS] ---------------------------------------------------------------------------


class Plot(msgspec.Struct, frozen=True, kw_only=True):
    """Record curve placed in its editor's native axes, the shown height `scale * y + slope * x + offset`."""

    scale: float = 1.0
    slope: float = 0.0
    offset: float = 0.0

    def shown(self, x: float, y: float) -> float:
        """Returns editor height of record point."""
        return self.scale * y + self.slope * x + self.offset

    def held(self, x: float, y: float) -> float:
        """Returns record height of editor point."""
        return (y - self.slope * x - self.offset) / self.scale


class CurveEditor(msgspec.Struct, frozen=True, kw_only=True):
    """Native curve node holding knot fields of a record as its curves in field order, drawn as one editor with channel tabs.

    `plots` place each field's curve in the node's own axes, and `view` is the node's own view in place of the record bounds.
    """

    key: Key
    node: type[bpy.types.Node]
    fields: tuple[str, ...]
    plots: tuple[Plot, ...] = ()
    view: Option[tuple[float, float, float, float]] = Nothing

    def plot(self, field: object) -> Plot:
        """Returns placement of field's curve, identity when the editor declares none."""
        return self.plots[self.fields.index(field)] if self.plots else Plot()


class Field(msgspec.Struct, frozen=True, kw_only=True):
    """One record member at its key path with its metadata-free shape."""

    path: KeyPath
    info: msgspec.structs.FieldInfo
    shape: native.Type

    @property
    def key(self) -> Key:
        """Interface item key of the path."""
        return item_key(*self.path)

    @property
    def forms(self) -> tuple[GraphForm, ...]:
        """Graph forms the annotation declares."""
        return markers(self.info.type, GraphForm)

    @property
    def graphed(self) -> bool:
        """Whether the owned graph holds the value: a graph form, a ramp, or a struct collection."""
        match self.shape:
            case native.StructType(cls=record):
                return bool(self.forms) or (typing.get_origin(record) or record) is Ramp
            case native.VarTupleType(item_type=native.StructType()):
                return True
            case _:
                return bool(self.forms)

    @property
    def menu(self) -> Option[MenuItems]:
        """Menu items of a choice or case union field the graph does not own."""
        match self.shape:
            case native.EnumType(cls=choices) if issubclass(choices, Choice) and not self.forms:
                return Some(MenuItems.of(choices))
            case native.UnionType(types=cases) if not self.forms:
                return Some(MenuItems.of(tuple(case.cls for case in cases if isinstance(case, native.StructType) and issubclass(case.cls, Case))))
            case _:
                return Nothing

    @property
    def collection(self) -> Option[type[msgspec.Struct]]:
        """Item record of a struct collection other than knots."""
        match self.shape:
            case native.VarTupleType(item_type=native.StructType(cls=item)) if (typing.get_origin(item) or item) is not Knot:
                return Some(item)
            case _:
                return Nothing

    @classmethod
    def of(cls, record: type[msgspec.Struct], *, value: Option[msgspec.Struct] = Nothing, prefix: KeyPath = (), counts: Mapping[Key, int] = {}) -> tuple["Field", ...]:
        """Walks record fields in declaration order, case alternatives and collection items reading indices from values or counts.

        Args:
            record: Record type walked.
            value: Record whose collections and active cases the walk follows.
            prefix: Path the walk starts under.
            counts: Collection lengths by item key, read when no value is given.

        Returns:
            Fields in walk order, gated values under their own path and the gate itself left out.
        """

        def member(info: msgspec.structs.FieldInfo, shape: native.Type, at: KeyPath, selected: Option[object]) -> Iterator[Field]:
            yield cls(path=at, info=info, shape=shape)
            if markers(info.type, GraphForm):
                return
            match shape:
                case native.TupleType() as content if not vector(content):
                    for index, component in enumerate(RecordGroup.components(msgspec.structs.replace(info, default=selected.default_value(msgspec.NODEFAULT))), 1):
                        yield from member(component, unwrapped(native.type_info(component.type)), (*at, index), Some(component.default) if component.default is not msgspec.NODEFAULT else Nothing)
                case native.StructType(cls=nested) if (typing.get_origin(nested) or nested) not in {Rgb, Ramp, Knot}:
                    yield from walked(nested, at, selected)
                case native.UnionType(types=cases):
                    for alternative in cases:
                        if isinstance(alternative, native.StructType) and issubclass(alternative.cls, Case):
                            yield from walked(alternative.cls, (*at, alternative.cls), selected.filter(lambda held: isinstance(held, alternative.cls)))
                case native.VarTupleType(item_type=native.StructType(cls=item)) if (typing.get_origin(item) or item) is not Knot:
                    match selected.to_optional():
                        case tuple() | list() as values:
                            for index, entry in enumerate(values, 1):
                                yield from walked(item, (*at, index), Some(entry))
                        case None:
                            for index in range(1, counts.get(item_key(*at), 0) + 1):
                                yield from walked(item, (*at, index), Nothing)
                        case _:
                            pass
                case _:
                    pass

        def walked(kind: type[msgspec.Struct], path: KeyPath, held: Option[object]) -> Iterator[Field]:
            for info, shape in _members(kind):
                yield from member(info, shape, (*path, info.name), held.map(lambda owner: getattr(owner, info.name)))

        return tuple(walked(record, prefix, value))


class JsonSlot[T](msgspec.Struct, frozen=True, kw_only=True):
    """Typed JSON text in an ID property under the extension's attachment name, read as `{}` while absent."""

    key: Key
    form: type[T]

    def read(self, identity: Identity, holder: bpy.types.ID) -> BlenderResult[T]:
        """Decodes the slot of holder."""
        return identity.name(Name.ATTACHMENT, self.key).bind(
            lambda name: catch(lambda: msgspec.json.decode(holder.get(name, b"{}"), type=self.form), exception=msgspec.DecodeError)().map_error(
                lambda error: (ImagingFileFault(access=Access.READ, request=repr(holder), error=error),)
            )
        )

    def written(self, identity: Identity, holder: bpy.types.ID, value: T) -> BlenderResult[bool]:
        """Writes value as deterministic JSON text when it differs from the value the slot reads, tagging the holder for its evaluated copies, answering whether it wrote."""
        text = msgspec.json.encode(value, order="deterministic")
        return paired(self.read(identity, holder), identity.name(Name.ATTACHMENT, self.key)).bind(
            lambda held: (
                Ok(False)
                if msgspec.json.encode(held[0], order="deterministic") == text
                else BlenderEditFault
                .attempted(repr(holder), lambda: holder.__setitem__(held[1], text.decode()))
                .bind(lambda _: BlenderEditFault.attempted(repr(holder), holder.update_tag))
                .map(lambda _: True)
            )
        )


# --- [CONSTANTS] ------------------------------------------------------------------------

HELD: Final = JsonSlot(key="held", form=dict[Key, Snapshot])
VALUES: Final = JsonSlot(key="values", form=dict[Key, object])

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PATHS]


def reached(value: msgspec.Struct, path: KeyPath) -> object:
    """Reads value at record path, skipping case type parts."""
    held: object = value
    for part in path:
        if isinstance(part, int) and isinstance(held, tuple | list):
            held = held[part - 1]
        elif isinstance(part, str):
            held = getattr(held, part)
    return held


def active(value: msgspec.Struct, path: KeyPath) -> bool:
    """Answers whether record holds every case its path names."""
    return all(not isinstance(part, type) or isinstance(reached(value, path[:index]), part) for index, part in enumerate(path))


def updated[T: msgspec.Struct](value: T, path: KeyPath, change: Callable[[object], object]) -> T:
    """Replaces the value at a record field path by its change, case type parts skipped and collection items indexed from 1.

    Args:
        value: Record holding the path.
        path: Field path from the record, starting at one of its fields.
        change: New value from the held one.

    Returns:
        Record with the reached value replaced and every enclosing struct and collection rebuilt, unchanged for a path that reaches nothing.
    """
    match tuple(part for part in path if not isinstance(part, type)):
        case (str(name), *rest):
            return msgspec.structs.replace(value, **{name: _changed(getattr(value, name), tuple(rest), change)})
        case _:
            return value


# --- [ENTRY_INPUTS]


def entry_input(identity: Identity, entry: bpy.types.SceneCompositorEffect, key: Key) -> "BlenderResult[bpy.types.bpy_struct[object]]":
    """Resolves live entry input by key after interface rebuilds."""
    return (
        Result[bpy.types.NodeTree, BlenderFaults]
        .bind(
            Option.of_optional(entry.node_group).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key="node_group"),)),
            lambda group: slot(identity).map(lambda name: addresses(group, name)),
        )
        .bind(lambda keys: Option.of_optional(keys.get(key)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key=key),)))
        .bind(lambda address: interface_input(entry, address))
    )


def inputs_of(entry: bpy.types.SceneCompositorEffect) -> "BlenderResult[bpy.types.bpy_struct[object] | bpy.types.bpy_prop_collection[bpy.types.bpy_struct[object]]]":
    """Resolves the entry's generated `properties.inputs`, absent without a group."""
    return struct_at(entry, "properties.inputs").to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key="properties.inputs"),))


def interface_input(entry: bpy.types.SceneCompositorEffect, identifier: str) -> "BlenderResult[bpy.types.bpy_struct[object]]":
    """Resolves live entry input by interface socket identifier."""
    match struct_at(entry, f"properties.inputs.{identifier}").to_optional():
        case bpy.types.bpy_struct() as value:
            return Ok(value)
        case _:
            return Error((BlenderAbsentFault(owner=repr(entry), key=identifier),))


def path(identity: Identity, entry: bpy.types.SceneCompositorEffect, key: Key) -> BlenderResult[str]:
    """Returns animation data path of entry input value."""
    return entry_input(identity, entry, key).map(lambda value: value.path_from_id("value"))


def inputs[T: msgspec.Struct](
    identity: Identity, entry: bpy.types.SceneCompositorEffect, prefix: KeyPath, record: type[T], graphs: Mapping[Key, object] = {}, *, baseline: Option[T] = Nothing
) -> BlenderResult[T]:
    """Reads record from graph values and group inputs or from baseline, a collection spanning the baseline's items when one is given."""
    graph_paths = frozenset(field.path for field in Field.of(record, value=baseline, prefix=prefix) if field.graphed)

    def socket(at: KeyPath) -> BlenderResult[Value]:
        return entry_input(identity, entry, item_key(*at)).bind(lambda value: Descriptor.resolve(value, "value", bpy.types.Property).map(lambda descriptor: descriptor.read(value)))

    def read(at: KeyPath, info: msgspec.structs.FieldInfo, shape: native.Type, annotation: object) -> BlenderResult[object]:
        key = item_key(*at)
        match shape, baseline:
            case _, _ if key in graphs and (
                markers(info.type, GraphForm)
                or not isinstance(shape, native.VarTupleType)
                or (isinstance(shape.item_type, native.StructType) and (typing.get_origin(shape.item_type.cls) or shape.item_type.cls) is Knot)
            ):
                return Ok(graphs[key])
            case _, Option(tag="some", some=held) if not any(graphed[: len(at)] == at for graphed in graph_paths):
                return Ok(msgspec.to_builtins(reached(held, at[len(prefix) :])))
            case native.StructType(cls=kind, fields=(_, value)), _ if (typing.get_origin(kind) or kind) is Gated:
                enabled_field, value_field = Gated.__struct_fields__
                _, declared = msgspec.structs.fields(kind)
                selected = annotation if markers(annotation, Presentation) else declared.type
                enabled = baseline.map(lambda held: Ok(reached(held, (*at[len(prefix) :], enabled_field)))).default_with(lambda: socket(at))
                return paired(enabled, read((*at, value.name), declared, unwrapped(value.type), selected)).map(lambda pair: {enabled_field: pair[0], value_field: pair[1]})
            case native.StructType(cls=kind), _ if (typing.get_origin(kind) or kind) is Rgb:
                return entry_input(identity, entry, key).map(lambda value: value.value[:3])  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            case native.StructType(cls=kind), _:
                return lowered(kind, at)
            case native.UnionType(types=cases), Option(tag="some", some=held):
                chosen = type(reached(held, at[len(prefix) :]))
                kind, branch = next((case.cls, case) for case in cases if isinstance(case, native.StructType) and issubclass(case.cls, Case) and case.cls is chosen)
                return lowered(kind, (*at, kind)).map(lambda values: values | ({branch.tag_field: branch.tag} if branch.tag_field is not None else {}))
            case native.UnionType(types=cases), _:
                alternatives: tuple[tuple[type[Case], native.StructType], ...] = tuple((case.cls, case) for case in cases if isinstance(case, native.StructType) and issubclass(case.cls, Case))

                def selected_case(label: object) -> BlenderResult[object]:
                    found: BlenderResult[tuple[type[Case], native.StructType]] = Option.of_optional(next((case for case in alternatives if case[0].title == label), None)).to_result_with(
                        lambda: (BlenderChoiceFault(path=key, identifier=str(label), accepted=tuple(kind.title for kind, _ in alternatives)),)
                    )
                    return Result[tuple[type[Case], native.StructType], BlenderFaults].bind(
                        found, lambda case: lowered(case[0], (*at, case[0])).map(lambda values: values | ({case[1].tag_field: case[1].tag} if case[1].tag_field is not None else {}))
                    )

                return socket(at).bind(selected_case)
            case native.EnumType(cls=choices), _ if issubclass(choices, Choice):
                members = tuple((choice.label, choice.value) for choice in choices)
                return socket(at).bind(
                    lambda label: Option.of_optional(next((value for title, value in members if title == label), None)).to_result_with(
                        lambda: (BlenderChoiceFault(path=key, identifier=str(label), accepted=tuple(title for title, _ in members)),)
                    )
                )
            case native.SetType(item_type=native.EnumType(cls=choices)) | native.FrozenSetType(item_type=native.EnumType(cls=choices)), _:
                return accumulated(socket((*at, choice.name)).map(lambda chosen: (choice, chosen)) for choice in choices).map(
                    lambda values: tuple(choice.value for choice, chosen in values if chosen is True)
                )
            case native.FloatType(), _:
                return socket(at).bind(lambda value: Unit.of(annotation, shape).restored(value, shape))
            case native.TupleType(), _ if vector(shape):
                return socket(at).bind(lambda value: Unit.of(annotation, shape).restored(value, shape))
            case native.TupleType(), _:
                return accumulated(read((*at, index), component, unwrapped(native.type_info(component.type)), component.type) for index, component in enumerate(RecordGroup.components(info), 1))
            case native.VarTupleType(item_type=native.StructType() as item), _:
                match baseline.map(lambda held: reached(held, at[len(prefix) :])).default_with(lambda: graphs.get(key)):
                    case tuple() | list() as collection:
                        return accumulated(lowered(item.cls, (*at, index)) for index in range(1, len(collection) + 1))
                    case None:
                        return Error((BlenderAbsentFault(owner=repr(entry), key=key),))
                    case malformed:
                        return Error((ImagingFileFault(access=Access.READ, request=key, error=TypeError(repr(malformed))),))
            case _:
                return socket(at)

    def lowered(kind: type[msgspec.Struct], at: KeyPath) -> BlenderResult[dict[str, object]]:
        return accumulated(read((*at, info.name), info, shape, info.type).map(lambda value: (info.encode_name, value)) for info, shape in _members(kind)).map(dict)

    return lowered(record, prefix).bind(
        lambda values: catch(lambda: msgspec.convert(values, record), exception=msgspec.ValidationError)().map_error(
            lambda error: (ImagingFileFault(access=Access.READ, request=repr(entry), error=error),)
        )
    )


def entered(identity: Identity, entry: bpy.types.SceneCompositorEffect, prefix: KeyPath, record: msgspec.Struct) -> BlenderResult[bool]:
    """Writes inputs differing from record outside graph fields, answering whether any changed."""
    fields = Field.of(type(record), value=Some(record), prefix=prefix)
    presented = frozenset(
        field.path for field in fields if isinstance(field.shape, native.StructType) and (typing.get_origin(field.shape.cls) or field.shape.cls) is Gated and markers(field.info.type, Presentation)
    )
    annotations = {field.path: field.info.type for field in fields}

    def write(at: KeyPath, shape: native.Type, annotation: object, value: object) -> BlenderResult[bool]:
        def socket(held: Value) -> BlenderResult[bool]:
            return entry_input(identity, entry, item_key(*at)).bind(lambda item: written(item, "value", held)).map(Option.is_some)

        match value, shape:
            case Gated(), _:
                return socket(value.enabled)
            case Rgb(), _:
                return socket((*msgspec.structs.astuple(value), 1.0))
            case Case(), native.UnionType():
                return socket(type(value).title)
            case Choice(), _:
                return socket(value.label)
            case set() | frozenset(), native.SetType(item_type=native.EnumType(cls=choices)) | native.FrozenSetType(item_type=native.EnumType(cls=choices)):
                return accumulated(entry_input(identity, entry, item_key(*at, choice.name)).bind(lambda item: written(item, "value", choice in value)).map(Option.is_some) for choice in choices).map(
                    any
                )
            case _, native.TupleType() if not vector(shape):
                return Ok(False)
            case _, native.FloatType() | native.TupleType():
                return Unit.of(annotation, shape).stored(value, shape).bind(socket)
            case _, native.StructType() | native.VarTupleType():
                return Ok(False)
            case _:
                return socket(msgspec.to_builtins(value))

    return accumulated(
        write(field.path, field.shape, annotations[field.path[:-1]] if field.path[:-1] in presented else field.info.type, reached(record, field.path[len(prefix) :]))
        for field in fields
        if active(record, field.path[len(prefix) :]) and not field.forms
    ).map(any)


# --- [GRAPH_VALUES]


def graph_values(
    identity: Identity,
    entry: bpy.types.SceneCompositorEffect,
    records: tuple[tuple[KeyPath, type[msgspec.Struct], Option[msgspec.Struct], Domain, tuple[CurveEditor, ...]], ...],
    *,
    edited: bool = True,
) -> BlenderResult[dict[Key, object]]:
    """Overlays ramp and curve edits on stored or given graph values, knots read from the curve of a shared editor or the field node."""

    def values(group: bpy.types.NodeTree, held: dict[Key, object]) -> BlenderResult[dict[Key, object]]:
        marked = owned(group, identity)
        supplied = {
            item_key(*prefix, *field.path): msgspec.to_builtins(reached(record, field.path))
            for prefix, kind, value, _, _ in records
            for record in value.to_list()
            for field in Field.of(kind, value=value)
            if field.graphed and active(record, field.path)
        }
        answer = held | supplied
        counts = {key: len(value) for key, value in answer.items() if isinstance(value, tuple | list)}
        fields = tuple(
            (
                field,
                domain,
                Option.of_optional(
                    next(((item_key(*prefix, editor.key), editor.fields.index(field.path[-1]), editor.plot(field.path[-1])) for editor in editors if field.path[-1] in editor.fields), None)
                ),
            )
            for prefix, kind, value, domain, editors in records
            for field in Field.of(kind, value=value, prefix=prefix, counts=counts)
            if field.graphed
        )

        def overlaid(restored: dict[Key, tuple[str, Ramp]]) -> dict[Key, object]:
            answer.update({key: msgspec.to_builtins(value) for key, (_, value) in restored.items()})
            for field, _domain, shared in fields:
                if not edited or (field.key in restored and restored[field.key][0] != bpy.data.colorspace.working_space):
                    continue
                node, curve, plot = shared.map(lambda editor: (marked.get(editor[0]), editor[1], editor[2])).default_with(lambda: (marked.get(field.key), 0, Plot()))
                match field.shape, node:
                    case native.StructType(cls=record), bpy.types.ShaderNodeValToRGB() as node if record is Ramp:
                        ramp = node.color_ramp
                        answer[field.key] = msgspec.to_builtins(
                            Ramp(
                                stops=tuple(Stop(position=element.position, color=Rgb(red=element.color[0], green=element.color[1], blue=element.color[2])) for element in ramp.elements),  # type: ignore[index, union-attr]  # ty: ignore[unresolved-attribute]
                                interpolation=Interpolation[ramp.interpolation],  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                                mode=ColorMode[ramp.color_mode],  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                                hue_path=HuePath[ramp.hue_interpolation],  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                            )
                        )
                    case (
                        native.VarTupleType(item_type=native.StructType(cls=record)),
                        bpy.types.ShaderNodeFloatCurve() | bpy.types.CompositorNodeCurveRGB() | bpy.types.CompositorNodeHueCorrect() as held,
                    ) if (typing.get_origin(record) or record) is Knot:
                        answer[field.key] = tuple((point.location.x, plot.held(point.location.x, point.location.y)) for point in held.mapping.curves[curve].points)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            return answer

        return (
            accumulated(
                catch(lambda: msgspec.convert(held[field.key], tuple[str, Ramp]), exception=msgspec.ValidationError)()
                .map_error(lambda error: (ImagingFileFault(access=Access.READ, request=field.key, error=error),))
                .map(lambda value: (field.key, value))
                for field, domain, _ in fields
                if isinstance(field.shape, native.StructType) and field.shape.cls is Ramp and domain is Domain.DISPLAY and field.key in held and field.key not in supplied
            )
            .map(dict)
            .map(overlaid)
        )

    return Result[bpy.types.NodeTree, BlenderFaults].bind(
        Option.of_optional(entry.node_group).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key="node_group"),)),
        lambda group: (VALUES.read(identity, group) if any(value.is_none() for _, _, value, _, _ in records) else Ok({})).bind(lambda held: values(group, held)),
    )


def stored_graphs(identity: Identity, group: bpy.types.NodeTree, records: tuple[tuple[KeyPath, msgspec.Struct, Domain], ...]) -> BlenderResult[bool]:
    """Writes active graph values, keeping inactive case values, answering whether the store changed.

    Scene-domain ramps the group holds a node for live in that node alone; display-domain ramps keep the working space they were stored in.

    Returns:
        `Ok` of whether the store changed. `Error` of slot read and write faults.
    """

    def kept(held: dict[Key, object]) -> dict[Key, object]:
        nodes = owned(group, identity)
        live = {
            item_key(*prefix, *field.path): (reached(record, field.path), domain)
            for prefix, record, domain in records
            for field in Field.of(type(record), value=Some(record))
            if field.graphed and active(record, field.path)
        }
        return {key: value for key, value in held.items() if key not in live} | {
            key: (bpy.data.colorspace.working_space, msgspec.to_builtins(value)) if isinstance(value, Ramp) and domain is Domain.DISPLAY else msgspec.to_builtins(value)
            for key, (value, domain) in live.items()
            if not isinstance(value, Ramp) or domain is Domain.DISPLAY or key not in nodes
        }

    return VALUES.read(identity, group).bind(lambda held: VALUES.written(identity, group, kept(held)))


def case_values(record: Effect, tree: Option[bpy.types.NodeTree], identity: Identity) -> BlenderResult[dict[KeyPath, tuple[Case, ...]]]:
    """Builds active case and complete stored alternatives per union path, nested unions yielding held case."""

    def made(values: Mapping[Key, object]) -> BlenderResult[dict[KeyPath, tuple[Case, ...]]]:
        def case(at: KeyPath, current: object, alternative: type[Case], fields: tuple[native.Field, ...]) -> Option[BlenderResult[Case]]:
            given = {field.encode_name: values[key] for field in fields for key in (item_key(*at, alternative, field.name),) if key in values}
            return (
                Some(Ok(current))
                if isinstance(current, alternative)
                else Nothing
                if any(field.required and field.encode_name not in given for field in fields)
                else Some(
                    catch(lambda: msgspec.convert(given, alternative), exception=msgspec.ValidationError)().map_error(
                        lambda error: (ImagingFileFault(access=Access.READ, request=item_key(*at, alternative), error=error),)
                    )
                )
            )

        def unions(owner: msgspec.Struct, prefix: KeyPath, *, alternatives: bool) -> BlenderResult[dict[KeyPath, tuple[Case, ...]]]:
            return accumulated(
                accumulated(
                    cased
                    for branch in field.shape.types
                    if isinstance(branch, native.StructType) and issubclass(branch.cls, Case) and (alternatives or isinstance(current, branch.cls))
                    for cased in case(field.path, current, branch.cls, branch.fields).to_list()
                ).bind(
                    lambda cases: accumulated(unions(held, (*field.path, type(held)), alternatives=alternatives and held is current) for held in cases).map(
                        lambda inner: reduce(or_, inner, {field.path: cases})
                    )
                )
                for field in Field.of(type(owner), value=Some(owner), prefix=prefix)
                if isinstance(field.shape, native.UnionType) and not any(isinstance(part, type) for part in field.path[len(prefix) :])
                for current in (reached(owner, field.path[len(prefix) :]),)
            ).map(lambda found: reduce(or_, found, {}))

        return unions(record, (), alternatives=True)

    return tree.map(lambda group: VALUES.read(identity, group).bind(made)).default_with(lambda: made({}))


@functools.cache
def _members(record: type[msgspec.Struct]) -> tuple[tuple[msgspec.structs.FieldInfo, native.Type], ...]:
    """Field infos and metadata-free shapes of one record type, a gated record's gate left out."""
    fields = msgspec.structs.fields(record)
    gated = (typing.get_origin(record) or record) is Gated
    return tuple((info, unwrapped(shape)) for info, shape in zip(fields, native.multi_type_info(info.type for info in fields)) if not (gated and info.name == item_key(Gated.enabled)))


def _changed(held: object, path: tuple[Key | int, ...], change: Callable[[object], object]) -> object:
    """Rebuilds held along the remaining path, its end replaced by change, held unchanged where the path reaches nothing."""
    match path, held:
        case (), _:
            return change(held)
        case (int(index), *rest), tuple() | list():
            return (*held[: index - 1], _changed(held[index - 1], tuple(rest), change), *held[index:])
        case (str(name), *rest), msgspec.Struct():
            return msgspec.structs.replace(held, **{name: _changed(getattr(held, name), tuple(rest), change)})
        case _:
            return held


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "HELD",
    "VALUES",
    "CurveEditor",
    "Field",
    "JsonSlot",
    "KeyPath",
    "Plot",
    "active",
    "case_values",
    "entered",
    "entry_input",
    "graph_values",
    "inputs",
    "inputs_of",
    "interface_input",
    "path",
    "reached",
    "stored_graphs",
    "updated",
]
