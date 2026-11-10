"""Color-managed view and display settings records, scene, display node, and preview views, HDR window showing, Blender tint, and OCIO installation."""

from collections.abc import Callable, Generator, Iterable, Mapping
from functools import cache, cached_property, partial, reduce
from itertools import takewhile
from pathlib import Path
import shutil
from typing import Annotated, Final, override

import bpy
from expression import Option, Some
from expression.extra.result import catch
import msgspec
import PyOpenColorIO

from rasm_bpy.commands.operators import Call, Done, Empty, Operator
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import Acquisitions
from rasm_bpy.data._rna.conditions import Condition, Setting
from rasm_bpy.data._rna.descriptors import Descriptor, written
from rasm_bpy.data._rna.paths import id_data, RnaPath, Struct
from rasm_bpy.data._rna.snapshots import assigned
from rasm_bpy.nodes._addressing import interface_sockets
from rasm_bpy.nodes._desired import member_paths, Members
from rasm_bpy.nodes._trees import active_output, feeds, socket_at
from rasm_imaging import called, ConfigBuild, DisplayView, hdr, pq, WhitePoint
from rasm_records import accumulated, committed, Fault, files_copied, folder_made, Name, Nothing, Ok, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type ColorSpace = Annotated[Name, Descriptor(root=bpy.types.ColorManagedInputColorspaceSettings, prop=bpy.types.ColorManagedInputColorspaceSettings.bl_rna.properties["name"])]
type _Origin = str | DisplayView

# --- [CONSTANTS] ------------------------------------------------------------------------

CONFIGURED: Final = Setting(
    descriptor=Descriptor(root=bpy.types.BlendData, owner=("colorspace",), prop=bpy.types.BlendFileColorspace.bl_rna.properties["is_missing_opencolorio_config"]), accepted=(False,)
)
EXPOSURE: Final = Descriptor.of(bpy.types.ColorManagedViewSettings, ("exposure",), bpy.types.FloatProperty).prop
NO_LOOK: Final[str] = "None"
_TINT_PER_DUV: Final = 3000.0

# --- [MODELS] ---------------------------------------------------------------------------


