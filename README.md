Scripl
======

Scripl let's you edit your exe files as if they were just regulary script files written in C#. 
Scripl does it by storing locally source code of exe using exe file's checksum.

## Command Line Usage
    scripl
        Runs the scripl monitoring and recompiling service. If it's not running, rest of the commands will still work.

    scripl edit [-w|wait] [-d|default]<file>
        Opens the file for edit and starts monitoring it for changes until the user closes the editor.
        -wait: waits for user input. Helpful when scripl can't wait for editor (i.e. when single instance editors are used)
        -default: omits the visual studio installation check and forces to use default application for cs files.

    scripl new 
        Creates new editme.exe and opens it for edit.

    scripl setup -service
        Installs scripl as windows service, optionally set the port number

    scripl setup -service -remove
        Removes scripl windows service

    scripl setup -shell
        Installs scripl shell extensions

    scripl setup -shell -remove
        Removes scripl shell extensions

    scripl add <sources> <existing file>
        Registers the sources for existing file

    scripl monitor <source> <exec>
        Starts monitoring and recompiling given file. The source file contents will be overriden by exec sources.
        -no-wait: monitor won't wait for user input. Used internally by edit command.
        -is-temp: indicates that source is temporary file and can be deleted.

## Using without command line
You can you scripl without command line - just run the 'scripl setup -service -shell' in Administrator Command Line. This will install Scripl Windows Service, and will add "Edit" option in the exe file context menu.



