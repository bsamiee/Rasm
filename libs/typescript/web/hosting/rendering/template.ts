import { load } from 'cheerio/slim';
import { Cause, Context, Effect, Function, Layer, Option, Record, Schema, UndefinedOr } from 'effect';
import { HttpServerResponse } from 'effect/http';
import { renderToReadableStream } from 'react-dom/server';
import { HostingError } from '#/app/error';
import type { Shell } from '#/rendering/shell';

// --- [SERVICES] ------------------------------------------------------------------------

class Template extends Context.Service<Template, string>()('@rasm/hosting/rendering/template') {
    static readonly layer = (assets: { readonly fetch: (request: Request) => Promise<Response> }) =>
        Layer.effect(
            Template,
            Effect.tryPromise({ try: (signal) => assets.fetch(new Request('http://localhost/', { signal })), catch: HostingError.of('Template', 'fetch') }).pipe(
                Effect.filterOrFail(
                    ({ ok }) => ok,
                    ({ status }) => HostingError.of('Template', `answer ${status}`)(),
                ),
                Effect.flatMap((response) => Effect.tryPromise({ try: () => response.text(), catch: HostingError.of('Template', 'read') })),
            ),
        );
    static readonly render = Effect.fn('Template.render')(function* <S extends Schema.Codec<unknown, unknown>>(shell: Shell<S>, value: S['Type']) {
        const [template, context] = yield* Effect.all([Template, Effect.context<never>()]);
        const onError = (error: unknown) => Effect.logError(Cause.die(error)).pipe(Effect.runSyncWith(context));
        const [serialized, markup] = yield* Effect.all(
            [
                Schema.encodeEffect(shell.script.json)(value).pipe(Effect.orDie),
                Effect.promise((signal) => renderToReadableStream(shell.render(value), { signal, onError })).pipe(
                    Effect.tap((stream) => Effect.promise(Function.constant(stream.allReady))),
                    Effect.flatMap((stream) => Effect.promise(Response.prototype.text.bind(new Response(stream)))),
                ),
            ],
            { concurrency: 'unbounded' },
        );
        const { title, meta } = shell.head(value);
        const document = load(template);
        const upsert = (selector: string, element: string) => (document(selector).length > 0 ? document(selector) : document(element).appendTo('head'));
        document(`#${shell.root}`)
            .html(markup)
            .after(document('<script>').attr({ id: shell.script.id, type: 'application/json' }).text(serialized));
        upsert('title', '<title>').text((_, current) => Option.getOrElse(title, Function.constant(current)));
        const tagged = (attribute: string, contents: Record.ReadonlyRecord<string, string>) => Record.collect(contents, (key, content) => upsert(`meta[${attribute}="${key}"]`, '<meta>').attr({ [attribute]: key, content }));
        Record.collect(Record.map(meta, Record.getSomes), tagged);
        return HttpServerResponse.html(document.html()).pipe(UndefinedOr.match(shell.policy, { onUndefined: () => Function.identity, onDefined: (policy) => HttpServerResponse.setHeader('Content-Security-Policy', Record.collect(policy, (directive, sources) => `${directive} ${sources.join(' ')}`).join('; ')) }));
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Template };
