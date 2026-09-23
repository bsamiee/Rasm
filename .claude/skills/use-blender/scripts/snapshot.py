# ast-grep-ignore: no-json-codec, no-stdlib-record
# mypy: disable-error-code="truthy-bool"
"""Write the evaluated state of the scene to `.artifacts/blender/<name>.json` and name what changed since an earlier snapshot, run inside Blender through `runpy.run_path`."""

from collections.abc import Mapping
from dataclasses import asdict, dataclass
import hashlib
import json
from pathlib import Path
import runpy
from typing import cast

import bpy
from bpy_extras import anim_utils
import numpy as np

# --- [TYPES] ----------------------------------------------------------------------------

type Outcome = Snapshot | UnknownObjects | MissingSnapshot

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True)
class Geometry:
    """Hash over realized positions and instance transforms, with the element counts of the evaluated geometry set."""

    shape: str
    vertices: int
    faces: int
    curve_points: int
    cloud_points: int
    instances: int


@dataclass(frozen=True)
class Channel:
    """Key count, interpolations, and a hash over key coordinates of one F-curve."""

    keys: int
    interpolations: list[str]
    digest: str


@dataclass(frozen=True)
class Animated:
    """Action and slot animating the object, with its channels by data path and array index."""

    action: str
    slot: str
    channels: dict[str, Channel]


@dataclass(frozen=True)
class Unassigned:
    """Action on an object whose slot is unset, with the slots that would animate it."""

    action: str
    suitable: list[str]


@dataclass(frozen=True)
class Modifier:
    """Modifier name, type, and visibility."""

    name: str
    type: str
    show_viewport: bool
    show_render: bool


@dataclass(frozen=True)
class Constraint:
    """Constraint name, type, and whether it is enabled."""

    name: str
    type: str
    enabled: bool


@dataclass(frozen=True)
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


@dataclass(frozen=True)
class SceneState:
    """Scene settings, orphan count, linked library files, and missing file paths."""

    name: str
    frame: int
    engine: str
    camera: str | None
    unit_system: str
    length_unit: str
    scale_length: float
    orphans: int
    library_files: list[str]
    missing: list[str]


@dataclass(frozen=True)
class State:
    """JSON a snapshot file holds: the scene, one record per object, the count per `bpy.data` collection, and one hash per node tree."""

    scene: dict[str, object]
    objects: dict[str, dict[str, object]]
    datablocks: dict[str, int]
    trees: dict[str, str]


@dataclass(frozen=True)
class Comparison:
    """Objects present in one snapshot alone, and before and after of each changed object field, scene field, datablock count, and tree hash."""

    since: str
    added: tuple[str, ...]
    removed: tuple[str, ...]
    changed: dict[str, dict[str, tuple[object, object]]]
    scene: dict[str, tuple[object, object]]
    datablocks: dict[str, tuple[int | None, int | None]]
    trees: dict[str, tuple[str | None, str | None]]


@dataclass(frozen=True)
class Snapshot:
    """Written file, object count, hash of the file, and the comparison when `since` named an earlier snapshot."""

    path: str
    objects: int
    digest: str
    comparison: Comparison | None


# --- [ERRORS] ---------------------------------------------------------------------------


@dataclass(frozen=True)
class UnknownObjects:
    """Object names absent from the scene."""

    names: tuple[str, ...]


@dataclass(frozen=True)
class MissingSnapshot:
    """Snapshot name with no file under `.artifacts/blender/`."""

    name: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def snapshot(name: str, objects: tuple[str, ...] = (), since: str | None = None) -> Outcome:
    """Write the state of the named or every object with the scene, datablocks, and node trees, then compare with `since`."""
    path = next(p for p in Path(__file__).resolve().parents if (p / ".git").exists()) / ".artifacts" / "blender" / f"{name}.json"
    scene = cast("bpy.types.Scene", bpy.context.scene)
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

    table = np.array(
        [
            (owner.name, np.array(i.matrix_world), np.array(body.bound_box))
            for i in depsgraph.object_instances
            if (body := i.object) is not None and (owner := i.parent if i.is_instance else body) is not None
        ],
        dtype=[("owner", object), ("matrix", np.float64, (4, 4)), ("box", np.float64, (8, 3))],
    )
    solid = table[np.ptp(table["box"], axis=1).any(axis=1)]
    corners = solid["box"] @ solid["matrix"][:, :3, :3].mT + solid["matrix"][:, None, :3, 3]
    owners, index = np.unique(solid["owner"], return_inverse=True)
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
        return {k: (a.get(k), b.get(k)) for k in sorted(a.keys() | b.keys()) if a.get(k) != b.get(k)}

    nodes = runpy.run_path(str(Path(__file__).with_name("nodes.py")))
    now = State(
        asdict(
            SceneState(
                scene.name,
                scene.frame_current,
                scene.render.engine,
                scene.camera.name if scene.camera else None,
                scene.unit_settings.system,
                scene.unit_settings.length_unit,
                scene.unit_settings.scale_length,
                sum(block.users == 0 for block in bpy.data.all_ids),
                sorted(library.filepath for library in bpy.data.libraries),
                sorted(p for p in bpy.utils.blend_paths(absolute=True) if not Path(p).exists()),
            )
        ),
        {o.name: asdict(state(o)) for o in ([scene.objects[n] for n in objects] or scene.objects)},
        {p.identifier: len(getattr(bpy.data, p.identifier)) for p in bpy.data.bl_rna.properties if isinstance(p, bpy.types.CollectionProperty)},
        {n: digest(json.dumps(asdict(nodes["record"](owner, tree)), sort_keys=True).encode()) for n, (owner, tree) in nodes["trees"]().items()},
    )

    def compare(label: str) -> Comparison:
        before = State(**json.loads(path.with_stem(label).read_text(encoding="utf-8")))
        return Comparison(
            label,
            tuple(sorted(now.objects.keys() - before.objects.keys())),
            tuple(sorted(before.objects.keys() - now.objects.keys())),
            {n: delta(before.objects[n], now.objects[n]) for n in sorted(before.objects.keys() & now.objects.keys()) if before.objects[n] != now.objects[n]},
            delta(before.scene, now.scene),
            delta(before.datablocks, now.datablocks),
            delta(before.trees, now.trees),
        )

    comparison = None if since is None else compare(since)
    text = json.dumps(asdict(now), sort_keys=True, indent=1)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8")
    return Snapshot(str(path), len(now.objects), digest(text.encode()), comparison)


def as_result(value: Outcome) -> dict[str, object]:
    """`result` dict for `execute_blender_code`, the case name under `kind`."""
    return {"kind": type(value).__name__, **asdict(value)}


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
    "as_result",
    "snapshot",
]
