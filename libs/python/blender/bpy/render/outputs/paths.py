"""Scene output grammar, native stored names, versioning, and planned text edits."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager
import datetime
from functools import partial, reduce
from itertools import chain, count
from pathlib import Path
import posixpath
import re
from typing import ClassVar, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, Suppressible, tallied, Unchanged
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import AllOf, Not, Setting
from blender.bpy.data.rna.descriptors import assigned, Descriptor, rooted, Value, written
from blender.bpy.extension.events.handlers import LOAD_POST, PathHandler, SAVE_POST, SAVE_PRE
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity, Name
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.files.blends import BlendFile
from blender.bpy.render.outputs.formats import Kind
from blender.bpy.settings.attachments import Attached, Attachment, Changed, Stored
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.stores import existing
from blender.rendering.outputs.naming import Count, Field, Fields, Number, Output, OutputName, RESERVED, Slot, stepped, Word

# --- [MODELS] ---------------------------------------------------------------------------


class Outputs(bpy.types.PropertyGroup):
    """Scene output template, source-file grammar, take, and project-variable root."""

    template: bpy.props.StringProperty(name="Template", description="Written file path under output folder", default="{file}/v{version:3}/{layer}/{pass}/{frame:4}")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    stem: bpy.props.StringProperty(name="File Name Grammar", description="Project, shot, and take grammar of blend file name", default="{project}_{shot}")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    version: bpy.props.IntProperty(name="Version", description="Take when file name holds none", min=1, default=1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_variable: bpy.props.BoolProperty(name="Project Variable", description="Root outputs under a project path variable, preferences output folder when off")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    variable: bpy.props.StringProperty(name="Variable", description="Project path variable", default="renders")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Replace(bpy.types.PropertyGroup):
    """Pattern, replacement text, regex mode, and case sensitivity."""

    find: bpy.props.StringProperty(name="Find", description="Text to find in selected paths, labels, and item names")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    replace: bpy.props.StringProperty(name="Replace", description=r"Replacement, regex groups as \1 or \g<name>")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    regex: bpy.props.BoolProperty(name="Regular Expression", description="Read Find as a Python regular expression")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    case: bpy.props.BoolProperty(name="Match Case", description="Match upper and lower case as written")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Bump(bpy.types.PropertyGroup):
    """Version-token offset."""

    offset: bpy.props.IntProperty(name="Offset", description="Versions each token moves by", default=1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Undated(msgspec.Struct, frozen=True, kw_only=True):
    """Render-time path containing a date Blender cannot expand."""

    @property
    def message(self) -> str:
        """Unsupported render-time field requirement."""
        return "Requires template without {date}, Blender cannot expand it"


class Unrooted(msgspec.Struct, frozen=True, kw_only=True):
    """Output folder resolving against the process working directory."""

    folder: str

    @property
    def message(self) -> str:
        """Relative output folder requirement."""
        return f"Requires saved file or absolute output folder, {self.folder} resolves against working directory"


class Malformed(msgspec.Struct, frozen=True, kw_only=True):
    """Unparsable regex pattern or replacement template."""

    text: str
    error: re.PatternError

    @property
    def message(self) -> str:
        """Refused text and parser's explanation."""
        return f"Regular expression {self.text} did not parse ({self.error})"


