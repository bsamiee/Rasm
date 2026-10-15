"""Declared requirements over native context, collections, jobs, and output state."""

from collections.abc import Callable, Mapping
import os
from pathlib import Path
import shutil
from typing import TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.faults import BlenderResult, BlenderUnmetFault
from blender.imaging.color.authoring import DynamicRange
from blender.imaging.color.processors import DisplayView
from blender.imaging.faults import accumulated

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import WmJobTypeItems

# --- [TYPES] ----------------------------------------------------------------------------

type Condition = Setting | Present | Not | Count | Job | Extension | Folder | HdrWindow | AnyOf | AllOf

# --- [MODELS] ---------------------------------------------------------------------------


class Setting(msgspec.Struct, frozen=True, kw_only=True):
    """Native property holding one of its accepted values."""

    descriptor: Descriptor
    accepted: tuple[Value, ...] = (True,)

    @property
    def message(self) -> str:
        """Property name and required values."""
        return f"Requires {self.descriptor.prop.name}: {', '.join(map(self.descriptor.shown, self.accepted))}"


class Present(msgspec.Struct, frozen=True, kw_only=True):
    """Native pointer holding a target."""

    descriptor: Descriptor[bpy.types.PointerProperty]

    @property
    def message(self) -> str:
        """Required pointer's native name."""
        return f"Requires {self.descriptor.prop.name}"


class Not(msgspec.Struct, frozen=True, kw_only=True):
    """Negation of another requirement."""

    condition: Condition

    @property
    def message(self) -> str:
        """Native property, pointer, or requirement being excluded."""
        match self.condition:
            case Setting(descriptor=descriptor, accepted=accepted):
                return f"Requires {descriptor.prop.name} other than {', '.join(map(descriptor.shown, accepted))}"
            case Present(descriptor=descriptor):
                return f"Requires no {descriptor.prop.name}"
            case _:
                return f"Requires the opposite of: {self.condition.message}"


class Count(msgspec.Struct, frozen=True, kw_only=True):
    """Collection containing a bounded number of admitted items."""

    collection: Descriptor[bpy.types.CollectionProperty]
    least: int = 1
    most: Option[int] = Nothing
    where: Option[Condition] = Nothing

    @property
    def message(self) -> str:
        """Required count and the condition selecting its items."""
        items = self.where.map(lambda where: f"{self.collection.prop.name} ({where.message})").default_value(self.collection.prop.name)
        return self.most.map(lambda most: f"Requires {self.least} to {most} {items}").default_value(f"Requires at least {self.least} {items}")


class Job(msgspec.Struct, frozen=True, kw_only=True):
    """Native job required to be running or finished."""

    kind: "WmJobTypeItems"
    running: bool = False

    @property
    def message(self) -> str:
        """Required job state."""
        return f"Requires a {self.kind} job running" if self.running else f"Requires no {self.kind} job running"


class Extension(msgspec.Struct, frozen=True, kw_only=True):
    """Enabled extension or add-on module."""

    module: str

    @property
    def message(self) -> str:
        """Required module name."""
        return f"Requires the {self.module} extension"


class Folder(msgspec.Struct, frozen=True, kw_only=True):
    """Writable output folder with enough free space."""

    path: Descriptor[bpy.types.StringProperty]
    least: int

    @property
    def message(self) -> str:
        """Required free space and native path setting."""
        return f"Requires a writable folder with {self.least // 2**20} MiB free for {self.path.prop.name}"


class HdrWindow(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """HDR window showing a frame formed in an absolute PQ display space."""

    formed: Callable[[bpy.types.Context], Option[DisplayView]]

    @property
    def message(self) -> str:
        """Display formation and window requirement."""
        return "Requires a frame formed on a PQ display in a window with HDR color"


class AnyOf(msgspec.Struct, frozen=True, kw_only=True):
    """Requirements of which at least one must hold."""

    conditions: tuple[Condition, ...]

    @property
    def message(self) -> str:
        """Alternatives joined in their declaration order."""
        return " or ".join(condition.message for condition in self.conditions)


class AllOf(msgspec.Struct, frozen=True, kw_only=True):
    """Requirements of which every member must hold."""

    conditions: tuple[Condition, ...]

    @property
    def message(self) -> str:
        """Requirements joined in their declaration order."""
        return " and ".join(condition.message for condition in self.conditions)


# --- [OPERATIONS] -----------------------------------------------------------------------


def existing(path: Path) -> Path:
    """Returns the deepest existing folder containing an absolute file or folder path."""
    return next(folder for folder in (path, *path.parents) if folder.is_dir())


def holds(condition: Condition, context: bpy.types.Context, item: "Option[bpy.types.bpy_struct[object]]" = Nothing) -> bool:
    """Returns whether a requirement holds, rooting nested collection conditions at an item."""
    match condition:
        case Setting(descriptor=descriptor, accepted=accepted):
            return descriptor.owner_of(context, item).map(descriptor.read).filter(lambda held: any(descriptor.equal(held, value) for value in accepted)).is_some()
        case Present(descriptor=descriptor):
            return descriptor.owner_of(context, item).bind(lambda owner: Option.of_optional(getattr(owner, descriptor.prop.identifier))).is_some()
        case Not(condition=inner):
            return not holds(inner, context, item)
        case Count(collection=collection, least=least, most=most, where=where):
            return (
                collection
                .owner_of(context, item)
                .map(lambda owner: sum(1 for each in getattr(owner, collection.prop.identifier) if all(holds(test, context, Some(each)) for test in where.to_list())))
                .filter(lambda size: least <= size and all(size <= bound for bound in most.to_list()))
                .is_some()
            )
        case Job(kind=kind, running=running):
            return bpy.app.is_job_running(kind) == running
        case Extension(module=module):
            return module in context.preferences.addons  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        case Folder(path=path, least=least):
            return (
                path
                .owner_of(context, item)
                .map(lambda owner: existing(Path(bpy.path.abspath(os.fsdecode(getattr(owner, path.prop.identifier))))))
                .filter(lambda folder: os.access(folder, os.W_OK) and shutil.disk_usage(folder).free >= least)
                .is_some()
            )
        case HdrWindow(formed=formed):
            return (
                formed(context).bind(lambda view: view.colorspace(PyOpenColorIO.GetCurrentConfig())).filter(DynamicRange.absolute).is_some()
                and Option.of_optional(context.window).filter(lambda window: window.support_hdr_color).is_some()
            )
        case AnyOf(conditions=members):
            return any(holds(member, context, item) for member in members)
        case AllOf(conditions=members):
            return all(holds(member, context, item) for member in members)


def check(condition: Condition, context: bpy.types.Context, item: "Option[bpy.types.bpy_struct[object]]" = Nothing) -> BlenderResult[None]:
    """Returns success or every unmet member of a conjunction in declaration order."""
    match condition:
        case AllOf(conditions=members):
            return accumulated(check(member, context, item) for member in members).map(lambda _: None)
        case Setting() | Present() | Not() | Count() | Job() | Extension() | Folder() | HdrWindow() | AnyOf():
            return Ok(None) if holds(condition, context, item) else Error((BlenderUnmetFault(requirement=condition),))


def polled(condition: Option[Condition]) -> Mapping[str, object]:
    """Returns the native class poll member testing a present condition."""
    return condition.map(lambda held: {"poll": classmethod(lambda _cls, context: holds(held, context))}).default_value({})


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AllOf", "AnyOf", "Condition", "Count", "Extension", "Folder", "HdrWindow", "Job", "Not", "Present", "Setting", "check", "existing", "holds", "polled"]
