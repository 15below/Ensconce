---
title: webHelper.ps1 - DownloadStringUntilOK
linkText: DownloadStringUntilOK
description: Details about the DownloadStringUntilOK function in webHelper.ps1 helper script
---

# DownloadStringUntilOK

```PowerShell
{% raw %}
DownloadStringUntilOK
    [-url] <String>
    [-maxChecks] <int>
    [-sleepSeconds] <int>
    [-okText] <String[]>
    [-failText] <String[]>
{% endraw %}
```

## Description

Polls a url which prints a string and keeps checking (for a configurable number of checks).

It will poll with a configurable sleep between checks.

If the web response is one of the provided `OkText` the process will end.

If the web response is one of the `failText` an exception will be thrown

## Examples

```PowerShell
{% raw %}
DownloadStringUntilOK -url "https://example.com/status" -maxChecks 50 -sleepSeconds 1 -okText @("Passed") -failText @("Failed","Errored")
{% endraw %}
```
