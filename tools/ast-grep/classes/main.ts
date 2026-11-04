import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Cause, Effect, FileSystem, Option, Path, Predicate, Record, Schema, Struct } from 'effect';
import type { CharSet } from 'refa';
import { LineCounter, type Pair, parseAllDocuments, Scalar, YAMLMap, YAMLSeq } from 'yaml';
import { decodeFile } from '#/eng/typescript/manifest';
import { TextFormat } from '#/eng/typescript/text-format';
import { classes, regex, sets } from '../grammars/derive.js';

// --- [TYPES] ---------------------------------------------------------------------------

type Facts = Readonly<Record<string, { readonly characters: CharSet; readonly unicode: boolean }>>;
interface Site {
    readonly file: string;
    readonly line: number;
    readonly language: string;
    readonly pattern: string;
}
interface ClassNode {
    readonly characters: CharSet;
    readonly source: { readonly start: number; readonly end: number };
    readonly perl: boolean;
}
interface Delta {
    readonly missing: CharSet;
    readonly extra: CharSet;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const [{ characters: PRINTABLE }] = sets(regex('[\\t-\\r -~]'));
const TRANSFORM = /^replace\(.*?[(,]\p{White_Space}*replace=(?<quote>['"])(?<pattern>.*?)\k<quote>/su;
const SHOWN = 4;
const RELATIONS: Readonly<Record<string, string>> = { 'false:true': 'narrower than', 'true:false': 'wider than', 'false:false': 'different from' };

// --- [MODELS] --------------------------------------------------------------------------

const Sgconfig = TextFormat.Yaml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            ruleDirs: Schema.Array(Schema.String),
            utilDirs: Schema.Array(Schema.String),
            customLanguages: Schema.Record(Schema.String, Schema.Struct({ libraryPath: Schema.String, outlineRules: Schema.OptionFromOptionalKey(Schema.String) })),
        }),
    ),
);
const Grammar = TextFormat('JSON', JSON.parse);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [JUDGEMENT]

const shown = (characters: CharSet) => `${characters.size} (${characters.toRangesString().split(', ').slice(0, SHOWN).join(', ')}${characters.ranges.length > SHOWN ? ', …' : ''})`;
const judged = (characters: CharSet, fact: CharSet) =>
    Array.findFirst(fact.isDisjointWith(PRINTABLE) ? [] : [characters, characters.negate()], (side) => side.intersect(PRINTABLE).equals(fact.intersect(PRINTABLE))).pipe(
        Option.filter((side) => !side.equals(fact)),
        Option.map((side): Delta => ({ missing: fact.without(side), extra: side.without(fact) })),
    );
const described = ({ missing, extra }: Delta) => [`is ${RELATIONS[`${missing.isEmpty}:${extra.isEmpty}`]} the grammar's`, ...(missing.isEmpty ? [] : [`lacks ${shown(missing)}`]), ...(extra.isEmpty ? [] : [`adds ${shown(extra)}`])].join(', ');
const finding =
    (site: Site, node: ClassNode) =>
    (fact: string, { characters }: { readonly characters: CharSet }) =>
        Option.map(judged(node.characters, characters), (delta) => `${site.file}:${site.line}: ${site.language} ${fact} class ${[...site.pattern].slice(node.source.start, node.source.end).join('')} ${described(delta)}`);
const findings = (site: Site, facts: Facts) => {
    const nodes: readonly ClassNode[] = sets(regex(site.pattern));
    return nodes.filter(({ perl }) => !perl).flatMap((node) => Array.getSomes(Record.collect(facts, finding(site, node))));
};

// --- [DOCUMENTS]

const scalarValue = (node: unknown) => (node instanceof Scalar ? Option.some(node.value) : Option.none());
const patternOf = (key: unknown, keys: readonly unknown[], text: string) => (key === 'regex' || (key === 'replace' && keys.includes('by')) ? Option.some(text) : Option.fromNullishOr(TRANSFORM.exec(text)?.groups?.pattern));
const owned =
    (keys: readonly unknown[]) =>
    ({ key, value }: Pair) =>
        value instanceof Scalar && Predicate.isString(value.value) ? Option.toArray(Option.map(patternOf(Option.getOrNull(scalarValue(key)), keys, value.value), (pattern) => ({ pattern, offset: value.range?.[0] ?? 0 }))) : [];
const patterns = (node: unknown): readonly { readonly pattern: string; readonly offset: number }[] => {
    if (node instanceof YAMLSeq) {
        return Array.fromIterable(node).flatMap(patterns);
    }
    const pairs = node instanceof YAMLMap ? Array.fromIterable(node.pairs()) : [];
    const keys = pairs.map(({ key }) => Option.getOrNull(scalarValue(key)));
    return pairs.flatMap((pair) => [...owned(keys)(pair), ...patterns(pair.value)]);
};
const inspected = (file: string, text: string, facts: Readonly<Record<string, Facts>>) => {
    const lines = new LineCounter();
    const documents = Array.fromIterable(parseAllDocuments(text, { lineCounter: lines }));
    const known = (language: string) => Option.map(Record.get(facts, language), (fact) => [language, fact] as const);
    const site = ([language, fact]: readonly [string, Facts], document: (typeof documents)[number]) => patterns(document.value).flatMap(({ pattern, offset }) => findings({ file, line: lines.linePos(offset).line, language, pattern }, fact));
    return documents.flatMap((document) => Option.match(Option.flatMap(Option.filter(scalarValue(document.get('language')), Predicate.isString), known), { onNone: () => [], onSome: (pair) => site(pair, document) }));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const check = Effect.gen(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { ruleDirs, utilDirs, customLanguages } = yield* decodeFile(Sgconfig, 'sgconfig.yml');
    const grammar = ([language, { libraryPath }]: readonly [string, { readonly libraryPath: string }]) => Effect.map(decodeFile(Grammar, path.dirname(libraryPath), path.basename(libraryPath, path.extname(libraryPath)), 'src', 'grammar.json'), (decoded) => [language, classes(decoded)] as const);
    const facts = Record.fromEntries(yield* Effect.forEach(Record.toEntries(customLanguages), grammar, { concurrency: 'unbounded' }));
    const folders = yield* Effect.forEach([...ruleDirs, ...utilDirs], (folder) => fs.glob(`${folder}/**/*.yml`), { concurrency: 'unbounded' });
    const files = [...Array.getSomes(Object.values(customLanguages).map(Struct.get('outlineRules'))), ...folders.flat()];
    const read = (file: string) => Effect.map(fs.readFileString(file), (text) => inspected(file, text, facts));
    const found = (yield* Effect.forEach(files, read, { concurrency: 'unbounded' })).flat();
    return yield* Array.match(found, { onEmpty: () => Effect.void, onNonEmpty: (lines) => Effect.fail(new Cause.UnknownError(lines, lines.join('\n'))) });
});
check.pipe(Effect.provide(layer), runMain);
