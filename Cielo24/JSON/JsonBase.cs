using Newtonsoft.Json;

namespace Cielo24.JSON
{
    public abstract class JsonBase
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
