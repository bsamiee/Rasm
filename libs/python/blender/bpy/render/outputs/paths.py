"""Scene output grammar, its native template spelling, takes, and the output folder."""

from collections.abc import Generator, Mapping
from contextlib import contextmanager
import datetime
from functools import partial
from pathlib import Path
import posixpath
from typing import ClassVar, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.commands.invocations import Suppressible
from blender.bpy.commands.operators import Call, Done, Empty, Operator
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import AllOf, Condition, Setting
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.mirrors import Changed
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.events import Handler, located, runtime, Subscription
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.files.sessions import BlendFile
from blender.bpy.render.outputs.renames import Bump, output_replace, output_version, previewed, Replace
from blender.bpy.settings.attachments import Attached, Attachment, Stored
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired
from blender.records.stores import existing, removed
from blender.rendering.outputs.naming import Field, Fields, Number, NumberField, Output, OutputName, Slot, Word

# --- [MODELS] ---------------------------------------------------------------------------


class Outputs(bpy.types.PropertyGroup):
    """Scene output template, source-file grammar, take, and project-variable root."""

    template: bpy.props.StringProperty(name="Template", description="Written file path under output folder", default="{file}/v{version:3}/{layer}/{pass}/{frame:4}")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    stem: bpy.props.StringProperty(name="File Name Grammar", description="Project, shot, and take grammar of blend file name", default="{project}_{shot}")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    version: bpy.props.IntProperty(name="Version", description="Take when file name holds none", min=1, default=1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_variable: bpy.props.BoolProperty(name="Project Variable", description="Root outputs under a project path variable, preferences output folder when off")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    variable: bpy.props.StringProperty(name="Variable", description="Project path variable", default="renders")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class NodePaths(msgspec.Struct, frozen=True, kw_only=True):
    """Path member values and requested File Output item names."""

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
        return msgspec.structs.replace(self, root=escaped(self.base.as_posix()), spelled=self.spelled - {Word.FILE})

    def rerooted(self, base: Path) -> Self:
        """Returns path under worker's mapped output directory."""
        return msgspec.structs.replace(self, base=base, root=escaped(base.as_posix()))

    def spelling(self, part: Field) -> Option[str]:
        """Returns native variables for selected words and frame padding."""
        match part:
            case Slot(key=key):
                return Option.of_optional({Word.SCENE: "{scene_name}", Word.CAMERA: "{camera_name}", Word.FILE: "{blend_name}"}.get(key)).filter(lambda _: key in self.spelled)
            case Number(key=NumberField.FRAME, width=width):
                return Some("#" * width)
            case Number():
                return Nothing

    @classmethod
    def of(cls, outputs: Outputs, scene: bpy.types.Scene, root: tuple[str, Path], blend: Option[Path], day: datetime.date, unsaved: Condition) -> Outcome[Self]:
        """Combines template and stem grammars with source-file fields."""
        native, base = root

        def built(name: OutputName, stem: OutputName) -> Outcome[Self]:
            words = frozenset(part.key for part in name.slots if isinstance(part, Slot)) & frozenset(part.key for part in stem.slots if isinstance(part, Slot))
            counted = all(any(isinstance(part, Number) and part.key is NumberField.VERSION for part in grammar.slots) for grammar in (name, stem))
            parsed = (
                Result[Path, Faults].bind(blend.to_result_with(lambda: Fault.of(BlenderReason.REQUIRED, unsaved.message)), lambda path: stem.parse(path.stem)) if words or counted else Ok(Fields())
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
                    fields=Fields(
                        words=host | {key: value for key, value in found.words.items() if key in words}, counts={NumberField.VERSION: found.counts[NumberField.VERSION] if counted else outputs.version}
                    ),
                    orders={layer.name: position for position, layer in enumerate(scene.view_layers, 1)},
                )
            )

        return Result[tuple[OutputName, OutputName], Faults].bind(paired(OutputName.of(outputs.template), OutputName.of(outputs.stem)), lambda pair: built(*pair))

    def _filled(self, layer: str, output: Output, item: Option[str], frame: Option[int] = Nothing) -> Fields:
        return Fields(
            words={**self.fields.words, Word.LAYER: layer, Word.OUTPUT: output, Word.PASS: item.default_value(output)},
            counts={
                **self.fields.counts,
                **dict(Option.of_optional(self.orders.get(layer)).map(lambda order: (NumberField.ORDER, order)).to_list()),
                **dict(frame.map(lambda held: (NumberField.FRAME, held)).to_list()),
            },
        )

    def text(self, layer: str, output: Output, item: Option[str] = Nothing) -> Outcome[str]:
        """Composes a relative native template with every field value read literally, refusing a {date} the host cannot expand."""
        filled = self._filled(layer, output, item)
        return (
            Error(Fault.of(BlenderReason.UNDATED, layer))
            if Slot(key=Word.DATE) in self.name.slots
            else self.name.compose(msgspec.structs.replace(filled, words={key: escaped(value) for key, value in filled.words.items()}), self.spelling)
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
                catch(scene.render.frame_path, exception=RuntimeError)(frame=frame).map(Path).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(scene.render))),
                lambda file: removed(file).map(lambda _: None) if bpy.path.is_subdir(str(file), str(self.base)) else Ok(None),
            )

        return accumulated(map(dropped, frames)).map(lambda _: None)


