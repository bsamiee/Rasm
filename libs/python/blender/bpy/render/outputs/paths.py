"""Scene output grammar, its native template spelling, takes, the output folder, and planned text edits of File Output and frame node strings."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager
import datetime
from functools import partial
from itertools import chain, count
import ntpath
import os
from pathlib import Path
import posixpath
import re
import string
from types import MappingProxyType
from typing import ClassVar, Final, Self

import bpy
from expression import Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.invocations import Suppressible
from blender.bpy.commands.operators import Answer, Call, called, Done, Empty, Operator, tallied, Unchanged
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Condition, Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.paths import definition, rooted
from blender.bpy.data.rna.properties import Changed
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.events import located, runtime
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Handler, Registrable, Subscription
from blender.bpy.files.sessions import BlendFile
from blender.bpy.render.outputs.formats import extension
from blender.bpy.scene.cameras import render_views
from blender.bpy.settings.attachments import Attached, Attachment, Stored
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Store
from blender.records.faults import accumulated, Error, Fault, Faults, Nothing, Ok, Outcome, paired, Reason
from blender.records.stores import existing, removed
from blender.rendering.outputs.naming import Field, Fields, NumberField, NumberKey, Output, OutputName, stepped, TextField, TextKey

# --- [CONSTANTS] ------------------------------------------------------------------------

_NATIVE: Final = MappingProxyType({TextKey.SCENE: "scene_name", TextKey.CAMERA: "camera_name", TextKey.FILE: "blend_name"})
_FILEPATH: Final[bpy.types.StringProperty] = bpy.types.RenderSettings.bl_rna.properties["filepath"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_EDITED: Final = Setting(
    descriptor=Descriptor(root=bpy.types.SpaceNodeEditor, prop=bpy.types.SpaceNodeEditor.bl_rna.properties["tree_type"]), accepted=(bpy.types.CompositorNodeTree.__name__,)
) & Setting(descriptor=Descriptor(root=bpy.types.SpaceNodeEditor, owner=("edit_tree",), prop=bpy.types.ID.bl_rna.properties["is_editable"]))
_FILE_OUTPUT: Final = Setting(
    descriptor=Descriptor(root=bpy.types.SpaceNodeEditor, owner=("edit_tree", "nodes"), prop=bpy.types.Nodes.bl_rna.properties["active"]), accepted=(bpy.types.CompositorNodeOutputFile.__name__,)
)

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


class Rename(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Native string owner with its original and requested text."""

    node: bpy.types.Node
    owner: bpy.types.Node | bpy.types.NodeCompositorFileOutputItem
    prop: bpy.types.StringProperty
    before: str
    requested: str


class NodePaths(msgspec.Struct, frozen=True, kw_only=True):
    """Path member values and requested File Output item names."""

    members: Mapping[Descriptor, str]
    items: tuple[str, ...]


