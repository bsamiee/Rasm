# [FUNCTION_HOOKS]

Hooks correct tool calls, format edits, and draw session facts. With `observation` true, they record events as rows:
- Use `plugin-authoring` skill for writing or changing a function hook
- Use `observation` skill for database reads and writes

## [01]-[TOOL_CALLS]

Hooks fix a wrong call and let it run, the model keeps working and learns the repository form from the returned context:
- ALWAYS rewrite a call when its repository form follows from the call, and refuse only when no form follows
- ALWAYS write each policy as a pure function in `policies/` over the parsed call, folded under the one `tool.call` registration
- ALWAYS declare a program's options, bodies, input, and inner command as its `invocation.ts` row, an undeclared option's value parses as an operand
- ALWAYS read files, paths, environment, and repository root through `Host`, `register.ts` and `.codex/hooks/host.ts` each build one for `commandDecision`
- Calls the hook cannot parse or judge are refused

## [02]-[OUTPUT]

Texts reaching a model or harness (refusal, context, notice, band row) appear only when they change the next action:
- ALWAYS write a fact sentence, then an action sentence where one follows, under 15 words and passing `clean-prose`
- ALWAYS state what the hook derived from the call and point to the owner (skill, file, target) of the rest
- ALWAYS draw a visual only for a fact its reader acts on
- Enumerated cases, hardcoded values, and guessed fixes steer the model to a wrong form

## [03]-[RUNTIME]

Harness loads `hooks/register.ts` and its imports with no DOM and no Node. Modules import their own files by relative path and `claude-code` alone:
- ALWAYS place each pure function in the folder of its concern, `hooks/register.ts` alone reads `$`
- ALWAYS compose faults through the result type in `composition.ts`
- ALWAYS keep session values in `$.state`, keys and types declared in `hooks/state.d.ts`, module variables and timers reset at reload
- ALWAYS draw render hooks from `$.state` alone, handlers and events write

## [04]-[OBSERVATION]

Database schema is the declarations in `observation/sql.ts`, and each load applies the delta. Removing `.cache/observation/` resets it:
- ALWAYS add a purpose as one `_VIEWS` element with its reader in the `observation` skill, or as one `.claude/agents/` file preloading that skill
- ALWAYS change the `observation` skill in the same change as the database it reads
- Rows insert once, no statement updates or deletes one
