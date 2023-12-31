﻿using System.Collections;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace NpgsqlBinaryCopyHelper;

public static class CopyHelper
{
    private static ILogger? _logger;

    public static void SetLogger(ILogger? logger)
    {
        _logger = logger;
    }

    public static async Task CopyAsync<T>(this List<T> model, DbContext context,
        bool insertWithPrimaryKeys = false)
        where T : class
    {
        try
        {
            if (model == null || !model.Any())
            {
                throw new ArgumentException("The model list cannot be null or empty.");
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "The DbContext instance cannot be null.");
            }

            var sp = Stopwatch.StartNew();

            var connection = new NpgsqlConnection(context.Database.GetConnectionString());

            _logger?.LogDebug("Connection retrieved successfully");

            var entity = context.Model.FindEntityType(typeof(T))! ??
                         throw new InvalidOperationException("Entity type not found.");

            var tableName = entity.GetTableName();

            if (string.IsNullOrEmpty(tableName))
            {
                throw new InvalidOperationException("Table name is null or empty.");
            }

            _logger?.LogDebug("Database set name {TableName} found successfully", tableName);

            var properties = entity.GetProperties().ToList();

            if (!insertWithPrimaryKeys)
                properties = properties.Where(x => !x.IsKey()).ToList();

            var columnNames = properties.Select(x => x.GetColumnName()).ToList();

            if (!columnNames.Any())
            {
                throw new InvalidOperationException("Column names are null or empty.");
            }

            var columnNamesForSql = string.Join(", ", columnNames);

            var columnCount = columnNames.Count;
            var rowCount = model.Count;

            _logger?.LogDebug(
                "Column names found successfully. \n Total column count: {ColumnCount} \n Total row count: {RowCount}",
                columnCount, rowCount);

            var sql = $"COPY {tableName} ({columnNamesForSql}) FROM STDIN (FORMAT BINARY)";

            _logger?.LogInformation("SQL query created successfully. Sql query: {Sql}", sql);

            await connection.OpenAsync();

            _logger?.LogDebug("Connection opened successfully");

            await using var writer = await connection.BeginBinaryImportAsync(sql);

            _logger?.LogDebug("Binary import is starting. This may take a while...");

            var rowProgress = 1;

            var propertyInfos = properties.Select(x => x.PropertyInfo).ToList();
            var propertyTypes = propertyInfos.Select(x => x!.PropertyType).ToList();


            foreach (var item in model)
            {
                var spInLoop = Stopwatch.StartNew();

                var values = propertyInfos.Select(property => property!.GetValue(item)).ToList();

                // Converting enum to int if needed.
                for (var i = 0; i < columnCount; i++)
                {
                    if (propertyTypes[i].IsEnum)
                    {
                        var enumMapping = properties[i].FindTypeMapping();

                        if (enumMapping is IntTypeMapping)
                        {
                            values[i] = (int)values[i]!;
                        }
                    }

                    if (!propertyTypes[i].IsGenericType ||
                        propertyTypes[i].GetGenericTypeDefinition() != typeof(List<>)) continue;
                    {
                        var genericType = propertyTypes[i].GetGenericArguments()[0];

                        if (!genericType.IsEnum) continue;
                        var enumMapping = properties[i].FindTypeMapping();

                        if (enumMapping is not NpgsqlArrayTypeMapping) continue;
                        var list = (IList)values[i]!;

                        var listInt = (from object? item1 in list select (int)item1!).ToList();

                        values[i] = listInt;
                    }
                }

                _logger?.LogDebug("Values for the row {Row} are retrieved successfully", rowProgress);

                await writer.StartRowAsync();

                for (var i = 0; i < columnCount; i++)
                {
                    await writer.WriteAsync(values[i]);
                }

                _logger?.LogDebug("Row number {RowProgress} written successfully in {Milliseconds} ms", rowProgress,
                    spInLoop.ElapsedMilliseconds);
                rowProgress++;
            }

            await writer.CompleteAsync();
            await connection.CloseAsync();

            _logger?.LogInformation("Binary copy completed successfully. Total time: {Seconds} seconds",
                sp.ElapsedMilliseconds / 1000);
        }
        catch (Exception ex)
        {
            _logger?.LogError("An error occurred while performing binary copy. Error: {Error}", ex);
            throw;
        }
    }
}