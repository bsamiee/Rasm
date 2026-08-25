-- Title    : applescript-library
-- Purpose  : Reusable script object compiled to a .scpt/.scptd library — policy rows, the atomic
--            writer, and handlers consumers call by stable name.
-- Build    : osacompile -o ~/Library/Script\ Libraries/<LIBRARY_NAME>.scpt <LIBRARY_NAME>.applescript
-- Consume  : use <LIBRARY_NAME> : script "<LIBRARY_NAME>"
-- Replace  : <LIBRARY_NAME>, the policy row values, and the <DOMAIN_HANDLERS> region.
use AppleScript version "2.8"
use framework "Foundation"
use scripting additions

property parent : AppleScript
-- Policy rows are the library's whole tunable surface; configure replaces the record wholesale.
property policies : {strictCase:true, delimiter:",", timeoutSeconds:30}

-- Atomic UTF-8 write through Foundation; the reference out-parameter carries the fault back as a
-- value, and open for access remains a compatibility path rather than the primary writer.
on writeUTF8(textValue, posixPath)
	set sourceString to current application's NSString's stringWithString:textValue
	set targetPath to (current application's NSString's stringWithString:posixPath)'s stringByExpandingTildeInPath()
	set {ok, writeError} to sourceString's writeToFile:targetPath atomically:true ¬
		encoding:(current application's NSUTF8StringEncoding) |error|:(reference)
	if ok is false then error (writeError's localizedDescription() as text) number 9201
	return targetPath as text
end writeUTF8

on configure given strictCase:strictCaseFlag, delimiter:delimiterText, timeoutSeconds:budget
	set my policies to {strictCase:strictCaseFlag, delimiter:delimiterText, timeoutSeconds:budget}
	return me
end configure

-- <DOMAIN_HANDLERS> — exported operations take every input as an argument, return domain values,
-- and let native errors propagate.
