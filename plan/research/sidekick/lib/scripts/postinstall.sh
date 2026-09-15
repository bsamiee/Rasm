#!/bin/bash
#
# Post-install script for macOS .pkg installer
# 1. Creates symlink to the binary
# 2. Prompts user to register Sidekick with Claude Desktop
#

set -x

# Create symlink (oclif default behavior)
sudo mkdir -p /usr/local/bin
sudo ln -sf /usr/local/lib/indesign-sidekick/bin/indesign-sidekick /usr/local/bin/indesign-sidekick

# Check if Claude Desktop config directory exists
CONFIG_DIR="$HOME/Library/Application Support/Claude"
if [ -d "$CONFIG_DIR" ]; then
  # Show dialog asking user if they want to register
  response=$(osascript -e 'display dialog "Register Sidekick with Claude Desktop?\n\nThis will add Sidekick to your Claude Desktop configuration so it'\''s available automatically." buttons {"No", "Yes"} default button "Yes" with title "Sidekick for InDesign"' 2>/dev/null)

  if [[ "$response" == *"Yes"* ]]; then
    /usr/local/bin/indesign-sidekick register 2>/dev/null || true
  fi
fi

exit 0
