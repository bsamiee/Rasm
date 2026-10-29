"""Registration bands derived from host base classes, class registration in dependency order, host registries for translations, manual links, and command-line commands, and the extension registration service."""

from collections import deque
from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import AbstractContextManager, contextmanager, ExitStack
from enum import auto, Enum
from functools import partial, reduce
import graphlib
from itertools import chain
from types import MappingProxyType
from typing import Any, Final, NoReturn, Protocol, runtime_checkable

import bpy
from expression import Error, Nothing, Ok, Option, Result, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.mirrors import annotated
from blender.bpy.data.rna.paths import definition, Struct
from blender.bpy.extension.events import Handler, Subscription, Timer
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.reports import rendered, window_manager
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Faults, Outcome
from blender.records.stores import argued

# --- [TYPES] ----------------------------------------------------------------------------


class Band(Enum):
    """Acquisition order, with resources released in reverse."""

    GROUPS = auto()
    ATTACHMENTS = auto()
    RESOURCES = auto()
    PREFERENCES = auto()
    OPERATORS = auto()
    INTERFACE = auto()
    APPENDS = auto()
    KEYMAP_ITEMS = auto()
    DRAW_HANDLERS = auto()
    APP_HANDLERS = auto()
    SUBSCRIPTIONS = auto()
    TIMERS = auto()
    REGISTRIES = auto()


type Acquisition = type | AbstractContextManager[Outcome[None] | None]
type Declaration = Registrable | Resource[Any] | Handler | Subscription | Timer
type _Banded = tuple[Band, AbstractContextManager[Outcome[None] | None]]
type Acquisitions = tuple[Declaration | type | _Banded, ...]


@runtime_checkable
class Registrable(Protocol):
    """Row building its acquisitions under extension identity."""

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions] | AbstractContextManager[Outcome[Acquisitions]]:
        """Builds acquisitions, optionally inside a construction scope.

        Args:
            identity: Extension with names, folder, and reports row reads.

        Returns:
            Build result, direct or yielded by its construction context manager.
        """


@runtime_checkable
class Dependent(Protocol):
    """Row whose build reads declarations the scope builds before every row."""

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Declarations built before dependent rows build."""


@runtime_checkable
class Gathering(Protocol):
    """Row building its acquisitions over every declaration the scope built before it."""

    def gathered(self, identity: Identity, declared: "tuple[Declaration | Gathering, ...]", /) -> Outcome[Acquisitions]:
        """Builds acquisitions over declarations of every other row.

        Args:
            identity: Extension with names, folder, and reports row reads.
            declared: Declarations in build order.

        Returns:
            Build result.
        """


# --- [CONSTANTS] ------------------------------------------------------------------------

_SCHEMA: Final = (Band.GROUPS, Band.ATTACHMENTS)
_EVENTS: Final[Mapping[type[Handler | Subscription | Timer], Band]] = MappingProxyType({Handler: Band.APP_HANDLERS, Subscription: Band.SUBSCRIPTIONS, Timer: Band.TIMERS})
_BANDS: Final[Mapping[type, Band]] = MappingProxyType({
    bpy.types.PropertyGroup: Band.GROUPS,
    bpy.types.AddonPreferences: Band.PREFERENCES,
    bpy.types.KeyConfigPreferences: Band.PREFERENCES,
    bpy.types.Operator: Band.OPERATORS,
    bpy.types.Macro: Band.OPERATORS,
    bpy.types.Panel: Band.INTERFACE,
    bpy.types.Menu: Band.INTERFACE,
    bpy.types.Header: Band.INTERFACE,
    bpy.types.UIList: Band.INTERFACE,
    bpy.types.AssetShelf: Band.INTERFACE,
    bpy.types.FileHandler: Band.INTERFACE,
    bpy.types.GizmoGroup: Band.INTERFACE,
    bpy.types.Gizmo: Band.INTERFACE,
    bpy.types.RenderEngine: Band.INTERFACE,
    bpy.types.KeyingSetInfo: Band.INTERFACE,
    bpy.types.NodeTree: Band.INTERFACE,
    bpy.types.Node: Band.INTERFACE,
    bpy.types.NodeSocket: Band.INTERFACE,
    bpy.types.NodeTreeInterfaceSocket: Band.INTERFACE,
    bpy.types.USDHook: Band.INTERFACE,
})

# --- [MODELS] ---------------------------------------------------------------------------


class Translations(msgspec.Struct, frozen=True, kw_only=True):
    """Interface text translations registered under extension package, in the host form keyed by locale, then by translation context and message."""

    table: Mapping[str, Mapping[tuple[str, str], str]]

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Builds table registration acquisition.

        Args:
            identity: Extension with package keying translation table.

        Returns:
            Ok of one registry acquisition.
        """
        table = {locale: dict(entries) for locale, entries in self.table.items()}
        return Ok(((Band.REGISTRIES, _held(identity.package, partial(bpy.app.translations.register, identity.package, table), lambda _: bpy.app.translations.unregister(identity.package))),))


