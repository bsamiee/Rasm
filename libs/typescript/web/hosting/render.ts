import { Array, Effect, Formatter, Option, Schema } from 'effect';
import type { ReactNode } from 'react';
import { renderToReadableStream } from 'react-dom/server';

// --- [TYPES] ---------------------------------------------------------------------------

interface Shell<S extends Schema.Codec<unknown, unknown>> {
    readonly root: string;
    readonly data: string;
    readonly schema: S;
}

// --- [ERRORS] --------------------------------------------------------------------------

const RenderReason = Schema.TaggedUnion({
    Untemplated: { url: Schema.String, cause: Schema.Defect() },
    Unrendered: { cause: Schema.Defect() },
});
class RenderError extends Schema.TaggedError<RenderError, { readonly brand: unique symbol }>()('RenderError', { reason: RenderReason }) {
    override get message(): string {
        return RenderReason.match(this.reason, {
            Untemplated: ({ url, cause }) => `the page template at ${url} could not be fetched: ${Formatter.format(cause)}`,
            Unrendered: ({ cause }) => `React could not render the page: ${Formatter.format(cause)}`,
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const renderPage = Effect.fnUntraced(function* <S extends Schema.Codec<unknown, unknown>>(
    shell: Shell<S>,
    { assets, url, value, render, title, description }: { readonly assets: Fetcher; readonly url: string; readonly value: S['Type']; readonly render: (value: S['Type']) => ReactNode; readonly title: Option.Option<string>; readonly description: Option.Option<string> },
): Effect.fn.Return<Response, RenderError | Schema.SchemaError> {
    const [serialized, markup, template] = yield* Effect.all(
        [
            Schema.encodeEffect(Schema.fromJsonString(shell.schema))(value),
            Effect.tryPromise({ try: (signal) => renderToReadableStream(render(value), { signal }).then((stream) => stream.allReady.then(() => stream)), catch: (cause) => RenderError.make({ reason: { _tag: 'Unrendered', cause } }) }),
            Effect.tryPromise({ try: () => assets.fetch(url), catch: (cause) => RenderError.make({ reason: { _tag: 'Untemplated', url, cause } }) }),
        ],
        { concurrency: 'unbounded' },
    );
    const handlers: readonly (readonly [selector: string, write: (target: Element) => void])[] = [
        ...Option.toArray(Option.map(title, (text) => ['title', (target: Element): Element => target.setInnerContent(text)] as const)),
        ...Option.toArray(Option.map(description, (text) => ['meta[name="description"]', (target: Element): Element => target.setAttribute('content', text)] as const)),
        [`#${shell.root}`, (target): Element => target.setInnerContent(markup, { html: true })],
        [`#${shell.data}`, (target): Element => target.setInnerContent(serialized.replaceAll('<', '\\u003c'), { html: true })],
    ];
    return Array.reduce(handlers, new HTMLRewriter(), (rewriter, [selector, write]) => rewriter.on(selector, { element: write })).transform(template);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { renderPage, type Shell };
