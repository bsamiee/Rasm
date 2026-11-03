import { file, handle, illegal, keys, read, reflected, step } from '@rasm/extendscript/handle';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default handle({
    indexBase: 1,
    dialogs: { suppress: () => app.beginSuppressDialogs(), restore: () => app.endSuppressDialogs(false) },
    undoGroup: { begin: (label) => app.beginUndoGroup(label), end: () => app.endUndoGroup() },
    specifier: (value) => {
        const [project, comp, id] = [step('project'), read(value, 'containingComp'), read(value, 'id')];
        if (value === app.project) {
            return project;
        }
        if (value instanceof CompItem || value instanceof FolderItem || value instanceof FootageItem) {
            return `${project}${step('item', ['id', value.id])}`;
        }
        return comp instanceof CompItem && typeof id === 'number' ? `${project}${step('item', ['id', comp.id])}${step('layer', ['id', id])}` : undefined;
    },
    open: ({ file: url }) => {
        const project = app.open(file(url));
        if (project === null) {
            throw new Error(`After Effects opened no project from ${url}`);
        }
        return { type: reflected(project)?.name, specifier: step('project'), name: project.file?.displayName, valid: true };
    },
    render: ({ target, longestSide, options, file: url }, resolve) => {
        const [composition] = resolve(target).slice(-1);
        const accepted = reflected(CompItem)?.name;
        if (!(composition instanceof CompItem)) {
            throw illegal(`${target} names a ${reflected(composition)?.name}, render takes a ${accepted}`);
        }
        if (options !== undefined) {
            throw illegal(`${accepted} ${target} takes no PNG options, received ${keys(options).join(', ')}`);
        }
        const coarsest = 99;
        const longest = Math.max(composition.width, composition.height);
        const factor = Math.ceil(longest / longestSide);
        if (factor > coarsest) {
            throw illegal(`${target} spans ${longest} px and resolutionFactor stops at ${coarsest}, longestSide takes ${Math.ceil(longest / coarsest)} or more`);
        }
        const held = composition.resolutionFactor;
        composition.resolutionFactor = [factor, factor];
        try {
            // biome-ignore lint/nursery/noUnsafeTypeAssertion: types-for-adobe declares void, the host returns a DeferredCall that waits for the PNG write
            (composition.saveFrameToPng(composition.time, file(url)) as unknown as { readonly wait: () => void }).wait();
        } finally {
            composition.resolutionFactor = held;
        }
    },
});
