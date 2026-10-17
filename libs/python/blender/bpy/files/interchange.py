"""Native file handlers, export references, units, and collection export declarations."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from enum import Enum
from itertools import starmap
from pathlib import Path
from typing import Annotated, Literal, TYPE_CHECKING

import bpy
from bpy_extras.io_utils import path_reference, path_reference_mode, poll_file_object_drop  # type: ignore[attr-defined]  # ty: ignore[unresolved-import]
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Open, Operator, Save
from blender.bpy.data.ids.ownership import committed
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderEditFault, BlenderReadFault, BlenderResult
from blender.bpy.files.blends import files_copied
from blender.imaging.faults import accumulated, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Drop = Callable[[bpy.types.Context], bool] | frozenset[rna_enums.SpaceTypeItems]
type HandlerRows[I: msgspec.Struct, E: msgspec.Struct] = Importing[I] | tuple[Operator[E], E] | Bidirectional[I, E]


class SceneFormat(Enum):
    """Scene export policies valued by their native operators."""

    ALEMBIC = bpy.ops.wm.alembic_export
    USD = bpy.ops.wm.usd_export


# --- [MODELS] ---------------------------------------------------------------------------


class Importing[I: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Import operator and its native drop-target policy."""

    operator: Operator[I]
    record: I
    drop: Drop = poll_file_object_drop


