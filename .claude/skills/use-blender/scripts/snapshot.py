# ty: ignore[redundant-condition-strict, unresolved-attribute]
# mypy: disable-error-code="attr-defined, union-attr"
"""Evaluated scene state written to `.artifacts/blender/<name>.json`, compared against an earlier snapshot."""

from collections.abc import Sequence
import hashlib
from pathlib import Path
from typing import Any

import attrs
import bpy
from bpy_extras import anim_utils
from nodes import Digest, record, trees
import numpy as np
from numpy.typing import NDArray
from results import artifacts, collect_faults, Fault, JSON, Resolved, unknown
from rna import DIGITS, plain, stored
from scene import bounds, drawings, points

# --- [TYPES] ----------------------------------------------------------------------------

type Animation = Animated | Unassigned | None
type Change = dict[str | int, Change] | tuple[object, object]

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Geometry:
    """Hash over realized positions, stroke points, and instance transforms, with the element counts of the evaluated geometry set."""

    shape: str
    vertices: int
    faces: int
    curve_points: int
    cloud_points: int
    strokes: int
    instances: int


@attrs.frozen
class Channel:
    """Key count, interpolations, and a hash over key and handle coordinates of one F-curve."""

    keys: int
    interpolations: list[str]
    shape: str


@attrs.frozen
class Animated:
    """Action and slot animating the object, with its channels by data path and array index."""

    action: str
    slot: str
    channels: dict[str, Channel]


@attrs.frozen
class Unassigned:
    """Action on an object with no slot assigned, with the slots that can animate it."""

    action: str
    suitable: list[str]


@attrs.frozen
class Modifier:
    """Modifier name, type, visibility, and the stored settings its type adds, Geometry Nodes inputs included."""

    name: str
    type: str
    show_viewport: bool
    show_render: bool
    settings: dict[str, object]


@attrs.frozen
class Constraint:
    """Constraint name, type, and enabled state."""

    name: str
    type: str
    enabled: bool


@attrs.frozen
class ObjectState:
    """World transform, evaluated bounds with instances, geometry, stacks, animation, and drivers of one object."""

    type: str
    data: str | None
    parent: str | None
    collections: list[str]
    location: list[float]
    rotation: list[float]
    scale: list[float]
    bounds: list[list[float]] | None
    visible: bool
    hide_render: bool
    materials: list[str | None]
    modifiers: list[Modifier]
    constraints: list[Constraint]
    geometry: Geometry | None
    animation: Animation
    drivers: dict[str, object]


@attrs.frozen
class SceneState:
    """Scene frame, engine, camera, unit and color management settings, orphan count, linked library files, and missing file paths."""

    name: str
    frame: int
    engine: str
    camera: str | None
    unit_settings: object
    view_settings: object
    display_settings: object
    orphans: int
    library_files: list[str]
    missing: list[str]


@attrs.frozen
class State:
    """Scene state a snapshot file holds as JSON."""

    scene: SceneState
    objects: dict[str, ObjectState]
    materials: dict[str, dict[str, object]]
    datablocks: dict[str, int]
    trees: dict[str, Digest]


@attrs.frozen
class Comparison:
    """Objects present in one snapshot alone, and the before and after of each changed value nested under its keys and list indexes."""

    added: tuple[str, ...]
    removed: tuple[str, ...]
    changed: Change


@attrs.frozen
class Snapshot:
    """Written file, object count, hash of the file, and the comparison when `since` named an earlier snapshot."""

    path: str
    objects: int
    hash: str
    comparison: Comparison | None


# --- [OPERATIONS] -----------------------------------------------------------------------


