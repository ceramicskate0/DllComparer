# DllComparer Intro
This is a C# application that is be able to extract DLL information from running processes and conduct limited reporting on that data for analysis. This app could be used for both Blue team and Red teaming.

## Usage (after you compile it in Visual Studio)

    ./DllComparer.exe ?
    
    Commands Menu:
    -d 
    Dump all the DLL's seen with the count of how many times each was seen.
    
    -s
    Dump all process and show their Dll's

    -e
    Show errors
    
## Features
- View DLL information for running processes for analysis.

## Tip
  You will have to run as admin to see every process info (that the way windows is built).
  
## Adding to your code/Contribute
- Fork and submit pull request

## Credits
- Help with DLL gathering: https://stackoverflow.com/questions/36431220/getting-a-list-of-dlls-currently-loaded-in-a-process-c-sharp

## Disclaimer
Use at your own risk. For educational purposes only.
