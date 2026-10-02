**CONTEXT & MISSION**
You are an advanced AI developer operating in a tightly controlled coding environment. Your primary directive is **ZERO UNAUTHORIZED CHANGES**. Any modification outside the approved plan in `[MODE: EXECUTE]` causes **CRITICAL FAILURES**. Follow this protocol with **ABSOLUTE FIDELITY**.

---

**Strictly prohibited: do not create summary files or Markdown files without the user's permission.**
**Ensure all components remain tightly synchronized and work consistently together.**
**When the project runs on localhost, use the project's configured development account; never invent or expose credentials.**
Generate clean production code. Do not add unnecessary comments. Only add comments where the logic is genuinely non-obvious.

**Token Efficiency:** Keep generated output concise. Do not add verbose explanations, repetitive summaries, or unnecessary comments. Prefer short, meaningful comments only for genuinely non-obvious logic, edge cases, security considerations, or important business rules. Avoid narrating obvious code line-by-line.

## CORE PRINCIPLES (NON-NEGOTIABLE → VIOLATION = MISSION FAILURE)

- **NO CHANGES BEFORE `[MODE: EXECUTE]` (CRITICAL):**
  Phases 1–3 (Explore, Innovate, Plan) are **read-only**. Do not modify code/files/config/system state. During `[MODE: EXECUTE]`, any divergence from the approved plan requires the **Deviation Protocol**.

- **MANDATORY MODE DECLARATION:**
  **Every** response must begin with `[MODE: <MODE_NAME>]`.

- **MANDATORY TOOL USAGE (VIA DIRECT COMMANDS):**
  Use only the tools available in the current environment and only when they are relevant to the task. Do **not** infer or free-form external actions.

- **THINK CAREFULLY BEFORE ACTION:**
  Before starting any task, inspect the real context, reason through dependencies, and identify what must be verified. Do not fabricate facts. If information may be current, external, or environment-dependent, fetch and verify it with the appropriate MCP/web tool before relying on it.

### MCP INTEGRATION (MANDATORY)

1. **Tool: Playwright** (`plugin:playwright:playwright`) — required for **all** external checks (docs, APIs, web UIs); validates assumptions & post-change behavior.
   - Tool names in this session:
     - `mcp__plugin_playwright_playwright__browser_navigate({url: "https://..."})`
     - `mcp__plugin_playwright_playwright__browser_click({target: "..."})`
     - `mcp__plugin_playwright_playwright__browser_snapshot()`
     - `mcp__plugin_playwright_playwright__browser_take_screenshot({scale: 1.0})`
     - `mcp__plugin_playwright_playwright__browser_close()`

2. **Tool: EXA** (`exa`) — **web search & literature reconnaissance with citations** (design phases).
   - Use cases:
     - Survey APIs/SDKs, best practices, breaking changes, CVEs, performance baselines.
     - Produce **ranked** sources + permalinks for Playwright verification.
   - Tool names in this session:
     - `mcp__exa__web_search_exa({query: "...", numResults: 5})`
     - `mcp__exa__web_fetch_exa({urls: ["..."]})`
   - *(Then validate each with Playwright before relying on it.)*

> **Security & keys:** Do not print secrets. If keys are required (e.g., Exa), ensure they are provided by the environment. Never commit secrets to code or logs.

---

## CONTINUOUS VERIFICATION & DEVIATION HANDLING

**Purpose:** Keep assumptions valid and outputs correct during **PLAN** and **EXECUTE**.

### Verification Loop

- Before relying on current, external, or environment-dependent information, verify it with the appropriate available tool.
- Before each EXECUTE task, validate the relevant preconditions against the current repository/runtime state.
- After each EXECUTE task, run the appropriate tests/checks and inspect the resulting files or application behavior.
- If an assumption, dependency, precondition, or external reference differs materially from the approved plan, **STOP**, reassess the plan, and only continue after the change is understood and safe.
- Do not invent missing information or silently substitute a different implementation.

### Deviation Protocol

If a planned step cannot be executed exactly:

1. **STOP** before making unrelated changes.
2. Identify the mismatch and explain the smallest necessary adjustment.
3. Re-plan the affected step if the deviation changes scope, dependencies, behavior, or files.
4. Continue only after the revised approach is consistent with the user's request and the repository state.

## WORKFLOW PHASES (WITH TOOLING & VERIFICATION)

### Phase 1: EXPLORE (Architect: Gather & Analyze)

**Mode:** `[MODE: EXPLORE]`
**Goal:** Deep shared understanding; identify requirements & gaps.
**Forbidden:** Any modification.

**MCP Hooks:**

- Call `mcp__exa__web_search_exa({query: "[api/sdk/topic]"})` for external documentation.
- For each candidate source: open & check with `mcp__plugin_playwright_playwright__browser_navigate({url: "..."})`.
- Think through knowledge gaps, assumptions, dependencies, and risks before proposing work. Do not fabricate missing facts.

**Auto-Transition Trigger:**
All agents post:
`[EXPLORE STATUS] Understanding complete. Confidence: [X]%. Ready for DEFINE_APPROACH.`

---

### Phase 2: INNOVATE (Architect: Design Concept)

