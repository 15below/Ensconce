---
title: Template Less Operators
linkText: Less Operators
description: Template Less Operators
---

# Less

## Less Than

Supported symbols is `<`

## Less Than Example

```text
{% raw %}
{% if TestValue < '5' %}Is Less Than 5{% else %}Is Greater Than Or Equal To 5{% endif %}
{% endraw %}
```

## Less Than Or Equal To

Supported symbols is `<=`

## Less Than Or Equal To Example

```text
{% raw %}
{% if TestValue <= '5' %}Is Less Than Or Equal To 5{% else %}Is Greater Than 5{% endif %}
{% endraw %}
```

## Important Note

If the input value is not an integer, this will NOT error, but will evaluate as `false`

So, given the above examples, if the value is `Not-A-Number` then then `else` condition will output/execute