"""Color-managed view and display settings records, scene, display node, and preview views, HDR window showing, Blender tint, and OCIO installation."""

from collections.abc import Callable, Iterable
from functools import cached_property
from pathlib import Path
from typing import Final, override

import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
import gpu
import msgspec
from msgspec import UNSET, UnsetType
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, Operator
from blender.bpy.data.rna.conditions import Condition, Setting
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.paths import RnaPath, Struct
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.nodes.desired import member_paths, Members
from blender.bpy.nodes.trees import active_output, endpoints, links_of, nested, socket_at
from blender.imaging.color.authoring import ConfigBuild
from blender.imaging.color.colorimetry import WhitePoint
from blender.imaging.color.processors import called, DisplayView, hdr, pq
from blender.records.faults import accumulated, Outcome, paired
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
    """Application OCIO rows and views installed over Blender's bundled configuration, keeping the view transforms an earlier installation added."""

    build: ConfigBuild = msgspec.field(default_factory=ConfigBuild)
    views: tuple[ConfigView, ...] = ()

    @property
    def folder(self) -> Path:
        """User configuration folder Blender reads on start in place of its bundled one."""
        return Path(bpy.utils.user_resource("DATAFILES", path="colormanagement"))

    @property
    def merged(self) -> Outcome[tuple[PyOpenColorIO.Config, ConfigBuild]]:
        """Bundled base configuration, independent of a user installation, and the application build with the views' view transforms and the earlier installation's added."""
        bundled: str = bpy.utils.system_resource("DATAFILES", path="colormanagement")  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        base, earlier = (str(Path(folder, "config.ocio")) for folder in (bundled, self.folder))

        def kept(config: PyOpenColorIO.Config, rows: tuple[PyOpenColorIO.ViewTransform, ...], prior: Option[PyOpenColorIO.Config]) -> Outcome[tuple[PyOpenColorIO.Config, ConfigBuild]]:
            build = msgspec.structs.replace(self.build, view_transforms=(*self.build.view_transforms, *rows))
            return prior.map(lambda installed: build.keeping(config, installed)).default_value(Ok(build)).map(lambda held: (config, held))

        return called(PyOpenColorIO.Config.CreateFromFile, base, base).bind(
            lambda config: paired(
                accumulated(view.view_transform(config) for view in self.views),
                Some(earlier).filter(lambda path: Path(path).is_file()).map(lambda path: called(PyOpenColorIO.Config.CreateFromFile, path, path).map(Some)).default_value(Ok(Nothing)),
            ).bind(lambda parts: kept(config, *parts))
        )

    def staged(self, stage: Path) -> Outcome[PyOpenColorIO.Config]:
        """Stages full build in caller's resource scope."""
        return self.merged.bind(lambda parts: parts[1].staged(parts[0], stage))

    def written(self) -> Outcome[Path]:
        """Installs complete configuration read on restart."""
        return self.merged.bind(lambda parts: parts[1].installed(parts[0], self.folder))

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


