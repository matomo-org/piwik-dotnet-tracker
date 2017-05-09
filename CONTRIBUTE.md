# Publishing the project to NuGet

## Requirements

* Must be executed before adding a release tag to git
* Must be done by a member of the Piwik team, holder of the private NuGet Key
* [WSL](https://msdn.microsoft.com/en-us/commandline/wsl/about)
* [.NET Core command-line (CLI) tools](https://github.com/dotnet/cli)
* [cbwin](https://github.com/xilun/cbwin)
* [nuget](https://dist.nuget.org/index.html)

## How-to

1. [Validate tests](https://travis-ci.org/piwik/piwik-dotnet-tracker)
1. `./publish.sh`
1. `./generate-changelog.sh`
1. close github milestone
