"""Sun lamp of a site at a moment or over a study, its keys on the lamp and following world nodes, and sun paths."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import math
import sys
from typing import Annotated, ClassVar
import zoneinfo

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.operators import Done, Empty, InvokePopup, Operator, Outcome, Unchanged
from blender.bpy.data.ids.ownership import earliest, Mark, Ownership, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.animation import Animation, Keys
from blender.bpy.data.rna.descriptors import Descriptor, Native, root_in, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band, entered
from blender.bpy.faults import BlenderChoiceFault, BlenderEditFault, BlenderRenderingFault, BlenderResult, BlenderZoneFault
from blender.bpy.scene.objects import ObjectState, reconciled, shaped, spawned
from blender.bpy.scene.worlds import EnvironmentNode, Follower, followers, WorldEnvironment
from blender.bpy.settings.attachments import Attached, Attachment, reconciling
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.layout.rows import Line, Member, OperatorButton, RowDraw
from blender.bpy.ui.layout.sections import Hosted, Section, Single, Store
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case, Gated, Presentation, Quantity
from blender.rendering.history import Retention
from blender.rendering.lighting.solar import Daylight, DayStudy, LocalTime, moments, Polar, SeasonStudy, Site, SolarDay, SunPaths, SunPosition

# --- [TYPES] ----------------------------------------------------------------------------

type Timing = Moment | DayStudy | SeasonStudy


# --- [CHOICES] --------------------------------------------------------------------------

zones = Choices(lambda _identity, _owner, _context: ((zone, zone, "") for zone in sorted(zoneinfo.available_timezones())))


# --- [MODELS] ---------------------------------------------------------------------------


class Moment(Case, frozen=True, kw_only=True, tag=3):
    """One moment the sun stands at."""

    title = "Moment"
    start: Annotated[LocalTime, msgspec.Meta(title="Moment", description="Date and clock time the sun stands at")] = LocalTime()


class Paths(msgspec.Struct, frozen=True, kw_only=True):
    """Sun path objects and the radius of the analemma tubes."""

    surface: Annotated[bool, msgspec.Meta(title="Surface", description="Sky band the sun crosses over the year")] = True
    analemmas: Annotated[bool, msgspec.Meta(title="Analemmas", description="Sun position at each hour over the year")] = True
    bevel: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Bevel", description="Radius of the analemma tubes"), Presentation(soft=Some((0.00635, 0.3048)), quantity=Some(Quantity.LENGTH))
    ] = 0.0254


class SunSetup(msgspec.Struct, frozen=True, kw_only=True):
    """Site, timing, refraction, distance, and paths of a scene's sun."""

    site: Annotated[Site, msgspec.Meta(title="Site", description="Place on Earth the sun is seen from")] = Site()
    timing: Annotated[Timing, msgspec.Meta(title="Timing", description="One moment, or a study keyed one moment per frame")] = Moment()
    refraction: Annotated[bool, msgspec.Meta(title="Refraction", description="Apparent elevation through the atmosphere")] = True
    distance: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Distance", description="Lamp and path distance from the world origin"),
        Presentation(soft=Some((0.0, 304.8)), quantity=Some(Quantity.LENGTH)),
    ] = 15.24
    paths: Annotated[Gated[Paths], msgspec.Meta(title="Paths", description="Sun paths as objects the camera sees")] = Gated(enabled=False, value=Paths())


class Zoned(msgspec.Struct, frozen=True, kw_only=True):
    """Time zone the zone operator writes into the site."""

    zone: Annotated[str, msgspec.Meta(title="Time Zone", description="IANA time zone of the site's clock"), zones] = Site().zone


