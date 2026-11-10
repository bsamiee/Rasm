import { Data, type Record, type Schema, String } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Binding = Data.TaggedEnum<{
    sqlite: Record.ReadonlyRecord<never, never>;
    postgres: Record.ReadonlyRecord<never, never>;
    bucket: Record.ReadonlyRecord<never, never>;
    secret: Record.ReadonlyRecord<never, never>;
    text: { readonly value: string };
    json: { readonly value: Schema.MutableJson };
}>;

// --- [MODELS] --------------------------------------------------------------------------

class App<const E extends Record.ReadonlyRecord<string, Binding> = Record.ReadonlyRecord<string, Binding>> extends Data.Class<{ readonly name: string; readonly env: E }> {
    static readonly Binding = Data.taggedEnum<Binding>();
    static readonly modes = { production: '', disposable: '-disposable' } as const;
    static readonly bundle = { environment: 'node', entry: 'index', assets: 'assets', portVariable: 'PORT', readiness: '/readyz' } as const;
    static readonly make = <const Env extends Record.ReadonlyRecord<string, Binding>>(app: { readonly name: string; readonly env: Env }) => new App<Env>(app);
    static readonly resource = (app: App, mode: keyof typeof App.modes, ...variables: readonly string[]) => `${[app.name, ...variables.map((variable) => String.snakeToKebab(variable.toLowerCase()))].join('-')}${App.modes[mode]}`;
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { App, type Binding };
