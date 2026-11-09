"""Extension identity, generated host names, add-on preferences, and native manifest reads."""

from base64 import b32encode
from collections.abc import Mapping
from functools import partial
import importlib.resources
from pathlib import Path
from types import MappingProxyType
from typing import Annotated, Final, Self
from uuid import NAMESPACE_URL, uuid5

import bpy
from expression import Option, Result, try_downcast
from expression.extra.result import catch
import msgspec

from rasm_bpy.data._extension.reports import Refusal, Reports
from rasm_bpy.data._rna.paths import _addon_preferences, Struct
from rasm_records import Fault, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Key = Annotated[str, msgspec.Meta(pattern=r"\A[a-z][a-z0-9]*(?:_[a-z0-9]+)*\Z")]

# --- [CONSTANTS] ------------------------------------------------------------------------

_ENGINE_ID_BYTES: Final = 31
_FORMS: Final[Mapping[type[Struct], str]] = MappingProxyType({
    **{
        base: f"{{ID}}_{infix}_{{key}}"
        for base, infix in (
            (bpy.types.Panel, "PT"),
            (bpy.types.Menu, "MT"),
            (bpy.types.Header, "HT"),
            (bpy.types.UIList, "UL"),
            (bpy.types.AssetShelf, "AST"),
            (bpy.types.FileHandler, "FH"),
            (bpy.types.GizmoGroup, "GGT"),
            (bpy.types.Gizmo, "GT"),
        )
    },
    **dict.fromkeys((bpy.types.NodeTree, bpy.types.Node, bpy.types.NodeSocket, bpy.types.NodeTreeInterfaceSocket, bpy.types.KeyingSetInfo, bpy.types.USDHook), "{ID}_{BASE}_{key}"),
    bpy.types.Operator: "{id}.{key}",
    bpy.types.PropertyGroup: "{ID}_{key}",
})

# --- [MODELS] ---------------------------------------------------------------------------


class Manifest(msgspec.Struct, frozen=True, kw_only=True):
    """`blender_manifest.toml` fields read."""

    id: str
    name: str
    version: str
    website: str | None = None

    @property
    def prefix(self) -> str:
        """Prefix of every property, record, and command identifier the extension declares."""
        return f"{self.id.lower()}_"

    @property
    def stamp(self) -> Mapping[str, str]:
        """File metadata naming the extension and its version as the software that wrote the file."""
        return {"Software": f"{self.name} {self.version}"}


class Identity(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Enabled extension context."""

    # --- [STATE]

    package: str
    manifest: Manifest
    folder: Path
    reports: Reports

    # --- [NAMES]

    def name(self, base: type[Struct], key: Key, /) -> str:
        """Returns the host class name under the extension key."""
        match base:
            case bpy.types.RenderEngine:
                scope, stem = self.manifest.id.upper(), key.upper()
                return (
                    f"{scope}_{stem}"
                    if scope and not scope.startswith("_") and len(scope.encode()) + len(stem.encode()) < _ENGINE_ID_BYTES
                    else f"_{b32encode(uuid5(NAMESPACE_URL, f'{self.manifest.id}/{key}').bytes).decode().rstrip('=')}"
                )
            case _:
                return _FORMS[base].format(ID=self.manifest.id.upper(), id=self.manifest.id.lower(), key=key, BASE=base.__name__)

    def identifier(self, key: Key, /) -> str:
        """Returns the property, record, or command identifier under the manifest ID."""
        return f"{self.manifest.prefix}{key}"

    def owns(self, identifier: str, /) -> bool:
        """Whether an identifier takes the manifest prefix, a property, record, or command the extension declares."""
        return identifier.startswith(self.manifest.prefix)

    # --- [READS]

    def preferences[T: bpy.types.AddonPreferences](self, kind: type[T], /) -> Option[T]:
        """Reads the extension's add-on preferences as kind (`bpy.types.AddonPreferences` for any), Nothing while disabled or holding another class."""
        preferences: bpy.types.Preferences = bpy.context.preferences  # ty: ignore[invalid-assignment]
        return _addon_preferences(preferences, self.package).bind(lambda held: Option.of_optional(try_downcast(kind, held)))

    @classmethod
    def read(cls, package: str, /) -> Outcome[Self]:
        """Reads an extension package's manifest and created user folder, unreadable fault for its manifest, unregistered fault for a package outside extensions, unwritable fault for the empty answer of a folder the host failed to create."""
        path = importlib.resources.files(package) / "blender_manifest.toml"
        manifest = (
            Result[bytes, OSError | msgspec.DecodeError]
            .bind(catch(path.read_bytes, exception=OSError)(), lambda data: catch(lambda: msgspec.toml.decode(data, type=Manifest), exception=msgspec.DecodeError)())
            .map_error(partial(Fault.of, Reason.UNREADABLE, str(path)))
        )
        folder = (
            catch(bpy.utils.extension_path_user, exception=ValueError)(package, create=True)
            .map_error(partial(Fault.of, Refusal.UNREGISTERED, package))
            .filter(bool, Fault.of(Reason.UNWRITABLE, package, "host created no user folder"))
            .map(Path)
        )
        return paired(manifest, folder).map(lambda pair: cls(package=package, manifest=pair[0], folder=pair[1], reports=Reports(package, pair[1], extension=pair[0].id)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Identity", "Key", "Manifest"]
