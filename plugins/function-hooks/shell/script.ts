import { Array, Effect, Equal, Match, Option, Predicate, Result, Schema, Struct } from 'effect';
import { HookError, Host } from '#/host';

// --- [TYPES] ---------------------------------------------------------------------------

type Node = { readonly Pos: Offset; readonly End: Offset; readonly Semicolon?: Offset } & { readonly [Key in 'Type' | 'Op' | 'Value']?: string } & { readonly [Key in 'Dollar' | 'Background' | 'TempFile' | 'ReplyVar']?: boolean } & {
    readonly [Key in 'Cmd' | 'X' | 'Y' | 'Else' | 'Loop' | 'Body' | 'Word' | 'Hdoc' | 'N' | 'Name' | 'Param']?: Node;
} & {
    readonly [Key in 'Stmts' | 'Parts' | 'Redirs' | 'Cond' | 'Then' | 'Do' | 'Items' | 'Patterns']?: readonly Node[];
} & { readonly [Key in 'Args' | 'Assigns']?: readonly Assign[] };
interface Assign extends Pick<Node, 'Pos' | 'End' | 'Parts' | 'Name'> {
    readonly Value?: Node;
    readonly Array?: unknown;
    readonly Append?: boolean;
    readonly Naked?: boolean;
}
interface Span {
    readonly start: number;
    readonly end: number;
}
interface Word extends Span {
    readonly raw: string;
    readonly text: string;
    readonly known: boolean;
    readonly glob: boolean;
    readonly spread: boolean;
}
interface Redirect extends Span {
    readonly raw: string;
    readonly op: string;
    readonly descriptor: Option.Option<string>;
    readonly target: Option.Option<Word>;
}
interface Surroundings {
    readonly source: Uint8Array;
    readonly fed: boolean;
    readonly pipes: boolean;
    readonly stage: Option.Option<{ readonly pipeline: object; readonly at: number }>;
    readonly looping: boolean;
    readonly busy: boolean;
    readonly substituted: boolean;
    readonly origin: 'call' | 'body' | 'file';
}
interface Command extends Span, Surroundings {
    readonly words: readonly Word[];
    readonly redirects: readonly Redirect[];
    readonly cwd: Option.Option<string>;
    readonly bound: ReadonlyMap<string, Option.Option<string>>;
    readonly input: Option.Option<string>;
}
interface Script {
    readonly commands: readonly Command[];
    readonly jobs: readonly (Span & Pick<Surroundings, 'origin'>)[];
}
interface Beginning extends Pick<Command, 'cwd' | 'origin'>, Partial<Pick<Surroundings, 'fed' | 'looping'>> {
    readonly visited?: ReadonlySet<string>;
}
interface Frame {
    readonly values: ReadonlyMap<string, Option.Option<string>>;
    readonly opaque: boolean;
    readonly functions: ReadonlySet<string>;
}
interface Place extends Surroundings {
    readonly read: (name: string) => Option.Option<string>;
    readonly resolve: (typeof Host.Service)['place'];
    readonly home: Option.Option<string>;
    readonly dry: boolean;
}
type Walked = readonly [Frame, Script];
type Piece = Pick<Word, 'text' | 'known' | 'glob' | 'spread'>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const DIRECTORY = 'PWD';
const REFERENCE = /(?<=\$\{?)[A-Za-z_]\w*/gu;
const DECODER = new TextDecoder();
const ANSI_ESCAPE = /\\(?:(?<code>[abeEfnrtv\\'"?])|(?<octal>[0-7]{1,3})|x(?<hex>[\da-fA-F]{1,2})|[uU](?<wide>[\da-fA-F]{1,8}))/gu;
const ANSI_CODES = { a: '\x07', b: '\b', e: '\x1B', E: '\x1B', f: '\f', n: '\n', r: '\r', t: '\t', v: '\v', '\\': '\\', "'": "'", '"': '"', '?': '?' } as const;
const UNQUOTED_ESCAPE = /\\(?<code>[\s\S])/gu;
const QUOTED_ESCAPE = /\\(?<code>["\\$`\n])/gu;
const GLOB = /[*?[{]/u;
const SPREADS = /\s|^$/u;
const FIELD = /\S+/gu;
const TILDE = /^~(?<user>[^/]*)/u;
const NAME = /^[A-Za-z_]\w*$/u;
const INPUT: ReadonlySet<string | undefined> = new Set(['<', '<<', '<<-', '<<<', '<&']);
const HEREDOC: ReadonlySet<string | undefined> = new Set(['<<', '<<-', '<<<']);
const LISTING: ReadonlySet<string | undefined> = new Set(['&&', '||']);
const PIPING: ReadonlySet<string | undefined> = new Set(['|', '|&']);
const PLAIN_EXPANSION: ReadonlySet<string> = new Set(['Type', 'Pos', 'End', 'Dollar', 'Rbrace', 'Short', 'Param']);
const SHIFTING: ReadonlySet<string> = new Set(['OLDPWD', 'SHLVL', '_']);
const IDLE: ReadonlySet<string | undefined> = new Set([':', 'true']);
const OPAQUE: ReadonlySet<string> = new Set(['eval', 'source', '.']);
const PREFIXES: ReadonlySet<string> = new Set(['builtin', 'command']);
const ASSIGNING: ReadonlySet<string> = new Set(['read', 'readarray', 'mapfile', 'getopts', 'printf', 'unset']);
const ENTERING: ReadonlySet<string> = new Set(['cd', 'pushd', 'popd']);
const NOTHING: Script = { commands: [], jobs: [] };

// --- [MODELS] --------------------------------------------------------------------------

const Offset = Schema.Struct({ Offset: Schema.Finite });
type Offset = typeof Offset.Type;
const isNode: (value: unknown) => value is Node = Schema.is(Schema.Struct({ Pos: Offset, End: Offset }));
const Tree = Schema.fromJsonString(Schema.Struct({ Stmts: Schema.optionalKey(Schema.Array(Schema.declare(isNode))) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [WORDS]

const sliced = ({ Pos, End }: Pick<Node, 'Pos' | 'End'>, { source }: Place): string => DECODER.decode(source.subarray(Pos.Offset, End.Offset));

const spanOf = ({ Pos, End }: Pick<Node, 'Pos' | 'End'>): Span => ({ start: Pos.Offset, end: End.Offset });

const looked = (frame: Frame, name: string, place: Place): Option.Option<string> => frame.values.get(name) ?? (frame.opaque || SHIFTING.has(name) ? Option.none() : place.read(name));

const ansiDecoded = (_all: string, code?: keyof typeof ANSI_CODES, octal?: string, hex?: string, wide?: string): string => (code === undefined ? String.fromCodePoint(Number(octal === undefined ? `0x${hex ?? wide}` : `0o${octal}`)) : ANSI_CODES[code]);

const pieceOf = ({ Value = '', ...part }: Node, frame: Frame, place: Place, quoted: boolean): Piece =>
    Match.value(part.Type).pipe(
        Match.when('Lit', () => ({ text: Value.replaceAll(quoted ? QUOTED_ESCAPE : UNQUOTED_ESCAPE, (_all, code: string) => (code === '\n' ? '' : code)), known: true, glob: !quoted && GLOB.test(Value), spread: false })),
        Match.when('SglQuoted', () => ({ text: part.Dollar === true ? Value.replaceAll(ANSI_ESCAPE, ansiDecoded) : Value, known: true, glob: false, spread: false })),
        Match.when('DblQuoted', () => pieces(part.Parts ?? [], frame, place, true)),
        Match.when('ParamExp', () => {
            const value = new Set(Object.keys(part)).isSubsetOf(PLAIN_EXPANSION) && part.Param?.Value !== undefined ? looked(frame, part.Param.Value, place) : Option.none();
            const text = Option.getOrElse(value, () => sliced(part, place));
            const splits = !quoted && Option.isSome(value);
            return { text, known: Option.isSome(value), glob: splits && GLOB.test(text), spread: splits && SPREADS.test(text) };
        }),
        Match.orElse(() => ({ text: sliced(part, place), known: false, glob: false, spread: false })),
    );

const pieces = (parts: readonly Node[], frame: Frame, place: Place, quoted: boolean): Piece =>
    parts.reduce<Piece>(
        (all, part) => {
            const piece = pieceOf(part, frame, place, quoted);
            return { text: `${all.text}${piece.text}`, known: all.known && piece.known, glob: all.glob || piece.glob, spread: all.spread || piece.spread };
        },
        { text: '', known: true, glob: false, spread: false },
    );

const worded = (node: Pick<Node, 'Pos' | 'End' | 'Parts'>, frame: Frame, place: Place): Word => {
    const parts = node.Parts ?? [];
    const raw = sliced(node, place);
    const piece = pieces(parts, frame, place, false);
    const text = Option.match(
        Option.filter(place.home, () => raw.startsWith('~')),
        { onNone: () => piece.text, onSome: (home) => piece.text.replace(TILDE, (_all, user: string) => (user === '' ? home : `${home.slice(0, home.lastIndexOf('/') + 1)}${user}`)) },
    );
    return { raw, ...spanOf(node), ...piece, text, known: piece.known && (!piece.spread || parts.length === 1) };
};

const fields = (word: Word): readonly Word[] => {
    const split = word.spread && word.known ? (word.text.match(FIELD) ?? []) : undefined;
    return split === undefined ? [word] : split.map((text) => ({ ...word, raw: text, text, glob: GLOB.test(text), spread: split.length > 1 }));
};

const assignedValue = ({ Value, Array: list, Append }: Assign, frame: Frame, place: Place): Option.Option<string> => {
    if (list !== undefined || Append === true) {
        return Option.none();
    }
    const word = Value === undefined ? { known: true, text: '' } : worded(Value, frame, place);
    return word.known ? Option.some(word.text) : Option.none();
};

const delivered = (redirects: readonly Node[], frame: Frame, place: Place): Option.Option<string> => {
    const redirect = redirects.findLast(({ Op }) => HEREDOC.has(Op));
    const quoted = redirect?.Hdoc !== undefined && redirect.Word?.Parts?.every(({ Type }) => Type === 'Lit') === false;
    return Option.map(Option.fromNullishOr(redirect?.Hdoc ?? redirect?.Word), (body) => (quoted ? (body.Parts ?? []).map(({ Value = '' }) => Value).join('') : pieces(body.Parts ?? [], frame, place, redirect?.Hdoc !== undefined).text));
};

// --- [FRAMES]

const merged = (scripts: readonly Script[]): Script => ({ commands: scripts.flatMap(({ commands }) => commands), jobs: scripts.flatMap(({ jobs }) => jobs) });

const joined = (left: Frame, right: Frame): Frame => ({
    values: new Map(Array.map([...left.values.keys(), ...right.values.keys()], (name) => [name, Equal.equals(left.values.get(name), right.values.get(name)) ? (left.values.get(name) ?? Option.none()) : Option.none<string>()] as const)),
    opaque: left.opaque || right.opaque,
    functions: left.functions.union(right.functions),
});

const bound = (frame: Frame, entries: readonly (readonly [string, Option.Option<string>])[]): Frame => ({ ...frame, values: new Map([...frame.values, ...entries]) });

const entered = (frame: Frame, [program, ...args]: readonly Word[], place: Place): Frame => {
    const target = args.find(({ text }) => text === '-' || !text.startsWith('-'));
    const named = Option.fromNullishOr(target).pipe(
        Option.filter(({ known, text }) => known && text !== '-' && program?.text !== 'popd'),
        Option.map(({ text }) => text),
    );
    const word = program?.text === 'cd' && target === undefined ? place.home : named;
    return bound(frame, [[DIRECTORY, Option.flatMap(word, (given) => place.resolve(given, looked(frame, DIRECTORY, place)))]]);
};

const effects = (frame: Frame, words: readonly Word[], place: Place): Frame => {
    const peeled = Array.dropWhile(words, ({ text }) => PREFIXES.has(text));
    const head = peeled[0]?.text;
    if (head !== undefined && ENTERING.has(head)) {
        return entered(frame, peeled, place);
    }
    if (head !== undefined && (OPAQUE.has(head) || frame.functions.has(head))) {
        return { ...frame, values: new Map(), opaque: true };
    }
    return head !== undefined && ASSIGNING.has(head)
        ? bound(
              frame,
              Array.filterMap(peeled.slice(1), ({ text }) => (NAME.test(text) ? Result.succeed([text, Option.none<string>()] as const) : Result.failVoid)),
          )
        : frame;
};

// --- [WALKS]

const chained = (frame: Frame, ...steps: readonly ((current: Frame) => Walked)[]): Walked => {
    const [final, scripts] = Array.mapAccum(steps, frame, (current, step) => step(current));
    return [final, merged(scripts)];
};

const nested = (value: unknown, frame: Frame, place: Place): Walked => {
    if (isNode(value) && (value.Type === 'CmdSubst' || value.Type === 'ProcSubst')) {
        const [inner, script] = nested(value.Stmts, frame, { ...place, fed: place.fed || value.Op === '>(', pipes: false, stage: Option.none(), substituted: place.substituted || value.Type === 'ProcSubst' });
        return [value.TempFile === true || value.ReplyVar === true ? joined(frame, inner) : frame, script];
    }
    if (isNode(value) && value.Cmd !== undefined) {
        return statement(value, frame, place);
    }
    return Predicate.isObjectOrArray(value) ? chained(frame, ...Object.values(value).map((child) => (current: Frame) => nested(child, current, place))) : [frame, NOTHING];
};

const statement = (stmt: Node, frame: Frame, place: Place): Walked => {
    const redirects = stmt.Redirs ?? [];
    const own = { ...place, fed: place.fed || redirects.some(({ Op, N }) => INPUT.has(Op) && (N === undefined || N.Value === '0')) };
    const [after, script] = chained(
        frame,
        (current) => nested(redirects, current, own),
        (current) => commanded(stmt, current, own),
    );
    return stmt.Background === true && stmt.Semicolon !== undefined ? [frame, merged([script, { ...NOTHING, jobs: [{ start: stmt.Semicolon.Offset, end: stmt.Semicolon.Offset + 1, origin: place.origin }] }])] : [after, script];
};

const commanded = (stmt: Node, frame: Frame, place: Place): Walked => {
    const inner = { ...place, pipes: false, stage: Option.none() };
    const node = stmt.Cmd;
    return node === undefined
        ? [frame, NOTHING]
        : Match.value(PIPING.has(node.Op) ? 'Pipeline' : node.Type).pipe(
              Match.when('CallExpr', () => called(node, stmt, frame, place)),
              Match.when('Pipeline', (): Walked => [frame, merged(stages(node).map((stage, at, all) => nested(stage, frame, { ...place, fed: place.fed || at > 0, pipes: at < all.length - 1, stage: Option.some({ pipeline: node, at }) })[1]))]),
              Match.when('BinaryCmd', (): Walked => {
                  const [flow, script] = flowed(node, frame, inner);
                  return [node.Op === '&&' ? flow : joined(frame, flow), script];
              }),
              Match.when('Subshell', (): Walked => [frame, nested(node.Stmts, frame, inner)[1]]),
              Match.when('IfClause', () => branched(node, frame, inner)),
              Match.when('WhileClause', () => looped([node.Cond, node.Do], frame, { ...inner, looping: true, busy: !place.fed && (node.Do ?? []).every(idle) }, Option.none())),
              Match.when('ForClause', () => looped([node.Loop, node.Do], frame, { ...inner, looping: true }, Option.fromNullishOr(node.Loop?.Name?.Value))),
              Match.when('CaseClause', (): Walked => {
                  const [tested, testing] = nested(node.Word, frame, inner);
                  const arms = (node.Items ?? []).map(({ Patterns, Stmts }) => nested([Patterns, Stmts], tested, inner));
                  return [arms.reduce((all, [arm]) => joined(all, arm), tested), merged([testing, ...arms.map(([, script]) => script)])];
              }),
              Match.when('FuncDecl', (): Walked => {
                  const functions = frame.functions.union(new Set(Option.toArray(Option.fromNullishOr(node.Name?.Value))));
                  return [{ ...frame, functions }, nested(node.Body, { values: new Map(), opaque: true, functions }, { ...inner, fed: true })[1]];
              }),
              Match.when('DeclClause', () => exported(node.Args ?? [], frame, inner)),
              Match.orElse(() => nested(node, frame, inner)),
          );
};

const idle = ({ Cmd: cmd }: Node): boolean => cmd?.Type === 'CallExpr' && cmd.Args?.[0]?.Parts?.every((part) => part.Type === 'Lit' && IDLE.has(part.Value)) === true;

const looped = (body: unknown, frame: Frame, place: Place, variable: Option.Option<string>): Walked => {
    const entry = bound(frame, Option.toArray(Option.map(variable, (name) => [name, Option.none<string>()] as const)));
    const widened = joined(entry, nested(body, entry, { ...place, dry: true })[0]);
    return [widened, place.dry ? NOTHING : nested(body, widened, place)[1]];
};

const branched = ({ Cond, Then, Else }: Node, frame: Frame, place: Place): Walked => {
    const [tested, testing] = nested(Cond, frame, place);
    const [taken, taking] = nested(Then, tested, place);
    const [other, othering] = Else === undefined ? [tested, NOTHING] : branched(Else, tested, place);
    return [joined(taken, other), merged([testing, taking, othering])];
};

const assigning = (args: readonly Assign[], frame: Frame, place: Place, dynamic: boolean): Frame =>
    bound(
        frame,
        Array.filterMap(args, (assign) => (assign.Name?.Value === undefined || assign.Naked === true ? Result.failVoid : Result.succeed([assign.Name.Value, dynamic ? Option.none<string>() : assignedValue(assign, frame, place)] as const))),
    );

const exported = (args: readonly Assign[], frame: Frame, place: Place): Walked => {
    const [ready, early] = nested(args, frame, place);
    const dynamic = args.some(({ Naked, Value }) => Naked === true && Value?.Parts?.some((part) => part.Type === 'Lit' && part.Value?.startsWith('-') === true) === true);
    return [assigning(args, ready, place, dynamic), early];
};

const flowed = ({ X, Y }: Node, frame: Frame, place: Place): Walked =>
    chained(
        frame,
        (current) => (X?.Cmd?.Type === 'BinaryCmd' && LISTING.has(X.Cmd.Op) ? flowed(X.Cmd, current, place) : nested(X, current, place)),
        (current) => nested(Y, current, place),
    );

const stages = ({ X, Y, Op }: Node): readonly unknown[] => [...(X?.Cmd?.Type === 'BinaryCmd' && X.Cmd.Op === Op ? stages(X.Cmd) : [X]), Y];

const called = ({ Assigns = [], Args = [] }: Node, stmt: Node, frame: Frame, place: Place): Walked => {
    const [ready, early] = nested([Assigns, Args], frame, place);
    const words = Args.flatMap((node) => fields(worded(node, ready, place)));
    if (words.length === 0) {
        return [assigning(Assigns, ready, place, false), early];
    }
    const redirects = stmt.Redirs ?? [];
    const prefix = Assigns.map((assign): Word => {
        const value = assignedValue(assign, ready, place);
        return { raw: sliced(assign, place), text: `${assign.Name?.Value}=${Option.getOrElse(value, () => '')}`, ...spanOf(assign), known: Option.isSome(value), glob: false, spread: false };
    });
    const command: Command = {
        start: stmt.Pos.Offset,
        end: stmt.Semicolon?.Offset ?? stmt.End.Offset,
        ...Struct.omit(place, ['read', 'resolve', 'home', 'dry']),
        words: [...prefix, ...words],
        redirects: redirects.map(({ Op = '', N, Word: target, Pos, End }) => {
            const span = { Pos, End: target?.End ?? End };
            return { raw: sliced(span, place), op: Op, descriptor: Option.fromNullishOr(N?.Value), target: Option.map(Option.fromNullishOr(target), (node) => worded(node, ready, place)), ...spanOf(span) };
        }),
        cwd: looked(ready, DIRECTORY, place),
        bound: ready.values,
        input: delivered(redirects, ready, place),
    };
    return [effects(ready, words, place), merged([early, { ...NOTHING, commands: [command] }])];
};

const scripted = Effect.fnUntraced(function* (stmts: readonly Node[], text: string, { cwd, visited: _visited, ...beginning }: Beginning) {
    const host = yield* Host;
    const names = [...new Set(text.match(REFERENCE))];
    const environment = new Map(Array.zip(names, yield* Effect.forEach(names, host.environment)));
    return nested(
        stmts,
        { values: new Map([[DIRECTORY, cwd]]), opaque: false, functions: new Set() },
        { fed: false, looping: false, pipes: false, stage: Option.none(), busy: false, substituted: false, ...beginning, source: new TextEncoder().encode(text), read: (name) => environment.get(name) ?? Option.none(), resolve: host.place, home: host.home, dry: false },
    )[1];
});

const tree = Effect.fnUntraced(function* (source: string, dialect: 'bash' | 'zsh') {
    const host = yield* Host;
    const { code, stdout, stderr } = yield* host.run(['shfmt', '--to-json', '-ln', dialect], { stdin: source });
    const { Stmts = [] } = code === 0 ? yield* Effect.mapError(Schema.decodeEffect(Tree)(stdout), HookError.of('shfmt')) : yield* HookError.of('shfmt')(stderr);
    return Stmts;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Beginning, type Command, merged, type Script, type Span, scripted, tree, type Word };
