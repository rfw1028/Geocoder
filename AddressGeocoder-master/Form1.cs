using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Web;

namespace CensusTractValidator
{
    // Data models
    public class AddressRecord
    {
        public object ID { get; set; } = "";
        public string Address1 { get; set; } = "";
        public string Address2 { get; set; } = "";
        public string City { get; set; } = "";
        public string State { get; set; } = "";
        public string PostalCode { get; set; } = "";
    }

    public class GeocodeResult
    {
        public bool Success { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Affgeoid { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FormattedAddress { get; set; }
    }

    // One parsed row returned by the Census batch geocoder, keyed back to the source record.
    public class BatchGeocodeRow
    {
        public string Id { get; set; } = "";
        public bool Success { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Affgeoid { get; set; }
        public string? MatchedAddress { get; set; }
    }

    public class CensusGeocodeResponse
    {
        [JsonProperty("result")]
        public CensusGeocodeResult? Result { get; set; }
    }

    public class CensusGeocodeResult
    {
        [JsonProperty("addressMatches")]
        public CensusAddressMatch[]? AddressMatches { get; set; }
    }

    public class CensusAddressMatch
    {
        [JsonProperty("coordinates")]
        public CensusCoordinates? Coordinates { get; set; }

        [JsonProperty("geographies")]
        public CensusGeographies? Geographies { get; set; }

        [JsonProperty("matchedAddress")]
        public string? MatchedAddress { get; set; }
    }

    public class CensusCoordinates
    {
        [JsonProperty("x")]
        public double X { get; set; } // Longitude

        [JsonProperty("y")]
        public double Y { get; set; } // Latitude
    }

    public class CensusGeographies
    {
        [JsonProperty("Census Tracts")]
        public CensusTract[]? CensusTracts { get; set; }
    }

    public class CensusTract
    {
        [JsonProperty("TRACT")]
        public string? TRACT { get; set; }

        [JsonProperty("STATE")]
        public string? STATE { get; set; }

        [JsonProperty("COUNTY")]
        public string? COUNTY { get; set; }
    }

    public partial class Form1 : Form
    {
        private readonly HttpClient _httpClient;
        private string _connectionString = "";
        private CancellationTokenSource? _cancellationTokenSource;

        public Form1()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CensusTractValidator/1.0");
            // Batch responses for large files can take a while to generate server-side.
            _httpClient.Timeout = TimeSpan.FromMinutes(15);
            LoadDefaultSettings();
        }

        private void LoadDefaultSettings()
        {
            txtConnectionString.Text = AppConfig.Get("GEOCODER_CONNECTION") ?? string.Empty;
            txtTableName.Text = "Paddress";
            txtIdColumn.Text = "ID";
            txtAddress1Column.Text = "Address1";
            txtAddress2Column.Text = "Address2";
            txtCityColumn.Text = "City";
            txtStateColumn.Text = "State";
            txtZipColumn.Text = "PostalCode";
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            try
            {
                _connectionString = txtConnectionString.Text;
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                AppendResult("✓ Database connection successful!");

                var count = GetAddressCount();
                AppendResult($"Found {count} addresses needing geocoding in {txtTableName.Text} table");

                lblRecordCount.Text = $"Records needing geocoding: {count}";
                btnStartGeocoding.Enabled = count > 0;
            }
            catch (Exception ex)
            {
                AppendResult($"✗ Database connection failed: {ex.Message}");
                btnStartGeocoding.Enabled = false;
            }
        }

        private int GetAddressCount()
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var tableName = txtTableName.Text;
            var address1Col = txtAddress1Column.Text;
            var cityCol = txtCityColumn.Text;
            var stateCol = txtStateColumn.Text;
           

            var query = $@"
                SELECT COUNT(*) 
                FROM [{tableName}] 
                WHERE [{address1Col}] IS NOT NULL 
                AND LEN(TRIM([{address1Col}])) > 0
                AND [{cityCol}] IS NOT NULL 
                AND [{stateCol}] IS NOT NULL                
                AND (GeocodedLatitude IS NULL OR GeocodedLongitude IS NULL)";

            using var command = new SqlCommand(query, connection);
            return (int)command.ExecuteScalar();
        }

        private async void btnStartGeocoding_Click(object sender, EventArgs e)
        {
            try
            {
                btnStartGeocoding.Enabled = false;
                btnStopGeocoding.Enabled = true;
                _cancellationTokenSource = new CancellationTokenSource();

                var batchSize = (int)numBatchSize.Value;
                var delayMs = (int)numDelay.Value;

                toolStripStatusLabel1.Text = "Starting geocoding process...";
                AppendResult("=== Starting Bulk Geocoding ===");
                AppendResult("Only processing records where GeocodedLatitude/GeocodedLongitude are NULL");

                await AddGeocodingColumns();
                await ProcessAddressRecords(batchSize, delayMs, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                AppendResult("Geocoding process cancelled by user.");
            }
            catch (Exception ex)
            {
                AppendResult($"Error during geocoding process: {ex.Message}");
            }
            finally
            {
                btnStartGeocoding.Enabled = true;
                btnStopGeocoding.Enabled = false;
                toolStripStatusLabel1.Text = "Ready";
            }
        }

        private void btnStopGeocoding_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            AppendResult("Stopping geocoding process...");
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            richTextBoxResults.Clear();
        }

        private async void btnAddColumns_Click(object sender, EventArgs e)
        {
            try
            {
                btnAddColumns.Enabled = false;
                _connectionString = txtConnectionString.Text;
                toolStripStatusLabel1.Text = "Adding geocoding columns...";
                AppendResult($"Adding geocoding columns to [{txtTableName.Text}] (existing columns are left unchanged)...");

                await AddGeocodingColumns();

                AppendResult("✓ Geocoding columns are in place. You can now Test Connection and Start Geocoding.");
            }
            catch (Exception ex)
            {
                AppendResult($"✗ Failed to add geocoding columns: {ex.Message}");
            }
            finally
            {
                btnAddColumns.Enabled = true;
                toolStripStatusLabel1.Text = "Ready";
            }
        }

        private async Task AddGeocodingColumns()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var tableName = txtTableName.Text;

                var columns = new[]
                {
                    "GeocodedLatitude FLOAT NULL",
                    "GeocodedLongitude FLOAT NULL",
                    "GeocodedAffgeoid NVARCHAR(20) NULL",
                    "GeocodedAddress NVARCHAR(500) NULL",
                    "GeocodedDate DATETIME2 NULL",
                    "GeocodedSuccess BIT NULL"
                };

                foreach (var column in columns)
                {
                    try
                    {
                        var alterQuery = $"ALTER TABLE [{tableName}] ADD {column}";
                        using var cmd = new SqlCommand(alterQuery, connection);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch { /* Column might already exist */ }
                }
            }
            catch (Exception ex)
            {
                AppendResult($"Warning: Could not add geocoding columns: {ex.Message}");
            }
        }

