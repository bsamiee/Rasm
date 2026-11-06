import type { ProjectGraph, ProjectGraphProjectNode, TargetConfiguration } from '@nx/devkit';
import { Array, Effect, Equal, Option, Predicate, Record, Schema, String } from 'effect';
import { type Call, cut, edited, type Fix, feedsJq, has, inserted, type Policy, perSubcommand, quote, redirected, refusal, silent, spoken, unassign, when, whole } from '#/policies/fix';
import type { Word } from '#/shell/script';
import { analyzed, Shell, type Site } from '#/shell/site';

// --- [TYPES] ---------------------------------------------------------------------------

type Targets = Readonly<Record<string, TargetConfiguration<Options>>>;
interface Options {
    readonly command?: string;
    readonly commands?: readonly (string | { readonly command: string; readonly forwardAllArgs?: boolean })[];
    readonly args?: string | readonly string[];
    readonly forwardAllArgs?: boolean;
}
interface Task {
    readonly project: string;
    readonly target: string;
    readonly outputs: readonly string[];
}
interface Run {
    readonly target: string;
    readonly forwardAllArgs: boolean;
    readonly narrows: boolean;
    readonly site: Site;
    readonly keys: ReadonlySet<string>;
    readonly places: readonly string[];
    readonly extra: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CACHE_FLAGS = new Set(['--skip-nx-cache', '--disable-nx-cache']);
const SHELL_GLOB = /^[\w@%+=:,./*?[\]{}!-]+$/u;

// --- [MODELS] --------------------------------------------------------------------------

const CustomLanguages = Schema.fromJsonString(Schema.Struct({ customLanguages: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ libraryPath: Schema.String, extensions: Schema.Array(Schema.String) }))) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [GRAPH]

const targetsOf = ({ data }: ProjectGraphProjectNode): Targets => data.targets ?? {};

const argsOf = (options: Options | undefined): readonly string[] => Array.ensure(options?.args ?? []);

const forwards = Schema.is(Schema.Struct({ params: Schema.Literal('forward') }));

const membersOf = (targets: Targets, parent: string, project: string, forwarded = false): readonly string[] => {
    const named = (targets[parent]?.dependsOn ?? [])
        .filter((entry) => !forwarded || forwards(entry))
        .map((entry) => (typeof entry === 'string' ? entry : entry.target))
        .map((name) => (name.startsWith(`${project}:`) ? name.slice(project.length + 1) : name));
    return Record.keys(targets).filter((candidate) => named.some((name) => name === candidate || (name.endsWith('*') && candidate.startsWith(name.slice(0, -1)))));
};

const cached = (node: ProjectGraphProjectNode, target: string): boolean => targetsOf(node)[target]?.cache === true || membersOf(targetsOf(node), target, node.name).some((member) => cached(node, member));

const groupOf = (targets: Targets, target: string, project: string): string =>
    Option.getOrElse(
        Array.findFirst(Record.keys(targets), (parent) => {
            const members = membersOf(targets, parent, project);
            return members.includes(target) && (targets[parent]?.dependsOn ?? []).some(forwards) && [parent, ...members].every((name) => argsOf(targets[name]?.options).length === 0);
        }),
        () => target,
    );

const deepestOwner = (graph: ProjectGraph, host: Call['host'], place: string): Option.Option<ProjectGraphProjectNode> =>
    Array.head(
        Object.values(graph.nodes)
            .filter(({ data }) => host.inside(place, host.path.join(host.root, data.root)))
            .toSorted((left, right) => right.data.root.length - left.data.root.length),
    );

const fileFolder = (place: string, host: Call['host']): string => (host.path.extname(place) === '' ? place : host.path.dirname(place));

const owners = (graph: ProjectGraph, host: Call['host'], { row, values }: Site, folder: string): readonly ProjectGraphProjectNode[] => {
    if (row.routes?.across === true) {
        return Object.values(graph.nodes);
    }
    const named = values(...(row.routes?.selects ?? []));
    const placeOwner = (place: string) => deepestOwner(graph, host, fileFolder(place, host));
    return row.routes?.rooted === true || named.length === 0
        ? Option.toArray(deepestOwner(graph, host, row.routes?.rooted === true ? host.root : folder))
        : Array.getSomes(named.map((value) => Option.orElse(Option.fromNullishOr(graph.nodes[value]), () => Option.flatMap(host.place(value, Option.some(folder)), placeOwner))));
};

// --- [RUNS]

const valueKeys = (names: readonly string[], values: readonly string[]): readonly string[] => Array.cartesianWith(names.slice(-1), values, (name, value) => `${name}=${value}`);

const keysOf = ({ items }: Site, known: Predicate.Predicate<string> = () => true): ReadonlySet<string> =>
    new Set(
        items.flatMap(({ names, values }) => {
            const held = names.filter(known);
            return [...held, ...valueKeys(held, values)];
        }),
    );

const subcommandWords = ({ row, path }: Site): number => (row.routes?.flat === true || path.length > 0 ? 0 : 1);

const scopeOf = (site: Site, host: Call['host']) => ({
    places: site.row.routes?.entries === true ? [] : Array.getSomes(site.operands.slice(subcommandWords(site), site.operands.length - site.tail.length).map(({ text }) => host.place(text, site.command.cwd))),
    extra: site.tail.map(({ text }) => text),
});

const subcommandOf = ({ row, path, operands }: Site): Option.Option<string> => (row.routes?.flat === true ? Option.none() : Array.head([...path, ...operands.map(({ text }) => text)]));

const bodyOf = (text: string, host: Call['host']) => analyzed(text, { cwd: Option.some(host.root), origin: 'body' });

const commandsOf = (target: string, { options, configurations = {} }: Targets[string]) => {
    const args = argsOf(options);
    const listed = options?.command === undefined ? (options?.commands ?? []) : [options.command];
    const running = (address: string, entries: NonNullable<Options['commands']>) =>
        entries.length > 1
            ? []
            : entries.map((entry) => {
                  const { command, forwardAllArgs = options?.forwardAllArgs ?? true } = typeof entry === 'string' ? { command: entry } : entry;
                  return { target: address, args, forwardAllArgs, narrows: !args.every(String.startsWith('-')), text: [command, ...(forwardAllArgs ? args : [])].join(' ') };
              });
    const configured = Object.entries(configurations).flatMap(([name, { command, commands = [] }]: [string, Options]) => [...running(target, command === undefined ? [] : [command]), ...running(`${target}:${name}`, commands)]);
    return [...running(target, listed), ...configured];
};

const runsOf = Effect.fnUntraced(function* (node: ProjectGraphProjectNode, site: Site, host: Call['host']) {
    const { programs } = yield* Shell;
    const routedRow = programs.get(site.row.routes?.through ?? '') ?? site.row;
    const named = new RegExp(
        [...programs.keys()]
            .filter((name) => programs.get(name) === routedRow)
            .map(RegExp.escape)
            .join('|'),
        'u',
    );
    const entries = Record.collect(targetsOf(node), commandsOf)
        .flat()
        .filter(({ text }) => named.test(text) && !text.includes('{args'));
    const bodies = yield* Effect.forEach(entries, ({ text }) => bodyOf(text, host), { concurrency: 'unbounded' });
    return Array.zip(entries, bodies)
        .filter(([{ args }, { commands }]) => args.length === 0 || commands.length <= 1)
        .flatMap(([entry, { sites }]) => Array.cartesian([entry], sites))
        .map(([{ target, forwardAllArgs, narrows }, ran]): Run => ({ target, forwardAllArgs, narrows, site: ran, keys: keysOf(ran), ...scopeOf(ran, host) }))
        .filter((run) => run.site.row === routedRow && Equal.equals(subcommandOf(run.site), subcommandOf(site)));
});

// --- [FIT]

const shellWord = ({ glob }: Word, text: string): Option.Option<string> => (glob ? Option.fromNullishOr(SHELL_GLOB.exec(text)?.[0]) : Option.some(quote(text)));

const forwardedWord = (word: Word, moved: boolean, folder: string, host: Call['host']): Option.Option<string> => {
    if (!moved) {
        return shellWord(word, word.text);
    }
    return word.known
        ? host.place(word.text, Option.some(folder)).pipe(
              Option.filter((place) => host.inside(place, host.root)),
              Option.flatMap((place) => shellWord(word, host.path.relative(host.root, place) || '.')),
          )
        : Option.none();
};

const chosenFile = ({ values, row: { routes } }: Site, host: Call['host']): string =>
    values(...(routes?.selects ?? []))
        .map((value) => host.path.basename(value))
        .filter((leaf) => host.path.extname(leaf) !== '' && leaf !== routes?.config)
        .join(' ');

const namesRootConfig = (value: string, root: string, folder: string, host: Call['host']): boolean => Option.exists(host.place(value, Option.some(folder)), (place) => fileFolder(place, host) === root);

const readsOwnConfig = Effect.fnUntraced(function* ({ data, name }: ProjectGraphProjectNode, { row: { routes }, values }: Site, folder: string, host: Call['host']) {
    const config = routes?.config;
    const root = host.path.join(host.root, data.root);
    const named = values(...(routes?.selects ?? [])).filter((value) => value !== name);
    if (config === undefined) {
        return true;
    }
    if (named.length > 0) {
        return named.every((value) => namesRootConfig(value, root, folder, host));
    }
    const parts = host.path.relative(root, folder).split(host.path.sep).filter(String.isNonEmpty);
    const nested = yield* Effect.forEach(parts, (_part, at) => host.exists(host.path.join(root, ...parts.slice(0, at + 1), config)));
    return !nested.some(Boolean);
});

const fitted = (site: Site, host: Call['host'], node: ProjectGraphProjectNode, runs: readonly Run[], folder: string) => {
    const targets = targetsOf(node);
    const { modes = [], selects = [], ignores = [], rooted } = site.row.routes ?? {};
    const found = runs.filter((ran) => modes.every((flag) => site.has(flag) === ran.site.has(flag)) && chosenFile(site, host) === chosenFile(ran.site, host));
    const known = new Set(found.flatMap((ran) => [...ran.site.names]));
    const wanted = keysOf(site, (name) => known.has(name) && !selects.includes(name) && !ignores.includes(name));
    const { places, extra } = scopeOf(site, host);
    const covers = (folders: readonly string[]): boolean => places.every((file) => folders.some((held) => host.inside(file, held)));
    const fits = found.filter((ran) => wanted.isSubsetOf(ran.keys) && new Set(extra).isSubsetOf(new Set(ran.extra)) && (ran.places.length === 0 || (places.length > 0 && covers(ran.places))));
    const fitting = places.length > 0 && fits.some(({ places: held }) => held.length > 0) ? fits.filter(({ places: held }) => held.length > 0) : fits;
    const dropped = new Set([...known, ...selects, ...ignores]);
    const [skipped] = site.items.filter(({ names }) => names.length === 0).slice(0, subcommandWords(site));
    const rest = site.items.filter((item) => item !== skipped && (item.names.length === 0 ? site.row.routes?.entries !== true : !new Set(item.names).isSubsetOf(dropped) && new Set(valueKeys(item.names, item.values)).isDisjointFrom(dropped)));
    const switched = rest.some(({ names }) => names.length > 0);
    const takes = ({ target: own }: Run): boolean => groupOf(targets, own, node.name) === own || membersOf(targets, groupOf(targets, own, node.name), node.name, true).includes(own);
    const accepting = switched ? fitting.filter((ran) => takes(ran) && (Option.isNone(ran.site.separator) || ran.site.row !== site.row)) : fitting;
    const fewest = Math.min(...accepting.map(({ keys }) => keys.size));
    const targetOf = (ran: Run): string => {
        const group = groupOf(targets, ran.target, node.name);
        return switched || ran.places.length > 0 || !membersOf(targets, group, node.name).every((member) => found.some(({ target: held }) => held === member)) ? ran.target : group;
    };
    const [target, ...others] = [...new Set(accepting.filter(({ keys }) => keys.size === fewest).map(targetOf))];
    if (target === undefined || others.length > 0) {
        return Option.none();
    }
    const running = accepting.filter((ran) => targetOf(ran) === target);
    const narrowing = running.some(({ narrows }) => narrows);
    const scoped = running.some(({ places: held, narrows }) => held.length > 0 && !narrows);
    const aggregate = !found.some((ran) => ran.target === target);
    const moved = rooted === true && folder !== host.root;
    const passed = Option.all(scoped && !switched ? [] : rest.flatMap(({ words }) => words).map((word) => forwardedWord(word, moved, folder, host)));
    const forwarded = [...Option.getOrElse(passed, () => []), ...(moved && !scoped && rest.length === 0 ? [quote(host.path.relative(host.root, folder))] : [])];
    const uniform = membersOf(targets, target, node.name).every((member) => found.some((ran) => ran.target === member && wanted.isSubsetOf(ran.keys)));
    const refused =
        (forwarded.length > 0 && running.some(({ forwardAllArgs }) => !forwardAllArgs)) ||
        rest.some(({ words }) => words.some(({ known: fixed, raw }) => !fixed && quote(raw) !== raw)) ||
        (aggregate && ((forwarded.length > 0 && modes.some((flag) => site.has(flag))) || (wanted.size > 0 && !uniform))) ||
        (moved && switched) ||
        rest.flatMap(({ words, values }) => [...values, ...words.map(({ text }) => text)]).some((text) => host.path.isAbsolute(text) && !host.inside(text, host.root)) ||
        Option.isNone(passed) ||
        !covers([host.root]) ||
        (!scoped && Option.isSome(site.separator));
    return refused ? Option.none() : Option.some({ project: node.name, target, args: forwarded.slice(0, 1).map(() => `--args=${quote([...argsOf(targets[target]?.options).filter((arg) => !narrowing || arg.startsWith('-')), ...forwarded].join(' '))}`) });
};

const launcherStart = ({ head, outer }: Site): Word => outer[outer.findLastIndex(({ row, path }) => !(row.launcher === true || row.module !== undefined || row.wraps === '--' || path.length > 0)) + 1]?.head ?? head;

const hiddenErrors: Fix = (site) => {
    const nulled = redirected(site, '>', undefined, '/dev/null');
    return [...nulled, ...redirected(site, '&>', undefined, '/dev/null'), ...(nulled.some(({ descriptor }) => Option.isNone(descriptor)) ? redirected(site, '>&', '2', '1') : [])].flatMap((redirect) => cut(site, [redirect]));
};

const looped = (what: string, action: string, ...reads: readonly string[]): Fix =>
    when(
        ({ repeated, operands }) => repeated && !reads.includes(operands[0]?.text ?? ''),
        () => refusal(`Loop reruns ${what}, ${action}`),
    );

const route: Fix = Effect.fnUntraced(function* (site, call) {
    const { host } = call;
    const { command, last, repeated } = site;
    const folder = Option.getOrUndefined(command.cwd);
    if (folder === undefined || !host.inside(folder, host.root) || command.fed || repeated || command.origin !== 'call' || !last || feedsJq(site, call) || Option.exists(subcommandOf(site), (subcommand) => site.row.routes?.direct?.includes(subcommand) === true)) {
        return [];
    }
    const owned = Option.match(yield* host.graph, { onNone: () => [], onSome: (held) => owners(held, host, site, folder) });
    const nodes = (yield* Effect.forEach(owned, (node) => readsOwnConfig(node, site, folder, host))).every(Boolean) ? owned : [];
    const runs = yield* Effect.forEach(nodes, (node) => runsOf(node, site, host), { concurrency: 'unbounded' });
    const fits = Array.zipWith(nodes, runs, (node, held) => fitted(site, host, node, held, folder));
    const [first, ...others] = site.row.routes?.across === true ? Array.getSomes(fits) : Option.getOrElse(Option.all(fits), () => []);
    const together = first !== undefined && others.length > 0 && first.args.length === 0 && others.every(({ target, args }) => target === first.target && args.length === 0);
    const graph = Option.getOrUndefined(yield* host.graph);
    if (first === undefined || graph === undefined || (others.length > 0 && !together)) {
        return [];
    }
    const tasks = [first, ...others].flatMap(({ project, target, args }) => leavesOf(graph, { project, target, outputs: [] }, args.length > 0));
    const built = yield* prerequisitesOf(graph, tasks, site, host);
    const [only, ...more] = tasks;
    const named = only !== undefined && more.length === 0 ? `nx run ${only.project}:${only.target}` : runMany(tasks);
    return whole(site, [...built, [named, '--exclude-task-dependencies', ...first.args].join(' ')].join(' && '), site.program, launcherStart(site));
});

const runMany = (tasks: readonly Task[]): string => `nx run-many -p ${Array.dedupe(tasks.map(({ project }) => project)).join(',')} -t ${Array.dedupe(tasks.map(({ target }) => target)).join(',')}`;

const taskOf = (graph: ProjectGraph, project: string, target: string): Task => ({ project, target, outputs: graph.nodes[project]?.data.targets?.[target]?.outputs ?? [] });

const leavesOf = (graph: ProjectGraph, task: Task, forwarded: boolean): readonly Task[] => {
    const targets = graph.nodes[task.project]?.data.targets ?? {};
    const members = targets[task.target]?.executor === 'nx:noop' ? membersOf(targets, task.target, task.project, forwarded) : [];
    return members.length === 0 ? [task] : members.flatMap((member) => leavesOf(graph, taskOf(graph, task.project, member), forwarded));
};

const matching = (graph: ProjectGraph, [owner, pattern]: readonly [string, string]): readonly Task[] => {
    const matches = (target: string) => target === pattern || (pattern.endsWith('*') && target.startsWith(pattern.slice(0, -1)));
    return Record.keys(graph.nodes[owner]?.data.targets ?? {})
        .filter(matches)
        .map((target) => taskOf(graph, owner, target));
};

const dependenciesOf = (graph: ProjectGraph, { project, target }: Task): readonly Task[] =>
    (graph.nodes[project]?.data.targets?.[target]?.dependsOn ?? [])
        .map((entry) => (Predicate.isString(entry) ? addressOfDependency(graph, project, entry) : ([Array.ensure(entry.projects ?? project)[0] ?? project, entry.target] as const)))
        .flatMap((address) => matching(graph, address))
        .flatMap((task) => [task, ...dependenciesOf(graph, task)]);

const addressOfDependency = (graph: ProjectGraph, project: string, entry: string): readonly [string, string] =>
    Option.match(
        Array.findFirst(Record.keys(graph.nodes), (name) => entry.startsWith(`${name}:`)),
        { onNone: () => [project, entry] as const, onSome: (name) => [name, entry.slice(name.length + 1)] as const },
    );

const prerequisitesOf = Effect.fnUntraced(function* (graph: ProjectGraph, tasks: readonly Task[], site: Site, host: Call['host']) {
    const dependencies = tasks.flatMap((task) => dependenciesOf(graph, task)).filter(({ outputs }) => outputs.length > 0);
    if (dependencies.length === 0) {
        return [];
    }
    const { stdout } = yield* host.run(['yq', '-o=json', '.', host.path.join(host.root, 'sgconfig.yml')]);
    const languages = Option.match(Schema.decodeOption(CustomLanguages)(stdout), { onNone: () => [], onSome: (config) => Object.values(config.customLanguages ?? {}) });
    const places = scopeOf(site, host).places.map((place) => host.path.relative(host.root, place));
    const listed = yield* host.run(['git', 'ls-files', '-z', '--cached', '--others', '--exclude-standard', '--', ...(places.length === 0 ? ['.'] : places)]);
    const extensions = new Set(listed.stdout.split('\0').map((file) => host.path.extname(file).slice(1)));
    const libraries = new Set(languages.filter((language) => !new Set(language.extensions).isDisjointFrom(extensions)).map(({ libraryPath }) => `{workspaceRoot}/${libraryPath}`));
    const needed = dependencies.filter(({ outputs }) => !new Set(outputs).isDisjointFrom(libraries));
    return needed.length === 0 ? [] : [runMany(needed)];
});

const launchedThrough: Fix = Effect.fnUntraced(function* (site, { host }) {
    const root = Option.flatMap(yield* host.graph, (held) => deepestOwner(held, host, host.root));
    if (Option.isNone(root) || site.outer.length > 0) {
        return [];
    }
    const launcher = Option.getOrUndefined(Array.findFirst(yield* runsOf(root.value, site, host), (ran) => ran.site.outer.length > 0))?.site;
    return launcher === undefined ? [] : edited(site, site.head, spoken(launcher.command.words.filter(({ start }) => start <= launcher.head.start)), site.program);
});

const customLanguages: Fix = (site) => {
    const [, subcommand = site.head] = site.words;
    return site.has('--custom-languages') ? [] : inserted(site, (subcommand.text.startsWith('-') ? site.head : subcommand).end, ' --custom-languages allow');
};

// --- [NX]

const addressOf = ({ operands }: Site) => {
    const [subcommand, address] = operands.map(({ text }) => text);
    const colon = address?.indexOf(':') ?? -1;
    return subcommand === 'run' && address !== undefined && colon > 0 ? Option.some([address.slice(0, colon), address.slice(colon + 1)] as const) : Option.none();
};

const askedTargets = (site: Site, graph: ProjectGraph) => {
    const listed = (...names: readonly string[]) => site.values(...names).flatMap((value) => value.split(','));
    const { projects, targets } = Option.match(addressOf(site), {
        onNone: () => ({ projects: listed('--projects'), targets: listed('--targets', '--target') }),
        onSome: ([project, target]) => ({ projects: [project], targets: [target] }),
    });
    const nodes = projects.length === 0 ? Object.values(graph.nodes) : Array.getSomes(projects.map((project) => Option.fromNullishOr(graph.nodes[project])));
    return Array.cartesian(nodes, targets).filter(([node, target]) => projects.length > 0 || targetsOf(node)[target] !== undefined);
};

const cacheFlags: Fix = Effect.fnUntraced(function* (site, { host }) {
    const asked = Option.match(yield* host.graph, { onNone: () => [], onSome: (held) => askedTargets(site, held) });
    const names = [...new Set(asked.map(([, target]) => target))].join(' ');
    return asked.length > 0 && asked.every(([node, target]) => cached(node, target)) ? site.items.filter(({ names: flagged }) => !new Set(flagged).isDisjointFrom(CACHE_FLAGS)).flatMap(({ words }) => cut(site, words, `nx ${words[0].text} on ${names}`)) : [];
});

const forwardedDuplicates: Fix = Effect.fnUntraced(function* (site, { host }) {
    const configured = Option.flatMap(Option.all([addressOf(site), yield* host.graph]), ([[project, target], held]): Option.Option<string> => Option.fromNullishOr(held.nodes[project]?.data.targets?.[target]?.options?.command));
    if (Option.isNone(configured) || Option.isNone(site.separator)) {
        return [];
    }
    const ran = (yield* bodyOf(configured.value, host)).sites.at(-1);
    const [forwarded] = ran === undefined ? [] : (yield* bodyOf(`${ran.program} ${spoken(site.tail)}`, host)).sites;
    return ran !== undefined && forwarded !== undefined && forwarded.items.length > 0 && forwarded.items.every(({ names }) => names.length > 0) && keysOf(forwarded).isSubsetOf(keysOf(ran)) ? silent(cut(site, [site.separator.value, ...site.tail], `nx -- ${spoken(site.tail)}`)) : [];
});

// --- [TABLES] --------------------------------------------------------------------------

const TASKS: Policy = {
    programs: {
        nx: {
            camelCase: true,
            reports: true,
            flags: ['-c, --configuration=', '-t, --targets=', '--target=', '-p, --projects='],
            fixes: [
                when(has(...CACHE_FLAGS), cacheFlags),
                unassign(/^NX_DAEMON=false$/u, false),
                when(({ tail }) => tail.length > 0, forwardedDuplicates),
                (site) => (launcherStart(site) === site.head ? [] : edited(site, { start: launcherStart(site).start, end: site.head.start }, '')),
                perSubcommand({ show: [hiddenErrors], graph: [hiddenErrors] }),
                looped('nx tasks', 'use one nx run-many', 'show'),
            ],
        },
        tsc: {
            aliases: ['tsgo'],
            flags: ['--pretty=', '--incremental'],
            informational: ['-v', '-b', '--build', '-w', '--watch', '--init', '--showConfig'],
            routes: { selects: ['--project'], config: 'tsconfig.json', ignores: ['--noEmit', '--pretty', '--incremental', '--tsBuildInfoFile'] },
        },
        cargo: { flags: ['-p, --package=', '--profile='], routes: { modes: ['--fix'], selects: ['--package'], config: 'Cargo.toml' } },
        biome: { routes: { rooted: true, modes: ['--write', '--unsafe'] } },
        ruff: { routes: { rooted: true, modes: ['--fix', '--no-fix', '--fix-only', '--no-fix-only', '--unsafe-fixes', '--no-unsafe-fixes', '--check', '--diff'] } },
        'ast-grep': {
            aliases: ['sg'],
            flags: ['-U, --update-all', '-i, --interactive', '--custom-languages=', '--filter=', '--globs=', '--color='],
            routes: { rooted: true, modes: ['--update-all', '--interactive'], direct: ['outline'] },
            fixes: [customLanguages],
        },
        mypy: {
            flags: ['--cache-dir=', '--config-file=', '--python-version='],
            routes: { rooted: true, flat: true, through: 'refurb', ignores: ['--cache-dir', '--no-pretty', '--config-file'] },
        },
        refurb: { flags: ['--config-file=', '--python-version='], informational: ['--explain'], routes: { rooted: true, flat: true }, fixes: [launchedThrough] },
        pytest: { routes: { rooted: true, flat: true, across: true } },
        ty: { flags: ['--python-version='], routes: { rooted: true } },
        vitest: {
            flags: ['--exclude='],
            routes: { ignores: ['--run'] },
            fixes: [looped('vitest', 'use --repeats')],
        },
        depcruise: { flags: ['--config=', '--output-type=', '-T='], routes: { rooted: true, flat: true, entries: true, ignores: ['--config', '--output-type'] } },
    },
    every: [when(({ row }) => row.routes !== undefined, route)],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { TASKS };
