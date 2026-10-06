#!/bin/sh
exec jq --rawfile text "${0%/*}/root-cause.md" '{hookSpecificOutput: {hookEventName: .hook_event_name, additionalContext: $text}}'
