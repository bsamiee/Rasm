import { Array, Equal, Equivalence, Match, Option, Predicate, Result, Schema, Struct } from 'effect';

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
    readonly prefix?: string;
    readonly field?: { readonly index: number; readonly words: readonly string[] };
}
interface Word extends Span {
    readonly raw: string;
    readonly text: string;
    readonly known: boolean;
    readonly glob: boolean;
    readonly spread: boolean;
    readonly parts: readonly Part[];
}
interface Parameter {
    readonly name: string;
    readonly offset: number;
    readonly length?: number;
}
type Part = Pick<Word, 'raw' | 'text' | 'known' | 'glob'> & { readonly quote?: readonly [string, string] } & ({ readonly spread: false; readonly parameter?: Parameter } | { readonly spread: true; readonly parameter: Parameter });
interface Context {
    readonly source: Uint8Array;
    readonly origin: 'call' | 'body' | 'file';
    readonly fed: boolean;
    readonly pipes: boolean;
    readonly stage: Option.Option<{ readonly pipeline: object; readonly at: number }>;
    readonly looping: boolean;
    readonly busy: boolean;
    readonly substituted: boolean;
    readonly captured: boolean;
    readonly dry: boolean;
    readonly home: Option.Option<string>;
    readonly environment: ReadonlyMap<string, Option.Option<string>>;
    readonly resolve: (word: string, cwd: Option.Option<string>) => Option.Option<string>;
}
interface Command extends Span, Omit<Context, 'dry' | 'home' | 'environment' | 'resolve'> {
    readonly words: readonly Word[];
    readonly redirects: readonly (Span & { readonly raw: string; readonly op: string; readonly descriptor: Option.Option<string>; readonly target: Option.Option<Word> })[];
    readonly cwd: Option.Option<string>;
    readonly variables: ReadonlyMap<string, Option.Option<string>>;
    readonly input: Option.Option<string>;
}
interface Script {
    readonly commands: readonly Command[];
    readonly jobs: readonly (Span & Pick<Command, 'origin'>)[];
}
interface Scope extends Pick<Command, 'variables'> {
    readonly opaque: boolean;
    readonly functions: ReadonlySet<string>;
}
type Walk = readonly [Scope, Script];

// --- [CONSTANTS] -----------------------------------------------------------------------

