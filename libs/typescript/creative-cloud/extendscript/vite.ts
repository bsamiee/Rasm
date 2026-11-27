import { checks } from '@rasm/vite/checks';
import { Array, Cause, Effect, Schema } from 'effect';
import { build, type Plugin, type Rolldown } from 'vite';
import { lower } from '#/node';

// --- [MODELS] --------------------------------------------------------------------------

const Built = Schema.Struct({ output: Schema.NonEmptyArray(Schema.Struct({ code: Schema.String })) }).annotate({ description: 'Single-environment Vite build output, entry chunk first' });
const Quoted = Schema.fromJsonString(Schema.String);

// --- [OPERATIONS] ----------------------------------------------------------------------

const extendscript = (): Rolldown.Plugin => ({ name: 'extendscript', renderChunk: (code) => Effect.runPromise(lower(code, lower.extendscript)) });
const inlineScript = (entry: string, name: string): Plugin => {
    const compiled = Effect.tryPromise({
        try: () =>
            build({
                configFile: false,
                logLevel: 'warn',
                plugins: [checks()],
                build: {
                    write: false,
                    rolldownOptions: {
                        input: entry,
                        platform: 'neutral',
                        preserveEntrySignatures: 'exports-only',
                        output: { format: 'iife', name, generatedCode: { symbols: false }, minify: false, plugins: extendscript() },
                    },
                },
            }),
        catch: (cause) => new Cause.UnknownError(cause, `Build of ${entry} failed`),
    }).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(Built)),
        Effect.flatMap(({ output }) => Effect.orDie(Schema.encodeEffect(Quoted)(Array.headNonEmpty(output).code))),
        Effect.map((code) => `export default ${code}`),
    );
    return { name: 'script', load: { filter: { id: new RegExp(`^${RegExp.escape(entry)}\\?inline$`, 'u') }, handler: () => Effect.runPromise(compiled) } };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { extendscript, inlineScript };
