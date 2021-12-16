---
title: Template Tag
linkText: Template Tag
description: Template Tag
---

# Template Tag

The template tag allows for printing of characters which are part of the Ensconce template syntax.

## Available Tags

| Argument        | Output                    |
|-----------------|---------------------------|
| `openblock`     | `{% raw %}{%{% endraw %}` |
| `closeblock`    | `{% raw %}%}{% endraw %}` |
| `openvariable`  | `{% raw %}{{{% endraw %}` |
| `closevariable` | `{% raw %}}}{% endraw %}` |
| `openbrace`     | `{% raw %}{{% endraw %}`  |
| `closebrace`    | `{% raw %}}{% endraw %}`  |
| `opencomment`   | `{% raw %}{#{% endraw %}` |
| `closecomment`  | `{% raw %}#}{% endraw %}` |

## Example

```text
{% raw %}
{% templatetag openvariable %} Content In Double Braces {% templatetag closevariable  %}
{% endraw %}
```
