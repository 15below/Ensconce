---
title: XML Add Child Content (Legacy Format)
linkText: Add Child Content
description: Add Child Content legacy XML format
---

# XML Add Child Content (Legacy Format)

## Description

Will add the content as the child of the node in xPath

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
