# NEXUS Service Marketing System — Known Issues (Node 2 baseline)

| | |
|---|---|
| Version | 1.3 — Node 2 baseline (.NET) |
| Date | 2026-10-22 |
| Scope | Defects and gaps in the source specification (all resolved by decisions in `decision-log.md`), project risks, and MVP limitations. No application defects exist yet because no application code has been written. |

Severity: **H** can cause wrong money, wrong identifiers, or blocked delivery · **M** changes a workflow or rule · **L** minor or wording.

## A. Defects and gaps in the source specification (S1)

| ID | Issue | Sev. | Impact | Status | Link |
|---|---|---|---|---|---|
| KI-01 | Account ID described three ways: 10-digit (Existing ¶2), "16 digit … numeric" (Proposed ¶2), letter + 3 + 12 (Note) | H | Identifier format and counters | Resolved — D-01 | CON-02 |
| KI-02 | Order and account IDs are called "digit" codes but start with a letter | L | Wording; resolved for order ID (11 characters) | Resolved — D-02 | ORD-02 |
| KI-03 | Bulk-discount ranges overlap at 15, 25, 50; 1–9 not stated | H | Wrong discount at boundaries | Resolved — D-03 | BIL-06 |
| KI-04 | "Advance" is never defined; counting basis for connections not stated | H | Discount base unknown | Resolved — D-03 | BIL-06 |
| KI-05 | Yearly STD rental price is blank | H | Plan cannot be priced | Resolved — D-05 | PLN-07 |
| KI-06 | Half-yearly STD plan "valid for a month" | M | Wrong validity if copied literally | Resolved — D-06 | PLN-04 |
| KI-07 | Tax "on the whole bill": deposit, discount order, rounding unspecified | H | Totals differ by tens of dollars per bill | Resolved — D-07 | BIL-07 |
| KI-08 | Orders: retail only (Non-Financial 3) vs customer can place (Proposed ¶2, ¶6) | M | Role and workflow | Resolved — D-08 | ORD-01 |
| KI-09 | Admin "solely" maintains equipment (Existing ¶5) vs Technical maintains equipment (Non-Financial 7) | M | Permissions overlap | Resolved — D-09 | EQP-09 |
| KI-10 | Broadband 128 Kbps quarterly price ($445) looks inconsistent with the other quarterly plans | M | Possible typo | Resolved — D-04 | PLN-02 |
| KI-11 | Mobile messaging priced "per minute"; rate rises with longer plans while other rates fall | M | Unit and values unclear | Resolved — D-04 | PLN-04 |
| KI-12 | Landline Local plan named "Unlimited" but charged per minute | L | Naming/confusion | Resolved — D-04 | PLN-04 |
| KI-13 | Equipment affects charges, but no equipment price, rental or deposit is given | M | Bill cannot include equipment charges without a rule | Resolved — D-15 | BIL-15 |
| KI-14 | Plan changes mentioned (Existing ¶5), no workflow | M | Scope | Resolved — D-16 | ORD-09, CON-08 |
| KI-15 | Customer enquiries mentioned, no record or workflow | L | Scope | Resolved — D-19 | ORD-11 |
| KI-16 | How customers obtain credentials is not stated; no customer email field | M | Portal sign-in design | Resolved — D-12 | AUTH-05 |
| KI-17 | Phone + internet applications: one order or two is not stated; only one type letter per ID | H | Order and account structure | Resolved — D-10 | ORD-05, CON-10 |
| KI-18 | Whether Retail lists are per shop, per city or company-wide is not stated | H | Data scoping across all queries | Resolved — D-11 | AUTH-04 |
| KI-19 | Broadband landline dependency not stated (only dial-up requires a landline) | L | Feasibility and order rules | Resolved — D-10 (not required) | FEA-02 |
| KI-20 | Hourly plans: no overage, rollover or usage-capture rule | M | Billing for hourly plans | Resolved — D-04, D-14 | BIL-13 |
| KI-21 | Billing while Temporarily inactive and the link between unpaid bills and connection status not defined | M | Billing and status automation | Resolved — D-14 | CON-07 |
| KI-22 | Deposit refund process (timing, deductions) not defined | M | Refund handling | Resolved — D-17 | CON-09 |
| KI-23 | Receipt numbering/format not defined | L | Receipt design | Resolved — D-20 | PAY-06 |
| KI-24 | Feedback format not defined | L | Feedback form | Resolved — D-21 | PRT-04 |
| KI-25 | Terminology: "employers" = employees; "retail show room" = retail shop; "purchasal lists" = purchase lists | L | Wording only | Resolved | — |
| KI-26 | S1 describes legacy-style services (dial-up, Kbps-class plans) and a named "service tax"; currency is not stated | M | Whether prices and tax are live values | Resolved — D-04, D-07, D-18 (values stay flagged unverified until the go-live confirmations) | PLN-02 |
| KI-27 | S1 gives a "connection count" for discounts, but each account ID belongs to one connection and the counting basis is not defined | M | Order/connection structure for bulk customers | Resolved — D-03 (bulk applications) | ORD-12 |
| KI-28 | Customer address vs installation address not distinguished, yet account IDs carry a city code | M | Wrong city code in IDs | Resolved — D-21 | CON-02 |
| KI-29 | The product list at connection depends on the plan chosen (Existing ¶4), but the customer chooses modem or router (Existing ¶2) | M | Equipment selection rule | Resolved — D-15 (eligible equipment per plan) | EQP-10 |
| KI-30 | Customers may pay at any outlet, but retail data is shop-scoped | M | Payments and duplicate customers across shops | Resolved — D-22 | CUS-06, PAY-07 |
| KI-31 | The "Awaiting equipment" status conflicts with equipment being issued to a connection that does not exist yet | M | Order flow | Resolved — D-23 | ORD-04, EQP-04 |
| KI-32 | How "an existing landline with the same vendor" is verified is not stated | M | Dial-up provisioning rule | Resolved — D-10 | CON-10 |

