import { formatFiles, getProjects, readJson, type Tree, writeJson } from '@nx/devkit';
import { loadTsFile, resolveImplementation, type SyncGeneratorResult } from '@nx/devkit/internal';
import { Array, Effect, Equal, JsonSchema, Option, Path, Predicate, pipe, Record, Result, Schema, Struct } from 'effect';
import { NxError, run } from '#/inference';

// --- [MODELS] --------------------------------------------------------------------------

const Collection = Schema.Struct({ executors: Schema.Record(Schema.String, Schema.Struct({ implementation: Schema.String, schema: Schema.String })) });
const Manifest = Schema.Struct({ name: Schema.String, executors: Schema.String });

// --- [COMPOSITION] ---------------------------------------------------------------------

const generate = Effect.fnUntraced(function* (tree: Tree) {
    const path = yield* Path.Path;
    const projects = Object.fromEntries(getProjects(tree));
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
    const changed = ([at, stated]: readonly [string, object]) => !(tree.exists(at) && Equal.equals(readJson(tree, at), stated));
    const derived = yield* Effect.forEach(
        rows.flat(),
        Effect.fnUntraced(function* ({ root, name, collection, executor, at, implementation }) {
            const [file = implementation] = implementation.split('#');
            const loaded = yield* Effect.try({ try: (): unknown => loadTsFile(resolveImplementation(file, path.join(tree.root, root), name, projects)), catch: NxError.of({ _tag: 'Unloaded', tool: 'nx' }, path.join(root, file)) });
            return pipe(
                Option.liftPredicate(loaded, Predicate.isObject),
                Option.flatMap((module) => Array.findFirst(Object.values(module), Schema.isSchema)),
                Option.map((schema) => JsonSchema.toDocumentDraft07(Schema.toJsonSchemaDocument(schema, { includeAnnotationKey: (key) => key.startsWith('$') || key.startsWith('x-') || key === 'hidden' }))),
                Option.map(({ schema, schema: { description }, definitions }) => ({ collection, executor, at, description, schema: { $schema: JsonSchema.META_SCHEMA_URI_DRAFT_07, version: 2, ...schema, ...(Record.isEmptyRecord(definitions) ? {} : { definitions }) } })),
            );
        }),
    );
    const declared = Array.getSomes(derived);
    const described = Record.collect(Array.groupBy(declared, Struct.get('collection')), (collection, members): readonly [string, object] => {
        const listed = readJson<{ readonly executors: Readonly<Record<string, object>> }>(tree, collection);
        return [collection, { ...listed, executors: { ...listed.executors, ...Record.fromIterableWith(members, ({ executor, description }) => [executor, { ...listed.executors[executor], description }]) } }];
    });
    const written = [...declared.map(({ at, schema }): readonly [string, object] => [at, schema]), ...described].filter(changed);
    written.forEach(([to, value]) => writeJson(tree, to, value));
    yield* Effect.promise(() => formatFiles(tree));
    return { outOfSyncMessage: 'Executor schema.json files and executors.json descriptions differ from the options schemas their executors declare', outOfSyncDetails: written.map(([to]) => to) } satisfies SyncGeneratorResult;
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { generate };
