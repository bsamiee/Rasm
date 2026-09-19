// --- [IMPORTS] -------------------------------------------------------------------------

import {
    app,
    BaselineGridRelativeOption,
    BlendingSpace,
    ColorModel,
    ColorSpace,
    DimensionsConstraints,
    DocumentIntentOptions,
    FirstBaseline,
    FlexWidthHeightMode,
    HorizontalOrVertical,
    Justification,
    LayoutRuleOptions,
    MeasurementUnits,
    PageNumberStyle,
    RulerOrigin,
    SmartMatchOptions,
    SpecialCharacters,
    TextWrapModes,
    UIColors,
    VariableTypes,
    VerticalJustification,
} from 'adobe:indesign';
import { thrown } from '@rasm/creative-cloud-server/client';
import type { HostRejection } from '@rasm/creative-cloud-server/errors';
import type { CustomTextVariablePreference, Font, LanguageWithVendors, ObjectStyle, ParagraphStyle, Rectangle, TextVariable } from '@rasm/creative-cloud-server/indesign';
import { type Body, FontSource, type Reply, TypographyError } from '@rasm/creative-cloud-server/indesign/jobs';
import { AbsolutePath } from '@rasm/creative-cloud-server/values';
import { GeometryError, partition, quantize, resolveGrid } from '@rasm/typography/grid';
import { fitMetrics, MetricsError } from '@rasm/typography/metrics';
import { type PageSize, POINTS, pageSizes, Standard } from '@rasm/typography/page-sizes';
import { Array, Cause, Data, Effect, Equal, Exit, Function, flow, HashSet, Match, Option, pipe, Record, Result, Schema, Scope, Struct } from 'effect';
import { close, fileFor, translate } from '../host.ts';
import { type Typography, typography } from '../typography.ts';
import { configureDocument } from './publishing.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

type Content = Data.TaggedEnum<{
    readonly text: { readonly role: keyof Typography['paragraphs']; readonly object: ObjectStyle; readonly baseline: number; readonly columns: number; readonly variable: Option.Option<TextVariable> };
    readonly figure: { readonly caption: { readonly style: ObjectStyle; readonly height: number; readonly gap: number } };
}>;
const Content = Data.taggedEnum<Content>();

interface Composition {
    readonly name: string;
    readonly paragraphs: Typography['paragraphs'];
    readonly backgrounds: readonly Required<Pick<Rectangle['properties'], 'geometricBounds' | 'appliedObjectStyle' | 'fillColor' | 'textWrapPreferences'>>[];
    readonly panels: Array.NonEmptyReadonlyArray<Array.NonEmptyReadonlyArray<{ readonly item: Content; readonly minimum: number; readonly preferred: number; readonly width: number }>>;
}

const _PAGE = { bleed: { mm: 3, in: 0.125, pt: 9, px: 0 }, viewThreshold: 25, competition: { width: 60, height: 40 } } as const;
const _LETTERING = {
    iso: { note: 2.5, subtitle: 3.5, title: 5, identifier: 7 },
    aec: { note: 0.093_75, subtitle: 0.125, title: 0.1875, identifier: 0.281_25 },
} as const;
const _ISO = { field: 50, border: 5, rule: 0.35, frame: 0.7, extension: 10, singleEdgeIndex: 4 } as const;
const _INTENT = { print: DocumentIntentOptions.PRINT_INTENT, web: DocumentIntentOptions.WEB_INTENT, mobile: DocumentIntentOptions.MOBILE_INTENT } as const;

// --- [DOCUMENT] ------------------------------------------------------------------------

