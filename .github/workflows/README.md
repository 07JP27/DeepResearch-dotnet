# GitHub Actions CI/CD

This directory contains GitHub Actions workflows for the DeepResearch-dotnet project.

## ci.yml

The main CI workflow that runs on pull requests and pushes to main/master branches.

### What it does:
1. **Checkout code** - Downloads the repository code
2. **Setup .NET** - Installs .NET 9.0 SDK (including preview versions)
3. **Restore dependencies** - Runs `dotnet restore` in the app directory
4. **Build** - Compiles the solution in Release configuration
5. **Test** - Runs all tests and generates TRX reports
6. **Publish test results** - Reports test results as status checks
7. **Post test results as PR comment** - Posts detailed test results directly in PR comments

### Branch Protection
This workflow creates status checks that can be used for branch protection rules:
- `build-and-test` - Must pass for PRs to be mergeable

### Test Results
Test results are published in two ways:
1. **Status checks** - Creates check runs in the PR showing pass/fail status
2. **PR comments** - Posts detailed test results directly as comments in PRs

The PR comment includes:
- Number of tests passed/failed/skipped
- Test execution time
- Detailed failure information if any tests fail
- Summary statistics

The workflow fails if any tests fail, preventing the PR from being merged.