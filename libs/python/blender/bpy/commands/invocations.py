"""Invoke forms over window manager popups, suppressible confirmation, file and folder browsers, and cursor picks."""

from collections.abc import Callable, Mapping
from functools import partial
import inspect
from pathlib import Path
from typing import ClassVar, Final, TYPE_CHECKING

import bpy
from bpy_extras.io_utils import ExportHelper, ImportHelper
from expression import Nothing, Ok, Option
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.operators import Body, Returns
from blender.bpy.data.rna.descriptors import written
from blender.bpy.data.rna.paths import definition, preferred
from blender.bpy.data.rna.properties import annotated
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.extension.reports import alerted, rendered
from blender.records.faults import Fault, Outcome, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [CONSTANTS] ------------------------------------------------------------------------

_DIRECTORY: Final = bpy.props.StringProperty(subtype="DIR_PATH", options={"HIDDEN", "SKIP_SAVE"})

# --- [MODELS] ---------------------------------------------------------------------------


class Popup[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True):
    """Window manager popup: confirmation, properties dialog or popup, redo popup, enum search, or a menu, pie, or popover `draw` lays out.

    `call` binds host function keywords, checked at registration, with operator, event, and draw arguments in the function's parameter order.
    Menu, pie, or popover outlives the operator the host frees once invoke returns, and `draw` reads no operator properties.
    Redo popup adds `UNDO`, without which the host refuses it.
    """

    asks: ClassVar[bool] = True
    call: partial[set["rna_enums.OperatorReturnItems"]] | partial[None]
    message: Option[Callable[[bpy.types.Context, T], Outcome[str]]] = Nothing
    draw: Option[Callable[[bpy.types.UILayout, bpy.types.Context, Identity], None]] = Nothing

    def classed(self, body: Body[T], base: type[bpy.types.Operator], /) -> Outcome[Acquisitions]:
        """Derives the class invoking the bound host function, unregistered fault for keywords the function does not take or a draw apart from its `draw_func`."""

        def painted(popup: bpy.types.UIPopupMenu | bpy.types.UIPieMenu | bpy.types.UIPopover, context: bpy.types.Context) -> None:
            layout: bpy.types.UILayout = popup.layout  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            for draw in self.draw.to_list():
                draw(layout, context, body.identity)

        def opened(names: tuple[str, ...], operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> Outcome[Returns]:
            return (
                self.message
                .map(lambda compute: compute(context, body.properties(operator)).map(lambda text: {"message": text}))
                .default_value(Ok({}))
                .bind(lambda keywords: _opened(self.call, names, {"self": context.window_manager, "operator": operator, "event": event, "draw_func": painted}, **keywords))
            )

        def built(names: tuple[str, ...]) -> Acquisitions:
            redo = {"bl_options": base.bl_options | {"UNDO"}} if self.call.func == bpy.types.WindowManager.invoke_props_popup else {}
            return (body.derived(base, {"invoke": body.invoking(partial(opened, names)), **redo}),)

        return (
            _accepted(self.call.func, (*self.call.keywords, *(("message",) if self.message.is_some() else ())))
            .filter_with(lambda names: ("draw_func" in names) == self.draw.is_some(), lambda _: Fault.of(Reason.UNREGISTERED, repr(self.call.func), "draw_func"))
            .map(built)
        )


class Suppressible[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Properties dialog over a computed message, its faults drawn as alert rows, while the add-on preferences' `ask_<key>` flag holds, a declared flag drawing the checkbox that clears it after a run."""

    asks: ClassVar[bool] = True
    message: Callable[[bpy.types.Context, T], Outcome[str]]
    icon: "rna_enums.IconItems" = "NONE"
    dialog: partial[set["rna_enums.OperatorReturnItems"]] = partial(bpy.types.WindowManager.invoke_props_dialog)

    @staticmethod
    def flag(key: Key, /) -> str:
        """Returns the add-on preferences flag a command's confirmation reads."""
        return f"ask_{key}"

    def classed(self, body: Body[T], base: type[bpy.types.Operator], /) -> Outcome[Acquisitions]:
        """Derives the class asking while the flag holds or no flag is declared, running the body directly otherwise."""
        flag, form = self.flag(body.command.key), annotated(body.command.form)

        def declared() -> Option[bpy.types.AddonPreferences]:
            return body.identity.preferences(bpy.types.AddonPreferences).filter(lambda held: flag in definition(held).properties)

        def cleared(operator: bpy.types.Operator, context: bpy.types.Context) -> Outcome[None]:
            suppressed: bool = operator.suppress  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            preferences: bpy.types.Preferences = context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return preferred(preferences, body.identity.package, lambda held: written(held, (flag,), False)).map(lambda _: None) if suppressed else Ok(None)

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> Returns:
            return body.finished(operator, body.performed(operator, context).bind(lambda answer: cleared(operator, context).map(lambda _: answer)))

        def listed(layout: bpy.types.UILayout, _context: bpy.types.Context, properties: T, _identity: object) -> None:
            for prop in definition(properties).properties:
                if prop.identifier in form and not prop.is_hidden:
                    layout.prop(properties, prop.identifier)

        def draw(operator: bpy.types.Operator, context: bpy.types.Context) -> None:
            layout: bpy.types.UILayout = operator.layout  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            message = self.message(context, body.properties(operator))
            layout.use_property_split = True
            for text in message.to_option().to_list():
                layout.label_multiline(text=text, icon=self.icon)
            alerted(layout, message)
            body.command.draw.default_value(listed)(layout, context, body.properties(operator), body.identity)
            for _ in declared().to_list():
                layout.prop(operator.properties, "suppress")

        def asked(names: tuple[str, ...], operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> Outcome[Returns]:
            asking = declared().map(lambda held: bool(getattr(held, flag))).default_value(True)
            return _opened(self.dialog, names, {"self": context.window_manager, "operator": operator, "event": event}) if asking else Ok(execute(operator, context))

        def built(names: tuple[str, ...]) -> Acquisitions:
            return (
                body.derived(
                    base,
                    {
                        "__annotations__": {"suppress": bpy.props.BoolProperty(name="Do not show again", options={"SKIP_SAVE"})},
                        "draw": draw,
                        "execute": execute,
                        "invoke": body.invoking(partial(asked, names)),
                    },
                ),
            )

        return _accepted(self.dialog.func, tuple(self.dialog.keywords)).map(built)


class Open[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Import file browser filtering by the formats the properties choose, a set file path opening its confirmation instead."""

    asks: ClassVar[bool] = True
    extensions: Callable[[T], tuple[str, ...]]
    confirm_text: str = ""

    def classed(self, body: Body[T], base: type[bpy.types.Operator], /) -> Outcome[Acquisitions]:
        """Derives the `ImportHelper` class running the body over the chosen files."""

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> Returns:
            directory: str = operator.directory  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            files: bpy.types.bpy_prop_collection[bpy.types.OperatorFileListElement] = operator.files  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            filepath: str = operator.filepath  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return body.chose(operator, context, tuple(Path(directory, item.name) for item in files if item.name) or tuple(Path(path) for path in (filepath,) if path))

        return Ok((
            body.derived(
                base,
                {
                    "__annotations__": {
                        "filter_glob": _globbed(self.extensions),
                        "files": bpy.props.CollectionProperty(type=bpy.types.OperatorFileListElement, options={"HIDDEN", "SKIP_SAVE"}),
                        "directory": _DIRECTORY,
                    },
                    "check": _checking(ImportHelper, body),
                    "execute": execute,
                    "invoke": body.invoking(lambda operator, context, _event: Ok(ImportHelper.invoke_popup(operator, context, confirm_text=self.confirm_text))),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                },
                ImportHelper,
            ),
        ))


class Folder(msgspec.Struct, frozen=True):
    """Folder browser running the body over the chosen directory."""

    asks: ClassVar[bool] = True

    @staticmethod
    def classed[T: bpy.types.PropertyGroup](body: Body[T], base: type[bpy.types.Operator], /) -> Outcome[Acquisitions]:
        """Derives the class filtering the browser to folders and running the body over the accepted directory."""

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> Returns:
            directory: str = operator.directory  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return body.chose(operator, context, tuple(Path(path) for path in (directory,) if path))

        def opened(operator: bpy.types.Operator, _context: bpy.types.Context, _event: bpy.types.Event) -> Outcome[Returns]:
            bpy.types.WindowManager.fileselect_add(operator)
            return Ok({"RUNNING_MODAL"})

        return Ok((
            body.derived(
                base,
                {
                    "__annotations__": {"directory": _DIRECTORY, "filter_folder": bpy.props.BoolProperty(default=True, options={"HIDDEN", "SKIP_SAVE"})},
                    "execute": execute,
                    "invoke": body.invoking(opened),
                },
            ),
        ))


class Save[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Export file browser with optional output destinations, existing destinations confirmed once per invocation."""

    asks: ClassVar[bool] = True
    extensions: Callable[[T], tuple[str, *tuple[str, ...]]]
    destinations: Option[Callable[[Path, T], tuple[Path, ...]]] = Nothing

    def classed(self, body: Body[T], base: type[bpy.types.Operator], /) -> Outcome[Acquisitions]:
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
                (_present(paths).map_error(partial(Fault.of, Reason.UNREADABLE, repr(paths))) if confirming else Ok(()))
                .map(lambda found: replaced(operator, found) if found else body.finished(operator, body.performed(operator, context, paths)))
                .default_with(lambda faults: body.noted(operator, rendered(faults), "CANCELLED"))
            )

        def extension(operator: bpy.types.Operator) -> str:
            return self.extensions(body.properties(operator))[0]

        destined = {"check_existing": bpy.props.BoolProperty(default=False, options={"HIDDEN", "SKIP_SAVE"}), "replacing": bpy.props.BoolProperty(options={"HIDDEN", "SKIP_SAVE"})}
        return Ok((
            body.derived(
                base,
                {
                    "__annotations__": {"filter_glob": _globbed(self.extensions), **(destined if self.destinations.is_some() else {})},
                    "filename_ext": property(extension),
                    "check": _checking(ExportHelper, body),
                    "execute": execute,
                    "invoke": body.invoking(lambda operator, context, event: Ok(ExportHelper.invoke(operator, context, event))),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                },
                ExportHelper,
            ),
        ))


class Pick(msgspec.Struct, frozen=True, kw_only=True):
    """Invocation the host defers until the pointer picks a location, showing the pending cursor and asking for no input."""

    asks: ClassVar[bool] = False
    cursor: "rna_enums.WindowCursorItems"

    def classed[T: bpy.types.PropertyGroup](self, body: Body[T], base: type[bpy.types.Operator], /) -> Outcome[Acquisitions]:
        """Derives the class declaring `DEPENDS_ON_CURSOR` with the pending cursor."""
        return Ok((body.derived(base, {"bl_options": base.bl_options | {"DEPENDS_ON_CURSOR"}, "bl_cursor_pending": self.cursor}),))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _accepted(function: Callable[..., object], named: tuple[str, ...], /) -> Outcome[tuple[str, ...]]:
    """Returns the names a window manager function takes positionally once it takes every named keyword, unregistered fault naming the keywords it lacks.

    RNA class functions take required inputs positionally and others as keywords, script methods leading parameters positionally and the rest as keywords.
    """
    parameters = (
        Block
        .of_seq(definition(bpy.types.WindowManager).functions)
        .filter(lambda candidate: not candidate.use_self and getattr(bpy.types.WindowManager, candidate.identifier) == function)
        .try_head()
        .map(lambda found: tuple((parameter.identifier, parameter.is_required) for parameter in found.parameters if not parameter.is_output))
        .default_with(
            lambda: tuple(
                (name, parameter.kind is parameter.POSITIONAL_OR_KEYWORD)
                for name, parameter in inspect.signature(function).parameters.items()
                if parameter.kind in {parameter.POSITIONAL_OR_KEYWORD, parameter.KEYWORD_ONLY}
            )
        )
    )
    unknown = frozenset(named) - frozenset(name for name, positional in parameters if not positional)
    return Ok(tuple(name for name, positional in parameters if positional)).filter_with(lambda _: not unknown, lambda _: Fault.of(Reason.UNREGISTERED, repr(function), ", ".join(sorted(unknown))))


def _checking[T: bpy.types.PropertyGroup](helper: type[ImportHelper | ExportHelper], body: Body[T], /) -> Callable[[bpy.types.Operator, bpy.types.Context], bool]:
    """Class `check` running the file helper's check and the command's, redrawing when either changed a property."""
    return lambda operator, context: any((helper.check(operator, context), *body.command.check.map(lambda callback: callback(operator, context)).to_list()))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]


def _globbed[T: bpy.types.PropertyGroup](extensions: Callable[[T], tuple[str, ...]], /) -> bpy.props._PropertyDeferred:
    """File browser `filter_glob` property over the extensions the properties choose."""

    def glob(properties: T) -> str:
        return ";".join(f"*{extension}" for extension in extensions(properties))

    return bpy.props.StringProperty(options={"HIDDEN", "SKIP_SAVE"}, get=glob)


def _opened(call: partial[Returns] | partial[None], names: tuple[str, ...], arguments: Mapping[str, object], /, **keywords: str) -> Outcome[Returns]:
    """Calls a window manager function over the arguments it takes positionally, a script method's `None` read as `INTERFACE`, refused fault for a raise."""
    return catch(call, exception=TypeError)(*(arguments[name] for name in names), **keywords).map(lambda answer: answer or {"INTERFACE"}).map_error(partial(Fault.of, Reason.REFUSED, repr(call.func)))


@catch(exception=OSError)
def _present(paths: tuple[Path, ...], /) -> tuple[Path, ...]:
    """Returns the paths that exist, a path whose stat is refused raising."""
    return tuple(path for path in paths if path.exists())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Folder", "Open", "Pick", "Popup", "Save", "Suppressible"]
