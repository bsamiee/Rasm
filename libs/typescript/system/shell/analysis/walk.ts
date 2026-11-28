import { Array, type Cause, Data, Effect, Equal, Function, HashMap, HashSet, Match, Option, Record, Ref, Stream } from 'effect';
import { BinCmdOperator, type CallExpr, type Command, type Node, type Stmt, walk as visit, type Word } from 'sh-syntax';
import { bind } from '#/analysis/binding';
import { Invocation, Loop, Place, Program, Redirection, Stdin, Within, Written } from '#/analysis/invocation';
import { Callee, Flow, State } from '#/analysis/state';
import { unquote } from '#/evaluation/expansion';
import { Binding, Expanded, type Expansion, Field, Origin, type Scope, trace } from '#/evaluation/value';
import type { Usage } from '#/programs/usage';
import { posix } from '#/syntax/dialect';
import type { Script, ShellError } from '#/syntax/script';

// --- [TYPES] ---------------------------------------------------------------------------

type Form = (stmt: Stmt, state: State, context: Context) => Effect.Effect<Flow, ShellError>;
type Transfer = (simple: SimpleCommand, invocation: Invocation) => Effect.Effect<Flow, ShellError>;
interface Visit {
    readonly node: Node;
    readonly parent: Node | null;
    readonly prop: string | null;
}
interface Hosts {
    readonly parse: (source: string, dialect: Script['dialect']) => Effect.Effect<Script, ShellError>;
    readonly probe: (shell: string, environment: Readonly<Record<string, string>>) => Effect.Effect<Option.Option<Script['dialect']>>;
    readonly read: (path: string) => Effect.Effect<Option.Option<string>, Cause.NoSuchElementError>;
    readonly directory: (path: string) => Effect.Effect<boolean>;
    readonly programs: (program: string) => Effect.Effect<Option.Option<Usage>, ShellError>;
}
interface Body {
    readonly source: Effect.Effect<Option.Option<string>>;
    readonly shell: boolean;
    readonly zero: Option.Option<Written>;
    readonly operands: readonly Written[];
    readonly frames: readonly Within[];
    readonly stdin: Stdin;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const specials = ['PWD', 'OLDPWD', 'HOME', 'IFS'] as const;
const arithmetic: ReadonlySet<string | undefined> = new Set(['ArithmCmd', 'ArithmExp', 'BinaryArithm', 'CallArithm', 'CStyleLoop', 'FlagsArithm', 'LetClause', 'ParenArithm', 'UnaryArithm'] satisfies readonly Node['type'][]);
const subscripts: ReadonlySet<string | null> = new Set(['index', 'offset', 'length', 'subscripts']);
const declarators: ReadonlySet<string | undefined> = new Set(['local', 'declare', 'typeset']);
const chains: Partial<Record<BinCmdOperator, 'zero' | 'nonzero'>> = { [BinCmdOperator.AndStmt]: 'zero', [BinCmdOperator.OrStmt]: 'nonzero' };
const pipes: ReadonlySet<number> = new Set([BinCmdOperator.Pipe, BinCmdOperator.PipeAll]);
const escapes: Readonly<Record<string, string>> = { '%%': '%', '\\\\': '\\', '\\n': '\n', '\\t': '\t' };
const blank = new Expanded({ fields: [Field.known('')], exact: true, refusals: [] });

// --- [MODELS] --------------------------------------------------------------------------

class Context extends Data.Class<{
    readonly walk: Hosts & { readonly origin: Script; readonly files: Ref.Ref<HashMap.HashMap<string, Option.Option<string>>> };
    readonly script: Script;
    readonly remote: Option.Option<Field>;
    readonly visited: HashSet.HashSet<readonly [Option.Option<Field>, string]>;
    readonly aliased: ReadonlySet<string>;
    readonly calls: ReadonlySet<Stmt>;
    readonly at: Pick<Invocation, 'within' | 'stage' | 'stdin' | 'background' | 'repeated'>;
}> {
    // --- [SITUATION]

    set(at: Partial<Context['at']>): Context {
        return new Context({ ...this, at: { ...this.at, ...at } });
    }

    enter(...frames: readonly Within[]): Context {
        return this.set({ within: [...frames, ...this.at.within] });
    }

    loop(clause: Node, condition: boolean): Context {
        const { source, located } = this.script;
        return this.set({ repeated: Option.some(new Loop({ source, start: located(clause.start), end: located(clause.end), condition })) });
    }

    // --- [SOURCE]

    read(script: Script, body: readonly Stmt[] = [], aliased: readonly string[] = []): Context {
        return new Context({ ...this, script, calls: this.calls.union(new Set(body)), aliased: this.aliased.union(new Set(aliased)) });
    }

    open(script: Script, remote: Context['remote'], aliased = this.aliased): Context {
        return new Context({ ...this, script, remote, aliased, visited: HashSet.add(this.visited, [remote, script.source] as const) });
    }

    saw(remote: Context['remote'], source: string): boolean {
        return HashSet.has(this.visited, [remote, source] as const);
    }

    cwd(scope: Scope): Field {
        const held = scope.get('PWD');
        const local = Option.isNone(this.remote);
        return held._tag === 'Known' && local ? Field.known(held.value) : Field.unknown(Origin.Variable({ name: 'PWD', origins: held._tag === 'Unknown' && local ? held.origins : HashSet.empty() }));
    }

    expand(words: readonly (Word | null)[], scope: Scope): readonly [Scope, readonly Expansion[]] {
        return Array.mapAccum(words, scope, (from, word) => {
            const [expansion, to] = word === null ? [blank, from] : this.script.expand(word, from);
            return [to, expansion];
        });
    }

    // --- [FILES]

    load(file: string, missing: Option.Option<string> = Option.none()): Effect.Effect<Option.Option<string>> {
        const disk = Effect.catchTag(this.walk.read(file), 'NoSuchElementError', () => Effect.succeed(missing));
        return Effect.flatMap(Ref.get(this.walk.files), (files) => Option.match(HashMap.get(files, file), { onSome: Effect.succeed, onNone: () => disk }));
    }

    store(file: string, written: Option.Option<string>, append: boolean): Effect.Effect<void> {
        const prior = append ? this.load(file, Option.some('')) : Effect.succeedSome('');
        return Effect.flatMap(prior, (held) => Ref.update(this.walk.files, HashMap.set(file, Option.map(Option.all([held, written]), Array.join('')))));
    }

    fetch(path: Field, remote: Context['remote']): Effect.Effect<Option.Option<string>> {
        return Option.match(
            Option.filter(path.text, () => Option.isNone(remote)),
            { onNone: () => Effect.succeedNone, onSome: (file) => this.load(file) },
        );
    }
}

class SimpleCommand extends Data.Class<{
    readonly stmt: Stmt;
    readonly words: readonly Written[];
    readonly assigns: Invocation['assigns'];
    readonly redirections: readonly Redirection[];
    readonly scope: Scope;
    readonly state: State;
    readonly context: Context;
}> {
    get operands(): Option.Option<readonly string[]> {
        return Option.all(this.words.slice(1).map(({ text: written }) => written));
    }

    invoke(program: Program, usage: Option.Option<Usage> = Option.none()): Invocation {
        const { context, stmt, words, assigns, redirections } = this;
        const [cwd, { source, located }] = [context.cwd(this.scope), context.script];
        const args = bind(usage, words, cwd);
        const opaque = Program.$match(program, { Function: () => false, Builtin: () => args.opaque && Option.isSome(usage), External: () => args.opaque, Unknown: () => true });
        const place = Option.match(context.remote, { onNone: () => Place.Local({ cwd }), onSome: (host) => Place.Remote({ host }) });
        const node = Option.liftPredicate(stmt, () => context.script === context.walk.origin);
        return new Invocation({ program, source, start: located(stmt.start), end: located(stmt.end), words, node, assigns, arguments: args, redirections, opaque, inner: [], place, ...context.at });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const descend = (nodes: readonly Node[]): readonly Visit[] => {
    const visits: Visit[] = [];
    visit(nodes, (node, parent, prop) => {
        visits.push({ node, parent, prop });
        return node.type !== 'CmdSubst' && node.type !== 'ProcSubst';
    });
    return visits;
};
const lex = ({ node, parent, prop }: Visit): readonly string[] =>
    node.type === 'Word' && (arithmetic.has(parent?.type) || subscripts.has(prop)) ? node.parts.flatMap((part) => (part.type === 'DblQuoted' ? part.parts : [part])).flatMap((part) => (part.type === 'Lit' || part.type === 'SglQuoted' ? (part.value.match(/[A-Za-z_]\w*/gu) ?? []) : [])) : [];
const mention = (nodes: readonly Node[]) => {
    const visits = descend(nodes);
    return {
        names: [...specials, ...visits.flatMap((held) => (held.node.type === 'ParamExp' ? Array.fromNullishOr(held.node.param?.value) : lex(held)))],
        bodies: visits.flatMap(({ node }) => (node.type === 'CmdSubst' || node.type === 'ProcSubst' ? [node] : [])),
    };
};
const forget = (node: Node, state: State): State =>
    descend([node])
        .flatMap(lex)
        .reduce((held, name) => held.bind(name, Binding.Unknown({ origins: HashSet.make(Origin.Arithmetic()) })), state);
const limit = (clause: Node, state: State, context: Context): ReadonlySet<string> => {
    const held = HashMap.toValues(state.variables.listed).flatMap(({ values }) => Array.fromIterable(values).flatMap(Binding.$match({ Known: ({ value }) => [value], Listed: ({ items }) => items, Unset: () => [], Unknown: () => [] })));
    const written = descend([clause]).flatMap(({ node }) => (node.type === 'Word' ? Option.toArray(unquote(node, context.script.dialect)) : []));
    return new Set([...held, ...written]);
};
const pass = (words: readonly Written[]): State['positional'] =>
    Option.map(
        Option.liftPredicate(words, (all) => all.every(({ exact }) => exact)),
        Array.map(({ binding }) => binding),
    );
const redirect = (stmt: Stmt, scope: Scope, context: Context): readonly Redirection[] =>
    stmt.redirs.map(({ op, n, word, hdoc }) => {
        const [, [expansion = blank]] = context.expand(Array.fromNullishOr(hdoc ?? word), scope);
        const target = expansion._tag === 'Expanded' ? Array.head(expansion.fields) : Option.some(Field.unknown(Origin.Unsupported()));
        const body = Option.map(expansion._tag === 'Expanded' ? expansion.text : Option.none(), (written) => (hdoc === null ? `${written}\n` : written));
        return Redirection.of(
            op,
            Option.map(Option.fromNullOr(n), ({ value }) => value),
            target,
            context.cwd(scope),
            body,
        );
    });
const supply = (redirections: readonly Redirection[], inherited: Stdin): Stdin =>
    Option.match(
        Array.findLast(redirections, (redirection) => redirection.opens('0', 'read', 'feed', 'duplicate')),
        { onNone: () => inherited, onSome: ({ stdin }) => stdin },
    );
const echo = (words: readonly string[]): Option.Option<string> => {
    const bare = words[0] === '-n';
    const shown = bare ? words.slice(1) : words;
    return Option.liftPredicate(`${shown.join(' ')}${bare ? '' : '\n'}`, () => !shown.some((word) => word.includes('\\')) && !Option.exists(Array.head(shown), (word) => word.startsWith('-')));
};
const printf = ([format = '', ...args]: readonly string[]): Option.Option<string> => {
    const tokens = format.split(/(?<token>%%|%s|\\[\\nt])/u);
    const slots = tokens.filter((token) => token === '%s').length;
    const rounds = slots === 0 ? 1 : Math.max(1, Math.ceil(args.length / slots));
    const printed = Array.makeBy(rounds, (round) => Array.mapAccum(tokens, round * slots, (at, token) => (token === '%s' ? [at + 1, args[at] ?? ''] : [at, escapes[token] ?? token]))[1].join(''));
    return Option.liftPredicate(printed.join(''), () => !format.startsWith('-') && tokens.every((token, at) => at % 2 === 1 || !/[%\\]/u.test(token)));
};
const record = (invocation: Invocation, context: Context, printed: Option.Option<string>): Effect.Effect<void> => {
    const { arguments: args, redirections, stdin } = invocation;
    const copies = args.flags.length === 0 && Option.exists(args.input, ({ roles }) => roles.includes('copy'));
    const copied = Option.filter(stdin._tag === 'Text' ? stdin.text : Option.none<string>(), () => copies);
    const output = Option.orElse(printed, () => copied);
    const redirected = redirections.flatMap((redirection) => (redirection.modes.includes('write') ? [{ target: redirection.target, written: Option.filter(output, () => redirection.opens('1', 'print')), append: redirection.modes.includes('append') }] : []));
    const targets = [...args.effects.writes.map((field) => ({ target: Option.some(field), written: copied, append: false })), ...redirected];
    const files = targets.flatMap(({ target, written, append }) =>
        Option.toArray(
            Option.map(
                Option.flatMap(target, (field) => field.text),
                (file) => ({ file, written, append }),
            ),
        ),
    );
    return Effect.forEach(Option.isSome(context.remote) ? [] : files, ({ file, written, append }) => context.store(file, written, append), { discard: true });
};
const iterate = (entry: State, thresholds: ReadonlySet<string>, round: (head: State) => Effect.Effect<Flow, ShellError>): Effect.Effect<Flow, ShellError> =>
    Stream.paginate(entry, (head) =>
        Effect.map(round(head), (flow) => {
            const wider = head.widen(Option.match(flow.continues, { onNone: () => head, onSome: (again) => head.join(again) }), thresholds);
            return [[flow], Option.liftPredicate(wider, () => !Equal.equals(wider, head))] as const;
        }),
    ).pipe(
        Stream.runFold(
            () => Flow.none,
            (seen, flow) => flow.prepend(seen.found),
        ),
        Effect.map((flow) => flow.route({ zero: ['zero', 'nonzero', 'breaks'], nonzero: [], breaks: [], continues: [] })),
    );
const cycle = (flow: Flow): Flow => flow.route({ continues: ['continues', 'zero', 'nonzero'], zero: [], nonzero: [] });
const repeat = (clause: Extract<Command, { readonly do: readonly Stmt[] }>, state: State, context: Context, enter: (head: State) => State): Effect.Effect<Flow, ShellError> =>
    iterate(enter(state), limit(clause, state, context), (head) => Effect.map(run(clause.do, enter(head), context.loop(clause, false)), (flow) => Flow.join([Flow.of('zero', head), cycle(flow)])));
const concretize = Effect.fnUntraced(function* (nodes: readonly Node[], state: State, context: Context, each: (scope: Scope) => Effect.Effect<Flow, ShellError>): Effect.fn.Return<Flow, ShellError> {
    const { names, bodies } = mention(nodes);
    const flows = yield* Effect.forEach(bodies, (body) => run(body.stmts, state, context.enter(Within.Substitution()).set({ stage: Option.none() })));
    const printers = flows.map(({ found }) => Array.dedupe(found.flatMap(({ leaves }) => leaves).flatMap((leaf) => (leaf.inner.length === 0 ? [leaf.label] : []))));
    const scopes = state.split(names, HashMap.fromIterable<Node, readonly string[]>(Array.zip(bodies, printers)));
    return Flow.join(yield* Effect.forEach(scopes, each)).prepend(flows.flatMap(({ found }) => found));
});
const run = (stmts: readonly Stmt[], state: State, context: Context): Effect.Effect<Flow, ShellError> =>
    Effect.reduce(
        stmts,
        () => Flow.of('zero', state),
        (flow, stmt) => flow.andThen((next) => perform(stmt, next, context)),
    );
const perform = (stmt: Stmt, state: State, context: Context): Effect.Effect<Flow, ShellError> => {
    const detached = stmt.background ? context.set({ background: true, stdin: Stdin.Closed() }) : context;
    const compound = stmt.cmd?.type !== 'CallExpr' && Array.isArrayNonEmpty(stmt.redirs);
    const fed = compound ? detached.set({ stdin: supply(redirect(stmt, state.project(), detached), detached.at.stdin) }) : detached;
    return Option.match(Option.fromNullOr(stmt.cmd), {
        onNone: () => Effect.succeed(Flow.status(Option.some(state), Option.none())),
        onSome: (command) =>
            Effect.map(dispatch(command)(stmt, state, fed), (flow) => {
                const ran = stmt.background ? Flow.status(Option.some(state), Option.none(), flow.found) : flow;
                return stmt.negated ? ran.route({ zero: ['nonzero'], nonzero: ['zero'] }) : ran;
            }),
    });
};
const pipe = Effect.fnUntraced(function* (binary: Extract<Command, { readonly type: 'BinaryCmd' }>, state: State, context: Context) {
    const stages = (stmt: Stmt | null): readonly Stmt[] => (stmt?.cmd?.type === 'BinaryCmd' && pipes.has(stmt.cmd.op) && !stmt.negated ? [...stages(stmt.cmd.x), ...stages(stmt.cmd.y)] : Array.fromNullishOr(stmt));
    const [all, pipeline] = [[...stages(binary.x), ...stages(binary.y)], context.script.located(binary.start)];
    const flows = yield* Effect.forEach(all, (stage, index) => {
        const stdin = index === 0 ? context.at.stdin : Stdin.Piped({ pipeline, index: index - 1 });
        return perform(stage, state, context.set({ stage: Option.some({ pipeline, index, count: all.length }), stdin }));
    });
    const last = Option.getOrElse(Array.last(flows), () => Flow.none);
    return (context.script.dialect.options.lastpipe ? last : last.contain(state)).prepend(flows.slice(0, -1).flatMap(({ found }) => found));
});
const unalias = (expr: CallExpr, stmt: Stmt, context: Context): Option.Option<{ readonly source: string; readonly names: readonly string[] }> => {
    const { dialect, located, source } = context.script;
    const found = expr.args.flatMap((word, at) => {
        const [part] = word.parts;
        const literal = word.parts.length === 1 && part?.type === 'Lit' && !part.value.includes('\\') ? Option.some(part.value) : Option.none();
        const substitute = Option.flatMap(
            Option.filter(literal, (name) => !context.aliased.has(name)),
            (name) => Option.map(dialect.alias(name, at === 0), (written) => ({ name, written })),
        );
        return Option.toArray(Option.map(substitute, (held) => ({ ...held, start: located(word.start), end: located(word.end) })));
    });
    const [rest, pieces] = Array.mapAccum(found, located(stmt.start), (from, { written, start, end }) => [end, `${source.slice(from, start)}${written}`]);
    return Option.liftPredicate({ source: `${pieces.join('')}${source.slice(rest, located(stmt.end))}`, names: found.map(({ name }) => name) }, () => Array.isReadonlyArrayNonEmpty(found));
};
const embed = (invocation: Invocation, remote: Context['remote'], context: Context): readonly Body[] => {
    const { arguments: args, stdin } = invocation;
    const fed = stdin._tag === 'Text' ? { source: Effect.succeed(stdin.text), stdin: Stdin.Text({ text: Option.none() }) } : { source: Effect.succeedNone, stdin };
    return [
        ...args.scripts.map((script) => ({ ...script, source: Effect.succeed(script.text), frames: [], stdin })),
        ...args.files.flatMap((file) => (file.shell ? [{ ...file, source: context.fetch(file.path, remote), frames: Option.toArray(Option.map(file.path.text, (path) => Within.File({ path }))), stdin }] : [])),
        ...Option.toArray(args.input).flatMap(({ roles }) => (roles.includes('script') || roles.includes('shell') ? [{ ...fed, shell: roles.includes('shell'), zero: Option.none(), operands: args.operands.map(({ word }) => word), frames: [] }] : [])),
    ];
};
const nest = Effect.fnUntraced(function* (simple: SimpleCommand, invocation: Invocation): Effect.fn.Return<Invocation, ShellError> {
    const { arguments: args, stdin } = invocation;
    const { context } = simple;
    const name = Option.getOrElse(invocation.name, () => '');
    const environment = simple.assigns.reduce((held, { name: assigned, value }) => held.assign(assigned, HashSet.make(value.binding), true), simple.state);
    const inside = context.enter(Within.Wrapper({ name })).set({ stage: Option.none(), background: false });
    const consumed = Option.exists(args.input, ({ roles }) => roles.includes('command'));
    const commands = yield* Effect.forEach(args.commands, (words) => {
        const listed = new SimpleCommand({ ...simple, words, assigns: [], state: environment, context: inside.set({ stdin: consumed ? Stdin.Closed() : stdin }) });
        const program = Option.map(
            Option.flatMap(Array.head(words), ({ text: written }) => written),
            (written) => written.slice(written.lastIndexOf('/') + 1),
        );
        return Option.match(program, {
            onNone: () => Effect.succeed(listed.invoke(Program.Unknown({ origin: Origin.Command({ name: '' }) }))),
            onSome: (short) => Effect.flatMap(context.walk.programs(short), (usage) => nest(listed, listed.invoke(Program.External({ name: short }), usage))),
        });
    });
    const remote = Option.orElse(
        Option.map(args.host, ({ field }) => field),
        () => context.remote,
    );
    const shell = Option.getOrElse(
        Option.flatMap(Array.head(invocation.words), ({ text: written }) => written),
        () => name,
    );
    const inner = yield* Effect.forEach(
        embed(invocation, remote, context),
        Effect.fnUntraced(function* (body) {
            const probed = Option.isSome(args.host) ? Option.some(posix('sh', {})) : yield* context.walk.probe(body.shell ? shell : 'sh', context.script.dialect.environment);
            const source = yield* Effect.fromOption(Option.filter(yield* body.source, (written) => !context.saw(remote, written)));
            const script = yield* Effect.fromOption(yield* Effect.option(context.walk.parse(source, yield* Effect.fromOption(probed))));
            const zero = Option.getOrElse(
                Option.flatMap(body.zero, ({ text: written }) => written),
                () => script.dialect.binary,
            );
            const entered = Option.isSome(args.host) ? new State({ ...environment.havoc(), positional: Option.some([]), name: 'sh' }) : environment.enter(script.scope, script.dialect, pass(body.operands), zero);
            return (yield* run(
                script.syntax.stmts,
                entered,
                inside
                    .open(script, remote, new Set())
                    .enter(...body.frames)
                    .set({ stdin: body.stdin }),
            )).found;
        }, Effect.catchNoSuchElement),
    );
    return new Invocation({ ...invocation, inner: [...commands, ...Array.getSomes(inner).flat()], opaque: invocation.opaque || inner.some(Option.isNone) });
});
const spawn =
    (print: (words: readonly string[]) => Option.Option<string>): Transfer =>
    (simple, invocation) =>
        nest(simple, invocation).pipe(
            Effect.tap(() => record(invocation, simple.context, Option.flatMap(simple.operands, print))),
            Effect.map((nested) => Flow.status(Option.some(simple.state), Option.some(simple.state), [nested])),
        );
const end =
    (zero: (simple: SimpleCommand) => Option.Option<State>, nonzero: (simple: SimpleCommand) => Option.Option<State>): Transfer =>
    (simple, invocation) =>
        Effect.succeed(Flow.status(zero(simple), nonzero(simple), [invocation]));
const leave =
    (channel: 'breaks' | 'continues' | 'returns' | 'exits'): Transfer =>
    (simple, invocation) =>
        Effect.succeed(Flow.of(channel, simple.state, [invocation]));
const keep = ({ state }: SimpleCommand) => Option.some(state);
const assign =
    (names: (operands: readonly string[]) => readonly string[], binding: Binding) =>
    ({ state, operands }: SimpleCommand) =>
        Option.some(Option.match(operands, { onNone: () => state.havoc(), onSome: (given) => names(given).reduce((held, name) => held.bind(name, binding), state) }));
const chdir: Transfer = (simple, invocation) => {
    const { state, scope, operands, context } = simple;
    const named = Option.filter(operands, (given) => !given.some((operand) => operand.startsWith('-')));
    const target = Option.flatMap(named, (given) => Option.orElse(Array.head(given), () => scope.text('HOME')));
    const path = Option.flatMap(target, (directory) => (directory.startsWith('/') ? Option.some(directory) : Option.map(scope.text('PWD'), (base) => `${base}/${directory}`)));
    const moved = Option.map(path, (written) => Option.getOrElse(Field.known(written).resolve(Field.known('/')).text, () => written));
    const pwd = Option.match(moved, { onNone: () => Binding.Unknown({ origins: HashSet.empty() }), onSome: (value) => Binding.Known({ value }) });
    const present = Option.match(
        Option.filter(moved, () => Option.isNone(context.remote)),
        { onNone: () => Effect.succeed(false), onSome: context.walk.directory },
    );
    return Effect.map(present, (entered) =>
        Flow.status(
            Option.some(state.bind('OLDPWD', scope.get('PWD')).bind('PWD', pwd)),
            Option.liftPredicate(state, () => !entered),
            [invocation],
        ),
    );
};
const shift = ({ state, operands }: SimpleCommand) => {
    const places = Option.filter(
        Option.map(operands, ([written = '1']) => Number(written)),
        Number.isSafeInteger,
    );
    return Option.some(state.position(Option.flatMap(state.positional, (held) => Option.map(places, (dropped) => held.slice(dropped)))));
};
const include: Transfer = Effect.fnUntraced(
    function* (simple, invocation) {
        const { context, state } = simple;
        const file = yield* Effect.fromOption(Array.head(invocation.arguments.files));
        const path = yield* Effect.fromOption(file.path.text);
        const source = yield* Effect.fromOption(Option.filter(yield* context.fetch(file.path, context.remote), (written) => !context.saw(context.remote, written)));
        const script = yield* Effect.fromOption(yield* Effect.option(context.walk.parse(source, context.script.dialect)));
        const given = Array.isReadonlyArrayNonEmpty(file.operands);
        const flow = yield* run(script.syntax.stmts, given ? state.position(pass(file.operands)) : state, context.open(script, context.remote).enter(Within.File({ path })));
        return flow.conclude((after) => (given ? after.position(state.positional) : after)).prepend([invocation]);
    },
    (effect, simple, invocation) => Effect.catchTag(effect, 'NoSuchElementError', () => Effect.succeed(Flow.havoc(simple.state, [new Invocation({ ...invocation, opaque: true })]))),
);
const evaluate: Transfer = (simple, invocation) => {
    const { context, state } = simple;
    return Option.match(simple.operands, {
        onNone: () => Effect.succeed(Flow.havoc(state, [invocation, new SimpleCommand({ ...simple, words: [], assigns: [] }).invoke(Program.Unknown({ origin: Origin.Script() }))])),
        onSome: (given) => Effect.flatMap(context.walk.parse(given.join(' '), context.script.dialect), (script) => Effect.map(run(script.syntax.stmts, state, context.read(script).enter(Within.Eval())), (flow) => flow.prepend([invocation]))),
    });
};
const transfers: Readonly<Record<string, Transfer>> = {
    ':': end(keep, Option.none),
    true: end(keep, Option.none),
    false: end(Option.none, keep),
    break: leave('breaks'),
    continue: leave('continues'),
    return: leave('returns'),
    exit: leave('exits'),
    cd: chdir,
    unset: end(
        assign((given) => given.filter((operand) => !operand.startsWith('-')), Binding.Unset()),
        keep,
    ),
    read: end(
        assign((given) => [...given, 'REPLY'], Binding.Unknown({ origins: HashSet.make(Origin.Input()) })),
        keep,
    ),
    shift: end(shift, keep),
    source: include,
    '.': include,
    eval: evaluate,
    echo: spawn(echo),
    printf: spawn(printf),
};
const call = (simple: SimpleCommand, name: string, { body, script }: Extract<Callee, { readonly _tag: 'Defined' }>): Effect.Effect<Flow, ShellError> => {
    const { context, state } = simple;
    const locals = descend([body]).flatMap(({ node }) => (node.type === 'DeclClause' && declarators.has(node.variant?.value) ? node.args.flatMap((declared) => Array.fromNullishOr(declared.name?.value)) : []));
    const entered = context.read(script, [body]).enter(Within.Function({ name }));
    return context.calls.has(body)
        ? Effect.succeed(Flow.havoc(state, [simple.invoke(Program.Unknown({ origin: Origin.Recursion() }))]))
        : Effect.map(perform(body, state.position(pass(simple.words.slice(1))), entered), (flow) => flow.conclude((after) => after.restore(state, locals)).prepend([simple.invoke(Program.Function({ name }))]));
};
const resolve = Effect.fnUntraced(function* (simple: SimpleCommand, name: string): Effect.fn.Return<Flow, ShellError> {
    const [own, short] = [simple.context.script.dialect.builtins.has(name), name.slice(name.lastIndexOf('/') + 1)];
    const usage = yield* simple.context.walk.programs(own ? name : short);
    const transfer = Option.getOrElse(
        Option.filter(Record.get(transfers, name), () => own),
        () => spawn(Option.none),
    );
    const program = own ? Program.Builtin({ name }) : Program.External({ name: short });
    const flows = yield* Effect.forEach(simple.state.functions.get(name), (callee) => (callee._tag === 'Defined' ? call(simple, name, callee) : transfer(simple, simple.invoke(program, usage))));
    return Flow.join(flows);
});
const execute = (expr: CallExpr, stmt: Stmt, state: State, scope: Scope, context: Context): Effect.Effect<Flow, ShellError> => {
    const sources = [...expr.args, ...expr.assigns.map(({ value }) => value)];
    const [after, expansions] = context.expand(sources, scope);
    const fields = Array.zipWith(sources, expansions, (word, expansion) => {
        const node = Option.filter(Option.fromNullOr(word), () => expansion._tag === 'Expanded' && expansion.fields.length === 1 && context.script === context.walk.origin);
        return expansion._tag === 'Expanded' ? expansion.fields.map((field) => new Written({ field, exact: expansion.exact, node })) : [];
    });
    const [words, values] = Array.splitAt(fields, expr.args.length);
    const assigns = Array.zipWith(expr.assigns, values, ({ name }, held) => Array.take(held, 1).map((value) => ({ name: name?.value ?? '', value }))).flat();
    const redirections = redirect(stmt, scope, context);
    const simple = new SimpleCommand({ stmt, words: words.flat(), assigns, redirections, scope, state: state.absorb(scope, after), context: context.set({ stdin: supply(redirections, context.at.stdin) }) });
    const assigned = Array.zip(expr.assigns, expansions.slice(expr.args.length)).reduce((held, [assigning, expansion]) => {
        const [name, listed] = [assigning.name?.value ?? '', assigning.array !== null || assigning.index !== null || assigning.append];
        const exported = context.script.dialect.options.allexport || held.variables.get(name).exported;
        return held.assign(name, HashSet.make(listed ? Binding.Unknown({ origins: HashSet.empty() }) : expansion.binding), exported);
    }, simple.state);
    const name = Option.flatMap(Array.head(simple.words), ({ text: written }) => written);
    return Match.value({ refused: expansions.some((expansion) => expansion._tag === 'Refused'), bare: Array.isReadonlyArrayEmpty(simple.words), name: Option.getOrUndefined(name) }).pipe(
        Match.when({ refused: true }, () => Effect.succeed(Flow.of('exits', simple.state))),
        Match.when({ bare: true }, () => Effect.succeed(Flow.status(Option.some(assigned), Option.none()))),
        Match.when({ name: Match.string }, ({ name: program }) => resolve(simple, program)),
        Match.orElse(() => {
            const invocation = simple.invoke(Program.Unknown({ origin: Origin.Command({ name: '' }) }));
            return Effect.as(record(invocation, simple.context, Option.none()), Flow.havoc(simple.state, [invocation]));
        }),
    );
};
const declare = (clause: Extract<Command, { readonly type: 'DeclClause' }>, simple: SimpleCommand): Flow => {
    const exports = clause.variant?.value === 'export';
    const dynamic = clause.args.some(({ name, value }) => name === null && value !== null && !(value.parts[0]?.type === 'Lit' && /^[-+]/u.test(value.parts[0].value)));
    const next = clause.args.reduce(
        (held, declared) =>
            Option.match(Option.fromNullOr(declared.name), {
                onNone: () => held,
                onSome: ({ value: name }) => {
                    const [, [expansion]] = simple.context.expand(declared.naked || declared.array !== null ? [] : Array.fromNullishOr(declared.value), simple.scope);
                    const assigned = Option.match(Option.fromUndefinedOr(expansion), { onNone: () => Binding.Unknown({ origins: HashSet.empty() }), onSome: (written) => written.binding });
                    return held.assign(name, HashSet.make(declared.naked ? held.variables.get(name).binding : assigned), exports || held.variables.get(name).exported);
                },
            }),
        dynamic ? simple.state.havoc() : simple.state,
    );
    return Flow.status(Option.some(next), Option.none(), [simple.invoke(Program.Builtin({ name: clause.variant?.value ?? '' }))]);
};
const enumerate = (clause: Extract<Command, { readonly type: 'ForClause' }>, items: readonly Word[], name: string, state: State, scope: Scope, context: Context): Effect.Effect<Flow, ShellError> => {
    const [, expansions] = context.expand(items, scope);
    const fields = expansions.flatMap((expansion) => (expansion._tag === 'Expanded' && expansion.exact ? expansion.fields.map((field) => field.text) : [Option.none<string>()]));
    const origins = expansions.flatMap((expansion) => [...trace(expansion.binding)]);
    const unroll = (given: readonly string[]) =>
        Effect.reduce(
            given,
            () => Flow.of('zero', state),
            (flow, item) => flow.andThen((next) => Effect.map(run(clause.do, next.bind(name, Binding.Known({ value: item })), context.loop(clause, false)), (ran) => ran.route({ zero: ['zero', 'nonzero', 'continues'], nonzero: [], continues: [] }))),
        );
    return Option.match(
        Option.filter(Option.all(fields), () => Array.isReadonlyArrayNonEmpty(items)),
        {
            onNone: () => repeat(clause, state, context, (head) => head.bind(name, Binding.Unknown({ origins: HashSet.fromIterable(origins) }))),
            onSome: (given) => Effect.map(unroll(given), (flow) => flow.route({ zero: ['zero', 'breaks'], breaks: [] })),
        },
    );
};
const select = (clause: Extract<Command, { readonly type: 'CaseClause' }>, state: State, scope: Scope, context: Context): Effect.Effect<Flow, ShellError> => {
    const [, [subject]] = context.expand(Array.fromNullishOr(clause.word), scope);
    const given = subject?._tag === 'Expanded' ? subject.text : Option.none<string>();
    const arms = clause.items.map((item) => ({ item, verdicts: context.expand(item.patterns, scope)[1].map((pattern) => Option.flatMap(given, (written) => (pattern._tag === 'Expanded' ? pattern.matches(written) : Option.none()))) }));
    return Effect.map(
        Effect.reduce(
            arms,
            () => ({ unmatched: Option.some(state), flows: Array.empty<Flow>() }),
            ({ unmatched, flows }, { item, verdicts }) =>
                Option.match(
                    Option.filter(unmatched, () => !verdicts.every(Option.contains(false))),
                    {
                        onNone: () => Effect.succeed({ unmatched, flows }),
                        onSome: (reaching) => Effect.map(run(item.stmts, reaching, context), (flow) => ({ unmatched: Option.filter(unmatched, () => !verdicts.some(Option.contains(true))), flows: [...flows, flow] })),
                    },
                ),
        ),
        ({ unmatched, flows }) => Flow.join([...flows, Flow.status(unmatched, Option.none())]),
    );
};
const forms: { readonly [T in Command['type']]: (form: Extract<Command, { readonly type: T }>) => Form } = {
    CallExpr: (expr) => (stmt, state, context) =>
        Option.match(unalias(expr, stmt, context), {
            onSome: ({ source, names }) => Effect.flatMap(context.walk.parse(source, context.script.dialect), (script) => run(script.syntax.stmts, state, context.read(script, [], names))),
            onNone: () => concretize([expr, ...stmt.redirs], state, context, (scope) => execute(expr, stmt, state, scope, context)),
        }),
    Block:
        ({ stmts }) =>
        (_, state, context) =>
            run(stmts, state, context),
    Subshell:
        ({ stmts }) =>
        (_, state, context) =>
            Effect.map(run(stmts, state, context.enter(Within.Subshell())), (flow) => flow.contain(state)),
    BinaryCmd: (binary) => (_, state, context) =>
        Option.match(Option.fromUndefinedOr(chains[binary.op]), {
            onNone: () => pipe(binary, state, context),
            onSome: (channel) => Effect.flatMap(run(Array.fromNullishOr(binary.x), state, context), (left) => left.branch(Record.singleton(channel, (met: State) => run(Array.fromNullishOr(binary.y), met, context)))),
        }),
    IfClause: (clause) => (stmt, state, context) =>
        Array.isArrayEmpty(clause.cond)
            ? run(clause.then, state, context)
            : Effect.flatMap(run(clause.cond, state, context), (tested) =>
                  tested.branch({
                      zero: (met) => run(clause.then, met, context),
                      nonzero: (unmet) => Option.match(Option.fromNullOr(clause.else), { onNone: () => Effect.succeed(Flow.status(Option.some(unmet), Option.none())), onSome: (other) => dispatch(other)(stmt, unmet, context) }),
                  }),
              ),
    WhileClause: (clause) => (_, state, context) =>
        iterate(state, limit(clause, state, context), (head) =>
            Effect.flatMap(run(clause.cond, head, context.loop(clause, true)), (tested) => (clause.until ? tested.route({ zero: ['nonzero'], nonzero: ['zero'] }) : tested).branch({ zero: (met) => Effect.map(run(clause.do, met, context.loop(clause, false)), cycle) })),
        ),
    ForClause: (clause) => (_, state, context) => {
        const iteration = clause.loop;
        return iteration?.type === 'WordIter'
            ? concretize(iteration.items, state, context, (scope) => enumerate(clause, iteration.items, iteration.name?.value ?? '', state, scope, context))
            : repeat(clause, state, context, (head) => Option.match(Option.fromNullOr(iteration), { onNone: () => head, onSome: (counted) => forget(counted, head) }));
    },
    RepeatClause: (clause) => (_, state, context) => repeat(clause, state, context, Function.identity),
    CaseClause: (clause) => (_, state, context) => concretize([...Array.fromNullishOr(clause.word), ...clause.items.flatMap(({ patterns }) => patterns)], state, context, (scope) => select(clause, state, scope, context)),
    FuncDecl: (declared) => (stmt, state, context) => {
        const names = [...Array.fromNullishOr(declared.name), ...declared.names].map(({ value }) => value);
        return Effect.succeed(Flow.status(Option.some(state.define(names, Callee.Defined({ body: declared.body ?? stmt, script: context.script }))), Option.none()));
    },
    DeclClause: (clause) => (stmt, state, context) => concretize([clause], state, context, (scope) => Effect.succeed(declare(clause, new SimpleCommand({ stmt, words: [], assigns: [], redirections: redirect(stmt, scope, context), scope, state, context })))),
    ArithmCmd: (clause) => (_, state, context) => concretize([clause], state, context, () => Effect.succeed(Flow.status(Option.some(forget(clause, state)), Option.some(forget(clause, state))))),
    LetClause: (clause) => (_, state, context) => concretize([clause], state, context, () => Effect.succeed(Flow.status(Option.some(forget(clause, state)), Option.some(forget(clause, state))))),
    TestClause: (clause) => (_, state, context) => concretize([clause], state, context, () => Effect.succeed(Flow.status(Option.some(state), Option.some(state)))),
    TimeClause: (clause) => (_, state, context) => run(Array.fromNullishOr(clause.stmt), state, context),
    TestDecl: (clause) => (_, state, context) => run(Array.fromNullishOr(clause.body), state, context),
    CoprocClause: (clause) => (_, state, context) => Effect.map(run(Array.fromNullishOr(clause.stmt), state, context.set({ background: true })), (flow) => Flow.status(Option.some(state), Option.none(), flow.found)),
    TryClause: (clause) => (_, state, context) => run([...(clause.try?.stmts ?? []), ...(clause.always?.stmts ?? [])], state, context),
};
const dispatch = Match.type<Command>().pipe(Match.discriminatorsExhaustive('type')(forms));
const interpret = Effect.fnUntraced(function* (script: Script, hosts: Hosts) {
    const files = yield* Ref.make(HashMap.empty<string, Option.Option<string>>());
    const at = { within: [], stage: Option.none(), stdin: Stdin.Inherited(), background: false, repeated: Option.none() };
    const context = new Context({ walk: { ...hosts, origin: script, files }, script, remote: Option.none(), visited: HashSet.make([Option.none(), script.source] as const), aliased: new Set(), calls: new Set(), at });
    return Array.dedupe((yield* run(script.syntax.stmts, State.start(script.scope, script.dialect), context)).found);
});
const builtins = Record.keys(transfers);

// --- [EXPORTS] -------------------------------------------------------------------------

export { builtins, interpret };
