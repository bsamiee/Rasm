"""Render queue jobs: the background `prepare` run writing each planned scope's job file, placing its traced files for a farm, and sending the queue to its target."""

from functools import partial
import os
from pathlib import Path
import posixpath

from blender_asset_tracer.hashing import get_hasher
import bpy
from expression import Error, Ok, Option, Result
from expression.extra.result import catch

from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.paths import rna, struct_at
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.files.sessions import BlendFile, FarmTransfer, saved, session_loaded, traced
from blender.bpy.render.outputs.paths import OutputPath
from blender.bpy.render.sessions.engines import samples
from blender.bpy.render.sessions.plans import bare, Dispatcher, JobScope, Plan, Preparing, ranged, scene_of, Sent
from blender.bpy.render.view_layers.passes import pass_flags, soloed, used
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.stores import RecordFile
from blender.rendering.batch.flamenco import Flamenco
from blender.rendering.batch.processes import RunCommand, RunFile
from blender.rendering.batch.queue import FixedCamera, Job, PackedFile, RenderQueue
from blender.rendering.outputs.naming import Output

# --- [OPERATIONS] -----------------------------------------------------------------------


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

    loaded: Outcome[Plan] = bare(RunCommand.PREPARE, trailing).bind(lambda _: RecordFile(path=dispatcher.host.path(arguments.plan, RunFile.PLAN), form=Plan).read())
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
        layers = Option.of_optional(scope.layer).map(lambda name: soloed(scene, name).map(lambda _: None)).default_value(Ok(None))
        return paired(camera, values, layers, dispatcher.framed.grown(scene)).bind(
            lambda parts: dispatcher.pathed.path(scene, day).bind(lambda path: dispatcher.placed(scene, path.copied)).map(lambda path: (path, parts[3][1]))
        )

    def written(scene: bpy.types.Scene, path: OutputPath, pixels: tuple[int, int]) -> Outcome[Job]:
        layers = used(scene)
        camera = (scope.camera.camera,) if isinstance(scope.camera, FixedCamera) else ()
        name = " ".join(bpy.path.clean_name(held) for held in (scope.scene, *Option.of_optional(scope.layer).to_list(), *camera))
        file = path.base / "snapshots" / stem / f"{part}_{name}.blend"
        active: bpy.types.ViewLayer = bpy.context.view_layer  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        layer = Option.of_optional(scope.layer).default_value(active.name)
        outputs = accumulated(path.text(layer, output) for output in Output).map(lambda texts: dict(zip(Output, (posixpath.join(path.root, text) for text in texts))))
        passes = tuple(sorted({rna(descriptor.path) for held in layers for _, descriptor in pass_flags() for owner in struct_at(held, descriptor.owner).to_list() if bool(descriptor.read(owner))}))
        counts = tuple(count for held in layers for count in samples(scene, held).to_list())

        def placed(saved_file: Path) -> Outcome[tuple[PackedFile, tuple[PackedFile, ...]]]:
            if shipping:
                transfer = FarmTransfer()
                return traced(transfer, project=Path(os.path.commonpath((source.parent, saved_file.parent)))).map(lambda pack: (*transfer.placed, pack.skipped)).map(partial(_held, dispatcher))
            return (
                catch(get_hasher(), exception=OSError)(saved_file)
                .map_error(partial(Fault.of, RecordReason.UNREADABLE, str(saved_file)))
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


def _held(dispatcher: Dispatcher, placed: tuple[PackedFile, tuple[PackedFile, ...], tuple[Fault, ...]]) -> tuple[PackedFile, tuple[PackedFile, ...]]:
    """Holds the faults of files a pack skipped in the extension's reports, the job still sending every placed file."""
    blend, files, skipped = placed
    match skipped:
        case (head, *rest):
            dispatcher.identity.reports.hold((head, *rest))
        case _:
            pass
    return blend, files


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["job", "prepared"]
