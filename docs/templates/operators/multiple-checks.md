---
title: Combining Operators
linkText: Combining Operators
description: Combining Operators
---

# Combining Operators

You can combine operators using the `and` or `or` keywords

## And Example

```text
{% raw %}
{% if TestValue|exists and TestValue == 'Testing' %}Value Exists & Has Value Testing{% else %}Value Doesn't Exist Or Is Not Testing{% endif %}
{% endraw %}
```

## Or Example

```text
{% raw %}
{% if TestValue == 'Testing' or TestValue == 'True' %}Value Is Testing Or True{% else %}Value Is NOT Testing Or True{% endif %}
{% endraw %}
```
