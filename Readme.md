Flow.Launcher.Plugin.BaseConverter
==================

A **Base N number converter plugin** for the [Flow launcher](https://github.com/Flow-Launcher/Flow.Launcher).

This plugin is ported from a [WOX](https://github.com/Wox-launcher/Wox) plugin [Hexadecimal-conversion](https://github.com/encyclist/Hexadecimal-conversion),

Thanks to the contribution of orginal author [encyclist](https://github.com/encyclist)!

### Develop

* Use *dotnet clean*, *dotnet publish* to build the plugin,
* then copy files contained in publish directories to "$FlowLauncher/$FlowLauncher_Version/Plugins/Flow.Launcher.Plugin.BaseConverter/*"

### Usage

    * b~ numbers: convert digits from binary form into others
    * o~ numbers: convert digits from octal form into others
    * d~ numbers: convert digits from decimal form into others
    * x~ numbers: convert digits from hexdecimal form into others