class OutputPath(msgspec.Struct, frozen=True, kw_only=True):
    """Composed scene output request with literal and native-template spellings."""

    name: OutputName
    root: str
    base: Path
    fields: Fields
    orders: Mapping[str, int]
    spelled: frozenset[TextKey] = frozenset(_NATIVE)

    @property
    def copied(self) -> Self:
        """Job-copy path spelling source folder and file name literally."""
        return msgspec.structs.replace(self, root=escaped(self.base.as_posix()), spelled=self.spelled - {TextKey.FILE})

    def rerooted(self, base: Path) -> Self:
        """Returns path under worker's mapped output directory."""
        return msgspec.structs.replace(self, base=base, root=escaped(base.as_posix()))

    def spelling(self, part: Field) -> Option[str]:
        """Returns native variables for selected words and frame padding."""
        match part:
            case TextField(key=key):
                return Option.of_optional(_NATIVE.get(key)).filter(lambda _: key in self.spelled).map(lambda name: f"{{{name}}}")
            case NumberField():
                return _frame(part)

    @classmethod
    def of(cls, outputs: Outputs, scene: bpy.types.Scene, root: tuple[str, Path], blend: Option[Path], day: datetime.date) -> Outcome[Self]:
        """Combines template and stem grammars with source-file fields."""
        native, base = root

        def built(name: OutputName, stem: OutputName) -> Outcome[Self]:
            shared = name.keys & stem.keys
            words, counted = shared - frozenset(NumberKey), NumberKey.VERSION in shared
            parsed = (
                Result[Path, Faults].bind(blend.to_result_with(lambda: Fault.of(Reason.REQUIRED, Pathed.unsaved.message)), lambda path: stem.parse(path.stem)) if words or counted else Ok(Fields())
            )
            host = (
                {TextKey.SCENE: scene.name, TextKey.DATE: day.isoformat()}
                | dict(Option.of_optional(scene.camera).map(lambda camera: (TextKey.CAMERA, camera.name)).to_list())
                | {TextKey.FILE: blend.map(lambda path: path.stem).default_with(lambda: bpy.app.translations.pgettext_data("Unsaved"))}
            )
            return parsed.map(
                lambda found: cls(
                    name=name,
                    root=native,
                    base=base,
                    fields=Fields(
                        texts=host | {key: value for key, value in found.texts.items() if key in words}, numbers={NumberKey.VERSION: found.numbers[NumberKey.VERSION] if counted else outputs.version}
                    ),
                    orders={layer.name: position for position, layer in enumerate(scene.view_layers, 1)},
                )
            )

        return Result[tuple[OutputName, OutputName], Faults].bind(paired(OutputName.of(outputs.template), OutputName.of(outputs.stem)), lambda pair: built(*pair))

    def _filled(self, layer: str, output: Output, item: Option[str]) -> Fields:
        return Fields(
            texts={**self.fields.texts, TextKey.LAYER: layer, TextKey.OUTPUT: output, TextKey.PASS: item.default_value(output)},
            numbers={**self.fields.numbers, **dict(Option.of_optional(self.orders.get(layer)).map(lambda order: (NumberKey.ORDER, order)).to_list())},
        )

    def text(self, layer: str, output: Output, item: Option[str] = Nothing) -> Outcome[str]:
        """Composes a relative native template with literal field values."""
        filled = self._filled(layer, output, item)
        return self.name.compose(msgspec.structs.replace(filled, texts={key: escaped(value) for key, value in filled.texts.items()}), self.spelling)

    def scene_output(self, scene: bpy.types.Scene, *, layer: Option[bpy.types.ViewLayer] = Nothing) -> Outcome[str]:
        """Composes the requested layer's preview path, resolving native target-scene context when none was requested."""
        with bpy.context.temp_override(scene=scene):
            return (
                layer.map(Ok).default_with(lambda: rooted(bpy.types.ViewLayer, bpy.context)).bind(lambda held: self.text(held.name, Output.PREVIEW)).map(lambda text: posixpath.join(self.root, text))
            )

    def literal(self, layer: str, output: Output, item: Option[str], frame: int, *, suffix: Option[str] = Nothing) -> Outcome[Path]:
        """Composes a frame's literal filename and suffix under its output root."""
        fields = self._filled(layer, output, item)
        return self.name.path(self.base, msgspec.structs.replace(fields, numbers={**fields.numbers, NumberKey.FRAME: frame}), suffix=suffix.default_value(""))

    def next_version(self) -> int:
        """Returns next take after matching files, ignoring date."""
        held = self.name.keys - {TextKey.DATE}
        return self.name.next_version(self.base, Fields(texts={key: value for key, value in self.fields.texts.items() if key in held}))

    def removed(self, scene: bpy.types.Scene, frames: range) -> Outcome[None]:
        """Removes scene outputs under job root."""

        def dropped(frame: int, view: Option[bpy.types.SceneRenderView]) -> Outcome[None]:
            return frame_file(scene.render, frame, view).bind(lambda file: removed(file).map(lambda _: None) if bpy.path.is_subdir(str(file), str(self.base)) else Ok(None))

        views = tuple(map(Some, render_views(scene.render))) if scene.render.use_multiview and scene.render.image_settings.views_format == "INDIVIDUAL" else (Nothing,)
        return accumulated(dropped(frame, view) for frame in frames for view in views).map(lambda _: None)


