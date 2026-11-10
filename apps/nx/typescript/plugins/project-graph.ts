import { findPackageJSON } from 'node:module';
import { pathToFileURL } from 'node:url';
import { type CreateNodes, type CreateNodesContext, type CreateNodesResultArray, hashArray, type TargetConfiguration } from '@nx/devkit';
import { globWithWorkspaceContext, hashWithWorkspaceContext } from '@nx/devkit/internal';
import { aggregated, CONFIG, cached, GENERATED, identity, inferred, launcher, line, type Members, reported } from '@rasm/nx-devkit/inference';
import { ToolError } from '@rasm/nx-devkit/process';
import { configured, type Solution } from '@rasm/nx-devkit/typescript';
import { Array, Effect, FileSystem, Option, Path, Result } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Program {
    readonly root: string;
    readonly commands: readonly string[];
    readonly outputs: readonly string[];
    readonly chain: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TECHNOLOGIES = ['typescript'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const resolved = (path: Path.Path, workspaceRoot: string, root: string, { config, errors, members, chain }: Solution): Result.Result<Program, ToolError> => {
    const holding = members.filter(({ parsed }) => parsed.fileNames.length > 0);
    const checked = Option.match(
        Array.findFirst(holding, (member) => member.config === config),
        { onSome: Array.of, onNone: () => holding },
    );
    const emitted = holding.filter(({ parsed }) => parsed.options.noEmit !== true).map(({ parsed }) => parsed.options);
    return Array.match(errors, {
        onNonEmpty: (reasons) => Result.fail(ToolError.make({ subject: config, detail: reasons.join('; ') })),
        onEmpty: () =>
            Result.succeed({
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
    });
};

const compiled = (path: Path.Path, exec: string, program: Program, external: readonly string[], linted: boolean): Members => {
    const metadata = (description: string, example: readonly string[]) => ({ technologies: TECHNOLOGIES, description, help: { command: `${exec} tsc --help`, example: { args: [...example] } } });
    const tsc: TargetConfiguration = {
        executor: 'nx:run-commands',
        options: { commands: program.commands, cwd: program.root },
        dependsOn: [...GENERATED.dependsOn],
        cache: true,
        inputs: ['default', '^default', ...program.chain.filter((at) => !at.startsWith(`${program.root}/`)).map((at) => path.join('{workspaceRoot}', at)), ...GENERATED.inputs, { externalDependencies: ['@typescript/native', ...external] }],
        outputs: [],
    };
    return Array.match(program.outputs, {
        onEmpty: () => ({ check: { tsc: { ...tsc, metadata: metadata('Type-checks every program of the project with tsc', ['--extendedDiagnostics']) } } }),
        onNonEmpty: (outputs) => ({
            build: { tsc: { ...tsc, outputs: outputs.map((at) => path.join('{workspaceRoot}', at)), metadata: metadata('Emits JavaScript and declarations with tsc, type-checking every program it builds', ['--listEmittedFiles']) } },
            check: {
                tsc: { executor: 'nx:noop', dependsOn: ['build--tsc'], inputs: tsc.inputs ?? [], metadata: metadata('Type-checks through the build--tsc compile', ['--extendedDiagnostics']) },
                ...(linted
                    ? {
                          publint: {
                              executor: 'nx:run-commands',
                              options: { command: 'publint --strict', cwd: program.root },
                              dependsOn: ['build--*'],
                              cache: true,
                              inputs: ['default', { dependentTasksOutputFiles: '**/*' }, { externalDependencies: ['publint'] }],
                              outputs: [],
                              metadata: { technologies: TECHNOLOGIES, description: 'Lints the tarball the package manager packs against its package.json with publint, warnings failing through --strict', help: { command: `${exec} publint run --help`, example: { args: ['--level', 'warning'] } } },
                          },
                      }
                    : {}),
            },
        }),
    });
};

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: unknown, { workspaceRoot }: CreateNodesContext) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const [roots, plugin] = yield* Effect.all([Effect.filter(Array.dedupe(files.map(path.dirname)).toSorted(), (root) => fs.exists(path.join(workspaceRoot, root, 'package.json')), { concurrency: 'unbounded' }), identity(import.meta.url, workspaceRoot)]);
    const installed = (specifier: string) => Option.isSome(Option.liftThrowable(findPackageJSON)(specifier, pathToFileURL(path.join(workspaceRoot, 'package.json'))));
    const external = installed('@effect/tsgo') ? ['@effect/tsgo'] : [];
    const cache = yield* cached<Program>(plugin, options);
    const held = yield* Effect.forEach(
        roots,
        Effect.fnUntraced(function* (root) {
            const nested = roots.filter((other) => other.startsWith(`${root}/`)).map((other) => path.join(other, '**'));
            const names = hashArray(yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [path.join(root, '**')], nested)));
            const digest = ({ chain }: Program) =>
                Effect.map(
                    Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [path.join(root, CONFIG), ...chain])),
                    (content) => hashArray([plugin.digest, names, content]),
                );
            return [path.join(root, CONFIG), { root, digest, kept: yield* cache.get(root, digest) }] as const;
        }),
        { concurrency: 'unbounded' },
    );
    const asked = new Map(held.filter(([, { kept }]) => Option.isNone(kept)));
    const solutions = asked.size === 0 ? [] : yield* configured(workspaceRoot, [...asked.keys()]);
    const read = yield* Effect.forEach(
        solutions,
        Effect.fnUntraced(function* ([config, solution]) {
            const { root, digest } = yield* Effect.orDie(Effect.fromNullishOr(asked.get(config)));
            const program = Result.flatMap(solution, (solved) => resolved(path, workspaceRoot, root, solved));
            yield* Result.match(program, { onFailure: () => Effect.void, onSuccess: (built) => Effect.flatMap(digest(built), (now) => cache.set(root, built, now)) });
            return Result.mapError(program, (error) => [config, error] as const);
        }),
    );
    const linted = installed('publint');
    const exec = launcher(workspaceRoot);
    const fresh = new Map(Array.getSuccesses(read).map((program) => [program.root, program] as const));
    return yield* reported(
        Array.getSomes(held.map(([, { root, kept }]) => Option.orElse(kept, () => Option.fromUndefinedOr(fresh.get(root))))).map((program): CreateNodesResultArray[number] => [
            path.join(program.root, CONFIG),
            { projects: { [program.root]: { metadata: { technologies: TECHNOLOGIES }, targets: aggregated(compiled(path, exec, program, external, linted), TECHNOLOGIES, { executor: plugin.cleaner, tool: 'tsc' }) } } },
        ]),
        Array.getFailures(read),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [`**/${CONFIG}`, (files, options, context) => inferred(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
