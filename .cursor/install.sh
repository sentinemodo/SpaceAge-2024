#!/usr/bin/env bash
# Idempotent setup for the SpaceAge .NET Framework 4.8 solution on Linux.
# .NET Framework is built and run through Mono (mono + xbuild). NuGet packages
# use the classic packages.config layout, restored with nuget.exe under Mono.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

TOOLS_DIR="$REPO_ROOT/.cursor/tools"
NUGET_EXE="$TOOLS_DIR/nuget.exe"
NUGET_VERSION="v6.9.1"
NUNIT_RUNNER_VERSION="3.18.3"
mkdir -p "$TOOLS_DIR"

if [ "$(id -u)" -eq 0 ]; then SUDO=""; else SUDO="sudo"; fi

# 1. System toolchain: Mono provides the .NET Framework runtime, xbuild and mcs.
if ! command -v mono >/dev/null 2>&1 || ! command -v xbuild >/dev/null 2>&1; then
  echo "==> Installing mono-complete"
  $SUDO apt-get update -y
  $SUDO DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends mono-complete
else
  echo "==> mono/xbuild already present: $(mono --version | head -n1)"
fi

# 2. nuget.exe (runs under Mono) for classic packages.config restore.
if [ ! -f "$NUGET_EXE" ]; then
  echo "==> Downloading nuget.exe ($NUGET_VERSION)"
  curl -sSL -o "$NUGET_EXE" "https://dist.nuget.org/win-x86-commandline/$NUGET_VERSION/nuget.exe"
else
  echo "==> nuget.exe already present"
fi

# 3. Restore NuGet packages into ./packages (packages.config projects).
echo "==> Restoring NuGet packages"
mono "$NUGET_EXE" restore "$REPO_ROOT/SpaceAge.sln"

# 4. NUnit console runner for running the test suite from the CLI.
if [ ! -f "$TOOLS_DIR/nunit-runner/NUnit.ConsoleRunner.$NUNIT_RUNNER_VERSION/tools/nunit3-console.exe" ]; then
  echo "==> Installing NUnit.ConsoleRunner $NUNIT_RUNNER_VERSION"
  mono "$NUGET_EXE" install NUnit.ConsoleRunner -Version "$NUNIT_RUNNER_VERSION" \
    -OutputDirectory "$TOOLS_DIR/nunit-runner"
else
  echo "==> NUnit console runner already present"
fi

# 5. Build the solution (Debug).
# The Tests project references the legacy nunit.core / nunit.core.interfaces
# assemblies via a machine-specific HintPath from the original author's PC. The
# matching DLLs are committed under Tests/nunit, so we add that directory to the
# assembly search path (semicolons escaped as %3B for the xbuild command line)
# instead of editing the checked-in project file.
NUNIT_LEGACY_DIR="$REPO_ROOT/Tests/nunit"
SEARCH_PATHS="{CandidateAssemblyFiles}%3B{HintPathFromItem}%3B{TargetFrameworkDirectory}%3B{AssemblyFolders}%3B{GAC}%3B{RawFileName}%3B${NUNIT_LEGACY_DIR}%3B{OutDir}"

echo "==> Building SpaceAge.sln (Debug)"
xbuild /p:Configuration=Debug "/p:AssemblySearchPaths=${SEARCH_PATHS}" "$REPO_ROOT/SpaceAge.sln"

echo "==> Setup complete."
