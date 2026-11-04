"""Native RNA values, path grammar, enum identifiers, and stored references."""

from contextlib import ExitStack
from functools import partial
from itertools import permutations
from operator import getitem, setitem
from pathlib import Path
from typing import Final

import bpy
from expression import Some
from mathutils import Matrix
import msgspec
import numpy as np
import pytest

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import definition, parsed, rna
from blender.bpy.data.rna.properties import choices, EnumItem
from blender.bpy.data.rna.restoration import native
from blender.bpy.data.rna.snapshots import applied, Snapshot
from blender.bpy.settings.attachments import Attachment, Stored
from blender.bpy.settings.presets import Preset
from blender.records.faults import Nothing, Ok, Reason
from blender.records.trees import Tree
from tests.python.libs.blender.bpy.background.session import Acquired

# --- [CONSTANTS] ------------------------------------------------------------------------

_EXTERNAL: Final = ("external_188781", "external_326020")
_PERSISTED: Final = 13403807

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("identifiers", [(_EXTERNAL[0],), (_EXTERNAL[1],), (_EXTERNAL[0], _EXTERNAL[0])])
def test_external_enum_identifiers_keep_persisted_values(identifiers: tuple[str, ...]) -> None:
    """Generated identifiers keep their persisted numbers and repeated identical rows."""
    rows = tuple((identifier, identifier, "") for identifier in identifiers)
    assert tuple(item[4] for item in choices(iter(rows))) == (_PERSISTED,) * len(rows)


@pytest.mark.parametrize("identifiers", list(permutations(_EXTERNAL)))
def test_external_enum_collision_refuses_ambiguity(identifiers: tuple[str, ...]) -> None:
    """Distinct colliding identifiers are refused in either order."""
    rows = tuple((identifier, identifier, "") for identifier in identifiers)
    with pytest.raises(ValueError, match=identifiers[0]) as refused:
        choices(iter(rows))
    assert set(refused.value.args[0]) == set(identifiers)


