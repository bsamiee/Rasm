# ty: ignore[invalid-assignment, not-subscriptable, unresolved-attribute]
# mypy: disable-error-code="func-returns-value, index, no-any-return, union-attr, var-annotated"
"""Blender's shared asset folder and its library of site planting species, grown through Modular Tree and written into the folder when the stamp of its declared inputs moves."""

from collections.abc import Callable, Iterator
from functools import partial
from importlib import import_module
from itertools import accumulate
import sys
from types import ModuleType
from typing import Final
from uuid import NAMESPACE_URL, uuid5

import addon_utils
from attrs import frozen
import bpy
import numpy as np

from interface.blender.script.screens import TICK
from interface.render import DESIGN_TOOLS
from interface.report import changes, digest, Error, subscript, Update
from interface.units import Length

# --- [CONSTANTS] ------------------------------------------------------------------------

STAMP: Final = "Stamp"

# --- [FILES] ----------------------------------------------------------------------------

ASSETS: Final = DESIGN_TOOLS / "assets"
LIBRARY: Final = ASSETS / "assets.blend"

# --- [MODELS] ---------------------------------------------------------------------------


@frozen
class Species:
    """Species by its asset name, Modular Tree preset, and grown height and crown width in meters."""

    name: str
    preset: str
    height: float
    width: float


@frozen
class Planting:
    """Declared inputs of the library: its catalog path, species, seed, trunk and branch resolution, mesher points and smoothing, fit tolerance, factor bracket, halvings per bisection, and fitting rounds."""

    catalog: str
    species: tuple[Species, ...]
    seed: int
    resolution: float
    radial_points: int
    smoothing: int
    tolerance: float
    bracket: tuple[float, float]
    halvings: int
    rounds: int


@frozen
class Specimen:
    """Tree of a species grown at its trunk and branch factors, as Modular Tree's mesh with its height and wider horizontal extent in meters."""

    species: Species
    trunk: float
    branch: float
    mesh: object
    height: float
    width: float


# --- [PLANTING] -------------------------------------------------------------------------

PLANTING: Final = Planting(
    catalog="Props/Site Planting",
    species=(Species("Live Oak", "OAK", 70 * Length.FEET, 90 * Length.FEET), Species("Bald Cypress", "PINE", 70 * Length.FEET, 30 * Length.FEET)),
    seed=7,
    resolution=1.0,
    radial_points=8,
    smoothing=4,
    tolerance=0.02,
    bracket=(0.25, 4.0),
    halvings=12,
    rounds=8,
)

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [TREES]
def grown(tree: ModuleType, presets: ModuleType, species: Species, trunk_factor: float, branch_factor: float) -> Specimen:
    """Species grown as `mtree.quick_generate` grows its preset, the trunk length and radii scaled by the trunk factor and the branch length by the branch factor."""
    trunk, branches, plant, seed = tree.TrunkFunction(), tree.BranchFunction(), tree.Tree(), PLANTING.seed
    trunk.seed, branches.seed = seed, seed + 1
    presets.apply_trunk_preset(trunk, species.preset)
    presets.apply_preset(branches, species.preset)
    trunk.length, trunk.start_radius, trunk.end_radius = (value * trunk_factor for value in (trunk.length, trunk.start_radius, trunk.end_radius))
    branches.length = tree.PropertyWrapper(tree.ConstantProperty((preset := presets.TREE_PRESETS[species.preset]).branches["length"] * branch_factor))
    trunk.resolution = branches.resolution = PLANTING.resolution
    if preset.sub_branches:
        sub_branches = tree.BranchFunction()
        sub_branches.seed = seed + 2
        presets.apply_sub_branch_preset(sub_branches, species.preset)
        sub_branches.resolution = PLANTING.resolution
        branches.add_child(sub_branches)
    trunk.add_child(branches)
    plant.set_trunk_function(trunk)
    plant.execute_functions()
    mesher = tree.ManifoldMesher()
    mesher.radial_n_points, mesher.smooth_iterations = PLANTING.radial_points, PLANTING.smoothing
    mesh = mesher.mesh_tree(plant)
    points = mesh.get_vertices().reshape(-1, 3)
    extent = np.nanmax(points, axis=0) - np.nanmin(points, axis=0)
    return Specimen(species, trunk_factor, branch_factor, mesh, float(extent[2]), float(max(extent[0], extent[1])))


def bisected(grow: Callable[[float], Specimen], miss: Callable[[Specimen], float], low: float, high: float, halvings: int) -> Specimen:
    """Specimen grown at the middle factor of the interval whose relative miss is within the tolerance, found by at most the given halvings."""
    middle = (low + high) / 2
    specimen = grow(middle)
    match miss(specimen):
        case error if abs(error) <= PLANTING.tolerance or halvings == 0:
            return specimen
        case error if error > 0:
            return bisected(grow, miss, low, middle, halvings - 1)
        case _:
            return bisected(grow, miss, middle, high, halvings - 1)


