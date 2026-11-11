import { assert } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-vite';
import manifest from '@rasm/nx-vite/package.json' with { type: 'json' };
import { Array, Effect, Result } from 'effect';
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

// --- [OPERATIONS] ----------------------------------------------------------------------

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
});
