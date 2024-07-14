## ELECTRA MESSAGEBOX NOTIFIER ##
# By memescoep, 2024
# https://github.com/MEMESCOEP/Electra


## IMPORTS ##
from PySide6.QtWidgets import QMessageBox
from termcolor import colored


## CLASSES ##
class MessageBox:


    ## VARIABLES ##
    DebugText = colored("DEBUG", "magenta", attrs=["bold", "underline"])
    ErrorText = colored("ERROR", "red", attrs=["blink", "bold"])
    WarnText = colored("WARN", "light_yellow", attrs=["bold"])
    InfoText = colored("INFO", "green")
    ParentWindow = None
    ButtonTypes = {"Yes": QMessageBox.Yes, "No": QMessageBox.No, "Ok": QMessageBox.Ok, "Cancel": QMessageBox.Cancel, "Close": QMessageBox.Close, "Reset": QMessageBox.Reset, "Discard": QMessageBox.Discard, "Apply": QMessageBox.Apply, "RestoreDefaults": QMessageBox.RestoreDefaults, "Save": QMessageBox.Save}
    MsgTypes = {"Info": QMessageBox.information, "Question": QMessageBox.question, "Warning": QMessageBox.warning, "Error": QMessageBox.critical}

    ## FUNCTIONS ##
    def ShowMessage(Title, Message, MsgType=QMessageBox.information, Buttons=QMessageBox.StandardButton.Ok, DefaultButton=QMessageBox.StandardButton.NoButton):
        return MsgType(MessageBox.ParentWindow, Title, Message, Buttons, DefaultButton)