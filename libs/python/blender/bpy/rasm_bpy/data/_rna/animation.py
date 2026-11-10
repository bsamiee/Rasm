"""RNA channel keyframes, drivers, modifiers, and sampling."""

from collections.abc import Callable, Iterable, Mapping
from functools import partial
from types import MappingProxyType
from typing import Final, TYPE_CHECKING

import bpy
from bpy_extras import anim_utils
from expression import Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
from numpy.typing import NDArray

from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._rna.descriptors import Descriptor, differing, DTYPES, Value, written
from rasm_bpy.data._rna.paths import id_data, id_path, rna, RnaPath, Struct, struct_at
from rasm_bpy.data._rna.snapshots import assigned
from rasm_records import accumulated, Case, Error, Fault, Nothing, Ok, Outcome, paired, Reason, typed

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Curves = bpy.types.ActionChannelbagFCurves | bpy.types.AnimDataDrivers

# --- [CONSTANTS] ------------------------------------------------------------------------

_CO: Final = Descriptor.of(bpy.types.Keyframe, ("co",), bpy.types.FloatProperty).prop

# --- [MODELS] ---------------------------------------------------------------------------


class Keys(Case, frozen=True, kw_only=True):
    """Keyframes as frame and value pairs."""

    points: tuple[tuple[float, float], ...]

    @staticmethod
    def curves(data: bpy.types.AnimData) -> Option[_Curves]:
        """Returns F-curves of the slot data animates, Nothing before an action and slot hold one."""
        return Option.of_optional(anim_utils.animdata_get_channelbag_for_assigned_slot(data)).map(lambda bag: bag.fcurves)


class Driven(Case, frozen=True, kw_only=True):
    """Driver members and one member mapping per variable, each written through the snapshot writer.

    Values name `Driver` members (`type`, `expression`), variable mappings `DriverVariable` members and targets (`("targets", 0, "id")`).
    """

    values: Mapping[RnaPath, Value] = MappingProxyType({})
    variables: tuple[Mapping[RnaPath, Value], ...] = ()

    @staticmethod
    def curves(data: bpy.types.AnimData) -> Option[_Curves]:
        """Returns driver F-curves of data."""
        return Some(data.drivers)


class Modifier(msgspec.Struct, frozen=True, kw_only=True):
    """F-curve modifier type and property values."""

    kind: "rna_enums.FmodifierTypeItems"
    values: Mapping[RnaPath, Value] = MappingProxyType({})


