import { homedir, hostname, tmpdir, userInfo } from 'node:os';
import { isAbsolute, join, posix, resolve } from 'node:path';
import { createConformanceRule } from '@nx/conformance';
import { getOutputsForTargetAndConfiguration, readNxJson } from '@nx/devkit';
import { createTaskId } from '@nx/devkit/internal';
import { above, below, inside } from '@rasm/nx/inference';
import { Array, Config, ConfigProvider, DateTime, Effect, Formatter, Function, Match, Option, Predicate, pipe, Record, Result, Struct } from 'effect';
import { getInputs } from 'nx/src/hasher/task-hasher';
import { createTaskGraph } from 'nx/src/tasks-runner/create-task-graph';
import { assertTaskGraphDoesNotContainInvalidTargets, findCycle } from 'nx/src/tasks-runner/task-graph-utils';
import { createGitIgnoreChecker } from 'nx/src/utils/ignore';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STAMP = /\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})?/gu;
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

const machined = (root: string, text: string) => [root, ...FOLDERS].some((folder) => text.includes(folder)) || text.split('/').some((segment) => NAMES.has(segment)) || text.matchAll(STAMP).some(([stamp]) => Option.exists(DateTime.make(stamp), DateTime.isPastUnsafe));

const outside = (root: string, project: string, text: string) =>
    text.split(WORD).filter((word) => {
        const path = word.replaceAll('{workspaceRoot}', root).replaceAll('{projectRoot}', project);
        return [root, project].every((base) => !inside(root)(resolve(base, path)));
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
        'Plans every target of every project as Nx runs it with no task cycle; keeps each output of a cached target, under each configuration, in a folder git ignores and apart from the outputs of every other cached target, as a replay deletes and restores it; gives every project declaring outputs a `clean` target no cached task depends on; and keeps the checkout, home, or temporary folder, a user or host name segment, and a past timestamp out of every project caching a target and out of every variable a cached task hashes, and a path outside the workspace out of every cached target, as one remote cache serves every runner',
    implementation: ({ tree, projectGraph, ruleOptions: { clean } }) => {
        const { root } = tree;
        const ignored = createGitIgnoreChecker(tree);
        const nodes = Object.values(projectGraph.nodes);
        const declared = nodes.flatMap((node) => Record.collect(node.data.targets ?? {}, (target, configuration) => ({ node, target, configuration })));
        const cached = declared.filter(({ configuration }) => configuration.cache === true);
        const bases = pipe(
            cached,
            Array.flatMap(({ node, target, configuration }) => Record.collect({ '': {}, ...configuration.configurations }, (named, options) => ({ node, target, label: createTaskId(node.name, target, named), options }))),
            Array.flatMap(({ node, target, label, options }) => getOutputsForTargetAndConfiguration({ project: node.name, target }, options, node).map((output) => ({ owner: `${node.name}:${target}`, project: node.name, label, base: posix.normalize(output.replace(GLOBBED, '')).replace(/\/$/u, '') }))),
            Array.filter(({ base }) => !base.startsWith('!')),
        );
        const writers = Array.groupBy(bases, Struct.get('base'));
        const caching = new Set(cached.map(({ node }) => node.name));
        const texts = declared.filter(({ node }) => caching.has(node.name)).flatMap(({ node, target, configuration }) => strings(Struct.pick(configuration, ['inputs', 'outputs', 'options', 'configurations']), `${node.name}:${target}`).map(([at, text]) => ({ node, cached: configuration.cache === true, at, text })));
        const graph = createTaskGraph(projectGraph, {}, Array.dedupe(declared.map(({ node }) => node.name)), Array.dedupe(declared.map(({ target }) => target)), undefined, {});
        const edges = Record.map(graph.tasks, (_, id) => [...(graph.dependencies[id] ?? []), ...(graph.continuousDependencies[id] ?? [])].map(Struct.get('id')));
        const cleaning = new Set(Object.values(graph.tasks).flatMap(({ id, target }) => (target.target === clean ? [id, ...(edges[id] ?? [])] : [])));
        const reached = reaching(edges, cleaning);
        const found = [
            ...Array.fromNullishOr(findCycle(graph)).map((cycle) => ({ workspaceViolation: true, message: `Task cycle ${cycle.join(' -> ')}, drop one dependsOn entry of it` })),
            ...Option.toArray(Result.getFailure(Result.try(() => assertTaskGraphDoesNotContainInvalidTargets(graph)))).map((cause) => ({ workspaceViolation: true, message: Predicate.isError(cause) ? cause.message : Formatter.format(cause) })),
            ...Object.values(graph.tasks)
                .filter(({ id, cache }) => cache && !cleaning.has(id) && reached.has(id))
                .map(({ id, target }) => ({ sourceProject: target.project, message: `${id} is cached and depends on a ${clean} task, which deletes outputs before every replay, drop that dependsOn entry` })),
            ...bases
                .filter(({ base }) => !isAbsolute(base) && inside('.')(base) && !(below('.')(base) && (ignored.isIgnoredDirectory(base) || ignored.isIgnoredFile(base))))
                .map(({ project, label, base }) => ({ sourceProject: project, message: `${label} writes ${base}, which git does not ignore, so a replay deletes workspace files there, write it under an ignored folder` })),
            ...bases.flatMap((inner) =>
                above(inner.base)
                    .filter((folder) => inside(folder)(inner.base))
                    .flatMap((folder) => writers[folder] ?? [])
                    .filter((outer) => outer.owner !== inner.owner)
                    .map((outer) => ({ sourceProject: outer.project, message: `${outer.label} writes ${outer.base}, which holds ${inner.base} of ${inner.label}` })),
            ),
            ...nodes.filter(({ data: { targets = {} } }) => !(clean in targets) && Object.values(targets).some(({ outputs = [] }) => Array.isReadonlyArrayNonEmpty(outputs))).map(({ name }) => ({ sourceProject: name, message: `${name} declares outputs and no ${clean} target, give it ${clean}` })),
            ...texts.filter(({ text }) => machined(root, text)).map(({ node, at, text }) => ({ sourceProject: node.name, message: `${at} holds ${text}, which names this machine or this run, state it relative to {workspaceRoot} or {projectRoot}` })),
            ...texts.filter(Struct.get('cached')).flatMap(({ node, at, text }) => outside(root, join(root, node.data.root), text).map((word) => ({ sourceProject: node.name, message: `${at} names ${word} outside the workspace, which no other runner holds, move it inside` }))),
        ];
        const nxJson = readNxJson(tree) ?? {};
        const variables = Object.values(graph.tasks)
            .filter(({ cache }) => cache)
            .flatMap((task) => Array.filterMap(getInputs(task, projectGraph, nxJson).selfInputs, (input) => ('env' in input ? Result.succeed({ id: task.id, project: task.target.project, variable: input.env }) : Result.failVoid)));
        return Effect.filterMapEffect(variables, ({ id, project, variable }) =>
            Config.option(Config.String(variable)).pipe(
                Config.map(Option.filter((held) => machined(root, held))),
                Config.map(Result.fromOption(Function.constVoid)),
                Config.map(Result.map((held) => ({ sourceProject: project, message: `${id} hashes ${variable}=${held}, which names this machine, so no other runner shares its cache entry, drop it from the inputs` }))),
            ),
        ).pipe(
            Effect.map((named) => ({ severity: 'high' as const, details: { violations: [...found, ...named] } })),
            Effect.provideService(ConfigProvider.ConfigProvider, ConfigProvider.fromEnv()),
            Effect.runPromise,
        );
    },
});
