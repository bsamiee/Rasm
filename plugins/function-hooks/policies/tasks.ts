import process from 'node:process';
import type { ProjectGraph, ProjectGraphProjectNode, TargetConfiguration } from '@nx/devkit';
import { Array, Cache, Effect, Equal, Option, type Predicate, Record, String } from 'effect';
import { all, become, type Call, cut, edited, ensure, type Fix, feedsJq, has, holds, inserted, type Policy, perSubcommand, quote, redirected, refusal, refuse, respell, silent, spoken, unassign, when, whole } from '#/policies/fix.ts';
import type { Word } from '#/shell/script.ts';
import { analyzed, Shell, type Site } from '#/shell/site.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Targets = Readonly<Record<string, TargetConfiguration<Options>>>;
interface Options {
    readonly command?: string;
    readonly commands?: readonly (string | { readonly command: string; readonly forwardAllArgs?: boolean })[];
    readonly args?: string | readonly string[];
    readonly cwd?: string;
    readonly forwardAllArgs?: boolean;
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
const SEARCH_PATH = 'PYTHONPATH';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [GRAPHS]

const nx = Effect.promise(() => import('@nx/devkit/internal'));

const targetsOf = ({ data }: ProjectGraphProjectNode): Targets => data.targets ?? {};

const optionsOf = ({ options, configurations, defaultConfiguration }: Targets[string], configuration = defaultConfiguration): Options => (configuration === undefined ? (options ?? {}) : { ...options, ...configurations?.[configuration] });

const argsOf = (options: Options): readonly string[] => Array.ensure(options.args ?? []);

const fileFolder = (place: string, host: Call['host']): string => (host.path.extname(place) === '' ? place : host.path.dirname(place));

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

const scopeOf = (site: Site, host: Call['host'], unnamed: readonly string[] = []) => {
    const named = site.row.routes?.entries === true ? [] : Array.getSomes(site.operands.slice(subcommandWords(site), site.operands.length - site.tail.length).map(({ text }) => host.place(text, site.command.cwd)));
    return { places: named.length === 0 && site.row.routes?.rooted === true ? unnamed : named, extra: site.tail.map(({ text }) => text) };
};

const subcommandOf = ({ row, path, operands }: Site): Option.Option<string> => (row.routes?.flat === true ? Option.none() : Array.head([...path, ...operands.map(({ text }) => text)]));

const bodyOf = (text: string, host: Call['host'], cwd = host.root) => analyzed(text, { cwd: Option.some(cwd), origin: 'body' });

const commandsOf = (target: string, configuration: Targets[string]) => {
    const options = optionsOf(configuration);
    const args = argsOf(options);
    const [entry, ...others] = options.command === undefined ? (options.commands ?? []) : [options.command];
    if (entry === undefined || others.length > 0 || (configuration.syncGenerators?.length ?? 0) > 0) {
        return Option.none();
    }
    const { command, forwardAllArgs = options.forwardAllArgs ?? true } = typeof entry === 'string' ? { command: entry } : entry;
    return Option.some({ target, cwd: options.cwd, forwardAllArgs, narrows: !args.every(String.startsWith('-')), text: [command, ...(forwardAllArgs ? args : [])].join(' ') });
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
    const entries = Array.getSomes(Record.collect(targetsOf(node), commandsOf)).filter(({ text }) => named.test(text) && !text.includes('{args'));
    const bodies = yield* Effect.forEach(entries, ({ text, cwd }) => bodyOf(text, host, host.path.resolve(host.root, cwd ?? '.')), { concurrency: 'unbounded' });
    return Array.zip(entries, bodies)
        .filter(([, { commands }]) => commands.length === 1)
        .flatMap(([entry, { sites }]) => Array.cartesian([entry], sites))
        .map(([{ target, forwardAllArgs, narrows }, ran]): Run => ({ target, forwardAllArgs, narrows, site: ran, keys: keysOf(ran), ...scopeOf(ran, host) }))
        .filter((run) => run.site.row === routedRow && Equal.equals(subcommandOf(run.site), subcommandOf(site)));
});

