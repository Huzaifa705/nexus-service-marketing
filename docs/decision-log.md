# NEXUS Service Marketing System — Decision Log

| | |
|---|---|
| Version | 1.2 — Node 2 baseline |
| Date | 2026-10-20 |
| Rule | On 2026-10-20 the owner accepted all recommended defaults and delegated every remaining gap to me ("assume what is appropriate; do what normally happens in comparable software; double-check"). Every item below is therefore **DECIDED (by delegation)**. These decisions fix system behaviour and configuration. They do not verify real-world facts: prices, the tax rate and the currency stay flagged unverified until confirmed (see "Pre-go-live confirmations"). |
| Authority | On 2026-10-20 the owner also authorised me to make all routine technical, architectural, documentation and project-management decisions without per-step approval, to resolve specification ambiguities by choosing the safest and most maintainable interpretation, and to proceed through the nodes autonomously. Every such decision is recorded here with its rationale. Anything that needs the owner's account, a secret or an external approval is listed in the audit report as an owner action. |

**Status:** `DECIDED` approved by the owner on 2026-10-20 (by delegation). A decision that is reopened later returns to `OPEN` and is logged in the change history.
**Tier** (original urgency, kept for traceability): `T1` answer was needed before the stated day · `T2` default was proceeded on · `T3` flagged demo data until confirmed before go-live.

## Summary

| ID | Topic | Tier | Status | Needed by | Decision |
|---|---|---|---|---|---|
| D-01 | Account ID format | T1 | DECIDED | Day 3 | 16 chars: letter + 3-digit city + 12-digit serial; legacy ID stored separately |
| D-02 | Order ID format and counter | T2 | DECIDED | Day 5 | 11 chars: letter + 10 digits; separate counter per letter |
| D-03 | Bulk discount boundaries, "advance", counting basis | T1 | DECIDED | Day 9 | Upper-bound inclusive: 10–15 → 25%, 16–25 → 50%, 26–50 → 75%, 51+ → 100%; counted per bulk application (eligible orders in the group) |
| D-04 | Currency, price validity, price anomalies | T3 | DECIDED | Day 8 / go-live | USD as demo; all seed prices flagged unverified |
| D-05 | Yearly STD rental price (blank) | T3 | DECIDED | Day 8 / go-live | Demo assumption $720.00 ($60/month), flagged "assumed — confirm" |
| D-06 | Half-yearly STD validity wording | T3 | DECIDED | Day 8 / go-live | 6 months, flagged "assumed — confirm" |
| D-07 | Service tax base, order, deposits, rounding | T1 | DECIDED | Day 9 | Tax on charges after discount; refundable deposit not taxed; round once per bill |
| D-08 | Who can place orders | T1 | DECIDED | Day 5 | Retail employees only; customer ordering later |
| D-09 | Equipment/stock split between Admin and Technical | T1 | DECIDED | Day 3 | Admin = catalogue/vendors/POs/corrections; Technical = receipt/issue/return/damage |
| D-10 | Combined landline + dial-up applications | T1 | DECIDED | Day 5 | Two linked orders (T and D), each with its own IDs |
| D-11 | Data scope per role (shop/city) | T1 | DECIDED | Day 3 | Retail = own shop; Admin, Accounts, Technical = all |
| D-12 | Customer login identity and public lookup | T2 | DECIDED | Day 10 | Email login, staff-invited; no public lookup |
| D-13 | Feasibility = manual technical decision | T2 | DECIDED | Day 6 | Manual with structured fields |
| D-14 | Billing mechanics (cycle, usage entry, status effect, temp-inactive, credit) | T2 | DECIDED | Day 8 | Manual by Accounts; no automatic suspension |
| D-15 | Equipment charges | T2 | DECIDED | Day 8 | Optional charge per product; blank = free |
| D-16 | Plan changes | T2 | DECIDED | Day 5 | Edit plan on order before provisioning only; live changes post-MVP |
| D-17 | Deposit refunds | T2 | DECIDED | Day 8 | Track deposit; Accounts records refund manually |
| D-18 | Working mode and release target | T1 | DECIDED | Day 2 | Files + commands in chat (repo workspace if available); staging-grade MVP with a Day 15 go/no-go gate; owner approves rules |
| D-19 | Enquiry log | T2 | DECIDED | Day 5 | Out of MVP |
| D-20 | Bill/receipt numbers, payment due days, time zone | T2 | DECIDED | Day 4 | `BL-YYYY-NNNNNN`, `RC-YYYY-NNNNNN`; due 15 days after issue; display time zone is a setting |
| D-21 | Customer fields, installation address, feedback format | T2 | DECIDED | Day 5 | See entry |
| D-22 | Cross-shop desk lookup (customer by phone, connection for payments) | T2 | DECIDED | Day 5 | Exact-match, minimal-view lookups for Retail; everything else stays own-shop |
| D-23 | Stock model, equipment issue timing, plan-to-equipment options | T2 | DECIDED | Day 8 | One central stock pool; "Awaiting equipment" is advisory; issue follows connection creation; plans list eligible equipment |
| D-24 | Order-status transitions: who may perform each | T2 | DECIDED | Day 5 | Actor table in architecture.md §7 |
| D-25 | Authorisation strategy, password policy, portal invitations, document limits | T2 | DECIDED | Day 3 | Server-side checks + RLS deny-by-default; ≥10-char passwords; staff-invited customers; signed URLs ≤300 s |
| D-26 | Hosting plan constraints (Vercel, Supabase) | T3 | DECIDED | Day 2 / go-live | Provider-neutral build; free tiers for development and non-commercial demo only; paid plans before live business use |
| D-27 | Framework configuration and tooling (Node 2) | T2 | DECIDED | Day 2 | Cache Components off; system fonts; shadcn/ui deferred to Node 4; baseline security headers; CI gates on the production audit |

