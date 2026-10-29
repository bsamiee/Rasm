"""Keyed site sun light, following world nodes, and sun paths."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
import datetime
from functools import partial
import math
from types import MappingProxyType
from typing import ClassVar, Final
import zoneinfo

import bpy
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, Unchanged
from blender.bpy.data.ids.ownership import Mark, Ownership, released, retained, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.animation import Animation, Keys
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.mirrors import annotated, choices, nested, reconciling
from blender.bpy.data.rna.paths import id_path, Native, RnaPath, root_in, rooted
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import Subscription
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band, entered
from blender.bpy.scene.lights.groups import Lobe
from blender.bpy.scene.objects import ObjectState, owned_object, reconciled, shaped, VERTEX
from blender.bpy.scene.worlds import Follower, followers, WorldEnvironment
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.layout.rows import Line, OperatorButton, RowDraw
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Store
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Retention
from blender.rendering.lighting.solar import Daylight, Polar, Site, SolarDay, Study, SunPaths, SunPosition

# --- [CONSTANTS] ------------------------------------------------------------------------

_SPLINE_POINT: Final = Descriptor.declared(bpy.types.SplinePoint, ("co",), bpy.types.FloatProperty)
_IRRADIANCE: Final = Descriptor.declared(bpy.types.SunLight, ("energy",), bpy.types.FloatProperty)
_CAMERA_ALONE: Final[Mapping[RnaPath, Value]] = MappingProxyType(dict[RnaPath, Value]({(name,): False for name in (*(lobe.value for lobe in Lobe), "visible_shadow")}))
_ZONES: Final = tuple(sorted(zoneinfo.available_timezones()))
_TIMINGS: Final = choices((("MOMENT", "Moment", "Single sun moment"), ("DAY", "Day", "Moments stepped by elapsed minutes"), ("SEASON", "Season", "Moments stepped by days at one clock time")))

# --- [MODELS] ---------------------------------------------------------------------------


class SunSetup(bpy.types.PropertyGroup):
    """Scene sun setup: observer site, local timing, refraction, distance, and path objects."""

    latitude: bpy.props.FloatProperty(name="Latitude", description="Angle north of equator", subtype="ANGLE", min=-math.pi / 2.0, max=math.pi / 2.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    longitude: bpy.props.FloatProperty(name="Longitude", description="Angle east of Greenwich", subtype="ANGLE", min=-math.pi, max=math.pi)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    north: bpy.props.FloatProperty(name="North", description="True north counterclockwise from +Y", subtype="ANGLE", min=-math.pi, max=math.pi)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    zone: bpy.props.StringProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Time Zone",
        description="IANA time zone of site clock",
        default="UTC",
        search=lambda _setup, _context, text: tuple(zone for zone in _ZONES if text.casefold() in zone.casefold()),
        search_options={"SORT"},
    )
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
                lambda held: paired(SolarDay.of(held[0], held[1].start.date()), msgspec.structs.replace(held[1], frames=1).positions(held[0], refraction=setup.refraction)).map(text)
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
        return scene.resolve().bind(lambda held: sun_placed(held, identity)).map(lambda _: Nothing)

    attachment: ClassVar[Attachment[SunSetup]] = Attachment(key="sun", form=SunSetup, owner=bpy.types.Scene, changed=Some(partial(reconciling, bpy.types.Scene, resited)))
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
                        WorldEnvironment.attachment.bound(identity).bind(
                            lambda attached: entered(
                                stack,
                                tuple(
                                    Subscription(owner=kind, setting=kind.bl_rna.properties[name], body=stored).held(identity)
                                    for kind in (attached.form, *nested(attached.form))
                                    for name in annotated(kind)
                                ),
                            )
                        )
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

    def section(self) -> Section[SunSetup]:
        """Returns scene sun setup with solar caption, and place and remove buttons."""
        return Section(
            key="sun",
            title="Sun",
            rows=(SolarCaption(),),
            header=(OperatorButton(target=sun_place), OperatorButton(target=sun_remove)),
            store=Some(Store(attachment=SunLight.attachment, retention=self.retention)),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def sited(setup: SunSetup) -> Outcome[tuple[Site, Study]]:
    """Returns the site and timing a setup states, a moment as a one-frame study, `outside` for a site or time zone the record refuses."""
    start = datetime.datetime.combine(datetime.date(setup.year, setup.month, 1) + datetime.timedelta(days=setup.day - 1), datetime.time(setup.hour, setup.minute))
    match setup.timing:
        case "DAY":
            timing = Study(start=start, step=datetime.timedelta(minutes=setup.minutes), frames=setup.frames)
        case "SEASON":
            timing = Study(start=start, step=datetime.timedelta(days=setup.days), frames=setup.frames)
        case _:
            timing = Study(start=start, step=datetime.timedelta(), frames=1)
    site: Outcome[Site] = catch(msgspec.convert, exception=msgspec.ValidationError)(
        {"latitude": setup.latitude, "longitude": setup.longitude, "north": setup.north, "zone": setup.zone}, Site
    ).map_error(lambda error: Fault.of(Reason.OUTSIDE, repr(setup), str(error), member="a site the time zone database holds"))
    return site.map(lambda held: (held, timing))


def sun_placed(scene: bpy.types.Scene, identity: Identity) -> Outcome[bpy.types.Object]:
    """Places and keys scene sun with following nodes, study frame range, and paths.

    Args:
        scene: Scene storing setup.
        identity: Identity binding attachment and marking owned objects.

    Returns:
        Sun object, calendar faults before any write, or write faults.
    """
    name = f"{scene.name} Sun"

    def ranged(setup: SunSetup, count: int) -> Outcome[None]:
        return Ok(None) if setup.timing == "MOMENT" else written(scene, ("frame_end",), scene.frame_start + count - 1).map(lambda _: None)

    def keyed(setup: SunSetup, position: SunPosition, sun: bpy.types.Object, following: tuple[tuple[bpy.types.NodeTree, Follower], ...]) -> Outcome[None]:
        frames, located = (scene.frame_start + np.arange(position.elevation.size)).tolist(), position.vectors * setup.distance
        rows = (
            *((sun, ("location", axis), located[:, axis]) for axis in range(3)),
            (sun, ("rotation_euler", 0), position.elevation - math.pi / 2.0),
            (sun, ("rotation_euler", 2), -position.rotation),
            *((owner, path, value(position)) for tree, follower in following for owner, path, value in _channels(tree, *follower)),
        )
        return accumulated((*(Animation(path=path, source=Keys(points=tuple(zip(frames, values.tolist())))).apply(owner) for owner, path, values in rows), ranged(setup, len(frames)))).map(
            lambda _: None
        )

    def followed(tree: bpy.types.NodeTree) -> Outcome[tuple[tuple[bpy.types.NodeTree, Follower], ...]]:
        return followers(tree, identity).map(lambda nodes: tuple((tree, node) for node in nodes))

    def lit() -> Outcome[tuple[bpy.types.Object, bpy.types.Light]]:
        seeded: Callable[[bpy.types.Light], Outcome[bpy.types.Light]] = lambda light: _IRRADIANCE.write(light, 139.3).map(lambda _: light)
        return owned_object((SunLight.sun, SunLight.light), name, partial(bpy.data.lights.new, name, "SUN"), scene.collection, identity, of=Some(scene), scope=Some(rollback), seeded=seeded)

    def placed(setup: SunSetup, site: Site, timing: Study) -> Outcome[bpy.types.Object]:
        return timing.positions(site, refraction=setup.refraction).bind(
            lambda position: (
                dropped(scene, identity)
                .bind(lambda _: paired(lit(), _tree(scene).map(followed).default_value(Ok(()))))
                .bind(
                    lambda pair: (
                        keyed(setup, position, pair[0][0], pair[1])
                        .bind(lambda _: drawn(site, timing.start.year, Some(setup).filter(lambda held: held.use_paths), scene, identity))
                        .map(lambda _: pair[0][0])
                    )
                )
            )
        )

    with ExitStack() as rollback:
        return retained(SunLight.attachment.read(identity, scene).bind(lambda setup: sited(setup).bind(lambda held: placed(setup, *held))), rollback)


def drawn(site: Site, year: int, paths: Option[SunSetup], scene: bpy.types.Scene, identity: Identity) -> Outcome[tuple[bpy.types.Object, ...]]:
    """Builds shown path objects, removing the others.

    Args:
        site: Observer location the paths trace.
        year: Year the paths trace.
        paths: Setup of the path objects to show and their distance from origin, `Nothing` showing none.
        scene: Scene holding path objects.
        identity: Identity marking path objects.

    Returns:
        Shown path objects, or build faults.
    """
    traced = SunPaths.of(site, year)
    camera = Snapshot.of(bpy.types.Object, _CAMERA_ALONE)
    bevel = Snapshot.of(bpy.types.Curve, {("dimensions",): "3D", ("bevel_depth",): paths.map(lambda shown: shown.bevel).default_value(0.0)})
    geometry = paired(
        paths.filter(lambda shown: shown.surface).map(lambda shown: Descriptor.quantized(VERTEX.prop, traced.surface.reshape(-1, 3) * shown.distance).map(Some)).default_value(Ok(Nothing)),
        paths
        .filter(lambda shown: shown.analemmas)
        .map(
            lambda shown: accumulated(
                Descriptor.quantized(_SPLINE_POINT.prop, np.hstack((run * shown.distance, np.ones((len(run), 1))))).map(lambda points: (points, cyclic)) for run, cyclic in traced.arcs
            ).map(Some)
        )
        .default_value(Ok(Nothing)),
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

    def held[D: bpy.types.ID](rows: tuple[Ownership[bpy.types.Object], Ownership[D]], name: str, data: Callable[[], D], filled: Callable[[D], Outcome[None]]) -> Outcome[bpy.types.Object]:
        return owned_object(rows, name, data, scene.collection, identity, of=Some(scene), scope=Some(rollback)).bind(
            lambda pair: filled(pair[1]).bind(lambda _: reconciled(ObjectState(values=Some(camera)), pair[0], ())).map(lambda _: pair[0])
        )

    def placed(prepared: tuple[Option[npt.NDArray[np.float32]], Option[tuple[tuple[npt.NDArray[np.float32], bool], ...]]]) -> Outcome[tuple[bpy.types.Object, ...]]:
        vertices, loops = prepared
        surface, analemmas = f"{scene.name} Sun Surface", f"{scene.name} Sun Analemmas"
        return _gone(tuple(item for row, value in ((SunLight.surface, vertices), (SunLight.analemmas, loops)) if value.is_none() for item in row.serving(identity, scene)), identity).bind(
            lambda _: accumulated((
                *(held((SunLight.surface, SunLight.mesh), surface, partial(bpy.data.meshes.new, surface), lambda mesh: shaped(mesh, points, traced.faces)) for points in vertices.to_list()),
                *(held((SunLight.analemmas, SunLight.curve), analemmas, partial(bpy.data.curves.new, analemmas, "CURVE"), lambda curve: curved(curve, points)) for points in loops.to_list()),
            ))
        )

    with ExitStack() as rollback:
        return retained(geometry.bind(placed), rollback)


def dropped(scene: bpy.types.Scene, identity: Identity) -> Outcome[None]:
    """Clears sun keys on owned world nodes that follow no sun, `Ok` without world."""

    def unfollowed(tree: bpy.types.NodeTree, following: tuple[Follower, ...]) -> tuple[tuple[bpy.types.Node, float], ...]:
        held = frozenset(node for node, _ in following)
        return tuple((node, 0.0) for node in tree.nodes if node not in held and Mark.read(identity, node).is_some())

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


def _unkeyed(scene: bpy.types.Scene, identity: Identity, chosen: Callable[[bpy.types.NodeTree, tuple[Follower, ...]], Iterable[tuple[bpy.types.Node, float]]]) -> Outcome[None]:
    """Clears sun channel keys of the world nodes `chosen` picks from the tree and its followers."""

    def cleared(tree: bpy.types.NodeTree) -> Outcome[None]:
        return (
            followers(tree, identity)
            .bind(
                lambda following: accumulated(
                    Animation(path=path, source=Keys(points=())).clear(owner) for node, heading in chosen(tree, following) for owner, path, _ in _channels(tree, node, heading)
                )
            )
            .map(lambda _: None)
        )

    return _tree(scene).map(cleared).default_value(Ok(None))


def _channels(owner: bpy.types.NodeTree, node: bpy.types.Node, heading: float) -> tuple[tuple[bpy.types.ID, RnaPath, Callable[[SunPosition], npt.NDArray[np.float64]]], ...]:
    """Returns owner, channel path ending at the member or its element, and position value per sun channel, the image sun heading applied."""
    match node, id_path(node).to_optional():
        case bpy.types.ShaderNodeTexSky(), tuple(path):
            return (
                (owner, (*path, "sun_elevation"), lambda position: position.elevation),
                (owner, (*path, "sun_rotation"), lambda position: position.rotation),
                *((owner, (*path, "sun_direction", axis), lambda position, axis=axis: position.vectors[:, axis]) for axis in range(3)),
            )
        case bpy.types.ShaderNodeTexEnvironment(), tuple(path):
            return ((owner, (*path, "texture_mapping", "rotation", 2), lambda position: position.rotation - heading),)
        case _:
            return ()


def _gone(blocks: tuple[bpy.types.Object, ...], identity: Identity) -> Outcome[int]:
    """Removes sun objects and unused sun data, returning removed count."""
    return released(blocks).bind(lambda count: accumulated(row.removed(identity) for row in (SunLight.light, SunLight.mesh, SunLight.curve)).map(lambda counts: count + sum(counts)))


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(Empty)
def sun_place(call: Call[Empty]) -> Outcome[Done]:
    """Places and keys the scene sun with following world nodes."""
    return rooted(bpy.types.Scene, call.context).bind(lambda scene: sun_placed(scene, call.identity)).map(lambda _: Done())


@Operator.of(Empty)
def sun_remove(call: Call[Empty]) -> Outcome[Answer]:
    """Removes the scene sun, its paths, and its keys."""
    return rooted(bpy.types.Scene, call.context).bind(lambda scene: sun_removed(scene, call.identity))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["SolarCaption", "SunLight", "SunSetup", "drawn", "dropped", "sited", "sun_place", "sun_placed", "sun_remove", "sun_removed"]
