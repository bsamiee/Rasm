import { devNull } from 'node:os';
import { matchesGlob } from 'node:path';
import process from 'node:process';
import type { ProjectGraph, ProjectGraphProjectNode, TargetConfiguration } from '@nx/devkit';
import { Array, Cache, Effect, Option, type Predicate, pipe, Record, Result, Schema, String } from 'effect';
import { all, become, type Call, cut, edited, ensure, type Fix, feedsJq, folderOf, has, holds, inserted, type Policy, perSubcommand, quote, redirected, refusal, refuse, respell, silent, spoken, unassign, when, whole } from '#/policies/fix';
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
const ADDRESSING = new Set(['--projects', '--targets', '--target']);
const AGGREGATE = 'nx:noop';
const SEARCH_PATH = 'PYTHONPATH';
const PUBLISH = /(?:^|[:,])nx-release-publish(?:,|$)/u;
const SPACES = /\s+/u;
const STUB_CACHE = 'MYPY_CACHE_DIR';
const HOOKS: ReadonlySet<string> = new Set(['check', 'fix']);

// --- [MODELS] --------------------------------------------------------------------------

const Plan = Schema.fromJsonString(Schema.Struct({ steps: Schema.Array(Schema.Struct({ status: Schema.String })) }));

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

const keysOf = ({ items }: Pick<Site, 'items'>): ReadonlySet<string> => new Set(items.flatMap(({ names, values }) => [...names, ...Array.cartesianWith(names.slice(-1), values, (name, value) => `${name}=${value}`)]));

const programOf = ({ program }: Pick<Site, 'program'>, programs: (typeof Shell.Service)['programs']) => programs.get(program) ?? program;

const spellingsOf = (site: Site, programs: (typeof Shell.Service)['programs']): ReadonlySet<string> => new Set([site.program, ...[...programs].filter(([, row]) => row === programOf(site, programs)).map(([name]) => name)]);

const argued = ({ path, operands }: Site): string => [...path, ...operands.map(({ text }) => text)].join(' ');

const placed = (word: Word, cwd: Option.Option<string>, host: Call['host']) =>
    Option.match(
        Option.filter(host.place(word.text, cwd), (place) => word.known && host.inside(place, host.root)),
        {
            onNone: () => Effect.succeed(Option.none<string>()),
            onSome: (place) => Effect.map(host.exists(place), (exists) => Option.liftPredicate(place, () => exists)),
        },
    );

const scopeOf = Effect.fnUntraced(function* (site: Site, host: Call['host']) {
    const own = Array.difference(site.operands, site.tail);
    const places = yield* Effect.forEach(own, (word) => placed(word, site.command.cwd, host));
    const files = own.filter((_, at) => Option.isSome(places[at] ?? Option.none()));
    return { places: Array.getSomes(places), files, words: [...site.path, ...Array.difference(own, files).map(({ text }) => text)].join(' ') };
});

const routable = (site: Site, call: Call): boolean =>
    Option.exists(site.command.cwd, (folder) => call.host.inside(folder, call.host.root)) && site.command.origin === 'call' && site.last && site.tail.length === 0 && site.assigned.length === 0 && !(site.command.fed || site.command.captured || site.informational || site.repeated || feedsJq(site, call));

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
    const spellings = spellingsOf(site, programs);
    const helps = Array.getSomes(
        Record.collect(targetsOf(node), (target, { metadata }) =>
            pipe(
                Option.fromNullishOr(metadata?.help?.command),
                Option.filter((command) => !spellings.isDisjointFrom(new Set(command.split(' ')))),
                Option.map((command) => [target, command] as const),
            ),
        ),
    );
    const ran = yield* Effect.forEach(helps, ([, command]) => Effect.map(bodyOf(command, host), ({ sites }) => sites.at(-1)));
    const running = Array.zip(helps, ran)
        .filter(([, helped]) => helped !== undefined && programOf(helped, programs) === programOf(site, programs) && argued(helped) === words)
        .map(([[target]]) => target);
    const modes = Record.toEntries(site.row.routes?.modes ?? {});
    const named = new Set(modes.filter(([flag]) => site.has(flag)).map(([, verb]) => verb));
    const moded = new Set(modes.map(([, verb]) => verb));
    const targets = targetsOf(node);
    return running.map((target): Runner => ({ node, target, verb: verbOf(targets, target) })).filter(({ verb }) => (named.size > 0 ? named.has(verb) : !moded.has(verb)));
});

