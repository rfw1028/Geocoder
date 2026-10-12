# AddressGeocoder (Census Tract Validator)

Windows Forms tool that reads address rows from SQL Server, batch-geocodes them with the
US Census geocoder, and writes the results (including census tract) back to the table.

## Configuration

No credentials are stored in the repository. The connection string box is pre-filled from
configuration, or you can type it in the app.

Set `GEOCODER_CONNECTION` as an environment variable, or copy `appsettings.local.example.json`
to `appsettings.local.json` next to the executable and fill in your values. That file is git-ignored.

Use a least-privilege SQL login (not `sa`).
