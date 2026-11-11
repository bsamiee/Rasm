import { findPackageJSON } from 'node:module';
import { pathToFileURL } from 'node:url';
import type { TargetConfiguration } from '@nx/devkit';
import { inferred, line, listed, type Members, NODE, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Option, Path, Result, Schema } from 'effect';
import { CONFIG, configured, type Solution } from '#/configuration';

// --- [TYPES] ---------------------------------------------------------------------------

interface Program {
    readonly commands: readonly string[];
    readonly outputs: readonly string[];
    readonly chain: readonly string[];
}
interface Read extends Unit {
    readonly installed: readonly string[];
}

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

const compiled = (path: Path.Path, exec: string, root: string, { commands, outputs, chain }: Program, installed: readonly string[]): Members => {
    const metadata = (description: string, example: readonly string[]) => ({ description, help: { command: `${exec} tsc --help`, example: { args: [...example] } } });
    const tsc: TargetConfiguration = {
        executor: 'nx:run-commands',
        options: { commands, cwd: root },
        dependsOn: ['codegen--*', '^codegen'],
        cache: true,
        inputs: ['default', '^default', ...chain.filter((at) => !at.startsWith(`${root}/`)).map((at) => path.join('{workspaceRoot}', at)), { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: ['@typescript/native', ...Array.intersection(installed, ['@effect/tsgo'])] }],
        outputs: [],
    };
    return Array.match(outputs, {
        onEmpty: () => ({ check: { tsc: { ...tsc, metadata: metadata('Type-checks every program of the project with tsc', ['--extendedDiagnostics']) } } }),
        onNonEmpty: (written) => ({
            build: { tsc: { ...tsc, dependsOn: ['codegen--*', '^codegen', '^build'], outputs: written.map((at) => path.join('{workspaceRoot}', at)), metadata: metadata('Emits JavaScript and declarations with tsc, type-checking every program it builds', ['--listEmittedFiles']) } },
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
    files: [CONFIG],
    options: Schema.Struct({}),
    units: Effect.fnUntraced(function* ({ files, context: { workspaceRoot } }: Request<unknown>) {
        const path = yield* Path.Path;
        const installed = ['publint', '@effect/tsgo'].filter((name) => Option.isSome(Option.liftThrowable(findPackageJSON)(name, pathToFileURL(path.join(workspaceRoot, 'package.json')))));
        const nested = files.filter((file) => path.dirname(file) !== '.');
        const names = yield* listed(workspaceRoot, Array.dedupe(nested.map(path.dirname)));
        return Array.filterMap(nested, (file) =>
            Result.map(
                Result.fromNullishOr(names.get(path.dirname(file)), () => file),
                (salt): Read => ({ key: file, file, installed, salt }),
            ),
        );
    }),
    read: Effect.fnUntraced(function* (missing: Array.NonEmptyReadonlyArray<Read>, { context: { workspaceRoot } }: Request<unknown>) {
        const path = yield* Path.Path;
        const solutions = new Map(
            yield* configured(
                workspaceRoot,
                missing.map(({ file }) => file),
            ),
        );
        return missing.map(({ file }) =>
            Result.map(
                Result.flatMap(solutions.get(file) ?? Result.fail(ToolError.make({ subject: file, detail: 'opens no TypeScript project' })), (solved) => resolved(path, workspaceRoot, path.dirname(file), solved)),
                (report) => ({ report }),
            ),
        );
    }),
    sources: ({ chain }: Program, { file }: Read) => ({ tracked: [file, ...chain] }),
    projects: (program: Program, { file, installed }: Read, { path, exec }) => [{ file, root: path.dirname(file), technologies: ['typescript'], clean: 'tsc', members: compiled(path, exec, path.dirname(file), program, installed) }],
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
