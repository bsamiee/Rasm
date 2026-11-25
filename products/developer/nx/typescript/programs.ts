import { createRequire } from 'node:module';
import { basename, dirname, extname, join, posix, relative, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { hashArray, normalizePath, type PackageManager, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { findProjectForPath, getFilesInDirectoryUsingContext, getLockFileName, globWithWorkspaceContext, hashFile, hashWithWorkspaceContext, PluginCache, type ProjectRootMappings, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { type Anchor, EXEC, EXECUTORS, inside, type Stated, tokened, type Verbs, within } from '@rasm/nx';
import type { API, ParsedCommandLine } from '@typescript/native/sync';
import { Array, Cause, Effect, FileSystem, Function, Option, Predicate, Result, UndefinedOr } from 'effect';
import { MANIFEST, projected } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Result.Result.Success<ReturnType<typeof resolved>>;
type Program = Resolved & { readonly ignored: readonly string[] };

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'tsconfig.json';

// --- [OPERATIONS] ----------------------------------------------------------------------

const referenced = ({ projectReferences = [] }: ParsedCommandLine) => projectReferences.map(({ path: at }) => (extname(at) === '.json' ? at : join(at, CONFIG)));

const connected = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, from: string) {
    const native = yield* Effect.tryPromise({ try: (): Promise<{ readonly API: typeof API }> => import(pathToFileURL(createRequire(resolve(workspaceRoot, from)).resolve(`${compiler}/sync`)).href), catch: (cause) => new Cause.UnknownError(cause, `${workspaceRoot} resolves no ${compiler}/sync`) });
    return yield* Effect.acquireDisposable(Effect.sync(() => new native.API({ cwd: workspaceRoot })));
});

const solved = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, owners: ProjectRootMappings, configs: Array.NonEmptyReadonlyArray<string>) {
    const unopened = (cause: unknown) => new Cause.UnknownError(cause, `${compiler}/sync opens no snapshot of ${workspaceRoot}`);
    const api = yield* connected(workspaceRoot, compiler, Array.headNonEmpty(configs));
    const fs = yield* FileSystem.FileSystem;
    const linked = (at: string) => UndefinedOr.liftThrowable((file: string) => referenced(api.parseConfigFile(file)))(at) ?? [];
    const reached = (pending: readonly string[], seen: ReadonlySet<string>): ReadonlySet<string> =>
        Array.match(
            pending.filter((at) => !seen.has(at)),
            { onEmpty: () => seen, onNonEmpty: (fresh) => reached(fresh.flatMap(linked), new Set([...seen, ...fresh])) },
        );
    const parsed = (config: string) => {
        const file = resolve(workspaceRoot, config);
        const read = api.parseConfigFile(file);
        const [owned, external] = Array.partition(referenced(read), (at) => Result.liftPredicate(at, () => (findProjectForPath(relative(workspaceRoot, at), owners) ?? '.') === dirname(config), Function.identity));
        return { entry: { file, parsed: read, chain: [file], loaded: read.fileNames }, owned, external: [...reached([...(Array.isReadonlyArrayEmpty(read.fileNames) ? [] : external), ...owned.flatMap(linked)], new Set([file, ...owned]))].filter((at) => !(at === file || owned.includes(at))) };
    };
    const reads = configs.map((config) => [config, Result.try({ try: () => parsed(config), catch: (cause) => new Cause.UnknownError(cause, `${config} does not parse as a tsconfig`) })] as const);
    const files = yield* Effect.filter(Array.dedupe(Array.getSuccesses(reads.map(([, read]) => read)).flatMap(({ entry, owned, external }) => [entry.file, ...owned, ...external])), (at) => fs.exists(at));
    const snapshot = yield* Effect.acquireDisposable(Effect.try({ try: () => api.createSnapshot({ openProjects: files }), catch: unopened }));
    const configured = (file: string) => Option.map(Option.fromNullishOr(snapshot.getConfiguredProject(file)), (project) => [file, { file, parsed: project.parsedCommandLine, chain: project.program.getConfigFileNames(), loaded: project.program.getSourceFileNames() }] as const);
    const opened = new Map(yield* Effect.try({ try: () => Array.getSomes(files.map(configured)), catch: unopened }));
    const members = (listed: readonly string[]) => Array.getSomes(listed.map((file) => Option.fromUndefinedOr(opened.get(file))));
    return reads.map(([config, read]) => [config, Result.map(read, ({ entry, owned, external }) => ({ config, entry: opened.get(entry.file) ?? entry, members: members(owned), external: members(external) }))] as const);
}, Effect.scoped);