class Bidirectional[I: msgspec.Struct, E: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Import and export operators sharing one file handler."""

    importing: Importing[I]
    exporter: Operator[E]


class Interchange[I: msgspec.Struct, E: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """File handler registered with its original import and export operator rows."""

    key: Key
    title: str
    rows: HandlerRows[I, E]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns nested operator rows and a handler class, or independent naming and format faults."""

        def named[T: msgspec.Struct](row: Option[Operator[T]]) -> BlenderResult[Option[str]]:
            return row.map(lambda operator: identity.name(Name.OPERATOR, operator.key).map(Some)).default_value(Ok(Nothing))

        def formats[T: msgspec.Struct](row: Operator[T], record: T) -> BlenderResult[tuple[str, ...]]:
            match row.invoke:
                case Option(tag="some", some=Open(extensions=extensions) | Save(extensions=extensions)):
                    return Ok(extensions(record))
                case _:
                    return Error((BlenderAbsentFault(owner=row.key, key="invoke"),))

        match self.rows:
            case Bidirectional(importing=importing, exporter=exporter):
                importing_row, exporting_row = Some(importing.operator), Some(exporter)
                operators: Acquisitions = (importing.operator, exporter)
                extensions = formats(importing.operator, importing.record)
                drop: Option[Drop] = Some(importing.drop)
            case Importing(operator=importer, record=record, drop=policy):
                importing_row, exporting_row = Some(importer), Nothing
                operators = (importer,)
                extensions = formats(importer, record)
                drop = Some(policy)
            case Operator() as exporter, record:
                importing_row, exporting_row = Nothing, Some(exporter)
                operators = (exporter,)
                extensions = formats(exporter, record)
                drop = Nothing

        def built(names: tuple[tuple[Option[str], Option[str]], tuple[str, tuple[str, ...]]]) -> Acquisitions:
            (import_name, export_name), (name, suffixes) = names

            kind = type(
                name,
                (bpy.types.FileHandler,),
                {
                    "bl_idname": name,
                    "bl_label": self.title,
                    "bl_file_extensions": ";".join(suffixes),
                    **import_name.map(lambda identifier: {"bl_import_operator": identifier}).default_value({}),
                    **export_name.map(lambda identifier: {"bl_export_operator": identifier}).default_value({}),
                    **drop.map(
                        lambda policy: {
                            "poll_drop": classmethod(
                                lambda _cls, context: Option.of_optional(context.area).map(lambda area: area.type in policy).default_value(False) if isinstance(policy, frozenset) else policy(context)
                            )
                        }
                    ).default_value({}),
                },
            )
            return (*operators, (Band.INTERFACE, kind))

        return paired(paired(named(importing_row), named(exporting_row)), paired(identity.name(Name.FILE_HANDLER, self.key), extensions)).map(built)


class References(msgspec.Struct, frozen=True, kw_only=True):
    """Native export path mode and the destination subfolder for copied references."""

    mode: Literal["AUTO", "ABSOLUTE", "RELATIVE", "MATCH", "STRIP", "COPY"] = path_reference_mode.keywords["default"]
    subfolder: str = ""

    def path(self, filepath: str, destination: Path, library: Option[bpy.types.Library] = Nothing) -> BlenderResult[str]:
        """Returns a native export reference after its copies, or write faults."""
        copies: set[tuple[str, str]] = set()
        reference = path_reference(filepath, bpy.path.abspath("//"), str(destination), mode=self.mode, copy_subdir=self.subfolder, copy_set=copies, library=library.to_optional())
        return files_copied((Path(source), Path(target)) for source, target in copies).map(lambda _: reference)


class Exporter(msgspec.Struct, frozen=True, kw_only=True):
    """Collection export settings using an extension key or a native handler enum item."""

    handler: Key | bpy.types.EnumPropertyItem
    path: str
    values: Mapping[str, Value] = msgspec.field(default_factory=dict)


# --- [OPERATIONS] -----------------------------------------------------------------------


def scale(scene: bpy.types.Scene, source: str) -> BlenderResult[float]:
    """Returns source-unit text in scene units, or the native length-parser refusal."""
    return (
        catch(lambda: bpy.utils.units.to_value(bpy.utils.units.systems.METRIC, bpy.utils.units.categories.LENGTH, source), exception=ValueError)()  # type: ignore[attr-defined]
        .map(lambda meters: meters / scene.unit_settings.scale_length)
        .map_error(lambda error: (BlenderReadFault(request=source, error=error),))
    )


def reconciled(identity: Identity, collection: bpy.types.Collection, exporters: Mapping[str, Exporter]) -> BlenderResult[None]:
    """Returns None after reconciliation, or every independent naming, acquisition, and write fault."""
    held = collection.exporters
    existing = dict(held.items())
    extent = Descriptor.resolve(bpy.types.CollectionExport, "name", bpy.types.StringProperty).map(lambda descriptor: descriptor.prop.length_max - 1)

    with ExitStack() as cleanup:

        def placed(name: str, exporter: Exporter) -> BlenderResult[None]:
            def acquired(identifier: str) -> BlenderResult[None]:
                match Option.of_optional(existing.get(name)):
                    case Option(tag="some", some=item):
                        made = item
                    case _:
                        try:
                            made = held.new(type=identifier, name=name)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                        except TypeError:
                            return Error((
                                BlenderChoiceFault(
                                    path=f"{collection!r}.exporters",
                                    identifier=identifier,
                                    accepted=tuple(item.identifier for item in bpy.types.CollectionExports.bl_rna.functions["new"].parameters["type"].enum_items),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                                ),
                            ))
                        except (RuntimeError, ReferenceError) as error:
                            return Error((BlenderEditFault(request=f"{collection!r}.exporters", error=error),))
                        cleanup.callback(held.remove, made)
                return accumulated(starmap(written, ((made, "filepath", exporter.path), *((made.export_properties, key, value) for key, value in exporter.values.items())))).map(lambda _: None)

            match exporter.handler:
                case bpy.types.EnumPropertyItem(identifier=identifier):
                    handler: BlenderResult[str] = Ok(identifier)
                case key:
                    handler = identity.name(Name.FILE_HANDLER, key)
            try:
                admitted = extent.map(
                    lambda limit: msgspec.convert((name, name.encode()), tuple[Annotated[str, msgspec.Meta(pattern=r"\A[^\x00]+\Z")], Annotated[bytes, msgspec.Meta(max_length=limit)]])
                )
            except (msgspec.ValidationError, UnicodeError) as error:
                admitted = Error((BlenderReadFault(request=name, error=error),))
            return paired(admitted, handler).bind(lambda pair: acquired(pair[1]))

        def removed(item: bpy.types.CollectionExport) -> BlenderResult[None]:
            try:
                held.remove(item)
            except (RuntimeError, ReferenceError) as error:
                return Error((BlenderEditFault(request=f"{collection!r}.exporters", error=error),))
            return Ok(None)

        def completed(_: tuple[None, ...]) -> BlenderResult[None]:
            return committed(accumulated(removed(item) for name, item in existing.items() if name not in exporters), cleanup).map(lambda _: None)

        return accumulated(starmap(placed, exporters.items())).bind(completed)


def exported(scene: bpy.types.Scene, form: SceneFormat, collection: bpy.types.Collection, path: Path) -> BlenderResult[Path]:
    """Returns a collection's Alembic or animated USD path, or the native export refusal."""
    match form:
        case SceneFormat.ALEMBIC:
            properties: dict[str, object] = {"selected": False, "as_background_job": False}
        case SceneFormat.USD:
            properties = {"export_animation": True, "convert_scene_units": "CUSTOM", "meters_per_unit": scene.unit_settings.scale_length}
    return called(form.value, {"filepath": str(path), "collection": collection.name} | properties, override=Some(bpy.context.temp_override(scene=scene))).map(lambda _: path)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Bidirectional", "Drop", "Exporter", "HandlerRows", "Importing", "Interchange", "References", "SceneFormat", "exported", "reconciled", "scale"]
