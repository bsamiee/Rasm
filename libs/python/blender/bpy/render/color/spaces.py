"""Working-space effect color follow-through, scene and display node views, Blender tint, and OCIO installation."""

from collections.abc import Callable, Generator, Iterable
from contextlib import contextmanager, ExitStack
from pathlib import Path
from typing import ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, Operator
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import assigned, Descriptor, Native, struct_at, Value, written
from blender.bpy.extension.events.handlers import UpdateHandler
from blender.bpy.extension.events.timers import Startup
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderResult
from blender.bpy.render.compositor.inputs import inputs_of
from blender.bpy.render.outputs.formats import owners
from blender.bpy.settings.attachments import Stored
from blender.imaging.color.authoring import ConfigBuild, Formation
from blender.imaging.color.config import ConfigFile, opened, resolved
from blender.imaging.color.processors import ColorSpace, DisplayView, processor
from blender.imaging.color.white import White
from blender.imaging.faults import ImagingResult
from blender.records.faults import accumulated
from blender.records.fields import Gated

# --- [CONSTANTS] ------------------------------------------------------------------------

CONFIGURED: Final = Setting(descriptor=Descriptor.declared(bpy.types.BlendData, "colorspace.is_missing_opencolorio_config", bpy.types.BoolProperty), accepted=(False,))
NO_LOOK: Final = "None"
TINT_PER_DUV: Final = 3000.0

# --- [MODELS] ---------------------------------------------------------------------------


class WorkingColors(msgspec.Struct, frozen=True, kw_only=True):
    """Effect color inputs native working space conversion skips, carried into each new working space from the space each scene last held them in."""

    stamp: ClassVar[Key] = "working_space"

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Holds depsgraph check converting effect colors in one undo step after a working space change, which publishes no property notification."""
        previous: Option[str] = Nothing

        def seeded() -> BlenderResult[None]:
            nonlocal previous
            previous = Some(bpy.data.colorspace.working_space)
            return Ok(None)

        def source(stamped: Stored[str], scene: bpy.types.Scene, target: str) -> BlenderResult[str]:
            return stamped.read(scene).map(lambda held: held.or_else(previous).default_value(target))

        def converted(stamped: Stored[str]) -> BlenderResult[Option[str]]:
            target = bpy.data.colorspace.working_space

            def scene_converted(scene: bpy.types.Scene, start: str) -> BlenderResult[int]:
                inputs = effect_colors(scene)

                def recolored(cpu: PyOpenColorIO.CPUProcessor) -> BlenderResult[int]:
                    for _item, _descriptor, values in inputs:
                        cpu.applyRGB(values[:3])
                    return (
                        accumulated(descriptor.write(item, tuple(values.tolist())) for item, descriptor, values in inputs)
                        .map(lambda priors: sum(prior.is_some() for prior in priors))
                        .bind(lambda count: stamped.write(scene, target).map(lambda _: count))
                    )

                return (
                    config_of((start, target))
                    .bind(lambda config: processor(config, PyOpenColorIO.ColorSpaceTransform(src=start, dst=target)).map(PyOpenColorIO.Processor.getDefaultCPUProcessor))
                    .bind(recolored)
                )

            def each(scene: bpy.types.Scene) -> BlenderResult[Option[int]]:
                return source(stamped, scene, target).bind(lambda start: Ok(Nothing) if start == target else scene_converted(scene, start).map(Some))

            counted = accumulated(each(scene) for scene in bpy.data.scenes)
            return seeded().bind(lambda _: counted).map(lambda counts: Some("Convert Effect Colors") if any(count.is_some() for count in counts) else Nothing)

        with ExitStack() as scope:

            def followed(stamped: Stored[str]) -> Acquisitions:
                later = scope.enter_context(identity.reports.deferred(lambda: converted(stamped)))

                def checked(scene: bpy.types.Scene, _updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> BlenderResult[None]:
                    target = bpy.data.colorspace.working_space
                    return source(stamped, scene, target).map(lambda start: None if start == target else later())

                return Startup(body=seeded), UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.ID,), body=checked)

            yield identity.name(Name.ATTACHMENT, self.stamp).map(lambda name: followed(Stored(name=name, decoder=msgspec.msgpack.Decoder(str))))


class ConfigView(msgspec.Struct, frozen=True, kw_only=True):
    """View transform an installation adds, its operation built over the base configuration in its process space."""

    name: str
    description: str
    transform: Callable[[PyOpenColorIO.Config], ImagingResult[PyOpenColorIO.Transform]]
    space: ColorSpace = PyOpenColorIO.ROLE_SCENE_LINEAR

    def formation(self, base: PyOpenColorIO.Config) -> BlenderResult[Formation]:
        """Native formation of the view's operation over base."""
        return self.transform(base).map(lambda transform: Formation.of(self.name, self.description, transform, self.space))