class SolarCaption(Line, frozen=True, kw_only=True):
    """Row of the start date's sunrise, sunset, solar noon, and day length, and the start's sun position."""

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Binds the sun attachment into the row's draw.

        Args:
            identity: Extension whose attachment the draw reads.

        Returns:
            `Ok` of the draw, or `register` for a refused attachment name.
        """

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
            def shown(layout: bpy.types.UILayout, holder: Native, context: bpy.types.Context) -> None:
                for text in root_in(bpy.types.Scene, context).bind(identity.reports.reported(lambda scene: attached.read(scene).bind(lines))).default_value(()):
                    layout.label(text=text)

            return shown

        return SunLamp.attachment.bound(identity).map(draw)


class SunLamp(msgspec.Struct, frozen=True, kw_only=True):
    """Row attaching `SunSetup` to scenes with the owned lamp, its keys, the following sky and environment keys, and the paths."""

    retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing

    @staticmethod
    def resited(scene: Handle[bpy.types.Scene], identity: Identity) -> BlenderResult[Option[str]]:
        """Places a scene's sun from the setup it stores, the write a setup edit, a world change, or a start frame change defers.

        Args:
            scene: Handle of the scene storing the setup.
            identity: Identity binding the attachment and marking owned objects.

        Returns:
            `Ok` of `Nothing`, a placement pushing no undo step; `absent` for a missing scene and the faults of `sun_placed`.
        """
        return scene.resolve().bind(lambda held: sun_placed(held, identity, bpy.context)).map(lambda _: Nothing)

    attachment: ClassVar[Attachment[SunSetup]] = Attachment(key="sun", form=SunSetup, owner=bpy.types.Scene, changed=Some(reconciling(bpy.types.Scene, resited)))
    lamp: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="sun", saved=Saved.USED)
    surface: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="sun_surface", saved=Saved.USED)
    analemmas: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="sun_analemmas", saved=Saved.USED)
    light: ClassVar[Ownership[bpy.types.Light]] = Ownership(kind=bpy.types.Light, key="sun", saved=Saved.USED)
    mesh: ClassVar[Ownership[bpy.types.Mesh]] = Ownership(kind=bpy.types.Mesh, key="sun_surface", saved=Saved.USED)
    curve: ClassVar[Ownership[bpy.types.Curve]] = Ownership(kind=bpy.types.Curve, key="sun_analemmas", saved=Saved.USED)

    commands: ClassVar[tuple[Operator[Empty], Operator[Empty], Operator[Zoned]]] = (
        Operator(
            key="sun_place",
            title="Place Sun",
            description="Place and key the scene's sun lamp and the world nodes following it",
            form=Empty,
            run=lambda call: call.root(bpy.types.Scene).bind(lambda scene: sun_placed(scene, call.identity, call.context)).map(lambda _: Done()),
        ),
        Operator(
            key="sun_remove",
            title="Remove Sun",
            description="Remove the scene's sun lamp, its paths, and every sun key",
            form=Empty,
            run=lambda call: call.root(bpy.types.Scene).bind(lambda scene: sun_removed(scene, call.identity)),
        ),
        Operator(
            key="sun_zone",
            title="Time Zone",
            description="Choose the site's time zone",
            form=Zoned,
            invoke=Some(InvokePopup.SEARCH),
            run=lambda call: (
                paired(call.root(bpy.types.Scene), SunLamp.attachment.bound(call.identity))
                .bind(lambda pair: pair[1].read(pair[0]).bind(lambda setup: pair[1].write(pair[0], msgspec.structs.replace(setup, site=msgspec.structs.replace(setup.site, zone=call.record.zone)))))
                .map(lambda _: Done())
            ),
        ),
    )

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Answers the attachment, ownership merges, operators, and scene and environment subscriptions.

        Args:
            identity: Extension placing the row.

        Yields:
            `Ok` of every band and acquisition pair, or every construction fault.
        """
        with identity.reports.deferred(SunLamp.resited) as resite:

            def stored() -> BlenderResult[None]:
                def deferred(attached: Attached[SunSetup]) -> None:
                    for scene in bpy.data.scenes:
                        if attached.stored(scene):
                            resite(Handle.of(scene), identity)

                return SunLamp.attachment.bound(identity).map(deferred)

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
                SunLamp.attachment,
                WorldEnvironment.attachment,
                SunLamp.lamp,
                SunLamp.surface,
                SunLamp.analemmas,
                SunLamp.light,
                SunLamp.mesh,
                SunLamp.curve,
                self.section(),
                *(Subscription(owner=bpy.types.Scene, setting=bpy.types.Scene.bl_rna.properties[member], body=stored) for member in ("world", "frame_start")),
                (Band.SUBSCRIPTIONS, worlds()),
            ))

    def section(self) -> Section:
        """Section drawing the scene's sun setup, the zone button, the solar day caption, and the place and remove buttons."""
        place, remove, zone = self.commands
        return Single(
            key="sun",
            title="Sun",
            rows=(OperatorButton(target=zone), SolarCaption()),
            header=(OperatorButton(target=place), OperatorButton(target=remove)),
            store=Some(Hosted(host=Member(root=bpy.types.Context, path="scene"), store=Store(attachment=SunLamp.attachment, retention=self.retention))),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def positions(setup: SunSetup, zone: zoneinfo.ZoneInfo) -> BlenderResult[SunPosition]:
    """Answers the sun's position at each keyed moment of a setup.

    Args:
        setup: Site, timing, and refraction gate.
        zone: Native zone opened at the consuming boundary.

    Returns:
        One position per frame at the selected elevation, or the study's native calendar overflow.
    """
    calendar = Ok((setup.timing.start.moment(zone),)) if isinstance(setup.timing, Moment) else moments(setup.timing, zone)
    return (
        calendar
        .map(lambda sequence: SunPosition.at(setup.site, np.array([moment.timestamp() for moment in sequence], dtype=np.float64)))
        .map(lambda position: position.apparent() if setup.refraction else position)
        .map_error(BlenderRenderingFault.lifted)
    )


def sun_placed(scene: bpy.types.Scene, identity: Identity, context: bpy.types.Context) -> BlenderResult[bpy.types.Object]:
    """Places a scene's sun: the lamp and following nodes keyed, the study's frame range, and the paths.

    Args:
        scene: Scene storing the setup.
        identity: Identity binding the attachment and marking owned objects.
        context: Context the channel writes read.

    Returns:
        `Ok` of the lamp; native zone or calendar failures before host mutation, and faults of the setup, cleanup, build, channels, frame range, and paths.
    """
    mark = Mark(key=SunLamp.lamp.key, of=Some(scene))

    def retained(item: bpy.types.Object) -> bpy.types.Object:
        rollback.callback(bpy.data.batch_remove, ids=(item,))
        return item

    def lit(light: bpy.types.Light) -> BlenderResult[bpy.types.Object]:
        rollback.callback(bpy.data.batch_remove, ids=(light,))
        return written(light, "energy", 139.3).bind(lambda _: spawned("Sun", Some(light), scene.collection)).bind(lambda item: SunLamp.lamp.owned(identity, item, mark)).map(retained)

    def built() -> BlenderResult[bpy.types.Object]:
        try:
            light = bpy.data.lights.new("Sun", "SUN")
        except (RuntimeError, TypeError, ValueError) as error:
            return Error((BlenderEditFault(request="Sun", error=error),))
        return SunLamp.light.owned(identity, light, Mark(key=SunLamp.light.key, of=Some(scene))).bind(lit)

    def ranged(timing: Timing, count: int) -> BlenderResult[None]:
        match timing:
            case Moment():
                return Ok(None)
            case DayStudy() | SeasonStudy():
                return written(scene, "frame_end", scene.frame_start + count - 1).map(lambda _: None)

    def keyed(setup: SunSetup, position: SunPosition, lamp: bpy.types.Object, following: tuple[tuple[bpy.types.NodeTree, Follower], ...]) -> BlenderResult[None]:
        frames, located = (scene.frame_start + np.arange(position.elevation.size)).tolist(), position.vectors * setup.distance
        rows = (
            *((lamp, "", "location", axis, located[:, axis]) for axis in range(3)),
            (lamp, "", "rotation_euler", 0, position.elevation - math.pi / 2.0),
            (lamp, "", "rotation_euler", 2, -position.rotation),
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
                        Option.of_optional(earliest(SunLamp.lamp.held(identity)).get(mark)).map(Ok).default_with(built),
                        Option.of_optional(scene.world).bind(lambda world: Option.of_optional(world.node_tree)).map(followed).default_value(Ok(())),
                    )
                )
                .bind(lambda pair: keyed(setup, position, *pair).bind(lambda _: drawn(setup, scene, identity)).map(lambda _: pair[0]))
            )
        )

    def published(item: bpy.types.Object) -> bpy.types.Object:
        rollback.pop_all()
        return item

    with ExitStack() as rollback:
        return SunLamp.attachment.bound(identity).bind(lambda attached: attached.read(scene)).bind(placed).map(published)


