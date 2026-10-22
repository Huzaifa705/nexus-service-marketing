# NEXUS Service Marketing System — Node 1 Audit Report

| | |
|---|---|
| Date | 2026-10-20 |
| Scope | The five Node 1 documents in the Project context, checked against each other and against the original specification (S1) |
| Result | **READY for Node 2** (all objective criteria below pass; go-live confirmations remain and do not block Nodes 2–14) |
| Method | (1) A dependency-free checker (a Node script at the time; now the C# tool `tools/Nexus.DocsCheck`, which runs the same checks), run before and after the fixes. (2) A manual read of the eight areas you named. (3) Re-computation of every money example with exact decimal arithmetic. |

## 1. Files

The Project holds `architecture.md`, `decision-log.md`, `known-issues.md`, `requirements-matrix.md` and `test-plan.md`. They were byte-identical to the v1.0 set. The names you listed (`decision.log.md`, `known issues.md`, `requirements matrix.md`, `testplan.md`) do not exist. The standard names above match the project guide and are now enforced by the checker. The specification's real filename is `NEXUS_SERVICE_MARKETING_SYSTEM.doc`; the matrix cited it with spaces and now uses the real name.

## 2. Before and after

| | v1.0 as found | v1.1 |
|---|---|---|
| Checker errors | 38 | 0 |
| Checker warnings | 29 | 0 |
| Must requirements with a test | 57 of 84 | 88 of 88 |
| Requirements (Must / Should / Could) | 109 (84 / 18 / 7) | 114 (88 / 19 / 7) |
| Test cases | 74 | 118 |
| Decisions | 21 | 26 |

## 3. Corrections

**Filenames and references**
- Specification filename corrected (above).
- 5 requirements cited a decision that did not list them in its "Affects" line, and 9 "Affects" entries named requirements that did not cite the decision; both directions now agree.
- 2 requirements marked ambiguous (`SRC⚠`) cited no decision; each now cites one.
- One invalid source reference (`FIN (uses $)`) replaced with real section codes.
- Stale wording removed ("please confirm", "ask now", conditional "if D-11 = …" in tests).

**Contradictions found by reading**
1. **Tax and "advance".** The billing steps taxed "items 1–4 plus the advance", which double-counts the first month's rental because the advance is that rental. The advance is now a flag (`is_advance`) on the first rental line, each bill line carries a `taxable` flag, and the worked example reproduces TV-T2.
2. **Bulk-discount counting.** "Non-cancelled orders" would have counted rejected and not-feasible orders. The count now covers only eligible orders (not Cancelled, Rejected or Not feasible), the tier is fixed when each initial bill is created, and later changes never alter issued bills. A Must requirement no longer depends on a Should one.
3. **Role matrix versus requirements.** Retail could not read the product list, yet orders record a modem/router choice. Accounts could not read product charges or damage records, yet bills use them. Vendor payments and customer-portal invitations had no owner. All added to the matrix.
4. **Shop isolation versus the spec.** The spec lets customers pay at retail outlets, but strict own-shop access would block payments at another shop and create duplicate customers. Resolved with a minimal-view, exact-match desk lookup (D-22).
5. **Order status transitions** had no stated actor. Added an actor table (D-24).
6. **Equipment issue versus "Awaiting equipment".** Equipment was issued to a connection that does not exist yet. "Awaiting equipment" is now advisory, and issue follows connection creation (D-23).
7. **Plan-dependent equipment.** The spec says the product list depends on the plan (Existing ¶4). Added eligible equipment per plan (EQP-10).
8. **Orders versus account IDs.** The spec's "connection count" cannot map to one account ID per connection. Orders are grouped into bulk applications (D-03), and each order stores its installation address and city, which supplies the account ID's city code.
9. **Deposit refunds** now use the deposit actually charged, after any bulk discount.
10. **Existing landline** verification defined: a T account in the system, or a legacy identifier flagged `legacy_unverified`.
11. **Authentication.** The customer-invitation capability, password policy and the open "row-level security as well" question were undecided; now D-25.
12. **Hosting.** The stack assumed free tiers would do. Verified on 2026-10-20: Vercel Hobby is non-commercial only, and Supabase Free pauses after a week of inactivity and has no automatic backups (D-26; superseded by D-29 after the stack change).

**Acceptance criteria and tests**
- 27 Must requirements had no test; each now has one. Should requirements have a conditional test block.
- Vague criteria rewritten with numbers (360 px width, 300-second links, 5 MB uploads, 409 on in-use deletes, identical not-found responses).
- Test wording aligned with the matrix (Admin can read the feasibility queue; the end-to-end journey starts with Retail, not the customer).

## 4. Decisions made under your authority

New: D-22 desk lookup, D-23 stock model and equipment timing, D-24 transition actors, D-25 authorisation and passwords, D-26 hosting constraints. Amended: D-03, D-07, D-09, D-10, D-11, D-12, D-14, D-15, D-17, D-18. Rationale for each is in `decision-log.md`.

## 5. Readiness criteria

| # | Criterion | Evidence | Result |
|---|---|---|---|
| 1 | Five documents present with standard names, no variants | Checker C1 | Pass |
| 2 | Every decision reference resolves; all 26 decisions DECIDED with a recorded decision | C2, C3 | Pass |
| 3 | Every requirement, dependency and source reference valid; no duplicate IDs | C4, C8 | Pass |
| 4 | 88 of 88 Must requirements have an acceptance criterion and at least one test | C5, C6, C10 | Pass |
| 5 | All 26 ambiguous requirements cite their decision | C7 | Pass |
| 6 | Decisions and requirements cross-reference each other | C9 | Pass |
| 7 | No stale wording; matching versions and dates | C11, C15 | Pass |
| 8 | Section, known-issue and source-coverage links valid (all 36 source items covered) | C12, C13, C14 | Pass |
| 9 | Every money example recomputed exactly | Decimal arithmetic run this session | Pass |
| 10 | The eight named contradiction areas reviewed and resolved | Section 3 | Pass |
| 11 | No application code written during the audit | None created | Pass |

**Limits of this audit.** The checker proves structure and traceability, not business truth. Acceptance-criteria quality was screened automatically for vague wording and reviewed manually for the rows I changed. The Mermaid diagram was not rendered. No test has been run because no application exists.

## 6. Remaining risks

- **Commercial values are unverified:** prices, the 12.24% tax rate, currency, the two assumed STD values, bulk tiers and "advance" (the 8 pre-go-live confirmations in `decision-log.md`). Day 15 stays a demo/staging release until you confirm them.
- **Schedule (KI-R3):** no spare days; Days 12–13 are the only buffer.
- **Provider terms (KI-R7):** closed after the stack change; the equivalent risk is now KI-R8 (SQL Server edition and licensing).
- **Real data (KI-R6):** synthetic data only until staging is approved.

## 7. Actions only you can do

1. Create a private GitHub repository and push the Node 2 archive (steps in the Node 2 handoff).
2. (Superseded by the stack change: install the .NET 10 SDK and SQL Server instead; see the addendum.)
3. Upload the v1.1 documents to the Project files, replacing v1.0.
4. Before any live use: decide whether the deployment is commercial and confirm the eight go-live items.

## Addendum (2026-10-22): stack change

The project's required software (.NET and SQL Server) was reported after this audit. Result: the technology stack changed (D-28), D-26 and D-27 are superseded by D-29 and D-30, D-25 and D-12 are amended, and D-31 was added. Every Node 1 finding above, the requirements and decisions D-01 to D-24 stand. The checker was ported to C# and produces identical results to the Node original on the defective v1.0 documents (38 errors, 29 warnings, 67 identical lines). The owner's actions are now: push the re-platformed repository, install the .NET 10 SDK and SQL Server (Express or Developer) with SSMS, and run `.\scripts\verify.ps1`.
