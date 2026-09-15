-- menu-click — click one menu item by path.
--
--   osascript menu-click.applescript "<process>" "Edit" "Color Settings..."
--
-- Prints "clicked" or "ERROR: <message>". It deliberately does not wait for a
-- window: an Adobe dialog drawn by the Drover framework never appears in the AX
-- window list at all, so the caller detects the new window through the window
-- server instead.

on run argv
	set procName to item 1 of argv
	set menuPath to rest of argv
	tell application "System Events"
		tell process procName
			try
				set topName to item 1 of menuPath
				-- "container" and "target" are System Events property names; using
				-- either as a variable makes AppleScript parse this as a property set.
				set theMenu to menu 1 of (menu bar item topName of menu bar 1)
				repeat with i from 2 to (count of menuPath)
					set itemName to item i of menuPath
					set theItem to menu item itemName of theMenu
					if i < (count of menuPath) then
						set theMenu to menu 1 of theItem
					else
						click theItem
					end if
				end repeat
				return "clicked"
			on error errMsg
				return "ERROR: " & errMsg
			end try
		end tell
	end tell
end run
