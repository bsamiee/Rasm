import type { Tree } from '@nx/devkit';
import { Array, Effect, Match, Option, Path, Predicate, Record, Stream } from 'effect';
import { type Document, Pair, parseAllDocuments, Scalar, visit, YAMLMap, YAMLSeq } from 'yaml';
import { Sgconfig } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';
import { decodeTree, syncGenerator, treeText } from '#/tools/nx/internal/plugin-runtime';
import { WorkspaceError } from '#/tools/nx/internal/workspace-error';

// --- [TYPES] ---------------------------------------------------------------------------

type Kind = keyof typeof KINDS;
type Table = Readonly<Record<Kind, ReadonlyMap<string, Pair>>>;
type Held = Readonly<Record<Kind, ReadonlySet<string>>>;
interface Shared {
    readonly table: Table;
    readonly variants: Record.ReadonlyRecord<string, Table>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const LAYOUT = { indent: 4, lineWidth: 0, flowCollectionPadding: false, singleQuote: true } as const;
const KINDS = { utils: ['rule'], rewriters: ['transform', 'name'], transform: ['name'] } as const satisfies Record.ReadonlyRecord<string, readonly string[]>;
const VARIABLE = /\$+([A-Z_][A-Z0-9_]*)/g;
const LISTED = /rewriters=\[([^\]]*)\]/g;
const READS = ['name', 'signature', 'transform'];
const CAPTURES = ['rule', 'constraints', 'utils'];
const SELECTOR = 'variants';

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
const strings = (nodes: readonly unknown[]) => reached(nodes).flatMap((node) => (node instanceof Scalar && Predicate.isString(node.value) ? [node.value] : []));
const keyed = (nodes: readonly unknown[], key: string) => reached(nodes).filter((node): node is Pair => node instanceof Pair && named(node.key) === key);
const variables = (nodes: readonly unknown[]) => new Set(strings(nodes).flatMap((text) => Array.getSomes([...text.matchAll(VARIABLE)].map(([, name]) => Option.fromNullishOr(name)))));
const listed = (nodes: readonly unknown[]) => [
    ...keyed(nodes, 'rewriters').flatMap(({ value }) => (value instanceof YAMLSeq ? [...value].map(named) : [])),
    ...strings(nodes).flatMap((text) => Array.getSomes([...text.matchAll(LISTED)].map(([, names]) => Option.fromNullishOr(names))).flatMap((names) => names.split(',').map((name) => name.trim()))),
];
const entries = (kind: Kind, node: unknown): ReadonlyMap<string, Pair> =>
    new Map(
        kind === 'rewriters'
            ? (node instanceof YAMLSeq ? [...node] : []).filter((item): item is YAMLMap => item instanceof YAMLMap).map((item) => [named(item.get('id')), new Pair(item.get('id'), item)] as const)
            : (node instanceof YAMLMap ? [...node.pairs()] : []).map((pair) => [named(pair.key), pair] as const),
    );
const table = (node: unknown): Table => Record.map(KINDS, (_, kind) => entries(kind, node instanceof YAMLMap ? node.get(kind) : undefined));
const merged = (base: Table, over: Table): Table => Record.map(KINDS, (_, kind) => new Map([...base[kind], ...over[kind]]));
const unanchored = (doc: Document) => {
    const flat = doc.clone();
    const drop = (_: unknown, node: Scalar | YAMLMap | YAMLSeq) => {
        delete node.anchor;
    };
    visit(flat, { Alias: (_, alias) => alias.resolve(flat)?.clone() });
    visit(flat, { Scalar: drop, Map: drop, Seq: drop });
    return flat.value;
};
const shared = (header: unknown): Shared => {
    const variants = header instanceof YAMLMap ? header.get(SELECTOR) : undefined;
    return { table: table(header), variants: Record.fromEntries((variants instanceof YAMLMap ? [...variants.pairs()] : []).map((pair) => [named(pair.key), table(pair.value)] as const)) };
};