const owners = Effect.fnUntraced(function* ({ nodes }: ProjectGraph, host: Call['host'], site: Site, folder: string) {
    const { row, values } = site;
    if (row.routes?.across === true) {
        return Object.values(nodes);
    }
    const { createProjectRootMappings, findProjectForPath } = yield* nx;
    const roots = createProjectRootMappings(nodes);
    const placeOwner = (place: string) => Option.flatMap(Option.fromNullishOr(host.inside(place, host.root) ? findProjectForPath(host.path.relative(host.root, place) || '.', roots) : undefined), (name) => Record.get(nodes, name));
    if (row.routes?.rooted === true) {
        const root = placeOwner(host.root);
        const nearest = yield* Effect.forEach(scopeOf(site, host, [folder]).places, (place) => Option.match(placeOwner(place), { onNone: () => Effect.succeed(root), onSome: (node) => Effect.map(runsOf(node, site, host), (runs) => (runs.length > 0 ? Option.some(node) : root)) }));
        return Array.dedupe(Array.getSomes(nearest));
    }
    const named = values(...(row.routes?.selects ?? []));
    return named.length === 0 ? Option.toArray(placeOwner(folder)) : Array.getSomes(named.map((value) => Option.orElse(Record.get(nodes, value), () => Option.flatMap(host.place(value, Option.some(folder)), placeOwner))));
});

// --- [FITS]

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
    const { modes = [], selects = [], ignores = [], requires, rooted } = site.row.routes ?? {};
    const atRoot = rooted === true && node.data.root === '.';
    const found = runs.filter((ran) => (requires === undefined || ran.site.has(requires)) && modes.every((flag) => site.has(flag) === ran.site.has(flag)) && chosenFile(site, host) === chosenFile(ran.site, host));
    const known = new Set(found.flatMap((ran) => [...ran.site.names]));
    const wanted = keysOf(site, (name) => known.has(name) && !selects.includes(name) && !ignores.includes(name));
    const { places, extra } = scopeOf(site, host, [folder]);
    const covers = (folders: readonly string[]): boolean => places.every((file) => folders.some((held) => host.inside(file, held)));
    const fits = found.filter((ran) => wanted.isSubsetOf(ran.keys) && new Set(extra).isSubsetOf(new Set(ran.extra)) && (ran.places.length === 0 || (places.length > 0 && covers(ran.places))));
    const fitting = places.length > 0 && fits.some(({ places: held }) => held.length > 0) ? fits.filter(({ places: held }) => held.length > 0) : fits;
    const dropped = new Set([...known, ...selects, ...ignores]);
    const [skipped] = site.items.filter(({ names }) => names.length === 0).slice(0, subcommandWords(site));
    const rest = site.items.filter((item) => item !== skipped && (item.names.length === 0 ? site.row.routes?.entries !== true : !new Set(item.names).isSubsetOf(dropped) && new Set(valueKeys(item.names, item.values)).isDisjointFrom(dropped)));
    const switched = rest.some(({ names }) => names.length > 0);
    const accepting = switched ? fitting.filter((ran) => Option.isNone(ran.site.separator) || ran.site.row !== site.row) : fitting;
    const fewest = Math.min(...accepting.map(({ keys }) => keys.size));
    const [target, ...others] = Array.dedupe(accepting.filter(({ keys }) => keys.size === fewest).map(({ target: name }) => name));
    const configuration = target === undefined ? undefined : targets[target];
    if (target === undefined || others.length > 0 || configuration === undefined) {
        return Option.none();
    }
    const running = accepting.filter(({ target: name }) => name === target);
    const narrowing = running.some(({ narrows }) => narrows);
    const scoped = running.some(({ places: held, narrows }) => held.length > 0 && !narrows);
    const moved = atRoot && folder !== host.root;
    const passed = Option.all(scoped && !switched ? [] : rest.flatMap(({ words }) => words).map((word) => forwardedWord(word, moved, folder, host)));
    const forwarded = [...Option.getOrElse(passed, () => []), ...(moved && !scoped && rest.length === 0 ? [quote(host.path.relative(host.root, folder))] : [])];
    const refused =
        (forwarded.length > 0 && running.some(({ forwardAllArgs }) => !forwardAllArgs)) ||
        rest.some(({ words }) => words.some(({ known: fixed, raw }) => !fixed && quote(raw) !== raw)) ||
        (!atRoot && forwarded.length > 0 && running.some(({ site: ran }) => !Option.contains(ran.command.cwd, folder))) ||
        (moved && switched) ||
        rest.flatMap(({ words, values }) => [...values, ...words.map(({ text }) => text)]).some((text) => host.path.isAbsolute(text) && !host.inside(text, host.root)) ||
        Option.isNone(passed) ||
        !covers([host.root]) ||
        (!scoped && Option.isSome(site.separator));
    return refused ? Option.none() : Option.some({ project: node.name, target, args: forwarded.slice(0, 1).map(() => `--args=${quote([...argsOf(optionsOf(configuration)).filter((arg) => !narrowing || arg.startsWith('-')), ...forwarded].join(' '))}`) });
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
    const { command, informational, last, repeated } = site;
    const folder = Option.getOrUndefined(command.cwd);
    const required = site.row.routes?.requires;
    if (folder === undefined || !host.inside(folder, host.root) || command.fed || informational || repeated || command.origin !== 'call' || !last || feedsJq(site, call) || (required !== undefined && !site.has(required))) {
        return [];
    }
    const owned = yield* Option.match(yield* host.graph, { onNone: () => Effect.succeed([]), onSome: (held) => owners(held, host, site, folder) });
    const nodes = (yield* Effect.forEach(owned, (node) => readsOwnConfig(node, site, folder, host))).every(Boolean) ? owned : [];
    const runs = yield* Effect.forEach(nodes, (node) => runsOf(node, site, host), { concurrency: 'unbounded' });
    const fits = Array.zipWith(nodes, runs, (node, held) => fitted(site, host, node, held, folder));
    const [first, ...others] = site.row.routes?.across === true ? Array.getSomes(fits) : Option.getOrElse(Option.all(fits), () => []);
    if (first === undefined || (others.length > 0 && (first.args.length > 0 || others.some(({ target, args }) => target !== first.target || args.length > 0)))) {
        return [];
    }
    const named = others.length === 0 ? `nx run ${first.project}:${first.target}` : `nx run-many -t ${first.target} -p ${Array.dedupe([first, ...others].map(({ project }) => project)).join(',')}`;
    return whole(site, [named, ...first.args].join(' '), site.program, launcherStart(site));
});

