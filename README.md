# Athena
<img src=".github/AthenaLogo.png" height="130" align="right"> 

Fortnite Profile Athena & Catalog creator for Private Servers that use Fortnite-Live manifest.

> [!IMPORTANT]
> We take no responsibility for the inproper use of this program. Epic Games does not tolerate the possibility of having cosmetics that are NOT purchased in the game's official item shop within private servers (being violation of the [UELA](https://store.epicgames.com/en-US/eula)).

> [!TIP]
> You can use this program with [Neonite](https://github.com/HybridFNBR/Neonite) backend, saving the profile_athena.json in the profiles folder and shop.json in the responses folder. Make sure to save the `fortnitegame.json` (file situated in the .backend folder) in the responses folder.

-----------------

#### Requirements

* <a href='https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime'>.NET 8.0 Runtime or higher</a>

#### Build via command line

1. Clone the source code
```
git clone https://github.com/djlorenzouasset/Athena --recursive
```

2. Build the program
```
cd Athena
dotnet publish Athena -c Release --no-self-contained -p:PublishReadyToRun=false -p:PublishSingleFile=true -p:DebugType=None -p:GenerateDocumentationFile=false -p:DebugSymbols=false
```

Or install the latest release [here](https://github.com/djlorenzouasset/Athena/releases/latest) 

#### Support the project
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/F1F6IB03D)

-----------------

## Credits
- A very big thank to [@andredotuasset](https://twitter.com/andredotuasset) & [@unrealhybrid](https://twitter.com/unrealhybrid) for helping in the development.
- Profile Athena model (modified) made by [@OutTheShade](https://github.com/OutTheShade).
