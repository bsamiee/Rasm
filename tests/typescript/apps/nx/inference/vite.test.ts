import { assert } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-vite';
import manifest from '@rasm/nx-vite/package.json' with { type: 'json' };
import { Array, Effect, Option, Result } from 'effect';
import { cases, inferred, running, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const subject: Subject = {
    plugin: { name: manifest.name, createNodes },
    packages: ['vite', '@rasm/nx-vite'],
    runs: ['build'],
    changed: ['kit/entry.ts', "export const outDir = 'built';\n"],
    served: ['site', 'serve--vite', 'http://localhost:4517'],
    files: {
        'package.json': '{ "name": "root", "workspaces": ["site", "kit"] }\n',
        'site/package.json': '{ "name": "site", "type": "module" }\n',
        'site/index.html': '<!doctype html><html lang="en"><head><title>Site</title></head><body><script type="module" src="./main.ts"></script></body></html>\n',
        'site/main.ts': "document.title = 'Site';\n",
        'site/vite.config.ts': "import { readySignal } from '@rasm/nx-vite/ready-signal';\nimport { defineConfig } from 'vite';\n\nexport default defineConfig({ build: { outDir: 'out' }, server: { port: 4517, strictPort: true }, plugins: [readySignal()] });\n",
        'kit/package.json': '{ "name": "kit", "type": "module" }\n',
        'kit/index.ts': 'export const kit = 1;\n',
        'kit/entry.ts': "export const outDir = 'dist';\n",
        'kit/vite.config.ts': "import { defineConfig } from 'vite';\nimport { outDir } from './entry.ts';\n\nexport default defineConfig({ build: { outDir, lib: { entry: 'index.ts', formats: ['es'] } } });\n",
    },
    broken: ['kit/vite.config.ts', "import { defineConfig } from 'vite';\n\nexport default defineConfig({ build: { lib: { entry:\n"],
};

const LIBRARY = 'nrwl/nx@2a424d3 packages/vite/src/plugins/plugin.spec.ts "Library mode"';
const LIB = "lib: { entry: 'index.ts', formats: ['es'] }";
const LIBRARIES = [
    { label: 'no server', config: `export default defineConfig({ build: { ${LIB} } });`, ran: ['vite build'] },
    { label: 'a server port', config: `export default defineConfig({ build: { ${LIB} }, server: { port: 3000 } });`, ran: ['vite', 'vite build', 'vite preview'] },
    { label: 'an empty server', config: `export default defineConfig({ build: { ${LIB} }, server: {} });`, ran: ['vite', 'vite build', 'vite preview'] },
    { label: 'a server stated while serving alone', config: `export default defineConfig(({ command }) => ({ build: { ${LIB} }, ...(command === 'serve' ? { server: { port: 4200 } } : {}) }));`, ran: ['vite', 'vite build', 'vite preview'] },
    { label: 'a server a plugin of the config states', config: `export default defineConfig({ build: { ${LIB} }, plugins: [{ name: 'user-plugin', config: () => ({ server: { port: 4200 } }) }] });`, ran: ['vite', 'vite build', 'vite preview'] },
    {
        label: 'a config that throws while serving',
        config: `export default defineConfig(({ command }) => {\n    if (command === 'serve') {\n        throw new Error('This config only builds');\n    }\n    return { build: { ${LIB} } };\n});`,
        ran: ['vite build'],
    },
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const subcommands = (graph: ProjectGraph) =>
    Array.dedupe(
        running('vite')(graph).map(({ command }) =>
            Option.getOrElse(
                Array.findFirst(['vite build', 'vite preview'], (stated) => command.startsWith(stated)),
                () => 'vite',
            ),
        ),
    ).toSorted();

const previewed = (graph: ProjectGraph) => running('vite preview')(graph).map(({ project, continuous, after }) => ({ project, continuous, after }));

const built = (graph: ProjectGraph) => Array.filterMap(running('vite build')(graph), ({ project, target }) => (project === 'site' ? Result.succeed({ project, continuous: true, after: [target] }) : Result.failVoid));

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('builds each config with vite build, cached, writing the build.outDir the config resolves to', () =>
        inferred(subject).pipe(
            Effect.map(running('vite build')),
            Effect.map(Array.map(({ project, cache, outputs }) => ({ project, cache, outputs }))),
            Effect.map((builds) =>
                assert.deepStrictEqual(builds, [
                    { project: 'kit', cache: true, outputs: ['kit/dist'] },
                    { project: 'site', cache: true, outputs: ['site/out'] },
                ]),
            ),
        ),
    );

    it.effect('serves a config that reports its address to Nx as an uncached continuous task ready on that signal, and no config that reports none', () =>
        inferred(subject).pipe(
            Effect.map(running('vite')),
            Effect.map(Array.filter(({ command }) => command === 'vite')),
            Effect.map(Array.map(({ project, continuous, cache, readyWhen }) => ({ project, continuous, cache, readyWhen }))),
            Effect.map((served) => assert.deepStrictEqual(served, [{ project: 'site', continuous: true, cache: false, readyWhen: { signal: true } }])),
        ),
    );

    it.effect('previews the build output as a continuous task after the target that builds it', () => Effect.map(inferred(subject), (graph) => assert.deepStrictEqual(previewed(graph), built(graph))));

    it.effect.each(LIBRARIES)('runs $ran for a library config with $label', ({ config, ran }) =>
        inferred({ ...subject, files: { 'package.json': '{ "name": "root", "workspaces": ["kit"] }\n', 'kit/package.json': '{ "name": "kit", "type": "module" }\n', 'kit/index.ts': 'export const kit = 1;\n', 'kit/vite.config.ts': `import { defineConfig } from 'vite';\n\n${config}\n` } }).pipe(
            Effect.map((graph) => assert.deepStrictEqual(subcommands(graph), [...ran], LIBRARY)),
        ),
    );

    it.effect('writes the build under the outputPath nx.json states', () =>
        inferred({ ...subject, files: { ...subject.files, 'nx.json': '{ "pluginsConfig": { "@rasm/nx-devkit": { "outputPath": ".artifacts" } } }\n' } }).pipe(
            Effect.map(running('vite build')),
            Effect.map(Array.map(({ project, outputs }) => ({ project, outputs }))),
            Effect.map((builds) =>
                assert.deepStrictEqual(builds, [
                    { project: 'kit', outputs: ['.artifacts/vite/kit'] },
                    { project: 'site', outputs: ['.artifacts/vite/site'] },
                ]),
            ),
        ),
    );

    it.effect('builds a config that states its entry as build input where the folder holds no index.html', () =>
        inferred({
            ...subject,
            files: {
                'package.json': '{ "name": "root", "workspaces": ["entered"] }\n',
                'entered/package.json': '{ "name": "entered", "type": "module" }\n',
                'entered/main.ts': "export const entered = 'main';\n",
                'entered/vite.config.ts': "import { defineConfig } from 'vite';\n\nexport default defineConfig({ build: { rolldownOptions: { input: { index: 'main.ts' } } } });\n",
            },
        }).pipe(
            Effect.map(running('vite build')),
            Effect.map(Array.map(({ project }) => project)),
            Effect.map((builds) => assert.deepStrictEqual(builds, ['entered'], 'nrwl/nx@2a424d3 packages/vite/src/plugins/plugin.spec.ts "should create nodes when rollupOptions contains input"')),
        ),
    );

    it.effect('previews a config that reports its address to Nx as a task ready on that signal', () =>
        inferred(subject).pipe(
            Effect.map(running('vite preview')),
            Effect.map(Array.map(({ project, readyWhen }) => ({ project, readyWhen }))),
            Effect.map((ready) => assert.deepStrictEqual(ready, [{ project: 'site', readyWhen: { signal: true } }], 'nrwl/nx@2a424d3 packages/vite/src/plugins/plugin.spec.ts "waits for the signal when the config loads nxReadySignal()" targets.preview.readyWhen')),
        ),
    );
});
