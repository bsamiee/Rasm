/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, color, flatten, items, run, select, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [STYLES] --------------------------------------------------------------------------

interface Style {
    readonly name: string;
    readonly weight: number;
    readonly dash: number[];
    readonly cap: string;
    readonly join: string;
    readonly miterLimit: number;
    readonly strokeColor: ColorSpec;
    readonly fillColor?: ColorSpec;
    readonly blendMode: string;
    readonly targets: 'selection' | string[];
}

const targeted = (doc: Document, targets: Style['targets']): PageItem[] =>
    flatten(targets === 'selection' ? items<PageItem>(doc.selection) : collect(targets, (uuid): PageItem => doc.getPageItemFromUuid(uuid) as PageItem));

const styled = (item: PathItem, style: Style): void => {
    const path = item;
    path.stroked = true;
    path.strokeWidth = style.weight;
    path.strokeDashes = style.dash;
    path.strokeCap = $.global.StrokeCap[style.cap];
    path.strokeJoin = $.global.StrokeJoin[style.join];
    path.strokeMiterLimit = style.miterLimit;
    path.strokeColor = color(style.strokeColor);
    path.filled = style.fillColor !== undefined;
    if (style.fillColor !== undefined) {
        path.fillColor = color(style.fillColor);
    }
    path.blendingMode = $.global.BlendModes[style.blendMode];
};

// --- [ENTRY] ---------------------------------------------------------------------------

const strokeStyles = (request: { readonly styles: Style[] }, _at: Site): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const applied: JsonObject[] = [];
    const rejected: JsonObject[] = [];
    visit(request.styles, (style): void => {
        const reached = targeted(doc, style.targets);
        const paths = select(reached, (item): boolean => item.typename === 'PathItem' || item.typename === 'CompoundPathItem');
        visit(paths, (item): void => {
            if (item.typename === 'PathItem') {
                styled(item as PathItem, style);
                return;
            }
            visit(items((item as CompoundPathItem).pathItems), (path): void => styled(path, style));
        });
        applied.push({ name: style.name, items: paths.length });
        visit(
            select(reached, (item): boolean => item.typename !== 'PathItem' && item.typename !== 'CompoundPathItem'),
            (item): void => {
                rejected.push({ name: style.name, uuid: item.uuid, typename: item.typename, reason: 'notAPath' });
            },
        );
    });
    return { value: { kind: 'strokesStyled', applied, rejected }, unavailable: [] };
};

run(strokeStyles);
