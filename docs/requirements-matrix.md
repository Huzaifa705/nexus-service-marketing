# NEXUS Service Marketing System — Requirements Matrix

| | |
|---|---|
| Version | 1.3 — Node 2 baseline (.NET) |
| Date | 2026-10-22 |
| Status | BASELINE. Approved by the owner by delegation on 2026-10-20. Every `SRC⚠` item is resolved by the decision `D-xx` it cites in `decision-log.md`. Real-world values (prices, tax, currency) stay flagged unverified until confirmed before go-live. |
| Inputs reviewed in full | **S1** `NEXUS_SERVICE_MARKETING_SYSTEM.doc` (original specification) · **G1** `NEXUS_Service_Marketing_System_15_Day_Claude_Guide_Rev2_dotnet.pdf` (build plan, revision 2: ASP.NET Core and SQL Server; replaces the original `NEXUS_Service_Marketing_System_15_Day_Claude_Guide.pdf`) |

## 1. How to read this document

**Source references.** Paragraph numbers count the non-empty paragraphs of each section of S1, top to bottom. The account-ID "Note" is Proposed Solution ¶3.

| Code | Location in S1 |
|---|---|
| INT-n | Introduction ¶n (1–4) |
| EXS-n | Existing Scenario ¶n (1–5) |
| PRO-n | Proposed Solution ¶n (1–8) |
| NF-n | Non-Financial, item n (1–9) |
| FIN-1 | Financial, opening paragraph |
| FIN-DEP, FIN-DU, FIN-BB, FIN-LL, FIN-TAX | Financial: security deposits, dial-up plans, broadband plans, landline plans, service tax |
| FR-n | Functional Requirements, item n (1–9) |

**Type:** `SRC` stated in S1 and clear · `SRC⚠` stated in S1 but ambiguous or contradictory (resolved by the decision cited in Dep. or in the criteria) · `REC` my recommendation, not in S1.
**Priority:** `M` Must (MVP) · `S` Should (MVP if time allows; first to cut) · `C` Could (after MVP).
**Node:** guide node where it is built (N3–N15). **Dep.:** requirement or decision it depends on.

## 2. User roles

| ID | Role | Source | Responsibilities stated in S1 | Open points |
|---|---|---|---|---|
| R-01 | Admin (Manager) | INT-4, EXS-5, PRO-1, NF-6 | Maintains employees (solely), retail shops, vendors, product/equipment details, purchase lists, plans and charges (insert/update/delete/search) | Overlap with Technical on equipment — D-09 |
| R-02 | Accounts | PRO-5, NF-5, FIN-1 | Calculates charges, generates bills, updates bill details in the system (Accounts alone), records payments made at the city office | Central or per-city? — D-11 |
| R-03 | Technical | NF-7 | Tracks orders, records feasibility, creates connections, sets connections temporarily/permanently inactive, maintains product/equipment details | Equipment overlap — D-09 |
| R-04 | Retail employee | INT-2, PRO-2, NF-3, NF-4, NF-5 | Places orders (the only role allowed, per NF-3), checks order status, sees bills/plans, records payments, sees till-date orders/connections/bills/payments | Scope: own shop, plus exact-match desk lookups — D-11, D-22 |
| R-05 | Customer | PRO-2, PRO-6, FR-8 | Views/maintains contact details, tracks order and connection, views bills, gives feedback; "places an order" per PRO-6 (conflicts with NF-3) | D-08, D-12 |
| R-06 | Public visitor | PRO-2 | Not a stated role. Exists only if status lookup is public | D-12 |

## 3. Requirements

### 3.1 Authentication and roles

| ID | Requirement | Source | Type | Pri | Node | Dep. | Acceptance criteria |
|---|---|---|---|---|---|---|---|
| AUTH-01 | Every employee and customer signs in with individual credentials; Admin provisions employee credentials | NF-2, PRO-1, PRO-2 | SRC | M | N3 | D-28 | All 5 roles sign in/out with their own account; no shared accounts outside labelled demo users; passwords are stored only as salted hashes by ASP.NET Core Identity, never in plain text |
| AUTH-02 | Role-based permissions (Admin, Accounts, Technical, Retail, Customer) enforced on the server for every protected read and write | PRO-1, PRO-2, PRO-5, NF-2…NF-7 | SRC | M | N3 | AUTH-01, D-08, D-09, D-11, D-25 | Every mutation in the role matrix (architecture.md §4) has an automated test: allowed roles succeed, all others get 403; direct URL/API calls without the role fail |
| AUTH-03 | Only Admin creates, edits, deactivates employees and assigns roles | INT-4, EXS-5, NF-2 | SRC | M | N4 | AUTH-02 | Non-Admin attempts rejected; each role change writes an audit entry |
| AUTH-04 | Each retail employee is linked to one retail shop, used to scope data | INT-2, NF-4 | REC | M | N3 | D-11, D-22 | With two shops seeded, a shop-1 user cannot read or change shop-2 records |
| AUTH-05 | A customer login maps to exactly one customer record and sees only that customer's data | PRO-6 | SRC⚠ | M | N3, N10 | D-12 | Changing an ID in a URL/API call to another customer's record returns 403/404 (IDOR test) |
| AUTH-06 | Sessions expire; deactivated users lose access immediately | — | REC | S | N3 | AUTH-01 | Expired session → login page; a deactivated user's next request fails |
| AUTH-07 | Staff can create a portal login for a customer: Admin, or Retail for customers of their own shop; the login is linked to exactly one customer and starts with a one-time temporary password | PRO-6 | REC | M | N10 | AUTH-05, D-12, D-25, D-31 | Creating a login links one Customer-role user to that customer; a second login for the same customer is rejected while the first is active; the temporary password is shown once and must be changed at first sign-in; a customer login receives 403 on every staff route |
| AUTH-08 | Password policy: minimum 10 characters; temporary credentials must be changed at first sign-in | NF-2 | REC | M | N3 | AUTH-01, D-25 | A 9-character password is rejected and a 10-character one accepted; a user created with temporary credentials cannot reach any page before changing the password |