class HdrWindow(Condition, frozen=True, kw_only=True):
    """Window showing in HDR color a frame the context scene's preview forms into an HDR display space, an absolute PQ space alone under `absolute`."""

    absolute: bool = False

    def formed(self, context: bpy.types.Context) -> bool:
        """Whether the context scene's preview forms the frame into an HDR display space, absolute PQ under `absolute`."""
        config = PyOpenColorIO.GetCurrentConfig()
        return Option.of_optional(context.scene).bind(lambda scene: shown(scene).colorspace(config)).filter(pq if self.absolute else hdr).is_some()

    def refusal(self, context: bpy.types.Context) -> Option[str]:
        """Returns why the window shows the HDR frame the preview forms in standard range, Nothing for a window showing HDR color or a frame formed for none."""
        return (
            Some(context)
            .filter(lambda held: self.formed(held) and not any(window.support_hdr_color for window in Option.of_optional(held.window).to_list()))
            .map(lambda _: "OpenGL backend cannot show HDR" if gpu.platform.backend_type_get() == "OPENGL" else "Window cannot show HDR")
        )

    @override
    def holds(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> bool:
        return self.formed(context) and self.refusal(context).is_none()

    @property
    @override
    def message(self) -> str:
        return f"frame formed for {'PQ' if self.absolute else 'HDR'} display in window with HDR color"


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


def shown(scene: bpy.types.Scene) -> DisplayView:
    """Returns the view the viewport compositor forms the frame through, the first unmuted Convert to Display node a preview-enabled entry reaches, else the scene's."""
    return (
        Block
        .of_seq(
            node
            for entry in scene.compositor_effects
            if entry.enable_for_preview
            for tree in nested(Option.of_optional(entry.node_group).to_list(), muted=False)
            for node in tree.nodes
            if isinstance(node, bpy.types.CompositorNodeConvertToDisplay) and not node.mute
        )
        .try_head()
        .map(viewed)
        .default_with(lambda: viewed(scene))
    )


# --- [TRACES]


def traced(config: PyOpenColorIO.Config, tree: bpy.types.NodeTree, socket: bpy.types.NodeSocket, entering: Callable[[str], Option[str]]) -> Option[str]:
    """Returns the colorspace of the value reaching an input socket, read upstream to the node that sets it.

    Args:
        config: OCIO config naming each space.
        tree: Tree holding the socket.
        socket: Input socket the value reaches.
        entering: Space of the value at each group input socket identifier of `tree`.

    Returns:
        The space a Convert to Display or Convert Colorspace node writes, a data image holds, or a group input carries, through group nodes and
        every other node's first linked color input, scene linear for a node reading no color, Nothing for an unlinked socket or unknown space.
    """
    linear = Some(config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName())

    def through(node: bpy.types.Node) -> Option[str]:
        match Block.of_seq(node.inputs).filter(lambda inlet: isinstance(inlet, bpy.types.NodeSocketColor) and inlet.is_linked).try_head().to_optional():
            case bpy.types.NodeSocket() as inlet:
                return traced(config, tree, inlet, entering)
            case _:
                return linear

    def source(link: bpy.types.NodeLink) -> Option[str]:
        node, outlet, _, _ = endpoints(link)
        match node:
            case _ if node.mute:
                return through(node)
            case bpy.types.NodeGroupInput():
                return entering(outlet.identifier)
            case bpy.types.CompositorNodeConvertToDisplay() as display:
                return viewed(display).colorspace(config).map(lambda found: found.getName())
            case bpy.types.CompositorNodeConvertColorSpace(to_color_space=name):
                return Option.of_optional(config.getColorSpace(name)).map(lambda found: found.getName())
            case bpy.types.CompositorNodeImage(image=bpy.types.Image(colorspace_settings=bpy.types.ColorManagedInputColorspaceSettings(is_data=True, name=name))):
                return Some(name)
            case bpy.types.CompositorNodeGroup(node_tree=bpy.types.NodeTree() as inner):
                return (
                    active_output(inner)
                    .bind(lambda output: socket_at(output.inputs, outlet.identifier))
                    .to_option()
                    .bind(lambda inlet: traced(config, inner, inlet, lambda identifier: socket_at(node.inputs, identifier).to_option().bind(lambda held: traced(config, tree, held, entering))))
                )
            case _:
                return through(node)

    return Block.of_seq(links_of(socket)).filter(lambda link: link.is_valid and not link.is_muted).try_head().bind(source)


def leaving(config: PyOpenColorIO.Config, entry: bpy.types.SceneCompositorEffect, entering: Option[str]) -> Option[str]:
    """Returns the colorspace an entry's first output leaves in from the space entering it, an entry without a group passing it on."""
    match entry.node_group:
        case bpy.types.NodeTree() as tree:
            return active_output(tree).to_option().bind(lambda output: Block.of_seq(output.inputs).try_head()).bind(lambda inlet: traced(config, tree, inlet, lambda _: entering))
        case _:
            return entering


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
    "HdrWindow",
    "Settings",
    "ViewSettings",
    "Viewing",
    "displayed",
    "followed",
    "leaving",
    "shown",
    "tint_of",
    "traced",
    "viewed",
    "white_of",
]
