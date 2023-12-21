# Setup
## Required Information
In order to set up Electra, you'll need 4 things:
* Your PiShock account's name
* A name to use for logging (This will be used to see who sent commands)
* A share code
* An API key (this can be retrieved from the account page on the PiShock website)

<br/>

## Files and Directories
> [!NOTE]  
> An example JSON configuration file can be found [here](https://github.com/MEMESCOEP/Electra/blob/main/Examples/ElectraConfig.json).

> [!IMPORTANT]
> All files will be placed in the `Assets` folder. This folder should be created in the same directory as the compiled binary.

<br/>

All of the information outlined in the [Required Information](#required-information) section will be placed in a JSON file called `ElectraConfig.json`, which will be placed in the `Assets` folder you created earlier.

Make sure to populate the required fields correctly! In the future, I'll most likely make a way to configure from a GUI.

Electra loads icons at runtime from the `Assets` directory. Copy the `Logo_32x32.png`, `Logo_128x128.png`, `Intensity.png`, and `Duration.png` files from the `Resources` directory to the `Assets` directory.
