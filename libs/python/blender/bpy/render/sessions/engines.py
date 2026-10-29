"""Render engine classes built from callback bodies."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from types import MappingProxyType
from typing import Annotated, ClassVar, Final, Literal, TYPE_CHECKING

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
from blender.bpy.faults import BlenderReason
from blender.bpy.render.outputs.results import COMBINED
from blender.records.faults import accumulated, Fault, Outcome

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


class PlaneKey(msgspec.Struct, frozen=True, kw_only=True):
    """Replayed tile key: frame, scene session UID, view layer, and view."""

    frame: int
    scene: int
    layer: str
    view: str


class EnginePass(msgspec.Struct, frozen=True, kw_only=True):
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
    planes: tuple[tuple[EnginePass, npt.NDArray[np.float32]], *tuple[tuple[EnginePass, npt.NDArray[np.float32]], ...]]
    view: Option[str] = Nothing
    stamps: tuple[tuple[str, str], ...] = ()


class RenderRequest(msgspec.Struct, frozen=True, kw_only=True):
    """Engine, render depsgraph, scene, and view layer of one render call."""

    engine: bpy.types.RenderEngine
    depsgraph: bpy.types.Depsgraph
    scene: bpy.types.Scene
    layer: bpy.types.ViewLayer

    def written(self, tiles: Iterable[Outcome[Tile]]) -> Outcome[int]:
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
                    view = tile.view.default_value("")
                    if (result := self.engine.begin_result(tile.x, tile.y, width, height, layer=self.layer.name, view=view)) is None:
                        continue
                    try:
                        filled = accumulated(_filled(result, view, pass_, plane[-result.resolution_y :, : result.resolution_x]) for pass_, plane in tile.planes)
                        for field, value in tile.stamps:
                            result.stamp_data_add_field(field, value)
                        area += result.resolution_x * result.resolution_y
                    finally:
                        self.engine.end_result(result, cancel=self.engine.test_break())
                    match filled:
                        case Result(tag="error", error=faults):
                            return Error(faults)
                        case Result():
                            count += 1
                    self.engine.update_progress(area / (self.engine.resolution_x * self.engine.resolution_y))
        return Ok(count)


class Baking(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Bake request with result holding primitive, differential, and seed passes."""

    engine: bpy.types.RenderEngine
    depsgraph: bpy.types.Depsgraph
    target: bpy.types.Object
    kind: "rna_enums.BakePassTypeItems"
    filter: int
    result: bpy.types.RenderResult


class Viewport(msgspec.Struct, frozen=True, kw_only=True):
    """Viewport update and draw bodies."""

    update: Callable[[bpy.types.Context, bpy.types.Depsgraph], Outcome[None]]
    draw: Callable[[bpy.types.Context, bpy.types.Depsgraph], Outcome[None]]


class Engine(msgspec.Struct, frozen=True, kw_only=True):
    """Render engine built from enabled `bl_` flags, others at their RNA default, and callback bodies, labelled by its key."""

    combined: ClassVar[EnginePass] = EnginePass(name=COMBINED, chanid="RGBA")
    key: Key
    render: Callable[[RenderRequest], Iterable[Outcome[Tile]]]
    enabled: frozenset[Descriptor[bpy.types.BoolProperty]] = frozenset()
    passes: tuple[EnginePass, ...] | Callable[[bpy.types.Scene, bpy.types.ViewLayer], tuple[EnginePass, ...]] = ()
    viewport: Option[Viewport] = Nothing
    bake: Option[Callable[[Baking], Outcome[npt.NDArray[np.float32]]]] = Nothing
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
            """Writes body tiles over the render depsgraph's scene and view layer, reporting each fault at ERROR level."""
            scene: bpy.types.Scene = depsgraph.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            layer: bpy.types.ViewLayer = depsgraph.view_layer  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            request = RenderRequest(engine=engine, depsgraph=depsgraph, scene=scene, layer=layer)
            for _, text in request.written(self.render(request)).swap().map(rendered).default_value(()):
                engine.report({"ERROR"}, text)

        namespace: dict[str, object] = {
            "__module__": identity.package,
            "bl_idname": name,
            "bl_label": bpy.path.display_name(self.key),
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

            def bake(
                engine: bpy.types.RenderEngine, depsgraph: bpy.types.Depsgraph, target: bpy.types.Object, pass_type: "rna_enums.BakePassTypeItems", pass_filter: int, width: int, height: int
            ) -> None:
                """Writes body plane to Combined pass of bake result."""
                if (result := engine.begin_result(0, 0, width, height)) is None:
                    return
                try:
                    baked = body(Baking(engine=engine, depsgraph=depsgraph, target=target, kind=pass_type, filter=pass_filter, result=result)).bind(
                        lambda plane: _filled(result, "", self.combined, plane)
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
    def replayed(cls, identity: Identity, key: Key, planes: Mapping[PlaneKey, Tile], frame: Callable[[], int]) -> Generator[Outcome[str]]:
        """Registers postprocess engine replaying captured planes until scope exit."""

        def matching(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> Iterable[Tile]:
            current = frame()
            return (tile for at, tile in planes.items() if (at.frame, at.scene, at.layer) == (current, scene.session_uid, layer.name))

        def passes(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> tuple[EnginePass, ...]:
            return tuple({held.name: held for tile in matching(scene, layer) for held, _image in tile.planes if held.name != cls.combined.name}.values())

        replay = cls(key=key, render=lambda request: map(Ok, matching(request.scene, request.layer)), passes=passes, enabled=frozenset({POSTPROCESS}))
        with ExitStack() as scope:
            yield identity.name(bpy.types.RenderEngine, key).bind(lambda name: entered(scope, (replay.built(identity, name),)).map(lambda _: name))

    @contextmanager
    def joined(self, name: str) -> Generator[Outcome[None]]:
        """Adds engine to `COMPAT_ENGINES` sets holding a `joins` name until scope exit."""
        panels = tuple(panel.COMPAT_ENGINES for panel in bpy.types.Panel.__subclasses__() if hasattr(panel, "COMPAT_ENGINES") and isinstance(panel.COMPAT_ENGINES, set))
        selected = {id(engines): engines for engines in panels if self.joins.intersection(engines) and name not in engines}
        answer = accumulated(
            Ok(None) if any(identifier in engines for engines in panels) else Error(Fault.of(BlenderReason.ABSENT, "COMPAT_ENGINES", member=identifier)) for identifier in sorted(self.joins)
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

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires engine class, then `COMPAT_ENGINES` memberships."""
        return identity.name(bpy.types.RenderEngine, self.key).map(lambda name: (self.built(identity, name), (Band.INTERFACE, self.joined(name))))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _filled(result: bpy.types.RenderResult, view: str, pass_: EnginePass, plane: npt.NDArray[np.float32]) -> Outcome[None]:
    """Writes a plane, top row first, into the result's pass of that name and view, absent fault for a pass the engine never registered."""
    return (
        Option
        .of_optional(result.layers[0].passes.find_by_name(pass_.name, view))
        .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(result), member=pass_.name))
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

__all__ = ["ENGINE_SAMPLES", "FLAGS", "POSTPROCESS", "Baking", "Engine", "EnginePass", "PlaneKey", "RenderRequest", "Tile", "Viewport", "samples"]
