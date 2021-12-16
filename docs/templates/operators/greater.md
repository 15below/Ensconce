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
{% raw %}
{% if TestValue > '5' %}Is Greater Than 5{% else %}Is Less Than Or Equal To 5{% endif %}
{% endraw %}
```

## Greater Than Or Equal To

Supported symbols is `>=`

## Greater Than Or Equal To Example

```text
{% raw %}
{% if TestValue >= '5' %}Is Greater Than Or Equal To 5{% else %}Is Less Than 5{% endif %}
{% endraw %}
```

## Important Notes

### Non String Inputs

If the input value is not an integer, this will NOT error, but will evaluate as `false`

So, given the above examples, if the value is `Not-A-Number` then then `else` condition will output/execute

### Integer Inputs

If the value being checked is an integer (for example if you have used the length filter) then the conditional value should also be an integer.

```text
{% raw %}
{% if PropertyGroup|length > 1 %}More Than 1 Instances Of Group{% else %}1 Or Less Instances Of Group{% endif %}
{% endraw %}
```
