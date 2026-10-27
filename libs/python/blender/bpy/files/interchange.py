"""File handlers, export references, units, and collection exporters."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from enum import Enum
from functools import partial
from itertools import starmap
from pathlib import Path
from typing import Final, Literal, TYPE_CHECKING

import bpy
from bpy_extras.io_utils import path_reference, path_reference_mode, poll_file_object_drop  # type: ignore[attr-defined]  # ty: ignore[unresolved-import]
from expression import Error, identity, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Operator
from blender.bpy.data.ids.ownership import retained
from blender.bpy.data.rna.descriptors import assigned, definition, Descriptor, typed, Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.stores import files_copied

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Drop = Callable[[bpy.types.Context], bool] | frozenset[rna_enums.SpaceTypeItems]
type HandlerRows[I: bpy.types.PropertyGroup, E: bpy.types.PropertyGroup] = Importing[I] | Exporting[E] | Bidirectional[I, E]


class SceneFormat(Enum):
    """Scene export formats valued by export operator."""

    ALEMBIC = bpy.ops.wm.alembic_export
    USD = bpy.ops.wm.usd_export


# --- [CONSTANTS] ------------------------------------------------------------------------

EXPORTER_NAME: Final = Descriptor.declared(bpy.types.CollectionExport, "name", bpy.types.StringProperty)
_METRIC: Final[str] = bpy.utils.units.systems.METRIC  # type: ignore[attr-defined]
_LENGTH: Final[str] = bpy.utils.units.categories.LENGTH  # type: ignore[attr-defined]

# --- [MODELS] ---------------------------------------------------------------------------


class Importing[I: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Import operator with the file extensions its handler lists and drop poll."""

    operator: Operator[I]
    extensions: tuple[str, ...]
    drop: Drop = poll_file_object_drop


