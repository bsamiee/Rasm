import { homedir, tmpdir, userInfo } from 'node:os';
import type { ProjectNodesWithFiles } from '@nx/conformance/testing';
import type { TargetConfiguration } from '@nx/devkit';
import rule from '@rasm/conformance/tasks';
import { Effect } from 'effect';
import { describe, expect, it, onTestFinished, vi } from 'vitest';
import { project, reported, violations } from '#/workspace';

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = ({ options = {}, ...target }: Omit<TargetConfiguration<Readonly<Record<string, string>>>, 'executor'> = {}): TargetConfiguration => ({ executor: 'nx:run-commands', ...target, options: { command: 'true', ...options } });

const cleaned = (name: string, targets: Readonly<Record<string, TargetConfiguration>>, files: readonly string[] = []) => project(name, { targets: { clean: { executor: 'nx:noop' }, ...targets } }, files);

const checked = (...projects: readonly ProjectNodesWithFiles[]) => Effect.runPromise(violations(rule, { clean: 'clean' }, { projects, files: { '.gitignore': '.artifacts/\ndist/\n' } }));

const stubbed = (variable: string, value: string) => {
    vi.stubEnv(variable, value);
    onTestFinished(() => {
        vi.unstubAllEnvs();
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('tasks', () => {
    it('accepts ignored cached outputs apart under every configuration, a clean target, and machine paths only in projects caching nothing', async () => {
        stubbed('RASM_CONFORMANCE_MODE', 'ci');
        const lib = cleaned(
            'lib',
            {
                'build--tsc': run({ cache: true, inputs: ['{projectRoot}/**/*', { env: 'RASM_CONFORMANCE_MODE' }], outputs: ['{workspaceRoot}/.artifacts/tsc/{projectName}'], options: { cwd: '{projectRoot}' }, configurations: { release: {} } }),
                'pack--pnpm': run({ cache: true, outputs: ['{projectRoot}/{options.out}'], options: { out: 'dist' }, configurations: { preview: { out: 'dist/preview' } }, dependsOn: ['build--tsc'] }),
            },
            ['src/index.ts'],
        );
        const host = project('host', { targets: { install: run({ outputs: ['{projectRoot}/dist'], options: { command: `cp -R dist ${homedir()}/Applications` } }), clean: run() } });
        await expect(checked(lib, host)).resolves.toStrictEqual([]);
    });

    it('reports a task cycle and a cached task depending on clean', async () => {
        const rows = ['workspace | Task cycle lib:', 'lib | lib:build--tsc is cached and depends on a clean task'];
        expect(reported(await checked(cleaned('lib', { first: run({ dependsOn: ['second'] }), second: run({ dependsOn: ['first'] }), 'build--tsc': run({ cache: true, dependsOn: ['clean'] }) })), rows)).toStrictEqual(rows);
    });

    it('reports cached outputs git tracks under a configuration or holding another cached output, an output outside the workspace, and outputs in a project without clean', async () => {
        const rows = [
            'lib | lib:build--tsc:inplace writes libs/lib/src, which git does not ignore',
            'lib | lib:build--tsc.outputs.1 names {workspaceRoot}/../../escaped outside the workspace',
            'lib | lib:check--tsc writes .artifacts/tsc/lib, which holds .artifacts/tsc/lib/packed of lib:pack--tsc',
            'bare | bare declares outputs and no clean target',
        ];
        const lib = cleaned(
            'lib',
            {
                'build--tsc': run({ cache: true, outputs: ['{projectRoot}/{options.out}', '{workspaceRoot}/../../escaped'], options: { out: 'dist' }, configurations: { inplace: { out: 'src' } } }),
                'check--tsc': run({ cache: true, outputs: ['{workspaceRoot}/.artifacts/tsc/lib'] }),
                'pack--tsc': run({ cache: true, outputs: ['{workspaceRoot}/.artifacts/tsc/lib/packed'] }),
            },
            ['src/index.ts'],
        );
        expect(reported(await checked(lib, project('bare', { targets: { 'build--tsc': run({ outputs: ['{projectRoot}/dist'] }) } })), rows)).toStrictEqual(rows);
    });

    it('reports a machine path, user name, or timestamp anywhere in a project caching a target, a path outside the workspace in a cached target, and a hashed variable naming this machine', async () => {
        stubbed('RASM_CONFORMANCE_HOME', homedir());
        const rows = [
            'lib | lib:build--tsc.inputs.0 names {workspaceRoot}/../shared/** outside the workspace',
            `lib | lib:build--tsc.outputs.0 holds {workspaceRoot}/.artifacts/${userInfo().username}, which names this machine`,
            `lib | lib:build--tsc.options.command holds node ${tmpdir()}/build.js, which names this machine`,
            `lib | lib:build--tsc.options.command names ${tmpdir()}/build.js outside the workspace`,
            'lib | lib:build--tsc.configurations.stamped.command holds echo 2026-11-15T10:31:00',
            `lib | lib:serve.options.command holds node ${homedir()}/serve.js`,
            `lib | lib:build--tsc hashes RASM_CONFORMANCE_HOME=${homedir()}, which names this machine`,
        ];
        const lib = cleaned('lib', {
            'build--tsc': run({ cache: true, inputs: ['{workspaceRoot}/../shared/**', { env: 'RASM_CONFORMANCE_HOME' }], outputs: [`{workspaceRoot}/.artifacts/${userInfo().username}`], options: { command: `node ${tmpdir()}/build.js` }, configurations: { stamped: { command: 'echo 2026-11-15T10:31:00' } } }),
            serve: run({ options: { command: `node ${homedir()}/serve.js` } }),
        });
        expect(reported(await checked(lib), rows)).toStrictEqual(rows);
    });
});
