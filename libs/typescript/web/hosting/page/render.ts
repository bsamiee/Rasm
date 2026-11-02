import { load } from 'cheerio/slim';
import { Cause, Context, Effect, Function, flow, Option, Record, Schema, SchemaGetter } from 'effect';
import { HttpServerRequest, HttpServerResponse } from 'effect/http';
import type { ReactNode } from 'react';
import { renderToReadableStream } from 'react-dom/server';

// --- [TYPES] ---------------------------------------------------------------------------

interface Shell<S extends Schema.Codec<unknown, unknown>> {
    readonly root: string;
    readonly data: string;
    readonly schema: S;
    readonly render: (value: S['Type']) => ReactNode;
    readonly head: (value: S['Type']) => { readonly title: Option.Option<string>; readonly meta: Record.ReadonlyRecord<string, Option.Option<string>> };
}

// --- [SERVICES] ------------------------------------------------------------------------

class Page extends Context.Service<Page, { readonly fetch: (request: Request) => Promise<Response> }>()('@rasm/hosting/page/render/Page') {
    static readonly render = Effect.fn('Page.render')(function* <S extends Schema.Codec<unknown, unknown>>(shell: Shell<S>, value: S['Type']) {
        const [assets, url] = yield* Effect.all([Page, Effect.flatMap(HttpServerRequest.HttpServerRequest, flow(HttpServerRequest.toURL, Effect.fromOption)).pipe(Effect.orDie)]);
        const [serialized, markup, template] = yield* Effect.all(
            [
                Schema.encodeEffect(wire(shell.schema))(value).pipe(Effect.orDie),
                Effect.tryPromise({ try: (signal) => renderToReadableStream(shell.render(value), { signal }), catch: failure('markup render') }).pipe(
                    Effect.tap((stream) => Effect.tryPromise({ try: () => stream.allReady, catch: failure('markup render') })),
                    Effect.flatMap((stream) => Effect.tryPromise({ try: () => new Response(stream).text(), catch: failure('markup read') })),
                ),
                Effect.tryPromise({ try: (signal) => assets.fetch(new Request(new URL('/', url), { signal })), catch: failure('template fetch') }).pipe(
                    Effect.filterOrFail(
                        ({ ok }) => ok,
                        (response) => new Cause.UnknownError(response, `page template answered ${response.status}`),
                    ),
                    Effect.flatMap((response) => Effect.tryPromise({ try: () => response.text(), catch: failure('template read') })),
                ),
            ],
            { concurrency: 'unbounded' },
        );
        const { title, meta } = shell.head(value);
        const page = load(template);
        page(`#${shell.root}`).html(markup);
        page(`#${shell.data}`).text(serialized);
        page('title').text((_, current) => Option.getOrElse(title, () => current));
        Record.collect(Record.getSomes(meta), (name, content) => {
            const present = page(`meta[name="${name}"], meta[property="${name}"]`);
            return (present.length > 0 ? present : page('<meta>').attr('name', name).appendTo('head')).attr('content', content);
        });
        return HttpServerResponse.html(page.html());
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const failure = (step: string) => (cause: unknown) => new Cause.UnknownError(cause, `page ${step} failed`);
const wire = Function.memoize((schema: Schema.Codec<unknown, unknown>) => Schema.fromJsonString(schema).pipe(Schema.encode({ decode: SchemaGetter.passthrough(), encode: SchemaGetter.transform((json: string) => json.replaceAll('<', '\\u003c')) })));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Page, type Shell };