---

## D-01 Account ID format

- **Status / Tier / Needed by:** DECIDED · T1 · Day 3 (schema) · **Affects:** CON-02, CON-03, CUS-05, SCH-02
- **Source evidence:** Existing Scenario ¶2 mentions a "unique 10 digit account id". Proposed Solution ¶2 says "unique 16 digit code … (numeric code)". The Note (Proposed ¶3) defines 1 type letter (D/B/T) + 3-digit city code + 12-digit serial.
- **Problem:** Three incompatible descriptions: 10-digit numeric, 16-digit numeric, and 16 characters starting with a letter. Also unstated: whether the 12-digit serial is global, per type or per city; which "city" is meant (customer address, shop, installation site); what happens if type or city changes.
- **Options:**
  - **A (recommended)** 16 characters `[D|B|T]` + 3-digit city + 12-digit serial for all new connections. Legacy 10-digit IDs, if any, go in a separate `legacy_account_id`.
  - **B** Keep 10-digit numeric IDs. Contradicts the Note and cannot carry type or city.
  - **C** Purely numeric 16 digits. Would need an invented numeric code for the type, which S1 does not supply.
- **Sub-defaults under A:** (i) one global 12-digit counter ("serial order of the connection assigned"); (ii) city = city of the installation address, chosen from the city table; (iii) a change of type or city is a new connection with a new ID; (iv) IDs are never edited or reused; (v) issued only at provisioning.
- **If we guess wrong:** IDs already given to staff or customers would need remapping. Cheap before the first real record, expensive after.
- **Decision (2026-10-20, by delegation):** Option A with sub-defaults (i)–(v): 16 characters `[D|B|T]` + 3-digit city + 12-digit global serial; city = the order's installation city; legacy ID kept in a separate field.

## D-02 Order ID format and counter

- **Status / Tier / Needed by:** DECIDED · T2 · Day 5 · **Affects:** ORD-02
- **Source evidence:** Proposed ¶2: "11 digit" alphanumeric code, serial, first character D/T/B, example `D0000000001`.
- **Analysis:** The example is 1 letter + 10 digits = 11 characters, so the stated length is consistent; "digit" is used loosely. Length is therefore not ambiguous. The only open point is whether the counter is per letter or shared.
- **Options:** **A (default)** one counter per letter (first dial-up order `D0000000001`, first broadband order `B0000000001`), which matches the wording "the first order for a dial-up connection". **B** one counter shared across letters.
- **Rules applied either way:** format `^[DTB][0-9]{10}$`; IDs never reused after cancellation; the counter increments in the same transaction as the order insert; capacity 9,999,999,999 per letter.
- **Decision (2026-10-20, by delegation):** Default accepted: 11 characters (letter + 10 digits), one counter per letter, IDs never reused.

## D-03 Bulk/corporate discount

- **Status / Tier / Needed by:** DECIDED · T1 · Day 9 (tiers seeded on Day 4) · **Affects:** BIL-06, CUS-02, ORD-06, ORD-12
- **Source evidence:** Existing Scenario ¶1: 10–15 connections → 25% off advance and security deposit; 15–25 → 50%; 25–50 → 75%; above 50 → 100%.
- **Problems:** (1) ranges overlap at 15, 25 and 50; (2) 1–9 connections not stated; (3) "advance" is not defined anywhere in S1; (4) what counts toward the number of connections (one order, one corporate customer in total, or currently active) is not stated; (5) the 100% tier waives the deposit entirely.
- **Boundary options:**
  - **A (default)** upper-bound inclusive, consistent with the one strict wording ("above 50"): 10–15 → 25%; 16–25 → 50%; 26–50 → 75%; 51+ → 100%; 1–9 → 0%.
  - **B** lower-bound inclusive: 10–14 → 25%; 15–24 → 50%; 25–49 → 75%; 50+ → 100%.
  - **C** owner supplies the table.
