---
title: Template Equality Operators
linkText: Equality Operators
description: Template Equality Operators
---

# Equality

## Equals

Supported symbols are `=` and `==`

## Equals Example

```text
{% raw %}
{% if TestValue = 'Testing' %}Is Testing{% else %}Is Not Testing{% endif %}
{% endraw %}
```

```text
{% raw %}
{% if TestValue == 'Testing' %}Is Testing{% else %}Is Not Testing{% endif %}
{% endraw %}
```

## Not Equals

Supported symbol is `!=`

## Not Equals Example

```text
{% raw %}
{% if TestValue != 'Testing' %}Is Not Testing{% else %}Is Testing{% endif %}
{% endraw %}
```
