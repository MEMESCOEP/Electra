#!/bin/bash
# Check for root access
if [ "$EUID" -ne 0 ]; then
  echo "This script requires root."
  exit 1
fi

# Find a package manager, if a supported one is installed
PACMAN_CMD=$(which pacman)
APT_GET_CMD=$(which apt-get)
BREW_CMD=$(which brew)

# Run the appropriate package manager if a supported one is installed
if [ ! -z $PACMAN_CMD ]; then
  $PACMAN_CMD --noconfirm python3 python3-pip libsdl2-dev dotnet-sdk-8.0
elif [ ! -z $APT_GET_CMD ]; then
  $APT_GET_CMD install -y python3 python3-pip libsdl2-dev dotnet-sdk-8.0
elif [ ! -z $BREW_CMD ]; then
  $BREW_CMD install python3 python3-pip libsdl2-dev dotnet-sdk-8.0
else
  echo "Your OS doesn't have a supported package manager installed. Please install either Pacman, Apt, or Brew."
  exit 2
fi

# Install python modules
if [ $1 = "-risky" ]; then
  python3 -m pip install -r Requirements.txt --break-system-packages
else
  python3 -m pip install -r Requirements.txt
fi