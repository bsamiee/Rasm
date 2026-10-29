"""Extension rows: a witness of the registered identity and band order, panels and groups out of dependency order, translations, handlers, a startup row, a renewed resource, a timer, and a msgbus key, with probes reading them."""

from collections.abc import Callable, Generator, Iterator, Sequence
from contextlib import contextmanager, ExitStack, nullcontext
from functools import partial
from pathlib import Path
from typing import Final, override, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator
from blender.bpy.data.rna.paths import definition
from blender.bpy.extension.events import Change, Handler, located, runtime, Startup, Subscription, Timer, updated
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.extension.registration import Acquisitions, Band, Registration, Translations
from blender.bpy.extension.reports import level
from blender.records.faults import Fault, Outcome, Reason

from .commands import OPERATORS

# --- [CONSTANTS] ------------------------------------------------------------------------

PACKAGE: Final = __name__.rpartition(".")[0]
GREETING: Final = "Fixture Hello"
BASELINE: Final = (len(bpy.app.handlers.load_post), len(bpy.app.handlers.depsgraph_update_post))

# --- [MODELS] ---------------------------------------------------------------------------


class Inner(bpy.types.PropertyGroup):
    """Group another group points at."""

    level: bpy.props.IntProperty(name="Level")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Outer(bpy.types.PropertyGroup):
    """Group pointing at and collecting the inner group."""

    inner: bpy.props.PointerProperty(type=Inner)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    items: bpy.props.CollectionProperty(type=Inner)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class ChildPanel(bpy.types.Panel):
    """Panel nested under a parent panel named by its `bl_idname`."""

    bl_idname = "FIXTURE_PT_child"
    bl_label = "Child"
    bl_space_type = "PROPERTIES"
    bl_region_type = "WINDOW"
    bl_context = "scene"
    bl_parent_id = "FIXTURE_PT_parent"

    @override
    def draw(self, context: bpy.types.Context) -> None:
        """Draws an empty body."""


class ParentPanel(bpy.types.Panel):
    """Panel holding the child panel."""

    bl_idname = "FIXTURE_PT_parent"
    bl_label = "Parent"
    bl_space_type = "PROPERTIES"
    bl_region_type = "WINDOW"
    bl_context = "scene"

    @override
    def draw(self, context: bpy.types.Context) -> None:
        """Draws an empty body."""


class Orphan(bpy.types.Panel):
    """Panel naming a parent no registration holds, which the host refuses."""

    bl_idname = "FIXTURE_PT_orphan"
    bl_label = "Orphan"
    bl_space_type = "PROPERTIES"
    bl_region_type = "WINDOW"
    bl_parent_id = "FIXTURE_PT_absent"

    @override
    def draw(self, context: bpy.types.Context) -> None:
        """Draws an empty body."""


class Drawless(bpy.types.Panel):
    """Panel declaring no draw callback, which the host refuses."""

    bl_idname = "FIXTURE_PT_drawless"
    bl_label = "Drawless"
    bl_space_type = "PROPERTIES"
    bl_region_type = "WINDOW"


DEFERRED: Final[type[bpy.types.Panel]] = type(
    "FIXTURE_PT_deferred",
    (bpy.types.Panel,),
    {"bl_label": "Deferred", "bl_space_type": "PROPERTIES", "bl_region_type": "WINDOW", "bl_context": "scene", "bl_parent_id": "FIXTURE_PT_parent", "draw": lambda _panel, _context: None},
)


class Steps(msgspec.Struct, frozen=True):
    """Request taking one host pass per step left, noting each step it applies."""

    left: int
    noted: Callable[[int], None]

    def advanced(self) -> Outcome[Option[Self]]:
        """Applies one step, keeping the rest."""
        self.noted(self.left)
        return Ok(Some(msgspec.structs.replace(self, left=self.left - 1)) if self.left > 1 else Nothing)


# --- [SERVICES] -------------------------------------------------------------------------