class Pathed(msgspec.Struct, frozen=True, kw_only=True):
    """Bound scene output settings and shared native descriptors."""

    filepath: ClassVar[Descriptor[bpy.types.StringProperty]] = Descriptor.declared(bpy.types.RenderSettings, ("filepath",), bpy.types.StringProperty)
    directory: ClassVar[Descriptor[bpy.types.StringProperty]] = Descriptor.declared(bpy.types.CompositorNodeOutputFile, ("directory",), bpy.types.StringProperty)
    file_name: ClassVar[Descriptor[bpy.types.StringProperty]] = Descriptor.declared(bpy.types.CompositorNodeOutputFile, ("file_name",), bpy.types.StringProperty)
    unsaved: ClassVar[Condition] = ~Setting(descriptor=Descriptor.declared(bpy.types.Context, ("blend_data", "filepath"), bpy.types.StringProperty), accepted=("",))
    attached: Attached[Outputs]
    scope: Stored[OutputPath]

    def path(self, scene: bpy.types.Scene, day: datetime.date, saving: Option[Path] = Nothing) -> Outcome[OutputPath]:
        """Returns stored render path, else composes scene settings at day for the file being saved or the session file."""
        blend = saving.or_else_with(lambda: BlendFile.read().path)

        def resolved(native: str, folder: str) -> Outcome[tuple[str, Path]]:
            base = Path(bpy.path.abspath(folder, start=blend.map(lambda file: str(file.parent)).to_optional()))
            return Ok((native, base)) if base.is_absolute() else Error(Fault.of(BlenderReason.UNROOTED, folder))

        def rooted(outputs: Outputs) -> Outcome[tuple[str, Path]]:
            preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            folder = preferences.filepaths.render_output_directory
            return (
                Option
                .of_optional(bpy.data.project)
                .bind(lambda project: Option.of_optional(project.variables.get(outputs.variable)))
                .map(lambda variable: resolved(f"{{{variable.name}}}", variable.value))
                .default_with(lambda: Error(Fault.of(BlenderReason.ABSENT, "bpy.data.project.variables", member=outputs.variable)))
                if outputs.use_variable
                else resolved(escaped(folder), folder)
            )

        outputs = self.attached.struct(scene)
        return self.scope.read(scene).bind(lambda held: held.map(Ok).default_with(lambda: rooted(outputs).bind(lambda root: OutputPath.of(outputs, scene, root, blend, day, self.unsaved))))

    def node(self, path: OutputPath, layer: str, output: Output, passes: tuple[str, ...], *, layered: bool) -> Outcome[NodePaths]:
        """Builds node members and requested item names for one multilayer file or a file per item."""
        match layered:
            case True:
                return path.text(layer, output).map(
                    lambda text: NodePaths(
                        members={self.directory: path.root, self.file_name: text}, items=passes if Slot(key=Word.LAYER) in path.name.slots else tuple(f"{layer}.{name}" for name in passes)
                    )
                )
            case False:
                return accumulated(path.text(layer, output, Some(name)) for name in passes).map(lambda texts: NodePaths(members={self.directory: path.root, self.file_name: ""}, items=texts))

    def recomposed(self, scene: bpy.types.Scene, day: datetime.date, saving: Option[Path] = Nothing) -> Outcome[None]:
        """Writes scene output at `day` when native value differs."""
        return self.path(scene, day, saving).bind(OutputPath.scene_output).bind(lambda text: self.filepath.write(scene.render, text)).map(lambda _: None)

    def placed(self, scene: bpy.types.Scene, path: OutputPath) -> Outcome[OutputPath]:
        """Stores path as the scene's render path and writes its scene output, a job copy or a rerooted farm path rendering through it."""
        return self.scope.write(scene, path).bind(lambda _: path.scene_output()).bind(lambda text: self.filepath.write(scene.render, text)).map(lambda _: path)

    def refreshed(self, identity: Identity, day: datetime.date, saving: Option[Path] = Nothing) -> Outcome[None]:
        """Recomposes stored scenes at `day` except owned runtime scenes."""
        return accumulated(self.recomposed(scene, day, saving) for scene in bpy.data.scenes if self.attached.stored(scene) and not runtime(identity, scene)).map(lambda _: None)

    def took(self, scene: bpy.types.Scene, day: datetime.date) -> Outcome[int]:
        """Writes next take from one settings snapshot."""

        def wrote(path: OutputPath) -> Outcome[int]:
            version = path.next_version()
            return assigned(self.attached.struct(scene), {("version",): version}).map(lambda _: version)

        return self.path(scene, day).bind(wrote)


# --- [OPERATIONS] -----------------------------------------------------------------------


def today() -> datetime.date:
    """Returns the local calendar date an entry point composes output paths at, read once per entry."""
    return datetime.datetime.now(datetime.UTC).astimezone().date()


