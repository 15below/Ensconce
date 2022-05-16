---
title: XML Add Child Content
linkText: Add Child Content
description: Add Child Content XML format
---

# XML Add Child Content

## Description

Will add the content as the child of the node in xPath

## Notes

It's recommended to use the `ifNotExists` attribute on the `Change` node to only add the child if it does not exist.

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
<Change type="AddChildContent" xPath="/root">
    <Content>
        <![CDATA[<node2>value2</node2>]]>
    </Content>
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
