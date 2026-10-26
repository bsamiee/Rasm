"""Keyed site sun light, following world nodes, and sun paths."""

from collections.abc import Callable, Generator, Iterable
from contextlib import contextmanager, ExitStack
import datetime
from functools import partial
import math
from typing import ClassVar, Final
import zoneinfo

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.operators import Answer, Call, Done, Empty, InvokePopup, Operator, Unchanged
from blender.bpy.data.ids.ownership import committed, Mark, Ownership, released, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.animation import Animation, Keys
from blender.bpy.data.rna.descriptors import Descriptor, Native, root_in, typed, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band, entered
from blender.bpy.faults import BlenderReason
from blender.bpy.scene.objects import ObjectState, reconciled, shaped, spawned, VERTEX
from blender.bpy.scene.worlds import EnvironmentNode, Follower, followers, WorldEnvironment
from blender.bpy.settings.attachments import Attached, Attachment, nested, reconciling
from blender.bpy.ui.layout.rows import Line, OperatorButton, RowDraw
from blender.bpy.ui.layout.sections import Section, Single
from blender.bpy.ui.layout.values import Hosted, Store
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.fields import Case, NonNegative
from blender.records.trees import Retention
from blender.rendering.lighting.solar import Daylight, DayStudy, LocalTime, moments, Polar, SeasonStudy, Site, SolarDay, SunPaths, SunPosition

# --- [TYPES] ----------------------------------------------------------------------------

type Timing = Moment | DayStudy | SeasonStudy


# --- [CONSTANTS] ------------------------------------------------------------------------

_SPLINE_POINT: Final = Descriptor.declared(bpy.types.SplinePoint, "co", bpy.types.FloatProperty)
_IRRADIANCE: Final = Descriptor.declared(bpy.types.SunLight, "energy", bpy.types.FloatProperty)
ZONES: Final = tuple((zone, zone, "") for zone in sorted(zoneinfo.available_timezones()))
_TIMINGS: Final = (("MOMENT", "Moment", "Single sun moment"), ("DAY", "Day", "Moments stepped by elapsed minutes"), ("SEASON", "Season", "Moments stepped by days at one clock time"))

# --- [MODELS] ---------------------------------------------------------------------------


class Moment(Case, frozen=True, kw_only=True):
    """Single sun moment."""

    start: LocalTime = LocalTime()


class Paths(msgspec.Struct, frozen=True, kw_only=True):
    """Sun path objects and analemma tube radius."""

    surface: bool = True
    analemmas: bool = True
    bevel: NonNegative = 0.0254