class Witness:
    """Row keeping the identity its registration built, the order its banded scopes enter and leave, and what its handler bodies received."""

    __slots__ = ("events", "identity", "loads", "moves", "starts")

    def __init__(self) -> None:
        """Holds no identity until registration and no handler run."""
        self.identity: Option[Identity] = Nothing
        self.events: list[str] = []
        self.loads: list[Option[Path]] = []
        self.moves: list[tuple[str, str | None]] = []
        self.starts = 0

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Keeps identity, declaring scopes in bands out of band order."""
        self.identity = Some(identity)
        return Ok(tuple((band, self.logged(band)) for band in (Band.TIMERS, Band.GROUPS, Band.INTERFACE)))

    @contextmanager
    def logged(self, band: Band) -> Generator[None]:
        """Logs the band's scope entry and exit."""
        with ExitStack() as stack:
            self.events.append(f"enter {band.name}")
            stack.callback(self.events.append, f"exit {band.name}")
            yield

    def loaded(self, path: Option[Path]) -> Outcome[None]:
        """Keeps the loaded file path."""
        self.loads.append(path)
        return Ok(None)

    def moved(self, _scene: bpy.types.Scene, updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
        """Keeps each moved object's name and change kinds."""
        for update in updates:
            changed: bpy.types.ID = update.id  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            self.moves.append((changed.name, Change.of(update).name))
        return Ok(None)

    def started(self) -> Outcome[None]:
        """Counts startup runs."""
        self.starts += 1
        return Ok(None)


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [BODIES]


def required(*_: object) -> Outcome[None]:
    """Refuses every load for a requirement it lacks."""
    return Error(Fault.of(Reason.REQUIRED, "a fixture strip"))


def written(kept: Callable[[str], None], text: str) -> Outcome[Option[str]]:
    """Keeps one deferred write through its keeper."""
    kept(text)
    return Ok(Nothing)


@Operator.of(Empty)
def orphaned(_call: Call[Empty]) -> Outcome[Answer]:
    """Never runs, its operator released when its scope refuses."""
    return Ok(Done())


# --- [READS]


def _registered() -> dict[str, bool]:
    """Answers which fixture classes the host holds."""
    return {cls.__name__: "bl_rna" in vars(cls) for cls in (ChildPanel, ParentPanel, Outer, Inner, DEFERRED)}


def _handlers() -> tuple[int, int]:
    """Counts load and depsgraph handlers."""
    return len(bpy.app.handlers.load_post), len(bpy.app.handlers.depsgraph_update_post)


# --- [PROBES]


def names(identity: Identity) -> dict[str, Outcome[str]]:
    """Builds the host name of one key under every named base."""
    bases = (
        bpy.types.Panel,
        bpy.types.Menu,
        bpy.types.Header,
        bpy.types.UIList,
        bpy.types.AssetShelf,
        bpy.types.FileHandler,
        bpy.types.GizmoGroup,
        bpy.types.Gizmo,
        bpy.types.RenderEngine,
        bpy.types.Operator,
    )
    return {base.__name__: identity.name(base, "main") for base in bases} | {"PropertyGroup": identity.name(bpy.types.PropertyGroup, "main")}


def refusals(identity: Identity) -> dict[str, object]:
    """Builds names past the host length, keys outside lowercase snake case, and one valid identifier."""
    return {"long": identity.name(bpy.types.Panel, "a" * 60), "key": identity.identifier("Bad Key"), "identifier": identity.identifier("amount")}


def read(identity: Identity) -> dict[str, object]:
    """Reads the identity's manifest and user folder, and a package Blender holds as no extension."""
    manifest = identity.manifest
    return {"package": identity.package, "manifest": (manifest.id, manifest.name, manifest.version, manifest.website), "folder": identity.folder.parts[-2:], "foreign": Identity.read("blender.bpy")}


def foreign(identity: Identity) -> object:
    """Reads the add-on preferences as a class the extension never registered."""

    class Foreign(bpy.types.AddonPreferences):
        """Preferences class no extension registers."""

    return identity.preferences(Foreign)


def ordered(_identity: Identity) -> dict[str, object]:
    """Reads the band scopes' entry order and the classes registered out of dependency order."""
    return {"events": tuple(WITNESS.events), "registered": _registered(), "parent": definition(ChildPanel).identifier}


def texts(identity: Identity) -> dict[str, object]:
    """Translates the message under the French interface, restoring the language after, and reads the manual patterns under the manifest website, the preferences class, and the held resource."""
    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    view = preferences.view
    with ExitStack() as stack:
        stack.callback(setattr, view, "language", view.language)
        stack.callback(setattr, view, "use_translate_interface", view.use_translate_interface)
        view.language, view.use_translate_interface = "fr_FR", True
        translated = bpy.app.translations.pgettext_iface(GREETING)
    mappings: Iterator[tuple[str, Sequence[tuple[str, str]]]] = bpy.utils.manual_map()  # type: ignore[func-returns-value, assignment]  # ty: ignore[invalid-assignment]
    return {
        "translated": translated,
        "patterns": tuple(pattern for prefix, patterns in mappings for pattern in patterns if prefix == identity.manifest.website),
        "preferences": identity.preferences(bpy.types.AddonPreferences).map(lambda held: type(held).__name__),
        "tally": identity.held.get(TALLY),
    }


def depsgraph(_identity: Identity) -> tuple[tuple[str, str | None], ...]:
    """Moves a new object, answering the transform updates the filtered handler passed on."""
    obj = bpy.data.objects.new("Probe", bpy.data.meshes.new("Probe"))
    scene: bpy.types.Scene = bpy.context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    layer: bpy.types.ViewLayer = bpy.context.view_layer  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    scene.collection.objects.link(obj)
    layer.update()
    WITNESS.moves.clear()
    obj.location.x += 1.0
    layer.update()
    return tuple(WITNESS.moves)


def scened(identity: Identity) -> bool:
    """Answers whether the context scene is one of the extension's runtime scenes."""
    scene: bpy.types.Scene = bpy.context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return runtime(identity, scene)


def levels(_identity: Identity) -> dict[str, str]:
    """Reads the report type of every reason."""
    return {reason.name: level(Fault.of(reason, "fixture")[0]) for reason in Reason}


def held(identity: Identity) -> tuple[str, ...]:
    """Holds three faults, two equal, answering the report types of the entries they added."""
    before = len(identity.reports.entries)
    absent, failed = Fault.of(Reason.ABSENT, "fixture")[0], Fault.of(Reason.FAILED, "fixture")[0]
    identity.reports.hold((absent, absent, failed))
    return tuple(kind for kind, _ in identity.reports.entries[before:])


def queued(identity: Identity) -> dict[str, object]:
    """Queues a three-step request, answering the steps applied."""
    steps: list[int] = []
    return {"outcome": identity.reports.queued(Steps(3, steps.append)), "steps": tuple(steps)}


def deferred(identity: Identity) -> tuple[str, ...]:
    """Calls a deferred write twice inside its scope and once after, answering the writes applied."""
    writes: list[str] = []
    with identity.reports.deferred(partial(written, writes.append)) as later:
        later("inside")
        later("inside")
    later("after")
    return tuple(writes)


def reloaded(identity: Identity) -> dict[str, object]:
    """Holds a fault, loads the startup file, and answers the loads, startup runs, report types held after, and the resource value before and after its renewal."""
    WITNESS.loads.clear()
    identity.reports.hold(Fault.of(Reason.ABSENT, "before load"))
    before = identity.held.get(TALLY)
    answer = bpy.ops.wm.read_homefile(use_empty=True)
    return {"answer": answer, "loads": tuple(WITNESS.loads), "starts": WITNESS.starts, "entries": tuple(kind for kind, _ in identity.reports.entries), "tally": (before, identity.held.get(TALLY))}


# --- [RELEASES]


def order() -> dict[str, object]:
    """Reads the band scopes' entry and exit order and the classes the host still holds."""
    return {"events": tuple(WITNESS.events), "registered": _registered(), "handlers": _handlers() == BASELINE}


def withdrawn() -> dict[str, object]:
    """Reads the texts after release."""
    return WITNESS.identity.map(texts).default_value({})


def refusing() -> dict[str, dict[str, object]]:
    """Registers scopes whose panel the host refuses after their operator registered, one naming a parent no registration holds and one declaring no draw callback, answering the error type each registration raised and what its unwinding left."""

    def refused(panel: type[bpy.types.Panel]) -> dict[str, object]:
        raised = catch(Registration(PACKAGE, (orphaned, panel)).register, exception=Exception)().swap().map(lambda error: type(error).__name__).default_value("")
        return {"raised": raised, "operators": tuple(dir(OPERATORS)), "panel": "bl_rna" in vars(panel), "handlers": _handlers() == BASELINE}

    return {"orphan": refused(Orphan), "drawless": refused(Drawless)}


# --- [COMPOSITION] ----------------------------------------------------------------------

WITNESS: Final = Witness()
TALLY: Final = Resource[int](made=lambda _identity: nullcontext(0), renewed=Some(lambda count: count + 1))

rows: Final = (
    WITNESS,
    Translations(table={"fr_FR": {("*", GREETING): "Bonjour Fixture"}}),
    ChildPanel,
    ParentPanel,
    Outer,
    Inner,
    (Band.INTERFACE, lambda: Ok((DEFERRED,))),
    Handler(handlers=bpy.app.handlers.load_post, body=partial(located, WITNESS.loaded)),
    Handler(handlers=bpy.app.handlers.load_post, body=required),
    Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.Object,), WITNESS.moved, changes=Some(Change.TRANSFORM))),
    Startup(body=WITNESS.started),
    TALLY,
    Timer(body=lambda: Ok(Nothing), first_interval=1.0),
    Subscription(owner=bpy.types.Scene, setting=bpy.types.Scene.bl_rna.properties["frame_current"], body=lambda: Ok(None)),
)
PROBES: Final[tuple[Callable[[Identity], object], ...]] = (names, refusals, read, foreign, ordered, texts, depsgraph, scened, levels, held, queued, deferred, reloaded)
RELEASED: Final[tuple[Callable[[], object], ...]] = (order, withdrawn, refusing)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PROBES", "RELEASED", "WITNESS", "rows"]
