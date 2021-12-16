---
title: Conditional Change - matchAll
linkText: matchAll
description: Details of `matchAll` condition that can be applied to changes
---

# `matchAll` Condition

## Compatible With

The `matchAll` condition can be applied to all change types.

## Description

When performing changes if the XPath of jsonPath matches multiple elements the substitution will fail.

However, if you legitimately wanted to update all instances that match you can provide a `matchAll` value of `true` which signifies that you do want all matching elements to be updated

## Example

```XML
{% raw %}
<Change type="ChangeValue" xPath="/root/nodes/node" value="NewValue" matchAll="true" />
{% endraw %}
```
