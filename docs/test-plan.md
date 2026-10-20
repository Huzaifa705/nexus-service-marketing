# NEXUS Service Marketing System — Test Plan (Node 2 baseline)

| | |
|---|---|
| Version | 1.2 — Node 2 baseline — baseline tests written and passing (see §7); all other cases are planned only |
| Date | 2026-10-20 |
| Status | All cases below are `Not started`. Money-related vectors follow the decided rule set (D-03, D-07, D-14) and must be recalculated if either decision is reopened. |

## 1. Strategy

| Level | Tool | What it proves |
|---|---|---|
| Unit | Vitest | Pure domain logic: ID formats, discount tiers, tax/rounding, state transitions, permission rules |
| Integration | Vitest against a test database | Transactions, constraints, concurrency (IDs, stock, payments), role enforcement on every mutation |
| End-to-end | Playwright | Complete journeys through the real UI |
| Security review | Manual checklist + dependency audit | IDOR, uploads, secrets, injection, XSS |
| Documentation | `node scripts/check-docs.mjs` | Document consistency: references, IDs, Must-requirement test coverage |
| Acceptance (UAT) | Scripted scenario with the owner | Business correctness on staging (Day 14) |

Priorities if time is short (from the guide): authorisation, identifier correctness, money arithmetic, and the full order-to-bill journey come before anything cosmetic.

## 2. Entry and exit criteria per node

- **Entry:** requirements for the node are listed in `requirements-matrix.md`; blocking decisions are DECIDED or have a flagged default.
- **Exit:** lint, type-check, unit and integration tests pass with recorded output; the node's workflow was exercised with a test record; docs updated; known issues listed; work committed.
- A task is never reported complete without evidence (command output or a recorded manual result).

## 3. Test cases

Status for every case: `Not started`, except T-NFR-03, which is implemented in `tests/docs-check.test.ts` and passing (see §7).

### 3.1 Authentication, roles, scope

| ID | Level | Scenario | Expected result | Req. | Node |
|---|---|---|---|---|---|
| T-AUTH-01 | E2E | Each of the 5 roles signs in, reaches its home page, signs out | Session ends on sign-out | AUTH-01 | N3 |
| T-AUTH-02 | Integration | Call every mutation in the role matrix as every role (generated from the matrix) | Allowed roles succeed; all others get 403 | AUTH-02 | N3 onward |
| T-AUTH-03 | Integration | Use an expired or invalid session | 401 or redirect to login | AUTH-06 | N3 |
| T-AUTH-04 | Integration | Deactivate a user, then reuse their session | Request fails | AUTH-06 | N3 |
| T-AUTH-05 | Security | Customer A requests Customer B's order, bill, connection by ID | 403/404 | AUTH-05, SCH-05 | N10 |
| T-AUTH-06 | Integration | Shop-1 retail user reads or edits shop-2 orders | Denied (D-11: Retail = own shop) | AUTH-04 | N5 |
| T-AUTH-07 | Integration | Admin creates, edits and deactivates an employee; Accounts, Technical and Retail try the same | Admin succeeds and each role change is audited; others 403 | AUTH-03 | N4 |
| T-AUTH-08 | Integration | Admin invites a customer; Retail invites a customer of its own shop and of another shop; invite the same customer twice; use the customer login on a staff route | Admin and own-shop invites succeed; other-shop invite denied; second invite rejected; staff route returns 403 | AUTH-07 | N10 |
| T-AUTH-09 | Integration | Create users with a 9-character and a 10-character password; sign in with temporary credentials | 9 rejected, 10 accepted; the temporary-credential user must change the password before reaching any page | AUTH-08 | N3 |
| T-ADM-01 | Unit | City codes `001` valid; `01`, `abcd`, `12a`, duplicate rejected | Only valid unique codes saved | ADM-01 | N4 |
| T-ADM-02 | Integration | Delete a shop, vendor or plan that has history | Blocked; deactivate offered | ADM-05, PLN-01 | N4 |
| T-ADM-03 | Integration | Admin creates, edits, searches shops, vendors and employees; non-Admin tries the same | Admin succeeds; non-Admin gets 403 | ADM-02, ADM-03, ADM-04 | N4 |