@pytest.mark.parametrize("identifier", list(_EXTERNAL))
def test_native_enum_aliases_remain_distinct_from_external_collisions(acquired: Acquired, identifier: str) -> None:
    """Native aliases retain their shared value while an external collision is refused in every input order."""

    class NativeAliases(bpy.types.PropertyGroup):
        """Native identifiers deliberately sharing one persisted enum value."""

        choice: bpy.props.EnumProperty(items=(("A", "First", "First native alias", "NONE", _PERSISTED), ("B", "Second", "Second native alias", "NONE", _PERSISTED)))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    with acquired((NativeAliases,)):
        prop: bpy.types.EnumProperty = NativeAliases.bl_rna.properties["choice"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        native = tuple(prop.enum_items_static)
        mixed: tuple[bpy.types.EnumPropertyItem | tuple[str, str, str], ...] = (*native, (identifier, identifier, ""))
        for rows in permutations(mixed):
            with pytest.raises(ValueError, match=identifier):
                choices(iter(rows))
        expected = tuple((item.identifier, item.name, item.description, item.icon, item.value) for item in native)
        assert tuple(map(choices, permutations(native))) == tuple(permutations(expected))


def test_preset_native_prefix_refuses_a_generated_zero_collision(tmp_path: Path) -> None:
    """A persisted zero identifier cannot alias explicit native preset rows."""
    identifier = "external_zero_@@@@@@@@@@@AABABBC@BCBAA"
    sources = {identifier: tmp_path / f"{identifier}.json"}
    prefix = (("UNPICKED", "No preset", "", "NONE", 0), ("ALIAS", "Native alias", "", "NONE", 0))
    assert Preset.entries(sources)[0][4] == 0
    assert Preset.entries({}, prefix=prefix) == prefix
    with pytest.raises(ValueError, match=identifier) as refused:
        Preset.entries(sources, prefix=prefix)
    assert set(refused.value.args[0]) == {prefix[0][0], identifier}


def test_generated_preset_refuses_a_native_prefix_identifier(tmp_path: Path) -> None:
    """A generated preset cannot reuse a native identifier while explicitly numbered native aliases remain accepted."""
    identifier = "NONE"
    prefix: EnumItem = (identifier, "No preset", "", "NONE", 0)
    sources = {identifier: tmp_path / f"{identifier}.json"}
    (generated,) = Preset.entries(sources)
    native = prefix, generated
    assert tuple(map(choices, permutations(native))) == tuple(permutations(native))
    mixed: tuple[EnumItem | tuple[str, str, str], ...] = (prefix, generated[:3])
    for rows in permutations(mixed):
        with pytest.raises(ValueError, match=identifier) as refused:
            choices(iter(rows))
        assert refused.value.args[0][0] == identifier
        assert set(refused.value.args[0][1:]) == {prefix[4], generated[4]}
    with pytest.raises(ValueError, match=identifier):
        Preset.entries(sources, prefix=(prefix,))


def test_stored_native_reference_preserves_the_callers_extension_hook() -> None:
    """Stored ID references resolve natively while nonreserved MessagePack extensions reach the supplied hook."""
    seen: list[tuple[int, bytes]] = []

    def extended(code: int, data: memoryview) -> tuple[int, bytes]:
        value = code, bytes(data)
        seen.append(value)
        return value

    with ExitStack() as release:
        owner = bpy.data.objects.new("Storage owner", None)
        target = bpy.data.objects.new("Storage target", None)
        release.callback(bpy.data.batch_remove, ids=(owner, target))
        external = msgspec.msgpack.Ext(7, b"external")
        expected = external.code, bytes(external.data)
        store = Stored(name="native_store", decoder=msgspec.msgpack.Decoder(dict[str, object], ext_hook=extended))
        assert store.write(owner, {"target": target, "custom": external}) == Ok(None)
        record = store.read(owner).default_with(lambda faults: pytest.fail(repr(faults))).default_with(lambda: pytest.fail("Stored record is absent"))
        assert record["target"] == target
        assert record["custom"] == expected
        assert seen == [expected]
        assert store.removed(owner) == Ok(value=True)
        assert store.read(owner) == Ok(Nothing)
        assert store.removed(owner) == Ok(value=False)


def test_vector_writes_preserve_native_updates_and_scalar_coordinates(acquired: Acquired) -> None:
    """Whole and element writes match native setters, callback counts, prior values, and scalar coordinate types."""
    updates: list[bpy.types.PropertyGroup] = []

    def updated(owner: bpy.types.PropertyGroup, _context: bpy.types.Context) -> None:
        """Records the actual native property owner reached by RNA updates."""
        updates.append(owner)

    class Vectors(bpy.types.PropertyGroup):
        """Native vectors with distinct scalar coordinates."""

        integer: bpy.props.IntVectorProperty(size=3, update=updated)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        real: bpy.props.FloatVectorProperty(size=3, update=updated)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        flags: bpy.props.BoolVectorProperty(size=3, update=updated)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    attachment = Attachment(key="vector_coordinates", owner=bpy.types.Scene, form=Vectors)
    with acquired((attachment,)) as identity, ExitStack() as release:
        scene = bpy.data.scenes.new("Native vector coordinates")
        reference_scene = bpy.data.scenes.new("Native vector reference")
        release.callback(bpy.data.batch_remove, ids=(scene, reference_scene))
        vectors = attachment.read(identity, scene).default_with(lambda faults: pytest.fail(repr(faults)))
        reference = attachment.read(identity, reference_scene).default_with(lambda faults: pytest.fail(repr(faults)))
        for name, kind, values in (("integer", bpy.types.IntProperty, (3, 5, 7)), ("real", bpy.types.FloatProperty, (-2.5, 1.25, 9.5)), ("flags", bpy.types.BoolProperty, (True, False, True))):
            index = 1
            array = getattr(reference, name)
            for coordinates, value, read_reference, write_reference in (
                ((), values, partial(tuple, array), partial(setattr, reference, name, values)),
                ((index,), values[0], partial(getitem, array, index), partial(setitem, array, index, values[0])),
            ):
                prior = read_reference()
                updates.clear()
                write_reference()
                count = len(updates)
                assert count > 0
                assert updates == [reference] * count
                updates.clear()
                descriptor = Descriptor.resolve(vectors, (name, *coordinates), kind).default_with(lambda faults: pytest.fail(repr(faults)))
                assert descriptor.write(vectors, value) == Ok(Some(prior))
                assert updates == [vectors] * count
                np.testing.assert_array_equal(np.asarray(vectors.path_resolve(name)), np.asarray(reference.path_resolve(name)))
            np.testing.assert_array_equal(Descriptor.resolve(vectors, (name,), kind).default_with(lambda faults: pytest.fail(repr(faults))).read(vectors), np.asarray(vectors.path_resolve(name)))
            for index in range(len(values)):
                native = vectors.path_resolve(f"{name}[{index}]")
                actual = Descriptor.resolve(vectors, (name, index), kind).default_with(lambda faults: pytest.fail(repr(faults))).read(vectors)
                assert actual == native
                assert type(actual) is type(native)


def test_matrix_values_match_raw_native_storage_and_evaluated_writes() -> None:
    """Matrix reads match native storage and a descriptor write produces the independently assigned native transform."""
    with ExitStack() as release:
        scene = bpy.data.scenes.new("Native matrix coordinates")
        owner = bpy.data.objects.new("Descriptor transform", None)
        reference = bpy.data.objects.new("Native reference transform", None)
        release.callback(bpy.data.batch_remove, ids=(owner, reference, scene))
        scene.collection.objects.link(owner)
        scene.collection.objects.link(reference)
        reference.matrix_world = Matrix(((0, -3, 0, 5), (2, 0, 0, 7), (0, 0, 4, 11), (0, 0, 0, 1)))
        layer = scene.view_layers[0]
        layer.update()
        descriptor = Descriptor.resolve(reference, ("matrix_world",), bpy.types.FloatProperty).default_with(lambda faults: pytest.fail(repr(faults)))
        raw = np.empty(descriptor.prop.array_length, np.float32)
        native: bpy.types.bpy_prop_array = reference.path_resolve("matrix_world", False)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]  # ruff: ignore[boolean-positional-value-in-call]
        native.foreach_get(raw)
        transform = reference.matrix_world.copy()
        np.testing.assert_array_equal(descriptor.read(reference), raw)
        for row, column in np.ndindex(np.asarray(reference.matrix_world).shape):
            assert Descriptor.resolve(reference, ("matrix_world", row, column), bpy.types.FloatProperty).default_with(lambda faults: pytest.fail(repr(faults))).read(
                reference
            ) == reference.path_resolve(f"matrix_world[{row}][{column}]")
        before = np.empty_like(raw)
        own: bpy.types.bpy_prop_array = owner.path_resolve("matrix_world", False)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]  # ruff: ignore[boolean-positional-value-in-call]
        own.foreach_get(before)
        written = Descriptor.resolve(owner, ("matrix_world",), bpy.types.FloatProperty).default_with(lambda faults: pytest.fail(repr(faults))).write(owner, tuple(map(float, raw)))
        assert written == Ok(Some(tuple(map(float, before))))
        reference.matrix_world = transform
        layer.update()
        with bpy.context.temp_override(scene=scene, view_layer=layer):
            graph = bpy.context.evaluated_depsgraph_get()
            np.testing.assert_array_equal(np.asarray(owner.evaluated_get(graph).matrix_world), np.asarray(reference.evaluated_get(graph).matrix_world))


