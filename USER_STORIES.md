# Personal Finance Tracker — User Stories

Format: `US-XXX: As a [role], I want to [goal], so that [benefit].`
Each story includes brief acceptance criteria (AC).

---

## Role: Guest (Unauthenticated Visitor)

**US-001** — As a guest, I want to register an account with email and password, so that I can start tracking my finances.
- AC: Email must be unique and a valid format
- AC: Password meets minimum complexity rules
- AC: On success, user is redirected to login or auto-logged in

**US-002** — As a guest, I want to log in with my credentials, so that I can access my data.
- AC: Invalid credentials show a clear error, no account enumeration
- AC: Successful login issues a JWT access token (+ refresh token)

**US-003** — As a guest, I want to request a password reset, so that I can regain access if I forget my password.
- AC: Reset link sent to registered email, expires after a set time
- AC: Reset link can only be used once

**US-004** — As a guest, I want to see a landing page describing the app, so that I understand what it offers before signing up.
- AC: Page lists core features and a call-to-action to register

---

## Role: Registered User (primary role)

### Account Management

**US-010** — As a user, I want to create multiple accounts (checking, savings, credit card, cash), so that I can track balances separately.
- AC: Account requires a name, type, starting balance, currency
- AC: Account list shows current calculated balance

**US-011** — As a user, I want to edit or archive an account, so that I can keep my account list accurate without losing history.
- AC: Archived accounts are hidden from active lists but transactions remain queryable

**US-012** — As a user, I want to see my account balance history over time, so that I can understand how my finances are trending.
- AC: Balance snapshot chart shows daily/weekly/monthly points

### Transactions

**US-020** — As a user, I want to record a transaction (amount, date, category, account, note), so that my spending/income is tracked accurately.
- AC: Amount must be positive, type determines sign in aggregates
- AC: Transaction instantly updates the account's calculated balance

**US-021** — As a user, I want to edit or delete a transaction, so that I can correct mistakes.
- AC: Edits recalculate balances; deletes require confirmation

**US-022** — As a user, I want to filter/search transactions by date range, category, account, and amount, so that I can find specific entries quickly.
- AC: Filters combine (AND logic), results paginated

**US-023** — As a user, I want to set up a recurring transaction (e.g. rent, salary), so that I don't have to enter it manually every month.
- AC: Recurring rule generates a transaction automatically on schedule
- AC: User can pause/cancel a recurring rule

**US-024** — As a user, I want to attach a receipt photo/PDF to a transaction, so that I have proof of purchase stored with the record.
- AC: Upload limited to image/PDF, reasonable size cap

**US-025** — As a user, I want to record a transfer between two of my own accounts, so that moving money isn't double-counted as income/expense.
- AC: Transfer creates a linked debit/credit pair, excluded from income/expense totals

**US-026** — As a user, I want to import transactions from a CSV/bank export, so that I don't have to manually enter historical data.
- AC: Import maps columns, flags duplicates, allows category assignment before confirming

### Categories & Tags

**US-030** — As a user, I want to create and manage my own categories, so that my spending breakdown reflects how I actually think about money.
- AC: Default categories seeded on signup, fully editable/deletable

**US-031** — As a user, I want subcategories (e.g. Food > Groceries), so that I can report at different levels of detail.
- AC: A subcategory always belongs to exactly one parent

### Budgeting

**US-040** — As a user, I want to set a monthly budget per category, so that I can control my spending.
- AC: Budget is scoped to a category + month
- AC: System recalculates budget usage as transactions are added

**US-041** — As a user, I want to see budget vs actual spend for the current month, so that I know where I stand.
- AC: Progress indicator per category, flags categories over 100%

**US-042** — As a user, I want to be alerted when I'm about to exceed a budget, so that I can adjust my spending in time.
- AC: Alert triggers at a configurable threshold (e.g. 90%)

### Reporting & Analytics

**US-050** — As a user, I want a monthly summary of income vs expenses, so that I can see my net position at a glance.

**US-051** — As a user, I want a spend-by-category breakdown chart, so that I can see where my money goes.

**US-052** — As a user, I want to see my net worth trend over time, so that I can track long-term financial progress.

**US-053** — As a user, I want to export a report to PDF/Excel, so that I can keep records outside the app or share them.

### Goals & Savings

**US-060** — As a user, I want to create a savings goal with a target amount and date, so that I can work toward something specific.

**US-061** — As a user, I want to see progress toward each goal, so that I stay motivated.

### Notifications

**US-070** — As a user, I want to receive an email when I exceed a budget, so that I'm notified even when I'm not in the app.

**US-071** — As a user, I want to opt into a weekly digest email, so that I get a regular summary without checking manually.

---

## Role: Household Member (shared accounts — v2 scope)

**US-080** — As a household owner, I want to invite another user to share specific accounts, so that we can track shared finances together.
- AC: Invite sent via email, invitee must accept before gaining access

**US-081** — As a household member, I want to see and add transactions on shared accounts, so that both of us can log spending.
- AC: Shared account transactions show which member logged them

**US-082** — As a household owner, I want to revoke a member's access to a shared account, so that I control who sees our finances.

---

## Role: Admin (system administration — stretch scope)

**US-090** — As an admin, I want to view a list of registered users, so that I can monitor system usage.

**US-091** — As an admin, I want to deactivate a user account, so that I can respond to abuse or support requests.

**US-092** — As an admin, I want to see basic system health metrics (request volume, error rate), so that I can catch issues early.
