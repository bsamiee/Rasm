# [FORMATTING]

Build reusable formatting around the relationship that must persist when content changes. Retain intentional local exceptions outside the requested change.

## [01]-[OWNERSHIP]

Choose the native owner by the requested result:

| [INDEX] | [RESULT]                          | [OWNER]                                |
| :-----: | :-------------------------------- | :------------------------------------- |
|  [01]   | Paragraph role and composition    | Paragraph style                        |
|  [02]   | Selected character attributes     | Character style                        |
|  [03]   | Frame behavior and appearance     | Object style                           |
|  [04]   | Table regions and cell text       | Table, cell, and paragraph styles      |
|  [05]   | Pattern-dependent text emphasis   | Paragraph-style GREP rule              |
|  [06]   | Sequential or line-based emphasis | Nested style or nested line style      |
|  [07]   | Numbered hierarchy                | Shared numbering list and level styles |
|  [08]   | Generated document text           | Native source-to-output relationship   |

Distinguish persistent formatting from a one-time text or formatting change. A saved Find/Change query stores search settings; it does not become a document rule.

## [02]-[DEFINITIONS]

Read actual style relationships, grouped definitions, affected text or objects, and local overrides. Retain native identities when names repeat. Reuse compatible definitions; derive related variants through inheritance and assign only differences.

Choose which attributes a character style or object-style category owns. Preserve unspecified attributes. Connect table regions to cell styles and their text styles. Establish list identity and restart policy before formatting markers or indentation.

For automatic text formatting, select the trigger from content behavior. Use GREP for textual patterns, sequential rules for delimiters or counted phrases, and line rules for composition-dependent ranges. Keep generated numbers and document structures with their native owners.

## [03]-[APPLICATION]

Apply definitions to their intended native scope. A stored definition alone changes no unstyled content. Preserve inheritance when adding rules and retain rule references for later changes. Resolve overlapping attributes through actual rule order and style precedence.

Evaluate the requested rule on representative positive, negative, and boundary cases from actual content. For persistent formatting, change representative text and confirm recomposition follows the intended rule. Inspect composed appearance alongside native attributes; a getter alone does not establish visible GREP formatting.

## [04]-[RESULT]

Keep definitions grouped by the user's editing purpose. Remove superseded definitions only after replacing their actual uses. Report native definitions and affected scope, explain deliberate local exceptions, and identify source conditions that prevent the requested rule from applying.
