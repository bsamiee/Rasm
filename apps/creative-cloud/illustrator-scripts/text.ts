/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, colors, flatMap, flatten, fold, items, nth, present, run, select, split, typed, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [REQUEST] -------------------------------------------------------------------------

interface Highlight {
    readonly width: { readonly kind: 'fraction' | 'points'; readonly value: number };
    readonly height: { readonly kind: 'fraction' | 'points'; readonly value: number };
    readonly glyph: string;
    readonly anchor: [number, number];
    readonly color: ColorSpec;
    readonly yOffset: number;
    readonly dash: { readonly dash: number; readonly gap: number }[];
    readonly effect: string[];
    readonly groupPerFrame: boolean;
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

const outlineBox = (clone: TextFrame): Rect => {
    const copy = clone.duplicate();
    const acquired: PageItem[] = [copy];
    try {
        const outline = copy.createOutline();
        acquired[0] = outline;
        return outline.geometricBounds;
    } finally {
        visit(acquired, (item): void => item.remove());
    }
};

const measured = (frame: TextFrame, line: TextRange, top: number, left: number, glyph: string): Rect => {
    const clone = frame.duplicate();
    try {
        clone.contents = '';
        line.duplicate(nth(clone.insertionPoints, 0), ElementPlacement.PLACEATBEGINNING);
        if (clone.contents === '') {
            clone.contents = glyph;
        }
        if (frame.orientation === TextOrientation.VERTICAL) {
            clone.left = left;
        } else {
            clone.top = top;
            clone.left = alignedLeft(frame, line, clone.width);
        }
        const box = outlineBox(clone);
        clone.contents = glyph;
        const rule = outlineBox(clone);
        return [box[0], rule[1], box[2], rule[3]];
    } finally {
        clone.remove();
    }
};

// --- [DRAWING] -------------------------------------------------------------------------

const drawn = (frame: TextFrame, dest: Rect, highlight: Highlight, fill: Color): PathItem => {
    const absW = highlight.width.kind === 'points';
    const absH = highlight.height.kind === 'points';
    const measuredWidth = dest[2] - dest[0];
    const measuredHeight = dest[1] - dest[3];
    const boxW = absW ? highlight.width.value : measuredWidth;
    const boxH = absH ? highlight.height.value : measuredHeight;
    const [horizontal, vertical] = highlight.anchor;
    const left = absW ? dest[0] - horizontal * (boxW - measuredWidth) : dest[0] + horizontal * boxW * (1 - highlight.width.value);
    const shifted = absH ? dest[1] + vertical * (boxH - measuredHeight) : dest[1] - vertical * boxH * (1 - highlight.height.value);
    const thickness = boxH * (absH ? 1 : highlight.height.value);
    const width = boxW * (absW ? 1 : highlight.width.value);
    const path = frame.layer.pathItems.add();
    try {
        const center = shifted + highlight.yOffset - thickness / 2;
        path.setEntirePath([
            [left, center],
            [left + width, center],
        ]);
        path.filled = false;
        path.stroked = true;
        path.strokeColor = fill;
        path.strokeWidth = thickness;
        visit(highlight.dash, (dash): void => {
            path.strokeDashes = [dash.dash, dash.gap];
        });
        visit(highlight.effect, (effect): void => {
            path.applyEffect(effect);
        });
        path.move(frame, ElementPlacement.PLACEAFTER);
        return path;
    } catch (error) {
        path.remove();
        throw error;
    }
};

// --- [ENTRY] ---------------------------------------------------------------------------

const text = (request: { readonly highlight: Highlight }, at: Site): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const { highlight } = request;
    const ranges = flatMap(items(doc.stories), (story) =>
        flatMap(story.textSelection, (range) =>
            collect(items(story.textFrames), (frame) => ({
                frame,
                start: Math.max(range.start, frame.textRange.start),
                end: Math.min(range.end, frame.textRange.end),
            })),
        ),
    );
    const frames =
        ranges.length === 0
            ? collect(select(flatten(items<PageItem>(doc.selection)), typed<TextFrame>('TextFrame')), (frame) => ({ frame, lines: items(frame.lines) }))
            : collect(
                  select(ranges, ({ start, end }): boolean => end > start),
                  ({ frame, start, end }) => {
                      const range = frame.textRange;
                      range.start = start;
                      range.end = end;
                      return { frame, lines: items(range.lines) };
                  },
              );
    if (frames.length === 0) {
        return present(split([]));
    }
    const paint = colors(doc, [highlight.color], false, at);
    if ('rejected' in paint.value) {
        return { value: split(collect(paint.value.rejected, ({ name, reason }): JsonObject => ({ color: name, reason }))), unavailable: paint.unavailable };
    }
    const fill = nth(paint.value.values, 0);
    return {
        value: split(
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
        unavailable: paint.unavailable,
    };
};

run(text);
