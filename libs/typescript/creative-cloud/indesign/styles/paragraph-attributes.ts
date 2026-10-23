import type { ParagraphStyle } from 'adobe:indesign';
import { Array, Record, Schema, Struct } from 'effect';
import { Reference } from '#/dom/reference';

// --- [MODELS] --------------------------------------------------------------------------

const TabStop = Schema.Struct({ position: Schema.Finite, alignment: Schema.String, leader: Schema.String, alignmentCharacter: Schema.String });
class ParagraphAttributes extends Schema.Class<ParagraphAttributes, { readonly brand: unique symbol }>('ParagraphAttributes')({
    nestedGrepStyles: Schema.Array(Schema.Struct({ grepExpression: Schema.String, appliedCharacterStyle: Reference })).annotate({ description: 'GREP styles in order' }),
    nestedStyles: Schema.Array(Schema.Struct({ appliedCharacterStyle: Reference, delimiter: Schema.Json, repetition: Schema.Int, inclusive: Schema.Boolean })),
    nestedLineStyles: Schema.Array(Schema.Struct({ appliedCharacterStyle: Reference, lineCount: Schema.Int, repeatLast: Schema.Int })),
    tabList: Schema.Array(TabStop),
    bulletsAndNumberingListType: Schema.String,
    numberingLevel: Schema.Int,
    bulletChar: Schema.Int.annotate({ description: 'Bullet character code point' }),
    numberingFormat: Schema.Json,
}) {
    static readonly of = (owner: Pick<ParagraphStyle, keyof typeof ParagraphAttributes.fields>) => ({
        nestedGrepStyles: Array.map(owner.nestedGrepStyles.everyItem().getElements(), ({ grepExpression, appliedCharacterStyle }) => ({ grepExpression, appliedCharacterStyle })),
        nestedStyles: Array.map(owner.nestedStyles.everyItem().getElements(), ({ appliedCharacterStyle, delimiter, repetition, inclusive }) => ({ appliedCharacterStyle, delimiter, repetition, inclusive })),
        nestedLineStyles: Array.map(owner.nestedLineStyles.everyItem().getElements(), ({ appliedCharacterStyle, lineCount, repeatLast }) => ({ appliedCharacterStyle, lineCount, repeatLast })),
        tabList: Array.map(owner.tabList, (tab) => Record.fromIterableWith(Struct.keys(TabStop.fields), (key) => [key, Reflect.get(tab, key)])),
        bulletsAndNumberingListType: owner.bulletsAndNumberingListType,
        numberingLevel: owner.numberingLevel,
        bulletChar: owner.bulletChar.characterValue,
        numberingFormat: owner.numberingFormat,
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ParagraphAttributes };
