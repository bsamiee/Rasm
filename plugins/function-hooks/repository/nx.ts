import { both, decoded, fault, map, none, type Option, ok, type Result, some } from '../composition.ts';
import type { Command } from '../policies/command.ts';
import { operands } from '../policies/invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Input = string | object;
type Read = (path: string) => Promise<Result<string>>;

interface TargetDependency {
    readonly target: string;
    readonly projects?: string | readonly string[];
    readonly dependencies?: boolean;
}
interface TargetConfiguration {
    readonly options: { readonly command?: string; readonly commands?: readonly (string | { readonly command: string })[] };
    readonly dependsOn?: readonly (string | TargetDependency)[];
}
interface ProjectGraph {
    readonly nodes: Readonly<Record<string, { readonly data: { readonly tags: readonly string[]; readonly targets: Readonly<Record<string, TargetConfiguration>> } }>>;
    readonly dependencies: Readonly<Record<string, readonly { readonly target: string }[]>>;
}
interface Repository {
    readonly targets: Readonly<Record<string, { readonly inputs?: readonly Input[] }>>;
    readonly namedInputs: Readonly<Record<string, readonly Input[]>>;
}
interface Request {
    readonly targets: readonly string[];
    readonly projects: readonly string[];
}
interface Task {
    readonly project: string;
    readonly target: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOTED = /^(?<negation>!?)\{(?:workspace|project)Root\}\//u;
const _TOKEN = /\*\*\/|\*\*|[*?]|\{[^}]*\}|[$()+.[\]\\^|]/gu;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DECODING]

const graphPath = (data: Option<string>): Result<string> => (data.kind === 'some' ? ok(`${data.value}/project-graph.json`) : fault({ kind: 'unread', subject: 'NX_WORKSPACE_DATA_DIRECTORY', cause: 'unset' }));

const graph = async (read: Read, path: string): Promise<Result<ProjectGraph>> => decoded<ProjectGraph>('project-graph.json', await read(path));

const repository = async (read: Read, root: string): Promise<Result<Repository>> => {
    const [manifest, configuration] = await Promise.all([read(`${root}/package.json`), read(`${root}/nx.json`)]);
    return map(both(decoded<{ readonly nx: Pick<Repository, 'targets'> }>('package.json', manifest), decoded<Pick<Repository, 'namedInputs'>>('nx.json', configuration)), ([{ nx }, { namedInputs }]) => ({ targets: nx.targets, namedInputs }));
};

// --- [GLOBS]

const _glob = (pattern: string): RegExp => {
    const wild: Readonly<Record<string, string>> = { '**/': '(?:.*/)?', '**': '.*', '*': '[^/]*', '?': '[^/]' };
    const converted = (text: string): string => text.replaceAll(_TOKEN, (token) => wild[token] ?? (token.startsWith('{') ? `(?:${token.slice(1, -1).split(',').map(converted).join('|')})` : `\\${token}`));
    return new RegExp(`^${converted(pattern)}$`, 'u');
};

const matches = (globs: readonly string[], path: string): boolean => globs.some((glob) => !glob.startsWith('!') && _glob(glob).test(path)) && !globs.some((glob) => glob.startsWith('!') && _glob(glob.slice(1)).test(path));

const inputs = (target: string, { targets, namedInputs }: Repository): readonly string[] => {
    const expanded = (entries: readonly Input[]): readonly string[] =>
        entries
            .filter((entry) => typeof entry === 'string')
            .flatMap((entry) => {
                const named = namedInputs[entry];
                return named === undefined ? [entry] : expanded(named);
            });
    return expanded(targets[target]?.inputs ?? [])
        .filter((entry) => _ROOTED.test(entry))
        .map((entry) => entry.replace(_ROOTED, '$<negation>'));
};

// --- [TASKS]

