import { isDeepStrictEqual } from 'node:util';
import { createConformanceRule } from '@nx/conformance';
import type { TargetConfiguration } from '@nx/devkit';
import { Array, Option, Predicate, Record, Struct } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const stated = (held: NonNullable<TargetConfiguration['inputs']>, input: NonNullable<TargetConfiguration['inputs']>[number]) => held.some((other) => isDeepStrictEqual(other, input) || (Predicate.isObject(input) && 'dependentTasksOutputFiles' in input && isDeepStrictEqual(other, { ...input, transitive: true })));

const faults = (name: string, aggregate: string, separator: string, held: TargetConfiguration | undefined, tools: Record.ReadonlyRecord<string, TargetConfiguration>) => {
    const named = Record.keys(tools);
    return [
        ...(held === undefined && Array.isReadonlyArrayNonEmpty(named) ? [`holds ${named.join(', ')} and no ${aggregate} target, give it ${aggregate} over them`] : []),
        ...(held !== undefined && Array.isReadonlyArrayEmpty(named) ? [`${aggregate} runs no ${aggregate}${separator}<tool> target, drop ${aggregate}`] : []),
        ...Option.toArray(Option.fromNullishOr(held)).flatMap(({ executor, dependsOn = [], inputs = [] }) => [
            ...(executor === 'nx:noop' &&
            isDeepStrictEqual(
                dependsOn.map((entry) => (Predicate.isObject(entry) && isDeepStrictEqual(entry.projects, [name]) ? Struct.omit(entry, ['projects']) : entry)),
                [{ target: `${aggregate}${separator}*`, params: 'forward' }],
            )
                ? []
                : [`${aggregate} runs ${executor ?? 'a command'} after ${JSON.stringify(dependsOn)}, make it nx:noop depending on ${aggregate}${separator}* with its parameters forwarded`]),
            ...Record.collect(tools, (tool, tooled) => (tooled.inputs ?? []).filter((input) => !stated(inputs, input)).map((input) => `${aggregate} lacks input ${JSON.stringify(input)} of ${tool}, add it`)).flat(),
        ]),
    ];
};

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createConformanceRule<{ readonly separator: string }>({
    name: 'aggregate-targets',
    category: 'consistency',
    description:
        'Gives a project target `<name>` exactly where it holds a `<name><separator><tool>` target, as an nx:noop depending on those targets alone with its parameters forwarded and stating each of their inputs, so nx affected selects it by them, and keeps every other target from depending on it, as Nx forwards the parameters of the run, not of the dependent, to its tool targets',
    implementation: ({ projectGraph: { nodes }, ruleOptions: { separator } }) => {
        const aggregates = Array.dedupe(
            Object.values(nodes).flatMap(({ data }) =>
                Record.keys(data.targets ?? {})
                    .filter((target) => target.includes(separator))
                    .map((target) => target.slice(0, target.indexOf(separator))),
            ),
        );
        const violations = Object.values(nodes).flatMap(({ name, data: { targets = {} } }) =>
            [
                ...aggregates.flatMap((aggregate) =>
                    faults(
                        name,
                        aggregate,
                        separator,
                        targets[aggregate],
                        Record.filter(targets, (_, target) => target.startsWith(`${aggregate}${separator}`)),
                    ),
                ),
                ...Record.collect(targets, (target, { dependsOn = [] }) =>
                    Array.intersection(
                        dependsOn.map((entry) => (Predicate.isString(entry) ? entry.replace(/^\^|^.*:/u, '') : entry.target)),
                        aggregates,
                    ).map((aggregate) => `${target} depends on ${aggregate}, which forwards the parameters of the run to its tool targets, depend on ${aggregate}${separator}* in its place`),
                ).flat(),
            ].map((fault) => ({ sourceProject: name, message: `${name} ${fault}` })),
        );
        return Promise.resolve({ severity: 'medium' as const, details: { violations } });
    },
});