class Exporting[E: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Export operator with the file extensions its handler lists."""

    operator: Operator[E]
    extensions: tuple[str, ...]


class Bidirectional[I: bpy.types.PropertyGroup, E: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Import and export operators sharing one file handler, extensions from import."""

    importing: Importing[I]
    exporting: Exporting[E]


class Interchange[I: bpy.types.PropertyGroup, E: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """File handler registered with its import and export operators."""

    key: Key
    title: str
    rows: HandlerRows[I, E]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns operators and file handler class."""

        def dropped(drop: Drop, context: bpy.types.Context) -> bool:
            match drop:
                case frozenset(spaces):
                    return Option.of_optional(context.area).filter(lambda area: area.type in spaces).is_some()
                case _:
                    return drop(context)

        importing: Option[Importing[I]]
        exporting: Option[Exporting[E]]
        match self.rows:
            case Bidirectional(importing=both, exporting=other):
                importing, exporting, extensions = Some(both), Some(other), both.extensions
            case Importing() as row:
                importing, exporting, extensions = Some(row), Nothing, row.extensions
            case Exporting() as row:
                importing, exporting, extensions = Nothing, Some(row), row.extensions

        def named(key: Option[Key]) -> Outcome[Option[str]]:
            return key.map(lambda held: identity.name(bpy.types.Operator, held).map(Some)).default_value(Ok(Nothing))

        def built(parts: tuple[Option[str], Option[str], str]) -> Acquisitions:
            import_name, export_name, name = parts
            kind = type(
                name,
                (bpy.types.FileHandler,),
                {
                    "bl_idname": name,
                    "bl_label": self.title,
                    "bl_file_extensions": ";".join(extensions),
                    **import_name.map(lambda identifier: {"bl_import_operator": identifier}).default_value({}),
                    **export_name.map(lambda identifier: {"bl_export_operator": identifier}).default_value({}),
                    **importing.map(lambda row: {"poll_drop": classmethod(lambda _cls, context: dropped(row.drop, context))}).default_value({}),
                },
            )
            return (*importing.map(lambda row: row.operator).to_list(), *exporting.map(lambda row: row.operator).to_list(), (Band.INTERFACE, kind))

        return paired(named(importing.map(lambda row: row.operator.key)), named(exporting.map(lambda row: row.operator.key)), identity.name(bpy.types.FileHandler, self.key)).map(built)


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
    values: Mapping[str, Value] = msgspec.field(default_factory=dict)


# --- [OPERATIONS] -----------------------------------------------------------------------


def scale(scene: bpy.types.Scene, source: str) -> Outcome[float]:
    """Parses length text into scene units."""
    return (
        catch(bpy.utils.units.to_value, exception=ValueError)(_METRIC, _LENGTH, source)
        .map(lambda meters: meters / scene.unit_settings.scale_length)
        .map_error(partial(Fault.of, RecordReason.UNREADABLE, source))
    )


def _exported(exporters: bpy.types.CollectionExports, handler: str, name: str) -> bpy.types.CollectionExport:
    """Returns a new exporter of a live file handler under name, the stubs naming stock handlers alone."""
    return exporters.new(type=handler, name=name)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]


_NEW: Final[Callable[[bpy.types.CollectionExports, str, str], Result[object, TypeError | RuntimeError | ReferenceError]]] = catch(
    catch(catch(_exported, exception=ReferenceError), exception=RuntimeError), exception=TypeError
)


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
            def outside(choices: bpy.types.EnumProperty) -> Faults:
                return Fault.of(RecordReason.OUTSIDE, request, handler, member=", ".join(item.identifier for item in choices.enum_items))

            def refused(error: TypeError | RuntimeError | ReferenceError) -> Faults:
                match error:
                    case TypeError():
                        return (
                            typed(definition(bpy.types.CollectionExports).functions["new"].parameters["type"], bpy.types.EnumProperty, "bpy.types.CollectionExports.new.type")
                            .map(outside)
                            .default_with(identity)
                        )
                    case _:
                        return Fault.of(BlenderReason.REFUSED, request, error)

            def tracked(item: bpy.types.CollectionExport) -> bpy.types.CollectionExport:
                cleanup.callback(held.remove, item)
                return item

            return _NEW(held, handler, name).map_error(refused).bind(lambda item: typed(item, bpy.types.CollectionExport, request)).map(tracked)

        def placed(name: str, exporter: Exporter) -> Outcome[None]:
            def filled(item: bpy.types.CollectionExport) -> Outcome[None]:
                return paired(
                    assigned(item, {"filepath": exporter.path}),
                    Option
                    .of_optional(item.export_properties)
                    .map(lambda properties: assigned(properties, exporter.values))
                    .default_with(lambda: Error(Fault.of(BlenderReason.ABSENT, repr(item), member="export_properties"))),
                ).map(lambda _: None)

            found = Option.of_optional(existing.get(name))
            return Descriptor.validated(EXPORTER_NAME.prop, name).bind(lambda _: found.map(filled).default_with(lambda: made(name, exporter.handler).bind(filled)))

        def completed(_: tuple[None, ...]) -> Outcome[None]:
            return retained(
                accumulated(catch(held.remove, exception=RuntimeError)(item).map_error(partial(Fault.of, BlenderReason.REFUSED, request)) for name, item in existing.items() if name not in exporters),
                cleanup,
            ).map(lambda _: None)

        return accumulated(starmap(placed, exporters.items())).bind(completed)


def exported(scene: bpy.types.Scene, form: SceneFormat, collection: bpy.types.Collection, path: Path) -> Outcome[Path]:
    """Exports collection as Alembic or animated USD."""
    match form:
        case SceneFormat.ALEMBIC:
            properties: dict[str, object] = {"selected": False, "as_background_job": False}
        case SceneFormat.USD:
            properties = {"export_animation": True, "convert_scene_units": "CUSTOM", "meters_per_unit": scene.unit_settings.scale_length}
    return called(form.value, {"filepath": str(path), "collection": collection.name} | properties, override=Some(bpy.context.temp_override(scene=scene))).map(lambda _: path)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["EXPORTER_NAME", "Bidirectional", "Drop", "Exporter", "Exporting", "HandlerRows", "Importing", "Interchange", "References", "SceneFormat", "exported", "reconciled", "scale"]