- **Money impact:** a corporate customer with exactly 15 broadband connections has total deposits of 15 × $500 = $7,500. The deposit discount is $1,875 under A and $3,750 under B (advance not included).
- **"Advance" default:** the first billing period's rental paid at activation (decided by delegation; confirm before go-live). See the v1.1 amendment below.
- **Counting default:** connections in one bulk application (a group of orders placed together by the same customer; one order = one connection = one account ID).
- **Implementation:** tiers live in a table (min, max, percent, explicit inclusive flags), editable by Admin. The discount engine is built on Day 9 from this decision; the tiers are flagged for confirmation before go-live.
- **Decision (2026-10-20, by delegation):** Option A (upper-bound inclusive): 1–9 → 0%; 10–15 → 25%; 16–25 → 50%; 26–50 → 75%; 51+ → 100%. "Advance" = the first billing period's rental charged at activation. Counting basis = a *bulk application*: a group of orders placed together by the same customer (one order = one connection = one account ID). The tier is set by the number of non-cancelled orders in the group when the bill is created, and the percentage applies to each connection's own advance and deposit. Tiers are Admin-editable configuration.
- **Amendment (v1.1, 2026-10-20, audit):** Eligible orders (those that count toward the group size) are orders in the group whose status is not Cancelled, Rejected or Not feasible. The tier is fixed when each connection's initial bill is created; later cancellations never change issued bills. Bulk applications are available for any customer type (the individual/corporate flag is informational). "Advance" is not a separate charge: it is the `is_advance` flag on the first-period rental line of the initial bill. Only that line and the security deposit are discounted; later bills are not. The discount appears as its own negative bill lines (one for the advance, one for the deposit).

## D-04 Currency, price validity, price anomalies

- **Status / Tier / Needed by:** DECIDED · T3 · Day 8 / go-live · **Affects:** PLN-02, PLN-04, PLN-06, BIL-13
- **Source evidence:** Financial section uses "$" and "cents"; no country or currency is named; prices are undated.
- **To confirm before go-live:** (1) currency and symbol; (2) whether every listed price is current; (3) the anomalies in requirements-matrix.md Appendix A4: broadband 128 Kbps quarterly $445; mobile messaging billed "per minute" with a rising rate; the "Unlimited" landline Local plan with per-minute charges; hourly-plan behaviour when hours or validity run out.
- **Default:** currency setting = USD (demo); every seeded price flagged "verify before production"; hourly overage handled by manual bill lines.
- **If we guess wrong:** wrong bills. Prices are data, so correction is an Admin edit; issued bills keep their snapshots.
- **Decision (2026-10-20, by delegation):** Currency = USD, stored as a setting. Every price in requirements-matrix Appendix A3 is loaded as stated in S1 and flagged "unverified — demo". Anomalies are kept exactly as in S1 and flagged: broadband 128 Kbps quarterly $445; messaging rate stored as stated (per minute) in a unit field; the local landline plan keeps its name with per-minute charges. Hourly plans: no automatic overage or rollover; Accounts adds manual lines.

## D-05 Yearly STD rental price

- **Status / Tier / Needed by:** DECIDED · T3 · Day 8 / go-live · **Affects:** PLN-04, PLN-07
- **Source evidence:** Financial section lists "Yearly – $ (Valid for an year)" with no amount; call rates Local $0.60, STD $1.75, messaging $1.25.
- **Options:** **A (default)** seed the plan inactive; Admin enters the price when known. **B** owner supplies the price now. **C** I pick a placeholder: rejected, because it would invent a financial value.
- **Consequence:** the plan cannot be sold until a price exists.
- **Decision (2026-10-20, by delegation):** Demo assumption: Yearly STD rental = $720.00 ($60/month), continuing the discount trend in S1 ($125/month monthly → $70/month half-yearly → $60/month yearly). Seeded active but flagged "assumed demo price — confirm before go-live".

## D-06 Half-yearly STD plan validity

- **Status / Tier / Needed by:** DECIDED · T3 · Day 8 / go-live · **Affects:** PLN-04, PLN-07
- **Source evidence:** The plan is named Half-yearly, costs $420, and is described as "valid for a month".
- **Analysis:** $420 is about 3.4× the monthly STD rental ($125), and the yearly plan is "valid for an year", so a one-month validity looks like a typo.
- **Options:** **A** 6 months (more plausible). **B** 1 month, as literally written.
- **Default:** seed inactive with validity unset until confirmed. A staging demo may show 6 months, clearly flagged.
- **Decision (2026-10-20, by delegation):** Half-yearly STD validity = 6 months, flagged "assumed — confirm before go-live".

## D-07 Service tax

- **Status / Tier / Needed by:** DECIDED · T1 · Day 9 · **Affects:** BIL-07, BIL-08, BIL-10, PLN-08
- **Source evidence:** Financial section: service tax 12.24% "on the whole bill generated".
- **Questions:** (a) is 12.24% current, and from what date; (b) is the refundable deposit taxable; (c) is tax computed before or after discounts; (d) rounding rule.
- **Options:**
  - **A** literal: tax on every bill line including the refundable deposit.
  - **B (recommended)** tax on rental, advance, usage and equipment charges after discounts; refundable deposit excluded.
  - **C** tax before discounts.
