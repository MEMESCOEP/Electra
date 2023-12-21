# Building Electra
## Requirements
These requirements can be installed on a Linux or MacOS system by running the `InstallRequirements.sh` script.
In order to build Electra, you'll need the following:

* The source code
* .NET 8 SDK
* Python 3.7 or later

<br/>

## Libraries
Electra uses the following libraries:

C#:
* Newtonsoft.Json
* Raylib-CsLo
* Discord RPC
* SharpHook
* SDL2

Python:
* PyInstaller
* Serial
* Rich

<br/>

## Build Environment
> [!NOTE]
> Python 3.7 or later is required.

Before compiling Electra, you need to set up a build environment. You'll need to intall everything listed in listed under [Requirements](#requirements), and the python libraries listed under [Libraries](#libraries). The python libraries can be installed using the `pip install -r Requirements.txt` command.

<br/>

## Compilation
> [!NOTE]
> The output directory where the final binary and DLLs will be located is `bin/Debug/net8.0/` or `bin/Release/net8.0/`.

### The Easy Way
The easiest way to compile is to use the `Compile.py` script, because it takes care of all build steps for you. All you need to do is run the script.

<br/>

### Visual Studio
> [!NOTE]
> This only compiles the C# portion of Electra. To compile the Python portion, see [Command Line](#command-line).

Open the solution and Press either `F5` to compile & debug, or `CRTL+SHIFT+B` to compile without debugging.

<br/>

### Command Line
The easiest way to compile Electra is by using the compilation scipt called `Compile.py`. Run this file to compile both the C# and Python portions of Electra.

If you don't want to use the build script, enter the project directory and run `dotnet build` to compile the C# portion of Electra.<br/>
Next, run `pyinstaller --onefile GetCOMName.py` to compile the Python portion. Rename the binary in `dist` to `GetCOMName.pye` and copy it to the output folder. Finally, delete the `build` and `dist` folders.

To run the compiled binary, change to the output directory and run `"Electra.exe"` (Win32) or `"Electra"` (Other operating systems).