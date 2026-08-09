using MudBlazor.Utilities;

// Test WithAlpha approach
var hex = "#67584b";
var alpha = 0.28;

var c = new MudColor(hex);
Console.WriteLine($"Original color: R={c.R}, G={c.G}, B={c.B}, A={c.A}");

var alphaByte = (byte)(alpha * 255);
Console.WriteLine($"Alpha byte: {alphaByte}");

var withAlpha = new MudColor(c.R, c.G, c.B, alphaByte);
var hexA = withAlpha.ToString(MudColorOutputFormats.HexA);
Console.WriteLine($"HexA output: '{hexA}'");

// Try parsing the HexA output back
try {
    var parsed = new MudColor(hexA);
    Console.WriteLine($"Parsed back: R={parsed.R}, G={parsed.G}, B={parsed.B}, A={parsed.A} - OK");
} catch (Exception ex) {
    Console.WriteLine($"Parse FAILED: {ex.Message}");
}

// Test all WithAlpha values used in Build()
double[] alphas = [0.28, 0.35, 0.20, 0.22, 0.12, 0.30, 0.16, 0.18, 0.10];
foreach (var a in alphas) {
    var ab = (byte)(a * 255);
    var mc = new MudColor(c.R, c.G, c.B, ab);
    var s = mc.ToString(MudColorOutputFormats.HexA);
    try {
        var p = new MudColor(s);
        Console.WriteLine($"alpha={a} -> byte={ab} -> '{s}' -> parsed OK");
    } catch (Exception ex) {
        Console.WriteLine($"alpha={a} -> byte={ab} -> '{s}' -> FAILED: {ex.Message}");
    }
}

// Test Hex output as well
var hexOnly = withAlpha.ToString(MudColorOutputFormats.Hex);
Console.WriteLine($"\nHex output (no alpha): '{hexOnly}'");
try {
    var p = new MudColor(hexOnly);
    Console.WriteLine($"Parsed Hex: R={p.R}, G={p.G}, B={p.B}, A={p.A} - OK");
} catch (Exception ex) {
    Console.WriteLine($"Parse Hex FAILED: {ex.Message}");
}
