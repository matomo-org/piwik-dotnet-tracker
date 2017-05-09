item({desc: 'piwik-dotnet-tracker'}, function() {
  item({key: 'g', desc: 'generate changelog', cmd: './scripts/generate-changelog.sh'})
  item({key: 'p', desc: 'publish new version', cmd: './scripts/publish.sh'})
})
