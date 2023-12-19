### Electra Compilation Script ###

## IMPORTS ##
from rich import print as rprint
from rich.panel import Panel
from datetime import datetime
import rich.table
import subprocess
import traceback
import platform
import shutil
import time
import os

## VARIABLES ##
OutputPath = "./Electra/bin/Debug/net8.0/"
StartTime = datetime.now()

## FUNCTIONS ##
def BuildFailure(ReturnCode):
    print("\n\n")
    rprint(Panel.fit("[red]🛑 BUILD FAILED! 🛑[/red]", style="bold"))
    rprint(f"[red]Build failed: {ReturnCode}.[/red]")

    if str(ReturnCode).isnumeric():
        exit(ReturnCode)

    else:
        exit(-1)

def StartProcess(Process):
    Child = subprocess.Popen(Process.split(' '))
    Child.wait()
    ReturnCode = Child.poll()
    
    if ReturnCode != 0:
        BuildFailure(ReturnCode)

## MAIN CODE ##
try:
    print(f"[INFO] >> Build started at {StartTime}\n\n")
    
    # Compile the DOTNET portion of Electra
    rprint(Panel.fit("[blue]🔧 COMPILING C# 🔧[/blue]", style="bold"))
    print("[INFO] >> Running dotnet...")
    StartProcess("dotnet build")

    # Compile the python portion of Electra, The file extension will be changed to ".pye" for easy cross platform compatibility
    print("\n\n")
    rprint(Panel.fit("[blue]🔧 COMPILING PYTHON 🔧[/blue]", style="bold"))
    if platform.system() == "Windows":
        print("[INFO] >> Running pyinstaller on \"Windows\"...")
        StartProcess("pyinstaller --onefile ./Electra/GetCOMName.py")

    else:
        print(f"[INFO] >> Running pyinstaller on \"{platform.system()}\"")
        StartProcess("python3 -m PyInstaller --onefile ./Electra/GetCOMName.py -n GetCOMName")

    # Do some cleanup (move the binary, delete temp files & dirs)
    print("\n\n")
    rprint(Panel.fit("[blue]✨ CLEANING UP ✨[/blue]", style="bold"))

    # Find and move the binary to the debug folder
    Filename = "GetCOMName.exe"
        
    if platform.system() != "Windows":
        print(f"[INFO] >> Running on \"{platform.system()}\", filename variable will be updated.")
        Filename = "GetCOMName"

    print("[INFO] >> Checking for binary...")
    if not os.path.exists(os.path.join("./dist/", Filename)):
        raise Exception("Could not find the compiled python binary!")
    
    print("[INFO] >> Moving binary to output folder...")
    shutil.move(os.path.join("./dist/", Filename), os.path.join(OutputPath, "GetCOMName.pye"))

    # Remove temporary files and directories
    print("[INFO] >> Removing temporary directories and files...")
    os.remove("./GetCOMName.spec")
    shutil.rmtree("./build")
    shutil.rmtree("./dist")
    print("\n\n")
    rprint(Panel.fit("[green]✅ BUILD SUCCEEDED ✅[/green]", style="bold"))
    print(f"[INFO] >> Build finished.\n\tBuild start time: {StartTime}\n\tBuild finish time: {datetime.now()}\n\tTotal build time: {datetime.now() - StartTime}.")
    
except Exception as EX:
    rprint(f"[red]{traceback.format_exc()}[/red]")
    BuildFailure(str(EX))
