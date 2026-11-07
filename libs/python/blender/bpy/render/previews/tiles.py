"""Effect preview tiles over a render proxy: an offered effect, a layer stack's adjustment effects, the output an entry leaves, or a set of entries, each rendered in the preview scene through a closing effect or the scene view, demanded per slot at draw and fed into the preview queue."""

from collections import defaultdict
from collections.abc import Hashable
from contextlib import nullcontext
from functools import cache, cached_property, partial, reduce
import itertools
from typing import Self

import bpy
from expression import Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.images import fingerprinted
from blender.bpy.data.ids.store import Handle, retained
from blender.bpy.data.rna.paths import root_in, Struct
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import updated
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Handler, Resource
from blender.bpy.nodes.desired import Graph, item_key
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor.effects import CompositorEffect, CompositorRecord, Entry
from blender.bpy.render.compositor.entries import Enabled
from blender.bpy.render.compositor.inputs import restored
from blender.bpy.render.compositor.layers import Adjustment, EFFECTS, LayerStack
from blender.bpy.render.compositor.motion import rate
from blender.bpy.render.compositor.offered import OfferedEffects
from blender.bpy.render.compositor.recipes import Valued
from blender.bpy.render.compositor.stack_service import stacked_by
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.outputs.results import CompletedRender, Consume, INTERMEDIATES, ResultRead
from blender.bpy.render.previews.queue import Demands, Job, PreviewFeed, Rank
from blender.bpy.render.previews.scenes import PreviewLighting, PreviewScene, PreviewStack, Restoring, RoutedEntry, Staging
from blender.bpy.ui.layout.rows import ConditionalRow, RowDraw
from blender.bpy.ui.resources.previews import PreviewKey, PREVIEWS
from blender.bpy.ui.resources.theme import Metrics
from blender.imaging.color.processors import converted_image, DisplayView
from blender.imaging.files.images import ImageFile
from blender.imaging.geometry.frames import Raster
from blender.records.faults import accumulated, Error, Fault, Faults, Nothing, Ok, Outcome, paired, Reason
from blender.records.stores import content_hash, ContentHash

# --- [TYPES] ----------------------------------------------------------------------------

type Tiled = Valued[Entry] | Valued[LayerStack] | str | tuple[Valued[Entry], ...]
type _Slot = tuple[Handle[bpy.types.Scene], str]
type _Context = tuple[Snapshot, DisplayView, float, float, ContentHash | None]
type _Plan = tuple[tuple[RoutedEntry, ...], tuple[tuple[CompositorEffect, Valued[Entry]], ...]]
type _Requested = tuple[_Slot, int, _Plan, _Context, Option[_Proxy]]

# --- [MODELS] ---------------------------------------------------------------------------


