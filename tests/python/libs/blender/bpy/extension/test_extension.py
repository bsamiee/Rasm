"""Native extension acquisition, callbacks, registry lifetime, and failure unwinding."""

from collections.abc import Generator, Iterator, Mapping, Sequence
from contextlib import contextmanager, ExitStack, nullcontext
from fnmatch import fnmatchcase
from functools import partial
import gc
from tempfile import TemporaryFile
from typing import override, Self
import weakref

import bpy
import expression
from expression import Option, Some
import msgspec
import pytest

from blender.bpy.commands.operators import Answer, Call, called, Done, Empty, idname, operation, Operator
from blender.bpy.data.rna.paths import definition
from blender.bpy.data.rna.properties import Callbacks, choices, IconedItem
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Classes, Declarations, Handler, Registration, Resource, scoped, Tier, Timer, Translations
from blender.bpy.settings.attachments import Attachment
from blender.bpy.ui.elements.panels import PreferencesPage
from blender.records.faults import Error, Fault, Nothing, Ok, Outcome, Reason
from tests.python.libs.blender.bpy.background.session import Acquired

# --- [MODELS] ---------------------------------------------------------------------------


class Drawless(bpy.types.Panel):
    """Panel whose missing draw callback Blender refuses."""

    bl_idname = "FIXTURE_PT_drawless"
    bl_label = "Drawless"
    bl_space_type = "PROPERTIES"
    bl_region_type = "WINDOW"


class Orphan(bpy.types.Panel):
    """Panel whose missing native parent Blender refuses."""

    bl_idname = "FIXTURE_PT_orphan"
    bl_label = "Orphan"
    bl_space_type = "PROPERTIES"
    bl_region_type = "WINDOW"
    bl_parent_id = "FIXTURE_PT_absent"

    @override
    def draw(self, context: bpy.types.Context) -> None:
        """Draws an empty body."""


# --- [TESTS] ----------------------------------------------------------------------------
# --- [DEPENDENCIES]