## B. Project risks

| ID | Risk | Sev. | Mitigation | Status |
|---|---|---|---|---|
| KI-R1 | In this chat Claude can create downloadable files and build projects that need no NuGet packages, but cannot reach GitHub or NuGet and cannot run commands on the owner's machine or on SQL Server | H | Working mode decided (D-18): files and commands in chat, repository workspace if available on Day 2; never claim a command, test or deployment succeeded without its output | Mitigated |
| KI-R2 | The owner's GitHub account and local installs (.NET 10 SDK, SQL Server Express or Developer, SSMS) must be in place by the end of Day 2 or the Day 15 release is at risk | H | Owner action list in the Day 2 plan | Open (Node 2 action) |
| KI-R3 | The schedule has no spare days; Days 12–13 are the only effective buffer | H | Fixed scope; cut order: all Should items first, then dashboards/polish; never cut authorisation, IDs, money, or core tests | Open (monitor daily) |
| KI-R4 | Financial rules (D-03, D-07) are decided by delegation but not verified against real business policy | H | Decided (D-03, D-07); engines configurable; unverified values flagged and listed in the pre-go-live confirmations | Mitigated |
| KI-R5 | Role/scope decisions (D-09, D-11) are wrong for the real staffing | M | Decided (D-09, D-11) before schema work | Mitigated |
| KI-R6 | Real customer data or documents used before staging is approved | H | Synthetic seed only; real data only after owner approval (Day 14–15) | Open |
| KI-R7 | Provider terms: Vercel Hobby is non-commercial only; Supabase Free pauses after a week and has no automatic backups | H | Superseded: the hosting stack was replaced (D-28, D-29) | Closed — superseded by D-29 |
| KI-R8 | SQL Server Developer edition is licensed for development and testing only; Express has database size and resource limits | H | D-29: Developer is never used for production data; Express or a licensed edition is chosen at Nodes 14–15 | Open (go-live gate) |

## C. MVP limitations (by design)

- No online payment gateway, SMS or email notifications, live telecom feasibility, ERP integration, or native mobile app.
- No legacy data import or legacy account ID mapping tool.
- No automatic suspension for non-payment and no automatic metering of hourly plans.
- One configurable currency, English only.
- Customer self-service ordering and public status lookup are off by default (D-08, D-12).
- Live-connection plan changes and enquiry log are post-MVP (D-16, D-19).
- Deactivating a landline does not automatically change a dependent dial-up connection (D-10).
- No merge tool for duplicate customers; duplicates are prevented by the exact contact-number lookup (D-22).
- One central stock pool; no per-shop or per-city stock (D-23).
- Customer portal logins are created by staff; there is no emailed invitation or self-service password reset (D-31).

## D. Template for later nodes

| ID | Found in | Description | Sev. | Steps to reproduce | Status | Fix / commit |
|---|---|---|---|---|---|---|
| KI-N2-01 | Node 2 | (Next.js baseline) Full `npm audit` reports 5 high findings, all one dev-only chain (eslint-config-next → @next/eslint-plugin-next → fast-glob → micromatch → braces, GHSA-vfj7-8cjw-p6xm: denial of service through deeply nested glob patterns). braces 3.0.3 is the latest release and is inside the affected range, so no patched version exists. Production dependencies: 0 findings. | S4 | `npm audit` | Closed — superseded by the stack change (D-28) | D-28 |
| KI-N2-02 | Node 2 | (Next.js baseline) The GitHub Actions workflow `.github/workflows/ci.yml` has never run (no repository yet) | S3 | Push to GitHub and open the Actions tab | Closed — the Next.js workflow ran green on the owner's repository, then was replaced (see KI-N2-06) | D-28 |
| KI-N2-03 | Node 2 | The Mermaid diagram in `architecture.md` has not been rendered | S4 | Open the file on GitHub | Open (check after the first push) | — |
| KI-N2-04 | Node 2 (.NET) | The xUnit test project, and any code that needs NuGet packages, could not be restored or run in the build sandbox (NuGet is unreachable). The test sources were compiled and run against a minimal xUnit stand-in only | S3 | Run `dotnet test` on Windows | Open (owner action) | — |
| KI-N2-05 | Node 2 (.NET) | `scripts/verify.ps1` and the PowerShell commands in the README were not run in the build sandbox (Linux) | S3 | Run `.\scripts\verify.ps1` | Open (owner action) | — |
| KI-N2-06 | Node 2 (.NET) | The new .NET GitHub Actions workflow has never run | S3 | Push to GitHub and open the Actions tab | Open (owner action) | — |
| KI-N2-07 | Node 2 (.NET) | With only HTTP configured the application logs "Failed to determine the https port for redirect"; harmless in development, but HTTPS must be configured or terminated in front of the application at deployment | S4 | Run the application on an http-only URL | Deferred to Node 15 | — |
| KI-N… | Node | … | S1–S4 | … | Open / Fixed / Deferred | … |
