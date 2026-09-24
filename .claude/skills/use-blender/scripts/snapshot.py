# mypy: disable-error-code="truthy-bool"
"""Write the evaluated state of the scene to `.artifacts/blender/<name>.json` and name what changed since an earlier snapshot, run inside Blender through `runpy.run_path`."""

from collections.abc import Mapping
import hashlib
from pathlib import Path
import runpy
from typing import Final

import attrs
import bpy
from bpy_extras import anim_utils
from cattrs.preconf.json import make_converter
import numpy as np

# --- [TYPES] ----------------------------------------------------------------------------

type Outcome = Snapshot | UnknownObjects | MissingSnapshot | Unset

# --- [CONSTANTS] ------------------------------------------------------------------------

JSON: Final = make_converter()

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Geometry:
    """Hash over realized positions and instance transforms, with the element counts of the evaluated geometry set."""

    shape: str
    vertices: int
    faces: int
    curve_points: int
    cloud_points: int
    instances: int


@attrs.frozen
class Channel:
    """Key count, interpolations, and a hash over key coordinates of one F-curve."""

    keys: int
    interpolations: list[str]
    digest: str


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
    """Modifier name, type, and visibility."""

    name: str
    type: str
    show_viewport: bool
    show_render: bool


@attrs.frozen
class Constraint:
    """Constraint name, type, and whether it is enabled."""

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
    animation: Animated | Unassigned | None
    drivers: dict[str, str]


@attrs.frozen
class SceneState:
    """Scene settings, color management, orphan count, linked library files, and missing file paths."""

    name: str
    frame: int
    engine: str
    camera: str | None
    unit_system: str
    length_unit: str
    scale_length: float
    display_device: str
    view_transform: str
    look: str
    exposure: float
    orphans: int
    library_files: list[str]
    missing: list[str]


@attrs.frozen
class State:
    """JSON a snapshot file holds: the scene, one record per object, the count per `bpy.data` collection, and one hash per node tree."""

    scene: SceneState
    objects: dict[str, ObjectState]
    datablocks: dict[str, int]
    trees: dict[str, str]


@attrs.frozen
class Comparison:
    """Objects present in one snapshot alone, and before and after of each changed object field, scene field, datablock count, and tree hash."""

    since: str
    added: tuple[str, ...]
    removed: tuple[str, ...]
    changed: dict[str, dict[str, tuple[object, object]]]
    scene: dict[str, tuple[object, object]]
    datablocks: dict[str, tuple[int | None, int | None]]
    trees: dict[str, tuple[str | None, str | None]]


@attrs.frozen
class Snapshot:
    """Written file, object count, hash of the file, and the comparison when `since` named an earlier snapshot."""

    path: str
    objects: int
    digest: str
    comparison: Comparison | None


# --- [ERRORS] ---------------------------------------------------------------------------


@attrs.frozen
class UnknownObjects:
    """Object names absent from the scene."""

    names: tuple[str, ...]


@attrs.frozen
class MissingSnapshot:
    """Snapshot name with no file under `.artifacts/blender/`."""

    name: str


