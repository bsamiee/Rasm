"""Extension identity, manifest facts, writable folder, and generated host names."""

from enum import Enum
import importlib.resources
import logging
from pathlib import Path
from typing import Annotated, Self
from uuid import NAMESPACE_URL, uuid5

import bpy
from expression import Error, Ok
from expression.extra.result import catch
import msgspec

from blender.bpy.extension.reports import Reports
from blender.bpy.faults import BlenderReadFault, BlenderRegisterFault, BlenderResult

# --- [TYPES] ----------------------------------------------------------------------------

type Key = Annotated[str, msgspec.Meta(pattern=r"\A[a-z][a-z0-9]*(?:_[a-z0-9]+)*\Z")]


class Name(Enum):
    """Generated host name shape and native constraints."""

    PANEL = ("{ID}_PT_{key}", msgspec.Meta(max_length=63))
    MENU = ("{ID}_MT_{key}", msgspec.Meta(max_length=63))
    HEADER = ("{ID}_HT_{key}", msgspec.Meta(max_length=63))
    LIST = ("{ID}_UL_{key}", msgspec.Meta(max_length=63))
    SHELF = ("{ID}_AST_{key}", msgspec.Meta(max_length=63))
    FILE_HANDLER = ("{ID}_FH_{key}", msgspec.Meta(max_length=63))
    GIZMO_GROUP = ("{ID}_GGT_{key}", msgspec.Meta(max_length=63))
    GROUP = ("GROUP_{digest}", msgspec.Meta(max_length=63))
    ENGINE = ("{ID}_{KEY}", msgspec.Meta(max_length=63))
    ATTACHMENT = ("{id}_{key}", msgspec.Meta(max_length=63))
    OPERATOR = ("{id}.{key}", msgspec.Meta(max_length=60), r"\A[a-z0-9_]+\.[a-z0-9_]+\Z")
    CLI = ("{id}_{key}", msgspec.Meta())

    def __init__(self, template: str, constraint: msgspec.Meta, pattern: str | None = None) -> None:
        """Holds the name shape and native constraints.

        Args:
            template: Format over the manifest ID and row key in either case, or a digest of both.
            constraint: Native UTF-8 byte extent applied at the naming boundary.
            pattern: Native name syntax where the host restricts its characters.
        """
        self.template = template
        self.constraint = constraint
        self.pattern = pattern


# --- [MODELS] ---------------------------------------------------------------------------


class Manifest(msgspec.Struct, frozen=True, kw_only=True):
    """Manifest facts consumed by an extension."""

    id: Annotated[str, msgspec.Meta(min_length=1)]
    name: str
    version: str
    website: str | None = None


class Identity(msgspec.Struct, frozen=True, kw_only=True):
    """Extension package, manifest, writable folder, and callback reports."""

    package: str
    manifest: Manifest
    folder: Path
    reports: Reports

    # --- [NAMING]

    def name(self, form: Name, key: Key) -> BlenderResult[str]:
        """Builds and validates a row's host name.

        Args:
            form: Name shape consumed by the row's native host registration.
            key: Row's lowercase snake identifier.

        Returns:
            Ok(name), or a register fault holding the refused generated name and native validation error.
        """
        name = form.template.format(ID=self.manifest.id.upper(), id=self.manifest.id.lower(), key=key, KEY=key.upper(), digest=uuid5(NAMESPACE_URL, f"{self.manifest.id}/{form.name}/{key}").hex)
        return (
            catch(
                lambda: msgspec.convert((key, name, name.encode()), tuple[Key, Annotated[str, msgspec.Meta(pattern=form.pattern)], Annotated[bytes, form.constraint]]),
                exception=msgspec.ValidationError,
            )()
            .map(lambda _: name)
            .map_error(lambda error: (BlenderRegisterFault(request=(name,), error=error),))
        )

    # --- [READING]

    @classmethod
    def read(cls, package: str) -> BlenderResult[Self]:
        """Reads the manifest and native user-folder path of an enabled extension.

        Args:
            package: Extension's fully qualified package name.

        Returns:
            Ok(identity), or a read fault holding the manifest path and its file or decode error.
        """
        manifest = importlib.resources.files(package) / "blender_manifest.toml"
        try:
            return Ok(
                cls(
                    package=package,
                    manifest=msgspec.toml.decode(manifest.read_bytes(), type=Manifest),
                    folder=Path(bpy.utils.extension_path_user(package)),
                    reports=Reports(logging.getLogger(package)),
                )
            )
        except (OSError, msgspec.DecodeError) as error:
            return Error((BlenderReadFault(request=manifest, error=error),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Identity", "Key", "Manifest", "Name"]
