# PKMods
Various mods for Prehistoric Kingdom
Still early in development. again

![Ugly UI cuz im not an artist](https://i.imgur.com/bHLDyW5.png)

#### Current Functionality
- Save textures
- Load TexturePack

#### In Progress
- Save/Load textures on demand for the selected animal
- Scenery
- other stuff

## TO USE
Compile from source or download the latest release, then.
Either copy the files to your Prehistoric Kingdom folder, or use the installer (coming soon?)

After injecting press F1 to pull up the menu.
Press f2 to reload TexturePacks;

#### Manual Injection
In the event a newer version of the game is released, or you just feel like doing it youself, you can wait for a patched dll or do the following

- Use a mono injector like the one included. Check the Inject.cmd for an example command to inject, or run it directly.
- Using a dissasembler, find a method that starts early on, (such as MainMenuGUIManager.Awake) and add something like the following:
```csharp
Assembly.LoadFrom(Application.dataPath + "/Mods/PKMods.dll").GetType("PKMods.Loader").GetMethod("Load").Invoke(null, null);
```


