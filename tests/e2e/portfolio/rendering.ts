import type { Published } from '#/served';

// --- [TYPES] ---------------------------------------------------------------------------

interface Case {
    readonly viewer: keyof typeof drawn;
    readonly path: string;
    readonly shows: string;
    readonly width: number;
    readonly read: () => unknown;
    readonly expected: unknown;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PIXEL = Uint8Array.fromBase64('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==');
const SHEET = Uint8Array.fromBase64(
    'JVBERi0xLjQKMSAwIG9iago8PCAvVHlwZSAvQ2F0YWxvZyAvUGFnZXMgMiAwIFIgPj4KZW5kb2JqCjIgMCBvYmoKPDwgL1R5cGUgL1BhZ2VzIC9LaWRzIFszIDAgUl0gL0NvdW50IDEgPj4KZW5kb2JqCjMgMCBvYmoKPDwgL1R5cGUgL1BhZ2UgL1BhcmVudCAyIDAgUiAvTWVkaWFCb3ggWzAgMCA4NDIgNTk1XSAvUmVzb3VyY2VzIDw8IC9Gb250IDw8IC9GMSA0IDAgUiA+PiA+PiAvQ29udGVudHMgNSAwIFIgPj4KZW5kb2JqCjQgMCBvYmoKPDwgL1R5cGUgL0ZvbnQgL1N1YnR5cGUgL1R5cGUxIC9CYXNlRm9udCAvSGVsdmV0aWNhID4+CmVuZG9iago1IDAgb2JqCjw8IC9MZW5ndGggNDYgPj4Kc3RyZWFtCkJUIC9GMSA3MiBUZiA5NiAyNjAgVGQgKEhhcmJvdXIgc2VjdGlvbikgVGogRVQKZW5kc3RyZWFtCmVuZG9iagp4cmVmCjAgNgowMDAwMDAwMDAwIDY1NTM1IGYgCjAwMDAwMDAwMDkgMDAwMDAgbiAKMDAwMDAwMDA1OCAwMDAwMCBuIAowMDAwMDAwMTE1IDAwMDAwIG4gCjAwMDAwMDAyNDEgMDAwMDAgbiAKMDAwMDAwMDMxMSAwMDAwMCBuIAp0cmFpbGVyCjw8IC9TaXplIDYgL1Jvb3QgMSAwIFIgPj4Kc3RhcnR4cmVmCjQwNwolJUVPRgo=',
);
const DESKTOP = 1280;
const PHONE = 390;
const section = { id: '6f1c2a4e-3b5d-4c6e-8f90-1a2b3c4d5e6f', kind: 'image', name: 'harbour-section.png', mime: 'image/png', size: PIXEL.length, width: 1600, height: 1000 } as const;
const room = { id: '5e0d6c1b-2a3f-4b8e-9c7d-6f5a4b3c2d1e', kind: 'image', name: 'reading-room.png', mime: 'image/png', size: PIXEL.length, width: 1200, height: 1800 } as const;
const panorama = { id: '8a4f2c6e-1b3d-4e5f-a7c9-0d2e4f6a8b1c', kind: 'image', name: 'harbour-panorama.png', mime: 'image/png', size: PIXEL.length, width: 4000, height: 1000 } as const;
const drawings = { id: '1d3f5b7a-9c2e-4a6b-8d0f-2b4c6e8a0c1e', kind: 'pdf', name: 'drawing-set.pdf', mime: 'application/pdf', size: SHEET.length, pages: [{ width: 842, height: 595 }] } as const;
const content = {
    files: [
        { asset: section, bytes: PIXEL },
        { asset: room, bytes: PIXEL },
        { asset: panorama, bytes: PIXEL },
        { asset: drawings, bytes: SHEET },
    ],
    portfolio: {
        name: 'Studio Rasm',
        introduction: 'Housing, civic buildings, and studies of timber construction.',
        hero: { kind: 'image', assetId: room.id, role: 'photograph', alt: 'Reading room', framing: { mode: 'cover', x: 20, y: 80 } },
        entries: [
            {
                id: '7f1c2a4e-3b5d-4c6e-8f90-1a2b3c4d5e6f',
                kind: 'project',
                title: 'Harbour Library',
                compositions: [
                    ...(['left', 'center', 'right'] as const).flatMap((align, column) => (['full', 'medium', 'small'] as const).map((scale, row) => ({ id: `4c0d1e2f-5a6b-4c7d-8e9f-0000000000${column}${row}`, items: [{ kind: 'image', assetId: panorama.id, role: 'drawing' }], scale, align }))),
                    {
                        id: '4c0d1e2f-5a6b-4c7d-8e9f-000000000030',
                        items: [
                            { kind: 'image', assetId: section.id, role: 'drawing' },
                            { kind: 'image', assetId: room.id, role: 'photograph', framing: { mode: 'contain' } },
                        ],
                    },
                ],
            },
            { id: '0a9b8c7d-6e5f-4a3b-9c2d-1e0f9a8b7c6d', kind: 'study', title: 'Timber Joints' },
            { id: '3c1f0e52-8a47-4d19-b6c3-2e5d7a9f1b04', kind: 'project', title: 'Quay Pavilion', compositions: [{ id: '4c0d1e2f-5a6b-4c7d-8e9f-000000000040', items: [{ kind: 'pdf', assetId: drawings.id, page: 1 }] }] },
            { id: '9d2e4b61-5f38-4c07-a1d4-6b8c0e2f3a15', kind: 'project', title: 'Tide Archive', cover: { kind: 'image', assetId: section.id, role: 'drawing' } },
        ],
    },
} as const satisfies Published;

// --- [OPERATIONS] ----------------------------------------------------------------------

const gutters = () => {
    const header = document.querySelector('header')?.getBoundingClientRect();
    return header === undefined ? null : { inset: Math.round(header.left) > 0, difference: Math.round(header.left) - Math.round(document.documentElement.clientWidth - header.right) };
};

const indexes = () => {
    const title = document.querySelector('h1')?.getBoundingClientRect();
    const shown = [...document.querySelectorAll('nav[aria-label="Work index"]')].filter((index) => index.checkVisibility());
    const apart = ({ left, right, top, bottom }: DOMRect) => title === undefined || right <= title.left || left >= title.right || bottom <= title.top || top >= title.bottom;
    return { forms: shown.map((index) => (index.querySelector('select') === null ? 'links' : 'select')), clear: shown.every((index) => apart(index.getBoundingClientRect())) };
};

const cover = () => {
    const chapter = [...document.querySelectorAll('h3')].find(({ textContent }) => textContent === 'Tide Archive');
    const figure = chapter?.closest('section')?.querySelector('figure');
    const box = figure?.getBoundingClientRect();
    return box === undefined ? null : { ratio: Math.round((box.width / box.height) * 10) / 10, image: figure?.querySelector('img') !== null };
};

const focus = () => {
    const photograph = document.querySelector('img[alt="Reading room"]');
    return photograph === null ? null : getComputedStyle(photograph).objectPosition;
};

const stated = () => ({
    titles: Array.from(document.querySelectorAll('title'), ({ textContent }) => textContent),
    descriptions: Array.from(document.querySelectorAll('meta[name="description"]'), (described) => described.getAttribute('content')),
    work: Array.from(document.querySelectorAll('h3'), ({ textContent }) => textContent),
});

const picked = () => {
    [...document.querySelectorAll<HTMLAnchorElement>('nav[aria-label="Work index"] a')].find(({ textContent }) => textContent === 'Timber Joints')?.click();
    return {
        current: Array.from(document.querySelectorAll('[aria-current="location"]'), ({ textContent }) => textContent),
        focused: document.activeElement instanceof HTMLHeadingElement ? document.activeElement.textContent : null,
        titles: Array.from(document.querySelectorAll('title'), ({ textContent }) => textContent),
    };
};

const aligned = () =>
    Object.fromEntries(
        [...document.querySelectorAll<HTMLElement>('[aria-label="Harbour Library compositions"] .composition')].slice(0, 9).map((composition) => {
            const frame = composition.getBoundingClientRect();
            const items = composition.firstElementChild?.getBoundingClientRect() ?? frame;
            const free = frame.width - items.width;
            return [`${composition.dataset['scale']} ${composition.dataset['align']}`, Math.round(free) === 0 ? 'fills' : Math.abs(Math.round(((items.left - frame.left) / free) * 100))];
        }),
    );

const scaled = () => {
    const measured = [...document.querySelectorAll<HTMLElement>('[aria-label="Harbour Library compositions"] .composition')].slice(0, 9).map((composition) => ({ scale: composition.dataset['scale'], align: composition.dataset['align'], width: composition.firstElementChild?.getBoundingClientRect().width ?? 0 }));
    return Object.fromEntries(
        ['left', 'center', 'right'].map((align) => [
            align,
            measured
                .filter((composition) => composition.align === align)
                .toSorted((narrow, wide) => narrow.width - wide.width)
                .map(({ scale }) => scale),
        ]),
    );
};

const paired = () => {
    const composition = document.querySelector('[aria-label="Harbour Library compositions"] .composition:has(figure + figure)');
    const [first, second] = Array.from(composition?.querySelectorAll('.media-frame') ?? [], (frame) => frame.getBoundingClientRect());
    return composition === null || first === undefined || second === undefined ? null : { wide: composition.getBoundingClientRect().width >= 600, beside: second.left >= first.right, beneath: second.top >= first.bottom };
};

const levelled = () => {
    const [first, second] = Array.from(document.querySelectorAll('[aria-label="Harbour Library compositions"] .composition:has(figure + figure) .media-frame'), (frame) => frame.getBoundingClientRect());
    return first === undefined || second === undefined ? null : { taller: Math.abs(Math.round(first.height - second.height)), shapes: [first, second].map(({ width, height }) => Math.round((width / height) * 10) / 10) };
};

const hugged = () => {
    const composition = document.querySelector('[aria-label="Harbour Library compositions"] .composition');
    const items = composition?.firstElementChild?.getBoundingClientRect();
    const padded = composition === null ? undefined : getComputedStyle(composition);
    return composition === null || items === undefined || padded === undefined ? null : Math.sign(Math.round(composition.getBoundingClientRect().height) - Math.round(items.height + Number.parseFloat(padded.paddingTop) + Number.parseFloat(padded.paddingBottom)));
};

const ended = () => {
    const [header, held] = [document.querySelector('header'), document.querySelector('.site-content')].map((element) => element?.getBoundingClientRect());
    return header === undefined || held === undefined ? null : Math.sign(Math.round(header.right) - Math.round(held.right));
};

const shown = () => {
    const carousel = document.querySelector('[aria-label="Harbour Library compositions"]');
    const viewport = carousel?.querySelector('fieldset')?.parentElement?.parentElement;
    viewport?.scrollIntoView({ block: 'center', behavior: 'instant' });
    const box = viewport?.getBoundingClientRect();
    return box === undefined
        ? null
        : {
              within: [...(carousel?.querySelectorAll('fieldset') ?? [])].filter((slide) => slide.getBoundingClientRect().right > box.left + 1 && slide.getBoundingClientRect().left < box.right - 1).map((slide) => slide.getAttribute('aria-label')),
              centre:
                  document
                      .elementFromPoint(box.left + box.width / 2, box.top + box.height / 2)
                      ?.closest('fieldset')
                      ?.getAttribute('aria-label') ?? null,
              spill: document.documentElement.scrollWidth - document.documentElement.clientWidth,
          };
};

const advanced = () => {
    const carousel = document.querySelector('[aria-label="Harbour Library compositions"]');
    const viewport = carousel?.querySelector('fieldset')?.parentElement?.parentElement;
    const selected = () => carousel?.querySelector('fieldset:not([inert])')?.getAttribute('aria-label') ?? null;
    viewport?.scrollIntoView({ block: 'center', behavior: 'instant' });
    (selected() === '1 of 10' ? carousel?.querySelector<HTMLElement>('[aria-label="Next composition"]') : null)?.click();
    const box = viewport?.getBoundingClientRect();
    return box === undefined
        ? null
        : {
              selected: selected(),
              within: [...(carousel?.querySelectorAll('fieldset') ?? [])].filter((slide) => slide.getBoundingClientRect().right > box.left + 1 && slide.getBoundingClientRect().left < box.right - 1).map((slide) => slide.getAttribute('aria-label')),
              centre:
                  document
                      .elementFromPoint(box.left + box.width / 2, box.top + box.height / 2)
                      ?.closest('fieldset')
                      ?.getAttribute('aria-label') ?? null,
              spill: document.documentElement.scrollWidth - document.documentElement.clientWidth,
          };
};

const controls = () => {
    const carousel = document.querySelector('[aria-label="Harbour Library compositions"]');
    const frame = carousel?.getBoundingClientRect();
    const [overview, previous, next] = [[...(carousel?.querySelectorAll('button') ?? [])].find(({ textContent }) => textContent === 'Overview'), carousel?.querySelector('[aria-label="Previous composition"]'), carousel?.querySelector('[aria-label="Next composition"]')].map((control) => control?.getBoundingClientRect());
    return frame === undefined || overview === undefined || previous === undefined || next === undefined
        ? null
        : {
              start: Math.abs(Math.round(overview.left - frame.left)),
              end: Math.abs(Math.round(frame.right - next.right)),
              ordered: previous.right <= next.left,
              level: [overview, previous].map((box) => Math.abs(Math.round(box.top + box.height / 2 - next.top - next.height / 2))),
              targets: [previous, next].flatMap(({ width, height }) => [width, height]),
          };
};

const fitted = () => {
    const carousel = document.querySelector('[aria-label="Harbour Library compositions"]');
    (carousel?.querySelector('fieldset')?.inert === false ? carousel.querySelector<HTMLElement>('[aria-label="Next composition"]') : null)?.click();
    const viewport = carousel?.querySelector('fieldset')?.parentElement?.parentElement?.getBoundingClientRect();
    const slides = Array.from(carousel?.querySelectorAll('fieldset') ?? [], (slide) => ({ label: slide.getAttribute('aria-label'), selected: !slide.inert, height: slide.firstElementChild?.getBoundingClientRect().height ?? 0 }));
    const selected = slides.find((slide) => slide.selected);
    return viewport === undefined || selected === undefined ? null : { selected: selected.label, extra: Math.abs(Math.round(viewport.height - selected.height)), taller: slides.some(({ height }) => height > selected.height + 1) };
};

const expanded = () => {
    const viewer = document.querySelector('.viewer-dialog');
    (viewer === null ? document.querySelector<HTMLElement>('[aria-label="Expand reading-room.png"]') : null)?.click();
    const frame = viewer?.getBoundingClientRect();
    const stage = viewer?.querySelector('.viewer-stage')?.getBoundingClientRect();
    const margins = frame === undefined ? [] : [frame.top, frame.left, document.documentElement.clientWidth - frame.right, document.documentElement.clientHeight - frame.bottom].map(Math.round);
    return viewer === null || frame === undefined || stage === undefined
        ? null
        : {
              even: new Set(margins).size === 1,
              bleeds: margins.every((margin) => margin === 0),
              stage: Math.abs(Math.round(frame.bottom - Number.parseFloat(getComputedStyle(viewer).paddingBottom) - stage.bottom)),
              front: document.elementFromPoint(stage.left + stage.width / 2, stage.top + stage.height / 2)?.closest('.viewer-dialog') === viewer,
              focused: document.activeElement?.closest('[role="dialog"]')?.contains(viewer) === true,
          };
};

const listed = () => {
    const modal = document.querySelector('.overview');
    (modal === null ? [...document.querySelectorAll<HTMLElement>('[aria-label="Harbour Library composition controls"] button')].find(({ textContent }) => textContent === 'Overview') : undefined)?.click();
    const list = modal?.querySelector('[role="grid"]');
    list?.scrollTo({ top: list.scrollHeight, behavior: 'instant' });
    const last = [...(list?.querySelectorAll('[role="row"]') ?? [])].at(-1);
    const [frame, close] = [modal, modal?.querySelector('[aria-label="Close overview"]')].map((element) => element?.getBoundingClientRect());
    return list === null || list === undefined || frame === undefined || close === undefined
        ? null
        : {
              centred: [frame.left - document.documentElement.clientWidth + frame.right, frame.top - document.documentElement.clientHeight + frame.bottom].map((offset) => Math.abs(Math.round(offset))),
              scrolled: list.scrollTop > 0,
              last: last?.querySelector('.eyebrow')?.textContent ?? null,
              close:
                  document
                      .elementFromPoint(close.left + close.width / 2, close.top + close.height / 2)
                      ?.closest('button')
                      ?.getAttribute('aria-label') ?? null,
              focused: list.contains(document.activeElement),
          };
};

const sheeted = () => {
    const viewer = document.querySelector('.viewer-dialog');
    (viewer === null ? document.querySelector<HTMLElement>('[aria-label="Expand drawing-set.pdf · Sheet 1 of 1"]') : null)?.click();
    const scroller = viewer?.querySelector('.sheet-scroller');
    const [stage, frame, text] = [viewer?.querySelector('.viewer-stage'), scroller, scroller?.querySelector('.textLayer span')].map((element) => element?.getBoundingClientRect());
    return scroller === null || scroller === undefined || stage === undefined || frame === undefined || text === undefined
        ? null
        : {
              inset: [frame.top - stage.top, frame.left - stage.left, stage.right - frame.right, stage.bottom - frame.bottom].map((gap) => Math.abs(Math.round(gap))),
              overflow: [scroller.scrollWidth - scroller.clientWidth, scroller.scrollHeight - scroller.clientHeight],
              under: document.elementFromPoint(text.left + text.width / 2, text.top + text.height / 2)?.textContent ?? null,
          };
};

const zoomed = () => {
    const viewer = document.querySelector('.viewer-dialog');
    (viewer === null ? document.querySelector<HTMLElement>('[aria-label="Expand drawing-set.pdf · Sheet 1 of 1"]') : null)?.click();
    const scroller = viewer?.querySelector('.sheet-scroller');
    const spare = scroller === null || scroller === undefined || scroller.querySelector('.textLayer span') === null ? undefined : [scroller.scrollWidth - scroller.clientWidth - 40, scroller.scrollHeight - scroller.clientHeight - 30];
    (spare?.some((gap) => gap < 0) === true ? viewer?.querySelector<HTMLElement>('[aria-label="Zoom in"]') : null)?.click();
    scroller?.scrollTo({ left: 40, top: 30, behavior: 'instant' });
    return scroller === null || scroller === undefined ? null : [scroller.scrollLeft, scroller.scrollTop].map(Math.round);
};

const panned = () => {
    const viewer = document.querySelector('.viewer-dialog');
    (viewer === null ? document.querySelector<HTMLElement>('[aria-label="Expand drawing-set.pdf · Sheet 1 of 1"]') : null)?.click();
    const scroller = viewer?.querySelector<HTMLElement>('.sheet-scroller');
    const tools = [...(viewer?.querySelectorAll('details') ?? [])].find((group) => group.querySelector('summary')?.textContent === 'Tools');
    tools?.toggleAttribute('open', true);
    (scroller?.dataset['pan'] === undefined ? [...(tools?.querySelectorAll('button') ?? [])].find(({ textContent }) => textContent === 'Pan') : undefined)?.click();
    const text = scroller?.querySelector('.textLayer span')?.getBoundingClientRect();
    return scroller === null || scroller === undefined || text === undefined ? null : { pans: scroller.dataset['pan'] !== undefined, touch: getComputedStyle(scroller).touchAction, text: document.elementFromPoint(text.left + text.width / 2, text.top + text.height / 2)?.closest('.textLayer') !== null };
};

const docked = () => {
    const editor = document.querySelector('.editor')?.getBoundingClientRect();
    return editor === undefined ? null : { edges: [document.documentElement.clientWidth - editor.right, editor.top, document.documentElement.clientHeight - editor.bottom].map((gap) => Math.abs(Math.round(gap))), beside: Math.round(editor.left) > 0 };
};

const panelled = () => {
    const editor = document.querySelector('.editor');
    const [create, named] = [[...(editor?.querySelectorAll('button') ?? [])].find(({ textContent }) => textContent === 'New project'), editor?.querySelector('input[name="name"]')];
    return create === undefined || named === null || named === undefined ? null : { entries: create.checkVisibility(), profile: named.checkVisibility(), rows: new Set(Array.from(editor?.querySelectorAll('[role="tab"]') ?? [], (tab) => Math.round(tab.getBoundingClientRect().top))).size };
};

const fielded = () => {
    const editor = document.querySelector('.editor');
    const tab = [...(editor?.querySelectorAll<HTMLElement>('[role="tab"]') ?? [])].find(({ textContent }) => textContent === 'Profile and hero');
    (tab?.getAttribute('aria-selected') === 'false' ? tab : undefined)?.click();
    const name = editor?.querySelector('input[name="name"]');
    const [field, label, input, area] = [name?.parentElement, name?.parentElement?.querySelector('label'), name, editor?.querySelector('textarea[name="introduction"]')].map((element) => element?.getBoundingClientRect());
    return field === undefined || label === undefined || input === undefined || area === undefined || !(name?.checkVisibility() ?? false) ? null : { above: label.bottom <= input.top, spans: Math.abs(Math.round(field.width - input.width)), target: input.height >= 44, taller: area.height > input.height };
};

const detailed = () => {
    const editor = document.querySelector('.editor');
    const details = editor?.querySelector('.entry-details');
    (details === null ? [...(editor?.querySelectorAll<HTMLElement>('[role="row"] button') ?? [])].find(({ firstChild }) => firstChild?.textContent === 'Harbour Library') : undefined)?.click();
    details?.toggleAttribute('open', true);
    const [first, second] = Array.from(details?.querySelector('.field-pair')?.children ?? [], (field) => field.getBoundingClientRect());
    return first === undefined || second === undefined || first.width === 0 ? null : { columns: second.left >= first.right ? 2 : 1, wider: Math.abs(Math.round(first.width - second.width)) };
};

const chosen = () => {
    const editor = document.querySelector('.editor');
    (editor?.querySelector('.entry-editor') === null ? [...editor.querySelectorAll<HTMLElement>('[role="row"] button')].find(({ firstChild }) => firstChild?.textContent === 'Harbour Library') : undefined)?.click();
    const group = [...(editor?.querySelectorAll('[role="radiogroup"]') ?? [])].find(({ firstElementChild }) => firstElementChild?.textContent === 'Scale');
    const label = group?.firstElementChild?.getBoundingClientRect();
    const options = Array.from(group?.querySelectorAll('label') ?? [], (option) => option.getBoundingClientRect());
    return label === undefined ? null : { above: options.every(({ top }) => label.bottom <= top), rows: new Set(options.map(({ top }) => Math.round(top))).size, targets: options.every(({ width, height }) => width >= 44 && height >= 44) };
};

const slid = () => {
    const editor = document.querySelector('.editor');
    const tab = [...(editor?.querySelectorAll<HTMLElement>('[role="tab"]') ?? [])].find(({ textContent }) => textContent === 'Profile and hero');
    (tab?.getAttribute('aria-selected') === 'false' ? tab : undefined)?.click();
    return Array.from(editor?.querySelectorAll('[role="group"]:has(input[type="range"])') ?? [], (slider) => {
        const [track, thumb, output] = [slider.lastElementChild, slider.lastElementChild?.firstElementChild, slider.querySelector('output')].map((element) => element?.getBoundingClientRect());
        return track === undefined || thumb === undefined || output === undefined
            ? null
            : {
                  value: slider.querySelector('output')?.textContent,
                  along: Math.round(((thumb.left + thumb.width / 2 - track.left) / track.width) * 100),
                  level: Math.abs(Math.round(thumb.top + thumb.height / 2 - track.top - track.height / 2)),
                  end: Math.abs(Math.round(track.right - output.right)),
                  track: Math.round(track.height),
              };
    });
};

const rowed = () => {
    const row = document.querySelector('.editor [role="row"][aria-label="Harbour Library"]');
    const [frame, selection, drag, move, entry] = [row, row?.querySelector('[slot="selection"] label'), row?.querySelector('[slot="drag"]'), row?.querySelector('[aria-label="Move Harbour Library"]'), row?.querySelector('.entry-row')].map((element) => element?.getBoundingClientRect());
    return frame === undefined || selection === undefined || drag === undefined || move === undefined || entry === undefined
        ? null
        : {
              ordered: selection.right <= drag.left && drag.right <= move.left,
              level: [selection, drag].map((box) => Math.abs(Math.round(box.top + box.height / 2 - move.top - move.height / 2))),
              end: Math.abs(Math.round(frame.right - move.right)),
              beneath: entry.top >= Math.max(selection.bottom, drag.bottom, move.bottom),
              spans: Math.abs(Math.round(frame.width - entry.width)),
              targets: [selection, drag].every(({ width, height }) => width >= 44 && height >= 44) && move.height >= 44,
          };
};

const moved = () => {
    const menu = document.querySelector('[role="menu"]');
    (menu === null ? document.querySelector<HTMLElement>('.editor [aria-label="Move Harbour Library"]') : null)?.click();
    const items = [...(menu?.querySelectorAll('[role="menuitem"]') ?? [])];
    const second = items.at(1)?.getBoundingClientRect();
    return menu === null || second === undefined
        ? null
        : {
              items: items.map(({ textContent }) => textContent),
              heights: items.map((item) => Math.round(item.getBoundingClientRect().height)),
              front: document.elementFromPoint(second.left + second.width / 2, second.top + second.height / 2)?.textContent ?? null,
              focused: menu.contains(document.activeElement),
          };
};

const confirmed = () => {
    const dialog = document.querySelector('[role="alertdialog"]');
    (dialog === null ? [...document.querySelectorAll<HTMLElement>('.editor [role="row"][aria-label="Harbour Library"] button')].find(({ textContent }) => textContent === 'Remove entry') : undefined)?.click();
    const [modal, keep, remove, save] = [dialog?.parentElement, ...[...(dialog?.querySelectorAll('button') ?? [])].slice(0, 2), [...document.querySelectorAll('.editor button')].find(({ textContent }) => textContent === 'Save draft')].map((element) => element?.getBoundingClientRect());
    return dialog === null || modal === undefined || keep === undefined || remove === undefined || save === undefined
        ? null
        : {
              centred: [modal.left - document.documentElement.clientWidth + modal.right, modal.top - document.documentElement.clientHeight + modal.bottom].map((offset) => Math.abs(Math.round(offset))),
              focused: document.activeElement?.textContent ?? null,
              level: Math.abs(Math.round(keep.top - remove.top)),
              end: Math.abs(Math.round(modal.right - Number.parseFloat(getComputedStyle(dialog).paddingRight) - remove.right)),
              front: document.elementFromPoint(keep.left + keep.width / 2, keep.top + keep.height / 2)?.textContent ?? null,
              covered: document.elementFromPoint(save.left + save.width / 2, save.top + save.height / 2)?.closest('.editor') === null,
          };
};

const spanned = () => {
    const editor = document.querySelector('.editor');
    const tab = [...(editor?.querySelectorAll<HTMLElement>('[role="tab"]') ?? [])].find(({ textContent }) => textContent === 'Files');
    (tab?.getAttribute('aria-selected') === 'false' ? tab : undefined)?.click();
    const button = editor?.querySelector('button[aria-haspopup="listbox"]');
    const [field, trigger, value, icon] = [button?.parentElement, button, button?.firstElementChild, button?.lastElementChild].map((element) => element?.getBoundingClientRect());
    return field === undefined || trigger === undefined || value === undefined || icon === undefined ? null : { spans: Math.abs(Math.round(field.width - trigger.width)), balance: Math.abs(Math.round(trigger.right - icon.right - value.left + trigger.left)) };
};

const destined = () => {
    const editor = document.querySelector('.editor');
    const tab = [...(editor?.querySelectorAll<HTMLElement>('[role="tab"]') ?? [])].find(({ textContent }) => textContent === 'Files');
    (tab?.getAttribute('aria-selected') === 'false' ? tab : undefined)?.click();
    const button = editor?.querySelector<HTMLElement>('button[aria-haspopup="listbox"]');
    const list = document.querySelector('[role="listbox"]');
    (list === null ? button : null)?.click();
    const options = [...(list?.querySelectorAll('[role="option"]') ?? [])];
    const first = options.at(0)?.getBoundingClientRect();
    return first === undefined
        ? null
        : {
              options: options.map(({ textContent }) => textContent),
              heights: options.map((option) => Math.round(option.getBoundingClientRect().height)),
              front: document.elementFromPoint(first.left + first.width / 2, first.top + first.height / 2)?.textContent ?? null,
          };
};

const filed = () => {
    const editor = document.querySelector('.editor');
    const tab = [...(editor?.querySelectorAll<HTMLElement>('[role="tab"]') ?? [])].find(({ textContent }) => textContent === 'Files');
    (tab?.getAttribute('aria-selected') === 'false' ? tab : undefined)?.click();
    const [first, second] = Array.from(editor?.querySelectorAll('[aria-label="Source files"] [role="row"]') ?? [], (file) => file.getBoundingClientRect());
    return first === undefined || second === undefined ? null : { beside: second.left >= first.right, wider: Math.abs(Math.round(first.width - second.width)), lower: Math.abs(Math.round(second.top - first.top)) };
};

const sequenced = () => {
    const editor = document.querySelector('.editor');
    (editor?.querySelector('.entry-editor') === null ? [...editor.querySelectorAll<HTMLElement>('[role="row"] button')].find(({ firstChild }) => firstChild?.textContent === 'Harbour Library') : undefined)?.click();
    const [list, composing, toggle] = [editor?.querySelector('.sequence-layout > div'), editor?.querySelector('.composition-editor'), editor?.querySelector('[aria-label="Choose composition"]')];
    return list === null || list === undefined || composing === null || composing === undefined || toggle === null || toggle === undefined
        ? null
        : { list: list.checkVisibility(), toggle: toggle.checkVisibility(), beside: list.checkVisibility() && composing.getBoundingClientRect().left >= list.getBoundingClientRect().right };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const drawn = {
    visitor: {
        path: '/',
        rows: [
            { shows: 'insets the page header by one gutter on both sides', width: DESKTOP, read: gutters, expected: { inset: true, difference: 0 } },
            { shows: 'shows the side index as the one Work index at desktop width, clear of the page title', width: DESKTOP, read: indexes, expected: { forms: ['links'], clear: true } },
            { shows: 'shows the select as the one Work index at phone width', width: PHONE, read: indexes, expected: { forms: ['select'], clear: true } },
            { shows: 'sizes the cover of an entry below the fold to the 1600 by 1000 of its drawing before its image is in the page', width: DESKTOP, read: cover, expected: { ratio: 1.6, image: false } },
            { shows: 'frames the hero photograph around its focus point of 20 across and 80 down', width: DESKTOP, read: focus, expected: '20% 80%' },
            {
                shows: 'serves one title and one description from the published profile with every published entry',
                width: DESKTOP,
                read: stated,
                expected: { titles: ['Studio Rasm · Architecture'], descriptions: ['Housing, civic buildings, and studies of timber construction.'], work: ['Harbour Library', 'Timber Joints', 'Quay Pavilion', 'Tide Archive'] },
            },
            {
                shows: 'hydrates the streamed document under its one title, an entry picked from the side index becoming the current work and taking focus',
                width: DESKTOP,
                read: picked,
                expected: { current: ['Timber Joints'], focused: 'Timber Joints', titles: ['Studio Rasm · Architecture'] },
            },
            {
                shows: 'fills its container at full scale and puts a narrower composition at the left edge, the middle, or the right edge its alignment names, in a container of 600px or over',
                width: 1900,
                read: aligned,
                expected: { 'full left': 'fills', 'full center': 'fills', 'full right': 'fills', 'medium left': 0, 'medium center': 50, 'medium right': 100, 'small left': 0, 'small center': 50, 'small right': 100 },
            },
            {
                shows: 'fills its container at full scale and puts a narrower composition at the left edge, the middle, or the right edge its alignment names, in a container under 600px',
                width: 600,
                read: aligned,
                expected: { 'full left': 'fills', 'full center': 'fills', 'full right': 'fills', 'medium left': 0, 'medium center': 50, 'medium right': 100, 'small left': 0, 'small center': 50, 'small right': 100 },
            },
            { shows: 'draws small narrower than medium and medium narrower than full at every alignment, in a container of 600px or over', width: 1900, read: scaled, expected: { left: ['small', 'medium', 'full'], center: ['small', 'medium', 'full'], right: ['small', 'medium', 'full'] } },
            { shows: 'draws small narrower than medium and medium narrower than full at every alignment, in a container under 600px', width: 600, read: scaled, expected: { left: ['small', 'medium', 'full'], center: ['small', 'medium', 'full'], right: ['small', 'medium', 'full'] } },
            { shows: 'sets the two items of a composition side by side in a container of 600px or over', width: 680, read: paired, expected: { wide: true, beside: true, beneath: false } },
            { shows: 'stacks the two items of a composition in a container under 600px', width: 640, read: paired, expected: { wide: false, beside: false, beneath: true } },
            { shows: 'gives two items side by side one height, each frame in the shape of its file, 1600 by 1000 and 1200 by 1800', width: DESKTOP, read: levelled, expected: { taller: 0, shapes: [1.6, 0.7] } },
            { shows: 'keeps a short composition taller than its content from a 600px page', width: 600, read: hugged, expected: 1 },
            { shows: 'draws a short composition as tall as its content under a 600px page', width: 599, read: hugged, expected: 0 },
            { shows: 'ends the page content inside the header edge from a 1900px page', width: 1900, read: ended, expected: 1 },
            { shows: 'ends the page content at the header edge under a 1900px page', width: 1899, read: ended, expected: 0 },
            { shows: 'shows the first of ten compositions alone in its carousel, under the pointer at its middle, the rest clipped inside the page width', width: DESKTOP, read: shown, expected: { within: ['1 of 10'], centre: '1 of 10', spill: 0 } },
            { shows: 'shows the second composition alone once Next composition is pressed, the rest clipped inside the page width', width: DESKTOP, read: advanced, expected: { selected: '2 of 10', within: ['2 of 10'], centre: '2 of 10', spill: 0 } },
            { shows: 'sets Overview at the start of its carousel and both arrows at its end on one line, each arrow a 44px target', width: DESKTOP, read: controls, expected: { start: 0, end: 0, ordered: true, level: [0, 0], targets: [44, 44, 44, 44] } },
            { shows: 'sizes its carousel to the second composition once it shows, while the first is taller', width: DESKTOP, read: fitted, expected: { selected: '2 of 10', extra: 0, taller: true } },
            { shows: 'opens the viewer in front of the page with one margin on every side, its stage reaching the end of the dialog and focus inside it, from a 600px page', width: 600, read: expanded, expected: { even: true, bleeds: false, stage: 0, front: true, focused: true } },
            { shows: 'opens the viewer over the whole page under a 600px page', width: 599, read: expanded, expected: { even: true, bleeds: true, stage: 0, front: true, focused: true } },
            {
                shows: 'centres the contact sheet, which scrolls to its last composition while Close overview stays in reach and focus stays in the list',
                width: DESKTOP,
                read: listed,
                expected: { centred: [0, 0], scrolled: true, last: 'Composition 10', close: 'Close overview', focused: true },
            },
            { shows: 'fits a sheet in the viewer stage with its text under the pointer and nothing to scroll', width: DESKTOP, read: sheeted, expected: { inset: [0, 0, 0, 0], overflow: [0, 0], under: 'Harbour section' } },
            { shows: 'scrolls a zoomed sheet inside the viewer stage to 40 across and 30 down', width: DESKTOP, read: zoomed, expected: [40, 30] },
            { shows: 'passes a pointer over sheet text to the page under it with Pan on, and takes touch drags itself', width: DESKTOP, read: panned, expected: { pans: true, touch: 'none', text: false } },
        ],
    },
    owner: {
        path: '/?edit=true',
        rows: [
            { shows: 'docks the editor at the end of the page from top to bottom, the page in view beside it at desktop width', width: DESKTOP, read: docked, expected: { edges: [0, 0, 0], beside: true } },
            { shows: 'spans the editor over the whole page at phone width', width: PHONE, read: docked, expected: { edges: [0, 0, 0], beside: false } },
            { shows: 'draws the entries panel and keeps the mounted profile fields out of view, with the three tabs on one line', width: DESKTOP, read: panelled, expected: { entries: true, profile: false, rows: 1 } },
            { shows: 'sets a label above its text field, the field spanning its panel at a 44px target and the multiline field taller', width: DESKTOP, read: fielded, expected: { above: true, spans: 0, target: true, taller: true } },
            { shows: 'sets Title and Entry type in two equal columns from a 600px page', width: 600, read: detailed, expected: { columns: 2, wider: 0 } },
            { shows: 'sets Title and Entry type in one column under a 600px page', width: 599, read: detailed, expected: { columns: 1, wider: 0 } },
            { shows: 'sets the Scale label above its three choices, which share one line as 44px targets', width: DESKTOP, read: chosen, expected: { above: true, rows: 1, targets: true } },
            {
                shows: 'puts each focal point thumb at 20 and 80 along the middle of a 44px track, with its value at the end of the label line',
                width: DESKTOP,
                read: slid,
                expected: [
                    { value: '20', along: 20, level: 0, end: 0, track: 44 },
                    { value: '80', along: 80, level: 0, end: 0, track: 44 },
                ],
            },
            { shows: 'sets the checkbox, drag handle, and Move of an entry row on one line as 44px targets, Move at its end and the entry spanning the row beneath', width: DESKTOP, read: rowed, expected: { ordered: true, level: [0, 0], end: 0, beneath: true, spans: 0, targets: true } },
            {
                shows: 'opens Move as a menu in front of the editor with one 44px item per entry and focus inside',
                width: DESKTOP,
                read: moved,
                expected: { items: ['Harbour Library', 'Timber Joints', 'Quay Pavilion', 'Tide Archive'], heights: [44, 44, 44, 44], front: 'Timber Joints', focused: true },
            },
            {
                shows: 'centres the Remove entry confirmation in front of the editor with focus on Keep, both actions on one line at the end, and Save draft out of reach',
                width: DESKTOP,
                read: confirmed,
                expected: { centred: [0, 0], focused: 'Keep', level: 0, end: 0, front: 'Keep', covered: true },
            },
            {
                shows: 'opens one 44px option per destination in front of the editor',
                width: DESKTOP,
                read: destined,
                expected: {
                    options: ['Library', 'Hero (one file)', 'Harbour Library · Project', 'Timber Joints · Study', 'Quay Pavilion · Project', 'Tide Archive · Project'],
                    heights: [44, 44, 44, 44, 44, 44],
                    front: 'Library',
                },
            },
            { shows: 'spans the destination select over its field with its value and chevron at opposite ends', width: DESKTOP, read: spanned, expected: { spans: 0, balance: 0 } },
            { shows: 'lists library files two to a line at one width', width: DESKTOP, read: filed, expected: { beside: true, wider: 0, lower: 0 } },
            { shows: 'sets the sequence list beside the composition it edits from an 800px page', width: 800, read: sequenced, expected: { list: true, toggle: false, beside: true } },
            { shows: 'hides the sequence list behind Choose under an 800px page', width: 799, read: sequenced, expected: { list: false, toggle: true, beside: false } },
        ],
    },
} as const;

const cases: readonly Case[] = (['visitor', 'owner'] as const).flatMap((viewer) => drawn[viewer].rows.map((row) => ({ ...row, viewer, path: drawn[viewer].path })));

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Case, cases, content };
