"""Render engine classes built from callback bodies."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from typing import Annotated, ClassVar, Final, Literal

import bpy
from expression import Error, Nothing, Ok, Option, Result
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Band, entered
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.render.outputs.exports import COMBINED
from blender.records.faults import accumulated

# --- [TYPES] ----------------------------------------------------------------------------


class Captured(msgspec.Struct, frozen=True, kw_only=True):
    """Replayed tile key: frame, scene session UID, view layer, and view."""

    frame: int
    scene: int
    layer: str
    view: str


# --- [CONSTANTS] ------------------------------------------------------------------------

FLAGS: Final = tuple(prop for prop in bpy.types.RenderEngine.bl_rna.properties if isinstance(prop, bpy.types.BoolProperty) and prop.identifier.startswith("bl_"))
POSTPROCESS: Final = Descriptor.declared(bpy.types.RenderEngine, "bl_use_postprocess", bpy.types.BoolProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class Pass(msgspec.Struct, frozen=True, kw_only=True):
    """Render pass with channel count from `chanid` length."""

    name: str
    chanid: Annotated[str, msgspec.Meta(min_length=1, max_length=8)]

    @property
    def kind(self) -> Literal["VALUE", "VECTOR", "COLOR"]:
        """Registered pass type: one channel a value, RGB or RGBA a color, others a vector."""
        return "VALUE" if len(self.chanid) == 1 else "COLOR" if self.chanid in {"RGB", "RGBA"} else "VECTOR"


class Tile(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Result rectangle at bottom-left offset, pass planes top row first."""

    x: int
    y: int
    planes: tuple[tuple[Pass, npt.NDArray[np.float32]], *tuple[tuple[Pass, npt.NDArray[np.float32]], ...]]
    view: Option[str] = Nothing
    stamps: tuple[tuple[str, str], ...] = ()


class RenderRequest(msgspec.Struct, frozen=True, kw_only=True):
    """Engine, render depsgraph, scene, and view layer of one render call."""

    engine: bpy.types.RenderEngine
    depsgraph: bpy.types.Depsgraph
    scene: bpy.types.Scene
    layer: bpy.types.ViewLayer

    def written(self, tiles: Iterable[BlenderResult[Tile]]) -> BlenderResult[int]:
        """Writes tiles until cancellation or first fault, returning written count."""
        count, area = 0, 0
        for answer in tiles:
            if self.engine.test_break():
                break
            match answer:
                case Result(tag="error", error=faults):
                    return Error(faults)
                case Result(ok=tile):
                    height, width = tile.planes[0][1].shape[:2]
                    if (result := self.engine.begin_result(tile.x, tile.y, width, height, layer=self.layer.name, view=tile.view.default_value(""))) is None:
                        continue
                    try:
                        for pass_, plane in tile.planes:
                            result.layers[0].passes[pass_.name].rect.foreach_set(plane[-result.resolution_y :, : result.resolution_x][::-1].ravel())  # type: ignore[arg-type]
                        for field, value in tile.stamps:
                            result.stamp_data_add_field(field, value)
                        area += result.resolution_x * result.resolution_y
                        count += 1
                    finally:
                        self.engine.end_result(result, cancel=self.engine.test_break())
                    self.engine.update_progress(area / (self.engine.resolution_x * self.engine.resolution_y))
        return Ok(count)


