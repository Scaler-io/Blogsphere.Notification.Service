# Create New Branch

This command creates a new git branch from `main` for feature development or bug fixes. The branch name is derived from your changes, and `main` is kept up to date with GitHub before branching.

## Instructions

Follow these steps **in order**. Use the Shell tool for all git commands.

---

### Step 1: Ask User for Branch Type

**Before proceeding**, ask the user:

> "Do you want to create a **feature** branch or a **bugfix** branch?"

- If **feature** → branch prefix will be `feature/`
- If **bugfix** → branch prefix will be `bugfix/`

Wait for the user's response before continuing.

---

### Step 2: Check Git Changes to Determine Branch Name

Run these commands to analyze the working tree and derive a branch name:

```
git status
git diff --stat
git diff --cached --stat
```

**Analyze the changes:**
- Look at modified/added/deleted file paths
- Look at the content of diffs (`git diff` and `git diff --cached`)
- Identify the main theme: e.g., "password-reset-email", "user-invitation", "error-queue-reprocessor", etc.

**Branch naming rules:**
- Use kebab-case (lowercase, hyphens)
- Keep it short and descriptive (e.g., `feature/password-reset-one-time-code`, `bugfix/email-template-encoding`)
- If no changes exist yet, ask the user for a descriptive branch name

**Propose the full branch name** to the user (e.g., `feature/password-reset-one-time-code`) and confirm before creating it.

---

### Step 3: Ensure Main is Up to Date with GitHub

Run these commands:

```
git fetch origin
git status
git log main..origin/main --oneline
```

**Check if local main is behind origin/main:**
- If `git log main..origin/main` shows commits → local main is **behind**
- If it shows nothing → local main is **up to date**

**If main is behind:**
1. Switch to main: `git checkout main`
2. Pull the latest: `git pull origin main`
3. Verify: `git log -1 --oneline`

**If main is already up to date:** Skip the pull, but still ensure you're ready to branch from main.

---

### Step 4: Create the New Branch

1. Ensure you're on `main`: `git checkout main` (if not already)
2. Create and switch to the new branch:
   ```
   git checkout -b <branch-name>
   ```
   Example: `git checkout -b feature/password-reset-one-time-code`

3. Verify:
   ```
   git branch --show-current
   git status
   ```

---

### Step 5: Final Output

Report back to the user:
- The new branch name created
- Confirmation that main was updated (if it was behind)
- Any uncommitted changes that are now on the new branch (from Step 2)

---

## Important Rules

- ALWAYS ask for feature vs bugfix before proceeding
- ALWAYS check git changes to propose a meaningful branch name
- ALWAYS fetch and pull main if it's behind origin before creating the branch
- NEVER create a branch from main without ensuring main is up to date
- NEVER skip user confirmation for the branch type and proposed branch name
- Use `main` (or detect `master` if that's the primary branch in the repo)
