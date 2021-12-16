---
title: JSON Change Value
linkText: Change Value
description: Change value JSON format
---

# JSON Change Value

## Description

Updates the value of a JSON path

## Example

### Base File

```json
{% raw %}
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace"
    }
  }
}
{% endraw %}
```

### Substitution

```XML
{% raw %}
<Change type="ChangeValue" jsonPath="$.Logging.LogLevel.Default" value="Debug" />
{% endraw %}
```

### Output

```json
{% raw %}
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
{% endraw %}
```
