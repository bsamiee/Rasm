// --- [IMPORTS] -------------------------------------------------------------------------

import { AnchorPoint, BoundingBoxLimits, CoordinateSpaces, MeasurementUnits, PageSideOptions } from 'adobe:indesign';
import type { HostRejection } from '@rasm/creative-cloud-server/errors';
import type { Body, Reply } from '@rasm/creative-cloud-server/indesign/jobs';
import { Array, Effect, Option, Record, Struct } from 'effect';
import { documentFor, swapped } from '../host.ts';

// --- [HANDLER] -------------------------------------------------------------------------

const getLayout: (body: Body<'getLayout'>) => Effect.Effect<Reply<'getLayout'>, HostRejection> = Effect.fnUntraced(function* ({
    documentId,
    includeItems,
    pageCursor,
    itemCursor,
    limit,
}: Body<'getLayout'>) {
    const doc = yield* documentFor(documentId);
    yield* swapped(doc.viewPreferences, { horizontalMeasurementUnits: MeasurementUnits.POINTS, verticalMeasurementUnits: MeasurementUnits.POINTS });
    const pageCount = doc.pages.length;
    const pages = Array.unfold(pageCursor, (index) => (index < Math.min(pageCount, pageCursor + limit) ? Option.some([doc.pages.item(index), index + 1]) : Option.none()));
    const items = includeItems ? Array.flatMap(pages, (page, index) => Array.map(page.allPageItems, (item) => ({ page: index, item }))) : [];
    const shown = Array.groupBy(
        Array.map(Array.take(Array.drop(items, itemCursor), limit), ({ page, item }) => {
            const corners = Array.map([AnchorPoint.TOP_LEFT_ANCHOR, AnchorPoint.TOP_RIGHT_ANCHOR, AnchorPoint.BOTTOM_RIGHT_ANCHOR, AnchorPoint.BOTTOM_LEFT_ANCHOR], (anchor) => {
                const [[x, y]] = item.resolve([anchor, BoundingBoxLimits.GEOMETRIC_PATH_BOUNDS], CoordinateSpaces.PAGE_COORDINATES);
                return { x: Number(x), y: Number(y) };
            });
            const xs = Array.map(corners, Struct.get('x'));
            const ys = Array.map(corners, Struct.get('y'));
            return {
                page,
                item: {
                    id: item.id,
                    type: item.constructor.name,
                    name: item.name,
                    bounds: { top: Math.min(...ys), left: Math.min(...xs), bottom: Math.max(...ys), right: Math.max(...xs) },
                    hasGraphic: item.graphics.length > 0,
                },
            };
        }),
        ({ page }) => String(page),
    );
    return {
        kind: 'layout' as const,
        document: { id: doc.id, name: doc.name },
        facingPages: doc.documentPreferences.facingPages,
        measurementUnits: { horizontal: String(doc.viewPreferences.horizontalMeasurementUnits), vertical: String(doc.viewPreferences.verticalMeasurementUnits) },
        pages: Array.map(pages, (page, index) => {
            const [[left, top]] = page.resolve(AnchorPoint.TOP_LEFT_ANCHOR, CoordinateSpaces.PAGE_COORDINATES);
            const [[right, bottom]] = page.resolve(AnchorPoint.BOTTOM_RIGHT_ANCHOR, CoordinateSpaces.PAGE_COORDINATES);
            const margins = {
                top: Number(page.marginPreferences.top),
                bottom: Number(page.marginPreferences.bottom),
                inside: Number(page.marginPreferences.left),
                outside: Number(page.marginPreferences.right),
            };
            const [contentLeft, contentRight] = page.side.equals(PageSideOptions.LEFT_HAND) ? [left + margins.outside, right - margins.inside] : [left + margins.inside, right - margins.outside];
            return {
                index: page.documentOffset,
                id: page.id,
                name: page.name,
                documentOffset: page.documentOffset,
                side: String(page.side),
                bounds: { top, left, bottom, right },
                margins,
                contentArea: { top: top + margins.top, left: contentLeft, bottom: bottom - margins.bottom, right: contentRight },
                guides: Array.map(page.guides.everyItem().getElements(), (guide) => {
                    const [[startX, startY]] = guide.resolve(AnchorPoint.TOP_LEFT_ANCHOR, CoordinateSpaces.PAGE_COORDINATES);
                    const [[endX, endY]] = guide.resolve(AnchorPoint.BOTTOM_RIGHT_ANCHOR, CoordinateSpaces.PAGE_COORDINATES);
                    return { id: guide.id, line: { start: { x: Number(startX), y: Number(startY) }, end: { x: Number(endX), y: Number(endY) } } };
                }),
                items: includeItems
                    ? Option.some(
                          Array.map(
                              Option.getOrElse(Record.get(shown, String(index)), () => []),
                              Struct.get('item'),
                          ),
                      )
                    : Option.none(),
            };
        }),
        pageCount,
        pageCursor: Option.liftPredicate(pageCursor + limit, (next) => next < pageCount),
        itemCount: items.length,
        itemCursor: Option.liftPredicate(itemCursor + limit, (next) => next < items.length),
    };
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { getLayout };
