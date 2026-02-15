Here's the improved `README.md` file, incorporating the new content while maintaining the existing structure and information:

# RetailPricing

This repository contains two main projects:

- `RetailPricing.Api` — .NET 8 Web API that accepts CSV pricing uploads, records upload history and errors, and exposes endpoints for status and error download.
- `RetailPricing.Ui` — User interface (project folder may vary) for interacting with the API.

## Prerequisites

- .NET 8 SDK
- Visual Studio 2022 (or newer) with .NET 8 support or VS Code
- Git

## Running the API (RetailPricing.Api)

Using Visual Studio:
1. Open the solution in Visual Studio 2022.
2. Restore NuGet packages.
3. Set `RetailPricing.Api` as the startup project (or configure multiple startup projects if running UI and API together).
4. Configure the connection string in `appsettings.json` or user secrets / environment variables if required.
5. Run the project (F5) or without debugging (Ctrl+F5).

Using dotnet CLI:

cd RetailPricing.Api
dotnet restore
dotnet build
dotnet run

Default API base path: `https://localhost:5001` or the URL printed in the console.

### API Endpoints

- **POST `/api/pricing/upload`**
  - Upload a CSV file (form file named `file`).
  - Response: `{ "BatchId": "{guid}" }` on success.
  - Example curl:

    ```bash
    curl -v -F "file=@pricing.csv" https://localhost:5001/api/pricing/upload
    ```

- **GET `/api/pricing/upload/{uploadId}`**
  - Returns upload history for the given `uploadId` (GUID).
  - Example:

    ```bash
    curl https://localhost:5001/api/pricing/upload/00000000-0000-0000-0000-000000000000
    ```

- **GET `/api/pricing/upload/{uploadId}/errors`**
  - Downloads a CSV containing validation errors for the upload. Returns `204 No Content` if no errors.
  - Example:

    ```bash
    curl -OJ https://localhost:5001/api/pricing/upload/00000000-0000-0000-0000-000000000000/errors
    ```

## Running the UI (RetailPricing.Ui)

If the UI is an ASP.NET project:

cd RetailPricing.Ui
dotnet restore
dotnet build
dotnet run

If the UI is a JavaScript/TypeScript project (React, Angular, etc.), follow its README in `RetailPricing.Ui` — typically:

cd RetailPricing.Ui
npm install
npm start

Open the UI in a browser; configure the UI to point to the API base URL (usually in an environment file or settings panel).

## Local Development Tips

- Use HTTPS and trust the developer certificate if prompted.
- Use the database connection string appropriate for local development (SQLite, LocalDB, or SQL Server developer instance).
- Seed or migrate the database if the solution includes EF Core migrations.

## Tests

If present, run unit tests using:

dotnet test

## GitHub: How to check-in your changes

1. Create a branch for your work (feature/fix):

    ```bash
    git checkout -b feat/<short-description>
    ```

2. Stage your changes:

    ```bash
    git add .
    ```

3. Commit with a clear message (follow your team conventions):

    ```bash
    git commit -m "feat: add README with run and GitHub instructions"
    ```

4. Push the branch to origin:

    ```bash
    git push -u origin feat/<short-description>
    ```

5. Open GitHub and create a Pull Request from your branch into the main branch. Include:
   - Summary of changes
   - Testing steps
   - Any migration or configuration notes

6. Request reviewers and wait for CI to pass. Address feedback and rebase or merge as your team prefers.

7. Merge the PR (Squash/Merge or Merge Commit according to your workflow) and delete the branch.

## Commit Message Guidance

- Use conventional commits if your team follows them (e.g., `feat:`, `fix:`, `chore:`).
- Keep messages short and descriptive.

## Troubleshooting

- If the API fails to start, check logs in the console for missing configuration or port conflicts.
- Ensure database migrations are applied and connection strings are valid.
- If you get CORS or 401/403 from the UI, verify the UI is configured to call the correct API URL and that authentication (if any) is configured.

## Contact / Contribution

- Follow `CONTRIBUTING.md` for branch, PR, and code style rules.
- Report issues by opening GitHub Issues with reproduction steps.

This revised `README.md` maintains the original structure while enhancing clarity and coherence, ensuring that users can easily navigate and understand the project.