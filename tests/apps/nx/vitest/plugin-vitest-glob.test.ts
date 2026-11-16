import { symlinkSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import type { CreateNodesContext } from '@nx/devkit';
import { createNodes } from '@rasm/nx-vitest';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('temp', async ({}, { onCleanup }) => {
        const temp = new TempFs('vitest-plugin-glob');
        const previous = process.cwd();
        process.chdir(temp.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            temp.cleanup();
        });
        temp.createDirSync('node_modules');
        ['vitest', 'vite'].forEach((name) => symlinkSync(dirname(fileURLToPath(import.meta.resolve(`${name}/package.json`))), join(temp.tempDir, 'node_modules', name), 'dir'));
        await temp.createFiles({ 'nx.json': '{}', 'package.json': '{"name":"repo","workspaces":["libs/*"]}', '.gitignore': 'node_modules/\ngenerated/\n' });
        return temp;
    })
    .extend('context', ({ temp }): CreateNodesContext => ({
        nxJsonConfiguration: { targetDefaults: {}, namedInputs: { default: ['{projectRoot}/**/*'], production: ['!{projectRoot}/**/*.spec.ts'] } },
        workspaceRoot: temp.tempDir,
    }));

describe('@rasm/nx-vitest discovery against a real filesystem', () => {
    const [, createNodesFunction] = createNodes;

    describe('setup file inputs', () => {
        it('should declare a setup file outside the project root, and its tsconfig', async ({ temp, context }) => {
            await temp.createFiles({
                'libs/lib1/vitest.config.ts': "export default { root: import.meta.dirname, test: { setupFiles: ['../../tools/vitest/setup.mts'] } };\n",
                'libs/lib1/package.json': '{"name":"lib1"}',
                'libs/lib1/src/a.spec.ts': '',
                'tools/vitest/setup.mts': '',
                'tools/vitest/tsconfig.json': '{"compilerOptions":{}}',
            });

            const nodes = await createNodesFunction(['libs/lib1/vitest.config.ts'], {}, context);

            expect(nodes[0]?.[1].projects?.['libs/lib1']?.targets?.['test']?.inputs).toContainEqual('{workspaceRoot}/tools/vitest/setup.mts');
            expect(nodes[0]?.[1].projects?.['libs/lib1']?.targets?.['test']?.inputs).toContainEqual({ json: '{workspaceRoot}/tools/vitest/tsconfig.json', fields: ['compilerOptions'] });
        });

        it('should declare it for a config that authors no root', async ({ temp, context }) => {
            await temp.createFiles({
                'libs/lib1/vitest.config.ts': "export default { test: { setupFiles: ['../../tools/vitest/setup.mts'] } };\n",
                'libs/lib1/package.json': '{"name":"lib1"}',
                'libs/lib1/src/a.spec.ts': '',
                'tools/vitest/setup.mts': '',
            });

            const nodes = await createNodesFunction(['libs/lib1/vitest.config.ts'], {}, context);

            expect(nodes[0]?.[1].projects?.['libs/lib1']?.targets?.['test']?.inputs).toContainEqual('{workspaceRoot}/tools/vitest/setup.mts');
        });

        it('should resolve entries against an authored root outside the project', async ({ temp, context }) => {
            await temp.createFiles({
                'libs/lib1/vitest.config.ts': `export default { root: ${JSON.stringify(join(temp.tempDir, 'libs'))}, test: { setupFiles: ['./shared/setup.mts'] } };\n`,
                'libs/lib1/package.json': '{"name":"lib1"}',
                'libs/lib1/src/a.spec.ts': '',
                'libs/shared/setup.mts': '',
            });

            const nodes = await createNodesFunction(['libs/lib1/vitest.config.ts'], {}, context);

            expect(Object.values({ ...nodes[0]?.[1].projects })[0]?.targets?.['test']?.inputs).toContainEqual('{workspaceRoot}/libs/shared/setup.mts');
        });

        it('should declare a setup file and tsconfig created after the targets were cached', async ({ temp, context }) => {
            await temp.createFiles({
                'libs/lib1/vitest.config.ts': `// ${temp.tempDir}\nexport default { test: { setupFiles: ['../../tools/vitest/setup.mts'] } };\n`,
                'libs/lib1/package.json': '{"name":"lib1"}',
                'libs/lib1/src/a.spec.ts': '',
            });
            await createNodesFunction(['libs/lib1/vitest.config.ts'], { outputPath: 'cached-test' }, context);
            await temp.createFiles({ 'tools/vitest/setup.mts': '', 'tools/vitest/tsconfig.json': '{"compilerOptions":{}}' });

            const nodes = await createNodesFunction(['libs/lib1/vitest.config.ts'], { outputPath: 'cached-test' }, context);

            expect(nodes[0]?.[1].projects?.['libs/lib1']?.targets?.['test']?.inputs).toContainEqual('{workspaceRoot}/tools/vitest/setup.mts');
            expect(nodes[0]?.[1].projects?.['libs/lib1']?.targets?.['test']?.inputs).toContainEqual({ json: '{workspaceRoot}/tools/vitest/tsconfig.json', fields: ['compilerOptions'] });
        });
    });
});
