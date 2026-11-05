import { type Edit, parse, registerDynamicLanguage, type Rule, type SgNode } from '@ast-grep/napi';
import type { Tree } from '@nx/devkit';
import { readNxJsonFromDisk } from '@nx/devkit/internal';
import { Array, Effect, Equivalence, Match, Option, Order, Path, Predicate, Record, Result, Schema, String, Struct } from 'effect';
import { parseAllDocuments } from 'yaml';
import { accumulate } from '#/eng/typescript/accumulate';
import { Sgconfig } from '#/eng/typescript/manifest';
import { decodeTree, formatted, syncGenerator, treeText } from '#/tools/nx/sync/tree';
import { openWorkspace, SWIFT_MANIFEST, SwiftPackage, WorkspaceError } from '#/tools/nx/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

interface Dependency {
    readonly identity: string;
    readonly path: string;
    readonly dependencies: readonly Dependency[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FACTORIES = ['target', 'executableTarget', 'testTarget', 'macro', 'plugin'];

// --- [MODELS] --------------------------------------------------------------------------

const Dependency: Schema.Codec<Dependency> = Schema.Struct({ identity: Schema.String, path: Schema.String, dependencies: Schema.Array(Schema.suspend((): Schema.Codec<Dependency> => Dependency)) });
const Matcher = Schema.declare<Rule>(Predicate.isObject);
const ImportDocument = Schema.Struct({ isImport: Schema.Literal(true), name: Schema.TemplateLiteralParser(['$', Schema.String]), rule: Matcher, utils: Schema.optionalKey(Schema.Record(Schema.String, Matcher)) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const reached = ({ dependencies }: Dependency): readonly Dependency[] => dependencies.flatMap((dependency) => [dependency, ...reached(dependency)]);
const rewritten = (desired: Record.ReadonlyRecord<string, readonly string[]>) => (call: SgNode) => {
    const values = call
        .children()
        .flatMap((suffix) => (suffix.is('call_suffix') ? suffix.children() : []))
        .flatMap((held) => (held.is('value_arguments') ? held.children() : []))
        .filter((held) => held.is('value_argument'));
    const at = (label: string) => Array.findFirstIndex(values, (value) => value.field('name')?.text() === label);
    const end = (index: number) => Option.map(Array.get(values, index), (value) => value.range().end.index);
    const name = Option.flatMap(at('name'), (index) => Option.flatMap(Array.get(values, index), (value) => Option.fromNullishOr(value.find({ rule: { kind: 'line_str_text' } }))));
    return Option.toArray(Option.flatMap(name, (held) => Record.get(desired, held.text()))).flatMap(
        (rows): readonly Edit[] =>
            Match.value({ listed: `[${rows.join(', ')}]`, filled: Array.isReadonlyArrayNonEmpty(rows), present: at('dependencies') }).pipe(
                Match.when({ filled: true, present: Option.isSome }, ({ listed, present }) => Option.toArray(Option.flatMap(Array.get(values, present.value), (value) => Option.map(Option.fromNullishOr(value.field('value')), (list) => list.replace(listed))))),
                Match.when({ present: Option.isSome }, ({ present }) => Option.toArray(Option.map(Option.all([end(present.value - 1), end(present.value)]), ([startPos, endPos]) => ({ startPos, endPos, insertedText: '' })))),
                Match.when({ filled: true }, ({ listed }) => Option.toArray(Option.flatMap(at('name'), (index) => Option.map(end(index), (position) => ({ startPos: position, endPos: position, insertedText: `, dependencies: ${listed}` }))))),
                Match.orElse(() => []),
            ),
    );
};
const rows = Effect.fnUntraced(function* (tree: Tree) {
    const [path, workspace, { customLanguages }] = yield* Effect.all([Path.Path, openWorkspace({ workspaceRoot: tree.root, nxJsonConfiguration: readNxJsonFromDisk(tree.root) }), decodeTree(tree, Sgconfig.schema, Sgconfig.file)]);
    const [language, { outlineRules, ...registration }] = yield* Effect.fromOption(Record.findFirst(customLanguages, ({ extensions }) => extensions.includes(path.extname(SWIFT_MANIFEST).slice(1))));
    yield* Effect.sync(() => registerDynamicLanguage({ [language]: { ...registration, libraryPath: path.join(tree.root, registration.libraryPath), extensions: [...registration.extensions] } }));
    const matchers = Array.getSomes(parseAllDocuments(yield* Effect.flatMap(Effect.fromOption(outlineRules), (file) => treeText(tree, file))).map((document) => Schema.decodeUnknownOption(ImportDocument)(document.toJS())));
    const parsed = (file: string) => Effect.map(treeText(tree, file), (text) => parse(language, text).root());
    const imported = (file: string) => Effect.map(parsed(file), (root) => matchers.flatMap((matcher) => Array.getSomes(root.findAll(Struct.pick(matcher, ['rule', 'utils'])).map((node) => Option.map(Option.fromNullishOr(node.getMatch(matcher.name[1])), (held) => Array.headNonEmpty(String.split(held.text(), '.')))))));
    const pinned = ['Package.resolved'];
    const [{ targets, dependencies }, graph] = yield* Effect.all([workspace.swift, workspace.cached(Dependency, pinned, workspace.swiftPackage('show-dependencies', '--format', 'json'))]);
    const packages = yield* Effect.forEach(Array.dedupeWith(reached(graph), Equivalence.mapInput(Equivalence.String, Struct.get('identity'))), ({ identity, path: checkout }) => Effect.map(workspace.cached(SwiftPackage, pinned, workspace.swiftPackage('--package-path', checkout, 'describe', '--type', 'json')), Struct.assign({ identity })));
    const providers = Record.fromEntries(
        packages.flatMap(({ identity, targets: provided, products }) =>
            Array.getSomes(provided.map(({ name, c99name }) => Array.match(products.filter(({ targets: members }) => members.includes(name)), { onEmpty: Option.none, onNonEmpty: (owners) => Option.some([c99name, { product: Array.min(owners, Order.mapInput(Order.Number, ({ targets: members }) => members.length)).name, identity }] as const) }))),
        ),
    );
    const locals = Record.fromIterableWith(targets, ({ name, c99name }) => [c99name, name]);
    const declared = new Set(dependencies.map(Struct.get('identity')));
    const resolved = yield* Effect.forEach(targets, ({ name, c99name, path: folder, sources }) =>
        Effect.map(Effect.forEach(sources, (source) => imported(path.join(folder, source))), (modules) => {
            const owned = Array.dedupe(modules.flat()).filter((module) => module !== c99name);
            const [products, transitive] = Array.partition(
                Array.getSomes(owned.map((module) => Option.map(Record.get(providers, module), Struct.assign({ module })))),
                Result.liftPredicate(({ identity }) => declared.has(identity), ({ identity, module }) => WorkspaceError.missing(SWIFT_MANIFEST, `${identity} package, which ${name} imports through ${module}`)),
            );
            return {
                name,
                transitive,
                listed: [
                    ...Array.sort(Array.getSomes(owned.map((module) => Record.get(locals, module))), Order.String).map((local) => JSON.stringify(local)),
                    ...Array.sortWith(products, Struct.get('product'), Order.String).map(({ product, identity }) => `.product(name: ${JSON.stringify(product)}, package: ${JSON.stringify(identity)})`),
                ],
            };
        }),
    );
    yield* accumulate(resolved.flatMap(Struct.get('transitive')), Effect.fail);
    const desired = Record.fromIterableWith(resolved, ({ name, listed }) => [name, listed]);
    const root = yield* parsed(SWIFT_MANIFEST);
    const edits = root
        .findAll({ rule: { kind: 'call_expression', has: { kind: 'prefix_expression', has: { field: 'target', regex: `^(${FACTORIES.join('|')})$` } } } })
        .flatMap(rewritten(desired));
    return [{ file: SWIFT_MANIFEST, next: yield* formatted(tree, SWIFT_MANIFEST, root.commitEdits([...edits])) }];
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const swiftDependencies = syncGenerator(`${SWIFT_MANIFEST} target dependencies differ from the modules each target's sources import`, rows);

// --- [EXPORTS] -------------------------------------------------------------------------

export { swiftDependencies };
