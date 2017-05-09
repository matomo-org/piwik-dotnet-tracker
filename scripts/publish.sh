#!/bin/bash

# http://redsymbol.net/articles/unofficial-bash-strict-mode/
set -euo pipefail
IFS=$'\n\t'

echo -n 'new version'
read NEW_VERSION

git checkout master
git pull

# bump vesion in AssemblyInfo.cs
sed -i "s/^\[assembly: AssemblyVersion[(]".*"[)]\]/[assembly: AssemblyVersion(\"$NEW_VERSION\")]/" Piwik.Tracker/Properties/AssemblyInfo.cs

wrun dotnet build Piwik.Tracker/Piwik.Tracker.csproj -c Release
wrun nuget pack Piwik.Tracker/Piwik.Tracker.csproj -Prop Configuration=Release -Symbols
wrun nuget push Piwik.Tracker.$NEW_VERSION.nupkg -Source nuget.org

git add Piwik.Tracker/Properties/AssemblyInfo.cs
git commit -m "chore: bump version to $NEW_VERSION"
git tag -a $NEW_VERSION -m $NEW_VERSION
git push --follow-tags
