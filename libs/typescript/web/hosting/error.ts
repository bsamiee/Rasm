import { Cause, Effect, Formatter, Schema } from 'effect';
import { HttpApiError } from 'effect/http-api';

// --- [ERRORS] --------------------------------------------------------------------------

class HostingError extends Schema.TaggedError<HostingError, { readonly brand: unique symbol }>()('HostingError', {
    reason: Schema.TaggedUnion({ Build: { environment: Schema.String, entry: Schema.String }, Storage: { operation: Schema.String, key: Schema.String }, Template: { step: Schema.Literals(['fetch', 'answer', 'read']) } }),
    cause: Schema.Defect(),
}) {
    static readonly build = (environment: string, entry: string) => (cause: unknown) => HostingError.make({ reason: { _tag: 'Build', environment, entry }, cause });
    static readonly storage = (operation: string, key: string) => (cause: unknown) => HostingError.make({ reason: { _tag: 'Storage', operation, key }, cause });
    static readonly template = (step: Extract<HostingError['reason'], { readonly _tag: 'Template' }>['step']) => (cause: unknown) => HostingError.make({ reason: { _tag: 'Template', step }, cause });
    static readonly unavailable = Effect.fn('HostingError.unavailable')((error: Cause.YieldableError) => Effect.andThen(Effect.logError(Cause.fail(error)), Effect.fail(HttpApiError.ServiceUnavailable.make())));

    override get message() {
        const failed = HostingError.fields.reason.match(this.reason, {
            Build: ({ environment, entry }) => `${environment} environment build of ${entry}`,
            Storage: ({ operation, key }) => `object ${key} ${operation}`,
            Template: ({ step }) => `page template ${step}`,
        });
        return `${failed} failed, ${Formatter.format(this.cause)}`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { HostingError };
