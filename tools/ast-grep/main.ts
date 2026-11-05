import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Console, Effect, FileSystem, Option, Order, Path, Predicate, Record, Runtime, Schema, Struct } from 'effect';
import { CharSet } from 'refa';
import { LineCounter, type Pair, parseAllDocuments, Scalar, YAMLMap, YAMLSeq } from 'yaml';
import { decodeFile, Sgconfig } from '#/eng/typescript/manifest';
import { classes } from './grammars/derive.js';
import { ASCII, NON_ASCII, regex, sets } from './grammars/regex.js';

// --- [TYPES] ---------------------------------------------------------------------------

interface ClassNode {
    readonly characters: CharSet;
    readonly source: { readonly start: number; readonly end: number };
    readonly perl: boolean;
    readonly inverted: boolean;
}
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
interface Located {
    readonly value: string;
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
const REGIONS = [ASCII, NON_ASCII];
const PRINTABLE = CharSet.empty(ASCII.maximum).union([
    { min: 0x09, max: 0x0d },
    { min: 0x20, max: 0x7e },
]);
const WORD = CharSet.empty(ASCII.maximum).union([
    { min: 0x30, max: 0x39 },
    { min: 0x41, max: 0x5a },
    { min: 0x5f, max: 0x5f },
    { min: 0x61, max: 0x7a },
]);
const TRANSFORM = /^(?<name>[^(]*)\([^,]*,(?<parameters>.*?)\)*$/su;
const PARAMETER = /(?<key>[^=]*)=\p{White_Space}*(?<value>'[^']*'|"[^"]*"|\[[^\]]*\]|[^,]*)[\p{White_Space},]*/guy;
const SHOWN = 4;
const WORDING = { written: { relation: '', missing: 'lacks', extra: 'adds' }, complement: { relation: 'complement of the ', missing: 'matches', extra: 'excludes' } };

// --- [MODELS] --------------------------------------------------------------------------

const Grammar = Schema.fromJsonString(Schema.Unknown);

// --- [ERRORS] --------------------------------------------------------------------------

class Mismatch extends Schema.TaggedError<Mismatch, { readonly brand: unique symbol }>()('Mismatch', { findings: Schema.NonEmptyArray(Schema.String) }) {
    override readonly [Runtime.errorReported] = false;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [FACTS]

const derived = (grammar: unknown): Facts => {
    const facts = Record.collect<string, CharSet, Fact>(classes(grammar), (name, characters) => ({ name, characters, stated: characters.intersect(PRINTABLE), core: characters.intersect(WORD), lacked: REGIONS.map((region) => region.without(characters)) }));
    return { facts, claimed: CharSet.empty(PRINTABLE.maximum).union(...facts.map(Struct.get('stated'))) };
};

// --- [SITES]

const parsed = (source: string) => {
    const lines = new LineCounter();
    return { lines, documents: Array.fromIterable(parseAllDocuments(source, { lineCounter: lines })) };
};
const isText = (node: unknown): node is Scalar<string> => node instanceof Scalar && Predicate.isString(node.value);
const text = (node: unknown, language: Option.Option<string>) => (isText(node) ? Option.some<Located>({ value: node.value, offset: node.range?.[0] ?? 0, language }) : Option.none());
const replaced = (site: Located) =>
    Option.fromNullishOr(TRANSFORM.exec(site.value.trim())?.groups).pipe(
        Option.filter(({ name }) => name?.trim() === 'replace'),
        Option.flatMap(({ parameters = '' }) => Array.findFirst(parameters.trim().matchAll(PARAMETER), ({ groups }) => groups?.key?.trim() === 'replace')),
        Option.flatMap(({ groups }) => Array.head(parsed(groups?.value ?? '').documents)),
        Option.filter(({ errors }) => Array.isArrayEmpty(errors)),
        Option.map((document): unknown => document.toJS()),
        Option.filter(Predicate.isString),
        Option.map((value): Located => ({ ...site, value })),
    );
const replacement = (node: unknown, language: Option.Option<string>) => {
    const object = node instanceof YAMLMap ? node.get('replace') : undefined;
    return object instanceof YAMLMap ? text(object.get('replace'), language) : Option.flatMap(text(node, language), replaced);
};
const owned = ({ key, value }: Pair, language: Option.Option<string>) => {
    const name = key instanceof Scalar ? key.value : undefined;
    if (name === 'regex') {
        return Option.toArray(text(value, language));
    }
    return name === 'transform' && value instanceof YAMLMap ? Array.getSomes([...value.pairs()].map(({ value: entry }) => replacement(entry, language))) : [];
};
const located = (node: unknown, language: Option.Option<string>): readonly Located[] => {
    const pairs = node instanceof YAMLMap ? [...node.pairs()] : [];
    const scope = Array.findFirst(pairs, ({ key }) => key instanceof Scalar && SCOPES.has(key.value)).pipe(
        Option.map(Struct.get('value')),
        Option.filter(isText),
        Option.map(({ value }) => value),
        Option.orElse(() => language),
    );
    return node instanceof YAMLSeq ? [...node].flatMap((item) => located(item, language)) : pairs.flatMap((pair) => [...owned(pair, scope), ...located(pair.value, scope)]);
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
const judged = (side: CharSet, { facts, claimed }: Facts) => {
    const matched = Array.getSomes(facts.map(targeted(side, claimed)));
    return Array.isArrayNonEmpty(matched) ? Option.liftPredicate(Array.min(matched, distance), ({ missing, extra }) => !(missing.isEmpty && extra.isEmpty)) : Option.none();
};
const findings = (place: string, language: string, pattern: string, facts: Facts) =>
    sets(regex(pattern)).flatMap(({ characters, source, perl, inverted }: ClassNode) => {
        const wording = inverted ? WORDING.complement : WORDING.written;
        const heading = `${place}: ${language} class ${[...pattern].slice(source.start, source.end).join('')}`;
        return perl
            ? [`${heading} holds a Unicode Perl class, which no grammar states`]
            : Option.toArray(
                  Option.map(
                      judged(inverted ? characters.negate() : characters, facts),
                      ({ fact, missing, extra }) => `${heading} differs from the ${wording.relation}grammar's ${fact} class: ${[...(missing.isEmpty ? [] : [`${wording.missing} ${shown(missing)}`]), ...(extra.isEmpty ? [] : [`${wording.extra} ${shown(extra)}`])].join(', ')}`,
                  ),
              );
    });
const inspected = (file: string, { lines, documents }: ReturnType<typeof parsed>, languages: Readonly<Record<string, Facts>>) => {
    const site = ({ value, offset, language }: Located) =>
        Option.match(
            Option.flatMap(language, (name) => Option.map(Record.get(languages, name), (facts) => [name, facts] as const)),
            { onNone: () => [], onSome: ([name, facts]) => findings(`${file}:${lines.linePos(offset).line}`, name, value, facts) },
        );
    return documents.flatMap((document) => [...document.errors.map(({ message }) => `${file}: ${message}`), ...located(document.value, Option.none()).flatMap(site)]);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const check = Effect.gen(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const source = yield* fs.readFileString(Sgconfig.file);
    const { ruleDirs, utilDirs, customLanguages } = yield* Schema.decodeEffect(Sgconfig.schema)(source);
    const grammar = ([language, { libraryPath }]: readonly [string, { readonly libraryPath: string }]) => Effect.map(decodeFile(Grammar, path.dirname(libraryPath), path.basename(libraryPath, path.extname(libraryPath)), 'src', 'grammar.json'), (decoded) => [language, derived(decoded)] as const);
    const languages = Record.fromEntries(yield* Effect.forEach(Record.toEntries(customLanguages), grammar, { concurrency: 'unbounded' }));
    const folders = yield* Effect.forEach([...ruleDirs, ...utilDirs], (folder) => fs.glob(`${folder}/**/*.{yml,yaml}`), { concurrency: 'unbounded' });
    const files = [...Array.getSomes(Object.values(customLanguages).map(Struct.get('outlineRules'))), ...folders.flat()];
    const read = (file: string) => Effect.map(fs.readFileString(file), (content) => inspected(file, parsed(content), languages));
    const found = [...inspected(Sgconfig.file, parsed(source), languages), ...(yield* Effect.forEach(files, read, { concurrency: 'unbounded' })).flat()];
    return yield* Array.match(found, { onEmpty: () => Effect.void, onNonEmpty: (reported) => Effect.andThen(Console.error(reported.join('\n')), Mismatch.make({ findings: reported })) });
});
check.pipe(Effect.provide(layer), runMain);
