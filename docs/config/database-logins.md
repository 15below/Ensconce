---
title: Database Logins
description: Specifying database logins as part of your deployment configuration
linkText: Database Logins
---

# Database Logins

## Overview

Database logins can be provided in a special section specifying the username, default database & password.

It's also possible to specify an entire database connection string.

If no connection string is populated, you should have a general property called `DbServer`.

The connection string will become `Data Source=[ DbServer ]; Initial Catalog=[ DefaultDb ]; User ID=[ Name ]; Password=[ Password ];`

## Example

```XML
{% raw %}
<Structure xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
  ...
  <DbLogins>
    <DbLogin>
      <Name>LoginName</Name>
      <DefaultDb>master</DefaultDb>
      <Password>NoPasswordsRoundHere</Password>
    </DbLogin>
    <DbLogin>
      <Name>AnotherLoginName</Name>
      <ConnectionString>ConnectionStringHere</ConnectionString>
    </DbLogin>
  </DbLogins>
</Structure>
{% endraw %}
```
