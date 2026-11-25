import { Array, Data, HashSet, Match, Option, String } from 'effect';
import { Integer } from '#/evaluation/arithmetic';
import { Binding, Field, Origin, type Scope } from '#/evaluation/value';

// --- [TYPES] ---------------------------------------------------------------------------

type Content = Data.TaggedEnum<{
    Unset: object;
    Scalar: { readonly text: string };
    List: { readonly items: readonly string[]; readonly spread: boolean };
    Unknown: { readonly origin: Origin; readonly many: boolean };
}>;
type Change = (text: string) => Option.Option<string>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const Content = Data.taggedEnum<Content>();
const identifier = /^[A-Za-z_]\w*$/u;
const closing: Readonly<Record<string, string>> = { '(': ')', '[': ']', '{': '}', '<': '>' };
const ascii = Option.liftPredicate((text: string) => /^\p{ASCII}*$/u.test(text));
const extension = (text: string) => Option.liftPredicate(text.lastIndexOf('.'), (dot) => dot > text.lastIndexOf('/'));
const modifiers: Readonly<Record<string, (text: string, pwd: Option.Option<string>) => Option.Option<string>>> = {
    t: (text) => Option.some(Array.lastNonEmpty(String.split(text.replace(/\/+$/u, ''), '/'))),
    h: (text) => {
        const path = text.replace(/(?<=.)\/+$/u, '');
        const head = path.slice(0, path.lastIndexOf('/') + 1).replace(/(?<=.)\/+$/u, '');
        return Option.some(head === '' ? '.' : head);
    },
    r: (text) => Option.some(Option.match(extension(text), { onNone: () => text, onSome: (dot) => text.slice(0, dot) })),
    e: (text) => Option.some(Option.match(extension(text), { onNone: () => '', onSome: (dot) => text.slice(dot + 1) })),
    l: (text) => Option.map(ascii(text), String.toLowerCase),
    u: (text) => Option.map(ascii(text), String.toUpperCase),
    a: (text, pwd) => Option.flatMap(text.startsWith('/') || text === '' ? Option.some('/') : pwd, (base) => Field.known(text).resolve(Field.known(base)).text),
};
const first = <A>(candidates: readonly A[], decide: (candidate: A) => Option.Option<boolean>): Option.Option<Option.Option<A>> => Option.map(Option.all(candidates.map(decide)), (verdicts) => Array.findFirst(candidates, (_, at) => verdicts[at] === true));

// --- [MODELS] --------------------------------------------------------------------------

class Value extends Data.Class<{ readonly origin: Origin; readonly content: Content; readonly refusals: readonly { readonly origin: Origin; readonly certain: boolean }[] }> {
    // --- [READING]

    get many(): boolean {
        return this.content._tag === 'List' || (this.content._tag === 'Unknown' && this.content.many);
    }

    get named(): boolean {
        return this.origin._tag === 'Variable' && identifier.test(this.origin.name);
    }

    empty(colon: boolean): Option.Option<boolean> {
        return Content.$match(this.content, { Unset: () => Option.some(true), Unknown: () => Option.none(), Scalar: ({ text }) => Option.some(colon && text === ''), List: ({ items }) => Option.some(colon && items.length === 0) });
    }

    // --- [HOLDING]

    hold(content: Content): Value {
        return new Value({ ...this, content });
    }

    obscure(many = this.many, origin = this.origin): Value {
        return this.hold(Content.Unknown({ origin, many }));
    }

    refuse(...refusals: Value['refusals']): Value {
        return new Value({ ...this, refusals: [...this.refusals, ...refusals] });
    }

    map(change: Change): Value {
        return Content.$match(this.content, {
            Unset: (): Value => this,
            Unknown: (): Value => this,
            Scalar: ({ text }) => Option.match(change(text), { onNone: () => this.obscure(false), onSome: (changed) => this.hold(Content.Scalar({ text: changed })) }),
            List: ({ items, spread }) => Option.match(Option.all(items.map(change)), { onNone: () => this.obscure(true), onSome: (changed) => this.hold(Content.List({ items: changed, spread })) }),
        });
    }

    // --- [SELECTING]

