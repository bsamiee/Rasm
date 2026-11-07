import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Cause, Console, Effect, FileSystem, Match, Option, Order, Path, Predicate, Record, Result, Runtime, Schema, Stream, Struct } from 'effect';
import { CharSet, JS } from 'refa';
import { Alias, type Document, isNode, LineCounter, type Pair, parseAllDocuments, Scalar, YAMLMap, YAMLSeq } from 'yaml';
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
type Classes = (typeof Reply.Type)['regexes'][number]['classes'];
interface Subject {
    readonly kinds: readonly string[];
    readonly parent?: { readonly field: string; readonly subject: Subject };
}
interface Token {
    readonly value: string;
    readonly flags?: string;
}
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
const COMPOSITE: ReadonlySet<unknown> = new Set(['all', 'any', 'not']);
const EMPTY: Subject = { kinds: [] };
const REGIONS = [NON_ASCII.negate(), NON_ASCII];
const PRINTABLE = CharSet.empty(MAXIMUM).union([
    { min: 0x09, max: 0x0d },
    { min: 0x20, max: 0x7e },
]);
const WORD = JS.createCharSet([{ kind: 'word', negate: false }], { unicode: true });
const SHOWN = 4;
const WORDING = { written: { relation: '', missing: 'lacks', extra: 'adds' }, complement: { relation: 'complement of the ', missing: 'matches', extra: 'excludes' } };
const NANOSECONDS = new Intl.NumberFormat('en', { style: 'unit', unit: 'nanosecond', unitDisplay: 'long' });
const BYTES = new Intl.NumberFormat('en', { style: 'unit', unit: 'byte', unitDisplay: 'long' });

// --- [MODELS] --------------------------------------------------------------------------

const GrammarFile = Schema.fromJsonString(Schema.Struct({ rules: Schema.Record(Schema.String, Schema.Unknown), extras: Schema.Array(Schema.Unknown), externals: Schema.Array(Schema.Unknown), word: Schema.optional(Schema.NullOr(Schema.String)) }));
const Subject: Schema.Codec<Subject> = Schema.suspend(() => Schema.Struct({ kinds: Schema.Array(Schema.String), parent: Schema.optionalKey(Schema.Struct({ field: Schema.String, subject: Subject })) }));
const Site = Schema.Struct({ language: Schema.String, subject: Subject });
const Written = Schema.Union([Schema.Struct({ regex: Schema.String, site: Schema.OptionFromOptionalKey(Site) }), Schema.Struct({ transform: Schema.Json })]);
const Request = Schema.fromJsonString(Schema.Struct({ tokens: Schema.Array(Schema.Struct({ pattern: Schema.String, caseless: Schema.Boolean })), regexes: Schema.Array(Written), languages: Schema.Record(Schema.String, Schema.Json) }));
const Ranges = Schema.Array(Schema.Tuple([Schema.Finite, Schema.Finite]));
const Node: Schema.Codec<Node, unknown> = Schema.suspend(() =>
    Schema.Union([
        Schema.Struct({ type: Schema.Literal('CharacterClass'), ranges: Ranges }),
        Schema.Struct({ type: Schema.Literal('Alternation'), alternatives: Schema.Array(Schema.Array(Node)) }),
        Schema.Struct({ type: Schema.Literal('Quantifier'), min: Schema.Finite, max: Schema.NullOr(Schema.Finite), alternatives: Schema.Array(Schema.Array(Node)) }),
    ]),
);
const Literal = Schema.Struct({ text: Schema.String, kind: Schema.String, size: Schema.Finite, regex: Schema.Finite, pattern: Schema.Finite });
const Class = Schema.TaggedUnion({
    Characters: { source: Schema.String, polarity: Schema.Literals(['Stated', 'Complement']), ranges: Ranges },
    UnicodePerl: { source: Schema.String },
    UnicodeBoundary: { source: Schema.String },
});
const Reply = Schema.fromJsonString(
    Schema.Struct({
        tokens: Schema.Array(Node),
        regexes: Schema.Array(
            Schema.Struct({
                classes: Schema.Array(Class),
                literal: Schema.OptionFromNullOr(Literal),
            }),
        ),
    }),
);

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
const derived = (lexical: ReturnType<typeof lexemes>, parse: (rule: Token) => unknown): Facts => {
    const facts = Record.collect<string, CharSet, Fact>(classes(lexical, parse), (name, characters) => ({ name, characters, stated: characters.intersect(PRINTABLE), core: characters.intersect(WORD), lacked: REGIONS.map((region) => region.without(characters)) }));
    return { facts, claimed: CharSet.empty(PRINTABLE.maximum).union(...facts.map(Struct.get('stated'))) };
};

