# AGENTS.md — handoff notes for AI coding agents

Read this before changing anything. It describes what SEGAK Tracker is, how the code is laid out,
how to build and verify it (including on machines without the MAUI workload), what is done, what
is not, and the traps previous work ran into.

## 1. What this is

A .NET MAUI 8 app (Android / iOS / Windows / Mac Catalyst) that helps Malaysian students prepare
for **SEGAK**, the national school fitness test. Students record practice attempts of the four
scored tests, get a 1–5 score per test and an A–E grade, follow a daily training checklist per
test, and track improvement over time. Fully offline: SQLite + Preferences, no network, no accounts.

The owner is a student finishing this as a portfolio piece (originally university coursework).
They speak Malay; UI text in the app is English.

## 2. Status (as of the last handoff)

Feature complete per the README roadmap. All work is on branch `claude/vigilant-gates-8neuyl`
(not merged to `main`, no PR opened — the owner decides). Verified state:

- 126 xUnit tests pass (`dotnet test SegakTracker.Tests`).
- `tools/XamlCheck` builds clean (C# + XAML + compiled bindings).
- GitHub Actions CI is green: unit tests + a real `net8.0-android` Release build (0 warnings) that
  uploads a signed APK artifact.
- **Never run on a device or emulator.** Visual/UX behaviour is untested by a human. If the owner
  reports UI bugs, they are plausible — nothing guarantees layout correctness beyond compilation.

## 3. Repository layout

```
SEGAK Tracker.sln
SEGAK Tracker/            MAUI app (namespace SEGAK_Tracker). Note the space in the folder name.
  App.xaml(.cs)           Theme resources/styles; picks OnboardingPage or AppShell at start-up
  AppShell.xaml(.cs)      Shell flyout: home, fitnessTracking, trainingProgram, progressList;
                          registers the pushed route "exerciseDetail"
  MauiProgram.cs          DI registrations (all services, pages, view models)
  *Page.xaml(.cs)         Thin pages: set BindingContext to an injected VM, call Load in OnAppearing
  Services/               MAUI implementations of Core interfaces (Preferences, dialogs, Shell nav)
  Converters/             HasTextConverter, HexToColorConverter
  Resources/Styles/       Colors.xaml (theme tokens at the bottom), Styles.xaml
  Platforms/              Per-platform bootstrap; AndroidManifest has NO permissions, backup off
SegakTracker.Core/        net8.0, no MAUI dependency — all logic lives here
  Models/                 FitnessRecord, TaskCompletion (SQLite tables), Gender, FitnessTest
  Scoring/                SegakNorms (norm tables), ScoreThresholds, SegakScorer, SegakGrading,
                          FitnessInputValidator
  Data/                   SegakDatabase (lazy SQLite connection), repositories + interfaces
  Training/               TrainingCatalog: the 4 exercise programmes and their tasks
  Services/               ISettingsService, IDialogService, INavigationService, Routes
  ViewModels/             One VM per screen (CommunityToolkit.Mvvm source generators)
SegakTracker.Tests/       xUnit; fakes in ViewModels/Fakes.cs, temp DB helpers in TestSupport.cs
tools/XamlCheck/          Compile-check harness for the app without the MAUI workload (see §4)
.github/workflows/ci.yml  Tests + Android build
docs/screenshots/         Design MOCKUPS (not real app screenshots) — the UI follows them
```

## 4. Build and verify

You need the **.NET 8 SDK**. On Ubuntu without access to dot.net: `apt-get install dotnet-sdk-8.0`.

| Check | Command | Needs |
|---|---|---|
| Unit tests | `dotnet test SegakTracker.Tests` | .NET 8 SDK only |
| App C#/XAML compile check | `dotnet build tools/XamlCheck` | .NET 8 SDK + NuGet |
| Real Android build | see `.github/workflows/ci.yml` | MAUI android workload + Android SDK + JDK 17 |
| Full app in VS | open `SEGAK Tracker.sln`, startup = SEGAK Tracker | Windows + VS 2022 MAUI workload |

Before every push: run the first two. `tools/XamlCheck` uses `MauiStrictXamlCompilation`, so a
typo in a compiled binding (`{Binding Foo}` under an `x:DataType`) fails the build — that is the
only binding verification available without a device. Do not run `dotnet test` on the `.sln` on
Linux: the MAUI project in it cannot build there.

If your environment cannot build Android, push and read the CI result instead (Actions run on
pushes to `main` and `claude/**`).

## 5. Architecture and conventions

- **MVVM.** Pages contain no logic beyond wiring. VMs depend only on Core interfaces, so every VM
  is unit-testable with the fakes in `SegakTracker.Tests/ViewModels/Fakes.cs`. Add a test for any
  VM change.
- **CommunityToolkit.Mvvm 8.4** with field-based `[ObservableProperty] private T _name;` and
  `[RelayCommand]`. Keep that style (no partial properties — C# 12).
- `TreatWarningsAsErrors` is on in Core. Nullable is enabled everywhere.
- **Compiled bindings:** every page and `DataTemplate` declares `x:DataType`. Commands invoked from
  inside a template use `{Binding Source={RelativeSource AncestorType={x:Type vm:XxxViewModel}}, Path=...}`
  (not compiled; don't add `x:DataType` inside the binding — unsupported in MAUI 8).
- **Navigation:** `INavigationService`. Flyout sections with absolute routes (`"//" + Routes.ProgressList`);
  the exercise checklist with `GoToAsync(Routes.ExerciseDetail, {"id": exerciseId})`, received by
  `ExerciseDetailPage.ApplyQueryAttributes`. Onboarding is outside Shell; `ShowMainAppAsync`
  swaps `MainPage` to a DI-resolved `AppShell`.
- **Time:** inject `TimeProvider` (tests use `FakeTimeProvider` in `TestSupport.cs`). Records are
  stored in UTC (`RecordedAtUtc`; SQLite loses `DateTimeKind`, use `RecordedAtLocal` for display).
  Training ticks are keyed by **local** day `yyyy-MM-dd` and reset daily.
- **Errors:** VMs catch exceptions and show a generic dialog — never put `ex.Message` in UI text.
- **Theme:** one dark purple theme (`UserAppTheme = Dark`). Use the tokens in `Colors.xaml`
  (`PageBackground`, `ScoreYellow`, `Highlight`, ...) and the styles in `App.xaml`
  (`ScreenTitle`, `Card`, `Pill`, `PrimaryButton`, ...). Use `Border`, not `Frame`.
- XAML text must escape `&` as `&amp;` (the original code didn't compile because of this).

## 6. SEGAK domain rules (encoded in Core/Scoring)

- Four scored tests: step test pulse (bpm, **lower is better**), push-ups (girls do modified
  push-ups), partial curl-ups, sit & reach (cm, decimals allowed). BMI is part of SEGAK but **not**
  implemented.
- Each test scores 1–5 from age (10–17) + gender norms; total 4–20.
- Grades: A 18–20, B 15–17; then primary (age ≤ 12) C 12–14, D 8–11, E 4–7; secondary C 10–14,
  D 7–9, E 4–6. See `SegakGrading.GradeFor`.
- **Norm data caveat:** values in `SegakNorms.cs` were gathered from web search summaries of the
  KPM *Panduan SEGAK* because the source pages were blocked. Rows marked `IsEstimated: true`
  (female 11, 13, 15, 17; male 17) are interpolated/copied. The app shows a notice for them.
  Treat all numbers as unverified until checked against the official booklet; `SegakNormsTests`
  enforces that every row stays monotonic and reaches all five scores.
- Input limits (`FitnessInputValidator`): age 10–17, pulse 40–220, reps 0–150, reach 0–100 cm;
  "31,5" and "31.5" both accepted.

## 7. Known gaps / suggested next work (ask the owner before big changes)

1. **Device testing** — run on an Android emulator/phone and fix layout issues. Highest value.
2. **Verify norms** against the official KPM booklet and clear `IsEstimated` flags.
3. **.NET MAUI 8 is out of support (May 2025).** Upgrading to .NET 10 MAUI is recommended but may
   require a newer Visual Studio than the owner's VS 2022 — confirm with them first. The .NET 10
   SDK treats `net8.0-android` as an error (NETSDK1202), which is why CI pins the 8.0 SDK.
4. Optional features from the README roadmap: BMI with WHO BMI-for-age, teacher mode
   (multiple students), Bahasa Melayu localisation.
5. iOS/Mac Catalyst have never been built (need macOS).

## 8. Gotchas already hit

- CI: the runner's default SDK is .NET 10; the workflow creates a `global.json` pinning the latest
  8.0 SDK before `dotnet workload install maui-android`. Don't pass `-p:TargetFrameworks=...` on
  the command line — global properties flow into `SegakTracker.Core` and break it. The csproj
  already drops iOS/Mac targets on Linux.
- `sqlite-net-pcl` 1.11 brings SQLitePCLRaw 3.x itself. Do **not** add `SQLitePCLRaw.bundle_green`
  2.x back: it mixes versions and 2.1.x native SQLite has a High advisory (GHSA-2m69-gcr7-jv3q).
  Check with `dotnet list SegakTracker.Tests package --vulnerable --include-transitive`.
- Shell resolves pages from DI only if they are registered in `MauiProgram.cs` — register every new
  page and its VM there.
- Don't put a `FlyoutPage` or `NavigationPage` inside `Shell` (the original app did; unsupported).

## 9. Working with the owner

- Commit in small, verified steps and push to the working branch; don't push to `main` or open PRs
  unless asked.
- Explain results to the owner in casual Malay; keep code, comments and commit messages in English.
