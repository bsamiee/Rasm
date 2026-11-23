import { Shell } from '@rasm/hosting/shell';
import { Template } from '@rasm/hosting/template';
import { Effect, Layer, Option, Schema } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HttpServerResponse } from 'effect/http';
import { createElement, Suspense, use } from 'react';
import { expect, it } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const built = '<!doctype html><html lang="fa"><head><meta charset="UTF-8"><link rel="icon" href="/favicon.ico"><script type="module" src="/assets/main-4f2a.js"></script></head><body></body></html>';

// --- [OPERATIONS] ----------------------------------------------------------------------

const Sheet = ({ drawn }: { readonly drawn: Promise<string> }) => createElement('p', null, use(drawn));
const served = (drawn: Promise<string>, policy?: Shell<typeof Schema.String>['policy']) => {
    const shell = new Shell({
        name: 'drawings',
        schema: Schema.String,
        render: (title) => createElement('main', null, createElement('title', null, title), createElement('meta', { name: 'description', content: 'Sections and plans' }), createElement(Suspense, { fallback: 'Drawing sheet' }, createElement(Sheet, { drawn }))),
        ...(policy !== undefined && { policy }),
    });
    const layer = Layer.merge(Template.layer({ fetch: () => Promise.resolve(new Response(built)) }), SecureRandom.layerWebCrypto);
    return Effect.runPromise(Effect.map(Effect.provide(Template.render(shell, '</script><script>alert(1)</script>', { 'connect-src': ['https://bucket.example'] }), layer), (response) => ({ shell, response: HttpServerResponse.toWeb(response) })));
};
const read = async (body: ReadableStreamDefaultReader<string>, until: string, text = ''): Promise<string> => {
    const { done, value = '' } = await body.read();
    return done || `${text}${value}`.includes(until) ? `${text}${value}` : read(body, until, `${text}${value}`);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

it('sends the document head and the fallback of suspended content before that content settles, then the content', async () => {
    const drawn = Promise.withResolvers<string>();
    const { response } = await served(drawn.promise);
    const body = response.body?.pipeThrough(new TextDecoderStream()).getReader();
    const shell = await read(body ?? expect.fail('response holds no body'), 'Drawing sheet');
    expect({ module: shell.includes('<script type="module" src="/assets/main-4f2a.js"></script>'), fallback: shell.includes('Drawing sheet'), settled: shell.includes('Sheet A-101') }).toStrictEqual({ module: true, fallback: true, settled: false });
    drawn.resolve('Sheet A-101');
    expect(await read(body ?? expect.fail('response holds no body'), '</html>')).toContain('Sheet A-101');
});

it('renders one document in the template language whose head holds the template tags and the metadata the page states', async () => {
    const { response } = await served(Promise.resolve('Sheet A-101'));
    const page = await response.text();
    const head = page.slice(page.indexOf('<head>'), page.indexOf('</head>'));
    expect({
        type: response.headers.get('content-type'),
        language: /^<!doctype html><html lang="fa">/iu.test(page),
        icon: head.includes('<link rel="icon" href="/favicon.ico">'),
        titles: page.split('<title>').length - 1,
        title: head.includes('<title>&lt;/script&gt;&lt;script&gt;alert(1)&lt;/script&gt;</title>'),
        description: /<meta name="description" content="Sections and plans"\/?>/u.test(head),
    }).toStrictEqual({ type: 'text/html; charset=utf-8', language: true, icon: true, titles: 1, title: true, description: true });
});

it('carries request data in a script no value closes, decoding to the value served', async () => {
    const { shell, response } = await served(Promise.resolve('Sheet A-101'));
    const page = await response.text();
    const open = `<script id="${shell.data.id}" type="application/json">`;
    const data = page.slice(page.indexOf(open) + open.length);
    const json = data.slice(0, data.indexOf('</script>'));
    expect({ scripts: page.split('<script>alert(1)').length - 1, value: Schema.decodeOption(shell.data.json)(json) }).toStrictEqual({ scripts: 0, value: Option.some('</script><script>alert(1)</script>') });
});

it('states the content security policy with the sources of the request and one nonce every inline script it sends holds', async () => {
    const drawn = Promise.withResolvers<string>();
    const { response } = await served(drawn.promise, { 'script-src': ["'self'"], 'connect-src': ["'self'"] });
    const policy = response.headers.get('content-security-policy') ?? '';
    const nonce = /'nonce-(?<nonce>[^']+)'/u.exec(policy)?.groups?.['nonce'];
    drawn.resolve('Sheet A-101');
    const page = await response.text();
    const inline = Array.from(page.matchAll(/<script(?<attributes>[^>]*)>/gu), ({ groups }) => groups?.['attributes'] ?? '').filter((attributes) => !attributes.includes('src=') && !attributes.includes('application/json'));
    expect({ policy, inline: inline.length > 0 && inline.every((attributes) => attributes.includes(`nonce="${nonce}"`)) }).toStrictEqual({ policy: `script-src 'self' 'nonce-${nonce}'; connect-src 'self' https://bucket.example`, inline: true });
});

it('sets no content security policy for a shell stating none', async () => {
    const { response } = await served(Promise.resolve('Sheet A-101'));
    expect(response.headers.has('content-security-policy')).toBe(false);
});
