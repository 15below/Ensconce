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

### Non String Inputs

If the input value is not an integer, this will NOT error, but will evaluate as `false`

So, given the above examples, if the value is `Not-A-Number` then then `else` condition will output/execute

### Integer Inputs

If the value being checked is an integer (for example if you have used the length filter) then the conditional value should also be an integer.

```text
{% raw %}
{% if PropertyGroup|length < 1 %}No Instances Of Group{% else %}At Least 1 Instance Of Group{% endif %}
{% endraw %}
```
