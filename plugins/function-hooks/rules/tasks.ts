import { devNull } from 'node:os';
import { matchesGlob } from 'node:path';
import process from 'node:process';
import type { ProjectGraphProjectNode, TargetConfiguration } from '@nx/devkit';
import { Array, Effect, Option, Predicate, pipe, Record, Result, Schema, String } from 'effect';
import { all, type Call, cut, edited, ensure, feedsJq, holds, inserted, perSubcommand, quote, type Rule, type Ruleset, redirected, refusal, refuse, respell, silent, spelled, unassign, when, whole } from '#/rules/rule';
import { analyzed, type Invocation } from '#/shell/invocation';
import type { Word } from '#/shell/script';

// --- [TYPES] ---------------------------------------------------------------------------

type Targets = Readonly<Record<string, TargetConfiguration<Readonly<Record<string, unknown>>>>>;
type Graph = Option.Option.Value<Effect.Success<Call['host']['graph']>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const LONG = /^--/u;
const PUBLISH = /(?:^|[:,])nx-release-publish(?:,|$)/u;
const STUB_CACHE = 'MYPY_CACHE_DIR';

// --- [MODELS] --------------------------------------------------------------------------

const Plan = Schema.fromJsonString(Schema.Struct({ steps: Schema.Array(Schema.Struct({ status: Schema.String })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [GRAPH]

const targetsOf = ({ data }: ProjectGraphProjectNode): Targets => data.targets ?? {};

const bodyOf = (text: string, { host }: Call) => analyzed(text, { cwd: Option.some(host.root), origin: 'body' });

const patternsOf = ({ executor, dependsOn = [] }: Targets[string]): readonly string[] =>
    (executor === 'nx:noop' ? dependsOn : []).flatMap((entry) => (typeof entry === 'string' ? [entry] : Option.toArray(Option.liftPredicate(entry.target, () => entry.projects === undefined && entry.dependencies !== true)))).filter((pattern) => !pattern.startsWith('^'));

const members = (targets: Targets, aggregate: string): readonly string[] => {
    const patterns = Option.match(Record.get(targets, aggregate), { onNone: () => [], onSome: patternsOf });
    return Object.keys(targets).filter((name) => name !== aggregate && patterns.some((pattern) => pattern !== name && matchesGlob(name, pattern)));
};

const aggregateOf = (targets: Targets, target: string): string =>
    Option.getOrElse(
        Array.findFirst(Object.keys(targets), (aggregate) => members(targets, aggregate).includes(target)),
        () => target,
    );

// --- [NX_TARGETS]

const keysOf = ({ args }: Pick<Invocation, 'args'>): ReadonlySet<string> => new Set(args.flatMap(({ names, values }) => [...names, ...Array.cartesianWith(names.slice(-1), values, (name, value) => `${name}=${value}`)]));

const programOf = ({ program }: Invocation, { programs }: Call) => programs.get(program) ?? program;

const spellingsOf = (invocation: Invocation, call: Call): ReadonlySet<string> => new Set([invocation.program, ...[...call.programs].filter(([, row]) => row === programOf(invocation, call)).map(([name]) => name)]);

const argued = ({ path, operands }: Invocation): string => [...path, ...operands.map(({ text }) => text)].join(' ');

const scopeOf = Effect.fnUntraced(function* (invocation: Invocation, { host }: Call) {
    const own = Array.difference(invocation.operands, invocation.tail);
    const inside = own.map(({ text, known }) => Option.filter(host.resolve(text, invocation.command.cwd), (file) => known && host.inside(file, host.root)));
    const present = yield* Effect.forEach(inside, Option.match({ onNone: () => Effect.succeed(false), onSome: host.exists }));
    const files = own.filter((_, at) => present[at] === true);
    return { paths: Array.getSomes(inside.filter((_, at) => present[at] === true)), files, words: [...invocation.path, ...Array.difference(own, files).map(({ text }) => text)].join(' ') };
});

const replaceable = (invocation: Invocation, call: Call): boolean =>
    Option.exists(invocation.command.cwd, (folder) => call.host.inside(folder, call.host.root)) &&
    invocation.command.origin === 'call' &&
    invocation.last &&
    invocation.tail.length === 0 &&
    invocation.assigned.length === 0 &&
    !(invocation.command.fed || invocation.command.captured || invocation.repeated || feedsJq(invocation, call));

const ownersOf = ({ nodes, owner }: Graph, invocation: Invocation, paths: readonly string[], call: Call) => {
    const { host } = call;
    const { cwd } = invocation.command;
    const located = (value: string) =>
        Option.flatMap(
            Option.filter(host.resolve(value, cwd), (file) => host.inside(file, host.root)),
            owner,
        );
    const spellings = spellingsOf(invocation, call);
    const { selects = [] } = invocation.row.nx ?? {};
    const run = Object.values(nodes)
        .flatMap((node) => Object.values(targetsOf(node)).map((target) => [node, target] as const))
        .filter(([, { metadata }]) => !spellings.isDisjointFrom(new Set(metadata?.help?.command.split(' '))));
    const stated = new Map(Array.cartesianWith(run, selects, ([node, { options = {} }], flag) => [options[flag.replace(LONG, '')], node] as const));
    const selected = invocation.values(...selects).map((value) => Option.firstSomeOf([Option.fromUndefinedOr(stated.get(value)), Record.get(nodes, value), located(value)]));
    const named = [...selected, ...paths.map(owner)];
    const found = named.length === 0 ? Option.toArray(Option.flatMap(cwd, owner)) : Option.getOrElse(Option.all(named), () => []);
    return [...new Map(found.map((node) => [node.name, node])).values()];
};

const runnersOf = Effect.fnUntraced(function* (node: ProjectGraphProjectNode, invocation: Invocation, words: string, call: Call) {
    const spellings = spellingsOf(invocation, call);
    const helps = Array.getSomes(
        Record.collect(targetsOf(node), (target, { metadata }) =>
            pipe(
                Option.fromNullishOr(metadata?.help?.command),
                Option.filter((command) => !spellings.isDisjointFrom(new Set(command.split(' ')))),
                Option.map((command) => [target, command] as const),
            ),
        ),
    );
    const ran = yield* Effect.forEach(helps, ([, command]) => Effect.map(bodyOf(command, call), ({ invocations }) => invocations.at(-1)));
    const running = Array.zip(helps, ran)
        .filter(([, helped]) => helped !== undefined && programOf(helped, call) === programOf(invocation, call) && argued(helped) === words)
        .map(([[target]]) => target);
    const flagged = Record.toEntries(invocation.row.nx?.targets ?? {});
    const named = new Set(flagged.filter(([flag]) => invocation.has(flag)).map(([, aggregate]) => aggregate));
    const selectable = new Set(flagged.map(([, aggregate]) => aggregate));
    const targets = targetsOf(node);
    return running.map((target) => ({ node, target, aggregate: aggregateOf(targets, target) })).filter(({ aggregate, target }) => (named.size > 0 ? named.has(aggregate) : !selectable.has(aggregate) && targets[target]?.cache === true));
});

const launcherStart = ({ head, outer }: Invocation): Word => outer[outer.findLastIndex(({ row, path }) => !(row.launcher === true || row.module !== undefined || (row.wraps !== '--' && path.length > 0))) + 1]?.head ?? head;

const nxTarget: Rule = Effect.fnUntraced(function* (invocation, call) {
    const { selects = [], targets = {}, ignores = [] } = invocation.row.nx ?? {};
    const settled = new Set([...selects, ...Object.keys(targets), ...ignores]);
    if (!replaceable(invocation, call) || invocation.args.some(({ names }) => names.length > 0 && !new Set(names).isSubsetOf(settled))) {
        return [];
    }
    const { paths, words } = yield* scopeOf(invocation, call);
    const owners = Option.match(yield* call.host.graph, { onNone: () => [], onSome: (graph) => ownersOf(graph, invocation, paths, call) });
    const runners = yield* Effect.forEach(owners, (node) => runnersOf(node, invocation, words, call));
    const [first, ...others] = runners.flatMap((runner) => (runner.length === 1 ? runner : []));
    const uncached = first !== undefined && targetsOf(first.node)[first.target]?.cache !== true;
    if (first === undefined || others.length + 1 !== owners.length || others.some(({ aggregate }) => aggregate !== first.aggregate) || (uncached && others.length > 0)) {
        return [];
    }
    const one = uncached ? `nx run ${quote([first.node.name, first.target].join(':'))}` : `nx ${first.aggregate} ${quote(first.node.name)}`;
    const named = others.length === 0 ? one : `nx run-many -t ${first.aggregate} -p ${[first, ...others].map(({ node }) => quote(node.name)).join(',')}`;
    return whole(invocation, named, invocation.program, launcherStart(invocation));
});

const hkStep: Rule = Effect.fnUntraced(function* (invocation, call) {
    const { host } = call;
    const { command } = invocation;
    const merged = redirected(invocation, '>&', '2', '1').length + redirected(invocation, '&>').length > 0;
    const heard = merged || !(command.pipes || command.redirects.some(({ op }) => op.startsWith('>')));
    const { paths, files, words } = heard && replaceable(invocation, call) ? yield* scopeOf(invocation, call) : { paths: [], files: [], words: '' };
    const spellings = [...spellingsOf(invocation, call)];
    const ignores = new Set(invocation.row.nx?.ignores);
    const runs = (files.length === 0 ? [] : yield* host.steps).filter(({ text }) => spellings.some((name) => text.includes(name)));
    const bodies = yield* Effect.forEach(runs, ({ text }) => bodyOf(text, call));
    const ran = bodies.map(({ invocations }) => invocations.find((other) => programOf(other, call) === programOf(invocation, call)));
    const flags = ({ args }: Invocation) => keysOf({ args: args.filter(({ names }) => new Set(names).isDisjointFrom(ignores)) });
    const modes = Array.filterMap(Array.zip(runs, ran), ([run, found]) => (found === undefined ? Result.failVoid : Result.succeed({ ...run, keys: flags(found), words: argued(found) })));
    const asked = flags(invocation);
    const common = (held: typeof modes) => held.slice(1).reduce((shared, { keys }) => shared.intersection(keys), held[0]?.keys ?? new Set<string>());
    const policy = ({ step, mode }: (typeof modes)[number]) => {
        const held = modes.filter((other) => other.step === step);
        return mode === 'fix' ? common(held.length > 1 ? held : []) : common(held.filter((other) => other.words === words));
    };
    const [fit, ...more] = modes.filter((mode) => (mode.mode === 'check' || mode.mode === 'fix') && mode.words === words && asked.isSubsetOf(mode.keys) && mode.keys.difference(policy(mode)).isSubsetOf(asked));
    if (fit === undefined || more.length > 0) {
        return [];
    }
    const plans = yield* Effect.forEach(paths, (file) => host.output(['hk', fit.mode, '--step', fit.step, '--plan', '--json', file]), { concurrency: 'unbounded' });
    const included = Array.getSomes(plans.map((plan) => Schema.decodeOption(Plan)(plan))).filter(({ steps }) => steps.some(({ status }) => status === 'included'));
    return included.length === paths.length ? whole(invocation, ['hk', fit.mode, '--step', quote(fit.step), spelled(files)].join(' '), invocation.program, launcherStart(invocation)) : [];
});

const customLanguages: Rule = (invocation) => {
    const [, subcommand = invocation.head] = invocation.words;
    return invocation.has('--custom-languages') ? [] : inserted(invocation, (subcommand.text.startsWith('-') ? invocation.head : subcommand).end, ' --custom-languages allow');
};

const hiddenErrors: Rule = (invocation) => {
    const nulled = redirected(invocation, '>', undefined, '/dev/null');
    return [...nulled, ...redirected(invocation, '&>', undefined, '/dev/null'), ...(nulled.some(({ descriptor }) => Option.isNone(descriptor)) ? redirected(invocation, '>&', '2', '1') : [])].flatMap((redirect) => cut(invocation, [redirect]));
};

const loopRefusal =
    (what: string, action: string, ...reads: readonly string[]): Rule =>
    ({ repeated, operands }) =>
        repeated && !Option.exists(Array.head(operands), ({ text }) => reads.includes(text)) ? refusal(`Loop reruns ${what}, ${action}`) : [];

// --- [NX_ARGUMENTS]

const addressOf = ({ operands }: Invocation, { nodes }: Graph): Option.Option<readonly [string, string]> => {
    const [first, second] = operands.map(({ text }) => text);
    const [project, target] = second?.split(':') ?? [];
    if (first === 'run') {
        return Option.filter(Option.all([Option.fromUndefinedOr(project), Option.fromUndefinedOr(target)] as const), Array.every(String.isNonEmpty));
    }
    return first !== undefined && second !== undefined && nodes[second]?.data.targets?.[first] !== undefined ? Option.some([second, first] as const) : Option.none();
};

const askedTargets = (invocation: Invocation, graph: Graph) => {
    const listed = (...names: readonly string[]) => invocation.values(...names).flatMap((value) => value.split(','));
    const { projects, targets } = Option.match(addressOf(invocation, graph), {
        onNone: () => ({ projects: listed('--projects'), targets: listed('--targets', '--target') }),
        onSome: ([project, target]) => ({ projects: [project], targets: [target] }),
    });
    const nodes = projects.length === 0 ? Object.values(graph.nodes) : Array.getSomes(projects.map((project) => Option.fromNullishOr(graph.nodes[project])));
    return Array.cartesian(nodes, targets).filter(([node, target]) => targetsOf(node)[target] !== undefined);
};

const aggregateArgs: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const asked = Option.match(yield* host.graph, { onNone: () => [], onSome: (graph) => askedTargets(invocation, graph) });
    const [aggregated] = asked.filter(([asking, target]) => patternsOf(targetsOf(asking)[target] ?? {}).length > 0);
    if (aggregated === undefined) {
        return [];
    }
    const [node, aggregate] = aggregated;
    const tools = members(targetsOf(node), aggregate);
    const [tool] = tools;
    const kept = invocation.args.filter(({ names }) => names.length > 0 && !names.some((name) => ['--projects', '--targets', '--target'].includes(name))).map(({ words }) => spelled(words));
    return asked.length === 1 && tool !== undefined && tools.length === 1 ? whole(invocation, ['nx run', quote([node.name, tool].join(':')), ...kept, '--', spelled(invocation.tail)].join(' '), `nx ${aggregate}`) : refusal(`pass one-run arguments to one of: ${tools.join(',')}`);
});

const forwardedOptions: Rule = Effect.fnUntraced(function* (invocation, call) {
    const configured = Option.flatMap(yield* call.host.graph, (graph) =>
        Option.flatMap(addressOf(invocation, graph), ([project, target]) => {
            const entry = graph.nodes[project]?.data.targets?.[target];
            const configuration = invocation.values('--configuration').at(-1) ?? entry?.defaultConfiguration;
            return Option.fromNullishOr(entry === undefined ? undefined : { ...entry.options, ...(configuration === undefined ? {} : entry.configurations?.[configuration]) }.command);
        }),
    );
    const ran = yield* Option.match(configured, {
        onNone: () => Effect.undefined,
        onSome: (command) => Effect.map(bodyOf(command, call), ({ invocations }) => invocations.at(-1)),
    });
    if (ran === undefined) {
        return [];
    }
    const tail = spelled(invocation.tail);
    const [forwarded] = (yield* bodyOf([ran.program, ...invocation.values('--args'), tail].join(' '), call)).invocations;
    if (forwarded === undefined) {
        return [];
    }
    const guards = ['mypy', 'pytest'].includes(ran.program) ? yield* all(...(call.programs.get(forwarded.program)?.rules ?? []))(forwarded, call) : [];
    return [
        ...guards,
        ...(Option.isSome(invocation.separator) && !invocation.has('--args') && forwarded.args.length > 0 && forwarded.args.every(({ names }) => names.length > 0) && keysOf(forwarded).isSubsetOf(keysOf(ran)) ? silent(cut(invocation, [invocation.separator.value, ...invocation.tail], `nx -- ${tail}`)) : []),
    ];
});

// --- [PYTHON]

const projectOptions =
    (...rules: readonly Rule[]): Rule =>
    (invocation, call) =>
        Option.exists(invocation.command.cwd, (folder) => call.host.inside(folder, call.host.root)) ? all(...rules)(invocation, call) : [];

const strayCache: Rule = (invocation, { host }) => Effect.map(host.ignored(Array.getSomes(invocation.values('--cache-dir').map((value) => host.resolve(value, invocation.command.cwd)))), (kept) => (kept ? [] : respell({ drops: ['--cache-dir'], notify: true })(invocation)));

const stubCache: Rule = (invocation) => {
    const set = [...invocation.outer, invocation].some(({ assigned }) => assigned.some(({ text }) => text.startsWith(`${STUB_CACHE}=`))) || invocation.command.variables.has(STUB_CACHE);
    return set ? [] : inserted(invocation, launcherStart(invocation).start, `${STUB_CACHE}=${devNull} `);
};

const absolute = Effect.fnUntraced(function* (word: Word, cwd: Option.Option<string>, host: Call['host']) {
    const file = Option.filter(host.resolve(word.text, cwd), () => !host.path.isAbsolute(word.text));
    return Option.isSome(file) && (yield* host.exists(file.value)) ? Option.some(quote(file.value)) : Option.none();
});

const outsideScript: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const searchPath = 'PYTHONPATH';
    const { head, rest, row, assigned, command } = invocation;
    const [script] = invocation.operands;
    const cwd = invocation.outer
        .filter(({ program }) => program === 'env')
        .flatMap(({ values }) => values('-C'))
        .reduce((folder, value) => host.resolve(value, folder), command.cwd);
    const file = script?.known === true && !invocation.has(...(row.code ?? [])) ? Option.filter(host.resolve(script.text, cwd), (outside) => !host.inside(outside, host.root)) : Option.none();
    if (script === undefined || Option.isNone(file) || !Option.exists(cwd, (folder) => host.inside(folder, host.root))) {
        return [];
    }
    const entries = Option.map(
        Option.match(
            Array.findLast(assigned, ({ text }) => text.startsWith(`${searchPath}=`)),
            {
                onNone: () => command.variables.get(searchPath) ?? Option.some(''),
                onSome: ({ known, text }) => Option.liftPredicate(text.slice(searchPath.length + 1), () => known),
            },
        ),
        (value) => (value === '' ? [] : value.split(':')),
    );
    if (Option.isNone(entries) || rest.some(({ known }) => !known)) {
        return refuse('arguments unresolved', `pass literal words and ${searchPath}`)(invocation);
    }
    const launched = head.text.includes('/') ? Option.getOrElse(yield* absolute(head, cwd, host), () => head.raw) : head.raw;
    const search = entries.value.some((entry) => !host.path.isAbsolute(entry)) ? [`${searchPath}=${quote(Array.getSomes(entries.value.map((entry) => host.resolve(entry, cwd))).join(':'))}`] : [];
    const moved = yield* Effect.forEach([script, ...rest], (word) =>
        Effect.map(
            absolute(word, cwd, host),
            Option.map((text) => edited(invocation, word, text)),
        ),
    );
    return [...edited(invocation, head, ['env -C', quote(host.path.dirname(file.value)), ...search, launched].join(' '), head.raw), ...Array.getSomes(moved).flat()];
});

