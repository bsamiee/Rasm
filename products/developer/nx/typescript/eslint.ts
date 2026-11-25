import { createRequire } from 'node:module';
import { join, posix, relative } from 'node:path';
import { pathToFileURL } from 'node:url';
import { hashArray, normalizePath, type PackageManager, type TargetConfiguration } from '@nx/devkit';
import { findProjectForPath, getFilesInDirectoryUsingContext, getLockFileName, isRelativePath, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { above, EXEC, packaged, stamped } from '@rasm/nx';
import { Array, Cause, Effect, Filter, Option, Record, Result, Schema, Stream } from 'effect';
import type { ESLint } from 'eslint';
import { connected } from '#/programs';
import { MANIFEST } from '#/workspace';

// --- [MODELS] --------------------------------------------------------------------------

const Literal = Schema.Struct({ text: Schema.String }).annotate({ description: 'A module specifier literal of a source file the native TypeScript API parses' });
const Calculated = Schema.Struct({
    languageOptions: Schema.Struct({ parserOptions: Schema.Record(Schema.String, Schema.Unknown).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    settings: Schema.Record(Schema.String, Schema.Unknown).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
}).annotate({
    description: 'The parser options and shared settings of the configuration ESLint calculates for one file: a project or projectService parser option builds a TypeScript program over the tsconfig chain and upstream sources, and shared settings carry the resolvers rules read other files through',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const imported = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, configs: Array.NonEmptyReadonlyArray<string>) {
    const api = yield* connected(workspaceRoot, compiler, Array.headNonEmpty(configs));
    const specified = (file: string) =>
        Effect.map(Effect.acquireDisposable(Effect.try({ try: () => api.createSourceFileFromFile(file), catch: (cause) => new Cause.UnknownError(cause, `${compiler}/sync parses no ${file}`) })), ({ sourceFile }) =>
            Array.filterMap(sourceFile.imports, Filter.fromPredicateOption(Schema.decodeUnknownOption(Literal))).map(({ text }) => [text, join(workspaceRoot, file)] as const),
        );
    const reached = Effect.fnUntraced(function* ({ pending, seen }: { readonly pending: readonly string[]; readonly seen: ReadonlySet<string> }) {
        const found = yield* Effect.forEach(pending, specified);
        const next = new Set(Array.filterMap(found.flat(), ([text, from]) => (isRelativePath(text) ? Result.succeed(posix.join(posix.dirname(normalizePath(relative(workspaceRoot, from))), text)) : Result.failVoid))).difference(seen);
        return [Array.zip(pending, found), Option.liftPredicate({ pending: [...next], seen: seen.union(next) }, () => next.size > 0)] as const;
    });
    return yield* Effect.forEach(
        configs,
        Effect.fnUntraced(function* (config) {
            const visited = yield* Stream.paginate({ pending: [config], seen: new Set([config]) }, reached).pipe(Stream.runCollect);
            const files = visited.map(([file]) => file);
            const { packages, linked, folders } = yield* packaged(visited.flatMap(([, imports]) => imports));
            return [config, { config, files, packages: Array.dedupe(['eslint', ...packages]), linked, read: [...files, ...folders.map((folder) => posix.join(normalizePath(relative(workspaceRoot, folder)), '**/*'))] }] as const;
        }),
    );
}, Effect.scoped);

const inspected = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, manager: PackageManager, rows: Array.NonEmptyReadonlyArray<{ readonly root: string; readonly scope: string; readonly owned: readonly string[] }>) {
    const unanswered = (cause: unknown) => new Cause.UnknownError(cause, `eslint resolved from ${workspaceRoot} answers no config or ignore question`);
    const { ESLint: Linter } = yield* Effect.tryPromise({ try: (): Promise<{ readonly ESLint: typeof ESLint }> => import(pathToFileURL(createRequire(join(workspaceRoot, MANIFEST)).resolve('eslint')).href), catch: unanswered });
    const eslint = new Linter({ cwd: workspaceRoot });
    const linting = (file: string) => Effect.map(Effect.tryPromise({ try: () => eslint.isPathIgnored(join(workspaceRoot, file)), catch: unanswered }), (ignored) => !ignored);
    const reading = (file: string) =>
        Effect.tryPromise({ try: (): Promise<unknown> => eslint.calculateConfigForFile(join(workspaceRoot, file)), catch: unanswered }).pipe(
            Effect.flatMap(Schema.decodeUnknownEffect(Calculated)),
            Effect.map(({ languageOptions, settings }) => [languageOptions.parserOptions['project'], languageOptions.parserOptions['projectService']].some(Boolean) || !Record.isEmptyRecord(settings)),
        );
    const governed = yield* Effect.forEach(
        rows,
        Effect.fnUntraced(function* ({ root, scope, owned }) {
            const config = Option.map(Option.fromUndefinedOr(yield* Effect.tryPromise({ try: () => eslint.findConfigFile(join(workspaceRoot, root, MANIFEST)), catch: unanswered })), (at) => normalizePath(relative(workspaceRoot, at)));
            const linted = yield* Effect.filter(Option.isSome(config) ? owned : [], linting, { concurrency: 'unbounded' });
            return { root, scope, config, lintable: Array.isReadonlyArrayNonEmpty(linted), typed: Option.isSome(yield* Effect.findFirst(linted, reading)) };
        }),
        { concurrency: 'unbounded' },
    );
    const imports = new Map(
        yield* Array.match(Array.dedupe(Array.getSomes(governed.map(({ config }) => config))), {
            onEmpty: () => Effect.succeed([]),
            onNonEmpty: (configs) => imported(workspaceRoot, compiler, configs),
        }),
    );
    return new Map(
        governed.map(({ root, scope, config, lintable, typed }) => {
            const held = Option.flatMapNullishOr(config, (at) => imports.get(at));
            const target = Option.map(lintable ? held : Option.none(), ({ config: at, files, packages, linked }): TargetConfiguration => ({
                command: root === scope ? 'eslint .' : `eslint ./${scope}`,
                options: { cwd: root },
                cache: true,
                inputs: ['default', ...(typed ? ['^default'] : []), ...Array.cartesianWith(['{workspaceRoot}'], files, posix.join), ...Array.match(linked, { onEmpty: () => [], onNonEmpty: (projects) => [{ input: 'default', projects: [...projects] }] }), { externalDependencies: [...packages] }],
                outputs: [],
                metadata: { description: `Lints the files ${at} governs in the project with eslint`, help: { command: `${EXEC[manager]} eslint --help`, example: { args: ['--max-warnings', '0'] } } },
            }));
            return [root, { read: Array.fromOption(held).flatMap(({ read }) => read), targets: Option.toArray(target), typed }] as const;
        }),
    );
});

const linted = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, manager: PackageManager, roots: readonly { readonly root: string; readonly scope: string }[], hashed: string) {
    const cache = new PluginCache<{ readonly targets: readonly TargetConfiguration[]; readonly typed: boolean; readonly read: readonly string[]; readonly stamp: string; readonly digest: string }>(join(workspaceDataDirectory, 'typescript-eslint.hash'));
    const owners = new Map(roots.map(({ root }) => [root, root] as const));
    const globs = roots.flatMap(({ root }) => [[getLockFileName(manager), ...Array.cartesianWith(above(root), ['eslint.config.js', 'eslint.config.mjs', 'eslint.config.cjs', 'eslint.config.ts', 'eslint.config.mts', 'eslint.config.cts'], posix.join)], [...(cache.get(root)?.read ?? [])]]);
    const [listed, hashes] = yield* Effect.all([Effect.forEach(roots, ({ scope }) => Effect.promise(() => getFilesInDirectoryUsingContext(workspaceRoot, scope)), { concurrency: 'unbounded' }), Effect.forEach(globs, stamped(workspaceRoot), { concurrency: 'unbounded' })], { concurrency: 'unbounded' });
    const keyed = Array.zipWith(Array.zip(roots, listed), Array.chunksOf(hashes, 2), ([{ root, scope }, beneath], [configured, stamp]) => {
        const owned = beneath.filter((file) => (findProjectForPath(file, owners) ?? '.') === root);
        const digest = hashArray([hashed, configured, ...owned]);
        return { root, scope, owned, digest, current: Option.exists(Option.fromNullishOr(cache.get(root)), (entry) => entry.digest === digest && entry.stamp === stamp) };
    });
    const missed = keyed.filter(({ current }) => !current);
    const fresh = yield* Array.match(missed, { onEmpty: () => Effect.succeed(new Map<string, { readonly read: readonly string[]; readonly targets: readonly TargetConfiguration[]; readonly typed: boolean }>()), onNonEmpty: (rows) => inspected(workspaceRoot, compiler, manager, rows) });
    const stored = Array.getSomes(missed.map(({ root, digest }) => Option.map(Option.fromUndefinedOr(fresh.get(root)), (held) => ({ root, digest, ...held }))));
    const stamps = yield* Effect.forEach(stored, ({ read }) => stamped(workspaceRoot)(read), { concurrency: 'unbounded' });
    yield* Effect.forEach(Array.zip(stored, stamps), ([{ root, digest, read, targets, typed }, stamp]) => Effect.sync(() => cache.set(root, { targets, typed, read, stamp, digest })), { discard: true });
    yield* Array.isReadonlyArrayNonEmpty(missed) ? Effect.sync(() => cache.writeToDisk()) : Effect.void;
    return new Map(Array.getSomes(keyed.map(({ root }) => Option.map(Option.all({ target: Array.head(cache.get(root)?.targets ?? []), typed: Option.fromUndefinedOr(cache.get(root)?.typed) }), (held) => [root, held] as const))));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { linted };
