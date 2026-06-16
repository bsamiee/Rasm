/// <reference types="vite/client" />
/**
 * Root Vite configuration: imports factory and executes for workspace root.
 * Apps/packages import from vite.factory.ts to avoid triggering this execution.
 */

import { Effect } from 'effect';
import { defineConfig, type UserConfig } from 'vite';
import { createConfig } from './vite.factory.ts';

// --- [EXPORTS] ---------------------------------------------------------------

const config: UserConfig = defineConfig(
    Effect.runSync(
        createConfig({
            entry: './vite.factory.ts',
            external: [
                '@rolldown/plugin-babel',
                '@tailwindcss/vite',
                '@vitejs/plugin-react',
                'effect',
                'rollup-plugin-visualizer',
                'vite',
                'vite-plugin-compression',
                'vite-plugin-csp',
                'vite-plugin-image-optimizer',
                'vite-plugin-inspect',
                'vite-plugin-pwa',
                'vite-plugin-svgr',
                'vite-plugin-webfont-dl',
            ],
            mode: 'library',
            name: 'WorkspaceFoundation',
        }),
    ),
);

export default config;
