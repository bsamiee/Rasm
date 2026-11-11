import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { Array, Cause, Effect, Layer, Predicate, Record, Schema } from 'effect';
import { findMatchingProjects } from 'nx/src/devkit-internals';
import { createTaskGraph } from 'nx/src/tasks-runner/create-task-graph';
import { assertTaskGraphDoesNotContainInvalidTargets, findCycle } from 'nx/src/tasks-runner/task-graph-utils';
import { expandDependencyConfigSyntaxSugar } from 'nx/src/tasks-runner/utils';
import { Composed, SLOW, type Stated } from '#/workspace/graph';

// --- [CONSTANTS] -----------------------------------------------------------------------

const NX = /(?:^|[\s;&|(])(?:npx\s+|pnpm\s+(?:exec\s+|dlx\s+)?|yarn\s+|bunx\s+|(?:\.\/)?node_modules\/\.bin\/)?nx(?:\s|$)/u;
const GLOB = /[*?!{[(]/u;
const LEGACY = ['self', 'dependencies'];

// --- [MODELS] --------------------------------------------------------------------------

const Commanded = Schema.Struct({ command: Schema.String });
const Ran = Schema.Struct({ command: Schema.optionalKey(Schema.String), commands: Schema.optionalKey(Schema.Array(Schema.Union([Schema.String, Commanded]))) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const planned = (projectGraph: ProjectGraph, target: string) => {
    const tasks = createTaskGraph(projectGraph, {}, Record.keys(Record.filter(projectGraph.nodes, (node) => node.data.targets?.[target] !== undefined)), [target], undefined, {});
    assertTaskGraphDoesNotContainInvalidTargets(tasks);
    return findCycle(tasks);
};

const depended = ({ node, configuration }: Stated, projectGraph: ProjectGraph) => (configuration.dependsOn ?? []).map((entry) => expandDependencyConfigSyntaxSugar(entry, projectGraph, node.name));

const absent = (stated: Stated, projectGraph: ProjectGraph) =>
    depended(stated, projectGraph)
        .filter(({ target, projects, dependencies }) => projects === undefined && dependencies !== true && !GLOB.test(target) && stated.node.data.targets?.[target] === undefined)
        .map(({ target }) => `${stated.task} depends on ${target}`);

const unmatched = (stated: Stated, projectGraph: ProjectGraph) =>
    depended(stated, projectGraph)
        .map(({ projects = [] }) => Array.difference(Array.ensure(projects), LEGACY))
        .filter((patterns) => Array.isReadonlyArrayNonEmpty(patterns) && findMatchingProjects(patterns, projectGraph.nodes).length === 0)
        .map((patterns) => `${stated.task} depends on projects ${patterns.join(', ')}`);

const lines = (options: unknown) => Array.flatMap(Array.fromOption(Schema.decodeUnknownOption(Ran)(options)), ({ command, commands = [] }) => [...Array.fromNullishOr(command), ...commands.map((entry) => (Predicate.isString(entry) ? entry : entry.command))]);

const nested = ({ task, configuration }: Stated) =>
    lines(configuration.options)
        .filter((line) => NX.test(line))
        .map((line) => `${task}: ${line}`);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Layer.merge(layer, Composed.layer), { excludeTestServices: true, timeout: SLOW.timeout })('task graph', (methods) => {
    methods.effect(
        'builds for every target name a task graph without a cycle and without a task Nx refuses beside a continuous dependency',
        Effect.fnUntraced(function* () {
            const { projectGraph, targets } = yield* Composed;
            const [cycles, refused] = yield* Effect.partition(Array.dedupe(targets.map(({ target }) => target)), (target) => Effect.try({ try: () => planned(projectGraph, target), catch: (cause) => new Cause.UnknownError(cause, target) }));
            assert.deepStrictEqual([refused, cycles.filter(Predicate.isNotNull)], [[], []], 'nrwl/nx@2a424d3 packages/nx/src/tasks-runner/task-graph-utils.ts findCycle and assertTaskGraphDoesNotContainInvalidTargets, create-task-graph.spec.ts');
        }),
        SLOW,
    );

    methods.effect(
        'names in every dependsOn entry on the same project a target that project holds',
        Effect.fnUntraced(function* () {
            const { projectGraph, targets } = yield* Composed;
            assert.deepStrictEqual(
                targets.flatMap((stated) => absent(stated, projectGraph)),
                [],
                'nrwl/nx@2a424d3 packages/nx/src/tasks-runner/create-task-graph.ts:222 projectHasTarget',
            );
        }),
        SLOW,
    );

    methods.effect(
        'matches at least one project with every dependsOn projects pattern',
        Effect.fnUntraced(function* () {
            const { projectGraph, targets } = yield* Composed;
            assert.deepStrictEqual(
                targets.flatMap((stated) => unmatched(stated, projectGraph)),
                [],
                'nrwl/nx@2a424d3 packages/nx/src/tasks-runner/create-task-graph.ts:189 dependsOn.projects matching no project',
            );
        }),
        SLOW,
    );

    methods.effect(
        'runs nx from no target command',
        Effect.fnUntraced(function* () {
            const { targets } = yield* Composed;
            assert.deepStrictEqual(targets.flatMap(nested), [], 'Evanion/libraries@30e00ee tools/repo-checks/src/task-invokes-nx.test.ts, nrwl/nx@2a424d3 packages/nx/src/tasks-runner/task-orchestrator.ts:673 "Recursive task invocation detected"');
        }),
        SLOW,
    );
});
