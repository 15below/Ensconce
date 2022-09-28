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

## For Loop Variables

For loops have context variables which can be used

| Variable              | Description                                                   |
|-----------------------|---------------------------------------------------------------|
| `forloop.counter`     | The current iteration of the loop (1-indexed)                 |
| `forloop.counter0`    | The current iteration of the loop (0-indexed)                 |
| `forloop.revcounter`  | The number of iterations from the end of the loop (1-indexed) |
| `forloop.revcounter0` | The number of iterations from the end of the loop (0-indexed) |
| `forloop.first`       | True if this is the first time through the loop               |
| `forloop.last`        | True if this is the last time through the loop                |


These can be used to extend the example above

```text
{% raw %}
$instances = ("{% for instance in PropertyGroupLabel %}{{ instance.identity }}{% if not forloop.last %};{% endif %}{% endfor %}" | ensconce -i).Split(";") | Where-Object { $_ -ne "" }
{% endraw %}
```

Which will not include the `;` at the end and only between each item.
