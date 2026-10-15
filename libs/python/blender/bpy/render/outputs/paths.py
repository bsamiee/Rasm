"""Scene output grammar, native stored names, versioning, and planned text edits."""

from collections.abc import Callable, Generator, Iterable
from contextlib import contextmanager
import datetime
from functools import reduce
from itertools import chain, count
from pathlib import Path
import posixpath
import re
from typing import Annotated, ClassVar, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Call, Done, Empty, Operator, Outcome, Suppressible, tallied, Unchanged
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import AllOf, existing, Not, Setting
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.extension.events.handlers import PathHandler
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderRenderingFault, BlenderResult, BlenderUnmetFault, BlenderWriteFault
from blender.bpy.files.blends import BlendFile
from blender.bpy.render.outputs.formats import Kind
from blender.bpy.settings.attachments import Attached, Attachment, Stored
from blender.bpy.settings.properties import Changed
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Gated
from blender.rendering.outputs.naming import Count, Field, Fields, Number, Output, OutputName, Slot, stepped, stored_name, Word

# --- [MODELS] ---------------------------------------------------------------------------


class Outputs(msgspec.Struct, frozen=True, kw_only=True):
    """Scene output template, source-file grammar, take, and project-variable root."""

    template: Annotated[str, msgspec.Meta(description="Path of each written file under the output folder")] = "{file}/v{version:3}/{layer}/{pass}/{frame:4}"
    stem: Annotated[str, msgspec.Meta(title="File Name Grammar", description="Grammar of project, shot, and take in the blend file name")] = "{project}_{shot}"
    version: Annotated[int, msgspec.Meta(ge=1, description="Take used where the file name holds none")] = 1
    variable: Annotated[Gated[str], msgspec.Meta(title="Project Variable", description="Project path variable, the preference folder while disabled")] = Gated(enabled=False, value="renders")


class Replace(msgspec.Struct, frozen=True, kw_only=True):
    """Pattern, replacement text, regex mode, and case sensitivity."""

    find: Annotated[str, msgspec.Meta(description="Text found in selected node paths, labels, and item names")] = ""
    replace: Annotated[str, msgspec.Meta(description=r"Replacement text; regex groups use \1 or \g<name>")] = ""
    regex: Annotated[bool, msgspec.Meta(title="Regular Expression", description="Read Find as a Python regular expression")] = False
    case: Annotated[bool, msgspec.Meta(title="Match Case", description="Match upper and lower case as written")] = False


class Bump(msgspec.Struct, frozen=True, kw_only=True):
    """Version-token offset."""

    offset: Annotated[int, msgspec.Meta(description="Versions each token moves by")] = 1


class Undated(msgspec.Struct, frozen=True, kw_only=True):
    """Render-time path containing a date Blender cannot expand."""

    @property
    def message(self) -> str:
        """Unsupported render-time field requirement."""
        return "Requires a template without {date}, which Blender does not expand"


class Malformed(msgspec.Struct, frozen=True, kw_only=True):
    """Pattern or replacement template refused by the native regex parser."""

    text: str
    error: re.PatternError

    @property
    def message(self) -> str:
        """Refused text and parser's explanation."""
        return f"Requires a regular expression Python reads: {self.text} ({self.error})"


