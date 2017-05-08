#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

github_changelog_generator --token $CHANGELOG_GITHUB_TOKEN
