import { basename, declared, type Given, type Invocation, informational, invocations, operands, segments, unquoted, within } from './invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Args = string | readonly string[];
type Line = string | { readonly command: string; readonly forwardAllArgs?: boolean };
type Scope = { readonly kind: 'at'; readonly cwd: string; readonly forwarded: readonly number[] } | { readonly kind: 'every'; readonly folders: readonly string[] } | { readonly kind: 'none' };
type Exclusion = { readonly kind: 'path'; readonly path: string } | { readonly kind: 'pattern'; readonly pattern: RegExp } | { readonly kind: 'opaque'; readonly source: string };

interface Declared {
    readonly executor?: string;
    readonly defaultConfiguration?: string;
    readonly options?: { readonly command?: string; readonly commands?: readonly Line[]; readonly args?: Args; readonly cwd?: string };
    readonly configurations?: { readonly [name: string]: { readonly args?: Args } };
}
interface Graph {
    readonly nodes: { readonly [project: string]: { readonly data: { readonly targets?: { readonly [target: string]: Declared } } } };
}
interface Task {
    readonly project: string;
    readonly target: string;
    readonly sole: boolean;
    readonly cwd: string;
    readonly launchers: readonly Invocation[];
    readonly tool: readonly string[];
    readonly command: readonly string[];
    readonly passed: readonly string[];
    readonly include: readonly string[];
    readonly exclude: readonly Exclusion[];
    readonly options: readonly string[];
    readonly values: readonly Given[];
    readonly varied: readonly string[];
}
interface Catalog {
    readonly root: string;
    readonly tools: ReadonlyMap<string, readonly Task[]>;
}
interface Parsed {
    readonly inputs: readonly number[];
    readonly words: readonly string[];
    readonly passed: readonly string[];
    readonly options: readonly string[];
    readonly values: readonly Given[];
}
interface Route {
    readonly forwarded: readonly number[];
    readonly form: string;
    readonly notice: string;
    readonly lacking: readonly string[];
}
interface Routing {
    readonly routes: readonly Route[];
    readonly denials: readonly string[];
    readonly launchers: readonly Invocation[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _PLAIN = /^(?:[^"'\\|&;<>()`$]|\$(?!\()|\\.|"(?:[^"\\`$]|\$(?!\()|\\.)*"|'[^']*')*$/su;
const _WORD = /(?:[^\s"'\\]|\\.|"(?:[^"\\]|\\.)*"|'[^']*')+/gu;
const _MEMBER = /<Project\b[^>]*\bPath="(?<path>[^"]+)"/gu;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PATHS]

const _absolute = (base: string, path: string): string => `/${segments(path.startsWith('/') ? path : `${base}/${path}`).join('/')}`;
const _folder = (path: string): string => `/${segments(path).slice(0, -1).join('/')}`;

const _base = (program: string, values: readonly Given[], cwd: string): string => {
    const { relocations = [], manifests = [] } = declared(program);
    const moved = values.findLast(([name]) => relocations.includes(name))?.[1] ?? '.';
    return manifests.some((suffix) => moved.endsWith(suffix)) ? _folder(_absolute(cwd, moved)) : _absolute(cwd, moved);
};

const _members = (path: string, listed: string): readonly string[] =>
    listed
        .matchAll(_MEMBER)
        .map(({ groups }) => _folder(_absolute(_folder(path), (groups?.path ?? '').replaceAll('\\', '/'))))
        .toArray();

// --- [CATALOG]

const _parsed = (invocation: Invocation): Parsed => {
    const { options, values, positions } = operands(invocation);
    const held = new Set(positions.flatMap(({ at, taken }) => Array.from({ length: taken + 1 }, (_, step) => at + step)));
    const separator = invocation.findIndex((word, index) => index > 0 && word === '--' && !held.has(index));
    const end = separator < 0 ? invocation.length : separator;
    const inputs = invocation.flatMap((_, index) => (index === 0 || index >= end || held.has(index) ? [] : [index]));
    return { inputs, words: invocation.filter((_, index) => inputs.includes(index)), passed: invocation.slice(end + 1), options, values };
};

const _exclusion = (cwd: string, value: string, kind: 'path' | 'pattern'): Exclusion => {
    if (kind === 'path') {
        return { kind, path: _absolute(cwd, value) };
    }
    try {
        return { kind, pattern: new RegExp(value, 'u') };
    } catch {
        return { kind: 'opaque', source: value };
    }
};

const _tasks = (root: string, project: string, target: string, entry: Declared): readonly Omit<Task, 'varied'>[] => {
    const words = (text: string): readonly string[] =>
        text
            .matchAll(_WORD)
            .map(([word]) => unquoted(word))
            .toArray();
    const args = (typeof entry.defaultConfiguration === 'string' ? entry.configurations?.[entry.defaultConfiguration]?.args : undefined) ?? entry.options?.args ?? [];
    const lines: readonly (Line | undefined)[] = entry.executor === 'nx:run-commands' ? (entry.options?.commands ?? [entry.options?.command]) : [undefined];
    const folder = _absolute(root, entry.options?.cwd ?? '.');
    return lines.map((line) => {
        const [command, given] = typeof line === 'object' ? [line.command, line.forwardAllArgs === false ? [] : args] : [line, args];
        const plain = command !== undefined && _PLAIN.test(command) && (typeof given !== 'string' || _PLAIN.test(given));
        const chain = plain ? invocations([...words(command), ...(typeof given === 'string' ? words(given) : given)]) : [];
        const tool = chain.at(-1);
        const parsed: Parsed = tool === undefined ? { inputs: [], words: [], passed: [], options: [], values: [] } : _parsed(tool);
        const { commands = parsed.words.length, excludes = [], patterns = [] } = tool === undefined ? {} : declared(tool[0]);
        const cwd = tool === undefined ? folder : _base(tool[0], parsed.values, folder);
        return {
            project,
            target,
            sole: lines.length === 1,
            cwd,
            launchers: chain.slice(0, -1),
            tool: tool ?? [],
            command: parsed.words.slice(0, commands),
            passed: parsed.passed,
            include: parsed.words.slice(commands).map((path) => _absolute(cwd, path)),
            exclude: parsed.values.flatMap(([name, value]) => [...(excludes.includes(name) ? [_exclusion(cwd, value, 'path')] : []), ...(patterns.includes(name) ? [_exclusion(cwd, value, 'pattern')] : [])]),
            options: parsed.options,
            values: parsed.values,
        };
    });
};

const catalog = (graph: Graph, root: string): Catalog => {
    const lines = Object.entries(graph.nodes).flatMap(([project, { data }]) => Object.entries(data.targets ?? {}).flatMap(([target, entry]) => _tasks(root, project, target, entry)));
    const invoked = ({ project, tool, command }: Omit<Task, 'varied'>): string => [project, ...tool.slice(0, 1), ...command].join('\0');
    const siblings = Map.groupBy(lines, invoked);
    const tasks = lines.map((line): Task => {
        const held = siblings.get(invoked(line)) ?? [];
        return { ...line, varied: [...new Set(held.flatMap(({ options }) => options))].filter((name) => !held.every(({ options }) => options.includes(name))) };
    });
    return {
        root,
        tools: new Map(
            [
                ...Map.groupBy(
                    tasks.flatMap((task) => task.tool.slice(0, 1).map((program) => ({ program, task }))),
                    ({ program }) => program,
                ),
            ].map(([program, held]) => [program, held.map(({ task }) => task)]),
        ),
    };
};

// --- [ROUTES]

const solutions = (words: Invocation, cwd: string): readonly string[] => {
    const { solutions: suffixes = [] } = declared(basename(words[0]));
    return words.slice(1).flatMap((word) => (suffixes.some((suffix) => word.endsWith(suffix)) ? [_absolute(cwd, word)] : []));
};

const _scope = (words: Invocation, extra: readonly number[], cwd: string, files: ReadonlyMap<string, string>, folders: ReadonlySet<string>): Scope => {
    const { manifests = [], solutions: suffixes = [] } = declared(words[0]);
    const named = extra.flatMap((index) => words.slice(index, index + 1));
    const [only] = named;
    if (named.some((word) => word.startsWith('-'))) {
        return { kind: 'none' };
    }
    if (named.length !== 1 || only === undefined) {
        return { kind: 'at', cwd, forwarded: extra };
    }
    const path = _absolute(cwd, only);
    const listed = files.get(path);
    if (suffixes.some((suffix) => only.endsWith(suffix))) {
        return listed === undefined ? { kind: 'none' } : { kind: 'every', folders: _members(path, listed) };
    }
    if (manifests.some((suffix) => only.endsWith(suffix))) {
        return { kind: 'at', cwd: _folder(path), forwarded: [] };
    }
    return path === cwd || (manifests.length > 0 && folders.has(path)) ? { kind: 'at', cwd: path, forwarded: [] } : { kind: 'at', cwd, forwarded: extra };
};

const _covers = (root: string, task: Task, path: string): boolean => {
    const relative = `${path.slice(root.length + 1)}/`;
    const excluded = task.exclude.some((exclusion) => {
        switch (exclusion.kind) {
            case 'path':
                return within(path, exclusion.path);
            case 'pattern':
                return exclusion.pattern.test(relative);
            default:
                return true;
        }
    });
    return !excluded && (task.include.length === 0 ? [task.cwd] : task.include).some((folder) => within(path, folder) || within(folder, path));
};

const _routed = (owned: Catalog, words: Invocation, call: Parsed, base: string, files: ReadonlyMap<string, string>, strict: boolean): readonly Route[] => {
    const [program] = words;
    const { spans = [], relocations = [] } = declared(program);
    const spanned = call.options.some((name) => spans.includes(name));
    const candidates = (owned.tools.get(program) ?? []).filter((task) => task.sole && task.command.every((word, at) => words[call.inputs[at] ?? -1] === word) && call.passed.every((word) => task.passed.includes(word)) && task.varied.every((name) => task.options.includes(name) === call.options.includes(name)));
    const holds = (task: Task, name: string): boolean => task.values.some(([held, given]) => held === name && call.values.some(([option, value]) => option === name && value === given));
    const takes = (task: Task): boolean => (!strict || call.options.every((name) => spans.includes(name) || relocations.includes(name) || task.options.includes(name))) && call.values.every(([name]) => relocations.includes(name) || holds(task, name) || !(strict || candidates.some((other) => holds(other, name))));
    const folders = new Set(candidates.map(({ cwd: folder }) => folder));
    const fitted = candidates.flatMap((task) => {
        const scope = _scope(words, call.inputs.slice(task.command.length), base, files, folders);
        const named = scope.kind === 'at' ? scope.forwarded.flatMap((index) => words.slice(index, index + 1).map((word) => _absolute(scope.cwd, word))) : [];
        const paths = scope.kind === 'at' && named.length === 0 ? [scope.cwd] : named;
        return (scope.kind === 'every' && scope.folders.includes(task.cwd)) || (scope.kind === 'at' && scope.cwd === task.cwd && paths.some((path) => _covers(owned.root, task, path))) ? [{ task, scope, paths }] : [];
    });
    const groups = Map.groupBy(fitted, ({ task }) => [task.project, task.cwd, ...task.include, ...task.exclude.map((exclusion) => JSON.stringify(exclusion.kind === 'pattern' ? exclusion.pattern.source : exclusion))].join('\0'))
        .values()
        .map((held) => held.filter(({ task }) => takes(task)).toSorted((left, right) => left.task.tool.length - right.task.tool.length))
        .filter((held) => held.length > 0)
        .toArray();
    const chosen = groups.flatMap(([cheapest, next]) => (cheapest === undefined || next?.task.tool.length === cheapest.task.tool.length ? [] : [cheapest]));
    const [first] = chosen;
    if (first === undefined || chosen.length < groups.length) {
        return [];
    }
    const tasks = chosen.map(({ task }) => task).toSorted((left, right) => left.project.localeCompare(right.project) || left.target.localeCompare(right.target));
    const projects = [...new Set(tasks.map((task) => task.project))];
    const targets = [...new Set(tasks.map((task) => task.target))].toSorted();
    const { scope, paths } = first;
    const selected = tasks.every((task) => task.tool.includes(task.project) && words.includes(task.project));
    const omitted = tasks.some((task) => task.tool.includes(task.project) && !words.includes(task.project));
    const same = (owned.tools.get(program) ?? []).filter((task) => targets.includes(task.target));
    const spanning = scope.kind === 'every' ? scope.folders.every((folder) => tasks.some((task) => task.cwd === folder)) : selected || !(spanned || omitted || projects.length > 1) || tasks.length === same.length;
    const covered = paths.every((path) => tasks.some((task) => _covers(owned.root, task, path)));
    const forwarded = scope.kind === 'at' && tasks.every(({ include }) => include.length === 0) ? scope.forwarded : [];
    const lacking = [
        ...new Set([
            ...call.options.filter((name) => !(spans.includes(name) || relocations.includes(name) || tasks.some((task) => task.options.includes(name)))),
            ...call.values.flatMap(([name, value]) => (relocations.includes(name) || tasks.some((task) => task.values.some(([held, given]) => held === name && given === value)) ? [] : [name])),
        ]),
    ];
    if (!(spanning && covered) || (strict && lacking.length > 0) || (projects.length > 1 && targets.length > 1)) {
        return [];
    }
    const form = projects.length === 1 && targets.length === 1 ? `nx run ${projects.join('')}:${targets.join('')}` : `nx run-many -t ${targets.join(',')} -p ${projects.join(',')}`;
    return [{ forwarded, form, notice: `${words.join(' ')} ran as ${form}`, lacking }];
};

const route = (owned: Catalog, head: Invocation, cwd: string, files: ReadonlyMap<string, string>): Routing => {
    const words: Invocation = [basename(head[0]), ...head.slice(1)];
    const [program] = words;
    const { manifests = [], solutions: suffixes = [] } = declared(program);
    const call = _parsed(words);
    if (informational(program, call.options)) {
        return { routes: [], denials: [], launchers: [] };
    }
    const base = _base(program, call.values, cwd);
    const strict = _routed(owned, words, call, base, files, true);
    const loose = strict.length > 0 ? [] : _routed(owned, words, call, base, files, false);
    const named = call.words.flatMap((word) => {
        const path = _absolute(base, word);
        const listed = files.get(path);
        if (manifests.some((suffix) => word.endsWith(suffix))) {
            return [_folder(path)];
        }
        return listed === undefined || !suffixes.some((suffix) => word.endsWith(suffix)) ? [] : [_folder(path), ..._members(path, listed)];
    });
    const folders = named.length === 0 ? [base] : named;
    const tasks = (owned.tools.get(program) ?? []).filter(({ sole }) => sole);
    const related = tasks.filter(({ command }) => command.every((word, at) => call.words[at] === word));
    const prefixes = new Set(related.map(({ launchers }) => JSON.stringify(launchers)));
    const local = tasks.filter((task) => task.command.slice(0, Math.max(call.words.length, 1)).every((word, at) => call.words[at] === word) && folders.some((folder) => (task.include.length === 0 ? task.cwd === folder : task.include.some((path) => within(folder, path)))));
    const owners = manifests.length === 0 ? related : local;
    const forms = [...new Set(owners.filter((task) => related.includes(task)).map(({ project, target }) => `nx run ${project}:${target}`))];
    const unowned = strict.length > 0 || loose.length > 0 || owners.length === 0 || !within(base, owned.root);
    return {
        routes: strict,
        denials: [
            ...loose.map(
                ({ form, forwarded, lacking }) =>
                    `${lacking.length === 0 ? `${words.join(' ')} sets an option value its Nx targets differ on` : `No Nx target passes ${lacking.join(' or ')} to ${program}`}. Run ${[form, ...(forwarded.length === 0 ? [] : ['--', ...forwarded.flatMap((index) => words.slice(index, index + 1))])].join(' ')}`,
            ),
            ...(unowned ? [] : [`${words.join(' ')} matches no Nx target. ${forms.length > 0 ? `Run ${forms.join(' or ')}` : `nx show project ${[...new Set(owners.map(({ project }) => project))].join(' or ')} lists the targets to run`}`]),
        ],
        launchers: prefixes.size === 1 ? (related[0]?.launchers ?? []) : [],
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Catalog, Graph, Route };
export { catalog, route, solutions };
