"""Render proxies, effect tiles, and preview jobs of drawn tile slots."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, nullcontext
from functools import reduce
from typing import Annotated, Final

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.ids.images import Bounded
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import root_in, rooted
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import SceneHandler, UpdateHandler
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band, Registrable
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.trees import fingerprinted
from blender.bpy.render.effects.compositing import layers
from blender.bpy.render.effects.formation import displaying, formation, looked, NONE, viewed
from blender.bpy.render.effects.records import inputs_of
from blender.bpy.render.effects.rows import Enabled
from blender.bpy.render.effects.stack import EffectStack
from blender.bpy.render.outputs.exports import combined, Source
from blender.bpy.render.outputs.framing import FrameSize, Framing
from blender.bpy.render.outputs.results import intermediate, INTERMEDIATES, ResultRead
from blender.bpy.render.previews.queue import BACKLOG, Job, PreviewQueue, Rank
from blender.bpy.render.previews.scenes import Chain, Lighting, Mirrored, Offered, PreviewEngine, PreviewScene, Staging
from blender.bpy.settings.attachments import Attachment
from blender.bpy.settings.properties import Changed, Choices, EnumRows
from blender.bpy.ui.layout.rows import joined, Member, placed, Property, RowContent, RowDraw
from blender.bpy.ui.layout.templates import IconView
from blender.bpy.ui.resources.previews import Drawn, PREVIEWS
from blender.imaging.chain import Effect
from blender.imaging.color.processors import DisplayView
from blender.imaging.compositing.blending import Entry
from blender.imaging.compositing.layers import Adjustment, LayerStack
from blender.imaging.faults import accumulated, ImagingBufferFault, paired
from blender.imaging.hashing import content_hash, ContentHash
from blender.imaging.parameters import Gated

# --- [MODELS] ---------------------------------------------------------------------------


class Proxied(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Linear render pixels, camera framing at pixel scale, and content key."""

    plane: npt.NDArray[np.float32]
    framing: Framing
    key: ContentHash


class Wanted(msgspec.Struct, frozen=True, kw_only=True):
    """Tile slot's requested effect or layer stack and UI scale."""

    record: Effect | LayerStack[layers.Source, Effect]
    scale: float


