import yaml from '@tree-sitter-grammars/tree-sitter-yaml/grammar.js';
import { extend } from '#/tools/ast-grep/grammars/derive.js';

// --- [EXPORTS] -------------------------------------------------------------------------

export default extend(yaml);