def drawn(setup: SunSetup, scene: bpy.types.Scene, identity: Identity) -> BlenderResult[tuple[bpy.types.Object, ...]]:
    """Builds the sun path objects a setup turns on and removes those it turns off.

    Args:
        setup: Site, timing, distance, and gated paths.
        scene: Scene holding the path objects.
        identity: Identity marking the path objects.

    Returns:
        `Ok` of the path objects shown; the faults of each build and reconcile.
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
        def current(spline: bpy.types.Spline, points: npt.NDArray[np.float32], cyclic: bool) -> bool:
            live = np.empty(len(spline.points) * 4, np.float32)
            spline.points.foreach_get("co", live)
            return spline.type == "POLY" and spline.use_cyclic_u == cyclic and np.array_equal(live.view(np.uint32), points.ravel().view(np.uint32))

        try:
            if len(curve.splines) != len(loops) or not all(current(spline, *loop) for spline, loop in zip(curve.splines, loops)):
                curve.splines.clear()
                for points, cyclic in loops:
                    spline = curve.splines.new("POLY")
                    spline.points.add(len(points) - 1)
                    spline.points.foreach_set("co", points.ravel())
                    spline.use_cyclic_u = cyclic
        except (ReferenceError, RuntimeError, TypeError, ValueError) as error:
            return Error((BlenderEditFault(request=repr(curve), error=error),))
        return bevel.apply(curve).outcome

    def retained[D: bpy.types.ID](block: D) -> D:
        rollback.callback(bpy.data.batch_remove, ids=(block,))
        return block

    def held[D: bpy.types.ID](row: Ownership[bpy.types.Object], owner: Ownership[D], name: str, data: Callable[[], D], filled: Callable[[D], BlenderResult[None]]) -> BlenderResult[bpy.types.Object]:
        mark = Mark(key=row.key, of=Some(scene))

        def reused(item: bpy.types.Object) -> BlenderResult[tuple[bpy.types.Object, D]]:
            block = item.data
            return Ok((item, block)) if isinstance(block, owner.kind) else Error((BlenderChoiceFault(path=f"{item!r}.data", identifier=type(block).__name__, accepted=(owner.kind.__name__,)),))

        def built() -> BlenderResult[tuple[bpy.types.Object, D]]:
            try:
                block = data()
            except (RuntimeError, TypeError, ValueError) as error:
                return Error((BlenderEditFault(request=name, error=error),))
            return (
                owner
                .owned(identity, block, Mark(key=owner.key, of=Some(scene)))
                .map(retained)
                .bind(lambda block: spawned(name, Some(block), scene.collection).bind(lambda item: row.owned(identity, item, mark)).map(retained).map(lambda item: (item, block)))
            )

        return (
            Option
            .of_optional(earliest(row.held(identity)).get(mark))
            .map(reused)
            .default_with(built)
            .bind(lambda pair: filled(pair[1]).bind(lambda _: reconciled(ObjectState(values=Some(camera)), pair[0], ())).map(lambda _: pair[0]))
        )

    def placed(prepared: tuple[Option[npt.NDArray[np.float32]], Option[tuple[tuple[npt.NDArray[np.float32], bool], ...]]]) -> BlenderResult[tuple[bpy.types.Object, ...]]:
        vertices, loops = prepared
        return _gone(_marked(tuple(row for row, value in ((SunLamp.surface, vertices), (SunLamp.analemmas, loops)) if value.is_none()), scene, identity), scene, identity).bind(
            lambda _: accumulated((
                *(held(SunLamp.surface, SunLamp.mesh, "Sun Surface", lambda: bpy.data.meshes.new("Sun Surface"), lambda mesh: shaped(mesh, points, paths.faces)) for points in vertices.to_list()),
                *(held(SunLamp.analemmas, SunLamp.curve, "Sun Analemmas", lambda: bpy.data.curves.new("Sun Analemmas", "CURVE"), lambda curve: curved(curve, points)) for points in loops.to_list()),
            ))
        )

    def published(items: tuple[bpy.types.Object, ...]) -> tuple[bpy.types.Object, ...]:
        rollback.pop_all()
        return items

    with ExitStack() as rollback:
        return geometry.bind(placed).map(published)


def dropped(scene: bpy.types.Scene, identity: Identity) -> BlenderResult[None]:
    """Clears the sun's channels on owned world nodes no longer following.

    Args:
        scene: Scene whose world is read.
        identity: Identity reading node marks and binding the environment attachment.

    Returns:
        `Ok` once cleared, also for a scene with no world; the faults of `followers` and each clear.
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
    """Removes a scene's lamp and path objects and clears the followers' channels.

    Args:
        scene: Scene holding the sun.
        identity: Identity reading the marks.

    Returns:
        `Ok` of `Done` noting the objects removed, `Unchanged` where the scene holds no sun object; the faults of `followers` and each clear.
    """
    if not (blocks := _marked((SunLamp.lamp, SunLamp.surface, SunLamp.analemmas), scene, identity)):
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
        .bind(lambda _: _gone(blocks, scene, identity))
        .map(lambda count: Done(notes=(("INFO", f"Removed {count} sun objects"),)))
    )