- **Worked example** (broadband 128 Kbps monthly rental $350 + deposit $500, no discount): under A tax = 12.24% × $850 = $104.04, total $954.04. Under B tax = 12.24% × $350 = $42.84, total $892.84. Difference: $61.20 per bill.
- **Rounding default:** tax computed once per bill on the taxable subtotal, rounded half-up to 2 decimals.
- **Consequence of A:** tax would be charged on money that is later refunded, which needs credit-note rules.
- **Implementation:** `deposit_taxable` flag (default false) and rate with effective date in configuration.
- **Decision (2026-10-20, by delegation):** Option B: 12.24% tax on rental, advance, usage and equipment charges after discounts; the refundable security deposit is excluded (`deposit_taxable = false`); tax computed once per bill and rounded half-up to 2 decimals. Rate and flag are configurable with effective dates; the rate is flagged unverified.
- **Amendment (v1.1, 2026-10-20, audit):** Tax is computed per bill line: every line has a `taxable` flag (rental, usage, equipment and adjustment lines default to taxable; the deposit line follows `deposit_taxable`, default false). Taxable base = sum of taxable lines minus the discount lines that reduce taxable lines. Worked example in `architecture.md` §8; vectors in `test-plan.md` §4.

## D-08 Who can place orders

- **Status / Tier / Needed by:** DECIDED · T1 · Day 5 · **Affects:** ORD-01, ORD-10, AUTH-02
- **Source evidence:** Non-Financial item 3: only members/employees of the retail outlet can place orders. Proposed ¶2 and ¶6: the customer can place an order and gets an order ID.
- **Options:**
  - **A (recommended for MVP)** retail employees only. Customers track status in the portal.
  - **B** retail employees plus customer self-service online orders.
  - **C** customers submit a request that staff convert into an order.
- **Consequences:** B and C need identity verification, customer-side document upload, and a way to handle unverified applicants who consume order IDs. That is extra scope and security risk inside 15 days.
- **Default:** A. The schema carries `channel` (retail/online) and `placed_by` from Day 5, so B or C can be switched on later without a migration.
- **Decision (2026-10-20, by delegation):** Option A: retail employees only in the MVP. The schema carries `channel` and `placed_by` so customer ordering can be added later.

## D-09 Equipment and stock responsibilities

- **Status / Tier / Needed by:** DECIDED · T1 · Day 3 (role matrix) · **Affects:** EQP-09, EQP-02, EQP-03, AUTH-02
- **Source evidence:** Intro ¶4: Admin takes care of product details, retail shops, vendors, purchase lists. Existing ¶5: equipment, employees, vendors maintained and updated "solely" by Admin. Proposed ¶1: Admin enters stock details. Non-Financial item 7: Technical "will maintain the details of the products or the equipments".
- **Options:**
  - **A (recommended)** split. Admin: product catalogue, vendors, purchase orders, stock corrections. Technical: goods receipt, issue to connection, return, damaged/replacement entries (read access to the catalogue; cannot edit prices or vendors).
  - **B** Admin only (literal Existing ¶5). Technicians could not record issues they perform on site.
  - **C** both roles full rights. Overlap makes the audit trail ambiguous.
- **Default:** A.
- **Decision (2026-10-20, by delegation):** Option A split: Admin = catalogue, vendors, purchase orders, stock corrections; Technical = goods receipt, issue, return, damage.
- **Amendment (v1.1, 2026-10-20, audit):** Read access other roles need: Retail reads the active product list (names only) to record the equipment choice on an order; Accounts reads product charges and damage records to bill them. Vendor payments are recorded by Admin only. Stock adjustments are Admin-only, require a reason, and are the way opening stock is entered.

## D-10 Combined landline + dial-up applications

- **Status / Tier / Needed by:** DECIDED · T1 · Day 5 · **Affects:** ORD-05, ORD-01, FEA-02, CON-10
- **Source evidence:** A dial-up connection needs a landline from the same vendor (Intro ¶2). If the customer applies for both, feasibility is checked for both; if they already hold the landline, internet only (Intro ¶3). IDs carry exactly one type letter (D, B or T); deposits differ per type ($325 dial-up, $250 landline).
- **Problem:** S1 never says whether "phone + internet" is one order or two.
- **Options:**
  - **A (recommended)** two linked orders (one T, one D), each with its own order ID, feasibility check, account ID and deposit; the D order cannot be provisioned until the T connection exists (or an existing landline account is referenced).
  - **B** one order with several lines. Needs an invented ID rule for the combination.
  - **C** treat dial-up as implying the landline inside one D connection. Conflicts with the T type code.
- **Also assumed:** broadband does not require a landline (S1 is silent).
- **Default:** A.
- **Decision (2026-10-20, by delegation):** Option A: two linked orders (one T, one D), each with its own IDs, feasibility and deposit; D cannot be provisioned until the landline connection exists or an existing landline account is referenced. Broadband needs no landline.
- **Amendment (v1.1, 2026-10-20, audit):** The landline reference on a dial-up order is either the account ID of an Active T connection in the system or, for a landline held before this system existed, a legacy landline identifier typed by Technical and flagged `legacy_unverified`. Deactivating a landline does not automatically change a dependent dial-up connection (MVP limitation).

## D-11 Data scope per role

