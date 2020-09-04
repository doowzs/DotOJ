# Changelog

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
