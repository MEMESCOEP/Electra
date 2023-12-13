### Electra Compilation Script ###

## IMPORTS ##
from rich import print as rprint
from rich.panel import Panel
import rich.table
import subprocess
import traceback

## VARIABLES ##
OutputTable = rich.table.Table(show_header=False)

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
    rprint(Panel.fit("[blue]🔧 COMPILING DOTNET 🔧[/blue]", style="bold"))
    StartProcess("dotnet build")

    print("\n\n")
    rprint(Panel.fit("[blue]🔧 COMPILING PYTHON 🔧[/blue]", style="bold"))
    StartProcess("pyinstaller --onefile ./Electra/GetCOMName.py")

    print("\n\n")
    rprint(Panel.fit("[blue]🔧 CLEANING UP 🔧[/blue]", style="bold"))
    #StartProcess("move ./dist/GetCOMName.exe ./Electra/bin/debug/net8.0/GetCOMName.exe")
    StartProcess("rmdir /Q /s build")
    StartProcess("rmdir /Q /s dist")
    StartProcess("del /Q GetCOMName.spec")

    print("\n\n")
    rprint(Panel.fit("[green]✅ BUILD SUCCEEDED ✅[/green]", style="bold"))
    
except Exception as EX:
    rprint(f"[red]{traceback.format_exc()}[/red]")
    BuildFailure(EX.args[0])