    refer(scope: Scope): Value {
        return this.content._tag === 'Scalar' && /^(?:[A-Za-z_]\w*|\d+|[@*#])$/u.test(this.content.text) ? this.hold(Value.read(this.content.text, scope).content) : this.obscure(false);
    }

    first(): Value {
        return this.content._tag === 'List' ? this.hold(Option.match(Array.head(this.content.items), { onNone: () => Content.Unset(), onSome: (text) => Content.Scalar({ text }) })) : this;
    }

    spread(spread: boolean): Value {
        return Content.$match(this.content, {
            Unset: () => this.hold(Content.List({ items: [], spread })),
            Scalar: ({ text }) => this.hold(Content.List({ items: [text], spread })),
            List: ({ items }) => this.hold(Content.List({ items, spread })),
            Unknown: () => this.obscure(true),
        });
    }

    index(position: Integer, zsh: boolean, base: number): Value {
        const element = (items: readonly string[]) => (position._tag === 'Known' && (position.value !== 0n || base === 0) ? this.hold(Content.Scalar({ text: items.at(Number(position.value) - (position.value < 0n ? 0 : base)) ?? '' })) : this.obscure(false));
        return Match.value({ content: this.content, zsh }).pipe(
            Match.when({ content: { _tag: 'List' }, zsh: true }, ({ content }) => element(content.items)),
            Match.when({ content: { _tag: 'Scalar' }, zsh: true }, ({ content }) => Option.match(ascii(content.text), { onNone: () => this.obscure(false), onSome: (text) => element([...text]) })),
            Match.when({ content: { _tag: 'Scalar' } }, () => Integer.$match(position, { Known: ({ value }) => (value === 0n ? this : this.hold(Content.Unset())), Unknown: () => this.obscure(false), Refused: () => this.obscure(false) })),
            Match.when({ content: { _tag: 'List' } }, ({ content }) => (this.named ? element(content.items) : this.obscure(false))),
            Match.orElse((): Value => this),
        );
    }

    slice(bounds: readonly Integer[], negative: boolean, zero: string): Value {
        const [from = 0n, count] = bounds.map((bound) => (bound._tag === 'Known' ? bound.value : 0n));
        const unknown = this.obscure(this.many, Origin.Arithmetic());
        const cut = <A>(items: readonly A[]) => {
            const begin = Number(from) + (from < 0n ? items.length : 0);
            const end = Option.match(Option.fromUndefinedOr(count), { onNone: () => items.length, onSome: (taken) => (taken < 0n ? items.length : begin) + Number(taken) });
            return begin < 0 || begin > items.length ? Option.some<readonly A[]>([]) : Option.liftPredicate(items.slice(begin, end), () => end >= begin);
        };
        return Match.value({ refused: bounds.some(Integer.$is('Refused')) || (count !== undefined && count < 0n && !(negative && this.content._tag !== 'List')), known: bounds.every(Integer.$is('Known')) }).pipe(
            Match.when({ refused: true }, () => unknown.refuse({ origin: Origin.Arithmetic(), certain: true })),
            Match.when({ known: false }, () => unknown.refuse({ origin: Origin.Arithmetic(), certain: false })),
            Match.orElse(() =>
                Content.$match(this.content, {
                    Unset: (): Value => this,
                    Unknown: (): Value => this,
                    Scalar: ({ text }) => Option.match(ascii(text), { onNone: () => this.obscure(false), onSome: (plain) => Option.match(cut([...plain]), { onNone: () => unknown, onSome: (kept) => this.hold(Content.Scalar({ text: kept.join('') })) }) }),
                    List: ({ items, spread }) => Option.match(cut(this.named ? items : [zero, ...items]), { onNone: () => unknown, onSome: (kept) => this.hold(Content.List({ items: kept, spread })) }),
                }),
            ),
        );
    }

    exists(): Value {
        return Content.$match(this.content, { Unset: () => this.hold(Content.Scalar({ text: '0' })), Unknown: (): Value => this, Scalar: () => this.hold(Content.Scalar({ text: '1' })), List: () => this.hold(Content.Scalar({ text: '1' })) });
    }

    measure(): Value {
        return Content.$match(this.content, {
            Unset: () => this.hold(Content.Scalar({ text: '0' })),
            Unknown: ({ origin }) => this.obscure(false, origin),
            Scalar: ({ text }) => this.map(() => Option.map(ascii(text), (plain) => `${plain.length}`)),
            List: ({ items }) => this.hold(Content.Scalar({ text: `${items.length}` })),
        });
    }

    // --- [REWRITING]

    remove(pattern: Field, prefix: boolean, largest: boolean): Value {
        return this.map((text) => {
            const cuts = Array.range(0, text.length);
            const found = first(prefix === largest ? cuts.toReversed() : cuts, (at) => pattern.matches(prefix ? text.slice(0, at) : text.slice(at)));
            return Option.map(found, Option.match({ onNone: () => text, onSome: (at) => (prefix ? text.slice(at) : text.slice(0, at)) }));
        });
    }

    replace(pattern: Field, anchor: Option.Option<string>, all: boolean, put: Change): Value {
        return this.map((text) => {
            const starts = Option.contains(anchor, '#') ? [0] : Array.range(0, text.length);
            const ends = (at: number) => (Option.contains(anchor, '%') ? [text.length] : Array.range(at, text.length).toReversed());
            const longest = Option.all(starts.map((at) => first(ends(at), (end) => pattern.matches(text.slice(at, end)))));
            const found = Array.zip(starts, Option.getOrElse(longest, Array.empty<Option.Option<number>>));
            const matches = found.flatMap(([at, end]) => Option.toArray(Option.filter(end, (to) => to > at || Option.isSome(anchor))).map((to) => [at, to] as const));
            const spans = Array.unfold(0, (from) =>
                Option.map(
                    Array.findFirst(matches, ([at]) => at >= from),
                    (span) => [span, all && span[1] < text.length ? Math.max(span[1], span[0] + 1) : text.length + 1] as const,
                ),
            );
            const puts = Option.filter(Option.all(spans.map(([at, end]) => put(text.slice(at, end)))), () => Option.isSome(longest));
            return Option.map(puts, (written) => spans.reduceRight((whole, [at, end], index) => `${whole.slice(0, at)}${written[index] ?? ''}${whole.slice(end)}`, text));
        });
    }

    recase(change: (text: string) => string): Value {
        return this.map((text) => Option.map(ascii(text), change));
    }

    modify(letter: string, pwd: Option.Option<string>): Value {
        return Option.match(Option.fromUndefinedOr(modifiers[letter]), { onNone: () => this.obscure(), onSome: (modifier) => this.map((text) => modifier(text, pwd)) });
    }

    flag(flags: ReadonlyMap<string, string>, quoted: boolean): Value {
        const joined = Option.match(Option.fromUndefinedOr(flags.get('j')), { onNone: (): Value => this, onSome: (glue) => (this.content._tag === 'List' ? this.hold(Content.Scalar({ text: this.content.items.join(glue) })) : this) });
        const part = (by: string, items: readonly string[]) => {
            const parts = items.flatMap((item) => item.split(by));
            return by === '' ? joined.obscure(true) : joined.hold(Content.List({ items: quoted && !flags.has('@') ? parts.filter((item, at) => item !== '' || at === 0 || at === parts.length - 1) : parts, spread: true }));
        };
        const split = Option.match(Option.fromUndefinedOr(flags.has('f') ? '\n' : flags.get('s')), {
            onNone: () => joined,
            onSome: (by) => Content.$match(joined.content, { Scalar: ({ text }) => part(by, [text]), List: ({ items }) => part(by, items), Unset: () => joined.hold(Content.List({ items: [], spread: true })), Unknown: () => joined.obscure(true) }),
        });
        const change: Option.Option<(text: string) => string> = flags.has('U') ? Option.some(String.toUpperCase) : Option.liftPredicate(String.toLowerCase, () => flags.has('L'));
        const cased = Option.match(change, { onNone: () => split, onSome: (apply) => split.recase(apply) });
        return flags.has('@') && cased.content._tag === 'List' ? cased.hold(Content.List({ ...cased.content, spread: true })) : cased;
    }

    static read(name: string, scope: Scope): Value {
        const content = Binding.$match(scope.get(name), {
            Unset: () => Content.Unset(),
            Known: ({ value }) => Content.Scalar({ text: value }),
            Listed: ({ items }) => Content.List({ items, spread: name === '@' }),
            Unknown: ({ origins }) => Content.Unknown({ origin: Origin.Variable({ name, origins }), many: name === '@' || name === '*' }),
        });
        return new Value({ origin: Origin.Variable({ name, origins: HashSet.empty() }), content, refusals: [] });
    }

    static flags(written: string): Option.Option<ReadonlyMap<string, string>> {
        const parsed = Array.unfold(0, (at) => {
            const [letter, open] = [written.charAt(at), written.charAt(at + 1)];
            const finish = written.indexOf(closing[open] ?? open, at + 2);
            const taking = Option.liftPredicate([Option.some([letter, written.slice(at + 2, finish)] as const), finish + 1] as const, () => 'sj'.includes(letter) && finish !== -1);
            const flag = 'ULf@'.includes(letter) ? Option.some([Option.some([letter, ''] as const), at + 1] as const) : taking;
            return Option.liftPredicate(
                Option.getOrElse(flag, () => [Option.none<readonly [string, string]>(), written.length] as const),
                () => letter !== '',
            );
        });
        return Option.map(Option.all(parsed), (flags) => new Map(flags.toReversed()));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Content, Value };
