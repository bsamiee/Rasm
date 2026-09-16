// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Option, Schema, SchemaGetter, String } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Entry<Key extends string> {
    readonly key: Key;
    readonly value: string;
}

interface Preset {
    readonly global: readonly Entry<(typeof GLOBAL_KEYS)[number]>[];
    readonly masters: readonly { readonly index: number; readonly entries: readonly Entry<(typeof MASTER_KEYS)[number]>[] }[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _GUARD = [
    '\n\n\n',
    '/*********** DO NOT REMOVE OR CHANGE THIS SEPARATOR ***********/',
    '\n\n\n',
    '/*WARNING: Please do not change the format of this file, otherwise it can cause the application to malfunction*/',
    '\n',
] as const;
const GLOBAL_KEYS = [
    'Type',
    'Version',
    'Preset Name',
    'Date',
    'Document Setup',
    'Number of Masters',
    'Facing Pages',
    'Unit',
    'Width',
    'Height',
    'Leading (pt)',
    'Correct Leading (pt)',
    'Desired Grid Width',
    'Applied Grid Width',
    'Square',
    'Fit Leading',
    'Subdivision',
    'Width 4',
    'H. Module 2',
    'H. Module 3',
    'H. Subdiv 2',
    'H. Subdiv 3',
    'Height 4',
    'V. Module 2',
    'V. Module 3',
    'V. Subdiv 2',
    'V. Subdiv 3',
    'Leading',
    'Font',
    'Style',
    'Size (pt)',
    'Style Leading',
    'Alignment',
    'Tracking',
    'Kerning',
    'Indent/Exdent',
    'Tabs',
    'Figure Style',
    'Bleed (Chain)',
    'Bleed (Top)',
    'Bleed (Bottom)',
    'Bleed (Inside)',
    'Bleed (Outside)',
    'Slug (Chain)',
    'Slug (Top)',
    'Slug (Bottom)',
    'Slug (Inside)',
    'Slug (Outside)',
    'Vertical Value Mode',
] as const;
const MASTER_KEYS = [
    'No. of Master Pages',
    'Horizontal Value Mode',
    'Image-lines height (pt)',
    'Image-lines',
    ' Smart Columns applied',
    ' Smart Rows applied',
    ' Desired Top Margin',
    ' Desired Bottom Margin',
    ' Desired Inside Margin',
    ' Desired Outside Margin',
    'Top margin (lines)',
    'Bottom margin (lines)',
    'Top margin',
    'Bottom margin',
    'Inside margin (lines)',
    'Outside margin (lines)',
    'Inside margin',
    'Outside margin',
    'Main Columns',
    'Custom Columns',
    'Main Subcolumns',
    'Custom Subcolumns',
    'Fit sec. columns',
    'Main Secondary Columns',
    'Custom Secondary Columns',
    'Fit rows',
    'Main Rows',
    'Custom Rows',
    'Main Subrows',
    'Custom Subrows',
    'Fit sec. rows',
    'Main Secondary Rows',
    'Custom Secondary Rows',
    'Typearea Grid (Horizontal)',
    'Typearea Grid (Subleading)',
] as const;
const _CUSTOM_DECIMALS = 3;

// --- [VALUES] --------------------------------------------------------------------------

const _LevelKind = Schema.Literals(['columns', 'rows']);
const _MeasureUnit = Schema.Literals(['pt', 'mm', 'in']);
const _Fixed = Schema.String.pipe(Schema.decodeTo(Schema.Number, { decode: SchemaGetter.Number(), encode: SchemaGetter.transform((amount: number) => amount.toFixed(_CUSTOM_DECIMALS)) }));

const Level: Schema.Codec<{ readonly kind: 'columns' | 'rows'; readonly count: number; readonly lines: number; readonly gutter: number }, string> = Schema.TemplateLiteralParser([
    Schema.FiniteFromString,
    ' ',
    _LevelKind,
    ' (',
    Schema.FiniteFromString,
    ' lines); gutter: ',
    Schema.FiniteFromString,
    Schema.Literals([' line', ' lines']),
]).pipe(
    Schema.decodeTo(Schema.Struct({ kind: _LevelKind, count: Schema.Number, lines: Schema.Number, gutter: Schema.Number }), {
        decode: SchemaGetter.transform(([count, _space, kind, _open, lines, _close, gutter]) => ({ kind, count, lines, gutter })),
        encode: SchemaGetter.transform(({ kind, count, lines, gutter }) => [count, ' ', kind, ' (', lines, ' lines); gutter: ', gutter, gutter === 1 ? ' line' : ' lines'] as const),
    }),
);

const Measure: Schema.Codec<{ readonly value: number; readonly unit: 'pt' | 'mm' | 'in' }, string> = Schema.TemplateLiteralParser([Schema.FiniteFromString, ' ', _MeasureUnit]).pipe(
    Schema.decodeTo(Schema.Struct({ value: Schema.Number, unit: _MeasureUnit }), {
        decode: SchemaGetter.transform(([amount, _space, unit]) => ({ value: amount, unit })),
        encode: SchemaGetter.transform(({ value: amount, unit }) => [amount, ' ', unit] as const),
    }),
);

const Custom: Schema.Codec<{ readonly count: number; readonly width: number; readonly gutter: number }, string> = Schema.TemplateLiteralParser([
    'Num: ',
    Schema.FiniteFromString,
    ', Width: ',
    _Fixed,
    ', Gutter: ',
    _Fixed,
]).pipe(
    Schema.decodeTo(Schema.Struct({ count: Schema.Number, width: Schema.Number, gutter: Schema.Number }), {
        decode: SchemaGetter.transform(([_num, count, _width, width, _gutter, gutter]) => ({ count, width, gutter })),
        encode: SchemaGetter.transform(({ count, width, gutter }) => ['Num: ', count, ', Width: ', width, ', Gutter: ', gutter] as const),
    }),
);

const TypeareaGrid: Schema.Codec<{ readonly lines: number; readonly value: number }, string> = Schema.TemplateLiteralParser([Schema.FiniteFromString, ' lines - ', Schema.FiniteFromString]).pipe(
    Schema.decodeTo(Schema.Struct({ lines: Schema.Number, value: Schema.Number }), {
        decode: SchemaGetter.transform(([lines, _separator, amount]) => ({ lines, value: amount })),
        encode: SchemaGetter.transform(({ lines, value: amount }) => [lines, ' lines - ', amount] as const),
    }),
);

// --- [GRAMMAR] -------------------------------------------------------------------------

const _entry = <Key extends string>(keys: readonly Key[]): Schema.Codec<Entry<Key>, string> => {
    const Keys = Schema.Literals(keys);
    return Schema.TemplateLiteralParser([Keys, ': ', Schema.String, ';']).pipe(
        Schema.decodeTo(Schema.Struct({ key: Keys, value: Schema.String }), {
            decode: SchemaGetter.transform(([key, _colon, value]) => ({ key, value })),
            encode: SchemaGetter.transform(({ key, value }) => [key, ': ', value, ';'] as const),
        }),
    );
};

const _lines = <Key extends string>(entry: Schema.Codec<Entry<Key>, string>): Schema.Codec<readonly Entry<Key>[], string> =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.NonEmptyArray(Schema.String), { decode: SchemaGetter.transform(String.split('\n')), encode: SchemaGetter.transform(Array.join('\n')) }),
        Schema.decodeTo(Schema.Array(entry)),
    );

