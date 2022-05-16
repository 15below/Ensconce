---
title: XML Add Attribute
linkText: Add Attribute
description: Add attribute XML format
---

# XML Add Attribute

## Description

Will add an attribute to an already existing node in the XML

## Notes

This has been deprecated in favour of [SetAttribute](set-attribute)

## Example

### Base File

```XML
{% raw %}
<root>
    <node>value</node>
</root>
{% endraw %}
```

### Substitution

```XML
{% raw %}
<Change type="AddAttribute" xPath="/root/node" attributeName="attrVal" value="true" />
{% endraw %}
```

### Output

```XML
{% raw %}
<root>
    <node attrVal="true">value</node>
</root>
{% endraw %}
```
