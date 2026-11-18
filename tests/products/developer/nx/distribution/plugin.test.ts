import { chmodSync, globSync } from 'node:fs';
import { join } from 'node:path';
import { env } from 'node:process';
import { type CreateNodesContext, type PostTasksExecutionContext, readJsonFile } from '@nx/devkit';
import { createNodes, postTasksExecution, preTasksExecution } from '@rasm/nx-distribution';
import { answered, remote } from '@rasm/testing/nx';
import { TempFs } from '@rasm/testing/temp-fs';
import { Effect, Exit, Function, Option, Scope } from 'effect';
import { describe, expect, test, vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const host = vi.hoisted((): { platform: NodeJS.Platform; arch: NodeJS.Architecture } => ({ platform: 'darwin', arch: 'arm64' }));
const cached = { server: 'NX_SELF_HOSTED_REMOTE_CACHE_SERVER', token: 'NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN' } as const;
const uploaded = { command: 'gh release upload --help', example: { options: { dryRun: true } } };
const received = [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }];
const transitive = ['...', { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: [] }];
const unstated = ['default', '^default', { externalDependencies: [] }];
const manifests = {
    blender: '# Marks points\nschema_version = "1.0.0"\nid = "marker_tools"\nversion = "1.2.3"\nname = "Marker Tools"\ntagline = "Marks points"\nmaintainer = "Suite <suite@example.com>"\ntype = "add-on"\nblender_version_min = "4.2.0"\nlicense = ["SPDX:MIT"]\n',
    yak: '# Marks points\nname: marker\nversion: 1.2.3\nauthors:\n    - Suite\ndescription: Marks points\nurl: https://example.com\n',
    uxp: '{\n    "manifestVersion": 5,\n    "id": "com.example.panel",\n    "name": "Panel",\n    "version": "1.2.3",\n    "main": "index.html",\n    "host": { "app": "PS", "minVersion": "24.0.0" }\n}\n',
} as const;

vi.mock(import('node:process'), async (actual) => ({
    ...(await actual()),
    arch: host.arch,
    get platform() {
        return host.platform;
    },
}));

// --- [OPERATIONS] ----------------------------------------------------------------------

const released = (manifest: string) => ({ version: { versionActions: '@rasm/nx-distribution/release/version-actions', versionActionsOptions: { manifest } } });

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('distribution-plugin');
        onCleanup(() => {
            vi.unstubAllEnvs();
            tempFs.cleanup();
        });
        tempFs.createFilesSync({ 'pnpm-lock.yaml': '', 'bin/.keep': '' });
        vi.stubEnv('PATH', join(tempFs.tempDir, 'bin'));
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({
        nxJsonConfiguration: {
            namedInputs: {
                default: ['{projectRoot}/**/*'],
                production: ['!{projectRoot}/**/*.spec.ts'],
            },
            release: { projects: ['tag:release'] },
        },
        workspaceRoot: tempFs.tempDir,
    }))
    .extend('hosted', ({}, { onCleanup }) => {
        onCleanup(() => {
            host.platform = 'darwin';
        });
        return (platform: NodeJS.Platform) => {
            host.platform = platform;
        };
    })
    .extend('run', ({ tempFs }): PostTasksExecutionContext => {
        vi.stubEnv('NX_TASK_TARGET_PROJECT', undefined);
        vi.stubEnv('GITHUB_ACTIONS', undefined);
        vi.stubEnv('CI', 'false');
        return {
            id: 'run-7f3a',
            workspaceRoot: tempFs.tempDir,
            nxJsonConfiguration: {},
            argv: ['node', 'nx', 'run-many', '--targets=build,check,test,e2e'],
            startTime: 1000,
            endTime: 9000,
            taskResults: {
                'app:build:release': {
                    task: {
                        id: 'app:build:release',
                        target: { project: 'app', target: 'build', configuration: 'release' },
                        overrides: {},
                        outputs: ['dist/app'],
                        cache: true,
                        hash: 'hash-app-build',
                        hashDetails: { command: 'command-app-build', nodes: { 'app:{projectRoot}/**/*': 'node-1', 'env:CI': 'node-2' }, implicitDeps: { 'nx.json': 'implicit-1' }, runtime: { 'node -v': 'runtime-1' } },
                        startTime: 1100,
                        endTime: 4100,
                    },
                    status: 'failure',
                    code: 2,
                    terminalOutput: 'error TS2322',
                },
                'app:test': { task: { id: 'app:test', target: { project: 'app', target: 'test' }, overrides: {}, outputs: [], cache: true, hash: 'hash-app-test', hashDetails: { command: 'command-app-test', nodes: { 'app:{projectRoot}/**/*': 'node-1' } }, startTime: 1200, endTime: 5200 }, status: 'failure', code: 1 },
                'app:e2e': { task: { id: 'app:e2e', target: { project: 'app', target: 'e2e' }, overrides: {}, outputs: [], cache: false }, status: 'skipped', code: 1 },
                'lib:build': {
                    task: { id: 'lib:build', target: { project: 'lib', target: 'build' }, overrides: {}, outputs: ['dist/lib'], cache: true, hash: 'hash-lib-build', hashDetails: { command: 'command-lib-build', nodes: { 'lib:{projectRoot}/**/*': 'node-3' } }, startTime: 1300, endTime: 1310 },
                    status: 'remote-cache',
                    code: 0,
                },
                'lib:check': {
                    task: { id: 'lib:check', target: { project: 'lib', target: 'check' }, overrides: {}, outputs: [], cache: true, hash: 'hash-lib-check', hashDetails: { command: 'command-lib-check', nodes: { 'lib:{projectRoot}/**/*': 'node-3' } }, startTime: 1400, endTime: 1405 },
                    status: 'local-cache',
                    code: 0,
                },
                'lib:test': { task: { id: 'lib:test', target: { project: 'lib', target: 'test' }, overrides: {}, outputs: [], cache: true, hash: 'hash-lib-test', hashDetails: { command: 'command-lib-test', nodes: { 'lib:{projectRoot}/**/*': 'node-3' } }, startTime: 1500, endTime: 8500 }, status: 'success', code: 0 },
            },
        };
    })
    .extend('printed', ({}, { onCleanup }) => {
        const log = vi.spyOn(console, 'log').mockReturnValue(undefined);
        const warn = vi.spyOn(console, 'warn').mockReturnValue(undefined);
        onCleanup(() => {
            log.mockRestore();
            warn.mockRestore();
        });
        return { log, warn };
    })
    .extend('cache', ({}, { onCleanup }) => {
        const scope = Effect.runSync(Scope.make());
        onCleanup(() => Effect.runPromise(Scope.close(scope, Exit.void)));
        vi.stubEnv(cached.token, 'held-token');
        return (answer: Parameters<typeof remote>[1]) => Effect.runPromise(Effect.provideService(remote('held-token', answer), Scope.Scope, scope));
    });

