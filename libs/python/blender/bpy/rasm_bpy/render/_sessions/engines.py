"""Render engines from native callbacks, replayed multilayer passes, and sample settings."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from functools import partial
from itertools import islice
from types import MappingProxyType
from typing import ClassVar, Final, Literal, Self, TYPE_CHECKING

import bpy
from expression import Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import _entered, _Tier, Acquisitions
from rasm_bpy.data._extension.reports import _rendered
from rasm_bpy.data._rna.descriptors import Descriptor
from rasm_bpy.data._rna.paths import _struct_at, Native
from rasm_bpy.render._outputs.framing import Framing, RenderRegion
from rasm_bpy.render._outputs.results import COMBINED
from rasm_imaging import Layer, Multilayer
from rasm_records import accumulated, Error, Fault, Nothing, Ok, Outcome, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [CONSTANTS] ------------------------------------------------------------------------

_ENGINE_SAMPLES: Final = MappingProxyType({
    "BLENDER_EEVEE": (("eevee", "taa_render_samples"), ("eevee", "taa_samples")),
    "BLENDER_WORKBENCH": (("display", "render_aa"), ("display", "viewport_aa")),
    "CYCLES": (("cycles", "samples"), ("cycles", "preview_samples")),
})

# --- [MODELS] ---------------------------------------------------------------------------


class _EnginePass(msgspec.Struct, frozen=True, kw_only=True):
    """Render pass with channel count from `chanid` length."""

    name: str
    chanid: str

    @property
    def kind(self) -> Literal["VALUE", "VECTOR", "COLOR"]:
        """Registered pass type: one channel a value, RGB or RGBA a color, others a vector."""
        return "VALUE" if len(self.chanid) == 1 else "COLOR" if self.chanid.upper() in {"RGB", "RGBA"} else "VECTOR"


class _Tile(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Result rectangle at bottom-left offset in one view, the empty view the host's default, pass planes top row first."""

    x: int
    y: int
    planes: tuple[tuple[_EnginePass, npt.NDArray[np.float32]], *tuple[tuple[_EnginePass, npt.NDArray[np.float32]], ...]]
    view: str = ""
    stamps: tuple[tuple[str, str], ...] = ()

    @classmethod
    def replayed(cls, file: Multilayer, scene: bpy.types.Scene, /) -> Outcome[Mapping[str, tuple[Self, ...]]]:
        """Returns the tiles of each scene view layer a saved multilayer render holds, one per view, under the first part's header stamps.

        Planes a file holds at the whole canvas of a border render are cropped to the border window the engine writes.
        """
        names = tuple(sorted((layer.name for layer in scene.view_layers), key=len, reverse=True))
        framing = Framing.of(scene.render)
        (width, height), window = framing.bounds
        stamps = tuple((attribute.name, value) for part in islice(file.parts.values(), 1) for attribute in part.extra_attribs if isinstance(value := attribute.value, str))

        def cropped(values: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
            bordered = isinstance(framing.extent, RenderRegion) and values.shape[:2] == (height, width)
            return np.ascontiguousarray(values[window.ybegin : window.yend, window.xbegin : window.xend] if bordered else values)

        def plane(owner: str, layer: Layer) -> Outcome[tuple[str, str, _EnginePass, npt.NDArray[np.float32]]]:
            declared = _EnginePass(name=layer.name[len(owner) + 1 :], chanid="".join(layer.channels))
            return file.plane(layer, layer.channels).map(lambda values: (owner, file.parts[layer.part].get_string_attribute("view"), declared, cropped(values)))

        def grouped(planes: tuple[tuple[str, str, _EnginePass, npt.NDArray[np.float32]], ...]) -> Mapping[str, tuple[Self, ...]]:
            held: dict[str, dict[str, list[tuple[_EnginePass, npt.NDArray[np.float32]]]]] = {}
            for owner, view, declared, values in planes:
                held.setdefault(owner, {}).setdefault(view, []).append((declared, values))
            return {owner: tuple(cls(x=0, y=0, planes=(found[0], *found[1:]), view=view, stamps=stamps) for view, found in views.items()) for owner, views in held.items()}

        return accumulated(plane(owner, layer) for layer in file.layers for owner in Option.of_optional(next((name for name in names if layer.name.startswith(f"{name}.")), None)).to_list()).map(
            grouped
        )


class _Engine(msgspec.Struct, frozen=True, kw_only=True):
    """Render engine with explicit native flag overrides and callbacks, labeled by its key."""

    # --- [STATE]

    combined: ClassVar[_EnginePass] = _EnginePass(name=COMBINED, chanid="RGBA")
    key: Key
    render: Callable[[bpy.types.RenderEngine, bpy.types.Depsgraph], Iterable[Outcome[_Tile]]]
    flags: Mapping[bpy.types.BoolProperty, bool] = MappingProxyType({})
    passes: tuple[_EnginePass, ...] = ()
    pass_builder: Option[Callable[[bpy.types.Scene, bpy.types.ViewLayer], tuple[_EnginePass, ...]]] = Nothing
    view_update: Option[Callable[[bpy.types.RenderEngine, bpy.types.Context, bpy.types.Depsgraph], Outcome[None]]] = Nothing
    view_draw: Option[Callable[[bpy.types.RenderEngine, bpy.types.Context, bpy.types.Depsgraph], Outcome[None]]] = Nothing
    bake: Option[Callable[[bpy.types.RenderEngine, bpy.types.Depsgraph, bpy.types.Object, "rna_enums.BakePassTypeItems", int, int, int], Outcome[None]]] = Nothing
    compat_engines: frozenset[str] = frozenset()

    # --- [CALLBACKS]

    def built(self, identity: Identity) -> type[bpy.types.RenderEngine]:
        """Builds `RenderEngine` subclass from callback bodies."""

        def update_render_passes(engine: bpy.types.RenderEngine, scene: bpy.types.Scene, renderlayer: bpy.types.ViewLayer) -> None:
            """Registers Combined, then engine passes."""
            for pass_ in (self.combined, *self.passes, *self.pass_builder.map(lambda build: build(scene, renderlayer)).default_value(())):
                engine.register_pass(scene, renderlayer, pass_.name, len(pass_.chanid), pass_.chanid, pass_.kind)

        def render(engine: bpy.types.RenderEngine, depsgraph: bpy.types.Depsgraph) -> None:
            """Writes body tiles into the render depsgraph's view layer, reporting each fault at ERROR level."""
            layer: bpy.types.ViewLayer = depsgraph.view_layer  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            for _, text in _written(engine, layer.name, self.render(engine, depsgraph)).swap().map(_rendered).default_value(()):
                engine.report({"ERROR"}, text)

        def view_update(engine: bpy.types.RenderEngine, context: bpy.types.Context, depsgraph: bpy.types.Depsgraph) -> None:
            """Runs viewport update, logging faults."""
            for body in self.view_update.to_list():
                identity.reports.reported(body)(engine, context, depsgraph).to_optional()

        def view_draw(engine: bpy.types.RenderEngine, context: bpy.types.Context, depsgraph: bpy.types.Depsgraph) -> None:
            """Draws under scene display space shader, unbinding it after, logging faults."""
            with ExitStack() as scope:
                engine.bind_display_space_shader(depsgraph.scene)
                scope.callback(engine.unbind_display_space_shader)
                for body in self.view_draw.to_list():
                    identity.reports.reported(body)(engine, context, depsgraph).to_optional()

        def bake(engine: bpy.types.RenderEngine, depsgraph: bpy.types.Depsgraph, target: bpy.types.Object, pass_type: "rna_enums.BakePassTypeItems", pass_filter: int, width: int, height: int) -> None:
            """Runs native bake body and reports its faults."""
            for body in self.bake.to_list():
                for _, text in body(engine, depsgraph, target, pass_type, pass_filter, width, height).swap().map(_rendered).default_value(()):
                    engine.report({"ERROR"}, text)

        name = identity.name(bpy.types.RenderEngine, self.key)
        namespace: dict[str, object] = {
            "__module__": identity.package,
            "bl_idname": name,
            "bl_label": bpy.path.display_name(self.key),
            **{prop.identifier: value for prop, value in self.flags.items()},
            "update_render_passes": update_render_passes,
            "render": render,
            **({"view_update": view_update} if self.view_update.is_some() else {}),
            **({"view_draw": view_draw} if self.view_draw.is_some() else {}),
            **({"bake": bake} if self.bake.is_some() else {}),
        }
        return type(name, (bpy.types.RenderEngine,), namespace)

    # --- [REGISTRATION]

    @classmethod
    @contextmanager
    def replayed(cls, identity: Identity, key: Key, tiles: Callable[[bpy.types.Scene, bpy.types.ViewLayer], tuple[_Tile, ...]]) -> Generator[Outcome[str]]:
        """Registers a postprocess engine writing the tiles the caller holds for each rendered scene and view layer until scope exit."""

        def passes(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> tuple[_EnginePass, ...]:
            return tuple({held.name: held for tile in tiles(scene, layer) for held, _ in tile.planes if held.name != cls.combined.name}.values())

        def render(_engine: bpy.types.RenderEngine, depsgraph: bpy.types.Depsgraph) -> Iterable[Outcome[_Tile]]:
            scene: bpy.types.Scene = depsgraph.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            layer: bpy.types.ViewLayer = depsgraph.view_layer  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return map(Ok, tiles(scene, layer))

        replay = cls(key=key, render=render, pass_builder=Some(passes)).built(identity)
        with ExitStack() as scope:
            yield _entered(scope, (replay,)).map(lambda _: replay.bl_idname)

    @contextmanager
    def joined(self, name: str) -> Generator[Outcome[None]]:
        """Adds engine to `COMPAT_ENGINES` sets holding a `compat_engines` name until scope exit."""
        with ExitStack() as scope:
            for panel in bpy.types.Panel.__subclasses__() if self.compat_engines else ():
                try:
                    engines: set[str] = panel.COMPAT_ENGINES  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute, unsound-assignment]
                except AttributeError:  # ruff: ignore[try-except-continue]
                    continue
                if self.compat_engines.intersection(engines) and name not in engines:
                    engines.add(name)
                    scope.callback(engines.discard, name)
            yield Ok(None)

    def acquisitions(self, identity: Identity, /) -> Acquisitions:
        """Acquires engine class, then `COMPAT_ENGINES` memberships."""
        engine = self.built(identity)
        return engine, (_Tier.INTERFACE, self.joined(engine.bl_idname))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _written(engine: bpy.types.RenderEngine, layer: str, tiles: Iterable[Outcome[_Tile]]) -> Outcome[int]:
    """Writes tiles into a view layer's results until cancellation or first fault, returning written count."""
    final: bpy.types.RenderResult = engine.get_result()  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    count, area = 0, 0
    for answer in tiles:
        if engine.test_break():
            break
        match answer:
            case Result(tag="error", error=faults):
                return Error(faults)
            case Result(ok=tile):
                height, width = tile.planes[0][1].shape[:2]
                if (result := engine.begin_result(tile.x, tile.y, width, height, layer=layer, view=tile.view)) is None:
                    return Error(Fault.of(Reason.ABSENT, repr(engine), member=bpy.types.RenderResult.__name__))
                try:
                    filled = accumulated(_filled(result, tile.view, pass_, plane[-result.resolution_y :, : result.resolution_x]) for pass_, plane in tile.planes)
                    for field, value in tile.stamps:
                        final.stamp_data_add_field(field, value)
                    area += result.resolution_x * result.resolution_y
                finally:
                    engine.end_result(result, cancel=engine.test_break())
                match filled:
                    case Result(tag="error", error=faults):
                        return Error(faults)
                    case Result():
                        count += 1
                engine.update_progress(area / (engine.resolution_x * engine.resolution_y))
    return Ok(count)


def _filled(result: bpy.types.RenderResult, view: str, pass_: _EnginePass, plane: npt.NDArray[np.float32]) -> Outcome[None]:
    """Writes a plane, top row first, into the result's pass of that name and view, absent fault for a pass the engine never registered, invalid fault for a plane the pass refuses."""

    @catch(exception=TypeError)
    def filled(found: bpy.types.RenderPass) -> None:
        found.rect.foreach_set(plane[::-1].ravel())  # type: ignore[arg-type]

    return (
        Option
        .of_optional(result.layers[0].passes.find_by_name(pass_.name, view))
        .to_result_with(lambda: Fault.of(Reason.ABSENT, repr(result), member=pass_.name))
        .bind(lambda found: filled(found).map_error(partial(Fault.of, Reason.INVALID, pass_.name)))
    )


def _samples(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> Option[int]:
    """Returns a layer's sample override, else the render samples of the scene's engine read from its own settings alone, `Nothing` for an engine stating none.

    Args:
        scene: Scene whose engine settings hold the render samples.
        layer: View layer whose nonzero `samples` overrides the engine's.

    Returns:
        Positive sample count, an enum member counting by its item's host value.
    """

    def counted(descriptor: Descriptor[bpy.types.Property], owner: Native) -> Option[int]:
        match descriptor.prop, descriptor.read(owner):
            case bpy.types.EnumProperty(enum_items=items), str(identifier):
                return Some(items[identifier].value)
            case _, int(count):
                return Some(count)
            case _:
                return Nothing

    return (
        Some(layer.samples)
        .filter(lambda count: count > 0)
        .or_else_with(
            lambda: (
                Option
                .of_optional(_ENGINE_SAMPLES.get(scene.render.engine))
                .bind(lambda paths: Descriptor.resolve(bpy.types.Scene, paths[0], bpy.types.Property).to_option())
                .bind(lambda descriptor: _struct_at(scene, descriptor.owner).bind(lambda owner: counted(descriptor, owner)))
            )
        )
        .filter(lambda count: count > 0)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
