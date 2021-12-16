---
title: XML Change Value
linkText: Change Value
description: Change value XML format
---

# XML Change Value

## Description

Changes the value of a specified attribute

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
<Change type="ChangeValue" xPath="/root/node" value="new-value" />
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
