## ELECTRA DISCORD RPC ##
# By memescoep, 2024
# https://github.com/MEMESCOEP/Electra


## IMPORTS ##
from PySide6 import QtWidgets, QtCore
from MessageBox import MessageBox
from pypresence import Presence, PipeClosed
from datetime import datetime
import time
import os


## CLASSES ##
class RPC:


    ## VARIABLES ##
    DiscordAppID = 1258271086659899493
    InitTime = 0
    RPCValues = [1, 0.1]
    RPCButtons = [{"label": "hhh", "url": "https://www.google.com"}, {"label": "GUH!!!", "url": "https://www.youtube.com"}]
    RPCManager = None
    RPCShockEnabled = True
    KillRPCUpdate = False
    Update = False
    EnableRPC = False


    ## FUNCTIONS ##
    def InitRPC():
        try:
            print(f"[{MessageBox.InfoText}] >> Initializing DiscordRPC...")
            RPC.InitTime = int(time.time())
            RPC.RPCManager = Presence(RPC.DiscordAppID)
            RPC.RPCManager.connect()
            RPC.UpdateRPC()

        except Exception as EX:
            print(f"[{MessageBox.ErrorText}] >> Failed to initialize DiscordRPC: {EX}")
            MessageBox.ShowMessage("Electra - Error", f"Failed to connect to Discord.\nError: {EX}\n\nPossible solutions:\n • Ensure Discord is running", MessageBox.MsgTypes["Error"])
            RPC.KillRPCUpdate = True

    def UpdateRPC(Loop = False):
        try:
            print(f"[{MessageBox.InfoText}] >> Updating DiscordRPC (Threaded loop = {Loop})...")
            CurrentRPCResponse = None

            while RPC.EnableRPC == True:
                time.sleep(0.1)

                if RPC.KillRPCUpdate == True:
                    break

                if RPC.Update == False and Loop == True or RPC.RPCManager == None:
                    continue
                
                if RPC.RPCShockEnabled:
                    CurrentRPCResponse = RPC.RPCManager.update(state=f"Duration: {RPC.RPCValues[1]}s", details=f"Intensity: {RPC.RPCValues[0]}%", large_image="icon", buttons=RPC.RPCButtons, start=RPC.InitTime, pid=os.getpid())

                else:
                    CurrentRPCResponse = RPC.RPCManager.update(state=f"Duration: {RPC.RPCValues[1]}s", details=f"Intensity: {RPC.RPCValues[0]}%", large_image="icon", start=RPC.InitTime, pid=os.getpid())

                RPC.Update = False

                if Loop == False:
                    break
        except PipeClosed:
            print(f"[{MessageBox.WarnText}] >> Discord RPC status couldn't be updated because the pipe was closed. RPC will be disabled.")
            RPC.EnableRPC = False

        except Exception as EX:
            print(f"[{MessageBox.ErrorText}] >> Discord RPC update failed: {EX}")
            RPC.EnableRPC = False

            if RPC.RPCManager != None:
                RPC.StopRPC()

    def StopRPC():
        try:
            print(f"[{MessageBox.InfoText}] >> Stopping Discord RPC...")

            if RPC.KillRPCUpdate == True:
                print(f"[{MessageBox.WarnText}] >> Discord RPC is not running.")
                return

            RPC.RPCManager.clear(pid=os.getpid())
            RPC.RPCManager.close()

        except PipeClosed:
            print(f"[{MessageBox.WarnText}] >> Discord RPC couldn't be stopped because the pipe was closed.")

        except Exception as EX:
            print(f"[{MessageBox.ErrorText}] >> Discord RPC kill failed: {EX}")