/// <reference types="vite/client" />
/**
 * Root Vite anchor: executes the factory for dev/typecheck parity, then drops the build block —
 * the root emits NO artifact. Apps/packages import vite.factory.ts and own their build outputs.
 */

import { Effect } from 'effect';
import { defineConfig, type UserConfig } from 'vite';
import { createConfig } from './vite.factory.ts';

// --- [EXPORTS] ---------------------------------------------------------------

const { build: _build, ...anchor } = Effect.runSync(
    createConfig({
        entry: './vite.factory.ts',
        mode: 'library',
        name: 'WorkspaceFoundation',
    }),
);
const config: UserConfig = defineConfig(anchor);

export default config;
