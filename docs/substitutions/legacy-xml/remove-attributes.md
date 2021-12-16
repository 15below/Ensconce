---
title: XML Remove Attributes (Legacy Format)
linkText: Remove Attributes
description: Remove Attributes legacy XML format
---

# XML Remove Attributes (Legacy Format)

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
<Change>
    <XPath>/root/node</XPath>
    <RemoveCurrentAttributes />
</Change>
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