const launcherStart = ({ head, outer }: Site): Word => outer[outer.findLastIndex(({ row, path }) => !(row.launcher === true || row.module !== undefined || row.wraps === '--' || path.length > 0)) + 1]?.head ?? head;

const route: Fix = Effect.fnUntraced(function* (site, call) {
    const { host } = call;
    const { command, row } = site;
    const folder = Option.getOrUndefined(command.cwd);
    const settled = new Set([...(row.routes?.selects ?? []), ...Object.keys(row.routes?.modes ?? {}), ...(row.routes?.ignores ?? [])]);
    if (folder === undefined || !routable(site, call) || site.items.some(({ names }) => names.length > 0 && !new Set(names).isSubsetOf(settled))) {
        return [];
    }
    const { places, words } = yield* scopeOf(site, host);
    const owners = yield* Option.match(yield* host.graph, { onNone: () => Effect.succeed<readonly ProjectGraphProjectNode[]>([]), onSome: (graph) => ownersOf(graph, site, places, folder, host) });
    const runners = yield* Effect.forEach(owners, (node) => runnersOf(node, site, words, host));
    const [first, ...others] = runners.flatMap((runner) => (runner.length === 1 ? runner : []));
    if (first === undefined || others.length + 1 !== owners.length || others.some(({ verb }) => verb !== first.verb)) {
        return [];
    }
    const named = others.length === 0 ? `nx ${first.verb} ${quote(first.node.name)}` : `nx run-many -t ${first.verb} -p ${[first, ...others].map(({ node }) => quote(node.name)).join(',')}`;
    return whole(site, named, site.program, launcherStart(site));
});