- **Status / Tier / Needed by:** DECIDED · T1 · Day 3 · **Affects:** AUTH-02, AUTH-04, ORD-07, CON-06, FEA-05, SCH-05, RPT-01, CUS-06
- **Source evidence:** Retail employees track "till date" orders, connections, bills and payments (Non-Financial item 4). Retail shops exist in each city; payments may be made at "the office within the city" (item 5). S1 does not say whether those lists are per shop, per city or company-wide, or whether Accounts and Technical staff are per city.
- **Options:** Retail — own shop (default) / all shops in the city / all. Accounts, Technical — company-wide (default) / per city.
- **Consequence:** scoping touches every query and every database policy. Changing it after Day 5 means rewriting policies and tests; deciding now is cheap.
- **Default:** Retail = own shop; Admin, Accounts, Technical = all.
- **Decision (2026-10-20, by delegation):** Retail = own shop; Admin, Accounts and Technical = company-wide.
- **Amendment (v1.1, 2026-10-20, audit):** "Own shop" means customers who have an order at, or were created by, the user's shop; orders, connections, bills and payments are always shop-scoped. The only exception is the exact-match desk lookup in D-22.

## D-12 Customer login identity and public lookup

- **Status / Tier / Needed by:** DECIDED · T2 · Day 10 · **Affects:** AUTH-05, AUTH-07, PRT-05, SCH-05
- **Source evidence:** Customers can check details, orders and bills "from his end" (Proposed ¶6); status lookups by order ID and account ID (Proposed ¶2). S1 says nothing about how customers get credentials and has no customer email field.
- **Default:** customers log in with email + password; after an order exists, staff create the portal invitation; email is an optional customer field. No public (unauthenticated) lookup in the MVP. If wanted later: status keyword only, rate-limited, no names or amounts.
- **Alternatives:** account ID + PIN; phone OTP (needs an SMS provider, which is out of scope).
- **Decision (2026-10-20, by delegation):** Default accepted: email + password, staff-invited; no public lookup in the MVP.
- **Amendment (v1.1, 2026-10-20, audit):** Portal invitations are sent by Admin, or by Retail for customers of their own shop (D-25). One login maps to exactly one customer record, and a customer login can never reach staff routes.

## D-13 Feasibility is a manual technical decision

- **Status / Tier / Needed by:** DECIDED · T2 · Day 6 · **Affects:** FEA-02, FEA-03
- **Source evidence:** Intro ¶3: distance, server availability, "company patterns". No formula or API.
- **Default:** Technical staff record structured manual fields (distance, server availability yes/no, notes) and a decision. No automation until real rules or an API are supplied.
- **Decision (2026-10-20, by delegation):** Default accepted: manual technical decision with structured fields.

## D-14 Billing mechanics

- **Status / Tier / Needed by:** DECIDED · T2 · Day 8 · **Affects:** BIL-04, BIL-09, CON-07, PAY-05, RPT-02
- **Source evidence:** Postpaid only, and "the status of the connection depends" on the bill (Proposed ¶5). No cycle, usage capture or suspension rule.
- **Defaults:** (a) Accounts creates bills per connection per period; (b) call/usage minutes entered manually; (c) no automatic status change on non-payment: an overdue report is provided and Technical changes status on instruction; (d) no automatic billing change while Temporarily inactive: Accounts adds a manual adjustment; (e) overpayments are held as credit and applied to the next bill.
- **Decision (2026-10-20, by delegation):** Defaults (a)–(e) accepted and refined. Initial bill at activation = first-period rental (the advance) + security deposit (less any bulk discount) + tax per D-07. Later bills = rental for the next validity period in advance + usage for the previous period in arrears (manually entered). Due date = issue date + 15 days (D-20). Overdue is derived and reported; no automatic suspension.
- **Amendment (v1.1, 2026-10-20, audit):** (f) The overdue-bills report is read by Accounts, and the overdue-connections report by Accounts and Technical, so Technical can act on instruction.

## D-15 Equipment charges

- **Status / Tier / Needed by:** DECIDED · T2 · Day 8 · **Affects:** BIL-15, EQP-07, EQP-10
- **Source evidence:** Charges depend on the equipment chosen (modem or router) (Existing ¶2, Proposed ¶5), but no equipment price, rental or deposit is given. Replacement charges for customer damage are mentioned without an amount.
- **Default:** optional customer charge per product (blank = no charge); damage/replacement charges entered manually as bill lines by Accounts.
- **Decision (2026-10-20, by delegation):** Default accepted: optional customer charge per product (blank = free); damage charges entered manually.
- **Amendment (v1.1, 2026-10-20, audit):** Each plan can list the equipment types eligible to be supplied with it (EXS-4: the product list depends on the plan chosen); an order's equipment choice must be one of them. A plan with no list allows any active product.

## D-16 Plan changes

- **Status / Tier / Needed by:** DECIDED · T2 · Day 5 · **Affects:** ORD-09, CON-08
- **Source evidence:** Existing ¶5 notes that some customers try to change their plan. No workflow is described.
- **Default:** plan can be changed on an order before provisioning (Should). Plan changes on a live connection are post-MVP; Accounts adjusts manually and records a note.
- **Decision (2026-10-20, by delegation):** Default accepted: plan change only on an order before provisioning; live-connection changes post-MVP.

