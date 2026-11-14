import { createRequire } from 'node:module';
import { execPath } from 'node:process';
import { formatFiles, getProjects, readJson, type Tree, writeJson } from '@nx/devkit';
import { loadTsFile, resolveImplementation, type SyncGeneratorResult } from '@nx/devkit/internal';
import { Array, Effect, JsonSchema, Option, Path, Predicate, pipe, Record, Result, Schema, Stream } from 'effect';
import { DistributionError, run, spawned } from '#/executor';

// --- [MODELS] --------------------------------------------------------------------------

const Collection = Schema.Struct({ executors: Schema.Record(Schema.String, Schema.Struct({ implementation: Schema.String, schema: Schema.String })) });
const Manifest = Schema.Struct({ name: Schema.String, executors: Schema.String });

// --- [COMPOSITION] ---------------------------------------------------------------------

const generate = Effect.fnUntraced(function* (tree: Tree) {
    const path = yield* Path.Path;
    const projects = Record.fromEntries([...getProjects(tree)]);
    const collections = Array.filterMap(
        Object.values(projects)
            .map(({ root }) => ({ root, manifest: path.join(root, 'package.json') }))
            .filter(({ manifest }) => tree.exists(manifest)),
        ({ root, manifest }) => Result.map(Schema.decodeUnknownResult(Manifest)(readJson(tree, manifest)), ({ name, executors }) => ({ root, name, collection: path.join(root, executors) })),
    );
    const rows = yield* Effect.forEach(
        collections,
        Effect.fnUntraced(function* ({ root, name, collection }) {
            const { executors } = yield* Schema.decodeUnknownEffect(Collection)(readJson(tree, collection));
            return Record.collect(executors, (executor, { implementation, schema }) => ({ root, name, collection, executor, at: path.join(path.dirname(collection), schema), implementation }));
        }),
    );
    const derived = yield* Effect.forEach(
        rows.flat(),
        Effect.fnUntraced(function* ({ root, name, collection, executor, at, implementation }) {
            const [file = implementation] = implementation.split('#');
            const loaded = yield* Effect.try({ try: (): unknown => loadTsFile(resolveImplementation(file, path.join(tree.root, root), name, projects)), catch: DistributionError.refused(`${name}:${executor}`, `does not load ${implementation}`) });
            const stated = pipe(
                Option.liftPredicate(loaded, Predicate.isObject),
                Option.flatMap((module) => Array.findFirst(Object.values(module), Schema.isSchema)),
                Option.map((schema) => Schema.toJsonSchemaDocument(schema, { includeAnnotationKey: (key) => key.startsWith('$') || key.startsWith('x-') || key === 'hidden' })),
                Option.map(JsonSchema.toDocumentDraft07),
                Option.map(({ schema, schema: { description }, definitions }) => ({ description, schema: { $schema: JsonSchema.META_SCHEMA_URI_DRAFT_07, version: 2, ...schema, ...(Record.isEmptyRecord(definitions) ? {} : { definitions }) } })),
            );
            const listed = readJson<{ readonly executors: Readonly<Record<string, object>> }>(tree, collection);
            return Option.toArray(stated).flatMap(({ schema, description }) => {
                writeJson(tree, at, schema);
                writeJson(tree, collection, { ...listed, executors: { ...listed.executors, [executor]: { ...listed.executors[executor], description } } });
                return [at, collection];
            });
        }),
    );
    yield* Effect.promise(() => formatFiles(tree));
    const own = new Set(derived.flat());
    const biome = Option.liftThrowable(createRequire(path.join(tree.root, 'package.json')).resolve)('@biomejs/biome/bin/biome');
    yield* Effect.forEach(
        Option.match(biome, { onNone: () => [], onSome: (bin) => [...own].map((to) => ({ bin, to })) }),
        Effect.fnUntraced(function* ({ bin, to }) {
            const input = yield* Effect.fromNullishOr(tree.read(to, 'utf-8'));
            tree.write(to, yield* spawned([execPath, bin, 'format', `--stdin-file-path=${to}`], { cwd: tree.root, stdin: Stream.encodeText(Stream.make(input)), stdout: 'pipe' }));
        }),
        { concurrency: 'unbounded' },
    );
    const stale = tree.listChanges().filter((change) => own.has(change.path));
    return { outOfSyncMessage: `Executor schema.json files and executors.json descriptions differ from the options schemas their executors declare, in the form the workspace formatter writes: ${stale.map((change) => change.path).join(', ')}` } satisfies SyncGeneratorResult;
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { generate };
