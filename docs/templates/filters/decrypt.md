---
title: Decrypt Template Filter
linkText: Decrypt
description: Decrypt Template Filter
---

# Decrypt Template Filter

Will decrypt the input value using the provided certificate

Useful when coupled with `encrypt` as can decrypt with 1 certificate and then re-encrypt with a different certificate

Also useful to use a value which is stored as encrypted as part of your deployment. - for example a user password which is stored encrypted, but can be decrypted in a script to then create/update the user on a machine.

## Example

```text
{% raw %}
{{ TestValue|decrypt:'MyCertificate' }}
{% endraw %}
```

```text
{% raw %}
{{ TestValue|decrypt:'MyCertificate'|encrypt:'MyOtherCertificate' }}
{% endraw %}
```
