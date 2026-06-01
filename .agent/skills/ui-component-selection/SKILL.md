---
name: ui-component-selection
description: Decision tree for selecting between PartialViews and ViewComponents in .NET MVC.
---

# ui-component-selection

When extracting reusable UI components in Razor, follow this decision tree to pick the right technology.

## 1. Do I need backend data fetching independent of the parent Controller?
- **YES** ➔ Use a **ViewComponent**. (e.g., A shopping cart dropdown that fetches from the DB, independent of whether the user is on the Home page or Profile page).
- **NO** ➔ Go to Step 2.

## 2. Is this just a static HTML template or simple data formatter?
- **YES** ➔ Use a **PartialView**. Pass the required Model data from the parent view (`@await Html.PartialAsync("_UserInfo", Model.User)`).
- **NO** ➔ Go to Step 3.

## 3. Does this UI fragment represent a form that will be posted via HTMX?
- **YES** ➔ Use a **PartialView**. The Controller handling the POST will typically return this same PartialView to update the DOM with validation errors or a success message.

## Summary
- **PartialView**: Dumb components. Rely on data passed to them. Great for HTMX responses.
- **ViewComponent**: Smart components. They have their own C# class (`InvokeAsync`), inject their own services, and fetch their own data.
