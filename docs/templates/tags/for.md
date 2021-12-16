---
title: For Tag
linkText: For
description: For Tag
---

# For Tags

Used for iterating over sections of the template tags.

Can be used to pull values from config into a powershell variable.

## Example

```text
{% raw %}
$instances = ("{% for instance in PropertyGroupLabel %}{{ instance.identity }};{% endfor %}" | ensconce -i).Split(";") | Where-Object { $_ -ne "" }
{% endraw %}
```
