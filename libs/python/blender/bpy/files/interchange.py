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
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Open, Operator, Save
from blender.bpy.data.ids.ownership import committed
from blender.bpy.data.rna.descriptors import assigned, Descriptor, Value
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderEditFault, BlenderResult
from blender.bpy.files.blends import files_copied
from blender.imaging.faults import Access, accumulated, ImagingFileFault, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Drop = Callable[[bpy.types.Context], bool] | frozenset[rna_enums.SpaceTypeItems]
type HandlerRows[I: msgspec.Struct, E: msgspec.Struct] = Importing[I] | Exporting[E] | Bidirectional[I, E]


class SceneFormat(Enum):
    """Scene export formats valued by export operator."""

    ALEMBIC = bpy.ops.wm.alembic_export
    USD = bpy.ops.wm.usd_export


# --- [CONSTANTS] ------------------------------------------------------------------------

EXPORTER_NAME: Final = Descriptor.declared(bpy.types.CollectionExport, "name", bpy.types.StringProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class Importing[I: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Import operator with drop poll."""

    operator: Operator[I]
    record: I
    drop: Drop = poll_file_object_drop


class Exporting[E: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Export operator with record its file formats read."""

    operator: Operator[E]
    record: E


class Bidirectional[I: msgspec.Struct, E: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Import and export operators sharing one file handler, extensions from import."""

    importing: Importing[I]
    exporting: Exporting[E]


class Interchange[I: msgspec.Struct, E: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """File handler registered with its import and export operators."""

    key: Key
    title: str
    rows: HandlerRows[I, E]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns operators and file handler class, absent fault per operator lacking file invoke."""

        def formats[T: msgspec.Struct](operator: Operator[T], record: T) -> BlenderResult[tuple[str, ...]]:
            match operator.invoke:
                case Option(tag="some", some=Open(extensions=extensions) | Save(extensions=extensions)):
                    return Ok(extensions(record))
                case _:
                    return Error((BlenderAbsentFault(owner=operator.key, key="invoke"),))

        def dropped(drop: Drop) -> Callable[[bpy.types.Context], bool]:
            match drop:
                case frozenset(spaces):
                    return lambda context: Option.of_optional(context.area).filter(lambda area: area.type in spaces).is_some()
                case _:
                    return drop

        importing: Option[Importing[I]]
        exporting: Option[Exporting[E]]
        match self.rows:
            case Bidirectional(importing=both, exporting=other):
                importing, exporting, extensions = Some(both), Some(other), formats(both.operator, both.record)
            case Importing(operator=operator, record=record) as row:
                importing, exporting, extensions = Some(row), Nothing, formats(operator, record)
            case Exporting(operator=operator, record=record) as row:
                importing, exporting, extensions = Nothing, Some(row), formats(operator, record)

        def named(key: Option[Key]) -> BlenderResult[Option[str]]:
            return key.map(lambda held: identity.name(Name.OPERATOR, held).map(Some)).default_value(Ok(Nothing))

        def built(parts: tuple[Option[str], Option[str], str, tuple[str, ...]]) -> Acquisitions:
            import_name, export_name, name, suffixes = parts
            kind = type(
                name,
                (bpy.types.FileHandler,),
                {
                    "bl_idname": name,
                    "bl_label": self.title,
                    "bl_file_extensions": ";".join(suffixes),
                    **import_name.map(lambda identifier: {"bl_import_operator": identifier}).default_value({}),
                    **export_name.map(lambda identifier: {"bl_export_operator": identifier}).default_value({}),
                    **importing.map(lambda row: dropped(row.drop)).map(lambda polled: {"poll_drop": classmethod(lambda _cls, context: polled(context))}).default_value({}),
                },
            )
            return (*importing.map(lambda row: row.operator).to_list(), *exporting.map(lambda row: row.operator).to_list(), (Band.INTERFACE, kind))

        return paired(named(importing.map(lambda row: row.operator.key)), named(exporting.map(lambda row: row.operator.key)), identity.name(Name.FILE_HANDLER, self.key), extensions).map(built)


class References(msgspec.Struct, frozen=True, kw_only=True):
    """Export path mode and destination subfolder for copied files."""

    mode: Literal["AUTO", "ABSOLUTE", "RELATIVE", "MATCH", "STRIP", "COPY"] = path_reference_mode.keywords["default"]
    subfolder: str = ""

    def path(self, filepath: str, destination: Path, library: Option[bpy.types.Library] = Nothing) -> BlenderResult[str]:
        """Copies referenced files, returning export path."""
        copies: set[tuple[str, str]] = set()
        reference = path_reference(filepath, bpy.path.abspath("//"), str(destination), mode=self.mode, copy_subdir=self.subfolder, copy_set=copies, library=library.to_optional())
        return files_copied((Path(source), Path(target)) for source, target in copies).map(lambda _: reference)


class Exporter(msgspec.Struct, frozen=True, kw_only=True):
    """Collection exporter over a file handler `bl_idname`, a stock one or an `Interchange` row's `Name.FILE_HANDLER` name."""

    handler: str
    path: str
    values: Mapping[str, Value] = msgspec.field(default_factory=dict)


# --- [OPERATIONS] -----------------------------------------------------------------------


def scale(scene: bpy.types.Scene, source: str) -> BlenderResult[float]:
    """Parses length text into scene units."""
    return (
        catch(lambda: bpy.utils.units.to_value(bpy.utils.units.systems.METRIC, bpy.utils.units.categories.LENGTH, source), exception=ValueError)()  # type: ignore[attr-defined]
        .map(lambda meters: meters / scene.unit_settings.scale_length)
        .map_error(lambda error: (ImagingFileFault(access=Access.READ, request=source, error=error),))
    )


def reconciled(collection: bpy.types.Collection, exporters: Mapping[str, Exporter]) -> BlenderResult[None]:
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

        def made(name: str, handler: str) -> BlenderResult[bpy.types.CollectionExport]:
            try:
                item = held.new(type=handler, name=name)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            except TypeError:
                return Error((
                    BlenderChoiceFault(
                        path=request,
                        identifier=handler,
                        accepted=tuple(item.identifier for item in bpy.types.CollectionExports.bl_rna.functions["new"].parameters["type"].enum_items),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    ),
                ))
            except (RuntimeError, ReferenceError) as error:
                return Error((BlenderEditFault(request=request, error=error),))
            cleanup.callback(held.remove, item)
            return Ok(item)

        def placed(name: str, exporter: Exporter) -> BlenderResult[None]:
            def filled(item: bpy.types.CollectionExport) -> BlenderResult[None]:
                return paired(
                    assigned(item, {"filepath": exporter.path}),
                    Option
                    .of_optional(item.export_properties)
                    .map(lambda properties: assigned(properties, exporter.values))
                    .default_with(lambda: Error((BlenderAbsentFault(owner=repr(item), key="export_properties"),))),
                ).map(lambda _: None)

            found = Option.of_optional(existing.get(name))
            return Descriptor.validated(EXPORTER_NAME.prop, name).bind(lambda _: found.map(filled).default_with(lambda: made(name, exporter.handler).bind(filled)))

        def completed(_: tuple[None, ...]) -> BlenderResult[None]:
            return committed(accumulated(BlenderEditFault.attempted(request, partial(held.remove, item)) for name, item in existing.items() if name not in exporters), cleanup).map(lambda _: None)

        return accumulated(starmap(placed, exporters.items())).bind(completed)


def exported(scene: bpy.types.Scene, form: SceneFormat, collection: bpy.types.Collection, path: Path) -> BlenderResult[Path]:
    """Exports collection as Alembic or animated USD."""
    match form:
        case SceneFormat.ALEMBIC:
            properties: dict[str, object] = {"selected": False, "as_background_job": False}
        case SceneFormat.USD:
            properties = {"export_animation": True, "convert_scene_units": "CUSTOM", "meters_per_unit": scene.unit_settings.scale_length}
    return called(form.value, {"filepath": str(path), "collection": collection.name} | properties, override=Some(bpy.context.temp_override(scene=scene))).map(lambda _: path)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["EXPORTER_NAME", "Bidirectional", "Drop", "Exporter", "Exporting", "HandlerRows", "Importing", "Interchange", "References", "SceneFormat", "exported", "reconciled", "scale"]
