"""Render queue background commands: `prepare` writing each planned scope's job file, placing its traced files for a farm, and sending the queue to its target; `supervise` holding local render processes; and `render` writing frames."""

from collections.abc import Callable
import datetime
from functools import partial, reduce
import os
from pathlib import Path
import posixpath
import time

from blender_asset_tracer.hashing import get_hasher
import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.paths import rna, struct_at
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.files.sessions import BlendFile, FarmTransfer, saved, session_loaded, traced
from blender.bpy.render.outputs.paths import OutputPath
from blender.bpy.render.sessions.engines import samples
from blender.bpy.render.sessions.plans import Dispatcher, JobScope, Plan, Preparing, Queueing, ranged, scene_of, Sent
from blender.bpy.render.sessions.renders import RenderCall
from blender.bpy.render.view_layers.passes import PassFlags, soloed, used
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason
from blender.records.stores import RecordFile
from blender.rendering.batch.notices import Completed, Estimated, Failed, Started
from blender.rendering.batch.queue import FixedCamera, Job, observed, PackedFile, Progress, RenderFrames, RenderQueue, StageMark
from blender.rendering.batch.supervision import awake, Run, RunCommand, RunFile, shut, supervise
from blender.rendering.batch.targets import Flamenco
from blender.rendering.outputs.naming import Output

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ARGUMENTS]


def _bare(command: str, trailing: tuple[str, ...]) -> Outcome[None]:
    """Refuses trailing arguments a command takes none of."""
    return Error(Fault.of(Reason.OUTSIDE, command, repr(trailing), member="no trailing arguments")) if trailing else Ok(None)


def _scoped(dispatcher: Dispatcher, scene: bpy.types.Scene) -> Outcome[OutputPath]:
    """Returns the output path a scene's job copy holds, absent fault for a scene no job wrote."""
    scope = dispatcher.routing.pathed.scope
    return scope.read(scene).bind(lambda found: found.to_result_with(lambda: Fault.of(Reason.ABSENT, repr(scene), member=scope.name)))


# --- [PREPARE]


def prepared(dispatcher: Dispatcher, arguments: Preparing, trailing: tuple[str, ...]) -> Outcome[Sent]:
    """Writes job files of the stored plan, sends the queue to its target, and stores the `Sent` record beside the plan."""
    source = Path(bpy.data.filepath)
    shipping = isinstance(arguments.target, Flamenco)

    def sent(jobs: tuple[Job, ...]) -> Outcome[Sent]:
        queue = RenderQueue(jobs=jobs, blender=bpy.app.version_string, extension=dispatcher.identity.manifest.version)
        match arguments.target.sent(queue, dispatcher.host):
            case Result(tag="error", error=faults):
                return Error(faults)
            case Result(ok=results):
                record = Sent(queue=queue, target=arguments.target, issued=tuple(value for result in results for value in result.to_option().to_list()))
                for result in results:
                    match result:
                        case Result(tag="ok", ok=issued):
                            dispatcher.identity.reports.logger.info("Queue sent", issued=issued)
                        case Result(error=refused):
                            dispatcher.identity.reports.hold(refused)
                return RecordFile(path=dispatcher.host.path(arguments.plan, RunFile.SENT), form=Sent).written(record).map(lambda _: record)

    def planned(plan: Plan) -> Outcome[tuple[Job, ...]]:
        return accumulated(job(dispatcher, scope, plan, source, arguments.plan, part, shipping=shipping) for part, scope in enumerate(plan.scopes))

    loaded: Outcome[Plan] = _bare(RunCommand.PREPARE, trailing).bind(lambda _: RecordFile(path=dispatcher.host.path(arguments.plan, RunFile.PLAN), form=Plan).read())
    return Result[Plan, Faults].bind(loaded, planned).bind(sent)