// --- [RESOLUTION]
const closure = (definitions: Table, local: Table, pairs: readonly Pair[], captures: ReadonlySet<string>, current: Held = Record.map(KINDS, () => new Set<string>())): Held => {
    const inlined = (kinds: readonly Kind[]) => kinds.flatMap((kind) => Array.getSomes([...current[kind]].map((name) => Option.fromNullishOr(definitions[kind].get(name)))));
    const within = (keys: readonly string[], kinds: readonly Kind[]) => [...pairs.filter((pair) => keys.includes(named(pair.key))), ...inlined(kinds)];
    const everything = [...pairs, ...inlined(Record.keys(KINDS))];
    const captured = new Set([...captures, ...variables(within(CAPTURES, ['utils']))]);
    const grown = (kind: Kind, names: Iterable<string>) => new Set([...current[kind], ...[...names].filter((name) => definitions[kind].has(name) && !local[kind].has(name))]);
    const next: Held = {
        utils: grown('utils', keyed(everything, 'matches').map(({ value }) => named(value))),
        rewriters: grown('rewriters', listed(everything)),
        transform: grown('transform', [...variables(within(READS, ['transform']))].filter((name) => !captured.has(name))),
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
const placed = (pairs: readonly Pair[], key: string, value: Option.Option<unknown>, before: readonly string[]) =>
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
        onNonEmpty: (held) => Option.some(kind === 'rewriters' ? new YAMLSeq(schema, held.map(({ value }) => value)) : new YAMLMap(schema, [...held])),
    });
const written = (catalog: Shared, language: string, source: string) => (doc: Document, index: number) =>
    Effect.gen(function* () {
        const map = yield* Effect.fromOption(Option.liftPredicate(doc.value, (value): value is YAMLMap => value instanceof YAMLMap), () => WorkspaceError.missing(source, `mapping at document ${index + 1}`));
        const selector = map.get(SELECTOR);
        const variants = yield* Effect.forEach(selector instanceof YAMLSeq ? [...selector].map(named) : [], (name) => Effect.fromOption(Record.get(catalog.variants, name), () => WorkspaceError.missing(source, `${SELECTOR} entry ${name}`)));
        const definitions = variants.reduce(merged, catalog.table);
        const local = table(map);
        const pairs = [...map.pairs()].filter((pair) => named(pair.key) !== SELECTOR);
        const kept = closure(definitions, local, pairs, new Set());
        const assembled = Record.toEntries(KINDS).reduce(
            (held, [kind, before]) => placed(held, kind, gathered(kind, map.schema, [...[...definitions[kind]].filter(([name]) => kept[kind].has(name)).map(([, pair]) => pair), ...local[kind].values()]), before),
            placed(pairs, 'language', Option.some(new Scalar(language)), ['role']),
        );
        const next = doc.clone();
        next.value = new YAMLMap(map.schema, assembled);
        next.directives.docStart = index === 0 ? null : true;
        return next.toString(LAYOUT);
    });
const generated = Effect.fnUntraced(function* (tree: Tree, language: string, file: string, source: string) {
    const docs = parseAllDocuments(yield* treeText(tree, source));
    const header = Option.filter(Array.head(docs), ({ value }) => !(value instanceof YAMLMap && value.has('id')));
    const catalog = shared(Option.getOrUndefined(Option.map(header, unanchored)));
    const texts = yield* Effect.forEach(Option.isSome(header) ? docs.slice(1) : docs, written(catalog, language, source));
    return { file, next: yield* runCommand('yamlfmt', ['-conf', '.config/yamlfmt.yaml', '-in'], { cwd: tree.root, stdin: Stream.encodeText(Stream.make(texts.join(''))) }) };
});
const rows = Effect.fnUntraced(function* (tree: Tree) {
    const [path, { customLanguages }] = yield* Effect.all([Path.Path, decodeTree(tree, Sgconfig.schema, Sgconfig.file)]);
    const sourced = Array.getSomes(Record.collect(customLanguages, (language, { outlineRules }) => Option.filter(Option.map(outlineRules, (file) => ({ language, file, source: path.join(path.dirname(file), 'sources', path.basename(file)) })), ({ source }) => tree.exists(source))));
    return yield* Effect.forEach(sourced, ({ language, file, source }) => generated(tree, language, file, source));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const outlineCatalogs = syncGenerator('Outline catalogs differ from their sources and the shared utils, rewriters, and transforms each document calls', rows);

// --- [EXPORTS] -------------------------------------------------------------------------

export { outlineCatalogs };
