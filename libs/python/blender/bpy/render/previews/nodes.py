"""Per-node previews over every geometry node editor under the node previews preference: the commands, the feed planning a job per changed target, and the stored thumbnails each render writes."""

from collections import Counter, defaultdict
from collections.abc import Callable, Hashable, Mapping
from functools import cache, cached_property, partial, reduce
import itertools
import os
from pathlib import Path
from typing import ClassVar, Final

from bl_ui.space_node import NODE_MT_context_menu
import bpy
from expression import Option, Some, try_downcast
from expression.extra.result import catch
import msgspec
import OpenImageIO
import PyOpenColorIO

from blender.bpy.commands.invocations import Save
from blender.bpy.commands.keymaps import Chord, Keymap
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, Placement, tallied, Unchanged
from blender.bpy.data.ids.images import fingerprinted, loaded, Tag
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import updated
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Handler
from blender.bpy.nodes.desired import Graph
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor.motion import rate
from blender.bpy.render.outputs.results import INTERMEDIATES
from blender.bpy.render.previews.queue import Job, PreviewFeed, PreviewQueue, Rank
from blender.bpy.render.previews.scenes import PreviewLighting, PreviewScene, Readback
from blender.bpy.render.previews.targets import CLAY, Editor, enclosing, NODE_PREVIEWS, NodePreviews, rendered, RenderKind, SETTING, SWATCH, Target, targets
from blender.bpy.render.previews.thumbnails import Cell, NODE_STATES, NODE_TEXTURES, NodeThumbnails, ThumbnailSettings
from blender.bpy.ui.drawing.handlers import Drawing, Overlay, Signal
from blender.bpy.ui.elements.menus import Append
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import OperatorButton, Property, Row
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Store
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.screens.areas import AreaQuery, areas, Located, to_region
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.buffers import Depth
from blender.imaging.files.images import Format, ImageFile, read_file, Storage
from blender.records.faults import accumulated, Error, Fault, Nothing, Ok, Outcome, paired, Reason
from blender.records.fields import Retention
from blender.records.stores import content_hash, ContentHash, removed

# --- [TYPES] ----------------------------------------------------------------------------

type _Candidate = tuple[bpy.types.Node, bpy.types.NodeSocket, Target, int, tuple[bpy.types.Node, ...], bpy.types.Scene, Rank]

# --- [CONSTANTS] ------------------------------------------------------------------------

_SUFFIX: Final = ".exr"
_SUFFIXES: Final[tuple[str, *tuple[str, ...]]] = (_SUFFIX, *(suffix for row in Format.writable() for suffix in row.suffixes))

# --- [MODELS] ---------------------------------------------------------------------------


