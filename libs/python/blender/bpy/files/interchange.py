"""File handlers, export references, units, collection exporters, and exports through the host exporters."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from functools import partial
from itertools import starmap
from pathlib import Path
from types import MappingProxyType
from typing import Final, Literal, TYPE_CHECKING

import bpy
from bpy_extras.io_utils import path_reference, path_reference_mode, poll_file_object_drop  # type: ignore[attr-defined]  # ty: ignore[unresolved-import]
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called, Operation, Setting
from blender.bpy.commands.operators import Operator
from blender.bpy.data.ids.ownership import retained, tracked
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason, typed
from blender.records.stores import files_copied

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [CONSTANTS] ------------------------------------------------------------------------

_EXPORTER_NAME: Final = Descriptor.declared(bpy.types.CollectionExport, ("name",), bpy.types.StringProperty)
_REFUSALS: Final[type[TypeError | RuntimeError | ReferenceError]] = (TypeError, RuntimeError, ReferenceError)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

# --- [MODELS] ---------------------------------------------------------------------------


class Interchange[I: bpy.types.PropertyGroup, E: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """File handler registered with its import and export operators, its label derived from its key unless stated."""

    key: Key
    extensions: tuple[str, ...]
    importing: Option[Operator[I]] = Nothing
    exporting: Option[Operator[E]] = Nothing
    drop: Callable[[bpy.types.Context], bool] = poll_file_object_drop
    title: Option[str] = Nothing

    def __post_init__(self) -> None:
        """Refuses a handler naming neither an import nor an export operator."""
        if self.importing.is_none() and self.exporting.is_none():
            raise TypeError(f"{self.key!r} names no import or export operator")

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns operators and file handler class."""

        def named(operator: Option[Operator[I]] | Option[Operator[E]]) -> Outcome[Option[str]]:
            return operator.map(lambda held: identity.name(bpy.types.Operator, held.key).map(Some)).default_value(Ok(Nothing))

        def built(parts: tuple[Option[str], Option[str], str]) -> Acquisitions:
            import_name, export_name, name = parts
            kind = type(
                name,
                (bpy.types.FileHandler,),
                {
                    "bl_idname": name,
                    "bl_label": self.title.default_with(lambda: bpy.path.display_name(self.key)),
                    "bl_file_extensions": ";".join(self.extensions),
                    **import_name.map(lambda identifier: {"bl_import_operator": identifier, "poll_drop": classmethod(lambda _cls, context: self.drop(context))}).default_value({}),
                    **export_name.map(lambda identifier: {"bl_export_operator": identifier}).default_value({}),
                },
            )
            return (*self.importing.to_list(), *self.exporting.to_list(), kind)

        return paired(named(self.importing), named(self.exporting), identity.name(bpy.types.FileHandler, self.key)).map(built)


class References(msgspec.Struct, frozen=True, kw_only=True):
    """Export path mode and destination subfolder for copied files."""

    mode: Literal["AUTO", "ABSOLUTE", "RELATIVE", "MATCH", "STRIP", "COPY"] = path_reference_mode.keywords["default"]
    subfolder: str = ""

    def path(self, filepath: str, destination: Path, library: Option[bpy.types.Library] = Nothing) -> Outcome[str]:
        """Copies referenced files, returning export path."""
        copies: set[tuple[str, str]] = set()
        reference = path_reference(filepath, bpy.path.abspath("//"), str(destination), mode=self.mode, copy_subdir=self.subfolder, copy_set=copies, library=library.to_optional())
        return files_copied((Path(source), Path(target)) for source, target in copies).map(lambda _: reference)


class Exporter(msgspec.Struct, frozen=True, kw_only=True):
    """Collection exporter over a file handler `bl_idname`, a stock one or an `Interchange` row's `FileHandler` name."""

    handler: str
    path: str
    values: Mapping[str, Value] = MappingProxyType({})


# --- [OPERATIONS] -----------------------------------------------------------------------


def dropped_in(spaces: frozenset["rna_enums.SpaceTypeItems"], context: bpy.types.Context) -> bool:
    """Returns whether a drop lands in an editor of the spaces, bound through `partial` as an `Interchange` drop poll."""
    return Option.of_optional(context.area).filter(lambda area: area.type in spaces).is_some()


def length(scene: bpy.types.Scene, source: str) -> Outcome[float]:
    """Parses length text in the scene's unit system into scene units, a bare number read in the scene's length unit."""
    units = scene.unit_settings
    category: str = bpy.utils.units.categories.LENGTH  # type: ignore[attr-defined]
    return (
        catch(bpy.utils.units.to_value, exception=ValueError)(units.system, category, source, str_ref_unit=f"1 {units.length_unit}")
        .map(lambda meters: meters / units.scale_length)
        .map_error(partial(Fault.of, Reason.UNREADABLE, source))
    )


