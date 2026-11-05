import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Console, Effect, FileSystem, Match, Option, Order, Path, Predicate, Record, Runtime, Schema, Stream, Struct } from 'effect';
import { CharSet, JS } from 'refa';
import { type Document, isNode, LineCounter, type Pair, parseAllDocuments, Scalar, YAMLMap, YAMLSeq } from 'yaml';
import { decodeFile, Sgconfig } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';
import { classes, collected, lexemes, MAXIMUM, NON_ASCII } from './grammars/derive.js';

// --- [TYPES] ---------------------------------------------------------------------------

type Node =
    | { readonly type: 'CharacterClass'; readonly ranges: readonly (readonly [number, number])[] }
    | { readonly type: 'Alternation'; readonly alternatives: readonly (readonly Node[])[] }
    | { readonly type: 'Quantifier'; readonly min: number; readonly max: number | null; readonly alternatives: readonly (readonly Node[])[] };
interface Fact {
    readonly name: string;
    readonly characters: CharSet;
    readonly stated: CharSet;
    readonly core: CharSet;
    readonly lacked: readonly CharSet[];
}
interface Facts {
    readonly facts: readonly Fact[];
    readonly claimed: CharSet;
}
type Classes = (typeof Reply.Type)['classes'][number];
interface Token {
    readonly value: string;
    readonly flags?: string;
}
type Grammar = typeof GrammarFile.Type;
interface Located {
    readonly value: typeof Written.Type;
    readonly offset: number;
    readonly language: Option.Option<string>;
}
interface Judgement {
    readonly fact: string;
    readonly missing: CharSet;
    readonly extra: CharSet;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SCOPES: ReadonlySet<unknown> = new Set(['language', 'hostLanguage']);
const REGIONS = [NON_ASCII.negate(), NON_ASCII];
const PRINTABLE = CharSet.empty(MAXIMUM).union([
    { min: 0x09, max: 0x0d },
    { min: 0x20, max: 0x7e },
]);
const WORD = JS.createCharSet([{ kind: 'word', negate: false }], { unicode: true });
const SHOWN = 4;
const WORDING = { written: { relation: '', missing: 'lacks', extra: 'adds' }, complement: { relation: 'complement of the ', missing: 'matches', extra: 'excludes' } };

// --- [MODELS] --------------------------------------------------------------------------

const GrammarFile = Schema.fromJsonString(Schema.Struct({ rules: Schema.Record(Schema.String, Schema.Unknown), extras: Schema.Array(Schema.Unknown), externals: Schema.Array(Schema.Unknown), word: Schema.optional(Schema.NullOr(Schema.String)) }));
const Written = Schema.Union([Schema.String, Schema.Struct({ transform: Schema.Json })]);
const Request = Schema.fromJsonString(Schema.Struct({ tokens: Schema.Array(Schema.Struct({ pattern: Schema.String, caseless: Schema.Boolean })), regexes: Schema.Array(Written) }));
const Ranges = Schema.Array(Schema.Tuple([Schema.Finite, Schema.Finite]));
const Node: Schema.Codec<Node, unknown> = Schema.suspend(() =>
    Schema.Union([
        Schema.Struct({ type: Schema.Literal('CharacterClass'), ranges: Ranges }),
        Schema.Struct({ type: Schema.Literal('Alternation'), alternatives: Schema.Array(Schema.Array(Node)) }),
        Schema.Struct({ type: Schema.Literal('Quantifier'), min: Schema.Finite, max: Schema.NullOr(Schema.Finite), alternatives: Schema.Array(Schema.Array(Node)) }),
    ]),
);
const Reply = Schema.fromJsonString(Schema.Struct({ tokens: Schema.Array(Node), classes: Schema.Array(Schema.Array(Schema.Struct({ source: Schema.String, reading: Schema.Literals(['Plain', 'UnicodePerl']), polarity: Schema.Literals(['Stated', 'Complement']), ranges: Ranges }))) }));

// --- [ERRORS] --------------------------------------------------------------------------

class Mismatch extends Schema.TaggedError<Mismatch, { readonly brand: unique symbol }>()('Mismatch', { findings: Schema.NonEmptyArray(Schema.String) }) {
    override readonly [Runtime.errorReported] = false;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [FACTS]

const charset = (ranges: readonly (readonly [number, number])[]) => CharSet.empty(MAXIMUM).union(ranges.map(([min, max]) => ({ min, max })));
const concatenations = (alternatives: readonly (readonly Node[])[]): unknown => alternatives.map((elements) => ({ type: 'Concatenation', elements: elements.map(revived) }));
const revived: (node: Node) => unknown = Match.type<Node>().pipe(
    Match.discriminators('type')({
        CharacterClass: ({ ranges }) => ({ type: 'CharacterClass', characters: charset(ranges) }),
        Alternation: ({ alternatives }) => ({ type: 'Alternation', alternatives: concatenations(alternatives) }),
        Quantifier: ({ min, max, alternatives }) => ({ type: 'Quantifier', min, max: max ?? Number.POSITIVE_INFINITY, lazy: false, alternatives: concatenations(alternatives) }),
    }),
    Match.exhaustive,
);
const patterns = (rule: unknown): readonly Token[] => collected(rule, ({ type }: { readonly type: string }) => type === 'PATTERN');
const patternKey = ({ value, flags }: Token) => `${flags?.includes('i') === true ? 'i' : ''}/${value}`;
const derived = (grammar: Grammar, parse: (rule: Token) => unknown): Facts => {
    const facts = Record.collect<string, CharSet, Fact>(classes(lexemes(grammar), parse), (name, characters) => ({ name, characters, stated: characters.intersect(PRINTABLE), core: characters.intersect(WORD), lacked: REGIONS.map((region) => region.without(characters)) }));
    return { facts, claimed: CharSet.empty(PRINTABLE.maximum).union(...facts.map(Struct.get('stated'))) };
};

// --- [SITES]

const parsed = (source: string) => {
    const lines = new LineCounter();
    return { lines, documents: Array.fromIterable(parseAllDocuments(source, { lineCounter: lines })) };
};
const isText = (node: unknown): node is Scalar<string> => node instanceof Scalar && Predicate.isString(node.value);
const text = (node: unknown, language: Option.Option<string>) => (isText(node) ? Option.some<Located>({ value: node.value, offset: node.range?.[0] ?? 0, language }) : Option.none());
const transformed = (document: Document, node: unknown, language: Option.Option<string>) => (isNode(node) ? Option.some<Located>({ value: { transform: node.toJS(document) }, offset: node.range?.[0] ?? 0, language }) : Option.none());
const owned = (document: Document, { key, value }: Pair, language: Option.Option<string>) => {
    const name = key instanceof Scalar ? key.value : undefined;
    if (name === 'regex') {
        return Option.toArray(text(value, language));
    }
    return name === 'transform' && value instanceof YAMLMap ? Array.getSomes([...value.pairs()].map(({ value: entry }) => transformed(document, entry, language))) : [];
};
const located = (document: Document, node: unknown, language: Option.Option<string>): readonly Located[] => {
    const pairs = node instanceof YAMLMap ? [...node.pairs()] : [];
    const scope = Array.findFirst(pairs, ({ key }) => key instanceof Scalar && SCOPES.has(key.value)).pipe(
        Option.map(Struct.get('value')),
        Option.filter(isText),
        Option.map(({ value }) => value),
        Option.orElse(() => language),
    );
    const bodies = ({ key, value }: Pair) => (key instanceof Scalar && key.value === 'utils' && value instanceof YAMLMap ? [...value.pairs()].map(Struct.get('value')) : [value]);
    return node instanceof YAMLSeq ? [...node].flatMap((item) => located(document, item, language)) : pairs.flatMap((pair) => [...owned(document, pair, scope), ...bodies(pair).flatMap((body) => located(document, body, scope))]);
};

// --- [JUDGEMENT]

const shown = (characters: CharSet) => `${characters.size} (${characters.toRangesString().split(', ').slice(0, SHOWN).join(', ')}${characters.ranges.length > SHOWN ? ', …' : ''})`;
const distance = Order.mapInput(Order.Number, ({ missing, extra }: Judgement) => missing.size + extra.size);
const targeted =
    (side: CharSet, claimed: CharSet) =>
    ({ name, characters, stated, core, lacked }: Fact): Option.Option<Judgement> => {
        const held = side.intersect(claimed);
        return !stated.isEmpty && (core.isEmpty ? held.equals(stated) : held.isSubsetOf(stated) && core.isSubsetOf(side)) && side.intersect(PRINTABLE).without(characters).size <= stated.size && lacked.every((region) => region.isEmpty || !region.isSubsetOf(side))
            ? Option.some({ fact: name, missing: characters.without(side), extra: side.without(PRINTABLE).without(characters) })
            : Option.none();
    };
const closest = (side: CharSet, { facts, claimed }: Facts) => {
    const matched = Array.getSomes(facts.map(targeted(side, claimed)));
    return Array.isArrayNonEmpty(matched) ? Option.liftPredicate(Array.min(matched, distance), ({ missing, extra }) => !(missing.isEmpty && extra.isEmpty)) : Option.none();
};
const findings = (place: string, language: string, held: Classes, facts: Facts) =>
    held.flatMap(({ source, reading, polarity, ranges }) => {
        const inverted = polarity === 'Complement';
        const characters = charset(ranges);
        const wording = inverted ? WORDING.complement : WORDING.written;
        const heading = `${place}: ${language} class ${source}`;
        return reading === 'UnicodePerl'
            ? [`${heading} holds a Unicode Perl class, which no grammar states`]
            : Option.toArray(
                  Option.map(
                      closest(inverted ? characters.negate() : characters, facts),
                      ({ fact, missing, extra }) => `${heading} differs from the ${wording.relation}grammar's ${fact} class: ${[...(missing.isEmpty ? [] : [`${wording.missing} ${shown(missing)}`]), ...(extra.isEmpty ? [] : [`${wording.extra} ${shown(extra)}`])].join(', ')}`,
                  ),
              );
    });
const sited = (file: string, { lines, documents }: ReturnType<typeof parsed>) => ({
    errors: documents.flatMap((document) => document.errors.map(({ message }) => `${file}: ${message}`)),
    sites: documents.flatMap((document) => located(document, document.value, Option.none())).map((site) => ({ ...site, place: `${file}:${lines.linePos(site.offset).line}` })),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const check = Effect.gen(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const source = yield* fs.readFileString(Sgconfig.file);
    const { ruleDirs, utilDirs, customLanguages } = yield* Schema.decodeEffect(Sgconfig.schema)(source);
    const grammar = ([language, { libraryPath }]: readonly [string, { readonly libraryPath: string }]) => Effect.map(decodeFile(GrammarFile, path.dirname(libraryPath), path.basename(libraryPath, path.extname(libraryPath)), 'src', 'grammar.json'), (decoded) => [language, decoded] as const);
    const grammars = yield* Effect.forEach(Record.toEntries(customLanguages), grammar, { concurrency: 'unbounded' });
    const folders = yield* Effect.forEach([...ruleDirs, ...utilDirs], (folder) => fs.glob(`${folder}/**/*.{yml,yaml}`), { concurrency: 'unbounded' });
    const files = [...Array.getSomes(Object.values(customLanguages).map(Struct.get('outlineRules'))), ...folders.flat()];
    const read = (file: string) => Effect.map(fs.readFileString(file), (content) => sited(file, parsed(content)));
    const written = [sited(Sgconfig.file, parsed(source)), ...(yield* Effect.forEach(files, read, { concurrency: 'unbounded' }))];
    const tokens = grammars.flatMap(([, held]) => Object.values(lexemes(held)).flat().flatMap(patterns));
    const sites = written.flatMap(Struct.get('sites'));
    const request = yield* Schema.encodeEffect(Request)({ tokens: tokens.map((token) => ({ pattern: token.value, caseless: token.flags?.includes('i') === true })), regexes: sites.map(Struct.get('value')) });
    const reply = yield* runCommand('cargo', ['run', '--quiet', '--locked', '--package', 'rasm-regex-classes'], { stdin: { stream: Stream.encodeText(Stream.make(request)) } }).pipe(Effect.flatMap(Schema.decodeEffect(Reply)));
    const structures = new Map(Array.zip(tokens, reply.tokens).map(([token, structure]) => [patternKey(token), structure] as const));
    const parse = (rule: Token) => [{ type: 'Concatenation', elements: Option.toArray(Option.map(Option.fromUndefinedOr(structures.get(patternKey(rule))), revived)) }];
    const languages = Record.fromIterableWith(grammars, ([language, held]) => [language, derived(held, parse)] as const);
    const known = (name: string) => Option.map(Record.get(languages, name), (facts) => [name, facts] as const);
    const judged = ([{ place, language }, held]: readonly [(typeof sites)[number], Classes]) => Option.match(Option.flatMap(language, known), { onNone: () => [], onSome: ([name, facts]) => findings(place, name, held, facts) });
    const found = [...written.flatMap(Struct.get('errors')), ...Array.zip(sites, reply.classes).flatMap(judged)];
    return yield* Array.match(found, { onEmpty: () => Effect.void, onNonEmpty: (reported) => Effect.andThen(Console.error(reported.join('\n')), Mismatch.make({ findings: reported })) });
});
check.pipe(Effect.provide(layer), runMain);
