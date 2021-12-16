---
title: XML Change Value (Legacy Format)
linkText: Change Value
description: Change value legacy XML format
---

# XML Change Value (Legacy Format)

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
<Change>
    <XPath>/root/node</XPath>
    <ChangeValue>new-value</AddAttribute>
</Change>
{% endraw %}
```

Alternative version

```XML
{% raw %}
<Change>
    <XPath>/root/node</XPath>
    <ChangeValue value="new-value" />
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
