# RpgMakerEncoder

The purpose of this application is to decode RPG Maker files so they can be used with source control. The application is capable of effectively writing RPG Maker files as json files, and back again.

### Background
RPG Maker files are binaries marshaled with Ruby, which makes it near impossible to use them in most source control systems.
I chose json as the source control file format as it is relatively easy to resolve merge conflicts, and it can contain most data structures.

## Usage

* Download RpgMakerEncoder into the directory meant for source control.
   
   RpgMakerEncoder is designed to run in the directory indended to be checked into source control.
   For simplicity, the application can be checked into source control for easy consumption of others, without needing additional tools/scripts to download it after cloning.
   It is recomended to keep all additional files out of the Game directory because all additional files will be packed when RPG Maker builds a deployable game package.
   
   Sample Directory Structure
   ```
   ./MySource
     - This directory is the root level of the git repository
   ./MySource/MyGame
     - This directory would contain the game files and folders(such as Game.exe and Game.rxproj)
   ./MySource/RpgMakerEncoder
     - This directory would contain the RpgMakerEncoder application files
   ```

* Open RpgMakerEncoder from the doanloded directory.

* If this is the first time opening the application, verify that the Game and Source directories match the intended layout.

   It is recomended to check in the `settings.json` file so the setup is consistient across contributers.
   The `settings.json` file will be saved upon application exit.

* Click `Decode` to write RPG Maker files as json files.
* Click `Encode` to write json files as RPG Maker files.
