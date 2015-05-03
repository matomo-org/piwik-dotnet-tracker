# Piwik C# Tracking API

This is the official C# implementation of the [Piwik Tracking API](http://piwik.org/docs/tracking-api/).

Check the [release page](https://github.com/piwik/piwik-dotnet-tracker/releases)
for supported versions of Piwik.

## Usage

Three Visual Studio Solutions are provided : 

* [Piwik.Tracker.sln](Piwik.Tracker.sln) : Library project
* [Piwik.Tracker.Samples.sln](Piwik.Tracker.Samples.sln) : Console Samples project
* [Piwik.Tracker.Web.Samples.sln](Piwik.Tracker.Web.Samples.sln) : ASP.NET Samples project

## How to contribute

The Piwik C# Tracking API is a translation of the [official PHP Tracking API](https://github.com/piwik/piwik/tree/master/libs/PiwikTracker) in C#.

Translating the project is currently a manual process.

Manually translating the project is tedious because it requires

* identifying features that have already been translated
* merging code

We would ideally like to automate this process, we welcome contributions aimed towards this goal.

To ease the process in the mean time, the following rules are applied to any new code contributions :

* a commit in the PHP project implies a commit in the C# project with the same message and content
* one-to-one tag mapping between the PHP and C# projects
* the C# code should mirror as close as possible the PHP code

As long as we do not have an automated process, we welcome suggestions in improving the manual process.

### Code style

To mirror as close as possible the PHP code, we copy-paste the PHP code in the C# class and alter it so it compiles.

This can lead in a loss of C# best practices. We consider the time savings an acceptable trade-off.

## Publishing the project to NuGet

### Requirements

1. The process detailed in this section must be executed right before adding a
   release tag to git.
2. Publishing the project to NuGet must be done by a member of the Piwik team,
   holder of the private NuGet Key.

### Steps

1. Validate tests (requires #7)
2. Update and commit `AssemblyInfo.cs` with new version
3. Create git tag
4. Build the project using the Release solution configuration
5. Create NuGet package using `nuget_pack.bat`
6. Publish the package using `nuget_push.bat`

# [License](LICENSE)
