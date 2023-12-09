# Building PiShockDesktop
## Requirements
In order to build PiShock Desktop, you'll need the following:
* .NET 8 SDK
* The PiShock Desktop source code

<br/>

PiShock Desktop uses the following libraries:
* Raylib-CsLo
* Newtonsoft.Json

<br/>

## Compilation
### Output
The output directory where the final binary and DLLs will be located is `bin/Debug/net8.0/` or `bin/Release/net8.0/`.

<br/>

### Visual Studio
Open the solution and Press either `F5` to compile & debug, or `CRTL+SHIFT+B` to compile without debugging.

<br/>

### Command Line
Enter the porject directory and run `dotnet build` to compile. To run the compiled binary, change to the output directory and run `"PiShock Desktop.exe"`.
