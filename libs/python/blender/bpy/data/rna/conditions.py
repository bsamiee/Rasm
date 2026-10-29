"""Requirements over context, collections, jobs, and extensions, and owner scopes: the scenes, view layers, or objects one edit reaches, chosen from context."""

from abc import abstractmethod
from collections.abc import Mapping
from typing import ClassVar, Final, override, Protocol, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.mirrors import case_choices
from blender.bpy.data.rna.paths import root_in, rooted, Struct
from blender.records.faults import accumulated, Fault, Outcome, Reason
from blender.records.fields import Case

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import WmJobTypeItems

# --- [TYPES] ----------------------------------------------------------------------------

type _Poll = classmethod[type, [bpy.types.Context], bool]
type Scope = Scenes | ViewLayers | Selection | Objects


class Scoped[O: Struct = Struct](Protocol):
    """Context-selected owners of one struct type."""

    @property
    def owner_type(self) -> type[O]:
        """Owner struct type."""
        ...

    def owners(self, context: bpy.types.Context, /) -> Outcome[tuple[O, ...]]:
        """Returns selected owners in stable order."""
        ...


class ScopeForm(Protocol):
    """Operator form declaring `scope_choice` and `SCOPE_COLLECTION`."""

    scope: str
    collection: str


# --- [CONSTANTS] ------------------------------------------------------------------------

SCOPE_COLLECTION: Final = bpy.props.StringProperty(name="Collection", description="Collection whose objects a collection scope reaches")

# --- [MODELS] ---------------------------------------------------------------------------
# --- [CONDITIONS]


