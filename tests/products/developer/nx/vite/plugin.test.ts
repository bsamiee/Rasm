import { execSync } from 'node:child_process';
import { existsSync, globSync, symlinkSync } from 'node:fs';
import { delimiter, join } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath } from 'node:url';
import { getReadyCheckReadyWhen } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-vite';
import { TempFs } from '@rasm/testing/temp-fs';
import { resolveConfig } from 'vite';
import { describe, expect, test, vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const check = fileURLToPath(import.meta.resolve('@nx/vite/ready-check'));
const applicationTargets = ['serve', 'serve--vite', 'preview', 'preview--vite'];

// --- [COMPOSITION] ---------------------------------------------------------------------

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));
vi.mock(import('vite'), async (importOriginal) => {
    const vite = await importOriginal();
    return { ...vite, resolveConfig: vi.fn<typeof vite.resolveConfig>(vite.resolveConfig) };
});

const [, createNodesFunction] = createNodes;
const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const cwd = process.cwd();
        const tempFs = new TempFs('vite-plugin-tests');
        process.chdir(tempFs.tempDir);
        symlinkSync(join(import.meta.dirname, 'node_modules'), join(tempFs.tempDir, 'node_modules'), 'dir');
        onCleanup(() => {
            tempFs.cleanup();
            process.chdir(cwd);
        });
        return tempFs;
    })
    .extend('inferProject', ({ tempFs }) => async (root: string, config: string, extraFiles: readonly string[] = [], options: Parameters<typeof createNodesFunction>[1] = {}) => {
        vi.mocked(resolveConfig).mockClear();
        tempFs.createFilesSync({
            'package-lock.json': '{}',
            [join(root, 'package.json')]: '{}',
            'package.json': JSON.stringify({ name: 'repo', workspaces: ['*'], nx: {} }),
            [join(root, 'vite.config.ts')]: config,
            ...Object.fromEntries(extraFiles.map((file) => [file, ''])),
        });

        const nodes = await createNodesFunction([join(root, 'vite.config.ts')], options, {
            nxJsonConfiguration: { namedInputs: { default: ['{projectRoot}/**/*'], production: ['!{projectRoot}/**/*.spec.ts'] } },
            workspaceRoot: tempFs.tempDir,
        });

        const { targets = {}, projectType } = nodes[0]?.[1].projects?.[root] ?? {};
        return {
            nodes,
            targets,
            projectType,
            inferredApplicationTargets: applicationTargets.filter((target) => target in targets),
            resolvedCommands: vi.mocked(resolveConfig).mock.calls.map(([, command]) => command),
        };
    });

