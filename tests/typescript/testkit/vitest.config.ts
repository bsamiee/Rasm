import { defineProject } from 'vitest/config';
import { createProject } from '../../../vitest.config.ts';

export default defineProject(createProject(import.meta.dirname));
