---
title: XML Change Attribute
linkText: Change Attribute
description: Change attribute XML format
---

# XML Change Attribute

## Description

Changes the value of a specified attribute

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
<Change type="ChangeAttribute" xPath="/root/node" attributeName="attrVal" value="false" />
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
