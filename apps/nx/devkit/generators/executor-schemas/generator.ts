import { getProjects, readJson, type Tree, writeJson } from '@nx/devkit';
import { hashObject, loadTsFile, resolveImplementation } from '@nx/devkit/internal';
import { Array, Effect, JsonSchema, Option, Path, Predicate, Record, Schema } from 'effect';
import type { SyncGeneratorResult } from 'nx/src/utils/sync-generators';
import { runtime, ToolError } from '#/process';

// --- [MODELS] --------------------------------------------------------------------------

const Collection = Schema.Struct({ executors: Schema.Record(Schema.String, Schema.Struct({ implementation: Schema.String, schema: Schema.String })) });
const Manifest = Schema.Struct({ name: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const written = Effect.fnUntraced(function* (tree: Tree) {
    const path = yield* Path.Path;
    const projects = Record.fromEntries([...getProjects(tree)]);
    const collections = Object.values(projects)
        .map(({ root }) => ({ root, collection: path.join(root, 'executors.json') }))
        .filter(({ collection }) => tree.exists(collection));
    const rows = yield* Effect.forEach(
        collections,
        Effect.fnUntraced(function* ({ root, collection }) {
            const [{ executors }, { name }] = yield* Effect.all([Schema.decodeUnknownEffect(Collection)(readJson(tree, collection)), Schema.decodeUnknownEffect(Manifest)(readJson(tree, path.join(root, 'package.json')))]);
            return Record.collect(executors, (executor, { implementation, schema }) => ({ root, name, executor, at: path.join(root, schema), implementation }));
        }),
    );
    const stale = yield* Effect.forEach(
        rows.flat(),
        Effect.fnUntraced(function* ({ root, name, executor, at, implementation }) {
            const [file, member = 'default'] = implementation.split('#');
            const loaded = yield* Effect.try({ try: (): unknown => loadTsFile(resolveImplementation(file ?? implementation, path.join(tree.root, root), name, projects)), catch: ToolError.of(`${name}:${executor}`, `does not load ${implementation}`) });
            const derived = Option.liftPredicate(loaded, Predicate.hasProperty(member)).pipe(
                Option.map((module) => module[member]),
                Option.filter(Predicate.hasProperty('options')),
                Option.map((held) => held.options),
                Option.filter(Schema.isSchema),
                Option.map(Schema.toJsonSchemaDocument),
                Option.map(JsonSchema.toDocumentDraft07),
                Option.map(({ schema, definitions }) => ({ $schema: JsonSchema.META_SCHEMA_URI_DRAFT_07, version: 2, ...schema, ...(Record.isEmptyRecord(definitions) ? {} : { definitions }) })),
                Option.filter((schema) => !(tree.exists(at) && hashObject(readJson(tree, at)) === hashObject(schema))),
            );
            return Option.map(derived, (schema) => {
                writeJson(tree, at, schema);
                return at;
            });
        }),
    );
    return { outOfSyncMessage: `Executor schema.json files differ from the options schemas their executors declare: ${Array.getSomes(stale).join(', ')}` } satisfies SyncGeneratorResult;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const generate = (tree: Tree) => runtime.runPromise(written(tree));

// --- [EXPORTS] -------------------------------------------------------------------------

export { generate };
