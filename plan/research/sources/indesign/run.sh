#!/bin/zsh
JSX="$1"
SECS="${2:-1800}"
/usr/bin/osascript \
  -e "with timeout of ${SECS} seconds" \
  -e "tell application id \"com.adobe.InDesign\" to do script \"app.doScript(File('${JSX}'), ScriptLanguage.JAVASCRIPT)\" language javascript" \
  -e "end timeout"
