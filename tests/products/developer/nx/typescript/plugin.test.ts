import { execFileSync } from 'node:child_process';
import { readdirSync, symlinkSync } from 'node:fs';
import { dirname, join, matchesGlob } from 'node:path';
import { fileURLToPath } from 'node:url';
import { type CreateNodesContext, detectPackageManager } from '@nx/devkit';
import { getLockFileName, setupWorkspaceContext, workspaceDataDirectory } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-typescript';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const COMPILER = dirname(fileURLToPath(import.meta.resolve('@typescript/native/package.json')));
const UPSTREAM = ['^production', { dependentTasksOutputFiles: '**/*.{d.ts,d.cts,d.mts,tsbuildinfo}', transitive: true }, { externalDependencies: ['typescript'] }];
const EMITTED = '**/*.{js,cjs,mjs,jsx,d.ts,d.cts,d.mts}{,.map}';

// --- [OPERATIONS] ----------------------------------------------------------------------

const applyFilesToTempFsAndContext = async (tempFs: TempFs, fileSys: Record<string, string>) => {
    await tempFs.createFiles(fileSys);
    setupWorkspaceContext(tempFs.tempDir);
    return Object.keys(fileSys).filter((file) => matchesGlob(file, createNodes[0]));
};

const invokeCreateNodesOnMatchingFiles = async (configFiles: readonly string[], context: CreateNodesContext, pluginOptions: Parameters<(typeof createNodes)[1]>[1]) => {
    const results = await createNodes[1](configFiles, pluginOptions, context);
    return { projects: Object.fromEntries(results.flatMap(([, nodes]) => Object.entries({ ...nodes.projects }))) };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const tempFs = new TempFs('typescript-plugin');
        const previous = process.cwd();
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            tempFs.cleanup();
        });
        tempFs.createDirSync('node_modules');
        symlinkSync(COMPILER, join(tempFs.tempDir, 'node_modules/typescript'), 'dir');
        await applyFilesToTempFsAndContext(tempFs, { [getLockFileName(detectPackageManager(tempFs.tempDir))]: '', 'package.json': JSON.stringify({ workspaces: ['**'] }) });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({
        nxJsonConfiguration: { namedInputs: { default: ['{projectRoot}/**/*'], production: ['!{projectRoot}/**/*.spec.ts'] } },
        workspaceRoot: tempFs.tempDir,
    }));

it('should handle missing lock file', async ({ tempFs, context }) => {
    const configFiles = await applyFilesToTempFsAndContext(tempFs, {
        'libs/my-lib/tsconfig.json': JSON.stringify({ files: [] }),
        'libs/my-lib/package.json': '{}',
    });
    tempFs.removeFileSync(getLockFileName(detectPackageManager(tempFs.tempDir)));

    await expect(invokeCreateNodesOnMatchingFiles(configFiles, context, {})).resolves.not.toThrow();
});

it('should not create check or build targets for root tsconfig.json files', async ({ tempFs, context }) => {
    const configFiles = await applyFilesToTempFsAndContext(tempFs, {
        'package.json': '{}',
        'tsconfig.json': '{}',
        'src/index.ts': "console.log('Hello World!');",
    });
    const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

    expect(Object.keys(projects)).toStrictEqual(['.']);
    expect(Object.keys({ ...projects['.']?.targets })).toStrictEqual(['upgrade', 'upgrade--npm']);
});

