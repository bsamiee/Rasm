"""Color-managed view and display settings records, scene and display node views, Blender tint, and OCIO installation."""

from collections.abc import Callable, Iterable
from functools import cached_property
from pathlib import Path
from typing import Final

import bpy
from expression import Nothing, Ok
import msgspec
from msgspec import UNSET, UnsetType
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, Operator
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.paths import RnaPath
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.nodes.desired import member_paths, Members
from blender.imaging.color.authoring import ConfigBuild
from blender.imaging.color.colorimetry import WhitePoint
from blender.imaging.color.processors import called, DisplayView
from blender.records.faults import accumulated, Outcome
from blender.records.fields import Name

# --- [CONSTANTS] ------------------------------------------------------------------------

CONFIGURED: Final = Setting(descriptor=Descriptor.declared(bpy.types.BlendData, ("colorspace", "is_missing_opencolorio_config"), bpy.types.BoolProperty), accepted=(False,))
EXPOSURE: Final = Descriptor.declared(bpy.types.ColorManagedViewSettings, ("exposure",), bpy.types.FloatProperty)
NO_LOOK: Final[str] = "None"
TINT_PER_DUV: Final = 3000.0
UNTAGGED: Final = ("default_byte", "default_float")

# --- [MODELS] ---------------------------------------------------------------------------


class Settings(msgspec.Struct, frozen=True, kw_only=True):
    """Host settings record whose unset members keep the values the host holds, nested records standing for pointer members."""

    def members(self, owner: RnaPath = ()) -> Members:
        """Set members by path under `owner`, nested records flattened to their member paths."""

        def flattened(path: RnaPath, held: object) -> tuple[tuple[RnaPath, Value], ...]:
            match held:
                case dict():
                    return tuple(pair for key, item in held.items() for pair in flattened((*path, str(key)), item))
                case list():
                    return ((path, tuple(item for item in held if isinstance(item, bool | int | float | str))),)
                case bool() | int() | float() | str() | None:
                    return ((path, held),)
                case _:
                    return ()

        return dict[str | RnaPath, Value](flattened(owner, msgspec.to_builtins(self)))


class DisplaySettings(Settings, frozen=True, kw_only=True):
    """`ColorManagedDisplaySettings` members."""

    display_device: str | UnsetType = UNSET
    emulation: str | UnsetType = UNSET


class ViewSettings(Settings, frozen=True, kw_only=True):
    """`ColorManagedViewSettings` members, the white point left to the temperature and tint it derives from."""

    view_transform: str | UnsetType = UNSET
    look: str | UnsetType = UNSET
    exposure: float | UnsetType = UNSET
    gamma: float | UnsetType = UNSET
    use_curve_mapping: bool | UnsetType = UNSET
    use_white_balance: bool | UnsetType = UNSET
    white_balance_temperature: float | UnsetType = UNSET
    white_balance_tint: float | UnsetType = UNSET


class Viewing(Settings, frozen=True, kw_only=True):
    """Display and view settings of one color-managed owner: a scene, a display node, or an overriding image format."""

    display_settings: DisplaySettings = DisplaySettings()
    view_settings: ViewSettings = ViewSettings()

    @classmethod
    def of(cls, view: DisplayView) -> "Viewing":
        """Returns the settings a view forms through, the host's look `NO_LOOK` as disabled."""
        return cls(
            display_settings=DisplaySettings(display_device=view.display),
            view_settings=ViewSettings(view_transform=view.view, look=view.looks_override if view.looks_override_enabled else NO_LOOK, exposure=view.exposure, gamma=view.gamma),
        )


class ConfigView(msgspec.Struct, frozen=True, kw_only=True):
    """View transform an installation adds, its operation built over the base configuration in its process space."""

    name: str
    description: str
    transform: Callable[[PyOpenColorIO.Config], Outcome[PyOpenColorIO.Transform]]
    space: Name = PyOpenColorIO.ROLE_SCENE_LINEAR

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