class ConfigInstall(msgspec.Struct, frozen=True, kw_only=True):
    """Application OCIO rows and views installed over Blender's bundled configuration."""

    build: ConfigBuild
    views: tuple[ConfigView, ...] = ()

    @property
    def base(self) -> BlenderResult[PyOpenColorIO.Config]:
        """Bundled base configuration independent of a user installation."""
        return opened(ConfigFile(path=str(Path(bpy.utils.system_resource("DATAFILES", path="colormanagement"), "config.ocio"))), ())  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    def merged(self, base: PyOpenColorIO.Config) -> BlenderResult[ConfigBuild]:
        """Application build with the views' formations added."""
        return accumulated(view.formation(base) for view in self.views).map(lambda formations: msgspec.structs.replace(self.build, formations=(*self.build.formations, *formations)))

    def staged(self, stage: Path) -> BlenderResult[PyOpenColorIO.Config]:
        """Stages full build in caller's resource scope."""
        return self.base.bind(lambda base: self.merged(base).bind(lambda build: build.staged(base, stage)))

    def written(self) -> BlenderResult[Path]:
        """Installs complete configuration read on restart."""
        return self.base.bind(lambda base: self.merged(base).bind(lambda build: build.installed(base, Path(bpy.utils.user_resource("DATAFILES", path="colormanagement")))))

    def installed(self, _call: Call[Empty]) -> BlenderResult[Done]:
        """Installs build with a restart note."""
        return self.written().map(lambda path: Done(notes=(("INFO", f"Installed {path}, restart Blender to read it"),)))

    @property
    def installing(self) -> Operator[Empty]:
        """Command installing the build."""
        return Operator(key="install_config", title="Install Color Config", description="Install extension formations read on restart", form=Empty, run=self.installed, undo=False)

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns install command."""
        return Ok((self.installing,))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [COLORS]


def config_of(spaces: Iterable[str] = ()) -> BlenderResult[PyOpenColorIO.Config]:
    """Returns live OCIO config, a fault per unresolved required role or given space."""
    return resolved(PyOpenColorIO.GetCurrentConfig(), spaces)


def effect_colors(scene: bpy.types.Scene) -> tuple[tuple[Native, Descriptor[bpy.types.FloatProperty], npt.NDArray[np.float32]], ...]:
    """Returns color socket values of every scene effect group input with its writer."""
    return tuple(
        (item, descriptor, np.asarray(descriptor.read(item), dtype=np.float32).copy())
        for effect in scene.compositor_effects
        for inputs in inputs_of(effect).to_option().to_list()
        if isinstance(inputs, bpy.types.bpy_struct)
        for prop in inputs.bl_rna.properties
        if isinstance(prop, bpy.types.PointerProperty)
        for item in struct_at(inputs, prop.identifier).to_list()
        if isinstance(item, bpy.types.bpy_struct)
        if "value" in item.bl_rna.properties
        for color in (item.bl_rna.properties["value"],)
        if isinstance(color, bpy.types.FloatProperty) and color.subtype == "COLOR"
        for descriptor in (Descriptor(root=type(item), owner="", prop=color),)
    )


def white_of(temperature: float, tint: float) -> White:
    """Returns white of a Blender temperature and tint, the tint `TINT_PER_DUV` per unit Duv."""
    return White(temperature=temperature, duv=tint / TINT_PER_DUV)


def tint_of(white: White) -> float:
    """Returns the Blender tint of `white`."""
    return white.duv * TINT_PER_DUV


# --- [VIEWS]


def viewed(holder: bpy.types.Scene | bpy.types.CompositorNodeConvertToDisplay) -> DisplayView:
    """Reads the display, view, look, exposure, and gamma a scene or display node forms through, the host's look `NO_LOOK` as disabled."""
    view, shown = holder.view_settings, holder.display_settings
    return DisplayView(display=shown.display_device, view=view.view_transform, look=Gated(enabled=view.look != NO_LOOK, value=view.look), exposure=view.exposure, gamma=view.gamma)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


def displayed(scene: bpy.types.Scene, view: DisplayView) -> BlenderResult[None]:
    """Writes a view to the scene and every output overriding color management in host order, the display before the view settings it governs."""
    settings: dict[str, Value] = {
        "display_settings.display_device": view.display,
        "view_settings.view_transform": view.view,
        "view_settings.look": view.look.value if view.look.enabled else NO_LOOK,
        "view_settings.exposure": view.exposure,
        "view_settings.gamma": view.gamma,
    }
    holders: tuple[bpy.types.Scene | bpy.types.ImageFormatSettings, ...] = (scene, *(owner for owner in owners(scene) if owner.color_management == "OVERRIDE"))
    return accumulated(assigned(holder, settings) for holder in holders).map(lambda _: None)


def followed(scene: bpy.types.Scene, nodes: Iterable[bpy.types.CompositorNodeConvertToDisplay]) -> BlenderResult[tuple[tuple[bpy.types.CompositorNodeConvertToDisplay, DisplayView], ...]]:
    """Writes the scene display device to each display node showing another, answering each written node with the view Blender matched to the device."""
    display = viewed(scene).display
    return accumulated(written(node, "display_settings.display_device", display).map(lambda _: (node, viewed(node))) for node in nodes if viewed(node).display != display)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CONFIGURED", "NO_LOOK", "TINT_PER_DUV", "ConfigInstall", "ConfigView", "WorkingColors", "config_of", "displayed", "effect_colors", "followed", "tint_of", "viewed", "white_of"]
