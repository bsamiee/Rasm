import { Array, flow, Match, Option, Predicate, pipe, Record, Result } from 'effect';
import MagicString from 'magic-string';
import regjsparser, { type AstNode, type CharacterClass, type CharacterClassBody, type Group, type UnicodePropertyEscape, type Value } from 'regjsparser';
import aliases from 'unicode-property-value-aliases';
import type { InDesignError } from './contract.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Walk {
    readonly script: boolean;
    readonly sets: boolean;
    readonly word: string;
    readonly wild: string;
}
type Construct = Extract<InDesignError['reasons'][number], { readonly _tag: 'Untranslatable' }>['constructs'][number];
type Node = AstNode<typeof FEATURES>;
type Rows = readonly Result.Result<readonly [start: number, end: number, text: string, construct: string], Construct>[];

// --- [CONSTANTS] -----------------------------------------------------------------------

const FEATURES = { lookbehind: true, namedGroups: true, unicodePropertyEscape: true, unicodeSet: true } as const;
const SETS = /--|&&|\\q\{/u;
const ASCII = /^[dw]$/u;
const POSIX = /\[\[[:=]/u;
const CATEGORY = /^(?:gc|General_Category)=/u;
const PREFIXED = /^Is/u;
const DIGITS = /^\d+$/u;
const TOKEN = /\\Q[\s\S]*?(?:\\E|$)|\\(?<escape>[\s\S])|\[\^?\]?(?:\\[\s\S]|\[[:=.][\s\S]*?[:=.]\]|[^\]\\])*\]|\(\?\([^)]*\)|\(\?P<(?<python>\w+)>|\(\?P=(?<back>\w+)\)|\(\?(?<recursion>R|[+-]?\d+|&\w+)\)|\(\?<(?<named>[A-Za-z_]\w*)>|(?<plain>\((?![?*]))|(?<trailing>\\$)/gu;
const CHANGE = /\\(?<back>[1-9])|(?<whole>\$&)|\$\{(?<braced>\w+)\}|\$<(?<angled>\w+)>|(?<dollar>\$\$)|\\u(?:(?<hex>[\dA-Fa-f]{4})|\{(?<point>[\dA-Fa-f]+)\})|(?<many>\$\d{2,})|\\E(?<rest>[\s\S]*)|\\[\s\S]/gu;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [TEXT]
const hex = (code: number): string => {
    const radix = 16;
    return `\\x{${code.toString(radix).toUpperCase()}}`;
};
const escapes = (source: string): readonly string[] => Array.flatMap(Array.fromIterable(source.matchAll(TOKEN)), ({ groups }) => Array.fromNullishOr(groups?.escape));
const derived = ((): ((source: string, flags: 'u' | 'v') => string) => {
    const known = new Map<string, string>();
    const last = 0x10_ff_ff;
    const computed = (source: string, flags: 'u' | 'v'): string => {
        const member = new RegExp(`^${source}$`, flags);
        const inside = Array.makeBy(last + 2, (code) => code <= last && String.fromCodePoint(code).isWellFormed() && member.test(String.fromCodePoint(code)));
        const edges = Array.range(0, last + 1).filter((code) => inside[code] !== (inside[code - 1] ?? false));
        const side = (parity: number): readonly number[] => edges.filter((_, index) => index % 2 === parity);
        const ranges = Array.zipWith(side(0), side(1), (first, after) => (first === after - 1 ? hex(first) : `${hex(first)}-${hex(after - 1)}`)).join('');
        known.set(source, ranges);
        return ranges;
    };
    return (source, flags) => known.get(source) ?? computed(source, flags);
})();
const scanned = (source: string, tokens: RegExp, handlers: Record.ReadonlyRecord<string, (captured: string) => Result.Result<string, Construct['alternative']>>): Rows =>
    Array.flatMap(Array.fromIterable(source.matchAll(tokens)), (token) =>
        pipe(
            Record.toEntries(token.groups ?? {}),
            Array.filter(([, captured]) => Predicate.isNotUndefined(captured)),
            Array.flatMap(([name, captured]) => Option.toArray(Option.map(Record.get(handlers, name), (handler) => handler(captured)))),
            Array.map(Result.mapBoth({ onSuccess: (text) => [token.index, token.index + token[0].length, text, token[0]] as const, onFailure: (alternative) => ({ construct: token[0], alternative }) })),
        ),
    );
const settled = (source: string, rows: Rows): Result.Result<readonly [text: string, notes: readonly string[]], Array.NonEmptyReadonlyArray<Construct>> => {
    const [kept, refused] = Array.separate(rows);
    const applied = kept.filter(([start, end, text]) => source.slice(start, end) !== text);
    const rewritten = Array.dedupe(Array.map(applied, ([, , , construct]) => construct));
    const spliced = applied.reduce((edited, [start, end, text]) => (start === end ? edited.appendLeft(start, text) : edited.overwrite(start, end, text)), new MagicString(source)).toString();
    return Array.match(refused, { onNonEmpty: Result.fail, onEmpty: () => Result.succeed([spliced, Array.isReadonlyArrayEmpty(rewritten) ? [] : [`rewritten for InDesign: ${rewritten.join(' ')}`]] as const) });
};

// --- [WIDTHS]
const total = (nodes: readonly Node[]): Option.Option<number> =>
    Option.map(
        Option.all(Array.map(nodes, width)),
        Array.reduce(0, (sum, each) => sum + each),
    );
const width = (node: Node): Option.Option<number> =>
    Match.value(node).pipe(
        Match.discriminators('type')({
            anchor: () => Option.some(0),
            alternative: ({ body }) => total(body),
            disjunction: ({ body }) => Option.flatMap(Option.all(Array.map(body, width)), (widths) => Option.liftPredicate(Array.headNonEmpty(widths), () => new Set(widths).size === 1)),
            group: ({ behavior, body }) => (behavior === 'normal' || behavior === 'ignore' ? total(body) : Option.some(0)),
            quantifier: ({ body, min, max }) => Option.flatMap(total(body), (each) => Option.liftPredicate(min * each, () => min === max)),
            reference: () => Option.none(),
        }),
        Match.orElse(() => Option.some(1)),
    );

// --- [PATTERNS]
const point = ({ kind, codePoint, raw }: Value): string => (kind === 'unicodeEscape' || kind === 'unicodeCodePointEscape' || String.fromCodePoint(codePoint).length > 1 ? hex(codePoint) : raw);
const shorthand = ({ script }: Walk, value: string): Option.Option<readonly [negated: boolean, set: string]> =>
    Option.map(
        Option.liftPredicate(value.toLowerCase(), (lower) => script && ASCII.test(lower)),
        (lower) => [value !== lower, derived(`\\${lower}`, 'u')],
    );
const property = ({ value, negative, raw }: UnicodePropertyEscape): Result.Result<string, Construct> => {
    const category = value.replace(CATEGORY, '');
    const compiles = (source: string): boolean => Result.isSuccess(Result.try(() => new RegExp(source, 'u')));
    const scripted = pipe(
        Array.findFirst(
            Array.flatMap([value, value.replace(PREFIXED, '')], (name) => [name, `Script=${name}`]),
            (name) => compiles(`\\p{${name}}`),
        ),
        Result.fromOption((): Construct => ({ construct: raw, alternative: Option.none() })),
        Result.map((name) => `[${negative ? '^' : ''}${derived(`\\p{${name}}`, 'u')}]`),
    );
    return compiles(`\\p{gc=${category}}`) ? Result.succeed(`\\${negative ? 'P' : 'p'}{${category.length === 1 ? (aliases.get('General_Category')?.get(category) ?? category) : category}}`) : scripted;
};
const member = (walk: Walk, node: CharacterClassBody): Result.Result<readonly [inside: readonly string[], apart: readonly string[]], Construct> =>
    Match.value(node).pipe(
        Match.discriminators('type')({
            unicodePropertyEscape: (escape) => Result.map(property(escape), (text): readonly [readonly string[], readonly string[]] => [[], [text]]),
            characterClassEscape: ({ value, raw }) => Result.succeed(Option.match(shorthand(walk, value), { onNone: () => [[raw], []] as const, onSome: ([negated, set]) => (negated ? ([[], [`[^${set}]`]] as const) : ([[set], []] as const)) })),
            characterClassRange: ({ min, max }) => Result.succeed([[`${point(min)}-${point(max)}`], []] as const),
        }),
        Match.orElse((value) => Result.succeed([[point(value)], []] as const)),
    );
const classed = (walk: Walk, node: CharacterClass<typeof FEATURES>, put: (text: string) => Rows): Rows => {
    const [members, refused] = Array.separate(walk.sets ? [] : Array.map(node.body, (each) => member(walk, each)));
    const [kept, pulled] = [Array.flatMap(members, ([inside]) => inside).join(''), Array.flatMap(members, ([, apart]) => apart)];
    const [negation, closing, joined] = [node.negative ? '(?!' : '', node.negative ? ')(?s:.)' : '', [...(kept === '' ? [] : [`[${kept}]`]), ...pulled].join('|')];
    const caret = node.negative ? '^' : '';
    const built = Array.isReadonlyArrayEmpty(pulled) ? `[${caret}${kept}]` : `(?:${negation}${joined}${closing})`;
    return Array.match(refused, { onNonEmpty: Array.map(Result.fail), onEmpty: () => put(walk.sets ? `[${derived(node.raw, 'v')}]` : built) });
};
const rejoined = (node: Group<typeof FEATURES>, branches: readonly Node[]): Rows => {
    const [[start, end], negative] = [node.range, node.behavior === 'negativeLookbehind'];
    const open = `(?<${negative ? '!' : '='}`;
    const wrapped = negative ? [] : [Result.succeed([start, start + open.length, `(?:${open}`, node.raw] as const), Result.succeed([end - 1, end, '))', node.raw] as const)];
    return [...wrapped, ...Array.zipWith(branches, Array.drop(branches, 1), (left, right) => Result.succeed([left.range[1], right.range[0], `)${negative ? '' : '|'}${open}`, node.raw] as const))];
};
const grouped = (walk: Walk, node: Group<typeof FEATURES>, branched: boolean): Rows => {
    const [behind, [only]] = [node.behavior === 'negativeLookbehind' || node.behavior === 'lookbehind', node.body];
    const branches = node.body.length === 1 && only?.type === 'disjunction' ? only.body : [];
    const widths = Array.isReadonlyArrayEmpty(branches) ? [total(node.body)] : Array.map(branches, width);
    const split = behind && new Set(Array.getSomes(widths)).size > 1 ? rejoined(node, branches) : [];
    return behind && Array.some(widths, Option.isNone) ? [Result.fail({ construct: node.raw, alternative: Option.some('\\K') })] : [...split, ...Array.flatMap(node.body, (each) => splices(walk, each, branched))];
};
const splices = (walk: Walk, node: Node, branched: boolean): Rows => {
    const [[start, end], { script, word, wild }] = [node.range, walk];
    const put = (text: string): Rows => [Result.succeed([start, end, text, node.raw])];
    const anchors = {
        start: branched ? [`(?<!${wild})`] : [],
        end: branched ? [`(?!${wild})`] : [],
        boundary: script ? [`(?:(?<=[${word}])(?![${word}])|(?<![${word}])(?=[${word}]))`] : [],
        'not-boundary': script ? [`(?:(?<=[${word}])(?=[${word}])|(?<![${word}])(?![${word}]))`] : [],
    };
    return Match.value(node).pipe(
        Match.discriminators('type')({
            value: flow(point, put),
            anchor: ({ kind }) => Array.flatMap(anchors[kind], put),
            characterClassEscape: ({ value }) => Array.flatMap(Option.toArray(shorthand(walk, value)), ([negated, set]) => put(`[${negated ? '^' : ''}${set}]`)),
            unicodePropertyEscape: (escape) => [Result.map(property(escape), (text) => [start, end, text, node.raw] as const)],
            characterClass: (each) => classed(walk, each, put),
            disjunction: ({ body }) => Array.flatMap(body, (branch) => splices(walk, branch, true)),
            alternative: ({ body }) => Array.flatMap(body, (each) => splices(walk, each, branched)),
            quantifier: ({ body }) => Array.flatMap(body, (each) => splices(walk, each, branched)),
            group: (each) => grouped(walk, each, branched),
        }),
        Match.orElse(() => []),
    );
};
const parsed = (plain: string, script: boolean, multiline: boolean): Rows => {
    const modes: readonly ('u' | 'v')[] = SETS.test(plain) && !POSIX.test(plain) ? ['v', 'u'] : ['u'];
    const tree = Array.findFirst(modes, (mode) => Option.map(Result.getSuccess(Result.try(() => regjsparser.parse(plain, mode, FEATURES))), (root) => [mode, root] as const));
    return Option.match(tree, { onNone: () => [], onSome: ([mode, root]) => splices({ script, sets: mode === 'v', word: derived('\\w', 'u'), wild: multiline ? '[^\\r]' : '(?s:.)' }, root, false) });
};
const pattern = (value: string | RegExp): ReturnType<typeof settled> => {
    const script = Predicate.isRegExp(value);
    const source = script ? value.source : value;
    const told = escapes(source);
    const [flagged, multiline]: readonly [Rows, boolean] = script ? [[Result.succeed([0, 0, `${value.ignoreCase ? '(?i)' : ''}${value.dotAll ? '(?s)' : ''}${value.multiline ? '' : '(?-m)'}`, `/${value.flags}`] as const)], value.multiline] : [[], true];
    const breaks = Array.contains(told, 'n') && !Array.contains(told, 'r') ? ['\\n matches a forced line break, \\r ends a paragraph'] : [];
    const scanning = scanned(source, TOKEN, {
        python: (name) => Result.succeed(`(?<${name}>`),
        back: (name) => Result.succeed(`\\k<${name}>`),
        recursion: () => Result.fail(Option.none()),
        trailing: () => Result.fail(Option.some('\\\\')),
    });
    return Result.flatMap(settled(source, [...(source === '' ? [Result.fail({ construct: source, alternative: Option.none() })] : []), ...scanning]), ([plain, tokens]) => Result.map(settled(plain, [...flagged, ...parsed(plain, script, multiline)]), ([text, notes]) => [text, [...tokens, ...notes, ...breaks]] as const));
};
const changed = (value: string, sought: Option.Option<string>): ReturnType<typeof settled> => {
    const last = 9;
    const groups = Array.getSomes(Array.map(Array.fromIterable(Option.getOrElse(sought, () => '').matchAll(TOKEN)), ({ groups: token }) => Array.findFirst([token?.named, token?.python, token?.plain], Predicate.isNotUndefined)));
    const ordinal = (reference: string): Result.Result<string, Construct['alternative']> =>
        pipe(
            DIGITS.test(reference)
                ? Option.some(Number(reference))
                : Option.map(
                      Array.findFirstIndex(groups, (name) => name === reference),
                      (at) => at + 1,
                  ),
            Option.filter((index) => index <= last),
            Result.fromOption(() => Option.none<string>()),
            Result.map((index) => `$${index}`),
        );
    const coded = (digits: string): Result.Result<string, Construct['alternative']> => Result.succeed(`\\x{${digits}}`);
    return settled(
        value,
        scanned(value, CHANGE, {
            back: (digit) => Result.succeed(`$${digit}`),
            whole: () => Result.succeed('$0'),
            braced: ordinal,
            angled: ordinal,
            dollar: () => Result.succeed('\\$'),
            hex: coded,
            point: coded,
            many: () => Result.fail(Option.none()),
            rest: Result.liftPredicate(
                (after: string) => after === '',
                () => Option.none<string>(),
            ),
        }),
    );
};
const translated = (type: string, field: string, value: unknown, sought: Option.Option<string>): Result.Result<readonly [value: unknown, notes: readonly string[], probe: Option.Option<readonly [pattern: string, text: string]>], Array.NonEmptyReadonlyArray<Construct>> => {
    const kept = Result.succeed([value, [], Option.none()] as const);
    const sentinel = 'grepsentinel';
    const probed = ([text, notes]: readonly [string, readonly string[]]) => [text, notes, Option.some([Array.contains(escapes(text), 'K') ? `${text}|${sentinel}` : `(?:${text})|${sentinel}`, sentinel] as const)] as const;
    return Match.value(`${type}.${field}`).pipe(
        Match.when(Match.is('FindGrepPreference.findWhat', 'NestedGrepStyle.grepExpression'), () => (Predicate.isString(value) || Predicate.isRegExp(value) ? Result.map(pattern(value), probed) : kept)),
        Match.when('ChangeGrepPreference.changeTo', () => (Predicate.isString(value) ? Result.map(changed(value, sought), ([text, notes]) => [text, notes, Option.none()] as const) : kept)),
        Match.orElse(() => kept),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { translated };
