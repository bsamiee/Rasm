---
paths:
  - "plugins/function-hooks/**"
  - ".codex/hooks/**"
---

# [FUNCTION_HOOKS]

Claude Code's mod and `.codex/hooks/rewrite.ts` apply shared command policies through native host adapters.

- Use `plugin-authoring` skill for writing or changing a function hook

## [01]-[TOOL_CALLS]

Hooks fix a wrong call and let it run, the model keeps working:
- ALWAYS rewrite a call when its repository form follows from the call, and refuse only when no form follows
- ALWAYS write each command policy as a pure function in `policies/` over the parsed call, folded into `commandDecision`
- ALWAYS declare a program's options, bodies, input, and inner command as its `invocation.ts` row, an undeclared option's value parses as an operand
- ALWAYS read process and path facts through the `System` each adapter builds once and passes to `commandDecision`
- Discover tasks through native Nx JSON commands, hooks own no task catalog or command routing
- Settings `PreToolUse` hooks run inside `next` on the rewritten command and return a refusal as an `isError` result
- Registrations on one event nest in order, first outermost, and a repeated registration fails module load
- Calls the hook cannot parse or judge are refused

## [02]-[OUTPUT]

Texts reaching a model or harness (refusal, context, toast, log line) appear only when they change the next action:
- ALWAYS write a fact sentence, then an action sentence where one follows, under 15 words and passing `clean-prose`
- ALWAYS state what the hook derived from the call and point to the owner (skill, file, target) of the rest
- Enumerated cases, hardcoded values, and guessed fixes steer the model to a wrong form
- `context` is a note Claude reads after a tool result, and users never see it
- `$.ui.toast` text reaches users alone

## [03]-[RUNTIME]

Harness loads `hooks/register.ts` and its imports with no DOM and no Node. Modules import their own files by relative path and `claude-code` alone:
- ALWAYS place each pure function in the folder of its concern, `hooks/register.ts` alone reads `$`
- ALWAYS pass `$` whole to functions declared at top level of `register.ts`, any other use of `$` fails module load
- ALWAYS keep files `.codex/hooks/rewrite.ts` imports inside the plugin, with APIs both Node and the harness supply
- ALWAYS compose faults through the result type in `composition.ts`
- ALWAYS spawn a program by name, or by the path `mise which` prints in the repository when mise configuration changes the resolved path
- Modules that fail to load register no hook, and every call then runs unchecked
