import { HostError } from '@rasm/automation/requests';
import { Array, Cause, Context, Effect, Option, Predicate, Record, Result, Schema } from 'effect';
import type { Manifest } from '#/manifest';

// --- [MODELS] --------------------------------------------------------------------------

const Described = Schema.Struct({ message: Schema.Unknown });
const Located = Schema.Struct({ stack: Schema.Int });

// --- [SERVICES] ------------------------------------------------------------------------

class Uxp extends Context.Service<Uxp, { readonly uxp: typeof import('adobe:uxp').default; readonly fs: typeof import('adobe:fs') }>()('@rasm/uxp/runtime/Uxp') {
    static readonly Entry = Schema.Struct({ isEntry: Schema.Literal(true), url: Schema.Union([Schema.String, Schema.URL]) });
    static readonly thrown = (cause: unknown): HostError => {
        const prepare = Error.prepareStackTrace;
        Error.prepareStackTrace = (_error, frames) => frames.find((frame) => frame.isEval())?.getLineNumber();
        try {
            return HostError.thrown(
                Option.match(Schema.decodeUnknownOption(Described)(cause), { onNone: () => String(cause), onSome: ({ message }) => String(message) }),
                Option.map(Schema.decodeUnknownOption(Located)(cause), ({ stack }) => stack),
            )(cause);
        } finally {
            Error.prepareStackTrace = prepare;
        }
    };
    static readonly evaluate = (scope: Record.ReadonlyRecord<string, unknown>, code: string) =>
        Effect.try({
            // biome-ignore lint/nursery/noImpliedEval: execute_script evaluates caller source in the host
            try: (): unknown => new Function(...Record.keys(scope), 'code', 'return eval(code)')(...Object.values(scope), code),
            catch: Uxp.thrown,
        }).pipe(
            Effect.filterOrFail(Predicate.isFunction, () => new Cause.IllegalArgumentError('code holds no body function')),
            Effect.map((body) => Effect.try({ try: (): unknown => body(), catch: Uxp.thrown })),
        );
    static readonly accessors = (prototype: object) => {
        const declared = Object.entries(Object.getOwnPropertyDescriptors(prototype));
        return {
            getters: Array.filterMap(declared, ([name, { get, set }]) => (get === undefined ? Result.failVoid : Result.succeed({ name, settable: set !== undefined }))),
            methods: Array.filterMap(declared, ([name, { value }]) => (Predicate.isFunction(value) && name !== 'constructor' ? Result.succeed(name) : Result.failVoid)),
        };
    };
}
class Plugin extends Context.Service<Plugin, Manifest>()('@rasm/uxp/runtime/Plugin') {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Plugin, Uxp };
