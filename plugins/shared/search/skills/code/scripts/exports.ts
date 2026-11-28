import { findPackageJSON } from 'node:module';
import { dirname, join, posix, relative, resolve } from 'node:path';
import process from 'node:process';
import { pathToFileURL } from 'node:url';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { isExportDeclaration, isImportDeclaration, isVariableDeclaration, type Node, type SourceFile, SyntaxKind } from '@typescript/native/ast';
import { API, type Checker, ModuleKind, SymbolFlags, type Symbol as TypeScriptSymbol } from '@typescript/native/sync';
import { Array, Console, Effect, FileSystem, Number, Option, Predicate, Result, Schema, String, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const OPTIONS = ['--module', 'nodenext', '--moduleResolution', 'nodenext', '--skipLibCheck', '--noEmit', '--types', ''] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Arguments = Schema.Tuple([Schema.NonEmptyString, Schema.NonEmptyString]);
const Pattern = Schema.TemplateLiteralParser([Schema.String, '*', Schema.String]);
const Subpaths = Schema.Record(Schema.TemplateLiteral(['.', Schema.String]), Schema.Unknown);
const Manifest = Schema.fromJsonString(
    Schema.Struct({
        type: Schema.Union([Schema.Literal('module').transform(ModuleKind.ESNext), Schema.Literal('commonjs').transform(ModuleKind.CommonJS)]).pipe(Schema.withDecodingDefaultKey(Effect.succeed('commonjs'))),
        exports: Schema.optionalKey(Schema.Unknown),
    }),
);

// --- [ERRORS] --------------------------------------------------------------------------

class ExportsError extends Schema.TaggedError<ExportsError, { readonly brand: unique symbol }>()('ExportsError', {
    reason: Schema.TaggedUnion({ Unresolved: {}, Empty: {} }),
    subject: Schema.String,
    message: Schema.String,
}) {
    static readonly of = (reason: ExportsError['reason'], subject: string) => () => ExportsError.make({ reason, subject, message: `${subject} ${String.toLowerCase(reason._tag)}` });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const leaves = (value: unknown): readonly string[] => (Predicate.isObject(value) ? Object.values(value).flatMap(leaves) : Array.filter([value], Predicate.isString));
const firstLine = (text: string) => Array.findFirst(text.split('\n'), (line) => String.isNonEmpty(line.trim()));
const statement = (node: Node) => (isVariableDeclaration(node) ? node.parent.parent : node);
const header = (file: SourceFile) =>
    Option.fromUndefinedOr(file.statements[0]).pipe(
        Option.filter((first) => isImportDeclaration(first) || isExportDeclaration(first) || (first.jsDoc?.length ?? 0) > 1),
        Option.flatMapNullishOr((first) => first.jsDoc?.[0]),
        Option.flatMap((doc) => firstLine(doc.comment.map((part) => part.text).join(''))),
    );
const declarations = (symbol: TypeScriptSymbol) => Array.filterMap(symbol.declarations, (handle) => Result.fromOption(Option.fromUndefinedOr(handle.resolve()), () => handle.path));
const row = (checker: Checker, exported: TypeScriptSymbol, symbol: TypeScriptSymbol) => {
    const nodes = declarations(symbol);
    return Option.map(Array.head(nodes), (first) => {
        const file = first.getSourceFile();
        const held = nodes.filter((node) => node.getSourceFile() === file);
        const spanned = [...held, ...(exported === symbol ? [] : declarations(exported))].filter((node) => node.getSourceFile() === file).map(statement);
        const line = (position: number) => file.getLineAndCharacterOfPosition(position).line + 1;
        const span = `${relative(process.cwd(), file.fileName)}:${line(Math.min(...spanned.map((node) => node.getStart(file, true))))}-${line(Math.max(...spanned.map((node) => node.getEnd())))}`;
        const summary = Option.orElse(firstLine(checker.getDocumentationCommentOfSymbol(exported)), () => firstLine(checker.getDocumentationCommentOfSymbol(symbol)));
        return [exported.name, Array.dedupe(held.map((node) => SyntaxKind[node.kind])).join('+'), span, Option.getOrElse(summary, () => '')].join('\t');
    });
};
const walk = (
    checker: Checker,
    entry: string,
    label: string,
    module: TypeScriptSymbol,
    ancestors: ReadonlySet<string>,
): readonly { readonly entry: string; readonly label: string; readonly key: string; readonly summary: Option.Option<string>; readonly rows: readonly string[]; readonly unresolved: readonly string[] }[] =>
    Option.match(Array.head(module.declarations), {
        onNone: () => [],
        onSome: (handle) => {
            const [modules, members] = Array.partition(checker.getExportsOfModule(module), (each) => {
                const target = (each.flags & SymbolFlags.Alias) === 0 ? each : checker.getAliasedSymbol(each);
                return target.declarations[0]?.kind === SyntaxKind.SourceFile ? Result.succeed({ name: each.name, target }) : Result.fail({ each, target });
            });
            const [rows, unresolved] = Array.partition(members, ({ each, target }) => Result.fromOption(row(checker, each, target), () => each.name));
            const held = new Set([...ancestors, handle.path]);
            return [
                { entry, label, key: handle.path, summary: Option.flatMap(Option.fromUndefinedOr(handle.resolve()), (node) => header(node.getSourceFile())), rows, unresolved },
                ...modules.filter(({ target }) => Option.exists(Array.head(target.declarations), ({ path }) => !held.has(path))).flatMap(({ name, target }) => walk(checker, entry, `${label}.${name}`, target, held)),
            ];
        },
    });
const subpaths = Effect.fnUntraced(function* (root: string, exports: unknown) {
    const fs = yield* FileSystem.FileSystem;
    const [wildcards, literals] = Array.partition(Object.entries(Option.getOrElse(Schema.decodeUnknownOption(Subpaths)(exports), () => ({ '.': exports }))), ([key, value]) =>
        Result.fromOption(
            Option.map(Schema.decodeUnknownOption(Pattern)(key), ([head, , tail]) => ({ head, tail, value })),
            () => ({ key, value }),
        ),
    );
    const targets = wildcards.flatMap(({ head, tail, value }) => leaves(value).map((leaf) => ({ head, tail, leaf })));
    const patterns = Array.getSomes(targets.map(({ head, tail, leaf }) => Option.map(Schema.decodeUnknownOption(Pattern)(leaf), ([from, , to]) => ({ head, tail, from, to }))));
    const globbed = yield* Effect.forEach(patterns, ({ head, tail, from, to }) =>
        fs.glob(posix.join(from, '**', `*${to}`), { root }).pipe(
            Effect.map(
                Array.map((match) => {
                    const star = posix.relative(from, match);
                    return `${head}${star.slice(0, star.length - to.length)}${tail}`;
                }),
            ),
        ),
    );
    return [...literals.filter(({ value }) => value !== null).map(Struct.get('key')), ...globbed.flat()];
});
const list = Effect.fnUntraced(function* (project: string, specifier: string) {
    const fs = yield* FileSystem.FileSystem;
    const read = (path: string) => Effect.flatMap(fs.readFileString(path), Schema.decodeEffect(Manifest));
    const { type } = yield* read(join(project, 'package.json'));
    const api = yield* Effect.acquireDisposable(Effect.sync(() => new API({ cwd: project })));
    const { options } = api.parseCommandLine(OPTIONS);
    const resolver = api.createModuleResolver(options);
    const locate = (each: string) => Option.fromUndefinedOr(resolver.resolveModuleName(each, project, type).resolvedModule);
    const unresolved = (subject: string) => ExportsError.of({ _tag: 'Unresolved' }, `${subject} from ${project}`);
    const { name, version } = yield* Effect.fromOption(Option.flatMapNullishOr(locate(specifier), Struct.get('packageId')), unresolved(specifier));
    const specifiers = yield* name === specifier
        ? Effect.fromOption(Option.fromUndefinedOr(findPackageJSON(name, pathToFileURL(join(project, 'package.json')))), unresolved(`${name}/package.json`)).pipe(
              Effect.flatMap((path) => Effect.flatMap(read(path), ({ exports }) => subpaths(dirname(path), exports))),
              Effect.map((found) => found.map((subpath) => posix.join(name, subpath))),
          )
        : Effect.succeed(Array.empty<string>());
    const located: readonly { readonly entry: string; readonly file: string }[] = Array.filterMap(Array.dedupe([specifier, ...specifiers]), (each) =>
        Result.fromOption(
            Option.map(locate(each), ({ resolvedFileName }) => ({ entry: each, file: resolvedFileName })),
            () => each,
        ),
    );
    const entries = Array.dedupeWith(located, (left, right) => left.file === right.file);
    const files = entries.map(Struct.get('file'));
    const { checker } = api.createProgram(files, options).getProject();
    const groups = Array.dedupeWith(
        Array.zip(entries, checker.getSymbolOfSourceFile(files)).flatMap(([{ entry }, module]) => (module === undefined ? [] : walk(checker, entry, entry, module, new Set()))),
        (left, right) => left.key === right.key,
    ).filter(({ rows }) => Array.isReadonlyArrayNonEmpty(rows));
    const members = Number.sumAll(groups.map(({ rows }) => rows.length));
    const folder = join('.artifacts', 'search', 'exports', `${name}@${version}`);
    yield* Effect.when(ExportsError.of({ _tag: 'Empty' }, specifier)(), Effect.succeed(members === 0));
    yield* Effect.forEach(groups, ({ label, rows }) => Effect.andThen(fs.makeDirectory(dirname(join(folder, label)), { recursive: true }), fs.writeFileString(join(folder, `${label}.tsv`), `${rows.join('\n')}\n`)), { discard: true });
    const [listed, others] = Array.partition(groups, (group) => (group.entry === specifier ? Result.succeed(group) : Result.fail(group)));
    const rest = Object.entries(Array.groupBy(others, Struct.get('entry')));
    return [
        `exports: ${name}@${version}`,
        `folder: ${folder}`,
        `members: ${members}`,
        ...Array.match(groups.flatMap(Struct.get('unresolved')), { onEmpty: () => [], onNonEmpty: (names) => [`unresolved: ${names.join(', ')}`] }),
        `groups: ${listed.length}`,
        ...listed.map(({ label, rows, summary }) => `    ${[label, rows.length, Option.getOrElse(summary, () => '')].join('\t')}`),
        ...Array.match(rest, { onEmpty: () => [], onNonEmpty: () => [`entries: ${rest.length}`] }),
        ...rest.map(([entry, held]) => `    ${[entry, held.length, Number.sumAll(held.map(({ rows }) => rows.length))].join('\t')}`),
    ].join('\n');
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

Schema.decodeUnknownEffect(Arguments)(process.argv.slice(2)).pipe(
    Effect.flatMap(([project, specifier]) => list(resolve(project), specifier)),
    Effect.flatMap(Console.log),
    Effect.provide(layer),
    runMain(),
);
