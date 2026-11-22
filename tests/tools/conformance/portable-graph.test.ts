import { homedir, tmpdir, userInfo } from 'node:os';
import type { TargetConfiguration } from '@nx/devkit';
import rule from '@rasm/conformance/portable-graph';
import { Effect } from 'effect';
import { describe, expect, it } from 'vitest';
import { project, violations } from '#/workspace.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const checked = (targets: Readonly<Record<string, TargetConfiguration>>) => Effect.runPromise(violations(rule, undefined, { projects: [project('lib', { targets })] }));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('portable-graph', () => {
    it('accepts cached targets stating paths relative to the workspace and project roots, and a fixed path outside the workspace in a target no cache holds', async () => {
        await expect(
            checked({
                build: { executor: 'nx:run-commands', cache: true, inputs: ['{projectRoot}/**/*'], outputs: ['{workspaceRoot}/.artifacts/tsc/lib'], options: { command: 'tsc -p ../../tsconfig.base.json', cwd: '{projectRoot}' } },
                install: { executor: 'nx:run-commands', options: { command: 'cp -R dist /Applications' } },
            }),
        ).resolves.toStrictEqual([]);
    });

    it('reports a machine path in a target no cache holds of a project caching another target, as Nx hashes every target of a project into each of its tasks', async () => {
        await expect(
            checked({
                build: { executor: 'nx:run-commands', cache: true, inputs: ['{projectRoot}/**/*'], options: { command: 'tsc', cwd: '{projectRoot}' } },
                serve: { executor: 'nx:run-commands', options: { command: `node ${homedir()}/serve.js` } },
            }),
        ).resolves.toStrictEqual([{ sourceProject: 'lib', message: `lib:serve.options.command holds node ${homedir()}/serve.js, which names this machine or this run, state it relative to {workspaceRoot} or {projectRoot}` }]);
    });

    it('accepts a machine path in a project caching no target', async () => {
        await expect(checked({ serve: { executor: 'nx:run-commands', options: { command: `node ${homedir()}/serve.js` } } })).resolves.toStrictEqual([]);
    });

    it('reports a temporary folder, a user name segment, a timestamp, and a path outside the workspace in a cached target', async () => {
        await expect(
            checked({
                build: { executor: 'nx:run-commands', cache: true, inputs: ['{workspaceRoot}/../shared/**'], outputs: [`{workspaceRoot}/.artifacts/${userInfo().username}`], options: { command: `node ${tmpdir()}/build.js` }, configurations: { stamped: { command: 'echo 2026-11-15T10:31:00' } } },
            }),
        ).resolves.toStrictEqual([
            { sourceProject: 'lib', message: 'lib:build.inputs.0 names {workspaceRoot}/../shared/** outside the workspace, which no other runner holds, move it inside' },
            { sourceProject: 'lib', message: `lib:build.outputs.0 holds {workspaceRoot}/.artifacts/${userInfo().username}, which names this machine or this run, state it relative to {workspaceRoot} or {projectRoot}` },
            { sourceProject: 'lib', message: `lib:build.options.command holds node ${tmpdir()}/build.js, which names this machine or this run, state it relative to {workspaceRoot} or {projectRoot}` },
            { sourceProject: 'lib', message: `lib:build.options.command names ${tmpdir()}/build.js outside the workspace, which no other runner holds, move it inside` },
            { sourceProject: 'lib', message: 'lib:build.configurations.stamped.command holds echo 2026-11-15T10:31:00, which names this machine or this run, state it relative to {workspaceRoot} or {projectRoot}' },
        ]);
    });
});
