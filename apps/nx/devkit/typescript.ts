import { API, type ParsedCommandLine } from '@typescript/native/sync';
import { Array, Effect, FileSystem, Option, Path, Result } from 'effect';
import { ancestors } from '#/inference';
import { ToolError } from '#/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'tsconfig.json';

// --- [OPERATIONS] ----------------------------------------------------------------------

const configured = Effect.fnUntraced(function* (workspaceRoot: string, configs: readonly string[]) {
    const path = yield* Path.Path;
    const api = yield* Effect.acquireRelease(
        Effect.sync(() => new API({ cwd: workspaceRoot })),
        (opened) => Effect.sync(() => opened.close()),
    );
    const referenced = ({ projectReferences = [] }: ParsedCommandLine) => projectReferences.map(({ path: at }) => (path.extname(at) === '.json' ? at : path.join(at, CONFIG)));
    const solutions = configs.map((config) =>
        Result.try({
            try: () => {
                const file = path.resolve(workspaceRoot, config);
                const parsed = api.parseConfigFile(file);
                return { config, errors: parsed.errors.map(({ text }) => text), owned: [file, ...referenced(parsed)] };
            },
            catch: ToolError.of(config, 'does not read as a TypeScript configuration'),
        }),
    );
    const files = Array.dedupe(Array.getSuccesses(solutions).flatMap(({ owned }) => owned));
    const members = yield* Effect.try({
        try: () => {
            const snapshot = api.createSnapshot({ openProjects: files });
            return new Map(
                Array.getSomes(
                    files.map((file) =>
                        Option.map(Option.fromUndefinedOr(snapshot.getConfiguredProject(file)), (project) => [file, { config: file, parsed: project.parsedCommandLine, chain: project.program.getConfigFileNames().map((at) => path.relative(workspaceRoot, at)) }] as const),
                    ),
                ),
            );
        },
        catch: ToolError.of(workspaceRoot, 'opens no TypeScript project'),
    });
    return solutions.map(Result.map(({ config, errors, owned }) => ({ config, errors, members: Array.getSomes(owned.map((file) => Option.fromUndefinedOr(members.get(file)))) })));
}, Effect.scoped);

const chained = Effect.fnUntraced(function* (workspaceRoot: string, roots: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const nearest = yield* Effect.forEach(roots, (root) =>
        Effect.findFirst(
            ancestors(path, root).map((at) => path.join(at, CONFIG)),
            (file) => fs.exists(path.join(workspaceRoot, file)),
        ),
    );
    const configs = Array.dedupe(Array.getSomes(nearest));
    const solved = new Map(Array.zip(configs, Array.isReadonlyArrayNonEmpty(configs) ? yield* configured(workspaceRoot, configs) : []));
    return Array.zipWith(roots, nearest, (root, config) =>
        Option.match(
            Option.flatMap(config, (file) => Option.flatMap(Option.fromUndefinedOr(solved.get(file)), Result.getSuccess)),
            { onNone: () => [], onSome: ({ members }) => Array.dedupe(members.flatMap(({ chain }) => chain)).filter((at) => !(at.startsWith('..') || at.startsWith(`${root}/`) || at.split(path.sep).includes('node_modules'))) },
        ),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { CONFIG, chained, configured };