class Rename(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Native string edit with its original, requested, and predicted stored spelling."""

    node: bpy.types.Node
    owner: bpy.types.Node | bpy.types.NodeCompositorFileOutputItem
    prop: bpy.types.StringProperty
    before: str
    requested: str
    stored: str

    @classmethod
    def field(cls, node: bpy.types.Node, prop: bpy.types.StringProperty, before: str, requested: str) -> Self:
        """Edit of a node string member, its stored spelling cut to the member's byte capacity."""
        return cls(node=node, owner=node, prop=prop, before=before, requested=requested, stored=requested.encode()[: prop.length_max - 1].decode(errors="ignore") if prop.length_max else requested)


class NodePaths(msgspec.Struct, frozen=True, kw_only=True):
    """Native path member values and settled File Output item names."""

    members: dict[Descriptor, Value]
    items: tuple[str, ...]


class OutputPath(msgspec.Struct, frozen=True, kw_only=True):
    """Composed scene output request with literal and native-template spellings."""

    name: OutputName
    root: str
    base: Path
    fields: Fields
    orders: dict[str, int]
    outputs: Outputs
    spelled: frozenset[Word] = frozenset({Word.SCENE, Word.CAMERA, Word.FILE})

    @property
    def copied(self) -> Self:
        """Job-copy path retaining the source file's literal folder and file name."""
        return msgspec.structs.replace(self, root=self.base.as_posix(), spelled=self.spelled - {Word.FILE})

    def rerooted(self, base: Path) -> Self:
        """Retains acquired naming facts under the worker's mapped output directory."""
        return msgspec.structs.replace(self, base=base, root=base.as_posix())

    def spelling(self, part: Field) -> Option[str]:
        """Returns native variables for selected words and frame padding."""
        match part:
            case Slot(key=key):
                return Option.of_optional({Word.SCENE: "{scene_name}", Word.CAMERA: "{camera_name}", Word.FILE: "{blend_name}"}.get(key)).filter(lambda _: key in self.spelled)
            case Number(key=Count.FRAME, width=width):
                return Some("#" * width)
            case Number():
                return Nothing

    @classmethod
    def of(cls, outputs: Outputs, scene: bpy.types.Scene, root: tuple[str, Path], blend: Option[Path], day: datetime.date, unsaved: Not) -> BlenderResult[Self]:
        """Combines both grammars and their source-file fields, retaining all grammar faults."""
        native, base = root

        def built(name: OutputName, stem: OutputName) -> BlenderResult[Self]:
            words = frozenset(part.key for part in name.slots if isinstance(part, Slot)) & frozenset(part.key for part in stem.slots if isinstance(part, Slot))
            counted = all(any(isinstance(part, Number) and part.key is Count.VERSION for part in grammar.slots) for grammar in (name, stem))
            parsed = (
                Result[Path, BlenderFaults].bind(blend.to_result_with(lambda: (BlenderUnmetFault(requirement=unsaved),)), lambda path: stem.parse(path.stem).map_error(BlenderRenderingFault.lifted))
                if words or counted
                else Ok(Fields())
            )
            host = (
                {Word.SCENE: scene.name, Word.DATE: day.isoformat()}
                | dict(Option.of_optional(scene.camera).map(lambda camera: (Word.CAMERA, camera.name)).to_list())
                | dict(blend.map(lambda path: (Word.FILE, path.stem)).to_list())
            )
            return parsed.map(
                lambda found: cls(
                    name=name,
                    root=native,
                    base=base,
                    outputs=outputs,
                    fields=Fields(words=host | {key: value for key, value in found.words.items() if key in words}, counts={Count.VERSION: found.counts[Count.VERSION] if counted else outputs.version}),
                    orders={layer.name: position for position, layer in enumerate(scene.view_layers, 1)},
                )
            )

        return Result[tuple[OutputName, OutputName], BlenderFaults].bind(
            paired(OutputName.of(outputs.template).map_error(BlenderRenderingFault.lifted), OutputName.of(outputs.stem).map_error(BlenderRenderingFault.lifted)), lambda pair: built(*pair)
        )

    def _filled(self, layer: str, output: Output, item: Option[str], frame: Option[int] = Nothing) -> Fields:
        named = output.name.lower()
        return Fields(
            words=self.fields.words | {Word.LAYER: layer, Word.OUTPUT: named, Word.PASS: item.default_value(named)},
            counts=self.fields.counts
            | dict(Option.of_optional(self.orders.get(layer)).map(lambda order: (Count.ORDER, order)).to_list())
            | dict(frame.map(lambda held: (Count.FRAME, held)).to_list()),
        )

    def text(self, layer: str, output: Output, item: Option[str] = Nothing, *, spelled: bool = True) -> BlenderResult[str]:
        """Composes a relative native template or returns unsupported-date and grammar faults."""
        return (
            Error((BlenderUnmetFault(requirement=Undated()),))
            if spelled and Slot(key=Word.DATE) in self.name.slots
            else self.name.compose(self._filled(layer, output, item), self.spelling if spelled else lambda _: Nothing).map_error(BlenderRenderingFault.lifted)
        )

    def scene_output(self, scene: bpy.types.Scene) -> BlenderResult[str]:
        """Composes the first view layer's preview output under the native root."""
        return self.text(scene.view_layers[0].name, Output.PREVIEW).map(lambda text: posixpath.join(self.root, text))

    def written(self, layer: str, output: Output, item: Option[str], frame: int, suffix: str) -> BlenderResult[Path]:
        """Composes a literal written-file path, retaining naming and containment refusals."""
        return self.name.path(self.base, self._filled(layer, output, item, Some(frame))).map(lambda path: path.with_name(f"{path.name}{suffix}")).map_error(BlenderRenderingFault.lifted)

    def next_version(self) -> int:
        """Returns the take after matching scene-level files, excluding the calendar day."""
        held = frozenset(part.key for part in self.name.slots if isinstance(part, Slot) and part.key is not Word.DATE)
        return self.name.next_version(self.base, Fields(words={key: value for key, value in self.fields.words.items() if key in held}))

    def removed(self, scene: bpy.types.Scene, frames: range) -> BlenderResult[None]:
        """Removes native scene outputs within the job root, accumulating actual unlink refusals."""

        def dropped(frame: int) -> BlenderResult[None]:
            return Result[Path, BlenderFaults].bind(
                catch(lambda: Path(bpy.path.abspath(scene.render.frame_path(frame=frame))), exception=RuntimeError)().map_error(
                    lambda error: (BlenderEditFault(request=repr(scene.render), error=error),)
                ),
                lambda file: (
                    catch(lambda: file.unlink(missing_ok=True), exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=file, error=error),))
                    if bpy.path.is_subdir(str(file), str(self.base))
                    else Ok(None)
                ),
            )

        return accumulated(map(dropped, frames)).map(lambda _: None)


