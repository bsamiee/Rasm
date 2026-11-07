import process from 'node:process';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Cause, Effect, FileSystem, Match, Option, Record, Schema, String, Struct } from 'effect';
import { Document, type Node, Pair, parseAllDocuments, Scalar, visit, YAMLMap, YAMLSeq } from 'yaml';

// --- [TYPES] ---------------------------------------------------------------------------

type Kind = keyof typeof KINDS;
type Table = Readonly<Record<Kind, ReadonlyMap<string, Pair>>>;
interface Shared {
    readonly table: Table;
    readonly variants: Record.ReadonlyRecord<string, Table>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const KINDS = { utils: ['rule'], rewriters: ['transform', 'name'], transform: ['name'] } as const satisfies Record.ReadonlyRecord<string, readonly string[]>;
const VARIABLE = /\$+(?<name>[A-Z_][A-Z0-9_]*)/gu;
const LISTED = /rewriters=\[(?<names>[^\]]*)\]/gu;
const READS = ['name', 'signature', 'transform'];
const CAPTURES = ['rule', 'constraints', 'utils'];
const SELECTOR = 'variants';

// --- [MODELS] --------------------------------------------------------------------------

const Arguments = Schema.Tuple([Schema.String, Schema.String, Schema.String]);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [READING]
const named = (node: unknown) => (node instanceof Scalar ? globalThis.String(node.value) : '');
const descend = (node: unknown): readonly unknown[] =>
    Match.value(node).pipe(
        Match.when(Match.instanceOf(YAMLMap), (map) => [...map.pairs()].flatMap(descend)),
        Match.when(Match.instanceOf(YAMLSeq), (seq) => [...seq].flatMap((item) => [item, ...descend(item)])),
        Match.when(Match.instanceOf(Pair), (pair) => [pair, pair.key, pair.value, ...descend(pair.key), ...descend(pair.value)]),
        Match.orElse(() => []),
    );
const reached = (nodes: readonly unknown[]) => [...nodes, ...nodes.flatMap(descend)];
const strings = (nodes: readonly unknown[]) =>
    reached(nodes)
        .filter((node): node is Scalar<string> => node instanceof Scalar && typeof node.value === 'string')
        .map(({ value }) => value);
const matched = (nodes: readonly unknown[], pattern: RegExp, group: string) => Array.getSomes(strings(nodes).flatMap((text) => [...text.matchAll(pattern)].map(({ groups }) => Option.fromNullishOr(groups?.[group]))));
const keyed = (nodes: readonly unknown[], key: string) => reached(nodes).filter((node): node is Pair => node instanceof Pair && named(node.key) === key);
const listed = (nodes: readonly unknown[]) => [
    ...keyed(nodes, 'rewriters')
        .map(({ value }) => value)
        .filter((value) => value instanceof YAMLSeq)
        .flatMap((seq) => [...seq].map(named)),
    ...matched(nodes, LISTED, 'names').flatMap(String.split(',')).map(String.trim),
];
const entries = (node: unknown): ReadonlyMap<string, Pair> =>
    new Map(
        Match.value(node)
            .pipe(
                Match.when(Match.instanceOf(YAMLSeq), (seq) => [...seq].filter((item) => item instanceof YAMLMap).map((item) => new Pair(new Scalar(named(item.get('id'))), item))),
                Match.when(Match.instanceOf(YAMLMap), (map) => [...map.pairs()]),
                Match.orElse(() => []),
            )
            .map((pair) => [named(pair.key), pair] as const),
    );
const table = (node: unknown): Table => Record.map(KINDS, (_, kind) => entries(node instanceof YAMLMap ? node.get(kind) : undefined));
const merged = (base: Table, over: Table): Table => Record.map(KINDS, (_, kind) => new Map([...base[kind], ...over[kind]]));
const unanchored = (doc: Document) => {
    const flat = doc.clone();
    const plain = (_: unknown, node: Node) => (node.anchor === undefined ? undefined : flat.createNode(node.toJS(flat)));
    visit(flat, { Alias: (_, alias) => alias.resolve(flat)?.clone() });
    visit(flat, { Scalar: plain, Collection: plain });
    return flat.value;
};
const shared = (header: unknown): Shared => {
    const variants = header instanceof YAMLMap ? header.get(SELECTOR) : undefined;
    return { table: table(header), variants: Record.fromIterableWith(variants instanceof YAMLMap ? variants.pairs() : [], (pair) => [named(pair.key), table(pair.value)] as const) };
};

