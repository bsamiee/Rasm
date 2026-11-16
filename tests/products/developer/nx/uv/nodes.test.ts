import { execFileSync } from 'node:child_process';
import { join } from 'node:path';
import type { CreateNodesContext } from '@nx/devkit';
import { createNodes } from '@rasm/nx-uv';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const previous = process.cwd();
        const tempFs = new TempFs('uv-nodes');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            tempFs.cleanup();
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({
        nxJsonConfiguration: {
            namedInputs: {
                default: ['{projectRoot}/**/*'],
                production: ['!{projectRoot}/**/*.spec.ts'],
            },
        },
        workspaceRoot: tempFs.tempDir,
    }))
    .extend('locked', ({ tempFs }) => async (folder: string, dev: readonly string[], files: Readonly<Record<string, string>> = {}) => {
        await tempFs.createFiles({
            [join(folder, 'pyproject.toml')]: `[project]\nname = "uv-tutorial"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = []\n\n[dependency-groups]\ndev = ${JSON.stringify(dev)}\n\n[tool.uv.workspace]\nmembers = ["proj/*"]\n`,
            [join(folder, 'proj/library/pyproject.toml')]: '[project]\nname = "library"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = []\n\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n',
            [join(folder, 'proj/library/src/library/__init__.py')]: 'def one() -> int:\n    return 1\n',
            [join(folder, 'proj/library/tests/test_library.py')]: 'from library import one\n\n\ndef test_one() -> None:\n    assert one() == 1\n',
            ...files,
        });
        execFileSync('uv', ['lock', '--quiet'], { cwd: join(tempFs.tempDir, folder) });
    });

describe('@rasm/nx-uv/plugin/nodes', { timeout: 120_000 }, () => {
    const [, createNodesFunction] = createNodes;

    it('should create nodes based on uv', async ({ locked, context }) => {
        await locked('.', ['pytest>=8', 'pytest-cov']);

        const results = await createNodesFunction(['uv.lock'], { outputPath: '.artifacts/uv/{projectName}', coveragePath: '.artifacts/coverage/{projectName}' }, context);

        const command = 'uv run --no-active --locked --isolated --package library --all-extras --group dev pytest proj/library';
        expect(results).toMatchObject([
            [
                'uv.lock',
                {
                    projects: {
                        'proj/library': {
                            targets: {
                                'build--uv': {
                                    command: 'uv build --package library --no-sources --out-dir .artifacts/uv/library --clear --no-create-gitignore',
                                    options: { cwd: '.' },
                                    outputs: ['{workspaceRoot}/.artifacts/uv/library'],
                                },
                                'test--pytest': {
                                    command,
                                    options: { cwd: '.' },
                                    outputs: ['{options.outputs.0}'],
                                    configurations: {
                                        coverage: {
                                            command: `${command} --cov '--cov-report=lcov:.artifacts/coverage/{projectName}/coverage.lcov'`,
                                            env: { COVERAGE_FILE: '.artifacts/coverage/{projectName}/.coverage' },
                                            outputs: ['{workspaceRoot}/.artifacts/coverage/{projectName}/{coverage.lcov,.coverage}'],
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            ],
        ]);
    });

    it('should create nodes based on uv for nested project root', async ({ locked, context }) => {
        await locked('nested/nested/proj', ['pytest>=8']);

        const results = await createNodesFunction(['nested/nested/proj/uv.lock'], undefined, context);

        expect(results).toMatchObject([
            [
                'nested/nested/proj/uv.lock',
                {
                    projects: {
                        'nested/nested/proj': {
                            name: 'uv-tutorial',
                            targets: { 'check--uv': { command: 'uv lock --check', options: { cwd: 'nested/nested/proj' } } },
                        },
                        'nested/nested/proj/proj/library': {
                            name: 'library',
                            release: { version: { versionActionsOptions: { lock: 'nested/nested/proj/uv.lock' } } },
                            targets: {
                                'build--uv': {
                                    command: 'uv build --package library --no-sources',
                                    options: { cwd: 'nested/nested/proj' },
                                    outputs: ['{workspaceRoot}/nested/nested/proj/dist/library-0.1.0.tar.gz', '{workspaceRoot}/nested/nested/proj/dist/library-0.1.0-*.whl', '{workspaceRoot}/nested/nested/proj/dist/.gitignore'],
                                },
                                'test--pytest': {
                                    command: 'uv run --no-active --locked --isolated --package library --all-extras --group dev pytest proj/library',
                                    options: { cwd: 'nested/nested/proj' },
                                    inputs: [
                                        '{projectRoot}/**/*',
                                        { runtime: 'node -p process.platform+process.arch' },
                                        { runtime: 'uv python find --show-version --directory nested/nested/proj' },
                                        '^default',
                                        '{workspaceRoot}/nested/nested/proj/.python-version',
                                        '{workspaceRoot}/nested/nested/proj/uv.lock',
                                        { externalDependencies: [] },
                                    ],
                                },
                            },
                        },
                    },
                },
            ],
        ]);
    });

    it('should create check targets for each checker the environment of the member locks', async ({ locked, context }) => {
        await locked('.', ['pytest>=8', 'ty'], { 'ty.toml': '[rules]\nunresolved-import = "error"\n' });

        const results = await createNodesFunction(['uv.lock'], undefined, context);

        expect(results).toMatchObject([
            [
                'uv.lock',
                {
                    projects: {
                        'proj/library': {
                            targets: {
                                check: { dependsOn: ['check--*'] },
                                'check--ty': {
                                    command: 'uv run --no-active --locked --package library --all-extras --group dev ty check proj/library',
                                    options: { cwd: '.' },
                                    cache: true,
                                    inputs: ['{projectRoot}/**/*.py', '{projectRoot}/**/*.pyi', '{workspaceRoot}/ty.toml', { env: 'TY_UV' }, { runtime: 'node -p process.platform+process.arch' }, '^default', '{workspaceRoot}/.python-version', '{workspaceRoot}/uv.lock', { externalDependencies: [] }],
                                },
                            },
                        },
                    },
                },
            ],
        ]);
    });

    it('should hash the ty configuration ty finds from the workspace it runs in, not a table of the member', async ({ tempFs, locked, context }) => {
        await locked('.', ['pytest>=8', 'ty'], { 'ty.toml': '[rules]\nunresolved-import = "error"\n' });
        await tempFs.createFile('proj/library/pyproject.toml', `${await tempFs.readFile('proj/library/pyproject.toml')}\n[tool.ty.rules]\nunresolved-import = "ignore"\n`);

        const results = await createNodesFunction(['uv.lock'], undefined, context);

        const inputs = results[0]?.[1].projects?.['proj/library']?.targets?.['check--ty']?.inputs;
        expect(inputs).toContain('{workspaceRoot}/ty.toml');
        expect(inputs).not.toContain('{workspaceRoot}/proj/library/pyproject.toml');
    });

    it('should not create check targets for a checker the environment of the member does not lock', async ({ locked, context }) => {
        await locked('.', ['pytest>=8'], { 'ty.toml': '[rules]\nunresolved-import = "error"\n' });

        const results = await createNodesFunction(['uv.lock'], undefined, context);

        expect(results[0]?.[1].projects?.['proj/library']?.targets).not.toHaveProperty('check--ty');
        expect(results[0]?.[1].projects?.['proj/library']?.targets?.['test--pytest']).not.toHaveProperty('configurations');
    });
});
