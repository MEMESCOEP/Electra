## ELECTRA COMPILATION SCRIPT ##
# By memescoep, 2024
# https://github.com/MEMESCOEP/Electra


## IMPORTS ##
from rich.table import Table as RTable
from rich import print as RPrint
from rich.panel import Panel
from datetime import datetime
import subprocess
import traceback
import platform
import shutil
import sys
import os




## VARIABLES ##
EnableConsoleInBuild = False
SkipCleanup = False
NoSplash = False
UseUPX = False
BuildStartTime = datetime.now()
SplashIMGPath = "./src/Images/Splash.png"
AppIconPath = "./src/Images/Icon.png"
OutputPath = "./bin/"
UPXPath = r""
PyInstallerOptions = [
        './src/Electra.py',
        f'--distpath {OutputPath}',
        '--onefile',
        '--clean',
        '--noconfirm',
        '--name Electra',
        f'--icon {AppIconPath}',
        '--add-data=src/UIFiles/*.ui:UIFiles',
        '--add-data=src/ResourceFiles/*.qrc:ResourceFiles',
        '--add-data=src/Images/*:Images',
]




## FUNCTIONS ##
def BuildFailure(ReturnCode):
    print("\n\n")
    RPrint(Panel.fit("[red]🛑 BUILD FAILED! 🛑[/red]", style="bold"))
    RPrint(f"[red]Build failed: {ReturnCode}[/red]")

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


def ShowDetails():
    DetailsTable = RTable(title="Electra", style="magenta")

    DetailsTable.add_column("Property", style="cyan", no_wrap=True)
    DetailsTable.add_column("Description", style="yellow")

    DetailsTable.add_row("Author", "memescoep")
    DetailsTable.add_row("Github", "https://github.com/MEMESCOEP/Electra")
    DetailsTable.add_row("Python", "3.10 & later")
    DetailsTable.add_row("License", "MIT License (https://opensource.org/license/mit/)")    

    RPrint(DetailsTable)


def ShowHelp():
    ShowDetails()
    print("\n\n\n")

    CommandTable = RTable(title="Compiler", style="magenta")
    ExtrasTable = RTable(title="\n\n\nExtras", style="magenta")
    OSTable = RTable(title="\n\n\nSupported Operating Systems", style="magenta")
    
    CommandTable.add_column("Command", style="cyan", no_wrap=True)
    CommandTable.add_column("Description", style="yellow")
    ExtrasTable.add_column("Notes", style="cyan", no_wrap=True)    
    OSTable.add_column("OS", style="cyan", no_wrap=True)
    OSTable.add_column("Version", style="yellow")

    CommandTable.add_row("--help", "Shows help")
    CommandTable.add_row("--verbose", "Enables verbosity when compiling")
    CommandTable.add_row("--debug", "Enables debug mode when compiling [italic bold red]!! EXPERIMENTAL !![/italic bold red]")
    CommandTable.add_row("--enable-console", "Creates a console (useful for debugging)")
    CommandTable.add_row("--upx <upx dir>", "Uses UPX to compress the binary (useful for decreasing its size) [italic bold red]!! EXPERIMENTAL !![/italic bold red]")
    CommandTable.add_row("--skip-cleanup", "Skips clean up steps after compiling (useful for debugging)")
    CommandTable.add_row("--no-splash", "Skips adding the splash screen image")
    ExtrasTable.add_row("Some antiviruses may falsely flag the binary. You can choose to keep it, or directly run Electra.py.")
    ExtrasTable.add_row("Only python 3.10 and later are currently supported. Please use one of these versions.")
    OSTable.add_row("Microsoft Windows", "Windows 10 and later")
    OSTable.add_row("GNU/Linux", "Distros with QT support")
    OSTable.add_row("Apple MacOS", "Unknown")
    
    RPrint(CommandTable)
    RPrint(OSTable)
    RPrint(ExtrasTable)




## MAIN CODE ##
try:
    RPrint(Panel.fit("[yellow]⚡ ELECTRA ⚡[/yellow]", style="bold"))

    # Handle command line arguments
    SkipArgument = False

    for Arg in sys.argv[1:]:
        if SkipArgument:
            SkipArgument = False
            continue

        if "--help" in sys.argv:
            ShowHelp()
            sys.exit(0)

        match Arg:
            case "--enable-console":
                print(f"[INFO] >> The application will be compiled with console support.")
                EnableConsoleInBuild = True

            case "--debug":
                print(f"[INFO] >> Compilation will be run in debug mode.")
                PyInstallerOptions.append('--debug all')

            case "--verbose":
                print(f"[INFO] >> verbosity enabled.")
                PyInstallerOptions.append('--log-level DEBUG')

            case "--upx":
                if len(sys.argv) < (sys.argv.index(Arg) + 2):
                    raise KeyError("The path of the UPX firectory needs to be specified")
                
                if os.path.isdir(sys.argv[sys.argv.index(Arg) + 1]) == False:
                    raise NotADirectoryError("The specified UPX path is not a directory, or it doesn't exist")
                
                print(f"[INFO] >> The binary will be compressed with UPX.")
                SkipArgument = True
                UPXPath = sys.argv[sys.argv.index(Arg) + 1].replace("'", "").replace("\"", "")
                UseUPX = True

            case "--skip-cleanup":
                print(f"[INFO] >> Cleanup steps will be skipped.")
                SkipCleanup = True

            case "--no-splash":
                print(f"[INFO] >> The splash screen will no be added.")
                NoSplash = True

            case _:
                ShowHelp()
                raise Exception(f"The argument \"{Arg}\" is unknown.")

    if EnableConsoleInBuild == False:
        PyInstallerOptions.append('--noconsole')

    if NoSplash == False:        
        PyInstallerOptions.append(f'--splash {SplashIMGPath}')

    if UseUPX == True:
        PyInstallerOptions.append(f'--upx-dir {UPXPath}')


    # Compile Electra
    print(f"[INFO] >> Build started at {BuildStartTime}\n\n")
    RPrint(Panel.fit("[blue]🔧 COMPILING ELECTRA 🔧[/blue]", style="bold"))
    
    if shutil.which("pyinstaller") == None:
        print("[WARN] >> The pyinstaller executable was not found, trying to proceed with the module command...")
        StartProcess(f"python -m PyInstaller {' '.join(PyInstallerOptions)}")

    else:
        StartProcess(f"pyinstaller {' '.join(PyInstallerOptions)}")

    
    # Do some cleanup (move the binary, delete temp files & dirs)
    print("\n\n")
    RPrint(Panel.fit("[blue]✨ CLEANING UP ✨[/blue]", style="bold"))
    

    # Remove temporary files and directories
    if SkipCleanup:
        print(f"[INFO] >> Cleanup steps have been skipped.")

    else:
        print("[INFO] >> Removing temporary directories and files...")
        shutil.rmtree("build")
        os.remove("Electra.spec")

    print("\n\n")


    # The build is complete
    RPrint(Panel.fit("[green]✅ BUILD SUCCEEDED ✅[/green]", style="bold"))
    print(f"[INFO] >> Build finished.\n\tBuild start time: {BuildStartTime}\n\tBuild finish time: {datetime.now()}\n\tTotal build time: {datetime.now() - BuildStartTime}.")
    
except Exception as EX:
    RPrint(f"[red]{traceback.format_exc()}[/red]")
    BuildFailure(str(EX))
