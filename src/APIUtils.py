## ELECTRA API UTILITIES ##
# By memescoep, 2024
# https://github.com/MEMESCOEP/Electra


## IMPORTS ##
from PySide6 import QtWidgets, QtCore
from MessageBox import MessageBox
import serial
import pprint
import requests
import os


## CLASSES ##
class APIRequests:


    ## VARIABLES ##
    APICommands = { "Shock": 0, "Vibrate": 1, "Beep": 2, "Info": 3 }
    APICommandsSTR = ["shock", "vibrate", "beep", "info"]
    PiShockAPIKey = "Not configured"
    PiShockShareCode = "Not configured"
    PiShockUsername = "Not configured"
    PiShockAPIURL = "https://do.pishock.com/api/apioperate/"
    SerialShockerIDs = ["Not configured" for f in range(8)]
    SerialHubPath = "/dev/ttyUSB0" #"Not configured"
    SerialHandle = None
    UseSerial = False
    ShowCMD = False


    ## FUNCTIONS ##
    def SendCommand(APICommand = 3, Intensity = 0, Duration = 0):
        if APIRequests.UseSerial == True:
            if APIRequests.VerifyConnection() == False:
                return

            print(f"[{MessageBox.InfoText}] >> Sending JSON to serial device \"{APIRequests.SerialHubPath}\"...")

            # Construct the JSON command that will be sent to the hub
            SerialCommand = bytes(f"{{\"cmd\": \"operate\", \"value\": {{\"id\": {APIRequests.SerialShockerIDs[0]}, \"op\": \"{APIRequests.APICommandsSTR[APICommand]}\", \"duration\": {Duration * 100}, \"intensity\": {Intensity}}}}}\n", "utf-8")
            
            if APIRequests.ShowCMD == True:
                print(f"[{MessageBox.DebugText}] >> Serial command: \"{SerialCommand.decode().replace("\n", "\\n")}\"")

            # Send the JSON command to the hub
            APIRequests.SerialHandle.write(SerialCommand)

        else:
            print(f"[{MessageBox.InfoText}] >> Posting JSON to \"{APIRequests.PiShockAPIURL}\"...")
            CorrectedDuration = min(round(Duration / 10), 15)
            CorrectedDuration = max(1, CorrectedDuration)
            CommandJSON = {"Username": APIRequests.PiShockUsername, "Code": APIRequests.PiShockShareCode, "Apikey": APIRequests.PiShockAPIKey, "Intensity": Intensity, "Duration": CorrectedDuration, "Op": APICommand}
            QtWidgets.QApplication.setOverrideCursor(QtCore.Qt.WaitCursor)

            if APICommand == 3:
                CommandJSON = {"Username": APIRequests.PiShockUsername, "Code": APIRequests.PiShockShareCode, "Apikey": APIRequests.PiShockAPIKey}
            
            if APIRequests.ShowCMD == True:
                print(f"[{MessageBox.DebugText}] >> JSON command: \"{CommandJSON}\"")

            Response = requests.post(APIRequests.PiShockAPIURL, json=CommandJSON)

            QtWidgets.QApplication.restoreOverrideCursor()
            APIRequests.ParseAPIResponse(Response)

    def ParseAPIResponse(Response):
        print(f"[{MessageBox.InfoText}] >> Parsing response...")

        if Response.status_code != 200 or Response.text != "Operation Attempted.":
            PrettifiedResponseText = pprint.pformat(Response.text, compact=True)
            print(f"[{MessageBox.ErrorText}] >> The command did not succeed:\n\tStatus code: {Response.status_code}\n\tResponse text: {PrettifiedResponseText}\n")
            MessageBox.ShowMessage("Electra - Error", f"The command did not succeed:\n • Status code \"{Response.status_code}\"\n • Response text: {PrettifiedResponseText}", MessageBox.MsgTypes["Error"])

        print(f"[{MessageBox.InfoText}] >> Command succeeded (Status code {Response.status_code}).")

    def InitSerial():
        try:
            APIRequests.SerialHandle = serial.Serial(APIRequests.SerialHubPath, 115200, timeout=1)

        except Exception as EX:
            MessageBox.ShowMessage("Electra - Error", f"Failed to open the serial port \"{APIRequests.SerialHubPath}\".\nError: {EX}\n\nPossible solutions:\n • Ensure your USB cable can transmit data\n • Ensure you plug the hub into the computer\n • Ensure your hub is receiving enough power\n  > The hub requires at least 5V & 2A", MessageBox.MsgTypes["Error"])

    def VerifyConnection():
        if os.path.exists(APIRequests.SerialHubPath) == False or APIRequests.SerialHandle == None:
            MessageBox.ShowMessage("Electra - Error", f"No hubs connected via serial were found.\n\nPossible solutions:\n • Ensure your USB cable can transmit data\n • Ensure you plug the hub into the computer\n • Ensure your hub is receiving enough power\n  > The hub requires at least 5V & 2A", MessageBox.MsgTypes["Error"])
            return False

        if APIRequests.SerialHandle.is_open == False:
            MessageBox.ShowMessage("Electra - Error", f"The serial handle is closed.\n\nPossible solutions:\n • Ensure your USB cable can transmit data\n • Ensure you plug the hub into the computer\n • Ensure your hub is receiving enough power\n  > The hub requires at least 5V & 2A", MessageBox.MsgTypes["Error"])
            return False

        return True