import { assert, it } from '@effect/vitest';
import { Shell } from '@rasm/hosting/shell';
import { Template } from '@rasm/hosting/template';
import { Array, Cause, Deferred, Effect, Layer, Option, Predicate, Schema, Sink, Stream, String } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HttpEffect, HttpServerResponse } from 'effect/http';
import { createElement, Suspense, use } from 'react';

// --- [OPERATIONS] ----------------------------------------------------------------------

const Sheet = ({ drawn }: { readonly drawn: Promise<string> }) => createElement('p', null, use(drawn));
const served = Effect.fn('served')(
    function* (drawn: Effect.Effect<string>, policy?: Shell<typeof Schema.String>['policy']) {
        const sheet = Effect.runPromiseWith(yield* Effect.context<never>())(drawn);
        const shell = new Shell({
            name: 'drawings',
            schema: Schema.String,
            render: (title) => createElement('main', null, createElement('title', null, title), createElement('meta', { name: 'description', content: 'Sections and plans' }), createElement(Suspense, { fallback: 'Drawing sheet' }, createElement(Sheet, { drawn: sheet }))),
            ...(policy !== undefined && { policy }),
        });
        const response = HttpServerResponse.toWeb(yield* Template.render(shell, '</script><script>alert(1)</script>', { 'connect-src': ['https://bucket.example'] }));
        const body = yield* Effect.fromNullishOr(response.body);
        return { shell, headers: response.headers, text: Stream.fromReadableStream({ evaluate: () => body.pipeThrough(new TextDecoderStream()), onError: (error) => new Cause.UnknownError(error, 'response body read') }) };
    },
    Effect.provide(
        Layer.merge(
            Template.layer({ fetch: HttpEffect.toWebHandler(Effect.succeed(HttpServerResponse.html('<!doctype html><html lang="fa"><head><meta charset="UTF-8"><link rel="icon" href="/favicon.ico"><script type="module" src="/assets/main-4f2a.js"></script></head><body></body></html>'))) }),
            SecureRandom.layerWebCrypto,
        ),
    ),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.effect(
    'sends the document head and the fallback of suspended content before that content settles, then the content',
    Effect.fnUntraced(function* () {
        const drawn = yield* Deferred.make<string>();
        const { text } = yield* served(Deferred.await(drawn));
        const [shell, rest] = yield* Stream.peel(
            text,
            Sink.reduceWhile(() => '', Predicate.not(String.includes('Drawing sheet')), String.concat),
        );
        assert.deepStrictEqual({ module: shell.includes('<script type="module" src="/assets/main-4f2a.js"></script>'), fallback: shell.includes('Drawing sheet'), settled: shell.includes('Sheet A-101') }, { module: true, fallback: true, settled: false });
        yield* Deferred.succeed(drawn, 'Sheet A-101');
        assert.include(yield* Stream.mkString(rest), 'Sheet A-101');
    }),
);

it.effect(
    'renders one document in the template language whose head holds the template tags and the metadata the page states',
    Effect.fnUntraced(function* () {
        const { headers, text } = yield* served(Effect.succeed('Sheet A-101'));
        const page = yield* Stream.mkString(text);
        const head = page.slice(page.indexOf('<head>'), page.indexOf('</head>'));
        assert.deepStrictEqual(
            {
                type: headers.get('content-type'),
                language: /^<!doctype html><html lang="fa">/iu.test(page),
                icon: head.includes('<link rel="icon" href="/favicon.ico">'),
                titles: page.split('<title>').length - 1,
                title: head.includes('<title>&lt;/script&gt;&lt;script&gt;alert(1)&lt;/script&gt;</title>'),
                description: /<meta name="description" content="Sections and plans"\/?>/u.test(head),
            },
            { type: 'text/html; charset=utf-8', language: true, icon: true, titles: 1, title: true, description: true },
        );
    }),
);

it.effect(
    'carries request data in a script no value closes, decoding to the value served',
    Effect.fnUntraced(function* () {
        const { shell, text } = yield* served(Effect.succeed('Sheet A-101'));
        const page = yield* Stream.mkString(text);
        const open = `<script id="${shell.data.id}" type="application/json">`;
        const data = page.slice(page.indexOf(open) + open.length);
        const json = data.slice(0, data.indexOf('</script>'));
        assert.deepStrictEqual({ scripts: page.split('<script>alert(1)').length - 1, value: Schema.decodeOption(Schema.fromJsonString(Schema.Unknown))(json) }, { scripts: 0, value: Option.some('</script><script>alert(1)</script>') });
    }),
);

it.effect(
    'states the content security policy with the sources of the request and one nonce every inline script it sends holds',
    Effect.fnUntraced(function* () {
        const drawn = yield* Deferred.make<string>();
        const { headers, text } = yield* served(Deferred.await(drawn), { 'script-src': ["'self'"], 'connect-src': ["'self'"] });
        const policy = yield* Effect.fromNullishOr(headers.get('content-security-policy'));
        const nonce = yield* Effect.fromOption(Option.flatMap(String.match(/'nonce-(?<nonce>[^']+)'/u)(policy), Array.get(1)));
        yield* Deferred.succeed(drawn, 'Sheet A-101');
        const inline = (yield* Stream.mkString(text))
            .split('<script')
            .slice(1)
            .map((tag) => tag.slice(0, tag.indexOf('>')))
            .filter((attributes) => !/src=|application\/json/u.test(attributes));
        const directives = Object.fromEntries(
            policy
                .split(';')
                .map((directive) => String.split(String.trim(directive), /\s+/u))
                .map(([name, ...sources]) => [name, new Set(sources)]),
        );
        assert.deepStrictEqual({ directives, held: Array.dedupe(inline.map(String.includes(`nonce="${nonce}"`))) }, { directives: { 'script-src': new Set(["'self'", `'nonce-${nonce}'`]), 'connect-src': new Set(["'self'", 'https://bucket.example']) }, held: [true] });
    }),
);

it.effect(
    'sets no content security policy for a shell stating none',
    Effect.fnUntraced(function* () {
        const { headers } = yield* served(Effect.succeed('Sheet A-101'));
        assert.isFalse(headers.has('content-security-policy'));
    }),
);
