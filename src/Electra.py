## ELECTRA ##
# By memescoep, 2024
# https://github.com/MEMESCOEP/Electra


## IMPORTS ##
from PySide6 import QtWidgets, QtCore
from termcolor import colored
from MessageBox import MessageBox
from DiscordRPC import RPC
from FileUtils import FileUtils
from Settings import Settings
from APIUtils import APIRequests
import threading
import sys

if getattr(sys, 'frozen', False):
    import pyi_splash


## VARIABLES ##
RPCUpdateThread = None
Version = "2.0"
ExitCode = 0


## FUNCTIONS ##
def UpdateSliderLabels(Window, UpdateRPC=True):
    Window.IntensityBarLabel.setText(f"Intensity ({Window.IntensitySlider.value()}%):")
    Window.DurationBarLabel.setText(f"Duration ({round((Window.DurationSlider.value() / 150) * 15, 2)}s):")
    
    if UpdateRPC == True and RPC.EnableRPC == True:
        RPC.RPCValues = GetIntensityDuration(Window)
        RPC.Update = True

def GetIntensityDuration(Window):
    return [Window.IntensitySlider.value(), Window.DurationSlider.value() / 10]


## MAIN CODE ##
if __name__ == "__main__":
    try:
        print(f"◄{'─' * 14} {colored('ELECTRA', 'light_yellow')} {'─' * 15}►")
        print(f"    Electra {Version} by memescoep, 2024\n  https://github.com/MEMESCOEP/Electra\n\n\n\n")

        print(f"◄{'─' * 14} {colored('CONSOLE', 'light_grey')} {'─' * 15}►")
        if hasattr(sys, '_MEIPASS') == True:
            print(f"[{MessageBox.InfoText}] >> Module \"sys._MEIPASS\" contains: \"{sys._MEIPASS}\"")
            os.chdir(sys._MEIPASS)

        else:
            print(f"[{MessageBox.InfoText}] >> Module \"sys\" doesn't have an \"_MEIPASS\" property, which probably means this isn't compiled.")

        print(f"[{MessageBox.InfoText}] >> Getting current platform...")
        Platform = sys.platform

        print(f"[{MessageBox.InfoText}] >> Current platform: \"{Platform}\"")

        # Initialize QT
        print(f"[{MessageBox.InfoText}] >> Initializing QT app...")
        App = QtWidgets.QApplication(sys.argv)
        MainWindow = FileUtils.LoadUIFile("UIFiles/MainForm.ui")

        # Initialize settings
        if getattr(sys, 'frozen', False):
            pyi_splash.close()
        
        MessageBox.ParentWindow = MainWindow
        Settings.Init()

        if APIRequests.UseSerial == True:
            APIRequests.InitSerial()

        if RPC.EnableRPC == True:
            RPC.InitRPC()

        # Assign actions to controls
        print(f"[{MessageBox.InfoText}] >> Assigning actions to controls...")
        MainWindow.ShockButton.clicked.connect(lambda: APIRequests.SendCommand(APIRequests.APICommands["Shock"], MainWindow.IntensitySlider.value(), MainWindow.DurationSlider.value()))
        MainWindow.VibrateButton.clicked.connect(lambda: APIRequests.SendCommand(APIRequests.APICommands["Vibrate"], MainWindow.IntensitySlider.value(), MainWindow.DurationSlider.value()))
        MainWindow.BeepButton.clicked.connect(lambda: APIRequests.SendCommand(APIRequests.APICommands["Beep"], Duration=MainWindow.DurationSlider.value()))
        MainWindow.SettingsButton.clicked.connect(lambda: Settings.ShowSettingsManager(MainWindowIcon))
        MainWindow.APISerialInfoButton.clicked.connect(lambda: Settings.DisplayConfigInfo(MainWindowIcon))
        MainWindow.ConfigureAction.triggered.connect(MainWindow.close)
        MainWindow.QuitAction.triggered.connect(MainWindow.close)
        MainWindow.IntensitySlider.valueChanged.connect(lambda: UpdateSliderLabels(MainWindow))
        MainWindow.DurationSlider.valueChanged.connect(lambda: UpdateSliderLabels(MainWindow))
        Settings.SettingsManager.buttonBox.button(QtWidgets.QDialogButtonBox.Apply).clicked.connect(Settings.ApplySettings)
        Settings.SettingsManager.buttonBox.button(QtWidgets.QDialogButtonBox.Discard).clicked.connect(Settings.DiscardChanges)
        Settings.SettingsManager.buttonBox.button(QtWidgets.QDialogButtonBox.RestoreDefaults).clicked.connect(lambda: Settings.PopulateSettings(True))

        #print(f"[{MessageBox.InfoText}] >> Setting title...")
        #MainWindow.setWindowTitle("Electra (Online API)" if APIRequests.UseSerial == False else "Electra (Serial)")

        # Load images form the disk
        print(f"[{MessageBox.InfoText}] >> Loading pixmaps...")
        MainWindowIcon = FileUtils.LoadIcon("Images/Icon.png")
        MainWindow.IntensityIcon.setPixmap(FileUtils.LoadPixmap("Images/Intensity.png"))
        MainWindow.DurationIcon.setPixmap(FileUtils.LoadPixmap("Images/Duration.png"))

        # Configure the main form
        print(f"[{MessageBox.InfoText}] >> Setting main form properties...")
        App.setDesktopFileName("Electra") # Can fix the taskbar icon not showing on some wayland systems
        MainWindow.setWindowIcon(MainWindowIcon)
        MainWindow.setWindowFlags(QtCore.Qt.WindowCloseButtonHint | QtCore.Qt.WindowMinimizeButtonHint)
        UpdateSliderLabels(MainWindow, False)
        MainWindow.show()

        print(f"[{MessageBox.InfoText}] >> Checking for first time init...")
        
        if Settings.SetupOnBoot == True:
            print(f"[{MessageBox.InfoText}] >> Entering first time setup...")
            Settings.ShowSettingsManager(MainWindowIcon)

        print(f"[{MessageBox.InfoText}] >> Starting threads...")
        RPCUpdateThread = threading.Thread(target=RPC.UpdateRPC, args=(True,))
        RPCUpdateThread.start()

        print(f"[{MessageBox.InfoText}] >> Init finished.")
        ExitCode = App.exec()

    except Exception as EX:
        print(f"[{MessageBox.ErrorText}] >> Initialization failed: {EX}")
        MessageBox.ShowMessage("Electra - Error", f"Initialization failed!\n\nError: {EX}\nTraceback: {EX.with_traceback}", MessageBox.MsgTypes["Error"])

    print(f"[{MessageBox.InfoText}] >> Killing threads...")
    RPC.KillRPCUpdate = True

    if RPCUpdateThread != None:
        RPCUpdateThread.join()

    RPC.StopRPC()

    if APIRequests.UseSerial == True:
        if APIRequests.SerialHandle != None and APIRequests.SerialHandle.is_open == True:
            print(f"[{MessageBox.InfoText}] >> Closing serial port \"{APIRequests.SerialHubPath}\"...")
            APIRequests.SerialHandle.close()

    print(f"[{MessageBox.InfoText}] >> Exiting with code {ExitCode}...")
    sys.exit(ExitCode)
