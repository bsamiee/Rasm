"""Render engine classes built from callback bodies taking the host's own callback parameters, the replay engine, and engine sample reads."""

from collections.abc import Callable, Generator, Iterable
from contextlib import contextmanager, ExitStack
from types import MappingProxyType
from typing import ClassVar, Final, Literal, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import Native, struct_at
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band, entered
from blender.bpy.extension.reports import rendered
from blender.bpy.render.outputs.results import COMBINED
from blender.records.faults import accumulated, Fault, Outcome, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [CONSTANTS] ------------------------------------------------------------------------

FLAGS: Final = tuple(prop for prop in bpy.types.RenderEngine.bl_rna.properties if isinstance(prop, bpy.types.BoolProperty) and prop.identifier.startswith("bl_"))
POSTPROCESS: Final = Descriptor.declared(bpy.types.RenderEngine, ("bl_use_postprocess",), bpy.types.BoolProperty)
ENGINE_SAMPLES: Final = MappingProxyType({
    "BLENDER_EEVEE": (("eevee", "taa_render_samples"), ("eevee", "taa_samples")),
    "BLENDER_WORKBENCH": (("display", "render_aa"), ("display", "viewport_aa")),
    "CYCLES": (("cycles", "samples"), ("cycles", "preview_samples")),
})

# --- [MODELS] ---------------------------------------------------------------------------


class EnginePass(msgspec.Struct, frozen=True, kw_only=True):
    """Render pass with channel count from `chanid` length."""

    name: str
    chanid: str

    @property
    def kind(self) -> Literal["VALUE", "VECTOR", "COLOR"]:
        """Registered pass type: one channel a value, RGB or RGBA a color, others a vector."""
        return "VALUE" if len(self.chanid) == 1 else "COLOR" if self.chanid in {"RGB", "RGBA"} else "VECTOR"


