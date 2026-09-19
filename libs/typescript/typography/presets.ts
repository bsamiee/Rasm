// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Effect, Function, Match, Option, Record, Result, Schema, SchemaGetter, String, Struct } from 'effect';
import { Count, GridDefinition, GridProblem, quantize, RULE, round, SubdivisionIndex } from './grid.ts';
import { POINTS } from './page-sizes.ts';

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

const _LevelKind: Schema.Literals<readonly ['columns', 'rows', 'subcolumns', 'subrows']> = Schema.Literals(['columns', 'rows', 'subcolumns', 'subrows']);
const _MeasureUnit = Schema.Literals(['pt', 'mm', 'in']);
const _Fixed = Schema.String.pipe(Schema.decodeTo(Schema.Number, { decode: SchemaGetter.Number(), encode: SchemaGetter.transform((amount: number) => amount.toFixed(_CUSTOM_DECIMALS)) }));

const Level: Schema.Codec<{ readonly kind: typeof _LevelKind.Type; readonly count: number; readonly lines: number; readonly gutter: number }, string> = Schema.TemplateLiteralParser([
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

// --- [GEOMETRY IMPORT] -----------------------------------------------------------------

const _Count = Schema.FiniteFromString.pipe(Schema.decodeTo(Count));
const _NonNegative = Schema.FiniteFromString.check(Schema.isGreaterThanOrEqualTo(0));
const _Positive = Schema.FiniteFromString.check(Schema.isGreaterThan(0));
const _Flag = Schema.Literals(['Yes', 'No']);
const _Fit = Schema.Literals(['Fit to margins', 'Fit to page']);
const _Subdivision = Schema.Union([
    Schema.TemplateLiteralParser(['Applied Leading: ', _Positive, ' pt']),
    Schema.TemplateLiteralParser([Schema.Literals(['/', 'x']), Schema.FiniteFromString.check(Schema.isInt(), Schema.isBetween({ minimum: 2, maximum: 5 })), ' - V. Subdiv: ', _Positive, ' pt']),
]);
const _LEVELS = [
    { axis: 'columns', role: 'main', main: 'Main Columns', custom: 'Custom Columns', fit: 'margins', kind: 'columns' },
    { axis: 'columns', role: 'sub', main: 'Main Subcolumns', custom: 'Custom Subcolumns', fit: 'margins', kind: 'subcolumns' },
    { axis: 'columns', role: 'secondary', main: 'Main Secondary Columns', custom: 'Custom Secondary Columns', fit: 'Fit sec. columns', kind: 'columns' },
    { axis: 'rows', role: 'main', main: 'Main Rows', custom: 'Custom Rows', fit: 'Fit rows', kind: 'rows' },
    { axis: 'rows', role: 'sub', main: 'Main Subrows', custom: 'Custom Subrows', fit: 'margins', kind: 'subrows' },
    { axis: 'rows', role: 'secondary', main: 'Main Secondary Rows', custom: 'Custom Secondary Rows', fit: 'Fit sec. rows', kind: 'rows' },
] as const;

/** The file boundary owns blank-as-absence and accumulates every independent field error. */
const _read = <A, I>(context: string, entries: readonly Entry<string>[], schema: Schema.Codec<A, I>): Effect.Effect<A, GridProblem> => {
    const values = Record.fromIterableWith(entries, ({ key, value }) => [key, value.trim()]);
    const duplicates = entries.length !== Record.size(values);
    return duplicates
        ? Effect.fail(GridProblem.make({ context, reason: 'invalid', cause: 'Preset keys must be unique within their block.' }))
        : Schema.decodeUnknownEffect(schema, { errors: 'all' })(Record.filter(values, String.isNonEmpty)).pipe(Effect.mapError((cause) => GridProblem.make({ context, reason: 'invalid', cause })));
};

/** Import the saved grid geometry. Body typography remains in the lossless Preset value. */
const presetGrid: (preset: Preset) => Effect.Effect<GridDefinition, Array.NonEmptyReadonlyArray<GridProblem>> = Effect.fnUntraced(function* (preset: Preset) {
    const global = yield* _read(
        'global',
        preset.global,
        Schema.Struct({
            type: Schema.Literal('Grid Calculator Publishing Edition'),
            version: Schema.Literals(['1.3', '1.4']),
            mode: Schema.Literals(['Quick', 'Modular', 'Smart']),
            parents: _Count,
            facing: Schema.Literals(['true', 'false']),
            unit: Schema.Literals(['Millimeters', 'Inches', 'Points and Pixels']),
            verticalValue: _Flag,
        }).pipe(
            Schema.encodeKeys({ type: 'Type', version: 'Version', mode: 'Document Setup', parents: 'Number of Masters', facing: 'Facing Pages', unit: 'Unit', verticalValue: 'Vertical Value Mode' }),
        ),
    ).pipe(Effect.mapError(Array.of));
    if (global.parents !== preset.masters.length || Array.dedupe(Array.map(preset.masters, Struct.get('index'))).length !== preset.masters.length) {
        return yield* Effect.fail([GridProblem.make({ context: 'masters', reason: 'invalid', cause: 'The declared parent count must match unique parent identities.' })] as const);
    }
    const scale = Match.value(global.unit).pipe(
        Match.when('Millimeters', Function.constant(POINTS.mm)),
        Match.when('Inches', Function.constant(POINTS.in)),
        Match.when('Points and Pixels', Function.constant(POINTS.pt)),
        Match.exhaustive,
    );
    const dimensions = _read(
        'dimensions',
        preset.global,
        Schema.Struct({ width: _Positive, height: _Positive }).pipe(Schema.encodeKeys(global.mode === 'Quick' ? { width: 'Width', height: 'Height' } : { width: 'Width 4', height: 'Height 4' })),
    ).pipe(Effect.map(Record.map((value: number) => value * scale)), Effect.mapError(Array.of));
    const rhythm = Effect.gen(function* () {
        if (global.mode === 'Quick') {
            const values = yield* _read(
                'quick',
                preset.global,
                Schema.Struct({ leading: _Positive, applied: _Positive, desiredWidth: _NonNegative, appliedWidth: _NonNegative, square: _Flag, fit: _Flag, subdivision: _Subdivision }).pipe(
                    Schema.encodeKeys({
                        leading: 'Leading (pt)',
                        applied: 'Correct Leading (pt)',
                        desiredWidth: 'Desired Grid Width',
                        appliedWidth: 'Applied Grid Width',
                        square: 'Square',
                        fit: 'Fit Leading',
                        subdivision: 'Subdivision',
                    }),
                ),
            );
            if (global.verticalValue === 'Yes' && values.fit !== 'Yes') {
                return yield* Effect.fail(GridProblem.make({ context: 'Fit Leading', reason: 'invalid', cause: 'Vertical Value mode requires fitted leading.' }));
            }
            if (global.verticalValue === 'Yes') {
                return { _tag: 'value' as const, leading: quantize(values.applied), horizontal: values.appliedWidth * scale };
            }
            return {
                _tag: 'quick' as const,
                leading: values.leading,
                fitLeading: values.fit === 'Yes',
                subdivision: SubdivisionIndex.make(values.subdivision[0] === 'Applied Leading: ' ? 0 : values.subdivision[1] + (values.subdivision[0] === '/' ? -1 : RULE.steps - 1)),
                square: values.square === 'Yes',
                ...(values.desiredWidth > 0 ? { horizontal: values.desiredWidth * scale } : {}),
            };
        }
        if (global.verticalValue === 'Yes') {
            return yield* Effect.fail(GridProblem.make({ context: 'Vertical Value Mode', reason: 'invalid', cause: 'The vendor format defines vertical Value mode only for Quick layouts.' }));
        }
        const values = yield* _read(
            'modular',
            preset.global,
            Schema.Struct({ columns: _Count, columnParts: _Count, rows: _Count, rowParts: _Count, rowSize: _Positive, leading: _Positive }).pipe(
                Schema.encodeKeys({ columns: 'H. Module 2', columnParts: 'H. Subdiv 2', rows: 'V. Module 2', rowParts: 'V. Subdiv 2', rowSize: 'V. Subdiv 3', leading: 'Leading' }),
            ),
        );
        return { _tag: 'modular' as const, ...values };
    }).pipe(Effect.mapError(Array.of));
    const boundaries = Effect.validate(
        ['Bleed', 'Slug'] as const,
        Effect.fnUntraced(function* (kind) {
            const keys = { top: 'Top', bottom: 'Bottom', inside: 'Inside', outside: 'Outside' } as const;
            const values = yield* _read(
                kind,
                preset.global,
                Schema.Struct(Record.map(keys, Function.constant(Schema.OptionFromOptionalKey(_NonNegative)))).pipe(Schema.encodeKeys(Record.map(keys, (side) => `${kind} (${side})`))),
            );
            const present = Record.values(values);
            if (Array.every(present, Option.isNone)) {
                return [kind, Option.none()] as const;
            }
            const sides = yield* Option.all(values).pipe(
                Effect.fromOption(() => GridProblem.make({ context: kind, reason: 'missing', cause: 'An applied bleed or slug requires all four side values.' })),
            );
            return [kind, Option.some(Record.map(sides, (value) => value * scale))] as const;
        }),
    );
    const parents = Effect.validate(
        preset.masters,
        Effect.fnUntraced(function* (master) {
            const state = yield* _read(
                `parent.${master.index}`,
                master.entries,
                Schema.Struct({
                    'No. of Master Pages': _Count,
                    'Image-lines height (pt)': Schema.OptionFromOptionalKey(_Positive),
                    'Image-lines': Schema.OptionFromOptionalKey(Schema.Literals(['x', 'H', 'Custom'])),
                    'Typearea Grid (Horizontal)': TypeareaGrid,
                    'Typearea Grid (Subleading)': TypeareaGrid,
                }),
            );
            if (Option.isSome(state['Image-lines']) !== Option.isSome(state['Image-lines height (pt)'])) {
                return yield* Effect.fail(
                    GridProblem.make({ context: `parent.${master.index}.imageLine`, reason: 'missing', cause: 'Image-line selection and its measured height must be supplied together.' }),
                );
            }
            return { master, state };
        }),
    );
    const margins = Effect.validate(
        Array.flatMap(preset.masters, (master) => [
            { master, axis: 'columns' as const, before: 'Inside', after: 'Outside' },
            { master, axis: 'rows' as const, before: 'Top', after: 'Bottom' },
        ]),
        Effect.fnUntraced(function* ({ master, axis, before, after }) {
            const value =
                axis === 'rows'
                    ? global.verticalValue === 'Yes' && master.index === 1
                    : (yield* _read(`parent.${master.index}.Horizontal Value Mode`, master.entries, Schema.Struct({ mode: _Flag }).pipe(Schema.encodeKeys({ mode: 'Horizontal Value Mode' })))).mode ===
                      'Yes';
            const suffix = value ? ' margin' : ' margin (lines)';
            const keys = { before: `${before}${suffix}`, after: `${after}${suffix}` };
            const values = yield* _read(`parent.${master.index}.${axis}.margins`, master.entries, Schema.Struct({ before: _NonNegative, after: _NonNegative }).pipe(Schema.encodeKeys(keys)));
            return { index: master.index, axis, before: values.before * (value ? scale : 1), after: values.after * (value ? scale : 1), unit: value ? ('pt' as const) : ('line' as const) };
        }),
    );
    const fields = Effect.validate(
        Array.flatMap(preset.masters, (master) => Array.map(_LEVELS, (level) => ({ master, level }))),
        Effect.fnUntraced(function* ({ master, level }) {
            const { main } = yield* _read(
                `parent.${master.index}.${level.main}`,
                master.entries,
                Schema.Struct({ main: Schema.OptionFromOptionalKey(Level) }).pipe(Schema.encodeKeys({ main: level.main })),
            );
            if (Option.exists(main, ({ kind }) => kind !== level.kind)) {
                return yield* Effect.fail(GridProblem.make({ context: `parent.${master.index}.${level.main}`, reason: 'invalid', cause: 'The serialized field kind must match its level.' }));
            }
            const active: Option.Option<{ readonly count: number; readonly size: number; readonly gutter: number; readonly unit: 'line' | 'pt' }> = Option.isSome(main)
                ? Option.some({ count: main.value.count, size: main.value.lines, gutter: main.value.gutter, unit: 'line' as const })
                : yield* _read(
                      `parent.${master.index}.${level.custom}`,
                      master.entries,
                      Schema.Struct({ custom: Schema.OptionFromOptionalKey(Custom) }).pipe(Schema.encodeKeys({ custom: level.custom })),
                  ).pipe(
                      Effect.map(({ custom }) =>
                          Option.map(
                              Option.filter(custom, ({ count }) => count !== 0),
                              ({ count, width, gutter }) => ({ count, size: width * scale, gutter: gutter * scale, unit: 'pt' as const }),
                          ),
                      ),
                  );
            if (Option.isNone(active)) {
                return Option.none();
            }
            const fit =
                level.fit === 'margins'
                    ? 'Fit to margins'
                    : (yield* _read(`parent.${master.index}.${level.fit}`, master.entries, Schema.Struct({ fit: _Fit }).pipe(Schema.encodeKeys({ fit: level.fit })))).fit;
            return Option.some({
                index: master.index,
                axis: level.axis,
                role: level.role,
                fit: fit === 'Fit to margins' ? ('margins' as const) : ('page' as const),
                unit: active.value.unit,
                gutter: active.value.gutter,
                fields: { _tag: 'equal' as const, count: active.value.count, size: active.value.size },
            });
        }),
    );
    const parsed = yield* Effect.all({ dimensions, rhythm, boundaries, parents, margins, fields }, { mode: 'result' });
    const failures = Array.flatten(Array.getFailures(Object.values(parsed)));
    if (Array.isArrayNonEmpty(failures)) {
        return yield* Effect.fail(failures);
    }
    const source = yield* Effect.fromResult(Result.all(parsed));
    const sides = Record.fromEntries(source.boundaries);
    const position = yield* Effect.gen(function* () {
        if (source.rhythm._tag !== 'value') {
            return Option.none<{ readonly leading: number; readonly phase: number; readonly remainder: number }>();
        }
        const primary = yield* Option.all({
            margin: Array.findFirst(source.margins, ({ index, axis }) => index === 1 && axis === 'rows'),
            parent: Array.findFirst(source.parents, ({ master }) => master.index === 1),
        }).pipe(Effect.fromOption(() => [GridProblem.make({ context: 'masters', reason: 'missing', cause: 'The vendor format derives vertical Value mode from parent 1.' })] as const));
        const { leading } = source.rhythm;
        const gap = Option.match(primary.parent.state['Image-lines height (pt)'], { onNone: Function.constant(0), onSome: (height) => leading - height });
        const top = primary.margin.before - gap;
        const phase = quantize(top - Math.floor(top / leading) * leading) % leading;
        return Option.some({ leading, phase, remainder: source.dimensions.height - phase - Math.floor((source.dimensions.height - phase) / leading) * leading });
    });
    const layouts = Array.map(source.parents, ({ master, state }) => ({
        index: master.index,
        pages: state['No. of Master Pages'],
        ...Record.getSomes({ imageLine: state['Image-lines height (pt)'] }),
        margins: Record.fromIterableWith(
            Array.filter(source.margins, ({ index }) => index === master.index),
            ({ axis, before, after, unit }) => [
                axis,
                Option.isSome(position) && axis === 'rows' && master.index !== 1
                    ? {
                          before:
                              before * position.value.leading +
                              position.value.phase +
                              Option.match(state['Image-lines height (pt)'], { onNone: Function.constant(0), onSome: (height) => position.value.leading - height }),
                          after: after * position.value.leading + position.value.remainder,
                          unit: 'pt',
                      }
                    : { before, after, unit },
            ],
        ),
        levels: Array.map(
            Array.filter(Array.getSomes(source.fields), ({ index }) => index === master.index),
            Struct.omit(['index']),
        ),
        typeArea: Record.getSomes({
            columns: Option.liftPredicate(state['Typearea Grid (Horizontal)'].lines, (lines) => lines !== 0),
            rows: Option.liftPredicate(state['Typearea Grid (Subleading)'].lines, (lines) => lines !== 0),
        }),
    }));
    return yield* Schema.decodeUnknownEffect(GridDefinition, { errors: 'all' })({
        ...source.dimensions,
        facingPages: global.facing === 'true',
        baseline: { origin: 'page', start: Option.match(position, { onNone: Function.constant(0), onSome: Struct.get('phase') }) },
        rhythm: Match.value(source.rhythm).pipe(
            Match.when({ _tag: 'quick' }, Function.identity),
            Match.when({ _tag: 'value' }, ({ horizontal, leading }) => ({ _tag: 'applied', columns: { size: horizontal, subdivisions: 1 }, rows: { size: leading, subdivisions: 1 }, leading })),
            Match.when({ _tag: 'modular' }, ({ columns, columnParts, rows, rowParts, leading, rowSize }) => ({
                _tag: 'applied',
                columns: { size: source.dimensions.width / columns, subdivisions: columnParts },
                rows: { size: source.dimensions.height / rows, subdivisions: rowParts },
                leading: (source.dimensions.height / rows / rowParts) * round(leading / rowSize),
            })),
            Match.exhaustive,
        ),
        layouts,
        ...Record.getSomes({ bleed: Option.flatten(Record.get(sides, 'Bleed')), slug: Option.flatten(Record.get(sides, 'Slug')) }),
    }).pipe(Effect.mapError((cause) => [GridProblem.make({ context: 'preset', reason: 'geometry', cause })] as const));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Custom, GLOBAL_KEYS, Level, MASTER_KEYS, Measure, Preset, presetGrid, TypeareaGrid };