class Condition(msgspec.Struct, frozen=True, kw_only=True):
    """Requirement holding over context and an optional item, composed by `&`, `|` and `~`, its message the phrase a requirement fault states."""

    @abstractmethod
    def holds(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> bool:
        """Whether requirement holds over context, item supplying the owner first."""

    @property
    @abstractmethod
    def message(self) -> str:
        """Requirement phrase over the case fields."""

    @property
    def opposed(self) -> str:
        """Phrase of the negated requirement."""
        return f"opposite of {self.message}"

    def check(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> Outcome[None]:
        """Returns unmet fault carrying the requirement sentence."""
        return Ok(None).filter_with(lambda _: self.holds(context, item), lambda _: Fault.of(Reason.REQUIRED, self.message))

    def __and__(self, other: "Condition") -> "AllOf":
        """Conjunction of this and other requirement."""
        return AllOf(conditions=(self, other))

    def __or__(self, other: "Condition") -> "AnyOf":
        """Disjunction of this and other requirement."""
        return AnyOf(conditions=(self, other))

    def __invert__(self) -> "Condition":
        """Negation of this requirement."""
        return Not(condition=self)


class Setting(Condition, frozen=True, kw_only=True):
    """Property holding one of accepted values."""

    descriptor: Descriptor
    accepted: tuple[Value, ...] = (True,)

    @override
    def holds(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> bool:
        return self.descriptor.owner_of(context, item).map(self.descriptor.read).filter(lambda held: any(self.descriptor.equal(held, value) for value in self.accepted)).is_some()

    @property
    @override
    def message(self) -> str:
        return f"{self.descriptor.prop.name} {self.shown}"

    @property
    @override
    def opposed(self) -> str:
        return f"{self.descriptor.prop.name} other than {self.shown}"

    @property
    def shown(self) -> str:
        """Accepted values as display text."""
        return " or ".join(map(self.descriptor.shown, self.accepted))


class Present(Condition, frozen=True, kw_only=True):
    """Pointer holding target."""

    descriptor: Descriptor[bpy.types.PointerProperty]

    @override
    def holds(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> bool:
        return self.descriptor.owner_of(context, item).bind(lambda owner: Option.of_optional(getattr(owner, self.descriptor.prop.identifier))).is_some()

    @property
    @override
    def message(self) -> str:
        return self.descriptor.prop.name

    @property
    @override
    def opposed(self) -> str:
        return f"no {self.descriptor.prop.name}"


class Not(Condition, frozen=True, kw_only=True):
    """Negation of another requirement."""

    condition: Condition

    @override
    def holds(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> bool:
        return not self.condition.holds(context, item)

    @property
    @override
    def message(self) -> str:
        return self.condition.opposed


class Count(Condition, frozen=True, kw_only=True):
    """Collection with bounded count of `where` matches."""

    collection: Descriptor[bpy.types.CollectionProperty]
    least: int = 1
    most: Option[int] = Nothing
    where: Option[Condition] = Nothing

    @override
    def holds(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> bool:
        return (
            self.collection
            .owner_of(context, item)
            .map(lambda owner: sum(self.where.map(lambda where: where.holds(context, Some(each))).default_value(True) for each in getattr(owner, self.collection.prop.identifier)))
            .filter(lambda size: self.least <= size and all(size <= bound for bound in self.most.to_list()))
            .is_some()
        )

    @property
    @override
    def message(self) -> str:
        items = self.where.map(lambda where: f"{self.collection.prop.name} ({where.message})").default_value(self.collection.prop.name)
        return self.most.map(lambda most: f"{self.least} to {most} {items}").default_value(f"at least {self.least} {items}")


class Job(Condition, frozen=True, kw_only=True):
    """Job kind in required running state."""

    kind: "WmJobTypeItems"
    running: bool = False

    @override
    def holds(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> bool:
        return bpy.app.is_job_running(self.kind) == self.running

    @property
    @override
    def message(self) -> str:
        return f"{'' if self.running else 'no '}running {self.kind} job"

    @override
    def __invert__(self) -> "Job":
        return msgspec.structs.replace(self, running=not self.running)


class Extension(Condition, frozen=True, kw_only=True):
    """Enabled extension or add-on module."""

    module: str

    @override
    def holds(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> bool:
        return root_in(bpy.types.Preferences, context).map(lambda preferences: self.module in preferences.addons).default_value(False)

    @property
    @override
    def message(self) -> str:
        return f"enabled {self.module} extension"


class AnyOf(Condition, frozen=True, kw_only=True):
    """Disjunction of requirements."""

    conditions: tuple[Condition, ...]

    @override
    def holds(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> bool:
        return any(member.holds(context, item) for member in self.conditions)

    @property
    @override
    def message(self) -> str:
        return " or ".join(member.message for member in self.conditions)

    @override
    def __or__(self, other: Condition) -> "AnyOf":
        return AnyOf(conditions=(*self.conditions, other))


class AllOf(Condition, frozen=True, kw_only=True):
    """Conjunction of requirements, checked member by member."""

    conditions: tuple[Condition, ...]

    @override
    def holds(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> bool:
        return all(member.holds(context, item) for member in self.conditions)

    @property
    @override
    def message(self) -> str:
        return " and ".join(member.message for member in self.conditions)

    @override
    def check(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> Outcome[None]:
        return accumulated(member.check(context, item) for member in self.conditions).map(lambda _: None)

    @override
    def __and__(self, other: Condition) -> "AllOf":
        return AllOf(conditions=(*self.conditions, other))


# --- [SCOPES]


class Scenes(Case, frozen=True, kw_only=True):
    """Named or every non-runtime scene in file order."""

    owner_type: ClassVar[type[bpy.types.Scene]] = bpy.types.Scene
    names: tuple[str, ...] | None = None

    def owners(self, _context: bpy.types.Context) -> Outcome[tuple[bpy.types.Scene, ...]]:
        """Returns scenes, absent fault per missing name."""
        return (
            Option
            .of_optional(self.names)
            .map(lambda names: accumulated(Option.of_optional(bpy.data.scenes.get(name)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(bpy.data.scenes), member=name)) for name in names))
            .default_with(lambda: Ok(tuple(scene for scene in bpy.data.scenes if not scene.is_runtime_data)))
        )


class ViewLayers(Case, frozen=True, kw_only=True):
    """View layers of selected scenes."""

    owner_type: ClassVar[type[bpy.types.ViewLayer]] = bpy.types.ViewLayer
    scenes: tuple[str, ...] | None = None

    def owners(self, context: bpy.types.Context) -> Outcome[tuple[bpy.types.ViewLayer, ...]]:
        """Returns view layers in scene and layer order."""
        return Scenes(names=self.scenes).owners(context).map(lambda owners: tuple(layer for scene in owners for layer in scene.view_layers))


class Selection(Case, frozen=True, kw_only=True):
    """Objects selected in context view layer."""

    owner_type: ClassVar[type[bpy.types.Object]] = bpy.types.Object

    @staticmethod
    def owners(context: bpy.types.Context) -> Outcome[tuple[bpy.types.Object, ...]]:
        """Returns selected objects in view layer order."""
        return rooted(bpy.types.ViewLayer, context).map(lambda layer: tuple(layer.objects.selected))


class Objects(Case, frozen=True, kw_only=True):
    """Objects of named collection and its descendants."""

    owner_type: ClassVar[type[bpy.types.Object]] = bpy.types.Object
    collection: str

    def owners(self, _context: bpy.types.Context) -> Outcome[tuple[bpy.types.Object, ...]]:
        """Returns collection objects."""
        return (
            Option
            .of_optional(bpy.data.collections.get(self.collection))
            .to_result_with(lambda: Fault.of(Reason.ABSENT, repr(bpy.data.collections), member=self.collection))
            .map(lambda collection: tuple(collection.all_objects))
        )


# --- [TABLES] ---------------------------------------------------------------------------

SCOPES: Final = case_choices(Scope)

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [CONDITIONS]


def admits(condition: Option[Condition], context: bpy.types.Context, item: Option[Struct] = Nothing, /) -> bool:
    """Whether a stated condition holds over the context and item, every context passing while none is stated."""
    return condition.map(lambda required: required.holds(context, item)).default_value(True)


def polled(condition: Option[Condition]) -> Mapping[str, _Poll]:
    """Returns the `poll` class member testing condition, empty when absent.

    An operator states every unmet requirement through `poll_message_set`, the reason its disabled button shows.
    """

    def poll(cls: type, context: bpy.types.Context) -> bool:
        faults = condition.bind(lambda required: required.check(context).swap().to_option())
        for failures in faults.to_list():
            if issubclass(cls, bpy.types.Operator):
                cls.poll_message_set(lambda: " ".join(fault.message for fault in failures))
        return faults.is_none()

    return {"poll": classmethod(poll)} if condition.is_some() else {}


# --- [SCOPES]


def scope_choice(default: type[Scenes | ViewLayers | Selection | Objects]) -> bpy.props._PropertyDeferred:
    """Returns the scope enum an operator form declares beside `SCOPE_COLLECTION`, defaulting to one case."""
    return bpy.props.EnumProperty(name="Scope", description="Owners the operator reaches", items=SCOPES, default=default.__name__)


def chosen(form: ScopeForm) -> Scope:
    """Returns the scope a form's choice names, a collection's objects by its name."""
    held: Scope = msgspec.convert({"kind": form.scope, "collection": form.collection}, Scope)
    return held


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "SCOPES",
    "SCOPE_COLLECTION",
    "AllOf",
    "AnyOf",
    "Condition",
    "Count",
    "Extension",
    "Job",
    "Not",
    "Objects",
    "Present",
    "Scenes",
    "Scope",
    "ScopeForm",
    "Scoped",
    "Selection",
    "Setting",
    "ViewLayers",
    "admits",
    "chosen",
    "polled",
    "scope_choice",
]
