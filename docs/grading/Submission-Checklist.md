# Submission Checklist

## Repository hygiene

- [ ] `git status` is clean (no unintended untracked/modified files)
- [ ] No secrets committed — grep `appsettings.json`/`appsettings.Development.json` for any
      real connection string, password, or API key
- [ ] `.gitignore` excludes `bin/`, `obj/`, generated backups (`App_Data/Backups/`), and
      test results

## Build and test

- [ ] `dotnet build` succeeds with 0 errors, 0 warnings
- [ ] `dotnet test` run against a real MySQL instance with `CUSTOMERLEDGER_TEST_CONNECTION`
      set — record the actual pass/fail/skip counts, do not carry over the sandbox's skip
      counts as if they were passes

## Database

- [ ] Migration applies cleanly to a fresh database
      (`dotnet ef database update`)
- [ ] Views and triggers created (`database/views/CreateViews.sql`,
      `database/triggers/CreateTriggers.sql`)
- [ ] Verification scripts run without unexpected errors
      (`database/verification/*.sql`)

## Documentation completeness

- [ ] Every file listed in [Grading-Checklist.md](Grading-Checklist.md) exists and reflects
      the actual, current code (re-read each one against the codebase — do not assume it's
      still accurate after later changes)
- [ ] All six release documents are present and each one's Tests/Verification sections
      reflect a real test run, not the development sandbox's environment-limited run
- [ ] README's "Current Version" badge and roadmap table match the actual latest release

## Demonstration readiness

- [ ] [Demonstration-Script.md](../viva/Demonstration-Script.md) has been rehearsed
      end-to-end at least once on the actual machine that will be used for grading
- [ ] Evidence from [Evidence-Checklist.md](../testing/Evidence-Checklist.md) has been
      captured, not left as placeholders

## Final sanity check

- [ ] No file anywhere in the repository claims a capability, test result, or screenshot
      that was not actually produced — if something couldn't be verified, it says so
      explicitly (this has been the standing discipline throughout every release document)
