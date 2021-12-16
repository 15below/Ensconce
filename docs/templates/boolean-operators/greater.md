---
title: Template Greater Operators
linkText: Greater Operators
description: Template Greater Operators
---

# Greater

## Greater Than

Supported symbols is `>`

## Greater Than Example

```text
{% if TestValue > '5' %}Is Greater Than 5{% else %}Is Less Than Or Equal To 5{% endif %}
```

## Greater Than Or Equal To

Supported symbols is `>=`

## Greater Than Or Equal To Example

```text
{% if TestValue >= '5' %}Is Greater Than Or Equal To 5{% else %}Is Less Than 5{% endif %}
```

## Important Note

If the input value is not an integer, this will NOT error, but will evaluate as `false`

So, given the above examples, if the value is `Not-A-Number` then then `else` condition will output/execute
