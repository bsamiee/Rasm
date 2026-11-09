import { bindings, type ConfigContext, type WorkerConfig } from 'cf/config';
import { Option, Record, type Schema } from 'effect';
import { App, type Binding } from '#/app';

// --- [TABLES] --------------------------------------------------------------------------

const projection = {
    sqlite: () => bindings.d1(),
    postgres: (id: string) => bindings.hyperdrive({ id }),
    bucket: () => bindings.r2(),
    secret: () => bindings.secret(),
    text: (value: string) => bindings.text(value),
    json: (value: Schema.MutableJson) => bindings.json(value),
} as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const worker =
    <const E extends Record.ReadonlyRecord<string, Binding>, const C extends Omit<WorkerConfig, 'name'>>(app: App<E>, config: C) =>
    ({ mode }: ConfigContext) => {
        const named = Option.getOrElse(
            Option.filter(Option.fromUndefinedOr(mode), (held): held is keyof typeof App.modes => Object.hasOwn(App.modes, held)),
            () => 'production' as const,
        );
        const projected = Record.map(app.env, (binding, variable) =>
            App.Binding.$match(binding, {
                sqlite: projection.sqlite,
                postgres: () => projection.postgres(App.resource(app, named, variable)),
                bucket: projection.bucket,
                secret: projection.secret,
                text: ({ value }) => projection.text(value),
                json: ({ value }) => projection.json(value),
            }),
        ) as { readonly [K in keyof E]: ReturnType<(typeof projection)[E[K]['_tag']]> };
        return {
            ...config,
            name: App.resource(app, named),
            ...(named === 'disposable' && { domains: [], triggers: (config.triggers ?? []).filter(({ type }) => type !== 'fetch') }),
            env: { ...(config.env as C['env']), ...projected },
        } satisfies WorkerConfig;
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { worker };
