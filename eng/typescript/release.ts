#!/usr/bin/env node
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { createProjectGraphAsync } from '@nx/devkit';
import { findMatchingProjects, readNxJsonFromDisk } from '@nx/devkit/internal';
import { Array, Cause, Console, Effect, Record, Result, Schema, String } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import nx from 'nx/package.json' with { type: 'json' };
import { releaseChangelog, releaseVersion } from 'nx/release';
import { parseConventionalCommitsMessage } from 'nx/src/command-line/release/utils/git';
import { accumulate, runCommand } from '#/run-command.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Released = Schema.fromJsonString(Schema.Struct({ versions: Schema.Record(Schema.String, Schema.Struct({ currentVersion: Schema.NullOr(Schema.String), newVersion: Schema.NullOr(Schema.String) })), published: Schema.Array(Schema.String) }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const release = Command.make(
    'rasm-release',
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
            yield* accumulate(published, (target) => runCommand.shown('nx', ['run', target]));
        }
        yield* Effect.flatMap(Schema.encodeEffect(Released)({ versions: Record.map(projectsVersionData, ({ currentVersion, newVersion }) => ({ currentVersion, newVersion })), published }), Console.log);
    }),
);

const scope = Command.make(
    'scope',
    { message: Argument.FileText('message') },
    Effect.fn('scope')(function* ({ message }) {
        const [graph, { release: config }] = yield* Effect.all([Effect.tryPromise(() => createProjectGraphAsync()), Effect.try(readNxJsonFromDisk)]);
        const patterns = (parseConventionalCommitsMessage(Array.headNonEmpty(String.split(message, '\n')))?.scope.split(',') ?? []).map(String.trim).filter(String.isNonEmpty);
        const ambiguous = Record.collect(config?.groups ?? {}, (group, { projects }) => {
            const members = findMatchingProjects(Array.ensure(projects), graph.nodes);
            return patterns.map((pattern) => ({ group, pattern, matches: Array.intersection(findMatchingProjects([pattern], graph.nodes), members) }));
        }).flat();
        yield* Effect.succeed(ambiguous.filter(({ matches }) => matches.length > 1)).pipe(
            Effect.filterOrFail(Array.isArrayEmpty, (found) => new Cause.UnknownError(found, found.map(({ group, pattern, matches }) => `Scope "${pattern}" matches ${matches.join(', ')} in release group ${group}, scope the commit with one of these project names`).join('\n'))),
        );
    }),
);

if (import.meta.main) {
    release.pipe(Command.withSubcommands([scope]), Command.run({ version: nx.version }), Effect.provide(layer), runMain);
}
