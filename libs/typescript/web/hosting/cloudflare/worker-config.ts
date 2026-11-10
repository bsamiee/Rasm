import { workerConfig } from '@rasm/workers/config';
import { bindings, type WorkerConfig } from 'cf/config';
import { Record, type Schema, Struct } from 'effect';
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
    workerConfig({
        ...config,
        name: App.resource(app, 'production'),
        env: {
            ...Struct.get(config, 'env'),
            ...Struct.evolve(
                Record.map<keyof E & string, Binding, string>(app.env, (_, variable) => App.resource(app, 'production', variable)),
                Struct.map(app.env, Struct.lambda<Projection>(App.Binding.$match(projection))),
            ),
        },
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { worker };
