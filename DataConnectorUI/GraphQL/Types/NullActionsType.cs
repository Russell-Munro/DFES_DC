using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GraphQL.Types;
using UDC.Common;

namespace DataConnectorUI.GraphQL.Types
{
    public class NullActionsType:EnumerationGraphType<Constants.NullActions>
    {
        public NullActionsType()
        {
            Name = "NullActions";
            
        }
    }
}
