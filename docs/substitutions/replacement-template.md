---
title: Replacement Template
linkText: Replacement Template
description: The `ReplacementTemplate` node and use case
---

# Replacement Template

Using the `ReplacementTemplate` options means that Ensconce will read one file, apply the substitution to it but save it under a different file name.

This means you have the ability to read a single file, but save that file multiple times with slightly different content.

## Example

### Base File

```XML
{% raw %}
<root>
    <lookupValue></lookupValue>
</root>
{% endraw %}
```

### Substitution

```XML
{% raw %}
<Files>
    <File Filename="PATH\File1.xml">
        <ReplacementTemplate>PATH\BaseFile.xml</ReplacementTemplate>
        <Changes>
            <Change type="ChangeValue" xPath="/root/lookupValue" value="File1" />
        </Changes>
    </File>
    <File Filename="PATH\File2.xml">
        <ReplacementTemplate>PATH\BaseFile.xml</ReplacementTemplate>
        <Changes>
            <Change type="ChangeValue" xPath="/root/lookupValue" value="File2" />
        </Changes>
    </File>
</Files>
{% endraw %}
```

### Output (File1)

```XML
{% raw %}
<root>
    <lookupValue>File1</lookupValue>
</root>
{% endraw %}
```

### Output (File2)

```XML
{% raw %}
<root>
    <lookupValue>File2</lookupValue>
</root>
{% endraw %}
```