### 3.2 Administration (company setup)

| ID | Requirement | Source | Type | Pri | Node | Dep. | Acceptance criteria |
|---|---|---|---|---|---|---|---|
| ADM-01 | Maintain cities of the territory, each with a unique 3-digit numeric code | PRO-3 | SRC | M | N4 | — | Code stored as text matching `^[0-9]{3}$`; unique; code cannot be edited once a connection uses it |
| ADM-02 | Maintain retail shops (name, city, address, contact, active flag) | INT-2, INT-4, FR-2, PRO-1 | SRC | M | N4 | ADM-01 | Admin can create/edit/search; a shop with orders can only be deactivated |
| ADM-03 | Maintain vendors (name, contact details, orders placed, payment lists) | INT-4, EXS-3, FR-4 | SRC | M | N4, N8 | — | Admin can create/edit/search; a vendor with purchase history is deactivate-only |
| ADM-04 | Maintain employee profiles for admin, accounts, technical and retail staff | NF-2, FR-3 | SRC | M | N4 | AUTH-03 | Admin can create/edit/search/deactivate; each employee has exactly one role |
| ADM-05 | Where history exists, "delete" becomes deactivate/archive | NF-6 | REC | M | N4 | — | Deleting a referenced record returns 409 with an in-use message; deactivated records (`active = false`) are hidden from pickers but remain visible in history |

### 3.3 Plans, pricing and tax

| ID | Requirement | Source | Type | Pri | Node | Dep. | Acceptance criteria |
|---|---|---|---|---|---|---|---|
| PLN-01 | Admin can insert, update, delete (safely), and search plans and charges with detailed descriptions | NF-6, FIN-1, FR-1 | SRC | M | N4 | AUTH-02 | Create, read, update, search and deactivate each covered by an integration test; non-Admin gets 403; deleting a plan referenced by an order or connection returns 409 |
| PLN-02 | Catalogue covers security deposits (dial-up, broadband, landline), dial-up hourly and unlimited (28/56 Kbps), broadband hourly and unlimited (64/128 Kbps), landline Local and STD plans | FIN-DEP, FIN-DU, FIN-BB, FIN-LL | SRC | M | N4 | D-04 | All source plans seeded exactly as in Appendix A3, except the two assumed values (D-05, D-06) which carry an "assumed" flag; every plan flagged "verify before production" |
| PLN-03 | Hourly plans store an hours allowance and a validity period | FIN-DU, FIN-BB | SRC | M | N4 | — | Allowance and validity stored as data, not text |
| PLN-04 | Landline plans store rental, validity and per-minute rates for local, STD and mobile-messaging calls | FIN-LL | SRC⚠ | M | N4 | D-04, D-05, D-06 | Rates stored per plan version; unit of "messaging" confirmed (D-04) |
| PLN-05 | Prices, validity, deposits, rates, tax and discount tiers are effective-dated data; editing never alters issued bills | NF-8, FR-9 | REC | M | N4, N9 | — | Edit a price → an existing bill is unchanged; a new bill uses the new price |
| PLN-06 | Currency is a setting, not hard-coded | FIN-DEP, FIN-DU, FIN-BB, FIN-LL | REC | M | N4 | D-04 | Currency code held in settings; UI formats amounts from it |
| PLN-07 | A plan with an empty price or validity cannot be activated; plans seeded with assumed demo values show a visible "assumed — confirm" flag | FIN-LL | REC | M | N4 | D-05, D-06 | Activation rejected while price/validity is empty; Yearly STD ($720.00) and Half-yearly STD (6 months) are seeded with the flag |
| PLN-08 | Tax rate (seed 12.24%) is configurable with an effective date | FIN-TAX | SRC | M | N4 | D-07 | Rate and `deposit_taxable` flag editable by Admin only; changes audited |
| PLN-09 | Employees and customers can view active plans and descriptions | PRO-2 | SRC | S | N10 | PLN-01 | Retail and customer pages list active plans with description and price |