const PWD = 'PWD';
const REFERENCE = /(?<=\$\{?)[A-Za-z_]\w*/gu;
const DECODER = new TextDecoder();
const ANSI_ESCAPE = /\\(?:(?<code>[abeEfnrtv\\'"?])|(?<octal>[0-7]{1,3})|x(?<hex>[\da-fA-F]{1,2})|[uU](?<wide>[\da-fA-F]{1,8}))/gu;
const ANSI_CODES = { a: '\x07', b: '\b', e: '\x1B', E: '\x1B', f: '\f', n: '\n', r: '\r', t: '\t', v: '\v', '\\': '\\', "'": "'", '"': '"', '?': '?' } as const;
const UNQUOTED_ESCAPE = /\\(?:\n|(?<code>[\s\S]))/gu;
const QUOTED_ESCAPE = /\\(?:\n|(?<code>["\\$`]))/gu;
const CHARACTER = /[\s\S]/gu;
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

const sliced = ({ Pos, End }: Pick<Node, 'Pos' | 'End'>, { source }: Context): string => DECODER.decode(source.subarray(Pos.Offset, End.Offset));

const spanOf = ({ Pos, End }: Pick<Node, 'Pos' | 'End'>): Span => ({ start: Pos.Offset, end: End.Offset });

const lookup = (scope: Scope, name: string, context: Context): Option.Option<string> => scope.variables.get(name) ?? (scope.opaque || SHIFTING.has(name) ? Option.none() : (context.environment.get(name) ?? Option.none()));

const ansiDecoded = (_all: string, code?: keyof typeof ANSI_CODES, octal?: string, hex?: string, wide?: string): string => (code === undefined ? String.fromCodePoint(Number(octal === undefined ? `0x${hex ?? wide}` : `0o${octal}`)) : ANSI_CODES[code]);

const piecesOf = ({ Value = '', ...part }: Node, scope: Scope, context: Context, quote?: readonly [string, string]): readonly Part[] =>
    Match.value(part.Type).pipe(
        Match.when('Lit', (): readonly Part[] =>
            [...Value.matchAll(new RegExp(`${(quote === undefined ? UNQUOTED_ESCAPE : QUOTED_ESCAPE).source}|${CHARACTER.source}`, 'gu'))].map(([raw]) => ({
                raw,
                text: raw.replaceAll(quote === undefined ? UNQUOTED_ESCAPE : QUOTED_ESCAPE, '$<code>'),
                known: true,
                glob: quote === undefined && GLOB.test(raw) && !raw.startsWith('\\'),
                spread: false,
                ...(quote === undefined ? {} : { quote }),
            })),
        ),
        Match.when('SglQuoted', (): readonly Part[] => {
            const quoted = [part.Dollar === true ? "$'" : "'", "'"] as const;
            const characters = [...Value.matchAll(part.Dollar === true ? new RegExp(`${ANSI_ESCAPE.source}|${CHARACTER.source}`, 'gu') : CHARACTER)];
            return (characters.length === 0 ? [''] : characters.map(([raw]) => raw)).map((raw) => ({ raw, text: part.Dollar === true ? raw.replaceAll(ANSI_ESCAPE, ansiDecoded) : raw, known: true, glob: false, spread: false, quote: quoted }));
        }),
        Match.when('DblQuoted', (): readonly Part[] => {
            const quoted = [part.Dollar === true ? '$"' : '"', '"'] as const;
            const held = (part.Parts ?? []).flatMap((child) => piecesOf(child, scope, context, quoted));
            return held.length === 0 ? [{ raw: '', text: '', known: true, glob: false, spread: false, quote: quoted }] : held;
        }),
        Match.when('ParamExp', (): readonly Part[] => {
            const parameter = new Set(Object.keys(part)).isSubsetOf(PLAIN_EXPANSION) ? part.Param?.Value : undefined;
            const value = parameter === undefined ? Option.none() : lookup(scope, parameter, context);
            const raw = sliced(part, context);
            const text = Option.getOrElse(value, () => raw);
            const splits = quote === undefined && Option.isSome(value);
            return parameter === undefined
                ? [{ raw, text, known: false, glob: false, spread: false, ...(quote === undefined ? {} : { quote }) }]
                : [{ raw, text, known: Option.isSome(value), glob: splits && GLOB.test(text), spread: splits && SPREADS.test(text), parameter: { name: parameter, offset: 0 }, ...(quote === undefined ? {} : { quote }) }];
        }),
        Match.orElse((): readonly Part[] => [{ raw: sliced(part, context), text: sliced(part, context), known: false, glob: false, spread: false, ...(quote === undefined ? {} : { quote }) }]),
    );

const pieces = (parts: readonly Part[]): Pick<Word, 'text' | 'known' | 'glob' | 'spread'> =>
    parts.reduce<Pick<Word, 'text' | 'known' | 'glob' | 'spread'>>((all, part) => ({ text: `${all.text}${part.text}`, known: all.known && part.known, glob: all.glob || part.glob, spread: all.spread || part.spread }), { text: '', known: true, glob: false, spread: false });

const worded = (node: Pick<Node, 'Pos' | 'End' | 'Parts'>, scope: Scope, context: Context): Word => {
    const parts = (node.Parts ?? []).flatMap((part) => piecesOf(part, scope, context));
    const raw = sliced(node, context);
    const piece = pieces(parts);
    const text = Option.match(
        Option.filter(context.home, () => raw.startsWith('~')),
        { onNone: () => piece.text, onSome: (home) => piece.text.replace(TILDE, (_all, user: string) => (user === '' ? home : `${home.slice(0, home.lastIndexOf('/') + 1)}${user}`)) },
    );
    return { raw, ...spanOf(node), ...piece, parts, text, known: piece.known && (!piece.spread || parts.length === 1) };
};

const spelled = (parts: readonly Part[]): string => {
    const groups = Array.isReadonlyArrayNonEmpty(parts) ? Array.groupWith(parts, Equivalence.mapInput(Equivalence.strictEqual<Part['quote']>(), Struct.get('quote'))) : [];
    return groups.map(([first, ...rest]) => [first.quote?.[0], first.raw, ...rest.map(({ raw }) => raw), first.quote?.[1]].join('')).join('');
};

const slicedWord = (word: Word, at: number): Option.Option<Word> => {
    const [remaining, positions] = Array.mapAccum(word.parts, at, (left, part) => [Math.max(0, left - part.text.length), { part, left }]);
    if (remaining > 0 || positions.some(({ part, left }) => left > 0 && left < part.text.length && (!part.known || part.parameter === undefined))) {
        return Option.none();
    }
    const divided = positions.map(({ part, left }): readonly [readonly Part[], readonly Part[]] => {
        if (left > 0 && left < part.text.length && part.parameter !== undefined) {
            const before = part.text.slice(0, left);
            const after = part.text.slice(left);
            const { name, offset, length } = part.parameter;
            const consumed = [...before].length;
            const next = offset + consumed;
            return [
                [{ ...part, raw: `\${${name}:${offset}:${consumed}}`, text: before, glob: part.quote === undefined && GLOB.test(before), spread: part.quote === undefined && SPREADS.test(before), parameter: { name, offset, length: consumed } }],
                [
                    {
                        ...part,
                        raw: length === undefined ? `\${${name}:${next}}` : `\${${name}:${next}:${length - consumed}}`,
                        text: after,
                        glob: part.quote === undefined && GLOB.test(after),
                        spread: part.quote === undefined && SPREADS.test(after),
                        parameter: { name, offset: next, ...(length === undefined ? {} : { length: length - consumed }) },
                    },
                ],
            ];
        }
        return left > 0 ? [[part], []] : [[], [part]];
    });
    const parts = divided.flatMap(([, after]) => after);
    const piece = pieces(parts);
    return Option.some({ ...word, ...piece, parts, prefix: [word.prefix, spelled(divided.flatMap(([before]) => before))].join(''), raw: spelled(parts) || "''", known: piece.known && (!piece.spread || word.known) });
};

const fields = (word: Word): readonly Word[] => {
    const splitting = word.known ? word.parts.find((part) => part.spread) : undefined;
    if (splitting === undefined) {
        return [word];
    }
    const matches = [...word.text.matchAll(FIELD)];
    const parts = matches.map((match): Part => {
        const [text] = match;
        const parameter = { name: splitting.parameter.name, offset: splitting.parameter.offset + [...word.text.slice(0, match.index)].length, length: [...text].length };
        const raw = `\${${parameter.name}:${parameter.offset}:${parameter.length}}`;
        return { ...splitting, raw, text, glob: GLOB.test(text), spread: false, parameter };
    });
    const words = parts.map(({ raw }) => raw);
    return parts.map((part, index) => ({ ...word, raw: part.raw, text: part.text, glob: part.glob, spread: matches.length > 1, parts: [part], field: { index, words } }));
};

const assignedValue = ({ Value, Array: list, Append }: Assign, scope: Scope, context: Context): Option.Option<string> => {
    if (list !== undefined || Append === true) {
        return Option.none();
    }
    const word = Value === undefined ? { known: true, text: '' } : worded(Value, scope, context);
    return word.known ? Option.some(word.text) : Option.none();
};

const delivered = (redirects: readonly Node[], scope: Scope, context: Context): Option.Option<string> => {
    const redirect = redirects.findLast(({ Op }) => HEREDOC.has(Op));
    const quoted = redirect?.Hdoc !== undefined && redirect.Word?.Parts?.every(({ Type }) => Type === 'Lit') === false;
    return Option.map(Option.fromNullishOr(redirect?.Hdoc ?? redirect?.Word), (body) => (quoted ? (body.Parts ?? []).map(({ Value = '' }) => Value).join('') : pieces((body.Parts ?? []).flatMap((part) => piecesOf(part, scope, context, redirect?.Hdoc === undefined ? undefined : ['', '']))).text));
};

// --- [SCOPES]

const concat = (scripts: readonly Script[]): Script => ({ commands: scripts.flatMap(({ commands }) => commands), jobs: scripts.flatMap(({ jobs }) => jobs) });

const joined = (left: Scope, right: Scope): Scope => ({
    variables: new Map(Array.map([...left.variables.keys(), ...right.variables.keys()], (name) => [name, Equal.equals(left.variables.get(name), right.variables.get(name)) ? (left.variables.get(name) ?? Option.none()) : Option.none<string>()] as const)),
    opaque: left.opaque || right.opaque,
    functions: left.functions.union(right.functions),
});

const bound = (scope: Scope, entries: readonly (readonly [string, Option.Option<string>])[]): Scope => ({ ...scope, variables: new Map([...scope.variables, ...entries]) });

const entered = (scope: Scope, [program, ...args]: readonly Word[], context: Context): Scope => {
    const target = args.find(({ text }) => text === '-' || !text.startsWith('-'));
    const named = Option.fromNullishOr(target).pipe(
        Option.filter(({ known, text }) => known && text !== '-' && program?.text !== 'popd'),
        Option.map(({ text }) => text),
    );
    const word = program?.text === 'cd' && target === undefined ? context.home : named;
    return bound(scope, [[PWD, Option.flatMap(word, (given) => context.resolve(given, lookup(scope, PWD, context)))]]);
};

const effects = (scope: Scope, words: readonly Word[], context: Context): Scope => {
    const peeled = Array.dropWhile(words, ({ text }) => PREFIXES.has(text));
    const head = peeled[0]?.text;
    if (head !== undefined && ENTERING.has(head)) {
        return entered(scope, peeled, context);
    }
    if (head !== undefined && (OPAQUE.has(head) || scope.functions.has(head))) {
        return { ...scope, variables: new Map(), opaque: true };
    }
    return head !== undefined && ASSIGNING.has(head)
        ? bound(
              scope,
              Array.filterMap(peeled.slice(1), ({ text }) => (NAME.test(text) ? Result.succeed([text, Option.none<string>()] as const) : Result.failVoid)),
          )
        : scope;
};

// --- [SYNTAX]

const chained = (scope: Scope, ...steps: readonly ((current: Scope) => Walk)[]): Walk => {
    const [final, scripts] = Array.mapAccum(steps, scope, (current, step) => step(current));
    return [final, concat(scripts)];
};

const nested = (value: unknown, scope: Scope, context: Context): Walk => {
    if (isNode(value) && (value.Type === 'CmdSubst' || value.Type === 'ProcSubst')) {
        const [inner, script] = nested(value.Stmts, scope, { ...context, fed: context.fed || value.Op === '>(', pipes: false, stage: Option.none(), substituted: context.substituted || value.Type === 'ProcSubst', captured: context.captured || value.Op !== '>(' });
        return [value.TempFile === true || value.ReplyVar === true ? joined(scope, inner) : scope, script];
    }
    if (isNode(value) && value.Cmd !== undefined) {
        return statement(value, scope, context);
    }
    return Predicate.isObjectOrArray(value) ? chained(scope, ...Object.values(value).map((child) => (current: Scope) => nested(child, current, context))) : [scope, NOTHING];
};

const statement = (stmt: Node, scope: Scope, context: Context): Walk => {
    const redirects = stmt.Redirs ?? [];
    const own = { ...context, fed: context.fed || redirects.some(({ Op, N }) => INPUT.has(Op) && (N === undefined || N.Value === '0')) };
    const [after, script] = chained(
        scope,
        (current) => nested(redirects, current, own),
        (current) => commanded(stmt, current, own),
    );
    return stmt.Background === true && stmt.Semicolon !== undefined ? [scope, concat([script, { ...NOTHING, jobs: [{ start: stmt.Semicolon.Offset, end: stmt.Semicolon.Offset + 1, origin: context.origin }] }])] : [after, script];
};

const commanded = (stmt: Node, scope: Scope, context: Context): Walk => {
    const inner = { ...context, pipes: false, stage: Option.none() };
    const node = stmt.Cmd;
    return node === undefined
        ? [scope, NOTHING]
        : Match.value(PIPING.has(node.Op) ? 'Pipeline' : node.Type).pipe(
              Match.when('CallExpr', () => called(node, stmt, scope, context)),
              Match.when('Pipeline', (): Walk => [scope, concat(pipelineStages(node).map((stage, at, all) => nested(stage, scope, { ...context, fed: context.fed || at > 0, pipes: at < all.length - 1, stage: Option.some({ pipeline: node, at }) })[1]))]),
              Match.when('BinaryCmd', (): Walk => {
                  const [flow, script] = flowed(node, scope, inner);
                  return [node.Op === '&&' ? flow : joined(scope, flow), script];
              }),
              Match.when('Subshell', (): Walk => [scope, nested(node.Stmts, scope, inner)[1]]),
              Match.when('IfClause', () => branched(node, scope, inner)),
              Match.when('WhileClause', () => loop([node.Cond, node.Do], scope, { ...inner, looping: true, busy: !context.fed && (node.Do ?? []).every(idle) }, Option.none())),
              Match.when('ForClause', () => loop([node.Loop, node.Do], scope, { ...inner, looping: true }, Option.fromNullishOr(node.Loop?.Name?.Value))),
              Match.when('CaseClause', (): Walk => {
                  const [tested, testing] = nested(node.Word, scope, inner);
                  const arms = (node.Items ?? []).map(({ Patterns, Stmts }) => nested([Patterns, Stmts], tested, inner));
                  return [arms.reduce((all, [arm]) => joined(all, arm), tested), concat([testing, ...arms.map(([, script]) => script)])];
              }),
              Match.when('FuncDecl', (): Walk => {
                  const functions = scope.functions.union(new Set(Option.toArray(Option.fromNullishOr(node.Name?.Value))));
                  return [{ ...scope, functions }, nested(node.Body, { variables: new Map(), opaque: true, functions }, { ...inner, fed: true })[1]];
              }),
              Match.when('DeclClause', () => exported(node.Args ?? [], scope, inner)),
              Match.orElse(() => nested(node, scope, inner)),
          );
};

const idle = ({ Cmd: cmd }: Node): boolean => cmd?.Type === 'CallExpr' && cmd.Args?.[0]?.Parts?.every((part) => part.Type === 'Lit' && IDLE.has(part.Value)) === true;

const loop = (body: unknown, scope: Scope, context: Context, variable: Option.Option<string>): Walk => {
    const entry = bound(scope, Option.toArray(Option.map(variable, (name) => [name, Option.none<string>()] as const)));
    const widened = joined(entry, nested(body, entry, { ...context, dry: true })[0]);
    return [widened, context.dry ? NOTHING : nested(body, widened, context)[1]];
};

const branched = ({ Cond, Then, Else }: Node, scope: Scope, context: Context): Walk => {
    const [tested, testing] = nested(Cond, scope, context);
    const [taken, taking] = nested(Then, tested, context);
    const [other, othering] = Else === undefined ? [tested, NOTHING] : branched(Else, tested, context);
    return [joined(taken, other), concat([testing, taking, othering])];
};

const assigning = (args: readonly Assign[], scope: Scope, context: Context, dynamic: boolean): Scope =>
    bound(
        scope,
        Array.filterMap(args, (assign) => (assign.Name?.Value === undefined || assign.Naked === true ? Result.failVoid : Result.succeed([assign.Name.Value, dynamic ? Option.none<string>() : assignedValue(assign, scope, context)] as const))),
    );

const exported = (args: readonly Assign[], scope: Scope, context: Context): Walk => {
    const [ready, early] = nested(args, scope, context);
    const dynamic = args.some(({ Naked, Value }) => Naked === true && Value?.Parts?.some((part) => part.Type === 'Lit' && part.Value?.startsWith('-') === true) === true);
    return [assigning(args, ready, context, dynamic), early];
};

const flowed = ({ X, Y }: Node, scope: Scope, context: Context): Walk =>
    chained(
        scope,
        (current) => (X?.Cmd?.Type === 'BinaryCmd' && LISTING.has(X.Cmd.Op) ? flowed(X.Cmd, current, context) : nested(X, current, context)),
        (current) => nested(Y, current, context),
    );

const pipelineStages = ({ X, Y, Op }: Node): readonly unknown[] => [...(X?.Cmd?.Type === 'BinaryCmd' && X.Cmd.Op === Op ? pipelineStages(X.Cmd) : [X]), Y];

const called = ({ Assigns = [], Args = [] }: Node, stmt: Node, scope: Scope, context: Context): Walk => {
    const [ready, early] = nested([Assigns, Args], scope, context);
    const words = Args.flatMap((node) => fields(worded(node, ready, context)));
    if (words.length === 0) {
        return [assigning(Assigns, ready, context, false), early];
    }
    const redirects = stmt.Redirs ?? [];
    const prefix = Assigns.map((assign): Word => {
        const value = assignedValue(assign, ready, context);
        const part: Part = { raw: sliced(assign, context), text: Option.match(value, { onNone: () => `${assign.Name?.Value}=`, onSome: (assigned) => `${assign.Name?.Value}=${assigned}` }), known: Option.isSome(value), glob: false, spread: false };
        return { ...part, ...spanOf(assign), parts: [part] };
    });
    const command: Command = {
        start: stmt.Pos.Offset,
        end: stmt.Semicolon?.Offset ?? stmt.End.Offset,
        ...Struct.omit(context, ['dry', 'home', 'environment', 'resolve']),
        words: [...prefix, ...words],
        redirects: redirects.map(({ Op = '', N, Word: target, Pos, End }) => {
            const span = { Pos, End: target?.End ?? End };
            return { raw: sliced(span, context), op: Op, descriptor: Option.fromNullishOr(N?.Value), target: Option.map(Option.fromNullishOr(target), (node) => worded(node, ready, context)), ...spanOf(span) };
        }),
        cwd: lookup(ready, PWD, context),
        variables: ready.variables,
        input: delivered(redirects, ready, context),
    };
    return [effects(ready, words, context), concat([early, { ...NOTHING, commands: [command] }])];
};

const referenced = (text: string): readonly string[] => [...new Set(text.match(REFERENCE))];

const scriptOf = (stmts: readonly Node[], text: string, { cwd, ...start }: Pick<Command, 'cwd'> & Pick<Context, 'origin' | 'home' | 'environment' | 'resolve'> & Partial<Pick<Context, 'fed' | 'looping'>>): Script =>
    nested(stmts, { variables: new Map([[PWD, cwd]]), opaque: false, functions: new Set() }, { fed: false, looping: false, pipes: false, stage: Option.none(), busy: false, substituted: false, captured: false, dry: false, ...start, source: new TextEncoder().encode(text) })[1];

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Command, concat, referenced, type Script, type Span, scriptOf, slicedWord, Tree, type Word };