def test_snapshot_application_writes_a_resolved_owner_without_skipping_it() -> None:
    """A root member writes through its resolved native owner and reports no skipped owner."""
    with ExitStack() as release:
        item = bpy.data.objects.new("Resolved snapshot owner", None)
        release.callback(bpy.data.objects.remove, item)
        path, wanted = ("location",), (4.0, 5.0, 6.0)
        report = Snapshot.of(bpy.types.Object, {path: wanted}).apply(item)
        assert report.outcome == Ok(None)
        assert report.written == (path,)
        assert report.skipped == ()
        assert item.location.to_tuple() == wanted


def test_snapshot_application_skips_an_absent_parent_without_writing_the_root() -> None:
    """An absent pointer owner is skipped once while unrelated native root values remain unchanged."""
    with ExitStack() as release:
        item = bpy.data.objects.new("Absent snapshot parent", None)
        release.callback(bpy.data.objects.remove, item)
        origin, parent = (1.0, 2.0, 3.0), ("parent",)
        item.location = origin
        report = Snapshot.of(bpy.types.Object, {(*parent, "location"): (4.0, 5.0, 6.0)}).apply(item)
        assert report.outcome == Ok(None)
        assert report.written == ()
        assert report.skipped == (parent,)
        assert item.location.to_tuple() == origin


