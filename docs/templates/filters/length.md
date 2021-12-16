---
title: Length Template Filter
linkText: Length
description: Length Template Filter
---

# Length Template Filter

Get the length of a collection of items

## Example

```text
{% raw %}
{% if PropertyGroup|length > 1 %}More Than 1 Instances Of Group{% else %}1 Or Less Instances Of Group{% endif %}
{% endraw %}
```
