"""Current-render proxies, effect tiles, and demand-driven native preview jobs."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from functools import reduce
from pathlib import Path
from typing import ClassVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.ids.images import Bounded, ResultRead
from blender.bpy.data.rna.descriptors import root_in, struct_at
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import PathHandler, SceneHandler, UpdateHandler
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderImagingFault, BlenderResult
from blender.bpy.nodes.trees import fingerprinted
from blender.bpy.render.color.luts import Enabled
from blender.bpy.render.effects.stack import EffectStack, viewed
from blender.bpy.render.outputs.exports import plane
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.previews.queue import Job, PreviewQueue, Rank
from blender.bpy.render.previews.scenes import Chain, Lighting, Mirrored, Offered, PreviewEngine, PreviewScene, Staging
from blender.bpy.ui.resources.previews import Drawn, Previews
from blender.imaging.chain import Effect
from blender.imaging.color.processors import DisplayView
from blender.imaging.compositing.blending import Entry
from blender.imaging.faults import accumulated, ImagingBufferFault, paired
from blender.imaging.hashing import content_hash, ContentHash
from blender.rendering.outputs.framing import Framing

# --- [MODELS] ---------------------------------------------------------------------------


class Proxied(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Linear render pixels, actual camera framing at that scale, and their content key."""

    plane: npt.NDArray[np.float32]
    framing: Framing
    key: ContentHash


class Wanted(msgspec.Struct, frozen=True, kw_only=True):
    """Effect and UI scale requested by one drawn tile slot."""

    record: Effect
    scale: float


