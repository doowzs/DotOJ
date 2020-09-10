# Changelog

## 0.5.2 - 2020-09-10

### Added

- Custom judging service using `isolate` in worker.
- Check program output line by line ignoring trailing spaces.

### Removed

- Dropped Judge0 API in worker and docker containers.

## 0.4.5 - 2020-09-09

### Added

- Add support for importing/exporting problem as a zip archive.
- A tooltip will appear when clicking on copy icon of sample data.

### Changed

- Use a local modified version of markdown component.
- Updated KaTeX rendering option and blockquote stylesheet.
- Changed upload request size limit from default to 300MiB.

### Fixed

- Fixed form data handling logic that could prevent updating problem.
- Fixed listing test cases using a list in `nz-table`.
- Fixed wrong queuing status of submissions caused by DB caching.
- Fixed extracting test cases when importing a problem archive.

## 0.3.1 - 2020-09-07

### Added

- Show submission time, memory and message in detail drawer.
- Use a monospace font (Ubuntu Mono) in source code displaying.

### Fixed

- Fixed file permissions in special judge file archive.
- Fixed handling empty output and message (null string) of runs.
- Fixed special judge logic when the original run fails.
- Fixed UTF8 character displaying in Base64 encoded strings.

## 0.2.5 - 2020-09-07

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
- Fixed an issue that forbids submitting code in textarea.
- Fixed RO permission when worker opens file in judge-data folder.
- Fixed incorrect integer conversion in backend interaction of worker.
- Fixed empty sequence error when calculating score for problem.

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
