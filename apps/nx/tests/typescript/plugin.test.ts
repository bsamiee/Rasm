import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import plugin from '@rasm/nx-typescript/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx';
import compiler from '@typescript/native/package.json' with { type: 'json' };
import { Duration, Effect, Formatter, Layer, Path, Record, Schema } from 'effect';
import nx from 'nx/package.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const SPEC = 'nrwl/nx packages/js/src/plugins/typescript/plugin.spec.ts';
const TASKS = 'README.md TASKS';
const UNREAD = /(?:^|[\s"'])-{0,2}version\b|mise\.lock|node_modules|@rasm\/nx-|TOKEN|SECRET|PASSWORD|CREDENTIAL/u;
const bounds = { built: '5 minutes', test: Duration.toMillis('3 minutes') } as const;
const manifest = JSON.stringify({ name: 'consumer', private: true, type: 'module', devDependencies: { nx: nx.version, [plugin.name]: '*', typescript: compiler.version } });
const emitted = (outDir: string) => `{ "extends": "../../tsconfig.base.json", "compilerOptions": { "outDir": "${outDir}", "rootDir": ".", "declaration": true }, "include": ["*.ts"] }\n`;
const files = {
    'package.json': manifest,
    'tsconfig.strict.json': '{ "compilerOptions": { "strict": true } }\n',
    'tsconfig.base.json': '{ "extends": "./tsconfig.strict.json", "compilerOptions": { "module": "nodenext", "skipLibCheck": true, "types": [] } }\n',
    'tsconfig.json': '{ "extends": "./tsconfig.base.json", "files": [] }\n',
    'libs/shared/units.ts': 'export const unit = 1;\n',
    'libs/kit/package.json': '{ "name": "kit", "version": "0.1.0", "type": "module", "exports": { ".": "./index.ts" } }\n',
    'libs/kit/tsconfig.json': '{ "extends": "../../tsconfig.base.json", "compilerOptions": { "noEmit": true }, "include": ["*.ts", "../shared/*.ts"] }\n',
    'libs/kit/index.ts': 'export const kit: number = 1;\n',
    'libs/emitting/package.json': '{ "name": "emitting", "version": "0.1.0", "type": "module", "exports": { ".": { "types": "./dist/index.d.ts", "default": "./dist/index.js" } }, "files": ["dist"] }\n',
    'libs/emitting/tsconfig.json': emitted('dist'),
    'libs/emitting/index.ts': 'export const emitting: number = 1;\n',
    'libs/faulty/package.json': '{ "name": "faulty", "version": "0.1.0", "type": "module", "exports": { ".": "./index.ts" } }\n',
    'libs/faulty/tsconfig.json': '{ "extends": "../../tsconfig.base.json", "compilerOptions": { "noEmit": true }, "include": ["*.ts"] }\n',
    'libs/faulty/index.ts': "export const faulty: number = 'text';\n",
};

// --- [MODELS] --------------------------------------------------------------------------

const Shown = Schema.fromJsonString(Schema.Struct({ compilerOptions: Schema.Struct({ outDir: Schema.optionalKey(Schema.String), noEmit: Schema.optionalKey(Schema.Boolean) }) })).annotate({
    description: 'Fields of the configuration tsc --showConfig prints for one project: outDir relative to the config file, and noEmit',
});
const Names = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.flatMap(Consumer, (consumer) => consumer.read);

const shown = Effect.fnUntraced(function* (folder: string) {
    const { called } = yield* Consumer;
    const { stdout } = yield* called('tsc', ['--showConfig', '--project', '.'], folder);
    return yield* Schema.decodeEffect(Shown)(stdout);
});

const tooled = ({ nodes }: Pick<ProjectGraph, 'nodes'>, project: string, verb: string) => Record.filter(nodes[project]?.data.targets ?? {}, (_, name) => name.startsWith(`${verb}--`));

const hashed = Effect.fnUntraced(function* (project: string, verb: string) {
    const [{ inputs }, graph] = yield* Effect.all([Consumer, read]);
    const listed = yield* Effect.forEach(Record.keys(tooled(graph, project, verb)), (name) => inputs(`${project}:${name}`));
    return listed.flatMap(({ files: held }) => held);
});

const declared = Effect.fnUntraced(function* (project: string, verb: string) {
    const [{ outputs }, graph] = yield* Effect.all([Consumer, read]);
    const listed = yield* Effect.forEach(Record.keys(tooled(graph, project, verb)), (name) => outputs(`${project}:${name}`));
    return { paths: listed.flatMap(({ outputPaths }) => outputPaths), stored: listed.flatMap(({ expandedOutputs }) => expandedOutputs) };
});

const workspace = (stated: Readonly<Record<string, string>>) => Layer.provideMerge(Consumer.layer({ resolved: import.meta.url, settings: { plugins: [plugin.name] }, ignored: ['dist', 'emitted', '*.tsbuildinfo'], files: stated }), Layer.provideMerge(Packed.layer(plugin.name), layer));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(workspace(files), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    const api = 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesV2';

    methods.effect(
        'reading the graph writes no file',
        Effect.fnUntraced(function* () {
            const [, written] = yield* Effect.flatMap(Consumer, ({ written: tracked }) => tracked(read));
            assert.deepStrictEqual(written, [], `${api}: createNodes reads configuration and writes nothing, a tsbuildinfo or emitted file included`);
        }),
        bounds.test,
    );

    methods.effect(
        'a project tsconfig.json beside a package manifest gives its project a check target the check verb aggregates',
        Effect.fnUntraced(function* () {
            const graph = yield* read;
            const title = 'typecheck target > should create a node with a typecheck target for a project level tsconfig.json file by default (when there is a sibling package.json or project.json)';
            const principle = 'README.md PRINCIPLES "Verbs aggregate <verb>--<tool> targets"';
            const { kit } = graph.nodes;
            const names = Record.keys(tooled(graph, 'kit', 'check'));
            const { check } = kit?.data.targets ?? {};
            const rows = new Set(check?.dependsOn);
            assert.deepStrictEqual(graph.failures, [], `${SPEC} "${title}": a valid workspace reads without failure`);
            assert.isNotEmpty(names, `${SPEC} "${title}"; ${principle}: a check--<tool> target type-checks the project`);
            assert.isTrue(rows.has('check--*') || rows.isSupersetOf(new Set(names)), `${principle}: check depends on each check--<tool> target`);
        }),
        bounds.test,
    );

    methods.effect(
        'the root tsconfig.json gives the workspace root no check or build target',
        Effect.fnUntraced(function* () {
            const graph = yield* read;
            const [root] = Object.values(graph.nodes).filter(({ data }) => data.root === '.');
            assert.deepStrictEqual(
                ['check', 'build'].flatMap((verb) => Record.keys(tooled(graph, root?.name ?? '.', verb))),
                [],
                `${SPEC} "should not create nodes for root tsconfig.json files"`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'every config a tsconfig extends is an input of the check',
        Effect.fnUntraced(function* () {
            assert.includeMembers(yield* hashed('kit', 'check'), ['libs/kit/tsconfig.json', 'tsconfig.base.json', 'tsconfig.strict.json'], `${SPEC} "typecheck target > inputs > should add extended config files when there are multiple extended config files"`);
        }),
        bounds.test,
    );

    methods.effect(
        'a file an include pattern reaches outside the project is an input of the check',
        Effect.fnUntraced(function* () {
            assert.include(yield* hashed('kit', 'check'), 'libs/shared/units.ts', `${SPEC} "typecheck target > inputs > should normalize and expand ".." in \`include\` with the ts extensions"`);
        }),
        bounds.test,
    );

    methods.effect(
        'a change to an extended config marks the project affected',
        Effect.fnUntraced(function* () {
            const { nx: run } = yield* Consumer;
            const { checked } = yield* run('show', 'projects', '--affected', '--files=tsconfig.strict.json', '--json');
            assert.includeMembers([...(yield* Effect.flatMap(checked, Schema.decodeEffect(Names)))], ['kit', 'emitting', 'faulty'], `${TASKS} "--files=<path> limits it to one change's reach": each project extends tsconfig.strict.json through tsconfig.base.json`);
        }),
        bounds.test,
    );

    methods.effect(
        'a package with entry points in its outDir gets a build target, and one stating noEmit gets none',
        Effect.fnUntraced(function* () {
            const [graph, kit, emitting] = yield* Effect.all([read, shown('libs/kit'), shown('libs/emitting')]);
            assert.deepStrictEqual([kit.compilerOptions.noEmit, emitting.compilerOptions.outDir], [true, './dist'], 'tsc --showConfig: libs/kit states noEmit, libs/emitting states outDir ./dist');
            assert.isNotEmpty(Record.keys(tooled(graph, 'emitting', 'build')), `${SPEC} "build target > should create build target when the entry points point to dist files"`);
            assert.isEmpty(Record.keys(tooled(graph, 'kit', 'build')), `${SPEC} "build target > should not create build target when the entry points point to source files", "typecheck target > should not invoke \`tsc --build\` when \`noEmit\` is set in the tsconfig.json file"`);
        }),
        bounds.test,
    );

    methods.effect(
        'the build is cached and its outputs lie in the outDir tsc shows',
        Effect.fnUntraced(function* () {
            const [graph, path, { compilerOptions }] = yield* Effect.all([read, Path.Path, shown('libs/emitting')]);
            const folder = path.join('libs/emitting', compilerOptions.outDir ?? '.');
            assert.isTrue(
                Object.values(tooled(graph, 'emitting', 'build')).every(({ cache }) => cache === true),
                `${SPEC} "build target > should create a node with a build target when enabled, for a project level tsconfig.lib.json build file by default": build states cache true`,
            );
            assert.isTrue(
                (yield* declared('emitting', 'build')).paths.some((output) => output === folder || output.startsWith(`${folder}${path.sep}`)),
                `${SPEC} "build target > outputs > should add the \`outDir\`": outputs name outDir of tsc --showConfig`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'plugin isolation on and off give equal projects',
        Effect.fnUntraced(function* () {
            const { called } = yield* Consumer;
            const printed = (isolate: boolean) => called('env', ['NX_CACHE_PROJECT_GRAPH=false', `NX_ISOLATE_PLUGINS=${isolate}`, 'nx', 'show', 'project', 'emitting', '--json']);
            const [isolated, shared] = [yield* printed(true), yield* printed(false)];
            assert.deepStrictEqual([shared.code, shared.stdout], [isolated.code, isolated.stdout], 'nrwl/nx packages/nx/src/project-graph/plugins/isolation/isolated-plugin.spec.ts: a plugin returns the same project from a worker and in process');
        }),
        bounds.test,
    );

    methods.effect(
        'no input of an inferred target is a version command (--version, -version, <tool> version), plugin source, mise.lock, or a credential variable',
        Effect.fnUntraced(function* () {
            const rows = Object.values((yield* read).nodes)
                .flatMap(({ data }) => Object.values(data.targets ?? {}))
                .flatMap(({ inputs = [] }) => inputs)
                .map((row) => Formatter.format(row));
            assert.deepStrictEqual(
                rows.filter((row) => UNREAD.test(row)),
                [],
                'README.md PRINCIPLES "Task inputs hold the files a task\'s tool reads: plugin and executor source, .config/mise.lock, tool --version output, and credentials are no input"',
            );
        }),
        bounds.test,
    );
    methods.effect(
        'two reads of one workspace give equal projects, and a file no config reads changes none',
        Effect.fnUntraced(function* () {
            const { wrote } = yield* Consumer;
            const [first, second] = [yield* read, yield* read];
            yield* wrote({ 'docs/notes.md': '# Notes\n' });
            assert.deepStrictEqual(second.nodes, first.nodes, `${api}: one set of files gives one set of nodes`);
            assert.deepStrictEqual((yield* read).nodes, first.nodes, `${api}: a file no config reads changes no node`);
        }),
        bounds.test,
    );
});

it.layer(workspace(files), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    methods.effect.each([
        ['kit', true],
        ['faulty', false],
    ] as const)(
        'the check verb passes where tsc passes and fails where tsc fails: %s',
        Effect.fnUntraced(function* ([project, passes]) {
            const { called, nx: run } = yield* Consumer;
            const [compiled, checked] = [yield* called('tsc', ['--noEmit', '--project', '.'], `libs/${project}`), yield* run('run', `${project}:check`, '--output-style=static')];
            assert.strictEqual(compiled.code === 0, passes, `tsc --noEmit --project libs/${project} ${passes ? 'passes' : 'fails'}: ${compiled.stdout}`);
            assert.strictEqual(checked.code === 0, passes, `${TASKS} "check reports findings of checkers reading the project model": nx run ${project}:check follows tsc: ${checked.stdout}${checked.stderr}`);
        }),
        bounds.test,
    );

    methods.effect(
        'a build writes every file under the declared outputs',
        Effect.fnUntraced(function* () {
            const [{ nx: run, written }, path] = yield* Effect.all([Consumer, Path.Path]);
            const [{ code, stdout, stderr }, created] = yield* written(run('run', 'emitting:build', '--output-style=static'));
            const { stored } = yield* declared('emitting', 'build');
            const under = new RegExp(`^(?:${stored.map(RegExp.escape).join('|')})(?:${RegExp.escape(path.sep)}|$)`, 'u');
            assert.strictEqual(code, 0, `nx run emitting:build exits 0: ${stdout}${stderr}`);
            assert.include(created, 'libs/emitting/dist/index.js', 'tsc emits index.js into outDir');
            assert.deepStrictEqual(
                created.filter((file) => !under.test(file)),
                [],
                `nx/schemas/project-schema.json outputs; ${SPEC} "build target > outputs > should include tsbuildinfo file when outDir and rootDir at both set": every file the build writes lies under a declared output`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'an edited outDir moves the declared outputs to the folder tsc shows',
        Effect.fnUntraced(function* () {
            const [{ wrote }, path] = yield* Effect.all([Consumer, Path.Path]);
            yield* wrote({ 'libs/emitting/tsconfig.json': emitted('emitted'), 'libs/emitting/package.json': files['libs/emitting/package.json'].replaceAll('./dist', './emitted') });
            const folder = path.join('libs/emitting', (yield* shown('libs/emitting')).compilerOptions.outDir ?? '.');
            assert.isTrue(
                (yield* declared('emitting', 'build')).paths.some((output) => output === folder || output.startsWith(`${folder}${path.sep}`)),
                'nrwl/nx packages/nx/src/project-graph/nx-deps-cache.spec.ts: a changed file invalidates what was computed from it',
            );
        }),
        bounds.test,
    );
});

it.layer(workspace({ ...files, 'libs/broken/package.json': '{ "name": "broken", "version": "0.1.0", "type": "module" }\n', 'libs/broken/tsconfig.json': '{ "compilerOptions": ', 'libs/broken/index.ts': 'export {};\n' }), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    methods.effect(
        'a tsconfig that fails to load is named in the failure, and every other project keeps its targets',
        Effect.fnUntraced(function* () {
            const graph = yield* read;
            const errors = 'nrwl/nx packages/nx/src/project-graph/error-types.ts AggregateCreateNodesError';
            assert.include(
                graph.failures.flatMap((failure) => failure.files).map(({ file }) => file),
                'libs/broken/tsconfig.json',
                `${errors}: errors hold a [file, error] tuple for the file that failed`,
            );
            assert.isNotEmpty(Record.keys(tooled(graph, 'kit', 'check')), `${errors}: "It allows Nx to recieve partial results and continue processing"`);
            assert.isNotEmpty(Record.keys(tooled(graph, 'emitting', 'build')), `${errors}: partial results hold every project whose file loaded`);
        }),
        bounds.test,
    );
});
