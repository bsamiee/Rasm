import type { Tree } from '@nx/devkit';
import { Array, Effect, Equal, Match, Option, Predicate, Stream, String } from 'effect';
import { type Document, isNode, Pair, parseAllDocuments, parseDocument, Scalar, visit, YAMLMap, YAMLSeq } from 'yaml';
import { Sgconfig } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';
import { glob } from '#/tools/nx/internal/open-workspace';
import { decodeTree, syncGenerator, treeText } from '#/tools/nx/internal/plugin-runtime';
import { WorkspaceError } from '#/tools/nx/internal/workspace-error';

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
const descend = (node: unknown): readonly unknown[] =>
    Match.value(node).pipe(
        Match.when(Match.instanceOf(YAMLMap), (map) => [...map.pairs()].flatMap((pair) => [pair, pair.key, pair.value, ...descend(pair.key), ...descend(pair.value)])),
        Match.when(Match.instanceOf(YAMLSeq), (seq) => [...seq].flatMap((item) => [item, ...descend(item)])),
        Match.orElse(() => []),
    );
const calls = (nodes: readonly unknown[]) =>
    Array.getSomes(
        nodes
            .flatMap((node) => [node, ...descend(node)])
            .filter((node): node is Pair => node instanceof Pair && Option.contains(scalar(node.key), 'matches'))
            .map(({ value }) => Option.filter(scalar(value), Predicate.isString)),
    );
const closure = (shared: ReadonlyMap<string, Pair>, held: ReadonlySet<string>, frontier: readonly string[]): ReadonlySet<string> =>
    Array.match(Array.dedupe(frontier.filter((name) => shared.has(name) && !held.has(name))), {
        onEmpty: () => held,
        onNonEmpty: (fresh) => closure(shared, new Set([...held, ...fresh]), calls(fresh.map((name) => shared.get(name)?.value))),
    });
const anchored = (entry: YAMLMap, pairs: readonly Pair[]) => {
    const source = new Map(
        Array.getSomes(
            descend(entry)
                .filter(isNode)
                .map((node) => Option.map(Option.fromNullishOr(node.anchor), (anchor) => [anchor, node] as const)),
        ),
    );
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
    const held = closure(shared, new Set(), calls([...placed, ...own].map(({ value }) => value)));
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
    const [{ ruleDirs }, text] = yield* Effect.all([decodeTree(tree, Sgconfig.schema, Sgconfig.file), treeText(tree, Sgconfig.file)]);
    const entries = Option.match(
        Option.filter(field(parseDocument(text).value, 'languageInjections'), (held): held is YAMLSeq => held instanceof YAMLSeq),
        { onNone: () => [], onSome: (listed) => [...listed].filter(isMap) },
    );
    const files = yield* glob(
        tree.root,
        ruleDirs.map((dir) => `${dir}/**/*.{yml,yaml}`),
    );
    const written = yield* Effect.forEach(
        files,
        Effect.fnUntraced(function* (file) {
            const current = yield* treeText(tree, file);
            const docs: readonly Document[] = String.includes(MIRROR)(current) ? parseAllDocuments(current) : [];
            const texts = yield* Effect.forEach(docs, mirrored(entries, file));
            const stdin = Stream.encodeText(Stream.make(texts.join('')));
            return Array.isReadonlyArrayNonEmpty(texts) ? Option.some({ file, next: yield* runCommand('yamlfmt', ['-conf', '.config/yamlfmt.yaml', '-in'], { cwd: tree.root, stdin }) }) : Option.none();
        }),
    );
    return Array.getSomes(written);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const injectionUtils = syncGenerator(`Rule utils differ from the ${Sgconfig.file} injection utils their ${MIRROR} metadata names`, rows);

// --- [EXPORTS] -------------------------------------------------------------------------

export { injectionUtils };
