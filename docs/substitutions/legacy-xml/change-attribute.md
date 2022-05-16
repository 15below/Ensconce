---
title: XML Change Attribute (Legacy Format)
linkText: Change Attribute
description: Change attribute legacy XML format
---

# XML Change Attribute (Legacy Format)

## Description

Changes the value of a specified attribute

## Notes

This has been deprecated in favour of [SetAttribute](set-attribute)

## Example

### Base File

```XML
{% raw %}
<root>
    <node attrVal="true">value</node>
</root>
{% endraw %}
```

### Substitution

```XML
{% raw %}
<Change>
    <XPath>/root/node</XPath>
    <ChangeAttribute attributeName="attrVal">false</AddAttribute>
</Change>
{% endraw %}
```

Alternative version

```XML
{% raw %}
<Change>
    <XPath>/root/node</XPath>
    <AddAttribute attributeName="attrVal" value="false" />
</Change>
{% endraw %}
```

### Output

```XML
{% raw %}
<root>
    <node attrVal="false">value</node>
</root>
{% endraw %}
```
