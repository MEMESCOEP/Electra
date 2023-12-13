### GetCOMName ###
# Uses a serial VID and PID to find the PiShock Hub's COM port name
# Based on https://github.com/zerario/Python-PiShock/blob/main/src/pishock/zap/serialapi.py

## IMPORTS ##
import serial.tools.list_ports

## VARIABLES ##
USB_SERIAL_IDS = [
    (0x1A86, 0x7523),  # CH340, PiShock Next
    (0x1A86, 0x55D4),  # CH9102, PiShock Lite
]

def GetCOMPortName():
    for info in serial.tools.list_ports.comports():
        if (info.vid, info.pid) in USB_SERIAL_IDS:
            return f"{info.device}, {info.vid}, {info.pid}"

if __name__ == '__main__':
    print(GetCOMPortName())
