import { load } from 'cheerio/slim';
import { Cause, Context, Effect, Option, Record, Schema } from 'effect';
import { HttpServerResponse } from 'effect/http';
import { renderToReadableStream } from 'react-dom/server';
import type { Shell } from '#/page/shell';

// --- [SERVICES] ------------------------------------------------------------------------

class Page extends Context.Service<Page, { readonly fetch: (request: Request) => Promise<Response> }>()('@rasm/hosting/page/render/Page') {
    static readonly render = Effect.fn('Page.render')(function* <S extends Schema.Codec<unknown, unknown>>(shell: Shell<S>, value: S['Type']) {
        const [page, context] = yield* Effect.all([Page, Effect.context<never>()]);
        const onError = (error: unknown) => Effect.logError(Cause.die(error)).pipe(Effect.runSyncWith(context));
        const [serialized, markup, text] = yield* Effect.all(
            [
                Schema.encodeEffect(shell.script.json)(value).pipe(Effect.orDie),
                Effect.promise((signal) => renderToReadableStream(shell.render(value), { signal, onError })).pipe(
                    Effect.tap((stream) => Effect.promise(() => stream.allReady)),
                    Effect.flatMap((stream) => Effect.promise(() => new Response(stream).text())),
                ),
                Effect.tryPromise({ try: (signal) => page.fetch(new Request('http://localhost/', { signal })), catch: failure('template fetch') }).pipe(
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
        const document = load(text);
        const upsert = (selector: string, element: string) => {
            const present = document(selector);
            return present.length > 0 ? present : document(element).appendTo('head');
        };
        document(`#${shell.root}`).html(markup).after(document('<script>').attr({ id: shell.script.id, type: 'application/json' }).text(serialized));
        Option.toArray(title).forEach((content) => upsert('title', '<title>').text(content));
        Record.toEntries(meta).forEach(([attribute, values]) => Record.toEntries(Record.getSomes(values)).forEach(([key, content]) => upsert(`meta[${attribute}="${key}"]`, '<meta>').attr({ [attribute]: key, content })));
        return HttpServerResponse.html(document.html());
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const failure = (step: string) => (cause: unknown) => new Cause.UnknownError(cause, `page ${step} failed`);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Page };
