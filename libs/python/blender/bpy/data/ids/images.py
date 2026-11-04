"""Image loads, tags, pixels, and readings, and data-block content hashes over members, buffers, pixels, node graphs, and compositor entries."""

from collections import deque
from collections.abc import Iterable
from contextlib import ExitStack
from functools import partial, reduce
from graphlib import TopologicalSorter
from itertools import starmap, takewhile
import math
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Final, Literal, overload

import bpy
from expression import Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Descriptor, DTYPES, Value
from blender.bpy.data.rna.paths import definition, Native, RnaPath, Struct, struct_at
from blender.bpy.data.rna.snapshots import assigned, Capture, Snapshot
from blender.bpy.nodes.trees import endpoints, upstream
from blender.imaging.analysis.lighting import Lighting, LightingReading
from blender.imaging.color.processors import called
from blender.imaging.files.buffers import premultiplied, unassociated
from blender.imaging.files.images import ImageFile, read_file
from blender.records.faults import accumulated, Error, Fault, Nothing, Ok, Outcome, paired, Reason
from blender.records.stores import content_hash, ContentHash
from blender.records.trees import Tree

# --- [TYPES] ----------------------------------------------------------------------------

type _Buffer = npt.NDArray[np.bool | np.int32 | np.float32]
type _Elements[T] = bpy.types.bpy_prop_collection[T]
type _Parts = tuple[tuple[Struct, ...], tuple[npt.NDArray[np.bool | np.int32 | np.float32 | np.float64], ...], tuple[tuple[str, str | None], ...]]
type _Subject = bpy.types.ID | bpy.types.Node | bpy.types.SceneCompositorEffect

# --- [CONSTANTS] ------------------------------------------------------------------------

_UNHASHED: Final = frozenset({"MOVIE", "SEQUENCE", "TILED"})
_COMPOSITE: Final = frozenset({"MULTILAYER", "RENDER_RESULT"})

# --- [MODELS] ---------------------------------------------------------------------------


class Tag(msgspec.Struct, frozen=True, kw_only=True):
    """Requested image reading, absent values keeping image settings, a space naming a color space or an OCIO role of the current config."""

    space: Option[str] = Nothing
    alpha: Option[Literal["STRAIGHT", "PREMUL", "CHANNEL_PACKED", "NONE"]] = Nothing
    view_as_render: Option[bool] = Nothing
    sequence: bool = False

    def members(self, config: Option[PyOpenColorIO.Config] = Nothing) -> Outcome[dict[RnaPath, Value]]:
        """Returns requested native members, resolving explicit color spaces and leaving omitted fields untouched."""
        return (
            self.space
            .map(
                lambda name: (
                    config
                    .map(Ok)
                    .default_with(lambda: called(PyOpenColorIO.GetCurrentConfig, name))
                    .bind(
                        lambda current: (
                            Option.of_optional(current.getColorSpace(name)).map(lambda space: Some(space.getName())).to_result_with(lambda: Fault.of(Reason.ABSENT, current.getName(), member=name))
                        )
                    )
                )
            )
            .default_value(Ok(Nothing))
            .map(
                lambda space: dict[RnaPath, Value]((
                    *(((("source",), "SEQUENCE"),) if self.sequence else ()),
                    *((("colorspace_settings", "name"), name) for name in space.to_list()),
                    *((("alpha_mode",), alpha) for alpha in self.alpha.to_list()),
                    *((("use_view_as_render",), shown) for shown in self.view_as_render.to_list()),
                ))
            )
        )

    def applied(self, image: bpy.types.Image) -> Outcome[None]:
        """Resolves the tag in Blender's current config and writes its requested native members in host order."""
        return self.members().bind(lambda values: assigned(image, values))


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [LOADS]


def loaded(path: str, tag: Tag) -> Outcome[bpy.types.Image]:
    """Loads or reuses file image, then applies tag."""
    return catch(bpy.data.images.load, exception=RuntimeError)(path, check_existing=True).map_error(partial(Fault.of, Reason.REFUSED, path)).bind(lambda image: tag.applied(image).map(lambda _: image))


# --- [PIXELS]


def pixels(image: bpy.types.Image) -> Outcome[npt.NDArray[np.float32]]:
    """Reads float32 pixels bottom row first as height, width, channels, refused fault for an image holding no buffer."""
    (width, height), channels = np.asarray(image.size), image.channels
    plane = np.empty((height, width, channels), np.float32)
    return catch(image.pixels.foreach_get, exception=TypeError)(plane.reshape(-1)).map_error(partial(Fault.of, Reason.REFUSED, repr(image))).map(lambda _: plane)


