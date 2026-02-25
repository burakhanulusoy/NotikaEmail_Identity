using Microsoft.CodeAnalysis.CSharp.Syntax;
using NotikaEmail_Identity.Entities.Common;

namespace NotikaEmail_Identity.Entities
{
    public class Category:BaseEntity
    {
        public string Name { get; set; }
        public string IconUrl { get; set; } 
        public bool Status { get; set; }

        public IList<Message> Messages { get; set; }

    }
}
