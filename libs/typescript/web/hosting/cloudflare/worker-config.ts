import { bindings, type WorkerConfig } from 'cf/config';
import { Array, Record, type Schema, Struct } from 'effect';
import { App, type Binding } from '#/app/app';

// --- [TYPES] ---------------------------------------------------------------------------

interface Projection extends Struct.Lambda {
    (binding: Binding): ReturnType<(typeof projection)[Binding['_tag']]>;
    readonly '~lambda.in': Binding;
    readonly '~lambda.out': ReturnType<(typeof projection)[this['~lambda.in']['_tag']]>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const projection = {
    sqlite: () => (name: string) => bindings.d1({ name }),
    postgres: () => () => undefined,
    bucket: () => (name: string) => bindings.r2({ name }),
    secret: () => () => bindings.secret(),
    text: (text: { readonly value: string }) => () => bindings.text(text.value),
    json: (json: { readonly value: Schema.MutableJson }) => () => bindings.json(json.value),
} as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const worker = <const E extends Record.ReadonlyRecord<string, Binding>, const C extends Omit<WorkerConfig, 'name'>>(app: App<E>, config: C) =>
    ({
        ...config,
        name: App.resource(app, 'production'),
        compatibilityFlags: Array.dedupe([...(config.compatibilityFlags ?? []), 'enable_request_signal']),
        env: {
            ...Struct.get(config, 'env'),
            [App.bundle.fetcher]: bindings.assets(),
            OTEL_SERVICE_NAME: bindings.text(app.name),
            ...Struct.evolve(
                Record.map<keyof E & string, Binding, string>(app.env, (_, variable) => App.resource(app, 'production', variable)),
                Struct.map(app.env, Struct.lambda<Projection>(App.Binding.$match(projection))),
            ),
        },
    }) satisfies WorkerConfig;

// --- [EXPORTS] -------------------------------------------------------------------------

export { worker };
