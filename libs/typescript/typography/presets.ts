// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Effect, Equal, flow, Match, Option, pipe, Result, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type GlobalKey = (typeof GLOBAL_KEYS)[number];
type MasterKey = (typeof MASTER_KEYS)[number];

interface Entry<Key extends string> {
    readonly key: Key;
    readonly value: string;
}

interface Preset {
    readonly global: readonly Entry<GlobalKey>[];
    readonly masters: readonly { readonly index: number; readonly entries: readonly Entry<MasterKey>[] }[];
}

interface Accumulator {
    readonly preset: Preset;
    readonly guards: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SEPARATOR = '/*********** DO NOT REMOVE OR CHANGE THIS SEPARATOR ***********/';
const WARNING = '/*WARNING: Please do not change the format of this file, otherwise it can cause the application to malfunction*/';
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
    Schema.String.pipe(Schema.decodeTo(Schema.Number, { decode: SchemaGetter.Number(), encode: SchemaGetter.transform((amount: number) => amount.toFixed(_CUSTOM_DECIMALS)) })),
    ', Gutter: ',
    Schema.String.pipe(Schema.decodeTo(Schema.Number, { decode: SchemaGetter.Number(), encode: SchemaGetter.transform((amount: number) => amount.toFixed(_CUSTOM_DECIMALS)) })),
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

const Setup: Schema.Codec<'Quick' | 'Modular' | 'Smart'> = Schema.Literals(['Quick', 'Modular', 'Smart']);
const UnitName: Schema.Codec<'Millimeters' | 'Inches' | 'Points and Pixels'> = Schema.Literals(['Millimeters', 'Inches', 'Points and Pixels']);

// --- [GRAMMAR] -------------------------------------------------------------------------

const _entry = <Tag extends string, Key extends string>(tag: Tag, keys: readonly Key[]): Schema.Codec<{ readonly _tag: Tag; readonly key: Key; readonly value: string }, string> => {
    const Keys = Schema.Literals(keys);
    return Schema.TemplateLiteralParser([Keys, ': ', Schema.String, ';']).pipe(
        Schema.decodeTo(Schema.TaggedStruct(tag, { key: Keys, value: Schema.String }), {
            decode: SchemaGetter.transform(([key, _colon, value]) => ({ _tag: tag, key, value })),
            encode: SchemaGetter.transform(({ key, value }) => [key, ': ', value, ';'] as const),
        }),
    );
};

const _Guard = Schema.Literals([SEPARATOR, WARNING]);
const _Line = Schema.Union([
    _entry('global', GLOBAL_KEYS),
    _entry('master', MASTER_KEYS),
    Schema.TemplateLiteralParser(['[GC-', Schema.FiniteFromString, ']']).pipe(
        Schema.decodeTo(Schema.TaggedStruct('header', { index: Schema.Number }), {
            decode: SchemaGetter.transform(([_open, index]) => ({ _tag: 'header' as const, index })),
            encode: SchemaGetter.transform(({ index }) => ['[GC-', index, ']'] as const),
        }),
    ),
    _Guard.pipe(
        Schema.decodeTo(Schema.TaggedStruct('guard', { text: _Guard }), {
            decode: SchemaGetter.transform((text) => ({ _tag: 'guard' as const, text })),
            encode: SchemaGetter.transform(Struct.get('text')),
        }),
    ),
    Schema.Literal('').pipe(
        Schema.decodeTo(Schema.TaggedStruct('blank', {}), { decode: SchemaGetter.transform(() => ({ _tag: 'blank' as const })), encode: SchemaGetter.transform(() => '' as const) }),
    ),
]);

const _Preset = Schema.Struct({
    global: Schema.Array(Schema.Struct({ key: Schema.Literals(GLOBAL_KEYS), value: Schema.String })),
    masters: Schema.Array(Schema.Struct({ index: Schema.Number, entries: Schema.Array(Schema.Struct({ key: Schema.Literals(MASTER_KEYS), value: Schema.String })) })),
});

const _reducer = (fold: Accumulator, line: typeof _Line.Type, index: number): Result.Result<Accumulator, SchemaIssue.Issue> =>
    Match.value(line).pipe(
        Match.withReturnType<Result.Result<Accumulator, SchemaIssue.Issue>>(),
        Match.discriminatorsExhaustive('_tag')({
            global: ({ key, value }) =>
                Array.match(fold.preset.masters, {
                    onEmpty: () => Result.succeed({ ...fold, preset: { ...fold.preset, global: Array.append(fold.preset.global, { key, value }) } }),
                    onNonEmpty: () => Result.fail(new SchemaIssue.Pointer([index], new SchemaIssue.InvalidValue({ message: 'Key belongs to the global block' }, Option.some(line)))),
                }),
            master: ({ key, value }) =>
                Array.match(fold.preset.masters, {
                    onEmpty: () => Result.fail(new SchemaIssue.Pointer([index], new SchemaIssue.InvalidValue({ message: 'Key belongs to a master block' }, Option.some(line)))),
                    onNonEmpty: (masters) =>
                        Result.succeed({
                            ...fold,
                            preset: { ...fold.preset, masters: Array.modifyLastNonEmpty(masters, (last) => ({ ...last, entries: Array.append(last.entries, { key, value }) })) },
                        }),
                }),
            header: (header) => Result.succeed({ ...fold, preset: { ...fold.preset, masters: Array.append(fold.preset.masters, { index: header.index, entries: [] }) } }),
            guard: ({ text }) => Result.succeed({ ...fold, guards: Array.append(fold.guards, text) }),
            blank: () => Result.succeed(fold),
        }),
    );

const _fold = (lines: readonly (typeof _Line.Type)[]): Result.Result<Preset, SchemaIssue.Issue> =>
    pipe(
        lines,
        Array.reduce<Result.Result<Accumulator, SchemaIssue.Issue>, typeof _Line.Type>(Result.succeed({ preset: { global: [], masters: [] }, guards: [] }), (state, line, index) =>
            Result.flatMap(state, (fold) => _reducer(fold, line, index)),
        ),
        Result.flatMap((fold) =>
            Option.exists(Array.head(fold.preset.global), (entry) => entry.key === 'Type')
                ? Result.succeed(fold)
                : Result.fail(new SchemaIssue.Pointer([0], new SchemaIssue.InvalidValue({ message: '`Type` is the first key' }, Option.some(lines)))),
        ),
        Result.flatMap((fold) =>
            Equal.equals(fold.guards, [SEPARATOR, WARNING])
                ? Result.succeed(fold.preset)
                : Result.fail(new SchemaIssue.InvalidValue({ message: 'Preset ends with the separator and the warning' }, Option.some(fold.guards))),
        ),
    );

const _lines = (preset: Preset): readonly (typeof _Line.Type)[] => [
    ...Array.map(preset.global, (entry) => ({ _tag: 'global' as const, ...entry })),
    ...Array.flatMap(preset.masters, (master) => [
        { _tag: 'blank' as const },
        { _tag: 'header' as const, index: master.index },
        ...Array.map(master.entries, (entry) => ({ _tag: 'master' as const, ...entry })),
    ]),
    { _tag: 'blank' },
    { _tag: 'blank' },
    { _tag: 'guard', text: SEPARATOR },
    { _tag: 'blank' },
    { _tag: 'blank' },
    { _tag: 'guard', text: WARNING },
    { _tag: 'blank' },
];

const Preset: Schema.Codec<Preset, string> = Schema.String.pipe(
    Schema.decodeTo(Schema.Array(Schema.String), { decode: SchemaGetter.split({ separator: '\n' }), encode: SchemaGetter.transform(Array.join('\n')) }),
    Schema.decodeTo(Schema.Array(_Line)),
    Schema.decodeTo(_Preset, { decode: SchemaGetter.transformEffect(flow(_fold, Effect.fromResult)), encode: SchemaGetter.transform(_lines) }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Entry, GlobalKey, MasterKey };
export { Custom, GLOBAL_KEYS, Level, MASTER_KEYS, Measure, Preset, SEPARATOR, Setup, TypeareaGrid, UnitName, WARNING };
