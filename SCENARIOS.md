# Banking System – Exam Test Scenarios

This document describes the **pre-seeded test scenarios** used for the exam project
*Testing and Software Quality*.

The system is intentionally seeded with deterministic data to support:

- Behaviour-Driven Development (BDD)
- Decision Table–Based Testing
- DD-Path (Decision-to-Decision) Testing
- Automated API testing

The primary focus areas are:

- **Transfers**
- **BSRun (Betalingsservice-like batch processing)**

All scenarios start from a **known database state**, reset on application startup.

---

## Seeded Identifiers (Stable Across Runs)

These IDs are fixed to allow repeatable tests and demos.

### Customers
- **Customer A** – Transfer User / Debtor  
  `11111111-1111-1111-1111-111111111111`
- **Customer B** – Receiver / Settlement Owner  
  `22222222-2222-2222-2222-222222222222`

### Accounts
| Account | Purpose |
|------|--------|
| `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa` | Active payer account (sufficient balance) |
| `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab` | Active payer account (low balance) |
| `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaac` | Inactive payer account |
| `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb` | Active receiver account |
| `cccccccc-cccc-cccc-cccc-cccccccccccc` | Settlement account for BS |

### Mandates
| Mandate | Status | Purpose |
|-------|-------|--------|
| `dddddddd-dddd-dddd-dddd-dddddddddddd` | Active | Successful BS collection |
| `eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee` | Cancelled | BS failure path |

---

# TRANSFER SCENARIOS

## TS-01 – Successful Transfer
**Purpose:** Happy-path transfer

- From: Active account with sufficient balance
- To: Active account
- Amount: Below limit

**Expected Result**
- Transfer succeeds
- Source balance decreases
- Target balance increases
- Transaction recorded with `Completed` status

---

## TS-02 – Insufficient Funds
**Purpose:** Decision table rule – balance check

- From: Active account with low balance
- To: Active account
- Amount: Greater than available balance

**Expected Result**
- Transfer rejected
- No balances changed
- Failure reason: `InsufficientFunds`

---

## TS-03 – Inactive Source Account
**Purpose:** Validation rule

- From: Inactive account
- To: Active account
- Amount: Any

**Expected Result**
- Transfer rejected
- Failure reason: `AccountInactive`

---

## TS-04 – Manual Approval Required
**Purpose:** Decision table threshold path

- From: Active account
- To: Active account
- Amount: Above manual-approval threshold

**Expected Result**
- Transfer returns `PendingApproval`
- No balances changed
- Transaction stored with `Pending` status

---

# BSRUN SCENARIOS (BETALINGSSERVICE)

## BS-01 – Notify Upcoming Collection
**Purpose:** Upcoming notification logic

- Collection status: `Created`
- Due date: Today + 7 days
- Mandate: Active

**When**
- `BSRun.NotifyUpcoming(daysAhead = 7)` is executed

**Expected Result**
- Collection status → `Notified`
- `NotifiedUtc` is set

---

## BS-02 – Collect Due Collection (Success)
**Purpose:** Happy-path BS collection

- Collection status: `Approved`
- Due date: Today
- Mandate: Active
- Payer has sufficient balance

**When**
- `BSRun.CollectDue()` is executed

**Expected Result**
- Money transferred from payer → settlement account
- Collection status → `Collected`
- `CollectedUtc` is set

---

## BS-03 – Collect Due Collection (Cancelled Mandate)
**Purpose:** Failure branch in BSRun loop

- Collection status: `Approved`
- Due date: Today
- Mandate: Cancelled

**Expected Result**
- No transfer performed
- Collection status → `Failed`
- Failure reason: `MandateNotActive`

---

## BS-04 – Ignore Already Collected
**Purpose:** Guard clause / idempotency

- Collection status: `Collected`
- Due date: Past

**Expected Result**
- Collection ignored
- No state change

---

# TRACEABILITY MATRIX (EXAM FRIENDLY)

| Feature | Testing Technique |
|------|------------------|
| Transfer limits | Decision Table Testing |
| Transfer flow | DD-Path Testing |
| BSRun batch loop | DD-Path Testing |
| BS notify/collect | BDD Scenarios |
| API endpoints | Automated API Tests |

---

## Notes for Examiner

- Database is reset and seeded on startup in Development
- All scenarios are deterministic and repeatable
- Seed data is intentionally minimal and scenario-driven
- Business rules are implemented in Services, not Controllers

