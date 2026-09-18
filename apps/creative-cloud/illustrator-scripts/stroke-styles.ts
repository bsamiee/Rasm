/// <reference path="./prelude.ts"/>

declare global {
    enum BlendModes {}
    enum StrokeCap {}
    enum StrokeJoin {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, color, contains, flatMap, flatten, items, paths, present, run, select, split, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

const strokeStyles = (request: {
    readonly styles: {
        readonly name: string;
        readonly weight: number;
        readonly dash: number[];
        readonly cap: keyof typeof StrokeCap & string;
        readonly join: keyof typeof StrokeJoin & string;
        readonly miterLimit: number;
        readonly strokeColor: ColorSpec;
        readonly fillColor: ColorSpec[];
        readonly blendMode: keyof typeof BlendModes & string;
        readonly targets: 'selection' | string[];
    }[];
}): Reading<JsonObject> => {
    const doc = app.activeDocument;
    return present(
        split(
            flatMap(request.styles, (style): JsonObject[] => {
                const { targets } = style;
                const reached = targets === 'selection' ? flatten(items<PageItem>(doc.selection)) : select(flatten(items(doc.pageItems)), (item): boolean => contains(targets, item.uuid));
                const carriers = collect(reached, (item) => ({ item, list: paths(item) }));
                const stroke = color(doc, style.strokeColor);
                const fills = collect(style.fillColor, (fill): Color => color(doc, fill));
                visit(
                    flatMap(carriers, ({ list }): PathItem[] => list),
                    (item): void => {
                        const path = item;
                        path.stroked = true;
                        path.strokeWidth = style.weight;
                        path.strokeDashes = style.dash;
                        path.strokeCap = StrokeCap[style.cap];
                        path.strokeJoin = StrokeJoin[style.join];
                        path.strokeMiterLimit = style.miterLimit;
                        path.strokeColor = stroke;
                        path.filled = fills.length > 0;
                        visit(fills, (fill): void => {
                            path.fillColor = fill;
                        });
                        path.blendingMode = BlendModes[style.blendMode];
                    },
                );
                const applied: JsonObject = { name: style.name, items: select(carriers, ({ list }): boolean => list.length > 0).length };
                return [applied].concat(
                    collect(
                        select(carriers, ({ list }): boolean => list.length === 0),
                        ({ item }): JsonObject => ({ name: style.name, uuid: item.uuid, typename: item.typename, reason: 'notAPath' }),
                    ),
                );
            }),
        ),
    );
};

run(strokeStyles);
