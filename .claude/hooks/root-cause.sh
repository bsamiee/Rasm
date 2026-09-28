#!/bin/sh
exec jq --rawfile text "$CLAUDE_PROJECT_DIR/.claude/hooks/root-cause.md" '{hookSpecificOutput: {hookEventName: .hook_event_name, additionalContext: $text}}'