// --- [RESOLUTION]
const closure = (definitions: Table, local: Table, pairs: readonly Pair[], captures: ReadonlySet<string>, current: Readonly<Record<Kind, ReadonlySet<string>>> = Record.map(KINDS, () => new Set<string>())): Readonly<Record<Kind, ReadonlySet<string>>> => {
    const inlined = (kinds: readonly Kind[]) => kinds.flatMap((kind) => Array.getSomes([...current[kind]].map((name) => Option.fromNullishOr(definitions[kind].get(name)))));
    const within = (keys: readonly string[], kinds: readonly Kind[]) => [...pairs.filter((pair) => keys.includes(named(pair.key))), ...inlined(kinds)];
    const everything = [...pairs, ...inlined(Record.keys(KINDS))];
    const captured = new Set([...captures, ...matched(within(CAPTURES, ['utils']), VARIABLE, 'name')]);
    const grown = (kind: Kind, names: readonly string[]) => new Set([...current[kind], ...names.filter((name) => definitions[kind].has(name) && !local[kind].has(name))]);
    const next = {
        utils: grown(
            'utils',
            keyed(everything, 'matches').map(({ value }) => named(value)),
        ),
        rewriters: grown('rewriters', listed(everything)),
        transform: grown(
            'transform',
            matched(within(READS, ['transform']), VARIABLE, 'name').filter((name) => !captured.has(name)),
        ),
    };
    const shadowed = [...next.transform].filter((name) => captured.has(name));
    return Match.value(next).pipe(
        Match.when(
            () => Array.isArrayNonEmpty(shadowed),
            () => closure(definitions, local, pairs, new Set([...captured, ...shadowed])),
        ),
        Match.when(
            (grew) => Record.keys(KINDS).every((kind) => grew[kind].size === current[kind].size),
            () => current,
        ),
        Match.orElse((grew) => closure(definitions, local, pairs, captures, grew)),
    );
};
const placed = (pairs: readonly Pair[], key: string, value: Option.Option<Scalar | YAMLMap | YAMLSeq>, before: readonly string[]) =>
    Option.match(value, {
        onNone: () => pairs.filter((pair) => named(pair.key) !== key),
        onSome: (node) =>
            Option.match(
                Array.findFirstIndex(pairs, (pair) => named(pair.key) === key),
                {
                    onSome: (at) => pairs.map((pair, index) => (index === at ? new Pair(pair.key, node) : pair)),
                    onNone: () => {
                        const at = Option.getOrElse(
                            Array.findFirstIndex(pairs, (pair) => before.includes(named(pair.key))),
                            () => pairs.length,
                        );
                        return [...pairs.slice(0, at), new Pair(new Scalar(key), node), ...pairs.slice(at)];
                    },
                },
            ),
    });
const gathered = (kind: Kind, schema: YAMLMap['schema'], items: readonly Pair[]) =>
    Array.match(items, {
        onEmpty: () => Option.none(),
        onNonEmpty: (held) =>
            Option.some(
                kind === 'rewriters'
                    ? new YAMLSeq(
                          schema,
                          held.map(({ value }) => value),
                      )
                    : new YAMLMap(schema, [...held]),
            ),
    });
const written = Effect.fnUntraced(function* (catalog: Shared, language: string, source: string, doc: Document, index: number) {
    const map = yield* Effect.fromOption(
        Option.liftPredicate(doc.value, (value) => value instanceof YAMLMap),
        () => new Cause.IllegalArgumentError(`${source} document ${index + 1} holds no mapping`),
    );
    const selector = map.get(SELECTOR);
    const variants = yield* Effect.forEach(selector instanceof YAMLSeq ? [...selector].map(named) : [], (name) => Effect.fromOption(Record.get(catalog.variants, name), () => new Cause.IllegalArgumentError(`${source} ${SELECTOR} holds no ${name}`)));
    const definitions = variants.reduce(merged, catalog.table);
    const local = table(map);
    const pairs = [...map.pairs()].filter((pair) => named(pair.key) !== SELECTOR);
    const kept = closure(definitions, local, pairs, new Set());
    const kinds = Record.collect(KINDS, (kind, before) => ({ kind, before, items: [...definitions[kind].entries()].filter(([name]) => kept[kind].has(name)) }));
    const assembled = kinds.reduce((held, { kind, before, items }) => placed(held, kind, gathered(kind, map.schema, [...items.map(([, pair]) => pair), ...local[kind].values()]), before), placed(pairs, 'language', Option.some(new Scalar(language)), ['role']));
    return new Document(new YAMLMap(map.schema, assembled)).toString({ lineWidth: 0 });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const build = Effect.gen(function* () {
    const [language, source, output] = yield* Schema.decodeUnknownEffect(Arguments)(process.argv.slice(2));
    const fs = yield* FileSystem.FileSystem;
    const parsed = parseAllDocuments(yield* fs.readFileString(source));
    const docs = Array.isArray(parsed) ? parsed : [];
    const failures = docs.flatMap(Struct.get('errors')).map(({ message }) => `${source}: ${message}`);
    yield* Array.match(failures, { onEmpty: () => Effect.void, onNonEmpty: (held) => Effect.fail(new Cause.IllegalArgumentError(held.join('\n'))) });
    const header = Option.filter(Array.head(docs), ({ value }) => !(value instanceof YAMLMap && value.has('id')));
    const common = shared(Option.getOrUndefined(Option.map(header, unanchored)));
    const texts = yield* Effect.forEach(Option.isSome(header) ? docs.slice(1) : docs, (doc, index) => written(common, language, source, doc, index + Number(Option.isSome(header))));
    yield* fs.writeFileString(output, texts.join('---\n'));
});
build.pipe(Effect.provide(layer), runMain);
