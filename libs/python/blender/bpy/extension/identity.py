"""Extension identity and generated host names."""

from collections.abc import Mapping
from enum import auto, Enum
import importlib.resources
from pathlib import Path
from types import MappingProxyType
from typing import Annotated, Final, Self
from uuid import NAMESPACE_URL, uuid5

import bpy
from expression import Error, Ok
from expression.extra.result import catch
import msgspec
import structlog
from structlog.typing import FilteringBoundLogger

from blender.bpy.extension.reports import Reports
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Outcome, RecordReason

# --- [TYPES] ----------------------------------------------------------------------------

type Key = Annotated[str, msgspec.Meta(pattern=r"\A[a-z][a-z0-9]*(?:_[a-z0-9]+)*\Z")]


class Name(Enum):
    """Host name form."""

    PANEL = auto()
    MENU = auto()
    HEADER = auto()
    LIST = auto()
    SHELF = auto()
    FILE_HANDLER = auto()
    GIZMO_GROUP = auto()
    GROUP = auto()
    ENGINE = auto()
    ATTACHMENT = auto()
    OPERATOR = auto()
    CLI = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class _Form(msgspec.Struct, frozen=True):
    """Format over manifest ID and row key in either case or digest of both, host UTF-8 byte length limit, and character syntax host enforces."""

    template: str
    constraint: msgspec.Meta = msgspec.Meta(max_length=63)
    pattern: str | None = None


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

    def name(self, form: Name, key: Key) -> Outcome[str]:
        """Builds validated row host name.

        Args:
            form: Host registration template.
            key: Row key in lowercase snake case.

        Returns:
            Ok(name), register fault for refused name.
        """
        name = _FORMS[form].template.format(
            ID=self.manifest.id.upper(), id=self.manifest.id.lower(), key=key, KEY=key.upper(), digest=uuid5(NAMESPACE_URL, f"{self.manifest.id}/{form.name}/{key}").hex
        )
        return (
            catch(
                lambda: msgspec.convert((key, name, name.encode()), tuple[Key, Annotated[str, msgspec.Meta(pattern=_FORMS[form].pattern)], Annotated[bytes, _FORMS[form].constraint]]),
                exception=msgspec.ValidationError,
            )()
            .map(lambda _: name)
            .map_error(lambda error: (Fault(reason=BlenderReason.UNREGISTERED, subject=name, cause=error),))
        )

    # --- [READING]

    @classmethod
    def read(cls, package: str) -> Outcome[Self]:
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
            return Error((Fault(reason=RecordReason.UNREADABLE, subject=str(path), cause=error),))
        logger = structlog.get_logger(package, extension=manifest.id)
        return Ok(cls(package=package, manifest=manifest, folder=Path(bpy.utils.extension_path_user(package)), logger=logger, reports=Reports(logger)))


# --- [TABLES] ---------------------------------------------------------------------------

_FORMS: Final[Mapping[Name, _Form]] = MappingProxyType({
    Name.PANEL: _Form("{ID}_PT_{key}"),
    Name.MENU: _Form("{ID}_MT_{key}"),
    Name.HEADER: _Form("{ID}_HT_{key}"),
    Name.LIST: _Form("{ID}_UL_{key}"),
    Name.SHELF: _Form("{ID}_AST_{key}"),
    Name.FILE_HANDLER: _Form("{ID}_FH_{key}"),
    Name.GIZMO_GROUP: _Form("{ID}_GGT_{key}"),
    Name.GROUP: _Form("GROUP_{digest}"),
    Name.ENGINE: _Form("{ID}_{KEY}"),
    Name.ATTACHMENT: _Form("{id}_{key}"),
    Name.OPERATOR: _Form("{id}.{key}", msgspec.Meta(max_length=60), r"\A[a-z0-9_]+\.[a-z0-9_]+\Z"),
    Name.CLI: _Form("{id}_{key}", msgspec.Meta()),
})

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Identity", "Key", "Manifest", "Name"]
