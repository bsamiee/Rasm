import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { workspaceRoot } from '@nx/devkit';
import { Array, Effect, FileSystem, Filter, Option, Order, Path, Record, Schema } from 'effect';
import { Yaml } from 'effect/encoding';
import { findMatchingProjects } from 'nx/src/utils/find-matching-projects';
import { read, type Workspace } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

interface Told {
    readonly job: string;
    readonly labels: readonly string[];
    readonly gated: boolean;
    readonly listing: boolean;
    readonly targeted: boolean;
    readonly projects: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    covered:
        'README.md PRINCIPLES: CI job `linux` runs the affected targets of every project without tag `os:darwin`, job `macos` the `os:darwin` and `os:all` projects; CI job `unreal` runs the affected `os:linux` projects; nrwl/nx packages/nx/src/command-line/yargs-utils/shared-options.ts exclude: Exclude certain projects from being processed',
    listed: 'README.md PRINCIPLES: CI job `affected` lists the affected `os:linux` projects, and job `unreal` runs where it lists one; Listings select by project, never by target: an Unreal project holds no target on a runner outside its engine image; CI job `macos` starts on every run and its own `nx affected` selects what runs: no Linux graph holds an Apple project to list; nrwl/nx packages/nx/src/command-line/show/projects.ts: Grab only the nodes with the specified target',
} as const;
const COMMAND = /\bnx (?<command>affected|show projects --affected)(?<stated>[^\n]*)/gu;
const NAMED = /--(?<flag>exclude|projects)=(?<quote>'?)(?<patterns>[^'\s)"]+)\k<quote>/gu;
const TARGETED = /\s(?:--with-?[Tt]arget|-t)[=\s]/u;

// --- [MODELS] --------------------------------------------------------------------------

const Workflow = Schema.Struct({
    jobs: Schema.Record(Schema.String, Schema.Struct({ 'runs-on': Schema.Union([Schema.String, Schema.Array(Schema.String)]), if: Schema.optionalKey(Schema.String), steps: Schema.Array(Schema.Struct({ run: Schema.optionalKey(Schema.String) })) })),
}).annotate({ description: 'GitHub Actions workflow: each job with its runner labels, its condition, and the shell text of its run steps' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const selected = ({ nodes }: Workspace, every: readonly string[], stated: string) => {
    const named = Array.groupBy(
        Array.fromIterable(stated.matchAll(NAMED)).flatMap(({ groups }) => Array.fromNullishOr(groups)),
        ({ flag = '' }) => flag,
    );
    const matched = (flag: string) =>
        Option.map(Record.get(named, flag), (flags) =>
            findMatchingProjects(
                flags.flatMap(({ patterns = '' }) => patterns.split(',')),
                nodes,
            ),
        );
    return Array.sort(
        Array.difference(
            Option.getOrElse(matched('projects'), () => every),
            Option.getOrElse(matched('exclude'), Array.empty<string>),
        ),
        Order.String,
    );
};

const commands = Effect.fnUntraced(function* (workspace: Workspace) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { jobs } = yield* Effect.flatMap(fs.readFileString(path.join(workspaceRoot, '.github/workflows/ci.yml')), (text) => Schema.decodeUnknownEffect(Workflow)(Yaml.parse(text)));
    const every = Array.sort(Record.keys(workspace.nodes), Order.String);
    const told = Record.collect(jobs, (job, { 'runs-on': labels, if: condition, steps }) =>
        Array.fromIterable(
            steps
                .map(({ run = '' }) => run)
                .join('\n')
                .matchAll(COMMAND),
        )
            .flatMap(({ groups }) => Array.fromNullishOr(groups))
            .map(({ command, stated = '' }): Told => ({ job, labels: Array.ensure(labels), gated: condition?.includes('needs.') === true, listing: command !== 'affected', targeted: TARGETED.test(stated), projects: selected(workspace, every, stated) })),
    ).flat();
    const [listed, ran] = Array.partition(
        told,
        Filter.fromPredicate(({ listing }: Told) => listing),
    );
    return { every, ran, listed };
});

const twice = (ran: readonly Told[]) => Array.cartesian(ran, ran).flatMap(([first, second]) => (first.job < second.job && first.labels.join() === second.labels.join() ? Array.intersection(first.projects, second.projects).map((project) => `${project} in ${first.job} and ${second.job}`) : []));

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
        'gates a job on a listing only on a runner of the listing job, by a project listing of exactly the projects that job runs',
        Effect.fnUntraced(function* () {
            const { ran, listed } = yield* commands(yield* read('fresh'));
            const gated = ran.filter((job) => job.gated);
            assert.isNotEmpty(gated, SOURCES.listed);
            assert.deepStrictEqual(
                listed.filter(({ targeted }) => targeted).map(({ job }) => job),
                [],
                SOURCES.listed,
            );
            const listing = new Set(listed.flatMap(({ labels }) => labels));
            assert.deepStrictEqual(
                gated.filter(({ labels }) => listing.isDisjointFrom(new Set(labels))).map(({ job }) => job),
                [],
                SOURCES.listed,
            );
            assert.sameMembers(
                listed.map(({ projects }) => projects.join(' ')),
                gated.map(({ projects }) => projects.join(' ')),
                SOURCES.listed,
            );
        }),
    );
});
