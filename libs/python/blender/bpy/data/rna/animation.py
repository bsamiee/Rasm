"""RNA channel keyframes, drivers, modifiers, and sampling."""

from collections.abc import Iterable, Mapping
from functools import partial
from types import MappingProxyType
from typing import Annotated, TYPE_CHECKING

import bpy
from bpy_extras import anim_utils
from expression import Error, Nothing, Ok, Option, Some, try_downcast
from expression.extra.result import catch
import msgspec
import numpy as np
from numpy.typing import NDArray

from blender.bpy.data.rna.descriptors import assigned, definition, Descriptor, id_path, rna, RnaPath, Struct, struct_at, Value, written
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason
from blender.records.fields import Case

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Source = Keys | Driven
type _Curves = bpy.types.ActionChannelbagFCurves | bpy.types.AnimDataDrivers

# --- [CONSTANTS] ------------------------------------------------------------------------

_SAMPLED: tuple[type[bpy.types.Property], ...] = (bpy.types.BoolProperty, bpy.types.IntProperty, bpy.types.FloatProperty, bpy.types.EnumProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class Keys(Case, frozen=True, kw_only=True):
    """Keyframes as frame and value pairs."""

    points: tuple[tuple[float, float], ...]

    @staticmethod
    def curves(data: bpy.types.AnimData) -> Option[_Curves]:
        """Returns F-curves of the slot data animates, Nothing before an action and slot hold one."""
        return Option.of_optional(anim_utils.animdata_get_channelbag_for_assigned_slot(data)).map(lambda bag: bag.fcurves)


class Variable(msgspec.Struct, frozen=True, kw_only=True):
    """Driver variable over an animated property of the driven ID."""

    name: str
    path: RnaPath


class Driven(Case, frozen=True, kw_only=True):
    """Scripted driver over variables."""

    expression: str
    variables: tuple[Variable, ...] = ()

    @staticmethod
    def curves(data: bpy.types.AnimData) -> Option[_Curves]:
        """Returns driver F-curves of data."""
        return Some(data.drivers)


class Modifier(msgspec.Struct, frozen=True, kw_only=True):
    """F-curve modifier type and property values."""

    kind: "rna_enums.FmodifierTypeItems"
    values: Mapping[str, Value] = MappingProxyType({})


class Animation(msgspec.Struct, frozen=True, kw_only=True):
    """Desired animation on one scalar property or flat array element of a struct under an ID."""

    owner: RnaPath = ()
    member: str
    index: Annotated[int, msgspec.Meta(ge=0)] | None = None
    source: _Source
    modifiers: tuple[Modifier, ...] = ()

    # --- [RESOLUTION]

    @property
    def path(self) -> RnaPath:
        """RNA path of the channel from the ID, array index left out."""
        return (*self.owner, self.member)

    def channel(self, block: bpy.types.ID) -> Outcome[Struct]:
        """Returns the struct holding the animatable member at index."""
        absent = partial(Fault.of, BlenderReason.ABSENT, repr(block), member=rna(self.path))
        return (
            struct_at(block, self.owner)
            .bind(lambda struct: Option.of_optional(try_downcast(bpy.types.bpy_struct, struct)))
            .filter(lambda struct: Option.of_optional(definition(struct).properties.get(self.member)).filter(partial(_fits, self.index)).is_some())
            .to_result_with(absent)
        )

    # --- [CHANNELS]

    def apply(self, block: bpy.types.ID) -> Outcome[bool]:
        """Reconciles the channel on block, answering whether it wrote."""
        text, index = rna(self.path), Option.of_optional(self.index).default_value(0)
        refused = partial(Fault.of, BlenderReason.REFUSED, f"{block!r}.{text}")

        def keyed(curve: bpy.types.FCurve, points: NDArray[np.float32]) -> Outcome[bool]:
            held = np.empty(len(curve.keyframe_points) * 2, np.float32)
            curve.keyframe_points.foreach_get("co", held)
            if np.array_equal(held.reshape(-1, 2), points):
                return Ok(False)
            curve.keyframe_points.clear()
            curve.keyframe_points.add(len(points))
            curve.keyframe_points.foreach_set("co", points.ravel())
            curve.update_autoflags(block)
            curve.update()
            return Ok(True)

        def driven(curve: bpy.types.FCurve, source: Driven) -> Outcome[bool]:
            def variables(driver: bpy.types.Driver) -> Outcome[bool]:
                if len(driver.variables) == len(source.variables) and all(
                    held.type == "SINGLE_PROP" and held.name == row.name and held.targets[0].id == block and held.targets[0].id_type == block.id_type and held.targets[0].data_path == rna(row.path)
                    for held, row in zip(driver.variables, source.variables)
                ):
                    return Ok(False)
                for held in tuple(driver.variables):
                    driver.variables.remove(held)
                return accumulated(
                    accumulated((
                        written(variable, ("name",), row.name),
                        written(variable, ("targets", 0, "id_type"), block.id_type).bind(lambda _: written(variable, ("targets", 0, "id"), block)),
                        written(variable, ("targets", 0, "data_path"), rna(row.path)),
                    ))
                    for row in source.variables
                    for variable in (driver.variables.new(),)
                ).map(lambda _: True)

            return (
                Option
                .of_optional(curve.driver)
                .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(curve), member="driver"))
                .bind(
                    lambda driver: accumulated((written(driver, ("type",), "SCRIPTED"), written(driver, ("expression",), source.expression))).bind(
                        lambda prior: variables(driver).map(lambda wrote: wrote or any(held.is_some() for held in prior))
                    )
                )
            )

        def modified(curve: bpy.types.FCurve) -> Outcome[bool]:
            held = tuple(curve.modifiers)
            same = len(held) == len(self.modifiers) and all(
                modifier.type == row.kind
                and all(
                    Descriptor.resolve(modifier, (name,), bpy.types.Property).map(lambda descriptor: descriptor.equal(descriptor.read(modifier), value)).default_value(False)
                    for name, value in row.values.items()
                )
                for modifier, row in zip(held, self.modifiers)
            )
            if same:
                return Ok(False)

            def added(row: Modifier) -> Outcome[None]:
                return (
                    catch(curve.modifiers.new, exception=TypeError)(row.kind)
                    .map_error(partial(Fault.of, BlenderReason.REFUSED, repr(curve.modifiers)))
                    .bind(lambda made: Option.of_optional(made).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(curve.modifiers), member=row.kind)))
                    .bind(lambda made: assigned(made, row.values))
                )

            return (
                accumulated(catch(curve.modifiers.remove, exception=RuntimeError)(modifier).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(curve.modifiers))) for modifier in held)
                .bind(lambda _: accumulated(map(added, self.modifiers)))
                .map(lambda _: True)
            )

        def curve_of(data: bpy.types.AnimData, struct: Struct) -> Outcome[bpy.types.FCurve]:
            match self.source:
                case Keys():
                    return (
                        Option
                        .of_optional(data.action)
                        .map(Ok)
                        .default_with(
                            lambda: (
                                catch(bpy.data.actions.new, exception=RuntimeError)(f"{block.name}Action")
                                .map_error(refused)
                                .bind(lambda action: written(data, ("action",), action).map(lambda _: action))
                            )
                        )
                        .bind(lambda action: catch(action.fcurve_ensure_for_datablock, exception=RuntimeError)(block, text, index=index).map_error(refused))
                    )
                case Driven():
                    return (
                        catch(struct.driver_add, exception=TypeError)(self.member, index)
                        .map_error(refused)
                        .bind(lambda made: Option.of_optional(try_downcast(bpy.types.FCurve, made)).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(struct), member="driver")))
                    )

        def reconciled(curve: bpy.types.FCurve, source: NDArray[np.float32] | Driven) -> Outcome[bool]:
            content = driven(curve, source) if isinstance(source, Driven) else keyed(curve, source)
            return accumulated((content, written(curve, ("mute",), False).map(Option.is_some), modified(curve))).map(any)

        def prepared() -> Outcome[NDArray[np.float32] | Driven]:
            match self.source:
                case Keys(points=points):
                    return Descriptor.quantized(Descriptor.declared(bpy.types.Keyframe, ("co",), bpy.types.FloatProperty).prop, np.asarray(points, dtype=np.float64))
                case Driven(variables=variables) as source:
                    return accumulated(Descriptor.resolve(block, row.path, bpy.types.Property) for row in variables).map(lambda _: source)

        return paired(self.channel(block), prepared()).bind(
            lambda found: (
                Option
                .of_optional(block.animation_data_create())
                .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(block), member="animation_data"))
                .bind(lambda data: curve_of(data, found[0]).bind(lambda curve: reconciled(curve, found[1])))
            )
        )

    def clear(self, block: bpy.types.ID) -> Outcome[None]:
        """Removes the channel's F-curve or driver."""
        text, index = rna(self.path), Option.of_optional(self.index).default_value(0)
        return self.channel(block).bind(
            lambda _: accumulated(
                catch(curves.remove, exception=RuntimeError)(curve).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(curves)))
                for curves in _animation_data(block).bind(self.source.curves).to_list()
                for curve in Option.of_optional(curves.find(text, index=index)).to_list()
            ).map(lambda _: None)
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def sampled(owner: Struct, member: str, frames: Iterable[float], index: int = 0, proposed: Option[float] = Nothing) -> Outcome[tuple[float, ...]]:
    """Samples the keyed channel of a member at frames, repeating the proposed or stored value when unkeyed.

    Args:
        owner: Struct holding the member.
        member: Member identifier.
        frames: Frames to sample.
        index: Array element of an array member.
        proposed: Value repeated for an unkeyed member, its stored value when Nothing.

    Returns:
        One value per frame, absent fault for a member no number or enum holds.
    """
    held = tuple(frames)
    absent = partial(Fault.of, BlenderReason.ABSENT, repr(owner), member=member)

    def projected(prop: bpy.types.Property, values: NDArray[np.float32]) -> Outcome[tuple[float, ...]]:
        match prop:
            case bpy.types.BoolProperty():
                return Ok(tuple((values > (np.float32(1) - np.spacing(np.float32(1)))).astype(float).tolist()))
            case bpy.types.IntProperty():
                return Ok(tuple(values.astype(np.int64).clip(prop.hard_min, prop.hard_max).astype(float).tolist()))
            case bpy.types.FloatProperty():
                return Ok(tuple(values.clip(prop.hard_min, prop.hard_max).astype(float).tolist()))
            case bpy.types.EnumProperty():
                return Ok(tuple(values.astype(np.int64).astype(float).tolist()))
            case _:
                return Error(Fault.of(RecordReason.OUTSIDE, member, type(prop).__name__, member=", ".join(kind.__name__ for kind in _SAMPLED)))

    def keyed(curve: bpy.types.FCurve) -> Outcome[tuple[float, ...]]:
        return Descriptor.resolve(owner, (member,), bpy.types.Property).bind(lambda descriptor: projected(descriptor.prop, np.asarray(tuple(map(curve.evaluate, held)), dtype=np.float32)))

    def stored() -> Outcome[float]:
        return Descriptor.resolve(owner, (member,), bpy.types.Property).bind(
            lambda descriptor: (
                Some(descriptor)
                .filter(lambda found: isinstance(found.prop, bpy.types.BoolProperty | bpy.types.IntProperty | bpy.types.FloatProperty))
                .map(lambda found: np.asarray(found.read(owner), np.float64).ravel())
                .filter(lambda values: index < values.size)
                .map(lambda values: float(values[index]))
                .to_result_with(absent)
            )
        )

    def unkeyed() -> Outcome[tuple[float, ...]]:
        return proposed.map(Ok).default_with(stored).map(lambda value: tuple(value for _ in held))

    return (
        Option
        .of_optional(try_downcast(bpy.types.ID, owner.id_data))
        .bind(_animation_data)
        .bind(Keys.curves)
        .bind(lambda curves: id_path(owner).bind(lambda path: Option.of_optional(curves.find(rna((*path, member)), index=index))))
        .map(keyed)
        .default_with(unkeyed)
    )


def _fits(index: int | None, prop: bpy.types.Property) -> bool:
    """Whether the member animates at index, arrays taking an element and other members none."""
    match prop, index:
        case bpy.types.BoolProperty(is_array=True) | bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True) as array, int(at):
            return array.is_animatable and at < array.array_length
        case bpy.types.BoolProperty(is_array=True) | bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True), None:
            return False
        case _, None:
            return prop.is_animatable
        case _:
            return False


def _animation_data(block: bpy.types.ID) -> Option[bpy.types.AnimData]:
    """Returns the block's animation data, Nothing for an ID kind that holds none."""
    return struct_at(block, ("animation_data",)).bind(lambda data: Option.of_optional(try_downcast(bpy.types.AnimData, data)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Animation", "Driven", "Keys", "Modifier", "Variable", "sampled"]
