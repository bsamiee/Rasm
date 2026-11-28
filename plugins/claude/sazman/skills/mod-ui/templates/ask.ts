import { Answer, ask } from '@rasm/claude-code/ask';
import type { Element } from '@rasm/claude-code/element';
import { host } from '@rasm/claude-code/host';
import type { On } from 'claude-code';
import { Context, Duration, Effect, Layer, Option, Schema, Stream, SubscriptionRef } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WAIT = Duration.minutes(2);

// --- [MODELS] --------------------------------------------------------------------------

const Fetch = Schema.Struct({ url: Schema.String });

// --- [SERVICES] ------------------------------------------------------------------------

class Asking extends Context.Service<Asking, { readonly url: SubscriptionRef.SubscriptionRef<Option.Option<string>> }>()('@rasm/sazman/skills/mod-ui/templates/ask/Asking') {
    static readonly layer = Layer.effect(
        Asking,
        Effect.map(SubscriptionRef.make(Option.none<string>()), (url) => ({ url })),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const held = (url: Option.Option<string>) => Asking.use((asking) => SubscriptionRef.set(asking.url, url));
const decide = (url: string) =>
    ask({
        question: `Fetch ${url}?`,
        header: 'Fetch',
        options: [
            { label: 'Fetch', description: 'Run the call as the model wrote it' },
            { label: 'Skip', description: 'Refuse it, and the model reads the refusal' },
        ],
    }).pipe(
        Effect.timeoutOption(WAIT),
        Effect.map(
            Option.match({
                onNone: () => Option.some(`no answer in ${Duration.format(WAIT)}`),
                onSome: Answer.$match({
                    Picked: ({ labels }) => (labels.includes('Fetch') ? Option.none() : Option.some('the person skipped it')),
                    Other: ({ text }) => Option.some(`the person answered: ${text}`),
                    Noted: ({ notes }) => Option.some(`the person noted: ${notes}`),
                    Declined: () => Option.some('the person dismissed the question'),
                }),
            }),
        ),
        Effect.ensuring(held(Option.none())),
        Effect.catch((error) => Effect.as(Effect.logWarning(`ask for ${url} failed`, error), Option.none<string>())),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: On) {
    host(on, {
        layer: Asking.layer,
        hooks: {
            'tool.call': (e, next) =>
                Option.match(e.tool === 'WebFetch' ? Schema.decodeUnknownOption(Fetch)(e) : Option.none(), {
                    onNone: () => next(e),
                    onSome: Effect.fnUntraced(function* ({ url }) {
                        yield* held(Option.some(url));
                        const refused = yield* decide(url);
                        return Option.isNone(refused) ? yield* next(e) : { deny: `WebFetch ${url}: refused, ${refused.value}` };
                    }),
                }),
        },
        render: {
            state: Stream.unwrap(Asking.use((asking) => Effect.succeed(asking.url.pipe(SubscriptionRef.changes)))),
            AskUserQuestion: (_e, url, next) =>
                Option.match(url, {
                    onNone: () => next(),
                    onSome: (asked) => Effect.map(next(), (dialog): Element => ({ _tag: 'Box', flexDirection: 'column', children: [{ _tag: 'Text', dimColor: true, wrap: 'truncate-middle', children: `asked before the model fetches ${asked}` }, dialog] })),
                }),
        },
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
