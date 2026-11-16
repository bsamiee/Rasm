import { symlinkSync } from 'node:fs';
import { dirname, join, matchesGlob } from 'node:path';
import { fileURLToPath } from 'node:url';
import type { CreateNodesContext } from '@nx/devkit';
import { setupWorkspaceContext } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-typescript';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const INSTALLED = { typescript: import.meta.resolve('@typescript/native/package.json'), eslint: import.meta.resolve('eslint/package.json') } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const linted = (cwd: string, config: string, inputs: readonly string[]) => ({
    command: 'eslint .',
    options: { cwd },
    cache: true,
    inputs: ['default', '^default', ...inputs, { externalDependencies: ['eslint'] }],
    outputs: [],
    metadata: { technologies: ['eslint'], description: `Lints the files ${config} governs in the project with eslint`, help: { command: 'npx eslint --help', example: { args: ['--max-warnings', '0'] } } },
});

const invokeCreateNodesOnMatchingFiles = async (tempFs: TempFs, context: CreateNodesContext, fileSys: Record<string, string>) => {
    await tempFs.createFiles({ 'package-lock.json': '{}', 'package.json': JSON.stringify({ workspaces: ['**'] }), ...fileSys });
    setupWorkspaceContext(tempFs.tempDir);
    const results = await createNodes[1](
        Object.keys(fileSys).filter((file) => matchesGlob(file, createNodes[0])),
        {},
        context,
    );
    return Object.fromEntries(
        results.flatMap(([, nodes]) =>
            Object.entries({ ...nodes.projects }).flatMap(([root, project]) =>
                Object.entries({ ...project.targets })
                    .filter(([name]) => name === 'check--eslint')
                    .map(([, target]) => [root, target]),
            ),
        ),
    );
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('eslint-plugin');
        const previous = process.cwd();
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            tempFs.cleanup();
        });
        Object.entries(INSTALLED).forEach(([name, from]) => {
            tempFs.createDirSync('node_modules');
            symlinkSync(dirname(fileURLToPath(from)), join(tempFs.tempDir, 'node_modules', name), 'dir');
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: { namedInputs: { default: ['{projectRoot}/**/*'], production: ['!{projectRoot}/**/*.spec.ts'] } }, workspaceRoot: tempFs.tempDir }));

