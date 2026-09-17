// --- [IMPORTS] -------------------------------------------------------------------------

import { HOSTS } from '@rasm/creative-cloud-server/values';
import type { UXP_Config, UXP_Manifest } from 'vite-uxp-plugin';

// --- [MANIFEST] ------------------------------------------------------------------------

const host: UXP_Manifest['host'][number] = { app: 'ID', minVersion: '21.6' };

const panel: Extract<UXP_Manifest['entrypoints'][number], { readonly label: { readonly default: string } }> = {
    type: 'panel',
    id: 'bridge',
    label: { default: 'Rasm' },
    minimumSize: { width: 200, height: 60 },
    preferredDockedSize: { width: 235, height: 80 },
};

const manifest: Omit<UXP_Manifest, 'host'> & { readonly host: UXP_Manifest['host'] | UXP_Manifest['host'][number] } = {
    manifestVersion: 5,
    id: 'rasm.indesign.bridge',
    name: 'Rasm InDesign Bridge',
    version: '0.1.7',
    main: 'plugin-main.js',
    host,
    entrypoints: [panel],
    icons: [{ width: 48, height: 48, path: 'icons/plugin.png', scale: [1, 2], theme: ['all'], species: ['pluginList'] }],
    requiredPermissions: { localFileSystem: 'fullAccess', network: { domains: [`ws://localhost:${HOSTS.indesign.port}`] }, allowCodeGenerationFromStrings: true },
};

const config: UXP_Config = { manifest: manifest as UXP_Manifest, hotReloadPort: 0, webviewUi: false, webviewReloadPort: 0, copyZipAssets: [] };

// --- [EXPORTS] -------------------------------------------------------------------------

export { config, host, manifest, panel };