class Settings(msgspec.Struct, frozen=True, kw_only=True):
    """Host settings record with unset members keeping the host's values, nested records standing for pointer members."""

    def members(self, owner: RnaPath = ()) -> Members:
        """Set members by path under `owner`, nested records flattened to their member paths."""
        return {
            path: value
            for name, held in msgspec.structs.asdict(self).items()
            for path, value in (held.members((*owner, name)).items() if isinstance(held, Settings) else (((*owner, name), held),) if isinstance(held, bool | int | float | str) else ())
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
    """Application OCIO rows and views installed over Blender's bundled configuration, keeping the view transforms an earlier installation added and the folder content the first installation replaced, which the restore command puts back."""

    build: ConfigBuild = msgspec.field(default_factory=ConfigBuild)
    views: tuple[ConfigView, ...] = ()

    @property
    def folder(self) -> Path:
        """User configuration folder Blender reads on start in place of its bundled one."""
        return Path(bpy.utils.user_resource("DATAFILES", path="colormanagement"))

    @property
    def merged(self) -> Outcome[tuple[PyOpenColorIO.Config, ConfigBuild]]:
        """Bundled base configuration, independent of a user installation, and the application build with the views' view transforms and the earlier installation's added."""
        base, earlier = (str(Path(folder, "config.ocio")) for folder in (bpy.utils.system_resource("DATAFILES", path="colormanagement"), self.folder))  # ty: ignore[missing-attribute]

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

    def written(self, identity: Identity) -> Outcome[Path]:
        """Installs the complete configuration Blender reads on restart, keeping under the extension ID the folder content the first installation replaced."""
        kept = self.folder / identity.manifest.id

        def staged(stage: Path) -> Outcome[object]:
            prior = kept if kept.is_dir() else self.folder
            return (
                self.merged
                .bind(lambda parts: parts[1].staged(parts[0], stage))
                .bind(lambda config: committed(stage / "config.ocio", config.serialize().encode()))
                .bind(lambda _: files_copied(((prior, stage / identity.manifest.id),)) if prior.is_dir() else folder_made(stage / identity.manifest.id))
            )

        return committed(self.folder, staged).map(lambda folder: folder / "config.ocio")

    def restored(self, identity: Identity) -> Outcome[Path]:
        """Restores the folder content the first installation replaced, removing the folder where it held none, absent fault for a folder no installation of the extension wrote."""
        kept = self.folder / identity.manifest.id
        return (
            Ok(kept)
            .filter_with(Path.is_dir, lambda _: Fault.of(Reason.ABSENT, str(self.folder), member=f"{identity.manifest.name} installation"))
            .bind(
                lambda held: (
                    committed(self.folder, lambda stage: files_copied(((held, stage),)))
                    if any(held.iterdir())
                    else catch(shutil.rmtree, exception=OSError)(self.folder).map(lambda _: self.folder).map_error(partial(Fault.of, Reason.UNWRITABLE, str(self.folder)))
                )
            )
        )

    def install_config(self, call: Call[Empty]) -> Outcome[Done]:
        """Install extension view transforms read on restart."""
        return self.written(call.identity).map(lambda path: Done(notes=(("INFO", f"Installed {path}, restart Blender to read it"),)))

    def remove_config(self, call: Call[Empty]) -> Outcome[Done]:
        """Restore the color configuration the first installation replaced, read on restart."""
        return self.restored(call.identity).map(lambda path: Done(notes=(("INFO", f"Restored {path}, restart Blender to read it"),)))

    @cached_property
    def installing(self) -> Operator[Empty]:
        """Command installing the build."""
        return Operator(form=Empty, run=self.install_config, options=frozenset())

    @cached_property
    def removing(self) -> Operator[Empty]:
        """Command restoring the configuration the first installation replaced."""
        return Operator(form=Empty, run=self.remove_config, options=frozenset())

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns install and restore commands."""
        return (self.installing, self.removing)


class HdrWindow(Condition, frozen=True, kw_only=True):
    """Window showing HDR color while the context scene's preview forms an HDR frame, PQ alone under `absolute`."""

    absolute: bool = False

    def formed(self, context: bpy.types.Context) -> bool:
        """Whether the context scene's preview forms the frame into an HDR display space, absolute PQ under `absolute`."""
        config = PyOpenColorIO.GetCurrentConfig()
        return Option.of_optional(context.scene).bind(_shown).bind(lambda view: view.colorspace(config)).filter(pq if self.absolute else hdr).is_some()

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
    return WhitePoint(temperature=temperature, duv=tint / _TINT_PER_DUV)


def tint_of(white: WhitePoint) -> float:
    """Returns the Blender tint of `white`."""
    return white.duv * _TINT_PER_DUV


# --- [VIEWS]


def viewed(holder: bpy.types.Scene | bpy.types.CompositorNodeConvertToDisplay | bpy.types.ImageFormatSettings) -> DisplayView:
    """Reads an owner's OCIO display, view, look, exposure, and gamma, the host's look `NO_LOOK` as disabled."""
    match holder:
        case bpy.types.bpy_struct(
            view_settings=bpy.types.ColorManagedViewSettings(view_transform=view, look=look, exposure=exposure, gamma=gamma),
            display_settings=bpy.types.ColorManagedDisplaySettings(display_device=display),
        ):
            return DisplayView(display=display, view=view, looks_override=look, looks_override_enabled=look != NO_LOOK, exposure=exposure, gamma=gamma)
        case _:
            raise TypeError(holder)


def displayed(scene: bpy.types.Scene, view: DisplayView) -> Outcome[None]:
    """Writes a view to the scene in host order, outputs overriding color management keeping their own."""
    return assigned(scene, member_paths(ColorManagement.of(view).members()))


def followed(scene: bpy.types.Scene, nodes: Iterable[bpy.types.CompositorNodeConvertToDisplay]) -> Outcome[tuple[tuple[bpy.types.CompositorNodeConvertToDisplay, DisplayView], ...]]:
    """Writes the scene display device to each display node showing another, returning each written node with the view Blender matched to the device."""
    display = viewed(scene).display
    return accumulated(written(node, ("display_settings", "display_device"), display).map(lambda _: (node, viewed(node))) for node in nodes if viewed(node).display != display)


def _shown(scene: bpy.types.Scene) -> Option[DisplayView]:
    """Returns the effective preview's display formation or scene view, Nothing for data or an unknown encoding."""
    config = PyOpenColorIO.GetCurrentConfig()
    source: Option[_Origin] = _linear(config)
    return (
        flow(config, (entry for entry in scene.compositor_effects if entry.enable_for_preview), source)
        .default_value(source)
        .bind(lambda origin: Some(origin) if isinstance(origin, DisplayView) else Some(viewed(scene)).filter(lambda _: not config.getColorSpace(origin).isData()))
    )


def entering_space(config: PyOpenColorIO.Config, entry: bpy.types.SceneCompositorEffect, identifier: str) -> Option[str]:
    """Returns a native entry input's encoding, Nothing for an unknown source."""
    source: Option[_Origin] = _linear(config)
    first = Option.of_optional(entry.node_group).map(lambda tree: next((socket.identifier for socket in interface_sockets(tree)), None)).default_value(None)
    return (
        id_data(bpy.types.Scene, entry)
        .bind(lambda scene: flow(config, (prior for prior in takewhile(lambda prior: prior != entry, scene.compositor_effects) if prior.enable_for_preview), source))
        .default_value(Nothing)
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
    linear: Option[_Origin] = _linear(config)
    parents: list[tuple[bpy.types.CompositorNodeGroup, bpy.types.NodeTree, int | None]] = []
    instances: dict[tuple[bpy.types.CompositorNodeGroup, int | None], int] = {}

    @cache
    def linked(owner: bpy.types.NodeTree) -> Mapping[bpy.types.NodeSocket, tuple[tuple[bpy.types.Node, bpy.types.NodeSocket], ...]]:
        return feeds((owner,))

    def agreed(children: Iterable[tuple[bpy.types.NodeSocket, int | None]]) -> Generator[tuple[bpy.types.NodeSocket, int | None], Option[_Origin], Option[_Origin]]:
        held: Option[Option[_Origin]] = Nothing
        for child in children:
            origin = yield child
            if held.filter(lambda prior: prior != origin).is_some():
                return Nothing
            held = Some(origin)
        return held.default_value(linear)  # ruff: ignore[return-in-generator]

    def walked(state: tuple[bpy.types.NodeSocket, int | None]) -> Generator[tuple[bpy.types.NodeSocket, int | None], Option[_Origin], Option[_Origin]]:
        inlet, context = state
        fed = linked(tree if context is None else parents[context][1])
        if not inlet.is_output:
            return (yield from agreed((outlet, context) for _, outlet in fed.get(inlet, ())))
        match inlet.node:
            case bpy.types.Node(mute=True) as node:
                source = next((link.from_socket for link in node.internal_links if link.to_socket == inlet), None)
                return linear if source is None else (yield (source, context))
            case bpy.types.NodeGroupInput():
                if context is None:
                    return entering(inlet.identifier)
                parent, _, enclosing = parents[context]
                source = socket_at(parent.inputs, inlet.identifier).to_option().to_optional()
                return Nothing if source is None else (yield (source, enclosing))
            case bpy.types.CompositorNodeConvertToDisplay(inputs=bpy.types.bpy_prop_collection() as inputs) as display:
                return Nothing if (invert := inputs["Invert"]) in fed else linear if invert.default_value else Some(viewed(display))
            case bpy.types.CompositorNodeConvertColorSpace(to_color_space=name):
                return Option.of_optional(config.getColorSpace(name)).map(lambda found: found.getName())
            case bpy.types.CompositorNodeImage(image=bpy.types.Image(colorspace_settings=bpy.types.ColorManagedInputColorspaceSettings(is_data=True, name=name))):
                return Some(name)
            case bpy.types.CompositorNodeGroup(node_tree=bpy.types.NodeTree(is_missing=False) as inner) as node:
                if (key := (node, context)) not in instances:
                    instances[key] = len(parents)
                    parents.append((node, inner, context))
                source = active_output(inner).bind(lambda output: socket_at(output.inputs, inlet.identifier)).to_option().to_optional()
                return linear if source is None else (yield (source, instances[key]))
            case _:
                return (yield from agreed((held, context) for node in Option.of_optional(inlet.node).to_list() for held in node.inputs if isinstance(held, bpy.types.NodeSocketColor)))  # ruff: ignore[return-in-generator]

    root: tuple[bpy.types.NodeSocket, int | None] = socket, None
    pending: list[tuple[tuple[bpy.types.NodeSocket, int | None], Generator[tuple[bpy.types.NodeSocket, int | None], Option[_Origin], Option[_Origin]]]] = [(root, walked(root))]
    active, completed = {root}, dict[tuple[bpy.types.NodeSocket, int | None], Option[_Origin]]()
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


def _exited(config: PyOpenColorIO.Config, entry: bpy.types.SceneCompositorEffect, entering: Option[_Origin]) -> Option[Option[_Origin]]:
    """Reads a group's first color output, Nothing when native execution stops at that entry."""
    match entry.node_group:
        case None | bpy.types.NodeTree(is_missing=True):
            return Some(entering)
        case tree:
            if not isinstance(socket := next(interface_sockets(tree, "OUTPUT"), None), bpy.types.NodeTreeInterfaceSocketColor):
                return Nothing
            first = next((item.identifier for item in interface_sockets(tree)), None)
            linear: Option[_Origin] = _linear(config)
            return Some(
                active_output(tree)
                .to_option()
                .map(lambda output: _traced(config, tree, output.inputs[socket.identifier], lambda identifier: entering if identifier == first else linear))
                .default_value(linear)
            )


def flow(config: PyOpenColorIO.Config, entries: Iterable[bpy.types.SceneCompositorEffect], entering: Option[_Origin]) -> Option[Option[_Origin]]:
    """Propagates encoding until native output eligibility stops the effect stack."""
    return reduce(lambda held, entry: held.bind(lambda origin: _exited(config, entry, origin)), entries, Some(entering))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CONFIGURED",
    "EXPOSURE",
    "NO_LOOK",
    "ColorManagement",
    "ColorSpace",
    "ConfigInstall",
    "ConfigView",
    "DisplaySettings",
    "HdrWindow",
    "Settings",
    "ViewSettings",
    "displayed",
    "entering_space",
    "flow",
    "followed",
    "tint_of",
    "traced",
    "viewed",
    "white_of",
]