class CliCommand[A](msgspec.Struct, frozen=True, kw_only=True):
    """Command-line command with base64 JSON first argument, exiting with the code its run answers, 1 after faults."""

    key: Key
    decoder: msgspec.json.Decoder[A]
    run: Callable[[A, tuple[str, ...]], Outcome[int]]

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Builds named command acquisition, holding the command while the scope stands with its decode and run faults held in reports.

        Args:
            identity: Extension with manifest ID prefixing command name.

        Returns:
            Ok of one registry acquisition, register fault for refused key.
        """

        def registered(command: str) -> AbstractContextManager[Outcome[None]]:
            executed = identity.reports.reported(partial(self.executed, command))
            return _held(command, partial(bpy.utils.register_cli_command, command, lambda arguments: executed(arguments).default_value(1)), bpy.utils.unregister_cli_command)

        return identity.identifier(self.key).map(lambda command: ((Band.REGISTRIES, registered(command)),))

    def executed(self, command: str, arguments: list[str], /) -> Outcome[int]:
        """Decodes the first argument, passing the rest unchanged.

        Args:
            command: Name `blender --command` takes.
            arguments: Arguments after the command name.

        Returns:
            Run result, read fault for an undecodable document, absent fault for no argument.
        """
        match arguments:
            case [document, *trailing]:
                return argued(self.decoder, command, document).bind(lambda value: self.run(value, tuple(trailing)))
            case _:
                return Error(Fault.of(BlenderReason.ABSENT, command, member="arguments"))


# --- [SERVICES] -------------------------------------------------------------------------


class Registration:
    """Register and unregister callbacks of one extension over its rows, rebuilt on each registration.

    Prerequisites of every row precede the rows, a nested row's prerequisites precede it, and gathering rows build last.
    Equal declarations build once, each class acquires in the band of its host base, and a manifest website holds Online Manual links over the extension.
    A refused registration raises one `RuntimeError` of rendered faults after every acquisition before it released.
    """

    __slots__ = ("_declarations", "_held", "_pairs", "_stacks", "package", "rows", "unregister")

    def __init__(self, package: str, rows: tuple[Declaration | Gathering | type, ...], /) -> None:
        """Holds the package `Identity.read` reads and the rows, with nothing registered, `unregister` releasing every registration in reverse."""
        self.package, self.rows, self._held = package, rows, ExitStack()
        self.unregister: Callable[[], None] = self._held.close
        self._declarations: list[Declaration | Gathering] = []
        self._pairs: dict[tuple[Band, Acquisition], bool] = {}
        self._stacks: dict[Band, ExitStack] = {}

    # --- [LIFECYCLE]

    def register(self) -> None:
        """Acquires every row's registrations, holding their releases until `unregister`, a refusal raising the `RuntimeError` of every rendered fault."""

        def refused(faults: Faults) -> NoReturn:
            raise RuntimeError("\n".join(text for _, text in rendered(faults)))

        with ExitStack() as stack:
            Identity.read(self.package).bind(partial(self._acquired, stack)).map(lambda _: self._held.push(stack.pop_all())).default_with(refused)

    # --- [BUILD]

    def _acquired(self, stack: ExitStack, identity: Identity, /) -> Outcome[None]:
        """Enters the keymap merge, reports, and band stacks onto stack, holding the manual map hook while the manifest names a website, then builds every row's prerequisites, the rows with gathering rows last, and enters the bands after the schema."""

        def manual(website: str) -> AbstractContextManager[Outcome[None]]:
            hook = partial(_mapped, identity, website)
            return _held(website, partial(bpy.utils.register_manual_map, hook), lambda _: bpy.utils.unregister_manual_map(hook))

        stack.callback(lambda: window_manager().keyconfigs.update(keep_properties=True))
        stack.enter_context(identity.reports)
        self._stacks = {band: stack.enter_context(ExitStack()) for band in Band}
        self._declarations = []
        self._pairs = dict.fromkeys(((Band.REGISTRIES, manual(website)) for website in Option.of_optional(identity.manifest.website).to_list()), False)
        built = partial(self._built, stack, identity)
        prerequisites = chain.from_iterable(map(_prerequisites, self.rows))
        return _each(built, (*prerequisites, *sorted(self.rows, key=lambda row: isinstance(row, Gathering)))).bind(lambda _: _each(self._opened, (band for band in Band if band not in _SCHEMA)))

    def _built(self, stack: ExitStack, identity: Identity, row: Declaration | Gathering | type | _Banded, /) -> Outcome[None]:
        """Pairs each class with the band of the host base nearest it in its method resolution order, and builds each declaration once after its prerequisites, holding a construction scope on the registration stack and entering new schema acquisitions after each build."""
        built = partial(self._built, stack, identity)

        def declared(declaration: Declaration | Gathering) -> Outcome[Acquisitions]:
            answer: Outcome[Acquisitions] | AbstractContextManager[Outcome[Acquisitions]]
            match declaration:
                case Resource():
                    answer = Ok((
                        (Band.RESOURCES, identity.held.holding(declaration, partial(declaration.made, identity))),
                        *(Handler(handlers=bpy.app.handlers.load_post, body=partial(identity.held.renewed, declaration, renew)) for renew in declaration.renewed.to_list()),
                    ))
                case Handler() | Subscription() | Timer():
                    answer = Ok(((_EVENTS[type(declaration)], declaration.held(identity)),))
                case Gathering():
                    answer = declaration.gathered(identity, tuple(self._declarations))
                case _:
                    answer = declaration.acquisitions(identity)
            self._declarations.append(declaration)
            return stack.enter_context(answer) if isinstance(answer, AbstractContextManager) else answer

        match row:
            case type() as cls:
                return (
                    Block
                    .of_seq(cls.__mro__)
                    .choose(lambda base: Option.of_optional(_BANDS.get(base)))
                    .try_head()
                    .to_result_with(partial(Fault.of, BlenderReason.ABSENT, cls.__name__, member="registrable host base"))
                    .map(lambda band: self._pairs.setdefault((band, cls), False))
                    .map(lambda _: None)
                )
            case _ if row in self._declarations:
                return Ok(None)
            case Resource() | Handler() | Subscription() | Timer() | Gathering() | Registrable():
                return (
                    _each(built, _prerequisites(row))
                    .bind(lambda _: _each(self._opened, _SCHEMA))
                    .bind(lambda _: declared(row))
                    .bind(partial(_each, built))
                    .bind(lambda _: _each(self._opened, _SCHEMA))
                )
            case _:
                self._pairs.setdefault(row, False)
                return Ok(None)

    def _opened(self, band: Band, /) -> Outcome[None]:
        """Enters the band's pairs not yet entered, marking them entered."""
        fresh = tuple((kind, acquisition) for (kind, acquisition), done in self._pairs.items() if kind is band and not done)
        return entered(self._stacks[band], tuple(acquisition for _, acquisition in fresh)).map(lambda _: self._pairs.update(dict.fromkeys(fresh, True)))


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [ROWS]


