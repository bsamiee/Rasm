import { Array, Data, Glob, HashMap, HashSet, Match, Option, pipe, Predicate, Record, Redacted } from 'effect';
import { type ArithmExpr, type BraceExp, LangVariant, type Lit, type ParamExp, ParExpOperator, ParNamesOperator, type Replace, type SglQuoted, type Slice, type Word, type WordPart } from 'sh-syntax';
import { evaluate } from '#/evaluation/arithmetic';
import { Content, Value } from '#/evaluation/parameter';
import { Binding, binding, Expanded, type Expansion, Field, Matching, Origin, Piece, Refused, Role, Scope, Unmatched } from '#/evaluation/value';
import type { Dialect } from '#/syntax/dialect';

// --- [TYPES] ---------------------------------------------------------------------------

type Chunk = Data.TaggedEnum<{
    Text: { readonly text: string; readonly quoted: boolean; readonly split: boolean; readonly glob: boolean; readonly secret: boolean };
    Unknown: { readonly origin: Origin; readonly certain: boolean; readonly many: boolean };
    Break: object;
    Refusal: Value['refusals'][number];
}>;
type Segment = Extract<Chunk, { readonly _tag: 'Text' | 'Unknown' }>;
type Evaluated = readonly [Scope, readonly Chunk[]];
type Operator = (exp: ParamExp, held: Held, frame: Frame, plain: boolean) => Held;
type Splitting = typeof unsplit;
interface Site {
    readonly braces: boolean;
    readonly split: boolean;
    readonly single: boolean;
    readonly sole: boolean;
    readonly tilde: boolean;
    readonly colons: boolean;
    readonly named: boolean;
    readonly heredoc: Option.Option<boolean>;
    readonly glob: Option.Option<ReturnType<typeof globbing>>;
}
interface Frame {
    readonly dialect: Dialect;
    readonly zsh: boolean;
    readonly site: Site;
    readonly patterns: Site;
    readonly quoted: boolean;
    readonly result: boolean;
    readonly secret: (part: WordPart) => boolean;
}
interface Held {
    readonly value: Value;
    readonly scope: Scope;
    readonly spliced: Option.Option<readonly Chunk[]>;
}
interface Draft {
    readonly segments: readonly Segment[];
    readonly certain: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const Chunk = Data.taggedEnum<Chunk>();
const limit = 10_000;
const spaces = ' \t\n';
const blank: Draft = { segments: [], certain: false };
const unsplit = { fields: Array.empty<Draft>(), current: blank, started: false, spaced: false, exact: true };
const integral = /^[-+]?\d+$/u;
const assignment = /^[A-Za-z_]\w*=/u;
const spreads: Readonly<Record<string, boolean>> = { '@': true, '*': false };
const escapes: Readonly<Record<string, string>> = { a: '\u0007', b: '\b', e: '\u001B', E: '\u001B', f: '\f', n: '\n', r: '\r', t: '\t', v: '\v', '\\': '\\', "'": "'", '"': '"', '?': '?' };
const byte = (value: number) => `%${value.toString(16).padStart(2, '0')}`;
const control = (character: string) => byte(character === '?' ? 0x7f : (character.codePointAt(0) ?? 0) & 0x1f);
const ansi: Readonly<Record<string, (held: string, zsh: boolean, dialect: Dialect) => Option.Option<string>>> = {
    plain: (held) => Option.some(encodeURIComponent(held)),
    caret: (held, zsh) => Option.some(zsh ? encodeURIComponent(`c${held}`) : control(held)),
    chord: (held, zsh) => Option.some(zsh ? control(held) : encodeURIComponent(`\\C-${held}`)),
    meta: (held, zsh) => Option.some(zsh ? byte(0x80 + ((held.codePointAt(0) ?? 0) & 0x7f)) : encodeURIComponent(`\\M-${held}`)),
    hex: (held) => Option.some(byte(Number.parseInt(held, 16))),
    octal: (held) => Option.some(byte(Number.parseInt(held, 8) & 0xff)),
    point: (held, zsh, dialect) => {
        const digits = held.slice(1, held.startsWith('u') ? 5 : 9);
        const code = Number.parseInt(digits, 16);
        const rest = encodeURIComponent(held.slice(1 + digits.length));
        const [letter, width] = code < 0x1_00_00 ? ['u', 4] : ['U', 8];
        const encoded = Option.getOrElse(Option.liftThrowable(() => encodeURIComponent(String.fromCodePoint(code)))(), () => '%');
        const escaped = encodeURIComponent(`\\${letter}${code.toString(16).toUpperCase().padStart(width, '0')}`);
        return Match.value({ known: dialect.supports('unicode'), converts: code <= 0x7f || dialect.supports('wide'), narrow: dialect.supports('narrow') }).pipe(
            Match.when({ known: false }, () => Option.some(encodeURIComponent(`\\${held}`))),
            Match.when({ converts: true }, () => Option.some(`${encoded}${rest}`)),
            Match.when({ narrow: true }, () => Option.liftPredicate(`${escaped}${rest}`, () => !zsh)),
            Match.orElse(() => Option.some('%')),
        );
    },
    other: (held, zsh) => Option.some(encodeURIComponent(escapes[held] ?? (zsh ? held : `\\${held}`))),
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const globbing = ({ options, variant }: Dialect, paths: boolean) => {
    const zsh = variant === LangVariant.LangZsh;
    const magic = zsh ? /[*?(]|\[.+\]/su : /[*?]|\[.+\]/su;
    const beyond = options.extendedglob ? /[()|^#~]|\*{3}/u : /[()|]|\*{3}/u;
    const stars = zsh ? /\\[\s\S]|\*{2,}(?!\/)/gu : /\\[\s\S]|\*{2,}/gu;
    const policies = [
        [options.nomatch, Unmatched.Refused()],
        [options.nullglob, Unmatched.Dropped()],
    ] as const;
    const unmatched = Option.match(
        Array.findFirst(zsh ? policies.toReversed() : policies, ([set]) => set),
        { onNone: () => Unmatched.Kept(), onSome: ([, policy]) => policy },
    );
    const fold = (pattern: string) => (zsh || !options.globstar ? pattern.replaceAll(stars, (found) => (found.startsWith('\\') ? found : '*')) : pattern);
    return (active: string) => {
        const modelled = !(options.nocaseglob || (zsh && beyond.test(active)) || (paths && !options.globskipdots && /(?:^|\/)\./u.test(active)));
        const read = (pattern: Option.Option<string>) => Option.filter(pattern, () => modelled);
        const match = (pattern: Option.Option<string>) => (paths ? Matching.Paths({ glob: Option.map(read(pattern), (text) => Glob.make(fold(text), { dot: options.dotglob })), unmatched }) : Matching.Strings({ glob: Option.map(read(pattern), Field.strings) }));
        return Option.liftPredicate(match, () => (!paths || options.glob) && magic.test(active));
    };
};
const place = (role: Role, dialect: Dialect): Site => {
    const { options, variant } = dialect;
    const single: Site = { braces: false, split: false, single: true, sole: false, tilde: true, colons: false, named: false, heredoc: Option.none(), glob: Option.none() };
    return Role.$match(role, {
        Argument: (): Site => ({ ...single, braces: options.braceexpand, split: true, single: false, named: variant === LangVariant.LangBash && !options.posix, glob: Option.some(globbing(dialect, true)) }),
        Redirect: (): Site => ({ ...single, braces: options.braceexpand, split: !options.posix, single: false, sole: !(variant === LangVariant.LangZsh && options.multios), glob: options.posix ? Option.none() : Option.some(globbing(dialect, true)) }),
        Assignment: () => ({ ...single, colons: true }),
        Pattern: (): Site => ({ ...single, glob: Option.some(globbing(dialect, false)) }),
        Operand: () => single,
        Heredoc: ({ expands }) => ({ ...single, tilde: false, heredoc: Option.some(expands) }),
    });
};
const unescape = (escapable: RegExp) => (text: string) => text.replaceAll(escapable, (_, held: string) => (held === '\n' ? '' : held));
const quote = (text: string, secret = false) => Chunk.Text({ text, quoted: true, split: false, glob: false, secret });
const splits = ({ site, dialect }: Frame, exp?: ParamExp) => site.split && (exp?.split === 1 || (dialect.options.wordsplit && exp?.split !== 2));
const emit = (text: string, frame: Frame, exp?: ParamExp) => {
    const globs = Option.isSome(frame.site.glob) && (exp?.globSubst === 1 || (frame.dialect.options.globsubst && exp?.globSubst !== 2));
    return Chunk.Text({ text, quoted: frame.quoted, split: !frame.quoted && splits(frame, exp), glob: !frame.quoted && globs, secret: false });
};
const obscure = (origin: Origin, frame: Frame, plural = false, exp?: ParamExp) => {
    const one = frame.quoted || frame.site.single;
    return Chunk.Unknown({ origin, certain: one && !plural, many: !frame.site.single && (plural || (!frame.quoted && splits(frame, exp))) });
};
const spell = (chunks: readonly Chunk[], write = ({ text }: Extract<Chunk, { readonly _tag: 'Text' }>) => text): Option.Option<string> =>
    Option.map(Option.all(chunks.map(Chunk.$match({ Text: (held) => Option.some(write(held)), Break: () => Option.some(' '), Unknown: () => Option.none(), Refusal: () => Option.some('') }))), Array.join(''));
const separators = (scope: Scope) => Option.orElse(scope.text('IFS'), () => Option.liftPredicate(spaces, () => scope.get('IFS')._tag === 'Unset'));

// --- [BRACES]

const enumerate = (from: number, to: number, by: number, zsh: boolean): Option.Option<readonly number[]> => {
    const stride = Math.abs(by) || 1;
    const count = Math.floor(Math.abs(to - from) / stride) + 1;
    const values = Array.makeBy(Math.min(count, limit), (index) => from + Math.sign(to - from || 1) * index * stride);
    return Option.liftPredicate(zsh && by < 0 ? values.toReversed() : values, () => count <= limit && [from, to, by].every(Number.isSafeInteger));
};
const sequence = ({ elems }: BraceExp, dialect: Dialect): Option.Option<readonly string[]> =>
    Option.flatMap(Option.all(elems.map(({ parts }) => (parts.length === 1 && parts[0]?.type === 'Lit' ? Option.some(parts[0].value) : Option.none()))), ([start = '', end = '', step]) => {
        const zsh = dialect.variant === LangVariant.LangZsh;
        const width = dialect.supports('padding') && [start, end].some((text) => /^[-+]?0\d/u.test(text)) ? Math.max(start.length, end.length) : 0;
        const kinds = [
            { fits: integral.test(start) && integral.test(end), read: Number, write: (value: number) => (value < 0 ? `-${`${-value}`.padStart(Math.max(width - 1, 0), '0')}` : `${value}`.padStart(width, '0')) },
            { fits: [...start].length === 1 && [...end].length === 1 && (step === undefined || !zsh), read: (text: string) => text.codePointAt(0) ?? 0, write: (code: number) => (code === 0x5c && !zsh ? '' : String.fromCodePoint(code)) },
        ];
        const by = step === undefined ? Option.some(1) : Option.liftPredicate(Number(step), () => integral.test(step) && dialect.supports('steps'));
        const written = Option.match(Option.all([Array.findFirst(kinds, ({ fits }) => fits), by]), {
            onNone: () => Option.some([`{${[start, end, ...Array.fromNullishOr(step)].join('..')}}`]),
            onSome: ([{ read, write }, stride]) => Option.map(enumerate(read(start), read(end), stride, zsh), Array.map(write)),
        });
        return Option.filter(written, () => !(zsh && step === '0' && dialect.supports('steps')));
    });
const choose = (part: WordPart, dialect: Dialect): Option.Option<readonly (readonly WordPart[])[]> =>
    Match.value(part).pipe(
        Match.when({ type: 'BraceExp', sequence: true }, (brace) => {
            const literal = (value: string): readonly SglQuoted[] => [{ type: 'SglQuoted', start: brace.start, end: brace.end, left: brace.lbrace, right: brace.rbrace, dollar: false, value }];
            return Option.map(sequence(brace, dialect), Array.map(literal));
        }),
        Match.when({ type: 'BraceExp' }, ({ elems }) => Option.map(Option.all(elems.map(({ parts }) => alternate(parts, dialect))), Array.flatten)),
        Match.when({ type: 'Lit', value: (value: string) => dialect.options.braceccl && /\{[^{},]+\}/u.test(value) }, () => Option.none()),
        Match.orElse((plain) => Option.some([[plain]])),
    );
const alternate = (parts: readonly WordPart[], dialect: Dialect): Option.Option<readonly (readonly WordPart[])[]> =>
    parts.reduce<Option.Option<readonly (readonly WordPart[])[]>>(
        (prefixes, part) => Option.flatMap(Option.all([prefixes, choose(part, dialect)]), ([heads, tails]) => (heads.length * tails.length > limit ? Option.none() : Option.some(Array.cartesianWith(heads, tails, (head: readonly WordPart[], tail: readonly WordPart[]) => Array.appendAll(head, tail))))),
        Option.some([[]]),
    );

// --- [PARAMETERS]

const calculate = (expression: ArithmExpr | null, scope: Scope, frame: Frame) =>
    evaluate(expression, scope, {
        dialect: frame.dialect,
        text: (word, from) => {
            const [next, chunks] = expandWord(word, from, frame);
            return [next, spell(chunks)];
        },
    });
const look = (exp: ParamExp, scope: Scope, frame: Frame): readonly [Scope, Value] => {
    const direct = Value.read(exp.param?.value ?? '', scope);
    const base = Match.value({ nested: exp.nestedParam, excl: exp.excl, shaped: exp.names !== ParNamesOperator.None || exp.index !== null }).pipe(
        Match.when({ nested: { type: 'ParamExp' } }, ({ nested }) => direct.hold(look(nested, scope, frame)[1].content)),
        Match.when({ nested: Match.defined }, () => direct.obscure(false, Origin.Substitution({ programs: [] }))),
        Match.when({ excl: true, shaped: true }, () => direct.obscure(true)),
        Match.when({ excl: true }, () => direct.refer(scope)),
        Match.orElse(() => direct),
    );
    const subscript = Option.map(Option.fromNullOr(exp.index), (index) => (index.type === 'Word' && index.parts.length === 1 && index.parts[0]?.type === 'Lit' ? index.parts[0].value : ''));
    return Option.match(subscript, {
        onNone: () => [scope, direct.named && !frame.zsh ? base.first() : base],
        onSome: (written) =>
            Option.match(Record.get(spreads, written), {
                onSome: (spread) => [scope, base.spread(spread)],
                onNone: () => {
                    const [next, position] = calculate(exp.index, scope, frame);
                    return [next, base.index(position, frame.zsh, frame.dialect.options.ksharrays ? 0 : 1)];
                },
            }),
    });
};
const expandPattern = (word: Word | null | undefined, held: Held, frame: Frame): readonly [Held, Option.Option<Field>] =>
    Option.match(Option.fromNullishOr(word), {
        onNone: () => [held, Option.none()],
        onSome: ({ parts }) => {
            const within = { ...frame, site: frame.patterns, quoted: false, result: false };
            const [scope, chunks] = expandParts(parts, held.scope, within, true);
            return [{ ...held, scope, value: held.value.refuse(...chunks.filter(Chunk.$is('Refusal'))) }, Option.some(settle(gather(chunks), within))];
        },
    });
const keep: Operator = (_, held) => held;
const forget: Operator = (_, held) => ({ ...held, value: held.value.obscure() });
const refuse =
    (certain: boolean): Operator =>
    (_, held) => ({ ...held, value: held.value.obscure().refuse({ origin: held.value.origin, certain }) });
const substitute =
    (given: boolean): Operator =>
    (exp, { value, scope }, frame, plain) => {
        const [next, chunks] = Option.match(Option.fromNullishOr(given ? exp.exp?.word : null), { onNone: (): Evaluated => [scope, []], onSome: ({ parts }) => expandParts(parts, scope, { ...frame, result: true }, true) });
        const read = value.refuse(...chunks.filter(Chunk.$is('Refusal'))).hold(Content.Scalar({ text: '' }));
        const whole = !frame.quoted && splits(frame, exp) ? read.obscure(true) : read.map(() => spell(chunks));
        return plain ? { value, scope: next, spliced: Option.some(frame.quoted && Array.isReadonlyArrayEmpty(chunks) ? [quote('')] : chunks) } : { value: whole, scope: next, spliced: Option.none() };
    };
const assign: Operator = (exp, { value, scope, spliced }, frame) => {
    const [next, chunks] = Option.match(Option.fromNullishOr(exp.exp?.word), { onNone: (): Evaluated => [scope, []], onSome: (word) => expandWord(word, scope, frame) });
    const assignable = value.named && exp.index === null && exp.nestedParam === null;
    const stored = value.refuse(...chunks.filter(Chunk.$is('Refusal'))).hold(Content.Scalar({ text: '' }));
    return { value: stored.map(() => Option.filter(spell(chunks), () => assignable)), scope: assignable ? next.set(exp.param?.value ?? '', binding(spell(chunks), [value.origin])) : next, spliced };
};
const test =
    (colon: boolean, empty: Operator, full: Operator, unknown = forget): Operator =>
    (exp, held, frame, plain) =>
        Option.match(held.value.empty(colon), { onNone: () => unknown, onSome: (is) => (is ? empty : full) })(exp, held, frame, plain);
const remove =
    (prefix: boolean, largest: boolean): Operator =>
    (exp, held, frame) => {
        const [next, pattern] = expandPattern(exp.exp?.word, held, frame);
        return Option.match(pattern, { onNone: () => next, onSome: (field) => ({ ...next, value: next.value.remove(field, prefix, largest) }) });
    };
const support =
    (feature: 'casing' | 'transforms', change: (value: Value, exp: ParamExp) => Value): Operator =>
    (exp, held, frame) => ({ ...held, value: frame.dialect.supports(feature) ? change(held.value, exp) : held.value.obscure().refuse({ origin: Origin.Unsupported(), certain: true }) });
const recase = (change: (text: string) => string) => support('casing', (value, exp) => (exp.exp?.word === null ? value.recase(change) : value.obscure()));
const operators: Readonly<Record<ParExpOperator, Operator>> = {
    [ParExpOperator.DefaultUnset]: test(false, substitute(true), keep),
    [ParExpOperator.DefaultUnsetOrNull]: test(true, substitute(true), keep),
    [ParExpOperator.AlternateUnset]: test(false, substitute(false), substitute(true)),
    [ParExpOperator.AlternateUnsetOrNull]: test(true, substitute(false), substitute(true)),
    [ParExpOperator.AssignUnset]: test(false, assign, keep),
    [ParExpOperator.AssignUnsetOrNull]: test(true, assign, keep),
    [ParExpOperator.ErrorUnset]: test(false, refuse(true), keep, refuse(false)),
    [ParExpOperator.ErrorUnsetOrNull]: test(true, refuse(true), keep, refuse(false)),
    [ParExpOperator.RemSmallPrefix]: remove(true, false),
    [ParExpOperator.RemLargePrefix]: remove(true, true),
    [ParExpOperator.RemSmallSuffix]: remove(false, false),
    [ParExpOperator.RemLargeSuffix]: remove(false, true),
    [ParExpOperator.UpperFirst]: recase((text) => `${text.slice(0, 1).toUpperCase()}${text.slice(1)}`),
    [ParExpOperator.UpperAll]: recase((text) => text.toUpperCase()),
    [ParExpOperator.LowerFirst]: recase((text) => `${text.slice(0, 1).toLowerCase()}${text.slice(1)}`),
    [ParExpOperator.LowerAll]: recase((text) => text.toLowerCase()),
    [ParExpOperator.OtherParamOps]: support('transforms', (value) => value.obscure()),
    [ParExpOperator.MatchEmpty]: forget,
    [ParExpOperator.ArrayExclude]: forget,
    [ParExpOperator.ArrayIntersect]: forget,
};
const operate =
    (exp: ParamExp, frame: Frame, plain: boolean) =>
    (held: Held): Held =>
        exp.exp === null ? held : operators[exp.exp.op](exp, held, frame, plain);
const slice =
    (bounds: Slice | null, frame: Frame) =>
    (held: Held): Held =>
        Option.match(Option.fromNullOr(bounds), {
            onNone: () => held,
            onSome: ({ offset, length }) => {
                const [scope, edges] = Array.mapAccum([offset, ...Array.fromNullishOr(length)], held.scope, (from, edge) => calculate(edge, from, frame));
                return { ...held, scope, value: held.value.slice(edges, frame.dialect.supports('negative'), held.scope.name) };
            },
        });
const replace =
    (found: Replace | null, frame: Frame) =>
    (held: Held): Held =>
        Option.match(Option.fromNullOr(found), {
            onNone: () => held,
            onSome: (repl) => {
                const head = Option.filter(Array.head(repl.orig?.parts ?? []), (part): part is Lit => part.type === 'Lit');
                const anchor = Option.filter(
                    Option.map(head, ({ value }) => value.charAt(0)),
                    (mark) => (mark === '#' || mark === '%') && (frame.zsh || !repl.all),
                );
                const bare = Option.map(Option.fromNullOr(repl.orig), (word) => ({ ...word, parts: word.parts.map((part, at) => (at === 0 && part.type === 'Lit' && Option.isSome(anchor) ? { ...part, value: part.value.slice(1) } : part)) }));
                const [searched, pattern] = expandPattern(Option.getOrNull(bare), held, frame);
                const [scope, chunks] = Option.match(Option.fromNullOr(repl.with), { onNone: (): Evaluated => [searched.scope, []], onSome: (word) => expandWord(word, searched.scope, frame) });
                const put = (match: string) => spell(chunks, ({ text, quoted }) => (frame.dialect.options.patsub && !quoted ? text.replaceAll('&', match) : text));
                const value = searched.value.refuse(...chunks.filter(Chunk.$is('Refusal')));
                return { ...searched, scope, value: Option.match(pattern, { onNone: () => value.obscure(), onSome: (field) => value.replace(field, anchor, repl.all, put) }) };
            },
        });
const render = ({ content, origin: read }: Value, frame: Frame, scope: Scope, exp?: ParamExp): readonly Chunk[] => {
    const { quoted, site, dialect } = frame;
    const strict = exp !== undefined && dialect.options.nounset;
    return Content.$match(content, {
        Unset: () => [...(strict ? [Chunk.Refusal({ origin: read, certain: true })] : []), ...(quoted || site.single ? [emit('', frame, exp)] : [])],
        Scalar: ({ text }) => [emit(text, frame, exp)],
        List: ({ items, spread }) => {
            const glue = spread && site.single && dialect.variant === LangVariant.LangBash ? Option.some(' ') : Option.map(separators(scope), (ifs) => [...ifs][0] ?? '');
            const joined = Option.match(glue, { onNone: () => Chunk.Unknown({ origin: Origin.Variable({ name: 'IFS', origins: HashSet.empty() }), certain: true, many: false }), onSome: (between) => emit(items.join(between), frame, exp) });
            const broken = items.map((item): Chunk => emit(item, frame, exp));
            return site.single || (quoted && !spread) ? [joined] : Array.intersperse(broken, Chunk.Break());
        },
        Unknown: ({ origin, many }) => [...(strict ? [Chunk.Refusal({ origin, certain: false })] : []), obscure(origin, frame, many, exp)],
    });
};
const expandParameter = (exp: ParamExp, trailing: readonly string[], scope: Scope, frame: Frame): Evaluated => {
    const { isSet: tests } = exp;
    const letters = [...exp.modifiers.map(({ value }) => value), ...trailing];
    const flags = Value.flags(exp.flags?.value ?? '');
    const plain = Array.isReadonlyArrayEmpty(letters) && Option.exists(flags, ({ size }) => size === 0) && !exp.length && exp.slice === null && exp.repl === null;
    const modelled = !(exp.width || exp.rcExpand === 1 || exp.subscripts.length > 0 || (exp.names !== ParNamesOperator.None && !exp.excl));
    const [looked, found] = look(exp, scope, frame);
    const start: Held = { value: tests ? found.exists() : found, scope: looked, spliced: Option.none() };
    const { value, scope: next, spliced } = pipe(start, operate(exp, frame, plain), slice(exp.slice, frame), replace(exp.repl, frame));
    const modified = letters.reduce((held, letter) => held.modify(letter, next.text('PWD')), value);
    const flagged = Option.map(flags, (held) => (exp.length ? modified.measure() : modified).flag(held, frame.quoted));
    const body = Option.match(
        Option.filter(flagged, () => modelled),
        { onNone: () => render(value.obscure(true), frame, next, exp), onSome: (final) => Option.getOrElse(spliced, () => render(final, frame, next, exp)) },
    );
    return [next, [...value.refusals.map(Chunk.Refusal), ...body]];
};

// --- [WORDS]

const decode = (raw: string, { dialect, zsh }: Frame, secret: boolean): Chunk => {
    const found = raw.matchAll(/\\(?:c(?<caret>[\s\S])|C-(?<chord>[\s\S])|M-(?<meta>[\s\S])|x(?<hex>[\da-fA-F]{1,2})|(?<point>[uU][\da-fA-F]+)|(?<octal>[0-7]{1,3})|(?<other>[\s\S]))|(?<plain>[^\\]+)|\\$/gu);
    const encode = ([form, held]: readonly [string, string | undefined]) =>
        Option.map(
            Option.filter(Record.get(ansi, form), () => Predicate.isString(held)),
            (write) => write(held ?? '', zsh, dialect),
        );
    return pipe(
        Option.all(Array.fromIterable(found).map(({ groups = {} }) => Option.getOrElse(Array.findFirst(Record.toEntries(groups), encode), () => Option.some('%5C')))),
        Option.map((parts) => parts.join('')),
        Option.map((bytes) => Option.liftThrowable(decodeURIComponent)(zsh || !bytes.includes('%00') ? bytes : bytes.slice(0, bytes.indexOf('%00')))),
        Option.match({
            onNone: () => Chunk.Refusal({ origin: Origin.Bytes(), certain: true }),
            onSome: Option.match({ onNone: () => Chunk.Unknown({ origin: Origin.Bytes(), certain: true, many: false }), onSome: (text) => quote(text, secret) }),
        }),
    );
};
const scan = (text: string, frame: Frame, secret: boolean): readonly Chunk[] =>
    Array.fromIterable(text.matchAll(/\\(?<escaped>[\s\S])|(?<plain>[^\\]+)|\\$/gu)).flatMap(({ groups = {} }) => {
        const { escaped = '\\', plain } = groups;
        const kept = escaped === '\n' ? [] : [quote(escaped, secret)];
        return plain === undefined ? kept : [Chunk.Text({ text: plain, quoted: false, split: frame.result && splits(frame), glob: Option.isSome(frame.site.glob), secret })];
    });
const expandTilde = (lit: Lit, next: Option.Option<WordPart>, scope: Scope, { zsh }: Frame, colons: boolean): readonly [readonly Chunk[], Lit] => {
    const { prefix = '', user = '' } = (colons ? /^(?<prefix>~(?<user>[^/\\:]*))/u : /^(?<prefix>~(?<user>[^/\\]*))/u).exec(lit.value)?.groups ?? {};
    const rest = { ...lit, value: lit.value.slice(prefix.length) };
    const home = (of: Option.Option<string>) => [...(zsh && Option.isSome(of) ? [Chunk.Refusal({ origin: Origin.Home({ user: of }), certain: false })] : []), Chunk.Unknown({ origin: Origin.Home({ user: of }), certain: true, many: false })];
    const stacked = (name: string) => {
        const unknown = (origins: HashSet.HashSet<Origin>) => [Chunk.Unknown({ origin: Origin.Variable({ name, origins }), certain: true, many: false })];
        return Binding.$match(scope.get(name), { Known: ({ value }) => [quote(value)], Unset: () => [quote(prefix)], Listed: () => unknown(HashSet.empty()), Unknown: ({ origins }) => unknown(origins) });
    };
    const opening = Option.flatMap(next, (part) =>
        Match.value(part).pipe(
            Match.when({ type: 'SglQuoted', dollar: false }, ({ value }) => Option.some(value)),
            Match.when({ type: 'DblQuoted' }, ({ parts }) =>
                Option.map(
                    Option.filter(Array.head(parts), (inner): inner is Lit => inner.type === 'Lit'),
                    ({ value }) => value,
                ),
            ),
            Match.orElse(() => Option.none()),
        ),
    );
    const slashed = Option.exists(opening, (text) => text.startsWith('/'));
    const found = Match.value({ tilde: prefix !== '', open: rest.value === '' && Option.isSome(next), zsh, slashed, user }).pipe(
        Match.when({ tilde: false }, () => Option.none()),
        Match.when({ open: true, zsh: false }, () => Option.none()),
        Match.when({ open: true, slashed: false }, () => Option.some(user).pipe(home, Option.some)),
        Match.when({ user: '' }, () => Option.some(Option.match(scope.text('HOME'), { onNone: () => home(Option.none()), onSome: (value) => [quote(value)] }))),
        Match.when({ user: '+' }, () => Option.some(stacked('PWD'))),
        Match.when({ user: '-' }, () => Option.some(stacked('OLDPWD'))),
        Match.when({ user: (name: string) => /^[+-](?!\d+$)/u.test(name) }, () => Option.some([quote(prefix)])),
        Match.orElse(() => Option.some(user).pipe(home, Option.some)),
    );
    return Option.match(found, { onNone: () => [[], lit], onSome: (chunks) => [chunks, rest] });
};
const expandPart = (part: WordPart, trailing: readonly string[], scope: Scope, frame: Frame): Evaluated => {
    const { site, quoted } = frame;
    const secret = frame.secret(part);
    const substituted = (node: WordPart) => Origin.Substitution({ programs: Option.getOrElse(HashMap.get(scope.substitutions, node), Array.empty<string>) });
    return Match.value(part).pipe(
        Match.discriminatorsExhaustive('type')({
            Lit: ({ value }): Evaluated => {
                const inside = Option.map(
                    Option.liftPredicate(value, () => quoted),
                    unescape(/\\(?<escaped>[$`"\\\n])/gu),
                );
                const literal = Option.match(site.heredoc, { onNone: () => inside, onSome: (expands) => Option.some(expands ? unescape(/\\(?<escaped>[$`\\\n])/gu)(value) : value) });
                return [scope, Option.match(literal, { onNone: () => scan(value, frame, secret), onSome: (text) => [quote(text, secret)] })];
            },
            SglQuoted: ({ value, dollar }): Evaluated => [
                scope,
                [
                    Match.value({ quoted, dollar }).pipe(
                        Match.when({ quoted: true }, () => quote(`${dollar ? '$' : ''}'${value}'`, secret)),
                        Match.when({ dollar: true }, () => decode(value, frame, secret)),
                        Match.orElse(() => quote(value, secret)),
                    ),
                ],
            ],
            DblQuoted: ({ parts }): Evaluated => (Array.isReadonlyArrayEmpty(parts) ? [scope, [quote('')]] : expandParts(parts, scope, { ...frame, quoted: true }, false)),
            ParamExp: (exp) => expandParameter(exp, trailing, scope, frame),
            CmdSubst: (node): Evaluated => [scope, [obscure(substituted(node), frame)]],
            ProcSubst: (node): Evaluated => [scope, [Chunk.Unknown({ origin: substituted(node), certain: true, many: false })]],
            ArithmExp: ({ x }): Evaluated => {
                const [next, result] = calculate(x, scope, frame);
                const chunks = Match.valueTags(result, {
                    Known: ({ value }) => [emit(`${value}`, frame)],
                    Unknown: () => [Chunk.Refusal({ origin: Origin.Arithmetic(), certain: false }), obscure(Origin.Arithmetic(), frame)],
                    Refused: () => [Chunk.Refusal({ origin: Origin.Arithmetic(), certain: true })],
                });
                return [next, chunks];
            },
            ExtGlob: ({ pattern }): Evaluated => [scope, [Chunk.Unknown({ origin: Origin.Glob({ text: pattern?.value ?? '' }), certain: false, many: !site.single && Option.isSome(site.glob) })]],
            BraceExp: (): Evaluated => [scope, [Chunk.Unknown({ origin: Origin.Braces(), certain: false, many: true })]],
        }),
    );
};
const expandParts = (parts: readonly WordPart[], scope: Scope, frame: Frame, start: boolean): Evaluated => {
    const { site, quoted, zsh, dialect } = frame;
    const head = Option.filter(Array.head(parts), (part): part is Lit => start && !quoted && part.type === 'Lit');
    const colons = !quoted && (site.colons || (site.named && Option.exists(head, ({ value }) => assignment.test(value))));
    const items = colons ? parts.flatMap((part) => (part.type === 'Lit' ? part.value.split(/(?<=:|^[A-Za-z_]\w*=)(?=~)/u).map((value): WordPart => ({ ...part, value })) : [part])) : parts;
    const trails = items.map((part, at) => {
        const next = items[at + 1];
        return zsh && part.type === 'ParamExp' && part.short && next?.type === 'Lit' ? Array.fromIterable(next.value.matchAll(/:(?<letter>[aAcehlPqQrstu&])/guy)).map(([, letter = '']) => letter) : [];
    });
    const tildes = (at: number) => site.tilde && !quoted && ((at === 0 && start) || (colons && Option.exists(Array.get(items, at - 1), (before) => before.type === 'Lit' && /(?::|^[A-Za-z_]\w*=)$/u.test(before.value))));
    const [next, chunks] = Array.mapAccum(items, scope, (from, part, at) => {
        const cut = part.type === 'Lit' ? { ...part, value: part.value.slice((trails[at - 1]?.length ?? 0) * 2) } : part;
        const [lead, kept] = cut.type === 'Lit' && tildes(at) ? expandTilde(cut, Array.get(items, at + 1), scope, frame, colons) : ([[], cut] as const);
        const [to, found] = kept.type === 'Lit' && kept.value === '' ? ([from, []] as const) : expandPart(kept, trails[at] ?? [], from, frame);
        return [to, [...lead, ...found]] as const;
    });
    const command = Option.filter(head, ({ value }) => !frame.result && !site.single && dialect.options.equals && /^=[^/=]/u.test(value));
    return [
        next,
        Option.match(
            Option.map(command, ({ value }) => Origin.Command({ name: value.slice(1) })),
            {
                onNone: () => chunks.flat(),
                onSome: (origin) => [Chunk.Refusal({ origin, certain: false }), Chunk.Unknown({ origin, certain: true, many: false }), ...chunks.slice(1).flat()],
            },
        ),
    ];
};
const expandWord = (word: Word, scope: Scope, frame: Frame): Evaluated => expandParts(word.parts, scope, { ...frame, quoted: false, result: false }, true);

// --- [FIELDS]

const append =
    (segment: Segment, certain: boolean, many: boolean) =>
    (state: Splitting): Splitting => ({ ...state, current: { segments: [...state.current.segments, segment], certain: state.current.certain || certain }, started: true, spaced: false, exact: state.exact && !many });
const end =
    (space: boolean, nulls = true) =>
    (state: Splitting): Splitting => {
        const kept = nulls || Array.isReadonlyArrayNonEmpty(state.current.segments) ? [state.current] : [];
        return state.started ? { fields: [...state.fields, ...kept], current: blank, started: false, spaced: space, exact: state.exact && state.current.certain } : { ...state, spaced: state.spaced && space };
    };
const delimit = (state: Splitting): Splitting => (state.spaced ? { ...state, spaced: false } : { ...state, fields: [...state.fields, { ...state.current, certain: true }], current: blank, started: false });
const split = (chunks: readonly Chunk[], ifs: Option.Option<string>, dialect: Dialect): { readonly fields: readonly Draft[]; readonly exact: boolean } => {
    const mark = (state: Splitting): Splitting => (state.spaced && !dialect.supports('nulls') ? state : { ...state, current: { ...state.current, certain: true }, started: true, spaced: false });
    const breaks = Option.map(ifs, (by) => {
        const classes = [true, false].map((whitespace) => Array.fromIterable(by).filter((character) => spaces.includes(character) === whitespace));
        const [white = '', hard = ''] = classes.map((characters) => characters.map(RegExp.escape).join(''));
        return new RegExp(`(?<space>[${white}]+)|(?<delimiter>[${hard}])|[^${white}${hard}]+`, 'gu');
    });
    const cut = (text: Extract<Chunk, { readonly _tag: 'Text' }>, pattern: RegExp) =>
        Array.fromIterable(text.text.matchAll(pattern)).map(([piece, space, delimiter]) => {
            const ended = space === undefined ? delimit : end(true, dialect.supports('leadingnulls'));
            return space === undefined && delimiter === undefined ? append({ ...text, text: piece }, true, false) : ended;
        });
    const steps = chunks.flatMap(
        Chunk.$match({
            Break: () => [end(false)],
            Refusal: () => [],
            Unknown: (unknown) => [append(unknown, unknown.certain, unknown.many)],
            Text: (text) =>
                Match.value({ split: text.split, empty: text.text === '', breaks: Option.getOrUndefined(breaks) }).pipe(
                    Match.when({ split: false, empty: true }, () => (text.quoted ? [mark] : [])),
                    Match.when({ split: false }, () => [append(text, true, false)]),
                    Match.when({ empty: true }, () => []),
                    Match.when({ breaks: Match.defined }, (found) => cut(text, found.breaks)),
                    Match.orElse(() => [append(Chunk.Unknown({ origin: Origin.Variable({ name: 'IFS', origins: HashSet.empty() }), certain: false, many: true }), false, true)]),
                ),
        }),
    );
    const last = steps.reduce((state, step) => step(state), unsplit);
    return { fields: [...last.fields, ...(last.started ? [last.current] : [])], exact: last.exact && (!last.started || last.current.certain) };
};
const gather = (chunks: readonly Chunk[]): Draft => ({ segments: chunks.filter((chunk): chunk is Segment => chunk._tag === 'Unknown' || (chunk._tag === 'Text' && chunk.text !== '')), certain: true });
const settle = ({ segments }: Draft, { site }: Frame): Field => {
    const active = segments.flatMap((segment) => (segment._tag === 'Text' && segment.glob ? [segment.text] : [])).join('');
    const match = Option.flatMap(site.glob, (read) => read(active));
    const pieces = segments.map((segment) =>
        Match.value(segment).pipe(
            Match.when({ _tag: 'Unknown' }, ({ origin }) => Piece.Unknown({ origin })),
            Match.when({ secret: true }, ({ text }) => Piece.Secret({ text: Redacted.make(text) })),
            Match.when({ glob: true }, ({ text }) => (Option.isSome(match) ? Piece.Wild({ text }) : Piece.Text({ text }))),
            Match.orElse(({ text }) => Piece.Text({ text })),
        ),
    );
    const pattern = Option.all(pieces.map(Piece.$match({ Text: ({ text }) => Option.some(Glob.escape(text)), Wild: ({ text }) => Option.some(text.replaceAll(/[{}]/gu, String.raw`\$&`)), Secret: () => Option.none(), Unknown: () => Option.none() })));
    return new Field({ pieces, matching: Option.map(match, (compile) => compile(Option.map(pattern, Array.join('')))) });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const expand = (word: Word, scope: Scope, dialect: Dialect, at: { readonly braces: Option.Option<Word>; readonly role: Role; readonly secret: (part: WordPart) => boolean }): { readonly expansion: Expansion; readonly scope: Scope } => {
    const zsh = dialect.variant === LangVariant.LangZsh;
    const site = place(at.role, dialect);
    const frame: Frame = { dialect, zsh, site, patterns: place(Role.Pattern(), dialect), quoted: Option.isSome(site.heredoc), result: false, secret: at.secret };
    const kept = zsh && Option.exists(at.braces, ({ parts }) => parts.some((part) => part.type === 'BraceExp'));
    const opaque = { fields: [Field.unknown(Origin.Braces())], exact: false };
    return Option.match(site.braces ? alternate(Option.getOrElse(at.braces, () => word).parts, dialect) : Option.some([word.parts]), {
        onNone: () => ({ expansion: new Expanded({ ...opaque, refusals: [] }), scope }),
        onSome: (alternatives) => {
            const [next, words] = Array.mapAccum(alternatives, scope, (from, parts) => {
                const [to, chunks] = expandParts(parts, from, frame, true);
                const made = site.single ? { fields: [gather(chunks)], exact: true } : split(chunks, separators(to), dialect);
                return [to, { ...made, fields: kept && Array.isReadonlyArrayEmpty(made.fields) ? [{ ...blank, certain: true }] : made.fields, chunks }] as const;
            });
            const fields = words.flatMap((made) => made.fields.map((draft) => settle(draft, frame)));
            const paths = fields.flatMap((field) => Option.toArray(Option.map(Option.filter(field.matching, Matching.$is('Paths')), ({ unmatched }) => ({ field, unmatched }))));
            const exact = words.every((made) => made.exact) && Array.isReadonlyArrayEmpty(paths);
            const unmatched = paths.filter((path) => path.unmatched._tag === 'Refused').map(({ field }) => ({ origin: Origin.Glob({ text: field.pieces.map((piece) => (piece._tag === 'Text' || piece._tag === 'Wild' ? piece.text : '')).join('') }), certain: false }));
            const refusals = [...words.flatMap(({ chunks }) => chunks.filter(Chunk.$is('Refusal'))), ...unmatched];
            const certain = Option.map(
                Array.findFirst(refusals, (refusal) => refusal.certain),
                ({ origin }) => origin,
            );
            const refused = Option.orElse(certain, () => Option.liftPredicate(Origin.Ambiguous(), () => site.sole && exact && fields.length !== 1));
            const expanded = new Expanded({ ...(kept && words.some(({ chunks }) => chunks.some(Chunk.$is('Break'))) ? opaque : { fields, exact }), refusals: [...refusals.map(({ origin }) => origin), ...(site.sole && !exact ? [Origin.Ambiguous()] : [])] });
            return { expansion: Option.match(refused, { onNone: () => expanded, onSome: (origin) => new Refused({ origin }) }), scope: next };
        },
    });
};
const unquote = (word: Word, dialect: Dialect): Option.Option<string> => {
    const empty = new Scope({ variables: HashMap.empty(), unlisted: Binding.Unset(), positional: Option.some([]), name: dialect.binary, substitutions: HashMap.empty() });
    const { expansion } = expand(word, empty, dialect, { braces: Option.none(), role: Role.Argument(), secret: () => false });
    return expansion._tag === 'Expanded' ? expansion.text : Option.none();
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { expand, unquote };
