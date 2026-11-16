import { createConformanceRule } from '@nx/conformance';
import { readNxJson, readProjectsConfigurationFromProjectGraph } from '@nx/devkit';
import { getExecutorInformation, parseExecutor } from '@nx/devkit/internal';
import { Ajv } from 'ajv';
import { Array, Cause, Effect, Exit, Record, Result } from 'effect';
import settings from 'nx/schemas/nx-schema.json' with { type: 'json' };
import project from 'nx/schemas/project-schema.json' with { type: 'json' };
import { combineOptionsForExecutor } from 'nx/src/utils/params';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createConformanceRule({
    name: 'nx-schemas',
    category: 'reliability',
    description: "Accepts nx.json under Nx's settings schema, every resolved project under Nx's project schema, and the options of every target and configuration under its executor's schema",
    implementation: ({ tree, projectGraph }) => {
        const validator = new Ajv({ strict: false, allErrors: true });
        const [settled, configured] = [validator.compile(settings), validator.compile(project)];
        const { projects } = readProjectsConfigurationFromProjectGraph(projectGraph);
        const nodes = Object.values(projectGraph.nodes);
        const stated = settled(readNxJson(tree)) ? [] : [{ file: 'nx.json', message: `nx.json ${validator.errorsText(settled.errors)}` }];
        const resolved = Array.filterMap(nodes, ({ name, data }) => (configured(data) ? Result.failVoid : Result.succeed({ sourceProject: name, message: `${name} ${validator.errorsText(configured.errors)}` })));
        const cases = nodes.flatMap(({ name, data }) =>
            Record.collect(data.targets ?? {}, (target, configuration) => [{ project: name, label: `${name}:${target}`, configuration, named: '' }, ...Record.keys(configuration.configurations ?? {}).map((named) => ({ project: name, label: `${name}:${target}:${named}`, configuration, named }))]).flat(),
        );
        const optioned = Effect.forEach(cases, ({ project: owner, label, configuration, named }) =>
            Effect.exit(
                Effect.sync(() => {
                    combineOptionsForExecutor({}, named, configuration, getExecutorInformation(...parseExecutor(configuration.executor ?? 'nx:run-commands'), tree.root, projects).schema, null, null);
                }),
            ).pipe(Effect.map(Exit.match({ onFailure: (cause) => Cause.prettyErrors(cause).map(({ message }) => ({ sourceProject: owner, message: `${label} ${message}` })), onSuccess: () => [] }))),
        );
        return optioned.pipe(
            Effect.map((refused) => ({ severity: 'high' as const, details: { violations: [...stated, ...resolved, ...refused.flat()] } })),
            Effect.runPromise,
        );
    },
});
