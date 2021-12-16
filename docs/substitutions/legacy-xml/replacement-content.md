---
title: XML Replacement Content (Legacy Format)
linkText: Replacement Content
description: Replacement Content legacy XML format
---

# XML Replacement Content (Legacy Format)

## Description

Replaces content of a node with a new set of content

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
    <ReplacementContent>
        <![CDATA[
        <nodes>
            <node1>value</node1>
            <node2>value2</node2>
            <node3>value3</node3>
        </nodes>
        ]]>
    </ReplacementContent>
</Change>
{% endraw %}
```

### Output

```XML
{% raw %}
<root>
    <nodes>
        <node1>value2</node1>
        <node2>value2</node2>
        <node3>value2</node3>
    </nodes>    
</root>
{% endraw %}
```
