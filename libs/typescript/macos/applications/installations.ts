import { Schema, Struct } from 'effect';
import { Failed, InstanceSnapshot, InstanceState, LaunchedInstance } from '#/instances';

// --- [TYPES] ---------------------------------------------------------------------------

type AppleEventValue = null | boolean | number | string | readonly AppleEventValue[] | TypedRecord | Specifier | typeof Descriptor.Type | { readonly _tag?: never; readonly [term: string]: AppleEventValue };

// --- [MODELS] --------------------------------------------------------------------------

const Nested = Schema.suspend((): Schema.Codec<AppleEventValue> => AppleEventValue);
const Code = Schema.String.annotate({ description: 'Four-character descriptor type' });
class TypedRecord extends Schema.Opaque<TypedRecord>()(Schema.TaggedStruct('record', { type: Code, fields: Schema.Record(Schema.String, Nested).annotate({ description: 'Values by property name or code' }) }).annotate({ description: 'Record with custom descriptor type' })) {}
class Specifier extends Schema.Opaque<Specifier>()(
    Schema.TaggedStruct('specifier', {
        want: Schema.String.annotate({ description: 'Specified object class, derived as prop for property form' }),
        form: Schema.Literals(['property', 'index', 'name', 'id', 'relative', 'range', 'test', 'whose']),
        seld: Nested.annotate({ description: 'Selection data, first, middle, last, any, or every for index form, or next or previous for relative form' }),
        from: Schema.optionalKey(Nested.annotate({ description: 'Container specifier, null or absent for application' })),
    }).annotate({ description: 'Object specifier' }),
) {}
const Descriptor = Schema.Union([
    Schema.TaggedStruct('type', { name: Schema.String }).annotate({ description: 'Class or property by name or code' }),
    Schema.TaggedStruct('enumerator', { name: Schema.String }).annotate({ description: 'Enumerator by name or code' }),
    Schema.TaggedStruct('date', { date: Schema.String.annotate({ description: 'ISO 8601 date' }) }),
    Schema.TaggedStruct('file', { url: Schema.String }),
    Schema.TaggedStruct('opaque', { type: Code, data: Schema.String.annotate({ description: 'Descriptor bytes in base64' }), value: Schema.optionalKey(Schema.String) }).annotate({ description: 'Descriptor no other case decodes, sent back unchanged' }),
]);
const AppleEventValue: Schema.Codec<AppleEventValue> = Schema.Union([
    Schema.Null,
    Schema.Boolean,
    Schema.Finite,
    Schema.String.annotate({ description: 'Text, or enumerator, class, date, or file URL per parameter type' }),
    Schema.Array(Nested),
    TypedRecord,
    Specifier,
    Descriptor,
    Schema.StructWithRest(Schema.Struct({ _tag: Schema.optionalKey(Schema.Never) }), [Schema.Record(Schema.String, Nested)]).annotate({ description: 'Plain record by property name or code' }),
]);
const Term = Schema.Struct({ name: Schema.String, code: Schema.String.annotate({ description: 'Code as the scripting definition spells it' }) });
const Command = Term.mapFields(
    Struct.assign({
        parameters: Schema.Array(
            Term.mapFields(
                Struct.assign({
                    types: Schema.Array(Schema.String).annotate({ description: 'Types the scripting definition declares for it' }),
                    list: Schema.Boolean.annotate({ description: 'Takes a list' }),
                    enumerators: Schema.Array(Term).annotate({ description: 'Enumerators of the enumeration typing it' }),
                }),
            ),
        ),
    }),
);
const DictionaryKind = Schema.Literals(['suite', 'command', 'class', 'record-type', 'value-type', 'enumeration']).annotate({ description: 'Scripting definition element kind' });
class ScriptingDictionary extends Schema.Class<ScriptingDictionary, { readonly brand: unique symbol }>('ScriptingDictionary')(
    {
        ...Struct.Record(DictionaryKind.literals, Schema.Array(Term)),
        command: Schema.Array(Command),
        script: Schema.OptionFromOptionalKey(Command.annotate({ description: 'Command a direct text parameter runs as source: the one taking a list of text arguments, else the do script event' })),
        open: Schema.OptionFromOptionalKey(Command.annotate({ description: 'Command of the open documents event, its parameters the names launch takes in opening' })),
    },
    { description: 'Every declaration by kind, name, and code, commands with their parameters, and the script and open commands' },
) {
    static readonly Kind = DictionaryKind;
}
class Installation extends Schema.Class<Installation, { readonly brand: unique symbol }>('Installation')({
    url: InstanceSnapshot.fields.url,
    executable: Schema.OptionFromOptionalKey(Schema.URL.annotate({ description: 'Main executable of the bundle' })),
    identifier: LaunchedInstance.fields.identifier,
    name: Schema.String.annotate({ description: 'Display name of the bundle, as Finder shows it' }),
    bundleName: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'CFBundleName, the name system and crash dialogs show' })),
    version: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'CFBundleShortVersionString' })),
    build: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'CFBundleVersion' })),
    scriptable: Schema.OptionFromOptionalKey(Schema.Boolean.annotate({ description: 'Whether the application takes Apple events through a scripting dictionary, absent when its read failed' })),
    scriptingDefinitionFile: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'OSAScriptingDefinition file in bundle Resources' })),
    activationPolicy: InstanceSnapshot.fields.activationPolicy.annotate({ description: 'From LSBackgroundOnly and LSUIElement' }),
    documentTypes: Schema.OptionFromOptionalKey(
        Schema.Array(Schema.Struct({ role: Schema.optionalKey(Schema.String.annotate({ description: 'CFBundleTypeRole' })), extensions: Schema.Array(Schema.String) })).annotate({
            description: 'CFBundleDocumentTypes by role, extensions as declared and as the system states them for each declared content type',
        }),
    ),
    urlSchemes: Schema.OptionFromOptionalKey(Schema.Array(Schema.String).annotate({ description: 'CFBundleURLSchemes of every CFBundleURLTypes entry' })),
    exportedTypes: Schema.OptionFromOptionalKey(Schema.Array(Schema.Struct({ identifier: Schema.String, extensions: Schema.Array(Schema.String) })).annotate({ description: 'UTExportedTypeDeclarations' })),
    multipleInstancesProhibited: Schema.OptionFromOptionalKey(Schema.Boolean.annotate({ description: 'LSMultipleInstancesProhibited' })),
    environment: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.String).annotate({ description: 'LSEnvironment' })),
    architectures: Schema.OptionFromOptionalKey(Schema.Array(Schema.String).annotate({ description: 'Architectures of the main executable' })),
    quarantine: Schema.OptionFromOptionalKey(
        Schema.Struct({
            agentName: Schema.optionalKey(Schema.String.annotate({ description: 'LSQuarantineAgentName, application that downloaded the bundle' })),
            type: Schema.optionalKey(Schema.String.annotate({ description: 'LSQuarantineType' })),
            originURL: Schema.optionalKey(Schema.String.annotate({ description: 'LSQuarantineOriginURL, page the download came from' })),
            timeStamp: Schema.optionalKey(Schema.String.annotate({ description: 'LSQuarantineTimeStamp, ISO 8601 download time' })),
        }).annotate({ description: 'Quarantine attribute Gatekeeper checks before a first launch, which a failed launch names as its cause' }),
    ),
    claim: Schema.OptionFromOptionalKey(
        Schema.Struct({ prefixes: Schema.Array(Schema.String).annotate({ description: 'Bundle identifier prefixes' }), server: Schema.String.annotate({ description: 'MCP server name' }), scope: Schema.String }).annotate({
            description: 'ApplicationServerClaim: applications the server of this bundle drives',
        }),
    ),
    appIntents: Schema.OptionFromOptionalKey(
        Schema.Array(
            Schema.Struct({
                identifier: Schema.String,
                title: Schema.optionalKey(Schema.String.annotate({ description: 'Title in the interface language, as Shortcuts shows it' })),
                description: Schema.optionalKey(Schema.String),
                openAppWhenRun: Schema.Boolean.annotate({ description: 'Whether running the action opens the application, as it declares' }),
                supportedModes: Schema.Array(Schema.Literals(['background', 'foreground', 'deferred', 'dynamic'])).annotate({
                    description: 'IntentModes it runs in: background without opening the application, foreground opening it at once, deferred opening it later in the run, dynamic deciding as it runs',
                }),
                isDiscoverable: Schema.Boolean.annotate({ description: 'Whether Shortcuts and Spotlight offer it' }),
                parameters: Schema.Array(Schema.Struct({ name: Schema.String, title: Schema.optionalKey(Schema.String), description: Schema.optionalKey(Schema.String), isOptional: Schema.Boolean })),
            }),
        ).annotate({ description: 'App Intents actions from the Metadata.appintents of the bundle, its extensions, and the framework packages they include, which Shortcuts and Siri run, absent when their read failed' }),
    ),
    unread: Schema.optionalKey(Schema.Array(Failed).annotate({ description: 'Facts read after the Info.plist that failed, each with its cause, the file as subject: scriptable and quarantine, architectures, appIntents' })),
}) {
    static readonly runs =
        ({ url }: Pick<Installation, 'url'>) =>
        (instance: InstanceSnapshot) =>
            instance.url.href === url.href && !InstanceState.isAnyOf(['exited'])(instance.state);
}
const LaunchEvent = Schema.Union([Schema.TaggedStruct('launched', LaunchedInstance.fields), Schema.TaggedStruct('state', InstanceSnapshot.fields), Schema.TaggedStruct('opened', { reply: AppleEventValue }), InstanceState.cases.failed])
    .annotate({ description: 'Launch events: launched first, then states, the reply of the open command once sent, ending idle or asking' })
    .pipe(Schema.toTaggedUnion('_tag'));

// --- [EXPORTS] -------------------------------------------------------------------------

export { AppleEventValue, Installation, LaunchEvent, ScriptingDictionary };
