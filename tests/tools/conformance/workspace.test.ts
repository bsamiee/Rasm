import type { ProjectNodesWithFiles } from '@nx/conformance/testing';
import rule from '@rasm/conformance/workspace';
import { Effect, Record } from 'effect';
import { describe, expect, it } from 'vitest';
import { project, reported, violations } from '#/workspace';

// --- [OPERATIONS] ----------------------------------------------------------------------

const installed = (packages: Record.ReadonlyRecord<string, string>) => Record.mapEntries(packages, (version, key) => [`npm:${key}`, { type: 'npm' as const, name: `npm:${key}` as const, data: { packageName: key.replace(/@[^@/]+$/u, ''), version } }]);

const checked = (nxJson: Readonly<Record<string, unknown>>, packages: Record.ReadonlyRecord<string, string>, ...projects: readonly ProjectNodesWithFiles[]) => Effect.runPromise(violations(rule, undefined, { nxJson, externalNodes: installed(packages), projects }));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('workspace', () => {
    it('accepts valid settings, nx and its migration group at one version beside packages outside it, and release configuration on exactly the selected projects', async () => {
        await expect(
            checked(
                { parallel: 4, release: { groups: { libraries: { projects: ['tag:release'] }, tools: { projects: 'cli' } } } },
                { nx: '24.1.0', '@nx/devkit': '24.1.0', '@nx/conformance': '24.1.0', '@nx/key': '5.0.0', 'nx-cloud': '19.1.0', effect: '4.1.4' },
                project('lib', { tags: ['release'], release: {} }),
                project('cli', { release: {} }),
                project('app', { tags: ['os:darwin'] }),
            ),
        ).resolves.toStrictEqual([]);
    });

    it('reports settings outside the schema against nx.json and every version the migration group splits into', async () => {
        const rows = ['nx.json | nx.json ', 'workspace | Installs nx and its migration group at 24.1.0 (nx, @nx/devkit); 24.0.6 (@nx/devkit); 23.0.0 (create-nx-workspace),'];
        expect(reported(await checked({ parallel: 'many' }, { nx: '24.1.0', '@nx/devkit': '24.1.0', '@nx/devkit@24.0.6': '24.0.6', 'create-nx-workspace': '23.0.0', '@nx/key': '5.0.0' }), rows)).toStrictEqual(rows);
    });

    it('reports a selected project without release configuration and release configuration outside the selection', async () => {
        const rows = ['lib | ', 'app | '];
        expect(reported(await checked({ release: { projects: ['tag:release'] } }, {}, project('lib', { tags: ['release'] }), project('app', { release: {} }), project('cli', { tags: ['release'], release: {} })), rows)).toStrictEqual(rows);
    });

    it('selects every project when release names neither projects nor groups, as nx release does', async () => {
        const rows = ['app | '];
        expect(reported(await checked({ release: {} }, {}, project('lib', { release: {} }), project('app')), rows)).toStrictEqual(rows);
    });
});
