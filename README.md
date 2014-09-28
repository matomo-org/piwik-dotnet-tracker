# Piwik C# Tracking API

This is the official C# implementation of the [Piwik Tracking API](http://piwik.org/docs/tracking-api/).

Check the [release page](releases)
to know which versions of Piwik are supported.

## Usage

Three Visual Studio Solutions are provided : 

* [Piwik.Tracker.sln](Piwik.Tracker.sln) : Library project
* [Piwik.Tracker.Samples.sln](Piwik.Tracker.Samples.sln) : Console Samples project
* [Piwik.Tracker.Web.Samples.sln](Piwik.Tracker.Web.Samples.sln) : ASP.NET Samples project

The two Samples projects provide exhaustive examples.

## How to contribute

The Piwik C# Tracking API is a translation of the [official PHP Tracking API](../piwik/tree/master/libs/PiwikTracker) in C#.

Translating the project is currently a manual process.

Manually translating the project is tedious because it requires

* identifying features that have already been translated
* merging code

We would ideally like to automate this process, we welcome contributions aimed towards this goal.

To ease the process in the mean time, the following rules are applied to any new code contributions :

* one-to-one commit and tag mappings between the PHP and C# projects
* the C# code should mirror as close as possible the PHP code

To apply the second rule, we copy-paste the PHP code in the C# class and alter it so it compiles. This can lead in a loss of C# best practices. We consider the time savings an acceptable trade-off.

As long as we do not have an automated process, we welcome suggestions in improving the manual process.


# [License](LICENSE)