const launchedThrough: Fix = Effect.fnUntraced(function* (site, { host }) {
    if (site.outer.length > 0) {
        return [];
    }
    const [owner] = yield* Option.match(yield* host.graph, { onNone: () => Effect.succeed([]), onSome: (held) => owners(held, host, site, host.root) });
    const launches = owner === undefined ? [] : (yield* runsOf(owner, site, host)).map(({ site: { command, head } }) => spoken(command.words.filter(({ start }) => start <= head.start)));
    return Option.match(
        Array.findFirst(launches, (launch) => launch !== site.program),
        { onNone: () => [], onSome: (launch) => edited(site, site.head, launch, site.program) },
    );
});

const customLanguages: Fix = (site) => {
    const [, subcommand = site.head] = site.words;
    return site.has('--custom-languages') ? [] : inserted(site, (subcommand.text.startsWith('-') ? site.head : subcommand).end, ' --custom-languages allow');
};

const configFlag = (flag: `--${string}`, file: string, forwarded = false): Fix =>
    Effect.fnUntraced(function* (site, { host }) {
        const place = host.path.join(host.root, '.config', file);
        const folder = Option.getOrUndefined(site.command.cwd);
        const grammar = yield* Shell.use(({ grammars }) => Cache.get(grammars, site.program));
        const reads = forwarded || site.path.length === 0 || site.path.reduce((deeper, name) => deeper.commands.get(name) ?? deeper, grammar).options.has(flag);
        const held = forwarded ? site.tail.some(({ text }) => text === flag || text.startsWith(`${flag}=`)) : site.has(flag);
        if (!reads || held || folder === undefined || !host.inside(folder, host.root) || !(yield* host.exists(place))) {
            return [];
        }
        const words = `${flag} ${quote(host.path.relative(folder, place))}`;
        return forwarded
            ? inserted(
                  site,
                  Option.getOrElse(
                      Option.map(site.separator, ({ end }) => end),
                      () => Array.lastNonEmpty(site.words).end,
                  ),
                  `${Option.isSome(site.separator) ? '' : ' --'} ${words}`,
              )
            : ensure(words)(site);
    });

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
    return asked.length > 0 && asked.every(([node, target]) => targetsOf(node)[target]?.cache === true) ? site.items.filter(({ names: flagged }) => !new Set(flagged).isDisjointFrom(CACHE_FLAGS)).flatMap(({ words }) => cut(site, words, `nx ${words[0].text} on ${names}`)) : [];
});

