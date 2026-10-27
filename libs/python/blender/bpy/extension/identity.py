"""Extension identity, generated host names, and resources held from registration to release."""

from collections.abc import Callable, Generator, Mapping
from contextlib import AbstractContextManager, contextmanager, ExitStack
from functools import partial
import importlib.resources
from pathlib import Path
from types import MappingProxyType
from typing import Annotated, Final, Generic, Protocol, Self, TypeVar
from uuid import NAMESPACE_URL, uuid5

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import structlog
from structlog.typing import FilteringBoundLogger

from blender.bpy.data.rna.descriptors import Struct
from blender.bpy.extension.reports import Reports
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Outcome, RecordReason

# --- [TYPES] ----------------------------------------------------------------------------

type Key = Annotated[str, msgspec.Meta(pattern=r"\A[a-z][a-z0-9]*(?:_[a-z0-9]+)*\Z")]
type _Made[T] = Callable[[Identity], AbstractContextManager[T]]

_T_co = TypeVar("_T_co", covariant=True)


class _Store(Protocol):
    """Values keyed by resource, each read typed by the resource that made it."""

    def __getitem__[T](self, resource: "Resource[T]", /) -> T: ...
    def __setitem__[T](self, resource: "Resource[T]", value: T, /) -> None: ...
    def __contains__(self, resource: object, /) -> bool: ...
    def pop[T](self, resource: "Resource[T]", /) -> T: ...


# --- [MODELS] ---------------------------------------------------------------------------


class _Form(msgspec.Struct, frozen=True):
    """Format over manifest ID and row key in either case or digest of both, and host UTF-8 byte length limit."""

    template: str
    constraint: msgspec.Meta = msgspec.Meta(max_length=63)


class Manifest(msgspec.Struct, frozen=True, kw_only=True):
    """`blender_manifest.toml` fields read."""

    id: str
    name: str
    version: str
    website: str | None = None


class Resource(msgspec.Struct, Generic[_T_co], frozen=True, kw_only=True, eq=False):
    """Value each extension holds while its registration scope stands, optionally renewed after every file load."""

    made: _Made[_T_co]
    renewed: Option[Callable[[_T_co], _T_co]] = Nothing


# --- [SERVICES] -------------------------------------------------------------------------


class Held:
    """Values one extension's resources made, keyed by resource."""

    __slots__ = ("values",)

    def __init__(self) -> None:
        """Holds no value until the registration scope makes one."""
        self.values: _Store = {}  # type: ignore[assignment]

    def __getitem__[T](self, resource: Resource[T]) -> T:
        """Returns the value resource made."""
        return self.values[resource]

    def get[T](self, resource: Resource[T]) -> Option[T]:
        """Returns the value resource made, Nothing outside its scope."""
        return Some(self[resource]) if resource in self.values else Nothing

    @contextmanager
    def holding[T](self, resource: Resource[T], opened: Callable[[], AbstractContextManager[T]]) -> Generator[None]:
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

    def renewed[T](self, resource: Resource[T], renew: Callable[[T], T]) -> Outcome[None]:
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
    logger: FilteringBoundLogger
    reports: Reports
    held: Held

    # --- [NAMING]

    def name(self, base: type[Struct], key: Key) -> Outcome[str]:
        """Builds validated class host name.

        Args:
            base: Host base class the named class extends.
            key: Row key in lowercase snake case.

        Returns:
            Ok(name), register fault for refused name.
        """
        form = _FORMS[base]
        name = form.template.format(ID=self.manifest.id.upper(), id=self.manifest.id.lower(), key=key, KEY=key.upper(), digest=uuid5(NAMESPACE_URL, f"{self.manifest.id}/{key}").hex)
        return _checked(name, (key, name.encode()), tuple[Key, Annotated[bytes, form.constraint]])

    def identifier(self, key: Key) -> Outcome[str]:
        """Builds validated property, record, or command identifier.

        Args:
            key: Row key in lowercase snake case.

        Returns:
            Ok(identifier), register fault for refused key.
        """
        return _checked(f"{self.manifest.id.lower()}_{key}", key, Key)

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
        return (
            Result[bytes, OSError | msgspec.DecodeError]
            .bind(catch(path.read_bytes, exception=OSError)(), lambda data: catch(msgspec.toml.decode, exception=msgspec.DecodeError)(data, type=Manifest))
            .map_error(partial(Fault.of, RecordReason.UNREADABLE, str(path)))
            .map(
                lambda manifest: cls(
                    package=package,
                    manifest=manifest,
                    folder=Path(bpy.utils.extension_path_user(package)),
                    logger=(logger := structlog.get_logger(package, extension=manifest.id)),
                    reports=Reports(logger),
                    held=Held(),
                )
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _checked(name: str, parts: object, form: object) -> Outcome[str]:
    """Returns name once parts convert to form, register fault naming it otherwise."""
    return catch(msgspec.convert, exception=msgspec.ValidationError)(parts, form).map(lambda _: name).map_error(partial(Fault.of, BlenderReason.UNREGISTERED, name))


# --- [TABLES] ---------------------------------------------------------------------------

_FORMS: Final[Mapping[type[Struct], _Form]] = MappingProxyType({
    bpy.types.Panel: _Form("{ID}_PT_{key}"),
    bpy.types.Menu: _Form("{ID}_MT_{key}"),
    bpy.types.Header: _Form("{ID}_HT_{key}"),
    bpy.types.UIList: _Form("{ID}_UL_{key}"),
    bpy.types.AssetShelf: _Form("{ID}_AST_{key}"),
    bpy.types.FileHandler: _Form("{ID}_FH_{key}"),
    bpy.types.GizmoGroup: _Form("{ID}_GGT_{key}"),
    bpy.types.PropertyGroup: _Form("GROUP_{digest}"),
    bpy.types.RenderEngine: _Form("{ID}_{KEY}"),
    bpy.types.Operator: _Form("{id}.{key}", msgspec.Meta()),
})

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Held", "Identity", "Key", "Manifest", "Resource"]
