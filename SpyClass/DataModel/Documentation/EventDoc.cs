using System;
using System.Text;
using Mono.Cecil;
using SpyClass.DataModel.Documentation.Base;
using SpyClass.DataModel.Documentation.Components;

namespace SpyClass.DataModel.Documentation
{
    public class EventDoc : DocPart
    {
        public TypeDoc Owner { get; }
     
        public string Name { get; private set; }
        public TypeInfo EventTypeInfo { get; private set; }

        public EventModifiers Modifiers { get; private set; }

        public EventDoc(TypeDoc owner, EventDefinition ev) 
        {
            Owner = owner;
            
            AnalyzeEvent(ev);
        }
        
        private void AnalyzeEvent(EventDefinition ev)
        {
            Name = ev.Name;

            EventTypeInfo = new TypeInfo(ev.EventType);
            
            Access = DetermineAccess(ev);
            Modifiers = DetermineModifiers(ev);
        }
        
        public static AccessModifier DetermineAccess(EventDefinition ev)
        {
            if (ev.AddMethod.IsPublic)
            {
                return AccessModifier.Public;
            }
            else if (ev.AddMethod.IsFamily)
            {
                return AccessModifier.Protected;
            }
            else if (ev.AddMethod.IsAssembly)
            {
                return AccessModifier.Internal;
            }
            else if (ev.AddMethod.IsFamilyOrAssembly)
            {
                return AccessModifier.ProtectedInternal;
            }
            else if (ev.AddMethod.IsPrivate)
            {
                return AccessModifier.Private;
            }
            else if (ev.AddMethod.IsFamilyAndAssembly)
            {
                return AccessModifier.PrivateProtected;
            }

            throw new NotSupportedException($"Cannot determine type access: {ev.Name}");
        }
        
        public static EventModifiers DetermineModifiers(EventDefinition ev)
        {
            EventModifiers ret = 0;

            if (ev.AddMethod.IsStatic)
            {
                ret |= EventModifiers.Static;
            }

            return ret;
        }

        private string BuildEventModifierString()
        {
            var sb = new StringBuilder();
            
            if (Modifiers.HasFlag(EventModifiers.Static))
            {
                sb.Append(" static");
            }
            
            return sb.ToString();
        }
        
        protected override string BuildStringRepresentation(int indent)
        {
            var sb = new StringBuilder();

            sb.Append(AccessModifierString);
            sb.Append(BuildEventModifierString());
            sb.Append(" event ");
            sb.Append(EventTypeInfo.BuildStringRepresentation());
            sb.Append(" ");
            sb.Append(Name);

            return sb.ToString();
        }

        public override string ToString()
        {
            return BuildStringRepresentation(0);
        }
    }
}