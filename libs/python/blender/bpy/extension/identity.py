"""Extension identity and generated host names."""

from enum import Enum
import importlib.resources
from pathlib import Path
from typing import Annotated, Self
from uuid import NAMESPACE_URL, uuid5

import bpy
from expression import Error, Ok
from expression.extra.result import catch
import msgspec
import structlog
from structlog.typing import FilteringBoundLogger

from blender.bpy.extension.reports import Reports
from blender.bpy.faults import BlenderReadFault, BlenderRegisterFault, BlenderResult

# --- [TYPES] ----------------------------------------------------------------------------

type Key = Annotated[str, msgspec.Meta(pattern=r"\A[a-z][a-z0-9]*(?:_[a-z0-9]+)*\Z")]


class Name(Enum):
    """Host name template and constraints."""

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
        """Holds name template and constraints.

        Args:
            template: Format over manifest ID and row key in either case, or digest of both.
            constraint: Host UTF-8 byte length limit.
            pattern: Character syntax host enforces.
        """
        self.template = template
        self.constraint = constraint
        self.pattern = pattern


# --- [MODELS] ---------------------------------------------------------------------------


class Manifest(msgspec.Struct, frozen=True, kw_only=True):
    """`blender_manifest.toml` fields read."""

    id: Annotated[str, msgspec.Meta(min_length=1)]
    name: str
    version: str
    website: str | None = None


class Identity(msgspec.Struct, frozen=True, kw_only=True):
    """Enabled extension context."""

    package: str
    manifest: Manifest
    folder: Path
    logger: FilteringBoundLogger
    reports: Reports

    # --- [NAMING]

    def name(self, form: Name, key: Key) -> BlenderResult[str]:
        """Builds validated row host name.

        Args:
            form: Host registration template.
            key: Row key in lowercase snake case.

        Returns:
            Ok(name), register fault for refused name.
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
        """Reads enabled extension's manifest and user folder.

        Args:
            package: Fully qualified extension package name.

        Returns:
            Ok(identity), read fault for unreadable manifest.
        """
        path = importlib.resources.files(package) / "blender_manifest.toml"
        try:
            manifest = msgspec.toml.decode(path.read_bytes(), type=Manifest)
        except (OSError, msgspec.DecodeError) as error:
            return Error((BlenderReadFault(request=path, error=error),))
        logger = structlog.get_logger(package, extension=manifest.id)
        return Ok(cls(package=package, manifest=manifest, folder=Path(bpy.utils.extension_path_user(package)), logger=logger, reports=Reports(logger)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Identity", "Key", "Manifest", "Name"]