// --- [SITES]

const parsed = (source: string) => {
    const lines = new LineCounter();
    return { lines, documents: Array.fromIterable(parseAllDocuments(source, { lineCounter: lines })) };
};
const isText = (node: unknown): node is Scalar<string> => node instanceof Scalar && Predicate.isString(node.value);
const keyed = (pairs: readonly Pair[], name: string) => Array.findFirst(pairs, ({ key }) => key instanceof Scalar && key.value === name);
const resolved = (document: Document, value: unknown) => (value instanceof Alias ? value.resolve(document) : value);
const scalar = (document: Document, pairs: readonly Pair[], name: string) =>
    keyed(pairs, name).pipe(
        Option.map(({ value }) => resolved(document, value)),
        Option.filter(isText),
        Option.map(Struct.get('value')),
    );
const owned = (document: Document, { key, value }: Pair, language: Option.Option<string>, site: Option.Option<typeof Site.Type>): readonly Located[] => {
    const name = key instanceof Scalar ? key.value : undefined;
    const entry = name === 'regex' || name === 'transform' ? resolved(document, value) : value;
    if (name === 'regex' && isNode(value) && isText(entry)) {
        return [{ value: { regex: entry.value, site }, offset: value.range?.[0] ?? 0, language }];
    }
    const offset = value instanceof Alias ? value.range?.[0] : undefined;
    return name === 'transform' && entry instanceof YAMLMap ? Array.filterMap([...entry.pairs()], ({ value: node }) => (isNode(node) ? Result.succeed({ value: { transform: node.toJS(document) }, offset: offset ?? node.range?.[0] ?? 0, language }) : Result.failVoid)) : [];
};
const nested = (document: Document, { key, value }: Pair, subject: Subject): Subject => {
    const name = key instanceof Scalar ? key.value : undefined;
    if (name !== 'has' || !(value instanceof YAMLMap)) {
        return COMPOSITE.has(name) ? subject : EMPTY;
    }
    const pairs = [...value.pairs()];
    return Option.filter(scalar(document, pairs, 'field'), () => Option.isNone(keyed(pairs, 'stopBy'))).pipe(Option.match({ onNone: () => EMPTY, onSome: (field) => ({ kinds: [], parent: { field, subject } }) }));
};
const located = (document: Document, node: unknown, language: Option.Option<string>, subject: Subject): readonly Located[] => {
    if (node instanceof YAMLSeq) {
        return [...node].flatMap((item) => located(document, item, language, node.anchor === undefined ? subject : EMPTY));
    }
    if (!(node instanceof YAMLMap)) {
        return [];
    }
    const held = node.anchor === undefined ? subject : EMPTY;
    const pairs = [...node.pairs()];
    const scope = Array.findFirst(pairs, ({ key }) => key instanceof Scalar && SCOPES.has(key.value)).pipe(
        Option.map(({ value }) => resolved(document, value)),
        Option.filter(isText),
        Option.map(({ value }) => value),
        Option.orElse(() => language),
    );
    const here: Subject = { ...held, kinds: [...held.kinds, ...Option.toArray(scalar(document, pairs, 'kind'))] };
    const site = Option.filter(scope, () => Option.isNone(keyed(pairs, 'pattern'))).pipe(Option.map((name) => ({ language: name, subject: here })));
    const bodies = ({ key, value }: Pair) => (key instanceof Scalar && key.value === 'utils' && value instanceof YAMLMap ? [...value.pairs()].map(Struct.get('value')) : [value]);
    return pairs.flatMap((pair) => [...owned(document, pair, scope, site), ...bodies(pair).flatMap((body) => located(document, body, scope, nested(document, pair, here)))]);
};

