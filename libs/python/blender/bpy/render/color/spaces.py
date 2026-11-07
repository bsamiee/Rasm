"""Color-managed view and display settings records, scene, display node, and preview views, HDR window showing, Blender tint, and OCIO installation."""

from collections.abc import Callable, Generator, Iterable, Mapping
from functools import cache, cached_property, reduce
from itertools import takewhile
from pathlib import Path
from typing import Annotated, Final, override

import bpy
from expression import Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, Operator
from blender.bpy.data.rna.conditions import Condition, Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.paths import RnaPath, Struct
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.nodes.addressing import interface_sockets
from blender.bpy.nodes.desired import member_paths, Members
from blender.bpy.nodes.trees import active_output, feeds, socket_at
from blender.imaging.color.authoring import ConfigBuild
from blender.imaging.color.colorimetry import WhitePoint
from blender.imaging.color.processors import called, DisplayView, hdr, pq
from blender.records.faults import accumulated, Nothing, Ok, Outcome, paired
from blender.records.fields import Name

# --- [TYPES] ----------------------------------------------------------------------------

type ColorSpace = Annotated[Name, Descriptor(root=bpy.types.ColorManagedInputColorspaceSettings, prop=bpy.types.ColorManagedInputColorspaceSettings.bl_rna.properties["name"])]
type _Origin = str | DisplayView

# --- [CONSTANTS] ------------------------------------------------------------------------

