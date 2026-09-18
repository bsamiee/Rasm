/// <reference path="./prelude.ts"/>

declare global {
    enum ElementPlacement {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, flatMap, flatten, items, present, run, select, split, typed, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [ENTRY] ---------------------------------------------------------------------------

interface Commands {
    readonly strayPoints: string;
    readonly cleanUp: string;
    readonly simplify: string;
    readonly outlineStroke: string;
    readonly expand: string;
    readonly expandAppearance: string;
    readonly noCompoundPath: string;
    readonly ungroup: string;
    readonly compoundPath: string;
    readonly livePathfinderCrop: string;
}

const cleanupPaths = (request: { readonly operations: (keyof Commands | 'trimMasks')[]; readonly keepFilledMask: boolean; readonly commands: Commands }): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const { commands } = request;
    const selected = items<PageItem>(doc.selection);
    const groups = select(flatten(selected).concat(selected), (item): item is GroupItem => typed<GroupItem>('GroupItem')(item) && item.clipped);
    return present(
        split(
            collect(request.operations, (operation): JsonObject => {
                if (operation !== 'trimMasks') {
                    doc.selection = selected;
                    app.executeMenuCommand(commands[operation]);
                    return { operation, count: selected.length };
                }
                visit(
                    flatMap(groups, (group): PathItem[] => items(group.pathItems)),
                    (path): void => {
                        const member = path;
                        member.evenodd = false;
                    },
                );
                const glyphs = flatMap(
                    flatMap(groups, (group): TextFrame[] => items(group.textFrames)),
                    (frame): { readonly glyph: PathItem; readonly fill: Color }[] => {
                        const fill = frame.textRange.characterAttributes.fillColor;
                        return collect(items(frame.createOutline().pathItems), (glyph) => ({ glyph, fill }));
                    },
                );
                visit(glyphs, ({ glyph, fill }): void => {
                    const outlined = glyph;
                    outlined.fillColor = fill;
                });
                visit(
                    flatMap(groups, (group): CompoundPathItem[] => items(group.compoundPathItems)),
                    (compound): void => {
                        doc.selection = [compound];
                        app.executeMenuCommand(commands.noCompoundPath);
                        app.executeMenuCommand(commands.ungroup);
                        app.executeMenuCommand(commands.compoundPath);
                    },
                );
                const cropped = flatMap(groups, (group): { readonly item: PageItem; readonly opacity: number; readonly blendingMode: BlendModes }[] => {
                    const [filledMask] = select(items(group.pathItems), (path): boolean => path.clipping && path.filled);
                    if (request.keepFilledMask && filledMask !== undefined) {
                        filledMask.duplicate(group, ElementPlacement.PLACEAFTER);
                    }
                    const { opacity, blendingMode } = group;
                    doc.selection = [group];
                    app.executeMenuCommand(commands.livePathfinderCrop);
                    app.executeMenuCommand(commands.expandAppearance);
                    return collect(items<PageItem>(doc.selection), (item) => ({ item, opacity, blendingMode }));
                });
                visit(cropped, ({ item, opacity, blendingMode }): void => {
                    const result = item;
                    result.opacity = opacity;
                    result.blendingMode = blendingMode;
                });
                return { operation, count: groups.length };
            }),
        ),
    );
};

run(cleanupPaths);
