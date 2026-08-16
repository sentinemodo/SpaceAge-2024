#!/usr/bin/env bash
# Run the NUnit test suite under Mono.
# The tests read fixture files (data.xml, gamein.xml, SampleGame/*) relative to
# the current working directory, so we run from the Tests output directory.
# --inprocess is required: the out-of-process agent targets net-4.8, which Mono
# does not provide (it only offers mono-4.0).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
RUNNER="$REPO_ROOT/.cursor/tools/nunit-runner/NUnit.ConsoleRunner.3.18.3/tools/nunit3-console.exe"
TEST_BIN="$REPO_ROOT/Tests/bin/Debug"

cd "$TEST_BIN"
mono "$RUNNER" Tests.dll --inprocess --work=. "$@"
