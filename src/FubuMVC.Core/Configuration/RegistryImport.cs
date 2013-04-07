using FubuCore;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Diagnostics;

namespace FubuMVC.Core.Configuration
{
    public class RegistryImport
    {
        public string Prefix { get; set; }
        public FubuRegistry Registry { get; set; }
        public ProvenanceChain Provenance { get; set; }

        public void ImportInto(BehaviorGraph parent, ConfigLog log)
        {
            var childGraph = BehaviorGraphBuilder.Import(Registry, parent, log);
            parent.As<IChainImporter>().Import(childGraph, b => {
                b.PrependToUrl(Prefix);
                b.Origin = Registry.Name;
            });

            log.Import(Registry.Config, Provenance);
        }

        public bool Equals(RegistryImport other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Prefix, Prefix) && Equals(other.Registry.GetType(), Registry.GetType());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(RegistryImport)) return false;
            return Equals((RegistryImport)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Prefix != null ? Prefix.GetHashCode() : 0) * 397) ^ (Registry.GetType() != null ? Registry.GetType().GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("Registry: {0}", Registry);
        }
    }
}