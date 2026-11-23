import { load } from 'cheerio/slim';
import { Array, Cause, Context, Duration, Effect, Exit, Function, Layer, Option, Record, Schema, Stream, UndefinedOr } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { Base64 } from 'effect/encoding';
import { HttpServerResponse } from 'effect/http';
import { renderToReadableStream } from 'react-dom/server';
import { HostingError } from '#/app/error';
import type { Shell } from '#/rendering/shell';

// --- [SERVICES] ------------------------------------------------------------------------

class Template extends Context.Service<Template, Effect.Effect<{ readonly lang: string; readonly head: string }, HostingError>>()('@rasm/hosting/rendering/template') {
    static readonly layer = (assets: { readonly fetch: (request: Request) => Promise<Response> }) =>
        Layer.effect(
            Template,
            Effect.tryPromise({ try: (signal) => assets.fetch(new Request('http://localhost/', { signal })), catch: HostingError.of('Template', 'fetch') }).pipe(
                Effect.filterOrFail(
                    ({ ok }) => ok,
                    ({ status }) => HostingError.of('Template', `answer ${status}`)(),
                ),
                Effect.flatMap((response) => Effect.tryPromise({ try: () => response.text(), catch: HostingError.of('Template', 'read') })),
                Effect.flatMap((html) => {
                    const document = load(html);
                    return Effect.fromOption(Option.all({ lang: Option.fromUndefinedOr(document('html').attr('lang')), head: Option.fromNullOr(document('head').html()) })).pipe(Effect.mapError(HostingError.of('Template', 'language and head')));
                }),
                Effect.cachedWithTTL((exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero)),
            ),
        );
    static readonly render = Effect.fn('Template.render')(function* <S extends Schema.Codec<unknown, unknown>>(shell: Shell<S>, value: S['Type'], sources: Record.ReadonlyRecord<string, readonly string[]> = {}) {
        const [document, context, json, random] = yield* Effect.all([Effect.flatten(Template), Effect.context<never>(), Effect.orDie(Schema.encodeEffect(shell.data.json)(value)), SecureRandom.SecureRandom]);
        const nonce = Base64.encode(yield* random.randomBytes(16));
        const rendered = yield* Effect.promise((signal) => renderToReadableStream(shell.page(value, json, document), { signal, nonce, onError: (error) => Effect.runSyncWith(context)(Effect.logError(Cause.die(error))) }));
        return HttpServerResponse.stream(Stream.fromReadableStream<Uint8Array, HostingError>({ evaluate: Function.constant(rendered), onError: HostingError.of('Template', 'render') }), { contentType: 'text/html; charset=utf-8' }).pipe(
            UndefinedOr.match(shell.policy, {
                onUndefined: () => Function.identity,
                onDefined: (policy) =>
                    HttpServerResponse.setHeader(
                        'Content-Security-Policy',
                        Record.collect(
                            [sources, { 'script-src': [`'nonce-${nonce}'`] }].reduce((stated, added) => Record.union(stated, added, (held: readonly string[], more: readonly string[]) => Array.appendAll(held, more)), policy),
                            (directive, allowed) => `${directive} ${Array.dedupe(allowed).join(' ')}`,
                        ).join('; '),
                    ),
            }),
        );
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Template };