def _prerequisites(row: object, /) -> tuple[Declaration, ...]:
    """Declarations a dependent row reads, none for any other row."""
    return row.prerequisites if isinstance(row, Dependent) else ()


# --- [CLASSES]


def entered(stack: ExitStack, acquisitions: tuple[Acquisition, ...], /) -> Outcome[None]:
    """Registers classes in dependency order, then enters managers, onto stack until first refusal, each class released in reverse whenever the host holds it.

    Classes order after their pointer and collection property targets and parent panels, names read as the host reads them through the class and its bases: a panel keys by its `bl_idname`, else its class name, and names its parent in `bl_parent_id`.

    Args:
        stack: Stack holding every release.
        acquisitions: Classes and context managers of one band.

    Returns:
        Ok(None), the first refusal's faults, a dependency cycle's register fault.
    """
    classes = tuple(acquisition for acquisition in acquisitions if isinstance(acquisition, type) and issubclass(acquisition, bpy.types.bpy_struct))
    panels: dict[str, type[Struct]] = {Option.of_optional(try_downcast(str, panel.bl_idname)).default_value(panel.__name__): panel for panel in classes if issubclass(panel, bpy.types.Panel)}

    @catch(exception=AttributeError)
    def declared(panel: type[bpy.types.Panel]) -> object:
        return panel.bl_parent_id

    def parent(cls: type[Struct]) -> Option[type[Struct]]:
        return (
            declared(cls).to_option().bind(lambda name: Option.of_optional(try_downcast(str, name))).bind(lambda name: Option.of_optional(panels.get(name)))
            if issubclass(cls, bpy.types.Panel)
            else Nothing
        )

    targets = {
        cls: {*(prop.keywords["type"] for prop in annotated(cls).values() if prop.function in {bpy.props.PointerProperty, bpy.props.CollectionProperty}), *parent(cls).to_list()} for cls in classes
    }
    return (
        catch(tuple, exception=graphlib.CycleError)(graphlib.TopologicalSorter({cls: held & targets.keys() for cls, held in targets.items()}).static_order())
        .map_error(partial(Fault.of, BlenderReason.UNREGISTERED, ", ".join(cls.__name__ for cls in classes)))
        .bind(lambda order: _each(partial(_acquired, stack), (*order, *(acquisition for acquisition in acquisitions if acquisition not in classes))))
    )