class Pathed(msgspec.Struct, frozen=True, kw_only=True):
    """Bound scene output settings and shared native descriptors."""

    filepath: ClassVar[Descriptor] = Descriptor(root=bpy.types.RenderSettings, prop=bpy.types.RenderSettings.bl_rna.properties["filepath"])
    directory: ClassVar[Descriptor] = Descriptor(root=bpy.types.CompositorNodeOutputFile, prop=bpy.types.CompositorNodeOutputFile.bl_rna.properties["directory"])
    file_name: ClassVar[Descriptor] = Descriptor(root=bpy.types.CompositorNodeOutputFile, prop=bpy.types.CompositorNodeOutputFile.bl_rna.properties["file_name"])
    unsaved: ClassVar[Condition] = ~Setting(descriptor=Descriptor(root=bpy.types.Context, owner=("blend_data",), prop=bpy.types.BlendData.bl_rna.properties["filepath"]), accepted=("",))
    attached: Attached[Outputs]
    stored: Stored[OutputPath]

    def path(self, scene: bpy.types.Scene, day: datetime.date, saving: Option[Path] = Nothing) -> Outcome[OutputPath]:
        """Returns stored render path, else composes scene settings at day for the file being saved or the session file."""
        blend = saving.or_else_with(lambda: BlendFile.read().path)

        def resolved(native: str, folder: str) -> Outcome[tuple[str, Path]]:
            base = Path(bpy.path.abspath(folder, start=blend.map(lambda file: str(file.parent)).to_optional()))
            return Ok((native, base)) if base.is_absolute() else Error(Fault.of(Reason.REQUIRED, f"saved file or absolute output folder, {folder} resolving against the working directory"))

        def root(outputs: Outputs) -> Outcome[tuple[str, Path]]:
            preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            folder = preferences.filepaths.render_output_directory
            return (
                Option
                .of_optional(bpy.data.project)
                .bind(lambda project: Option.of_optional(project.variables.get(outputs.variable)))
                .map(lambda variable: resolved(f"{{{variable.name}}}", variable.value))
                .default_with(lambda: Error(Fault.of(Reason.ABSENT, "bpy.data.project.variables", member=outputs.variable)))
                if outputs.use_variable
                else resolved(escaped(folder), folder)
            )

        outputs = self.attached.struct(scene)
        return self.stored.read(scene).bind(lambda held: held.map(Ok).default_with(lambda: root(outputs).bind(lambda found: OutputPath.of(outputs, scene, found, blend, day))))

    def node(self, path: OutputPath, layer: str, output: Output, passes: tuple[str, ...], *, layered: bool) -> Outcome[NodePaths]:
        """Builds node members and requested item names for one multilayer file or a file per item."""
        return (
            path.text(layer, output).map(
                lambda text: NodePaths(members={self.directory: path.root, self.file_name: text}, items=passes if TextKey.LAYER in path.name.keys else tuple(f"{layer}.{name}" for name in passes))
            )
            if layered
            else accumulated(path.text(layer, output, Some(name)) for name in passes).map(lambda texts: NodePaths(members={self.directory: path.root, self.file_name: ""}, items=texts))
        )

    def recomposed(self, scene: bpy.types.Scene, day: datetime.date, saving: Option[Path] = Nothing) -> Outcome[None]:
        """Writes scene output at `day` when native value differs."""
        return self.path(scene, day, saving).bind(lambda path: path.scene_output(scene)).bind(lambda text: self.filepath.write(scene.render, text)).map(lambda _: None)

    def placed(self, scene: bpy.types.Scene, path: OutputPath) -> Outcome[OutputPath]:
        """Stores path as the scene's render path and writes its scene output, a job copy or a rerooted farm path rendering through it."""
        return self.stored.write(scene, path).bind(lambda _: path.scene_output(scene)).bind(lambda text: self.filepath.write(scene.render, text)).map(lambda _: path)

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

# --- [PATHS]


def today() -> datetime.date:
    """Returns the local calendar date an entry point composes output paths at, read once per entry."""
    return datetime.datetime.now(datetime.UTC).astimezone().date()


def escaped(text: str) -> str:
    """Returns text the host's path templates read literally, each brace doubled, a spelling no host member states."""
    return text.replace("{", "{{").replace("}", "}}")


def _frame(part: Field) -> Option[str]:
    """Spells native frame placeholders at the declared field width."""
    match part:
        case NumberField(key=NumberKey.FRAME, width=width):
            return Some("#" * width)
        case _:
            return Nothing


