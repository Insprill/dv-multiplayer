[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![Apache 2.0 License][license-shield]][license-url]




<!-- PROJECT LOGO -->
<div align="center">
  <h1>Derail Valley Multiplayer</h1>
  <p>
    A <a href="https://store.steampowered.com/app/588030">Derail Valley</a> mod that adds multiplayer.
    <br />
    <br />
    <a href="https://github.com/AMacro/dv-multiplayer/issues">Report Bug</a>
    ·
    <a href="https://github.com/AMacro/dv-multiplayer/issues">Request Feature</a>
    ·
    <a href="https://discord.com/channels/332511223536943105/1234574186161377363" target="blank" rel="noopener noreferrer">Discord</a>
  </p>
</div>




<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project">About The Project</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#building">Building</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#translations">Translations</a></li>
    <li><a href="#license">License</a></li>
  </ol>
</details>

# Important!
At present, assume all other mods are incompatible!
Some mods may work, but many do cause issues and break multiplayer capabilities.

Our primary focus is to have the vanilla game working in multiplayer; once this is achieved we will then work on compatibility with other mods, dedicated servers and company modes.

## Installation
If you are only intending to use the mod and not work on the code, it is recommended you download from the [Nexus Mods page](https://www.nexusmods.com/derailvalley/mods/1070).
Unity Mod Manager is required to use this mod.

<!-- ABOUT THE PROJECT -->

## About The Project

Multiplayer is a Derail Valley mod that adds multiplayer to the game, allowing you to play with your friends.

It works by having one player host a game, and then other players can join that game.

This fork is a continuation of [Insprill's](https://github.com/Insprill/dv-multiplayer) amazing efforts.



<!-- Roadmap -->

## Roadmap

For a list of planned features, see the [project board][project-board-url].  
The mod will be released on Nexus once it's ready.




<!-- BUILDING -->

## Building

Before you can build Multiplayer, you'll need to either:

1) Build the Unity project.

or

2) Copy `multiplayer.assetbundle`, `MultiplayerEditor.dll` and `UnityChan.dll` from the released binaries to the `build` directory.

### Building Unity Assets
1) Install Unity Editior **2019.4.40f1**
2) Open the `MultiplayerAssets` folder in Unity **2019.4.40f1**
3) Click on the `Multiplayer` > `Build Asset Bundle and Scripts ` menu item.
The asset file and dlls will be compiled and copied to the `build` directory

Once the Unity project is compiled, you can build the mod.

1. In the main project folder copy `Directory.Build.targets.EXAMPLE` and rename the copy to `Directory.Build.targets`.
2. Open `Directory.Build.targets` in your favourite text editor and update the paths for `DvInstallDir` and `UnityInstallDir`.
If you are NOT using a code signing certificate, leave the `Cert-Thumb` attribute blank, otherwise, ensure the `SignToolPath` is correct and the `Cert-Thumb` matches the thumbprint of your certificate.
Example `Directory.Build.targets`:
```xml
<Project>
    <PropertyGroup>
        <DvInstallDir>C:\Program Files (x86)\Steam\steamapps\common\Derail Valley</DvInstallDir>
        <UnityInstallDir>C:\Program Files\Unity\Hub\Editor\2019.4.40f1\Editor</UnityInstallDir>
        <ReferencePath>
            $(DvInstallDir)\DerailValley_Data\Managed\;
            $(DvInstallDir)\DerailValley_Data\Managed\UnityModManager\;
            $(UnityInstallDir)\Data\Managed\
        </ReferencePath>
        <AssemblySearchPaths>$(AssemblySearchPaths);$(ReferencePath);</AssemblySearchPaths>
		<SignToolPath>C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\</SignToolPath>
		<Cert-Thumb>2ce2b8a98a59ffd407ada2e94f233bf24a0e68b9</Cert-Thumb>
    </PropertyGroup>
</Project>
```
3. Install xmldoc2md from the [AMacro repository](https://github.com/AMacro/xmldoc2md/), unless the [official repo](https://charlesdevandiere.github.io/xmldoc2md/) has been updated with [PR #45](https://github.com/charlesdevandiere/xmldoc2md/pull/45).
4. Open the Multiplayer solution and compile (solution may need to be compiled a couple of times for all errors to clear.

Notes:
- Debug builds will output a `harmony.log.txt` to the desktop
- Release builds will be signed (if code signing is available) and packaged.

<!-- CONTRIBUTING -->

## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create.  
Any contributions you make are **greatly appreciated**!  
If you're new to contributing to open-source projects, you can follow [this][contributing-quickstart-url] guide.




<!-- TRANSLATIONS -->

## Translations

Special thanks to those who have assisted with translations - Apologies if I've missed you, drop me a line and I'll update this section.
If you'd like to help with translations, please create a pull request or send a message on our [Discord channel](https://discord.com/channels/332511223536943105/1234574186161377363).
| **Translator** | **Language** |
| :------------ | :------------ 
| Ádi | Hungarian   |
| My Name Is BorING | Chinese (Simplified) |
| Harfeur | French |




<!-- LICENSE -->

## License

Code is distributed under the Apache 2.0 license.  
See [LICENSE][license-url] for more information.




<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->

[contributors-shield]: https://img.shields.io/github/contributors/AMacro/dv-multiplayer.svg?style=for-the-badge
[contributors-url]: https://github.com/AMacro/dv-multiplayer/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/AMacro/dv-multiplayer.svg?style=for-the-badge
[forks-url]: https://github.com/AMacro/dv-multiplayer/network/members
[stars-shield]: https://img.shields.io/github/stars/AMacro/dv-multiplayer.svg?style=for-the-badge
[stars-url]: https://github.com/AMacro/dv-multiplayer/stargazers
[issues-shield]: https://img.shields.io/github/issues/AMacro/dv-multiplayer.svg?style=for-the-badge
[issues-url]: https://github.com/AMacro/dv-multiplayer/issues
[license-shield]: https://img.shields.io/github/license/AMacro/dv-multiplayer.svg?style=for-the-badge
[license-url]: https://github.com/AMacro/dv-multiplayer/blob/master/LICENSE
[altfuture-support-email-url]: mailto:support@altfuture.gg
[contributing-quickstart-url]: https://docs.github.com/en/get-started/quickstart/contributing-to-projects
[asset-studio-url]: https://github.com/Perfare/AssetStudio
[mapify-building-docs]: https://dv-mapify.readthedocs.io/en/latest/contributing/building/
[project-board-url]: https://github.com/users/AMacro/projects/2
