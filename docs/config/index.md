---
title: Deployment Configuration
description: The home of deployment configuration documentation
linkText: Deployment Configuration
---

# Deployment Configuration

## Overview

Ensconce will load configuration from your environment variables as well as an optional "FixedStructure" deployment configuration file.

This section of the documentation will explain all about the deployment configuration file.  A deployment configuration file has 4 sections, these are:

* [Client/Environment](client-environment)
* [Properties](properties)
* [Property Groups](property-groups)
* [DBLogins](database-logins)

## Basic Structure

The structure is defined in a [XSD](deploy-config-xsd).  An example structure can be seen below.

```XML
{% raw %}
<Structure xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
  <ClientCode>AAA</ClientCode>
  <Environment>PRD</Environment>
  <Properties>
    <Property name="Key1">Value1</Property>
    <Property name="Key2">Value2</Property>
    <Property name="KeyWithDynamic">{{ ClientCode }}-Value</Property>
  </Properties>
  <PropertyGroups>
    <PropertyGroup label="Group" identity="1">
      <Property name="GroupKey1">GroupValue1</Property>
      <Property name="GroupKey2">GroupValue2</Property>
    </PropertyGroup>
    <PropertyGroup label="Group" identity="2">
      <Property name="GroupKey1">GroupValue1</Property>
      <Property name="GroupKey2">GroupValue2</Property>
    </PropertyGroup>
  </PropertyGroups>
  <DbLogins>
    <DbLogin>
      <Name>LoginName</Name>
      <DefaultDb>master</DefaultDb>
      <Password>NoPasswordsRoundHere</Password>
    </DbLogin>
  </DbLogins>
</Structure>
{% endraw %}
```