### 3.2 Plans and pricing

| ID | Level | Scenario | Expected result | Req. | Node |
|---|---|---|---|---|---|
| T-PLN-01 | Integration | Compare seeded plans with requirements-matrix Appendix A3 | Seed matches exactly except the two assumed values (Yearly STD $720.00, Half-yearly STD 6 months), which carry an "assumed" flag; all flagged unverified | PLN-02 | N4 |
| T-PLN-02 | Integration | Activate any plan whose price or validity is empty | Rejected | PLN-07 | N4 |
| T-PLN-03 | Integration | Edit a plan price after a bill was issued | Existing bill unchanged; new bill uses new price | PLN-05, BIL-05 | N9 |
| T-PLN-04 | Integration | Non-Admin edits plans, tax rate, discount tiers | 403 | PLN-01, PLN-08 | N4 |
| T-PLN-05 | Unit | Save an hourly plan without hours or validity; save a landline plan without rental, validity or per-minute rates | Both rejected with field errors | PLN-03, PLN-04 | N4 |
| T-PLN-06 | Integration | Change the currency setting; scan templates for hard-coded currency symbols | Amounts re-format from the setting; no hard-coded symbol found | PLN-06 | N4 |

### 3.3 Orders

| ID | Level | Scenario | Expected result | Req. | Node |
|---|---|---|---|---|---|
| T-ORD-01 | Integration | Create a dial-up order in an empty database | ID is `D0000000001`; matches `^D[0-9]{10}$` | ORD-02 | N5 |
| T-ORD-02 | Integration | 50 parallel order creations of mixed types | 50 unique IDs; per-letter sequences contiguous | ORD-02 | N5 |
| T-ORD-03 | Unit | Telephone and broadband orders | Prefixes `T` and `B` | ORD-02 | N5 |
| T-ORD-04 | Unit | Try every status pair not in the state table | All rejected (table-driven) | ORD-04 | N5 |
| T-ORD-05 | Integration | Make a transition; try to edit or delete a history row | History row added; edit/delete rejected | ORD-03 | N5 |
| T-ORD-06 | Integration | Submit a dial-up order without landline information | Rejected with field error | ORD-05 | N5 |
| T-ORD-07 | Integration | Place an order as Admin, Accounts, Technical, Customer | 403 for all (D-08: retail only) | ORD-01 | N5 |
| T-ORD-08 | Integration | Cancel an order with a reason, then try to provision it | Cancel saved; provisioning rejected | ORD-08, CON-01 | N5 |
| T-ORD-10 | Integration | Create an order with all fields | Shop, employee, timestamp, plan, equipment choice, installation address and city, bulk reference and notes persist and show on the detail page | ORD-06 | N5 |
| T-ORD-11 | Integration | Retail lists orders for its shop and filters by status | Only own-shop orders appear; the filter is exact | ORD-07 | N5 |
| T-ORD-12 | Unit | Attempt every transition as every role (generated from the §7 actor table) | Only the listed actors succeed | ORD-04 | N5 |
| T-CUS-01 | Integration | Create a customer with missing required fields, then valid; repeat the primary contact number at the same shop | Field errors; valid record saved; duplicate warning | CUS-01 | N5 |
| T-CUS-02 | Security | Desk lookup by exact, partial and wildcard contact number; open the full record of another shop's customer | Exact match returns name, city and masked contact only; partial and wildcard return nothing; full record denied; each lookup audited | CUS-06 | N5 |
| T-ORD-09 | Integration | Link 16 orders in one bulk application; cancel one, then mark another Not feasible and reject a third; also issue a bill before the changes | 16 → 50% tier; after the cancel 15 → 25%; Not feasible and Rejected orders stop counting (14, 13 → 25%); the bill issued earlier keeps its tier | ORD-12, BIL-06 | N5, N9 |

### 3.4 Feasibility and connections