class ConfigInstall(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Application OCIO rows and views installed over Blender's bundled configuration."""

    build: ConfigBuild
    views: tuple[ConfigView, ...] = ()

    @property
    def merged(self) -> Outcome[tuple[PyOpenColorIO.Config, ConfigBuild]]:
        """Bundled base configuration, independent of a user installation, and the application build with the views' view transforms added."""
        folder: str = bpy.utils.system_resource("DATAFILES", path="colormanagement")  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        path = str(Path(folder, "config.ocio"))
        return called(PyOpenColorIO.Config.CreateFromFile, path, path).bind(
            lambda base: accumulated(view.view_transform(base) for view in self.views).map(
                lambda rows: (base, msgspec.structs.replace(self.build, view_transforms=(*self.build.view_transforms, *rows)))
            )
        )

    def staged(self, stage: Path) -> Outcome[PyOpenColorIO.Config]:
        """Stages full build in caller's resource scope."""
        return self.merged.bind(lambda parts: parts[1].staged(parts[0], stage))

    def written(self) -> Outcome[Path]:
        """Installs complete configuration read on restart."""
        return self.merged.bind(lambda parts: parts[1].installed(parts[0], Path(bpy.utils.user_resource("DATAFILES", path="colormanagement"))))

    def install_config(self, _call: Call[Empty]) -> Outcome[Done]:
        """Install extension view transforms read on restart."""
        return self.written().map(lambda path: Done(notes=(("INFO", f"Installed {path}, restart Blender to read it"),)))

    @cached_property
    def installing(self) -> Operator[Empty]:
        """Command installing the build."""
        return Operator(form=Empty, run=self.install_config, undo=Nothing)

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns install command."""
        return Ok((self.installing,))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [COLORS]


def white_of(temperature: float, tint: float) -> WhitePoint:
    """Returns white of a Blender temperature and tint, the tint `TINT_PER_DUV` per unit Duv."""
    return WhitePoint(temperature=temperature, duv=tint / TINT_PER_DUV)


def tint_of(white: WhitePoint) -> float:
    """Returns the Blender tint of `white`."""
    return white.duv * TINT_PER_DUV


# --- [VIEWS]


def viewed(holder: bpy.types.Scene | bpy.types.CompositorNodeConvertToDisplay) -> DisplayView:
    """Reads the display, view, look, exposure, and gamma a scene or display node forms through, the host's look `NO_LOOK` as disabled."""
    view: bpy.types.ColorManagedViewSettings = holder.view_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    shown: bpy.types.ColorManagedDisplaySettings = holder.display_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return DisplayView(display=shown.display_device, view=view.view_transform, looks_override=view.look, looks_override_enabled=view.look != NO_LOOK, exposure=view.exposure, gamma=view.gamma)


def displayed(scene: bpy.types.Scene, view: DisplayView) -> Outcome[None]:
    """Writes a view to the scene in host order, outputs overriding color management keeping their own."""
    return assigned(scene, member_paths(Viewing.of(view).members()))


def followed(scene: bpy.types.Scene, nodes: Iterable[bpy.types.CompositorNodeConvertToDisplay]) -> Outcome[tuple[tuple[bpy.types.CompositorNodeConvertToDisplay, DisplayView], ...]]:
    """Writes the scene display device to each display node showing another, answering each written node with the view Blender matched to the device."""
    display = viewed(scene).display
    return accumulated(written(node, ("display_settings", "display_device"), display).map(lambda _: (node, viewed(node))) for node in nodes if viewed(node).display != display)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CONFIGURED",
    "EXPOSURE",
    "NO_LOOK",
    "TINT_PER_DUV",
    "UNTAGGED",
    "ConfigInstall",
    "ConfigView",
    "DisplaySettings",
    "Settings",
    "ViewSettings",
    "Viewing",
    "displayed",
    "followed",
    "tint_of",
    "viewed",
    "white_of",
]
