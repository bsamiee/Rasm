import { homedir, userInfo } from 'node:os';
import { createReadOnlyTree, createStubbedProjectGraphAndFileMapCache } from '@nx/conformance/testing';
import type { TargetConfiguration } from '@nx/devkit';
import rule from '@rasm/conformance/portable-graph';
import { describe, expect, it, onTestFinished } from 'vitest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const checked = async (targets: Readonly<Record<string, TargetConfiguration>>) => {
    const { tree, cleanup } = await createReadOnlyTree();
    onTestFinished(cleanup);
    const { projectGraph, fileMapCache } = await createStubbedProjectGraphAndFileMapCache(tree, [{ projectNode: { name: 'lib', type: 'lib', data: { root: 'libs/lib', targets } } }]);
    const { details } = await rule.implementation({ tree, projectGraph, fileMapCache, ruleOptions: undefined });
    return details.violations.map(({ message }) => message);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('portable-graph', () => {
    it('accepts cached targets stating paths relative to the workspace and project roots, and a machine path in a target no cache holds', async () => {
        await expect(
            checked({
                build: { executor: 'nx:run-commands', cache: true, inputs: ['{projectRoot}/**/*'], outputs: ['{workspaceRoot}/.artifacts/tsc/lib'], options: { command: 'tsc -p ../../tsconfig.base.json', cwd: '{projectRoot}' } },
                serve: { executor: 'nx:run-commands', continuous: true, readyWhen: { command: `node ${homedir()}/ready.js` } },
            }),
        ).resolves.toStrictEqual([]);
    });

    it('reports a home folder, a user name segment, a timestamp, and a path outside the workspace in a cached target', async () => {
        await expect(
            checked({
                build: { executor: 'nx:run-commands', cache: true, inputs: ['{workspaceRoot}/../shared/**'], outputs: [`{workspaceRoot}/.artifacts/${userInfo().username}`], options: { command: `node ${homedir()}/build.js` }, configurations: { stamped: { command: 'echo 2026-11-15T10:31:00' } } },
            }),
        ).resolves.toStrictEqual([
            'lib:build.inputs.0 names {workspaceRoot}/../shared/** outside the workspace, which no other runner holds, move it inside',
            `lib:build.outputs.0 holds {workspaceRoot}/.artifacts/${userInfo().username}, which names this machine or this run, state it relative to {workspaceRoot} or {projectRoot}`,
            `lib:build.options.command holds node ${homedir()}/build.js, which names this machine or this run, state it relative to {workspaceRoot} or {projectRoot}`,
            `lib:build.options.command names ${homedir()}/build.js outside the workspace, which no other runner holds, move it inside`,
            'lib:build.configurations.stamped.command holds echo 2026-11-15T10:31:00, which names this machine or this run, state it relative to {workspaceRoot} or {projectRoot}',
        ]);
    });
});
