namespace RoseCakes.Api.Models;
public static class ImagemProduto
{
    public static bool Valida(string? foto)
    {
        if (string.IsNullOrEmpty(foto)) return true;
        if (foto.Length > 1100000) return false;
        var partes = foto.Split(',', 2);
        if (partes.Length != 2) return false;
        try
        {
            var bytes = Convert.FromBase64String(partes[1]);
            if (bytes.Length > 750000 || bytes.Length < 12) return false;
            return partes[0] switch
            {
                "data:image/png;base64" => bytes.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}),
                "data:image/jpeg;base64" => bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255,
                "data:image/webp;base64" => System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP",
                _ => false
            };
        }
        catch (FormatException) { return false; }
    }
}