// --- [JUDGEMENT]

const shown = (characters: CharSet) => `${characters.size} (${characters.toRangesString().split(', ').slice(0, SHOWN).join(', ')}${characters.ranges.length > SHOWN ? ', …' : ''})`;
const distance = Order.mapInput(Order.Number, ({ missing, extra }: Judgement) => missing.size + extra.size);
const closest = (side: CharSet, { facts, claimed }: Facts) => {
    const held = side.intersect(claimed);
    const printable = side.intersect(PRINTABLE);
    const outside = side.without(PRINTABLE);
    const matched = Array.filterMap(facts, ({ name, characters, stated, core, lacked }) =>
        !stated.isEmpty && (core.isEmpty ? held.equals(stated) : held.isSubsetOf(stated) && core.isSubsetOf(side)) && printable.without(characters).size <= stated.size && lacked.every((region) => region.isEmpty || !region.isSubsetOf(side))
            ? Result.succeed({ fact: name, missing: characters.without(side), extra: outside.without(characters) })
            : Result.failVoid,
    );
    return Array.isArrayNonEmpty(matched) ? Option.liftPredicate(Array.min(matched, distance), ({ missing, extra }) => !(missing.isEmpty && extra.isEmpty)) : Option.none();
};
const findings = (place: string, held: Classes, grammar: Option.Option<readonly [string, Facts]>) =>
    held.flatMap(
        Class.match({
            Characters: ({ source, polarity, ranges }) =>
                Option.toArray(
                    Option.flatMap(grammar, ([language, facts]) => {
                        const inverted = polarity === 'Complement';
                        const characters = charset(ranges);
                        const wording = inverted ? WORDING.complement : WORDING.written;
                        return Option.map(
                            closest(inverted ? characters.negate() : characters, facts),
                            ({ fact, missing, extra }) => `${place}: ${language} class ${source} differs from the ${wording.relation}grammar's ${fact} class: ${[...(missing.isEmpty ? [] : [`${wording.missing} ${shown(missing)}`]), ...(extra.isEmpty ? [] : [`${wording.extra} ${shown(extra)}`])].join(', ')}`,
                        );
                    }),
                ),
            UnicodePerl: ({ source }) => [`${place}: Unicode Perl class ${source}, write the grammar's class or scope the Perl class to ASCII`],
            UnicodeBoundary: ({ source }) => [`${place}: Unicode word boundary ${source}, scope it to ASCII`],
        }),
    );
const replacement = (place: string, language: string, { text, kind, size, regex, pattern }: typeof Literal.Type) =>
    `${place}: ${language} regex matches the ${kind} text ${text} alone, which pattern: ${text} states in ${NANOSECONDS.format(pattern)} of compile against ${NANOSECONDS.format(regex)} for the regex's program of ${BYTES.format(size)}`;