def frame_file(render: bpy.types.RenderSettings, frame: int, view: Option[bpy.types.SceneRenderView] = Nothing) -> Outcome[Path]:
    """Returns the native animation file path, with templates, frame, and camera-view suffix resolved."""
    return catch(render.frame_path, exception=RuntimeError)(frame=frame, view=view.map(lambda held: held.name).default_value("")).map(Path).map_error(partial(Fault.of, Reason.REFUSED, repr(render)))


def file_output_path(
    scene: bpy.types.Scene,
    node: bpy.types.CompositorNodeOutputFile,
    settings: bpy.types.ImageFormatSettings,
    frame: int,
    *,
    animation: Option[bool],
    item: Option[bpy.types.NodeCompositorFileOutputItem] = Nothing,
    view: Option[bpy.types.SceneRenderView] = Nothing,
) -> Outcome[Path]:
    """Resolves an owned File Output's live strings, actual item name, format and render mode."""
    separators = "/\\" if os.name == "nt" else "/"
    base = _path_bytes(f"{node.directory}{'' if node.directory.endswith(tuple(separators)) else os.sep}{node.file_name}{item.map(lambda held: held.name).default_value('')}")

    def chosen(paths: tuple[Path, ...]) -> Outcome[Path]:
        match tuple(dict.fromkeys(paths)):
            case (path,):
                return Ok(path)
            case candidates:
                return Error(Fault.of(Reason.REQUIRED, repr(node), candidates, member="render mode for distinct still and animation paths"))

    return (
        extension(settings)
        .to_result_with(lambda: Fault.of(Reason.OUTSIDE, repr(settings), settings.file_format, member="still image extension"))
        .bind(
            lambda suffix: accumulated(
                _image_path(
                    scene,
                    base if animated else _path_frame(base, frame, 0),
                    suffix,
                    view,
                    use_extension=node.use_file_extension,
                    framed=partial(_path_frame, frame=frame, digits=4) if animated else lambda text: text,
                ).map(Path)
                for animated in animation.map(lambda mode: (mode,)).default_with(lambda: (False, True))
            )
        )
        .bind(chosen)
    )


def scene_still_path(scene: bpy.types.Scene, *, view: Option[bpy.types.SceneRenderView] = Nothing) -> Outcome[Path]:
    """Resolves a native scene still, adding its individual view after its extension."""
    return _image_path(scene, scene.render.filepath, scene.render.file_extension, Nothing, use_extension=scene.render.use_file_extension).map(lambda text: Path(_view_path(scene, text, view)))


def _path_bytes(text: str, reserve: int = 0) -> str:
    """Copies a native path buffer, retaining filesystem bytes at its RNA capacity."""
    return text.encode(errors="surrogateescape")[: _FILEPATH.length_max - 1 - reserve].decode(errors="surrogateescape")


def _path_frame(text: str, frame: int, digits: int) -> str:
    """Expands the last basename hash group, appending requested frame digits when absent."""
    name = ntpath.basename(text)
    if not (width := len(found[0]) if (found := re.search(r"#+(?=[^#]*$)", name)) else digits):
        return text
    number = f"{'-' if frame < 0 else ''}{abs(frame):0{width}d}"
    start = len(text) - len(name)
    return _path_bytes(f"{text[: start + found.start()]}{number}{text[start + found.end() :]}" if found else f"{text}{number}")


def _path_suffix(text: str, suffix: str) -> str:
    """Inserts a native suffix before an existing extension when its path buffer holds it."""
    stem, previous = ntpath.splitext(text)
    return f"{stem}{suffix}{previous}" if len(text.encode(errors="surrogateescape")) + len(suffix.encode()) < _FILEPATH.length_max else text


def _view_path(scene: bpy.types.Scene, text: str, view: Option[bpy.types.SceneRenderView]) -> str:
    """Adds the requested native view suffix while scene multiview is enabled."""
    return _path_suffix(text, view.map(lambda held: held.file_suffix).default_value("") if scene.render.use_multiview else "")


