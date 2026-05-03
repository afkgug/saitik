using System.Text.Json;
using guitar_shop.Models;

namespace guitar_shop.Services
{
    public static class GuitarService
    {
        public static List<Guitar> LoadGuitars(IWebHostEnvironment env)
        {
            var path = Path.Combine(env.ContentRootPath, "data", "guitars.json");
            if (!File.Exists(path)) return new List<Guitar>();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Guitar>>(json);
        }

        public static List<Guitar> GetAll(IWebHostEnvironment env) => LoadGuitars(env);
        public static Guitar GetById(int id, IWebHostEnvironment env) => LoadGuitars(env).FirstOrDefault(g => g.Id == id);
    }
}