---
title: Template Boolean Operators
linkText: Boolean Operators
description: Template Boolean Operators
---

# Boolean Operators

If a condition is a boolean expression can check it directly.

It's also possible to invert the check using the `not` keyword

## Example

```text
{% raw %}
{% if TestValue|exists %}Value Exists{% else %}Value Doesn't Exist{% endif %}
{% endraw %}
```

## Inverted Example

```text
{% raw %}
{% if not TestValue|exists %}Value Doesn't Exist{% else %}Value Exists{% endif %}
{% endraw %}
```
