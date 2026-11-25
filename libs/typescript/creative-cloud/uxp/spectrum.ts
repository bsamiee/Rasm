import { Array, Equal, MutableRef } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

declare global {
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

const element = <K extends keyof HTMLElementTagNameMap>(tag: K, fields: Partial<HTMLElementTagNameMap[K]>, ...children: readonly HTMLElement[]): HTMLElementTagNameMap[K] => {
    const node = document.createElement(tag);
    children.forEach((child) => node.append(child));
    return Object.assign(node, fields);
};
const assignSlot = (slot: string, child: HTMLElement) => {
    child.setAttribute('slot', slot);
    return child;
};
const picker = <A>(label: string, options: readonly (readonly [string, A])[]) => {
    const [selection, values] = [MutableRef.make(0), Array.map(options, ([, value]) => value)];
    const menu = element('sp-picker', { selectedIndex: MutableRef.get(selection) }, assignSlot('label', element('sp-label', { textContent: label })), assignSlot('options', element('sp-menu', {}, ...Array.map(options, ([textContent]) => element('sp-menu-item', { textContent })))));
    return {
        element: menu,
        read: () => Array.get(values, menu.selectedIndex),
        show: (value: unknown) => Object.assign(menu, { selectedIndex: MutableRef.setAndGet(selection, values.findIndex(Equal.equals(value))) }),
        onChange: (work: () => void) =>
            menu.addEventListener('change', () => {
                if (MutableRef.getAndSet(selection, menu.selectedIndex) !== menu.selectedIndex) {
                    work();
                }
            }),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { element, picker };
