---
title: Tag Dictionary Extraction
description: How to extract the entire Ensconce tag dictionary
linkText: Tag Dictionary Extraction
---

# Tag Dictionary Extraction

## Overview

There may be cases when you want to save the entire tag dictionary Ensconce has loaded as a JSON file, or post this JSON to a REST endpoint.

___Warning___ The data being saved or exported will not be encrypted

## Example

### Save To Path

```powershell
ensconce --dictionarySavePath=[pathToJsonFile]
```

### Post To REST Endpoint

```powershell
ensconce --dictionaryPostUrl=[urlToPostJsonTo]
```
