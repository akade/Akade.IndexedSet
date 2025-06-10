using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Akade.IndexedSet.Tests.Data;

/// <summary>
/// "Amtliches Ortschaftenverzeichnis mit Postleitzahl und Perimeter"
/// https://www.swisstopo.admin.ch/en/official-directory-of-towns-and-cities
/// </summary>
internal static class SwissZipCodes
{
    public static async Task<IEnumerable<SwissZipCode>> LoadAsync()
    {
        await using FileStream stream = File.OpenRead("../../../Data/AMTOVZ_CSV_WGS84.csv");
        using StreamReader reader = new(stream);
        List<SwissZipCode> zipCodes = [];

        // Skip header line
        _ = await reader.ReadLineAsync();
        string? line;

        while ((line = await reader.ReadLineAsync()) is not null)
        {
            string[] parts = line.Split(';');

            double easting = double.Parse(parts[6], CultureInfo.InvariantCulture);
            double northing = double.Parse(parts[7], CultureInfo.InvariantCulture);

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
internal sealed record class SwissZipCode
{
    private Coordinates _coordinates;

    public required string Name { get; init; }
    public required string ZipCode { get; init; }
    public required string? AdditionalDigit { get; init; }
    public required string MunicipalityName { get; init; }
    public required int MunicipalityNumber { get; init; }
    public required string? CantonCode { get; init; }
    public required Coordinates Coordinates { get => _coordinates; init => _coordinates = value; }
    public required string Language { get; init; }
    public required string Validity { get; init; }

    public override string ToString()
    {
        return $"{Name} ({ZipCode}) - {MunicipalityName} ({CantonCode})";
    }

    public ReadOnlySpan<double> GetCoordinatesSpan()
    {
        return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<Coordinates, double>(ref _coordinates), 2);
    }

}

[StructLayout(LayoutKind.Sequential, Pack = 0)]
internal readonly struct Coordinates
{
    internal readonly double Easting;
    internal readonly double Northing;

    public Coordinates(double easting, double northing) : this()
    {
        Easting = easting;
        Northing = northing;
    }

}
