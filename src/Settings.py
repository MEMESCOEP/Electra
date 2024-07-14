## ELECTRA SETTINGS ##
# By memescoep, 2024
# https://github.com/MEMESCOEP/Electra


## IMPORTS ##
from PySide6 import QtWidgets, QtGui
from configparser import ConfigParser
from MessageBox import MessageBox
from APIUtils import APIRequests
from FileUtils import FileUtils
from DiscordRPC import RPC
import serial
import os


## CLASSES ##
class Settings:


    ## VARIABLES ##
    RequiredConfigSections = ["API", "RPC", "SERIAL", "DEBUG"]
    RequiredConfigOptions = ["PiShock_API_Key", "Username", "Share_Code", "Enable_Serial", "Enable_RPC", "Show_CMD", "Show_Icon_Pixmap_Info"]
    ConfigFileOptions = [False, False, False, False, False]
    CFGParser = ConfigParser()
    ConfigFilePath = os.path.abspath("ElectraConfig.ini")
    SettingsManager = None
    ConfigDialog = None
    SetupOnBoot = False
    LoadedUIFiles = False

    ## FUNCTIONS ##
    def Init():
        print(f"[{MessageBox.InfoText}] >> Initializing settings...")
        if Settings.LoadedUIFiles == False:
            Settings.SettingsManager = FileUtils.LoadUIFile("UIFiles/Settings.ui")
            Settings.ConfigDialog = FileUtils.LoadUIFile("UIFiles/ConfigInfo.ui")
            Settings.LoadedUIFiles = True

        if os.path.exists(Settings.ConfigFilePath) == False:
            print(f"[{MessageBox.WarnText}] >> Config file wasn't found!")
            QButtons = MessageBox.ButtonTypes["Yes"] | MessageBox.ButtonTypes["No"]
            SetupButton = MessageBox.ButtonTypes["Yes"]
            Settings.SetupOnBoot = MessageBox.ShowMessage("Electra - Setup", "Welcome to Electra!\n\nSince this is your first time, would you like to enter setup?", MessageBox.MsgTypes["Question"], QButtons, SetupButton) == SetupButton
                
        else:
            print(f"[{MessageBox.InfoText}] >> Config file found.")
            Settings.LoadSettingsFromFile(Settings.ConfigFilePath)

    def LoadSettingsFromFile(CFGPath):
        print(f"[{MessageBox.InfoText}] >> Loading configuration from file: \"{CFGPath}\"...")
        Settings.CFGParser.read(CFGPath)
        RPC.EnableRPC = Settings.CFGParser.get("RPC", "Enable_RPC") == "yes"
        APIRequests.PiShockAPIKey = Settings.CFGParser.get("API", "PiShock_API_Key")
        APIRequests.PiShockUsername = Settings.CFGParser.get("API", "Username")
        APIRequests.PiShockShareCode = Settings.CFGParser.get("API", "Share_Code")
        APIRequests.UseSerial = Settings.CFGParser.get("SERIAL", "Enable_Serial") == "yes"
        APIRequests.ShowCMD = Settings.CFGParser.get("DEBUG", "Show_CMD") == "yes"
        FileUtils.ShowIMGInfo = Settings.CFGParser.get("DEBUG", "Show_Icon_Pixmap_Info") == "yes"

        # Store each shocker's ID
        for SerialIDIndex in range(1, 9):
            ShockerID = f"Shocker_ID_{SerialIDIndex}"

            if Settings.CFGParser.has_option("SERIAL", ShockerID) == False:
                continue

            APIRequests.SerialShockerIDs[SerialIDIndex - 1] = Settings.CFGParser.get("SERIAL", ShockerID)

    def DisplayConfigInfo(WindowIcon: QtGui.QIcon):
        InfoText = f"""[===== API =====]
API Key(s):
    • PiShock API key: {APIRequests.PiShockAPIKey}

Credentials:
    • PiShock username: {APIRequests.PiShockUsername}
    • PiShock share code: {APIRequests.PiShockShareCode}



[===== SERIAL =====]
Serial enabled: {APIRequests.UseSerial}
Hub path: {APIRequests.SerialHubPath}
Shockers:"""

        for Index in range(1, 9):
            InfoText += f"\n    • Shocker ID ({Index}): {APIRequests.SerialShockerIDs[Index - 1]}"

        Settings.ConfigDialog.InfoTextbox.setText(InfoText)
        Settings.ConfigDialog.setWindowIcon(WindowIcon)
        Settings.ConfigDialog.exec()

    def ApplySettings():
        print(f"[{MessageBox.InfoText}] >> Saving settings to config file \"{Settings.ConfigFilePath}\"...")
        for Section in Settings.RequiredConfigSections:
            if Settings.CFGParser.has_section(Section) == False:
                Settings.CFGParser.add_section(Section)

        Settings.CFGParser.set("API", "PiShock_API_Key", Settings.SettingsManager.APIKeyEntry.text())
        Settings.CFGParser.set("API", "Share_Code", Settings.SettingsManager.ShareCodeEntry.text())
        Settings.CFGParser.set("API", "Username", Settings.SettingsManager.UsernameEntry.text())
        Settings.CFGParser.set("RPC", "Enable_RPC", "yes" if Settings.SettingsManager.EnableRPCCheckBox.isChecked() == True else "no")
        Settings.CFGParser.set("RPC", "Enable_External_RPC_Control", "yes" if Settings.SettingsManager.EnableRPCButtonsCheckBox.isChecked() == True else "no")
        Settings.CFGParser.set("SERIAL", "Enable_Serial", "yes" if Settings.SettingsManager.EnableSerialCheckBox.isChecked() == True else "no")

        for ShockerID in range(1, 9):
            IDInTable = Settings.SettingsManager.IDsTable.item(ShockerID - 1, 0).text()
            Settings.CFGParser.set("SERIAL", f"Shocker_ID_{ShockerID}", IDInTable)

        Settings.CFGParser.set("DEBUG", "Show_CMD", "yes" if Settings.SettingsManager.ShowCMDCheckBox.isChecked() == True else "no")
        Settings.CFGParser.set("DEBUG", "Show_Icon_Pixmap_Info", "yes" if Settings.SettingsManager.ShowIconPixmapCheckBox.isChecked() == True else "no")

        with open(Settings.ConfigFilePath, 'w') as CFGFile:
            Settings.CFGParser.write(CFGFile)

        Settings.Init()
        Settings.SettingsManager.close()

    def DiscardChanges():
        QButtons = MessageBox.ButtonTypes["Yes"] | MessageBox.ButtonTypes["No"]
        DiscardButton = MessageBox.ButtonTypes["Yes"]

        if MessageBox.ShowMessage("Electra - Warning", "Are you sure you want to discard your changes?", MessageBox.MsgTypes["Warning"], QButtons, DiscardButton) == DiscardButton:
            Settings.SettingsManager.close()

    def PopulateSettings(RestoreDefaults = False):
        if RestoreDefaults == True:
            QButtons = MessageBox.ButtonTypes["Yes"] | MessageBox.ButtonTypes["No"]
            ResetButton = MessageBox.ButtonTypes["Yes"]

            if MessageBox.ShowMessage("Electra - Warning", "Are you sure you want to restore the default settings?", MessageBox.MsgTypes["Warning"], QButtons, ResetButton) != ResetButton:
                return

            print(f"[{MessageBox.InfoText}] >> Restoring default settings...")
            Settings.SettingsManager.APIKeyEntry.setText("")
            Settings.SettingsManager.ShareCodeEntry.setText("")
            Settings.SettingsManager.UsernameEntry.setText("")
            Settings.SettingsManager.EnableSerialCheckBox.setChecked(False)
            Settings.SettingsManager.EnableRPCCheckBox.setChecked(False)
            Settings.SettingsManager.EnableRPCButtonsCheckBox.setChecked(False)
            Settings.SettingsManager.ShowCMDCheckBox.setChecked(False)
            Settings.SettingsManager.ShowIconPixmapCheckBox.setChecked(False)
            
            for ShockerID in range(8):
                Settings.SettingsManager.IDsTable.item(ShockerID, 0).setText("Not configured")

        else:
            print(f"[{MessageBox.InfoText}] >> Populating settings...")
            Settings.SettingsManager.APIKeyEntry.setText(APIRequests.PiShockAPIKey)
            Settings.SettingsManager.ShareCodeEntry.setText(APIRequests.PiShockShareCode)
            Settings.SettingsManager.UsernameEntry.setText(APIRequests.PiShockUsername)
            Settings.SettingsManager.EnableSerialCheckBox.setChecked(APIRequests.UseSerial == True)
            Settings.SettingsManager.EnableRPCCheckBox.setChecked(RPC.EnableRPC == True)
            Settings.SettingsManager.EnableRPCButtonsCheckBox.setChecked(RPC.RPCShockEnabled == True)
            Settings.SettingsManager.ShowCMDCheckBox.setChecked(APIRequests.ShowCMD == True)
            Settings.SettingsManager.ShowIconPixmapCheckBox.setChecked(FileUtils.ShowIMGInfo == True)
            
            for ShockerID in range(8):
                Settings.SettingsManager.IDsTable.item(ShockerID, 0).setText(APIRequests.SerialShockerIDs[ShockerID])

    def ShowSettingsManager(WindowIcon: QtGui.QIcon):
        Settings.PopulateSettings()
        Settings.SettingsManager.setWindowIcon(WindowIcon)
        Settings.SettingsManager.exec()