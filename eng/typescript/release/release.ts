import { readCachedProjectGraph } from '@nx/devkit';
import { Array, Cause, Console, Effect, Record, Result } from 'effect';
import { Command, Flag } from 'effect/cli';
import { releaseChangelog, releaseVersion } from 'nx/release';
import { accumulate } from '#/eng/typescript/accumulate';
import { runCommand } from '#/eng/typescript/run-command';

// --- [COMPOSITION] ---------------------------------------------------------------------

const release = Command.make(
    'release',
    { dryRun: Flag.Boolean('dry-run').pipe(Flag.withDefault(false)), specifier: Flag.String('specifier').pipe(Flag.optional) },
    Effect.fn('release')(function* ({ dryRun, specifier }) {
        yield* runCommand.records('\0', 'git', ['status', '--porcelain', '-z', '--no-renames']).pipe(Effect.filterOrFail(Array.isArrayEmpty, (paths) => new Cause.UnknownError(paths, `Uncommitted changes in ${paths.join(', ')}, commit before release`)));
        yield* runCommand.shown('git', ['fetch', '--tags']);
        const { projectsVersionData, releaseGraph } = yield* Effect.tryPromise(() => releaseVersion({ dryRun, ...Record.getSomes({ specifier }) }));
        const applying = !dryRun && Object.values(projectsVersionData).some(({ newVersion }) => newVersion !== null);
        if (applying) {
            yield* runCommand.shown('git', ['push', 'origin', 'HEAD']);
        }
        const [{ projectChangelogs }, { nodes }] = yield* Effect.all([Effect.tryPromise(() => releaseChangelog({ versionData: projectsVersionData, releaseGraph, dryRun, gitTag: false, gitPush: false })), Effect.try(readCachedProjectGraph)]);
        if (applying) {
            yield* runCommand.shown('git', ['fetch', '--tags']);
        }
        const published = Object.values(Record.filterMap(projectChangelogs ?? {}, ({ releaseVersion: released }, project) => (nodes[project]?.data.targets?.publish === undefined ? Result.failVoid : Result.succeed([`${project}:publish`, `--tag=${released.gitTag}`]))));
        yield* accumulate(published, (args) => (dryRun ? Console.log(`nx run ${args.join(' ')} [dry-run]`) : runCommand.shown('nx', ['run', ...args])));
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { release };
