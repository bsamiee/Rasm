"""Invoke forms over window manager popups, suppressible confirmation, file and folder browsers, and cursor picks."""

from collections.abc import Callable, Iterable, Mapping
from functools import partial
import inspect
from pathlib import Path
from typing import Annotated, ClassVar, TYPE_CHECKING

import bpy
from bpy_extras.io_utils import ExportHelper, ImportHelper
from expression import Option
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from rasm_bpy.commands.operators import Body
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions
from rasm_bpy.data._extension.reports import Refusal, rendered
from rasm_bpy.data._rna.descriptors import written
from rasm_bpy.data._rna.paths import preferred, rna_definition, rooted
from rasm_bpy.data._rna.properties import annotated
from rasm_records import Fault, Nothing, Ok, Outcome, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [MODELS] ---------------------------------------------------------------------------


class _Browsed(bpy.types.Operator):
    """Operator a file or folder browser fills, the accepted folder its `directory`."""

    directory: Annotated[str, bpy.props.StringProperty(subtype="DIR_PATH", options={"HIDDEN", "SKIP_SAVE"})]


class _Opening(_Browsed):
    """Import browser operator, its chosen files beside the `filepath` its `ImportHelper` base declares."""

    filepath: str
    files: Annotated[Iterable[bpy.types.OperatorFileListElement], bpy.props.CollectionProperty(type=bpy.types.OperatorFileListElement, options={"HIDDEN", "SKIP_SAVE"})]


class _Saving(bpy.types.Operator):
    """Export browser operator, the `filepath` its `ExportHelper` base declares, `replacing` once its run confirmed replacement."""

    filepath: str
    replacing: Annotated[bool, bpy.props.BoolProperty(options={"HIDDEN", "SKIP_SAVE"})]


class _Suppressing(bpy.types.Operator):
    """Confirmation operator whose checkbox clears the add-on preferences flag after a run."""

    suppress: Annotated[bool, bpy.props.BoolProperty(name="Do not show again", options={"SKIP_SAVE"})]