| ID | Level | Scenario | Expected result | Req. | Node |
|---|---|---|---|---|---|
| T-FEA-01 | Integration | Technical and other roles open the feasibility queue | Technical can act; Admin can read; Accounts, Retail, Customer denied | FEA-01 | N6 |
| T-FEA-02 | Integration | Mark Not feasible without a reason, then with one; later re-decide | First rejected; second saved; earlier decision still visible | FEA-04 | N6 |
| T-FEA-03 | Integration | Dial-up with new landline vs existing landline | Two checks required vs one | FEA-02 | N6 |
| T-FEA-04 | E2E | Not-feasible order: attempt to create a connection via UI and API | Rejected; no connection row; no account ID consumed | CON-01 | N6, N7 |
| T-FEA-05 | Integration | Record a feasibility check with distance, server availability and notes; try as non-Technical | Fields saved per check; non-Technical denied | FEA-03 | N6 |
| T-FEA-06 | Integration | Retail (own shop), Retail (other shop) and the customer view an order's feasibility outcome and reason | Own shop and owning customer see it; the other shop does not | FEA-05 | N6 |
| T-CON-01 | Integration | Provision a feasible order | ID matches `^[DBT][0-9]{15}$`; type letter and city code correct | CON-02 | N7 |
| T-CON-02 | Integration | 50 parallel provisions | 50 unique account IDs | CON-02 | N7 |
| T-CON-03 | Integration | Attempt to update an account ID | Rejected | CON-03 | N7 |
| T-CON-04 | Unit | Active → Temp → Active → Permanent; then Permanent → Active | Last step rejected; actor/reason stored | CON-04 | N7 |
| T-CON-05 | Integration | Provision a dial-up connection with no landline reference | Rejected (per D-10) | CON-10 | N7 |
| T-CON-06 | Integration | Provision from orders with different types and installation cities | Type letter equals order type; city code equals installation city | CON-02 | N7 |
| T-CON-07 | Integration | Open a connection after provisioning and equipment issue | Customer, order, plan, city and start date shown; equipment listed once issued | CON-05 | N7, N8 |
| T-CON-08 | Integration | Retail lists connections | Only own-shop connections appear | CON-06 | N7 |

### 3.5 Equipment

| ID | Level | Scenario | Expected result | Req. | Node |
|---|---|---|---|---|---|
| T-EQP-01 | Integration | Receive a purchase order | Stock rises; ledger sum equals on-hand | EQP-02, EQP-03 | N8 |
| T-EQP-02 | Integration | Issue more units than on hand | Rejected; stock unchanged | EQP-04 | N8 |
| T-EQP-03 | Integration | Two simultaneous issues of the last unit | Exactly one succeeds; stock never negative | EQP-02 | N8 |
| T-EQP-04 | Integration | Issue, return, mark damaged | Movements reconcile with on-hand | EQP-02, EQP-08 | N8 |
| T-EQP-05 | Integration | Technical edits catalogue/vendor; Admin records an issue (per D-09) | Technical denied catalogue edits; role split enforced | EQP-09 | N8 |
| T-EQP-06 | Integration | Record customer-caused damage | Billable item available to Accounts | EQP-07 | N8, N9 |
| T-EQP-07 | Integration | Admin creates a product with a vendor link and searches the catalogue; Technical tries to edit it | Admin succeeds; Technical denied | EQP-01 | N8 |
| T-EQP-08 | Integration | Order with equipment not eligible for the plan; plan with no eligibility list | Ineligible rejected; no list allows any active product | EQP-10 | N8 |
| T-EQP-09 | Integration | Stock adjustment without a reason; as Technical; opening stock as Admin | Reason required; Technical denied; opening stock accepted and audited | EQP-02 | N8 |

### 3.6 Billing and payments

