import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { workspaceRoot } from '@nx/devkit';
import { Array, Effect, FileSystem, Order, Path, Record, Schema } from 'effect';
import { Yaml } from 'effect/encoding';
import { findMatchingProjects } from 'nx/src/utils/find-matching-projects';
import { read, type Workspace } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

interface Ran {
    readonly job: string;
    readonly runner: string;
    readonly gated: boolean;
    readonly projects: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    covered:
        'README.md PRINCIPLES: CI job `linux` runs the affected targets of every project without tag `os:darwin`, job `macos` the `os:darwin` and `os:all` projects; CI job `unreal` runs the affected `os:linux` projects; nrwl/nx packages/nx/src/command-line/yargs-utils/shared-options.ts exclude: Exclude certain projects from being processed',
    listed: 'README.md PRINCIPLES: CI job `affected` lists the affected `os:darwin` and `os:all` projects and Unreal engines, and jobs `macos` and `unreal` run where it lists one; nrwl/nx packages/nx/src/command-line/show/command-object.ts projects: Show only projects that match a given pattern',
} as const;
const COMMAND = /\bnx (?<command>affected|show projects --affected)(?<stated>[^\n]*)/gu;
const NAMED = /--(?:exclude|projects)=(?<quote>'?)(?<patterns>[^'\s)"]+)\k<quote>/u;

// --- [MODELS] --------------------------------------------------------------------------

const Workflow = Schema.Struct({
    jobs: Schema.Record(Schema.String, Schema.Struct({ 'runs-on': Schema.Union([Schema.String, Schema.Array(Schema.String)]), if: Schema.optionalKey(Schema.String), steps: Schema.Array(Schema.Struct({ run: Schema.optionalKey(Schema.String) })) })),
}).annotate({ description: 'GitHub Actions workflow: each job with its runner labels, its condition, and the shell text of its run steps' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const matched = ({ nodes }: Workspace, stated: string | undefined) => {
    const named = Array.fromNullishOr(stated)
        .flatMap((text) => Array.fromNullishOr(NAMED.exec(text)?.groups))
        .flatMap(({ patterns }) => Array.fromNullishOr(patterns));
    return Array.sort(
        findMatchingProjects(
            named.flatMap((patterns) => patterns.split(',')),
            nodes,
        ),
        Order.String,
    );
};

const commands = Effect.fnUntraced(function* (workspace: Workspace) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { jobs } = yield* Effect.flatMap(fs.readFileString(path.join(workspaceRoot, '.github/workflows/ci.yml')), (text) => Schema.decodeUnknownEffect(Workflow)(Yaml.parse(text)));
    const every = Array.sort(Record.keys(workspace.nodes), Order.String);
    const stated = Record.collect(jobs, (job, { 'runs-on': labels, if: condition, steps }) => ({
        job,
        runner: Array.ensure(labels).join(' '),
        gated: condition?.includes('needs.') === true,
        found: Array.fromIterable(
            steps
                .map(({ run = '' }) => run)
                .join('\n')
                .matchAll(COMMAND),
        ),
    }));
    const told = stated.flatMap(({ found, ...job }) => found.flatMap(({ groups }) => Array.fromNullishOr(groups)).map(({ command, stated: named }) => ({ ...job, command, projects: matched(workspace, named) })));
    return {
        every,
        ran: told.filter(({ command }) => command === 'affected').map(({ projects, ...job }): Ran => ({ ...job, projects: Array.difference(every, projects) })),
        listed: told.filter(({ command }) => command !== 'affected').map(({ projects }) => projects.join(' ')),
    };
});

const twice = (ran: readonly Ran[]) => Array.cartesian(ran, ran).flatMap(([first, second]) => (first.job < second.job && first.runner === second.runner ? Array.intersection(first.projects, second.projects).map((project) => `${project} in ${first.job} and ${second.job}`) : []));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })((suite) => {
    suite.effect(
        'runs every project in a job of ci.yml, and none in two jobs on one runner',
        Effect.fnUntraced(function* () {
            const { every, ran } = yield* commands(yield* read('fresh'));
            assert.isNotEmpty(ran, SOURCES.covered);
            assert.deepStrictEqual(
                Array.difference(
                    every,
                    ran.flatMap(({ projects }) => projects),
                ),
                [],
                SOURCES.covered,
            );
            assert.deepStrictEqual(twice(ran), [], SOURCES.covered);
        }),
    );

    suite.effect(
        'gates each job that waits on the affected listing on a listing of exactly the projects that job runs',
        Effect.fnUntraced(function* () {
            const { ran, listed } = yield* commands(yield* read('fresh'));
            const gated = ran.filter((job) => job.gated).map(({ projects }) => projects.join(' '));
            assert.isNotEmpty(gated, SOURCES.listed);
            assert.sameMembers(listed, gated, SOURCES.listed);
        }),
    );
});