### 3.4 Customers

| ID | Requirement | Source | Type | Pri | Node | Dep. | Acceptance criteria |
|---|---|---|---|---|---|---|---|
| CUS-01 | Create and maintain customer personal and contact details | EXS-2, FR-4, PRO-6 | SRC | M | N5 | D-21 | Required fields (D-21) validated on the server; same contact number triggers a duplicate warning |
| CUS-02 | Customer type: individual or corporate | EXS-1 | REC | S | N5 | D-03 | Type stored; used by the bulk-scheme logic |
| CUS-03 | Customer record holds address-proof, application-form and receipt documents | EXS-2 | SRC | M | N11 | DOC-01 | Documents listed on the customer record; access follows DOC-01 rules |
| CUS-04 | Customer can view and edit permitted contact details | PRO-2, PRO-6 | SRC | S | N10 | AUTH-05 | Only contact fields editable by the customer; other changes via staff; every change audited |
| CUS-05 | Legacy 10-digit account IDs can be stored against a customer | EXS-2 vs PRO-2 | SRC⚠ | C | post-MVP | D-01 | Nullable `legacy_account_id` exists; no import tool in MVP |
| CUS-06 | Retail desk lookup: find an existing customer by exact primary contact number across shops (minimal view) to avoid duplicate customers | EXS-1, NF-4 | REC | M | N5 | CUS-01, D-11, D-22 | Exact match returns full name, city and masked contact only; partial or wildcard input returns nothing; the full record opens only for customers with an order at the user's shop; each lookup writes an audit row |

### 3.5 Orders

| ID | Requirement | Source | Type | Pri | Node | Dep. | Acceptance criteria |
|---|---|---|---|---|---|---|---|
| ORD-01 | A retail employee can place an order for a customer for type D (dial-up), B (broadband) or T (telephone only) | NF-3, PRO-2, FR-5 | SRC⚠ | M | N5 | D-08, D-10 | Order saved with customer, type, plan, shop, employee, timestamp; roles other than Retail get 403 |
| ORD-02 | Server generates the order ID: type letter + 10-digit sequence = 11 characters (e.g. `D0000000001`) | PRO-2 | SRC | M | N5 | D-02 | Matches `^[DTB][0-9]{10}$`; unique DB constraint; 50 parallel creations → 50 distinct IDs; first D order in an empty DB is `D0000000001` |
| ORD-03 | Order status is tracked with full history (who, when, old, new, reason) | PRO-2, NF-3, NF-4 | SRC | M | N5, N6 | — | Every transition adds a history row; history rows cannot be edited or deleted |
| ORD-04 | Controlled status list: Submitted → Under review → Feasibility pending → Feasible / Not feasible → Awaiting equipment → Connection created / Rejected / Cancelled | NF-4, NF-7 | REC | M | N5 | D-23, D-24 | Illegal transitions rejected on the server (table-driven test); each permitted transition is allowed only for the actor roles in architecture.md §7 |
| ORD-05 | Dial-up orders record whether the customer already holds a landline with the vendor (and which) or is applying for one together | INT-2, INT-3 | SRC⚠ | M | N5 | D-10 | Field mandatory for type D; accepts an existing T account ID, a legacy identifier, or a linked T order; drives FEA-02 |
| ORD-06 | Order captures shop, employee, application date/time, plan, equipment choice (modem/router), installation address and city, optional bulk-application reference, notes | EXS-1, EXS-2, NF-4, FR-8 | SRC | M | N5 | D-03, D-21 | All fields persisted and shown on the order detail page |
| ORD-07 | Retail employees can list all orders placed to date with status (own shop) | NF-4 | SRC | M | N5 | D-11 | List shows only the caller's shop; filter by status |
| ORD-08 | An order can be cancelled with a reason | EXS-5 | REC | S | N5 | — | Cancelled order cannot be provisioned; reason and actor recorded; only Admin, or Retail for its own shop, can cancel (all other roles get 403) |
| ORD-09 | Plan can be changed on an order before provisioning | EXS-5 | SRC⚠ | S | N5 | D-16 | Change recorded in history; feasibility re-checked if type changes |
| ORD-10 | Customer self-service order placement | PRO-2, PRO-6 vs NF-3 | SRC⚠ | C | post-MVP | D-08 | Schema already carries `channel` and `placed_by`; if enabled, orders enter a staff-verification queue |
| ORD-11 | Log customer enquiries before an order | EXS-1, PRO-8, NF-9 | SRC⚠ | C | post-MVP | D-19 | S1 names enquiries but defines no record or workflow |
| ORD-12 | Orders placed together by the same customer can be grouped into a bulk application; its size (orders that are not Cancelled, Rejected or Not feasible) sets the discount tier; each order stays one connection | EXS-1, FR-9 | SRC⚠ | M | N5 | D-03 | Orders link to one group; a cancelled, rejected or not-feasible order lowers the count for bills not yet created; issued bills never change; a single-connection order needs no group |

