/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, color, contains, flatMap, flatten, fold, items, nth, paths, present, range, run, select, spec, split, typed, visit }: Prelude = $.evalFile(
    new File(`${new File($.fileName).path}/prelude.jsx`),
);

// --- [GRADIENTS] -----------------------------------------------------------------------

interface Found {
    readonly gradient: Gradient;
    readonly objects: string[];
}

const collected = (doc: Document, attributes: ('fill' | 'stroke')[]): Found[] => {
    const carriers = flatMap(flatten(items<PageItem>(doc.selection)), (item): { readonly uuid: string; readonly path: PathItem }[] => collect(paths(item), (path) => ({ uuid: item.uuid, path })));
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
        const [row] = select(list, (candidate): boolean => candidate.gradient.name === gradient.name);
        if (row === undefined) {
            return list.concat([{ gradient, objects: [uuid] }]);
        }
        return collect(list, (candidate): Found => (candidate === row && !contains(row.objects, uuid) ? { gradient: row.gradient, objects: row.objects.concat([uuid]) } : candidate));
    });
};

// --- [MODES] ---------------------------------------------------------------------------

interface Modes {
    readonly read: { readonly attributes: ('fill' | 'stroke')[] };
    readonly write: {
        readonly attributes: ('fill' | 'stroke')[];
        readonly gradients: { readonly name: string; readonly stops: { readonly position: number; readonly opacity: number; readonly color: ColorSpec }[] }[];
    };
}

const MODES: { readonly [K in keyof Modes]: (doc: Document, request: Modes[K]) => JsonObject } = {
    read: (doc, request) =>
        split(
            collect(collected(doc, request.attributes), ({ gradient, objects }): JsonObject => {
                const listed = items(gradient.gradientStops);
                const stops = flatMap(listed, (stop): JsonObject[] => collect(spec(stop.color), (read): JsonObject => ({ position: stop.rampPoint, opacity: stop.opacity, color: read })));
                return stops.length === listed.length ? { gradient: gradient.name, objects, stops } : { gradient: gradient.name, objects, reason: 'unreadableStop' };
            }),
        ),
    write: (doc, request) => {
        const found = collected(doc, request.attributes);
        const rows = split(
            collect(request.gradients, (wanted): JsonObject => {
                const [row] = select(found, (candidate): boolean => candidate.gradient.name === wanted.name);
                if (row === undefined) {
                    return { gradient: wanted.name, reason: 'notInSelection' };
                }
                const stops = row.gradient.gradientStops;
                const added = Math.max(0, wanted.stops.length - stops.length);
                visit(range(added), (): void => {
                    stops.add();
                });
                visit(items(stops).slice(wanted.stops.length), (stop): void => {
                    stop.remove();
                });
                const listed = items(stops);
                visit(wanted.stops.slice(-1), (last): void => {
                    nth(listed, listed.length - 1).rampPoint = last.position;
                });
                visit(listed, (stop, index): void => {
                    const target = stop;
                    const chosen = nth(wanted.stops, index);
                    target.rampPoint = chosen.position;
                    target.opacity = chosen.opacity;
                    target.color = color(doc, chosen.color);
                });
                return { gradient: wanted.name, stopsAdded: added };
            }),
        );
        app.redraw();
        rows['shared'] = collect(
            select(found, (row): boolean => row.objects.length > 1),
            (row): JsonObject => ({ gradient: row.gradient.name, objects: row.objects }),
        );
        return rows;
    },
};

// --- [ENTRY] ---------------------------------------------------------------------------

const gradientBlend = <const K extends keyof Modes>(request: { readonly mode: K } & Modes[K]): Reading<JsonObject> => present(MODES[request.mode](app.activeDocument, request));

run<{ readonly [K in keyof Modes]: { readonly mode: K } & Modes[K] }[keyof Modes]>(gradientBlend);
