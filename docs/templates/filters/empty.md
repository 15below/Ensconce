---
title: Empty Template Filter
linkText: Empty
description: Empty Template Filter
---

# Empty Template Filter

Determine if a value exists and is not a whitespace character

## Example

```text
{% raw %}
{% if TestValue|empty %}Empty Value{% else %}Populated Value{% endif %}
{% endraw %}
```
