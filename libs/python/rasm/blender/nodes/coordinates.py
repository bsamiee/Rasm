"""Compositor image coordinates: camera frame positions, data-window pixel grids, and their Map UV placements."""

from typing import Literal

import bpy
import msgspec

from rasm.blender.extension.identity import Key
from rasm.blender.faults import BlenderResult
from rasm.blender.nodes.graphs import End, ended, Fragment, math, node, Operand, vector
from rasm.blender.nodes.interface import item_key
from rasm.imaging.faults import paired

# --- [MODELS] ---------------------------------------------------------------------------


class FrameSpace(msgspec.Struct, frozen=True, kw_only=True):
    """Camera frame positions of a source: shares of the displayed greater side, frame fractions, that side in pixels, per-axis pixels, and the Map UV placement, each the built pair of its node."""

    point: tuple[Fragment, End]
    extent: tuple[Fragment, End]
    greater: tuple[Fragment, End]
    fraction: tuple[Fragment, End]
    scale: tuple[Fragment, End]
    placing: tuple[Fragment, End]
    margin: tuple[Fragment, End]


class PixelGrid(msgspec.Struct, frozen=True, kw_only=True):
    """Data-window UV centres and increments, displayed pixel centres, and the single-value window offset, each with its native graph."""

    base: tuple[Fragment, End]
    inverse: tuple[Fragment, End]
    point: tuple[Fragment, End]
    offset: tuple[Fragment, End]

    def at(self, key: Key, offset: Operand, scale: Operand) -> BlenderResult[tuple[Fragment, End]]:
        """Builds the Map UV coordinate `offset ⊙ scale + base` of a pixel offset.

        Args:
            key: Key of the result node.
            offset: Operand of the offset in pixels.
            scale: Operand of the grid's `inverse` or a signed multiple of it.

        Returns:
            `Ok` of the fragment and its `Vector` end; `Error` of the faults `node` answers.
        """
        return vector(key, "MULTIPLY_ADD", offset, scale, self.base)


# --- [OPERATIONS] -----------------------------------------------------------------------


def framed(key: Key, source: Operand, aspect: Operand, scale: Operand = 1.0) -> BlenderResult[FrameSpace]:
    """Builds the frame positions of the camera frame centred in a source, over its displayed frame.

    Args:
        key: Prefix of the build's node keys.
        source: Operand of the image whose pixels the build reads.
        aspect: Operand of the pixel aspect `pixel_aspect_x / pixel_aspect_y`.
        scale: Operand of the camera frame's share of each side of the source, 1 for a source holding the camera frame alone.

    Returns:
        `Ok` of the source's `FrameSpace`, each field carrying the nodes upstream of it; `Error` of the faults `node` answers.
    """

    def spaced(
        greater: tuple[Fragment, End], parts: tuple[tuple[tuple[Fragment, End], tuple[Fragment, End]], tuple[tuple[Fragment, End], tuple[tuple[Fragment, End], tuple[Fragment, End]]]]
    ) -> BlenderResult[FrameSpace]:
        (point, extent), (stretch, (placing, margin)) = parts
        return vector(f"{key}_fraction", "DIVIDE", point, extent).map(
            lambda fraction: FrameSpace(point=point, extent=extent, greater=greater, fraction=fraction, scale=stretch, placing=placing, margin=margin)
        )

    def placed(
        axes: tuple[Fragment, End], grid: PixelGrid, camera: tuple[tuple[Fragment, End], tuple[Fragment, End]], sizes: tuple[tuple[Fragment, End], tuple[Fragment, End]]
    ) -> BlenderResult[FrameSpace]:
        (pixels, offset), (sized, displayed) = camera, sizes
        return (
            node(f"{key}_sides", bpy.types.ShaderNodeSeparateXYZ, {"Vector": sized}, {})
            .bind(lambda sides: math(f"{key}_greater", "MAXIMUM", ended(f"{key}_sides", "X")(sides), ended(f"{key}_sides", "Y")(sides)))
            .bind(
                lambda greater: paired(
                    paired(vector(f"{key}_point", "DIVIDE", displayed, greater), vector(f"{key}_extent", "DIVIDE", sized, greater)),
                    paired(
                        vector(f"{key}_scale", "DIVIDE", greater, axes),
                        paired(
                            vector(f"{key}_placing", "MULTIPLY", pixels, grid.inverse),
                            vector(f"{key}_data_offset", "SUBTRACT", offset, grid.offset)
                            .bind(lambda shifted: vector(f"{key}_uv_offset", "MULTIPLY", shifted, grid.inverse))
                            .bind(lambda normalized: vector(f"{key}_uv_margin", "ADD", normalized, (0.0, 0.0, 1.0))),
                        ),
                    ),
                ).bind(lambda parts: spaced(greater, parts))
            )
        )

    def measured(style: tuple[tuple[Fragment, End], tuple[Fragment, End]], coordinates: tuple[PixelGrid, tuple[Fragment, End]]) -> BlenderResult[FrameSpace]:
        axes, margin = style
        grid, dimensions = coordinates
        return paired(vector(f"{key}_camera_pixels", "SCALE", dimensions, scale), vector(f"{key}_camera_offset", "SCALE", dimensions, margin)).bind(
            lambda camera: paired(
                vector(f"{key}_sized", "MULTIPLY", camera[0], axes),
                vector(f"{key}_shifted", "SUBTRACT", grid.point, camera[1]).bind(lambda shifted: vector(f"{key}_displayed", "MULTIPLY", shifted, axes)),
            ).bind(lambda sizes: placed(axes, grid, camera, sizes))
        )

    return paired(
        paired(node(f"{key}_aspect", bpy.types.ShaderNodeCombineXYZ, {"X": aspect, "Y": 1.0}, {}).map(ended(f"{key}_aspect", "Vector")), math(f"{key}_margin", "MULTIPLY_ADD", scale, -0.5, 0.5)),
        _coordinates(key, source),
    ).bind(lambda parts: measured(*parts))


