import { Version } from '@rasm/automation';
import { Record, Schema, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSION = { current: 5, addon: 6 } as const;
const THEMES = { lightest: 'light', light: 'light', medium: 'dark', dark: 'dark', darkest: 'dark' } as const;
const INKS = Record.map({ light: { color: '#000000', opacity: 0.8 }, dark: { color: '#FFFFFF', opacity: 0.8 } } as const, (ink, name) => ({ ...ink, theme: Struct.keys(THEMES).filter((theme) => THEMES[theme] === name) }));

// --- [MODELS] --------------------------------------------------------------------------

const Flag = Schema.optionalKey(Schema.Boolean);
const Size = Schema.optionalKey(Schema.Struct({ width: Schema.Finite, height: Schema.Finite }));
const Localized = Schema.Union([Schema.String, Schema.StructWithRest(Schema.Struct({ default: Schema.String }), [Schema.Record(Schema.String, Schema.String)])]);
const Icon = Schema.Struct({
    width: Schema.Finite,
    height: Schema.Finite,
    path: Schema.String,
    scale: Schema.optionalKey(Schema.Array(Schema.Finite)),
    theme: Schema.optionalKey(Schema.Array(Schema.Literals([...Struct.keys(THEMES), 'all']))),
    species: Schema.optionalKey(Schema.Array(Schema.Literals(['generic', 'toolbar', 'pluginList']))),
});
const Entrypoint = Schema.Struct({
    id: Schema.String,
    label: Localized,
    description: Schema.optionalKey(Localized),
    hostUIContext: Schema.optionalKey(Schema.Struct(Struct.Record(['hideFromMenu', 'hideFromPluginsPanel', 'shouldReanimateAsFloatingPanel', 'allowViewPersistence'], Flag))),
});

class UxpManifest extends Schema.Class<UxpManifest, { readonly brand: unique symbol }>('UxpManifest')(
    Schema.Struct({
        manifestVersion: Schema.Literals([VERSION.current, VERSION.addon]),
        id: Schema.String,
        name: Schema.String,
        version: Version,
        main: Schema.optionalKey(Schema.String),
        host: Schema.Struct({
            app: Schema.Literals(['ID', 'PS', 'premierepro', 'ame', 'aftereffects']),
            minVersion: Version,
            maxVersion: Schema.optionalKey(Version),
            data: Schema.optionalKey(Schema.Struct({ apiVersion: Schema.optionalKey(Schema.Literals([1, 2])), loadEvent: Schema.optionalKey(Schema.Literals(['use', 'startup'])), enableMenuRecording: Flag })),
        }),
        icons: Schema.optionalKey(Schema.Array(Icon)),
        strings: Schema.optionalKey(Schema.Union([Schema.String, Schema.Record(Schema.String, Localized)])),
        addon: Schema.optionalKey(Schema.Struct({ name: Schema.String })),
        entrypoints: Schema.Array(
            Schema.Union([
                Entrypoint.pipe(Schema.fieldsAssign({ type: Schema.Literal('command'), shortcut: Schema.optionalKey(Schema.Struct({ mac: Schema.optionalKey(Schema.String), win: Schema.optionalKey(Schema.String) })) })),
                Entrypoint.pipe(Schema.fieldsAssign({ type: Schema.Literal('panel'), minimumSize: Size, maximumSize: Size, preferredDockedSize: Size, preferredFloatingSize: Size, allowViewPersistence: Flag, icons: Schema.optionalKey(Schema.Array(Icon)) })),
            ]),
        ),
        requiredPermissions: Schema.Struct({
            localFileSystem: Schema.optionalKey(Schema.Literals(['plugin', 'request', 'fullAccess'])),
            network: Schema.optionalKey(Schema.Struct({ domains: Schema.Union([Schema.NonEmptyArray(Schema.String), Schema.Literal('all')]) })),
            clipboard: Schema.optionalKey(Schema.Literals(['read', 'readAndWrite'])),
            launchProcess: Schema.optionalKey(Schema.Struct({ schemes: Schema.optionalKey(Schema.NonEmptyArray(Schema.String)), extensions: Schema.optionalKey(Schema.NonEmptyArray(Schema.String)) })),
            ipc: Schema.optionalKey(Schema.Struct({ enablePluginCommunication: Schema.Boolean })),
            webview: Schema.optionalKey(
                Schema.Struct({
                    allow: Schema.Literal('yes'),
                    domains: Schema.Union([Schema.NonEmptyArray(Schema.String), Schema.Literal('all')]),
                    enableMessageBridge: Schema.optionalKey(Schema.Literals(['localAndRemote', 'localOnly', 'no'])),
                    allowLocalRendering: Schema.optionalKey(Schema.Literal('yes')),
                    allowAutomation: Flag,
                }),
            ),
            allowCodeGenerationFromStrings: Flag,
            enableUserInfo: Flag,
            enableAddon: Flag,
        }),
        featureFlags: Schema.optionalKey(Schema.Struct(Struct.Record(['unhandledRejection', 'uncaughtException', 'enableFillAsCustomAttribute', 'enableSWCSupport', 'enableAlerts', 'allowFocusOnClickForPanels', 'enableResizeObserver', 'enableUpdatedResizeBehavior'], Flag))),
    }).check(
        Schema.makeFilter(
            ({ addon, manifestVersion, requiredPermissions: { enableAddon } }) => addon === undefined || (manifestVersion === VERSION.addon && enableAddon === true) || `addon needs manifestVersion ${VERSION.addon} and requiredPermissions.enableAddon, received ${String(manifestVersion)} and ${String(enableAddon)}`,
        ),
    ),
) {
    static readonly version = VERSION;
    static readonly Theme = Schema.Literals(Struct.keys(THEMES));
    static readonly themes = THEMES;
    static readonly inks = INKS;
    static readonly json = Schema.fromJsonString(UxpManifest);
    static readonly file = 'manifest.json';
    static readonly target = 'es2022';
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { UxpManifest };
