import type { Rgb } from 'culori/fn';
import { Array, Record, Result, Schema, SemVer, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST_VERSIONS = { current: 5, addon: 6 } as const;
const HOSTS = {
    ID: { product: 'IDSN', since: SemVer.make({ major: 20, minor: 0, patch: 0 }), registry: true, runs: true, opacity: 0.65 },
    PS: { product: 'PHSP', since: SemVer.make({ major: 27, minor: 11, patch: 0 }), registry: true, runs: true, opacity: 0.8 },
    aftereffects: { product: 'AEFT', since: SemVer.make({ major: 27, minor: 1, patch: 0 }), registry: true, runs: true, opacity: 0.65 },
    premierepro: { product: 'PPRO', since: SemVer.make({ major: 27, minor: 1, patch: 0 }), registry: true, runs: true, opacity: 0.65 },
    ame: { product: 'AME', since: SemVer.make({ major: 27, minor: 0, patch: 0 }), registry: true, runs: true, opacity: 0.65 },
    AI: { product: 'ILST', since: SemVer.make({ major: 31, minor: 1, patch: 0 }), registry: false, runs: false, opacity: 0.65 },
} as const;
const THEMES = { lightest: 'light', light: 'light', medium: 'dark', dark: 'dark', darkest: 'dark' } as const;
const SIDES = { toolbar: 23, pluginList: 24 } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Version = Schema.SemVerFromString.check(Schema.makeFilter((version) => !(SemVer.isPrerelease(version) || SemVer.hasBuild(version)), { description: 'major.minor.patch' }));
const Flag = Schema.optionalKey(Schema.Boolean);
const Size = Schema.optionalKey(Schema.Struct({ width: Schema.Finite, height: Schema.Finite }));
const App = Schema.Literals(Struct.keys(HOSTS));
const Localized = Schema.Union([Schema.String, Schema.StructWithRest(Schema.Struct({ default: Schema.String }), [Schema.Record(Schema.String, Schema.String)])]);
const Icon = Schema.Struct({
    width: Schema.Finite,
    height: Schema.Finite,
    path: Schema.String,
    scale: Schema.optionalKey(Schema.Array(Schema.Finite)),
    theme: Schema.optionalKey(Schema.Array(Schema.Literals([...Struct.keys(THEMES), 'all']))),
    species: Schema.optionalKey(Schema.Array(Schema.Literals(['generic', ...Struct.keys(SIDES)]))),
}).check(
    Schema.makeFilter(({ width, height, species = [] }) => {
        const refused = Array.filterMap(species, (kind) => (kind === 'generic' || (SIDES[kind] === width && SIDES[kind] === height) ? Result.failVoid : Result.succeed(`${kind} takes ${SIDES[kind]} x ${SIDES[kind]}`)));
        return Array.isReadonlyArrayEmpty(refused) || `species ${refused.join(', ')} at 1x, received ${String(width)} x ${String(height)}`;
    }),
);
const Entrypoint = Schema.Struct({
    id: Schema.String,
    label: Localized,
    description: Schema.optionalKey(Localized),
    hostUIContext: Schema.optionalKey(Schema.Struct(Struct.Record(['hideFromMenu', 'hideFromPluginsPanel', 'shouldReanimateAsFloatingPanel', 'allowViewPersistence'], Flag))),
});

class UxpManifest extends Schema.Class<UxpManifest, { readonly brand: unique symbol }>('UxpManifest')(
    Schema.Struct({
        manifestVersion: Schema.Literals([MANIFEST_VERSIONS.current, MANIFEST_VERSIONS.addon]),
        id: Schema.String,
        name: Schema.String,
        version: Version,
        main: Schema.optionalKey(Schema.String),
        host: Schema.Struct({
            app: App,
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
            ({ addon, manifestVersion, requiredPermissions: { enableAddon } }) =>
                addon === undefined || (manifestVersion === MANIFEST_VERSIONS.addon && enableAddon === true) || `addon needs manifestVersion ${MANIFEST_VERSIONS.addon} and requiredPermissions.enableAddon, received ${String(manifestVersion)} and ${String(enableAddon)}`,
        ),
    ),
) {
    static readonly Theme = Schema.Literals(Struct.keys(THEMES));
    static readonly fills = Record.map(HOSTS, ({ opacity }) => Record.map(THEMES, (tone): Rgb => ({ mode: 'rgb', ...Struct.Record(['r', 'g', 'b'], tone === 'light' ? 0 : 1), alpha: opacity })));
    static readonly json = Schema.fromJsonString(UxpManifest);
    static readonly identity = Schema.fromJsonString(Schema.Struct({ id: Schema.String, host: Schema.Struct({ app: App }) })).annotate({ description: 'Plugin id and host app, the manifest fields a running plugin reads' });
    static readonly file = 'manifest.json';
    static readonly target = 'es2022';
    static readonly scriptLanguage = 'USLg';
    static readonly script = Object.assign(
        (body: string, { runtime, host }: { readonly runtime: string; readonly host: { readonly module: string; readonly around: string } }) =>
            [
                `(() => { const { script } = require('uxp'); const answer = (reply) => script.setResult(encodeURIComponent(reply)); try {`,
                `const around = ${host.around}; const hosted = require(${JSON.stringify(host.module)}); const prepare = Error.prepareStackTrace; const lines = [];`,
                runtime,
                `Error.prepareStackTrace = (error, frames) => { lines.push(...frames.filter((frame) => frame.getScriptNameOrSourceURL() === 'body.js').map((frame) => frame.getLineNumber())); return String(error); };`,
                `try { answer(runtime.reply(() => around(hosted, () => { let value; try { value = new Function(${JSON.stringify(`return ${body}\n//# sourceURL=body.js`)})()(); } catch (error) { void error?.stack; throw { message: String(error?.message ?? error), line: lines[0] }; }`,
                `if (typeof value?.then === 'function') { value.then(undefined, () => undefined); throw { _tag: 'IllegalArgument', message: 'body returned a promise, a do script body runs synchronously' }; } return { result: value }; })));`,
                `} finally { Error.prepareStackTrace = prepare; } } catch (error) { answer(JSON.stringify({ _tag: 'Thrown', message: String(error?.message ?? error) })); } })();`,
            ].join('\n'),
        { offset: 2 },
    );
    static readonly origin = 'file://';
    static readonly link = new URL('ws://localhost:7490');
    static readonly logs = (folder: string) => ({ folder: `Library/Logs/Adobe/${folder}`, glob: 'UXPLogs_*.log' });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { HOSTS, MANIFEST_VERSIONS, THEMES, UxpManifest };