const _document: (
    size: PageSize,
    input: Body<'buildTypography'>,
    prepared: Pick<Parameters<typeof typography>[1], 'fonts' | 'languages' | 'localized'>,
    master: boolean,
) => Effect.Effect<Pick<Extract<Reply<'buildTypography'>, { readonly kind: 'completed' }>, 'files' | 'failures'>, Extract<Reply<'buildTypography'>, { readonly kind: 'rejected' }>['errors']> =
    Effect.fnUntraced(
        function* (size, input, prepared, master) {
            const scope = yield* Effect.flatMap(Scope.Scope, Scope.fork);
            const document = yield* Effect.acquireRelease(
                Effect.sync(() => app.documents.add(false, { documentPreferences: { facingPages: false, pagesPerDocument: 1, pageWidth: size.width, pageHeight: size.height } })),
                (open) => close(open).pipe(Effect.orDie),
            ).pipe(Scope.provide(scope));
            const existingColors = Record.map(input.palette, (specification, name) => ({ specification, color: document.colors.itemByName(name) }));
            const conflicts = Array.map(
                Array.filter(
                    Record.toEntries(existingColors),
                    ([name, { specification, color }]) =>
                        document.swatches.itemByName(name).isValid &&
                        !(color.isValid && color.model.equals(ColorModel.PROCESS) && color.space.equals(ColorSpace.RGB) && Array.makeEquivalence(Equal.equals)(color.colorValue, specification.rgb)),
                ),
                ([name]) => name,
            );
            if (Array.isArrayNonEmpty(conflicts)) {
                return yield* Effect.fail([TypographyError.cases.paletteConflicts.make({ names: conflicts })] as const);
            }
            document.viewPreferences.properties = {
                horizontalMeasurementUnits: size.unit === 'px' ? MeasurementUnits.PIXELS : MeasurementUnits.POINTS,
                verticalMeasurementUnits: size.unit === 'px' ? MeasurementUnits.PIXELS : MeasurementUnits.POINTS,
                rulerOrigin: RulerOrigin.PAGE_ORIGIN,
            };
            document.marginPreferences.properties = { top: 0, bottom: 0, left: 0, right: 0, columnCount: 1, columnGutter: 0 };
            const pages = [...document.pages.everyItem().getElements(), ...Array.flatMap(document.masterSpreads.everyItem().getElements(), (spread) => spread.pages.everyItem().getElements())];
            Array.forEach(pages, ({ marginPreferences }) => {
                marginPreferences.properties = document.marginPreferences.properties;
            });
            document.documentPreferences.properties = {
                facingPages: false,
                pagesPerDocument: 1,
                pageWidth: size.width,
                pageHeight: size.height,
                intent: _INTENT[size.intent],
                documentBleedUniformSize: true,
                documentBleedTopOffset: _PAGE.bleed[size.sourceUnit] * POINTS[size.sourceUnit],
                documentSlugUniformSize: true,
                slugTopOffset: 0,
            };
            const [top, left, bottom, right] = document.pages.firstItem().bounds;
            const dimensions = { width: right - left, height: bottom - top };
            if (Array.some(Struct.keys(dimensions), (axis) => quantize(dimensions[axis]) !== quantize(size[axis]))) {
                return yield* Effect.fail([TypographyError.cases.nativePageGeometry.make({ expected: Struct.pick(size, ['width', 'height']), actual: dimensions })] as const);
            }
            document.textPreferences.properties = { useOpticalSize: false, shapeIndicAndLatinWithHarbuzz: true };
            document.rgbProfile = 'sRGB IEC61966-2.1';
            document.transparencyPreferences.blendingSpace = BlendingSpace.RGB;
            const palette = Record.map(existingColors, ({ specification: { rgb, group }, color }, name) => {
                const swatch = color.isValid ? color : document.colors.add({ name, model: ColorModel.PROCESS, space: ColorSpace.RGB, colorValue: [...rgb] });
                Option.map(group, (groupName) => {
                    const existing = document.colorGroups.itemByName(groupName);
                    const swatches = (existing.isValid ? existing : document.colorGroups.add({ name: groupName })).colorGroupSwatches;
                    if (!Array.some(swatches.everyItem().getElements(), ({ swatchItemRef }) => swatchItemRef.id === swatch.id)) {
                        swatches.add(swatch);
                    }
                });
                return swatch;
            });
            const colors = Struct.pick(palette, ['Accent', 'Ink', 'Field']);
            const styles = yield* typography(document, { module: size.module, measure: size.width - size.margins.left - size.margins.right, measured: input.fonts, ...prepared, colors });
            const grid = yield* Effect.fromResult(resolveGrid(size, styles.ink)).pipe(Effect.mapError(Array.of));
            styles.objects.row.flexLayoutAttributeOptions.flexGapColumn = grid.gutters.columns;
            styles.objects.column.flexLayoutAttributeOptions.flexGapRow = grid.gutters.rows;
            document.marginPreferences.properties = {
                top: grid.bounds.top,
                bottom: size.height - grid.bounds.bottom,
                left: grid.bounds.left,
                right: size.width - grid.bounds.right,
                columnCount: Option.isSome(size.sheet) ? 1 : grid.tracks.columns.length,
                columnGutter: grid.gutters.columns,
            };
            Array.forEach(pages, ({ marginPreferences }) => {
                marginPreferences.properties = document.marginPreferences.properties;
            });
            const resources = yield* configureDocument(document, styles, size.module, size.intent, colors.Ink);
            document.gridPreferences.properties = {
                baselineStart: styles.ink.above,
                baselineDivision: size.module,
                baselineGridRelativeOption: BaselineGridRelativeOption.TOP_OF_MARGIN_OF_BASELINE_GRID_RELATIVE_OPTION,
                baselineGridShown: true,
                baselineViewThreshold: _PAGE.viewThreshold,
                documentGridShown: false,
                documentGridSnapto: false,
                horizontalGridlineDivision: size.module,
                verticalGridlineDivision: size.module,
                horizontalGridSubdivision: 1,
                verticalGridSubdivision: 1,
            };
            const initialLayer = document.layers.firstItem();
            const layers = Record.map(
                {
                    fields: { name: 'Fields', layerColor: UIColors.YELLOW, printable: true },
                    figures: { name: 'Figures', layerColor: UIColors.RED, printable: true },
                    text: { name: 'Text', layerColor: UIColors.BLUE, printable: true },
                    folios: { name: 'Folios', layerColor: UIColors.GRAY, printable: true },
                    grid: { name: 'Grid', layerColor: UIColors.GREEN, printable: false },
                    imageLines: { name: 'Optical Alignment', layerColor: UIColors.LIGHT_BLUE, printable: false },
                    notes: { name: 'Notes', layerColor: UIColors.ORANGE, printable: false },
                },
                (properties) => document.layers.add(properties),
            );
            initialLayer.remove();
            const foundation = document.masterSpreads.firstItem();
            foundation.baseName = Option.isSome(grid.strip) ? 'Sheet' : 'Grid';
            const guideAxes = [
                { tracks: grid.tracks.columns, orientation: HorizontalOrVertical.VERTICAL },
                { tracks: grid.tracks.rows, orientation: HorizontalOrVertical.HORIZONTAL },
            ];
            Array.forEach(
                Array.flatMap(guideAxes, ({ tracks, orientation }) => Array.map(Array.dedupe(Array.flatMap(tracks, ({ start, end }) => [start, end])), (location) => ({ orientation, location }))),
                (properties) => foundation.guides.add(layers.grid, { ...properties, fitToPage: true }),
            );
            Array.forEach(grid.baselines, (baseline) =>
                foundation.guides.add(layers.imageLines, { orientation: HorizontalOrVertical.HORIZONTAL, location: baseline - styles.alignment.above, fitToPage: true }),
            );
            const extent = { width: grid.bounds.right - grid.bounds.left, height: grid.bounds.bottom - grid.bounds.top };
            const entry = Option.map(
                Option.liftPredicate(size, (pageSize) => pageSize.family === 'board'),
                () => {
                    const variable = document.textVariables.add({ name: 'Competition Entry', variableType: VariableTypes.CUSTOM_TEXT_TYPE });
                    const options: CustomTextVariablePreference = variable.variableOptions;
                    options.contents = '';
                    return variable;
                },
            );
            if (Option.isSome(grid.strip)) {
                const strip = grid.strip.value;
                const iso = Option.filter(size.standard, Standard.$is('iso'));
                const policy = Option.match(iso, {
                    onNone: () => ({ lettering: _LETTERING.aec, addressRole: 'note' as const, frame: styles.rule, border: Option.none<number>() }),
                    onSome: () => ({ lettering: _LETTERING.iso, addressRole: 'subtitle' as const, frame: _ISO.frame * POINTS.mm, border: Option.some(_ISO.border * POINTS.mm) }),
                });
                const source = yield* Effect.fromOption(
                    Option.all(Struct.pick(input.fonts.body.metrics, ['capHeight', 'ascent', 'descent'])),
                    () => [MetricsError.cases.metricsMissing.make({ postScriptName: input.fonts.body.postScriptName, fields: ['capHeight', 'ascent', 'descent'] })] as const,
                );
                const heights = Record.map(policy.lettering, (height) => height * POINTS[size.sourceUnit]);
                const measured = yield* Effect.all(
                    Record.map(heights, (height) => Effect.fromResult(fitMetrics(input.fonts.body, { height, extent: source.capHeight }))),
                    { mode: 'result' },
                );
                const [errors] = Array.partition(Record.values(measured), Function.identity);
                if (Array.isArrayNonEmpty(errors)) {
                    return yield* Effect.fail(errors);
                }
                const fitted = yield* Effect.fromResult(Result.all(measured)).pipe(Effect.mapError(Array.of));
                const group = document.paragraphStyleGroups.add({ name: 'Drawing' });
                const lettering = Record.map(fitted, (face, role) => {
                    const scale = face.size / input.fonts.body.size;
                    const above = source.ascent * scale;
                    const below = -source.descent * scale;
                    const leading = Math.ceil(Math.max(heights[role] + heights[role] / 2, above + below) / size.module) * size.module;
                    return {
                        above,
                        below,
                        leading,
                        style: group.paragraphStyles.add({
                            name: styles.paragraphs[role === 'identifier' ? 'indicator' : role].name,
                            basedOn: styles.paragraphs.body,
                            pointSize: face.size,
                            leading,
                            alignToBaseline: false,
                            firstLineIndent: 0,
                            spaceBefore: 0,
                            spaceAfter: 0,
                            keepAllLinesTogether: false,
                            keepLinesTogether: false,
                        }),
                    };
                });
                const page = foundation.pages.firstItem();
                const padding = Math.max(0, (lettering.note.leading - lettering.note.above - lettering.note.below) / 2);
                const revision = { name: 'Revision', fields: {} };
                const specifications: Array.NonEmptyReadonlyArray<{ readonly name: string; readonly fields: Readonly<Record<string, ParagraphStyle>> }> = [
                    { name: 'Designer', fields: { 'Issuing Agency': lettering.subtitle.style } },
                    revision,
                    {
                        name: 'Management',
                        fields: {
                            'Design Firm': lettering.note.style,
                            'Designed By': lettering.note.style,
                            'Drawn By': lettering.note.style,
                            'Checked By': lettering.note.style,
                            'Submitted By': lettering.note.style,
                            'Contract Number': lettering.note.style,
                        },
                    },
                    {
                        name: 'Project and Sheet',
                        fields: { 'Project Location': lettering.note.style, 'Project ID': lettering.note.style, 'Sheet Title': lettering.title.style, 'Sheet Location': lettering.note.style },
                    },
                    { name: 'Sheet Identification', fields: { 'Sheet ID': lettering.identifier.style, 'Building ID': lettering.note.style } },
                ];
                const minimum = Array.map(
                    specifications,
                    (specification) =>
                        2 * padding +
                        lettering.subtitle.leading +
                        (specification === revision
                            ? 2 * lettering.note.leading + Number(styles.table.spaceAfter)
                            : Array.reduce(Record.values(specification.fields), 0, (height, style) => height + Number(style.leading))),
                );
                const required = Array.reduce(minimum, 0, (sum, height) => sum + height);
                if (required > extent.height) {
                    return yield* Effect.fail([GeometryError.cases.tracksDoNotFit.make({ axis: 'rows', minimum, available: extent.height, tracks: grid.baselines.length })] as const);
                }
                const [, blocks] = Array.mapAccum(specifications, strip.top, (offset, specification, index) => {
                    const height = Array.getUnsafe(minimum, index) + (specification === revision ? extent.height - required : 0);
                    return [offset + height, { specification, height, top: offset, bottom: offset + height }];
                });
                page.rectangles.add(layers.fields, {
                    name: foundation.baseName,
                    geometricBounds: [grid.bounds.top, grid.bounds.left, grid.bounds.bottom, grid.bounds.right],
                    appliedObjectStyle: styles.objects.field,
                    strokeColor: colors.Ink,
                    strokeWeight: policy.frame,
                });
                const frames = Array.map(blocks, (block) => {
                    const frame = page.textFrames.add(layers.text, {
                        name: block.specification.name,
                        geometricBounds: [block.top, strip.left, block.bottom, strip.right],
                        appliedObjectStyle: styles.objects.text,
                        strokeColor: colors.Ink,
                        strokeWeight: styles.rule,
                        contents: `${block.specification.name}\r`,
                        textFramePreferences: { insetSpacing: padding, firstBaselineOffset: FirstBaseline.FIXED_HEIGHT, minimumFirstBaselineOffset: lettering.subtitle.above },
                    });
                    frame.parentStory.appliedParagraphStyle = lettering.note.style;
                    frame.paragraphs.firstItem().appliedParagraphStyle = lettering.subtitle.style;
                    return { ...block, frame };
                });
                const fields = Array.flatMap(frames, ({ specification, frame }) => Array.map(Record.toEntries(specification.fields), ([name, style], index) => ({ name, style, frame, index })));
                Array.forEach(fields, ({ name, style, frame, index }) => {
                    const variable = document.textVariables.add({ name, variableType: VariableTypes.CUSTOM_TEXT_TYPE });
                    const options: CustomTextVariablePreference = variable.variableOptions;
                    options.contents = '';
                    if (index > 0) {
                        frame.insertionPoints.lastItem().contents = '\r';
                    }
                    frame.insertionPoints.lastItem().properties = { appliedParagraphStyle: lettering.note.style, pointSize: lettering.note.style.pointSize, leading: lettering.note.leading };
                    frame.insertionPoints.lastItem().contents = `${name}: `;
                    frame.insertionPoints.lastItem().properties = { pointSize: style.pointSize, leading: style.leading };
                    frame.insertionPoints.lastItem().textVariableInstances.add({ associatedTextVariable: variable });
                });
                const issue = Array.getUnsafe(frames, specifications.indexOf(revision));
                const headings = ['Revision', 'Issued'];
                const table = issue.frame.insertionPoints.lastItem().tables.add({
                    appliedTableStyle: styles.table,
                    columnCount: headings.length,
                    headerRowCount: 1,
                    bodyRowCount: Math.max(1, Math.floor((issue.height - lettering.subtitle.leading - 2 * padding - Number(styles.table.spaceAfter)) / lettering.note.leading) - 1),
                    contents: headings,
                    width: strip.right - strip.left - 2 * padding,
                });
                table.cells.everyItem().texts.everyItem().appliedParagraphStyle = lettering.note.style;
                table.cells.everyItem().properties = { firstBaselineOffset: FirstBaseline.FIXED_HEIGHT, minimumFirstBaselineOffset: lettering.note.above };
                table.rows.everyItem().properties = { autoGrow: true, minimumHeight: lettering.note.leading };
                const letters = Array.filter(Array.range('A'.charCodeAt(0), 'Z'.charCodeAt(0)), (code) => code !== 'I'.charCodeAt(0) && code !== 'O'.charCodeAt(0));
                const addressStyle = lettering[policy.addressRole];
                const addressBorder = Option.getOrElse(policy.border, () => addressStyle.leading);
                const zones = Record.map(
                    {
                        columns: { start: grid.bounds.left, end: grid.bounds.right, length: size.width },
                        rows: { start: grid.bounds.top, end: grid.bounds.bottom, length: size.height },
                    },
                    ({ start, end, length }, axis) => {
                        const count = 2 * Math.round(length / (2 * _ISO.field * POINTS.mm));
                        return Option.isSome(iso)
                            ? Array.makeBy(count, (index) => ({
                                  start: index === 0 ? start : length / 2 + (index - count / 2) * _ISO.field * POINTS.mm,
                                  end: index === count - 1 ? end : length / 2 + (index + 1 - count / 2) * _ISO.field * POINTS.mm,
                              }))
                            : grid.tracks[axis];
                    },
                );
                const borders = {
                    top: [grid.bounds.top - addressBorder, grid.bounds.top],
                    bottom: [grid.bounds.bottom, grid.bounds.bottom + addressBorder],
                    left: [grid.bounds.left - addressBorder, grid.bounds.left],
                    right: [grid.bounds.right, grid.bounds.right + addressBorder],
                } as const;
                const sides = [
                    { axis: 'columns' as const, tracks: zones.columns, borders: Option.exists(iso, ({ index }) => index !== _ISO.singleEdgeIndex) ? [borders.top, borders.bottom] : [borders.top] },
                    {
                        axis: 'rows' as const,
                        tracks: Option.isSome(iso) ? zones.rows : Array.reverse(zones.rows),
                        borders: Option.match(iso, {
                            onNone: () => [borders.left],
                            onSome: ({ index }) => (index === _ISO.singleEdgeIndex ? [borders.right] : [borders.left, borders.right]),
                        }),
                    },
                ];
                const labels = Array.flatMap(sides, ({ axis, tracks, borders: edges }) =>
                    Array.map(tracks, (track, index) => ({
                        axis,
                        track,
                        edges,
                        index,
                        contents:
                            axis === 'columns'
                                ? String(index + 1)
                                : Array.reverse(
                                      Array.unfold(index + 1, (value) =>
                                          value > 0
                                              ? Option.some([String.fromCodePoint(Array.getUnsafe(letters, (value - 1) % letters.length)), Math.floor((value - 1) / letters.length)] as const)
                                              : Option.none(),
                                      ),
                                  ).join(''),
                    })),
                );
                const addresses = Array.flatMap(labels, ({ edges, ...label }) =>
                    Array.map(edges, ([before, after]) => ({
                        ...label,
                        bounds: label.axis === 'columns' ? ([before, label.track.start, after, label.track.end] as const) : ([label.track.start, before, label.track.end, after] as const),
                    })),
                );
                Array.forEach(addresses, (address) => {
                    const frameTop = (address.bounds[0] + address.bounds[2] + heights[policy.addressRole]) / 2 - addressStyle.above;
                    const frame = page.textFrames.add(layers.folios, {
                        geometricBounds: [frameTop, address.bounds[1], frameTop + addressStyle.leading, address.bounds[3]],
                        appliedObjectStyle: styles.objects.text,
                        contents: address.contents,
                        textFramePreferences: {
                            firstBaselineOffset: FirstBaseline.FIXED_HEIGHT,
                            minimumFirstBaselineOffset: addressStyle.above,
                            verticalJustification: VerticalJustification.TOP_ALIGN,
                        },
                    });
                    frame.parentStory.properties = { appliedParagraphStyle: addressStyle.style, justification: Justification.CENTER_ALIGN };
                });
                Option.map(iso, () => {
                    page.rectangles.add(layers.folios, {
                        geometricBounds: [grid.bounds.top - addressBorder, grid.bounds.left - addressBorder, grid.bounds.bottom + addressBorder, grid.bounds.right + addressBorder],
                        fillColor: document.swatches.firstItem(),
                        strokeColor: colors.Ink,
                        strokeWeight: _ISO.rule * POINTS.mm,
                    });
                    Array.forEach(
                        Array.filter(addresses, ({ index }) => index > 0),
                        ({ bounds, axis }) =>
                            page.graphicLines.add(layers.folios, {
                                geometricBounds: axis === 'columns' ? [bounds[0], bounds[1], bounds[2], bounds[1]] : [bounds[0], bounds[1], bounds[0], bounds[3]],
                                strokeColor: colors.Ink,
                                strokeWeight: _ISO.rule * POINTS.mm,
                            }),
                    );
                    Array.forEach(
                        [
                            [grid.bounds.top - addressBorder, size.width / 2, grid.bounds.top + _ISO.extension * POINTS.mm, size.width / 2],
                            [grid.bounds.bottom - _ISO.extension * POINTS.mm, size.width / 2, grid.bounds.bottom + addressBorder, size.width / 2],
                            [size.height / 2, grid.bounds.left - addressBorder, size.height / 2, grid.bounds.left + _ISO.extension * POINTS.mm],
                            [size.height / 2, grid.bounds.right - _ISO.extension * POINTS.mm, size.height / 2, grid.bounds.right + addressBorder],
                        ],
                        (geometricBounds) => page.graphicLines.add(layers.folios, { geometricBounds, strokeColor: colors.Ink, strokeWeight: _ISO.frame * POINTS.mm }),
                    );
                });
                const noteColumn = Array.lastNonEmpty(grid.tracks.columns);
                const lastRow = Array.lastNonEmpty(grid.tracks.rows);
                const drawing = page.rectangles.add(layers.figures, {
                    name: 'Drawing',
                    geometricBounds: [grid.bounds.top, grid.bounds.left, grid.bounds.bottom, noteColumn.start],
                    appliedObjectStyle: styles.objects.figure,
                    textWrapPreferences: { textWrapMode: TextWrapModes.NONE },
                });
                const notes = page.textFrames.add(layers.text, {
                    name: 'Sheet Notes',
                    geometricBounds: [grid.bounds.top, noteColumn.start, lastRow.start, noteColumn.end],
                    appliedObjectStyle: styles.objects.text,
                    textFramePreferences: { insetSpacing: padding, firstBaselineOffset: FirstBaseline.FIXED_HEIGHT, minimumFirstBaselineOffset: lettering.note.above },
                });
                notes.parentStory.appliedParagraphStyle = lettering.note.style;
                const keyPlan = page.rectangles.add(layers.figures, {
                    name: 'Key Plan',
                    geometricBounds: [lastRow.start, noteColumn.start, lastRow.end, noteColumn.end],
                    appliedObjectStyle: styles.objects.figure,
                    textWrapPreferences: { textWrapMode: TextWrapModes.NONE },
                });
                const productionHeight = 2 * lettering.note.leading + 2 * padding;
                document.documentPreferences.properties = { documentSlugUniformSize: false, slugTopOffset: productionHeight };
                const production = page.textFrames.add(layers.folios, {
                    name: 'Production',
                    geometricBounds: [-productionHeight, grid.bounds.left, 0, grid.bounds.right],
                    appliedObjectStyle: styles.objects.text,
                    textFramePreferences: { insetSpacing: padding, firstBaselineOffset: FirstBaseline.FIXED_HEIGHT, minimumFirstBaselineOffset: lettering.note.above },
                });
                production.parentStory.appliedParagraphStyle = lettering.note.style;
                Array.forEach([resources.fileName, resources.outputDate], (variable, index) => {
                    if (index > 0) {
                        production.insertionPoints.lastItem().contents = '\r';
                    }
                    production.insertionPoints.lastItem().textVariableInstances.add({ associatedTextVariable: variable });
                });
                const sheet = document.pages.firstItem();
                sheet.appliedMaster = foundation;
                Array.forEach([drawing, notes, keyPlan, ...Array.map(frames, Struct.get('frame'))], (item) => resources.article.articleMembers.add(item.override(sheet)));
            }
            const captionLeading = Number(styles.paragraphs.caption.leading);
            const caption = { style: styles.objects.caption, height: captionLeading + styles.extents.caption.below, gap: Math.ceil(captionLeading / size.module) * size.module - captionLeading };
            const captionPadding = { top: styles.ink.above - styles.alignment.above, bottom: styles.ink.below - styles.extents.caption.below };
            const firstColumn = Array.headNonEmpty(grid.tracks.columns);
            const text = Record.map(
                Struct.pick(styles.paragraphs, ['body', 'first', 'title', 'lead', 'heading', 'subtitle', 'indicator', 'contentsTitle', 'contents1', 'contents2']),
                (style, role) => {
                    const leading = Number(style.leading);
                    const baseline = styles.ink.above + Math.ceil((styles.extents[role].above - styles.ink.above) / size.module) * size.module;
                    return {
                        item: Content.text({ role, object: styles.objects.text, baseline, columns: 1, variable: Option.none() }),
                        minimum: baseline + styles.extents[role].below + leading * (style.keepLinesTogether ? Math.max(style.keepFirstLines, style.keepLastLines) - 1 : 0),
                        preferred: (extent.height * Number(styles.paragraphs.body.leading)) / leading,
                        width: firstColumn.end - firstColumn.start,
                    };
                },
            );
            const figure = {
                item: Content.figure({ caption }),
                minimum: caption.height + caption.gap + captionPadding.top + captionPadding.bottom + size.module,
                preferred: extent.height,
                width: size.module,
            };
            const bleed = Number(document.documentPreferences.documentBleedTopOffset);
            const edge = { geometricBounds: [-bleed, -bleed, size.height + bleed, size.width + bleed], textWrapPreferences: { textWrapMode: TextWrapModes.NONE } };
            const paper = { paragraphs: styles.paragraphs, backgrounds: [] };
            const dark = { paragraphs: styles.white, backgrounds: [{ ...edge, appliedObjectStyle: styles.objects.field, fillColor: colors.Ink }] };
            const image = { paragraphs: styles.white, backgrounds: [{ ...edge, appliedObjectStyle: styles.objects.figure, fillColor: colors.Ink }] };
            const body = { ...paper, name: 'Text', panels: [[text.body]] } satisfies Composition;
            const cover = { ...paper, name: 'Cover', panels: [[text.title, text.lead]] } satisfies Composition;
            const contents = { ...paper, name: 'Contents', panels: [[text.contentsTitle, text.contents1]] } satisfies Composition;
            const banded = { ...dark, paragraphs: { ...styles.paragraphs, title: styles.white.title }, name: 'Banded Title', panels: [[text.title, figure]] } satisfies Composition;
            const competition = {
                ...paper,
                name: 'Competition',
                panels: [
                    [
                        {
                            ...text.indicator,
                            item: Content.text({ ...text.indicator.item, variable: entry }),
                            minimum: _PAGE.competition.height * POINTS.mm,
                            width: _PAGE.competition.width * POINTS.mm,
                        },
                        figure,
                    ],
                ],
            } satisfies Composition;
            const layouts: readonly Composition[] = Option.isSome(grid.strip)
                ? []
                : [
                      body,
                      cover,
                      contents,
                      banded,
                      ...(size.family === 'board' ? [competition] : []),
                      { ...paper, name: 'Two Column Text', panels: [[{ ...text.body, item: Content.text({ ...text.body.item, columns: 2 }), width: 2 * text.body.width + grid.gutters.columns }]] },
                      { ...paper, name: 'Title and Image', panels: [[text.indicator, text.title, text.lead], [figure]] },
                      { ...dark, name: 'Dark Title', panels: [[text.indicator, text.title, text.lead], [text.contents1]] },
                      { ...image, name: 'Image Overlay', panels: [[text.indicator, text.title, text.lead]] },
                      { ...paper, name: 'Section', panels: [[text.indicator, text.title], [text.contents1]] },
                      { ...dark, name: 'Dark Section', panels: [[text.indicator, text.title], [text.contents1]] },
                      { ...paper, name: 'Image Led', panels: [[figure, text.title]] },
                      { ...image, name: 'Full Bleed Image', panels: [[text.indicator]] },
                      {
                          ...paper,
                          name: 'Text Led',
                          panels: [
                              [text.heading, text.first],
                              [figure, figure],
                          ],
                      },
                      {
                          ...paper,
                          name: 'Comparison',
                          panels: [
                              [text.heading, figure],
                              [text.subtitle, figure],
                          ],
                      },
                      { ...paper, name: 'Diagram', panels: [[figure], [text.heading, text.contents2]] },
                      {
                          ...paper,
                          name: 'Gallery',
                          panels: [
                              [text.heading, figure, figure],
                              [text.subtitle, figure, figure],
                          ],
                      },
                      { ...paper, name: 'Asymmetric Gallery', panels: [[figure], [text.heading, figure, figure]] },
                  ];
            const [excluded, resolved] = yield* Effect.partition(
                layouts,
                Effect.fnUntraced(
                    function* (layout: Composition) {
                        const horizontal = yield* Effect.fromResult(
                            partition(
                                grid.tracks,
                                'columns',
                                Array.map(layout.panels, (panel) => {
                                    const figures = Array.countBy(panel, ({ item }) => Content.$is('figure')(item));
                                    return {
                                        item: panel,
                                        minimum: Math.max(...Array.map(panel, Struct.get('width'))),
                                        preferred: figures > 0 ? extent.height / figures : extent.width / layout.panels.length,
                                    };
                                }),
                            ),
                        ).pipe(Effect.mapError(Array.of));
                        const panels = yield* Effect.validate(horizontal, ({ item, tracks }) =>
                            Effect.map(Effect.fromResult(partition(tracks, 'rows', item)), (placements) => ({ tracks, placements })),
                        );
                        return { layout, panels };
                    },
                    (effect, layout) => Effect.mapError(effect, (errors) => ({ layout, errors })),
                ),
            );
            const bodyFailure = Array.findFirst(excluded, ({ layout }) => layout === body);
            if (Option.isSome(bodyFailure)) {
                return yield* Effect.fail(bodyFailure.value.errors);
            }
            const parents = Array.map(resolved, ({ layout, panels }) => {
                const spread = document.masterSpreads.add(1, { baseName: layout.name, appliedMaster: foundation });
                const page = spread.pages.firstItem();
                page.layoutRule = LayoutRuleOptions.OBJECT_BASED;
                page.marginPreferences.properties = document.marginPreferences.properties;
                Array.forEach(layout.backgrounds, (properties) =>
                    page.rectangles.add(layers.fields, {
                        ...properties,
                        geometricBounds:
                            layout === banded
                                ? [-bleed, -bleed, Array.lastNonEmpty(Array.headNonEmpty(Array.getUnsafe(panels, 0).placements).tracks.rows).end, size.width + bleed]
                                : properties.geometricBounds,
                        horizontalLayoutConstraints: [DimensionsConstraints.FIXED_DIMENSION, DimensionsConstraints.FLEXIBLE_DIMENSION, DimensionsConstraints.FIXED_DIMENSION],
                        verticalLayoutConstraints:
                            layout === banded
                                ? [DimensionsConstraints.FIXED_DIMENSION, DimensionsConstraints.FIXED_DIMENSION, DimensionsConstraints.FLEXIBLE_DIMENSION]
                                : [DimensionsConstraints.FIXED_DIMENSION, DimensionsConstraints.FLEXIBLE_DIMENSION, DimensionsConstraints.FIXED_DIMENSION],
                    }),
                );
                const container = page.flexObjects.add({
                    name: layout.name,
                    itemLayer: layers.text,
                    geometricBounds: [grid.bounds.top, grid.bounds.left, grid.bounds.bottom, grid.bounds.right],
                    appliedObjectStyle: styles.objects.row,
                    horizontalLayoutConstraints: [DimensionsConstraints.FIXED_DIMENSION, DimensionsConstraints.FLEXIBLE_DIMENSION, DimensionsConstraints.FIXED_DIMENSION],
                    verticalLayoutConstraints: [DimensionsConstraints.FIXED_DIMENSION, DimensionsConstraints.FLEXIBLE_DIMENSION, DimensionsConstraints.FIXED_DIMENSION],
                });
                return { layout, spread, page, container, panels };
            });
            const columns = Array.map(
                Array.flatMap(parents, (parent) => Array.map(parent.panels, (panel) => ({ parent, ...panel }))),
                ({ parent, tracks, placements }) => {
                    const width = Array.lastNonEmpty(tracks.columns).end - Array.headNonEmpty(tracks.columns).start;
                    const column = parent.page.flexObjects.add({
                        itemLayer: layers.text,
                        geometricBounds: [0, 0, extent.height, width],
                        appliedObjectStyle: styles.objects.column,
                        flexItemWidthMode:
                            tracks.columns.length === Math.max(...Array.map(parent.panels, (panel) => panel.tracks.columns.length)) ? FlexWidthHeightMode.FLEX_FILL : FlexWidthHeightMode.FLEX_FIXED,
                        flexItemHeightMode: FlexWidthHeightMode.FLEX_FILL,
                    });
                    parent.container.addPageItemAtEnd(column);
                    return { parent, column, placements, fillRows: Math.max(...Array.map(placements, (placement) => placement.tracks.rows.length)) };
                },
            );
            const fields = Array.map(
                Array.flatMap(columns, (column) => Array.map(column.placements, (placement) => ({ ...placement, ...Struct.omit(column, ['placements']) }))),
                ({ parent, column, item, tracks, fillRows }) => {
                    const width = Array.lastNonEmpty(tracks.columns).end - Array.headNonEmpty(tracks.columns).start;
                    const height = Array.lastNonEmpty(tracks.rows).end - Array.headNonEmpty(tracks.rows).start;
                    const properties = {
                        geometricBounds: [0, 0, height, width],
                        flexItemWidthMode: FlexWidthHeightMode.FLEX_FILL,
                        flexItemHeightMode: tracks.rows.length === fillRows ? FlexWidthHeightMode.FLEX_FILL : FlexWidthHeightMode.FLEX_FIXED,
                    };
                    if (Content.$is('figure')(item)) {
                        const container = parent.page.flexObjects.add({
                            ...properties,
                            itemLayer: layers.figures,
                            appliedObjectStyle: styles.objects.column,
                            flexGapRow: item.caption.gap,
                            flexPaddingTop: captionPadding.top,
                            flexPaddingBottom: captionPadding.bottom,
                        });
                        column.addPageItemAtEnd(container);
                        return { ...item, parent, container, width };
                    }
                    const style = parent.layout.paragraphs[item.role];
                    const frame = parent.page.textFrames.add(layers.text, {
                        ...properties,
                        name: style.name,
                        appliedObjectStyle: item.object,
                        textFramePreferences: {
                            firstBaselineOffset: FirstBaseline.FIXED_HEIGHT,
                            minimumFirstBaselineOffset: item.baseline,
                            textColumnCount: item.columns,
                            textColumnGutter: grid.gutters.columns,
                            verticalBalanceColumns: item.columns > 1,
                        },
                    });
                    frame.parentStory.appliedParagraphStyle = style;
                    Option.map(item.variable, (variable) => frame.insertionPoints.lastItem().textVariableInstances.add({ associatedTextVariable: variable }));
                    if (parent.layout === competition) {
                        const header = parent.page.flexObjects.add({ ...properties, itemLayer: layers.text, appliedObjectStyle: styles.objects.row });
                        frame.properties = {
                            geometricBounds: [0, 0, _PAGE.competition.height * POINTS.mm, _PAGE.competition.width * POINTS.mm],
                            flexItemWidthMode: FlexWidthHeightMode.FLEX_FIXED,
                            flexItemHeightMode: FlexWidthHeightMode.FLEX_FIXED,
                            strokeColor: colors.Ink,
                            strokeWeight: styles.rule,
                        };
                        const panel = parent.page.textFrames.add(layers.folios, {
                            ...properties,
                            appliedObjectStyle: styles.objects.text,
                            flexItemHeightMode: FlexWidthHeightMode.FLEX_FIXED,
                            textFramePreferences: { firstBaselineOffset: FirstBaseline.FIXED_HEIGHT, minimumFirstBaselineOffset: item.baseline },
                        });
                        panel.parentStory.properties = { appliedParagraphStyle: parent.layout.paragraphs.indicator, justification: Justification.RIGHT_ALIGN };
                        panel.insertionPoints.lastItem().contents = SpecialCharacters.AUTO_PAGE_NUMBER;
                        header.addPageItemAtEnd(frame);
                        header.addPageItemAtEnd(panel);
                        column.addPageItemAtEnd(header);
                        return { ...item, parent, container: frame, width };
                    }
                    column.addPageItemAtEnd(frame);
                    return { ...item, parent, container: frame, width };
                },
            );
            Array.forEach(fields, (field) => {
                if (field._tag !== 'figure') {
                    return;
                }
                const graphic = field.parent.page.rectangles.add(layers.figures, {
                    geometricBounds: [0, 0, size.module, field.width],
                    appliedObjectStyle: styles.objects.figure,
                    textWrapPreferences: { textWrapMode: TextWrapModes.NONE },
                    flexItemWidthMode: FlexWidthHeightMode.FLEX_FILL,
                    flexItemHeightMode: FlexWidthHeightMode.FLEX_FILL,
                });
                const label = field.parent.page.textFrames.add(layers.text, {
                    geometricBounds: [0, 0, field.caption.height, field.width],
                    appliedObjectStyle: field.caption.style,
                    flexItemWidthMode: FlexWidthHeightMode.FLEX_FILL,
                    flexItemHeightMode: FlexWidthHeightMode.FLEX_FIXED,
                });
                label.parentStory.appliedParagraphStyle = field.parent.layout.paragraphs.caption;
                field.container.addPageItemAtEnd(graphic);
                field.container.addPageItemAtEnd(label);
            });
            Array.forEach(
                Array.filter(fields, (field) => field._tag === 'text'),
                ({ parent, container, role }) => {
                    if (role === 'body') {
                        parent.spread.primaryTextFrame = container;
                    }
                },
            );
            Array.forEach(Array.cartesian(parents, Option.toArray(Option.orElse(size.footer, () => size.folio))), ([{ spread, layout }, baseline]) => {
                const page = spread.pages.firstItem();
                const descent = Math.max(styles.extents.running.below, styles.extents.indicator.below);
                const height = baseline + descent - grid.bounds.bottom;
                const footer = page.flexObjects.add({
                    itemLayer: layers.folios,
                    geometricBounds: [grid.bounds.bottom, grid.bounds.left, baseline + descent, grid.bounds.right],
                    appliedObjectStyle: styles.objects.row,
                    flexGapColumn: 0,
                    horizontalLayoutConstraints: [DimensionsConstraints.FIXED_DIMENSION, DimensionsConstraints.FLEXIBLE_DIMENSION, DimensionsConstraints.FIXED_DIMENSION],
                    verticalLayoutConstraints: [DimensionsConstraints.FLEXIBLE_DIMENSION, DimensionsConstraints.FIXED_DIMENSION, DimensionsConstraints.FIXED_DIMENSION],
                });
                const frames = Record.map(Struct.pick(layout.paragraphs, ['running', 'indicator']), (style) => {
                    const frame = page.textFrames.add(layers.folios, {
                        name: style.name,
                        geometricBounds: [0, 0, height, extent.width],
                        appliedObjectStyle: styles.objects.text,
                        flexItemWidthMode: FlexWidthHeightMode.FLEX_FILL,
                        flexItemHeightMode: FlexWidthHeightMode.FLEX_FILL,
                        textFramePreferences: {
                            firstBaselineOffset: FirstBaseline.FIXED_HEIGHT,
                            minimumFirstBaselineOffset: baseline - grid.bounds.bottom,
                            verticalJustification: VerticalJustification.TOP_ALIGN,
                        },
                    });
                    frame.parentStory.appliedParagraphStyle = style;
                    footer.addPageItemAtEnd(frame);
                    return frame;
                });
                frames.running.insertionPoints.lastItem().textVariableInstances.add({ associatedTextVariable: resources.running });
                frames.indicator.insertionPoints.lastItem().contents = SpecialCharacters.AUTO_PAGE_NUMBER;
                frames.indicator.insertionPoints.lastItem().contents = ' / ';
                frames.indicator.insertionPoints.lastItem().textVariableInstances.add({ associatedTextVariable: resources.lastPage });
            });
            const sequence = Match.value(size).pipe(
                Match.when({ family: 'board' }, () => [competition]),
                Match.when({ intent: 'print', sheet: Option.isNone }, () => [cover, contents, body]),
                Match.orElse(() => [body]),
                Array.flatMap((layout) => Option.toArray(Array.findFirst(parents, (candidate) => candidate.layout === layout))),
            );
            Array.forEach(sequence, ({ spread, container }, index) => {
                const page = index === 0 ? document.pages.firstItem() : document.pages.add();
                page.appliedMaster = spread;
                resources.article.articleMembers.add(container.override(page));
            });
            if (sequence.length > 1) {
                document.sections.firstItem().pageNumberStyle = PageNumberStyle.LOWER_ROMAN;
                document.sections.add({ pageStart: document.pages.lastItem(), continueNumbering: false, pageNumberStart: 1, pageNumberStyle: PageNumberStyle.ARABIC });
            }
            layers.grid.properties = { locked: true, visible: false };
            layers.imageLines.properties = { locked: true, visible: false };
            layers.folios.locked = true;
            document.activeLayer = layers.text;
            document.pageItemDefaults.appliedTextObjectStyle = styles.objects.text;
            document.pageItemDefaults.appliedGraphicObjectStyle = styles.objects.figure;
            document.textDefaults.appliedParagraphStyle = styles.paragraphs.body;
            const documentName = master ? 'Default Template' : size.name;
            const template = AbsolutePath.make(`${input.directory}/${master ? '' : 'Sizes/'}${documentName}.indt`);
            const source = AbsolutePath.make(`${input.directory}/Sizes/Book/${documentName}.indd`);
            const description = {
                name: documentName,
                width: Number(document.documentPreferences.pageWidth),
                height: Number(document.documentPreferences.pageHeight),
                leading: Number(document.gridPreferences.baselineDivision),
                styles: {
                    paragraph: Array.map(document.allParagraphStyles, Struct.get('name')),
                    character: Array.map(document.allCharacterStyles, Struct.get('name')),
                    object: Array.map(document.allObjectStyles, Struct.get('name')),
                    table: Array.map(document.allTableStyles, Struct.get('name')),
                    cell: Array.map(document.allCellStyles, Struct.get('name')),
                },
                layouts: {
                    created: [...(Option.isSome(grid.strip) ? [foundation.baseName] : []), ...Array.map(parents, ({ layout }) => layout.name)],
                    excluded: Array.map(excluded, ({ layout, errors }) => ({ name: layout.name, errors })),
                },
            };
            const [writeErrors, outputs] = yield* Effect.partition(
                [
                    { kind: 'document' as const, path: source },
                    { kind: 'template' as const, path: template },
                ],
                (output) =>
                    Effect.flatMap(fileFor(output.path, true), (file) =>
                        Effect.as(Effect.try({ try: () => (output.kind === 'document' ? document.save(file) : document.saveACopy(file, true)), catch: thrown }), output),
                    ),
                { concurrency: 1 },
            );
            const closed = yield* Effect.result(Scope.close(scope, Exit.void).pipe(Effect.catchDefect(flow(thrown, Effect.fail))));
            const failures = [...writeErrors, ...Option.toArray(Result.getFailure(closed))];
            return {
                files: Array.isArrayNonEmpty(outputs) ? [{ ...description, outputs }] : [],
                failures: Array.isArrayNonEmpty(failures) ? [{ name: documentName, errors: failures }] : [],
            };
        },
        Effect.scoped,
        Effect.catchCause((cause): Effect.Effect<never, Extract<Reply<'buildTypography'>, { readonly kind: 'rejected' }>['errors']> => {
            if (Cause.hasInterrupts(cause)) {
                return Effect.failCause(cause);
            }
            const errors = Array.flatMap(cause.reasons, (reason): readonly Extract<Reply<'buildTypography'>, { readonly kind: 'rejected' }>['errors'][number][] => {
                if (Cause.isFailReason(reason)) {
                    return reason.error;
                }
                return Cause.isDieReason(reason) ? [thrown(reason.defect)] : [];
            });
            return Array.isArrayNonEmpty(errors) ? Effect.fail(errors) : Effect.failCause(cause);
        }),
    );

