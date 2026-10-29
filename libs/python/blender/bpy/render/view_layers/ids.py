"""Cryptomatte ID colors: recoloring commands, the ID layer files a written frame gains, and its ID document."""

from collections.abc import Callable
import functools
import itertools
from pathlib import Path

import bpy
from expression import Ok, Result, Some
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.operators import Answer, Call, Done, Empty, Unchanged
from blender.bpy.data.rna.paths import rooted
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderReason
from blender.bpy.render.outputs.finishing import Finish, stamped, Written
from blender.bpy.render.outputs.paths import OutputPath, ScenePaths, today
from blender.bpy.render.view_layers.overrides import colors, encoded, Spectrum
from blender.bpy.render.view_layers.passes import ENTITIES, Entity
from blender.bpy.render.view_layers.sources import located, named, ROUTING, Routing
from blender.imaging.files.buffers import Depth, Rounded, unassociated
from blender.imaging.files.cryptomatte import Matte
from blender.imaging.files.documents import Document, LayerGroup, PixelLayer
from blender.imaging.files.images import Deflate, Storage
from blender.imaging.files.multilayer import Multilayer, Pass, PassLayout, PassRole
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired
from blender.records.identity import content_hash, rng
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------

type Coded = np.uint8 | np.uint16 | np.float32
type Recolor = Callable[[tuple[tuple[float, float, float], ...]], tuple[tuple[float, float, float], ...]]

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [COLORS]


def colors_written(layer: bpy.types.ViewLayer, entity: Entity, chosen: Recolor) -> Outcome[int]:
    """Writes chosen colors onto entity names, each block keeping its alpha."""
    rows = named(layer, entity)
    return accumulated(
        ENTITIES[entity].color.write(block, (*color, getattr(block, ENTITIES[entity].color.prop.identifier)[3])) for (_, block, _), color in zip(rows, chosen(tuple(color for _, _, color in rows)))
    ).map(lambda priors: sum(prior.is_some() for prior in priors))


def recolored(chosen: Recolor, call: Call[Empty]) -> Outcome[Answer]:
    """Writes chosen colors onto the names of each Cryptomatte kind the active layer draws as ID colors."""

    def painted(layer: bpy.types.ViewLayer, routing: Routing) -> Outcome[Answer]:
        if not routing.kinds:
            return Ok(Unchanged(reason=f"{layer.name} draws no ID colors"))
        return accumulated(colors_written(layer, entity, chosen) for entity in routing.kinds).map(
            lambda counts: Done() if sum(counts) else Unchanged(reason=f"{layer.name} ID colors hold chosen colors")
        )

    return Result[bpy.types.ViewLayer, Faults].bind(rooted(bpy.types.ViewLayer, call.context), lambda layer: ROUTING.read(call.identity, layer).bind(lambda routing: painted(layer, routing)))


def ids_spread(call: Call[Empty]) -> Outcome[Answer]:
    """Set evenly spaced hues on layer ID names."""
    return recolored(lambda held: colors(Spectrum(saturation=1.0, value=1.0), len(held)), call)


def ids_shuffle(call: Call[Empty]) -> Outcome[Answer]:
    """Shuffle colors among layer ID names."""
    return recolored(lambda held: tuple(held[index] for index in rng(content_hash(Routing, arrays=(np.asarray(held),))).permutation(len(held))), call)


# --- [FILES]


