import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { readCachedProjectGraph } from '@nx/devkit';
import { Array, Cause, Console, Effect, Option, Record, Result, Schema } from 'effect';
import { Command, Flag } from 'effect/cli';
import nx from 'nx/package.json' with { type: 'json' };
import { releaseChangelog, releaseVersion } from 'nx/release';
import { accumulate } from '#/eng/typescript/accumulate';
import { records, run, SHOWN } from '#/eng/typescript/process';

// --- [ERRORS] --------------------------------------------------------------------------

class UncommittedChanges extends Schema.TaggedError<UncommittedChanges, { readonly brand: unique symbol }>()('UncommittedChanges', { paths: Schema.NonEmptyArray(Schema.String) }) {
    override get message(): string {
        return `Uncommitted changes in ${this.paths.join(', ')}, commit before release`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const failed = (step: string) => (cause: unknown) => new Cause.UnknownError(cause, `${step} failed: ${String(cause)}`);

// --- [COMPOSITION] ---------------------------------------------------------------------

Command.make('release', { dryRun: Flag.Boolean('dry-run').pipe(Flag.withDefault(false)), specifier: Flag.String('specifier').pipe(Flag.optional) }, ({ dryRun, specifier }) =>
    Effect.gen(function* () {
        const changed = yield* records('\0', 'git', ['status', '--porcelain', '-z', '--no-renames']);
        yield* Array.match(changed, { onEmpty: () => Effect.void, onNonEmpty: (entries) => Effect.fail(UncommittedChanges.make({ paths: Array.map(entries, (entry) => entry.slice(3)) })) });
        const { projectsVersionData, releaseGraph } = yield* Effect.tryPromise({ try: () => releaseVersion({ dryRun, ...Option.match(specifier, { onNone: () => ({}), onSome: (value) => ({ specifier: value }) }) }), catch: failed('nx release version') });
        const [{ projectChangelogs }, { nodes }] = yield* Effect.all([Effect.tryPromise({ try: () => releaseChangelog({ versionData: projectsVersionData, releaseGraph, dryRun }), catch: failed('nx release changelog') }), Effect.try(readCachedProjectGraph)]);
        const published = Record.values(Record.filterMap(projectChangelogs ?? {}, ({ releaseVersion: released }, project) => (nodes[project]?.data.targets?.publish === undefined ? Result.failVoid : Result.succeed([`${project}:publish`, `--tag=${released.gitTag}`]))));
        yield* accumulate(published, (args) => (dryRun ? Console.log(`nx run ${args.join(' ')} [dry-run]`) : run('nx', ['run', ...args], SHOWN)));
    }),
).pipe(Command.run({ version: nx.version }), Effect.provide(NodeServices.layer), NodeRuntime.runMain);