const forwardedOptions: Fix = Effect.fnUntraced(function* (site, call) {
    const { host } = call;
    const configured = Option.flatMap(Option.all([addressOf(site), yield* host.graph]), ([[project, target], graph]) => {
        const entry = graph.nodes[project]?.data.targets?.[target];
        return Option.fromNullishOr(entry === undefined ? undefined : optionsOf(entry, site.values('--configuration').at(-1)).command);
    });
    const ran = yield* Option.match(configured, {
        onNone: () => Effect.undefined,
        onSome: (command) => Effect.map(bodyOf(command, host), ({ sites }) => sites.at(-1)),
    });
    if (ran === undefined) {
        return [];
    }
    const { programs } = yield* Shell;
    const reached = Array.findFirst([...programs], ([, row]) => row.routes?.through === ran.program).pipe(
        Option.map(([name]) => name),
        Option.getOrElse(() => ran.program),
    );
    const tail = spoken(site.tail);
    const [forwarded] = (yield* bodyOf([reached, ...site.values('--args'), tail].join(' '), host)).sites;
    if (forwarded === undefined) {
        return [];
    }
    const guards = ['mypy', 'pytest'].includes(reached) ? yield* all(...(forwarded.row.fixes ?? []))(forwarded, call) : [];
    return [...guards, ...(Option.isSome(site.separator) && !site.has('--args') && forwarded.items.length > 0 && forwarded.items.every(({ names }) => names.length > 0) && keysOf(forwarded).isSubsetOf(keysOf(ran)) ? silent(cut(site, [site.separator.value, ...site.tail], `nx -- ${tail}`)) : [])];
});

// --- [PYTHON]

const projectOptions =
    (...fixes: readonly Fix[]): Fix =>
    (site, call) =>
        Option.exists(site.command.cwd, (folder) => call.host.inside(folder, call.host.root)) ? all(...fixes)(site, call) : [];

const workingFolder = ({ outer, command }: Site, host: Call['host']): Option.Option<string> =>
    outer
        .filter(({ program }) => program === 'env')
        .flatMap(({ values }) => values('-C'))
        .reduce((folder, value) => host.place(value, folder), command.cwd);

const searchPath = ({ assigned, command }: Site): Option.Option<readonly string[]> =>
    Option.map(
        Option.match(
            Array.findLast(assigned, ({ text }) => text.startsWith(`${SEARCH_PATH}=`)),
            {
                onNone: () => command.bound.get(SEARCH_PATH) ?? Option.some(''),
                onSome: ({ known, text }) => Option.liftPredicate(text.slice(SEARCH_PATH.length + 1), () => known),
            },
        ),
        (value) => (value === '' ? [] : value.split(':')),
    );

const anchored = Effect.fnUntraced(function* (word: Word, cwd: Option.Option<string>, host: Call['host']) {
    const place = Option.filter(host.place(word.text, cwd), () => !host.path.isAbsolute(word.text));
    return Option.isSome(place) && (yield* host.exists(place.value)) ? Option.some(quote(place.value)) : Option.none();
});

const outsideScript: Fix = Effect.fnUntraced(function* (site, { host }) {
    const { head, rest, row } = site;
    const [script] = site.operands;
    const cwd = workingFolder(site, host);
    const place = script?.known === true && !site.has(...(row.code ?? [])) ? Option.filter(host.place(script.text, cwd), (held) => !host.inside(held, host.root)) : Option.none();
    if (script === undefined || Option.isNone(place) || !Option.exists(cwd, (folder) => host.inside(folder, host.root))) {
        return [];
    }
    const entries = searchPath(site);
    if (Option.isNone(entries) || rest.some(({ known }) => !known)) {
        return refuse('arguments unresolved', `pass literal words and ${SEARCH_PATH}`)(site);
    }
    const launched = head.text.includes('/') ? Option.getOrElse(yield* anchored(head, cwd, host), () => head.raw) : head.raw;
    const search = entries.value.some((entry) => !host.path.isAbsolute(entry)) ? [`${SEARCH_PATH}=${quote(Array.getSomes(entries.value.map((entry) => host.place(entry, cwd))).join(':'))}`] : [];
    const moved = yield* Effect.forEach([script, ...rest], (word) =>
        Effect.map(
            anchored(word, cwd, host),
            Option.map((text) => edited(site, word, text)),
        ),
    );
    return [...become(['env -C', quote(host.path.dirname(place.value)), ...search, launched].join(' '))(site), ...Array.getSomes(moved).flat()];
});

// --- [TABLES] --------------------------------------------------------------------------

