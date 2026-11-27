import { assert, describe, it } from '@effect/vitest';
import { Entry } from '@rasm/applications/catalog';
import { Effect, Option, Record, Struct } from 'effect';

const form = (name: string, product: string, identifier: string, folder: string): Entry => ({
    url: new URL(`file:///Applications/${encodeURIComponent(folder)}/${encodeURIComponent(name)}.app/`),
    identifier,
    name,
    product,
    aliases: [],
    facts: {},
    claim: Option.none(),
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
const LOOKUPS = Record.map(NAMED, (_, query) => Entry.lookup(CATALOG, query));

describe('workspace', () => {
    it.effect('takes the first listed form of a product for a bare product name, and a later form only by its exact name or identifier', () =>
        Effect.gen(function* () {
            const found = yield* Effect.all(LOOKUPS);
            assert.deepStrictEqual(Record.map(found, Struct.get('name')), NAMED);
        }),
    );
    it.effect('refuses words two products share, naming each, and words no listed name holds', () =>
        Effect.gen(function* () {
            const [shared, none] = yield* Effect.all([Entry.lookup(CATALOG, 'Acrobat').pipe(Effect.flip), Entry.lookup(CATALOG, 'Lightroom').pipe(Effect.flip)]);
            assert.deepStrictEqual({ shared: shared.candidates, none: Option.getOrUndefined(none.query) }, { shared: ['Adobe Acrobat', 'Acrobat Distiller'], none: 'Lightroom' });
        }),
    );
});
