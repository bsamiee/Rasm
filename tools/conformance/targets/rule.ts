import { isDeepStrictEqual } from 'node:util';
import { createConformanceRule } from '@nx/conformance';
import { type ProjectGraph, readProjectsConfigurationFromProjectGraph, type TargetConfiguration, type TargetDependencyConfig } from '@nx/devkit';
import { getExecutorInformation, normalizeDependencyConfigDefinition, parseExecutor } from '@nx/devkit/internal';
import { Ajv } from 'ajv';
import { Array, Formatter, Option, Predicate, Record, Result, Struct } from 'effect';
import project from 'nx/schemas/project-schema.json' with { type: 'json' };
import { isGlobPattern } from 'nx/src/utils/globs';
import { combineOptionsForExecutor } from 'nx/src/utils/params';

// --- [OPERATIONS] ----------------------------------------------------------------------

const resolves = (graph: ProjectGraph, names: readonly string[], owner: string, entry: string | TargetDependencyConfig) => {
    const expanded = normalizeDependencyConfigDefinition(Predicate.isString(entry) ? entry : { ...entry }, owner, graph, [...names]);
    const patterned = !Predicate.isString(entry) && entry.projects !== undefined && Array.ensure(entry.projects).every((pattern) => isGlobPattern(pattern));
    return isGlobPattern(Predicate.isString(entry) ? entry : entry.target) ? Array.isReadonlyArrayNonEmpty(expanded) : expanded.some(({ target, projects, dependencies = false }) => dependencies || patterned || projects.some((held) => target in (graph.nodes[held]?.data.targets ?? {})));
};

const same = (stated: NonNullable<TargetConfiguration['inputs']>[number], read: NonNullable<TargetConfiguration['inputs']>[number]) => isDeepStrictEqual(stated, read) || (Predicate.isObject(read) && 'dependentTasksOutputFiles' in read && isDeepStrictEqual(stated, { ...read, transitive: true }));

const aggregated = (name: string, aggregate: string, separator: string, held: TargetConfiguration | undefined, tools: Record.ReadonlyRecord<string, TargetConfiguration>) => {
    const read = Record.collect(tools, (tool, { inputs = [] }) => inputs.map((input) => [tool, input] as const)).flat();
    return Option.match(Option.fromNullishOr(held), {
        onNone: () => (Record.isEmptyRecord(tools) ? [] : [`holds ${Record.keys(tools).join(', ')} and no ${aggregate} target, give it ${aggregate} over them`]),
        onSome: ({ executor, dependsOn = [], inputs = [] }) => [
            ...(Record.isEmptyRecord(tools) ? [`${aggregate} runs no ${aggregate}${separator}<tool> target, drop ${aggregate}`] : []),
            ...(executor === 'nx:noop' &&
            isDeepStrictEqual(
                dependsOn.map((entry) => (Predicate.isObject(entry) && isDeepStrictEqual(entry.projects, [name]) ? Struct.omit(entry, ['projects']) : entry)),
                [{ target: `${aggregate}${separator}*`, params: 'forward' }],
            )
                ? []
                : [`${aggregate} runs ${executor ?? 'a command'} after ${JSON.stringify(dependsOn)}, make it nx:noop depending on ${aggregate}${separator}* with its parameters forwarded`]),
            ...read.filter(([, input]) => !inputs.some((own) => same(own, input))).map(([tool, input]) => `${aggregate} lacks input ${JSON.stringify(input)} of ${tool}, add it`),
            ...inputs.filter((own) => !read.some(([, input]) => same(own, input))).map((own) => `${aggregate} states input ${JSON.stringify(own)} no ${aggregate}${separator}<tool> target reads, drop it`),
        ],
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createConformanceRule<{ readonly separator: string }>({
    name: 'targets',
    category: 'consistency',
    description:
        "Holds every project under Nx's project schema and the options of every target and configuration under its executor's schema, resolves every dependsOn entry to a target, an exact ^ name or project glob aside, as Nx runs one on the projects it reaches holding its target and on none where that target exists on another platform alone, and gives a project an nx:noop target `<name>` exactly where it holds `<name><separator><tool>` targets, depending on them alone with its parameters forwarded and stating exactly their inputs, with no other target depending on it, as Nx forwards the parameters of the run to its tool targets",
    implementation: ({ tree, projectGraph, ruleOptions: { separator } }) => {
        const validator = new Ajv({ strict: false, allErrors: true });
        const configured = validator.compile(project);
        const { projects } = readProjectsConfigurationFromProjectGraph(projectGraph);
        const nodes = Object.values(projectGraph.nodes);
        const declared = nodes.flatMap(({ name, data }) => Record.collect(data.targets ?? {}, (target, configuration) => ({ name, target, configuration, dependsOn: configuration.dependsOn ?? [] })));
        const names = Array.dedupe(declared.map(Struct.get('target')));
        const aggregates = Array.dedupe(Array.filterMap(names, (target) => (target.includes(separator) ? Result.succeed(target.slice(0, target.indexOf(separator))) : Result.failVoid)));
        const violations = [
            ...nodes.filter(({ data }) => !configured(data)).map(({ name }) => ({ sourceProject: name, message: `${name} ${validator.errorsText(configured.errors)}` })),
            ...declared.flatMap(({ name, target, configuration }) =>
                [undefined, ...Record.keys(configuration.configurations ?? {})].flatMap((named) =>
                    Result.match(
                        Result.try(() => {
                            combineOptionsForExecutor({}, named ?? '', configuration, getExecutorInformation(...parseExecutor(configuration.executor ?? 'nx:run-commands'), tree.root, projects).schema, null, null);
                        }),
                        { onFailure: (cause) => [{ sourceProject: name, message: `${[name, target, ...Array.fromNullishOr(named)].join(':')} ${Predicate.isError(cause) ? cause.message : Formatter.format(cause)}` }], onSuccess: () => [] },
                    ),
                ),
            ),
            ...declared.flatMap(({ name, target, dependsOn }) => dependsOn.filter((entry) => !resolves(projectGraph, names, name, entry)).map((entry) => ({ sourceProject: name, message: `${name}:${target} dependsOn ${JSON.stringify(entry)} names no target, name an existing target or drop the entry` }))),
            ...declared.flatMap(({ name, target, dependsOn }) =>
                Array.intersection(
                    dependsOn.map((entry) => (Predicate.isString(entry) ? entry.replace(/^\^|^.*:/u, '') : entry.target)),
                    aggregates,
                ).map((aggregate) => ({ sourceProject: name, message: `${name} ${target} depends on ${aggregate}, which forwards the parameters of the run to its tool targets, depend on ${aggregate}${separator}* in its place` })),
            ),
            ...nodes.flatMap(({ name, data: { targets = {} } }) =>
                aggregates.flatMap((aggregate) =>
                    aggregated(
                        name,
                        aggregate,
                        separator,
                        targets[aggregate],
                        Record.filter(targets, (_, target) => target.startsWith(`${aggregate}${separator}`)),
                    ).map((fault) => ({ sourceProject: name, message: `${name} ${fault}` })),
                ),
            ),
        ];
        return Promise.resolve({ severity: 'high' as const, details: { violations } });
    },
});
