---
title: Conditional Change - ifNotExists
linkText: ifNotExists
description: Details of `ifNotExists` condition that can be applied to changes
---

# `ifNotExists` Condition

## Compatible With

The `ifNotExists` can only be used on the `AddChildContent` change types

## Description

The use case for `ifNotExists` is to only apply the operation to add some XML content if the content does not already exist in the output.

It's advisable to always add the child content as an empty node, and then populate it as a second change.  This is so that if the node does already exist, the replacement operation still occurs.

## Example

```XML
{% raw %}
<Change type="AddChildContent" xPath="/root" ifNotExists="/root/testing">
    <Content>
        <![CDATA[<testing/>]]>
    </Content>
</Change>
<Change type="ReplacementContent" xPath="/root/testing">
    <Content>
        <![CDATA[<test myAttr="before">value</test>]]>
    </Content>
</Change>
{% endraw %}
```
