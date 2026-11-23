import { lexer } from 'css-tree';
import type { Config } from 'stylelint';
import recess from 'stylelint-config-recess-order/groups';

// --- [CONSTANTS] -----------------------------------------------------------------------

const systemSyntax = lexer.getType('system-color')?.syntax;
const systemColors = systemSyntax?.type === 'Group' ? systemSyntax.terms.flatMap((term) => (term.type === 'Keyword' ? [term.name] : [])) : [];
const vendorPrefix = '/^-(?!-)/';

// --- [STYLELINT] -----------------------------------------------------------------------

export default {
    extends: ['stylelint-config-standard', '@projectwallace/stylelint-plugin/configs/correctness'],
    plugins: ['stylelint-use-nesting', 'stylelint-use-logical-spec', 'stylelint-order', 'stylelint-declaration-block-no-ignored-properties', 'stylelint-plugin-defensive-css'],
    ignoreDisables: true,
    reportDescriptionlessDisables: true,
    reportInvalidScopeDisables: true,
    reportNeedlessDisables: true,
    reportUnscopedDisables: true,
    overrides: [
        {
            name: 'portfolio',
            files: ['../products/web/portfolio/**/*.css'],
            referenceFiles: ['../products/web/portfolio/site/theme.css'],
            rules: { 'rule-nesting-at-rule-required-list': ['layer'] },
        },
    ],
    rules: {
        // --- [AT_RULES]

        'at-rule-allowed-list': ['container', 'font-face', 'function', 'import', 'keyframes', 'layer', 'media', 'page', 'position-try', 'property', 'scope', 'starting-style', 'supports', 'view-transition'],
        'at-rule-prelude-no-invalid': true,
        'at-rule-property-required-list': { 'font-face': ['font-display', 'font-family', 'src'], property: ['syntax', 'inherits', 'initial-value'] },
        'keyframe-selector-notation': 'percentage',
        'media-feature-name-disallowed-list': ['/^(min-|max-)?device-(width|height|aspect-ratio)$/', vendorPrefix],

        // --- [COLORS]

        'alpha-value-notation': 'number',
        'color-named': 'never',
        'color-no-hex': true,
        'color-no-invalid-hex': true,
        'value-type-custom-property-required-list': ['color', { ignoreValues: ['transparent', 'currentcolor', `/^(?:${systemColors.join('|')})$/i`] }],

        // --- [COMMENTS]

        'comment-pattern': String.raw`^((?=.{85}$)--- \[[A-Z0-9_]+\] -+|--- \[[A-Z0-9_]+\]|stylelint-.+)$`,

        // --- [DECLARATIONS]

        'block-no-empty': true,
        'declaration-block-no-duplicate-properties': true,
        'declaration-property-value-disallowed-list': { display: [String.raw`/\binline-(block|flex|grid|table)\b/`], '/^transition/': [String.raw`/\ball\b/`], '/^(?!--)/': [String.raw`/(?:^|[\s(,])-(?!-)[a-z]+-/`] },
        'font-weight-notation': 'numeric',
        'import-notation': 'string',
        'max-nesting-depth': [2, { ignore: ['blockless-at-rules'], ignoreAtRules: ['layer', 'scope'] }],
        'no-descending-specificity': null,
        'no-unknown-animations': true,
        'no-unknown-custom-media': true,
        'no-unknown-custom-properties': true,
        'property-disallowed-list': [vendorPrefix],
        'property-no-unknown': [true, { checkPrefixed: true }],
        'string-no-newline': true,
        'time-min-milliseconds': 50,

        // --- [FUNCTIONS]

        'function-disallowed-list': ['rgb', 'rgba', 'hsl', 'hsla', 'hwb'],
        'function-linear-gradient-no-nonstandard-direction': true,
        'function-no-unknown': true,
        'function-url-no-scheme-relative': true,
        'function-url-scheme-allowed-list': ['data'],

        // --- [SELECTORS]

        'relative-selector-nesting-notation': 'implicit',
        'selector-attribute-name-disallowed-list': ['class', 'id', 'style'],
        'selector-class-pattern': '^(?:[a-z][a-z0-9]*(?:-[a-z0-9]+)*|react-aria-[A-Z][A-Za-z]*|uppy-[A-Z][A-Za-z]*(?:-[a-z]+)*|textLayer)$',
        'selector-combinator-allowed-list': [' ', '>', '+', '~'],
        'selector-max-attribute': 2,
        'selector-max-combinators': 2,
        'selector-max-compound-selectors': 3,
        'selector-max-id': 0,
        'selector-max-pseudo-class': 3,
        'selector-max-specificity': '0,4,0',
        'selector-max-universal': 1,
        'selector-no-vendor-prefix': true,
        'selector-pseudo-class-disallowed-list': [vendorPrefix],
        'selector-pseudo-element-disallowed-list': [vendorPrefix],
        'selector-type-no-unknown': true,

        // --- [UNITS]

        'declaration-property-unit-disallowed-list': { '/^(font|font-size|line-height)$/': ['px'] },
        'unit-disallowed-list': ['cm', 'mm', 'q', 'in', 'pt', 'pc'],
        'unit-no-unknown': true,
        'value-no-vendor-prefix': true,

        // --- [PLUGINS]

        'csstools/use-nesting': 'always',
        'defensive-css/no-accidental-hover': true,
        'liberty/use-logical-spec': 'always',
        'order/order': ['custom-properties', 'declarations', 'rules', 'at-rules'],
        'order/properties-order': [recess, { unspecified: 'bottomAlphabetical' }],
        'plugin/declaration-block-no-ignored-properties': true,
        'projectwallace/no-duplicate-custom-properties': null,
        'projectwallace/no-unused-custom-properties': null,
        'projectwallace/no-unused-layers': null,
    },
} satisfies Config;
