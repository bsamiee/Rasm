// --- [IMPORTS] -------------------------------------------------------------------------

import { BaselineGridRelativeOption, HorizontalOrVertical, MeasurementUnits, PageSideOptions, RulerOrigin } from 'adobe:indesign';
import { thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { type GridInput, GridOutput } from '@rasm/creative-cloud-server/indesign/grid';
import { GridProblem, quantize, resolveGridDefinition } from '@rasm/typography/grid';
import { Preset, presetGrid } from '@rasm/typography/presets';
import { Array, Effect, Equivalence, flow, Match, Option, Order, Record, Result, Schema, Struct } from 'effect';
import { documentFor, scripting, swapped, undoable } from '../host.ts';

// --- [APPLICATION] ---------------------------------------------------------------------

const applyGrid: (body: typeof GridInput.Type) => Effect.Effect<typeof GridOutput.Type, HostRejection> = Effect.fnUntraced(
    function* (body: typeof GridInput.Type) {
        const document = yield* documentFor(body.documentId);
        yield* swapped(document.viewPreferences, { horizontalMeasurementUnits: MeasurementUnits.POINTS, verticalMeasurementUnits: MeasurementUnits.POINTS, rulerOrigin: RulerOrigin.SPREAD_ORIGIN });
        yield* swapped(document, { zeroPoint: [0, 0] });
        const definition = yield* Effect.result(
            Match.value(body.source).pipe(
                Match.when({ _tag: 'geometry' }, (source) => Effect.succeed(source.definition)),
                Match.when({ _tag: 'presetGeometry' }, ({ text }) =>
                    Schema.decodeUnknownEffect(Preset)(text).pipe(
                        Effect.mapError((cause) => [GridProblem.make({ context: 'preset', reason: 'invalid', cause })] as const),
                        Effect.flatMap(presetGrid),
                    ),
                ),
                Match.exhaustive,
            ),
        );
        if (Result.isFailure(definition)) {
            return { kind: 'rejected', errors: definition.failure };
        }
        const baseline = Option.getOrElse(
            Option.orElse(
                Option.map(body.baselineStart, (start) => ({ origin: 'page' as const, start })),
                () => definition.success.baseline,
            ),
            () => ({
                origin: document.gridPreferences.baselineGridRelativeOption.equals(BaselineGridRelativeOption.TOP_OF_MARGIN_OF_BASELINE_GRID_RELATIVE_OPTION) ? ('margin' as const) : ('page' as const),
                start: Number(document.gridPreferences.baselineStart),
            }),
        );
        const resolution = yield* Effect.result(resolveGridDefinition(definition.success, baseline));
        if (Result.isFailure(resolution)) {
            return { kind: 'rejected', errors: resolution.failure };
        }
        const geometry = resolution.success;
        const relative =
            geometry.baseline.origin === 'margin' ? BaselineGridRelativeOption.TOP_OF_MARGIN_OF_BASELINE_GRID_RELATIVE_OPTION : BaselineGridRelativeOption.TOP_OF_PAGE_OF_BASELINE_GRID_RELATIVE_OPTION;
        const requests = Array.flatMap(body.targets, ({ layout, scope }) => Array.map(scope.ids, (id) => ({ layout, scope: scope._tag, id })));
        const [problems, found] = yield* Effect.partition(
            requests,
            Effect.fnUntraced(function* (target) {
                const layout = yield* Effect.fromOption(
                    Array.findFirst(geometry.layouts, ({ index }) => index === target.layout),
                    () => GridProblem.make({ context: `layout.${target.layout}`, reason: 'missing', cause: 'No grid definition has this layout identity.' }),
                );
                const container = target.scope === 'pages' ? document.pages.itemByID(target.id) : document.masterSpreads.itemByID(target.id);
                if (!container.isValid) {
                    return yield* Effect.fail(GridProblem.make({ context: `${target.scope}.${target.id}`, reason: 'missing', cause: 'The selected native target does not exist in this document.' }));
                }
                const pages = target.scope === 'pages' ? [document.pages.itemByID(target.id)] : document.masterSpreads.itemByID(target.id).pages.everyItem().getElements();
                if (target.scope === 'parents' && pages.length !== layout.pages) {
                    return yield* Effect.fail(GridProblem.make({ context: `parents.${target.id}`, reason: 'invalid', cause: { expectedPages: layout.pages, actualPages: pages.length } }));
                }
                return Array.map(pages, (page) => ({ page, layout }));
            }),
        );
        const targets = Array.flatten(found);
        const layer = document.layers.itemByID(body.layerId);
        const availableGuides = Array.flatMap(targets, ({ page }) => page.guides.everyItem().getElements());
        const removed = Array.filter(availableGuides, (guide) => Array.contains(body.replaceGuideIds, guide.id));
        const missingGuides = Array.difference(body.replaceGuideIds, Array.map(removed, Struct.get('id')));
        const invalid = [
            ...problems,
            ...Option.toArray(layer.isValid ? Option.none() : Option.some(GridProblem.make({ context: `layer.${body.layerId}`, reason: 'missing', cause: 'The target guide layer does not exist.' }))),
            ...Option.toArray(Array.isArrayEmpty(missingGuides) ? Option.none() : Option.some(GridProblem.make({ context: 'replaceGuideIds', reason: 'missing', cause: missingGuides }))),
            ...Option.toArray(
                Array.dedupe(Array.map(targets, ({ page }) => page.id)).length === targets.length
                    ? Option.none()
                    : Option.some(GridProblem.make({ context: 'targets', reason: 'invalid', cause: 'Each native page can receive exactly one layout.' })),
            ),
        ];
        if (Array.isArrayNonEmpty(invalid)) {
            return { kind: 'rejected', errors: invalid };
        }
        yield* Effect.forEach(
            Array.dedupeWith([layer, ...Array.map(removed, Struct.get('itemLayer'))], (left, right) => left.id === right.id),
            (owner) => swapped(owner, { locked: false, lockGuides: false }),
        );
        yield* swapped(document.guidePreferences, { guidesLocked: false });
        const columns = Array.map(targets, ({ page, layout }) => {
            const positions = Array.flatMap(layout.tracks.columns, ({ start, end }) => [start - layout.margins.inside, end - layout.margins.inside]);
            const span = geometry.width - layout.margins.inside - layout.margins.outside;
            const widths = Array.map(layout.tracks.columns, ({ start, end }) => end - start);
            const gutter = layout.tracks.columns.length > 1 ? (span - Array.reduce(widths, 0, (sum, width) => sum + width)) / (layout.tracks.columns.length - 1) : 0;
            return {
                page,
                layout,
                gutter,
                positions: {
                    right: positions,
                    left: Array.sort(
                        Array.map(positions, (position) => span - position),
                        Order.Number,
                    ),
                },
            };
        });
        const guideAxes = Array.flatMap(targets, (target) => Array.map(Record.toEntries(target.layout.guides), ([axis, locations]) => ({ ...target, axis, locations })));
        const guideLocations = Array.flatMap(guideAxes, ({ page, axis, locations }) => Array.map(locations, (location) => ({ page, axis, location })));
        const guideIds = yield* undoable('Apply Grid', Schema.Array(Schema.Int), () => {
            document.documentPreferences.properties = { pageWidth: geometry.width, pageHeight: geometry.height, facingPages: geometry.facingPages };
            document.gridPreferences.properties = {
                verticalGridlineDivision: geometry.grid.columns.size,
                verticalGridSubdivision: geometry.grid.columns.subdivisions,
                horizontalGridlineDivision: geometry.grid.rows.size,
                horizontalGridSubdivision: geometry.grid.rows.subdivisions,
                baselineDivision: geometry.leading,
                baselineStart: geometry.baseline.start,
                baselineGridRelativeOption: relative,
            };
            if (Option.isSome(geometry.bleed)) {
                document.documentPreferences.documentBleedUniformSize = false;
                document.documentPreferences.properties = {
                    documentBleedTopOffset: geometry.bleed.value.top,
                    documentBleedBottomOffset: geometry.bleed.value.bottom,
                    documentBleedInsideOrLeftOffset: geometry.bleed.value.inside,
                    documentBleedOutsideOrRightOffset: geometry.bleed.value.outside,
                };
            }
            if (Option.isSome(geometry.slug)) {
                document.documentPreferences.documentSlugUniformSize = false;
                document.documentPreferences.properties = {
                    slugTopOffset: geometry.slug.value.top,
                    slugBottomOffset: geometry.slug.value.bottom,
                    slugInsideOrLeftOffset: geometry.slug.value.inside,
                    slugRightOrOutsideOffset: geometry.slug.value.outside,
                };
            }
            Array.forEach(columns, ({ page, layout, gutter, positions }) => {
                page.marginPreferences.properties = {
                    top: layout.margins.top,
                    bottom: layout.margins.bottom,
                    left: layout.margins.inside,
                    right: layout.margins.outside,
                    columnCount: layout.tracks.columns.length,
                    columnGutter: gutter,
                };
                page.marginPreferences.columnsPositions = page.side.equals(PageSideOptions.LEFT_HAND) ? positions.left : positions.right;
            });
            Array.forEach(removed, (guide) => {
                Object.assign(guide, { locked: false });
                guide.remove();
            });
            return Array.map(guideLocations, ({ page, axis, location }) => {
                const [top, left] = page.bounds;
                const horizontal = page.side.equals(PageSideOptions.LEFT_HAND) ? geometry.width - location : location;
                return page.guides.add(layer, {
                    orientation: axis === 'columns' ? HorizontalOrVertical.VERTICAL : HorizontalOrVertical.HORIZONTAL,
                    location: axis === 'columns' ? left + horizontal : top + location,
                    fitToPage: true,
                    locked: body.locked,
                }).id;
            });
        });
        return yield* Effect.try({
            try: () => {
                const guides = Array.map(Array.zip(guideLocations, guideIds), ([{ page, axis }, id]) => {
                    const guide = page.guides.itemByID(id);
                    const [top, left] = page.bounds;
                    const offset = Number(guide.location) - (axis === 'columns' ? left : top);
                    return { pageId: page.id, id, axis, location: axis === 'columns' && page.side.equals(PageSideOptions.LEFT_HAND) ? geometry.width - offset : offset, locked: guide.locked };
                });
                const byPage = Array.groupBy(guides, ({ pageId }) => String(pageId));
                const pages = Array.map(targets, ({ page, layout }) => {
                    const [top, left, bottom, right] = page.bounds;
                    return {
                        id: page.id,
                        layout: layout.index,
                        width: right - left,
                        height: bottom - top,
                        margins: Record.map(
                            { top: page.marginPreferences.top, bottom: page.marginPreferences.bottom, inside: page.marginPreferences.left, outside: page.marginPreferences.right },
                            Number,
                        ),
                        columns: page.marginPreferences.columnsPositions,
                        guides: Option.getOrElse(Record.get(byPage, String(page.id)), Array.empty),
                    };
                });
                const native = {
                    width: Number(document.documentPreferences.pageWidth),
                    height: Number(document.documentPreferences.pageHeight),
                    facingPages: document.documentPreferences.facingPages,
                    grid: {
                        columns: { size: Number(document.gridPreferences.verticalGridlineDivision), subdivisions: document.gridPreferences.verticalGridSubdivision },
                        rows: { size: Number(document.gridPreferences.horizontalGridlineDivision), subdivisions: document.gridPreferences.horizontalGridSubdivision },
                    },
                    leading: Number(document.gridPreferences.baselineDivision),
                    baseline: {
                        start: Number(document.gridPreferences.baselineStart),
                        origin: document.gridPreferences.baselineGridRelativeOption.equals(BaselineGridRelativeOption.TOP_OF_MARGIN_OF_BASELINE_GRID_RELATIVE_OPTION)
                            ? ('margin' as const)
                            : ('page' as const),
                    },
                    bleed: Option.some(
                        Record.map(
                            {
                                top: document.documentPreferences.documentBleedTopOffset,
                                bottom: document.documentPreferences.documentBleedBottomOffset,
                                inside: document.documentPreferences.documentBleedInsideOrLeftOffset,
                                outside: document.documentPreferences.documentBleedOutsideOrRightOffset,
                            },
                            Number,
                        ),
                    ),
                    slug: Option.some(
                        Record.map(
                            {
                                top: document.documentPreferences.slugTopOffset,
                                bottom: document.documentPreferences.slugBottomOffset,
                                inside: document.documentPreferences.slugInsideOrLeftOffset,
                                outside: document.documentPreferences.slugRightOrOutsideOffset,
                            },
                            Number,
                        ),
                    ),
                };
                const comparisons: readonly { readonly context: string; readonly expected: readonly number[]; readonly actual: readonly number[] }[] = [
                    ...Array.map(Struct.keys(Struct.pick(geometry, ['width', 'height', 'leading'])), (key) => ({ context: key, expected: [geometry[key]], actual: [native[key]] })),
                    { context: 'baseline.start', expected: [geometry.baseline.start], actual: [native.baseline.start] },
                    ...Array.flatMap(Record.keys(geometry.grid), (axis) =>
                        Array.map(Struct.keys(geometry.grid[axis]), (key) => ({ context: `${axis}.${key}`, expected: [geometry.grid[axis][key]], actual: [native.grid[axis][key]] })),
                    ),
                    ...Array.flatten(
                        Array.zipWith(columns, pages, (target, page) => [
                            { context: `${page.id}.dimensions`, expected: [geometry.width, geometry.height], actual: [page.width, page.height] },
                            { context: `${page.id}.columns`, expected: target.page.side.equals(PageSideOptions.LEFT_HAND) ? target.positions.left : target.positions.right, actual: page.columns },
                            ...Array.map(Record.keys(target.layout.margins), (key) => ({ context: `${page.id}.margins.${key}`, expected: [target.layout.margins[key]], actual: [page.margins[key]] })),
                        ]),
                    ),
                    ...Array.flatMap(['bleed', 'slug'] as const, (key) =>
                        Option.toArray(
                            Option.map(Option.product(geometry[key], native[key]), ([expected, actual]) => ({
                                context: key,
                                expected: Record.values(expected),
                                actual: Record.values(Struct.pick(actual, Struct.keys(expected))),
                            })),
                        ),
                    ),
                    ...Array.zipWith(guideLocations, guides, (expected, actual) => ({ context: `${actual.id}.location`, expected: [expected.location], actual: [actual.location] })),
                ];
                const equivalent = Equivalence.Array(Equivalence.mapInput(Equivalence.Number, quantize));
                const differences = [
                    ...Array.map(
                        Array.filter(comparisons, ({ expected, actual }) => !equivalent(expected, actual)),
                        ({ context, expected, actual }) => GridProblem.make({ context, reason: 'geometry', cause: { expected, actual } }),
                    ),
                    ...Array.map(
                        Array.filter(guides, (guide) => guide.locked !== body.locked),
                        ({ id, locked }) => GridProblem.make({ context: `${id}.locked`, reason: 'geometry', cause: { expected: body.locked, actual: locked } }),
                    ),
                    ...Option.toArray(
                        geometry.facingPages === native.facingPages
                            ? Option.none()
                            : Option.some(GridProblem.make({ context: 'facingPages', reason: 'geometry', cause: { expected: geometry.facingPages, actual: native.facingPages } })),
                    ),
                    ...Option.toArray(
                        geometry.baseline.origin === native.baseline.origin
                            ? Option.none()
                            : Option.some(
                                  GridProblem.make({
                                      context: 'baseline.origin',
                                      reason: 'geometry',
                                      cause: { expected: geometry.baseline.origin, actual: native.baseline.origin },
                                  }),
                              ),
                    ),
                ];
                return { kind: 'gridApplied' as const, documentId: document.id, geometry: native, pages, differences };
            },
            catch: thrown,
        });
    },
    Effect.scoped,
    scripting,
    Effect.flatMap(
        flow(
            Schema.decodeUnknownEffect(Schema.toType(GridOutput)),
            Effect.mapError((cause) => HostRejection.cases.resultNotJson.make({ cause })),
        ),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { applyGrid };