def job(dispatcher: Dispatcher, scope: JobScope, plan: Plan, source: Path, stem: str, part: int, *, shipping: bool) -> Outcome[Job]:
    """Writes scope's job file from the saved source, the scope's values and fixed camera applied first, its traced files placed for a farm when shipping under the folder holding both source and job file, so relative paths keep their layout."""
    day = plan.moment.astimezone().date()

    def applied(scene: bpy.types.Scene) -> Outcome[tuple[OutputPath, tuple[int, int]]]:
        camera = (
            Snapshot
            .of(bpy.types.Scene, {("camera",): IdName(name=scope.camera.camera), **{("timeline_markers", index, "camera"): None for index in range(len(scene.timeline_markers))}})
            .apply(scene)
            .outcome
            if isinstance(scope.camera, FixedCamera)
            else Ok(None)
        )
        values = Option.of_optional(scope.values).map(lambda snapshot: snapshot.apply(scene).outcome).default_value(Ok(None))
        for name in Option.of_optional(scope.layer).to_list():
            soloed(scene, name)
        return paired(camera, values, dispatcher.framed.grown(scene)).bind(
            lambda parts: dispatcher.routing.pathed.path(scene, day).bind(lambda path: dispatcher.routing.placed(scene, path.copied)).map(lambda path: (path, parts[2][1]))
        )

    def written(scene: bpy.types.Scene, path: OutputPath, pixels: tuple[int, int]) -> Outcome[Job]:
        layers = used(scene)
        camera = (scope.camera.camera,) if isinstance(scope.camera, FixedCamera) else ()
        name = " ".join(bpy.path.clean_name(held) for held in (scope.scene, *Option.of_optional(scope.layer).to_list(), *camera))
        file = path.base / "snapshots" / stem / f"{part}_{name}.blend"
        active: bpy.types.ViewLayer = bpy.context.view_layer  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        layer = Option.of_optional(scope.layer).default_value(active.name)
        outputs = accumulated(path.text(layer, output) for output in Output).map(lambda texts: dict(zip(Output, (posixpath.join(path.root, text) for text in texts))))
        passes = tuple(
            sorted({rna(descriptor.path) for held in layers for _, descriptor in PassFlags.of().every for owner in struct_at(held, descriptor.owner).to_list() if bool(descriptor.read(owner))})
        )
        counts = tuple(count for held in layers for count in samples(scene, held).to_list())

        def placed(saved_file: Path) -> Outcome[tuple[PackedFile, tuple[PackedFile, ...]]]:
            def held(blend: PackedFile, files: tuple[PackedFile, ...], skipped: tuple[Fault, ...]) -> tuple[PackedFile, tuple[PackedFile, ...]]:
                match skipped:
                    case (head, *rest):
                        dispatcher.identity.reports.hold((head, *rest))
                    case _:
                        pass
                return blend, files

            if shipping:
                transfer = FarmTransfer()
                return traced(transfer, project=Path(os.path.commonpath((source.parent, saved_file.parent)))).map(lambda pack: held(*transfer.placed, pack.skipped))
            return (
                catch(get_hasher(), exception=OSError)(saved_file)
                .map_error(partial(Fault.of, Reason.UNREADABLE, str(saved_file)))
                .map(lambda digest: (PackedFile(source=str(saved_file), path=saved_file.name, digest=bytes.fromhex(digest)), ()))
            )

        def made(texts: dict[Output, str], shipped: tuple[PackedFile, tuple[PackedFile, ...]]) -> Job:
            blend, pack = shipped
            return Job(
                file=blend.source,
                digest=blend.digest,
                scene=scene.name,
                layer=scope.layer,
                engine=scene.render.engine,
                frames=ranged(scene),
                camera=scope.camera,
                pixels=pixels,
                samples=sum(counts) if counts and len(counts) == len(layers) else None,
                output_root=str(path.base),
                outputs=texts,
                passes=passes,
                prerequisites=scope.prerequisites,
                pack=pack,
            )

        return saved(file, bpy.ops.wm.save_mainfile, compress=True, relative_remap=True).bind(lambda saved_file: paired(outputs, placed(saved_file))).map(lambda parts: made(*parts))

    def opened(_: BlendFile) -> Outcome[Job]:
        return scene_of(scope.scene).bind(lambda scene: applied(scene).bind(lambda parts: written(scene, *parts)))

    return session_loaded(bpy.ops.wm.open_mainfile, filepath=str(source)).bind(opened)


# --- [RENDER]


def frames[T](dispatcher: Dispatcher, scene: bpy.types.Scene, numbers: tuple[int, ...], consumed: Callable[[int, float], Outcome[T]], base: Option[Path] = Nothing) -> Outcome[tuple[T, ...]]:
    """Renders and consumes each frame through the call's own frame range, output rerooted under `base` when set."""

    def frame(number: int) -> Outcome[T]:
        started = time.monotonic()
        return dispatcher.render(dispatcher.identity, scene, RenderCall(animation=True, frame_start=Some(number), frame_end=Some(number))).bind(lambda _: consumed(number, time.monotonic() - started))

    return (
        _scoped(dispatcher, scene)
        .bind(lambda original: base.map(lambda root: dispatcher.routing.placed(scene, original.rerooted(root)).map(lambda _: None)).default_value(Ok(None)))
        .bind(lambda _: reduce(lambda done, number: done.bind(lambda values: frame(number).map(lambda value: (*values, value))), numbers, Result[tuple[T, ...], Faults](ok=())))
    )


