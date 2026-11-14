import { findPackageJSON } from 'node:module';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { type ProjectConfiguration, readProjectsConfigurationFromProjectGraph, workspaceRoot } from '@nx/devkit';
import { Ajv } from 'ajv';
import { Array, Effect, FileSystem, Option, Path, Record, Schema } from 'effect';
import { getGeneratorInformation } from 'nx/src/command-line/generate/generator-utils';
import { getExecutorInformation } from 'nx/src/command-line/run/executor-utils';
import { readNxJson } from 'nx/src/config/nx-json';
import { nx, read, refusals } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

interface Entry {
    readonly name: string;
    readonly kind: 'executors' | 'generators';
    readonly entry: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    sync: "README.md TASKS: `nx sync` writes each executor's `schema.json` and `executors.json` description from its declared options schema, `nx sync:check` fails on a stale one",
    settings: 'nx/schemas/nx-schema.json, named by `$schema` of nx.json',
    project: 'nx/schemas/project-schema.json: the configuration of every project Nx resolves',
    declared: 'nrwl/nx packages/eslint-plugin/src/rules/nx-plugin-checks.spec.ts: noExecutorsOrBuildersFound, noGeneratorsOrSchematicsFound, valueShouldBeObject',
    resolved: 'nrwl/nx packages/eslint-plugin/src/rules/nx-plugin-checks.spec.ts: missingImplementation, invalidImplementationPath, invalidImplementationModule, missingRequiredSchema, invalidSchemaPath',
    options: 'nrwl/nx packages/nx/src/executors/run-commands/schema.json: an executor schema is a version 2 JSON Schema of an object; packages/nx/src/utils/params.ts validateOptsAgainstSchema reads `type`, `properties`, and `required`',
} as const;

const VERSION = 2;

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Unknown));
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String, executors: Schema.optionalKey(Schema.String), generators: Schema.optionalKey(Schema.String) }));
const Collection = Schema.fromJsonString(Schema.Struct({ executors: Schema.optionalKey(Schema.Record(Schema.String, Schema.Unknown)), generators: Schema.optionalKey(Schema.Record(Schema.String, Schema.Unknown)) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const validator = Effect.fnUntraced(function* (name: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const manifest = yield* Effect.fromNullishOr(findPackageJSON('nx', import.meta.url));
    const { $schema: _, ...stated } = yield* Effect.flatMap(fs.readFileString(path.join(path.dirname(manifest), 'schemas', name)), Schema.decodeEffect(Json));
    return new Ajv({ strict: false, allErrors: true }).compile(stated);
});

const named = Effect.fnUntraced(function* (root: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { name, executors, generators } = yield* Effect.flatMap(fs.readFileString(path.join(workspaceRoot, root, 'package.json')), Schema.decodeEffect(Manifest));
    const filed = (kind: Entry['kind'], file: string | undefined) => Array.fromNullishOr(file).map((held) => ({ name, kind, file: path.join(root, held) }));
    return [...filed('executors', executors), ...filed('generators', generators)];
});

const collections = Effect.gen(function* () {
    const [fs, path, workspace] = yield* Effect.all([FileSystem.FileSystem, Path.Path, read('fresh')]);
    const files = yield* Effect.forEach(Object.values(workspace.nodes), ({ data }) => Effect.orElseSucceed(named(data.root), () => []));
    const held = yield* Effect.forEach(files.flat(), ({ name, kind, file }) => Effect.map(Effect.option(Effect.flatMap(fs.readFileString(path.join(workspaceRoot, file)), Schema.decodeEffect(Collection))), (collection) => ({ name, kind, file, entries: Record.keys(Option.getOrUndefined(collection)?.[kind] ?? {}) })));
    return { projects: readProjectsConfigurationFromProjectGraph(workspace).projects, held, entries: held.flatMap(({ name, kind, entries }) => entries.map((entry): Entry => ({ name, kind, entry }))) };
});

const informed = ({ name, kind, entry }: Entry, projects: Record<string, ProjectConfiguration>) => (kind === 'executors' ? getExecutorInformation(name, entry, workspaceRoot, projects) : { ...getGeneratorInformation(name, entry, workspaceRoot, projects), batchImplementationFactory: undefined });

const implemented = (entry: Entry, projects: Record<string, ProjectConfiguration>) => {
    const { implementationFactory, batchImplementationFactory } = informed(entry, projects);
    return [implementationFactory, ...Array.fromNullishOr(batchImplementationFactory)].map((factory) => assert.isFunction(factory()));
};

const optioned = (entry: Entry, projects: Record<string, ProjectConfiguration>) => {
    const { $schema: _, version, type, ...options }: Record<string, unknown> = informed(entry, projects).schema;
    new Ajv({ strict: false }).compile({ type, ...options });
    return assert.deepStrictEqual({ type, version }, { type: 'object', version: { executors: VERSION, generators: version }[entry.kind] });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })((suite) => {
    suite.effect(
        'finds every file a sync generator writes up to date',
        Effect.fnUntraced(function* () {
            const { code, stdout, stderr } = yield* nx(['sync:check']);
            assert.strictEqual(Number(code), 0, `${SOURCES.sync}\n${stdout}${stderr}`);
        }),
    );

    suite.effect(
        'accepts nx.json under the schema nx.json names',
        Effect.fnUntraced(function* () {
            const validate = yield* validator('nx-schema.json');
            validate(readNxJson(workspaceRoot));
            assert.deepStrictEqual(validate.errors ?? [], [], SOURCES.settings);
        }),
    );

    suite.effect(
        'accepts the configuration of every project under the project schema of Nx',
        Effect.fnUntraced(function* () {
            const [validate, { nodes }] = yield* Effect.all([validator('project-schema.json'), read('fresh')]);
            const refused = Object.values(nodes).filter(({ data }) => !validate(data));
            assert.deepStrictEqual(
                refused.map(({ name }) => name),
                [],
                SOURCES.project,
            );
        }),
    );

    suite.effect(
        'declares at least one executor or generator in every collection file a manifest names',
        Effect.fnUntraced(function* () {
            const { held } = yield* collections;
            assert.isNotEmpty(held, SOURCES.declared);
            assert.deepStrictEqual(
                held.filter(({ entries }) => !Array.isReadonlyArrayNonEmpty(entries)).map(({ file }) => file),
                [],
                SOURCES.declared,
            );
        }),
    );

    suite.effect(
        'resolves the implementation and batch implementation of every declared executor and generator to a function',
        Effect.fnUntraced(function* () {
            const { projects, entries } = yield* collections;
            const broken = yield* refusals(
                entries,
                ({ name, entry }) => `${name}:${entry}`,
                (entry) => implemented(entry, projects),
            );
            assert.deepStrictEqual(broken, [], SOURCES.resolved);
        }),
    );

    suite.effect(
        'states the options of every declared executor and generator as a JSON Schema of an object, version 2 for an executor',
        Effect.fnUntraced(function* () {
            const { projects, entries } = yield* collections;
            const invalid = yield* refusals(
                entries,
                ({ name, entry }) => `${name}:${entry}`,
                (entry) => optioned(entry, projects),
            );
            assert.deepStrictEqual(invalid, [], SOURCES.options);
        }),
    );
});
