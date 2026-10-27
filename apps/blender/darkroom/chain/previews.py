"""Chain previews over a render proxy: effect and layer stack tiles, the stage thumbnail of an entry, and the entering frame compositor node thumbnails read."""

from contextlib import nullcontext
from functools import partial, reduce
import itertools
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.ids.content import fingerprinted
from blender.bpy.data.rna.descriptors import root_in, rooted, Struct
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import Handler, updated
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor.blending import Entry
from blender.bpy.render.compositor.effects import CompositorEffect, Context, Enabled, Restoring
from blender.bpy.render.compositor.entries import EffectRows
from blender.bpy.render.compositor.inputs import inputs_of, restored
from blender.bpy.render.compositor.layers import Adjustment, LayerStack, Source
from blender.bpy.render.compositor.stacking import stacked_by, Valued
from blender.bpy.render.outputs.exports import combined, LongestSide
from blender.bpy.render.outputs.framing import FrameSize, Framing
from blender.bpy.render.outputs.results import intermediate, INTERMEDIATES, ResultRead
from blender.bpy.render.previews.queue import BACKLOG, Job, PreviewQueue, Rank
from blender.bpy.render.previews.scenes import PreviewLighting, PreviewScene, PreviewStack, RoutedEntry, Staging
from blender.bpy.ui.layout.rows import Line, RowDraw
from blender.bpy.ui.resources.previews import PreviewKey, PREVIEWS
from blender.bpy.ui.resources.theme import Metrics
from blender.imaging.files.buffers import checked, pixels
from blender.records.faults import accumulated, Outcome, paired
from blender.records.identity import content_hash, ContentHash

from .stack import Chain
from .stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------

type Previewed = Valued[Entry[Effect]] | Valued[LayerStack[Source, Effect]] | Through | Recipe
type Offered = tuple[CompositorEffect[Effect], Valued[Entry[Effect]]]

# --- [MODELS] ---------------------------------------------------------------------------


class Through(msgspec.Struct, frozen=True, kw_only=True):
    """Chain through one named entry, the stage output that entry leaves."""

    name: str


class Recipe(msgspec.Struct, frozen=True, kw_only=True):
    """Entries with their input values in stack order standing in for every mirrored entry, a setup projected over the scene."""

    entries: tuple[Valued[Entry[Effect]], ...]


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


# --- [SERVICES] -------------------------------------------------------------------------


class TileState:
    """Render proxy, requested slots, and held demands of one extension."""

    __slots__ = ("demands", "proxy", "wanted")

    def __init__(self) -> None:
        """Holds no proxy, slot, or demand."""
        self.proxy: Option[Proxied] = Nothing
        self.wanted: dict[str, Wanted] = {}
        self.demands: dict[str, Demand] = {}

    def requested(self, slot: str, request: Wanted) -> None:
        """Records a slot request, keeping an equal held one."""
        if self.wanted.get(slot) != request:
            self.wanted[slot] = request

    def demanded(self, slot: str, demand: Demand) -> Demand:
        """Holds the slot demand, keeping the held one over the same request, content, scene, and config."""
        held = Option.of_optional(self.demands.get(slot)).filter(
            lambda prior: prior.wanted is demand.wanted and prior.content == demand.content and prior.scene == demand.scene and prior.config == demand.config
        )
        self.demands[slot] = kept = held.default_value(demand)
        return kept

    def holds(self, slot: str, demand: Demand) -> bool:
        """Whether the slot still holds the demand and its request."""
        return self.demands.get(slot) is demand and self.wanted.get(slot) is demand.wanted


TILES: Final = Resource[TileState](made=lambda _identity: nullcontext(TileState()), renewed=Some(lambda _held: TileState()))


