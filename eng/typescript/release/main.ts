import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { readCachedProjectGraph } from '@nx/devkit';
import { Array, Console, Effect, Option, Schema, String } from 'effect';
import { Command, Flag } from 'effect/cli';
import nx from 'nx/package.json' with { type: 'json' };
import { releaseChangelog, releaseVersion } from 'nx/release';
import { stdout } from '#/eng/typescript/process';

// --- [ERRORS] --------------------------------------------------------------------------

class UncommittedChanges extends Schema.TaggedError<UncommittedChanges, { readonly brand: unique symbol }>()('UncommittedChanges', { paths: Schema.NonEmptyArray(Schema.String) }) {
    override get message(): string {
        return `Release packs the tagged commit while ${this.paths.join(', ')} hold uncommitted changes`;
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

Command.make('release', { dryRun: Flag.Boolean('dry-run').pipe(Flag.withDefault(false)), specifier: Flag.String('specifier').pipe(Flag.optional) }, ({ dryRun, specifier }) =>
    Effect.gen(function* () {
        const changed = (yield* stdout('git', ['status', '--porcelain'])).split('\n').filter(String.isNonEmpty);
        yield* Array.match(changed, { onEmpty: () => Effect.void, onNonEmpty: (paths) => Effect.fail(UncommittedChanges.make({ paths })) });
        const { projectsVersionData, releaseGraph } = yield* Effect.tryPromise(() => releaseVersion({ dryRun, ...Option.match(specifier, { onNone: () => ({}), onSome: (value) => ({ specifier: value }) }) }));
        const [{ projectChangelogs }, { nodes }] = yield* Effect.all([Effect.tryPromise(() => releaseChangelog({ versionData: projectsVersionData, releaseGraph, dryRun })), Effect.try(readCachedProjectGraph)]);
        const published = Object.entries(projectChangelogs ?? {}).flatMap(([project, { releaseVersion: released }]) => (nodes[project]?.data.targets?.publish === undefined ? [] : [[`${project}:publish`, `--tag=${released.gitTag}`]]));
        yield* Effect.forEach(published, (args) => (dryRun ? Console.log(`nx run ${args.join(' ')} [dry-run]`) : stdout('nx', ['run', ...args], { stdout: 'inherit' })), { discard: true });
    }),
).pipe(Command.run({ version: nx.version }), Effect.provide(layer), runMain);
