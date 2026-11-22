import { execFileSync, execSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { env } from 'node:process';
import { createDependencies, createNodes } from '@rasm/nx-docker';
import { it, shimmed } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('compose validation', () => {
    it.for([
        { case: 'a required variable nothing sets', files: { 'proj/compose.yaml': `services:\n    app:\n        image: "alpine:\${TAG:?TAG unset}"\n` }, success: false },
        { case: 'a required variable the project .env sets', files: { 'proj/compose.yaml': `services:\n    app:\n        image: "alpine:\${TAG:?TAG unset}"\n`, 'proj/.env': 'TAG=3\n' }, success: true },
        { case: 'a missing env file of a service under a profile', files: { 'proj/compose.yaml': 'services:\n    app:\n        image: alpine:3\n    debugger:\n        image: alpine:3\n        profiles: [debug]\n        env_file: debug.env\n' }, success: false },
        { case: 'a present env file of a service under a profile', files: { 'proj/compose.yaml': 'services:\n    app:\n        image: alpine:3\n    debugger:\n        image: alpine:3\n        profiles: [debug]\n        env_file: debug.env\n', 'proj/debug.env': 'DEBUG=1\n' }, success: true },
    ])('check--compose over $case succeeds: $success', async ({ files, success }, { workspace }) => {
        const graph = await workspace.write({ ...files, 'proj/project.json': '{"name":"proj"}' }).graph({ createNodes, createDependencies });

        await expect(workspace.run(graph, 'proj', 'check--compose')).resolves.toMatchObject({ success });
    });

    it.for([
        { case: 'a Dockerfile breaking a build check', dockerfile: 'FROM scratch as base\nCOPY compose.yaml /compose.yaml\n', success: false },
        { case: 'a Dockerfile passing every build check', dockerfile: 'FROM scratch AS base\nCOPY compose.yaml /compose.yaml\n', success: true },
    ])('check--buildx over $case succeeds: $success', { timeout: 60_000 }, async ({ dockerfile, success }, { workspace }) => {
        const graph = await workspace.write({ 'proj/project.json': '{"name":"proj"}', 'proj/compose.yaml': 'services:\n    app:\n        build: .\n', 'proj/Dockerfile': dockerfile }).graph({ createNodes, createDependencies });

        await expect(workspace.run(graph, 'proj', 'check--buildx')).resolves.toMatchObject({ success });
    });
});

describe('image build', () => {
    it('build--compose builds the image of each service into the engine', { timeout: 120_000 }, async ({ workspace, onTestFinished }) => {
        const graph = await workspace.write({ 'nxdockerbuilt/project.json': '{"name":"built"}', 'nxdockerbuilt/compose.yaml': 'services:\n    app:\n        build: .\n', 'nxdockerbuilt/Dockerfile': 'FROM scratch\nCOPY compose.yaml /compose.yaml\n' }).graph({ createNodes, createDependencies });
        onTestFinished(() => {
            execFileSync('docker', ['image', 'rm', '--force', 'nxdockerbuilt-app'], { stdio: 'ignore' });
        });

        const { success } = await workspace.run(graph, 'built', 'build--compose');

        expect(success).toBe(true);
        expect(execFileSync('docker', ['image', 'inspect', '--format', '{{.Id}}', 'nxdockerbuilt-app'], { encoding: 'utf-8' })).toMatch(/^sha256:/u);
    });
});

describe('service start readiness', () => {
    it('serve--compose turns ready once docker compose ps lists its own service running, polled from the Compose folder', { timeout: 60_000 }, async ({ workspace }) => {
        const graph = await workspace
            .write({
                'infra/project.json': '{"name":"infra"}',
                'infra/compose.yaml': 'services:\n    api:\n        build:\n            context: ../apps/api\n    worker:\n        build:\n            context: ../apps/worker\n',
                'apps/api/project.json': '{"name":"api"}',
                'apps/api/Dockerfile': 'FROM scratch\n',
                'apps/worker/project.json': '{"name":"worker"}',
                'apps/worker/Dockerfile': 'FROM scratch\n',
            })
            .graph({ createNodes, createDependencies });
        workspace.install(import.meta.resolve('@rasm/nx-docker/package.json'));
        shimmed(join(workspace.root, 'bin'), { docker: `echo "$PWD $*" >> ${join(workspace.root, 'calls')}\n[ "$(wc -l < ${join(workspace.root, 'calls')})" -ge 2 ] && echo '{"Service":"api","State":"running","Health":"healthy"}'\nexit 0` });
        const serve = graph.nodes['api']?.data.targets?.['serve--compose'];
        const ready = serve?.readyWhen ?? {};

        execSync('command' in ready && typeof ready.command === 'string' ? ready.command : 'false', { cwd: workspace.root, env: { ...env, NX_TASK_OPTIONS: JSON.stringify(serve?.options) }, stdio: 'pipe' });

        expect(readFileSync(join(workspace.root, 'calls'), 'utf-8')).toBe(`${join(workspace.root, 'infra')} compose ps --format json api\n`.repeat(2));
    });
});
