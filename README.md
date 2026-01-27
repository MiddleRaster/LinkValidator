# LinkValidator

A small command-line utility to follow links in a domain and make sure they're all valid; useful for checking links in your github pages.

Usage:  LinkValidator.exe some-url

Note: LinkValidator uses Microsoft.Playwright nuget package, and using that requires a nuget-supplied powershell script to be run to download Chromium, Firefox and WebKit.
