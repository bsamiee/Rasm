import { availableParallelism } from 'node:os';
import type { TargetConfiguration } from '@nx/devkit';
import { ancestors, inferred, line, listed, type Members, NODE, type Project, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, identity, Match, Option, Path, Predicate, Record, Result, Schema } from 'effect';
import { CONFIG, configured, type Solution } from '#/configuration';

// --- [TYPES] ---------------------------------------------------------------------------

interface Program {
    readonly commands: readonly string[];
    readonly outputs: readonly string[];
    readonly chain: readonly string[];
}
interface Read extends Unit {
    readonly tools: { readonly tsc: readonly string[]; readonly tsgo: readonly string[]; readonly publint: readonly string[] };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'package.json';

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ private: Schema.optionalKey(Schema.Boolean) }));
const Installed = Schema.fromJsonString(Schema.Struct({ name: Schema.String, bin: Schema.optionalKey(Schema.Union([Schema.String, Schema.Record(Schema.String, Schema.String)])) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const resolved = (path: Path.Path, workspaceRoot: string, root: string, { config, errors, members, chain }: Solution): Result.Result<Program, ToolError> => {
    const holding = members.filter(({ parsed }) => parsed.fileNames.length > 0);
    const checked = Option.match(
        Array.findFirst(holding, (member) => member.config === config),
        { onSome: Array.of, onNone: () => holding },
    );
    const built = checked.some(({ parsed }) => (parsed.projectReferences ?? []).length > 0);
    const outputs = holding
        .filter(({ parsed }) => parsed.options.noEmit !== true)
        .flatMap(({ config: file, parsed: { options } }) => {
            const folder = path.dirname(path.resolve(workspaceRoot, file));
            const emitted = options.resolveJsonModule === true ? '**/*.{js,cjs,mjs,jsx,json,d.ts,d.cts,d.mts}{,.map}' : '**/*.{js,cjs,mjs,jsx,d.ts,d.cts,d.mts}{,.map}';
            const stated = path.resolve(options.outDir ?? folder, options.outDir === undefined || options.rootDir === undefined ? '.' : path.relative(options.rootDir, folder));
            return [
                path.join(options.outDir ?? folder, emitted),
                ...(options.declarationDir === undefined ? [] : [path.join(options.declarationDir, '**/*.{d.ts,d.cts,d.mts}{,.map}')]),
                ...(built || options.incremental === true || options.composite === true ? [options.tsBuildInfoFile ?? path.join(stated, `${path.basename(file, '.json')}.tsbuildinfo`)] : []),
            ].map((at) => path.relative(workspaceRoot, at));
        });
    return Array.match(errors, {
        onNonEmpty: (reasons) => Result.fail(ToolError.make({ subject: config, detail: reasons.join('; ') })),
        onEmpty: () =>
            Result.succeed({
                commands: checked.map(({ config: file, parsed }) => line(['tsc', (parsed.projectReferences ?? []).length > 0 ? '--build' : '--project', path.relative(root, file)])),
                outputs: Array.dedupe(outputs),
                chain,
            }),
    });
};

const held = Effect.fnUntraced(
    function* (workspaceRoot: string, folders: readonly string[]) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const entries = (folder: string, scope: string) => fs.readDirectory(path.join(workspaceRoot, folder, 'node_modules', scope)).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(Array.empty<string>())));
        const top = yield* Effect.forEach(folders, (folder) =>
            Effect.map(
                entries(folder, ''),
                Array.map((name) => [folder, name] as const),
            ),
        );
        const scoped = yield* Effect.forEach(
            top.flat().filter(([, name]) => name.startsWith('@')),
            ([folder, scope]) =>
                Effect.map(
                    entries(folder, scope),
                    Array.map((name) => [folder, `${scope}/${name}`] as const),
                ),
        );
        const installed = [...top.flat().filter(([, name]) => !name.startsWith('.')), ...scoped.flat()];
        const manifests = yield* Effect.forEach(
            installed,
            ([folder, name]) =>
                fs.readFileString(path.join(workspaceRoot, folder, 'node_modules', name, 'package.json')).pipe(
                    Effect.map(Schema.decodeOption(Installed)),
                    Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeedNone),
                ),
            { concurrency: availableParallelism() },
        );
        const named = Array.zip(installed, manifests).flatMap(([[folder, owner], manifest]) =>
            Option.toArray(Option.map(manifest, ({ name, bin = {} }) => (Predicate.isString(bin) ? [path.basename(name)] : Record.keys(bin))))
                .flat()
                .map((command) => [command, folder, owner] as const),
        );
        return Record.map(
            Array.groupBy(named, ([command]) => command),
            (rows) =>
                Record.map(
                    Array.groupBy(rows, ([, folder]) => folder),
                    Array.map(([, , name]) => name),
                ),
        );
    },
    (effect, workspaceRoot) => Effect.mapError(effect, ToolError.of(workspaceRoot, 'does not list the packages its projects install')),
);

