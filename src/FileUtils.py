 ## ELECTRA FILE UTILS ##
# By memescoep, 2024
# https://github.com/MEMESCOEP/Electra


## IMPORTS ##
from PySide6.QtUiTools import QUiLoader
from PySide6 import QtCore, QtGui
from MessageBox import MessageBox
import os


## CLASSES ##
class FileUtils:


    ## VARIABLES ##
    MissingIcon = QtGui.QIcon.fromTheme("dialog-warning")
    UILoader = QUiLoader()
    ShowIMGInfo = False


    ## FUNCTIONS ##
    def LoadUIFile(Path):
        print(f"[{MessageBox.InfoText}] >> Loading UI file \"{Path}\"...")
        return FileUtils.UILoader.load(Path, None)

    def LoadIcon(Path):
        print(f"[{MessageBox.InfoText}] >> Loading icon \"{Path}\"...")

        if os.path.exists(Path) == False:
            print(f"[{MessageBox.ErrorText}] >> The icon file \"{Path}\" could not be found!")
            MessageBox.ShowMessage("Electra - Error", f"The icon file \"{Path}\" could not be found!", MessageBox.MsgTypes["Error"])
            return FileUtils.MissingIcon

        Icon = QtGui.QIcon(Path)

        if FileUtils.ShowIMGInfo == True:
            Pixmap = Icon.pixmap(Icon.availableSizes()[0])
            print(f"[{MessageBox.DebugText}] >> QPixmap size: {Pixmap.width()}x{Pixmap.height()}")
            print(f"[{MessageBox.DebugText}] >> QPixmap depth: {Pixmap.depth()}")
            print(f"[{MessageBox.DebugText}] >> QPixmap has alpha: {"yes" if Pixmap.hasAlpha() == True else "no"}")
            print(f"[{MessageBox.DebugText}] >> QPixmap DPI: {Pixmap.logicalDpiX()}, {Pixmap.logicalDpiY()}")
            print(f"[{MessageBox.DebugText}] >> QPixmap color count: {Pixmap.colorCount()}")

        return Icon

    def LoadPixmap(Path):
        print(f"[{MessageBox.InfoText}] >> Loading pixmap \"{Path}\"...")

        if os.path.exists(Path) == False:
            print(f"[{MessageBox.ErrorText}] >> The pixmap file \"{Path}\" could not be found!")
            MessageBox.ShowMessage("Electra - Error", f"The pixmap file \"{Path}\" could not be found!", MessageBox.MsgTypes["Error"])
            return FileUtils.MissingIcon.pixmap(QtCore.QSize(16, 16))
        
        Pixmap = QtGui.QPixmap(Path)

        if FileUtils.ShowIMGInfo == True:
            print(f"[{MessageBox.DebugText}] >> QPixmap size: {Pixmap.width()}x{Pixmap.height()}")
            print(f"[{MessageBox.DebugText}] >> QPixmap depth: {Pixmap.depth()}")
            print(f"[{MessageBox.DebugText}] >> QPixmap has alpha: {"yes" if Pixmap.hasAlpha() == True else "no"}")
            print(f"[{MessageBox.DebugText}] >> QPixmap DPI: {Pixmap.logicalDpiX()}, {Pixmap.logicalDpiY()}")
            print(f"[{MessageBox.DebugText}] >> QPixmap color count: {Pixmap.colorCount()}")

        return Pixmap