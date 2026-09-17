/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

declare global {
    enum ColorConvertPurpose {}
    enum DocumentColorSpace {}
    enum ElementPlacement {}
    enum ImageColorSpace {}
    enum Justification {}
    enum ParagraphDirectionType {}
    enum TextOrientation {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, color, items, run, select, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [REQUEST] -------------------------------------------------------------------------

interface Extent {
    readonly kind: 'percent' | 'points';
    readonly value: number;
}

interface Highlight {
    readonly width: Extent;
    readonly height: Extent;
    readonly heightBasis: 'xHeight' | 'capHeight' | 'glyph';
    readonly glyph: string;
    readonly anchor: 'topLeft' | 'top' | 'topRight' | 'left' | 'center' | 'right' | 'bottomLeft' | 'bottom' | 'bottomRight';
    readonly color: ColorSpec;
    readonly yOffset: number;
    readonly dash?: { readonly dash: number; readonly gap: number; readonly weight: number };
    readonly zigzag?: { readonly size: number; readonly relative: boolean; readonly ridges: number; readonly smooth: boolean };
    readonly groupPerFrame: boolean;
}

interface Measured {
    readonly left: number;
    readonly top: number;
    readonly width: number;
    readonly height: number;
}

const HALF = 0.5;
const PERCENT = 100;
const BASIS_GLYPH = { xHeight: 'x', capHeight: 'H' };
const ANCHORS: { readonly [Anchor in Highlight['anchor']]: [number, number] } = {
    topLeft: [0, 0],
    top: [HALF, 0],
    topRight: [1, 0],
    left: [0, HALF],
    center: [HALF, HALF],
    right: [1, HALF],
    bottomLeft: [0, 1],
    bottom: [HALF, 1],
    bottomRight: [1, 1],
};
const ZIGZAG = '<LiveEffect name="Adobe Zigzag"><Dict data="R amount #1 R relAmount #2 R absoluteness #3 R ridges #4 R roundness #5 "/></LiveEffect>';

// --- [MEASUREMENT] ---------------------------------------------------------------------

const advance = (line: TextRange): number => {
    const attributes = line.characterAttributes;
    return attributes.autoLeading ? (attributes.size * line.paragraphAttributes.autoLeadingAmount) / PERCENT : attributes.leading;
};

const aligned = (clone: TextFrame, frame: TextFrame, line: TextRange): void => {
    const { justification } = line.paragraphAttributes;
    const rightToLeft = line.paragraphAttributes.paragraphDirection === ParagraphDirectionType.RIGHT_TO_LEFT_DIRECTION;
    const target = clone;
    if (justification === Justification.CENTER) {
        target.left = frame.left + frame.width * HALF - clone.width * HALF;
        return;
    }
    const right = justification === Justification.RIGHT || (justification === Justification.LEFT && rightToLeft);
    target.left = right ? frame.left + frame.width - clone.width : frame.left;
};

const outlineBox = (clone: TextFrame, contents: string): Measured => {
    const copy = clone.duplicate() as TextFrame;
    copy.contents = contents;
    const outline = copy.createOutline();
    const box = { left: outline.left, top: outline.top, width: outline.width, height: outline.height };
    outline.remove();
    return box;
};

const measured = (frame: TextFrame, line: TextRange, top: number, left: number, glyph: string): Measured => {
    const clone = frame.duplicate() as TextFrame;
    clone.contents = line.contents === '' ? glyph : line.contents;
    if (frame.orientation === TextOrientation.VERTICAL) {
        clone.left = left;
    } else {
        clone.top = top;
        aligned(clone, frame, line);
    }
    const box = outlineBox(clone, clone.contents);
    const rule = outlineBox(clone, glyph);
    clone.remove();
    return { left: box.left, top: rule.top, width: box.width, height: rule.height };
};

// --- [GEOMETRY] ------------------------------------------------------------------------

const placedBox = (dest: Measured, highlight: Highlight): { readonly left: number; readonly right: number; readonly top: number; readonly thickness: number } => {
    const absW = highlight.width.kind === 'points';
    const absH = highlight.height.kind === 'points';
    const rW = absW ? highlight.width.value : highlight.width.value / PERCENT;
    const rH = absH ? highlight.height.value : highlight.height.value / PERCENT;
    const boxW = absW ? rW : dest.width;
    const boxH = absH ? rH : dest.height;
    const lineEndX = boxW * (absW ? 1 : rW);
    const [horizontal, vertical] = ANCHORS[highlight.anchor];
    const left = absW ? dest.left - horizontal * (boxW - dest.width) : dest.left + horizontal * boxW * (1 - rW);
    const shifted = absH ? dest.top + vertical * (boxH - dest.height) : dest.top - vertical * boxH * (1 - rH);
    const thickness = boxH * (absH ? 1 : rH);
    return { left, right: left + lineEndX, top: shifted - (HALF * thickness - highlight.yOffset), thickness };
};

const converted = (doc: Document, spec: ColorSpec): Color => {
    const cmyk = doc.documentColorSpace === DocumentColorSpace.CMYK;
    if (spec.model === 'Gray' || (spec.model === 'CMYK') === cmyk) {
        return color(spec);
    }
    const values = app.convertSampleColor(cmyk ? ImageColorSpace.RGB : ImageColorSpace.CMYK, spec.values, cmyk ? ImageColorSpace.CMYK : ImageColorSpace.RGB, ColorConvertPurpose.defaultpurpose);
    return color({ model: cmyk ? 'CMYK' : 'RGB', values });
};

const drawn = (frame: TextFrame, dest: Measured, highlight: Highlight, fill: Color): PathItem => {
    const box = placedBox(dest, highlight);
    const rect = frame.layer.pathItems.rectangle(box.top + box.thickness * HALF, box.left, box.right - box.left, box.thickness);
    rect.filled = true;
    rect.fillColor = fill;
    rect.stroked = highlight.dash !== undefined;
    if (highlight.dash !== undefined) {
        rect.strokeColor = fill;
        rect.strokeWidth = highlight.dash.weight;
        rect.strokeDashes = [highlight.dash.dash, highlight.dash.gap];
    }
    if (highlight.zigzag !== undefined) {
        const z = highlight.zigzag;
        rect.applyEffect(
            ZIGZAG.split('#1')
                .join(String(z.relative ? 0 : z.size))
                .split('#2')
                .join(String(z.relative ? z.size : 0))
                .split('#3')
                .join(z.relative ? '0' : '1')
                .split('#4')
                .join(String(z.ridges))
                .split('#5')
                .join(z.smooth ? '1' : '0'),
        );
    }
    rect.move(frame, ElementPlacement.PLACEAFTER);
    return rect;
};

// --- [ENTRY] ---------------------------------------------------------------------------

const framesOf = (doc: Document): { readonly frame: TextFrame; readonly lines: TextRange[] }[] => {
    const selected = items<PageItem | TextRange>(doc.selection);
    const [range] = select(selected, (item): boolean => item.typename === 'TextRange') as TextRange[];
    if (range !== undefined) {
        const frame = (range.parent as { readonly parent: TextFrame }).parent;
        return [{ frame, lines: items(range.lines) }];
    }
    return collect(select(selected, (item): boolean => item.typename === 'TextFrame') as TextFrame[], (frame): { readonly frame: TextFrame; readonly lines: TextRange[] } => ({ frame, lines: items(frame.lines) }));
};

const text = (request: { readonly highlight: Highlight }, _at: Site): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const { highlight } = request;
    const glyph = highlight.heightBasis === 'glyph' ? highlight.glyph : BASIS_GLYPH[highlight.heightBasis];
    const fill = converted(doc, highlight.color);
    const applied: JsonObject[] = [];
    visit(framesOf(doc), ({ frame, lines }): void => {
        let { top, left } = frame;
        const rects = collect(lines, (line): PathItem => {
            const dest = measured(frame, line, top, left, glyph);
            const leading = advance(line);
            top -= leading;
            left -= leading;
            return drawn(frame, dest, highlight, fill);
        });
        if (highlight.groupPerFrame) {
            const group = frame.layer.groupItems.add();
            group.move(frame, ElementPlacement.PLACEAFTER);
            visit(rects, (rect): void => {
                rect.move(group, ElementPlacement.PLACEATEND);
            });
        }
        applied.push({ frame: frame.uuid, lines: rects.length });
    });
    return { value: { kind: 'highlighted', applied }, unavailable: [] };
};

run(text);
