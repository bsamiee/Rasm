# ty: ignore[invalid-assignment, invalid-context-manager, not-subscriptable, unresolved-attribute]
# mypy: disable-error-code="attr-defined, func-returns-value, index, no-any-return, union-attr"
"""Blender's shared asset library of site planting species, rebuilt into the assets folder when its source stamp moves."""

from collections.abc import Callable
from importlib import import_module
from itertools import accumulate
from pathlib import Path
import sys
from types import ModuleType
from typing import Final
from uuid import NAMESPACE_URL, uuid5

import addon_utils
import bpy
from cattrs.preconf.json import make_converter
import numpy as np

from interface.blender.addons import loaded
from interface.render import ASSETS
from interface.report import ABSENT, digest, Kind
from interface.units import FOOT

# --- [CONSTANTS] ------------------------------------------------------------------------

LIBRARY: Final = ASSETS / "assets.blend"
STAMP: Final = "Stamp"
TOLERANCE: Final = 0.02

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [TREES]
def grown(tree: ModuleType, presets: ModuleType, preset: str, trunk_factor: float, branch_factor: float) -> object:
    """Modular Tree mesh of the preset grown as `mtree.quick_generate` grows it, trunk and branch sizes scaled by the factors."""
    trunk, branches, plant, seed, resolution = tree.TrunkFunction(), tree.BranchFunction(), tree.Tree(), 7, 1.0
    trunk.seed, branches.seed = seed, seed + 1
    presets.apply_trunk_preset(trunk, preset)
    presets.apply_preset(branches, preset)
    for key in ("length", "start_radius", "end_radius"):
        setattr(trunk, key, getattr(trunk, key) * trunk_factor)
    branches.length = tree.PropertyWrapper(tree.ConstantProperty(presets.TREE_PRESETS[preset].branches["length"] * branch_factor))
    trunk.resolution = branches.resolution = resolution
    if presets.TREE_PRESETS[preset].sub_branches:
        sub_branches = tree.BranchFunction()
        sub_branches.seed = seed + 2
        presets.apply_sub_branch_preset(sub_branches, preset)
        sub_branches.resolution = resolution
        branches.add_child(sub_branches)
    trunk.add_child(branches)
    plant.set_trunk_function(trunk)
    plant.execute_functions()
    mesher = tree.ManifoldMesher()
    mesher.radial_n_points, mesher.smooth_iterations = 8, 4
    return mesher.mesh_tree(plant)


def measured(tree: ModuleType, presets: ModuleType, preset: str, trunk_factor: float, branch_factor: float) -> tuple[float, float]:
    """Height and wider horizontal extent of the grown mesh."""
    points = np.asarray(grown(tree, presets, preset, trunk_factor, branch_factor).get_vertices()).reshape(-1, 3)
    extent = np.nanmax(points, axis=0) - np.nanmin(points, axis=0)
    return float(extent[2]), float(max(extent[0], extent[1]))


def bisected(miss: Callable[[float], float], low: float, high: float, halvings: int = 12) -> float:
    """Factor in the interval whose relative miss is within the tolerance, found by at most the given halvings."""
    middle = (low + high) / 2
    match miss(middle):
        case error if abs(error) <= TOLERANCE or halvings == 0:
            return middle
        case error if error > 0:
            return bisected(miss, low, middle, halvings - 1)
        case _:
            return bisected(miss, middle, high, halvings - 1)


def solved(measure: Callable[[float, float], tuple[float, float]], height: float, width: float, trunk: float = 1.0, branch: float = 1.0, rounds: int = 8) -> tuple[float, float]:
    """Trunk and branch factors whose grown tree matches the height and width within the tolerance, bisected round by round."""
    factors = (0.25, 4.0)
    match measure(trunk, branch):
        case grown_height, grown_width if abs(grown_height / height - 1) <= TOLERANCE and abs(grown_width / width - 1) <= TOLERANCE:
            return trunk, branch
        case grown_height, grown_width if rounds == 0:
            raise ArithmeticError(f"factors {trunk} and {branch} grow {grown_height:.2f} m by {grown_width:.2f} m after the last round and need {height:.2f} m by {width:.2f} m")
        case _:
            solved_trunk = bisected(lambda factor: measure(factor, branch)[0] / height - 1, *factors)
            return solved(measure, height, width, solved_trunk, bisected(lambda factor: measure(solved_trunk, factor)[1] / width - 1, *factors), rounds - 1)


