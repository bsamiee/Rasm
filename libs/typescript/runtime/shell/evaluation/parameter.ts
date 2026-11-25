import { Array, Data, Match, Option } from 'effect';
import { type ArithmExpr, LangVariant, ParExpOperator, ParNamesOperator, type ParamExp, type Replace, type Slice, type Word, type WordPart } from 'sh-syntax';
import { evaluated, Integer } from '#/evaluation/arithmetic';
import { Binding, type Field, type Matching, Origin, type Scope } from '#/evaluation/value';
import type { Dialect } from '#/syntax/dialect';

// --- [TYPES] ---------------------------------------------------------------------------

type Chunk = Data.TaggedEnum<{
    Text: { readonly text: string; readonly quoted: boolean; readonly split: boolean; readonly glob: boolean; readonly secret: boolean };
    Hidden: { readonly origin: Origin; readonly certain: boolean; readonly many: boolean };
    Break: object;
    Refuse: { readonly origin: Origin; readonly certain: boolean };
}>;
type Value = Data.TaggedEnum<{
    Unset: object;
    Scalar: { readonly text: string };
    List: { readonly items: readonly string[]; readonly spread: boolean };
    Unknown: { readonly origin: Origin; readonly many: boolean };
}>;
type Parted = readonly [Scope, readonly Chunk[]];
type Stepped = readonly [Value, Scope, readonly Chunk[]];
interface Rules {
    readonly braces: boolean;
    readonly split: boolean;
    readonly glob: Option.Option<Matching['_tag']>;
    readonly single: boolean;
    readonly tilde: boolean;
    readonly colons: boolean;
    readonly named: boolean;
    readonly heredoc: Option.Option<boolean>;
}
interface Frame {
    readonly dialect: Dialect;
    readonly rules: Rules;
    readonly quoted: boolean;
    readonly result: boolean;
    readonly secret: (part: WordPart) => boolean;
    readonly worded: (parts: readonly WordPart[], scope: Scope, frame: Frame, start: boolean) => Parted;
    readonly patterned: (word: Word, scope: Scope, frame: Frame) => readonly [Field, Scope, readonly Chunk[]];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const Chunk = Data.taggedEnum<Chunk>();
const Value = Data.taggedEnum<Value>();
const identifier = /^[A-Za-z_]\w*$/u;
const closing: Readonly<Record<string, string>> = { '(': ')', '[': ']', '{': '}', '<': '>' };
const defaults = [ParExpOperator.DefaultUnset, ParExpOperator.DefaultUnsetOrNull] as const;
const alternates = [ParExpOperator.AlternateUnset, ParExpOperator.AlternateUnsetOrNull] as const;
const assigns = [ParExpOperator.AssignUnset, ParExpOperator.AssignUnsetOrNull] as const;
const errors = [ParExpOperator.ErrorUnset, ParExpOperator.ErrorUnsetOrNull] as const;
const colons = new Set<number>([ParExpOperator.DefaultUnsetOrNull, ParExpOperator.AlternateUnsetOrNull, ParExpOperator.AssignUnsetOrNull, ParExpOperator.ErrorUnsetOrNull]);
const removals = new Map<number, { readonly prefix: boolean; readonly largest: boolean }>([
    [ParExpOperator.RemSmallPrefix, { prefix: true, largest: false }],
    [ParExpOperator.RemLargePrefix, { prefix: true, largest: true }],
    [ParExpOperator.RemSmallSuffix, { prefix: false, largest: false }],
    [ParExpOperator.RemLargeSuffix, { prefix: false, largest: true }],
]);
const cases = new Map<number, (text: string) => string>([
    [ParExpOperator.UpperAll, (text) => text.toUpperCase()],
    [ParExpOperator.LowerAll, (text) => text.toLowerCase()],
    [ParExpOperator.UpperFirst, (text) => `${text.slice(0, 1).toUpperCase()}${text.slice(1)}`],
    [ParExpOperator.LowerFirst, (text) => `${text.slice(0, 1).toLowerCase()}${text.slice(1)}`],
]);
const modifiers = new Map<string, (text: string, pwd: Option.Option<string>) => Option.Option<string>>([
    ['t', (text) => Option.some(text.replace(/\/+$/u, '').split('/').at(-1) ?? '')],
    [
        'h',
        (text) =>
            Option.some(
                Match.value(text.replace(/(?<=.)\/+$/u, '')).pipe(
                    Match.when(Match.is('/'), () => '/'),
                    Match.when(
                        (path: string) => !path.includes('/'),
                        () => '.',
                    ),
                    Match.orElse((path) => path.slice(0, path.lastIndexOf('/')).replace(/\/+$/u, '') || '/'),
                ),
            ),
    ],
    ['r', (text) => Option.some(text.lastIndexOf('.') > text.lastIndexOf('/') ? text.slice(0, text.lastIndexOf('.')) : text)],
    ['e', (text) => Option.some(text.lastIndexOf('.') > text.lastIndexOf('/') ? text.slice(text.lastIndexOf('.') + 1) : '')],
    ['l', (text) => Option.liftPredicate(text.toLowerCase(), () => ascii(text))],
    ['u', (text) => Option.liftPredicate(text.toUpperCase(), () => ascii(text))],
    ['a', (text, pwd) => (text === '' ? Option.some('') : Option.map(text.startsWith('/') ? Option.some(text) : Option.map(pwd, (cwd) => `${cwd}/${text}`), absolute))],
]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const ascii = (text: string) => /^\p{ASCII}*$/u.test(text);
const absolute = (path: string) =>
    `/${path
        .split('/')
        .reduce<readonly string[]>(
            (kept, segment) =>
                Match.value(segment).pipe(
                    Match.when('..', () => kept.slice(0, -1)),
                    Match.when(Match.is('.', ''), () => kept),
                    Match.orElse((name) => [...kept, name]),
                ),
            [],
        )
        .join('/')}`;
const held = (text: string, secret = false) => Chunk.Text({ text, quoted: true, split: false, glob: false, secret });
const refusal = (origin: Origin) => Chunk.Refuse({ origin, certain: true });
const splits = (frame: Frame, exp?: ParamExp) => frame.rules.split && (exp?.split === 1 || (frame.dialect.options.wordsplit && exp?.split !== 2));
const globs = (frame: Frame, exp?: ParamExp) => Option.isSome(frame.rules.glob) && (exp?.globSubst === 1 || (frame.dialect.options.globsubst && exp?.globSubst !== 2));
const many = (value: Value) => value._tag === 'List' || (value._tag === 'Unknown' && value.many);
const mapped = (value: Value, change: (text: string) => Option.Option<string>, origin: Origin): Value =>
    Value.$match(value, {
        Unset: () => value,
        Unknown: () => value,
        Scalar: ({ text }) => Option.match(change(text), { onNone: () => Value.Unknown({ origin, many: false }), onSome: (changed) => Value.Scalar({ text: changed }) }),
        List: ({ items, spread }) => Option.match(Option.all(items.map(change)), { onNone: () => Value.Unknown({ origin, many: true }), onSome: (changed) => Value.List({ items: changed, spread }) }),
    });
const valued = (word: Word, scope: Scope, frame: Frame): readonly [Option.Option<string>, Scope, readonly Chunk[]] => {
    const [next, chunks] = frame.worded(word.parts, scope, { ...frame, quoted: false, result: false }, true);
    const text = Option.all(chunks.map(Chunk.$match({ Text: ({ text: found }) => Option.some(found), Break: () => Option.some(' '), Hidden: () => Option.none(), Refuse: () => Option.some('') })));
    return [Option.map(text, (texts) => texts.join('')), next, chunks.filter(Chunk.$is('Refuse'))];
};
const counted = (expression: ArithmExpr | null, scope: Scope, frame: Frame) => evaluated(expression, scope, frame.dialect, (word, from) => valued(word, from, frame));
const bound = (name: string, scope: Scope): Value =>
    Match.value(name).pipe(
        Match.when('0', () => Value.Scalar({ text: scope.name })),
        Match.when(
            (candidate: string) => /^(?:[@*#]|[1-9]\d*)$/u.test(candidate),
            (special) =>
                Option.match(scope.positional, {
                    onNone: () => Value.Unknown({ origin: Origin.Variable({ name }), many: special === '@' || special === '*' }),
                    onSome: (positional) =>
                        Match.value(special).pipe(
                            Match.when('@', () => Value.List({ items: positional, spread: true })),
                            Match.when('*', () => Value.List({ items: positional, spread: false })),
                            Match.when('#', () => Value.Scalar({ text: `${positional.length}` })),
                            Match.orElse((position) => Option.match(Array.get(positional, Number(position) - 1), { onNone: () => Value.Unset(), onSome: (text) => Value.Scalar({ text }) })),
                        ),
                }),
        ),
        Match.orElse(() => Binding.$match(scope.get(name), { Unset: () => Value.Unset(), Known: ({ value }) => Value.Scalar({ text: value }), Listed: ({ items }) => Value.List({ items, spread: false }), Unknown: () => Value.Unknown({ origin: Origin.Variable({ name }), many: false }) })),
    );
const listed = (value: Value, spread: boolean, origin: Origin): Value =>
    Value.$match(value, {
        Unset: () => Value.List({ items: [], spread }),
        Scalar: ({ text }) => Value.List({ items: [text], spread }),
        List: ({ items }) => Value.List({ items, spread }),
        Unknown: () => Value.Unknown({ origin, many: true }),
    });
const picked = (value: Value, position: Integer, frame: Frame, origin: Origin): Value => {
    const offset = frame.dialect.options.ksharrays ? 0 : 1;
    const unknown = Value.Unknown({ origin, many: false });
    const element = (items: readonly string[]) => (position._tag === 'Known' && (position.value !== 0n || offset === 0) ? Value.Scalar({ text: items.at(position.value < 0n ? Number(position.value) : Number(position.value) - offset) ?? '' }) : unknown);
    return Match.value({ value, zsh: frame.dialect.variant === LangVariant.LangZsh }).pipe(
        Match.when({ value: { _tag: 'List' }, zsh: true }, ({ value: { items } }) => element(items)),
        Match.when({ value: { _tag: 'Scalar' }, zsh: true }, ({ value: { text } }) => (ascii(text) ? element([...text]) : unknown)),
        Match.when({ value: { _tag: 'Scalar' } }, () => Integer.$match(position, { Known: ({ value: at }) => (at === 0n ? value : Value.Unset()), Unknown: () => unknown, Refused: () => unknown })),
        Match.when({ value: { _tag: 'List' } }, ({ value: { items } }) => (origin._tag === 'Variable' && identifier.test(origin.name) ? element(items) : unknown)),
        Match.orElse(() => value),
    );
};
const looked = (exp: ParamExp, scope: Scope, frame: Frame): readonly [Value, Scope] => {
    const name = exp.param?.value ?? '';
    const origin = Origin.Variable({ name });
    const direct = bound(name, scope);
    const base = Match.value({ nested: exp.nestedParam, excl: exp.excl, shaped: exp.names !== ParNamesOperator.None || exp.index !== null }).pipe(
        Match.when({ nested: { type: 'ParamExp' } }, ({ nested }) => looked(nested, scope, frame)[0]),
        Match.when({ nested: Match.defined }, () => Value.Unknown({ origin: Origin.Substitution(), many: false })),
        Match.when({ excl: true, shaped: true }, () => Value.Unknown({ origin, many: true })),
        Match.when({ excl: true }, () => (direct._tag === 'Scalar' && /^(?:[A-Za-z_]\w*|\d+|[@*#])$/u.test(direct.text) ? bound(direct.text, scope) : Value.Unknown({ origin, many: false }))),
        Match.orElse(() => direct),
    );
    const { index } = exp;
    const [only] = index?.type === 'Word' && index.parts.length === 1 ? index.parts : [];
    const first = base._tag === 'List' && identifier.test(name) && frame.dialect.variant !== LangVariant.LangZsh ? Option.match(Array.head(base.items), { onNone: () => Value.Unset(), onSome: (text) => Value.Scalar({ text }) }) : base;
    return Match.value(index === null ? Option.none() : Option.some(only?.type === 'Lit' ? only.value : '')).pipe(
        Match.when({ _tag: 'None' }, () => [first, scope] as const),
        Match.when({ value: '@' }, () => [listed(base, true, origin), scope] as const),
        Match.when({ value: '*' }, () => [listed(base, false, origin), scope] as const),
        Match.orElse(() => {
            const [position, next] = counted(index, scope, frame);
            return [picked(base, position, frame, origin), next] as const;
        }),
    );
};
const sliced = (slice: Slice, value: Value, scope: Scope, frame: Frame, origin: Origin): Stepped => {
    const [offset, after] = counted(slice.offset, scope, frame);
    const [measured, next] = slice.length === null ? [undefined, after] : counted(slice.length, after, frame);
    const length = Option.fromUndefinedOr(measured);
    const bounds = [offset, ...Option.toArray(length)];
    const negative = Option.exists(length, (found) => found._tag === 'Known' && found.value < 0n);
    const cut = (items: readonly string[]) => {
        const [from = 0n, count] = bounds.map((edge) => (edge._tag === 'Known' ? edge.value : 0n));
        const begin = from < 0n ? items.length + Number(from) : Number(from);
        const end = Option.match(Option.fromUndefinedOr(count), { onNone: () => items.length, onSome: (taken) => (taken < 0n ? items.length : begin) + Number(taken) });
        return Match.value({ outside: begin < 0 || begin > items.length, reversed: end < begin }).pipe(
            Match.when({ outside: true }, () => Option.some<readonly string[]>([])),
            Match.when({ reversed: true }, () => Option.none()),
            Match.orElse(() => Option.some(items.slice(begin, end))),
        );
    };
    const unknown = Value.Unknown({ origin: Origin.Arithmetic(), many: many(value) });
    return Match.value({ refused: bounds.some(Integer.$is('Refused')) || (negative && (!frame.dialect.supports('negative') || value._tag === 'List')), known: bounds.every(Integer.$is('Known')) }).pipe(
        Match.when({ refused: true }, () => [unknown, next, [refusal(Origin.Arithmetic())]] as const),
        Match.when({ known: false }, () => [unknown, next, [Chunk.Refuse({ origin: Origin.Arithmetic(), certain: false })]] as const),
        Match.orElse(
            () =>
                [
                    Value.$match(value, {
                        Unset: () => value,
                        Unknown: () => value,
                        Scalar: ({ text }) => (ascii(text) ? Option.match(cut([...text]), { onNone: () => unknown, onSome: (characters) => Value.Scalar({ text: characters.join('') }) }) : Value.Unknown({ origin, many: false })),
                        List: ({ items, spread }) => Option.match(cut(origin._tag === 'Variable' && identifier.test(origin.name) ? items : [scope.name, ...items]), { onNone: () => unknown, onSome: (kept) => Value.List({ items: kept, spread }) }),
                    }),
                    next,
                    [],
                ] as const,
        ),
    );
};
const longest = (text: string, pattern: Field, at: number, whole: boolean): Option.Option<Option.Option<number>> => {
    const ends = whole ? [text.length] : Array.range(at, text.length).toReversed();
    return Option.map(Option.all(ends.map((end) => pattern.matches(text.slice(at, end)))), (verdicts) =>
        Option.map(
            Array.findFirst(Array.zip(ends, verdicts), ([, matched]) => matched),
            ([end]) => end,
        ),
    );
};
const replaced = (repl: Replace, value: Value, scope: Scope, frame: Frame, origin: Origin): Stepped => {
    const head = repl.orig?.parts[0];
    const anchor = head?.type === 'Lit' ? Option.liftPredicate(head.value.slice(0, 1), (mark) => mark === '#' || mark === '%') : Option.none();
    const orig = Option.map(Option.fromNullishOr(repl.orig), (word) => (Option.isSome(anchor) && head?.type === 'Lit' ? { ...word, parts: [{ ...head, value: head.value.slice(1) }, ...word.parts.slice(1)] } : word));
    const [given, after, refused] = Option.match(orig, { onNone: () => [undefined, scope, []] as const, onSome: (word) => frame.patterned(word, scope, frame) });
    const pattern = Option.fromUndefinedOr(given);
    const [next, chunks] = repl.with === null ? [after, []] : frame.worded(repl.with.parts, after, { ...frame, quoted: false, result: false }, true);
    const put = (match: string) =>
        Option.map(Option.all(chunks.map(Chunk.$match({ Text: ({ text, quoted }) => Option.some(frame.dialect.options.patsub && !quoted ? text.replaceAll('&', match) : text), Break: () => Option.some(' '), Hidden: () => Option.none(), Refuse: () => Option.some('') }))), (texts) => texts.join(''));
    const swap = (field: Field) => (text: string) => {
        const starts = Option.contains(anchor, '#') ? [0] : Array.range(0, text.length);
        const spans = (from: number): Option.Option<readonly (readonly [number, number])[]> =>
            Option.flatMap(
                Option.all(
                    starts
                        .filter((at) => at >= from)
                        .map((at) =>
                            Option.map(
                                longest(text, field, at, Option.contains(anchor, '%')),
                                Option.flatMap((end) => Option.liftPredicate([at, end] as const, () => end > at || Option.isSome(anchor))),
                            ),
                        ),
                ),
                (found) => Option.match(Option.firstSomeOf(found), { onNone: () => Option.some([]), onSome: ([at, end]) => (repl.all && end < text.length ? Option.map(spans(end), (rest) => [[at, end] as const, ...rest]) : Option.some([[at, end] as const])) }),
            );
        return Option.flatMap(spans(0), (found) => Option.map(Option.all(found.map(([at, end]) => put(text.slice(at, end)))), (puts) => found.reduceRight((written, [at, end], index) => `${written.slice(0, at)}${puts[index] ?? ''}${written.slice(end)}`, text)));
    };
    return [Option.match(pattern, { onNone: () => Value.Unknown({ origin, many: many(value) }), onSome: (field) => mapped(value, swap(field), origin) }), next, [...refused, ...chunks.filter(Chunk.$is('Refuse'))]];
};
const removed = (removal: { readonly prefix: boolean; readonly largest: boolean }, text: string, pattern: Field): Option.Option<string> => {
    const ascending = Array.range(0, text.length);
    const piece = (at: number) => (removal.prefix ? text.slice(0, at) : text.slice(at));
    const verdicts = Option.all((removal.prefix === removal.largest ? ascending.toReversed() : ascending).map((at) => Option.map(pattern.matches(piece(at)), (matched) => [at, matched] as const)));
    return Option.map(verdicts, (checked) =>
        Option.match(
            Array.findFirst(checked, ([, matched]) => matched),
            { onNone: () => text, onSome: ([at]) => (removal.prefix ? text.slice(at) : text.slice(0, at)) },
        ),
    );
};
const operated = (exp: ParamExp, value: Value, scope: Scope, frame: Frame, direct: boolean): readonly [Value, Scope, Option.Option<readonly Chunk[]>, readonly Chunk[]] => {
    const name = exp.param?.value ?? '';
    const origin = Origin.Variable({ name });
    const unknown = Value.Unknown({ origin, many: many(value) });
    const empty = (colon: boolean) => Value.$match(value, { Unset: () => Option.some(true), Unknown: () => Option.none(), Scalar: ({ text }) => Option.some(colon && text === ''), List: ({ items }) => Option.some(colon && items.length === 0) });
    const substituted = (word: Word | null): readonly [Value, Scope, Option.Option<readonly Chunk[]>, readonly Chunk[]] => {
        const [next, chunks] = word === null ? [scope, []] : frame.worded(word.parts, scope, { ...frame, result: true }, true);
        return [Value.Scalar({ text: '' }), next, Option.liftPredicate([...(frame.quoted && chunks.length === 0 ? [held('')] : []), ...chunks], () => direct), []];
    };
    const assigned = (word: Word | null): readonly [Value, Scope, Option.Option<readonly Chunk[]>, readonly Chunk[]] => {
        const [text, next, refused] = word === null ? [Option.some(''), scope, []] : valued(word, scope, frame);
        const assignable = identifier.test(name) && exp.index === null && exp.nestedParam === null;
        return [
            Option.match(
                Option.filter(text, () => assignable),
                { onNone: () => Value.Unknown({ origin, many: false }), onSome: (given) => Value.Scalar({ text: given }) },
            ),
            assignable ? next.set(name, Option.match(text, { onNone: () => Binding.Unknown(), onSome: (given) => Binding.Known({ value: given }) })) : next,
            Option.none(),
            refused,
        ];
    };
    return Option.match(Option.fromNullishOr(exp.exp), {
        onNone: () => [value, scope, Option.none(), []] as const,
        onSome: ({ op, word }) =>
            Match.value({ op, empty: Option.getOrUndefined(empty(colons.has(op))), removal: removals.get(op), change: cases.get(op) }).pipe(
                Match.when({ op: Match.is(...errors), empty: Match.undefined }, () => [unknown, scope, Option.none(), [Chunk.Refuse({ origin, certain: false })]] as const),
                Match.when({ op: Match.is(...defaults, ...alternates, ...assigns), empty: Match.undefined }, () => [unknown, scope, Option.none(), []] as const),
                Match.when({ op: Match.is(...defaults), empty: true }, () => substituted(word)),
                Match.when({ op: Match.is(...alternates), empty: true }, () => substituted(null)),
                Match.when({ op: Match.is(...alternates) }, () => substituted(word)),
                Match.when({ op: Match.is(...assigns), empty: true }, () => assigned(word)),
                Match.when({ op: Match.is(...errors), empty: true }, () => [unknown, scope, Option.none(), [refusal(origin)]] as const),
                Match.when({ op: Match.is(...defaults, ...assigns, ...errors) }, () => [value, scope, Option.none(), []] as const),
                Match.when({ removal: Match.defined }, ({ removal }) => {
                    const [pattern, next, refused] = word === null ? [undefined, scope, []] : frame.patterned(word, scope, frame);
                    return [Option.match(Option.fromUndefinedOr(pattern), { onNone: () => value, onSome: (field) => mapped(value, (text) => removed(removal, text, field), origin) }), next, Option.none(), refused] as const;
                }),
                Match.when(
                    { change: Match.defined },
                    ({ change }) => [frame.dialect.supports('casing') && word === null ? mapped(value, (text) => Option.map(Option.liftPredicate(text, ascii), change), origin) : unknown, scope, Option.none(), frame.dialect.supports('casing') ? [] : [refusal(Origin.Unsupported())]] as const,
                ),
                Match.when({ op: ParExpOperator.OtherParamOps }, () => [unknown, scope, Option.none(), frame.dialect.supports('transforms') ? [] : [refusal(Origin.Unsupported())]] as const),
                Match.orElse(() => [unknown, scope, Option.none(), []] as const),
            ),
    });
};
const flagged = (flags: string): Option.Option<readonly { readonly letter: string; readonly argument: string }[]> =>
    Option.all(
        Array.unfold(0, (at) =>
            Option.map(
                Option.liftPredicate(at, (position) => position < flags.length),
                (position) =>
                    Match.value(flags[position] ?? '').pipe(
                        Match.when(
                            (letter: string) => 'ULf@'.includes(letter),
                            (letter) => [Option.some({ letter, argument: '' }), position + 1] as const,
                        ),
                        Match.when(Match.is('s', 'j'), (letter) => {
                            const open = flags[position + 1] ?? '';
                            const finish = flags.indexOf(closing[open] ?? open, position + 2);
                            return finish === -1 ? ([Option.none(), flags.length] as const) : ([Option.some({ letter, argument: flags.slice(position + 2, finish) }), finish + 1] as const);
                        }),
                        Match.orElse(() => [Option.none(), flags.length] as const),
                    ),
            ),
        ),
    );
const flagging = (flags: readonly { readonly letter: string; readonly argument: string }[], value: Value, frame: Frame, origin: Origin): readonly [Value, boolean] => {
    const argument = (letter: string) => Option.fromUndefinedOr(flags.find((flag) => flag.letter === letter)?.argument);
    const spread = flags.some((flag) => flag.letter === '@');
    const joined = Option.match(argument('j'), { onNone: () => value, onSome: (glue) => (value._tag === 'List' ? Value.Scalar({ text: value.items.join(glue) }) : value) });
    const kept = (items: readonly string[]) => (frame.quoted && !spread ? items.filter((item, index) => item !== '' || index === 0 || index === items.length - 1) : items);
    const split = Option.match(
        Option.orElse(Option.as(argument('f'), '\n'), () => argument('s')),
        {
            onNone: () => joined,
            onSome: (by) =>
                Value.$match(joined, {
                    Scalar: ({ text }) => (by === '' ? Value.Unknown({ origin, many: true }) : Value.List({ items: kept(text.split(by)), spread: true })),
                    List: ({ items }) => (by === '' ? Value.Unknown({ origin, many: true }) : Value.List({ items: kept(items.flatMap((item) => item.split(by))), spread: true })),
                    Unset: () => Value.List({ items: [], spread: true }),
                    Unknown: () => Value.Unknown({ origin, many: true }),
                }),
        },
    );
    const change = Option.orElse(
        Option.as(argument('U'), (text: string) => text.toUpperCase()),
        () => Option.as(argument('L'), (text: string) => text.toLowerCase()),
    );
    return [Option.match(change, { onNone: () => split, onSome: (apply) => mapped(split, (text) => Option.map(Option.liftPredicate(text, ascii), apply), origin) }), spread];
};
const rendered = (value: Value, frame: Frame, exp: ParamExp | undefined, scope: Scope, spread: boolean): readonly Chunk[] => {
    const { dialect, quoted, rules } = frame;
    const name = exp?.param?.value ?? '';
    const piece = (text: string) => Chunk.Text({ text, quoted, split: !quoted && splits(frame, exp), glob: !quoted && globs(frame, exp), secret: false });
    const glue = Binding.$match(scope.get('IFS'), { Unset: () => Option.some(' '), Known: ({ value: known }) => Option.some([...known][0] ?? ''), Listed: () => Option.none(), Unknown: () => Option.none() });
    const strict = exp !== undefined && dialect.options.nounset;
    const joined = (items: readonly string[], spaced: boolean) => (spaced ? [piece(items.join(' '))] : [Option.match(glue, { onNone: () => Chunk.Hidden({ origin: Origin.Variable({ name: 'IFS' }), certain: true, many: false }), onSome: (between) => piece(items.join(between)) })]);
    return Value.$match(value, {
        Unset: () => [...(strict ? [refusal(Origin.Variable({ name }))] : []), ...(quoted || rules.single ? [piece('')] : [])],
        Scalar: ({ text }) => [piece(text)],
        List: ({ items, spread: own }) => (rules.single || (quoted && !(own || spread)) ? joined(items, (own || spread) && dialect.variant === LangVariant.LangBash && rules.single) : items.flatMap((item, index) => (index === 0 ? [piece(item)] : [Chunk.Break(), piece(item)]))),
        Unknown: ({ origin, many: plural }) => [...(strict ? [Chunk.Refuse({ origin, certain: false })] : []), Chunk.Hidden({ origin, certain: (quoted || rules.single) && !plural, many: !rules.single && (plural || (!quoted && splits(frame, exp))) })],
    });
};
const parameter = (exp: ParamExp, trailing: readonly string[], scope: Scope, frame: Frame): Parted => {
    const origin = Origin.Variable({ name: exp.param?.value ?? '' });
    const letters = [...exp.modifiers.map(({ value }) => value), ...trailing];
    const flags = flagged(exp.flags?.value ?? '');
    const { length: counting } = exp;
    const direct = letters.length === 0 && Option.exists(flags, Array.isReadonlyArrayEmpty) && !counting && exp.slice === null && exp.repl === null;
    const unmodelled = exp.width || exp.rcExpand === 1 || exp.subscripts.length > 0 || (exp.names !== ParNamesOperator.None && !exp.excl) || Option.isNone(flags);
    const [base, looking] = looked(exp, scope, frame);
    const checked = exp.isSet ? Value.$match(base, { Unset: () => Value.Scalar({ text: '0' }), Unknown: () => base, Scalar: () => Value.Scalar({ text: '1' }), List: () => Value.Scalar({ text: '1' }) }) : base;
    const [operation, operating, chunks, tests] = operated(exp, checked, looking, frame, direct);
    const [cut, cutting, slices] = exp.slice === null ? [operation, operating, []] : sliced(exp.slice, operation, operating, frame, origin);
    const [swapped, swapping, swaps] = exp.repl === null ? [cut, cutting, []] : replaced(exp.repl, cut, cutting, frame, origin);
    const pwd = Binding.$match(swapping.get('PWD'), { Known: ({ value }) => Option.some(value), Unset: () => Option.none(), Listed: () => Option.none(), Unknown: () => Option.none() });
    const changed = letters.reduce((current, letter) => Option.match(Option.fromUndefinedOr(modifiers.get(letter)), { onNone: () => Value.Unknown({ origin, many: many(current) }), onSome: (modifier) => mapped(current, (text) => modifier(text, pwd), origin) }), swapped);
    const measured = counting
        ? Value.$match(changed, {
              Unset: () => Value.Scalar({ text: '0' }),
              Unknown: (unknown) => Value.Unknown({ ...unknown, many: false }),
              Scalar: ({ text }) => (ascii(text) ? Value.Scalar({ text: `${text.length}` }) : Value.Unknown({ origin, many: false })),
              List: ({ items }) => Value.Scalar({ text: `${items.length}` }),
          })
        : changed;
    const [final, spread] = flagging(Option.getOrElse(flags, Array.empty), measured, frame, origin);
    const refusals = [...tests, ...slices, ...swaps];
    return [
        swapping,
        Match.value({ unmodelled, chunks: Option.getOrUndefined(chunks) }).pipe(
            Match.when({ unmodelled: true }, () => [...refusals, ...rendered(Value.Unknown({ origin, many: true }), frame, exp, swapping, false)]),
            Match.when({ chunks: Match.defined }, ({ chunks: given }) => [...refusals, ...given]),
            Match.orElse(() => [...refusals, ...rendered(final, frame, exp, swapping, spread)]),
        ),
    ];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Chunk, type Frame, held, parameter, type Parted, rendered, type Rules, splits, Value, valued };