@attrs.frozen
class Unset:
    """RNA pointer the snapshot reads that holds no value, by its path."""

    path: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def snapshot(name: str, objects: tuple[str, ...] = (), since: str | None = None) -> Outcome:
    """Write the state of the named or every object with the scene, datablocks, and node trees, then compare with `since`."""
    path = next(p for p in Path(__file__).resolve().parents if (p / ".git").exists()) / ".artifacts" / "blender" / f"{name}.json"
    if (scene := bpy.context.scene) is None:
        return Unset("context.scene")
    if (view_settings := scene.view_settings) is None:
        return Unset("scene.view_settings")
    if (display_settings := scene.display_settings) is None:
        return Unset("scene.display_settings")
    if missing := tuple(n for n in objects if n not in scene.objects):
        return UnknownObjects(missing)
    if since is not None and not path.with_stem(since).is_file():
        return MissingSnapshot(since)
    depsgraph = bpy.context.evaluated_depsgraph_get()

    def digest(data: bytes) -> str:
        return hashlib.blake2b(data, digest_size=8).hexdigest()

    def fixed(values: object) -> list[float]:
        return (np.round(np.asarray(values, dtype=np.float64), 5) + 0.0).ravel().tolist()

    def floats[T: bpy.types.bpy_struct](collection: "bpy.types.bpy_prop_collection[T]", field: str, width: int) -> bytes:
        values = np.empty(len(collection) * width, dtype=np.float32)
        collection.foreach_get(field, values)
        return (np.round(values, 5) + 0.0).tobytes()

    def indexed(curve: bpy.types.FCurve) -> str:
        return f"{curve.data_path}[{curve.array_index}]"

    solid = np.rec.fromrecords(
        [
            (owner.name, np.array(i.matrix_world), box)
            for i in depsgraph.object_instances
            if (body := i.object) is not None and (owner := i.parent if i.is_instance else body) is not None and np.ptp(box := np.array(body.bound_box), axis=0).any()
        ],
        dtype=[("owner", object), ("matrix", np.float64, (4, 4)), ("box", np.float64, (8, 3))],
    )
    corners = solid.box @ solid.matrix[:, :3, :3].mT + solid.matrix[:, None, :3, 3]
    owners, index = np.unique(solid.owner, return_inverse=True)
    low, high = np.full(((count := len(owners)), 3), np.inf), np.full((count, 3), -np.inf)
    np.minimum.at(low, index, corners.min(axis=1))
    np.maximum.at(high, index, corners.max(axis=1))
    boxes = {str(o): [fixed(a), fixed(b)] for o, a, b in zip(owners, low, high, strict=True)}

    def geometry(obj: bpy.types.Object) -> Geometry | None:
        try:
            geometry_set = obj.evaluated_get(depsgraph).evaluated_geometry()
        except TypeError:
            return None
        mesh, curves, cloud, instances = geometry_set.mesh, geometry_set.curves, geometry_set.pointcloud, geometry_set.instances_pointcloud()
        transforms = instances.attributes["instance_transform"] if instances else None
        return Geometry(
            digest(
                b"".join((
                    floats(mesh.vertices, "co", 3) if mesh else b"",
                    floats(curves.points, "position", 3) if curves else b"",
                    floats(cloud.points, "co", 3) if cloud else b"",
                    floats(transforms.data, "value", 16) if isinstance(transforms, bpy.types.Float4x4Attribute) else b"",
                ))
            ),
            len(mesh.vertices) if mesh else 0,
            len(mesh.polygons) if mesh else 0,
            len(curves.points) if curves else 0,
            len(cloud.points) if cloud else 0,
            len(instances.points) if instances else 0,
        )

    def animation(obj: bpy.types.Object) -> Animated | Unassigned | None:
        match obj.animation_data:
            case bpy.types.AnimData(action=bpy.types.Action() as action, action_slot=bpy.types.ActionSlot() as slot):
                bag = anim_utils.action_get_channelbag_for_slot(action, slot)
                channels = {
                    indexed(c): Channel(len(c.keyframe_points), sorted({k.interpolation for k in c.keyframe_points}), digest(floats(c.keyframe_points, "co", 2))) for c in (bag.fcurves if bag else ())
                }
                return Animated(action.name, slot.identifier, channels)
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
            fixed(location),
            fixed(rotation.to_euler()),
            fixed(scale),
            boxes.get(obj.name),
            obj.visible_get(),
            obj.hide_render,
            [s.material.name if s.material else None for s in obj.material_slots],
            [Modifier(m.name, m.type, m.show_viewport, m.show_render) for m in obj.modifiers],
            [Constraint(c.name, c.type, c.enabled) for c in obj.constraints],
            geometry(obj),
            animation(obj),
            {indexed(d): driver.expression for d in (obj.animation_data.drivers if obj.animation_data else ()) if (driver := d.driver)},
        )

    def delta[V](a: Mapping[str, V], b: Mapping[str, V]) -> dict[str, tuple[V | None, V | None]]:
        return {k: (old, new) for k in sorted(a.keys() | b.keys()) if (old := a.get(k)) != (new := b.get(k))}

    nodes = runpy.run_path(str(Path(__file__).with_name("nodes.py")))
    records = {n: nodes["record"](owner, tree) for n, (owner, tree) in nodes["trees"]().items()}
    if (unset := next((r for r in records.values() if isinstance(r, nodes["Unset"])), None)) is not None:
        return Unset(unset.path)
    now = State(
        SceneState(
            scene.name,
            scene.frame_current,
            scene.render.engine,
            scene.camera.name if scene.camera else None,
            scene.unit_settings.system,
            scene.unit_settings.length_unit,
            round(scene.unit_settings.scale_length, 5),
            display_settings.display_device,
            view_settings.view_transform,
            view_settings.look,
            round(view_settings.exposure, 5),
            sum(block.users == 0 for block in bpy.data.all_ids),
            sorted(library.filepath for library in bpy.data.libraries),
            sorted(p for p in bpy.utils.blend_paths(absolute=True) if not Path(p).exists()),
        ),
        {o.name: state(o) for o in ([scene.objects[n] for n in objects] or scene.objects)},
        {p.identifier: len(getattr(bpy.data, p.identifier)) for p in bpy.data.bl_rna.properties if isinstance(p, bpy.types.CollectionProperty)},
        {n: digest(JSON.dumps(r, sort_keys=True).encode()) for n, r in records.items()},
    )

    def compare(label: str) -> Comparison:
        before = JSON.loads(path.with_stem(label).read_text(encoding="utf-8"), State)
        return Comparison(
            label,
            tuple(sorted(now.objects.keys() - before.objects.keys())),
            tuple(sorted(before.objects.keys() - now.objects.keys())),
            {n: fields for n in sorted(before.objects.keys() & now.objects.keys()) if (fields := delta(attrs.asdict(before.objects[n]), attrs.asdict(now.objects[n])))},
            delta(attrs.asdict(before.scene), attrs.asdict(now.scene)),
            delta(before.datablocks, now.datablocks),
            delta(before.trees, now.trees),
        )

    comparison = None if since is None else compare(since)
    text = JSON.dumps(now, sort_keys=True, indent=1)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8")
    return Snapshot(str(path), len(now.objects), digest(text.encode()), comparison)


def as_result(value: Outcome) -> dict[str, object]:
    """`result` dict for `execute_blender_code`, the case name under `kind`."""
    return {"kind": type(value).__name__, **attrs.asdict(value)}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Animated",
    "Channel",
    "Comparison",
    "Constraint",
    "Geometry",
    "MissingSnapshot",
    "Modifier",
    "ObjectState",
    "Outcome",
    "SceneState",
    "Snapshot",
    "State",
    "Unassigned",
    "UnknownObjects",
    "Unset",
    "as_result",
    "snapshot",
]
