import { join } from 'node:path';
import { defaults } from '@rasm/test-support/vitest';
import { searchForWorkspaceRoot } from 'vite';
import { defineProject, mergeConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default mergeConfig(defaults(import.meta.filename), defineProject({ test: { include: ['postgres.integration.test.ts'], provide: { squawkConfig: join(searchForWorkspaceRoot(import.meta.dirname), '.config', 'squawk.toml') } } }));
