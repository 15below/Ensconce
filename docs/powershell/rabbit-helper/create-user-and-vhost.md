---
title: rabbitHelper.ps1 - CreateRabbitUserAndVHost
linkText: CreateRabbitUserAndVHost
description: Details about the CreateRabbitUserAndVHost function in rabbitHelper.ps1 helper script
---

# CreateRabbitUserAndVHost

```PowerShell
{% raw %}
CreateRabbitUserAndVHost
    [-deployUser] <String>
    [-deployPassword] <String>
    [-rabbitApiUrl] <String>
    [-user] <String>
    [-password] <String>
    [-vHost] <String>
{% endraw %}
```

## Description

Creates a new rabbit user and vhost

## Example

```PowerShell
{% raw %}
CreateRabbitUserAndVHost -deployUser "rabbit-deploy" -deployPassword "Secure_Deploy_Password" -rabbitApiUrl "http://myrabbitserver:15672/api" -user "MyRabbitUser" -password "Secure_User_Password" -vHost "MyRabbitVHost"
{% endraw %}
```
