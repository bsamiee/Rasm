import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-typescript';
import { execute } from '@rasm/nx-typescript/executors/upgrade';
import { it, shimmed } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const PNPM = { 'package.json': '{"name":"root"}', 'pnpm-lock.yaml': '' };
const MANIFEST = { pinned: '{"name":"root","dependencies":{"@react-aria/live-announcer":"3.5.0"}}', moved: '{"name":"root","dependencies":{"@react-aria/live-announcer":"3.5.1"}}' };
const ANNOUNCER = {
    tags: { latest: '3.5.1', nightly: '3.0.0-nightly-2691598d3-261008' },
    time: { '3.5.0': '2026-04-14T22:43:40.020Z', '3.5.1': '2026-05-28T17:28:41.548Z', '3.0.0-nightly-2691598d3-261008': '2026-10-08T09:09:59.668Z' },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const answering = (root: string, ...managers: readonly string[]) => Object.fromEntries(managers.map((manager) => [manager, `case "$1" in\n    view) cat "${join(root, 'registry')}/$2.json" ;;\n    -*) ;;\n    *) echo "${manager} $*" >> "${join(root, 'ran.txt')}" ;;\nesac`]));

const published = (name: string, { tags, time }: { readonly tags: Readonly<Record<string, string>>; readonly time: Readonly<Record<string, string>> }) => ({
    [`registry/${name}.json`]: JSON.stringify({ 'dist-tags': tags, time }),
    ...Object.fromEntries(Object.values(tags).map((version) => [`registry/${name}@${version}.json`, JSON.stringify({ version })])),
});

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
    ])('holds $case: $name $pin', async ({ name, pin, tags, time }, { workspace }) => {
        shimmed(join(workspace.root, '.bin'), answering(workspace.root, 'pnpm'));
        const catalog = `packages: []\ncatalog:\n    ${name}: ${pin}\n`;
        const graph = await workspace.write({ ...PNPM, 'pnpm-workspace.yaml': catalog, ...published(name, { tags, time }) }).graph({ createNodes });

        const { success } = await execute({ catalogs: ['pnpm-workspace.yaml'], supported: {} }, workspace.context(graph, 'root', 'upgrade--pnpm'));

        expect([success, readFileSync(join(workspace.root, 'pnpm-workspace.yaml'), 'utf-8'), readFileSync(join(workspace.root, 'ran.txt'), 'utf-8').trim().split('\n')]).toStrictEqual([true, catalog, ['pnpm store prune']]);
    });
});

describe('upgrade lock step', () => {
    it.for([
        {
            manager: 'pnpm',
            files: { ...PNPM, 'pnpm-workspace.yaml': "packages: []\ncatalog:\n    '@react-aria/live-announcer': 3.5.0\n" },
            catalogs: ['pnpm-workspace.yaml'],
            file: 'pnpm-workspace.yaml',
            text: "packages: []\ncatalog:\n    '@react-aria/live-announcer': 3.5.1\n",
            ran: ['pnpm dedupe', 'pnpm store prune'],
        },
        { manager: 'npm', files: { 'package.json': MANIFEST.pinned, 'package-lock.json': '' }, catalogs: [], file: 'package.json', text: MANIFEST.moved, ran: ['npm install', 'npm dedupe', 'npm cache verify'] },
        { manager: 'yarn', files: { 'package.json': MANIFEST.pinned, 'yarn.lock': '' }, catalogs: [], file: 'package.json', text: MANIFEST.moved, ran: ['yarn install', 'yarn dedupe', 'yarn cache clean'] },
        { manager: 'bun', files: { 'package.json': MANIFEST.pinned, 'bun.lock': '' }, catalogs: [], file: 'package.json', text: MANIFEST.moved, ran: ['bun install', 'bun dedupe', 'bun pm cache rm'] },
    ])('moves a stable pin to the newer stable head, then installs and deduplicates the $manager lock', async ({ manager, files, catalogs, file, text, ran }, { workspace }) => {
        shimmed(join(workspace.root, '.bin'), answering(workspace.root, manager, 'npm'));
        const graph = await workspace.write({ ...files, ...published('@react-aria/live-announcer', ANNOUNCER) }).graph({ createNodes });

        const { success } = await execute({ catalogs, supported: {} }, workspace.context(graph, 'root', `upgrade--${manager}`));

        expect([success, readFileSync(join(workspace.root, file), 'utf-8'), readFileSync(join(workspace.root, 'ran.txt'), 'utf-8').trim().split('\n')]).toStrictEqual([true, text, ran]);
    });
});