describe('@rasm/nx-vite', () => {
    describe('root project', () => {
        it('should create nodes', async ({ inferProject }) => {
            const { nodes } = await inferProject('.', 'export default {};', ['index.html']);

            const needs = {
                options: { cwd: '.' },
                inputs: ['production', '^production', { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: ['vite'] }],
            };
            expect(nodes).toStrictEqual([
                [
                    'vite.config.ts',
                    {
                        projects: {
                            '.': {
                                root: '.',
                                projectType: 'application',
                                metadata: { technologies: ['vite'] },
                                targets: {
                                    build: { dependsOn: [{ target: 'build--*', params: 'forward' }], inputs: ['...', ...needs.inputs] },
                                    'build--vite': {
                                        ...needs,
                                        command: 'vite build',
                                        dependsOn: ['codegen--*', '^codegen--*', '^build--*'],
                                        cache: true,
                                        outputs: ['{projectRoot}/dist'],
                                        metadata: { technologies: ['vite'], description: 'Builds every environment of the Vite config with vite build', help: { command: 'npx vite build --help', example: { args: ['--mode', 'production'] } } },
                                    },
                                    clean: { dependsOn: [{ target: 'clean--*', params: 'forward' }], inputs: ['...', 'default', '^default', { externalDependencies: [] }] },
                                    'clean--vite': {
                                        executor: '@rasm/nx-distribution:clean',
                                        options: { targets: ['build--vite'] },
                                        inputs: ['default', '^default', { externalDependencies: [] }],
                                        metadata: { technologies: ['vite'], description: 'Removes the outputs build--vite declare', help: { command: 'npx nx run --help', example: { options: { targets: ['build--vite'] } } } },
                                    },
                                    serve: { dependsOn: [{ target: 'serve--*', params: 'forward' }], inputs: ['...', ...needs.inputs] },
                                    'serve--vite': {
                                        ...needs,
                                        command: 'vite',
                                        dependsOn: ['codegen--*', '^codegen--*'],
                                        continuous: true,
                                        readyWhen: getReadyCheckReadyWhen(check, 'vite.config.ts', 'serve'),
                                        metadata: { technologies: ['vite'], description: 'Serves every environment of the Vite config with vite', help: { command: 'npx vite --help', example: { args: ['--port', '<port>'] } } },
                                    },
                                    preview: { dependsOn: [{ target: 'preview--*', params: 'forward' }], inputs: ['...', ...needs.inputs] },
                                    'preview--vite': {
                                        ...needs,
                                        command: 'vite preview',
                                        dependsOn: ['build--vite'],
                                        continuous: true,
                                        readyWhen: getReadyCheckReadyWhen(check, 'vite.config.ts', 'preview'),
                                        metadata: { technologies: ['vite'], description: 'Serves the build output of the Vite config with vite preview', help: { command: 'npx vite preview --help', example: { args: ['--port', '<port>'] } } },
                                    },
                                },
                            },
                        },
                    },
                ],
            ]);
        });

        it('should create nodes when rollupOptions contains input', async ({ inferProject }) => {
            const { targets } = await inferProject('.', "export default { build: { rollupOptions: { input: { index: 'src/index.js' } } } };", ['index.html']);

            expect(targets['build--vite']?.command).toMatch(/vite/u);
            expect(targets['serve--vite']?.command).toMatch(/vite/u);
        });

        it('should not create serve and preview targets for a bundle-only config with no index.html and no server', async ({ inferProject }) => {
            const { targets } = await inferProject('.', "export default { build: { rollupOptions: { input: { index: 'src/index.js' } } } };");

            expect(targets['build--vite']?.command).toBe('vite build');
            expect(Object.keys(targets)).toStrictEqual(['build', 'build--vite', 'clean', 'clean--vite']);
        });

        it('waits for the signal when the config loads nxReadySignal()', async ({ inferProject }) => {
            const { targets } = await inferProject('.', "import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';\n\nexport default { plugins: [nxReadySignal()] };", ['index.html']);

            expect(targets['serve--vite']?.readyWhen).toStrictEqual({ signal: true });
            expect(targets['preview--vite']?.readyWhen).toStrictEqual({ signal: true });
        });
    });

    describe('not root project', () => {
        it('should create nodes', async ({ inferProject }) => {
            const { nodes } = await inferProject('my-app', "import '../tools/vite/base.ts';\n\nexport default {};", ['my-app/index.html', 'tools/vite/base.ts'], { outputPath: '.artifacts/vite/{projectName}' });

            const needs = {
                options: { cwd: 'my-app' },
                inputs: ['production', '^production', '{workspaceRoot}/tools/vite/base.ts', { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: ['vite'] }],
            };
            expect(nodes).toStrictEqual([
                [
                    'my-app/vite.config.ts',
                    {
                        projects: {
                            'my-app': {
                                root: 'my-app',
                                projectType: 'application',
                                metadata: { technologies: ['vite'] },
                                targets: {
                                    build: { dependsOn: [{ target: 'build--*', params: 'forward' }], inputs: ['...', ...needs.inputs] },
                                    'build--vite': {
                                        ...needs,
                                        command: "vite build --outDir '../.artifacts/vite/{projectName}' --emptyOutDir",
                                        dependsOn: ['codegen--*', '^codegen--*', '^build--*'],
                                        cache: true,
                                        outputs: ['{workspaceRoot}/.artifacts/vite/{projectName}'],
                                        metadata: { technologies: ['vite'], description: 'Builds every environment of the Vite config with vite build', help: { command: 'npx vite build --help', example: { args: ['--mode', 'production'] } } },
                                    },
                                    clean: { dependsOn: [{ target: 'clean--*', params: 'forward' }], inputs: ['...', 'default', '^default', { externalDependencies: [] }] },
                                    'clean--vite': {
                                        executor: '@rasm/nx-distribution:clean',
                                        options: { targets: ['build--vite'] },
                                        inputs: ['default', '^default', { externalDependencies: [] }],
                                        metadata: { technologies: ['vite'], description: 'Removes the outputs build--vite declare', help: { command: 'npx nx run --help', example: { options: { targets: ['build--vite'] } } } },
                                    },
                                    serve: { dependsOn: [{ target: 'serve--*', params: 'forward' }], inputs: ['...', ...needs.inputs] },
                                    'serve--vite': {
                                        ...needs,
                                        command: 'vite',
                                        dependsOn: ['codegen--*', '^codegen--*'],
                                        continuous: true,
                                        readyWhen: getReadyCheckReadyWhen(check, 'vite.config.ts', 'serve'),
                                        metadata: { technologies: ['vite'], description: 'Serves every environment of the Vite config with vite', help: { command: 'npx vite --help', example: { args: ['--port', '<port>'] } } },
                                    },
                                    preview: { dependsOn: [{ target: 'preview--*', params: 'forward' }], inputs: ['...', ...needs.inputs] },
                                    'preview--vite': {
                                        ...needs,
                                        command: "vite preview --outDir '../.artifacts/vite/{projectName}'",
                                        dependsOn: ['build--vite'],
                                        continuous: true,
                                        readyWhen: getReadyCheckReadyWhen(check, 'vite.config.ts', 'preview'),
                                        metadata: { technologies: ['vite'], description: 'Serves the build output of the Vite config with vite preview', help: { command: 'npx vite preview --help', example: { args: ['--port', '<port>'] } } },
                                    },
                                },
                            },
                        },
                    },
                ],
            ]);
        });
    });

    describe('library mode', () => {
        const lib = "{ entry: 'index.ts', name: 'my-lib' }";
        const needs = { options: { cwd: 'my-lib' }, inputs: ['production', '^production', { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: ['vite'] }] };

        it('should exclude serve and preview targets when vite.config.ts is in library mode', async ({ inferProject }) => {
            const { targets, projectType } = await inferProject('my-lib', `export default { build: { lib: ${lib} } };`);

            expect(projectType).toBe('library');
            expect(Object.keys(targets)).toStrictEqual(['build', 'build--vite', 'clean', 'clean--vite']);
            expect(targets['build--vite']).toStrictEqual({
                ...needs,
                command: 'vite build',
                dependsOn: ['codegen--*', '^codegen--*', '^build--*'],
                cache: true,
                outputs: ['{projectRoot}/dist'],
                metadata: { technologies: ['vite'], description: 'Builds every environment of the Vite config with vite build', help: { command: 'npx vite build --help', example: { args: ['--mode', 'production'] } } },
            });
        });

        it('should not exclude serve and preview targets when vite.config.ts is in library mode when user has defined a server config', async ({ inferProject }) => {
            const { targets, projectType } = await inferProject('my-lib', `export default { build: { lib: ${lib} }, server: { port: 3000, host: 'localhost' } };`);

            expect(projectType).toBe('library');
            expect(Object.keys(targets)).toStrictEqual(['build', 'build--vite', 'clean', 'clean--vite', 'serve', 'serve--vite', 'preview', 'preview--vite']);
            expect(targets['serve--vite']).toStrictEqual({
                ...needs,
                command: 'vite',
                dependsOn: ['codegen--*', '^codegen--*'],
                continuous: true,
                readyWhen: getReadyCheckReadyWhen(check, 'vite.config.ts', 'serve'),
                metadata: { technologies: ['vite'], description: 'Serves every environment of the Vite config with vite', help: { command: 'npx vite --help', example: { args: ['--port', '<port>'] } } },
            });
            expect(targets['preview--vite']).toStrictEqual({
                ...needs,
                command: 'vite preview',
                dependsOn: ['build--vite'],
                continuous: true,
                readyWhen: getReadyCheckReadyWhen(check, 'vite.config.ts', 'preview'),
                metadata: { technologies: ['vite'], description: 'Serves the build output of the Vite config with vite preview', help: { command: 'npx vite preview --help', example: { args: ['--port', '<port>'] } } },
            });
        });

        it('should keep the application targets when only the serve config has a server, with build outputs from the build config', async ({ inferProject }) => {
            const { targets, projectType, inferredApplicationTargets, resolvedCommands } = await inferProject('my-lib', `export default ({ command }) => ({ build: { lib: ${lib}, outDir: command === 'build' ? 'dist-build' : 'dist-serve' }, server: command === 'serve' ? { port: 4200 } : undefined });`);

            expect(inferredApplicationTargets).toStrictEqual(applicationTargets);
            expect(targets['build--vite']?.outputs).toStrictEqual(['{projectRoot}/dist-build']);
            expect(projectType).toBe('library');
            expect(resolvedCommands).toStrictEqual(['build', 'serve']);
        });

        it.for([
            ['in both modes', '', ['build']],
            ['only in serve mode', ", apply: 'serve'", ['build', 'serve']],
        ] as const)('should keep the application targets when a plugin config hook sets the server %s', async ([, pluginOptions, expectedCommands], { inferProject }) => {
            const { inferredApplicationTargets, resolvedCommands } = await inferProject('my-lib', `export default { build: { lib: ${lib} }, plugins: [{ name: 'user-plugin', config: () => ({ server: { port: 4200 } })${pluginOptions} }] };`);

            expect(inferredApplicationTargets).toStrictEqual(applicationTargets);
            expect(resolvedCommands).toStrictEqual(expectedCommands);
        });

        it('should keep the build target and omit the application targets when the config throws in serve mode', async ({ inferProject }) => {
            const { targets, inferredApplicationTargets } = await inferProject('my-lib', `export default ({ command }) => {\n    if (command === 'serve') {\n        throw new Error('This config only builds');\n    }\n    return { build: { lib: ${lib} } };\n};`);

            expect(targets['build--vite']?.command).toBe('vite build');
            expect(inferredApplicationTargets).toStrictEqual([]);
            expect(resolveConfig).toHaveBeenLastCalledWith(expect.objectContaining({ logLevel: 'silent' }), 'serve', 'development', 'development', false, undefined, expect.any(Function));
        });

        it('should keep the application targets for an explicit empty server block', async ({ inferProject }) => {
            const { inferredApplicationTargets, resolvedCommands } = await inferProject('my-lib', `export default { build: { lib: ${lib} }, server: {} };`);

            expect(inferredApplicationTargets).toStrictEqual(applicationTargets);
            expect(resolvedCommands).toStrictEqual(['build']);
        });

        it('should exclude the application targets when neither mode has a server', async ({ inferProject }) => {
            const { inferredApplicationTargets, resolvedCommands } = await inferProject('my-lib', `export default { build: { lib: ${lib} } };`);

            expect(inferredApplicationTargets).toStrictEqual([]);
            expect(resolvedCommands).toStrictEqual(['build', 'serve']);
        });

        it.for([
            ['an application', 'export default { build: {} };', ['my-lib/index.html']],
            ['a library with a build mode server', `export default { build: { lib: ${lib} }, server: { port: 3000 } };`, []],
        ] as const)('should resolve the config once for %s', async ([, config, extraFiles], { inferProject }) => {
            const { inferredApplicationTargets, resolvedCommands } = await inferProject('my-lib', config, extraFiles);

            expect(inferredApplicationTargets).toStrictEqual(applicationTargets);
            expect(resolvedCommands).toStrictEqual(['build']);
            expect(vi.mocked(resolveConfig).mock.calls[0]?.[0].logLevel).toBeUndefined();
        });
    });

    it('should add the tsconfig extends chain outside the project root, leaving the root tsconfig to the native hasher', async ({ tempFs, inferProject }) => {
        tempFs.createFilesSync({
            'tsconfig.base.json': JSON.stringify({}),
            'tsconfig.shared.json': JSON.stringify({ extends: './tsconfig.base.json' }),
            'my-lib/tsconfig.json': JSON.stringify({ extends: '../tsconfig.shared.json' }),
        });
        const { targets } = await inferProject('my-lib', "export default { build: { lib: { entry: 'index.ts', name: 'my-lib' } } };");

        expect(targets['build--vite']?.inputs).toContainEqual({ json: '{workspaceRoot}/tsconfig.shared.json', fields: ['compilerOptions'] });
        expect(targets['build--vite']?.inputs).not.toContainEqual(expect.objectContaining({ json: '{workspaceRoot}/tsconfig.base.json' }));
    });

    it('should read the outDir a plugin sets in configEnvironment while running no configResolved hook, where a build plugin removes its last output', async ({ tempFs, inferProject }) => {
        tempFs.createFilesSync({ 'my-app/.output/kept.json': '{}' });
        const plugin = "{ name: 'output-plugin', configEnvironment: () => ({ build: { outDir: '.output/client' } }), configResolved: ({ command, root }) => command === 'build' && rmSync(join(root, '.output'), { recursive: true }) }";
        const { targets } = await inferProject('my-app', `import { rmSync } from 'node:fs';\nimport { join } from 'node:path';\nexport default { plugins: [${plugin}] };`, ['my-app/index.html']);

        expect(targets['build--vite']?.outputs).toContain('{projectRoot}/.output/client');
        expect(existsSync(join(tempFs.tempDir, 'my-app/.output/kept.json'))).toBe(true);
    });

    it('should create nodes only where a package.json is in the package manager workspaces or beside a project.json', async ({ tempFs }) => {
        const config = 'export default { build: { lib: { entry: "index.ts" } } };';
        tempFs.createFilesSync({
            'package.json': JSON.stringify({ name: 'repo', workspaces: ['apps/*'] }),
            'package-lock.json': '{}',
            'apps/listed/package.json': '{}',
            'apps/listed/vite.config.ts': config,
            'tools/marked/project.json': '{}',
            'tools/marked/vite.config.ts': config,
            'tools/unlisted/package.json': '{}',
            'tools/unlisted/vite.config.ts': config,
        });

        const nodes = await createNodesFunction(['apps/listed/vite.config.ts', 'tools/marked/vite.config.ts', 'tools/unlisted/vite.config.ts'], {}, { nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir });

        expect(nodes.map(([file]) => file)).toStrictEqual(['apps/listed/vite.config.ts', 'tools/marked/vite.config.ts']);
    });

    it('writes every declared output and no file outside them', async ({ tempFs, inferProject }) => {
        tempFs.createFileSync('my-lib/index.ts', 'export const value = 1;\n');
        const { targets } = await inferProject('my-lib', "export default { build: { lib: { entry: 'index.ts', formats: ['es'] } } };");
        const listed = () => globSync('**', { cwd: tempFs.tempDir, exclude: ['**/node_modules', '.nx', 'tmp'] });
        const before = new Set(listed());

        expect(targets['build--vite']).toMatchObject({ command: 'vite build', options: { cwd: 'my-lib' }, outputs: ['{projectRoot}/dist'] });
        execSync('vite build', { cwd: join(tempFs.tempDir, 'my-lib'), env: { ...env, PATH: [join(tempFs.tempDir, 'node_modules', '.bin'), env['PATH']].join(delimiter) }, stdio: 'pipe' });

        const written = listed().filter((file) => !before.has(file));
        expect(written).not.toHaveLength(0);
        expect(written.filter((file) => !/^my-lib\/dist(?:\/|$)/u.test(file))).toStrictEqual([]);
        expect(existsSync(join(tempFs.tempDir, 'my-lib', 'dist'))).toBe(true);
    });
});
