---
title: XML Add Child Content (Legacy Format)
linkText: Add Child Content
description: Add Child Content legacy XML format
---

# XML Add Child Content (Legacy Format)

## Description

Will add the content as the child of the node in xPath

## Notes

It's recommended to use the `ifNotExists` attribute on the `AddContent` node to only add the child if it does not exist.

The `ifNotExists` attribute requires a xPath for checking existence.

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
    <XPath>/root</XPath>
    <AddChildContent>
        <![CDATA[<node2>value2</node2>]]>
    </AddChildContent>
</Change>
{% endraw %}
```

### Output

```XML
{% raw %}
<root>
    <node>value</node>
    <node2>value2</node2>
</root>
{% endraw %}
```
