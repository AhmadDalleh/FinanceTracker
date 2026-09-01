# Personal Finance Tracker — Feature List

**Priority legend**
- 🟢 Core — MVP, build first
- 🟡 Should-have — v2, adds real-world depth
- 🔵 Stretch — nice-to-have, only if time allows

---

## 1. Auth & Multi-user
- 🟢 Register / Login (JWT-based)
- 🟢 Per-user data isolation
- 🟢 Password reset flow
- 🟡 Household / shared accounts (multiple users, one set of accounts)
- 🔵 Admin role (user management, system stats)

## 2. Account Management
- 🟢 Multiple accounts per user (checking, savings, credit card, cash, investment)
- 🟢 Account balance auto-calculated from transactions
- 🟡 Multi-currency support per account
- 🟡 Account balance history / snapshot over time (for net-worth charts)
- 🟡 Archive/close an account (soft delete, retain history)

## 3. Transactions
- 🟢 CRUD: amount, date, type (income/expense/transfer), category, note
- 🟢 Search/filter by date range, category, amount range, account
- 🟡 Transfers between accounts (linked pair, excluded from income/expense totals)
- 🟡 Recurring transactions (rent, salary, subscriptions) with auto-generation job
- 🟡 Attachments (receipt photo/PDF per transaction)
- 🔵 Split transactions (one purchase, multiple categories)
- 🔵 Bulk import from CSV / bank statement export

## 4. Categories & Tags
- 🟢 User-defined categories, default set seeded on signup
- 🟡 Parent/subcategory hierarchy (Food > Groceries)
- 🔵 Free-form tags in addition to categories (e.g. "trip-to-turkey")

## 5. Budgeting
- 🟢 Monthly budget per category
- 🟢 Budget vs actual comparison view
- 🟡 Overspend alert (in-app)
- 🔵 Budget rollover toggle (unused amount carries to next month)

## 6. Reporting & Analytics
- 🟢 Monthly income vs expense summary
- 🟢 Spend-by-category breakdown (chart-ready aggregate endpoint)
- 🟡 Net worth over time
- 🟡 Custom date-range reports
- 🔵 Export to PDF/Excel

## 7. Goals & Savings
- 🟡 Savings goals (target amount, target date, linked account)
- 🟡 Progress tracking toward a goal

## 8. Notifications & Automation
- 🟡 Budget-overrun alert via email
- 🔵 n8n webhook integration (Telegram/email digest)
- 🔵 Weekly/monthly digest email
- 🔵 Recurring transaction reminders

## 9. Security & Reliability
- 🟢 FluentValidation on all commands
- 🟢 Global exception handling middleware, consistent error response shape
- 🟡 Rate limiting
- 🟡 Audit fields (CreatedAt/By, UpdatedAt/By) on all entities

## 10. Testing & DevOps
- 🟢 Unit tests on MediatR handlers (xUnit)
- 🟡 Integration tests via Testcontainers + Postgres
- 🟡 Dockerized (API + DB + Angular, docker-compose)
- 🟡 CI pipeline (build/test/lint on PR — GitHub Actions)
- 🔵 Deployed demo (Render/Railway/Azure)

## 11. Nice-to-have / Stretch
- 🔵 Dark mode / theming
- 🔵 Mobile-responsive layout
- 🔵 Live currency conversion API
- 🔵 Open banking API sync (flagged for awareness only — out of scope for v1)

---

**Suggested v1 scope:** everything marked 🟢 — a working, testable, deployable app without ballooning the timeline.
**🟡 items** are what push it from "CRUD demo" to "real world project."
**🔵 items** are optional polish, add only if time allows.