def test_native_selected_curve_point_captures_only_its_enclosing_mapping() -> None:
    """A selected point captures its whole enclosing compound and restores unselected native curve data."""
    with ExitStack() as release:
        tree: bpy.types.ShaderNodeTree = bpy.data.node_groups.new("Selected curve compound", "ShaderNodeTree")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        release.callback(bpy.data.node_groups.remove, tree)
        node: bpy.types.ShaderNodeRGBCurve = tree.nodes.new(definition(bpy.types.ShaderNodeRGBCurve).identifier)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        mapping: bpy.types.CurveMapping = node.mapping  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        curve = mapping.curves[-1]
        selected, unselected = curve.points[-1].location.to_tuple(), mapping.curves[0].points[0].location.to_tuple()
        prop = definition(bpy.types.ShaderNodeRGBCurve).properties["mapping"]
        descriptor = Descriptor.resolve(node, (prop.identifier, "curves", len(mapping.curves) - 1, "points", len(curve.points) - 1, "location"), bpy.types.FloatProperty).default_with(
            lambda faults: pytest.fail(repr(faults))
        )
        captured = native(node, Some((descriptor,))).default_with(lambda faults: pytest.fail(repr(faults)))
        assert tuple(name for _, owner in captured.rows for name, _ in owner.rows) == ((prop.identifier,),)
        compound = captured.rows[0][1].rows[0][1][1]
        assert isinstance(compound, Tree)
        curve.points[-1].location[:] = (1.0, 0.75)
        mapping.curves[0].points[0].location[:] = (0.0, 0.25)
        mapping.update()
        report = applied(node, compound, lambda _, row: row[1])
        assert report.outcome == Ok(None)
        assert report.skipped == ()
        assert mapping.curves[-1].points[-1].location.to_tuple() == selected
        assert mapping.curves[0].points[0].location.to_tuple() == unselected


@pytest.mark.parametrize("index", [False, True])
def test_boolean_path_steps_use_the_native_numeric_grammar(*, index: bool) -> None:
    """A typed boolean integer step reaches a native coordinate while textual boolean subscripts are refused."""
    with ExitStack() as release:
        obj = bpy.data.objects.new("Native path grammar", None)
        release.callback(bpy.data.objects.remove, obj)
        obj.location = (3, 5, 7)
        assert obj.path_resolve(rna(("location", index))) == obj.location[index]
        external = f"location[{index}]"
        with pytest.raises(ValueError, match="location"):
            obj.path_resolve(external)
        faults = parsed(external).swap().default_with(lambda value: pytest.fail(repr(value)))
        assert len(faults) == 1
        fault = faults[0]
        assert fault.reason is Reason.UNREADABLE
        assert fault.subject == external
