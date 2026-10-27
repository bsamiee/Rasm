"""Requirements over context, collections, jobs, and extensions."""

from abc import abstractmethod
from collections.abc import Mapping
from typing import override, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from blender.bpy.data.rna.descriptors import Descriptor, Struct, Value
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Outcome

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import WmJobTypeItems

# --- [TYPES] ----------------------------------------------------------------------------

type _Poll = classmethod[type, [bpy.types.Context], bool]

# --- [MODELS] ---------------------------------------------------------------------------


class Condition(msgspec.Struct, frozen=True, kw_only=True):
    """Requirement holding over context and an optional item, composed by `&`, `|` and `~`."""

    @abstractmethod
    def holds(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> bool:
        """Whether requirement holds over context, item supplying the owner first."""

    @property
    @abstractmethod
    def message(self) -> str:
        """Requirement sentence over the case fields."""

    @property
    def opposed(self) -> str:
        """Sentence of the negated requirement."""
        return f"Requires opposite of {self.message}"

    def check(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> Outcome[None]:
        """Returns unmet fault carrying the requirement sentence."""
        return Ok(None).filter_with(lambda _: self.holds(context, item), lambda _: Fault.of(BlenderReason.UNMET, self.message))

    def __and__(self, other: "Condition") -> "AllOf":
        """Conjunction of this and other requirement."""
        return AllOf(conditions=(self, other))

    def __or__(self, other: "Condition") -> "AnyOf":
        """Disjunction of this and other requirement."""
        return AnyOf(conditions=(self, other))

    def __invert__(self) -> "Not":
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
        return f"Requires {self.descriptor.prop.name} {self.shown}"

    @property
    @override
    def opposed(self) -> str:
        return f"Requires {self.descriptor.prop.name} other than {self.shown}"

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
        return f"Requires {self.descriptor.prop.name}"

    @property
    @override
    def opposed(self) -> str:
        return f"Requires no {self.descriptor.prop.name}"


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
            .map(lambda owner: sum(1 for each in getattr(owner, self.collection.prop.identifier) if all(test.holds(context, Some(each)) for test in self.where.to_list())))
            .filter(lambda size: self.least <= size and all(size <= bound for bound in self.most.to_list()))
            .is_some()
        )

    @property
    @override
    def message(self) -> str:
        items = self.where.map(lambda where: f"{self.collection.prop.name} ({where.message})").default_value(self.collection.prop.name)
        return self.most.map(lambda most: f"Requires {self.least} to {most} {items}").default_value(f"Requires at least {self.least} {items}")


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
        return f"Requires {'' if self.running else 'no '}running {self.kind} job"


class Extension(Condition, frozen=True, kw_only=True):
    """Enabled extension or add-on module."""

    module: str

    @override
    def holds(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> bool:
        preferences: bpy.types.Preferences = context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return self.module in preferences.addons

    @property
    @override
    def message(self) -> str:
        return f"Requires enabled {self.module} extension"


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


# --- [OPERATIONS] -----------------------------------------------------------------------


def polled(condition: Option[Condition]) -> Mapping[str, _Poll]:
    """Returns `poll` class member testing condition, empty when absent."""
    return condition.map(lambda held: {"poll": classmethod(lambda _cls, context: held.holds(context))}).default_value({})


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AllOf", "AnyOf", "Condition", "Count", "Extension", "Job", "Not", "Present", "Setting", "polled"]
