---
title: Encrypt Template Filter
linkText: Encrypt
description: Encrypt Template Filter
---

# Encrypt Template Filter

Will encrypt the input value using the provided certificate

## Example

```text
{% raw %}
{{ TestValue|encrypt:'MyCertificate' }}
{% endraw %}
```