### 3.6 Feasibility

| ID | Requirement | Source | Type | Pri | Node | Dep. | Acceptance criteria |
|---|---|---|---|---|---|---|---|
| FEA-01 | Technical staff see a queue of orders awaiting feasibility | NF-7, INT-3 | SRC | M | N6 | ORD-03 | Queue lists orders in Feasibility pending; an empty queue shows the empty-state text; Technical can act, Admin can read, Accounts/Retail/Customer get 403 |
| FEA-02 | Checks recorded per case: dial-up with new landline → landline + internet; dial-up with existing vendor landline → internet only; broadband / telephone-only → one check (assumed) | INT-3 | SRC⚠ | M | N6 | D-10, D-13 | Required checks computed from the order; order cannot be marked Feasible until all required checks pass |
| FEA-03 | Feasibility inputs captured manually: distance, server availability, notes | INT-3 | SRC⚠ | M | N6 | D-13 | S1 gives no formula or API; structured manual fields stored with each check |
| FEA-04 | Outcome Feasible or Not feasible; a reason is mandatory for Not feasible; decisions are append-only | NF-7 | SRC | M | N6 | FEA-01, D-24 | Not feasible without a reason rejected; earlier decisions remain visible after a new one |
| FEA-05 | Authorised retail staff (and the customer where permitted) can see the outcome and reason | NF-4, PRO-6 | SRC | M | N6 | D-11 | Result visible on order detail; other shops/customers cannot see it |

### 3.7 Connections and account IDs

| ID | Requirement | Source | Type | Pri | Node | Dep. | Acceptance criteria |
|---|---|---|---|---|---|---|---|
| CON-01 | Technical can create a connection only from an order in Feasible status | NF-7, INT-3 | SRC | M | N7 | FEA-04, D-24 | Attempt on a non-feasible order is rejected server-side; no connection row and no ID consumed |
| CON-02 | Account ID generated only at provisioning: type letter + 3-digit city code + 12-digit sequence = 16 characters | PRO-2, PRO-3 | SRC⚠ | M | N7 | D-01, D-21, ADM-01 | Matches `^[DBT][0-9]{15}$`; type letter equals the order's type; city code is the order's installation city; unique constraint; 50 parallel provisions → 50 distinct IDs; unprovisioned orders have no account ID |
| CON-03 | Account ID is immutable once issued and never reused | PRO-2 | REC | M | N7 | CON-02, D-01 | No API or UI path edits it; test attempts an update |
| CON-04 | Statuses: Active, Temporarily inactive, Permanently inactive, changed by Technical with a reason | NF-7, PRO-2 | SRC | M | N7 | — | Allowed transitions enforced; actor, time and reason stored in history |
| CON-05 | A connection links customer, order, plan, equipment issued, city and start date | NF-1, EXS-4, FR-8 | SRC | M | N7 | — | All links present; connection detail page shows them |
| CON-06 | Retail employees can list connections provided to date (own shop) | NF-4 | SRC | M | N7 | D-11 | List scoped to shop |
| CON-07 | Relationship between bill/payment status and connection status | PRO-5 | SRC⚠ | S | N9 | D-14 | Overdue report lists connections; no automatic status change unless D-14 says otherwise |
| CON-08 | Plan change on a live connection | EXS-5 | SRC⚠ | C | post-MVP | D-16 | Out of MVP; Accounts adjusts manually |
| CON-09 | Withdrawal/cancellation: connection set Permanently inactive and deposit refund recorded | FIN-DEP | SRC⚠ | S | N9 | D-17 | Refund entry (amount, date, reason, actor) can be recorded against the connection |
| CON-10 | A dial-up connection references the vendor landline account it depends on | INT-2 | SRC⚠ | M | N7 | D-10 | Dial-up cannot be provisioned without a landline reference: the account ID of an Active T connection in the system, or a legacy landline identifier typed by Technical and flagged `legacy_unverified` (D-10) |

### 3.8 Equipment, vendors and procurement

