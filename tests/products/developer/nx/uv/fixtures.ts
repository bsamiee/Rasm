import { mkdirSync } from 'node:fs';
import { join } from 'node:path';
import { it as workspaced } from '@rasm/testing/nx';
import { vi } from 'vitest';

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = workspaced.extend('temporary', { auto: true }, ({ workspace }, { onCleanup }) => {
    const folder = join(workspace.root, 'tmp', 'temporary');
    mkdirSync(folder, { recursive: true });
    vi.stubEnv('TMPDIR', folder);
    onCleanup(() => {
        vi.unstubAllEnvs();
    });
    return folder;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { it };
