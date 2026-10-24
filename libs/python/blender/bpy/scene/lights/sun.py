"""Keyed site sun light, following world nodes, and sun paths."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import math
from typing import Annotated, ClassVar
import zoneinfo

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.operators import Done, Empty, InvokePopup, Operator, Outcome, Unchanged
from blender.bpy.data.ids.ownership import committed, Mark, Ownership, released, Saved
from blender.bpy.data.ids.store import Handle, vacant
from blender.bpy.data.rna.animation import Animation, Keys
from blender.bpy.data.rna.descriptors import Descriptor, Native, root_in, typed, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band, entered
from blender.bpy.faults import BlenderEditFault, BlenderRenderingFault, BlenderResult, BlenderZoneFault
from blender.bpy.scene.objects import ObjectState, reconciled, shaped, spawned
from blender.bpy.scene.worlds import EnvironmentNode, Follower, followers, WorldEnvironment
from blender.bpy.settings.attachments import Attached, Attachment, reconciling
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.layout.rows import Line, Member, OperatorButton, RowDraw
from blender.bpy.ui.layout.sections import Hosted, Section, Single, Store
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case, Gated, NonNegative, Presentation, Quantity
from blender.rendering.history import Retention
from blender.rendering.lighting.solar import Daylight, DayStudy, LocalTime, moments, Polar, SeasonStudy, Site, SolarDay, SunPaths, SunPosition

# --- [TYPES] ----------------------------------------------------------------------------

type Timing = Moment | DayStudy | SeasonStudy


# --- [CHOICES] --------------------------------------------------------------------------

zones = Choices(lambda _identity, _owner, _context: ((zone, zone, "") for zone in sorted(zoneinfo.available_timezones())))


# --- [MODELS] ---------------------------------------------------------------------------


class Moment(Case, frozen=True, kw_only=True, tag=3):
    """Single sun moment."""

    start: Annotated[LocalTime, msgspec.Meta(title="Moment", description="Local date and time")] = LocalTime()


class Paths(msgspec.Struct, frozen=True, kw_only=True):
    """Sun path objects and analemma tube radius."""

    surface: Annotated[bool, msgspec.Meta(description="Yearly sky band of sun")] = True
    analemmas: Annotated[bool, msgspec.Meta(description="Hourly sun positions over year")] = True
    bevel: Annotated[NonNegative, msgspec.Meta(description="Analemma tube radius"), Presentation(soft=Some((0.00635, 0.3048)), quantity=Some(Quantity.LENGTH))] = 0.0254


class SunSetup(msgspec.Struct, frozen=True, kw_only=True):
    """Scene sun setup."""

    site: Annotated[Site, msgspec.Meta(description="Observer location on Earth")] = Site()
    timing: Annotated[Timing, msgspec.Meta(description="Moment, or study keying one moment per frame")] = Moment()
    refraction: Annotated[bool, msgspec.Meta(description="Apparent elevation")] = True
    distance: Annotated[NonNegative, msgspec.Meta(description="Sun and path distance from origin"), Presentation(soft=Some((0.0, 304.8)), quantity=Some(Quantity.LENGTH))] = 15.24
    paths: Annotated[Gated[Paths], msgspec.Meta(description="Path objects visible to camera alone")] = Gated(enabled=False, value=Paths())


class Zoned(msgspec.Struct, frozen=True, kw_only=True):
    """Site time zone."""

    zone: Annotated[str, msgspec.Meta(title="Time Zone", description="IANA time zone of site clock"), zones] = Site().zone


class SolarCaption(Line, frozen=True, kw_only=True):
    """Row of sunrise, solar noon, sunset, day length, and sun position at start."""

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Returns caption draw, `register` fault for a refused attachment name."""

        def lines(setup: SunSetup) -> BlenderResult[tuple[str, ...]]:
            start, site = setup.timing.start, setup.site
            try:
                zone = zoneinfo.ZoneInfo(site.zone)
            except (ValueError, zoneinfo.ZoneInfoNotFoundError) as error:
                return Error((BlenderZoneFault(request=site.zone, error=error),))

            def text(pair: tuple[SolarDay, SunPosition]) -> tuple[str, ...]:
                day, position = pair
                sun = f"Azimuth {math.degrees(float(position.azimuth[0])):.2f}°, elevation {math.degrees(float(position.elevation[0])):.2f}°"
                match day.light:
                    case Daylight(sunrise=sunrise, sunset=sunset):
                        return (f"Sunrise {sunrise:%H:%M}", f"Solar noon {day.noon:%H:%M}", f"Sunset {sunset:%H:%M}", f"Day length {sunset - sunrise}", sun)
                    case Polar.DAY:
                        return (f"Solar noon {day.noon:%H:%M}", "Polar day", sun)
                    case Polar.NIGHT:
                        return (f"Solar noon {day.noon:%H:%M}", "Polar night", sun)

            return paired(SolarDay.of(site, start.date, zone).map_error(BlenderRenderingFault.lifted), positions(msgspec.structs.replace(setup, timing=Moment(start=start)), zone)).map(text)

        def draw(attached: Attached[SunSetup]) -> RowDraw:
            def shown(layout: bpy.types.UILayout, _holder: Native, context: bpy.types.Context) -> None:
                for text in root_in(bpy.types.Scene, context).bind(identity.reports.reported(lambda scene: attached.read(scene).bind(lines))).default_value(()):
                    layout.label(text=text)

            return shown

        return SunLight.attachment.bound(identity).map(draw)