class Rename(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """String edit with original, requested, and predicted stored spelling."""

    node: bpy.types.Node
    owner: bpy.types.Node | bpy.types.NodeCompositorFileOutputItem
    prop: bpy.types.StringProperty
    before: str
    requested: str
    stored: str

    @classmethod
    def field(cls, node: bpy.types.Node, prop: bpy.types.StringProperty, before: str, requested: str) -> Self:
        """Returns node string edit, stored spelling cut to member byte capacity."""
        return cls(node=node, owner=node, prop=prop, before=before, requested=requested, stored=requested.encode()[: prop.length_max - 1].decode(errors="ignore") if prop.length_max else requested)


class NodePaths(msgspec.Struct, frozen=True, kw_only=True):
    """Path member values and predicted File Output item names."""

    members: Mapping[Descriptor, Value]
    items: tuple[str, ...]


class OutputPath(msgspec.Struct, frozen=True, kw_only=True):
    """Composed scene output request with literal and native-template spellings."""

    name: OutputName
    root: str
    base: Path
    fields: Fields
    orders: Mapping[str, int]
    spelled: frozenset[Word] = frozenset({Word.SCENE, Word.CAMERA, Word.FILE})

    @property
    def copied(self) -> Self:
        """Job-copy path spelling source folder and file name literally."""
        return msgspec.structs.replace(self, root=self.base.as_posix(), spelled=self.spelled - {Word.FILE})

    def rerooted(self, base: Path) -> Self:
        """Returns path under worker's mapped output directory."""
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
    def of(cls, outputs: Outputs, scene: bpy.types.Scene, root: tuple[str, Path], blend: Option[Path], day: datetime.date, unsaved: Not) -> Outcome[Self]:
        """Combines template and stem grammars with source-file fields."""
        native, base = root

        def built(name: OutputName, stem: OutputName) -> Outcome[Self]:
            words = frozenset(part.key for part in name.slots if isinstance(part, Slot)) & frozenset(part.key for part in stem.slots if isinstance(part, Slot))
            counted = all(any(isinstance(part, Number) and part.key is Count.VERSION for part in grammar.slots) for grammar in (name, stem))
            parsed = (
                Result[Path, Faults].bind(blend.to_result_with(lambda: (Fault(reason=BlenderReason.UNMET, subject=unsaved.message),)), lambda path: stem.parse(path.stem))
                if words or counted
                else Ok(Fields())
            )
            host = (
                {Word.SCENE: scene.name, Word.DATE: day.isoformat()}
                | dict(Option.of_optional(scene.camera).map(lambda camera: (Word.CAMERA, camera.name)).to_list())
                | {Word.FILE: blend.map(lambda path: path.stem).default_with(lambda: bpy.app.translations.pgettext_data("Unsaved"))}
            )
            return parsed.map(
                lambda found: cls(
                    name=name,
                    root=native,
                    base=base,
                    fields=Fields(words=host | {key: value for key, value in found.words.items() if key in words}, counts={Count.VERSION: found.counts[Count.VERSION] if counted else outputs.version}),
                    orders={layer.name: position for position, layer in enumerate(scene.view_layers, 1)},
                )
            )

        return Result[tuple[OutputName, OutputName], Faults].bind(paired(OutputName.of(outputs.template), OutputName.of(outputs.stem)), lambda pair: built(*pair))

    def _filled(self, layer: str, output: Output, item: Option[str], frame: Option[int] = Nothing) -> Fields:
        return Fields(
            words={**self.fields.words, Word.LAYER: layer, Word.OUTPUT: output, Word.PASS: item.default_value(output)},
            counts={
                **self.fields.counts,
                **dict(Option.of_optional(self.orders.get(layer)).map(lambda order: (Count.ORDER, order)).to_list()),
                **dict(frame.map(lambda held: (Count.FRAME, held)).to_list()),
            },
        )

    def text(self, layer: str, output: Output, item: Option[str] = Nothing, *, spelled: bool = True) -> Outcome[str]:
        """Composes a relative native template, refusing a spelled {date}."""
        return (
            Error((Fault(reason=BlenderReason.UNMET, subject=Undated().message),))
            if spelled and Slot(key=Word.DATE) in self.name.slots
            else self.name.compose(self._filled(layer, output, item), self.spelling if spelled else lambda _: Nothing)
        )

    def scene_output(self) -> Outcome[str]:
        """Composes the context view layer's preview output under native root."""
        return rooted(bpy.types.ViewLayer, bpy.context).bind(lambda layer: self.text(layer.name, Output.PREVIEW)).map(lambda text: posixpath.join(self.root, text))

    def written(self, layer: str, output: Output, item: Option[str], frame: int, suffix: str) -> Outcome[Path]:
        """Composes literal written-file path."""
        return self.name.path(self.base, self._filled(layer, output, item, Some(frame))).map(lambda path: path.with_name(f"{path.name}{suffix}"))

    def next_version(self) -> int:
        """Returns next take after matching files, ignoring date."""
        held = frozenset(part.key for part in self.name.slots if isinstance(part, Slot) and part.key is not Word.DATE)
        return self.name.next_version(self.base, Fields(words={key: value for key, value in self.fields.words.items() if key in held}))

    def removed(self, scene: bpy.types.Scene, frames: range) -> Outcome[None]:
        """Removes scene outputs under job root."""

        def dropped(frame: int) -> Outcome[None]:
            return Result[Path, Faults].bind(
                catch(lambda: Path(bpy.path.abspath(scene.render.frame_path(frame=frame))), exception=RuntimeError)().map_error(
                    lambda error: (Fault(reason=BlenderReason.REFUSED, subject=repr(scene.render), cause=error),)
                ),
                lambda file: (
                    catch(lambda: file.unlink(missing_ok=True), exception=OSError)().map_error(lambda error: (Fault(reason=RecordReason.UNWRITABLE, subject=str(file), cause=error),))
                    if bpy.path.is_subdir(str(file), str(self.base))
                    else Ok(None)
                ),
            )

        return accumulated(map(dropped, frames)).map(lambda _: None)


class Pathed(msgspec.Struct, frozen=True, kw_only=True):
    """Bound scene output settings and shared native descriptors."""

    filepath: ClassVar[Descriptor[bpy.types.StringProperty]] = Descriptor.declared(bpy.types.RenderSettings, "filepath", bpy.types.StringProperty)
    directory: ClassVar[Descriptor[bpy.types.StringProperty]] = Descriptor.declared(bpy.types.CompositorNodeOutputFile, "directory", bpy.types.StringProperty)
    file_name: ClassVar[Descriptor[bpy.types.StringProperty]] = Descriptor.declared(bpy.types.CompositorNodeOutputFile, "file_name", bpy.types.StringProperty)
    unsaved: ClassVar[Not] = Not(condition=Setting(descriptor=Descriptor.declared(bpy.types.Context, "blend_data.filepath", bpy.types.StringProperty), accepted=("",)))
    attached: Attached[Outputs]
    scope: Stored[OutputPath]

    def path(self, scene: bpy.types.Scene, day: datetime.date, saving: Option[Path] = Nothing) -> Outcome[OutputPath]:
        """Returns stored render path, else composes scene settings at day for the file being saved or the session file."""
        blend = saving.or_else_with(lambda: BlendFile.read().path)

        def resolved(native: str, folder: str) -> Outcome[tuple[str, Path]]:
            base = Path(bpy.path.abspath(folder, start=blend.map(lambda file: str(file.parent)).to_optional()))
            return Ok((native, base)) if base.is_absolute() else Error((Fault(reason=BlenderReason.UNMET, subject=Unrooted(folder=folder).message),))

        def rooted(outputs: Outputs) -> Outcome[tuple[str, Path]]:
            folder = bpy.context.preferences.filepaths.render_output_directory  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            return (
                Option
                .of_optional(bpy.data.project)
                .bind(lambda project: Option.of_optional(project.variables.get(outputs.variable)))
                .map(lambda variable: resolved(f"{{{variable.name}}}", variable.value))
                .default_with(lambda: Error((Fault(reason=BlenderReason.ABSENT, subject="bpy.data.project.variables", member=outputs.variable),)))
                if outputs.use_variable
                else resolved(folder, folder)
            )

        outputs = self.attached.struct(scene)
        return self.scope.read(scene).bind(lambda held: held.map(Ok).default_with(lambda: rooted(outputs).bind(lambda root: OutputPath.of(outputs, scene, root, blend, day, self.unsaved))))

    def node(self, path: OutputPath, layer: str, output: Output, passes: tuple[str, ...], kind: Kind) -> Outcome[NodePaths]:
        """Builds node members and predicted item names for a file layout."""
        match kind:
            case Kind.LAYERED:
                return path.text(layer, output).map(
                    lambda text: NodePaths(
                        members={self.directory: path.root, self.file_name: text}, items=predicted(passes if Slot(key=Word.LAYER) in path.name.slots else tuple(f"{layer}.{name}" for name in passes))
                    )
                )
            case Kind.EXR | Kind.IMAGE:
                return accumulated(path.text(layer, output, Some(name)) for name in passes).map(
                    lambda texts: NodePaths(members={self.directory: path.root, self.file_name: ""}, items=predicted(texts))
                )

    def recomposed(self, scene: bpy.types.Scene, saving: Option[Path] = Nothing) -> Outcome[None]:
        """Writes scene output when native value differs."""
        return (
            self
            .path(scene, datetime.datetime.now(datetime.UTC).astimezone().date(), saving)
            .bind(OutputPath.scene_output)
            .bind(lambda text: self.filepath.write(scene.render, text))
            .map(lambda _: None)
        )

    def refreshed(self, identity: Identity, saving: Option[Path] = Nothing) -> Outcome[None]:
        """Recomposes stored scenes except owned runtime scenes."""
        return accumulated(
            self.recomposed(scene, saving)
            for scene in bpy.data.scenes
            if self.attached.stored(scene) and not (scene.is_runtime_data and identity.manifest.id in scene)  # type: ignore[operator]
        ).map(lambda _: None)

    def took(self, scene: bpy.types.Scene, day: datetime.date) -> Outcome[int]:
        """Writes next take from one settings snapshot."""

        def wrote(path: OutputPath) -> Outcome[int]:
            version = path.next_version()
            return assigned(self.attached.struct(scene), {"version": version}).map(lambda _: version)

        return self.path(scene, day).bind(wrote)


# --- [OPERATIONS] -----------------------------------------------------------------------


def predicted(requested: Iterable[str], held: tuple[str, ...] = ()) -> tuple[str, ...]:
    """Predicts Blender's truncation, sanitization, and uniqueness in native item order."""
    asked = tuple(requested)

    def stored(done: tuple[str, ...], name: str) -> tuple[str, ...]:
        index, safe = len(done), re.sub(f"[{RESERVED}]", "_", name.encode()[:1023].decode(errors="ignore"))
        taken = {*done, *(kept for later, kept in enumerate(held) if later > index and asked[later] == kept)}
        base, number = (found[1], int(found[2])) if (found := re.fullmatch(r"(.*)_(\d+)", safe)) else (safe, 0)
        return (*done, safe if held[index : index + 1] == (name,) or safe not in taken else next(f"{base}_{number:03d}" for number in count(number + 1) if f"{base}_{number:03d}" not in taken))

    return reduce(stored, asked, ())


def edits(node: bpy.types.CompositorNodeOutputFile | bpy.types.NodeFrame, transform: Callable[[str], Outcome[Option[str]]]) -> Outcome[tuple[Rename, ...]]:
    """Plans node strings and File Output item names a transform changes, item names predicted as the host stores them."""
    texts = tuple(prop for prop in node.bl_rna.properties if isinstance(prop, bpy.types.StringProperty) and (prop.is_path_supports_templates or prop.identifier == "label"))
    items = tuple(
        (item, prop)
        for item in (node.file_output_items if isinstance(node, bpy.types.CompositorNodeOutputFile) else ())
        for prop in Option.of_optional(item.bl_rna.name_property).to_list()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    )
    before, held = tuple(getattr(node, prop.identifier) for prop in texts), tuple(getattr(item, prop.identifier) for item, prop in items)

    def named(asked: tuple[Option[str], ...]) -> tuple[Rename, ...]:
        requests = tuple(request.default_value(name) for request, name in zip(asked, held))
        return tuple(
            Rename(node=node, owner=item, prop=prop, before=text, requested=request, stored=stored)
            for (item, prop), text, request, stored in zip(items, held, requests, predicted(requests, held))
            if request != text
        )

    return paired(
        accumulated(map(transform, before)).map(lambda asked: tuple(Rename.field(node, prop, text, request) for prop, text, changed in zip(texts, before, asked) for request in changed.to_list())),
        accumulated(map(transform, held)).map(named),
    ).map(lambda pair: (*pair[0], *pair[1]))


def planned(nodes: Iterable[bpy.types.Node], properties: Replace) -> Outcome[tuple[Rename, ...]]:
    """Plans every string edit before writes."""

    def substituted(pattern: re.Pattern[str], text: str) -> Outcome[Option[str]]:
        insert: Callable[[re.Match[str]], str] = (lambda found: found.expand(properties.replace)) if properties.regex else (lambda _: properties.replace)
        return (
            catch(lambda: pattern.sub(insert, text), exception=re.PatternError)()
            .map_error(lambda error: (Fault(reason=BlenderReason.UNMET, subject=Malformed(text=properties.replace, error=error).message),))
            .map(lambda changed: Some(changed) if changed != text else Nothing)
        )

    return (
        Result[re.Pattern[str], Faults]
        .bind(
            catch(lambda: re.compile(properties.find if properties.regex else re.escape(properties.find), 0 if properties.case else re.IGNORECASE), exception=re.PatternError)().map_error(
                lambda error: (Fault(reason=BlenderReason.UNMET, subject=Malformed(text=properties.find, error=error).message),)
            ),
            lambda pattern: accumulated(edits(node, partial(substituted, pattern)) for node in nodes if isinstance(node, bpy.types.CompositorNodeOutputFile | bpy.types.NodeFrame)),
        )
        .map(lambda groups: tuple(chain.from_iterable(groups)))
    )


def previewed(context: bpy.types.Context, properties: Replace) -> str:
    """Returns planned spellings or regex fault text for confirmation."""
    return (
        planned(context.selected_nodes, properties)
        .map(lambda renames: "\n".join(f"{rename.node.name} · {rename.before} → {rename.stored}" for rename in renames))
        .default_with(lambda faults: "\n".join(fault.message for fault in faults))
    )


def applied(renames: tuple[Rename, ...], unmatched: str) -> Outcome[Answer]:
    """Writes each request after freeing renamed items for swaps, reporting what the host stored, Unchanged when no stored spelling differs."""
    if not (changed := tuple(rename for rename in renames if rename.stored != rename.before)):
        return Ok(Unchanged(reason=unmatched))
    taken = {
        *(rename.before for rename in changed),
        *(rename.requested for rename in changed),
        *(rename.stored for rename in changed),
        *(item.name for rename in changed if isinstance(rename.node, bpy.types.CompositorNodeOutputFile) for item in rename.node.file_output_items),
    }

    def reported(writes: tuple[Outcome[object], ...]) -> Outcome[Answer]:
        respelled = tuple((rename.requested, stored) for rename in changed for stored in (getattr(rename.owner, rename.prop.identifier),) if stored != rename.requested)
        return tallied(
            writes,
            (
                ("INFO", f"Replaced {len(changed)} fields across {len({rename.node.name for rename in changed})} nodes"),
                *((("WARNING", f"Blender stores {respelled[0][0]} as {respelled[0][1]}, {len(respelled)} names respelled"),) if respelled else ()),
            ),
        )

    return accumulated(
        written(rename.owner, rename.prop.identifier, next(f"{index}{'_' * number}" for number in count() if f"{index}{'_' * number}" not in taken))
        for index, rename in enumerate(changed)
        if rename.owner is not rename.node
    ).bind(lambda _: reported(tuple(written(rename.owner, rename.prop.identifier, rename.requested) for rename in changed)))


def output_replace(call: Call[Replace]) -> Outcome[Answer]:
    """Replace text in selected paths, labels, and item names."""
    return planned(call.context.selected_nodes, call.properties).bind(lambda renames: applied(renames, "No selected path holds find text"))


def output_version(call: Call[Bump]) -> Outcome[Answer]:
    """Move version tokens in active File Output node."""
    match call.context.active_node:
        case bpy.types.CompositorNodeOutputFile() as node:
            return edits(node, lambda text: Ok(stepped(text, call.properties.offset))).bind(lambda renames: applied(renames, "No version token in node paths"))
        case _:
            return Error((Fault(reason=BlenderReason.ABSENT, subject="bpy.context", member="active_node"),))


def output_take(call: Call[Empty]) -> Outcome[Done]:
    """Set scene take past matching written files."""
    return paired(ScenePaths().bound(call.identity), call.root(bpy.types.Scene)).bind(
        lambda pair: pair[0].took(pair[1], datetime.datetime.now(datetime.UTC).astimezone().date()).map(lambda version: Done(notes=(("INFO", f"Version {version}"),)))
    )


def output_folder(call: Call[Empty]) -> Outcome[Done]:
    """Open deepest existing output folder."""
    if (scene := (call.context.scene)) is None:
        return Error((Fault(reason=BlenderReason.ABSENT, subject=repr(call.context), member="scene"),))
    render = scene.render
    return (
        Result[Path, Faults]
        .bind(
            catch(lambda: Path(bpy.path.abspath(render.frame_path(frame=scene.frame_current))), exception=RuntimeError)().map_error(
                lambda error: (Fault(reason=BlenderReason.REFUSED, subject=repr(render), cause=error),)
            ),
            lambda file: called(bpy.ops.wm.path_open, {"filepath": str(existing(file))}),
        )
        .map(lambda _: Done())
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


class ScenePaths(msgspec.Struct, frozen=True, kw_only=True):
    """Scene grammar attachment with replace, version, take, and folder actions."""

    edited: ClassVar[tuple[Setting, Setting]] = (
        Setting(descriptor=Descriptor.declared(bpy.types.SpaceNodeEditor, "tree_type", bpy.types.EnumProperty), accepted=(bpy.types.CompositorNodeTree.__name__,)),
        Setting(descriptor=Descriptor.declared(bpy.types.SpaceNodeEditor, "edit_tree.is_editable", bpy.types.BoolProperty)),
    )

    @staticmethod
    @contextmanager
    def changed(identity: Identity) -> Generator[Changed]:
        """Defers update writes to main thread for attachment lifetime."""
        with identity.reports.deferred(lambda handle: handle.resolve().bind(lambda scene: ScenePaths().bound(identity).bind(lambda pathed: pathed.recomposed(scene))).map(lambda _: Nothing)) as write:
            yield lambda group, _context: write(Handle.of(group.id_data))

    attachment: ClassVar[Attachment[Outputs]] = Attachment(key="outputs", form=Outputs, owner=bpy.types.Scene, changed=Some(changed))
    replacing: ClassVar[Operator[Replace]] = Operator(
        condition=Some(AllOf(conditions=edited)), icon=Some("FILE_REFRESH"), form=Replace, run=output_replace, invoke=Some(Suppressible(message=previewed, width=400))
    )
    versioning: ClassVar[Operator[Bump]] = Operator(
        condition=Some(
            AllOf(
                conditions=(
                    *edited,
                    Setting(descriptor=Descriptor.declared(bpy.types.SpaceNodeEditor, "edit_tree.nodes.active", bpy.types.PointerProperty), accepted=(bpy.types.CompositorNodeOutputFile.__name__,)),
                )
            )
        ),
        form=Bump,
        run=output_version,
        undo=output_version.__name__,
    )
    taking: ClassVar[Operator[Empty]] = Operator(icon=Some("ADD"), form=Empty, run=output_take)
    opening: ClassVar[Operator[Empty]] = Operator(icon=Some("FILE_FOLDER"), form=Empty, run=output_folder, undo=False)
    prerequisites: ClassVar[tuple[Registrable, ...]] = (attachment,)

    def bound(self, identity: Identity) -> Outcome[Pathed]:
        """Binds registered scene settings."""
        return paired(self.attachment.bound(identity), self.scope(identity)).map(lambda pair: Pathed(attached=pair[0], scope=pair[1]))

    @staticmethod
    def scope(identity: Identity) -> Outcome[Stored[OutputPath]]:
        """Names persistent render paths shared by preparation, workers, and finishing."""
        return identity.name(Name.ATTACHMENT, "output_path").map(
            lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(OutputPath, dec_hook=lambda kind, value: kind(value)), encoder=msgspec.msgpack.Encoder(enc_hook=Path.as_posix))
        )

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns commands and refresh handlers binding attachment on call."""

        def refreshed(saving: Option[Path] = Nothing) -> Outcome[None]:
            return self.bound(identity).bind(lambda pathed: pathed.refreshed(identity, saving))

        return Ok((
            self.replacing,
            self.versioning,
            self.taking,
            self.opening,
            PathHandler(handlers=LOAD_POST, body=lambda _: refreshed()),
            PathHandler(handlers=SAVE_PRE, body=refreshed),
            PathHandler(handlers=SAVE_POST, body=lambda _: refreshed()),
            *(
                Subscription(owner=owner, setting=owner.bl_rna.properties[key], body=refreshed)
                for owner, key in ((bpy.types.ViewLayer, "name"), (bpy.types.Scene, "view_layers"), (bpy.types.PreferencesFilePaths, "render_output_directory"))
            ),
        ))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = [
    "Bump",
    "Malformed",
    "NodePaths",
    "OutputPath",
    "Outputs",
    "Pathed",
    "Rename",
    "Replace",
    "ScenePaths",
    "Undated",
    "Unrooted",
    "applied",
    "output_folder",
    "output_replace",
    "output_take",
    "output_version",
    "planned",
    "predicted",
    "previewed",
]
