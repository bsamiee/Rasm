"""Extension identity, generated host names, add-on preferences, and resources held from registration to release."""

from collections.abc import Callable, Generator, Mapping
from contextlib import AbstractContextManager, contextmanager, ExitStack
from functools import partial
import importlib.resources
from pathlib import Path
from types import MappingProxyType
from typing import Annotated, Any, Final, Self
from uuid import NAMESPACE_URL, uuid5

import bpy
from expression import Nothing, Ok, Option, Result, Some, try_downcast
from expression.extra.result import catch
import msgspec
import structlog

from blender.bpy.data.rna.paths import addon_preferences, Struct
from blender.bpy.extension.reports import Reports
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Outcome, paired, RecordReason

# --- [TYPES] ----------------------------------------------------------------------------

type Key = Annotated[str, msgspec.Meta(pattern=r"\A[a-z][a-z0-9]*(?:_[a-z0-9]+)*\Z")]
type _Made[T] = Callable[[Identity], AbstractContextManager[T]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_FORMS: Final[Mapping[type[Struct], tuple[str, msgspec.Meta]]] = MappingProxyType({
    bpy.types.Panel: ("{ID}_PT_{key}", msgspec.Meta(max_length=63)),
    bpy.types.Menu: ("{ID}_MT_{key}", msgspec.Meta(max_length=63)),
    bpy.types.Header: ("{ID}_HT_{key}", msgspec.Meta(max_length=63)),
    bpy.types.UIList: ("{ID}_UL_{key}", msgspec.Meta(max_length=63)),
    bpy.types.AssetShelf: ("{ID}_AST_{key}", msgspec.Meta(max_length=63)),
    bpy.types.FileHandler: ("{ID}_FH_{key}", msgspec.Meta(max_length=63)),
    bpy.types.GizmoGroup: ("{ID}_GGT_{key}", msgspec.Meta(max_length=63)),
    bpy.types.Gizmo: ("{ID}_GT_{key}", msgspec.Meta(max_length=63)),
    bpy.types.PropertyGroup: ("GROUP_{digest}", msgspec.Meta(max_length=63)),
    bpy.types.RenderEngine: ("{ID}_{KEY}", msgspec.Meta(max_length=63)),
    bpy.types.NodeTree: ("{ID}_{BASE}_{key}", msgspec.Meta(max_length=63)),
    bpy.types.Node: ("{ID}_{BASE}_{key}", msgspec.Meta(max_length=63)),
    bpy.types.NodeSocket: ("{ID}_{BASE}_{key}", msgspec.Meta(max_length=63)),
    bpy.types.NodeTreeInterfaceSocket: ("{ID}_{BASE}_{key}", msgspec.Meta(max_length=63)),
    bpy.types.KeyingSetInfo: ("{ID}_{BASE}_{key}", msgspec.Meta(max_length=63)),
    bpy.types.USDHook: ("{ID}_{BASE}_{key}", msgspec.Meta(max_length=63)),
    bpy.types.Operator: ("{id}.{key}", msgspec.Meta(max_length=60)),
})

# --- [MODELS] ---------------------------------------------------------------------------


class Manifest(msgspec.Struct, frozen=True, kw_only=True):
    """`blender_manifest.toml` fields read."""

    id: str
    name: str
    version: str
    website: str | None = None


class Resource[T](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Value each extension holds while its registration scope stands, optionally renewed after every file load."""

    made: _Made[T]
    renewed: Option[Callable[[T], T]] = Nothing


# --- [SERVICES] -------------------------------------------------------------------------


class Held:
    """Values one extension's resources made, keyed by resource, each read typed by the resource that made it."""

    __slots__ = ("values",)

    def __init__(self) -> None:
        """Holds no value until the registration scope makes one."""
        self.values: dict[Resource[Any], Any] = {}

    def __getitem__[T](self, resource: Resource[T], /) -> T:
        """Returns the value resource made."""
        value: T = self.values[resource]
        return value

    def get[T](self, resource: Resource[T], /) -> Option[T]:
        """Returns the value resource made, Nothing outside its scope."""
        return Some(self[resource]) if resource in self.values else Nothing

    @contextmanager
    def holding[T](self, resource: Resource[T], opened: Callable[[], AbstractContextManager[T]], /) -> Generator[None]:
        """Holds the value opened yields under resource until scope exit.

        Args:
            resource: Resource keying the value.
            opened: Context manager factory yielding the value.

        Yields:
            None while the value is held.
        """
        with ExitStack() as stack:
            self.values[resource] = stack.enter_context(opened())
            stack.callback(self.values.pop, resource)
            yield

    def renewed[T](self, resource: Resource[T], renew: Callable[[T], T], /) -> Outcome[None]:
        """Replaces the held value with its renewal.

        Args:
            resource: Resource keying the value.
            renew: Step building the value after a file load.

        Returns:
            Ok(None).
        """
        self.values[resource] = renew(self[resource])
        return Ok(None)


class Identity(msgspec.Struct, frozen=True, kw_only=True):
    """Enabled extension context."""

    package: str
    manifest: Manifest
    folder: Path
    reports: Reports
    held: Held

    # --- [NAMING]

    def name(self, base: type[Struct], key: Key, /) -> Outcome[str]:
        """Builds validated class host name, within the bytes the host keeps of a name of its form.

        Args:
            base: Host base class the named class extends.
            key: Row key in lowercase snake case.

        Returns:
            Ok(name), register fault for refused name or a base no name form serves.
        """

        def formed(form: tuple[str, msgspec.Meta]) -> Outcome[str]:
            template, limit = form
            name = template.format(ID=self.manifest.id.upper(), id=self.manifest.id.lower(), key=key, KEY=key.upper(), BASE=base.__name__, digest=uuid5(NAMESPACE_URL, f"{self.manifest.id}/{key}").hex)
            return _checked(name, (key, name.encode()), tuple[Key, Annotated[bytes, limit]])

        return Option.of_optional(_FORMS.get(base)).to_result_with(partial(Fault.of, BlenderReason.UNREGISTERED, base.__name__, member="name form")).bind(formed)

    def identifier(self, key: Key, /) -> Outcome[str]:
        """Builds validated property, record, or command identifier.

        Args:
            key: Row key in lowercase snake case.

        Returns:
            Ok(identifier), register fault for refused key.
        """
        return _checked(f"{self.manifest.id.lower()}_{key}", key, Key)

    # --- [READING]

    def preferences[T: bpy.types.AddonPreferences](self, kind: type[T], /) -> Option[T]:
        """Reads the extension's add-on preferences as kind.

        Args:
            kind: Preferences class the extension registers, or `bpy.types.AddonPreferences` for any.

        Returns:
            Some(preferences), Nothing while the extension is disabled or holds preferences of another class.
        """
        preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return addon_preferences(preferences, self.package).bind(lambda held: Option.of_optional(try_downcast(kind, held)))

    @classmethod
    def read(cls, package: str, /) -> Outcome[Self]:
        """Reads enabled extension's manifest and user folder.

        Args:
            package: Fully qualified extension package name.

        Returns:
            Ok(identity), read fault for unreadable manifest, register fault for a package Blender holds as no extension.
        """
        path = importlib.resources.files(package) / "blender_manifest.toml"
        manifest = (
            Result[bytes, OSError | msgspec.DecodeError]
            .bind(catch(path.read_bytes, exception=OSError)(), lambda data: catch(msgspec.toml.decode, exception=msgspec.DecodeError)(data, type=Manifest))
            .map_error(partial(Fault.of, RecordReason.UNREADABLE, str(path)))
        )
        folder = catch(bpy.utils.extension_path_user, exception=ValueError)(package).map_error(partial(Fault.of, BlenderReason.UNREGISTERED, package))
        return paired(manifest, folder).map(
            lambda pair: cls(package=package, manifest=pair[0], folder=Path(pair[1]), reports=Reports(structlog.get_logger(package, extension=pair[0].id)), held=Held())
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _checked(name: str, parts: object, form: object, /) -> Outcome[str]:
    """Returns name once parts convert to form, register fault naming it otherwise."""
    return catch(msgspec.convert, exception=msgspec.ValidationError)(parts, form).map(lambda _: name).map_error(partial(Fault.of, BlenderReason.UNREGISTERED, name))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Held", "Identity", "Key", "Manifest", "Resource"]
