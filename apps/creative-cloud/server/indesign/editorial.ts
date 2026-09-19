// --- [IMPORTS] -------------------------------------------------------------------------

import { Data, identity, Record, Schema, SchemaGetter, Struct } from 'effect';
import { type AppliedReply, applied } from '../errors.ts';
import { OptionalInt, OptionalString, PageIndex } from '../values.ts';
import { enumerations, members } from './indesign.ts';

// --- [SELECTIONS] ----------------------------------------------------------------------

const _id: Schema.Int = Schema.Int.pipe(Schema.check(Schema.isGreaterThan(0)));
const _optionalId: Schema.OptionFromOptionalKey<Schema.Int> = Schema.OptionFromOptionalKey(_id);
class NativeConstant extends Data.Class<{ readonly enumeration: string; readonly constant: string }> {}
const _enum = (name: keyof typeof enumerations): Schema.Codec<NativeConstant, string> =>
    Schema.Literals(Record.keys<string, number>(enumerations[name])).pipe(
        Schema.decodeTo(Schema.instanceOf(NativeConstant), {
            decode: SchemaGetter.transform((constant) => new NativeConstant({ enumeration: name, constant })),
            encode: SchemaGetter.transform(Struct.get('constant')),
        }),
    );
const _numberingStyle: ReturnType<typeof _enum> = _enum('NumberingStyle');
const _footnoteStyle: ReturnType<typeof _enum> = _enum('FootnoteNumberingStyle');
const _range: Schema.Struct<{ readonly storyId: Schema.Int; readonly from: Schema.Int; readonly to: Schema.Int }> = Schema.Struct({ storyId: _id, from: PageIndex, to: PageIndex }).pipe(
    Schema.check(Schema.makeFilter(({ from, to }) => from <= to || 'The inclusive range must end at or after its start')),
);
const _direction: Schema.TaggedUnion<{
    readonly document: Schema.TaggedStruct<'document', { readonly value: ReturnType<typeof _enum> }>;
    readonly story: Schema.TaggedStruct<'story', { readonly storyId: Schema.Int; readonly value: ReturnType<typeof _enum> }>;
    readonly paragraphs: Schema.TaggedStruct<'paragraphs', { readonly range: typeof _range; readonly value: ReturnType<typeof _enum> }>;
    readonly characters: Schema.TaggedStruct<'characters', { readonly range: typeof _range; readonly value: ReturnType<typeof _enum> }>;
    readonly table: Schema.TaggedStruct<'table', { readonly storyId: Schema.Int; readonly tableId: Schema.Int; readonly value: ReturnType<typeof _enum> }>;
}> = Schema.TaggedUnion({
    document: { value: _enum('PageBindingOptions') },
    story: { storyId: _id, value: _enum('StoryDirectionOptions') },
    paragraphs: { range: _range, value: _enum('ParagraphDirectionOptions') },
    characters: { range: _range, value: _enum('CharacterDirectionOptions') },
    table: { storyId: _id, tableId: _id, value: _enum('TableDirectionOptions') },
});
const _list: Schema.OptionFromOptionalKey<Schema.Struct<{ readonly name: Schema.NonEmptyString; readonly acrossStories: Schema.Boolean; readonly acrossDocuments: Schema.Boolean }>> =
    Schema.OptionFromOptionalKey(Schema.Struct({ name: Schema.NonEmptyString, acrossStories: Schema.Boolean, acrossDocuments: Schema.Boolean }));
