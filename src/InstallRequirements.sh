#!/bin/bash
# Check for root access
if [ "$EUID" -ne 0 ]; then
  printf "[ERROR] >> This script requires root.\n"
  exit 1
fi

# Find a package manager, if a supported one is installed
printf "[INFO] >> Finding installed package managers...\n"
APT_GET_CMD=$(which apt-get)
PACMAN_CMD=$(which pacman)
BREW_CMD=$(which brew)
DNF_CMD=$(which dnf)
YUM_CMD=$(which yum)

# Run the appropriate package manager if a supported one is installed
if [ ! -z $PACMAN_CMD ]; then
  $PACMAN_CMD -Syu --noconfirm python3 python-pip sdl2 dotnet-sdk binutils
elif [ ! -z $APT_GET_CMD ]; then
  $APT_GET_CMD install -y python3 python3-pip libsdl2-dev dotnet-sdk-8.0 binutils
elif [ ! -z $BREW_CMD ]; then
  $BREW_CMD install python3 python3-pip libsdl2-dev dotnet-sdk-8.0 binutils
elif [ ! -z $YUM_CMD ]; then
  $YUM_CMD install -y python3 python3-pip SDL2-devel dotnet-sdk-8.0 binutils
elif [ ! -z $DNF_CMD ]; then
  $DNF_CMD install -y python3 python3-pip SDL2-devel dotnet-sdk-8.0 binutils
else
  printf "\n\n[ERROR] >> Your OS doesn't have a supported package manager installed. Please install either Pacman, Apt, DNF, Yum, or Brew.\n"
  exit 2
fi

# Install python modules
if [ "$1" = "-risky" ]; then
  python3 -m pip install -r Requirements.txt --break-system-packages
else
  python3 -m pip install -r Requirements.txt
fi