def planted(module: str, name: str, preset: str, height: float, width: float) -> tuple[bpy.types.Object, str, str]:
    """Species mesh object grown at its solved factors, with Modular Tree's name as author and its evaluated size, vertex count, and factors."""
    tree, presets = import_module("m_tree"), import_module(f"{module}.python_classes.presets")
    trunk, branch = solved(lambda trunk, branch: measured(tree, presets, preset, trunk, branch), height, width)
    mesh = bpy.data.meshes.new(name)
    import_module(f"{module}.python_classes.mesh_utils").create_mesh_from_cpp(mesh, grown(tree, presets, preset, trunk, branch))
    target = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(target)
    target["trunk_factor"], target["branch_factor"] = trunk, branch
    dimensions = target.evaluated_get(bpy.context.evaluated_depsgraph_get()).dimensions
    size = f"{name} {dimensions.z / FOOT:.1f} ft by {max(dimensions.x, dimensions.y) / FOOT:.1f} ft, {len(mesh.vertices):,} vertices, factors {trunk:.4f} and {branch:.4f}"
    return target, addon_utils.module_bl_info(sys.modules[module])["name"], size


# --- [LIBRARY]
def library_stamp() -> str:
    """Stamp the library file holds, empty before the first build."""
    if not LIBRARY.exists():
        return ""
    with bpy.data.libraries.load(str(LIBRARY), link=True) as (_, target):
        target.texts = [STAMP]
    return target.texts[0].as_string()


def assembled(stamp: str, module: str | None) -> tuple[str, ...]:
    """Mark each species as an asset in its catalog, preview it under a neutral scene, write the library with the stamp and catalog file, and return each species' size."""
    species = (("Live Oak", "OAK", 70 * FOOT, 90 * FOOT), ("Bald Cypress", "PINE", 70 * FOOT, 30 * FOOT))
    trees, planting = tuple(planted(module, *each) for each in species) if module else (), "Props/Site Planting"
    paths = accumulate(planting.split("/"), lambda head, name: f"{head}/{name}") if trees else ()
    catalogs, neutral = {path: str(uuid5(NAMESPACE_URL, path)) for path in paths}, bpy.data.scenes.new("Neutral Display")
    for target, author, _ in trees:
        target.asset_mark()
        target.asset_data.catalog_id, target.asset_data.author = catalogs[planting], author
        with bpy.context.temp_override(scene=neutral):
            target.asset_generate_preview()
    text = bpy.data.texts.new(STAMP)
    text.write(stamp)
    ASSETS.mkdir(parents=True, exist_ok=True)
    bpy.data.libraries.write(str(LIBRARY), {*(target for target, _, _ in trees), text}, fake_user=True)
    (ASSETS / "blender_assets.cats.txt").write_text("".join(("VERSION 1\n", *(f"{identity}:{path}:{path.replace('/', '-')}\n" for path, identity in catalogs.items()))), encoding="utf-8")
    return tuple(size for _, _, size in trees)


def built(path: str) -> None:
    """Rebuild the library when the stamp of Modular Tree's version and this module moved, and write its change rows as JSON to the path."""
    module = loaded().get("modular_tree")
    version = addon_utils.module_bl_info(sys.modules[module])["version"] if module else ()
    stamp, before = digest(repr(version).encode() + Path(__file__).read_bytes()), library_stamp()
    changes = () if stamp == before else ((Kind.CHANGE, f"library.{ASSETS.name}", before or ABSENT, "; ".join((stamp, *assembled(stamp, module)))),)
    Path(path).write_text(make_converter().dumps(changes), encoding="utf-8")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["built"]
