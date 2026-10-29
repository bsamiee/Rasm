"""Extension commands, operator calls, operator placements in the host keymap-data form, and registered popups their window manager call operator opens."""

from abc import abstractmethod
from collections.abc import Callable, Mapping
from contextlib import nullcontext
from enum import Enum
from types import MappingProxyType
from typing import Self, TYPE_CHECKING

import _bpy_types
import bpy
from expression import Nothing, Option, Result, try_downcast
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.conditions import Condition, polled
from blender.bpy.data.rna.descriptors import Scalar
from blender.bpy.data.rna.paths import definition, rna, RnaPath, Struct
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.records.faults import Fault, Faults, Outcome, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Operation = Callable[..., set[rna_enums.OperatorReturnItems]]
type Setting = Scalar | tuple[Scalar, ...]
type Pairs = tuple[tuple[str, Setting], ...]


class Called(Enum):
    """Registered popup kind with the host base it extends, the window manager operator calling it, and the layout button opening it in place, a pie opening through its operator alone."""

    MENU = (bpy.types.Menu, bpy.ops.wm.call_menu, "menu")
    PIE = (bpy.types.Menu, bpy.ops.wm.call_menu_pie, None)
    PANEL = (bpy.types.Panel, bpy.ops.wm.call_panel, "popover")
    SHELF = (bpy.types.AssetShelf, bpy.ops.wm.call_asset_shelf_popover, "template_asset_shelf_popover")

    def __init__(self, base: type[Struct], operator: Operation, button: str | None) -> None:
        """Holds the host base, the call operator, and the `UILayout` function the button draws through."""
        self.base, self.operator = base, operator
        self.button: Option[bpy.types.Function] = Option.of_optional(button).map(definition(bpy.types.UILayout).functions.__getitem__)


# --- [MODELS] ---------------------------------------------------------------------------


class Command(msgspec.Struct, frozen=True, kw_only=True):
    """Extension operator with poll condition, button icon, and search visibility, its host name and title derived from its key."""

    condition: Option[Condition] = Nothing
    icon: Option["rna_enums.IconItems"] = Nothing
    internal: bool = False

    # --- [TEXT]

    @property
    @abstractmethod
    def key(self) -> Key:
        """Identifier the host name derives from."""

    @property
    @abstractmethod
    def description(self) -> str:
        """Tooltip text."""

    @property
    def title(self) -> str:
        """Label the key displays as."""
        return bpy.path.display_name(self.key)

    @property
    def asks(self) -> bool:
        """Whether the command asks for input before it runs, its button label ending in an ellipsis."""
        return False

    # --- [REGISTRATION]

    @abstractmethod
    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Builds command registrations."""

    # --- [CLASS_MEMBERS]

    def namespace(self, name: str, flags: frozenset["rna_enums.OperatorTypeFlagItems"], /) -> dict[str, object]:
        """Returns class members with poll message of condition faults."""
        return {
            "bl_idname": name,
            "bl_label": f"{self.title}..." if self.asks else self.title,
            "bl_description": self.description,
            "bl_options": flags | (frozenset({"INTERNAL"}) if self.internal else frozenset()),
            **polled(self.condition),
        }


class Opened(msgspec.Struct, frozen=True):
    """Registered menu, panel, or asset shelf by class or extension key, opened as its kind, a class an extension or the stock interface declares."""

    target: type[bpy.types.Menu | bpy.types.Panel | bpy.types.AssetShelf | _bpy_types.Menu | _bpy_types.Panel | _bpy_types.AssetShelf] | Key
    kind: Called

    def __post_init__(self) -> None:
        """Refuses a class outside the kind's host base."""
        base = self.kind.base
        if not isinstance(self.target, str) and not issubclass(self.target, base):
            raise TypeError(f"{self.target.__name__} does not extend {base.__name__}")


