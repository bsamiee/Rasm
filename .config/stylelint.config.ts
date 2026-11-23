import patches from '@csstools/css-syntax-patches-for-csstree' with { type: 'json' };
import { type DSNode, definitionSyntax, fork, type SyntaxDescriptor } from 'css-tree';
import stylelint, { type Config, type Rule } from 'stylelint';
import recess from 'stylelint-config-recess-order/groups';

// --- [CONSTANTS] -----------------------------------------------------------------------

const vendorPrefix = '/^-(?!-)/';

// --- [GRAMMAR] -------------------------------------------------------------------------

const { lexer } = fork(patches.next);

const named = (type: string, kind: 'Keyword' | 'Type') => [lexer.getType(type)?.syntax].flatMap((syntax) => (syntax?.type === 'Group' ? syntax.terms : [])).flatMap((term) => (term.type === kind ? [term.name] : []));

const systemColors = named('system-color', 'Keyword').join('|');
const namedColors = named('named-color', 'Keyword').join('|');
const easings = named('easing-function', 'Type')
    .flatMap((type) => named(type, 'Keyword'))
    .join('|');
const colorFunctions = named('color-function', 'Type')
    .map((name) => name.replace('()', ''))
    .filter((name) => name !== 'oklch')
    .join('|');

const standard = (node: DSNode): DSNode => (node.type === 'Group' ? { ...node, terms: node.terms.filter((term) => !(term.type === 'Type' && /^(?:-(?:non-standard|legacy)-|display-legacy$)/u.test(term.name))).map(standard) } : node);

const grammar = (described: readonly (SyntaxDescriptor | null)[]) =>
    Object.fromEntries(
        described
            .flatMap((entry) => (entry?.syntax === null || entry?.syntax === undefined ? [] : [{ name: entry.name, stated: definitionSyntax.generate(entry.syntax), kept: definitionSyntax.generate(standard(entry.syntax)) }]))
            .filter(({ stated, kept }) => stated !== kept)
            .map(({ name, kept }) => [name, kept]),
    );

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
    languageOptions: { syntax: { properties: grammar(Object.keys(patches.next.properties).map((name) => lexer.getProperty(name))), types: grammar(Object.keys(patches.next.types).map((name) => lexer.getType(name))) } },
    overrides: [
        {
            name: 'portfolio',
            files: ['../products/web/portfolio/**/*.css'],
            referenceFiles: ['../products/web/portfolio/**/*.css'],
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
        'value-type-custom-property-required-list': [['color', 'easing-function'], { ignoreValues: ['transparent', 'currentcolor', `/^(?:${systemColors}|${easings})$/i`] }],

        // --- [COMMENTS]

        'comment-pattern': String.raw`^--- \[[A-Z][A-Z0-9]*(?:_[A-Z0-9]+)*\](?: -+(?<=^.{85}))?$`,

        // --- [DECLARATIONS]

        'declaration-block-no-duplicate-properties': true,
        'declaration-property-value-disallowed-list': {
            '/^/': [String.raw`/color-mix\((?!\s*in\s+oklab\s*,)/i`],
            '/^transition/': [String.raw`/(?<![\w-])all(?![\w-])/`],
            '/^(?!--)/': [String.raw`/(?:^|[\s(,])-(?!-)[a-z]+-/i`, String.raw`/oklch\((?!\s*from\s)/i`, String.raw`/(?:#[\da-f]+|(?<![\w-])(?:${namedColors}))\s*;/i`],
        },
        'font-weight-notation': ['numeric', { ignore: ['relative'] }],
        'import-notation': 'string',
        'max-nesting-depth': [3, { ignore: ['blockless-at-rules'], ignoreAtRules: ['starting-style'] }],
        // Flags a nested part after a more specific part of another component, each class rule nests its own parts
        'no-descending-specificity': null,
        'no-unknown-animations': true,
        'no-unknown-custom-media': true,
        'property-disallowed-list': [vendorPrefix],
        'string-no-newline': true,
        'time-min-milliseconds': [50, { ignore: ['delay'] }],
        'value-keyword-case': ['lower', { ignoreKeywords: [`/^(?:${systemColors});$/i`] }],

        // --- [FUNCTIONS]

        'function-disallowed-list': [`/^(?:${colorFunctions}|device-cmyk)$/i`],
        'function-linear-gradient-no-nonstandard-direction': true,
        'function-no-unknown': true,
        'function-url-no-scheme-relative': true,
        'function-url-scheme-allowed-list': ['data'],

        // --- [SELECTORS]

        'relative-selector-nesting-notation': 'implicit',
        'selector-attribute-name-disallowed-list': ['/^(class|id|open|style)$/i'],
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
        'defensive-css/no-list-style-none': true,
        'defensive-css/no-unsafe-clamp-font-size': true,
        'defensive-css/no-unsafe-will-change': true,
        'defensive-css/require-focus-visible': true,
        'defensive-css/require-forced-colors-focus': true,
        'liberty/use-logical-spec': 'always',
        'order/order': ['custom-properties', 'declarations', 'rules', 'at-rules'],
        'order/properties-order': [recess, { unspecified: 'bottomAlphabetical' }],
        'plugin/declaration-block-no-ignored-properties': true,
        'projectwallace/no-anonymous-layers': true,
        'projectwallace/no-atrule-browserhacks': true,
        // Flags a variant, state, or query reassigning a custom property, `declaration-block-no-duplicate-custom-properties` owns one block
        'projectwallace/no-duplicate-custom-properties': null,
        'projectwallace/no-duplicate-data-urls': true,
        'projectwallace/no-property-browserhacks': true,
        'projectwallace/no-value-browserhacks': true,
    },
} satisfies Config;
