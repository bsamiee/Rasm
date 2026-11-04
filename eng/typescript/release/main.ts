import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { readCachedProjectGraph } from '@nx/devkit';
import { Array, Cause, Console, Effect, Record, Result } from 'effect';
import { Command, Flag } from 'effect/cli';
import nx from 'nx/package.json' with { type: 'json' };
import { releaseChangelog, releaseVersion } from 'nx/release';
import { accumulate } from '#/eng/typescript/accumulate';
import { runCommand } from '#/eng/typescript/run-command';

// --- [COMPOSITION] ---------------------------------------------------------------------

Command.make(
    'release',
    { dryRun: Flag.Boolean('dry-run').pipe(Flag.withDefault(false)), specifier: Flag.String('specifier').pipe(Flag.optional) },
    Effect.fn('release')(function* ({ dryRun, specifier }) {
        yield* runCommand.records('\0', 'git', ['status', '--porcelain', '-z', '--no-renames']).pipe(Effect.filterOrFail(Array.isArrayEmpty, (paths) => new Cause.UnknownError(paths, `Uncommitted changes in ${paths.join(', ')}, commit before release`)));
        const { projectsVersionData, releaseGraph } = yield* Effect.tryPromise(() => releaseVersion({ dryRun, ...Record.getSomes({ specifier }) }));
        const [{ projectChangelogs }, { nodes }] = yield* Effect.all([Effect.tryPromise(() => releaseChangelog({ versionData: projectsVersionData, releaseGraph, dryRun })), Effect.try(readCachedProjectGraph)]);
        const published = Object.values(Record.filterMap(projectChangelogs ?? {}, ({ releaseVersion: released }, project) => (nodes[project]?.data.targets?.publish === undefined ? Result.failVoid : Result.succeed([`${project}:publish`, `--tag=${released.gitTag}`]))));
        yield* accumulate(published, (args) => (dryRun ? Console.log(`nx run ${args.join(' ')} [dry-run]`) : runCommand.shown('nx', ['run', ...args])));
    }),
).pipe(Command.run({ version: nx.version }), Effect.provide(layer), runMain);