class Tiles(msgspec.Struct, frozen=True, kw_only=True):
    """Chain previews in the preview scene over the render proxy, the formation alone ahead of the previewed record while `isolated`."""

    chain: Chain
    isolated: bool = False

    # --- [STACKS]

    def formed(self, identity: Identity, mirror: RoutedEntry) -> bool:
        """Whether the formation row owns a mirrored entry."""
        return self.chain.rows.row(identity, mirror.entry).filter(lambda row: row is self.chain.formation).is_some()

    def mirrors(self, identity: Identity, scene: bpy.types.Scene) -> tuple[RoutedEntry, ...]:
        """Returns preview-enabled entries, the formation alone while isolated."""
        held = RoutedEntry.of(scene, Enabled.PREVIEW)
        return tuple(mirror for mirror in held if self.formed(identity, mirror)) if self.isolated else held

    def stacked(self, identity: Identity, scene: bpy.types.Scene, record: Previewed, head: Option[npt.NDArray[np.float32]]) -> Outcome[PreviewStack]:
        """Builds the preview stack: an effect replacing the first entry of its type or single stage, a layer stack's adjustment effects replacing the head stack, a recipe replacing every entry, or the entries through a stage, formed through the scene view without a formation."""
        rows, formation, mirrors = self.chain.rows, self.chain.formation, self.mirrors(identity, scene)

        def offered(entries: tuple[Valued[Entry[Effect]], ...], kept: tuple[RoutedEntry, ...]) -> Outcome[PreviewStack]:
            formed = any(self.formed(identity, mirror) for mirror in kept) or any(type(entry.structure.record) is formation.record for entry in entries)
            seeded = Ok(()) if formed else formation.seeded(scene).map(lambda view: (Valued(structure=Entry(record=view)),))
            return seeded.bind(lambda opening: accumulated(rows.offering(type(entry.structure.record)).map(lambda row: (row, entry)) for entry in (*opening, *entries))).map(
                lambda found: PreviewStack(head=head, mirrored=kept, placing=partial(placed, rows, found))
            )

        match record:
            case Through(name=name):
                through = (*itertools.takewhile(lambda entry: entry.name != name, scene.compositor_effects), *(entry for entry in scene.compositor_effects if entry.name == name))
                return offered((), tuple(mirror for mirror in mirrors if mirror.entry in through))
            case Recipe(entries=entries):
                return offered(entries, ())
            case Valued(structure=structure, values=values, prefix=prefix):
                match structure:
                    case LayerStack():
                        held = record.rebased("").to_optional()
                        layered = tuple(
                            Valued(structure=entry, values=held, prefix=item_key(layer.key, "effects", index))
                            for layer, _ in structure.composited()
                            if isinstance(layer.source, Adjustment)
                            for index, entry in enumerate(layer.effects, 1)
                        )
                        return offered(layered, tuple(mirror for mirror in mirrors if stacked_by(identity, mirror.entry).is_none()))
                    case Entry(record=effect):
                        kind = type(effect)
                        replaced = next(
                            (
                                mirror
                                for mirror in mirrors
                                if rows.row(identity, mirror.entry).filter(lambda row: row.record is kind or (kind.stage in Stage.single and row.record.stage is kind.stage)).is_some()
                            ),
                            None,
                        )
                        return offered((Valued(structure=structure, values=values, prefix=prefix),), tuple(mirror for mirror in mirrors if mirror is not replaced))

    def parts(self, identity: Identity, scene: bpy.types.Scene) -> Outcome[Option[tuple[msgspec.Struct | ContentHash, ...]]]:
        """Reads the scene view, proxy key, mirrored entry records, input values, and group fingerprints, `Nothing` for a group without one."""

        def captured(mirror: RoutedEntry) -> Outcome[Option[tuple[msgspec.Struct | ContentHash, ...]]]:
            records = self.chain.rows.row(identity, mirror.entry).map(lambda row: row.read(identity, mirror.entry).map(lambda record: (record,))).default_value(Ok(()))
            return paired(records, inputs_of(mirror.entry).bind(Snapshot.capture), fingerprinted(mirror.group)).map(lambda found: found[2].map(lambda key: (*found[0], found[1], key)))

        head: Option[tuple[msgspec.Struct | ContentHash, ...]] = Some((viewed(scene), *(proxy.key for proxy in identity.held[TILES].proxy.to_list())))
        return accumulated(map(captured, self.mirrors(identity, scene))).map(lambda values: reduce(lambda held, part: held.map2(lambda prior, current: (*prior, *current), part), values, head))

    def rendered(self, identity: Identity, record: Previewed, side: int, *, scene: bpy.types.Scene) -> Outcome[npt.NDArray[np.float32]]:
        """Renders a record over the render proxy fitted to side, the source's updates during the render marked as its own."""
        lighting, proxy = PreviewLighting(), identity.held[TILES].proxy

        def staged(head: Option[npt.NDArray[np.float32]]) -> Outcome[npt.NDArray[np.float32]]:
            staging = paired(self.stacked(identity, scene, record, head), lighting.environment).map(
                lambda pair: Staging(
                    source=scene,
                    size=head.map(lambda plane: (plane.shape[1], plane.shape[0])).default_value((side, side)),
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

    # --- [DEMAND]

    @staticmethod
    def icon(identity: Identity, slot: str, record: Previewed, scale: Option[float] = Nothing) -> Option[int]:
        """Records the slot demand at its scale, Blender's preview render size by default, and returns the published slot icon."""
        request = Wanted(record=record, scale=scale.default_with(lambda: bpy.app.render_preview_size / Metrics.read().unit))
        identity.held[TILES].requested(slot, request)
        return identity.held[PREVIEWS].icon_id(slot)

    def jobs(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Job, ...]]:
        """Returns a job per stale slot from one config and entry read."""
        state, previews = identity.held[TILES], identity.held[PREVIEWS]

        def requests(scene: bpy.types.Scene, parts: Option[tuple[msgspec.Struct | ContentHash, ...]], config: PyOpenColorIO.Config) -> tuple[Job, ...]:
            cache = config.getCacheID()

            def job(slot: str, request: Wanted) -> tuple[Job, ...]:
                content = parts.map(lambda values: content_hash(Tiles, request.record, *values, config=Some(cache)))
                demand = state.demanded(slot, Demand(wanted=request, content=content, scene=scene, config=cache))
                key = content.default_value(demand)

                def rendered() -> Outcome[None]:
                    return previews.tile(
                        slot, key, lambda side: self.rendered(identity, request.record, side, scene=scene).map(lambda plane: Some(Some(plane)) if state.holds(slot, demand) else Nothing), request.scale
                    ).map(lambda _: None)

                return () if previews.current(slot, key, request.scale) else (Job(place=(Tiles, slot), key=PreviewKey.read(key, request.scale), rank=Rank.SHOWN, run=rendered),)

            return tuple(queued for slot, request in state.wanted.items() for queued in job(slot, request))

        return rooted(bpy.types.Scene, context).bind(lambda scene: self.parts(identity, scene).map(lambda parts: requests(scene, parts, PyOpenColorIO.GetCurrentConfig()))) if state.wanted else Ok(())

    # --- [LIFECYCLE]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns tile state, previews, queue feed, and the render proxy reader."""

        def consumed(scene: bpy.types.Scene, path: str) -> Outcome[Option[str]]:
            bound = root_in(bpy.types.WindowManager, bpy.context).bind(
                lambda manager: Option.of_optional(max((area.width for window in manager.windows for area in window.screen.areas), default=None))
            )
            camera = FrameSize().bound(identity).bind(lambda frame: paired(frame.read(scene), frame.base(scene)).map(lambda pair: pair[1].default_value(pair[0])))

            def stored(plane: npt.NDArray[np.float32], framing: Framing, proxy: npt.NDArray[np.float32]) -> Option[str]:
                fitted_camera = scaled(framing, plane.shape[:2], proxy.shape[:2])
                identity.held[TILES].proxy = Some(Proxied(plane=proxy, framing=fitted_camera, key=content_hash(Proxied, fitted_camera, arrays=(proxy,))))
                return Nothing

            return paired(rooted(bpy.types.ViewLayer, bpy.context).bind(lambda layer: combined(path, Some(layer.name))).map(lambda read: read[1]), camera).bind(
                lambda pair: bound.map(lambda side: fitted(pair[0], side)).default_value(Ok(pair[0])).map(lambda proxy: stored(*pair, proxy))
            )

        def invalidated(scene: bpy.types.Scene, *_: object) -> Outcome[None]:
            if scene.session_uid not in identity.held[INTERMEDIATES]:
                identity.held[TILES].demands.clear()
            return Ok(None)

        return Ok((
            TILES,
            BACKLOG,
            PREVIEWS,
            (Band.RESOURCES, PreviewQueue.fed(identity, self.jobs)),
            PreviewScene(),
            FrameSize.attachment,
            ResultRead(consume=consumed),
            Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.ID,), invalidated)),
            Handler(handlers=bpy.app.handlers.frame_change_post, body=invalidated),
        ))


