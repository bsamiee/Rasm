import { assert } from '@effect/vitest';
import { createNodes } from '@rasm/nx-typescript';
import manifest from '@rasm/nx-typescript/package.json' with { type: 'json' };
import { Array, Effect } from 'effect';
import { cases, inferred, running, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const subject: Subject = {
    plugin: { name: manifest.name, createNodes },
    packages: ['@typescript/native'],
    runs: ['build', 'check'],
    reads: { 'inherited:check--tsc': ['tsconfig.shared.json'] },
    changed: ['tsconfig.shared.json', '{ "compilerOptions": { "noEmit": false, "outDir": "dist" } }\n'],
    files: {
        'package.json': '{ "name": "root", "workspaces": ["emitted", "checked", "inherited"] }\n',
        'tsconfig.shared.json': '{ "compilerOptions": { "noEmit": true } }\n',
        'emitted/package.json': '{ "name": "emitted", "type": "module" }\n',
        'emitted/tsconfig.json': '{ "compilerOptions": { "outDir": "dist", "rootDir": ".", "declaration": true, "module": "nodenext" }, "include": ["*.ts"] }\n',
        'emitted/index.ts': 'export const emitted = 1;\n',
        'checked/package.json': '{ "name": "checked", "type": "module" }\n',
        'checked/tsconfig.json': '{ "compilerOptions": { "noEmit": true, "module": "nodenext" }, "include": ["*.ts"] }\n',
        'checked/main.ts': 'export const checked = 1;\n',
        'inherited/package.json': '{ "name": "inherited", "type": "module" }\n',
        'inherited/tsconfig.json': '{ "extends": "../tsconfig.shared.json", "compilerOptions": { "module": "nodenext" }, "include": ["*.ts"] }\n',
        'inherited/main.ts': 'export const inherited = 1;\n',
    },
    broken: ['checked/tsconfig.json', '{ "compilerOptions": { "noEmit": true, "module": "no-such-module" }, "include": ["*.ts"] }\n'],
};

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('gives the root project no tsc target for a root tsconfig.json that only references projects', () =>
        inferred({ ...subject, files: { ...subject.files, 'tsconfig.json': '{ "files": [], "references": [{ "path": "./emitted" }, { "path": "./checked" }, { "path": "./inherited" }] }\n' } }).pipe(
            Effect.map(running('tsc')),
            Effect.map(Array.map(({ project }) => project)),
            Effect.map((compiled) => assert.deepStrictEqual(compiled.toSorted(), ['checked', 'emitted', 'inherited'], 'nrwl/nx@2a424d3 packages/js/src/plugins/typescript/plugin.spec.ts "should not create nodes for root tsconfig.json files"')),
        ),
    );

    it.effect('gives the root project no tsc target for a root tsconfig.json beside its sources', () =>
        inferred({ ...subject, files: { 'package.json': '{ "name": "root" }\n', 'tsconfig.json': '{}\n', 'src/index.ts': 'export const root = 1;\n' } }).pipe(
            Effect.map(running('tsc')),
            Effect.map((compiled) => assert.deepStrictEqual(compiled, [], 'nrwl/nx@2a424d3 packages/js/src/plugins/typescript/plugin.spec.ts "should not create nodes for root tsconfig.json files"')),
        ),
    );

    it.effect('compiles a tsconfig that references a folder the workspace lacks', () =>
        inferred({
            ...subject,
            files: {
                'package.json': '{ "name": "root", "workspaces": ["lib"] }\n',
                'lib/package.json': '{ "name": "lib", "type": "module" }\n',
                'lib/tsconfig.json': '{ "compilerOptions": { "outDir": "dist", "rootDir": ".", "declaration": true, "module": "nodenext" }, "include": ["*.ts"], "references": [{ "path": "../lib-2" }] }\n',
                'lib/index.ts': 'export const lib = 1;\n',
            },
        }).pipe(
            Effect.map(running('tsc')),
            Effect.map(Array.map(({ project }) => project)),
            Effect.map((compiled) => assert.deepStrictEqual(compiled, ['lib'], 'nrwl/nx@2a424d3 packages/js/src/plugins/typescript/plugin.spec.ts "should handle ts project references pointing to non-existing files and not throw"')),
        ),
    );

    it.effect('compiles each tsconfig with tsc, cached, writing under the outDir of a config that emits and nothing for a config that sets or inherits noEmit', () =>
        inferred(subject).pipe(
            Effect.map(running('tsc')),
            Effect.map(Array.map(({ project, cache, outputs }) => ({ project, cache, outputs }))),
            Effect.map((compiled) =>
                assert.deepStrictEqual(compiled, [
                    { project: 'checked', cache: true, outputs: [] },
                    { project: 'emitted', cache: true, outputs: ['emitted/dist'] },
                    { project: 'inherited', cache: true, outputs: [] },
                ]),
            ),
        ),
    );
});