def _image_path(scene: bpy.types.Scene, base: str, extension: str, view: Option[bpy.types.SceneRenderView], *, use_extension: bool, framed: Callable[[str], str] = lambda text: text) -> Outcome[str]:
    """Resolves compiler-owned templates, a frame phase, suffixes and extensions.

    Blender exposes neither its still resolver nor its filename sanitizer to Python.
    Native image paths reserve ten input bytes before template expansion.

    Returns:
        Native path text, or faults for unknown variables or invalid template syntax.
    """
    capacity = _FILEPATH.length_max

    def filename(value: str) -> str:
        safe = re.sub(r'[\x01-\x1f/\\?*:|"<>]', "_", _path_bytes(value))
        safe = f"_{safe[1:]}" if safe and not safe.strip(".") else safe
        if os.name == "nt":
            reserved = re.match(r"(?:con|prn|aux|null|com[1-9]|lpt[1-9])(?:\.|$)", safe, re.IGNORECASE | re.ASCII)
            safe = f"{safe[:-1]}_" if safe.endswith(".") else safe
            safe = f"_{safe[1:]}" if reserved else safe
        return safe

    variables = {
        _NATIVE[TextKey.SCENE]: filename(scene.name),
        **dict(Option.of_optional(scene.camera).map(lambda camera: (_NATIVE[TextKey.CAMERA], filename(camera.name))).to_list()),
        _NATIVE[TextKey.FILE]: _path_bytes(ntpath.splitext(ntpath.basename(bpy.data.filepath))[0] if bpy.data.filepath else bpy.app.translations.pgettext_data("Unsaved")),
        **{
            variable.name: _path_bytes(variable.value) if variable.subtype == "FILEPATH" else filename(variable.value)
            for project in Option.of_optional(bpy.data.project).to_list()
            for variable in project.variables
            if isinstance(variable, bpy.types.ProjectVariableString)
        },
    }

    def resolved() -> str:
        parts = tuple(string.Formatter().parse(_path_bytes(base, 10)))
        if any(spec or conversion is not None for _, key, spec, conversion in parts if key is not None):
            raise ValueError(base)
        text = _path_bytes("".join(literal + (variables[key] if key is not None else "") for literal, key, _, _ in parts))
        if os.name != "nt" and re.match(r"[A-Za-z]:[/\\]", text):
            text = f"/{text[0].lower()}{text[2:]}"
        prefix = 4 if text.startswith("\\\\?\\") else 2 if text.startswith("\\\\") else 0
        text = text[:prefix] + text[prefix:].replace("\\", "/")
        text = _path_bytes(f"{bpy.path.abspath('//')}{text[2:]}" if text.startswith("//") else bpy.path.abspath(text))
        trailing = text.endswith(tuple("/\\" if os.name == "nt" else "/"))
        terminal_dot = text.endswith(f"{os.sep}.")
        terminal_parent = text.endswith(f"{os.sep}..")
        text = os.path.normpath(text[:-1] if terminal_dot else text) if text else text
        trailing = trailing or (terminal_parent and ntpath.basename(text) not in {".", ".."})
        text = f"{text}{'' if text.endswith(os.sep) else os.sep}." if terminal_dot else text
        text = f"{text}{os.sep}" if trailing and not text.endswith(os.sep) else text
        text = framed(text)
        text = _path_suffix(text, bpy.app.translations.pgettext_data("Untitled")) if text.endswith(tuple("/\\" if os.name == "nt" else "/")) else text
        previous = ntpath.splitext(text)[1]
        text = _view_path(scene, text, view)
        accepted = {".jpg", ".jpeg"} if extension == ".jpg" else {".tif", ".tiff"} if extension == ".tif" else {extension}
        if use_extension and previous.lower() not in accepted:
            stem, previous = ntpath.splitext(text)
            replaced = previous.lower() in bpy.path.extensions_image  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            target = stem if replaced else text.rstrip(".")
            text = f"{target}{extension}" if len(target.encode(errors="surrogateescape")) + len(extension.encode()) < capacity else text if replaced else target
        return text

    try:
        return Ok(resolved())
    except (KeyError, ValueError) as error:
        return Error(Fault.of(Reason.INVALID, base, error))


# --- [RENAMES]


