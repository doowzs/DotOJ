# Changelog

## 1.6.9 - 2021-06-11

### Added

- Added exam mode where only one contest is accessible.
- Added support for Github Actions and binary releases.

### Changed

- Changed display language to Simplified Chinese.
- Python3 are now judged with Pylint and pre-compile.
- Use `docker-compose up --scale` to create multiple workers easily.
- Worker's Box IDs are now determined by unique IP addresses of workers.
- Job requests are now dispatched to RebbitMQ in a background queue.

### Fixed

- Fixed handling negative interval in cron job background service.
- Fixed using undefined values in client submission service.

## 1.5.26 - 2021-03-26

### Added

- Added lab problems which accepts archive submission and custom judging.
- Added feature to download viewable submissions as file or zip archive.

### Changed

- Changed file naming rules when exporting submissions.
- Refactored major implementations of runners.
- Submissions before contest will not appear in statistics.
- Submission manager can view all submissions before contest begins.
- Use tencent nuget mirror when building env container.
- Some required fields of lab problems are now filled in client.
- Lab problem judging will stop on failure of compile steps and stages.
- Showing units of time, memory and code length on submission list and timeline.
- Submit token will not be invalidated after consuming.
- If submission failed on samples, a score of 0 will not be shown.

### Fixed

- Fixed wrong predicate when exporting accepted submissions.
- Fixed no compiler output for C# Roslyn compiler.
- Fixed substring handling and checker compiling in worker.
- Fixed editing testkit lab problems in client.
- Fixed not opening manifest.json in read only mode.
- Fixed problem statistics not invalidated after updating related data.
- Fixed copying .git folder when it does not exist for lab problems.

## 1.4.2 - 2021-02-25

This version contains breaking changes.

- Data folder structure has been changed. All judge data now goes to `${DATA}/tests`.

### Added

- Added plagiarism detection.
- Added score bonus and decay for contests.
- Added problem and queue statistics information.
- Added user import for admin use.

### Changed

- Submission queue is now reimplemented with RabbitMQ.
- Submission will become viewable after solving the problem.
- Authentication page is updated for a consistent style.
- Contest standings page is redesigned.
- Submission detail page is redesigned.
- Deleting account by user is removed for security reasons.

### Fixed

- Fixed problems may be caused by long error message from compiler.
- Fixed null reference errors in angular app.
- Fixed admin button showing up in non-admin users' menus.
- Fixed wrong contest mode description in contest management page.

## 1.3.1 - 2021-01-17

### Added

- Added CI config for Drone.
- Can now export submissions of a problem.

### Changed

- Upgrade framework and runtime to dotnet 5.0.
- Submissions made by admin before contest is now hidden from public.

### Fixed

- Fixed incorrect pagination when making the 6th submission.
- Fixed handling interrupted judging results by triggering a rejudge.

## 1.2.15 - 2020-10-30

### Added

- Redesign user experience with Bootstrap.
- Add support for Ajax.org Cloud9 Editor for coding.
- Add support for Vditor for Markdown editing.
- All times in client are calculated and shifted to server time.
- Contest and problems are only fetched once using shareReplay.
- WebApp version is now displayed in footer of pages.

### Changed

- Administrators and contest managers can submit to any problems before contest begins.
- Move submission verdict component to be a dedicated component and module.
- Increased output size limit to the larger one of either double of standard output or 10MiB.
- Privileged users can now enter contest by buttons and links before the contest has begun.
- Redesigned buttons to toggle code editor in problem description pages.
- Current contest is now ordered by end time instead of begin time.
- Updated background highlight style of bold text in all pages.

### Fixed

- Fixed subscribing on user subject in destroyed component instances.
- Fixed value changed after check error in code editor.
- Fixed setting fullscreen query parameter in problem view page.
- Fixed null reference in submission timeline component.
- Fixed a performance issue caused by repeatedly force resizing code editor.
- Fixed replaceUrl set to false when updating a contest in admin page.
- Fixed overwrite flag when copying a new compiled checker to judge data folder.
- Fixed high CPU usage caused by redrawing progress bar too frequently.

### Removed

- Dropped Ant Design support.

## 1.1.15 - 2020-09-27

### Added

- Allow contest manager to create submission without registering.
- Add button to export standings page to an Excel workbook.
- If the program is killed by signal 11, will show SIGSEGV in message.
- Special judge checker will be cached after compiling until problem is updated.

### Changed

- Non-participate users will not have rank in standings page.
- Update ranking calculation logic to allow same ranking of participants.
- Disabled ASP.NET HTTPS redirection, which shall be handled by Caddy.
- Set cookie policy to SameSiteMode.Lax for non-HTTPS support.
- Use ngx-clipboard for copying test input/output data to clipboard.
- Users can now login with usernames instead of emails.
- Limit width of sample case data in problem pages.
- Swap is now disabled in worker containers to allow precise memory control.

### Fixed

- Fixed parameter name in handling deleting registrations.
- Fixed non-zero seconds and milliseconds of timestamps in submitted form data.
- Fixed updating a non-accepted verdict of submissions in worker.
- Fixed rebuilding statistics after copying registrations.
- Fixed not expiring cookie when logging out caused by `SameSite=None` in non-secure environment.
- Fixed output file size limit to 2x of standard output.
- Fixed an error in admin problem form where C++ language is not found in list.
- Fixed failure percentage not showing for Memory Limit Exceeded verdict.
- Fixed handling error when copying checker in read-only file systems.
- Fixed wrong pagination link in registration management table.
- Fixed wrong throttling of submission creation API.

## 1.0.9 - 2020-09-14

### Added

- Add problem statistics to problem detail page.
- Add reload button to problem detail, submissions and standings pages.
- Show contestant ID in tooltip on contest standings page.
- Set images to be horizontally centered and add width limit.

### Changed

- Rename project to dotOJ (AKA. .OJ, NTOJ, OJ Core, etc.).
- When API request failed with 401 Unauthorized, redirect user to login page.
- When the user is unauthorized, return 401 error instead of 302 to login page.
- When viewing submission detail, contestant ID is displayed instead of nickname.

### Fixed

- Fixed multipart body size limit of uploading test cases and archives.
- Fixed multiple worker conflict caused by not setting isolated box ID.
- Fixed security issue where user can take a long contestant ID or name.
- Fixed concurrency bug in standings page caused by using contest before loaded.
- Fixed security issue of problem limits caused by DB caching in workers.
- Fixed error of using a disposed ServiceScope in workers.
- Fixed limitation of log files and size of Docker containers.
- Fixed pagination of table on submission list page.
- Fixed date input of updating data and null reference error when deleting submissions.
- Fixed wrong byte length of Base64 encoded source code calculated by server.

## 0.5.7 - 2020-09-11

### Added

- Custom judging service using `isolate` in worker.
- Added C/C++, Java, Python, Golang, Rust, C# and Haskell runners.
- Check program output line by line ignoring trailing spaces.
- Limit RO access to outer box when running contestant programs.
- Added quick links for administrator to edit contest or problem.
- Added progress bar and end of contest prompt in contest pages.
- Added help page to show judging specs and available languages.

### Changed

- Pending submissions in submission list page will be updated automatically.
- Users can only view submissions of others after contest ends.

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