def escaped(text: str) -> str:
    """Returns text the host's path templates read literally, each brace doubled, a spelling no host member states."""
    return text.replace("{", "{{").replace("}", "}}")


def output_take(call: Call[Empty]) -> Outcome[Done]:
    """Set scene take past matching written files."""
    return paired(ScenePaths().bound(call.identity), rooted(bpy.types.Scene, call.context)).bind(
        lambda pair: pair[0].took(pair[1], today()).map(lambda version: Done(notes=(("INFO", f"Version {version}"),)))
    )


def output_folder(call: Call[Empty]) -> Outcome[Done]:
    """Open deepest existing output folder."""

    def opened(scene: bpy.types.Scene) -> Outcome[Done]:
        return (
            Result[Path, Faults]
            .bind(
                catch(scene.render.frame_path, exception=RuntimeError)(frame=scene.frame_current).map(Path).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(scene.render))),
                lambda file: called(bpy.ops.wm.path_open, {"filepath": str(existing(file))}),
            )
            .map(lambda _: Done())
        )

    return rooted(bpy.types.Scene, call.context).bind(opened)


# --- [COMPOSITION] ----------------------------------------------------------------------


class ScenePaths(msgspec.Struct, frozen=True, kw_only=True):
    """Scene grammar attachment with replace, version, take, and folder actions."""

    edited: ClassVar[AllOf] = Setting(descriptor=Descriptor.declared(bpy.types.SpaceNodeEditor, ("tree_type",), bpy.types.EnumProperty), accepted=(bpy.types.CompositorNodeTree.__name__,)) & Setting(
        descriptor=Descriptor.declared(bpy.types.SpaceNodeEditor, ("edit_tree", "is_editable"), bpy.types.BoolProperty)
    )

    @staticmethod
    @contextmanager
    def changed(identity: Identity) -> Generator[Changed]:
        """Defers update writes to main thread for attachment lifetime."""
        with identity.reports.deferred(
            lambda handle: handle.resolve().bind(lambda scene: ScenePaths().bound(identity).bind(lambda pathed: pathed.recomposed(scene, today()))).map(lambda _: Nothing)
        ) as write:
            yield lambda group, _context: write(Handle.of(group.id_data))

    attachment: ClassVar[Attachment[Outputs]] = Attachment(key="outputs", form=Outputs, owner=bpy.types.Scene, changed=Some(changed))
    replacing: ClassVar[Operator[Replace]] = Operator(
        condition=Some(edited),
        icon=Some("FILE_REFRESH"),
        form=Replace,
        run=output_replace,
        invoke=Some(Suppressible(message=previewed, dialog=partial(bpy.types.WindowManager.invoke_props_dialog, width=400))),
    )
    versioning: ClassVar[Operator[Bump]] = Operator(
        condition=Some(
            edited
            & Setting(descriptor=Descriptor.declared(bpy.types.SpaceNodeEditor, ("edit_tree", "nodes", "active"), bpy.types.PointerProperty), accepted=(bpy.types.CompositorNodeOutputFile.__name__,))
        ),
        form=Bump,
        run=output_version,
        undo=Some(Some(output_version.__name__)),
    )
    taking: ClassVar[Operator[Empty]] = Operator(icon=Some("ADD"), form=Empty, run=output_take)
    opening: ClassVar[Operator[Empty]] = Operator(icon=Some("FILE_FOLDER"), form=Empty, run=output_folder, undo=Nothing)
    prerequisites: ClassVar[tuple[Registrable, ...]] = (attachment,)

    def bound(self, identity: Identity) -> Outcome[Pathed]:
        """Binds registered scene settings."""
        return paired(self.attachment.bound(identity), self.scope(identity)).map(lambda pair: Pathed(attached=pair[0], scope=pair[1]))

    @staticmethod
    def scope(identity: Identity) -> Outcome[Stored[OutputPath]]:
        """Names persistent render paths shared by preparation, workers, and finishing."""
        return Stored.keyed(
            identity, "output_path", msgspec.msgpack.Decoder(OutputPath, dec_hook=lambda kind, value: kind(value)), enc_hook=Some(lambda value: value.as_posix() if isinstance(value, Path) else value)
        )

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns commands and refresh handlers binding attachment on call."""

        def refreshed(saving: Option[Path]) -> Outcome[None]:
            return self.bound(identity).bind(lambda pathed: pathed.refreshed(identity, today(), saving))

        current = partial(refreshed, Nothing)
        return Ok((
            self.replacing,
            self.versioning,
            self.taking,
            self.opening,
            Handler(handlers=bpy.app.handlers.load_post, body=current),
            Handler(handlers=bpy.app.handlers.save_pre, body=partial(located, refreshed)),
            Handler(handlers=bpy.app.handlers.save_post, body=current),
            *(
                Subscription(owner=owner, setting=owner.bl_rna.properties[key], body=current)
                for owner, key in ((bpy.types.ViewLayer, "name"), (bpy.types.Scene, "view_layers"), (bpy.types.PreferencesFilePaths, "render_output_directory"))
            ),
        ))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["NodePaths", "OutputPath", "Outputs", "Pathed", "ScenePaths", "escaped", "output_folder", "output_take", "today"]
