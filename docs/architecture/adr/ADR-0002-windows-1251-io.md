# ADR-0002: Windows-1251 for all game I/O

Date: 2026-08-16  
Status: Accepted (documents existing practice)

## Context

Catalog XML, game snapshots, orders, and faction reports historically use Cyrillic-capable **Windows-1251**. Default UTF-8 would corrupt existing `data.xml` / SampleGame goldens and player files.

## Decision

Read and write game XML, order files, reports, and RELEASE `error.log` with `Encoding.GetEncoding(1251)`. XML declarations use `encoding="windows-1251"`.

The cloud image installs **`mono-complete`**, which provides code page 1251 on Mono (verified end-to-end: a full turn writes 1251-encoded reports and the suite passes). No separate i18n package is required.

## Consequences

- Tests compare files as 1251 text (`TTest` helpers).
- Do not “fix encoding” to UTF-8 as a drive-by change.
- Changing encoding is a breaking GM/player-data migration and requires a new ADR plus regenerated goldens.
