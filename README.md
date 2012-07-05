Ensconce
========

en·sconce/en'skäns/
Verb:	
Establish or settle (someone) in a comfortable, safe, or secret place

What is this?
-------------

A .net command line tool for aiding deployment of server components.

* Update configuration files and other XML
* Initialise/update related databases
* Render arbitrary text files with environment specific variables
* Move the updated files to their final locations on disk
* 'Snapshot' the final deploy location for audit and easy roll back purposes
* Cope with multiple instances of the same component

How do I use it?
----------------

* Get a copy of your component with default configuration to the target server (at 15below we use [Octopus]:(http://octopusdeploy.com))
* Set up your environment; Ensconce expects one of the following environment variables sets to exist:
	* ClientCode
	* Environment
	* FixedStructure
* Run Ensconce:
	d:\DeployTools\Ensconce.exe -finalise -replace -deployFrom . -deployTo c:\targetDir -updateConfig
* Stuff happens: check the [wiki]:(https://github.com/15below/Ensconce/wiki) for details