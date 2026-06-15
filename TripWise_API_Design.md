# TripWise – API Design & Architecture Documentation

---

## Project Architecture (Solution Structure)

```
TripWise.sln
│
├── TripWise.API                  → ASP.NET Core Web API (Controllers, Middleware, Program.cs)
├── TripWise.Application          → Class Library (Use Cases / Services / DTOs / Interfaces)
├── TripWise.Domain               → Class Library (Entities, Enums, Domain Logic)
└── TripWise.Infrastructure       → Class Library (EF Core, Repositories, DB Context, Migrations)
```

### Layer Responsibilities

| Layer | Project Type | Responsibility |
|---|---|---|
| API | ASP.NET Web API | Controllers, Auth Middleware, Request/Response |
| Application | Class Library | Business logic, Services, DTOs, Interface definitions |
| Domain | Class Library | Entities, Enums, Domain rules (no dependencies) |
| Infrastructure | Class Library | EF Core DbContext, Repositories, Migrations |

### Dependency Flow
```
API → Application → Domain
Infrastructure → Application → Domain
```

---

## Database Tables Overview

| Table | Description |
|---|---|
| Users | App users |
| Trips | Trip records |
| TripMembers | Trip participants |
| BudgetPlans | Planned budgets per trip |
| BudgetCategories | Budget per category |
| Expenses | Actual expenses |
| ExpenseSplits | How expense is split among members |
| Settlements | Who pays whom |
| WalletContributions | Advance contributions per member |
| Notifications | User notifications |
| AuditLogs | Admin audit trail |

---

## MODULE 1 – User Management

### Tables

**Users**
| Column | Type | Notes |
|---|---|---|
| UserId | UNIQUEIDENTIFIER | PK |
| FullName | NVARCHAR(100) | |
| Email | NVARCHAR(150) | Unique |
| PhoneNumber | NVARCHAR(15) | |
| PasswordHash | NVARCHAR(255) | |
| Role | NVARCHAR(20) | User / Admin |
| ProfilePicture | NVARCHAR(300) | URL |
| IsActive | BIT | |
| CreatedAt | DATETIME | |

---

### APIs

#### POST `/api/auth/register`
- Purpose: Register a new user
- Request: `FullName, Email, PhoneNumber, Password`
- Response: `UserId, Token`

#### POST `/api/auth/login`
- Purpose: Authenticate user and return JWT token
- Request: `Email, Password`
- Response: `Token, UserId, Role`

#### POST `/api/auth/forgot-password`
- Purpose: Send password reset link to email
- Request: `Email`

#### POST `/api/auth/reset-password`
- Purpose: Reset password using token
- Request: `ResetToken, NewPassword`

#### GET `/api/users/{userId}`
- Purpose: Get user profile
- Response: `UserId, FullName, Email, PhoneNumber, ProfilePicture`

#### PUT `/api/users/{userId}`
- Purpose: Update user profile
- Request: `FullName, PhoneNumber, ProfilePicture`

---

## MODULE 2 – Trip Planning

### Tables

**Trips**
| Column | Type | Notes |
|---|---|---|
| TripId | UNIQUEIDENTIFIER | PK |
| CreatedByUserId | UNIQUEIDENTIFIER | FK → Users |
| TripName | NVARCHAR(150) | |
| Destination | NVARCHAR(200) | |
| StartDate | DATE | |
| EndDate | DATE | |
| Description | NVARCHAR(500) | |
| TripType | NVARCHAR(50) | Solo / Group / Family |
| Status | NVARCHAR(20) | Active / Completed / Cancelled |
| CreatedAt | DATETIME | |

**TripMembers**
| Column | Type | Notes |
|---|---|---|
| TripMemberId | UNIQUEIDENTIFIER | PK |
| TripId | UNIQUEIDENTIFIER | FK → Trips |
| UserId | UNIQUEIDENTIFIER | FK → Users |
| Role | NVARCHAR(20) | Admin / Member |
| JoinedAt | DATETIME | |

---

### APIs

#### POST `/api/trips`
- Purpose: Create a new trip
- Request: `TripName, Destination, StartDate, EndDate, Description, TripType`

#### GET `/api/trips`
- Purpose: Get all trips for logged-in user
- Response: List of trips with status