const _numbering: Schema.TaggedUnion<{
    readonly section: Schema.TaggedStruct<'section', { readonly sectionId: Schema.Int; readonly style: ReturnType<typeof _enum> }>;
    readonly chapter: Schema.TaggedStruct<
        'chapter',
        { readonly style: ReturnType<typeof _enum>; readonly source: Schema.OptionFromOptionalKey<ReturnType<typeof _enum>>; readonly number: typeof OptionalInt }
    >;
    readonly footnotes: Schema.TaggedStruct<'footnotes', { readonly style: ReturnType<typeof _enum> }>;
    readonly endnotes: Schema.TaggedStruct<'endnotes', { readonly style: ReturnType<typeof _enum> }>;
    readonly paragraphs: Schema.TaggedStruct<
        'paragraphs',
        {
            readonly range: typeof _range;
            readonly style: ReturnType<typeof _enum>;
            readonly list: typeof _list;
            readonly level: typeof OptionalInt;
            readonly start: typeof OptionalInt;
            readonly expression: typeof OptionalString;
        }
    >;
}> = Schema.TaggedUnion({
    section: { sectionId: _id, style: _enum('PageNumberStyle') },
    chapter: {
        style: _numberingStyle,
        source: Schema.OptionFromOptionalKey(_enum('ChapterNumberSources')),
        number: _optionalId,
    },
    footnotes: { style: _footnoteStyle },
    endnotes: { style: _footnoteStyle },
    paragraphs: {
        range: _range,
        style: _numberingStyle,
        list: _list,
        level: _optionalId,
        start: _optionalId,
        expression: OptionalString,
    },
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const _writes = (keys: Readonly<Record<string, boolean>>): Schema.$Array<Schema.Struct<{ readonly key: Schema.Literals<string[]>; readonly value: Schema.Codec<Schema.Json> }>> =>
    Schema.Array(Schema.Struct({ key: Schema.Literals(Struct.keys(Record.filter(keys, identity))), value: Schema.Json }));

const _rtl: { readonly role: Schema.Literals<readonly ['persian', 'arabic']>; readonly languageId: Schema.Int } = {
    role: Schema.Literals(['persian', 'arabic']),
    languageId: _id,
};
const _rtlStyle: Schema.Struct<typeof _rtl & { readonly name: Schema.NonEmptyString; readonly basedOn: typeof _optionalId; readonly overrides: ReturnType<typeof _writes> }> = Schema.Struct({
    ..._rtl,
    name: Schema.NonEmptyString,
    basedOn: _optionalId,
    overrides: _writes(members.ParagraphStyle),
});

const _query: {
    readonly storyId: typeof OptionalInt;
    readonly query: typeof OptionalString;
    readonly reverseOrder: Schema.Boolean;
} = { storyId: OptionalInt, query: OptionalString, reverseOrder: Schema.Boolean };

const _operation: Schema.TaggedUnion<{
    readonly rtlDefaults: Schema.TaggedStruct<'rtlDefaults', typeof _rtl & { readonly scope: Schema.Literals<readonly ['application', 'document']>; readonly overrides: ReturnType<typeof _writes> }>;
    readonly rtlStyles: Schema.TaggedStruct<'rtlStyles', { readonly styles: Schema.NonEmptyArray<typeof _rtlStyle>; readonly changeComposer: Schema.Boolean }>;
    readonly direction: Schema.TaggedStruct<'direction', { readonly target: typeof _direction }>;
    readonly numbering: Schema.TaggedStruct<'numbering', { readonly target: typeof _numbering }>;
    readonly insertSpecialCharacter: Schema.TaggedStruct<'insertSpecialCharacter', { readonly storyId: Schema.Int; readonly offset: Schema.Int; readonly character: ReturnType<typeof _enum> }>;
    readonly findText: Schema.TaggedStruct<
        'findText',
        typeof _query & { readonly find: ReturnType<typeof _writes>; readonly change: ReturnType<typeof _writes>; readonly options: ReturnType<typeof _writes> }
    >;
    readonly findGrep: Schema.TaggedStruct<
        'findGrep',
        typeof _query & { readonly find: ReturnType<typeof _writes>; readonly change: ReturnType<typeof _writes>; readonly options: ReturnType<typeof _writes> }
    >;
}> = Schema.TaggedUnion({
    rtlDefaults: { ..._rtl, scope: Schema.Literals(['application', 'document']), overrides: _writes(members.TextDefault) },
    rtlStyles: { styles: Schema.NonEmptyArray(_rtlStyle), changeComposer: Schema.Boolean },
    direction: { target: _direction },
    numbering: { target: _numbering },
    insertSpecialCharacter: { storyId: _id, offset: PageIndex, character: _enum('SpecialCharacters') },
    findText: { ..._query, find: _writes(members.FindTextPreference), change: _writes(members.ChangeTextPreference), options: _writes(members.FindChangeTextOption) },
    findGrep: { ..._query, find: _writes(members.FindGrepPreference), change: _writes(members.ChangeGrepPreference), options: _writes(members.FindChangeGrepOption) },
});

const Input: Schema.Struct<{ readonly documentId: typeof OptionalInt; readonly operation: typeof _operation }> = Schema.Struct({ documentId: OptionalInt, operation: _operation });

// --- [RESULTS] -------------------------------------------------------------------------

const Applied: AppliedReply<{ readonly path: Schema.String }> = applied({ path: Schema.String });
const EditorialError: Schema.TaggedUnion<{
    readonly selectionMissing: Schema.TaggedStruct<'selectionMissing', { readonly collection: Schema.String; readonly id: Schema.Int }>;
    readonly rangeOutOfBounds: Schema.TaggedStruct<
        'rangeOutOfBounds',
        {
            readonly storyId: Schema.Int;
            readonly collection: Schema.Literals<readonly ['paragraphs', 'characters', 'insertionPoints']>;
            readonly from: Schema.Int;
            readonly to: Schema.Int;
            readonly count: Schema.Int;
        }
    >;
    readonly queryConfiguration: Schema.TaggedStruct<'queryConfiguration', { readonly rejected: (typeof Applied)['fields']['rejected'] }>;
    readonly nativeConstantMissing: Schema.TaggedStruct<'nativeConstantMissing', { readonly property: Schema.String; readonly constant: Schema.String }>;
}> = Schema.TaggedUnion({
    selectionMissing: { collection: Schema.String, id: Schema.Int },
    rangeOutOfBounds: { storyId: Schema.Int, collection: Schema.Literals(['paragraphs', 'characters', 'insertionPoints']), from: Schema.Int, to: Schema.Int, count: Schema.Int },
    queryConfiguration: { rejected: Applied.fields.rejected },
    nativeConstantMissing: { property: Schema.String, constant: Schema.String },
});

const Output: Schema.Union<
    readonly [
        typeof Applied,
        Schema.Struct<{ readonly kind: Schema.Literal<'changed'>; readonly count: Schema.Int }>,
        Schema.Struct<{ readonly kind: Schema.Literal<'inserted'>; readonly storyId: Schema.Int; readonly offset: Schema.Int; readonly character: Schema.String; readonly length: Schema.Int }>,
        Schema.Struct<{ readonly kind: Schema.Literal<'rejected'>; readonly error: typeof EditorialError }>,
    ]
> = Schema.Union([
    Applied,
    Schema.Struct({ kind: Schema.Literal('changed'), count: Schema.Int }),
    Schema.Struct({ kind: Schema.Literal('inserted'), storyId: Schema.Int, offset: Schema.Int, character: Schema.String, length: Schema.Int }),
    Schema.Struct({ kind: Schema.Literal('rejected'), error: EditorialError }),
]);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Applied, EditorialError, Input, NativeConstant, Output };