class ThumbnailExport(bpy.types.PropertyGroup):
    """Exported thumbnail size and image load after write."""

    size: bpy.props.IntProperty(name="Size", description="Image side in pixels", subtype="PIXEL", min=256, max=2048, default=512)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    opened: bpy.props.BoolProperty(name="Open Image", description="Load written image", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------


class NodePreview(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Row of per-node previews over every node editor under the node previews preference, its commands built once."""

    retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing
    preview: PreviewScene = PreviewScene()
    signal: Signal = msgspec.field(default_factory=Signal)
    key: ClassVar[Key] = NODE_PREVIEWS.key

    # --- [COMMANDS]

    @cached_property
    def toggle(self) -> Operator[Empty]:
        """Command toggling node editor thumbnails."""
        return Operator(form=Empty, options=frozenset(), run=self.toggle_node_previews)

    @cached_property
    def enlarge(self) -> Operator[Empty]:
        """Command toggling the larger active node thumbnail."""
        return Operator(form=Empty, options=frozenset(), run=self.enlarge_node_preview)

    @cached_property
    def refresh(self) -> Operator[Empty]:
        """Command rendering every thumbnail again."""
        return Operator(form=Empty, options=frozenset(), run=self.refresh_node_previews)

    @cached_property
    def clear(self) -> Operator[Empty]:
        """Command removing cached and stored thumbnails."""
        return Operator(form=Empty, options=frozenset(), run=self.clear_node_previews)

    @cached_property
    def export(self) -> Operator[ThumbnailExport]:
        """Command writing the active node thumbnail to an image file."""
        return Operator(form=ThumbnailExport, options=frozenset(), invoke=Some(Save(extensions=lambda _: _SUFFIXES)), run=self.export_node_preview)

    def flipped(self, call: Call[Empty], change: Callable[[NodePreviews], Outcome[object]]) -> Outcome[Answer]:
        """Writes the change into the preference and refreshes thumbnails."""
        return NODE_PREVIEWS.read(call.identity).bind(change).bind(lambda _: self.signal.fired()).map(lambda _: Done())

    def toggle_node_previews(self, call: Call[Empty]) -> Outcome[Answer]:
        """Toggle node editor thumbnails."""
        return self.flipped(call, lambda held: written(held, ("enabled",), not held.enabled))

    def enlarge_node_preview(self, call: Call[Empty]) -> Outcome[Answer]:
        """Toggle the larger active node thumbnail on top."""
        return self.flipped(call, lambda held: written(held, ("use_enlarge",), not held.use_enlarge))

    def refresh_node_previews(self, call: Call[Empty]) -> Outcome[Answer]:
        """Render every thumbnail again."""
        return self.rerendered(call.identity).map(lambda _: Done())

    def clear_node_previews(self, call: Call[Empty]) -> Outcome[Answer]:
        """Empty texture cache, held values, and previews folder, then render every thumbnail again."""
        return _thumbnails(call.identity).bind(lambda files: self.rerendered(call.identity, Some(files)).map(lambda _: Done(notes=(("INFO", f"Removed {len(files)} stored thumbnails"),))))

    def export_node_preview(self, call: Call[ThumbnailExport]) -> Outcome[Answer]:
        """Write the active node thumbnail to an image file in the scene display space."""
        shown = paired(rooted(bpy.types.SpaceNodeEditor, call.context), rooted(bpy.types.Scene, call.context)).bind(
            lambda found: Editor.of(*found).bind(
                lambda editor: editor.bind(
                    lambda held: Option.of_optional(found[0].edit_tree).bind(lambda tree: Option.of_optional(tree.nodes.active).map(lambda node: (held, tree, node)))
                ).to_result_with(partial(Fault.of, Reason.ABSENT, "SpaceNodeEditor.edit_tree.nodes", member="active"))
            )
        )

        def chosen(editor: Editor, tree: bpy.types.NodeTree, node: bpy.types.Node, settings: NodePreviews) -> Outcome[Target]:
            return targets(call.identity, editor, tree, settings).bind(
                lambda found: Option.of_optional(next((target for source, _socket, target in found if source == node), None)).to_result_with(
                    partial(Fault.of, Reason.ABSENT, repr(tree.nodes), member=node.name)
                )
            )

        def wrote(target: Target, settings: NodePreviews) -> Outcome[Path]:
            path = call.paths[0]
            lighting = target.kind.lighting(settings.lighting)
            return paired(lighting.prepared(), Format.of(path)).bind(
                lambda pair: rendered(call.identity, self.preview, target, call.properties.size, lighting, pair[0][0]).bind(
                    lambda readback: ImageFile(pixels=readback.plane, colorspace=readback.colorspace).written(path, Storage(depth=pair[1].depths[-1]), {}, PyOpenColorIO.GetCurrentConfig())
                )
            )

        return (
            paired(shown, NODE_PREVIEWS.read(call.identity))
            .bind(lambda parts: chosen(*parts[0], parts[1]).bind(lambda target: wrote(target, parts[1])))
            .bind(lambda name: loaded(str(name), Tag()).map(lambda _: name) if call.properties.opened else Ok(name))
            .map(lambda name: Done.wrote((name,)))
        )

    # --- [THUMBNAILS]

    def thumbnailed(self, identity: Identity, target: Target, demand: Hashable, size: int, kept: frozenset[Hashable], limit: int, lighting: PreviewLighting, world: Graph) -> Outcome[None]:
        """Renders a target into texture cache and previews folder, its uniform value held and stored as a typed header attribute."""
        state, config = NODE_STATES[identity], PyOpenColorIO.GetCurrentConfig()

        def pruned() -> Outcome[None]:
            def oldest(paths: tuple[Path, ...]) -> Outcome[list[Path]]:
                try:
                    return Ok(sorted(paths, key=lambda path: path.stat().st_mtime))
                except OSError as error:
                    return Error(Fault.of(Reason.UNREADABLE, str(_folder(identity)), error))

            return _thumbnails(identity).bind(oldest).bind(lambda found: accumulated(removed(path) for path in found[:-limit])).map(lambda _: None)

        def stored(readback: Readback, name: str) -> Outcome[None]:
            number = readback.uniform

            def written_file(digest: ContentHash) -> Outcome[None]:
                header = OpenImageIO.ImageSpec()
                for held in number.to_list():
                    header.attribute(name, held)
                return (
                    ImageFile(pixels=readback.plane, colorspace=readback.colorspace, header=header)
                    .written(_thumbnail(identity, digest), Storage(depth=Depth.HALF), {}, config)
                    .bind(lambda _: pruned())
                )

            return (
                NODE_TEXTURES[identity]
                .stored(demand, readback.plane, limit, kept, metadata=number)
                .bind(lambda _: written_file(demand) if isinstance(demand, bytes) else Ok(None))
                .bind(lambda _: self.signal.fired())
                if state.demands.holds(target, demand)
                else Ok(None)
            )

        return rendered(identity, self.preview, target, size, lighting, world).bind(lambda readback: stored(readback, identity.identifier(PreviewScene.value)))

    # --- [FEED]

    def jobs(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Job, ...]]:
        """Writes editor cells and returns a job per target key neither cache nor folder holds."""
        state, metrics = NODE_STATES[identity], Metrics.read()
        inventory = cache(lambda: _thumbnails(identity).map(frozenset))
        manager: bpy.types.WindowManager = context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        views: dict[Editor, tuple[bpy.types.Scene, bpy.types.SpaceNodeEditor, bpy.types.NodeTree, list[Located]]] = {}

        def located(found: Located) -> Outcome[None]:
            match found.area.spaces.active:
                case bpy.types.SpaceNodeEditor(edit_tree=bpy.types.NodeTree() as tree) as space:

                    def kept(editors: Option[Editor]) -> None:
                        for at, editor in itertools.product(found.within().to_option().to_list(), editors.to_list()):
                            views.setdefault(editor, (found.window.scene, space, tree, []))[3].append(at)

                    return Editor.of(space, found.window.scene).map(kept)
                case _:
                    return Ok(None)

        @cache
        def source(item: Handle[bpy.types.Object], modifier: str) -> Outcome[Option[ContentHash]]:
            """Reads each source's upstream content once in this feed."""
            return item.resolve().bind(lambda held: fingerprinted(held, before_modifier=Some(modifier)))

        def keyed(
            node: bpy.types.Node,
            target: Target,
            basis: ThumbnailSettings,
            nested: tuple[bpy.types.Node, ...],
            images: tuple[bpy.types.Image, ...],
            hashes: Mapping[bpy.types.Node | bpy.types.Image, Option[ContentHash]],
        ) -> Outcome[Option[ContentHash]]:
            return source(target.editor.item, target.editor.modifier).map(
                lambda upstream: reduce(
                    lambda own, key: own.map2(lambda held, current: content_hash(Target, held, current), key),
                    (*tuple(hashes[parent] for parent in nested), *tuple(hashes[image] for image in images), upstream),
                    hashes[node].map(lambda own: content_hash(Target, basis, own)),
                )
            )

        def restored(key: ContentHash, path: Path, config: PyOpenColorIO.Config, name: str, limit: int, kept: frozenset[Hashable]) -> Outcome[None]:
            def cached(image: ImageFile) -> Outcome[None]:
                number = Option.of_optional(try_downcast(float, image.header.getattribute(name)))
                return (
                    NODE_TEXTURES[identity]
                    .stored(key, image.pixels, limit, kept, metadata=number)
                    .bind(lambda _: catch(os.utime, exception=OSError)(path).map_error(partial(Fault.of, Reason.UNWRITABLE, str(path))))
                    .map(lambda _: None)
                )

            return read_file(path, config).bind(cached)

        def planned(settings: NodePreviews, config: PyOpenColorIO.Config, name: str) -> Outcome[tuple[Job, ...]]:
            type Lighting = tuple[PreviewLighting, Graph, tuple[bpy.types.Image, ...]]

            host, cache_id = (bpy.app.version, bpy.app.build_hash), config.getCacheID()
            scale: float = settings.scale  # ty: ignore[unsound-assignment]

            @cache
            def prepared(kind: RenderKind) -> Outcome[Lighting]:
                lighting = kind.lighting(settings.lighting)
                return lighting.prepared().map(lambda found: (lighting, *found))

            @cache
            def scene_values(scene: bpy.types.Scene) -> Outcome[tuple[Snapshot, DisplayView, tuple[float, ...]]]:
                """Reads the native rendering basis once per source scene."""
                return self.preview.engine(scene).map(lambda engine: (engine, viewed(scene), (scene.frame_current_final, rate(scene)) if settings.frames else ()))

            def demanded(
                node: bpy.types.Node,
                target: Target,
                size: int,
                nested: tuple[bpy.types.Node, ...],
                scene: bpy.types.Scene,
                lighting: Lighting,
                hashes: Mapping[bpy.types.Node | bpy.types.Image, Option[ContentHash]],
            ) -> Outcome[Hashable]:
                return (
                    scene_values(scene)
                    .bind(
                        lambda values: keyed(
                            node,
                            target,
                            ThumbnailSettings(kind=target.kind, socket=target.socket, engine=values[0], size=size, lighting=lighting[0], view=values[1], frame=values[2], host=host, config=cache_id),
                            nested,
                            lighting[2],
                            hashes,
                        )
                    )
                    .map(partial(state.demands.demanded, target))
                )

            def viewed_in(editor: Editor, scene: bpy.types.Scene, space: bpy.types.SpaceNodeEditor, tree: bpy.types.NodeTree, locations: list[Located]) -> Outcome[tuple[_Candidate, ...]]:
                def rows(nested: tuple[bpy.types.Node, ...], found: tuple[tuple[bpy.types.Node, bpy.types.NodeSocket, Target], ...]) -> tuple[_Candidate, ...]:
                    counts, active = Counter(target.node for _node, _socket, target in found), tree.nodes.active

                    def each(node: bpy.types.Node, socket: bpy.types.NodeSocket, target: Target) -> _Candidate:
                        size = round(node.width * metrics.scale * scale / (2 if counts[target.node] > 1 else 1))
                        location = node.location_absolute
                        rank = Rank.ACTIVE if node.select or node == active else Rank.SHOWN if any(to_region(at, location.x, location.y).is_some() for at in locations) else Rank.REST
                        return node, socket, target, size, nested, scene, rank

                    return tuple(itertools.starmap(each, found))

                return paired(enclosing(space), targets(identity, editor, tree, settings)).map(lambda found: rows(*found))

            def hashed(candidates: tuple[_Candidate, ...]) -> Outcome[tuple[Job, ...]]:
                roots = tuple(dict.fromkeys(root for node, _socket, _target, _size, nested, _scene, _rank in candidates for root in (node, *nested)))

                def ready(lighting: Mapping[RenderKind, Lighting], hashes: Mapping[bpy.types.Node | bpy.types.Image, Option[ContentHash]]) -> Outcome[tuple[Job, ...]]:
                    return accumulated(
                        demanded(node, target, size, nested, scene, lighting[target.kind], hashes).map(lambda demand: (socket, target, size, demand, rank))
                        for node, socket, target, size, nested, scene, rank in candidates
                    ).bind(partial(scheduled, lighting))

                def read(lighting: Mapping[RenderKind, Lighting]) -> Outcome[tuple[Job, ...]]:
                    subjects: tuple[bpy.types.Node | bpy.types.Image, ...] = (*roots, *tuple(dict.fromkeys(image for _lighting, _world, images in lighting.values() for image in images)))
                    return fingerprinted(subjects).bind(lambda read: ready(lighting, dict(zip(subjects, read))))

                return accumulated(prepared(kind).map(lambda found: (kind, found)) for kind in dict.fromkeys(target.kind for _node, _socket, target, _size, _nested, _scene, _rank in candidates)).bind(
                    lambda lighting: read(dict(lighting))
                )

            def scheduled(lighting: Mapping[RenderKind, Lighting], plans: tuple[tuple[bpy.types.NodeSocket, Target, int, Hashable, Rank], ...]) -> Outcome[tuple[Job, ...]]:
                state.demands.held = {target: demand for _socket, target, _size, demand, _rank in plans}
                kept = frozenset(demand for *_, demand, held in plans if held is not Rank.REST)

                def shown(ready: frozenset[Hashable]) -> Outcome[tuple[Job, ...]]:
                    grouped: defaultdict[Editor, list[Cell]] = defaultdict(list)
                    for socket, target, _size, demand, _rank in plans:
                        grouped[target.editor].append(
                            Cell(node=target.node, key=demand, label=socket.name, value=Option.of_optional(NODE_TEXTURES[identity].held.get(demand)).bind(lambda held: held[1]))
                        )
                    cells = {editor: tuple(items) for editor, items in grouped.items()}
                    changed, state.cells = cells != state.cells, cells
                    return (self.signal.fired() if changed else Ok(None)).map(
                        lambda _: tuple(
                            Job(place=target, key=demand, rank=rank, run=partial(self.thumbnailed, identity, target, demand, size, kept, settings.limit, light, world), playback=settings.frames)
                            for _socket, target, size, demand, rank in plans
                            if demand not in ready
                            for light, world, _images in (lighting[target.kind],)
                        )
                    )

                return (
                    accumulated(
                        Ok(Some(key))
                        if key in NODE_TEXTURES[identity].held
                        else (
                            inventory().bind(
                                lambda files: restored(key, path, config, name, settings.limit, kept).map(lambda _: Some(key)) if (path := _thumbnail(identity, key)) in files else Ok(Nothing)
                            )
                            if isinstance(key, bytes)
                            else Ok(Nothing)
                        )
                        for key in dict.fromkeys(demand for *_, demand, _rank in plans)
                    )
                    .map(lambda parts: frozenset(key for part in parts for key in part.to_list()))
                    .bind(shown)
                )

            return accumulated(viewed_in(editor, *sources) for editor, sources in views.items()).map(lambda parts: tuple(itertools.chain.from_iterable(parts))).bind(hashed)

        return NODE_PREVIEWS.read(identity).bind(
            lambda settings: (accumulated(map(located, areas(manager.windows))).map(lambda _: None) if settings.enabled else Ok(None)).bind(
                lambda _: planned(settings, PyOpenColorIO.GetCurrentConfig(), identity.identifier(PreviewScene.value))
            )
        )

    # --- [REGISTRATION]

    def section(self, header: tuple[Row, ...] = ()) -> Section:
        """Returns node previews section: the commands and the active node settings."""
        active = ("edit_tree", "nodes", "active")
        return Section(
            key=self.key,
            header=header,
            rows=(
                *(OperatorButton(target=row) for row in (self.toggle, self.refresh, self.clear, self.export)),
                Section(
                    key=f"{self.key}_active",
                    title=Some("Active Node"),
                    store=Some(
                        Store(
                            attachment=SETTING,
                            host=Some(Member(root=bpy.types.SpaceNodeEditor, path=active)),
                            retention=self.retention,
                            fields={"output": lambda member: Property(target=member, search=Some(Member(root=bpy.types.SpaceNodeEditor, path=(*active, "outputs"))))},
                        )
                    ),
                ),
            ),
        )

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns preview declarations, state, resources, overlay, and handlers, updates of a scene under a preview render left to the render."""

        def forgotten(switch: Callable[[NodePreviews], bool]) -> Outcome[None]:
            read = NODE_PREVIEWS.read(identity)
            for _ in read.to_option().filter(switch).to_list():
                NODE_STATES[identity].demands.forgotten()
            return read.map(lambda _: None)

        def dropped(scene: bpy.types.Scene, _updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
            if scene.session_uid in INTERMEDIATES[identity]:
                return Ok(None)
            return forgotten(lambda settings: settings.automatic)

        overlay = Overlay(drawing=Drawing(area=AreaQuery(space=bpy.types.SpaceNodeEditor), part=NodeThumbnails(identity=identity), refreshes=(self.signal,)))
        keymap = Keymap(
            keymap="Node Editor",
            bindings=((Chord(type="P", ctrl=1, alt=1), Placement(self.toggle)), (Chord(type="Z", ctrl=1, alt=1), Placement(self.enlarge)), (Chord(type="R", ctrl=1, alt=1), Placement(self.refresh))),
        )
        return (
            self.preview,
            INTERMEDIATES,
            SETTING,
            NODE_STATES,
            NODE_PREVIEWS,
            keymap,
            CLAY,
            SWATCH,
            NODE_TEXTURES,
            PreviewFeed(feed=self.jobs),
            self.toggle,
            self.enlarge,
            self.refresh,
            self.clear,
            mark_node_previews,
            self.export,
            Append(target=NODE_MT_context_menu, rows=(OperatorButton(target=mark_node_previews), OperatorButton(target=self.export), OperatorButton(target=self.refresh))),
            overlay,
            Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.ID,), dropped)),
            Handler(handlers=bpy.app.handlers.frame_change_post, body=lambda: forgotten(lambda settings: settings.frames)),
        )

    def rerendered(self, identity: Identity, paths: Option[tuple[Path, ...]] = Nothing) -> Outcome[None]:
        """Removes held textures, failures, and demand files, or supplied files, for a fresh render."""
        dropped = NODE_STATES[identity].demands.forgotten()
        PreviewQueue.forced(identity, dropped)
        NODE_TEXTURES[identity].cleared()
        files = paths.default_with(lambda: tuple(_thumbnail(identity, key) for key in dict.fromkeys(dropped.values()) if isinstance(key, bytes)))
        return accumulated(removed(path) for path in files).bind(lambda _: self.signal.fired())


# --- [OPERATIONS] -----------------------------------------------------------------------


def _folder(identity: Identity) -> Path:
    """Folder holding an extension's stored thumbnails."""
    return identity.folder / NodePreview.key


def _thumbnails(identity: Identity) -> Outcome[tuple[Path, ...]]:
    """Stored thumbnail files of an extension."""
    folder = _folder(identity)
    try:
        return Ok(tuple(path for path in folder.iterdir() if path.suffix == _SUFFIX))
    except FileNotFoundError:
        return Ok(())
    except OSError as error:
        return Error(Fault.of(Reason.UNREADABLE, str(folder), error))


def _thumbnail(identity: Identity, key: ContentHash) -> Path:
    """File storing the thumbnail of a content hash."""
    return _folder(identity) / f"{key.hex()}{_SUFFIX}"


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(Empty, condition=Some(Setting(descriptor=Descriptor(root=bpy.types.SpaceNodeEditor, owner=("edit_tree",), prop=bpy.types.ID.bl_rna.properties["is_editable"]), accepted=(True,))))
def mark_node_previews(call: Call[Empty]) -> Outcome[Answer]:
    """Toggle the node preview each selected node shows under the Marked scope."""
    if not (nodes := tuple(call.context.selected_nodes)):
        return Ok(Unchanged(reason="No node selected"))
    edits = tuple(written(node, ("show_preview",), not node.show_preview) for node in nodes)
    return tallied(edits, (("INFO", f"Toggled {sum(edit.is_ok() for edit in edits)} node marks"),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["NodePreview", "ThumbnailExport", "mark_node_previews"]
