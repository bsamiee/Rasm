import { createRequire } from 'node:module';
import { join, posix, relative } from 'node:path';
import { pathToFileURL } from 'node:url';
import { hashArray, normalizePath, type PackageManager, type TargetConfiguration } from '@nx/devkit';
import { findProjectForPath, getFilesInDirectoryUsingContext, getLockFileName, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { above, EXEC, packaged, tokened } from '@rasm/nx-distribution/inference';
import { Array, Cause, Effect, Option, Schema, type Scope } from 'effect';
import type { ESLint } from 'eslint';
import { hashMultiGlobWithWorkspaceContext } from 'nx/src/devkit-internals';
import { connected } from '#/programs';
import { MANIFEST } from '#/workspace';

// --- [MODELS] --------------------------------------------------------------------------

const Literal = Schema.Struct({ text: Schema.String }).annotate({ description: 'A module specifier literal of a source file the native TypeScript API parses' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const imported = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, configs: Array.NonEmptyReadonlyArray<string>) {
    const api = yield* connected(workspaceRoot, compiler, Array.headNonEmpty(configs));
    const specified = (file: string) =>
        Effect.map(Effect.acquireDisposable(Effect.try({ try: () => api.createSourceFileFromFile(file), catch: (cause) => new Cause.UnknownError(cause, `${compiler}/sync parses no ${file}`) })), ({ sourceFile }) =>
            Array.getSomes(sourceFile.imports.map((node) => Schema.decodeUnknownOption(Literal)(node))).map(({ text }) => [text, join(workspaceRoot, file)] as const),
        );
    const reached = (pending: readonly string[], files: ReadonlySet<string>, imports: readonly (readonly [string, string])[]): Effect.Effect<{ readonly files: readonly string[]; readonly imports: readonly (readonly [string, string])[] }, Cause.UnknownError, Scope.Scope> =>
        Array.match(
            pending.filter((at) => !files.has(at)),
            {
                onEmpty: () => Effect.succeed({ files: [...files], imports }),
                onNonEmpty: (fresh) =>
                    Effect.flatMap(Effect.forEach(fresh, specified), (found) =>
                        reached(
                            found.flat().flatMap(([text, from]) => (text.startsWith('.') ? [posix.join(posix.dirname(normalizePath(relative(workspaceRoot, from))), text)] : [])),
                            new Set([...files, ...fresh]),
                            [...imports, ...found.flat()],
                        ),
                    ),
            },
        );
    return yield* Effect.forEach(
        configs,
        Effect.fnUntraced(function* (config) {
            const { files, imports } = yield* reached([config], new Set(), []);
            const { packages, linked, folders } = yield* packaged(imports);
            return [config, { config, files, packages: Array.dedupe(['eslint', ...packages]), linked, read: [...files, ...folders.map((folder) => posix.join(normalizePath(relative(workspaceRoot, folder)), '**/*'))] }] as const;
        }),
    );
}, Effect.scoped);

const inspected = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, manager: PackageManager, rows: Array.NonEmptyReadonlyArray<{ readonly root: string; readonly scope: string; readonly owned: readonly string[] }>) {
    const unanswered = (cause: unknown) => new Cause.UnknownError(cause, `eslint resolved from ${workspaceRoot} answers no config or ignore question`);
    const { ESLint: Linter } = yield* Effect.tryPromise({ try: (): Promise<{ readonly ESLint: typeof ESLint }> => import(pathToFileURL(createRequire(join(workspaceRoot, MANIFEST)).resolve('eslint')).href), catch: unanswered });
    const eslint = new Linter({ cwd: workspaceRoot });
    const linting = (file: string) => Effect.map(Effect.tryPromise({ try: () => eslint.isPathIgnored(join(workspaceRoot, file)), catch: unanswered }), (ignored) => !ignored);
    const governed = yield* Effect.forEach(
        rows,
        Effect.fnUntraced(function* ({ root, scope, owned }) {
            const config = Option.map(Option.fromUndefinedOr(yield* Effect.tryPromise({ try: () => eslint.findConfigFile(join(workspaceRoot, root, MANIFEST)), catch: unanswered })), (at) => normalizePath(relative(workspaceRoot, at)));
            const lintable = Option.isSome(yield* Effect.findFirst(Option.isSome(config) ? owned : [], linting));
            return { root, scope, config, lintable };
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
        governed.map(({ root, scope, config, lintable }) => {
            const held = Option.flatMapNullishOr(config, (at) => imports.get(at));
            const target = Option.map(
                Option.filter(held, () => lintable),
                ({ config: at, files, packages, linked }): TargetConfiguration => ({
                    command: root === scope ? 'eslint .' : `eslint ./${scope}`,
                    options: { cwd: root },
                    cache: true,
                    inputs: ['default', '^default', ...files.map(tokened(root)), ...Array.match(linked, { onEmpty: () => [], onNonEmpty: (projects) => [{ input: 'default', projects: [...projects] }] }), { externalDependencies: [...packages] }],
                    outputs: [],
                    metadata: { description: `Lints the files ${at} governs in the project with eslint`, help: { command: `${EXEC[manager]} eslint --help`, example: { args: ['--max-warnings', '0'] } } },
                }),
            );
            return [root, { read: Array.fromOption(held).flatMap(({ read }) => read), targets: Option.toArray(target) }] as const;
        }),
    );
});

const linted = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, manager: PackageManager, roots: readonly { readonly root: string; readonly scope: string }[], hashed: string) {
    const cache = new PluginCache<{ readonly targets: readonly TargetConfiguration[]; readonly read: readonly string[]; readonly stamp: string; readonly digest: string }>(join(workspaceDataDirectory, 'typescript-eslint.hash'));
    const owners = new Map(roots.map(({ root }) => [root, root] as const));
    const [listed, hashes] = yield* Effect.all(
        [
            Effect.forEach(roots, ({ scope }) => Effect.promise(() => getFilesInDirectoryUsingContext(workspaceRoot, scope)), { concurrency: 'unbounded' }),
            Effect.promise(() =>
                hashMultiGlobWithWorkspaceContext(
                    workspaceRoot,
                    roots.flatMap(({ root }) => [[getLockFileName(manager), ...Array.cartesianWith(above(root), ['eslint.config.js', 'eslint.config.mjs', 'eslint.config.cjs', 'eslint.config.ts', 'eslint.config.mts', 'eslint.config.cts'], posix.join)], [...(cache.get(root)?.read ?? [])]]),
                ),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const keyed = Array.zipWith(Array.zip(roots, listed), Array.chunksOf(hashes, 2), ([{ root, scope }, beneath], [configured, stamp]) => {
        const owned = beneath.filter((file) => (findProjectForPath(file, owners) ?? '.') === root);
        const digest = hashArray([hashed, configured, ...owned]);
        return { root, scope, owned, digest, current: Option.exists(Option.fromNullishOr(cache.get(root)), (entry) => entry.digest === digest && entry.stamp === stamp) };
    });
    const missed = keyed.filter(({ current }) => !current);
    const fresh = yield* Array.match(missed, { onEmpty: () => Effect.succeed(new Map<string, { readonly read: readonly string[]; readonly targets: readonly TargetConfiguration[] }>()), onNonEmpty: (rows) => inspected(workspaceRoot, compiler, manager, rows) });
    const stored = Array.getSomes(missed.map(({ root, digest }) => Option.map(Option.fromUndefinedOr(fresh.get(root)), ({ read, targets }) => ({ root, digest, read, targets }))));
    const stamps = yield* Effect.promise(() =>
        hashMultiGlobWithWorkspaceContext(
            workspaceRoot,
            stored.map(({ read }) => [...read]),
        ),
    );
    yield* Effect.forEach(Array.zip(stored, stamps), ([{ root, digest, read, targets }, stamp]) => Effect.sync(() => cache.set(root, { targets, read, stamp, digest })), { discard: true });
    yield* Array.isReadonlyArrayNonEmpty(missed) ? Effect.sync(() => cache.writeToDisk()) : Effect.void;
    return new Map(Array.getSomes(keyed.map(({ root }) => Option.map(Array.head(cache.get(root)?.targets ?? []), (target) => [root, target] as const))));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { linted };