const compiled = (path: Path.Path, exec: string, root: string, { commands, outputs, chain }: Program, tools: Read['tools']): Members => {
    const metadata = (description: string, example: readonly string[]) => ({ description, help: { command: `${exec} tsc --help`, example: { args: [...example] } } });
    const tsc = {
        executor: 'nx:run-commands',
        options: { commands, cwd: root },
        dependsOn: ['codegen--*', '^codegen'],
        cache: true,
        inputs: ['default', '^default', ...chain.filter((at) => !at.startsWith(`${root}/`)).map((at) => path.join('{workspaceRoot}', at)), { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: [...tools.tsc, ...tools.tsgo] }],
        outputs: [],
    } satisfies TargetConfiguration;
    return Array.match(outputs, {
        onEmpty: () => ({ check: { tsc: { ...tsc, metadata: metadata('Type-checks every program of the project with tsc', ['--extendedDiagnostics']) } } }),
        onNonEmpty: (written) => ({
            build: { tsc: { ...tsc, dependsOn: ['clean--tsc', ...tsc.dependsOn], outputs: written.map((at) => path.join('{workspaceRoot}', at)), metadata: metadata('Emits JavaScript and declarations with tsc, type-checking every program it builds', ['--listEmittedFiles']) } },
            check: {
                tsc: { executor: 'nx:noop', dependsOn: ['build--tsc'], metadata: metadata('Type-checks through the build--tsc compile', ['--extendedDiagnostics']) },
                ...(Array.isReadonlyArrayNonEmpty(tools.publint) && {
                    publint: {
                        command: 'publint --strict',
                        dependsOn: ['build--*'],
                        cache: true,
                        inputs: ['default', { dependentTasksOutputFiles: '**/*' }, NODE, { externalDependencies: [...tools.publint] }],
                        outputs: [],
                        metadata: { description: 'Lints the tarball the package manager packs against its package.json with publint, warnings failing through --strict', help: { command: `${exec} publint run --help`, example: { args: ['--level', 'warning'] } } },
                    },
                }),
            },
        }),
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes } = inferred({
    module: import.meta.url,
    files: [CONFIG, MANIFEST],
    options: Schema.Struct({}),
    units: Effect.fnUntraced(function* ({ files, context: { workspaceRoot } }: Request<unknown>) {
        const path = yield* Path.Path;
        const nested = files.filter((file) => path.dirname(file) !== '.');
        const packages = yield* held(workspaceRoot, Array.dedupe(nested.flatMap((file) => ancestors(path, path.dirname(file)))));
        const holding = (command: string, file: string) => Array.dedupe(ancestors(path, path.dirname(file)).flatMap((folder) => packages[command]?.[folder] ?? []));
        const names = yield* listed(workspaceRoot, Array.dedupe(nested.map(path.dirname)));
        return Array.filterMap(nested, (file) =>
            Result.map(
                Result.fromNullishOr(names.get(path.dirname(file)), () => file),
                (salt): Read => ({ key: file, file, tools: { tsc: holding('tsc', file), tsgo: holding('effect-tsgo', file), publint: holding('publint', file) }, salt }),
            ),
        );
    }),
    read: Effect.fnUntraced(function* (missing: Array.NonEmptyReadonlyArray<Read>, { context: { workspaceRoot } }: Request<unknown>) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [manifests, programs] = Array.partition(
            missing,
            Result.liftPredicate(({ file }: Read) => path.basename(file) === MANIFEST, identity),
        );
        const solutions = new Map(
            yield* Array.isReadonlyArrayNonEmpty(programs)
                ? configured(
                      workspaceRoot,
                      Array.headNonEmpty(programs),
                      Array.map(programs, ({ file }) => file),
                  )
                : Effect.succeed([]),
        );
        const packed = new Map<string, Result.Result<Program | typeof Manifest.Type, Error>>(yield* Effect.forEach(manifests, ({ file }) => Effect.map(Effect.result(Effect.flatMap(fs.readFileString(path.join(workspaceRoot, file)), Schema.decodeEffect(Manifest))), (read) => [file, read] as const)));
        return missing.map(({ file }) => Result.map(packed.get(file) ?? Result.flatMap(solutions.get(file) ?? Result.fail(ToolError.make({ subject: file, detail: 'opens no TypeScript project' })), (solved) => resolved(path, workspaceRoot, path.dirname(file), solved)), (report) => ({ report })));
    }),
    sources: (report: Program | typeof Manifest.Type, { file }: Read) => ({ tracked: [file, ...('chain' in report ? report.chain : [])] }),
    projects: (report: Program | typeof Manifest.Type, { file, tools }: Read, { path, exec, placed }) => {
        const root = path.dirname(file);
        const folder = Option.getOrElse(placed('pnpm', root), () => root);
        const tarball: Project = {
            file,
            root,
            technologies: ['npm'],
            members: {
                pack: {
                    pnpm: {
                        command: line(['pnpm', 'pack', '--out', path.join(path.relative(root, folder), '%s.tgz')]),
                        dependsOn: ['build'],
                        cache: true,
                        inputs: ['default', { dependentTasksOutputFiles: '**/*' }, NODE],
                        outputs: [path.join('{workspaceRoot}', folder, '*.tgz')],
                        metadata: { description: 'Packs the package into one tarball named for it with pnpm pack', help: { command: 'pnpm pack --help', example: { args: ['--json'] } } },
                    },
                },
            },
            packaged: ['pack--pnpm'],
        };
        return Match.value(report).pipe(
            Match.when({ chain: Match.any }, (program): readonly Project[] => [{ file, root, technologies: ['typescript'], clean: 'tsc', members: compiled(path, exec, root, program, tools) }]),
            Match.when({ private: true }, () => []),
            Match.orElse(() => [tarball]),
        );
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
