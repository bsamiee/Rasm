import type { ExecutorContext } from '@nx/devkit';
import { Effect, Option, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import { Dry, dried, forwarded, packaged, released, run, Words } from '#/executor.ts';
import { spawned } from '#/inference.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Attach = Dry.mapFields(Struct.assign({ __unparsed__: Words })).annotate({
    title: 'Attach',
    description:
        "Uploads the files a project's packaging targets wrote to the GitHub release of the project's tag at the checked-out commit through gh release upload --clobber, the tag nx release made from release.releaseTag.pattern, and uploads nothing where nx release released no version of the project at that commit, a dry run previewing every project",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* (stated: typeof Attach.Type, context: ExecutorContext) {
    const [dry, { project, files }, tag] = yield* Effect.all([dried(stated), packaged(context), released(context)]);
    const upload = (at: string) => ['gh', 'release', 'upload', at, ...files, '--clobber', ...forwarded(stated.__unparsed__ ?? [])] as const;
    const pending = `<tag nx release makes for ${project}>`;
    yield* dry
        ? Effect.logInfo(`Would run ${CommandLine.display(upload(Option.getOrElse(tag, () => pending)))}`)
        : Option.match(tag, { onNone: () => Effect.logInfo(`Skipped ${project}: HEAD holds no tag of ${project}, so nx release released no version of it at this commit`), onSome: (at) => Effect.asVoid(spawned(upload(at), { cwd: context.root })) });
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Attach, execute };