def fitted(grow: Callable[[float, float], Specimen], specimen: Specimen, rounds: int) -> Specimen | Error:
    """Specimen whose height and width match its species within the tolerance, the trunk and then the branch factor bisected each round, or the error line after the last round."""
    species = specimen.species
    match specimen.height / species.height - 1, specimen.width / species.width - 1:
        case tall, wide if abs(tall) <= PLANTING.tolerance and abs(wide) <= PLANTING.tolerance:
            return specimen
        case _ if rounds == 0:
            height, width, declared_height, declared_width = (value / Length.FEET for value in (specimen.height, specimen.width, species.height, species.width))
            return Error(
                f"{species.name} grows {height:.1f} by {width:.1f} ft against its declared {declared_height:.1f} by {declared_width:.1f} ft at trunk factor {specimen.trunk:.4f} and branch factor {specimen.branch:.4f} after {PLANTING.rounds} fitting rounds"
            )
        case _:
            trunk = bisected(lambda factor: grow(factor, specimen.branch), lambda each: each.height / species.height - 1, *PLANTING.bracket, PLANTING.halvings).trunk
            return fitted(grow, bisected(lambda factor: grow(trunk, factor), lambda each: each.width / species.width - 1, *PLANTING.bracket, PLANTING.halvings), rounds - 1)


# --- [LIBRARY]
def library_stamp() -> str | None:
    """Stamp the library file holds, None while no library file exists."""
    try:
        with bpy.data.temp_data() as data:
            with data.libraries.load(str(LIBRARY)) as (_, held):
                held.texts = [STAMP]
            return held.texts[0].as_string()
    except OSError:
        return None


def written(scene: bpy.types.Scene, module: str, author: str, specimens: tuple[Specimen, ...], stamp: str) -> Iterator[float]:
    """Mark each specimen as an asset in the planting catalog, render its preview under a new scene's factory color management, write the library with the stamp and the planting catalogs beside every catalog the folder's file holds, then remove every data-block the build created."""
    catalogs = {path: str(uuid5(NAMESPACE_URL, path)) for path in accumulate(PLANTING.catalog.split("/"), lambda head, name: f"{head}/{name}")}
    fill = import_module(f"{module}.python_classes.mesh_utils").create_mesh_from_cpp
    meshes = tuple(bpy.data.meshes.new(specimen.species.name) for specimen in specimens)
    targets = tuple(bpy.data.objects.new(mesh.name, mesh) for mesh in meshes)
    for mesh, target, specimen in zip(meshes, targets, specimens, strict=True):
        fill(mesh, specimen.mesh)
        scene.collection.objects.link(target)
        target.asset_mark()
        target.asset_data.catalog_id, target.asset_data.author = catalogs[PLANTING.catalog], author
    scene.view_layers[0].update()
    preview = bpy.data.scenes.new("Preview")
    for target in targets:
        with bpy.context.temp_override(scene=preview):
            target.asset_generate_preview()
        while bpy.app.is_job_running("RENDER_PREVIEW"):
            yield TICK
    text = bpy.data.texts.new(STAMP)
    text.write(stamp)
    ASSETS.mkdir(parents=True, exist_ok=True)
    bpy.data.libraries.write(str(LIBRARY), {*targets, text}, fake_user=True)
    definitions = ASSETS / "blender_assets.cats.txt"
    try:
        held = definitions.read_text(encoding="utf-8").splitlines()
    except FileNotFoundError:
        held = ["VERSION 1"]
    planted = (f"{identity}:{path}:{path.replace('/', '-')}" for path, identity in catalogs.items())
    definitions.write_text("".join(f"{entry}\n" for entry in dict.fromkeys((*held, *planted))), encoding="utf-8")
    bpy.data.batch_remove((*targets, *meshes, text, preview))


def built_library(scene: bpy.types.Scene, module: str) -> Iterator[Update]:
    """Rebuild the library from Modular Tree's module when the stamp of its version and the declared planting moved, with the change line from the held stamp to the new one or an error line per species no fitting round grows."""
    info = addon_utils.module_bl_info(sys.modules[module])
    stamp, held = digest(repr((info["version"], PLANTING)).encode()), library_stamp()
    if stamp == held:
        return
    tree, presets = import_module("m_tree"), import_module(f"{module}.python_classes.presets")
    results = tuple(fitted(partial(grown, tree, presets, species), grown(tree, presets, species, 1.0, 1.0), PLANTING.rounds) for species in PLANTING.species)
    match tuple(result for result in results if isinstance(result, Error)):
        case ():
            yield from written(scene, module, info["name"], tuple(result for result in results if isinstance(result, Specimen)), stamp)
            yield from changes(subscript("library", LIBRARY.name), held, stamp)
        case errors:
            yield from errors


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ASSETS", "built_library"]
