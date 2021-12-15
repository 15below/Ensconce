---
title: rabbitHelper.ps1 - CreateRabbitUser
linkText: CreateRabbitUser
description: Details about the CreateRabbitUser function in rabbitHelper.ps1 helper script
---

# CreateRabbitUser

```PowerShell
{% raw %}
CreateRabbitUser
    [-deployUser] <String>
    [-deployPassword] <String>
    [-rabbitApiUrl] <String>
    [-user] <String>
    [-password] <String>
{% endraw %}
```

## Description

Creates a new rabbit user

## Example

```PowerShell
{% raw %}
CreateRabbitUser -deployUser "rabbit-deploy" -deployPassword "Secure_Deploy_Password" -rabbitApiUrl "http://myrabbitserver:15672/api" -user "MyRabbitUser" -password "Secure_User_Password"
{% endraw %}
```