class Baking(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Bake request with result holding primitive, differential, and seed passes."""

    engine: bpy.types.RenderEngine
    depsgraph: bpy.types.Depsgraph
    target: bpy.types.Object
    kind: str
    filter: int
    result: bpy.types.RenderResult


class Viewport(msgspec.Struct, frozen=True, kw_only=True):
    """Viewport update and draw bodies."""

    update: Callable[[bpy.types.Context, bpy.types.Depsgraph], BlenderResult[None]]
    draw: Callable[[bpy.types.Context, bpy.types.Depsgraph], BlenderResult[None]]


class Engine(msgspec.Struct, frozen=True, kw_only=True):
    """Render engine built from enabled `bl_` flags, others at their RNA default, and callback bodies."""

    combined: ClassVar[Pass] = Pass(name=COMBINED, chanid="RGBA")
    key: Key
    title: str
    render: Callable[[RenderRequest], Iterable[BlenderResult[Tile]]]
    enabled: frozenset[Descriptor[bpy.types.BoolProperty]] = frozenset()
    passes: tuple[Pass, ...] | Callable[[bpy.types.Scene, bpy.types.ViewLayer], tuple[Pass, ...]] = ()
    viewport: Option[Viewport] = Nothing
    bake: Option[Callable[[Baking], BlenderResult[npt.NDArray[np.float32]]]] = Nothing
    joins: frozenset[str] = frozenset()

    # --- [CALLBACKS]

    def built(self, identity: Identity, name: str) -> type[bpy.types.RenderEngine]:
        """Builds `RenderEngine` subclass from callback bodies."""

        def update_render_passes(engine: bpy.types.RenderEngine, scene: bpy.types.Scene, renderlayer: bpy.types.ViewLayer) -> None:
            """Registers Combined, then engine passes."""
            declaration = self.passes
            passes = declaration if isinstance(declaration, tuple) else declaration(scene, renderlayer)
            for pass_ in (self.combined, *passes):
                engine.register_pass(scene, renderlayer, pass_.name, len(pass_.chanid), pass_.chanid, pass_.kind)

        def render(engine: bpy.types.RenderEngine, depsgraph: bpy.types.Depsgraph) -> None:
            """Writes body tiles and reports each fault at ERROR level."""
            request: BlenderResult[RenderRequest] = (
                Option
                .of_optional(depsgraph.scene)
                .bind(lambda scene: Option.of_optional(depsgraph.view_layer).map(lambda layer: RenderRequest(engine=engine, depsgraph=depsgraph, scene=scene, layer=layer)))
                .to_result_with(lambda: (BlenderAbsentFault(owner=repr(depsgraph), key="view_layer"),))
            )
            for _, text in Result[RenderRequest, BlenderFaults].bind(request, lambda held: held.written(self.render(held))).swap().map(rendered).default_value(()):
                engine.report({"ERROR"}, text)

        namespace: dict[str, object] = {
            "__module__": identity.package,
            "bl_idname": name,
            "bl_label": self.title,
            **{prop.identifier: prop.default or any(prop == descriptor.prop for descriptor in self.enabled) for prop in FLAGS},
            "update_render_passes": update_render_passes,
            "render": render,
        }
        for viewport in self.viewport.to_list():
            updated, drawn = identity.reports.reported(viewport.update), identity.reports.reported(viewport.draw)

            def view_update(_engine: bpy.types.RenderEngine, context: bpy.types.Context, depsgraph: bpy.types.Depsgraph) -> None:
                """Runs viewport update, holding faults in extension reports."""
                return updated(context, depsgraph).to_optional()

            def view_draw(engine: bpy.types.RenderEngine, context: bpy.types.Context, depsgraph: bpy.types.Depsgraph) -> None:
                """Draws under scene display space shader, unbinding it after."""
                engine.bind_display_space_shader(depsgraph.scene)
                try:
                    return drawn(context, depsgraph).to_optional()
                finally:
                    engine.unbind_display_space_shader()

            namespace.update(view_update=view_update, view_draw=view_draw)
        for body in self.bake.to_list():

            def bake(engine: bpy.types.RenderEngine, depsgraph: bpy.types.Depsgraph, target: bpy.types.Object, pass_type: str, pass_filter: int, width: int, height: int) -> None:
                """Writes body plane to Combined pass of bake result."""
                if (result := engine.begin_result(0, 0, width, height)) is None:
                    return
                try:
                    baked = body(Baking(engine=engine, depsgraph=depsgraph, target=target, kind=pass_type, filter=pass_filter, result=result)).map(
                        lambda plane: result.layers[0].passes[self.combined.name].rect.foreach_set(plane[::-1].ravel())  # type: ignore[arg-type]
                    )
                    for _, text in baked.swap().map(rendered).default_value(()):
                        engine.report({"ERROR"}, text)
                finally:
                    engine.end_result(result, cancel=engine.test_break())

            namespace["bake"] = bake
        return type(name, (bpy.types.RenderEngine,), namespace)

    # --- [REGISTRATION]

    @classmethod
    @contextmanager
    def replayed(cls, identity: Identity, key: Key, planes: Mapping[Captured, Tile], frame: Callable[[], int]) -> Generator[BlenderResult[str]]:
        """Registers postprocess engine replaying captured planes until scope exit."""

        def matching(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> Iterable[Tile]:
            current = frame()
            return (tile for at, tile in planes.items() if (at.frame, at.scene, at.layer) == (current, scene.session_uid, layer.name))

        def passes(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> tuple[Pass, ...]:
            return tuple({held.name: held for tile in matching(scene, layer) for held, _image in tile.planes if held.name != cls.combined.name}.values())

        replay = cls(key=key, title="Captured Render", render=lambda request: map(Ok, matching(request.scene, request.layer)), passes=passes, enabled=frozenset({POSTPROCESS}))
        with ExitStack() as scope:
            yield identity.name(Name.ENGINE, key).bind(lambda name: entered(scope, (replay.built(identity, name),)).map(lambda _: name))

    @contextmanager
    def joined(self, name: str) -> Generator[BlenderResult[None]]:
        """Adds engine to `COMPAT_ENGINES` sets holding a `joins` name until scope exit."""
        panels = tuple(panel.COMPAT_ENGINES for panel in bpy.types.Panel.__subclasses__() if hasattr(panel, "COMPAT_ENGINES") and isinstance(panel.COMPAT_ENGINES, set))
        selected = {id(engines): engines for engines in panels if self.joins.intersection(engines) and name not in engines}
        answer = accumulated(
            Ok(None) if any(identifier in engines for engines in panels) else Error((BlenderAbsentFault(owner="COMPAT_ENGINES", key=identifier),)) for identifier in sorted(self.joins)
        ).map(lambda _: None)
        if answer.is_ok():
            for engines in selected.values():
                engines.add(name)
        try:
            yield answer
        finally:
            if answer.is_ok():
                for engines in selected.values():
                    engines.discard(name)

    def acquisitions(self, identity: Identity, /) -> BlenderResult[Acquisitions]:
        """Acquires engine class, then `COMPAT_ENGINES` memberships."""
        return identity.name(Name.ENGINE, self.key).map(lambda name: ((Band.INTERFACE, self.built(identity, name)), (Band.INTERFACE, self.joined(name))))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FLAGS", "POSTPROCESS", "Baking", "Captured", "Engine", "Pass", "RenderRequest", "Tile", "Viewport"]
