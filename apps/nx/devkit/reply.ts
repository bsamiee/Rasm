import { writeSync } from 'node:fs';
import { createRequire, registerHooks } from 'node:module';
import { chdir, cwd, env, exit, stdin } from 'node:process';
import { text } from 'node:stream/consumers';
import { fileURLToPath, pathToFileURL } from 'node:url';
import type { NodeServices } from '@effect/platform-node/NodeServices';
import { Array, Effect, Option, Path, Result, Schema, Struct } from 'effect';
import { ToolError } from '#/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Job = Schema.Codec<{ readonly root: string; readonly file: string }, unknown>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPLIES = 3;

// --- [MODELS] --------------------------------------------------------------------------

const Jobs = <J extends Job>(job: J) => Schema.fromJsonString(Schema.toCodecJson(Schema.Array(job)));
const Reply = <A, I>(report: Schema.Codec<A, I>) => Schema.fromJsonString(Schema.toCodecJson(Schema.Struct({ loaded: Schema.Array(Schema.String), outcome: Schema.Result(report, ToolError) })));

// --- [OPERATIONS] ----------------------------------------------------------------------

const reached = (path: Path.Path, edges: ReadonlyMap<string | undefined, ReadonlySet<string>>, workspaceRoot: string, root: string) => {
    const inside = `${pathToFileURL(path.join(workspaceRoot, root)).href}/`;
    const seen = new Set<string>();
    const walk = (frontier: readonly string[]): ReadonlySet<string> => {
        const next = Array.dedupe(frontier.flatMap((url) => [...(edges.get(url) ?? [])])).filter((url) => !seen.has(url) && url.startsWith('file:') && !url.includes('/node_modules/'));
        next.forEach((url) => seen.add(url));
        return next.length === 0 ? seen : walk(next);
    };
    return [...walk(Array.getSomes([...edges.keys()].map((url) => Option.filter(Option.fromUndefinedOr(url), (at) => at.startsWith(inside)))))].map((url) => fileURLToPath(url));
};

const loaded = <A>(folder: string, ...specifiers: Array.NonEmptyReadonlyArray<string>) =>
    Effect.tryPromise({ try: (): Promise<A> => import(pathToFileURL(specifiers.reduce((from, specifier) => createRequire(from).resolve(specifier), `${folder}/package.json`)).href), catch: ToolError.of(specifiers.join(' > '), `does not load from ${folder}`) });

const outermost = (path: Path.Path, workspaceRoot: string, folders: readonly string[]) => Array.dedupe(folders.map((at) => path.relative(workspaceRoot, at))).filter((at, _, all) => !all.some((other) => at.startsWith(`${other}/`)));

const served = Effect.fn('served')(function* <J extends Job, A, I>(job: J, report: Schema.Codec<A, I>, read: (job: J['Type'], workspaceRoot: string) => Effect.Effect<{ readonly report: A; readonly sources: readonly string[] }, ToolError, NodeServices>): Effect.fn.Return<void, Error, NodeServices> {
    const path = yield* Path.Path;
    const workspaceRoot = cwd();
    const edges = new Map<string | undefined, Set<string>>();
    const ambient = { ...env };
    const entered = (root: string) =>
        Effect.sync(() => {
            Object.keys(env)
                .filter((name) => !(name in ambient))
                .forEach((name) => Reflect.deleteProperty(env, name));
            Object.assign(env, ambient);
            chdir(path.join(workspaceRoot, root));
        });
    const [jobs, reply] = [
        yield* Effect.flatMap(
            Effect.promise(() => text(stdin)),
            Schema.decodeEffect(Jobs(job)),
        ),
        Schema.encodeEffect(Reply(report)),
    ];
    yield* Effect.sync(() =>
        registerHooks({
            resolve: (specifier, context, next) => {
                const resolution = next(specifier, context);
                edges.set(context.parentURL, (edges.get(context.parentURL) ?? new Set<string>()).add(resolution.url));
                return resolution;
            },
        }),
    );
    yield* Effect.forEach(
        jobs,
        Effect.fnUntraced(function* (each) {
            yield* entered(each.root);
            const outcome = yield* Effect.result(read(each, workspaceRoot));
            const sources = Result.getOrElse(Result.map(outcome, Struct.get('sources')), Array.empty<string>);
            const line = yield* reply({ loaded: Array.dedupe([...reached(path, edges, workspaceRoot, each.root), ...sources].map((at) => path.relative(workspaceRoot, at))), outcome: Result.map(outcome, Struct.get('report')) });
            yield* Effect.sync(() => writeSync(REPLIES, `${line}\n`));
        }),
        { discard: true },
    );
    return yield* Effect.sync(() => exit());
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Job, Jobs, loaded, outermost, REPLIES, Reply, served };
