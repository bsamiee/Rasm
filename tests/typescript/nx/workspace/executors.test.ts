import { inspect } from 'node:util';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { type ProjectConfiguration, readProjectsConfigurationFromProjectGraph, workspaceRoot } from '@nx/devkit';
import { Cause, Effect, Layer, Record } from 'effect';
import { getExecutorInformation, parseExecutor } from 'nx/src/devkit-internals';
import { combineOptionsForExecutor } from 'nx/src/utils/params';
import { Composed, SLOW, type Stated } from '#/workspace/graph';

// --- [OPERATIONS] ----------------------------------------------------------------------

const accepted = ({ node, configuration }: Stated, projects: Record<string, ProjectConfiguration>) => {
    const [module, name] = parseExecutor(configuration.executor ?? 'nx:run-commands');
    const { schema, implementationFactory } = getExecutorInformation(module, name, workspaceRoot, projects);
    implementationFactory();
    ['', ...Record.keys(configuration.configurations ?? {})].forEach((selected) => {
        combineOptionsForExecutor({}, selected, configuration, schema, node.name, null);
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Layer.merge(layer, Composed.layer), { excludeTestServices: true, timeout: SLOW.timeout })('executors', (methods) => {
    methods.effect(
        'resolves the schema and implementation of every executor a target names, and gives it options and configurations its schema accepts',
        Effect.fnUntraced(function* () {
            const { projectGraph, targets } = yield* Composed;
            const { projects } = readProjectsConfigurationFromProjectGraph(projectGraph);
            const [, refused] = yield* Effect.partition(targets, (target) => Effect.try({ try: () => accepted(target, projects), catch: (cause) => new Cause.UnknownError(cause, `${target.task}: ${inspect(cause)}`) }));
            assert.deepStrictEqual(
                refused.map(({ message }) => message),
                [],
                'nrwl/nx@2a424d3 packages/nx/src/config/schema-utils.ts SchemaResolutionError and ImplementationResolutionError, packages/eslint-plugin/src/rules/nx-plugin-checks.ts',
            );
        }),
        SLOW,
    );
});
