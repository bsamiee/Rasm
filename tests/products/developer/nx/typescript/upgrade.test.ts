import { existsSync, readFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-typescript';
import { execute } from '@rasm/nx-typescript/executors/upgrade';
import { it, shimmed } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PNPM = { 'package.json': '{"name":"root"}', 'pnpm-lock.yaml': '' };
const MANIFEST = { pinned: '{"name":"root","dependencies":{"@react-aria/live-announcer":"3.5.0"}}', moved: '{"name":"root","dependencies":{"@react-aria/live-announcer":"3.5.1"}}' };
const CATALOG = { pinned: "packages: []\ncatalog:\n    '@react-aria/live-announcer': 3.5.0\n", moved: "packages: []\ncatalog:\n    '@react-aria/live-announcer': 3.5.1\n" };
const ANNOUNCER = {
    tags: { latest: '3.5.1', nightly: '3.0.0-nightly-2691598d3-261008' },
    time: { '3.5.0': '2026-04-14T22:43:40.020Z', '3.5.1': '2026-05-28T17:28:41.548Z', '3.0.0-nightly-2691598d3-261008': '2026-10-08T09:09:59.668Z' },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const answering = (root: string, store: string, ...managers: readonly string[]) =>
    Object.fromEntries(
        managers.map((manager) => [manager, `case "$1" in\n    view) cat "${join(root, 'registry')}/$2.json" ;;\n    -*) ;;\n    store) echo "${manager} $*" >> "${join(root, 'ran.txt')}"; if [ "$2" = path ]; then echo "${store}"; fi ;;\n    *) echo "${manager} $*" >> "${join(root, 'ran.txt')}" ;;\nesac`]),
    );

const published = (name: string, { tags, time }: { readonly tags: Readonly<Record<string, string>>; readonly time: Readonly<Record<string, string>> }) => ({
    [`registry/${name}.json`]: JSON.stringify({ 'dist-tags': tags, time }),
    ...Object.fromEntries(Object.values(tags).map((version) => [`registry/${name}@${version}.json`, JSON.stringify({ version })])),
});

const ran = (root: string) => (existsSync(join(root, 'ran.txt')) ? readFileSync(join(root, 'ran.txt'), 'utf-8').trim().split('\n') : []);

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('upgrade pins', () => {
    it.for([
        {
            case: 'a stable pin against a prerelease head of a higher release published before it',
            name: 'react-aria-components',
            pin: '1.22.1',
            tags: { latest: '1.22.1', nightly: '3.0.0-nightly-2691598d3-261008' },
            time: { '1.22.1': '2026-10-09T20:23:49.939Z', '3.0.0-nightly-2691598d3-261008': '2026-10-08T09:09:47.605Z' },
        },
        {
            case: 'a prerelease pin against a stable head of a lower release published after it',
            name: 'storybook',
            pin: '11.0.0-beta.3',
            tags: { latest: '10.6.8', next: '11.0.0-beta.3' },
            time: { '10.6.8': '2026-11-20T08:21:41.941Z', '11.0.0-beta.3': '2026-11-13T15:44:40.887Z' },
        },
    ])('holds $case, touching neither the catalog nor the lockfile: $name $pin', async ({ name, pin, tags, time }, { workspace }) => {
        shimmed(join(workspace.root, '.bin'), answering(workspace.root, tmpdir(), 'pnpm'));
        const catalog = `packages: []\ncatalog:\n    ${name}: ${pin}\n`;
        const graph = await workspace.write({ ...PNPM, 'pnpm-workspace.yaml': catalog, ...published(name, { tags, time }) }).graph({ createNodes });

        const { success } = await execute({ supported: {}, ...graph.nodes['root']?.data.targets?.['upgrade--pnpm']?.options }, workspace.context(graph, 'root', 'upgrade--pnpm'));

        expect([success, readFileSync(join(workspace.root, 'pnpm-workspace.yaml'), 'utf-8'), ran(workspace.root)]).toStrictEqual([true, catalog, []]);
    });
});

describe('upgrade lock and store steps', () => {
    it.for([
        { case: 'pnpm keeping its store outside the workspace', manager: 'pnpm', files: { ...PNPM, 'pnpm-workspace.yaml': CATALOG.pinned }, file: 'pnpm-workspace.yaml', text: CATALOG.moved, store: tmpdir(), ran: ['pnpm dedupe', 'pnpm store path', 'pnpm store prune'] },
        { case: 'pnpm keeping its store inside the workspace', manager: 'pnpm', files: { ...PNPM, 'pnpm-workspace.yaml': CATALOG.pinned }, file: 'pnpm-workspace.yaml', text: CATALOG.moved, store: '<root>/.cache/pnpm/store/v11', ran: ['pnpm dedupe', 'pnpm store path'] },
        { case: 'npm', manager: 'npm', files: { 'package.json': MANIFEST.pinned, 'package-lock.json': '' }, file: 'package.json', text: MANIFEST.moved, store: '', ran: ['npm install', 'npm dedupe'] },
        { case: 'yarn', manager: 'yarn', files: { 'package.json': MANIFEST.pinned, 'yarn.lock': '' }, file: 'package.json', text: MANIFEST.moved, store: '', ran: ['yarn install', 'yarn dedupe'] },
        { case: 'bun', manager: 'bun', files: { 'package.json': MANIFEST.pinned, 'bun.lock': '' }, file: 'package.json', text: MANIFEST.moved, store: '', ran: ['bun install', 'bun dedupe'] },
    ])('moves a stable pin to the newer stable head and relocks, pruning only packages no registered workspace uses, with $case', async ({ manager, files, file, text, store, ran: commands }, { workspace }) => {
        shimmed(join(workspace.root, '.bin'), answering(workspace.root, store.replace('<root>', workspace.root), manager, 'npm'));
        const graph = await workspace.write({ ...files, ...published('@react-aria/live-announcer', ANNOUNCER) }).graph({ createNodes });

        const { success } = await execute({ supported: {}, ...graph.nodes['root']?.data.targets?.[`upgrade--${manager}`]?.options }, workspace.context(graph, 'root', `upgrade--${manager}`));

        expect([success, readFileSync(join(workspace.root, file), 'utf-8'), ran(workspace.root)]).toStrictEqual([true, text, commands]);
    });
});
