# In-process Plugins

These are .NET libraries that are loaded at runtime by the hub and shown
within `Plugin` menus.

The plugin must target the same version of .NET that the hub targets,
which is .NET Standard 2.0.

If you cannot target .NET Standard 2.0 then you will need to write an
out-of-process plugin (coming soon®).

## In-Process versus Out-of-Process

In-process plugins can take advantage of all the features that the hub
exposes to pages.

Out-of-Process plugins can be written in any language, and can have
lifetimes that differ from the hub.

Out-of-Process plugins will communicate with the hub over the network,
probably using [MQTT](https://mqtt.org/), so they don't need to be on
the same machine as the CDU Hub instance.



## How to build an in-process plugin

Better instructions will be written once things have been fleshed out.
The basic approach is:

1. Link your library against Cduhub.dll (either from an installation,
   or better yet against a build of the source).

2. Implement one public class, the details class.

3. Add a `Manifest.json` to identify which DLL is your plugin.


### Plugin Details Class

This class has to be public and it has to implement the
`Cduhub.Plugin.InProcess.IPluginDetail` interface. To do this you need
to implement four properties and a function:

`Guid Id { get; }` - return a unique ID for your plugin.

`string Label { get; }` - return a *short* (10 or fewer) character label
to show for the plugin in menus.

`int DisplayOrder { get; }` - if you want your plugin to appear before
others then return a value less than zero. If you want your plugin to
appear after others return a value greater than zero. If you don't care
about display orders then return zero.

`EntryPointPage EntryPointPage { get; }` - which plugin menu your plugin
is to appear on. The possible values are:

| Enum   | Location |
| ---    | --- |
| `Root` | The main page |

`Page CreatePage(Hub hub)` - a function that creates the main page for the
plugin. Each hub instance only creates one of these. This class is still
evolving, see examples in the Pages namespace.


#### Multiple Plugin Details

You are allowed as many plugin detail classes in the same DLL as you like.
The details are really describing an entry page for the plugin rather than
the plugin itself.


### `Manifest.json`

Here's an example:

```
{
    "FileName": "InProcessPlugin.dll",
    "MinimumHubVersion": "1.4.0"
}
```

The filename should be the plugin's main DLL. It should be pathed relative to the
plugin's deployment folder.

On case sensitive file systems the file must be called `Manifest.json`.

### Plugin deployment

After your first run of CDU Hub v1.5 and above you will have a `Plugins` folder in
the working folder. There is a link to the working folder in the UI, by default on
Windows it is:

```
%LOCALAPPDATA%\cduhub
```

Under the `Plugins` folder you need to create a folder just for your plugin.

Copy your plugin DLL and `Manifest.json` into your plugin folder and restart CDUHub.


### Errors

Error reporting is a bit basic - go to the `About` page off root and there should be
a count of loaded plugins there. If any plugins failed to load then press the link
to the errors page.

Make sure that you have targeted .NET Standard 2.0 and compiled against the same
version of `CDUHub` that you are plugging into.
