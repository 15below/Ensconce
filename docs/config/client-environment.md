---
title: Client & Environment
description: Specifying a client and environment in deployment configuration
linkText: Client & Environment
---

# Client & Environment

## Overview

The deployment configuration file has a `ClientCode` and `Environment` node.

At 15below we use [Octopus Deploy](https://octopus.com){:.link-secondary} which means the `Environment` is populated by the deployment environment rather than the deployment configuration file.

We also set a `ClientCode` value as part of the Octopus project and name the files `[ClientCode]-[Environment]-Config.xml`.

But if you are not using Octopus, the `ClientCode` and `Environment` nodes can be useful for setting up these default values.

## Example

```XML
{% raw %}
<Structure xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
  <ClientCode>AAA</ClientCode>
  <Environment>PRD</Environment>
  ...
</Structure>
{% endraw %}
```
