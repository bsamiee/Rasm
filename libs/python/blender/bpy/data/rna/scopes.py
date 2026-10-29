"""Owner scopes: the scenes, view layers, or objects one edit reaches, chosen from context."""

from typing import ClassVar, Final, Protocol

import bpy
from expression import Ok, Option
import msgspec

from blender.bpy.data.rna.mirrors import case_choices
from blender.bpy.data.rna.paths import rooted, Struct
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Outcome
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

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


class Scenes(Case, frozen=True, kw_only=True):
    """Named or every non-runtime scene in file order."""

    owner_type: ClassVar[type[bpy.types.Scene]] = bpy.types.Scene
    names: tuple[str, ...] | None = None

    def owners(self, _context: bpy.types.Context) -> Outcome[tuple[bpy.types.Scene, ...]]:
        """Returns scenes, absent fault per missing name."""
        return (
            Option
            .of_optional(self.names)
            .map(lambda names: accumulated(Option.of_optional(bpy.data.scenes.get(name)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(bpy.data.scenes), member=name)) for name in names))
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
            .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(bpy.data.collections), member=self.collection))
            .map(lambda collection: tuple(collection.all_objects))
        )


# --- [TABLES] ---------------------------------------------------------------------------

SCOPES: Final = case_choices(Scope)

# --- [OPERATIONS] -----------------------------------------------------------------------


def scope_choice(default: type[Scenes | ViewLayers | Selection | Objects]) -> bpy.props._PropertyDeferred:
    """Returns the scope enum an operator form declares beside `SCOPE_COLLECTION`, defaulting to one case."""
    return bpy.props.EnumProperty(name="Scope", description="Owners the operator reaches", items=SCOPES, default=default.__name__)


def chosen(form: ScopeForm) -> Scope:
    """Returns the scope a form's choice names, a collection's objects by its name."""
    held: Scope = msgspec.convert({"kind": form.scope, "collection": form.collection}, Scope)
    return held


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["SCOPES", "SCOPE_COLLECTION", "Objects", "Scenes", "Scope", "ScopeForm", "Scoped", "Selection", "ViewLayers", "chosen", "scope_choice"]
