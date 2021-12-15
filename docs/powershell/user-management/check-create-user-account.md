---
title: userManagement.ps1 - CheckAndCreateUserAccount
linkText: CheckAndCreateUserAccount
description: Details about the CheckAndCreateUserAccount function in userManagement.ps1 helper script
---

# CheckAndCreateUserAccount

```PowerShell
{% raw %}
CheckAndCreateUserAccount
    [-name] <String>
    [-password] <String>
{% endraw %}
```

## Description

Checks if user account already exists and if not creates it.

If the user account does exist, the password will be updated

## Examples

```PowerShell
{% raw %}
CheckAndCreateUserAccount -name "MyUser" -password "Secure_User_Password"
{% endraw %}
```