def _acquired(stack: ExitStack, acquisition: Acquisition, /) -> Outcome[None]:
    """Registers a class after pushing its release, or enters a manager."""
    match acquisition:
        case type() as cls:

            def released() -> None:
                """Unregisters class while the host holds it, a class whose `register` raised included."""
                if "bl_rna" in vars(cls):
                    bpy.utils.unregister_class(cls)  # ty: ignore[invalid-argument-type]

            stack.callback(released)
            registered: Result[None, Exception] = catch(bpy.utils.register_class, exception=(AttributeError, RuntimeError, ValueError))(cls)  # type: ignore[call-overload]  # ty: ignore[invalid-argument-type]
            return registered.map_error(partial(Fault.of, BlenderReason.UNREGISTERED, cls.__name__))
        case _:
            return Option.of_optional(stack.enter_context(acquisition)).default_value(Ok(None))


def _each[T](step: Callable[[T], Outcome[None]], items: Iterable[T], /) -> Outcome[None]:
    """Runs step over items in order until the first refusal."""
    return reduce(lambda result, item: result.bind(lambda _: step(item)), items, Result[None, Faults](ok=None))


# --- [REGISTRIES]


@contextmanager
def _held[H](subject: str, acquired: Callable[[], H], released: Callable[[H], object], /) -> Generator[Outcome[None]]:
    """Runs acquired, then holds the release of the handle it answers until scope exit, register fault for a registration the host refuses."""
    with ExitStack() as stack:
        yield catch(acquired, exception=ValueError)().map(lambda handle: stack.callback(released, handle)).map(lambda _: None).map_error(partial(Fault.of, BlenderReason.UNREGISTERED, subject))


def _mapped(identity: Identity, website: str, /) -> tuple[str, list[tuple[str, str]]]:
    """Returns website with extension operator, type, and property patterns, then one pattern per preferences struct and property group behind an extension-prefixed property reachable from `BlendData` or nested in such a group or in the extension's preferences."""
    category, group = identity.manifest.id.lower(), definition(bpy.types.PropertyGroup)

    def grouped(struct: bpy.types.Struct) -> bool:
        return struct.base == group

    attached = tuple(target for identifier, target in _walked((definition(bpy.types.BlendData),), lambda _: True) if identifier.startswith(f"{category}_") and grouped(target))
    owned = identity.preferences(bpy.types.AddonPreferences).map(definition).to_list()
    nested = tuple(target for _, target in _walked((*attached, *owned), grouped) if grouped(target))
    return website, [
        *((f"bpy.ops.{category}.{key}", f"#{key}") for key in dir(getattr(bpy.ops, category))),
        (f"bpy.types.{category}_*", ""),
        (f"bpy.types.*.{category}_*", ""),
        *((f"bpy.types.{identifier.lower()}.*", "") for identifier in dict.fromkeys(struct.identifier for struct in (*owned, *attached, *nested))),
    ]


def _walked(starts: tuple[bpy.types.Struct, ...], followed: Callable[[bpy.types.Struct], bool], /) -> tuple[tuple[str, bpy.types.Struct], ...]:
    """Returns identifier and fixed target of every pointer and collection property breadth-first from starts, entering each target `followed` accepts once."""
    seen, frontier, edges = {start.identifier for start in starts}, deque(starts), list[tuple[str, bpy.types.Struct]]()
    while frontier:
        properties = frontier.popleft().properties
        for identifier, target in (
            (prop.identifier, target) for prop in properties if isinstance(prop, bpy.types.PointerProperty | bpy.types.CollectionProperty) for target in Option.of_optional(prop.fixed_type).to_list()
        ):
            edges.append((identifier, target))
            if target.identifier not in seen and followed(target):
                seen.add(target.identifier)
                frontier.append(target)
    return tuple(edges)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Acquisition", "Acquisitions", "Band", "CliCommand", "Declaration", "Dependent", "Gathering", "Registrable", "Registration", "Translations", "entered"]