class _Proxy(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Last finished render's encoded pixels, native fitted windows, and captured data and camera grids."""

    file: ImageFile
    encoding: Option[PyOpenColorIO.Processor]
    data: Raster
    camera: Raster

    @property
    def key(self) -> ContentHash:
        """Content identity of the stored plane and framing."""
        return content_hash(_Proxy, self.data, self.camera, self.file.colorspace, self.encoding.map(lambda source: source.getCacheID()).to_optional(), arrays=(self.file.pixels,))

    def fitted(self, side: int) -> Outcome[Self]:
        """Fits stored pixels inside `side`, retaining native windows and the original camera grid."""
        return self.file.fitted((side, side)).map(lambda file: msgspec.structs.replace(self, file=file, data=msgspec.structs.replace(self.data, shape=(file.header.height, file.header.width))))


# --- [SERVICES] -------------------------------------------------------------------------


class _TileState:
    """Render proxy per source scene, the tile each source slot requests, and its held demand."""

    __slots__ = ("demands", "proxy", "requests")

    def __init__(self) -> None:
        """Holds no proxy, request, or demand."""
        self.proxy: Option[_Proxy] = Nothing
        self.requests: dict[str, tuple[Tiled, int]] = {}
        self.demands: Demands[str] = Demands()

    def forgotten(self) -> None:
        """Drops a source's requests and demands, each drawn slot recording its tile again."""
        self.requests.clear()
        self.demands.forgotten()


class EffectTiles(msgspec.Struct, frozen=True, kw_only=True, dict=True, eq=False):
    """Preview tiles of offered effects over the render proxy, each stack closing through `closing`, else through the scene view.

    `isolated` keeps the closing effect's entries alone ahead of the tiled record.
    `icon` records a slot's tile at draw and returns its published icon, and the preview queue renders each tile its slot holds no current icon of.
    Slots name tiles within the collection.
    """

    # --- [STATE]

    rows: OfferedEffects
    closing: Option[CompositorEffect] = Nothing
    isolated: bool = False
    preview: PreviewScene = PreviewScene()

    @cached_property
    def states(self) -> Resource[defaultdict[Handle[bpy.types.Scene], _TileState]]:
        """Scene requests and proxies for this effect collection."""
        return Resource[defaultdict[Handle[bpy.types.Scene], _TileState]](made=lambda _identity: nullcontext(defaultdict(_TileState)), renewed=Some(lambda _held: defaultdict(_TileState)))

    # --- [STACKS]

    def closes(self, identity: Identity, item: RoutedEntry) -> bool:
        """Whether the closing effect owns a routed entry."""
        return self.closing.map(lambda closing: self.rows.row(identity, item.entry).filter(lambda row: row == closing).is_some()).default_value(value=False)

    def routed(self, identity: Identity, scene: bpy.types.Scene) -> tuple[RoutedEntry, ...]:
        """Returns preview-enabled entries, the closing effect's alone while isolated."""
        held = RoutedEntry.of(scene, Enabled.PREVIEW)
        return tuple(item for item in held if self.closes(identity, item)) if self.isolated else held

    def _planned(self, identity: Identity, scene: bpy.types.Scene, record: Tiled, routed: tuple[RoutedEntry, ...]) -> Outcome[_Plan]:
        """Builds a tiled record's preview stack: an effect replacing its own or a rival's entry, adjustment effects the layer stacks, a recipe every entry, an entry output keeping entries through it, a seeded closing entry where none stands."""

        def offered(entries: tuple[Valued[Entry], ...], kept: tuple[RoutedEntry, ...]) -> Outcome[_Plan]:
            def opened(closing: CompositorEffect) -> Outcome[tuple[Valued[Entry], ...]]:
                closed = any(self.closes(identity, item) for item in kept) or any(type(entry.structure.record) is closing.record for entry in entries)
                return Ok(()) if closed else closing.seeded(scene).map(lambda seed: (Valued(structure=Entry(record=seed)),))

            opening: Outcome[tuple[Valued[Entry], ...]] = self.closing.map(opened).default_value(Ok(()))
            return opening.bind(lambda first: accumulated(self.rows.offering(type(entry.structure.record)).map(lambda row: (row, entry)) for entry in (*first, *entries))).map(
                lambda found: (kept, found)
            )

        def rival(effect: CompositorRecord, item: RoutedEntry) -> bool:
            chosen = self.rows.offering(type(effect)).to_option()
            return self.rows.row(identity, item.entry).filter(lambda row: chosen.filter(lambda own: row == own or self.rows.rivals(own, row)).is_some()).is_some()

        match record:
            case str(name):
                if (position := scene.compositor_effects.find(name)) < 0:
                    return Error(Fault.of(Reason.ABSENT, repr(scene.compositor_effects), member=name))
                through = frozenset(itertools.islice(scene.compositor_effects, position + 1))
                return offered((), tuple(item for item in routed if item.entry in through))
            case tuple(entries):
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
                        return offered(layered, tuple(item for item in routed if stacked_by(identity, item.entry.node_group).is_none()))
                    case Entry(record=effect):
                        replaced = next((item for item in routed if rival(effect, item)), None)
                        kept = tuple(item for item in routed if item is not replaced)
                        return offered((Valued(structure=structure, values=values, prefix=prefix, enables=enables),), kept)

    def rendered(self, identity: Identity, record: Tiled, side: int, scene: bpy.types.Scene) -> Outcome[npt.NDArray[np.float32]]:
        """Renders a tiled record over the render proxy fitted to `side`, over the preview surface without a proxy, the source's updates during the render marked as its own."""
        lighting = PreviewLighting()
        return paired(self._planned(identity, scene, record, self.routed(identity, scene)), lighting.prepared()).bind(
            lambda pair: self._rendered(identity, pair[0], side, scene, self.states[identity][Handle.of(scene)].proxy, lighting, pair[1][0])
        )

    def _rendered(self, identity: Identity, plan: _Plan, side: int, scene: bpy.types.Scene, proxy: Option[_Proxy], lighting: PreviewLighting, world: Graph) -> Outcome[npt.NDArray[np.float32]]:
        def staged(plane: Option[tuple[_Proxy, ImageFile]]) -> Outcome[npt.NDArray[np.float32]]:
            staging = Staging(
                source=scene,
                input=plane.map(lambda held: held[1].pixels).default_value((side, side)),
                world=world,
                sun=lighting.sun,
                stack=PreviewStack(routed=plan[0], placing=partial(_placed, self.rows, plan[1])),
                surface=Nothing if plane.is_some() else Some(lighting.surface),
                rasters=plane.map(lambda held: (held[0].data, held[0].camera)),
            )
            return self.preview.rendered(identity, staging).map(lambda readback: readback.plane)

        config = PyOpenColorIO.GetCurrentConfig()
        return (
            proxy
            .map(
                lambda held: held.fitted(side).bind(lambda fitted: converted_image(config, PyOpenColorIO.ROLE_SCENE_LINEAR, fitted.file, source=fitted.encoding).map(lambda file: Some((fitted, file))))
            )
            .default_value(Ok(Nothing))
            .bind(staged)
        )

    # --- [DEMAND]

    def icon(self, identity: Identity, scene: bpy.types.Scene, slot: str, record: Tiled, /, side: Option[int] = Nothing) -> Option[int]:
        """Records the slot's tile at its side in pixels, Blender's preview render size by default, and returns the slot's published icon."""
        source = Handle.of(scene)
        self.states[identity][source].requests[slot] = (record, side.default_with(lambda: bpy.app.render_preview_size))
        return PREVIEWS[identity].get((self.states, (source, slot))).map(lambda preview: preview.icon_id)

    def jobs(self, identity: Identity, _context: bpy.types.Context) -> Outcome[tuple[Job, ...]]:
        """Returns a job per requested slot holding no current icon, stack parts read once per step for a slot holding no demand."""
        sources, previews = self.states[identity], PREVIEWS[identity]
        live = retained(sources, bpy.data.scenes)
        erased: list[Outcome[None]] = []
        for slot in tuple(previews.held):
            match slot:
                case (owner, (Handle(kind=bpy.types.Scene) as source, str())) if owner is self.states and source not in live:
                    erased.append(previews.dropped(slot))
        if not (requested := tuple(((source, slot), record, side) for source, state in sources.items() for slot, (record, side) in state.requests.items())):
            return accumulated(erased).map(lambda _: ())
        config = PyOpenColorIO.GetCurrentConfig().getCacheID()
        lighting = PreviewLighting()

        @cache
        def context(source: Handle[bpy.types.Scene]) -> Outcome[tuple[bpy.types.Scene, _Context]]:
            """Reads each live source and its immutable rendering inputs once per feed."""
            scene = live[source]
            return self.preview.engine(scene).map(
                lambda engine: (scene, (engine, viewed(scene), scene.frame_current_final, rate(scene), sources[source].proxy.map(lambda held: held.key).to_optional()))
            )

        @cache
        def routes(source: Handle[bpy.types.Scene]) -> tuple[RoutedEntry, ...]:
            """Reads a live source's candidate routes once per feed."""
            return self.routed(identity, live[source])

        def job(slot: _Slot, side: int, plan: _Plan, proxy: Option[_Proxy], demand: Hashable, world: Graph) -> tuple[Job, ...]:
            shown = PreviewKey.read(demand, side)

            def published(plane: npt.NDArray[np.float32]) -> Outcome[Option[int]]:
                return (
                    previews.written((self.states, slot), shown, Some(shown.checkered(plane))) if (state := sources.get(slot[0])) is not None and state.demands.holds(slot[1], demand) else Ok(Nothing)
                )

            def run() -> Outcome[None]:
                return self._rendered(identity, plan, side, live[slot[0]], proxy, lighting, world).bind(published).map(lambda _: None)

            return () if previews.read((self.states, slot), shown).is_some() else (Job(place=slot, key=shown, rank=Rank.SHOWN, run=run),)

        def keyed(plans: tuple[_Requested, ...], entries: tuple[bpy.types.SceneCompositorEffect, ...], world: Graph, read: tuple[Option[ContentHash], ...]) -> tuple[Job, ...]:
            hashes = dict(zip(entries, read[: len(entries)]))
            environment = reduce(
                lambda own, key: own.map2(lambda previous, digest: content_hash(PreviewLighting, previous, digest), key), read[len(entries) :], Some(content_hash(PreviewLighting, lighting))
            )
            return tuple(
                itertools.chain.from_iterable(
                    job(
                        slot,
                        side,
                        plan,
                        proxy,
                        sources[slot[0]].demands.demanded(
                            slot[1],
                            reduce(
                                lambda own, item: own.map2(lambda previous, digest: content_hash(EffectTiles, previous, digest), hashes[item.entry]),
                                plan[0],
                                environment.map(lambda light: content_hash(EffectTiles, tuple(entry for _row, entry in plan[1]), captured, side, config, light)),
                            ),
                        ),
                        world,
                    )
                    for slot, side, plan, captured, proxy in plans
                )
            )

        def scheduled(plans: tuple[_Requested, ...], prepared: tuple[Graph, tuple[bpy.types.Image, ...]]) -> Outcome[tuple[Job, ...]]:
            entries = tuple(dict.fromkeys(item.entry for _slot, _side, plan, _context, _proxy in plans for item in plan[0]))
            return fingerprinted((*entries, *prepared[1])).map(partial(keyed, plans, entries, prepared[0]))

        plans = accumulated(
            context(slot[0]).bind(lambda captured: self._planned(identity, captured[0], record, routes(slot[0])).map(lambda plan: (slot, side, plan, captured[1], sources[slot[0]].proxy)))
            for slot, record, side in requested
        )
        return paired(accumulated(erased), plans, lighting.prepared()).bind(lambda parts: scheduled(parts[1], parts[2]))

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns tile state, previews, the queue feed, the preview scene, and the reader retaining each render's captured source Combined pass as its scene's proxy.

        External changes and new proxies drop that source's requests and demands. Renders without a captured source Combined pass keep its prior proxy.
        """

        def captured(scene: bpy.types.Scene, _config: PyOpenColorIO.Config) -> Outcome[Consume]:
            source = Handle.of(scene)
            return FrameSize().bound(identity).bind(lambda frame: frame.rasters(scene)).map(lambda rasters: partial(consumed, source, rasters))

        def consumed(source: Handle[bpy.types.Scene], rasters: tuple[Raster, Raster], completed: CompletedRender) -> Outcome[Option[str]]:
            widest = root_in(bpy.types.WindowManager, bpy.context).bind(
                lambda manager: Option.of_optional(max((area.width for window in manager.windows for area in window.screen.areas), default=None))
            )

            def stored(proxy: _Proxy) -> Option[str]:
                state = self.states[identity][source]
                state.proxy = Some(proxy)
                state.forgotten()
                return Nothing

            def unrendered(faults: Faults) -> Outcome[Option[ImageFile]]:
                return Ok(Nothing) if all(fault.reason is Reason.ABSENT for fault in faults) else Error(faults)

            def downscaled(file: ImageFile) -> Outcome[Option[str]]:
                return (
                    completed.source_encoding
                    .map(lambda encoding: _Proxy(file=file, encoding=encoding, data=msgspec.structs.replace(rasters[0], shape=(file.header.height, file.header.width)), camera=rasters[1]))
                    .bind(lambda proxy: widest.map(proxy.fitted).default_value(Ok(proxy)))
                    .map(stored)
                )

            return completed.source().map(Some).or_else_with(unrendered).bind(lambda found: found.map(downscaled).default_value(Ok(Nothing)))

        def invalidated(scene: bpy.types.Scene, *_: object) -> Outcome[None]:
            if scene.session_uid not in INTERMEDIATES[identity]:
                for state in Option.of_optional(self.states[identity].get(Handle.of(scene))).to_list():
                    state.forgotten()
            return Ok(None)

        return (
            self.states,
            PREVIEWS,
            INTERMEDIATES,
            FrameSize.attachment,
            self.preview,
            PreviewFeed(feed=self.jobs),
            ResultRead(capture=captured),
            Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.ID,), invalidated)),
            Handler(handlers=bpy.app.handlers.frame_change_post, body=invalidated),
        )


class EntryTile(ConditionalRow, frozen=True, kw_only=True):
    """Tile of the output a compositor entry holder leaves, as wide as a panel in its region up to the height Blender opens a preview at."""

    tiles: EffectTiles

    def acquisitions(self, _identity: Identity, /) -> Acquisitions:
        """Returns the tiles rendering the output."""
        return (self.tiles,)

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Returns the draw recording the entry output's tile at panel width, bounded by the preview height, and drawing its published icon."""

        def shown(layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
            match holder, context.region:
                case bpy.types.SceneCompositorEffect(id_data=bpy.types.Scene() as scene, name=name), bpy.types.Region(width=width):
                    metrics = Metrics.read()
                    side = min(width - 2.0 * metrics.margin, metrics.preview)
                    for icon in self.tiles.icon(identity, scene, f"entry {name}", name, Some(round(side))).to_list():
                        layout.template_icon(icon_value=icon, scale=side / metrics.unit)
                case _:
                    pass

        return Ok(shown)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _placed(
    rows: OfferedEffects, offered: tuple[tuple[CompositorEffect, Valued[Entry]], ...], identity: Identity, _source: bpy.types.Scene, target: bpy.types.Scene, rasters: Option[tuple[Raster, Raster]]
) -> Outcome[Restoring]:
    """Places offered entries with their input values after the routed ones, returning the removal of every entry it placed."""

    def valued(row: CompositorEffect, entry: Valued[Entry]) -> Outcome[bpy.types.SceneCompositorEffect]:
        return rows.placed(identity, target, row, entry.structure, rasters=rasters).bind(
            lambda held: entry.rebased("").map(lambda values: restored(identity, held, values).map(lambda _: held)).default_value(Ok(held))
        )

    return accumulated(itertools.starmap(valued, offered)).map(lambda held: partial(_removed, rows, identity, target, tuple(entry.name for entry in held)))


def _removed(rows: OfferedEffects, identity: Identity, scene: bpy.types.Scene, names: tuple[str, ...]) -> Outcome[None]:
    """Removes the named preview entries."""
    return accumulated(rows.removed(identity, scene, name) for name in names).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EffectTiles", "EntryTile", "Tiled"]