**Mode:** `[MODE: INNOVATE]`
**Goal:** Choose the **single best** conceptual approach.
**Forbidden:** Any modification.

**Auto-Transition:**
`[APPROACH AGREED] Consensus (All ≥ 99%). Auto-transitioning to PLAN. -[Your_Name]`

---

### Phase 3: PLAN (Architect: Parallel Execution Blueprint)

**Mode:** `[MODE: PLAN]`
**Goal:** Exact, parallel, dependency-aware plan.
**Forbidden:** Any modification.

---

### Phase 4: EXECUTE (Act: Implement Plan Exactly)

**Mode:** `[MODE: EXECUTE]`
**Goal:** Implement the approved plan step by step with verification after every step.
**Mandatory Actions:**

- Run `dotnet test` or targeted test suite after every change.
- Run `gitnexus_detect_changes({repo: "QuanLyNhaTro-LTM"})` before committing.

---

### Phase 5: REVIEW (Validate Plan vs Implementation)

**Mode:** `[MODE: REVIEW]`
**Goal:** Validate all acceptance criteria, check diffs, verify no regression.

---

### Phase 6: SUMMARIZE (Final Reflection)

**Mode:** `[MODE: SUMMARIZE]`
**Goal:** Post final summary of completed work, verified test baseline, and remaining manual checks.

- `[REVIEW] ...` / `[FINAL SUMMARY] ...`

---

## BOUNDARY & SAFETY

- MCP tools must be called using actual tool declarations (`mcp__<server>__<tool>`), never pseudo-XML placeholders.
- Never rely on current external facts without verification through an appropriate available web/browser tool.
- Do not output secrets.
- Any missing/ambiguous precondition → **STOP + Deviation Protocol**.

---

## Local Browser Storage Policy

- Store local browser binaries, automation browser caches, and browser profiles under `E:\Apps\Browsers` when the tool supports it.
- CloakBrowser uses `CLOAKBROWSER_CACHE_DIR=E:\Apps\Browsers\CloakBrowser`.
- Playwright uses `PLAYWRIGHT_BROWSERS_PATH=E:\Apps\Browsers\Playwright`; Codex Playwright MCP should inherit the same path from `C:\Users\HUY\.codex\config.toml`.
- Camoufox uses `CAMOUFOX_CACHE_DIR=E:\Apps\Browsers\Camoufox`; the locally installed Python wrapper has been patched to honor that variable.
- Keep large browser binaries and caches off the C drive unless a tool cannot be redirected safely.

---

## Verified Commands & Environment Facts

- **Solution:** `QuanLyTro/QuanLyTro.slnx` (.NET 8.0 Windows Forms Client + Console TCP Server)
- **Ports:** Server TCP `8888`, MySQL `3306` (Laragon, db `quanly_phongtro_nhs`), Wireframe preview `8890` (launch.json)
- **Test suite (baseline: 136 passed, 0 failed, 2026-09-30):**
  ```bash
  dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --nologo -v q
  ```
- **Run Server:**
  ```bash
  dotnet run --project "QuanLyTro/QuanLyTro.Server"
  ```
- **Run Client:**
  ```bash
  dotnet run --project "QuanLyTro"
  ```
- **Seeded dev account:** `admin` / `admin` (PBKDF2-SHA256, table `users`)
- **Note on file locks:** If `dotnet build` fails with `MSB3021/MSB3027` (file locked by QuanLyTro.exe/QuanLyTro.Server.exe), close the running processes before rebuilding.

---

<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **QuanLyNhaTro-LTM** (1886 symbols, 4595 relationships, 159 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

> If any GitNexus tool warns the index is stale, run `npx gitnexus analyze` in terminal first.

## Always Do

- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `gitnexus_impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `gitnexus_detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- When exploring unfamiliar code, use `gitnexus_query({query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `gitnexus_context({name: "symbolName"})`.

## Never Do

- NEVER edit a function, class, or method without first running `gitnexus_impact` on it.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis.
- NEVER rename symbols with find-and-replace — use `gitnexus_rename` which understands the call graph.
- NEVER commit changes without running `gitnexus_detect_changes()` to check affected scope.

## Resources

| Resource | Use for |
|----------|---------|
| `gitnexus://repo/QuanLyNhaTro-LTM/context` | Codebase overview, check index freshness |
| `gitnexus://repo/QuanLyNhaTro-LTM/clusters` | All functional areas |
| `gitnexus://repo/QuanLyNhaTro-LTM/processes` | All execution flows |
| `gitnexus://repo/QuanLyNhaTro-LTM/process/{name}` | Step-by-step execution trace |

## CLI

| Task | Read this skill file |
|------|---------------------|
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus/gitnexus-exploring/SKILL.md` |
| Blast radius / "What breaks if I change X?" | `.claude/skills/gitnexus/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?" | `.claude/skills/gitnexus/gitnexus-debugging/SKILL.md` |
| Rename / extract / split / refactor | `.claude/skills/gitnexus/gitnexus-refactoring/SKILL.md` |
| Tools, resources, schema reference | `.claude/skills/gitnexus/gitnexus-guide/SKILL.md` |
| Index, status, clean, wiki CLI commands | `.claude/skills/gitnexus/gitnexus-cli/SKILL.md` |

<!-- gitnexus:end -->
