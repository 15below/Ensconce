---
title: rabbitHelper.ps1 - CreateRabbitVHost
linkText: CreateRabbitVHost
description: Details about the CreateRabbitVHost function in rabbitHelper.ps1 helper script
---

# CreateRabbitVHost

```PowerShell
{% raw %}
CreateRabbitVHost
    [-deployUser] <String>
    [-deployPassword] <String>
    [-rabbitApiUrl] <String>
    [-vHost] <String>
{% endraw %}
```

## Description

Creates a new rabbit vhost

## Example

```PowerShell
{% raw %}
CreateRabbitVHost -deployUser "rabbit-deploy" -deployPassword "Secure_Deploy_Password" -rabbitApiUrl "http://myrabbitserver:15672/api" -vHost "MyRabbitVHost"
{% endraw %}
```
