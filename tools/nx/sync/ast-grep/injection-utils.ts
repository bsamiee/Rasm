import { globAsync, type Tree } from '@nx/devkit';
import { Array, Effect, Equal, Option, Predicate, String } from 'effect';
import { type Document, isNode, type Node, Pair, Scalar, visit, YAMLMap, YAMLSeq } from 'yaml';
import { Sgconfig } from '#/eng/typescript/manifest';
import { decodeTree, syncGenerator, treeText, yamlDocuments } from '#/tools/nx/sync/tree';
import { WorkspaceError } from '#/tools/nx/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MIRROR = 'injection-utils';
const LAYOUT = { indent: 4, lineWidth: 0, flowCollectionPadding: false, singleQuote: true } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const isMap = (held: unknown): held is YAMLMap => held instanceof YAMLMap;
const scalar = (node: unknown) =>
    Option.map(
        Option.liftPredicate(node, (held): held is Scalar => held instanceof Scalar),
        ({ value }) => value,
    );
const field = (node: unknown, key: string) => Option.flatMap(Option.liftPredicate(node, isMap), (map) => Option.fromNullishOr(map.get(key)));
const pairsOf = (node: unknown) => Option.match(Option.filter(field(node, 'utils'), isMap), { onNone: () => [], onSome: (map) => [...map.pairs()] });
const named = ({ key }: Pair) => Option.match(scalar(key), { onNone: () => '', onSome: (value) => globalThis.String(value) });
const calls = (nodes: readonly unknown[]) => {
    const names = new Set<string>();
    nodes.filter(isNode).forEach((node) => {
        visit(node, {
            Pair: (_, { key, value }) => {
                if (Option.contains(scalar(key), 'matches') && value instanceof Scalar && typeof value.value === 'string') {
                    names.add(value.value);
                }
            },
        });
    });
    return names;
};
const closure = (shared: ReadonlyMap<string, Pair>, names: Set<string>) => {
    const held = new Set<string>();
    names.forEach((name) => {
        const entry = shared.get(name);
        if (entry !== undefined) {
            held.add(name);
            calls([entry.value]).forEach(names.add.bind(names));
        }
    });
    return held;
};
const anchored = (entry: YAMLMap, pairs: readonly Pair[]) => {
    const source = new Map<string, Node>();
    visit(entry, {
        Node: (_, node) => {
            if (node.anchor !== undefined) {
                source.set(node.anchor, node);
            }
        },
    });
    const defined = new Set<string>();
    const map = new YAMLMap(
        entry.schema,
        pairs.map((pair) => pair.clone()),
    );
    visit(map, {
        Node: (_, { anchor }) => {
            if (anchor !== undefined) {
                defined.add(anchor);
            }
        },
        Alias: (_, alias) => (defined.has(alias.source) ? undefined : source.get(alias.source)?.clone()),
    });
    return map;
};
const mirror = (doc: Document, root: YAMLMap, entry: YAMLMap) => {
    const shared = new Map([new Pair(new Scalar('injection'), entry.get('rule')), ...pairsOf(entry)].map((pair) => [named(pair), pair] as const));
    const own = pairsOf(root).filter((pair) => !shared.has(named(pair)));
    const placed = [...root.pairs()].filter((pair) => named(pair) !== 'utils');
    const held = closure(shared, calls([...placed, ...own].map(({ value }) => value)));
    const utils = anchored(entry, [...[...shared.values()].filter((pair) => held.has(named(pair))), ...own]);
    const at = placed.findIndex((pair) => named(pair) === 'rule');
    const next = doc.clone();
    next.value = new YAMLMap(root.schema, utils.size === 0 ? placed : [...placed.slice(0, at), new Pair(new Scalar('utils'), utils), ...placed.slice(at)]);
    return next.toString(LAYOUT);
};
const mirrored = (entries: readonly YAMLMap[], file: string) => (doc: Document) => {
    const language = Option.flatMap(field(doc.value, 'language'), scalar);
    const injected = Option.filter(
        Option.flatMap(
            Option.flatMap(field(doc.value, 'metadata'), (metadata) => field(metadata, MIRROR)),
            scalar,
        ),
        Predicate.isString,
    );
    return Option.match(Option.all({ root: Option.liftPredicate(doc.value, isMap), wanted: injected }), {
        onNone: () => Effect.succeed(doc.toString(LAYOUT)),
        onSome: ({ root, wanted }) =>
            Effect.map(
                Effect.fromOption(
                    Array.findFirst(entries, (entry) => Equal.equals(Option.flatMap(field(entry, 'hostLanguage'), scalar), language) && Option.contains(Option.flatMap(field(entry, 'injected'), scalar), wanted)),
                    () => WorkspaceError.missing(file, `${Sgconfig.file} languageInjections entry from ${Option.match(language, { onNone: () => 'no language', onSome: globalThis.String })} to ${wanted}`),
                ),
                (entry) => mirror(doc, root, entry),
            ),
    });
};
const rows = Effect.fnUntraced(function* (tree: Tree) {
    const text = yield* treeText(tree, Sgconfig.file);
    const [config] = yield* yamlDocuments(text, Sgconfig.file);
    const { ruleDirs } = yield* decodeTree(tree, Sgconfig.schema, Sgconfig.file);
    const entries = Option.match(
        Option.filter(field(config?.value, 'languageInjections'), (held): held is YAMLSeq => held instanceof YAMLSeq),
        { onNone: () => [], onSome: (listed) => [...listed].filter(isMap) },
    );
    const patterns = ruleDirs.map((dir) => `${dir}/**/*.{yml,yaml}`);
    const files = yield* Effect.tryPromise(() => globAsync(tree, patterns));
    const written = yield* Effect.validate(
        files,
        Effect.fnUntraced(function* (file) {
            const current = yield* treeText(tree, file);
            const docs = String.includes(MIRROR)(current) ? yield* yamlDocuments(current, file) : [];
            const texts = yield* Effect.validate(docs, mirrored(entries, file));
            return Array.isReadonlyArrayNonEmpty(texts) ? Option.some({ file, next: texts.join('') }) : Option.none();
        }),
    );
    return Array.getSomes(written);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const injectionUtils = syncGenerator(`Rule utils differ from the ${Sgconfig.file} injection utils their ${MIRROR} metadata names`, rows);

// --- [EXPORTS] -------------------------------------------------------------------------

export { injectionUtils };
