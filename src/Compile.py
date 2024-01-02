### Electra Compilation Script ###

## IMPORTS ##
from rich.panel import Panel as RPanel
from rich import print as RPrint
from datetime import datetime
import subprocess
import traceback
import platform
import shutil
import os

## VARIABLES ##
PyInstallerBuildFolder = "./build/"
PyInstallerDistFolder = "./dist/"
OutputPath = "./Electra/bin/Build/"
StartTime = datetime.now()
CWD = os.getcwd()

## FUNCTIONS ##
# Called if/when the build fails
def BuildFailure(ReturnCode):
    print("\n\n")
    RPrint(RPanel.fit("[red]🛑 BUILD FAILED! 🛑[/red]", style="bold"))
    RPrint(f"[red]Build failed: {ReturnCode}.[/red]")

    if str(ReturnCode).isnumeric():
        exit(ReturnCode)

    else:
        exit(-1)

# Start a process and wait for its return code
def StartProcess(Process):
    Child = subprocess.Popen(Process.split(' '))
    Child.wait()
    ReturnCode = Child.poll()
    
    if ReturnCode != 0:
        BuildFailure(ReturnCode)

## MAIN CODE ##
try:
    print(f"[INFO] >> Build started at {StartTime}")
    print(f"[INFO] >> Current working directory is: {CWD}\n\n")

    if os.path.exists(OutputPath):
        RPrint(RPanel.fit("[blue]✨ CLEANING BUILD DIRECTORY ✨[/blue]", style="bold"))        
        os.chdir(os.path.abspath(OutputPath))

        for Item in os.listdir('.'):
            if os.path.basename(Item) == "Assets" and os.path.isdir(Item):
                continue
            
            if os.path.isdir(Item):
                print(f"[INFO] >> Removing directory \"{Item}\"...")
                shutil.rmtree(Item)
        
            else:
                print(f"[INFO] >> Removing file \"{Item}\"...")
                os.remove(Item)

        os.chdir(CWD)
    
    # Compile the DOTNET portion of Electra
    print("\n\n")
    RPrint(RPanel.fit("[blue]🔧 COMPILING C# 🔧[/blue]", style="bold"))
    print("[INFO] >> Running dotnet...")
    StartProcess(f"dotnet build -o {OutputPath} -v normal")

    # Compile the PYTHON portion of Electra. The file extension will be changed to ".pye" for easy cross platform compatibility
    print("\n\n")
    RPrint(RPanel.fit("[blue]🔧 COMPILING PYTHON 🔧[/blue]", style="bold"))
    print(f"[INFO] >> Running pyinstaller on \"{platform.system()}\"")

    # Run the correct python executable based on the OS
    if platform.system() == "Windows":
        StartProcess("pyinstaller --onefile ./Electra/GetCOMName.py")

    else:
        StartProcess("python3 -m PyInstaller --onefile ./Electra/GetCOMName.py")

    # Do some cleanup (move the binary, delete temp files & dirs)
    print("\n\n")
    RPrint(RPanel.fit("[blue]✨ CLEANING UP ✨[/blue]", style="bold"))

    # Find and move the binary to the output folder
    Filename = "GetCOMName.exe"
        
    if platform.system() != "Windows":
        print(f"[INFO] >> Running on \"{platform.system()}\", filename variable will be updated.")
        Filename = "GetCOMName"

    print("[INFO] >> Checking for binary...")
    if not os.path.exists(os.path.join(PyInstallerDistFolder, Filename)):
        raise Exception("Could not find the compiled python binary!")
    
    print("[INFO] >> Moving binary to output folder...")
    shutil.move(os.path.join(PyInstallerDistFolder, Filename), os.path.join(OutputPath, "GetCOMName.pye"))

    # Remove temporary files and directories
    print("[INFO] >> Removing temporary directories and files...")
    os.remove("./GetCOMName.spec")
    shutil.rmtree(PyInstallerBuildFolder)
    shutil.rmtree(PyInstallerDistFolder)

    print("\n\n")
    RPrint(RPanel.fit("[green]✅ BUILD SUCCEEDED ✅[/green]", style="bold"))
    print(f"[INFO] >> Build finished.\n\tBuild start time: {StartTime}\n\tBuild finish time: {datetime.now()}\n\tTotal build time: {datetime.now() - StartTime}")
    
except Exception as EX:
    RPrint(f"[red]{traceback.format_exc()}[/red]")
    BuildFailure(str(EX))