| ID | Requirement | Source | Type | Pri | Node | Dep. | Acceptance criteria |
|---|---|---|---|---|---|---|---|
| EQP-01 | Equipment/product catalogue (modem, router, etc.) linked to vendors | INT-2, INT-4, FR-6, EXS-5 | SRC | M | N8 | ADM-03 | CRUD + search by role (EQP-09); product has a vendor link |
| EQP-02 | Stock is a movement ledger: purchase, issue, return, damaged/replacement, adjustment; on-hand = sum of movements | INT-4 | SRC | M | N8 | EQP-01, D-09, D-23 | No direct edit of a stock number; on-hand can never go negative (concurrency test); adjustments are Admin-only, require a reason and are audited; opening stock is entered as an adjustment |
| EQP-03 | Purchase orders / purchase lists per vendor with items, quantities, status, and vendor payment records | INT-4, EXS-3 | SRC | M | N8 | EQP-01, D-09 | PO created, received (goods receipt creates movements), vendor payments recorded by Admin only |
| EQP-04 | Equipment issued to each connection (materials provided with the customer) | EXS-4, FR-7 | SRC | M | N8 | CON-05, D-23 | Issue reduces stock in one transaction after the connection exists; blocked when stock is insufficient |
| EQP-05 | Procurement planning: pending orders needing equipment versus stock on hand | INT-4 | SRC | S | N8, N11 | EQP-02, D-23 | View lists shortfall per product |
| EQP-06 | Low-stock report with configurable threshold | — | REC | S | N11 | EQP-02 | Products at/below threshold listed |
| EQP-07 | Replacement due to customer damage creates a billable item | EXS-4 | SRC | M | N8, N9 | BIL-03, D-15 | Damage movement can generate a bill line for Accounts |
| EQP-08 | Equipment return on withdrawal | — | REC | S | N8 | EQP-02 | Return movement increases stock and closes the issued record |
| EQP-09 | Permissions: Admin owns catalogue, vendors, purchase orders, stock corrections; Technical owns goods receipt, issue, return, damage (proposed) | INT-4, EXS-5 vs NF-7 | SRC⚠ | M | N3, N8 | D-09 | Role-matrix tests pass for both roles |
| EQP-10 | Each plan can list the equipment types eligible to be supplied with it; the equipment chosen on an order must be eligible | EXS-4 | SRC | S | N8 | EQP-01, D-15, D-23 | An order with an ineligible product is rejected; a plan with no list allows any active product |

### 3.9 Billing

| ID | Requirement | Source | Type | Pri | Node | Dep. | Acceptance criteria |
|---|---|---|---|---|---|---|---|
| BIL-01 | Accounts alone creates and updates bills and charge details; other roles read only | PRO-5, NF-5, FIN-1 | SRC | M | N9 | AUTH-02 | Admin, Technical, Retail and Customer get 403 on bill mutations |
| BIL-02 | Charges derive from the customer's chosen plan and equipment | PRO-5, NF-8, EXS-4 | SRC | M | N9 | PLN-02, EQP-04 | Bill lines reference the plan version and equipment; totals match test vectors |
| BIL-03 | Bill is itemised: plan rental/advance, usage (call) charges, equipment/replacement, adjustments, discount, tax, previous balance/credits, paid, due | EXS-4, FIN-1, FR-9 | SRC | M | N9 | BIL-02 | Sum of lines equals total (property test) |
| BIL-04 | Postpaid model: bills are generated by Accounts after provisioning and per period | PRO-5 | SRC⚠ | M | N9 | D-14 | Initial bill at activation = first-period rental + deposit; later bills = next-period rental in advance + previous-period usage in arrears; due date = issue + 15 days (D-14, D-20) |
| BIL-05 | A bill stores an immutable snapshot of the rates, tax rate and discount used | — | REC | M | N9 | PLN-05 | Edit plan after billing → bill unchanged |
| BIL-06 | Bulk/corporate discount applied to advance and security deposit using configurable tiers | EXS-1, FR-9 | SRC⚠ | M | N4, N9 | D-03 | Boundary vectors TV-D pass; tiers editable by Admin with explicit inclusive/exclusive bounds |
| BIL-07 | Service tax (seed 12.24%) applied per the approved rule; calculation shown on the bill | FIN-TAX | SRC⚠ | M | N9 | D-07 | Tax vectors TV-T pass |
| BIL-08 | Refundable security deposit tracked as a separate liability, not revenue | FIN-DEP | REC | M | N9 | D-07, D-17 | Deposit line labelled refundable; deposit balance per connection available |
| BIL-09 | Call/usage charges entered manually by Accounts (rate × minutes per call type) | FIN-LL, EXS-4 | SRC⚠ | M | N9 | D-14 | Computed from the rate snapshot; entry audited |
| BIL-10 | Money uses decimal-safe types and one documented rounding rule | — | REC | M | N9 | D-07 | No floating-point money arithmetic; rounding vector TV-T4 passes |
| BIL-11 | Paid and due amounts visible on each bill and per connection; bills viewable on the website | FIN-1, PRO-6, NF-4 | SRC | M | N9, N10 | BIL-03 | Staff and the owning customer can view; others cannot |
| BIL-12 | Previous payments/credits and prior unpaid balance are taken into account | FR-9, EXS-4 | SRC | M | N9 | PAY-03 | Carry-forward shown as its own line |
| BIL-13 | Hourly plans: allowance and validity stored; overage/rollover not automated | FIN-DU, FIN-BB | SRC⚠ | C | post-MVP | D-04 | Accounts can add a manual overage line |
| BIL-14 | Bill lifecycle: Draft → Issued → Void (with reason); payment state is derived | — | REC | M | N9 | — | Payments on a void bill rejected |
| BIL-15 | Equipment customer charge (price/rental) configurable per product | EXS-2, PRO-5 | SRC⚠ | S | N8 | D-15 | Blank means no charge; value flows to bill line |

