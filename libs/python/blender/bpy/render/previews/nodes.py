"""Per-node previews over every geometry node editor: the commands, the feed planning a job per changed target, and the stored thumbnails each render writes."""

from collections.abc import Callable, Hashable
from functools import partial, reduce
import itertools
import os
from pathlib import Path
from typing import ClassVar, Final

from bl_ui.space_node import NODE_MT_context_menu
import bpy
from expression import Error, Nothing, Ok, Option, Result, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import gpu
import msgspec
import OpenImageIO
import PyOpenColorIO

from blender.bpy.commands.calls import Placement
from blender.bpy.commands.invocations import Save
from blender.bpy.commands.keymaps import Chord, Keymap
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, tallied, Unchanged
from blender.bpy.data.ids.images import fingerprinted, loaded, Tag
from blender.bpy.data.ids.store import STORES
from blender.bpy.data.rna.descriptors import written
from blender.bpy.data.rna.paths import rooted
from blender.bpy.extension.events import Change, Handler, updated
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.nodes.trees import socket_at
from blender.bpy.render.color.spaces import UNTAGGED, viewed
from blender.bpy.render.outputs.results import INTERMEDIATES
from blender.bpy.render.previews.queue import BACKLOG, Job, PreviewQueue, Rank
from blender.bpy.render.previews.scenes import PreviewScene, Readback
from blender.bpy.render.previews.targets import CLAY, Editor, enclosing, NodePreviews, rendered, SETTING, SWATCH, Target, targets
from blender.bpy.render.previews.thumbnails import Cell, Counts, Demand, NODE_STATES, NodeThumbnails, Staged, Status
from blender.bpy.ui.drawing.handlers import Drawing, Overlay, Signal
from blender.bpy.ui.layout.rows import Member, OperatorButton, Property, Row
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Store
from blender.bpy.ui.resources.previews import TEXTURES
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.screens.areas import AreaQuery, areas, Located, to_region
from blender.bpy.ui.surfaces.menus import Append
from blender.imaging.files.buffers import Depth
from blender.imaging.files.images import Format, ImageFile, read_file, Storage
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason
from blender.records.fields import Retention
from blender.records.stores import content_hash, ContentHash, removed

# --- [CONSTANTS] ------------------------------------------------------------------------

STORED: Final = tuple(kind for kind in STORES if issubclass(kind, bpy.types.ID) and kind not in {bpy.types.ID, bpy.types.Object})
_SUFFIXES: Final[tuple[str, *tuple[str, ...]]] = (".exr", *(suffix for row in Format.writable() for suffix in row.suffixes if suffix != ".exr"))

# --- [MODELS] ---------------------------------------------------------------------------


