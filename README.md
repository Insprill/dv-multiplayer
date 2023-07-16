[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![Apache 2.0 License][license-shield]][license-url]




<!-- PROJECT LOGO -->
<div align="center">
  <h1>Gauge</h1>
  <p>
    A <a href="https://store.steampowered.com/app/588030">Derail Valley</a> that allows the track gauge to be changed. 
    <br />
    <br />
    <a href="https://github.com/Insprill/dv-gauge/issues">Report Bug</a>
    ·
    <a href="https://github.com/Insprill/dv-gauge/issues">Request Feature</a>
  </p>
</div>




<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project">About The Project</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#building">Building</a></li>
    <li><a href="#building-the-asset-bundle">Building the Asset Bundle</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
  </ol>
</details>




<!-- ABOUT THE PROJECT -->

## About The Project

Gauge a Derail Valley mod that allows you to change the track gauge to anything you want.

This changes the gauge of the tracks, rolling stock bogies, and track decorations like buffer stops.
Physics is unaffected, this mod is purely aesthetic.




<!-- BUILDING -->

## Building

Gauge uses the same build system as Mapify.
For instructions on how to build the project, please read the [Mapify building documentation][mapify-building-docs].




<!-- BUILDING ASSET BUNDLE -->

## Building The Asset Bundle

To build the AssetBundle for gauge, you'll need to install Unity **2019.4.40f1**.
You can then open up the `GaugeBundleBuilder` project in this repo.

To add the meshes to the project you'll need to export them yourself using something like [AssetStudio][asset-studio-url].
You can find all the meshes you need to export in `GaugeBundleBuilder/Assets/Meshes/meshes.txt`.

To build the bundle, go to `Gauge > Build Asset Bundle`.
If you're missing meshes, it won't let you build the bundle.




<!-- CONTRIBUTING -->

## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create.  
Any contributions you make are **greatly appreciated**!  
If you're new to contributing to open-source projects, you can follow [this][contributing-quickstart-url] guide.




<!-- LICENSE -->

## License

Code is distributed under the Apache 2.0 license.  
See [LICENSE][license-url] for more information.

AssetBundle assets are owned by Altfuture and are included with permission, for the purpose of having Read/Write protection on them removed, which is necessary for this mod to function.
These assets are not covered by the Apache 2.0 license and have different terms and conditions. Contact [support@altfuture.gg][altfuture-support-email-url] for more information.




<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->

[contributors-shield]: https://img.shields.io/github/contributors/Insprill/dv-gauge.svg?style=for-the-badge
[contributors-url]: https://github.com/Insprill/dv-gauge/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/Insprill/dv-gauge.svg?style=for-the-badge
[forks-url]: https://github.com/Insprill/dv-gauge/network/members
[stars-shield]: https://img.shields.io/github/stars/Insprill/dv-gauge.svg?style=for-the-badge
[stars-url]: https://github.com/Insprill/dv-gauge/stargazers
[issues-shield]: https://img.shields.io/github/issues/Insprill/dv-gauge.svg?style=for-the-badge
[issues-url]: https://github.com/Insprill/dv-gauge/issues
[license-shield]: https://img.shields.io/github/license/Insprill/dv-gauge.svg?style=for-the-badge
[license-url]: https://github.com/Insprill/dv-gauge/blob/master/LICENSE
[altfuture-support-email-url]: mailto:support@altfuture.gg
[contributing-quickstart-url]: https://docs.github.com/en/get-started/quickstart/contributing-to-projects
[asset-studio-url]: https://github.com/Perfare/AssetStudio
[mapify-building-docs]: https://dv-mapify.readthedocs.io/en/latest/contributing/building/
