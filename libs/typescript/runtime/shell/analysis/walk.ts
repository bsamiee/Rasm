import { Array, type Cause, Effect, Equal, HashMap, HashSet, Match, Option, Ref, Struct } from 'effect';
import { BinCmdOperator, type CallExpr, type Command, type Node, RedirOperator, type Stmt, walk as visit, type Word } from 'sh-syntax';
import { bound } from '#/analysis/binding';
import { Invocation, Loop, Place, Program, Stdin, Within, Written } from '#/analysis/invocation';
import { Callee, Flow, State } from '#/analysis/state';
import { text } from '#/evaluation/expansion';
import { Binding, Expanded, Field, Origin, type Scope } from '#/evaluation/value';
import type { Usage } from '#/programs/usage';
import { posix } from '#/syntax/dialect';
import type { Script, ShellError } from '#/syntax/script';

// --- [TYPES] ---------------------------------------------------------------------------

interface Context {
    readonly script: Script;
    readonly parse: (source: string, dialect: Script['dialect']) => Effect.Effect<Script, ShellError>;
    readonly interpreter: (binary: string, environment: Readonly<Record<string, string>>) => Effect.Effect<Option.Option<Script['dialect']>>;
    readonly read: (path: string) => Effect.Effect<Option.Option<string>, Cause.NoSuchElementError>;
    readonly files: Ref.Ref<HashMap.HashMap<string, Option.Option<string>>>;
    readonly stdin: Stdin;
    readonly aliased: ReadonlySet<string>;
    readonly within: readonly Within[];
    readonly loop: Option.Option<Loop>;
    readonly stage: Invocation['stage'];
    readonly background: boolean;
    readonly calls: readonly Stmt[];
    readonly programs: (program: string) => Effect.Effect<Option.Option<Usage>, ShellError>;
    readonly origin: Script;
    readonly remote: Option.Option<Field>;
    readonly visited: readonly (readonly [Option.Option<Field>, string])[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const builtins = ['cd', 'true', 'false', ':', 'break', 'continue', 'return', 'exit', 'eval', 'unset', 'read', 'shift', 'source', '.', 'echo', 'printf'] as const;
const special = ['PWD', 'OLDPWD', 'HOME', 'IFS'] as const;
const reading: ReadonlySet<number> = new Set([RedirOperator.RdrIn, RedirOperator.RdrInOut]);
const writing: ReadonlySet<number> = new Set([RedirOperator.RdrOut, RedirOperator.AppOut, RedirOperator.ClbOut, RedirOperator.AppClob, RedirOperator.RdrAll, RedirOperator.RdrAllClob, RedirOperator.AppAll, RedirOperator.AppAllClob, RedirOperator.RdrInOut]);
const feeding: ReadonlySet<number> = new Set([RedirOperator.Hdoc, RedirOperator.DashHdoc, RedirOperator.WordHdoc]);
const printing: ReadonlySet<number> = new Set([RedirOperator.RdrOut, RedirOperator.ClbOut, RedirOperator.AppOut, RedirOperator.AppClob]);
const appending: ReadonlySet<number> = new Set([RedirOperator.AppOut, RedirOperator.AppClob]);
const escapes: Readonly<Record<string, string>> = { '%%': '%', '\\\\': '\\', '\\n': '\n', '\\t': '\t' };

// --- [OPERATIONS] ----------------------------------------------------------------------

const referenced = (nodes: readonly Node[]): ReadonlySet<string> => {
    const names = new Set<string>(special);
    visit(nodes, (node) => {
        const written = Match.value(node).pipe(
            Match.discriminators('type')({ Lit: ({ value }) => value, ParamExp: ({ param }) => param?.value ?? '' }),
            Match.orElse(() => ''),
        );
        written.matchAll(/[A-Za-z_]\w*|[?@*#0-9]/gu).forEach(([name]) => {
            names.add(name);
        });
    });
    return names;
};
const literals = (node: Node, script: Script, state: State): ReadonlySet<string> => {
    const found = new Set<string>(Array.fromIterable(HashMap.values(state.variables)).flatMap((held) => [...held.values].flatMap(Binding.$match({ Known: ({ value }) => [value], Listed: ({ items }) => items, Unset: () => [], Unknown: () => [] }))));
    visit(node, (inner) => {
        if (inner.type === 'Word') {
            Option.toArray(text(inner, script.dialect)).forEach((written) => {
                found.add(written);
            });
        }
    });
    return found;
};
const cwd = (scope: Scope) =>
    Binding.$match(scope.get('PWD'), {
        Known: ({ value }) => Field.known(value),
        Unset: () => Field.unknown(Origin.Variable({ name: 'PWD', origins: HashSet.empty() })),
        Listed: () => Field.unknown(Origin.Variable({ name: 'PWD', origins: HashSet.empty() })),
        Unknown: ({ origins }) => Field.unknown(Origin.Variable({ name: 'PWD', origins })),
    });
const placed = (context: Context, scope: Scope) => Option.getOrElse(Option.as(context.remote, Field.unknown(Origin.Variable({ name: 'PWD', origins: HashSet.empty() }))), () => cwd(scope));
const positional = (words: readonly Written[]) =>
    Option.map(
        Option.liftPredicate(words, (all) => all.every(({ exact }) => exact)),
        Array.map((held) => held.binding),
    );
const supplied = (stmt: Stmt, scope: Scope, context: Context): Stdin =>
    Option.match(
        Array.findLast(stmt.redirs, ({ op, n }) => (reading.has(op) || feeding.has(op) || op === RedirOperator.DplIn) && (n === null || n.value === '0')),
        {
            onNone: () => context.stdin,
            onSome: ({ op, hdoc, word }) => {
                const expansion = Option.filter(
                    Option.map(Option.fromNullishOr(hdoc ?? word), (held) => context.script.expand(held, scope)[0]),
                    (expanded): expanded is Expanded => expanded._tag === 'Expanded',
                );
                const path = Option.getOrElse(
                    Option.flatMap(expansion, ({ fields }) => Array.head(fields)),
                    () => Field.unknown(Origin.Unsupported()),
                ).under(placed(context, scope));
                return Match.value({ fed: feeding.has(op), duplicated: op === RedirOperator.DplIn, closed: Option.contains(path.text, op === RedirOperator.DplIn ? '-' : '/dev/null') }).pipe(
                    Match.when({ fed: true }, () => Stdin.Text({ text: Option.map(Option.flatMap(expansion, Struct.get('text')), (body) => (hdoc === null ? `${body}\n` : body)) })),
                    Match.when({ closed: true }, () => Stdin.Closed()),
                    Match.when({ duplicated: true }, () => Stdin.File({ path: Field.unknown(Origin.Unsupported()) })),
                    Match.orElse(() => Stdin.File({ path })),
                );
            },
        },
    );
const echoed = (words: readonly string[]): Option.Option<string> => {
    const bare = words[0] === '-n';
    const shown = bare ? words.slice(1) : words;
    return Option.liftPredicate(`${shown.join(' ')}${bare ? '' : '\n'}`, () => !shown.some((word) => word.includes('\\')) && !Option.exists(Array.head(shown), (word) => word.startsWith('-')));
};
const formatted = ([format = '', ...args]: readonly string[]): Option.Option<string> => {
    const tokens = format.split(/(?<token>%%|%s|\\[\\nt])/u);
    const slots = tokens.filter((token) => token === '%s').length;
    const rounds = Array.makeBy(slots === 0 ? 1 : Math.max(1, Math.ceil(args.length / slots)), (round) => Array.mapAccum(tokens, round * slots, (at, token) => (token === '%s' ? ([at + 1, args[at] ?? ''] as const) : ([at, escapes[token] ?? token] as const)))[1].join(''));
    return Option.liftPredicate(rounds.join(''), () => !format.startsWith('-') && tokens.every((token, at) => at % 2 === 1 || !/[%\\]/u.test(token)));
};
const stored = (file: string, missing: Option.Option<string>, context: Context): Effect.Effect<Option.Option<string>> =>
    Effect.flatMap(Ref.get(context.files), (files) => Option.match(HashMap.get(files, file), { onNone: () => Effect.catchTag(context.read(file), 'NoSuchElementError', () => Effect.succeed(missing)), onSome: Effect.succeed }));
const contents = (path: Field, remote: Option.Option<Field>, context: Context): Effect.Effect<Option.Option<string>> =>
    Option.match(
        Option.filter(path.text, () => Option.isNone(remote)),
        { onNone: () => Effect.succeedNone, onSome: (file) => stored(file, Option.none(), context) },
    );
const invoked = (program: Program, stmt: Stmt, words: readonly Written[], scope: Scope, context: Context, usage: Option.Option<Usage> = Option.none(), assigns: Invocation['assigns'] = []) => {
    const base = placed(context, scope);
    const binding = bound(usage, words, base);
    const redirected = stmt.redirs.flatMap((redirect) =>
        Option.toArray(Option.fromNullishOr(redirect.word)).flatMap((word) => {
            const [expansion] = context.script.expand(word, scope);
            const fields = expansion._tag === 'Expanded' ? expansion.fields.slice(0, 1).map((field) => field.under(base)) : [Field.unknown(Origin.Unsupported())];
            return [{ op: redirect.op, printed: (redirect.n === null || redirect.n.value === '1') && printing.has(redirect.op), fields }];
        }),
    );
    const invocation = new Invocation({
        program,
        source: context.script.source,
        start: context.script.located(stmt.start),
        end: context.script.located(stmt.end),
        words,
        node: Option.liftPredicate(stmt, () => context.script === context.origin),
        assigns,
        usage: binding.usage,
        path: binding.path,
        flags: binding.flags,
        operands: binding.operands,
        rest: binding.rest,
        reads: [...binding.reads, ...redirected.filter(({ op }) => reading.has(op)).flatMap(({ fields }) => fields)],
        writes: [...binding.writes, ...redirected.filter(({ op }) => writing.has(op)).flatMap(({ fields }) => fields)],
        deletes: binding.deletes,
        opaque: Program.$match(program, { Function: () => false, Builtin: () => binding.opaque && Option.isSome(usage), External: () => binding.opaque, Unknown: () => true }),
        inner: [],
        place: Option.match(context.remote, { onNone: () => Place.Local({ cwd: base }), onSome: (host) => Place.Remote({ host }) }),
        within: context.within,
        stage: context.stage,
        stdin: context.stdin,
        background: context.background,
        repeated: context.loop,
    });
    return { invocation, binding, redirected };
};
const recorded = ({ invocation, binding, redirected }: ReturnType<typeof invoked>, name: string, context: Context): Effect.Effect<void> => {
    const words = Option.all(invocation.words.slice(1).map((held) => held.text));
    const copied = Option.filter(Option.some(invocation.stdin), Stdin.$is('Text')).pipe(
        Option.filter(() => binding.flags.length === 0 && Option.exists(binding.input, (slot) => slot.roles.includes('copy'))),
        Option.flatMap(Struct.get('text')),
    );
    const printed = Match.value(name).pipe(
        Match.when('echo', () => Option.flatMap(words, echoed)),
        Match.when('printf', () => Option.flatMap(words, formatted)),
        Match.orElse(() => copied),
    );
    const targets = [...binding.writes.map((field) => ({ field, text: copied, append: false })), ...redirected.filter(({ op }) => writing.has(op)).flatMap(({ op, printed: out, fields }) => fields.map((field) => ({ field, text: Option.filter(printed, () => out), append: appending.has(op) })))];
    const known = Option.isSome(context.remote) ? [] : targets.flatMap(({ field, text: written, append }) => Option.toArray(Option.map(field.text, (path) => ({ path, written, append }))));
    return Effect.forEach(known, ({ path, written, append }) => Effect.flatMap(append ? stored(path, Option.some(''), context) : Effect.succeedNone, (prior) => Ref.update(context.files, HashMap.set(path, append ? Option.map(Option.all([prior, written]), Array.join('')) : written))), { discard: true });
};
const nested = Effect.fnUntraced(function* (made: ReturnType<typeof invoked>, name: string, stmt: Stmt, environment: State, scope: Scope, context: Context): Effect.fn.Return<Invocation, ShellError> {
    const { invocation, binding } = made;
    const inside = { ...context, within: [Within.Wrapper({ name }), ...context.within], stage: Option.none(), background: false };
    const listed = { ...inside, stdin: Option.exists(binding.input, (slot) => slot.roles.includes('command')) ? Stdin.Closed() : invocation.stdin };
    const fed = Option.map(
        Option.filter(binding.input, (slot) => slot.roles.includes('script') || slot.roles.includes('shell')),
        (slot) => ({
            text: invocation.stdin._tag === 'Text' ? invocation.stdin.text : Option.none(),
            shell: slot.roles.includes('shell'),
            zero: Option.none<Written>(),
            operands: binding.operands.map(({ word }) => word),
            within: Option.none<Within>(),
            stdin: invocation.stdin._tag === 'Text' ? Stdin.Text({ text: Option.none() }) : invocation.stdin,
        }),
    );
    const lists = yield* Effect.forEach(
        binding.lists,
        Effect.fnUntraced(function* (list) {
            const program = Option.map(
                Option.flatMap(Array.head(list), (held) => held.text),
                (written) => written.slice(written.lastIndexOf('/') + 1),
            );
            const usage = Option.flatten(yield* Effect.transposeOption(Option.map(program, context.programs)));
            return yield* Option.match(program, {
                onNone: () => Effect.succeed(invoked(Program.Unknown({ origin: Origin.Command({ name: '' }) }), stmt, list, scope, listed).invocation),
                onSome: (short) => nested(invoked(Program.External({ name: short }), stmt, list, scope, listed, usage), short, stmt, environment, scope, listed),
            });
        }),
    );
    const remote = Option.orElse(
        Option.map(binding.host, ({ field }) => field),
        () => context.remote,
    );
    const shells = binding.files.filter(({ shell }) => shell);
    const bodies = yield* Effect.forEach(shells, (file) => contents(file.path, remote, context));
    const files = Array.zipWith(shells, bodies, (file, written) => ({ ...file, text: written, within: Option.map(file.path.text, (path) => Within.File({ path })), stdin: invocation.stdin }));
    const seen = new Set(context.visited.filter(([place]) => Equal.equals(place, remote)).map(([, body]) => body));
    const walkedScripts = yield* Effect.forEach(
        [...binding.scripts.map((body) => ({ ...body, within: Option.none<Within>(), stdin: invocation.stdin })), ...files, ...Option.toArray(fed)],
        Effect.fnUntraced(function* (body) {
            const interpreter = Option.isSome(binding.host)
                ? Option.some(posix('sh', {}))
                : yield* context.interpreter(
                      body.shell
                          ? Option.getOrElse(
                                Option.flatMap(Array.head(invocation.words), (held) => held.text),
                                () => name,
                            )
                          : 'sh',
                      context.script.dialect.environment,
                  );
            const source = Option.all({ written: Option.filter(body.text, (written) => !seen.has(written)), dialect: interpreter });
            const script = Option.flatten(yield* Effect.transposeOption(Option.map(source, ({ written, dialect }) => Effect.option(context.parse(written, dialect)))));
            const entered = (parsed: Script) =>
                Option.isSome(binding.host)
                    ? new State({ ...environment.havoc(), positional: Option.some([]), name: 'sh' })
                    : environment.entered(
                          parsed.scope,
                          parsed.dialect,
                          positional(body.operands),
                          Option.getOrElse(
                              Option.flatMap(body.zero, (held) => held.text),
                              () => parsed.dialect.binary,
                          ),
                      );
            return yield* Effect.transposeOption(
                Option.map(script, (parsed) =>
                    Effect.map(statements(parsed.syntax.stmts, entered(parsed), { ...inside, script: parsed, remote, visited: [...context.visited, [remote, parsed.source]], aliased: new Set(), within: [...Option.toArray(body.within), ...inside.within], stdin: body.stdin }), Struct.get('found')),
                ),
            );
        }),
    );
    return new Invocation({ ...invocation, inner: [...lists, ...Array.getSomes(walkedScripts).flat()], opaque: invocation.opaque || walkedScripts.some(Option.isNone) });
});
const included = Effect.fnUntraced(function* (made: ReturnType<typeof invoked>, state: State, context: Context) {
    const file = Array.head(made.binding.files);
    const seen = new Set(context.visited.filter(([place]) => Equal.equals(place, context.remote)).map(([, body]) => body));
    const written = Option.filter(Option.flatten(yield* Effect.transposeOption(Option.map(file, ({ path }) => contents(path, context.remote, context)))), (body) => !seen.has(body));
    const script = Option.flatten(yield* Effect.transposeOption(Option.map(written, (body) => Effect.option(context.parse(body, context.script.dialect)))));
    const given = Option.flatMap(file, ({ operands }) => Option.liftPredicate(operands, Array.isReadonlyArrayNonEmpty));
    const restored = (after: State) => (Option.isSome(given) ? new State({ ...after, positional: state.positional }) : after);
    const entered = Option.match(given, { onNone: () => state, onSome: (operands) => new State({ ...state, positional: positional(operands) }) });
    return yield* Option.match(Option.all({ parsed: script, path: Option.flatMap(file, ({ path }) => path.text) }), {
        onNone: () => Effect.succeed(Flow.ran(Option.some(state.havoc()), Option.some(state.havoc()), [new Invocation({ ...made.invocation, opaque: true })])),
        onSome: ({ parsed, path }) =>
            Effect.map(
                statements(parsed.syntax.stmts, entered, { ...context, script: parsed, within: [Within.File({ path }), ...context.within], visited: [...context.visited, [context.remote, parsed.source]] }),
                (flow) => new Flow({ ...Flow.none, zero: Option.map(State.union([flow.zero, flow.returns]), restored), nonzero: Option.map(flow.nonzero, restored), exits: flow.exits, found: [made.invocation, ...flow.found] }),
            ),
    });
});
const printers = (flow: Flow) => Array.dedupe(flow.found.flatMap(({ leaves }) => leaves).flatMap((invocation) => (invocation.inner.length === 0 ? [invocation.label] : [])));
const substituted = Effect.fnUntraced(function* (nodes: readonly Node[], state: State, context: Context) {
    const bodies: Extract<Node, { readonly type: 'CmdSubst' | 'ProcSubst' }>[] = [];
    visit(nodes, (node) => (node.type === 'CmdSubst' || node.type === 'ProcSubst' ? (bodies.push(node), false) : undefined));
    const flows = yield* Effect.forEach(bodies, (body) => statements(body.stmts, state, { ...context, within: [Within.Substitution(), ...context.within], stage: Option.none() }));
    return { found: flows.flatMap((flow) => flow.found), substitutions: HashMap.fromIterable(Array.zipWith(bodies, flows, (body, flow): readonly [Node, readonly string[]] => [body, printers(flow)])) };
});
const statements = (stmts: readonly Stmt[], state: State, context: Context): Effect.Effect<Flow, ShellError> =>
    Effect.reduce(
        stmts,
        () => new Flow({ ...Flow.none, zero: Option.some(state) }),
        (flow, stmt) =>
            Option.match(flow.next, {
                onNone: () => Effect.succeed(flow),
                onSome: (next) => Effect.map(statement(stmt, next, context), (ran) => flow.followed(ran)),
            }),
    );
const statement = Effect.fnUntraced(function* (stmt: Stmt, state: State, context: Context) {
    const detached = stmt.background ? { ...context, background: true, stdin: Stdin.Closed() } : context;
    const own = (command: Command) => (command.type === 'CallExpr' ? detached : { ...detached, stdin: Option.match(Array.head(state.scopes(referenced(stmt.redirs))), { onNone: () => detached.stdin, onSome: (scope) => supplied(stmt, scope, detached) }) });
    const ran = yield* Option.match(Option.fromNullishOr(stmt.cmd), {
        onNone: () => Effect.succeed(Flow.ran(Option.some(state), Option.none())),
        onSome: (command) => (stmt.background ? Effect.map(commanded(command, stmt, state, own(command)), (flow) => Flow.ran(Option.some(state), Option.none(), flow.found)) : commanded(command, stmt, state, own(command))),
    });
    return stmt.negated ? new Flow({ ...ran, zero: ran.nonzero, nonzero: ran.zero }) : ran;
});
const looped = (head: State, known: ReadonlySet<string>, iteration: (state: State) => Effect.Effect<{ readonly flow: Flow; readonly again: Option.Option<State>; readonly out: Option.Option<State> }, ShellError>): Effect.Effect<Flow, ShellError> =>
    Effect.flatMap(iteration(head), ({ flow, again, out }) => {
        const widened = head.widen(
            Option.getOrElse(State.union([Option.some(head), again, flow.continues]), () => head),
            known,
        );
        return Equal.equals(widened, head) ? Effect.succeed(new Flow({ ...Flow.none, zero: State.union([out, flow.breaks]), returns: flow.returns, exits: flow.exits, found: flow.found })) : Effect.map(looped(widened, known, iteration), (rest) => new Flow({ ...rest, found: [...flow.found, ...rest.found] }));
    });
const called = Effect.fnUntraced(function* (body: Stmt, script: Script, name: string, operands: Option.Option<readonly Binding[]>, state: State, context: Context) {
    const locals = new Set<string>();
    visit(body, (node) => {
        if (node.type === 'DeclClause') {
            node.args
                .flatMap((assign) => Option.toArray(Option.fromNullishOr(assign.name)))
                .forEach(({ value }) => {
                    locals.add(value);
                });
        }
    });
    const flow = yield* statements([body], new State({ ...state, positional: operands }), { ...context, script, within: [Within.Function({ name }), ...context.within], calls: [...context.calls, body] });
    const restored = (after: State) => new State({ ...after, positional: state.positional, variables: locals.values().reduce((variables, local) => HashMap.set(variables, local, state.variable(local)), after.variables) });
    return new Flow({ ...Flow.none, zero: Option.map(State.union([flow.zero, flow.returns]), restored), nonzero: Option.map(flow.nonzero, restored), exits: flow.exits, found: flow.found });
});
const aliasing = (call: CallExpr, stmt: Stmt, context: Context): Option.Option<{ readonly text: string; readonly names: readonly string[] }> => {
    const { dialect, located, source } = context.script;
    const replaced = call.args.flatMap((word, at) => {
        const [part] = word.parts;
        const name = word.parts.length === 1 && part?.type === 'Lit' && !part.value.includes('\\') ? Option.some(part.value) : Option.none<string>();
        const alias = Option.flatMap(name, (held) =>
            Option.map(
                Option.filter(Option.fromUndefinedOr(dialect.aliases.get(held)), ({ global }) => dialect.options.aliases && !context.aliased.has(held) && (global || at === 0)),
                ({ text: written }) => ({ name: held, text: written }),
            ),
        );
        const suffix = Option.flatMap(
            Option.filter(name, (held) => at === 0 && dialect.options.aliases && held.includes('.')),
            (held) => Option.map(Option.fromUndefinedOr(dialect.suffixes.get(held.slice(held.lastIndexOf('.') + 1))), (program) => ({ name: held, text: `${program} ${held}` })),
        );
        return Option.toArray(
            Option.map(
                Option.orElse(alias, () => suffix),
                (found) => ({ ...found, start: located(word.start), end: located(word.end) }),
            ),
        );
    });
    const [start, end] = [located(stmt.start), located(stmt.end)];
    return Option.map(Array.match(replaced, { onEmpty: Option.none, onNonEmpty: Option.some }), (found) => {
        const [at, pieces] = Array.mapAccum(found, start, (from, { text: put, start: cut, end: resumed }) => [resumed, `${source.slice(from, cut)}${put}`] as const);
        return { text: `${pieces.join('')}${source.slice(at, end)}`, names: found.map(({ name }) => name) };
    });
};
const simple = Effect.fnUntraced(function* (call: CallExpr, stmt: Stmt, state: State, context: Context) {
    const aliased = aliasing(call, stmt, context);
    if (Option.isSome(aliased)) {
        const script = yield* context.parse(aliased.value.text, context.script.dialect);
        return yield* statements(script.syntax.stmts, state, { ...context, script, aliased: new Set([...context.aliased, ...aliased.value.names]) });
    }
    const { found, substitutions } = yield* substituted([call, ...stmt.redirs], state, context);
    const flows = yield* Effect.forEach(state.scopes(referenced([call, ...stmt.redirs]), substitutions), (scope) => concrete(call, stmt, state, scope, context));
    const flow = Flow.joined(flows);
    return new Flow({ ...flow, found: [...found, ...flow.found] });
});
const concrete = Effect.fnUntraced(function* (call: CallExpr, stmt: Stmt, state: State, scope: Scope, context: Context) {
    const { script } = context;
    const [after, expansions] = Array.mapAccum(call.args, scope, (from, word) => {
        const [expansion, to] = script.expand(word, from);
        return [to, expansion] as const;
    });
    const [settled, values] = Array.mapAccum(call.assigns, after, (from, assign) => {
        const [expansion, to] = Option.match(Option.fromNullishOr(assign.value), { onNone: () => [new Expanded({ fields: [Field.known('')], exact: true, refusals: [] }), from] as const, onSome: (word: Word) => script.expand(word, from) });
        return [to, { name: assign.name?.value ?? '', word: assign.value, expansion, array: assign.array !== null || assign.index !== null || assign.append }] as const;
    });
    const current = state.absorbed(scope, settled);
    const own = { ...context, stdin: supplied(stmt, scope, context) };
    const sourced = [...Array.zip(call.args, expansions), ...values.map(({ word, expansion }) => [word, expansion] as const)];
    const refused = sourced.some(([, expansion]) => expansion._tag === 'Refused');
    const fielded = sourced.map(([word, expansion]) => (expansion._tag === 'Expanded' ? expansion.fields.map((field) => new Written({ field, exact: expansion.exact, node: expansion.fields.length === 1 && context.script === context.origin ? Option.fromNullishOr(word) : Option.none() })) : []));
    const words = fielded.slice(0, call.args.length).flat();
    const assigns = values.flatMap(({ name: assigned }, index) => Array.take(fielded[call.args.length + index] ?? [], 1).map((value) => ({ name: assigned, value })));
    const assigned = values.reduce((held, { name, expansion, array }) => held.assign(name, HashSet.make(array ? Binding.Unknown({ origins: HashSet.empty() }) : expansion.binding), script.dialect.options.allexport || held.variable(name).exported), current);
    const head = Array.head(words);
    const name = Option.flatMap(head, ({ field, exact }) => Option.filter(field.text, () => exact));
    const stated = Match.value({ refused, empty: Option.isNone(head), name: Option.getOrUndefined(name) }).pipe(
        Match.when({ refused: true }, () => Effect.succeed(new Flow({ ...Flow.none, exits: Option.some(current) }))),
        Match.when({ empty: true }, () => Effect.succeed(Flow.ran(Option.some(assigned), Option.none()))),
        Match.when({ name: Match.undefined }, () => {
            const made = invoked(Program.Unknown({ origin: Origin.Command({ name: '' }) }), stmt, words, scope, own, Option.none(), assigns);
            return Effect.as(recorded(made, '', own), Flow.ran(Option.some(current.havoc()), Option.some(current.havoc()), [made.invocation]));
        }),
        Match.orElse(({ name: program = '' }) => resolved(program, words, assigns, stmt, current, scope, own)),
    );
    return yield* stated;
});
const resolved = Effect.fnUntraced(function* (name: string, words: Invocation['words'], assigns: Invocation['assigns'], stmt: Stmt, state: State, scope: Scope, context: Context) {
    const texts = Option.all(words.slice(1).map(({ field, exact }) => Option.filter(field.text, () => exact)));
    const own = context.script.dialect.builtins.has(name);
    const usage = yield* context.programs(own ? name : name.slice(name.lastIndexOf('/') + 1));
    const flows = yield* Effect.forEach(state.callees(name), (callee) =>
        Callee.$match(callee, {
            Defined: ({ body, script }) =>
                context.calls.includes(body)
                    ? Effect.succeed(Flow.ran(Option.some(state.havoc()), Option.some(state.havoc()), [invoked(Program.Unknown({ origin: Origin.Recursion() }), stmt, words, scope, context, Option.none(), assigns).invocation]))
                    : Effect.map(called(body, script, name, positional(words.slice(1)), state, context), (flow) => new Flow({ ...flow, found: [invoked(Program.Function({ name }), stmt, words, scope, context, Option.none(), assigns).invocation, ...flow.found] })),
            Absent: () => builtin(name, usage, words, assigns, stmt, state, scope, context, texts),
        }),
    );
    return Flow.joined(flows);
});
const builtin = (name: string, usage: Option.Option<Usage>, words: Invocation['words'], assigns: Invocation['assigns'], stmt: Stmt, state: State, scope: Scope, context: Context, texts: Option.Option<readonly string[]>): Effect.Effect<Flow, ShellError> => {
    const own = context.script.dialect.builtins.has(name);
    const short = name.slice(name.lastIndexOf('/') + 1);
    const made = invoked(own ? Program.Builtin({ name }) : Program.External({ name: short }), stmt, words, scope, context, usage, assigns);
    const found = [made.invocation];
    const operands = Option.getOrElse(texts, () => []);
    const pwd = Binding.$match(scope.get('PWD'), { Known: ({ value }) => Option.some(value), Unset: () => Option.none(), Listed: () => Option.none(), Unknown: () => Option.none() });
    const target = Option.flatMap(
        Option.filter(texts, () => !operands.some((operand) => operand.startsWith('-'))),
        (given) => Option.orElse(Array.head(given), () => Option.flatMap(Option.some(scope.get('HOME')), (home) => (home._tag === 'Known' ? Option.some(home.value) : Option.none()))),
    );
    const moved = Option.map(
        Option.flatMap(target, (dir) => (dir.startsWith('/') ? Option.some(dir) : Option.map(pwd, (base) => `${base}/${dir}`))),
        (path) =>
            Field.known(path)
                .under(Field.known('/'))
                .text.pipe(Option.getOrElse(() => path)),
    );
    return Match.value(own ? name : '').pipe(
        Match.when(Match.is(':', 'true'), () => Effect.succeed(Flow.ran(Option.some(state), Option.none(), found))),
        Match.when('false', () => Effect.succeed(Flow.ran(Option.none(), Option.some(state), found))),
        Match.when('break', () => Effect.succeed(new Flow({ ...Flow.none, breaks: Option.some(state), found }))),
        Match.when('continue', () => Effect.succeed(new Flow({ ...Flow.none, continues: Option.some(state), found }))),
        Match.when('return', () => Effect.succeed(new Flow({ ...Flow.none, returns: Option.some(state), found }))),
        Match.when('exit', () => Effect.succeed(new Flow({ ...Flow.none, exits: Option.some(state), found }))),
        Match.when('cd', () => Effect.succeed(Flow.ran(Option.some(state.bound('OLDPWD', scope.get('PWD')).bound('PWD', Option.match(moved, { onNone: () => Binding.Unknown({ origins: HashSet.empty() }), onSome: (value) => Binding.Known({ value }) }))), Option.some(state), found))),
        Match.when('unset', () => Effect.succeed(Flow.ran(Option.some(Option.isSome(texts) ? operands.filter((operand) => !operand.startsWith('-')).reduce((held, operand) => held.bound(operand, Binding.Unset()), state) : state.havoc()), Option.some(state), found))),
        Match.when('read', () => Effect.succeed(Flow.ran(Option.some(Option.isSome(texts) ? [...operands, 'REPLY'].reduce((held, operand) => held.bound(operand, Binding.Unknown({ origins: HashSet.make(Origin.Input()) })), state) : state.havoc()), Option.some(state), found))),
        Match.when('shift', () => Effect.succeed(Flow.ran(Option.some(new State({ ...state, positional: Option.flatMap(state.positional, (held) => Option.map(Option.liftPredicate(Number(operands[0] ?? '1'), Number.isSafeInteger), (count) => held.slice(count))) })), Option.some(state), found))),
        Match.when(Match.is('source', '.'), () => included(made, state, context)),
        Match.when('eval', () =>
            Option.match(texts, {
                onNone: () => Effect.succeed(Flow.ran(Option.some(state.havoc()), Option.some(state.havoc()), [...found, invoked(Program.Unknown({ origin: Origin.Script() }), stmt, [], scope, context).invocation])),
                onSome: (given) => Effect.flatMap(context.parse(given.join(' '), context.script.dialect), (script) => Effect.map(statements(script.syntax.stmts, state, { ...context, script, within: [Within.Eval(), ...context.within] }), (flow) => new Flow({ ...flow, found: [...found, ...flow.found] }))),
            }),
        ),
        Match.orElse(() =>
            Effect.map(
                nested(
                    made,
                    short,
                    stmt,
                    assigns.reduce((held, { name: assigned, value }) => held.assign(assigned, HashSet.make(value.binding), true), state),
                    scope,
                    context,
                ).pipe(Effect.tap(() => recorded(made, own ? name : '', context))),
                (invocation) => Flow.ran(Option.some(state), Option.some(state), [invocation]),
            ),
        ),
    );
};
const commanded = (command: Command, stmt: Stmt, state: State, context: Context): Effect.Effect<Flow, ShellError> =>
    Match.value(command).pipe(
        Match.discriminatorsExhaustive('type')({
            CallExpr: (call) => simple(call, stmt, state, context),
            Block: ({ stmts }) => statements(stmts, state, context),
            Subshell: ({ stmts }) => Effect.map(statements(stmts, state, { ...context, within: [Within.Subshell(), ...context.within] }), (flow) => Flow.ran(Option.as(flow.zero, state), Option.as(State.union([flow.nonzero, flow.exits]), state), flow.found)),
            BinaryCmd: (binary) => paired(binary.op, binary.x, binary.y, binary, state, context),
            IfClause: (clause) =>
                clause.cond.length === 0
                    ? statements(clause.then, state, context)
                    : Effect.gen(function* () {
                          const tested = yield* statements(clause.cond, state, context);
                          const chosen = yield* Option.match(tested.zero, { onNone: () => Effect.succeed(Flow.none), onSome: (zero) => statements(clause.then, zero, context) });
                          const other = yield* Option.match(tested.nonzero, { onNone: () => Effect.succeed(Flow.none), onSome: (nonzero) => (clause.else === null ? Effect.succeed(Flow.ran(Option.some(nonzero), Option.none())) : commanded(clause.else, stmt, nonzero, context)) });
                          return new Flow({ ...tested, zero: Option.none(), nonzero: Option.none(), found: [] })
                              .join(chosen)
                              .join(other)
                              .pipe((joined) => new Flow({ ...joined, found: [...tested.found, ...chosen.found, ...other.found] }));
                      }),
            WhileClause: (clause) => {
                const loop = { source: context.script.source, start: context.script.located(clause.start), end: context.script.located(clause.end) };
                return looped(state, literals(clause, context.script, state), (head) =>
                    Effect.gen(function* () {
                        const tested = yield* statements(clause.cond, head, { ...context, loop: Option.some(new Loop({ ...loop, condition: true })) });
                        const [enter, leave] = clause.until ? [tested.nonzero, tested.zero] : [tested.zero, tested.nonzero];
                        const body = yield* Option.match(enter, { onNone: () => Effect.succeed(Flow.none), onSome: (entered) => statements(clause.do, entered, { ...context, loop: Option.some(new Loop({ ...loop, condition: false })) }) });
                        return { flow: new Flow({ ...body, exits: State.union([tested.exits, body.exits]), returns: State.union([tested.returns, body.returns]), found: [...tested.found, ...body.found] }), again: body.next, out: leave };
                    }),
                );
            },
            ForClause: (clause) => iterated(clause, state, context),
            CaseClause: (clause) => cased(clause, state, context),
            FuncDecl: (declared) =>
                Effect.succeed(
                    Flow.ran(
                        Option.some(new State({ ...state, functions: [...Option.toArray(Option.fromNullishOr(declared.name)), ...declared.names].reduce((functions, { value }) => HashMap.set(functions, value, HashSet.make(Callee.Defined({ body: declared.body ?? stmt, script: context.script }))), state.functions) })),
                        Option.none(),
                    ),
                ),
            DeclClause: (clause) => Effect.map(substituted([clause], state, context), (walked) => declared(clause, stmt, state, context, walked)),
            ArithmCmd: (arithmetic) => Effect.map(substituted([arithmetic], state, context), ({ found }) => Flow.ran(Option.some(unknowing(arithmetic, state)), Option.some(unknowing(arithmetic, state)), found)),
            LetClause: (clause) => Effect.map(substituted([clause], state, context), ({ found }) => Flow.ran(Option.some(unknowing(clause, state)), Option.some(unknowing(clause, state)), found)),
            TestClause: (clause) => Effect.map(substituted([clause], state, context), ({ found }) => Flow.ran(Option.some(state), Option.some(state), found)),
            TimeClause: (clause) => Option.match(Option.fromNullishOr(clause.stmt), { onNone: () => Effect.succeed(Flow.ran(Option.some(state), Option.none())), onSome: (inner) => statement(inner, state, context) }),
            TestDecl: (clause) => Option.match(Option.fromNullishOr(clause.body), { onNone: () => Effect.succeed(Flow.ran(Option.some(state), Option.none())), onSome: (inner) => statement(inner, state, context) }),
            CoprocClause: (clause) =>
                Option.match(Option.fromNullishOr(clause.stmt), { onNone: () => Effect.succeed(Flow.ran(Option.some(state), Option.none())), onSome: (inner) => Effect.map(statement(inner, state, { ...context, background: true }), (flow) => Flow.ran(Option.some(state), Option.none(), flow.found)) }),
            TryClause: (clause) => statements([...(clause.try?.stmts ?? []), ...(clause.always?.stmts ?? [])], state, context),
            RepeatClause: (clause) => looped(state, literals(clause, context.script, state), (head) => Effect.map(statements(clause.do, head, context), (flow) => ({ flow, again: flow.next, out: Option.some(head) }))),
        }),
    );
const unknowing = (node: Node, state: State) => [...referenced([node])].filter((name) => /^[A-Za-z_]\w*$/u.test(name)).reduce((held, name) => held.bound(name, Binding.Unknown({ origins: HashSet.make(Origin.Arithmetic()) })), state);
const declared = (clause: Extract<Command, { readonly type: 'DeclClause' }>, stmt: Stmt, state: State, context: Context, { found, substitutions }: Effect.Success<ReturnType<typeof substituted>>): Flow => {
    const exported = clause.variant?.value === 'export';
    const [scope] = state.scopes(referenced([clause]), substitutions);
    const named = clause.args.flatMap((assign) => Option.toArray(Option.map(Option.fromNullishOr(assign.name), ({ value }) => ({ value, assign }))));
    const next = named.reduce(
        (held, { value, assign }) => {
            const [expansion] = assign.value === null || scope === undefined ? [undefined] : context.script.expand(assign.value, scope);
            const binding = assign.naked
                ? held.variable(value).single
                : Option.match(
                      Option.filter(Option.fromUndefinedOr(expansion), () => assign.array === null),
                      { onNone: () => Binding.Unknown({ origins: HashSet.empty() }), onSome: (written) => written.binding },
                  );
            return held.assign(value, HashSet.make(binding), exported || held.variable(value).exported);
        },
        clause.args.some((assign) => assign.name === null && assign.value !== null && !(assign.value.parts[0]?.type === 'Lit' && /^[-+]/u.test(assign.value.parts[0].value))) ? state.havoc() : state,
    );
    return Flow.ran(Option.some(next), Option.none(), [...found, ...(scope === undefined ? [] : [invoked(Program.Builtin({ name: clause.variant?.value ?? '' }), stmt, [], scope, context).invocation])]);
};
const paired = Effect.fnUntraced(function* (op: number, x: Stmt | null, y: Stmt | null, node: Node, state: State, context: Context) {
    const stages = (stmt: Stmt | null): readonly Stmt[] => (stmt?.cmd?.type === 'BinaryCmd' && (stmt.cmd.op === BinCmdOperator.Pipe || stmt.cmd.op === BinCmdOperator.PipeAll) && !stmt.negated ? [...stages(stmt.cmd.x), ...stages(stmt.cmd.y)] : Option.toArray(Option.fromNullishOr(stmt)));
    const left = yield* Option.match(Option.fromNullishOr(x), { onNone: () => Effect.succeed(Flow.ran(Option.some(state), Option.none())), onSome: (stmt) => (op === BinCmdOperator.AndStmt || op === BinCmdOperator.OrStmt ? statement(stmt, state, context) : Effect.succeed(Flow.none)) });
    const piped = op === BinCmdOperator.Pipe || op === BinCmdOperator.PipeAll;
    const all = [...stages(x), ...stages(y)];
    const ran = yield* Effect.forEach(all, (stage, index) => statement(stage, state, { ...context, stage: Option.some({ pipeline: context.script.located(node.start), index, count: all.length }), stdin: index === 0 ? context.stdin : Stdin.Piped({ pipeline: context.script.located(node.start), index: index - 1 }) }));
    const last = Array.last(ran).pipe(Option.getOrElse(() => Flow.none));
    const kept = context.script.dialect.options.lastpipe ? last : Flow.ran(Option.as(last.zero, state), Option.as(State.union([last.nonzero, last.exits]), state));
    const right = (from: Option.Option<State>) => Option.match(Option.all([from, Option.fromNullishOr(y)]), { onNone: () => Effect.succeed(Flow.none), onSome: ([start, stmt]) => statement(stmt, start, context) });
    return yield* Match.value(op).pipe(
        Match.when(BinCmdOperator.AndStmt, () => Effect.map(right(left.zero), (after) => new Flow({ ...left, zero: Option.none() }).join(after))),
        Match.when(BinCmdOperator.OrStmt, () => Effect.map(right(left.nonzero), (after) => new Flow({ ...left, nonzero: Option.none() }).join(after))),
        Match.orElse(() => Effect.succeed(new Flow({ ...kept, found: ran.flatMap((flow) => flow.found) }).pipe((flow) => (piped ? flow : Flow.none)))),
    );
});
const iterated = Effect.fnUntraced(function* (clause: Extract<Command, { readonly type: 'ForClause' }>, state: State, context: Context) {
    const loop = { source: context.script.source, start: context.script.located(clause.start), end: context.script.located(clause.end) };
    const inner = { ...context, loop: Option.some(new Loop({ ...loop, condition: false })) };
    const iteration = clause.loop?.type === 'WordIter' ? clause.loop : undefined;
    const name = iteration?.name?.value ?? '';
    const { found, substitutions } = yield* substituted(iteration?.items ?? [], state, context);
    const scopes = state.scopes(referenced(iteration?.items ?? []), substitutions);
    const expansions = scopes.map(
        (scope) =>
            Array.mapAccum(iteration?.items ?? [], scope, (from, word) => {
                const [expansion, to] = context.script.expand(word, from);
                return [to, expansion] as const;
            })[1],
    );
    const items = Option.flatMap(
        Option.liftPredicate(expansions, (all) => all.length === 1 && iteration !== undefined && iteration.items.length > 0),
        ([held = []]) => Option.all(held.flatMap((expansion) => (expansion._tag === 'Expanded' && expansion.exact ? expansion.fields.map(Struct.get('text')) : [Option.none<string>()]))),
    );
    const entry = Binding.Unknown({ origins: HashSet.fromIterable(expansions.flat().flatMap((expansion) => (expansion.binding._tag === 'Unknown' ? [...expansion.binding.origins] : [Origin.Literal()]))) });
    const exact = Effect.reduce(
        Option.getOrElse(items, () => []),
        () => new Flow({ ...Flow.none, zero: Option.some(state), found }),
        (flow, item) =>
            Option.match(flow.next, {
                onNone: () => Effect.succeed(flow),
                onSome: (next) =>
                    Effect.map(statements(clause.do, next.bound(name, Binding.Known({ value: item })), inner), (ran) =>
                        new Flow({ ...flow.followed(ran), zero: State.union([ran.next, ran.continues]), nonzero: Option.none(), breaks: Option.none(), continues: Option.none() }).join(new Flow({ ...Flow.none, zero: ran.breaks })),
                    ),
            }),
    );
    const widened = looped(clause.loop?.type === 'CStyleLoop' ? unknowing(clause.loop, state) : state.bound(name, entry), literals(clause, context.script, state), (head) =>
        Effect.map(statements(clause.do, clause.loop?.type === 'CStyleLoop' ? unknowing(clause.loop, head) : head.bound(name, entry), inner), (flow) => ({ flow, again: flow.next, out: Option.some(head) })),
    );
    return yield* Option.isSome(items) ? exact : Effect.map(widened, (flow) => new Flow({ ...flow, found: [...found, ...flow.found] }));
});
const cased = Effect.fnUntraced(function* (clause: Extract<Command, { readonly type: 'CaseClause' }>, state: State, context: Context) {
    const { found, substitutions } = yield* substituted([clause], state, context);
    const [scope] = state.scopes(referenced([clause]), substitutions);
    const [subject] = clause.word === null || scope === undefined ? [undefined] : context.script.expand(clause.word, scope);
    const verdicts = Option.match(Option.product(subject?._tag === 'Expanded' ? subject.text : Option.none(), Option.fromUndefinedOr(scope)), {
        onNone: () => clause.items.map(({ patterns }) => patterns.map(() => Option.none<boolean>())),
        onSome: ([given, at]) =>
            clause.items.map(({ patterns }) =>
                patterns.map((pattern) => {
                    const [expansion] = context.script.expand(pattern, at);
                    return expansion._tag === 'Expanded' ? expansion.matches(given) : Option.none();
                }),
            ),
    });
    const [left, flows] = yield* Effect.reduce(
        Array.zip(clause.items, verdicts),
        () => [Option.some(state), [] as readonly Flow[]] as const,
        ([reaching, done], [item, verdict]) =>
            Option.match(reaching, {
                onNone: () => Effect.succeed([reaching, done] as const),
                onSome: (from) => {
                    const certain = verdict.some(Option.contains(true));
                    const never = verdict.every(Option.contains(false));
                    return never ? Effect.succeed([reaching, done] as const) : Effect.map(statements(item.stmts, from, context), (flow) => [certain ? Option.none<State>() : reaching, [...done, flow]] as const);
                },
            }),
    );
    const joined = Flow.joined([...flows, Flow.ran(left, Option.none())]);
    return new Flow({ ...joined, found: [...found, ...joined.found] });
});
const walked = Effect.fnUntraced(function* (script: Script, hosts: Pick<Context, 'parse' | 'programs' | 'interpreter' | 'read'>) {
    const flow = yield* statements(script.syntax.stmts, State.started(script.scope, script.dialect), {
        ...hosts,
        files: yield* Ref.make(HashMap.empty<string, Option.Option<string>>()),
        stdin: Stdin.Inherited(),
        script,
        origin: script,
        remote: Option.none(),
        visited: [[Option.none(), script.source]],
        within: [],
        loop: Option.none(),
        stage: Option.none(),
        background: false,
        calls: [],
        aliased: new Set(),
    });
    return Array.mapAccum(flow.found, HashSet.empty<Invocation>(), (seen, invocation) => [HashSet.add(seen, invocation), HashSet.has(seen, invocation) ? [] : [invocation]] as const)[1].flat();
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { builtins, walked };
