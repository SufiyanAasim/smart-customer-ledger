# Changelog

All notable changes to **Smart Customer Ledger** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [7.0.0] - 2024-06-28

### Added
- Supervised Logistic Regression Payment-Risk Model for credit default probability scoring.
- Recency, Frequency, and Monetary (RFM) customer segmentation engine.
- Smart AI Credit & Ledger Assistant widget on Analytics dashboard.
- Dark/Light mode theme toggle with persistent state.
- Author leadership & Credits page (`/Home/Credits`) featuring Sufiyan Aasim's profile, GitHub, and email buttons.

### Changed
- Brand title updated to **Smart Customer Ledger** across all files and views.
- Top header logo enlarged and brand text removed for a clean logo-only presentation.
- Admin dropdown menu background styled to match dark glassmorphic navbar.

### Improved
- Dark mode text contrast across all cards, badges, and tech stack lists.
- Unified right header menu controls for user email, branch context, theme switcher, and logout.

### Fixed
- Fixed Z-index dropdown overlap issue so dropdown menus overlay cleanly on top of all page elements.
- Fixed 404 navigation routing issues across area controllers.

### Security
- Enforced strict role-based access policy on Analytics and Admin sub-modules.

### Documentation
- Updated README.md, release documents, architectural guides, and API schemas.

---

## [6.0.0] - 2024-06-18

### Added
- Logical sharding architecture with deterministic `BranchId % ShardCount` routing.
- Admin Shard Status screen and cross-shard revenue aggregation.

---

## [5.0.0] - 2024-06-12

### Added
- Primary/Replica database separation and replica health monitoring service.

---

## [4.0.0] - 2024-06-05

### Added
- Database data dictionary generator and Mermaid ERD visualization diagrams.

---

## [3.0.0] - 2024-05-28

### Added
- MySQL backup/restore engine and validated CSV/JSON import/export utilities.

---

## [2.0.0] - 2024-05-20

### Added
- ACID transactional payment settlement, installment remainder splitting, and account balance reconciliation.

---

## [1.0.0] - 2024-05-10

### Added
- Initial core architecture, EF Core mappings, MVC views, and MySQL triggers.
