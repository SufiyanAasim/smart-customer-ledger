# Authentication Flow

```mermaid
sequenceDiagram
    participant U as User
    participant AC as AccountController
    participant SM as SignInManager<ApplicationUser>
    participant CF as ApplicationClaimsPrincipalFactory
    participant DB as MySQL

    U->>AC: GET /Identity/Account/Login
    AC-->>U: Login view

    U->>AC: POST credentials
    AC->>SM: PasswordSignInAsync(user, password)
    SM->>DB: Verify password hash
    DB-->>SM: OK / Locked out / Invalid

    alt Success
        SM->>CF: GenerateClaimsAsync(user)
        CF-->>SM: ClaimsIdentity + BranchId claim + EmployeeCode claim
        SM-->>AC: SignInResult.Succeeded
        AC->>DB: user.LastLoginAtUtc = UtcNow; SaveChanges
        AC-->>U: Redirect to Dashboard (or ReturnUrl)
    else Locked out
        AC-->>U: "Account locked out" error
    else Invalid
        AC-->>U: "Invalid login attempt" error
    end
```

Every subsequent request reads `BranchId`/role claims directly from the authentication
cookie via `ICurrentUserContext` — no per-request database lookup of the user's branch.
This means a Branch reassignment does not take effect until the user signs in again (see
Known Limitations in `docs/releases/v1.0.0-Index.md`).

There is no public self-registration endpoint — every account is created by an
Administrator under Admin → Users (`UsersController.Create`), consistent with a business
ledger app where only vetted staff should ever hold credentials.
