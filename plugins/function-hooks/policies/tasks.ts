import { matchesGlob } from 'node:path';
import process from 'node:process';
import type { ProjectGraph, ProjectGraphProjectNode, TargetConfiguration } from '@nx/devkit';
import { Array, Effect, Option, type Predicate, Record } from 'effect';
import { all, become, type Call, cut, edited, type Fix, feedsJq, has, holds, inserted, type Policy, perSubcommand, quote, redirected, refusal, refuse, respell, silent, spoken, unassign, when, whole } from '#/policies/fix';
import type { Word } from '#/shell/script';
import { analyzed, Shell, type Site } from '#/shell/site';

// --- [TYPES] ---------------------------------------------------------------------------

type Targets = Readonly<Record<string, TargetConfiguration<{ readonly command?: string }>>>;
interface Runner {
    readonly node: ProjectGraphProjectNode;
    readonly target: string;
    readonly verb: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CACHE_FLAGS = new Set(['--skip-nx-cache', '--disable-nx-cache']);
const AGGREGATE = 'nx:noop';
const SEARCH_PATH = 'PYTHONPATH';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [GRAPHS]

const nx = Effect.promise(() => import('@nx/devkit/internal'));

const targetsOf = ({ data }: ProjectGraphProjectNode): Targets => data.targets ?? {};

const bodyOf = (text: string, host: Call['host'], cwd = host.root) => analyzed(text, { cwd: Option.some(cwd), origin: 'body' });

const patternsOf = ({ executor, dependsOn = [] }: Targets[string]): readonly string[] =>
    (executor === AGGREGATE ? dependsOn : []).flatMap((entry) => (typeof entry === 'string' ? [entry] : Option.toArray(Option.liftPredicate(entry.target, () => entry.projects === undefined && entry.dependencies !== true)))).filter((pattern) => !pattern.startsWith('^'));

const members = (targets: Targets, verb: string): readonly string[] => {
    const patterns = Option.match(Record.get(targets, verb), { onNone: () => [], onSome: patternsOf });
    return Object.keys(targets).filter((name) => name !== verb && patterns.some((pattern) => pattern !== name && matchesGlob(name, pattern)));
};

const verbOf = (targets: Targets, target: string): string =>
    Option.getOrElse(
        Array.findFirst(Object.keys(targets), (verb) => members(targets, verb).includes(target)),
        () => target,
    );

// --- [ROUTES]

const keysOf = ({ items }: Site): ReadonlySet<string> => new Set(items.flatMap(({ names, values }) => [...names, ...Array.cartesianWith(names.slice(-1), values, (name, value) => `${name}=${value}`)]));

const programOf = ({ program }: Pick<Site, 'program'>, programs: (typeof Shell.Service)['programs']) => programs.get(program) ?? program;

const placed = (word: Word, cwd: Option.Option<string>, host: Call['host']) =>
    Option.match(
        Option.filter(host.place(word.text, cwd), (place) => word.known && host.inside(place, host.root)),
        {
            onNone: () => Effect.succeed(Option.none<string>()),
            onSome: (place) => Effect.map(host.exists(place), (held) => Option.liftPredicate(place, () => held)),
        },
    );

const scopeOf = Effect.fnUntraced(function* (site: Site, host: Call['host']) {
    const own = site.operands.filter((word) => !site.tail.includes(word));
    const places = yield* Effect.forEach(own, (word) => placed(word, site.command.cwd, host));
    const words = own.filter((_, at) => !Option.isSome(places[at] ?? Option.none())).map(({ text }) => text);
    return { places: Array.getSomes(places), words: [...site.path, ...words].join(' ') };
});

const ownersOf = Effect.fnUntraced(function* ({ nodes }: ProjectGraph, site: Site, places: readonly string[], folder: string, host: Call['host']) {
    const { createProjectRootMappings, findProjectForPath } = yield* nx;
    const roots = createProjectRootMappings(nodes);
    const owner = (place: string) => Option.flatMap(Option.fromNullishOr(findProjectForPath(host.path.relative(host.root, place) || '.', roots)), (name) => Record.get(nodes, name));
    const located = (value: string) =>
        Option.flatMap(
            Option.filter(host.place(value, Option.some(folder)), (place) => host.inside(place, host.root)),
            owner,
        );
    const selected = site.values(...(site.row.routes?.selects ?? [])).map((value) => Option.orElse(Record.get(nodes, value), () => located(value)));
    const named = [...selected, ...places.map(owner)];
    const found = named.length === 0 ? Option.toArray(owner(folder)) : Option.getOrElse(Option.all(named), () => []);
    return [...new Map(found.map((node) => [node.name, node])).values()];
});

const runnersOf = Effect.fnUntraced(function* (node: ProjectGraphProjectNode, site: Site, words: string, host: Call['host']) {
    const { programs } = yield* Shell;
    const program = programOf(site, programs);
    const spellings = new Set([site.program, ...[...programs].filter(([, row]) => row === program).map(([name]) => name)]);
    const helps = Record.toEntries(targetsOf(node)).flatMap(([target, { metadata }]) => Option.toArray(Option.filter(Option.fromNullishOr(metadata?.help?.command), (command) => !spellings.isDisjointFrom(new Set(command.split(' ')))).pipe(Option.map((command) => [target, command] as const))));
    const ran = yield* Effect.forEach(helps, ([, command]) => Effect.map(bodyOf(command, host), ({ sites }) => sites.at(-1)));
    const running = Array.zip(helps, ran).flatMap(([[target], held]) => (held !== undefined && programOf(held, programs) === program && [...held.path, ...held.operands.map(({ text }) => text)].join(' ') === words ? [target] : []));
    const modes = Record.toEntries(site.row.routes?.modes ?? {});
    const named = new Set(modes.filter(([flag]) => site.has(flag)).map(([, verb]) => verb));
    const moded = new Set(modes.map(([, verb]) => verb));
    const targets = targetsOf(node);
    return running.map((target): Runner => ({ node, target, verb: verbOf(targets, target) })).filter(({ verb }) => (named.size > 0 ? named.has(verb) : !moded.has(verb)));
});

const launcherStart = ({ head, outer }: Site): Word => outer[outer.findLastIndex(({ row, path }) => !(row.launcher === true || row.module !== undefined || row.wraps === '--' || path.length > 0)) + 1]?.head ?? head;

const route: Fix = Effect.fnUntraced(function* (site, call) {
    const { host } = call;
    const { command, informational, last, repeated, row } = site;
    const folder = Option.getOrUndefined(command.cwd);
    const settled = new Set([...(row.routes?.selects ?? []), ...Object.keys(row.routes?.modes ?? {}), ...(row.routes?.ignores ?? [])]);
    const extra = site.tail.length > 0 || site.assigned.length > 0 || site.items.some(({ names }) => names.length > 0 && !names.every((name) => settled.has(name)));
    if (folder === undefined || !host.inside(folder, host.root) || command.fed || informational || repeated || command.origin !== 'call' || !last || extra || feedsJq(site, call)) {
        return [];
    }
    const graph = yield* host.graph;
    if (Option.isNone(graph)) {
        return [];
    }
    const { places, words } = yield* scopeOf(site, host);
    const owners = yield* ownersOf(graph.value, site, places, folder, host);
    const runners = yield* Effect.forEach(owners, (node) => runnersOf(node, site, words, host));
    const [first, ...others] = runners.flatMap((held) => (held.length === 1 ? held : []));
    if (first === undefined || others.length + 1 !== owners.length || others.some(({ verb }) => verb !== first.verb)) {
        return [];
    }
    const named = others.length === 0 ? `nx ${first.verb} ${quote(first.node.name)}` : `nx run-many -t ${first.verb} -p ${[first, ...others].map(({ node }) => quote(node.name)).join(',')}`;
    return whole(site, named, site.program, launcherStart(site));
});

const customLanguages: Fix = (site) => {
    const [, subcommand = site.head] = site.words;
    return site.has('--custom-languages') ? [] : inserted(site, (subcommand.text.startsWith('-') ? site.head : subcommand).end, ' --custom-languages allow');
};

const hiddenErrors: Fix = (site) => {
    const nulled = redirected(site, '>', undefined, '/dev/null');
    return [...nulled, ...redirected(site, '&>', undefined, '/dev/null'), ...(nulled.some(({ descriptor }) => Option.isNone(descriptor)) ? redirected(site, '>&', '2', '1') : [])].flatMap((redirect) => cut(site, [redirect]));
};

const looped = (what: string, action: string, ...reads: readonly string[]): Fix =>
    when(
        ({ repeated, operands }) => repeated && !reads.includes(operands[0]?.text ?? ''),
        () => refusal(`Loop reruns ${what}, ${action}`),
    );

// --- [NX]

const addressOf = ({ operands }: Site, { nodes }: ProjectGraph): Option.Option<readonly [string, string]> => {
    const [first, second] = operands.map(({ text }) => text);
    const [project = '', target = ''] = second?.split(':') ?? [];
    if (first === 'run') {
        return project !== '' && target !== '' ? Option.some([project, target] as const) : Option.none();
    }
    return first !== undefined && second !== undefined && nodes[second]?.data.targets?.[first] !== undefined ? Option.some([second, first] as const) : Option.none();
};

const askedTargets = (site: Site, graph: ProjectGraph) => {
    const listed = (...names: readonly string[]) => site.values(...names).flatMap((value) => value.split(','));
    const { projects, targets } = Option.match(addressOf(site, graph), {
        onNone: () => ({ projects: listed('--projects'), targets: listed('--targets', '--target') }),
        onSome: ([project, target]) => ({ projects: [project], targets: [target] }),
    });
    const nodes = projects.length === 0 ? Object.values(graph.nodes) : Array.getSomes(projects.map((project) => Option.fromNullishOr(graph.nodes[project])));
    return Array.cartesian(nodes, targets).filter(([node, target]) => targetsOf(node)[target] !== undefined);
};

const cacheFlags: Fix = Effect.fnUntraced(function* (site, { host }) {
    const asked = Option.match(yield* host.graph, { onNone: () => [], onSome: (held) => askedTargets(site, held) });
    const names = [...new Set(asked.map(([, target]) => target))].join(' ');
    return asked.length > 0 && asked.every(([node, target]) => targetsOf(node)[target]?.cache === true) ? site.items.filter(({ names: flagged }) => !new Set(flagged).isDisjointFrom(CACHE_FLAGS)).flatMap(({ words }) => cut(site, words, `nx ${words[0].text} on ${names}`)) : [];
});

const aggregateArgs: Fix = Effect.fnUntraced(function* (site, { host }) {
    const asked = Option.match(yield* host.graph, { onNone: () => [], onSome: (held) => askedTargets(site, held) });
    const [aggregate] = asked.filter(([held, target]) => patternsOf(targetsOf(held)[target] ?? {}).length > 0);
    if (aggregate === undefined) {
        return [];
    }
    const [node, verb] = aggregate;
    const tools = members(targetsOf(node), verb);
    const [tool] = tools;
    return asked.length === 1 && tool !== undefined && tools.length === 1 ? whole(site, `nx run ${quote(`${node.name}:${tool}`)} -- ${spoken(site.tail)}`, `nx ${verb}`) : refusal(`pass one-run arguments to one of: ${tools.join(',')}`);
});

const forwardedOptions: Fix = Effect.fnUntraced(function* (site, call) {
    const { host } = call;
    const configured = Option.flatMap(yield* host.graph, (graph) =>
        Option.flatMap(addressOf(site, graph), ([project, target]) => {
            const entry = graph.nodes[project]?.data.targets?.[target];
            const configuration = site.values('--configuration').at(-1) ?? entry?.defaultConfiguration;
            return Option.fromNullishOr(entry === undefined ? undefined : { ...entry.options, ...(configuration === undefined ? {} : entry.configurations?.[configuration]) }.command);
        }),
    );
    const ran = yield* Option.match(configured, {
        onNone: () => Effect.undefined,
        onSome: (command) => Effect.map(bodyOf(command, host), ({ sites }) => sites.at(-1)),
    });
    if (ran === undefined) {
        return [];
    }
    const tail = spoken(site.tail);
    const [forwarded] = (yield* bodyOf([ran.program, ...site.values('--args'), tail].join(' '), host)).sites;
    if (forwarded === undefined) {
        return [];
    }
    const guards = ['mypy', 'pytest'].includes(ran.program) ? yield* all(...(forwarded.row.fixes ?? []))(forwarded, call) : [];
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

const BOOT_MODE: Predicate.Predicate<Site> = ({ informational, operands, has: supplied }) => process.platform === 'darwin' && !informational && ['start', 'run'].includes(operands[1]?.text ?? '') && supplied('--force');

const TASKS: Policy = {
    programs: {
        nx: {
            camelCase: true,
            reports: true,
            flags: ['-c, --configuration=', '-t, --targets=', '--target=', '-p, --projects=', '--args='],
            fixes: [
                when(has(...CACHE_FLAGS), cacheFlags),
                unassign(/^NX_DAEMON=false$/u, false),
                when((site) => site.tail.length > 0, aggregateArgs),
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
            routes: { selects: ['--project'], ignores: ['--noEmit', '--pretty', '--incremental'] },
        },
        cargo: { flags: ['-p, --package=', '--profile='], routes: { selects: ['--package'], modes: { '--fix': 'fix' } } },
        uv: { flags: ['--package=', '-U, --upgrade', '--check', '--locked'], routes: { selects: ['--package'], modes: { '--check': 'check', '--upgrade': 'upgrade' }, ignores: ['--locked'] } },
        dotnet: { flags: ['-warnaserror', '--verify-no-changes'], routes: { modes: { '--verify-no-changes': 'check', '-warnaserror': 'check' } } },
        rustfmt: { flags: ['--config-path=', '--config=', '--edition=', '--emit=', '--print-config='] },
        swiftlint: { dump: true },
        ruff: {
            flags: ['--isolated', '--cache-dir='],
            fixes: [projectOptions(respell({ '--isolated': '', '--cache-dir': '' }, { notify: true }), unassign(/^RUFF_CACHE_DIR=/u))],
        },
        'ast-grep': {
            aliases: ['sg'],
            flags: ['-U, --update-all', '-i, --interactive', '--custom-languages=', '--filter=', '--globs=', '--color='],
            fixes: [customLanguages],
        },
        mypy: {
            flags: ['--cache-dir=', '--config-file=', '--python-version='],
            routes: { ignores: ['--no-pretty'] },
            fixes: [projectOptions(respell({ '--cache-dir': '' }, { notify: true }), unassign(/^MYPY_CACHE_DIR=/u))],
        },
        refurb: { flags: ['--config-file=', '--python-version='], informational: ['--explain'], routes: {} },
        pytest: {
            flags: ['-o, --override-ini=', '-c, --config-file=', '--benchmark-storage='],
            routes: {},
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
        ty: { flags: ['--python-version=', '--exclude='], routes: {} },
        python: { fixes: [outsideScript] },
        pulumi: {
            fixes: [
                perSubcommand({
                    up: [refuse('skips stack programs', 'run nx deploy <project>')],
                    refresh: [refuse('skips stack programs', 'run nx deploy <project>')],
                    destroy: [refuse('skips stack programs', 'run nx deploy <project> --destroy')],
                    preview: [refuse('skips stack programs', 'run nx release --dry-run')],
                }),
            ],
        },
        vitest: {
            flags: ['--exclude=', '--mode='],
            routes: { ignores: ['--run'] },
            fixes: [looped('vitest', 'use --repeats')],
        },
        pitchfork: { flags: ['-f, --force'], fixes: [perSubcommand({ supervisor: [when(BOOT_MODE, () => refusal('Forced supervisor restart drops boot mode, use launchd'))] })] },
    },
    every: [route],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { TASKS };
