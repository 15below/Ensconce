---
title: Home
description: The home of the 15below Ensconce documentation
linkText: Home
---

# Overview

The 15below Ensconce command line tool is the perfect tool to help deploy your applications onto Windows or Linux machines.

At 15below, we use Ensconce alongside [Octopus Deploy](https://octopus.com){:.link-secondary} as part of our Powershell deployment scripting.

Ensconce can be called by placing the `Ensconce.Console.exe` app on the machine you're deploying to and invoking commands on it, or by using the dotnet tool which can be installed by running `dotnet tool install --global Ensconce.DotNetTool`.

On windows, or machines with powershell core the powershell helpers can also be used.  The `deployHelp.ps1` is the entry point for calling Ensconce and various other powershell helpers exist for different functions.
