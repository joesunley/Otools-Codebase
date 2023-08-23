using OTools.Common;

namespace OTools.Symbols;

public static class SymbolSetLoader
{

}

public interface ISymbolSetLoaderV1
{
    XMLNode SaveSymbolSet(SymbolSet symbolSet);
    SymbolSet LoadSymbolSet(XMLNode node);
}

public class SymbolSetLoaderV1 : ISymbolSetLoaderV1
{
    public SymbolSet LoadSymbolSet(XMLNode node)
    {
        throw new NotImplementedException();
    }

    public XMLNode SaveSymbolSet(SymbolSet symbolSet)
    {
        throw new NotImplementedException();
    }
}