        // Census batch geocoder accepts up to 10,000 rows per request. We chunk smaller so the
        // UI shows steady progress and each request returns comfortably within the HTTP timeout.
        private const int CensusBatchChunkSize = 1000;
        private const string BatchGeocoderUrl =
            "https://geocoding.geo.census.gov/geocoder/geographies/addressbatch";

        private async Task ProcessAddressRecords(int batchSize, int delayMs, CancellationToken cancellationToken)
        {
            var all = await GetAllRecordsNeedingGeocoding();
            if (all.Count == 0)
            {
                AppendResult("No records need geocoding.");
                return;
            }

            AppendResult($"Loaded {all.Count} address(es). Submitting to Census batch geocoder " +
                         $"in chunks of {CensusBatchChunkSize}...");

            var processed = 0;
            var successCount = 0;
            var failureCount = 0;
            var totalChunks = (all.Count + CensusBatchChunkSize - 1) / CensusBatchChunkSize;

            for (var i = 0; i < all.Count; i += CensusBatchChunkSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var chunk = all.GetRange(i, Math.Min(CensusBatchChunkSize, all.Count - i));
                var chunkNo = (i / CensusBatchChunkSize) + 1;
                toolStripStatusLabel1.Text = $"Submitting chunk {chunkNo}/{totalChunks} ({chunk.Count} addresses)...";
                AppendResult($"→ Chunk {chunkNo}/{totalChunks}: submitting {chunk.Count} addresses...");

                try
                {
                    var csv = await GeocodeBatchAsync(chunk, cancellationToken);
                    var rows = ParseBatchResults(csv);
                    await BulkUpdateResults(rows, cancellationToken);

                    var chunkSuccess = rows.Count(r => r.Success);
                    successCount += chunkSuccess;
                    failureCount += rows.Count - chunkSuccess;
                    processed += chunk.Count;

                    AppendResult($"✓ Chunk {chunkNo}/{totalChunks} done: " +
                                 $"{chunkSuccess} matched, {chunk.Count - chunkSuccess} unmatched.");
                    toolStripStatusLabel1.Text =
                        $"Processed: {processed}/{all.Count} - Matched: {successCount}, Unmatched: {failureCount}";
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    AppendResult($"✗ Chunk {chunkNo}/{totalChunks} failed: {ex.Message}");
                    failureCount += chunk.Count;
                    processed += chunk.Count;
                }

                Application.DoEvents();
            }

            AppendResult("=== Geocoding Complete ===");
            AppendResult($"Processed: {processed}, Matched: {successCount}, Unmatched: {failureCount}");
        }