def snapshot(name: str, objects: tuple[str, ...] = (), since: str | None = None) -> Resolved[Snapshot]:
    """Return the snapshot written with the state of the named or every object, the scene, materials, datablock counts, and node trees, compared with `since`."""
    path, scene = artifacts() / f"{name}.json", bpy.context.scene
    source = None if since is None else path.with_stem(since)
    try:
        before, missing = None if source is None else JSON.loads(source.read_bytes(), dict[str, Any]), None
    except FileNotFoundError:
        before, missing = None, Fault(Path, since, tuple(sorted(p.stem for p in path.parent.glob("*.json"))))
    if faults := collect_faults(missing, unknown(scene.objects, objects)):
        return faults
    depsgraph = bpy.context.evaluated_depsgraph_get()

    def hashed(data: bytes) -> str:
        return hashlib.blake2b(data, digest_size=8).hexdigest()

    def fixed(values: Sequence[float] | NDArray[np.float32] | NDArray[np.float64]) -> NDArray[np.float64]:
        return np.round(np.asarray(values, dtype=np.float64), DIGITS) + 0.0

    def floats[T](collection: "bpy.types.bpy_prop_collection[T]", field: str, width: int) -> bytes:
        values = np.empty(len(collection) * width, dtype=np.float32)
        collection.foreach_get(field, values)
        return fixed(values).tobytes()

    def indexed(curve: bpy.types.FCurve) -> str:
        return f"{curve.data_path}[{curve.array_index}]"

    def settings(struct: "bpy.types.bpy_struct[object]", base: "type[bpy.types.bpy_struct[object]]") -> dict[str, object]:
        inherited = frozenset(base.bl_rna.properties.keys())
        return {p.identifier: plain(getattr(struct, p.identifier)) for p in struct.bl_rna.properties if p.identifier not in inherited and stored(p)}

    boxes = {owner: fixed(box).tolist() for owner, box in bounds(depsgraph, drawn=False).items()}

    def geometry(obj: bpy.types.Object) -> Geometry | None:
        try:
            found = (evaluated := obj.evaluated_get(depsgraph)).evaluated_geometry()
        except TypeError:
            return None
        mesh, curves, cloud, instances = found.mesh, found.curves, found.pointcloud, found.instances_pointcloud()
        return Geometry(
            hashed(fixed(points(evaluated, drawn=False)).tobytes() + (floats(instances.attributes["instance_transform"].data, "value", 16) if instances is not None else b"")),
            len(mesh.vertices) if mesh else 0,
            len(mesh.polygons) if mesh else 0,
            len(curves.points) if curves else 0,
            len(cloud.points) if cloud else 0,
            sum(len(drawing.strokes) for drawing in drawings(found.grease_pencil)),
            len(instances.points) if instances is not None else 0,
        )

    def channel(curve: bpy.types.FCurve) -> Channel:
        keys = curve.keyframe_points
        return Channel(len(keys), sorted({k.interpolation for k in keys}), hashed(b"".join(floats(keys, field, 2) for field in ("co", "handle_left", "handle_right"))))

    def animation(obj: bpy.types.Object) -> Animation:
        match obj.animation_data:
            case bpy.types.AnimData(action=bpy.types.Action() as action, action_slot=bpy.types.ActionSlot() as slot):
                bag = anim_utils.action_get_channelbag_for_slot(action, slot)
                return Animated(action.name, slot.identifier, {indexed(c): channel(c) for c in (bag.fcurves if bag else ())})
            case bpy.types.AnimData(action=bpy.types.Action() as action) as data:
                return Unassigned(action.name, [s.identifier for s in data.action_suitable_slots])
            case _:
                return None

    def state(obj: bpy.types.Object) -> ObjectState:
        location, rotation, scale = obj.matrix_world.decompose()
        return ObjectState(
            obj.type,
            obj.data.name if obj.data else None,
            obj.parent.name if obj.parent else None,
            sorted(c.name for c in obj.users_collection),
            fixed(location[:]).tolist(),
            fixed(rotation.to_euler()[:]).tolist(),
            fixed(scale[:]).tolist(),
            boxes.get(obj.name),
            obj.visible_get(),
            obj.hide_render,
            [s.material.name if s.material else None for s in obj.material_slots],
            [Modifier(m.name, m.type, m.show_viewport, m.show_render, settings(m, bpy.types.Modifier)) for m in obj.modifiers],
            [Constraint(c.name, c.type, c.enabled) for c in obj.constraints],
            geometry(obj),
            animation(obj),
            {indexed(d): plain(d.driver) for d in (obj.animation_data.drivers if obj.animation_data else ())},
        )

    def delta(before: object, after: object) -> Change:
        match before, after:
            case list() | tuple(), list() | tuple():
                return delta(dict(enumerate(before)), dict(enumerate(after)))
            case dict(), dict():
                return {k: change for k in dict.fromkeys([*before, *after]) if (change := delta(before.get(k), after.get(k)))}
            case _:
                return {} if before == after else (before, after)

    current = JSON.unstructure(
        State(
            SceneState(
                scene.name,
                scene.frame_current,
                scene.render.engine,
                scene.camera.name if scene.camera else None,
                plain(scene.unit_settings),
                plain(scene.view_settings),
                plain(scene.display_settings),
                sum(block.users == 0 for block in bpy.data.all_ids),
                sorted(library.filepath for library in bpy.data.libraries),
                sorted(p for p in bpy.utils.blend_paths(absolute=True) if not Path(p).exists()),
            ),
            {o.name: state(o) for o in ([scene.objects[n] for n in objects] or scene.objects)},
            {m.name: settings(m, bpy.types.ID) for m in bpy.data.materials},
            {p.identifier: len(getattr(bpy.data, p.identifier)) for p in bpy.data.bl_rna.properties if isinstance(p, bpy.types.CollectionProperty) and p.fixed_type.base is not None},
            {n: record(owner, tree) for n, (owner, tree) in trees().items()},
        )
    )

    match before:
        case None:
            comparison = None
        case _:
            earlier, later = before["objects"].keys(), current["objects"].keys()
            shared = sorted(earlier & later)
            comparison = Comparison(
                tuple(sorted(later - earlier)), tuple(sorted(earlier - later)), delta(*({**side, "objects": {n: side["objects"][n] for n in shared}} for side in (before, current)))
            )
    text = JSON.dumps(current, sort_keys=True, indent=1)
    path.write_text(text, encoding="utf-8")
    return Snapshot(str(path), len(current["objects"]), hashed(text.encode()), comparison)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Animated", "Animation", "Change", "Channel", "Comparison", "Constraint", "Geometry", "Modifier", "ObjectState", "SceneState", "Snapshot", "State", "Unassigned", "snapshot"]
