---
title: Template Syntax
description: The home of template syntax documentation
linkText: Template Syntax
---
# Template Syntax

## Overview

Ensconce uses a template language based on a version on Django.

This was ported to .net and later brought directly into Ensconce when support for the port ceased.

The port is based on [version 1.8](https://docs.djangoproject.com/en/1.8/ref/templates/builtins/#built-in-template-tags-and-filters){:.link-secondary} of Django, though not all functionality was ported.

To verify support, this section will detail the known working template syntax elements.

These are broken down into 3 sections

{% include childPages.html %}

## General Principals

### Content Tags

A tag starts with `{% raw %}{{{% endraw %}` and ends with `{% raw %}}}{% endraw %}`.  For example `{% raw %}{{ MyValue }}{% endraw %}`

Tags can be tiers to include property groups whereby they syntax is `{% raw %}{{ Label.Identity.Property }}{% endraw %}`

### Content Tags With Filters

Filters are applied using a `|` and a `:` for any inputs to the filter.  For example `{% raw %}{{ MyValue|default:'DefaultValue' }}{% endraw %}`

### Other Tags

Other tags such as `if` statements and `for` loops start with `{% raw %}{%{% endraw %}` and end with `{% raw %}%}{% endraw %}`. for example `{% raw %}{% if MyValue = 'test' %}True{% else %}False{% endif %}{% endraw %}`.
