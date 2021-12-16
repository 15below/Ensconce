---
title: Default Template Filter
linkText: Default
description: Default Template Filter
---

# Default Template Filter

Will use a default value if the input doesn't exist within the tag dictionary.

## Example

```text
{% raw %}
{{ TestValue|default:'test' }}
{% endraw %}
```
