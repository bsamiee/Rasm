import { isDeepStrictEqual } from 'node:util';
import { createConformanceRule } from '@nx/conformance';
import { type ProjectGraph, type ProjectGraphProjectNode, readProjectsConfigurationFromProjectGraph, type TargetConfiguration, type TargetDependencyConfig } from '@nx/devkit';
import { createTaskId, getExecutorInformation, normalizeDependencyConfigDefinition, parseExecutor } from '@nx/devkit/internal';
import { Ajv } from 'ajv';
import { Array, Formatter, Predicate, pipe, Record, Result, Struct, UndefinedOr } from 'effect';
import project from 'nx/schemas/project-schema.json' with { type: 'json' };
import { isGlobPattern } from 'nx/src/utils/globs';
import { combineOptionsForExecutor } from 'nx/src/utils/params';

// --- [OPERATIONS] ----------------------------------------------------------------------

const resolves = (graph: ProjectGraph, names: readonly string[], owner: string, entry: string | TargetDependencyConfig) => {
    const expanded = normalizeDependencyConfigDefinition(Predicate.isString(entry) ? entry : { ...entry }, owner, graph, [...names]);
    return isGlobPattern(Predicate.isString(entry) ? entry : entry.target) ? Array.isReadonlyArrayNonEmpty(expanded) : expanded.some(({ target, projects, dependencies = false }) => (dependencies ? names.includes(target) : projects.some((held) => target in (graph.nodes[held]?.data.targets ?? {}))));
};

const same = (stated: NonNullable<TargetConfiguration['inputs']>[number], read: NonNullable<TargetConfiguration['inputs']>[number]) => isDeepStrictEqual(stated, read) || (Predicate.isObject(read) && 'dependentTasksOutputFiles' in read && isDeepStrictEqual(stated, { ...read, transitive: true }));

const aggregated = ({ name, data }: ProjectGraphProjectNode, aggregate: string, separator: string) => {
    const tools = Record.filter(data.targets ?? {}, (_, target) => target.startsWith(`${aggregate}${separator}`));
    const read = Record.collect(tools, (tool, { inputs = [] }) => inputs.map((input) => [tool, input] as const)).flat();
    const forwarded = { target: `${aggregate}${separator}*`, params: 'forward' };
    return UndefinedOr.match(data.targets?.[aggregate], {
        onUndefined: () => (Record.isEmptyRecord(tools) ? [] : [`holds ${Record.keys(tools).join(', ')} and no ${aggregate} target, give it ${aggregate} over them`]),
        onDefined: ({ executor, dependsOn = [], inputs = [] }) => [
            ...(Record.isEmptyRecord(tools) ? [`${aggregate} runs no ${aggregate}${separator}<tool> target, drop ${aggregate}`] : []),
            ...(executor === 'nx:noop' && Array.containsWith(isDeepStrictEqual)([[forwarded], [{ ...forwarded, projects: [name] }]], dependsOn) ? [] : [`${aggregate} runs ${executor ?? 'a command'} after ${JSON.stringify(dependsOn)}, make it nx:noop depending on ${forwarded.target} with its parameters forwarded`]),
            ...read.filter(([, input]) => !inputs.some((own) => same(own, input))).map(([tool, input]) => `${aggregate} lacks input ${JSON.stringify(input)} of ${tool}, add it`),
            ...inputs.filter((own) => !read.some(([, input]) => same(own, input))).map((own) => `${aggregate} states input ${JSON.stringify(own)} no ${aggregate}${separator}<tool> target reads, drop it`),
        ],
    }).map((fault) => ({ sourceProject: name, message: `${name} ${fault}` }));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createConformanceRule<{ readonly separator: string }>({
    name: 'targets',
    category: 'consistency',
    description:
        "Holds every project under Nx's project schema and the options of every target and configuration under its executor's schema, resolves every dependsOn entry to a target, as Nx warns on each task holding an entry that reaches no task, and gives a project an nx:noop target `<name>` exactly where it holds `<name><separator><tool>` targets, depending on them alone with its parameters forwarded and stating exactly their inputs, with no other target depending on it, as Nx forwards the parameters of the run to its tool targets",
    implementation: ({ tree, projectGraph, ruleOptions: { separator } }) => {
        const validator = new Ajv({ strict: false, allErrors: true });
        const configured = validator.compile(project);
        const { projects } = readProjectsConfigurationFromProjectGraph(projectGraph);
        const nodes = Object.values(projectGraph.nodes);
        const declared = nodes.flatMap(({ name, data }) => Record.collect(data.targets ?? {}, (target, configuration) => ({ name, target, configuration })));
        const names = Array.dedupe(declared.map(Struct.get('target')));
        const aggregates = Array.dedupe(Array.filterMap(names, (target) => (target.includes(separator) ? Result.succeed(target.slice(0, target.indexOf(separator))) : Result.failVoid)));
        const entries = declared.flatMap(({ name, target, configuration }) => (configuration.dependsOn ?? []).map((entry) => ({ name, target, entry, depended: Predicate.isString(entry) ? entry.replace(/^\^|^.*:/u, '') : entry.target })));
        const violations = [
            ...nodes.filter(({ data }) => !configured(data)).map(({ name }) => ({ sourceProject: name, message: `${name} ${validator.errorsText(configured.errors)}` })),
            ...pipe(
                declared,
                Array.flatMap(({ name, target, configuration }) => ['', ...Record.keys(configuration.configurations ?? {})].map((named) => ({ name, label: createTaskId(name, target, named), configuration, named }))),
                Array.map(({ name, label, configuration, named }) => ({ name, label, checked: Result.try((): unknown => combineOptionsForExecutor({}, named, configuration, getExecutorInformation(...parseExecutor(configuration.executor ?? 'nx:run-commands'), tree.root, projects).schema, null, null)) })),
                Array.filterMap(({ name, label, checked }) => Result.map(Result.flip(checked), (cause) => ({ sourceProject: name, message: `${label} ${Error.isError(cause) ? cause.message : Formatter.format(cause)}` }))),
            ),
            ...entries.filter(({ name, entry }) => !resolves(projectGraph, names, name, entry)).map(({ name, target, entry }) => ({ sourceProject: name, message: `${name}:${target} dependsOn ${JSON.stringify(entry)} names no target, name an existing target or drop the entry` })),
            ...entries.filter(({ depended }) => aggregates.includes(depended)).map(({ name, target, depended }) => ({ sourceProject: name, message: `${name} ${target} depends on ${depended}, which forwards the parameters of the run to its tool targets, depend on ${depended}${separator}* in its place` })),
            ...Array.cartesian(nodes, aggregates).flatMap(([node, aggregate]) => aggregated(node, aggregate, separator)),
        ];
        return Promise.resolve({ severity: 'high' as const, details: { violations } });
    },
});
