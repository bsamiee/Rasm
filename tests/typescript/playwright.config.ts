import { globSync } from 'node:fs';
import { join } from 'node:path';
import { defineConfig } from '@playwright/test';
import { searchForWorkspaceRoot } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ARTIFACTS = join(searchForWorkspaceRoot(import.meta.dirname), '.artifacts', 'typescript', 'playwright');

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    updateSnapshots: 'none',
    reporter: [['list'], ['html', { outputFolder: join(_ARTIFACTS, 'report'), open: 'never' }]],
    use: { trace: { mode: 'retain-on-failure', snapshots: { dom: true, aria: true, screen: true } }, locale: 'en-US', timezoneId: 'UTC', reducedMotion: 'reduce' },
    projects: globSync('e2e-*', { cwd: import.meta.dirname }).map((name) => ({ name, testDir: join(import.meta.dirname, name), outputDir: join(_ARTIFACTS, name) })),
});
