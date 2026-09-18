// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Effect, Function, Option, Order, pipe, Record, Result, Schema, String, Struct } from 'effect';
import { XMLParser } from 'fast-xml-parser';
import { IndentationText, type ManipulationSettings, NewLineKind, QuoteKind } from 'ts-morph';
import { OptionalString } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Dictionary = (typeof Dictionary)['Type'];
type SdefClass = (typeof Class)['Type'];
type SdefProperty = (typeof Property)['Type'];
type SdefEnumeration = (typeof Enumeration)['Type'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ACRONYM = /^[A-Z][A-Z0-9]*$/u;
const MANIPULATION: Partial<ManipulationSettings> = { indentationText: IndentationText.FourSpaces, newLineKind: NewLineKind.LineFeed, quoteKind: QuoteKind.Single };
const SDEF_TYPES: Readonly<Record<string, string>> = {
    any: 'unknown',
    boolean: 'boolean',
    date: 'Date',
    'file specification': 'File',
    integer: 'number',
    list: 'unknown[]',
    real: 'number',
    record: 'unknown',
    specifier: 'unknown',
    text: 'string',
    type: 'string',
};

// --- [MODELS] --------------------------------------------------------------------------

const _children = <S extends Schema.Top>(schema: S): Schema.withDecodingDefaultKey<Schema.$Array<S>> => Schema.Array(schema).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));
const _named: {
    readonly name: Schema.String;
    readonly code: Schema.String;
    readonly description: Schema.OptionFromOptionalKey<Schema.String>;
    readonly hidden: Schema.OptionFromOptionalKey<Schema.Literal<'yes'>>;
} = {
    name: Schema.String,
    code: Schema.String,
    description: OptionalString,
    hidden: Schema.OptionFromOptionalKey(Schema.Literal('yes')),
};
const _Named: Schema.Struct<typeof _named> = Schema.Struct(_named);
const _Type: Schema.Struct<{ readonly attributes: Schema.Struct<{ readonly type: Schema.String; readonly list: Schema.OptionFromOptionalKey<Schema.Literal<'yes'>> }> }> = Schema.Struct({
    attributes: Schema.Struct({ type: Schema.String, list: Schema.OptionFromOptionalKey(Schema.Literal('yes')) }),
});
const Property: Schema.Struct<{
    readonly attributes: Schema.Struct<typeof _named & { readonly type: Schema.OptionFromOptionalKey<Schema.String>; readonly access: Schema.OptionFromOptionalKey<Schema.Literal<'r'>> }>;
    readonly type: Schema.withDecodingDefaultKey<Schema.$Array<typeof _Type>>;
}> = Schema.Struct({
    attributes: Schema.Struct({ ..._named, type: OptionalString, access: Schema.OptionFromOptionalKey(Schema.Literal('r')) }),
    type: _children(_Type),
});
const Class: Schema.Struct<{
    readonly attributes: Schema.Struct<typeof _named & { readonly inherits: Schema.OptionFromOptionalKey<Schema.String>; readonly plural: Schema.OptionFromOptionalKey<Schema.String> }>;
    readonly property: Schema.withDecodingDefaultKey<Schema.$Array<typeof Property>>;
}> = Schema.Struct({
    attributes: Schema.Struct({ ..._named, inherits: OptionalString, plural: OptionalString }),
    property: _children(Property),
});
const Enumeration: Schema.Struct<{
    readonly attributes: typeof _Named;
    readonly enumerator: Schema.withDecodingDefaultKey<Schema.$Array<Schema.Struct<{ readonly attributes: typeof _Named }>>>;
}> = Schema.Struct({
    attributes: _Named,
    enumerator: _children(Schema.Struct({ attributes: _Named })),
});
const Dictionary: Schema.Struct<{
    readonly dictionary: Schema.Struct<{
        readonly attributes: Schema.OptionFromOptionalKey<Schema.Struct<{ readonly title: Schema.String }>>;
        readonly suite: Schema.$Array<
            Schema.Struct<{ readonly class: Schema.withDecodingDefaultKey<Schema.$Array<typeof Class>>; readonly enumeration: Schema.withDecodingDefaultKey<Schema.$Array<typeof Enumeration>> }>
        >;
    }>;
}> = Schema.Struct({
    dictionary: Schema.Struct({
        attributes: Schema.OptionFromOptionalKey(Schema.Struct({ title: Schema.String })),
        suite: Schema.Array(Schema.Struct({ class: _children(Class), enumeration: _children(Enumeration) })),
    }),
});

const _LISTS = Array.difference(
    [
        ...Struct.keys(Dictionary.fields.dictionary.fields),
        ...Struct.keys(Class.fields),
        ...Struct.keys(Property.fields),
        ...Struct.keys(Enumeration.fields),
        ...Struct.keys(Dictionary.fields.dictionary.fields.suite.value.fields),
    ],
    ['attributes'],
);

const _parser = new XMLParser({
    ignoreAttributes: false,
    attributeNamePrefix: '',
    attributesGroupName: 'attributes',
    ignoreDeclaration: true,
    trimValues: false,
    parseTagValue: false,
    parseAttributeValue: false,
    isArray: (tag): boolean => Array.contains(_LISTS, tag),
});

// --- [DICTIONARY] ----------------------------------------------------------------------

const dictionary = (xml: string): Effect.Effect<Dictionary, Schema.SchemaError> => Schema.decodeUnknownEffect(Dictionary)(_parser.parse(xml));

// --- [NAMES] ---------------------------------------------------------------------------

const _words = (name: string): Array.NonEmptyReadonlyArray<string> => String.split(Array.headNonEmpty(String.split(name, '.')), ' ');

const _capitalized = (word: string): string => (_ACRONYM.test(word) ? word : String.capitalize(word));

const pascal = (name: string): string => Array.join(Array.map(_words(name), _capitalized), '');

const camel = (name: string): string => {
    const [head, ...rest] = _words(name);
    return `${_ACRONYM.test(head) ? String.toLowerCase(head) : String.uncapitalize(head)}${Array.join(Array.map(rest, _capitalized), '')}`;
};

const constant = (name: string): string => Array.join(Array.map(_words(name), String.toUpperCase), '_');

const fourcc = (code: string): number => new DataView(new TextEncoder().encode(code).buffer).getUint32(0);

const sorted = (names: Iterable<string>): readonly string[] => Array.sort(Array.dedupe(Array.fromIterable(names)), Order.String);

// --- [RECONCILIATION] ------------------------------------------------------------------

const absent = (self: Readonly<Record<string, readonly string[]>>, that: Readonly<Record<string, readonly string[]>>): Readonly<Record<string, Array.NonEmptyReadonlyArray<string>>> =>
    Record.filterMap(self, (members, name) =>
        pipe(
            Array.difference(
                members,
                Option.getOrElse(Record.get(that, name), () => []),
            ),
            Result.liftPredicate(Array.isReadonlyArrayNonEmpty<string>, Function.constVoid),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Dictionary, SdefClass, SdefEnumeration, SdefProperty };
export { absent, camel, constant, dictionary, fourcc, MANIPULATION, pascal, SDEF_TYPES, sorted };
