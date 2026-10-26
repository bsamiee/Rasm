"""Chain previews over a render proxy: effect and layer stack tiles, the stage thumbnail of an entry, and the entering frame compositor node thumbnails read."""

from contextlib import nullcontext
from functools import reduce
import itertools
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.ids.images import Bounded
from blender.bpy.data.rna.descriptors import root_in, rooted
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import SceneHandler, UpdateHandler
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.trees import fingerprinted
from blender.bpy.render.color.spaces import config_of, viewed
from blender.bpy.render.compositor.blending import Entry
from blender.bpy.render.compositor.effects import CompositorEffect, Context, Enabled, Named, Restoring
from blender.bpy.render.compositor.inputs import inputs_of
from blender.bpy.render.compositor.layers import Adjustment, LayerStack, Source
from blender.bpy.render.compositor.stacking import stacked_by
from blender.bpy.render.outputs.exports import combined
from blender.bpy.render.outputs.framing import FrameSize, Framing
from blender.bpy.render.outputs.results import intermediate, INTERMEDIATES, ResultRead
from blender.bpy.render.previews.nodes import Entering
from blender.bpy.render.previews.queue import BACKLOG, Job, PreviewQueue, Rank
from blender.bpy.render.previews.scenes import EntryPlacement, Lighting, Mirrored, PreviewEngine, PreviewScene, PreviewStack, Staging
from blender.bpy.ui.layout.rows import Line, RowDraw
from blender.bpy.ui.resources.previews import PreviewKey, PREVIEWS
from blender.bpy.ui.resources.theme import Metrics
from blender.imaging.files.images import checked, pixels
from blender.records.faults import accumulated, paired
from blender.records.identity import content_hash, ContentHash

from .stack import Chain
from .stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------

type Previewed = Effect | LayerStack[Source, Effect] | Through | Recipe
type Offered = tuple[CompositorEffect[Effect], Entry[Effect]]

# --- [MODELS] ---------------------------------------------------------------------------


class Through(Named, frozen=True, kw_only=True):
    """Chain through one named entry, the stage output that entry leaves."""


class Recipe(msgspec.Struct, frozen=True, kw_only=True):
    """Entries in stack order standing in for every mirrored entry, a setup projected over the scene."""

    entries: tuple[Entry[Effect], ...]