def aspect(image: bpy.types.Image) -> Outcome[float]:
    """Returns the default image buffer's displayed width to height, absent fault without a buffer."""
    size: Iterable[int] = image.size  # type: ignore[assignment]  # ty: ignore[unsound-assignment]
    (width, height), display = size, image.display_aspect
    return Ok(width * display.x / (height * display.y)) if width and height else Error(Fault.of(Reason.ABSENT, repr(image), member=image.filepath))


def read(image: bpy.types.Image, colorspace: str) -> Outcome[ImageFile]:
    """Returns associated float32 pixels top row first in their producer's encoding, absent fault when empty."""

    def formed(plane: npt.NDArray[np.float32]) -> ImageFile:
        flipped = np.ascontiguousarray(plane[::-1])
        return ImageFile(pixels=premultiplied(flipped) if _straight(image) else flipped, colorspace=colorspace)

    return pixels(image).map(formed) if np.asarray(image.size).prod() else Error(Fault.of(Reason.ABSENT, repr(image), member=image.filepath))


def normalized(image: bpy.types.Image, config: PyOpenColorIO.Config) -> Outcome[ImageFile]:
    """Reads an ordinary image through native full-float linear export, preserving data encoding.

    Render products require their producer's encoding through `read` or a recorded render origin.

    Returns:
        Associated pixels with their encoding, or native configuration, export, and read faults.
    """
    if image.source == "VIEWER" or image.type == "MULTILAYER":
        return Error(Fault.of(Reason.REQUIRED, repr(image), member="render encoding colorspace"))
    settings: bpy.types.ColorManagedInputColorspaceSettings = image.colorspace_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    if settings.is_data:
        return read(image, settings.name)
    refused = partial(Fault.of, Reason.REFUSED, repr(image))

    def captured(space: str, scene: bpy.types.Scene) -> Outcome[ImageFile]:
        try:
            with ExitStack() as lifetime:
                lifetime.callback(bpy.data.scenes.remove, scene)
                path = Path(lifetime.enter_context(TemporaryDirectory(dir=bpy.app.tempdir)), "image.exr")

                try:
                    output = scene.render.image_settings
                    output.color_management = "OVERRIDE"
                    output.file_format = "OPEN_EXR"
                    output.color_depth = "32"
                    output.color_mode = "RGBA"
                    linear: bpy.types.ColorManagedInputColorspaceSettings = output.linear_colorspace_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                    linear.name = space  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                    image.save_render(str(path), scene=scene)
                except (RuntimeError, ValueError, TypeError) as error:
                    return Error(refused(error))
                return read_file(path, config, colorspace=Some(space))
        except OSError as error:
            return Error(refused(error))

    return (
        Option
        .of_optional(config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR))
        .map(PyOpenColorIO.ColorSpace.getName)
        .to_result_with(lambda: Fault.of(Reason.ABSENT, config.getName(), member=PyOpenColorIO.ROLE_SCENE_LINEAR))
        .bind(lambda space: catch(bpy.data.scenes.new, exception=RuntimeError)("Image Read").map_error(refused).bind(partial(captured, space)))
    )


def lighting(image: bpy.types.Image | None, config: PyOpenColorIO.Config) -> Outcome[Option[LightingReading]]:
    """Returns the lighting reading of an image, `Nothing` without image or measurable reading."""
    return Option.of_optional(image).map(lambda held: normalized(held, config).bind(lambda file: Lighting().read(config, file.pixels))).default_value(Ok(Nothing))


def filled(image: bpy.types.Image, plane: npt.NDArray[np.float32]) -> Outcome[None]:
    """Writes associated plane into pixels, widening gray and padding channels."""
    straight = unassociated(plane) if _straight(image) else plane
    color = np.repeat(straight, 3, axis=-1) if straight.shape[2] == 1 else straight
    padded = np.pad(color, ((0, 0), (0, 0), (0, image.channels - color.shape[2])))
    if color.shape[2] < 4 and image.channels == 4:
        padded[..., 3] = 1.0
    return catch(image.pixels.foreach_set, exception=TypeError)(padded[::-1].reshape(-1)).map_error(partial(Fault.of, Reason.REFUSED, repr(image))).map(lambda _: image.update())


