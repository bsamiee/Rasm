import { Array, Equal, MutableRef, Record } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

declare global {
    interface Element {
        disabled: boolean;
    }
    interface HTMLElementTagNameMap {
        readonly 'sp-action-button': HTMLElement & { quiet: boolean };
        readonly 'sp-checkbox': HTMLElement & { checked: boolean };
        readonly 'sp-detail': HTMLElement;
        readonly 'sp-label': HTMLElement;
        readonly 'sp-menu': HTMLElement;
        readonly 'sp-menu-item': HTMLElement;
        readonly 'sp-picker': HTMLElement & { selectedIndex: number };
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const createElement = <K extends keyof HTMLElementTagNameMap>(tag: K, fields: Partial<HTMLElementTagNameMap[K]>, ...children: readonly HTMLElement[]): HTMLElementTagNameMap[K] => {
    const node = document.createElement(tag);
    node.append(...children);
    return Object.assign(node, fields);
};
const createPicker = <A>(label: string, options: readonly (readonly [string, A])[]) => {
    const values = Array.map(options, ([, value]) => value);
    const selection = MutableRef.make(0);
    const slots = { label: createElement('sp-label', { textContent: label }), options: createElement('sp-menu', {}, ...Array.map(options, ([textContent]) => createElement('sp-menu-item', { textContent }))) };
    Record.toEntries(slots).forEach(([slot, child]) => child.setAttribute('slot', slot));
    const picker = createElement('sp-picker', { selectedIndex: MutableRef.get(selection) }, ...Record.values(slots));
    return {
        element: picker,
        read: () => Array.get(values, picker.selectedIndex),
        show: (value: unknown) => Object.assign(picker, { selectedIndex: MutableRef.setAndGet(selection, values.findIndex(Equal.equals(value))) }),
        onChange: (work: () => void) =>
            picker.addEventListener('change', () => {
                if (MutableRef.getAndSet(selection, picker.selectedIndex) !== picker.selectedIndex) {
                    work();
                }
            }),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createElement, createPicker };
