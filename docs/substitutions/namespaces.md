---
title: Namespaces
linkText: Namespaces
description: Including namespace definition for XML file substitution
---

# Namespaces

If the XML file you are trying to substitute contains namespaces you can provide Ensconce the details of these through the namespace definitions.

This means that your XPaths can contain the correct namespace prefix.

## Example

```XML
{% raw %}
<?xml version="1.0" encoding="utf-8"?>
<Root xmlns="http://15below.com/Substitutions.xsd">
    <Namespaces>
        <Namespace Prefix="x">
            http://15below.com/Substitutions.xsd
        </Namespace>
    </Namespaces>
    ...
</Root>
{% endraw %}
```
