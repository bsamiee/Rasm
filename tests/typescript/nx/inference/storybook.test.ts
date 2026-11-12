import { assert } from '@effect/vitest';
import type { NxPlugin, ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-storybook';
import manifest from '@rasm/nx-storybook/package.json' with { type: 'json' };
import vitest from '@rasm/nx-vitest/package.json' with { type: 'json' };
import { graphed, workspace } from '@rasm/testing/nx/inference';
import framework from '@storybook/react-vite/package.json' with { type: 'json' };
import { Array, Effect, Option, Predicate, Record, Schema } from 'effect';
import { LoadedNxPlugin } from 'nx/src/devkit-internals';
import { cases, inferred, running, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STORY = "export default { title: 'Sheet' };\n\nexport const Plain = {};\n";
const main = (addons: string) => `export default { framework: '${framework.name}', stories: ['../*.stories.ts'], addons: [${addons}] };\n`;

const subject: Subject = {
    plugin: { name: manifest.name, createNodes },
    packages: ['storybook', framework.name, manifest.name],
    files: {
        'package.json': '{ "name": "root", "workspaces": ["buttons", "forms"] }\n',
        'buttons/package.json': '{ "name": "buttons", "type": "module" }\n',
        'buttons/.storybook/main.ts': main(''),
        'buttons/sheet.stories.ts': STORY,
        'forms/package.json': '{ "name": "forms", "type": "module" }\n',
        'forms/.storybook/main.ts': main(''),
        'forms/sheet.stories.ts': STORY,
    },
    broken: ['forms/.storybook/main.ts', 'export default { framework:\n'],
};

const DEVELOPMENT = { continuous: true, cache: false, readyWhen: { port: '{options.port ?? env.SBCONFIG_PORT ?? env.PORT ?? auto}', host: '{env.SBCONFIG_HOSTNAME ?? options.host ?? localhost}' } };
const SPEC = 'nrwl/nx@2a424d3 packages/storybook/src/plugins';
const ADDONS = [
    { label: 'a string entry', addons: "'@storybook/addon-vitest'", source: '"should infer test-storybook from a generated Vite config with addon-vitest"' },
    { label: 'an object entry', addons: "{ name: '@storybook/addon-vitest', options: {} }", source: '"should infer test-storybook from an addon object entry"' },
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Served = Schema.Struct({ buildTarget: Schema.String, staticFilePath: Schema.String });
const Commanded = Schema.Struct({ command: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const previewed = (graph: ProjectGraph): readonly [found: readonly unknown[], expected: readonly unknown[]] => {
    const built = running('storybook build')(graph);
    return [
        built.map(({ project }) => {
            const { executor, continuous, dependsOn = [], options, readyWhen } = graph.nodes[project]?.data.targets?.['preview--storybook'] ?? {};
            return { project, executor, continuous, after: dependsOn.map((entry) => (Predicate.isString(entry) ? entry : entry.target)), options: Option.getOrUndefined(Schema.decodeUnknownOption(Served)(options)), readyWhen };
        }),
        built.map(({ project, target, outputs }) => ({ project, executor: '@nx/web:file-server', continuous: true, after: [target], options: { buildTarget: target, staticFilePath: outputs.join() }, readyWhen: { signal: true } })),
    ];
};

const tested = (graph: ProjectGraph) =>
    Object.values(graph.nodes)
        .filter((node) => node.name === 'tested')
        .flatMap((node) => Record.toEntries(node.data.targets ?? {}))
        .filter(([name, { options }]) => name.startsWith('test--') || Option.exists(Schema.decodeUnknownOption(Commanded)(options), ({ command }) => command.includes('vitest')))
        .map(([name, { options }]) => ({ name, filtered: Option.exists(Schema.decodeUnknownOption(Commanded)(options), ({ command }) => command.includes('--project')) }));

const listing = Effect.fnUntraced(function* (addons: string, beside: boolean) {
    const tester = yield* Effect.promise(() => import('@rasm/nx-vitest'));
    const plugins: ReadonlyArray<NxPlugin & { readonly name: string }> = [subject.plugin, ...(beside ? [{ name: vitest.name, createNodes: tester.createNodes }] : [])];
    const root = yield* workspace(
        {
            'package.json': '{ "name": "root", "workspaces": ["tested"] }\n',
            'tested/package.json': '{ "name": "tested", "type": "module" }\n',
            'tested/.storybook/main.ts': main(addons),
            'tested/sheet.stories.ts': STORY,
            'tested/vitest.config.ts': "import { defineConfig } from 'vitest/config';\n\nexport default defineConfig({ test: { projects: [{ extends: true, test: { name: 'storybook', include: ['*.stories.ts'] } }] } });\n",
        },
        [...(subject.packages ?? []), '@storybook/addon-vitest', 'vitest', 'vite'],
        import.meta.url,
    );
    return tested(
        yield* graphed(
            root,
            plugins.map((plugin) => new LoadedNxPlugin(plugin, { plugin: plugin.name })),
        ),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('builds a Storybook whose config is main.js', () =>
        inferred({ ...subject, files: { ...subject.files, 'package.json': '{ "name": "root", "workspaces": ["buttons", "forms", "plain"] }\n', 'plain/package.json': '{ "name": "plain", "type": "module" }\n', 'plain/.storybook/main.js': main(''), 'plain/sheet.stories.ts': STORY } }).pipe(
            Effect.map(running('storybook build')),
            Effect.map(Array.map(({ project }) => project)),
            Effect.map((built) => assert.include(built, 'plain', 'nrwl/nx@2a424d3 packages/storybook/src/plugins/plugin.spec.ts "should support main.js"')),
        ),
    );

    it.effect('builds each Storybook with storybook build, cached, writing storybook-static', () =>
        inferred(subject).pipe(
            Effect.map(running('storybook build')),
            Effect.map(Array.map(({ project, cache, outputs }) => ({ project, cache, outputs }))),
            Effect.map((built) =>
                assert.deepStrictEqual(built, [
                    { project: 'buttons', cache: true, outputs: ['buttons/storybook-static'] },
                    { project: 'forms', cache: true, outputs: ['forms/storybook-static'] },
                ]),
            ),
        ),
    );

    it.effect('serves each Storybook with storybook dev as an uncached continuous task, ready on the port a run sets and on start where Storybook picks one', () =>
        inferred(subject).pipe(
            Effect.map(running('storybook dev')),
            Effect.map(Array.map(({ project, continuous, cache, readyWhen }) => ({ project, continuous, cache, readyWhen }))),
            Effect.map((served) =>
                assert.deepStrictEqual(
                    served,
                    [
                        { project: 'buttons', ...DEVELOPMENT },
                        { project: 'forms', ...DEVELOPMENT },
                    ],
                    `${SPEC}/plugin.ts:268-274,380-388 the storybook target of every config: storybook dev, continuous; nrwl/nx@7fca107 packages/storybook/src/utils/dev-server-ready-check.ts resolvePort flag, SBCONFIG_PORT, PORT, "Probes nothing when no port is set"`,
                ),
            ),
        ),
    );

    it.effect('previews the built Storybook of each project through the file server as a continuous task after its build target, serving the folder that target writes, ready on its signal', () =>
        Effect.map(inferred(subject), (graph) => assert.deepStrictEqual(previewed(graph)[0], previewed(graph)[1], `${SPEC}/plugin.ts:426-443 static target, ${SPEC}/plugin.spec.ts "should create nodes" static-storybook; README [04] preview`)),
    );

    it.effect('writes the build under the outputPath nx.json states and previews that folder', () =>
        Effect.map(inferred({ ...subject, files: { ...subject.files, 'nx.json': '{ "pluginsConfig": { "@rasm/nx-devkit": { "outputPath": ".artifacts" } } }\n' } }), (graph) => {
            const [found, expected] = previewed(graph);
            const served = { executor: '@nx/web:file-server', continuous: true, after: ['build--storybook'], readyWhen: { signal: true } };
            assert.deepStrictEqual(
                found,
                [
                    { project: 'buttons', ...served, options: { buildTarget: 'build--storybook', staticFilePath: '.artifacts/storybook/buttons' } },
                    { project: 'forms', ...served, options: { buildTarget: 'build--storybook', staticFilePath: '.artifacts/storybook/forms' } },
                ],
                'README [03] generated output under .artifacts/<tool>',
            );
            assert.deepStrictEqual(expected, found, `${SPEC}/plugin.ts:426-443 static target serves the folder the build target writes`);
        }),
    );

    it.effect.each(ADDONS)('presents no test target of its own for a config that lists addon-vitest as $label, and runs the storybook Vitest project under test--vitest alone', ({ addons, source }) =>
        Effect.map(Effect.all([listing(addons, false), listing(addons, true)]), (ran) =>
            assert.deepStrictEqual(ran, [[], [{ name: 'test--vitest', filtered: false }]], `${SPEC}/plugin.ts:406-424 vitest run --project=storybook, ${SPEC}/plugin.spec.ts ${source}; README [03] verbs aggregate <verb>--<tool> targets, the tool here is Vitest`),
        ),
    );
});
