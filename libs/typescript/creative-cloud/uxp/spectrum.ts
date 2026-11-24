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

const widget = <K extends keyof HTMLElementTagNameMap>(tag: K, fields: Partial<HTMLElementTagNameMap[K]>, ...children: readonly HTMLElement[]): HTMLElementTagNameMap[K] => {
    const element = document.createElement(tag);
    children.forEach((child) => element.append(child));
    return Object.assign(element, fields);
};
const slotted = (slot: string, element: HTMLElement) => {
    element.setAttribute('slot', slot);
    return element;
};
const picker = <A>(label: string, options: readonly (readonly [string, A])[]) => {
    const [shown, values] = [MutableRef.make(0), Array.map(options, ([, value]) => value)];
    const element = widget('sp-picker', { selectedIndex: MutableRef.get(shown) }, slotted('label', widget('sp-label', { textContent: label })), slotted('options', widget('sp-menu', {}, ...Array.map(options, ([textContent]) => widget('sp-menu-item', { textContent })))));
    return {
        element,
        read: () => Array.get(values, element.selectedIndex),
        show: (value: unknown) => Object.assign(element, { selectedIndex: MutableRef.setAndGet(shown, values.findIndex(Equal.equals(value))) }),
        picked: (work: () => void) =>
            element.addEventListener('change', () => {
                if (MutableRef.getAndSet(shown, element.selectedIndex) !== element.selectedIndex) {
                    work();
                }
            }),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { picker, widget };
