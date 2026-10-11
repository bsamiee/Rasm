"""Native render engines, declared passes, result tiles, bakes, and viewport callbacks."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from typing import Annotated, ClassVar, Literal

import bpy
from expression import Error, Nothing, Ok, Option
import msgspec
import numpy as np
import numpy.typing as npt

from rasm.blender.extension.identity import Identity, Key, Name
from rasm.blender.extension.registration import Acquisitions, Band, entered
from rasm.blender.extension.reports import rendered
from rasm.blender.faults import BlenderAbsentFault, BlenderResult
from rasm.imaging.faults import accumulated

# --- [TYPES] ----------------------------------------------------------------------------

type PassKind = Literal["VALUE", "VECTOR", "COLOR"]

# --- [MODELS] ---------------------------------------------------------------------------


class Traits(msgspec.Struct, frozen=True, kw_only=True):
    """Native engine traits, named without their host prefix."""

    use_preview: bool = False
    use_postprocess: bool = False
    use_eevee_viewport: bool = False
    use_custom_freestyle: bool = False
    use_image_save: bool = True
    use_gpu_context: bool = False
    use_shading_nodes_custom: bool = True
    use_spherical_stereo: bool = False
    use_stereo_viewport: bool = False
    write_viewport_depth: bool = False
    use_materialx: bool = False


class Pass(msgspec.Struct, frozen=True, kw_only=True):
    """Declared pass, with its channel count derived from its channel identifiers."""

    name: str
    chanid: Annotated[str, msgspec.Meta(min_length=1, max_length=8)]
    kind: PassKind


class Shot(msgspec.Struct, frozen=True, kw_only=True):
    """Engine and its evaluated render dependencies."""

    engine: bpy.types.RenderEngine
    depsgraph: bpy.types.Depsgraph


class Tile(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """One native bottom-left rectangle's top-first pass planes, view, and stamp fields."""

    x: int
    y: int
    planes: tuple[tuple[Pass, npt.NDArray[np.float32]], *tuple[tuple[Pass, npt.NDArray[np.float32]], ...]]
    view: Option[str] = Nothing
    stamps: tuple[tuple[str, str], ...] = ()


