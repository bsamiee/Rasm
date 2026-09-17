// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Effect, Option, Order, Record, Schema, String } from 'effect';
import { XMLParser } from 'fast-xml-parser';
import { IndentationText, type ManipulationSettings, NewLineKind, QuoteKind } from 'ts-morph';

// --- [TYPES] ---------------------------------------------------------------------------

type Dictionary = (typeof Dictionary)['Type'];
type SdefClass = (typeof Class)['Type'];
type SdefProperty = (typeof Property)['Type'];
type SdefEnumeration = (typeof Enumeration)['Type'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ACRONYM = /^[A-Z][A-Z0-9]*$/u;
const _BYTE = 256;
const MANIPULATION: Partial<ManipulationSettings> = { indentationText: IndentationText.FourSpaces, newLineKind: NewLineKind.LineFeed, quoteKind: QuoteKind.Single };

// --- [MODELS] --------------------------------------------------------------------------

const _children = <S extends Schema.Top>(schema: S): Schema.withDecodingDefaultKey<Schema.$Array<S>> => Schema.Array(schema).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));
const _optionalString = Schema.OptionFromOptionalKey(Schema.String);
const _yes = Schema.OptionFromOptionalKey(Schema.Literal('yes'));
const _named = { name: Schema.String, code: Schema.String, description: _optionalString, hidden: _yes };
const _Named = Schema.Struct(_named);
const Property = Schema.Struct({
    attributes: Schema.Struct({ ..._named, type: _optionalString, access: Schema.OptionFromOptionalKey(Schema.Literal('r')) }),
    type: _children(Schema.Struct({ attributes: Schema.Struct({ type: Schema.String, list: _yes }) })),
});
const Class = Schema.Struct({ attributes: Schema.Struct({ ..._named, inherits: _optionalString, plural: _optionalString }), property: _children(Property) });
const Enumeration = Schema.Struct({ attributes: _Named, enumerator: _children(Schema.Struct({ attributes: _Named })) });
const Dictionary = Schema.Struct({
    dictionary: Schema.Struct({
        attributes: Schema.OptionFromOptionalKey(Schema.Struct({ title: Schema.String })),
        suite: Schema.Array(Schema.Struct({ class: _children(Class), enumeration: _children(Enumeration) })),
    }),
});

const _parser = new XMLParser({
    ignoreAttributes: false,
    attributeNamePrefix: '',
    attributesGroupName: 'attributes',
    ignoreDeclaration: true,
    trimValues: false,
    parseTagValue: false,
    parseAttributeValue: false,
    isArray: (tag): boolean => Array.contains(['suite', 'class', 'property', 'type', 'enumeration', 'enumerator'], tag),
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

const fourcc = (code: string): number => Array.reduce(code.split(''), 0, (total, character) => total * _BYTE + character.charCodeAt(0));

const sorted = (names: Iterable<string>): readonly string[] => Array.sort(Array.dedupe(Array.fromIterable(names)), Order.String);

// --- [RECONCILIATION] ------------------------------------------------------------------

const absent = (self: Readonly<Record<string, readonly string[]>>, that: Readonly<Record<string, readonly string[]>>): Readonly<Record<string, Array.NonEmptyReadonlyArray<string>>> =>
    Record.getSomes(
        Record.map(self, (members, name) =>
            Option.liftPredicate(Array.isReadonlyArrayNonEmpty<string>)(
                Array.difference(
                    members,
                    Option.getOrElse(Record.get(that, name), () => []),
                ),
            ),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Dictionary, SdefClass, SdefEnumeration, SdefProperty };
export { absent, camel, dictionary, fourcc, MANIPULATION, pascal, sorted };
