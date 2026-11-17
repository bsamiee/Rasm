import { Array, Effect, Equal, Equivalence, Match, Option, Predicate, Result, Struct } from 'effect';
import { type Assign, type BaseNode, BinCmdOperator, type BinaryCmd, type CallExpr, type Command as CommandNode, type IfClause, LangVariant, type Node, ParExpOperator, type ParamExp, ProcOperator, parse, RedirOperator, type Redirect, type Stmt, type WordPart, type Word as WordNode } from 'sh-syntax/sync';
import { HostError } from '#/host';

// --- [TYPES] ---------------------------------------------------------------------------

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
    readonly dialect: 'bash' | 'zsh';
    readonly fed: boolean;
    readonly pipes: boolean;
    readonly stage: Option.Option<{ readonly pipeline: object; readonly at: number }>;
    readonly looping: boolean;
    readonly busy: boolean;
    readonly substituted: boolean;
    readonly captured: boolean;
    readonly job: boolean;
    readonly dry: boolean;
    readonly home: Option.Option<string>;
    readonly environment: ReadonlyMap<string, Option.Option<string>>;
    readonly resolve: (word: string, cwd: Option.Option<string>) => Option.Option<string>;
}
interface Command extends Span, Omit<Context, 'dialect' | 'dry' | 'home' | 'environment' | 'resolve'> {
    readonly words: readonly Word[];
    readonly redirects: readonly (Span & { readonly raw: string; readonly op: RedirOperator; readonly descriptor: Option.Option<string>; readonly target: Option.Option<Word> })[];
    readonly cwd: Option.Option<string>;
    readonly variables: ReadonlyMap<string, Option.Option<string>>;
    readonly input: Option.Option<string>;
}
interface Script {
    readonly commands: readonly Command[];
    readonly jobs: readonly (Span & Pick<Command, 'origin'>)[];
    readonly declarations: readonly (Span & Pick<Command, 'origin' | 'source'> & { readonly assigns: readonly (Span & { readonly name: string })[] })[];
    readonly zshAssigned: readonly string[];
}
interface Scope extends Pick<Command, 'variables'> {
    readonly opaque: boolean;
    readonly wiped: boolean;
    readonly functions: ReadonlyMap<string, Option.Option<ReadonlySet<string>>>;
}
type Walk = readonly [Scope, Script];

// --- [CONSTANTS] -----------------------------------------------------------------------