## D-17 Deposit refunds

- **Status / Tier / Needed by:** DECIDED · T2 · Day 8 · **Affects:** CON-09, BIL-08
- **Source evidence:** The security deposit is refunded on withdrawal or cancellation (Financial). No refund process, deductions or timing.
- **Default:** deposit balance tracked per connection; when a connection becomes Permanently inactive the system flags it for refund review; Accounts records the refund (amount, date, reason) manually, including any netting against dues.
- **Decision (2026-10-20, by delegation):** Default accepted: deposit tracked per connection; Accounts records refunds manually.
- **Amendment (v1.1, 2026-10-20, audit):** The refundable deposit balance is the deposit actually charged (after any bulk discount) minus refunds already recorded; a deposit discounted 100% therefore has nothing to refund.

## D-18 Working mode and release target

- **Status / Tier / Needed by:** DECIDED · T1 · Day 2 · **Affects:** all nodes, N14–N15
- **Why it matters:** in this chat I can create downloadable files, but I cannot reach your GitHub, Supabase or Vercel accounts or run commands against your repository.
- **Questions:**
  - (a) Will I work in a coding workspace connected to your repository (preferred, so I can run lint, tests and builds and report real output), or will I produce files and commands that you run and paste back?
  - (b) Should Day 15 be a live release for real customers, or a staging/demo-grade release? A live release adds real-data protection, backup and sign-off requirements.
  - (c) Who approves business rules? If you are the decision-maker, your answers are final. If someone else owns the business, the financial questions (D-03, D-04, D-05, D-06, D-07) should go to them.
- **Default:** (a) decided on Day 2 from what is available; (b) staging-grade MVP with a go/no-go gate on Day 15; (c) you.
- **Decision (2026-10-20, by delegation):** (a) Files and commands in this chat; you run commands and paste the full output back. If a repository-connected coding workspace is available on Day 2, I use it instead. (b) Day 15 = staging-grade MVP with a go/no-go release gate; live use only after the pre-go-live confirmations. (c) The owner approves business rules.
- **Amendment (v1.1, 2026-10-20, audit):** Refinement: the working sandbox can run Node, npm and git, so Node 2 is built and verified there with real command output and delivered as a downloadable archive; the owner creates the GitHub repository and pushes it (account action). If a repository-connected workspace becomes available it is used instead.

## D-19 Enquiry log

- **Status / Tier / Needed by:** DECIDED · T2 · Day 5 · **Affects:** ORD-11
- **Source evidence:** Customers "enquire" at retail shops (Existing ¶1); the system should "respond to the customer queries" (Proposed ¶8). No enquiry record or workflow is defined.
- **Default:** out of the MVP. Customer feedback (FR-8) is in scope.
- **Decision (2026-10-20, by delegation):** Default accepted: out of the MVP.

## D-20 Bill and receipt numbers, payment due days, time zone

