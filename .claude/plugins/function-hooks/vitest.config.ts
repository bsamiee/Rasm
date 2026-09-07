import type { ViteUserConfig } from 'vitest/config';
import { createVitestConfig } from '../../../vitest.config.ts';

const config: Promise<ViteUserConfig> = createVitestConfig(import.meta.dirname);

export default config;
