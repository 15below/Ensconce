---
title: XML Set Attribute
linkText: Set Attribute
description: Set attribute XML format
---

# XML Set Attribute

## Description

Will add or update an attribute in the XML

## Example

### Base File

```XML
{% raw %}
<root>
    <node attr="false">value</node>
</root>
{% endraw %}
```

### Substitution

#### New Attribute

```XML
{% raw %}
<Change type="SetAttribute" xPath="/root/node" attributeName="attrVal" value="true" />
{% endraw %}
```

#### Existing Attribute

```XML
{% raw %}
<Change type="SetAttribute" xPath="/root/node" attributeName="attr" value="true" />
{% endraw %}
```

### Output

#### New Attribute

```XML
{% raw %}
<root>
    <node attr="false" attrVal="true">value</node>
</root>
{% endraw %}
```

#### Existing Attribute

```XML
{% raw %}
<root>
    <node attr="true">value</node>
</root>
{% endraw %}
```
