LFandUTF8
=========

Fixing windows cygwin git pain.. partially
---

This tool is intended to convert files in git repository into __UTF8 without BOM__ and __LF (Unix)__ line endings.
Give it a git-tracked repository as an argument and then answer questions like this:


       /your/repo/file.txt
         is it text or binary file? Press T/B


It will learn text or binary files based on extensions (remembering choice, but will always ask for files without extension) and do wollowing on each __text__ file:

* Create backup (placed near with name like __file.txt.{datetime}.bak__) of current file
* Detect UTF-8 or 1251 as fallback (it is hardcoded, change to your system default if needed)
* Replace CRLF with LF
* Add trailing newline if not present
* Save the file in-place

_**Note:** files marked as binary remain intact_

Known issues
---

* Fails on UTF-8 with BOM (detects as 1251)
* Fails on cp866 and other encodings