def exported(scene: bpy.types.Scene, file: Written, identity: Identity) -> Outcome[None]:
    """Writes one ID layer file per matte of a written multilayer file, one layer per matte name a scene block colors, refusing a path equal to source."""

    def painted(path: OutputPath, matte: Matte, layer: bpy.types.ViewLayer, entity: Entity) -> Outcome[str]:
        shown = {name: color for name, _, color in named(layer, entity)}
        known = {name: shown[name] for name in matte.ids if name in shown}
        return (
            path
            .written(file.layer, Output.MATTE, Some(ENTITIES[entity].pass_name), scene.frame_current, ".exr")
            .map(str)
            .filter_with(lambda target: target != file.path, lambda target: Fault.of(BlenderReason.TAKEN, target, member=file.path))
            .bind(
                lambda target: Multilayer.written(
                    Path(target),
                    tuple(Pass(layer=name, channels=("R", "G", "B", "A"), role=PassRole.COLOR) for name in known),
                    PassLayout.PARTS,
                    Storage(depth=Depth.HALF, compression=Some(Deflate())),
                    (matte.file.parts[0].width, matte.file.parts[0].height),
                    stamped(identity),
                    lambda held: matte.filled(frozenset({held.layer}), np.asarray(known[held.layer])),
                )
            )
            .map(str)
        )

    def matted(path: OutputPath, matte: Matte) -> Outcome[str]:
        return located(scene, matte, file.layer).bind(lambda pair: painted(path, matte, *pair))

    return (
        paired(ScenePaths().bound(identity).bind(lambda pathed: pathed.path(scene, today())), Multilayer.read(Path(file.path)).bind(Matte.found))
        .bind(lambda found: accumulated(matted(found[0], matte) for matte in found[1]))
        .map(lambda _: None)
    )


def document(_identity: Identity, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> Outcome[Document[Coded]]:
    """Builds the ID document of a written frame, one layer group per matte and one layer per matte name in its block's display color."""
    size = (frame.parts[0].height, frame.parts[0].width)
    coded = functools.partial(depth.coded, coding=Rounded())

    def grouped(index: int, matte: Matte) -> Outcome[tuple[LayerGroup[Coded], npt.NDArray[np.float32]]]:
        def cover(name: str, color: tuple[float, float, float]) -> Outcome[tuple[str, tuple[float, float, float], npt.NDArray[np.float32]]]:
            return matte.coverage(frozenset({name})).map(lambda plane: (name, color, plane))

        def covered(layer: bpy.types.ViewLayer, entity: Entity) -> Outcome[tuple[tuple[str, tuple[float, float, float], npt.NDArray[np.float32]], ...]]:
            return accumulated(cover(name, color) for name, _, color in named(layer, entity))

        def layered(name: str, color: tuple[float, float, float], plane: npt.NDArray[np.float32]) -> Outcome[PixelLayer[Coded]]:
            return paired(coded(np.broadcast_to(np.asarray(encoded(color), dtype=np.float32), (*size, 3))), coded(plane)).map(lambda codes: PixelLayer(name=name, color=codes[0], alpha=codes[1]))

        return located(scene, matte, matte.name.rpartition(".")[0]).bind(
            lambda pair: covered(*pair).bind(
                lambda rows: accumulated(itertools.starmap(layered, rows)).map(
                    lambda layers: (
                        LayerGroup(name=matte.name, visible=index == 0, layers=layers),
                        functools.reduce(np.add, (plane[..., None] * np.asarray((*encoded(color), 1.0), dtype=np.float32) for _, color, plane in rows), np.zeros((*size, 4), np.float32)),
                    )
                )
            )
        )

    return Result[tuple[Matte, ...], Faults].bind(
        Matte.found(frame),
        lambda mattes: accumulated(itertools.starmap(grouped, enumerate(mattes))).bind(
            lambda groups: coded(unassociated(next((filled for _, filled in groups), np.zeros((*size, 4), np.float32)))).map(
                lambda composite: Document(layers=tuple(group for group, _ in groups), composite=composite, colorspace="sRGB", density=scene.render.ppm_factor / scene.render.ppm_base)
            )
        ),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

ids_export = Finish(run=exported, formats=frozenset({"OPEN_EXR_MULTILAYER"}))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Coded", "Recolor", "colors_written", "document", "exported", "ids_export", "ids_shuffle", "ids_spread", "recolored"]
