import { execFileSync } from 'node:child_process';
import { readdirSync } from 'node:fs';
import { join, relative } from 'node:path';
import type { CreateNodesContext } from '@nx/devkit';
import { createNodes } from '@rasm/nx-uv';
import { TempFs } from '@rasm/testing/temp-fs';
import { minimatch } from 'minimatch';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const lock = '{workspaceRoot}/uv.lock';
const uv = { runtime: 'uv self version --short' };
const technologies = ['python'];
const release = { version: { versionActions: '@rasm/nx-uv/release/version-actions', versionActionsOptions: { lock: 'uv.lock' } } };
const locking = ['{workspaceRoot}/pyproject.toml', '{workspaceRoot}/proj/application/pyproject.toml', '{workspaceRoot}/proj/library/pyproject.toml', lock, '{projectRoot}/uv.toml', '{projectRoot}/.python-version', { externalDependencies: [] }];
const pytest = ['{projectRoot}/**/*', { runtime: 'node -p process.platform+process.arch' }, { runtime: 'uv python find --show-version --directory .' }, '^default', '{workspaceRoot}/.python-version', lock, { externalDependencies: [] }];

// --- [OPERATIONS] ----------------------------------------------------------------------

const packed = (name: string) => ['{projectRoot}/pyproject.toml', `{projectRoot}/src/${name}/**`, '!{projectRoot}/**/__pycache__', '!{projectRoot}/**/__pycache__/**', '!{projectRoot}/**/*.pyc', '!{projectRoot}/**/*.pyc/**', '!{projectRoot}/**/*.pyo', '!{projectRoot}/**/*.pyo/**', '{workspaceRoot}/pyproject.toml'];
const released = (name: string, sources: readonly string[]) => ({
    build: { dependsOn: [{ target: 'build--*', params: 'forward' }], inputs: ['...', ...sources, uv, { externalDependencies: [] }] },
    'build--uv': {
        command: `uv build --package ${name} --no-sources`,
        options: { cwd: '.' },
        cache: true,
        inputs: [...sources, uv, { externalDependencies: [] }],
        outputs: [`{workspaceRoot}/dist/${name}-0.1.0.tar.gz`, `{workspaceRoot}/dist/${name}-0.1.0-*.whl`, '{workspaceRoot}/dist/.gitignore'],
        metadata: { technologies, description: 'Builds the member sdist and wheel with uv build', help: { command: 'uv build --help', example: { args: ['--wheel'] } } },
    },
    clean: { dependsOn: [{ target: 'clean--*', params: 'forward' }], inputs: ['...', 'default', '^default', lock, { externalDependencies: [] }] },
    'clean--uv': {
        executor: '@rasm/nx-distribution:clean',
        options: { targets: ['build--uv'] },
        inputs: ['default', '^default', lock, { externalDependencies: [] }],
        metadata: { technologies, description: 'Removes the outputs build--uv declare', help: { command: 'nx run --help', example: { options: { targets: ['build--uv'] } } } },
    },
    attach: { dependsOn: [{ target: 'attach--*', params: 'forward' }], inputs: ['...', { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: [] }] },
    'attach--gh': {
        executor: '@rasm/nx-distribution:attach',
        dependsOn: ['build--uv'],
        inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
        metadata: {
            technologies,
            description: "Uploads the files build--uv wrote to the GitHub release of the version the project's manifest states with gh release upload",
            help: { command: 'gh release upload --help', example: { options: { dryRun: true } } },
        },
    },
    'nx-release-publish': {
        executor: '@rasm/nx-distribution:publish',
        options: { publish: ['uv', 'publish'], preview: ['--dry-run'], credential: 'UV_PUBLISH_TOKEN' },
        dependsOn: ['build--uv', '^nx-release-publish'],
        inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
        metadata: { description: 'Publishes the distributions build--uv wrote with uv publish, which reads UV_PUBLISH_TOKEN', help: { command: 'uv publish --help', example: { options: { dryRun: true } } } },
    },
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const previous = process.cwd();
        const tempFs = new TempFs('uv-plugin');
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
    }));

