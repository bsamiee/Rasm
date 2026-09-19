// --- [IMPORTS] -------------------------------------------------------------------------

import { Compiler } from '@swc/core';
import { Array, Effect, FileSystem, identity, Match, Option, Path, Record, Schema, Struct } from 'effect';
import { AbsolutePath, OptionalInt, OptionalNumber, OptionalString, PageIndex } from '../values.ts';
import type * as Native from './native.ts';
import { FIELD_TYPES, literal } from './native.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Operation = (typeof Operation)['Type'];

// --- [TABLES] --------------------------------------------------------------------------

const _TARGETS = {
    digital: { profile: 'sRGB IEC61966-2.1', preserveBlack: false },
    print: { profile: 'GRACoL2013_CRPC6.icc', preserveBlack: true },
} as const;
const _NON_PRINT = { flatten: 0, keep: 1, remove: 2 } as const;

const NAMES = {
    makeAccessible: 'Adobe:MakeAccessible',
    accessibilityCheck: 'AccCheck:DoCheck',
    pdfUa: 'Verify compliance with PDF/UA-1 (syntax checks only)',
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const _Component: Schema.Number = Schema.Number.pipe(Schema.check(Schema.isBetween({ minimum: 0, maximum: 1 })));

const Color: Schema.Union<
    readonly [
        Schema.Tuple<readonly [Schema.Literal<'T'>]>,
        Schema.Tuple<readonly [Schema.Literal<'G'>, Schema.Number]>,
        Schema.Tuple<readonly [Schema.Literal<'RGB'>, Schema.Number, Schema.Number, Schema.Number]>,
        Schema.Tuple<readonly [Schema.Literal<'CMYK'>, Schema.Number, Schema.Number, Schema.Number, Schema.Number]>,
    ]
> = Schema.Union([
    Schema.Tuple([Schema.Literal('T')]),
    Schema.Tuple([Schema.Literal('G'), _Component]),
    Schema.Tuple([Schema.Literal('RGB'), _Component, _Component, _Component]),
    Schema.Tuple([Schema.Literal('CMYK'), _Component, _Component, _Component, _Component]),
]);

const _optionalColor = Schema.OptionFromOptionalKey(Color);
const _optionalBoolean = Schema.OptionFromOptionalKey(Schema.Boolean);
const _optionalJson = Schema.OptionFromOptionalKey(Schema.Json);
const _optionalPage = Schema.OptionFromOptionalKey(PageIndex);

const _Items: Schema.$Array<Schema.Struct<{ readonly label: Schema.String; readonly export: Schema.String }>> = Schema.Array(Schema.Struct({ label: Schema.String, export: Schema.String }));

const _FieldType: Schema.Literals<Array<keyof typeof FIELD_TYPES>> = Schema.Literals(Struct.keys(FIELD_TYPES));

const _Rect: Schema.Tuple<readonly [Schema.Number, Schema.Number, Schema.Number, Schema.Number]> = Schema.Tuple([Schema.Number, Schema.Number, Schema.Number, Schema.Number]);

const Field: Schema.Struct<{
    readonly name: Schema.String;
    readonly type: typeof _FieldType;
    readonly page: Schema.Union<readonly [Schema.Int, Schema.$Array<Schema.Int>]>;
    readonly rect: typeof _Rect;
    readonly readonly: Schema.OptionFromOptionalKey<Schema.Boolean>;
    readonly required: Schema.OptionFromOptionalKey<Schema.Boolean>;
    readonly multiline: Schema.OptionFromOptionalKey<Schema.Boolean>;
    readonly value: Schema.OptionFromOptionalKey<Schema.Codec<Schema.Json>>;
    readonly defaultValue: Schema.OptionFromOptionalKey<Schema.Codec<Schema.Json>>;
    readonly calcOrderIndex: Schema.OptionFromOptionalKey<Schema.Int>;
    readonly charLimit: Schema.OptionFromOptionalKey<Schema.Int>;
    readonly numItems: Schema.OptionFromOptionalKey<Schema.Int>;
    readonly textSize: Schema.OptionFromOptionalKey<Schema.Number>;
    readonly lineWidth: Schema.OptionFromOptionalKey<Schema.Number>;
    readonly textFont: Schema.OptionFromOptionalKey<Schema.String>;
    readonly style: Schema.OptionFromOptionalKey<Schema.String>;
    readonly exportValues: Schema.OptionFromOptionalKey<Schema.$Array<Schema.String>>;
    readonly strokeColor: Schema.OptionFromOptionalKey<typeof Color>;
    readonly fillColor: Schema.OptionFromOptionalKey<typeof Color>;
    readonly items: Schema.OptionFromOptionalKey<typeof _Items>;
    readonly borderStyle: Schema.OptionFromOptionalKey<Schema.Literals<readonly ['solid', 'dashed', 'beveled', 'inset', 'underline']>>;
    readonly alignment: Schema.OptionFromOptionalKey<Schema.Literals<readonly ['left', 'center', 'right']>>;
}> = Schema.Struct({
    name: Schema.String,
    type: _FieldType,
    page: Schema.Union([Schema.Int, Schema.Array(Schema.Int)]),
    rect: _Rect,
    readonly: _optionalBoolean,
    required: _optionalBoolean,
    multiline: _optionalBoolean,
    value: _optionalJson,
    defaultValue: _optionalJson,
    calcOrderIndex: OptionalInt,
    charLimit: OptionalInt,
    numItems: OptionalInt,
    textSize: OptionalNumber,
    lineWidth: OptionalNumber,
    textFont: OptionalString,
    style: OptionalString,
    exportValues: Schema.OptionFromOptionalKey(Schema.Array(Schema.String)),
    strokeColor: _optionalColor,
    fillColor: _optionalColor,
    items: Schema.OptionFromOptionalKey(_Items),
    borderStyle: Schema.OptionFromOptionalKey(Schema.Literals(['solid', 'dashed', 'beveled', 'inset', 'underline'])),
    alignment: Schema.OptionFromOptionalKey(Schema.Literals(['left', 'center', 'right'])),
});

const _Spec: Schema.Struct<{
    readonly name: Schema.String;
    readonly create: Schema.OptionFromOptionalKey<Schema.Struct<{ readonly type: typeof _FieldType; readonly page: Schema.Int; readonly rect: typeof _Rect }>>;
    readonly format: Schema.OptionFromOptionalKey<
        Schema.Union<
            readonly [
                Schema.Struct<{ readonly kind: Schema.Literal<'plain'>; readonly charLimit: Schema.OptionFromOptionalKey<Schema.Int> }>,
                Schema.Struct<{
                    readonly kind: Schema.Literal<'number'>;
                    readonly decimals: Schema.Int;
                    readonly range: Schema.OptionFromOptionalKey<Schema.Struct<{ readonly min: Schema.Number; readonly max: Schema.Number }>>;
                }>,
                Schema.Struct<{ readonly kind: Schema.Literal<'percent'>; readonly decimals: Schema.Int }>,
                Schema.Struct<{ readonly kind: Schema.Literal<'date'> }>,
                Schema.Struct<{ readonly kind: Schema.Literal<'time'> }>,
                Schema.Struct<{ readonly kind: Schema.Literal<'total'>; readonly decimals: Schema.Int; readonly operands: Schema.NonEmptyArray<Schema.String> }>,
            ]
        >
    >;
    readonly items: Schema.OptionFromOptionalKey<typeof _Items>;
    readonly exportValues: Schema.OptionFromOptionalKey<Schema.$Array<Schema.String>>;
    readonly caption: Schema.OptionFromOptionalKey<Schema.String>;
    readonly mouseUp: Schema.OptionFromOptionalKey<Schema.Union<readonly [Schema.Literal<'resetForm'>, Schema.Struct<{ readonly submitForm: Schema.String }>]>>;
    readonly readOnly: Schema.OptionFromOptionalKey<Schema.Boolean>;
    readonly required: Schema.OptionFromOptionalKey<Schema.Boolean>;
    readonly defaultValue: Schema.OptionFromOptionalKey<Schema.String>;
}> = Schema.Struct({
    name: Schema.String,
    create: Schema.OptionFromOptionalKey(Schema.Struct({ type: _FieldType, page: PageIndex, rect: _Rect })),
    format: Schema.OptionFromOptionalKey(
        Schema.Union([
            Schema.Struct({ kind: Schema.Literal('plain'), charLimit: OptionalInt }),
            Schema.Struct({ kind: Schema.Literal('number'), decimals: Schema.Int, range: Schema.OptionFromOptionalKey(Schema.Struct({ min: Schema.Number, max: Schema.Number })) }),
            Schema.Struct({ kind: Schema.Literal('percent'), decimals: Schema.Int }),
            Schema.Struct({ kind: Schema.Literal('date') }),
            Schema.Struct({ kind: Schema.Literal('time') }),
            Schema.Struct({ kind: Schema.Literal('total'), decimals: Schema.Int, operands: Schema.NonEmptyArray(Schema.String) }),
        ]),
    ),
    items: Schema.OptionFromOptionalKey(_Items),
    exportValues: Schema.OptionFromOptionalKey(Schema.Array(Schema.String)),
    caption: OptionalString,
    mouseUp: Schema.OptionFromOptionalKey(Schema.Union([Schema.Literal('resetForm'), Schema.Struct({ submitForm: Schema.String })])),
    readOnly: _optionalBoolean,
    required: _optionalBoolean,
    defaultValue: OptionalString,
});

const Fields: Schema.NonEmptyArray<typeof _Spec> = Schema.NonEmptyArray(_Spec);

const Sources: Schema.NonEmptyArray<
    Schema.Struct<{
        readonly path: typeof AbsolutePath;
        readonly start: Schema.OptionFromOptionalKey<Schema.Int>;
        readonly end: Schema.OptionFromOptionalKey<Schema.Int>;
        readonly label: Schema.OptionFromOptionalKey<Schema.String>;
    }>
> = Schema.NonEmptyArray(Schema.Struct({ path: AbsolutePath, start: _optionalPage, end: _optionalPage, label: OptionalString }));

const Operation: Schema.toTaggedUnion<
    'op',
    readonly [
        Schema.Struct<{ readonly op: Schema.Literal<'colorConvertPage'>; readonly page: Schema.Int; readonly target: Schema.Literals<Array<keyof typeof _TARGETS>> }>,
        Schema.Struct<{ readonly op: Schema.Literal<'embedOutputIntent'>; readonly profile: Schema.String }>,
        Schema.Struct<{
            readonly op: Schema.Literal<'flattenPages'>;
            readonly nonPrint: Schema.Literals<Array<keyof typeof _NON_PRINT>>;
            readonly start: Schema.OptionFromOptionalKey<Schema.Int>;
            readonly end: Schema.OptionFromOptionalKey<Schema.Int>;
        }>,
        Schema.Struct<{
            readonly op: Schema.Literal<'addWatermarkFromText'>;
            readonly text: Schema.String;
            readonly font: Schema.OptionFromOptionalKey<Schema.String>;
            readonly size: Schema.OptionFromOptionalKey<Schema.Number>;
            readonly color: Schema.OptionFromOptionalKey<typeof Color>;
            readonly opacity: Schema.OptionFromOptionalKey<Schema.Number>;
            readonly rotation: Schema.OptionFromOptionalKey<Schema.Number>;
            readonly start: Schema.OptionFromOptionalKey<Schema.Int>;
            readonly end: Schema.OptionFromOptionalKey<Schema.Int>;
        }>,
        Schema.Struct<{ readonly op: Schema.Literal<'applyRedactions'> }>,
    ]
> = Schema.Union([
    Schema.Struct({ op: Schema.Literal('colorConvertPage'), page: PageIndex, target: Schema.Literals(Struct.keys(_TARGETS)) }),
    Schema.Struct({ op: Schema.Literal('embedOutputIntent'), profile: Schema.String }),
    Schema.Struct({ op: Schema.Literal('flattenPages'), nonPrint: Schema.Literals(Struct.keys(_NON_PRINT)), start: _optionalPage, end: _optionalPage }),
    Schema.Struct({
        op: Schema.Literal('addWatermarkFromText'),
        text: Schema.String,
        font: OptionalString,
        size: OptionalNumber,
        color: _optionalColor,
        opacity: OptionalNumber,
        rotation: OptionalNumber,
        start: _optionalPage,
        end: _optionalPage,
    }),
    Schema.Struct({ op: Schema.Literal('applyRedactions') }),
]).pipe(Schema.toTaggedUnion('op'));

// --- [NATIVE BOUNDARY] -----------------------------------------------------------------

const compiled: Effect.Effect<string, never, FileSystem.FileSystem | Path.Path> = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const source = yield* fs.readFileString(yield* path.fromFileUrl(new URL('./native.ts', import.meta.url)));
    const compiler = new Compiler();
    const module = compiler.parseSync(source, { syntax: 'typescript' });
    module.body = module.body.filter((statement) => statement.type !== 'ExportNamedDeclaration' && !(statement.type === 'ImportDeclaration' && statement.typeOnly));
    return compiler.transformSync(module, { isModule: false, minify: true, jsc: { parser: { syntax: 'typescript' }, target: 'es3', assumptions: { iterableIsArray: true } } }).code;
}).pipe(Effect.orDie);

