"""Layered documents of a layer stack's saved frame: each layer's formed color and raw mask coverage under its blend, nested stacks as groups, the composite in the document's colorspace."""

from collections.abc import Callable
from functools import partial
from pathlib import Path

import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.addressing import grouped_value, owned
from blender.bpy.nodes.desired import item_key
from blender.bpy.render.compositor.inputs import entry_value
from blender.bpy.render.compositor.layers import Adjustment, COMPOSITE_PART, LAYER_BLEND, LayerStack, stack_file, StackLayer
from blender.bpy.render.compositor.masks import MODE, OPACITY, photoshop
from blender.bpy.render.compositor.stack_service import LayerStackService, nested_tree
from blender.bpy.render.outputs.formats import extension
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.buffers import Coded, Depth, unassociated
from blender.imaging.files.documents import Document, DocumentLayers, LayerGroup, PixelLayer
from blender.imaging.files.multilayer import Multilayer
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason, typed
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type _Planes = tuple[DocumentLayers[Coded], npt.NDArray[np.float32]]

# --- [OPERATIONS] -----------------------------------------------------------------------


def layered[M: Case](stacking: LayerStackService[M], view: DisplayView, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> Outcome[Document[Coded]]:
    """Layered document of formed color and raw mask coverage of the scene's head stack at a saved frame, formed through the view, nested stacks as groups.

    Returns:
        `Ok` of the document. `Error` of `absent` for a missing file, layer, or frame number, and `refused` for an unreadable number or opacity.
    """
    identity, config = stacking.identity, PyOpenColorIO.GetCurrentConfig()
    number = (
        Block
        .of_seq(frame.parts[:1])
        .choose(lambda part: Option.of_optional(part.header.get("Frame")))
        .try_head()
        .to_result_with(partial(Fault.of, Reason.ABSENT, str(frame.path), member="Frame"))
        .bind(lambda text: catch(int, exception=ValueError)(text).map_error(partial(Fault.of, Reason.REFUSED, str(frame.path))))
    )

    def read_tree(tree: bpy.types.NodeTree, record: LayerStack[M], at: int, valued: Callable[[Key], Outcome[Value]]) -> Outcome[_Planes]:
        shown = frozenset(layer.key for layer, _ in record.composited())
        path = (
            Block
            .of_seq(owned(tree, identity).values())
            .choose(lambda node: Option.of_optional(try_downcast(bpy.types.CompositorNodeOutputFile, node)))
            .try_head()
            .bind(lambda node: Option.of_optional(node.format).bind(extension).map(lambda suffix: Path(bpy.path.abspath(node.directory)) / f"{stack_file(tree.session_uid, Some(at))}{suffix}"))
            .to_result_with(partial(Fault.of, Reason.ABSENT, repr(tree.nodes), member=bpy.types.CompositorNodeOutputFile.__name__))
        )

        def planes(file: Multilayer) -> Outcome[_Planes]:
            def raw(name: str) -> Outcome[npt.NDArray[np.float32]]:
                found = Block.of_seq(file.layers).filter(lambda layer: layer.name == name).try_head().to_result_with(partial(Fault.of, Reason.ABSENT, str(file.path), member=name))
                return found.bind(lambda layer: file.plane(layer, layer.channels))

            def color(name: str) -> Outcome[tuple[npt.NDArray[Coded], npt.NDArray[Coded]]]:
                return raw(name).bind(lambda pixels: view.form(config, pixels)).map(unassociated).bind(lambda pixels: paired(depth.coded(pixels[..., :3]), depth.coded(pixels[..., 3])))

            def blend(layer: StackLayer[M]) -> Outcome[tuple[object, float]]:
                read = partial(item_key, layer.key, LAYER_BLEND)
                return paired(valued(read(MODE.key)), valued(read(OPACITY.key)).bind(partial(typed, float, layer.key)))

            def each(layer: StackLayer[M]) -> Outcome[PixelLayer[Coded] | LayerGroup[Coded]]:
                visible = layer.key in shown
                if isinstance(layer.source, Adjustment):
                    return color(layer.composite_part).map(lambda codes: PixelLayer(name=layer.name, color=codes[0], alpha=codes[1], visible=visible))
                mask = raw(layer.mask_part).bind(lambda pixels: depth.coded(pixels[..., 0])).map(Some) if layer.blend.masks or layer.matte else Ok(Nothing)
                nested = (
                    nested_tree(identity, tree, layer)
                    .filter(lambda _: layer.plain)
                    .map(
                        lambda inner: paired(stacking.read(inner), blend(layer)).bind(
                            lambda parts: read_tree(inner, parts[0], at, partial(grouped_value, identity, inner, owned(tree, identity)[layer.node])).map(
                                lambda held: LayerGroup(name=layer.name, layers=held[0], mode=Some(photoshop(str(parts[1][0]))), opacity=parts[1][1], visible=visible)
                            )
                        )
                    )
                )
                return nested.default_with(
                    lambda: paired(color(layer.key), mask, blend(layer)).map(
                        lambda parts: PixelLayer(name=layer.name, color=parts[0][0], alpha=parts[0][1], mask=parts[1], mode=photoshop(str(parts[2][0])), opacity=parts[2][1], visible=visible)
                    )
                )

            return paired(accumulated(each(layer) for layer in record.layers), raw(COMPOSITE_PART).bind(lambda pixels: view.form(config, pixels)).map(unassociated))

        return path.bind(Multilayer.read).bind(planes)

    def composed(entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree, record: LayerStack[M], at: int) -> Outcome[Document[Coded]]:
        density = scene.render.ppm_factor / scene.render.ppm_base
        space = view.colorspace(config).to_result_with(partial(Fault.of, Reason.ABSENT, repr(config), member=f"{view.display}/{view.view}"))
        return paired(read_tree(tree, record, at, partial(entry_value, identity, entry)), space).bind(
            lambda parts: depth.coded(parts[0][1]).map(lambda composite: Document(layers=parts[0][0], composite=composite, colorspace=parts[1].getName(), density=density))
        )

    return stacking.head(scene).bind(lambda held: paired(stacking.read(held[1]), number).bind(lambda parts: composed(held[0], held[1], *parts)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["layered"]
