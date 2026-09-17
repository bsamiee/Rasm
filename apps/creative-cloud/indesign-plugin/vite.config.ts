// --- [IMPORTS] -------------------------------------------------------------------------

import { defaultClientConditions, defaultClientMainFields, defineConfig, type UserConfig } from 'vite';
import { uxp } from 'vite-uxp-plugin';
import { config } from './uxp.config.ts';

// --- [CONFIGURATION] -------------------------------------------------------------------

const userConfig: UserConfig = defineConfig({
    plugins: [uxp(config)],
    resolve: {
        alias: [{ find: /^adobe:/u, replacement: '' }],
        conditions: defaultClientConditions.filter((condition) => condition !== 'browser'),
        mainFields: defaultClientMainFields.filter((field) => field !== 'browser'),
    },
    build: {
        target: 'es2022',
        outDir: '../../../.artifacts/apps/creative-cloud/indesign-plugin',
        emptyOutDir: true,
        rolldownOptions: {
            input: 'plugin-main.ts',
            external: ['uxp', 'indesign'],
            output: { format: 'cjs', codeSplitting: false, entryFileNames: 'plugin-main.js' },
        },
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export default userConfig;
