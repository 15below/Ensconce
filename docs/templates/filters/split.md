---
title: Split Template Filter
linkText: Split
description: Split Template Filter
---

# Split Template Filter

Splits a string into an array using the specified delimiter character

## Example

```text
{% raw %}
{% for server in 'web01,web02,web03'|split:',' %}
  Server: {{ server }}
{% endfor %}
{% endraw %}
```