class SunSetup(bpy.types.PropertyGroup):
    """Scene sun setup: observer site, local timing, refraction, distance, and path objects."""

    latitude: bpy.props.FloatProperty(name="Latitude", description="Angle north of equator", subtype="ANGLE", min=-math.pi / 2.0, max=math.pi / 2.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    longitude: bpy.props.FloatProperty(name="Longitude", description="Angle east of Greenwich", subtype="ANGLE", min=-math.pi, max=math.pi)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    north: bpy.props.FloatProperty(name="North", description="True north counterclockwise from +Y", subtype="ANGLE", min=-math.pi, max=math.pi)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    zone: bpy.props.EnumProperty(name="Time Zone", description="IANA time zone of site clock", items=ZONES, default="UTC")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    timing: bpy.props.EnumProperty(name="Timing", description="Moment, or study keying one moment per frame", items=_TIMINGS, default="MOMENT")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    year: bpy.props.IntProperty(name="Year", min=datetime.MINYEAR, max=datetime.MAXYEAR, default=2000)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    month: bpy.props.IntProperty(name="Month", min=1, max=12, default=1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    day: bpy.props.IntProperty(name="Day", description="Day of month, rolling into next month", min=1, max=31, default=1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    hour: bpy.props.IntProperty(name="Hour", description="Hour on site clock of the moment or first frame", min=0, max=23, default=12)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    minute: bpy.props.IntProperty(name="Minute", min=0, max=59)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    minutes: bpy.props.IntProperty(name="Minutes", description="Minutes between frames of a day study", min=1, max=1440, default=30)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    days: bpy.props.IntProperty(name="Days", description="Days between frames of a season study", min=1, max=366, default=30)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    frames: bpy.props.IntProperty(name="Frames", description="Frames a study keys", min=1, default=25)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    refraction: bpy.props.BoolProperty(name="Refraction", description="Apparent elevation", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    distance: bpy.props.FloatProperty(name="Distance", description="Sun and path distance from origin", subtype="DISTANCE", min=0.0, soft_max=304.8, default=15.24)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_paths: bpy.props.BoolProperty(name="Paths", description="Path objects visible to camera alone")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    surface: bpy.props.BoolProperty(name="Surface", description="Yearly sky band of sun", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    analemmas: bpy.props.BoolProperty(name="Analemmas", description="Hourly sun positions over year", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    bevel: bpy.props.FloatProperty(name="Bevel", description="Analemma tube radius", subtype="DISTANCE", min=0.0, soft_min=0.00635, soft_max=0.3048, default=0.0254)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Zoned(bpy.types.PropertyGroup):
    """Site time zone."""

    zone: bpy.props.EnumProperty(name="Time Zone", description="IANA time zone of site clock", items=ZONES, default="UTC")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class SolarCaption(Line, frozen=True, kw_only=True):
    """Row of sunrise, solar noon, sunset, day length, and sun position at start."""

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Returns caption draw, `register` fault for a refused attachment name."""

        def lines(setup: SunSetup) -> Outcome[tuple[str, ...]]:
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

            return sited(setup).bind(
                lambda held: paired(SolarDay.of(held[0], held[1].start.date, held[0].clock), positions(held[0], Moment(start=held[1].start), refraction=setup.refraction)).map(text)
            )

        def shown(attached: Attached[SunSetup], layout: bpy.types.UILayout, _holder: Native, context: bpy.types.Context) -> None:
            for text in root_in(bpy.types.Scene, context).bind(identity.reports.reported(lambda scene: lines(attached.struct(scene)))).default_value(()):
                layout.label(text=text)

        return SunLight.attachment.bound(identity).map(lambda attached: self.conditioned(partial(shown, attached)))


class SunLight(msgspec.Struct, frozen=True, kw_only=True):
    """Sun setup attachment with owned sun light, world node keys, and paths."""

    retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing

    @staticmethod
    def resited(scene: Handle[bpy.types.Scene], identity: Identity) -> Outcome[Option[str]]:
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

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Yields attachments, ownerships, section, and scene and world subscriptions.

        Args:
            identity: Identity of extension registering row.

        Yields:
            Acquisitions with `Band` pairs.
        """
        with identity.reports.deferred(SunLight.resited) as resite:

            def stored() -> Outcome[None]:
                def deferred(attached: Attached[SunSetup]) -> None:
                    for scene in bpy.data.scenes:
                        if attached.stored(scene):
                            resite(Handle.of(scene), identity)

                return SunLight.attachment.bound(identity).map(deferred)

            @contextmanager
            def worlds() -> Generator[Outcome[None]]:
                with ExitStack() as stack:
                    yield (
                        WorldEnvironment.attachment
                        .bound(identity)
                        .bind(
                            lambda attached: accumulated(
                                Subscription(owner=kind, setting=kind.bl_rna.properties[name], body=stored).acquisitions(identity)
                                for kind in (attached.form, *nested(attached.form))
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
        return Single(
            key="sun",
            title="Sun",
            rows=(OperatorButton(target=sun_zone_command), SolarCaption()),
            header=(OperatorButton(target=sun_place_command), OperatorButton(target=sun_remove_command)),
            store=Some(Hosted(store=Store(attachment=SunLight.attachment, retention=self.retention))),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def sited(setup: SunSetup) -> Outcome[tuple[Site, Timing]]:
    """Returns the site and timing a setup states, `outside` for a time zone the database lacks."""
    start = LocalTime(year=setup.year, month=setup.month, day=setup.day, hour=setup.hour, minute=setup.minute)
    match setup.timing:
        case "DAY":
            timing: Timing = DayStudy(start=start, minutes=setup.minutes, frames=setup.frames)
        case "SEASON":
            timing = SeasonStudy(start=start, days=setup.days, frames=setup.frames)
        case _:
            timing = Moment(start=start)
    site: Outcome[Site] = catch(Site, exception=ValueError)(latitude=setup.latitude, longitude=setup.longitude, north=setup.north, zone=setup.zone).map_error(
        lambda error: (Fault(reason=RecordReason.OUTSIDE, subject=repr(setup), member="zone", cause=error),)
    )
    return site.map(lambda held: (held, timing))


def positions(site: Site, timing: Timing, *, refraction: bool) -> Outcome[SunPosition]:
    """Returns sun position at each keyed moment of timing on the site clock.

    Args:
        site: Observer location and clock.
        timing: Moment, or study keying one moment per frame.
        refraction: Whether positions are apparent.

    Returns:
        One position per frame, apparent with refraction on, or calendar fault.
    """
    zone = site.clock
    calendar = Ok((timing.start.moment(zone),)) if isinstance(timing, Moment) else moments(timing, zone)
    return calendar.map(lambda sequence: SunPosition.at(site, np.array([moment.timestamp() for moment in sequence], dtype=np.float64))).map(
        lambda position: position.apparent() if refraction else position
    )


def sun_placed(scene: bpy.types.Scene, identity: Identity, context: bpy.types.Context) -> Outcome[bpy.types.Object]:
    """Places and keys scene sun with following nodes, study frame range, and paths.

    Args:
        scene: Scene storing setup.
        identity: Identity binding attachment and marking owned objects.
        context: Context keyframe writes read.

    Returns:
        Sun object, calendar faults before any write, or write faults.
    """

    def built() -> Outcome[bpy.types.Object]:
        return (
            Result[bpy.types.Light, Faults]
            .bind(
                catch(bpy.data.lights.new, exception=RuntimeError)(f"{scene.name} Sun", "SUN").map_error(
                    lambda error: (Fault(reason=BlenderReason.REFUSED, subject=f"{scene.name} Sun", cause=error),)
                ),
                lambda light: SunLight.light.owned(identity, light, of=Some(scene), filled=lambda light: _IRRADIANCE.write(light, 139.3).map(lambda _: light), scope=Some(rollback)),
            )
            .bind(lambda light: spawned(f"{scene.name} Sun", Some(light), scene.collection))
        )

    def ranged(timing: Timing, count: int) -> Outcome[None]:
        match timing:
            case Moment():
                return Ok(None)
            case DayStudy() | SeasonStudy():
                return written(scene, "frame_end", scene.frame_start + count - 1).map(lambda _: None)

    def keyed(setup: SunSetup, timing: Timing, position: SunPosition, sun: bpy.types.Object, following: tuple[tuple[bpy.types.NodeTree, Follower], ...]) -> Outcome[None]:
        frames, located = (scene.frame_start + np.arange(position.elevation.size)).tolist(), position.vectors * setup.distance
        rows = (
            *((sun, "", "location", axis, located[:, axis]) for axis in range(3)),
            (sun, "", "rotation_euler", 0, position.elevation - math.pi / 2.0),
            (sun, "", "rotation_euler", 2, -position.rotation),
            *((owner, struct, member, index, value(position)) for tree, follower in following for owner, struct, member, index, value in _channels(tree, follower)),
        )
        return accumulated((
            *(Animation(struct=struct, member=member, index=index, source=Keys(points=tuple(zip(frames, values.tolist())))).apply(owner, context) for owner, struct, member, index, values in rows),
            ranged(timing, len(frames)),
        )).map(lambda _: None)

    def followed(tree: bpy.types.NodeTree) -> Outcome[tuple[tuple[bpy.types.NodeTree, Follower], ...]]:
        return followers(tree, identity).map(lambda nodes: tuple((tree, node) for node in nodes))

    def placed(setup: SunSetup, site: Site, timing: Timing) -> Outcome[bpy.types.Object]:
        paths = Some(Paths(surface=setup.surface, analemmas=setup.analemmas, bevel=setup.bevel)).filter(lambda _: setup.use_paths)
        return positions(site, timing, refraction=setup.refraction).bind(
            lambda position: (
                dropped(scene, identity)
                .bind(lambda _: paired(SunLight.sun.made(identity, built, of=Some(scene), scope=Some(rollback)), _tree(scene).map(followed).default_value(Ok(()))))
                .bind(lambda pair: keyed(setup, timing, position, *pair).bind(lambda _: drawn(site, timing.start.date.year, setup.distance, paths, scene, identity)).map(lambda _: pair[0]))
            )
        )

    with ExitStack() as rollback:
        return committed(SunLight.attachment.read(identity, scene).bind(lambda setup: sited(setup).bind(lambda held: placed(setup, *held))), rollback)


def drawn(site: Site, year: int, distance: float, paths: Option[Paths], scene: bpy.types.Scene, identity: Identity) -> Outcome[tuple[bpy.types.Object, ...]]:
    """Builds shown path objects, removing the others.

    Args:
        site: Observer location the paths trace.
        year: Year the paths trace.
        distance: Path distance from origin.
        paths: Path objects to show, `Nothing` showing none.
        scene: Scene holding path objects.
        identity: Identity marking path objects.

    Returns:
        Shown path objects, or build faults.
    """
    traced = SunPaths.of(site, year)
    camera = Snapshot.of(bpy.types.Object, dict.fromkeys(("visible_diffuse", "visible_glossy", "visible_transmission", "visible_volume_scatter", "visible_shadow"), False))
    bevel = Snapshot.of(bpy.types.Curve, {"dimensions": "3D", "bevel_depth": paths.map(lambda shown: shown.bevel).default_value(0.0)})

    def runs(loops: npt.NDArray[np.float64]) -> tuple[tuple[npt.NDArray[np.float64], bool], ...]:
        def split(loop: npt.NDArray[np.float64]) -> tuple[tuple[npt.NDArray[np.float64], bool], ...]:
            above = loop[:, 2] >= 0.0
            rolled = np.roll(loop, -int(np.argmin(above)), axis=0)
            pieces = np.split(rolled, np.flatnonzero(np.diff(rolled[:, 2] >= 0.0)) + 1)
            return ((loop, True),) if above.all() else tuple((piece, False) for piece in pieces if piece[0, 2] >= 0.0 and len(piece) > 1)

        return tuple(run for loop in loops for run in split(loop))

    geometry = paired(
        Descriptor.quantized(VERTEX.prop, traced.surface.reshape(-1, 3) * distance).map(Some) if paths.filter(lambda shown: shown.surface).is_some() else Ok(Nothing),
        accumulated(Descriptor.quantized(_SPLINE_POINT.prop, np.hstack((run, np.ones((len(run), 1))))).map(lambda points: (points, cyclic)) for run, cyclic in runs(traced.analemmas * distance)).map(
            Some
        )
        if paths.filter(lambda shown: shown.analemmas).is_some()
        else Ok(Nothing),
    )

    def curved(curve: bpy.types.Curve, loops: tuple[tuple[npt.NDArray[np.float32], bool], ...]) -> Outcome[None]:
        def current(spline: bpy.types.Spline, loop: tuple[npt.NDArray[np.float32], bool]) -> bool:
            points, cyclic = loop
            live = np.empty(len(spline.points) * 4, np.float32)
            spline.points.foreach_get("co", live)
            return spline.type == "POLY" and spline.use_cyclic_u == cyclic and np.array_equal(live.view(np.uint32), points.ravel().view(np.uint32))

        if len(curve.splines) != len(loops) or not all(map(current, curve.splines, loops)):
            curve.splines.clear()
            for points, cyclic in loops:
                spline = curve.splines.new("POLY")
                spline.points.add(len(points) - 1)
                spline.points.foreach_set("co", points.ravel())
                spline.use_cyclic_u = cyclic
        return bevel.apply(curve).outcome

    def held[D: bpy.types.ID](row: Ownership[bpy.types.Object], owner: Ownership[D], name: str, data: Callable[[], D], filled: Callable[[D], Outcome[None]]) -> Outcome[bpy.types.Object]:
        def built() -> Outcome[bpy.types.Object]:
            return (
                Result[D, Faults]
                .bind(
                    catch(data, exception=RuntimeError)().map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=name, cause=error),)),
                    lambda block: owner.owned(identity, block, of=Some(scene), scope=Some(rollback)),
                )
                .bind(lambda block: spawned(name, Some(block), scene.collection))
            )

        return row.made(identity, built, of=Some(scene), scope=Some(rollback)).bind(
            lambda item: typed(item.data, owner.kind, f"{item!r}.data").bind(filled).bind(lambda _: reconciled(ObjectState(values=Some(camera)), item, ())).map(lambda _: item)
        )

    def placed(prepared: tuple[Option[npt.NDArray[np.float32]], Option[tuple[tuple[npt.NDArray[np.float32], bool], ...]]]) -> Outcome[tuple[bpy.types.Object, ...]]:
        vertices, loops = prepared
        return _gone(tuple(item for row, value in ((SunLight.surface, vertices), (SunLight.analemmas, loops)) if value.is_none() for item in row.serving(identity, scene)), identity).bind(
            lambda _: accumulated((
                *(
                    held(SunLight.surface, SunLight.mesh, f"{scene.name} Sun Surface", lambda: bpy.data.meshes.new(f"{scene.name} Sun Surface"), lambda mesh: shaped(mesh, points, traced.faces))
                    for points in vertices.to_list()
                ),
                *(
                    held(SunLight.analemmas, SunLight.curve, f"{scene.name} Sun Analemmas", lambda: bpy.data.curves.new(f"{scene.name} Sun Analemmas", "CURVE"), lambda curve: curved(curve, points))
                    for points in loops.to_list()
                ),
            ))
        )

    with ExitStack() as rollback:
        return committed(geometry.bind(placed), rollback)


def dropped(scene: bpy.types.Scene, identity: Identity) -> Outcome[None]:
    """Clears sun keys on owned world nodes that follow no sun, `Ok` without world."""

    def unfollowed(tree: bpy.types.NodeTree, following: tuple[Follower, ...]) -> tuple[bpy.types.Node, ...]:
        held = frozenset(follower.node if isinstance(follower, EnvironmentNode) else follower for follower in following)
        return tuple(node for node in tree.nodes if node not in held and Mark.read(identity, node).is_some())

    return _unkeyed(scene, identity, unfollowed)


def sun_removed(scene: bpy.types.Scene, identity: Identity) -> Outcome[Answer]:
    """Removes scene sun light and path objects, clearing follower keys.

    Args:
        scene: Scene holding sun.
        identity: Identity reading marks.

    Returns:
        `Done` noting removed count, `Unchanged` without sun objects, or `followers` and clear faults.
    """
    if not (blocks := tuple(item for row in (SunLight.sun, SunLight.surface, SunLight.analemmas) for item in row.serving(identity, scene))):
        return Ok(Unchanged(reason=f"{scene.name} holds no sun"))
    return _unkeyed(scene, identity, lambda _, following: following).bind(lambda _: _gone(blocks, identity)).map(lambda count: Done(notes=(("INFO", f"Removed {count} sun objects"),)))


def _tree(scene: bpy.types.Scene) -> Option[bpy.types.NodeTree]:
    """Returns scene world node tree."""
    return Option.of_optional(scene.world).bind(lambda world: Option.of_optional(world.node_tree))


def _unkeyed(scene: bpy.types.Scene, identity: Identity, chosen: Callable[[bpy.types.NodeTree, tuple[Follower, ...]], Iterable[bpy.types.Node | EnvironmentNode]]) -> Outcome[None]:
    """Clears sun channel keys of the world nodes `chosen` picks from the tree and its followers."""

    def cleared(tree: bpy.types.NodeTree) -> Outcome[None]:
        return (
            followers(tree, identity)
            .bind(
                lambda following: accumulated(
                    Animation(struct=struct, member=member, index=index, source=Keys(points=())).clear(owner)
                    for node in chosen(tree, following)
                    for owner, struct, member, index, _ in _channels(tree, node)
                )
            )
            .map(lambda _: None)
        )

    return _tree(scene).map(cleared).default_value(Ok(None))


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


def _gone(blocks: tuple[bpy.types.Object, ...], identity: Identity) -> Outcome[int]:
    """Removes sun objects and unused sun data, returning removed count."""
    return released(blocks).bind(lambda count: accumulated(row.removed(identity) for row in (SunLight.light, SunLight.mesh, SunLight.curve)).map(lambda counts: count + sum(counts)))


# --- [COMPOSITION] ----------------------------------------------------------------------


def sun_place(call: Call[Empty]) -> Outcome[Done]:
    """Places and keys the scene sun with following world nodes."""
    return call.root(bpy.types.Scene).bind(lambda scene: sun_placed(scene, call.identity, call.context)).map(lambda _: Done())


def sun_remove(call: Call[Empty]) -> Outcome[Answer]:
    """Removes the scene sun, its paths, and its keys."""
    return call.root(bpy.types.Scene).bind(lambda scene: sun_removed(scene, call.identity))


def sun_zone(call: Call[Zoned]) -> Outcome[Answer]:
    """Sets the site time zone."""
    return (
        call
        .root(bpy.types.Scene)
        .bind(lambda scene: SunLight.attachment.read(call.identity, scene))
        .bind(lambda setup: written(setup, "zone", call.properties.zone))
        .map(lambda prior: Done() if prior.is_some() else Unchanged(reason=f"Site clock is {call.properties.zone}"))
    )


sun_place_command: Operator[Empty] = Operator(form=Empty, run=sun_place)
sun_remove_command: Operator[Empty] = Operator(form=Empty, run=sun_remove)
sun_zone_command: Operator[Zoned] = Operator(form=Zoned, invoke=Some(InvokePopup.SEARCH), primary=Some("zone"), run=sun_zone)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ZONES",
    "Moment",
    "Paths",
    "SolarCaption",
    "SunLight",
    "SunSetup",
    "Timing",
    "Zoned",
    "drawn",
    "dropped",
    "positions",
    "sited",
    "sun_place_command",
    "sun_placed",
    "sun_remove_command",
    "sun_removed",
    "sun_zone_command",
]
