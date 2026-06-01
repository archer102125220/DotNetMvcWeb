# Database Migration

Generate or modify Entity Framework Core (EF Core) database migrations following project standards.

## Usage

Use this command when you need to:
- Create new database tables (via Entity classes)
- Modify existing schema
- Add/remove columns
- Create relationships or indexes
- Seed data

## ⚠️ CRITICAL: Production Check

**BEFORE ANY schema change, you MUST ask**:
> "Is this project deployed to production?"

- **Not deployed**: You may drop the database (`dotnet ef database drop`), remove the last unapplied migration (`dotnet ef migrations remove`), or delete DB and apply.
- **Deployed**: NEVER modify existing executed migrations, ALWAYS create NEW migration files (`dotnet ef migrations add`).

## Template

Please create a database migration for:

**Change Type**:
- [ ] Create new entity (table)
- [ ] Add column(s)
- [ ] Modify column type or constraints
- [ ] Add index / foreign key
- [ ] Seed data

**Details**:
- **Entity**: [ClassName]
- **Changes**: [describe changes]
- **Reason**: [why this change is needed]

**Migration Requirements**:
- ✅ Update the EF Core Entity Model in `Models/Entities/`
- ✅ Update the `AppDbContext` (if adding a new `DbSet`)
- ✅ Use EF Core Code-First approach
- ✅ Check for Nullable reference types (`string?` vs `string`)

**Commands to Run**:
```bash
# Generate migration
dotnet ef migrations add [MigrationName]

# Run migration
dotnet ef database update

# Rollback (if needed)
dotnet ef migrations remove
```

## Example

```
Please create a database migration for:

**Change Type**:
- [x] Add column(s)

**Details**:
- **Entity**: User
- **Changes**: Add `Role` column (enum: Admin, User, Guest)
- **Reason**: Implement role-based access control

**Additional Requirements**:
- Default value: User
- Required field (NOT NULL)
```

## EF Core Configuration Patterns

### Create Entity
```csharp
public class User
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Add Foreign Key / Relationship
```csharp
public class Post
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; } = null!; // Navigation property
}
```

## Related Skills
- [Backend ORM Best Practices](../rules/backend-orm.md)
- [C# Standards](../rules/csharp-standards.md)
