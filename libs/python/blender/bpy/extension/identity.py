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
from blender.records.faults import Fault, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Key = Annotated[str, msgspec.Meta(pattern=r"\A[a-z][a-z0-9]*(?:_[a-z0-9]+)*\Z")]
type _Made[T] = Callable[[Identity], AbstractContextManager[T]]

# --- [CONSTANTS] ------------------------------------------------------------------------

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
    bpy.types.PropertyGroup: "GROUP_{digest}",
    bpy.types.RenderEngine: "{ID}_{KEY}",
    bpy.types.Operator: "{id}.{key}",
})

# --- [MODELS] ---------------------------------------------------------------------------


class Manifest(msgspec.Struct, frozen=True, kw_only=True):
    """`blender_manifest.toml` fields read."""

    id: str
    name: str
    version: str
    website: str | None = None

    @property
    def stamp(self) -> Mapping[str, str]:
        """File metadata naming the extension and its version as the software that wrote the file."""
        return {"Software": f"{self.name} {self.version}"}


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
        """Holds the value the opened scope yields under resource until scope exit."""
        with ExitStack() as stack:
            self.values[resource] = stack.enter_context(opened())
            stack.callback(self.values.pop, resource)
            yield

    def renewed[T](self, resource: Resource[T], renew: Callable[[T], T], /) -> Outcome[None]:
        """Replaces the value held under resource with its renewal after a file load."""
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
        """Builds the host name of a class of the base under a key, register fault for a name past the 63 bytes the host keeps or a base no name form serves, an operator name past 60 characters refused by the host at registration."""

        def formed(template: str) -> Outcome[str]:
            name = template.format(ID=self.manifest.id.upper(), id=self.manifest.id.lower(), key=key, KEY=key.upper(), BASE=base.__name__, digest=uuid5(NAMESPACE_URL, f"{self.manifest.id}/{key}").hex)
            return _checked(name, (key, name.encode()), tuple[Key, Annotated[bytes, msgspec.Meta(max_length=63)]])

        return Option.of_optional(_FORMS.get(base)).to_result_with(partial(Fault.of, Reason.UNREGISTERED, base.__name__, member="name form")).bind(formed)

    def identifier(self, key: Key, /) -> Outcome[str]:
        """Builds a property, record, or command identifier under the manifest ID, register fault for a key outside lowercase snake case."""
        return _checked(f"{self.manifest.id.lower()}_{key}", key, Key)

    # --- [READING]

    def preferences[T: bpy.types.AddonPreferences](self, kind: type[T], /) -> Option[T]:
        """Reads the extension's add-on preferences as kind (`bpy.types.AddonPreferences` for any), Nothing while disabled or holding another class."""
        preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return addon_preferences(preferences, self.package).bind(lambda held: Option.of_optional(try_downcast(kind, held)))

    @classmethod
    def read(cls, package: str, /) -> Outcome[Self]:
        """Reads an enabled extension package's manifest and user folder, read fault for an unreadable manifest, register fault for a package Blender holds as no extension."""
        path = importlib.resources.files(package) / "blender_manifest.toml"
        manifest = (
            Result[bytes, OSError | msgspec.DecodeError]
            .bind(catch(path.read_bytes, exception=OSError)(), lambda data: catch(msgspec.toml.decode, exception=msgspec.DecodeError)(data, type=Manifest))
            .map_error(partial(Fault.of, Reason.UNREADABLE, str(path)))
        )
        folder = catch(bpy.utils.extension_path_user, exception=ValueError)(package).map_error(partial(Fault.of, Reason.UNREGISTERED, package))
        return paired(manifest, folder).map(
            lambda pair: cls(package=package, manifest=pair[0], folder=Path(pair[1]), reports=Reports(structlog.get_logger(package, extension=pair[0].id)), held=Held())
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _checked(name: str, parts: object, form: object, /) -> Outcome[str]:
    """Returns name once parts convert to form, register fault naming it otherwise."""
    return catch(msgspec.convert, exception=msgspec.ValidationError)(parts, form).map(lambda _: name).map_error(partial(Fault.of, Reason.UNREGISTERED, name))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Held", "Identity", "Key", "Manifest", "Resource"]
