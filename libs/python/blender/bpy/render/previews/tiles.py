"""Effect preview tiles over a render proxy: an offered effect, a layer stack's adjustment effects, the output an entry leaves, or a set of entries, each rendered in the preview scene through a closing effect or the scene view, demanded per slot at draw and fed into the preview queue."""

from contextlib import nullcontext
from fractions import Fraction
from functools import cache, partial, reduce
import itertools
from typing import Any, Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.images import fingerprinted
from blender.bpy.data.rna.paths import root_in, rooted, Struct
from blender.bpy.extension.events import Handler, updated
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.nodes.desired import item_key
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor.effects import CompositorEffect, Entry
from blender.bpy.render.compositor.entries import EffectRows
from blender.bpy.render.compositor.inputs import PreviewFrame, restored
from blender.bpy.render.compositor.layers import Adjustment, EFFECTS, LayerStack
from blender.bpy.render.compositor.placement import Enabled
from blender.bpy.render.compositor.recipes import Valued
from blender.bpy.render.compositor.stacking import stacked_by
from blender.bpy.render.outputs.framing import FrameSize, Framing
from blender.bpy.render.outputs.results import combined, intermediate, INTERMEDIATES, ResultRead
from blender.bpy.render.previews.queue import Demand, Demands, Job, PreviewFeed, Rank
from blender.bpy.render.previews.scenes import PreviewLighting, PreviewScene, PreviewStack, Restoring, RoutedEntry, Staging
from blender.bpy.ui.layout.rows import Line, RowDraw
from blender.bpy.ui.resources.previews import PreviewKey, PREVIEWS
from blender.bpy.ui.resources.theme import Metrics
from blender.imaging.files.buffers import fitted
from blender.records.faults import accumulated, Faults, Outcome, paired, Reason
from blender.records.fields import Case
from blender.records.stores import content_hash, ContentHash

# --- [TYPES] ----------------------------------------------------------------------------

type Tiled[R: Case] = Valued[Entry[R]] | Valued[LayerStack[R]] | Through | Recipe[R]
type _Parts = Option[tuple[msgspec.Struct | ContentHash, ...]]
type _Basis = tuple[Tiled[Any], int, bpy.types.Scene, str]

# --- [MODELS] ---------------------------------------------------------------------------


class Through(msgspec.Struct, frozen=True, kw_only=True):
    """Stack through one named entry, the output that entry leaves."""

    name: str