describe('typecheck target', () => {
    it('should create a check target for a tsconfig.json beside a project.json, and none for a package.json outside the package manager workspaces', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'package.json': JSON.stringify({ workspaces: ['libs/*'] }),
            'libs/my-lib/tsconfig.json': JSON.stringify({ files: ['index.ts'], compilerOptions: { noEmit: true } }),
            'libs/my-lib/index.ts': 'export {};',
            'libs/my-lib/project.json': '{}',
            'tools/script/tsconfig.json': JSON.stringify({ files: ['index.ts'], compilerOptions: { noEmit: true } }),
            'tools/script/index.ts': 'export {};',
            'tools/script/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['check--tsc']?.options).toStrictEqual({ commands: ['tsc'], cwd: 'libs/my-lib' });
        expect(projects['tools/script']).toBeUndefined();
    });

    it('should create a node with check and build targets for a project level tsconfig.json file beside a package.json', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ files: ['index.ts'] }),
            'libs/my-lib/index.ts': 'export const value = 1;',
            'libs/my-lib/package.json': '{}',
        });
        const inputs = ['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/index.ts', ...UPSTREAM];

        await expect(invokeCreateNodesOnMatchingFiles(configFiles, context, {})).resolves.toStrictEqual({
            projects: {
                'libs/my-lib': {
                    root: 'libs/my-lib',
                    metadata: { technologies: ['typescript'] },
                    targets: {
                        build: { dependsOn: [{ target: 'build--*', params: 'forward' }], inputs: ['...', ...inputs] },
                        'build--tsc': {
                            executor: 'nx:run-commands',
                            options: { commands: ['tsc'], cwd: 'libs/my-lib' },
                            dependsOn: ['codegen--*', '^codegen', '^build--tsc'],
                            cache: true,
                            inputs,
                            outputs: [`{projectRoot}/${EMITTED}`],
                            metadata: { technologies: ['typescript'], description: 'Emits JavaScript and declarations with tsc, type-checking every program it builds', help: { command: 'npx tsc --help', example: { args: ['--listEmittedFiles'] } } },
                        },
                        check: { dependsOn: [{ target: 'check--*', params: 'forward' }], inputs: ['...', ...inputs] },
                        'check--tsc': {
                            executor: 'nx:noop',
                            dependsOn: ['build--tsc'],
                            inputs,
                            metadata: { technologies: ['typescript'], description: 'Type-checks through the build--tsc compile with tsc', help: { command: 'npx tsc --help', example: { args: ['--extendedDiagnostics'] } } },
                        },
                        clean: { dependsOn: [{ target: 'clean--*', params: 'forward' }], inputs: ['...', 'default', '^default', { externalDependencies: ['typescript'] }] },
                        'clean--tsc': {
                            executor: '@rasm/nx-distribution:clean',
                            options: { targets: ['build--tsc'], commands: [['tsc', '--build', '--clean', 'tsconfig.json']], cwd: 'libs/my-lib' },
                            inputs: ['default', '^default', { externalDependencies: ['typescript'] }],
                            metadata: { technologies: ['typescript'], description: 'Removes the outputs build--tsc declare and each program output with tsc --build --clean', help: { command: 'npx tsc --build --help', example: { args: ['--dry'] } } },
                        },
                    },
                },
            },
        });
    });

    it('should create check and build targets with custom compiler packages', async ({ tempFs, context }) => {
        tempFs.createDirSync('node_modules/@typescript');
        symlinkSync(COMPILER, join(tempFs.tempDir, 'node_modules/@typescript/native'), 'dir');
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ files: ['index.ts'] }),
            'libs/my-lib/index.ts': 'export const value = 1;',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, { compilerPackages: ['@typescript/native', '@effect/tsgo'] });

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs?.at(-1)).toStrictEqual({ externalDependencies: ['@typescript/native', '@effect/tsgo'] });
        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.options).toStrictEqual({ commands: ['tsc'], cwd: 'libs/my-lib' });
    });

    it('should not invoke `tsc --build` when `noEmit` is set in the tsconfig.json file', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ compilerOptions: { noEmit: true }, files: ['index.ts'] }),
            'libs/my-lib/index.ts': 'export const value = 1;',
            'libs/my-lib/package.json': '{}',
        });
        const direct = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(Object.keys({ ...direct.projects['libs/my-lib']?.targets })).toStrictEqual(['check', 'check--tsc']);
        expect(direct.projects['libs/my-lib']?.targets?.['check--tsc']).toMatchObject({ executor: 'nx:run-commands', options: { commands: ['tsc'], cwd: 'libs/my-lib' }, cache: true, outputs: [] });

        const extendedFiles = await applyFilesToTempFsAndContext(tempFs, {
            'tsconfig.base.json': JSON.stringify({ compilerOptions: { noEmit: true } }),
            'libs/my-lib/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.base.json', files: ['index.ts'] }),
            'libs/my-lib/index.ts': 'export const value = 1;',
            'libs/my-lib/package.json': '{}',
        });
        const extended = await invokeCreateNodesOnMatchingFiles(extendedFiles, context, {});

        expect(Object.keys({ ...extended.projects['libs/my-lib']?.targets })).toStrictEqual(['check', 'check--tsc']);
        expect(extended.projects['libs/my-lib']?.targets?.['check--tsc']?.inputs).toStrictEqual(['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{workspaceRoot}/tsconfig.base.json', '{projectRoot}/index.ts', ...UPSTREAM]);
    });

    it('should not invoke `tsc --build` when `noEmit` is set in any of the referenced tsconfig.json files', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ files: [], references: [{ path: './tsconfig.lib.json' }] }),
            'libs/my-lib/tsconfig.lib.json': JSON.stringify({ compilerOptions: { noEmit: true }, files: ['index.ts'] }),
            'libs/my-lib/index.ts': 'export const value = 1;',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(Object.keys({ ...projects['libs/my-lib']?.targets })).toStrictEqual(['check', 'check--tsc']);
        expect(projects['libs/my-lib']?.targets?.['check--tsc']).toMatchObject({
            options: { commands: ['tsc -p tsconfig.lib.json'], cwd: 'libs/my-lib' },
            inputs: ['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/tsconfig.lib.json', '{projectRoot}/index.ts', ...UPSTREAM],
            outputs: [],
        });
    });

    it('should handle ts project references pointing to non-existing files and not throw', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ references: [{ path: '../my-lib-2' }] }),
            'libs/my-lib/package.json': '{}',
        });

        await expect(invokeCreateNodesOnMatchingFiles(configFiles, context, {})).resolves.not.toThrow();
    });
});