def edits(node: bpy.types.CompositorNodeOutputFile | bpy.types.NodeFrame, transform: Callable[[str], Outcome[Option[str]]]) -> Outcome[tuple[Rename, ...]]:
    """Plans original requests for native node strings and File Output item names."""
    texts = tuple(prop for prop in definition(node).properties if isinstance(prop, bpy.types.StringProperty) and (prop.is_path_supports_templates or prop.identifier == "label"))
    items = tuple(
        (item, prop) for item in (node.file_output_items if isinstance(node, bpy.types.CompositorNodeOutputFile) else ()) for prop in Option.of_optional(definition(item).name_property).to_list()
    )
    return accumulated(
        transform(before).map(lambda changed: tuple(Rename(node=node, owner=owner, prop=prop, before=before, requested=request) for request in changed.to_list()))
        for owner, prop in (*((node, prop) for prop in texts), *items)
        for before in (getattr(owner, prop.identifier),)
    ).map(lambda groups: tuple(chain.from_iterable(groups)))


def planned(nodes: Iterable[bpy.types.Node], properties: Replace) -> Outcome[tuple[Rename, ...]]:
    """Plans every string edit before writes."""
    replacement: str = properties.replace  # ty: ignore[unsound-assignment]

    def substituted(pattern: re.Pattern[str], text: str) -> Outcome[Option[str]]:
        insert: Callable[[re.Match[str]], str] = (lambda found: found.expand(replacement)) if properties.regex else (lambda _: replacement)
        try:
            changed = pattern.sub(insert, text)
        except (IndexError, re.PatternError) as error:
            return Error(Fault.of(Reason.INVALID, replacement, error))
        return Ok(Some(changed).filter(lambda value: value != text))

    return (
        Result[re.Pattern[str], Faults]
        .bind(
            catch(re.compile, exception=re.PatternError)(properties.find if properties.regex else re.escape(properties.find), 0 if properties.case else re.IGNORECASE).map_error(
                partial(Fault.of, Reason.INVALID, properties.find)
            ),
            lambda pattern: accumulated(edits(node, partial(substituted, pattern)) for node in nodes if isinstance(node, bpy.types.CompositorNodeOutputFile | bpy.types.NodeFrame)),
        )
        .map(lambda groups: tuple(chain.from_iterable(groups)))
    )


def _applied(renames: tuple[Rename, ...], unmatched: str) -> Outcome[Answer]:
    """Writes original requests after freeing renamed items for swaps, unchanged when every request matches its current text."""
    if not (changed := tuple(rename for rename in renames if rename.requested != rename.before)):
        return Ok(Unchanged(reason=unmatched))
    taken = {
        *(rename.before for rename in changed),
        *(rename.requested for rename in changed),
        *(item.name for rename in changed if isinstance(rename.node, bpy.types.CompositorNodeOutputFile) for item in rename.node.file_output_items),
    }

    def reported(writes: tuple[Outcome[object], ...]) -> Outcome[Answer]:
        return tallied(writes, (("INFO", f"Requested {len(changed)} fields across {len({rename.node.name for rename in changed})} nodes"),))

    return accumulated(
        written(rename.owner, (rename.prop.identifier,), next(f"{index}{'_' * number}" for number in count() if f"{index}{'_' * number}" not in taken))
        for index, rename in enumerate(changed)
        if rename.owner is not rename.node
    ).bind(lambda _: reported(tuple(written(rename.owner, (rename.prop.identifier,), rename.requested) for rename in changed)))


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(
    Replace,
    condition=Some(_EDITED),
    icon=Some("FILE_REFRESH"),
    invoke=Some(
        Suppressible[Replace](
            message=lambda context, properties: planned(context.selected_nodes, properties).map(
                lambda renames: "\n".join(f"{rename.node.name} · {rename.before} → {rename.requested}" for rename in renames)
            ),
            dialog=partial(bpy.types.WindowManager.invoke_props_dialog, width=400),
        )
    ),
)
def output_replace(call: Call[Replace]) -> Outcome[Answer]:
    """Replace text in selected paths, labels, and item names."""
    return planned(call.context.selected_nodes, call.properties).bind(lambda renames: _applied(renames, "No selected path holds find text"))


