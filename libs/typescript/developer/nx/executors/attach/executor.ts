import { Cause, Effect, Match, Option, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import { Dry, forwarded, packaged, released } from '#/executor';
import { dried, executor, ran, spawned, Words } from '#/inference';

// --- [MODELS] --------------------------------------------------------------------------

const Attach = Dry.mapFields(Struct.assign({ __unparsed__: Words })).annotate({
    title: 'Attach',
    description:
        "Uploads the files a project's packaging targets wrote to the GitHub release of the project's tag at the checked-out commit through gh release upload --clobber, or where gh release view finds no release of that tag creates it with those files through gh release create --verify-tag, the tag nx release made from release.releaseTag.pattern or one made by hand, fails where gh release view fails for another reason, and uploads nothing where HEAD holds no tag of the project, a dry run previewing every project",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Attach, function* ({ dryRun, __unparsed__: words = [] }, context) {
    const [dry, { project, files }, tag] = yield* Effect.all([dried(dryRun), packaged(context), released(context)]);
    const commands = (at: string) => ({ upload: ['gh', 'release', 'upload', at, ...files, '--clobber', ...forwarded(words)], create: ['gh', 'release', 'create', at, '--verify-tag', ...files, ...forwarded(words)] }) as const;
    const attached = Effect.fnUntraced(function* (at: string) {
        const { upload, create } = commands(at);
        const { invoked, said, code } = yield* ran(['gh', 'release', 'view', at, '--json', 'tagName'], { cwd: context.root });
        const argv = yield* Match.value({ code, said: said.trim() }).pipe(
            Match.when({ code: 0 }, () => Effect.succeed(upload)),
            Match.when({ said: 'release not found' }, () => Effect.succeed(create)),
            Match.orElse(({ said: stated }) => Effect.fail(new Cause.UnknownError(code, `${invoked} exits ${code}: ${stated}`))),
        );
        return yield* spawned(argv, { cwd: context.root });
    });
    const previewed = (at: string) => `Would run ${CommandLine.display(commands(at).upload)} where ${at} has a GitHub release, else ${CommandLine.display(commands(at).create)}`;
    yield* dry ? Effect.logInfo(previewed(Option.getOrElse(tag, () => `<tag nx release makes for ${project}>`))) : Option.match(tag, { onNone: () => Effect.logInfo(`Skipped ${project}: HEAD holds no tag of ${project}, so nx release released no version of it at this commit`), onSome: attached });
    return { success: true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Attach, execute };
