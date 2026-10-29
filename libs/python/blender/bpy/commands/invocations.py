"""Invoke forms over window manager popups, suppressible confirmation, file and folder browsers, and cursor picks."""

from collections.abc import Callable
from functools import partial
import inspect
from pathlib import Path
from typing import TYPE_CHECKING

import bpy
from bpy_extras.io_utils import ExportHelper, ImportHelper
from expression import Nothing, Ok, Option
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.operators import Body, Returns
from blender.bpy.data.rna.descriptors import written
from blender.bpy.data.rna.mirrors import annotated
from blender.bpy.data.rna.paths import definition, preferred
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.extension.reports import rendered, window_manager
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Outcome, RecordReason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [MODELS] ---------------------------------------------------------------------------


class Popup[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True):
    """Window manager popup the operator opens: confirmation, properties dialog or popup, redo popup, enum search, or a menu, pie, or popover `draw` lays out.

    `call` binds the host function's keywords, checked against its parameters at registration, its positional operator, event, and draw arguments following the function's own parameters.
    A menu, pie, or popover outlives the operator the host frees once invoke answers, so `draw` reads no operator properties.
    A redo popup adds `UNDO`, without which the host refuses it.
    """

    call: partial[set["rna_enums.OperatorReturnItems"]] | partial[None]
    message: Option[Callable[[bpy.types.Context, T], Outcome[str]]] = Nothing
    draw: Option[Callable[[bpy.types.UILayout, bpy.types.Context, Identity], None]] = Nothing

    @property
    def asks(self) -> bool:
        """Whether the popup asks for input, always."""
        return True

    def classed[S](self, body: Body[T, S], base: type[bpy.types.Operator], /) -> Outcome[Acquisitions]:
        """Derives the class invoking the bound host function, register fault for keywords or a draw the function does not take."""

        def painted(popup: bpy.types.UIPopupMenu | bpy.types.UIPieMenu | bpy.types.UIPopover, context: bpy.types.Context) -> None:
            for draw in self.draw.to_list():
                draw(Body.layout(popup), context, body.identity)

        def opened(names: tuple[str, ...], operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> Outcome[Returns]:
            arguments = {"self": window_manager(), "operator": operator, "event": event, "draw_func": painted}
            return (
                self.message
                .map(lambda compute: compute(context, body.properties(operator)).map(lambda text: {"message": text}))
                .default_value(Ok({}))
                .bind(
                    lambda keywords: (
                        catch(self.call, exception=TypeError)(*(arguments[name] for name in names), **keywords).map(answered).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(self.call.func)))
                    )
                )
            )

        def answered(answer: Returns | None) -> Returns:
            return {"INTERFACE"} if answer is None else answer

        def built(names: tuple[str, ...]) -> Acquisitions:
            redo = {"bl_options": base.bl_options | {"UNDO"}} if self.call.func == bpy.types.WindowManager.invoke_props_popup else {}
            return (type(base.__name__, (base,), {"invoke": body.invoking(partial(opened, names)), **redo}),)

        return (
            _accepted(self.call.func, (*self.call.keywords, *(("message",) if self.message.is_some() else ())))
            .filter_with(lambda names: "draw_func" not in names or self.draw.is_some(), lambda _: Fault.of(BlenderReason.UNREGISTERED, repr(self.call.func), member="draw"))
            .map(built)
        )