describe('typecheck target inputs', () => {
    it('should add the config file and the files `include` and `exclude` select', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ include: ['src/**/*.ts'], exclude: ['src/**/foo.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/src/foo.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs).toStrictEqual(['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/src/index.ts', ...UPSTREAM]);
        expect(projects['libs/my-lib']?.targets?.['check--tsc']?.inputs).toStrictEqual(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs);
    });

    it('should add the config file and the files `include` and `exclude` select using the configDir template', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ include: [`\${configDir}/src/**/*.ts`], exclude: [`\${configDir}/src/**/foo.ts`], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/src/foo.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs).toStrictEqual(['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/src/index.ts', ...UPSTREAM]);
    });

    it.for([
        [{}, ['src/a.ts', 'src/b.tsx', 'src/c.d.ts', 'src/d.cts', 'src/e.d.cts', 'src/f.mts', 'src/g.d.mts']],
        [{ allowJs: true }, ['src/a.ts', 'src/b.tsx', 'src/c.d.ts', 'src/d.cts', 'src/e.d.cts', 'src/f.mts', 'src/g.d.mts', 'src/h.js', 'src/i.jsx', 'src/j.cjs', 'src/k.mjs']],
        [{ resolveJsonModule: true }, ['src/a.ts', 'src/b.tsx', 'src/c.d.ts', 'src/d.cts', 'src/e.d.cts', 'src/f.mts', 'src/g.d.mts']],
        [{ allowJs: true, resolveJsonModule: true }, ['src/a.ts', 'src/b.tsx', 'src/c.d.ts', 'src/d.cts', 'src/e.d.cts', 'src/f.mts', 'src/g.d.mts', 'src/h.js', 'src/i.jsx', 'src/j.cjs', 'src/k.mjs']],
    ] as const)('should add the files a directory in `include` selects under compiler options %j', async ([compilerOptions, selected], { tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ include: ['src'], compilerOptions: { outDir: 'dist', ...compilerOptions } }),
            ...Object.fromEntries(['a.ts', 'b.tsx', 'c.d.ts', 'd.cts', 'e.d.cts', 'f.mts', 'g.d.mts', 'h.js', 'i.jsx', 'j.cjs', 'k.mjs'].map((file) => [`libs/my-lib/src/${file}`, 'export {};'])),
            'libs/my-lib/src/l.json': '{}',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(
            projects['libs/my-lib']?.targets?.['build--tsc']?.inputs
                ?.filter((input): input is string => typeof input === 'string')
                .filter((input) => input.startsWith('{projectRoot}/src/'))
                .toSorted(),
        ).toStrictEqual(selected.map((file) => `{projectRoot}/${file}`).toSorted());
    });

    it('should expand "." in `include` to every file of the project', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ include: ['.'], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/index.ts': 'export {};',
            'libs/my-lib/nested/deep.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs).toStrictEqual(['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/index.ts', '{projectRoot}/nested/deep.ts', ...UPSTREAM]);
    });

    it('should expand ".." in `include` to the files of the parent folder', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ include: ['..'], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/index.ts': 'export {};',
            'libs/shared/units.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs).toStrictEqual(['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/index.ts', '{workspaceRoot}/libs/shared/units.ts', ...UPSTREAM]);
    });

    it('should not expand "." in `include` when the project directory name contains a dot, matching TypeScript', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my.lib/tsconfig.json': JSON.stringify({ include: ['.'], compilerOptions: { outDir: 'dist' } }),
            'libs/my.lib/index.ts': 'export {};',
            'libs/my.lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my.lib']?.targets?.['check--tsc']).toMatchObject({ options: { commands: ['tsc'] }, inputs: ['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', ...UPSTREAM] });
    });

    it('should keep files an exclude drops when another tsconfig file of the project includes them', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ include: ['.'], references: [{ path: './tsconfig.lib.json' }] }),
            'libs/my-lib/tsconfig.lib.json': JSON.stringify({ include: ['src'], exclude: ['src/**/*.spec.ts'], compilerOptions: { noEmit: true } }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/src/index.spec.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['check--tsc']?.inputs).toStrictEqual(expect.arrayContaining(['{projectRoot}/tsconfig.json', '{projectRoot}/src/index.ts', '{projectRoot}/src/index.spec.ts']));
    });

    it('should drop the whole subtree of a directory in `exclude`', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ include: ['.'], exclude: ['tools'], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/tools/nested/script.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs).toStrictEqual(['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/src/index.ts', ...UPSTREAM]);
    });

    it('should add extended config files', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'tsconfig.base.json': JSON.stringify({ exclude: ['node_modules', 'tmp'] }),
            'tsconfig.foo.json': JSON.stringify({ extends: './tsconfig.base.json' }),
            'libs/my-lib/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.foo.json', include: ['src/**/*.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs).toStrictEqual(['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{workspaceRoot}/tsconfig.base.json', '{workspaceRoot}/tsconfig.foo.json', '{projectRoot}/src/index.ts', ...UPSTREAM]);
    });

    it('should add extended config files when there are multiple extended config files', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'tsconfig.base.json': JSON.stringify({ exclude: ['node_modules', 'tmp'] }),
            'tsconfig.foo.json': '{}',
            'tsconfig.bar.json': JSON.stringify({ extends: './tsconfig.foo.json', exclude: ['node_modules', 'dist'] }),
            'libs/my-lib/tsconfig.json': JSON.stringify({ extends: ['../../tsconfig.base.json', '../../tsconfig.bar.json'], include: ['src/**/*.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs).toStrictEqual(expect.arrayContaining(['{projectRoot}/tsconfig.json', '{workspaceRoot}/tsconfig.base.json', '{workspaceRoot}/tsconfig.bar.json', '{workspaceRoot}/tsconfig.foo.json']));
    });

    it('should add extended config files supporting node.js style resolution and local workspace packages', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'tsconfig.base.json': JSON.stringify({ extends: '@tsconfig/strictest/tsconfig.json', exclude: ['node_modules', 'tmp'] }),
            'tsconfig.foo.json': JSON.stringify({ extends: './tsconfig.base' }),
            'libs/my-lib/tsconfig.json': JSON.stringify({ extends: ['../../tsconfig.foo.json', '@my-org/my-package/tsconfig.base.json'], include: ['src/**/*.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
            'libs/my-package/package.json': '{}',
            'libs/my-package/tsconfig.base.json': '{}',
            'node_modules/@tsconfig/strictest/tsconfig.json': '{}',
        });
        tempFs.createSymlinkSync('libs/my-package', 'node_modules/@my-org/my-package', 'dir');
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs).toStrictEqual(expect.arrayContaining(['{workspaceRoot}/tsconfig.foo.json', '{workspaceRoot}/tsconfig.base.json', '{workspaceRoot}/libs/my-package/tsconfig.base.json', '{projectRoot}/tsconfig.json']));
        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs?.filter((input): input is string => typeof input === 'string').filter((input) => input.includes('node_modules'))).toStrictEqual([]);
    });

    it('should add files from internal project references and only the tsconfig files of external ones', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({
                include: ['src/**/*.ts'],
                exclude: ['src/**/*.spec.ts'],
                references: [{ path: './tsconfig.lib.json' }, { path: './tsconfig.spec.json' }, { path: './cypress/tsconfig.json' }, { path: './nested-project/tsconfig.json' }, { path: '../other-lib' }],
                compilerOptions: { outDir: 'dist' },
            }),
            'libs/my-lib/tsconfig.lib.json': JSON.stringify({ include: ['src/**/*.ts'], exclude: ['src/**/*.spec.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/tsconfig.spec.json': JSON.stringify({ include: ['src/**/*.spec.ts'], references: [{ path: './tsconfig.lib.json' }], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/cypress/tsconfig.json': JSON.stringify({ include: ['**/*.ts', '../cypress.config.ts', '../**/*.cy.ts'], references: [{ path: '../tsconfig.lib.json' }], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/src/index.spec.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
            'libs/my-lib/nested-project/package.json': '{}',
            'libs/my-lib/nested-project/tsconfig.json': JSON.stringify({ include: ['lib/**/*.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/nested-project/lib/index.ts': 'export {};',
            'libs/other-lib/tsconfig.json': JSON.stringify({ include: ['**/*.ts'] }),
            'libs/other-lib/index.ts': 'export {};',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});
        const inputs = projects['libs/my-lib']?.targets?.['build--tsc']?.inputs;

        expect(inputs).toStrictEqual(expect.arrayContaining(['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/tsconfig.lib.json', '{projectRoot}/tsconfig.spec.json', '{projectRoot}/src/index.ts', '{projectRoot}/src/index.spec.ts']));
        expect(inputs?.filter((input): input is string => typeof input === 'string').filter((input) => /nested-project|other-lib/u.test(input))).toStrictEqual(['{projectRoot}/nested-project/tsconfig.json', '{workspaceRoot}/libs/other-lib/tsconfig.json']);
    });

    it('should add the distinct tsconfig files of file-specific external project references', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ include: ['src/**/*.ts'], references: [{ path: './tsconfig.lib.json' }, { path: './tsconfig.spec.json' }], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/tsconfig.lib.json': JSON.stringify({ include: ['src/**/*.ts'], exclude: ['src/**/*.spec.ts'], references: [{ path: '../other-lib/tsconfig.lib.json' }], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/tsconfig.spec.json': JSON.stringify({ include: ['src/**/*.spec.ts'], references: [{ path: './tsconfig.lib.json' }, { path: '../other-lib/tsconfig.spec.json' }], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
            'libs/other-lib/tsconfig.json': JSON.stringify({ references: [{ path: './tsconfig.lib.json' }, { path: './tsconfig.spec.json' }] }),
            'libs/other-lib/tsconfig.lib.json': JSON.stringify({ include: ['src/**/*.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/other-lib/tsconfig.spec.json': JSON.stringify({ include: ['src/**/*.spec.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/other-lib/src/index.ts': 'export {};',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs).toStrictEqual(expect.arrayContaining(['{workspaceRoot}/libs/other-lib/tsconfig.lib.json', '{workspaceRoot}/libs/other-lib/tsconfig.spec.json']));
    });

    it('should use the nested paths of the tsconfig files an external project reference reaches', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ include: ['src/**/*.ts'], references: [{ path: './tsconfig.lib.json' }, { path: '../other-lib' }], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/tsconfig.lib.json': JSON.stringify({ include: ['src/**/*.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
            'libs/other-lib/tsconfig.json': JSON.stringify({ references: [{ path: './tsconfig.lib.json' }, { path: './cypress/tsconfig.json' }] }),
            'libs/other-lib/tsconfig.lib.json': JSON.stringify({ include: ['src/**/*.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/other-lib/cypress/tsconfig.json': JSON.stringify({ include: ['**/*.cy.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/other-lib/src/index.ts': 'export {};',
            'libs/other-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs).toStrictEqual(expect.arrayContaining(['{workspaceRoot}/libs/other-lib/cypress/tsconfig.json', '{workspaceRoot}/libs/other-lib/tsconfig.lib.json', '{workspaceRoot}/libs/other-lib/tsconfig.json']));
    });

    it('should add the tsconfig files of `extends` chains in external project references', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ include: ['src/**/*.ts'], references: [{ path: './tsconfig.lib.json' }, { path: '../other-lib/tsconfig.lib.json' }], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/tsconfig.lib.json': JSON.stringify({ include: ['src/**/*.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
            'libs/other-lib/tsconfig.json': JSON.stringify({ references: [{ path: './tsconfig.lib.json' }] }),
            'libs/other-lib/tsconfig.lib.json': JSON.stringify({ extends: './tsconfig.shared.json', include: ['src/**/*.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/other-lib/tsconfig.shared.json': JSON.stringify({ compilerOptions: { strict: true } }),
            'libs/other-lib/src/index.ts': 'export {};',
            'libs/other-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs).toStrictEqual(expect.arrayContaining(['{workspaceRoot}/libs/other-lib/tsconfig.lib.json', '{workspaceRoot}/libs/other-lib/tsconfig.shared.json']));
    });

    it('should preserve shared external reference subtrees with cycles and missing configs', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'tsconfig.base.json': JSON.stringify({ compilerOptions: { strict: true } }),
            'apps/app-one/tsconfig.json': JSON.stringify({ files: ['main.ts'], references: [{ path: '../../libs/shared' }], compilerOptions: { outDir: 'dist' } }),
            'apps/app-one/main.ts': 'export {};',
            'apps/app-one/package.json': '{}',
            'apps/app-two/tsconfig.json': JSON.stringify({ files: ['main.ts'], references: [{ path: '../../libs/shared' }], compilerOptions: { outDir: 'dist' } }),
            'apps/app-two/main.ts': 'export {};',
            'apps/app-two/package.json': '{}',
            'libs/shared/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.base.json', files: [], references: [{ path: './tsconfig.lib.json' }, { path: './nested/tsconfig.json' }, { path: './missing/tsconfig.json' }] }),
            'libs/shared/tsconfig.lib.json': JSON.stringify({ files: ['index.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/shared/index.ts': 'export {};',
            'libs/shared/nested/tsconfig.json': JSON.stringify({ files: ['index.ts'], references: [{ path: '../tsconfig.json' }], compilerOptions: { outDir: 'dist' } }),
            'libs/shared/nested/index.ts': 'export {};',
            'libs/shared/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});
        const expectedPatterns = ['{workspaceRoot}/libs/shared/tsconfig.json', '{workspaceRoot}/libs/shared/tsconfig.lib.json', '{workspaceRoot}/libs/shared/nested/tsconfig.json'];

        expect(projects['apps/app-one']?.targets?.['build--tsc']?.inputs).toStrictEqual(expect.arrayContaining(expectedPatterns));
        expect(projects['apps/app-two']?.targets?.['build--tsc']?.inputs).toStrictEqual(expect.arrayContaining(expectedPatterns));
    });

    it('should not reuse reference expansions across invocations', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'apps/my-app/tsconfig.json': JSON.stringify({ files: ['main.ts'], references: [{ path: '../../libs/shared' }], compilerOptions: { outDir: 'dist' } }),
            'apps/my-app/main.ts': 'export {};',
            'apps/my-app/package.json': '{}',
            'libs/shared/tsconfig.json': JSON.stringify({ files: [], references: [{ path: './tsconfig.one.json' }] }),
            'libs/shared/tsconfig.one.json': JSON.stringify({ files: ['index.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/shared/tsconfig.two.json': JSON.stringify({ files: ['index.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/shared/index.ts': 'export {};',
            'libs/shared/package.json': '{}',
        });
        const firstResult = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(firstResult.projects['apps/my-app']?.targets?.['build--tsc']?.inputs).toContain('{workspaceRoot}/libs/shared/tsconfig.one.json');

        await tempFs.createFiles({ 'libs/shared/tsconfig.json': JSON.stringify({ files: [], references: [{ path: './tsconfig.two.json' }] }) });
        setupWorkspaceContext(tempFs.tempDir);
        const secondResult = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(secondResult.projects['apps/my-app']?.targets?.['build--tsc']?.inputs).toContain('{workspaceRoot}/libs/shared/tsconfig.two.json');
        expect(secondResult.projects['apps/my-app']?.targets?.['build--tsc']?.inputs).not.toContain('{workspaceRoot}/libs/shared/tsconfig.one.json');
    });

    it('should only add the tsconfig files of the build reference chain', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ include: ['src/**/*.ts'], references: [{ path: '../other-lib/tsconfig.lib.json' }], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
            'libs/other-lib/tsconfig.json': JSON.stringify({ references: [{ path: './tsconfig.lib.json' }, { path: './tsconfig.spec.json' }] }),
            'libs/other-lib/tsconfig.lib.json': JSON.stringify({ include: ['src/**/*.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/other-lib/tsconfig.spec.json': JSON.stringify({ include: ['src/**/*.spec.ts'], compilerOptions: { outDir: 'dist' } }),
            'libs/other-lib/src/index.ts': 'export {};',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs).toContain('{workspaceRoot}/libs/other-lib/tsconfig.lib.json');
        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs).not.toContain('{workspaceRoot}/libs/other-lib/tsconfig.spec.json');
    });

    it('should add the files of the internal project references a solution tsconfig.json holds', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ files: [], include: [], references: [{ path: './tsconfig.lib.json' }, { path: './tsconfig.spec.json' }], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/tsconfig.lib.json': JSON.stringify({ include: ['src/**/*.ts'], exclude: ['tests'], compilerOptions: { outDir: 'dist' } }),
            'libs/my-lib/tsconfig.spec.json': JSON.stringify({ include: ['tests'], references: [{ path: './tsconfig.lib.json' }], compilerOptions: { outDir: 'dist', allowJs: true } }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/tests/index.test.js': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.options).toStrictEqual({ commands: ['tsc -p tsconfig.lib.json', 'tsc --build tsconfig.spec.json'], cwd: 'libs/my-lib' });
        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs).toStrictEqual(expect.arrayContaining(['{projectRoot}/tsconfig.json', '{projectRoot}/tsconfig.lib.json', '{projectRoot}/tsconfig.spec.json', '{projectRoot}/src/index.ts', '{projectRoot}/tests/index.test.js']));
    });

    it.for([
        [{ include: ['src/**/*.ts'], exclude: ['src/**/*.spec.ts'] }, { include: ['src/**/*.spec.ts'] }, ['src/index.ts', 'src/index.spec.ts', 'src/foo.ts']],
        [{ include: ['**/*.ts'], exclude: ['**/*.spec.ts'] }, { include: ['src/**/*.spec.ts'] }, ['src/index.ts', 'src/index.spec.ts', 'src/foo.ts']],
        [{ include: ['src/**/*.ts'], exclude: ['src/**/*.spec.ts'] }, { include: ['**/*.spec.ts'] }, ['src/index.ts', 'src/index.spec.ts', 'src/foo.ts']],
        [{ include: ['src/**/*.ts'], exclude: ['src/**/*.spec.ts'] }, { include: ['./**/*.spec.ts'] }, ['src/index.ts', 'src/index.spec.ts', 'src/foo.ts']],
        [{ include: ['src/**/*.ts'], exclude: ['src/**/*.spec.ts', 'src/**/foo.ts'] }, { include: ['**/*.spec.ts'] }, ['src/index.ts', 'src/index.spec.ts']],
    ] as const)('should only drop the files an exclude removes that no other tsconfig file includes: lib %j, spec %j', async ([lib, spec, selected], { tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ files: [], references: [{ path: './tsconfig.lib.json' }, { path: './tsconfig.spec.json' }] }),
            'libs/my-lib/tsconfig.lib.json': JSON.stringify({ ...lib, compilerOptions: { noEmit: true } }),
            'libs/my-lib/tsconfig.spec.json': JSON.stringify({ ...spec, references: [{ path: './tsconfig.lib.json' }], compilerOptions: { noEmit: true } }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/src/index.spec.ts': 'export {};',
            'libs/my-lib/src/foo.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(
            projects['libs/my-lib']?.targets?.['check--tsc']?.inputs
                ?.filter((input): input is string => typeof input === 'string')
                .filter((input) => input.startsWith('{projectRoot}/src/'))
                .toSorted(),
        ).toStrictEqual(selected.map((file) => `{projectRoot}/${file}`).toSorted());
    });

    it('should add the files of a `files` list when not using include', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ files: ['main.ts'] }),
            'libs/my-lib/main.ts': 'export {};',
            'libs/my-lib/other.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.inputs).toStrictEqual(['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/main.ts', ...UPSTREAM]);
    });
});

describe('typecheck target outputs', () => {
    it('should add the `outDir`', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ compilerOptions: { outDir: '../../dist/libs/my-lib' }, files: ['main.ts'] }),
            'libs/my-lib/main.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.outputs).toStrictEqual([`{workspaceRoot}/dist/libs/my-lib/${EMITTED}`]);
    });

    it('should include tsbuildinfo file when outDir and rootDir at both set', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ files: [], references: [{ path: './tsconfig.lib.json' }] }),
            'libs/my-lib/tsconfig.lib.json': JSON.stringify({ compilerOptions: { outDir: 'out-tsc/my-lib', rootDir: 'src', composite: true }, files: ['src/main.ts'] }),
            'libs/my-lib/src/main.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']).toMatchObject({
            options: { commands: ['tsc -p tsconfig.lib.json'] },
            outputs: [`{projectRoot}/out-tsc/my-lib/${EMITTED}`, '{projectRoot}/out-tsc/tsconfig.lib.tsbuildinfo'],
        });
    });

    it('should resolve the tsbuildinfo file relative to rootDir, even outside outDir', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ files: [], references: [{ path: './tsconfig.lib.json' }] }),
            'libs/my-lib/tsconfig.lib.json': JSON.stringify({ compilerOptions: { outDir: 'dist', rootDir: 'src', composite: true }, files: ['src/main.ts'] }),
            'libs/my-lib/src/main.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.outputs).toStrictEqual([`{projectRoot}/dist/${EMITTED}`, '{projectRoot}/tsconfig.lib.tsbuildinfo']);
    });

    it('should infer the same tsbuildinfo file from the persisted plugin cache when rootDir is the workspace root', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'tsconfig.base.json': JSON.stringify({ compilerOptions: { rootDir: '.' } }),
            'libs/my-lib/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.base.json', files: [], references: [{ path: './tsconfig.lib.json' }] }),
            'libs/my-lib/tsconfig.lib.json': JSON.stringify({ extends: './tsconfig.json', compilerOptions: { outDir: 'out-tsc/lib', composite: true }, include: ['src/**/*.ts'] }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const fromParsedTsconfigs = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(readdirSync(workspaceDataDirectory)).toContain('typescript.hash');

        const fromPluginCache = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});
        const outputs = fromParsedTsconfigs.projects['libs/my-lib']?.targets?.['build--tsc']?.outputs;

        expect(outputs).toContain('{projectRoot}/out-tsc/lib/libs/my-lib/tsconfig.lib.tsbuildinfo');
        expect(fromPluginCache.projects['libs/my-lib']?.targets?.['build--tsc']?.outputs).toStrictEqual(outputs);
    });

    it('should add the inline output files when `outDir` is not defined', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ files: ['main.ts'] }),
            'libs/my-lib/main.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.outputs).toStrictEqual([`{projectRoot}/${EMITTED}`]);
    });

    it('should collect outputs from all internal project references', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({
                files: [],
                references: [{ path: './tsconfig.lib.json' }, { path: './tsconfig.spec.json' }, { path: './cypress/tsconfig.json' }, { path: './nested-project/tsconfig.json' }, { path: '../other-lib' }],
            }),
            'libs/my-lib/tsconfig.lib.json': JSON.stringify({ compilerOptions: { outDir: '../../dist/libs/my-lib/lib' }, files: ['main.ts'] }),
            'libs/my-lib/tsconfig.spec.json': JSON.stringify({ compilerOptions: { outDir: '../../dist/out-tsc/libs/my-lib/specs' }, include: ['src/**/*.spec.ts'], references: [{ path: './tsconfig.lib.json' }] }),
            'libs/my-lib/cypress/tsconfig.json': JSON.stringify({ compilerOptions: { outDir: '../../../dist/out-tsc/libs/my-lib/cypress' }, references: [{ path: '../tsconfig.lib.json' }] }),
            'libs/my-lib/main.ts': 'export {};',
            'libs/my-lib/src/index.spec.ts': 'export {};',
            'libs/my-lib/cypress/e2e.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
            'libs/my-lib/nested-project/package.json': '{}',
            'libs/my-lib/nested-project/tsconfig.json': JSON.stringify({ compilerOptions: { outDir: '../../../dist/out-tsc/libs/my-lib/nested-project' } }),
            'libs/my-lib/nested-project/index.ts': 'export {};',
            'libs/other-lib/tsconfig.json': JSON.stringify({ compilerOptions: { outDir: '../../dist/out-tsc/libs/other-lib' } }),
            'libs/other-lib/index.ts': 'export {};',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.outputs).toStrictEqual([
            `{workspaceRoot}/dist/libs/my-lib/lib/${EMITTED}`,
            '{workspaceRoot}/dist/libs/my-lib/lib/tsconfig.lib.tsbuildinfo',
            `{workspaceRoot}/dist/out-tsc/libs/my-lib/specs/${EMITTED}`,
            '{workspaceRoot}/dist/out-tsc/libs/my-lib/specs/tsconfig.spec.tsbuildinfo',
        ]);
    });

    it('should respect the "tsBuildInfoFile" option', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ compilerOptions: { incremental: true, tsBuildInfoFile: '../../dist/libs/my-lib/my-lib.tsbuildinfo' }, files: ['main.ts'] }),
            'libs/my-lib/main.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.outputs).toStrictEqual([`{projectRoot}/${EMITTED}`, '{workspaceRoot}/dist/libs/my-lib/my-lib.tsbuildinfo']);
    });

    it('should set the "tsBuildInfoFile" option when outside of the "outDir"', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ compilerOptions: { outDir: 'dist', incremental: true, tsBuildInfoFile: 'my-lib.tsbuildinfo' }, files: ['main.ts'] }),
            'libs/my-lib/main.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.outputs).toStrictEqual([`{projectRoot}/dist/${EMITTED}`, '{projectRoot}/my-lib.tsbuildinfo']);
    });

    it('should not set the "tsBuildInfoFile" option when contained in the "outDir"', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ compilerOptions: { outDir: 'dist', incremental: true, tsBuildInfoFile: 'dist/my-lib.tsbuildinfo' }, files: ['main.ts'] }),
            'libs/my-lib/main.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.outputs).toStrictEqual([`{projectRoot}/dist/${EMITTED}`, '{projectRoot}/dist/my-lib.tsbuildinfo']);
    });

    it('should support the configDir template', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ compilerOptions: { outDir: `\${configDir}/dist` }, files: ['main.ts'] }),
            'libs/my-lib/main.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']?.outputs).toStrictEqual([`{projectRoot}/dist/${EMITTED}`]);
    });
});

