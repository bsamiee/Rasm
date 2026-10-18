import { load } from 'cheerio/slim';
import { Effect, Option, Schema } from 'effect';
import { HttpServerResponse } from 'effect/http';
import type { ReactNode } from 'react';
import { renderToReadableStream } from 'react-dom/server';

// --- [TYPES] ---------------------------------------------------------------------------

interface Shell<S extends Schema.Codec<unknown, unknown>> {
    readonly root: string;
    readonly data: string;
    readonly schema: S;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const renderPage = Effect.fn('renderPage')(function* <S extends Schema.Codec<unknown, unknown>>(
    shell: Shell<S>,
    {
        assets,
        request,
        value,
        render,
        title,
        description,
    }: {
        readonly assets: { readonly fetch: (request: Request) => Promise<Response> };
        readonly request: Request;
        readonly value: S['Type'];
        readonly render: (value: S['Type']) => ReactNode;
        readonly title: Option.Option<string>;
        readonly description: Option.Option<string>;
    },
) {
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
    Option.toArray(title).forEach((text) => {
        page('title').text(text);
    });
    Option.toArray(description).forEach((text) => {
        page('meta[name="description"]').attr('content', text);
    });
    return HttpServerResponse.html(page.html());
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { renderPage, type Shell };
