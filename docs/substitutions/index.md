---
title: Substitution Files
description: The home of substitution files documentation
---

# Substitution Files

## Overview

Substitution files give the option to update both XML & JSON based configuration files.

The substitution is written in an XML format and can be run at deployment time to update configuration.

This means you do not need to worry about changing application configuration within your source code repository, you know that the deployed system will always have the correct configuration values.

## Basic Structure

The structure is defined in a [XSD](substitution-xsd).  An example structure can be seen below.

```XML
{% raw %}
<?xml version="1.0" encoding="utf-8"?>
<Root xmlns="http://15below.com/Substitutions.xsd">
  <Namespaces />
  <Files>
    <File>
      <Changes>
        <Change>
        ...
        </Change>
      </Changes>
    </File>
    <File>
      <ReplacementTemplate>...</ReplacementTemplate>
    </File>
  </Files>
</Root>
{% endraw %}
```

The structure of change and files nodes can vary.

Each type of structure can be found on dedicated pages

## Types Of Substitution

* [File Node](file)
* [JSON](json)
* [XML (Legacy format)](legacy-xml)
* [XML](xml)