class Popup[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True):
    """Window manager popup: confirmation, properties dialog or popup, redo popup, enum search, or a menu, pie, or popover `draw` lays out.

    `call` binds host keywords, with operator, event, and draw arguments in parameter order.
    Menu, pie, or popover outlives the operator the host frees once invoke returns, and `draw` reads no operator properties.
    Redo popup adds `UNDO`, without which the host refuses it.
    """

    asks: ClassVar[bool] = True
    call: partial[set["rna_enums.OperatorReturnItems"]] | partial[None]
    message: Option[Callable[[bpy.types.Context, T], Outcome[str]]] = Nothing
    draw: Option[Callable[[bpy.types.UILayout, bpy.types.Context, Identity], None]] = Nothing

    def classed(self, body: Body[T], base: type[bpy.types.Operator], /) -> Acquisitions:
        """Derives the class invoking the bound host function."""
        names = _parameters(self.call.func)

        def painted(popup: bpy.types.UIPopupMenu | bpy.types.UIPieMenu | bpy.types.UIPopover, context: bpy.types.Context) -> None:
            for layout in Option.of_optional(popup.layout).to_list():
                for draw in self.draw.to_list():
                    draw(layout, context, body.identity)

        def opened(operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> Outcome[set["rna_enums.OperatorReturnItems"]]:
            return (
                self.message
                .map(lambda compute: compute(context, body.properties(operator)).map(lambda text: {"message": text}))
                .default_value(Ok({}))
                .bind(lambda keywords: _opened(self.call, names, {"self": context.window_manager, "operator": operator, "event": event, "draw_func": painted}, **keywords))
            )

        redo = {"bl_options": base.bl_options | {"UNDO"}} if self.call.func == bpy.types.WindowManager.invoke_props_popup else {}
        return (body.derived(base, {"invoke": body.invoking(opened), **redo}),)


class Suppressible[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Properties dialog over a computed message, its faults logged, while the add-on preferences' `ask_<key>` flag holds, a declared flag drawing the checkbox that clears it after a run."""

    asks: ClassVar[bool] = True
    message: Callable[[bpy.types.Context, T], Outcome[str]]
    icon: "rna_enums.IconItems" = "NONE"
    dialog: partial[set["rna_enums.OperatorReturnItems"]] = partial(bpy.types.WindowManager.invoke_props_dialog)

    @staticmethod
    def flag(key: Key, /) -> str:
        """Returns the add-on preferences flag a command's confirmation reads."""
        return f"ask_{key}"

    def classed(self, body: Body[T], base: type[bpy.types.Operator], /) -> Acquisitions:
        """Derives the class asking while the flag holds or no flag is declared, running the body directly otherwise."""
        flag, form, names = self.flag(body.command.key), annotated(body.command.form), _parameters(self.dialog.func)

        def declared() -> Option[bpy.types.AddonPreferences]:
            return body.identity.preferences(bpy.types.AddonPreferences).filter(lambda held: flag in rna_definition(held).properties)

        def cleared(operator: _Suppressing, context: bpy.types.Context) -> Outcome[None]:
            write = partial(preferred, package=body.identity.package, write=lambda held: written(held, (flag,), value=False))
            return rooted(bpy.types.Preferences, context).bind(write).map(lambda _: None) if operator.suppress else Ok(None)

        def execute(operator: _Suppressing, context: bpy.types.Context) -> set["rna_enums.OperatorReturnItems"]:
            return body.finished(operator, body.performed(operator, context).bind(lambda answer: cleared(operator, context).map(lambda _: answer)))

        def listed(layout: bpy.types.UILayout, _context: bpy.types.Context, properties: T, _identity: object) -> None:
            for prop in rna_definition(properties).properties:
                if prop.identifier in form and not prop.is_hidden:
                    layout.prop(properties, prop.identifier)

        def draw(operator: _Suppressing, context: bpy.types.Context) -> None:
            for layout in Option.of_optional(operator.layout).to_list():
                layout.use_property_split = True
                for text in body.identity.reports.settled(self.message(context, body.properties(operator))).to_list():
                    layout.label_multiline(text=text, icon=self.icon)
                body.command.draw.default_value(listed)(layout, context, body.properties(operator), body.identity)
                for _ in declared().to_list():
                    layout.prop(operator.properties, "suppress")

        def asked(operator: _Suppressing, context: bpy.types.Context, event: bpy.types.Event) -> Outcome[set["rna_enums.OperatorReturnItems"]]:
            asking = declared().map(lambda held: bool(getattr(held, flag))).default_value(value=True)
            return _opened(self.dialog, names, {"self": context.window_manager, "operator": operator, "event": event}) if asking else Ok(execute(operator, context))

        return (body.derived(base, {"draw": draw, "execute": execute, "invoke": body.invoking(asked)}, _Suppressing),)


class Open[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Import file browser filtering by the formats the properties choose, a set file path opening its confirmation instead."""

    asks: ClassVar[bool] = True
    extensions: Callable[[T], tuple[str, ...]]
    confirm_text: str = ""

    def classed(self, body: Body[T], base: type[bpy.types.Operator], /) -> Acquisitions:
        """Derives the `ImportHelper` class running the body over the chosen files."""

        def execute(operator: _Opening, context: bpy.types.Context) -> set["rna_enums.OperatorReturnItems"]:
            chosen = tuple(Path(operator.directory, item.name) for item in operator.files if item.name)
            return body.chose(operator, context, chosen or tuple(Path(path) for path in (operator.filepath,) if path))

        return (
            body.derived(
                base,
                {
                    "__annotations__": {"filter_glob": _globbed(self.extensions)},
                    "check": _checking(ImportHelper, body),
                    "execute": execute,
                    "invoke": body.invoking(lambda operator, context, _event: Ok(ImportHelper.invoke_popup(operator, context, confirm_text=self.confirm_text))),  # ty: ignore[invalid-argument-type]
                },
                ImportHelper,
                _Opening,
            ),
        )


class Folder(msgspec.Struct, frozen=True):
    """Folder browser running the body over the chosen directory."""

    asks: ClassVar[bool] = True

    @staticmethod
    def classed[T: bpy.types.PropertyGroup](body: Body[T], base: type[bpy.types.Operator], /) -> Acquisitions:
        """Derives the class filtering the browser to folders and running the body over the accepted directory."""

        def execute(operator: _Browsed, context: bpy.types.Context) -> set["rna_enums.OperatorReturnItems"]:
            return body.chose(operator, context, tuple(Path(path) for path in (operator.directory,) if path))

        def opened(operator: _Browsed, _context: bpy.types.Context, _event: bpy.types.Event) -> Outcome[set["rna_enums.OperatorReturnItems"]]:
            bpy.types.WindowManager.fileselect_add(operator)
            return Ok({"RUNNING_MODAL"})

        return (
            body.derived(
                base, {"__annotations__": {"filter_folder": bpy.props.BoolProperty(default=True, options={"HIDDEN", "SKIP_SAVE"})}, "execute": execute, "invoke": body.invoking(opened)}, _Browsed
            ),
        )


class Save[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Export file browser with optional output destinations, existing destinations confirmed once per invocation."""

    asks: ClassVar[bool] = True
    extensions: Callable[[T], tuple[str, *tuple[str, ...]]]
    destinations: Option[Callable[[Path, T], tuple[Path, ...]]] = Nothing

    def classed(self, body: Body[T], base: type[bpy.types.Operator], /) -> Acquisitions:
        """Derives the `ExportHelper` class, confirming replacement of existing destinations before the body runs."""

        def located(operator: _Saving) -> tuple[Path, ...]:
            return tuple(found for path in (operator.filepath,) if path for found in self.destinations.map(lambda project: project(Path(path), body.properties(operator))).default_value((Path(path),)))

        def replaced(operator: _Saving, found: tuple[Path, ...]) -> set["rna_enums.OperatorReturnItems"]:
            operator.replacing = True
            iface = bpy.app.translations.pgettext_iface
            return bpy.types.WindowManager.invoke_confirm(operator, None, title=iface("Replace existing files?"), message="\n".join(map(str, found)), confirm_text=iface("Replace"), translate=False)

        def execute(operator: _Saving, context: bpy.types.Context) -> set["rna_enums.OperatorReturnItems"]:
            confirming = self.destinations.is_some() and operator.options.is_invoke and not operator.options.is_repeat and not operator.replacing
            paths = located(operator)
            return (
                (_present(paths).map_error(partial(Fault.of, Reason.UNREADABLE, repr(paths))) if confirming else Ok(()))
                .map(lambda found: replaced(operator, found) if found else body.chose(operator, context, paths))
                .default_with(lambda faults: body.noted(operator, rendered(faults), "CANCELLED"))
            )

        def extension(operator: bpy.types.Operator) -> str:
            return self.extensions(body.properties(operator))[0]

        unchecked = {"check_existing": bpy.props.BoolProperty(default=False, options={"HIDDEN", "SKIP_SAVE"})} if self.destinations.is_some() else {}
        return (
            body.derived(
                base,
                {
                    "__annotations__": {"filter_glob": _globbed(self.extensions), **unchecked},
                    "filename_ext": property(extension),
                    "check": _checking(ExportHelper, body),
                    "execute": execute,
                    "invoke": body.invoking(lambda operator, context, event: Ok(ExportHelper.invoke(operator, context, event))),  # ty: ignore[invalid-argument-type]
                },
                ExportHelper,
                _Saving,
            ),
        )


class Pick(msgspec.Struct, frozen=True, kw_only=True):
    """Invocation the host defers until the pointer picks a location, showing the pending cursor and asking for no input."""

    asks: ClassVar[bool] = False
    cursor: "rna_enums.WindowCursorItems"

    def classed[T: bpy.types.PropertyGroup](self, body: Body[T], base: type[bpy.types.Operator], /) -> Acquisitions:
        """Derives the class declaring `DEPENDS_ON_CURSOR` with the pending cursor."""
        return (body.derived(base, {"bl_options": base.bl_options | {"DEPENDS_ON_CURSOR"}, "bl_cursor_pending": self.cursor}),)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _parameters[**P, R](function: Callable[P, R], /) -> tuple[str, ...]:
    """Returns positional parameters of a window manager function.

    RNA class functions take required inputs positionally and others as keywords, script methods leading parameters positionally and the rest as keywords.
    """
    return (
        Block
        .of_seq(rna_definition(bpy.types.WindowManager).functions)
        .filter(lambda candidate: not candidate.use_self and getattr(bpy.types.WindowManager, candidate.identifier) == function)
        .try_head()
        .map(lambda found: tuple(parameter.identifier for parameter in found.parameters if parameter.is_required and not parameter.is_output))
        .default_with(lambda: tuple(name for name, parameter in inspect.signature(function).parameters.items() if parameter.kind in {parameter.POSITIONAL_ONLY, parameter.POSITIONAL_OR_KEYWORD}))
    )


def _checking[T: bpy.types.PropertyGroup](helper: type[ImportHelper | ExportHelper], body: Body[T], /) -> Callable[[bpy.types.Operator, bpy.types.Context], bool]:
    """Class `check` running the file helper's check and the command's, redrawing when either changed a property."""
    return lambda operator, context: any((helper.check(operator, context), *body.command.check.map(lambda callback: callback(operator, context)).to_list()))  # ty: ignore[invalid-argument-type]


def _globbed[T: bpy.types.PropertyGroup](extensions: Callable[[T], tuple[str, ...]], /) -> bpy.props._PropertyDeferred:
    """File browser `filter_glob` property over the extensions the properties choose."""

    def glob(properties: T) -> str:
        return ";".join(f"*{extension}" for extension in extensions(properties))

    return bpy.props.StringProperty(options={"HIDDEN", "SKIP_SAVE"}, get=glob)


def _opened(
    call: partial[set["rna_enums.OperatorReturnItems"]] | partial[None], names: tuple[str, ...], arguments: Mapping[str, object], /, **keywords: str
) -> Outcome[set["rna_enums.OperatorReturnItems"]]:
    """Calls a window manager function over the arguments it takes positionally, a script method's `None` read as `INTERFACE`, refused fault for a raise."""
    return (
        catch(call, exception=TypeError)(*(arguments[name] for name in names), **keywords)
        .map(lambda answer: set["rna_enums.OperatorReturnItems"](("INTERFACE",)) if answer is None else answer)
        .map_error(partial(Fault.of, Refusal.REFUSED, repr(call.func)))
    )


@catch(exception=OSError)
def _present(paths: tuple[Path, ...], /) -> tuple[Path, ...]:
    """Returns the paths that exist, a path whose stat is refused raising."""
    return tuple(path for path in paths if path.exists())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Folder", "Open", "Pick", "Popup", "Save", "Suppressible"]
