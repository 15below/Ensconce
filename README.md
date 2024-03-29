Ensconce
========

en·sconce/en'skäns/
Verb:
Establish or settle (someone) in a comfortable, safe, or secret place

[![GitHub release (latest by date)](https://img.shields.io/github/v/release/15below/Ensconce?label=latest%20github%20release)](https://github.com/15below/Ensconce/releases/latest)
[![GitHub all releases](https://img.shields.io/github/downloads/15below/Ensconce/total?label=github%20downloads)](https://github.com/15below/Ensconce/releases/latest)
[![Nuget](https://img.shields.io/nuget/v/Ensconce.DotNetTool?label=latest%20nuget%20dotnet%20tool%20release)](https://www.nuget.org/packages/Ensconce.DotNetTool)
[![Nuget](https://img.shields.io/nuget/dt/Ensconce.DotNetTool?label=nuget%20dotnet%20tool%20downloads)](https://www.nuget.org/packages/Ensconce.DotNetTool)


What is this?
-------------

A .net command line tool for aiding deployment of server components.

* Update configuration files and other XML
* Initialise/update related databases
* Render arbitrary text files with environment specific variables
* Move the updated files to their final locations on disk
* Cope with multiple instances of the same component
* Deploy SSRS reports

Deploying A Release
-------------------

Each release contains a nuget package which is an [Octopus](https://octopus.com/) deployment package.

There are 3 required variables, these are:

* DeployPath - set to the folder you want to deploy Ensconce too
* IncludeK8s - set to `True` or `False` depending on if you want Kubernetes deployments
* VersionNumber - set as `#{Octopus.Release.Number}` in order to get the right version number

How do I use it?
----------------

Documentation for how to use ensconce can be found at https://15below.github.io/Ensconce/