describe('@rasm/nx-distribution', () => {
    const [, createNodesFunction] = createNodes;
    const ran = 'pnpm exec nx run --help';

    it('should create nodes based on blender_manifest.toml', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'proj/blender_manifest.toml': manifests.blender, 'proj/__init__.py': '' });

        const results = await createNodesFunction(['proj/blender_manifest.toml'], {}, context);

        const technologies = ['blender'];
        const inputs = ['production', '^production', { runtime: String.raw`uv run --no-project python -c 'import hashlib, os, shutil; print(hashlib.file_digest(open(shutil.which('\''blender_ext.py'\'', os.F_OK), '\''rb'\''), '\''sha256'\'').hexdigest())'` }, { externalDependencies: [] }];
        expect(results).toStrictEqual([
            [
                'proj/blender_manifest.toml',
                {
                    projects: {
                        proj: {
                            root: 'proj',
                            name: 'marker_tools',
                            metadata: { technologies },
                            tags: ['release'],
                            release: released('blender_manifest.toml'),
                            targets: {
                                build: { dependsOn: [{ target: 'build--*', params: 'forward' }], inputs: ['...', ...inputs] },
                                'build--blender': {
                                    executor: '@rasm/nx-distribution:extension-build',
                                    options: { outputPath: '{projectRoot}' },
                                    cache: true,
                                    inputs,
                                    outputs: ['{options.outputPath}/*.zip'],
                                    metadata: {
                                        technologies,
                                        description: 'Builds the blender project its blender_manifest.toml declares with blender_ext.py build',
                                        help: { command: String.raw`uv run --no-project python -c 'import os, runpy, shutil, sys; sys.argv[0] = shutil.which('\''blender_ext.py'\'', os.F_OK); runpy.run_path(sys.argv[0], run_name='\''__main__'\'')' build --help`, example: { options: { outputPath: '{projectRoot}' } } },
                                    },
                                },
                                check: { dependsOn: [{ target: 'check--*', params: 'forward' }], inputs: ['...', ...inputs] },
                                'check--blender': {
                                    executor: '@rasm/nx-distribution:extension-build',
                                    parallelism: false,
                                    cache: true,
                                    inputs,
                                    outputs: [],
                                    options: { outputPath: '{projectRoot}', operation: 'check' },
                                    metadata: { technologies, description: 'Checks the blender project its blender_manifest.toml declares with blender_ext.py validate', help: { command: ran, example: { options: { operation: 'check' } } } },
                                },
                                clean: { dependsOn: [{ target: 'clean--*', params: 'forward' }], inputs: ['...', ...unstated] },
                                'clean--blender': { executor: '@rasm/nx-distribution:clean', options: { targets: ['build--blender'] }, inputs: unstated, metadata: { technologies, description: 'Removes the outputs build--blender declare', help: { command: ran, example: { options: { targets: ['build--blender'] } } } } },
                                attach: { dependsOn: [{ target: 'attach--*', params: 'forward' }], inputs: transitive },
                                'attach--gh': {
                                    executor: '@rasm/nx-distribution:attach',
                                    dependsOn: ['build--blender'],
                                    inputs: received,
                                    metadata: { technologies, description: 'Uploads the files build--blender wrote to the GitHub release of the tag nx release made for the project at the checked-out commit with gh release upload', help: uploaded },
                                },
                                'nx-release-publish': {
                                    executor: '@rasm/nx-distribution:release-publish',
                                    dependsOn: ['build--blender', '^nx-release-publish'],
                                    inputs: received,
                                    options: { credential: 'BLENDER_EXTENSIONS_TOKEN' },
                                    metadata: {
                                        technologies,
                                        description: 'Publishes the .zip packages of the project to https://extensions.blender.org with POST /api/v1/extensions/<id>/versions/upload/, under the credential BLENDER_EXTENSIONS_TOKEN holds',
                                        help: { command: ran, example: { options: { dryRun: true } } },
                                    },
                                },
                            },
                        },
                    },
                },
            ],
        ]);
    });

    it('should create nodes based on manifest.yml', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'proj/manifest.yml': manifests.yak });

        const results = await createNodesFunction(['proj/manifest.yml'], {}, context);

        const technologies = ['yak'];
        const inputs = ['production', '^production', { dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }];
        expect(results).toStrictEqual([
            [
                'proj/manifest.yml',
                {
                    projects: {
                        proj: {
                            root: 'proj',
                            name: 'marker',
                            metadata: { technologies },
                            tags: ['release'],
                            release: released('manifest.yml'),
                            targets: {
                                build: { dependsOn: [{ target: 'build--*', params: 'forward' }], inputs: ['...', 'production', '^production', { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: [] }] },
                                'build--yak': {
                                    executor: '@rasm/nx-distribution:package',
                                    dependsOn: ['build--*'],
                                    options: { outputPath: '{projectRoot}' },
                                    cache: true,
                                    inputs,
                                    outputs: ['{options.outputPath}/*.yak'],
                                    metadata: { technologies, description: 'Builds the yak project its manifest.yml declares with yak build', help: { command: 'yak build --help', example: { options: { outputPath: '{projectRoot}' } } } },
                                },
                                install: { dependsOn: [{ target: 'install--*', params: 'forward' }], inputs: transitive },
                                'install--yak': {
                                    executor: '@rasm/nx-distribution:install',
                                    dependsOn: ['build--yak'],
                                    inputs: received,
                                    metadata: { technologies, description: "Installs what build wrote in the run's configuration where its host loads it with yak install", help: { command: 'yak install --help', example: { args: [] } } },
                                },
                                clean: { dependsOn: [{ target: 'clean--*', params: 'forward' }], inputs: ['...', ...unstated] },
                                'clean--yak': { executor: '@rasm/nx-distribution:clean', options: { targets: ['build--yak'] }, inputs: unstated, metadata: { technologies, description: 'Removes the outputs build--yak declare', help: { command: ran, example: { options: { targets: ['build--yak'] } } } } },
                                attach: { dependsOn: [{ target: 'attach--*', params: 'forward' }], inputs: transitive },
                                'attach--gh': {
                                    executor: '@rasm/nx-distribution:attach',
                                    dependsOn: ['build--yak'],
                                    inputs: received,
                                    metadata: { technologies, description: 'Uploads the files build--yak wrote to the GitHub release of the tag nx release made for the project at the checked-out commit with gh release upload', help: uploaded },
                                },
                                'nx-release-publish': {
                                    executor: '@rasm/nx-distribution:release-publish',
                                    dependsOn: ['build--yak', '^nx-release-publish'],
                                    inputs: received,
                                    options: { credential: 'YAK_TOKEN' },
                                    metadata: { technologies, description: 'Publishes the .yak packages of the project to https://yak.rhino3d.com with yak push, under the credential YAK_TOKEN holds', help: { command: ran, example: { options: { dryRun: true } } } },
                                },
                            },
                        },
                    },
                },
            ],
        ]);
    });

    it('should create nodes based on manifest.json', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'proj/manifest.json': manifests.uxp, 'proj/index.html': '<!doctype html><title>Panel</title>\n' });

        const results = await createNodesFunction(['proj/manifest.json'], {}, context);

        const technologies = ['uxp'];
        const inputs = ['production', '^production', { dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }];
        expect(results).toStrictEqual([
            [
                'proj/manifest.json',
                {
                    projects: {
                        proj: {
                            root: 'proj',
                            name: 'com.example.panel',
                            metadata: { technologies },
                            tags: ['release'],
                            release: released('manifest.json'),
                            targets: {
                                build: { dependsOn: [{ target: 'build--*', params: 'forward' }], inputs: ['...', 'production', '^production', { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: [] }] },
                                'build--uxp': {
                                    executor: '@rasm/nx-distribution:package',
                                    dependsOn: ['build--*'],
                                    options: { outputPath: '{projectRoot}' },
                                    cache: true,
                                    inputs,
                                    outputs: ['{options.outputPath}/*.ccx'],
                                    metadata: { technologies, description: 'Builds the uxp project its manifest.json declares with a zip archive named .ccx', help: { command: ran, example: { options: { outputPath: '{projectRoot}' } } } },
                                },
                                install: { dependsOn: [{ target: 'install--*', params: 'forward' }], inputs: transitive },
                                'install--uxp': {
                                    executor: '@rasm/nx-distribution:install',
                                    dependsOn: ['build--!(uxp)'],
                                    inputs: received,
                                    metadata: {
                                        technologies,
                                        description: "Installs what build wrote in the run's configuration where its host loads it with a copy into the per-user UXP plug-in folder in place of its earlier folders and CEP extension, and the plug-in's row in the PluginsInfo registry of a host that reads one",
                                        help: { command: ran, example: { args: [] } },
                                    },
                                },
                                clean: { dependsOn: [{ target: 'clean--*', params: 'forward' }], inputs: ['...', ...unstated] },
                                'clean--uxp': { executor: '@rasm/nx-distribution:clean', options: { targets: ['build--uxp'] }, inputs: unstated, metadata: { technologies, description: 'Removes the outputs build--uxp declare', help: { command: ran, example: { options: { targets: ['build--uxp'] } } } } },
                                attach: { dependsOn: [{ target: 'attach--*', params: 'forward' }], inputs: transitive },
                                'attach--gh': {
                                    executor: '@rasm/nx-distribution:attach',
                                    dependsOn: ['build--uxp'],
                                    inputs: received,
                                    metadata: { technologies, description: 'Uploads the files build--uxp wrote to the GitHub release of the tag nx release made for the project at the checked-out commit with gh release upload', help: uploaded },
                                },
                            },
                        },
                    },
                },
            ],
        ]);
    });

    it('should create nodes based on a .uproject and the .uplugin it loads', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'Engine/Engine/Build/Build.version': '{ "MajorVersion": 5, "MinorVersion": 6, "PatchVersion": 0 }\n',
            'Game/Game.uproject': '{ "FileVersion": 3, "EngineAssociation": "../Engine", "Plugins": [{ "Name": "Marker", "Enabled": true }] }\n',
            'Game/Plugins/Marker/Marker.uplugin': '{ "FileVersion": 3, "Version": 1, "VersionName": "1.2.3", "FriendlyName": "Marker" }\n',
        });

        const results = await createNodesFunction(['Game/Game.uproject', 'Game/Plugins/Marker/Marker.uplugin'], {}, context);

        const technologies = ['unreal'];
        const engine = { runtime: `cat ${tempFs.tempDir}/Engine/Engine/Build/Build.version` };
        const game = ['production', '^production', engine, { externalDependencies: [] }];
        const marker = ['production', '^production', { runtime: 'node -p process.platform+process.arch' }, engine, { externalDependencies: [] }];
        const hosted = [...marker, { json: '{workspaceRoot}/Game/Game.uproject', fields: ['EngineAssociation'] }];
        const loaded = { executor: 'nx:noop', dependsOn: [{ projects: ['Game'], target: 'build' }], inputs: unstated, metadata: { technologies, description: 'Builds Game, the host the result loads in, with its build target', help: { command: ran, example: { options: {} } } } };
        const plugin = { outputPath: '{projectRoot}/Saved', descriptor: 'Game/Plugins/Marker/Marker.uplugin', hosts: ['Game/Game.uproject'], dependencies: [] };
        const project = { outputPath: '{projectRoot}/Saved', descriptor: 'Game/Game.uproject', hosts: ['Game/Game.uproject'], dependencies: ['Game/Plugins/Marker/Marker.uplugin'] };
        expect(results[0]).toStrictEqual([
            'Game/Game.uproject',
            {
                projects: {
                    Game: {
                        root: 'Game',
                        name: 'Game',
                        metadata: { technologies },
                        tags: ['os:darwin', 'os:linux'],
                        implicitDependencies: ['Marker'],
                        targets: {
                            build: { dependsOn: [{ target: 'build--*', params: 'forward' }], inputs: ['...', ...game] },
                            'build--unreal': {
                                executor: '@rasm/nx-distribution:unreal-build',
                                options: project,
                                parallelism: false,
                                cache: true,
                                inputs: game,
                                outputs: ['{projectRoot}/Binaries', '{projectRoot}/Plugins/**/Binaries'],
                                metadata: { technologies, description: 'Builds the unreal project its Game.uproject declares with UnrealBuildTool', help: { command: ran, example: { options: { outputPath: '{projectRoot}/Saved' } } } },
                            },
                            check: { dependsOn: [{ target: 'check--*', params: 'forward' }], inputs: ['...', ...game] },
                            'check--unreal': {
                                executor: '@rasm/nx-distribution:unreal-build',
                                parallelism: false,
                                cache: true,
                                inputs: game,
                                outputs: [],
                                options: { ...project, operation: 'check' },
                                metadata: { technologies, description: 'Checks the unreal project its Game.uproject declares with UnrealBuildTool -StaticAnalyzer=Default -StaticAnalyzerProjectOnly -DisableUnity -NoPCH', help: { command: ran, example: { options: { operation: 'check' } } } },
                            },
                            install: { dependsOn: [{ target: 'install--*', params: 'forward' }], inputs: ['...', ...unstated] },
                            'install--unreal': loaded,
                            clean: { dependsOn: [{ target: 'clean--*', params: 'forward' }], inputs: ['...', ...unstated] },
                            'clean--unreal': { executor: '@rasm/nx-distribution:clean', options: { targets: ['build--unreal'] }, inputs: unstated, metadata: { technologies, description: 'Removes the outputs build--unreal declare', help: { command: ran, example: { options: { targets: ['build--unreal'] } } } } },
                        },
                    },
                },
            },
        ]);
        expect(results[1]).toStrictEqual([
            'Game/Plugins/Marker/Marker.uplugin',
            {
                projects: {
                    'Game/Plugins/Marker': {
                        root: 'Game/Plugins/Marker',
                        name: 'Marker',
                        metadata: { technologies },
                        tags: ['release', 'os:darwin', 'os:linux'],
                        release: released('Marker.uplugin'),
                        targets: {
                            build: { dependsOn: [{ target: 'build--*', params: 'forward' }], inputs: ['...', ...hosted] },
                            'build--unreal': {
                                executor: '@rasm/nx-distribution:unreal-build',
                                options: plugin,
                                parallelism: false,
                                cache: true,
                                inputs: hosted,
                                outputs: ['{options.outputPath}/package'],
                                metadata: { technologies, description: 'Builds the unreal project its Marker.uplugin declares with AutomationTool BuildPlugin -StrictIncludes', help: { command: ran, example: { options: { outputPath: '{projectRoot}/Saved' } } } },
                            },
                            check: { dependsOn: [{ target: 'check--*', params: 'forward' }], inputs: ['...', ...hosted] },
                            'check--unreal': { executor: 'nx:noop', dependsOn: ['build--unreal'], inputs: hosted, metadata: { technologies, description: 'Checks the unreal project with its build--unreal compile', help: { command: ran, example: { options: {} } } } },
                            test: { dependsOn: [{ target: 'test--*', params: 'forward' }], inputs: ['...', ...marker, '{workspaceRoot}/Game/Game.uproject'] },
                            'test--unreal': {
                                executor: '@rasm/nx-distribution:unreal-build',
                                parallelism: false,
                                cache: true,
                                inputs: [...marker, '{workspaceRoot}/Game/Game.uproject'],
                                dependsOn: ['install--unreal'],
                                outputs: ['{options.outputPath}/report'],
                                options: { ...plugin, operation: 'test' },
                                metadata: {
                                    technologies,
                                    description: 'Tests the unreal project its Marker.uplugin declares with UnrealEditor-Cmd -ExecCmds="Automation RunTests ^<plug-in name>.;Quit" -nullrhi -unattended in each project loading the plug-in, then a read of each test state from the exported report',
                                    help: { command: ran, example: { options: { operation: 'test' } } },
                                },
                            },
                            pack: { dependsOn: [{ target: 'pack--*', params: 'forward' }], inputs: ['...', ...hosted] },
                            'pack--unreal': {
                                executor: '@rasm/nx-distribution:unreal-build',
                                parallelism: false,
                                cache: true,
                                inputs: hosted,
                                dependsOn: ['build--unreal'],
                                outputs: ['{options.outputPath}/archive'],
                                options: { ...plugin, operation: 'pack' },
                                metadata: {
                                    technologies,
                                    description: "Packs the unreal project its Marker.uplugin declares with the Fab store checks, then one zip per engine of the tracked sources with each installed package and that engine's EngineVersion",
                                    help: { command: ran, example: { options: { operation: 'pack' } } },
                                },
                            },
                            install: { dependsOn: [{ target: 'install--*', params: 'forward' }], inputs: ['...', ...unstated] },
                            'install--unreal': loaded,
                            clean: { dependsOn: [{ target: 'clean--*', params: 'forward' }], inputs: ['...', ...unstated] },
                            'clean--unreal': {
                                executor: '@rasm/nx-distribution:clean',
                                options: { targets: ['build--unreal', 'test--unreal', 'pack--unreal'] },
                                inputs: unstated,
                                metadata: { technologies, description: 'Removes the outputs build--unreal, test--unreal, pack--unreal declare', help: { command: ran, example: { options: { targets: ['build--unreal', 'test--unreal', 'pack--unreal'] } } } },
                            },
                            attach: { dependsOn: [{ target: 'attach--*', params: 'forward' }], inputs: transitive },
                            'attach--gh': {
                                executor: '@rasm/nx-distribution:attach',
                                dependsOn: ['pack--unreal'],
                                inputs: received,
                                metadata: { technologies, description: 'Uploads the files pack--unreal wrote to the GitHub release of the tag nx release made for the project at the checked-out commit with gh release upload', help: uploaded },
                            },
                        },
                    },
                },
            },
        ]);
    });

    it('should use default namedInputs when production is not available', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'proj/blender_manifest.toml': manifests.blender });

        const results = await createNodesFunction(['proj/blender_manifest.toml'], {}, { ...context, nxJsonConfiguration: { namedInputs: { default: ['{projectRoot}/**/*'] } } });

        expect(results[0]?.[1].projects?.['proj']?.targets?.['build--blender']?.inputs?.slice(0, 2)).toStrictEqual(['default', '^default']);
        expect(results[0]?.[1].projects?.['proj']?.targets?.['check--blender']?.inputs?.slice(0, 2)).toStrictEqual(['default', '^default']);
    });

    it('should handle nested project structures', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'apps/api/manifest.yml': manifests.yak });

        const results = await createNodesFunction(['apps/api/manifest.yml'], {}, context);

        expect(results[0]?.[0]).toBe('apps/api/manifest.yml');
        expect(results[0]?.[1].projects?.['apps/api']).toMatchObject({ root: 'apps/api', name: 'marker', targets: { 'build--yak': { options: { outputPath: '{projectRoot}' } } } });
    });

    it('should handle multiple manifests', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'app1/blender_manifest.toml': manifests.blender, 'app2/manifest.yml': manifests.yak });

        const results = await createNodesFunction(['app1/blender_manifest.toml', 'app2/manifest.yml'], {}, context);

        expect(results).toHaveLength(2);
        expect(results[0]?.[0]).toBe('app1/blender_manifest.toml');
        expect(results[1]?.[0]).toBe('app2/manifest.yml');
        expect(results[0]?.[1].projects?.['app1']?.root).toBe('app1');
        expect(results[1]?.[1].projects?.['app2']?.root).toBe('app2');
    });

    it('should return equal results for two calls over the same files', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'proj/blender_manifest.toml': manifests.blender });

        const results1 = await createNodesFunction(['proj/blender_manifest.toml'], {}, context);
        const results2 = await createNodesFunction(['proj/blender_manifest.toml'], {}, context);

        expect(results1).toStrictEqual(results2);
    });

    it('should handle root level manifest', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'manifest.yml': manifests.yak });

        const results = await createNodesFunction(['manifest.yml'], {}, context);

        expect(results[0]?.[1].projects?.['.']?.root).toBe('.');
    });

    it('should give the folder holding .github/workflows one jactionlint upgrade over its workflows and composite actions', async ({ tempFs, context }) => {
        await tempFs.createFiles({ '.github/workflows/ci.yml': 'on: push\n', '.github/workflows/release.yaml': 'on: push\n' });

        const results = await createNodesFunction(['.github/workflows/ci.yml', '.github/workflows/release.yaml'], {}, context);

        const inputs = ['{projectRoot}/.github/**/*', '{projectRoot}/**/action.{yml,yaml}', { externalDependencies: [] }];
        expect(results).toStrictEqual([
            [
                '.github/workflows/ci.yml',
                {
                    projects: {
                        '.': {
                            root: '.',
                            targets: {
                                upgrade: { dependsOn: [{ target: 'upgrade--*', params: 'forward' }], inputs: ['...', ...inputs] },
                                'upgrade--jactionlint': {
                                    executor: '@rasm/nx-distribution:upgrade',
                                    options: { cwd: '{projectRoot}', commands: [{ apply: ['jactionlint', 'update', '--apply'], preview: ['jactionlint', 'update', '--diff'] }] },
                                    inputs,
                                    metadata: {
                                        technologies: ['github-actions'],
                                        description: 'Moves every external action and reusable workflow the workflows and composite actions use to its newest release with jactionlint update --apply, keeping commit pins and their version comments',
                                        help: { command: 'jactionlint update --help', example: { options: { dryRun: true } } },
                                    },
                                },
                            },
                        },
                    },
                },
            ],
        ]);
    });

    it('should not create nodes for a manifest.json that is no UXP manifest', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'web/site/manifest.json': '{ "name": "Site", "short_name": "Site", "start_url": "/", "display": "standalone", "icons": [] }\n' });

        const results = await createNodesFunction(['web/site/manifest.json'], {}, context);

        expect(results).toStrictEqual([]);
    });

    it('should name a manifest that fails to parse in an AggregateCreateNodesError holding the projects of every other manifest', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'torn/blender_manifest.toml': 'id = "torn\nversion = \n', 'proj/manifest.yml': manifests.yak });

        await expect(() => createNodesFunction(['torn/blender_manifest.toml', 'proj/manifest.yml'], {}, context)).rejects.toMatchObject({
            name: 'AggregateCreateNodesError',
            errors: [['torn/blender_manifest.toml', expect.any(Error)]],
            partialResults: [['proj/manifest.yml', { projects: { proj: { name: 'marker' } } }]],
        });
    });

    describe('normalizePluginOptions', () => {
        it('should use default values when options are not provided', async ({ tempFs, context }) => {
            await tempFs.createFiles({ 'proj/manifest.yml': manifests.yak });

            const results = await createNodesFunction(['proj/manifest.yml'], undefined, context);

            expect(results[0]?.[1].projects?.['proj']?.targets?.['build--yak']?.options).toStrictEqual({ outputPath: '{projectRoot}' });
            expect(results[0]?.[1].projects?.['proj']?.targets?.['nx-release-publish']?.options).toStrictEqual({ credential: 'YAK_TOKEN' });
        });

        it('should write each package to outputPath', async ({ tempFs, context }) => {
            await tempFs.createFiles({ 'proj/manifest.yml': manifests.yak });

            const results = await createNodesFunction(['proj/manifest.yml'], { outputPath: '.artifacts/distribution/{projectName}' }, context);

            expect(results[0]?.[1].projects?.['proj']?.targets?.['build--yak']).toMatchObject({ options: { outputPath: '.artifacts/distribution/{projectName}' }, outputs: ['{options.outputPath}/*.yak'] });
        });

        it('should read the store credential from the variable storeCredentials names', async ({ tempFs, context }) => {
            await tempFs.createFiles({ 'proj/blender_manifest.toml': manifests.blender });

            const results = await createNodesFunction(['proj/blender_manifest.toml'], { storeCredentials: { blender: 'EXTENSIONS_TOKEN' } }, context);

            expect(results[0]?.[1].projects?.['proj']?.targets?.['nx-release-publish']?.options).toStrictEqual({ credential: 'EXTENSIONS_TOKEN' });
        });
    });

    describe('dependency handling', () => {
        it.for([
            ['proj/manifest.yml', manifests.yak, 'build--yak'],
            ['proj/manifest.json', manifests.uxp, 'build--uxp'],
            ['proj/blender_manifest.toml', manifests.blender, 'build--blender'],
        ] as const)('should set the attach target of %s to depend on its packaging target', async ([file, text, packer], { tempFs, context }) => {
            await tempFs.createFiles({ [file]: text });

            const results = await createNodesFunction([file], {}, context);

            expect(results[0]?.[1].projects?.['proj']?.targets).toMatchObject({ attach: { dependsOn: [{ target: 'attach--*', params: 'forward' }] }, 'attach--gh': { dependsOn: [packer] } });
        });

        it('should give nx-release-publish only to a host whose store takes uploads, under the variable that store reads', async ({ tempFs, context }) => {
            await tempFs.createFiles({ 'blender/blender_manifest.toml': manifests.blender, 'yak/manifest.yml': manifests.yak, 'uxp/manifest.json': manifests.uxp });

            const results = await createNodesFunction(['blender/blender_manifest.toml', 'yak/manifest.yml', 'uxp/manifest.json'], {}, context);

            expect(results[0]?.[1].projects?.['blender']?.targets?.['nx-release-publish']?.options).toStrictEqual({ credential: 'BLENDER_EXTENSIONS_TOKEN' });
            expect(results[1]?.[1].projects?.['yak']?.targets?.['nx-release-publish']?.options).toStrictEqual({ credential: 'YAK_TOKEN' });
            expect(results[2]?.[1].projects?.['uxp']?.targets).not.toHaveProperty('nx-release-publish');
        });
    });

    describe('host platforms', () => {
        it.for([
            ['darwin', 'object'],
            ['win32', 'object'],
            ['linux', 'undefined'],
        ] as const)('should infer the targets a host runs or needs on %s alone', async ([platform, inferred], { tempFs, context, hosted }) => {
            hosted(platform);
            await tempFs.createFiles({ 'yak/manifest.yml': manifests.yak, 'uxp/manifest.json': manifests.uxp });

            const results = await createNodesFunction(['yak/manifest.yml', 'uxp/manifest.json'], {}, context);

            expect(results[0]?.[1].projects?.['yak']?.targets?.['install--yak']).toBeTypeOf(inferred);
            expect(results[1]?.[1].projects?.['uxp']?.targets?.['install--uxp']).toBeTypeOf(inferred);
        });

        it('should infer install--blender where blender is on PATH alone', async ({ tempFs, context }) => {
            await tempFs.createFiles({ 'proj/blender_manifest.toml': manifests.blender, 'bin/blender': '#!/bin/sh\n' });
            chmodSync(join(tempFs.tempDir, 'bin/blender'), 0o755);

            const results = await createNodesFunction(['proj/blender_manifest.toml'], {}, context);

            expect(results[0]?.[1].projects?.['proj']?.targets?.['install--blender']?.dependsOn).toStrictEqual(['build--blender']);
        });
    });

    describe('preTasksExecution', () => {
        it.for([
            ['404 to the held token', Function.flow(answered, Option.some)],
            ['200', Function.constant(Option.some(200))],
        ] as const)('should keep the server variable and print nothing where one read of the run is answered %s', async ([, answer], { run, cache, printed }) => {
            const { address, requests } = await cache(answer);
            vi.stubEnv(cached.server, address);

            await preTasksExecution({}, run);

            expect(env[cached.server]).toBe(address);
            expect(requests).toStrictEqual(['GET /v1/cache/run-7f3a']);
            expect(printed.warn).not.toHaveBeenCalled();
        });

        it.for([
            ['answered 401', Function.constant(Option.some(401)), '10 seconds', String.raw`\b401\b`],
            ['answered 500', Function.constant(Option.some(500)), '10 seconds', String.raw`\b500\b`],
            ['unanswered within remoteCacheTimeout', Function.constant(Option.none()), '50 millis', String.raw`\S`],
        ] as const)('should empty the server variable and print one warning where the read is %s', async ([, answer, remoteCacheTimeout, cause], { run, cache, printed }) => {
            const { address } = await cache(answer);
            vi.stubEnv(cached.server, address);

            await preTasksExecution({ remoteCacheTimeout }, run);

            expect(env[cached.server]).toBe('');
            expect(printed.warn.mock.calls).toStrictEqual([[expect.stringMatching(new RegExp(`^${RegExp.escape(`Remote cache ${address} serves no read, so this run uses the local cache alone: `)}.*${cause}`, 'u'))]]);
        });

        it('should empty the server variable and print one warning where the connection is refused', async ({ run, printed }) => {
            const { address } = await Effect.runPromise(Effect.scoped(remote('held-token', Function.constant(Option.none()))));
            vi.stubEnv(cached.server, address);

            await preTasksExecution({ remoteCacheTimeout: '10 seconds' }, run);

            expect(env[cached.server]).toBe('');
            expect(printed.warn.mock.calls).toStrictEqual([[expect.stringMatching(new RegExp(`^${RegExp.escape(`Remote cache ${address} serves no read, so this run uses the local cache alone: `)}\\S`, 'u'))]]);
        });

        it('should print the warning as a workflow command under GITHUB_ACTIONS', async ({ run, cache, printed }) => {
            const { address } = await cache(Function.constant(Option.some(500)));
            vi.stubEnv(cached.server, address);
            vi.stubEnv('GITHUB_ACTIONS', 'true');

            await preTasksExecution({ remoteCacheTimeout: '10 seconds' }, run);

            expect(printed.warn.mock.calls).toStrictEqual([[expect.stringMatching(new RegExp(`^${RegExp.escape(`::warning title=Nx remote cache::Remote cache ${address} serves no read, so this run uses the local cache alone: `)}.*\\b500\\b`, 'u'))]]);
        });

        it.for([undefined, ''] as const)('should resolve and print nothing where the server variable is %o', async (stated, { run, printed }) => {
            vi.stubEnv(cached.server, stated);

            await expect(preTasksExecution({ remoteCacheTimeout: '10 seconds' }, run)).resolves.toBeUndefined();

            expect(env[cached.server]).toBe(stated);
            expect(printed.warn).not.toHaveBeenCalled();
        });

        it('should send no request for a run nested in a task', async ({ run, cache, printed }) => {
            const { address, requests } = await cache(Function.constant(Option.some(500)));
            vi.stubEnv(cached.server, address);
            vi.stubEnv('NX_TASK_TARGET_PROJECT', 'app');

            await preTasksExecution({ remoteCacheTimeout: '10 seconds' }, run);

            expect(requests).toStrictEqual([]);
            expect(env[cached.server]).toBe(address);
            expect(printed.warn).not.toHaveBeenCalled();
        });
    });

    describe('postTasksExecution', () => {
        it('should write the run and each task result with its hash inputs to <id>.json under taskResultsPath on a CI run', async ({ tempFs, run }) => {
            vi.stubEnv('CI', 'true');

            await postTasksExecution({ taskResultsPath: 'records/runs' }, run);

            expect(readJsonFile(join(tempFs.tempDir, 'records/runs/run-7f3a.json'))).toStrictEqual({
                id: 'run-7f3a',
                argv: ['node', 'nx', 'run-many', '--targets=build,check,test,e2e'],
                startTime: 1000,
                endTime: 9000,
                tasks: [
                    { id: 'app:build:release', project: 'app', target: 'build', configuration: 'release', hash: 'hash-app-build', status: 'failure', code: 2, startTime: 1100, endTime: 4100, hashDetails: { command: 'command-app-build', nodes: { 'app:{projectRoot}/**/*': 'node-1', 'env:CI': 'node-2' } } },
                    { id: 'app:test', project: 'app', target: 'test', hash: 'hash-app-test', status: 'failure', code: 1, startTime: 1200, endTime: 5200, hashDetails: { command: 'command-app-test', nodes: { 'app:{projectRoot}/**/*': 'node-1' } } },
                    { id: 'app:e2e', project: 'app', target: 'e2e', status: 'skipped', code: 1 },
                    { id: 'lib:build', project: 'lib', target: 'build', hash: 'hash-lib-build', status: 'remote-cache', code: 0, startTime: 1300, endTime: 1310, hashDetails: { command: 'command-lib-build', nodes: { 'lib:{projectRoot}/**/*': 'node-3' } } },
                    { id: 'lib:check', project: 'lib', target: 'check', hash: 'hash-lib-check', status: 'local-cache', code: 0, startTime: 1400, endTime: 1405, hashDetails: { command: 'command-lib-check', nodes: { 'lib:{projectRoot}/**/*': 'node-3' } } },
                    { id: 'lib:test', project: 'lib', target: 'test', hash: 'hash-lib-test', status: 'success', code: 0, startTime: 1500, endTime: 8500, hashDetails: { command: 'command-lib-test', nodes: { 'lib:{projectRoot}/**/*': 'node-3' } } },
                ],
            });
        });

        it.for([
            ['true', undefined],
            ['true', {}],
            ['false', { taskResultsPath: 'records/runs' }],
        ] as const)('should write no record where CI is %s and options are %o', async ([ci, options], { tempFs, run }) => {
            vi.stubEnv('CI', ci);

            await postTasksExecution(options, run);

            expect(globSync('**/*.json', { cwd: tempFs.tempDir })).toStrictEqual([]);
        });

        it('should write no record and print nothing for a run nested in a task', async ({ tempFs, run, printed }) => {
            vi.stubEnv('CI', 'true');
            vi.stubEnv('NX_TASK_TARGET_PROJECT', 'app');
            vi.stubEnv('GITHUB_ACTIONS', 'true');

            await postTasksExecution({ taskResultsPath: 'records/runs' }, run);

            expect(globSync('**/*.json', { cwd: tempFs.tempDir })).toStrictEqual([]);
            expect(printed.log).not.toHaveBeenCalled();
            expect(printed.warn).not.toHaveBeenCalled();
        });

        it.for([
            ['true', 'true', { taskResultsPath: 'records/runs' }, ['::error title=app:build:release::app:build:release exits 2, recorded in records/runs/run-7f3a.json', '::error title=app:test::app:test exits 1, recorded in records/runs/run-7f3a.json']],
            ['true', 'true', {}, ['::error title=app:build:release::app:build:release exits 2', '::error title=app:test::app:test exits 1']],
            ['false', 'true', { taskResultsPath: 'records/runs' }, ['::error title=app:build:release::app:build:release exits 2', '::error title=app:test::app:test exits 1']],
            ['true', undefined, { taskResultsPath: 'records/runs' }, []],
        ] as const)('should print one error annotation per failed task where CI is %s, GITHUB_ACTIONS is %s, and options are %o', async ([ci, actions, options, lines], { run, printed }) => {
            vi.stubEnv('CI', ci);
            vi.stubEnv('GITHUB_ACTIONS', actions);

            await postTasksExecution(options, run);

            expect(printed.log.mock.calls.flat().map(String).toSorted()).toStrictEqual(lines);
            expect(printed.warn).not.toHaveBeenCalled();
        });

        it.for([
            [undefined, /^Run run-7f3a writes no record to records\/runs\/run-7f3a\.json: \S/u, []],
            ['true', /^::warning::Run run-7f3a writes no record to records\/runs\/run-7f3a\.json: \S/u, ['::error title=app:build:release::app:build:release exits 2', '::error title=app:test::app:test exits 1']],
        ] as const)('should resolve with one warning and no record named where a file occupies the record folder and GITHUB_ACTIONS is %s', async ([actions, warning, lines], { tempFs, run, printed }) => {
            vi.stubEnv('CI', 'true');
            vi.stubEnv('GITHUB_ACTIONS', actions);
            await tempFs.createFiles({ records: '' });

            await expect(postTasksExecution({ taskResultsPath: 'records/runs' }, run)).resolves.toBeUndefined();

            expect(printed.warn.mock.calls).toStrictEqual([[expect.stringMatching(warning)]]);
            expect(printed.log.mock.calls.flat().map(String).toSorted()).toStrictEqual(lines);
        });
    });
});