class Demand(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """One retained tile request and its optional observable native content."""

    wanted: Wanted
    content: Option[ContentHash]
    scene: bpy.types.Scene
    config: str


class TileState(msgspec.Struct, kw_only=True):
    """One extension's current render proxy and latest requested tile slots."""

    proxy: Option[Proxied] = Nothing
    wanted: dict[str, Wanted] = msgspec.field(default_factory=dict)
    demands: dict[str, Demand] = msgspec.field(default_factory=dict)


# --- [SERVICES] -------------------------------------------------------------------------


class Tiles(msgspec.Struct, frozen=True, kw_only=True):
    """Renders offered effects through native preview scenes over the current render."""

    stack: EffectStack
    isolated: bool = False
    held: ClassVar[dict[str, TileState]] = {}

    # --- [RENDERING]

    def mirrors(self, identity: Identity, scene: bpy.types.Scene) -> tuple[Mirrored, ...]:
        """Returns preview-enabled entries, retaining only formation while isolated."""
        held = PreviewScene(stack=self.stack).held(identity, scene, Enabled.PREVIEW)
        return tuple(mirror for mirror in held if mirror.row.filter(lambda row: row.record is DisplayView).is_some()) if self.isolated else held

    def chain(self, identity: Identity, scene: bpy.types.Scene, record: Effect, head: Option[npt.NDArray[np.float32]]) -> BlenderResult[Chain]:
        """Replaces the candidate's first matching type or single stage in the native chain."""
        kind, mirrors = type(record), self.mirrors(identity, scene)
        replaced = next((mirror for mirror in mirrors if mirror.row.filter(lambda row: row.record is kind or (kind.stage.single and row.record.stage is kind.stage)).is_some()), None)
        return self.stack.offering(kind).map(
            lambda row: PreviewScene(stack=self.stack).chain(scene, head, tuple(mirror for mirror in mirrors if mirror is not replaced), (Offered(row=row, record=Entry(effect=record)),))
        )

    def parts(self, identity: Identity, scene: bpy.types.Scene, *, mirrored: Option[tuple[Mirrored, ...]] = Nothing) -> BlenderResult[Option[tuple[msgspec.Struct | ContentHash, ...]]]:
        """Reads mirrored records and complete observable native group content."""

        def captured(mirror: Mirrored) -> BlenderResult[Option[tuple[msgspec.Struct | ContentHash, ...]]]:
            values = mirror.row.map(lambda row: row.read(identity, mirror.entry).map(lambda record: (record,))).default_with(
                lambda: (
                    struct_at(mirror.entry, "properties.inputs")
                    .map(lambda inputs: Snapshot.capture(inputs).map(lambda snapshot: (snapshot,)))
                    .default_with(lambda: Error((BlenderAbsentFault(owner=repr(mirror.entry), key="properties.inputs"),)))
                )
            )
            return paired(values, fingerprinted(mirror.group)).map(lambda parts: parts[1].map(lambda key: (*parts[0], key)))

        return accumulated(captured(mirror) for mirror in mirrored.default_with(lambda: self.mirrors(identity, scene))).map(
            lambda values: reduce(
                lambda held, part: held.map2(lambda prior, current: (*prior, *current), part),
                values,
                Option[tuple[msgspec.Struct | ContentHash, ...]].Some((viewed(scene), *(proxy.key for proxy in Tiles.held[identity.package].proxy.to_list()))),
            )
        )

    def rendered(
        self, identity: Identity, record: Effect | Callable[[Option[npt.NDArray[np.float32]]], BlenderResult[Chain]], side: int, *, scene: bpy.types.Scene
    ) -> BlenderResult[npt.NDArray[np.float32]]:
        """Fits the source proxy and renders an effect or caller-supplied native chain."""
        source, lighting = scene, Lighting()

        def staged(head: Option[npt.NDArray[np.float32]]) -> BlenderResult[npt.NDArray[np.float32]]:
            context = Tiles.held[identity.package].proxy.bind(
                lambda proxy: head.map(lambda pixels: ((pixels.shape[0], pixels.shape[1]), scaled(proxy.framing, proxy.plane.shape[:2], pixels.shape[:2])))
            )
            chain = self.chain(identity, source, record, head) if isinstance(record, Effect) else record(head)
            return paired(chain, lighting.environment).bind(
                lambda pair: (
                    PreviewScene(stack=self.stack)
                    .rendered(
                        identity,
                        Staging(
                            source=source,
                            size=head.map(lambda pixels: (pixels.shape[1], pixels.shape[0])).default_value((side, side)),
                            engine=PreviewEngine.of(source),
                            world=pair[1],
                            lamp=lighting.lamp,
                            chain=pair[0],
                            shape=Nothing if head.is_some() else Some(lighting.shape),
                            context=context,
                        ),
                    )
                    .map(lambda readback: readback.plane)
                )
            )

        return Tiles.held[identity.package].proxy.map(lambda proxy: fitted(proxy.plane, side).map(Some)).default_value(Ok(Nothing)).bind(staged)

    # --- [DEMAND]

    def icon(self, identity: Identity, slot: str, record: Effect, scale: float) -> Option[int]:
        """Records draw demand and returns the slot's successfully published native icon."""
        if (wanted := Tiles.held[identity.package].wanted).get(slot) != (request := Wanted(record=record, scale=scale)):
            wanted[slot] = request
        return Previews(identity=identity).icon_id(slot)

    def jobs(self, identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[Job, ...]]:
        """Returns all stale requests against one read of configuration and source entries."""
        state, previews = Tiles.held[identity.package], Previews(identity=identity)
        wanted = state.wanted

        def requests(scene: bpy.types.Scene, pair: tuple[Option[tuple[msgspec.Struct | ContentHash, ...]], PyOpenColorIO.Config]) -> tuple[Job, ...]:
            parts, config = pair

            def job(slot: str, request: Wanted) -> tuple[Job, ...]:
                content = parts.map(lambda values: content_hash(Tiles, request.record, *values, config=Some(config)))
                prior = state.demands.get(slot)
                cache_id = config.getCacheID()
                demand = (
                    prior
                    if prior is not None and prior.wanted is request and prior.content == content and prior.scene == scene and prior.config == cache_id
                    else Demand(wanted=request, content=content, scene=scene, config=cache_id)
                )
                state.demands[slot] = demand
                key = content.default_value(demand)

                def rendered() -> BlenderResult[None]:
                    return previews.tile(
                        slot,
                        key,
                        lambda side: self.rendered(identity, request.record, side, scene=scene).map(
                            lambda pixels: Some(Some(pixels)) if state.demands.get(slot) is demand and state.wanted.get(slot) is request else Nothing
                        ),
                        request.scale,
                    ).map(lambda _: None)

                return () if previews.current(slot, key, request.scale) else (Job(place=(Tiles, slot), key=Drawn.read(key, request.scale), rank=Rank.SHOWN, run=rendered),)

            return tuple(queued for slot, request in wanted.items() for queued in job(slot, request))

        return (
            Result[bpy.types.Scene, BlenderFaults].bind(
                root_in(bpy.types.Scene, context).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="scene"),)),
                lambda scene: paired(self.parts(identity, scene), self.stack.config()).map(lambda pair: requests(scene, pair)),
            )
            if wanted
            else Ok(())
        )

    # --- [LIFECYCLE]

    def consumed(self, identity: Identity, scene: bpy.types.Scene, path: str) -> BlenderResult[Option[str]]:
        """Reads the native Combined plane and camera extent into a linear display-sized proxy."""
        bound = root_in(bpy.types.WindowManager, bpy.context).bind(lambda manager: Option.of_optional(max((area.width for window in manager.windows for area in window.screen.areas), default=None)))

        def held(pair: tuple[npt.NDArray[np.float32], Framing]) -> BlenderResult[Option[str]]:
            pixels, framing = pair

            def stored(proxy: npt.NDArray[np.float32]) -> Option[str]:
                camera = scaled(framing, pixels.shape[:2], proxy.shape[:2])
                Tiles.held[identity.package].proxy = Some(Proxied(plane=proxy, framing=camera, key=content_hash(Proxied, camera, arrays=(proxy,))))
                return Nothing

            return bound.map(lambda side: fitted(pixels, side)).default_value(Ok(pixels)).map(stored)

        camera = FrameSize().bound(identity).bind(lambda frame: paired(frame.read(scene), frame.base(scene)).map(lambda pair: pair[1].default_value(pair[0])))
        return paired(plane(path, (f"{scene.view_layers[0].name}.Combined",), ("R", "G", "B", "A")), camera).bind(held)

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns scoped demand, shared previews, the queue feed, and render-result consumer."""

        @contextmanager
        def held() -> Generator[None]:
            Tiles.held[identity.package] = TileState()
            try:
                yield
            finally:
                del Tiles.held[identity.package]

        def cleared(_path: Option[Path]) -> BlenderResult[None]:
            Tiles.held[identity.package] = TileState()
            return Ok(None)

        def invalidated(_scene: bpy.types.Scene, _updates: tuple[bpy.types.DepsgraphUpdate, ...] = ()) -> BlenderResult[None]:
            Tiles.held[identity.package].demands.clear()
            return Ok(None)

        return Ok((
            (Band.RESOURCES, held()),
            (Band.RESOURCES, Previews(identity=identity)),
            (Band.RESOURCES, PreviewQueue.fed(identity, self.jobs)),
            PreviewScene(stack=self.stack),
            FrameSize.attachment,
            ResultRead(consume=lambda scene, path: self.consumed(identity, scene, path)),
            UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.ID,), body=invalidated),
            SceneHandler(handlers=bpy.app.handlers.frame_change_post, body=invalidated),
            PathHandler(handlers=bpy.app.handlers.load_post, body=cleared),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        ))


# --- [OPERATIONS] -----------------------------------------------------------------------


def fitted(plane: npt.NDArray[np.float32], side: int) -> BlenderResult[npt.NDArray[np.float32]]:
    """Reduces linear pixels to the longest side, preserving native resize failures."""

    def resized(size: tuple[int, int]) -> BlenderResult[npt.NDArray[np.float32]]:
        buffer = OpenImageIO.ImageBufAlgo.resize(OpenImageIO.ImageBuf(plane), roi=OpenImageIO.ROI(0, size[0], 0, size[1], 0, 1, 0, plane.shape[2]))
        return (
            Option.of_optional(buffer.get_pixels(OpenImageIO.FLOAT)).to_result_with(lambda: (BlenderImagingFault(faults=(ImagingBufferFault(request=buffer, error=buffer.geterror()),)),))
            if not buffer.has_error
            else Error((BlenderImagingFault(faults=(ImagingBufferFault(request=buffer, error=buffer.geterror()),)),))
        )

    return Bounded(pixels=side).fitted(plane.shape[1], plane.shape[0]).map(resized).default_value(Ok(plane))


def scaled(framing: Framing, stored: tuple[int, int], target: tuple[int, int]) -> Framing:
    """Applies the proxy's longest-side ratio to the actual camera extent."""
    numerator, denominator = max(target), max(stored)
    width, height = framing.size
    return msgspec.structs.replace(
        framing, resolution_x=(width * numerator + denominator - 1) // denominator, resolution_y=(height * numerator + denominator - 1) // denominator, resolution_percentage=100
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Proxied", "TileState", "Tiles", "Wanted", "fitted", "scaled"]
