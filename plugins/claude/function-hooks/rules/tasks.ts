import { availableParallelism, devNull } from 'node:os';
import process from 'node:process';
import type { ProjectGraphProjectNode, TargetConfiguration } from '@nx/devkit';
import { Array, Effect, Function, Glob, Match, Option, Predicate, Record, Result, Schema, String, Struct } from 'effect';
import { RedirOperator } from 'sh-syntax/sync';
import type { Host, HostError } from '#/host';
import { Verdict } from '#/rules/decision';
import { all, type Call, commandOf, cut, edited, ensure, erased, feedsJq, holds, inserted, listed, perSubcommand, quote, type Rule, type Ruleset, redirected, refusal, refuse, respell, silent, spelled, unassign, when, whole } from '#/rules/rule';
import { analyzed, type Invocation, type Parser, WRITES } from '#/shell/invocation';
import { parsed, type Word } from '#/shell/script';

// --- [TYPES] ---------------------------------------------------------------------------

type Targets = Readonly<Record<string, TargetConfiguration<Readonly<Record<string, unknown>>>>>;
type Graph = Option.Option.Value<Effect.Success<Call['host']['graph']>>;
interface Helping {
    readonly node: ProjectGraphProjectNode;
    readonly target: string;
    readonly command: string;
}
type Loop = Option.Option.Value<Effect.Success<ReturnType<typeof forLoop>>>;
type Runner = Effect.Success<ReturnType<typeof runnersOf>>['chosen'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const LONG = /^--/u;
const PUBLISH = /(?:^|[:,])nx-release-publish(?:,|$)/u;
const STUB_CACHE = 'MYPY_CACHE_DIR';
const FORMAT_FLAGS = ['-p, --package=', '--manifest-path=', '--message-format='] as const;
const LOOP_READS: ReadonlySet<string> = new Set(['show', 'graph']);
const ADDOPTS = /^addopts=/u;
const ADDOPTS_VARIABLE = 'PYTEST_ADDOPTS=';
const PREVIEW = { aggregate: 'dry-run', args: [], applies: false } as const;
const PULUMI_OPERATIONS: Record.ReadonlyRecord<string, { readonly aggregate: string; readonly args: readonly string[]; readonly applies: boolean }> = {
    up: { aggregate: 'deploy', args: [], applies: true },
    preview: PREVIEW,
    refresh: PREVIEW,
    destroy: { aggregate: 'deploy', args: ['--operation=destroy'], applies: true },
};
const PULUMI_FLAGS: ReadonlySet<string> = new Set(['--stack', '--yes', '--skip-preview', '--non-interactive', '--cwd']);
const CORES = `${availableParallelism()}`;

// --- [MODELS] --------------------------------------------------------------------------

const Plan = Schema.fromJsonString(Schema.Struct({ steps: Schema.Array(Schema.Struct({ status: Schema.String })) }));
const Options = Schema.Struct({ command: Schema.optionalKey(Schema.String), commands: Schema.optionalKey(Schema.Array(Schema.String)), cwd: Schema.optionalKey(Schema.String) });
const Metadata = Schema.fromJsonString(Schema.Struct({ workspace_root: Schema.String, packages: Schema.Array(Schema.Struct({ name: Schema.String, manifest_path: Schema.String })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [GRAPH]

const targetsOf = ({ data }: ProjectGraphProjectNode): Targets => data.targets ?? {};

const helpingBy = Function.memoize((nodes: Graph['nodes']) => {
    const targets = Object.values(nodes).flatMap((node) => Record.collect(targetsOf(node), (target, { metadata }): Helping => ({ node, target, command: metadata?.help?.command ?? '' })));
    return Map.groupBy(
        targets.flatMap((helping) => helping.command.split(' ').map((word) => ({ word, helping }))),
        ({ word }) => word,
    );
});

const bodyOf = (text: string, { host }: Call) => analyzed(text, { cwd: Option.some(host.root), origin: 'body' });

const patternsOf = ({ executor, dependsOn = [] }: Targets[string]): readonly string[] =>
    (executor === 'nx:noop' ? dependsOn : []).flatMap((entry) => (typeof entry === 'string' ? [entry] : Option.toArray(Option.liftPredicate(entry.target, () => entry.projects === undefined && entry.dependencies !== true)))).filter((pattern) => !pattern.startsWith('^'));

const members = (targets: Targets, aggregate: string): readonly string[] => {
    const globs = Option.match(Record.get(targets, aggregate), { onNone: () => [], onSome: patternsOf }).map((pattern) => Glob.make(pattern));
    return Object.keys(targets).filter((name) => name !== aggregate && globs.some((glob) => glob.pattern !== name && Glob.match(glob, name)));
};

const aggregateOf = (targets: Targets, target: string): string =>
    Option.getOrElse(
        Array.findFirst(Object.keys(targets), (aggregate) => members(targets, aggregate).includes(target)),
        () => target,
    );

// --- [NX_TARGETS]

const keysOf = ({ args }: Pick<Invocation, 'args'>): ReadonlySet<string> => new Set(args.flatMap(({ names, values }) => [...names, ...Array.cartesianWith(names.slice(-1), values, (name, value) => `${name}=${value.text}`)]));

const flagsOf = ({ args, row }: Invocation): ReadonlySet<string> => keysOf({ args: args.filter(({ names }) => new Set(names).isDisjointFrom(new Set(row.nx?.ignores))) });

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

const ownersOf = ({ nodes, owner }: Graph, invocation: Invocation, paths: readonly string[], helping: readonly Helping[], call: Call) => {
    const { host } = call;
    const { cwd } = invocation.command;
    const located = (value: string) =>
        Option.flatMap(
            Option.filter(host.resolve(value, cwd), (file) => host.inside(file, host.root)),
            owner,
        );
    const { selects = [] } = invocation.row.nx ?? {};
    const stated = new Map(Array.cartesianWith(helping, selects, ({ node, target }, flag) => [targetsOf(node)[target]?.options?.[flag.replace(LONG, '')], node] as const));
    const selected = invocation.values(...selects).map((value) => Option.firstSomeOf([Option.fromUndefinedOr(stated.get(value)), Record.get(nodes, value), located(value)]));
    const named = [...selected, ...paths.map(owner)];
    const found = named.length === 0 ? Option.toArray(Option.flatMap(cwd, owner)) : Option.getOrElse(Option.all(named), () => []);
    return [...new Map(found.map((node) => [node.name, node])).values()];
};

const executedFiles = Effect.fnUntraced(function* (options: Targets[string]['options'], reads: string | undefined, call: Call) {
    const decoded: typeof Options.Type = Option.getOrElse(Schema.decodeUnknownOption(Options)(options), () => ({}));
    const { command, commands = [], cwd = '' } = decoded;
    const folder = Option.some(call.host.path.join(call.host.root, cwd));
    const bodies = yield* Effect.forEach([...Option.toArray(Option.fromUndefinedOr(command)), ...commands], (text) => bodyOf(text, call));
    const words = bodies.flatMap(({ invocations }) => invocations.flatMap((invocation) => invocation.words));
    return new Set([...Array.getSomes(words.map(({ text }) => call.host.resolve(text, folder))), ...(reads === undefined ? [] : [call.host.path.join(call.host.root, cwd, reads)])]);
});

const runnersOf = Effect.fnUntraced(function* (node: ProjectGraphProjectNode, invocation: Invocation, { words, paths, selected, helping }: { readonly words: readonly string[]; readonly paths: readonly string[]; readonly selected: readonly string[]; readonly helping: readonly Helping[] }, call: Call) {
    const root = call.host.path.join(call.host.root, node.data.root);
    const narrowed = invocation.row.nx?.narrows === true && paths.some((file) => file !== root && call.host.inside(file, root));
    const targets = targetsOf(node);
    const helps = helping.filter((held) => held.node === node).map(({ target, command }) => [target, command] as const);
    const ran = yield* Effect.forEach(helps, ([, command]) => Effect.map(bodyOf(command, call), ({ invocations }) => invocations.at(-1)));
    const executed = selected.length === 0 ? [] : yield* Effect.forEach(helps, ([target]) => executedFiles(targets[target]?.options, invocation.row.nx?.reads, call));
    const eligible = Array.zip(helps, ran).flatMap(([[target], helped], slot) => {
        const stated = helped === undefined ? [] : argued(helped).split(' ').filter(String.isNonEmpty);
        const aggregate = aggregateOf(targets, target);
        const covered = selected.every((file) => executed[slot]?.has(file) === true);
        const fits = !narrowed && covered && helped !== undefined && programOf(helped, call) === programOf(invocation, call) && stated.every((word, at) => words[at] === word) && words.slice(stated.length).every((word) => word === aggregate);
        return fits ? [{ node, target, aggregate, cached: targets[target]?.cache === true, noop: targets[target]?.executor === 'nx:noop' }] : [];
    });
    const flagged = Record.toEntries(invocation.row.nx?.targets ?? {});
    const asked = flagged.filter(([flag]) => invocation.has(flag)).map(([, aggregate]) => aggregate);
    const named = new Set([...asked, ...words.filter((word) => Record.has(targets, word))]);
    const selectable = new Set(flagged.map(([, aggregate]) => aggregate));
    const picked = eligible.filter(({ aggregate }) => named.has(aggregate));
    const pool = picked.length > 0 || asked.length > 0 ? picked : eligible.filter(({ aggregate, cached }) => cached && !selectable.has(aggregate));
    const cached = pool.filter((runner) => runner.cached);
    return { eligible, chosen: cached.length > 0 ? cached : pool.filter(({ noop }) => !noop) };
});

const launcherStart = ({ head, outer }: Invocation): Word => outer[outer.findLastIndex(({ row, path, has }) => has('-C', '--cd') || !(row.launcher === true || row.module !== undefined || (row.wraps !== '--' && path.length > 0))) + 1]?.head ?? head;

const routed = (invocation: Invocation, [first, ...others]: Array.NonEmptyReadonlyArray<Runner>, foreign: { readonly words: readonly string[]; readonly located: boolean }, widened: boolean, everywhere: boolean): readonly Verdict[] => {
    const projects = everywhere && widened ? '' : ` -p ${[first, ...others].map(({ node }) => quote(node.name)).join(',')}`;
    const tool = others.length === 0 ? `nx run ${quote(`${first.node.name}:${first.target}`)}` : `nx run-many -t ${first.target}${projects}`;
    const forwarded = [tool, '--', ...foreign.words].join(' ');
    if (!others.every(({ target, aggregate }) => target === first.target && aggregate === first.aggregate) || (widened && foreign.words.length > 0)) {
        return [];
    }
    if (foreign.words.length > 0) {
        return foreign.located ? refusal(`${commandOf(invocation)} ${foreign.words.join(' ')} names a path the target folder reads elsewhere, run ${forwarded} with paths from that folder`) : whole(invocation, forwarded, invocation.program, launcherStart(invocation));
    }
    const named = Match.value({ widened, many: others.length > 0, cached: first.cached }).pipe(
        Match.when({ widened: true }, () => tool),
        Match.when({ cached: false }, () => tool),
        Match.when({ many: true }, () => `nx run-many -t ${first.aggregate}${projects}`),
        Match.orElse(() => `nx ${first.aggregate} ${quote(first.node.name)}`),
    );
    return whole(invocation, named, invocation.program, launcherStart(invocation));
};

const nxTarget: Rule = Effect.fnUntraced(function* (invocation, call) {
    const { host } = call;
    const { selects = [], targets = {}, ignores = [], reads } = invocation.row.nx ?? {};
    const settled = new Set([...selects, ...Object.keys(targets), ...ignores]);
    const graph = yield* host.graph;
    const helping = Option.match(graph, { onNone: () => [], onSome: ({ nodes }) => [...new Set([...spellingsOf(invocation, call)].flatMap((word) => (helpingBy(nodes).get(word) ?? []).map((found) => found.helping)))] });
    if (Option.isNone(graph) || helping.length === 0 || invocation.program === 'nx' || !replaceable(invocation, call)) {
        return [];
    }
    const { paths, words } = yield* scopeOf(invocation, call);
    const asked = words.split(' ').filter(String.isNonEmpty);
    const owners = ownersOf(graph.value, invocation, paths, helping, call);
    const given = Array.getSomes(invocation.values(...selects).map((value) => host.resolve(value, invocation.command.cwd)));
    const kinds = yield* Effect.forEach(given, (file) => Effect.option(host.fs.stat(file)));
    const selected = yield* Effect.filter(
        Array.zipWith(given, kinds, (file, info) => (reads !== undefined && Option.exists(info, ({ type }) => type === 'Directory') ? host.path.join(file, reads) : file)),
        (file) => host.exists(file),
    );
    const owned = yield* Effect.forEach(owners, (node) => runnersOf(node, invocation, { words: asked, paths, selected, helping }, call));
    const folders = paths.length > 0 ? paths : Option.toArray(invocation.command.cwd);
    const widened = owners.length <= 1 && invocation.values(...selects).length === 0 && owned.every(({ eligible }) => eligible.length === 0);
    const running = new Set(helping.map(({ node }) => node));
    const nested = widened ? Object.values(graph.value.nodes).filter((node) => running.has(node) && node !== owners[0] && folders.some((folder) => host.inside(host.path.join(host.root, node.data.root), folder))) : [];
    const runners = (widened ? yield* Effect.forEach(nested, (node) => runnersOf(node, invocation, { words: asked, paths, selected, helping }, call)) : owned).flatMap(({ eligible, chosen }) => (widened && eligible.length === 0 ? [] : [chosen]));
    const picks = runners.flatMap((chosen) => (chosen.length === 1 ? chosen : []));
    const several = runners.find((chosen) => chosen.length > 1) ?? [];
    const unsettled = invocation.args.filter(({ names }) => names.length > 0 && !new Set(names).isSubsetOf(settled));
    const located = yield* Effect.findFirst(
        unsettled.flatMap(({ values }) => values),
        ({ text, known }) =>
            Option.match(
                Option.filter(host.resolve(text, invocation.command.cwd), () => known),
                { onNone: () => Effect.succeed(!known), onSome: host.exists },
            ),
    );
    const foreign = { words: unsettled.map(({ words: held }) => spelled(held)), located: Option.isSome(located) };
    if (several.length > 1) {
        return refusal(`${commandOf(invocation)} maps to several targets, run one of: ${several.map(({ node, target }) => `nx run ${quote(`${node.name}:${target}`)}`).join(', ')}`);
    }
    return Array.isReadonlyArrayNonEmpty(picks) && picks.length === runners.length ? routed(invocation, picks, foreign, widened, folders.includes(host.root)) : [];
});

const hkStep: Rule = Effect.fnUntraced(function* (invocation, call) {
    const { host } = call;
    const { command } = invocation;
    const merged = redirected(invocation, RedirOperator.DplOut, '2', '1').length + redirected(invocation, RedirOperator.RdrAll).length > 0;
    const heard = merged || !(command.pipes || command.redirects.some(({ op }) => WRITES.has(op) || op === RedirOperator.DplOut));
    const { paths, files, words } = heard && replaceable(invocation, call) ? yield* scopeOf(invocation, call) : { paths: [], files: [], words: '' };
    const spellings = [...spellingsOf(invocation, call)];
    const runs = (files.length === 0 ? [] : yield* Effect.orElseSucceed(host.steps, () => [])).filter(({ text }) => spellings.some((name) => text.includes(name)));
    const bodies = yield* Effect.forEach(runs, ({ text }) => bodyOf(text, call));
    const ran = bodies.map(({ invocations }) => invocations.find((other) => programOf(other, call) === programOf(invocation, call)));
    const modes = Array.filterMap(Array.zip(runs, ran), ([run, found]) => (found === undefined ? Result.failVoid : Result.succeed({ ...run, keys: flagsOf(found), words: argued(found) })));
    const asked = flagsOf(invocation);
    const common = (held: typeof modes) => held.slice(1).reduce((shared, { keys }) => shared.intersection(keys), held[0]?.keys ?? new Set<string>());
    const policy = ({ step, mode }: (typeof modes)[number]) => {
        const held = modes.filter((other) => other.step === step);
        return mode === 'fix' ? common(held.length > 1 ? held : []) : common(held.filter((other) => other.words === words));
    };
    const [fit, ...more] = modes.filter((mode) => (mode.mode === 'check' || mode.mode === 'fix') && mode.words === words && asked.isSubsetOf(mode.keys) && mode.keys.difference(policy(mode)).isSubsetOf(asked));
    if (fit === undefined || more.length > 0) {
        return [];
    }
    const plans = yield* Effect.forEach(paths, (file) => Effect.orElseSucceed(host.output(['hk', fit.mode, '--step', fit.step, '--plan', '--json', file]), () => ''), { concurrency: 'unbounded' });
    const included = Array.getSomes(plans.map((plan) => Schema.decodeOption(Plan)(plan))).filter(({ steps }) => steps.some(({ status }) => status === 'included'));
    return included.length === paths.length ? whole(invocation, ['hk', fit.mode, '--step', quote(fit.step), spelled(files)].join(' '), invocation.program, launcherStart(invocation)) : [];
});

const customLanguages: Rule = (invocation) => {
    const [, subcommand = invocation.head] = invocation.words;
    return invocation.has('--custom-languages') ? [] : inserted(invocation, (subcommand.text.startsWith('-') ? invocation.head : subcommand).end, ' --custom-languages allow');
};

const hiddenErrors: Rule = (invocation, call) => {
    const nulled = redirected(invocation, RedirOperator.RdrOut, undefined, '/dev/null');
    const merged = nulled.some(({ descriptor }) => Option.isNone(descriptor)) || feedsJq(invocation, call);
    return [...nulled, ...redirected(invocation, RedirOperator.RdrAll, undefined, '/dev/null'), ...(merged ? redirected(invocation, RedirOperator.DplOut, '2', '1') : [])].flatMap((redirect) => cut(invocation, [redirect]));
};

const editedExecutor = Effect.fnUntraced(function* ({ host }: Call, graph: Graph, pairs: readonly (readonly [ProjectGraphProjectNode, string])[]) {
    const executors = pairs.flatMap(([node, target]) => [target, ...members(targetsOf(node), target)].flatMap((name) => Option.toArray(Option.fromNullishOr(targetsOf(node)[name]?.executor))));
    const roots = Array.dedupe(Array.getSomes(executors.map((executor) => Option.map(Record.get(graph.nodes, executor.slice(0, executor.lastIndexOf(':'))), ({ data }) => data.root))));
    const changed = Array.isReadonlyArrayNonEmpty(roots) ? yield* Effect.option(host.output(['git', 'status', '--porcelain', '--', ...roots])) : Option.some('');
    return Option.match(changed, { onNone: () => true, onSome: (changes) => changes.trim() !== '' });
});

const skippedCache: Rule = Effect.fnUntraced(function* (invocation, call) {
    const graph = yield* call.host.graph;
    if (!invocation.has('--skip-nx-cache') || Option.isNone(graph)) {
        return [];
    }
    return (yield* editedExecutor(call, graph.value, askedTargets(invocation, graph.value))) ? [] : respell({ drops: ['--skip-nx-cache'], notify: true })(invocation);
});

const forLoop = Effect.fnUntraced(function* ({ request }: Call) {
    const stmts = yield* Effect.option(parsed(request.command, 'bash'));
    const [stmt, ...more] = Option.getOrElse(stmts, () => []);
    const clause = stmt?.cmd;
    const head = clause?.type === 'ForClause' && clause.do.length === 1 && more.length === 0 ? clause.loop : undefined;
    const iterated = head?.type === 'WordIter' ? head : undefined;
    const items = (iterated?.items ?? []).map(({ parts }) => parts.map((part) => (part.type === 'Lit' || part.type === 'SglQuoted' ? part.value : undefined)));
    const literal = items.flatMap((parts) => (parts.every(Predicate.isString) ? [parts.join('')] : []));
    const variable = iterated?.name?.value;
    return Option.filter(Option.fromNullishOr(variable === undefined ? undefined : { variable, items: literal, reference: new RegExp(`^\\$\\{?${variable}\\}?(?::(?<target>[\\w:-]+))?$`, 'u') }), () => literal.length > 0 && literal.length === items.length);
});

const loopedRun = (refused: string, form: (invocation: Invocation, loop: Loop, call: Call) => Effect.Effect<Option.Option<string>, HostError, Host | Parser>): Rule =>
    Effect.fnUntraced(function* (invocation, call) {
        const { command, operands } = invocation;
        if (!invocation.repeated || Option.isNone(command.cwd) || Option.exists(Array.head(operands), ({ text }) => LOOP_READS.has(text))) {
            return [];
        }
        const loop = Option.filter(yield* forLoop(call), () => command.origin === 'call');
        const text = yield* Option.match(loop, { onNone: () => Effect.succeedNone, onSome: (held) => form(invocation, held, call) });
        return Option.match(Option.all({ text, loop }), {
            onNone: () => refusal(refused),
            onSome: (rewritten) => [Verdict.Edit({ start: 0, end: command.source.length, text: Option.some(rewritten.text), subject: Option.some(`for ${rewritten.loop.variable} loop`) })],
        });
    });

const runMany = Effect.fnUntraced(function* (invocation: Invocation, { items, reference }: Loop, call: Call) {
    const { operands, path } = invocation;
    const [first, second] = operands.map(({ text }) => text);
    const run = path[0] === 'run' || first === 'run';
    const named = operands.slice(first === 'run' ? 1 : 0).map(({ raw }) => reference.exec(raw.replaceAll('"', ''))?.groups);
    const at = named.findIndex(Predicate.isNotUndefined);
    const target = named[at]?.['target'];
    const pair = Match.value({ at, run, target }).pipe(
        Match.when({ at: 0, run: true, target: Predicate.isString }, (held) => Option.some({ targets: [held.target], projects: items })),
        Match.when({ at: 1, run: false, target: undefined }, () => Option.map(Option.fromNullishOr(first), (held) => ({ targets: [held], projects: items }))),
        Match.when({ at: 0, run: false, target: undefined }, () => Option.map(Option.fromNullishOr(second), (held) => ({ targets: items, projects: [held] }))),
        Match.orElse(() => Option.none()),
    );
    const graph = yield* call.host.graph;
    const kept = Option.isSome(pair) && Option.isSome(graph) && invocation.has('--skip-nx-cache') ? yield* editedExecutor(call, graph.value, Array.cartesian(Array.getSomes(pair.value.projects.map((name) => Record.get(graph.value.nodes, name))), pair.value.targets)) : false;
    const flags = invocation.args.filter(({ names }) => names.length > 0 && (kept || !names.includes('--skip-nx-cache'))).map(({ words }) => spelled(words));
    const tail = invocation.tail.length > 0 ? ['--', spelled(invocation.tail)] : [];
    return Option.map(pair, ({ targets, projects }) => ['nx run-many -t', targets.join(','), '-p', projects.map((name) => quote(name)).join(','), ...flags, ...tail].join(' '));
});

const repeated = (invocation: Invocation, { items, reference }: Loop) => Effect.succeed(Option.liftPredicate([spelled(invocation.words), `--repeats ${items.length - 1}`].join(' '), () => !invocation.has('--repeats') && invocation.words.every(({ raw }) => !reference.test(raw.replaceAll('"', '')))));

const optionWords = (text: string) =>
    text
        .split(/\s+/u)
        .filter(String.isNonEmpty)
        .map((word) => quote(word))
        .join(' ');

const addedOptions: Rule = (invocation) => {
    const prefixed = [...invocation.outer, invocation].flatMap(({ assigned, head }) => Array.window([...assigned, head], 2)).filter(([{ text }]) => text.startsWith(ADDOPTS_VARIABLE));
    const overridden = invocation.args.filter(({ names, values }) => names.includes('--override-ini') && values.some(({ text }) => text.startsWith('addopts=')));
    if (prefixed.some(([{ known }]) => !known) || overridden.some(({ values }) => values.some(({ known }) => !known))) {
        return refusal('pytest options expand later, pass test flags directly');
    }
    return [
        ...overridden.flatMap(({ words: held, values }) => edited(invocation, { start: held[0].start, end: Array.lastNonEmpty(held).end }, values.map(({ text }) => optionWords(text.replace(ADDOPTS, ''))).join(' ') || "''", `pytest ${spelled(held)}`)),
        ...prefixed.flatMap(([word, next]) => [...erased(invocation, { start: word.start, end: next.start }, word.raw), ...inserted(invocation, invocation.head.end, ` ${optionWords(word.text.slice(ADDOPTS_VARIABLE.length))}`)]),
    ];
};

const pulumiDeploy: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const [verb = ''] = invocation.path;
    const operation = PULUMI_OPERATIONS[verb];
    if (operation === undefined) {
        return [];
    }
    const target = `${operation.aggregate}--pulumi`;
    const graph = yield* host.graph;
    const deployable = Option.match(graph, { onNone: () => [], onSome: ({ nodes }) => Object.values(nodes).filter((node) => targetsOf(node)[target] !== undefined) });
    const owned = Option.flatMap(graph, ({ owner }) => Option.filter(Option.flatMap(invocation.command.cwd, owner), (node) => targetsOf(node)[target] !== undefined));
    const project = Option.getOrElse(
        Option.map(
            Option.orElse(owned, () => Option.filter(Array.head(deployable), () => deployable.length === 1)),
            ({ name }) => quote(name),
        ),
        () => `<${deployable.map(({ name }) => name).join('|') || 'project'}>`,
    );
    const [stack] = invocation.values('--stack');
    const foreign = invocation.args.filter(({ names }) => names.length > 0 && !names.every((name) => PULUMI_FLAGS.has(name)));
    const confirmed = invocation.has('--yes', '--skip-preview');
    const text = Array.isReadonlyArrayEmpty(operation.args) && stack === undefined ? `nx ${operation.aggregate} ${project}` : [`nx run ${project}:${target}`, ...operation.args, ...(stack === undefined ? [] : [`--stack=${quote(stack)}`])].join(' ');
    const derived = !project.startsWith('<') && foreign.length === 0 && (!operation.applies || (confirmed && stack !== undefined));
    return derived ? whole(invocation, text, `pulumi ${verb}`) : refusal(`pulumi ${verb} skips stack programs, run ${text}`);
});

const cargoFormat: Rule = Effect.fnUntraced(function* (invocation, call) {
    const { host } = call;
    const formatter = 'rustfmt';
    const here = Option.filter(invocation.command.cwd, (folder) => host.inside(folder, host.root));
    const [forwarded] = (yield* bodyOf([formatter, spelled(invocation.tail)].join(' '), call)).invocations;
    if (Option.isNone(here) || forwarded === undefined) {
        return [];
    }
    const mode = invocation.has('--check') || forwarded.has('--check') ? 'check' : 'fix';
    const runs = (yield* Effect.orElseSucceed(host.steps, () => [])).filter((run) => run.mode === mode && run.text.includes(formatter));
    const bodies = yield* Effect.forEach(runs, ({ text }) => bodyOf(text, call));
    const ran = bodies.map(({ invocations }) => invocations.find((other) => programOf(other, call) === programOf(forwarded, call)));
    const [fit, ...more] = Array.filterMap(Array.zip(runs, ran), ([{ step }, found]) => (found === undefined ? Result.failVoid : Result.succeed({ step, keys: flagsOf(found) })));
    if (fit === undefined || more.length > 0) {
        return [];
    }
    const extra = Array.difference(invocation.operands, invocation.tail)
        .filter((word, at) => at > 0 || word.text !== 'fmt')
        .map(({ text }) => text);
    const named = [...invocation.values('--package'), ...(invocation.has('--package') ? extra : [])];
    const manifest = host.resolve(invocation.values('--manifest-path').at(-1) ?? 'Cargo.toml', here);
    const folders = invocation.has('--all')
        ? Option.some([host.root])
        : Option.flatMap(Schema.decodeOption(Metadata)(yield* Effect.orElseSucceed(host.output(['cargo', 'metadata', '--no-deps', '--format-version', '1']), () => '')), ({ workspace_root, packages }) => {
              const byName = new Map(packages.map(({ name, manifest_path }) => [name, host.path.dirname(manifest_path)]));
              const byManifest = new Map([[host.path.join(workspace_root, 'Cargo.toml'), workspace_root], ...packages.map(({ manifest_path }) => [manifest_path, host.path.dirname(manifest_path)] as const)]);
              return Option.all(named.length > 0 ? named.map((name) => Option.fromUndefinedOr(byName.get(name))) : [Option.flatMap(manifest, (file) => Option.fromUndefinedOr(byManifest.get(file)))]);
          });
    const targets = Option.map(
        folders,
        Array.map((folder) => (folder === host.root ? '--all' : quote(host.path.relative(here.value, folder) || '.'))),
    );
    const command = ['hk', mode, '--step', quote(fit.step), ...Array.dedupe(Option.getOrElse(targets, () => ['<paths>'])), ...forwarded.operands.map(({ raw }) => raw)].join(' ');
    if (invocation.has('--message-format', '-C') || (extra.length > 0 && !invocation.has('--package')) || !flagsOf(forwarded).isSubsetOf(fit.keys)) {
        return refusal(`cargo fmt option has no hk form, run ${command}`);
    }
    return Option.isNone(targets) ? refusal('cargo fmt resolves no crate, pass --package <crate> or run it in the crate folder') : whole(invocation, command, 'cargo fmt', launcherStart(invocation));
});

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
    const named = (...names: readonly string[]) => invocation.values(...names).flatMap((value) => value.split(','));
    const { projects, targets } = Option.match(addressOf(invocation, graph), {
        onNone: () => ({ projects: named('--projects'), targets: named('--targets', '--target') }),
        onSome: ([project, target]) => ({ projects: [project], targets: [target] }),
    });
    const nodes = projects.length === 0 ? Object.values(graph.nodes) : Array.getSomes(projects.map((project) => Option.fromNullishOr(graph.nodes[project])));
    return Array.cartesian(nodes, targets).filter(([node, target]) => targetsOf(node)[target] !== undefined);
};

const bounded = (invocation: Invocation): readonly Verdict[] =>
    Array.match(
        invocation.outer.filter(({ program }) => program === 'timeout').flatMap(({ head, rest }) => rest.slice(0, 1).flatMap(({ start }) => erased(invocation, { start: head.start, end: start }))),
        {
            onEmpty: () => [],
            onNonEmpty: (edits) => [...edits, Verdict.Note({ subject: 'timeout nx', text: 'timeout before nx removed, as its kill loses unfinished tasks of a run that ends on its own: a foreground call past its timeout moves to the background and notifies, and TaskStop ends a hung run' })],
        },
    );

const boundedTasks: Rule = Effect.fnUntraced(function* (invocation, call) {
    const asked = Option.match(yield* call.host.graph, { onNone: () => [], onSome: (graph) => askedTargets(invocation, graph) });
    const continuous = asked.flatMap(([node, target]) => [target, ...members(targetsOf(node), target)].map((name) => targetsOf(node)[name]?.continuous === true));
    return continuous.length > 0 && !continuous.includes(true) ? bounded(invocation) : [];
});

const aggregateArgs: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const asked = Option.match(yield* host.graph, { onNone: () => [], onSome: (graph) => askedTargets(invocation, graph) });
    const aggregated = asked.find(([asking, target]) => patternsOf(targetsOf(asking)[target] ?? {}).length > 0);
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
            return Option.flatMapNullishOr(Schema.decodeUnknownOption(Options)(entry === undefined ? undefined : { ...entry.options, ...(configuration === undefined ? {} : entry.configurations?.[configuration]) }), Struct.get('command'));
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

// --- [CONCURRENCY]

const sharedDaemon: Rule = (invocation, call) => {
    const { command, words } = invocation;
    const stopped = spelled(words);
    const next = call.analysis.invocations.find((other) => other.program === 'nx' && other.command.source === command.source && other.command.start >= command.end && !['daemon', 'reset'].includes(other.path[0] ?? ''));
    return Option.match(
        Option.filter(listed(command.source, command), () => next !== undefined),
        {
            onNone: () => refusal(`${stopped} stops the daemon every session shares, leave it running`),
            onSome: (span) => [...silent(erased(invocation, span, stopped)), Verdict.Note({ subject: stopped, text: `${stopped} removed, the daemon is shared by every session` })],
        },
    );
};

const cores =
    (counts: RegExp, ...flags: readonly string[]): Rule =>
    (invocation) =>
        invocation.args
            .filter(({ names, values }) => names.some((name) => flags.includes(name)) && values.every(({ known, text }) => known && (text === '0' || !counts.test(text))))
            .flatMap(({ words, values: [value] }) => {
                const [flag] = words;
                const before = `${invocation.program} ${spelled(words)}`;
                return value?.text === '0' ? edited(invocation, value, CORES, before) : edited(invocation, flag, `${flag.text}${flag.text.startsWith('--') ? '=' : ''}${CORES}`, before);
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
    if (script === undefined || script === head || !invocation.outer.every(({ program }) => program === 'env') || Option.isNone(file) || !Option.exists(cwd, (folder) => host.inside(folder, host.root))) {
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
        return [];
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
                    show: [hiddenErrors, when(({ has }) => !has('--web'), bounded)],
                    graph: [hiddenErrors, when(({ has }) => has('--file', '--print'), bounded)],
                    sync: [bounded],
                    'sync:check': [bounded],
                    'conformance:check': [bounded],
                    daemon: [when(({ has }) => has('--stop'), sharedDaemon)],
                    reset: [
                        when(({ has }) => has('--only-daemon'), sharedDaemon),
                        (invocation, { host }) => (!invocation.has('--only-cache', '--only-daemon', '--only-cloud') && Option.exists(invocation.command.cwd, (folder) => host.inside(folder, host.root)) ? refuse('drops the graph', 'fix the inputs or pass --only-cache')(invocation) : []),
                    ],
                }),
                when((invocation) => !invocation.has('--graph'), loopedRun('Loop reruns nx tasks, use one nx run-many', runMany)),
                skippedCache,
                boundedTasks,
                (invocation) => {
                    const dropped = respell({ drops: ['--skip-sync'] })(invocation);
                    return dropped.length === 0 ? [] : [...dropped, ...edited(invocation, invocation.head, 'nx sync && nx'), Verdict.Note({ subject: 'nx --skip-sync', text: '--skip-sync became nx sync before the command, as a skipped sync leaves tsconfig references stale' })];
                },
            ],
        },
        tsc: {
            aliases: ['tsgo'],
            flags: ['--pretty=', '--incremental'],
            informational: ['-v', '--init', '--showConfig'],
            nx: { selects: ['--project'], reads: 'tsconfig.json', targets: { '--noEmit': 'check' }, ignores: ['--pretty', '--incremental', '--watch', '-w', '--build', '-b'] },
        },
        cargo: { toolchain: true, flags: [...FORMAT_FLAGS, '--profile='], nx: { selects: ['--package'], targets: { '--fix': 'fix' }, ignores: ['--workspace', '--all'] }, rules: [perSubcommand({ fmt: [cargoFormat] })] },
        'cargo-fmt': { toolchain: true, flags: FORMAT_FLAGS, rules: [cargoFormat] },
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
        dotnet: {
            aliases: ['xcodebuild'],
            pipefail: true,
            flags: ['-warnaserror', '--verify-no-changes', '-project=', '-workspace=', '-scheme='],
            informational: ['--info', '--list-sdks', '--list-runtimes', '-list', '-showsdks', '-showBuildSettings', '-showdestinations', '-version'],
            nx: { selects: ['-project', '-workspace', '-scheme'], targets: { '--verify-no-changes': 'check', '-warnaserror': 'check' } },
        },
        rustfmt: { toolchain: true, flags: ['--config-path=', '--config=', '--edition=', '--emit=', '--print-config='], nx: { ignores: ['--edition'] } },
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
        ryl: { flags: ['-c, --config-file=', '-d, --config-data=', '-f, --output-format=', '-o, --output-file='] },
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
            nx: { narrows: true },
            rules: [
                projectOptions(
                    respell({ drops: ['--override-ini'], only: /^(?:addopts=|cache_dir=.*)$/u, notify: true }),
                    when(holds(/^addopts=.+/u, '--override-ini'), addedOptions),
                    when(({ assigned, outer }) => [...outer.flatMap((wrapper) => wrapper.assigned), ...assigned].some(({ text }) => text.startsWith(ADDOPTS_VARIABLE)), addedOptions),
                    ({ has }) => (has('--config-file', '--benchmark-storage') ? refusal('Pytest must use project configuration and benchmark storage') : []),
                ),
            ],
        },
        ty: { flags: ['--python-version=', '--exclude='], nx: {} },
        python: { rules: [outsideScript] },
        Blender: { rules: [when(({ has }) => has('--python', '--python-expr'), ensure(['--python-use-system-env']))] },
        pnpm: { pipefail: true, launcher: true, rules: [perSubcommand(Record.fromIterableWith(['patch', 'patch-commit'], (subcommand) => [subcommand, [refuse('copies a package into the tree', 'set the package option, a newer release, or the consumer tsconfig')]]))] },
        pulumi: { flags: ['-s, --stack=', '-y, --yes', '-C, --cwd='], rules: [pulumiDeploy] },
        vitest: {
            flags: ['--exclude=', '--mode=', '--repeats=', '-r, --root=', '--dir=', '--config='],
            nx: { ignores: ['--run'], narrows: true },
            rules: [loopedRun('Loop reruns vitest, use --repeats', repeated)],
        },
        xargs: { rules: [cores(/./u, '-P', '--max-procs')] },
        parallel: { flags: ['-j, --jobs=', '-P, --max-procs='], rules: [cores(/./u, '--jobs', '--max-procs')] },
        make: { aliases: ['gmake'], rules: [cores(/^\d+$/u, '--jobs')] },
        ninja: { flags: ['-j='], rules: [cores(/./u, '-j')] },
        pitchfork: {
            flags: ['-f, --force'],
            rules: [
                perSubcommand({
                    supervisor: [when(({ path, operands, has }) => process.platform === 'darwin' && has('--force') && [...path, ...operands.map(({ text }) => text)].some((word) => word === 'start' || word === 'run'), respell({ drops: ['--force'], notify: true }))],
                }),
            ],
        },
    },
    every: [nxTarget, hkStep],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { TASKS };
