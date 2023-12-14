### Electra Compilation Script ###

## IMPORTS ##
from rich import print as rprint
from rich.panel import Panel
import rich.table
import subprocess
import traceback
import platform
import shutil
import os

## VARIABLES ##
OutputPath = "./Electra/bin/Debug/net8.0/"

## FUNCTIONS ##
def BuildFailure(ReturnCode):
    print("\n\n")
    rprint(Panel.fit("[red]🛑 BUILD FAILED! 🛑[/red]", style="bold"))
    rprint(f"[red]Build failed with exit code {ReturnCode}.[/red]")
    exit(ReturnCode)

def StartProcess(Process):
    Child = subprocess.Popen(Process.split())
    Child.wait()
    ReturnCode = Child.poll()
    
    if ReturnCode != 0:
        BuildFailure(ReturnCode)

## MAIN CODE ##
try:
    # Compile the DOTNET portion of Electra
    rprint(Panel.fit("[blue]🔧 COMPILING DOTNET 🔧[/blue]", style="bold"))
    StartProcess("dotnet build")

    # Compile the python portion of Electra
    print("\n\n")
    rprint(Panel.fit("[blue]🔧 COMPILING PYTHON 🔧[/blue]", style="bold"))
    if platform.system() == "Windows":
        StartProcess("pyinstaller --onefile ./Electra/GetCOMName.py")

    else:
        StartProcess("python3 -m PyInstaller --onefile ./Electra/GetCOMName.py")

    # Do some cleanup (move the binary, delete temp files & dirs)
    print("\n\n")
    rprint(Panel.fit("[blue]✨ CLEANING UP ✨[/blue]", style="bold"))

    # Find the compiled python binary
    File = list(filter(lambda x: 'GetCOMName' in x, os.listdir("./dist/")))

    # Move the binary to the debug folder
    if len(File) > 0:
        print("[INFO} >> Moving compiled binary to debug folder...")
        shutil.move(os.path.join("./dist/", File[0]), os.path.join(OutputPath, "GetCOMName.pye"))

    else:
        raise Exception("Could not find the compiled python binary!")

    print("[INFO} >> Removing temporary directories and files...")
    shutil.rmtree("./build")
    shutil.rmtree("./dist")
    os.remove("./GetCOMName.spec")

    print("\n\n")
    rprint(Panel.fit("[green]✅ BUILD SUCCEEDED ✅[/green]", style="bold"))
    
except Exception as EX:
    rprint(f"[red]{traceback.format_exc()}[/red]")
    BuildFailure(EX.args[0])
