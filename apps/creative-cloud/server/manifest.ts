// --- [IMPORTS] -------------------------------------------------------------------------

import { Schema, String } from 'effect';
import { LOOPBACK, type SOCKETS, type SocketHost } from './values.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _BUILT = /\.ts$/u;
const _MANIFEST_VERSION = 5;
const _API_VERSIONS = [1, 2] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Size: Schema.Struct<{ readonly width: Schema.Number; readonly height: Schema.Number }> = Schema.Struct({ width: Schema.Number, height: Schema.Number });

const _optionalSize: Schema.optionalKey<typeof Size> = Schema.optionalKey(Size);

const Icon: Schema.Struct<{
    readonly width: Schema.Number;
    readonly height: Schema.Number;
    readonly path: Schema.String;
    readonly scale: Schema.optionalKey<Schema.$Array<Schema.Number>>;
    readonly theme: Schema.optionalKey<Schema.$Array<Schema.Literals<readonly ['all', 'lightest', 'light', 'medium', 'dark', 'darkest']>>>;
    readonly species: Schema.optionalKey<Schema.$Array<Schema.Literals<readonly ['generic', 'toolbar', 'pluginList']>>>;
}> = Schema.Struct({
    width: Schema.Number,
    height: Schema.Number,
    path: Schema.String,
    scale: Schema.optionalKey(Schema.Array(Schema.Number)),
    theme: Schema.optionalKey(Schema.Array(Schema.Literals(['all', 'lightest', 'light', 'medium', 'dark', 'darkest']))),
    species: Schema.optionalKey(Schema.Array(Schema.Literals(['generic', 'toolbar', 'pluginList']))),
});

const Panel: Schema.Struct<{
    readonly type: Schema.Literal<'panel'>;
    readonly id: Schema.String;
    readonly label: Schema.Struct<{ readonly default: Schema.String }>;
    readonly minimumSize: typeof Size;
    readonly maximumSize: typeof _optionalSize;
    readonly preferredDockedSize: typeof Size;
    readonly preferredFloatingSize: typeof _optionalSize;
    readonly icons: Schema.optionalKey<Schema.$Array<typeof Icon>>;
}> = Schema.Struct({
    type: Schema.Literal('panel'),
    id: Schema.String,
    label: Schema.Struct({ default: Schema.String }),
    minimumSize: Size,
    maximumSize: _optionalSize,
    preferredDockedSize: Size,
    preferredFloatingSize: _optionalSize,
    icons: Schema.optionalKey(Schema.Array(Icon)),
});

const Host: Schema.Struct<{
    readonly app: Schema.String;
    readonly data: Schema.optionalKey<
        Schema.Struct<{
            readonly apiVersion: Schema.optionalKey<Schema.Literals<typeof _API_VERSIONS>>;
            readonly loadEvent: Schema.optionalKey<Schema.Literals<readonly ['use', 'startup']>>;
            readonly enableMenuRecording: Schema.optionalKey<Schema.Boolean>;
        }>
    >;
}> = Schema.Struct({
    app: Schema.String,
    data: Schema.optionalKey(
        Schema.Struct({
            apiVersion: Schema.optionalKey(Schema.Literals(_API_VERSIONS)),
            loadEvent: Schema.optionalKey(Schema.Literals(['use', 'startup'])),
            enableMenuRecording: Schema.optionalKey(Schema.Boolean),
        }),
    ),
});

const Permissions: Schema.Struct<{
    readonly localFileSystem: Schema.optionalKey<Schema.Literals<readonly ['plugin', 'request', 'fullAccess']>>;
    readonly network: Schema.Struct<{ readonly domains: Schema.$Array<Schema.String> }>;
    readonly allowCodeGenerationFromStrings: Schema.Literal<true>;
}> = Schema.Struct({
    localFileSystem: Schema.optionalKey(Schema.Literals(['plugin', 'request', 'fullAccess'])),
    network: Schema.Struct({ domains: Schema.Array(Schema.String) }),
    allowCodeGenerationFromStrings: Schema.Literal(true),
});

const Manifest: Schema.Struct<{
    readonly manifestVersion: Schema.Literal<typeof _MANIFEST_VERSION>;
    readonly id: Schema.String;
    readonly name: Schema.String;
    readonly version: Schema.String;
    readonly main: Schema.String;
    readonly host: typeof Host;
    readonly entrypoints: Schema.Tuple<readonly [typeof Panel]>;
    readonly icons: Schema.$Array<typeof Icon>;
    readonly requiredPermissions: typeof Permissions;
    readonly featureFlags: Schema.Struct<{ readonly uncaughtException: Schema.Literal<true>; readonly unhandledRejection: Schema.Literal<true> }>;
}> = Schema.Struct({
    manifestVersion: Schema.Literal(_MANIFEST_VERSION),
    id: Schema.String,
    name: Schema.String,
    version: Schema.String,
    main: Schema.String,
    host: Host,
    entrypoints: Schema.Tuple([Panel]),
    icons: Schema.Array(Icon),
    requiredPermissions: Permissions,
    featureFlags: Schema.Struct({ uncaughtException: Schema.Literal(true), unhandledRejection: Schema.Literal(true) }),
});

const Placed: Schema.Struct<(typeof Manifest)['fields'] & { readonly host: Schema.Struct<(typeof Host)['fields'] & { readonly minVersion: Schema.String }> }> = Schema.Struct({
    ...Manifest.fields,
    host: Schema.Struct({ ...Host.fields, minVersion: Schema.String }),
});

const _ICON: (typeof Icon)['Type'] = { width: 24, height: 24, path: 'icons/plugin.png', scale: [1, 2], species: ['pluginList'] };

// --- [TYPES] ---------------------------------------------------------------------------

type Panel = (typeof Panel)['Type'];
type Manifest = (typeof Manifest)['Type'];
type Placed = (typeof Placed)['Type'];

interface Facts extends Pick<Manifest, 'id' | 'name'> {
    readonly panel: Omit<Panel, 'type' | 'id'>;
    readonly permissions: Pick<Manifest['requiredPermissions'], 'localFileSystem'>;
}

interface Bridge {
    readonly endpoint: `ws://${typeof LOOPBACK.dialed}:${number}`;
    readonly manifest: Manifest;
    readonly panel: Panel;
}

// --- [BUILDER] -------------------------------------------------------------------------

const plugin = (row: (typeof SOCKETS)[SocketHost], { main, version }: { readonly main: string; readonly version: string }, { panel: view, permissions, ...facts }: Facts): Bridge => {
    const endpoint = `ws://${LOOPBACK.dialed}:${row.port}` as const;
    const panel = Panel.make({ type: 'panel', id: 'bridge', ...view });
    return {
        endpoint,
        panel,
        manifest: Manifest.make({
            ...facts,
            version,
            manifestVersion: Manifest.fields.manifestVersion.literal,
            main: String.replace(_BUILT, '.js')(main),
            host: row.uxp,
            entrypoints: [panel],
            icons: [_ICON],
            requiredPermissions: { ...permissions, network: { domains: [endpoint] }, allowCodeGenerationFromStrings: true },
            featureFlags: { uncaughtException: true, unhandledRejection: true },
        }),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Bridge, Facts, Manifest, Panel };
export { Placed, plugin };