describe('@nx/eslint/plugin', () => {
    it('should not create any nodes when there are no eslint configs', async ({ tempFs, context }) => {
        await expect(invokeCreateNodesOnMatchingFiles(tempFs, context, { 'project.json': '{}', 'apps/my-app/project.json': '{}', 'apps/my-app/index.ts': "console.log('hello world')" })).resolves.toStrictEqual({});
    });

    describe('root eslint config only', () => {
        it('should not create any nodes for just a package.json and root level eslint config', async ({ tempFs, context }) => {
            await expect(invokeCreateNodesOnMatchingFiles(tempFs, context, { 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'package.json': '{}' })).resolves.toStrictEqual({});
        });

        it.for(['src', 'lib'])('should create a node for just a package.json and root level eslint config if accompanied by a %s directory', async (folder, { tempFs, context }) => {
            await expect(
                invokeCreateNodesOnMatchingFiles(tempFs, context, {
                    'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n",
                    'package.json': '{}',
                    [`${folder}/index.ts`]: "console.log('hello world')",
                }),
            ).resolves.toStrictEqual({ '.': { ...linted('.', 'eslint.config.mjs', ['{projectRoot}/eslint.config.mjs']), command: `eslint ./${folder}` } });
        });

        it('should not create a node for just a package.json and root level eslint config if accompanied by a src directory when all files are ignored', async ({ tempFs, context }) => {
            await expect(
                invokeCreateNodesOnMatchingFiles(tempFs, context, {
                    'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }, { ignores: ['**/*'] }];\n",
                    'package.json': '{}',
                    'src/index.ts': "console.log('hello world')",
                }),
            ).resolves.toStrictEqual({});
        });

        it('should create a node for a nested project (with a project.json and any lintable file) which does not have its own eslint config if accompanied by a root level eslint config', async ({ tempFs, context }) => {
            await expect(
                invokeCreateNodesOnMatchingFiles(tempFs, context, {
                    'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n",
                    'apps/my-app/project.json': '{}',
                    'apps/my-app/index.ts': "console.log('hello world')",
                }),
            ).resolves.toStrictEqual({ 'apps/my-app': linted('apps/my-app', 'eslint.config.mjs', ['{workspaceRoot}/eslint.config.mjs']) });
        });

        it('should create a node for a nested project (with a package.json and any lintable file) which does not have its own eslint config if accompanied by a root level eslint config', async ({ tempFs, context }) => {
            await expect(
                invokeCreateNodesOnMatchingFiles(tempFs, context, {
                    'eslint.config.mjs': "import { defineConfig } from 'eslint/config';\n\nexport default defineConfig({ files: ['**/*.ts'] });\n",
                    'apps/my-app/package.json': '{}',
                    'apps/my-app/index.ts': "console.log('hello world')",
                    'apps/my-app/README.md': '# my-app',
                }),
            ).resolves.toStrictEqual({ 'apps/my-app': linted('apps/my-app', 'eslint.config.mjs', ['{workspaceRoot}/eslint.config.mjs']) });
        });

        it.for(['package.json', 'project.json'])('should not create a node for a nested project (with a %s and no lintable files) which does not have its own eslint config if accompanied by a root level eslint config', async (manifest, { tempFs, context }) => {
            await expect(
                invokeCreateNodesOnMatchingFiles(tempFs, context, {
                    'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n",
                    [`apps/my-app/${manifest}`]: '{}',
                    'apps/my-app/one.png': '...',
                    'apps/my-app/three.css': '...',
                    'apps/my-app/config-one.yaml': '...',
                }),
            ).resolves.toStrictEqual({});
        });

        it.for(['package.json', 'project.json'])('should not create a node for a nested project (with a %s and all files ignored) which does not have its own eslint config if accompanied by a root level eslint config', async (manifest, { tempFs, context }) => {
            await expect(
                invokeCreateNodesOnMatchingFiles(tempFs, context, {
                    'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }, { ignores: ['**/*'] }];\n",
                    [`apps/my-app/${manifest}`]: '{}',
                    'apps/my-app/index.ts': "console.log('hello world')",
                }),
            ).resolves.toStrictEqual({});
        });

        it('should create a node for a project whose only lintable files are the json files the config states', async ({ tempFs, context }) => {
            await expect(
                invokeCreateNodesOnMatchingFiles(tempFs, context, {
                    'eslint.config.mjs': "export default [{ files: ['**/package.json', '**/executors.json'] }];\n",
                    'apps/my-plugin/package.json': '{}',
                    'apps/my-plugin/executors.json': '{}',
                }),
            ).resolves.toStrictEqual({ 'apps/my-plugin': linted('apps/my-plugin', 'eslint.config.mjs', ['{workspaceRoot}/eslint.config.mjs']) });
        });
    });

    describe('nested eslint configs only', () => {
        it('should create appropriate nodes for nested projects without a root level eslint config', async ({ tempFs, context }) => {
            await expect(
                invokeCreateNodesOnMatchingFiles(tempFs, context, {
                    'apps/my-app/eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n",
                    'apps/my-app/project.json': '{}',
                    'apps/my-app/index.ts': "console.log('hello world')",
                    'libs/my-lib/eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n",
                    'libs/my-lib/project.json': '{}',
                    'libs/my-lib/index.ts': "console.log('hello world')",
                }),
            ).resolves.toStrictEqual({
                'apps/my-app': linted('apps/my-app', 'apps/my-app/eslint.config.mjs', ['{projectRoot}/eslint.config.mjs']),
                'libs/my-lib': linted('libs/my-lib', 'libs/my-lib/eslint.config.mjs', ['{projectRoot}/eslint.config.mjs']),
            });
        });
    });

    describe('root eslint config and nested eslint configs', () => {
        it('should create appropriate nodes for a nested project without its own eslint config but with an orphaned eslint config in its parent hierarchy', async ({ tempFs, context }) => {
            await expect(
                invokeCreateNodesOnMatchingFiles(tempFs, context, {
                    'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n",
                    'apps/eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n",
                    'apps/myapp/project.json': '{}',
                    'apps/myapp/index.ts': 'console.log("hello world")',
                }),
            ).resolves.toStrictEqual({ 'apps/myapp': linted('apps/myapp', 'apps/eslint.config.mjs', ['{workspaceRoot}/apps/eslint.config.mjs']) });
        });

        it('should handle multiple levels of nesting and ignored files correctly', async ({ tempFs, context }) => {
            await expect(
                invokeCreateNodesOnMatchingFiles(tempFs, context, {
                    'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }, { ignores: ['**/*'] }];\n",
                    'apps/myapp/project.json': '{}',
                    'apps/myapp/index.ts': 'console.log("hello world")',
                    'apps/myapp/nested/mylib/eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n",
                    'apps/myapp/nested/mylib/project.json': '{}',
                    'apps/myapp/nested/mylib/index.ts': 'console.log("hello world")',
                }),
            ).resolves.toStrictEqual({ 'apps/myapp/nested/mylib': linted('apps/myapp/nested/mylib', 'apps/myapp/nested/mylib/eslint.config.mjs', ['{projectRoot}/eslint.config.mjs']) });
        });

        it('should add each local module the config imports as an input and each package any of them imports as an external dependency', async ({ tempFs, context }) => {
            const targets = await invokeCreateNodesOnMatchingFiles(tempFs, context, {
                'eslint.config.mjs': "import { defineConfig } from 'eslint/config';\nimport { files } from './tools/lint/files.mjs';\nimport 'node:path';\n\nexport default defineConfig({ files });\n",
                'tools/lint/files.mjs': "import manifest from 'typescript/package.json' with { type: 'json' };\n\nexport const files = manifest.name === 'typescript' ? ['**/*.ts'] : [];\n",
                'apps/my-app/project.json': '{}',
                'apps/my-app/index.ts': 'export {};',
            });

            expect(targets['apps/my-app']?.inputs).toStrictEqual(['default', '^default', '{workspaceRoot}/eslint.config.mjs', '{workspaceRoot}/tools/lint/files.mjs', { externalDependencies: ['eslint', 'typescript'] }]);
        });

        it('should read the imports of a local module the config imports again once that module changes', async ({ tempFs, context }) => {
            const files = { 'eslint.config.mjs': "import { files } from './tools/lint/files.mjs';\n\nexport default [{ files }];\n", 'tools/lint/files.mjs': "export const files = ['**/*.ts'];\n", 'apps/my-app/project.json': '{}', 'apps/my-app/index.ts': 'export {};' };
            const before = await invokeCreateNodesOnMatchingFiles(tempFs, context, files);
            const after = await invokeCreateNodesOnMatchingFiles(tempFs, context, { ...files, 'tools/lint/files.mjs': "import 'typescript/package.json' with { type: 'json' };\n\nexport const files = ['**/*.ts'];\n" });

            expect(before['apps/my-app']?.inputs?.at(-1)).toStrictEqual({ externalDependencies: ['eslint'] });
            expect(after['apps/my-app']?.inputs?.at(-1)).toStrictEqual({ externalDependencies: ['eslint', 'typescript'] });
        });
    });

    describe('tsconfig extends chain inputs', () => {
        const app = { 'eslint.config.mjs': "export default [{ files: ['**/*.ts'] }];\n", 'apps/my-app/project.json': '{}', 'apps/my-app/index.ts': "console.log('hello world')" };

        it('should not add tsconfig inputs when the project has no tsconfig.json', async ({ tempFs, context }) => {
            const targets = await invokeCreateNodesOnMatchingFiles(tempFs, context, app);

            expect(targets['apps/my-app']?.inputs).not.toContainEqual(expect.stringMatching(/tsconfig/iu));
        });

        it.for([
            ['has no extends', {}],
            ['extends a file inside the project root', { 'apps/my-app/tsconfig.json': JSON.stringify({ extends: './tsconfig.lib.json' }), 'apps/my-app/tsconfig.lib.json': '{}' }],
        ] as const)('should not add tsconfig inputs when tsconfig.json %s', async ([, files], { tempFs, context }) => {
            const targets = await invokeCreateNodesOnMatchingFiles(tempFs, context, { ...app, 'apps/my-app/tsconfig.json': '{}', ...files });

            expect(targets['apps/my-app']?.inputs).not.toContainEqual(expect.stringContaining('tsconfig'));
        });

        it('should exclude the root tsconfig from inputs since it is handled by the native selective hasher', async ({ tempFs, context }) => {
            const targets = await invokeCreateNodesOnMatchingFiles(tempFs, context, { ...app, 'tsconfig.base.json': '{}', 'apps/my-app/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.base.json' }) });

            expect(targets['apps/my-app']?.inputs).not.toContain('{workspaceRoot}/tsconfig.base.json');
        });

        it('should add the tsconfig file to inputs when extends points outside the project root', async ({ tempFs, context }) => {
            const targets = await invokeCreateNodesOnMatchingFiles(tempFs, context, { ...app, 'tsconfig.shared.json': '{}', 'apps/my-app/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.shared.json' }) });

            expect(targets['apps/my-app']?.inputs).toContain('{workspaceRoot}/tsconfig.shared.json');
        });

        it('should add every file in a transitive extends chain that lives outside the project root except the root tsconfig', async ({ tempFs, context }) => {
            const targets = await invokeCreateNodesOnMatchingFiles(tempFs, context, {
                ...app,
                'tsconfig.root.json': '{}',
                'tsconfig.base.json': JSON.stringify({ extends: './tsconfig.root.json' }),
                'apps/my-app/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.base.json' }),
            });

            expect(targets['apps/my-app']?.inputs).not.toContain('{workspaceRoot}/tsconfig.base.json');
            expect(targets['apps/my-app']?.inputs).toContain('{workspaceRoot}/tsconfig.root.json');
        });

        it('should add every file when extends is an array', async ({ tempFs, context }) => {
            const targets = await invokeCreateNodesOnMatchingFiles(tempFs, context, { ...app, 'tsconfig.a.json': '{}', 'tsconfig.b.json': '{}', 'apps/my-app/tsconfig.json': JSON.stringify({ extends: ['../../tsconfig.a.json', '../../tsconfig.b.json'] }) });

            expect(targets['apps/my-app']?.inputs).toContain('{workspaceRoot}/tsconfig.a.json');
            expect(targets['apps/my-app']?.inputs).toContain('{workspaceRoot}/tsconfig.b.json');
        });

        it('should drop shareable tsconfig packages resolved from node_modules', async ({ tempFs, context }) => {
            const targets = await invokeCreateNodesOnMatchingFiles(tempFs, context, {
                ...app,
                'node_modules/@some/preset/package.json': JSON.stringify({ name: '@some/preset' }),
                'node_modules/@some/preset/tsconfig.json': '{}',
                'apps/my-app/tsconfig.json': JSON.stringify({ extends: '@some/preset/tsconfig.json' }),
            });

            expect(targets['apps/my-app']?.inputs).not.toContainEqual(expect.stringContaining('node_modules'));
            expect(targets['apps/my-app']?.inputs).not.toContainEqual(expect.stringContaining('@some/preset'));
        });

        it('should not crash on a self-referential extends cycle', async ({ tempFs, context }) => {
            const targets = await invokeCreateNodesOnMatchingFiles(tempFs, context, {
                ...app,
                'tsconfig.a.json': JSON.stringify({ extends: './tsconfig.b.json' }),
                'tsconfig.b.json': JSON.stringify({ extends: './tsconfig.a.json' }),
                'apps/my-app/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.a.json' }),
            });

            expect(targets['apps/my-app']?.inputs).toContain('{workspaceRoot}/tsconfig.a.json');
            expect(targets['apps/my-app']?.inputs).toContain('{workspaceRoot}/tsconfig.b.json');
        });
    });
});
