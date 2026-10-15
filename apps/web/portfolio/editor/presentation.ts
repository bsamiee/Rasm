// --- [CONSTANTS] -----------------------------------------------------------------------

const popover = 'max-h-[50vh] max-w-[min(480px,calc(100vw-32px))] overflow-auto border border-control-line bg-background';
const option = 'min-h-11 cursor-pointer wrap-anywhere px-4 py-2.5 focus:bg-foreground focus:text-background';
const reorderable = {
    dragLabel: 'Drag',
    moveLabel: 'Move',
    moveMenuLabel: 'Move to position',
    classNames: {
        list: '[counter-reset:item]',
        item: 'border-line border-t bg-background dragging:opacity-50 [counter-increment:item]',
        controls: 'flex items-center gap-1 py-1 text-muted',
        selection: 'group grid min-h-11 min-w-11 place-items-center',
        checkbox: 'grid size-5 place-items-center border border-control-line group-selected:border-accent-text group-selected:bg-accent-text group-selected:text-background',
        check: 'size-3.5 opacity-0 group-selected:opacity-100',
        drag: 'grid min-h-11 min-w-11 cursor-grab touch-none place-items-center',
        grip: 'size-5',
        position: "before:content-['['_counter(item,decimal-leading-zero)_']'] mr-auto whitespace-nowrap font-mono text-muted text-xs",
        move: 'button button-ghost px-2 text-xs',
        popover,
        option: `${option} disabled:opacity-40`,
        dropIndicator: 'h-0.5 bg-transparent drop-target:bg-accent-text',
    },
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { option, popover, reorderable };