#### GET `/api/trips/{tripId}`
- Purpose: Get trip details
- Response: Trip info + members + financial summary

#### PUT `/api/trips/{tripId}`
- Purpose: Update trip details
- Request: `TripName, Destination, StartDate, EndDate, Description`

#### DELETE `/api/trips/{tripId}`
- Purpose: Delete/cancel a trip

#### GET `/api/trips/{tripId}/dashboard`
- Purpose: Trip dashboard – budget, actual expense, settlements, recent activity
- Response: Budget, ActualExpense, RemainingBudget, PayableAmount, ReceivableAmount, RecentExpenses

---

## MODULE 3 – Budget Planning

### Tables

**BudgetPlans**
| Column | Type | Notes |
|---|---|---|
| BudgetPlanId | UNIQUEIDENTIFIER | PK |
| TripId | UNIQUEIDENTIFIER | FK → Trips |
| TotalBudget | DECIMAL(10,2) | |
| CreatedAt | DATETIME | |

**BudgetCategories**
| Column | Type | Notes |
|---|---|---|
| BudgetCategoryId | UNIQUEIDENTIFIER | PK |
| BudgetPlanId | UNIQUEIDENTIFIER | FK → BudgetPlans |
| Category | NVARCHAR(50) | Travel / Accommodation / Food / Shopping / Fuel / Emergency / Miscellaneous |
| PlannedAmount | DECIMAL(10,2) | |

---

### APIs

#### POST `/api/trips/{tripId}/budget`
- Purpose: Create or update budget plan for a trip
- Request: `TotalBudget, Categories [ { Category, PlannedAmount } ]`

#### GET `/api/trips/{tripId}/budget`
- Purpose: Get budget plan with category breakdown
- Response: `TotalBudget, Categories, TotalPlanned`

#### GET `/api/trips/{tripId}/budget/summary`
- Purpose: Budget vs Actual comparison per category
- Response: `[ { Category, PlannedAmount, ActualAmount, Difference } ]`

---

## MODULE 4 – Expense Tracking

### Tables

**Expenses**
| Column | Type | Notes |
|---|---|---|
| ExpenseId | UNIQUEIDENTIFIER | PK |
| TripId | UNIQUEIDENTIFIER | FK → Trips |
| PaidByUserId | UNIQUEIDENTIFIER | FK → Users |
| Amount | DECIMAL(10,2) | |
| Category | NVARCHAR(50) | |
| Description | NVARCHAR(300) | |
| ExpenseDate | DATE | |
| AttachmentUrl | NVARCHAR(300) | Receipt image URL |
| CreatedAt | DATETIME | |

---

### APIs

#### POST `/api/trips/{tripId}/expenses`
- Purpose: Add a new expense to a trip
- Request: `Amount, Category, Description, ExpenseDate, PaidByUserId, AttachmentUrl`

#### GET `/api/trips/{tripId}/expenses`
- Purpose: Get all expenses for a trip
- Query Params: `category, startDate, endDate`

#### GET `/api/trips/{tripId}/expenses/{expenseId}`
- Purpose: Get expense details including split info

#### PUT `/api/trips/{tripId}/expenses/{expenseId}`
- Purpose: Update expense details

#### DELETE `/api/trips/{tripId}/expenses/{expenseId}`
- Purpose: Delete an expense

#### POST `/api/trips/{tripId}/expenses/{expenseId}/attachment`
- Purpose: Upload receipt image
- Request: `multipart/form-data`

---

## MODULE 5 – Group Management

### Tables
Uses existing `TripMembers` table.

**InviteTokens** (optional for invite links)
| Column | Type | Notes |
|---|---|---|
| InviteTokenId | UNIQUEIDENTIFIER | PK |
| TripId | UNIQUEIDENTIFIER | FK → Trips |
| Token | NVARCHAR(100) | Unique invite token |
| ExpiresAt | DATETIME | |
| IsUsed | BIT | |

---

### APIs

#### POST `/api/trips/{tripId}/members/invite`
- Purpose: Invite a member by phone number or email
- Request: `Email or PhoneNumber`

#### POST `/api/trips/{tripId}/members/join`
- Purpose: Join a trip using invite token
- Request: `InviteToken`

