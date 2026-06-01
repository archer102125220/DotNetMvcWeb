# CSS Property Order

Properties MUST be ordered in this sequence:

1. **Positioning** (position, top, left, right, bottom, z-index)
2. **Display & Box Model** (display, flex, grid, width, height, margin, padding, border)
3. **Typography** (font, color, text-align, line-height)
4. **Visual** (background, box-shadow, opacity, border-radius)
5. **Animation** (transition, animation, transform)
6. **Misc** (cursor, content, pointer-events)

## Example

```scss
.example {
  /* Positioning */
  position: relative;
  top: 0;
  z-index: 10;

  /* Display & Box Model */
  display: flex;
  width: 100%;
  padding: 20px;
  border: 1px solid #ccc;

  /* Typography */
  font-size: 16px;
  color: #333;

  /* Visual */
  background-color: #fff;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);

  /* Animation */
  transition: all 0.3s;

  /* Misc */
  cursor: pointer;
}
```