class ThumbnailExport(bpy.types.PropertyGroup):
    """Exported thumbnail size and image load after write."""

    size: bpy.props.IntProperty(name="Size", description="Image side in pixels", subtype="PIXEL", min=256, max=2048, default=512)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    opened: bpy.props.BoolProperty(name="Open Image", description="Load written image", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------


class NodePreview(msgspec.Struct, frozen=True, kw_only=True):
    """Row of per-node previews over every node editor, its commands equal across reads."""

    settings: Callable[[Identity], Outcome[NodePreviews]]
    retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing
    preview: PreviewScene = PreviewScene()
    signal: Signal = msgspec.field(default_factory=Signal)
    key: ClassVar[Key] = "node_previews"

    # --- [COMMANDS]

    @property
    def toggle(self) -> Operator[Empty]:
        """Command toggling node editor thumbnails."""
        return Operator(form=Empty, undo=Nothing, run=self.toggle_node_previews)

    @property
    def enlarge(self) -> Operator[Empty]:
        """Command toggling the larger active node thumbnail."""
        return Operator(form=Empty, undo=Nothing, run=self.enlarge_node_preview)

    @property
    def refresh(self) -> Operator[Empty]:
        """Command rendering every thumbnail again."""
        return Operator(form=Empty, undo=Nothing, run=self.refresh_node_previews)

    @property
    def clear(self) -> Operator[Empty]:
        """Command removing cached and stored thumbnails."""
        return Operator(form=Empty, undo=Nothing, run=self.clear_node_previews)

    @property
    def export(self) -> Operator[ThumbnailExport]:
        """Command writing the active node thumbnail to an image file."""
        return Operator(form=ThumbnailExport, undo=Nothing, invoke=Some(Save(extensions=lambda _: _SUFFIXES)), run=self.export_node_preview)

    def flipped(self, call: Call[Empty], change: Callable[[NodePreviews], Outcome[object]]) -> Outcome[Answer]:
        """Writes the change into the settings and refreshes thumbnails."""
        return self.settings(call.identity).bind(change).bind(lambda _: self.signal.fired()).map(lambda _: Done())

    def toggle_node_previews(self, call: Call[Empty]) -> Outcome[Answer]:
        """Toggles node editor thumbnails."""
        return self.flipped(call, lambda held: written(held, ("enabled",), not held.enabled))

    def enlarge_node_preview(self, call: Call[Empty]) -> Outcome[Answer]:
        """Toggles the larger active node thumbnail on top."""
        return self.flipped(call, lambda held: written(held, ("enlarged",), not held.enlarged))

    def refresh_node_previews(self, call: Call[Empty]) -> Outcome[Answer]:
        """Renders every thumbnail again."""
        return self.rerendered(call.identity).map(lambda _: Done())

    def clear_node_previews(self, call: Call[Empty]) -> Outcome[Answer]:
        """Empties texture cache, held values, and previews folder, then renders every thumbnail again."""
        files = tuple(_folder(call.identity).glob("*.exr"))
        call.identity.held[TEXTURES].cleared()
        call.identity.held[NODE_STATES].cleared()
        return accumulated(removed(path) for path in files).bind(lambda _: self.rerendered(call.identity)).map(lambda _: Done(notes=(("INFO", f"Removed {len(files)} stored thumbnails"),)))

    def export_node_preview(self, call: Call[ThumbnailExport]) -> Outcome[Answer]:
        """Writes the active node thumbnail to an image file in the scene display space."""
        space = call.context.space_data
        shown: Outcome[tuple[Editor, bpy.types.NodeTree, bpy.types.Node]] = (
            Option
            .of_optional(space if isinstance(space, bpy.types.SpaceNodeEditor) else None)
            .bind(
                lambda found: Editor.of(found).bind(lambda editor: Option.of_optional(found.edit_tree).bind(lambda tree: Option.of_optional(tree.nodes.active).map(lambda node: (editor, tree, node))))
            )
            .to_result_with(partial(Fault.of, Reason.ABSENT, "SpaceNodeEditor.edit_tree.nodes", member="active"))
        )

        def chosen(editor: Editor, tree: bpy.types.NodeTree, node: bpy.types.Node, settings: NodePreviews) -> Outcome[Target]:
            return targets(call.identity, editor, tree, settings).bind(
                lambda found: (
                    Block.of_seq(found).filter(lambda pair: pair[0] == node).try_head().map(lambda pair: pair[1]).to_result_with(partial(Fault.of, Reason.ABSENT, repr(tree.nodes), member=node.name))
                )
            )

        def wrote(scene: bpy.types.Scene, target: Target, settings: NodePreviews) -> Outcome[Path]:
            path = call.paths[0]
            return paired(rendered(call.identity, self.preview, target, call.properties.size, settings, scene=scene), Format.of(path)).bind(
                lambda pair: ImageFile(pixels=pair[0].plane, colorspace=pair[0].colorspace).written(path, Storage(depth=pair[1].depths[-1]), {}, PyOpenColorIO.GetCurrentConfig())
            )

        return (
            paired(shown, self.settings(call.identity), rooted(bpy.types.Scene, call.context))
            .bind(lambda parts: chosen(*parts[0], parts[1]).bind(lambda target: wrote(parts[2], target, parts[1])))
            .bind(lambda name: loaded(str(name), Tag()).map(lambda _: name) if call.properties.opened else Ok(name))
            .map(lambda name: Done.wrote((name,)))
        )

    # --- [THUMBNAILS]

    def thumbnailed(self, identity: Identity, target: Target, demand: Demand, size: int, kept: frozenset[Hashable], settings: NodePreviews, *, scene: bpy.types.Scene) -> Outcome[None]:
        """Renders a target into texture cache and previews folder, its uniform value held and stored as a typed header attribute."""
        state, stored_in, limit, config = identity.held[NODE_STATES], _folder(identity), settings.limit, PyOpenColorIO.GetCurrentConfig()

        def pruned() -> Outcome[None]:
            files: Outcome[list[Path]] = catch(sorted, exception=OSError)(stored_in.glob("*.exr"), key=lambda path: path.stat().st_mtime).map_error(
                partial(Fault.of, Reason.UNREADABLE, str(stored_in))
            )
            return Result[list[Path], Faults].bind(files, lambda found: accumulated(removed(path) for path in found[:-limit])).map(lambda _: None)

        def stored(readback: Readback, name: str) -> Outcome[None]:
            number = readback.uniform

            def written_file(digest: ContentHash) -> Outcome[None]:
                header = tuple(OpenImageIO.ParamValue(name, held) for held in number.to_list())
                return (
                    ImageFile(pixels=readback.plane, colorspace=readback.colorspace, header=header)
                    .written(stored_in / f"{digest.hex()}.exr", Storage(depth=Depth.HALF), {}, config)
                    .bind(lambda _: pruned())
                )

            def saved(_texture: gpu.types.GPUTexture) -> Outcome[None]:
                for held in number.to_list():
                    state.values[demand.key] = held
                return demand.content.map(written_file).default_value(Ok(None)).bind(lambda _: self.signal.fired())

            return identity.held[TEXTURES].stored(demand.key, readback.plane, limit, kept).bind(saved) if state.keys.get(target) is demand else Ok(None)

        return paired(rendered(identity, self.preview, target, size, settings, scene=scene), identity.identifier(PreviewScene.value)).bind(lambda parts: stored(*parts))

    # --- [FEED]

    def jobs(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Job, ...]]:
        """Writes editor cells and returns a job per target key neither cache nor folder holds."""
        state, metrics, failed = identity.held[NODE_STATES], Metrics.read(), identity.held[BACKLOG].failed
        prior, state.keys = state.keys, {}
        manager: bpy.types.WindowManager = context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        views = tuple(
            (editor, space, at, tree)
            for found in areas(manager.windows)
            for space in (found.area.spaces.active,)
            if isinstance(space, bpy.types.SpaceNodeEditor)
            for at in found.within().to_option().to_list()
            for editor in Editor.of(space).to_list()
            for tree in Option.of_optional(space.edit_tree).to_list()
        )

        def keyed(node: bpy.types.Node, target: Target, staged: Staged, nested: tuple[bpy.types.Node, ...]) -> Outcome[Option[ContentHash]]:
            editor = target.editor
            place = (editor.item, editor.modifier)
            source = (
                Option
                .of_optional(state.held.get(place))
                .map(Ok)
                .default_with(lambda: editor.item.resolve().bind(lambda item: fingerprinted(item, entering=Some(editor.modifier))).map(lambda key: state.held.setdefault(place, key)))
            )
            return paired(fingerprinted(node), accumulated(fingerprinted(group) for group in nested), source).map(
                lambda parts: reduce(
                    lambda own, key: own.map2(lambda held, current: content_hash(Target, held, current), key), (*parts[1], parts[2]), parts[0].map(lambda own: content_hash(Target, staged, own))
                )
            )

        def restored(key: ContentHash, config: PyOpenColorIO.Config, name: str, limit: int, kept: frozenset[Hashable]) -> Outcome[bool]:
            path = _folder(identity) / f"{key.hex()}.exr"

            def cached(image: ImageFile) -> Outcome[bool]:
                for number in Block.of_seq(image.header).filter(lambda field: field.name == name).choose(lambda field: Option.of_optional(try_downcast(float, field.value)))[-1:]:
                    state.values[key] = number
                return (
                    identity
                    .held[TEXTURES]
                    .stored(key, image.pixels, limit, kept)
                    .bind(lambda _: catch(os.utime, exception=OSError)(path).map_error(partial(Fault.of, Reason.UNWRITABLE, str(path))))
                    .map(lambda _: True)
                )

            match read_file(path, config, untagged=Some(UNTAGGED)):
                case Result(tag="error", error=faults) if all(fault.reason in {Reason.UNREADABLE, Reason.UNWRITABLE} for fault in faults):
                    return Ok(False)
                case Result(tag="error", error=faults):
                    return Error(faults)
                case Result(ok=image):
                    return cached(image)

        def planned(scene: bpy.types.Scene, settings: NodePreviews, config: PyOpenColorIO.Config, name: str) -> Outcome[tuple[Job, ...]]:
            def demanded(node: bpy.types.Node, target: Target, size: int, nested: tuple[bpy.types.Node, ...]) -> Outcome[Demand]:
                staged = Staged(
                    kind=target.kind,
                    socket=target.socket,
                    engine=scene.render.engine,
                    size=size,
                    lighting=settings.lighting,
                    view=viewed(scene),
                    frame=(scene.frame_current,) if settings.frames else (),
                    host=(bpy.app.version, bpy.app.build_hash),
                    config=config.getCacheID(),
                )

                def remembered(digest: Option[ContentHash]) -> Demand:
                    state.keys[target] = demand = (
                        Option.of_optional(prior.get(target)).filter(lambda held: held.staged == staged and held.content == digest).default_with(lambda: Demand(staged=staged, content=digest))
                    )
                    return demand

                return (
                    Option
                    .of_optional(prior.get(target))
                    .filter(lambda held: held.staged == staged)
                    .map(lambda held: Ok(remembered(held.content)))
                    .default_with(lambda: keyed(node, target, staged, nested).map(remembered))
                )

            def viewed_in(editor: Editor, space: bpy.types.SpaceNodeEditor, at: Located, tree: bpy.types.NodeTree) -> Outcome[tuple[tuple[bpy.types.Node, Target, int, Demand, Rank], ...]]:
                nested = tuple(node for found in enclosing(space) for node in found.to_list())

                def rows(found: tuple[tuple[bpy.types.Node, Target], ...]) -> Outcome[tuple[tuple[bpy.types.Node, Target, int, Demand, Rank], ...]]:
                    def each(node: bpy.types.Node, target: Target) -> Outcome[tuple[bpy.types.Node, Target, int, Demand, Rank]]:
                        size = round(node.width * metrics.scale * settings.scale / (2 if sum(1 for held, _ in found if held == node) > 1 else 1))
                        location = node.location_absolute
                        rank = Rank.ACTIVE if node.select or node == tree.nodes.active else Rank.SHOWN if to_region(at, location.x, location.y).is_some() else Rank.REST
                        return demanded(node, target, size, nested).map(lambda demand: (node, target, size, demand, rank))

                    return accumulated(itertools.starmap(each, found))

                return targets(identity, editor, tree, settings).bind(rows)

            def scheduled(plans: tuple[tuple[bpy.types.Node, Target, int, Demand, Rank], ...]) -> Outcome[tuple[Job, ...]]:
                kept = frozenset(demand.key for *_, demand, held in plans if held is not Rank.REST)

                def shown(ready: frozenset[Hashable]) -> Outcome[tuple[Job, ...]]:
                    cells = {
                        editor: tuple(
                            Cell(
                                node=node.name,
                                key=demand.key,
                                label=socket_at(node.outputs, target.socket).map(lambda socket: socket.name).default_value(node.name),
                                status=Status.READY if demand.key in ready else Status.FAILED if ((Some(self.jobs), target), demand.key) in failed.items() else Status.QUEUED,
                                value=Option.of_optional(state.values.get(demand.key)),
                            )
                            for node, target, _size, demand, _rank in plans
                            if target.editor == editor
                        )
                        for editor in dict.fromkeys(target.editor for _node, target, *_ in plans)
                    }
                    changed, state.cells = cells != state.cells, cells
                    return (self.signal.fired() if changed else Ok(None)).map(
                        lambda _: tuple(
                            Job(place=target, key=demand.key, rank=rank, run=partial(self.thumbnailed, identity, target, demand, size, kept, settings, scene=scene), playback=settings.frames)
                            for _node, target, size, demand, rank in plans
                            if demand.key not in ready
                        )
                    )

                return (
                    accumulated(
                        Ok(Some(key))
                        if key in identity.held[TEXTURES].held
                        else digest.map(lambda found: restored(found, config, name, settings.limit, kept).map(lambda held: Some(key) if held else Nothing)).default_value(Ok(Nothing))
                        for key, digest in {demand.key: demand.content for *_, demand, _rank in plans}.items()
                    )
                    .map(lambda parts: frozenset(key for part in parts for key in part.to_list()))
                    .bind(shown)
                )

            return accumulated(itertools.starmap(viewed_in, views if settings.enabled else ())).map(lambda parts: tuple(itertools.chain.from_iterable(parts))).bind(scheduled)

        return paired(rooted(bpy.types.Scene, context), self.settings(identity), identity.identifier(PreviewScene.value)).bind(
            lambda parts: planned(parts[0], parts[1], PyOpenColorIO.GetCurrentConfig(), parts[2])
        )

    # --- [REGISTRATION]

    def section(self, header: tuple[Row, ...] = ()) -> Section:
        """Returns node previews section."""
        active = ("edit_tree", "nodes", "active")
        return Section(
            key=self.key,
            title=bpy.path.display_name(self.key),
            header=header,
            rows=(
                *(OperatorButton(target=row) for row in (self.toggle, self.refresh, self.clear, self.export)),
                Counts(),
                Section(
                    key=f"{self.key}_active",
                    title="Active Node",
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

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns preview declarations, state, resources, overlay, and handlers, updates of a scene under a preview render left to the render."""

        def forgotten(switch: Callable[[NodePreviews], bool]) -> Outcome[None]:
            read = self.settings(identity)
            for _ in read.to_option().filter(switch).to_list():
                identity.held[NODE_STATES].forgotten()
            return read.map(lambda _: None)

        def dropped(scene: bpy.types.Scene, updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
            if scene.session_uid in identity.held[INTERMEDIATES]:
                return Ok(None)
            for block in (held for update in updates for held in Option.of_optional(update.id).to_list()):
                identity.held[NODE_STATES].released(block)
            return forgotten(lambda settings: settings.automatic)

        overlay = Overlay(drawing=Drawing(area=AreaQuery(space=bpy.types.SpaceNodeEditor), part=NodeThumbnails(identity=identity, settings=self.settings), refreshes=(self.signal,)))
        keymap = Keymap(
            keymap="Node Editor",
            bindings=((Chord(type="P", ctrl=1, alt=1), Placement(self.toggle)), (Chord(type="Z", ctrl=1, alt=1), Placement(self.enlarge)), (Chord(type="R", ctrl=1, alt=1), Placement(self.refresh))),
        )
        return Ok((
            self.preview,
            INTERMEDIATES,
            SETTING,
            NODE_STATES,
            BACKLOG,
            keymap,
            CLAY,
            SWATCH,
            TEXTURES,
            (Band.RESOURCES, PreviewQueue.fed(identity, self.jobs)),
            self.toggle,
            self.enlarge,
            self.refresh,
            self.clear,
            mark_node_previews,
            self.export,
            Append(target=NODE_MT_context_menu, rows=(OperatorButton(target=mark_node_previews), OperatorButton(target=self.export), OperatorButton(target=self.refresh))),
            overlay,
            Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, STORED, dropped)),
            Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.Object,), dropped, changes=Some(Change.GEOMETRY | Change.SHADING))),
            Handler(handlers=bpy.app.handlers.frame_change_post, body=lambda: forgotten(lambda settings: settings.frames)),
        ))

    def rerendered(self, identity: Identity) -> Outcome[None]:
        """Drops target keys and held failures for a full rerender."""
        PreviewQueue.forced(identity, identity.held[NODE_STATES].forgotten())
        return self.signal.fired()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _folder(identity: Identity) -> Path:
    """Folder holding an extension's stored thumbnails."""
    return identity.folder / NodePreview.key


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(Empty)
def mark_node_previews(call: Call[Empty]) -> Outcome[Answer]:
    """Toggles the node preview each selected node shows under the Marked scope."""
    if not (nodes := tuple(call.context.selected_nodes)):
        return Ok(Unchanged(reason="No node selected"))
    edits = tuple(written(node, ("show_preview",), not node.show_preview) for node in nodes)
    return tallied(edits, (("INFO", f"Toggled {sum(edit.is_ok() for edit in edits)} node marks"),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["NodePreview", "ThumbnailExport", "mark_node_previews"]
