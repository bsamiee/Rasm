// --- [IMPORTS] -------------------------------------------------------------------------

import { String } from 'effect';
import type { Row } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Size {
    readonly width: number;
    readonly height: number;
}

interface Icon {
    readonly width: number;
    readonly height: number;
    readonly path: string;
    readonly scale?: readonly number[];
    readonly theme?: readonly ('all' | 'lightest' | 'light' | 'medium' | 'dark' | 'darkest')[];
    readonly species?: readonly ('generic' | 'toolbar' | 'pluginList')[];
}

interface Panel {
    readonly type: 'panel';
    readonly id: string;
    readonly label: { readonly default: string };
    readonly minimumSize: Size;
    readonly maximumSize?: Size;
    readonly preferredDockedSize: Size;
    readonly preferredFloatingSize?: Size;
    readonly icons?: readonly Icon[];
}

interface Host {
    readonly app: string;
    readonly minVersion: string;
    readonly data?: { readonly apiVersion?: 1 | 2; readonly loadEvent?: 'use' | 'startup'; readonly enableMenuRecording?: boolean };
}

interface Manifest {
    readonly manifestVersion: 5;
    readonly id: string;
    readonly name: string;
    readonly version: string;
    readonly main: string;
    readonly host: Host;
    readonly entrypoints: readonly [Panel];
    readonly icons: readonly Icon[];
    readonly requiredPermissions: {
        readonly localFileSystem?: 'plugin' | 'request' | 'fullAccess';
        readonly network: { readonly domains: readonly string[] };
        readonly allowCodeGenerationFromStrings: true;
    };
    readonly featureFlags: { readonly uncaughtException: true; readonly unhandledRejection: true };
}

interface Package {
    readonly main: string;
    readonly version: string;
}

interface Facts extends Pick<Manifest, 'id' | 'name'> {
    readonly panel: Omit<Panel, 'type' | 'id'>;
    readonly permissions: Pick<Manifest['requiredPermissions'], 'localFileSystem'>;
}

interface Bridge {
    readonly endpoint: `ws://localhost:${number}`;
    readonly manifest: Manifest;
    readonly panel: Panel;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ICON: Icon = { width: 24, height: 24, path: 'icons/plugin.png', scale: [1, 2], species: ['pluginList'] };
const _built = String.replace(/\.ts$/u, '.js');

// --- [BUILDER] -------------------------------------------------------------------------

const plugin = (row: Extract<Row, { readonly uxp: Host }>, { main, version }: Package, { panel: view, permissions, ...facts }: Facts): Bridge => {
    const endpoint = `ws://localhost:${row.port}` as const;
    const panel: Panel = { type: 'panel', id: 'bridge', ...view };
    return {
        endpoint,
        panel,
        manifest: {
            ...facts,
            version,
            manifestVersion: 5,
            main: _built(main),
            host: row.uxp,
            entrypoints: [panel],
            icons: [_ICON],
            requiredPermissions: { ...permissions, network: { domains: [endpoint] }, allowCodeGenerationFromStrings: true },
            featureFlags: { uncaughtException: true, unhandledRejection: true },
        },
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Bridge, Facts, Manifest, Panel };
export { plugin };
