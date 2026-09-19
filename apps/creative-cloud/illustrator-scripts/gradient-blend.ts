/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { channelScale, collect, colors, contains, each, flatMap, flatten, fold, items, nth, paths, present, replaceStops, run, select, spec, split, typed }: Prelude = $.evalFile(
    new File(`${new File($.fileName).path}/prelude.jsx`),
);

// --- [GRADIENTS] -----------------------------------------------------------------------

interface Found {
    readonly gradient: Gradient;
    readonly objects: string[];
}

const collected = (selection: PageItem[], attributes: ('fill' | 'stroke')[]): Found[] => {
    const carriers = flatMap(flatten(selection), (item): { readonly uuid: string; readonly path: PathItem }[] => collect(paths(item), (path) => ({ uuid: item.uuid, path })));
    const found = flatMap(carriers, ({ uuid, path }): { readonly gradient: Gradient; readonly uuid: string }[] =>
        collect(
            select(
                collect(attributes, (attribute): Color => (attribute === 'fill' ? path.fillColor : path.strokeColor)),
                typed<GradientColor>('GradientColor'),
            ),
            (value) => ({ gradient: value.gradient, uuid }),
        ),
    );
    return fold<{ readonly gradient: Gradient; readonly uuid: string }, Found[]>(found, [], (list, { gradient, uuid }): Found[] => {
        const [row] = select(list, (candidate): boolean => candidate.gradient === gradient);
        if (row === undefined) {
            list.push({ gradient, objects: [uuid] });
        } else if (!contains(row.objects, uuid)) {
            row.objects.push(uuid);
        }
        return list;
    });
};

// --- [MODES] ---------------------------------------------------------------------------

interface Modes {
    readonly read: { readonly attributes: ('fill' | 'stroke')[] };
    readonly write: {
        readonly attributes: ('fill' | 'stroke')[];
        readonly gradients: { readonly name: string; readonly stops: { readonly position: number; readonly midpoint: number; readonly opacity: number; readonly color: ColorSpec }[] }[];
    };
}

const MODES: { readonly [K in keyof Modes]: (doc: Document, request: Modes[K], at: Site) => Reading<JsonObject> } = {
    read: (doc, request, at) => {
        const reading = each(at, collected(items<PageItem>(doc.selection), request.attributes), ({ gradient, objects }): Reading<JsonObject> => {
            const listed = collect(items(gradient.gradientStops), (stop) => ({ stop, specs: spec(stop.color) }));
            if (select(listed, ({ specs }): boolean => specs.length === 0).length > 0) {
                return present({ gradient: gradient.name, objects, reason: 'unreadableStop' });
            }
            const stops = collect(listed, ({ stop, specs }): JsonObject => {
                const read = nth(specs, 0);
                const ink = read.model === 'Spot' ? read.ink : read;
                const rgb = app.convertSampleColor(
                    ImageColorSpace[ink.model === 'GRAY' ? 'GrayScale' : ink.model],
                    ink.model === 'GRAY' ? [(1 - ink.values[0]) * channelScale.percentage] : ink.values,
                    ImageColorSpace.RGB,
                    ColorConvertPurpose.defaultpurpose,
                );
                return { position: stop.rampPoint, midpoint: stop.midPoint, opacity: stop.opacity, color: read, rgb };
            });
            return present({ gradient: gradient.name, objects, stops });
        });
        return { value: split(reading.value), unavailable: reading.unavailable };
    },
    write: (doc, request, at) => {
        const found = collected(items<PageItem>(doc.selection), request.attributes);
        const selected = collect(found, ({ gradient }): Gradient => gradient);
        const references = collected(items(doc.pageItems), ['fill', 'stroke']);
        const written = each(at, request.gradients, (wanted, site): Reading<JsonObject[]> => {
            const [row] = select(found, (candidate): boolean => candidate.gradient.name === wanted.name);
            if (row === undefined) {
                return present([{ gradient: wanted.name, reason: 'notInSelection' }]);
            }
            const paint = colors(
                doc,
                collect(wanted.stops, ({ color }): ColorSpec => color),
                false,
                site,
            );
            if ('rejected' in paint.value) {
                return { value: collect(paint.value.rejected, ({ reason }): JsonObject => ({ gradient: wanted.name, reason })), unavailable: paint.unavailable };
            }
            const { values } = paint.value;
            const added = replaceStops(
                row.gradient,
                collect(wanted.stops, (stop, index) => ({ rampPoint: stop.position, midPoint: stop.midpoint, opacity: stop.opacity, color: nth(values, index) })),
            );
            return present([{ gradient: wanted.name, stopsAdded: added }]);
        });
        const rows = split(Array.prototype.concat.apply([], written.value));
        app.redraw();
        rows['shared'] = collect(
            select(references, (row): boolean => row.objects.length > 1 && contains(selected, row.gradient)),
            (row): JsonObject => ({ gradient: row.gradient.name, objects: row.objects }),
        );
        return { value: rows, unavailable: written.unavailable };
    },
};

// --- [ENTRY] ---------------------------------------------------------------------------

const gradientBlend = <const K extends keyof Modes>(request: { readonly mode: K } & Modes[K], at: Site): Reading<JsonObject> => MODES[request.mode](app.activeDocument, request, at);

run<{ readonly [K in keyof Modes]: { readonly mode: K } & Modes[K] }[keyof Modes]>(gradientBlend);
