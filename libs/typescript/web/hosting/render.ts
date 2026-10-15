import { Effect, Formatter, Option, Schema } from 'effect';
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
    Unstarted: { cause: Schema.Defect() },
    Incomplete: { cause: Schema.Defect() },
});
class RenderError extends Schema.TaggedError<RenderError, { readonly brand: unique symbol }>()('RenderError', { reason: RenderReason }) {
    override get message(): string {
        return RenderReason.match(this.reason, {
            Untemplated: ({ url, cause }) => `the page template at ${url} could not be fetched: ${Formatter.format(cause)}`,
            Unstarted: ({ cause }) => `React could not start rendering the page: ${Formatter.format(cause)}`,
            Incomplete: ({ cause }) => `React could not finish rendering the page: ${Formatter.format(cause)}`,
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fetchTemplate = (assets: Fetcher, url: string): Effect.Effect<Response, RenderError> => Effect.tryPromise({ try: () => assets.fetch(url), catch: (cause) => RenderError.make({ reason: { _tag: 'Untemplated', url, cause } }) });

const renderPage = Effect.fnUntraced(function* <S extends Schema.Codec<unknown, unknown>>(
    shell: Shell<S>,
    { template, value, element, title, description }: { readonly template: Response; readonly value: S['Type']; readonly element: ReactNode; readonly title: Option.Option<string>; readonly description: Option.Option<string> },
): Effect.fn.Return<Response, RenderError | Schema.SchemaError> {
    const serialized = yield* Schema.encodeEffect(Schema.fromJsonString(shell.schema))(value);
    const markup = yield* Effect.tryPromise({ try: (signal) => renderToReadableStream(element, { signal }), catch: (cause) => RenderError.make({ reason: { _tag: 'Unstarted', cause } }) });
    yield* Effect.tryPromise({ try: () => markup.allReady, catch: (cause) => RenderError.make({ reason: { _tag: 'Incomplete', cause } }) });
    return new HTMLRewriter()
        .on('title', {
            element: (target): void => {
                if (Option.isSome(title)) {
                    target.setInnerContent(title.value);
                }
            },
        })
        .on('meta[name="description"]', {
            element: (target): void => {
                if (Option.isSome(description)) {
                    target.setAttribute('content', description.value);
                }
            },
        })
        .on(`#${shell.root}`, {
            element: (target): void => {
                target.setInnerContent(markup, { html: true });
            },
        })
        .on(`#${shell.data}`, {
            element: (target): void => {
                target.setInnerContent(serialized.replaceAll('<', '\\u003c'), { html: true });
            },
        })
        .transform(template);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { fetchTemplate, renderPage, type Shell };