const hooked: Fix = Effect.fnUntraced(function* (site, call) {
    const { host } = call;
    const { command } = site;
    const { programs } = yield* Shell;
    const merged = redirected(site, '>&', '2', '1').length + redirected(site, '&>').length > 0;
    const heard = merged || !(command.pipes || command.redirects.some(({ op }) => op.startsWith('>')));
    const { places, files, words } = heard && routable(site, call) ? yield* scopeOf(site, host) : { places: [], files: [], words: '' };
    const spellings = spellingsOf(site, programs);
    const runs = (files.length === 0 ? [] : yield* host.steps).filter(({ text }) => !spellings.isDisjointFrom(new Set(text.split(SPACES))));
    const bodies = yield* Effect.forEach(runs, ({ text }) => bodyOf(text, host));
    const ran = bodies.map(({ sites }) => sites.find((other) => programOf(other, programs) === programOf(site, programs)));
    const flags = ({ items }: Site) => keysOf({ items: items.filter(({ names }) => new Set(names).isDisjointFrom(new Set(site.row.routes?.ignores))) });
    const modes = Array.filterMap(Array.zip(runs, ran), ([run, found]) => (found === undefined ? Result.failVoid : Result.succeed({ ...run, keys: flags(found), words: argued(found) })));
    const asked = flags(site);
    const common = (held: typeof modes) => held.slice(1).reduce((shared, { keys }) => shared.intersection(keys), held[0]?.keys ?? new Set<string>());
    const policy = ({ step, mode }: (typeof modes)[number]) => {
        const held = modes.filter((other) => other.step === step);
        return mode === 'fix' ? common(held.length > 1 ? held : []) : common(held.filter((other) => other.words === words));
    };
    const [fit, ...more] = modes.filter((mode) => HOOKS.has(mode.mode) && mode.words === words && asked.isSubsetOf(mode.keys) && mode.keys.difference(policy(mode)).isSubsetOf(asked));
    if (fit === undefined || more.length > 0) {
        return [];
    }
    const plans = yield* Effect.forEach(places, (place) => host.output(['hk', fit.mode, '--step', fit.step, '--plan', '--json', place]), { concurrency: 'unbounded' });
    const included = Array.getSomes(plans.map((plan) => Schema.decodeOption(Plan)(plan))).filter(({ steps }) => steps.some(({ status }) => status === 'included'));
    return included.length === places.length ? whole(site, ['hk', fit.mode, '--step', quote(fit.step), spoken(files)].join(' '), site.program, launcherStart(site)) : [];
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
        ({ repeated, operands }) => repeated && !Option.exists(Array.head(operands), ({ text }) => reads.includes(text)),
        () => refusal(`Loop reruns ${what}, ${action}`),
    );

// --- [NX]

const addressOf = ({ operands }: Site, { nodes }: ProjectGraph): Option.Option<readonly [string, string]> => {
    const [first, second] = operands.map(({ text }) => text);
    const [project, target] = second?.split(':') ?? [];
    if (first === 'run') {
        return Option.filter(Option.all([Option.fromUndefinedOr(project), Option.fromUndefinedOr(target)] as const), Array.every(String.isNonEmpty));
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
    const asked = Option.match(yield* host.graph, { onNone: () => [], onSome: (graph) => askedTargets(site, graph) });
    const names = [...new Set(asked.map(([, target]) => target))].join(' ');
    const cached = asked.every(([node, target]) => [target, ...members(targetsOf(node), target)].every((name) => patternsOf(targetsOf(node)[name] ?? {}).length > 0 || targetsOf(node)[name]?.cache === true));
    return asked.length > 0 && cached ? site.items.filter(({ names: flagged }) => !new Set(flagged).isDisjointFrom(CACHE_FLAGS)).flatMap(({ words }) => cut(site, words, `nx ${words[0].text} on ${names}`)) : [];
});

const aggregateArgs: Fix = Effect.fnUntraced(function* (site, { host }) {
    const asked = Option.match(yield* host.graph, { onNone: () => [], onSome: (graph) => askedTargets(site, graph) });
    const [aggregate] = asked.filter(([asking, target]) => patternsOf(targetsOf(asking)[target] ?? {}).length > 0);
    if (aggregate === undefined) {
        return [];
    }
    const [node, verb] = aggregate;
    const tools = members(targetsOf(node), verb);
    const [tool] = tools;
    const kept = site.items.filter(({ names }) => names.length > 0 && new Set(names).isDisjointFrom(ADDRESSING)).map(({ words }) => spoken(words));
    return asked.length === 1 && tool !== undefined && tools.length === 1 ? whole(site, ['nx run', quote([node.name, tool].join(':')), ...kept, '--', spoken(site.tail)].join(' '), `nx ${verb}`) : refusal(`pass one-run arguments to one of: ${tools.join(',')}`);
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

const strayCache: Fix = (site, { host }) =>
    Effect.map(
        Shell.use(({ ignored }) => Cache.get(ignored, Array.getSomes(site.values('--cache-dir').map((value) => host.place(value, site.command.cwd))))),
        (kept) => (kept ? [] : respell({ drops: ['--cache-dir'] }, { notify: true })(site)),
    );

const stubCache: Fix = (site) => {
    const set = [...site.outer, site].some(({ assigned }) => assigned.some(({ text }) => text.startsWith(`${STUB_CACHE}=`))) || site.command.bound.has(STUB_CACHE);
    return set ? [] : inserted(site, launcherStart(site).start, `${STUB_CACHE}=${devNull} `);
};

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
    const place = script?.known === true && !site.has(...(row.code ?? [])) ? Option.filter(host.place(script.text, cwd), (scriptPlace) => !host.inside(scriptPlace, host.root)) : Option.none();
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

const CONFIGURED = when(holds(/^[^=]+$/u, '--config'), (site) => (site.has('--no-cache') ? [] : inserted(site, (site.operands.at(0) ?? site.head).end, ' --no-cache')));

const BOOT_MODE: Predicate.Predicate<Site> = ({ informational, operands, has: supplied }) => process.platform === 'darwin' && !informational && Option.exists(Array.get(operands, 1), ({ text }) => ['start', 'run'].includes(text)) && supplied('--force');

const TASKS: Policy = {
    programs: {
        nx: {
            camelCase: true,
            reports: true,
            flags: ['-c, --configuration=', '-t, --targets=', '--target=', '-p, --projects=', '--args='],
            fixes: [
                when(has(...CACHE_FLAGS), cacheFlags),
                when((site) => holds(PUBLISH)(site) || holds(PUBLISH, '--targets', '--target')(site), refuse('publishes dependencies for real', 'run nx release')),
                unassign(/^NX_DAEMON=false$/u),
                when((site) => site.tail.length > 0, aggregateArgs),
                when((site) => site.tail.length > 0 || site.has('--args'), forwardedOptions),
                (site) => (launcherStart(site) === site.head ? [] : edited(site, { start: launcherStart(site).start, end: site.head.start }, '')),
                perSubcommand({ show: [hiddenErrors], graph: [hiddenErrors], reset: [(site, { host }) => (Option.exists(folderOf(host, site), (place) => host.inside(place, host.root)) ? refuse('drops the graph', 'fix the inputs')(site) : [])] }),
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
        uv: {
            flags: ['--package=', '--script=', '--all-packages', '-U, --upgrade', '--check', '--locked'],
            routes: { selects: ['--package'], modes: { '--check': 'check', '--upgrade': 'upgrade' }, ignores: ['--locked'] },
            fixes: [perSubcommand({ sync: [ensure('--all-packages', '--package', '--script')] })],
        },
        dotnet: { flags: ['-warnaserror', '--verify-no-changes'], routes: { modes: { '--verify-no-changes': 'check', '-warnaserror': 'check' } } },
        rustfmt: { flags: ['--config-path=', '--config=', '--edition=', '--emit=', '--print-config='] },
        swiftlint: { dump: true },
        ruff: {
            flags: ['--isolated', '--cache-dir=', '--config=', '-n, --no-cache'],
            routes: { ignores: ['--quiet'] },
            fixes: [projectOptions(respell({ drops: ['--isolated'] }, { notify: true }), strayCache, unassign(/^RUFF_CACHE_DIR=/u), perSubcommand({ check: [CONFIGURED], format: [CONFIGURED] }))],
        },
        yamllint: { flags: ['-c, --config-file=', '-f, --format='] },
        squawk: { flags: ['-c, --config=', '--reporter='] },
        'ast-grep': {
            aliases: ['sg'],
            flags: ['-U, --update-all', '-i, --interactive', '--custom-languages=', '--filter=', '--globs=', '--color=', '--report-style='],
            routes: { ignores: ['--report-style'] },
            fixes: [customLanguages],
        },
        stubgen: { fixes: [projectOptions(stubCache)] },
        mypy: {
            flags: ['--cache-dir=', '--config-file=', '--python-version='],
            routes: { ignores: ['--no-pretty'] },
            fixes: [projectOptions(strayCache, unassign(/^MYPY_CACHE_DIR=/u))],
        },
        refurb: { flags: ['--config-file=', '--python-version='], informational: ['--explain'], routes: {} },
        pytest: {
            flags: ['-o, --override-ini=', '-c, --config-file=', '--benchmark-storage='],
            routes: {},
            fixes: [
                projectOptions(
                    respell({ drops: ['--override-ini'] }, { only: /^addopts=$/u, notify: true }),
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
    every: [route, hooked],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { TASKS };
