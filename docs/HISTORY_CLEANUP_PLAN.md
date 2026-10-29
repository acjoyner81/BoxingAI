HISTORY CLEANUP PLAN
===================

This document contains a safe, repeatable plan to purge generated and large files from history using git-filter-repo.

Files/paths proposed for removal (also listed in `paths-to-remove.txt`):

- Temp/
- Logs/
- results/
- UserSettings/
-.vs/
-.vscode/
- Assembly-CSharp-Editor.csproj
- Assembly-CSharp.csproj
- BoxingAI_recreated.slnx
- BoxingAI.slnx

Pre-flight (recommended)
------------------------
1. Make sure your working tree is clean and you've pushed any desired branches.
   - git status
   - git push --all

2. Create a remote backup branch so the full history is preserved before any rewrite.
   - BACKUP_NAME="backup/pre-filter-$(date -u +%Y%m%dT%H%M%SZ)"
   - git branch "$BACKUP_NAME"
   - git push origin "$BACKUP_NAME"

3. Install git-filter-repo
   - python3 -m pip install --user git-filter-repo
   - (or on macOS) brew install git-filter-repo

Dry-run (mirror) — safe verification
-----------------------------------
Run the rewrite in a throwaway mirror repository first to confirm the outcome.

1. Clone a bare mirror somewhere temporary:
   - git clone --mirror https://github.com/acjoyner81/BoxingAI.git /tmp/BoxingAI-mirror.git

2. Change directory and run git-filter-repo against the mirror using the paths file.
   - cd /tmp/BoxingAI-mirror.git
   - git filter-repo --paths-from-file ../paths-to-remove.txt --invert-paths

   Note: Running on a mirror performs the rewrite locally and allows inspection of the result.

3. Verify the mirror (optional):
   - git reflog expire --expire=now --all
   - git gc --prune=now --aggressive
   - du -sh .  # compare size before/after

Execution (when ready)
----------------------
Once you confirm the mirror outcome, run the rewrite in the local clone of the repository:

1. Ensure you have the backup branch pushed (see Pre-flight step 2).

2. Run filter-repo locally (from repo root):
   - git filter-repo --paths-from-file paths-to-remove.txt --invert-paths --force

3. Cleanup and pack:
   - git reflog expire --expire=now --all
   - git gc --prune=now --aggressive

4. Force-push rewritten branches and tags to remote (this is destructive):
   - git push origin --force --all
   - git push origin --force --tags

Post-action (collaborator instructions)
--------------------------------------
After the force-push, all collaborators should reclone the repository (recommended):

1. git clone https://github.com/acjoyner81/BoxingAI.git

If they cannot reclone, they can try this in their existing clone (riskier):
  - git fetch origin
  - git checkout main
  - git reset --hard origin/main
  - git reflog expire --expire=now --all
  - git gc --prune=now --aggressive

Branch protection
-----------------
If `main` is protected by branch-protection rules that block force-push, you'll need to temporarily disable the protection or have an admin perform the force-push.

Questions or help
-----------------
If you want me to run any step for you (mirror dry-run, or the full execution), reply with `execute` (I will create the backup branch first). If you want me to only prepare files (done), confirm and I will wait for your go-ahead.