describe('typecheck target outside TS solution setups', () => {
    it('should check the app and spec tsconfig files a solution tsconfig.json references without emitting', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'apps/my-app/tsconfig.json': JSON.stringify({ files: [], references: [{ path: './tsconfig.app.json' }, { path: './tsconfig.spec.json' }] }),
            'apps/my-app/tsconfig.app.json': JSON.stringify({ include: ['src/**/*.ts'], exclude: ['src/**/*.spec.ts'], compilerOptions: { noEmit: true } }),
            'apps/my-app/tsconfig.spec.json': JSON.stringify({ include: ['src/**/*.spec.ts'], compilerOptions: { noEmit: true } }),
            'apps/my-app/src/main.ts': 'export {};',
            'apps/my-app/src/main.spec.ts': 'export {};',
            'apps/my-app/package.json': '{}',
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['apps/my-app']?.targets?.['check--tsc']).toMatchObject({ options: { commands: ['tsc -p tsconfig.app.json', 'tsc -p tsconfig.spec.json'], cwd: 'apps/my-app' }, outputs: [] });
        expect(projects['apps/my-app']?.targets?.['check--tsc']?.syncGenerators).toBeUndefined();
    });

    it('should not fail when an extended tsconfig is missing', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'tsconfig.base.json': JSON.stringify({ compilerOptions: {} }),
            'libs/my-lib/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.base.json', include: ['src/**/*.ts'], compilerOptions: { noEmit: true } }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/package.json': '{}',
        });
        await invokeCreateNodesOnMatchingFiles(configFiles, context, {});
        tempFs.removeFileSync('tsconfig.base.json');
        setupWorkspaceContext(tempFs.tempDir);
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['check--tsc']?.options).toStrictEqual({ commands: ['tsc'], cwd: 'libs/my-lib' });
    });
});