class Tile(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Result rectangle at bottom-left offset in one view, the empty view the host's default, pass planes top row first."""

    x: int
    y: int
    planes: tuple[tuple[EnginePass, npt.NDArray[np.float32]], *tuple[tuple[EnginePass, npt.NDArray[np.float32]], ...]]
    view: str = ""
    stamps: tuple[tuple[str, str], ...] = ()


class Engine(msgspec.Struct, frozen=True, kw_only=True):
    """Render engine built from enabled `bl_` flags, others at their RNA default, and callback bodies over the host's own callback parameters, labelled by its key."""

    combined: ClassVar[EnginePass] = EnginePass(name=COMBINED, chanid="RGBA")
    key: Key
    render: Callable[[bpy.types.RenderEngine, bpy.types.Depsgraph], Iterable[Outcome[Tile]]]
    enabled: frozenset[Descriptor[bpy.types.BoolProperty]] = frozenset()
    passes: tuple[EnginePass, ...] | Callable[[bpy.types.Scene, bpy.types.ViewLayer], tuple[EnginePass, ...]] = ()
    view_update: Option[Callable[[bpy.types.Context, bpy.types.Depsgraph], Outcome[None]]] = Nothing
    view_draw: Option[Callable[[bpy.types.Context, bpy.types.Depsgraph], Outcome[None]]] = Nothing
    bake: Option[Callable[[bpy.types.RenderEngine, bpy.types.Depsgraph, bpy.types.Object, "rna_enums.BakePassTypeItems", int, int, int], Outcome[npt.NDArray[np.float32]]]] = Nothing
    joins: frozenset[str] = frozenset()

    # --- [CALLBACKS]

    def built(self, identity: Identity, name: str) -> type[bpy.types.RenderEngine]:
        """Builds `RenderEngine` subclass from callback bodies."""

        def update_render_passes(engine: bpy.types.RenderEngine, scene: bpy.types.Scene, renderlayer: bpy.types.ViewLayer) -> None:
            """Registers Combined, then engine passes."""
            declaration = self.passes
            for pass_ in (self.combined, *(declaration if isinstance(declaration, tuple) else declaration(scene, renderlayer))):
                engine.register_pass(scene, renderlayer, pass_.name, len(pass_.chanid), pass_.chanid, pass_.kind)

        def render(engine: bpy.types.RenderEngine, depsgraph: bpy.types.Depsgraph) -> None:
            """Writes body tiles into the render depsgraph's view layer, reporting each fault at ERROR level."""
            layer: bpy.types.ViewLayer = depsgraph.view_layer  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            for _, text in _written(engine, layer.name, self.render(engine, depsgraph)).swap().map(rendered).default_value(()):
                engine.report({"ERROR"}, text)

        def view_update(_engine: bpy.types.RenderEngine, context: bpy.types.Context, depsgraph: bpy.types.Depsgraph) -> None:
            """Runs viewport update, holding faults in extension reports."""
            for body in self.view_update.to_list():
                identity.reports.reported(body)(context, depsgraph).to_optional()

        def view_draw(engine: bpy.types.RenderEngine, context: bpy.types.Context, depsgraph: bpy.types.Depsgraph) -> None:
            """Draws under scene display space shader, unbinding it after, holding faults in extension reports."""
            with ExitStack() as scope:
                engine.bind_display_space_shader(depsgraph.scene)
                scope.callback(engine.unbind_display_space_shader)
                for body in self.view_draw.to_list():
                    identity.reports.reported(body)(context, depsgraph).to_optional()

        def bake(engine: bpy.types.RenderEngine, depsgraph: bpy.types.Depsgraph, target: bpy.types.Object, pass_type: "rna_enums.BakePassTypeItems", pass_filter: int, width: int, height: int) -> None:
            """Writes body plane to Combined pass of bake result."""
            if (result := engine.begin_result(0, 0, width, height)) is None:
                return
            try:
                for body in self.bake.to_list():
                    baked = body(engine, depsgraph, target, pass_type, pass_filter, width, height).bind(lambda plane: _filled(result, "", self.combined, plane))
                    for _, text in baked.swap().map(rendered).default_value(()):
                        engine.report({"ERROR"}, text)
            finally:
                engine.end_result(result, cancel=engine.test_break())

        namespace: dict[str, object] = {
            "__module__": identity.package,
            "bl_idname": name,
            "bl_label": bpy.path.display_name(self.key),
            **{prop.identifier: prop.default or any(prop == descriptor.prop for descriptor in self.enabled) for prop in FLAGS},
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
    def replayed(cls, identity: Identity, key: Key, tiles: Callable[[bpy.types.Scene, bpy.types.ViewLayer], tuple[Tile, ...]]) -> Generator[Outcome[str]]:
        """Registers a postprocess engine writing the tiles the caller holds for each rendered scene and view layer until scope exit."""

        def passes(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> tuple[EnginePass, ...]:
            return tuple({held.name: held for tile in tiles(scene, layer) for held, _ in tile.planes if held.name != cls.combined.name}.values())

        def render(_engine: bpy.types.RenderEngine, depsgraph: bpy.types.Depsgraph) -> Iterable[Outcome[Tile]]:
            scene: bpy.types.Scene = depsgraph.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            layer: bpy.types.ViewLayer = depsgraph.view_layer  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return map(Ok, tiles(scene, layer))

        replay = cls(key=key, render=render, passes=passes, enabled=frozenset({POSTPROCESS}))
        with ExitStack() as scope:
            yield identity.name(bpy.types.RenderEngine, key).bind(lambda name: entered(scope, (replay.built(identity, name),)).map(lambda _: name))

    @contextmanager
    def joined(self, name: str) -> Generator[Outcome[None]]:
        """Adds engine to `COMPAT_ENGINES` sets holding a `joins` name until scope exit."""
        panels = tuple(engines for panel in bpy.types.Panel.__subclasses__() if isinstance(engines := getattr(panel, "COMPAT_ENGINES", None), set))
        answer = accumulated(
            Ok(None) if any(identifier in engines for engines in panels) else Error(Fault.of(Reason.ABSENT, "COMPAT_ENGINES", member=identifier)) for identifier in sorted(self.joins)
        ).map(lambda _: None)
        with ExitStack() as scope:
            for engines in {id(engines): engines for engines in panels if answer.is_ok() and self.joins.intersection(engines) and name not in engines}.values():
                engines.add(name)
                scope.callback(engines.discard, name)
            yield answer

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires engine class, then `COMPAT_ENGINES` memberships."""
        return identity.name(bpy.types.RenderEngine, self.key).map(lambda name: (self.built(identity, name), (Band.INTERFACE, self.joined(name))))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _written(engine: bpy.types.RenderEngine, layer: str, tiles: Iterable[Outcome[Tile]]) -> Outcome[int]:
    """Writes tiles into a view layer's results until cancellation or first fault, returning written count."""
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
                    continue
                try:
                    filled = accumulated(_filled(result, tile.view, pass_, plane[-result.resolution_y :, : result.resolution_x]) for pass_, plane in tile.planes)
                    for field, value in tile.stamps:
                        result.stamp_data_add_field(field, value)
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


def _filled(result: bpy.types.RenderResult, view: str, pass_: EnginePass, plane: npt.NDArray[np.float32]) -> Outcome[None]:
    """Writes a plane, top row first, into the result's pass of that name and view, absent fault for a pass the engine never registered."""
    return (
        Option
        .of_optional(result.layers[0].passes.find_by_name(pass_.name, view))
        .to_result_with(lambda: Fault.of(Reason.ABSENT, repr(result), member=pass_.name))
        .map(lambda found: found.rect.foreach_set(plane[::-1].ravel()))  # type: ignore[arg-type]
    )


def samples(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> Option[int]:
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
                return Option.of_optional(items.get(identifier)).map(lambda item: item.value)
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
                .of_optional(ENGINE_SAMPLES.get(scene.render.engine))
                .bind(lambda paths: Descriptor.resolve(bpy.types.Scene, paths[0], bpy.types.Property).to_option())
                .bind(lambda descriptor: struct_at(scene, descriptor.owner).bind(lambda owner: counted(descriptor, owner)))
            )
        )
        .filter(lambda count: count > 0)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ENGINE_SAMPLES", "FLAGS", "POSTPROCESS", "Engine", "EnginePass", "Tile", "samples"]