class Demand(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Held tile request with optional content hash."""

    wanted: Wanted
    content: Option[ContentHash]
    scene: bpy.types.Scene
    config: str


class FormationLooks(msgspec.Struct, frozen=True, kw_only=True):
    """Look the formation's view applies, one of the OCIO looks Blender offers it."""

    look: Annotated[str, msgspec.Meta(title="Look", description="Look the formation's view applies ahead of its transform")] = NONE


class StackPresets(msgspec.Struct, frozen=True, kw_only=True):
    """Layer Stack preset the head stack takes, its effects moved from their defaults by Strength."""

    look: Annotated[str, msgspec.Meta(title="Stack Look", description="Layer Stack preset the head stack takes")] = ""
    strength: Annotated[int, msgspec.Meta(ge=0, le=200, title="Strength", description="Share of each preset effect's change from its defaults, in percent")] = 100


class TileState(msgspec.Struct, kw_only=True):
    """Render proxy, requested slots, and held demands."""

    proxy: Option[Proxied] = Nothing
    wanted: dict[str, Wanted] = msgspec.field(default_factory=dict)
    demands: dict[str, Demand] = msgspec.field(default_factory=dict)


# --- [SERVICES] -------------------------------------------------------------------------

TILES: Final = Scoped[TileState](made=lambda _identity: nullcontext(TileState()), renewed=Some(lambda _held: TileState()))


class Tiles(msgspec.Struct, frozen=True, kw_only=True):
    """Renders offered effects in preview scenes over render proxy."""

    stack: EffectStack
    isolated: bool = False

    # --- [RENDERING]

    def mirrors(self, identity: Identity, scene: bpy.types.Scene) -> tuple[Mirrored, ...]:
        """Returns preview-enabled entries, formation alone while isolated."""
        held = PreviewScene(stack=self.stack).held(identity, scene, Enabled.PREVIEW)
        return tuple(mirror for mirror in held if mirror.row.filter(lambda row: row is formation).is_some()) if self.isolated else held

    def chain(self, identity: Identity, scene: bpy.types.Scene, record: Effect | LayerStack[layers.Source, Effect], head: Option[npt.NDArray[np.float32]]) -> BlenderResult[Chain]:
        """Builds chain with an effect replacing the first same-type or single-stage entry, or a layer stack's adjustment effects replacing the head stack."""
        mirrors = self.mirrors(identity, scene)

        def offered(entries: tuple[Entry[Effect], ...], kept: tuple[Mirrored, ...]) -> BlenderResult[Chain]:
            rows = accumulated(self.stack.offering(type(entry.effect)).map(lambda row: Offered(row=row, record=entry)) for entry in entries)
            return rows.map(lambda placed: Chain.of(scene, head, kept, placed))

        match record:
            case Effect():
                kind = type(record)
                replaced = next((mirror for mirror in mirrors if mirror.row.filter(lambda row: row.record is kind or (kind.stage.single and row.record.stage is kind.stage)).is_some()), None)
                return offered((Entry(effect=record),), tuple(mirror for mirror in mirrors if mirror is not replaced))
            case LayerStack():
                entries = tuple(gated.value for layer, _ in record.composited() if isinstance(layer.source, Adjustment) and layer.effects.enabled for gated in layer.effects.value if gated.enabled)
                return offered(entries, tuple(mirror for mirror in mirrors if layers.stacked_by(identity, mirror.entry).is_none()))

    def parts(self, identity: Identity, scene: bpy.types.Scene, *, mirrored: Option[tuple[Mirrored, ...]] = Nothing) -> BlenderResult[Option[tuple[msgspec.Struct | ContentHash, ...]]]:
        """Reads entry records and group fingerprints, `Nothing` for a group without one."""

        def captured(mirror: Mirrored) -> BlenderResult[Option[tuple[msgspec.Struct | ContentHash, ...]]]:
            values = mirror.row.map(lambda row: row.read(identity, mirror.entry).map(lambda record: (record,))).default_with(
                lambda: inputs_of(mirror.entry).bind(Snapshot.capture).map(lambda snapshot: (snapshot,))
            )
            return paired(values, fingerprinted(mirror.group)).map(lambda parts: parts[1].map(lambda key: (*parts[0], key)))

        head: Option[tuple[msgspec.Struct | ContentHash, ...]] = Some((viewed(scene), *(proxy.key for proxy in TILES.of(identity).proxy.to_list())))
        return accumulated(captured(mirror) for mirror in mirrored.default_with(lambda: self.mirrors(identity, scene))).map(
            lambda values: reduce(lambda held, part: held.map2(lambda prior, current: (*prior, *current), part), values, head)
        )

    def rendered(
        self, identity: Identity, record: Effect | LayerStack[layers.Source, Effect] | Callable[[Option[npt.NDArray[np.float32]]], BlenderResult[Chain]], side: int, *, scene: bpy.types.Scene
    ) -> BlenderResult[npt.NDArray[np.float32]]:
        """Renders an effect, layer stack, or caller chain over render proxy fitted to side, the source's updates during the render marked as its own."""
        lighting, proxy = Lighting(), TILES.of(identity).proxy

        def staged(head: Option[npt.NDArray[np.float32]]) -> BlenderResult[npt.NDArray[np.float32]]:
            chain = self.chain(identity, scene, record, head) if isinstance(record, Effect | LayerStack) else record(head)
            staging = paired(chain, lighting.environment).map(
                lambda pair: Staging(
                    source=scene,
                    size=head.map(lambda pixels: (pixels.shape[1], pixels.shape[0])).default_value((side, side)),
                    engine=PreviewEngine.of(scene),
                    world=pair[1],
                    lamp=lighting.lamp,
                    chain=pair[0],
                    shape=Nothing if head.is_some() else Some(lighting.shape),
                    context=proxy.map2(lambda held, pixels: ((pixels.shape[0], pixels.shape[1]), scaled(held.framing, held.plane.shape[:2], pixels.shape[:2])), head),
                )
            )
            return staging.bind(lambda desired: PreviewScene(stack=self.stack).rendered(identity, desired)).map(lambda readback: readback.plane)

        with intermediate(identity, scene):
            return proxy.map(lambda held: fitted(held.plane, side).map(Some)).default_value(Ok(Nothing)).bind(staged)

    # --- [DEMAND]

    @staticmethod
    def icon(identity: Identity, slot: str, record: Effect | LayerStack[layers.Source, Effect], scale: float) -> Option[int]:
        """Records slot demand and returns published slot icon."""
        if (wanted := TILES.of(identity).wanted).get(slot) != (request := Wanted(record=record, scale=scale)):
            wanted[slot] = request
        return PREVIEWS.of(identity).icon_id(slot)

    def jobs(self, identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[Job, ...]]:
        """Returns a job per stale slot from one config and entry read."""
        state, previews = TILES.of(identity), PREVIEWS.of(identity)

        def requests(scene: bpy.types.Scene, parts: Option[tuple[msgspec.Struct | ContentHash, ...]], config: PyOpenColorIO.Config) -> tuple[Job, ...]:
            def job(slot: str, request: Wanted) -> tuple[Job, ...]:
                content, cache = parts.map(lambda values: content_hash(Tiles, request.record, *values, config=Some(config))), config.getCacheID()
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
                            lambda pixels: Some(Some(pixels)) if state.demands.get(slot) is demand and state.wanted.get(slot) is request else Nothing
                        ),
                        request.scale,
                    ).map(lambda _: None)

                return () if previews.current(slot, key, request.scale) else (Job(place=(Tiles, slot), key=Drawn.read(key, request.scale), rank=Rank.SHOWN, run=rendered),)

            return tuple(queued for slot, request in state.wanted.items() for queued in job(slot, request))

        return rooted(bpy.types.Scene, context).bind(lambda scene: paired(self.parts(identity, scene), self.stack.config()).map(lambda pair: requests(scene, *pair))) if state.wanted else Ok(())

    # --- [LIFECYCLE]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns tile state, previews, queue feed, and the render proxy reader."""

        def consumed(scene: bpy.types.Scene, path: str) -> BlenderResult[Option[str]]:
            bound = root_in(bpy.types.WindowManager, bpy.context).bind(
                lambda manager: Option.of_optional(max((area.width for window in manager.windows for area in window.screen.areas), default=None))
            )
            camera = FrameSize().bound(identity).bind(lambda frame: paired(frame.read(scene), frame.base(scene)).map(lambda pair: pair[1].default_value(pair[0])))

            def stored(pixels: npt.NDArray[np.float32], framing: Framing, proxy: npt.NDArray[np.float32]) -> Option[str]:
                fitted_camera = scaled(framing, pixels.shape[:2], proxy.shape[:2])
                TILES.of(identity).proxy = Some(Proxied(plane=proxy, framing=fitted_camera, key=content_hash(Proxied, fitted_camera, arrays=(proxy,))))
                return Nothing

            return paired(combined(path, Source.LAYER).map(lambda read: read[1]), camera).bind(
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
            PreviewScene(stack=self.stack),
            FrameSize.attachment,
            ResultRead(consume=consumed),
            UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.ID,), body=invalidated),
            SceneHandler(handlers=bpy.app.handlers.frame_change_post, body=invalidated),
        ))


