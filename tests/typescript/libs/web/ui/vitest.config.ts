import { defaults } from '@rasm/test-support/vitest';
import { react } from '@rasm/ui/vite';
import { defineProject, mergeConfig } from 'vitest/config';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default mergeConfig(defaults(import.meta.filename), defineProject({ plugins: [react()], test: { browser: { enabled: true } } }));