const TASKS: Policy = {
    programs: {
        nx: {
            camelCase: true,
            reports: true,
            flags: ['-c, --configuration=', '-t, --targets=', '--target=', '-p, --projects=', '--args='],
            fixes: [
                when(has(...CACHE_FLAGS), cacheFlags),
                unassign(/^NX_DAEMON=false$/u, false),
                when((site) => site.tail.length > 0 || site.has('--args'), forwardedOptions),
                (site) => (launcherStart(site) === site.head ? [] : edited(site, { start: launcherStart(site).start, end: site.head.start }, '')),
                perSubcommand({ show: [hiddenErrors], graph: [hiddenErrors], reset: [refuse('drops the graph', 'fix the inputs')] }),
                looped('nx tasks', 'use one nx run-many', 'show'),
            ],
        },
        tsc: {
            aliases: ['tsgo'],
            flags: ['--pretty=', '--incremental'],
            informational: ['-v', '-b', '--build', '-w', '--watch', '--init', '--showConfig'],
            routes: { selects: ['--project'], config: 'tsconfig.json', ignores: ['--noEmit', '--pretty', '--incremental', '--tsBuildInfoFile'] },
        },
        cargo: {
            flags: ['-p, --package=', '--profile='],
            routes: { modes: ['--fix', '--check'], selects: ['--package'], config: 'Cargo.toml' },
            fixes: [when(({ path, operands }) => [...path, ...operands.map(({ text }) => text)].find((word) => !word.startsWith('+')) === 'fmt', configFlag('--config-path', 'rustfmt.toml', true))],
        },
        rustfmt: { flags: ['--config-path=', '--config=', '--edition=', '--emit=', '--print-config='], fixes: [configFlag('--config-path', 'rustfmt.toml')] },
        swiftlint: { dump: true, fixes: [configFlag('--config', 'swiftlint.yml')] },
        ruff: {
            flags: ['--isolated', '--cache-dir='],
            fixes: [projectOptions(respell({ '--isolated': '', '--cache-dir': '' }, { notify: true }), unassign(/^RUFF_CACHE_DIR=/u))],
        },
        'ast-grep': {
            aliases: ['sg'],
            flags: ['-U, --update-all', '-i, --interactive', '--custom-languages=', '--filter=', '--globs=', '--color='],
            routes: { rooted: true, modes: ['--interactive'], requires: '--update-all' },
            fixes: [customLanguages],
        },
        mypy: {
            flags: ['--cache-dir=', '--config-file=', '--python-version='],
            routes: { rooted: true, flat: true, through: 'rasm-refurb', ignores: ['--cache-dir', '--no-pretty', '--config-file'] },
            fixes: [projectOptions(respell({ '--cache-dir': '' }, { notify: true }), unassign(/^MYPY_CACHE_DIR=/u))],
        },
        'rasm-refurb': { aliases: ['refurb'], flags: ['--config-file=', '--python-version='], informational: ['--explain'], routes: { rooted: true, flat: true }, fixes: [launchedThrough] },
        pytest: {
            flags: ['-o, --override-ini=', '-c, --config-file=', '--benchmark-storage='],
            routes: { rooted: true, flat: true, across: true, ignores: ['--override-ini=addopts='] },
            fixes: [
                projectOptions(
                    respell({ '--override-ini': '' }, { only: /^addopts=$/u, notify: true }),
                    when(holds(/^(?:addopts=.+|cache_dir=)/u, '--override-ini'), () => refusal('Pytest overrides project options, pass test flags directly')),
                    when(has('--config-file', '--benchmark-storage'), () => refusal('Pytest must use project configuration and benchmark storage')),
                    when(
                        ({ assigned }) => assigned.some(({ text }) => text.startsWith('PYTEST_ADDOPTS=')),
                        () => refusal('Pass pytest options directly to preserve project defaults'),
                    ),
                ),
            ],
        },
        ty: { flags: ['--python-version=', '--exclude='], routes: { rooted: true } },
        python: { fixes: [outsideScript] },
        pulumi: { fixes: [when(({ path, operands }) => ['up', 'destroy', 'refresh', 'preview'].includes(path[0] ?? operands[0]?.text ?? ''), refuse('skips the stack program', 'run <project>:infra'))] },
        mise: { fixes: [perSubcommand({ upgrade: [refuse('moves tools alone', 'run mise run update')], up: [refuse('moves tools alone', 'run mise run update')] })] },
        vitest: {
            flags: ['--exclude=', '--mode='],
            routes: { modes: ['--mode'], ignores: ['--run'] },
            fixes: [looped('vitest', 'use --repeats')],
        },
        pitchfork: {
            flags: ['-f, --force'],
            fixes: [
                perSubcommand({
                    supervisor: [
                        when(
                            ({ informational, operands, has: supplied }) => {
                                const verb = operands[1]?.text;
                                return process.platform === 'darwin' && !informational && (verb === 'start' || verb === 'run') && supplied('--force');
                            },
                            () => refusal('Forced supervisor restart drops boot mode, use launchd'),
                        ),
                    ],
                }),
            ],
        },
        depcruise: { flags: ['--config=', '--output-type=', '-T='], routes: { rooted: true, flat: true, entries: true, ignores: ['--config', '--output-type'] } },
    },
    every: [when(({ row }) => row.routes !== undefined, route)],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { TASKS };
