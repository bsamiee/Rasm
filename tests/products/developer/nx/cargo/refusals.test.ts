import { existsSync } from 'node:fs';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { refused } from '@rasm/testing/errors';
import { it, pathed, refusing } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };
const crates = {
    'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    'crates/sound/Cargo.toml': '[package]\nname = "sound"\nedition = "2024"\n',
    'crates/sound/src/lib.rs': '',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('cargo refusals', { timeout: 120_000 }, () => {
    it.for(refusing('cargo'))('refuses the manifest with the command and $case of cargo', async ({ scripts, refusal }, { workspace }) => {
        pathed(join(workspace.root, '.bin'), scripts);

        await expect(workspace.write(crates).graph(cargo)).rejects.toSatisfy(refused(refusal));
    });

    it('refuses with the member manifest cargo cannot parse, naming its file and line', async ({ workspace }) => {
        const pending = workspace.write({ ...crates, 'crates/broken/Cargo.toml': '[package\nname = ' }).graph(cargo);

        await expect(pending).rejects.toSatisfy(refused(/^cargo metadata --format-version 1 --no-deps --manifest-path Cargo\.toml exits 101: error: unclosed table, expected `\]`\n --> crates\/broken\/Cargo\.toml:1:9/u));
    });

    it('reads a manifest under a folder named with shell syntax without running it', async ({ workspace }) => {
        const graph = await workspace.write({ 'x; touch pwned/Cargo.toml': '[package]\nname = "literal"\nedition = "2024"\n', 'x; touch pwned/src/lib.rs': '' }).graph(cargo);

        expect(graph.nodes['literal']?.data.root).toBe('x; touch pwned');
        expect(existsSync(join(workspace.root, 'pwned'))).toBe(false);
    });
});