        private async Task<List<AddressRecord>> GetAllRecordsNeedingGeocoding()
        {
            var records = new List<AddressRecord>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var tableName = txtTableName.Text;
            var idCol = txtIdColumn.Text;
            var address1Col = txtAddress1Column.Text;
            var address2Col = txtAddress2Column.Text;
            var cityCol = txtCityColumn.Text;
            var stateCol = txtStateColumn.Text;
            var zipCol = txtZipColumn.Text;

            var query = $@"
                SELECT [{idCol}], [{address1Col}], [{address2Col}], [{cityCol}], [{stateCol}], [{zipCol}]
                FROM [{tableName}]
                WHERE [{address1Col}] IS NOT NULL
                AND LEN(TRIM([{address1Col}])) > 0
                AND [{cityCol}] IS NOT NULL
                AND [{stateCol}] IS NOT NULL
                AND GeocodedSuccess IS NULL
                AND (GeocodedLatitude IS NULL OR GeocodedLongitude IS NULL)
                ORDER BY [{idCol}]";

            using var command = new SqlCommand(query, connection);
            command.CommandTimeout = 300;
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                records.Add(new AddressRecord
                {
                    ID = reader[idCol],
                    Address1 = reader.IsDBNull(address1Col) ? "" : reader[address1Col].ToString()?.Trim() ?? "",
                    Address2 = reader.IsDBNull(address2Col) ? "" : reader[address2Col].ToString()?.Trim() ?? "",
                    City = reader.IsDBNull(cityCol) ? "" : reader[cityCol].ToString()?.Trim() ?? "",
                    State = reader.IsDBNull(stateCol) ? "" : reader[stateCol].ToString()?.Trim() ?? "",
                    PostalCode = reader.IsDBNull(zipCol) ? "" : reader[zipCol].ToString()?.Trim() ?? ""
                });
            }

            return records;
        }