def _straight(image: bpy.types.Image) -> bool:
    """Whether the image's byte buffer stores straight color under an alpha the host reads, which float buffers and data images never do."""
    settings: bpy.types.ColorManagedInputColorspaceSettings = image.colorspace_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return not image.is_float and image.channels == 4 and not settings.is_data and image.alpha_mode in {"STRAIGHT", "PREMUL"}


# --- [HASHES]


@overload
def fingerprinted(owner: _Subject, *, weighted: bool = False, before_modifier: Option[str] = Nothing) -> Outcome[Option[ContentHash]]: ...
@overload
def fingerprinted(owner: tuple[_Subject, ...], *, weighted: bool = False, before_modifier: Option[str] = Nothing) -> Outcome[tuple[Option[ContentHash], ...]]: ...
def fingerprinted(owner: _Subject | tuple[_Subject, ...], *, weighted: bool = False, before_modifier: Option[str] = Nothing) -> Outcome[Option[ContentHash]] | Outcome[tuple[Option[ContentHash], ...]]:
    """Hashes captured data-block content, upstream node graphs, or compositor entries, sharing dependencies across tuple roots.

    Args:
        owner: Image, node tree, node, compositor entry, object, any data-block, or tuple of roots; geometry adds element buffers to members.
        weighted: Whether mesh vertex group weights join the key.
        before_modifier: Object modifier whose input geometry the key covers: object data, vertex groups, earlier modifiers with their groups, and its own members.

    Returns:
        Content hash or tuple of hashes, Nothing for transient images or uncaptured external content, capture faults in root and dependency order.

    Notes:
        Graph keys capture saved members and referenced node trees and images. Evaluated frame, scene, engine, and color context belong to callers.
    """
    type Key = _Subject | tuple[bpy.types.Mesh, bool]
    type Reading = Outcome[Option[ContentHash]]
    type Step = Reading | Key
    type Prepared = tuple[Reading, tuple[Step, ...]]
    type Captured[T] = Outcome[tuple[T, tuple[bpy.types.ID, ...], bool]]
    type Content = Value | tuple[IdName | None, ...]

    def references[T](struct: Native, tree: Tree[T, RnaPath, RnaPath]) -> tuple[tuple[bpy.types.ID, ...], bool]:
        blocks: dict[bpy.types.ID, None] = {}
        paths = False
        for at, held in tree.rows:
            for name, value in held.rows:
                match value:
                    case IdName():
                        blocks.update(dict.fromkeys(struct_at(struct, (*at, *name), bpy.types.ID).to_list()))
                    case tuple() if value and (value[0] is None or isinstance(value[0], IdName)):
                        blocks.update(dict.fromkeys(block for collection in struct_at(struct, (*at, *name), bpy.types.bpy_prop_collection).to_list() for block in collection if block is not None))
                    case str() if value and not paths:
                        paths = any(
                            descriptor.prop.subtype == "FILE_PATH" and not descriptor.prop.is_path_output
                            for descriptor in Descriptor.resolve(struct, (*at, *name), bpy.types.Property).to_option().to_list()
                            if isinstance(descriptor.prop, bpy.types.StringProperty)
                        )
                    case _:
                        pass
        return tuple(blocks), paths

    def capture(struct: Native, selection: Option[tuple[Descriptor, ...]] = Nothing) -> Captured[Snapshot]:
        return Snapshot.capture(struct, selection).map(lambda snapshot: (snapshot, *references(struct, snapshot.tree)))

    def cacheable[T](captures: Iterable[Captured[T]]) -> bool:
        return all(not paths and all(isinstance(block, bpy.types.NodeTree | bpy.types.Image) for block in blocks) for result in captures for _, blocks, paths in result.to_option().to_list())

    def ordered[T](captures: Iterable[Captured[T]]) -> tuple[Step, ...]:
        return tuple[Step, ...](
            step
            for result in captures
            for step in (result.map(lambda _: Nothing), *(block for _, blocks, _ in result.to_option().to_list() for block in blocks if isinstance(block, bpy.types.NodeTree | bpy.types.Image)))
        )

    def buffers[T](values: _Elements[T], members: Iterable[bpy.types.Property]) -> tuple[_Buffer, ...]:
        count = len(values)

        def fetched(member: bpy.types.Property, dtype: type[np.bool | np.int32 | np.float32]) -> _Buffer:
            buffer: _Buffer = np.empty(count * math.prod(Descriptor.dimensions(member)), dtype)
            values.foreach_get(member.identifier, buffer)
            return buffer

        return tuple(fetched(member, DTYPES[type(member)]) for member in members if type(member) in DTYPES)

    def image(image: bpy.types.Image) -> Outcome[Option[ContentHash]]:
        transient = image.source in _UNHASHED or image.type in _COMPOSITE or image.is_multiview
        empty = (image.source == "VIEWER" and not image.has_data) or not np.asarray(image.size).prod()
        if transient or empty:
            return Ok(Nothing)
        members = Descriptor.saved(image, lambda prop: prop != bpy.types.Image.bl_rna.properties["pixels"])
        settings = msgspec.msgpack.encode((image.type, image.is_float))
        packed = tuple(np.frombuffer(file.data, np.uint8) for part in image.packed_files for file in Option.of_optional(part.packed_file).to_list())
        return paired(pixels(image), Snapshot.capture(image, Some(members))).map(lambda pair: Some(content_hash(bpy.types.Image, pair[1], settings, arrays=(pair[0], *packed))))

    def graph(owner: bpy.types.NodeTree | bpy.types.Node) -> Prepared:
        tree: bpy.types.NodeTree = owner if isinstance(owner, bpy.types.NodeTree) else owner.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        nodes = tuple(sorted(tree.nodes, key=lambda item: item.name)) if isinstance(owner, bpy.types.NodeTree) else upstream(owner)
        reached = frozenset(nodes)
        settings = Descriptor.saved(tree, lambda prop: not isinstance(prop, bpy.types.CollectionProperty))

        def linked(link: bpy.types.NodeLink) -> Outcome[ContentHash]:
            source, outlet, target, inlet = endpoints(link)
            connection = msgspec.msgpack.encode((source.name, outlet.identifier, target.name, inlet.identifier, link.multi_input_sort_id))
            return Snapshot.capture(link, Some(Descriptor.saved(link, lambda prop: not prop.is_readonly))).map(lambda snapshot: content_hash(bpy.types.NodeLink, connection, snapshot))

        captured = tuple(starmap(capture, ((tree, Some(settings)), *((item, Nothing) for item in nodes))))
        links = accumulated(linked(link) for link in tree.links if link.to_node in reached)
        seed = paired(accumulated(result.map(lambda parts: parts[0]) for result in captured), links).map(
            lambda parts: Some(content_hash(type(owner), *parts[0], *parts[1])) if cacheable(captured) else Nothing
        )
        return seed, (*ordered(captured), links.map(lambda _: Nothing))

    def elements(block: bpy.types.ID, *, weighted: bool) -> Prepared:
        shapes = tuple(Option.of_optional(block.shape_keys).to_list()) if isinstance(block, bpy.types.Mesh | bpy.types.Curve | bpy.types.Lattice) else ()
        keys = tuple(key for shape in shapes for key in shape.key_blocks)
        materials = (
            tuple(block.materials)
            if isinstance(block, bpy.types.Mesh | bpy.types.Curve | bpy.types.Curves | bpy.types.PointCloud | bpy.types.GreasePencil | bpy.types.MetaBall | bpy.types.Volume)
            else ()
        )
        drawings = tuple(drawing for layer in block.layers for frame in layer.frames for drawing in Option.of_optional(frame.drawing).to_list()) if isinstance(block, bpy.types.GreasePencil) else ()
        attributes = (
            *(block.attributes if isinstance(block, bpy.types.Mesh | bpy.types.Curves | bpy.types.PointCloud | bpy.types.GreasePencil) else ()),
            *(attribute for drawing in drawings for attribute in drawing.attributes),
        )

        def parts(structs: tuple[Struct, ...]) -> _Parts:
            match block:
                case bpy.types.Mesh():
                    weights = (
                        (np.fromiter((value for index, vertex in enumerate(block.vertices) for element in vertex.groups for value in (index, element.group, element.weight)), np.float64),)
                        if weighted
                        else ()
                    )
                    arrays = (
                        *buffers(block.polygons, (bpy.types.MeshPolygon.bl_rna.properties["loop_start"],)),
                        *(array for key in keys for array in buffers(key.points, bpy.types.ShapeKeyPoint.bl_rna.properties)),
                        *(array for layer in block.skin_vertices for array in buffers(layer.data, bpy.types.MeshSkinVertex.bl_rna.properties)),
                        *weights,
                    )
                    return structs, arrays, ()
                case bpy.types.Curve():
                    profiles = tuple(Option.of_optional(block.bevel_profile).to_list())
                    arrays = (
                        *buffers(block.splines, bpy.types.Spline.bl_rna.properties),
                        *(array for spline in block.splines for array in buffers(spline.points, bpy.types.SplinePoint.bl_rna.properties)),
                        *(array for spline in block.splines for array in buffers(spline.bezier_points, bpy.types.BezierSplinePoint.bl_rna.properties)),
                        *(array for key in keys for array in buffers(key.data, (bpy.types.ShapeKeyPoint.bl_rna.properties["co"],))),
                        *(array for profile in profiles for array in buffers(profile.points, bpy.types.CurveProfilePoint.bl_rna.properties)),
                        *(
                            (*buffers(block.body_format, bpy.types.TextCharacterFormat.bl_rna.properties), *buffers(block.text_boxes, bpy.types.TextBox.bl_rna.properties))
                            if isinstance(block, bpy.types.TextCurve)
                            else ()
                        ),
                    )
                    return (*structs, *profiles), arrays, ()
                case bpy.types.Curves():
                    return structs, buffers(block.curve_offset_data, bpy.types.IntAttributeValue.bl_rna.properties), ()
                case bpy.types.Lattice():
                    arrays = (
                        *buffers(block.points, (descriptor.prop for descriptor in Descriptor.saved(bpy.types.LatticePoint, lambda prop: not prop.is_readonly))),
                        *(array for key in keys for array in buffers(key.points, bpy.types.ShapeKeyPoint.bl_rna.properties)),
                    )
                    return structs, arrays, ()
                case bpy.types.MetaBall():
                    return structs, buffers(block.elements, bpy.types.MetaElement.bl_rna.properties), ()
                case bpy.types.GreasePencil():
                    nodes = (*block.layers, *block.layer_groups)
                    arrays = (
                        *(array for layer in block.layers for array in buffers(layer.frames, bpy.types.GreasePencilFrame.bl_rna.properties)),
                        *(array for drawing in drawings for array in buffers(drawing.curve_offsets, bpy.types.IntAttributeValue.bl_rna.properties)),
                    )
                    labels = tuple((node.name, Option.of_optional(node.parent_group).map(lambda group: group.name).to_optional()) for node in nodes)
                    return (block, *nodes), arrays, labels
                case _:
                    return structs, (), ()

        structs, arrays, labels = parts((block, *shapes, *keys))
        data = tuple(
            (getattr(attribute, data.identifier), item.properties)
            for attribute in attributes
            for data in (definition(attribute).properties["data"],)
            if isinstance(data, bpy.types.CollectionProperty)
            for item in Option.of_optional(data.fixed_type).to_list()
        )
        columns = tuple(array for values, members in data for array in buffers(values, members))
        text = tuple(tuple(getattr(item, member.identifier) for item in values) for values, members in data for member in members if isinstance(member, bpy.types.StringProperty))
        named = msgspec.msgpack.encode((
            tuple((attribute.name, attribute.domain, attribute.data_type) for attribute in attributes),
            tuple(Option.of_optional(material).map(IdName.of).to_optional() for material in materials),
            tuple(key.relative_key.name for key in keys),
            labels,
            *text,
        ))

        def valued(member: bpy.types.Property) -> bool:
            return not (isinstance(member, bpy.types.CollectionProperty) or (isinstance(member, bpy.types.PointerProperty) and member.is_readonly))

        captured = tuple(capture(struct, Some(Descriptor.saved(struct, valued))) for struct in structs)
        seed = accumulated(result.map(lambda parts: parts[0]) for result in captured).map(
            lambda snapshots: Some(content_hash(type(block), *snapshots, named, arrays=(*columns, *arrays))) if cacheable(captured) else Nothing
        )
        return seed, ordered(captured)

    def members(block: bpy.types.ID) -> Prepared:
        native = Capture[Content](
            block,
            lambda descriptor, struct, _inside, _compound: (
                Some(tuple(Option.of_optional(block).map(IdName.of).to_optional() for block in getattr(struct, descriptor.prop.identifier)))
                if isinstance(descriptor.prop, bpy.types.CollectionProperty)
                else Some(descriptor.read(struct))
            ),
            valued=Some(lambda descriptor: not isinstance(descriptor.prop, bpy.types.CollectionProperty | bpy.types.PointerProperty) or isinstance(descriptor.prop.fixed_type, bpy.types.ID)),
            inactive=True,
        )
        captured = (catch(native.tree, exception=ReferenceError)().map_error(partial(Fault.of, Reason.REFUSED, repr(block))).map(lambda tree: (tree, *references(block, tree))),)
        seed = captured[0].map(
            lambda parts: (
                Some(
                    content_hash(
                        type(block),
                        parts[0],
                        *(
                            (msgspec.msgpack.encode(tuple((bone.path_from_id(), Option.of_optional(bone.parent).map(lambda parent: parent.path_from_id()).to_optional()) for bone in block.bones)),)
                            if isinstance(block, bpy.types.Armature)
                            else ()
                        ),
                    )
                )
                if cacheable(captured)
                else Nothing
            )
        )
        return seed, ordered(captured)

    def item(block: bpy.types.Object) -> Prepared:
        earlier = tuple(takewhile(lambda modifier: before_modifier != Some(modifier.name), block.modifiers))
        named = tuple(modifier for modifier in block.modifiers if before_modifier == Some(modifier.name))[:1]
        groups = msgspec.msgpack.encode(tuple(group.name for group in block.vertex_groups))
        data = tuple(Option.of_optional(block.data).to_list())
        selection = Descriptor.saved(block, lambda prop: prop != bpy.types.Object.bl_rna.properties["modifiers"])
        captured = accumulated((capture(block, Some(selection)), *(capture(modifier) for modifier in (*earlier, *named))))
        seed = captured.map(
            lambda parts: (
                Some(content_hash(bpy.types.Object, *(snapshot for snapshot, _, _ in parts), groups))
                if all(not paths and all(isinstance(held, bpy.types.NodeTree) or held in data for held in blocks) for _, blocks, paths in parts)
                else Nothing
            )
        )
        steps: tuple[Step, ...] = (
            captured.map(lambda _: Nothing),
            *(group for modifier in earlier if isinstance(modifier, bpy.types.NodesModifier) for group in Option.of_optional(modifier.node_group).to_list()),
            *((held, bool(block.vertex_groups)) if isinstance(held, bpy.types.Mesh) else held for held in data),
        )
        return seed, steps

    def effect(entry: bpy.types.SceneCompositorEffect) -> Prepared:
        enables = msgspec.msgpack.encode((entry.enable_for_render, entry.enable_for_preview))
        captured = tuple(capture(struct) for struct in struct_at(entry, ("properties", "inputs")).to_list())
        seed = accumulated(result.map(lambda parts: parts[0]) for result in captured).map(
            lambda snapshots: Some(content_hash(bpy.types.SceneCompositorEffect, *snapshots, enables)) if cacheable(captured) else Nothing
        )
        return seed, (*ordered(captured), *Option.of_optional(entry.node_group).to_list())

    def prepare(key: Key) -> Prepared:
        subject, weighted = key if isinstance(key, tuple) else (key, False)
        match subject:
            case bpy.types.Image():
                return image(subject), ()
            case bpy.types.Object():
                return item(subject)
            case bpy.types.SceneCompositorEffect():
                return effect(subject)
            case bpy.types.NodeTree() | bpy.types.Node():
                return graph(subject)
            case bpy.types.Mesh() | bpy.types.Curve() | bpy.types.Curves() | bpy.types.PointCloud() | bpy.types.GreasePencil() | bpy.types.Lattice() | bpy.types.MetaBall() | bpy.types.Volume():
                return elements(subject, weighted=weighted)
            case _:
                return members(subject)

    roots: tuple[Key, ...] = tuple((subject, weighted) if isinstance(subject, bpy.types.Mesh) else subject for subject in (owner if isinstance(owner, tuple) else (owner,)))  # ty: ignore[unsound-assignment]
    captured: dict[Key, Prepared] = {}
    pending = deque(roots)
    while pending:
        if (key := pending.popleft()) not in captured:
            _, steps = captured[key] = prepare(key)
            pending.extend(step for step in steps if not isinstance(step, Result))
    hashes: dict[Key, Reading] = {}
    dependencies = {key: tuple(step for step in steps if not isinstance(step, Result)) for key, (_, steps) in captured.items()}
    for key in TopologicalSorter(dependencies).static_order():
        seed, steps = captured[key]
        hashes[key] = accumulated(step if isinstance(step, Result) else hashes[step] for step in steps).bind(
            lambda readings: seed.map(
                lambda own: reduce(
                    lambda held, needed: held.map2(partial(content_hash, type(key[0] if isinstance(key, tuple) else key)), needed),
                    (reading for step, reading in zip(steps, readings) if not isinstance(step, Result)),
                    own,
                )
            )
        )
    return accumulated(hashes[key] for key in roots) if isinstance(owner, tuple) else hashes[roots[0]]


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Tag", "aspect", "filled", "fingerprinted", "lighting", "loaded", "normalized", "pixels", "read"]