- **Status / Tier / Needed by:** DECIDED · T2 · Day 4 · **Affects:** BIL-04, PAY-06, SCH-03
- **Source evidence:** Bills are generated (Financial ¶1) and receipts are filed (Existing ¶2), but no numbering, due date or time zone is stated.
- **Decision (2026-10-20, by delegation):** bill number `BL-YYYY-NNNNNN` and receipt number `RC-YYYY-NNNNNN` (6-digit counter per year, kept in `id_counters`); payment due date = issue date + 15 days (Admin setting `payment_due_days`); timestamps stored in UTC and shown in a configurable display time zone (default: the owner's local zone, confirmed on Day 2).
- **Rationale:** standard billing practice; every value is a setting or counter, so changing it needs no code change.

## D-21 Customer fields, installation address, feedback format

- **Status / Tier / Needed by:** DECIDED · T2 · Day 5 · **Affects:** CUS-01, ORD-06, CON-02, PRT-04
- **Source evidence:** Customer "personal details" and "contact details" are required (Existing ¶2) with address proofs filed; S1 lists no fields. Account IDs carry a city code, but a customer can have connections at different addresses. Feedback is to be collected (FR-8) with no format.
- **Decision (2026-10-20, by delegation):**
  - Customer fields: type (individual/corporate); full name; organisation name (corporate); billing address, city and optional postal code; primary contact number (required); alternate number; email (optional, required for portal login); address-proof type. The proof file goes to private storage; no ID numbers are stored in the database.
  - The installation address and city are captured on the order. The account ID city code uses the installation city.
  - Feedback: rating 1–5 plus an optional comment, linked to one order or connection; one entry per item per customer.
- **Rationale:** minimal personal data, and it matches how comparable subscription systems separate billing and service addresses.

## D-22 Cross-shop desk lookup

- **Status / Tier / Needed by:** DECIDED · T2 · Day 5 · **Affects:** CUS-06, PAY-07, SCH-05, AUTH-04
- **Source evidence:** Customers can pay, enquire and order at retail outlets "at each place" (Existing ¶1; Non-Financial item 5); retail lists are "till date" (item 4). S1 does not say a customer may only use the shop that sold the connection.
- **Problem:** strict own-shop isolation (D-11) would let the same person be registered twice at different shops and would stop a customer paying at a shop other than the one that sold the connection.
- **Options:** **A (chosen)** exact-match "desk lookups" with a minimal view across shops; everything else stays own-shop. **B** no cross-shop access: duplicate customers and refused payments. **C** city-wide visibility: exposes full customer data to every shop in the city.
- **Decision (2026-10-20, by delegation):** Option A. Retail can look up (1) a customer by exact primary contact number and (2) a connection by exact account ID, to avoid duplicate customers and to accept payments. A customer result shows only full name, city and masked contact number; a connection result shows only account ID, customer name and amount due. Partial or wildcard queries return nothing. Opening a full record still needs own-shop scope. A payment taken for another shop's connection stores the receiving shop. Every lookup writes an audit row.
- **Rationale and risk:** common counter practice; exposure is limited to minimal fields and logged. Revisit if the business wants stricter isolation.

## D-23 Stock model, equipment issue timing, plan-to-equipment options

- **Status / Tier / Needed by:** DECIDED · T2 · Day 8 · **Affects:** EQP-02, EQP-04, EQP-05, EQP-10, ORD-04
- **Source evidence:** INT-4 (track stock; order equipment from the orders received), EXS-4 (the product list provided at connection depends on the plan chosen), EXS-2 (the customer chooses modem or router), Proposed ¶1 (Admin enters stock details). S1 mentions no warehouses. The guide's order status "Awaiting equipment" conflicts with equipment being issued to a connection that does not exist yet.
- **Decision (2026-10-20, by delegation):** (a) one central stock pool in the MVP (no per-shop or per-city stock); a location column can be added to `stock_movements` later. (b) Opening stock is entered by Admin as a stock adjustment with the reason "opening balance". (c) "Awaiting equipment" is an advisory status set by Technical when stock is insufficient; it feeds the procurement-planning view (EQP-05) and does not block provisioning. (d) Equipment is issued to a connection after the connection exists, as a separate transaction (node N8). (e) Each plan may list eligible equipment (EQP-10).
- **Rationale:** keeps stock movements auditable and avoids coupling provisioning to inventory in the same transaction.

## D-24 Order-status transitions: who may perform each

- **Status / Tier / Needed by:** DECIDED · T2 · Day 5 · **Affects:** ORD-04, CON-01, FEA-04
- **Source evidence:** Non-Financial item 7: Technical updates order status (feasible or not, connection provided or not) and creates connections. Non-Financial item 3: retail employees place orders. No other transition actor is stated.
- **Decision (2026-10-20, by delegation):** the actor table in `architecture.md` §7 applies. In short: Technical reviews, assesses feasibility, marks Awaiting equipment and provisions; Admin rejects; Admin, or Retail for its own shop, cancels; Customers perform no order transitions.
- **Rationale:** follows the stated Technical responsibilities and keeps retail staff to placing and withdrawing orders.

## D-25 Authorisation strategy, password policy, portal invitations, document limits

- **Status / Tier / Needed by:** DECIDED · T2 · Day 3 · **Affects:** AUTH-02, AUTH-07, AUTH-08, DOC-01, DOC-03
- **Source evidence:** S1 requires separate logins and role-based functions (Proposed ¶1–2, Non-Financial item 2) but no mechanism. The architecture left "row-level security as well as server checks" open for Day 2.
- **Decision (2026-10-20, by delegation):**
  - Row-level security (RLS) is enabled with deny-by-default on every table; no policy grants access to the browser keys. All data access runs in server code using the server-only service key, where the permission matrix (kept as data in `lib/domain/permissions`) and scope checks are enforced and tested. Revisit only if direct browser access to the database is ever needed.
  - Passwords are provider-managed with a minimum length of 10; temporary credentials must be changed at first sign-in; multi-factor authentication for Admin and Accounts is recommended after the MVP.
  - Customer portal invitations are sent by email by Admin, or by Retail for customers of their own shop; one login per customer.
  - Document links expire after at most 300 seconds; uploads are limited to PDF, JPEG and PNG up to 5 MB (both configurable).
- **Rationale:** one enforcement point that can be tested exhaustively, with the database as a backstop against accidental exposure.

## D-26 Hosting plan constraints (verified 2026-10-20)

- **Status / Tier / Needed by:** DECIDED · T3 · Day 2 / go-live · **Affects:** NFR-05
- **Sources:** Vercel fair-use guidelines (https://vercel.com/docs/limits/fair-use-guidelines); Supabase pricing (https://supabase.com/pricing.md).
- **Findings:** the Vercel Hobby plan is restricted to non-commercial personal use, and commercial use (defined to include a deployment used for anyone's financial gain, even a paid consultant writing the code) requires Pro or Enterprise. Supabase Free allows 2 active projects, pauses projects after one week of inactivity and does not include automatic backups; paid plans do not pause and Pro keeps daily backups for 7 days.
- **Decision (2026-10-20, by delegation):** build provider-neutral (a standard Next.js build that runs on any Node host, with Postgres on Supabase). Free tiers are for local development and non-commercial demo staging only. Staging and production are separate Supabase projects. Any live business use needs paid plans on both services (or an equivalent host). Until then a manual database export, documented in the Day 15 handover, is the backup mechanism. Terms are re-verified at Node 14. Whether the deployment counts as commercial is for the owner to determine; I do not assume either way.
- **Rationale:** avoids building on a plan the terms may not allow, without blocking development.

## D-27 Framework configuration and tooling (Node 2)

- **Status / Tier / Needed by:** DECIDED · T2 · Day 2 · **Affects:** NFR-03
- **Source evidence:** the guide's stack (Next.js, TypeScript, Tailwind CSS, shadcn/ui, Vitest, Playwright, GitHub, Vercel, Supabase). The generator's defaults on 2026-10-20 enable Cache Components and partial prefetching, load fonts from Google at build time, and ignore every `.env*` file including the template.
- **Decision (2026-10-20, by delegation):**
  - Installed and pinned by the lockfile: Node 22, Next.js 16.4.2, React 19.3.0, TypeScript 5.9.3 (the generator's 5.x), ESLint 9.39.5, Tailwind CSS 4.3.3, Vitest 5.0.5.
  - Cache Components and partial prefetching are switched off. Every NEXUS page shows per-user data, and the framework's own guidance for session pages is to stream them behind Suspense and manage cache directives explicitly; rendering stays dynamic so that user and financial data is never cached implicitly. Revisit only with a measured performance need.
  - Google Fonts are replaced by the system font stack: no network fetch at build time and no third-party request at runtime.
  - shadcn/ui stays in the stack but is initialised in Node 4, when the first forms are built, using its CLI (its registry is not reachable from the build sandbox). Playwright is added when pages exist (Node 12–13).
  - Baseline security headers (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy`) are set and `X-Powered-By` is removed; a Content-Security-Policy is added in Node 13 when it can be tested against real pages.
  - `.env.example` is committed (names only); every other `.env*` file is ignored.
  - GitHub Actions workflow runs docs check, lint, type-check, tests, the production-dependency audit and the build. Action majors are pinned to v6 (v7 exists at the time of writing; bump after the first green run). The workflow is unverified until the first push (KI-N2-02).
  - The full `npm audit` reports 5 high findings that are one dev-only chain ending in `braces`, for which no patched release exists (KI-N2-01). `npm audit fix --force` is not used because it downgrades the Next.js lint tooling to version 14. CI gates on `npm audit --omit=dev --audit-level=high`, which reports 0 findings.
  - The initial commit uses a neutral author identity; the owner's identity is used from their first commit.
- **Rationale:** the safest and most maintainable baseline for a data-sensitive application, with each deviation from the generator's defaults recorded.

---

## Pre-go-live confirmations

None of these block Nodes 2–14. Before a live release (Day 15 gate) the owner must confirm them, otherwise the release stays labelled demo/staging.

1. Currency, and that every price in requirements-matrix Appendix A3 is current.
2. The two assumed values: Yearly STD $720.00 and Half-yearly STD 6 months.
3. That 12.24% is the current tax rate, and that the refundable deposit is not taxed.
4. That the bulk-discount table, its boundaries and the meaning of "advance" match business policy.
5. The messaging rate unit and the broadband 128 Kbps quarterly price ($445).
6. Payment terms (15 days) and the bill/receipt number formats.
7. That the role matrix and shop scope match real staffing.
8. Hosting plans: that the deployment's use (commercial or not) is allowed on the chosen Vercel and Supabase plans, and that paid plans are in place for live use (D-26).

## Checked and found NOT ambiguous (no decision needed)

- Three connection types and their prefix letters: D dial-up, T telephone only, B broadband.
- Order ID length (11 characters) is consistent with the example.
- Connection statuses: active, permanently inactive, temporarily inactive.
- Only postpaid connections are offered.
- Roles: Admin, Accounts, Technical, retail employees, and customers (separate logins).
- Only Accounts updates bill details; payments are recorded by retail employees and by Accounts for office payments; Admin maintains plans and employees.
- Tax rate stated as 12.24%; deposit amounts stated as $325 / $500 / $250. Whether these are current is covered by D-04 and D-07.

## Change history

| Date | Change | By |
|---|---|---|
| 2026-10-20 | v0.1: initial log created at Node 1; items OPEN or ASSUMED | Claude |
| 2026-10-20 | v1.0: owner accepted all defaults and delegated remaining gaps; all items set to DECIDED; added D-20 and D-21; refined D-03 (bulk application grouping), D-05, D-06, D-14, D-18 | Claude, by owner delegation |
| 2026-10-20 | v1.1: Node 1 consistency audit. Added D-22 to D-26; amended D-03, D-07, D-09, D-10, D-11, D-12, D-14, D-15, D-17, D-18; standardised filenames; removed stale wording; aligned every document (see `node1-audit-report.md`) | Claude, under owner authority |
| 2026-10-20 | v1.2: Node 2 baseline built and verified; added D-27 (framework configuration and tooling) | Claude, under owner authority |