const sited = (file: string, { lines, documents }: ReturnType<typeof parsed>, language: Option.Option<string>) => ({
    errors: documents.flatMap((document) => document.errors.map(({ message }) => `${file}: ${message}`)),
    sites: documents.flatMap((document) => located(document, document.value, language, EMPTY)).map((site) => ({ ...site, place: `${file}:${lines.linePos(site.offset).line}` })),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const check = Effect.gen(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const source = yield* fs.readFileString(Sgconfig.file);
    const { ruleDirs, utilDirs, customLanguages } = yield* Schema.decodeEffect(Sgconfig.schema)(source);
    const grammar = ([language, row]: readonly [string, (typeof customLanguages)[string]]) => {
        const generated = [path.dirname(row.libraryPath), path.basename(row.libraryPath, path.extname(row.libraryPath)), 'src'];
        return Effect.map(decodeFile(GrammarFile, ...generated, 'grammar.json'), (decoded) => [language, lexemes(decoded), { ...Struct.omit(row, ['outlineRules']), nodeTypes: path.join(...generated, 'node-types.json') }] as const);
    };
    const grammars = yield* Effect.forEach(Record.toEntries(customLanguages), grammar, { concurrency: 'unbounded' });
    const folders = yield* Effect.forEach([...ruleDirs, ...utilDirs], (folder) => fs.glob(`${folder}/**/*.{yml,yaml}`), { concurrency: 'unbounded' });
    const files = [...Sgconfig.outlineCatalogs(path, customLanguages).map(({ language, source: file }) => ({ file, language: Option.some(language) })), ...folders.flat().map((file) => ({ file, language: Option.none<string>() }))];
    const read = ({ file, language }: (typeof files)[number]) => Effect.map(fs.readFileString(file), (content) => sited(file, parsed(content), language));
    const written = [sited(Sgconfig.file, parsed(source), Option.none()), ...(yield* Effect.forEach(files, read, { concurrency: 'unbounded' }))];
    const tokens = new Map(
        grammars.flatMap(([, held]) =>
            Object.values(held)
                .flat()
                .flatMap(patterns)
                .map((token) => [patternKey(token), token] as const),
        ),
    );
    const sites = written.flatMap(Struct.get('sites'));
    const request = yield* Schema.encodeEffect(Request)({
        tokens: globalThis.Array.from(tokens.values(), (token) => ({ pattern: token.value, caseless: token.flags?.includes('i') === true })),
        regexes: sites.map(Struct.get('value')),
        languages: Record.fromIterableWith(grammars, ([language, , library]) => [language, library] as const),
    });
    const reply = yield* runCommand('cargo', ['run', '--quiet', '--locked', '--release', '--package', 'rasm-regex-classes'], { stdin: { stream: Stream.encodeText(Stream.make(request)) } }).pipe(Effect.flatMap(Schema.decodeEffect(Reply)));
    if (reply.tokens.length !== tokens.size || reply.regexes.length !== sites.length) {
        return yield* new Cause.IllegalArgumentError(`Regex checker returned incorrect totals (tokens ${reply.tokens.length}/${tokens.size}, rule sites ${reply.regexes.length}/${sites.length})`);
    }
    const structures = new Map(Array.zip(tokens.keys(), reply.tokens));
    const parse = (rule: Token) => [{ type: 'Concatenation', elements: Option.toArray(Option.map(Option.fromUndefinedOr(structures.get(patternKey(rule))), revived)) }];
    const languages = Record.fromIterableWith(grammars, ([language, held]) => [language, derived(held, parse)] as const);
    const known = (name: string) => Option.map(Record.get(languages, name), (facts) => [name, facts] as const);
    const judged = ([{ place, language }, { classes: held, literal }]: readonly [(typeof sites)[number], (typeof reply.regexes)[number]]) => [...findings(place, held, Option.flatMap(language, known)), ...Option.toArray(Option.zipWith(language, literal, (name, item) => replacement(place, name, item)))];
    const found = [...written.flatMap(Struct.get('errors')), ...Array.zip(sites, reply.regexes).flatMap(judged)];
    return yield* Array.match(found, { onEmpty: () => Effect.void, onNonEmpty: (reported) => Effect.andThen(Console.error(reported.join('\n')), Mismatch.make({ findings: reported })) });
});
check.pipe(Effect.provide(layer), runMain);