class Suppressible[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Properties dialog over a computed message while the add-on preferences' `ask_<key>` flag holds, its checkbox clearing the flag after a run."""

    message: Callable[[bpy.types.Context, T], str]
    icon: "rna_enums.IconItems" = "NONE"
    dialog: partial[set["rna_enums.OperatorReturnItems"]] = partial(bpy.types.WindowManager.invoke_props_dialog)

    @property
    def asks(self) -> bool:
        """Whether the dialog asks for input, always."""
        return True

    @staticmethod
    def flag(key: Key, /) -> str:
        """Returns the add-on preferences flag a command's confirmation reads."""
        return f"ask_{key}"

    def classed[S](self, body: Body[T, S], base: type[bpy.types.Operator], /) -> Outcome[Acquisitions]:
        """Derives the class asking while the flag holds or no flag is declared, running the body directly otherwise."""
        flag, form = self.flag(body.command.key), annotated(body.command.form)

        def asked() -> bool:
            return body.identity.preferences(bpy.types.AddonPreferences).filter(lambda held: flag in definition(held).properties).map(lambda held: bool(getattr(held, flag))).default_value(True)

        def cleared(operator: bpy.types.Operator, context: bpy.types.Context) -> Outcome[None]:
            suppressed: bool = operator.suppress  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            preferences: bpy.types.Preferences = context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return preferred(preferences, body.identity.package, lambda held: written(held, (flag,), False)).map(lambda _: None) if suppressed else Ok(None)

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> Returns:
            return body.finished(operator, body.performed(operator, context).bind(lambda answer: cleared(operator, context).map(lambda _: answer)))

        def listed(layout: bpy.types.UILayout, _context: bpy.types.Context, properties: bpy.types.OperatorProperties, _identity: object) -> None:
            for prop in definition(properties).properties:
                if prop.identifier in form and not prop.is_hidden:
                    layout.prop(properties, prop.identifier)

        def draw(operator: bpy.types.Operator, context: bpy.types.Context) -> None:
            layout = Body.layout(operator)
            layout.use_property_split = True
            layout.label_multiline(text=self.message(context, body.properties(operator)), icon=self.icon)
            body.command.draw.default_value(listed)(layout, context, operator.properties, body.identity)
            layout.prop(operator.properties, "suppress")

        def built(_names: tuple[str, ...]) -> Acquisitions:
            return (
                type(
                    base.__name__,
                    (base,),
                    {
                        "__annotations__": {"suppress": bpy.props.BoolProperty(name="Do not show again", options={"SKIP_SAVE"})},
                        "draw": draw,
                        "execute": execute,
                        "invoke": body.invoking(lambda operator, context, _event: Ok(self.dialog(operator) if asked() else execute(operator, context))),
                    },
                ),
            )

        return _accepted(self.dialog.func, tuple(self.dialog.keywords)).map(built)


class Open[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Import file browser filtering by the formats the properties choose, a set file path opening its confirmation instead."""

    extensions: Callable[[T], tuple[str, ...]]
    confirm_text: str = ""

    @property
    def asks(self) -> bool:
        """Whether the browser asks for input, always."""
        return True

    def classed[S](self, body: Body[T, S], base: type[bpy.types.Operator], /) -> Outcome[Acquisitions]:
        """Derives the `ImportHelper` class running the body over the chosen files."""

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> Returns:
            directory: str = operator.directory  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            files: bpy.types.bpy_prop_collection[bpy.types.OperatorFileListElement] = operator.files  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            filepath: str = operator.filepath  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            paths = tuple(Path(directory, item.name) for item in files if item.name) or tuple(Path(path) for path in (filepath,) if path)
            return body.finished(operator, Ok(paths).filter_with(bool, lambda _: Fault.of(BlenderReason.UNCHOSEN, body.command.title)).bind(lambda chosen: body.performed(operator, context, chosen)))

        return Ok((
            type(
                base.__name__,
                (ImportHelper, base),
                {
                    "__annotations__": {
                        "filter_glob": _globbed(self.extensions),
                        "files": bpy.props.CollectionProperty(type=bpy.types.OperatorFileListElement, options={"HIDDEN", "SKIP_SAVE"}),
                        "directory": bpy.props.StringProperty(subtype="DIR_PATH", options={"HIDDEN", "SKIP_SAVE"}),
                    },
                    "check": _checking(ImportHelper, body),
                    "execute": execute,
                    "invoke": body.invoking(lambda operator, context, _event: Ok(ImportHelper.invoke_popup(operator, context, confirm_text=self.confirm_text))),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                },
            ),
        ))


class Folder(msgspec.Struct, frozen=True):
    """Folder browser running the body over the chosen directory."""

    @property
    def asks(self) -> bool:
        """Whether the browser asks for input, always."""
        return True

    @staticmethod
    def classed[T: bpy.types.PropertyGroup, S](body: Body[T, S], base: type[bpy.types.Operator], /) -> Outcome[Acquisitions]:
        """Derives the class filtering the browser to folders and running the body over the accepted directory."""

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> Returns:
            directory: str = operator.directory  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return body.finished(
                operator, Ok(directory).filter_with(bool, lambda _: Fault.of(BlenderReason.UNCHOSEN, body.command.title)).bind(lambda chosen: body.performed(operator, context, (Path(chosen),)))
            )

        def opened(operator: bpy.types.Operator, _context: bpy.types.Context, _event: bpy.types.Event) -> Outcome[Returns]:
            bpy.types.WindowManager.fileselect_add(operator)
            return Ok({"RUNNING_MODAL"})

        return Ok((
            type(
                base.__name__,
                (base,),
                {
                    "__annotations__": {
                        "directory": bpy.props.StringProperty(subtype="DIR_PATH", options={"HIDDEN", "SKIP_SAVE"}),
                        "filter_folder": bpy.props.BoolProperty(default=True, options={"HIDDEN", "SKIP_SAVE"}),
                    },
                    "execute": execute,
                    "invoke": body.invoking(opened),
                },
            ),
        ))


class Save[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Export file browser with optional output destinations, existing destinations confirmed once per invocation."""

    extensions: Callable[[T], tuple[str, *tuple[str, ...]]]
    destinations: Option[Callable[[Path, T], tuple[Path, ...]]] = Nothing

    @property
    def asks(self) -> bool:
        """Whether the browser asks for input, always."""
        return True

    def classed[S](self, body: Body[T, S], base: type[bpy.types.Operator], /) -> Outcome[Acquisitions]:
        """Derives the `ExportHelper` class, confirming replacement of existing destinations before the body runs."""

        def located(operator: bpy.types.Operator) -> tuple[Path, ...]:
            filepath: str = operator.filepath  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return self.destinations.map(lambda project: project(Path(filepath), body.properties(operator))).default_value((Path(filepath),))

        def replaced(operator: bpy.types.Operator, found: tuple[Path, ...]) -> Returns:
            operator.replacing = True  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            iface = bpy.app.translations.pgettext_iface
            return bpy.types.WindowManager.invoke_confirm(operator, None, title=iface("Replace existing files?"), message="\n".join(map(str, found)), confirm_text=iface("Replace"), translate=False)

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> Returns:
            confirming: bool = self.destinations.is_some() and operator.options.is_invoke and not operator.options.is_repeat and not operator.replacing  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            paths = located(operator)
            return (
                (_present(paths).map_error(partial(Fault.of, RecordReason.UNREADABLE, repr(paths))) if confirming else Ok(()))
                .map(lambda found: replaced(operator, found) if found else body.finished(operator, body.performed(operator, context, paths)))
                .default_with(lambda faults: body.noted(operator, rendered(faults), "CANCELLED"))
            )

        def extension(operator: bpy.types.Operator) -> str:
            return self.extensions(body.properties(operator))[0]

        destined = {"check_existing": bpy.props.BoolProperty(default=False, options={"HIDDEN", "SKIP_SAVE"}), "replacing": bpy.props.BoolProperty(options={"HIDDEN", "SKIP_SAVE"})}
        return Ok((
            type(
                base.__name__,
                (ExportHelper, base),
                {
                    "__annotations__": {"filter_glob": _globbed(self.extensions), **(destined if self.destinations.is_some() else {})},
                    "filename_ext": property(extension),
                    "check": _checking(ExportHelper, body),
                    "execute": execute,
                    "invoke": body.invoking(lambda operator, context, event: Ok(ExportHelper.invoke(operator, context, event))),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                },
            ),
        ))


class Pick(msgspec.Struct, frozen=True, kw_only=True):
    """Invocation the host defers until the pointer picks a location, showing the pending cursor."""

    cursor: "rna_enums.WindowCursorItems"

    @property
    def asks(self) -> bool:
        """Whether the pick asks for input, never, the pointer location coming with the event."""
        return False

    def classed[T: bpy.types.PropertyGroup, S](self, _body: Body[T, S], base: type[bpy.types.Operator], /) -> Outcome[Acquisitions]:
        """Derives the class declaring `DEPENDS_ON_CURSOR` with the pending cursor."""
        return Ok((type(base.__name__, (base,), {"bl_options": base.bl_options | {"DEPENDS_ON_CURSOR"}, "bl_cursor_pending": self.cursor}),))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _accepted(function: Callable[..., object], named: tuple[str, ...], /) -> Outcome[tuple[str, ...]]:
    """Returns the names a window manager function takes positionally once it takes every named keyword, register fault naming the keywords it lacks.

    An RNA class function takes its required inputs positionally and its other inputs as keywords, a script method its leading parameters positionally and the rest as keywords.
    """
    positional, keywords = (
        Block
        .of_seq(definition(bpy.types.WindowManager).functions)
        .filter(lambda candidate: not candidate.use_self and getattr(bpy.types.WindowManager, candidate.identifier) == function)
        .try_head()
        .map(
            lambda found: (
                tuple(parameter.identifier for parameter in found.parameters if parameter.is_required),
                frozenset(parameter.identifier for parameter in found.parameters if not parameter.is_required and not parameter.is_output),
            )
        )
        .default_with(
            lambda: (
                tuple(name for name, parameter in inspect.signature(function).parameters.items() if parameter.kind is parameter.POSITIONAL_OR_KEYWORD),
                frozenset(name for name, parameter in inspect.signature(function).parameters.items() if parameter.kind is parameter.KEYWORD_ONLY),
            )
        )
    )
    unknown = frozenset(named) - keywords
    return Ok(positional).filter_with(lambda _: not unknown, lambda _: Fault.of(BlenderReason.UNREGISTERED, repr(function), ", ".join(sorted(unknown))))


def _checking[T: bpy.types.PropertyGroup, S](helper: type[ImportHelper | ExportHelper], body: Body[T, S], /) -> Callable[[bpy.types.Operator, bpy.types.Context], bool]:
    """Class `check` running the file helper's check and the command's, redrawing when either changed a property."""
    return lambda operator, context: any((helper.check(operator, context), *body.command.check.map(lambda callback: callback(operator, context)).to_list()))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]


def _globbed[T: bpy.types.PropertyGroup](extensions: Callable[[T], tuple[str, ...]], /) -> bpy.props._PropertyDeferred:
    """File browser `filter_glob` property over the extensions the properties choose."""

    def glob(properties: T) -> str:
        return ";".join(f"*{extension}" for extension in extensions(properties))

    return bpy.props.StringProperty(options={"HIDDEN", "SKIP_SAVE"}, get=glob)


@catch(exception=OSError)
def _present(paths: tuple[Path, ...], /) -> tuple[Path, ...]:
    """Returns the paths that exist, a path whose stat is refused raising."""
    return tuple(path for path in paths if path.exists())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Folder", "Open", "Pick", "Popup", "Save", "Suppressible"]
