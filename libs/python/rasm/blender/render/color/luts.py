"""Native LUT formats, stack sampling, sequencer exports, and changed-file reads."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from enum import Enum
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Annotated, ClassVar, Protocol, Self

import bpy
from expression import Error, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm.blender.commands.calls import called
from rasm.blender.commands.operators import Call, Done, Operator, Properties, Save
from rasm.blender.data.ids.ownership import Ownership, Saved
from rasm.blender.data.rna.conditions import AllOf, Count, Job
from rasm.blender.data.rna.descriptors import Descriptor
from rasm.blender.data.rna.snapshots import Snapshot
from rasm.blender.extension.identity import Identity, Key
from rasm.blender.extension.registration import Acquisitions
from rasm.blender.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderFaults, BlenderImagingFault, BlenderReadFault, BlenderResult
from rasm.blender.render.color.spaces import ConfigInstall
from rasm.blender.settings.properties import Choices
from rasm.imaging.chain import Effect, Stage
from rasm.imaging.color.authoring import Formation
from rasm.imaging.color.config import admitted
from rasm.imaging.color.luts import Bake, Exact, Lattice, LutFormat, LutOutput, written
from rasm.imaging.color.processors import applied, processor
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.files.images import ImageFile, Storage
from rasm.imaging.hashing import content_hash, ContentHash

# --- [TYPES] ----------------------------------------------------------------------------


class Enabled(Enum):
    """Native compositor effect enable used by a stack evaluation."""

    RENDER = bpy.types.SceneCompositorEffect.bl_rna.properties["enable_for_render"]
    PREVIEW = bpy.types.SceneCompositorEffect.bl_rna.properties["enable_for_preview"]

    @property
    def descriptor(self) -> Descriptor:
        """Owning RNA property for reads and writes."""
        return Descriptor(root=bpy.types.SceneCompositorEffect, owner="", prop=self.value)


class Forming(Protocol):
    """Scene's ordered effects and their complete native color formation."""

    def config(self) -> BlenderResult[PyOpenColorIO.Config]:
        """Returns the current config admitted over the offered process spaces."""
        ...

    def entries(self, identity: Identity, scene: bpy.types.Scene, selection: Stage | type[Effect]) -> tuple[bpy.types.SceneCompositorEffect, ...]:
        """Returns owned entries of the stage or record type in stack order."""
        ...

    def pointwise(self, identity: Identity, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Admits a pointwise entry or returns its spatial dependency."""
        ...

    def forming(self, identity: Identity, scene: bpy.types.Scene, config: PyOpenColorIO.Config, enabled: Enabled) -> BlenderResult[Option[PyOpenColorIO.Transform]]:
        """Returns the enabled chain, absent when native sampling is required."""
        ...


class Sampling(Protocol):
    """Native evaluation of an RGB plane through ordered scene entries."""

    def sampled(self, identity: Identity, scene: bpy.types.Scene, entries: tuple[bpy.types.SceneCompositorEffect, ...], plane: npt.NDArray[np.float32]) -> BlenderResult[npt.NDArray[np.float32]]:
        """Returns the rendered top-first float32 plane or native sampling failures."""
        ...


# --- [CHOICES] --------------------------------------------------------------------------


def _formats(provider: Callable[[], tuple[LutFormat, ...]]) -> Choices:
    return Choices(lambda _identity, _owner, _context: ((row.name, f"{row.name} (.{row.extension})", "") for row in provider()))


bakeable = _formats(lambda: LutFormat.of(PyOpenColorIO.Baker.getFormats()))
writable = _formats(lambda: LutFormat.of(PyOpenColorIO.GroupTransform.GetWriteFormats()))
matrices = _formats(lambda: tuple(row for row in LutFormat.of(PyOpenColorIO.GroupTransform.GetWriteFormats()) if row in LutFormat.of(PyOpenColorIO.Baker.getFormats())))

# --- [MODELS] ---------------------------------------------------------------------------


class Formatted(msgspec.Struct, frozen=True, kw_only=True):
    """Native format name with its operation's format provider."""

    format: str
    formats: ClassVar[Callable[[], tuple[LutFormat, ...]]]

    def offered(self) -> LutFormat:
        """Returns the selected format or the provider's first dialog item."""
        rows = type(self).formats()
        return next((row for row in rows if row.name == self.format), rows[0])

    def chosen(self) -> BlenderResult[LutFormat]:
        """Admits the requested format without substituting a script's selection."""
        rows = type(self).formats()
        return Option.of_optional(next((row for row in rows if row.name == self.format), None)).to_result_with(
            lambda: (BlenderChoiceFault(path="format", identifier=self.format, accepted=tuple(row.name for row in rows)),)
        )

    def seeded(self, _event: bpy.types.Event) -> Self:
        """Initializes the browser from the provider's offered row."""
        return msgspec.structs.replace(self, format=self.offered().name)

    def extensions(self) -> tuple[str, *tuple[str, ...]]:
        """Returns the offered format's native extension."""
        return (f".{self.offered().extension}",)


class Baking(Formatted, frozen=True, kw_only=True):
    """Native bake format and its sampled lattice."""

    formats = staticmethod(lambda: LutFormat.of(PyOpenColorIO.Baker.getFormats()))
    format: Annotated[str, msgspec.Meta(title="Format"), bakeable]
    lattice: Lattice = Lattice()

    def output(self) -> BlenderResult[LutOutput]:
        """Keeps exact native operators when the format also supports direct writes."""
        return self.chosen().map(lambda row: Exact(format=row) if row in LutFormat.of(PyOpenColorIO.GroupTransform.GetWriteFormats()) else Bake(format=row, lattice=self.lattice))


class Writing(Formatted, frozen=True, kw_only=True):
    """Exact transform output in a native matrix-capable format."""

    formats = staticmethod(lambda: tuple(row for row in LutFormat.of(PyOpenColorIO.GroupTransform.GetWriteFormats()) if row in LutFormat.of(PyOpenColorIO.Baker.getFormats())))
    format: Annotated[str, msgspec.Meta(title="Format"), matrices]

    def output(self) -> BlenderResult[LutOutput]:
        """Returns an exact writer for the admitted native format."""
        return self.chosen().map(lambda row: Exact(format=row))


class ToneView(msgspec.Struct, frozen=True, kw_only=True):
    """Tone entry sampled into a named installed OCIO formation."""

    entry: Annotated[str, msgspec.Meta(min_length=1, title="Effect")]
    view: Annotated[str, msgspec.Meta(min_length=1, title="View")]
    description: str = ""
    lattice: Lattice = Lattice()


class LutExport(msgspec.Struct, frozen=True, kw_only=True):
    """Formation, strip, and tone-view outputs over application-owned color ports."""

    stack: Forming
    sampling: Sampling
    install: ConfigInstall
    lattices: ClassVar[Ownership[bpy.types.Scene]] = Ownership(kind=bpy.types.Scene, key="lattice", saved=Saved.NEVER, single=False)
    formation_bake: ClassVar[Key] = "bake_formation"
    strip_bake: ClassVar[Key] = "bake_strip"
    tone_view: ClassVar[Key] = "install_tone_view"

    def toned(self, identity: Identity, scene: bpy.types.Scene, entries: tuple[bpy.types.SceneCompositorEffect, ...], lattice: Lattice) -> BlenderResult[PyOpenColorIO.GroupTransform]:
        """Samples every RGB channel over the lattice's declared native domain."""
        samples: BlenderResult[npt.NDArray[np.float32]] = lattice.samples().map_error(lambda faults: (BlenderImagingFault(faults=faults),))
        return Result[npt.NDArray[np.float32], BlenderFaults].bind(samples, lambda plane: self.sampling.sampled(identity, scene, entries, plane)).map(lattice.transform)

    def formed(self, call: Call[Baking]) -> BlenderResult[Done]:
        """Writes the scene's enabled formation through the native atomic LUT writer."""
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
            .bind(lambda pair: written(pair[0], {str(call.paths[0]): pair[1]}).map_error(lambda faults: (BlenderImagingFault(faults=faults),)))
            .map(lambda paths: Done(notes=tuple(("INFO", f"Wrote {path}") for path in paths)))
        )

    def viewed(self, call: Call[ToneView]) -> BlenderResult[Done]:
        """Installs a pointwise tone entry as a view activated at the next start."""
        scene, record = call.context.scene, call.record
        if scene is None:
            return Error((BlenderAbsentFault(owner=repr(call.context), key="scene"),))
        entry: BlenderResult[bpy.types.SceneCompositorEffect] = Option.of_optional(
            next((entry for entry in self.stack.entries(call.identity, scene, Stage.TONE) if entry.name == record.entry), None)
        ).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=record.entry),))
        return (
            paired(self.stack.config(), Result[bpy.types.SceneCompositorEffect, BlenderFaults].bind(entry, lambda item: self.stack.pointwise(call.identity, item)))
            .bind(
                lambda pair: self.toned(call.identity, scene, (pair[1],), record.lattice).map(
                    lambda table: Formation.of(record.view, record.description, table, pair[0].getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName())
                )
            )
            .bind(lambda formation: self.install.extended(formation).written())
            .map(lambda path: Done(notes=(("INFO", f"Installed {record.view} into {path}, read at the next Blender start"),)))
        )

    @contextmanager
    def staged(self, identity: Identity) -> Generator[BlenderResult[bpy.types.Scene]]:
        """Owns a marked runtime scene through every render and failure path."""
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
            samples: BlenderResult[npt.NDArray[np.float32]] = lattice.samples().map_error(lambda faults: (BlenderImagingFault(faults=faults),))
            return (
                Result[npt.NDArray[np.float32], BlenderFaults]
                .bind(
                    samples,
                    lambda plane: ImageFile(pixels=plane, colorspace=space).write(str(folder / "nodes.exr"), Storage(), {}, config).map_error(lambda faults: (BlenderImagingFault(faults=faults),)),
                )
                .bind(lambda nodes: arranged(scene, nodes, folder / "look"))
                .bind(lambda _: called(bpy.ops.render.render, {"scene": scene.name, "write_still": True}))
                .bind(lambda _: ImageFile.read(scene.render.frame_path(frame=1), config).map_error(lambda faults: (BlenderImagingFault(faults=faults),)))
            )

        def tabled(config: PyOpenColorIO.Config, look: ImageFile[np.float32]) -> BlenderResult[PyOpenColorIO.GroupTransform]:
            return (
                processor(config, PyOpenColorIO.ColorSpaceTransform(src=look.colorspace, dst=space))
                .map(lambda built: lattice.transform(applied(built.getDefaultCPUProcessor(), np.ascontiguousarray(look.pixels[..., :3]))))
                .map_error(lambda faults: (BlenderImagingFault(faults=faults),))
            )

        with self.staged(call.identity) as made, TemporaryDirectory(dir=bpy.app.tempdir) as folder:
            config: BlenderResult[PyOpenColorIO.Config] = admitted(PyOpenColorIO.GetCurrentConfig(), (space,)).map_error(lambda faults: (BlenderImagingFault(faults=faults),))
            return (
                paired(paired(made, config), call.record.output())
                .bind(lambda parts: rendered(*parts[0], Path(folder)).bind(lambda look: tabled(parts[0][1], look)).map(lambda built: (parts[1], built)))
                .bind(lambda pair: written(pair[0], {str(call.paths[0]): pair[1]}).map_error(lambda faults: (BlenderImagingFault(faults=faults),)))
                .map(lambda paths: Done(notes=tuple(("INFO", f"Wrote {path}") for path in paths)))
            )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns nested ownership and original operator declarations once."""

        def rows(modifiers: Descriptor[bpy.types.CollectionProperty]) -> Acquisitions:
            def stripped(call: Call[Baking]) -> BlenderResult[Done]:
                match modifiers.owner_of(call.context).to_optional():
                    case bpy.types.Strip(id_data=bpy.types.Scene(sequencer_colorspace_settings=bpy.types.ColorManagedSequencerColorspaceSettings(name=space))) as active:
                        return self.sequenced(call, active, space)
                    case _:
                        return Error((BlenderAbsentFault(owner=repr(modifiers.root), key=modifiers.owner),))

            saved, seed = Some(Save[Baking](extensions=Baking.extensions)), Some(Baking.seeded)
            available = Job(kind="RENDER")
            ready = Some(available)
            return (
                self.lattices,
                Operator(
                    key=self.formation_bake,
                    title="Bake Formation LUT",
                    description="Bakes enabled scene formation into a native LUT",
                    form=Baking,
                    run=self.formed,
                    condition=ready,
                    invoke=saved,
                    seed=seed,
                    undo=False,
                ),
                Operator(
                    key=self.strip_bake,
                    title="Bake Strip LUT",
                    description="Bakes the active strip's modifiers in sequencer space",
                    form=Baking,
                    run=stripped,
                    condition=Some(AllOf(conditions=(Count(collection=modifiers), available))),
                    invoke=saved,
                    seed=seed,
                    undo=False,
                ),
                Operator(
                    key=self.tone_view,
                    title="Install Tone View",
                    description="Installs a sampled tone formation for the next Blender start",
                    form=ToneView,
                    run=self.viewed,
                    condition=ready,
                    invoke=Some(Properties()),
                    seed=Some(
                        lambda record, _event: (
                            Option
                            .of_optional(bpy.context.scene)
                            .bind(lambda scene: Option.of_optional(next(iter(self.stack.entries(identity, scene, Stage.TONE)), None)))
                            .map(lambda entry: msgspec.structs.replace(record, entry=entry.name))
                            .default_value(record)
                        )
                    ),
                    undo=False,
                ),
            )

        return Descriptor.resolve(bpy.types.WorkSpace, "sequencer_scene.sequence_editor.active_strip.modifiers", bpy.types.CollectionProperty).map(rows)


# --- [SERVICES] -------------------------------------------------------------------------


class LutFiles(msgspec.Struct, frozen=True, kw_only=True):
    """Last observed content of files in OCIO's process-wide native caches."""

    contents: dict[str, ContentHash] = msgspec.field(default_factory=dict)

    def current(self, path: str, config: PyOpenColorIO.Config) -> BlenderResult[str]:
        """Reads current content and clears native caches when that path has changed."""
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