### 3.10 Payments

| ID | Requirement | Source | Type | Pri | Node | Dep. | Acceptance criteria |
|---|---|---|---|---|---|---|---|
| PAY-01 | Retail employees and Accounts can record payments; Accounts records those made at the city office | NF-5, EXS-1 | SRC | M | N9 | — | Other roles get 403 |
| PAY-02 | A payment stores amount, date, method, reference, recorded-by, shop/office | EXS-4, FIN-1 | REC | M | N9 | — | All fields persisted; recorded-by cannot be edited |
| PAY-03 | Payments are allocated to bills and update paid/due atomically | FIN-1, FR-9 | SRC | M | N9 | BIL-03 | Vectors TV-P pass |
| PAY-04 | Duplicate submission protection (idempotency key) | — | REC | M | N9 | — | Same request sent twice → one payment |
| PAY-05 | Zero, partial, full and over-payment handled; overpayment held as credit | FR-9 | REC | M | N9 | D-14 | Vectors TV-P1…TV-P4 |
| PAY-06 | A receipt record (number, date, amount) is created per payment | EXS-2 | SRC⚠ | S | N9 | PAY-02, D-20 | Receipt viewable/printable; numbering format agreed at N9 |
| PAY-07 | Retail can record a payment for a connection of any shop via exact account-ID lookup (minimal view: customer name, account ID, amount due); the payment stores the receiving shop | NF-5, EXS-1 | REC | M | N9 | PAY-01, D-22 | The lookup returns only the minimal fields; the payment row shows the receiving shop; no other bill detail is exposed |

### 3.11 Customer portal

| ID | Requirement | Source | Type | Pri | Node | Dep. | Acceptance criteria |
|---|---|---|---|---|---|---|---|
| PRT-01 | Customer views the status of their own order | PRO-2, PRO-6 | SRC | M | N10 | AUTH-05 | Own orders only |
| PRT-02 | Customer views their own connection status and due amount | PRO-2, PRO-6 | SRC | M | N10 | AUTH-05 | Status ∈ {Active, Temporarily inactive, Permanently inactive}; due shown |
| PRT-03 | Customer views their own bills, paid and due | PRO-6, FIN-1 | SRC | M | N10 | BIL-11 | Own bills only |
| PRT-04 | Customer submits feedback on an order or connection | FR-8 | SRC⚠ | S | N10 | AUTH-05, D-21 | Rating + comment (assumed format) linked to order/connection; S1 gives no format |
| PRT-05 | Public (unauthenticated) status lookup | PRO-2 | SRC⚠ | C | post-MVP | D-12 | If enabled: status only, rate-limited, no personal or financial data |

### 3.12 Search

| ID | Requirement | Source | Type | Pri | Node | Dep. | Acceptance criteria |
|---|---|---|---|---|---|---|---|
| SCH-01 | Search order status by order ID | PRO-2 | SRC | M | N10 | ORD-02 | Valid in-scope ID → status; malformed ID → validation error; unknown or out-of-scope ID → identical not-found message (no existence leak) |
| SCH-02 | Search connection status by account ID, including due amount and status | PRO-2 | SRC | M | N10 | CON-02, D-01 | Returns status and due; scoped by role |
| SCH-03 | Advanced order search: unique ID, name, connection type, application date/period, contact number | PRO-4 | SRC | M | N10 | D-20 | Each filter alone and combined; date range inclusive; indexed; paginated |
| SCH-04 | Advanced connection search: unique ID, name, type, connection date/period, contact number | PRO-4 | SRC | M | N10 | — | As SCH-03 |
| SCH-05 | Results are limited to the caller's role/shop/own records; no enumeration of others' data | — | REC | M | N10 | D-11, D-12, D-22 | IDOR and negative tests pass; unknown and out-of-scope IDs return the identical not-found response |

### 3.13 Documents and audit

| ID | Requirement | Source | Type | Pri | Node | Dep. | Acceptance criteria |
|---|---|---|---|---|---|---|---|
| DOC-01 | Address proof, application form and receipts stored as metadata + file in private storage outside the web root, opened only through a permission-checked download link that expires | EXS-2 | SRC | M | N11 | AUTH-02, D-25, D-28 | Unauthenticated request fails (401/403); a download link stops working after at most 300 seconds |
| DOC-02 | Documents filterable by year and area (city) | EXS-2 | SRC | M | N11 | DOC-01 | Filtering by upload year and by city returns exactly the matching documents |
| DOC-03 | Upload validation: file type and size limits | — | REC | M | N11 | DOC-01, D-25 | Only PDF, JPEG and PNG files up to 5 MB are accepted (limits configurable); other types and larger files are rejected |
| DOC-04 | Which documents were submitted is captured at order time | EXS-2 | SRC | S | N5 | ORD-06 | Checklist saved on the order |
| AUD-01 | Append-only audit log: role changes, plan/price edits, feasibility decisions, status changes, bills, payments, stock adjustments (who, when, what) | PRO-1, G1 | REC | M | N3 → N11 | — | An audit row exists for each listed action; rows cannot be edited |
| AUD-02 | Key records show created/updated timestamps and the responsible user | — | REC | M | N3 | — | Present on all core tables |
| AUD-03 | Admin audit viewer | — | REC | S | N11 | AUD-01 | Filter by user, entity, date |

