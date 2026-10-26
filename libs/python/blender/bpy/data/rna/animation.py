"""RNA channel keyframes, drivers, modifiers, and sampling."""

from collections.abc import Iterable
from typing import Annotated, TYPE_CHECKING

import bpy
from bpy_extras import anim_utils
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
from numpy.typing import NDArray

from blender.bpy.data.ids.store import vacant
from blender.bpy.data.rna.descriptors import Descriptor, member_path, struct_at, Value, written
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Source = Keys | Driven

# --- [MODELS] ---------------------------------------------------------------------------


class Keys(Case, frozen=True, kw_only=True, tag=1):
    """Keyframes as frame and value pairs."""

    title = "Keyframes"
    points: tuple[tuple[float, float], ...]

    @staticmethod
    def curves(data: bpy.types.AnimData) -> Option[bpy.types.ActionChannelbagFCurves]:
        """Returns F-curves of the slot data animates, Nothing before an action and slot hold one."""
        return Option.of_optional(anim_utils.animdata_get_channelbag_for_assigned_slot(data)).map(lambda bag: bag.fcurves)


class Variable(msgspec.Struct, frozen=True, kw_only=True):
    """Driver variable over animated ID property."""

    name: str
    path: str


class Driven(Case, frozen=True, kw_only=True, tag=2):
    """Scripted driver over variables."""

    title = "Driver"
    expression: str
    variables: tuple[Variable, ...] = ()

    @staticmethod
    def curves(data: bpy.types.AnimData) -> Option[bpy.types.AnimDataDrivers]:
        """Returns driver F-curves of data."""
        return Some(data.drivers)


class Modifier(msgspec.Struct, frozen=True, kw_only=True):
    """F-curve modifier type and property values."""

    kind: "rna_enums.FmodifierTypeItems"
    values: dict[str, Value] = msgspec.field(default_factory=dict)


