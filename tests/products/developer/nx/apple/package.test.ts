import type { ExecutorContext } from '@nx/devkit';
import { execute } from '@rasm/nx-apple/executors/package';
import { it } from '@rasm/testing/nx';
import { describe, expect } from 'vitest';

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('package options', { timeout: 60_000 }, () => {
    it.for([
        { stated: 'neither archives nor products', options: {} },
        { stated: 'no archives', options: { products: [] } },
        { stated: 'no products', options: { exports: [] } },
    ])('packs nothing and succeeds where the target states $stated', async ({ options }, { workspace }) => {
        const context: ExecutorContext = { root: workspace.root, cwd: workspace.root, isVerbose: false, projectGraph: { nodes: {}, dependencies: {} }, projectsConfigurations: { version: 2, projects: {} }, nxJsonConfiguration: {} };

        await expect(execute(options, context)).resolves.toStrictEqual({ success: true });
    });
});
