using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Akade.IndexedSet.SampleData;

/// <summary>
/// "Amtliches Ortschaftenverzeichnis mit Postleitzahl und Perimeter"
/// https://www.swisstopo.admin.ch/en/official-directory-of-towns-and-cities
/// </summary>
public static class SwissZipCodes
{
    private const string _pathFromTests = "../../../../Akade.IndexedSet.SampleData/AMTOVZ_CSV_WGS84.csv";

    public static async Task<IEnumerable<SwissZipCode>> LoadAsync(string? prefix = null)
    {
        string path = _pathFromTests;

        if (!string.IsNullOrEmpty(prefix))
        {
            path = Path.Combine(prefix, _pathFromTests);
            path = @"C:\Source\Repos\akade\Akade.IndexedSet\Akade.IndexedSet.SampleData\AMTOVZ_CSV_WGS84.csv";
        }

        await using FileStream stream = File.OpenRead(path);
        using StreamReader reader = new(stream);
        List<SwissZipCode> zipCodes = [];

        // Skip header line
        _ = await reader.ReadLineAsync();
        string? line;

        while ((line = await reader.ReadLineAsync()) is not null)
        {
            string[] parts = line.Split(';');

            float easting = float.Parse(parts[6], CultureInfo.InvariantCulture);
            float northing = float.Parse(parts[7], CultureInfo.InvariantCulture);

            zipCodes.Add(new SwissZipCode()
            {
                Name = parts[0],
                ZipCode = parts[1],
                AdditionalDigit = parts[2],
                MunicipalityName = parts[3],
                MunicipalityNumber = int.Parse(parts[4], CultureInfo.InvariantCulture),
                CantonCode = parts[5],
                Coordinates = new Coordinates(easting, northing),
                Language = parts[8],
                Validity = parts[9]
            });
        }

        return zipCodes;
    }
}

// Ortschaftsname;PLZ;Zusatzziffer;Gemeindename;BFS-Nr;Kantonskürzel;E;N;Sprache;Validity
public sealed record class SwissZipCode
{
    public required string Name { get; init; }
    public required string ZipCode { get; init; }
    public required string? AdditionalDigit { get; init; }
    public required string MunicipalityName { get; init; }
    public required int MunicipalityNumber { get; init; }
    public required string? CantonCode { get; init; }
    public required Coordinates Coordinates { get; init; }

    public required string Language { get; init; }
    public required string Validity { get; init; }
    
    public CoordinateRectangle CoordinateRectangle => new(Coordinates, Coordinates);

    public override string ToString()
    {
        return $"{Name} ({ZipCode}) - {MunicipalityName} ({CantonCode}) - {Coordinates}";
    }
}

public readonly struct Coordinates
{
    public readonly float Easting { get; }
    public readonly float Northing { get; }

    public Coordinates(float easting, float northing) : this()
    {
        Easting = easting;
        Northing = northing;
    }

    public override string ToString()
    {
        return $"{Easting}, {Northing}";
    }

    public static implicit operator Vector2(Coordinates coordinates)
    {
        return new Vector2(coordinates.Easting, coordinates.Northing);
    }
}

public readonly struct CoordinateRectangle
{
    public readonly Coordinates Min { get; }

    public readonly Coordinates Max { get; }

    public CoordinateRectangle(Coordinates min, Coordinates max) : this()
    {
        Min = min;
        Max = max;
    }

    public override string ToString()
    {
        return $"Min: {Min}, Max: {Max}";
    }

    public float DistanceTo(Vector2 point)
    {
        Vector2 min = Min;
        Vector2 max = Max;
        
        var closestPoint = Vector2.Clamp(point, min, max);
        return Vector2.Distance(closestPoint, point);
    }
}
