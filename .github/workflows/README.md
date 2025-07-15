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
6. **Publish test results** - Reports test results back to the PR with detailed summary

### Branch Protection
This workflow creates status checks that can be used for branch protection rules:
- `build-and-test` - Must pass for PRs to be mergeable

### Test Results
Test results are published as check runs in the PR, showing:
- Number of tests passed/failed/skipped
- Test execution time
- Detailed failure information if any tests fail

The workflow fails if any tests fail, preventing the PR from being merged.