import { Data, type Record, Schema } from 'effect';
import { createElement, type ReactElement } from 'react';

// --- [MODELS] --------------------------------------------------------------------------

class Shell<S extends Schema.Codec<unknown, unknown>> extends Data.Class<{
    readonly name: string;
    readonly schema: S;
    readonly render: (value: S['Type']) => ReactElement;
    readonly policy?: Record.ReadonlyRecord<string, readonly string[]>;
}> {
    readonly data = { id: `${this.name}-data`, json: Schema.fromJsonString(this.schema) };
    page(value: S['Type'], json: string, { lang, head }: { readonly lang: string; readonly head?: string }) {
        return createElement('html', { lang }, createElement('head', head === undefined ? null : { dangerouslySetInnerHTML: { __html: head } }), createElement('body', null, this.render(value), createElement('script', { id: this.data.id, type: 'application/json' }, json)));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Shell };
