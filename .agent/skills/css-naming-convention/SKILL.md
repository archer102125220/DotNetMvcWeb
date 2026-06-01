---
name: css-naming-convention
description: BEM-like CSS naming convention rules for styling.
---

# css-naming-convention

When writing global CSS in `wwwroot/css/` or scoped CSS for Razor Views, use the modified BEM convention.

## Rules
- **Block**: `.countdown` (Single word)
- **Element**: `.countdown-title` (hyphen `-` separates Block-Element)
- **Sub-Element**: `.countdown-title-icon` (hyphen `-` separates Element-SubElement)
- **Multi-word Segment**: `.image_upload` (underscore `_` separates words **WITHIN** a single segment)
- **State**: `[data-is-active='true']` (HTML data attribute)

## Hierarchy vs Segments
- **Hyphen (`-`)**: Use for structural hierarchy (e.g. `.card-body`).
- **Underscore (`_`)**: Use for single concepts needing two words (e.g. `.scroll_area`).
