using System.Collections.Generic;
using FubuMVC.Core.Runtime.Conditionals;

namespace FubuMVC.Core.Resources.Conneg
{
    public interface IMedia<in T>
    {
        IEnumerable<string> Mimetypes { get; }
        IConditional Condition { get; }
        void Write(string mimeType, T resource);
        bool MatchesRequest();
    }
}