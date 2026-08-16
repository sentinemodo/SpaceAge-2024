# ADR-0002: Windows-1251 for all game I/O

Date: 2026-08-16  
Status: Accepted (documents existing practice)

## Context

Catalog XML, game snapshots, orders, and faction reports historically use Cyrillic-capable **Windows-1251**. Default UTF-8 would corrupt existing `data.xml` / SampleGame goldens and player files.

## Decision

Read and write game XML, order files, reports, and RELEASE `error.log` with `Encoding.GetEncoding(1251)`. XML declarations use `encoding="windows-1251"`.

Cloud images must install **`libmono-i18n4.0-all`** so code page 1251 is available on Mono.

## Consequences

- Tests compare files as 1251 text (`TTest` helpers).
- Do not “fix encoding” to UTF-8 as a drive-by change.
- Changing encoding is a breaking GM/player-data migration and requires a new ADR plus regenerated goldens.
