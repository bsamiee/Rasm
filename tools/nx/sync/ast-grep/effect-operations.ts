import { createRequire } from 'node:module';
import type { Tree } from '@nx/devkit';
import { readNxJsonFromDisk } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Function, HashSet, Match, Option, Order, Path, PlatformError, Record, Result, Schema, String, Struct, UndefinedOr } from 'effect';
import { all, type Checker, ModuleKind, ModuleResolutionKind, type Signature, SignatureKind, SymbolFlags } from 'typescript/sync';
import { type ESTree, parseSync } from 'vite';
import { Document, parseDocument, Scalar } from 'yaml';
import { decodeFile, Sgconfig, Workspace } from '#/eng/typescript/manifest';
import { decodeTree, formatted, syncGenerator, treeText, YAML_LAYOUT } from '#/tools/nx/sync/tree';
import { glob, openWorkspace, WorkspaceError } from '#/tools/nx/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

interface Shape {
    readonly parameters: readonly string[];
    readonly returned: readonly string[];
}
interface Bound {
    readonly file: string;
    readonly name: string;
}
interface Loaded {
    readonly declared: ReadonlyMap<string, ESTree.Expression>;
    readonly imported: ReadonlyMap<string, Bound>;
    readonly namespaces: ReadonlyMap<string, string>;
    readonly forwarded: ReadonlyMap<string, Bound>;
    readonly locals: ReadonlyMap<string, string>;
    readonly stars: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MEMBER = 'effect-module-member';
const KINDS = ['dual', 'overloaded'] as const;
const PACKAGE = /^(effect|@effect\/[^/]+)$/u;
const OWNER = /.*\/node_modules\/(?<owner>(@[^/]+\/)?[^/]+)\//u;

// --- [MODELS] --------------------------------------------------------------------------

const Target = Schema.NullOr(Schema.Union([Schema.String, Schema.Struct({ default: Schema.optionalKey(Schema.String) })]));
const Exports = Schema.fromJsonString(Schema.Struct({ exports: Schema.Record(Schema.String, Target) }));
const Tables = Schema.Record(Schema.Literals(KINDS), Schema.Record(Schema.String, Schema.Array(Schema.String)));
const MemberUtil = Schema.Struct({ language: Schema.String, rule: Schema.Struct({ kind: Schema.String }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [MODULES]
const owned = (file: string) => Option.isSome(Option.flatMap(Option.fromNullishOr(OWNER.exec(file)?.groups?.['owner']), String.match(PACKAGE)));
const resolved = (from: string, request: string) => Option.filter(Result.getSuccess(Result.try(() => createRequire(from).resolve(request))), owned);
const modules = Effect.fnUntraced(function* (root: string, name: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const barrel = 'index';
    const manifest = createRequire(path.join(root, 'package.json')).resolve(`${name}/package.json`);
    const directory = path.dirname(manifest);
    const { exports } = yield* decodeFile(Exports, manifest);
    const entries = Array.getSomes(Record.collect(exports, (key, target) => Option.map(Option.filter(Option.fromNullishOr(typeof target === 'string' ? target : target?.default), String.endsWith('.js')), (file) => ({ key, file: file.slice(2) }))));
    const listed = yield* Effect.forEach(entries, ({ key, file }) => {
        const [prefix, ...rest] = String.split(file, '*');
        return Array.match(rest, {
            onEmpty: () => Effect.succeed([{ subpath: key, file }]),
            onNonEmpty: ([suffix]) => Effect.map(fs.glob(`${prefix}**/*${suffix}`, { root: directory }), Array.map((held) => ({ subpath: key.replace('*', held.slice(prefix.length, held.length - suffix.length)), file: held }))),
        });
    });
    return listed
        .flat()
        .map(({ subpath, file }) => ({ subpath, module: path.basename(subpath), file: path.join(directory, file) }))
        .filter(({ subpath, module, file }) => module !== barrel && path.basename(file, '.js') !== barrel && Option.contains(resolved(manifest, `${name}${subpath.slice(1)}`), file));
});

// --- [RUNTIME]
const loaded = Effect.fnUntraced(function* (file: string) {
    const fs = yield* FileSystem.FileSystem;
    const { program, module } = parseSync(file, yield* fs.readFileString(file));
    const bound = (request: string, name: string | null) => Option.flatMap(Option.fromNullOr(name), (held) => Option.map(resolved(file, request), (target): Bound => ({ file: target, name: held })));
    const imports = module.staticImports.flatMap(({ moduleRequest, entries }) => entries.map(({ localName, importName }) => ({ request: moduleRequest.value, local: localName.value, importName })));
    const exports = module.staticExports.flatMap(Struct.get('entries'));
    const record: Loaded = {
        declared: new Map(
            program.body.flatMap((statement) => {
                const declaration = statement.type === 'ExportNamedDeclaration' ? statement.declaration : statement;
                return declaration?.type === 'VariableDeclaration' ? Array.filterMap(declaration.declarations, ({ id, init }) => (id.type === 'Identifier' && init !== null ? Result.succeed([id.name, init] as const) : Result.failVoid)) : [];
            }),
        ),
        imported: new Map(Array.getSomes(imports.filter(({ importName }) => importName.kind === 'Name').map(({ request, local, importName }) => Option.map(bound(request, importName.name), (target) => [local, target] as const)))),
        namespaces: new Map(Array.getSomes(imports.filter(({ importName }) => importName.kind === 'NamespaceObject').map(({ request, local }) => Option.map(resolved(file, request), (target) => [local, target] as const)))),
        forwarded: new Map(
            Array.getSomes(
                exports
                    .filter(({ importName }) => importName.kind === 'Name')
                    .map(({ moduleRequest, importName, exportName }) => Option.flatMap(Option.all([Option.fromNullOr(moduleRequest), Option.fromNullOr(exportName.name)]), ([request, exported]) => Option.map(bound(request.value, importName.name), (target) => [exported, target] as const))),
            ),
        ),
        locals: new Map(Array.getSomes(exports.filter(({ moduleRequest }) => moduleRequest === null).map(({ localName, exportName }) => Option.all([Option.fromNullOr(exportName.name), Option.fromNullOr(localName.name)])))),
        stars: Array.getSomes(exports.filter(({ exportName, importName }) => exportName.kind === 'None' && importName.kind === 'AllButDefault').map(({ moduleRequest }) => Option.flatMap(Option.fromNullOr(moduleRequest), ({ value }) => resolved(file, value)))),
    };
    return [file, record] as const;
});
const reached = (files: readonly string[], held: ReadonlyMap<string, Loaded> = new Map()): Effect.Effect<ReadonlyMap<string, Loaded>, PlatformError.PlatformError, FileSystem.FileSystem> =>
    Array.match(
        Array.dedupe(files).filter((file) => !held.has(file)),
        {
            onEmpty: () => Effect.succeed(held),
            onNonEmpty: (fresh) =>
                Effect.flatMap(Effect.forEach(fresh, loaded, { concurrency: 'unbounded' }), (records) =>
                    reached(
                        records.flatMap(([, { imported, namespaces, forwarded, stars }]) => [...[...imported.values(), ...forwarded.values()].map(Struct.get('file')), ...namespaces.values(), ...stars]),
                        new Map([...held, ...records]),
                    ),
                ),
        },
    );
const wrapped = (graph: ReadonlyMap<string, Loaded>) => {
    const exported = (file: string, name: string, seen: HashSet.HashSet<readonly [string, string]>): boolean =>
        !HashSet.has(seen, [file, name]) &&
        UndefinedOr.match(graph.get(file), {
            onUndefined: () => false,
            onDefined: (record) => {
                const next = HashSet.add(seen, [file, name] as const);
                return UndefinedOr.match(record.forwarded.get(name), {
                    onDefined: (target) => exported(target.file, target.name, next),
                    onUndefined: () => UndefinedOr.match(record.locals.get(name), { onDefined: (local) => bound(file, local, next), onUndefined: () => record.stars.some((star) => exported(star, name, next)) }),
                });
            },
        });
    const bound = (file: string, local: string, seen: HashSet.HashSet<readonly [string, string]>): boolean =>
        UndefinedOr.match(graph.get(file), {
            onUndefined: () => false,
            onDefined: (record) =>
                UndefinedOr.match(record.imported.get(local), {
                    onDefined: (target) => exported(target.file, target.name, seen),
                    onUndefined: () => UndefinedOr.match(record.declared.get(local), { onUndefined: () => false, onDefined: (value) => wraps(file, value, seen) }),
                }),
        });
    const wraps = (file: string, value: ESTree.Expression, seen: HashSet.HashSet<readonly [string, string]>): boolean =>
        Match.value(value).pipe(
            Match.when({ type: 'CallExpression' }, ({ callee }) => (callee.type === 'Identifier' && callee.name === 'dual') || (callee.type === 'MemberExpression' && !callee.computed && callee.property.type === 'Identifier' && callee.property.name === 'dual')),
            Match.when({ type: 'Identifier' }, ({ name }) => bound(file, name, seen)),
            Match.when({ type: 'MemberExpression', computed: false }, ({ object, property }) => object.type === 'Identifier' && property.type === 'Identifier' && Option.exists(Option.fromUndefinedOr(graph.get(file)?.namespaces.get(object.name)), (target) => exported(target, property.name, seen))),
            Match.orElse(() => false),
        );
    return (file: string, name: string) => exported(file, name, HashSet.empty());
};

// --- [DECLARATIONS]
const shaped = function* (checker: Checker, signature: Signature) {
    const [parameters, returned] = yield* all(signature.getParameters.gen(), signature.getReturnType.gen());
    const curried = yield* checker.getSignaturesOfType.gen(returned, SignatureKind.Call);
    const firsts = yield* all(...curried.map((held) => held.getParameters.gen()));
    return { parameters: parameters.map(Struct.get('name')), returned: Array.getSomes(firsts.map((held) => Option.map(Array.head(held), Struct.get('name')))) } satisfies Shape;
};
const dual = (shapes: readonly Shape[]) =>
    shapes.some((curried) => Array.isReadonlyArrayNonEmpty(curried.parameters) && curried.returned.some((first) => shapes.some(({ parameters: [self, ...rest] }) => self === first && Array.isReadonlyArrayNonEmpty(rest) && rest.every((name, index) => name === curried.parameters[index]))));

// --- [TABLES]
const tables = Effect.fnUntraced(function* (tree: Tree) {
    const [path, workspace, { catalog }] = yield* Effect.all([Path.Path, openWorkspace({ workspaceRoot: tree.root, nxJsonConfiguration: readNxJsonFromDisk(tree.root) }), decodeTree(tree, Workspace.schema, Workspace.file)]);
    return yield* workspace.memoized(
        'effect-operations',
        Tables,
        ['pnpm-lock.yaml'],
        Effect.gen(function* () {
            const listed = (yield* Effect.forEach(Record.keys(catalog).filter((name) => PACKAGE.test(name)), (name) => modules(tree.root, name), { concurrency: 'unbounded' })).flat();
            const [api, graph] = yield* Effect.all([workspace.typescript, reached(listed.map(Struct.get('file')))]);
            const declarations = listed.map(({ file }) => path.join(path.dirname(file), `${path.basename(file, '.js')}.d.ts`));
            const typed = yield* Effect.acquireUseRelease(
                Effect.sync(() => api.createProgram(declarations, { module: ModuleKind.NodeNext, moduleResolution: ModuleResolutionKind.NodeNext })),
                (program) =>
                    Effect.sync(() => {
                        const { checker } = program.getProject();
                        const exported = api.batch(...Array.getSomes(checker.getSymbolOfSourceFile(declarations).map(Option.fromUndefinedOr)).map((symbol) => checker.getExportsOfModule.gen(symbol)));
                        return api.batch(
                            ...exported.map((symbols) =>
                                all(
                                    ...symbols.map(function* (symbol) {
                                        const target = (symbol.flags & SymbolFlags.Alias) !== 0 ? yield* checker.getAliasedSymbol.gen(symbol) : symbol;
                                        const signatures = yield* checker.getSignaturesOfType.gen(yield* checker.getTypeOfSymbol.gen(target), SignatureKind.Call);
                                        return { name: symbol.name, shapes: yield* all(...signatures.map((signature) => shaped(checker, signature))) };
                                    }),
                                ),
                            ),
                        );
                    }),
                (program) => Effect.sync(program.dispose),
            );
            const runtime = wrapped(graph);
            const classified = Array.zip(listed, typed).flatMap(([{ module, file }, symbols]) =>
                symbols.map(({ name, shapes }) => {
                    const both = dual(shapes) || runtime(file, name);
                    return { module, name, dual: both, overloaded: !both && shapes.length > 1 };
                }),
            );
            return Record.map(Struct.Record(KINDS, classified), (rows, kind) => Record.map(Array.groupBy(rows.filter(Struct.get(kind)), Struct.get('module')), Function.flow(Array.map(Struct.get('name')), Array.dedupe, Array.sort(Order.String))));
        }),
    );
});

// --- [WRITING]
const regex = (document: Document, text: string, block: boolean) => Object.assign(document.createNode({ regex: Object.assign(new Scalar(text), { type: block ? Scalar.BLOCK_LITERAL : Scalar.QUOTE_SINGLE }) }), { flow: !block });
const arm = (document: Document, modules: readonly string[], members: readonly string[]) => {
    const listed = members.length > 1;
    const module = regex(document, `(?-u:\\b)${modules.length > 1 ? `(${modules.join('|')})` : modules.join('')}$`, false);
    const member = regex(document, `${listed ? '(?x)' : ''}^(${members.join(listed ? '\n|' : '|')})$`, listed);
    return document.createNode({ matches: Object.assign(document.createNode({ [MEMBER]: { module, member } }), { flow: !listed }) });
};
const rows = Effect.fnUntraced(function* (tree: Tree) {
    const [path, { utilDirs }] = yield* Effect.all([Path.Path, decodeTree(tree, Sgconfig.schema, Sgconfig.file)]);
    const util = yield* Effect.flatMap(glob(tree.root, utilDirs.map((dir) => `${dir}/**/${MEMBER}.yml`)), (found) => Effect.fromOption(Array.head(found), () => WorkspaceError.missing(Sgconfig.file, `${MEMBER} util under ${utilDirs.join(', ')}`)));
    const { language, rule } = yield* Schema.decodeUnknownEffect(MemberUtil)(parseDocument(yield* treeText(tree, util)).toJS());
    const held = yield* tables(tree);
    return yield* Effect.forEach(KINDS, (kind) => {
        const id = `effect-${kind}-operation`;
        const document = new Document();
        const grouped = Object.values(Array.groupBy(Record.toEntries(held[kind]), ([, members]) => members.join('|')));
        const arms = Array.sortWith(grouped, ([[first]]) => first, Order.String).map((group) => arm(document, Array.sort(group.map(([module]) => module), Order.String), Array.headNonEmpty(group)[1]));
        document.value = document.createNode({ id, language, rule: { kind: rule.kind, any: arms } });
        const file = path.join(path.dirname(util), `${id}.yml`);
        return Effect.map(formatted(tree, file, document.toString(YAML_LAYOUT)), (next) => ({ file, next }));
    });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const effectOperations = syncGenerator(`Effect operation utils differ from the duals and overloads of the ${Workspace.file} catalog effect packages`, rows);

// --- [EXPORTS] -------------------------------------------------------------------------

export { effectOperations };