def _channels(owner: bpy.types.NodeTree, follower: bpy.types.Node | EnvironmentNode) -> tuple[tuple[bpy.types.ID, str, str, int | None, Callable[[SunPosition], npt.NDArray[np.float64]]], ...]:
    """Each sun channel's owning ID, path, member, index, and value of position, including an environment heading."""
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


def _marked(rows: tuple[Ownership[bpy.types.Object], ...], scene: bpy.types.Scene, identity: Identity) -> tuple[bpy.types.Object, ...]:
    """Answers the scene's holders of the rows."""
    return tuple(block for row in rows for block, mark in row.held(identity) if mark.of == Some(scene))


def _gone(blocks: tuple[bpy.types.Object, ...], scene: bpy.types.Scene, identity: Identity) -> BlenderResult[int]:
    """Removes a scene's sun objects in one `batch_remove` with the unused sun data, answering their count."""
    try:
        if blocks:
            bpy.data.batch_remove(ids=blocks)
        return Ok(len(blocks) + sum(row.removed(identity) for row in (SunLamp.light, SunLamp.mesh, SunLamp.curve)))
    except (ReferenceError, RuntimeError, TypeError, ValueError) as error:
        return Error((BlenderEditFault(request=repr(scene), error=error),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Moment", "Paths", "SolarCaption", "SunLamp", "SunSetup", "Timing", "Zoned", "drawn", "dropped", "positions", "sun_placed", "sun_removed", "zones"]
