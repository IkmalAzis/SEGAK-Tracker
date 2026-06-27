# SEGAK Tracker 🏃

> A cross-platform mobile app to help Malaysian students prepare for the **SEGAK** physical fitness assessment — and help teachers run practice sessions before the real test.

![Status](https://img.shields.io/badge/status-in%20development-orange)
![Platform](https://img.shields.io/badge/platform-Android%20%7C%20iOS%20%7C%20Windows-blue)
![Framework](https://img.shields.io/badge/.NET%20MAUI-8.0-512BD4)

---

## ⚠️ Development Status

This project is **actively under development**. The user interface and navigation are complete and functional across all screens, but the core scoring/data logic is still being implemented. See the [Roadmap](#-roadmap) below for exactly what's done and what's coming.

I'm keeping the repository public during development to track progress transparently. Feedback is welcome.

---

## 📖 About

**SEGAK** (*Standard Kecergasan Fizikal Kebangsaan*) is Malaysia's national physical fitness standard, conducted in schools to assess students across several fitness components. The official test includes:

- **Up & down the bench** (step test — cardiovascular endurance)
- **Push-ups** (upper body muscular strength & endurance)
- **Partial curl-ups** (abdominal muscular strength & endurance)
- **Sit & reach** (flexibility)

**SEGAK Tracker** lets students record practice attempts, follow guided training for each component, and track their improvement over time — so they can walk into the real assessment prepared instead of nervous. Teachers can use it to run structured practice sessions.

## 📱 Screens

<table>
  <tr>
    <td align="center"><img src="docs/screenshots/01-onboarding.png" width="200"/><br/><sub><b>Onboarding</b></sub></td>
    <td align="center"><img src="docs/screenshots/02-home.png" width="200"/><br/><sub><b>Home / Main Menu</b></sub></td>
    <td align="center"><img src="docs/screenshots/03-training-program.png" width="200"/><br/><sub><b>Training Program</b></sub></td>
  </tr>
  <tr>
    <td align="center"><img src="docs/screenshots/05-pushup-detail.png" width="200"/><br/><sub><b>Exercise Detail</b></sub></td>
    <td align="center"><img src="docs/screenshots/04-progress.png" width="200"/><br/><sub><b>Progress & Grade</b></sub></td>
    <td></td>
  </tr>
</table>

## 🛠 Tech Stack

- **.NET MAUI 8.0** — single codebase targeting Android, iOS, Windows & macOS
- **C#** — application logic
- **XAML** — UI layouts
- **Visual Studio 2022**

## ✅ Roadmap

**Done**
- [x] Cross-platform project setup (.NET MAUI, 4 target platforms)
- [x] Onboarding flow & navigation shell
- [x] Flyout menu navigation between pages
- [x] UI/layout for the core screens (onboarding, home, training program, fitness tracking, progress)
- [x] App icon, splash screen & exercise assets

**In Progress / Planned**
- [ ] Save logic for the Fitness Tracking input form
- [ ] SEGAK score & grade calculation (age/gender-based standards)
- [ ] Local data persistence (SQLite / Preferences) so records survive app restarts
- [ ] Progress List driven by real saved data (currently shows sample values)
- [ ] Exercise detail / task-checklist screen wired to real state
- [ ] Reset functionality
- [ ] Input validation & error handling

## 🚀 Getting Started

**Requirements:** Visual Studio 2022 with the **.NET Multi-platform App UI development** workload installed.

```bash
git clone https://github.com/IkmalAzis/SEGAK-Tracker.git
```

Open `SEGAK Tracker.sln` in Visual Studio, select a target (e.g. Android emulator or Windows Machine), and run.

## 📝 Notes

This was originally built as a university Mobile Development coursework project and is now being revisited and completed as a portfolio piece. The commit history reflects that ongoing development.

---

*Built by [Ikmal Azis](https://github.com/IkmalAzis) with .NET MAUI.*