        // POST one chunk of addresses to the Census batch geocoder and return the CSV response body.
        private async Task<string> GeocodeBatchAsync(List<AddressRecord> records, CancellationToken cancellationToken)
        {
            var csv = new StringBuilder();
            foreach (var r in records)
            {
                var street = !string.IsNullOrWhiteSpace(r.Address2)
                    ? $"{r.Address1} {r.Address2}"
                    : r.Address1;

                // Column order required by Census: Unique ID, Street, City, State, ZIP
                csv.Append(CsvField(r.ID?.ToString() ?? "")).Append(',');
                csv.Append(CsvField(street)).Append(',');
                csv.Append(CsvField(r.City)).Append(',');
                csv.Append(CsvField(r.State)).Append(',');
                csv.Append(CsvField(r.PostalCode)).Append('\n');
            }

            using var form = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csv.ToString()));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            form.Add(fileContent, "addressFile", "addresses.csv");
            form.Add(new StringContent("Public_AR_Current"), "benchmark");
            form.Add(new StringContent("Census2020_Current"), "vintage");

            using var response = await _httpClient.PostAsync(BatchGeocoderUrl, form, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        // Parse the Census batch response CSV. Columns (no header):
        // 0 ID, 1 InputAddress, 2 MatchIndicator, 3 MatchType, 4 MatchedAddress,
        // 5 "Lon,Lat", 6 TigerLineId, 7 Side, 8 StateFips, 9 CountyFips, 10 Tract, 11 Block
        private static List<BatchGeocodeRow> ParseBatchResults(string csv)
        {
            var rows = new List<BatchGeocodeRow>();
            var lines = csv.Split('\n');

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var f = ParseCsvLine(line);
                if (f.Length < 3) continue;

                var row = new BatchGeocodeRow
                {
                    Id = f[0],
                    Success = string.Equals(f[2], "Match", StringComparison.OrdinalIgnoreCase)
                };

                if (row.Success && f.Length >= 6)
                {
                    row.MatchedAddress = f[4];

                    // Coordinates arrive as a single "lon,lat" field.
                    var coord = f[5].Split(',');
                    if (coord.Length == 2 &&
                        double.TryParse(coord[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var lon) &&
                        double.TryParse(coord[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat))
                    {
                        row.Longitude = lon;
                        row.Latitude = lat;
                    }

                    if (f.Length >= 11 &&
                        !string.IsNullOrEmpty(f[8]) && !string.IsNullOrEmpty(f[9]) && !string.IsNullOrEmpty(f[10]))
                    {
                        row.Affgeoid = $"1400000US{f[8]}{f[9]}{f[10]}";
                    }
                }

                rows.Add(row);
            }

            return rows;
        }

        // Push a chunk's parsed results back to the DB over a single open connection + transaction,
        // reusing one prepared UPDATE command (no per-row connection churn).
        private async Task BulkUpdateResults(List<BatchGeocodeRow> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0) return;

            var tableName = txtTableName.Text;
            var idCol = txtIdColumn.Text;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            using var tx = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

            var updateQuery = $@"
                UPDATE [{tableName}]
                SET GeocodedLatitude = @Latitude,
                    GeocodedLongitude = @Longitude,
                    GeocodedAffgeoid = @Affgeoid,
                    GeocodedAddress = @Address,
                    GeocodedDate = @Date,
                    GeocodedSuccess = @Success
                WHERE [{idCol}] = @ID
                AND GeocodedSuccess IS NULL";

            using var command = new SqlCommand(updateQuery, connection, tx);
            var pLat = command.Parameters.Add("@Latitude", SqlDbType.Float);
            var pLon = command.Parameters.Add("@Longitude", SqlDbType.Float);
            var pAff = command.Parameters.Add("@Affgeoid", SqlDbType.NVarChar, 20);
            var pAddr = command.Parameters.Add("@Address", SqlDbType.NVarChar, 500);
            var pDate = command.Parameters.Add("@Date", SqlDbType.DateTime2);
            var pSucc = command.Parameters.Add("@Success", SqlDbType.Bit);
            var pId = command.Parameters.Add("@ID", SqlDbType.NVarChar, 100);

            foreach (var row in rows)
            {
                pLat.Value = (object?)row.Latitude ?? DBNull.Value;
                pLon.Value = (object?)row.Longitude ?? DBNull.Value;
                pAff.Value = (object?)row.Affgeoid ?? DBNull.Value;
                pAddr.Value = (object?)row.MatchedAddress ?? DBNull.Value;
                pDate.Value = DateTime.Now;
                pSucc.Value = row.Success;
                pId.Value = row.Id;

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
        }

        // Minimal RFC-4180 helpers: quote a field for output; split a response line honoring quotes.
        private static string CsvField(string value)
        {
            value ??= "";
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var sb = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                        else inQuotes = false;
                    }
                    else sb.Append(c);
                }
                else
                {
                    if (c == '"') inQuotes = true;
                    else if (c == ',') { fields.Add(sb.ToString()); sb.Clear(); }
                    else if (c == '\r') { /* ignore */ }
                    else sb.Append(c);
                }
            }

            fields.Add(sb.ToString());
            return fields.ToArray();
        }

        private void AppendResult(string result)
        {
            if (InvokeRequired)
            {
                Invoke(() => AppendResult(result));
                return;
            }

            if (!string.IsNullOrEmpty(richTextBoxResults.Text))
                richTextBoxResults.AppendText(Environment.NewLine);

            richTextBoxResults.AppendText($"[{DateTime.Now:HH:mm:ss}] {result}");
            richTextBoxResults.ScrollToCaret();
        }

       
    }
}