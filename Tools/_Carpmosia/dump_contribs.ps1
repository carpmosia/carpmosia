#!/usr/bin/env pwsh

$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

if ($null -eq $env:GITHUB_TOKEN)
{
    throw "A GitHub API token is required to run this script properly without being rate limited. If you're a user, generate a personal access token and use that. If you're running this in a GitHub action, make sure you expose the GITHUB_TOKEN secret as an environment variable."
}

$qParams = @{
    "per_page" = 100
    "state" = "closed"
}

$headers = @{
    Authorization="Bearer $env:GITHUB_TOKEN"
}

$url = "https://api.github.com/repos/carpmosia/carpmosia/pulls" -f $repo

$prs = @()

while ($null -ne $url)
{
    $resp = Invoke-WebRequest $url -Body $qParams -Headers $headers

    $url = $resp.RelationLink.next

    $j = ConvertFrom-Json $resp.Content | Where-Object {$null -ne $_.merged_at}
    $prs += $j
}

$ignore = @{
    "github-actions[bot]" = $true
}

$prs | % { $_.user.login } | select -unique | Where-Object { -not $ignore[$_] }` | Sort-object | Join-String -Separator ", "
