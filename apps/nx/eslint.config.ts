import nx from '@nx/eslint-plugin';
import { defineConfig } from 'eslint/config';
import { parseForESLint } from 'jsonc-eslint-parser';

// --- [ESLINT] --------------------------------------------------------------------------

export default defineConfig(nx.configs['flat/base'], { files: ['**/package.json', '**/executors.json', '**/generators.json', '**/migrations.json'], languageOptions: { parser: { parseForESLint } }, rules: { '@nx/nx-plugin-checks': 'error' } });