const call = <Name extends keyof typeof Native>(name: Name, ...args: Parameters<Extract<(typeof Native)[Name], (...values: never[]) => unknown>>): string => `${name}.apply(this, ${literal(args)})`;

const envelope = (program: string, code: string): string => `(function () { ${program} return run(function () { return ${code}; }); })()`;

const withDocument = (device: string, document: AbsolutePath, code: string): string => `(function () { return ${code}; }).call(document(${literal(device)}, ${literal(document)}))`;

const setFields = (specs: (typeof Fields)['Type'], save: Option.Option<AbsolutePath>): string =>
    call(
        'setFields',
        Array.map(specs, (spec) => {
            const format = Option.getOrUndefined(spec.format);
            const scripts = Match.value(format).pipe(
                Match.withReturnType<readonly [string, string, string, string]>(),
                Match.when(undefined, () => ['', '', '', '']),
                Match.when({ kind: 'plain' }, () => ['', '', '', '']),
                Match.whenOr({ kind: 'number' }, { kind: 'total' }, (number) => [
                    `AFNumber_Format(${number.decimals}, 0, 0, 0, "", true);`,
                    `AFNumber_Keystroke(${number.decimals}, 0, 0, 0, "", true);`,
                    number.kind === 'number' ? Option.match(number.range, { onNone: () => '', onSome: ({ min, max }) => `AFRange_Validate(true, ${min}, true, ${max});` }) : '',
                    number.kind === 'total' ? `AFSimple_Calculate("SUM", ${literal(number.operands)});` : '',
                ]),
                Match.when({ kind: 'percent' }, ({ decimals }) => [`AFPercent_Format(${decimals}, 0);`, `AFPercent_Keystroke(${decimals}, 0);`, 'AFRange_Validate(true, 0, true, 1);', '']),
                Match.when({ kind: 'date' }, () => ['AFDate_FormatEx("yyyy-mm-dd");', 'AFDate_KeystrokeEx("yyyy-mm-dd");', '', '']),
                Match.when({ kind: 'time' }, () => ['AFTime_Format(0);', 'AFTime_Keystroke(0);', '', '']),
                Match.exhaustive,
            );
            const requested = Record.filter(
                { ...Struct.pick(spec, ['required', 'defaultValue', 'exportValues', 'format', 'items', 'caption', 'mouseUp']), charLimit: format?.kind === 'plain' ? format.charLimit : Option.none() },
                Option.isSome<unknown>,
            );
            const unsupported = Record.map(FIELD_TYPES, ({ read, configure }) => Array.difference(Struct.keys(requested), [...read, ...configure]));
            const writes: Native.FieldWrite[] = [
                ...(format === undefined ? [] : [['readonly', format.kind === 'total'] as const]),
                ...Array.fromOption(Option.map(spec.exportValues, (value): Native.FieldWrite => ['exportValues', value])),
            ];
            const overrides: Native.FieldWrite[] = [
                ...Array.fromOption(Option.map(spec.readOnly, (value): Native.FieldWrite => ['readonly', value])),
                ...Array.fromOption(Option.map(spec.required, (value): Native.FieldWrite => ['required', value])),
                ...Array.fromOption(Option.map(spec.defaultValue, (value): Native.FieldWrite => ['defaultValue', value])),
            ];
            const textWrites: Native.FieldWrite[] =
                format === undefined
                    ? []
                    : [
                          ['charLimit', format.kind === 'plain' ? Option.getOrElse(format.charLimit, () => 0) : 0],
                          ['comb', false],
                          ['doNotSpellCheck', true],
                          ['doNotScroll', false],
                          ['richText', false],
                          ['alignment', format.kind === 'number' || format.kind === 'percent' || format.kind === 'total' ? 'right' : 'left'],
                      ];
            return {
                ...Struct.omit(Schema.encodeSync(_Spec)(spec), ['format', 'readOnly', 'required', 'defaultValue', 'mouseUp', 'exportValues']),
                ...Record.getSomes({
                    mouseUp: Option.map(spec.mouseUp, (value) => (value === 'resetForm' ? 'this.resetForm();' : `this.submitForm(${literal(value.submitForm)});`)),
                }),
                unsupported,
                overrides,
                writes: Record.map(FIELD_TYPES, ({ defaults }, type): readonly Native.FieldWrite[] => [...defaults, ...(type === 'text' ? textWrites : []), ...writes]),
                actions: format === undefined ? [] : Array.zip(['Format', 'Keystroke', 'Validate', 'Calculate'], scripts),
                operands: format?.kind === 'total' ? format.operands : [],
            };
        }),
        Option.getOrNull(save),
    );

