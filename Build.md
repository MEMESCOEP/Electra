# Building Electra
## Requirements
In order to build Electra, you'll need the following:
* .NET 8 SDK
* The Electra source code

<br/>

Electra uses the following libraries:
* Newtonsoft.Json
* Raylib-CsLo
* Discord RPC
* SharpHook

<br/>

## Compilation
> [!NOTE]
> The output directory where the final binary and DLLs will be located is `bin/Debug/net8.0/` or `bin/Release/net8.0/`.

### Visual Studio
Open the solution and Press either `F5` to compile & debug, or `CRTL+SHIFT+B` to compile without debugging.

<br/>

### Command Line
Enter the porject directory and run `dotnet build` to compile. To run the compiled binary, change to the output directory and run `"Electra.exe"`.
