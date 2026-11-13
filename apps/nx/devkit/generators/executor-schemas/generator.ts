import { createRequire } from 'node:module';
import { execPath } from 'node:process';
import { formatFiles, getProjects, readJson, type Tree, writeJson } from '@nx/devkit';
import { loadTsFile, resolveImplementation, type SyncGeneratorResult } from '@nx/devkit/internal';
import { Array, Effect, JsonSchema, Option, Path, Predicate, Record, Result, Schema } from 'effect';
import { printed, runtime, ToolError } from '#/process';

// --- [MODELS] --------------------------------------------------------------------------

const Collection = Schema.Struct({ executors: Schema.Record(Schema.String, Schema.Struct({ implementation: Schema.String, schema: Schema.String })) });
const Manifest = Schema.Struct({ name: Schema.String, executors: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const written = Effect.fnUntraced(function* (tree: Tree) {
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
            const [file, member = 'default'] = implementation.split('#');
            const loaded = yield* Effect.try({ try: (): unknown => loadTsFile(resolveImplementation(file ?? implementation, path.join(tree.root, root), name, projects)), catch: ToolError.of(`${name}:${executor}`, `does not load ${implementation}`) });
            const attached = Option.liftPredicate(loaded, Predicate.hasProperty(member)).pipe(
                Option.map((module) => module[member]),
                Option.filter(Predicate.hasProperty('options')),
                Option.map((held) => held.options),
                Option.filter(Schema.isSchema),
            );
            const exported = Option.flatMap(Option.liftPredicate(loaded, Predicate.isObject), (module) => Array.findFirst(Object.values(module), Schema.isSchema));
            const stated = Option.orElse(attached, () => exported).pipe(
                Option.map(Schema.toJsonSchemaDocument),
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
        ({ bin, to }) =>
            Effect.fromNullishOr(tree.read(to, 'utf-8')).pipe(
                Effect.flatMap((input) => printed(execPath, [bin, 'format', `--stdin-file-path=${to}`], { cwd: tree.root, input })),
                Effect.map((text) => tree.write(to, text)),
            ),
        { concurrency: 'unbounded' },
    );
    const stale = tree
        .listChanges()
        .map((change) => change.path)
        .filter((at) => own.has(at));
    return { outOfSyncMessage: `Executor schema.json files and executors.json descriptions differ from the options schemas their executors declare, in the form the workspace formatter writes: ${stale.join(', ')}` } satisfies SyncGeneratorResult;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const generate = (tree: Tree) => runtime.runPromise(written(tree));

// --- [EXPORTS] -------------------------------------------------------------------------

export { generate };
