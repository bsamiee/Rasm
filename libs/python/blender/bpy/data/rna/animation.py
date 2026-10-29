"""RNA channel keyframes, drivers, modifiers, and sampling."""

from collections.abc import Iterable, Mapping
from functools import partial
from types import MappingProxyType
from typing import Final, TYPE_CHECKING

import bpy
from bpy_extras import anim_utils
from expression import Error, Nothing, Ok, Option, Some, try_downcast
from expression.extra.result import catch
import msgspec
import numpy as np
from numpy.typing import NDArray

from blender.bpy.data.rna.descriptors import Descriptor, differing, Value, written
from blender.bpy.data.rna.paths import id_path, rna, RnaPath, Struct, struct_at
from blender.bpy.data.rna.snapshots import assigned
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
_CO: Final = Descriptor.declared(bpy.types.Keyframe, ("co",), bpy.types.FloatProperty)

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

    Driver values name `Driver` members (`type`, `expression`, `use_self`); each variable mapping names `DriverVariable` members and its targets (`("type",)`, `("targets", 0, "id")`, `("targets", 0, "transform_type")`), so any variable type, target ID, and combining driver type is reachable.
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

    path: RnaPath
    source: _Source
    modifiers: tuple[Modifier, ...] = ()

    # --- [RESOLUTION]

    def channel(self, block: bpy.types.ID) -> Outcome[tuple[Struct, Descriptor]]:
        """Returns the struct holding the animatable member and its descriptor, an array member taking one element alone."""
        return Descriptor.located(block, self.path).filter_with(
            lambda found: found[1].prop.is_animatable and bool(found[1].coordinates) == bool(Descriptor.dimensions(found[1].prop)),
            lambda _: Fault.of(BlenderReason.ABSENT, repr(block), member=rna(self.path)),
        )

    # --- [CHANNELS]

    def apply(self, block: bpy.types.ID) -> Outcome[bool]:
        """Reconciles the channel on block, an existing F-curve or driver kept and a missing one added at the host's `index=-1` for a scalar member.

        Args:
            block: ID the channel's owner sits under.

        Returns:
            Whether a key, driver, mute, or modifier value changed, refusal faults of every host call accumulated.
        """
        refused = partial(Fault.of, BlenderReason.REFUSED, f"{block!r}.{rna(self.path)}")

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
                if len(driver.variables) == len(source.variables) and not any(map(differing, driver.variables, source.variables)):
                    return Ok(False)
                for held in tuple(driver.variables):
                    driver.variables.remove(held)
                return accumulated(assigned(driver.variables.new(), row) for row in source.variables).map(lambda _: True)

            def drove(driver: bpy.types.Driver) -> Outcome[bool]:
                changed = bool(differing(driver, source.values))
                return assigned(driver, source.values).bind(lambda _: variables(driver)).map(lambda wrote: wrote or changed)

            return Option.of_optional(curve.driver).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(curve), member="driver")).bind(drove)

        def modified(curve: bpy.types.FCurve) -> Outcome[bool]:
            held = tuple(curve.modifiers)
            if len(held) == len(self.modifiers) and all(modifier.type == row.kind and not differing(modifier, row.values) for modifier, row in zip(held, self.modifiers)):
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

        def curve_of(data: bpy.types.AnimData, struct: Struct, descriptor: Descriptor) -> Outcome[bpy.types.FCurve]:
            text, index = rna(descriptor.member), descriptor.index.default_value(-1)
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
                        Option
                        .of_optional(data.drivers.find(text, index=descriptor.index.default_value(0)))
                        .map(Ok)
                        .default_with(
                            lambda: (
                                catch(struct.driver_add, exception=TypeError)(descriptor.prop.identifier, index)
                                .map_error(refused)
                                .bind(lambda made: Option.of_optional(try_downcast(bpy.types.FCurve, made)).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(struct), member="driver")))
                            )
                        )
                    )

        def reconciled(curve: bpy.types.FCurve, source: NDArray[np.float32] | Driven) -> Outcome[bool]:
            content = driven(curve, source) if isinstance(source, Driven) else keyed(curve, source)
            return accumulated((content, written(curve, ("mute",), False).map(Option.is_some), modified(curve))).map(any)

        def prepared() -> Outcome[NDArray[np.float32] | Driven]:
            match self.source:
                case Keys(points=points):
                    return Descriptor.quantized(_CO.prop, np.asarray(points, dtype=np.float64))
                case Driven() as source:
                    return Ok(source)

        return paired(self.channel(block), prepared()).bind(
            lambda found: (
                Option
                .of_optional(block.animation_data_create())
                .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(block), member="animation_data"))
                .bind(lambda data: curve_of(data, *found[0]).bind(lambda curve: reconciled(curve, found[1])))
            )
        )

    def clear(self, block: bpy.types.ID) -> Outcome[None]:
        """Removes the channel's F-curve or driver."""
        return self.channel(block).bind(
            lambda found: accumulated(
                catch(curves.remove, exception=RuntimeError)(curve).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(curves)))
                for curves in _animation_data(block).bind(self.source.curves).to_list()
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
    absent = partial(Fault.of, BlenderReason.ABSENT, repr(owner), member=rna(path))

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
                return Error(Fault.of(RecordReason.OUTSIDE, rna(path), type(prop).__name__, member=", ".join(kind.__name__ for kind in _SAMPLED)))

    def curve(descriptor: Descriptor) -> Option[bpy.types.FCurve]:
        return (
            Option
            .of_optional(try_downcast(bpy.types.ID, owner.id_data))
            .bind(_animation_data)
            .bind(Keys.curves)
            .bind(lambda curves: id_path(owner).bind(lambda base: Option.of_optional(curves.find(rna((*base, *descriptor.member)), index=descriptor.index.default_value(0)))))
        )

    def stored(descriptor: Descriptor) -> Outcome[float]:
        return (
            Some(descriptor)
            .filter(lambda found: isinstance(found.prop, bpy.types.BoolProperty | bpy.types.IntProperty | bpy.types.FloatProperty))
            .map(lambda found: float(np.asarray(found.read(owner), np.float64).item()))
            .to_result_with(absent)
        )

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


def _animation_data(block: bpy.types.ID) -> Option[bpy.types.AnimData]:
    """Returns the block's animation data, Nothing for an ID kind that holds none."""
    return struct_at(block, ("animation_data",), bpy.types.AnimData)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Animation", "Driven", "Keys", "Modifier", "sampled"]
