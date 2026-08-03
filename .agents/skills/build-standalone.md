# Standalone Desktop Build Workflow Skill

## Purpose
Outline the exact workflow for triggering GitHub Actions to compile standalone desktop releases using electron-builder.

## Workflow & Steps
1. **Pre-requisites Check**:
   - Ensure local changes are committed and pushed to the designated branch.
   - Verify package versioning in `package.json` and release configuration for `electron-builder`.
2. **Trigger GitHub Actions Workflow**:
   - Trigger the desktop build workflow via GitHub CLI or GitHub web interface:
     ```bash
     gh workflow run release-desktop.yml --ref main
     ```
   - Alternatively, dispatch via GitHub REST API workflow dispatch endpoint.
3. **Compilation & Artifact Inspection**:
   - Monitor the GitHub Action run status.
   - Verify `electron-builder` completes target platform builds (Windows `.exe`/`.msi`, Linux `.AppImage`, macOS `.dmg`).
   - Download and verify artifact checksums upon completion.
