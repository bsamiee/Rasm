import { Data, type Option, type Record, Schema, SchemaGetter } from 'effect';
import type { ReactElement } from 'react';

// --- [MODELS] --------------------------------------------------------------------------

class Shell<S extends Schema.Codec<unknown, unknown>> extends Data.Class<{
    readonly root: string;
    readonly schema: S;
    readonly render: (value: S['Type']) => ReactElement;
    readonly head: (value: S['Type']) => { readonly title: Option.Option<string>; readonly meta: Record.ReadonlyRecord<'name' | 'property', Record.ReadonlyRecord<string, Option.Option<string>>> };
}> {
    readonly script = { id: `${this.root}-data`, json: Schema.fromJsonString(this.schema).pipe(Schema.encode({ decode: SchemaGetter.passthrough(), encode: SchemaGetter.transform((json: string) => json.replaceAll('<', '\\u003c')) })) };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Shell };