class Baking(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Native bake request and result, including its primitive, differential, and seed passes."""

    engine: bpy.types.RenderEngine
    depsgraph: bpy.types.Depsgraph
    target: bpy.types.Object
    kind: str
    filter: int
    result: bpy.types.RenderResult


class Viewport(msgspec.Struct, frozen=True, kw_only=True):
    """Native viewport update and draw bodies."""

    update: Callable[[bpy.types.Context, bpy.types.Depsgraph], BlenderResult[None]]
    draw: Callable[[bpy.types.Context, bpy.types.Depsgraph], BlenderResult[None]]


class Engine(msgspec.Struct, frozen=True, kw_only=True):
    """Registered native engine assembled from its traits and callback bodies."""

    combined: ClassVar[Pass] = Pass(name="Combined", chanid="RGBA", kind="COLOR")
    key: Key
    title: str
    render: Callable[[Shot], Iterable[BlenderResult[Tile]]]
    traits: Traits = Traits()
    passes: tuple[Pass, ...] | Callable[[bpy.types.Scene, bpy.types.ViewLayer], tuple[Pass, ...]] = ()
    viewport: Option[Viewport] = Nothing
    bake: Option[Callable[[Baking], BlenderResult[npt.NDArray[np.float32]]]] = Nothing
    joins: frozenset[str] = frozenset()

    # --- [CALLBACKS]

    def built(self, identity: Identity, name: str) -> type[bpy.types.RenderEngine]:
        """Builds the native class with its render, pass, bake, and viewport callbacks."""

        def update_render_passes(engine: bpy.types.RenderEngine, scene: bpy.types.Scene, renderlayer: bpy.types.ViewLayer) -> None:
            """Declares Combined followed by this engine's passes."""
            declaration = self.passes
            passes = declaration if isinstance(declaration, tuple) else declaration(scene, renderlayer)
            for pass_ in (self.combined, *passes):
                engine.register_pass(scene, renderlayer, pass_.name, len(pass_.chanid), pass_.chanid, pass_.kind)

        def render(engine: bpy.types.RenderEngine, depsgraph: bpy.types.Depsgraph) -> None:
            """Writes the body's tiles and reports every refused operation to Blender."""
            shot = Shot(engine=engine, depsgraph=depsgraph)
            for faults in written(shot, self.render(shot)).swap().to_option().to_list():
                for _, text in rendered(faults):
                    engine.report({"ERROR"}, text)

        namespace: dict[str, object] = {
            "__module__": identity.package,
            "bl_idname": name,
            "bl_label": self.title,
            **{f"bl_{field.name}": getattr(self.traits, field.name) for field in msgspec.structs.fields(self.traits)},
            "update_render_passes": update_render_passes,
            "render": render,
        }
        for viewport in self.viewport.to_list():
            updated, drawn = identity.reports.reported(viewport.update), identity.reports.reported(viewport.draw)

            def view_update(engine: bpy.types.RenderEngine, context: bpy.types.Context, depsgraph: bpy.types.Depsgraph) -> None:
                """Translates the viewport update result at its native callback boundary."""
                return updated(context, depsgraph).default_value(None)

            def view_draw(engine: bpy.types.RenderEngine, context: bpy.types.Context, depsgraph: bpy.types.Depsgraph) -> None:
                """Draws under the scene's display transform and releases its shader binding."""
                engine.bind_display_space_shader(depsgraph.scene)
                try:
                    return drawn(context, depsgraph).default_value(None)
                finally:
                    engine.unbind_display_space_shader()

            namespace.update(view_update=view_update, view_draw=view_draw)
        for body in self.bake.to_list():

            def bake(engine: bpy.types.RenderEngine, depsgraph: bpy.types.Depsgraph, target: bpy.types.Object, pass_type: str, pass_filter: int, width: int, height: int) -> None:
                """Reads native bake inputs and commits the body's Combined plane."""
                if (result := engine.begin_result(0, 0, width, height)) is None:
                    return
                try:
                    passes = result.layers[0].passes
                    if (answer := body(Baking(engine=engine, depsgraph=depsgraph, target=target, kind=pass_type, filter=pass_filter, result=result))).is_error():
                        for _, text in rendered(answer.error):
                            engine.report({"ERROR"}, text)
                    else:
                        passes[self.combined.name].rect.foreach_set(answer.ok[::-1].ravel())  # type: ignore[arg-type]
                finally:
                    engine.end_result(result, cancel=engine.test_break())

            namespace["bake"] = bake
        return type(name, (bpy.types.RenderEngine,), namespace)

    # --- [REGISTRATION]

    @classmethod
    @contextmanager
    def replayed(cls, identity: Identity, key: Key, planes: Mapping[tuple[int, int, str, str], Tile], frame: Callable[[], int]) -> Generator[BlenderResult[str]]:
        """Holds a compositor-capable engine replaying captured native layer and view planes."""

        def passes(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> tuple[Pass, ...]:
            current = frame()
            return tuple(
                {
                    pass_.name: pass_
                    for (at, source, name, _view), captured in planes.items()
                    if at == current and source == scene.session_uid and name == layer.name
                    for pass_, _image in captured.planes
                    if pass_.name != cls.combined.name
                }.values()
            )

        def rendered(shot: Shot) -> Iterable[BlenderResult[Tile]]:
            current = frame()
            for (at, source, layer, _view), captured in planes.items():
                if at != current or source != shot.depsgraph.scene.session_uid or layer != shot.depsgraph.view_layer.name:  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                    continue
                yield Ok(captured)

        replay = cls(key=key, title="Captured Render", render=rendered, passes=passes, traits=Traits(use_postprocess=True))
        with ExitStack() as scope:
            yield identity.name(Name.ENGINE, key).bind(lambda name: entered(scope, (replay.built(identity, name),)).map(lambda _: name))

    @contextmanager
    def joined(self, name: str) -> Generator[BlenderResult[None]]:
        """Holds this engine's membership in the requested stock panel sets."""
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
        """Acquires the class before its scoped panel compatibility memberships."""
        return identity.name(Name.ENGINE, self.key).map(lambda name: ((Band.INTERFACE, self.built(identity, name)), (Band.INTERFACE, self.joined(name))))


# --- [OPERATIONS] -----------------------------------------------------------------------


def written(shot: Shot, tiles: Iterable[BlenderResult[Tile]]) -> BlenderResult[int]:
    """Writes ordered native tiles until cancellation or the first refused tile."""
    count, area = 0, 0
    for answer in tiles:
        if shot.engine.test_break():
            break
        if answer.is_error():
            return Error(answer.error)
        tile = answer.ok
        height, width = tile.planes[0][1].shape[:2]
        if (result := shot.engine.begin_result(tile.x, tile.y, width, height, layer=shot.depsgraph.view_layer.name, view=tile.view.default_value(""))) is None:  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            continue
        try:
            for pass_, plane in tile.planes:
                result.layers[0].passes[pass_.name].rect.foreach_set(plane[-result.resolution_y :, : result.resolution_x][::-1].ravel())  # type: ignore[arg-type]
            for field, value in tile.stamps:
                result.stamp_data_add_field(field, value)
            area += result.resolution_x * result.resolution_y
            count += 1
        finally:
            shot.engine.end_result(result, cancel=shot.engine.test_break())
        shot.engine.update_progress(area / (shot.engine.resolution_x * shot.engine.resolution_y))
    return Ok(count)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Baking", "Engine", "Pass", "PassKind", "Shot", "Tile", "Traits", "Viewport", "written"]