class Recipe[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Entries with their input values in stack order standing in for every mirrored entry."""

    entries: tuple[Valued[Entry[R]], ...]


class _Proxy(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Linear pixels of the last finished render fitted to the widest area, its camera framing at that scale, and their content hash."""

    plane: npt.NDArray[np.float32]
    framing: Framing
    key: ContentHash

    def framed(self, plane: npt.NDArray[np.float32]) -> PreviewFrame:
        """Preview frame of a plane fitted from the proxy: its pixel size and the proxy framing scaled to it."""
        return PreviewFrame(size=(plane.shape[0], plane.shape[1]), framing=self.framing.scaled(Fraction(max(plane.shape[:2]), max(self.plane.shape[:2]))))


# --- [SERVICES] -------------------------------------------------------------------------


class _TileState:
    """Render proxy of one extension, the tiles and tile row each slot requests, and the demand each slot holds."""

    __slots__ = ("demands", "proxy", "requests")

    def __init__(self) -> None:
        """Holds no proxy, request, or demand."""
        self.proxy: Option[_Proxy] = Nothing
        self.requests: dict[str, tuple[EffectTiles[Any], Tiled[Any], int]] = {}
        self.demands: Demands[str, _Basis] = Demands()


_TILES: Final = Resource[_TileState](made=lambda _identity: nullcontext(_TileState()), renewed=Some(lambda _held: _TileState()))


class EffectTiles[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Preview tiles of offered effects over the render proxy, each stack closing through `closing` where one is given and through the scene view alone otherwise, the closing effect's entries alone preceding the tiled record while `isolated`.

    `icon` records a slot's tile at draw and answers its published icon, serving layer stack looks and gallery items alike, and the preview queue renders each tile its slot holds no current icon of.
    Slots name tiles across the extension.
    """

    rows: EffectRows[R]
    closing: Option[CompositorEffect[R]] = Nothing
    isolated: bool = False
    preview: PreviewScene = PreviewScene()

    # --- [STACKS]

    def closes(self, identity: Identity, mirror: RoutedEntry) -> bool:
        """Whether the closing effect owns a mirrored entry."""
        return self.closing.map(lambda closing: self.rows.row(identity, mirror.entry).filter(lambda row: row == closing).is_some()).default_value(False)

    def mirrors(self, identity: Identity, scene: bpy.types.Scene) -> tuple[RoutedEntry, ...]:
        """Returns preview-enabled entries, the closing effect's alone while isolated."""
        held = RoutedEntry.of(scene, Enabled.PREVIEW)
        return tuple(mirror for mirror in held if self.closes(identity, mirror)) if self.isolated else held

    def stacked(self, identity: Identity, scene: bpy.types.Scene, record: Tiled[R], head: Option[npt.NDArray[np.float32]]) -> Outcome[PreviewStack]:
        """Builds a tiled record's preview stack: an effect replacing its own or a rival's entry, adjustment effects the layer stacks, a recipe every entry, an entry output keeping entries through it, a seeded closing entry where none stands."""
        mirrors = self.mirrors(identity, scene)

        def offered(entries: tuple[Valued[Entry[R]], ...], kept: tuple[RoutedEntry, ...]) -> Outcome[PreviewStack]:
            def opened(closing: CompositorEffect[R]) -> Outcome[tuple[Valued[Entry[R]], ...]]:
                closed = any(self.closes(identity, mirror) for mirror in kept) or any(type(entry.structure.record) is closing.record for entry in entries)
                return Ok(()) if closed else closing.seeded(scene).map(lambda seed: (Valued(structure=Entry(record=seed)),))

            opening: Outcome[tuple[Valued[Entry[R]], ...]] = self.closing.map(opened).default_value(Ok(()))
            return opening.bind(lambda first: accumulated(self.rows.offering(type(entry.structure.record)).map(lambda row: (row, entry)) for entry in (*first, *entries))).map(
                lambda found: PreviewStack(head=head, mirrored=kept, placing=partial(_placed, self.rows, found))
            )

        def rival(effect: R, mirror: RoutedEntry) -> bool:
            chosen = self.rows.offering(type(effect)).to_option()
            return self.rows.row(identity, mirror.entry).filter(lambda row: chosen.filter(lambda own: row == own or self.rows.rivals(own, row)).is_some()).is_some()

        match record:
            case Through(name=name):
                through = (*itertools.takewhile(lambda entry: entry.name != name, scene.compositor_effects), *(entry for entry in scene.compositor_effects if entry.name == name))
                return offered((), tuple(mirror for mirror in mirrors if mirror.entry in through))
            case Recipe(entries=entries):
                return offered(entries, ())
            case Valued(structure=structure, values=values, prefix=prefix, enables=enables):
                match structure:
                    case LayerStack():
                        held = record.rebased("").to_optional()
                        layered = tuple(
                            Valued(structure=entry, values=held, prefix=item_key(layer.key, EFFECTS, index))
                            for layer, _ in structure.composited()
                            if isinstance(layer.source, Adjustment)
                            for index, entry in enumerate(layer.effects, 1)
                        )
                        return offered(layered, tuple(mirror for mirror in mirrors if stacked_by(identity, mirror.entry.node_group).is_none()))
                    case Entry(record=effect):
                        replaced = Block.of_seq(mirrors).filter(partial(rival, effect)).try_head()
                        kept = tuple(mirror for mirror in mirrors if replaced.filter(lambda held: held is mirror).is_none())
                        return offered((Valued(structure=structure, values=values, prefix=prefix, enables=enables),), kept)

    def parts(self, identity: Identity, scene: bpy.types.Scene) -> Outcome[_Parts]:
        """Reads the scene view, proxy key, and each mirrored entry's record and fingerprint, `Nothing` for an entry without one."""

        def captured(mirror: RoutedEntry) -> Outcome[_Parts]:
            records = self.rows.row(identity, mirror.entry).map(lambda row: row.read(identity, mirror.entry).map(lambda record: (record,))).default_value(Ok(()))
            return paired(records, fingerprinted(mirror.entry)).map(lambda pair: pair[1].map(lambda key: (*pair[0], key)))

        head: _Parts = Some((viewed(scene), *(proxy.key for proxy in identity.held[_TILES].proxy.to_list())))
        return accumulated(map(captured, self.mirrors(identity, scene))).map(lambda values: reduce(lambda held, part: held.map2(lambda prior, current: (*prior, *current), part), values, head))

    def rendered(self, identity: Identity, record: Tiled[R], side: int, scene: bpy.types.Scene) -> Outcome[npt.NDArray[np.float32]]:
        """Renders a tiled record over the render proxy fitted to `side`, over the preview surface without a proxy, the source's updates during the render marked as its own."""
        lighting, proxy = PreviewLighting(), identity.held[_TILES].proxy

        def staged(head: Option[npt.NDArray[np.float32]]) -> Outcome[npt.NDArray[np.float32]]:
            staging = paired(self.stacked(identity, scene, record, head), lighting.environment).map(
                lambda pair: Staging(
                    source=scene,
                    size=head.map(lambda plane: (plane.shape[1], plane.shape[0])).default_value((side, side)),
                    world=pair[1],
                    lamp=lighting.lamp,
                    stack=pair[0],
                    surface=Nothing if head.is_some() else Some(lighting.surface),
                    preview=proxy.map2(_Proxy.framed, head),
                )
            )
            return staging.bind(lambda desired: self.preview.rendered(identity, desired)).map(lambda readback: readback.plane)

        with intermediate(identity, scene):
            return proxy.map(lambda held: fitted(held.plane, (side, side)).map(Some)).default_value(Ok(Nothing)).bind(staged)

    # --- [DEMAND]

    def icon(self, identity: Identity, slot: str, record: Tiled[R], /, side: Option[int] = Nothing) -> Option[int]:
        """Records the slot's tile at its side in pixels, Blender's preview render size by default, and returns the slot's published icon."""
        identity.held[_TILES].requests[slot] = (self, record, side.default_with(lambda: bpy.app.render_preview_size))
        return identity.held[PREVIEWS].icon_id(slot)

    def jobs(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Job, ...]]:
        """Returns a job per requested slot holding no current icon, its demand kept while the tile, scene, and config stand until a change outside a preview render or a new render proxy drops it, the stack parts read once per step only for a slot holding no demand."""
        state, previews = identity.held[_TILES], identity.held[PREVIEWS]
        requested = tuple((slot, record, side) for slot, (owner, record, side) in state.requests.items() if owner == self)

        def planned(scene: bpy.types.Scene, config: str) -> Outcome[tuple[Job, ...]]:
            parts = cache(partial(self.parts, identity, scene))

            def content(record: Tiled[Any]) -> Outcome[Option[ContentHash]]:
                return parts().map(lambda read: read.map(lambda values: content_hash(EffectTiles, record, *values, config.encode())))

            def job(slot: str, record: Tiled[Any], side: int, demand: Demand[_Basis]) -> tuple[Job, ...]:
                shown = PreviewKey.read(demand.key, side)

                def published(plane: npt.NDArray[np.float32]) -> Outcome[Option[int]]:
                    return previews.written(slot, shown, Some(shown.checkered(plane))) if state.demands.holds(slot, demand) else Ok(Nothing)

                def run() -> Outcome[None]:
                    return self.rendered(identity, record, side, scene).bind(published).map(lambda _: None)

                return () if previews.current(slot, shown) else (Job(place=slot, key=shown, rank=Rank.SHOWN, run=run),)

            return accumulated(state.demands.demanded(slot, (record, side, scene, config), partial(content, record)).map(partial(job, slot, record, side)) for slot, record, side in requested).map(
                lambda jobs: tuple(itertools.chain.from_iterable(jobs))
            )

        return rooted(bpy.types.Scene, context).bind(lambda scene: planned(scene, PyOpenColorIO.GetCurrentConfig().getCacheID())) if requested else Ok(())

    # --- [LIFECYCLE]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns tile state, previews, the queue feed, the preview scene, and the reader keeping the context view layer's Combined pass as render proxy, every demand dropped on a change outside a preview render and on each new proxy.

        Renders holding no such pass, a sequencer frame or a layer off for render, keep the prior proxy.
        """

        def consumed(scene: bpy.types.Scene, path: str) -> Outcome[Option[str]]:
            widest = root_in(bpy.types.WindowManager, bpy.context).bind(
                lambda manager: Option.of_optional(max((area.width for window in manager.windows for area in window.screen.areas), default=None))
            )
            camera = FrameSize().bound(identity).bind(lambda frame: frame.base(scene).map(lambda base: base.default_value(frame.read(scene))))

            def stored(plane: npt.NDArray[np.float32], framing: Framing, proxy: npt.NDArray[np.float32]) -> Option[str]:
                scaled = framing.scaled(Fraction(max(proxy.shape[:2]), max(plane.shape[:2])))
                identity.held[_TILES].proxy = Some(_Proxy(plane=proxy, framing=scaled, key=content_hash(_Proxy, scaled, arrays=(proxy,))))
                identity.held[_TILES].demands.forgotten()
                return Nothing

            def unrendered(faults: Faults) -> Outcome[Option[npt.NDArray[np.float32]]]:
                return Ok(Nothing) if all(fault.reason is Reason.ABSENT for fault in faults) else Error(faults)

            held = rooted(bpy.types.ViewLayer, bpy.context).bind(lambda layer: combined(path, Some(layer.name)).map(lambda read: Some(read[1])).or_else_with(unrendered))
            return paired(held, camera).bind(
                lambda pair: (
                    pair[0].map(lambda plane: widest.map(lambda side: fitted(plane, (side, side))).default_value(Ok(plane)).map(lambda proxy: stored(plane, pair[1], proxy))).default_value(Ok(Nothing))
                )
            )

        def invalidated(scene: bpy.types.Scene, *_: object) -> Outcome[None]:
            if scene.session_uid not in identity.held[INTERMEDIATES]:
                identity.held[_TILES].demands.forgotten()
            return Ok(None)

        return Ok((
            _TILES,
            PREVIEWS,
            INTERMEDIATES,
            FrameSize.attachment,
            self.preview,
            PreviewFeed(feed=self.jobs),
            ResultRead(consume=consumed),
            Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.ID,), invalidated)),
            Handler(handlers=bpy.app.handlers.frame_change_post, body=invalidated),
        ))


class EntryTile(Line, frozen=True, kw_only=True):
    """Tile of the output a compositor entry holder leaves, as wide as a panel in its region up to the height Blender opens a preview at."""

    tiles: EffectTiles[Any]

    def acquisitions(self, _identity: Identity, /) -> Outcome[Acquisitions]:
        """Returns the tiles rendering the output."""
        return Ok((self.tiles,))

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Returns the draw recording the entry output's tile at panel width, bounded by the preview height, and drawing its published icon."""

        def shown(layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
            match holder, context.region:
                case bpy.types.SceneCompositorEffect(id_data=bpy.types.Scene() as scene, name=name), bpy.types.Region(width=width):
                    metrics = Metrics.read()
                    side = min(width - 2.0 * metrics.margin, metrics.preview)
                    for icon in self.tiles.icon(identity, f"entry {scene.session_uid} {name}", Through(name=name), Some(round(side))).to_list():
                        layout.template_icon(icon_value=icon, scale=side / metrics.unit)
                case _:
                    pass

        return Ok(shown)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _placed[R: Case](
    rows: EffectRows[R], offered: tuple[tuple[CompositorEffect[R], Valued[Entry[R]]], ...], identity: Identity, _source: bpy.types.Scene, target: bpy.types.Scene, preview: Option[PreviewFrame]
) -> Outcome[Restoring]:
    """Places offered entries with their input values after the mirrored ones, answering the removal of every entry it placed."""

    def valued(row: CompositorEffect[R], entry: Valued[Entry[R]]) -> Outcome[bpy.types.SceneCompositorEffect]:
        return rows.placed(identity, target, row, entry.structure, preview=preview).bind(
            lambda held: entry.rebased("").map(lambda values: restored(identity, held, values)).default_value(Ok(False)).map(lambda _: held)
        )

    return accumulated(itertools.starmap(valued, offered)).map(lambda held: partial(_removed, rows, identity, target, tuple(entry.name for entry in held)))


def _removed[R: Case](rows: EffectRows[R], identity: Identity, scene: bpy.types.Scene, names: tuple[str, ...]) -> Outcome[None]:
    """Removes the named preview entries."""
    return accumulated(rows.removed(identity, scene, name) for name in names).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EffectTiles", "EntryTile", "Recipe", "Through", "Tiled"]