const configFile = when(holds(/^[^=]+$/u, '--config'), (invocation) => (invocation.has('--no-cache') ? [] : inserted(invocation, (invocation.operands.at(0) ?? invocation.head).end, ' --no-cache')));

// --- [COMPOSITION] ---------------------------------------------------------------------

const TASKS: Ruleset = {
    programs: {
        nx: {
            camelCase: true,
            pipefail: true,
            flags: ['-c, --configuration=', '-t, --targets=', '--target=', '-p, --projects=', '--args='],
            rules: [
                when(Predicate.or(holds(PUBLISH), holds(PUBLISH, '--targets', '--target')), refuse('publishes dependencies for real', 'run nx release')),
                unassign(/^NX_DAEMON=false$/u),
                when((invocation) => invocation.tail.length > 0, aggregateArgs),
                when((invocation) => invocation.tail.length > 0 || invocation.has('--args'), forwardedOptions),
                (invocation) => {
                    const { start } = launcherStart(invocation);
                    return start === invocation.head.start ? [] : edited(invocation, { start, end: invocation.head.start }, '');
                },
                perSubcommand({
                    show: [hiddenErrors],
                    graph: [hiddenErrors],
                    reset: [(invocation, { host }) => (!invocation.has('--only-cache', '--only-daemon', '--only-cloud') && Option.exists(invocation.command.cwd, (folder) => host.inside(folder, host.root)) ? refuse('drops the graph', 'fix the inputs or pass --only-cache')(invocation) : [])],
                }),
                when((invocation) => !invocation.has('--graph'), loopRefusal('nx tasks', 'use one nx run-many', 'show', 'graph')),
            ],
        },
        tsc: {
            aliases: ['tsgo'],
            flags: ['--pretty=', '--incremental'],
            informational: ['-v', '-b', '--build', '-w', '--watch', '--init', '--showConfig'],
            nx: { selects: ['--project'], ignores: ['--noEmit', '--pretty', '--incremental'] },
        },
        cargo: { flags: ['-p, --package=', '--profile='], nx: { selects: ['--package'], targets: { '--fix': 'fix' } } },
        cmake: { flags: ['--build=', '--install=', '--preset=', '-S=', '-B=', '--target='], informational: ['--version', '--help', '--list-presets', '-E', '-P'], nx: { targets: { '--build': 'build', '--install': 'install' }, ignores: ['--preset'] } },
        ctest: { flags: ['--preset=', '--test-dir=', '--output-junit='], informational: ['--version', '--help', '--list-presets', '--show-only'], nx: { ignores: ['--preset', '--test-dir', '--output-junit'] } },
        cpack: { flags: ['--preset=', '--config=', '-G=', '-B='], informational: ['--version', '--help', '--list-presets'], nx: { ignores: ['--preset', '--config'] } },
        'clang-tidy': { flags: ['-p=', '--config-file=', '--checks=', '--fix'], informational: ['--version', '--list-checks', '--dump-config', '--verify-config'], nx: { targets: { '--fix': 'fix' }, ignores: ['-p'] } },
        'clang-format': { flags: ['-i', '--dry-run', '-Werror', '--style='], informational: ['--version', '--dump-config'] },
        gersemi: { flags: ['-c, --check', '-i, --in-place', '--warnings-as-errors', '--cache-dir=', '--config='], informational: ['--version', '--print-config'] },
        uv: {
            pipefail: true,
            flags: ['--package=', '--script=', '--all-packages', '-U, --upgrade', '--check', '--locked'],
            nx: { selects: ['--package'], targets: { '--check': 'check', '--upgrade': 'upgrade' }, ignores: ['--locked'] },
            rules: [perSubcommand({ sync: [ensure(['--all-packages'], '--package', '--script')] })],
        },
        dotnet: { aliases: ['xcodebuild'], pipefail: true, flags: ['-warnaserror', '--verify-no-changes'], nx: { targets: { '--verify-no-changes': 'check', '-warnaserror': 'check' } } },
        rustfmt: { flags: ['--config-path=', '--config=', '--edition=', '--emit=', '--print-config='] },
        swiftlint: { dumpsHelp: true },
        swift: { pipefail: true },
        npm: { pipefail: true },
        npx: { launcher: true },
        'effect-tsgo': { launcher: true },
        mise: { launcher: true },
        ruff: {
            flags: ['--isolated', '--cache-dir=', '--config=', '-n, --no-cache'],
            nx: { ignores: ['--quiet'] },
            rules: [projectOptions(respell({ drops: ['--isolated'], notify: true }), strayCache, unassign(/^RUFF_CACHE_DIR=/u), perSubcommand({ check: [configFile], format: [configFile] }))],
        },
        yamllint: { flags: ['-c, --config-file=', '-f, --format='] },
        squawk: { flags: ['-c, --config=', '--reporter='] },
        'ast-grep': {
            aliases: ['sg'],
            flags: ['-U, --update-all', '-i, --interactive', '--custom-languages=', '--filter=', '--globs=', '--color=', '--report-style='],
            nx: { ignores: ['--report-style'] },
            rules: [customLanguages],
        },
        stubgen: { rules: [projectOptions(stubCache)] },
        mypy: {
            flags: ['--cache-dir=', '--config-file=', '--python-version='],
            nx: { ignores: ['--no-pretty'] },
            rules: [projectOptions(strayCache, unassign(/^MYPY_CACHE_DIR=/u))],
        },
        refurb: { flags: ['--config-file=', '--python-version='], informational: ['--explain'], nx: {} },
        pytest: {
            flags: ['-o, --override-ini=', '-c, --config-file=', '--benchmark-storage='],
            nx: {},
            rules: [
                projectOptions(
                    respell({ drops: ['--override-ini'], only: /^addopts=$/u, notify: true }),
                    when(holds(/^(?:addopts=.+|cache_dir=)/u, '--override-ini'), () => refusal('Pytest overrides project options, pass test flags directly')),
                    ({ has }) => (has('--config-file', '--benchmark-storage') ? refusal('Pytest must use project configuration and benchmark storage') : []),
                    ({ assigned }) => (assigned.some(({ text }) => text.startsWith('PYTEST_ADDOPTS=')) ? refusal('Pass pytest options directly to preserve project defaults') : []),
                ),
            ],
        },
        ty: { flags: ['--python-version=', '--exclude='], nx: {} },
        python: { rules: [outsideScript] },
        pnpm: { pipefail: true, launcher: true, rules: [perSubcommand(Record.fromIterableWith(['patch', 'patch-commit'], (subcommand) => [subcommand, [refuse('copies a package into the tree', 'set the package option, a newer release, or the consumer tsconfig')]]))] },
        pulumi: {
            rules: [
                perSubcommand({
                    up: [refuse('skips stack programs', 'run nx deploy <project>')],
                    refresh: [refuse('skips stack programs', 'run nx deploy <project>')],
                    destroy: [refuse('skips stack programs', 'run nx run <project>:deploy--pulumi --destroy')],
                    preview: [refuse('skips stack programs', 'run nx release --dry-run')],
                }),
            ],
        },
        vitest: {
            flags: ['--exclude=', '--mode='],
            nx: { ignores: ['--run'] },
            rules: [loopRefusal('vitest', 'use --repeats')],
        },
        pitchfork: {
            flags: ['-f, --force'],
            rules: [perSubcommand({ supervisor: [({ operands, has }) => (process.platform === 'darwin' && has('--force') && Option.exists(Array.get(operands, 1), ({ text }) => ['start', 'run'].includes(text)) ? refusal('Forced supervisor start replaces the launchd-owned supervisor, use launchd') : [])] })],
        },
    },
    every: [nxTarget, hkStep],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { TASKS };