#### GET `/api/trips/{tripId}/members`
- Purpose: Get all members of a trip
- Response: `[ { UserId, FullName, Role, JoinedAt } ]`

#### DELETE `/api/trips/{tripId}/members/{userId}`
- Purpose: Remove a member from the trip

#### GET `/api/trips/{tripId}/invite-link`
- Purpose: Generate shareable invite link/token

---

## MODULE 6 – Expense Splitting

### Tables

**ExpenseSplits**
| Column | Type | Notes |
|---|---|---|
| SplitId | UNIQUEIDENTIFIER | PK |
| ExpenseId | UNIQUEIDENTIFIER | FK → Expenses |
| UserId | UNIQUEIDENTIFIER | FK → Users |
| SplitType | NVARCHAR(20) | Equal / Percentage / Custom |
| ShareAmount | DECIMAL(10,2) | Calculated share |
| SharePercentage | DECIMAL(5,2) | For percentage split |

---

### APIs

#### POST `/api/expenses/{expenseId}/split`
- Purpose: Define how an expense is split among members
- Request: `SplitType, Members [ { UserId, ShareAmount or SharePercentage } ]`
- Notes: Equal split auto-calculates shares

#### GET `/api/expenses/{expenseId}/split`
- Purpose: Get split details for an expense
- Response: `SplitType, [ { UserId, FullName, ShareAmount } ]`

#### PUT `/api/expenses/{expenseId}/split`
- Purpose: Update split configuration

---

## MODULE 7 – Settlement Engine

### Tables

**Settlements**
| Column | Type | Notes |
|---|---|---|
| SettlementId | UNIQUEIDENTIFIER | PK |
| TripId | UNIQUEIDENTIFIER | FK → Trips |
| PayerUserId | UNIQUEIDENTIFIER | FK → Users (who pays) |
| ReceiverUserId | UNIQUEIDENTIFIER | FK → Users (who receives) |
| Amount | DECIMAL(10,2) | |
| Status | NVARCHAR(20) | Pending / Paid |
| PaidAt | DATETIME | Nullable |
| CreatedAt | DATETIME | |

---

### APIs

#### GET `/api/trips/{tripId}/settlements`
- Purpose: Calculate and return settlement summary for the trip
- Response: `[ { PayerUserId, PayerName, ReceiverUserId, ReceiverName, Amount, Status } ]`

#### GET `/api/trips/{tripId}/settlements/my`
- Purpose: Get logged-in user's payables and receivables
- Response: `{ Payables: [], Receivables: [] }`

#### POST `/api/trips/{tripId}/settlements/{settlementId}/pay`
- Purpose: Mark a settlement as paid
- Request: `PaidAt`

#### GET `/api/trips/{tripId}/settlements/history`
- Purpose: Get all settled (paid) transactions for the trip

#### GET `/api/trips/{tripId}/settlements/member-balance`
- Purpose: Per-member balance summary
- Response: `[ { UserId, FullName, TotalPaid, FairShare, NetBalance } ]`

---

## MODULE 8 – Trip Wallet

### Tables

**WalletContributions**
| Column | Type | Notes |
|---|---|---|
| ContributionId | UNIQUEIDENTIFIER | PK |
| TripId | UNIQUEIDENTIFIER | FK → Trips |
| UserId | UNIQUEIDENTIFIER | FK → Users |
| Amount | DECIMAL(10,2) | |
| ContributedAt | DATETIME | |
| Note | NVARCHAR(200) | |

---

### APIs

#### POST `/api/trips/{tripId}/wallet/contribute`
- Purpose: Add wallet contribution for a member
- Request: `UserId, Amount, Note`

#### GET `/api/trips/{tripId}/wallet`
- Purpose: Get wallet balance and contributions
- Response: `{ TotalBalance, TotalExpenses, RemainingBalance, Contributions: [] }`

#### GET `/api/trips/{tripId}/wallet/transactions`
- Purpose: Get full wallet transaction history (contributions + deductions)

---

## MODULE 9 – Analytics & Reporting

> Read-only computed endpoints. No new tables required.

### APIs

#### GET `/api/trips/{tripId}/analytics/summary`
- Purpose: Trip financial overview
- Response: `TotalBudget, TotalExpense, RemainingBudget, TotalMembers, TopCategory`

