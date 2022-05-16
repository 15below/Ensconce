---
title: XML Add Attribute (Legacy Format)
linkText: Add Attribute
description: Add attribute legacy XML format
---

# XML Add Attribute (Legacy Format)

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
<Change>
    <XPath>/root/node</XPath>
    <AddAttribute attributeName="attrVal">true</AddAttribute>
</Change>
{% endraw %}
```

Alternative version

```XML
{% raw %}
<Change>
    <XPath>/root/node</XPath>
    <AddAttribute attributeName="attrVal" value="true" />
</Change>
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
