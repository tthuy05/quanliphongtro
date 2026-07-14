# Security checklist

- [x] Identity cookie HttpOnly, SameSite, Secure production, finite lifetime.
- [x] Unique email, password policy, lockout and generic login failure message.
- [x] Role constants centralized; protected endpoints use `[Authorize]`.
- [x] Critical resource queries check owner/member, including management detail and evidence download.
- [ ] Exhaustive cross-owner/cross-tenant HTTP ID matrix is still a release-hardening item; critical service scope and role-route denial are automated.
- [x] All state-changing MVC actions POST + anti-forgery.
- [x] Dedicated InputModels prevent overposting; server validates cross-field/state.
- [x] EF parameterization only; no SQL built from user input.
- [x] Razor output encoding retained; no untrusted `Html.Raw` except serialized server model.
- [x] Upload MIME, extension, signature where practical, size and safe generated name checked.
- [x] Files stored outside web root and served through authorized endpoint.
- [x] Password/token/identity/connection string/file content never logged.
- [x] Production exception handler hides stack/database errors; security headers and HTTPS/HSTS enabled.
- [x] Secrets supplied through environment/user-secrets; sample config contains placeholders only.
- [x] Login rate limiting and Identity lockout enabled.
- [x] Financial history uses restrict/no hard delete; payment confirmation transaction prevents race/overpay.
- [x] npm/NuGet dependency vulnerability audit and migration source/model review completed.
- [ ] Apply/inspect migrations on PostgreSQL and run the production container before release; unavailable on the audit machine.

Final evidence and remaining boundaries are recorded in `24-PROJECT-COMPLETION-AUDIT.md` and `26-FINAL-PROJECT-IMPLEMENTATION-REPORT.md`.
