---
title: userManagement.ps1 - CheckAndCreateServiceAccount
linkText: CheckAndCreateServiceAccount
description: Details about the CheckAndCreateServiceAccount function in userManagement.ps1 helper script
---

# CheckAndCreateServiceAccount

```PowerShell
{% raw %}
CheckAndCreateServiceAccount
    [-name] <String>
    [-password] <String>
{% endraw %}
```

## Description

Checks if service account already exists and if not creates it.

If the service account does exist, the password will be updated

## Example

```PowerShell
{% raw %}
CheckAndCreateServiceAccount -name "MyUser" -password "Secure_User_Password"
{% endraw %}
```
