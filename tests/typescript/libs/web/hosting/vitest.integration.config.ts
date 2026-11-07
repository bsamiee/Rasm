import { defaults } from '@rasm/test-support/vitest';
import { defineProject, mergeConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default mergeConfig(defaults(import.meta.filename), defineProject({ test: { include: ['s3.integration.test.ts'] } }));
