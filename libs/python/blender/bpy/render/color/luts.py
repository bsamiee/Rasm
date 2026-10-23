"""LUT formats, stack sampling, sequencer exports, and changed-file reads."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from enum import Enum
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Annotated, ClassVar, override, Protocol, Self

import bpy
from expression import Error, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Call, Done, Operator, Properties, Save
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.conditions import AllOf, Count, Job
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderFaults, BlenderImagingFault, BlenderReadFault, BlenderResult
from blender.bpy.render.color.spaces import config_of, ConfigInstall
from blender.bpy.settings.properties import Choices
from blender.imaging.chain import Effect, Stage
from blender.imaging.color.authoring import Formation
from blender.imaging.color.luts import Bake, Exact, Lattice, LutFormat, LutOutput, written
from blender.imaging.color.processors import applied, processor
from blender.imaging.faults import accumulated, paired
from blender.imaging.files.images import ImageFile, read_file, Storage
from blender.imaging.hashing import content_hash, ContentHash

# --- [TYPES] ----------------------------------------------------------------------------


class Enabled(Enum):
    """Compositor effect enable flag a stack evaluation reads."""

    RENDER = bpy.types.SceneCompositorEffect.bl_rna.properties["enable_for_render"]
    PREVIEW = bpy.types.SceneCompositorEffect.bl_rna.properties["enable_for_preview"]

    @property
    def descriptor(self) -> Descriptor:
        """Owning RNA property for reads and writes."""
        return Descriptor(root=bpy.types.SceneCompositorEffect, owner="", prop=self.value)


class Forming(Protocol):
    """Scene's ordered effects and their color formation."""

    def config(self) -> BlenderResult[PyOpenColorIO.Config]:
        """Returns config validated over offered process spaces."""
        ...

    def entries(self, identity: Identity, scene: bpy.types.Scene, selection: Stage | type[Effect]) -> tuple[bpy.types.SceneCompositorEffect, ...]:
        """Returns owned stage or record entries in stack order."""
        ...

    def pointwise(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Returns entry when pointwise, else its spatial dependency fault."""
        ...

    def forming(self, identity: Identity, scene: bpy.types.Scene, config: PyOpenColorIO.Config, enabled: Enabled) -> BlenderResult[Option[PyOpenColorIO.Transform]]:
        """Returns enabled chain, Nothing when native sampling is needed."""
        ...


class Sampling(Protocol):
    """Native evaluation of an RGB plane through ordered scene entries."""

    def sampled(self, identity: Identity, scene: bpy.types.Scene, entries: tuple[bpy.types.SceneCompositorEffect, ...], plane: npt.NDArray[np.float32]) -> BlenderResult[npt.NDArray[np.float32]]:
        """Returns rendered top-first float32 plane."""
        ...


# --- [CHOICES] --------------------------------------------------------------------------


def _formats(provider: Callable[[], tuple[LutFormat, ...]]) -> Choices:
    return Choices(lambda _identity, _owner, _context: ((row.name, f"{row.name} (.{row.extension})", "") for row in provider()))


bakeable = _formats(LutFormat.bakeable)
writable = _formats(LutFormat.writable)
matrices = _formats(LutFormat.matrices)

# --- [MODELS] ---------------------------------------------------------------------------


class Formatted(msgspec.Struct, frozen=True, kw_only=True):
    """Format name with its operation's format provider."""

    format: str
    formats: ClassVar[Callable[[], tuple[LutFormat, ...]]]

    def offered(self) -> LutFormat:
        """Returns selected format or provider's first dialog item."""
        rows = type(self).formats()
        return next((row for row in rows if row.name == self.format), rows[0])

    def chosen(self) -> BlenderResult[LutFormat]:
        """Returns requested format, a choice fault when absent."""
        rows = type(self).formats()
        return Option.of_optional(next((row for row in rows if row.name == self.format), None)).to_result_with(
            lambda: (BlenderChoiceFault(path="format", identifier=self.format, accepted=tuple(row.name for row in rows)),)
        )

    def seeded(self, _event: bpy.types.Event, _identity: Identity) -> Self:
        """Seeds file browser with offered format."""
        return msgspec.structs.replace(self, format=self.offered().name)

    def extensions(self) -> tuple[str, *tuple[str, ...]]:
        """Returns offered format's extension."""
        return (f".{self.offered().extension}",)

    def output(self) -> BlenderResult[LutOutput]:
        """Returns exact writer for chosen native format."""
        return self.chosen().map(lambda row: Exact(format=row))


class Baking(Formatted, frozen=True, kw_only=True):
    """Native bake format and its sampled lattice."""

    formats = staticmethod(LutFormat.bakeable)
    format: Annotated[str, msgspec.Meta(title="Format"), bakeable]
    lattice: Lattice = Lattice()

    @override
    def output(self) -> BlenderResult[LutOutput]:
        """Returns exact writer for a writable format, else a lattice bake."""
        return self.chosen().map(lambda row: Exact(format=row) if row in LutFormat.writable() else Bake(format=row, lattice=self.lattice))


class Writing(Formatted, frozen=True, kw_only=True):
    """Exact transform output in a native matrix-capable format."""

    formats = staticmethod(LutFormat.matrices)
    format: Annotated[str, msgspec.Meta(title="Format"), matrices]


class ToneView(msgspec.Struct, frozen=True, kw_only=True):
    """Tone entry sampled into a named installed OCIO formation."""

    entry: Annotated[str, msgspec.Meta(min_length=1, title="Effect")]
    view: Annotated[str, msgspec.Meta(min_length=1, title="View")]
    description: str = ""
    lattice: Lattice = Lattice()


class LutExport:
    """Formation, strip, and tone-view LUT outputs."""

    stack: Forming
    sampling: Sampling
    install: ConfigInstall
    forming: Operator[Baking]
    stripping: Operator[Baking]
    toning: Operator[ToneView]
    lattices: ClassVar[Ownership[bpy.types.Scene]] = Ownership(kind=bpy.types.Scene, key="lattice", saved=Saved.NEVER, single=False)
    modifiers: ClassVar[Descriptor[bpy.types.CollectionProperty]] = Descriptor.declared(bpy.types.WorkSpace, "sequencer_scene.sequence_editor.active_strip.modifiers", bpy.types.CollectionProperty)

    def __init__(self, *, stack: Forming, sampling: Sampling, install: ConfigInstall) -> None:
        """Declares formation, strip, and tone-view commands."""
        self.stack, self.sampling, self.install = stack, sampling, install
        saved, seed, available = Some(Save[Baking](extensions=Baking.extensions)), Some(Baking.seeded), Job(kind="RENDER")
        self.forming = Operator(
            key="bake_formation",
            title="Bake Formation LUT",
            description="Bake enabled scene formation into a native LUT",
            form=Baking,
            run=self.formed,
            condition=Some(available),
            invoke=saved,
            seed=seed,
            undo=False,
        )
        self.stripping = Operator(
            key="bake_strip",
            title="Bake Strip LUT",
            description="Bake active strip modifiers in sequencer space",
            form=Baking,
            run=self.stripped,
            condition=Some(AllOf(conditions=(Count(collection=self.modifiers), available))),
            invoke=saved,
            seed=seed,
            undo=False,
        )
        self.toning = Operator(
            key="install_tone_view",
            title="Install Tone View",
            description="Install sampled tone entry as a view read on restart",
            form=ToneView,
            run=self.viewed,
            condition=Some(available),
            invoke=Some(Properties()),
            seed=Some(
                lambda record, _event, identity: (
                    Option
                    .of_optional(bpy.context.scene)
                    .bind(lambda scene: Option.of_optional(next(iter(self.stack.entries(identity, scene, Stage.TONE)), None)))
                    .map(lambda entry: msgspec.structs.replace(record, entry=entry.name))
                    .default_value(record)
                )
            ),
            undo=False,
        )

    def toned(self, identity: Identity, scene: bpy.types.Scene, entries: tuple[bpy.types.SceneCompositorEffect, ...], lattice: Lattice) -> BlenderResult[PyOpenColorIO.GroupTransform]:
        """Samples every RGB channel over lattice domain."""
        samples: BlenderResult[npt.NDArray[np.float32]] = lattice.samples().map_error(BlenderImagingFault.lifted)
        return Result[npt.NDArray[np.float32], BlenderFaults].bind(samples, lambda plane: self.sampling.sampled(identity, scene, entries, plane)).map(lattice.transform)

    def formed(self, call: Call[Baking]) -> BlenderResult[Done]:
        """Writes enabled scene formation through atomic LUT writer."""
        scene, lattice = call.context.scene, call.record.lattice
        if scene is None:
            return Error((BlenderAbsentFault(owner=repr(call.context), key="scene"),))

        def sampled() -> BlenderResult[PyOpenColorIO.GroupTransform]:
            return self.toned(call.identity, scene, tuple(entry for entry in scene.compositor_effects if bool(Enabled.RENDER.descriptor.read(entry))), lattice)

        def source(config: PyOpenColorIO.Config, output: LutOutput) -> BlenderResult[PyOpenColorIO.GroupTransform]:
            return self.stack.forming(call.identity, scene, config, Enabled.RENDER).bind(
                lambda chain: chain.map(lambda transform: Ok(PyOpenColorIO.GroupTransform([transform]))).default_with(sampled) if isinstance(output, Exact) else sampled()
            )

        return (
            paired(self.stack.config(), call.record.output())
            .bind(lambda pair: source(*pair).map(lambda built: (pair[1], built)))
            .bind(lambda pair: written(pair[0], {str(call.paths[0]): pair[1]}).map_error(BlenderImagingFault.lifted))
            .map(Done.wrote)
        )

    def viewed(self, call: Call[ToneView]) -> BlenderResult[Done]:
        """Installs pointwise tone entry as a view read on restart."""
        scene, record = call.context.scene, call.record
        if scene is None:
            return Error((BlenderAbsentFault(owner=repr(call.context), key="scene"),))
        entry: BlenderResult[bpy.types.SceneCompositorEffect] = Option.of_optional(
            next((entry for entry in self.stack.entries(call.identity, scene, Stage.TONE) if entry.name == record.entry), None)
        ).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=record.entry),))
        return (
            paired(self.stack.config(), Result[bpy.types.SceneCompositorEffect, BlenderFaults].bind(entry, lambda item: self.stack.pointwise(call.identity, scene, item)))
            .bind(
                lambda pair: self.toned(call.identity, scene, (pair[1],), record.lattice).map(
                    lambda table: Formation.of(record.view, record.description, table, pair[0].getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName())
                )
            )
            .bind(lambda formation: self.install.extended(formation).written())
            .map(lambda path: Done(notes=(("INFO", f"Installed view {record.view} into {path}, restart Blender to read it"),)))
        )

    @contextmanager
    def staged(self, identity: Identity) -> Generator[BlenderResult[bpy.types.Scene]]:
        """Owns a marked runtime scene, removed on exit."""
        made = self.lattices.made(identity, lambda: Ok(bpy.data.scenes.new("LUT Lattice")))
        try:
            yield made
        finally:
            for scene in made.to_option().to_list():
                bpy.data.scenes.remove(scene)

    def sequenced(self, call: Call[Baking], active: bpy.types.Strip, space: str) -> BlenderResult[Done]:
        """Samples strip modifiers in one sequencer render over their native space."""
        lattice = call.record.lattice

        def arranged(scene: bpy.types.Scene, nodes: str, look: Path) -> BlenderResult[None]:
            match scene.sequence_editor_create().strips.new_image(name="Lattice", filepath=nodes, channel=1, frame_start=1), scene.sequencer_colorspace_settings:
                case bpy.types.ImageStrip(
                    colorspace_settings=bpy.types.ColorManagedInputColorspaceSettings() as strip_space
                ) as strip, bpy.types.ColorManagedSequencerColorspaceSettings() as sequencer:
                    render = scene.render
                    render.resolution_x, render.resolution_y, render.resolution_percentage = lattice.size**2, lattice.size, 100
                    render.use_sequencer, render.use_compositing, render.filepath = True, False, str(look)
                    render.image_settings.file_format, render.image_settings.color_depth = "OPEN_EXR", "32"
                    scene.frame_start = scene.frame_end = 1
                    sequencer.name = strip_space.name = space  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                    return accumulated(
                        Snapshot.capture(modifier).bind(lambda snapshot: snapshot.apply(strip.modifiers.new(name=modifier.name, type=modifier.type)).outcome) for modifier in active.modifiers
                    ).map(lambda _: None)
                case strip, _:
                    return Error((BlenderAbsentFault(owner=repr(strip), key="colorspace_settings"),))

        def rendered(scene: bpy.types.Scene, config: PyOpenColorIO.Config, folder: Path) -> BlenderResult[ImageFile[np.float32]]:
            samples: BlenderResult[npt.NDArray[np.float32]] = lattice.samples().map_error(BlenderImagingFault.lifted)
            return (
                Result[npt.NDArray[np.float32], BlenderFaults]
                .bind(samples, lambda plane: ImageFile(pixels=plane, colorspace=space).write(str(folder / "nodes.exr"), Storage(), {}, config).map_error(BlenderImagingFault.lifted))
                .bind(lambda nodes: arranged(scene, nodes, folder / "look"))
                .bind(lambda _: called(bpy.ops.render.render, {"scene": scene.name, "animation": True}))
                .bind(lambda _: read_file(scene.render.frame_path(frame=1), config).map_error(BlenderImagingFault.lifted))
            )

        def tabled(config: PyOpenColorIO.Config, look: ImageFile[np.float32]) -> BlenderResult[PyOpenColorIO.GroupTransform]:
            return (
                processor(config, PyOpenColorIO.ColorSpaceTransform(src=look.colorspace, dst=space))
                .map(lambda built: lattice.transform(applied(built.getDefaultCPUProcessor(), np.ascontiguousarray(look.pixels[..., :3]))))
                .map_error(BlenderImagingFault.lifted)
            )

        with self.staged(call.identity) as made, TemporaryDirectory(dir=bpy.app.tempdir) as folder:
            return (
                paired(paired(made, config_of((space,))), call.record.output())
                .bind(lambda parts: rendered(*parts[0], Path(folder)).bind(lambda look: tabled(parts[0][1], look)).map(lambda built: (parts[1], built)))
                .bind(lambda pair: written(pair[0], {str(call.paths[0]): pair[1]}).map_error(BlenderImagingFault.lifted))
                .map(Done.wrote)
            )

    def stripped(self, call: Call[Baking]) -> BlenderResult[Done]:
        """Bakes active strip modifiers in the strip scene's sequencer space."""
        match self.modifiers.owner_of(call.context).to_optional():
            case bpy.types.Strip(id_data=bpy.types.Scene(sequencer_colorspace_settings=bpy.types.ColorManagedSequencerColorspaceSettings(name=space))) as active:
                return self.sequenced(call, active, space)
            case _:
                return Error((BlenderAbsentFault(owner=repr(self.modifiers.root), key=self.modifiers.owner),))

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns lattice ownership and LUT commands."""
        return Ok((self.lattices, self.forming, self.stripping, self.toning))


# --- [SERVICES] -------------------------------------------------------------------------


class LutFiles(msgspec.Struct, frozen=True, kw_only=True):
    """Last seen content of files in OCIO process-wide caches."""

    contents: dict[str, ContentHash] = msgspec.field(default_factory=dict)

    def current(self, path: str, config: PyOpenColorIO.Config) -> BlenderResult[str]:
        """Returns resolved path after clearing native caches on changed content."""
        resolved = str(Path(bpy.path.abspath(path)).resolve())

        def held(content: bytes) -> str:
            digest = content_hash(LutFiles, content)
            if self.contents.get(resolved) != digest:
                PyOpenColorIO.ClearAllCaches()
                config.clearProcessorCache()
                self.contents[resolved] = digest
            return resolved

        return catch(lambda: Path(resolved).read_bytes(), exception=OSError)().map_error(lambda error: (BlenderReadFault(request=resolved, error=error),)).map(held)


# --- [COMPOSITION] ----------------------------------------------------------------------


lut_files = LutFiles()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Baking", "Enabled", "Formatted", "Forming", "LutExport", "LutFiles", "Sampling", "ToneView", "Writing", "bakeable", "lut_files", "matrices", "writable"]