@Operator.of(Bump, condition=Some(_EDITED & _FILE_OUTPUT), options=frozenset({"UNDO_GROUPED"}), undo_group=Some("output_version"))
def output_version(call: Call[Bump]) -> Outcome[Answer]:
    """Move version tokens in active File Output node."""
    match call.context.active_node:
        case bpy.types.CompositorNodeOutputFile() as node:
            return edits(node, lambda text: Ok(stepped(text, call.properties.offset))).bind(lambda renames: _applied(renames, "No version token in node paths"))
        case _:
            return Error(Fault.of(Reason.ABSENT, "bpy.context", member="active_node"))


@Operator.of(Empty, icon=Some("ADD"))
def output_take(call: Call[Empty]) -> Outcome[Answer]:
    """Set scene take past matching written files."""
    return paired(ScenePaths().bound(call.identity), rooted(bpy.types.Scene, call.context)).bind(
        lambda pair: pair[0].took(pair[1], today()).map(lambda version: Done(notes=(("INFO", f"Version {version}"),)))
    )


@Operator.of(Empty, icon=Some("FILE_FOLDER"), options=frozenset())
def output_folder(call: Call[Empty]) -> Outcome[Answer]:
    """Open deepest existing output folder."""
    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: frame_file(scene.render, scene.frame_current))
        .bind(lambda file: called(bpy.ops.wm.path_open, {"filepath": str(existing(file.parent))}))
        .map(lambda _: Done())
    )


class ScenePaths(msgspec.Struct, frozen=True, kw_only=True):
    """Scene grammar attachment with replace, version, take, and folder commands, and the section of take and folder over the grammar store and its presets."""

    key: ClassVar[Key] = "output_paths"

    @staticmethod
    @contextmanager
    def changed(identity: Identity) -> Generator[Changed]:
        """Defers update writes to main thread for attachment lifetime."""
        with identity.reports.deferred(
            lambda handle: handle.resolve().bind(lambda scene: ScenePaths().bound(identity).bind(lambda pathed: pathed.recomposed(scene, today()))).map(lambda _: Nothing)
        ) as write:
            yield lambda group, _context: write(Handle.of(group.id_data))

    attachment: ClassVar[Attachment[Outputs]] = Attachment(key="outputs", form=Outputs, owner=bpy.types.Scene, changed=Some(changed))
    prerequisites: ClassVar[tuple[Registrable, ...]] = (attachment,)
    section: ClassVar[Section[Outputs]] = Section(
        key=key, rows=(OperatorButton(target=output_take), OperatorButton(target=output_folder)), store=Some(Store(attachment=attachment, presets=Some(Preset(key=key, form=Snapshot))))
    )

    def bound(self, identity: Identity) -> Outcome[Pathed]:
        """Binds registered scene settings."""
        return self.attachment.bound(identity).map(lambda attached: Pathed(attached=attached, stored=self.stored(identity)))

    @staticmethod
    def stored(identity: Identity) -> Stored[OutputPath]:
        """Names persistent render paths shared by preparation, workers, and finishing."""
        return Stored.keyed(
            identity, "output_path", msgspec.msgpack.Decoder(OutputPath, dec_hook=lambda kind, value: kind(value)), enc_hook=Some(lambda value: value.as_posix() if isinstance(value, Path) else value)
        )

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns commands and refresh handlers binding attachment on call."""

        def refreshed(saving: Option[Path]) -> Outcome[None]:
            return self.bound(identity).bind(lambda pathed: pathed.refreshed(identity, today(), saving))

        current = partial(refreshed, Nothing)
        return (
            output_replace,
            output_version,
            output_take,
            output_folder,
            Handler(handlers=bpy.app.handlers.load_post, body=current),
            Handler(handlers=bpy.app.handlers.save_pre, body=partial(located, refreshed)),
            Handler(handlers=bpy.app.handlers.save_post, body=current),
            *(
                Subscription(owner=owner, setting=owner.bl_rna.properties[key], body=current)
                for owner, key in ((bpy.types.ViewLayer, "name"), (bpy.types.Scene, "view_layers"), (bpy.types.PreferencesFilePaths, "render_output_directory"))
            ),
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Bump",
    "NodePaths",
    "OutputPath",
    "Outputs",
    "Pathed",
    "Rename",
    "Replace",
    "ScenePaths",
    "edits",
    "escaped",
    "file_output_path",
    "frame_file",
    "output_folder",
    "output_replace",
    "output_take",
    "output_version",
    "planned",
    "scene_still_path",
    "today",
]