| ID | Level | Scenario | Expected result | Req. | Node |
|---|---|---|---|---|---|
| T-BIL-01 | Integration | Non-Accounts roles create or edit a bill | 403 | BIL-01 | N9 |
| T-BIL-02 | Unit | Tax vectors TV-T1…TV-T4 (§4) | Totals match | BIL-07, BIL-10 | N9 |
| T-BIL-03 | Unit | Discount boundary vectors TV-D (§4) | Percentages match | BIL-06 | N9 |
| T-BIL-04 | Integration | Change plan, tax rate, tiers after billing | Issued bill unchanged | BIL-05 | N9 |
| T-BIL-05 | Unit | Property test: random line items | Sum of lines = total; no rounding drift | BIL-03 | N9 |
| T-BIL-06 | Unit | Three lines of 0.10 | Total exactly 0.30 (no floating-point error) | BIL-10 | N9 |
| T-BIL-07 | Integration | Void a bill without a reason; pay a void bill | Both rejected | BIL-14 | N9 |
| T-BIL-08 | Integration | Issue 50 bills in parallel | Bill numbers unique, format `BL-YYYY-NNNNNN` | BIL-04 | N9 |
| T-BIL-09 | Unit | Issue a bill dated 2026-11-01 | Due date 2026-11-16 (15 days, setting-driven) | BIL-04 | N9 |
| T-BIL-10 | Integration | Generate a bill for a connection with a plan and equipment | Lines reference the plan version and equipment; totals match TV-T vectors | BIL-02 | N9 |
| T-BIL-11 | Integration | Deposit with a 25% discount, then a partial refund | Deposit line labelled refundable; balance = deposit charged after discount − refunds (TV-R1) | BIL-08 | N9 |
| T-BIL-12 | Integration | Enter usage minutes per call type | Charge = minutes × snapshot rate; entry audited | BIL-09 | N9 |
| T-BIL-13 | Integration | View paid and due on a bill and per connection as staff, the owning customer and another customer | Staff and owner see them; others denied | BIL-11 | N9, N10 |
| T-BIL-14 | Integration | Leave a balance and a credit, then issue the next bill | Previous balance and credit appear as separate lines | BIL-12 | N9 |
| T-PAY-01 | Integration | Record payment as Retail, Accounts, then other roles | First two succeed; others 403 | PAY-01 | N9 |
| T-PAY-02 | Unit/Integration | Payment vectors TV-P1…TV-P4 (§4) | Due and credit match | PAY-03, PAY-05 | N9 |
| T-PAY-03 | Integration | Submit the same payment (same idempotency key) twice | One payment | PAY-04 | N9 |
| T-PAY-04 | Integration | Two concurrent payments on one bill | Due amount correct afterwards | PAY-03 | N9 |
| T-PAY-05 | Integration | Record a payment | Receipt record created | PAY-06 | N9 |
| T-PAY-06 | Integration | Record 50 payments in parallel | Receipt numbers unique, format `RC-YYYY-NNNNNN` | PAY-06 | N9 |
| T-PAY-07 | Integration | Record a payment and try to edit its recorded-by | Amount, date, method, reference, recorded-by and shop/office persist; edit rejected | PAY-02 | N9 |
| T-PAY-08 | Security | Retail takes a payment for another shop's connection via exact account-ID lookup | Lookup shows only customer name, account ID and amount due; payment stores the receiving shop; no other bill detail visible | PAY-07 | N9 |

### 3.7 Search, portal, documents, audit

| ID | Level | Scenario | Expected result | Req. | Node |
|---|---|---|---|---|---|
| T-SCH-01 | Integration | Search by valid, unknown and malformed order ID | Status / clear not-found / validation message | SCH-01 | N10 |
| T-SCH-02 | Integration | Search by account ID | Status and due amount returned; scoped | SCH-02 | N10 |
| T-SCH-03 | Integration | Each advanced filter alone and in combination | Correct results | SCH-03, SCH-04 | N10 |
| T-SCH-04 | Integration | Date range at start/end boundaries and midnight | Inclusive and time-zone correct | SCH-03 | N10 |
| T-SCH-05 | Integration | Query with no matches | Empty state, no error | SCH-03 | N10 |
| T-SCH-06 | Security | Search by another person's name or phone as a customer | No records of others returned | SCH-05 | N10 |
| T-PRT-01 | E2E | Customer views own order, connection, bills | Own data only | PRT-01…PRT-03 | N10 |
| T-PRT-02 | Integration | Submit feedback on own order | Saved and linked | PRT-04 | N10 |
| T-PRT-03 | Integration | Unauthenticated status lookup | Not available (D-12) | PRT-05 | N10 |
| T-RPT-01 | Integration | Retail opens the till-date lists (orders, statuses, connections, bills, payments) | Only own-shop records; counts match the seed | RPT-01 | N10 |
| T-DOC-01 | Integration | Upload disallowed type and oversize file | Rejected | DOC-03 | N11 |
| T-DOC-02 | Security | Open a document URL without signing in | Denied | DOC-01 | N11 |
| T-DOC-03 | Security | Reuse a signed URL after expiry | Denied | DOC-01 | N11 |
| T-DOC-04 | Integration | Filter documents by year and city | Correct results | DOC-02 | N11 |
| T-DOC-05 | Integration | Open a customer's document list as Retail (own shop), Retail (other shop) and the customer | Own shop and owner see it; other shop denied | CUS-03, DOC-01 | N11 |
| T-AUD-01 | Integration | Perform each audited action | An audit row exists for each | AUD-01 | N11 |
| T-AUD-02 | Integration | Inspect every core table (schema test) | Created/updated timestamps and responsible user present | AUD-02 | N3 |

