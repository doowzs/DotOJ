# Changelog

## 0.2.0 - 2020-09-06

### Added

- Special judge and contest modes support.
- View submission detail, judge worker name and timestamp.
- Add bulletin model/service and show bulletins on homepage.
- Add user account manage pages and admin reset password page.

### Changed

- Rewrite judge worker classes to make them pluggable and configurable.
- Update UX design for navigation bar, submission creator and admin pages.

### Fixed

- Fixed submission timeline updating logic and verdict display.
- Fixed using UTC timestamp in creating and modifying contests.
- Fixed hunger in submission triggers caused by wrong picking logic.

## 0.1.0 - 2020-09-05

### Added

- Judging services now run as workers.
- Add Docker release script and CI config.

### Changed

- Rewrite judging logic: one test case at a time.
- Reconstruct project folders: use one solution with multiple projects.
- `Data` and `Notification` now is built as standalone libraries.
- Update Dockerize files and app configuration format.

### Removed

- Remove DB models and DbSets that are not used.
- Remove Hangfire and SQL server dependencies.

### Fixed

- Improved judging service stability and consistency.
- Fixed concurrency bug by triggering a rejudge on a running submission.
