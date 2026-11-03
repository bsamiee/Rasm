import { handle } from '@rasm/extendscript/handle';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default handle({
    indexBase: 1,
    dialogs: { suppress: () => app.beginSuppressDialogs(), restore: () => app.endSuppressDialogs(false) },
    undoGroup: { begin: (label) => app.beginUndoGroup(label), end: () => app.endUndoGroup() },
    specifier: (value, { step, read }) => {
        const [project, comp, id] = [step('project'), read(value, 'containingComp'), read(value, 'id')];
        if (value === app.project) {
            return project;
        }
        if (value instanceof CompItem || value instanceof FolderItem || value instanceof FootageItem) {
            return `${project}${step('item', ['id', value.id])}`;
        }
        return comp instanceof CompItem && typeof id === 'number' ? `${project}${step('item', ['id', comp.id])}${step('layer', ['id', id])}` : undefined;
    },
    open: ({ file: url }, { file, step, reflected }) => {
        const project = app.open(file(url));
        if (project === null) {
            throw new Error(`After Effects opened no project from ${url}`);
        }
        return { type: reflected(project)?.name, specifier: step('project'), name: project.file?.displayName, valid: true };
    },
    render: ({ target, longestSide, options, file: url }, { resolve, file, keys, reflected, illegal }) => {
        const chain = resolve(target);
        const composition = chain[chain.length - 1];
        const accepted = reflected(CompItem)?.name;
        if (!(composition instanceof CompItem)) {
            throw illegal(`${target} names a ${reflected(composition)?.name}, render takes a ${accepted}`);
        }
        if (options !== undefined) {
            throw illegal(`${accepted} ${target} takes no PNG options, received ${keys(options).join(', ')}`);
        }
        const coarsest = 99;
        const longest = Math.max(composition.width, composition.height);
        const factor = Math.max(1, Math.ceil(longest / longestSide));
        if (factor > coarsest) {
            throw illegal(`${target} spans ${longest} px and resolutionFactor stops at ${coarsest}, longestSide takes ${Math.ceil(longest / coarsest)} or more`);
        }
        const held = composition.resolutionFactor;
        composition.resolutionFactor = [factor, factor];
        try {
            const written: unknown = composition.saveFrameToPng(composition.time, file(url));
            if (typeof written === 'object' && written !== null && 'wait' in written && typeof written.wait === 'function') {
                written.wait();
            }
        } finally {
            composition.resolutionFactor = held;
        }
    },
});
