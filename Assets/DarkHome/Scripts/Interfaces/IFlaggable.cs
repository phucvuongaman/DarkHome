using System.Collections.Generic;


namespace DarkHome
{
    public interface IFlaggable
    {
        List<FlagData> RequiredFlags { get; }
        List<FlagData> GrantedFlags { get; }
    }
}