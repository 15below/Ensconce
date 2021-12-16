---
title: Conditional Change - if
linkText: if
description: Details of `if` condition that can be applied to changes
---

# `if` Condition

## Compatible With

The `if` condition can be applied to all change types.

## Description

The `if` attribute can be added to a change with a template string as it's value.

Only if that condition returns a true will the substitution execute.

## Example

```XML
{% raw %}
<Change type="ChangeValue" jsonPath="$.Logging.LogLevel.Default" value="Information" if="Environment = 'PRD'" />
{% endraw %}
```
