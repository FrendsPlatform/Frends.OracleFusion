# Frends.OracleFusion.ImportBulkData

Frends task that uploads FBDI files as a ZIP archive to Oracle Fusion UCM and returns a DocumentId for triggering the import job.

[![ImportBulkData_build](https://github.com/FrendsPlatform/Frends.OracleFusion/actions/workflows/ImportBulkData_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.OracleFusion/actions/workflows/ImportBulkData_test_on_main.yml)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.OracleFusion/Frends.OracleFusion.ImportBulkData|main)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

## Installing

You can install the Task via Frends UI Task View.

## Building

### Clone a copy of the repository

`git clone https://github.com/FrendsPlatform/Frends.OracleFusion.git`

### Build the project

`dotnet build`

### Run tests

Run the tests

`dotnet test`

### Create a NuGet package

`dotnet pack --configuration Release`

### StyleCop.Analyzers Version
This project uses StyleCop.Analyzers 1.2.0-beta.556, as recommended by the author, to get the latest fixes and improvements not available in the last stable release.