class Animation(msgspec.Struct, frozen=True, kw_only=True):
    """Desired animation on one scalar property or flat array element."""

    struct: str = ""
    member: str
    index: Annotated[int, msgspec.Meta(ge=0)] | None = None
    source: Source
    modifiers: tuple[Modifier, ...] = ()

    # --- [RESOLUTION]

    @property
    def path(self) -> str:
        """RNA data path of channel without array index."""
        return member_path(self.struct, self.member)

    def channel(self, owner: bpy.types.ID) -> BlenderResult[bpy.types.Property]:
        """Returns animatable property at member and index."""

        def fits(prop: bpy.types.Property) -> bool:
            match prop, self.index:
                case bpy.types.BoolProperty(is_array=True) | bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True) as array, int(index):
                    return array.is_animatable and index < array.array_length
                case bpy.types.BoolProperty(is_array=True) | bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True), None:
                    return False
                case _, None:
                    return prop.is_animatable
                case _:
                    return False

        return (
            struct_at(owner, self.struct)
            .bind(lambda struct: Option.of_optional(struct.bl_rna.properties.get(self.member)))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            .filter(fits)
            .to_result_with(lambda: (BlenderAbsentFault(owner=repr(owner), key=self.path),))
        )

    # --- [CHANNELS]

    def apply(self, owner: bpy.types.ID, context: bpy.types.Context) -> BlenderResult[bool]:
        """Reconciles channel, returning whether it wrote."""
        path, index = self.path, Option.of_optional(self.index).default_value(0)

        def matching(curve: bpy.types.FCurve, source: NDArray[np.float32] | Driven) -> BlenderResult[bool]:
            coordinates = (
                len(curve.keyframe_points) == len(source) and all(bool((point == key.co).all()) for key, point in zip(curve.keyframe_points, source))
                if isinstance(source, np.ndarray)
                else len(curve.keyframe_points) == 2 and all(key.co.x == key.co.y == float(index) for index, key in enumerate(curve.keyframe_points))
            )
            keys = (
                not curve.mute
                and not curve.sampled_points
                and curve.extrapolation == ("CONSTANT" if isinstance(self.source, Keys) else "LINEAR")
                and curve.auto_smoothing == context.preferences.edit.fcurve_new_auto_smoothing  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                and coordinates
                and all(key.interpolation == "BEZIER" and key.handle_left_type == key.handle_right_type == "AUTO_CLAMPED" for key in curve.keyframe_points)
            )
            match self.source:
                case Driven(expression=expression, variables=variables):
                    driven = (
                        Option
                        .of_optional(curve.driver)
                        .filter(
                            lambda driver: (
                                driver.type == "SCRIPTED"
                                and driver.expression == expression
                                and not driver.use_self
                                and len(driver.variables) == len(variables)
                                and all(
                                    held.type == "SINGLE_PROP"
                                    and held.name == row.name
                                    and held.targets[0].id == owner
                                    and held.targets[0].id_type == owner.id_type
                                    and held.targets[0].data_path == row.path
                                    and not held.targets[0].use_fallback_value
                                    for held, row in zip(driver.variables, variables)
                                )
                            )
                        )
                        .is_some()
                    )
                case Keys():
                    driven = True
            if not keys or not driven or len(curve.modifiers) != len(self.modifiers) or any(held.type != row.kind for held, row in zip(curve.modifiers, self.modifiers)):
                return Ok(False)
            return accumulated(
                Descriptor.resolve(held, name, bpy.types.Property).map(lambda descriptor: descriptor.equal(descriptor.read(held), value))
                for held, row in zip(curve.modifiers, self.modifiers)
                for name, value in row.values.items()
            ).map(all)

        def modified(curve: bpy.types.FCurve) -> BlenderResult[None]:
            def added(row: Modifier) -> BlenderResult[None]:
                return (
                    catch(lambda: curve.modifiers.new(row.kind), exception=TypeError)()
                    .to_option()
                    .bind(Option.of_optional)
                    .map(lambda made: accumulated(written(made, name, value) for name, value in row.values.items()).map(lambda _: None))
                    .default_with(
                        lambda: Descriptor.resolve(bpy.types.FModifier, "type", bpy.types.EnumProperty).bind(
                            lambda kind: Error((BlenderChoiceFault(path=repr(curve.modifiers), identifier=row.kind, accepted=tuple(item.identifier for item in kind.prop.enum_items if item.value)),))
                        )
                    )
                )

            return accumulated(map(added, self.modifiers)).map(lambda _: None)

        def replaced(data: bpy.types.AnimData, source: NDArray[np.float32] | Driven) -> BlenderResult[bool]:
            held = self.source.curves(data).bind(lambda curves: Option.of_optional(curves.find(path, index=index)))
            request = f"{owner!r}.{path}"

            def stored() -> BlenderResult[bpy.types.ActionChannelbagFCurves | bpy.types.AnimDataDrivers]:
                return (
                    Ok(data.drivers)
                    if isinstance(source, Driven)
                    else Option
                    .of_optional(data.action)
                    .map(Ok)
                    .default_with(
                        lambda: BlenderEditFault.attempted(request, lambda: bpy.data.actions.new(vacant(f"{owner.name}Action", bpy.data.actions.keys()))).bind(
                            lambda action: written(data, "action", action).map(lambda _: action)
                        )
                    )
                    .bind(
                        lambda action: (
                            Option
                            .of_optional(data.action_slot)
                            .map(Ok)
                            .default_with(
                                lambda: BlenderEditFault.attempted(request, lambda: action.slots.new(id_type=owner.id_type, name=owner.name)).bind(
                                    lambda slot: written(data, "action_slot", slot).map(lambda _: slot)
                                )
                            )
                            .bind(lambda slot: BlenderEditFault.attempted(request, lambda: anim_utils.action_ensure_channelbag_for_slot(action, slot)))
                        )
                    )
                    .map(lambda bag: bag.fcurves)
                )

            def filled(curve: bpy.types.FCurve) -> BlenderResult[bool]:
                match source:
                    case np.ndarray() as points:
                        curve.keyframe_points.add(len(points))
                        curve.keyframe_points.foreach_set("co", points.ravel())
                        curve.update_autoflags(owner)
                        curve.update()
                        return modified(curve).map(lambda _: True)
                    case Driven(expression=expression, variables=variables):
                        curve.update_autoflags(owner)
                        return (
                            Option
                            .of_optional(curve.driver)
                            .map(
                                lambda driver: (
                                    accumulated((written(driver, "type", "SCRIPTED"), written(driver, "expression", expression)))
                                    .bind(
                                        lambda _: accumulated(
                                            accumulated((
                                                written(variable, "name", row.name),
                                                written(variable, "targets[0].id_type", owner.id_type).bind(lambda _: written(variable, "targets[0].id", owner)),
                                                written(variable, "targets[0].data_path", row.path),
                                            ))
                                            for row in variables
                                            for variable in (driver.variables.new(),)
                                        )
                                    )
                                    .bind(lambda _: modified(curve))
                                    .map(lambda _: True)
                                )
                            )
                            .default_with(lambda: Error((BlenderAbsentFault(owner=repr(curve), key="driver"),)))
                        )

            return (
                held
                .map(lambda curve: matching(curve, source))
                .default_value(Ok(False))
                .bind(
                    lambda matched: (
                        Ok(False)
                        if matched
                        else stored()
                        .bind(
                            lambda curves: (
                                held
                                .map(lambda curve: BlenderEditFault.attempted(request, lambda: curves.remove(curve)))
                                .default_value(Ok(None))
                                .bind(lambda _: BlenderEditFault.attempted(request, lambda: curves.new(path, index=index)))
                            )
                        )
                        .bind(filled)
                    )
                )
            )

        def read(variable: Variable) -> BlenderResult[None]:
            return catch(lambda: owner.path_resolve(variable.path), exception=ValueError)().map_error(lambda _: (BlenderAbsentFault(owner=repr(owner), key=variable.path),)).map(lambda _: None)

        variables = self.source.variables if isinstance(self.source, Driven) else ()
        coordinates: BlenderResult[NDArray[np.float32] | Driven]
        match self.source:
            case Keys(points=points):
                coordinates = Descriptor.resolve(bpy.types.Keyframe, "co", bpy.types.FloatProperty).bind(lambda coordinate: Descriptor.quantized(coordinate.prop, np.asarray(points, dtype=np.float64)))
            case Driven() as driver:
                coordinates = Ok(driver)
        return paired(accumulated((self.channel(owner).map(lambda _: None), *map(read, variables))), coordinates).bind(
            lambda checked: (
                Option
                .of_optional(owner.animation_data_create())
                .map(lambda data: replaced(data, checked[1]))
                .default_with(lambda: Result[bool, BlenderFaults](error=(BlenderAbsentFault(owner=repr(owner), key=path),)))
            )
        )

    def clear(self, owner: bpy.types.ID) -> BlenderResult[None]:
        """Removes channel's F-curve or driver."""
        path, index = self.path, Option.of_optional(self.index).default_value(0)

        def cleared(_: bpy.types.Property) -> BlenderResult[None]:
            held: Option[bpy.types.ActionChannelbagFCurves | bpy.types.AnimDataDrivers] = _animation_data(owner).bind(self.source.curves)
            for curves in held.to_list():
                for curve in Option.of_optional(curves.find(path, index=index)).to_list():
                    curves.remove(curve)
            return Ok(None)

        return self.channel(owner).bind(cleared)


