using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ecoa.Core.Entities;
using Ecoa.Core.Ports;

namespace Ecoa.Controllers;

[ApiController]
[Route("api/cycle-paths")]
[Authorize(Roles = "Admin")]
public class CyclePathController : ControllerBase
{
    private readonly ICyclePathRepository _cyclePathRepository;
    private readonly IHttpClientFactory _httpClientFactory;

    public CyclePathController(ICyclePathRepository cyclePathRepository, IHttpClientFactory httpClientFactory)
    {
        _cyclePathRepository = cyclePathRepository;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportFromOsm()
    {
        var overpassUrl = "https://overpass-api.de/api/interpreter";

        var query = """
            [out:json][timeout:60];
            area["name"="Santos"]["admin_level"="8"]->.santos;
            area["name"="São Vicente"]["admin_level"="8"]->.sv;
            area["name"="Guarujá"]["admin_level"="8"]->.guaruja;
            area["name"="Cubatão"]["admin_level"="8"]->.cubatao;
            area["name"="Praia Grande"]["admin_level"="8"]->.pg;
            (
              way["highway"="cycleway"](area.santos)(area.sv)(area.guaruja)(area.cubatao)(area.pg);
              way["cycleway"](area.santos)(area.sv)(area.guaruja)(area.cubatao)(area.pg);
              way["bicycle"="designated"](area.santos)(area.sv)(area.guaruja)(area.cubatao)(area.pg);
            );
            out geom;
            """;

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{overpassUrl}?data={Uri.EscapeDataString(query)}");

            if (!response.IsSuccessStatusCode)
                return BadRequest(new { error = "Falha ao consultar Overpass API." });

            var json = await response.Content.ReadAsStringAsync();
            var osmData = JsonSerializer.Deserialize<OsmResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (osmData?.elements == null || osmData.elements.Count == 0)
                return BadRequest(new { error = "Nenhuma ciclovia encontrada na região." });

            await _cyclePathRepository.ClearAllAsync();

            var cyclePaths = new List<CyclePath>();
            foreach (var element in osmData.elements)
            {
                if (element.geometry == null || element.geometry.Count < 2)
                    continue;

                var lats = element.geometry.Select(g => g.lat).ToList();
                var lons = element.geometry.Select(g => g.lon).ToList();

                var geoJson = JsonSerializer.Serialize(new
                {
                    type = "LineString",
                    coordinates = element.geometry.Select(g => new[] { g.lon, g.lat }).ToList()
                });

                var name = element.tags?.TryGetValue("name", out var n) == true ? n : null;

                cyclePaths.Add(new CyclePath
                {
                    Name = name,
                    GeoJson = geoJson,
                    MinLatitude = lats.Min(),
                    MaxLatitude = lats.Max(),
                    MinLongitude = lons.Min(),
                    MaxLongitude = lons.Max()
                });
            }

            await _cyclePathRepository.AddRangeAsync(cyclePaths);

            return Ok(new { imported = cyclePaths.Count, message = $"{cyclePaths.Count} ciclovias importadas com sucesso." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"Erro ao importar: {ex.Message}" });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var paths = await _cyclePathRepository.GetAllAsync();
        return Ok(paths);
    }

    private class OsmResponse
    {
        public List<OsmElement> elements { get; set; } = new();
    }

    private class OsmElement
    {
        public string? type { get; set; }
        public Dictionary<string, string>? tags { get; set; }
        public List<OsmGeometry>? geometry { get; set; }
    }

    private class OsmGeometry
    {
        public double lat { get; set; }
        public double lon { get; set; }
    }
}