@catch(exception=_REFUSALS)
def _exported(exporters: bpy.types.CollectionExports, handler: str, name: str) -> object:
    """Returns a new exporter of a live file handler under name, the host refusing an unknown handler, the stubs naming stock handlers alone."""
    return exporters.new(type=handler, name=name)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]


def reconciled(collection: bpy.types.Collection, exporters: Mapping[str, Exporter]) -> Outcome[None]:
    """Reconciles collection exporters by name, removing exporters it created when any write refuses.

    Args:
        collection: Collection holding the exporters.
        exporters: Desired exporter per name.

    Returns:
        Faults per refused name, handler, and member write accumulated, then per refused removal.
    """
    held = collection.exporters
    existing = dict(held.items())
    request = f"{collection!r}.exporters"

    with ExitStack() as cleanup:

        def made(name: str, handler: str) -> Outcome[bpy.types.CollectionExport]:
            return tracked(cleanup, held.remove, _exported(held, handler, name).map_error(partial(Fault.of, Reason.REFUSED, request)).bind(partial(typed, bpy.types.CollectionExport, request)))

        def placed(name: str, exporter: Exporter) -> Outcome[None]:
            def filled(item: bpy.types.CollectionExport) -> Outcome[None]:
                return paired(
                    assigned(item, {("filepath",): exporter.path}),
                    Option
                    .of_optional(item.export_properties)
                    .map(lambda properties: assigned(properties, {(key,): value for key, value in exporter.values.items()}))
                    .default_with(lambda: Error(Fault.of(Reason.ABSENT, repr(item), member="export_properties"))),
                ).map(lambda _: None)

            found = Option.of_optional(existing.get(name))
            return Descriptor.validated(_EXPORTER_NAME.prop, name).bind(lambda _: found.map(filled).default_with(lambda: made(name, exporter.handler).bind(filled)))

        def completed(_: tuple[None, ...]) -> Outcome[None]:
            return retained(
                accumulated(catch(held.remove, exception=RuntimeError)(item).map_error(partial(Fault.of, Reason.REFUSED, request)) for name, item in existing.items() if name not in exporters), cleanup
            ).map(lambda _: None)

        return accumulated(starmap(placed, exporters.items())).bind(completed)


def exported(operator: Operation, collection: bpy.types.Collection, path: Path, /, *, scene: bpy.types.Scene, view_layer: bpy.types.ViewLayer, **params: Setting) -> Outcome[Path]:
    """Exports collection through an exporter operator and its keywords under the scene and view layer's context.

    Args:
        operator: Exporter taking `filepath` and `collection` (`bpy.ops.wm.alembic_export`, `bpy.ops.wm.usd_export`, ...).
        collection: Collection to export.
        path: File to write.
        scene: Scene the export reads.
        view_layer: View layer the export evaluates.
        **params: Operator keywords besides `filepath` and `collection`.

    Returns:
        Written path.
    """
    return called(operator, {"filepath": str(path), "collection": collection.name, **params}, override=Some(bpy.context.temp_override(scene=scene, view_layer=view_layer))).map(lambda _: path)


def configured(collection: bpy.types.Collection, *, scene: bpy.types.Scene, view_layer: bpy.types.ViewLayer, exporter: Option[str] = Nothing) -> Outcome[None]:
    """Runs the collection's configured exporters through `collection.export_all`, or the one named through `collection.exporter_export`.

    Args:
        collection: Collection holding the exporters.
        scene: Scene the exports read.
        view_layer: View layer the exports evaluate.
        exporter: Name of the one exporter to run.

    Returns:
        Ok(None), absent fault for a name the collection holds no exporter under, operator faults.
    """
    override = Some(bpy.context.temp_override(collection=collection, scene=scene, view_layer=view_layer))
    return (
        exporter
        .map(
            lambda name: (
                Ok(collection.exporters.find(name))
                .filter_with(lambda index: index >= 0, lambda _: Fault.of(Reason.ABSENT, f"{collection!r}.exporters", member=name))
                .bind(lambda index: called(bpy.ops.collection.exporter_export, {"index": index}, override=override))
            )
        )
        .default_with(lambda: called(bpy.ops.collection.export_all, {}, override=override))
        .map(lambda _: None)
    )


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Exporter", "Interchange", "References", "configured", "dropped_in", "exported", "length", "reconciled"]
