---
applyTo: "**/*.{css,scss}"
---

# CSS & SCSS Rules

## Modified BEM Naming Convention

Follow a strict Modified BEM naming convention to maintain scalable styles:

- **Block**: Single word (e.g., `.countdown`)
- **Element**: Use a hyphen `-` to separate Block and Element (e.g., `.countdown-title`)
- **Sub-Element**: Use a hyphen `-` to separate Element and Sub-Element (e.g., `.countdown-title-icon`)
- **Multi-word Segment**: Use an underscore `_` to separate words **WITHIN** a single concept/segment (e.g., `.image_upload` or `.scroll_area`)
- **State**: Use HTML data attributes (e.g., `[data-is-active='true']`) instead of state classes like `.is-active`.

**Critical Disambiguation**:
- ❌ NEVER use `__` (double underscore) or `--` (double hyphen).
- ✅ Use hyphen `-` for hierarchy (e.g., `.controls-group` where `group` is inside `controls`).
- ✅ Use underscore `_` for a specific concept that needs two words (e.g., `.scroll_area`).

## CSS Property Order

When writing CSS rules, order properties consistently:

1. **Positioning** (`position`, `top`, `right`, `bottom`, `left`, `z-index`)
2. **Display & Box Model** (`display`, `flex`, `grid`, `width`, `height`, `margin`, `padding`, `border`)
3. **Typography** (`font`, `line-height`, `color`, `text-align`)
4. **Visual** (`background`, `box-shadow`, `border-radius`, `opacity`)
5. **Animation** (`transition`, `animation`)
6. **Misc** (`cursor`, `pointer-events`)
