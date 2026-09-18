// --- [IMPORTS] -------------------------------------------------------------------------

import { app, type Document, type PageItems, PageSideOptions } from 'adobe:indesign';
import { opened } from '@rasm/creative-cloud-server/client';
import type { HostRejection } from '@rasm/creative-cloud-server/errors';
import type { Body, Item, Reply } from '@rasm/creative-cloud-server/indesign/jobs';
import { Array, Effect, identity, Option, Record, Struct } from 'effect';

// --- [READS] ---------------------------------------------------------------------------

const _items = (container: { readonly pageItems: PageItems }): readonly Item[] =>
    Array.flatMap(container.pageItems.everyItem().getElements(), (item) => {
        const [top, left, bottom, right] = item.geometricBounds;
        return [
            { id: item.id, type: item.constructor.name, name: item.name, bounds: { top, left, bottom, right }, hasGraphic: item.graphics.length > 0 },
            ...Array.flatMap(Array.liftPredicate((candidate: typeof item) => candidate.constructor.name === 'Group')(item), _items),
        ];
    });

const _layout = (doc: Document, { includeItems, pageCursor, itemCursor, limit }: Body<'getLayout'>): Reply<'getLayout'> => {
    const pageCount = doc.pages.length;
    const reading = Option.as(Option.liftPredicate(includeItems, identity), _items);
    const rows = Array.map(Array.take(Array.drop(doc.pages.everyItem().getElements(), pageCursor), limit), (page) => {
        const [top, left, bottom, right] = page.bounds;
        const { top: above, bottom: below, left: inside, right: outside } = page.marginPreferences;
        const width = right - left;
        const height = bottom - top;
        const [contentLeft, contentRight] = page.side.equals(PageSideOptions.LEFT_HAND) ? [outside, width - inside] : [inside, width - outside];
        return {
            index: page.documentOffset,
            id: page.id,
            name: page.name,
            documentOffset: page.documentOffset,
            side: String(page.side),
            bounds: { top: 0, left: 0, bottom: height, right: width },
            margins: { top: above, bottom: below, inside, outside },
            contentArea: { top: above, left: contentLeft, bottom: height - below, right: contentRight },
            guides: Array.map(page.guides.everyItem().getElements(), (guide) => ({ id: guide.id, orientation: String(guide.orientation), location: guide.location })),
            items: Option.map(reading, (read) => read(page)),
        };
    });
    const flat = Array.flatMap(rows, (row) =>
        Array.map(
            Option.getOrElse(row.items, () => []),
            (item) => ({ page: row.id, item }),
        ),
    );
    const shown = Array.groupBy(Array.take(Array.drop(flat, itemCursor), limit), ({ page }) => String(page));
    return {
        kind: 'layout',
        facingPages: doc.documentPreferences.facingPages,
        measurementUnits: { horizontal: String(doc.viewPreferences.horizontalMeasurementUnits), vertical: String(doc.viewPreferences.verticalMeasurementUnits) },
        pages: Array.map(rows, (row) => ({
            ...row,
            items: Option.as(
                row.items,
                Array.map(
                    Option.getOrElse(Record.get(shown, String(row.id)), () => []),
                    Struct.get('item'),
                ),
            ),
        })),
        pageCount,
        pageCursor: Option.liftPredicate(pageCursor + limit, (next) => next < pageCount),
        itemCount: flat.length,
        itemCursor: Option.liftPredicate(itemCursor + limit, (next) => next < flat.length),
    };
};

// --- [HANDLER] -------------------------------------------------------------------------

const getLayout = (body: Body<'getLayout'>): Effect.Effect<Reply<'getLayout'>, HostRejection> => Effect.flatMap(opened(app), (doc) => Effect.sync(() => _layout(doc, body)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { getLayout };
