---
title: Ends With Template Filter
linkText: Ends With
description: Ends With Template Filter
---

# Ends With Template Filter

Returns a boolean condition based on if the input value ends with the value passed to the filter

## Example

```text
{% raw %}
{% if TestValue|endsWith:'test' %}Ends With Test{% else %}Does NOT End With Test{% endif %}
{% endraw %}
```
