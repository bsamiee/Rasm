import { Effect, Option, Record, Schema, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import { Dry, forwarded, packaged, released } from '#/executor';
import { dried, executor, spawned, Words } from '#/inference';

// --- [CONSTANTS] -----------------------------------------------------------------------

const QUERY = 'query($owner: String!, $name: String!, $tag: String!) { repository(owner: $owner, name: $name) { release(tagName: $tag) { tagName } } }';

// --- [MODELS] --------------------------------------------------------------------------

const Held = Schema.fromJsonString(Schema.Struct({ data: Schema.Struct({ repository: Schema.Struct({ release: Schema.NullOr(Schema.Struct({ tagName: Schema.String })) }) }) })).annotate({
    description: "The release GitHub's GraphQL API holds for one tag of the repository gh resolves from the working directory, null where the tag has none",
});
const Attach = Dry.mapFields(Struct.assign({ __unparsed__: Words })).annotate({
    title: 'Attach',
    description:
        "Uploads the files a project's packaging targets wrote to the GitHub release of the project's tag at the checked-out commit through gh release upload --clobber, or where GitHub holds no release of that tag, which gh api graphql reads, creates it with those files through gh release create --verify-tag, the tag nx release made from release.releaseTag.pattern or one made by hand, fails where that read fails, and uploads nothing where HEAD holds no tag of the project, a dry run previewing every project",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Attach, function* ({ dryRun, __unparsed__: words = [] }, context) {
    const [dry, { files }, tag] = yield* Effect.all([dried(dryRun), packaged(context), Effect.map(released(context), Option.map(Struct.get('tag')))]);
    const commands = (at: string) => ({ upload: ['gh', 'release', 'upload', at, ...files, '--clobber', ...forwarded(words)], create: ['gh', 'release', 'create', at, '--verify-tag', ...files, ...forwarded(words)] }) as const;
    const attached = Effect.fnUntraced(function* (at: string) {
        const { data } = yield* Effect.flatMap(spawned(['gh', 'api', 'graphql', '-F', 'owner={owner}', '-F', 'name={repo}', '-f', `tag=${at}`, '-f', `query=${QUERY}`], { cwd: context.root }), Schema.decodeEffect(Held));
        return yield* spawned(data.repository.release === null ? commands(at).create : commands(at).upload, { cwd: context.root });
    });
    yield* dry ? Effect.annotateLogs(Effect.logInfo('skipped'), { ...Record.map(commands(Option.getOrElse(tag, () => '<tag>')), (argv) => CommandLine.display(argv)), dry }) : tag.pipe(Option.map(attached), Effect.transposeOption);
    return { success: true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Attach, execute };
