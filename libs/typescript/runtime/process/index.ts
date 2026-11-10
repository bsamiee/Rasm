import { layerMinimumLogLevels } from '@rasm/telemetry';
import { layerNode, Rotation } from '@rasm/telemetry/node';
import { Array, Cause, Config, Effect, Exit, Layer, LogLevel, Option, Ref, Runtime, Schema, SchemaTransformation, String } from 'effect';
import envPaths from 'env-paths';

// --- [MODELS] --------------------------------------------------------------------------

const Level = Schema.String.pipe(Schema.decode(SchemaTransformation.toLowerCase()), Schema.decode(SchemaTransformation.capitalize()), Schema.decodeTo(Schema.Literals(LogLevel.values)));
const Target = Schema.Literals(['file', 'stderr']);

// --- [COMPOSITION] ---------------------------------------------------------------------

const layerTelemetry = Effect.fnUntraced(function* ({ resource, target, level }: { readonly resource: Parameters<typeof layerNode>[0]['resource']; readonly target: typeof Target.Type; readonly level?: LogLevel.LogLevel }) {
    const refused = yield* Ref.make<readonly (readonly [setting: { readonly variable: string; readonly value: string }, error: Config.ConfigError])[]>([]);
    const prefix = String.constantCase(resource.serviceName);
    const decoded =
        <A>(codec: Schema.ConstraintCodec<A, unknown>) =>
        (variable: string) =>
            Config.schema(codec, variable);
    const read = <A>(variable: string, config: (variable: string) => Config.Config<A>) =>
        Effect.catch(
            Config.nested(Config.option(config(variable)), prefix),
            Effect.fnUntraced(function* (error) {
                const value = yield* Config.nested(Config.String(variable), prefix);
                yield* Ref.update(refused, Array.append([{ variable: `${prefix}_${variable}`, value }, error] as const));
                return Option.none<A>();
            }),
        );
    const stated = yield* Effect.all({
        level: read('LOG_LEVEL', decoded(Level)),
        filter: read('LOG_FILTER', (variable) => Config.Record(Schema.String, Level, variable, { trim: true })),
        target: read('LOG_TARGET', decoded(Target)),
        path: read('LOG_DIR', decoded(Schema.String)),
        size: read('LOG_SIZE', decoded(Rotation.fields.size)),
        interval: read('LOG_AGE', decoded(Rotation.fields.interval)),
        maxFiles: read('LOG_FILES', decoded(Rotation.fields.maxFiles)),
    });
    const rotation = {
        ...Option.getOrUndefined(Option.map(stated.size, (size) => ({ size }))),
        ...Option.getOrUndefined(Option.map(stated.interval, (interval) => ({ interval }))),
        ...Option.getOrUndefined(Option.map(stated.maxFiles, (maxFiles) => ({ maxFiles }))),
    };
    const file = [`${resource.serviceName}.log`, { path: Option.getOrElse(stated.path, () => envPaths(resource.serviceName, { suffix: '' }).log), ...rotation }] as const;
    const minimum = Option.getOrElse(
        Option.orElse(Option.fromUndefinedOr(level), () => stated.level),
        () => 'Info' as const,
    );
    return Ref.get(refused).pipe(
        Effect.flatMap(Effect.forEach(([setting, error]) => Effect.annotateLogs(Effect.logWarning('Setting refused, default stands', Cause.fail(error)), setting), { discard: true })),
        Layer.effectDiscard,
        Layer.provideMerge(
            Layer.provideMerge(
                layerMinimumLogLevels(
                    minimum,
                    Option.getOrElse(stated.filter, () => ({})),
                ),
                layerNode({ resource, ...(Option.getOrElse(stated.target, () => target) === 'stderr' ? {} : { file }) }),
            ),
        ),
    );
}, Layer.unwrap);
const teardown: Runtime.Teardown = (exit, onExit) => (Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause) ? onExit(0) : Runtime.defaultTeardown(exit, onExit));

// --- [EXPORTS] -------------------------------------------------------------------------

export { layerTelemetry, teardown };
