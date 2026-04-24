---
name: Token-efficient workflow
description: How the user prefers to handle terminal commands and git operations to reduce token usage
type: feedback
---

User runs all terminal commands themselves — git operations, dotnet build, dotnet test. Claude drafts the commands/messages and the user executes them.

**Why:** Reduces token usage — build/test output fed back into context is expensive.

**How to apply:**
- Draft commit messages and tell the user exactly what to run; don't execute git add/commit/push/checkout
- Draft build/test commands but don't run them unless diagnosing a specific failure
- When the user reports "all green" or pastes an error, take that as the input — don't re-run to verify
- For file writes/edits, still use tools directly (pure output, cheap)
