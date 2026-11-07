import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Cause, Console, Effect, Record, Result, Schema } from 'effect';
import { Command, Flag } from 'effect/cli';
import nx from 'nx/package.json' with { type: 'json' };
import { releaseChangelog, releaseVersion } from 'nx/release';
import { runCommand } from '#/run-command.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Released = Schema.fromJsonString(Schema.Struct({ versions: Schema.Record(Schema.String, Schema.Struct({ currentVersion: Schema.NullOr(Schema.String), newVersion: Schema.NullOr(Schema.String) })), published: Schema.Array(Schema.String) }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const release = Command.make(
    'release',
    { apply: Flag.Boolean('apply').pipe(Flag.withDefault(false)), specifier: Flag.String('specifier').pipe(Flag.optional) },
    Effect.fn('release')(function* ({ apply, specifier }) {
        if (apply) {
            yield* runCommand.records('\0', 'git', ['status', '--porcelain', '-z', '--no-renames']).pipe(Effect.filterOrFail(Array.isArrayEmpty, (paths) => new Cause.UnknownError(paths, `Uncommitted changes in ${paths.join(', ')}, commit before release`)));
        }
        yield* runCommand.shown('git', ['fetch', '--tags']);
        const { projectsVersionData, releaseGraph } = yield* Effect.tryPromise(() => releaseVersion({ dryRun: !apply, ...Record.getSomes({ specifier }) }));
        const applying = apply && Object.values(projectsVersionData).some(({ newVersion }) => newVersion !== null);
        if (applying) {
            yield* runCommand.shown('git', ['push', 'origin', 'HEAD']);
        }
        const { projectChangelogs } = yield* Effect.tryPromise(() => releaseChangelog({ versionData: projectsVersionData, releaseGraph, dryRun: !apply, gitTag: false, gitPush: false }));
        if (applying) {
            yield* runCommand.shown('git', ['fetch', '--tags']);
        }
        const published = Object.values(Record.filterMap(projectChangelogs ?? {}, (_, project) => (releaseGraph.getVersionActionsForProject(project)?.projectGraphNode.data.targets?.publish === undefined ? Result.failVoid : Result.succeed(`${project}:publish`))));
        if (apply) {
            yield* Effect.validate(published, (target) => runCommand.shown('nx', ['run', target]), { discard: true });
        }
        yield* Effect.flatMap(Schema.encodeEffect(Released)({ versions: Record.map(projectsVersionData, ({ currentVersion, newVersion }) => ({ currentVersion, newVersion })), published }), Console.log);
    }),
);

if (import.meta.main) {
    Command.run(release, { version: nx.version }).pipe(Effect.provide(layer), runMain);
}
