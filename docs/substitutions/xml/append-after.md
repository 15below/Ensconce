---
title: XML Append After
linkText: Append After
description: Append After XML format
---

# XML Append After

## Description

Will add the content as the sibling of the node in xPath

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
<Change type="AppendAfter" xPath="/root/node">
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
