import type { Rgb } from 'culori/fn';
import { Array, Record, Result, Schema, SemVer, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTS = {
    ID: { product: 'IDSN', pluginsInfo: true, opacity: 0.65 },
    PS: { product: 'PHSP', pluginsInfo: true, opacity: 0.8 },
    aftereffects: { product: 'AEFT', pluginsInfo: true, opacity: 0.65 },
    premierepro: { product: 'PPRO', pluginsInfo: true, opacity: 0.65 },
    ame: { product: 'AME', pluginsInfo: true, opacity: 0.65 },
    AI: { product: 'ILST', pluginsInfo: false, opacity: 0.65 },
} as const;
const THEMES = { lightest: 'light', light: 'light', medium: 'dark', dark: 'dark', darkest: 'dark' } as const;
const ICON_SIZES = { toolbar: 23, pluginList: 24 } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Version = Schema.SemVerFromString.check(Schema.makeFilter((version) => !(SemVer.isPrerelease(version) || SemVer.hasBuild(version)), { description: 'major.minor.patch' }));
const Flag = Schema.optionalKey(Schema.Boolean);
const Size = Schema.optionalKey(Schema.Struct({ width: Schema.Finite, height: Schema.Finite }));
const Side = Schema.Int.check(Schema.isGreaterThan(0));
const Localized = Schema.Union([Schema.NonEmptyString, Schema.StructWithRest(Schema.Struct({ default: Schema.NonEmptyString }), [Schema.Record(Schema.String, Schema.NonEmptyString)])]);
const Icon = Schema.Struct({
    width: Side,
    height: Side,
    path: Schema.String,
    scale: Schema.Finite.pipe(Schema.Array, Schema.optionalKey),
    theme: Schema.Literals([...Struct.keys(THEMES), 'all']).pipe(Schema.Array, Schema.optionalKey),
    species: Schema.Literals(['generic', ...Struct.keys(ICON_SIZES)]).pipe(Schema.Array, Schema.optionalKey),
}).check(
    Schema.makeFilter(({ width, height }) => width === height || `one side for width and height, received ${String(width)} x ${String(height)}`),
    Schema.makeFilter(({ width, species = [] }) => {
        const refused = Array.filterMap(species, (kind) => (kind === 'generic' || ICON_SIZES[kind] === width ? Result.failVoid : Result.succeed(`${kind} takes ${ICON_SIZES[kind]}`)));
        return Array.isReadonlyArrayEmpty(refused) || `species ${refused.join(', ')} at 1x, received ${String(width)}`;
    }),
);
const Entrypoint = Schema.Struct({
    id: Schema.NonEmptyString,
    label: Localized,
    description: Schema.optionalKey(Localized),
    hostUIContext: Schema.optionalKey(Schema.Struct(Struct.Record(['hideFromMenu', 'hideFromPluginsPanel', 'shouldReanimateAsFloatingPanel', 'allowViewPersistence'], Flag))),
});

class UxpManifest extends Schema.Class<UxpManifest, { readonly brand: unique symbol }>('UxpManifest')(
    Schema.Struct({
        manifestVersion: Schema.Literals([5, 6]),
        id: Schema.NonEmptyString,
        name: Schema.NonEmptyString,
        version: Version,
        main: Schema.optionalKey(Schema.String),
        host: Schema.Struct({
            app: Schema.Literals(Struct.keys(HOSTS)),
            minVersion: Version,
            maxVersion: Schema.optionalKey(Version),
            data: Schema.optionalKey(Schema.Struct({ apiVersion: Schema.optionalKey(Schema.Literals([1, 2])), loadEvent: Schema.optionalKey(Schema.Literals(['use', 'startup'])), enableMenuRecording: Flag })),
        }),
        icons: Icon.pipe(Schema.Array, Schema.optionalKey),
        strings: Schema.optionalKey(Schema.Union([Schema.String, Schema.Record(Schema.String, Localized)])),
        addon: Schema.optionalKey(Schema.Struct({ name: Schema.NonEmptyString })),
        entrypoints: Schema.Array(
            Schema.Union([
                Entrypoint.pipe(Schema.fieldsAssign({ type: Schema.Literal('command'), shortcut: Schema.optionalKey(Schema.Struct({ mac: Schema.optionalKey(Schema.String), win: Schema.optionalKey(Schema.String) })) })),
                Entrypoint.pipe(Schema.fieldsAssign({ type: Schema.Literal('panel'), minimumSize: Size, maximumSize: Size, preferredDockedSize: Size, preferredFloatingSize: Size, allowViewPersistence: Flag, icons: Icon.pipe(Schema.Array, Schema.optionalKey) })),
            ]),
        ),
        requiredPermissions: Schema.Struct({
            localFileSystem: Schema.optionalKey(Schema.Literals(['plugin', 'request', 'fullAccess'])),
            network: Schema.optionalKey(Schema.Struct({ domains: Schema.Union([Schema.NonEmptyArray(Schema.String), Schema.Literal('all')]) })),
            clipboard: Schema.optionalKey(Schema.Literals(['read', 'readAndWrite'])),
            launchProcess: Schema.optionalKey(Schema.Struct({ schemes: Schema.String.pipe(Schema.NonEmptyArray, Schema.optionalKey), extensions: Schema.String.pipe(Schema.NonEmptyArray, Schema.optionalKey) })),
            ipc: Schema.optionalKey(Schema.Struct({ enablePluginCommunication: Schema.Boolean, enableHostCommunication: Flag })),
            webview: Schema.optionalKey(
                Schema.Struct({
                    allow: Schema.Literal('yes'),
                    domains: Schema.Union([Schema.NonEmptyArray(Schema.String), Schema.Literal('all')]),
                    enableMessageBridge: Schema.optionalKey(Schema.Literals(['localAndRemote', 'localOnly', 'no'])),
                    allowLocalRendering: Schema.optionalKey(Schema.Literal('yes')),
                    allowAutomation: Flag,
                }),
            ),
            fonts: Schema.optionalKey(Schema.Literal('readInstalled')),
            allowCodeGenerationFromStrings: Flag,
            enableUserInfo: Flag,
            enableAddon: Flag,
        }),
        featureFlags: Schema.optionalKey(Schema.Struct(Struct.Record(['unhandledRejection', 'uncaughtException', 'enableFillAsCustomAttribute', 'enableSWCSupport', 'enableAlerts', 'allowFocusOnClickForPanels', 'enableResizeObserver', 'enableUpdatedResizeBehavior'], Flag))),
    }).check(Schema.makeFilter(({ addon, manifestVersion, requiredPermissions: { enableAddon } }) => addon === undefined || (manifestVersion === 6 && enableAddon === true) || `addon needs manifestVersion 6 and requiredPermissions.enableAddon, received ${String(manifestVersion)} and ${String(enableAddon)}`)),
) {
    static readonly Theme = Schema.Literals(Struct.keys(THEMES));
    static readonly fills = Record.map(HOSTS, ({ opacity }) => Record.map(THEMES, (tone): Rgb => ({ mode: 'rgb', ...Struct.Record(['r', 'g', 'b'], tone === 'light' ? 0 : 1), alpha: opacity })));
    static readonly json = Schema.fromJsonString(UxpManifest);
    static readonly Identity = Schema.fromJsonString(Schema.Struct({ id: UxpManifest.fields.id, host: UxpManifest.fields.host.mapFields(Struct.pick(['app'])), requiredPermissions: UxpManifest.fields.requiredPermissions.mapFields(Struct.pick(['network'])) })).annotate({
        description: 'Plugin id, host app, and permitted network domains, the manifest fields a running plugin reads',
    });
    static readonly file = 'manifest.json';
    static readonly engine = { chrome: '118' } as const;
    static readonly origin = 'file://';
}

const PluginsInfo = Object.assign(
    Schema.fromJsonString(Schema.Struct({ plugins: Schema.Array(Schema.StructWithRest(Schema.Struct({ pluginId: UxpManifest.fields.id, path: Schema.optionalKey(Schema.String) }), [Schema.Record(Schema.String, Schema.Unknown)])) })).annotate({
        description: 'PluginsInfo registry a UXP host reads from its machine, then its per-user, Adobe folder, one row per installed plugin',
    }),
    { file: (app: UxpManifest['host']['app']) => ['UXP', 'PluginsInfo', 'v1', `${app}.json`] as const },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { HOSTS, ICON_SIZES, PluginsInfo, THEMES, UxpManifest };