class Proxied(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Linear render pixels, camera framing at pixel scale, and content key."""

    plane: npt.NDArray[np.float32]
    framing: Framing
    key: ContentHash


class Wanted(msgspec.Struct, frozen=True, kw_only=True):
    """Tile slot's requested effect, layer stack, or stage, and its scale in widget units."""

    record: Previewed
    scale: float


class Demand(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Held tile request with its content hash, scene, and config."""

    wanted: Wanted
    content: Option[ContentHash]
    scene: bpy.types.Scene
    config: str


class TileState(msgspec.Struct, kw_only=True):
    """Render proxy, requested slots, and held demands."""

    proxy: Option[Proxied] = Nothing
    wanted: dict[str, Wanted] = msgspec.field(default_factory=dict)
    demands: dict[str, Demand] = msgspec.field(default_factory=dict)


# --- [SERVICES] -------------------------------------------------------------------------

TILES: Final = Scoped[TileState](made=lambda _identity: nullcontext(TileState()), renewed=Some(lambda _held: TileState()))


class Tiles(msgspec.Struct, frozen=True, kw_only=True):
    """Chain previews in the preview scene over the render proxy, the formation alone ahead of the previewed record while `isolated`."""

    chain: Chain
    isolated: bool = False

    # --- [STACKS]

    def formed(self, identity: Identity, mirror: Mirrored) -> bool:
        """Whether the formation row owns a mirrored entry."""
        return self.chain.rows.row(identity, mirror.entry).filter(lambda row: row is self.chain.formation).is_some()

    def mirrors(self, identity: Identity, scene: bpy.types.Scene) -> tuple[Mirrored, ...]:
        """Returns preview-enabled entries, the formation alone while isolated."""
        held = Mirrored.of(scene, Enabled.PREVIEW)
        return tuple(mirror for mirror in held if self.formed(identity, mirror)) if self.isolated else held

    def stacked(self, identity: Identity, scene: bpy.types.Scene, record: Previewed, head: Option[npt.NDArray[np.float32]]) -> BlenderResult[PreviewStack]:
        """Builds the preview stack: an effect replacing the first entry of its type or single stage, a layer stack's adjustment effects replacing the head stack, a recipe replacing every entry, or the entries through a stage, formed through the scene view without a formation."""
        rows, formation, mirrors = self.chain.rows, self.chain.formation, self.mirrors(identity, scene)

        def placing(offered: tuple[Offered, ...]) -> EntryPlacement:
            def place(placer: Identity, _source: bpy.types.Scene, target: bpy.types.Scene, context: Context) -> BlenderResult[Restoring]:
                def restoring(names: tuple[str, ...]) -> Restoring:
                    return lambda: accumulated(rows.removed(placer, target, name) for name in names).map(lambda _: None)

                return accumulated(rows.placed(placer, target, row, entry, context=context) for row, entry in offered).map(lambda placed: restoring(tuple(held.name for held in placed)))

            return place

        def offered(entries: tuple[Entry[Effect], ...], kept: tuple[Mirrored, ...]) -> BlenderResult[PreviewStack]:
            formed = any(self.formed(identity, mirror) for mirror in kept) or any(type(entry.record) is formation.record for entry in entries)
            seeded = Ok(()) if formed else formation.seeded(scene).map(lambda view: (Entry(record=view),))
            return seeded.bind(lambda opening: accumulated(rows.offering(type(entry.record)).map(lambda row: (row, entry)) for entry in (*opening, *entries))).map(
                lambda found: PreviewStack(head=head, mirrored=kept, placing=placing(found))
            )

        match record:
            case Through(name=name):
                through = (*itertools.takewhile(lambda entry: entry.name != name, scene.compositor_effects), *(entry for entry in scene.compositor_effects if entry.name == name))
                return offered((), tuple(mirror for mirror in mirrors if mirror.entry in through))
            case Recipe(entries=entries):
                return offered(entries, ())
            case LayerStack():
                entries = tuple(gated.value for layer, _ in record.composited() if isinstance(layer.source, Adjustment) and layer.effects.enabled for gated in layer.effects.value if gated.enabled)
                return offered(entries, tuple(mirror for mirror in mirrors if stacked_by(identity, mirror.entry).is_none()))
            case _:
                kind = type(record)
                replaced = next(
                    (
                        mirror
                        for mirror in mirrors
                        if rows.row(identity, mirror.entry).filter(lambda row: row.record is kind or (kind.stage in Stage.single and row.record.stage is kind.stage)).is_some()
                    ),
                    None,
                )
                return offered((Entry(record=record),), tuple(mirror for mirror in mirrors if mirror is not replaced))

    def parts(self, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[Option[tuple[msgspec.Struct | ContentHash, ...]]]:
        """Reads the scene view, proxy key, mirrored entry records, and group fingerprints, `Nothing` for a group without one."""

        def captured(mirror: Mirrored) -> BlenderResult[Option[tuple[msgspec.Struct | ContentHash, ...]]]:
            values = (
                self.chain.rows
                .row(identity, mirror.entry)
                .map(lambda row: row.read(identity, mirror.entry).map(lambda record: (record,)))
                .default_with(lambda: inputs_of(mirror.entry).bind(Snapshot.capture).map(lambda snapshot: (snapshot,)))
            )
            return paired(values, fingerprinted(mirror.group)).map(lambda found: found[1].map(lambda key: (*found[0], key)))

        head: Option[tuple[msgspec.Struct | ContentHash, ...]] = Some((viewed(scene), *(proxy.key for proxy in TILES.of(identity).proxy.to_list())))
        return accumulated(map(captured, self.mirrors(identity, scene))).map(lambda values: reduce(lambda held, part: held.map2(lambda prior, current: (*prior, *current), part), values, head))

    def rendered(self, identity: Identity, record: Previewed, side: int, *, scene: bpy.types.Scene) -> BlenderResult[npt.NDArray[np.float32]]:
        """Renders a record over the render proxy fitted to side, the source's updates during the render marked as its own."""
        lighting, proxy = Lighting(), TILES.of(identity).proxy

        def staged(head: Option[npt.NDArray[np.float32]]) -> BlenderResult[npt.NDArray[np.float32]]:
            staging = paired(self.stacked(identity, scene, record, head), lighting.environment).map(
                lambda pair: Staging(
                    source=scene,
                    size=head.map(lambda plane: (plane.shape[1], plane.shape[0])).default_value((side, side)),
                    engine=PreviewEngine.of(scene),
                    world=pair[1],
                    lamp=lighting.lamp,
                    stack=pair[0],
                    surface=Nothing if head.is_some() else Some(lighting.surface),
                    context=proxy.map2(framed, head),
                )
            )
            return staging.bind(lambda desired: PreviewScene().rendered(identity, desired)).map(lambda readback: readback.plane)

        with intermediate(identity, scene):
            return proxy.map(lambda held: fitted(held.plane, side).map(Some)).default_value(Ok(Nothing)).bind(staged)

    def entering(self, identity: Identity, _scene: bpy.types.Scene) -> Option[Entering]:
        """Render proxy compositor node thumbnails read, fitted to their side with its camera framing."""
        return TILES.of(identity).proxy.map(lambda held: Entering(key=held.key, fitted=lambda side: fitted(held.plane, side).map(lambda plane: (plane, Some(framed(held, plane))))))

    # --- [DEMAND]

    @staticmethod
    def icon(identity: Identity, slot: str, record: Previewed, scale: Option[float] = Nothing) -> Option[int]:
        """Records the slot demand at its scale, Blender's preview render size by default, and returns the published slot icon."""
        request = Wanted(record=record, scale=scale.default_with(lambda: bpy.app.render_preview_size / Metrics.read().unit))
        if (wanted := TILES.of(identity).wanted).get(slot) != request:
            wanted[slot] = request
        return PREVIEWS.of(identity).icon_id(slot)

    def jobs(self, identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[Job, ...]]:
        """Returns a job per stale slot from one config and entry read."""
        state, previews = TILES.of(identity), PREVIEWS.of(identity)

        def requests(scene: bpy.types.Scene, parts: Option[tuple[msgspec.Struct | ContentHash, ...]], config: PyOpenColorIO.Config) -> tuple[Job, ...]:
            cache = config.getCacheID()

            def job(slot: str, request: Wanted) -> tuple[Job, ...]:
                content = parts.map(lambda values: content_hash(Tiles, request.record, *values, config=Some(cache)))
                demand = (
                    Option
                    .of_optional(state.demands.get(slot))
                    .filter(lambda prior: prior.wanted is request and prior.content == content and prior.scene == scene and prior.config == cache)
                    .default_with(lambda: Demand(wanted=request, content=content, scene=scene, config=cache))
                )
                state.demands[slot] = demand
                key = content.default_value(demand)

                def rendered() -> BlenderResult[None]:
                    return previews.tile(
                        slot,
                        key,
                        lambda side: self.rendered(identity, request.record, side, scene=scene).map(
                            lambda plane: Some(Some(plane)) if state.demands.get(slot) is demand and state.wanted.get(slot) is request else Nothing
                        ),
                        request.scale,
                    ).map(lambda _: None)

                return () if previews.current(slot, key, request.scale) else (Job(place=(Tiles, slot), key=PreviewKey.read(key, request.scale), rank=Rank.SHOWN, run=rendered),)

            return tuple(queued for slot, request in state.wanted.items() for queued in job(slot, request))

        return rooted(bpy.types.Scene, context).bind(lambda scene: paired(self.parts(identity, scene), config_of()).map(lambda pair: requests(scene, *pair))) if state.wanted else Ok(())

    # --- [LIFECYCLE]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns tile state, previews, queue feed, and the render proxy reader."""

        def consumed(scene: bpy.types.Scene, path: str) -> BlenderResult[Option[str]]:
            bound = root_in(bpy.types.WindowManager, bpy.context).bind(
                lambda manager: Option.of_optional(max((area.width for window in manager.windows for area in window.screen.areas), default=None))
            )
            camera = FrameSize().bound(identity).bind(lambda frame: paired(frame.read(scene), frame.base(scene)).map(lambda pair: pair[1].default_value(pair[0])))

            def stored(plane: npt.NDArray[np.float32], framing: Framing, proxy: npt.NDArray[np.float32]) -> Option[str]:
                fitted_camera = scaled(framing, plane.shape[:2], proxy.shape[:2])
                TILES.of(identity).proxy = Some(Proxied(plane=proxy, framing=fitted_camera, key=content_hash(Proxied, fitted_camera, arrays=(proxy,))))
                return Nothing

            return paired(combined(path, Some(scene.view_layers[0].name)).map(lambda read: read[1]), camera).bind(
                lambda pair: bound.map(lambda side: fitted(pair[0], side)).default_value(Ok(pair[0])).map(lambda proxy: stored(*pair, proxy))
            )

        def invalidated(scene: bpy.types.Scene, _updates: tuple[bpy.types.DepsgraphUpdate, ...] = ()) -> BlenderResult[None]:
            if scene.session_uid not in INTERMEDIATES.of(identity):
                TILES.of(identity).demands.clear()
            return Ok(None)

        return Ok((
            TILES,
            BACKLOG,
            PREVIEWS,
            (Band.RESOURCES, PreviewQueue.fed(identity, self.jobs)),
            PreviewScene(),
            FrameSize.attachment,
            ResultRead(consume=consumed),
            UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.ID,), body=invalidated),
            SceneHandler(handlers=bpy.app.handlers.frame_change_post, body=invalidated),
        ))


class StageThumbnail(Line, frozen=True, kw_only=True):
    """Inline tile of the stage output an entry leaves, as wide as its region."""

    tiles: Tiles

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns the tiles rendering the stage."""
        return Ok((self.tiles,))

    def drawn(self, identity: Identity, /) -> BlenderResult[RowDraw]:
        """Records the entry's stage demand and draws its published icon."""

        def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            match holder, context.region:
                case bpy.types.SceneCompositorEffect(id_data=bpy.types.Scene() as scene, name=name), bpy.types.Region(width=width):
                    scale = width / Metrics.read().unit
                    for icon in Tiles.icon(identity, f"stage {scene.session_uid} {name}", Through(name=name), Some(scale)).to_list():
                        layout.template_icon(icon_value=icon, scale=scale)
                case _:
                    pass

        return Ok(shown)


# --- [OPERATIONS] -----------------------------------------------------------------------


def fitted(plane: npt.NDArray[np.float32], side: int) -> BlenderResult[npt.NDArray[np.float32]]:
    """Shrinks linear pixels so the longest side fits `side`."""

    def resized(size: tuple[int, int]) -> BlenderResult[npt.NDArray[np.float32]]:
        return checked(OpenImageIO.ImageBufAlgo.resize(OpenImageIO.ImageBuf(plane), roi=OpenImageIO.ROI(0, size[0], 0, size[1], 0, 1, 0, plane.shape[2]))).bind(pixels)

    return Bounded(pixels=side).fitted(plane.shape[1], plane.shape[0]).map(resized).default_value(Ok(plane))


def scaled(framing: Framing, stored: tuple[int, int], target: tuple[int, int]) -> Framing:
    """Scales camera framing by the target-to-stored longest side ratio."""
    numerator, denominator = max(target), max(stored)
    width, height = framing.size
    return msgspec.structs.replace(
        framing, resolution_x=(width * numerator + denominator - 1) // denominator, resolution_y=(height * numerator + denominator - 1) // denominator, resolution_percentage=100
    )


def framed(proxy: Proxied, plane: npt.NDArray[np.float32]) -> tuple[tuple[int, int], Framing]:
    """Frame context of a plane fitted from the proxy: its pixel size and the proxy framing scaled to it."""
    return (plane.shape[0], plane.shape[1]), scaled(proxy.framing, proxy.plane.shape[:2], plane.shape[:2])


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TILES", "Demand", "Offered", "Previewed", "Proxied", "Recipe", "StageThumbnail", "Through", "TileState", "Tiles", "Wanted", "fitted", "framed", "scaled"]