describe('@rasm/nx-uv', { timeout: 120_000 }, () => {
    const [, createNodesFunction] = createNodes;

    it('should create nodes', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'package.json': JSON.stringify({ name: 'repo' }),
            'pyproject.toml': '[project]\nname = "uv-tutorial"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = []\n\n[dependency-groups]\ndev = ["pytest>=8"]\n\n[tool.uv.workspace]\nmembers = ["proj/*"]\n',
            'proj/library/pyproject.toml': '[project]\nname = "library"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = []\n\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n',
            'proj/library/src/library/__init__.py': 'def one() -> int:\n    return 1\n',
            'proj/library/tests/test_library.py': 'from library import one\n\n\ndef test_one() -> None:\n    assert one() == 1\n',
            'proj/application/pyproject.toml':
                '[project]\nname = "application"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["library==0.1.0"]\n\n[project.scripts]\napplication = "application:main"\n\n[tool.uv.sources]\nlibrary = { workspace = true }\n\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n',
            'proj/application/src/application/__init__.py': 'from library import one\n\n\ndef main() -> None:\n    print(one())\n',
        });
        execFileSync('uv', ['lock', '--quiet'], { cwd: tempFs.tempDir });

        const nodes = await createNodesFunction(['uv.lock', 'pyproject.toml', 'proj/library/pyproject.toml', 'proj/application/pyproject.toml'], undefined, context);

        expect(nodes).toStrictEqual([
            [
                'uv.lock',
                {
                    projects: {
                        '.': {
                            root: '.',
                            name: 'uv-tutorial',
                            metadata: { technologies },
                            targets: {
                                check: { dependsOn: [{ target: 'check--*', params: 'forward' }], inputs: ['...', ...locking, uv, { env: 'UV_PREVIEW_FEATURES' }] },
                                'check--uv': {
                                    command: 'uv lock --check',
                                    options: { cwd: '.' },
                                    cache: true,
                                    inputs: [...locking, uv, { env: 'UV_PREVIEW_FEATURES' }],
                                    metadata: { technologies, description: 'Checks that uv.lock holds every change the manifests state with uv lock --check, writing nothing', help: { command: 'uv lock --help', example: { args: ['--offline'] } } },
                                },
                                upgrade: { dependsOn: [{ target: 'upgrade--*', params: 'forward' }], inputs: ['...', ...locking] },
                                'upgrade--uv': {
                                    executor: '@rasm/nx-distribution:upgrade',
                                    options: { cwd: '.', commands: [{ apply: ['uv', 'lock', '--upgrade'], preview: ['uv', 'lock', '--upgrade', '--dry-run'] }] },
                                    inputs: locking,
                                    metadata: { technologies, description: 'Moves every locked package to its newest allowed release with uv lock --upgrade', help: { command: 'uv lock --help', example: { options: { dryRun: true } } } },
                                },
                            },
                        },
                        'proj/library': {
                            root: 'proj/library',
                            name: 'library',
                            metadata: { technologies },
                            tags: ['release'],
                            release,
                            targets: {
                                ...released('library', packed('library')),
                                test: { dependsOn: [{ target: 'test--*', params: 'forward' }], inputs: ['...', ...pytest] },
                                'test--pytest': {
                                    command: 'uv run --no-active --locked --isolated --package library --all-extras --group dev pytest proj/library',
                                    options: { cwd: '.' },
                                    cache: true,
                                    inputs: pytest,
                                    metadata: { technologies, description: 'Runs the member tests with pytest', help: { command: 'uv run pytest --help', example: { args: ['-k', '<expression>'] } } },
                                },
                            },
                        },
                        'proj/application': {
                            root: 'proj/application',
                            name: 'application',
                            metadata: { technologies },
                            tags: ['release'],
                            release,
                            targets: {
                                ...released('application', packed('application')),
                                install: { dependsOn: [{ target: 'install--*', params: 'forward' }], inputs: ['...', ...packed('application'), lock, { externalDependencies: [] }] },
                                'install--uv': {
                                    command: "uv export --locked --package application --no-default-groups --no-emit-local --no-hashes --no-header --no-annotate --quiet | uv tool install --constraints - --reinstall-package application --reinstall-package library --python '>=3.13' .",
                                    options: { cwd: 'proj/application' },
                                    inputs: [...packed('application'), lock, { externalDependencies: [] }],
                                    metadata: {
                                        technologies,
                                        description: "Installs the member scripts from the checkout into uv's tool folder at the versions uv.lock pins, on an interpreter its requires-python accepts, with uv export piped to uv tool install --constraints",
                                        help: { command: 'uv tool install --help', example: { args: ['--editable'] } },
                                    },
                                },
                            },
                        },
                    },
                },
            ],
        ]);
    });
    it('should declare as outputs every file build--uv writes, each of them written', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'pyproject.toml': '[tool.uv.workspace]\nmembers = ["proj/*"]\n',
            'proj/library/pyproject.toml': '[project]\nname = "library"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = []\n\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n',
            'proj/library/src/library/__init__.py': 'def one() -> int:\n    return 1\n',
        });
        execFileSync('uv', ['lock', '--offline', '--quiet'], { cwd: tempFs.tempDir });
        const nodes = await createNodesFunction(['uv.lock'], undefined, context);
        const target = nodes[0]?.[1].projects?.['proj/library']?.targets?.['build--uv'];
        const files = () =>
            new Set(
                readdirSync(tempFs.tempDir, { recursive: true, withFileTypes: true })
                    .filter((entry) => entry.isFile())
                    .map((entry) => relative(tempFs.tempDir, join(entry.parentPath, entry.name))),
            );
        const before = files();

        expect(target).toMatchObject({ command: 'uv build --package library --no-sources', options: { cwd: '.' } });
        execFileSync('uv', ['build', '--package', 'library', '--no-sources', '--quiet'], { cwd: tempFs.tempDir });

        const written = files()
            .difference(before)
            .values()
            .filter((file) => !file.startsWith('tmp/'))
            .toArray();
        const outputs = target?.outputs?.map((output) => output.replace('{workspaceRoot}/', ''));
        const declared = new Set(outputs?.flatMap((output) => minimatch.match(written, output)));
        expect(written.filter((file) => !declared.has(file))).toStrictEqual([]);
        expect(outputs?.map((output) => minimatch.match(written, output).length)).toStrictEqual([1, 1, 1]);
    });
});
