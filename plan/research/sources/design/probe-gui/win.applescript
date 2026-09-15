on marks(mi, pn)
	set out to ""
	tell application "System Events"
		tell process pn
			repeat with m in mi
				set n to name of m
				set mk to ""
				try
					set mk to value of attribute "AXMenuItemMarkChar" of m
				end try
				set out to out & n & "\t" & mk & linefeed
			end repeat
		end tell
	end tell
	return out
end marks
on sub(menuName, itemName, pn)
	tell application "System Events"
		tell process pn
			set mi2 to every menu item of menu 1 of menu item itemName of menu 1 of menu bar item menuName of menu bar 1
		end tell
	end tell
	return "--" & menuName & " > " & itemName & linefeed & my marks(mi2, pn)
end sub
on top(menuName, pn)
	tell application "System Events"
		tell process pn
			set mi2 to every menu item of menu 1 of menu bar item menuName of menu bar 1
		end tell
	end tell
	return "--" & menuName & linefeed & my marks(mi2, pn)
end top
set pn to "Adobe InDesign 2026 (Beta)"
tell application "System Events"
	tell process pn
		set names to name of every menu bar item of menu bar 1
	end tell
end tell
set r to "MENUBAR: " & (names as text) & linefeed
set r to r & my top("Window", pn) & my top("File", pn) & my top("Edit", pn) & my top("Type", pn) & my top("Object", pn) & my top("Help", pn) & my top("Plug-Ins", pn) & my top("View", pn)
set r to r & my sub("Window", "Utilities", pn) & my sub("Window", "Comments", pn) & my sub("Window", "Styles", pn) & my sub("Window", "Interactive", pn) & my sub("Window", "Type & Tables", pn) & my sub("Window", "Object & Layout", pn) & my sub("Window", "Output", pn) & my sub("Window", "Editorial", pn) & my sub("Window", "Color", pn) & my sub("Window", "Workspace", pn) & my sub("File", "User Settings", pn) & my sub("View", "Extras", pn) & my sub("Object", "Content", pn)
return r
