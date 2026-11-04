import { Effect, Schema, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSION = 5;

// --- [MODELS] --------------------------------------------------------------------------

const Version = Schema.String.check(Schema.isPattern(/^\d+(?:\.\d+){0,2}$/u)).annotate({ description: 'major[.minor[.patch]]' });
const Flag = Schema.optionalKey(Schema.Boolean);
const Size = Schema.optionalKey(Schema.Struct({ width: Schema.Finite, height: Schema.Finite }));
const Icon = Schema.Struct({ width: Schema.Finite, height: Schema.Finite, path: Schema.String, scale: Schema.Array(Schema.Finite), theme: Schema.optionalKey(Schema.Array(Schema.Literals(['lightest', 'light', 'medium', 'dark', 'darkest', 'all']))), species: Schema.optionalKey(Schema.Array(Schema.String)) });
const Entrypoint = Schema.Struct({ id: Schema.String, label: Schema.Union([Schema.String, Schema.StructWithRest(Schema.Struct({ default: Schema.String }), [Schema.Record(Schema.String, Schema.String)])]) });

class UxpManifest extends Schema.Class<UxpManifest, { readonly brand: unique symbol }>('UxpManifest')({
    manifestVersion: Schema.tag(VERSION),
    id: Schema.String,
    name: Schema.String,
    version: Schema.String.check(Schema.isPattern(/^\d+(?:\.\d+){0,2}(?:-[\dA-Za-z-]+(?:\.[\dA-Za-z-]+)*)?(?:\+[\dA-Za-z-]+(?:\.[\dA-Za-z-]+)*)?$/u)).annotate({ description: 'major[.minor[.patch]][-prerelease][+build]' }),
    main: Schema.optionalKey(Schema.String),
    host: Schema.Struct({ app: Schema.Literals(['ID', 'PS', 'premierepro', 'ame', 'XD']), minVersion: Version, maxVersion: Schema.optionalKey(Version), data: Schema.optionalKey(Schema.Struct({ loadEvent: Schema.Literals(['use', 'startup']) })) }),
    icons: Schema.optionalKey(Schema.Array(Icon)),
    entrypoints: Schema.Array(
        Schema.Union([
            Entrypoint.pipe(Schema.fieldsAssign({ type: Schema.Literal('command'), shortcut: Schema.optionalKey(Schema.Struct({ mac: Schema.optionalKey(Schema.String), win: Schema.optionalKey(Schema.String) })) })),
            Entrypoint.pipe(Schema.fieldsAssign({ type: Schema.Literal('panel'), minimumSize: Size, maximumSize: Size, preferredDockedSize: Size, preferredFloatingSize: Size, icons: Schema.optionalKey(Schema.Array(Icon)) })),
        ]),
    ).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])), Schema.withConstructorDefault(Effect.succeed([]))),
    requiredPermissions: Schema.Struct({
        localFileSystem: Schema.optionalKey(Schema.Literals(['plugin', 'request', 'fullAccess'])),
        network: Schema.optionalKey(Schema.Struct({ domains: Schema.NonEmptyArray(Schema.String) })),
        clipboard: Schema.optionalKey(Schema.Literals(['read', 'readAndWrite'])),
        launchProcess: Schema.optionalKey(Schema.Struct({ schemes: Schema.optionalKey(Schema.NonEmptyArray(Schema.String)), extensions: Schema.optionalKey(Schema.NonEmptyArray(Schema.String)) })),
        ipc: Schema.optionalKey(Schema.Struct({ enablePluginCommunication: Schema.Boolean })),
        webview: Schema.optionalKey(Schema.Struct({ allow: Schema.Literal('yes'), domains: Schema.Union([Schema.NonEmptyArray(Schema.String), Schema.Literal('all')]), enableMessageBridge: Schema.optionalKey(Schema.Literals(['localAndRemote', 'localOnly', 'no'])) })),
        allowCodeGenerationFromStrings: Flag,
        enableUserInfo: Flag,
    }),
    featureFlags: Schema.optionalKey(Schema.Struct(Struct.Record(['unhandledRejection', 'uncaughtException', 'enableFillAsCustomAttribute', 'enableSWCSupport'], Flag))),
}) {
    static readonly Version = Version;
    static readonly json = Schema.fromJsonString(UxpManifest);
    static readonly file = 'manifest.json';
    static readonly target = 'esnext';
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { UxpManifest };
