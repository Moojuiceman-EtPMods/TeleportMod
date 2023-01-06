# TeleportMod

This is a mod for the game Escape The Pacific by Gamers4Gamers Team. You can get it here: https://store.steampowered.com/app/655290/Escape_The_Pacific/

This is a port of the ModAPI mod by Moritz as ModAPI no longer supports the game. The original mod is here https://modapi.survivetheforest.net/mod/69/teleport-mod

#### What it does

Lets you save and teleport to locations.

#### Save file backup

I try to not affect saves with my mods, but taking backups is recommended. This is also how you can share save files for deubgging.

* Saves are located at C:\Users\<Username>\AppData\LocalLow\Gamers4Gamers Team\Escape The Pacific\SavesDir
* Make a copy of the folder, either to another location or as a new folder name in the same place
* If sharing the save, zip the folder for easier transfer

#### Installation

* Download BepInEx 5 64-bit. Here is a direct link to version 5.4.21 which works with the game https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x64_5.4.21.0.zip
* Unzip it into the game folder, next to EscapeThePacific.exe. Find it via Steam by right clicking the game > Manage > Browse local files
* Start the game once so BepInEx creates the necessary folders
* Grab the latest .dll from the releases page and put it in the BepInEx\plugins folder
 
#### Configuration

Mod options are stored in BepInEx\config\<Modname>.cfg

* Open Menu Key (Default F4) - Keybind to open teleport menu