// --- [HANDLER] -------------------------------------------------------------------------

const buildTypography: (input: Body<'buildTypography'>) => Effect.Effect<Reply<'buildTypography'>> = Effect.fnUntraced(
    function* (input) {
        const translations = yield* Effect.all(Record.map({ composer: '$ID/HL Composer', rtl: '$ID/HL Composer Optyca', kerning: '$ID/Metrics' }, translate), { mode: 'result' });
        const requested = HashSet.fromIterable(Array.map(Record.values(input.fonts), flow(Struct.get('native'), Struct.get('postscriptName'))));
        const [unreadable, installed] = Array.partition(
            Array.filter(app.fonts.everyItem().getElements(), (font) => HashSet.has(requested, font.postscriptName)),
            (font) => Result.try({ try: () => ({ font, source: Schema.decodeUnknownSync(FontSource)(font.properties) }), catch: thrown }),
        );
        const selected = Record.map(input.fonts, (measured, role) => {
            const candidates = Array.filter(installed, flow(Struct.get('source'), Equal.equals(measured.native)));
            return Result.fromOption(
                Option.map(
                    Option.filter(Array.head(candidates), () => candidates.length === 1),
                    Struct.get('font'),
                ),
                Function.constant(
                    TypographyError.cases.nativeFontSelection.make({
                        role,
                        postScriptName: measured.native.postscriptName,
                        candidates: Array.map(candidates, Struct.get('source')),
                    }),
                ),
            );
        });
        const availableLanguages = app.languagesWithVendors.everyItem().getElements();
        const chosenLanguages = Record.map({ ...input.languages, code: '[No Language]' as const }, (selection, role) => {
            const candidates = Array.filter(availableLanguages, (language) => (typeof selection === 'number' ? language.id === selection : language.untranslatedName === selection));
            return Result.fromOption(
                Option.filter(Array.head(candidates), () => candidates.length === 1),
                Function.constant(TypographyError.cases.languageSelection.make({ role, selection, candidates: Array.map(candidates, Struct.get('id')) })),
            );
        });
        const selectionErrors = [
            ...unreadable,
            ...Array.flatMap(
                [...Record.values(selected), ...Record.values(chosenLanguages), ...Record.values(translations)],
                (result: Result.Result<Font | LanguageWithVendors | string, typeof TypographyError.Type | HostRejection>) => (Result.isFailure(result) ? [result.failure] : []),
            ),
        ];
        if (Array.isArrayNonEmpty(selectionErrors)) {
            return yield* Effect.fail(selectionErrors);
        }
        const fonts = yield* Effect.fromResult(Result.all(selected)).pipe(Effect.mapError(Array.of));
        const languages = yield* Effect.fromResult(Result.all(chosenLanguages)).pipe(Effect.mapError(Array.of));
        const localized = yield* Effect.fromResult(Result.all(translations)).pipe(Effect.mapError(Array.of));
        const [rejected, completed] = yield* Effect.partition(
            [
                ...Array.map(
                    Array.filter(pageSizes, (size) => size.family === 'digital'),
                    (size) => ({ size, master: true }),
                ),
                ...Array.map(input.scope === 'catalogue' ? pageSizes : [], (size) => ({ size, master: false })),
            ],
            ({ size, master }) =>
                _document(size, input, { fonts, languages, localized }, master).pipe(
                    Effect.map((result) => ({ ...result, master })),
                    Effect.mapError((errors) => ({ name: master ? 'Default Template' : size.name, errors })),
                ),
            { concurrency: 1 },
        );
        const files = Array.flatMap(completed, Struct.get('files'));
        const failures = [...rejected, ...Array.flatMap(completed, Struct.get('failures'))];
        const contents = pipe(
            completed,
            Array.filter(({ master }) => !master),
            Array.flatMap(({ files: generated }) => generated),
            Array.flatMap(({ outputs }) => outputs),
            Array.filter(({ kind }) => kind === 'document'),
        );
        if (Array.isArrayEmpty(contents)) {
            return { kind: 'completed' as const, files, failures, book: Option.none() };
        }
        const bookPath = AbsolutePath.make(`${input.directory}/Sizes/Book/Default Sizes.indb`);
        const scope = yield* Effect.flatMap(Scope.Scope, Scope.fork);
        const opened = yield* Effect.result(
            Effect.acquireRelease(
                Effect.flatMap(fileFor(bookPath, true), (file) => Effect.try({ try: () => app.books.add(file), catch: thrown })),
                (created) => close(created).pipe(Effect.orDie),
            ).pipe(Scope.provide(scope)),
        );
        if (Result.isFailure(opened)) {
            return { kind: 'completed' as const, files, failures: [...failures, { name: bookPath, errors: Array.of(opened.failure) }], book: Option.none() };
        }
        const book = opened.success;
        const publication = yield* Effect.result(
            Effect.flatMap(
                Effect.forEach(contents, ({ path }) => fileFor(path, false)),
                (documents) =>
                    Effect.try({
                        try: () => {
                            Array.forEach(documents, (file) => book.bookContents.add(file));
                            book.properties = {
                                automaticPagination: false,
                                styleSourceDocument: book.bookContents.firstItem(),
                                smartMatchStyleGroups: SmartMatchOptions.MATCH_STYLE_PATH,
                                synchronizeMasterPage: false,
                                synchronizeParagraphStyle: false,
                                synchronizeObjectStyle: false,
                                synchronizeTableStyle: false,
                                synchronizeCellStyle: false,
                                synchronizeCharacterStyle: false,
                                synchronizeSwatch: true,
                                synchronizeConditionalText: true,
                                synchronizeCrossReferenceFormat: false,
                                synchronizeTextVariable: false,
                                synchronizeTableOfContentStyle: false,
                                synchronizeBulletNumberingList: true,
                                synchronizeTrapStyle: false,
                            };
                            book.synchronize();
                            book.save();
                            return bookPath;
                        },
                        catch: thrown,
                    }),
            ),
        );
        const closed = yield* Effect.result(Scope.close(scope, Exit.void).pipe(Effect.catchDefect(flow(thrown, Effect.fail))));
        const bookErrors = [...Option.toArray(Result.getFailure(publication)), ...Option.toArray(Result.getFailure(closed))];
        return {
            kind: 'completed' as const,
            files,
            failures: [...failures, ...(Array.isArrayNonEmpty(bookErrors) ? [{ name: bookPath, errors: bookErrors }] : [])],
            book: Result.getSuccess(publication),
        };
    },
    Effect.scoped,
    Effect.catch((errors) => Effect.succeed({ kind: 'rejected' as const, errors })),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { buildTypography };
