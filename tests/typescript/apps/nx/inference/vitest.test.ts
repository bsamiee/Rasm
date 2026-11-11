import { assert } from '@effect/vitest';
import { createNodes } from '@rasm/nx-vitest';
import manifest from '@rasm/nx-vitest/package.json' with { type: 'json' };
import { Array, Effect } from 'effect';
import { cases, inferred, running, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TEST = "import { expect, it } from 'vitest';\n\nit('adds', () => {\n    expect(1 + 1).toBe(2);\n});\n";
const CONFIG = "import { defineConfig } from 'vitest/config';\n\nexport default defineConfig({ test: { include: ['*.test.ts'] } });\n";

const subject: Subject = {
    plugin: { name: manifest.name, createNodes },
    packages: ['vitest', 'vite'],
    files: {
        'package.json': '{ "name": "root", "workspaces": ["unit", "idle", "wide"] }\n',
        'unit/package.json': '{ "name": "unit", "type": "module" }\n',
        'unit/vitest.config.ts': CONFIG,
        'unit/sum.test.ts': TEST,
        'wide/package.json': '{ "name": "wide", "type": "module" }\n',
        'wide/vitest.config.ts': CONFIG,
        'wide/sum.test.ts': TEST,
        'idle/package.json': '{ "name": "idle", "type": "module" }\n',
        'idle/vitest.config.ts': CONFIG,
    },
    broken: ['unit/vitest.config.ts', "import { defineConfig } from 'vitest/config';\n\nexport default defineConfig({ test: { include:\n"],
};

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('runs vitest run, cached on the CI variable Vitest reads and writing its coverage folder, for each config that lists a test file and for no config that lists none', () =>
        inferred(subject).pipe(
            Effect.map(running('vitest run')),
            Effect.map(Array.map(({ project, cache, env, outputs }) => ({ project, cache, ci: env.includes('CI'), covered: outputs.includes(`${project}/coverage`) }))),
            Effect.map((tested) =>
                assert.deepStrictEqual(tested, [
                    { project: 'unit', cache: true, ci: true, covered: true },
                    { project: 'wide', cache: true, ci: true, covered: true },
                ]),
            ),
        ),
    );
});
