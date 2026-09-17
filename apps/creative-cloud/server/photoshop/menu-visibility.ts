// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Schema } from 'effect';

// --- [TABLE] ---------------------------------------------------------------------------

const _HIDDEN = [
    { path: 'File', label: 'Invite to Edit...' },
    { path: 'File', label: 'Share for Review' },
    { path: 'File > Export', label: 'Send to Firefly Boards' },
    { path: 'File', label: 'Search Adobe Stock...' },
    { path: 'File', label: 'Search Adobe Express Templates...' },
    { path: 'File', label: 'Version History' },
    { path: 'Edit', label: 'Prompt to Edit...' },
    { path: 'Edit', label: 'Generative Fill...' },
    { path: 'Edit', label: 'Generate Image...' },
    { path: 'Edit', label: 'Reflection Removal...' },
    { path: 'Edit', label: 'Sky Replacement...' },
    { path: 'Image', label: 'Generative Upscale...' },
    { path: 'Layer', label: 'Harmonize' },
    { path: 'Layer > Layer Mask', label: 'Enhance edge' },
    { path: 'Type', label: 'More from Adobe Fonts...' },
    { path: 'Filter', label: 'Neural Filters...' },
    { path: 'Filter', label: 'Parametric Filters...' },
    { path: 'Filter', label: 'AI Denoise...' },
    { path: 'Filter', label: 'AI Sharpen...' },
    { path: 'Window', label: 'AI Assistant' },
    { path: 'Window', label: 'AI Assisted Editor' },
    { path: 'Window', label: 'Adobe Stock' },
    { path: 'Window', label: 'Beta Feedback' },
    { path: 'Window', label: 'Comments' },
    { path: 'Window', label: 'Libraries' },
    { path: 'Window', label: 'Version History' },
    { path: 'Window', label: 'Materials' },
    { path: 'Plugins', label: 'Browse Plugins...' },
    { path: 'Help', label: 'Photoshop Help...' },
    { path: 'Help', label: 'Hands-on Tutorials...' },
    { path: 'Help', label: "What's New..." },
    { path: 'Help', label: 'Learn more about generative credits' },
    { path: 'Help', label: 'Adobe generative AI user guidelines' },
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const MenuVisibility: Schema.$Array<Schema.Struct<{ readonly path: Schema.String; readonly label: Schema.String; readonly visible: Schema.Boolean }>> = Schema.Array(
    Schema.Struct({ path: Schema.String, label: Schema.String, visible: Schema.Boolean }),
);

const MENU_VISIBILITY: (typeof MenuVisibility)['Type'] = Schema.decodeSync(MenuVisibility)(Array.map(_HIDDEN, (row) => ({ ...row, visible: false })));

// --- [EXPORTS] -------------------------------------------------------------------------

export { MENU_VISIBILITY, MenuVisibility };