class StageThumbnail(Line, frozen=True, kw_only=True):
    """Inline tile of the stage output an entry leaves, as wide as its region."""

    tiles: Tiles

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns the tiles rendering the stage."""
        return Ok((self.tiles,))

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Records the entry's stage demand and draws its published icon."""

        def shown(layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
            match holder, context.region:
                case bpy.types.SceneCompositorEffect(id_data=bpy.types.Scene() as scene, name=name), bpy.types.Region(width=width):
                    scale = width / Metrics.read().unit
                    for icon in Tiles.icon(identity, f"stage {scene.session_uid} {name}", Through(name=name), Some(scale)).to_list():
                        layout.template_icon(icon_value=icon, scale=scale)
                case _:
                    pass

        return Ok(self.conditioned(shown))


# --- [OPERATIONS] -----------------------------------------------------------------------


def placed(rows: EffectRows[Effect], offered: tuple[Offered, ...], identity: Identity, _source: bpy.types.Scene, target: bpy.types.Scene, context: Context) -> Outcome[Restoring]:
    """Places offered entries with their input values after the mirrored ones, answering the removal of every entry it placed."""

    def valued(row: CompositorEffect[Effect], entry: Valued[Entry[Effect]]) -> Outcome[bpy.types.SceneCompositorEffect]:
        return rows.placed(identity, target, row, entry.structure, context=context).bind(
            lambda held: entry.rebased("").map(lambda values: restored(identity, held, values)).default_value(Ok(False)).map(lambda _: held)
        )

    return accumulated(itertools.starmap(valued, offered)).map(lambda held: partial(removed, rows, identity, target, tuple(entry.name for entry in held)))


def removed(rows: EffectRows[Effect], identity: Identity, scene: bpy.types.Scene, names: tuple[str, ...]) -> Outcome[None]:
    """Removes the named preview entries."""
    return accumulated(rows.removed(identity, scene, name) for name in names).map(lambda _: None)


def fitted(plane: npt.NDArray[np.float32], side: int) -> Outcome[npt.NDArray[np.float32]]:
    """Shrinks linear pixels so the longest side fits `side`."""

    def resized(size: tuple[int, int]) -> Outcome[npt.NDArray[np.float32]]:
        return checked(OpenImageIO.ImageBufAlgo.resize(OpenImageIO.ImageBuf(plane), roi=OpenImageIO.ROI(0, size[0], 0, size[1], 0, 1, 0, plane.shape[2]))).bind(pixels)

    return LongestSide(pixels=side).fitted(plane.shape[1], plane.shape[0]).map(resized).default_value(Ok(plane))


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
