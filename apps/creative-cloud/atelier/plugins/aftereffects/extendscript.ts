import { illegalArgument, readProperty, recordKeys } from '@rasm/atelier-extendscript';
import { fileFromUrl, handleEnvelope, reflectionOf, specifierStep } from '@rasm/atelier-extendscript/host';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default handleEnvelope({
    indexBase: 1,
    dialogs: () => {
        app.beginSuppressDialogs();
        return () => {
            app.endSuppressDialogs(false);
        };
    },
    undoGroup: (label) => {
        app.beginUndoGroup(label);
        return () => {
            app.endUndoGroup();
        };
    },
    specifier: (value) => {
        const [project, comp, id] = [specifierStep('project'), readProperty(value, 'containingComp'), readProperty(value, 'id')];
        if (value === app.project) {
            return project;
        }
        if (value instanceof CompItem || value instanceof FolderItem || value instanceof FootageItem) {
            return `${project}${specifierStep('item', ['id', value.id])}`;
        }
        return comp instanceof CompItem && typeof id === 'number' ? `${project}${specifierStep('item', ['id', comp.id])}${specifierStep('layer', ['id', id])}` : undefined;
    },
    open: (payload) => {
        const project = app.open(fileFromUrl(payload.file));
        if (project === null) {
            throw illegalArgument(`${payload.file} opened no project`);
        }
        return project;
    },
    render: (payload, resolve) => {
        const chain = resolve(payload.target);
        const composition = chain[chain.length - 1];
        const accepted = reflectionOf(CompItem)?.name;
        if (!(composition instanceof CompItem)) {
            throw illegalArgument(`${payload.target} names a ${reflectionOf(composition)?.name}, render takes a ${accepted}`);
        }
        if (payload.options !== undefined) {
            throw illegalArgument(`${accepted} ${payload.target} takes no PNG options, received ${recordKeys(payload.options).join(', ')}`);
        }
        const coarsest = 99;
        const longest = Math.max(composition.width, composition.height);
        const factor = Math.ceil(longest / payload.longestSide);
        if (factor > coarsest) {
            throw illegalArgument(`${payload.target} spans ${longest} px and resolutionFactor stops at ${coarsest}, longestSide takes ${Math.ceil(longest / coarsest)} or more`);
        }
        const held = composition.resolutionFactor;
        composition.resolutionFactor = [factor, factor];
        try {
            // biome-ignore lint/nursery/noUnsafeTypeAssertion: types-for-adobe declares void, the host returns a DeferredCall that waits for the PNG write
            (composition.saveFrameToPng(composition.time, fileFromUrl(payload.file)) as unknown as { readonly wait: () => void }).wait();
        } finally {
            composition.resolutionFactor = held;
        }
    },
});