### 3.8 Security and end-to-end

| ID | Level | Scenario | Expected result | Req. | Node |
|---|---|---|---|---|---|
| T-SEC-01 | Security | Script text in names, notes, reasons | Rendered inert (no XSS) | NFR-02 | N13 |
| T-SEC-02 | Security | SQL metacharacters in search fields | No injection; safe results | NFR-02 | N13 |
| T-SEC-03 | Security | Scan repository and browser bundle for secrets | None found; service-role key not in bundle | NFR-03 | N13 |
| T-SEC-04 | Security | Dependency audit | No critical/high issues affecting release | NFR-03 | N13 |
| T-NFR-01 | E2E | Open login, order list, order form and bill view at 360 px width | No horizontal scroll | NFR-01 | N12 |
| T-NFR-02 | Integration | Run the seed script and inspect users and records | Every demo user and record is labelled synthetic; no real documents present | NFR-04 | N14 |
| T-NFR-03 | Automated | `npm run docs:check` | Exit code 0 | NFR-07 | every node |
| T-E2E-01 | E2E | Retail creates customer and order → feasibility → connection → issue equipment → bill → payment → due | Every step persists; due reconciles | all core | N12 |
| T-E2E-02 | E2E | Not-feasible order journey | No connection created; reason visible | CON-01 | N12 |
| T-E2E-03 | E2E | Customer portal journey | Own order, connection, bill, feedback | PRT-01…PRT-04 | N12 |
| T-E2E-04 | Manual | Backup restore rehearsal in a non-production environment | Data restored; steps documented | NFR-05 | N15 |

### 3.9 Should-priority cases (run only if the requirement is built)

| ID | Level | Scenario | Expected result | Req. | Node |
|---|---|---|---|---|---|
| T-SHD-01 | Integration | Retail and customer open the plan list | Only active plans with description and price | PLN-09 | N10 |
| T-SHD-02 | Unit | Store individual and corporate customers; create a bulk application for each | Type saved; both can group orders (flag is informational) | CUS-02 | N5 |
| T-SHD-03 | Integration | Customer edits a contact field and a non-contact field | Contact field saved and audited; other field rejected | CUS-04 | N10 |
| T-SHD-04 | Integration | Change the plan on an order before and after provisioning | Allowed before with a history row; rejected after | ORD-09 | N5 |
| T-SHD-05 | Integration | Overdue bill exists | Overdue report lists the connection; status unchanged | CON-07 | N9 |
| T-SHD-06 | Integration | Make a connection Permanently inactive and record a refund | Flagged for refund review; refund stores amount, date, reason, actor | CON-09 | N9 |
| T-SHD-07 | Integration | Orders in Awaiting equipment versus stock on hand | Shortfall per product is correct | EQP-05 | N8, N11 |
| T-SHD-08 | Integration | Set a threshold and lower stock to it | Product appears in the low-stock report | EQP-06 | N11 |
| T-SHD-09 | Integration | Product with blank and with set customer charge | Blank adds no bill line; set value adds a line | BIL-15 | N8 |
| T-SHD-10 | Integration | Save the document checklist on an order | Checklist persists | DOC-04 | N5 |
| T-SHD-11 | Integration | Admin filters the audit log by user, entity and date; others try | Correct rows; others 403 | AUD-03 | N11 |
| T-SHD-12 | Integration | Run each operational report on seed data | Counts match the seed | RPT-02 | N11 |
| T-SHD-13 | E2E | Complete login and order forms with the keyboard only | Every field reachable and labelled | NFR-06 | N12 |

