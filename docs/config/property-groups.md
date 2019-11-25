---
title: Property Groups
description: Adding sets of grouped properties 
---

# Property Groups

## Overview

The property groups are sets of key/value configuration which have been labeled together in a group.

Once labelled these can be looped over as part of the [template tag syntax]({{ '/' | relative_url }}templates).

Groups can contain an identity which is a pre-defined key with the value you provide.  This means a property group will always have the identity as a key/value property.

A property group can also be defined with multiple labels, but this syntax is much more verbose

## Example

```XML
{% raw %}
<Structure xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
  ...
  <PropertyGroups>
    <PropertyGroup label="Group" identity="1">
      <Property name="GroupKey1">GroupValue1</Property>
      <Property name="GroupKey2">GroupValue2</Property>
    </PropertyGroup>
    <PropertyGroup label="Group" identity="2">
      <Property name="GroupKey1">GroupValue1</Property>
      <Property name="GroupKey2">GroupValue2</Property>
    </PropertyGroup>
    <PropertyGroup identity="1">
      <Labels>
        <Label>Label1</Label>
        <Label>Label2</Label>
      </Labels>
      <Properties>
        <Property name="GroupKey1">GroupValue1</Property>
        <Property name="GroupKey2">GroupValue2</Property>
      </Properties>
    </PropertyGroup>
  </PropertyGroups>
  ...
</Structure>
{% endraw %}
```
