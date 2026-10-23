"""Compositor camera frame positions, data-window pixel grids, and Map UV placements."""

from typing import Literal

import bpy
import msgspec

from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End, ended, Fragment, math, node, Operand, vector
from blender.bpy.nodes.interface import item_key
from blender.imaging.faults import paired

# --- [MODELS] ---------------------------------------------------------------------------


class FrameSpace(msgspec.Struct, frozen=True, kw_only=True):
    """Source camera frame in displayed greater-side shares, frame fractions, pixels, and Map UV placement."""

    point: tuple[Fragment, End]
    extent: tuple[Fragment, End]
    greater: tuple[Fragment, End]
    fraction: tuple[Fragment, End]
    scale: tuple[Fragment, End]
    placing: tuple[Fragment, End]
    margin: tuple[Fragment, End]


class PixelGrid(msgspec.Struct, frozen=True, kw_only=True):
    """Data-window UV centers and steps, displayed pixel centers, and single-value window offset."""

    base: tuple[Fragment, End]
    inverse: tuple[Fragment, End]
    point: tuple[Fragment, End]
    offset: tuple[Fragment, End]

    def at(self, key: Key, offset: Operand, scale: Operand) -> BlenderResult[tuple[Fragment, End]]:
        """Builds Map UV coordinate `offset ⊙ scale + base`.

        Args:
            key: Result node key.
            offset: Offset in pixels.
            scale: Grid `inverse` or a signed multiple of it.

        Returns:
            `Ok` of fragment and `Vector` end. `Error` of `node` faults.
        """
        return vector(key, "MULTIPLY_ADD", offset, scale, self.base)


# --- [OPERATIONS] -----------------------------------------------------------------------


def framed(key: Key, source: Operand, aspect: Operand, scale: Operand = 1.0) -> BlenderResult[FrameSpace]:
    """Builds positions of camera frame centered in source.

    Args:
        key: Prefix of built node keys.
        source: Image the build reads pixels from.
        aspect: Pixel aspect `pixel_aspect_x / pixel_aspect_y`.
        scale: Camera frame share of each source side, 1 for a source holding camera frame alone.

    Returns:
        `Ok` of source `FrameSpace`, each field holding nodes upstream of it. `Error` of `node` faults.
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
    """Builds pixel grid of image.

    Args:
        key: Prefix of grid node keys.
        image: Image grid samples.

    Returns:
        `Ok` of grid, each field holding nodes upstream of it. `Error` of `node` faults.
    """
    return _coordinates(key, image).map(lambda parts: parts[0])


def sampled(key: Key, frame: FrameSpace, fraction: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Maps camera frame fraction to Map UV coordinate of source holding frame, coverage 1 in Z.

    Args:
        key: Result node key.
        frame: Frame space of sampled source.
        fraction: Position as camera frame fractions from bottom-left corner.

    Returns:
        `Ok` of fragment and `Vector` end. `Error` of `node` faults.
    """
    return vector(key, "MULTIPLY_ADD", fraction, frame.placing, frame.margin)


def pixels(key: Key, frame: FrameSpace, length: Operand, kind: Literal["FLOAT", "VECTOR"] = "FLOAT") -> BlenderResult[tuple[Fragment, End]]:
    """Converts `FRAME_GREATER` length or offset to stored pixels.

    Args:
        key: Result node key, axis split keyed `<key>_axes`.
        frame: `framed` space of measured source.
        length: Length, offset, or 2D size in displayed units.
        kind: `FLOAT` for stored X pixels, `VECTOR` for per-axis stored pixels.

    Returns:
        `Ok` of fragment and pixel end. `Error` of `node` faults.
    """
    match kind:
        case "VECTOR":
            return vector(key, "MULTIPLY", length, frame.scale)
        case "FLOAT":
            return node(f"{key}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": frame.scale}, {}).bind(lambda axes: math(key, "MULTIPLY", length, ended(f"{key}_axes", "X")(axes)))


def lensed() -> BlenderResult[Fragment]:
    """Builds Active Camera node `camera` and Camera Info node `lens`."""
    return node("camera", bpy.types.GeometryNodeInputActiveCamera, {}, {}).bind(lambda camera: node("lens", bpy.types.GeometryNodeCameraInfo, {"Camera": ended("camera", "Active Camera")(camera)}, {}))


def shorter(key: Key, extent: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Builds smaller side of 2D extent, axis split keyed `<key>_sides`."""
    return node(f"{key}_sides", bpy.types.ShaderNodeSeparateXYZ, {"Vector": extent}, {}).bind(
        lambda sides: math(key, "MINIMUM", ended(f"{key}_sides", "X")(sides), End(node=f"{key}_sides", socket="Y"))
    )


def _coordinates(key: Key, image: Operand) -> BlenderResult[tuple[PixelGrid, tuple[Fragment, End]]]:
    """Builds data and display coordinates over realized source."""
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

__all__ = ["FrameSpace", "PixelGrid", "framed", "gridded", "lensed", "pixels", "sampled", "shorter"]