describe('build target', () => {
    it('should create build target when the entry points point to source files, as tsc emits and publishConfig exports carry the built entry points', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ files: [], references: [{ path: './tsconfig.lib.json' }] }),
            'libs/my-lib/tsconfig.lib.json': JSON.stringify({ compilerOptions: { outDir: 'dist' }, include: ['src/**/*.ts'] }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/package.json': JSON.stringify({ exports: { '.': { default: './src/index.ts' } }, publishConfig: { exports: { '.': { default: './dist/index.js' } } } }),
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']).toMatchObject({ options: { commands: ['tsc -p tsconfig.lib.json'] }, outputs: [`{projectRoot}/dist/${EMITTED}`] });
    });

    it('should create build target when the entry points point to dist files', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ files: [], references: [{ path: './tsconfig.lib.json' }] }),
            'libs/my-lib/tsconfig.lib.json': JSON.stringify({ compilerOptions: { outDir: 'dist' }, include: ['src/**/*.ts'] }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/package.json': JSON.stringify({ exports: { '.': { development: './src/index.ts', types: './dist/index.d.ts', default: './dist/index.js' } } }),
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']).toMatchObject({ options: { commands: ['tsc -p tsconfig.lib.json'] }, outputs: [`{projectRoot}/dist/${EMITTED}`] });
    });

    it('should create a node with a build target for a tsconfig.lib.json a solution tsconfig.json references', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, {
            'libs/my-lib/tsconfig.json': JSON.stringify({ files: [], references: [{ path: './tsconfig.lib.json' }] }),
            'libs/my-lib/tsconfig.lib.json': JSON.stringify({ compilerOptions: { outDir: 'dist', rootDir: 'src' }, include: ['src/**/*.ts'] }),
            'libs/my-lib/src/index.ts': 'export {};',
            'libs/my-lib/package.json': JSON.stringify({ name: 'my-lib', exports: { '.': './dist/index.js' } }),
        });
        const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});

        expect(projects['libs/my-lib']?.targets?.['build--tsc']).toMatchObject({
            options: { commands: ['tsc -p tsconfig.lib.json'], cwd: 'libs/my-lib' },
            cache: true,
            inputs: ['{projectRoot}/package.json', '{projectRoot}/tsconfig.json', '{projectRoot}/tsconfig.lib.json', '{projectRoot}/src/index.ts', ...UPSTREAM],
            outputs: [`{projectRoot}/dist/${EMITTED}`],
        });
        expect(projects['libs/my-lib']?.targets?.['check--tsc']).toMatchObject({ executor: 'nx:noop', dependsOn: ['build--tsc'] });
    });
});

