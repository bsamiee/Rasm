import { Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSION = 5;

// --- [MODELS] --------------------------------------------------------------------------

const Command = Schema.Struct({ type: Schema.Literal('command'), id: Schema.String, label: Schema.Struct({ default: Schema.String }) });

class Manifest extends Schema.Class<Manifest, { readonly brand: unique symbol }>('Manifest')({
    manifestVersion: Schema.tag(VERSION),
    id: Schema.String,
    name: Schema.String,
    version: Schema.String,
    host: Schema.Struct({ app: Schema.Literals(['ID', 'PS', 'premierepro', 'ame', 'XD']), minVersion: Schema.String }),
    entrypoints: Schema.Array(Schema.Union([Command, Command.pipe(Schema.fieldsAssign({ type: Schema.Literal('panel'), minimumSize: Schema.OptionFromOptionalKey(Schema.Struct({ width: Schema.Finite, height: Schema.Finite })) }))])),
    requiredPermissions: Schema.Struct({
        localFileSystem: Schema.OptionFromOptionalKey(Schema.Literals(['plugin', 'request', 'fullAccess'])),
        network: Schema.OptionFromOptionalKey(Schema.Struct({ domains: Schema.NonEmptyArray(Schema.String) })),
    }),
}) {
    static readonly json = Schema.fromJsonString(Manifest);
    static readonly loopback = ({ port }: URL) => `ws://localhost:${port}`;
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Manifest };