def commanded(dispatcher: Dispatcher, arguments: Run | RenderFrames, trailing: tuple[str, ...]) -> Outcome[object]:
    """Runs local queue progress or farm worker frame request."""
    match arguments:
        case Run():
            return _bare(RunCommand.RENDER, trailing).bind(lambda _: rendered(dispatcher, arguments))
        case RenderFrames(scene=name, frames=numbers):
            match trailing:
                case (output_root,):
                    return scene_of(name).bind(lambda scene: frames(dispatcher, scene, numbers, lambda _frame, _seconds: Ok(None), Some(Path(output_root)))).map(lambda _: None)
                case _:
                    return Error(Fault.of(Reason.OUTSIDE, RunCommand.RENDER, repr(trailing), member="<output root>"))


def rendered(dispatcher: Dispatcher, run: Run) -> Outcome[tuple[StageMark, ...]]:
    """Renders pending frames of first remaining job, storing each frame mark."""
    identity = dispatcher.identity

    def ran(queue: RenderQueue, queueing: Queueing) -> Outcome[tuple[StageMark, ...]]:
        records = RecordFile(path=dispatcher.host.path(queue.stem, RunFile.PROGRESS), form=Progress)
        holding = frozenset(part for part, job in enumerate(queue.jobs) if job.frames.numbers)

        def finished(progress: Progress, mark: StageMark) -> Outcome[StageMark]:
            before = frozenset(held.part for held in progress.marks[:-1])
            first = holding <= before | {mark.part} and not holding <= before
            for forecast in progress.forecast(queue, datetime.datetime.now(datetime.UTC)).filter(lambda _: first).to_list():
                identity.reports.reported(dispatcher.posted)(queueing.listener, Estimated(forecast=forecast)).to_optional()
            job = queue.jobs[mark.part]
            return (
                scene_of(job.scene).bind(lambda scene: _scoped(dispatcher, scene).bind(lambda path: path.removed(scene, job.frames.numbers))).map(lambda _: mark)
                if all(part != mark.part for part, _ in progress.remaining(queue))
                else Ok(mark)
            )

        def appended(mark: StageMark) -> Outcome[StageMark]:
            persisted: Outcome[Progress] = records.revised(lambda held: msgspec.structs.replace(held, marks=(*held.marks, mark)))
            return Result[Progress, Faults].bind(persisted, lambda held: finished(held, mark))

        def runs(progress: Progress) -> Outcome[tuple[StageMark, ...]]:
            remaining = frozenset(progress.remaining(queue))
            match tuple(step for step in progress.pending if step in remaining):
                case ((part, _), *_) as steps:
                    return scene_of(queue.jobs[part].scene).bind(
                        lambda scene: frames(
                            dispatcher, scene, tuple(frame for _, frame in steps), lambda frame, seconds: appended(StageMark(part=part, item=frame, elapsed=datetime.timedelta(seconds=seconds)))
                        )
                    )
                case _:
                    return Ok(())

        loaded: Outcome[Progress] = records.read()
        return Result[Progress, Faults].bind(loaded, runs)

    return paired(RecordFile(path=Path(run.queue), form=RenderQueue).read(), dispatcher.settings(identity)).bind(lambda pair: ran(*pair))


# --- [SUPERVISE]


def supervised(dispatcher: Dispatcher, run: Run, trailing: tuple[str, ...]) -> Outcome[Progress]:
    """Supervises local render processes, sleep held off while the preferences keep the machine awake, posting notices."""
    identity, host = dispatcher.identity, dispatcher.host

    def ran(queue: RenderQueue, queueing: Queueing) -> Outcome[Progress]:
        def completed(progress: Progress) -> Outcome[Progress]:
            identity.reports.reported(dispatcher.posted)(queueing.listener, Completed(frames=len(progress.marks), elapsed=progress.elapsed)).to_optional()
            return (shut() if queueing.shutdown else Ok(None)).map(lambda _: progress)

        def failed(faults: Faults) -> Outcome[Progress]:
            identity.reports.reported(dispatcher.posted)(queueing.listener, Failed(job=queue.stem, reason=" ".join(fault.message for fault in faults))).to_optional()
            return Error(faults)

        def started(observations: Option[tuple[Progress, datetime.datetime]]) -> Outcome[Progress]:
            if not Progress.resumed(observations, queue, os.getpid()).marks:
                identity.reports.reported(dispatcher.posted)(queueing.listener, Started(jobs=len(queue.jobs), frames=len(queue.steps))).to_optional()
            operation = partial(supervise, run.target, queue, host, datetime.timedelta(seconds=queueing.interval))
            return (awake(operation) if queueing.awake else operation()).map(completed).default_with(failed)

        return observed(host.path(queue.stem, RunFile.PROGRESS)).bind(started)

    return _bare(RunCommand.SUPERVISE, trailing).bind(lambda _: paired(RecordFile(path=Path(run.queue), form=RenderQueue).read(), dispatcher.settings(identity))).bind(lambda pair: ran(*pair))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["commanded", "frames", "job", "prepared", "rendered", "supervised"]
