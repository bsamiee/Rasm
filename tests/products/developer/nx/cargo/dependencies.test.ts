import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { it } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('path dependency edges', { timeout: 120_000 }, () => {
    it('makes a path dependency between members a static edge, the root crate included', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'Cargo.toml': '[package]\nname = "root"\nedition = "2024"\n\n[workspace]\nmembers = ["clients", "api-checker/core"]\n\n[dependencies]\nclients = { path = "clients" }\n',
                'src/lib.rs': '',
                'clients/Cargo.toml': '[package]\nname = "clients"\nedition = "2024"\n',
                'clients/src/lib.rs': '',
                'api-checker/core/Cargo.toml': '[package]\nname = "api-checker-core"\nedition = "2024"\n\n[dependencies]\nclients = { path = "../../clients" }\n',
                'api-checker/core/src/lib.rs': '',
            })
            .graph(cargo);

        expect([...(graph.dependencies['root'] ?? []), ...(graph.dependencies['api_checker_core'] ?? [])]).toStrictEqual([
            { source: 'root', target: 'clients', type: 'static' },
            { source: 'api_checker_core', target: 'clients', type: 'static' },
        ]);
    });
});
