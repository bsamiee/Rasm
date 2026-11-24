import { mkdirSync, symlinkSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-typescript';
import { it, typescript } from '@rasm/testing/nx';
import { HashPlanInspector } from 'nx/src/hasher/hash-plan-inspector';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"root","workspaces":["libs/*"]}', 'package-lock.json': '' };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('eslint check inference', () => {
    it.for([
        { case: 'a root config governs a project.json project with a ts file', files: { 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'apps/app/project.json': '{"name":"app"}', 'apps/app/index.ts': 'export {};' }, linted: ['app'] },
        { case: 'a project config governs a workspace package', files: { 'libs/lib/eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/index.ts': 'export {};' }, linted: ['lib'] },
        { case: 'the config names json files a package holds', files: { 'eslint.config.mjs': "export default [{ files: ['**/executors.json'] }];\n", 'libs/lib/package.json': '{"name":"lib"}', 'libs/lib/executors.json': '{}' }, linted: ['lib'] },
        { case: 'no flat config exists', files: { 'apps/app/project.json': '{"name":"app"}', 'apps/app/index.ts': 'export {};' }, linted: [] },
        { case: 'a project holds no file the config lints', files: { 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'apps/app/project.json': '{"name":"app"}', 'apps/app/one.png': '...', 'apps/app/two.css': '...' }, linted: [] },
        { case: 'the config ignores every file of the project', files: { 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }, { ignores: ['apps/**'] }];\n", 'apps/app/project.json': '{"name":"app"}', 'apps/app/index.ts': 'export {};' }, linted: [] },
        {
            case: 'a project holds only the lintable files of a project nested in it',
            files: { 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'apps/app/project.json': '{"name":"app"}', 'apps/app/README.md': '# app', 'apps/app/nested/project.json': '{"name":"nested"}', 'apps/app/nested/index.ts': 'export {};' },
            linted: ['nested'],
        },
        { case: 'the workspace root holds a src folder', files: { 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'src/index.ts': 'export {};' }, linted: ['root'] },
        { case: 'the workspace root holds lintable files outside src and lib', files: { 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'scripts/index.ts': 'export {};' }, linted: [] },
    ])('lints the projects $linted when $case', async ({ files, linted }, { workspace }) => {
        const graph = await workspace
            .install(typescript, import.meta.resolve('eslint/package.json'))
            .write({ ...ROOT, ...files })
            .graph({ createNodes });

        expect(
            Object.values(graph.nodes)
                .filter(({ data: { targets = {} } }) => 'check--eslint' in targets)
                .map(({ name }) => name)
                .toSorted(),
        ).toStrictEqual(linted);
    });

    it('lints only the src folder of the workspace root, leaving nested projects to their own target', async ({ workspace }) => {
        workspace.install(typescript, import.meta.resolve('eslint/package.json')).write({
            ...ROOT,
            'eslint.config.mjs': "export default [{ files: ['**/*.ts'], rules: { 'no-debugger': 'error' } }];\n",
            'src/index.ts': 'export {};\n',
            'apps/app/project.json': '{"name":"app"}',
            'apps/app/index.ts': 'debugger;\n',
        });
        const graph = await workspace.graph({ createNodes });

        const [root, app] = [await workspace.run(graph, 'root', 'check--eslint'), await workspace.run(graph, 'app', 'check--eslint')];

        expect([root.success, app.success, app.terminalOutput.includes('no-debugger')]).toStrictEqual([true, false, true]);
    });

    it('hashes every file the config loads, its local modules and the sources of a workspace package it imports', async ({ workspace }) => {
        workspace.install(typescript, import.meta.resolve('eslint/package.json')).write({
            ...ROOT,
            'eslint.config.mjs': "import { files } from './tools/lint/files.mjs';\nimport { rules } from '@scope/rules';\nimport 'node:path';\n\nexport default [{ files, rules }];\n",
            'tools/lint/files.mjs': "export const files = ['**/*.ts'];\n",
            'tools/lint/unused.mjs': 'export {};\n',
            'libs/rules/package.json': '{"name":"@scope/rules","type":"module","exports":"./index.mjs"}',
            'libs/rules/index.mjs': "export const rules = { 'no-debugger': 'error' };\n",
            'apps/app/project.json': '{"name":"app"}',
            'apps/app/index.ts': 'export {};',
        });
        mkdirSync(join(workspace.root, 'node_modules/@scope'));
        symlinkSync(join(workspace.root, 'libs/rules'), join(workspace.root, 'node_modules/@scope/rules'), 'dir');
        const graph = await workspace.graph({ createNodes });
        const inspector = new HashPlanInspector({ ...graph, externalNodes: { 'npm:eslint': { type: 'npm', name: 'npm:eslint', data: { packageName: 'eslint', version: '10.0.0' } } } }, workspace.root);
        await inspector.init();

        const files = inspector.inspectTaskInputs({ project: 'app', target: 'check--eslint' })['app:check--eslint']?.files ?? [];

        expect([['eslint.config.mjs', 'tools/lint/files.mjs', 'libs/rules/index.mjs'].filter((file) => !files.includes(file)), files.includes('tools/lint/unused.mjs')]).toStrictEqual([[], false]);
    });

    it('reads the imports of a module the config imports again after it changes', async ({ workspace }) => {
        workspace.install(typescript, import.meta.resolve('eslint/package.json')).write({
            ...ROOT,
            'eslint.config.mjs': "import { files } from './tools/lint/files.mjs';\n\nexport default [{ files }];\n",
            'tools/lint/files.mjs': "export const files = ['**/*.ts'];\n",
            'apps/app/project.json': '{"name":"app"}',
            'apps/app/index.ts': 'export {};',
        });
        await workspace.graph({ createNodes });

        const graph = await workspace.write({ 'tools/lint/files.mjs': "import 'typescript/package.json' with { type: 'json' };\n\nexport const files = ['**/*.ts'];\n" }).graph({ createNodes });

        expect(graph.nodes['app']?.data.targets?.['check--eslint']?.inputs).toContainEqual({ externalDependencies: ['eslint', 'typescript'] });
    });

    it('infers the target it read before once the compiler stops resolving, while the config, the modules it imports, and the project files are unchanged', async ({ workspace }) => {
        workspace.install(typescript, import.meta.resolve('eslint/package.json')).write({ ...ROOT, 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'apps/app/project.json': '{"name":"app"}', 'apps/app/index.ts': 'export {};' });
        const fresh = await workspace.graph({ createNodes });

        const cached = await workspace.remove('node_modules/typescript').graph({ createNodes });

        expect(fresh.nodes['app']?.data.targets?.['check--eslint']).toBeDefined();
        expect(cached.nodes['app']?.data.targets?.['check--eslint']).toStrictEqual(fresh.nodes['app']?.data.targets?.['check--eslint']);
    });

    it.for([
        { config: "export default [{ files: ['**/*.json'] }];\n", upstream: false },
        { config: "export default [{ files: ['**/*.ts'] }];\n", upstream: false },
        { config: "export default [{ files: ['**/*.ts'], languageOptions: { parserOptions: { projectService: true } } }];\n", upstream: true },
        { config: "export default [{ files: ['**/*.ts'], settings: { 'import/resolver': { typescript: true } } }];\n", upstream: true },
    ])('hashes the upstream sources for check--eslint when a linted file reads them: $upstream', async ({ config, upstream }, { workspace }) => {
        const graph = await workspace
            .install(typescript, import.meta.resolve('eslint/package.json'))
            .write({ ...ROOT, 'eslint.config.mjs': config, 'apps/app/project.json': '{"name":"app"}', 'apps/app/package.json': '{"name":"app"}', 'apps/app/index.ts': 'export {};' })
            .graph({ createNodes });

        expect(graph.nodes['app']?.data.targets?.['check--eslint']?.inputs?.includes('^default')).toBe(upstream);
    });

    it('hashes the files a type-aware project tsconfig extends outside the project, except the root tsconfig and installed presets', async ({ workspace }) => {
        const graph = await workspace
            .install(typescript, import.meta.resolve('eslint/package.json'))
            .write({
                ...ROOT,
                'eslint.config.mjs': "export default [{ files: ['**/*.ts'], languageOptions: { parserOptions: { projectService: true } } }];\n",
                'tsconfig.base.json': '{"extends":"./tsconfig.root.json"}',
                'tsconfig.root.json': '{}',
                'tsconfig.shared.json': '{}',
                'node_modules/@some/preset/package.json': '{"name":"@some/preset"}',
                'node_modules/@some/preset/tsconfig.json': '{}',
                'apps/app/project.json': '{"name":"app"}',
                'apps/app/tsconfig.json': '{"extends":["../../tsconfig.shared.json","../../tsconfig.base.json","@some/preset/tsconfig.json"],"files":["index.ts"],"compilerOptions":{"noEmit":true}}',
                'apps/app/index.ts': 'export {};',
            })
            .graph({ createNodes });

        expect(graph.nodes['app']?.data.targets?.['check--eslint']?.inputs?.filter((input) => typeof input === 'string' && input.includes('tsconfig')).toSorted()).toStrictEqual(['{workspaceRoot}/tsconfig.root.json', '{workspaceRoot}/tsconfig.shared.json']);
    });
});
