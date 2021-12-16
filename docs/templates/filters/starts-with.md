---
title: Starts With Template Filter
linkText: Starts With
description: Starts With Template Filter
---

# Starts With Template Filter

Returns a boolean condition based on if the input value starts with the value passed to the filter

## Example

```text
{% raw %}
{% if TestValue|startsWith:'test' %}Starts With Test{% else %}Does NOT Start With Test{% endif %}
{% endraw %}
```
