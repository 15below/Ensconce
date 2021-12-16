---
title: Contains Template Filter
linkText: Contains
description: Contains Template Filter
---

# Contains Template Filter

Returns a boolean condition based on if the input value contains the value passed to the filter

## Example

```text
{% raw %}
{% if TestValue|contains:'test' %}Contains Test{% else %}Does NOT Contain Test{% endif %}
{% endraw %}
```