it('should write every file tsc emits under the declared build outputs and every declared output', { timeout: 30_000 }, async ({ tempFs, context }) => {
    const configFiles = await applyFilesToTempFsAndContext(tempFs, {
        'libs/my-lib/tsconfig.json': JSON.stringify({ compilerOptions: { outDir: 'dist', declaration: true, sourceMap: true, module: 'nodenext', types: [] }, files: ['index.ts'] }),
        'libs/my-lib/index.ts': 'export const value = 1;',
        'libs/my-lib/package.json': '{}',
    });
    const { projects } = await invokeCreateNodesOnMatchingFiles(configFiles, context, {});
    const build = projects['libs/my-lib']?.targets?.['build--tsc'];
    const before = new Set(readdirSync(tempFs.tempDir, { recursive: true, encoding: 'utf-8' }));
    execFileSync(process.execPath, [join(COMPILER, 'bin/tsc')], { cwd: join(tempFs.tempDir, 'libs/my-lib') });
    const written = readdirSync(join(tempFs.tempDir, 'libs/my-lib/dist'), { recursive: true, encoding: 'utf-8' }).map((file) => `libs/my-lib/dist/${file}`);

    expect(build?.options).toStrictEqual({ commands: ['tsc'], cwd: 'libs/my-lib' });
    expect(build?.outputs).toStrictEqual([`{projectRoot}/dist/${EMITTED}`]);
    expect(
        readdirSync(tempFs.tempDir, { recursive: true, encoding: 'utf-8' })
            .filter((file) => !before.has(file))
            .toSorted(),
    ).toStrictEqual(['libs/my-lib/dist', ...written].toSorted());
    expect(written.filter((file) => !matchesGlob(file, `libs/my-lib/dist/${EMITTED}`))).toStrictEqual([]);
    expect(written.toSorted()).toStrictEqual(['libs/my-lib/dist/index.d.ts', 'libs/my-lib/dist/index.js', 'libs/my-lib/dist/index.js.map']);
});

it('should handle concurrent invocations without crashing', async ({ tempFs, context }) => {
    const configFiles = await applyFilesToTempFsAndContext(tempFs, {
        'libs/my-lib/tsconfig.json': JSON.stringify({ compilerOptions: { outDir: '../../dist/libs/my-lib' }, files: ['index.ts'] }),
        'libs/my-lib/index.ts': 'export {};',
        'libs/my-lib/package.json': '{}',
        'libs/other-lib/tsconfig.json': JSON.stringify({ compilerOptions: { outDir: '../../dist/libs/other-lib' }, files: ['index.ts'] }),
        'libs/other-lib/index.ts': 'export {};',
        'libs/other-lib/package.json': '{}',
    });
    const [first, second] = [invokeCreateNodesOnMatchingFiles(configFiles, context, {}), invokeCreateNodesOnMatchingFiles(configFiles, context, {})];
    const [result1, result2] = [await first, await second];

    expect(Object.keys(result1.projects)).toStrictEqual(['libs/my-lib', 'libs/other-lib']);
    expect(result1.projects['libs/my-lib']?.targets?.['check--tsc']).toBeDefined();
    expect(result2).toStrictEqual(result1);
});
