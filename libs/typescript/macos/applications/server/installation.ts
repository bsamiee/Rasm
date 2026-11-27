import { Schema, Struct } from 'effect';
import { ApplicationFailure } from '#/server/failure';

// --- [MODELS] --------------------------------------------------------------------------

const Term = Schema.Struct({ name: Schema.String, code: Schema.String });
const Command = Term.mapFields(Struct.assign({ parameters: Schema.Array(Term.mapFields(Struct.assign({ types: Schema.Array(Schema.String), list: Schema.Boolean, enumerators: Schema.Array(Term) }))) }));
const Kind = Schema.Literals(['suite', 'command', 'class', 'record-type', 'value-type', 'enumeration']);
const Dictionary = Object.assign(Schema.Struct({ ...Struct.Record(Kind.literals, Schema.Array(Term)), command: Schema.Array(Command), script: Schema.OptionFromOptionalKey(Command), open: Schema.OptionFromOptionalKey(Command) }), { Kind });
class Installation extends Schema.Class<Installation, { readonly brand: unique symbol }>('Installation')({
    url: Schema.URL,
    executable: Schema.OptionFromOptionalKey(Schema.URL),
    identifier: Schema.String,
    name: Schema.String,
    bundleName: Schema.OptionFromOptionalKey(Schema.String),
    version: Schema.OptionFromOptionalKey(Schema.String),
    build: Schema.OptionFromOptionalKey(Schema.String),
    scriptable: Schema.OptionFromOptionalKey(Schema.Boolean),
    activationPolicy: Schema.Union([Schema.Literals(['regular', 'accessory', 'prohibited']), Schema.TemplateLiteral([Schema.Int])]),
    documentTypes: Schema.Struct({ role: Schema.optionalKey(Schema.String), extensions: Schema.Array(Schema.String) }).pipe(Schema.Array, Schema.OptionFromOptionalKey),
    urlSchemes: Schema.String.pipe(Schema.Array, Schema.OptionFromOptionalKey),
    multipleInstancesProhibited: Schema.OptionFromOptionalKey(Schema.Boolean),
    quarantine: Schema.OptionFromOptionalKey(Schema.Struct({ agentName: Schema.optionalKey(Schema.String), originURL: Schema.optionalKey(Schema.String) })),
    claim: Schema.OptionFromOptionalKey(Schema.Struct({ prefixes: Schema.Array(Schema.String), server: Schema.String, scope: Schema.String })),
    appIntents: Schema.Struct({
        identifier: Schema.String,
        title: Schema.optionalKey(Schema.String),
        openAppWhenRun: Schema.Boolean,
        parameters: Schema.Array(Schema.Struct({ name: Schema.String, isOptional: Schema.Boolean })),
    }).pipe(Schema.Array, Schema.OptionFromOptionalKey),
    unread: ApplicationFailure.Stated.pipe(Schema.Array, Schema.optionalKey),
}) {
    static readonly Dictionary = Dictionary;
    static readonly Listing = Schema.Struct({ installations: Schema.Array(Installation), unreadable: Schema.Array(ApplicationFailure.Stated) });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Installation };