const printProduction = (operations: readonly Operation[], save: Option.Option<AbsolutePath>): string =>
    call(
        'printProduction',
        Array.map(operations, (operation) =>
            Match.value(operation).pipe(
                Match.withReturnType<Parameters<typeof Native.printProduction>[0][number]>(),
                Match.discriminatorsExhaustive('op')({
                    colorConvertPage: identity,
                    embedOutputIntent: ({ op, profile }) => ({ op, args: [profile] }),
                    flattenPages: ({ op, start, end, nonPrint }) => ({ op, args: [{ nNonPrint: _NON_PRINT[nonPrint], ...Record.getSomes({ nStart: start, nEnd: end }) }] }),
                    addWatermarkFromText: ({ op, ...watermark }) => ({
                        op,
                        args: [
                            Schema.encodeSync(
                                Schema.Struct(Struct.omit(Operation.cases.addWatermarkFromText.fields, ['op'])).pipe(
                                    Schema.encodeKeys({
                                        text: 'cText',
                                        font: 'cFont',
                                        size: 'nFontSize',
                                        color: 'aColor',
                                        opacity: 'nOpacity',
                                        rotation: 'nRotation',
                                        start: 'nStart',
                                        end: 'nEnd',
                                    }),
                                ),
                            )(watermark),
                        ],
                    }),
                    applyRedactions: ({ op }) => ({ op, args: [] }),
                }),
            ),
        ),
        _TARGETS,
        Option.getOrNull(save),
    );

// --- [TEXT MATCHES] --------------------------------------------------------------------

const matches = (source: Native.DocumentResult['scan'], expression: RegExp): readonly (Native.DocumentRequest['redact']['matches'][number] & { readonly wordIndices: readonly number[] })[] =>
    Array.flatMap(source.pages, ({ page, words }) => {
        const [, spans] = Array.mapAccum(words, 0, (start, word) => [start + word.word.length, { ...word, start, end: start + word.word.length }]);
        const text = Array.map(words, ({ word }) => word).join('');
        const pattern = new RegExp(expression, expression.global ? expression.flags : `${expression.flags}g`);
        return Array.flatMap(Array.fromIterable(text.matchAll(pattern)), (match) => {
            const touched = match[0].length === 0 ? [] : Array.filter(spans, ({ start, end }) => start < match.index + match[0].length && end > match.index);
            return Array.map(Array.take(touched, 1), ({ wordIndex }) => ({
                page,
                wordIndex,
                wordIndices: Array.map(touched, Struct.get('wordIndex')),
                text: match[0],
                quads: Array.flatMap(touched, Struct.get('quads')),
            }));
        });
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Color, call, compiled, envelope, Field, Fields, matches, NAMES, Operation, printProduction, Sources, setFields, withDocument };
