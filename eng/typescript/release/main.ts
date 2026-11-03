import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { readCachedProjectGraph } from '@nx/devkit';
import { Array, Console, Effect, Option, Record, Result, Schema } from 'effect';
import { Command, Flag } from 'effect/cli';
import { releaseChangelog, releaseVersion } from 'nx/release';
import { records, run } from '#/eng/typescript/process';
import manifest from '#/package.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const STATUS = /^.{2} /u;

// --- [ERRORS] --------------------------------------------------------------------------

class UncommittedChanges extends Schema.TaggedError<UncommittedChanges, { readonly brand: unique symbol }>()('UncommittedChanges', { paths: Schema.NonEmptyArray(Schema.String) }) {
    override get message(): string {
        return `Uncommitted changes in ${this.paths.join(', ')}, commit before release`;
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

Command.make('release', { dryRun: Flag.Boolean('dry-run').pipe(Flag.withDefault(false)), specifier: Flag.String('specifier').pipe(Flag.optional) }, ({ dryRun, specifier }) =>
    Effect.gen(function* () {
        const changed = yield* records('\0', 'git', ['status', '--porcelain', '-z', '--no-renames']);
        yield* Array.match(changed, { onEmpty: () => Effect.void, onNonEmpty: (entries) => Effect.fail(UncommittedChanges.make({ paths: Array.map(entries, (entry) => entry.replace(STATUS, '')) })) });
        const { projectsVersionData, releaseGraph } = yield* Effect.tryPromise(() => releaseVersion({ dryRun, ...Option.match(specifier, { onNone: () => ({}), onSome: (value) => ({ specifier: value }) }) }));
        const [{ projectChangelogs }, { nodes }] = yield* Effect.all([Effect.tryPromise(() => releaseChangelog({ versionData: projectsVersionData, releaseGraph, dryRun })), Effect.try(readCachedProjectGraph)]);
        const published = Record.collect(projectChangelogs ?? {}, (project, { releaseVersion: released }) => (nodes[project]?.data.targets?.publish === undefined ? Result.failVoid : Result.succeed([`${project}:publish`, `--tag=${released.gitTag}`])));
        yield* Effect.validate(Array.getSuccesses(published), (args) => (dryRun ? Console.log(`nx run ${args.join(' ')} [dry-run]`) : run('nx', ['run', ...args], { stdout: 'inherit' })), { concurrency: 'unbounded', discard: true });
    }),
).pipe(Command.run({ version: manifest.version }), Effect.provide(NodeServices.layer), NodeRuntime.runMain);
