import { bindings, type ConfigContext, type WorkerConfig } from 'cf/config';
import { Option, Record, Schema, Struct } from 'effect';
import { App, type Binding } from '#/app';

// --- [TYPES] ---------------------------------------------------------------------------

interface Projection extends Struct.Lambda {
    (binding: Binding): ReturnType<(typeof projection)[Binding['_tag']]>;
    readonly '~lambda.in': Binding;
    readonly '~lambda.out': ReturnType<(typeof projection)[this['~lambda.in']['_tag']]>;
}

// --- [TABLES] --------------------------------------------------------------------------

const projection = {
    sqlite: () => () => bindings.d1(),
    postgres: () => (id: string) => bindings.hyperdrive({ id }),
    bucket: () => () => bindings.r2(),
    secret: () => () => bindings.secret(),
    text: (text: { readonly value: string }) => () => bindings.text(text.value),
    json: (json: { readonly value: Schema.MutableJson }) => () => bindings.json(json.value),
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Mode = Schema.Literals(Struct.keys(App.modes));

// --- [COMPOSITION] ---------------------------------------------------------------------

const worker =
    <const E extends Record.ReadonlyRecord<string, Binding>, const C extends Omit<WorkerConfig, 'name'>>(app: App<E>, config: C) =>
    ({ mode }: ConfigContext) => {
        const named = Option.getOrElse(Schema.decodeUnknownOption(Mode)(mode), () => 'production' as const);
        const projected = Struct.evolve(
            Record.map<keyof E & string, Binding, string>(app.env, (_, variable) => App.resource(app, named, variable)),
            Struct.map(app.env, Struct.lambda<Projection>(App.Binding.$match(projection))),
        );
        return {
            ...config,
            name: App.resource(app, named),
            ...(named === 'disposable' && { domains: [], triggers: (config.triggers ?? []).filter(({ type }) => type !== 'fetch') }),
            env: { ...Struct.get(config, 'env'), ...projected },
        } satisfies WorkerConfig;
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { worker };
