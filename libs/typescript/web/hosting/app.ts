import { Data, type Record, type Schema, String } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Binding = Data.TaggedEnum<{
    sqlite: {};
    postgres: {};
    bucket: {};
    secret: {};
    text: { readonly value: string };
    json: { readonly value: Schema.MutableJson };
}>;

// --- [MODELS] --------------------------------------------------------------------------

class App<const E extends Record.ReadonlyRecord<string, Binding> = Record.ReadonlyRecord<string, Binding>> extends Data.Class<{ readonly name: string; readonly env: E }> {
    static readonly Binding = Data.taggedEnum<Binding>();
    static readonly modes = { production: '', disposable: '-disposable' } as const;
    static readonly make = <const E extends Record.ReadonlyRecord<string, Binding>>(app: { readonly name: string; readonly env: E }) => new App<E>(app);
    static readonly resource = (app: App, mode: keyof typeof App.modes, ...variables: readonly string[]) => `${[app.name, ...variables.map((variable) => String.snakeToKebab(variable.toLowerCase()))].join('-')}${App.modes[mode]}`;
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { App, type Binding };