# --- [OPERATIONS] -----------------------------------------------------------------------


def sampled(owner: bpy.types.ID, path: str, frames: Iterable[float], index: int = 0, proposed: Option[float] = Nothing) -> BlenderResult[tuple[float, ...]]:
    """Samples keyed channel at frames, repeating proposed or stored value when unkeyed."""
    held, request = tuple(frames), f"{owner!r}.{path}"

    def keyed(curve: bpy.types.FCurve) -> BlenderResult[tuple[float, ...]]:
        def projected(prop: bpy.types.Property, values: NDArray[np.float32]) -> BlenderResult[tuple[float, ...]]:
            match prop:
                case bpy.types.BoolProperty():
                    return Ok(tuple((values > (np.float32(1) - np.spacing(np.float32(1)))).astype(float).tolist()))
                case bpy.types.IntProperty():
                    return Ok(tuple(float(min(max(int(value), prop.hard_min), prop.hard_max)) for value in values))
                case bpy.types.FloatProperty():
                    return Ok(tuple(values.clip(prop.hard_min, prop.hard_max).astype(float).tolist()))
                case bpy.types.EnumProperty():
                    return Ok(tuple(float(int(value)) for value in values))
                case _:
                    return Error((
                        BlenderChoiceFault(
                            path=path,
                            identifier=type(prop).__name__,
                            accepted=tuple(kind.__name__ for kind in (bpy.types.BoolProperty, bpy.types.IntProperty, bpy.types.FloatProperty, bpy.types.EnumProperty)),
                        ),
                    ))

        return paired(Descriptor.resolve(owner, path, bpy.types.Property), BlenderEditFault.attempted(request, lambda: np.asarray(tuple(map(curve.evaluate, held)), dtype=np.float32))).bind(
            lambda pair: projected(pair[0].prop, pair[1])
        )

    def stored() -> BlenderResult[float]:
        return Descriptor.resolve(owner, path, bpy.types.Property).bind(
            lambda descriptor: (
                descriptor
                .owner_of(owner)
                .filter(lambda _: isinstance(descriptor.prop, bpy.types.BoolProperty | bpy.types.IntProperty | bpy.types.FloatProperty))
                .map(lambda held: np.asarray(descriptor.read(held), np.float64).ravel())
                .filter(lambda values: index < values.size)
                .map(lambda values: float(values[index]))
                .to_result_with(lambda: (BlenderAbsentFault(owner=repr(owner), key=path),))
            )
        )

    def unkeyed() -> BlenderResult[tuple[float, ...]]:
        return proposed.map(Result[float, BlenderFaults].Ok).default_with(stored).map(lambda value: tuple(value for _ in held))

    return _animation_data(owner).bind(Keys.curves).bind(lambda curves: Option.of_optional(curves.find(path, index=index))).map(keyed).default_with(unkeyed)


def _animation_data(owner: bpy.types.ID) -> Option[bpy.types.AnimData]:
    """Returns owner's animation data, Nothing for an ID kind that holds none."""
    return struct_at(owner, "animation_data").bind(lambda data: Some(data) if isinstance(data, bpy.types.AnimData) else Nothing)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Animation", "Driven", "Keys", "Modifier", "Source", "Variable", "sampled"]
