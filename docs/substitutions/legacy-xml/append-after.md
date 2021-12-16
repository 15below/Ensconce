---
title: XML Append After (Legacy Format)
linkText: Append After
description: Append After legacy XML format
---

# XML Append After (Legacy Format)

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
<Change>
    <XPath>/root/node</XPath>
    <AppendAfter>
        <![CDATA[<node2>value2</node2>]]>
    </AppendAfter>
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
