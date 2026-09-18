/// <reference path="./prelude.ts"/>

declare global {
    enum ElementPlacement {}
    enum Justification {}
    enum ParagraphDirectionType {}
    enum TextOrientation {}

    interface TextFrame {
        duplicate: (relativeObject?: object, insertionLocation?: ElementPlacement) => TextFrame;
    }
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, color, fold, items, present, run, select, split, typed, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [REQUEST] -------------------------------------------------------------------------

interface Highlight {
    readonly width: { readonly kind: 'fraction' | 'points'; readonly value: number };
    readonly height: { readonly kind: 'fraction' | 'points'; readonly value: number };
    readonly glyph: string;
    readonly anchor: [number, number];
    readonly color: ColorSpec;
    readonly yOffset: number;
    readonly dash: { readonly dash: number; readonly gap: number; readonly weight: number }[];
    readonly effect: string[];
    readonly groupPerFrame: boolean;
}

interface Measured {
    readonly left: number;
    readonly top: number;
    readonly width: number;
    readonly height: number;
}

const PERCENT = 100;

// --- [MEASUREMENT] ---------------------------------------------------------------------

const alignedLeft = (frame: TextFrame, line: TextRange, width: number): number => {
    const { justification, paragraphDirection } = line.paragraphAttributes;
    if (justification === Justification.CENTER) {
        return frame.left + (frame.width - width) / 2;
    }
    const leftReadsRight = justification === Justification.LEFT && paragraphDirection === ParagraphDirectionType.RIGHT_TO_LEFT_DIRECTION;
    const right = justification === Justification.RIGHT || leftReadsRight;
    return right ? frame.left + frame.width - width : frame.left;
};

const outlineBox = (clone: TextFrame, contents: string): Measured => {
    const copy = clone.duplicate();
    copy.contents = contents;
    const outline = copy.createOutline();
    const box = { left: outline.left, top: outline.top, width: outline.width, height: outline.height };
    outline.remove();
    return box;
};

const measured = (frame: TextFrame, line: TextRange, top: number, left: number, glyph: string): Measured => {
    const clone = frame.duplicate();
    clone.contents = line.contents === '' ? glyph : line.contents;
    if (frame.orientation === TextOrientation.VERTICAL) {
        clone.left = left;
    } else {
        clone.top = top;
        clone.left = alignedLeft(frame, line, clone.width);
    }
    const box = outlineBox(clone, clone.contents);
    const rule = outlineBox(clone, glyph);
    clone.remove();
    return { left: box.left, top: rule.top, width: box.width, height: rule.height };
};

// --- [DRAWING] -------------------------------------------------------------------------

const drawn = (frame: TextFrame, dest: Measured, highlight: Highlight, fill: Color): PathItem => {
    const absW = highlight.width.kind === 'points';
    const absH = highlight.height.kind === 'points';
    const boxW = absW ? highlight.width.value : dest.width;
    const boxH = absH ? highlight.height.value : dest.height;
    const [horizontal, vertical] = highlight.anchor;
    const left = absW ? dest.left - horizontal * (boxW - dest.width) : dest.left + horizontal * boxW * (1 - highlight.width.value);
    const shifted = absH ? dest.top + vertical * (boxH - dest.height) : dest.top - vertical * boxH * (1 - highlight.height.value);
    const thickness = boxH * (absH ? 1 : highlight.height.value);
    const width = boxW * (absW ? 1 : highlight.width.value);
    const rect = frame.layer.pathItems.rectangle(shifted + highlight.yOffset, left, width, thickness);
    rect.filled = true;
    rect.fillColor = fill;
    rect.stroked = highlight.dash.length > 0;
    visit(highlight.dash, (dash): void => {
        rect.strokeColor = fill;
        rect.strokeWidth = dash.weight;
        rect.strokeDashes = [dash.dash, dash.gap];
    });
    visit(highlight.effect, (effect): void => {
        rect.applyEffect(effect);
    });
    rect.move(frame, ElementPlacement.PLACEAFTER);
    return rect;
};

// --- [ENTRY] ---------------------------------------------------------------------------

const text = (request: { readonly highlight: Highlight }): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const { highlight } = request;
    const fill = color(doc, highlight.color);
    const selected = items<PageItem | TextRange>(doc.selection);
    const [range] = select(selected, typed<TextRange>('TextRange'));
    const frames =
        range === undefined
            ? collect(select(selected, typed<TextFrame>('TextFrame')), (frame) => ({ frame, lines: items(frame.lines) }))
            : [{ frame: (range.parent as { readonly parent: TextFrame }).parent, lines: items(range.lines) }];
    return present(
        split(
            collect(frames, ({ frame, lines }): JsonObject => {
                const { rects } = fold<TextRange, { readonly top: number; readonly left: number; readonly rects: PathItem[] }>(
                    lines,
                    { top: frame.top, left: frame.left, rects: [] },
                    (state, line) => {
                        const dest = measured(frame, line, state.top, state.left, highlight.glyph);
                        const attributes = line.characterAttributes;
                        const leading = attributes.autoLeading ? (attributes.size * line.paragraphAttributes.autoLeadingAmount) / PERCENT : attributes.leading;
                        return { top: state.top - leading, left: state.left - leading, rects: state.rects.concat([drawn(frame, dest, highlight, fill)]) };
                    },
                );
                if (highlight.groupPerFrame) {
                    const group = frame.layer.groupItems.add();
                    group.move(frame, ElementPlacement.PLACEAFTER);
                    visit(rects, (rect): void => {
                        rect.move(group, ElementPlacement.PLACEATEND);
                    });
                }
                return { frame: frame.uuid, lines: rects.length };
            }),
        ),
    );
};

run(text);
