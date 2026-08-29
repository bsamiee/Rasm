import { defineProject } from 'vitest/config';
import { createVitestProject } from '../../../vitest.config.ts';

export default defineProject(createVitestProject(import.meta.dirname));
