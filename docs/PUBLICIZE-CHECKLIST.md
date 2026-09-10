# StackContract — Publicize checklist

Do these in order. Check boxes as you go. JARVIS will not flip visibility or publish until you say so.

**Repo today:** `https://github.com/munyaimanuwel/stackcontract` (public)  
**Target slug:** `stackcontract`  
**Sequence after public:** NuGet tool → Pro Kit (Polar)

---

## 0. Pre-flight (already mostly done)

- [x] Product rename ComposeContract → StackContract merged to `main` (PR #2)
- [x] Profile README pin updated to StackContract
- [x] Local + portable skill → `stack-contract-check`
- [x] GitHub **repository rename** to `stackcontract`  
  Path: repo → **Settings** → **General** → Repository name → `stackcontract` → Rename  
  Old `composecontract` URL redirects; update bookmarks / NuGet `RepositoryUrl` after rename
- [x] Confirm `main` builds locally: `dotnet test StackContract.sln -c Release` (8/8 passed on `publicize`; sln was missing project build configs)
- [x] Confirm tool pack: `dotnet pack StackContract.sln -c Release -o ./nupkg` then dry-run install of `stackcontract` (`stackcontract 0.1.0`; `validate --strict` on `samples/aspnet-compose` → OK)

---

## 1. Scrub before Public (secrets / PII)

- [x] `.gitignore` covers `.env`, `*.user`, `nupkg/`, `bin/`, `obj/` (also `*.nupkg`, `tool-install/`)
- [x] No real secrets in git history on `main` (sample `.env.example` keys only; test fixture password is a fake leak-check string)
- [x] No employer names, private hostnames, Tailscale IPs, or customer paths in README/samples
- [x] LICENSE is MIT and matches intent
- [x] README states: *Not affiliated with Docker, Inc.*
- [x] Repo **Description** updated (e.g. “Local-first .NET stack/config contract checker”)
- [x] Optional: Topics — `dotnet`, `docker-compose`, `config`, `cli`, `github-actions`

---

## 2. Flip visibility

- [x] Settings → **General** → Danger Zone → **Change repository visibility** → **Public**
- [x] Confirm anonymous browse works: `https://github.com/munyaimanuwel/stackcontract` (HTTP 200, `logged_in=no`)
- [ ] Pin the repo on your GitHub profile (API token cannot pin; Profile → Customize your pins)
- [x] Update profile README link if slug changed (drop “pending rename” note)

---

## 3. First public release hygiene

- [x] Tag `v0.1.0` (or next semver) on the public `main` commit
- [x] GitHub Release notes: what it does, install, `stackcontract validate --strict`, migration from ComposeContract names
- [x] Smoke from a clean machine: clone → pack → `dotnet tool install -g stackcontract` → `validate` on `samples/` (clone of `v0.1.0` → pack → tool-path install → `validate --strict` OK)

---

## 4. NuGet (after Public)

Use **Trusted Publishing** (GitHub Actions OIDC). Do not create a long-lived API key.

- [ ] nuget.org account (personal Microsoft or GitHub sign-in — not work email)
- [ ] Trusted Publishing policy on nuget.org (username → **Trusted Publishing** → add):
  - Repository Owner: `munyaimanuwel`
  - Repository: `stackcontract`
  - Workflow File: `publish.yml` (filename only)
  - Environment: leave empty
  - Scopes: allow **new packages** and **new versions**; glob `stackcontract*`
- [ ] GitHub repo secret `NUGET_USER` = nuget.org **profile name** (not email)
- [x] Package IDs free: `stackcontract` tool + `StackContract.*` libraries
- [x] Set `PackageProjectUrl` / `RepositoryUrl` to final public URL (`https://github.com/munyaimanuwel/stackcontract`)
- [x] Workflow `.github/workflows/publish.yml` (OIDC `NuGet/login`, no stored API key)
- [ ] Run **Actions → Publish NuGet → Run workflow** with tag `v0.1.0`
- [ ] Verify: `dotnet tool install -g stackcontract` (no local source)
- [ ] README badge: NuGet version + install one-liner

---

## 5. Pro Kit path (after NuGet is live)

- [ ] Polar (or chosen merchant) product: **StackContract Pro Kit**
- [x] Clear free vs paid boundary (MIT core stays free; Pro = kits / CI extras / templates — no phone-home in Core/Engine)
- [ ] Landing copy + pricing
- [ ] Delivery: private repo, download, or license key — pick one and document
- [x] Link Pro Kit from public README (“Sponsors / Pro” section) — Polar URL still TBD

---

## 6. Announce (optional, after 2–4)

- [ ] LinkedIn / X short post (personal accounts only)
- [ ] OfferZen / freelance profiles: one-line portfolio link
- [ ] Contra / Upwork portfolio attachment if useful

---

## Stop lines (do not skip)

1. **Do not** make Public until scrub (section 1) is checked.  
2. **Do not** push NuGet until the repo is Public, `RepositoryUrl` matches the live slug, and Trusted Publishing is configured (no long-lived API key).  
3. **Do not** sell Pro Kit until free core is installable from NuGet without a private feed.  
4. Personal email / accounts only — no work email or employer disclosure.

---

## One-liner status (keep current)

| Step | Status |
|------|--------|
| Rename code → StackContract | Done (main) |
| Repo slug → `stackcontract` | Done |
| Visibility → Public | Done |
| GitHub Release `v0.1.0` | Done |
| NuGet publish | Blocked on nuget.org account + Trusted Publishing policy |
| Pro Kit | Blocked on NuGet + Polar product |

When you’re ready for a step, tell JARVIS which checkbox number to execute (e.g. “do 4”) — otherwise this doc stays checklist-only.
