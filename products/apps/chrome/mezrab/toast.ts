// --- [OPERATIONS] ----------------------------------------------------------------------

const toast = ({
    css,
    text,
    lang,
    dir,
    zoom,
    shown,
    share,
}: {
    readonly css: string;
    readonly text: string;
    readonly lang: string;
    readonly dir: string;
    readonly zoom: number;
    readonly shown: number;
    readonly share?: { readonly url: string; readonly label: string; readonly glyph: string; readonly shown: number };
}) => {
    const host = document.querySelector('mezrab-toast') ?? document.createElement('mezrab-toast');
    const root = host.shadowRoot ?? host.attachShadow({ mode: 'open' });
    const output = root.querySelector('output') ?? document.createElement('output');
    const label = root.querySelector('span') ?? document.createElement('span');
    const button = root.querySelector('button') ?? document.createElement('button');
    const offered = [share].flatMap((action) => (action === undefined ? [] : [{ ...action, data: { url: action.url, ...(document.title === '' ? {} : { title: document.title }) } }])).filter(({ data }) => 'share' in navigator && 'canShare' in navigator && navigator.canShare(data));
    const sheet = new CSSStyleSheet();
    const removed = ({ newState }: ToggleEvent) => {
        const exiting = output.getAnimations();
        const left = () => [host].filter(() => newState === 'closed' && !output.matches(':popover-open') && exiting.every(({ playState }) => playState === 'finished' || playState === 'idle')).forEach((held) => held.remove());
        exiting.forEach((animation) => {
            Object.assign(animation, { onfinish: left, oncancel: left });
        });
        left();
    };
    const closed = (previous: EventTarget | null) => {
        [previous]
            .filter(() => root.activeElement !== null)
            .filter((target) => target instanceof HTMLElement)
            .forEach((target) => target.focus());
        output.hidePopover();
    };
    const dismissing = (previous: EventTarget | null) =>
        Object.assign(output, {
            onanimationend: () => closed(previous),
            onkeydown: ({ key }: KeyboardEvent) => [key].filter((pressed) => pressed === 'Escape').forEach(() => closed(previous)),
            onclick: () =>
                offered.forEach(({ data }) => {
                    output.getAnimations().forEach((animation) => animation.pause());
                    navigator
                        .share(data)
                        .then(() => closed(previous))
                        .catch(() => closed(previous));
                }),
        });
    sheet.replaceSync(css);
    root.adoptedStyleSheets = [sheet];
    root.append(...[output].filter(({ isConnected }) => !isConnected));
    document.documentElement.append(...[host].filter(({ isConnected }) => !isConnected));
    label.textContent = text;
    offered.forEach(({ label: name, glyph }) => {
        button.style.setProperty('--glyph', `url("${glyph}")`);
        Object.assign(button, { ariaLabel: name, onfocus: ({ relatedTarget }: FocusEvent) => dismissing(relatedTarget) });
    });
    output.replaceChildren(label, ...offered.map(() => button));
    Object.assign(output, { popover: 'manual', ariaLive: 'off', lang, dir, ontoggle: removed });
    dismissing(null);
    Object.assign(output.style, { zoom: `${1 / zoom}`, animationDuration: `${offered.at(0)?.shown ?? shown}ms` });
    output
        .getAnimations()
        .filter((animation) => animation instanceof CSSAnimation)
        .forEach((animation) => {
            Object.assign(animation, { currentTime: 0 });
        });
    output.togglePopover(true);
    document.ariaNotify(text);
    return output.matches(':popover-open');
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { toast };
