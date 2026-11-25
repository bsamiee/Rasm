import { assert, describe, it } from '@effect/vitest';
import { type Containment, encode, format, parse, reference, walk } from '@rasm/object-model/host';
import { Arbitrary, Effect, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTILE = ['Plan "B"', 'C:\\Proofs\\', String.raw`\u0041 stays text`, 'Plan to print', 'cut]here', '(open', '日本語'] as const;
const INK = { kind: 'layer', name: 'Ink', layers: { length: 0 } };
const ART = { kind: 'layer', name: 'Art', layers: { length: 1, 0: INK } };
const PAGES = [{ kind: 'page', name: 'i' }, { kind: 'page', name: 'ii' }, { kind: 'page' }] as const;
const COVER = { kind: 'document', name: 'cover.indd', pages: { length: 3, 1: PAGES[0], 2: PAGES[1], 3: PAGES[2] }, layers: { length: 1, 0: ART } };
const PROOF = { kind: 'document', name: 'Plan "B" to print]', pages: { length: 0 }, layers: { length: 0 } };
const OWNERS = new Map<object, object>([[INK, ART], [ART, COVER], ...PAGES.map((page) => [page, COVER] as const)]);
const HELD: Containment = {
    root: { documents: { length: 2, 0: COVER, 1: PROOF } },
    type: () => 'Object',
    kinds: {
        document: { collection: 'documents', keys: ['name'], is: (value) => Reflect.get(value, 'kind') === 'document' },
        page: { collection: 'pages', base: 1, keys: ['name'], is: (value) => Reflect.get(value, 'kind') === 'page', owner: (value) => OWNERS.get(value) },
        layer: { collection: 'layers', nested: true, keys: ['name'], is: (value) => Reflect.get(value, 'kind') === 'layer', owner: (value) => OWNERS.get(value) },
    },
};

// --- [MODELS] --------------------------------------------------------------------------

const Kind = Schema.String.check(Schema.isPattern(/^@?[A-Za-z][\w-]*$/u));
const Step = Schema.Struct({
    separator: Schema.Literals(['/', '//']),
    kind: Kind,
    selector: Schema.optionalKey(Schema.Union([Schema.TaggedStruct('Index', { index: Schema.Int }), Schema.TaggedStruct('Key', { key: Kind.check(Schema.isPattern(/^[A-Za-z]/u)), value: Schema.Union([Schema.Int, Schema.String, Schema.Literals(HOSTILE)]) })])),
});
const Path = Schema.Union([Schema.Struct({ steps: Schema.Array(Step) }), Schema.Struct({ steps: Schema.NonEmptyArray(Step), end: Schema.NonEmptyArray(Step) })]);
const Refusal = Schema.TaggedStruct('Unmatched', { query: Schema.String, candidates: Schema.Array(Schema.String) });
const Plain = Arbitrary.schema(Schema.Json).pipe(Arbitrary.map(structuredClone));

// --- [OPERATIONS] ----------------------------------------------------------------------

const walked = (specifier: string) => {
    const path = parse(specifier);
    assert.isDefined(path, specifier);
    return walk(HELD, path);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('object specifier', () => {
    it.prop('reads back every path it writes, quotes, backslashes, escape-like text, and ranges included', { path: Path }, ({ path }) => {
        assert.deepStrictEqual(parse(format(path)), path);
    });

    it('reads a range whose first end names an object by a quoted name holding the range word', () => {
        assert.deepStrictEqual(parse(String.raw`(/document[@name="Plan to \"B\""]/page[0] to /document[0]/page[-1])`), {
            steps: [
                { separator: '/', kind: 'document', selector: { _tag: 'Key', key: 'name', value: 'Plan to "B"' } },
                { separator: '/', kind: 'page', selector: { _tag: 'Index', index: 0 } },
            ],
            end: [
                { separator: '/', kind: 'document', selector: { _tag: 'Index', index: 0 } },
                { separator: '/', kind: 'page', selector: { _tag: 'Index', index: -1 } },
            ],
        });
    });

    it.each([
        { side: 'an empty end', specifier: '(/document[0]/page[0] to )' },
        { side: 'an empty start', specifier: '( to /document[0]/page[-1])' },
        { side: 'two empty ends', specifier: '( to )' },
    ])('refuses $specifier, a range with $side', ({ specifier }) => {
        assert.isUndefined(parse(specifier));
    });

    it.each([
        { specifier: '/document[@name="cover.indd"]/page[-1]', found: PAGES[2] },
        { specifier: '/document[0]/page[0]', found: PAGES[0] },
        { specifier: '/document[@name="cover.indd"]/layer[@name="Ink"]', found: INK },
    ])('walks $specifier to the object it names', ({ specifier, found }) => {
        assert.strictEqual(walked(specifier), found);
    });

    it.effect.each([
        { specifier: '/document[@name="back.indd"]', candidates: ['cover.indd', PROOF.name] },
        { specifier: '/document[0]/page[-4]', candidates: [] },
        { specifier: '/folder[0]', candidates: ['document', 'page', 'layer'] },
    ])(
        'answers $specifier, which names no held object, as unmatched with its query and the names it could have taken',
        Effect.fnUntraced(function* ({ specifier, candidates }) {
            const refused = yield* Schema.decodeUnknownEffect(Refusal)(walked(specifier));
            assert.strictEqual(refused.query, specifier);
            assert.sameMembers([...refused.candidates], candidates);
        }),
    );

    it.each([
        { held: 'a document whose name holds a quote, a range word, and a bracket', object: PROOF, name: PROOF.name },
        { held: 'a named page', object: PAGES[1], name: 'ii' },
        { held: 'a page with no key', object: PAGES[2], name: undefined },
        { held: 'a layer inside a layer', object: INK, name: 'Ink' },
    ])('names $held by a specifier that walks back to it, with its type, validity, and name', ({ object, name }) => {
        const named = reference(HELD, object);
        assert.isDefined(named);
        assert.deepStrictEqual([named.type, named.valid, named.name], ['Object', true, name]);
        assert.strictEqual(walked(named.specifier), object);
    });
});

describe(encode, () => {
    it.prop('writes every finite number, string, list, and record as the JSON value JSON.stringify writes for it', { tree: Plain }, ({ tree }) => {
        const encoded = encode(tree, { reference: () => undefined });
        assert.isTrue(Schema.is(Schema.Json)(encoded));
        assert.strictEqual(JSON.stringify(encoded), JSON.stringify(tree));
    });

    it.each([
        { held: 'a record', text: '{"__proto__":{"tint":40},"name":"Swatch"}' },
        { held: 'a number', text: '{"__proto__":7,"name":"Swatch"}' },
    ])('writes an own __proto__ key holding $held as a key of the record', ({ text }) => {
        assert.strictEqual(JSON.stringify(encode(JSON.parse(text), { reference: () => undefined })), text);
    });

    it('writes a record holding a constructor key as that record', () => {
        assert.deepStrictEqual(encode({ constructor: 'Swatch', tint: 40 }, { reference: () => undefined }), { constructor: 'Swatch', tint: 40 });
    });

    it('writes NaN and both infinities as null, as JSON has no value for them', () => {
        assert.deepStrictEqual(encode({ ratio: Number.NaN, bounds: [Infinity, -Infinity, 0.5] }, { reference: () => undefined }), { ratio: null, bounds: [null, null, 0.5] });
    });
});
