import { createReadOnlyTree, createStubbedProjectGraphAndFileMapCache } from '@nx/conformance/testing';
import rule from '@rasm/conformance/one-version';
import { Record } from 'effect';
import { describe, expect, it, onTestFinished } from 'vitest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const checked = async (installed: Record.ReadonlyRecord<string, string>) => {
    const { tree, cleanup } = await createReadOnlyTree();
    onTestFinished(cleanup);
    const { projectGraph, fileMapCache } = await createStubbedProjectGraphAndFileMapCache(tree, []);
    const externalNodes = Record.mapEntries(installed, (version, key) => [`npm:${key}`, { type: 'npm' as const, name: `npm:${key}` as const, data: { packageName: key.replace(/@[^@/]+$/u, ''), version } }]);
    const { details } = await rule.implementation({ tree, projectGraph: { ...projectGraph, externalNodes }, fileMapCache, ruleOptions: { packages: ['nx', '@nx/*'] } });
    return details.violations;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('one-version', () => {
    it('accepts every matched package at one version and ignores the rest', async () => {
        await expect(checked({ nx: '24.1.0', '@nx/js': '24.1.0', '@nx/devkit': '24.1.0', effect: '4.1.4' })).resolves.toStrictEqual([]);
    });

    it('reports each version with the packages installed at it', async () => {
        await expect(checked({ nx: '24.1.0', '@nx/js': '24.1.0', '@nx/js@24.0.6': '24.0.6', '@nx/devkit': '24.0.6' })).resolves.toStrictEqual([{ workspaceViolation: true, message: 'Installs 24.1.0 (nx, @nx/js); 24.0.6 (@nx/js, @nx/devkit), move each to one version in the catalog and lock' }]);
    });
});