## 4. Test vectors (decided rule set; recompute if D-03 or D-07 are reopened)

**Tax and discount (rate 12.24%; tax after discount; refundable deposit excluded from the taxable base; round half-up to 2 decimals once per bill).**

| ID | Inputs | Expected |
|---|---|---|
| TV-T1 | Broadband 128 Kbps monthly rental 350.00 + deposit 500.00, no discount | Taxable 350.00; tax 42.84; total 892.84 |
| TV-T1-alt | Same bill with `deposit_taxable = true` (the literal reading) | Taxable 850.00; tax 104.04; total 954.04 (not the default; kept for the flag test) |
| TV-T2 | Same bill, 25% bulk discount on advance (first-period rental 350.00) and deposit 500.00 | Discount 212.50; taxable 262.50; tax 32.13; total 669.63 |
| TV-T3 | Landline STD monthly: rental 125.00; local 100 min × 0.70; STD 20 min × 2.25; messaging 10 × 1.00 | Subtotal 250.00; tax 30.60; total 280.60 (messaging unit pending D-04) |
| TV-T4 | Rounding: taxable 33.33; taxable 10.01 | Tax 4.08 (4.079592); tax 1.23 (1.225224) |
| TV-T5 | 51-connection group (100% tier): rental 350.00 (advance) + deposit 500.00 | Discount lines −350.00 and −500.00; taxable 0.00; tax 0.00; total 0.00 |

**Bulk discount tiers (decided: upper-bound inclusive).**

| ID | Connections | Expected discount |
|---|---|---|
| TV-D1 | 9 | 0% |
| TV-D2 | 10 | 25% |
| TV-D3 | 15 | 25% |
| TV-D4 | 16 | 50% |
| TV-D5 | 25 | 50% |
| TV-D6 | 26 | 75% |
| TV-D7 | 50 | 75% |
| TV-D8 | 51 | 100% |
| TV-D9 | Group of 16 orders, one Not feasible (15 eligible) | 25% |

**Deposit refund.**

| ID | Inputs | Expected |
|---|---|---|
| TV-R1 | Deposit 500.00 with 25% discount (charged 375.00); refund 100.00 | Refundable balance 275.00 |

**Payments against a bill total of 892.84.**

| ID | Action | Expected |
|---|---|---|
| TV-P1 | Pay 0.00 | Rejected (validation); due stays 892.84 |
| TV-P2 | Pay 300.00 | Due 592.84; partly paid |
| TV-P3 | Then pay 592.84 | Due 0.00; paid |
| TV-P4 | Single payment of 900.00 | Due 0.00; credit 7.16 held for the next bill |

## 5. Defect severity

| Level | Meaning | Rule |
|---|---|---|
| S1 Critical | Data exposure, wrong money, duplicate IDs, bypassed permissions | Blocks release; fix and add a regression test |
| S2 High | Core workflow broken, no workaround | Fix before staging acceptance |
| S3 Medium | Workaround exists | Fix if time allows; list in known-issues.md |
| S4 Low | Cosmetic | Defer |

## 6. Evidence rule

Each test run records: the command, the date, and the exact pass/fail output. Unrun checks are listed as unrun. Results are copied into the end-of-node checkpoint and into this file's status column.

## 7. Run log

| Date | Command | Result | Notes |
|---|---|---|---|
| 2026-10-20 | `npm run verify` (docs check, lint, type-check, tests, build) in the build sandbox | Exit 0. Lint: 0 warnings. Vitest: 3 files, 4 tests passed (health endpoint, navigation, documents exist, documents consistent). Production build succeeded; routes `/` (static), `/health` (dynamic). | Node 2 baseline. T-NFR-03 passing. |
| 2026-10-20 | `next start`, then `curl` of `/`, `/health`, an unknown route | `/` 200, `/health` 200 JSON with `Cache-Control: no-store`, unknown route 404; baseline security headers present; no `X-Powered-By`; compiled CSS contains Tailwind classes | Manual runtime check, not an automated test |
| 2026-10-20 | `npm audit --omit=dev` | 0 vulnerabilities | Full `npm audit`: 5 high, one dev-only chain with no upstream fix (KI-N2-01) |