CONFIGURED: Final = Setting(
    descriptor=Descriptor(root=bpy.types.BlendData, owner=("colorspace",), prop=bpy.types.BlendFileColorspace.bl_rna.properties["is_missing_opencolorio_config"]), accepted=(False,)
)
EXPOSURE: Final[bpy.types.FloatProperty] = bpy.types.ColorManagedViewSettings.bl_rna.properties["exposure"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
NO_LOOK: Final[str] = "None"
TINT_PER_DUV: Final = 3000.0

# --- [MODELS] ---------------------------------------------------------------------------


class Settings(msgspec.Struct, frozen=True, kw_only=True):
    """Host settings record with unset members keeping the host's values, nested records standing for pointer members."""

    def members(self, owner: RnaPath = ()) -> Members:
        """Set members by path under `owner`, nested records flattened to their member paths."""
        return {  # ty: ignore[unsound-return-statement]
            path: value
            for name, held in msgspec.structs.asdict(self).items()
            if held is not msgspec.UNSET
            for path, value in (held.members((*owner, name)).items() if isinstance(held, Settings) else (((*owner, name), held),))
        }


class DisplaySettings(Settings, frozen=True, kw_only=True):
    """`ColorManagedDisplaySettings` members."""

    display_device: str | msgspec.UnsetType = msgspec.UNSET
    emulation: str | msgspec.UnsetType = msgspec.UNSET


class ViewSettings(Settings, frozen=True, kw_only=True):
    """`ColorManagedViewSettings` members, the white point left to the temperature and tint it derives from."""

    view_transform: str | msgspec.UnsetType = msgspec.UNSET
    look: str | msgspec.UnsetType = msgspec.UNSET
    exposure: float | msgspec.UnsetType = msgspec.UNSET
    gamma: float | msgspec.UnsetType = msgspec.UNSET
    use_curve_mapping: bool | msgspec.UnsetType = msgspec.UNSET
    use_white_balance: bool | msgspec.UnsetType = msgspec.UNSET
    white_balance_temperature: float | msgspec.UnsetType = msgspec.UNSET
    white_balance_tint: float | msgspec.UnsetType = msgspec.UNSET


class ColorManagement(Settings, frozen=True, kw_only=True):
    """Display and view settings of one color-managed owner: a scene, a display node, or an overriding image format."""

    display_settings: DisplaySettings = DisplaySettings()
    view_settings: ViewSettings = ViewSettings()

    @classmethod
    def of(cls, view: DisplayView) -> "ColorManagement":
        """Returns the settings a view forms through, the host's look `NO_LOOK` as disabled."""
        return cls(
            display_settings=DisplaySettings(display_device=view.display),
            view_settings=ViewSettings(view_transform=view.view, look=view.looks_override if view.looks_override_enabled else NO_LOOK, exposure=view.exposure, gamma=view.gamma),
        )


class ConfigView(msgspec.Struct, frozen=True, kw_only=True):
    """View transform an installation adds from its operation's process space."""

    name: str
    description: str
    transform: Callable[[], Outcome[PyOpenColorIO.Transform]]
    space: Name = PyOpenColorIO.ROLE_SCENE_LINEAR

    def view_transform(self) -> Outcome[PyOpenColorIO.ViewTransform]:
        """Scene-referred operation entered from scene interchange and left to display interchange through its process space."""
        return self.transform().map(
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
        bundled: str = bpy.utils.system_resource("DATAFILES", path="colormanagement")  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute, unsound-assignment]
        base, earlier = (str(Path(folder, "config.ocio")) for folder in (bundled, self.folder))

        def kept(config: PyOpenColorIO.Config, rows: tuple[PyOpenColorIO.ViewTransform, ...], prior: Option[PyOpenColorIO.Config]) -> Outcome[tuple[PyOpenColorIO.Config, ConfigBuild]]:
            build = msgspec.structs.replace(self.build, view_transforms=(*self.build.view_transforms, *rows))
            return prior.map(lambda installed: build.keeping(config, installed)).default_value(Ok(build)).map(lambda held: (config, held))

        return called(PyOpenColorIO.Config.CreateFromFile, base, base).bind(
            lambda config: paired(
                accumulated(view.view_transform() for view in self.views),
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
        return Operator(form=Empty, run=self.install_config, options=frozenset())

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns install command."""
        return (self.installing,)


class HdrWindow(Condition, frozen=True, kw_only=True):
    """Window showing HDR color while the context scene's preview forms an HDR frame, PQ alone under `absolute`."""

    absolute: bool = False

    def formed(self, context: bpy.types.Context) -> bool:
        """Whether the context scene's preview forms the frame into an HDR display space, absolute PQ under `absolute`."""
        config = PyOpenColorIO.GetCurrentConfig()
        return Option.of_optional(context.scene).bind(shown).bind(lambda view: view.colorspace(config)).filter(pq if self.absolute else hdr).is_some()

    @override
    def holds(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> bool:
        return self.formed(context) and any(window.support_hdr_color for window in Option.of_optional(context.window).to_list())

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


def viewed(holder: bpy.types.Scene | bpy.types.CompositorNodeConvertToDisplay | bpy.types.ImageFormatSettings) -> DisplayView:
    """Reads an owner's OCIO display, view, look, exposure, and gamma, the host's look `NO_LOOK` as disabled."""
    view: bpy.types.ColorManagedViewSettings = holder.view_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    shown: bpy.types.ColorManagedDisplaySettings = holder.display_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return DisplayView(display=shown.display_device, view=view.view_transform, looks_override=view.look, looks_override_enabled=view.look != NO_LOOK, exposure=view.exposure, gamma=view.gamma)


def displayed(scene: bpy.types.Scene, view: DisplayView) -> Outcome[None]:
    """Writes a view to the scene in host order, outputs overriding color management keeping their own."""
    return assigned(scene, member_paths(ColorManagement.of(view).members()))


def followed(scene: bpy.types.Scene, nodes: Iterable[bpy.types.CompositorNodeConvertToDisplay]) -> Outcome[tuple[tuple[bpy.types.CompositorNodeConvertToDisplay, DisplayView], ...]]:
    """Writes the scene display device to each display node showing another, returning each written node with the view Blender matched to the device."""
    display = viewed(scene).display
    return accumulated(written(node, ("display_settings", "display_device"), display).map(lambda _: (node, viewed(node))) for node in nodes if viewed(node).display != display)


def shown(scene: bpy.types.Scene) -> Option[DisplayView]:
    """Returns the effective preview's display formation or scene view, Nothing for data or an unknown encoding."""
    config = PyOpenColorIO.GetCurrentConfig()
    source: Option[_Origin] = _linear(config)
    return (
        _flow(config, (entry for entry in scene.compositor_effects if entry.enable_for_preview), source)
        .default_value(source)
        .bind(lambda origin: Some(origin) if isinstance(origin, DisplayView) else Some(viewed(scene)).filter(lambda _: not config.getColorSpace(origin).isData()))
    )


def entering(config: PyOpenColorIO.Config, entry: bpy.types.SceneCompositorEffect, identifier: str) -> Option[str]:
    """Returns a native entry input's encoding, Nothing for an unknown source."""
    scene: bpy.types.Scene = entry.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    source: Option[_Origin] = _linear(config)
    first = Option.of_optional(entry.node_group).map(lambda tree: next((socket.identifier for socket in interface_sockets(tree)), None)).default_value(None)
    return (
        _flow(config, (prior for prior in takewhile(lambda prior: prior != entry, scene.compositor_effects) if prior.enable_for_preview), source).default_value(Nothing)
        if identifier == first
        else source
    ).bind(lambda origin: _space(config, origin))


# --- [TRACES]


def _space(config: PyOpenColorIO.Config, origin: _Origin) -> Option[str]:
    """Resolves a display formation to its native output colorspace."""
    return origin.colorspace(config).map(lambda found: found.getName()) if isinstance(origin, DisplayView) else Some(origin)


def _linear(config: PyOpenColorIO.Config) -> Option[str]:
    """Resolves the native scene-linear role, absent for a configuration without it."""
    return Option.of_optional(config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR)).map(lambda space: space.getName())


def _traced(config: PyOpenColorIO.Config, tree: bpy.types.NodeTree, socket: bpy.types.NodeSocket, entering: Callable[[str], Option[_Origin]]) -> Option[_Origin]:
    """Resolves agreeing color origins over reachable links and group instances, unknown for cycles or mixed encodings."""
    type State = tuple[bpy.types.NodeSocket, int | None]
    type Walk = Generator[State, Option[_Origin], Option[_Origin]]

    linear: Option[_Origin] = _linear(config)
    parents: list[tuple[bpy.types.CompositorNodeGroup, bpy.types.NodeTree, int | None]] = []
    instances: dict[tuple[bpy.types.CompositorNodeGroup, int | None], int] = {}

    @cache
    def linked(owner: bpy.types.NodeTree) -> Mapping[bpy.types.NodeSocket, tuple[tuple[bpy.types.Node, bpy.types.NodeSocket], ...]]:
        return feeds((owner,))

    def agreed(children: Iterable[State]) -> Walk:
        held: Option[Option[_Origin]] = Nothing
        for child in children:
            origin = yield child
            if held.filter(lambda prior: prior != origin).is_some():
                return Nothing
            held = Some(origin)
        return held.default_value(linear)  # ruff: ignore[return-in-generator]

    def walked(state: State) -> Walk:
        inlet, context = state
        fed = linked(tree if context is None else parents[context][1])
        if not inlet.is_output:
            return (yield from agreed((outlet, context) for _, outlet in fed.get(inlet, ())))
        node: bpy.types.Node = inlet.node  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        match node:
            case _ if node.mute:
                source = next((link.from_socket for link in node.internal_links if link.to_socket == inlet), None)
                return linear if source is None else (yield (source, context))
            case bpy.types.NodeGroupInput():
                if context is None:
                    return entering(inlet.identifier)
                parent, _, enclosing = parents[context]
                source = socket_at(parent.inputs, inlet.identifier).to_option().to_optional()
                return Nothing if source is None else (yield (source, enclosing))
            case bpy.types.CompositorNodeConvertToDisplay() as display:
                inputs: bpy.types.NodeInputs = display.inputs  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                invert: bpy.types.NodeSocketBool = inputs["Invert"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                return Nothing if invert in fed else linear if invert.default_value else Some(viewed(display))
            case bpy.types.CompositorNodeConvertColorSpace(to_color_space=name):
                return Option.of_optional(config.getColorSpace(name)).map(lambda found: found.getName())
            case bpy.types.CompositorNodeImage(image=bpy.types.Image(colorspace_settings=bpy.types.ColorManagedInputColorspaceSettings(is_data=True, name=name))):
                return Some(name)
            case bpy.types.CompositorNodeGroup(node_tree=bpy.types.NodeTree(is_missing=False) as inner):
                if (key := (node, context)) not in instances:
                    instances[key] = len(parents)
                    parents.append((node, inner, context))
                source = active_output(inner).bind(lambda output: socket_at(output.inputs, inlet.identifier)).to_option().to_optional()
                return linear if source is None else (yield (source, instances[key]))
            case _:
                return (yield from agreed((held, context) for held in node.inputs if isinstance(held, bpy.types.NodeSocketColor)))  # ruff: ignore[return-in-generator]

    root: State = socket, None
    pending: list[tuple[State, Walk]] = [(root, walked(root))]
    active, completed = {root}, dict[State, Option[_Origin]]()
    answer: Option[Option[_Origin]] = Nothing
    while pending:
        try:
            child = answer.map(pending[-1][1].send).default_with(lambda: next(pending[-1][1]))
        except StopIteration as finished:
            state, _ = pending.pop()
            completed[state] = finished.value
            active.remove(state)
            answer = Some(completed[state])
        else:
            if child in completed:
                answer = Some(completed[child])
            elif child in active:
                answer = Some(Nothing)
            else:
                active.add(child)
                pending.append((child, walked(child)))
                answer = Nothing
    return completed[root]


def traced(config: PyOpenColorIO.Config, tree: bpy.types.NodeTree, socket: bpy.types.NodeSocket, entering: Callable[[str], Option[str]]) -> Option[str]:
    """Returns the colorspace reaching an input socket through effective links and native mute connections.

    Args:
        config: OCIO config naming each space.
        tree: Tree holding the socket.
        socket: Input socket the value reaches.
        entering: Space of the value at each group input socket identifier of `tree`.

    Returns:
        Space a display or colorspace conversion writes, a data image holds, or a group input receives, scene linear for socket defaults,
        Nothing for an unknown output colorspace.
    """
    return _traced(config, tree, socket, entering).bind(lambda origin: _space(config, origin))


def _leaving(config: PyOpenColorIO.Config, entry: bpy.types.SceneCompositorEffect, entering: Option[_Origin]) -> Option[Option[_Origin]]:
    """Reads a group's first color output, Nothing when native execution stops at that entry."""
    match entry.node_group:
        case None | bpy.types.NodeTree(is_missing=True):
            return Some(entering)
        case tree:
            if not isinstance(socket := next(interface_sockets(tree, "OUTPUT"), None), bpy.types.NodeTreeInterfaceSocketColor):
                return Nothing
            first = next((socket.identifier for socket in interface_sockets(tree)), None)
            linear: Option[_Origin] = _linear(config)
            return Some(
                active_output(tree)
                .to_option()
                .map(lambda output: _traced(config, tree, output.inputs[socket.identifier], lambda identifier: entering if identifier == first else linear))
                .default_value(linear)
            )


def _flow(config: PyOpenColorIO.Config, entries: Iterable[bpy.types.SceneCompositorEffect], entering: Option[_Origin]) -> Option[Option[_Origin]]:
    """Propagates encoding until native output eligibility stops the effect stack."""
    return reduce(lambda held, entry: held.bind(lambda origin: _leaving(config, entry, origin)), entries, Some(entering))


def leaving(config: PyOpenColorIO.Config, entries: Iterable[bpy.types.SceneCompositorEffect], entering: Option[str]) -> Option[str | DisplayView]:
    """Returns effective stack output encoding or display formation, scene-linear when native execution leaves the result uncomposited."""
    return _flow(config, entries, entering).default_value(_linear(config))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CONFIGURED",
    "EXPOSURE",
    "NO_LOOK",
    "TINT_PER_DUV",
    "ColorManagement",
    "ColorSpace",
    "ConfigInstall",
    "ConfigView",
    "DisplaySettings",
    "HdrWindow",
    "Settings",
    "ViewSettings",
    "displayed",
    "entering",
    "followed",
    "leaving",
    "shown",
    "tint_of",
    "traced",
    "viewed",
    "white_of",
]
