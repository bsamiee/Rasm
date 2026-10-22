import { load } from 'cheerio/slim';
import { Context, Effect, Option, Schema } from 'effect';
import { HttpServerRequest, HttpServerResponse } from 'effect/http';
import type { ReactNode } from 'react';
import { renderToReadableStream } from 'react-dom/server';

// --- [TYPES] ---------------------------------------------------------------------------

interface Shell<S extends Schema.Codec<unknown, unknown>> {
    readonly root: string;
    readonly data: string;
    readonly schema: S;
}

// --- [SERVICES] ------------------------------------------------------------------------

class StaticAssets extends Context.Service<StaticAssets, { readonly fetch: (request: Request) => Promise<Response> }>()('@rasm/hosting/rendering/StaticAssets') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const renderPage = Effect.fn('renderPage')(function* <S extends Schema.Codec<unknown, unknown>>(shell: Shell<S>, { value, render, title, description }: { readonly value: S['Type']; readonly render: (value: S['Type']) => ReactNode; readonly title: Option.Option<string>; readonly description: Option.Option<string> }) {
    const [assets, request] = yield* Effect.all([StaticAssets, Effect.flatMap(HttpServerRequest.HttpServerRequest, HttpServerRequest.toWeb)]);
    const [serialized, markup, template] = yield* Effect.all(
        [
            Schema.encodeEffect(Schema.fromJsonString(shell.schema))(value),
            Effect.tryPromise((signal) => renderToReadableStream(render(value), { signal })).pipe(
                Effect.tap((stream) => Effect.tryPromise(() => stream.allReady)),
                Effect.flatMap((stream) => Effect.tryPromise(() => new Response(stream).text())),
            ),
            Effect.tryPromise(() => assets.fetch(request)).pipe(Effect.flatMap((response) => Effect.tryPromise(() => response.text()))),
        ],
        { concurrency: 'unbounded' },
    );
    const page = load(template);
    page(`#${shell.root}`).html(markup);
    page(`#${shell.data}`).text(serialized.replaceAll('<', '\\u003c'));
    page('title').text((_, current) => Option.getOrElse(title, () => current));
    page('meta[name="description"]').attr('content', (_, current) => Option.getOrElse(description, () => current));
    return HttpServerResponse.html(page.html());
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { renderPage, type Shell, StaticAssets };