#### GET `/api/trips/{tripId}/analytics/category-breakdown`
- Purpose: Expense breakdown by category
- Response: `[ { Category, Amount, Percentage } ]`

#### GET `/api/trips/{tripId}/analytics/budget-vs-actual`
- Purpose: Planned vs actual per category
- Response: `[ { Category, Planned, Actual, Difference } ]`

#### GET `/api/trips/{tripId}/analytics/member-contributions`
- Purpose: How much each member paid
- Response: `[ { UserId, FullName, TotalPaid, SharePercentage } ]`

#### GET `/api/trips/{tripId}/analytics/spending-trend`
- Purpose: Daily expense trend
- Response: `[ { Date, TotalAmount } ]`

---

## MODULE 10 – Notifications

### Tables

**Notifications**
| Column | Type | Notes |
|---|---|---|
| NotificationId | UNIQUEIDENTIFIER | PK |
| UserId | UNIQUEIDENTIFIER | FK → Users |
| TripId | UNIQUEIDENTIFIER | FK → Trips (nullable) |
| Type | NVARCHAR(50) | ExpenseAdded / MemberJoined / SettlementPending / PaymentReceived / TripReminder |
| Message | NVARCHAR(300) | |
| IsRead | BIT | |
| CreatedAt | DATETIME | |

---

### APIs

#### GET `/api/notifications`
- Purpose: Get all notifications for logged-in user
- Query Params: `isRead (bool)`

#### PUT `/api/notifications/{notificationId}/read`
- Purpose: Mark a notification as read

#### PUT `/api/notifications/read-all`
- Purpose: Mark all notifications as read

---

## MODULE 11 – Admin Panel

### Tables
Uses existing tables. Additional:

**AuditLogs**
| Column | Type | Notes |
|---|---|---|
| AuditLogId | UNIQUEIDENTIFIER | PK |
| UserId | UNIQUEIDENTIFIER | FK → Users |
| Action | NVARCHAR(100) | e.g. UserDeleted, TripCreated |
| Entity | NVARCHAR(50) | |
| EntityId | NVARCHAR(100) | |
| Timestamp | DATETIME | |
| IPAddress | NVARCHAR(50) | |

---

### APIs

#### GET `/api/admin/dashboard`
- Purpose: System stats overview
- Response: `TotalUsers, ActiveTrips, TotalExpenses, TotalSettlements`

#### GET `/api/admin/users`
- Purpose: List all users with filters
- Query Params: `search, isActive, page, pageSize`

#### PUT `/api/admin/users/{userId}/deactivate`
- Purpose: Deactivate a user account

#### GET `/api/admin/trips`
- Purpose: List all trips across all users
- Query Params: `status, search, page, pageSize`

#### GET `/api/admin/audit-logs`
- Purpose: View audit trail
- Query Params: `userId, action, startDate, endDate`

---

## API Summary Count

| Module | API Count |
|---|---|
| User Management | 6 |
| Trip Planning | 6 |
| Budget Planning | 3 |
| Expense Tracking | 6 |
| Group Management | 5 |
| Expense Splitting | 3 |
| Settlement Engine | 5 |
| Trip Wallet | 3 |
| Analytics | 5 |
| Notifications | 3 |
| Admin Panel | 5 |
| **Total** | **50 APIs** |

---

## Common Conventions

- All responses wrapped in: `{ success: bool, data: {}, message: string }`
- Authentication: JWT Bearer Token on all endpoints except `/auth/*`
- All IDs: GUID / UNIQUEIDENTIFIER
- Dates: ISO 8601 format (`YYYY-MM-DD`)
- Pagination: `page` + `pageSize` query params where applicable
- Soft deletes preferred (IsActive / Status flags) over hard deletes

---

## MVP Release API Scope

**Release 1 (Core)**
- User Management ✓
- Trip Planning ✓
- Budget Planning ✓
- Expense Tracking ✓
- Expense Splitting ✓
- Settlement Engine ✓

**Release 2**
- Group Management ✓
- Trip Wallet ✓
- Notifications ✓
- Analytics ✓

**Release 3**
- Admin Panel ✓
- Advanced Reports