class Pathed(msgspec.Struct, frozen=True, kw_only=True):
    """Bound scene output settings and native descriptors shared by every output consumer."""

    filepath: ClassVar[Descriptor] = Descriptor(root=bpy.types.RenderSettings, owner="", prop=bpy.types.RenderSettings.bl_rna.properties["filepath"])
    directory: ClassVar[Descriptor] = Descriptor(root=bpy.types.CompositorNodeOutputFile, owner="", prop=bpy.types.CompositorNodeOutputFile.bl_rna.properties["directory"])
    file_name: ClassVar[Descriptor] = Descriptor(root=bpy.types.CompositorNodeOutputFile, owner="", prop=bpy.types.CompositorNodeOutputFile.bl_rna.properties["file_name"])
    unsaved: ClassVar[Not] = Not(condition=Setting(descriptor=Descriptor(root=bpy.types.Context, owner="blend_data", prop=bpy.types.BlendData.bl_rna.properties["filepath"]), accepted=("",)))
    attached: Attached[Outputs]
    scope: Stored[OutputPath]

    def path(self, scene: bpy.types.Scene, day: datetime.date) -> BlenderResult[OutputPath]:
        """Returns retained render paths or composes scene settings at the given day."""

        def rooted(outputs: Outputs) -> BlenderResult[tuple[str, Path]]:
            folder = bpy.context.preferences.filepaths.render_output_directory  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            return (
                Option
                .of_optional(bpy.data.project)
                .bind(lambda project: Option.of_optional(project.variables.get(outputs.variable.value)))
                .map(lambda variable: (f"{{{variable.name}}}", Path(bpy.path.abspath(variable.value))))
                .to_result_with(lambda: (BlenderAbsentFault(owner="bpy.data.project.variables", key=outputs.variable.value),))
                if outputs.variable.enabled
                else Ok((folder, Path(bpy.path.abspath(folder))))
            )

        return self.scope.read(scene).bind(
            lambda held: held.map(Ok).default_with(
                lambda: self.attached.read(scene).bind(lambda outputs: rooted(outputs).bind(lambda root: OutputPath.of(outputs, scene, root, BlendFile.read().path, day, self.unsaved)))
            )
        )

    def node(self, path: OutputPath, layer: str, output: Output, passes: tuple[str, ...], kind: Kind) -> BlenderResult[NodePaths]:
        """Builds native node members and settled items for the selected file layout."""
        match kind:
            case Kind.LAYERED:
                return path.text(layer, output).map(
                    lambda text: NodePaths(
                        members={self.directory: path.root, self.file_name: text}, items=settled(passes if Slot(key=Word.LAYER) in path.name.slots else tuple(f"{layer}.{name}" for name in passes))
                    )
                )
            case Kind.EXR | Kind.IMAGE:
                return accumulated(path.text(layer, output, Some(name)) for name in passes).map(lambda texts: NodePaths(members={self.directory: path.root, self.file_name: ""}, items=settled(texts)))

    def recomposed(self, scene: bpy.types.Scene) -> BlenderResult[None]:
        """Writes the current scene output only when its native value differs."""
        return (
            self
            .path(scene, datetime.datetime.now(datetime.UTC).astimezone().date())
            .bind(lambda path: path.scene_output(scene))
            .bind(lambda text: self.filepath.write(scene.render, text))
            .map(lambda _: None)
        )

    def refreshed(self, identity: Identity) -> BlenderResult[None]:
        """Recomposes stored nonowned-runtime scenes, retaining all independent failures."""
        return accumulated(
            self.recomposed(scene)
            for scene in bpy.data.scenes
            if self.attached.stored(scene) and not (scene.is_runtime_data and identity.manifest.id in scene)  # type: ignore[operator]
        ).map(lambda _: None)

    def took(self, scene: bpy.types.Scene, day: datetime.date) -> BlenderResult[int]:
        """Writes the next take from one scene-settings snapshot."""

        def wrote(path: OutputPath) -> BlenderResult[int]:
            version = path.next_version()
            return self.attached.write(scene, msgspec.structs.replace(path.outputs, version=version)).map(lambda _: version)

        return self.path(scene, day).bind(wrote)

    def taken(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Runs the take action and returns its native report entry."""
        return (
            call.root(bpy.types.Scene).bind(lambda scene: self.took(scene, datetime.datetime.now(datetime.UTC).astimezone().date())).map(lambda version: Done(notes=(("INFO", f"Version {version}"),)))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def settled(requested: Iterable[str], held: tuple[str, ...] = ()) -> tuple[str, ...]:
    """Predicts Blender's truncation, sanitization, and uniqueness in native item order."""
    asked = tuple(requested)

    def stored(done: tuple[str, ...], name: str) -> tuple[str, ...]:
        index, safe = len(done), stored_name(name.encode()[:1023].decode(errors="ignore"))
        taken = {*done, *(kept for later, kept in enumerate(held) if later > index and asked[later] == kept)}
        base, number = (found[1], int(found[2])) if (found := re.fullmatch(r"(.*)_(\d+)", safe)) else (safe, 0)
        return (*done, safe if held[index : index + 1] == (name,) or safe not in taken else next(f"{base}_{number:03d}" for number in count(number + 1) if f"{base}_{number:03d}" not in taken))

    return reduce(stored, asked, ())


def planned(nodes: Iterable[bpy.types.Node], record: Replace) -> BlenderResult[tuple[Rename, ...]]:
    """Plans all native string edits before writes, accumulating replacement-template refusals."""

    def substituted(pattern: re.Pattern[str], text: str) -> BlenderResult[str]:
        insert: Callable[[re.Match[str]], str] = (lambda found: found.expand(record.replace)) if record.regex else (lambda _: record.replace)
        return catch(lambda: pattern.sub(insert, text), exception=re.PatternError)().map_error(lambda error: (BlenderUnmetFault(requirement=Malformed(text=record.replace, error=error)),))

    def renames(pattern: re.Pattern[str], node: bpy.types.Node) -> BlenderResult[tuple[Rename, ...]]:
        texts = tuple(prop for prop in node.bl_rna.properties if isinstance(prop, bpy.types.StringProperty) and (prop.is_path_supports_templates or prop.identifier == "label"))
        items = tuple(
            (item, prop)
            for item in (node.file_output_items if isinstance(node, bpy.types.CompositorNodeOutputFile) else ())
            for prop in Option.of_optional(item.bl_rna.name_property).to_list()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        )
        before, held = tuple(getattr(node, prop.identifier) for prop in texts), tuple(getattr(item, prop.identifier) for item, prop in items)
        fields = accumulated(substituted(pattern, text) for text in before).map(lambda asked: tuple(Rename.field(node, prop, text, request) for prop, text, request in zip(texts, before, asked)))
        named = accumulated(substituted(pattern, name) for name in held).map(
            lambda asked: tuple(
                Rename(node=node, owner=item, prop=prop, before=text, requested=request, stored=stored) for (item, prop), text, request, stored in zip(items, held, asked, settled(asked, held))
            )
        )
        return paired(fields, named).map(lambda pair: tuple(rename for group in pair for rename in group if rename.requested != rename.before))

    return (
        Result[re.Pattern[str], BlenderFaults]
        .bind(
            catch(lambda: re.compile(record.find if record.regex else re.escape(record.find), 0 if record.case else re.IGNORECASE), exception=re.PatternError)().map_error(
                lambda error: (BlenderUnmetFault(requirement=Malformed(text=record.find, error=error)),)
            ),
            lambda pattern: accumulated(renames(pattern, node) for node in nodes if isinstance(node, bpy.types.CompositorNodeOutputFile | bpy.types.NodeFrame)),
        )
        .map(lambda groups: tuple(chain.from_iterable(groups)))
    )


def previewed(context: bpy.types.Context, record: Replace) -> str:
    """Returns every planned native spelling or the regex refusal text for confirmation."""
    return (
        planned(context.selected_nodes, record)
        .map(lambda renames: "\n".join(f"{rename.node.name}: {rename.before} → {rename.stored}" for rename in renames))
        .default_with(lambda faults: "\n".join(text for _, text in rendered(faults)))
    )


def applied(renames: tuple[Rename, ...], unmatched: str) -> BlenderResult[Outcome]:
    """Writes the renames whose stored spelling differs, freeing renamed items first so swaps hold, and answers the refused writes beside the written ones, every refusal where none wrote, or the reason none differs."""
    if not (edits := tuple(rename for rename in renames if rename.stored != rename.before)):
        return Ok(Unchanged(reason=unmatched))
    taken = {
        *(rename.before for rename in edits),
        *(rename.requested for rename in edits),
        *(rename.stored for rename in edits),
        *(item.name for rename in edits if isinstance(rename.node, bpy.types.CompositorNodeOutputFile) for item in rename.node.file_output_items),
    }
    forced = tuple(rename for rename in edits if rename.stored != rename.requested)
    return accumulated(
        written(rename.owner, rename.prop.identifier, next(f"{index}{'_' * number}" for number in count() if f"{index}{'_' * number}" not in taken))
        for index, rename in enumerate(edits)
        if rename.owner is not rename.node
    ).bind(
        lambda _: tallied(
            tuple(written(rename.owner, rename.prop.identifier, rename.stored) for rename in edits),
            (
                ("INFO", f"Replaced {len(edits)} fields across {len({rename.node.name for rename in edits})} nodes"),
                *((("WARNING", f"Blender stores {forced[0].requested} as {forced[0].stored}; {len(forced)} names changed spelling"),) if forced else ()),
            ),
        )
    )


def bumped(call: Call[Bump]) -> BlenderResult[Outcome]:
    """Plans every version edit and uses the same swap-safe native rename operation."""
    match call.context.active_node:
        case bpy.types.CompositorNodeOutputFile() as node:
            fields = tuple(
                Rename.field(node, prop, before, text)
                for prop in node.bl_rna.properties
                if isinstance(prop, bpy.types.StringProperty) and prop.is_path_supports_templates
                for before in (getattr(node, prop.identifier),)
                for text in stepped(before, call.record.offset).to_list()
            )
            named = tuple((item, prop) for item in node.file_output_items for prop in Option.of_optional(item.bl_rna.name_property).to_list())  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            held = tuple(getattr(item, prop.identifier) for item, prop in named)
            moved = tuple(stepped(name, call.record.offset).default_value(name) for name in held)
            items = tuple(
                Rename(node=node, owner=item, prop=prop, before=before, requested=request, stored=stored)
                for (item, prop), before, request, stored in zip(named, held, moved, settled(moved, held))
                if request != before
            )
            return applied((*fields, *items), "No version token in the node's paths")
        case _:
            return Error((BlenderAbsentFault(owner="bpy.context", key="active_node"),))


def opened(call: Call[Empty]) -> BlenderResult[Done]:
    """Opens the deepest existing output folder through the native operator."""
    if (scene := (call.context.scene)) is None:
        return Error((BlenderAbsentFault(owner=repr(call.context), key="scene"),))
    render = scene.render
    return (
        Result[Path, BlenderFaults]
        .bind(
            catch(lambda: Path(bpy.path.abspath(render.frame_path(frame=scene.frame_current))), exception=RuntimeError)().map_error(
                lambda error: (BlenderEditFault(request=repr(render), error=error),)
            ),
            lambda file: called(bpy.ops.wm.path_open, {"filepath": str(existing(file))}),
        )
        .map(lambda _: Done())
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


class ScenePaths(msgspec.Struct, frozen=True, kw_only=True):
    """Scene grammar attachment and native replace, version, take, and folder actions."""

    replace: ClassVar[Key] = "output_replace"
    version: ClassVar[Key] = "output_version"
    take: ClassVar[Key] = "output_take"
    folder: ClassVar[Key] = "output_folder"
    edited: ClassVar[tuple[Setting, Setting]] = (
        Setting(descriptor=Descriptor(root=bpy.types.SpaceNodeEditor, owner="", prop=bpy.types.SpaceNodeEditor.bl_rna.properties["tree_type"]), accepted=(bpy.types.CompositorNodeTree.__name__,)),
        Setting(descriptor=Descriptor(root=bpy.types.SpaceNodeEditor, owner="edit_tree", prop=bpy.types.NodeTree.bl_rna.properties["is_editable"])),
    )

    @staticmethod
    @contextmanager
    def changed(identity: Identity) -> Generator[Changed]:
        """Retains main-thread update admission for the native attachment lifetime."""
        with identity.reports.deferred(lambda handle: handle.resolve().bind(lambda scene: ScenePaths().bound(identity).bind(lambda pathed: pathed.recomposed(scene))).map(lambda _: Nothing)) as write:
            yield lambda group, _context, _property: write(Handle.of(group.id_data))

    attachment: ClassVar[Attachment[Outputs]] = Attachment(key="outputs", form=Outputs, owner=bpy.types.Scene, changed=Some(changed))
    replacing: ClassVar[Operator[Replace]] = Operator(
        key=replace,
        title="Replace in Outputs",
        description="Replace selected output paths, labels, and item names",
        condition=Some(AllOf(conditions=edited)),
        icon=Some("FILE_REFRESH"),
        form=Replace,
        run=lambda call: planned(call.context.selected_nodes, call.record).bind(lambda renames: applied(renames, "No selected path holds the find text")),
        invoke=Some(Suppressible(message=previewed, width=400)),
    )

    @property
    def page_rows(self) -> tuple[Attachment[Outputs], Operator[Replace]]:
        """Original attachment and suppressible replace declaration consumed by preferences."""
        return self.attachment, self.replacing

    def bound(self, identity: Identity) -> BlenderResult[Pathed]:
        """Binds the registered scene settings or returns native attachment faults."""
        return paired(self.attachment.bound(identity), self.scope(identity)).map(lambda pair: Pathed(attached=pair[0], scope=pair[1]))

    @staticmethod
    def scope(identity: Identity) -> BlenderResult[Stored[OutputPath]]:
        """Names persistent render paths shared by preparation, workers, and finishing."""
        return identity.name(Name.ATTACHMENT, "output_path").map(
            lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(OutputPath, dec_hook=lambda kind, value: kind(value)), encoder=msgspec.msgpack.Encoder(enc_hook=Path.as_posix))
        )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns nested declarations whose bodies bind the registered attachment when called."""

        def refreshed() -> BlenderResult[None]:
            return self.bound(identity).bind(lambda pathed: pathed.refreshed(identity))

        active = Setting(
            descriptor=Descriptor(root=bpy.types.SpaceNodeEditor, owner="edit_tree.nodes", prop=bpy.types.Nodes.bl_rna.properties["active"]), accepted=(bpy.types.CompositorNodeOutputFile.__name__,)
        )
        return Ok((
            *self.page_rows,
            Operator(
                key=self.version,
                title="Step Version",
                description="Moves version tokens in the active File Output node",
                condition=Some(AllOf(conditions=(*self.edited, active))),
                form=Bump,
                run=bumped,
                undo=self.version,
            ),
            Operator(
                key=self.take,
                title="New Take",
                description="Sets the scene take after all matching written files",
                icon=Some("ADD"),
                form=Empty,
                run=lambda call: self.bound(call.identity).bind(lambda pathed: pathed.taken(call)),
            ),
            Operator(key=self.folder, title="Open Output Folder", description="Opens the existing scene output folder", icon=Some("FILE_FOLDER"), form=Empty, run=opened, undo=False),
            PathHandler(handlers=bpy.app.handlers.load_post, body=lambda _: refreshed()),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            PathHandler(handlers=bpy.app.handlers.save_pre, body=lambda _: refreshed()),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            *(
                Subscription(owner=owner, setting=owner.bl_rna.properties[key], body=refreshed)
                for owner, key in ((bpy.types.ViewLayer, "name"), (bpy.types.Scene, "view_layers"), (bpy.types.PreferencesFilePaths, "render_output_directory"))
            ),
        ))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Bump", "Malformed", "NodePaths", "OutputPath", "Outputs", "Pathed", "Rename", "Replace", "ScenePaths", "Undated", "applied", "bumped", "opened", "planned", "previewed", "settled"]
