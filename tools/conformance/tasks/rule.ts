import { homedir, hostname, tmpdir, userInfo } from 'node:os';
import { isAbsolute, join, normalize, relative, resolve } from 'node:path';
import { createConformanceRule } from '@nx/conformance';
import { readNxJson } from '@nx/devkit';
import { Array, Config, ConfigProvider, Effect, Formatter, Match, Option, Predicate, Record, Result, Struct } from 'effect';
import { getInputs } from 'nx/src/hasher/task-hasher';
import { createTaskGraph } from 'nx/src/tasks-runner/create-task-graph';
import { assertTaskGraphDoesNotContainInvalidTargets, findCycle } from 'nx/src/tasks-runner/task-graph-utils';
import { getOutputsForTargetAndConfiguration } from 'nx/src/tasks-runner/utils';
import { createGitIgnoreChecker } from 'nx/src/utils/ignore';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STAMP = /\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/u;
const WORD = /[\s'"=]+/u;
const GLOBBED = /\/[^/]*[*?[{].*$/u;
const FOLDERS = [homedir(), tmpdir()];
const NAMES = new Set([userInfo().username, hostname()]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const strings = (value: unknown, at: string): readonly (readonly [at: string, text: string])[] =>
    Match.value(value).pipe(
        Match.when(Predicate.isObjectOrArray, (held) => Object.entries(held).flatMap(([key, inner]) => strings(inner, `${at}.${key}`))),
        Match.when(Predicate.isString, (text) => [[at, text] as const]),
        Match.orElse(() => []),
    );

const machined = (root: string, text: string) => [root, ...FOLDERS].some((folder) => text.includes(folder)) || text.split('/').some((segment) => NAMES.has(segment)) || STAMP.test(text);

const outside = (root: string, project: string, text: string) =>
    text.split(WORD).filter((word) => {
        const path = word.replaceAll('{workspaceRoot}', root).replaceAll('{projectRoot}', project);
        return (path.split('/').includes('..') || isAbsolute(path)) && [root, project].every((base) => relative(root, resolve(base, path)).startsWith('..'));
    });

const reaching = (edges: Record.ReadonlyRecord<string, readonly string[]>, held: ReadonlySet<string>): ReadonlySet<string> => {
    const next = held.union(new Set(Record.keys(Record.filter(edges, (targets) => targets.some((target) => held.has(target))))));
    return next.size === held.size ? held : reaching(edges, next);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

export default createConformanceRule<{ readonly clean: string }>({
    name: 'tasks',
    category: 'reliability',
    description:
        'Plans every target of every project as Nx runs it with no task cycle; keeps each output of a cached target, under each configuration, in a folder git ignores and apart from the outputs of every other cached target, as a replay deletes and restores it; gives every project declaring outputs a `clean` target no cached task depends on; and keeps the checkout, home, or temporary folder, a user or host name segment, and a timestamp out of every project caching a target and out of every variable a cached task hashes, and a path outside the workspace out of every cached target, as one remote cache serves every runner',
    implementation: ({ tree, projectGraph, ruleOptions: { clean } }) => {
        const { root } = tree;
        const ignored = createGitIgnoreChecker(tree);
        const nodes = Object.values(projectGraph.nodes);
        const declared = nodes.flatMap((node) => Record.collect(node.data.targets ?? {}, (target, configuration) => ({ node, target, configuration })));
        const bases = declared
            .filter(({ configuration }) => configuration.cache === true)
            .flatMap(({ node, target, configuration }) =>
                [undefined, ...Record.keys(configuration.configurations ?? {})].flatMap((named) =>
                    getOutputsForTargetAndConfiguration({ project: node.name, target, ...(named === undefined ? {} : { configuration: named }) }, {}, node)
                        .filter((output) => !output.startsWith('!'))
                        .map((output) => ({ owner: `${node.name}:${target}`, project: node.name, label: [node.name, target, ...Array.fromNullishOr(named)].join(':'), base: normalize(output.replace(GLOBBED, '')).replace(/\/$/u, '') })),
                ),
            );
        const stored = bases.flatMap(({ owner, project, label, base }) =>
            [
                ...(isAbsolute(base) || base.split('/')[0] === '..' || (base !== '.' && (ignored.isIgnoredDirectory(base) || ignored.isIgnoredFile(base))) ? [] : [`${label} writes ${base}, which git does not ignore, so a replay deletes workspace files there, write it under an ignored folder`]),
                ...bases.filter((inner) => inner.owner !== owner && `${inner.base}/`.startsWith(`${base}/`)).map((inner) => `${label} writes ${base}, which holds ${inner.base} of ${inner.label}`),
            ].map((message) => ({ sourceProject: project, message })),
        );
        const caching = new Set(declared.filter(({ configuration }) => configuration.cache === true).map(({ node }) => node.name));
        const hashed = declared
            .filter(({ node }) => caching.has(node.name))
            .flatMap(({ node, target, configuration }) =>
                strings(Struct.pick(configuration, ['inputs', 'outputs', 'options', 'configurations']), `${node.name}:${target}`)
                    .flatMap(([at, text]) => [
                        ...(machined(root, text) ? [`${at} holds ${text}, which names this machine or this run, state it relative to {workspaceRoot} or {projectRoot}`] : []),
                        ...(configuration.cache === true ? outside(root, join(root, node.data.root), text) : []).map((word) => `${at} names ${word} outside the workspace, which no other runner holds, move it inside`),
                    ])
                    .map((message) => ({ sourceProject: node.name, message })),
            );
        const unclean = nodes.filter(({ data: { targets = {} } }) => !(clean in targets) && Object.values(targets).some(({ outputs = [] }) => Array.isReadonlyArrayNonEmpty(outputs))).map(({ name }) => ({ sourceProject: name, message: `${name} declares outputs and no ${clean} target, give it ${clean}` }));
        const graph = createTaskGraph(projectGraph, {}, Array.dedupe(declared.map(({ node }) => node.name)), Array.dedupe(declared.map(({ target }) => target)), undefined, {});
        const edges = Record.map(graph.tasks, (_, id) => [...(graph.dependencies[id] ?? []), ...(graph.continuousDependencies[id] ?? [])].map(Struct.get('id')));
        const cleaning = new Set(Object.values(graph.tasks).flatMap(({ id, target }) => (target.target === clean ? [id, ...(edges[id] ?? [])] : [])));
        const reached = reaching(edges, cleaning);
        const planned = [
            ...Array.fromNullishOr(findCycle(graph)).map((cycle) => ({ workspaceViolation: true, message: `Task cycle ${cycle.join(' -> ')}, drop one dependsOn entry of it` })),
            ...Result.match(
                Result.try(() => assertTaskGraphDoesNotContainInvalidTargets(graph)),
                { onFailure: (cause) => [{ workspaceViolation: true, message: Predicate.isError(cause) ? cause.message : Formatter.format(cause) }], onSuccess: () => [] },
            ),
            ...Object.values(graph.tasks)
                .filter(({ id, cache }) => cache && !cleaning.has(id) && reached.has(id))
                .map(({ id, target }) => ({ sourceProject: target.project, message: `${id} is cached and depends on a ${clean} task, which deletes outputs before every replay, drop that dependsOn entry` })),
        ];
        const nxJson = readNxJson(tree) ?? {};
        const variables = Object.values(graph.tasks)
            .filter(({ cache }) => cache)
            .flatMap((task) => Array.filterMap(getInputs(task, projectGraph, nxJson).selfInputs, (input) => ('env' in input ? Result.succeed({ id: task.id, project: task.target.project, variable: input.env }) : Result.failVoid)));
        return Effect.forEach(variables, ({ id, project, variable }) =>
            Effect.map(Config.option(Config.String(variable)), (value) =>
                Option.toArray(Option.filter(value, (held) => machined(root, held))).map((held) => ({ sourceProject: project, message: `${id} hashes ${variable}=${held}, which names this machine, so no other runner shares its cache entry, drop it from the inputs` })),
            ),
        ).pipe(
            Effect.map((named) => ({ severity: 'high' as const, details: { violations: [...planned, ...stored, ...unclean, ...hashed, ...named.flat()] } })),
            Effect.provideService(ConfigProvider.ConfigProvider, ConfigProvider.fromEnv()),
            Effect.runPromise,
        );
    },
});
