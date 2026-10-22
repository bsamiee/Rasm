import { basename, resolve } from 'node:path';
import { defineConfig } from '@playwright/test';
import { searchForWorkspaceRoot } from 'vite';

// --- [COMPOSITION] ---------------------------------------------------------------------

const config = (directory: string) => {
    const artifacts = (kind: string) => resolve(searchForWorkspaceRoot(import.meta.dirname), '.artifacts', kind, basename(directory));
    return defineConfig({
        outputDir: artifacts('playwright-test'),
        updateSnapshots: 'none',
        reporter: [['list'], ['html', { outputFolder: artifacts('playwright-report'), open: 'never' }]],
        use: { trace: { mode: 'retain-on-failure', snapshots: { dom: true, aria: true, screen: true } }, locale: 'en-US', timezoneId: 'UTC', reducedMotion: 'reduce' },
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { config };
