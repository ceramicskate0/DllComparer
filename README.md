# DllComparer Intro
This is a C# application that is be able to extract DLL information from running processes and conduct limited reporting on that data for analysis. This app could be used for both Blue team and Red teaming.

# Like the work dont forget to hit that Star Button and Sponsor

## Usage (after you compile it in Visual Studio)

    ./DllComparer.exe ?
    
            Commands Menu:
            -h
            Show Help Menu

            -d 
            Dump all the DLL's seen with the count of how many times each was seen.

            -s
            Dump all process and show their Dll's

            -e
            Show errors

            -f {SearchTerm}
            Search for Process name, PID, or DLL name

            -j {FileName}
            Write a JSON with all findings to disk
    
## Features
- View DLL information for running processes for analysis.

## Tip
  You will have to run as admin to see every process info (that the way windows is built).

### Powershell 1 Liner (Work in progress)
`Get-Process * | select -ExpandProperty modules | select -Unique | Where-Object {!$_.FileName.ToLower().Contains('c:\windows\') -and !$_.FileName.ToLower().Contains('c:\program files\windowsapps\')}| out-file "$Env:HOMEPATH\Downloads\json.json"`

## Adding to your code/Contribute
- Fork and submit pull request

## Credits
- Help with DLL gathering: https://stackoverflow.com/questions/36431220/getting-a-list-of-dlls-currently-loaded-in-a-process-c-sharp

## Disclaimer
Use at your own risk. For educational purposes only.