class Animation(msgspec.Struct, frozen=True, kw_only=True):
    """Desired animation on one scalar member or array element under an ID, the element's coordinates ending its path."""

    # --- [STATE]

    path: RnaPath
    source: Keys | Driven
    modifiers: tuple[Modifier, ...] = ()

    # --- [RESOLUTION]

    def channel(self, block: bpy.types.ID) -> Outcome[tuple[Struct, Descriptor]]:
        """Returns the struct holding the animatable member and its descriptor, an array member taking one element alone."""
        match Descriptor.located(block, self.path):
            case Result(tag="error", error=faults):
                return Error(faults)
            case Result(ok=(struct, descriptor)) if (
                not isinstance(struct, bpy.types.bpy_prop_collection) and descriptor.prop.is_animatable and bool(descriptor.coordinates) == bool(Descriptor.dimensions(descriptor.prop))
            ):
                return Ok((struct, descriptor))
        return Error(Fault.of(Reason.ABSENT, repr(block), member=rna(self.path)))

    # --- [CHANNELS]

    def apply(self, block: bpy.types.ID) -> Outcome[bool]:
        """Reconciles the channel on block, an existing F-curve or driver kept and a missing one added at the host's `index=-1` for a scalar member.

        Args:
            block: ID the channel's owner sits under.

        Returns:
            Whether a key, driver, mute, or modifier value changed, refused faults of every host call accumulated.
        """
        refused = partial(Fault.of, Refusal.REFUSED, f"{block!r}.{rna(self.path)}")

        def keyed(points: NDArray[np.float32], curve: bpy.types.FCurve) -> Outcome[bool]:
            held = np.empty(len(curve.keyframe_points) * 2, np.float32)
            curve.keyframe_points.foreach_get(_CO.identifier, held)
            if np.array_equal(held.reshape(-1, 2), points):
                return Ok(value=False)
            curve.keyframe_points.clear()
            curve.keyframe_points.add(len(points))
            curve.keyframe_points.foreach_set(_CO.identifier, points.ravel())
            curve.update_autoflags(block)
            curve.update()
            return Ok(value=True)

        def driven(source: Driven, curve: bpy.types.FCurve) -> Outcome[bool]:
            def variables(driver: bpy.types.Driver) -> Outcome[bool]:
                if len(driver.variables) == len(source.variables) and not any(map(differing, driver.variables, source.variables)):
                    return Ok(value=False)
                for held in tuple(driver.variables):
                    driver.variables.remove(held)
                return accumulated(assigned(driver.variables.new(), row) for row in source.variables).map(lambda _: True)

            def drove(driver: bpy.types.Driver) -> Outcome[bool]:
                changed = bool(differing(driver, source.values))
                return assigned(driver, source.values).bind(lambda _: variables(driver)).map(lambda wrote: wrote or changed)

            return Option.of_optional(curve.driver).to_result_with(partial(Fault.of, Reason.ABSENT, repr(curve), member="driver")).bind(drove)

        def modified(curve: bpy.types.FCurve) -> Outcome[bool]:
            held = tuple(curve.modifiers)
            if len(held) == len(self.modifiers) and all(modifier.type == row.kind and not differing(modifier, row.values) for modifier, row in zip(held, self.modifiers)):
                return Ok(value=False)

            def added(row: Modifier) -> Outcome[None]:
                return (
                    catch(curve.modifiers.new, exception=TypeError)(row.kind)
                    .map_error(partial(Fault.of, Refusal.REFUSED, repr(curve.modifiers)))
                    .bind(lambda made: Option.of_optional(made).to_result_with(partial(Fault.of, Reason.ABSENT, repr(curve.modifiers), member=row.kind)))
                    .bind(lambda made: assigned(made, row.values))
                )

            return (
                accumulated(catch(curve.modifiers.remove, exception=RuntimeError)(modifier).map_error(partial(Fault.of, Refusal.REFUSED, repr(curve.modifiers))) for modifier in held)
                .bind(lambda _: accumulated(map(added, self.modifiers)))
                .map(lambda _: True)
            )

        def keyframed(data: bpy.types.AnimData, _struct: Struct, descriptor: Descriptor) -> Outcome[bpy.types.FCurve]:
            return (
                Option
                .of_optional(data.action)
                .map(Ok)
                .default_with(
                    lambda: catch(bpy.data.actions.new, exception=RuntimeError)(f"{block.name}Action").map_error(refused).bind(lambda action: written(data, ("action",), action).map(lambda _: action))
                )
                .bind(lambda action: catch(action.fcurve_ensure_for_datablock, exception=RuntimeError)(block, rna(descriptor.member), index=descriptor.index.default_value(-1)).map_error(refused))
            )

        def driver(_data: bpy.types.AnimData, struct: Struct, descriptor: Descriptor) -> Outcome[bpy.types.FCurve]:
            return (
                catch(struct.driver_add, exception=TypeError)(descriptor.prop.identifier, descriptor.index.default_value(-1))
                .map_error(refused)
                .bind(partial(typed, bpy.types.FCurve, rna(descriptor.path)))
            )

        content: Outcome[Callable[[bpy.types.FCurve], Outcome[bool]]]
        curve_of: Callable[[bpy.types.AnimData, Struct, Descriptor], Outcome[bpy.types.FCurve]]
        match self.source:
            case Keys(points=points):
                content, curve_of = Descriptor.quantized(_CO, np.asarray(points, dtype=np.float64).reshape(-1, 2)).map(lambda held: partial(keyed, held)), keyframed
            case Driven() as source:
                content, curve_of = Ok(partial(driven, source)), driver

        return paired(self.channel(block), content).bind(
            lambda found: (
                Option
                .of_optional(block.animation_data_create())
                .to_result_with(partial(Fault.of, Reason.ABSENT, repr(block), member="animation_data"))
                .bind(lambda data: curve_of(data, *found[0]))
                .bind(lambda curve: accumulated((found[1](curve), written(curve, ("mute",), value=False).map(Option.is_some), modified(curve))).map(any))
            )
        )

    def clear(self, block: bpy.types.ID) -> Outcome[None]:
        """Removes the channel's F-curve or driver."""
        return self.channel(block).bind(
            lambda found: accumulated(
                catch(curves.remove, exception=RuntimeError)(curve).map_error(partial(Fault.of, Refusal.REFUSED, repr(curves)))
                for curves in struct_at(block, ("animation_data",), bpy.types.AnimData).bind(self.source.curves).to_list()
                for curve in Option.of_optional(curves.find(rna(found[1].member), index=found[1].index.default_value(0))).to_list()
            ).map(lambda _: None)
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def sampled(owner: Struct, path: RnaPath, frames: Iterable[float], proposed: Option[float] = Nothing) -> Outcome[tuple[float, ...]]:
    """Samples the keyed channel of a member or element at frames, repeating the proposed or stored value when unkeyed.

    Args:
        owner: Struct the path starts from.
        path: RNA path to a scalar member or an array element.
        frames: Frames to sample.
        proposed: Value repeated for an unkeyed member, its stored value when Nothing.

    Returns:
        One value per frame, absent fault for a path naming no number or enum scalar.
    """
    held = tuple(frames)
    absent = partial(Fault.of, Reason.ABSENT, repr(owner), member=rna(path))

    def projected(prop: bpy.types.Property, values: NDArray[np.float32]) -> Outcome[tuple[float, ...]]:
        match prop:
            case bpy.types.BoolProperty():
                return Ok(tuple(map(float, values > (np.float32(1) - np.spacing(np.float32(1))))))
            case bpy.types.IntProperty():
                return Ok(tuple(map(float, values.astype(np.int64).clip(prop.hard_min, prop.hard_max))))
            case bpy.types.FloatProperty():
                return Ok(tuple(map(float, values.clip(prop.hard_min, prop.hard_max))))
            case bpy.types.EnumProperty():
                return Ok(tuple(map(float, values.astype(np.int64))))
            case _:
                return Error(Fault.of(Reason.OUTSIDE, rna(path), type(prop).__name__, member=", ".join(kind.__name__ for kind in DTYPES)))

    def curve(descriptor: Descriptor) -> Option[bpy.types.FCurve]:
        return (
            id_data(bpy.types.ID, owner)
            .bind(lambda block: struct_at(block, ("animation_data",), bpy.types.AnimData))
            .bind(Keys.curves)
            .bind(lambda curves: id_path(owner).bind(lambda base: Option.of_optional(curves.find(rna((*base, *descriptor.member)), index=descriptor.index.default_value(0)))))
        )

    def stored(descriptor: Descriptor) -> Outcome[float]:
        match descriptor.prop:
            case bpy.types.EnumProperty(is_enum_flag=False, enum_items=items):
                return Option.of_optional(items.get(str(descriptor.read(owner)))).map(lambda item: float(item.value)).to_result_with(absent)
            case bpy.types.BoolProperty() | bpy.types.IntProperty() | bpy.types.FloatProperty():
                return Ok(float(np.asarray(descriptor.read(owner), np.float64).item()))
            case _:
                return Error(absent())

    def sample(descriptor: Descriptor) -> Outcome[tuple[float, ...]]:
        return (
            curve(descriptor)
            .map(lambda keyed: projected(descriptor.prop, np.asarray(tuple(map(keyed.evaluate, held)), dtype=np.float32)))
            .default_with(lambda: proposed.map(Ok).default_with(lambda: stored(descriptor)).map(lambda value: tuple(value for _ in held)))
        )

    return (
        Descriptor
        .resolve(owner, path, bpy.types.Property)
        .filter_with(lambda descriptor: bool(descriptor.coordinates) == bool(Descriptor.dimensions(descriptor.prop)), lambda _: absent())
        .bind(sample)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Animation", "Driven", "Keys", "Modifier", "sampled"]
