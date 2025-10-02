# Athena

<img src=".github/AthenaLogoHW.png" height="130" align="right"> 

#### Powered by [CUE4Parse](https://github.com/FabianFG/CUE4Parse) and [EpicManifestParser](https://github.com/NotOfficer/EpicManifestParser)

[![Release](https://img.shields.io/github/release/djlorenzouasset/Athena)]()
[![Downloads](https://img.shields.io/github/downloads/djlorenzouasset/Athena/total?color=green)]()
[![Contributors](https://img.shields.io/github/contributors/djlorenzouasset/Athena)]()
[![Discord](https://discord.com/api/guilds/1229126278339231785/widget.png?style=shield)](https://discord.gg/nJBj9NjUS4)

-----------------

Fortnite Profile Athena and Item Shop generator for Private Servers that use Fortnite-Live manifest for fast in-game leaks on updates.

You can use this program with [Neonite](https://github.com/HybridFNBR/Neonite) backend, saving the `profile_athena.json` in the `profiles` folder and `shop.json` in the `responses/catalog` folder (rename it `shopv3.json`). Make sure to save the `fortnitegame.json` (file situated in the `backend` folder) in the `responses` folder.

> [!IMPORTANT]
> We take no responsibility for the inproper use of this program. Epic Games does not tolerate the possibility of having cosmetics that are NOT purchased in the game's official item shop within private servers (being violation of the [EULA](https://store.epicgames.com/en-US/eula)).

-----------------

### Requirements

- [NET 8.0 Runtime or higher](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime)

-----------------

### Download the Application

You can download the application already compiled [here](https://github.com/djlorenzouasset/Athena/releases/latest)

### Build via command line

1. Clone the source code
```
git clone https://github.com/djlorenzouasset/Athena --recursive
```

2. Build the program
```
cd Athena
dotnet publish Athena -c Release --no-self-contained -p:PublishReadyToRun=false -p:PublishSingleFile=true -p:DebugType=None -p:GenerateDocumentationFile=false -p:DebugSymbols=false
```
-----------------

## Need help?

If you need help with Athena, join the support server by [this link](https://discord.gg/nJBj9NjUS4)

## Support the development!

Want to support this project? Feel free to donate!

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/F1F6IB03D)
