import { Effect, Schema, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSION = 5;

// --- [MODELS] --------------------------------------------------------------------------

const Optional = <S extends Schema.Top>(schema: S) => Schema.OptionFromOptionalKey(schema).pipe(Schema.withConstructorDefault(Effect.succeedNone));
const Version = Schema.String.check(Schema.isPattern(/^\d+(?:\.\d+){0,2}$/u)).annotate({ description: 'major[.minor[.patch]]' });
const Flag = Optional(Schema.Boolean);
const Size = Optional(Schema.Struct({ width: Schema.Finite, height: Schema.Finite }));
const Icons = Optional(
    Schema.Array(
        Schema.Struct({
            width: Schema.Finite,
            height: Schema.Finite,
            path: Schema.String,
            scale: Schema.Array(Schema.Finite),
            theme: Optional(Schema.Array(Schema.Literals(['lightest', 'light', 'medium', 'dark', 'darkest', 'all']))),
            species: Optional(Schema.Array(Schema.String)),
        }),
    ),
);
const Entrypoint = Schema.Struct({ id: Schema.String, label: Schema.Union([Schema.String, Schema.StructWithRest(Schema.Struct({ default: Schema.String }), [Schema.Record(Schema.String, Schema.String)])]) });

class Manifest extends Schema.Class<Manifest, { readonly brand: unique symbol }>('Manifest')({
    manifestVersion: Schema.tag(VERSION),
    id: Schema.String,
    name: Schema.String,
    version: Version,
    main: Optional(Schema.String),
    host: Schema.Struct({ app: Schema.Literals(['ID', 'PS', 'premierepro', 'ame', 'XD']), minVersion: Version, maxVersion: Optional(Version), data: Optional(Schema.Struct({ loadEvent: Schema.Literals(['use', 'startup']) })) }),
    icons: Icons,
    entrypoints: Schema.Array(
        Schema.Union([
            Entrypoint.pipe(Schema.fieldsAssign({ type: Schema.Literal('command'), shortcut: Optional(Schema.Struct({ mac: Optional(Schema.String), win: Optional(Schema.String) })) })),
            Entrypoint.pipe(Schema.fieldsAssign({ type: Schema.Literal('panel'), minimumSize: Size, maximumSize: Size, preferredDockedSize: Size, preferredFloatingSize: Size, icons: Icons })),
        ]),
    ),
    requiredPermissions: Schema.Struct({
        localFileSystem: Optional(Schema.Literals(['plugin', 'request', 'fullAccess'])),
        network: Optional(Schema.Struct({ domains: Schema.NonEmptyArray(Schema.String) })),
        clipboard: Optional(Schema.Literals(['read', 'readAndWrite'])),
        launchProcess: Optional(Schema.Struct({ schemes: Optional(Schema.NonEmptyArray(Schema.String)), extensions: Optional(Schema.NonEmptyArray(Schema.String)) })),
        ipc: Optional(Schema.Struct({ enablePluginCommunication: Schema.Boolean })),
        webview: Optional(Schema.Struct({ allow: Schema.Literal('yes'), domains: Schema.Union([Schema.NonEmptyArray(Schema.String), Schema.Literal('all')]), enableMessageBridge: Optional(Schema.Literals(['localAndRemote', 'localOnly', 'no'])) })),
        allowCodeGenerationFromStrings: Flag,
        enableUserInfo: Flag,
    }),
    featureFlags: Optional(Schema.Struct(Struct.Record(['unhandledRejection', 'uncaughtException', 'enableFillAsCustomAttribute', 'enableSWCSupport'], Flag))),
}) {
    static readonly Version = Version;
    static readonly json = Schema.fromJsonString(Manifest);
    static readonly file = 'manifest.json';
    static readonly target = 'esnext';
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Manifest };