def gridded(key: Key, image: Operand) -> BlenderResult[PixelGrid]:
    """Builds data-window UVs, pixel centres, and the single-value window offset.

    Args:
        key: Prefix of the grid's node keys.
        image: Operand of the image whose pixels the grid samples.

    Returns:
        `Ok` of the grid, each field carrying the nodes upstream of it; `Error` of the faults `node` answers.
    """
    return _coordinates(key, image).map(lambda parts: parts[0])


def sampled(key: Key, frame: FrameSpace, fraction: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Maps a camera frame fraction onto the Map UV coordinate of the source holding that frame, with coverage 1 in Z.

    Args:
        key: Key of the result node.
        frame: Frame space of the source the coordinate samples.
        fraction: Operand of a position as fractions of the camera frame from its bottom-left corner.

    Returns:
        `Ok` of the fragment and its `Vector` end; `Error` of the faults `node` answers.
    """
    return vector(key, "MULTIPLY_ADD", fraction, frame.placing, frame.margin)


def pixels(key: Key, frame: FrameSpace, length: Operand, kind: Literal["FLOAT", "VECTOR"] = "FLOAT") -> BlenderResult[tuple[Fragment, End]]:
    """Converts a `FRAME_GREATER` length or offset into stored pixels of the frame space `framed` built.

    Args:
        key: Key of the result node, its axis split keyed `<key>_axes`.
        frame: Frame space of the source the length measures.
        length: Operand of a length, or of an offset or 2D size in displayed units.
        kind: `FLOAT` for a length in stored X pixels, `VECTOR` for per-axis stored pixels.

    Returns:
        `Ok` of the fragment and its pixel end; `Error` of the faults `node` answers.
    """
    match kind:
        case "VECTOR":
            return vector(key, "MULTIPLY", length, frame.scale)
        case "FLOAT":
            return node(f"{key}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": frame.scale}, {}).bind(lambda axes: math(key, "MULTIPLY", length, ended(f"{key}_axes", "X")(axes)))


def _coordinates(key: Key, image: Operand) -> BlenderResult[tuple[PixelGrid, tuple[Fragment, End]]]:
    """Builds native data and display coordinates over the realized source domain."""
    realized, coordinates, dimensions = item_key(key, "realized"), item_key(key, "coordinates"), item_key(key, "dimensions")
    information, offset = item_key(key, "information"), item_key(key, "window_offset")

    def built(pixel: tuple[Fragment, End], sizes: tuple[tuple[Fragment, End], tuple[Fragment, End]]) -> BlenderResult[tuple[PixelGrid, tuple[Fragment, End]]]:
        data, display = sizes
        return paired(
            paired(
                vector(item_key(key, "centre"), "ADD", pixel, (0.5, 0.5, 0.0)),
                paired(
                    node(item_key(offset, "x"), bpy.types.CompositorNodeLevels, {"Image": pixel, "Channel": "Red"}, {}).map(ended(item_key(offset, "x"), "Minimum")),
                    node(item_key(offset, "y"), bpy.types.CompositorNodeLevels, {"Image": pixel, "Channel": "Green"}, {}).map(ended(item_key(offset, "y"), "Minimum")),
                ).bind(lambda minima: node(offset, bpy.types.ShaderNodeCombineXYZ, dict(zip(("X", "Y"), minima)), {}).map(ended(offset, "Vector"))),
            ),
            vector(item_key(key, "inverse"), "DIVIDE", (1.0, 1.0, 0.0), data),
        ).bind(
            lambda parts: (
                vector(item_key(key, "local"), "SUBTRACT", *parts[0])
                .bind(lambda local: vector(item_key(key, "normalized"), "MULTIPLY", local, parts[1]))
                .bind(lambda normalized: vector(item_key(key, "base"), "ADD", normalized, (0.0, 0.0, 1.0)))
                .map(lambda base: (PixelGrid(base=base, inverse=parts[1], point=parts[0][0], offset=parts[0][1]), display))
            )
        )

    return paired(
        node(realized, bpy.types.CompositorNodeBlur, {"Image": image}, {})
        .map(ended(realized, "Image"))
        .bind(lambda source: node(coordinates, bpy.types.CompositorNodeImageCoordinates, {"Image": source}, {}).map(ended(coordinates, "Pixel"))),
        paired(
            node(information, bpy.types.CompositorNodeImageInfo, {"Image": image}, {}).map(ended(information, "Dimensions")),
            node(dimensions, bpy.types.CompositorNodeRelativeToPixel, {"Image": image, "Float Value": 1.0}, {"data_type": "FLOAT", "reference_dimension": "PER_DIMENSION"}).map(
                ended(dimensions, "Vector Value")
            ),
        ),
    ).bind(lambda parts: built(*parts))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FrameSpace", "PixelGrid", "framed", "gridded", "pixels", "sampled"]