class Placement(msgspec.Struct, frozen=True):
    """Operator with property values in the host keymap-data form."""

    operator: Command | Operation | Opened
    values: Mapping[str, Setting] = MappingProxyType({})

    @classmethod
    def context(cls, operator: Operation, path: RnaPath, /, **values: Setting) -> Self:
        """Places a stock `wm.context_*` operator over a context path with its other property values."""
        return cls(operator, MappingProxyType({"data_path": rna(path), **values}))

    def resolved(self, identity: Identity, /) -> Outcome[tuple[str, Pairs]]:
        """Resolves operator idname and property pairs, an opened popup's name leading its call operator's values."""
        values = tuple(self.values.items())
        match self.operator:
            case Opened(kind=kind) as opened:
                return idname(identity, opened).map(lambda name: (_dotted(kind.operator), (("name", name), *values)))
            case operator:
                return idname(identity, operator).map(lambda name: (name, values))


# --- [OPERATIONS] -----------------------------------------------------------------------


def idname(identity: Identity, target: Command | Operation | Opened, /) -> Outcome[str]:
    """Resolves an extension key under its host name form, a class's RNA identifier, or a stock operator's `bpy.ops` path."""
    match target:
        case Opened(target=str(key), kind=kind):
            return identity.name(kind.base, key)
        case Opened(target=opened):
            declared: type[Struct] = opened  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return Result[str, Faults].Ok(definition(declared).identifier)
        case Command() as command:
            return identity.name(bpy.types.Operator, command.key)
        case _:
            return Result[str, Faults].Ok(_dotted(target))


def filled(placed: tuple[str, Pairs], properties: object, /) -> Outcome[None]:
    """Writes placed pairs onto the operator properties a button, keymap item, gizmo, or macro step answers."""
    operator, pairs = placed
    return (
        Option
        .of_optional(try_downcast(bpy.types.OperatorProperties, properties))
        .to_result_with(lambda: Fault.of(Reason.ABSENT, operator, member="properties"))
        .bind(lambda held: assigned(held, {(name,): value for name, value in pairs}))
    )


def called(
    operator: Operation,
    properties: Mapping[str, Setting | Option[Setting]],
    /,
    *,
    context: "rna_enums.OperatorContextItems" = "EXEC_DEFAULT",
    undo: bool = False,
    override: Option[bpy.types.ContextTempOverride] = Nothing,
) -> Outcome[frozenset["rna_enums.OperatorReturnItems"]]:
    """Calls operator, leaving Nothing-valued properties unset."""
    keywords = {key: value for key, held in properties.items() for value in (held.to_list() if isinstance(held, Option) else (held,))}
    identifier = _dotted(operator)
    request = f"bpy.ops.{identifier}({', '.join(f'{key}={value!r}' for key, value in keywords.items())})"

    def run() -> frozenset["rna_enums.OperatorReturnItems"]:
        with override.default_with(nullcontext):
            return frozenset(operator(context, undo, **keywords))

    def refused(error: Exception) -> Faults:
        match error:
            case AttributeError():
                return Fault.of(Reason.ABSENT, "bpy.ops", error, member=identifier)
            case _:
                return Fault.of(Reason.FAILED, request, error)

    ran: Result[frozenset[rna_enums.OperatorReturnItems], Exception] = catch(run, exception=(AttributeError, RuntimeError, TypeError))()  # type: ignore[call-overload]  # ty: ignore[invalid-argument-type]
    return ran.map_error(refused).filter_with(
        lambda answer: not answer.isdisjoint(("FINISHED", "RUNNING_MODAL", "INTERFACE")), lambda answer: Fault.of(Reason.ANSWERED, request, member=", ".join(sorted(answer)))
    )


def _dotted(operator: Operation, /) -> str:
    """Returns the `bpy.ops` path of an operator, which the stubs type as a plain function."""
    identifier: str = operator.idname_py()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    return identifier


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Called", "Command", "Opened", "Operation", "Pairs", "Placement", "Setting", "called", "filled", "idname"]
