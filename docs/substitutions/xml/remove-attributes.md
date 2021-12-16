---
title: XML Remove Attributes
linkText: Remove Attributes
description: Remove Attributes XML format
---

# XML Remove Attributes

## Description

Will remove all attributes from a node

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
<Change type="RemoveCurrentAttributes" xPath="/root/node" />
{% endraw %}
```

### Output

```XML
{% raw %}
<root>
    <node>value</node>
</root>
{% endraw %}
```
