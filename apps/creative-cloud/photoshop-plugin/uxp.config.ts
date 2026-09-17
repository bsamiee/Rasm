// --- [IMPORTS] -------------------------------------------------------------------------

import { HOSTS } from '@rasm/creative-cloud-server/values';
import type { UXP_Config, UXP_Manifest } from 'vite-uxp-plugin';

// --- [MANIFEST] ------------------------------------------------------------------------

const endpoint: `ws://localhost:${number}` = `ws://localhost:${HOSTS.photoshop.port}`;

const host: Omit<UXP_Manifest['host'][number], 'data'> & { readonly data: { readonly apiVersion: 2; readonly loadEvent: 'startup' } } = {
    app: 'PS',
    minVersion: '27.11',
    data: { apiVersion: 2, loadEvent: 'startup' },
};

const panel: Extract<UXP_Manifest['entrypoints'][number], { readonly label: { readonly default: string } }> = {
    type: 'panel',
    id: 'bridge',
    label: { default: 'Rasm Bridge' },
    minimumSize: { width: 180, height: 80 },
    maximumSize: { width: 2000, height: 2000 },
    preferredDockedSize: { width: 235, height: 120 },
    preferredFloatingSize: { width: 235, height: 120 },
    icons: [
        { width: 23, height: 23, path: 'icons/dark.png', scale: [1, 2], theme: ['darkest', 'dark', 'medium'], species: ['generic'] },
        { width: 23, height: 23, path: 'icons/light.png', scale: [1, 2], theme: ['lightest', 'light'], species: ['generic'] },
    ],
};

const manifest: Omit<UXP_Manifest, 'host' | 'featureFlags'> & {
    readonly host: UXP_Manifest['host'] | typeof host;
    readonly featureFlags?: UXP_Manifest['featureFlags'] | { readonly uncaughtException: boolean; readonly unhandledRejection: boolean };
} = {
    manifestVersion: 5,
    id: 'rasm.photoshop.bridge',
    name: 'Rasm Photoshop Bridge',
    version: '1.0.0',
    main: 'plugin-main.js',
    host,
    entrypoints: [panel],
    icons: [{ width: 48, height: 48, path: 'icons/plugin.png', scale: [1, 2], theme: ['all'], species: ['pluginList'] }],
    requiredPermissions: { network: { domains: [endpoint] }, allowCodeGenerationFromStrings: true },
    featureFlags: { uncaughtException: true, unhandledRejection: true },
};

const config: UXP_Config = { manifest: manifest as UXP_Manifest, hotReloadPort: 0, webviewUi: false, webviewReloadPort: 0, copyZipAssets: [] };

// --- [EXPORTS] -------------------------------------------------------------------------

export { config, endpoint, host, manifest, panel };
