# Building for Unix-like Operating Systems

The intention is to offer the application up as a FlatPak
bundle. However, that hasn't been done yet.

In lieu of the FlatPak bundle, and for those who don't want
to (or cannot) use FlatPak, here are the instructions for
building and running CDU Hub on Unix-like operating systems.

## Summary

The short version of these instructions is:

1. Ensure that the .NET Core SDK (version 8) is installed.
2. Ensure that git is installed.
3. Either clone the source from `https://github.com/vradarserver/cduhub.git`
   or `git pull` the latest version if you already have it cloned.
4. `cd` into the source directory.
5. Run `dotnet run --project apps/cduhub-cli/cduhub-cli.csproj`


## Pre-requisites

### .NET Core 8 SDK

There is only one pre-requisite for building CDU Hub: the .NET Core 8 SDK.
If you already have the SDK installed then you can skip this section.


#### Ubuntu

The .NET Core 8 SDK is available as a package:

```
sudo apt install dotnet-sdk-8.0
```

#### All other distros / Windows

If the SDK is not available as a package (check your distro documentation)
then it is available for download from Microsoft's site here:

https://dotnet.microsoft.com/en-us/download/dotnet/8.0

There are lots of downloads on there - you only need one, which is the
SDK. If the page offers different versions of the SDK then install the
latest version.

### Check that the .NET Core 8 SDK is present (optional)

Type this from a command-line prompt:

```
dotnet sdk check
```

It should show something like this (the output goes on for longer than
this, and your version number is likely to be different - doesn't matter,
just check that the SDK version is at least 8.0.0):

```
.NET SDKs:
Version      Status
------------------------
8.0.119      Up to date.
```

### Git (optional)

Git is a source control system. It lets you download the source code with
a single command from the command-line, and likewise download any updates
to the source with a single command.

It is likely that you already have git installed with your distro. To check
to see if that is the case run this from the command-line:

```
git version
```

If it comes back with a version number then all is good. Otherwise you
should be able to install it via your distro's package manager.



## Download the source

### Git

The command to clone the CDU Hub repository into a local directory is:

```
git clone https://github.com/vradarserver/cduhub.git
```

That will create a sub-directory called `cduhub` under the current directory
and download all of the source into it.

For example, if you wanted to create a folder called `src` under your home
directory, and then clone CDU Hub in there, then the full set of commands at
the terminal would be:

* Switch to your home directory:<br/>
  `cd ~`
* Create a directory called src (only do this once):<br/>
  `mkdir src`
* Switch to the src directory:<br/>
  `cd src`
* Clone CDU Hub into a directory called cduhub under src (only do this once):<br/>
  `git clone https://github.com/vradarserver/cduhub.git`
* Switch to the cduhub directory:<br/>
  `cd cduhub`

If you have already done that and you just want to update the source then
cd into the cduhub directory and do:

```
git pull
```

#### Source for a specific version

The clone and pull commands will download the latest version of the source.
If you want to download the source for a particular version instead then cd
into the cduhub directory and do:

```
git checkout v1.4.1
```

Replace `1.4.1` with the version number that you want to download.

It will show you a bunch of warnings about being on a "detached HEAD", but it
should be using the source code for that version.

If you want to return to the latest source then do:

```
git checkout main
```


### ZIP or tarball

If you're not using git and you just want to download the source manually then
each release page has both a ZIP and a tarball archive of the source. The archive
for the latest version can be found here:

```
https://github.com/vradarserver/cduhub/releases/latest
```

## Building and running from the command-line

CD into the folder where you cloned or decompressed the source and then run
this from the command-line:

```
dotnet run --project apps/cduhub-cli/cduhub-cli.csproj
```

As of time of writing it'll probably show a warning about the dependency
on a pre-release version of `System.CommandLine`, and then it'll start up.
You can ignore the warning.

The documentation for the cduhub-cli program is here:

https://github.com/vradarserver/cduhub/blob/main/apps/cduhub-cli/README.md

