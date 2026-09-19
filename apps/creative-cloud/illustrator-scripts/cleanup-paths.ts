/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, contains, flatMap, flatten, items, nth, paths, present, run, select, split, typed }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

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
    const operations = collect(request.operations, (operation): JsonObject => {
        if (operation !== 'trimMasks') {
            const count = items<PageItem>(doc.selection).length;
            app.executeMenuCommand(commands[operation]);
            return { operation, count };
        }
        const selected = items<PageItem>(doc.selection);
        const queued = selected.length > 0 ? selected : select(items<PageItem>(doc.pageItems), (item): boolean => item.parent === item.layer);
        const groups: GroupItem[] = [];
        for (let index = 0; index < queued.length; index += 1) {
            const item = nth(queued, index);
            if (typed<GroupItem>('GroupItem')(item) && !contains(groups, item)) {
                groups.push(item);
                queued.push(...items(item.pageItems));
            }
        }
        const clipped = select(groups.reverse(), (group): boolean => group.clipped === true);
        const output: string[] = [];
        const cropped = collect(clipped, (group): JsonObject => {
            const descendants = flatten(items(group.pageItems));
            const prepared = flatMap(descendants, paths);
            for (let index = 0; index < prepared.length; index += 1) {
                const path = nth(prepared, index);
                path.evenodd = false;
            }
            const frames = select(descendants, typed<TextFrame>('TextFrame'));
            for (let index = 0; index < frames.length; index += 1) {
                const frame = nth(frames, index);
                const outlined = flatMap(flatten(items(frame.createOutline().pageItems)), paths);
                for (let glyphIndex = 0; glyphIndex < outlined.length; glyphIndex += 1) {
                    const glyph = nth(outlined, glyphIndex);
                    glyph.evenodd = false;
                }
            }
            const compounds = select(flatten(items(group.pageItems)), typed<CompoundPathItem>('CompoundPathItem'));
            for (let index = 0; index < compounds.length; index += 1) {
                doc.selection = [nth(compounds, index)];
                app.executeMenuCommand(commands.noCompoundPath);
                app.executeMenuCommand(commands.ungroup);
                app.executeMenuCommand(commands.compoundPath);
            }
            const [filledMask] = select(items(group.pathItems), (path): boolean => path.clipping && path.filled);
            const mask = request.keepFilledMask && filledMask !== undefined ? [filledMask.duplicate(group, ElementPlacement.PLACEAFTER)] : [];
            const { opacity, blendingMode } = group;
            try {
                doc.selection = [group];
                app.executeMenuCommand(commands.livePathfinderCrop);
                app.executeMenuCommand(commands.expandAppearance);
            } catch (error) {
                for (let index = 0; index < mask.length; index += 1) {
                    nth(mask, index).remove();
                }
                throw error;
            }
            const results = items<PageItem>(doc.selection);
            for (let index = 0; index < results.length; index += 1) {
                const item = nth(results, index);
                item.opacity = opacity;
                item.blendingMode = blendingMode;
                output.push(item.uuid);
            }
            return { items: results.length };
        });
        doc.selection = select(items(doc.pageItems), (item): boolean => contains(output, item.uuid));
        return { operation, count: cropped.length };
    });
    return present(split(operations));
};

run(cleanupPaths);
