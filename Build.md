# Building Electra
## Requirements
> [!NOTE]
> Linux hosts may require the `patchelf` package.

In order to build Electra, you'll need to aquire the following:

* The source code
* Python 3.10 or later

<br/>

## Libraries
Electra uses the following libraries to function:
* PyInstaller
* PrettyPrint
* PyPresence
* PySerial
* PySide6
* Pillow
* Requests
* Rich

<br/>

## Build Environment
> [!NOTE]
> Python 3.10 or later is required. This is due to the use of the `Match-Case` statement in `Compile.py`, which was not yet implemented in previous versions.

Before compiling Electra, you will have to set up a build environment. Install everything listed in listed under [Requirements](#requirements) and [Libraries](#libraries). The libraries can easily be installed using the `pip install -r Requirements.txt` command.

<br/>

## Compilation
> [!NOTE]
> The output directory where the final binary will be located is `bin/`.

### The Easy Way
The easiest way to compile is to use the `Compile.py` script, because it takes care of all build steps for you.

<br/>

### The hard way
Compiling Electra consists of the following steps:
* 
