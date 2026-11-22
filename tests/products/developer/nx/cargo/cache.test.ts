import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { it, pathed } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };
const crates = {
    'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    'crates/kit/Cargo.toml': '[package]\nname = "kit"\nversion = "0.1.0"\nedition = "2024"\n',
    'crates/kit/src/lib.rs': 'pub fn two() -> u32 {\n    2\n}\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('stored readings', { timeout: 120_000 }, () => {
    it('serves the stored reading while no manifest or target source changes, with cargo gone from PATH', async ({ workspace }) => {
        await workspace.write(crates).graph(cargo);
        pathed(join(workspace.root, '.bin'), {});

        const graph = await workspace.graph(cargo);

        expect(graph.nodes['kit']?.data.targets).toHaveProperty('build--cargo');
    });

    it('reads the crate again after a manifest changes', async ({ workspace }) => {
        const before = await workspace.write(crates).graph(cargo);
        workspace.write({ 'crates/kit/Cargo.toml': '[package]\nname = "kit"\nversion = "0.1.0"\nedition = "2024"\npublish = false\n' });

        const after = await workspace.graph(cargo);

        expect(before.nodes['kit']?.data.targets).toHaveProperty('nx-release-publish');
        expect(after.nodes['kit']?.data.targets).not.toHaveProperty('nx-release-publish');
    });

    it('reads the crate again after a binary source joins the crate layout, no manifest changing', async ({ workspace }) => {
        const before = await workspace.write(crates).graph(cargo);
        workspace.write({ 'crates/kit/src/bin/report.rs': 'fn main() {\n    println!("{}", kit::two());\n}\n' });

        const after = await workspace.graph(cargo);

        expect(before.nodes['kit']?.data.targets).not.toHaveProperty('install--cargo');
        expect(after.nodes['kit']?.data.targets).toHaveProperty('install--cargo');
    });
});
