"""Follow-up render planning, stored pass sequences, and load callbacks."""

from collections.abc import Callable
import datetime
from functools import cached_property, partial
from itertools import starmap
from pathlib import Path

import bpy
from expression import Option, Result, Some, try_downcast
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.paths import RnaPath
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Handler
from blender.bpy.render.outputs.paths import OutputPath, ScenePaths
from blender.bpy.render.outputs.results import COMBINED, Written
from blender.bpy.render.sessions.plans import JobScope, Plan, scene_of
from blender.bpy.render.view_layers.passes import PassRequest, required, used
from blender.bpy.render.view_layers.routing import RoutingRow
from blender.bpy.settings.attachments import Attachment
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Store
from blender.records.faults import accumulated, Fault, Nothing, Ok, Outcome, paired, Reason

# --- [MODELS] ---------------------------------------------------------------------------


class FollowUpSettings(bpy.types.PropertyGroup):
    """Scene setting queueing a follow-up job after each render, a follow-up's own settings extending it."""

    enabled: bpy.props.BoolProperty(name="Enabled", description="Queue the follow-up job after each render")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class WrittenPass(bpy.types.PropertyGroup):
    """Sequence file of one written pass, named after the pass, relative to the output root, with the image output holding the pass."""

    path: bpy.props.StringProperty(name="Path", description="Sequence file of the first frame")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    output: bpy.props.StringProperty(name="Output", description="Image output holding the pass")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class FollowedRender(bpy.types.PropertyGroup):
    """Frame range of the render a follow-up job reads, each follow-up's record holding one written pass member per pass it follows, empty outside the job's file."""

    first: bpy.props.IntProperty(name="First", description="First rendered frame", default=1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    last: bpy.props.IntProperty(name="Last", description="Last rendered frame", default=1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    def written(self, name: str, /) -> Outcome[WrittenPass]:
        """Returns the written pass of a pass name, absent fault for a pass the record follows none of or the render wrote none of."""
        found: Result[object, ValueError] = catch(self.path_resolve, exception=ValueError)(_member_key(name))
        return found.map_error(lambda error: Fault.of(Reason.ABSENT, repr(self), error, member=name)).bind(
            lambda value: Option.of_optional(try_downcast(WrittenPass, value)).filter(lambda held: bool(held.path)).to_result_with(partial(Fault.of, Reason.ABSENT, repr(self), member=name))
        )


class FollowUp[G: FollowUpSettings](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Job queued after each planned render whose scene enables the `form` setting under `key`, `built` making its work in the job file at load.

    Followed render writes Combined and the `passes` flags turn on, with `held` scene values applied; the follow-up job's file holds the written
    sequences in its followed render record, renders with every compositor entry off, and writes its frames one output version later.
    """

    key: Key
    form: type[G]
    built: Callable[[Identity, bpy.types.Scene, FollowedRender, G, Path], Outcome[None]]
    passes: tuple[Descriptor, ...] = ()
    held: Option[Snapshot] = Nothing

    @cached_property
    def settings(self) -> Attachment[G]:
        """Scene setting enabling the job, extended by the follow-up's own settings."""
        return Attachment(key=self.key, form=self.form, owner=bpy.types.Scene)

    @property
    def names(self) -> tuple[str, ...]:
        """Names of the passes the job follows, Combined first."""
        return (COMBINED, *(flag.prop.name for flag in self.passes))

    @cached_property
    def record(self) -> Attachment[FollowedRender]:
        """Followed render record the job file holds, one written pass member per followed pass."""
        members = {_member_key(name): bpy.props.PointerProperty(type=WrittenPass, name=name) for name in self.names}
        form: type[FollowedRender] = type(f"FollowedRender_{self.key}", (FollowedRender,), {"__doc__": FollowedRender.__doc__, "__annotations__": members})
        return Attachment(key=f"{self.key}_render", form=form, owner=bpy.types.Scene)

    @cached_property
    def section(self) -> Section[G]:
        """Scene setting fields with their value presets."""
        return Section(key=self.key, store=Some(Store(attachment=self.settings, presets=Some(Preset(key=self.key, form=Snapshot)))))

    # --- [PLANNING]

    def planned(self, identity: Identity, plan: Plan) -> Outcome[Plan]:
        """Declares enabled follow-up scopes without changing scene data."""
        return self._planned(identity, plan, lambda _scene, index, scope, _day: Ok(self._scopes(index, scope)))

    def prepared(self, identity: Identity, plan: Plan) -> Outcome[Plan]:
        """Materializes pass routes and complete follow-up values before dispatch saves the file."""
        return self._planned(identity, plan, partial(self.scoped, identity))

    def _planned(self, identity: Identity, plan: Plan, scoped: Callable[[bpy.types.Scene, int, JobScope, datetime.date], Outcome[tuple[JobScope, Option[JobScope]]]]) -> Outcome[Plan]:
        """Selects enabled follow-ups and assembles their scopes after the base scopes."""
        day = plan.moment.astimezone().date()

        def followed(index: int, scope: JobScope) -> Outcome[tuple[JobScope, Option[JobScope]]]:
            return scene_of(scope.scene).bind(lambda scene: self.settings.read(identity, scene).bind(lambda held: scoped(scene, index, scope, day) if held.enabled else Ok((scope, Nothing))))

        return accumulated(starmap(followed, enumerate(plan.scopes))).map(
            lambda pairs: msgspec.structs.replace(plan, scopes=(*(base for base, _ in pairs), *(follow for _, held in pairs for follow in held.to_list())))
        )

    def _scopes(self, index: int, scope: JobScope, values: Snapshot | None = None) -> tuple[JobScope, Option[JobScope]]:
        """Combines held base values with the follow-up's metadata and source dependency."""
        kept = self.held.map(lambda held: Option.of_optional(scope.values).map(lambda prior: msgspec.structs.replace(prior, tree=prior.tree.merged(held.tree))).default_value(held))
        return (
            kept.map(lambda merged: msgspec.structs.replace(scope, values=merged)).default_value(scope),
            Some(JobScope(scene=scope.scene, camera=scope.camera, layer=scope.layer, values=values, prerequisites=(index,))),
        )

    def scoped(self, identity: Identity, scene: bpy.types.Scene, index: int, scope: JobScope, day: datetime.date) -> Outcome[tuple[JobScope, Option[JobScope]]]:
        """Routes the scope's passes to its output and returns the scope under the held values with the follow-up scope reading what it writes."""
        requests = tuple(PassRequest(layer=layer.name, flags=frozenset(self.passes)) for layer in used(scene)) if self.passes else ()
        names = self.names

        def located(files: tuple[Written, ...]) -> Outcome[tuple[tuple[str, str, str], ...]]:
            routed = [
                (COMBINED if name == "Image" else name, (written.path, output))
                for written in files
                if scope.layer is None or written.layer == Some(scope.layer)
                for output, name in written.passes.items()
            ]
            sources = dict(reversed(routed))
            return accumulated(Option.of_optional(sources.get(name)).map(lambda held: (name, *held)).to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene), member=name)) for name in names)

        def planned(record: str, outputs: str, version: int, copied: OutputPath, found: tuple[tuple[str, str, str], ...]) -> tuple[JobScope, Option[JobScope]]:
            values: dict[RnaPath, Value] = {
                **{(record, _member_key(name), member): value for name, path, output in found for member, value in (("path", Path(path).relative_to(copied.base).as_posix()), ("output", output))},
                (record, "first"): scene.frame_start,
                (record, "last"): scene.frame_end,
                (outputs, "version"): version + 1,
                **{("compositor_effects", position, "enable_for_render"): False for position in range(len(scene.compositor_effects))},
            }
            return self._scopes(index, scope, Snapshot.of(bpy.types.Scene, values))

        return paired(ScenePaths().bound(identity), RoutingRow.bound(identity), self.record.bound(identity)).bind(
            lambda bound: (
                bound[0]
                .path(scene, day)
                .map(lambda path: path.copied)
                .bind(lambda copied: required(scene, bpy.context, requests).bind(lambda _: bound[1].reconciled(scene, bpy.context, Ok(copied))).map(lambda _: copied))
                .bind(
                    lambda copied: (
                        RoutingRow()
                        .written(scene, scene.frame_start, identity, Some(True))
                        .bind(located)
                        .map(lambda found: planned(bound[2].name, bound[0].attached.name, bound[0].attached.struct(scene).version, copied, found))
                    )
                )
            )
        )

    # --- [LOADING]

    def loaded(self, identity: Identity) -> Outcome[None]:
        """Runs `built` for every scene a job file loads holding a followed render, under the scene's stored output root."""

        def ran(scene: bpy.types.Scene) -> Outcome[None]:
            return paired(self.record.read(identity, scene), self.settings.read(identity, scene), ScenePaths().bound(identity).bind(lambda pathed: pathed.stored.read(scene))).bind(
                lambda parts: parts[2].filter(lambda _: parts[0].written(COMBINED).is_ok()).map(lambda path: self.built(identity, scene, parts[0], parts[1], path.base)).default_value(Ok(None))
            )

        return accumulated(map(ran, bpy.data.scenes)).map(lambda _: None)

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Acquires the scene setting, the followed render record, and the load handler running `built`."""
        return self.settings, self.record, Handler(handlers=bpy.app.handlers.load_post, body=partial(self.loaded, identity))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _member_key(name: str, /) -> Key:
    """Returns the record member a pass name keeps its written pass under."""
    return bpy.path.clean_name(name).lower()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FollowUp", "FollowUpSettings", "FollowedRender", "WrittenPass"]