const resolved = (workspaceRoot: string, solution: Effect.Success<ReturnType<typeof solved>>[number][1]) =>
    Result.map(solution, ({ config, entry, members, external }) => {
        const folder = join(workspaceRoot, dirname(config));
        const local = (at: string) => normalizePath(relative(workspaceRoot, at));
        const holding = [entry, ...members].filter(({ parsed }) => parsed.fileNames.length > 0);
        const declared = new Set(external.flatMap(({ parsed }) => parsed.fileNames).filter((at) => !/\.(?:d\.[cm]?ts|json)$/u.test(at)));
        const checked = entry.parsed.fileNames.length > 0 || (Array.isReadonlyArrayEmpty(holding) && Array.isReadonlyArrayEmpty(entry.parsed.projectReferences ?? [])) ? [entry] : holding;
        const configs = checked.map(({ file }) => relative(folder, file));
        const placed = holding.map(({ file, loaded, parsed: { options, projectReferences = [] } }) => {
            const offset = options.outDir === undefined || options.rootDir === undefined ? '.' : relative(options.rootDir, dirname(file));
            const base = join(options.outDir ?? dirname(file), inside('.')(offset) ? offset : '.');
            const scripts = options.emitDeclarationOnly === true ? [] : [join(base, `**/*.{js,cjs,mjs,jsx}${options.sourceMap === true ? '{,.map}' : ''}`), ...(loaded.some((at) => extname(at) === '.json') ? [join(base, '**/*.json')] : [])];
            const declarations = options.declaration === true || options.composite === true ? [join(options.declarationDir ?? base, `**/*.{d.ts,d.cts,d.mts}${options.declarationMap === true ? '{,.map}' : ''}`)] : [];
            return {
                emitted: options.noEmit === true ? [] : [...scripts, ...declarations].map(local),
                informed: options.incremental === true || options.composite === true || projectReferences.length > 0 ? [local(options.tsBuildInfoFile ?? join(resolve(options.outDir ?? dirname(file), offset), `${basename(file, '.json')}.tsbuildinfo`))] : [],
            };
        });
        return {
            commands: Array.zipWith(checked, configs, ({ parsed: { projectReferences = [] } }, at) => ['tsc', ...(projectReferences.length > 0 ? ['--build'] : []), ...[at].filter((named) => named !== CONFIG).flatMap((named) => (projectReferences.length > 0 ? [named] : ['-p', named]))].map(quoteShellArg).join(' ')),
            configs,
            outputs: Array.dedupe(placed.flatMap(({ emitted }) => emitted)),
            informed: Array.dedupe(placed.flatMap(({ informed }) => informed)),
            chain: entry.chain.map(local).filter(within),
            sources: Array.dedupe([...[entry, ...holding].flatMap(({ chain, parsed, loaded }) => [...chain, ...parsed.fileNames, ...loaded.filter((at) => !declared.has(at))]), ...external.flatMap(({ chain }) => chain)].map(local)).filter(within),
        };
    });

