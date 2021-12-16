---
title: Template Tag
linkText: Template Tag
description: Template Tag
---

# Template Tag

The template tag allows for printing of characters which are part of the Ensconce template syntax.

Available tags:
| Argument        | Output |
----------------------------
| `openblock`     | `{%`     |
| `closeblock`    | `%}`     |
| `openvariable`  | `{{`     |
| `closevariable` | `}}`     |
| `openbrace`     | `{`      |
| `closebrace`    | `}`      |
| `opencomment`   | `{#`     |
| `closecomment`  | `#}`     |

## Example

```text
{% raw %}
{% templatetag openvariable %} Content In Double Braces {% templatetag closevariable  %}
{% endraw %}
```
