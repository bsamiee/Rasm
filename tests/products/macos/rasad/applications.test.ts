import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { layer as services } from '@effect/platform-node/NodeServices';
import { assert, layer } from '@effect/vitest';
import { Array, Context, Effect, Layer, Option, Predicate, Schema, Struct } from 'effect';
import { type Bundle, served, text } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CLAIM = { server: 'vellum', scope: 'drives Quillwright press layouts through its own plug-in' } as const;
const BUNDLES: readonly Bundle[] = [
    { name: 'Tidewater', identifier: 'net.harrowfield.tidewater', window: 'Tidewater ledger' },
    { name: 'Basalt', identifier: 'net.harrowfield.basalt', window: 'Basalt survey' },
    { name: 'Lantern Press', identifier: 'org.quillwright.press.lantern' },
    { name: 'Scriptorium', identifier: 'org.quillwright.scriptorium', declared: { activationPolicy: 'prohibited', claim: { prefixes: ['org.quillwright.press.'], ...CLAIM } } },
];

// --- [MODELS] --------------------------------------------------------------------------

const Issued = Schema.TemplateLiteralParser(['handle ', Schema.String, ' holds your state, pass it on each later call']);

// --- [SERVICES] ------------------------------------------------------------------------

class Suite extends Context.Service<Suite>()('@rasm/test-rasad/applications.test/Suite', { make: served('@rasm/rasad', 'Rasad', BUNDLES, []) }) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const called = Effect.fnUntraced(function* (client: Effect.Success<Suite['Service']['connect']>, name: string, given: Readonly<Record<string, unknown>>) {
    const tool = yield* client.listTools().pipe(Effect.map(Array.findFirst((listed) => listed.name === name)), Effect.flatMap(Effect.fromOption));
    const result = yield* client.callTool({ tool, arguments: given });
    return { failed: result.isError === true, text: text(result) };
});
const handle = (lines: string) => Effect.map(Effect.flatMap(Effect.fromOption(Array.head(lines.split('\n'))), Schema.decodeUnknownEffect(Issued)), ([, issued]) => issued);
const naming = (value: unknown, name: string): readonly unknown[] => (Predicate.isObjectOrArray(value) ? [...(Object.values(value).includes(name) && !Array.isArray(value) ? [value] : []), ...Object.values(value).flatMap((each) => naming(each, name))] : []);

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(Layer.effect(Suite, Suite.make).pipe(Layer.provide([services, layerUndici])), { excludeTestServices: true })('built Rasad helper', (it) => {
    it.effect(
        "gives each of two clients the window its own opened application reports and never the other client's",
        Effect.fnUntraced(function* () {
            const { connect } = yield* Suite;
            const [first, second] = yield* Effect.all([connect, connect]);
            const [firstHandle, secondHandle] = yield* Effect.all([Effect.flatMap(called(first, 'applications', {}), ({ text: lines }) => handle(lines)), Effect.flatMap(called(second, 'applications', {}), ({ text: lines }) => handle(lines))]);
            const firstOpened = yield* called(first, 'applications', { action: 'open', application: 'Tidewater', handle: firstHandle });
            const secondOpened = yield* called(second, 'applications', { action: 'open', application: 'Basalt', handle: secondHandle });
            const [firstNext, secondNext] = yield* Effect.all([called(first, 'applications', { handle: firstHandle }), called(second, 'applications', { handle: secondHandle })]);
            const [firstSeen, secondSeen] = [`${firstOpened.text}\n${firstNext.text}`, `${secondOpened.text}\n${secondNext.text}`];
            assert.deepStrictEqual({ first: [firstSeen.includes('Tidewater ledger'), firstSeen.includes('Basalt survey')], second: [secondSeen.includes('Basalt survey'), secondSeen.includes('Tidewater ledger')] }, { first: [true, false], second: [true, false] }, `${firstSeen}\n---\n${secondSeen}`);
        }, Effect.scoped),
    );

    it.effect(
        'lists an application another installed product claims with that claim’s server, and refuses to open it with a text naming that server and its scope, launching nothing',
        Effect.fnUntraced(function* () {
            const { connect, launches } = yield* Suite;
            const client = yield* connect;
            const listed = yield* called(client, 'applications', { action: 'list' });
            const listing = yield* Effect.fromOption(Option.flatMap(Array.last(listed.text.split('\n')), Schema.decodeUnknownOption(Schema.fromJsonString(Schema.Unknown))));
            const carries = (name: string) => naming(listing, name).some((row) => JSON.stringify(row).includes(CLAIM.server));
            const before = yield* launches;
            const refused = yield* called(client, 'applications', { action: 'open', application: 'Lantern Press' });
            const after = yield* launches;
            assert.deepStrictEqual(
                { listed: { claimed: carries('Lantern Press'), unclaimed: carries('Tidewater') }, refused: { failed: refused.failed, server: refused.text.includes(CLAIM.server), scope: refused.text.includes(CLAIM.scope) }, launched: Array.drop(after, before.length) },
                { listed: { claimed: true, unclaimed: false }, refused: { failed: true, server: true, scope: true }, launched: [] },
                `${listed.text}\n---\n${refused.text}`,
            );
        }, Effect.scoped),
    );

    it.effect(
        'watches the instance a handle opens once, and reads it from the running listing on each later tool call',
        Effect.fnUntraced(function* () {
            const { connect, watched } = yield* Suite;
            const client = yield* connect;
            const issued = yield* Effect.flatMap(called(client, 'applications', {}), ({ text: lines }) => handle(lines));
            yield* called(client, 'applications', { action: 'open', application: 'Tidewater', handle: issued });
            const opened = yield* watched;
            const later = yield* Effect.all([called(client, 'view', { of: 'object', id: 'ledger', handle: issued }), called(client, 'view', { of: 'object', id: 'ledger', handle: issued })]);
            const after = yield* watched;
            assert.deepStrictEqual({ opened: opened.length > 0, later: Array.drop(after, opened.length), refused: later.map(({ text: lines }) => lines.includes('No linked source renders objects of Tidewater')) }, { opened: true, later: [], refused: [true, true] }, later.map(Struct.get('text')).join('\n---\n'));
        }, Effect.scoped),
    );
});