const requests = (commands: readonly Command[]): readonly Request[] =>
    commands
        .flatMap(({ invocations }) => invocations)
        .filter(([program]) => program === 'nx')
        .flatMap((invocation): readonly Request[] => {
            const {
                inputs: [subcommand, ...rest],
                values,
            } = operands(invocation);
            const given = (...names: readonly string[]): readonly string[] => values.flatMap(([name, value]) => (names.includes(name) ? value.split(',') : []));
            if (subcommand === 'run-many' || subcommand === 'affected') {
                return [{ targets: given('-t', '--targets', '--target'), projects: given('-p', '--projects') }];
            }
            if (subcommand === 'run') {
                return [{ targets: rest.slice(0, 1), projects: [] }];
            }
            return subcommand === undefined ? [] : [{ targets: [subcommand], projects: rest.slice(0, 1) }];
        });

const _configuration = ({ nodes }: ProjectGraph, { project, target }: Task): TargetConfiguration | undefined => nodes[project]?.data.targets[target];

const _declared = (held: ProjectGraph, target: string, projects: readonly string[]): readonly Task[] => projects.map((project) => ({ project, target })).filter((task) => _configuration(held, task) !== undefined);

const _task = (held: ProjectGraph, specifier: string, current: Option<string>): readonly Task[] => {
    const segments = specifier.split(':');
    const joined = (from: number, to: number): string => segments.slice(from, to).join(':');
    const cuts = segments.map((_segment, index) => segments.length - index);
    const owners = [...(current.kind === 'some' ? [[current.value, 0] as const] : []), ...cuts.map((cut) => [joined(0, cut), cut] as const)];
    return owners
        .flatMap(([project, from]) => cuts.filter((cut) => cut > from).map((cut) => ({ project, target: joined(from, cut) })))
        .filter((task) => _configuration(held, task) !== undefined)
        .slice(0, 1);
};

const _matching = ({ nodes }: ProjectGraph, patterns: readonly string[]): readonly string[] =>
    Object.entries(nodes)
        .filter(([name, { data }]) =>
            patterns.reduce((kept, pattern) => {
                const negated = pattern.startsWith('!');
                const text = pattern.slice(negated ? 1 : 0);
                const hit = text.startsWith('tag:') ? data.tags.some((tag) => _glob(text.slice('tag:'.length)).test(tag)) : _glob(text).test(name);
                return negated ? kept && !hit : kept || hit;
            }, patterns[0]?.startsWith('!') === true),
        )
        .map(([name]) => name);

const _upstream = (held: ProjectGraph, task: Task): readonly Task[] => {
    const upstream = (held.dependencies[task.project] ?? []).map(({ target }) => target);
    return (_configuration(held, task)?.dependsOn ?? []).flatMap((entry): readonly Task[] => {
        if (typeof entry === 'string') {
            return entry.startsWith('^') ? _declared(held, entry.slice(1), upstream) : _task(held, entry, some(task.project));
        }
        const patterns = [entry.projects ?? []].flat();
        if (patterns.length > 0) {
            return _declared(held, entry.target, _matching(held, patterns));
        }
        return _declared(held, entry.target, entry.dependencies === true ? upstream : [task.project]);
    });
};

const taskCommands = (held: ProjectGraph, specified: readonly Request[]): readonly string[] => {
    const id = ({ project, target }: Task): string => `${project}:${target}`;
    const closure = (pending: readonly Task[], seen: ReadonlyMap<string, Task>): ReadonlyMap<string, Task> => {
        const fresh = new Map(pending.flatMap((task) => (seen.has(id(task)) ? [] : [[id(task), task] as const])));
        return fresh.size === 0
            ? seen
            : closure(
                  [...fresh.values()].flatMap((task) => _upstream(held, task)),
                  new Map([...seen, ...fresh]),
              );
    };
    const named = specified.flatMap(({ targets, projects }) =>
        targets.flatMap((specifier) => {
            const found = _task(held, specifier, none);
            return found.length > 0 ? found : _declared(held, specifier, projects.length === 0 ? Object.keys(held.nodes) : _matching(held, projects));
        }),
    );
    return [...closure(named, new Map()).values()].flatMap((task) => {
        const options = _configuration(held, task)?.options;
        return [options?.command, ...(options?.commands ?? []).map((entry) => (typeof entry === 'string' ? entry : entry.command))].filter((text) => text !== undefined);
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { graph, graphPath, inputs, matches, repository, requests, taskCommands };