def test_dependency_order_supports_native_nested_groups_and_panels(acquired: Acquired) -> None:
    """Out-of-order declarations acquire pointer targets and parent panels before their dependents."""

    class Inner(bpy.types.PropertyGroup):
        """Nested values held by pointer and collection."""

        level: bpy.props.IntProperty()  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    class Outer(bpy.types.PropertyGroup):
        """Native form depending on the inner group twice."""

        inner: bpy.props.PointerProperty(type=Inner)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        items: bpy.props.CollectionProperty(type=Inner)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    class Parent(bpy.types.Panel):
        """Parent declared after its child."""

        bl_idname = "FIXTURE_PT_parent"
        bl_label = "Parent"
        bl_space_type = "PROPERTIES"
        bl_region_type = "WINDOW"
        bl_context = "scene"

        @override
        def draw(self, context: bpy.types.Context) -> None:
            """Draws an empty parent."""

    class Child(bpy.types.Panel):
        """Child requiring its parent's native registration."""

        bl_idname = "FIXTURE_PT_child"
        bl_label = "Child"
        bl_space_type = "PROPERTIES"
        bl_region_type = "WINDOW"
        bl_context = "scene"
        bl_parent_id = Parent.bl_idname

        @override
        def draw(self, context: bpy.types.Context) -> None:
            """Draws an empty child."""

    class Deferred(Child):
        """Panel returned by a deferred declaration build."""

        bl_idname = "FIXTURE_PT_deferred"

    attachment = Attachment(key="nested", owner=bpy.types.Scene, form=Outer)
    with acquired((Child, attachment, Parent, Inner, (Tier.INTERFACE, Classes(build=lambda: Ok((Deferred,)))))) as identity, ExitStack() as release:
        scene = bpy.data.scenes.new("Nested registration")
        release.callback(bpy.data.scenes.remove, scene)
        outer = attachment.read(identity, scene).default_with(lambda faults: pytest.fail(repr(faults)))
        outer.inner.level = 13
        outer.items.add().level = 29
        assert (outer.inner.level, outer.items[0].level) == (13, 29)
        pointer: bpy.types.PointerProperty = definition(Outer).properties["inner"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        collection: bpy.types.CollectionProperty = definition(Outer).properties["items"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        assert pointer.fixed_type == definition(Inner)
        assert collection.fixed_type == definition(Inner)
        assert Child.bl_parent_id == Parent.bl_idname
        assert bpy.types.Panel.bl_rna_get_subclass_py(Child.bl_idname) is Child
        assert bpy.types.Panel.bl_rna_get_subclass_py(Deferred.bl_idname) is Deferred
    assert attachment.bound(identity).swap().default_with(lambda value: pytest.fail(repr(value)))[0].reason is Reason.ABSENT
    assert all("bl_rna" not in vars(kind) for kind in (Inner, Outer, Parent, Child, Deferred))


# --- [REGISTRIES]


def test_translation_preferences_and_manual_links_leave_with_registration(acquired: Acquired) -> None:
    """Native translation and manual registries expose acquired owners and release their entries."""
    greeting, translated = "Fixture Hello", "Bonjour Fixture"

    @Operator.of(Empty)
    def documented(_call: Call[Empty]) -> Outcome[Answer]:
        """Completes the operation reached by its native manual reference."""
        return Ok(Done())

    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    view = preferences.view
    registered: Iterator[tuple[str, Sequence[tuple[str, str]]]] = bpy.utils.manual_map()  # type: ignore[assignment, func-returns-value]  # ty: ignore[invalid-assignment]
    before = tuple((prefix, tuple(patterns)) for prefix, patterns in registered)
    with ExitStack() as release:
        release.callback(setattr, view, "language", view.language)
        release.callback(setattr, view, "use_translate_interface", view.use_translate_interface)
        view.language, view.use_translate_interface = "fr_FR", True
        with acquired((Translations(table={"fr_FR": {("*", greeting): translated}}), documented, PreferencesPage())) as identity:
            assert bpy.app.translations.pgettext_iface(greeting) == translated
            held = identity.preferences(bpy.types.AddonPreferences).default_with(lambda: pytest.fail("Registered preferences are absent"))
            assert definition(held).identifier == identity.package
            registered = bpy.utils.manual_map()  # type: ignore[assignment, func-returns-value]  # ty: ignore[invalid-assignment]
            mappings: Sequence[tuple[str, str]] = next(patterns for prefix, patterns in registered if prefix == identity.manifest.website)
            reference = f"bpy.ops.{idname(identity, documented)}"
            assert next(suffix for pattern, suffix in mappings if fnmatchcase(reference, pattern)) == f"#{documented.key}"
            assert any(fnmatchcase(f"bpy.types.{definition(held).identifier.lower()}.draw", pattern) for pattern, _ in mappings)
            assert called(operation(identity, documented).default_with(lambda faults: pytest.fail(repr(faults))), {}) == Ok(frozenset({"FINISHED"}))
        assert bpy.app.translations.pgettext_iface(greeting) == greeting
        assert identity.preferences(bpy.types.AddonPreferences) == Nothing
        assert operation(identity, documented).swap().default_with(lambda value: pytest.fail(repr(value)))[0].reason is Reason.ABSENT
    registered = bpy.utils.manual_map()  # type: ignore[assignment, func-returns-value]  # ty: ignore[invalid-assignment]
    assert tuple((prefix, tuple(patterns)) for prefix, patterns in registered) == before


# --- [REPORTS]


def test_absence_keeps_the_native_expression_singleton() -> None:
    """Shared absence and native absent operations preserve Expression's published identity."""
    assert Nothing is expression.Nothing
    assert Nothing.map(lambda _: 1) is Nothing
    assert Nothing.bind(lambda _: Some(1)) is Nothing
    assert Nothing.filter(lambda _: True) is Nothing
    assert Option.of_optional(None) is Nothing
    assert Some(1).filter(lambda _: False) is Nothing


def test_queue_passes_and_deferred_scope_apply_native_writes(acquired: Acquired) -> None:
    """Each queue pass advances waiting continuations one step and a closed deferred scope cannot write again."""

    class Advance(msgspec.Struct, frozen=True):
        """A request advancing a native scene once on each pass."""

        scene: bpy.types.Scene
        remaining: int

        def advanced(self) -> Outcome[Option[Self]]:
            """Advances the scene and leaves the remaining passes."""
            self.scene.frame_set(self.scene.frame_current + 1)
            return Ok(Some(msgspec.structs.replace(self, remaining=self.remaining - 1)) if self.remaining > 1 else Nothing)

    with acquired(()) as identity, ExitStack() as release:
        scene = bpy.data.scenes.new("Queued writes")
        release.callback(bpy.data.scenes.remove, scene)
        scene.frame_set(10)
        assert identity.reports.queued(Advance(scene, 3)) == Ok(None)
        assert scene.frame_current == 11
        assert identity.reports.ticked() == 0.0
        assert scene.frame_current == 12
        assert identity.reports.ticked() is None
        assert scene.frame_current == 13
        key = identity.identifier("deferred_count")
        scene[key] = 0
        manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        before = Option.of_optional(manager.undo_stack).map(lambda history: (history.active_index, tuple(step.name for step in history.steps)))

        def write() -> Outcome[Option[str]]:
            """Increments the native property and supplies its undo label."""
            scene[key] = int(scene[key]) + 1
            return Ok(Some("Fixture write"))

        with identity.reports.deferred(write) as later:
            later()
            assert scene[key] == 0
            assert identity.reports.ticked() is None
            assert scene[key] == 1
        later()
        assert identity.reports.ticked() is None
        assert scene[key] == 1
        assert Option.of_optional(manager.undo_stack).map(lambda history: (history.active_index, tuple(step.name for step in history.steps))) == before.map(
            lambda history: (history[0] + 1, (*history[1], "Fixture write"))
        )


def test_resources_and_callback_reports_leave_their_registration_scope(acquired: Acquired) -> None:
    """The resource owns its real file lifetime and repeated callback faults occupy one clearable report entry."""
    required = Fault.of(Reason.REQUIRED, "native callback permission")
    resource = Resource(made=lambda _identity: TemporaryFile())  # ruff: ignore[open-file-with-context-handler]
    marker = Resource[None](made=lambda _identity: nullcontext(None))
    with acquired((resource, marker)) as identity:
        held = resource[identity]
        held.write(b"resource lifetime")
        assert marker.get(identity) == Some(None)
        assert marker[identity] is None
        with acquired((resource, marker)) as concurrent:
            assert concurrent.package == identity.package
            assert concurrent is not identity
            other = resource[concurrent]
            assert other is not held
            assert marker.get(concurrent) == Some(None)
            other.write(b"independent registration")
            assert resource[identity] is held
        assert other.closed
        assert not held.closed
        assert resource.get(concurrent) == Nothing
        assert marker.get(concurrent) == Nothing
        reported = identity.reports.reported(lambda: Error(required))
        assert reported() == Nothing
        assert reported() == Nothing
        (report,) = identity.reports.entries
        assert report[0] == "ERROR_INVALID_CONTEXT"
        assert "native callback permission" in report[1]
        held.seek(0)
        assert held.read() == b"resource lifetime"
        assert identity.reports.cleared()
        assert not identity.reports.cleared()
    assert resource.get(identity) == Nothing
    assert marker.get(identity) == Nothing
    assert held.closed


def test_equal_attachments_share_the_original_callback_scope(acquired: Acquired) -> None:
    """Equal declarations acquire one original scope whose native callbacks retain their signatures and state."""
    entered: list[Identity] = []
    exited: list[Identity] = []
    key = "equal_callback_scope"

    class Scoped(bpy.types.PropertyGroup):
        """Native integer held by its registration's callback scope."""

        amount: bpy.props.IntProperty()  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    reads: list[tuple[Scoped, bool]] = []
    writes: list[tuple[Scoped, int, bool]] = []
    updates: list[tuple[Scoped, bpy.types.Scene | None, bool]] = []

    @contextmanager
    def callbacks(identity: Identity) -> Generator[Mapping[str, Callbacks[Scoped]]]:
        """Holds the original state until this registration releases its acquisition scope."""
        value, live = 3, True
        entered.append(identity)

        def read(owner: Scoped, /) -> int:
            """Returns the scoped value and records its actual native owner."""
            reads.append((owner, live))
            return value

        def write(owner: Scoped, amount: int, /) -> None:
            """Stores the native integer and records its owner while the scope is live."""
            nonlocal value
            writes.append((owner, amount, live))
            value = amount

        def updated(owner: Scoped, context: bpy.types.Context, /) -> None:
            """Records the native owner and context scene while the scope is live."""
            updates.append((owner, context.scene, live))

        try:
            yield {"amount": Callbacks[Scoped](get=read, set=write, update=updated)}
        finally:
            live = False
            exited.append(identity)

    left = Attachment(key=key, form=Scoped, owner=bpy.types.Scene, callbacks=Some(callbacks))
    right = Attachment(key=key, form=Scoped, owner=bpy.types.Scene, callbacks=Some(callbacks))
    assert left == right
    assert hash(left) == hash(right)
    with acquired((left, right)) as identity, ExitStack() as release:
        scene = bpy.data.scenes.new("Equal attachment callbacks")
        release.callback(bpy.data.scenes.remove, scene)
        group = left.read(identity, scene).default_with(lambda faults: pytest.fail(repr(faults)))
        reads.clear()
        writes.clear()
        updates.clear()
        assert entered == [identity]
        assert exited == []
        with bpy.context.temp_override(scene=scene):
            assert group.amount == 3
            assert reads[-1] == (group, True)
            group.amount = 19
            assert writes == [(group, 19, True)]
            assert updates == [(group, scene, True)]
            assert group.amount == 19
            assert reads[-1] == (group, True)
        assert exited == []
    assert entered == exited == [identity]
    assert identity.identifier(key) not in bpy.types.Scene.bl_rna.properties
    assert bpy.types.PropertyGroup.bl_rna_get_subclass_py(Scoped.__name__) is None


# --- [HOST_REFUSALS]


@pytest.mark.parametrize("panel", [Drawless, Orphan], ids=lambda panel: panel.__name__)
def test_native_panel_refusal_releases_preceding_operator(acquired: Acquired, panel: type[bpy.types.Panel]) -> None:
    """Native missing-parent and missing-draw refusals unwind the preceding operator and callbacks."""

    @Operator.of(Empty)
    def preceding(_call: Call[Empty]) -> Outcome[Answer]:
        """Completes only while its registration remains acquired."""
        return Ok(Done())

    with acquired(()) as identity:
        before = tuple(map(id, bpy.app.handlers.load_post))
        registration = Registration(identity.package, (preceding, panel))
        with pytest.raises(RuntimeError, match=panel.__name__):
            registration.register()
        assert "bl_rna" not in vars(panel)
        assert operation(identity, preceding).swap().default_with(lambda value: pytest.fail(repr(value)))[0].reason is Reason.ABSENT
        assert tuple(map(id, bpy.app.handlers.load_post)) == before


# --- [RNA]


def test_overlapping_forms_hold_only_registration_strings(acquired: Acquired) -> None:
    """Shared form registrations retain canonical enum text until their own native release."""

    class Text(str):  # ruff: ignore[subclass-builtin]
        """Callback text whose native retention a weak reference observes."""

        __slots__ = ("__weakref__",)

    class Catalog:
        """Changing numeric icons and weak references to callback text per native form."""

        def __init__(self) -> None:
            """Starts without emitted text or registered form icons."""
            self.icons: dict[type[bpy.types.PropertyGroup], int] = {}
            self.references: dict[type[bpy.types.PropertyGroup], list[tuple[weakref.ReferenceType[Text], ...]]] = {}

        def items(self, owner: bpy.types.PropertyGroup, _context: bpy.types.Context | None) -> tuple[IconedItem, ...]:
            """Emits equal identifier, label, and description text with the current numeric icon."""
            kind = type(owner)
            identifier, name, description = tuple(Text(text) for text in (kind.__name__.upper(), kind.__name__, f"{kind.__name__} selection"))
            self.references[kind].append(tuple(map(weakref.ref, (identifier, name, description))))
            return choices(((identifier, name, description, self.icons[kind], 0),))

    catalog = Catalog()

    class Shared:
        """Plain form whose bound callback transfers to each concrete registration."""

        selection: bpy.props.EnumProperty(items=catalog.items, default=0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form, invalid-argument-type]

    class LifecycleLeft(Shared, bpy.types.PropertyGroup):
        """Concrete form with an annotation dictionary to restore exactly."""

        amount: bpy.props.IntProperty()  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    class LifecycleRight(Shared, bpy.types.PropertyGroup):
        """Concrete form with no initial annotation dictionary."""

    source = Shared.__annotations__
    selection = source["selection"]
    left_source = LifecycleLeft.__annotations__
    right_source = vars(LifecycleRight).get("__annotations__")
    icons: bpy.types.EnumProperty = definition(bpy.types.UILayout).functions["prop"].parameters["icon"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    requested = tuple(icons.enum_items_static[name].value for name in ("CHECKMARK", "CANCEL"))
    forms = (LifecycleLeft, LifecycleRight)
    attachments = tuple(Attachment(key=key, form=kind, owner=bpy.types.Scene) for kind, key in zip(forms, ("lifecycle_left", "lifecycle_right")))
    catalog.icons = dict.fromkeys(forms, requested[0])
    catalog.references = {kind: [] for kind in forms}

    with ExitStack() as all_scopes:
        left, right = tuple(all_scopes.enter_context(ExitStack()) for _ in forms)
        owners = tuple(scope.enter_context(acquired((attachment,))) for scope, attachment in zip((left, right), attachments))
        names = tuple(identity.identifier(attachment.key) for identity, attachment in zip(owners, attachments))
        for kind, name in zip(forms, names):
            group = getattr(bpy.context.scene, name)
            identifier = kind.__name__.upper()
            for icon in requested:
                catalog.icons[kind] = icon
                group.selection = identifier
                assert group.selection == identifier
                assert bpy.types.UILayout.enum_item_name(group, "selection", identifier) == kind.__name__
                assert bpy.types.UILayout.enum_item_description(group, "selection", identifier) == f"{kind.__name__} selection"
                assert bpy.types.UILayout.enum_item_icon(group, "selection", identifier) == icon
        del group  # ty: ignore[possibly-unresolved-reference]
        gc.collect()
        assert Shared.__annotations__ is source
        assert source["selection"] is selection
        assert selection.keywords["items"] == catalog.items
        for kind in forms:
            first, *duplicates = catalog.references[kind]
            assert all(reference() is not None for reference in first)
            assert all(reference() is None for row in duplicates for reference in row)
        left.close()
        gc.collect()
        assert all(reference() is None for row in catalog.references[LifecycleLeft] for reference in row)
        assert names[0] not in bpy.types.Scene.bl_rna.properties
        assert bpy.types.PropertyGroup.bl_rna_get_subclass_py(LifecycleLeft.__name__) is None
        assert LifecycleLeft.__annotations__ is left_source
        assert Shared.__annotations__ is source
        group = getattr(bpy.context.scene, names[1])
        identifier = LifecycleRight.__name__.upper()
        assert group.selection == identifier
        assert bpy.types.UILayout.enum_item_name(group, "selection", identifier) == LifecycleRight.__name__
        assert bpy.types.UILayout.enum_item_icon(group, "selection", identifier) == requested[-1]
        del group
        first, *duplicates = catalog.references[LifecycleRight]
        assert all(reference() is not None for reference in first)
        assert all(reference() is None for row in duplicates for reference in row)
    gc.collect()
    for kind, name in zip(forms, names):
        assert all(reference() is None for row in catalog.references[kind] for reference in row)
        assert name not in bpy.types.Scene.bl_rna.properties
        assert bpy.types.PropertyGroup.bl_rna_get_subclass_py(kind.__name__) is None
    assert Shared.__annotations__ is source
    assert source["selection"] is selection
    assert LifecycleLeft.__annotations__ is left_source
    assert vars(LifecycleRight).get("__annotations__") is right_source


def test_register_failure_releases_acquired_rna_in_dependency_order(acquired: Acquired) -> None:
    """Native class registration failure retains its typed cause and releases acquired pointer owners first."""
    refusal = RuntimeError("class register refused after native acquisition")
    released: list[type[bpy.types.PropertyGroup]] = []

    class LifecycleDependency(bpy.types.PropertyGroup):
        """Pointer target whose RNA must outlive the failing native class."""

        value: bpy.props.IntProperty()  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

        @classmethod
        def unregister(cls) -> None:
            """Reads live native RNA while its dependent class already released."""
            assert "value" in cls.bl_rna.properties
            assert bpy.types.PropertyGroup.bl_rna_get_subclass_py(LifecycleRefusal.__name__) is None
            released.append(cls)

    class LifecycleRefusal(bpy.types.PropertyGroup):
        """Native class refusing after its pointer property registered."""

        child: bpy.props.PointerProperty(type=LifecycleDependency)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

        @classmethod
        def register(cls) -> None:
            """Refuses after native pointer acquisition."""
            pointer: bpy.types.PointerProperty = definition(cls).properties["child"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            assert pointer.fixed_type == definition(LifecycleDependency)
            assert bpy.types.PropertyGroup.bl_rna_get_subclass_py(cls.__name__) is cls
            raise refusal

        @classmethod
        def unregister(cls) -> None:
            """Reads the live native target before Blender releases the failing class."""
            pointer: bpy.types.PointerProperty = definition(cls).properties["child"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            assert pointer.fixed_type == definition(LifecycleDependency)
            assert bpy.types.PropertyGroup.bl_rna_get_subclass_py(cls.__name__) is cls
            released.append(cls)

    with acquired(()) as identity:
        with Registration(identity.package, (LifecycleRefusal, LifecycleDependency)).opened() as outcome:
            assert outcome == Error(Fault.of(Reason.UNREGISTERED, LifecycleRefusal.__name__, refusal))
            assert bpy.types.PropertyGroup.bl_rna_get_subclass_py(LifecycleRefusal.__name__) is LifecycleRefusal
            assert bpy.types.PropertyGroup.bl_rna_get_subclass_py(LifecycleDependency.__name__) is LifecycleDependency
        assert released == [LifecycleRefusal, LifecycleDependency]
        for kind in (LifecycleRefusal, LifecycleDependency):
            assert bpy.types.PropertyGroup.bl_rna_get_subclass_py(kind.__name__) is None


# --- [EXTENSION]


def test_late_refusal_unwinds_owned_resources_and_preserves_foreign_identities(acquired: Acquired) -> None:
    """Late native refusal releases attachments and exact timer callables while preserving foreign callbacks."""
    refusal = RuntimeError("late class register refused after native acquisitions")
    released: list[type[bpy.types.PropertyGroup]] = []
    handlers = bpy.app.handlers.load_post

    class Foreign:
        """Foreign native handler and timer callback owner."""

        @bpy.app.handlers.persistent  # type: ignore[untyped-decorator]  # ty: ignore[dynamic-function-decorator-return]
        def loaded(self, _path: str, _unused: object) -> None:
            """Accepts native file-load arguments."""

        def ticked(self) -> float:  # ruff: ignore[no-self-use]
            """Keeps its timer registered until its owning scope releases it."""
            return 0.0

    foreign_owner, timer_owner = Foreign(), Foreign()
    foreign, foreign_timer, owned_timer = foreign_owner.loaded, foreign_owner.ticked, timer_owner.ticked
    with acquired(()) as identity, ExitStack() as cleanup:
        name = identity.identifier("lifecycle_failure")
        handlers.append(foreign)
        cleanup.callback(handlers.remove, foreign)
        bpy.app.timers.register(foreign_timer, persistent=True)
        cleanup.callback(bpy.app.timers.unregister, foreign_timer)
        before = tuple(map(id, handlers))

        class LifecycleAcquired(bpy.types.PropertyGroup):
            """Attached settings acquired before handlers and the refusing class."""

            amount: bpy.props.IntProperty()  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

            @classmethod
            def unregister(cls) -> None:
                """Reads native resource releases before their group leaves."""
                assert name not in bpy.types.Scene.bl_rna.properties
                assert len(handlers) == len(before) + 1
                assert any(callback is foreign for callback in handlers)
                assert not bpy.app.timers.is_registered(owned_timer)
                assert not bpy.app.timers.is_registered(timer_owner.ticked)
                assert bpy.app.timers.is_registered(foreign_timer)
                released.append(cls)

        class LifecycleLateRefusal(bpy.types.PropertyGroup):
            """Late native class acquisition refusing while earlier bands remain live."""

            @classmethod
            def register(cls) -> None:
                """Refuses after native attachments, handlers, and timers acquired."""
                assert name in bpy.types.Scene.bl_rna.properties
                assert len(handlers) == len(before) + 2
                assert bpy.types.PropertyGroup.bl_rna_get_subclass_py(cls.__name__) is cls
                assert bpy.app.timers.is_registered(owned_timer)
                assert not bpy.app.timers.is_registered(timer_owner.ticked)
                assert bpy.app.timers.is_registered(foreign_timer)
                raise refusal

            @classmethod
            def unregister(cls) -> None:
                """Reads earlier native acquisitions before releasing the failed class."""
                assert name in bpy.types.Scene.bl_rna.properties
                assert len(handlers) == len(before) + 2
                assert bpy.types.PropertyGroup.bl_rna_get_subclass_py(cls.__name__) is cls
                assert bpy.app.timers.is_registered(owned_timer)
                assert not bpy.app.timers.is_registered(timer_owner.ticked)
                assert bpy.app.timers.is_registered(foreign_timer)
                released.append(cls)

        rows: Declarations = (
            Attachment(key="lifecycle_failure", form=LifecycleAcquired, owner=bpy.types.Scene),
            Handler(handlers=handlers, body=lambda: Ok(None)),
            Timer(body=lambda: Ok(Nothing)),
            (Tier.TIMERS, scoped(owned_timer.__qualname__, partial(bpy.app.timers.register, owned_timer, persistent=True), lambda _: bpy.app.timers.unregister(owned_timer))),
            (Tier.REGISTRIES, LifecycleLateRefusal),
        )
        with Registration(identity.package, rows).opened() as outcome:
            assert outcome == Error(Fault.of(Reason.UNREGISTERED, LifecycleLateRefusal.__name__, refusal))
            assert name in bpy.types.Scene.bl_rna.properties
            assert bpy.app.timers.is_registered(owned_timer)
            assert bpy.app.timers.is_registered(foreign_timer)
        assert released == [LifecycleLateRefusal, LifecycleAcquired]
        assert name not in bpy.types.Scene.bl_rna.properties
        assert tuple(map(id, handlers)) == before
        assert not bpy.app.timers.is_registered(owned_timer)
        assert bpy.app.timers.is_registered(foreign_timer)
        for kind in (LifecycleLateRefusal, LifecycleAcquired):
            assert bpy.types.PropertyGroup.bl_rna_get_subclass_py(kind.__name__) is None
    assert not bpy.app.timers.is_registered(foreign_timer)
    assert all(callback is not foreign for callback in handlers)


def test_native_depsgraph_preserves_variadic_arguments_and_bound_handler_identity(acquired: Acquired) -> None:
    """Actual dependency graph updates deliver both native arguments while foreign handlers survive release."""
    updates: list[frozenset[int]] = []
    handlers = bpy.app.handlers.depsgraph_update_post

    class Foreign:
        """Foreign bound handler observed through actual native updates."""

        def updated(self, scene: bpy.types.Scene, graph: bpy.types.Depsgraph) -> None:
            """Keeps the actual argument handles for the following library callback."""
            foreign_updates.append((self, scene, graph))

    foreign_updates: list[tuple[Foreign, bpy.types.Scene, bpy.types.Depsgraph]] = []
    owner = Foreign()

    def updated(*arguments: object) -> Outcome[None]:
        """Reads native arguments and copies changed original IDs during their callback."""
        assert len(arguments) == 2
        scene, graph = arguments
        assert isinstance(scene, bpy.types.Scene)
        assert isinstance(graph, bpy.types.Depsgraph)
        assert graph.scene == scene
        assert (owner, scene, graph) == foreign_updates[-1]
        changed: list[int] = []
        for item in graph.updates:
            block: bpy.types.ID = item.id  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            original: bpy.types.ID = block.original  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            changed.append(original.session_uid)
        updates.append(frozenset(changed))
        return Ok(None)

    foreign = owner.updated
    with ExitStack() as cleanup:
        handlers.append(foreign)
        cleanup.callback(handlers.remove, foreign)
        before = tuple(map(id, handlers))
        with acquired((Handler(handlers=handlers, body=updated),)), ExitStack() as data_scope:
            assert any(callback is foreign for callback in handlers)
            scene = bpy.data.scenes.new("Lifecycle native updates")
            layer = scene.view_layers[0]
            obj = bpy.data.objects.new("Lifecycle native update", None)
            scene.collection.objects.link(obj)
            data_scope.callback(bpy.data.batch_remove, ids=(obj, scene))
            with bpy.context.temp_override(scene=scene, view_layer=layer):
                layer.update()
                updates.clear()
                obj.location.x = 1.0
                layer.update()
            assert updates
            assert any(obj.session_uid in changed for changed in updates)
        assert tuple(map(id, handlers)) == before
        assert any(callback is foreign for callback in handlers)
    assert all(callback is not foreign for callback in handlers)
