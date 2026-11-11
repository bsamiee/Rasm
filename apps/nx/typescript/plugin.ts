import { findPackageJSON } from 'node:module';
import { pathToFileURL } from 'node:url';
import { detectPackageManager, type PackageManager, type TargetConfiguration } from '@nx/devkit';
import { getLockFileName } from '@nx/devkit/internal';
import { inferred, line, listed, type Members, NODE, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Data, Effect, Option, Path, Record, Result, Schema } from 'effect';
import { CONFIG, configured, type Solution } from '#/configuration';

// --- [TYPES] ---------------------------------------------------------------------------

type Report = Data.TaggedEnum<{
    Program: { readonly root: string; readonly commands: readonly string[]; readonly outputs: readonly string[]; readonly chain: readonly string[] };
    Workspace: { readonly manager: PackageManager };
}>;
interface Read extends Unit {
    readonly manager?: PackageManager;
    readonly installed: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TECHNOLOGIES = ['typescript'];
const MANAGERS = {
    pnpm: { manifest: 'pnpm-workspace.yaml', upgrade: ['pnpm', 'update', '--recursive', '--latest'], prune: ['pnpm', 'store', 'prune'] },
} as const satisfies Partial<Record<PackageManager, { readonly manifest: string; readonly upgrade: Array.NonEmptyReadonlyArray<string>; readonly prune: Array.NonEmptyReadonlyArray<string> }>>;

// --- [MODELS] --------------------------------------------------------------------------

const Report = Data.taggedEnum<Report>();

// --- [OPERATIONS] ----------------------------------------------------------------------

const resolved = (path: Path.Path, workspaceRoot: string, root: string, { config, errors, members, chain }: Solution): Result.Result<Report, ToolError> => {
    const holding = members.filter(({ parsed }) => parsed.fileNames.length > 0);
    const checked = Option.match(
        Array.findFirst(holding, (member) => member.config === config),
        { onSome: Array.of, onNone: () => holding },
    );
    const emitted = holding.filter(({ parsed }) => parsed.options.noEmit !== true).map(({ parsed }) => parsed.options);
    return Array.match(errors, {
        onNonEmpty: (reasons) => Result.fail(ToolError.make({ subject: config, detail: reasons.join('; ') })),
        onEmpty: () =>
            Result.succeed(
                Report.Program({
                    root,
                    commands: checked.map(({ config: file, parsed }) => line(['tsc', (parsed.projectReferences ?? []).length > 0 ? '--build' : '--project', path.relative(root, file)])),
                    outputs: Array.dedupe(
                        emitted.flatMap((options) => [
                            ...(options.outDir === undefined ? [] : [path.join(path.relative(workspaceRoot, options.outDir), options.resolveJsonModule === true ? '**/*.{js,cjs,mjs,jsx,json,d.ts,d.cts,d.mts}{,.map}' : '**/*.{js,cjs,mjs,jsx,d.ts,d.cts,d.mts}{,.map}')]),
                            ...(options.declarationDir === undefined ? [] : [path.join(path.relative(workspaceRoot, options.declarationDir), '**/*.{d.ts,d.cts,d.mts}{,.map}')]),
                        ]),
                    ),
                    chain,
                }),
            ),
    });
};

const compiled = (path: Path.Path, exec: string, { root, commands, outputs, chain }: Extract<Report, { readonly _tag: 'Program' }>, installed: readonly string[]): Members => {
    const metadata = (description: string, example: readonly string[]) => ({ description, help: { command: `${exec} tsc --help`, example: { args: [...example] } } });
    const tsc: TargetConfiguration = {
        executor: 'nx:run-commands',
        options: { commands, cwd: root },
        dependsOn: ['build--buf', '^build--buf', 'build--cf'],
        cache: true,
        inputs: ['default', '^default', ...chain.filter((at) => !at.startsWith(`${root}/`)).map((at) => path.join('{workspaceRoot}', at)), { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: ['@typescript/native', ...Array.intersection(installed, ['@effect/tsgo'])] }],
        outputs: [],
    };
    return Array.match(outputs, {
        onEmpty: () => ({ check: { tsc: { ...tsc, metadata: metadata('Type-checks every program of the project with tsc', ['--extendedDiagnostics']) } } }),
        onNonEmpty: (written) => ({
            build: { tsc: { ...tsc, outputs: written.map((at) => path.join('{workspaceRoot}', at)), metadata: metadata('Emits JavaScript and declarations with tsc, type-checking every program it builds', ['--listEmittedFiles']) } },
            check: {
                tsc: { executor: 'nx:noop', dependsOn: ['build--tsc'], metadata: metadata('Type-checks through the build--tsc compile', ['--extendedDiagnostics']) },
                ...(installed.includes('publint') && {
                    publint: {
                        command: 'publint --strict',
                        dependsOn: ['build--*'],
                        cache: true,
                        inputs: ['default', { dependentTasksOutputFiles: '**/*' }, NODE, { externalDependencies: ['publint'] }],
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
    files: [CONFIG, ...Record.keys(MANAGERS).map(getLockFileName)],
    options: Schema.Struct({}),
    units: Effect.fnUntraced(function* ({ files, context: { workspaceRoot } }: Request<unknown>) {
        const path = yield* Path.Path;
        const manager = detectPackageManager(workspaceRoot);
        const installed = ['publint', '@effect/tsgo'].filter((name) => Option.isSome(Option.liftThrowable(findPackageJSON)(name, pathToFileURL(path.join(workspaceRoot, 'package.json')))));
        const names = yield* listed(workspaceRoot, Array.dedupe(files.map(path.dirname)));
        const held = files.filter((file) => names.has(path.dirname(file)));
        return [
            ...held.filter((file) => path.basename(file) === CONFIG).map((file): Read => ({ key: file, file, installed, ...Record.getSomes({ salt: Option.fromUndefinedOr(names.get(path.dirname(file))) }) })),
            ...held.filter((file) => path.basename(file) === getLockFileName(manager)).map((file): Read => ({ key: file, file, installed, manager })),
        ];
    }),
    read: Effect.fnUntraced(function* (missing: Array.NonEmptyReadonlyArray<Read>, { context: { workspaceRoot } }: Request<unknown>) {
        const path = yield* Path.Path;
        const asked = missing.filter(({ manager }) => manager === undefined).map(({ file }) => file);
        const solutions = new Map(asked.length === 0 ? [] : yield* configured(workspaceRoot, asked));
        return missing.map(({ file, manager }) =>
            Result.map(manager === undefined ? Result.flatMap(solutions.get(file) ?? Result.fail(ToolError.make({ subject: file, detail: 'opens no TypeScript project' })), (solved) => resolved(path, workspaceRoot, path.dirname(file), solved)) : Result.succeed(Report.Workspace({ manager })), (report) => ({ report })),
        );
    }),
    sources: (report: Report, { file }: Read) => ({ tracked: Report.$match(report, { Program: ({ chain }) => [file, ...chain], Workspace: () => [] }) }),
    projects: (report: Report, { file, installed }: Read, { path, exec }) => {
        const root = path.dirname(file);
        return Report.$match(report, {
            Program: (program) => [{ file, root, technologies: TECHNOLOGIES, clean: 'tsc', members: compiled(path, exec, program, installed) }],
            Workspace: ({ manager }) =>
                Option.toArray(Record.get<string, (typeof MANAGERS)[keyof typeof MANAGERS]>(MANAGERS, manager)).map(({ manifest, upgrade, prune }) => ({
                    file,
                    root,
                    technologies: [manager],
                    members: {
                        upgrade: {
                            [manager]: {
                                command: line(upgrade),
                                outputs: [`{projectRoot}/${manifest}`, `{projectRoot}/${getLockFileName(manager)}`],
                                metadata: { description: `Moves every dependency requirement of the workspace and its lockfile to the newest release with ${line(upgrade)}`, help: { command: `${line(upgrade.slice(0, 2))} --help`, example: { args: ['--interactive'] } } },
                            },
                        },
                        clean: { [manager]: { options: { commands: [prune], cwd: root }, metadata: { description: `Removes the packages no project on this machine references from the ${manager} store`, help: { command: `${line(prune)} --help`, example: {} } } } },
                    },
                })),
        });
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
