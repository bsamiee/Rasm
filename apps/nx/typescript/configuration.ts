import { createRequire } from 'node:module';
import { pathToFileURL } from 'node:url';
import { normalizePath } from '@nx/devkit';
import { ToolError } from '@rasm/nx-devkit/process';
import type { ParsedCommandLine } from '@typescript/native/sync';
import { Array, Effect, Option, Path, Result } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Solution = Result.Result.Success<Effect.Success<ReturnType<typeof configured>>[number][1]>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'tsconfig.json';

// --- [OPERATIONS] ----------------------------------------------------------------------

const configured = Effect.fn('configured')(function* (workspaceRoot: string, compiled: { readonly file: string; readonly tools: { readonly tsc: readonly string[] } }, configs: readonly string[]) {
    const path = yield* Path.Path;
    const unopened = ToolError.of(workspaceRoot, 'opens no TypeScript project');
    const local = createRequire(path.resolve(workspaceRoot, compiled.file));
    const { API } = yield* Array.match(compiled.tools.tsc, {
        onEmpty: () => ToolError.make({ subject: compiled.file, detail: 'reaches no installed package whose manifest holds bin.tsc' }),
        onNonEmpty: (names) =>
            Effect.firstSuccessOf(
                names.map((name) =>
                    Effect.tryPromise({
                        try: (): Promise<typeof import('@typescript/native/sync')> => import(pathToFileURL(local.resolve(`${name}/sync`)).href),
                        catch: ToolError.of(name, 'exports no ./sync compiler API, which native TypeScript holds'),
                    }),
                ),
            ),
    });
    const api = yield* Effect.acquireDisposable(Effect.sync(() => new API({ cwd: workspaceRoot })));
    const referenced = ({ projectReferences = [] }: ParsedCommandLine) => projectReferences.map(({ path: at }) => (path.extname(at) === '.json' ? at : path.join(at, CONFIG)));
    const solutions = configs.map(
        (config) =>
            [
                config,
                Result.try({
                    try: () => {
                        const file = path.resolve(workspaceRoot, config);
                        const parsed = api.parseConfigFile(file);
                        return { errors: parsed.errors.map(({ text }) => text), owned: [file, ...referenced(parsed)] };
                    },
                    catch: ToolError.of(config, 'does not read as a TypeScript configuration'),
                }),
            ] as const,
    );
    const files = Array.dedupe(Array.getSuccesses(solutions.map(([, read]) => read)).flatMap(({ owned }) => owned));
    const snapshot = yield* Effect.acquireDisposable(Effect.try({ try: () => api.createSnapshot({ openProjects: files }), catch: unopened }));
    const opened = yield* Effect.try({
        try: () =>
            Array.filterMap(files, (file) =>
                Result.map(
                    Result.fromNullishOr(snapshot.getConfiguredProject(file), () => file),
                    (project) => [file, { config: path.relative(workspaceRoot, file), parsed: project.parsedCommandLine, chain: project.program.getConfigFileNames() }] as const,
                ),
            ),
        catch: unopened,
    });
    const members = new Map(opened);
    const joined = (owned: readonly string[]) => {
        const held = Array.getSomes(owned.map((file) => Option.fromUndefinedOr(members.get(file))));
        return { members: held, chain: Array.dedupe(held.flatMap((member) => member.chain)).map((at) => normalizePath(path.relative(workspaceRoot, at))) };
    };
    return solutions.map(([config, read]) => [config, Result.map(read, ({ errors, owned }) => ({ config, errors, ...joined(owned) }))] as const);
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { CONFIG, configured, type Solution };
