---
title: Encrypt Output - encryptValueCert
linkText: encryptValueCert
description: Details of `encryptValueCert` condition that can be applied to changes
---

# `encryptValueCert` Condition

## Compatible With

The `encryptValueCert` condition can be applied to all change types.

## Description

The `encryptValueCert` attribute can be added to a change with a template string as it's value.

The value returned will be encrypted with the provided certificate

## Example

```XML
{% raw %}
<Change type="ChangeValue" jsonPath="$.Logging.LogLevel.Default" value="Information" encryptValueCert="MyCertificate" />
{% endraw %}
```
