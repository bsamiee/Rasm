import { assert } from '@effect/vitest';
import { createNodes } from '@rasm/nx-typescript';
import manifest from '@rasm/nx-typescript/package.json' with { type: 'json' };
import { Array, Effect } from 'effect';
import { cases, inferred, running, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const subject: Subject = {
    plugin: { name: manifest.name, createNodes },
    files: {
        'package.json': '{ "name": "root", "workspaces": ["emitted", "checked"] }\n',
        'emitted/package.json': '{ "name": "emitted", "type": "module" }\n',
        'emitted/tsconfig.json': '{ "compilerOptions": { "outDir": "dist", "rootDir": ".", "declaration": true, "module": "nodenext" }, "include": ["*.ts"] }\n',
        'emitted/index.ts': 'export const emitted = 1;\n',
        'checked/package.json': '{ "name": "checked", "type": "module" }\n',
        'checked/tsconfig.json': '{ "compilerOptions": { "noEmit": true, "module": "nodenext" }, "include": ["*.ts"] }\n',
        'checked/main.ts': 'export const checked = 1;\n',
    },
    broken: ['checked/tsconfig.json', '{ "compilerOptions": { "noEmit": true, "module": "no-such-module" }, "include": ["*.ts"] }\n'],
};

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('compiles each tsconfig with tsc, cached, writing under the outDir of a config that emits and nothing for a config that sets noEmit', () =>
        inferred(subject).pipe(
            Effect.map(running('tsc')),
            Effect.map(Array.map(({ project, cache, outputs }) => ({ project, cache, outputs }))),
            Effect.map((compiled) =>
                assert.deepStrictEqual(compiled, [
                    { project: 'checked', cache: true, outputs: [] },
                    { project: 'emitted', cache: true, outputs: ['emitted/dist'] },
                ]),
            ),
        ),
    );
});
