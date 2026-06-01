# HTMX & Razor View Workflow Checklist

This is the standard workflow to follow when implementing a new HTMX interaction in the .NET MVC project.

## Step 1: The Razor View (Client Side)
- [ ] Added `hx-get` or `hx-post` pointing to the correct Controller Action.
- [ ] Added `hx-target` specifying which DOM element will be replaced.
- [ ] Added `hx-swap` (usually `outerHTML` or `innerHTML`).
- [ ] Included CSRF token if performing a POST request.

## Step 2: The Controller (Server Side)
- [ ] Created the Action method in the Controller.
- [ ] Added `[ValidateAntiForgeryToken]` if it's a POST request.
- [ ] Processed the business logic.
- [ ] **CRITICAL**: Checked if it's an HTMX request (`Request.Headers["HX-Request"]`).
- [ ] Returned `PartialView("_MyPartial", model)` (NOT a full `View()`).

## Step 3: The Partial View (Response)
- [ ] Created the `_MyPartial.cshtml` file in `Views/Shared/` or `Views/ControllerName/`.
- [ ] Ensured the Partial View does NOT include the layout (it should just be the fragment).
- [ ] Validated that the outer element in the Partial View matches the ID of the `hx-target` if using `outerHTML` swap.

## Step 4: Verification
- [ ] Run `dotnet build`.
- [ ] Verify that clicking the button/link does NOT cause a full page refresh (check Network tab in DevTools for XHR request).
- [ ] Verify no layout duplication (header inside a header) occurred.
