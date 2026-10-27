"""Scene and display node views, Blender tint, and OCIO installation."""

from collections.abc import Callable, Iterable
from pathlib import Path
from typing import Final

import bpy
from expression import Ok
import msgspec
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, Operator
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import assigned, Descriptor, RnaPath, Value, written
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.imaging.color.authoring import ConfigBuild, ConfigFile
from blender.imaging.color.processors import ColorSpace, DisplayView
from blender.imaging.color.white import White
from blender.records.faults import accumulated, Outcome

# --- [CONSTANTS] ------------------------------------------------------------------------

CONFIGURED: Final = Setting(descriptor=Descriptor.declared(bpy.types.BlendData, ("colorspace", "is_missing_opencolorio_config"), bpy.types.BoolProperty), accepted=(False,))
NO_LOOK: Final = "None"
TINT_PER_DUV: Final = 3000.0

# --- [MODELS] ---------------------------------------------------------------------------


class ConfigView(msgspec.Struct, frozen=True, kw_only=True):
    """View transform an installation adds, its operation built over the base configuration in its process space."""

    name: str
    description: str
    transform: Callable[[PyOpenColorIO.Config], Outcome[PyOpenColorIO.Transform]]
    space: ColorSpace = PyOpenColorIO.ROLE_SCENE_LINEAR

    def view_transform(self, base: PyOpenColorIO.Config) -> Outcome[PyOpenColorIO.ViewTransform]:
        """Scene-referred view transform of the operation over base, entered from the scene interchange role and left to the display interchange role in the process space."""
        return self.transform(base).map(
            lambda transform: PyOpenColorIO.ViewTransform(
                referenceSpace=PyOpenColorIO.REFERENCE_SPACE_SCENE,
                name=self.name,
                description=self.description,
                fromReference=PyOpenColorIO.GroupTransform([
                    PyOpenColorIO.ColorSpaceTransform(src=PyOpenColorIO.ROLE_INTERCHANGE_SCENE, dst=self.space),
                    transform,
                    PyOpenColorIO.ColorSpaceTransform(src=self.space, dst=PyOpenColorIO.ROLE_INTERCHANGE_DISPLAY),
                ]),
            )
        )


class ConfigInstall(msgspec.Struct, frozen=True, kw_only=True):
    """Application OCIO rows and views installed over Blender's bundled configuration."""

    build: ConfigBuild
    views: tuple[ConfigView, ...] = ()

    @property
    def base(self) -> Outcome[PyOpenColorIO.Config]:
        """Bundled base configuration independent of a user installation."""
        return ConfigFile(path=str(Path(bpy.utils.system_resource("DATAFILES", path="colormanagement"), "config.ocio"))).opened()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    def merged(self, base: PyOpenColorIO.Config) -> Outcome[ConfigBuild]:
        """Application build with the views' view transforms added."""
        return accumulated(view.view_transform(base) for view in self.views).map(lambda rows: msgspec.structs.replace(self.build, view_transforms=(*self.build.view_transforms, *rows)))

    def staged(self, stage: Path) -> Outcome[PyOpenColorIO.Config]:
        """Stages full build in caller's resource scope."""
        return self.base.bind(lambda base: self.merged(base).bind(lambda build: build.staged(base, stage)))

    def written(self) -> Outcome[Path]:
        """Installs complete configuration read on restart."""
        return self.base.bind(lambda base: self.merged(base).bind(lambda build: build.installed(base, Path(bpy.utils.user_resource("DATAFILES", path="colormanagement")))))

    def install_config(self, _call: Call[Empty]) -> Outcome[Done]:
        """Install extension view transforms read on restart."""
        return self.written().map(lambda path: Done(notes=(("INFO", f"Installed {path}, restart Blender to read it"),)))

    @property
    def installing(self) -> Operator[Empty]:
        """Command installing the build."""
        return Operator(form=Empty, run=self.install_config, undo=False)

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns install command."""
        return Ok((self.installing,))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [COLORS]


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
    return DisplayView(display=shown.display_device, view=view.view_transform, looks_override=view.look, looks_override_enabled=view.look != NO_LOOK, exposure=view.exposure, gamma=view.gamma)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


def displayed(scene: bpy.types.Scene, view: DisplayView) -> Outcome[None]:
    """Writes a view to the scene in host order, the display before the view settings it governs, outputs overriding color management keeping their own."""
    settings: dict[RnaPath, Value] = {
        ("display_settings", "display_device"): view.display,
        ("view_settings", "view_transform"): view.view,
        ("view_settings", "look"): view.looks_override if view.looks_override_enabled else NO_LOOK,
        ("view_settings", "exposure"): view.exposure,
        ("view_settings", "gamma"): view.gamma,
    }
    return assigned(scene, settings).map(lambda _: None)


def followed(scene: bpy.types.Scene, nodes: Iterable[bpy.types.CompositorNodeConvertToDisplay]) -> Outcome[tuple[tuple[bpy.types.CompositorNodeConvertToDisplay, DisplayView], ...]]:
    """Writes the scene display device to each display node showing another, answering each written node with the view Blender matched to the device."""
    display = viewed(scene).display
    return accumulated(written(node, ("display_settings", "display_device"), display).map(lambda _: (node, viewed(node))) for node in nodes if viewed(node).display != display)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CONFIGURED", "NO_LOOK", "TINT_PER_DUV", "ConfigInstall", "ConfigView", "displayed", "followed", "tint_of", "viewed", "white_of"]
