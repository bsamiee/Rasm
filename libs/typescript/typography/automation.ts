// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Array, Console, Data, Effect, Option, Schema, Struct } from 'effect';
import { Argument, Command, Flag } from 'effect/unstable/cli';
import { Points, Positive, Quantized, quickUnits, Span } from './grid.ts';
import { POINTS, pageSizes } from './page-sizes.ts';

// --- [MODELS] --------------------------------------------------------------------------

const _Readout = Schema.Struct({ lines: Quantized, leading: Quantized, horizontal: Quantized });

// --- [ERRORS] --------------------------------------------------------------------------

const AutomationError = Data.taggedEnum<Data.TaggedEnum<{ readonly pageNotFound: { readonly page: string } }>>();

// --- [ENTRY] ---------------------------------------------------------------------------

const _grid = Command.make(
    'grid',
    {
        page: Argument.String('page').pipe(Argument.withDescription('A page-size name such as `A4 210x297`, or `<width>x<height>` in the unit')),
        leading: Argument.Finite('leading').pipe(Argument.withDescription('Desired leading in points')),
        unit: Flag.Literals('unit', Struct.keys(POINTS)).pipe(Flag.withDescription('Unit of a `<width>x<height>` page and of the unit rows'), Flag.withDefault('pt')),
    },
    Effect.fnUntraced(function* ({ page, leading, unit }) {
        const dimension = Schema.FiniteFromString.pipe(Schema.decodeTo(Points(unit)));
        const sheet = yield* Effect.fromOption(
            Option.orElse(
                Option.map(Schema.decodeUnknownOption(Schema.TemplateLiteralParser([dimension, 'x', dimension]))(page), ([across, _separator, down]) => ({ width: across, height: down })),
                () =>
                    Option.map(
                        Array.findFirst(pageSizes, (row) => row.name === page),
                        (row) => ({ width: row.width, height: row.height }),
                    ),
            ),
            () => AutomationError.pageNotFound({ page }),
        );
        const units = yield* Effect.fromResult(quickUnits(yield* Schema.decodeUnknownEffect(Schema.Struct({ width: Span, height: Span, leading: Positive }))({ ...sheet, leading })));
        const display = Quantized.pipe(Schema.decodeTo(Points(unit)));
        const rows = [
            Schema.encodeSync(_Readout)({ lines: units.lines, leading: units.leading, horizontal: units.horizontal }),
            Schema.encodeSync(Schema.Struct({ width: display, height: display, vertical: display, horizontal: display }))({ ...sheet, vertical: units.leading, horizontal: units.horizontal }),
        ];
        yield* Console.log(JSON.stringify(rows, null, 4));
    }),
);

Command.run(Command.make('automation').pipe(Command.withSubcommands([_grid])), { version: '' }).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
