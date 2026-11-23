import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-docker';
import { refused } from '@rasm/testing/errors';
import { it, pathed } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const tools = (targets: object | undefined) =>
    Object.keys(targets ?? {})
        .filter((target) => target.includes('--'))
        .toSorted();

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('compose projects', () => {
    it.for(['compose.yaml', 'compose.yml', 'docker-compose.yaml', 'docker-compose.yml'])('infers compose targets for a project folder holding %s', async (file, { workspace }) => {
        const graph = await workspace.write({ 'proj/project.json': '{"name":"proj"}', [`proj/${file}`]: 'services:\n    app:\n        image: alpine:3\n' }).graph({ createNodes, createDependencies });

        expect(tools(graph.nodes['proj']?.data.targets)).toStrictEqual(['check--compose', 'install--compose']);
    });

    it('infers nothing from a YAML file Compose does not load by default', async ({ workspace }) => {
        const graph = await workspace.write({ 'proj/project.json': '{"name":"proj"}', 'proj/compose.base.yaml': 'services:\n    app:\n        image: alpine:3\n' }).graph({ createNodes, createDependencies });

        expect(tools(graph.nodes['proj']?.data.targets)).toStrictEqual([]);
    });

    it.for([
        { anchor: 'no manifest', files: {}, inferred: false },
        { anchor: 'a project.json', files: { 'deploy/project.json': '{"name":"deploy"}' }, inferred: true },
        { anchor: 'a package.json the workspaces cover', files: { 'package.json': '{"name":"root","workspaces":["deploy"]}', 'deploy/package.json': '{"name":"deploy"}' }, inferred: true },
        { anchor: 'a package.json outside the workspaces', files: { 'package.json': '{"name":"root","workspaces":["apps/*"]}', 'deploy/package.json': '{"name":"deploy"}' }, inferred: false },
    ])('anchors a Compose folder holding $anchor: $inferred', async ({ files, inferred }, { workspace }) => {
        const graph = await workspace.write({ ...files, 'deploy/compose.yaml': 'services:\n    app:\n        image: alpine:3\n' }).graph({ createNodes, createDependencies });

        expect(Object.values(graph.nodes).some(({ data }) => data.root === 'deploy' && tools(data.targets).includes('check--compose'))).toBe(inferred);
    });

    it.for([
        { services: 'images alone', compose: 'services:\n    app:\n        image: alpine:3\n', files: {}, targets: ['check--compose', 'install--compose'] },
        {
            services: 'a build beside the Compose file',
            compose: 'services:\n    app:\n        build: .\n',
            files: { 'proj/Dockerfile': 'FROM scratch\n' },
            targets: ['build--compose', 'check--buildx', 'check--compose', 'clean--compose', 'install--compose', 'serve--compose'],
        },
        {
            services: 'a build from a folder no project holds',
            compose: 'services:\n    app:\n        build:\n            context: ../images/app\n',
            files: { 'images/app/Dockerfile': 'FROM scratch\n' },
            targets: ['build--compose', 'check--buildx', 'check--compose', 'clean--compose', 'install--compose'],
        },
    ])('gives a Compose project of $services the tool targets $targets', async ({ compose, files, targets }, { workspace }) => {
        const graph = await workspace.write({ ...files, 'proj/project.json': '{"name":"proj"}', 'proj/compose.yaml': compose }).graph({ createNodes, createDependencies });

        expect(tools(graph.nodes['proj']?.data.targets)).toStrictEqual(targets);
        expect(Object.values(graph.nodes).map(({ data }) => data.root)).not.toContain('images/app');
    });

    it('gives the project holding the Dockerfile of a service its image build and start, run from the Compose folder', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'infra/project.json': '{"name":"infra"}',
                'infra/compose.yaml': 'services:\n    api:\n        build:\n            context: ../apps/api\n    worker:\n        build:\n            context: ../apps/worker\n',
                'apps/api/project.json': '{"name":"api"}',
                'apps/api/Dockerfile': 'FROM scratch\n',
                'apps/worker/Dockerfile': 'FROM scratch\n',
            })
            .graph({ createNodes, createDependencies });
        const { targets } = graph.nodes['api']?.data ?? {};

        expect(tools(targets)).toStrictEqual(['build--buildx', 'serve--compose']);
        expect([targets?.['build--buildx']?.options, targets?.['serve--compose']?.options]).toMatchObject([{ cwd: 'infra' }, { cwd: 'infra' }]);
        expect(targets?.['build--buildx']?.options).toHaveProperty('command', expect.stringMatching(/ build --with-dependencies api$/u));
    });
});

describe('inference refusals', () => {
    it('refuses a Compose file docker compose rejects, naming the field docker names', async ({ workspace }) => {
        const read = workspace.write({ 'proj/project.json': '{"name":"proj"}', 'proj/compose.yaml': 'services:\n    app:\n        image: alpine:3\n        ports: nonsense\n' }).graph({ createNodes, createDependencies });

        await expect(read).rejects.toSatisfy(refused(/^docker compose config .*exits \d+: .*services\.app\.ports must be a array/u));
    });

    it('refuses when no docker executable is on PATH', async ({ workspace }) => {
        pathed(join(workspace.root, 'bin'), {});

        const read = workspace.write({ 'proj/project.json': '{"name":"proj"}', 'proj/compose.yaml': 'services:\n    app:\n        image: alpine:3\n' }).graph({ createNodes, createDependencies });

        await expect(read).rejects.toSatisfy(refused(/^docker compose config .*cannot start: /u));
    });
});