### 3.14 Reports and non-functional

| ID | Requirement | Source | Type | Pri | Node | Dep. | Acceptance criteria |
|---|---|---|---|---|---|---|---|
| RPT-01 | Retail "till-date" lists: orders, order status, connections, bills, payments (shop-scoped) | NF-4 | SRC | M | N5–N10 | D-11 | Delivered with ORD-07, CON-06, BIL-11, PAY-02 |
| RPT-02 | Operational reports: orders by status, feasible/not feasible, active connections, unpaid/overdue bills, overdue connections (also readable by Technical), payments, stock, equipment issued | G1 | REC | S | N11 | D-14 | Each report returns correct counts on seed data |
| RPT-03 | Restricted CSV export for sensitive data | G1 | REC | C | post-MVP | — | Admin/Accounts only |
| NFR-01 | Responsive web app for desktop and mobile browsers | G1 | REC | M | N12 | — | No horizontal scroll at 360 px viewport width on login, order list, order form and bill view (Playwright for .NET viewport test) |
| NFR-02 | All input validated on the server with shared schemas | G1 | REC | M | all | — | Invalid input rejected with field errors |
| NFR-03 | No secrets in code or Git; configuration templates hold placeholders only | G1 | REC | M | N2 | D-30 | Secret scan clean; real values live only in user-secrets or environment variables |
| NFR-04 | Seed/demo data is synthetic and labelled; no real customer documents | G1 | REC | M | N14 | — | Seed script and demo users labelled |
| NFR-05 | Scheduled backups and a tested restore procedure | G1 | REC | M | N15 | D-29 | A SQL Server backup file is restored into a non-production database and the data verified; the rehearsal is documented |
| NFR-06 | Basic accessibility: labels, keyboard operation | G1 | REC | S | N12 | — | Forms operable by keyboard |
| NFR-07 | Documentation updated at the end of every node | Project instructions | REC | M | all | D-30 | `dotnet run --project tools/Nexus.DocsCheck -- docs` exits 0: no undefined decision or requirement references, and every Must requirement has a test |

## 4. Source coverage check

Every item of S1 maps to at least one requirement (or is context only).

| S1 item | Requirement IDs |
|---|---|
| INT-1 | Context only |
| INT-2 | ORD-05, CON-10, ADM-02 |
| INT-3 | FEA-02, FEA-03, CON-01 |
| INT-4 | ADM-03, EQP-02, EQP-03, EQP-05, AUTH-03 |
| EXS-1 | BIL-06, CUS-02, CUS-06, PAY-01, PAY-07, ORD-11, ORD-12 |
| EXS-2 | CUS-01, DOC-01, DOC-02, CON-05 |
| EXS-3 | ADM-03, EQP-03 |
| EXS-4 | BIL-03, EQP-04, EQP-07, EQP-10, PAY-02 |
| EXS-5 | AUTH-03, ORD-08, ORD-09, CON-08 |
| PRO-1 | ADM-02, ADM-03, ADM-04, AUTH-03 |
| PRO-2 | ORD-01, ORD-02, CON-02, SCH-01, SCH-02, PRT-01…PRT-03, PLN-09 |
| PRO-3 | CON-02, ADM-01 |
| PRO-4 | SCH-03, SCH-04 |
| PRO-5 | BIL-01, BIL-04 |
| PRO-6 | PRT-01…PRT-04, CUS-04, AUTH-07 |
| PRO-7 | Data model, architecture.md §5 |
| PRO-8 | SCH-01…SCH-04, ORD-11 |
| NF-1 | ADM-02…ADM-04, PLN-01, CUS-01, ORD-01, CON-05, EQP-01, BIL-01, PAY-02 |
| NF-2 | AUTH-01, AUTH-03, AUTH-08, ADM-04 |
| NF-3 | ORD-01, ORD-03, ORD-10 |
| NF-4 | ORD-07, CON-06, CUS-06, BIL-11, RPT-01 |
| NF-5 | BIL-01, PAY-01, PAY-07 |
| NF-6 | PLN-01 |
| NF-7 | FEA-01…FEA-04, CON-01, CON-04, EQP-09 |
| NF-8 | BIL-02 |
| NF-9 | ADM-02…ADM-04, PLN-01, EQP-01, ORD-11 |
| FIN-1 | BIL-01, BIL-11, PAY-03 |
| FIN-DEP / DU / BB / LL / TAX | PLN-02…PLN-08, BIL-06…BIL-09, CON-09 |
| FR-1 | PLN-01…PLN-04 |
| FR-2 | ADM-02 |
| FR-3 | ADM-04 |
| FR-4 | CUS-01, ADM-03 |
| FR-5 | ORD-01…ORD-06 |
| FR-6 | EQP-01 |
| FR-7 | EQP-04 |
| FR-8 | ORD-06, CON-05, PRT-04 |
| FR-9 | BIL-06, BIL-12, PAY-03 |

