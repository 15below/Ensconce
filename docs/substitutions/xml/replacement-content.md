---
title: XML Replacement Content
linkText: Replacement Content
description: Replacement Content XML format
---

# XML Replacement Content

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
<Change type="ReplacementContent" xPath="/root">
    <Content>
        <![CDATA[
        <nodes>
            <node1>value</node1>
            <node2>value2</node2>
            <node3>value3</node3>
        </nodes>
        ]]>
    </Content>
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