class SunLight(msgspec.Struct, frozen=True, kw_only=True):
    """Sun setup attachment with owned sun light, world node keys, and paths."""

    retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing

    @staticmethod
    def resited(scene: Handle[bpy.types.Scene], identity: Identity) -> BlenderResult[Option[str]]:
        """Places scene sun from stored setup.

        Args:
            scene: Handle of scene storing setup.
            identity: Identity binding attachment and marking owned objects.

        Returns:
            `Nothing` naming no undo step, or `absent` and `sun_placed` faults.
        """
        return scene.resolve().bind(lambda held: sun_placed(held, identity, bpy.context)).map(lambda _: Nothing)

    attachment: ClassVar[Attachment[SunSetup]] = Attachment(key="sun", form=SunSetup, owner=bpy.types.Scene, changed=Some(reconciling(bpy.types.Scene, resited)))
    sun: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="sun", saved=Saved.USED)
    surface: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="sun_surface", saved=Saved.USED)
    analemmas: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="sun_analemmas", saved=Saved.USED)
    light: ClassVar[Ownership[bpy.types.Light]] = Ownership(kind=bpy.types.Light, key="sun", saved=Saved.USED)
    mesh: ClassVar[Ownership[bpy.types.Mesh]] = Ownership(kind=bpy.types.Mesh, key="sun_surface", saved=Saved.USED)
    curve: ClassVar[Ownership[bpy.types.Curve]] = Ownership(kind=bpy.types.Curve, key="sun_analemmas", saved=Saved.USED)

    commands: ClassVar[tuple[Operator[Empty], Operator[Empty], Operator[Zoned]]] = (
        Operator(
            key="sun_place",
            title="Place Sun",
            description="Place and key scene sun with following world nodes",
            form=Empty,
            run=lambda call: call.root(bpy.types.Scene).bind(lambda scene: sun_placed(scene, call.identity, call.context)).map(lambda _: Done()),
        ),
        Operator(
            key="sun_remove",
            title="Remove Sun",
            description="Remove scene sun, paths, and sun keys",
            form=Empty,
            run=lambda call: call.root(bpy.types.Scene).bind(lambda scene: sun_removed(scene, call.identity)),
        ),
        Operator(
            key="sun_zone",
            title="Time Zone",
            description="Set site time zone",
            form=Zoned,
            invoke=Some(InvokePopup.SEARCH),
            run=lambda call: (
                paired(call.root(bpy.types.Scene), SunLight.attachment.bound(call.identity))
                .bind(lambda pair: pair[1].read(pair[0]).bind(lambda setup: pair[1].write(pair[0], msgspec.structs.replace(setup, site=msgspec.structs.replace(setup.site, zone=call.record.zone)))))
                .map(lambda _: Done())
            ),
        ),
    )

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Yields attachments, ownerships, section, and scene and world subscriptions.

        Args:
            identity: Identity of extension registering row.

        Yields:
            Acquisitions with `Band` pairs.
        """
        with identity.reports.deferred(SunLight.resited) as resite:

            def stored() -> BlenderResult[None]:
                def deferred(attached: Attached[SunSetup]) -> None:
                    for scene in bpy.data.scenes:
                        if attached.stored(scene):
                            resite(Handle.of(scene), identity)

                return SunLight.attachment.bound(identity).map(deferred)

            @contextmanager
            def worlds() -> Generator[BlenderResult[None]]:
                with ExitStack() as stack:
                    yield (
                        WorldEnvironment.attachment
                        .bound(identity)
                        .bind(
                            lambda attached: accumulated(
                                Subscription(owner=kind, setting=kind.bl_rna.properties[name], body=stored).acquisitions(identity)
                                for kind in (attached.group.struct, *attached.group.nested)
                                for name in kind.__annotations__
                            )
                        )
                        .bind(lambda parts: entered(stack, tuple(acquisition for part in parts for _, acquisition in part)))
                    )

            yield Ok((
                SunLight.attachment,
                WorldEnvironment.attachment,
                SunLight.sun,
                SunLight.surface,
                SunLight.analemmas,
                SunLight.light,
                SunLight.mesh,
                SunLight.curve,
                self.section(),
                *(Subscription(owner=bpy.types.Scene, setting=bpy.types.Scene.bl_rna.properties[member], body=stored) for member in ("world", "frame_start")),
                (Band.SUBSCRIPTIONS, worlds()),
            ))

    def section(self) -> Section:
        """Returns scene sun setup with zone button, solar caption, and place and remove buttons."""
        place, remove, zone = self.commands
        return Single(
            key="sun",
            title="Sun",
            rows=(OperatorButton(target=zone), SolarCaption()),
            header=(OperatorButton(target=place), OperatorButton(target=remove)),
            store=Some(Hosted(host=Member(root=bpy.types.Context, path="scene"), store=Store(attachment=SunLight.attachment, retention=self.retention))),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def positions(setup: SunSetup, zone: zoneinfo.ZoneInfo) -> BlenderResult[SunPosition]:
    """Returns sun position at each keyed moment of setup.

    Args:
        setup: Site, timing, and refraction gate.
        zone: Time zone the caller opened.

    Returns:
        One position per frame, apparent with refraction on, or calendar fault.
    """
    calendar = Ok((setup.timing.start.moment(zone),)) if isinstance(setup.timing, Moment) else moments(setup.timing, zone)
    return (
        calendar
        .map(lambda sequence: SunPosition.at(setup.site, np.array([moment.timestamp() for moment in sequence], dtype=np.float64)))
        .map(lambda position: position.apparent() if setup.refraction else position)
        .map_error(BlenderRenderingFault.lifted)
    )


def sun_placed(scene: bpy.types.Scene, identity: Identity, context: bpy.types.Context) -> BlenderResult[bpy.types.Object]:
    """Places and keys scene sun with following nodes, study frame range, and paths.

    Args:
        scene: Scene storing setup.
        identity: Identity binding attachment and marking owned objects.
        context: Context keyframe writes read.

    Returns:
        Sun object, zone or calendar faults before any write, or write faults.
    """

    def built() -> BlenderResult[bpy.types.Object]:
        return (
            BlenderEditFault
            .attempted(f"{scene.name} Sun", lambda: bpy.data.lights.new(vacant(f"{scene.name} Sun", bpy.data.lights.keys()), "SUN"))
            .bind(lambda light: SunLight.light.owned(identity, light, of=Some(scene), filled=lambda light: written(light, "energy", 139.3).map(lambda _: light), scope=Some(rollback)))
            .bind(lambda light: spawned(f"{scene.name} Sun", Some(light), scene.collection))
        )

    def ranged(timing: Timing, count: int) -> BlenderResult[None]:
        match timing:
            case Moment():
                return Ok(None)
            case DayStudy() | SeasonStudy():
                return written(scene, "frame_end", scene.frame_start + count - 1).map(lambda _: None)

    def keyed(setup: SunSetup, position: SunPosition, sun: bpy.types.Object, following: tuple[tuple[bpy.types.NodeTree, Follower], ...]) -> BlenderResult[None]:
        frames, located = (scene.frame_start + np.arange(position.elevation.size)).tolist(), position.vectors * setup.distance
        rows = (
            *((sun, "", "location", axis, located[:, axis]) for axis in range(3)),
            (sun, "", "rotation_euler", 0, position.elevation - math.pi / 2.0),
            (sun, "", "rotation_euler", 2, -position.rotation),
            *((owner, struct, member, index, value(position)) for tree, follower in following for owner, struct, member, index, value in _channels(tree, follower)),
        )
        return accumulated((
            *(Animation(struct=struct, member=member, index=index, source=Keys(points=tuple(zip(frames, values.tolist())))).apply(owner, context) for owner, struct, member, index, values in rows),
            ranged(setup.timing, len(frames)),
        )).map(lambda _: None)

    def followed(tree: bpy.types.NodeTree) -> BlenderResult[tuple[tuple[bpy.types.NodeTree, Follower], ...]]:
        return followers(tree, identity).map(lambda nodes: tuple((tree, node) for node in nodes))

    def placed(setup: SunSetup) -> BlenderResult[bpy.types.Object]:
        try:
            zone = zoneinfo.ZoneInfo(setup.site.zone)
        except (ValueError, zoneinfo.ZoneInfoNotFoundError) as error:
            return Error((BlenderZoneFault(request=setup.site.zone, error=error),))
        return positions(setup, zone).bind(
            lambda position: (
                dropped(scene, identity)
                .bind(
                    lambda _: paired(
                        SunLight.sun.made(identity, built, of=Some(scene), scope=Some(rollback)),
                        Option.of_optional(scene.world).bind(lambda world: Option.of_optional(world.node_tree)).map(followed).default_value(Ok(())),
                    )
                )
                .bind(lambda pair: keyed(setup, position, *pair).bind(lambda _: drawn(setup, scene, identity)).map(lambda _: pair[0]))
            )
        )

    with ExitStack() as rollback:
        return committed(SunLight.attachment.bound(identity).bind(lambda attached: attached.read(scene)).bind(placed), rollback)


def drawn(setup: SunSetup, scene: bpy.types.Scene, identity: Identity) -> BlenderResult[tuple[bpy.types.Object, ...]]:
    """Builds enabled path objects, removing disabled ones.

    Args:
        setup: Site, timing, distance, and gated paths.
        scene: Scene holding path objects.
        identity: Identity marking path objects.

    Returns:
        Shown path objects, or build faults.
    """
    paths, distance, shown = SunPaths.of(setup.site, setup.timing.start.date.year), setup.distance, setup.paths.value
    camera = Snapshot.of(bpy.types.Object, dict.fromkeys(("visible_diffuse", "visible_glossy", "visible_transmission", "visible_volume_scatter", "visible_shadow"), False))
    bevel = Snapshot.of(bpy.types.Curve, {"dimensions": "3D", "bevel_depth": shown.bevel})

    def runs(loops: npt.NDArray[np.float64]) -> tuple[tuple[npt.NDArray[np.float64], bool], ...]:
        def split(loop: npt.NDArray[np.float64]) -> tuple[tuple[npt.NDArray[np.float64], bool], ...]:
            above = loop[:, 2] >= 0.0
            rolled = np.roll(loop, -int(np.argmin(above)), axis=0)
            pieces = np.split(rolled, np.flatnonzero(np.diff(rolled[:, 2] >= 0.0)) + 1)
            return ((loop, True),) if above.all() else tuple((piece, False) for piece in pieces if piece[0, 2] >= 0.0 and len(piece) > 1)

        return tuple(run for loop in loops for run in split(loop))

    geometry = paired(
        Descriptor.resolve(bpy.types.MeshVertex, "co", bpy.types.FloatProperty).bind(lambda co: Descriptor.quantized(co.prop, paths.surface.reshape(-1, 3) * distance)).map(Some)
        if setup.paths.enabled and shown.surface
        else Ok(Nothing),
        Descriptor
        .resolve(bpy.types.SplinePoint, "co", bpy.types.FloatProperty)
        .bind(lambda co: accumulated(Descriptor.quantized(co.prop, np.hstack((run, np.ones((len(run), 1))))).map(lambda points: (points, cyclic)) for run, cyclic in runs(paths.analemmas * distance)))
        .map(Some)
        if setup.paths.enabled and shown.analemmas
        else Ok(Nothing),
    )

    def curved(curve: bpy.types.Curve, loops: tuple[tuple[npt.NDArray[np.float32], bool], ...]) -> BlenderResult[None]:
        def current(spline: bpy.types.Spline, loop: tuple[npt.NDArray[np.float32], bool]) -> bool:
            points, cyclic = loop
            live = np.empty(len(spline.points) * 4, np.float32)
            spline.points.foreach_get("co", live)
            return spline.type == "POLY" and spline.use_cyclic_u == cyclic and np.array_equal(live.view(np.uint32), points.ravel().view(np.uint32))

        def rebuilt() -> None:
            if len(curve.splines) != len(loops) or not all(map(current, curve.splines, loops)):
                curve.splines.clear()
                for points, cyclic in loops:
                    spline = curve.splines.new("POLY")
                    spline.points.add(len(points) - 1)
                    spline.points.foreach_set("co", points.ravel())
                    spline.use_cyclic_u = cyclic

        return BlenderEditFault.attempted(repr(curve), rebuilt).bind(lambda _: bevel.apply(curve).outcome)

    def held[D: bpy.types.ID](row: Ownership[bpy.types.Object], owner: Ownership[D], name: str, data: Callable[[], D], filled: Callable[[D], BlenderResult[None]]) -> BlenderResult[bpy.types.Object]:
        def built() -> BlenderResult[bpy.types.Object]:
            return (
                BlenderEditFault
                .attempted(name, data)
                .bind(lambda block: owner.owned(identity, block, of=Some(scene), scope=Some(rollback)))
                .bind(lambda block: spawned(name, Some(block), scene.collection))
            )

        return row.made(identity, built, of=Some(scene), scope=Some(rollback)).bind(
            lambda item: typed(item.data, owner.kind, f"{item!r}.data").bind(filled).bind(lambda _: reconciled(ObjectState(values=Some(camera)), item, ())).map(lambda _: item)
        )

    def placed(prepared: tuple[Option[npt.NDArray[np.float32]], Option[tuple[tuple[npt.NDArray[np.float32], bool], ...]]]) -> BlenderResult[tuple[bpy.types.Object, ...]]:
        vertices, loops = prepared
        return _gone(tuple(item for row, value in ((SunLight.surface, vertices), (SunLight.analemmas, loops)) if value.is_none() for item in row.serving(identity, scene)), identity).bind(
            lambda _: accumulated((
                *(
                    held(
                        SunLight.surface,
                        SunLight.mesh,
                        f"{scene.name} Sun Surface",
                        lambda: bpy.data.meshes.new(vacant(f"{scene.name} Sun Surface", bpy.data.meshes.keys())),
                        lambda mesh: shaped(mesh, points, paths.faces),
                    )
                    for points in vertices.to_list()
                ),
                *(
                    held(
                        SunLight.analemmas,
                        SunLight.curve,
                        f"{scene.name} Sun Analemmas",
                        lambda: bpy.data.curves.new(vacant(f"{scene.name} Sun Analemmas", bpy.data.curves.keys()), "CURVE"),
                        lambda curve: curved(curve, points),
                    )
                    for points in loops.to_list()
                ),
            ))
        )

    with ExitStack() as rollback:
        return committed(geometry.bind(placed), rollback)


def dropped(scene: bpy.types.Scene, identity: Identity) -> BlenderResult[None]:
    """Clears sun keys on owned non-following world nodes.

    Args:
        scene: Scene with world to read.
        identity: Identity reading node marks and binding environment attachment.

    Returns:
        `Ok` once cleared or without world, or `followers` and clear faults.
    """

    def cleared(tree: bpy.types.NodeTree) -> BlenderResult[None]:
        return (
            followers(tree, identity)
            .bind(
                lambda following: accumulated(
                    Animation(struct=struct, member=member, index=index, source=Keys(points=())).clear(owner)
                    for node in tree.nodes
                    if all((follower.node if isinstance(follower, EnvironmentNode) else follower) != node for follower in following) and Mark.read(identity, node).is_some()
                    for owner, struct, member, index, _ in _channels(tree, node)
                )
            )
            .map(lambda _: None)
        )

    return Option.of_optional(scene.world).bind(lambda world: Option.of_optional(world.node_tree)).map(cleared).default_value(Ok(None))


def sun_removed(scene: bpy.types.Scene, identity: Identity) -> BlenderResult[Outcome]:
    """Removes scene sun light and path objects, clearing follower keys.

    Args:
        scene: Scene holding sun.
        identity: Identity reading marks.

    Returns:
        `Done` noting removed count, `Unchanged` without sun objects, or `followers` and clear faults.
    """
    if not (blocks := tuple(item for row in (SunLight.sun, SunLight.surface, SunLight.analemmas) for item in row.serving(identity, scene))):
        return Ok(Unchanged(reason=f"{scene.name} holds no sun"))

    def cleared(tree: bpy.types.NodeTree) -> BlenderResult[tuple[None, ...]]:
        return followers(tree, identity).bind(
            lambda following: accumulated(
                Animation(struct=struct, member=member, index=index, source=Keys(points=())).clear(owner) for follower in following for owner, struct, member, index, _ in _channels(tree, follower)
            )
        )

    return (
        Option
        .of_optional(scene.world)
        .bind(lambda world: Option.of_optional(world.node_tree))
        .map(cleared)
        .default_value(Ok(()))
        .bind(lambda _: _gone(blocks, identity))
        .map(lambda count: Done(notes=(("INFO", f"Removed {count} sun objects"),)))
    )


def _channels(owner: bpy.types.NodeTree, follower: bpy.types.Node | EnvironmentNode) -> tuple[tuple[bpy.types.ID, str, str, int | None, Callable[[SunPosition], npt.NDArray[np.float64]]], ...]:
    """Returns owner, struct path, member, index, and position value per sun channel, heading applied."""
    node, heading = (follower.node, follower.heading) if isinstance(follower, EnvironmentNode) else (follower, 0.0)
    path = node.path_from_id()
    match node:
        case bpy.types.ShaderNodeTexSky():
            return (
                (owner, path, "sun_elevation", None, lambda position: position.elevation),
                (owner, path, "sun_rotation", None, lambda position: position.rotation),
                *((owner, path, "sun_direction", axis, lambda position, axis=axis: position.vectors[:, axis]) for axis in range(3)),
            )
        case bpy.types.ShaderNodeTexEnvironment():
            return ((owner, f"{path}.texture_mapping", "rotation", 2, lambda position: position.rotation - heading),)
        case _:
            return ()


def _gone(blocks: tuple[bpy.types.Object, ...], identity: Identity) -> BlenderResult[int]:
    """Removes sun objects and unused sun data, returning removed count."""
    return released(blocks).bind(lambda count: accumulated(row.removed(identity) for row in (SunLight.light, SunLight.mesh, SunLight.curve)).map(lambda counts: count + sum(counts)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Moment", "Paths", "SolarCaption", "SunLight", "SunSetup", "Timing", "Zoned", "drawn", "dropped", "positions", "sun_placed", "sun_removed", "zones"]