class Looks[S: FormationLooks | StackPresets](msgspec.Struct, frozen=True, kw_only=True):
    """Look tiles over the render proxy in an icon view of a scene pick, the pick's further fields drawn beneath."""

    scale: float
    attachment: Attachment[S]

    @staticmethod
    def formation(scale: float) -> "Looks[FormationLooks]":
        """Binds the scene pick to the looks Blender offers the formation's view, read live from the formation and written after each pick."""

        def held(identity: Identity, scene: bpy.types.Scene) -> tuple[bpy.types.ColorManagedViewSettings, DisplayView]:
            return displaying(identity, scene).map(lambda pair: (pair[1].view_settings, viewed(pair[1]))).default_with(lambda: (scene.view_settings, viewed(scene)))  # type: ignore[arg-type, return-value]  # ty: ignore[invalid-return-type]

        def offered(identity: Identity, owner: "bpy.types.bpy_struct[object]", _context: bpy.types.Context | None) -> EnumRows:
            settings, record = held(identity, owner.id_data)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            config = PyOpenColorIO.GetCurrentConfig()
            return (
                (
                    name,
                    label,
                    Option.of_optional(config.getLook(name)).map(PyOpenColorIO.Look.getDescription).default_value(""),
                    Tiles.icon(identity, f"look {name}", msgspec.structs.replace(record, look=Gated(enabled=name != NONE, value=name)), scale).default_value(0),
                    index,
                )
                for index, name in enumerate((NONE, *config.getLookNames()))
                for label in (bpy.types.UILayout.enum_item_name(settings, "look", name),)
                if label
            )

        @contextmanager
        def picked(identity: Identity) -> Generator[Changed]:
            def write(handle: Handle[bpy.types.Scene], look: str) -> BlenderResult[Option[str]]:
                return handle.resolve().bind(lambda scene: looked(identity, scene, look)).map(lambda _: Some(f"Look {look}"))

            with identity.reports.deferred(write) as deferred:
                yield lambda owner, _context, prop: deferred(Handle.of(owner.id_data), owner[prop.identifier])  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

        choices = Choices(offered, current=Some(lambda identity, owner: held(identity, owner.id_data)[0].look))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        return Looks(scale=scale, attachment=Attachment(key="looks", form=FormationLooks, owner=bpy.types.Scene, changed=Some(picked), bindings=Some({vars(FormationLooks)["look"]: choices})))

    @staticmethod
    def stacked(scale: float, stacks: layers.LayerStacks[Effect]) -> "Looks[StackPresets]":
        """Binds the scene pick to the Layer Stack presets, each pick or Strength change writing the picked preset over the head stack."""
        preset = stacks.preset

        def tile(identity: Identity, stem: str) -> int:
            return preset.found(identity, stem).bind(preset.read).to_option().bind(lambda record: Tiles.icon(identity, f"stack look {stem}", record, scale)).default_value(0)

        def offered(identity: Identity, _owner: "bpy.types.bpy_struct[object]", _context: bpy.types.Context | None) -> EnumRows:
            return ((stem, name, tip, tile(identity, stem), index) for index, (stem, name, tip) in enumerate(preset.entries(identity)))

        @contextmanager
        def picked(identity: Identity) -> Generator[Changed]:
            def applied(scene: bpy.types.Scene, pick: StackPresets) -> BlenderResult[Option[str]]:
                match pick:
                    case StackPresets(look=""):
                        return Ok(Nothing)
                    case StackPresets(look=look, strength=strength):
                        record = preset.found(identity, look).bind(preset.read)
                        changed = paired(record, stacks.bound(identity)).bind(lambda parts: parts[1].looked(scene, parts[0], strength, bpy.context))
                        return changed.map(lambda wrote: Some(f"Look {bpy.path.display_name(look)}") if wrote else Nothing)

            def write(handle: Handle[bpy.types.Scene]) -> BlenderResult[Option[str]]:
                return handle.resolve().bind(lambda scene: attachment.read(identity, scene).bind(lambda pick: applied(scene, pick)))

            with identity.reports.deferred(write) as deferred:
                yield lambda owner, _context, _prop: deferred(Handle.of(owner.id_data))  # type: ignore[arg-type]

        attachment = Attachment(key="stack_looks", form=StackPresets, owner=bpy.types.Scene, changed=Some(picked), bindings=Some({vars(StackPresets)["look"]: Choices(offered)}))
        return Looks(scale=scale, attachment=attachment)

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Scene pick the icon view binds."""
        return (self.attachment,)

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Declares nothing beside the scene pick."""
        return Ok(())

    @property
    def row(self) -> RowContent:
        """Icon view over the scene pick's look, then the pick's further fields."""
        look, *rest = msgspec.structs.fields(self.attachment.form)

        def drawn(identity: Identity, name: str) -> BlenderResult[RowDraw]:
            rows = (
                IconView(target=Member(root=bpy.types.Scene, path=f"{name}.{look.name}"), scale=self.scale, show_labels=True),
                *(Property(target=Member(root=bpy.types.Scene, path=f"{name}.{field.name}"), slider=True) for field in rest),
            )
            return placed(rows, identity).map(joined)

        return (self.attachment, lambda identity: self.attachment.bound(identity).bind(lambda attached: drawn(identity, attached.name)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def fitted(plane: npt.NDArray[np.float32], side: int) -> BlenderResult[npt.NDArray[np.float32]]:
    """Shrinks linear pixels so longest side fits `side`."""

    def resized(size: tuple[int, int]) -> BlenderResult[npt.NDArray[np.float32]]:
        buffer = OpenImageIO.ImageBufAlgo.resize(OpenImageIO.ImageBuf(plane), roi=OpenImageIO.ROI(0, size[0], 0, size[1], 0, 1, 0, plane.shape[2]))
        return ImagingBufferFault.checked(buffer).bind(ImagingBufferFault.pixels)

    return Bounded(pixels=side).fitted(plane.shape[1], plane.shape[0]).map(resized).default_value(Ok(plane))


def scaled(framing: Framing, stored: tuple[int, int], target: tuple[int, int]) -> Framing:
    """Scales camera framing by target-to-stored longest side ratio."""
    numerator, denominator = max(target), max(stored)
    width, height = framing.size
    return msgspec.structs.replace(
        framing, resolution_x=(width * numerator + denominator - 1) // denominator, resolution_y=(height * numerator + denominator - 1) // denominator, resolution_percentage=100
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TILES", "Demand", "FormationLooks", "Looks", "Proxied", "StackPresets", "TileState", "Tiles", "Wanted", "fitted", "scaled"]
