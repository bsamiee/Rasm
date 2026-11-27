import { assert, describe, it } from '@effect/vitest';
import { Event } from '@rasm/applications/instances';
import { type Entry, follow, lookupEntry, ownsEvent } from '@rasm/applications/workspace';
import { Handle } from '@rasm/mcp/server';
import { Context, Effect, MutableRef, Option, Record, Schema, Struct } from 'effect';

const form = (name: string, product: string, identifier: string, folder: string): Entry => ({
    url: new URL(`file:///Applications/${encodeURIComponent(folder)}/${encodeURIComponent(name)}.app/`),
    identifier,
    name,
    product,
    aliases: [],
    facts: {},
    claim: Option.none(),
});
const asking = (instance: string) => Schema.decodeEffect(Event)({ instance, sequence: 1, at: '2026-11-26T09:30:00Z', change: { _tag: 'asking', container: 3, kind: 'sheet', title: 'Save', text: ['Do you want to save the changes?'], answers: [], default: 4, cancel: 5 } });
const handleOver = (state: MutableRef.MutableRef<Context.Context<never>>) => ({
    get: <A>(key: Context.Reference<A>) => Effect.sync(() => Context.get(MutableRef.get(state), key)),
    set: <A>(key: Context.Reference<A>, value: A) => Effect.asVoid(Effect.sync(() => MutableRef.set(state, Context.add(MutableRef.get(state), key, value)))),
});
const NAMED = {
    Illustrator: 'Adobe Illustrator 2027 (Beta)',
    'Adobe Illustrator': 'Adobe Illustrator 2027 (Beta)',
    'illustrator 2027': 'Adobe Illustrator 2027 (Beta)',
    ILST: 'Adobe Illustrator 2027 (Beta)',
    Photoshop: 'Adobe Photoshop (Beta)',
    'Adobe Illustrator 2027': 'Adobe Illustrator 2027',
    'com.adobe.illustrator': 'Adobe Illustrator 2027',
    'Adobe Illustrator 2027 (Beta)': 'Adobe Illustrator 2027 (Beta)',
} as const;
const CATALOG = [
    form('Adobe Acrobat', 'com.adobe.Acrobat.Pro', 'com.adobe.Acrobat.Pro', 'Adobe Acrobat DC'),
    form('Acrobat Distiller', 'com.adobe.distiller', 'com.adobe.distiller', 'Adobe Acrobat DC'),
    form('Adobe Illustrator 2027 (Beta)', 'ILST', 'com.adobe.illustratorBeta', 'Adobe Illustrator (Beta)'),
    form('Adobe Illustrator 2027', 'ILST', 'com.adobe.illustrator', 'Adobe Illustrator 2027'),
    form('Adobe Photoshop (Beta)', 'PHSP', 'com.adobe.Photoshop', 'Adobe Photoshop (Beta)'),
];
const LOOKUPS = Record.map(NAMED, (_, query) => lookupEntry(CATALOG, query));

describe('workspace', () => {
    it.effect('takes the first listed form of a product for a bare product name, and a later form only by its exact name or identifier', () =>
        Effect.gen(function* () {
            const found = yield* Effect.all(LOOKUPS);
            assert.deepStrictEqual(Record.map(found, Struct.get('name')), NAMED);
        }),
    );
    it.effect('refuses words two products share, naming each, and words no listed name holds', () =>
        Effect.gen(function* () {
            const [shared, none] = yield* Effect.all([lookupEntry(CATALOG, 'Acrobat').pipe(Effect.flip), lookupEntry(CATALOG, 'Lightroom').pipe(Effect.flip)]);
            assert.deepStrictEqual({ shared: shared.candidates, none: Option.getOrUndefined(none.query) }, { shared: ['Adobe Acrobat', 'Acrobat Distiller'], none: 'Lightroom' });
        }),
    );
    it.effect('owns the save-sheet asking event of an instance close follows though this handle never opened or targeted it', () =>
        Effect.gen(function* () {
            const state = MutableRef.make(Context.empty());
            const [followed, other] = yield* Effect.all([asking('512@1'), asking('513@1')]);
            yield* Effect.provideService(follow(followed.instance, true), Handle, handleOver(state));
            assert.deepStrictEqual({ followed: ownsEvent({ event: followed }, MutableRef.get(state)), other: ownsEvent({ event: other }, MutableRef.get(state)) }, { followed: true, other: false });
        }),
    );
});
