import type { Published } from '#/served';

// --- [TYPES] ---------------------------------------------------------------------------

interface Case {
    readonly shows: string;
    readonly width: number;
    readonly read: () => unknown;
    readonly expected: unknown;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PIXEL = Uint8Array.fromBase64('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==');
const DESKTOP = 1280;
const PHONE = 390;
const section = { id: '6f1c2a4e-3b5d-4c6e-8f90-1a2b3c4d5e6f', kind: 'image', name: 'harbour-section.png', mime: 'image/png', size: PIXEL.length, width: 1600, height: 1000 } as const;
const room = { id: '5e0d6c1b-2a3f-4b8e-9c7d-6f5a4b3c2d1e', kind: 'image', name: 'reading-room.png', mime: 'image/png', size: PIXEL.length, width: 1200, height: 1800 } as const;
const content = {
    files: [
        { asset: section, bytes: PIXEL },
        { asset: room, bytes: PIXEL },
    ],
    portfolio: {
        name: 'Studio Rasm',
        introduction: 'Housing, civic buildings, and studies of timber construction.',
        hero: { kind: 'image', assetId: room.id, role: 'photograph', alt: 'Reading room', framing: { mode: 'cover', x: 20, y: 80 } },
        entries: [
            { id: '7f1c2a4e-3b5d-4c6e-8f90-1a2b3c4d5e6f', kind: 'project', title: 'Harbour Library' },
            { id: '0a9b8c7d-6e5f-4a3b-9c2d-1e0f9a8b7c6d', kind: 'study', title: 'Timber Joints' },
            { id: '3c1f0e52-8a47-4d19-b6c3-2e5d7a9f1b04', kind: 'project', title: 'Quay Pavilion' },
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

// --- [COMPOSITION] ---------------------------------------------------------------------

const cases: readonly Case[] = [
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
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Case, cases, content };
