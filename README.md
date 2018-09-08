# PKMods

Modding platform for Prehistoric Kingdom.

## PKMODS is in very early development so anything can break at anytime for anyreason, and nothing is guarenteed at all

![Ugly UI cuz im not an artist](https://i.imgur.com/bHLDyW5.png)

#### Current Functionality
- Save textures
- Load TexturePack
- (Proof of concept) Share skins to a database, hosted on Firebase. Press the Browse button.

#### In Progress
- Save/Load textures on demand for the selected animal
- Scenery
- other stuff

## How to Install
Compile from source or download the latest release, then.
- Open your Prehistoric Kingdom installation folder. (might look like: C:\Program Files (x86)\Steam\steamapps\common\Prehistoric Kingdom Demo)

### From Precompiled Release:
 - From the release you downloaded, copy the Prehistoric Kingdom_Data contents to your local Prehistoric Kingdom_Data directory.

Start a game and press F1 to pull up the menu.
Press f2 to reload TexturePacks;
Thats it, enjoy the modding!


If you would prefer to fork and/or compile yourself, youll need to copy some files,
### From Compiled Binaries, copy
- TexturePackData.dll to Prehistoric Kingdom_Data/Managed  (allows for binary serialized and compressed texturepacks)
- modmenu.ab to Prehistoric Kingdom_Data/Mods (This is an assetbundle of the Mod UI. The source is in the DevKit Git)
- PKMods.dll  to Prehistoric Kingdom_Data/Mods (This is the compiled code of the mods)
- ~~liblzma.dll to Prehistoric Kingdom_Data/Mono (allows for compressed texturepacks)~~ compression removed, have not found an elegent solution yet.

#### Manual Injection
In the event a newer version of the game is released (and you dont want to wait for a patch), or you just feel like doing it youself, you can do EITHER of the following

- Use a mono injector like the one included. Check the Inject.cmd for an example command to inject, or run it directly.
or
- Using a dissasembler, find a method that starts early on, (such as MainMenuGUIManager.Awake) and add something like the following:
```csharp
Assembly.LoadFrom(Application.dataPath + "/Mods/PKMods.dll").GetType("PKMods.Loader").GetMethod("Load").Invoke(null, null);
```

## Adding texture packs
Download the latest release of the external texturepack creator here:
https://github.com/Pathos0001/TexturePackCreator
Then create a texturepack.

And add them to the Mods/Texturepacks/ folder



