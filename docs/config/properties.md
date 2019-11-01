---
title: Properties
description: Adding key/value properties into the deployment configuration
---

# Properties

# Overview

The properties is a simple key/value store of values which can then be used as part of your deployment.

If a property has already been defined either as an environment variable or a duplicate value in your deployment configuration file the 1st value found will be used.

Properties can contain dynamic content using the [template tag syntax]({{ '/' | relative_url }}templates).

## Example

```xml
<Structure xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
  ...
  <Properties>
    <Property name="Key1">Value1</Property>
    <Property name="Key2">Value2</Property>
    <Property name="KeyWithDynamic">{{ ClientCode }}-Value</Property>
  </Properties>
  ...
</Structure>
```
