using DevToys.Api;
using System.ComponentModel.Composition;

namespace DevToys.JsonToPython;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(JsonToPythonAssemblyIdentifier))]
internal sealed class JsonToPythonAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        throw new NotImplementedException();
    }
}