---
title: XML Set Attribute (Legacy Format)
linkText: Set Attribute
description: Set attribute legacy XML format
---

# XML Set Attribute (Legacy Format)

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
<Change>
    <XPath>/root/node</XPath>
    <SetAttribute attributeName="attrVal">true</SetAttribute>
</Change>
{% endraw %}
```

Alternative version

```XML
{% raw %}
<Change>
    <XPath>/root/node</XPath>
    <SetAttribute attributeName="attrVal" value="true" />
</Change>
{% endraw %}
```

#### Existing Attribute

```XML
{% raw %}
<Change>
    <XPath>/root/node</XPath>
    <SetAttribute attributeName="attr">true</SetAttribute>
</Change>
{% endraw %}
```

Alternative version

```XML
{% raw %}
<Change>
    <XPath>/root/node</XPath>
    <SetAttribute attributeName="attr" value="true" />
</Change>
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