const keyed = Effect.fnUntraced(function* (workspaceRoot: string, row: string, lock: string, rows: readonly (readonly [config: string, sources: readonly string[]])[]) {
    const folders = rows.map(([config, sources]) => [dirname(config), ...Array.dedupe(sources.filter(Predicate.not(inside(dirname(config)))).map(posix.dirname))] as const);
    const listed = Array.dedupe(folders.flat());
    const [listings, stamps, tracked] = yield* Effect.all(
        [
            Effect.forEach(listed, (at) => Effect.promise(() => getFilesInDirectoryUsingContext(workspaceRoot, posix.relative('.', at))), { concurrency: 'unbounded' }),
            Effect.forEach(rows, ([config, sources]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [posix.join(dirname(config), MANIFEST), lock, ...sources])), { concurrency: 'unbounded' }),
            Effect.map(
                Effect.promise(() => globWithWorkspaceContext(workspaceRoot, Array.dedupe(rows.flatMap(([, sources]) => sources)))),
                (found) => new Set(found),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const beneath = new Map(Array.zipWith(listed, listings, (at, files) => [at, { files, direct: files.filter((file) => posix.dirname(file) === at) }] as const));
    return Array.zipWith(folders, Array.zip(rows, stamps), ([folder, ...outside], [[config, sources], stamp]) => {
        const ignored = [...new Set(sources).difference(tracked)];
        return { config, digest: hashArray([row, ...(beneath.get(folder)?.files ?? []), ...outside.flatMap((at) => beneath.get(at)?.direct ?? [])]), stamp: hashArray([stamp, ...ignored.map((at) => hashFile(join(workspaceRoot, at)))]), ignored };
    });
});

const programmed = Effect.fnUntraced(function* (workspaceRoot: string, compiler: string, row: string, manager: PackageManager, roots: ReadonlySet<string>, files: readonly string[]) {
    const cache = new PluginCache<Resolved & { readonly digest: string; readonly stamp: string }>(join(workspaceDataDirectory, 'typescript.hash'));
    const [lock, owners, configs] = [getLockFileName(manager), new Map(roots.values().map((root) => [root, root] as const)), files.filter((file) => basename(file) === CONFIG)];
    const known = configs.map((config) => [config, cache.get(config)?.sources ?? []] as const);
    const [held, missed] = Array.partition(yield* keyed(workspaceRoot, row, lock, known), ({ config, digest, stamp, ignored }) => {
        const entry = cache.get(config);
        return entry?.digest === digest && entry.stamp === stamp ? Result.succeed([config, Result.succeed({ ...entry, ignored })] as const) : Result.fail(config);
    });
    const fresh = yield* Array.match(missed, { onEmpty: () => Effect.succeed([]), onNonEmpty: (unread) => solved(workspaceRoot, compiler, owners, unread).pipe(Effect.map(Array.map(([config, solution]) => [config, resolved(workspaceRoot, solution)] as const))) });
    const [stored, failed] = Array.partition(fresh, ([config, program]) => Result.mapBoth(program, { onFailure: (error) => [config, error] as const, onSuccess: (value) => [config, value] as const }));
    const listed = stored.map(([config, value]) => [config, value.sources] as const);
    const renewed = yield* keyed(workspaceRoot, row, lock, listed);
    yield* Effect.forEach(Array.zip(stored, renewed), ([[config, value], { digest, stamp }]) => Effect.sync(() => cache.set(config, { ...value, digest, stamp })), { discard: true });
    yield* Array.isReadonlyArrayNonEmpty(stored) ? Effect.sync(() => cache.writeToDisk()) : Effect.void;
    return new Map<string, Result.Result<Program, Cause.UnknownError>>([...held, ...failed.map(([config, error]) => [config, Result.fail(error)] as const), ...Array.zipWith(stored, renewed, ([config, value], { ignored }) => [config, Result.succeed({ ...value, ignored })] as const)]);
});

const compiled =
    (root: string, stated: Stated, packages: readonly string[], linted: boolean, synced: boolean, manager: PackageManager, anchor: Anchor) =>
    ({ commands, configs, outputs, informed, sources, ignored }: Program): Option.Option<ProjectConfiguration> => {
        const [token, unlisted] = [tokened(root), new Set(ignored)];
        const tsc = {
            executor: 'nx:run-commands',
            options: { commands, cwd: root, parallel: false },
            dependsOn: ['codegen--*', '^codegen--*', '^build--tsc'],
            cache: true,
            inputs: [...Array.dedupe([posix.join(root, MANIFEST), ...sources]).map((at) => (unlisted.has(at) ? { fileset: token(at), includeIgnored: true } : token(at))), `^{projectRoot}/${MANIFEST}`, { dependentTasksOutputFiles: '**/*.{d.ts,d.cts,d.mts}', transitive: true }, { externalDependencies: [...packages] }],
            ...(synced && { syncGenerators: ['@nx/js:typescript-sync'] }),
        } satisfies TargetConfiguration;
        const cleaned = (removed: string) => ({
            tsc: {
                executor: EXECUTORS.clean,
                options: { targets: [removed], commands: configs.map((config) => ['tsc', '--build', '--clean', config]), cwd: root },
                inputs: ['default', '^default', { externalDependencies: [...packages] }],
                metadata: { description: `Removes the outputs ${removed} declare and each program output with tsc --build --clean`, help: { command: `${EXEC[manager]} tsc --build --help`, example: { args: ['--dry'] } } },
            },
        });
        const verbs: Verbs = Array.match(outputs, {
            onEmpty: () => ({
                check: { tsc: { ...tsc, outputs: informed.map(token), metadata: { description: 'Type-checks every program of the project with tsc', help: { command: `${EXEC[manager]} tsc --help`, example: { args: ['--extendedDiagnostics'] } } } } },
                ...(Array.isReadonlyArrayNonEmpty(informed) && { clean: cleaned('check--tsc') }),
            }),
            onNonEmpty: (written) => ({
                build: { tsc: { ...tsc, outputs: Array.dedupe([...written, ...informed]).map(token), metadata: { description: 'Emits JavaScript and declarations with tsc, type-checking every program it builds', help: { command: `${EXEC[manager]} tsc --help`, example: { args: ['--listEmittedFiles'] } } } } },
                check: {
                    tsc: { executor: 'nx:noop', dependsOn: ['build--tsc'], inputs: tsc.inputs, metadata: { description: 'Type-checks through the build--tsc compile with tsc', help: { command: `${EXEC[manager]} tsc --help`, example: { args: ['--extendedDiagnostics'] } } } },
                    ...(linted && {
                        publint: {
                            command: 'publint --strict',
                            options: { cwd: root },
                            dependsOn: ['build--*'],
                            cache: true,
                            inputs: ['default', { dependentTasksOutputFiles: '**/*' }, { externalDependencies: ['publint'] }],
                            outputs: [],
                            metadata: { description: 'Lints the tarball the package manager packs against its package.json with publint, warnings failing through --strict', help: { command: `${EXEC[manager]} publint run --help`, example: { args: ['--level', 'warning'] } } },
                        },
                    }),
                },
                clean: cleaned('build--tsc'),
            }),
        });
        return Option.as(Option.liftPredicate(commands, Array.isReadonlyArrayNonEmpty), projected(root, 'typescript', verbs, stated, anchor));
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { compiled, CONFIG, connected, programmed };
