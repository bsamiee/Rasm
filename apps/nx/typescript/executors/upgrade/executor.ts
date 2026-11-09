import { matchesGlob } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Cause, Effect, FileSystem, Option, Path, pipe, Record, Result, Schema, SemVer, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import { Composer, CST, Parser, Scalar, YAMLMap } from 'yaml';

// --- [MODELS] --------------------------------------------------------------------------

const Workspace = Schema.Struct({
    catalog: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)),
    update: Schema.optionalKey(Schema.Struct({ ignoreDeps: Schema.optionalKey(Schema.Array(Schema.String)) })),
}).annotate({ description: 'pnpm-workspace.yaml fields the upgrade reads: the default catalog and the held packages' });
const Tags = Schema.fromJsonString(Schema.Record(Schema.String, Schema.String)).annotate({ description: 'pnpm view <name> dist-tags --json: each dist-tag and the version it names' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = Effect.fnUntraced(function* (args: readonly string[], cwd: string) {
    const handle = yield* ChildProcess.make('pnpm', args, { cwd, extendEnv: true, stdin: 'ignore', stderr: 'inherit' });
    const [stdout, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? stdout : yield* new Cause.UnknownError(args, `pnpm ${args.join(' ')} exited ${code}`);
}, Effect.scoped);

const upgraded = Effect.fnUntraced(function* (context: ExecutorContext) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const name = yield* Effect.fromNullishOr(context.projectName);
    const project = yield* Effect.fromNullishOr(context.projectGraph.nodes[name]);
    const root = path.join(context.root, project.data.root);
    const file = path.join(root, 'pnpm-workspace.yaml');
    const tokens = [...new Parser().parse(yield* fs.readFileString(file))];
    const document = yield* Effect.fromOption(Array.head(new Composer({ keepSourceTokens: true }).compose(tokens)));
    const table = document.get('catalog');
    const { catalog = {}, update } = yield* Schema.decodeUnknownEffect(Workspace)(document.toJS());
    const held = update?.ignoreDeps ?? [];
    const rows = Array.getSomes(
        Record.toEntries(catalog)
            .filter(([row]) => !held.some((pattern) => matchesGlob(row, pattern)))
            .map(([row, spec]) =>
                pipe(
                    SemVer.parse(spec),
                    Result.getSuccess,
                    Option.map((current) => ({ row, current })),
                ),
            ),
    );
    const tags = yield* Effect.forEach(rows, ({ row }) => Effect.flatMap(run(['view', row, 'dist-tags', '--json'], root), Schema.decodeEffect(Tags)), { concurrency: 8 });
    const moved = Array.zipWith(rows, tags, ({ row, current }, named) =>
        pipe(
            Array.getSuccesses(Object.values(named).map(SemVer.parse)),
            Array.match({ onEmpty: Option.none, onNonEmpty: (versions) => Option.some(Array.max(versions, SemVer.Order)) }),
            Option.filter(SemVer.isGreaterThan(current)),
            Option.map((version) => ({ row, current, version })),
        ),
    );
    yield* Effect.forEach(
        Array.getSomes(moved),
        ({ row, current, version }) => {
            const node = table instanceof YAMLMap ? table.get(row) : undefined;
            return Effect.andThen(
                Effect.sync(() => (node instanceof Scalar && node.srcToken !== undefined ? CST.setScalarValue(node.srcToken, SemVer.format(version)) : undefined)),
                Effect.logInfo(`${row} ${SemVer.format(current)} -> ${SemVer.format(version)}`),
            );
        },
        { discard: true },
    );
    yield* fs.writeFileString(file, tokens.map((token) => CST.stringify(token)).join(''));
    yield* run(['update', '--recursive'], root);
    return { success: true };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (_options: unknown, context: ExecutorContext) => upgraded(context).pipe(Effect.provide(layer), Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