## 5. Out of scope for the 15-day MVP

| Item | Reason |
|---|---|
| Real network/server-capacity feasibility checks, automatic provisioning | No rule or API in S1 (D-13) |
| Online card/bank payments, recurring auto-collection | Not in S1; payments are recorded by staff |
| SMS/email notifications, external accounting/ERP, government tax APIs | Not in S1 |
| Native mobile app | Responsive web only |
| Legacy data migration | No legacy data or format supplied |
| Automatic suspension for non-payment, automatic hourly-plan metering | Policy undefined (D-04, D-14) |
| Multi-currency, multi-language | Single configurable currency, English |

## Appendix A — Source fact sheet (transcribed from S1; not verified as current)

### A1. Identifier rules

| Item | S1 says | Note |
|---|---|---|
| Order ID | "11 digit" alphanumeric, serial; first character D (dial-up), T (telephone only), B (broadband); example `D0000000001` | Example = 1 letter + 10 digits = 11 characters, so length is consistent |
| Account ID (Proposed ¶2) | "16 digit", "numeric code" | Conflicts with the Note below |
| Account ID (Note, ¶3) | 1 type letter (D/B/T) + 3-digit city code + 12-digit serial | 16 characters, not purely numeric |
| Account ID (Existing ¶2) | "unique 10 digit account id" | Conflicts with the above (D-01) |

### A2. Bulk/corporate discount (applies to "advance" and "security deposit")

| Connections | Discount in S1 | Problem |
|---|---|---|
| 1–9 | Not stated | Assumed 0% |
| 10–15 | 25% | 15 also appears in the next tier |
| 15–25 | 50% | 15 and 25 overlap |
| 25–50 | 75% | 25 and 50 overlap |
| Above 50 | 100% | Only strict wording; implies 50 belongs to the 75% tier |

"Advance" is not defined anywhere in S1 (D-03).

### A3. Pricing catalogue

| Category | Plan | Price | Validity / notes |
|---|---|---|---|
| Security deposit | Dial-up | $325 | Refundable on withdrawal/cancellation |
| Security deposit | Broadband | $500 | Refundable |
| Security deposit | Landline | $250 | Refundable |
| Dial-up hourly | 10 hrs | $50 | 1 month |
| Dial-up hourly | 30 hrs | $130 | 3 months |
| Dial-up hourly | 60 hrs | $260 | 6 months |
| Dial-up unlimited 28 Kbps | Monthly / Quarterly | $75 / $150 | Quarterly = 3 months (implied) |
| Dial-up unlimited 56 Kbps | Monthly / Quarterly | $100 / $180 | Quarterly implied |
| Broadband hourly | 30 hrs | $175 | 1 month |
| Broadband hourly | 60 hrs | $315 | 6 months |
| Broadband unlimited 64 Kbps | Monthly / Quarterly | $225 / $400 | Quarterly implied |
| Broadband unlimited 128 Kbps | Monthly / Quarterly | $350 / $445 | See anomaly 1 |
| Landline Local "Unlimited" | Rental $75 | 1 year | Calls $0.55/min — see anomaly 3 |
| Landline Local Monthly | Rental $35 | 1 month | Calls $0.75/min |
| Landline STD Monthly | Rental $125 | 1 month | Local $0.70 · STD $2.25 · mobile messaging $1.00 (per minute) |
| Landline STD Half-yearly | Rental $420 | S1 says "valid for a month" → assumed 6 months (D-06) | Local $0.60 · STD $2.00 · messaging $1.15 |
| Landline STD Yearly | Price blank in S1 → demo assumption $720.00 (D-05) | 1 year | Local $0.60 · STD $1.75 · messaging $1.25 |
| Service tax | 12.24% | — | "On the whole bill" — D-07 |

### A4. Pricing anomalies to confirm (see D-04)

1. Broadband 128 Kbps quarterly ($445) is only 27% above its monthly price ($350); the other three quarterly plans are 78–100% above their monthly price. Three months at the monthly price would be $1,050.
2. Mobile messaging is priced "per minute", and its rate rises with plan length ($1.00 → $1.15 → $1.25) while local and STD rates fall.
3. The landline Local plan is called "Unlimited" yet carries per-minute call charges.
4. Hourly plans: behaviour when hours run out or validity ends (overage, rollover, disconnection) and how hours used are captured are not stated.
5. Equipment (modem/router) is said to affect charges (EXS-2, PRO-5), but no equipment price, rental or deposit is given (D-15).
