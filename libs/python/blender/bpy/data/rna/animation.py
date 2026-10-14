"""Declared keyframes, drivers, modifiers, and sampling of RNA channels."""

from collections.abc import Iterable
from typing import Annotated, TYPE_CHECKING

import bpy
from bpy_extras import anim_utils
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
from numpy.typing import NDArray

from blender.bpy.data.rna.descriptors import Descriptor, struct_at, Value, written
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


class Variable(msgspec.Struct, frozen=True, kw_only=True):
    """Driver variable reading a property of the animated data-block."""

    name: str
    path: str


class Driven(Case, frozen=True, kw_only=True, tag=2):
    """Scripted driver over variables from its animated data-block."""

    title = "Driver"
    expression: str
    variables: tuple[Variable, ...] = ()


class Modifier(msgspec.Struct, frozen=True, kw_only=True):
    """Native F-curve modifier type and its property values."""

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
        """RNA data path of the channel without its array index."""
        return f"{self.struct}.{self.member}".removeprefix(".")

    def channel(self, owner: bpy.types.ID) -> BlenderResult[bpy.types.Property]:
        """Resolves the live animatable property, returning absence or an invalid index."""

        def fits(prop: bpy.types.Property) -> bool:
            match prop, self.index:
                case bpy.types.BoolProperty(is_array=True) | bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True) as array, int() as index:
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
        """Reconciles the declared channel, retaining equal native state and unrelated channels, and answers whether it wrote."""
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
            try:
                held = (
                    Option.of_optional(anim_utils.animdata_get_channelbag_for_assigned_slot(data)).bind(lambda bag: Option.of_optional(bag.fcurves.find(path, index=index)))
                    if isinstance(self.source, Keys)
                    else Option.of_optional(data.drivers.find(path, index=index))
                ).to_list()
                for curve in held:
                    match matching(curve, source):
                        case Result(tag="ok", ok=matched):
                            if matched:
                                return Ok(False)
                        case Result(error=faults):
                            return Error(faults)
                match source:
                    case np.ndarray() as points:
                        if (action := data.action) is None:
                            action = bpy.data.actions.new(f"{owner.name}Action")
                            data.action = action
                        if (slot := data.action_slot) is None:
                            slot = action.slots.new(id_type=owner.id_type, name=owner.name)
                            data.action_slot = slot
                        bag = anim_utils.action_ensure_channelbag_for_slot(action, slot)
                        for curve in held:
                            bag.fcurves.remove(curve)
                        curve = bag.fcurves.new(path, index=index)
                        curve.keyframe_points.add(len(points))
                        curve.keyframe_points.foreach_set("co", points.ravel())
                        curve.update_autoflags(owner)
                        curve.update()
                        return modified(curve).map(lambda _: True)
                    case Driven(expression=expression, variables=variables):
                        for curve in held:
                            data.drivers.remove(curve)
                        curve = data.drivers.new(path, index=index)
                        curve.update_autoflags(owner)
                        match curve.driver:
                            case bpy.types.Driver() as driver:
                                driver.type = "SCRIPTED"
                                driver.expression = expression
                                for row in variables:
                                    variable = driver.variables.new()
                                    variable.name = row.name
                                    target = variable.targets[0]
                                    target.id_type = owner.id_type
                                    target.id = owner
                                    target.data_path = row.path
                                return modified(curve).map(lambda _: True)
                            case None:
                                return Error((BlenderAbsentFault(owner=repr(curve), key="driver"),))
            except (AttributeError, ReferenceError, RuntimeError, TypeError, ValueError) as error:
                return Error((BlenderEditFault(request=f"{owner!r}.{path}", error=error),))

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
            lambda admitted: (
                Option
                .of_optional(owner.animation_data_create())
                .map(lambda data: replaced(data, admitted[1]))
                .default_with(lambda: Result[bool, BlenderFaults](error=(BlenderAbsentFault(owner=repr(owner), key=path),)))
            )
        )

    def clear(self, owner: bpy.types.ID) -> BlenderResult[None]:
        """Removes the declared source's channel, returning an unresolved property's fault."""
        path, index = self.path, Option.of_optional(self.index).default_value(0)

        def cleared(_: bpy.types.Property) -> BlenderResult[None]:
            try:
                data = _animation_data(owner)
                collections: Option[bpy.types.ActionChannelbagFCurves | bpy.types.AnimDataDrivers]
                match self.source:
                    case Keys():
                        collections = data.bind(lambda held: Option.of_optional(anim_utils.animdata_get_channelbag_for_assigned_slot(held))).map(lambda bag: bag.fcurves)
                    case Driven():
                        collections = data.map(lambda held: held.drivers)
                for curves in collections.to_list():
                    for curve in Option.of_optional(curves.find(path, index=index)).to_list():
                        curves.remove(curve)
                return Ok(None)
            except ReferenceError as error:
                return Error((BlenderEditFault(request=f"{owner!r}.{path}", error=error),))

        return self.channel(owner).bind(cleared)


# --- [OPERATIONS] -----------------------------------------------------------------------


def sampled(owner: bpy.types.ID, path: str, frames: Iterable[float], index: int = 0, proposed: Option[float] = Nothing) -> BlenderResult[tuple[float, ...]]:
    """Samples rendered RNA values, or repeats the proposed or stored unkeyed value."""

    def keyed(curve: bpy.types.FCurve) -> BlenderResult[tuple[float, ...]]:
        def projected(descriptor: Descriptor) -> BlenderResult[tuple[float, ...]]:
            prop = descriptor.prop
            values = np.asarray(tuple(curve.evaluate(frame) for frame in frames), dtype=np.float32)
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

        return Descriptor.resolve(owner, path, bpy.types.Property).bind(projected)

    def unkeyed() -> BlenderResult[tuple[float, ...]]:
        try:
            stored = proposed.default_with(lambda: float(np.asarray(owner.path_resolve(path), dtype=np.float64).ravel()[index]))
            return Ok(tuple(stored for _ in frames))
        except (ValueError, TypeError, IndexError):
            return Error((BlenderAbsentFault(owner=repr(owner), key=path),))

    try:
        return (
            _animation_data(owner)
            .bind(lambda data: Option.of_optional(anim_utils.animdata_get_channelbag_for_assigned_slot(data)))
            .bind(lambda bag: Option.of_optional(bag.fcurves.find(path, index=index)))
            .map(keyed)
            .default_with(unkeyed)
        )
    except (ReferenceError, TypeError, ValueError, OverflowError) as error:
        return Error((BlenderEditFault(request=f"{owner!r}.{path}", error=error),))


def _animation_data(owner: bpy.types.ID) -> Option[bpy.types.AnimData]:
    return struct_at(owner, "animation_data").bind(lambda data: Some(data) if isinstance(data, bpy.types.AnimData) else Nothing)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Animation", "Driven", "Keys", "Modifier", "Source", "Variable", "sampled"]
