import { HostError } from '@rasm/automation/requests';
import { Array, Cause, Context, Effect, Option, Predicate, Record, Schema } from 'effect';
import type { Manifest } from '#/manifest';

// --- [SERVICES] ------------------------------------------------------------------------

class Uxp extends Context.Service<Uxp, { readonly uxp: typeof import('adobe:uxp').default; readonly fs: typeof import('adobe:fs') }>()('@rasm/uxp/runtime/Uxp') {
    static readonly Entry = Schema.Struct({ isEntry: Schema.Literal(true), url: Schema.Union([Schema.String, Schema.URL]) });
    static readonly thrown = (cause: unknown): HostError => {
        const prepare = Error.prepareStackTrace;
        Error.prepareStackTrace = (_error, frames) => frames.find((frame) => frame.isEval())?.getLineNumber();
        try {
            return HostError.thrown(Predicate.hasProperty(cause, 'message') ? String(cause.message) : String(cause), Option.liftPredicate(Predicate.hasProperty(cause, 'stack') ? cause.stack : undefined, Predicate.isNumber))(cause);
        } finally {
            Error.prepareStackTrace = prepare;
        }
    };
    static readonly evaluate = (scope: Record.ReadonlyRecord<string, unknown>, code: string) =>
        Effect.try({
            // biome-ignore lint/nursery/noImpliedEval: execute_script evaluates caller source in the host
            try: (): unknown => Reflect.apply(new Function(...Record.keys(scope), 'code', 'return eval(code)'), undefined, [...Object.values(scope), code]),
            catch: Uxp.thrown,
        }).pipe(
            Effect.filterOrFail(Predicate.isFunction, () => new Cause.IllegalArgumentError('code holds no body function')),
            Effect.map((body) => Effect.try({ try: (): unknown => Reflect.apply(body, undefined, []), catch: Uxp.thrown })),
        );
    static readonly accessors = (prototype: object) => {
        const declared = Object.entries(Object.getOwnPropertyDescriptors(prototype));
        return {
            getters: Array.flatMap(declared, ([name, { get, set }]) => (get === undefined ? [] : [{ name, settable: set !== undefined }])),
            methods: Array.flatMap(declared, ([name, { value }]) => (Predicate.isFunction(value) && name !== 'constructor' ? [name] : [])),
        };
    };
}
class Plugin extends Context.Service<Plugin, Manifest>()('@rasm/uxp/runtime/Plugin') {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Plugin, Uxp };
