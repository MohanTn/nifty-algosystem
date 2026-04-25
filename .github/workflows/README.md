# GitHub Actions CI/CD Pipeline

## Overview

This directory contains the GitHub Actions CI/CD pipeline that automatically validates all pull requests to ensure code quality and prevent regressions.

**Pipeline File:** `ci.yml`

---

## Workflow: CI - Build & Test

### Trigger

The pipeline automatically runs when a pull request is created or updated against the `main` or `develop` branches.

```yaml
on:
  pull_request:
    branches: [ main, develop ]
```

### Workflow Steps

The CI pipeline executes the following steps in order:

#### 1. **Checkout Code** 
- Retrieves the PR code
- Action: `actions/checkout@v4`

#### 2. **Setup .NET 8.0 SDK**
- Installs .NET 8.0 runtime and tools
- Action: `actions/setup-dotnet@v4`

#### 3. **Cache NuGet Packages**
- Caches downloaded NuGet packages for faster subsequent builds
- Uses cache key based on `global.json` and `*.csproj` files
- Action: `actions/cache@v4`
- **Performance Impact:** ~2-3 min faster on cache hit

#### 4. **Restore NuGet Packages**
- Downloads project dependencies
- Command: `dotnet restore`

#### 5. **Build Solution (Release)**
- Compiles code in Release configuration for production-like validation
- Command: `dotnet build --configuration Release --no-restore`
- **Failure Effect:** Blocks PR if build fails

#### 6. **Run Tests with Coverage**
- Executes all xUnit unit tests
- Collects code coverage in Cobertura XPlat format
- Generates TRX test result files
- Command: `dotnet test --configuration Release --no-build --logger:trx --collect:"XPlat Code Coverage"`
- **Failure Effect:** Blocks PR if tests fail

#### 7. **Generate Coverage Report**
- Converts Cobertura coverage data to human-readable HTML
- Uses ReportGenerator tool
- Output: `coverage-report/` directory with HTML files

#### 8. **Publish Test Results**
- Makes test results visible in PR with detailed failure information
- Action: `EnricoMi/publish-unit-test-result-action@v2`

#### 9. **Post Coverage Summary to PR**
- Automatically comments on the PR with coverage metrics
- Shows line and branch coverage percentages
- Action: `irongut/code-coverage-action@v1`
- **Minimum Threshold:** 50% (configurable)

#### 10. **Upload Test Results Artifacts**
- Stores TRX test result files for download
- Retention: 30 days
- Action: `actions/upload-artifact@v4`

#### 11. **Upload Coverage Reports**
- Stores Cobertura XML and HTML coverage reports for download
- Retention: 30 days
- Action: `actions/upload-artifact@v4`

#### 12. **Verify Build Status**
- Final check to ensure build and tests passed
- Fails the workflow if any previous step failed

---

## How to View Results

### In Pull Request

1. **Status Check:** Look for the "CI - Build & Test" check at the bottom of the PR
   - ✅ **Green** = All tests passed, PR can be merged
   - ❌ **Red** = Build or tests failed, PR is blocked from merging

2. **Detailed Results:** Click "Details" next to the check to view:
   - Full build and test output
   - Test failures with stack traces
   - Code coverage summary comment posted by CodeCoverageSummary action

3. **Coverage Metrics:** Look for coverage comment in PR discussion
   - Shows overall line and branch coverage percentages
   - Highlights coverage changes vs. base branch

### Download Artifacts

1. Go to the "Checks" tab in the PR
2. Click "CI - Build & Test" → "View workflow file at main"
3. Scroll to "Artifacts" section (available after workflow completes)
4. Download:
   - `test-results` - TRX files for detailed test analysis
   - `coverage-reports` - Cobertura XML and HTML coverage reports

---

## Understanding Coverage Reports

### Cobertura Format (`coverage.cobertura.xml`)
- Machine-readable XML format used for programmatic analysis
- Parsed by CodeCoverageSummary action for PR comments

### HTML Coverage Report (`coverage-report/`)
1. Download artifacts and extract `coverage-reports`
2. Open `index.html` in a web browser
3. Drill down by:
   - **Namespace** - View coverage by package
   - **Class** - See coverage by individual classes
   - **Method** - Examine method-level coverage
4. Green lines = covered code, Red lines = uncovered code

### Coverage Metrics
- **Line Coverage:** Percentage of code lines executed by tests
- **Branch Coverage:** Percentage of code branches (if/else) executed
- **Target:** 80% line coverage for NiftyOptionsAlgo.Core

---

## Troubleshooting

### Build Fails

**Problem:** "Build failed: error CS..."
- **Solution:** Fix the compilation error in your code and push again
- **Common Causes:** 
  - Syntax errors
  - Missing using statements
  - Type mismatches

### Tests Fail

**Problem:** "Test run for ... failed"
- **Solution:** 
  1. Check "Publish test results" step for failure details
  2. Review stack traces
  3. Fix failing tests and push again
- **Common Causes:**
  - Logic errors in code
  - Missing test setup
  - Broken dependencies

### Coverage Drop

**Problem:** Coverage percentage decreased
- **Solution:**
  1. Review coverage report (download artifacts)
  2. Identify uncovered code paths
  3. Add tests for those paths
- **Note:** Coverage is relative to changed files in PR

### Artifacts Not Available

**Problem:** Can't download test results or coverage reports
- **Solution:**
  1. Workflow must complete (success or failure)
  2. Check "Artifacts" section appears in workflow summary
  3. Artifacts auto-delete after 30 days
- **Retention Policy:** All artifacts kept for 30 days

---

## Performance Optimization

### NuGet Cache
- **First run:** ~3-4 minutes (downloads all packages)
- **Subsequent runs:** ~1-2 minutes (uses cache)
- **Cache invalidation:** Auto-updated when `global.json` or `*.csproj` changes

### Build Optimization
- Release configuration: Uses aggressive optimization
- No-restore flag: Skips restore if already done
- Parallel test execution: Tests run in parallel by xUnit

---

## CI/CD Best Practices Used

1. **Build in Release Mode** - Catches Release-specific issues
2. **Code Coverage Tracking** - Prevents coverage regression
3. **Artifact Preservation** - Available for 30 days for analysis
4. **Fast Feedback** - Results available in ~2-4 minutes
5. **PR Integration** - Coverage summary posted automatically
6. **Deterministic Builds** - Consistent across machines

---

## Configuration Files

### Global.json
- Specifies .NET SDK version
- Version: 8.0.x (latest 8.0 patch)

### CodeCoverage.runsettings
- Optional coverage configuration file
- Not currently used (using Cobertura defaults)
- Can be added to customize coverage rules

---

## Future Enhancements

Potential additions to the CI pipeline:

- [ ] Code quality analysis (SonarQube, Roslyn)
- [ ] Dependency vulnerability scanning
- [ ] Performance benchmarking
- [ ] Automated deployment on main branch
- [ ] Security scanning (SAST)
- [ ] Integration tests
- [ ] E2E testing for Dashboard/Worker

---

## Contact & Support

For issues with the CI pipeline:
1. Check this README first
2. Review workflow logs in GitHub Actions
3. Consult troubleshooting section above
4. Open an issue if further help needed

---

**Last Updated:** 2026-04-25
**Workflow Version:** 1.0
