---
title: rabbitHelper.ps1 - AddUserToVHost
linkText: AddUserToVHost
description: Details about the AddUserToVHost function in rabbitHelper.ps1 helper script
---

# AddUserToVHost

```PowerShell
{% raw %}
AddUserToVHost
    [-deployUser] <String>
    [-deployPassword] <String>
    [-rabbitApiUrl] <String>
    [-user] <String>
    [-vHost] <String>
{% endraw %}
```

## Description

Adds an existing user to an existing virtual host.

## Example

```PowerShell
{% raw %}
AddUserToVHost -deployUser "rabbit-deploy" -deployPassword "Secure_Deploy_Password" -rabbitApiUrl "http://myrabbitserver:15672/api" -user "MyRabbitUser" -vHost "MyRabbitVHost"
{% endraw %}
```