const _GlobalBlock = _lines(_entry(GLOBAL_KEYS)).pipe(Schema.check(Schema.makeFilter((entries) => Option.exists(Array.head(entries), (entry) => entry.key === 'Type') || '`Type` is the first key')));

const _MasterEntries = _lines(_entry(MASTER_KEYS));

const _MasterBlock = Schema.TemplateLiteralParser(['[GC-', Schema.FiniteFromString, ']\n', _MasterEntries]).pipe(
    Schema.decodeTo(Schema.Struct({ index: Schema.Number, entries: Schema.toType(_MasterEntries) }), {
        decode: SchemaGetter.transform(([_open, index, _close, entries]) => ({ index, entries })),
        encode: SchemaGetter.transform(({ index, entries }) => ['[GC-', index, ']\n', entries] as const),
    }),
);

const Preset: Schema.Codec<Preset, string> = Schema.TemplateLiteralParser([Schema.String, ..._GUARD]).pipe(
    Schema.decodeTo(Schema.String, {
        decode: SchemaGetter.transform(([body]) => body),
        encode: SchemaGetter.transform((body) => [body, ..._GUARD] as const),
    }),
    Schema.decodeTo(Schema.NonEmptyArray(Schema.String), { decode: SchemaGetter.transform(String.split('\n\n')), encode: SchemaGetter.transform(Array.join('\n\n')) }),
    Schema.decodeTo(Schema.TupleWithRest(Schema.Tuple([_GlobalBlock]), [_MasterBlock])),
    Schema.decodeTo(Schema.Struct({ global: Schema.toType(_GlobalBlock), masters: Schema.Array(Schema.toType(_MasterBlock)) }), {
        decode: SchemaGetter.transform(([global, ...masters]) => ({ global, masters })),
        encode: SchemaGetter.transform(({ global, masters }) => [global, ...masters] as const),
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Custom, GLOBAL_KEYS, Level, MASTER_KEYS, Measure, Preset, TypeareaGrid };
