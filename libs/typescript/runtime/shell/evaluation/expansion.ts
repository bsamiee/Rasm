import { Array, Data, Glob, HashMap, Match, Option, Redacted } from 'effect';
import { type BraceExp, LangVariant, type Lit, type SglQuoted, type Word, type WordPart } from 'sh-syntax';
import { evaluated } from '#/evaluation/arithmetic';
import { Chunk, type Parted, type Frame, held, parameter, rendered, type Rules, splits, Value, valued } from '#/evaluation/parameter';
import { Binding, Role, Expanded, type Expansion, Field, Matching, Origin, Piece, Refused, Scope, Unmatched } from '#/evaluation/value';
import type { Dialect } from '#/syntax/dialect';

// --- [TYPES] ---------------------------------------------------------------------------

type Token = Data.TaggedEnum<{
    Piece: { readonly text: string; readonly glob: boolean; readonly secret: boolean };
    Mark: object;
    Unseen: { readonly origin: Origin; readonly certain: boolean; readonly many: boolean };
    Space: object;
    Delimiter: object;
    Break: object;
}>;
type Segment = Extract<Token, { readonly _tag: 'Piece' | 'Unseen' }>;
interface Built {
    readonly segments: readonly Segment[];
    readonly certain: boolean;
}
interface Splitting {
    readonly current: Built;
    readonly started: boolean;
    readonly spaced: boolean;
    readonly exact: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const Token = Data.taggedEnum<Token>();
const limit = 10_000;
const spaces = ' \t\n';
const blank: Built = { segments: [], certain: false };
const simple: Readonly<Record<string, string>> = { a: '\u0007', b: '\b', e: '\u001B', E: '\u001B', f: '\f', n: '\n', r: '\r', t: '\t', v: '\v', '\\': '\\', "'": "'", '"': '"', '?': '?' };
const honored = new Set('aAcehlPqQrstu&');
const integral = /^[-+]?\d+$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const rules = (role: Role, dialect: Dialect): Rules => {
    const { braceexpand, posix } = dialect.options;
    const single: Rules = { braces: false, split: false, glob: Option.none(), single: true, tilde: true, colons: false, named: false, heredoc: Option.none() };
    return Role.$match(role, {
        Argument: (): Rules => ({ ...single, braces: braceexpand, split: true, glob: Option.some('Paths'), single: false, named: dialect.variant === LangVariant.LangBash && !posix }),
        Redirect: (): Rules => ({ ...single, braces: braceexpand, split: !posix, glob: posix ? Option.none() : Option.some('Paths'), single: false }),
        Assignment: () => ({ ...single, colons: true }),
        Pattern: (): Rules => ({ ...single, glob: Option.some('Strings') }),
        Operand: () => single,
        Heredoc: ({ expands }) => ({ ...single, tilde: false, heredoc: Option.some(expands) }),
    });
};
const home = (user: Option.Option<string>, zsh: boolean) => [...(zsh && Option.isSome(user) ? [Chunk.Refuse({ origin: Origin.Home({ user }), certain: false })] : []), Chunk.Hidden({ origin: Origin.Home({ user }), certain: true, many: false })];
const numbered = (width: number) => (value: number) => (value < 0 ? `-${`${-value}`.padStart(Math.max(width - 1, 0), '0')}` : `${value}`.padStart(width, '0'));
const lettered = (zsh: boolean) => (code: number) => (code === 0x5c && !zsh ? '' : String.fromCodePoint(code));
const opening = (part: WordPart) =>
    Match.value(part).pipe(
        Match.when({ type: 'SglQuoted', dollar: false }, ({ value }) => value),
        Match.when({ type: 'DblQuoted' }, ({ parts }) => parts.map((inner) => (inner.type === 'Lit' ? inner.value : '\u0000')).join('')),
        Match.orElse(() => ''),
    );
const byte = (value: number) => `%${value.toString(16).padStart(2, '0')}`;
const point = (value: number) => Option.getOrElse(Option.liftThrowable(() => encodeURIComponent(String.fromCodePoint(value)))(), () => '%');
const control = (character: string) => byte(character === '?' ? 0x7f : (character.codePointAt(0) ?? 0) & 0x1f);
const decoded = (raw: string, dialect: Dialect): Option.Option<string> => {
    const zsh = dialect.variant === LangVariant.LangZsh;
    const wide = dialect.supports('unicode');
    const pieces = Array.fromIterable(raw.matchAll(/\\(?:c(?<caret>[\s\S])|C-(?<chord>[\s\S])|M-(?<meta>[\s\S])|x(?<hex>[\da-fA-F]{1,2})|(?<escape>[uU])(?<code>[\da-fA-F]+)|(?<octal>[0-7]{1,3})|(?<other>[\s\S]))|(?<plain>[^\\]+)|\\$/gu)).map((found) => {
        const groups: { readonly caret?: string; readonly chord?: string; readonly meta?: string; readonly hex?: string; readonly escape?: string; readonly code?: string; readonly octal?: string; readonly other?: string; readonly plain?: string } = found.groups ?? {};
        return Match.value(groups).pipe(
            Match.when({ plain: Match.string }, ({ plain }) => encodeURIComponent(plain)),
            Match.when({ caret: Match.string }, ({ caret }) => (zsh ? encodeURIComponent(`c${caret}`) : control(caret))),
            Match.when({ chord: Match.string }, ({ chord }) => (zsh ? control(chord) : encodeURIComponent(`\\C-${chord}`))),
            Match.when({ meta: Match.string }, ({ meta }) => (zsh ? byte(0x80 + ((meta.codePointAt(0) ?? 0) & 0x7f)) : encodeURIComponent(`\\M-${meta}`))),
            Match.when({ hex: Match.string }, ({ hex }) => byte(Number.parseInt(hex, 16))),
            Match.when({ octal: Match.string }, ({ octal }) => byte(Number.parseInt(octal, 8) & 0xff)),
            Match.when({ escape: Match.string, code: Match.string }, ({ escape, code }) => {
                const digits = code.slice(0, escape === 'u' ? 4 : 8);
                return wide ? `${point(Number.parseInt(digits, 16))}${encodeURIComponent(code.slice(digits.length))}` : encodeURIComponent(`\\${escape}${code}`);
            }),
            Match.when({ other: Match.string }, ({ other }) => encodeURIComponent(simple[other] ?? (zsh ? other : `\\${other}`))),
            Match.orElse(() => encodeURIComponent('\\')),
        );
    });
    const joined = pieces.join('');
    return Option.liftThrowable(decodeURIComponent)(zsh || !joined.includes('%00') ? joined : joined.slice(0, joined.indexOf('%00')));
};
const bare = (text: string, frame: Frame, secret: boolean): readonly Chunk[] =>
    Array.fromIterable(text.matchAll(/\\(?<escaped>[\s\S])|(?<plain>[^\\]+)|\\$/gu)).flatMap((found) => {
        const groups: { readonly escaped?: string; readonly plain?: string } = found.groups ?? {};
        return Match.value(groups).pipe(
            Match.when({ plain: Match.string }, ({ plain }) => [Chunk.Text({ text: plain, quoted: false, split: frame.result && splits(frame), glob: Option.isSome(frame.rules.glob), secret })]),
            Match.when({ escaped: '\n' }, () => []),
            Match.orElse(({ escaped = '\\' }) => [held(escaped, secret)]),
        );
    });
const stepped = (from: number, to: number, by: number, zsh: boolean): Option.Option<readonly number[]> => {
    const stride = Math.abs(by) || 1;
    const count = Math.floor(Math.abs(to - from) / stride) + 1;
    const values = Array.makeBy(Math.min(count, limit), (index) => from + Math.sign(to - from || 1) * index * stride);
    return count > limit || ![from, to, by].every(Number.isSafeInteger) ? Option.none() : Option.some(zsh && by < 0 ? values.toReversed() : values);
};
const sequenced = (brace: BraceExp, dialect: Dialect): Option.Option<readonly string[]> =>
    Option.flatMap(Option.all(brace.elems.map((elem) => (elem.parts.length === 1 && elem.parts[0]?.type === 'Lit' ? Option.some(elem.parts[0].value) : Option.none()))), ([start = '', end = '', step]) => {
        const zsh = dialect.variant === LangVariant.LangZsh;
        const width = dialect.supports('padding') && [start, end].some((text) => /^[-+]?0\d/u.test(text)) ? Math.max(start.length, end.length) : 0;
        return Match.value({ start, end, step }).pipe(
            Match.when(
                (ends) => ends.step !== undefined && !dialect.supports('steps'),
                () => Option.some([`{${[start, end, step].join('..')}}`]),
            ),
            Match.when(
                (ends) => ends.step === '0' && zsh,
                () => Option.none(),
            ),
            Match.when(
                (ends) => integral.test(ends.start) && integral.test(ends.end) && (ends.step === undefined || integral.test(ends.step)),
                () => Option.map(stepped(Number(start), Number(end), Number(step ?? '1'), zsh), (values) => values.map(numbered(width))),
            ),
            Match.when(
                (ends) => [...ends.start].length === 1 && [...ends.end].length === 1 && (ends.step === undefined || (!zsh && integral.test(ends.step))),
                () => Option.map(stepped(start.codePointAt(0) ?? 0, end.codePointAt(0) ?? 0, Number(step ?? '1'), zsh), (points) => points.map(lettered(zsh))),
            ),
            Match.orElse(() => Option.some([`{${[start, end, ...Option.toArray(Option.fromUndefinedOr(step))].join('..')}}`])),
        );
    });
const choices = (part: WordPart, dialect: Dialect): Option.Option<readonly (readonly WordPart[])[]> =>
    Match.value(part).pipe(
        Match.when({ type: 'BraceExp', sequence: true }, (brace) => Option.map(sequenced(brace, dialect), (values) => values.map((value): readonly SglQuoted[] => [{ type: 'SglQuoted', start: brace.start, end: brace.end, left: brace.lbrace, right: brace.rbrace, dollar: false, value }]))),
        Match.when({ type: 'BraceExp' }, (brace) => Option.map(Option.all(brace.elems.map((elem) => alternated(elem.parts, dialect))), (found) => found.flat())),
        Match.when(
            (candidate: WordPart) => candidate.type === 'Lit' && dialect.options.braceccl && /\{[^{},]+\}/u.test(candidate.value),
            () => Option.none(),
        ),
        Match.orElse((plain) => Option.some([[plain]])),
    );
const alternated = (parts: readonly WordPart[], dialect: Dialect): Option.Option<readonly (readonly WordPart[])[]> =>
    parts.reduce<Option.Option<readonly (readonly WordPart[])[]>>(
        (prefixes, part) => Option.flatMap(Option.all([prefixes, choices(part, dialect)]), ([heads, tails]) => (heads.length * tails.length > limit ? Option.none() : Option.some(Array.cartesianWith(heads, tails, (head: readonly WordPart[], tail: readonly WordPart[]) => Array.appendAll(head, tail))))),
        Option.some([[]]),
    );
const tilded = (lit: Lit, next: Option.Option<WordPart>, scope: Scope, frame: Frame, colons: boolean): Option.Option<readonly [readonly Chunk[], string]> => {
    const zsh = frame.dialect.variant === LangVariant.LangZsh;
    const known = (name: string, unset: () => readonly Chunk[]) =>
        Binding.$match(scope.get(name), { Known: ({ value }) => [held(value)], Unset: unset, Listed: () => [Chunk.Hidden({ origin: Origin.Variable({ name }), certain: true, many: false })], Unknown: () => [Chunk.Hidden({ origin: Origin.Variable({ name }), certain: true, many: false })] });
    const slashed = Option.exists(Option.map(next, opening), (text) => text.startsWith('/'));
    return Option.flatMap(Option.fromNullishOr((colons ? /^~(?<user>[^/\\:]*)(?<slash>[/:]?)/u : /^~(?<user>[^/\\]*)(?<slash>\/?)/u).exec(lit.value)), (found) => {
        const { user = '', slash = '' } = found.groups ?? {};
        const rest = lit.value.slice(found[0].length - slash.length);
        const literal = () => [held(`~${user}`)];
        return Match.value({ open: slash === '' && rest === '' && Option.isSome(next), zsh, slashed }).pipe(
            Match.when({ open: true, zsh: false }, () => Option.none()),
            Match.when({ open: true, slashed: false }, () => Option.some([home(Option.some(user), zsh), rest] as const)),
            Match.orElse(() =>
                Option.some([
                    Match.value(user).pipe(
                        Match.when('', () => Binding.$match(scope.get('HOME'), { Known: ({ value }) => [held(value)], Unset: () => home(Option.none(), zsh), Listed: () => home(Option.none(), zsh), Unknown: () => home(Option.none(), zsh) })),
                        Match.when('+', () => known('PWD', literal)),
                        Match.when('-', () => known('OLDPWD', literal)),
                        Match.when((name: string) => /^[+-](?!\d+$)/u.test(name), literal),
                        Match.orElse((name) => home(Option.some(name), zsh)),
                    ),
                    rest,
                ] as const),
            ),
        );
    });
};
const parted = (part: WordPart, trailing: readonly string[], scope: Scope, frame: Frame): Parted => {
    const secret = frame.secret(part);
    return Match.value(part).pipe(
        Match.discriminatorsExhaustive('type')({
            Lit: ({ value }) =>
                [
                    scope,
                    Option.match(frame.rules.heredoc, {
                        onSome: (expands) => [held(expands ? value.replaceAll(/\\(?<escaped>[$`\\\n])/gu, (_, escaped: string) => (escaped === '\n' ? '' : escaped)) : value, secret)],
                        onNone: () =>
                            frame.quoted
                                ? [
                                      held(
                                          value.replaceAll(/\\(?<escaped>[$`"\\\n])/gu, (_, escaped: string) => (escaped === '\n' ? '' : escaped)),
                                          secret,
                                      ),
                                  ]
                                : bare(value, frame, secret),
                    }),
                ] as const,
            SglQuoted: ({ value, dollar }) =>
                [scope, [frame.quoted ? held(`${dollar ? '$' : ''}'${value}'`, secret) : Option.match(dollar ? decoded(value, frame.dialect) : Option.some(value), { onNone: () => Chunk.Hidden({ origin: Origin.Bytes(), certain: true, many: false }), onSome: (text) => held(text, secret) })]] as const,
            DblQuoted: ({ parts }) => (parts.length === 0 ? ([scope, [held('')]] as const) : worded(parts, scope, { ...frame, quoted: true }, false)),
            ParamExp: (exp) => parameter(exp, trailing, scope, frame),
            CmdSubst: () => [scope, [Chunk.Hidden({ origin: Origin.Substitution(), certain: frame.quoted || frame.rules.single, many: !frame.quoted && splits(frame) })]] as const,
            ProcSubst: () => [scope, [Chunk.Hidden({ origin: Origin.Substitution(), certain: true, many: false })]] as const,
            ArithmExp: ({ x }) => {
                const [result, next] = evaluated(x, scope, frame.dialect, (word, from) => valued(word, from, frame));
                return [
                    next,
                    Match.valueTags(result, {
                        Known: ({ value }) => rendered(Value.Scalar({ text: `${value}` }), frame, undefined, next, false),
                        Unknown: () => [Chunk.Refuse({ origin: Origin.Arithmetic(), certain: false }), Chunk.Hidden({ origin: Origin.Arithmetic(), certain: frame.quoted || frame.rules.single, many: !frame.quoted && splits(frame) })],
                        Refused: () => [Chunk.Refuse({ origin: Origin.Arithmetic(), certain: true })],
                    }),
                ] as const;
            },
            ExtGlob: ({ pattern }) => [scope, [Chunk.Hidden({ origin: Origin.Glob({ text: pattern?.value ?? '' }), certain: false, many: Option.contains(frame.rules.glob, 'Paths') })]] as const,
            BraceExp: () => [scope, [Chunk.Hidden({ origin: Origin.Braces(), certain: false, many: true })]] as const,
        }),
    );
};
const trailed = (parts: readonly WordPart[], at: number, zsh: boolean): readonly string[] => {
    const [part, next] = [parts[at], parts[at + 1]];
    const run = zsh && part?.type === 'ParamExp' && part.short && next?.type === 'Lit' ? Array.fromIterable(next.value.matchAll(/:(?<letter>[a-zA-Z&])/guy)).map((found) => found.groups?.['letter'] ?? '') : [];
    return Array.takeWhile(run, (letter) => honored.has(letter));
};
const worded = (parts: readonly WordPart[], scope: Scope, frame: Frame, start: boolean): Parted => {
    const zsh = frame.dialect.variant === LangVariant.LangZsh;
    const head = Option.filter(Option.fromUndefinedOr(parts[0]), (part): part is Lit => start && !frame.quoted && part.type === 'Lit');
    const colons = !frame.quoted && (frame.rules.colons || (frame.rules.named && Option.exists(head, ({ value }) => /^[A-Za-z_]\w*=/u.test(value))));
    const items: readonly WordPart[] = colons ? parts.flatMap((part): readonly WordPart[] => (part.type === 'Lit' ? part.value.split(/(?<=:|^[A-Za-z_]\w*=)(?=~)/u).map((value) => ({ ...part, value })) : [part])) : parts;
    const command = Option.filter(
        Option.map(head, ({ value }) => value),
        (value) => !frame.result && !frame.rules.single && frame.dialect.options.equals && /^=[^/=]/u.test(value),
    );
    const eligible = (at: number) => frame.rules.tilde && !frame.quoted && ((at === 0 && start) || (colons && Option.exists(Array.get(items, at - 1), (previous) => previous.type === 'Lit' && /(?::|^[A-Za-z_]\w*=)$/u.test(previous.value))));
    const [next, chunks] = Array.mapAccum(items, scope, (from, part, at) => {
        const consumed = at > 0 ? trailed(items, at - 1, zsh).length * 2 : 0;
        const kept = part.type === 'Lit' ? { ...part, value: part.value.slice(consumed) } : part;
        const tilde = kept.type === 'Lit' && eligible(at) ? Option.map(tilded(kept, Array.get(items, at + 1), scope, frame, colons), ([found, rest]) => [found, { ...kept, value: rest }] as const) : Option.none();
        const [lead, rest] = Option.getOrElse(tilde, () => [[], kept] as const);
        const [to, found] = rest.type === 'Lit' && rest.value === '' ? [from, []] : parted(rest, trailed(items, at, zsh), from, frame);
        return [to, [...lead, ...found]] as const;
    });
    return Option.match(command, {
        onSome: (name) => [scope, [Chunk.Refuse({ origin: Origin.Command({ name: name.slice(1) }), certain: false }), Chunk.Hidden({ origin: Origin.Command({ name: name.slice(1) }), certain: true, many: false })]] as const,
        onNone: () => [next, chunks.flat()] as const,
    });
};
const tokens = (chunk: Chunk, ifs: Option.Option<string>): readonly Token[] =>
    Chunk.$match(chunk, {
        Break: () => [Token.Break()],
        Refuse: () => [],
        Hidden: (hidden) => [Token.Unseen(hidden)],
        Text: ({ text, quoted, split, glob, secret }) =>
            Match.value({ split, empty: text === '', ifs: Option.getOrUndefined(ifs) }).pipe(
                Match.when({ split: false, empty: true }, () => (quoted ? [Token.Mark()] : [])),
                Match.when({ split: false }, () => [Token.Piece({ text, glob, secret })]),
                Match.when({ empty: true }, () => []),
                Match.when({ ifs: Match.undefined }, () => [Token.Unseen({ origin: Origin.Variable({ name: 'IFS' }), certain: false, many: true })]),
                Match.orElse(({ ifs: by = spaces }) => {
                    const [white, hard] = [true, false].map((whitespace) =>
                        [...by]
                            .filter((character) => spaces.includes(character) === whitespace)
                            .map(RegExp.escape)
                            .join(''),
                    );
                    return Array.fromIterable(text.matchAll(new RegExp(`(?<space>[${white}]+)|(?<delimiter>[${hard}])|(?<piece>[^${white}${hard}]+)`, 'gu'))).map((found) =>
                        Match.value(found.groups ?? {}).pipe(
                            Match.when({ space: Match.string }, () => Token.Space()),
                            Match.when({ delimiter: Match.string }, () => Token.Delimiter()),
                            Match.orElse(() => Token.Piece({ text: found[0], glob, secret })),
                        ),
                    );
                }),
            ),
    });
const fielded = (chunks: readonly Chunk[], ifs: Option.Option<string>, nulls: boolean): { readonly fields: readonly Built[]; readonly exact: boolean } => {
    const reset = (state: Splitting, spaced: boolean): Splitting => ({ ...state, current: blank, started: false, spaced });
    const extended = (state: Splitting, segment: Segment, certain: boolean, exact: boolean): Splitting => ({ current: { segments: [...state.current.segments, segment], certain: state.current.certain || certain }, started: true, spaced: false, exact });
    const [final, emitted] = Array.mapAccum(
        chunks.flatMap((chunk) => tokens(chunk, ifs)),
        { current: blank, started: false, spaced: false, exact: true } as Splitting,
        (state, token) =>
            Token.$match(token, {
                Piece: (piece) => [extended(state, piece, true, state.exact), Option.none<Built>()] as const,
                Mark: () => [state.spaced && !nulls ? state : { ...state, current: { ...state.current, certain: true }, started: true, spaced: false }, Option.none<Built>()] as const,
                Unseen: (unseen) => [extended(state, unseen, unseen.certain, state.exact && !unseen.many), Option.none<Built>()] as const,
                Space: () => (state.started ? ([reset({ ...state, exact: state.exact && state.current.certain }, true), Option.some(state.current)] as const) : ([state, Option.none<Built>()] as const)),
                Delimiter: () => (state.spaced ? ([{ ...state, spaced: false }, Option.none<Built>()] as const) : ([reset(state, false), Option.some<Built>({ ...state.current, certain: true })] as const)),
                Break: () => (state.started ? ([reset({ ...state, exact: state.exact && state.current.certain }, false), Option.some(state.current)] as const) : ([{ ...state, spaced: false }, Option.none<Built>()] as const)),
            }),
    );
    return { fields: [...Array.getSomes(emitted), ...(final.started ? [final.current] : [])], exact: final.exact && (!final.started || final.current.certain) };
};
const unmatching = (dialect: Dialect): Unmatched =>
    Match.value({ zsh: dialect.variant === LangVariant.LangZsh, dropped: dialect.options.nullglob, refused: dialect.options.nomatch }).pipe(
        Match.when({ zsh: false, refused: true }, () => Unmatched.Refused()),
        Match.when({ dropped: true }, () => Unmatched.Dropped()),
        Match.when({ refused: true }, () => Unmatched.Refused()),
        Match.orElse(() => Unmatched.Kept()),
    );
const settled = (built: Built, frame: Frame): Field => {
    const { dialect } = frame;
    const zsh = dialect.variant === LangVariant.LangZsh;
    const active = built.segments.flatMap((segment) => (segment._tag === 'Piece' && segment.glob ? [segment.text] : [])).join('');
    const paths = Option.contains(frame.rules.glob, 'Paths');
    const globbed = Option.isSome(frame.rules.glob) && (!paths || dialect.options.glob) && (zsh ? /[*?(]|\[.+\]/su : /[*?]|\[.+\]/su).test(active);
    const unmodelled = (zsh ? /[()|]|\*{3}/u.test(active) || (dialect.options.extendedglob && /[\^#~]/u.test(active)) : false) || dialect.options.nocaseglob || (paths && !dialect.options.globskipdots && /(?:^|\/)\./u.test(active));
    const pieces = built.segments.map((segment) =>
        Match.value(segment).pipe(
            Match.when({ _tag: 'Unseen' }, ({ origin }) => Piece.Unknown({ origin })),
            Match.when({ secret: true }, ({ text }) => Piece.Secret({ text: Redacted.make(text) })),
            Match.when({ glob: true }, ({ text }) => (globbed ? Piece.Wild({ text }) : Piece.Text({ text }))),
            Match.orElse(({ text }) => Piece.Text({ text })),
        ),
    );
    const pattern = Option.map(
        Option.liftPredicate(pieces, (all) => !unmodelled && all.every((piece) => piece._tag === 'Text' || piece._tag === 'Wild')),
        (all) => all.map((piece) => (piece._tag === 'Wild' ? piece.text.replaceAll(/[{}]/gu, String.raw`\$&`) : Glob.escape(piece._tag === 'Text' ? piece.text : ''))).join(''),
    );
    const folded = Option.map(pattern, (text) => (zsh || !dialect.options.globstar ? text.replaceAll(zsh ? /\*{2,}(?!\/)/gu : /\*{2,}/gu, '*') : text));
    return new Field({
        pieces,
        matching: Option.liftPredicate(paths ? Matching.Paths({ glob: Option.map(folded, (text) => Glob.make(text, { dot: dialect.options.dotglob })), unmatched: unmatching(dialect) }) : Matching.Strings({ glob: Option.map(pattern, Field.strings) }), () => globbed),
    });
};
const whole = (chunks: readonly Chunk[]): Built => ({ segments: chunks.flatMap((chunk) => tokens(chunk, Option.none())).filter((token): token is Segment => token._tag === 'Piece' || token._tag === 'Unseen'), certain: true });
const patterned = (word: Word, scope: Scope, frame: Frame): readonly [Field, Scope, readonly Chunk[]] => {
    const within = { ...frame, rules: rules(Role.Pattern(), frame.dialect), quoted: false, result: false };
    const [next, chunks] = worded(word.parts, scope, within, true);
    return [settled(whole(chunks), within), next, chunks.filter(Chunk.$is('Refuse'))];
};
const expanded = (word: Word, scope: Scope, dialect: Dialect, at: { readonly braces: Option.Option<Word>; readonly role: Role; readonly secret: (part: WordPart) => boolean }): { readonly expansion: Expansion; readonly scope: Scope } => {
    const zsh = dialect.variant === LangVariant.LangZsh;
    const given = rules(at.role, dialect);
    const frame: Frame = { dialect, rules: given, quoted: Option.isSome(given.heredoc), result: false, secret: at.secret, worded, patterned };
    const kept = zsh && Option.exists(at.braces, (split) => split.parts.some((part) => part.type === 'BraceExp'));
    const opaque = { fields: [Field.unknown(Origin.Braces())], exact: false };
    return Option.match(frame.rules.braces ? alternated(Option.getOrElse(at.braces, () => word).parts, dialect) : Option.some([word.parts]), {
        onNone: () => ({ expansion: new Expanded({ ...opaque, refusals: [] }), scope }),
        onSome: (alternatives) => {
            const [next, words] = Array.mapAccum(alternatives, scope, (from, parts) => {
                const [to, chunks] = worded(parts, from, frame, true);
                const split = frame.rules.single ? { fields: [whole(chunks)], exact: true } : fielded(chunks, Binding.$match(to.get('IFS'), { Unset: () => Option.some(spaces), Known: ({ value }) => Option.some(value), Listed: () => Option.none(), Unknown: () => Option.none() }), dialect.supports('nulls'));
                return [to, { ...split, fields: kept && split.fields.length === 0 ? [{ ...blank, certain: true }] : split.fields, listed: chunks.some(Chunk.$is('Break')), refusals: chunks.filter(Chunk.$is('Refuse')) }] as const;
            });
            const fields = words.flatMap((found) => found.fields.map((built) => settled(built, frame)));
            const exact = words.every((found) => found.exact) && fields.every((field) => !Option.exists(field.matching, Matching.$is('Paths')));
            const refusals = [
                ...words.flatMap((found) => found.refusals),
                ...fields.flatMap((field) =>
                    Option.exists(field.matching, (matching) => matching._tag === 'Paths' && matching.unmatched._tag === 'Refused') ? [Chunk.Refuse({ origin: Origin.Glob({ text: field.pieces.map((piece) => (piece._tag === 'Text' || piece._tag === 'Wild' ? piece.text : '')).join('') }), certain: false })] : [],
                ),
            ];
            const ambiguous = Role.$is('Redirect')(at.role) && !(zsh && dialect.options.multios);
            const certain = Option.orElse(
                Option.map(
                    Array.findFirst(refusals, (refusal) => refusal.certain),
                    ({ origin }) => origin,
                ),
                () => Option.liftPredicate(Origin.Ambiguous(), () => ambiguous && exact && fields.length !== 1),
            );
            return {
                expansion: Option.match(certain, {
                    onSome: (origin) => new Refused({ origin }),
                    onNone: () => new Expanded({ ...(kept && words.some((found) => found.listed) ? opaque : { fields, exact }), refusals: [...refusals.map(({ origin }) => origin), ...(ambiguous && !exact ? [Origin.Ambiguous()] : [])] }),
                }),
                scope: next,
            };
        },
    });
};
const text = (word: Word, dialect: Dialect): Option.Option<string> => {
    const { expansion } = expanded(word, new Scope({ variables: HashMap.empty(), unlisted: Binding.Unset(), positional: Option.some([]), name: dialect.binary }), dialect, { braces: Option.none(), role: Role.Argument(), secret: () => false });
    return expansion._tag === 'Expanded' ? expansion.text : Option.none();
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { expanded, text };
