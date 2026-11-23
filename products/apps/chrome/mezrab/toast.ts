// --- [OPERATIONS] ----------------------------------------------------------------------

const toast = ({ css, text, lang, dir, zoom, share }: { readonly css: string; readonly text: string; readonly lang: string; readonly dir: string; readonly zoom: number; readonly share?: { readonly url: string; readonly label: string; readonly glyph: string } }) => {
    const host = document.querySelector('mezrab-toast') ?? document.createElement('mezrab-toast');
    const root = host.shadowRoot ?? host.attachShadow({ mode: 'open' });
    const output = root.querySelector('output') ?? document.createElement('output');
    const label = root.querySelector('span') ?? document.createElement('span');
    const button = root.querySelector('button') ?? document.createElement('button');
    const data = { url: share?.url ?? location.href, ...(document.title === '' ? {} : { title: document.title }) };
    const offer = share !== undefined && 'share' in navigator && 'canShare' in navigator && navigator.canShare(data) ? share : undefined;
    const sheet = new CSSStyleSheet();
    const removed = ({ newState }: ToggleEvent) => {
        const exiting = output.getAnimations();
        const left = () => {
            if (newState === 'closed' && !output.matches(':popover-open') && exiting.every(({ playState }) => playState === 'finished' || playState === 'idle')) {
                host.remove();
            }
        };
        exiting.forEach((animation) => {
            Object.assign(animation, { onfinish: left, oncancel: left });
        });
        left();
    };
    const closed = (previous: EventTarget | null) => {
        if (root.activeElement !== null && previous instanceof HTMLElement) {
            previous.focus();
        }
        output.hidePopover();
    };
    const dismissing = (previous: EventTarget | null) =>
        Object.assign(output, {
            onanimationend: () => closed(previous),
            onkeydown: ({ key }: KeyboardEvent) => {
                if (key === 'Escape') {
                    closed(previous);
                }
            },
            onclick: () => {
                if (offer !== undefined) {
                    output.getAnimations().forEach((animation) => animation.pause());
                    navigator
                        .share(data)
                        .then(() => closed(previous))
                        .catch(() => closed(previous));
                }
            },
        });
    sheet.replaceSync(css);
    root.adoptedStyleSheets = [sheet];
    if (!host.isConnected) {
        root.append(output);
        document.documentElement.append(host);
    }
    label.textContent = text;
    if (offer !== undefined) {
        button.style.setProperty('--glyph', `url("${offer.glyph}")`);
        Object.assign(button, { ariaLabel: offer.label, onfocus: ({ relatedTarget }: FocusEvent) => dismissing(relatedTarget) });
    }
    output.replaceChildren(label, ...(offer === undefined ? [] : [button]));
    Object.assign(output, { popover: 'manual', ariaLive: 'off', lang, dir, ontoggle: removed });
    output.style.zoom = `${1 / zoom}`;
    dismissing(null);
    output
        .getAnimations()
        .filter((animation) => animation instanceof CSSAnimation)
        .forEach((animation) => {
            Object.assign(animation, { currentTime: 0 });
        });
    output.togglePopover(true);
    output.ariaNotify(text);
    return output.matches(':popover-open');
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { toast };
