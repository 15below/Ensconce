---
title: Reports Role
description: How to deploy reporting role for a user
---

# Reports Role

## Overview

Ensconce can configure reports being deployed to SQL Server Reporting Services (SSRS).

Ensconce can setup a specific domain user to have SSRS security permissons on a item or folder of items.

## Syntax

`--drr --rsv reportingServicesUrl=[urlToReportingServices] --rsv networkDomain=[authenticationDomain] --rsv networkLogin=[authenticationUser] --rsv networkPassword=[authenticationPassword] --rsv itemPath=[pathToAddRoleTo] --rsv reportingUserToAddRoleFor=[userToAdd] --rsv reportingRoleToAdd=[roleToAdd]`