const PWD = 'PWD';
const REFERENCE = /(?<=\$\{?)[A-Za-z_]\w*/gu;
const DECODER = new TextDecoder();
const ANSI_ESCAPE = /\\(?:(?<code>[abeEfnrtv\\'"?])|(?<octal>[0-7]{1,3})|x(?<hex>[\da-fA-F]{1,2})|[uU](?<wide>[\da-fA-F]{1,8}))/gu;
const ANSI_CODES = { a: '\u0007', b: '\b', e: '\u001B', E: '\u001B', f: '\f', n: '\n', r: '\r', t: '\t', v: '\v', '\\': '\\', "'": "'", '"': '"', '?': '?' } as const;
const LAST_CODE_POINT = 0x10_ff_ff;
const UNQUOTED_ESCAPE = /\\(?:\n|(?<code>[\s\S]))/gu;
const QUOTED_ESCAPE = /\\(?:\n|(?<code>["\\$`]))/gu;
const CHARACTER = /[\s\S]/gu;
const UNQUOTED_PIECE = new RegExp(`${UNQUOTED_ESCAPE.source}|${CHARACTER.source}`, 'gu');
const QUOTED_PIECE = new RegExp(`${QUOTED_ESCAPE.source}|${CHARACTER.source}`, 'gu');
const ANSI_PIECE = new RegExp(`${ANSI_ESCAPE.source}|${CHARACTER.source}`, 'gu');
const GLOB = /[*?[{]/u;
const SPREADS = /\s|^$/u;
const FIELD = /\S+/gu;
const TILDE = /^~(?<user>[^/]*)/u;
const NAME = /^[A-Za-z_]\w*$/u;
const INSTANT = /^(?:0+(?:\.0*)?|\.0+)[smhd]?$/u;
const DEFAULTS: ReadonlySet<ParExpOperator> = new Set([ParExpOperator.DefaultUnset, ParExpOperator.DefaultUnsetOrNull]);
const VARIANTS = { bash: LangVariant.LangBash, zsh: LangVariant.LangZsh } as const;
const INPUT: ReadonlySet<RedirOperator> = new Set([RedirOperator.RdrIn, RedirOperator.Hdoc, RedirOperator.DashHdoc, RedirOperator.WordHdoc, RedirOperator.DplIn]);
const HEREDOC: ReadonlySet<RedirOperator> = new Set([RedirOperator.Hdoc, RedirOperator.DashHdoc, RedirOperator.WordHdoc]);
const LISTING: ReadonlySet<BinCmdOperator> = new Set([BinCmdOperator.AndStmt, BinCmdOperator.OrStmt]);
const SHIFTING: ReadonlySet<string> = new Set(['OLDPWD', 'SHLVL', '_']);
const IDLE: ReadonlySet<string | undefined> = new Set([':', 'true']);
const OPAQUE: ReadonlySet<string> = new Set(['eval', 'source', '.']);
const PREFIXES: ReadonlySet<string> = new Set(['builtin', 'command']);
const ASSIGNING: ReadonlySet<string> = new Set(['read', 'readarray', 'mapfile', 'getopts', 'printf', 'unset']);
const ENTERING: ReadonlySet<string> = new Set(['cd', 'pushd', 'popd']);
const NOTHING: Script = { commands: [], jobs: [], declarations: [], zshAssigned: [] };

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [WORDS]

const sliced = ({ start, end }: BaseNode, { source }: Context): string => DECODER.decode(source.subarray(start, end));

const spanOf = ({ start, end }: BaseNode): Span => ({ start, end });

const isNode = (value: unknown): value is Node => Predicate.hasProperty(value, 'type');

const lookup = (scope: Scope, name: string, context: Context): Option.Option<string> => scope.variables.get(name) ?? (scope.opaque || SHIFTING.has(name) ? Option.none() : (context.environment.get(name) ?? Option.none()));

const ansiDecoded = (escape: string, code?: keyof typeof ANSI_CODES, octal?: string, hex?: string, wide?: string): string => {
    if (code !== undefined) {
        return ANSI_CODES[code];
    }
    const point = Number(octal === undefined ? `0x${hex ?? wide}` : `0o${octal}`);
    return point <= LAST_CODE_POINT ? String.fromCodePoint(point) : escape;
};

const plainName = ({ param, excl, length, width, isSet, flags, index, slice, repl, exp, nestedParam, subscripts, modifiers, names, split, globSubst, rcExpand }: ParamExp): Option.Option<string> =>
    Option.filter(Option.fromNullishOr(param?.value), () => [excl, length, width, isSet, flags, index, slice, repl, nestedParam, ...subscripts, ...modifiers].every((field) => field === false || field === null) && (exp === null || DEFAULTS.has(exp.op)) && names + split + globSubst + rcExpand === 0);

const literal = (word: WordNode): Option.Option<string> => (word.parts.every((part) => part.type === 'Lit') ? Option.some(word.parts.map((part) => part.value).join('')) : Option.none());

const piecesOf = (part: WordPart, scope: Scope, context: Context, quote?: readonly [string, string]): readonly Part[] =>
    Match.value(part).pipe(
        Match.withReturnType<readonly Part[]>(),
        Match.when({ type: 'Lit' }, ({ value }) =>
            [...value.matchAll(quote === undefined ? UNQUOTED_PIECE : QUOTED_PIECE)].map(([raw]) => ({
                raw,
                text: raw.replaceAll(quote === undefined ? UNQUOTED_ESCAPE : QUOTED_ESCAPE, '$<code>'),
                known: true,
                glob: quote === undefined && GLOB.test(raw) && !raw.startsWith('\\'),
                spread: false,
                ...(quote === undefined ? {} : { quote }),
            })),
        ),
        Match.when({ type: 'SglQuoted' }, ({ dollar, value }) => {
            const quoted = [dollar ? "$'" : "'", "'"] as const;
            const characters = [...value.matchAll(dollar ? ANSI_PIECE : CHARACTER)];
            return (characters.length === 0 ? [''] : characters.map(([raw]) => raw)).map((raw) => ({ raw, text: dollar ? raw.replaceAll(ANSI_ESCAPE, ansiDecoded) : raw, known: true, glob: false, spread: false, quote: quoted }));
        }),
        Match.when({ type: 'DblQuoted' }, ({ dollar, parts }) => {
            const quoted = [dollar ? '$"' : '"', '"'] as const;
            const held = parts.flatMap((child) => piecesOf(child, scope, context, quoted));
            return held.length === 0 ? [{ raw: '', text: '', known: true, glob: false, spread: false, quote: quoted }] : held;
        }),
        Match.when({ type: 'ParamExp' }, (expansion) => {
            const raw = sliced(expansion, context);
            const quoted = quote === undefined ? {} : { quote };
            return Option.match(plainName(expansion), {
                onNone: () => [{ raw, text: raw, known: false, glob: false, spread: false, ...quoted }],
                onSome: (name) => {
                    const value = lookup(scope, name, context);
                    const text = Option.getOrElse(value, () => raw);
                    const splits = quote === undefined && Option.isSome(value);
                    return [{ raw, text, known: Option.isSome(value), glob: splits && GLOB.test(text), spread: splits && SPREADS.test(text), parameter: { name, offset: 0 }, ...quoted }];
                },
            });
        }),
        Match.orElse((other) => [{ raw: sliced(other, context), text: sliced(other, context), known: false, glob: false, spread: false, ...(quote === undefined ? {} : { quote }) }]),
    );

const pieces = (parts: readonly Part[]): Pick<Word, 'text' | 'known' | 'glob' | 'spread'> =>
    parts.reduce<Pick<Word, 'text' | 'known' | 'glob' | 'spread'>>((all, part) => ({ text: `${all.text}${part.text}`, known: all.known && part.known, glob: all.glob || part.glob, spread: all.spread || part.spread }), { text: '', known: true, glob: false, spread: false });

const worded = (node: WordNode, scope: Scope, context: Context): Word => {
    const parts = node.parts.flatMap((part) => piecesOf(part, scope, context));
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
            const consumed = Array.fromIterable(before).length;
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
        const parameter = { name: splitting.parameter.name, offset: splitting.parameter.offset + Array.fromIterable(word.text.slice(0, match.index)).length, length: Array.fromIterable(text).length };
        const raw = `\${${parameter.name}:${parameter.offset}:${parameter.length}}`;
        return { ...splitting, raw, text, glob: GLOB.test(text), spread: false, parameter };
    });
    const words = parts.map(({ raw }) => raw);
    return parts.map((part, index) => ({ ...word, raw: part.raw, text: part.text, glob: part.glob, spread: matches.length > 1, parts: [part], field: { index, words } }));
};

const scalar = ({ array, index, append }: Assign): boolean => array === null && index === null && !append;

const assignedValue = (assign: Assign, scope: Scope, context: Context): Option.Option<string> => {
    const word = assign.value === null ? { known: true, text: '' } : worded(assign.value, scope, context);
    return scalar(assign) && word.known ? Option.some(word.text) : Option.none();
};

const delivered = (redirects: readonly Redirect[], scope: Scope, context: Context): Option.Option<string> =>
    Option.flatMap(Option.fromNullishOr(redirects.findLast(({ op }) => HEREDOC.has(op))), ({ hdoc, word }) =>
        Option.map(Option.fromNullishOr(hdoc ?? word), (body) => (hdoc !== null && word !== null && Option.isNone(literal(word)) ? sliced(body, context) : pieces(body.parts.flatMap((part) => piecesOf(part, scope, context, hdoc === null ? undefined : ['', '']))).text)),
    );

// --- [SCOPES]

const concat = (scripts: readonly Script[]): Script => ({ commands: scripts.flatMap(({ commands }) => commands), jobs: scripts.flatMap(({ jobs }) => jobs), declarations: scripts.flatMap(({ declarations }) => declarations), zshAssigned: scripts.flatMap(({ zshAssigned }) => zshAssigned) });

const declared = (assigns: readonly Assign[], stmt: Stmt, { origin, source }: Context): Script => ({
    ...NOTHING,
    declarations: [{ start: stmt.start, end: stmt.semicolon.line > 0 ? stmt.semicolon.offset : stmt.end, origin, source, assigns: Array.filterMap(assigns, ({ name, start, end }) => (name === null ? Result.failVoid : Result.succeed({ start, end, name: name.value }))) }],
});

const zshAssignments = (assigns: readonly Assign[], names: readonly string[], { dialect }: Context): Script => ({
    ...NOTHING,
    zshAssigned: dialect === 'zsh' ? [...Array.getSomes(assigns.filter(scalar).map(({ name }) => Option.fromNullishOr(name?.value))), ...names] : [],
});

const joined = (left: Scope, right: Scope): Scope => ({
    variables: new Map(Array.map([...left.variables.keys(), ...right.variables.keys()], (name) => [name, Equal.equals(left.variables.get(name), right.variables.get(name)) ? (left.variables.get(name) ?? Option.none()) : Option.none<string>()] as const)),
    opaque: left.opaque || right.opaque,
    wiped: left.wiped || right.wiped,
    functions: new Map([...left.functions, ...right.functions]),
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

const effects = (scope: Scope, peeled: readonly Word[], context: Context): Scope => {
    const head = peeled[0]?.text;
    if (head === undefined) {
        return scope;
    }
    if (ENTERING.has(head)) {
        return entered(scope, peeled, context);
    }
    const assigned = ASSIGNING.has(head)
        ? peeled
              .slice(1)
              .map(({ text }) => text)
              .filter((text) => NAME.test(text))
        : [];
    const forgets = OPAQUE.has(head) ? Option.none<ReadonlySet<string>>() : (scope.functions.get(head) ?? Option.some<ReadonlySet<string>>(new Set(assigned)));
    return Option.match(forgets, {
        onNone: () => ({ ...scope, variables: new Map(), opaque: true, wiped: true }),
        onSome: (names) =>
            bound(
                scope,
                [...names].map((name) => [name, Option.none<string>()] as const),
            ),
    });
};

// --- [SYNTAX]

const chained = (scope: Scope, ...steps: readonly ((current: Scope) => Walk)[]): Walk => {
    const [final, scripts] = Array.mapAccum(steps, scope, (current, step) => step(current));
    return [final, concat(scripts)];
};

const nested = (value: unknown, scope: Scope, context: Context): Walk => {
    if (isNode(value) && (value.type === 'CmdSubst' || value.type === 'ProcSubst')) {
        const outward = value.type === 'ProcSubst' && value.op === ProcOperator.CmdOut;
        const [inner, script] = nested(value.stmts, scope, { ...context, fed: context.fed || outward, pipes: false, stage: Option.none(), substituted: context.substituted || value.type === 'ProcSubst', captured: context.captured || !outward });
        return [value.type === 'CmdSubst' && (value.tempFile || value.replyVar) ? joined(scope, inner) : scope, script];
    }
    if (isNode(value) && value.type === 'Stmt') {
        return statement(value, scope, context);
    }
    return Predicate.isObjectOrArray(value) ? chained(scope, ...Object.values(value).map((child) => (current: Scope) => nested(child, current, context))) : [scope, NOTHING];
};

const statement = (stmt: Stmt, scope: Scope, context: Context): Walk => {
    const own = { ...context, fed: context.fed || stmt.redirs.some(({ op, n }) => INPUT.has(op) && (n === null || n.value === '0')), job: context.job || stmt.background };
    const [after, script] = chained(
        scope,
        (current) => nested(stmt.redirs, current, own),
        (current) => commanded(stmt, current, own),
    );
    return stmt.background ? [scope, concat([script, { ...NOTHING, jobs: [{ start: stmt.semicolon.offset, end: stmt.semicolon.offset + 1, origin: context.origin }] }])] : [after, script];
};

const commanded = (stmt: Stmt, scope: Scope, context: Context): Walk => {
    const inner = { ...context, pipes: false, stage: Option.none() };
    return stmt.cmd === null
        ? [scope, NOTHING]
        : Match.value<CommandNode>(stmt.cmd).pipe(
              Match.withReturnType<Walk>(),
              Match.when({ type: 'CallExpr' }, (call) => called(call, stmt, scope, context)),
              Match.when({ type: 'BinaryCmd', op: Match.is(BinCmdOperator.Pipe, BinCmdOperator.PipeAll) }, (pipeline) => [
                  scope,
                  concat(pipelineStages(pipeline).map((stage, at, all) => nested(stage, scope, { ...context, fed: context.fed || at > 0, pipes: at < all.length - 1, stage: Option.some({ pipeline, at }) })[1])),
              ]),
              Match.when({ type: 'BinaryCmd' }, (binary) => {
                  const [flow, script] = flowed(binary, scope, inner);
                  return [binary.op === BinCmdOperator.AndStmt ? flow : joined(scope, flow), script];
              }),
              Match.when({ type: 'Subshell' }, ({ stmts }) => [scope, nested(stmts, scope, inner)[1]]),
              Match.when({ type: 'IfClause' }, (clause) => branched(clause, scope, inner)),
              Match.when({ type: 'WhileClause' }, ({ cond, do: body }) => loop([cond, body], scope, { ...inner, looping: true, busy: !context.fed && body.every(idle) }, Option.none())),
              Match.when({ type: 'ForClause' }, ({ loop: head, do: body }) => loop([head, body], scope, { ...inner, looping: true }, Option.fromNullishOr(head?.type === 'WordIter' ? head.name?.value : undefined))),
              Match.when({ type: 'CaseClause' }, ({ word, items }) => {
                  const [tested, testing] = nested(word, scope, inner);
                  const arms = items.map(({ patterns, stmts }) => nested([patterns, stmts], tested, inner));
                  return [arms.reduce((all, [arm]) => joined(all, arm), tested), concat([testing, ...arms.map(([, script]) => script)])];
              }),
              Match.when({ type: 'FuncDecl' }, ({ name, body }) => {
                  const own = Option.toArray(Option.fromNullishOr(name?.value));
                  const [walked, script] = nested(body, { variables: new Map(), opaque: true, wiped: false, functions: new Map([...scope.functions, ...own.map((named) => [named, Option.none()] as const)]) }, { ...inner, fed: true });
                  const assigned = walked.wiped ? Option.none() : Option.some<ReadonlySet<string>>(new Set(walked.variables.keys()));
                  return [{ ...scope, functions: new Map([...scope.functions, ...own.map((named) => [named, assigned] as const)]) }, script];
              }),
              Match.when({ type: 'DeclClause' }, ({ args }) => exported(args, stmt, scope, inner)),
              Match.orElse((other) => nested(other, scope, inner)),
          );
};

const idle = ({ cmd }: Stmt): boolean => cmd?.type === 'CallExpr' && Option.exists(Option.all(cmd.args.map(literal)), ([head, ...operands]) => IDLE.has(head) || (head === 'sleep' && Array.isReadonlyArrayNonEmpty(operands) && operands.every((operand) => INSTANT.test(operand))));

const loop = (body: unknown, scope: Scope, context: Context, variable: Option.Option<string>): Walk => {
    const entry = bound(scope, Option.toArray(Option.map(variable, (name) => [name, Option.none<string>()] as const)));
    const widened = joined(entry, nested(body, entry, { ...context, dry: true })[0]);
    return [widened, context.dry ? NOTHING : concat([zshAssignments([], Option.toArray(variable), context), nested(body, widened, context)[1]])];
};

const branched = ({ cond, then, else: other }: IfClause, scope: Scope, context: Context): Walk => {
    const [tested, testing] = nested(cond, scope, context);
    const [taken, taking] = nested(then, tested, context);
    const [rest, resting] = other === null ? [tested, NOTHING] : branched(other, tested, context);
    return [joined(taken, rest), concat([testing, taking, resting])];
};

const assigning = (args: readonly Assign[], scope: Scope, context: Context, dynamic: boolean): Scope =>
    bound(
        scope,
        Array.filterMap(args, (assign) => (assign.name === null || assign.naked ? Result.failVoid : Result.succeed([assign.name.value, dynamic ? Option.none<string>() : assignedValue(assign, scope, context)] as const))),
    );

const exported = (args: readonly Assign[], stmt: Stmt, scope: Scope, context: Context): Walk => {
    const [ready, early] = nested(args, scope, context);
    const dynamic = args.some(({ naked, value }) => naked && value?.parts.some((part) => part.type === 'Lit' && part.value.startsWith('-')) === true);
    return [assigning(args, ready, context, dynamic), concat([early, declared(args, stmt, context), zshAssignments(dynamic ? [] : args, [], context)])];
};

const flowed = ({ x, y }: BinaryCmd, scope: Scope, context: Context): Walk =>
    chained(
        scope,
        (current) => (x?.cmd?.type === 'BinaryCmd' && LISTING.has(x.cmd.op) ? flowed(x.cmd, current, context) : nested(x, current, context)),
        (current) => nested(y, current, context),
    );

const pipelineStages = ({ x, y, op }: BinaryCmd): readonly (Stmt | null)[] => [...(x?.cmd?.type === 'BinaryCmd' && x.cmd.op === op ? pipelineStages(x.cmd) : [x]), y];

const called = ({ assigns, args }: CallExpr, stmt: Stmt, scope: Scope, context: Context): Walk => {
    const [ready, early] = nested([assigns, args], scope, context);
    const words = args.flatMap((node) => fields(worded(node, ready, context)));
    const peeled = Array.dropWhile(words, ({ text }) => PREFIXES.has(text));
    const assignments = zshAssignments(assigns, peeled[0]?.text === 'read' ? peeled.slice(1).map(({ text }) => text) : [], context);
    if (words.length === 0) {
        return [assigning(assigns, ready, context, false), concat([early, declared(assigns, stmt, context), assignments])];
    }
    const prefix = assigns.map((assign): Word => {
        const value = assignedValue(assign, ready, context);
        const name = assign.name?.value;
        const part: Part = { raw: sliced(assign, context), text: Option.match(value, { onNone: () => `${name}=`, onSome: (assigned) => `${name}=${assigned}` }), known: Option.isSome(value), glob: false, spread: false };
        return { ...part, ...spanOf(assign), parts: [part] };
    });
    const command: Command = {
        start: stmt.start,
        end: stmt.semicolon.line > 0 ? stmt.semicolon.offset : stmt.end,
        ...Struct.omit(context, ['dialect', 'dry', 'home', 'environment', 'resolve']),
        words: [...prefix, ...words],
        redirects: stmt.redirs.map(({ op, n, word: target, start, end }) => {
            const span = { start, end: target?.end ?? end };
            return { raw: DECODER.decode(context.source.subarray(span.start, span.end)), op, descriptor: Option.fromNullishOr(n?.value), target: Option.map(Option.fromNullishOr(target), (node) => worded(node, ready, context)), ...span };
        }),
        cwd: lookup(ready, PWD, context),
        variables: ready.variables,
        input: delivered(stmt.redirs, ready, context),
    };
    return [effects(ready, peeled, context), concat([early, assignments, { ...NOTHING, commands: [command] }])];
};

const referenced = (text: string): readonly string[] => [...new Set(text.match(REFERENCE))];

const parsed = (text: string, dialect: Context['dialect']) => Effect.try({ try: () => parse(text, { variant: VARIANTS[dialect] }).stmts, catch: HostError.of('sh-syntax') });

const scriptOf = (stmts: readonly Stmt[], text: string, { cwd, ...start }: Pick<Command, 'cwd'> & Pick<Context, 'origin' | 'dialect' | 'home' | 'environment' | 'resolve'> & Partial<Pick<Context, 'fed' | 'looping'>>): Script =>
    nested(stmts, { variables: new Map([[PWD, cwd]]), opaque: false, wiped: false, functions: new Map() }, { fed: false, looping: false, pipes: false, stage: Option.none(), busy: false, substituted: false, captured: false, job: false, dry: false, ...start, source: new TextEncoder().encode(text) })[1];

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Command, concat, parsed, referenced, type Script, type Span, scriptOf, slicedWord, type Word };
