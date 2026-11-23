import { lexer } from 'css-tree';
import stylelint, { type Config, type Rule } from 'stylelint';
import recess from 'stylelint-config-recess-order/groups';

// --- [CONSTANTS] -----------------------------------------------------------------------

const systemSyntax = lexer.getType('system-color')?.syntax;
const systemColors = systemSyntax?.type === 'Group' ? systemSyntax.terms.flatMap((term) => (term.type === 'Keyword' ? [term.name] : [])) : [];
const vendorPrefix = '/^-(?!-)/';

// --- [PLUGINS] -------------------------------------------------------------------------

const documentUnits = 'rasm/document-declaration-property-unit-disallowed-list';

const isolated = (root: Parameters<ReturnType<Rule>>[0]) => root.some((node) => node.type === 'rule' && node.selectors.some((selector) => /^:host$/iu.test(selector)) && node.some((held) => held.type === 'decl' && /^all$/iu.test(held.prop) && /^initial$/iu.test(held.value)));

const documentRule: Rule = Object.assign(
    (units: unknown): ReturnType<Rule> =>
        (root, result) =>
            isolated(root)
                ? undefined
                : stylelint.utils.checkAgainstRule({ ruleName: 'declaration-property-unit-disallowed-list', ruleSettings: units, root, result }, (warning) => {
                      stylelint.utils.report({ ruleName: documentUnits, result, message: warning.text, node: warning.node, start: { line: warning.line, column: warning.column }, end: { line: warning.endLine ?? warning.line, column: warning.endColumn ?? warning.column } });
                  }),
    { ruleName: documentUnits, messages: {} },
);

// --- [STYLELINT] -----------------------------------------------------------------------

export default {
    extends: ['stylelint-config-standard', '@projectwallace/stylelint-plugin/configs/correctness'],
    plugins: ['stylelint-use-nesting', 'stylelint-use-logical-spec', 'stylelint-order', 'stylelint-declaration-block-no-ignored-properties', 'stylelint-plugin-defensive-css', stylelint.createPlugin(documentUnits, documentRule)],
    ignoreDisables: true,
    overrides: [
        {
            name: 'portfolio',
            files: ['../products/web/portfolio/**/*.css'],
            referenceFiles: ['../products/web/portfolio/site/theme.css'],
            rules: {
                'rule-nesting-at-rule-required-list': ['layer'],
                'selector-class-pattern': '^(?:[a-z][a-z0-9]*(?:-[a-z0-9]+)*|react-aria-[A-Z][A-Za-z]*|uppy-[A-Z][A-Za-z]*(?:-[a-z]+)*|textLayer)$',
            },
        },
    ],
    rules: {
        // --- [AT_RULES]

        'at-rule-allowed-list': ['container', 'font-face', 'import', 'keyframes', 'layer', 'media', 'page', 'position-try', 'property', 'scope', 'starting-style', 'supports', 'view-transition'],
        'at-rule-property-required-list': { 'font-face': ['font-display', 'font-family', 'src'] },
        'keyframe-selector-notation': 'percentage',
        'media-feature-name-disallowed-list': ['/^(min-|max-)?device-(width|height|aspect-ratio)$/i', vendorPrefix],

        // --- [COLORS]

        'alpha-value-notation': 'number',
        'color-named': 'never',
        'color-no-hex': true,
        'value-type-custom-property-required-list': ['color', { ignoreValues: ['transparent', 'currentcolor', `/^(?:${systemColors.join('|')})$/i`] }],

        // --- [COMMENTS]

        'comment-pattern': String.raw`^--- \[[A-Z][A-Z0-9]*(?:_[A-Z0-9]+)*\](?: -+(?<=^.{85}))?$`,

        // --- [DECLARATIONS]

        'declaration-block-no-duplicate-properties': true,
        'declaration-property-value-disallowed-list': { display: [String.raw`/(?<![\w-])inline-/`], '/^transition/': [String.raw`/(?<![\w-])all(?![\w-])/`], '/^(?!--)/': [String.raw`/(?:^|[\s(,])-(?!-)[a-z]+-/i`] },
        'font-weight-notation': ['numeric', { ignore: ['relative'] }],
        'import-notation': 'string',
        'max-nesting-depth': [3, { ignore: ['blockless-at-rules'], ignoreAtRules: ['starting-style'] }],
        'no-descending-specificity': null,
        'no-unknown-animations': true,
        'no-unknown-custom-media': true,
        'property-disallowed-list': [vendorPrefix],
        'string-no-newline': true,
        'time-min-milliseconds': [50, { ignore: ['delay'] }],

        // --- [FUNCTIONS]

        'function-disallowed-list': ['rgb', 'rgba', 'hsl', 'hsla', 'hwb', 'lab', 'lch', 'oklab'],
        'function-linear-gradient-no-nonstandard-direction': true,
        'function-no-unknown': true,
        'function-url-no-scheme-relative': true,
        'function-url-scheme-allowed-list': ['data'],

        // --- [SELECTORS]

        'relative-selector-nesting-notation': 'implicit',
        'selector-attribute-name-disallowed-list': ['/^(class|id|style)$/i'],
        'selector-combinator-allowed-list': [' ', '>', '+', '~'],
        'selector-max-attribute': 2,
        'selector-max-combinators': 2,
        'selector-max-id': 0,
        'selector-max-pseudo-class': 3,
        'selector-max-specificity': '0,4,0',
        'selector-max-universal': 1,
        'selector-pseudo-class-disallowed-list': [vendorPrefix],
        'selector-pseudo-element-disallowed-list': [vendorPrefix],

        // --- [UNITS]

        [documentUnits]: { '/^(font|font-size|line-height)$/i': ['px'] },
        'unit-disallowed-list': ['cm', 'mm', 'q', 'in', 'pt', 'pc'],
        'unit-no-unknown': true,

        // --- [PLUGINS]

        'csstools/use-nesting': 'always',
        'defensive-css/no-accidental-hover': true,
        'defensive-css/no-unsafe-clamp-font-size': [true, { reportUnresolvable: false }],
        'defensive-css/no-unsafe-will-change': true,
        'liberty/use-logical-spec': 'always',
        'order/order': ['custom-properties', 'declarations', 'rules', 'at-rules'],
        'order/properties-order': [recess, { unspecified: 'bottomAlphabetical' }],
        'plugin/declaration-block-no-ignored-properties': true,
        'projectwallace/no-anonymous-layers': true,
        'projectwallace/no-duplicate-custom-properties': null,
        'projectwallace/no-unused-custom-properties': null,
        'projectwallace/no-unused-layers': null,
    },
} satisfies Config;
