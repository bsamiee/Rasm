import { assert } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-vitest';
import manifest from '@rasm/nx-vitest/package.json' with { type: 'json' };
import { Array, Effect, Order, Predicate, Record } from 'effect';
import { cases, inferred, running, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TEST = "import { expect, it } from 'vitest';\n\nit('adds', () => {\n    expect(1 + 1).toBe(2);\n});\n";
const CONFIG = "import { defineConfig } from 'vitest/config';\n\nexport default defineConfig({ test: { include: ['*.test.ts'] } });\n";

const subject: Subject = {
    plugin: { name: manifest.name, createNodes },
    packages: ['vitest', 'vite'],
    runs: ['test'],
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

const PLUGIN = 'nrwl/nx@2a424d3 packages/vitest/src/plugins/plugin-vitest.spec.ts';
const TYPED = "import { expectTypeOf, it } from 'vitest';\n\nit('types', () => {\n    expectTypeOf(1).toBeNumber();\n});\n";
const DISCOVERED = [
    { label: 'an include that names its test file', test: "include: ['*.check.ts']", files: { 'found/sum.check.ts': TEST }, tested: ['found'], source: '"should honor a config include/exclude over Vitest defaults"' },
    { label: 'an include that names no file', test: "include: ['*.check.ts']", files: { 'found/sum.test.ts': TEST }, tested: [], source: '"should honor a config include/exclude over Vitest defaults"' },
    { label: 'an exclude that names its only test file', test: "include: ['*.test.ts'], exclude: ['sum.test.ts']", files: { 'found/sum.test.ts': TEST }, tested: [], source: '"should honor a config include/exclude over Vitest defaults"' },
    {
        label: 'an includeSource file that holds the import.meta.vitest marker',
        test: "include: ['*.check.ts'], includeSource: ['*.ts']",
        files: { 'found/sum.ts': "export const sum = (left: number, right: number) => left + right;\n\nif (import.meta.vitest) {\n    import.meta.vitest.it('adds', () => {\n        import.meta.vitest?.expect(sum(1, 1)).toBe(2);\n    });\n}\n" },
        tested: ['found'],
        source: '"should keep only in-source files that contain the vitest marker"',
    },
    { label: 'an includeSource file without the marker', test: "include: ['*.check.ts'], includeSource: ['*.ts']", files: { 'found/sum.ts': 'export const sum = 2;\n' }, tested: [], source: '"should keep only in-source files that contain the vitest marker"' },
    { label: 'typecheck.only beside a runtime test alone', test: "include: ['*.test.ts'], typecheck: { enabled: true, only: true }", files: { 'found/sum.test.ts': TEST }, tested: [], source: '"should skip the regular test glob when typecheck.only is set"' },
    { label: 'typecheck.only beside a type test', test: "include: ['*.test.ts'], typecheck: { enabled: true, only: true }", files: { 'found/sum.test-d.ts': TYPED }, tested: ['found'], source: '"should skip the regular test glob when typecheck.only is set"' },
    { label: 'typecheck.enabled beside a type test alone', test: "include: ['*.test.ts'], typecheck: { enabled: true }", files: { 'found/sum.test-d.ts': TYPED }, tested: ['found'], source: '"should also glob type tests when typecheck is enabled"' },
] as const;
const REPORTED = [
    { reportsDirectory: 'reports/coverage', source: '"should resolve a relative vitest root against the project root"' },
    { reportsDirectory: '..coverage', source: '"should treat a reportsDirectory name starting with dots as inside the project"' },
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const configured = (test: string) => `import { defineConfig } from 'vitest/config';\n\nexport default defineConfig({ test: { ${test} } });\n`;

const distributed = (graph: ProjectGraph) => {
    const targets = Object.values(graph.nodes)
        .flatMap((node) => (node.name === 'unit' ? Record.toEntries(node.data.targets ?? {}) : []))
        .filter(([name]) => name.startsWith('test-ci'));
    return {
        names: targets.map(([name]) => name).toSorted(Order.String),
        cached: targets.every(([, { cache }]) => cache === true),
        parents: targets.filter(([name]) => name === 'test-ci').map(([, { executor, dependsOn = [] }]) => ({ executor, after: dependsOn.map((entry) => (Predicate.isString(entry) ? entry : entry.target)) })),
    };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('gives the root project no test target for a root config that lists test.projects', () =>
        inferred({ ...subject, files: { ...subject.files, 'vitest.config.ts': "import { defineConfig } from 'vitest/config';\n\nexport default defineConfig({ test: { projects: ['unit', 'wide'] } });\n" } }).pipe(
            Effect.map(running('vitest run')),
            Effect.map(Array.map(({ project }) => project)),
            Effect.map((tested) => assert.notInclude(tested, 'root', 'nrwl/nx@2a424d3 packages/vitest/src/plugins/plugin-vitest.spec.ts "should NOT create a project for root config with projects array"')),
        ),
    );

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

    it.effect.each(DISCOVERED)('runs vitest run for $tested where the config states $label', ({ test, files, tested, source }) =>
        inferred({ ...subject, files: { 'package.json': '{ "name": "root", "workspaces": ["found"] }\n', 'found/package.json': '{ "name": "found", "type": "module" }\n', 'found/vitest.config.ts': configured(test), ...files } }).pipe(
            Effect.map(running('vitest run')),
            Effect.map(Array.map(({ project }) => project)),
            Effect.map((ran) => assert.deepStrictEqual(ran, [...tested], `${PLUGIN} ${source}`)),
        ),
    );

    it.effect.each(REPORTED)('writes coverage to the reportsDirectory $reportsDirectory the config states, inside the project', ({ reportsDirectory, source }) =>
        inferred({ ...subject, files: { ...subject.files, 'unit/vitest.config.ts': configured(`include: ['*.test.ts'], coverage: { reportsDirectory: '${reportsDirectory}' }`) } }).pipe(
            Effect.map(running('vitest run')),
            Effect.map(Array.filter(({ project }) => project === 'unit')),
            Effect.map(Array.flatMap(({ outputs }) => outputs)),
            Effect.map((outputs) => assert.deepStrictEqual([outputs.includes(`unit/${reportsDirectory}`), outputs.includes('unit/coverage')], [true, false], `nrwl/nx@2a424d3 packages/vitest/src/plugins/plugin-vitest-glob.spec.ts ${source}`)),
        ),
    );

    it.effect('gives each test file one cached target under ciTargetName, sorted by file, and a cached nx:noop parent after every one', () =>
        Effect.map(inferred({ ...subject, options: { ciTargetName: 'test-ci' }, files: { ...subject.files, 'unit/product.test.ts': TEST } }), (graph) =>
            assert.deepStrictEqual(
                distributed(graph),
                { names: ['test-ci', 'test-ci--product.test.ts', 'test-ci--sum.test.ts'], cached: true, parents: [{ executor: 'nx:noop', after: ['test-ci--product.test.ts', 'test-ci--sum.test.ts'] }] },
                `${PLUGIN} "should create nodes with ci target name", "should sort atomized target names when test discovery returns shuffled paths"`,
            ),
        ),
    );
});
